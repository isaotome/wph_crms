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
// 機能  ：パーツステータス
// 作成日：2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
// 更新日：
//
//------------------------------------------------------------------------
namespace Crms.Controllers
{
    /// <summary>
    /// パーツステータス検索機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsStatusController : Controller
    {
        private static readonly string FORM_NAME = "パーツステータス";                           // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "パーツステータス検索";                // 処理名（ログ出力用）
        private static readonly string PROC_NAME_LISTDOWNLOAD = "画面リスト出力";                // 処理名(Excel出力)

        /// <summary>
        /// データコンテキスト
        /// </summary>
        /// <history>
        /// 2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
        /// </history>
        private CrmsLinqDataContext db;

        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <history>
        /// 2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
        /// </history>
        public PartsStatusController()
        {
            db = CrmsDataContext.GetDataContext();
        }


        /// <summary>
        /// パーツステータスを一覧表示する
        /// </summary>
        /// <returns></returns>
        /// <history>
        ///2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
        /// </history>
        public ActionResult Criteria()
        {
            criteriaInit = true;

            FormCollection form = new FormCollection();

            //初期値の設定
            //対象年月(デフォルトは当月)
            form["TargetDateY"] = DateTime.Today.Year.ToString().Substring(1, 3);
            form["TargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');
            form["DefaultTargetDateY"] = form["TargetDateY"];
            form["DefaultTargetDateM"] = form["TargetDateM"];

            //伝票ステータス
            form["ServiceOrderStatus"] = "002";                                 //Add 2017/05/08 arc yano #3726
            form["DefaultServiceOrderStatus"] = form["ServiceOrderStatus"];     //Add 2017/05/08 arc yano #3726

            //検索対象(デフォルトは「指定無」)
            form["Target"] = "0";
            form["DefaultTarget"] = form["Target"];

            //デフォルト動作は「検索」
            form["RequestFlag"] = "99";

            return Criteria(form);
        }

        /// <summary>
        /// 指定月のパーツステータスを表示
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            ActionResult ret = new EmptyResult();

            ModelState.Clear();


            //初回表示の場合
            if (criteriaInit)
            {
                PaginatedList<GetPartsStatusResult> list = new PaginatedList<GetPartsStatusResult>();
                ret = View("PartsStatusCriteria", list);
            }
            else
            {
                switch (form["RequestFlag"])
                {
                    case "1": //画面リスト出力

                        ret = ListDownload(form);

                        break;

                    default: //検索処理

                        // Infoログ出力
                        OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

                        List<GetPartsStatusResult> list = new List<GetPartsStatusResult>();

                        // 検索結果を取得
                        list = GetSearchResultList(form);

                        //検索結果にページ情報を付加
                        PaginatedList<GetPartsStatusResult> pageList = new PaginatedList<GetPartsStatusResult>(list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                        //画面設定
                        SetDataComponent(form);

                        ret = View("PartsStatusCriteria", pageList);

                        break;
                }
            }

            //画面設定
            SetDataComponent(form);

            return ret;
        }

        /// <summary>
        /// パーツステータス検索
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>パーツステータス検索結果</returns>
        /// <history>
        /// 2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
        /// </history>
        private List<GetPartsStatusResult> GetSearchResultList(FormCollection form)
        {
            List<GetPartsStatusResult> list = new List<GetPartsStatusResult>();

            //-------------------------
            //検索条件の設定
            //-------------------------
            PartsStatusCondition condition = new PartsStatusCondition();

            //検索対象
            condition.Target = form["Target"];

            //対象年月
            CodeDao dao = new CodeDao(db);
            condition.TargetDateFrom = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");  //年と月をつなげる  

            //部門
            condition.DepartmentCode = form["DepartmentCode"];

            //伝票ステータス
            condition.ServiceOrderStatus = form["ServiceOrderStatus"];

            //部品番号
            condition.PartsNumber = form["PartsNumber"];

            //検索結果を取得する
            list = new PartsStatusDao(db).GetListByCondition(condition);

            return list;
        }

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
        /// </history>
        private void SetDataComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //検索対象
            ViewData["Target"] = form["Target"];

            //対象年
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["TargetDateY"], false);
            //対象月
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["TargetDateM"], false);

            //部門
            ViewData["DepartmentCode"] = form["DepartmentCode"];

            //------------------
            //部門名
            //------------------
            string departmentName = "";

            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

                if (dep != null)
                {
                    departmentName = dep.DepartmentName;
                }
            }

            ViewData["DepartmentName"] = departmentName;

            //伝票ステータス
            ViewData["ServiceOrderStatusList"] = CodeUtils.GetSelectListByModel(dao.GetServiceOrderStatusAll(false), form["ServiceOrderStatus"], true);
            
            //部品番号
            ViewData["PartsNumber"] = form["PartsNumber"];

            //------------------
            //部品名
            //------------------
            string partsNameJp = "";

            if (!string.IsNullOrWhiteSpace(form["PartsNumber"]))
            {
                Parts parts = new PartsDao(db).GetByKey(form["PartsNumber"]);

                if (parts != null)
                {
                    partsNameJp = parts.MakerPartsNameJp;
                }
            }
            ViewData["PartsNameJp"] = form["partsNameJp"];

            //デフォルト値の設定
            ViewData["DefaultTargetDateY"] = form["DefaultTargetDateY"];
            ViewData["DefaultTargetDateM"] = form["DefaultTargetDateM"];
            ViewData["DefaultTarget"] = form["DefaultTarget"];

            ViewData["DefaultServiceOrderStatus"] = form["DefaultServiceOrderStatus"];      //Add 2017/05/08 arc yano #3726        
        }
        #endregion

        #region Excel出力処理
        /// <summary>
        /// 画面リストのダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
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
            List<GetPartsStatusResult> list = new List<GetPartsStatusResult>();

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            //ファイル名
            string fileName = "PartsStatus" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイルパス取得
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForPartsStatus"]) ? "" : ConfigurationManager.AppSettings["TemplateForPartsStatus"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetDataComponent(form);

                //検索結果にページ情報を付加
                PaginatedList<GetPartsStatusResult> pageList = new PaginatedList<GetPartsStatusResult>(list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                return View("PartsStatusCriteria", pageList);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetDataComponent(form);

                //検索結果にページ情報を付加
                PaginatedList<GetPartsStatusResult> pageList = new PaginatedList<GetPartsStatusResult>(list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                return View("PartsStatusCriteria", pageList);
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
        /// 2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
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
            List<GetPartsStatusResult> data = GetSearchResultList(form);

            //出力位置の設定
            configLine = dExport.GetConfigLine(config, 2);

            
            //データ設定
            ret = dExport.SetData<GetPartsStatusResult, PartsStatus_ExcelResult>(ref excelFile, data, configLine);
            

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
        /// 2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
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
            if (form["Target"].Equals("1"))         //検索項目が「入庫日」
            {
                conditionText += "入庫日=" + targetMonth + " ";
            }
            else if (form["Target"].Equals("2"))    //検索項目が「受注日」
            {
                conditionText += "受注日=" + targetMonth + " ";
            }
            else if (form["Target"].Equals("3"))    //検索項目が「作業開始日」
            {
                conditionText += "作業開始日=" + targetMonth + " ";
            }
            else if (form["Target"].Equals("4"))    //検索項目が「作業終了日」
            {
                conditionText += "作業終了日=" + targetMonth + " ";
            }
            else if (form["Target"].Equals("5"))    //検索項目が「納車日」
            {
                conditionText += "納車日=" + targetMonth + " ";
            }

            //------------------
            //部門
            //------------------
            string departmentName = "";

            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

                if (dep != null)
                {
                    departmentName = dep.DepartmentName;
                }

                conditionText += "部門=" + departmentName + "(" + form["DepartmentCode"] + ")" + "　";
            }

            //伝票ステータス
            if (!string.IsNullOrWhiteSpace(form["ServiceOrderStatus"]))
            {
                conditionText += "伝票ステータス=" + new CodeDao(db).GetServiceOrderStatusByKey(form["ServiceOrderStatus"]).Name + " ";
            }

            //------------------
            //部品名
            //------------------
            string partsNameJp = "";

            if (!string.IsNullOrWhiteSpace(form["PartsNumber"]))
            {
                Parts parts = new PartsDao(db).GetByKey(form["PartsNumber"]);

                if (parts != null)
                {
                    partsNameJp = parts.MakerPartsNameJp;
                }

                conditionText += "部品=" + partsNameJp + "(" + form["PartsNumber"] + ")" + "　";
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
