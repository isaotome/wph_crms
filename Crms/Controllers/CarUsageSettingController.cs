using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Transactions;
using Crms.Models;
using System.Text.RegularExpressions;

namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarUsageSettingController : Controller
    {
        
        
        #region 初期化

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarUsageSettingController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        #region 定数
        //処理種別
        private const string REGIST_DEMO      = "001";         // デモカー登録
        private const string REGIST_RENTAL    = "002";         // レンタカー登録
        private const string REGIST_BUSINESS  = "003";         // 業務車登録
        private const string REGIST_PUBLICITY = "004";         // 広報車登録
        private const string REGIST_LOANER    = "005";         // 代車登録
        private const string REGIST_OWN       = "006";         // 自社登録      //Add 2015/02/10 arc yano デモカー登録修正(自社登録追加)
        private const string REGIST_RELEASE   = "010";         // 登録解除
        private const string REGIST_CANCEL    = "011";         // 登録取り消し

        //利用用途
        private const string CAR_DEMO         = "001";         // デモカー
        private const string CAR_RENTAL       = "002";         // レンタカー
        private const string CAR_BUSINESS     = "003";         // 業務車両
        private const string CAR_PUBLICITY    = "004";         // 広報車
        private const string CAR_LOANER       = "005";         // 代車


        //販売タイプ
        private const string TSALES_OWN     　= "005";         // 自社登録     //Add 2015/02/10 arc yano デモカー登録修正(自社登録追加)
        private const string TSALES_DEMO      = "006";         // デモカー登録
        private const string TSALES_RENTAL    = "010";         // レンタカー登録
        private const string TSALES_LOANER    = "011";         // 代車登録
        private const string TSALES_PUBLICITY = "012";         // 広報車登録
        private const string TSALES_BUSINESS  = "013";         // 業務車登録
        private const string TSALES_OTHER     = "007";         // その他

        //Add 2018/06/06 arc yano #3883
        //耐用年数
        private const int USEFULLIVES_DEMO = 6;                //耐用年数(デモカー)
        private const int USEFULLIVES_LOANER = 5;              //耐用年数(代車)
        private const int USEFULLIVES_RENTAL = 3;              //耐用年数(レンタカー)

        //顧客、仕入れ先
        private const string CUSTOMER_CHECKERMOTORS = "002001";     //チェッカーモーターズ

        //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
        //車検案内可否
        private const string INSPECT_GUIDE_FLAG_NG = "002"; //不可

        private const string INSPECT_GUIDE_FLAG_OK = "001"; //可 //Add 2018/03/20 arc yano #3870

        //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
        //車検案内備考欄
        private const string INSPECT_GUIDE_MEMO = "社有車";

        private const string INSPECT_GUIDE_MEMO_SPACE = "";   //Add 2018/03/20 arc yano #3870


        //システムログ出力用定数
        private static readonly string FORM_NAME_CRITERIA = "車両利用用途検索";                　// 画面名(検索画面)
        private static readonly string FORM_NAME_ENTRY    = "車両利用用途変更";                　// 画面名(入力画面)
        private static readonly string PROC_NAME_SEARCH   = "車両ステータス変更(検索)";          // 処理名(検索機能)
        private static readonly string PROC_NAME_REGIST   = "車両ステータス変更(登録)";      　　// 処理名(登録機能)

        #endregion

        #region 検索画面
        /// <summary>
        /// 車両利用用途検索画面表示
        /// </summary>
        /// <returns>車両仕入検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 車両利用用途検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両利用用途検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME_CRITERIA, PROC_NAME_SEARCH);
         
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<SalesCar> list = GetSearchResultList(form);

            // その他出力項目の設定
            SetComponent(form);

            //Mod 2014/10/30 arc amii 車両ステータス追加対応
            // 車両仕入検索画面の表示
            return View("CarUsageSettingCriteria", list);
        }
        #endregion

        #region 検索処理
        /// <summary>
        /// 車両利用テーブル検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両利用テーブル検索結果リスト</returns>
        private PaginatedList<SalesCar> GetSearchResultList(FormCollection form)
        {
            SalesCarDao salesCarDao = new SalesCarDao(db);
            SalesCar saleCarCondition = new SalesCar();
            saleCarCondition.SalesCarNumber = form["SalesCarNumber"];
            saleCarCondition.Vin = form["Vin"];
            saleCarCondition.CarStatus = form["CarStatus"];
            saleCarCondition.CarUsage = form["CarUsage"];
            saleCarCondition.PossesorName = form["PossesorName"];
            saleCarCondition.UserName = form["UserName"]; 
            saleCarCondition.DelFlag = "0";
            return salesCarDao.GetListByCondition(saleCarCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private void SetComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);
            ViewData["SalesCarNumber"] = form["SalesCarNumber"];
            ViewData["Vin"] = form["Vin"];
            ViewData["CarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), form["CarStatus"], true);
            ViewData["CarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("004", false), form["CarUsage"], true);
            ViewData["PossesorName"] = form["PossesorName"];
            ViewData["UserName"] = form["UserName"];
        }  
        #endregion

        #region 入力画面
        /// <summary>
        /// 利用用途入力画面表示
        /// </summary>
        /// <param name="id">管理番号
        /// <returns>利用用途入力画面表示</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            //ViewData["Master"] = Request["Master"];
            SalesCar salesCar = new SalesCar();

            if (!string.IsNullOrEmpty(id))
            {
                // Mod 2015/05/01 arc nakayama #3193:車両用途変更でシステムエラー発生　暫定対応：DelフラグがONのデータも表示する(システムエラーで落ちるのは避けたい) 恒久対策はまた別途対応する。
                salesCar = new SalesCarDao(db).GetByKey(id, true);

                salesCar.OwnershipChangeDate = DateTime.Today;
                ViewData["Closed"] = "";

                ViewData["LocationName"] = "";
                try { ViewData["LocationName"] = salesCar.Location.LocationName; }
                catch (NullReferenceException) { }

                FormCollection form = new FormCollection();

                // その他表示データの取得
                GetEntryViewData(salesCar, form);
            }
            
            //Mod 2014/10/30 arc amii 車両ステータス追加対応
            // 出口
            return View("CarUsageSettingEntry", salesCar);
        }
        #endregion

        #region 車両用途登録
        /// <summary>
        /// 車両用途登録処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両用途入力画面</returns>
        /// <history>
        /// 2018/03/20 arc yano #3870 除却・自社登録時には車検案内を「可」とする
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(FormCollection form, SalesCar salesCar)
        {

            //modelの状態をクリア
            ModelState.Clear();

            
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME_ENTRY, PROC_NAME_REGIST);

            ActionResult result = Entry("");
            string selectCarUsage = form["CarUsageType"];
            string salesCarNumber = form["SalesCar.SalesCarNumber"];


            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            //-----------------------------------------
            //入力フォームのチェック
            //-----------------------------------------
            //登録種別の必須チェック
            if (string.IsNullOrEmpty(selectCarUsage))
            {
                ModelState.AddModelError("CarUsageType", MessageUtils.GetMessage("E0001", "登録種別"));
            }
            //振替日の必須チェック
            if (string.IsNullOrEmpty(form["changeDate"].ToString()))
            {
                ModelState.AddModelError("ChangeDate", MessageUtils.GetMessage("E0001", "振替日"));
            }
            //振替日のフォーマットチェック
            else if (!Regex.IsMatch(form["changeDate"].ToString(), @"\d{4}\/\d{2}\/\d{2}$"))
            {
                ModelState.AddModelError("ChangeDate", MessageUtils.GetMessage("E0005", "振替日"));
            }

            //Mod 2015/09/08 arc yano #3249 車両用途変更でロケーション入れないと入庫ロケーションがNULLになる　入庫ロケーションの必須チェックを追加
            //入庫ロケーションの必須チェック
            //登録種別の必須チェック
            if (string.IsNullOrWhiteSpace(form["PurchaseLocationCode"]))
            {
                ModelState.AddModelError("PurchaseLocationCode", MessageUtils.GetMessage("E0001", "入庫ロケーション"));
            }
            
            //検証エラーが発生した場合は、viewを返す
            if (!ModelState.IsValid)
            {
                //入力値の保持
                ViewData["ChangeDate"] = form["ChangeDate"];
                ViewData["PurchaseLocationCode"] = form["PurchaseLocationCode"];
                ViewData["PurchaseLocationName"] = form["PurchaseLocationName"];
                
                // その他表示データの取得
                GetEntryViewData(salesCar, form);
                return View("CarUsageSettingEntry", salesCar);
            }

            //----------------------------------
            // 登録種別による分岐
            //----------------------------------
            string customerCode = "";        //顧客コード
            string salesType = "";           //販売種別
            string procName = "";            //区分
            string carUsage = "";          　//利用用途
            string InspectGuideFlag = "";     //車検案内可否
            string InspectGuideMemo = "";     //車検案内可否
            DateTime? changeDate = null;
            int legalUsefulLives = 2;         //法定耐用年数(初期値)               //Add 2018/06/06 arc yano #3883

            //振替日

            if (!string.IsNullOrEmpty(form["ChangeDate"].ToString()))
            {
                changeDate = DateTime.ParseExact(form["ChangeDate"].ToString(), "yyyy/MM/dd", null);  //振替日(datetime型)
            }
            else
            {
                changeDate = System.DateTime.Now;   //振替日が設定されていない場合は現在日時を設定する。
            }


            //登録データの設定
            switch (selectCarUsage)
            {
                //デモ登録
                case REGIST_DEMO:
                    customerCode = CUSTOMER_CHECKERMOTORS;
                    salesType = TSALES_DEMO;
                    procName = "デモ登録";
                    carUsage = CAR_DEMO;       //デモカー
                    //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
                    InspectGuideFlag = INSPECT_GUIDE_FLAG_NG; //車検案内可否
                    InspectGuideMemo = INSPECT_GUIDE_MEMO;    //車検案内備考欄
                    legalUsefulLives = USEFULLIVES_DEMO;      //法定耐用年数 //Add 2018/06/06 arc yano #3883
                    break;

                //レンタカー登録
                case REGIST_RENTAL:
                    customerCode = CUSTOMER_CHECKERMOTORS;
                    salesType = TSALES_RENTAL;
                    procName = "レンタカー登録";
                    carUsage = CAR_RENTAL;       //レンタカー
                    //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
                    InspectGuideFlag = INSPECT_GUIDE_FLAG_NG; //車検案内可否
                    InspectGuideMemo = INSPECT_GUIDE_MEMO;    //車検案内備考欄
                    legalUsefulLives = USEFULLIVES_RENTAL;    //法定耐用年数 //Add 2018/06/06 arc yano #3883
                    break;
                //業務車登録
                case REGIST_BUSINESS:
                    customerCode = CUSTOMER_CHECKERMOTORS;
                    salesType = TSALES_BUSINESS;
                    procName = "業務車両登録";
                    carUsage = CAR_BUSINESS;       //業務車
                    //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
                    InspectGuideFlag = INSPECT_GUIDE_FLAG_NG; //車検案内可否
                    InspectGuideMemo = INSPECT_GUIDE_MEMO;    //車検案内備考欄
                    break;
                //広報車登録
                case REGIST_PUBLICITY:
                    customerCode = CUSTOMER_CHECKERMOTORS;
                    salesType = TSALES_PUBLICITY;
                    procName = "広報車登録";
                    carUsage = CAR_PUBLICITY;   //広報車
                    //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
                    InspectGuideFlag = INSPECT_GUIDE_FLAG_NG; //車検案内可否
                    InspectGuideMemo = INSPECT_GUIDE_MEMO;    //車検案内備考欄
                    break;
                //代車登録
                case REGIST_LOANER:
                    customerCode = CUSTOMER_CHECKERMOTORS;
                    salesType = TSALES_LOANER;
                    procName = "代車登録";
                    carUsage = CAR_LOANER;       //代車
                    //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
                    InspectGuideFlag = INSPECT_GUIDE_FLAG_NG; //車検案内可否
                    InspectGuideMemo = INSPECT_GUIDE_MEMO;    //車検案内備考欄
                    legalUsefulLives = USEFULLIVES_LOANER;    //法定耐用年数 //Add 2018/06/06 arc yano #3883
                    break;
                //登録解除
                case REGIST_RELEASE:
                    customerCode = CUSTOMER_CHECKERMOTORS;
                    salesType = TSALES_OTHER;
                    procName = "除却";
                    //Add 2018/03/20 arc yano #3870
                    InspectGuideFlag = INSPECT_GUIDE_FLAG_OK;       //車検案内可否
                    InspectGuideMemo = INSPECT_GUIDE_MEMO_SPACE;    //車検案内備考欄
                    carUsage = "";
                    break;
                // 取消
                case REGIST_CANCEL:
                    customerCode = CUSTOMER_CHECKERMOTORS;
                    salesType = TSALES_OTHER;
                    procName = "入力取消";
                    carUsage = "";
                    break;
                //Add 2015/02/10 arc yano デモカー登録修正(自社登録追加)
                // 自社登録
                case REGIST_OWN:
                    customerCode = CUSTOMER_CHECKERMOTORS;
                    salesType = TSALES_OWN;
                    procName = "自社登録";
                    carUsage = "";
                    //Mod 2018/03/20 arc yano #3870
                    //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
                    InspectGuideFlag = INSPECT_GUIDE_FLAG_OK;       //車検案内可否
                    InspectGuideMemo = INSPECT_GUIDE_MEMO_SPACE;    //車検案内備考欄
                    break;

                //その他
                default:
                    //何もしない
                    customerCode = "";
                    salesType = "";
                    procName = "";
                    carUsage = "";
                    //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
                    InspectGuideFlag = INSPECT_GUIDE_FLAG_NG; //車検案内可否
                    InspectGuideMemo = INSPECT_GUIDE_MEMO;    //車検案内備考欄
                    break;
            }

            //メッセージ出力
            string strmsg = "";

            //Dbコミット結果
            int ret = 0;

            string newSalesCarNumber = "";   //管理番号(新)
            string retSalesCarNumber = "";  //管理番号(リターン値)

            //Mod 2015/02/16 arc yano デモカー登録修正 自社登録処理の追加
            //登録種別によるデータ更新(仕入【デモ車仕入／レンタカー仕入／業務車仕入】以外の場合)

            using (TransactionScope ts = new TransactionScope())
            {
                if ((selectCarUsage != REGIST_RELEASE) && (selectCarUsage != REGIST_CANCEL))
                {
                    //Mod 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
                    retSalesCarNumber = SalesCarRegist(form, salesCar, salesCarNumber, carUsage, changeDate, customerCode, salesType, selectCarUsage, ts, InspectGuideFlag, InspectGuideMemo, legalUsefulLives);
                    
                    //検証エラーが発生している場合は、終了する。
                    if (!ModelState.IsValid)
                    {
                        //入力値の保持
                        ViewData["ChangeDate"] = form["ChangeDate"];
                        ViewData["PurchaseLocationCode"] = form["PurchaseLocationCode"];
                        ViewData["PurchaseLocationName"] = form["PurchaseLocationName"];

                        // その他表示データの取得
                        GetEntryViewData(salesCar, form);

                        return View("CarUsageSettingEntry", salesCar);
                    }
                }
                // 登録種別によるデータ更新(登録解除)または自社登録の場合
                if ((selectCarUsage == REGIST_RELEASE) || (selectCarUsage == REGIST_OWN))
                {
                    //----------------------------------------------
                    //新規管理番号取得
                    //----------------------------------------------
                    newSalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber("001", "U");     //車両販売の最新シリアルを取得
                    newSalesCarNumber = newSalesCarNumber.Replace("001U", "002U");

                    retSalesCarNumber = SalesCarPurchase(form, salesCar, salesCarNumber, customerCode, selectCarUsage, changeDate, procName, newSalesCarNumber, selectCarUsage, ts, InspectGuideFlag, InspectGuideMemo);

                    //検証エラーが発生している場合は、終了する。
                    if (!ModelState.IsValid)
                    {
                        //入力値の保持
                        ViewData["ChangeDate"] = form["ChangeDate"];
                        ViewData["PurchaseLocationCode"] = form["PurchaseLocationCode"];
                        ViewData["PurchaseLocationName"] = form["PurchaseLocationName"];

                        //Mod 2015/05/19 arc yano 仮締中データ編集権限追加で見つけた不具合
                        salesCar = new SalesCarDao(db).GetByKey(salesCar.SalesCarNumber);

                        // その他表示データの取得
                        GetEntryViewData(salesCar, form);
                        
                        return View("CarUsageSettingEntry", salesCar);
                    }
                }
                // 登録種別によるデータ更新(取り消し)の場合
                if ((selectCarUsage == REGIST_CANCEL))
                {

                    salesCarNumber = SalesCarCancel(form, salesCarNumber, selectCarUsage, ts);

                    //検証エラーが発生している場合は、終了する。
                    if (!ModelState.IsValid)
                    {
                        //入力値の保持
                        ViewData["ChangeDate"] = form["ChangeDate"];
                        ViewData["PurchaseLocationCode"] = form["PurchaseLocationCode"];
                        ViewData["PurchaseLocationName"] = form["PurchaseLocationName"];

                        // その他表示データの取得
                        GetEntryViewData(salesCar, form);

                        return View("CarUsageSettingEntry", salesCar);
                    }
                }

                //DBコミット処理
                ret = DbSubmit(salesCar, form, ts);
            
            }

            //Add 2015/02/16 arc yano 車両利用用途対応 自社登録処理追加のため、コミット処理の外出し 
            switch (ret)
            {
                case -1:    //エラー発生時
                    // エラーページに遷移
                    result = View("Error");
                    break;

                case 1:    //一意制約違反時
                    result = View("CarUsageSettingEntry", salesCar);
                    break;

                default:   //　正常時

                    //処理完了のメッセージの設定
                    if ((selectCarUsage == REGIST_RELEASE) || (selectCarUsage == REGIST_OWN))
                    {
                        strmsg = procName + "を行いました";
                        strmsg += "( 旧管理番号：" + salesCarNumber + " → 新管理番号：" + newSalesCarNumber + " )";
                    }
                    else
                    {
                        strmsg = procName + "を行いました";
                    }

                    //最後に処理完了のメッセージを表示して終了する。
                    ModelState.AddModelError("", strmsg);

                    result = Entry(salesCarNumber);
                    break;
            }

            return result;
        }
        #endregion

        #region 登録
        /// <summary>
        /// 登録処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="salesCar">車両マスタ</param>
        /// <param name="salesCarNumber">管理番号</param>
        /// <param name="carUsage">利用用途</param>
        /// <param name="changeDate">振替日</param>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="salesType">販売区分</param>
        /// <param name="selectCarUsage">登録種別</param>
        /// <param name="ts">トランザクションスコープ</param>
        /// <param name="InspectGuideFlag">車検案内フラグ</param>
        /// <param name="InspectGuideMemo">車検案内備考</param>
        /// <param name="legalUsefulLives">法定耐用年数</param>
        /// 
        /// <returns>車両用途入力画面</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 車両固定資産テーブル更新処理の追加
        /// 2018/03/20 arc yano #3870 除却・自社登録時には車検案内を「可」とする
        /// </history>
        //Mod 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
        private string SalesCarRegist(FormCollection form, SalesCar salesCar, string salesCarNumber, string carUsage, DateTime? changeDate, string customerCode, string salesType, string selectCarUsage, TransactionScope ts, string InspectGuideFlag, string InspectGuideMemo, int legalUsefulLives)
        {
            //------------------------------------------------------
            // ダミーデータ作成
            //------------------------------------------------------

            //車両伝票ヘッダ作成

            CarSalesHeader carSalesHeader = new CarSalesHeader();
            //string beforeLocationCode = "";
            

            //最新車両伝票番号取得(左１桁が'2'の伝票)
            string slipNumber = new CarSalesOrderDao(db).GetLatestSlipNumber();

            if (slipNumber == null)
            {
                slipNumber = "20000001";       //固定で入力
            }
            else
            {
                slipNumber = (int.Parse(slipNumber) + 1).ToString(); //最新伝票番号＋１
            }

         //   using (TransactionScope ts = new TransactionScope())
         //   {
                //データ編集
                carSalesHeader = EditCarSalesHeaderForInsert(form, carSalesHeader, slipNumber, customerCode, salesType, changeDate, salesCar);
                carSalesHeader = EditCarSalesHeaderForUpdate(carSalesHeader, slipNumber, salesCarNumber);
                carSalesHeader = SetCarSalesHeader2(carSalesHeader, salesCarNumber);

                // Mod 2015/04/15 arc yano　仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
                //Add 2015/02/10 arc yano デモカー登録修正 納車日に、棚卸済の日付を設定していないかをチェックする。
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(carSalesHeader.DepartmentCode, carSalesHeader.SalesDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                { 
                    ModelState.AddModelError("ChangeDate", "月次締め処理が終了しているため、指定された振替日は設定できません");
 
                    //トランザクションの破棄
                    ts.Dispose();
                    return salesCarNumber;
                    //return View("CarUsageSettingEntry", salesCar);
                }

                //インサート
                db.CarSalesHeader.InsertOnSubmit(carSalesHeader);

                //----------------------------------------------------------------
                //車両発注引当作成
                //----------------------------------------------------------------

                CarPurchaseOrder carPurcahseOrder = new CarPurchaseOrder();

                //最新車両発注依頼番号取得(左１桁が'2'の伝票)
                string carPurchaseOrderNumber = new CarPurchaseOrderDao(db).GetLatestPurchaseOrderNumber();

                if (carPurchaseOrderNumber == null)
                {
                    carPurchaseOrderNumber = "20000001";       //固定で入力
                }
                else
                {
                    carPurchaseOrderNumber = (int.Parse(carPurchaseOrderNumber) + 1).ToString(); //最新車両発注依頼番号＋１
                }

           
                //データ編集
                carPurcahseOrder = EditCarPurcahseOrderForInsert(form, carPurcahseOrder, carPurchaseOrderNumber, slipNumber, changeDate, salesCar);

                //インサート
                db.CarPurchaseOrder.InsertOnSubmit(carPurcahseOrder);

                //-----------------------------------------------------------
                //車両マスタ更新
                //-----------------------------------------------------------
                salesCar = new SalesCarDao(db).GetByKey(salesCarNumber);
                SalesCar oldSalesCar = new SalesCar();
                
                oldSalesCar.CarStatus = salesCar.CarStatus;
                oldSalesCar.LocationCode = salesCar.LocationCode;
                oldSalesCar.UserCode = salesCar.UserCode;
                oldSalesCar.OwnerCode = salesCar.OwnerCode;
                //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
                oldSalesCar.InspectGuidFlag = salesCar.InspectGuidFlag;
                oldSalesCar.InspectGuidMemo = salesCar.InspectGuidMemo;

                salesCar = SetSalesCar(salesCar, carSalesHeader, slipNumber, form["PurchaseLocationCode"], carUsage, InspectGuideFlag, InspectGuideMemo, selectCarUsage);


                //自社登録以外の処理
                if (selectCarUsage != REGIST_OWN)
                {
                    //-----------------------------------------------------------
                    //更新履歴データ作成
                    //-----------------------------------------------------------
                    //Mod 2015/02/17 arc yano 車両用途変更(自社登録処理追加) 自社登録の場合は、ここでは、更新履歴は作成しない　※仕入データ作成時に行う
                    BackGroundDemoCar demoCar = EditBackGroundDemoCarForUpdate(slipNumber, carPurcahseOrder.CarPurchaseOrderNumber, selectCarUsage, salesCarNumber, "", changeDate, oldSalesCar);
                    db.BackGroundDemoCar.InsertOnSubmit(demoCar);

                    //Add 2018/06/06 arc yano #3883 タマ表改善　固定資産テーブル更新
                    //-----------------------------------------------------------
                    //固定資産テーブル登録
                    //-----------------------------------------------------------
                    EditCarFixedAssets(salesCarNumber, changeDate, legalUsefulLives, salesCar);
                }

                return salesCarNumber;
        }
        #endregion

        //Mod 2015/02/16 arc yano 車両用途変更 自社登録処理追加のため、仕入処理の返却値の型を変更(ActionResult→String)
        #region 仕入
        /// <summary>
        /// 仕入
        /// </summary>
        /// <param name="form"></param>
        /// <param name="salesCar"></param>
        /// <param name="salesCarNumber"></param>
        /// <param name="customerCode"></param>
        /// <param name="carUsage"></param>
        /// <param name="changeDate"></param>
        /// <param name="procName"></param>
        /// <param name="newSalesCarNumber"></param>
        /// <param name="selectCarUsage"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 減価償却処理の追加
        /// 2018/03/20 arc yano #3870 自社登録・除却を行った際には車検案内を「可」に変更する
        /// </history>
        private string SalesCarPurchase(FormCollection form, SalesCar salesCar, string salesCarNumber, string customerCode, string carUsage, DateTime? changeDate, string procName, string newSalesCarNumber, string selectCarUsage, TransactionScope ts, string InspectGuideFlag, string InspectGuideMemo)
        {
            string oldSalesCarNumber = "";

            //Del 2015/02/17 arc yano 車両用途変更(自社登録処理追加) トランザクションスコープを移動
            //

            //--------------------------------------
            //車両仕入データ再作成
            //--------------------------------------
            CarPurchase oldCarPurchase = new CarPurchaseDao(db).GetBySalesCarNumber(salesCarNumber);   //既存データ取得
            CarPurchase newCarPurchase = new CarPurchase();                                            //新規データ取得

            //Add 2018/06/06 arc yano #3883
            //------------------------------------
            //減価償却処理
            //------------------------------------
            //固定資産テーブルにデータが存在する場合は未償却残高の計算を行う
            decimal? undepreciatedBalance = null;
            
            CarFixedAssets carfixAssets = new CarFixedAssetsDao(db).GetByKey(salesCarNumber);

            if(carfixAssets != null)
            {
                undepreciatedBalance = CalculateUndepreciatedBalance(carfixAssets, changeDate);
            }

            //Mod 2015/02/16 arc yano 車両用途変更 自社登録処理追加 自社登録時、または既存データが無い場合は新規データを作成する
            if ((selectCarUsage != REGIST_OWN ) && (oldCarPurchase != null))
            {
                //既存データで再作成
                newCarPurchase = EditCarPurchaseForUpdate(oldCarPurchase, newSalesCarNumber, customerCode, form["PurchaseLocationCode"], changeDate, undepreciatedBalance);  //Mod 2018/06/06 arc yano #3883
            }
            else
            {
                //新規データで作成
                newCarPurchase = EditCarPurchaseForInsert(newSalesCarNumber, customerCode, form["PurchaseLocationCode"], changeDate, selectCarUsage, oldCarPurchase, undepreciatedBalance);       //Mod 2018/06/06 arc yano #3883
            }

            // Mod 2015/04/15 arc yano　仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
            //Add 2015/02/10 arc yano デモカー登録修正 入庫日に、棚卸済の日付を設定していないかをチェックする。
            //入庫日が棚卸締め処理済みの月内であればエラー
            if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(newCarPurchase.DepartmentCode, newCarPurchase.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
            {
                ModelState.AddModelError("ChangeDate", "月次締め処理が終了しているため、指定された振替日は設定できません");

                //トランザクションの破棄
                ts.Dispose();

                return salesCarNumber;
            }
                
            db.CarPurchase.InsertOnSubmit(newCarPurchase);

            //-----------------------------------------------------------
            //車両マスタ更新
            //-----------------------------------------------------------
            SalesCar oldSalesCar = new SalesCarDao(db).GetByKey(salesCarNumber);
            SalesCar newSalesCar = new SalesCar();

            oldSalesCarNumber = oldSalesCar.SalesCarNumber;

            newSalesCar = EditSalesCarForUpdate(oldSalesCar, newSalesCarNumber, oldSalesCarNumber, procName, form["PurchaseLocationCode"], carUsage, InspectGuideFlag, InspectGuideMemo);   //Mod 2018/03/20 arc yano #3870
            oldSalesCar.DelFlag = "1";
            db.SalesCar.InsertOnSubmit(newSalesCar);

            //-----------------------------------------------------------
            //更新履歴データ作成
            //-----------------------------------------------------------
            CarPurchaseOrder purchaseOrder = new CarPurchaseOrderDao(db).GetBySalesCarNumber(salesCarNumber);
            if (purchaseOrder == null)
            {
                purchaseOrder = new CarPurchaseOrder();
            }

            BackGroundDemoCar demoCar = EditBackGroundDemoCarForUpdate(purchaseOrder.SlipNumber, "", carUsage, oldSalesCarNumber, newSalesCarNumber, changeDate, oldSalesCar);
            db.BackGroundDemoCar.InsertOnSubmit(demoCar);

            //-------------------------------------------------------------
            //DBコミット処理
            //-------------------------------------------------------------
            //Del 2015/02/17 arc yano 車両用途変更(自社登録処理追加) DBコミット処理を移動。

            return newSalesCarNumber;
        }
        #endregion


        //Add 2015/02/16 arc yano 車両利用用途対応 自社登録処理追加のため、コミット処理の外出し 
        #region DBコミット処理
        /// <summary>
        /// DBコミット処理
        /// </summary>
        /// <param name="salesCar">車両データ</param>
        /// <param name="form">フォームデータ</param>
        /// <param name="ts)">トランザクション</param>
        /// <returns>変更履歴画面</returns>
        private int  DbSubmit(SalesCar salesCar, FormCollection form, TransactionScope ts)
        {
            int ret = 0;
            
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
                    ts.Complete();
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
                        OutputLogger.NLogFatal(e, PROC_NAME_REGIST, FORM_NAME_ENTRY, "");
                        // エラーページに遷移
                        //return View("Error");
                        ret = -1;
                    }
                }
                catch (SqlException e)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        OutputLogger.NLogError(e, PROC_NAME_REGIST, FORM_NAME_ENTRY, "");

                        ModelState.AddModelError("SlipNumber", MessageUtils.GetMessage("E0010", new string[] { "伝票番号", "保存" }));
                        GetEntryViewData(salesCar, form);
                        //return View("CarUsageSettingEntry", salesCar);
                        ret = 1;
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME_REGIST, FORM_NAME_ENTRY, "");
                        //return View("Error");
                        ret = -1;
                    }
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_REGIST, FORM_NAME_ENTRY, "");
                    // エラーページに遷移
                    //return View("Error");
                    ret = -1;
                }
            }
            return ret;
        }
        #endregion
        //Add 2015/02/13 arc yano 車両利用用途対応 変更履歴画面追加
        #region 変更履歴画面
        /// <summary>
        /// 利用用途変更履歴画面表示
        /// </summary>
        /// <returns>変更履歴画面</returns>
        [AuthFilter]
        public ActionResult History()
        {
            FormCollection form = new FormCollection();
            
            form["id"] = "0";
            
            // 出口
            return History(form);
        }

        /// <summary>
        /// 変更履歴画面表示
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>変更履歴画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult History(FormCollection form)
        {

            PaginatedList<BackGroundDemoCar> list = GetHistoryList(form);
            
            return View("CarUsageSettingHistory", list);
        }
        
        /// <summary>
        /// 変更履歴一覧表示
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private PaginatedList<BackGroundDemoCar> GetHistoryList(FormCollection form)
        {

            var query = new BackGroundDemoCarDao(db).GetAllList();

            return new PaginatedList<BackGroundDemoCar>(query, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

        }
        #endregion

        #region 取り消し
        /// <summary>
        ///  取り消し処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="salesCarNumber">管理番号</param>
        /// <param name="selectCarUsage">登録種別</param>
        /// <returns>管理番号</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 固定資産データの削除処理を追加
        /// </history>
        private string SalesCarCancel(FormCollection form, string salesCarNumber, string selectCarUsage, TransactionScope ts)
        {
         
            //-------------------------------------------------------------
            //変更履歴テーブルの取得
            //-------------------------------------------------------------
            BackGroundDemoCar backGroundDemoCar = new BackGroundDemoCarDao(db).GetBySalesCarNumber(salesCarNumber);

            //変更履歴テーブルが無い場合は、取り消しできない。
            if (backGroundDemoCar == null)
            {
                ModelState.AddModelError("", MessageUtils.GetMessage("E0024", new string[] { "変更前のデータが存在しないため、取り消しできません。" }));
                    
                //トランザクションの破棄
                ts.Dispose();

                return salesCarNumber;
                    
                //return Entry(salesCarNumber);
            }

            //-------------------------------------------------------------
            //車両伝票ヘッダの取消
            //-------------------------------------------------------------
            CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(backGroundDemoCar.SlipNumber);
                
            header.DelFlag = "1";

            //----------------------------------------------------------------
            //車両発注の取消
            //----------------------------------------------------------------
            CarPurchaseOrder purchase = new CarPurchaseOrderDao(db).GetByKey(backGroundDemoCar.CarPurchaseOrderNumber);
            // 削除フラグを立てる
                
            purchase.DelFlag = "1";
                
            //----------------------------------------------------------------
            //車両マスタの取消
            //----------------------------------------------------------------
            SalesCar salesCar = new SalesCarDao(db).GetByKey(salesCarNumber);

            // 車両マスタの編集処理
            salesCar = EditSalesCarCancel(salesCar, backGroundDemoCar);


            //----------------------------------------------------------------
            //変更履歴テーブルの論理削除
            //----------------------------------------------------------------
            backGroundDemoCar.DelFlag = "1";


            //Add 2018/06/06 arc yano #3883
            //----------------------------------------------------------------
            //固定資産データの削除(論理削除)
            //----------------------------------------------------------------
            CarFixedAssets rec = new CarFixedAssetsDao(db).GetByKey(salesCarNumber);

            rec.DelFlag = "1";
            rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            rec.LastUpdateDate = DateTime.Now;
            

            //-------------------------------------------------------------
            //DBコミット処理
            //-------------------------------------------------------------
            //Del 2015/02/17 arc yano 車両用途変更(自社登録処理追加) DBコミット処理を移動。
                
            return salesCarNumber;
        }
        #endregion

        #region 画面データ取得
        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="salesCar">モデルデータ</param>
        private void GetEntryViewData(SalesCar salesCar, FormCollection form)
        {

            
            // ブランド名，車種名，グレード名の取得
            if (!string.IsNullOrEmpty(salesCar.CarGradeCode))
            {
                CarGradeDao carGradeDao = new CarGradeDao(db);
                CarGrade carGrade = carGradeDao.GetByKey(salesCar.CarGradeCode);
                if (carGrade != null)
                {
                    salesCar.CarGradeName = carGrade.CarGradeName;
                    try { salesCar.CarName = carGrade.Car.CarName; }
                    catch (NullReferenceException) { }
                    try { salesCar.CarBrandName = carGrade.Car.Brand.CarBrandName; }
                    catch (NullReferenceException) { }
                }
            }

            // お客様指定オイル，タイヤ名の取得
            PartsDao partsDao = new PartsDao(db);
            Parts parts;
            if (!string.IsNullOrEmpty(salesCar.Oil))
            {
                parts = partsDao.GetByKey(salesCar.Oil);
                if (parts != null)
                {
                    ViewData["OilName"] = parts.PartsNameJp;
                }
            }
            if (!string.IsNullOrEmpty(salesCar.Tire))
            {
                parts = partsDao.GetByKey(salesCar.Tire);
                if (parts != null)
                {
                    ViewData["TireName"] = parts.PartsNameJp;
                }
            }


            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), salesCar.NewUsedType, true);
            ViewData["ColorTypeList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), salesCar.ColorType, true);
            ViewData["MileageUnitList"] = CodeUtils.GetSelectListByModel(dao.GetMileageUnitAll(false), salesCar.MileageUnit, false);
            ViewData["CarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), salesCar.CarStatus, true);
            ViewData["UsageTypeList"] = CodeUtils.GetSelectListByModel(dao.GetUsageTypeAll(false), salesCar.UsageType, true);
            ViewData["UsageList"] = CodeUtils.GetSelectListByModel(dao.GetUsageAll(false), salesCar.Usage, true);
            ViewData["CarClassificationList"] = CodeUtils.GetSelectListByModel(dao.GetCarClassificationAll(false), salesCar.CarClassification, true);
            ViewData["MakerWarrantyList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.MakerWarranty, true);
            ViewData["RecordingNoteList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.RecordingNote, true);
            ViewData["ReparationRecordList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.ReparationRecord, true);
            ViewData["FigureList"] = CodeUtils.GetSelectListByModel(dao.GetFigureAll(false), salesCar.Figure, true);
            ViewData["ImportList"] = CodeUtils.GetSelectListByModel(dao.GetImportAll(false), salesCar.Import, true);
            ViewData["GuaranteeList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.Guarantee, true);
            ViewData["InstructionsList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.Instructions, true);
            ViewData["RecycleList"] = CodeUtils.GetSelectListByModel(dao.GetRecycleAll(false), salesCar.Recycle, true);
            ViewData["RecycleTicketList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.RecycleTicket, true);
            ViewData["SteeringList"] = CodeUtils.GetSelectListByModel(dao.GetSteeringAll(false), salesCar.Steering, true);
            ViewData["ChangeColorList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.ChangeColor, true);
            ViewData["LightList"] = CodeUtils.GetSelectListByModel(dao.GetLightAll(false), salesCar.Light, true);
            ViewData["AwList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.Aw, true);
            ViewData["AeroList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.Aero, true);
            ViewData["SrList"] = CodeUtils.GetSelectListByModel(dao.GetSrAll(false), salesCar.Sr, true);
            ViewData["CdList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.Cd, true);
            ViewData["MdList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.Md, true);
            ViewData["NaviTypeList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.NaviType, true);
            ViewData["NaviEquipmentList"] = CodeUtils.GetSelectListByModel(dao.GetNaviEquipmentAll(false), salesCar.NaviEquipment, true);
            ViewData["NaviDashboardList"] = CodeUtils.GetSelectListByModel(dao.GetNaviDashboardAll(false), salesCar.NaviDashboard, true);
            ViewData["SeatTypeList"] = CodeUtils.GetSelectListByModel(dao.GetSeatTypeAll(false), salesCar.SeatType, true);
            ViewData["DeclarationTypeList"] = CodeUtils.GetSelectListByModel(dao.GetDeclarationTypeAll(false), salesCar.DeclarationType, true);
            ViewData["AcquisitionReasonList"] = CodeUtils.GetSelectListByModel(dao.GetAcquisitionReasonAll(false), salesCar.AcquisitionReason, true);
            ViewData["TaxationTypeCarTaxList"] = CodeUtils.GetSelectListByModel(dao.GetTaxationTypeAll(false), salesCar.TaxationTypeCarTax, true);
            ViewData["TaxationTypeAcquisitionTaxList"] = CodeUtils.GetSelectListByModel(dao.GetTaxationTypeAll(false), salesCar.TaxationTypeAcquisitionTax, true);
            ViewData["ExpireTypeList"] = CodeUtils.GetSelectListByModel(dao.GetExpireTypeAll(false), salesCar.ExpireType, false);
            ViewData["CouponPresenceList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.CouponPresence, true);
            ViewData["DocumentCompleteList"] = CodeUtils.GetSelectListByModel(dao.GetDocumentCompleteAll(false), salesCar.DocumentComplete, true);
            ViewData["EraseRegistList"] = CodeUtils.GetSelectListByModel<c_EraseRegist>(dao.GetEraseRegistAll(false), salesCar.EraseRegist, true);
            ViewData["FinanceList"] = CodeUtils.GetSelectListByModel<c_OnOff>(dao.GetOnOffAll(false), salesCar.Finance, true);
            ViewData["OwnershipChangeTypeList"] = CodeUtils.GetSelectListByModel<c_OwnershipChangeType>(dao.GetOwnershipChangeTypeAll(false), salesCar.OwnershipChangeType, true);
            ViewData["FuelList"] = CodeUtils.GetSelectListByModel<c_Fuel>(dao.GetFuelTypeAll(false), salesCar.Fuel, true);
            ViewData["InspectGuidFlagList"] = CodeUtils.GetSelectListByModel(dao.GetNeededAll(false), salesCar.InspectGuidFlag, false);
          
            //ViewData["ChangeCarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), salesCar.CarStatus, true);

            //--------------------------------------------------
            // 利用用途による、コンポーネントの表示、非表示
            //--------------------------------------------------
            string selectedvalue = "";
            if (!ModelState.IsValid)    //検証エラーの場合は選択状態を保持する。
            {
                selectedvalue = form["CarUsageType"];
            }

            if (string.IsNullOrEmpty(salesCar.CarUsage))   //利用用途
            {
                salesCar.CarUsage = "";
            }
            if (string.IsNullOrEmpty(salesCar.CarStatus))    //在庫ステータス
            {
                salesCar.CarStatus = "";
            }
            if (string.IsNullOrEmpty(salesCar.UserCode))    //使用者
            {
                salesCar.UserCode = "";
            }
            if (string.IsNullOrEmpty(salesCar.OwnerCode))   //所有者
            {
                salesCar.OwnerCode = "";
            }

            if (salesCar.CarStatus.Equals("001"))    //在庫
            {
                ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("005", false), form["CarUsageType"], true);
            }
            else
            {
                //Mod 2015/09/08 arc yano #3249 車両用途変更 バグ対応で見つけた既存の不具合(DelFlag がnullの場合も考慮するように判定分を変更)
                if (!string.IsNullOrWhiteSpace(salesCar.DelFlag) && salesCar.DelFlag.Equals("1"))
                {
                    ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("006", false), form["CarUsageType"], true);
                    ViewData["SaveButtonHidden"] = "001";     //保存ボタンを表示しない。
                }
                else if ((salesCar.OwnerCode.Equals("002002")) || (salesCar.UserCode.Equals("002002")) || salesCar.CarUsage.Equals(CAR_DEMO)) //所有者、または使用者が「デモカー」、または利用用途が「デモカー」
                {
                    ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("006", false), form["CarUsageType"], true);
                }
                else if (salesCar.CarUsage.Equals(CAR_RENTAL)) //利用用途がレンタカー
                {
                    ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("006", false), form["CarUsageType"], true);
                }
                else if ((salesCar.OwnerCode.Equals("002004")) || (salesCar.UserCode.Equals("002004")) || salesCar.CarUsage.Equals(CAR_BUSINESS))//所有者、または使用者が「業務車両」、または利用用途が「業務車両」
                {
                    ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("006", false), form["CarUsageType"], true);
                }
                else if (salesCar.CarUsage.Equals(CAR_PUBLICITY)) //利用用途が広報車
                {
                    ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("006", false), form["CarUsageType"], true);
                }
                else if (salesCar.CarUsage.Equals(CAR_LOANER)) //利用用途が代車
                {
                    ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("006", false), form["CarUsageType"], true);
                }
                else
                {
                    ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("006", false), form["CarUsageType"], true);
                    ViewData["SaveButtonHidden"] = "001";     //保存ボタンを表示しない。
                }
            }

            
            Employee emp = HttpContext.Session["Employee"] as Employee;
            if ("999".Equals(emp.SecurityRoleCode))
            {
                salesCar.CarStatusEnabled = true;
            }
            else
            {
                salesCar.CarStatusEnabled = false;
            }

            salesCar.IssueDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.IssueDate);
            string issueDateGengou = "";
            if (salesCar.IssueDateWareki != null)
            {
                issueDateGengou = salesCar.IssueDateWareki.Gengou.ToString();
            }
            ViewData["IssueGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), issueDateGengou, false); //2018/06/22 arc yano #3891
            //ViewData["IssueGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), issueDateGengou, false);

            salesCar.RegistrationDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.RegistrationDate);
            string registrationDateGengou = "";
            if (salesCar.RegistrationDateWareki != null)
            {
                registrationDateGengou = salesCar.RegistrationDateWareki.Gengou.ToString();
            }
            ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), registrationDateGengou, false);  //2018/06/22 arc yano #3891
            //ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), registrationDateGengou, false);

            DateTime parseResult;
            DateTime? firstRegistrationDate = null;
            if (DateTime.TryParse(salesCar.FirstRegistrationYear + "/01", out parseResult))
            {
                firstRegistrationDate = DateTime.Parse(salesCar.FirstRegistrationYear + "/01");
            }
            salesCar.FirstRegistrationDateWareki = JapaneseDateUtility.GetJapaneseDate(firstRegistrationDate);
            string firstRegistrationDateGengou = "";
            if (salesCar.FirstRegistrationDateWareki != null)
            {
                firstRegistrationDateGengou = salesCar.FirstRegistrationDateWareki.Gengou.ToString();
            }
            ViewData["FirstRegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), firstRegistrationDateGengou, false);  //2018/06/22 arc yano #3891
            //ViewData["FirstRegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), firstRegistrationDateGengou, false);

            salesCar.ExpireDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.ExpireDate);
            string expireDateGengou = "";
            if (salesCar.ExpireDate != null)
            {
                expireDateGengou = salesCar.ExpireDateWareki.Gengou.ToString();
            }
            ViewData["ExpireGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), expireDateGengou, false);  //2018/06/22 arc yano #3891
            //ViewData["ExpireGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), expireDateGengou, false);

            salesCar.SalesDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.SalesDate);
            string salesDateGengou = "";
            if (salesCar.SalesDate != null)
            {
                salesDateGengou = salesCar.SalesDateWareki.Gengou.ToString();
            }
            ViewData["SalesGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), salesDateGengou, false);     //2018/06/22 arc yano #3891
            //ViewData["SalesGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), salesDateGengou, false);

            salesCar.InspectionDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.InspectionDate);
            string inspectionDateGengou = "";
            if (salesCar.InspectionDate != null)
            {
                inspectionDateGengou = salesCar.InspectionDateWareki.Gengou.ToString();
            }
            ViewData["InspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), inspectionDateGengou, false);   //2018/06/22 arc yano #3891
            //ViewData["InspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), inspectionDateGengou, false);

            salesCar.NextInspectionDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.NextInspectionDate);
            string nextInspectionDateGengou = "";
            if (salesCar.NextInspectionDate != null)
            {
                nextInspectionDateGengou = salesCar.NextInspectionDateWareki.Gengou.ToString();
            }

            ViewData["NextInspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), nextInspectionDateGengou, false);   //2018/06/22 arc yano #3891
            //ViewData["NextInspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), nextInspectionDateGengou, false);

            if (CommonUtils.DefaultString(salesCar.ManufacturingYear).Length > 4)
            {
                salesCar.ManufacturingYear = salesCar.ManufacturingYear.Substring(0, 4);
            }


            //在庫ステータス
            salesCar.c_CarStatus = dao.GetCarStatusByKey(salesCar.CarStatus);

        }
        #endregion

        /// <summary>
        /// 車両伝票ヘッダ追加編集
        /// </summary>
        /// <param name="carSalesHeader">車両伝票ヘッダ(登録内容)</param>
        /// <returns>車両伝票ヘッダクラス</returns>
        private CarSalesHeader EditCarSalesHeaderForInsert(FormCollection form, CarSalesHeader carSalesHeader, string slipNumber, string customerCode, string salesType, DateTime? changeDate, SalesCar salesCar)
        {
            carSalesHeader.SlipNumber = slipNumber;
            carSalesHeader.RevisionNumber = 1;
            carSalesHeader.QuoteDate = changeDate;
            carSalesHeader.QuoteExpireDate = changeDate;
            carSalesHeader.SalesOrderDate = changeDate;
            carSalesHeader.SalesDate = changeDate;
            carSalesHeader.SalesOrderStatus = "002";                                    //受注固定
            carSalesHeader.CustomerCode = customerCode;
            carSalesHeader.DepartmentCode = "021";                                      //FIATGroup課
            carSalesHeader.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carSalesHeader.SalesType = salesType;
            carSalesHeader.Vin = salesCar.Vin;
            carSalesHeader.HotStatus = "A";
            carSalesHeader.SalesCarNumber = salesCar.SalesCarNumber;
            carSalesHeader.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carSalesHeader.CreateDate = DateTime.Now;
            carSalesHeader.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carSalesHeader.LastUpdateDate = DateTime.Now;

            //税率の設定
            if (changeDate == null) //振替日未設定
            {
                carSalesHeader.ConsumptionTaxId = "001";
                carSalesHeader.Rate = 0;
            }
            else
            {
                carSalesHeader.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(changeDate);
                carSalesHeader.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(carSalesHeader.ConsumptionTaxId));
            }
            return carSalesHeader;
        }

        /// <summary>
        /// 車両伝票ヘッダ更新編集
        /// </summary>
        /// <param name="carSalesHeader">車両伝票ヘッダ(登録内容)</param>
        /// <returns>車両伝票ヘッダクラス</returns>
        /// <history>
        /// 2023/10/05 yano #4184 【車両伝票入力】販売諸費用の[中古車点検・整備費用][中古車継承整備費用]の削除 見直し
        /// 2023/01/11 yano #4158 【車両伝票入力】任意保険料入力項目の追加
        /// </history>
        private CarSalesHeader EditCarSalesHeaderForUpdate(CarSalesHeader carSalesHeader, string slipNumber, string salesCarNumber)
        {

            //車両マスタビューの内容を設定
            carSalesHeader = SetCarSalesHeader(carSalesHeader, salesCarNumber);

            carSalesHeader.ApprovalFlag = "0";
            carSalesHeader.CampaignCode1 = "";
            carSalesHeader.CampaignCode2 = "";
            carSalesHeader.ExteriorColorCode = "";
            carSalesHeader.ExteriorColorName = "";
            carSalesHeader.InteriorColorCode = "";
            carSalesHeader.InteriorColorName = "";
            carSalesHeader.MileageUnit = "1";
            carSalesHeader.RequestPlateNumber = "";
            carSalesHeader.RegistrationType = "";
            carSalesHeader.MorterViecleOfficialCode = "";
            carSalesHeader.OwnershipReservation = "";
            carSalesHeader.CarLiabilityInsuranceType = "";
            carSalesHeader.Memo = "";
            carSalesHeader.TaxFreeFieldName = "";
            carSalesHeader.TaxationFieldName = "";
            carSalesHeader.PossesorCode = "";
            carSalesHeader.UserCode = "";
            carSalesHeader.PrincipalPlace = "";
            carSalesHeader.VoluntaryInsuranceType = "001";        //Mod 2023/01/11 yano #4158
            carSalesHeader.VoluntaryInsuranceCompanyName = "";
            carSalesHeader.PaymentPlanType = "";
            carSalesHeader.TradeInMakerName1 = "";
            carSalesHeader.TradeInCarName1 = "";
            carSalesHeader.TradeInClassificationTypeNumber1 = "";
            carSalesHeader.TradeInModelSpecificateNumber1 = "";
            carSalesHeader.TradeInManufacturingYear1 = "";
            carSalesHeader.TradeInMileageUnit1 = "1";
            carSalesHeader.TradeInVin1 = "";
            carSalesHeader.TradeInRegistrationNumber1 = "";
            carSalesHeader.TradeInMakerName2 = "";
            carSalesHeader.TradeInCarName2 = "";
            carSalesHeader.TradeInClassificationTypeNumber2 = "";
            carSalesHeader.TradeInModelSpecificateNumber2 = "";
            carSalesHeader.TradeInManufacturingYear2 = "";
            carSalesHeader.TradeInMileageUnit2 = "1";
            carSalesHeader.TradeInVin2 = "";
            carSalesHeader.TradeInRegistrationNumber2 = "";
            carSalesHeader.TradeInMakerName3 = "";
            carSalesHeader.TradeInCarName3 = "";
            carSalesHeader.TradeInClassificationTypeNumber3 = "";
            carSalesHeader.TradeInModelSpecificateNumber3 = "";
            carSalesHeader.TradeInManufacturingYear3 = "";
            carSalesHeader.TradeInMileageUnit3 = "1";
            carSalesHeader.TradeInVin3 = "";
            carSalesHeader.TradeInRegistrationNumber3 = "";
            carSalesHeader.LoanCodeA = "";
            carSalesHeader.AuthorizationNumberA = "";
            carSalesHeader.LoanCodeB = "";
            carSalesHeader.AuthorizationNumberB = "";
            carSalesHeader.LoanCodeC = "";
            carSalesHeader.AuthorizationNumberC = "";
            carSalesHeader.DelFlag = "0";
            carSalesHeader.TradeInEraseRegist1 = "";
            carSalesHeader.TradeInEraseRegist2 = "";
            carSalesHeader.TradeInEraseRegist3 = "";
            carSalesHeader.SalesPrice = 0;
            carSalesHeader.DiscountAmount = 0;
            carSalesHeader.TaxationAmount = 0;
            carSalesHeader.ShopOptionAmount = 0;
            carSalesHeader.ShopOptionTaxAmount = 0;
            carSalesHeader.MakerOptionAmount = 0;
            carSalesHeader.MakerOptionTaxAmount = 0;
            carSalesHeader.SubTotalAmount = 0;
            carSalesHeader.CarLiabilityInsurance = 0;
            carSalesHeader.CarWeightTax = 0;
            carSalesHeader.AcquisitionTax = 0;
            carSalesHeader.NumberPlateCost = 0;
            carSalesHeader.RequestNumberCost = 0;
            carSalesHeader.TaxFreeTotalAmount = 0;
            carSalesHeader.InspectionRegistFee = 0;
            carSalesHeader.ParkingSpaceFee = 0;
            carSalesHeader.PreparationFee = 0;
            carSalesHeader.RecycleControlFee = 0;
            carSalesHeader.RequestNumberFee = 0;
            carSalesHeader.TradeInAppraisalFee = 0;
            carSalesHeader.TradeInMaintenanceFee = 0;
            carSalesHeader.SalesCostTotalAmount = 0;
            carSalesHeader.SalesCostTotalTaxAmount = 0;
            carSalesHeader.OtherCostTotalAmount = 0;
            carSalesHeader.CostTotalAmount = 0;
            carSalesHeader.TotalTaxAmount = 0;
            carSalesHeader.GrandTotalAmount = 0;
            carSalesHeader.TradeInAppropriation1 = 0;
            carSalesHeader.TradeInAppropriation2 = 0;
            carSalesHeader.TradeInAppropriation3 = 0;
            carSalesHeader.TradeInTotalAmount = 0;
            carSalesHeader.TradeInTaxTotalAmount = 0;
            carSalesHeader.TradeInUnexpiredCarTaxTotalAmount = 0;
            carSalesHeader.TradeInRemainDebtTotalAmount = 0;
            carSalesHeader.TradeInAppropriationTotalAmount = 0;
            carSalesHeader.PaymentTotalAmount = 0;
            carSalesHeader.PaymentCashTotalAmount = 0;
            carSalesHeader.LoanPrincipalAmount = 0;
            carSalesHeader.LoanFeeAmount = 0;
            carSalesHeader.LoanTotalAmount = 0;
            carSalesHeader.LoanPrincipalA = 0;
            carSalesHeader.LoanTotalAmountA = 0;
            carSalesHeader.LoanPrincipalB = 0;
            carSalesHeader.LoanTotalAmountB = 0;
            carSalesHeader.LoanPrincipalC = 0;
            carSalesHeader.LoanTotalAmountC = 0;
            carSalesHeader.ParkingSpaceFeeTax = 0;
            carSalesHeader.SalesTax = 0;
            carSalesHeader.DiscountTax = 0;
            carSalesHeader.LastEditScreen = "000";

            carSalesHeader.CostAreaCode = "";             //Add 2023/10/05 yano #4184
            carSalesHeader.TradeInHolderName1 = "";       //Add 2023/10/05 yano #4184
            carSalesHeader.TradeInHolderName2 = "";       //Add 2023/10/05 yano #4184
            carSalesHeader.TradeInHolderName3 = "";       //Add 2023/10/05 yano #4184


            // Add 2014/10/31 arc amii 車両ステータス追加対応
            //carSalesHeader.SalesCarNumber = salesCarNumber;

            return carSalesHeader;
        }

        /// <summary>
        /// 車両伝票データ設定
        /// </summary>
        /// <param name="carSalesHeader">車両伝票データ(登録内容)</param>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号</param>
        /// <returns>車両伝票データ</returns>
        private CarSalesHeader SetCarSalesHeader(CarSalesHeader carSalesHeader, string salesCarNumber)
        {

            var query =
                (from a in db.SalesCar
                 join b in db.WV_CarMaster on a.CarGradeCode equals b.CarGradeCode
                 where a.SalesCarNumber.Equals(salesCarNumber)
                 select new
                 {
                     b.MakerName,
                     b.CarBrandName,
                     b.CarName,
                     b.CarGradeName,
                     b.CarGradeCode,
                     a.ModelName,
                     a.Mileage
                 }
                 ).FirstOrDefault();


            carSalesHeader.MakerName = query.MakerName;
            carSalesHeader.CarBrandName = query.CarBrandName;
            carSalesHeader.CarName = query.CarName;
            carSalesHeader.CarGradeName = query.CarGradeName;
            carSalesHeader.CarGradeCode = query.CarGradeCode;
            carSalesHeader.ModelName = query.ModelName;
            carSalesHeader.Mileage = query.Mileage;

            return carSalesHeader;
        }

        /// <summary>
        /// 車両発注引当追加編集
        /// </summary>
        /// <param name="CarPurchaseOrder">車両発注引当(登録内容)</param>
        /// <returns>車両発注引当</returns>
        private CarPurchaseOrder EditCarPurcahseOrderForInsert(FormCollection form, CarPurchaseOrder carPurchaseOrder, string carPurchaseOrderNumber, string slipNumber,DateTime? changeDate, SalesCar salesCar)
        {

            carPurchaseOrder.CarPurchaseOrderNumber = carPurchaseOrderNumber;
            carPurchaseOrder.SlipNumber = slipNumber;
            carPurchaseOrder.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchaseOrder.CreateDate = System.DateTime.Now;
            carPurchaseOrder.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchaseOrder.LastUpdateDate = System.DateTime.Now;
            carPurchaseOrder.DelFlag = "0";

            // Add 2014/10/31 arc amii 車両ステータス追加対応
            carPurchaseOrder.SalesCarNumber = salesCar.SalesCarNumber;

            carPurchaseOrder.PurchaseOrderStatus = "1";
            carPurchaseOrder.ReservationStatus = "1";
            carPurchaseOrder.PurchasePlanStatus = "1";
            carPurchaseOrder.RegistrationStatus = "1";
            carPurchaseOrder.Vin = salesCar.Vin;
            carPurchaseOrder.RegistrationDate = changeDate;
            carPurchaseOrder.LastUpdateDate = System.DateTime.Now;
            carPurchaseOrder.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            return carPurchaseOrder;
        }

        /// <summary></summary>>
        /// <param name="carPurchaseOrder">車両データ設定</param>
        /// <param name="salesCarNumber">管理番号</param>
        /// <param name="revisionNumber">改訂番号</param>
        /// <returns>車両データ</returns>
        private SalesCar SetSalesCar(SalesCar salesCar, CarSalesHeader header, string salesCarNumber, string locationCode, string carUsage, string InspectGuideFlag, string InspectGuideMemo, string selectCarUsage)
        {
            Customer customer = new CustomerDao(db).GetByKey(header.CustomerCode);

            salesCar.CarStatus = "006";             //納車済
            salesCar.LocationCode = locationCode;
            salesCar.OwnerCode = customer.CustomerCode;
            salesCar.UserCode = customer.CustomerCode;
            salesCar.UserName = customer.CustomerName;
            salesCar.PossesorName = customer.CustomerName;
            salesCar.SalesDate = header.SalesDate;
            salesCar.LastUpdateDate = System.DateTime.Now;
            salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //Add 2014/10/30 arc amii 車両ステータス追加対応
            salesCar.CarUsage = carUsage;

            //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
            //除却・入力取消以外は車検案内可否と車検案内備考を登録する
            if ((selectCarUsage != REGIST_RELEASE) && (selectCarUsage != REGIST_CANCEL))
            {
                salesCar.InspectGuidFlag = InspectGuideFlag; //車検案内可否
                salesCar.InspectGuidMemo = InspectGuideMemo; //車検案内備考欄
            }
  
            return salesCar;
        }

        /// <summary>
        /// 車両伝票データ設定その２(伝票ステータス、新中区分)
        /// </summary>
        /// <param name="carSalesHeader">車両伝票データ(登録内容)</param>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号</param>
        /// <returns>車両伝票データ</returns>
        private CarSalesHeader SetCarSalesHeader2(CarSalesHeader carSalesHeader, string salesCarNumber)
        {
            var query =
                (from a in db.SalesCar
                 where a.SalesCarNumber.Equals(salesCarNumber)
                 && a.DelFlag.Equals("0")
                 select new
                 {
                     a.NewUsedType
                 }
                 ).FirstOrDefault();


            carSalesHeader.SalesOrderStatus = "005";        //納車済
            carSalesHeader.NewUsedType = query.NewUsedType;

            return carSalesHeader;
        }

        /// <summary>
        /// 車両仕入データ追加編集
        /// </summary>
        /// <param name="newSalesCarNumber">車両管理番号(新)</param>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="locationCode">ロケーションコード</param>
        /// <param name="changeDate">振替日</param>
        /// <param name="selectCarUsage">種別</param>
        /// <param name="oldCarPurchase">振替前の車両仕入データ</param>
        /// <param name="undepreciatedBalance">未償却残高</param>
        /// <returns>車両仕入データ</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 
        /// 2018/06/06 arc yano #3883 タマ表改善 財務価格を更新する処理を追加する
        /// </history>
        private CarPurchase EditCarPurchaseForInsert(string newSalesCarNumber, string customerCode, string locationCode, DateTime? changeDate, string selectCarUsage, CarPurchase oldCarPurchase, decimal? undepreciatedBalance)
        {
            CarPurchase carPurchase = new CarPurchase();

            //仕入id
            carPurchase.CarPurchaseId = Guid.NewGuid();
            //査定id
            carPurchase.CarAppraisalId = Guid.NewGuid();
            //仕入ステータス
            carPurchase.PurchaseStatus = "002"; //仕入済
            //入庫日
            carPurchase.PurchaseDate = changeDate;
            //仕入先
            carPurchase.SupplierCode = customerCode;
            //入庫ロケーション
            carPurchase.PurchaseLocationCode = locationCode;
            //部門コード
            carPurchase.DepartmentCode = "021";
            //担当者
            carPurchase.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //車両本体価格
            carPurchase.VehicleAmount = 0;
            //仕入価格
            carPurchase.Amount = 0;
            //消費税
            carPurchase.TaxAmount = 0;
            //管理番号
            carPurchase.SalesCarNumber = newSalesCarNumber;
            //作成者
            carPurchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //作成日
            carPurchase.CreateDate = System.DateTime.Now;
            //最終更新者
            carPurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //最終更新日
            carPurchase.LastUpdateDate = System.DateTime.Now;
            //車両本体消費税
            carPurchase.VehicleTax = 0;
            //車両本体税込価格
            carPurchase.VehicleAmount = 0;
            //仕入税込価格
            carPurchase.TotalAmount = 0;
            //メタリック価格
            carPurchase.MetallicPrice = 0;
            //オプション価格
            carPurchase.OptionPrice = 0;
            //ファーム価格
            carPurchase.FirmPrice = 0;
            //ディスカウント価格
            carPurchase.DiscountPrice = 0;
            //加装価格(税抜)
            carPurchase.EquipmentPrice = 0;
            //加修価格(税抜)
            carPurchase.RepairPrice = 0;
            //その他価格
            carPurchase.OthersPrice = 0;
            //削除フラグ
            carPurchase.DelFlag = "0";
            //自税充当税込
            carPurchase.CarTaxAppropriateAmount = 0;
            //リサイクル税込
            carPurchase.RecycleAmount = 0;
            //入庫区分
            carPurchase.CarPurchaseType = "006";        //自社仕入
            //メタリック消費税
            carPurchase.MetallicTax = 0;
            //メタリック税込価格
            carPurchase.MetallicAmount = 0;
            //オプション消費税
            carPurchase.OptionTax = 0;
            //オプション税込価格
            carPurchase.OptionAmount = 0;
            //ファーム消費税
            carPurchase.FirmTax = 0;
            //ファーム税込価格
            carPurchase.FirmAmount = 0;
            //ディスカウント消費税
            carPurchase.DiscountTax = 0;
            //ディスカウント税込価格
            carPurchase.DiscountAmount = 0;
            //その他消費税
            carPurchase.OthersTax = 0;
            //その他税込価格
            carPurchase.OthersAmount = 0;
            //オークション落札料
            carPurchase.AuctionFeePrice = 0;
            //オークション落札料消費税
            carPurchase.AuctionFeeTax = 0;
            //オークション落札料税込
            carPurchase.AuctionFeeAmount = 0;
            //
            carPurchase.CarTaxAppropriatePrice = 0;
            //加修価格(税抜)
            carPurchase.RecyclePrice = 0;

            //税率の設定
            if (changeDate == null) //振替日未設定
            {
                carPurchase.ConsumptionTaxId = "001";
                carPurchase.Rate = 0;
            }
            else
            {
                carPurchase.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(changeDate);
                carPurchase.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(carPurchase.ConsumptionTaxId));
            }

            //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
            //最終更新画面
            carPurchase.LastEditScreen = "000";

            //Add 2017/06/14 arc nakayama #3771_車両管理_他勘定仕入の内訳変更 自社登録の場合は自社登録フラグを立てる
            if (selectCarUsage == REGIST_OWN)
            {
                carPurchase.RegistOwnFlag = "1";

                carPurchase.FinancialAmount = (oldCarPurchase != null ? oldCarPurchase.FinancialAmount : 0);        //Add 2018/06/06 arc yano #3883

                carPurchase.FinancialAmountLocked = "1";                                                            //Add 2018/08/28 yano #3922
            }
            else
            {
                carPurchase.RegistOwnFlag = "0";

                carPurchase.FinancialAmount = undepreciatedBalance;     //Add 2018/06/06 arc yano #3883
            }


            return carPurchase;
        }


        /// <summary>
        /// 車両仕入データ編集
        /// </summary>
        /// <param name="carPurchase">車両仕入データ(登録内容)</param>
        /// <returns>車両仕入データ</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 減価償却処理の追加
        /// </history>
        private CarPurchase EditCarPurchaseForUpdate(CarPurchase oldCarPurchase, string newSalesCarNumber, string customerCode, string locationCode, DateTime? changeDate, decimal? undepreciatedBalance)
        {

            CarPurchase carPurchase = new CarPurchase();

            //仕入id
            carPurchase.CarPurchaseId = Guid.NewGuid();
            //車両発注引当ID
            carPurchase.CarPurchaseOrderNumber = oldCarPurchase.CarPurchaseOrderNumber;
            //車両査定ID
            carPurchase.CarAppraisalId = oldCarPurchase.CarAppraisalId;
            //仕入ステータス
            carPurchase.PurchaseStatus = oldCarPurchase.PurchaseStatus;
            //入庫日
            carPurchase.PurchaseDate = changeDate;
            //仕入先コード
            carPurchase.SupplierCode = customerCode;
            //仕入先ロケーションコード
            carPurchase.PurchaseLocationCode = locationCode;
            //部門コード
            carPurchase.DepartmentCode = oldCarPurchase.DepartmentCode;
            //担当者コード
            carPurchase.EmployeeCode = oldCarPurchase.EmployeeCode;
            //車両本体価格
            carPurchase.VehiclePrice = oldCarPurchase.VehiclePrice;
            //メタリック価格
            carPurchase.MetallicPrice = oldCarPurchase.MetallicPrice;
            //オプション価格
            carPurchase.OptionPrice = oldCarPurchase.OptionPrice;
            //ファーム価格
            carPurchase.FirmPrice = oldCarPurchase.FirmPrice;
            //ディスカウント価格
            carPurchase.DiscountPrice = oldCarPurchase.DiscountPrice;
            //加装価格(税抜)
            carPurchase.EquipmentPrice = oldCarPurchase.EquipmentPrice;
            //加修価格(税抜)
            carPurchase.RepairPrice = oldCarPurchase.RepairPrice;
            //その他価格
            carPurchase.OthersPrice = oldCarPurchase.OthersPrice;
            //仕入価格
            carPurchase.Amount = oldCarPurchase.Amount;
            //消費税
            carPurchase.TaxAmount = oldCarPurchase.TaxAmount;
            //管理番号
            carPurchase.SalesCarNumber = newSalesCarNumber;
            //作成者
            carPurchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //作成日時
            carPurchase.CreateDate = DateTime.Now;
            //最終更新者
            carPurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //最終更新日時
            carPurchase.LastUpdateDate = DateTime.Now; ;
            //削除フラグ
            carPurchase.DelFlag = oldCarPurchase.DelFlag;
            //抹消登録
            carPurchase.EraseRegist = oldCarPurchase.EraseRegist;
            //備考
            carPurchase.Memo = oldCarPurchase.Memo;
            //仕入日
            carPurchase.SlipDate = oldCarPurchase.SlipDate;
            //自税充当税込
            carPurchase.CarTaxAppropriateAmount = oldCarPurchase.CarTaxAppropriateAmount;
            //リサイクル税込
            carPurchase.RecycleAmount = oldCarPurchase.RecycleAmount;
            //入庫区分
            carPurchase.CarPurchaseType = "006";        //自社仕入
            //車両本体消費税
            carPurchase.VehicleTax = oldCarPurchase.VehicleTax;
            //車両本体税込価格
            carPurchase.VehicleAmount = oldCarPurchase.VehicleAmount;
            //メタリック消費税
            carPurchase.MetallicTax = oldCarPurchase.MetallicTax;
            //メタリック税込価格
            carPurchase.MetallicAmount = oldCarPurchase.MetallicAmount;
            //オプション消費税
            carPurchase.OptionTax = oldCarPurchase.OptionTax;
            //オプション税込価格
            carPurchase.OptionAmount = oldCarPurchase.OptionAmount;
            //ファーム消費税
            carPurchase.FirmTax = oldCarPurchase.FirmTax;
            //ファーム税込価格
            carPurchase.FirmAmount = oldCarPurchase.FirmAmount;
            //ディスカウント消費税
            carPurchase.DiscountTax = oldCarPurchase.DiscountTax;
            //ディスカウント税込価格
            carPurchase.DiscountAmount = oldCarPurchase.DiscountAmount;
            //その他消費税
            carPurchase.OthersTax = oldCarPurchase.OthersTax;
            //その他税込価格
            carPurchase.OthersAmount = oldCarPurchase.OthersAmount;
            //仕入税込価格
            carPurchase.TotalAmount = oldCarPurchase.TotalAmount;
            //オークション落札料
            carPurchase.AuctionFeePrice = oldCarPurchase.AuctionFeePrice;
            //オークション落札料消費税
            carPurchase.AuctionFeeTax = oldCarPurchase.AuctionFeeTax;
            //オークション落札料税込
            carPurchase.AuctionFeeAmount = oldCarPurchase.AuctionFeeAmount;
            //自税充当
            carPurchase.CarTaxAppropriatePrice = oldCarPurchase.CarTaxAppropriatePrice;
            //リサイクル
            carPurchase.RecyclePrice = oldCarPurchase.RecyclePrice;
            //加装価格(消費税)
            carPurchase.EquipmentTax = oldCarPurchase.EquipmentTax;
            //加装価格(税込)
            carPurchase.EquipmentAmount = oldCarPurchase.EquipmentAmount;
            //加修価格(消費税)
            carPurchase.RepairTax = oldCarPurchase.RepairTax;
            //加修価格(税込)
            carPurchase.RepairAmount = oldCarPurchase.RepairAmount;
            //？？？？
            carPurchase.CarTaxAppropriateTax = null;
            //税率Id
            carPurchase.ConsumptionTaxId = oldCarPurchase.ConsumptionTaxId;
            //消費税率
            carPurchase.Rate = oldCarPurchase.Rate;
            //キャンセルフラグ
            carPurchase.CancelFlag = null;
            
            //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
            //最終更新画面
            carPurchase.LastEditScreen = "000";

            //Add 2018/06/06 arc yano #3883
            carPurchase.FinancialAmount = undepreciatedBalance;             //未償却残高

            return carPurchase;
        }

        /// <summary>
        /// 車両データ編集
        /// </summary>
        /// <param name="SalesCar">車両データ</param>
        /// <returns>車両データ(再作成)</returns>
        /// <history>
        /// 2018/03/20 arc yano #3870
        /// </history>
        private SalesCar EditSalesCarForUpdate(SalesCar oldSalesCar, string newSalesCarNumber, string oldSalesCarNumber, string procName, string locationCode, string carUsage, string InspectGuideFlag, string InspectGuideMemo)
        {
            SalesCar salesCar = new SalesCar();

            //管理番号
            salesCar.SalesCarNumber = newSalesCarNumber;    //新管理番号に振り直し
            salesCar.CarGradeCode = oldSalesCar.CarGradeCode;
            salesCar.NewUsedType = "U";                 //中古車
            salesCar.ColorType = oldSalesCar.ColorType;
            salesCar.ExteriorColorCode = oldSalesCar.ExteriorColorCode;
            salesCar.ExteriorColorName = oldSalesCar.ExteriorColorName;
            salesCar.ChangeColor = oldSalesCar.ChangeColor;
            salesCar.InteriorColorCode = oldSalesCar.InteriorColorCode;
            salesCar.InteriorColorName = oldSalesCar.InteriorColorName;
            salesCar.ManufacturingYear = oldSalesCar.ManufacturingYear;
            salesCar.CarStatus = "001";                                                //在庫ステータス
            salesCar.LocationCode = locationCode;                                      //ロケーション
            salesCar.OwnerCode = oldSalesCar.OwnerCode;
            salesCar.Steering = oldSalesCar.Steering;
            salesCar.SalesPrice = oldSalesCar.SalesPrice;
            salesCar.IssueDate = oldSalesCar.IssueDate;
            salesCar.MorterViecleOfficialCode = oldSalesCar.MorterViecleOfficialCode;
            salesCar.RegistrationNumberType = oldSalesCar.RegistrationNumberType;
            salesCar.RegistrationNumberKana = oldSalesCar.RegistrationNumberKana;
            salesCar.RegistrationNumberPlate = oldSalesCar.RegistrationNumberPlate;
            salesCar.RegistrationDate = oldSalesCar.RegistrationDate;
            salesCar.FirstRegistrationYear = oldSalesCar.FirstRegistrationYear;
            salesCar.CarClassification = oldSalesCar.CarClassification;
            salesCar.Usage = oldSalesCar.Usage;
            salesCar.UsageType = oldSalesCar.UsageType;
            salesCar.Figure = oldSalesCar.Figure;
            salesCar.MakerName = oldSalesCar.MakerName;
            salesCar.Capacity = oldSalesCar.Capacity;
            salesCar.MaximumLoadingWeight = oldSalesCar.MaximumLoadingWeight;
            salesCar.CarWeight = oldSalesCar.CarWeight;
            salesCar.TotalCarWeight = oldSalesCar.TotalCarWeight;
            salesCar.Vin = oldSalesCar.Vin;
            salesCar.Length = oldSalesCar.Length;
            salesCar.Width = oldSalesCar.Width;
            salesCar.Height = oldSalesCar.Height;
            salesCar.FFAxileWeight = oldSalesCar.FFAxileWeight;
            salesCar.FRAxileWeight = oldSalesCar.FRAxileWeight;
            salesCar.RFAxileWeight = oldSalesCar.RFAxileWeight;
            salesCar.RRAxileWeight = oldSalesCar.RRAxileWeight;
            salesCar.ModelName = oldSalesCar.ModelName;
            salesCar.EngineType = oldSalesCar.EngineType;
            salesCar.Displacement = oldSalesCar.Displacement;
            salesCar.Fuel = oldSalesCar.Fuel;
            salesCar.ModelSpecificateNumber = oldSalesCar.ModelSpecificateNumber;
            salesCar.ClassificationTypeNumber = oldSalesCar.ClassificationTypeNumber;
            salesCar.PossesorName = oldSalesCar.PossesorName;
            salesCar.PossesorAddress = oldSalesCar.PossesorAddress;
            salesCar.UserName = oldSalesCar.UserName;
            salesCar.UserAddress = oldSalesCar.UserAddress;
            salesCar.PrincipalPlace = oldSalesCar.PrincipalPlace;
            salesCar.ExpireType = oldSalesCar.ExpireType;
            salesCar.ExpireDate = oldSalesCar.ExpireDate;
            salesCar.Mileage = oldSalesCar.Mileage;
            salesCar.MileageUnit = oldSalesCar.MileageUnit;
            salesCar.Memo = procName + ":" + oldSalesCarNumber;    　//メモには登録種別＋管理番号(旧)
            salesCar.DocumentComplete = oldSalesCar.DocumentComplete;
            salesCar.DocumentRemarks = oldSalesCar.DocumentRemarks;
            salesCar.SalesDate = null;                              //納車日はnull
            salesCar.InspectionDate = oldSalesCar.InspectionDate;
            salesCar.NextInspectionDate = oldSalesCar.NextInspectionDate;
            salesCar.UsVin = oldSalesCar.UsVin;
            salesCar.MakerWarranty = oldSalesCar.MakerWarranty;
            salesCar.RecordingNote = oldSalesCar.RecordingNote;
            salesCar.ProductionDate = oldSalesCar.ProductionDate;
            salesCar.ReparationRecord = oldSalesCar.ReparationRecord;
            salesCar.Oil = oldSalesCar.Oil;
            salesCar.Tire = oldSalesCar.Tire;
            salesCar.KeyCode = oldSalesCar.KeyCode;
            salesCar.AudioCode = oldSalesCar.AudioCode;
            salesCar.Import = oldSalesCar.Import;
            salesCar.Guarantee = oldSalesCar.Guarantee;
            salesCar.Instructions = oldSalesCar.Instructions;
            salesCar.Recycle = oldSalesCar.Recycle;
            salesCar.RecycleTicket = oldSalesCar.RecycleTicket;
            salesCar.CouponPresence = oldSalesCar.CouponPresence;
            salesCar.Light = oldSalesCar.Light;
            salesCar.Aw = oldSalesCar.Aw;
            salesCar.Aero = oldSalesCar.Aero;
            salesCar.Sr = oldSalesCar.Sr;
            salesCar.Cd = oldSalesCar.Cd;
            salesCar.Md = oldSalesCar.Md;
            salesCar.NaviType = oldSalesCar.NaviType;
            salesCar.NaviEquipment = oldSalesCar.NaviEquipment;
            salesCar.NaviDashboard = oldSalesCar.NaviDashboard;
            salesCar.SeatColor = oldSalesCar.SeatColor;
            salesCar.SeatType = oldSalesCar.SeatType;
            salesCar.Memo1 = oldSalesCar.Memo1;
            salesCar.Memo2 = oldSalesCar.Memo2;
            salesCar.Memo3 = oldSalesCar.Memo3;
            salesCar.Memo4 = oldSalesCar.Memo4;
            salesCar.Memo5 = oldSalesCar.Memo5;
            salesCar.Memo6 = oldSalesCar.Memo6;
            salesCar.Memo7 = oldSalesCar.Memo7;
            salesCar.Memo8 = oldSalesCar.Memo8;
            salesCar.Memo9 = oldSalesCar.Memo9;
            salesCar.Memo10 = oldSalesCar.Memo10;
            salesCar.DeclarationType = oldSalesCar.DeclarationType;
            salesCar.AcquisitionReason = oldSalesCar.AcquisitionReason;
            salesCar.TaxationTypeCarTax = oldSalesCar.TaxationTypeCarTax;
            salesCar.TaxationTypeAcquisitionTax = oldSalesCar.TaxationTypeAcquisitionTax;
            salesCar.CarTax = oldSalesCar.CarTax;
            salesCar.CarWeightTax = oldSalesCar.CarWeightTax;
            salesCar.CarLiabilityInsurance = oldSalesCar.CarLiabilityInsurance;
            salesCar.AcquisitionTax = oldSalesCar.AcquisitionTax;
            salesCar.RecycleDeposit = oldSalesCar.RecycleDeposit;
            salesCar.CreateEmployeeCode = oldSalesCar.CreateEmployeeCode;
            salesCar.CreateDate = oldSalesCar.CreateDate;
            salesCar.LastUpdateEmployeeCode = oldSalesCar.LastUpdateEmployeeCode;
            salesCar.LastUpdateDate = oldSalesCar.LastUpdateDate;
            salesCar.DelFlag = oldSalesCar.DelFlag;
            salesCar.EraseRegist = oldSalesCar.EraseRegist;
            salesCar.UserCode = oldSalesCar.UserCode;
            salesCar.ApprovedCarNumber = oldSalesCar.ApprovedCarNumber;
            salesCar.ApprovedCarWarrantyDateFrom = oldSalesCar.ApprovedCarWarrantyDateFrom;
            salesCar.ApprovedCarWarrantyDateTo = oldSalesCar.ApprovedCarWarrantyDateTo;
            salesCar.Finance = oldSalesCar.Finance;
            salesCar.InspectGuidFlag = InspectGuideFlag;        //Mod 2018/03/20 arc yano #3870
            salesCar.InspectGuidMemo = InspectGuideMemo;        //Mod 2018/03/20 arc yano #3870
            salesCar.CarUsage = "";                                                    // 利用用途

            return salesCar;
        }

        /// <summary>
        /// 車両ステータス変更履歴データ作成
        /// </summary>
        /// <param name="W_MakeDemoCar">車両ステータス変更履歴</param>
        /// <returns>車両ステータス変更履歴</returns>
        private BackGroundDemoCar EditBackGroundDemoCarForUpdate(string slipNumber, string carPurchaseOrderNumber, string procType, string oldSalesCarNumber, string newSalesCarNumber, DateTime? changeDate, SalesCar oldSalesCar)
        {
            BackGroundDemoCar demoCar = new BackGroundDemoCar();

            //デモカーID
            demoCar.DemoCarId = Guid.NewGuid();
            //伝票番号
            demoCar.SlipNumber = slipNumber;
            //車両発注依頼番号
            demoCar.CarPurchaseOrderNumber = carPurchaseOrderNumber;
            //登録種別
            demoCar.ProcType = procType;
            //振替日
            demoCar.ProcDate = changeDate;
            //入庫ロケーション
            demoCar.LocationCode = oldSalesCar.LocationCode;
            //所有者コード
            demoCar.OwnerCode = oldSalesCar.OwnerCode;
            //使用者コード
            demoCar.UserCode = oldSalesCar.UserCode;
            //在庫ステータス
            demoCar.CarStatus = oldSalesCar.CarStatus;
            //旧管理番号
            demoCar.SalesCarNumber = oldSalesCarNumber;
            //新管理番号
            demoCar.NewSalesCarNumber = newSalesCarNumber;
            //作成者
            demoCar.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //作成日
            demoCar.CreateDate = System.DateTime.Now;
            //最終更新者
            demoCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //最終更新日
            demoCar.LastUpdateDate = System.DateTime.Now;
            //変更ステータス
            demoCar.DelFlag = "0";
            //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
            //車検案内可否
            demoCar.InspectGuidFlag = oldSalesCar.InspectGuidFlag;
            //車検案内備考
            demoCar.InspectGuidMemo = oldSalesCar.InspectGuidMemo;

            return demoCar;
        }


        #region 車両マスタ取り消し編集
        //Add 2014/10/30 arc amii 車両ステータス追加対応
        /// <summary>
        /// 取り消し処理  車両マスタ編集
        /// </summary>
        /// <param name="salesCar"></param>
        /// <returns></returns>
        private SalesCar EditSalesCarCancel(SalesCar salesCar, BackGroundDemoCar backGroundDemoCar)
        {
                        
            salesCar.CarStatus = backGroundDemoCar.CarStatus;
            salesCar.LocationCode = backGroundDemoCar.LocationCode;
            salesCar.OwnerCode = backGroundDemoCar.OwnerCode;
            salesCar.UserCode = backGroundDemoCar.UserCode;
            if (!string.IsNullOrEmpty(backGroundDemoCar.UserCode))
            {
                salesCar.UserName = new CustomerDao(db).GetByKey(backGroundDemoCar.UserCode).CustomerName;
            }
            if (!string.IsNullOrEmpty(backGroundDemoCar.OwnerCode))
            {
                salesCar.PossesorName = new CustomerDao(db).GetByKey(backGroundDemoCar.OwnerCode).CustomerName;
            }
            salesCar.SalesDate = null;
            salesCar.LastUpdateDate = System.DateTime.Now;
            salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //Add 2016/01/28 arc nakayama #3265_車両用途変更画面にて社有車として処理する場合に車検案内には不可を設定
            salesCar.InspectGuidFlag = backGroundDemoCar.InspectGuidFlag;
            salesCar.InspectGuidMemo = backGroundDemoCar.InspectGuidMemo;
            
            salesCar.CarUsage = "";

            return salesCar;
        }
        #endregion

        #region 減価償却

        /// <summary>
        /// 固定資産テーブル登録
        /// </summary>
        /// <param name="salesCarNumber">車両管理番号</param>
        /// <param name="changeDate">取得日</param>
        /// <param name="legalUsefulLives">法定耐用年数</param>
        /// <param name="salesCar">車両マスタ</param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        private void EditCarFixedAssets(string salesCarNumber, DateTime ? changeDate, int legalUsefulLives, SalesCar salesCar)
        {
            CarPurchase carPurchase = new CarPurchaseDao(db).GetBySalesCarNumber(salesCarNumber);

            //耐用年数の取得
            int usefulLives = CalculateUsefulLives(changeDate, legalUsefulLives, salesCar);
            
            //既存データが存在する場合は更新
            CarFixedAssets rec = new CarFixedAssetsDao(db).GetByKey(salesCarNumber, true);

            if (rec == null)
            {
                rec = new CarFixedAssets();

                rec.SalesCarNumber = salesCarNumber;
                rec.Vin = (carPurchase.SalesCar != null ? carPurchase.SalesCar.Vin : "");
                rec.UsefulLives = usefulLives;
                rec.AcquisitionDate = changeDate;
                rec.LossDate = null;
                rec.AcquisitionPrice = carPurchase.FinancialAmount;
                rec.UndepreciatedBalance = carPurchase.FinancialAmount;
                rec.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                rec.CreateDate = DateTime.Now;
                rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                rec.LastUpdateDate = DateTime.Now;
                rec.DelFlag = "0";

                db.CarFixedAssets.InsertOnSubmit(rec);
            }
            else
            {
                rec.Vin = (carPurchase.SalesCar != null ? carPurchase.SalesCar.Vin : "");
                rec.UsefulLives = usefulLives;
                rec.AcquisitionDate = changeDate;
                rec.LossDate = null;
                rec.AcquisitionPrice = carPurchase.FinancialAmount;
                rec.UndepreciatedBalance = carPurchase.FinancialAmount;
                rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                rec.LastUpdateDate = DateTime.Now;
                rec.DelFlag = "0";
            }
        }

        /// <summary>
        /// 耐用年数計算
        /// </summary>
        /// <param name="salesCarNumber">車両管理番号</param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        private int CalculateUsefulLives(DateTime? changeDate, int legalUsefulLives, SalesCar salesCar)
        {
            int ret = 0;

            //耐用年数の計算
            if (salesCar != null)
            {
                //新車の場合は法定耐用年数を設定、中古車の場合は計算で設定
                if (!string.IsNullOrWhiteSpace(salesCar.NewUsedType) && salesCar.NewUsedType.Equals("N"))
                {
                    ret = legalUsefulLives;
                }
                else
                {
                    //初年度登録日
                    DateTime firstRegistrationDate = salesCar.FirstRegistrationDate ?? DaoConst.SQL_DATETIME_MIN;
                    //固定資産取得日
                    DateTime getDate = changeDate ?? DaoConst.SQL_DATETIME_MAX;

                    //初年度登録日から振替日までの経過年数を取得(月数で取得)
                    decimal monthdiff = (getDate.Month + (getDate.Year - firstRegistrationDate.Year) * 12) - firstRegistrationDate.Month;

                    //耐用年数 = 法定耐用年数(月変換)－経過月数 + 経過月数×20%
                    ret = (int)Math.Truncate( ((legalUsefulLives * 12 - monthdiff) + monthdiff / 5) / 12 ); 

                    //耐用年数が２年未満の場合は２年に設定
                    if (ret < 2)
                    {
                        ret = 2;
                    }                    
                }
            }

            return ret;
        }

        /// <summary>
        /// 未償却残高を算出する
        /// </summary>
        /// <param name="rec">車両固定資産データ</param>
        /// <param name="changeDate">除却日</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 未償却残高
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        private decimal CalculateUndepreciatedBalance(CarFixedAssets rec, DateTime ? changeDate)
        {
            decimal undepreciatedBalance = 0;
            int monthdiff = 0;

            //取得価格がnullの場合は計算しない
            if (rec != null)
            {

                ////Add 2018/08/28 yano #3922 取得価格がnullの場合は、車両仕入データから再取得する
                //if (rec.AcquisitionPrice == null)
                //{
                //    CarPurchase carpurchase = new CarPurchaseDao(db).GetBySalesCarNumber(rec.SalesCarNumber);

                //    if (carpurchase != null)
                //    {
                //        rec.AcquisitionPrice = carpurchase.FinancialAmount;
                //    }
                //}

                //耐用年数マスタから償却率を取得する
                DepreciationRate ret = new DepreciationRateDao(db).GetByKey(rec.UsefulLives ?? 0);

                //耐用年数データが存在した場合は計算
                if (ret != null)
                {
                    undepreciatedBalance = (rec.AcquisitionPrice ?? 0);         //未償却残高

                    DateTime workFromDate;                                      //作業用日付(from)
                    DateTime workToDate;                                　      //作業用日付(to)

                    //fromDate～toDate分までをループ処理で算出を行う
                    for (workFromDate = workToDate = new DateTime((rec.AcquisitionDate ?? DateTime.Now).Year, (rec.AcquisitionDate ?? DateTime.Now).Month, 1); workToDate < new DateTime((changeDate ?? DateTime.Now).Year, (changeDate ?? DateTime.Now).Month, 1) ; workToDate = workToDate.AddMonths(1))
                    {
                        //決算月(6月)の場合は一旦未償却残高を計算
                        if (workToDate.Month == 6)
                        {
                            //workFromDate～workDateの間の月数を算出する
                            monthdiff = (workToDate.Month + (workToDate.Year - workFromDate.Year) * 12) - workFromDate.Month + 1;

                            //未償却残高の算出
                            undepreciatedBalance = Math.Round(undepreciatedBalance - (undepreciatedBalance * (ret.Rate ?? 0) * monthdiff / 12), 0, MidpointRounding.AwayFromZero);

                            //決算月の翌月を設定
                            workFromDate = workToDate.AddMonths(1);
                        }
                    }

                    //for文抜けた後にworkToData > workfromDateの場合はもう一度未償却残高の計算を行う
                    //workFromDate～workDateの間の月数を算出する
                    if (workToDate > workFromDate)
                    {
                        monthdiff = (workToDate.Month + (workToDate.Year - workFromDate.Year) * 12) - workFromDate.Month + 1;

                        //未償却残高の算出
                        undepreciatedBalance = Math.Round(undepreciatedBalance - (undepreciatedBalance * (ret.Rate ?? 0) * monthdiff / 12), 0, MidpointRounding.AwayFromZero);
                    }
                 
                    //固定資産テーブルの更新
                    rec.LossDate = changeDate;
                    rec.UndepreciatedBalance = undepreciatedBalance;                                //未償却残高
                    rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;      //最終更新者
                    rec.LastUpdateDate = DateTime.Now;                                              //最終更新日
                }
            }

            return undepreciatedBalance;
        }

        #endregion
    }
}
