using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;

//Add 2014/08/28 arc yano ログ出力の為に追加
using System.Reflection;
using System.Data.Linq;
using System.Data.SqlClient;
using Crms.Models;

//Add 2014/09/02 arc yano IPO対応その２
using System.Transactions;


namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class MonthlyController : Controller
    {

        //Add 2014/08/28 arc yano エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "月次締め処理状況";             // 画面名
        private static readonly string PROC_NAME = "月次締め処理状況作成・更新";   // 処理名
        
        /// <summary>
        /// データコンテキスト
        /// </summary>
        public CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MonthlyController() {
            db = new CrmsLinqDataContext();
        }

        // Add 2015/03/19 arc nakayama 伝票修正対応
        /// <summary>
        /// サービス伝票処理サービス
        /// </summary>
        private IServiceSalesOrderService service;

        /// <summary>
        /// 当月のステータス一覧を表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria() {
            FormCollection form = new FormCollection();
            form["TargetYear"] = DateTime.Today.Year.ToString().Substring(1,3);
            form["TargetMonth"] = DateTime.Today.Month.ToString().PadLeft(3, '0');
            form["TargetRange"] = "0";
            form["RequestFlag"] = "0";

            return Criteria(form);
        }

        /// <summary>
        /// 指定月のステータス一覧を表示
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2018/03/26 arc yano #3855 現金出納帳 口座の現金締め対象フラグの追加
        /// 2017/05/10 arc yano #3762 車両棚卸機能追加　車両棚卸状況を表示
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            CodeDao dao = new CodeDao(db);
            DateTime targetMonth = DateTime.Parse(dao.GetYear(true, form["TargetYear"]).Name + "/" + dao.GetMonth(true, form["TargetMonth"]).Name);
            List<MonthlyStatus> list = new List<MonthlyStatus>();
            List<Department> departmentList = new DepartmentDao(db).GetListAll();

            //Mod 2014/08/27 IPO対応その２ 月締め処理追加
            //締め処理実行
            if (form["RequestFlag"].Equals("1"))
            {
                int ret = ExecClose(form);

                if (ret == -1)// 作成・更新エラーの場合
                {
                    return View("Error");
                }
                else if (ret == 1) //作成・更新条件を満たしていない場合
                {
                    //---------------------------------------
                    //エンティティのロールバック
                    //---------------------------------------
                    //InventorySchedule
                    List<InventorySchedule> listInventorySchedule = new InventoryScheduleDao(db).GetListByKey(new DateTime(targetMonth.Year, targetMonth.Month, 1), "001");
                    //更新したエンティティを元に戻す
                    db.Refresh(RefreshMode.OverwriteCurrentValues, listInventorySchedule);
                }
            }
            
            foreach (Department d in departmentList)
            {
                //Add 2016/08/13 arc yano #3596 
                //部門から使用倉庫を割り出す
                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, d.DepartmentCode);
                
                //倉庫コードの設定
                string warehouseCode = "";
                if (dWarehouse != null)
                {
                    warehouseCode = dWarehouse.WarehouseCode;
                }
                
                MonthlyStatus status = new MonthlyStatus();

                //Mod 2018/03/26 arc yano #3855
                List<CashAccount> accountList = (new CashAccountDao(db).GetListByOfficeCode(d.OfficeCode) != null ? new CashAccountDao(db).GetListByOfficeCode(d.OfficeCode).Where(x => x.NonCloseTarget == null || !x.NonCloseTarget.Equals("1")).ToList() : new List<CashAccount>());
                //List<CashAccount> accountList = new CashAccountDao(db).GetListByOfficeCode(d.OfficeCode);
                foreach (var a in accountList)
                {
                    //Mod 2014/09/25 arc yano #3095 現金出在高データ取得方法の変更 指定月末時点の現金在高を取得
                    CashBalance balance = new CashBalanceDao(db).GetLastMonthClosedData2(d.OfficeCode, a.CashAccountCode, targetMonth);
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if (balance == null || !CommonUtils.DefaultString(balance.CloseFlag).Equals("1"))
                    {
                        status.CashBalance = null;
                        break;
                    }
                    status.CashBalance = balance;
                }
                status.InventorySchedule = new InventoryScheduleDao(db).GetByKey(d.DepartmentCode, new DateTime(targetMonth.Year, targetMonth.Month, 1), "001");
                // Mod 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)
                //status.InventoryPartsSchedule = new InventorySchedulePartsDao(db).GetByKey(d.DepartmentCode, new DateTime(targetMonth.Year, targetMonth.Month, 1), "002");
                status.InventoryPartsSchedule = new InventorySchedulePartsDao(db).GetByKey(warehouseCode, new DateTime(targetMonth.Year, targetMonth.Month, 1), "002");  //Mod 2016/08/13 arc yano #3596 

                status.InventoryCarSchedule = new InventoryScheduleCarDao(db).GetByKey(warehouseCode, new DateTime(targetMonth.Year, targetMonth.Month, 1));  //Add 2017/05/10 arc yano #3762
                
                status.Department = d;
                list.Add(status);                
            }
            SetDataComponent(form);
           
            return View("MonthlyCriteria", list);
        }

        //Add 2014/08/27 arc yano IPO対応その２ 月締め処理追加
        /// <summary>
        /// 締め処理実行
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表) 機能改善②　本締め時の車両管理スナップショットの保存処理の廃止
        /// 2016/11/30 #3659 車両管理項目追加 本締め実行時に車両管理データのスナップショットを保存する
        /// 2015/03/23 arc nakayama 伝票修正対応　本締めを行ったら伝票の修正情報を削除する(締日以前の納車日の伝票すべて)
        /// </history>
        //[AcceptVerbs(HttpVerbs.Post)]
        private int ExecClose(FormCollection form)
        {
            int chkret = 0;
            int upsret = 0;
            int ret = 0;

            CodeDao dao = new CodeDao(db);
            DateTime targetMonth = new DateTime(int.Parse(dao.GetYear(true, form["TargetYear"]).Name), int.Parse(dao.GetMonth(true, form["TargetMonth"]).Name), 1);
            
            string closeType = form["CloseType"];               //締め処理タイプ
            string targetClass = form["TargetRange"];           //締め対象範囲(全体／拠点毎)
            string departmentCode = null;

            if (targetClass.Equals("1"))
            {
                if (!string.IsNullOrEmpty(form["DepartmentCode"]))
                {
                    departmentCode = form["DepartmentCode"];        //部門コード
                }
            }

            if (MvcApplication.CMOperateFlag == 0) //データ作成中で無い場合
            {
                lock (typeof(MvcApplication))
                {
                    //-----------------------------
                    //ロック処理
                    //-----------------------------
                    MvcApplication.CMOperateFlag = 1;
                    MvcApplication.CMOperateUser = new EmployeeDao(db).GetByKey(((Employee)Session["Employee"]).EmployeeCode).EmployeeName;

                    //-----------------------------
                    //本処理
                    //-----------------------------
                    //トランザクション処理
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //--------------------------------------------
                        // InventorySchedule作成・更新
                        //--------------------------------------------
                        //CloseMonthControlの締め処理状況をチェック
                        chkret = JusdgeCloseProcess(targetMonth,  departmentCode, closeType);

                        //締め処理実行可能な場合
                        if (chkret == 0)
                        {
                            //作成・更新処理(IventorySchedule)
                            upsret = UpsertInventorySchedule(targetMonth, departmentCode, closeType);

                            //エラー発生時
                            if (upsret != 0)
                            {
                                ret = upsret;
                            }
                            else
                            {
                                // トランザクションのコミット        
                                ts.Complete();
                            }
                        }
                        else
                        {
                            ret = chkret;
                        }
						//--------------------------------------------
                        // CloseMonthControl作成・更新
                        //--------------------------------------------
						//SQLトリガで行うため、処理なし
                    }   

                    //-----------------------------
                    //ロック解除
                    //-----------------------------
                    MvcApplication.CMOperateUser = "";
                    MvcApplication.CMOperateFlag = 0;
                }
            }
            else    //他ユーザが作成中の場合
            {
                //操作中フラグON
                form["hdCMOperateFlag"] = MvcApplication.CMOperateFlag.ToString();
                form["hdCMOperateUser"] = MvcApplication.CMOperateUser;
            }
            
            //Add 2015/03/23 arc nakayama 伝票修正対応
            service = new ServiceSalesOrderService(db);
            if (closeType == "003"){
                // 修正中の伝票情報を削除する
                service.CloseEnd(targetMonth, departmentCode);

                //Del 2018/08/28 yano #3922
                //Add 2016/11/30 #3659
                //日付の設定
                //DateTime[] dateRange = new DateTime[2];
                //dateRange[0] = targetMonth;                             //月初
                //dateRange[1] = targetMonth.AddMonths(1).AddDays(-1);    //月末

                //List<CarStock> retlist = new CarStockDao(db).MakeCarStockData(string.Format("{0:yyyyMMdd}", dateRange[0]), ((Employee)Session["Employee"]).EmployeeCode, dateRange, true);
            }

            return ret;
        }


        //Add 2014/08/27 arc yano IPO対応その２
        /// <summary>
        /// 定数定義
        /// </summary>
        //処理種別
        private const string TYPE_CLOSE_CANCEL = "001";   //仮締め解除    
        private const string TYPE_CLOSE_START = "002";    //仮締め
        private const string TYPE_CLOSE_END = "003";      //本締め

        //ScheduleStatus
        private const string STS_CLOSE_CANCEL = "001";      //仮締め解除    
        private const string STS_CLOSE_START = "002";       //仮締め
        private const string STS_CLOSE_END = "003";         //本締め
        private const string STS_CLOSE_START_ON = "004";    //仮締め中
        

        /// <summar
        /// 棚卸スケジュールデータ作成・更新(個別/複数)
        /// </summary>
        /// <param name="targetMonth">棚卸月</param>
        /// <param name="departmentcode">部門コード</param>
        /// <param name="closeType">締め処理タイプ</param>
        /// <returns></returns>
        private int UpsertInventorySchedule(DateTime targetMonth, string departmentcode, string closeType)
        {

            List<Department> departmentList = null;

            if (string.IsNullOrEmpty(departmentcode))
            {
                //全体
                
                //Mod 2014/09/19 arc yano 締め処理対象部門は、departmentに登録されている部門全て(但し、DelFlagが1のものは除く)に変更
                //departmentList = new DepartmentDao(db).GetListAllCloseMonthFlag();
                departmentList = new DepartmentDao(db).GetListAll();
            }
            else
            {
                //個別
                departmentList = new DepartmentDao(db).GetListByKey(departmentcode);
            }
            
            
            foreach (Department d in departmentList)
            {
                //対象部門、対象月のデータを取得する。
                //対象月のレコード
                //Mod 2014/11/05 arc nakayama 棚卸種別を「車両」のみに変更
                InventorySchedule istargetmonth = new InventoryScheduleDao(db).GetByKey(d.DepartmentCode, new DateTime(targetMonth.Year, targetMonth.Month, 1), "001");

                
                if (istargetmonth == null)
                {
                    //--------------------------
                    //データ追加
                    //--------------------------

                    istargetmonth = new InventorySchedule();   //新規作成

                    //部門
                    istargetmonth.DepartmentCode = d.DepartmentCode;
                    //棚卸月
                    istargetmonth.InventoryMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
                    //棚卸種別
                    istargetmonth.InventoryType = "001";
                    //棚卸ステータス
                    istargetmonth.InventoryStatus = closeType;
                    //棚卸開始日
                    istargetmonth.StartDate = System.DateTime.Now;
                    //棚卸終了日
                    istargetmonth.EndDate = System.DateTime.Now;
                    //作成者
                    istargetmonth.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    //作成日時
                    istargetmonth.CreateDate = System.DateTime.Now;
                    //最終更新者
                    istargetmonth.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    //最終更新日時
                    istargetmonth.LastUpdateDate = System.DateTime.Now;
                    //削除フラグ
                    istargetmonth.DelFlag = "0";

                    db.InventorySchedule.InsertOnSubmit(istargetmonth);
                }
                else
                {
                    //---------------------------
                    //更新
                    //---------------------------
                    //UpdateModel(istargetmonth);

                    //棚卸ステータス
                    istargetmonth.InventoryStatus = closeType;

                    //棚卸終了日
                    istargetmonth.EndDate = System.DateTime.Now;

                    //最終更新者
                    istargetmonth.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    //最終更新日時
                    istargetmonth.LastUpdateDate = System.DateTime.Now;
                }
                
            }

           //Dbサブミット処理
           return DbSubmit();
            
        }

        //Add 2014/08/27 arc yano IPO対応その２
        /// <summary>
        /// 棚卸スケジュールデータ更新可・不可チェック
        /// </summary>
        /// <param name="targetMonth">棚卸月</param>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="closeType">締め処理タイプ</param>
        /// <returns></returns>
        private int JusdgeCloseProcess(DateTime targetMonth, string departmentCode, string closeType)
        {
            int ret = 0;

            string strtargetMonth = string.Format("{0:yyyyMMdd}", targetMonth); //当月

            string strpreMonth = string.Format("{0:yyyyMMdd}", targetMonth.AddMonths(-1)); //前月
            
            string strnexttMonth = string.Format("{0:yyyyMMdd}", targetMonth.AddMonths(1)); //翌月



            //対象月のレコード
            CloseMonthControl cmctargetMonth = new CloseMonthControlDao(db).GetByKey(strtargetMonth);
            CloseMonthControl cmcnextMonth = null;
            CloseMonthControl cmcpreMonth = null;

            InventorySchedule ivstargetMonth = null;
            InventorySchedule ivsnextMonth = null;
            InventorySchedule ivspreMonth = null;

            
            //締め処理の種類による分岐
            switch (closeType)
            {

                case TYPE_CLOSE_CANCEL: //仮締め解除

                    //Mod 2014/09/19 arc yano 全体／部門別の締め処理により、チェック処理を変更する。
                    if (!string.IsNullOrEmpty(departmentCode)) //部門別
                    {
                        //翌月データ取得
                        ivsnextMonth = new InventoryScheduleDao(db).GetByKey(departmentCode, targetMonth.AddMonths(1));
                        //当月データ取得
                        ivstargetMonth = new InventoryScheduleDao(db).GetByKey(departmentCode, targetMonth);

                        //翌月データが存在し、かつステータスが仮締め解除でない場合
                        if ((ivsnextMonth != null) && (!ivsnextMonth.InventoryStatus.Equals(STS_CLOSE_CANCEL)))
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "翌月の部門別締め処理状況が仮締め解除でないため、仮締め解除は行えません"));
                            ret = 1;
                        }

                        //当月データが存在しない、またはステータスが仮締でない場合
                        if ((ivstargetMonth == null) || (!ivstargetMonth.InventoryStatus.Equals(STS_CLOSE_START)))
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "当月の部門別締め処理状況が、仮締めでないため、仮締め解除は行えません"));
                            ret = 1;
                        }
                    }
                    else  //全体
                    {
                        //対象月の翌月のデータを取得
                        cmcnextMonth = new CloseMonthControlDao(db).GetByKey(strnexttMonth);
                        
                        //翌月データが存在し、かつステータスが仮締め解除でない場合
                        if ((cmcnextMonth != null) && (!(cmcnextMonth.CloseStatus.Equals(STS_CLOSE_CANCEL))))
                        {
                            if (cmcnextMonth.CloseStatus.Equals(STS_CLOSE_START_ON)) //翌月ステータス=仮締め中
                            {
                                ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "翌月の締め処理状況が仮締め解除でない部門があります。一括で仮締め解除を行ってください"));
                            }
                            else
                            {
                                ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "翌月の締め処理状況が仮締め解除でないため、仮締め解除は行えません"));
                            }

                            ret = 1;
                        }
                        //当月データが存在しない、またはステータスが仮締、仮締中でない場合
                        if ((cmctargetMonth == null) || (!cmctargetMonth.CloseStatus.Equals(STS_CLOSE_START)) && (!cmctargetMonth.CloseStatus.Equals(STS_CLOSE_START_ON)))
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "当月の締め処理状況が、仮締め、または仮締め中でないため、仮締め解除は行えません"));
                            ret = 1;
                        }
                    }
                    break;

                case TYPE_CLOSE_START: //仮締め

                    
                    //Mod 2014/09/19 arc yano 全体／部門別の締め処理により、チェック処理を変更する。
                    if (!string.IsNullOrEmpty(departmentCode)) //部門別
                    {
                        //前月データ取得
                        ivspreMonth = new InventoryScheduleDao(db).GetByKey(departmentCode, targetMonth.AddMonths(-1));
                        //当月データ取得
                        ivstargetMonth = new InventoryScheduleDao(db).GetByKey(departmentCode, targetMonth);

                        //前月データが存在しない、または、ステータスが、仮締めまたは本締め以外(=仮締め解除)の場合
                        if ((ivspreMonth == null) || (ivspreMonth.InventoryStatus.Equals(STS_CLOSE_CANCEL)))
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "前月の部門別締め処理状況が仮締め、または本締めでないため、仮締めは行えません"));
                            ret = 1;
                        }

                        //当月データが存在し、かつステータスが仮締め解除でない場合
                        if ((ivstargetMonth != null) && (!ivstargetMonth.InventoryStatus.Equals(STS_CLOSE_CANCEL)))
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "当月の部門別締め処理状況が仮締め解除でないため、仮締めは行えません"));
                            ret = 1;
                        }
                    }
                    else //全体
                    {
                        //対象月の前月のデータを取得する。
                        cmcpreMonth = new CloseMonthControlDao(db).GetByKey(strpreMonth);

                        //前月データが存在しない
                        if (cmcpreMonth == null)
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "前月の締め処理状況が仮締め、または本締めでないため、仮締めは行えません"));
                            ret = 1;
                        }
                        else if (!(cmcpreMonth.CloseStatus.Equals(STS_CLOSE_START) || cmcpreMonth.CloseStatus.Equals(STS_CLOSE_END)))   //前月のステータスが仮締め、または本締め以外の場合
                        {
                            if(cmcpreMonth.CloseStatus.Equals(STS_CLOSE_START_ON))
                            {
                                ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "前月の締め処理状況が仮締め、または本締めでない部門があります。一括仮締め、または本締めを行ってください"));
                            }
                            else
                            {
                                ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "前月の締め処理状況が仮締め、または本締めでないため、仮締めは行えません"));
                            }
                            ret = 1;
                        }
                        else
                        {
                            //何もしない
                        }
                        //当月データが存在し、かつステータスが仮締め解除、または仮締め中でない場合
                        if ((cmctargetMonth != null) && ((!cmctargetMonth.CloseStatus.Equals(STS_CLOSE_CANCEL)) && (!cmctargetMonth.CloseStatus.Equals(STS_CLOSE_START_ON))))
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "当月の締め処理状況が仮締め解除、または仮締め中でないため、仮締めは行えません"));
                            ret = 1;
                        }
                    }
                    break;

                case TYPE_CLOSE_END: //本締め

                    //対象月の前月のデータを取得する。
                    cmcpreMonth = new CloseMonthControlDao(db).GetByKey(strpreMonth);

                    //前月データが存在しない、または、ステータスが、本締めでない場合
                    if ((cmcpreMonth == null) || (!cmcpreMonth.CloseStatus.Equals(STS_CLOSE_END)))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "前月の締め処理状況が本締めでないため、本締めは行えません"));
                        ret = 1;
                    }

                    //Mod 2014/09/19 arc yano 一括締め処理（仮締め、仮締め解除）の対象部門の変更対応 ステータスが仮締め中の場合のエラーメッセージを追加
                    //当月のデータが存在しない場合
                    if (cmctargetMonth == null)
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "当月の締め処理状況が仮締めでないため、本締めは行えません"));
                        ret = 1;
                    }
                    else if (!cmctargetMonth.CloseStatus.Equals(STS_CLOSE_START))　//当月のステータスが仮締め以外の場合
                    {
                        if (cmctargetMonth.CloseStatus.Equals(STS_CLOSE_START_ON)) //ステータスが仮締め中(画面の見た目上は全部門仮締めになってる場合があるため、一括仮締めを促すメッセージを表示する。)
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "当月の締め処理状況が仮締めでない部門があります。一括仮締めを行ってください"));
                        }
                        else
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "当月の締め処理状況が仮締めでないため、本締めは行えません"));
                        }
                        ret = 1;
                    }
                    else
                    {
                        //何もしない
                    }
                    break;

                default:
                    //処理なし  ここに来ることはない
                    break;
            }

            return ret;
        }

    
		//Add 2014/08/27 IPO対応その２ 月締め処理追加
        /// <summary>
        /// DBコミット処理
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private int DbSubmit()
        {
           int ret = 0;

            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException e)
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
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");

                        ret = -1;
                    }
                }
                /*
                catch (SqlException e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ログに出力
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("DepartmentCode, InvenToryMonth", MessageUtils.GetMessage("E0010", new string[] { "部門コード、棚卸月", "保存" }));

                        ret = 1;
                    }
                    else
                    {
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        ret = -1;
                    }
                }
                */
                catch (Exception ex)
                {
                    //ChangeConflictException以外の時のエラー処理追加
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
                    ret = -1;
                }
            }
            return ret;
        }

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private void SetDataComponent(FormCollection form) {

            CodeDao dao = new CodeDao(db);

            Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
            if (department != null) {
                ViewData["DepartmentName"] = department.DepartmentName;
            }

            //ViewData["TargetMonthList"] = CodeUtils.GetSelectList(CodeUtils.GetMonthsTypeAll(), form["TargetMonth"], false);
            
            //Mod 2014/08/27 IPO対応その２ レイアウト変更による、画面コンポーネントの設定項目の追加
            //締め処理タイプ
            ViewData["CloseTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCloseTypeAll(false), form["CloseType"], false);
            //対象年
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["TargetYear"], false);
            
            //対象月
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["TargetMonth"], false);

            //部門コード
            ViewData["DepartmentCode"] = form["DepartmentCode"];

            //処理範囲
            ViewData["TargetRange"] = form["TargetRange"]; 

            //操作フラグ
            ViewData["CMOperateFlag"] = form["hdCMOperateFlag"];
            
            //操作者
            ViewData["CMOperateUser"] = form["hdCMOperateUser"];
        }
        #endregion

        #region 部門コードから該当部門の棚卸状況を返却する(Ajax専用）
        /// <summary>
        /// 部門コードから該当部門の棚卸状況を返却する(Ajax専用）
        /// </summary>
        /// <param name="code">部門コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 引数の変更(departmentCode → warehouseCode)
        /// </history>
        public ActionResult PartsInventorySchedule(string WarehouseCode, DateTime InventoryMonth)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                //InventoryScheduleParts InventorySchedule = new InventorySchedulePartsDao(db).GetByKey(DepartmentCode, InventoryMonth, "002");
                InventoryScheduleParts InventorySchedule = new InventorySchedulePartsDao(db).GetByKey(WarehouseCode, InventoryMonth, "002");
                if (InventorySchedule != null)
                {
                    codeData.Code = InventorySchedule.InventoryStatus;
                    if (InventorySchedule.InventoryStatus == "001"){
                        codeData.Name = "実施中";
                    }else{
                        codeData.Name = "確定";
                    }
                    
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        #endregion

        #region 部門コードから該当部門の車両棚卸状況を返却する(Ajax専用）
        /// <summary>
        /// 部門コードから該当部門の棚卸状況を返却する(Ajax専用）
        /// </summary>
        /// <param name="code">部門コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public ActionResult CarInventorySchedule(string WarehouseCode, DateTime InventoryMonth)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();

                if (!string.IsNullOrWhiteSpace(WarehouseCode))
                {
                    InventoryScheduleCar InventorySchedule = new InventoryScheduleCarDao(db).GetByKey(WarehouseCode, InventoryMonth);
                    if (InventorySchedule != null)
                    {
                        codeData.Code = InventorySchedule.InventoryStatus;

                        switch (codeData.Code)
                        {
                            case "001": //実施中
                                codeData.Name = "実施中";
                                break;
                            case "002": //仮確定
                                codeData.Name = "仮確定";
                                break;
                            case "003": //本確定
                                codeData.Name = "確定";
                                break;
                            default:
                                codeData.Name = "";
                                break;
                        }
                    }
                    else
                    {
                        //存在しない場合は未実施として扱う
                        codeData.Code = "000";
                        codeData.Name = "未実施";
                    }
                }
                else //倉庫コードが未入力の場合はステータス不明とする
                {
                    codeData.Code = "999";
                    codeData.Name = "不明";
                }

                
                return Json(codeData);
            }
            return new EmptyResult();
        }
        #endregion

        //Add 2016/11/30 #3659 車両管理 処理中対応
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
