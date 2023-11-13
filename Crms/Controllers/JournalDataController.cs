using Crms.Models;
using CrmsDao;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Style.XmlAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Crms.Controllers
{
    /// <summary>
    /// 入金実績情報コントローラ
    /// </summary>
    public class JournalDataController : Controller
    {

        #region 定数
        private static readonly string FORM_NAME = "入金実績情報抽出";                   // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "入金実績情報検索";           // 処理名（ログ出力用）　
        private static readonly string PROC_NAME_EXCEL = "入金実績情報Excel出力";       // 処理名(CSV出力)
        #endregion

        #region 初期化
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        private bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public JournalDataController()
        {
            this.db = new CrmsLinqDataContext();

            //タイムアウト値の設定
            db.CommandTimeout = 600;
        }
        #endregion

        #region 検索画面
        /// <summary>
        /// 初期画面表示
        /// </summary>
        /// <returns>検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            return Criteria(form);
        }
        
        /// <summary>
        /// 検索、Excel出力処理
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>検索結果、またはExcel</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            //検索結果初期化
            PaginatedList<GetJournalDataResult> list = new PaginatedList<GetJournalDataResult>();

            //デフォルトの戻り値を設定
            ActionResult ret = View("JournalDataCriteria", list);

            if (!criteriaInit) {
                //ReuquestFlagによる処理の振分け
                switch (form["RequestFlag"])
                {
                    case "1":   //Excel出力
                        ret = Download(form);
                        break;

                    default:
                        //検索またはページング
                        //検索処理実行
                        ret = SearchList(form);
                        break;
                }                                       
            }

            return ret;
        }

        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        [AuthFilter]
        private ActionResult SearchList(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            ModelState.Clear();

            //検索結果初期化
            PaginatedList<GetJournalDataResult> list = new PaginatedList<GetJournalDataResult>();

            //検索処理(ページング付)
            list = new PaginatedList<GetJournalDataResult>(GetSearchResultList(form).AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            
            //画面設定
            SetComponent(form);

            return View("JournalDataCriteria", list);
        }
        
        /// <summary>
        /// 検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>検索結果リスト</returns>
        private List<GetJournalDataResult> GetSearchResultList(FormCollection form)
        {

            var ret = db.GetJournalData(form["TargetDateFrom"], form["TargetDateFromTo"]).ToList();

            return ret;
        }
        /// <summary>
        /// 画面項目の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        public void SetComponent(FormCollection form)
        {
            ViewData["TargetDateFrom"] = form["TargetDateFrom"];
            ViewData["TargetDateFromTo"] = form["TargetDateFromTo"];
        }
        #endregion

        #region Excelボタン押下
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Download(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCEL);

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            string DownLoadTime = string.Format("{0:yyyyMMddHHmmss}", System.DateTime.Now);
            //ファイル名(yyyyMMdd(ダウンロード年月日)_顧客データリスト)
            string fileName = "JournalData_" + DownLoadTime + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName);

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);

        }
        #endregion

        #region フィールド定義データから出力フィールドリストを取得する
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

        #endregion

        #region エクセルデータ作成(テンプレートファイルなし)
        /// <summary>
        /// エクセルデータ作成(テンプレートファイルなし)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="fileName">帳票名</param>
        /// <returns>エクセルデータ</returns>
        private byte[] MakeExcelData(FormCollection form, string fileName)
        {

            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                  //設定値
            byte[] excelData = null;                //エクセルデータ
            string sheetName;                       //シート名
            int dateType = 0;                       //データタイプ(帳票形式)
            string setPos = "A1";                   //設定位置
            bool ret = false;


            sheetName = (string.IsNullOrWhiteSpace(form["TargetDateFrom"]) ? "" : form["TargetDateFrom"].Replace("/", "")) + "～" + (string.IsNullOrWhiteSpace(form["TargetDateFromTo"]) ? "" : form["TargetDateFromTo"].Replace("/", ""));

            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルなし)
            ExcelPackage excelFile = dExport.MakeExcel(fileName);

            //ワークシート追加
            var worksheet = excelFile.Workbook.Worksheets.Add(sheetName);

            //----------------------------
            // 検索条件出力
            //----------------------------
            //設定値
            configLine = dExport.GetDefaultConfigLine(0, sheetName, dateType, setPos);

            //検索条件文字列を作成
            DataTable dtCondtion = MakeConditionRow(form);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // ヘッダ行出力
            //----------------------------
            //出力位置の設定
            setPos = "A2";

            //設定値
            configLine = dExport.GetDefaultConfigLine(0, sheetName, dateType, setPos);

            //ヘッダ行の取得
            IEnumerable<XElement> data = GetFieldList("JournalDataText");

            //取得したヘッダ行リストをデータテーブルに設定
            DataTable dtHeader = MakeHeaderRow(data);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtHeader, configLine);

            //----------------------------
            // データ行出力
            //----------------------------
            //出力位置の設定
            setPos = "A3";

            //設定値を取得
            configLine = dExport.GetDefaultConfigLine(0, sheetName, dateType, setPos);
           
            //棚卸情報の取得
            List<GetJournalDataResult> list = GetSearchResultList(form).ToList();

            //入金日、納車日列のフォーマットを設定
            //幅調整
            worksheet.Column(1).Width = 11;
            worksheet.Column(2).Width = 12;
            worksheet.Column(3).Width = 21;
            worksheet.Column(4).Width = 14;
            worksheet.Column(5).Width = 65;
            worksheet.Column(6).Width = 10;
            worksheet.Column(7).Width = 16;
            worksheet.Column(8).Width = 11;
            worksheet.Column(9).Width = 12;
            worksheet.Column(10).Width = 65;
            worksheet.Column(11).Width = 9;
            worksheet.Column(12).Width = 10;
            worksheet.Column(13).Width = 53;
            worksheet.Column(14).Width = 14;
            worksheet.Column(15).Width = 30;


            for (int i = 0; i < list.Count; i++)
            {
                worksheet.Cells[3 + i, 1].Style.Numberformat.Format = "yyyy/MM/dd";       //入金日
                worksheet.Cells[3 + i, 8].Style.Numberformat.Format = "yyyy/MM/dd";       //納車日
            }

            //データ設定
            ret = dExport.SetData<GetJournalDataResult, GetJournalDataResult>(ref excelFile, list, configLine);
            //戻り値チェック

            excelData = excelFile.GetAsByteArray();

            return excelData;
        }
        #endregion

        
        #region 検索条件文作成(Excel出力用)
        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="form">検索条件</param>
        /// <returns></returns>
        private DataTable MakeConditionRow(FormCollection form)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();
            String conditionText = "";

            //---------------------
            //　列定義
            //---------------------
            //１つの列を設定  
            dt.Columns.Add("CondisionText", Type.GetType("System.String"));

            //---------------
            //データ設定
            //---------------
            DataRow row = dt.NewRow();

            conditionText += "入金日=";

            if (string.IsNullOrWhiteSpace(form["TargetDateFrom"]) && string.IsNullOrWhiteSpace(form["TargetDateFromTo"]))
            {
                conditionText += "指定なし";
            }
            else
            {
                //対象年月from指定あり
                if (!string.IsNullOrWhiteSpace(form["TargetDateFrom"]))
                {
                    conditionText += form["TargetDateFrom"];
                }
                //対象年月To指定あり
                if (!string.IsNullOrWhiteSpace(form["TargetDateFromTo"]))
                {
                    conditionText += string.Format("～{0}", form["TargetDateFromTo"]);
                }
            }
            
            //作成したテキストをカラムに設定
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);

            return dt;
        }
        #endregion

        #region ヘッダ行の作成(Excel出力用)
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

        #region Ajax処理
        /// <summary>
        /// 処理中かどうかを取得する。(Ajax専用）
        /// </summary>
        /// <param name="processType">処理種別</param>
        /// <returns>アイドリング状態</returns>
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
