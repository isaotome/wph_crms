using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CrmsDao;
using System.Data.Linq;
using System.Transactions;
using Crms.Models;     

namespace Crms.Controllers
{
    /// <summary>
    /// 入金実績振替コントロールクラス
    /// </summary>
    /// <hisotory>
    /// 2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
    /// </hisotory>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class JournalTransferController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "入金実績振替";                   // 画面名
        private static readonly string PROC_NAME_CRITERIA = "入金検索";              // 処理名(検索処理)
        private static readonly string PROC_NAME_EXECTRANSFER = "入金振替";          // 処理名(振替実行)

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
        public JournalTransferController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 入金実績振替画面表示
        /// </summary>
        /// <returns>入金実績振替画面</returns>
        /// <history>
        /// 2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
        /// </history>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;

            FormCollection form = new FormCollection();

            //初期化
            form["RequestFlag"] = "99";

            return Criteria(form, null);
        }

        /// <summary>
        /// 入金実績振替画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>入金実績振替画面</returns>
        /// <history>
        /// 2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form, List<JournalTransfer> line)
        {
            ModelState.Clear();

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            //返却値の設定
            JournalTransferList data = new JournalTransferList(criteriaInit);

            ActionResult ret = View("JournalTransferCriteria", data);

            //--------------------------------------
            //ReuquestFlagによる処理の振分け
            //--------------------------------------
            switch (form["RequestFlag"])
            {
                case "1":   //振替実行
                    ret = ExecTransfer(form, line);
                    break;

                default: //検索、ページング
                    
                    //初期表示以外の場合のみ検索
                    if (!criteriaInit)
                    {
                        ret = GetSearchResultList(form);
                    }
                    break;
            }

            return ret;
        }

        /// <summary>
        /// 入金実績振替履歴の表示
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>振替履歴一覧</returns>
        /// <history>
        /// 2018/01/23 arc yano  #3836 入金実績振替機能移行
        /// </history>
        [ChildActionOnly]
        [OutputCache(Duration = 1)]
        public ActionResult JournalChangeList(FormCollection form)
        {
            List<W_JournalChange> list = new W_JournalChangeDao(db).GetQueryable().ToList();

            return PartialView("_JournalChangeList", list);
        }

        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>入金実績リスト</returns>
        /// <history>
        ///  2018/01/23 arc yano  #3836 入金実績振替機能移行 新規作成
        /// </history>
        private ActionResult GetSearchResultList(FormCollection form)
        {
            //Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_CRITERIA);

            JournalTransferList data = new JournalTransferList(criteriaInit);

            data.list = new JournalDao(db).GetJournalTrasnferList(form["SlipNumber"]);

            SetComponent(form);

            return View("JournalTransferCriteria", data);
        }

        /// <summary>
        /// 振替実行
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="journalTransfer">入金実績</param>
        /// <returns>入金実績振替画面</returns>
        /// <history>
        /// 2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
        /// </history>
        private ActionResult ExecTransfer(FormCollection form, List<JournalTransfer> list)
        {
            //Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXECTRANSFER);

            JournalTransferList data = new JournalTransferList(criteriaInit);

            string slipNumber = "";

            bool traderPlan = false;

            string tradeSlipNumber = "";

            //振替処理
            using (TransactionScope ts = new TransactionScope())
            {    
                //-----------------------------
                //入金実績の更新
                //-----------------------------
                foreach (var l in list)
                {
                    //チェックONのレコードの更新を行う
                    if (l.TransferCheck)
                    {
                        //入力値チェック
                        ValidateTransfer(l, list.IndexOf(l));

                        //validationエラーの場合
                        if (!ModelState.IsValid)
                        {
                            //以下の処理は行わない
                            continue;
                        }

                        Journal journal = EditJournalForuUpdate(l);

                        //振替を行う実績が「下取車」「残債」の場合
                        if (journal.AccountType.Equals("012") || journal.AccountType.Equals("013"))
                        {
                            //下取・残債の入金予定更新フラグ
                            traderPlan = true;
                            //振替先の伝票番号の退避
                            tradeSlipNumber = l.TransferStlipNumber;
                        }

                        //振替先の伝票番号を退避しておく
                        slipNumber = l.TransferStlipNumber;

                        //振替履歴の追加
                        W_JournalChange history = EditJournalChangeForuInsert(l);
                    }
                }

                //validationエラーの場合
                if (!ModelState.IsValid)
                {
                    SetComponent(form);

                    data.list = list;

                    return View("JournalTransferCriteria", data);
                }

                //-----------------------------
                //入金予定の更新
                //-----------------------------
                // 車両伝票
                CarSalesHeader carSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(list[0].SlipNumber);

                // 振替元の伝票が車両伝票の場合
                if (carSalesHeader != null)
                {
                    //伝票ステータス = 「キャンセル」「受注後キャンセル」の場合
                    if (carSalesHeader.SalesOrderStatus.Equals("006") || carSalesHeader.SalesOrderStatus.Equals("007"))
                    {
                        CreateCarReceiptPlan(carSalesHeader);
                    }
                    else
                    {
                        //振替を行った実績の中に「下取車」「残債」が含まれていた場合
                        if (traderPlan)
                        {
                            //振替元の下取車・残債の実績を再作成
                            CreateTradeReceiptPlan(carSalesHeader);

                            //振替先の下取車・残債の入金予定を更新
                            CarSalesHeader transferCarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(tradeSlipNumber);

                            //伝票が存在した場合は入金予定を再作成
                            if (transferCarSalesHeader != null)
                            {
                                CreateTradeReceiptPlan(transferCarSalesHeader);
                            }
                        }
                    }
                }
                else
                {
                    ServiceSalesHeader serviceSalesheader = new ServiceSalesOrderDao(db).GetBySlipNumber(list[0].SlipNumber);
                    
                    //振替元の伝票がサービス伝票で伝票ステータス = 「キャンセル」の場合
                    if (serviceSalesheader != null && serviceSalesheader.ServiceOrderStatus.Equals("007"))
                    {
                        CreateServiceReceiptPlan(serviceSalesheader);
                    }
                }

                for (int j = 0; j < DaoConst.MAX_RETRY_COUNT; j++)
                {
                    try
                    {
                        db.SubmitChanges();
                        // トランザクションのコミット
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_EXECTRANSFER, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME_EXECTRANSFER, FORM_NAME, "");
                        return View("Error");
                    }
                }
            }

            //再検索

            list = new JournalDao(db).GetJournalTrasnferList(slipNumber);

            data.list = list;

            //検索条件の伝票番号を振替先伝票番号に変更
            form["slipNumber"] = slipNumber;

            SetComponent(form);

            return View("JournalTransferCriteria", data);
        }

       
        /// <summary>
        /// 入金実績編集
        /// </summary>
        /// <param name="journalTransfer">振替実績</param>
        /// <returns>入金実績</returns>
        /// <history>
        ///  2018/01/23 arc yano  #3836 入金実績振替機能移行 新規作成
        /// </history>
        private Journal EditJournalForuUpdate(JournalTransfer journalTransfer)
        {
            Journal journal = new JournalDao(db).GetByKey(journalTransfer.JournalId);

            //伝票番号を振替先伝票番号に更新
            journal.SlipNumber = journalTransfer.TransferStlipNumber;
            //更新者
            journal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //更新日
            journal.LastUpdateDate = DateTime.Now;

            //更新
            db.SubmitChanges();

            return journal;
        }

        /// <summary>
        /// 振替履歴の作成
        /// </summary>
        /// <param name="journalTransfer">振替履歴</param>
        /// <returns>振替履歴のレコード</returns>
        /// <history>
        ///  2018/01/23 arc yano  #3836 入金実績振替機能移行 新規作成
        /// </history>
        private W_JournalChange EditJournalChangeForuInsert(JournalTransfer journalTransfer)
        {

            //履歴を作成
            W_JournalChange rec = new W_JournalChange();

            //履歴ID
            rec.JournalChangeId = Guid.NewGuid();
            //入金実績ID
            rec.JournalId = journalTransfer.JournalId;
            //伝票番号(振替元)
            rec.SlipNumber_old = journalTransfer.SlipNumber;
            //伝票番号(振替先)
            rec.SlipNumber_new = journalTransfer.TransferStlipNumber;
            //更新者
            rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //更新日
            rec.LastUpdateDate = DateTime.Now;

            //インサート処理
            db.W_JournalChange.InsertOnSubmit(rec);

            return rec;
        }


        #region validationチェック
        /// <summary>
        /// 振替時のチェック
        /// </summary>
        /// <param name="journalTransfer">振替実績</param>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/03/23 arc yano #3874 入金実績振替　仮締め時に仮締めデータ編集権限のあるユーザで振替を実行できない
        /// 2018/01/23 arc yano  #3836 入金実績振替機能移行 新規作成
        /// </history>
        private void ValidateTransfer(JournalTransfer line, int index)
        {
            //必須チェック
            if (string.IsNullOrWhiteSpace(line.TransferStlipNumber))
            {
                ModelState.AddModelError("line[" + index + "].TransferStlipNumber", MessageUtils.GetMessage("E0001", (index + 1) + "行目の伝票番号"));
            }
            else
            {
                //存在チェック
                bool ret = new CarSalesOrderDao(db).IsExistSlipNumber(line.TransferStlipNumber);

                //伝票が存在しない場合
                if (!ret)
                {
                    ModelState.AddModelError("line[" + index + "].TransferStlipNumber", (index + 1) + "行目の入金実績の振替先伝票が存在しないため、振替を行うことはできません");
                }
                else
                {
                    //振替元伝票と振替先伝票の種類が異なる場合(例：振替元…サービス伝票、振替先…車両伝票)はエラーとなる
                    //振替元                
                    V_ALL_SalesOrderList source = new V_ALL_SalesOrderListDao(db).GetByKey(line.SlipNumber);
                    //振替先
                    V_ALL_SalesOrderList dist = new V_ALL_SalesOrderListDao(db).GetByKey(line.TransferStlipNumber);

                    if (!source.STD.Equals(dist.STD))
                    {
                        if (source.STD.Equals("0"))
                        {
                            ModelState.AddModelError("line[" + index + "].TransferStlipNumber", (index + 1) + "行目の入金実績を車両伝票からサービス伝票へ振替を行うことはできません");
                        }
                        else
                        {
                            ModelState.AddModelError("line[" + index + "].TransferStlipNumber", (index + 1) + "行目の入金実績をサービス伝票から車両伝票へ振替を行うことはできません");
                        }
                    }
                }
            }

            // 経理締めチェック
            if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(line.DepartmentCode, line.JournalDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))         //Mod 2018/03/23 arc yano #3874
            {
                ModelState.AddModelError("", "経理締が行われているため、" + (index + 1) + "行目の入金実績は振替を行うことはできません");
            }   
        }
        #endregion

        #region 画面コンポーネント設定
        /// <summary>
        /// 画面コンポーネント設定
        /// </summary>
        /// <param name="form">フォーム値</param>
        /// <returns></returns>
        /// <history>
        /// 2018/01/23 arc yano  #3836 入金実績振替機能移行 新規作成
        /// </history>
        private void SetComponent(FormCollection form)
        {
            //伝票番号
            ViewData["SlipNumber"] = form["SlipNumber"];
        }
        #endregion


        #region 入金予定再作成
        /// <summary>
        ///  車両伝票の入金予定再作成 
        ///  キャンセル、受注後キャンセル伝票の入金予定の再作成
        ///  通常の伝票の入金予定は再作成しない
        /// </summary>
        /// <param name="header">車両伝票ヘッダ</param>
        /// <history>
        ///  2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
        /// </history>
        private void CreateCarReceiptPlan(CarSalesHeader header)
        {
            //入金予定を削除
            List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
            foreach (var p in planList)
            {
                p.DelFlag = "1";
                p.LastUpdateDate = DateTime.Now;
                p.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            db.SubmitChanges();

            //実績を取得
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber);

            foreach (var j in journalList)
            {
                // 実績分の入金予定を作成
                ReceiptPlan plusPlan = new ReceiptPlan();
                plusPlan.ReceiptPlanId = Guid.NewGuid();
                plusPlan.DepartmentCode = j.DepartmentCode;
                plusPlan.OccurredDepartmentCode = j.DepartmentCode;
                plusPlan.CustomerClaimCode = j.CustomerClaimCode;
                plusPlan.SlipNumber = j.SlipNumber;
                plusPlan.ReceiptType = j.AccountType;
                plusPlan.ReceiptPlanDate = DateTime.Today;
                plusPlan.AccountCode = j.AccountCode;
                plusPlan.Amount = j.Amount;
                plusPlan.ReceivableBalance = 0;             //残高 = 0(固定)
                plusPlan.CompleteFlag = "1";                //入金完了フラグ = 0(固定)
                plusPlan.CreateDate = DateTime.Now;
                plusPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plusPlan.LastUpdateDate = DateTime.Now;
                plusPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plusPlan.DelFlag = "0";
                plusPlan.JournalDate = DateTime.Today;
                plusPlan.DepositFlag = "0";
                db.ReceiptPlan.InsertOnSubmit(plusPlan);

                // 過入金分のマイナス入金予定を作成
                ReceiptPlan minusPlan = new ReceiptPlan();
                minusPlan.ReceiptPlanId = Guid.NewGuid();
                minusPlan.DepartmentCode = j.DepartmentCode;
                minusPlan.OccurredDepartmentCode = j.DepartmentCode;
                minusPlan.CustomerClaimCode = j.CustomerClaimCode;
                minusPlan.SlipNumber = j.SlipNumber;
                minusPlan.ReceiptType = "001";
                minusPlan.ReceiptPlanDate = DateTime.Today;
                minusPlan.AccountCode = j.AccountCode;
                minusPlan.Amount = -1 * j.Amount;
                minusPlan.ReceivableBalance = -1 * j.Amount;
                minusPlan.CompleteFlag = "0";
                minusPlan.CreateDate = DateTime.Now;
                minusPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                minusPlan.LastUpdateDate = DateTime.Now;
                minusPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                minusPlan.DelFlag = "0";
                minusPlan.JournalDate = DateTime.Today;
                minusPlan.DepositFlag = "0";
                db.ReceiptPlan.InsertOnSubmit(minusPlan);
            }

            db.SubmitChanges();
        }

        /// <summary>
        ///  サービス伝票入金予定再作成
        ///  キャンセル伝票の入金予定の再作成
        ///  通常の伝票の入金予定は再作成しない
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <history>
        ///  2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
        /// </history>
        private void CreateServiceReceiptPlan(ServiceSalesHeader header)
        {
            List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);

            foreach (var p in planList)
            {
                p.DelFlag = "1";
                p.LastUpdateDate = DateTime.Now;
                p.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            db.SubmitChanges();

            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber);

            //入金済みがあれば実績分のマイナスの入金予定を作成する
            if (journalList != null && journalList.Count() > 0)
            {
                //マイナスの入金予定の再作成
                foreach (var j in journalList)
                {
                    ReceiptPlan plan = new ReceiptPlan();
                    plan.ReceiptPlanId = Guid.NewGuid();
                    plan.DepartmentCode = j.DepartmentCode;
                    plan.OccurredDepartmentCode = j.DepartmentCode;
                    plan.CustomerClaimCode = j.CustomerClaimCode;
                    plan.SlipNumber = j.SlipNumber;
                    plan.ReceiptType = "001";                                               //「現金」固定
                    plan.ReceiptPlanDate = j.JournalDate;
                    plan.AccountCode = j.AccountCode;
                    plan.Amount = 0;                                                        //「0」固定
                    plan.ReceivableBalance = j.Amount * (-1);
                    plan.CompleteFlag = "0";                                                //「0」固定
                    plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; ;
                    plan.CreateDate = DateTime.Now;
                    plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; ;
                    plan.LastUpdateDate = DateTime.Now;
                    plan.DelFlag = "0";                                                     //「0」固定
                    plan.Summary = j.Summary;
                    plan.JournalDate = null;
                    plan.DepositFlag = null;                                                //諸費用フラグ = null
                    plan.PaymentKindCode = j.PaymentKindCode;
                    plan.CommissionRate = null;                                             //手数料率 = null
                    plan.CommissionAmount = null;                                           //手数料 = null
                    db.ReceiptPlan.InsertOnSubmit(plan);
                }
            }

            db.SubmitChanges();
        }

        /// <summary>
        /// 下取車に関する入金予定の作成
        /// </summary>
        /// <param name="header">車両伝票ヘッダ</param>
        /// <history>
        /// 2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
        /// </history>
        private void CreateTradeReceiptPlan(CarSalesHeader header)
        {
            //下取、残債の既存の入金予定の削除
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetListByslipNumber(header.SlipNumber).Where(x => x.ReceiptType == "012" || x.ReceiptType == "013").ToList();

            foreach (var d in delList)
            {
                d.DelFlag = "1";
                d.LastUpdateDate = DateTime.Now;
                d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            db.SubmitChanges();

            //科目データ取得
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null)
            {
                ModelState.AddModelError("", "科目設定が正しくありません。システム管理者に連絡して下さい。");
                return;
            }

            for (int i = 1; i <= 3; i++)
            {
                object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                {
                    string TradeInAmount = "0";
                    string TradeInRemainDebt = "0";

                    var varTradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i);
                    if (varTradeInAmount != null && !string.IsNullOrEmpty(varTradeInAmount.ToString()))
                    {
                        TradeInAmount = varTradeInAmount.ToString();
                    }

                    var varTradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                    if (varTradeInRemainDebt != null && !string.IsNullOrEmpty(varTradeInRemainDebt.ToString()))
                    {
                        TradeInRemainDebt = varTradeInRemainDebt.ToString();
                    }

                    decimal PlanAmount = decimal.Parse(TradeInAmount);
                    decimal PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);
                    decimal JournalAmount = 0; //下取の入金額
                    decimal JournalDebtAmount = 0; //残債の入金額

                    //下取の入金額取得
                    Journal JournalData = new JournalDao(db).GetTradeJournal(header.SlipNumber, "013", vin.ToString()).FirstOrDefault();
                    if (JournalData != null)
                    {
                        JournalAmount = JournalData.Amount;
                    }

                    //残債の入金額取得
                    Journal JournalData2 = new JournalDao(db).GetTradeJournal(header.SlipNumber, "012", vin.ToString()).FirstOrDefault();                   
                    if (JournalData2 != null)
                    {
                        JournalDebtAmount = JournalData2.Amount;
                    }

                    //下取の入金予定作成
                    ReceiptPlan TradePlan = new ReceiptPlan();
                    TradePlan.ReceiptPlanId = Guid.NewGuid();
                    TradePlan.DepartmentCode = header.DepartmentCode;
                    TradePlan.OccurredDepartmentCode = header.DepartmentCode;
                    TradePlan.CustomerClaimCode = header.CustomerCode;
                    TradePlan.SlipNumber = header.SlipNumber;
                    TradePlan.ReceiptType = "013";                                  //下取
                    TradePlan.ReceiptPlanDate = null;
                    TradePlan.AccountCode = carAccount.AccountCode;
                    TradePlan.Amount = decimal.Parse(TradeInAmount);
                    TradePlan.ReceivableBalance = decimal.Subtract(TradePlan.Amount ?? 0m, JournalAmount); //☆計算
                    if (TradePlan.ReceivableBalance == 0m)
                    {
                        TradePlan.CompleteFlag = "1";
                    }
                    else
                    {
                        TradePlan.CompleteFlag = "0";
                    }
                    TradePlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    TradePlan.CreateDate = DateTime.Now;
                    TradePlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    TradePlan.LastUpdateDate = DateTime.Now;
                    TradePlan.DelFlag = "0";
                    TradePlan.Summary = "";
                    TradePlan.JournalDate = null;
                    TradePlan.DepositFlag = "0";
                    TradePlan.PaymentKindCode = "";
                    TradePlan.CommissionRate = null;
                    TradePlan.CommissionAmount = null;
                    TradePlan.CreditJournalId = "";
                    TradePlan.TradeVin = vin.ToString();

                    db.ReceiptPlan.InsertOnSubmit(TradePlan);

                    //残債があった場合残債分の入金予定をマイナスで作成する
                    if (!string.IsNullOrEmpty(TradeInRemainDebt))
                    {
                        ReceiptPlan RemainDebtPlan = new ReceiptPlan();
                        RemainDebtPlan.ReceiptPlanId = Guid.NewGuid();
                        RemainDebtPlan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //残債は経理に振り替え
                        RemainDebtPlan.OccurredDepartmentCode = header.DepartmentCode;
                        RemainDebtPlan.CustomerClaimCode = header.CustomerCode;
                        RemainDebtPlan.SlipNumber = header.SlipNumber;
                        RemainDebtPlan.ReceiptType = "012";                                                                 //残債
                        RemainDebtPlan.ReceiptPlanDate = null;
                        RemainDebtPlan.AccountCode = carAccount.AccountCode;
                        RemainDebtPlan.Amount = PlanRemainDebt;
                        RemainDebtPlan.ReceivableBalance = decimal.Subtract(PlanRemainDebt, JournalDebtAmount); //計算
                        if (RemainDebtPlan.ReceivableBalance == 0m)
                        {
                            RemainDebtPlan.CompleteFlag = "1";
                        }
                        else
                        {
                            RemainDebtPlan.CompleteFlag = "0";
                        }
                        RemainDebtPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        RemainDebtPlan.CreateDate = DateTime.Now;
                        RemainDebtPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        RemainDebtPlan.LastUpdateDate = DateTime.Now;
                        RemainDebtPlan.DelFlag = "0";
                        RemainDebtPlan.Summary = "";
                        RemainDebtPlan.JournalDate = null;
                        RemainDebtPlan.DepositFlag = "0";
                        RemainDebtPlan.PaymentKindCode = "";
                        RemainDebtPlan.CommissionRate = null;
                        RemainDebtPlan.CommissionAmount = null;
                        RemainDebtPlan.CreditJournalId = "";
                        RemainDebtPlan.TradeVin = vin.ToString();            //Add 2017/11/14 arc yano #3811

                        db.ReceiptPlan.InsertOnSubmit(RemainDebtPlan);
                    }
                }
            }

            db.SubmitChanges();
        }

        #endregion


        /// <summary>
        /// ページ番号取得
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>ページ番号</returns>
        /// <history>
        /// 2018/01/23 arc yano  #3836 入金実績振替機能移行 新規作成
        /// </history>
        private int getPageIndex(FormCollection form, int idindex)
        {

            int pageIndex = 0;
            string[] strPageIndex = null;

            if (form["id"] != null)
            {

                strPageIndex = (form["id"] ?? "").Split(',');

                pageIndex = int.Parse(strPageIndex[idindex]);
            }

            return pageIndex;
        }

        /// <summary>
        /// 伝票番号存在チェック
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>ページ番号</returns>
        /// <history>
        /// 2018/01/23 arc yano  #3836 入金実績振替機能移行 新規作成
        /// </history>
        public ActionResult IsExistSlip(string slipNumber)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                if (new CarSalesOrderDao(db).IsExistSlipNumber(slipNumber))
                {
                    codeData.Code = slipNumber;
                }

                return Json(codeData);
            }

            return new EmptyResult();
        }
    }
}
