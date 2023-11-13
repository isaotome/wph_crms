using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.Linq;
using System.Transactions;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Crms.Models;
using System.Xml.Linq;
using System.Text;

//Add 2016/11/30 #3659 車両管理　ファイル出力形式の変更(CSV→EXCEL)
using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;
using Microsoft.VisualBasic;        //2018/06/06 arc yano #3883 タマ表改善 新規作成

namespace Crms.Controllers
{
    /// <summary>
    /// 車両管理
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarStockController : Controller
    {
        
         #region 初期化
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarStockController()
        {
            db = CrmsDataContext.GetDataContext();
        }
        #endregion

        //Add 2015/01/20 arc yano IPO対応(車両在庫)　ログ対応
        private static readonly string FORM_NAME = "車両管理";                      // 画面名
        private static readonly string PROC_NAME_SEARCH = "車両管理検索"; 			// 処理名(車両管理検索)
        //Add 2016/11/30 #3659 車両管理　ファイル出力形式の変更(CSV→EXCEL)
        private static readonly string PROC_NAME_EXCELDOWNLOAD = "Excel出力";       // 処理名(Excel出力)
        //Add 2018/08/28 yano #3922
        private static readonly string PROC_NAME_EXCELUPLOAD = "Excel取込";         // 処理名(Excel取込)
        //Add 2018/08/28 yano #3922
        private static readonly string PROC_NAME_CLOSE = "締め処理"; 		        // 処理名(締め処理)
        //Add 2018/08/28 yano #3922
        private static readonly string PROC_NAME_RELEASE = "締め解除"; 		        // 処理名(締め解除)
        
        /// <summary>
        /// 車両管理検索画面表示
        /// </summary>
        /// <returns>部品検索画面</returns>
        //[AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 車両管理検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両管理検索画面</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 処理の分岐条件の追加
        /// </history>
        //[AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            //Mod 2018/08/28 yano #3922

            ActionResult ret = null;

            switch (form["RequestFlag"])
            {
                case "1":   //車両管理表出力
                    ret = Download(form);
                    break;
                case "2":   //車両管理締め処理
                    ret = CloseCarsStock(form);
                    break;
                case "3":   //車両管理締め解除
                    ret = ReleaseCarsStock(form);
                    break;
                default:    //検索
                    ret = SearchListDisp(form);
                    break;
            }

            return ret;
        }
            
        /// <summary>
        /// 車両管理検索結果表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両管理検索画面</returns>
        /// <history>
        /// Mod 2016/11/30 #3659 車両管理項目追加 検索時にDBへの登録処理を廃止
        ///                      (今後、経理締め時にスナップショットをとる)
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SearchListDisp(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            //int ret = 0;

            // 検索結果リストの取得
            PaginatedList<CarStock> list = new PaginatedList<CarStock>();
            if (criteriaInit == true)
            {
                //何もしない
            }
            else
            {
                //Add 2015/04/09 arc yano 車両管理　対応④ validation処理追加
                CommonValidation(form);

                if (!ModelState.IsValid)
                {
                    SetComponent(form, false);
                    return View("CarStockCriteria", list);
                }

                //データ検索
                list = new PaginatedList<CarStock>(GetSearchResultList(form),  int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            }

            // その他出力項目の設定
            SetComponent(form, true);

            // 検索画面の表示
            return View("CarStockCriteria", list);
        }

        #region 締め処理関連
        /// <summary>
        /// 車両管理締め処理
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両管理検索画面</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CloseCarsStock(FormCollection form)
        {

            PaginatedList<CarStock> list = new PaginatedList<CarStock>();

            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_CLOSE);

            //共通vaildation
            CommonValidation(form);

            if (!ModelState.IsValid)
            {
                SetComponent(form, true);

                return View("CarStockCriteria", list);
            }

            //締め処理専用validation
            CloseMonthControlCarStock rec = CloseValidation(form);

            if (!ModelState.IsValid)
            {
                SetComponent(form, true);
                return View("CarStockCriteria", list);
            }

            //-------------------
            //締め処理
            //-------------------
            if (rec == null)
            {
                rec = new CloseMonthControlCarStock();

                rec.CloseMonth = (form["TargetMonth"] + "/01").Replace("/", "");
                rec.CloseStatus = "002";
                rec.CreateEmployeeCode = ((Employee)(Session["Employee"])).EmployeeCode;
                rec.CreateDate = DateTime.Now;
                rec.LastUpdateEmployeeCode = ((Employee)(Session["Employee"])).EmployeeCode;
                rec.LastUpdateDate = DateTime.Now;
                rec.DelFlag = "0";

                db.CloseMonthControlCarStock.InsertOnSubmit(rec);
            }
            else
            {
                rec.CloseStatus = "002";
                rec.LastUpdateEmployeeCode = ((Employee)(Session["Employee"])).EmployeeCode;
                rec.LastUpdateDate = DateTime.Now;
            }

            db.SubmitChanges();

            //データ検索
            list = new PaginatedList<CarStock>(GetSearchResultList(form), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
               
            // その他出力項目の設定
            SetComponent(form, true);

            // 検索画面の表示
            return View("CarStockCriteria", list);
        }

        /// <summary>
        /// 車両管理締め解除
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両管理検索画面</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ReleaseCarsStock(FormCollection form)
        {

            PaginatedList<CarStock> list = new PaginatedList<CarStock>();

            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_RELEASE);

            //共通vaildation
            CommonValidation(form);

            if (!ModelState.IsValid)
            {
                SetComponent(form, true);

                return View("CarStockCriteria", list);
            }

            //締め処理専用validation
            CloseMonthControlCarStock rec = ReleaseValidation(form);

            if (!ModelState.IsValid)
            {
                list = new PaginatedList<CarStock>(GetSearchResultList(form), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                SetComponent(form, true);

                return View("CarStockCriteria", list);
            }

            //-------------------
            //締め解除
            //-------------------
            if (rec == null)
            {
                rec = new CloseMonthControlCarStock();

                rec.CloseMonth = (form["TargetMonth"] + "/01").Replace("/", "");
                rec.CloseStatus = "001";
                rec.CreateEmployeeCode = ((Employee)(Session["Employee"])).EmployeeCode;
                rec.CreateDate = DateTime.Now;
                rec.LastUpdateEmployeeCode = ((Employee)(Session["Employee"])).EmployeeCode;
                rec.LastUpdateDate = DateTime.Now;
                rec.DelFlag = "0";

                db.CloseMonthControlCarStock.InsertOnSubmit(rec);
            }
            else
            {
                rec.CloseStatus = "001";
                rec.LastUpdateEmployeeCode = ((Employee)(Session["Employee"])).EmployeeCode;
                rec.LastUpdateDate = DateTime.Now;
            }

            db.SubmitChanges();


            // その他出力項目の設定
            SetComponent(form, true);

            // 検索画面の表示
            return View("CarStockCriteria", list);
        }
        #endregion

        #region Excel出力処理
        /// <summary>
        /// Excelファイルのダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2019/01/23 yano #3965 WE版新システム対応（Web.configによる処理の分岐)
        /// 2018/08/28 yano #3922 車両管理表(タマ表) 機能改善②　ストプロの全面改修のためにより、プログラムも修正
        /// Mod 2016/11/30 #3659 車両管理　ファイル出力形式の変更(CSV→EXCEL)
        /// </history>
        private ActionResult Download(FormCollection form)
        {
            //-------------------------------
            //初期処理
            //-------------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCELDOWNLOAD);

            ModelState.Clear();

            PaginatedList<CarStock> list = new PaginatedList<CarStock>();

            //-------------------------------
            //Excel出力処理
            //-------------------------------

            //Add 2019/01/23 yano #3965
            string fileprefix = "";

            fileprefix = (new CodeDao(db).GetCodeNameByKey("026", "001", false) != null ? new CodeDao(db).GetCodeNameByKey("026", "001", false).Name : ""); 

            //ファイル名
            string fileName = fileprefix + "_" + form["TargetMonth"].Replace("/", "") + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";      //Mod 2019/01/23 yano #3965

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイルパス取得
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForCarStock"]) ? "" : ConfigurationManager.AppSettings["TemplateForCarStock"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetComponent(form, false);
                return View("CarStockCriteria", list);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetComponent(form, false);
                return View("CarStockCriteria", list);
            }

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);
        }

        /// <summary>
        /// エクセルデータ作成(テンプレートファイルあり)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="fileName">帳票名</param>
        /// <param name="tfileName">帳票テンプレート</param>
        /// <returns>エクセルデータ</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② ストプロの変更による修正
        /// 2016/11/30 #3659 車両管理　ファイル出力形式の変更(CSV→EXCEL)
        /// </history>
        private byte[] MakeExcelData(FormCollection form, string fileName, string tfileName)
        {
            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                  //設定値
            byte[] excelData = null;                //エクセルデータ
            bool ret = false;
            bool tFileExists = true;                //テンプレートファイルあり／なし(実際にあるかどうか)


            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルあり)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //テンプレートファイルが無かった場合
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "テンプレートファイルが見つかりませんでした。");
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }


            // 設定シート取得
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //設定データを取得(config)
            if (config != null)
            {
                configLine = dExport.GetConfigLine(config, 3);
            }
            else //configシートが無い場合はエラー
            {
                ModelState.AddModelError("", "テンプレートファイルのconfigシートがみつかりません");

                excelData = excelFile.GetAsByteArray();

                //ファイル削除
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //******************************
            //新車シートの設定
            //******************************
            //------------------------
            // ヘッダ行出力
            //------------------------
            DateTime targetdate = DateTime.Parse(form["TargetMonth"] + "/01");

            // 新車シート取得
            ExcelWorksheet newCarSheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            //ヘッダ行に設定
            newCarSheet.Cells[1, 1].Value = targetdate.Year + "年" + targetdate.Month + "月 新車管理表";

            //-----------------------
            //データ取得
            //-----------------------
            //Excel出力の場合は検索条件は無効
            FormCollection newform = new FormCollection();

            newform["TargetMonth"] = form["TargetMonth"];
            newform["DataKind"] = "3";

            //データを取得する
            var carList = GetSearchResultList(newform).ToList();

            //----------------------------
            // データ行出力
            //----------------------------
            //新車検索結果の取得
            List<CarStock> newCarlist = carList.Where(x => x.NewUsedType.Equals("N")).ToList();

            //データ設定の前に行を挿入
            ret = dExport.InsertRow(ref excelFile, configLine, 5, newCarlist.Count, 4);

            //データ設定
            ret = dExport.SetData<CarStock, ExcelCarStockForNewCar>(ref excelFile, newCarlist, configLine);

            //シート名を変更
            newCarSheet.Name = "新車" + targetdate.Month + "月";

            //計算式の設定
            newCarSheet.Cells["H2"].Formula = "setting!$C$8";

            //******************************
            //中古車シートの設定
            //******************************
            //中古車設定シートの取得
            configLine = dExport.GetConfigLine(config, 4);

            //------------------------
            // ヘッダ行出力
            //------------------------
            // 設定シート取得
            ExcelWorksheet oldCarSheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            //ヘッダ行に設定
            oldCarSheet.Cells[1, 1].Value = targetdate.Year + "年" + targetdate.Month + "月 中古車管理表";

            //----------------------------
            // データ行出力
            //----------------------------
            //新車検索結果の取得
            List<CarStock> oldCarlist = carList.Where(x => x.NewUsedType.Equals("U")).ToList();

            //データ設定の前に行を挿入
            ret = dExport.InsertRow(ref excelFile, configLine, 5, oldCarlist.Count, 4);

            //データ設定
            ret = dExport.SetData<CarStock, ExcelCarStockForOldCar>(ref excelFile, oldCarlist, configLine);

            //シート名を変更
            oldCarSheet.Name = "中古車" + targetdate.Month + "月";

            //計算式の設定
            oldCarSheet.Cells["H2"].Formula = "setting!$C$8";

            //******************************
            //settingシートの設定
            //******************************
            //settingシートの設定を取得
            configLine = dExport.GetConfigLine(config, 2);

            ExcelWorksheet setting = excelFile.Workbook.Worksheets[configLine.SheetName];

            //対象年月の設定
            setting.Cells["C6"].Value = string.Format("{0:yyyy/MM/dd}", targetdate);

            //実棚
            setting.Cells["C8"].Value = targetdate.Month + "月実棚";

            //新車シートのデータ数
            //setting.Cells["D3"].Value = newCarlist.Count;
            setting.Cells["D3"].Formula = "MAX(COUNTA(" + newCarSheet.Name + "!$C" + setting.Cells["C3"].Value + ":$C" + (setting.Cells["C3"].GetValue<int>() + newCarlist.Count + 2) + "),COUNTA(" + newCarSheet.Name + "!$D" + setting.Cells["C3"].Value + ":$D" + (setting.Cells["C3"].GetValue<int>() + newCarlist.Count + 2) + "),COUNTA(" + newCarSheet.Name + "!$E" + setting.Cells["C3"].Value + ":$E" + (setting.Cells["C3"].GetValue<int>() + newCarlist.Count + 2) + "),COUNTA(" + newCarSheet.Name + "!$F" + setting.Cells["C3"].Value + ":$F" + (setting.Cells["C3"].GetValue<int>() + newCarlist.Count + 2) + "))";

            //中古車シートのデータ数
            //setting.Cells["D4"].Value = oldCarlist.Count;
            setting.Cells["D4"].Formula = "MAX(COUNTA(" + oldCarSheet.Name + "!$C" + setting.Cells["C4"].Value + ":$C" + (setting.Cells["C4"].GetValue<int>() + oldCarlist.Count + 2) + "),COUNTA(" + oldCarSheet.Name + "!$D" + setting.Cells["C4"].Value + ":$D" + (setting.Cells["C4"].GetValue<int>() + oldCarlist.Count + 2) + "),COUNTA(" + oldCarSheet.Name + "!$E" + setting.Cells["C4"].Value + ":$E" + (setting.Cells["C4"].GetValue<int>() + oldCarlist.Count + 2) + "),COUNTA(" + oldCarSheet.Name + "!$F" + setting.Cells["C4"].Value + ":$F" + (setting.Cells["C4"].GetValue<int>() + oldCarlist.Count + 2) + "))";

            //再計算処理
            excelFile.Workbook.Calculate();

            excelData = excelFile.GetAsByteArray();

            //ワークファイル削除
            try
            {
                excelFile.Stream.Close();
                excelFile.Dispose();
                dExport.DeleteFileStream(fileName);
            }
            catch
            {
                //
            }

            return excelData;
          
            ////----------------------------
            ////初期処理
            ////----------------------------
            //ConfigLine configLine;                  //設定値
            //byte[] excelData = null;                //エクセルデータ
            //bool ret = false;
            //bool tFileExists = true;                //テンプレートファイルあり／なし(実際にあるかどうか)


            ////データ出力クラスのインスタンス化
            //DataExport dExport = new DataExport();

            ////エクセルファイルオープン(テンプレートファイルあり)
            //ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            ////テンプレートファイルが無かった場合
            //if (tFileExists == false)
            //{
            //    ModelState.AddModelError("", "テンプレートファイルが見つかりませんでした。");
            //    try
            //    {
            //        dExport.DeleteFileStream(fileName);
            //    }
            //    catch
            //    {
            //        //
            //    }
            //    return excelData;
            //}


            //// 設定シート取得
            //ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            ////設定データを取得(config)
            //if (config != null)
            //{
            //    configLine = dExport.GetConfigLine(config, 2);
            //}
            //else //configシートが無い場合はエラー
            //{
            //    ModelState.AddModelError("", "テンプレートファイルのconfigシートがみつかりません");

            //    excelData = excelFile.GetAsByteArray();

            //    //ファイル削除
            //    try
            //    {
            //        dExport.DeleteFileStream(fileName);
            //    }
            //    catch
            //    {
            //        //
            //    }
            //    return excelData;
            //}

            ////ワークシートオープン
            //var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

        
            ////検索条件文字列を作成
            //DataTable dtCondtion = MakeConditionRow(form);

            ////データ設定
            //ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            ////----------------------------
            //// データ行出力
            ////----------------------------
            ////出力位置の設定
            //configLine = dExport.GetConfigLine(config, 2);

            ////検索結果の取得
            //List<CarStock> list = GetSearchResultList(form).ToList();

            ////取得した結果をExcel出力用に成形
            //List<ExcelCarStock> elist = new List<ExcelCarStock>();

            //elist = MakeExcelList(list, form);

            ////データ設定
            //ret = dExport.SetData<ExcelCarStock, ExcelCarStock>(ref excelFile, elist, configLine);

            //excelData = excelFile.GetAsByteArray();

            ////ワークファイル削除
            //try
            //{
            //    excelFile.Stream.Close();
            //    excelFile.Dispose();
            //    dExport.DeleteFileStream(fileName);
            //}
            //catch
            //{
            //    //
            //}

            //return excelData;
        }

        //Del 2018/08/28 yano #3922 
        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2016/11/30 #3659 車両管理　ファイル出力形式の変更(CSV→EXCEL)
        /// </history>
        //private DataTable MakeConditionRow(FormCollection form)
        //{
        //    //出力バッファ用コレクション
        //    DataTable dt = new DataTable();
        //    String conditionText = "";

        //    //---------------------
        //    //　列定義
        //    //---------------------
        //    dt.Columns.Add("CondisionText", Type.GetType("System.String"));


        //    //---------------
        //    //データ設定
        //    //---------------
        //    DataRow row = dt.NewRow();
        //    //種別
        //    if (!string.IsNullOrWhiteSpace(form["DataKind"]))
        //    {
        //        string DataKindName = new CodeDao(db).GetCarStockDataType(false, form["DataKind"]).Name;
        //        conditionText += "種別=" + DataKindName;
        //    }
        //    //対象年月
        //    if (!string.IsNullOrWhiteSpace(form["TargetMonth"]))
        //    {
        //        conditionText += "　対象年月=" + form["TargetMonth"];
        //    }
        //    //新中区分
        //    if (!string.IsNullOrWhiteSpace(form["NewUsedType"]))
        //    {
        //        string NewUsedTypeName = new CodeDao(db).GetNewUsedTypeByKey(form["NewUsedType"]).Name;
        //        conditionText += "　新中区分=" + NewUsedTypeName;
        //    }
        //    //仕入先
        //    if (!string.IsNullOrEmpty(form["SupplierCode"]))
        //    {
        //        conditionText += "　仕入先=" + form["SupplierCode"];
        //    }
        //    //管理番号
        //    if (!string.IsNullOrEmpty(form["SalesCarNumber"]))
        //    {
        //        conditionText += "　管理番号=" + form["SalesCarNumber"];
        //    }
        //    //車台番号
        //    if (!string.IsNullOrEmpty(form["Vin"]))
        //    {
        //        conditionText += "　車台番号=" + form["Vin"];
        //    }
          
        //    //作成したテキストをカラムに設定
        //    row["CondisionText"] = conditionText;

        //    dt.Rows.Add(row);

        //    return dt;
        //}

        //Del 2018/08/28 yano #3922 
        /// <summary>
        /// ヘッダ行の作成(Excel出力用)
        /// </summary>
        /// <param name="list">列名リスト</param>
        /// <returns></returns>
        /// <history>
        /// Add 2016/11/30 #3659 車両管理　ファイル出力形式の変更(CSV→EXCEL)
        /// </history>
        //private DataTable MakeHeaderRow(IEnumerable<XElement> list)
        //{
        //    //出力バッファ用コレクション
        //    DataTable dt = new DataTable();

        //    //データテーブルにxmlの値を設定する
        //    int i = 1;
        //    DataRow row = dt.NewRow();
        //    foreach (var header in list)
        //    {
        //        dt.Columns.Add("Column" + i, Type.GetType("System.String"));
        //        row["Column" + i] = header.Value;
        //        i++;
        //    }

        //    dt.Rows.Add(row);

        //    return dt;
        //}

        //Del 2018/08/28 yano #3922 
        /// <summary>
        /// 検索結果をExcel用に成形
        /// </summary>
        /// <param name="list">検索結果</param>
        /// <returns name="elist">検索結果(Exel出力用)</returns>
        /// <history>
        /// 2017/08/10 arc yano #3782_車両仕入_キャンセル機能追加
        ///2016/11/30 #3659 車両管理　ファイル出力形式の変更(CSV→EXCEL)
        /// </history>
        //private List<ExcelCarStock> MakeExcelList(List<CarStock> list, FormCollection form)
        //{
        //    List<ExcelCarStock> elist = new List<ExcelCarStock>();

        //    //他勘定受入、他勘定受入がNULLの場合
        //   elist.AddRange(
        //            list.Where(x => x.SelfRegistration == null && x.ReductionTotal == null).Select(x =>
        //            new ExcelCarStock()
        //            {
        //                NewUsedTypeName = (x.c_NewUsedType != null ? x.c_NewUsedType.Name : x.NewUsedTypeName)                                              //新中区分名
        //                ,
        //                PurchaseDate = (x.PurchaseDate != null ? string.Format("{0:yyyy/MM/dd}", x.PurchaseDate) : "")                                      //仕入日
        //                ,
        //                SalesCarNumber = x.SalesCarNumber                                                                                                   //管理番号
        //                ,
        //                CarName = x.CarName                                                                                                                 //メーカー名
        //                ,
        //                CarGradeName = (x.CarGrade != null ? x.CarGrade.Car != null ? x.CarGrade.Car.CarName : x.CarCarName : x.CarCarName)                 //車種名
        //                ,
        //                Vin = x.Vin                                                                                                                         //車台番号
        //                ,
        //                PurchaseLocationName = (x.PurchaseLocation != null ? x.PurchaseLocation.LocationName : x.PurchaseLocationName)                      //仕入拠点
        //                ,
        //                CarStockLocationName = (x.CarStockLocation != null ? x.CarStockLocation.LocationName : x.CarStockLocationName)                      //在庫拠点
        //                ,
        //                Location = (x.Location != null ? x.Location.LocationName : x.LocationCode)                                                          //当月実棚
        //                ,
        //                CarPurchaseTypeName = (x.c_CarPurchaseType != null ? x.c_CarPurchaseType.Name : x.CarPurchaseTypeName)                              //仕入区分
        //                ,
        //                SupplierName = (x.Supplier != null ? x.Supplier.SupplierName : x.SupplierName)                                                      //仕入先名
        //                ,
        //                BeginningInventory = (x.BeginningInventory != null ? string.Format("{0:N0}", x.BeginningInventory) : "")                            //月初在庫
        //                ,
        //                MonthPurchase = (x.MonthPurchase != null ? string.Format("{0:N0}", x.MonthPurchase) : "")                                           //当月仕入
        //                ,
        //                OtherAccount = (x.OtherAccount != null ? string.Format("{0:N0}", x.OtherAccount) : "")                                              //他勘定受入
        //                ,
        //                RecycleAmount = ( x.RecycleAmount != null ? string.Format("{0:N0}", x.RecycleAmount) : "")                                          //リサイクル料
        //                ,
        //                SalesDate = (x.SalesDate != null ? string.Format("{0:yyyy/MM/dd}", x.SalesDate) : "")                                               //納車日
        //                ,
        //                SlipNumber = x.SlipNumber                                                                                                           //伝票番号
        //                ,
        //                DepartmentName = (x.Department != null ? x.Department.DepartmentName : x.PurchaseDepartmentName)                                    //販売店舗名
        //                ,
        //                SalesTypeName = x.SalesType                                                                                                         //販売区分
        //                ,
        //                CustomerName = (x.Customer != null ? x.Customer.CustomerName : x.CustomerName)                                                      //販売先
        //                ,
        //                SalesPrice = (x.SalesPrice != null ? string.Format("{0:N0}", x.SalesPrice) : "")                                                    //車輛本体価格
        //                ,
        //                DiscountAmount = (x.DiscountAmount != null ? string.Format("{0:N0}", x.DiscountAmount) : "")                                        //値引き
        //                ,
        //                ShopOptionAmount = (x.ShopOptionAmount != null ? string.Format("{0:N0}", x.ShopOptionAmount) : "")                                  //付属品
        //                ,
        //                SalesCostTotalAmount = (x.SalesCostTotalAmount != null ? string.Format("{0:N0}", x.SalesCostTotalAmount) : "")                      //諸費用
        //                ,
        //                SalesTotalAmount = (x.SalesTotalAmount != null ? string.Format("{0:N0}", x.SalesTotalAmount) : "")                                  //売上総合計
        //                ,
        //                SalesCostAmount = (x.SalesCostAmount != null ? string.Format("{0:N0}", x.SalesCostAmount) : "")                                     //売上原価  
        //                ,
        //                SalesProfits = (x.SalesProfits != null ? string.Format("{0:N0}", x.SalesProfits) : "")                                              //粗利
        //                ,
        //                SelfRegistration = (x.SelfRegistration != null ? string.Format("{0:N0}", x.SelfRegistration) : "")                                  //自社登録
        //                ,
        //                ReductionTotal = (x.ReductionTotal != null ? string.Format("{0:N0}", x.ReductionTotal) : "")                                        //他勘定振替
        //                ,
        //                DemoCar = (x.DemoCar != null ? string.Format("{0:N0}", x.DemoCar) : "")                                                             //デモカー
        //                ,
        //                TemporaryCar = (x.TemporaryCar != null ? string.Format("{0:N0}", x.TemporaryCar) : "")                                              //代車
        //                ,
        //                RentalCar = (x.RentalCar != null ? string.Format("{0:N0}", x.RentalCar) : "")                                                       //レンタカー
        //                ,
        //                BusinessCar = ( x.BusinessCar != null ? string.Format("{0:N0}", x.BusinessCar) : "")                                                //業務車
        //                ,
        //                PRCar = (x.PRCar != null ? string.Format("{0:N0}", x.PRCar) : "")                                                                   //PRCar
        //                ,
        //                CancelPurchase = (x.CancelPurchase != null ? string.Format("{0:N0}", x.CancelPurchase) : "")                                        //仕入キャンセル //Add 2017/08/10 arc yano #3782
        //                ,
        //                EndInventory = (x.EndInventory != null ? string.Format("{0:N0}", x.EndInventory) : "")                                              //月末在庫
        //            }
        //        ).ToList()
            
        //    );

        //   //他勘定受入または他勘定受入がNULL以外場合
        //   elist.AddRange(
        //            list.Where(x => x.SelfRegistration != null || x.ReductionTotal != null).Select(x =>
        //            new ExcelCarStock()
        //            {
        //                NewUsedTypeName = (x.c_NewUsedType != null ? x.c_NewUsedType.Name : x.NewUsedTypeName)                                              //新中区分名
        //                ,
        //                PurchaseDate = (x.PurchaseDate != null ? string.Format("{0:yyyy/MM/dd}", x.PurchaseDate) : "")                                      //仕入日
        //                ,
        //                SalesCarNumber = x.SalesCarNumber                                                                                                   //管理番号
        //                ,
        //                CarName = x.CarName                                                                                                                 //メーカー名
        //                ,
        //                CarGradeName = (x.CarGrade != null ? x.CarGrade.Car != null ? x.CarGrade.Car.CarName : x.CarCarName : x.CarCarName)                 //車種名
        //                ,
        //                Vin = x.Vin                                                                                                                         //車台番号
        //                ,
        //                PurchaseLocationName = (x.PurchaseLocation != null ? x.PurchaseLocation.LocationName : x.PurchaseLocationName)                      //仕入拠点
        //                ,
        //                CarStockLocationName = (x.CarStockLocation != null ? x.CarStockLocation.LocationName : x.CarStockLocationName)                      //在庫拠点
        //                ,
        //                Location = (x.Location != null ? x.Location.LocationName : x.LocationCode)                                                          //当月実棚
        //                ,
        //                CarPurchaseTypeName = (x.c_CarPurchaseType != null ? x.c_CarPurchaseType.Name : x.CarPurchaseTypeName)                              //仕入区分
        //                ,
        //                SupplierName = (x.Supplier != null ? x.Supplier.SupplierName : x.SupplierName)                                                      //仕入先名
        //                ,
        //                BeginningInventory = (x.BeginningInventory != null ? string.Format("{0:N0}", x.BeginningInventory) : "")                            //月初在庫
        //                ,
        //                MonthPurchase = (x.MonthPurchase != null ? string.Format("{0:N0}", x.MonthPurchase) : "")                                           //当月仕入
        //                ,
        //                OtherAccount = (x.OtherAccount != null ? string.Format("{0:N0}", x.OtherAccount) : "")                                              //他勘定受入
        //                ,
        //                RecycleAmount = (x.RecycleAmount != null ? string.Format("{0:N0}", x.RecycleAmount) : "")                                          //リサイクル料
        //                ,
        //                SalesDate = ""                                                                                                                      //納車日
        //                ,
        //                SlipNumber = ""                                                                                                                     //伝票番号
        //                ,
        //                DepartmentName = ""                                                                                                                 //販売店舗名
        //                ,
        //                SalesTypeName = ""                                                                                                                  //販売区分
        //                ,
        //                CustomerName = ""                                                                                                                   //販売先
        //                ,
        //                SalesPrice = ""                                                                                                                     //車輛本体価格
        //                ,
        //                DiscountAmount = ""                                                                                                                 //値引き
        //                ,
        //                ShopOptionAmount = ""                                                                                                               //付属品
        //                ,
        //                SalesCostTotalAmount = ""                                                                                                           //諸費用
        //                ,
        //                SalesTotalAmount = ""                                                                                                               //売上総合計
        //                ,
        //                SalesCostAmount = ""                                                                                                                //売上原価  
        //                ,
        //                SalesProfits = ""                                                                                                                   //粗利
        //                ,
        //                SelfRegistration = (x.SelfRegistration != null ? string.Format("{0:N0}", x.SelfRegistration) : "")                                  //自社登録
        //                ,
        //                ReductionTotal = (x.ReductionTotal != null ? string.Format("{0:N0}", x.ReductionTotal) : "")                                        //他勘定振替
        //                ,
        //                DemoCar = (x.DemoCar != null ? string.Format("{0:N0}", x.DemoCar) : "")                                                             //デモカー
        //                ,
        //                TemporaryCar = (x.TemporaryCar != null ? string.Format("{0:N0}", x.TemporaryCar) : "")                                              //代車
        //                ,
        //                RentalCar = (x.RentalCar != null ? string.Format("{0:N0}", x.RentalCar) : "")                                                       //レンタカー
        //                ,
        //                BusinessCar = (x.BusinessCar != null ? string.Format("{0:N0}", x.BusinessCar) : "")                                                //業務車
        //                ,
        //                PRCar = (x.PRCar != null ? string.Format("{0:N0}", x.PRCar) : "")                                                                   //PRCar
        //                ,
        //                EndInventory = (x.EndInventory != null ? string.Format("{0:N0}", x.EndInventory) : "")                                              //月末在庫
        //            }
        //        ).ToList()

        //    );


        //   return elist.OrderBy(x => x.PurchaseDate).ThenBy(x => x.SalesCarNumber).ToList();

        //}

        #endregion


        //Del Excelに変更したため、csvは削除

        #region validatione関連
        /// <summary>
        /// 共通Validationチェック
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns></returns>
        /// <history>
        ///  2015/04/09 arc yano 車両管理対応④ ValidationCheck処理追加
        /// </history>
        private void CommonValidation(FormCollection form)
        {
            DateTime targetDate = new DateTime();

            //対象年月
            string strtargetDate = form["TargetMonth"].ToString() + "/01";

            bool ret = DateTime.TryParse(strtargetDate, out targetDate);

            //当月と比較する
            DateTime thismonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            //入力した対象年月が未来の場合
            if (targetDate > thismonth)
            {
                ModelState.AddModelError("TargetMonth", "対象年月に未来の月は入力できません");
            }
            
        }

        /// <summary>
        /// 締め処理時のValidationチェック
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両管理締め処理状況</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善②
        /// </history>
        private CloseMonthControlCarStock CloseValidation(FormCollection form)
        {
            CloseMonthControlCarStock target = null;

            //対象年月
            string strtargetDate = form["TargetMonth"].ToString() + "/01";

            DateTime targetDate;
            
            bool ret = DateTime.TryParse(strtargetDate, out targetDate);

            if (ret)
            {
                //当月が締まっているかを確認
                target = new CloseMonthControlCarStockDao(db).GetByKey(strtargetDate.Replace("/", ""));

                //対象月が締まっている場合
                if (target != null && target.CloseStatus.Equals("002"))
                {
                    ModelState.AddModelError("TargetMonth", "対象月が締まっているため、締め処理を行うことができません");

                    target = null;
                }
                else
                {
                    //前月が締まっているかを確認する
                    string srtPreviousMonth = string.Format("{0:yyyyMMdd}", targetDate.AddMonths(-1));

                    bool closed = new CloseMonthControlCarStockDao(db).IsClosedMonth(srtPreviousMonth);

                    if (closed)
                    {
                        ModelState.AddModelError("TargetMonth", "前月が締まっていないため、締め処理を行うことができません");
                        target = null;
                    }
                }
            }

            return target;
        }
        /// <summary>
        /// 締め解除時のValidationチェック
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善②
        /// </history>
        private CloseMonthControlCarStock ReleaseValidation(FormCollection form)
        {
            CloseMonthControlCarStock target = null;

            //対象年月
            string strtargetDate = form["TargetMonth"].ToString() + "/01";

            //当月が締まっているかを確認
            target = new CloseMonthControlCarStockDao(db).GetByKey(strtargetDate.Replace("/", ""));

            //対象月が締まっていない場合
            if (target == null || !target.CloseStatus.Equals("002"))
            {
                ModelState.AddModelError("TargetMonth", "対象月が締まっていないため、締め処理を行うことができません");

                target = null;
            }

            return target;
        }
        #endregion
        //Del 2016/11/30 arc yano #3659 車両管理 未使用のため、コメントアウト(車両管理データ作成は、本締め時に行う)
        
        /// <summary>
        /// フィールド定義データから出力フィールドリストを取得する
        /// </summary>
        /// <param name="documentName">帳票名</param>
        /// <returns></returns>
        private IEnumerable<XElement> GetFieldList(string documentName)
        {
            XDocument xml = XDocument.Load(Server.MapPath("/Models/ExportFieldList.xml"));
            var query = (from x in xml.Descendants("Title")
                         where x.Attribute("ID").Value.Equals(documentName)
                         select x).FirstOrDefault();
            if (query == null)
            {
                return null;
            }
            else
            {
                var list = from a in query.Descendants("Name") select a;
                return list;
            }
            
            
        }
        /// <summary>
        /// 検索条件をセットする
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　検索条件の変更
        /// </history>
        private CarStock SetCondition(FormCollection form)
        {
            CarStock carstockCondition = new CarStock();
            carstockCondition.TargetMonth = form["TargetMonth"].ToString();
            carstockCondition.DataKind = form["DataKind"]; 
            carstockCondition.NewUsedType = form["NewUsedType"];
            carstockCondition.SupplierCode = form["SupplierCode"];
            carstockCondition.SalesCarNumber = form["SalesCarNumber"];
            carstockCondition.Vin = form["Vin"];

            //Del 2018/08/28 yano #3922 
            ////日付の範囲の設定
            //DateTime[] dateRange = SetDayRange(form["TargetMonth"].ToString());

            //carstockCondition.DateFrom = dateRange[0];
            //carstockCondition.DateTo = dateRange[1];

            return carstockCondition;
        }

        //Del 2016/11/30 arc yano #3659 車両管理 使用しなくなったため、削除

        /// <summary>
        /// 車両管理データ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両管理マスタ検索結果リスト</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表) 締め処理状況の参照テーブルを変更する
        /// Mod 2016/11/30 arc yano #3659 車両管理　対象年月が経理締めかどうかでコールするメソッドを変更する
        /// </history>
        private IQueryable<CarStock> GetSearchResultList(FormCollection form)
        {
            IQueryable<CarStock> list = null;
           
            CarStockDao carstocktsDao = new CarStockDao(db);

            bool ret = new CloseMonthControlCarStockDao(db).IsClosedMonth((form["TargetMonth"].ToString() + "/01").Replace("/", ""));

            if (ret == true)    //本締めでない場合
            {
                list = carstocktsDao.GetNonClosedListByCondition(SetCondition(form));
            }
            else //本締めの場合
            {
                list = carstocktsDao.GetClosedListByCondition(SetCondition(form));
            }

            return list;
        }


        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="closeStatusVisible">締処理状況表示フラグ</param>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 新規作成　締状況の取得先の変更
        /// </history>
        private void SetComponent(FormCollection form, bool closeStatusVisible)
        {
            CodeDao dao = new CodeDao(db);

            ViewData["TargetMonth"] = form["TargetMonth"];
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), form["NewUsedType"], true);
            ViewData["DataKindList"] = CodeUtils.GetSelectListByModel(dao.GetCarStockDataTypeAll(false), form["DataKind"], false);
            ViewData["SalesCarNumber"] = form["SalesCarNumber"];
            ViewData["Vin"] = form["Vin"];
            ViewData["SupplierCode"] = form["SupplierCode"];
            ViewData["OperateFlag"] = form["hdOperateFlag"];
            ViewData["OperateUser"] = form["hdOperateUser"];

            
            //Add 2015/04/09 arc yano 車両管理対応④　締処理状況の追加
            if (closeStatusVisible == true)
            {
                string targetDay = ViewData["TargetMonth"] + "/01";
                targetDay = targetDay.Replace("/", "");

                //対象年月が想定される文字数でない場合(初期表示時のみ)
                if (targetDay.Length != 8)
                {
                    ViewData["CloseStatus"] = "";
                }
                else
                {
                    CloseMonthControlCarStock rec = new CloseMonthControlCarStockDao(db).GetByKey(targetDay);       //Mod 2018/08/28 yano #3922
                    //CloseMonthControl rec = new CloseMonthControlDao(db).GetByKey(targetDay);
                    if (rec != null)
                    {
                        //ViewData["CloseStatus"] = rec.c_CloseStatus == null ? "" : rec.c_CloseStatus.Name;

                        ViewData["CloseStatusName"] = rec.CloseStatus == "002" ? "締済" : "未締";
                        ViewData["CloseStatus"] = rec.CloseStatus;
                    }
                    else
                    {
                        ViewData["CloseStatusName"] = "未締";
                        ViewData["CloseStatus"] = "001";
                    }

                    if (!string.IsNullOrEmpty(form["SupplierCode"]))
                    {
                        ViewData["SupplierName"] = (new SupplierDao(db)).GetByKey(form["SupplierCode"]).SupplierName;
                    }
                }
            }
            else
            {
                ViewData["CloseStatus"] = null;
            }
        }
        #endregion

        //Del 2016/11/30 arc yano #3659 車両管理　未使用となったため、削除

        // Del 2015/04/08 arc yano 

        //Mod 2015/04/09 arc yano 車両管理対応④ 車両管理データの再作成の条件の変更(検索対象年月＝当月→対象年月＝締め済)
        /// <summary>
        /// 対象年月の締め処理状況の作成・更新
        /// </summary>
        /// <param name="targetMonth">年月</param>
        /// <returns name="ret">締済の場合=true,それ以外は=false</returns>
        private int SetClosedStatus(string targetMonth)
        {
            //戻り値
            int ret = 0;

            //対象年月日を設定(対象年月の１日とする)
            string targetDay = targetMonth + "/01";

            targetDay = targetDay.Replace("/", "");

            //対象年月で車両管理用の管理テーブルのレコードを取得
            CloseMonthControlCarStock rec = new CloseMonthControlCarStockDao(db).GetByKey(targetDay);

            //対象年月で締処理状況テーブルのレコードを取得
            CloseMonthControl cmcrec = new CloseMonthControlDao(db).GetByKey(targetDay);

            //車両管理用管理テーブルが無い場合(レコードを新規作成)
            if (rec == null)
            {
                //締処理状況のレコードがある場合
                if (cmcrec != null)
                {
                    CloseMonthControlCarStock newrec = new CloseMonthControlCarStock();
                    newrec.CloseMonth = cmcrec.CloseMonth;
                    newrec.CloseStatus = cmcrec.CloseStatus;
                    newrec.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    newrec.CreateDate = DateTime.Now;
                    newrec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    newrec.LastUpdateDate = DateTime.Now;
                    newrec.DelFlag = "0";
                    
                    //作成したレコードをインサート
                    db.CloseMonthControlCarStock.InsertOnSubmit(newrec);
                }
            }
            //更新時
            else
            {
                rec.CloseStatus = cmcrec.CloseStatus;
                rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                rec.LastUpdateDate = DateTime.Now;
            }

            return ret;
        }


        //Del 2016/11/30 arc yano #3659 未使用のため、コメントアウト 車両管理データ作成処理は本締めした際に作成する

        //Del 2018/08/28 yano #3922 
        ///// <summary>
        ///// 日付の範囲をセットする
        ///// </summary>
        ///// <param name="targetMonth">年月</param>
        ///// <returns name="dateRange">日付の範囲を設定する</returns>
        //private DateTime[] SetDayRange(string targetMonth)
        //{
        //    DateTime[] dateRange = new DateTime[2];


        //    //日付の範囲の設定

        //    //対象年月をDateTimeに変換(日は1日とする。)
        //    DateTime targetDay = DateTime.ParseExact((targetMonth + "/01"), "yyyy/MM/dd", null);

        //    //当月(文字列)
        //    string thismonth = string.Format("{0:yyyyMM}", System.DateTime.Today);


        //    //検索条件の対象年月を当月と比較する。
        //    if (targetMonth.Replace("/", "").Equals(thismonth))  //対象年月 = 当月
        //    {
        //        //当月の月初
        //        dateRange[0] = DateTime.ParseExact(System.DateTime.Today.ToString("yyyy/MM/01"), "yyyy/MM/dd", null);

        //        //当日
        //        dateRange[1] = System.DateTime.Today;
        //    }
        //    else
        //    {
        //        //対象年月の月初
        //        dateRange[0] = targetDay;

        //        //対象年月の月末
        //        dateRange[1] = new DateTime(targetDay.Year, targetDay.Month, DateTime.DaysInMonth(targetDay.Year, targetDay.Month));
        //    }
            
        //    return dateRange;
        //}

        #region ファイナンスデータ取込処理
        /// <summary>
        /// Excel取込用ダイアログ表示
        /// </summary>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        [AuthFilter]
        public ActionResult ImportDialog()
        {
            List<FinancialDataExcelImport> ImportList = new List<FinancialDataExcelImport>();
            FormCollection form = new FormCollection();

            ViewData["ErrFlag"] = "1";

            return View("CarStockImportDialog", ImportList);
        }

        /// <summary>
        /// Excel読み込み
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, FormCollection form)
        {
            List<FinancialDataExcelImport> ImportList = new List<FinancialDataExcelImport>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                //--------------
                //Excel読み込み
                //--------------
                case "1":
                    //Excel読み込み前のチェック
                    ValidateExcelFile(importFile, form);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarStockImportDialog", ImportList);
                    }

                    //Excel読み込み
                    ImportList = ReadExcelData(importFile, ImportList);

                    //読み込み時に何かエラーがあればここでリターン
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarStockImportDialog");
                    }

                    //Excelで読み込んだデータのバリデートチェック
                    ValidateImportList(ImportList, form);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarStockImportDialog");
                    }

                    //DB登録
                    DBExecute(ImportList, form);

                    form["ErrFlag"] = "1";
                    SetDialogDataComponent(form);
                    return View("CarStockImportDialog");

                //--------------
                //キャンセル
                //--------------
                case "2":
                    ImportList = new List<FinancialDataExcelImport>();
                    ViewData["ErrFlag"] = "1";//[取り込み]ボタンが押せないようにするため
                    SetDialogDataComponent(form);
                    return View("CarStockImportDialog", ImportList);

                //----------------------------------
                //その他(ここに到達することはない)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("CarStockImportDialog");
            }
        }

        /// Excelデータ取得&設定
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <param name="ImportList">ImportList</param>
        /// <returns>なし</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        private List<FinancialDataExcelImport> ReadExcelData(HttpPostedFileBase importFile, List<FinancialDataExcelImport> ImportList)
        {
            using (var pck = new ExcelPackage())
            {
                try
                {
                    pck.Load(importFile.InputStream);
                }
                catch (System.IO.IOException ex)
                {
                    if (ex.Message.Contains("because it is being used by another process."))
                    {
                        ModelState.AddModelError("importFile", "対象のファイルが開かれています。ファイルを閉じてから、再度実行して下さい");
                        return ImportList;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("importFile", "エラーが発生しました。" + ex.Message);
                    return ImportList;
                }

                //-----------------------------
                // データシート取得
                //-----------------------------
                var ws = pck.Workbook.Worksheets[1];

                //--------------------------------------
                //読み込むシートが存在しなかった場合
                //--------------------------------------
                if (ws == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。確認して再度実行して下さい"));
                    return ImportList;
                }
                //------------------------------
                //読み込み行が0件の場合
                //------------------------------
                if (ws.Dimension == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。更新処理を終了しました"));
                    return ImportList;
                }

                //読み取りの開始位置と終了位置を取得
                int StartRow = ws.Dimension.Start.Row;　       //行の開始位置
                int EndRow = ws.Dimension.End.Row;             //行の終了位置
                int StartCol = ws.Dimension.Start.Column;      //列の開始位置
                int EndCol = ws.Dimension.End.Column;          //列の終了位置

                var headerRow = ws.Cells[StartRow, StartCol, StartRow, EndCol];

                //タイトル行、ヘッダ行がおかしい場合は即リターンする
                if (!ModelState.IsValid)
                {
                    return ImportList;
                }

                //------------------------------
                // 読み取り処理
                //------------------------------
                int datarow = 0;
                string[] Result = new string[ws.Dimension.End.Column];

                for (datarow = StartRow + 1; datarow < EndRow + 1; datarow++)
                {
                    //更新データの取得
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)
                    {
                        Result[col - 1] = !string.IsNullOrWhiteSpace(ws.Cells[datarow, col].Text) ? Strings.StrConv(ws.Cells[datarow, col].Text.Trim(), VbStrConv.Narrow, 0x0411).ToUpper() : "";
                    }

                    //----------------------------------------
                    // 読み取り結果を画面の項目にセットする
                    //----------------------------------------
                    ImportList = SetProperty(ref Result, ref ImportList);
                }
            }
            return ImportList;

        }

        /// <summary>
        /// Excel読み取り前のチェック
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        private void ValidateExcelFile(HttpPostedFileBase filePath, FormCollection form)
        {
            // 必須チェック
            if (filePath == null || string.IsNullOrEmpty(filePath.FileName))
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルを選択してください"));
            }
            else
            {
                // 拡張子チェック
                System.IO.FileInfo cFileInfo = new System.IO.FileInfo(filePath.FileName);
                string stExtension = cFileInfo.Extension;

                if (stExtension.IndexOf("xlsx") < 0)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルの拡張子がxlsxファイルではありません"));
                }
            }

            return;
        }

        /// <summary>
        /// 読み込み結果のバリデーションチェック
        /// </summary>
        /// <param name="ImportList">Excelデータ</param>
        /// <param name="form">フォーム入力値</param>
        /// <returns>なし</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善②
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        public void ValidateImportList(List<FinancialDataExcelImport> ImportList, FormCollection form)
        {
            SalesCar salescar = null;
            SalesCar condition = new SalesCar();

            CarPurchase carpurchase = null;

            //現時点の税率を初期で設定
            int taxrate = new ConsumptionTaxDao(db).GetByKey(new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(DateTime.Today)).Rate;

            decimal carinsurance = 0m;

            //JLR車両保険料の取得
            ConfigurationSetting config = new ConfigurationSettingDao(db).GetByKey("GL_CarInsurance");
            if (config == null)
            {
                ModelState.AddModelError("", "JLR車両の保険料がアプリケーション設定で設定されていません。");
            }
            else
            {
                bool result = decimal.TryParse(config.Value, out carinsurance);
            }
            
            for (int i = 0; i < ImportList.Count; i++)
            {
                //----------------
                //車台番号
                //----------------
                //必須チェック
                if(string.IsNullOrEmpty(ImportList[i].Vin))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目の車台番号が入力されていません。車台番号"));
                }
                else //マスタチェック
                {
                    //車両マスタを検索
                    condition.NewUsedType = "N";
                    //車台番号
                    condition.Vin = ImportList[i].Vin;

                    salescar = new SalesCarDao(db).GetListByCondition(condition).FirstOrDefault();

                    if (salescar == null)
                    {
                        ModelState.AddModelError("", i + 1 + "行目の車両データが存在しません。車台番号を確認後、再度取込を行って下さい");
                    }
                    else
                    {
                        //車両仕入データを検索する
                        carpurchase = new CarPurchaseDao(db).GetBySalesCarNumber(salescar.SalesCarNumber);

                        if (carpurchase == null || !carpurchase.PurchaseStatus.Equals("002"))
                        {
                            ModelState.AddModelError("", i + 1 + "行目の仕入済の車両仕入データが存在しません。管理番号を確認後、再度取込を行って下さい");
                        }
                        else
                        {
                            //管理番号をセット
                            ImportList[i].SalesCarNumber = carpurchase.SalesCarNumber;
                        }
                    }
                }
                //----------------
                //財務価格
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].FinancialAmount))  //必須チェック
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目の財務価格が入力されていません。財務価格"));
                }
                else
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].FinancialAmount.Replace(",", ""), @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "財務価格", "正の整数10桁以内のみ" }));
                    }
                    else //Add 2018/08/28 yano #3922
                    {
                        if (!string.IsNullOrWhiteSpace(ImportList[i].SalesCarNumber))
                        {
                            //仕入データに消費税率IDが設定されている場合
                            ConsumptionTax rate = new ConsumptionTaxDao(db).GetByKey(carpurchase.ConsumptionTaxId);

                            if (rate != null)
                            {
                                taxrate = rate.Rate;
                            }

                            //税抜金額を算出する
                            switch (carpurchase.SupplierCode)
                            {
                                //GLコネクト仕入(JLRからの仕入)
                                case "KK00000770":
                                case "KK00000843":
                                    //仕入価格を計算　((仕入総額－保険料) × 100 ÷ (100＋税率)) + 保険料 ※四捨五入
                                    ImportList[i].CalcFinancialAmount = CommonUtils.CalcAmountWithoutTax(decimal.Parse(ImportList[i].FinancialAmount) - carinsurance, taxrate, 2) + carinsurance;  
                                    break;

                                //ジャックス仕入(FCAからの仕入)
                                default:
                                    //仕入価格を計算　(仕入金額(税込) × 100 ÷ (100＋税率)) ※端数切捨て
                                    ImportList[i].CalcFinancialAmount =  CommonUtils.CalcAmountWithoutTax(decimal.Parse(ImportList[i].FinancialAmount), taxrate, 1);  
                                    break;
                            }

                        }
                    }
                }

                carpurchase = null;     //仕入データをリセット
                salescar = null;        //車両データをリセット
            }

            //-----------------
            //重複チェック
            //-----------------
            var rec = ImportList.GroupBy(x => x.Vin).Select(c => new { Vin = c.Key, Count = c.Count() }).Where(c => c.Count > 1);

            foreach (var a in rec)
            {
                if (!string.IsNullOrWhiteSpace(a.Vin))
                {
                    ModelState.AddModelError("", "取込むファイルの中に車台番号[" + a.Vin + "]が複数定義されています");
                }
            }
        }

        /// <summary>
        /// Excelの読み取り結果をリストに設定する
        /// </summary>
        /// <param name="Result">Excelセルの値</param>
        /// <param name="ImportList">Excelセルの値の保存先</param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        public List<FinancialDataExcelImport> SetProperty(ref string[] Result, ref List<FinancialDataExcelImport> ImportList)
        {
            FinancialDataExcelImport SetLine = new FinancialDataExcelImport();

            // 車台番号
            SetLine.Vin = Result[0];
            // 財務価格
            SetLine.FinancialAmount = Result[1];
       
            ImportList.Add(SetLine);

            return ImportList;
        }

        /// <summary>
        /// ダイアログのデータ付きコンポーネント設定
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善②
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        private void SetDialogDataComponent(FormCollection form)
        {
            ViewData["ErrFlag"] = form["ErrFlag"];
            ViewData["RequestFlag"] = form["RequestFlag"];
            ViewData["id"] = form["id"];

            //Add 2018/08/28 yano #3922
            if (!string.IsNullOrWhiteSpace(form["TargetMonth"]))
            {
                ViewData["TargetMonth"] = form["TargetMonth"];
            }
        }

        /// <summary>
        /// 読み込んだデータをDBに登録
        /// </summary>
        /// <returns>戻り値(0:正常 1:エラー(償却率マスタ取込画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善②
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        private void DBExecute(List<FinancialDataExcelImport> ImportList, FormCollection form)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                //財務価格の更新
                foreach (var LineData in ImportList)
                {
                    CarPurchase editDate = new CarPurchaseDao(db).GetBySalesCarNumber(LineData.SalesCarNumber);

                    if (editDate != null)
                    {
                        //Mod 2018/08/28 yano #3922
                        editDate.FinancialAmount = LineData.CalcFinancialAmount;                             //財務価格
                        editDate.FinancialAmountLocked = "1";                                                //ファイナンスデータ取込済フラグ
                        //editDate.FinancialAmount = decimal.Parse(LineData.FinancialAmount);                //財務価格
                        editDate.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //最終更新者
                        editDate.LastUpdateDate = DateTime.Now;                                             //最終更新日

                        //さらに自社登録の場合は中古車の仕入データの財務価格も更新する
                        BackGroundDemoCar bgrec = new BackGroundDemoCarDao(db).GetBySalesCarNumber(LineData.SalesCarNumber);

                        if (bgrec != null && bgrec.ProcType.Equals("006"))
                        {
                            CarPurchase editDate2 = new CarPurchaseDao(db).GetBySalesCarNumber(bgrec.NewSalesCarNumber);

                            if (editDate2 != null)
                            {
                                editDate2.FinancialAmount = LineData.CalcFinancialAmount;                            //財務価格
                                editDate2.FinancialAmountLocked = "1";                                               //ファイナンスデータ取込済フラグ
                                editDate2.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //最終更新者
                                editDate2.LastUpdateDate = DateTime.Now;                 
                            }
                        }

                        //Add 2018/08/28 yano #3922
                        //さらに固定資産テーブルにデータがあった場合は取得価格をファイナンスデータで更新する
                        CarFixedAssets rec = new CarFixedAssetsDao(db).GetByKey(LineData.SalesCarNumber);

                        if (rec != null)
                        {
                            rec.AcquisitionPrice = LineData.CalcFinancialAmount;
                            rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //最終更新者
                            rec.LastUpdateDate = DateTime.Now;              
                        }
                    }
                }
                try
                {
                    db.SubmitChanges();
                    ts.Complete();

                    //取り込み完了のメッセージを表示する
                    ModelState.AddModelError("", "取り込みが完了しました。");
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME_EXCELUPLOAD, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_EXCELUPLOAD, FORM_NAME, "");
                    ts.Dispose();
                }
            }
        }
        #endregion

        #region 車両管理表取込

        /// <summary>
        /// Excel取込用ダイアログ表示
        /// </summary>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表) 機能改善② 新規作成
        /// </history>
        [AuthFilter]
        public ActionResult ImportDialogForCarStock()
        {
            List<CarStockForExcelImport> ImportList = new List<CarStockForExcelImport>();
            FormCollection form = new FormCollection();

            ViewData["TargetMonth"] = Request["TargetMonth"];

            ViewData["ErrFlag"] = "1";

            return View("CarStockImportDialogForCarStock", ImportList);
        }

        /// <summary>
        /// Excel読み込み
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表) 機能改善② 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialogForCarStock(HttpPostedFileBase importFile, FormCollection form)
        {
            List<CarStockForExcelImport> ImportList = new List<CarStockForExcelImport>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                //--------------
                //Excel読み込み
                //--------------
                case "1":
                    //Excel読み込み前のチェック
                    ValidateExcelFile(importFile, form);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarStockImportDialogForCarStock", ImportList);
                    }

                    //Excel読み込み
                    ImportList = ReadExcelDataForCartStock(importFile, ImportList);

                    //読み込み時に何かエラーがあればここでリターン
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarStockImportDialogForCarStock");
                    }

                    //Excelで読み込んだデータのバリデートチェック
                    ValidateImportListForCarStock(ImportList, form);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarStockImportDialogForCarStock");
                    }

                    //DB登録
                    DBExecuteForCarStock(ImportList, form);

                    form["ErrFlag"] = "1";
                    SetDialogDataComponent(form);
                    return View("CarStockImportDialogForCarStock");

                //--------------
                //キャンセル
                //--------------
                case "2":
                    ImportList = new List<CarStockForExcelImport>();
                    ViewData["ErrFlag"] = "1";//[取り込み]ボタンが押せないようにするため
                    SetDialogDataComponent(form);
                    return View("CarStockImportDialogForCarStock", ImportList);

                //----------------------------------
                //その他(ここに到達することはない)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("CarStockImportDialogForCarStock");
            }
        }

        /// Excelデータ取得&設定
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <param name="ImportList">ImportList</param>
        /// <returns>なし</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表) 機能改善② 新規作成
        /// </history>
        private List<CarStockForExcelImport> ReadExcelDataForCartStock(HttpPostedFileBase importFile, List<CarStockForExcelImport> ImportList)
        {

            using (var pck = new ExcelPackage())
            {
                try
                {
                    pck.Load(importFile.InputStream);
                }
                catch (System.IO.IOException ex)
                {
                    if (ex.Message.Contains("because it is being used by another process."))
                    {
                        ModelState.AddModelError("importFile", "対象のファイルが開かれています。ファイルを閉じてから、再度実行して下さい");
                        return ImportList;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("importFile", "エラーが発生しました。" + ex.Message);
                    return ImportList;
                }

                //-----------------------------
                // データシート取得
                //-----------------------------
                var ss = pck.Workbook.Worksheets[2];            //設定シート
                var ws = pck.Workbook.Worksheets[3];            //新車シート
                var ws2 = pck.Workbook.Worksheets[4];           //中古車シート

                string newcarsheetname = ws.Name;               //新車シート名
                string oldcarsheetname = ws2.Name;              //中古車シート名

                //--------------------------------------
                //読み込むシートが存在しなかった場合
                //--------------------------------------
                if (ws == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。確認して再度実行して下さい"));
                }
                if (ws2 == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelに中古車シートがありません。確認して再度実行して下さい"));
                }
                //validationエラーが発生しているは即リターン
                if (!ModelState.IsValid)
                {
                    return ImportList;
                }

                //------------------------------
                //読み込み行が0件の場合
                //------------------------------
                if (ws.Dimension == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", ws.Name + "にデータがありません。更新処理を終了しました"));
                }
                if (ws2.Dimension == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", ws2.Name + "にデータがありません。更新処理を終了しました"));
                }

                //-------------------------------
                //設定シートの対象年月読取
                //-------------------------------
                string targeDate = ss.Cells["C6"].GetValue<string>();

                //---------------------------------
                //新車シート読み取り
                //---------------------------------
                //読み取りの開始位置と終了位置を取得
                int StartRow = ws.Dimension.Start.Row;　       //行の開始位置
                int EndRow = ws.Dimension.End.Row;             //行の終了位置
                int StartCol = ws.Dimension.Start.Column;      //列の開始位置
                int EndCol = ws.Dimension.End.Column;          //列の終了位置

                //実データ開始位置
                int startPos = ss.Cells["C3"].GetValue<int>();
                //実データ行数
                int rowCnt = ss.Cells["D3"].GetValue<int>();
                //新車・中古車区切数
                int separateCnt = rowCnt;

                int datarow = 0;

                string[] ResultNewCar = new string[ws.Dimension.End.Column];

                for (datarow = startPos; datarow < rowCnt + startPos; datarow++)
                {
                    //更新データの取得
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)
                    {
                        ResultNewCar[col - 1] = !string.IsNullOrWhiteSpace(ws.Cells[datarow, col].Text) ? ws.Cells[datarow, col].Text.Trim() : "";
                    }

                    //----------------------------------------
                    // 読み取り結果を画面の項目にセットする
                    //----------------------------------------
                    ImportList = SetPropertyForCarStock(ref ResultNewCar, ref ImportList, targeDate, 0, newcarsheetname, separateCnt);
                }

                //---------------------------------
                //中古車シート読み取り
                //---------------------------------
                //読み取りの開始位置と終了位置を取得
                StartRow = ws2.Dimension.Start.Row;　       //行の開始位置
                EndRow = ws2.Dimension.End.Row;             //行の終了位置
                StartCol = ws2.Dimension.Start.Column;      //列の開始位置
                EndCol = ws2.Dimension.End.Column;          //列の終了位置

                //実データ開始位置
                startPos = ss.Cells["C4"].GetValue<int>();
                //実データ行数
                rowCnt = ss.Cells["D4"].GetValue<int>();

                datarow = 0;

                string[] ResultOldCar = new string[ws2.Dimension.End.Column];

                for (datarow = startPos; datarow < rowCnt + startPos; datarow++)
                {
                    //更新データの取得
                    for (int col = 1; col <= ws2.Dimension.End.Column; col++)
                    {
                        ResultOldCar[col - 1] = !string.IsNullOrWhiteSpace(ws2.Cells[datarow, col].Text) ? ws2.Cells[datarow, col].Text.Trim() : "";
                    }

                    //----------------------------------------
                    // 読み取り結果を画面の項目にセットする
                    //----------------------------------------
                    ImportList = SetPropertyForCarStock(ref ResultOldCar, ref ImportList, targeDate, 1, oldcarsheetname, separateCnt);
                }
            }
            return ImportList;

        }

        /// <summary>
        /// 読み込み結果のバリデーションチェック
        /// </summary>
        /// <param name="ImportList">Excelデータ</param>
        /// <param name="form">フォーム入力値</param>
        /// <returns>なし</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 新規作成
        /// </history>
        public void ValidateImportListForCarStock(List<CarStockForExcelImport> ImportList, FormCollection form)
        {
            SalesCar salescar = null;
            List<SalesCar> slist = null;

            SalesCar condition = new SalesCar();
            List<SalesCar> scList = new SalesCarDao(db).GetListByCondition(condition);

            List<string> salesCarList = new List<string>();

            for (int i = 0; i < ImportList.Count; i++)
            {
                //シート名
                string sheetName = ImportList[i].SheetName;
                //行数
                int rowCnt = (i >= ImportList[i].SeparateCnt ? i - ImportList[i].SeparateCnt + 1 : i + 1);

                //----------------
                //入庫日
                //----------------
                //入庫日チェック
                if (string.IsNullOrEmpty(ImportList[i].PurchaseDate))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", "シート：" + sheetName + " " + rowCnt + "行目の入庫日が入力されていません。入庫日"));
                }
                else //フォーマットチェック
                {
                    DateTime purchaseDate = new DateTime();

                    if (DateTime.TryParse(ImportList[i].PurchaseDate, out purchaseDate) == false)
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0005", "シート：" + sheetName + " " + rowCnt + "行目の入庫日"));
                    }
                }
                //----------------
                //管理番号
                //----------------
                //必須チェック
                if (string.IsNullOrEmpty(ImportList[i].SalesCarNumber))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", "シート：" + sheetName + " " + rowCnt + "行目の管理Noが入力されていません。管理No"));
                }
                else //マスタチェック
                {
                    salescar = scList.Where(x => x.SalesCarNumber.Trim().Equals(ImportList[i].SalesCarNumber)).FirstOrDefault();

                    //車両マスタが存在しない
                    if (salescar == null)
                    {
                        ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の管理No(" + ImportList[i].SalesCarNumber + ")は車両マスタに存在していません");
                    }
                    else
                    {
                        salesCarList.Add(ImportList[i].SalesCarNumber);
                    }
                }
                //----------------
                //車台番号
                //----------------
                //必須チェック
                if (string.IsNullOrEmpty(ImportList[i].Vin))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", "シート：" + sheetName + " " + rowCnt + "行目の車台Noが入力されていません。車台No"));
                }
                else //マスタチェック
                {
                    slist = scList.Where(x => x.Vin.Trim().Equals(ImportList[i].Vin)).ToList();

                    //車両マスタが存在しない
                    if(slist == null || slist.Count == 0)
                    {
                        ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の車台No(" + ImportList[i].Vin + ")は車両マスタに存在していません");
                    }
                    else
                    {
                        //車両管理番号と車台番号が不一致
                        if(!string.IsNullOrWhiteSpace(ImportList[i].SalesCarNumber) && slist.Where(x => x.SalesCarNumber.Trim().Equals(ImportList[i].SalesCarNumber)).ToList().Count == 0)
                        {
                            ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の管理No(" + ImportList[i].SalesCarNumber + ")と車台No(" + ImportList[i].Vin + ")は同じ車両ではありません");
                        }
                    }
                }
                //----------------
                //月初在庫
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].BeginningInventory))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].BeginningInventory.Replace(",", ""), @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の月初在庫", "正の整数10桁以内" }));
                    }
                }
                //----------------
                //当月仕入
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].MonthPurchase))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].MonthPurchase.Replace(",", ""), @"^\d{1,10}$") && !Regex.IsMatch(ImportList[i].MonthPurchase.Replace(",", ""), @"^-?\d{1,9}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の当月仕入", "正の整数10桁以内、または負の整数9桁以内" }));
                    }
                }
                //----------------
                //他勘定受入
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].OtherAccount))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].OtherAccount.Replace(",", ""), @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の他勘定受入", "正の整数10桁以内" }));
                    }
                }
                //----------------
                //リサイクル料
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].RecycleAmount))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].RecycleAmount.Replace(",", ""), @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目のリサイクル料", "正の整数10桁以内" }));
                    }
                }
                //----------------
                //車両本体価格
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].SalesPrice))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].SalesPrice.Replace(",", ""), @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の車両本体価格", "正の整数10桁以内" }));
                    }
                }
                //----------------
                //値引
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].DiscountAmount))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].DiscountAmount.Replace(",", ""), @"^-?\d{1,9}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の値引", "負の整数9桁以内" }));
                    }
                }
                //----------------
                //付属品
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].ShopOptionAmount))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].ShopOptionAmount.Replace(",", ""), @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の付属品", "正の整数10桁以内" }));
                    }
                }
                //----------------
                //諸費用
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].SalesCostTotalAmount))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].SalesCostTotalAmount.Replace(",", ""), @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の諸費用", "正の整数10桁以内" }));
                    }
                }
                //----------------
                //売上総合計
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].SalesTotalAmount))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].SalesTotalAmount.Replace(",", ""), @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の売上総合計", "正の整数10桁以内" }));
                    }
                }
                //----------------
                //売上原価
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].SalesCostAmount))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].SalesCostAmount.Replace(",", ""), @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の売上原価", "正の整数10桁以内" }));
                    }
                }
                //----------------
                //粗利
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].SalesProfits))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].SalesProfits.Replace(",", ""), @"^\d{1,10}$") && !Regex.IsMatch(ImportList[i].SalesProfits.Replace(",", ""), @"^-?\d{1,9}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の粗利", "正の整数10桁以内、または負の整数9桁以内" }));
                    }
                }
                //----------------
                //自社登録
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].SelfRegistration))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].SelfRegistration.Replace(",", ""), @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の自社登録", "正の整数10桁以内のみ" }));
                    }
                }
                //----------------
                //他勘定振替
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].ReductionTotal))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].ReductionTotal.Replace(",", ""), @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の他勘定振替", "正の整数10桁以内のみ" }));
                    }
                }
                //----------------
                //月末在庫
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].EndInventory))
                {
                    //フォーマットチェック
                    if (!Regex.IsMatch(ImportList[i].EndInventory.Replace(",", ""), @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { "シート：" + sheetName + " " + rowCnt + "行目の月末在庫", "正の整数10桁以内のみ" }));
                    }
                }
                //----------------
                //メーカー名
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].MakerName))
                {
                    //文字長チェック
                    if (ImportList[i].MakerName.Length > 100)
                    {
                        ModelState.AddModelError("",  "シート：" + sheetName + " " + rowCnt + "行目のメーカー名は100文字以内で入力してください");
                    }
                }
                //----------------
                //車種名
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].CarName))
                {
                    //文字長チェック
                    if (ImportList[i].CarName.Length > 100)
                    {
                        ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の車種名は100文字以内で入力してください");
                    }
                }
                //----------------
                //仕入・在庫拠点
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].PurchaseLocationName))
                {
                    //文字長チェック
                    if (ImportList[i].PurchaseLocationName.Length > 50)
                    {
                        ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の仕入・在庫拠点は50文字以内で入力してください");
                    }
                }
                //----------------
                //当月実棚
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].InventoryLocationName))
                {
                    //文字長チェック
                    if (ImportList[i].InventoryLocationName.Length > 50)
                    {
                        ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の当月実棚は50文字以内で入力してください");
                    }
                }
                //----------------
                //仕入区分名
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].CarPurchaseTypeName))
                {
                    //文字長チェック
                    if (ImportList[i].CarPurchaseTypeName.Length > 50)
                    {
                        ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の仕入区分は50文字以内で入力してください");
                    }
                }
                //----------------
                //仕入先名
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].SupplierName))
                {
                    //文字長チェック
                    if (ImportList[i].SupplierName.Length > 80)
                    {
                        ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の仕入先名は80文字以内で入力してください");
                    }
                }
                //----------------
                //伝票番号
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].SlipNumber))
                {
                    //伝票存在チェック
                    CarSalesHeader sh = new CarSalesOrderDao(db).GetBySlipNumber(ImportList[i].SlipNumber);

                    //車両伝票が存在しない、または伝票ステータスが「納車済」でない場合
                    if (sh == null || !sh.SalesOrderStatus.Equals("005"))
                    {
                        ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の注文書Noは有効な納車済伝票ではありません");
                    }
                    else
                    {
                        //----------------
                        //納車日
                        //----------------
                        //フォーマットチェック
                        DateTime dSalesDate = new DateTime();

                        if (DateTime.TryParse(ImportList[i].SalesDate, out dSalesDate) == false)
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0005", "シート：" + sheetName + " " + rowCnt + "行目の納車日"));
                        }
                        else if(dSalesDate != sh.SalesDate)
                        {
                            ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の納車日がシステム上の納車日と異なっています");
                        }
                    }
                }
                //----------------
                //販売店舗
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].SalesDepartmentName))
                {
                    //文字長チェック
                    if (ImportList[i].SalesDepartmentName.Length > 20)
                    {
                        ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の販売店舗は20文字以内で入力してください");
                    }
                }
                //----------------
                //販売先区分
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].CustomerTypeName))
                {
                    //文字長チェック
                    if (ImportList[i].CustomerTypeName.Length > 50)
                    {
                        ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の販売先区分は50文字以内で入力してください");
                    }
                }
                //----------------
                //販売先
                //----------------
                if (!string.IsNullOrEmpty(ImportList[i].CustomerName))
                {
                    //文字長チェック
                    if (ImportList[i].CustomerName.Length > 80)
                    {
                        ModelState.AddModelError("", "シート：" + sheetName + " " + rowCnt + "行目の販売先は80文字以内で入力してください");
                    }
                }

                salescar = null;        //車両データをリセット
            }

            //-----------------
            //重複チェック
            //-----------------
            //var rec = ImportList.GroupBy(x => x.SalesCarNumber).Select(c => new { SalesCarNumber = c.Key, Count = c.Count() }).Where(c => c.Count > 1);
            var rec = salesCarList.GroupBy(x => x).Select(c => new { SalesCarNumber = c.Key, Count = c.Count() }).Where(c => c.Count > 1);

            foreach (var a in rec)
            {
                if (!string.IsNullOrWhiteSpace(a.SalesCarNumber))
                {
                    ModelState.AddModelError("", "取込むファイルの中に管理No[" + a.SalesCarNumber + "]が複数定義されています");
                }
            }

            //-----------------------------
            //システムの車両管理表との比較
            //-----------------------------
            form["DataKind"] = "003";           //種別 = 在庫＋販売

            //システムの車両管理表を取得
            HashSet<string> hsList = new HashSet<string>(GetSearchResultList(form).Select(x => x.SalesCarNumber).ToList());

            //Excelの車両管表
            HashSet<string> hsListExcel = new HashSet<string>(salesCarList.Select(x => x).ToList());

            //Excelにあってシステムにないものの抽出
            hsListExcel.ExceptWith(hsList);

            ////Excelにあってシステムにない
            foreach (var a in hsListExcel)
            {
                ModelState.AddModelError("", "システムに存在しない管理No[" + a + "]の車両が取込データに存在しています");
            }

            ////システムにあってExcelにないものの抽出
            hsList = new HashSet<string>(GetSearchResultList(form).Select(x => x.SalesCarNumber).ToList());

            hsListExcel = new HashSet<string>(ImportList.Select(x => x.SalesCarNumber).ToList());

            hsList.ExceptWith(hsListExcel);

            //システムにあってExcelにない場合はメッセージを表示
            foreach (var a in hsList)
            {
                ModelState.AddModelError("", "取込データに存在しない管理No[" + a + "]の車両がシステムに存在しています");
            }

            //読取したため、一旦リセット
            db.Dispose();
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// Excelno 読取結果をリストに設定する
        /// </summary>
        /// <param name="Result">Excelセルの値</param>
        /// <param name="ImportList">Excelセルの値の保存先</param>
        /// <param name="processDate">対象年月日</param>
        /// <param name="newUsedType">新中区分(0:新車 1:中古車)</param>
        /// <param name="sheetName">シート名</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 新規作成
        /// </history>
        public List<CarStockForExcelImport> SetPropertyForCarStock(ref string[] Result, ref List<CarStockForExcelImport> ImportList, string processDate, int newUsedType, string sheetName, int rowcnt)
        {
            CarStockForExcelImport SetLine = new CarStockForExcelImport();

            //-----------------------------
            //新車・中古車共通
            //-----------------------------
            //対象年月日
            SetLine.ProcessDate = processDate;
            //取扱ブランド
            SetLine.BrandStore = Result[0];
            //入庫日
            SetLine.PurchaseDate = Result[1];
            //管理番号
            SetLine.SalesCarNumber = !string.IsNullOrWhiteSpace(Result[2]) ? Strings.StrConv(Result[2], VbStrConv.Narrow, 0x0411).ToUpper() : "";
            //メーカー名
            SetLine.MakerName = Result[3];
            //車種名
            SetLine.CarName = Result[4];
            //車台番号
            SetLine.Vin = !string.IsNullOrWhiteSpace(Result[5]) ? Strings.StrConv(Result[5], VbStrConv.Narrow, 0x0411).ToUpper() : "";
            //仕入・在庫拠点
            SetLine.PurchaseLocationName = Result[6];
            //実棚
            SetLine.InventoryLocationName = Result[7];
            //シート名
            SetLine.SheetName = sheetName;
            //実データ行数
            SetLine.SeparateCnt = rowcnt;

            //-----------------------------
            //新車
            //-----------------------------
            if (newUsedType == 0)
            {
                //仕入先名
                SetLine.SupplierName = Result[8];
                //月初在庫
                SetLine.BeginningInventory = Result[9];
                //当月仕入
                SetLine.MonthPurchase = Result[10].Replace("(", "-").Replace(")", "");
                //納車日
                SetLine.SalesDate = Result[11];
                //伝票番号
                SetLine.SlipNumber = !string.IsNullOrWhiteSpace(Result[12]) ? Strings.StrConv(Result[12], VbStrConv.Narrow, 0x0411).ToUpper() : "";
                //販売店舗
                SetLine.SalesDepartmentName = Result[13];
                //販売先
                SetLine.CustomerName = Result[14];
                //車輛本体価格
                SetLine.SalesPrice = Result[15];
                //値引
                SetLine.DiscountAmount = Result[16].Replace("(", "-").Replace(")","");
                //付属品
                SetLine.ShopOptionAmount = Result[17];
                //諸費用
                SetLine.SalesCostTotalAmount = Result[18];
                //売上総合計
                SetLine.SalesTotalAmount = Result[19];
                //売上原価
                SetLine.SalesCostAmount = Result[20];
                //粗利
                SetLine.SalesProfits = Result[21].Replace("(", "-").Replace(")", "");
                //自社登録
                SetLine.SelfRegistration = Result[22];
                //他勘定振替
                SetLine.ReductionTotal = Result[23];
                //月末在庫
                SetLine.EndInventory = Result[24];
                //新中区分
                SetLine.NewUsedType = "N";
            }
            //--------------------------------
            //中古車
            //--------------------------------
            else
            {
                //仕入区分名
                SetLine.CarPurchaseTypeName = Result[8];
                //仕入先名
                SetLine.SupplierName = Result[9];
                //月初在庫
                SetLine.BeginningInventory = Result[10];
                //当月仕入
                SetLine.MonthPurchase = Result[11].Replace("(", "-").Replace(")", "");
                //他勘定受入
                SetLine.OtherAccount = Result[12];
                //リサイクル料
                SetLine.RecycleAmount = Result[13];
                //納車日
                SetLine.SalesDate = Result[14];
                //伝票番号
                SetLine.SlipNumber = !string.IsNullOrWhiteSpace(Result[15]) ? Strings.StrConv(Result[15], VbStrConv.Narrow, 0x0411).ToUpper() : "";
                //販売店舗
                SetLine.SalesDepartmentName = Result[16];
                //販売先区分
                SetLine.CustomerTypeName = Result[17];
                //販売先
                SetLine.CustomerName = Result[18];
                //車輛本体価格
                SetLine.SalesPrice = Result[19];
                //値引
                SetLine.DiscountAmount = Result[20].Replace("(", "-").Replace(")", ""); ;
                //付属品
                SetLine.ShopOptionAmount = Result[21];
                //諸費用
                SetLine.SalesCostTotalAmount = Result[22];
                //売上総合計
                SetLine.SalesTotalAmount = Result[23];
                //売上原価
                SetLine.SalesCostAmount = Result[24];
                //粗利
                SetLine.SalesProfits = Result[25].Replace("(", "-").Replace(")", "");
                //他勘定振替
                SetLine.ReductionTotal = Result[26];
                //仕入キャンセル
                SetLine.CancelPurchase = Result[27];
                //月末在庫
                SetLine.EndInventory = Result[28];
                //新中区分
                SetLine.NewUsedType = "U";
            }

            ImportList.Add(SetLine);

            return ImportList;
        }


        /// <summary>
        /// 読み込んだデータをDBに登録
        /// </summary>
        /// <returns>戻り値(0:正常 1:エラー(取込画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 新規作成
        /// </history>
        private void DBExecuteForCarStock(List<CarStockForExcelImport> ImportList, FormCollection form)
        {
            CarStock carstock = new CarStock();

            //bool newrecflag = false;

            double TimeOutMinutes = CommonUtils.GetTimeOutMinutes();

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(TimeOutMinutes)))
            {
                List<CarStock> csList = new CarStockDao(db).GetByProcessDate(DateTime.Parse(ImportList[0].ProcessDate));

                //対象の車両が存在する場合は
                if (csList != null && csList.Count > 0)
                {
                    db.CarStock.DeleteAllOnSubmit(csList);
                }


                //車両タマ表の更新
                foreach (var LineData in ImportList)
                {
                    //carstock = csList.Where(x => x.SalesCarNumber.Equals(LineData.SalesCarNumber)).FirstOrDefault();

                    List<c_NewUsedType> nulist = new CodeDao(db).GetNewUsedTypeAll(false);

                    carstock = new CarStock();
                    
                    //対象年月
                    carstock.ProcessDate = DateTime.Parse(LineData.ProcessDate);
                    //管理番号
                    carstock.SalesCarNumber = LineData.SalesCarNumber;
                    //作成者
                    carstock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    //作成日
                    carstock.CreateDate = DateTime.Now;
                    //削除フラグ
                    carstock.DelFlag = "0";
                    //取扱ブランド
                    carstock.BrandStore = LineData.BrandStore;
                    //入庫日
                    carstock.PurchaseDate = DateTime.Parse(LineData.PurchaseDate);
                    //メーカー名
                    carstock.MakerName = LineData.MakerName;
                    //車種名
                    carstock.CarName = LineData.CarName;
                    //車台番号
                    carstock.Vin = LineData.Vin;
                    //仕入・在庫拠点
                    carstock.PurchaseLocationName = LineData.PurchaseLocationName;
                    //当月実棚
                    carstock.InventoryLocationName = LineData.InventoryLocationName;
                    //仕入先区分名
                    carstock.CarPurchaseTypeName = LineData.CarPurchaseTypeName;
                    //仕入先名
                    carstock.SupplierName = LineData.SupplierName;
                    //月初在庫
                    carstock.BeginningInventory = !string.IsNullOrWhiteSpace(LineData.BeginningInventory) ? (decimal?)(decimal.Parse(LineData.BeginningInventory)) : null;
                    //当月仕入
                    carstock.MonthPurchase = !string.IsNullOrWhiteSpace(LineData.MonthPurchase) ? (decimal?)(decimal.Parse(LineData.MonthPurchase)) : null;
                    //他勘定受入
                    carstock.OtherAccount = !string.IsNullOrWhiteSpace(LineData.OtherAccount) ? (decimal?)(decimal.Parse(LineData.OtherAccount)) : null;
                    //リサイクル料
                    carstock.RecycleAmount = !string.IsNullOrWhiteSpace(LineData.RecycleAmount) ? (decimal?)(decimal.Parse(LineData.RecycleAmount)) : null;
                    //納車日
                    carstock.SalesDate = !string.IsNullOrWhiteSpace(LineData.SalesDate) ? (DateTime?)(DateTime.Parse(LineData.SalesDate)) : null;
                    //伝票番号
                    carstock.SlipNumber = LineData.SlipNumber;
                    //販売店舗
                    carstock.SalesDepartmentName = LineData.SalesDepartmentName;
                    //販売先区分
                    carstock.CustomerTypeName = LineData.CustomerTypeName;
                    //販売先
                    carstock.CustomerName = LineData.CustomerName;
                    //車両本体
                    carstock.SalesPrice = !string.IsNullOrWhiteSpace(LineData.SalesPrice) ? (decimal?)(decimal.Parse(LineData.SalesPrice)) : null;
                    //値引
                    carstock.DiscountAmount = !string.IsNullOrWhiteSpace(LineData.DiscountAmount) ? (decimal?)(decimal.Parse(LineData.DiscountAmount)) : null;
                    //付属品
                    carstock.ShopOptionAmount = !string.IsNullOrWhiteSpace(LineData.ShopOptionAmount) ? (decimal?)(decimal.Parse(LineData.ShopOptionAmount)) : null;
                    //諸費用
                    carstock.SalesCostTotalAmount = !string.IsNullOrWhiteSpace(LineData.SalesCostTotalAmount) ? (decimal?)(decimal.Parse(LineData.SalesCostTotalAmount)) : null;
                    //売上原価
                    carstock.SalesCostAmount = !string.IsNullOrWhiteSpace(LineData.SalesCostAmount) ? (decimal?)(decimal.Parse(LineData.SalesCostAmount)) : null;
                    //売上総合計
                    carstock.SalesTotalAmount = !string.IsNullOrWhiteSpace(LineData.SalesTotalAmount) ? (decimal?)(decimal.Parse(LineData.SalesTotalAmount)) : null;
                    //粗利
                    carstock.SalesProfits = !string.IsNullOrWhiteSpace(LineData.SalesProfits) ? (decimal?)(decimal.Parse(LineData.SalesProfits)) : null;
                    //自社登録
                    carstock.SelfRegistration = !string.IsNullOrWhiteSpace(LineData.SelfRegistration) ? (decimal?)(decimal.Parse(LineData.SelfRegistration)) : null;
                    //他勘定振替
                    carstock.ReductionTotal = !string.IsNullOrWhiteSpace(LineData.ReductionTotal) ? (decimal?)(decimal.Parse(LineData.ReductionTotal)) : null;
                    //仕入キャンセル
                    carstock.CancelPurchase = !string.IsNullOrWhiteSpace(LineData.CancelPurchase) ? (decimal?)(decimal.Parse(LineData.CancelPurchase)) : null;
                    //月末在庫
                    carstock.EndInventory = !string.IsNullOrWhiteSpace(LineData.EndInventory) ? (decimal?)(decimal.Parse(LineData.EndInventory)) : null;
                    //新中区分
                    carstock.NewUsedType = LineData.NewUsedType;
                    //最終更新者
                    carstock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    //最終更新日
                    carstock.LastUpdateDate = DateTime.Now;
                    //新中区分名
                    carstock.NewUsedTypeName = nulist.Where(x => x.Code.Equals(carstock.NewUsedType)).FirstOrDefault() != null ? nulist.Where(x => x.Code.Equals(carstock.NewUsedType)).FirstOrDefault().Name : "";

                    db.CarStock.InsertOnSubmit(carstock);
                }
                    
                try
                {
                    db.SubmitChanges();
                    ts.Complete();

                    //取り込み完了のメッセージを表示する
                    ModelState.AddModelError("", "取り込みが完了しました。");
                }
                catch (SqlException se)
                {
                    //取り込み失敗のメッセージを表示する
                    ModelState.AddModelError("", "取り込みに失敗しました。システム管理者に連絡してください");
                    //実行sqlを取得
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME_EXCELUPLOAD, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    //取り込み失敗のメッセージを表示する
                    ModelState.AddModelError("", "取り込みに失敗しました。システム管理者に連絡してください");
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_EXCELUPLOAD, FORM_NAME, "");
                    ts.Dispose();
                }
            }
        }
        #endregion

        //Add
        /// <summary>
        /// 処理中かどうかを取得する。(Ajax専用）
        /// </summary>
        /// <param name="processType">処理種別</param>
        /// <returns>アイドリング状態</returns>
        /// <history>
        ///  2015/01/20 arc yano IPO対応(車両在庫) 処理中対応
        /// </history>
        public ActionResult GetProcessed(string processType)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> retParts = new Dictionary<string, string>();

                retParts.Add("ProcessedTime", "アイドリング中…");

                return Json(retParts);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 車両管理締め状況を取得する
        /// </summary>
        /// <param name="processDate">処理種別</param>
        /// <returns>アイドリング状態</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 新規作成
        /// </history>
        public ActionResult GetCloseMonthCarStock(string processDate)
        {
            if (Request.IsAjaxRequest())
            {
                CloseMonthControlCarStock rec = new CloseMonthControlCarStockDao(db).GetByKey(processDate);

                CodeData codeData = new CodeData();

                if (rec != null)
                {
                    codeData.Code = rec.CloseStatus;
                    codeData.Name = rec.CloseStatus == "002" ? "締済" : "未締";
                }
                else
                {
                    codeData.Code = "999";
                    codeData.Name = "未締";
                }

                return Json(codeData);
            }
            return new EmptyResult();
        }
    }
}