using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加

using System.Xml.Linq;                  //Add 2018/05/28 arc yano #3886
using OfficeOpenXml;                    //Add 2018/05/28 arc yano #3886
using System.Configuration;             //Add 2018/05/28 arc yano #3886
using Microsoft.VisualBasic;            //Add 2018/05/28 arc yano #3886



namespace Crms.Controllers {

    /// <summary>
    /// 入金消込機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class DepositController : Controller {
        //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "入金管理";               // 画面名
        private static readonly string PROC_NAME = "入金消込明細登録";           // 処理名 
        private static readonly string PROC_NAME_KOBETSU = "個別入金消込";           // 処理名
        private static readonly string PROC_NAME_EXCELUPLOAD = "ワランティ一括消込";       // 処理名(Excel取込)     //Add 2018/05/28 arc yano #3886
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// 検索画面名
        /// </summary>
        protected string criteriaName = "DepositCriteria";
        protected bool isShopDeposit = false;
        protected bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DepositController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 入金消込検索画面表示
        /// </summary>
        /// <returns>入金消込検索画面</returns>
        /// <history>
        /// 2016/05/18 arc yano #3558 入金管理 請求先タイプの絞込み追加
        /// </history>
        [AuthFilter]
        public ActionResult Criteria() {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            //Add 2016/05/18 arc yano #3558
            if (!isShopDeposit) //入金管理画面の場合
            {
                form["CustomerClaimFilter"] = "001";        //デフォルト値の設定
            }
            //Add 2016/07/20 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）表示単位を変更できる件条件追加
            form["DispFlag"] = "0"; //表示単位のデフォルト値　（０：明細　１：サマリ）
            form["DefaultDispFlag"] = form["DispFlag"];

            return Criteria(new EntitySet<Journal>(), form);
        }

        /// <summary>
        /// 入金消込検索画面表示
        /// </summary>
        /// <param name="line">モデルデータ(複数件の登録内容)</param>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>入金消込検索画面</returns>
        /// <history>
        /// 2017/12/12 arc yano #3818 店舗入金消込ーサマリー表示での入金消込不正 入金額がプラスの場合とマイナスの場合で消込対象を変更する
        /// 2016/05/17 arc yano #3543 店舗入金消込　入金消込登録処理のスキップ 金額が０の場合、実績を作成しないように修正
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(EntitySet<Journal> line, FormCollection form) {

            // デフォルト値の設定
            form["action"] = (form["action"] == null ? "" : form["action"]);
            
            
            // 入金消込明細登録処理
            if (form["action"].Equals("regist") && line != null) {

                // データチェック
                ValidateJournal(line, form);

                // データ登録処理
                if (ModelState.IsValid) {

                    for (int i = 0; i < line.Count; i++) {

                        string prefix = "line[" + CommonUtils.IntToStr(i) + "].";

                        //Mod 2016/01/07 arc nakayama #3303_入金消込登録でチェックが付いていないものも処理されてしまう
                        //チェックが入っている項目のみ更新対象とする
                        if ((!string.IsNullOrEmpty(form["line[" + i + "].targetFlag"]) && form["line[" + i + "].targetFlag"].Equals("1")))
                        {
                                Journal journal = line[i];

                                // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
                                db = new CrmsLinqDataContext();
                                db.Log = new OutputWriter();

                                using (TransactionScope ts = new TransactionScope())
                                {

                                    ReceiptPlan targetReceiptPlan = new ReceiptPlan();

                                    List<ReceiptPlan> TargetPlanList = new List<ReceiptPlan>();

                                    //Add 2016/07/20 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）表示単位を変更できる件条件追加
                                    //明細表示で消し込む場合は選択された入金予定1レコードに対して入金消込を行う
                                    if (form[prefix + "AccountType"].Equals("011") || form["KeePDispFlag"].Equals("0"))
                                    {
                                        //Add 2016/07/20 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理） キーはガイド型ではなくString型に変更
                                        //一件の入金予定取得
                                        targetReceiptPlan = new ReceiptPlanDao(db).GetByStringKey(form[prefix + "ReceiptPlanId"]);
                                        
                                        // 出納帳登録
                                        InsertJournal(targetReceiptPlan, journal);

                                        // 入金消込
                                        UpdateReceiptPlan(targetReceiptPlan, journal);
                                    }
                                    else //そうでなえれば（サマリ表示なら）、金額の小さい順に消し込んでいく
                                    {
                                        //Mod 2017/03/06 arc nakayama #3719_店舗入金消込　残債と下取も消し込んでいる
                                        //金額の小さい順に消込を行う
                                        //①伝票番号と請求先から入金予定を取得
                                        if (isShopDeposit)
                                        {
                                            TargetPlanList = new ReceiptPlanDao(db).GetListByCustomerClaim(form[prefix + "SlipNumber"], form[prefix + "CustomerClaimCode"]).Where(x => !x.ReceiptType.Equals("012") && !x.ReceiptType.Equals("013")).ToList();
                                        }
                                        else
                                        {
                                            //Mod 2018/02/07 arc yano #3818
                                            //ReceiptPlanIdが'00000000-0000-0000-0000-000000000000'以外の場合
                                            if (!string.IsNullOrWhiteSpace(form["line[" + i + "].ReceiptPlanId"]) && !(form["line[" + i + "].ReceiptPlanId"]).Equals("00000000-0000-0000-0000-000000000000"))
                                            {
                                                ReceiptPlan rp = new ReceiptPlanDao(db).GetByKey(new Guid(form["line[" + i + "].ReceiptPlanId"]));

                                                TargetPlanList.Add(rp);
                                            }
                                            else
                                            {
                                                TargetPlanList = new ReceiptPlanDao(db).GetListByCustomerClaim(form[prefix + "SlipNumber"], form[prefix + "CustomerClaimCode"]).Where(x => !x.ReceiptType.Equals("012") && !x.ReceiptType.Equals("013")).ToList();
                                            }
                                        }
                                        // 出納帳登録
                                        InsertJournal(TargetPlanList.FirstOrDefault(), journal);

                                        /*
                                        //Mod 2017/12/12 arc yano #3818 実績がプラスの場合はプラスの予定を、マイナスの場合はマイナスの予定を消し込む
                                        if (journal.Amount >= 0)
                                        {
                                            TargetPlanList = TargetPlanList.Where(x => x.Amount >= 0).ToList();
                                        }
                                        else
                                        {
                                            TargetPlanList = TargetPlanList.Where(x => x.Amount < 0).ToList();
                                        }
                                        */

                                        //少ない順に消込
                                        Payment(TargetPlanList, journal.Amount);

                                    }

                                    //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
                                    for (int j = 0; j < DaoConst.MAX_RETRY_COUNT; j++)
                                    {
                                        try
                                        {
                                            db.SubmitChanges();
                                            ts.Complete();
                                            break;
                                        }
                                        catch (ChangeConflictException cfe)
                                        {
                                            foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                                            {
                                                occ.Resolve(RefreshMode.KeepCurrentValues);
                                            }
                                            if (j + 1 >= DaoConst.MAX_RETRY_COUNT)
                                            {
                                                // セッションにSQL文を登録
                                                Session["ExecSQL"] = OutputLogData.sqlText;
                                                // ログに出力

                                                if (form[prefix + "AccountType"].Equals("011") || form["DispType"].Equals("0"))
                                                {
                                                    OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, targetReceiptPlan.SlipNumber);
                                                }
                                                else
                                                {
                                                    OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, form[prefix + "SlipNumber"]);
                                                }
                                                // エラーページに遷移
                                                return View("Error");
                                            }
                                        }
                                        catch (SqlException se)
                                        {
                                            // セッションにSQL文を登録
                                            Session["ExecSQL"] = OutputLogData.sqlText;

                                            if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                                            {
                                                if (form[prefix + "AccountType"].Equals("011") || form["DispType"].Equals("0"))
                                                {
                                                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, targetReceiptPlan.SlipNumber);
                                                }
                                                else
                                                {
                                                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, form[prefix + "SlipNumber"]);
                                                }
                                                ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "入金消込登録"));
                                            }
                                            else
                                            {
                                                // ログに出力
                                                if (form[prefix + "AccountType"].Equals("011") || form["DispType"].Equals("0"))
                                                {
                                                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, targetReceiptPlan.SlipNumber);
                                                }
                                                else
                                                {
                                                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, form[prefix + "SlipNumber"]);
                                                }
                                                // エラーページに遷移
                                                return View("Error");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            // セッションにSQL文を登録
                                            Session["ExecSQL"] = OutputLogData.sqlText;
                                            if (form[prefix + "AccountType"].Equals("011") || form["DispType"].Equals("0"))
                                            {
                                                OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, targetReceiptPlan.SlipNumber);
                                            }
                                            else
                                            {
                                                OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, form[prefix + "SlipNumber"]);
                                            }
                                            return View("Error");
                                        }
                                    }

                                }
                            //} //Del 2016/05/17 arc yano #3543
                        }
                    }
                    ModelState.Clear();
                }
            } else {
                ModelState.Clear();
            }

            // 検索結果リストの取得
            PaginatedList<ReceiptPlan> list;
            if (criteriaInit) {
                list = new PaginatedList<ReceiptPlan>();
            } else {
                list = GetSearchResultList(form);
            }

            // その他出力項目の設定
            GetCriteriaViewData(list, line, form);

            // 入金消込検索画面の表示
            return View(criteriaName, list);
        }

        /// <summary>
        /// 請求書の印刷
        /// </summary>
        public void PrintInvoice() {
            string id = Request["PlanId"];
            Response.Redirect("/Report/Print?reportName=Invoice&reportParam=" + id);
        }

        #region 個別入金
        /// <summary>
        /// 個別入金画面の表示
        /// </summary>
        /// <param name="id">入金予定ID</param>
        /// <returns></returns>
        /// <history>Add 2016/07/20 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）キーはガイド型ではなくString型に変更</history>
        public ActionResult Detail(string strReceiptPlanId, string SlipNumber, string CustomerClaimCode, string KeePDispFlag)
        {
            ReceiptPlan receiptPlan = new ReceiptPlan();

            if (KeePDispFlag.Equals("0"))
            {
                receiptPlan = new ReceiptPlanDao(db).GetByStringKey(strReceiptPlanId);

            }
            else
            {
                receiptPlan = new ReceiptPlanDao(db).GetByCustomerClaim(SlipNumber, CustomerClaimCode);
                //合計金額と残高だけ集計した値に置き換えて、入金予定日を空白にする、IDも空にする
                
                ReceiptPlan Condition = new ReceiptPlan();
                Condition.CustomerClaim = new CustomerClaim();
                Condition.Account = new Account();

                Condition.SlipNumber = SlipNumber;
                Condition.CustomerClaim.CustomerClaimCode = CustomerClaimCode;
                Condition.CustomerClaimFilter = "000";
                Condition.IsShopDeposit = true;

                List<ReceiptPlan> SumPlan = new ReceiptPlanDao(db).GetSummaryListByCondition(Condition);
                receiptPlan.ReceiptPlanId = Guid.Empty;
                receiptPlan.Amount = SumPlan.Sum(x => x.Amount) ?? 0m;
                receiptPlan.ReceivableBalance = SumPlan.Sum(x => x.ReceivableBalance) ?? 0m;
                receiptPlan.ReceiptPlanDate = null;
            }
            EntitySet<Journal> journalList = new EntitySet<Journal>();
            ViewData["DepartmentCode"] = receiptPlan.DepartmentCode;
            ViewData["OfficeCode"] = receiptPlan.ReceiptDepartment.OfficeCode;
            SetModelData(receiptPlan, journalList, new FormCollection());

            return View("ShopDepositDetail", receiptPlan);
        }
        /// <summary>
        /// 個別入金消込処理
        /// </summary>
        /// <param name="receiptPlan">入金予定データ</param>
        /// <param name="journalList">入金消込データリスト</param>
        /// <returns></returns>
        /// <history>
        /// 2017/12/12 arc yano #3818 実績がプラスの場合はプラスの予定を、マイナスの場合はマイナスの予定を消し込む
        /// 2016/07/20 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）キーはガイド型ではなくString型に変更
        /// 2016/05/18 arc yano #3560 カード消込 カード消込時は、実績には予定のIDを、予定には実績のIDを設定し、紐付を行う
        /// 2016/05/18 arc yano #3559 「カード」⇒「カード会社からの入金」顧客からのカード消込時に
        /// 　　　　　　　　　　　　　作成される経理用レコードの入金種別は「カード会社からの入金」に変更する
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Detail(ReceiptPlan receiptPlan,EntitySet<Journal> line,FormCollection form) {
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["OfficeCode"] = form["OfficeCode"];

            ValidateDepositDetail(receiptPlan, line, form);
            if (!ModelState.IsValid) {
                SetModelData(receiptPlan, line, form);
                return View("ShopDepositDetail", receiptPlan);
            }

            // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                //Add 2016/07/20 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）キーはガイド型ではなくString型に変更

                ReceiptPlan targetReceiptPlan = new ReceiptPlan();
                List<ReceiptPlan> targetReceiptPlanList = new List<ReceiptPlan>();

                //元になる入金予定
                if (receiptPlan.ReceiptPlanId != Guid.Empty)
                {
                    targetReceiptPlan = new ReceiptPlanDao(db).GetByStringKey(receiptPlan.ReceiptPlanId.ToString());
                }
                else
                {
                    targetReceiptPlan = new ReceiptPlanDao(db).GetListByCustomerClaim(receiptPlan.SlipNumber, receiptPlan.CustomerClaimCode).FirstOrDefault();
                    targetReceiptPlanList = new ReceiptPlanDao(db).GetListByCustomerClaim(receiptPlan.SlipNumber, receiptPlan.CustomerClaimCode).Where(x => !x.ReceiptType.Equals("012") && !x.ReceiptType.Equals("013")).ToList(); //2017/12/12 arc yano #3818
                    //targetReceiptPlanList = new ReceiptPlanDao(db).GetListByCustomerClaim(receiptPlan.SlipNumber, receiptPlan.CustomerClaimCode);
                }


                //出納帳への追加
                foreach (Journal item in line) {

                    //Journalへの転記
                    Journal newJournal = new Journal();
                    newJournal.JournalId = Guid.NewGuid();
                    newJournal.JournalDate = item.JournalDate;
                    newJournal.JournalType = "001";
                    newJournal.AccountType = item.AccountType;
                    newJournal.AccountCode = item.AccountCode;
                    newJournal.CustomerClaimCode = receiptPlan.CustomerClaimCode;
                    newJournal.DepartmentCode = form["DepartmentCode"];
                    newJournal.OfficeCode = form["OfficeCode"];
                    newJournal.CashAccountCode = item.CashAccountCode;
                    newJournal.SlipNumber = receiptPlan.SlipNumber;
                    newJournal.Amount = item.Amount;
                    newJournal.Summary = item.Summary;
                    newJournal.CreateDate = DateTime.Now;
                    newJournal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    newJournal.LastUpdateDate = DateTime.Now;
                    newJournal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    newJournal.DelFlag = "0";
                    newJournal.ReceiptPlanFlag = "1"; //消込済み
                   
                    //別売掛金への振り替え
                    if (item.AccountType != null && item.AccountType.Equals("003") && isShopDeposit) {
                        ReceiptPlan newPlan = new ReceiptPlan();
                        newPlan.ReceiptPlanId = Guid.NewGuid();
                        newPlan.AccountCode = item.AccountCode;
                        newPlan.Amount = item.Amount;
                        newPlan.CompleteFlag = "0";
                        newPlan.CustomerClaimCode = item.CustomerClaimCode;
                        newPlan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //経理に振り替え
                        newPlan.OccurredDepartmentCode = receiptPlan.DepartmentCode;
                        newPlan.ReceiptPlanDate = CommonUtils.GetPaymentPlanDate(item.JournalDate, new CustomerClaimDao(db).GetByKey(item.CustomerClaimCode));
                        //newPlan.ReceiptType = "003"; //クレジット
                        newPlan.ReceiptType = "011"; //カード会社からの入金        //Mod 2016/05/18 arc yano #3559
                        newPlan.ReceivableBalance = item.Amount;
                        newPlan.SlipNumber = receiptPlan.SlipNumber;
                        newPlan.Summary = item.Summary;
                        newPlan.CreateDate = DateTime.Now;
                        newPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        newPlan.LastUpdateDate = DateTime.Now;
                        newPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        newPlan.DelFlag = "0";

                        //Mod 2016/05/18 arc yano #3560
                        //カード消込を行った入金実績に作成された経理用レコードの入金予定IDを設定
                        newJournal.CreditReceiptPlanId = newPlan.ReceiptPlanId.ToString().ToUpper();
                        
                        // 決済条件を追加
                        newPlan.PaymentKindCode = item.PaymentKindCode;
                        PaymentKind paymentKind = new PaymentKindDao(db).GetByKey(item.PaymentKindCode);
                        newPlan.CommissionRate = paymentKind != null ? paymentKind.CommissionRate : 0m;
                        newPlan.CommissionAmount = paymentKind != null ? Math.Truncate(decimal.Multiply(decimal.Multiply(paymentKind.CommissionRate, item.Amount), 0.01m)) : 0m;


                        // 入金日が決済日となる
                        newPlan.JournalDate = item.JournalDate;

                        db.ReceiptPlan.InsertOnSubmit(newPlan);

                        newJournal.TransferFlag = "1"; // 転記フラグ
                    }
                    db.Journal.InsertOnSubmit(newJournal);
                }

                //入金予定データの更新
                Journal summary = new Journal();
                summary.Amount = line.Sum(x => x.Amount);

                if (receiptPlan.ReceiptPlanId != Guid.Empty)
                {
                    UpdateReceiptPlan(targetReceiptPlan, summary);
                }
                else
                {
                    //少ない順に消込
                    Payment(targetReceiptPlanList, summary.Amount);
                }
                // Add 2014/08/06 arc amii エラーログ対応 catch句にChangeConflictExceptionを追加
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException cfe)
                    {
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(cfe, PROC_NAME_KOBETSU, FORM_NAME, targetReceiptPlan.SlipNumber);
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        // Add 2014/08/06 arc amii エラーログ対応 SqlException発生時、エラーログ出力する処理を追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_KOBETSU, FORM_NAME, targetReceiptPlan.SlipNumber);
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "入金消込登録"));
                            SetModelData(receiptPlan, new EntitySet<Journal>(), form);
                            return View("ShopDepositDetail", receiptPlan);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME_KOBETSU, FORM_NAME, targetReceiptPlan.SlipNumber);
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME_KOBETSU, FORM_NAME, "");
                        return View("Error");
                    }
                }
            }
            SetModelData(receiptPlan, new EntitySet<Journal>(), form);
            ViewData["close"] = "1";
            return View("ShopDepositDetail", receiptPlan);
        }

        /// <summary>
        /// 個別入金画面行追加
        /// </summary>
        /// <param name="receiptPlan">入金予定データ</param>
        /// <param name="item">入金データリスト</param>
        /// <returns></returns>
        public ActionResult AddLine(ReceiptPlan receiptPlan, EntitySet<Journal> line,FormCollection form) {
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["OfficeCode"] = form["OfficeCode"];

            Journal journal = new Journal();
            journal.JournalDate = DateTime.Today ;
            journal.Account = new AccountDao(db).GetByKey(receiptPlan.AccountCode);
            journal.CustomerClaim = new CustomerClaimDao(db).GetByKey(receiptPlan.CustomerClaimCode);

            if (line == null) {
                line = new EntitySet<Journal>();
            }
            line.Add(journal);
            SetModelData(receiptPlan, line, form);
            return View("ShopDepositDetail", receiptPlan);
        }

        /// <summary>
        /// 個別入金画面行削除
        /// </summary>
        /// <param name="id">行番号</param>
        /// <param name="receiptPlan">入金予定データ</param>
        /// <param name="item">入金データリスト</param>
        /// <returns></returns>
        public ActionResult DelLine(int id, ReceiptPlan receiptPlan, EntitySet<Journal> line,FormCollection form) {
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            
            ModelState.Clear();
            line.RemoveAt(id);
            SetModelData(receiptPlan, line, form);
            return View("ShopDepositDetail", receiptPlan);
        }

        /// <summary>
        /// 個別入金画面データコンポーネントのセット
        /// </summary>
        /// <param name="receiptPlan"></param>
        /// <param name="item"></param>
        /// <history>
        /// 2016/05/19 arc yano #3549 店舗入金消込(個別消込)　入金種別リストの変更
        /// </history>
        private void SetModelData(ReceiptPlan receiptPlan, EntitySet<Journal> line,FormCollection form) {
            if (line == null) {
                line = new EntitySet<Journal>();
            }
            ViewData["JournalList"] = line;

            CodeDao dao = new CodeDao(db);
            List<IEnumerable<SelectListItem>> accountTypeList = new List<IEnumerable<SelectListItem>>();
            //List<c_AccountType> accountTypeListSrc = dao.GetAccountTypeAll(true);
            List<c_AccountType> accountTypeListSrc = dao.GetAccountType("003");   //Mod 2016/05/19 arc yano #3549
            List<IEnumerable<SelectListItem>> cashAccountCodeList = new List<IEnumerable<SelectListItem>>();
            List<IEnumerable<SelectListItem>> paymentKindCodeList = new List<IEnumerable<SelectListItem>>();

            foreach (var journal in line) {
                journal.CustomerClaim = new CustomerClaimDao(db).GetByKey(journal.CustomerClaimCode);
                journal.Account = new AccountDao(db).GetByKey(journal.AccountCode);
                accountTypeList.Add(CodeUtils.GetSelectListByModel(accountTypeListSrc, journal.AccountType, true));
                List<CashAccount> accountList = new CashAccountDao(db).GetListByOfficeCode(form["OfficeCode"]);
                List<CodeData> accountDataList = new List<CodeData>();
                foreach (var a in accountList) {
                    CodeData data = new CodeData();
                    data.Code = a.CashAccountCode;
                    data.Name = a.CashAccountName;
                    accountDataList.Add(data);
                }
                cashAccountCodeList.Add(CodeUtils.GetSelectList(accountDataList,journal.CashAccountCode,false));

                if (journal.CustomerClaim != null && journal.CustomerClaim.CustomerClaimable != null) {
                    List<CodeData> customerClaimableDataList = new List<CodeData>();
                    foreach (var customerClaimable in journal.CustomerClaim.CustomerClaimable) {
                        CodeData claimable = new CodeData();
                        claimable.Code = customerClaimable.PaymentKindCode;
                        claimable.Name = customerClaimable.PaymentKind != null ? customerClaimable.PaymentKind.PaymentKindName : "";
                        customerClaimableDataList.Add(claimable);
                    }
                    paymentKindCodeList.Add(CodeUtils.GetSelectListByModel(customerClaimableDataList, journal.PaymentKindCode, true));
                }
            }
            ViewData["AccountTypeList"] = accountTypeList;
            ViewData["CashAccountCodeList"] = cashAccountCodeList;
            ViewData["PaymentKindCodeList"] = paymentKindCodeList;
            receiptPlan.CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(receiptPlan.SlipNumber);
            receiptPlan.Account = new AccountDao(db).GetByKey(receiptPlan.AccountCode);
            receiptPlan.CustomerClaim = new CustomerClaimDao(db).GetByKey(receiptPlan.CustomerClaimCode);
            if (ViewData["DepartmentCode"] != null && !ViewData["DepartmentCode"].Equals("")) {
                Department department = new DepartmentDao(db).GetByKey(ViewData["DepartmentCode"].ToString());
                try { ViewData["DepartmentName"] = department.DepartmentName; } catch (NullReferenceException) { }
            }
            if (ViewData["OfficeCode"] != null && !ViewData["OfficeCode"].Equals("")) {
                Office office = new OfficeDao(db).GetByKey(ViewData["OfficeCode"].ToString());
                try { ViewData["OfficeName"] = office.OfficeName; } catch (NullReferenceException) { }
            }

            //Add 2017/04/19 arc nakayama #3754_店舗入金消込（個別）行追加で納車予定日と納車日が消える場合がある
            ServiceSalesHeader sh = new ServiceSalesOrderDao(db).GetBySlipNumber(receiptPlan.SlipNumber);
            CarSalesHeader ch = new CarSalesOrderDao(db).GetBySlipNumber(receiptPlan.SlipNumber);
            if (sh != null)
            {
                receiptPlan.ServiceSalesHeader = new ServiceSalesHeader();
                receiptPlan.ServiceSalesHeader.SalesDate = sh.SalesDate;
                receiptPlan.ServiceSalesHeader.SalesPlanDate = sh.SalesPlanDate;
            }
            else if (ch != null)
            {
                receiptPlan.CarSalesHeader = new CarSalesHeader();
                receiptPlan.CarSalesHeader.SalesDate = ch.SalesDate;
                receiptPlan.CarSalesHeader.SalesPlanDate = ch.SalesPlanDate;
            }
        }

        /// <summary>
        /// 個別入金時のValidationチェック
        /// </summary>
        /// <param name="item">入金明細データ</param>
        /// <history>
        /// 2019/02/07 yano #3966 店舗入金消込(入金管理)　消込直前の伝票修正による消込エラー
        /// </history>
        private void ValidateDepositDetail(ReceiptPlan receiptPlan, EntitySet<Journal> line, FormCollection form)
        {
            bool alreadyOutputMsgE0001 = false;
            bool alreadyOutputMsgE0002 = false;
            bool alreadyOutputMsgE0003 = false;
            bool alreadyOutputMsgE0004 = false;
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "計上部門"));
            }
            if (string.IsNullOrEmpty(form["OfficeCode"]))
            {
                ModelState.AddModelError("OfficeCode", MessageUtils.GetMessage("E0001", "入金事業所"));
            }
            if (line == null)
            {
                ModelState.AddModelError("", "明細を追加してください");
                return;
            }

            //Add 2019/02/07 yano #3966
            ReceiptPlan target = new ReceiptPlanDao(db).GetByKey(receiptPlan.ReceiptPlanId);

            if (target == null)
            {
                ModelState.AddModelError("", "伝票が更新されたため、消込を行うことができません。再検索後に消込を行って下さい");
            }


            for (int i = 0; i < line.Count(); i++)
            {
                string prefix = string.Format("line[{0}].", i);
                Journal journal = line[i];

                // 必須チェック(数値/日付項目は属性チェックを兼ねる)
                if (!ModelState.IsValidField(prefix + "JournalDate"))
                {
                    ModelState.AddModelError(prefix + "JournalDate", (alreadyOutputMsgE0003 ? "" : MessageUtils.GetMessage("E0003", "入金日")));
                    alreadyOutputMsgE0003 = true;
                    if (ModelState[prefix + "JournalDate"].Errors.Count > 1)
                    {
                        ModelState[prefix + "JournalDate"].Errors.RemoveAt(0);
                    }
                }
                if (string.IsNullOrEmpty(journal.AccountType))
                {
                    ModelState.AddModelError(prefix + "AccountType", (alreadyOutputMsgE0001 ? "" : MessageUtils.GetMessage("E0001", "入金種別")));
                    alreadyOutputMsgE0001 = true;
                }
                if (!ModelState.IsValidField(prefix + "Amount"))
                {
                    ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "金額", "0以外の10桁以内の整数のみ" })));
                    alreadyOutputMsgE0002 = true;
                    if (ModelState[prefix + "Amount"].Errors.Count > 1)
                    {
                        ModelState[prefix + "Amount"].Errors.RemoveAt(0);
                    }
                }
                if (string.IsNullOrEmpty(journal.CustomerClaimCode))
                {
                    ModelState.AddModelError(prefix + "CustomerClaimCode", (alreadyOutputMsgE0004 ? "" : MessageUtils.GetMessage("E0001", "請求先")));
                    if (ModelState[prefix + "CustomerClaimCode"].Errors.Count > 1)
                    {
                        ModelState[prefix + "CustomerClaimCode"].Errors.RemoveAt(0);
                    }
                }
                else
                {
                    CustomerClaim customerClaim = new CustomerClaimDao(db).GetByKey(journal.CustomerClaimCode);
                    //請求先がマスタに存在しなければエラー
                    if (customerClaim == null)
                    {
                        ModelState.AddModelError(prefix + "CustomerClaimCode", MessageUtils.GetMessage("E0016", journal.CustomerClaimCode));
                    }
                    else
                    {
                        //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                        //請求先が個人または法人なのにカードを選んでいたらエラー
                        if ((CommonUtils.DefaultString(customerClaim.CustomerClaimType).Equals("001")
                            || CommonUtils.DefaultString(customerClaim.CustomerClaimType).Equals("002"))
                            && CommonUtils.DefaultString(journal.AccountType).Equals("003"))
                        {
                            ModelState.AddModelError(prefix + "AccountType", "指定した請求先の種別はカード入金を選ぶことはできません");
                        }
                        else
                        {
                            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                            // 請求先がカード会社の場合はカード入金以外不可
                            if ((CommonUtils.DefaultString(customerClaim.CustomerClaimType).Equals("003")
                                || CommonUtils.DefaultString(customerClaim.CustomerClaimType).Equals("004"))
                                && !CommonUtils.DefaultString(journal.AccountType).Equals("003"))
                            {
                                ModelState.AddModelError(prefix + "AccountType", "指定した請求先の種別はカード入金以外選ぶことはできません");
                            }
                            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                            //カード会社の場合決済条件は必須設定
                            if (CommonUtils.DefaultString(customerClaim.CustomerClaimType).Equals("003"))
                            {
                                // マスタにない
                                if (customerClaim.CustomerClaimable == null || customerClaim.CustomerClaimable.Count == 0)
                                {
                                    ModelState.AddModelError(prefix + "CustomerClaimCode", "指定した請求先は決済条件が設定されていないため利用できません");
                                }
                                else if (string.IsNullOrEmpty(journal.PaymentKindCode))
                                {
                                    // 選択していない
                                    ModelState.AddModelError(prefix + "PaymentKindCode", MessageUtils.GetMessage("E0007", new string[] { "カード決済", "決済条件" }));
                                }
                            }
                        }

                    }
                }
                // フォーマットチェック
                if (ModelState.IsValidField(prefix + "Amount"))
                {
                    if (!Regex.IsMatch(journal.Amount.ToString(), @"^[-]?\d{1,10}$"))
                    {
                        ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "金額", "0以外の10桁以内の整数のみ" })));
                        alreadyOutputMsgE0002 = true;
                    }
                }

                // 値チェック
                if (ModelState.IsValidField(prefix + "JournalDate"))
                {
                    if (journal.JournalDate.CompareTo(DateTime.Now.Date) > 0)
                    {
                        ModelState.AddModelError(prefix + "JournalDate", MessageUtils.GetMessage("E0013", new string[] { "入金日", "本日以前" }));
                    }
                    CashBalance cashBalance = GetLatestClosedData(form["OfficeCode"], journal.CashAccountCode);
                    DateTime latestClosedDate = (cashBalance == null ? DaoConst.SQL_DATETIME_MIN : cashBalance.ClosedDate);
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if (journal.JournalDate.CompareTo(latestClosedDate) <= 0 && CommonUtils.DefaultString(journal.AccountType).Equals("001"))
                    {
                        ModelState.AddModelError(prefix + "JournalDate", MessageUtils.GetMessage("E0013", new string[] { "入金日", "前回締め日より後" }));
                    }

                    // Add 2014/04/17 arc nakayama #3096 締め処理後でも店舗入金消込が個別だと可能となる 全体での消込も個別消込も経理締めを見るようにする
                    string departmentCode = "";
                    CarSalesHeader carHeader = new CarSalesOrderDao(db).GetBySlipNumber(receiptPlan.SlipNumber);
                    ServiceSalesHeader serviceHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(receiptPlan.SlipNumber);
                    if (carHeader != null){
                        departmentCode = carHeader.DepartmentCode;
                    }else if (serviceHeader != null){
                        departmentCode = serviceHeader.DepartmentCode;
                    }

                    //ログインユーザ情報取得
                    Employee loginUser = ((Employee)Session["Employee"]);
                    ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "EditTempClosedData"); //仮締データ編集権限があるかないか

                    //仮締データ編集権限があれば本締めの時のみエラー
                    if (AppRole.EnableFlag){
                        if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(departmentCode, journal.JournalDate, "001"))
                        {
                            ModelState.AddModelError(prefix + "JournalDate", MessageUtils.GetMessage("E0013", new string[] { "入金日", "前回の月次締め処理が実行された年月より後" }));
                        }
                    }else{
                        // そうでなければ仮締め以上でエラー
                        if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(departmentCode, journal.JournalDate, "001"))
                        {
                            ModelState.AddModelError(prefix + "JournalDate", MessageUtils.GetMessage("E0013", new string[] { "入金日", "前回の月次締め処理が実行された年月より後" }));
                        }
                    }
                    
                    if (ModelState.IsValidField(prefix + "Amount"))
                    {
                        if (journal.Amount.Equals(0m))
                        {
                            ModelState.AddModelError(prefix + "Amount", MessageUtils.GetMessage("E0002", new string[] { "金額", "0以外の10桁以内の整数のみ" }));
                        }
                    }
                    CashAccount cashAccount = new CashAccountDao(db).GetByKey(form["OfficeCode"], journal.CashAccountCode);
                    if (cashAccount == null)
                    {
                        ModelState.AddModelError(prefix + "CashAccountCode", "指定した事業所と現金口座の組み合わせが不正です");
                    }
                }
                //if (receiptPlan.ReceivableBalance < line.Sum(x => x.Amount)) {
                //    ModelState.AddModelError("", "入金額が入金予定額よりも多いため消込処理できません");
                //}
            }
        }
        #endregion

        /// <summary>
        /// 検索画面表示データの取得
        /// </summary>
        /// <param name="list">入金予定検索結果リスト</param>
        /// <param name="line">モデルデータ(複数件の登録内容)</param>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <history>
        /// 2016/05/18 arc yano #3558 入金管理  請求先タイプの絞込み追加
        /// 2016/05/18 arc yano #3562 入金管理　一覧の入金種別の初期表示 一覧の入金種別の初期表示を入金予定の入金種別で表示する(現金は除く)
        /// 2016/05/18 arc yano #3459 カードの消込を行えないようにする
        /// 2016/05/18 arc yano #3546 店舗入金消込／入金管理により、選択できる入金種別を変更する
        /// </history>
        private void GetCriteriaViewData(PaginatedList<ReceiptPlan> list, EntitySet<Journal> line, FormCollection form) {

            CodeDao dao = new CodeDao(db);

            // 検索条件部の画面表示データ取得
            ViewData["CondDepartmentCode"] = form["CondDepartmentCode"];
            if (!string.IsNullOrEmpty(form["CondDepartmentCode"])) {
                Department department = new DepartmentDao(db).GetByKey(form["CondDepartmentCode"]);
                if (department != null) {
                    ViewData["CondDepartmentName"] = department.DepartmentName;
                }
            }
            ViewData["OfficeCode"] = form["OfficeCode"];
            if (!string.IsNullOrEmpty(form["OfficeCode"])) {
                Office office = new OfficeDao(db).GetByKey(form["OfficeCode"]);
                if (office != null) {
                    ViewData["OfficeName"] = office.OfficeName;
                }
            }
            ViewData["SalesDateFrom"] = form["SalesDateFrom"];
            ViewData["SalesDateTo"] = form["SalesDateTo"];
            ViewData["JournalDateFrom"] = form["JournalDateFrom"];
            ViewData["JournalDateTo"] = form["JournalDateTo"];
            ViewData["ReceiptPlanDateFrom"] = form["ReceiptPlanDateFrom"];
            ViewData["ReceiptPlanDateTo"] = form["ReceiptPlanDateTo"];
            ViewData["CondSlipNumber"] = form["CondSlipNumber"];
            ViewData["CondCustomerClaimCode"] = form["CondCustomerClaimCode"];

            ViewData["CustomerClaimFilter"] = form["CustomerClaimFilter"];      //Add 2016/05/18 arc yano #3558

            List<CodeData> paymentKindList = new List<CodeData>();
            if (!string.IsNullOrEmpty(form["CondCustomerClaimCode"])) {
                CustomerClaim customerClaim = new CustomerClaimDao(db).GetByKey(form["CondCustomerClaimCode"]);
                if (customerClaim != null) {
                    ViewData["CondCustomerClaimName"] = customerClaim.CustomerClaimName;
                }

                foreach (var item in customerClaim.CustomerClaimable) {
                    paymentKindList.Add(new CodeData() { Code = item.PaymentKindCode, Name = item.PaymentKind != null ? item.PaymentKind.PaymentKindName : "" });
                }
                
            }
            ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerClaimTypeAll(false, form["CustomerClaimFilter"]), form["CustomerClaimType"], true);   //Mod 2016/05/18 arc yano #3558
            ViewData["PaymentKindList"] = CodeUtils.GetSelectListByModel(paymentKindList, form["PaymentKindCode"], true);
            ViewData["AccountUsageTypeList"] = CodeUtils.GetSelectListByModel<c_AccountUsageType>(dao.GetAccountUsageTypeAll(false), form["AccountUsageType"], true);


            ViewData["ReceiptTypeList"] = CodeUtils.GetSelectListByModel<c_ReceiptType>(dao.GetReceiptTypeAll(false), form["ReceiptType"], true).Where(x => !x.Value.Equals("013")); //下取は表示しない

            // 出納帳明細登録及び検索結果部の画面表示データ取得
            List<Journal> journalList = new List<Journal>();
            List<IEnumerable<SelectListItem>> accountTypeList = new List<IEnumerable<SelectListItem>>();
            //List<c_AccountType> accountTypeListSrc = dao.GetAccountTypeAll(isShopDeposit ? false : true);
            
            //Mod 2016/05/18 arc yano #3546
            List<c_AccountType> accountTypeListSrc = null;
            
            if (isShopDeposit)
            {
                accountTypeListSrc = dao.GetAccountType("002");
            }
            else
            {
                accountTypeListSrc = dao.GetAccountType("001");        //Mod 2016/05/19 arc yano #3549
            }
            
            List<IEnumerable<SelectListItem>> cashAccountList = new List<IEnumerable<SelectListItem>>();

            for (int i = 0; i < list.Count; i++) { // 登録時入力エラーの場合はModelStateをクリアしない為、Journal初期化については画面反映されない。
                Journal journal = new Journal();
                journal.JournalDate = DateTime.Now.Date;
                journalList.Add(journal);
                string accountType = journal.AccountType;
                if (string.IsNullOrEmpty(accountType)) {
                    switch (list[i].ReceiptType) {
                        case "001":
                            accountType = "";
                            break;

                        /* Mod 2016/05/18 arc yano #3562 現金以外は入金予定の入金種別を設定
                        case "003":
                            accountType = "003";  
                            break;
                        case "004":
                            accountType = "004";
                            break;
                        */
                        default:
                            accountType = list[i].ReceiptType;
                            break;
                    }
                }
                accountTypeList.Add(CodeUtils.GetSelectListByModel(accountTypeListSrc, accountType, true));
                list[i].CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(list[i].SlipNumber);
                list[i].ServiceSalesHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(list[i].SlipNumber);

                List<CashAccount> accountList = new CashAccountDao(db).GetListByOfficeCode(list[i].ReceiptDepartment.OfficeCode);
                List<CodeData> accountDataList = new List<CodeData>();
                foreach (var a in accountList) {
                    CodeData data = new CodeData();
                    data.Code = a.CashAccountCode;
                    data.Name = a.CashAccountName;
                    accountDataList.Add(data);
                }
                cashAccountList.Add(CodeUtils.GetSelectList(accountDataList, journal.CashAccountCode, false));
            }

           

            ViewData["JournalList"] = journalList;
            ViewData["AccountTypeList"] = accountTypeList;
            ViewData["TotalBalance"] = list.Count()>0 ? GetTotalBalance(form) : 0m;
            ViewData["CalcTotalBalance"] = list.Count() > 0 ? GetTotalBalance(form) : 0m;
            ViewData["CashAccountList"] = cashAccountList;
            ViewData["TotaljournalAmount"] = "0"; //検索後はチェックがついていないため０を入れる
            //Add 2016/07/20 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）表示単位を変更できる件条件追加
            ViewData["DispFlag"] = form["DispFlag"];
            ViewData["DefaultDispFlag"] = form["DefaultDispFlag"];
            ViewData["KeePDispFlag"] = form["DispFlag"];

        }

        /// <summary>
        /// 入金消込検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>入金消込検索結果リスト</returns>
        /// <history>
        /// 2016/05/17 arc yano #3542 一覧の絞込み条件の変更 店舗入金消込画面の場合、請求先タイプが「クレジット」「ローン」「社内」を表示しない
        /// </history>
        private PaginatedList<ReceiptPlan> GetSearchResultList(FormCollection form) {

            PaginatedList<ReceiptPlan> list = new PaginatedList<ReceiptPlan>();

            //Add 2016/05/17 arc yano #3542 
            //Mod 2016/07/19 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）
            
            //明細表示の場合（入金予定を1レコードづつ取得）
            //店舗入金消込画面の場合
            if (form["DispFlag"].Equals("0"))
            {
                if (isShopDeposit)
                {
                    var ret = new ReceiptPlanDao(db).GetListByCondition(GetSearchCondition(form)).Where(
                            x => x.CustomerClaim != null && x.CustomerClaim.c_CustomerClaimType != null
                                                                    && x.CustomerClaim.c_CustomerClaimType.CustomerClaimFilter.Equals("000")     //請求先タイプ ≠「クレジット／ローン／社内」
                                                                    && (!x.ReceiptType.Equals("012") && !x.ReceiptType.Equals("013"))
                                                                    ).AsQueryable();

                    list = new PaginatedList<ReceiptPlan>(ret, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                }
                else
                {
                    var ret = new ReceiptPlanDao(db).GetListByCondition(GetSearchCondition(form)).Where(x => !x.ReceiptType.Equals("013")).AsQueryable();


                    list = new PaginatedList<ReceiptPlan>(ret, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                    //list = new ReceiptPlanDao(db).GetListByCondition(GetSearchCondition(form), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                }
            }
            else
            {
                if (isShopDeposit)
                {
                    var ret = new ReceiptPlanDao(db).GetSummaryListByCondition(GetSearchCondition(form)).AsQueryable();
                        
                    list = new PaginatedList<ReceiptPlan>(ret, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                }
                else
                {
                    var ret = new ReceiptPlanDao(db).GetSummaryListByCondition(GetSearchCondition(form)).AsQueryable();

                    list = new PaginatedList<ReceiptPlan>(ret, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                }
            }
            return list;
           
        }

        /// <summary>
        /// 残高合計取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>入金消込検索結果リスト</returns>
        private decimal GetTotalBalance(FormCollection form) {
            return new ReceiptPlanDao(db).GetTotalBalance(GetSearchCondition(form), isShopDeposit);
        }

        /// <summary>
        /// 検索条件設定
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>検索条件</returns>
        /// <history>
        /// 2016/05/18 arc yano #3558 入金管理 請求先タイプの絞込み追加
        /// 2016/05/17 arc yano #3543 店舗入金消込　入金消込登録処理のスキップ
        /// </history>
        private ReceiptPlan GetSearchCondition(FormCollection form) {

            ReceiptPlan receiptPlanCondition = new ReceiptPlan();
            receiptPlanCondition.JournalDateFrom = CommonUtils.GetDayStart(form["JournalDateFrom"], DaoConst.SQL_DATETIME_MAX);
            receiptPlanCondition.JournalDateTo = CommonUtils.GetDayEnd(form["JournalDateTo"], DaoConst.SQL_DATETIME_MIN);
            //receiptPlanCondition.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
            receiptPlanCondition.ReceiptPlanDateFrom = CommonUtils.GetDayStart(form["ReceiptPlanDateFrom"], DaoConst.SQL_DATETIME_MAX);
            receiptPlanCondition.ReceiptPlanDateTo = CommonUtils.GetDayEnd(form["ReceiptPlanDateTo"], DaoConst.SQL_DATETIME_MIN);
            if (!string.IsNullOrEmpty(form["CondSlipNumber"])) {
                try { receiptPlanCondition.SlipNumber = string.Format("{0:00000000}", decimal.Parse(CommonUtils.DefaultString(form["CondSlipNumber"]))); } catch (FormatException) { receiptPlanCondition.SlipNumber = form["CondSlipNumber"]; }
            }
            //receiptPlanCondition.Department = new Department();
            //receiptPlanCondition.Department.DepartmentCode = form["CondDepartmentCode"];
            receiptPlanCondition.OccurredDepartmentCode = form["CondDepartmentCode"];
            receiptPlanCondition.OfficeCode = form["OfficeCode"];
            receiptPlanCondition.CustomerClaim = new CustomerClaim();
            receiptPlanCondition.CustomerClaim.CustomerClaimCode = form["CondCustomerClaimCode"];
            receiptPlanCondition.CustomerClaim.CustomerClaimType = form["CustomerClaimType"];

            receiptPlanCondition.Account = new Account();
            receiptPlanCondition.Account.UsageType = form["AccountUsageType"];
            receiptPlanCondition.ReceiptType = form["ReceiptType"];
            receiptPlanCondition.SetAuthCondition((Employee)Session["Employee"]);
            receiptPlanCondition.PaymentKindCode = form["PaymentKindCode"];
            receiptPlanCondition.SalesDateFrom = CommonUtils.StrToDateTime(form["SalesDateFrom"]);
            receiptPlanCondition.SalesDateTo = CommonUtils.StrToDateTime(form["SalesDateTo"]);

            //Add 2016/05/18 arc yano #3558
            if(!string.IsNullOrWhiteSpace(form["CustomerClaimFilter"]))
            {
                //receiptPlanCondition.CustomerClaim = new CustomerClaim();
                //receiptPlanCondition.CustomerClaim.c_CustomerClaimType = new c_CustomerClaimType();
                receiptPlanCondition.CustomerClaimFilter = form["CustomerClaimFilter"];
            }

            //Add 2016/07/19 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）
            if (isShopDeposit)
            {
                receiptPlanCondition.CustomerClaimFilter = "000";
            }            
            receiptPlanCondition.DispType = form["DispFlag"];
            receiptPlanCondition.IsShopDeposit = isShopDeposit;

            return receiptPlanCondition;
        }

        /// <summary>
        /// 入金消込入力チェック
        /// </summary>
        /// <param name="journal">入金消込データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>入金消込データ</returns>
        /// <history>
        /// 2019/02/14 yano #3972 現金出納帳　出金データ登録時に伝票番号のみで紐づけることができない。
        /// 2019/02/07 yano #3966 店舗入金消込(入金管理)　消込直前の伝票修正による消込エラー
        /// 2016/05/18 arc yano #3557 入金消込登録  入金種別=011（カード会社からの入金）で　入金額 = 請求残高でない場合エラーとする
        /// </history>
        private EntitySet<Journal> ValidateJournal(EntitySet<Journal> line, FormCollection form) {

            bool alreadyOutputMsgE0001 = false;
            bool alreadyOutputMsgE0002 = false;
            bool alreadyOutputMsgE0003 = false;
            bool alreadyOutputMsgE0013a = false;
            bool alreadyOutputMsgAmount = false;
            bool alreadyOutputMsgCustomerClaimCode = false;
            bool alreadyOutputMsgE0001OfficeCode = false;            //Add 2019/02/14 yano #3972
            bool alreadyOutputMsgE0001CashAccountCode = false;       //Add 2019/02/14 yano #3972
 
            for (int i = 0; i < line.Count; i++)
            {

                //チェックが入ってるデータのみチェックの対象にする
                //Add 2016/01/07 arc nakayama #3303_入金消込登録でチェックが付いていないものも処理されてしまう
                if (!string.IsNullOrEmpty(form["line[" + i + "].targetFlag"]) && form["line[" + i + "].targetFlag"].Equals("1"))
                {
                    Journal journal = line[i];
                    string prefix = "line[" + CommonUtils.IntToStr(i) + "].";

                    //Add 2019/02/07 yano #3966
                    ReceiptPlan target = new ReceiptPlanDao(db).GetByStringKey(form[prefix + "ReceiptPlanId"]);
                    if (target == null)
                    {
                        ModelState.AddModelError("", "伝票が更新されたため、消込を行うことができません。再検索後に消込を行って下さい。");
                    }

                    /*  //Del 2016/05/17 arc yano #3582
                    if ((!CommonUtils.DefaultString(form[prefix + "JournalDate"]).Equals(string.Format("{0:yyyy/MM/dd}", DateTime.Now.Date))))
                        //|| (!string.IsNullOrEmpty(form[prefix + "AccountType"]))
                        //|| (!CommonUtils.DefaultString(form[prefix + "Amount"]).Equals("0")))     
                    {
                    */

                        // 必須チェック(数値/日付項目は属性チェックを兼ねる)
                        if (!ModelState.IsValidField(prefix + "JournalDate"))
                        {
                            ModelState.AddModelError(prefix + "JournalDate", (alreadyOutputMsgE0003 ? "" : MessageUtils.GetMessage("E0003", "入金日")));
                            alreadyOutputMsgE0003 = true;
                            if (ModelState[prefix + "JournalDate"].Errors.Count > 1)
                            {
                                ModelState[prefix + "JournalDate"].Errors.RemoveAt(0);
                            }
                        }
                        if (string.IsNullOrEmpty(journal.AccountType))
                        {
                            ModelState.AddModelError(prefix + "AccountType", (alreadyOutputMsgE0001 ? "" : MessageUtils.GetMessage("E0001", "入金種別")));
                            alreadyOutputMsgE0001 = true;
                        }
                        if (!ModelState.IsValidField(prefix + "Amount"))
                        {
                            ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "金額", "0以外の10桁以内の整数のみ" })));
                            alreadyOutputMsgE0002 = true;
                            if (ModelState[prefix + "Amount"].Errors.Count > 1)
                            {
                                ModelState[prefix + "Amount"].Errors.RemoveAt(0);
                            }
                        }

                        //Addd 2019/02/14 yano #3972
                        if (!isShopDeposit)
                        {
                            //-------------------
                            // 入金事業所
                            //-------------------
                            if (string.IsNullOrWhiteSpace(form[prefix + "OfficeCode"]))
                            {
                                ModelState.AddModelError(prefix + "OfficeCode", (alreadyOutputMsgE0001OfficeCode ? "" : MessageUtils.GetMessage("E0001", "入金事業所")));
                                alreadyOutputMsgE0001OfficeCode = true;
                            }
                            //----------------
                            // 入金口座
                            //----------------
                            string test = form[prefix + "CashAccountCode"];

                            if (string.IsNullOrWhiteSpace(form[prefix + "CashAccountCode"]))
                            {
                                ModelState.AddModelError(prefix + "CashAccountCode", (alreadyOutputMsgE0001CashAccountCode ? "" : MessageUtils.GetMessage("E0001", "口座")));
                                alreadyOutputMsgE0001CashAccountCode = true;
                            }
                        }

                       

                        // フォーマットチェック
                        if (ModelState.IsValidField(prefix + "Amount"))
                        {
                            if (!Regex.IsMatch(journal.Amount.ToString(), @"^[-]?\d{1,10}$"))
                            {
                                ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "金額", "0以外の10桁以内の整数のみ" })));
                                alreadyOutputMsgE0002 = true;
                            }
                        }

                       
                        //Add 2016/05/18 arc yano #3557   
                        //入金種別＝「カード会社からの入金」の場合のチェック処理
                        if (!string.IsNullOrWhiteSpace(journal.AccountType) && journal.AccountType.Equals("011"))
                        {
                            //入金予定を取得
                            ReceiptPlan plan = new ReceiptPlanDao(db).GetByStringKey(form[prefix + "ReceiptPlanId"]);

                            //請求先タイプのチェック
                            CustomerClaim rec = new CustomerClaimDao(db).GetByKey(plan.CustomerClaimCode);

                            //請求先のタイプ≠「クレジット」の場合、エラー
                            if (rec != null && !string.IsNullOrWhiteSpace(rec.CustomerClaimType) && !rec.CustomerClaimType.Equals("003"))
                            {
                                ModelState.AddModelError(prefix + "AccountType", (alreadyOutputMsgCustomerClaimCode ? "" : "請求先区分が「クレジット」以外の請求先の場合、入金種別の「カード会社からの入金」は選択できません"));
                                alreadyOutputMsgCustomerClaimCode = true;
                            }
                                
                            /*---------------------
                             *入金額のチェック
                             ---------------------*/                            
                            //入金額≠請求残高の場合エラー
                            if ((plan.ReceivableBalance ?? 0m) != journal.Amount)
                            {
                                ModelState.AddModelError(prefix + "Amount", ( alreadyOutputMsgAmount ? "" : "カード会社からの入金の場合、入金額は請求額全額を設定する必要があります"));
                                alreadyOutputMsgAmount = true;
                            }  
                        }
                           

                        // 値チェック
                        if (ModelState.IsValidField(prefix + "JournalDate"))
                        {
                            if (journal.JournalDate.CompareTo(DateTime.Now.Date) > 0)
                            {
                                ModelState.AddModelError(prefix + "JournalDate", (alreadyOutputMsgE0013a ? "" : MessageUtils.GetMessage("E0013", new string[] { "入金日", "本日以前" })));
                                alreadyOutputMsgE0013a = true;
                            }
                            // 締め処理済みかどうかチェック（2012.05.10修正）
                            // 最後の現金締め処理日を取得
                            CashBalance cashBalance = GetLatestClosedData(journal.OfficeCode, journal.CashAccountCode);

                            // 伝票日付が締日よりも前だったら締め処理済み扱い（現金）
                            if (journal.AccountType != null && journal.AccountType.Equals("001"))
                            {
                                if (cashBalance != null && journal.JournalDate <= cashBalance.ClosedDate)
                                {
                                    ModelState.AddModelError(prefix + "JournalDate", MessageUtils.GetMessage("E0013", new string[] { "入金日", "前回の現金締め日より後" }));
                                }
                            }
                            // Add 2014/04/17 arc nakayama #3096 締め処理後でも店舗入金消込が個別だと可能となる 全体での消込も個別消込も経理締めを見るようにする
                            string departmentCode = "";
                            CarSalesHeader carHeader = new CarSalesOrderDao(db).GetBySlipNumber(journal.SlipNumber);
                            ServiceSalesHeader serviceHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(journal.SlipNumber);
                            // Mod 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)
                            if (carHeader != null)
                            {
                                departmentCode = carHeader.DepartmentCode;
                            }
                            else if (serviceHeader != null)
                            {
                                departmentCode = serviceHeader.DepartmentCode;
                            }

                            //ログインユーザ情報取得
                            Employee loginUser = ((Employee)Session["Employee"]);
                            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "EditTempClosedData"); //仮締データ編集権限があるかないか

                            //仮締データ編集権限があれば本締めの時のみエラー
                            if (AppRole.EnableFlag)
                            {
                                if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(departmentCode, journal.JournalDate, "001"))
                                {
                                    ModelState.AddModelError(prefix + "JournalDate", (MessageUtils.GetMessage("E0013", new string[] { "入金日", "前回の月次締め処理が実行された年月より後" })));
                                }
                            }
                            else
                            {
                                // そうでなければ仮締め以上でエラー
                                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(departmentCode, journal.JournalDate, "001"))
                                {
                                    ModelState.AddModelError(prefix + "JournalDate", MessageUtils.GetMessage("E0013", new string[] { "入金日", "前回の月次締め処理が実行された年月より後" }));
                                }
                            }
                        }
                        if (ModelState.IsValidField(prefix + "Amount"))
                        {
                            if (journal.Amount.Equals(0m))
                            {
                                ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "金額", "0以外の10桁以内の整数のみ" })));
                                alreadyOutputMsgE0002 = true;
                            }
                        }
                    //} //Del 2016/05/17 arc yano #3582
                }
            }

            return line;
        }

        /// <summary>
        /// 直近の締め情報取得
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <returns>直近の締め情報</returns>
        private CashBalance GetLatestClosedData(string officeCode, string cashAccountCode) {

            CashBalance receiptPlanCondition = new CashBalance();
            receiptPlanCondition.OfficeCode = officeCode;
            receiptPlanCondition.CashAccountCode = cashAccountCode;
            return new CashBalanceDao(db).GetLatestClosedData(receiptPlanCondition);
        }

        /// <summary>
        /// 入金消込明細登録処理
        /// </summary>
        /// <param name="targetReceiptPlan">消込対象入金予定モデルデータ</param>
        /// <param name="journal">出納帳モデルデータ</param>
        /// <history>
        /// 2019/02/14 yano #3978 【入金管理】入金事業所、入金口座を編集できない
        /// 2017/11/14 arc yano #3811 車両伝票－下取車の入金予定残高更新不整合
        /// 2016/05/30 arc yano #3567 カード会社からの入金予定の消込 カード会社からの入金予定の場合、
        ///                     入金実績のCreditReceiptPlanIDに入金予定のIDを設定する
        /// </history>
        private void InsertJournal(ReceiptPlan targetReceiptPlan, Journal journal) {

            // JournalDate,AccountType,Amountはフレームワークにて編集済み
            journal.JournalId = Guid.NewGuid();
            journal.JournalType = "001";
            //Mod  2019/02/14 yano #3978
            if (isShopDeposit)
            {
                journal.OfficeCode = targetReceiptPlan.ReceiptDepartment.OfficeCode;
            }
            else
            {
                journal.OfficeCode = journal.OfficeCode;    
            }
            journal.DepartmentCode = targetReceiptPlan.DepartmentCode;
            journal.CustomerClaimCode = targetReceiptPlan.CustomerClaimCode;
            journal.SlipNumber = targetReceiptPlan.SlipNumber;
            journal.AccountCode = targetReceiptPlan.AccountCode;
            journal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            journal.CreateDate = DateTime.Now;
            journal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            journal.LastUpdateDate = DateTime.Now;
            journal.DelFlag = "0";
            journal.ReceiptPlanFlag = "1";
            //journal.Summary = targetReceiptPlan.Summary;

            journal.TradeVin = targetReceiptPlan.TradeVin;          //Add 2017/11/14 arc yano #3811

            //Add 2016/05/30 arc yano #3567
            //入金種別 =「カード会社からの入金」
            if (!string.IsNullOrWhiteSpace(journal.AccountType) && journal.AccountType.Equals("011"))
            {
                journal.CreditReceiptPlanId = targetReceiptPlan.ReceiptPlanId.ToString().ToUpper(); //入金実績のCreditReceiptPlanIDに入金予定のIDを設定する
            }
            db.Journal.InsertOnSubmit(journal);

        }

        /// <summary>
        /// 入金予定テーブル更新処理
        /// </summary>
        /// <param name="targetReceiptPlan">消込対象入金予定モデルデータ</param>
        /// <param name="journal">出納帳モデルデータ</param>
        /// <history>
        /// 2016/05/30 arc yano #3567 カード会社からの入金予定の消込 カード会社からの入金予定の場合、
        ///                     入金予定のCreditJournalIDに入金実績のIDを設定する
        /// </history>
        private void UpdateReceiptPlan(ReceiptPlan targetReceiptPlan, Journal journal) {

            targetReceiptPlan.ReceivableBalance = decimal.Subtract(targetReceiptPlan.ReceivableBalance ?? 0m, journal.Amount);
            if ((targetReceiptPlan.ReceivableBalance ?? 0m).Equals(0m)) {
                targetReceiptPlan.CompleteFlag = "1";
            }
            targetReceiptPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            targetReceiptPlan.LastUpdateDate = DateTime.Now;

            // Add 2016/05/30 arc yano #3567
            if (!string.IsNullOrWhiteSpace(journal.AccountType) && journal.AccountType.Equals("011"))
            {
                targetReceiptPlan.CreditJournalId = journal.JournalId.ToString().ToUpper();     //入金予定のCreditJournalIDに入金実績のIDを設定する
            }

        }
 
        #region Excel取込処理
        /// <summary>
        /// Excel取込用ダイアログ表示
        /// </summary>
        /// <param name="purchase">Excelデータ</param>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel取込(ワランティ消込)
        /// </history>
        [AuthFilter]
        public ActionResult ImportDialog(string id)
        {
            List<JournalExcelImport> ImportList = new List<JournalExcelImport>();
            FormCollection form = new FormCollection();

            ViewData["ErrFlag"] = "1";

            return View("ShopDepositImportDialog", ImportList);
        }

        /// <summary>
        /// Excel読み込み
        /// </summary>
        /// <param name="purchase">Excelデータ</param>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel取込(ワランティ消込)
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, FormCollection form)
        {
            List<JournalExcelImport> ImportList = new List<JournalExcelImport>();

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
                        return View("ShopDepositImportDialog", ImportList);
                    }

                    //Excel読み込み
                    ImportList = ReadExcelData(importFile, ImportList);

                    //読み込み時に何かエラーがあればここでリターン
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("ShopDepositImportDialog");
                    }

                    //Excelで読み込んだデータのバリデートチェック
                    ValidateImportList(ImportList, form);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("ShopDepositImportDialog");
                    }

                    //DB登録
                    DBExecute(ImportList, form);

                    form["ErrFlag"] = "1";
                    SetDialogDataComponent(form);
                    return View("ShopDepositImportDialog");

                //--------------
                //キャンセル
                //--------------
                case "2":
                    ImportList = new List<JournalExcelImport>();
                    ViewData["ErrFlag"] = "1";//[取り込み]ボタンが押せないようにするため
                    SetDialogDataComponent(form);
                    return View("ShopDepositImportDialog", ImportList);

                //----------------------------------
                //その他(ここに到達することはない)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("ShopDepositImportDialog");
            }
        }
        #endregion

        #region Excelデータ取得&設定
        /// Excelデータ取得&設定
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <returns>なし</returns>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel取込(ワランティ消込)
        /// </history>
        private List<JournalExcelImport> ReadExcelData(HttpPostedFileBase importFile, List<JournalExcelImport> ImportList)
        {
            //カラム番号保存用
            int[] colNumber = new int[11] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1};

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
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。シート名を確認して再度実行して下さい"));
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
                colNumber = SetColNumber(headerRow, colNumber);
                //タイトル行、ヘッダ行がおかしい場合は即リターンする
                if (!ModelState.IsValid)
                {
                    return ImportList;
                }

                //------------------------------
                // 読み取り処理
                //------------------------------
                int datarow = 0;
                string[] Result = new string[colNumber.Count()];

                for (datarow = StartRow + 1; datarow < EndRow + 1; datarow++)
                {
                    CarPurchaseExcelImportList data = new CarPurchaseExcelImportList();

                    //更新データの取得
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)
                    {
                        for (int i = 0; i < colNumber.Count(); i++)
                        {
                            if (col == colNumber[i])
                            {
                                Result[i] = !string.IsNullOrWhiteSpace(ws.Cells[datarow, col].Text) ? Strings.StrConv(ws.Cells[datarow, col].Text.Trim(), VbStrConv.Narrow, 0x0411).ToUpper() : "";
                                break;
                            }
                        }
                    }
                    //----------------------------------------
                    // 読み取り結果を画面の項目にセットする
                    //----------------------------------------
                    ImportList = SetProperty(ref Result, ref ImportList);
                }
            }
            return ImportList;

        }
        #endregion

        #region 各項目の列番号設定
        /// <summary>
        /// 各項目の列番号設定
        /// </summary>
        /// <param name="headerRow">ヘッダ行</param>
        /// <param name="colNumber">列番号</param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel取込(ワランティ消込)
        /// </history>
        private int[] SetColNumber(ExcelRangeBase headerRow, int[] colNumber)
        {
            //初期処理
            int cnt = 1;

            //列番号設定
            foreach (var cell in headerRow)
            {
                if (cell != null)
                {
                    //入金種別
                    if (cell.Text.Equals("JournalType"))
                    {
                        colNumber[0] = cnt;
                    }
                    //部門コード
                    if (cell.Text.Equals("DepartmentCode"))
                    {
                        colNumber[1] = cnt;
                    }
                    //請求先コード
                    if (cell.Text.Equals("CustomerClaimCode"))
                    {
                        colNumber[2] = cnt;
                    }
                    //伝票番号
                    if (cell.Text.Equals("SlipNumber"))
                    {
                        colNumber[3] = cnt;
                    }
                    //入金日
                    if (cell.Text.Equals("JournalDate"))
                    {
                        colNumber[4] = cnt;
                    }
                    //口座種別
                    if (cell.Text.Equals("AccountType"))
                    {
                        colNumber[5] = cnt;
                    }
                    //科目コード
                    if (cell.Text.Equals("AccountCode"))
                    {
                        colNumber[6] = cnt;
                    }
                    //入金額
                    if (cell.Text.Equals("Amount"))
                    {
                        colNumber[7] = cnt;
                    }
                    //入金消込フラグ
                    if (cell.Text.Equals("ReceiptPlanFlag"))
                    {
                        colNumber[8] = cnt;
                    }
                    //転記フラグ
                    if (cell.Text.Equals("TransferFlag"))
                    {
                        colNumber[9] = cnt;
                    }
                    //事業所コード
                    if (cell.Text.Equals("OfficeCode"))
                    {
                        colNumber[10] = cnt;
                    }
                    /*
                    //現金口座コード
                    if (cell.Text.Equals("CashAccountCode"))
                    {
                        colNumber[11] = cnt;
                    }
                    */

                    cnt++;
                }
            }
            return colNumber;
        }
    #endregion

        #region Excel読み取り前のチェック
        /// <summary>
        /// Excel読み取り前のチェック
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel取込(ワランティ消込)
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

            if (string.IsNullOrWhiteSpace(form["Summary"]))
            {
                ModelState.AddModelError("Summary", "コメントは入力必須です");
            }

            return;
        }
        #endregion

        #region 読み込み結果のバリデーションチェック
        /// <summary>
        /// 読み込み結果のバリデーションチェック
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <param name="form">フォーム入力値</param>
        /// <returns>なし</returns>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel取込(ワランティ消込)
        /// </history>
        public void ValidateImportList(List<JournalExcelImport> ImportList, FormCollection form)
        {
            //ReceiptPlan rpcondition = new ReceiptPlan();

            //入金予定リストを取得する
            //List<ReceiptPlan> receiptPlanList = new ReceiptPlanDao(db).GetListByCondition(rpcondition);

            //Journal jcondition = new Journal();

            //List<Journal> journalList = new JournalDao(db).GetListByCondition(jcondition);


            DateTime dtRet;

            string svDepartmentCode = "";
            string officeCode = "";
            string cashAccountCode = "";
            string accountType = "";
            string slipNumber = "";
            string customerClaimCode = "";
            string amount = "";

            //１行ずつチェック
            for (int i = 0; i < ImportList.Count; i++)
            {
                //初期化
                svDepartmentCode = "";
                officeCode = "";
                cashAccountCode = "";
                accountType = "";
                slipNumber = "";
                customerClaimCode = "";
                amount = "";

                //----------------
                //請求先コード
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].CustomerClaimCode))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目の請求先コードが入力されていません。請求先コード"));
                }
                else //マスタチェック
                {
                    CustomerClaim rec = new CustomerClaimDao(db).GetByKey(ImportList[i].CustomerClaimCode);

                    if (rec == null)
                    {
                        ModelState.AddModelError("", i + 1 + "行目の請求先コードはマスタに登録されていません");
                    }
                    else
                    {
                        //請求先コードを退避しておく
                        customerClaimCode = ImportList[i].CustomerClaimCode;
                    }
                }
                //----------------
                //伝票番号
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].SlipNumber))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目の伝票番号が入力されていません。伝票番号"));
                }
                else //マスタチェック
                {
                    ServiceSalesHeader sv = new ServiceSalesOrderDao(db).GetBySlipNumber(ImportList[i].SlipNumber);

                    if (sv == null)
                    {
                        ModelState.AddModelError("", i + 1 + "行目の伝票番号は存在しません");
                    }
                    else
                    {
                        //サービス伝票の伝票番号、部門コードを退避しておく
                        svDepartmentCode = sv.DepartmentCode;
                        slipNumber = ImportList[i].SlipNumber;
                    }
                }

                //----------------
                //入金種別
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].JournalType) || (!ImportList[i].JournalType.Equals("001") && !ImportList[i].JournalType.Equals("002")))
                {
                    ModelState.AddModelError("", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の入金種別には「001」または「002」を入力してください");
                }
                //----------------
                //部門コード
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].DepartmentCode))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の部門コードが入力されていません。部門コード"));
                }
                else //マスタチェック
                {
                    Department dep = new DepartmentDao(db).GetByKey(ImportList[i].DepartmentCode);

                    if (dep == null)
                    {
                        ModelState.AddModelError("", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の部門コードはマスタに登録されていません");
                    }
                }
                //----------------
                //口座種別
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].AccountType))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の口座種別が入力されていません。口座種別"));
                }
                else //マスタチェック
                {
                    c_AccountType rec = new CodeDao(db).GetAccountTypeByKey(ImportList[i].AccountType, false);

                    if (rec == null)
                    {
                        ModelState.AddModelError("", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の口座種別はマスタに登録されていません");
                    }
                    else
                    {
                        accountType = ImportList[i].AccountType;
                    }
                }
                //----------------
                //科目コード
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].AccountCode))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の科目コードが入力されていません。科目コード"));
                }
                else //マスタチェック
                {
                    Account account = new AccountDao(db).GetByKey(ImportList[i].AccountCode);

                    if (account == null)
                    {
                        ModelState.AddModelError("", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の科目コードはマスタに登録されていません");
                    }
                }
                //----------------
                //入金額
                //----------------
                if (!Regex.IsMatch(ImportList[i].Amount, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の入金額", "正の整数10桁以内" }));
                }
                else
                {
                    amount = ImportList[i].Amount;
                }
                //----------------
                //入金消込フラグ
                //----------------
                if (string.IsNullOrWhiteSpace(ImportList[i].ReceiptPlanFlag) || !ImportList[i].ReceiptPlanFlag.Equals("1"))
                {
                    ModelState.AddModelError("", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の入金消込フラグは「1」を入力して下さい");
                }
                //----------------
                //転記フラグ
                //----------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].TransferFlag) && !ImportList[i].TransferFlag.Equals("0") && !ImportList[i].TransferFlag.Equals("1"))
                {
                    ModelState.AddModelError("", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の転記フラグは「0」または「1」を入力して下さい");
                }
                //----------------
                //事業所コード
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].OfficeCode))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の事業所コードが入力されていません。事業所コード"));
                }
                else //マスタチェック
                {
                    Office office = new OfficeDao(db).GetByKey(ImportList[i].OfficeCode);

                    if (office == null)
                    {
                        ModelState.AddModelError("", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の事業所コードはマスタに登録されていません");
                    }
                    else
                    {
                        officeCode = ImportList[i].OfficeCode;
                    }
                }
                /*
                //----------------
                //現金口座コード
                //----------------
                if(!string.IsNullOrEmpty(ImportList[i].CashAccountCode)) //マスタチェック
                {
                    //事業所コードがマスタ登録されている場合
                    if (!string.IsNullOrWhiteSpace(officeCode))
                    {
                        CashAccount cashAccount = new CashAccountDao(db).GetByKey(officeCode, ImportList[i].CashAccountCode);

                        if (cashAccount == null)
                        {
                            ModelState.AddModelError("", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の現金口座コードはマスタに登録されていません");
                        }
                        else
                        {
                            cashAccountCode = ImportList[i].CashAccountCode;
                        }
                    }
                }
                */
                //----------------
                //入金日
                //----------------
                if (!DateTime.TryParse(ImportList[i].JournalDate, out dtRet)) //フォーマットチェック
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0003", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の入金日が日付の形式ではありません。入金日"));
                }
                else //締チェック
                {
                    if (!string.IsNullOrWhiteSpace(svDepartmentCode))
                    {
                        //--------------------
                        //現金締め
                        //--------------------
                        //入力された事業所コードがマスタ登録されている場合
                        if (!string.IsNullOrWhiteSpace(officeCode) && !string.IsNullOrWhiteSpace(cashAccountCode))
                        {
                            // 最後の現金締め処理日を取得
                            CashBalance cashBalance = GetLatestClosedData(officeCode, cashAccountCode);

                            // 伝票日付が締日よりも前だったら締め処理済み扱い（現金）
                            if (!string.IsNullOrWhiteSpace(accountType) && accountType.Equals("001"))
                            {
                                if (cashBalance != null && dtRet <= cashBalance.ClosedDate)
                                {
                                    ModelState.AddModelError("", MessageUtils.GetMessage("E0013", new string[] { i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")のの入金日", "前回の現金締め日より後" }));
                                }
                            }
                        
                        }

                        //ログインユーザ情報取得
                        Employee loginUser = ((Employee)Session["Employee"]);
                        ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "EditTempClosedData"); //仮締データ編集権限があるかないか

                        //仮締データ編集権限があれば本締めの時のみエラー
                        if (AppRole.EnableFlag)
                        {
                            if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(svDepartmentCode, dtRet, "001"))
                            {
                                ModelState.AddModelError("", (MessageUtils.GetMessage("E0013", new string[] { i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の入金日", "前回の月次締め処理が実行された年月より後" })));
                            }
                        }
                        else
                        {
                            // そうでなければ仮締め以上でエラー
                            if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(svDepartmentCode, dtRet, "001"))
                            {
                                ModelState.AddModelError("", (MessageUtils.GetMessage("E0013", new string[] { i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の入金日", "前回の月次締め処理が実行された年月より後" })));
                            }
                        }
                    }
                }

                //-----------------
                //入金予定チェック
                //-----------------
                if (!string.IsNullOrWhiteSpace(slipNumber) && !string.IsNullOrWhiteSpace(customerClaimCode))
                {
                    List<ReceiptPlan> rpList = new ReceiptPlanDao(db).GetListByCustomerClaim(slipNumber, customerClaimCode);
                    //List<ReceiptPlan> rpList = receiptPlanList.Where(x => !string.IsNullOrWhiteSpace(x.SlipNumber) && x.SlipNumber.Equals(slipNumber) && !string.IsNullOrWhiteSpace(x.CustomerClaimCode) && Strings.StrConv(x.CustomerClaimCode, VbStrConv.Narrow, 0x0411).ToUpper().Equals(customerClaimCode)).ToList();

                    if (rpList == null || rpList.Count == 0)
                    {
                        ModelState.AddModelError("", i + 1 + "行目の伝票番号(" + slipNumber + ")、請求先(" + customerClaimCode + ")に対する請求が存在しないため、消込できません");
                    }
                    else if (rpList.Count > 1)
                    {
                        ModelState.AddModelError("", i + 1 + "行目の伝票番号(" + slipNumber + ")、請求先(" + customerClaimCode + ")に対する請求が複数存在しているため、消込できません");
                    }
                    else
                    {
                        //入金予定の請求額と入金額が異なっている場合はエラー
                        if (!string.IsNullOrWhiteSpace(amount) && rpList[0].Amount != decimal.Parse(amount))
                        {
                            ModelState.AddModelError("", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の金額が請求額と一致しません");
                        }
                    }
                }

                //------------------
                //入金実績チェック
                //------------------
                if(!string.IsNullOrWhiteSpace(slipNumber) && !string.IsNullOrWhiteSpace(customerClaimCode))
                {
                    Journal journal = new JournalDao(db).GetListByCustomerAndSlip(slipNumber, customerClaimCode).FirstOrDefault();

                    //Journal journal = journalList.Where(x => !string.IsNullOrWhiteSpace(x.SlipNumber) && x.SlipNumber.Equals(slipNumber) && !string.IsNullOrWhiteSpace(x.CustomerClaimCode) && Strings.StrConv(x.CustomerClaimCode, VbStrConv.Narrow, 0x0411).ToUpper().Equals(customerClaimCode)).FirstOrDefault();

                    //既に実績が存在している場合
                    if (journal != null)
                    {
                        ModelState.AddModelError("", i + 1 + "行目(伝票番号：(" + slipNumber + "、請求先：" + customerClaimCode + ")の入金実績は既に登録されています");
                    }
                }

            }

            //-----------------
            //重複チェック
            //-----------------
            var ret = ImportList.GroupBy(x => new { SlipNumber = x.SlipNumber, CustomerClaimeCode = x.CustomerClaimCode }).Select(c => new { SlipNumber = c.Key.SlipNumber, CustomerClaimCode = c.Key.CustomerClaimeCode,  Count = c.Count() }).Where(c => c.Count > 1);

            foreach (var a in ret)
            {
                if (!string.IsNullOrWhiteSpace(a.SlipNumber) && !string.IsNullOrWhiteSpace(a.CustomerClaimCode))
                {
                    ModelState.AddModelError("", "取込むファイルの中に同一の伝票番号、請求先(伝票番号：" + a.SlipNumber + "、請求先：" + a.CustomerClaimCode + ")が複数定義されています");
                }
            }
        }
        #endregion

        #region Excelの読み取り結果をリストに設定する
        /// <summary>
        /// 結果をリストに設定する
        /// </summary>
        /// <param name="Result">Excelセルの値</param>
        /// <param name="ImportList"></param>
        /// <returns></returns>
        /// <history>
        ///  2018/05/28 arc yano #3886 Excel取込(ワランティ消込)
        /// </history>
        public List<JournalExcelImport> SetProperty(ref string[] Result, ref List<JournalExcelImport> ImportList)
        {
            JournalExcelImport SetLine = new JournalExcelImport();

            //入金種別 
            SetLine.JournalType = Result[0];
            //部門コード
            SetLine.DepartmentCode = Result[1];
            //請求先コード
            SetLine.CustomerClaimCode = Result[2];
            //伝票番号
            SetLine.SlipNumber = Result[3];
            //入金日
            SetLine.JournalDate = Result[4];
            //口座種別
            SetLine.AccountType = Result[5];
            //口座コード
            SetLine.AccountCode = Result[6];
            //入金額
            SetLine.Amount = Result[7];
            //入金消込フラグ
            SetLine.ReceiptPlanFlag = Result[8];
            //転記フラグ
            SetLine.TransferFlag = Result[9];
            //事業所コード
            SetLine.OfficeCode = Result[10];

            //現金口座コード
            //SetLine.CashAccountCode = Result[11];
           
            ImportList.Add(SetLine);

            return ImportList;
        }
        #endregion

        #region ダイアログのデータ付きコンポーネント設定
        /// <summary>
        /// ダイアログのデータ付きコンポーネント設定
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel取込(ワランティ消込)
        /// </history>
        private void SetDialogDataComponent(FormCollection form)
        {
            ViewData["ErrFlag"] = form["ErrFlag"];
            ViewData["RequestFlag"] = form["RequestFlag"];
            ViewData["Summary"] = form["Summary"];
        }
        #endregion

        #region 読み込んだデータをDBに登録
        /// <summary>
        /// DB更新
        /// </summary>
        /// <returns>戻り値(0:正常 1:エラー(ワランティ一括取込画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel取込(ワランティ消込)
        /// </history>
        private void DBExecute(List<JournalExcelImport> ImportList, FormCollection form)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                List<CashAccount> caList = new CashAccountDao(db).GetListByOfficeCode("");

                List<Journal> workList = new List<Journal>();

                //-------------------------------
                //入金実績データの作成
                //-------------------------------
                foreach (var LineData in ImportList)
                {
                    Journal journal = new Journal();

                    CashAccount rec = caList.Where(x => x.OfficeCode.Equals(LineData.OfficeCode) && x.CashAccountName.Trim().Equals("ｻｰﾋﾞｽ売上")).FirstOrDefault();

                    string cashAccountCode = rec != null ? rec.CashAccountCode : "";

                    journal.JournalId = Guid.NewGuid();                                                         //入金実績ID
                    journal.JournalType = LineData.JournalType;                                                 //入金種別
                    journal.DepartmentCode = LineData.DepartmentCode;                                           //部門コード
                    journal.CustomerClaimCode = LineData.CustomerClaimCode;                                     //請求先コード
                    journal.SlipNumber = LineData.SlipNumber;                                                   //伝票番号
                    journal.JournalDate = DateTime.Parse(LineData.JournalDate);                                 //入金日
                    journal.AccountType = LineData.AccountType;                                                 //口座種別
                    journal.AccountCode = LineData.AccountCode;                                                 //科目コード
                    journal.Amount = decimal.Parse(LineData.Amount);                                            //金額
                    journal.Summary = form["Summary"];                                                          //摘要
                    journal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                  //作成者
                    journal.CreateDate = DateTime.Now;                                                          //作成日
                    journal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;              //最終更新者
                    journal.LastUpdateDate = DateTime.Now;                                                      //最終更新日
                    journal.DelFlag = "0";                                                                      //削除フラグ　※「0」固定
                    journal.ReceiptPlanFlag = LineData.ReceiptPlanFlag;                                         //入金消込フラグ
                    journal.TransferFlag = LineData.TransferFlag;                                               //転記フラグ
                    journal.OfficeCode = LineData.OfficeCode;                                                   //事業所コード
                    journal.CashAccountCode = cashAccountCode;                                                  //現金口座コード
                    journal.PaymentKindCode = "";                                                               //支払種別コード　※「空文字」固定
                    journal.CreditReceiptPlanId = "";                                                           //クレジットタイプの入金予定ID　※「空文字」固定
                    journal.TradeVin = "";                                                                      //下取車台番号　※「空文字」固定

                    workList.Add(journal);
                }

                db.Journal.InsertAllOnSubmit(workList);

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

        #region Ajax
        public ActionResult GetReceiptBalance(string slipNumber, string customerClaimCode) {
            if (Request.IsAjaxRequest()) {
                decimal balance = new ReceiptPlanDao(db).GetCashAmountByCustomerClaim(slipNumber, customerClaimCode);
                return Json(balance);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 請求先タイプリストの取得
        /// </summary>
        /// <param name="customerClaimFilter">請求先種別フィルタ</param>
        /// <returns>リスト</returns>
        public ActionResult CustomerClaimTypeByCustomerClaimFilter(string customerClaimFilter)
        {
            if (Request.IsAjaxRequest())
            {
                List<c_CustomerClaimType> list = new CodeDao(db).GetCustomerClaimTypeAll(false, customerClaimFilter);

                CodeDataList clist = new CodeDataList();
                clist.DataList = new List<CodeData>();

                foreach (var l in list)
                {
                    CodeData data = new CodeData();

                    data.Code = l.Code;
                    data.Name = l.Name;

                    clist.DataList.Add(data);
                }

                return Json(clist);
            }
            return new EmptyResult();
        }
        #endregion


        #region 入金予定額の少ない順に入金を行う
        /// <summary>
        /// 入金予定額の少ない順に入金を行う（入金予定リスト, 入金額）
        /// </summary>
        /// <param name="ReceiptPlanList">入金予定リスト</param>
        /// <param name="JournalAmount">入金額</param>
        /// <returns></returns>
        /// <history>
        /// 2017/12/12 arc yano #3818 実績がプラスの場合はプラスの予定を、マイナスの場合はマイナスの予定を消し込む
        /// 2016/06/29 arc nakayama #3593_伝票に対して同一請求先が複数あった場合の考慮
        /// </history>
        private void Payment(List<ReceiptPlan> ReceiptPlanList, decimal JournalAmount)
        {
            decimal remainAmount = JournalAmount; //入金額

            List<ReceiptPlan> targetList = new List<ReceiptPlan>();

            if (JournalAmount > 0)
            {
                targetList = ReceiptPlanList.Where(x => x.Amount > 0).ToList();
            }
            else
            {
                targetList = ReceiptPlanList.Where(x => x.Amount <= 0).ToList();
            }

            //件数が0の場合、入金予定リストをそのままセット
            if (targetList.Count == 0)
            {
                targetList = ReceiptPlanList;
            }

            foreach (var plan in targetList)
            {
                if (plan.CompleteFlag == "0")
                {
                    //残高 - 入金額 = 0 ならそのまま減算する
                    if (decimal.Subtract(plan.ReceivableBalance ?? 0m ,remainAmount).Equals(0m))
                    {
                        plan.ReceivableBalance = decimal.Subtract(plan.ReceivableBalance ?? 0m, remainAmount);
                        remainAmount = 0m;
                    }
                    else
                    {
                        //そうでなければ、残高分全て引くため、入金額 = 入金額 - 残高　をして残高分全て減算したことにする
                        remainAmount = decimal.Subtract(remainAmount, plan.ReceivableBalance ?? 0m);

                        //残高がプラスの値の時↓
                        if (plan.ReceivableBalance > 0)
                        {
                            //入金額 - 残高　= マイナスの値になっていたら引きすぎているので↓
                            if (remainAmount < 0)
                            {
                                //入金額 = 入金額 + 残高 をして残りの入金額を求める↓
                                remainAmount = remainAmount + plan.ReceivableBalance ?? 0m;
                                //残高から求めた入金額分減算する
                                plan.ReceivableBalance = decimal.Subtract(plan.ReceivableBalance ?? 0m, remainAmount);
                                remainAmount = decimal.Subtract(remainAmount, plan.ReceivableBalance ?? 0m);
                            }
                            else
                            {
                                //そうでなければ、まだ入金額が残っているが該当の入金予定の残高いっぱいまで減算したことになるので残高を0に更新する
                                plan.ReceivableBalance = 0;
                            }
                        }
                        else //残高がマイナスの値の時↓
                        {
                            //入金額 - 残高　= プラスの値になっていたら引きすぎているので↓
                            if (remainAmount > 0)
                            {
                                //入金額 = 入金額 + 残高 をして残りの入金額を求める↓
                                remainAmount = remainAmount + plan.ReceivableBalance ?? 0m;
                                //残高から求めた入金額分減算する
                                plan.ReceivableBalance = decimal.Subtract(plan.ReceivableBalance ?? 0m, remainAmount);
                            }
                            else
                            {
                                //そうでなければ、まだ入金額が残っているが該当の入金予定の残高いっぱいまで減算したことになるので残高を0に更新する
                                plan.ReceivableBalance = 0;
                                remainAmount = decimal.Subtract(remainAmount, plan.ReceivableBalance ?? 0m);
                            }
                        }
                    }


                    //最後に残高が０になっていたら入金完了フラグを立てる
                    if (plan.ReceivableBalance.Equals(0m))
                    {
                        plan.CompleteFlag = "1";
                    }
                    plan.LastUpdateDate = DateTime.Now; //最終更新日
                    plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;　//最終更新者
                }
                //入金額が０になっていたら、抜ける
                if (remainAmount.Equals(0m))
                {
                    break;
                }
            }

        }
        #endregion
    }
}
