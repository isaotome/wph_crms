using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CrmsDao;
using Crms.Models;
using OfficeOpenXml;
using System.Configuration;


namespace Crms.Controllers
{

    ///<summary>
    /// ワランティ作業納品書発行コントローラクラス
    /// </summary>
    /// <history>
    /// 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 新規作成
    /// </history>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceSalesSlipController : Controller
    {

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
        public ServiceSalesSlipController()
        {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 定数定義
        /// </summary>
        private static readonly string FORM_NAME = "サービス伝票売上発行";         // 画面名
        private static readonly string PROC_NAME_SEARCH = "検索";                  // 処理名（ログ出力用）
        private static readonly string PROC_NAME_EXCELDOWNLOAD = "Excel出力";      // 処理名(Excel出力)

        private static readonly int EXCEL_LINECNT = 62;                            // Excel1ページ辺りの行数

        /// <summary>
        /// サービス売上伝票発行
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 新規作成
        /// </history>
        public ActionResult Criteria()
        {
            //-----------------------
            //初期値の設定
            //-----------------------
            criteriaInit = true;                            //初期表示フラグON　
            FormCollection form = new FormCollection();     //フォーム生成

            //処理種別
            form["RequestFlag"] = "";

            form["TargetDateY"] = DateTime.Today.Year.ToString().Substring(1, 3);
            form["TargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');
            form["DefaultTargetDateY"] = form["TargetDateY"];
            form["DefaultTargetDateM"] = form["TargetDateM"];
         
            //表示ページ数
            form["id"] = "0";

            return Criteria(form);
        }

        /// <summary>
        /// サービス売上伝票処理
        /// </summary>
        /// <param name="form">フォーム値</param>
        /// <returns></returns>
        /// <history>
        /// 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 新規作成
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            ActionResult ret;

            db = new CrmsLinqDataContext();

            //タイムアウト値の設定
            db.CommandTimeout = 600;

            db.Log = new OutputWriter();

            //--------------------------------------
            //ReuquestFlagによる処理の振分け
            //--------------------------------------
            switch (form["RequestFlag"])
            {
                case "1":   //Excel出力
                    ret = Download(form);
                    break;
     
                default:    //検索またはページング

                    //検索処理実行
                    ret = SearchList(form);
                    break;
            }

            return ret;
        }

        #region  検索処理
        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 新規作成
        /// </history>
        [AuthFilter]
        private ActionResult SearchList(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            ModelState.Clear();

            //検索結果初期化
            PaginatedList<GetServiceSalesSlipResult> list = new PaginatedList<GetServiceSalesSlipResult>();

            // 検索結果リストの取得
            if (criteriaInit == true)    //初期表示時は表示しない
            {
                //何もしない
            }
            else
            {
                //検索処理
                list = new PaginatedList<GetServiceSalesSlipResult>(GetSearchResultList(form), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            }

            //画面設定
            SetComponent(form);

            return View("ServiceSalesSlipCriteria", list);
        }

        /// <summary>
        /// 検索実行
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>検索結果一覧</returns>
        /// <history>
        /// 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 新規作成
        /// </history>
        private IQueryable<GetServiceSalesSlipResult> GetSearchResultList(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //---------------------
            //　検索項目の設定
            //---------------------
            GetServiceSalesSlipResult condition = SetCondition(form);

            //検索実行
            return new ServiceSalesOrderDao(db).GetServiceSalesSlip(condition);
        }

        /// <summary>
        /// 検索条件設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>検索条件</returns>
        /// <history>
        /// 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 新規作成
        /// </history>
        private GetServiceSalesSlipResult SetCondition(FormCollection form)
        {
            //---------------------
            //　検索項目の設定
            //---------------------
            GetServiceSalesSlipResult condition = new GetServiceSalesSlipResult();

            //伝票番号
            condition.SlipNumber = form["SlipNumber"];
            
            //入庫日の設定
            CodeDao dao = new CodeDao(db);

            //入庫日(From)
            DateTime targetDate = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");

            condition.ArrivalPlanDateFrom = string.Format("{0:yyyy/MM/dd}", targetDate);

            //入庫日(To)
            //当月の月末を設定
            targetDate = targetDate.AddMonths(1);

            condition.ArrivalPlanDateTo = string.Format("{0:yyyy/MM/dd}", targetDate);

            //部門コード
            condition.DepartmentCode = form["DepartmentCode"];

            //伝票ステータス
            condition.ServiceOrderStatus = form["ServiceOrderStatus"];

            //主作業
            condition.ServiceWorkCode = form["ServiceWorkCode"];

            //顧客コード
            condition.CustomerCode = form["CustomerCode"];

            //顧客名
            condition.CustomerName = form["CustomerName"];

            return condition;
        }
        #endregion
        
 
        #region 帳票出力
        /// <summary>
        /// Excelファイルのダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 新規作成
        /// </history>
        private ActionResult Download(FormCollection form)
        {
            //-------------------------------
            //初期処理
            //-------------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCELDOWNLOAD);

            ModelState.Clear();

            //検索結果の設定
            PaginatedList<GetServiceSalesSlipResult> list = new PaginatedList<GetServiceSalesSlipResult>();

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            //ファイル名(請求明細書_xxxxxxxx(伝票番号)_x(リビジョン番号)_xxxxx(主作業コード)_yyyyMMddHHmmss)
            string fileName = "請求明細書" + "_" + form["ExcelDownloadSlipNumber"] + "_" + form["ExcelDownloadRevisionNumber"] + "_" + form["ExcelDownloadServiceWorkCode"] + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイルパス取得
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForServiceSalesSlip"]) ? "" : ConfigurationManager.AppSettings["TemplateForServiceSalesSlip"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");

                //検索処理
                list = new PaginatedList<GetServiceSalesSlipResult>(GetSearchResultList(form), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                SetComponent(form);

                return View("ServiceSalesSlipCriteria", list);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetComponent(form);
                return View("ServiceSalesSlipCriteria", list);
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
        /// 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 新規作成
        /// </history>
        protected byte[] MakeExcelData(FormCollection form, string fileName, string tfileName)
        {
            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine hconfigLine = null;            //設定値(ヘッダ、フッタ)
            ConfigLine lconfigLine = null;            //設定値(明細)

            byte[] excelData = null;                  //エクセルデータ
            bool ret = false;
            bool tFileExists = true;                  //テンプレートファイルあり／なし(実際にあるかどうか)


            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルあり)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //テンプレートファイルが無かった場合
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "テンプレートファイルが見つかりませんでした。");
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

            //----------------------------
            // 設定値の取得
            //----------------------------
            int columnLine = 2;             //列の位置

            //設定ファイル読込
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //----------------------------------
            //ヘッダ、フッタの設定値の取得
            //----------------------------------
            if (config != null)
            {
                hconfigLine = dExport.GetConfigLine(config, columnLine);
                lconfigLine = dExport.GetConfigLine(config, ++columnLine);
            }
            if(config == null) //configシートが無い場合はエラー
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

            //----------------------------
            // 設定するデータの取得
            //----------------------------
            
            //DBからサービス伝票の情報を取得
            ServiceSalesHeader header = new ServiceSalesOrderDao(db).GetBySlipNumber(form["ExcelDownloadSlipNumber"]);

            List<ServiceSalesSlipHeader> headerList = SetSeviceSalesSlipList(header, form["ExcelDownloadServiceWorkCode"]);

            int pageIndex = 0;

            //------------------------------
            //データ設定
            //------------------------------
            foreach (var h in headerList)
            {
                //設定位置の変更
                if (pageIndex > 0)
                {
                    //ヘッダ情報
                    List<Tuple<int, int>> hList = new List<Tuple<int,int>>();

                    for(int i=0; i< hconfigLine.SetPosRowCol.Count ; i++)
                    {
                        int rowPos = hconfigLine.SetPosRowCol[i].Item1 + EXCEL_LINECNT;
                        int colPos = hconfigLine.SetPosRowCol[i].Item2;

                        Tuple<int, int> pos = new Tuple<int, int>(rowPos, colPos);

                        hList.Add(pos);
                    }

                    hconfigLine.SetPosRowCol = hList;

                    //明細情報
                    List<Tuple<int, int>> lList = new List<Tuple<int, int>>();

                    for (int i = 0; i < lconfigLine.SetPosRowCol.Count; i++)
                    {
                        int rowPos = lconfigLine.SetPosRowCol[i].Item1 + EXCEL_LINECNT;
                        int colPos = lconfigLine.SetPosRowCol[i].Item2;

                        Tuple<int, int> pos = new Tuple<int, int>(rowPos, colPos);

                        lList.Add(pos);
                    }

                    lconfigLine.SetPosRowCol = lList;
                }

                List<ServiceSalesSlipHeader> slipList = new List<ServiceSalesSlipHeader>();
                slipList.Add(h);

                //-------------------------
                //ヘッダー情報の設定
                //-------------------------
                //データ設定
                ret = dExport.SetData<ServiceSalesSlipHeader, ServiceSalesSlipHeader>(ref excelFile, slipList, hconfigLine);
                
                //---------------------------
                //明細設定値の取得
                //---------------------------
                //データ設定
                ret = dExport.SetData<ServiceSalesSlipLine, ServiceSalesSlipLine>(ref excelFile, h.list, lconfigLine);

                pageIndex++;
            }

            //-------------------------
            //印刷範囲の設定
            //-------------------------
            ExcelWorksheet worksheet = excelFile.Workbook.Worksheets[hconfigLine.SheetName];

            worksheet.PrinterSettings.PrintArea = worksheet.Cells["$B$1:$BQ$" + (pageIndex) * EXCEL_LINECNT]; // 印刷範囲(全体)
            worksheet.View.PageBreakView = true; // 改ページプレビュー
            worksheet.View.ZoomScale = 100; // ズーム
            worksheet.PrinterSettings.FitToPage = true; // 印刷設定：すべての列を１ページに印刷
            worksheet.PrinterSettings.PaperSize = ePaperSize.A4; // 用紙サイズ
            worksheet.PrinterSettings.FitToHeight = 0; // 用紙内のサイズ調整(縦)
            worksheet.PrinterSettings.FitToWidth = 1; // 用紙内のサイズ調整(横)

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
        /// Excelのヘッダー、フッター部分の設定
        /// </summary>
        /// <param name="header">ヘッダ情報</param>
        /// <returns name ="eHeader"></returns>
        /// <history>
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
        /// 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 新規作成
        /// </history>
        private List<ServiceSalesSlipHeader> SetSeviceSalesSlipList(ServiceSalesHeader header, string serviceWorkCode)
        {
            //ヘッダ情報
            List<ServiceSalesSlipHeader> headerList= new List<ServiceSalesSlipHeader>();

            ServiceSalesSlipHeader spHeader = new ServiceSalesSlipHeader();
            
            //明細情報
            List<ServiceSalesSlipLine> lineList = new List<ServiceSalesSlipLine>();


            //その伝票の入金実績を取得する
            List<Journal> journalList = new JournalDao(db).GetJournalCalcListBySlipNumber(header.SlipNumber);

            //-------------------------
            //ヘッダ情報の設定
            //-------------------------
            // 郵便番号
            spHeader.PostCode = header.Customer != null ? header.Customer.PostCode : "";
            // 都道府県
            spHeader.Prefecture = header.Customer != null ? header.Customer.Prefecture : "";
            // 住所１
            spHeader.Address1 = header.Customer != null ? header.Customer.Address1 : "";
            // 住所２
            spHeader.Address2 = header.Customer != null ? header.Customer.Address2 : "";
            // 顧客名
            spHeader.CustomerName = header.Customer != null ? (header.Customer.CustomerName + " 様") : "";
            // 電話番号
            spHeader.TelNumber = header.Customer != null ? header.Customer.TelNumber : "";
            // 携帯電話番号
            spHeader.MobileNumber = header.Customer != null ? header.Customer.MobileNumber : "";
            // 部門名称
            spHeader.DepartmentFullName = header.Department != null ? header.Department.FullName : "";
            // 郵便番号（部門）
            spHeader.D_PostCode = header.Department != null ? header.Department.PostCode : "";
            // 住所(部門) 都道府県＋住所１＋住所２（部門）
            spHeader.D_Adress = header.Department != null ? (header.Department.Prefecture + header.Department.Address1 + header.Department.Address2) : "";
            // 顧客コード
            spHeader.CustomerCode = header.CustomerCode;
            // 売上日(=納車日)
            spHeader.SalesDate = header.SalesDate;
            // 入庫日
            spHeader.ArrivalPlanDate = header.ArrivalPlanDate;
            // 出庫日(=納車日)
            spHeader.SalesDate2 = header.SalesDate;
            // 伝票番号＋リビジョン番号
            spHeader.SlipRevNumber = (header.SlipNumber + "_" + header.RevisionNumber);
            // ページ番号
            spHeader.PageCnt = 1;
            // 登録番号
            spHeader.RegistNumber = (header.MorterViecleOfficialCode ?? "") + (header.RegistrationNumberType ?? "") + (header.RegistrationNumberKana ?? "") + (header.RegistrationNumberPlate ?? "");
            // 車種名
            spHeader.CarName = (string.IsNullOrWhiteSpace(header.CarBrandName) ? "" : header.CarBrandName) + " " + (string.IsNullOrWhiteSpace(header.CarName) ? "" : header.CarName);
            // 担当者名
            spHeader.EmployeeName = header.FrontEmployee != null ? header.FrontEmployee.EmployeeName : "";
            // 原動機型式
            spHeader.EngineType = header.EngineType;
            // 車台番号
            spHeader.Vin = header.Vin;
            // 型式
            spHeader.ModelName = header.ModelName;
            // 走行距離
            spHeader.Mileage = header.Mileage;
            // 初年度登録
            spHeader.FirstRegistration = header.FirstRegistration;
            // 次回点検日
            spHeader.NextInspectionDate = header.NextInspectionDate;
            
            //自動車税
            if (header.CarTax != null)
            {
                spHeader.CarTaxTitle = "自動車税種別割";   //Mod 2019/09/04 yano #4011
                spHeader.CarTax = header.CarTax;
            }
            //自賠責保険料
            if (header.CarLiabilityInsurance != null)
            {
                spHeader.CarLiabilityInsuranceTitle = "自賠責保険料";
                spHeader.CarLiabilityInsurance = header.CarLiabilityInsurance;
            }
            //自動車重量税
            if (header.CarLiabilityInsurance != null)
            {
                spHeader.CarWeightTaxTitle = "自動車重量税";
                spHeader.CarWeightTax = header.CarWeightTax;
            }
            //印紙代
            if (header.FiscalStampCost != null)
            {
                spHeader.FiscalStampCostTitle = "印紙代";
                spHeader.FiscalStampCost = header.FiscalStampCost;
            }
            //ナンバー代
            if (header.NumberPlateCost != null)
            {
                spHeader.NumberPlateCostTitle = "ナンバー代";
                spHeader.NumberPlateCost = header.NumberPlateCost;
            }

            /* 現状は出力しない
            // 工賃合計
            spHeader.EngineerTotalAmount = header.EngineerTotalAmount;
            // 部品合計
            spHeader.PartsTotalAmount = header.PartsTotalAmount;
            // 整備料合計
            spHeader.SubTotalAmount = header.SubTotalAmount;
            // 消費税合計
            spHeader.TotalTaxAmount = header.TotalTaxAmount;
            // 諸費用合計
            spHeader.CostTotalAmount = header.CostTotalAmount;
            // 入金合計
            spHeader.TotalInAmount = journalList != null ? journalList.Sum(x => x.Amount) : 0m;
            // 請求残
            spHeader.TotalBalance = (header.GrandTotalAmount ?? 0m) - spHeader.TotalInAmount;
            */

            // 工賃合計
            spHeader.EngineerTotalAmount = null;
            // 部品合計
            spHeader.PartsTotalAmount = null;
            // 整備料合計
            spHeader.SubTotalAmount = null;
            // 消費税合計
            spHeader.TotalTaxAmount = null;
            // 諸費用合計
            spHeader.CostTotalAmount = null;
            // 請求残
            spHeader.TotalBalance = null;
            // 入金合計
            spHeader.TotalInAmount = null;

            //--------------------------------
            //明細情報の設定
            //---------------------------------
            List<ServiceSalesLine> lList = header.ServiceSalesLine.Where(x => !string.IsNullOrWhiteSpace(x.ServiceWorkCode) && x.ServiceWorkCode.Equals(serviceWorkCode)).OrderBy(x => x.LineNumber).ToList();

            //カウンタ
            int cnt = 0;

            foreach (var l in lList)
            {
                //メカニック担当者はヘッダに設定
                if(string.IsNullOrWhiteSpace(spHeader.MechanicEmployeeName))
                {
                    spHeader.MechanicEmployeeName = l.Employee != null ? l.Employee.EmployeeName : "";
                }

                //明細情報が22行以上の場合は次ページへ
                if (cnt > 21)
                {
                    //---------------------------
                    //次ページの情報の設定
                    //---------------------------
                    //次ページのヘッダ情報を設定
                    ServiceSalesSlipHeader setHeader = spHeader.Clone();
                    //明細情報を設定
                    setHeader.list = lineList;
                    //ヘッダ情報を設定
                    headerList.Add(setHeader);
                    //ページ数をカウントアップ
                    spHeader.PageCnt = ++spHeader.PageCnt;
                    //明細情報
                    lineList = new List<ServiceSalesSlipLine>();
                    //カウンタクリア
                    cnt = 0;
                }
                
                ServiceSalesSlipLine spLine = new ServiceSalesSlipLine();

                //作業内容
                spLine.LineContents = l.LineContents;
                //区分名
                spLine.WorkTypeName = l.c_WorkType != null ? l.c_WorkType.Name : "";
                //技術料
                spLine.FeeAmount = null;
                //数量
                spLine.Quantity = l.Quantity;
                //部品単価
                spLine.Price = null;
                //部品金額
                spLine.PartAmount = null;

                //技術料、金額、部品金額は出力しない ※将来出力した場合のために、コメントアウト
                /*
                if (l.ServiceWork != null && !l.ServiceWork.Classification2.Equals("006"))
                {
                    if(!string.IsNullOrWhiteSpace(l.ServiceType) && l.ServiceType.Equals("002"))
                    {
                        //技術料
                        spLine.FeeAmount = (l.TechnicalFeeAmount ?? 0m) + (l.TaxAmount ?? 0m);
                        //部品単価
                        spLine.Price = null;
                        //部品金額
                        spLine.PartAmount = null;
                    }
                    else if (!string.IsNullOrWhiteSpace(l.ServiceType) && l.ServiceType.Equals("002"))
                    {
                        //技術料
                        spLine.FeeAmount = null;
                        //部品単価
                        spLine.Price = (l.Price ?? 0m) + (l.TaxAmount ?? 0m);
                        //部品金額
                        spLine.PartAmount = (l.Quantity ?? 0m) * (l.Price ?? 0m) + (l.TaxAmount ?? 0m);
                    }
                    else
                    {
                        //技術料
                        spLine.FeeAmount = null;
                        //部品単価
                        spLine.Price = null;
                        //部品金額
                        spLine.PartAmount = null;
                    }
                    
                }
                */

                lineList.Add(spLine);

                cnt++;

                //最終要素の場合
                if (lList.IndexOf(l) == (lList.Count - 1))
                {
                    //次ページのヘッダ情報を設定
                    ServiceSalesSlipHeader setHeader = spHeader.Clone();
                    //明細情報を設定
                    setHeader.list = lineList;
                    //ヘッダ情報を設定
                    headerList.Add(setHeader);
                }
            }

            return headerList;
        }
        #endregion

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 新規作成
        /// </history>
        private void SetComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //伝票番号
            ViewData["SlipNumber"] = form["SlipNumber"];                                                          

            //------------------
            //入庫日
            //------------------
            //対象年
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["TargetDateY"], false);
            //対象月
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["TargetDateM"], false);

            //デフォルト対象年月
            ViewData["DefaultTargetDateY"] = form["DefaultTargetDateY"];
            ViewData["DefaultTargetDateM"] = form["DefaultTargetDateM"];

            //--------------
            //部門
            //--------------
            //部門コード
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            //部門名

            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

                ViewData["DepartmentName"] = dep != null ? dep.DepartmentName : "";
            }

            //伝票ステータス
            List<c_ServiceOrderStatus> statusList = dao.GetServiceOrderStatusAll(false).Where(x => x.Code.Equals("003") || x.Code.Equals("004") || x.Code.Equals("005") || x.Code.Equals("006") || x.Code.Equals("009")).ToList();

            ViewData["ServiceOrderStatusList"] = CodeUtils.GetSelectListByModel(statusList, form["ServiceOrderStatus"], true);

            //---------------
            //主作業
            //---------------
            //主作業コード
            ViewData["ServiceWorkCode"] = form["ServiceWorkCode"];
            //主作業名
            if (!string.IsNullOrEmpty(form["ServiceWorkCode"]))
            {
                ServiceWork sw = new ServiceWorkDao(db).GetByKey(form["ServiceWorkCode"]);

                ViewData["ServiceWorkName"] = sw != null ? sw.Name : "";
            }

            //顧客コード
            ViewData["CustomerCode"] = form["CustomerCode"];
            //顧客名
            ViewData["CustomerName"] = form["CustomerName"];


            return;
        }
        #endregion


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

    }
}
