using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CrmsDao;
using Crms.Models;
using OfficeOpenXml;
using System.Configuration;
using System.Data;
using System.Xml.Linq;


//------------------------------------------------------------------------
// 機能  ：古物台帳
// 作成日：2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳)
// 更新日：
//
//------------------------------------------------------------------------
namespace Crms.Controllers
{
    /// <summary>
    /// 古物台帳検索機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class AntiqueLedgerController : Controller
    {
        private static readonly string FORM_NAME = "古物台帳";                                   // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "台帳検索";                            // 処理名（ログ出力用）
        private static readonly string PROC_NAME_LISTDOWNLOAD = "画面リスト出力";                // 処理名(Excel出力)
        private static readonly string PROC_NAME_ANTIQUELEDGERDOWNLOAD = "古物台帳出力";         // 処理名(Excel出力)

        /// <summary>
        /// データコンテキスト
        /// </summary>
        /// <history>
        /// 2017/03/16 arc yano #3726_サブシステム移行（パーツステータス)
        /// </history>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <history>
        /// 2017/03/16 arc yano #3726_サブシステム移行（パーツステータス)
        /// </history>
        public AntiqueLedgerController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 古物台帳を一覧表示する
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/03/16 arc yano #3726_サブシステム移行（パーツステータス)
        /// </history>
        public ActionResult Criteria()
        {
            FormCollection form = new FormCollection();

            //初期値の設定
            //対象年月(デフォルトは当月)
            form["TargetDateY"] = DateTime.Today.Year.ToString().Substring(1, 3);
            form["TargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');
            form["DefaultTargetDateY"] = form["TargetDateY"];
            form["DefaultTargetDateM"] = form["TargetDateM"];

            //検索対象(デフォルトは「指定無」)
            form["Searched"] = "0";
            form["DefaultSearched"] = form["Searched"];

            //デフォルト動作は「検索」
            form["RequestFlag"] = "99";

            return Criteria(form);
        }

        /// <summary>
        /// 指定月の古物台帳を表示
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) 新規作成
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            ActionResult ret = new EmptyResult();

            switch (form["RequestFlag"])
            {
                case "1": //画面リスト出力

                    ret = ListDownload(form);

                    break;

                case "2": //古物台帳出力

                    ret = AntiqueLedgerDownload(form);

                    break;

                default: //検索処理

                    List<GetAntiqueLedgerListResult> list = new List<GetAntiqueLedgerListResult>();

                    // Infoログ出力
                    OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

                    // 検索結果を取得
                    list = GetSearchResultList(form);

                    //検索結果にページ情報を付加
                    PaginatedList<GetAntiqueLedgerListResult> pageList = new PaginatedList<GetAntiqueLedgerListResult>(list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                    //画面設定
                    SetDataComponent(form);

                    ret = View("AntiqueLedgerCriteria", pageList);

                    break;
            }

            //画面設定
            SetDataComponent(form);

            return ret;
        }

        /// <summary>
        /// 古物台帳検索
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>古物台帳検索結果</returns>
        /// <history>
        /// 2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) 新規作成
        /// </history>
        private List<GetAntiqueLedgerListResult> GetSearchResultList(FormCollection form)
        {
            bool antiqueLedgerDownload = false;
            
            List<GetAntiqueLedgerListResult> list = new List<GetAntiqueLedgerListResult>();

            //データアクセス
            AntiqueLedgerDao AntiqueLedgerDao = new AntiqueLedgerDao(db);

            CodeDao dao = new CodeDao(db);

            DateTime TargetDate = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");  //年と月をつなげる       

            //古物台帳出力の場合は
            if (form["RequestFlag"].Equals("2"))
            {
                antiqueLedgerDownload = true;
            }

            //検索結果を取得する
            list = new AntiqueLedgerDao(db).GetList(TargetDate, form["Searched"].ToString(), antiqueLedgerDownload);

            return list;
        }

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) 新規作成
        /// </history>
        private void SetDataComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //対象年
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["TargetDateY"], false);
            //対象月
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["TargetDateM"], false);

            ViewData["DefaultTargetDateY"] = form["DefaultTargetDateY"];
            ViewData["DefaultTargetDateM"] = form["DefaultTargetDateM"];
            ViewData["DefaultSearched"] = form["DefaultSearched"];
            ViewData["Searched"] = form["Searched"];
        }
        #endregion

        #region Excel出力処理
        /// <summary>
        /// 画面リストのダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) 新規作成
        /// </history>
        private ActionResult ListDownload(FormCollection form)
        {
            //-------------------------------
            //初期処理
            //-------------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_LISTDOWNLOAD);

            ModelState.Clear();

            //検索結果
            List<GetAntiqueLedgerListResult> list = new List<GetAntiqueLedgerListResult>();

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            //ファイル名
            string fileName = "AntiqueList" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイルパス取得
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForAntiqueList"]) ? "" : ConfigurationManager.AppSettings["TemplateForAntiqueList"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetDataComponent(form);

                //検索結果にページ情報を付加
                PaginatedList<GetAntiqueLedgerListResult> pageList = new PaginatedList<GetAntiqueLedgerListResult>(list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                return View("AntiqueLedgerCriteria", pageList);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetDataComponent(form);

                //検索結果にページ情報を付加
                PaginatedList<GetAntiqueLedgerListResult> pageList = new PaginatedList<GetAntiqueLedgerListResult>(list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                return View("AntiqueLedgerCriteria", pageList);
            }

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName); 
        }

        /// <summary>
        /// 古物台帳のダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) 新規作成
        /// </history>
        private ActionResult AntiqueLedgerDownload(FormCollection form)
        {
            //-------------------------------
            //初期処理
            //-------------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_ANTIQUELEDGERDOWNLOAD);

            ModelState.Clear();

            //検索結果
            List<GetAntiqueLedgerListResult> list = new List<GetAntiqueLedgerListResult>();

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            //ファイル名
            string fileName = "AntiqueLedger" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイルパス取得
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForAntiqueLedger"]) ? "" : ConfigurationManager.AppSettings["TemplateForAntiqueLedger"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetDataComponent(form);

                //検索結果にページ情報を付加
                PaginatedList<GetAntiqueLedgerListResult> pageList = new PaginatedList<GetAntiqueLedgerListResult>(list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                return View("AntiqueLedgerCriteria", pageList);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetDataComponent(form);

                //検索結果にページ情報を付加
                PaginatedList<GetAntiqueLedgerListResult> pageList = new PaginatedList<GetAntiqueLedgerListResult>(list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                return View("AntiqueLedgerCriteria", pageList);
            }

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);

        }

        /// <summary>
        /// エクセルデータ作成
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="fileName">帳票名</param>
        /// <param name="tfileName">帳票テンプレート</param>
        /// <returns>エクセルデータ</returns>
        /// <history>
        /// 2018/10/25 yano #3947 車両仕入入力　入力項目（古物取引時相手の確認方法）の追加
        /// 2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) 新規作成
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

            //----------------------------
            // 設定シート取得
            //----------------------------
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //設定データを取得(config)
            if (config != null)
            {
                configLine = dExport.GetConfigLine(config, 2);
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

            //ワークシートオープン
            var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            //画面リスト出力の場合は検索条件を出力する
            if (form["RequestFlag"].Equals("1"))
            {
                //----------------------------
                // 検索条件出力
                //----------------------------
                configLine.SetPos[0] = "A1";

                //検索条件文字列を作成
                DataTable dtCondtion = MakeConditionRow(form);

                //データ設定
                ret = dExport.SetData(ref excelFile, dtCondtion, configLine);
            
            }

            //----------------------------
            // データ行出力
            //----------------------------

            //出力データ取得
            List<GetAntiqueLedgerListResult> data = GetSearchResultList(form);

            //出力位置の設定
            configLine = dExport.GetConfigLine(config, 2);

            //Mod 2017/03/07 arc yano #3731
            if (form["RequestFlag"].Equals("1"))
            {
                //データ設定
                ret = dExport.SetData<GetAntiqueLedgerListResult, GetAntiqueLedgerListResult>(ref excelFile, data, configLine);
            }
            else
            {
                List<AntiqueLedger_ExcelResult> elist = MakeExcelList(data, form);

                //データ設定
                ret = dExport.SetData<AntiqueLedger_ExcelResult, AntiqueLedger_ExcelResult>(ref excelFile, elist, configLine);
            }

            //再計算
            excelFile.Workbook.Calculate();         //Add 2018/10/25 yano #3947

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
        }

        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <param name="data">検索結果</param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) 新規作成
        /// </history>
        private DataTable MakeConditionRow(FormCollection form)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();
            String conditionText = "";

            //---------------------
            //　列定義
            //---------------------
            dt.Columns.Add("CondisionText", Type.GetType("System.String"));

            //---------------
            //データ設定
            //---------------
            DataRow row = dt.NewRow();
            CodeDao dao = new CodeDao(db);

            //対象年月の文字列の作成
            string targetMonth = dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name;

            //年月
            if (form["Searched"].Equals("0"))       //検索項目が「仕入日」
            {
                conditionText += "仕入日=" + targetMonth;
            }
            else
            {
                conditionText += "納車日=" + targetMonth;
            }

            //作成したテキストをカラムに設定
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);

            return dt;
        }

        /// <summary>
        /// ヘッダ行の作成(Excel出力用)
        /// </summary>
        /// <param name="list">列名リスト</param>
        /// <returns></returns>
        private DataTable MakeHeaderRow(IEnumerable<XElement> list)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();

            //データテーブルにxmlの値を設定する
            int i = 1;
            DataRow row = dt.NewRow();
            foreach (var header in list)
            {
                dt.Columns.Add("Column" + i, Type.GetType("System.String"));
                row["Column" + i] = header.Value;
                i++;
            }

            dt.Rows.Add(row);

            return dt;
        }

        /// <summary>
        /// 検索結果を古物台帳用に成形
        /// </summary>
        /// <param name="list">検索結果</param>
        /// <returns name="elist">検索結果(古物台帳)</returns>
        /// <history>
        ///  2018/10/25 yano #3947 車両仕入入力　入力項目（古物取引時相手の確認方法）の追加
        ///  2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) 新規作成
        /// </history>
        private List<AntiqueLedger_ExcelResult> MakeExcelList(List<GetAntiqueLedgerListResult> list, FormCollection form)
        {
            List<AntiqueLedger_ExcelResult> elist = new List<AntiqueLedger_ExcelResult>();

            elist = list.Select(x =>
                    new AntiqueLedger_ExcelResult()
                    {
                          PurchaseDate = x.PurchaseDate
                          ,
                          PurchaseStatus = x.PurchaseStatus
                          ,
                          Article = "自動車"
                          ,
                          MakerCarName = x.MakerName + x.CarName
                          ,
                          SalesCarNumber = x.SalesCarNumber
                          ,
                          Vin = x.Vin
                          ,
                          Quantity = 1
                          ,
                          Amount = (x.Amount ?? 0m)
                          ,
                          OccupationName = x.OccupationName
                          ,
                          SupplierName = x.SupplierName
                          ,
                          Birthday = x.Birthday
                          ,
                          S_PrefectureCity = x.S_Prefecture + x.S_City
                          ,
                          S_Address = x.S_Address1 + x.S_Address2
                          ,
                          SalesDate = x.SalesDate
                          ,
                          SalesTypeName = x.SalesTypeName
                          ,
                          CustomerName = x.CustomerName
                          ,
                          C_PrefectureCity = x.C_Prefecture + x.C_City
                          ,
                          C_Address = x.C_Address1 + x.C_Address2
                          ,
                          ConfirmDriverLicense = x.ConfirmDriverLicense
                          ,
                          ConfirmCertificationSeal = x.ConfirmCertificationSeal
                          ,
                          ConfirmOther = x.ConfirmOther
                    }
            ).ToList();

            return elist;
        }

        #endregion

        #region Ajax専用
        /// <summary>
        /// 処理中かどうかを取得する。(Ajax専用）
        /// </summary>
        /// <param name="processType">処理種別</param>
        /// <returns>アイドリング状態</returns>
        /// <history>
        /// 2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) 新規作成
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
        #endregion
    }
}
