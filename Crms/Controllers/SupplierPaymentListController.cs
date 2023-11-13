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
// 機能  ：外注支払一覧
// 作成日：2017/03/23 arc yano #3729 サブシステム機能移行(外注支払一覧)
// 更新日：
//
//------------------------------------------------------------------------
namespace Crms.Controllers
{
    /// <summary>
    /// 外注先支払一覧検索機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SupplierPaymentListController : Controller
    {
        private static readonly string FORM_NAME = "外注先支払一覧";                             // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "検索処理";                            // 処理名（ログ出力用）
        private static readonly string PROC_NAME_LISTDOWNLOAD = "画面リスト出力";                // 処理名(Excel出力)

        /// <summary>
        /// データコンテキスト
        /// </summary>
        /// <history>
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注支払一覧)
        /// </history>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <history>
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注支払一覧)
        /// </history>
        public SupplierPaymentListController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 外注先支払を一覧表示する
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注支払一覧)
        /// </history>
        public ActionResult Criteria()
        {
            FormCollection form = new FormCollection();

            //初期値の設定
            //検索対象(デフォルトは「納車日」)
            form["Target"] = "0";
            form["DefaultTarget"] = form["Target"];

            //対象年月(From)の設定
            form["TargetDateFrom"] = string.Format("{0:yyyy/MM/dd}", new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
            form["DefaultTargetDateFrom"] = form["TargetDateFrom"];

            //対象年月(To)の設定
            form["TargetDateTo"] = string.Format("{0:yyyy/MM/dd}", DateTime.Parse(form["TargetDateFrom"]).AddMonths(1).AddDays(-1));
            form["DefaultTargetDateTo"] = form["TargetDateTo"];

            //デフォルト動作は「検索」
            form["RequestFlag"] = "99";

            return Criteria(form);
        }

        /// <summary>
        /// 検索処理    
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注支払一覧)
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

                default: //検索処理

                    List<GetSupplierPaymentListResult> list = new List<GetSupplierPaymentListResult>();

                    // Infoログ出力
                    OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

                    // 検索結果を取得
                    list = GetSearchResultList(form);

                    //検索結果にページ情報を付加
                    PaginatedList<GetSupplierPaymentListResult> pageList = new PaginatedList<GetSupplierPaymentListResult>(list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                    ret = View("SupplierPaymentListCriteria", pageList);

                    break;
            }

            //画面設定
            SetDataComponent(form);

            return ret;
        }

        /// <summary>
        ///　外注先支払一覧先検索
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>外注先支払一覧検索結果</returns>
        /// <history>
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注先支払一覧) 新規作成
        /// </history>
        private List<GetSupplierPaymentListResult> GetSearchResultList(FormCollection form)
        {
            //-----------------------
            //検索条件の設定
            //-----------------------
            GetSupplierPaymentListCondition condition = new GetSupplierPaymentListCondition();

            //データアクセス
            SupplierPaymentListDao SupplierPaymentListDao = new SupplierPaymentListDao(db);

            //検索対象
            condition.Target = form["Target"];                      
            
            //対象年月(From)
            if (!string.IsNullOrWhiteSpace(form["TargetDateFrom"]))
            {
                condition.TargetDateFrom = DateTime.Parse(form["TargetDateFrom"]);
            }
            //対象年月(To)
            if (!string.IsNullOrWhiteSpace(form["TargetDateTo"]))
            {
                condition.TargetDateTo = DateTime.Parse(form["TargetDateTo"]);
            }
            //部門コード
            condition.DepartmentCode = form["DepartmentCode"];
            //主作業コード
            condition.ServiceWorkCode = form["ServiceWork"];
            //伝票番号
            condition.SlipNumber = form["SlipNumber"];
            //車台番号
            condition.Vin = form["Vin"];
            //顧客コード
            condition.CustomerCode = form["CustomerCode"];
            //顧客名
            condition.CustomerName = form["CustomerName"];
            //外注コード
            condition.SupplierCode = form["SupplierCode"];
            //外注名
            condition.SupplierName = form["SupplierName"];

            //検索結果
            List<GetSupplierPaymentListResult> list = new List<GetSupplierPaymentListResult>();

            //検索結果を取得する
            list = new SupplierPaymentListDao(db).GetList(condition);

            return list;
        }

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注先支払一覧) 新規作成
        /// </history>
        private void SetDataComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //検索対象
            ViewData["Target"] = form["Target"];
            //対象年月(From)
            ViewData["TargetDateFrom"] = form["TargetDateFrom"];
            //対象年月(To)
            ViewData["TargetDateTo"] = form["TargetDateTo"];            
            //部門コード
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            //部門名
            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                ViewData["DepartmentName"] = (new DepartmentDao(db).GetByKey(form["DepartmentCode"]) != null ? new DepartmentDao(db).GetByKey(form["DepartmentCode"]).DepartmentName : "");
            }
            //-------------
            //主作業
            //-------------
            ServiceWork condtion = new ServiceWork();
            condtion.DelFlag = "0";
            //主作業選択リスト
            ViewData["ServiceWorkList"] = CodeUtils.GetSelectListByModel(new ServiceWorkDao(db).GetQueryable(condtion).ToList(), "ServiceWorkCode", "Name", form["ServiceWork"],true);
            //伝票番号
            ViewData["SlipNumber"] = form["SlipNumber"];
            //車台番号
            ViewData["Vin"] = form["Vin"];

            //顧客コード
            ViewData["CustomerCode"] = form["CustomerCode"];
           
            //顧客名
            ViewData["CustomerName"] = form["CustomerName"];
            
            //外注コード
            ViewData["SupplierCode"] = form["SupplierCode"];

            //外注名
            ViewData["SupplierName"] = form["SupplierName"];

            //---------------------------
            //デフォルト値の設定
            //---------------------------
            //検索対象(デフォルトは「納車日」)
            ViewData["DefaultTarget"] = form["DefaultTarget"];

            //対象年月(From)の設定
            ViewData["DefaultTargetDateFrom"] = form["DefaultTargetDateFrom"];

            //対象年月(To)の設定
            ViewData["DefaultTargetDateTo"] = form["DefaultTargetDateTo"];

            //デフォルト動作は「検索」
            ViewData["RequestFlag"] = form["RequestFlag"];
        }
        #endregion

        #region Excel出力処理
        /// <summary>
        /// 画面リストのダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注先支払一覧) 新規作成
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
            List<GetSupplierPaymentListResult> list = new List<GetSupplierPaymentListResult>();

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            //ファイル名
            string fileName = "SupplierPaymentList" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイルパス取得
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForSupplierPaymentList"]) ? "" : ConfigurationManager.AppSettings["TemplateForSupplierPaymentList"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetDataComponent(form);

                //検索結果にページ情報を付加
                PaginatedList<GetSupplierPaymentListResult> pageList = new PaginatedList<GetSupplierPaymentListResult>(list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                return View("SupplierPaymentListCriteria", pageList);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetDataComponent(form);

                //検索結果にページ情報を付加
                PaginatedList<GetSupplierPaymentListResult> pageList = new PaginatedList<GetSupplierPaymentListResult>(list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                return View("SupplierPaymentListCriteria", pageList);
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
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注先支払一覧) 新規作成
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
            List<GetSupplierPaymentListResult> data = GetSearchResultList(form);

            //出力位置の設定
            configLine = dExport.GetConfigLine(config, 2);

            //データ設定
            ret = dExport.SetData<GetSupplierPaymentListResult, GetSupplierPaymentListResult>(ref excelFile, data, configLine);
            
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
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注先支払一覧) 新規作成
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

            //年月
            string target;

            if (form["Target"].Equals("0"))       //検索項目が「納車日」
            {
                target = "納車日=";
            }
            else
            {
                target = "受注日=";
            }

            conditionText += target + form["TargetDateFrom"] + "～" + form["TargetDateTo"];

            //部門
            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                conditionText += "：部門=" + (new DepartmentDao(db).GetByKey(form["DepartmentCode"]) != null ? new DepartmentDao(db).GetByKey(form["DepartmentCode"]).DepartmentName : "") + "(" + form["DepartmentCode"] + ")";
            }

            //主作業
            if (!string.IsNullOrWhiteSpace(form["ServiceWork"]))
            {
                conditionText += "：主作業=" + (new ServiceWorkDao(db).GetByKey(form["ServiceWork"]) != null ? new ServiceWorkDao(db).GetByKey(form["ServiceWork"]).Name : "") + "(" + form["ServiceWork"] + ")";
            }

            //伝票番号
            if (!string.IsNullOrWhiteSpace(form["SlipNumber"]))
            {
                conditionText += "：伝票番号=" + form["SlipNumber"];
            }

            //車台番号
            if (!string.IsNullOrWhiteSpace(form["Vin"]))
            {
                conditionText += "：車台番号=" + form["Vin"];
            }

            //顧客
            if (!string.IsNullOrWhiteSpace(form["CustomerCode"]))
            {
                conditionText += "：顧客=" + (new CustomerDao(db).GetByKey(form["CustomerCode"]) != null ? new CustomerDao(db).GetByKey(form["CustomerCode"]).CustomerName : "") + "(" + form["CustomerCode"] + ")";
            }

            //外注
            if (!string.IsNullOrWhiteSpace(form["SupplierCode"]))
            {
                conditionText += "：外注先=" + (new SupplierDao(db).GetByKey(form["SupplierCode"]) != null ? new SupplierDao(db).GetByKey(form["SupplierCode"]).SupplierName : "") + "(" + form["SupplierCode"] + ")"; ;
            }

            //作成したテキストをカラムに設定
            row["CondisionText"] = conditionText;

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
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注先支払一覧) 新規作成
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
