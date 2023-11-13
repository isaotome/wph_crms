using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CrmsDao;
using System.Data.Linq;
using Microsoft.VisualBasic;

namespace Crms.Models
{
    public interface IServiceSalesOrderService
    {        
        /// <summary>
        /// 新しいサービス伝票を作成する
        /// </summary>
        /// <param name="header">伝票ヘッダ</param>
        /// <param name="lines">伝票明細</param>
        void CreateServiceSalesOrder(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines);

        /// <summary>
        /// サービス伝票を見積保存する
        /// </summary>
        /// <param name="header">伝票ヘッダ</param>
        /// <param name="lines">伝票明細</param>
        void Quote(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines);

        /// <summary>
        /// サービス伝票を作業履歴にする
        /// </summary>
        /// <param name="header">伝票ヘッダ</param>
        /// <param name="lines">伝票明細</param>
        void History(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines);

        /// <summary>
        /// サービス伝票を作業中止にする
        /// </summary>
        /// <param name="header">伝票ヘッダ</param>
        /// <param name="lines">伝票明細</param>
        void Stop(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines);

        //Mod 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更　引数の追加(List<PartsPurchaseOrder> list)
        /// <summary>
        /// サービス伝票を保存する（受注処理以降）
        /// </summary>
        /// <param name="header">伝票ヘッダ</param>
        /// <param name="lines">伝票明細</param>
        void SalesOrder(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines, ref List<PartsPurchaseOrder> list);

        /// <summary>
        /// サービス伝票をキャンセルする
        /// </summary>
        /// <param name="header">伝票ヘッダ</param>
        /// <param name="lines">伝票明細</param>
        void Cancel(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines);

        /// <summary>
        /// 赤伝票を作成する
        /// </summary>
        /// <param name="header">伝票ヘッダ</param>
        /// <returns></returns>
        ServiceSalesHeader CreateAkaden(ServiceSalesHeader header);

        /// <summary>
        /// 黒伝票を作成する
        /// </summary>
        /// <param name="header">伝票ヘッダ</param>
        /// <returns></returns>
        ServiceSalesHeader CreateKuroden(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines);

        /// <summary>
        /// 合計を計算する
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加 未使用のため、コメントアウト
        /// </history>
        //void CalcLineAmount(ServiceSalesHeader header);

        /// <summary>
        /// 税込に変換する
        /// </summary>
        /// <param name="lines"></param>
        void SetDiscountAmountWithTax(EntitySet<ServiceSalesLine> lines);

        /// <summary>
        /// 税抜きに変換する
        /// </summary>
        /// <param name="lines"></param>
        void SetDiscountAmountWithoutTax(EntitySet<ServiceSalesLine> lines);

        /// <summary>
        /// 値引レコードか判定する
        /// </summary>
        /// <param name="code">コード値</param>
        /// <returns></returns>
        bool IsDiscountRecord(string code);

        /// <summary>
        /// 伝票をロックする
        /// 条件１：既存の伝票であること
        /// 条件２：最新のリビジョンであること（DelFlag='0'）
        /// 条件３：既に自分がロックしていないこと
        /// </summary>
        /// <param name="header"></param>
        void ProcessLock(ServiceSalesHeader header);

        /// <summary>
        /// 伝票ロック解除
        /// 条件１：ロックしているのが自分であること
        /// 条件２：もしくは、強制解除であること
        /// </summary>
        /// <param name="header"></param>
        void ProcessUnLock(ServiceSalesHeader header);

        /// <summary>
        /// 伝票をロックしている担当者名を取得する
        /// 条件１：ProcessSessionId!=null
        /// 条件２：ProcessSessionControl!=null
        /// 条件３：自分以外がロックしている
        /// </summary>
        /// <param name="header"></param>
        /// <returns>ロック中の担当者名</returns>
        string GetProcessLockUser(ServiceSalesHeader header);

        /// <summary>
        /// ロックを自分のものにする
        /// </summary>
        /// <param name="header"></param>
        void ProcessLockUpdate(ServiceSalesHeader header);

        //Add 2015/03/17 arc nakayama 伝票修正対応　特定の権限の人のみ[伝票修正]ボタンを表示させる
        /// <summary>
        /// 権限のチェック
        /// </summary>
        /// <param name="code"></param>
        /// <returns>システム管理者/支店長だった場合:True  それ以外:False</returns>
        bool CheckApplicationRole(string EmployeeCode);

        //Add 2015/03/17 arc nakayama 伝票修正対応　修正中かどうかそうでないかを返す
        /// <summary>
        /// 修正中かどうかのチェック（修正中:True  それ以外:False）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>修正中:True  それ以外:False</returns>
        bool CheckModification(string SlipNumber, int RevisionNumber);

        //Add 2015/03/17 arc nakayama 伝票修正対応　伝票を修正中にする
        /// <summary>
        /// 伝票を修正中にする
        /// </summary>
        /// <param name="code"></param>
        /// <returns>なし</returns>
        void ModificationStart(ServiceSalesHeader header);

        //Add 2015/03/17 arc nakayama 伝票修正対応　伝票の修正をキャンセルする
        /// <summary>
        /// 伝票の修正をキャンセルする（修正を行えないようにする）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>なし</returns>
        void ModificationCancel(ServiceSalesHeader header);

        //Add 2015/03/17 arc nakayama 伝票修正対応　過去に赤黒処理を行った元伝票でないかチェックする
        /// <summary>
        /// 過去に赤黒処理を行った元伝票でないかチェックする（赤黒経歴なし:True  それ以外:False）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>赤黒経歴なし:True  赤黒経歴あり:False</returns>
        bool AkakuroCheck(string SlipNumber);

        //Add 2015/03/18 arc nakayama 伝票修正対応　過去に修正を行った伝票かどうかをチェックする
        /// <summary>
        /// 過去に修正処理を行った伝票でないかチェックする（修正履歴あり:True  修正履歴なし:False）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>修正履歴あり:True  修正履歴なし:False</returns>
        bool CheckModifiedReason(string SlipNumber);

        //Add 2015/03/18 arc nakayama 伝票修正対応　修正履歴を取得する
        /// <summary>
        /// 修正履歴を取得する（該当伝票の全履歴）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>修正履歴（修正時間・修正者・修正理由）</returns>
        void GetModifiedHistory(ServiceSalesHeader header);

        //Add 2015/03/17 arc nakayama 伝票修正対応　修正履歴を作成する
        /// <summary>
        /// 修正履歴を作成する（レコード作成）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>なし</returns>
        void CreateModifiedHistory(ServiceSalesHeader header);

        //Add 2015/03/19 arc nakayama 伝票修正対応　経理締めが本締めになったら削除する（本締めしたのに伝票が修正されないようにするため）
        /// <summary>
        /// 対象年月が納車年月の伝票が修正中だった場合、伝票情報を削除する
        /// </summary>
        /// <param name="code">対象日付（YYYY/MM/DD）</param>
        /// <param name="code">部門コード</param>
        /// <returns>なし</returns>
        void CloseEnd(DateTime targetMonth, string departmentCode);

        //Add 2015/08/05 arc nakayama #3221_無効となっている部門や車両等が設定されている納車確認書が印刷出来ない
        /// <summary>
        /// 引き継ぎメモだけを更新する
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        /// <returns>なし</returns>
        void UpDateMemoServiceSalesHeader(ServiceSalesHeader header);

       
        //Add 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更 引当処理は外部公開メソッドに変更する
         /// <summary>
        /// 引当処理
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ情報</param>
        /// <param name="line">サービス伝票明細情報</param>
        /// <return>発注情報</return>
        List<PartsPurchaseOrder> Hikiate(ref ServiceSalesHeader header, ref EntitySet<ServiceSalesLine> lines, string shikakariLocationCode);

                /// <summary>
        /// 入荷時の引当処理
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ情報</param>
        /// <param name="lines">サービス伝票明細情報</param>
        /// <param name="shikakariLocationCode">仕掛ロケーションコード</param>
        /// <param name="OrderType">発注区分</param>
        /// <param name="OrderPartsNumber">発注伝票番号</param>
        /// <param name="PurchasePartsNumber">入荷部品番号</param>
        /// <param name="PurchaseQuantity">入荷数</param>
        /// <param name="price">単価</param>
        /// <param name="Amount">金額</param>
        /// <param name="EmployeeCode">社員コード</param>
        /// <param name="SupplierCode">仕入先コード</param>
        /// <history>
        /// 2016/08/05 arc yano #3625 部品入荷入力　サービス伝票に紐づく入荷処理の引当処理 引当処理の引数(入荷ロケーション)追加
        /// </history>
        void PurchaseHikiate(ref ServiceSalesHeader header, ref EntitySet<ServiceSalesLine> lines, string shikakariLocationCode, string OrderType, string OrderPartsNumber, string PurchasePartsNumber, decimal? PurchaseQuantity, decimal? price, decimal? Amount, string EmployeeCode, string SupplierCode, string PurchaseLocationCode);

        /// <summary>
        /// 赤処理時の返金用入金予定作成
        /// </summary>
        /// <param name="header">サービス伝票</param>
        /// <history>
        /// Add 2016/05/23 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法
        /// </history>
        void CreateBackAmountAkaden(string SlipNumber);
    
           /// <summary>
        /// 部品移動処理(移動伝票作成＆在庫更新)
        /// </summary>
        /// <param name="departureLocationCode">出発ロケーション</param>
        /// <param name="arrivalLocationCode">到着ロケーション</param>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="quantity">数量</param>
        /// <param name="transferType">移動種別</param>
        /// <param name="updatePartsStock">部品在庫更新フラグ(true:更新する false:更新しない)</param>
        /// <history>
        /// 2016/06/13 arc yano #3571 メソッド名の変更と引数(updatePartsStock)の追加
        /// </history>
        void PartsTransfer(string departureLocationCode, string arrivalLocationCode, string partsNumber, decimal quantity, string slipNumber, string transferType, bool updatePartsStock);
    
        /// <summary>
        /// サービス伝票の引当済数の取得
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ情報</param>
        /// <param name="lines">サービス伝票明細情報</param>
        /// <hisotry>
        /// Add 2016/11/11 arc yano #3656 新規作成
        /// </hisotry>
        void ResetProvisionQuantity(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines);

        /// <summary>
        /// 引当前処理処理(明細行が削除された等により、引当済在庫の戻し処理)
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ情報</param>
        /// <param name="lines">サービス伝票明細情報</param>
        /// <return>発注情報</return>
        /// <history>
        /// 2017/01/31 arc yano #3566 サービス伝票入力　部門変更時の在庫の再引当 本メソッドを外部公開できるように変更
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 在庫リストの取得方法の変更(部門→ロケーションではなく、部門→倉庫→ロケーションで取得する)
        /// 2016/02/22 arc yano #3434 サービス伝票  消耗品(SP)の対応 原価０の部品の対応と同様の対応を行う
        /// 2016/02/17 arc yano #3435_サービス伝票　原価０(社達品)の部品の対応
        ///                         在庫判断で「社達」の場合は、引当を行わないように対応する
        /// </history>
        void PreProcessHikiate(ref ServiceSalesHeader header, ref EntitySet<ServiceSalesLine> lines, string shikakariLocationCode);

        /// <summary>
        /// 在庫情報を取得する
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="partsNumber">部品番号</param>
        /// <returns></returns>
        /// <history>
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 新規作成
        /// </history>
        List<PartsStock> GetStockListByTransfer(ServiceSalesHeader header, string partsNumber);

        /// <summary>
        /// 入金予定を作成する
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="lines">サービス伝票明細</param>
        /// <param name="lines">サービス伝票支払</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/22 yano #3930 入金実績リスト　マイナスの入金予定ができている状態で実績削除した場合の残高不正 外部アクセスできるようにする
        /// </history>
        void CreateReceiptPlan(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines, List<ServiceSalesPayment> paymentList);
    
    }

    public class ServiceSalesOrderService : IServiceSalesOrderService
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// 在庫サービス
        /// </summary>
        private IStockService stockService;

        /// <summary>
        /// 担当者コード
        /// </summary>
        private string sessionEmployeeCode;
        private Employee sessionEmployee;
        private DateTime sessionDate;


        /// <summary>
        /// 定数
        /// </summary>
        /// <history>
        /// Add 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更
        /// </history>
        private const string TYPE_ORDER_EO    = "002";      //オーダー区分「E/O」
        private const string STS_STOCK        = "999";      //在庫判断「在庫」
        private const string STS_BEGINWORK    = "003";      //伝票ステータス=「作業中」
        private const string STS_ENDWORK      = "004";      //伝票ステータス=「作業終了」
        private const string STS_CONFIRMED    = "005";      //伝票ステータス=「納車確認書確認済」
        private const string TTYPE_WORK       = "003";      //移動種別 =「仕掛」
        private const string TTYPE_CANCELWORK = "007";      //移動種別 =「仕掛取消」
        private const string TTYPE_PROVISION         = "006";      //移動種別 =「自動引当」 //Add 2016/06/13 arc yano #3571
        private const string TTYPE_CANCELPROVISION   = "008";      //移動種別 =「引当解除」 //Add 2016/06/13 arc yano #3571
        private const string TCLASS_PARTS     = "002";      //種別 =「部品」

        //Add 2016/04/04 arc yano #3441
        private static readonly List<string> excludeList = new List<string>() { "003", "004" }; //請求先タイプ = 「クレジット」「ローン」

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public ServiceSalesOrderService(CrmsLinqDataContext context)
        {
            db = context;
            sessionEmployee = (Employee)HttpContext.Current.Session["Employee"];
            sessionEmployeeCode = sessionEmployee.EmployeeCode;
            sessionDate = DateTime.Now;
            stockService = new StockService(context);
        }

        #region IServiceSalesOrderService メンバ

        #region 新しいサービス伝票を作成する
        public void CreateServiceSalesOrder(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines)
        {
            //新規の時は伝票番号を採番する
            if (header.RevisionNumber == 0 && string.IsNullOrEmpty(header.SlipNumber))
            {
                header.SlipNumber = (new SerialNumberDao(db)).GetNewSlipNumber();
                header.CreateEmployeeCode = sessionEmployeeCode;
                header.CreateDate = sessionDate;
                header.ServiceOrderStatus = "001"; // 見積
            }

            //古いリビジョンは削除
            List<ServiceSalesHeader> delList = new ServiceSalesOrderDao(db).GetListByLessThanRevision(header.SlipNumber, header.RevisionNumber);
            foreach (var d in delList)
            {
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                // 明細を削除
                foreach (var l in d.ServiceSalesLine)
                {
                    if (!CommonUtils.DefaultString(l.DelFlag).Equals("1"))
                    {
                        l.LastUpdateDate = sessionDate;
                        l.LastUpdateEmployeeCode = sessionEmployeeCode;
                        l.DelFlag = "1";
                    }
                }
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                // 支払方法を削除
                foreach (var p in d.ServiceSalesPayment)
                {
                    if (!CommonUtils.DefaultString(p.DelFlag).Equals("1"))
                    {
                        p.LastUpdateDate = sessionDate;
                        p.LastUpdateEmployeeCode = sessionEmployeeCode;
                        p.DelFlag = "1";
                    }
                }
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                // ヘッダを削除
                if (!CommonUtils.DefaultString(d.DelFlag).Equals("1"))
                {
                    d.LastUpdateDate = sessionDate;
                    d.LastUpdateEmployeeCode = sessionEmployeeCode;
                    d.DelFlag = "1";
                    if (d.ProcessSessionControl != null)
                    {
                        ProcessSessionControl control = new ProcessSessionControlDao(db).GetByKey(d.ProcessSessionId);
                        db.ProcessSessionControl.DeleteOnSubmit(control);
                        d.ProcessSessionControl = null;
                    }

                }
            }

            //リビジョンを1つ上げる
            header.RevisionNumber++;
            header.CreateDate = sessionDate;
            header.CreateEmployeeCode = sessionEmployeeCode;
            header.LastUpdateEmployeeCode = sessionEmployeeCode;
            header.LastUpdateDate = sessionDate;
            header.DelFlag = "0";
            header.ProcessSessionControl = new ProcessSessionControl();
            header.ProcessSessionControl.ProcessSessionId = Guid.NewGuid();
            header.ProcessSessionControl.TableName = "ServiceSalesHeader";
            header.ProcessSessionControl.EmployeeCode = sessionEmployeeCode;
            header.ProcessSessionControl.CreateDate = sessionDate;

            string parentServiceWorkCode = "";
            string parentClassification = "";

            //Add 2016/05/10 arc yano #3513
            ResetOrderQuantity(header, lines);      //発注数の再設定

            foreach (var l in lines)
            {
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                if (CommonUtils.DefaultString(l.ServiceType).Equals("001") && !parentServiceWorkCode.Equals(l.ServiceWorkCode))
                {
                    parentServiceWorkCode = l.ServiceWorkCode;
                    ServiceWork serviceWork = new ServiceWorkDao(db).GetByKey(parentServiceWorkCode);
                    parentClassification = serviceWork != null ? serviceWork.Classification1 : "";
                }
                l.SlipNumber = header.SlipNumber;
                l.RevisionNumber = header.RevisionNumber;
                l.ServiceWorkCode = parentServiceWorkCode;
                l.Classification1 = parentClassification;
                l.CreateDate = sessionDate;
                l.CreateEmployeeCode = header.CreateEmployeeCode;
                l.LastUpdateDate = sessionDate;
                l.LastUpdateEmployeeCode = sessionEmployeeCode;
                l.DelFlag = "0";
                l.PartsNumber = Strings.StrConv(l.PartsNumber, VbStrConv.Narrow, 0);
                //ADD 2014/02/20 ookubo
                l.ConsumptionTaxId = header.ConsumptionTaxId;
                l.Rate = header.Rate;

                // ブランクをNULLに変換
                if (string.IsNullOrEmpty(l.SetMenuCode))
                {
                    l.SetMenuCode = null;
                }
                if (string.IsNullOrEmpty(l.ServiceMenuCode))
                {
                    l.ServiceMenuCode = null;
                }
                if (string.IsNullOrEmpty(l.PartsNumber))
                {
                    l.PartsNumber = null;
                }
                if (string.IsNullOrEmpty(l.RequestComment))
                {
                    l.RequestComment = null;
                }
                if (string.IsNullOrEmpty(l.WorkType))
                {
                    l.WorkType = null;
                }
                if (string.IsNullOrEmpty(l.EmployeeCode))
                {
                    l.EmployeeCode = null;
                }
                if (string.IsNullOrEmpty(l.SupplierCode))
                {
                    l.SupplierCode = null;
                }
                if (string.IsNullOrEmpty(l.StockStatus))
                {
                    l.StockStatus = null;
                }
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                if (!CommonUtils.DefaultString(l.ServiceType).Equals("002"))
                {
                    l.LineType = null;
                }
                db.ServiceSalesLine.InsertOnSubmit(l);
            }


        }
        #endregion

        #region 見積保存
        public void Quote(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines)
        {
            CreateServiceSalesOrder(header, lines);
            CreateServiceSalesPayment(header, lines);
            header.ServiceOrderStatus = "001";
        }
        #endregion

        #region 作業履歴
        public void History(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines)
        {
            CreateServiceSalesOrder(header, lines);
            CreateServiceSalesPayment(header, lines);
            header.ServiceOrderStatus = "009";
            header.KeepsCarFlag = false;               //ADD 2015/4/26 #3180 ookubo
            header.WorkingEndDate = DateTime.Now;
        }
        #endregion

        #region 作業中止
        public void Stop(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines)
        {
            CreateServiceSalesOrder(header, lines);
            CreateServiceSalesPayment(header, lines);
            header.ServiceOrderStatus = "010";
            header.KeepsCarFlag = false;          //ADD 2015/07/01 arc nakayama #3219_サービス伝票の車両預かり中のチェックフラグ処理（その２） 作業中止も車両預かり中フラグを落とす
        }
        #endregion



        //Mod 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更 引当ロケーションへの移動の廃止
        #region 受注以降の保存
        /// <summary>
        /// 受注以降の保存
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        /// <param name="lines">サービス伝票明細行</param>
        /// <param name="list">発注データ</param>
        /// <history>
        /// 2022/07/05 yano #4142 【サービス伝票】外注依頼書表示の際に支払データが作成されない不具合の対応
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 部門から倉庫を割り出し、そこからロケーションを取得するように変更する
        /// 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更　引当処理の追加
        /// </history>
        public void SalesOrder(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines,  ref List<PartsPurchaseOrder> list)
        {

            //Add 2015/03/17 arc nakayama 伝票修正対応　伝票修正ボタンが押された場合は修正情報を作成する（修正中にする）
            if (header.ActionType == "ModificationStart")
            {
                ModificationStart(header);
                return; 
            }
            //Add 2015/03/17 arc nakayama 伝票修正対応　修正キャンセルが押された場合は修正情報を削除する（修正をキャンセルする）
            if (header.ActionType == "ModificationCancel")
            {
                ModificationCancel(header);
                return;
            }

            //Del 2015/10/30 arc yano #3289 引当ロケーションへの部品移動は行わない
            //引当ロケーションを取得する
            /*
            string hikiateLocationCode = "";
            Location hikiateLocation = stockService.GetHikiateLocation(header.DepartmentCode);
            hikiateLocationCode = hikiateLocation.LocationCode;
            */

            //仕掛ロケーションを取得する
            string shikakariLocationCode = "";

            //Mod 2016/08/13 arc yano #3596
            //部門から倉庫を割り出す
            DepartmentWarehouse departmentWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
            
            Location shikakariLocation = stockService.GetShikakariLocation(departmentWarehouse.WarehouseCode);
            //Location shikakariLocation = stockService.GetShikakariLocation(header.DepartmentCode);

            shikakariLocationCode = shikakariLocation.LocationCode;

            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            //受注以降は前回との差分を計算する
            if (!CommonUtils.DefaultString(header.ActionType).Equals("SalesOrder") && !CommonUtils.DefaultString(header.ActionType).Equals("Quote"))
            {
                StockReserve(header);
            }

            //**共通処理**
            CreateServiceSalesOrder(header, lines);

            /*---------------------------------------------------------------------------
             * //Mod 2015/10/28 arc yano #3289 受注以降の保存時は必ず引当を行う         　
              ---------------------------------------------------------------------------*/
            //伝票ステータス≠「納車済」の場合、引当処理を行う ※納車済の場合(伝票修正・赤伝等)は引当(解除)は行わず、直接在庫ロケーションへ  
            if(!header.ServiceOrderStatus.Equals("006"))
            {
                list = Hikiate(ref header, ref lines, shikakariLocationCode);
            }
            
            switch (header.ActionType)
            {
                //Mod 2022/07/05 yano #4142 伝票の更新はデフォルト動作に変更
                //case "Update":          //伝票の更新
                //    //入金予定(再)作成
                //    CreateReceiptPlan(header, lines, CreateServiceSalesPayment(header, lines));

                //    break;
                case "SalesOrder":      //受注
                    //入金予定(再)作成
                    CreateReceiptPlan(header, lines, CreateServiceSalesPayment(header, lines));

                   
                    UpdateSalesCar(header);
                    header.SalesOrderDate = DateTime.Today;
                    header.ServiceOrderStatus = "002";
                    break;
                case "StartWorking":    //作業開始
                    //入金予定(再)作成
                    CreateReceiptPlan(header, lines, CreateServiceSalesPayment(header, lines));

                    //作業開始登録
                    //Mod 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更  
                    //StartWorking(lines, hikiateLocationCode, shikakariLocationCode);
                    StartWorking(header, lines, shikakariLocationCode);
                    header.WorkingStartDate = DateTime.Now;
                    header.ServiceOrderStatus = "003";
                    break;
                case "EndWorking":      //作業終了
                    //入金予定(再)作成
                    CreateReceiptPlan(header, lines, CreateServiceSalesPayment(header, lines));

                    header.WorkingEndDate = DateTime.Now;
                    header.ServiceOrderStatus = "004";
                    break;
                case "SalesConfirm":    //納車確認書
                    //入金予定(再)作成
                    CreateReceiptPlan(header, lines, CreateServiceSalesPayment(header, lines));

                    header.ServiceOrderStatus = "005";
                    break;
                case "Sales":           //納車
                    // 支払方法計算
                    List<ServiceSalesPayment> payList = CreateServiceSalesPayment(header, lines);

                    //Mod 2016/05/26 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法 実績も黒に振り替えたため、通常通りの処理にする
                    // 赤黒処理の時は残高を精算する
                    //if (header.SlipNumber.Contains("-2"))
                    //{
                    //    ResetReceiptPlan(header, lines, payList);
                    //}
                    //else
                    //{
                        // 通常は入金予定再作成
                        CreateReceiptPlan(header, lines, payList);
                    //}

                    //在庫を更新する
                    stockService.UpdatePartsStock(lines.ToList(), shikakariLocationCode);

                    //画面入力した納車日を登録する
                    header.ServiceOrderStatus = "006";
                    //ADD 2014/11/25 #3135 ookubo
                    //車両預り中を初期化する
                    header.KeepsCarFlag = false;

                    // 車両マスタを更新
                    UpdateSalesCar(header);

                    break;

                // Add 2015/03/18 arc nakayama 伝票修正対応　修正完了処理追加
                case "ModificationEnd":　// 修正完了

                    // 支払方法計算
                    List<ServiceSalesPayment> payListForModification = CreateServiceSalesPayment(header, lines);

                    // 入金予定再作成
                    CreateReceiptPlan(header, lines, payListForModification);

                    //在庫を更新する（数量の一番多いロケーションに在庫を戻して　再度引き当てる　※伝票修正のみの処理）
                    stockService.UpdatePartsStockForModification(lines.ToList(), header);

                    //画面入力した納車日を登録する
                    header.ServiceOrderStatus = "006";

                    //車両預り中を初期化する
                    header.KeepsCarFlag = false;

                    // 車両マスタを更新
                    UpdateSalesCar(header);

                    //修正履歴作成
                    CreateModifiedHistory(header);

                    //修正中でなくす(修正中レコード削除) 
                    ModificationCancel(header);

                    break;

                //どれにも当てはまらない場合は更新と同様の動作とする
                default:  //Add 2022/07/05 yano #4142
                    //入金予定(再)作成
                    CreateReceiptPlan(header, lines, CreateServiceSalesPayment(header, lines));
                    break;
            }
        }
        #endregion

        #region キャンセル処理
        /// <summary>
        /// キャンセル処理
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        /// <history>
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応
        /// 2017/01/31 arc yano #3566 サービス伝票入力　部門変更時の在庫の再引当 部門の変更された場合は一度解除する
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚リストの取得方法の変更(部門ではなく倉庫をキーとしてリストを取得する)
        /// 2016/05/13 arc yano #3528 入金実績が存在するサービス伝票の削除について マイナスの入金予定を作成＆タスクアラートの廃止
        /// 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更　引当処理の追加
        /// </history>
        public void Cancel(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines)
        {

            //新規伝票追加
            CreateServiceSalesOrder(header, lines);
            CreateServiceSalesPayment(header, lines);
            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            //見積以降のみ処理
            if (!CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("001"))
            {

                //伝票の部門コードから倉庫を特定する
                DepartmentWarehouse departmentWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
                    
                //Mod 2017/01/31 arc yano #3566
                //Mod 2016/08/13 arc yano #3596
                //Add 2015/10/28 arc yano #3289
                //倉庫コードをキーにリストを取得
                Location shikakariLocation = stockService.GetShikakariLocation(departmentWarehouse != null ? departmentWarehouse.WarehouseCode : "");
                string shikakariLocationCode = shikakariLocation != null ? shikakariLocation.LocationCode : "";
                
                //----------------------------------------------------------------------
                //サービス伝票の入力値がDBの登録値と変更されているかどうかをチェックする
                //----------------------------------------------------------------------
                ServiceSalesHeader dbheader = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);
                
                //画面の入力値とDBの登録値が異なる場合(=部門が変更された場合)はDBの部門の在庫の引当を解除する
                if (dbheader != null && !dbheader.DepartmentCode.Equals(header.DepartmentCode))
                {
                    EntitySet<ServiceSalesLine> dblines = new EntitySet<ServiceSalesLine>();
                    string dbshikakariLocationCode = "";

                    DepartmentWarehouse dwrec = CommonUtils.GetWarehouseFromDepartment(db, dbheader.DepartmentCode);

                    //仕掛ロケーション設定
                    if (dwrec != null)
                    {
                        Location lrec = stockService.GetShikakariLocation(dwrec.WarehouseCode);

                        dbshikakariLocationCode = lrec != null ? lrec.LocationCode : "";
                    }

                    //Mod 2017/02/08 arc yano #3620
                    //引当解除(仕掛前処理で実行)
                    if (!string.IsNullOrWhiteSpace(dbshikakariLocationCode))    //仕掛ロケーションが設定されている場合
                    {
                    PreProcessHikiate(ref dbheader, ref dblines, dbshikakariLocationCode);
                    }

                    //引当解除が実行されたので、明細の引当済数は全て0にする
                    foreach (var l in lines)
                    {
                        l.ProvisionQuantity = 0m;
                    }
                }
                else
                {
                    PreProcessHikiate(ref header, ref lines, shikakariLocationCode);
                }

                //仕掛前処理
                //PreProcessHikiate(ref header, ref lines, shikakariLocationCode);

                decimal requireQuantity = 0;

                for (int i = 0; i < lines.Count(); i++)
                {
                    ServiceSalesLine l = lines[i];

                    List<PartsStock> stockList = new List<PartsStock>();

                    //Add 2017/02/08 arc yano #3620
                    //サービス伝票の引当履歴を元に在庫情報を取得する
                    stockList = GetStockListByTransfer(header, l.PartsNumber);

                    //サービス伝票の引当履歴より在庫情報を取得できない場合は、倉庫全体から在庫情報を取得する ※削除された在庫情報を含む
                    if (stockList.Count() == 0)
                    {
                        //在庫リストの取得
                        stockList = new PartsStockDao(db).GetListByWarehouse(departmentWarehouse != null ? departmentWarehouse.WarehouseCode : "", l.PartsNumber, true);
                    }

                    requireQuantity = l.ProvisionQuantity * -1 ?? 0;

                    //引当済数 > 0の場合
                    if (requireQuantity < 0)
                    {
                        ReleaseReserve(header, ref l, ref stockList, ref requireQuantity, shikakariLocationCode);
                    }
                }

                List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber);


                //既存の入金予定を削除
                List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
                foreach (var p in planList)
                {
                    p.DelFlag = "1";
                }

                //Mod 2016/05/13 arc yano #3528
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
                        plan.CreateEmployeeCode = sessionEmployeeCode;
                        plan.CreateDate = DateTime.Now;
                        plan.LastUpdateEmployeeCode = sessionEmployeeCode;
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
                
                //Del 2016/05/13 arc yano #3528
            }

            //ステータスをキャンセルに更新
            header.ServiceOrderStatus = "007";
            header.KeepsCarFlag = false;                  //ADD 2015/4/26 #3180 ookubo
        }

        #endregion

        #region 赤伝作成
        /// <summary>
        /// 赤伝作成処理
        /// </summary>
        /// <param name="header">サービス伝票</param>
        /// <history>
        /// 2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
        /// 2018/05/14 arc yano #3880 売上原価計算及び棚卸評価法の変更 移動平均単価の計算
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 削除済データも取得する
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 在庫リストの取得方法の変更(キー項目を部門→倉庫に変更)
        /// 2016/06/13 arc yano #3571 
        ///     ①移動伝票の作成処理を追加
        ///     ②赤伝の引当済数、発注数をマイナスにする
        /// 2016/05/23 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法
        /// 2016/02/22 arc yano #3434 サービス伝票  消耗品(SP)の対応
        /// 2016/02/17 arc yano #3435 サービス伝票　原価０の部品の対応
        /// 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更
        /// </history>
        public ServiceSalesHeader CreateAkaden(ServiceSalesHeader header)
        {
            ServiceSalesHeader history = new ServiceSalesHeader();
            history.SlipNumber = header.SlipNumber + "-1";
            history.RevisionNumber = 1;
            history.CarSlipNumber = header.CarSlipNumber;
            history.CarSalesOrderDate = header.CarSalesOrderDate;
            history.QuoteDate = header.QuoteDate;
            history.QuoteExpireDate = header.QuoteExpireDate;
            history.SalesOrderDate = header.SalesOrderDate;
            history.ServiceOrderStatus = "006"; // ステータス＝納車済み
            history.ArrivalPlanDate = header.ArrivalPlanDate;
            history.ApprovalFlag = header.ApprovalFlag;
            history.CampaignCode1 = header.CampaignCode1;
            history.CampaignCode2 = header.CampaignCode2;
            history.WorkingStartDate = header.WorkingStartDate;
            history.WorkingEndDate = header.WorkingEndDate;
            history.SalesDate = DateTime.Today; // 納車日＝システム日付
            history.CustomerCode = header.CustomerCode;
            history.DepartmentCode = header.DepartmentCode;
            history.CarEmployeeCode = header.CarEmployeeCode;
            history.FrontEmployeeCode = header.FrontEmployeeCode;
            history.ReceiptionEmployeeCode = header.ReceiptionEmployeeCode;
            history.CarGradeCode = header.CarGradeCode;
            history.CarBrandName = header.CarBrandName;
            history.CarName = header.CarName;
            history.CarGradeName = header.CarGradeName;
            history.EngineType = header.EngineType;
            history.ManufacturingYear = header.ManufacturingYear;
            history.Vin = header.Vin;
            history.ModelName = header.ModelName;
            history.Mileage = header.Mileage;
            history.MileageUnit = header.MileageUnit;
            history.SalesPlanDate = header.SalesPlanDate;
            history.FirstRegistration = header.FirstRegistration;
            history.NextInspectionDate = header.NextInspectionDate;
            history.MorterViecleOfficialCode = header.MorterViecleOfficialCode;
            history.RegistrationNumberType = header.RegistrationNumberType;
            history.RegistrationNumberKana = header.RegistrationNumberKana;
            history.RegistrationNumberPlate = header.RegistrationNumberPlate;
            history.MakerShipmentDate = header.MakerShipmentDate;
            history.RegistrationPlanDate = header.RegistrationPlanDate;
            history.RequestContent = header.RequestContent;
            history.CarTax = header.CarTax * (-1);
            history.CarLiabilityInsurance = header.CarLiabilityInsurance * (-1);
            history.CarWeightTax = header.CarWeightTax * (-1);
            history.FiscalStampCost = header.FiscalStampCost * (-1);
            history.InspectionRegistCost = header.InspectionRegistCost * (-1);
            history.ParkingSpaceCost = header.ParkingSpaceCost * (-1);
            history.TradeInCost = header.TradeInCost * (-1);
            history.ReplacementFee = header.ReplacementFee * (-1);
            history.InspectionRegistFee = header.InspectionRegistFee * (-1);
            history.ParkingSpaceFee = header.ParkingSpaceFee * (-1);
            history.TradeInFee = header.TradeInFee * (-1);
            history.PreparationFee = header.PreparationFee * (-1);
            history.RecycleControlFee = header.RecycleControlFee * (-1);
            history.RecycleControlFeeTradeIn = header.RecycleControlFeeTradeIn * (-1);
            history.RequestNumberFee = header.RequestNumberFee * (-1);
            history.CarTaxUnexpiredAmount = header.CarTaxUnexpiredAmount * (-1);
            history.CarLiabilityInsuranceUnexpiredAmount = header.CarLiabilityInsuranceUnexpiredAmount * (-1);
            history.LaborRate = header.LaborRate;
            history.Memo = header.Memo;
            history.EngineerTotalAmount = header.EngineerTotalAmount * (-1);
            history.PartsTotalAmount = header.PartsTotalAmount * (-1);
            history.SubTotalAmount = header.SubTotalAmount * (-1);
            history.TotalTaxAmount = header.TotalTaxAmount * (-1);
            history.ServiceTotalAmount = header.ServiceTotalAmount * (-1);
            history.CostTotalAmount = header.CostTotalAmount * (-1);
            history.GrandTotalAmount = header.GrandTotalAmount * (-1);
            history.PaymentTotalAmount = header.PaymentTotalAmount * (-1);
            history.CreateEmployeeCode = sessionEmployeeCode;
            history.CreateDate = DateTime.Now;
            history.LastUpdateEmployeeCode = sessionEmployeeCode;
            history.LastUpdateDate = DateTime.Now;
            history.DelFlag = "0";
            history.SalesCarNumber = header.SalesCarNumber;
            history.InspectionExpireDate = header.InspectionExpireDate;
            history.NumberPlateCost = header.NumberPlateCost * (-1);
            history.TaxFreeFieldName = header.TaxFreeFieldName;
            history.TaxFreeFieldValue = header.TaxFreeFieldValue * (-1);
            history.UsVin = header.UsVin;
            //MOD 赤伝消費税率不具合対応 2014/11/12 ookubo
            history.ConsumptionTaxId = header.ConsumptionTaxId;
            history.Rate = header.Rate;

            //Add 2020/02/17 yano #4025 ---------------------------------------------------------
            history.OptionalInsurance = header.OptionalInsurance * (-1);
            history.CarTaxMemo = header.CarTaxMemo;
            history.CarLiabilityInsuranceMemo = header.CarLiabilityInsuranceMemo;
            history.CarWeightTaxMemo = header.CarWeightTaxMemo;
            history.NumberPlateCostMemo = header.NumberPlateCostMemo;
            history.FiscalStampCostMemo = header.FiscalStampCostMemo;
            history.OptionalInsuranceMemo = header.OptionalInsuranceMemo;
            history.SubscriptionFee = header.SubscriptionFee * (-1);
            history.SubscriptionFeeMemo = header.SubscriptionFeeMemo;
            history.TaxableCostTotalAmount = header.TaxableCostTotalAmount * (-1);
            history.TaxableFreeFieldValue = header.TaxableFreeFieldValue * (-1);
            history.TaxableFreeFieldName = header.TaxableFreeFieldName;
            //Add 2023/05/01 openwave #xxxx
            history.CustomerClaimCode = header.CustomerClaimCode;
            //-----------------------------------------------------------------------------------


            db.ServiceSalesHeader.InsertOnSubmit(history);

            foreach (var item in header.ServiceSalesLine)
            {
                ServiceSalesLine history_line = new ServiceSalesLine();
                history_line.SlipNumber = history.SlipNumber;
                history_line.RevisionNumber = history.RevisionNumber;
                history_line.LineNumber = item.LineNumber;
                history_line.ServiceType = item.ServiceType;
                history_line.SetMenuCode = item.SetMenuCode;
                history_line.ServiceWorkCode = item.ServiceWorkCode;
                history_line.ServiceMenuCode = item.ServiceMenuCode;
                history_line.PartsNumber = item.PartsNumber;
                history_line.LineContents = item.LineContents;
                history_line.RequestComment = item.RequestComment;
                history_line.WorkType = item.WorkType;
                history_line.LaborRate = item.LaborRate * (-1);
                history_line.ManPower = item.ManPower;
                history_line.TechnicalFeeAmount = item.TechnicalFeeAmount * (-1);
                history_line.Quantity = item.Quantity * (-1);
                history_line.Price = item.Price;
                history_line.Amount = item.Amount * (-1);
                history_line.Cost = item.Cost * (-1);
                history_line.EmployeeCode = item.EmployeeCode;
                history_line.SupplierCode = item.SupplierCode;
                history_line.CustomerClaimCode = item.CustomerClaimCode;
                history_line.StockStatus = item.StockStatus;
                history_line.CreateEmployeeCode = sessionEmployeeCode;
                history_line.CreateDate = DateTime.Now;
                history_line.LastUpdateEmployeeCode = sessionEmployeeCode;
                history_line.LastUpdateDate = DateTime.Now;
                history_line.DelFlag = "0";
                history_line.Classification1 = item.Classification1;
                history_line.TaxAmount = item.TaxAmount * (-1);
                history_line.UnitCost = item.UnitCost * (-1);
                history_line.LineType = item.LineType;
                //MOD 赤伝消費税率不具合対応 2014/11/12 ookubo
                history_line.ConsumptionTaxId = history.ConsumptionTaxId;
                history_line.Rate = history.Rate;
                history_line.ProvisionQuantity = item.ProvisionQuantity * (-1);        //引当済数 Mod 2016/06/13 arc yano #3571 Mod 2015/10/28 arc yano #3289
                history_line.OrderQuantity = item.OrderQuantity * (-1);                //発注数   Mod 2016/06/13 arc yano #3571 Mod 2015/10/28 arc yano #3289

                db.ServiceSalesLine.InsertOnSubmit(history_line);
            }

            foreach (var item in header.ServiceSalesPayment)
            {
                ServiceSalesPayment history_pay = new ServiceSalesPayment();
                history_pay.SlipNumber = history.SlipNumber;
                history_pay.RevisionNumber = history.RevisionNumber;
                history_pay.LineNumber = item.LineNumber;
                history_pay.CustomerClaimCode = item.CustomerClaimCode;
                history_pay.PaymentPlanDate = item.PaymentPlanDate;
                history_pay.Amount = item.Amount * (-1);
                history_pay.CreateEmployeeCode = sessionEmployeeCode;
                history_pay.CreateDate = DateTime.Now;
                history_pay.LastUpdateEmployeeCode = sessionEmployeeCode;
                history_pay.LastUpdateDate = DateTime.Now;
                history_pay.DelFlag = "0";
                history_pay.Memo = item.Memo;
                history_pay.DepositFlag = item.DepositFlag != null && item.DepositFlag.Contains("true") ? "1" : "0";
                db.ServiceSalesPayment.InsertOnSubmit(history_pay);
            }

            List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "001");
            foreach (var item in planList)
            {
                ReceiptPlan plan = new ReceiptPlan();
                plan.ReceiptPlanId = Guid.NewGuid();
                plan.DepartmentCode = item.DepartmentCode;
                plan.OccurredDepartmentCode = item.OccurredDepartmentCode;
                plan.CustomerClaimCode = item.CustomerClaimCode;
                plan.SlipNumber = history.SlipNumber;
                plan.ReceiptType = item.ReceiptType;
                plan.ReceiptPlanDate = item.ReceiptPlanDate;
                plan.AccountCode = item.AccountCode;
                plan.Amount = item.Amount * (-1);
                plan.ReceivableBalance = item.Amount * (-1);
                //Mod 2016/05/23 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法
                plan.CompleteFlag = "1";//入金消込の対象外にするため
                plan.CreateEmployeeCode = sessionEmployeeCode;
                plan.CreateDate = DateTime.Now;
                plan.LastUpdateEmployeeCode = sessionEmployeeCode;
                plan.LastUpdateDate = DateTime.Now;
                plan.DelFlag = item.DelFlag;
                plan.Summary = "伝票番号" + header.SlipNumber + "の赤伝処理分";
                plan.JournalDate = item.JournalDate;
                plan.DepositFlag = item.DepositFlag;
                plan.PaymentKindCode = item.PaymentKindCode;
                plan.CommissionRate = item.CommissionRate;
                plan.CommissionAmount = item.CommissionAmount * (-1);
                db.ReceiptPlan.InsertOnSubmit(plan);
            }

            //Add 2016/05/23 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法
            //元伝票の入金予定は入金済み（CompleteFlag = "1"）にする　これ以上元伝票に対して入金できないようにするため
            List<ReceiptPlan> headerPlanList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "001");
            foreach (var item in headerPlanList)
            {
                item.CompleteFlag = "1";
                item.LastUpdateEmployeeCode = sessionEmployeeCode;
                item.LastUpdateDate = DateTime.Now;
            }

            //Add 2016/05/23 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法
            //入金種別が「カード会社からの入金」になっている入金予定もマイナス分の入金予定を作成する。
            List<ReceiptPlan> CreditPlanList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "011");
            foreach (var item in CreditPlanList)
            {
                ReceiptPlan Creditplan = new ReceiptPlan();
                Creditplan.ReceiptPlanId = Guid.NewGuid();
                Creditplan.DepartmentCode = item.DepartmentCode;
                Creditplan.OccurredDepartmentCode = item.OccurredDepartmentCode;
                Creditplan.CustomerClaimCode = item.CustomerClaimCode;
                Creditplan.SlipNumber = history.SlipNumber;
                Creditplan.ReceiptType = item.ReceiptType;
                Creditplan.ReceiptPlanDate = item.ReceiptPlanDate;
                Creditplan.AccountCode = item.AccountCode;
                Creditplan.Amount = item.Amount * (-1);
                Creditplan.ReceivableBalance = item.Amount * (-1);
                Creditplan.CompleteFlag = "1";//入金消込の対象外にするため
                Creditplan.CreateEmployeeCode = sessionEmployeeCode;
                Creditplan.CreateDate = DateTime.Now;
                Creditplan.LastUpdateEmployeeCode = sessionEmployeeCode;
                Creditplan.LastUpdateDate = DateTime.Now;
                Creditplan.DelFlag = item.DelFlag;
                Creditplan.Summary = "伝票番号" + header.SlipNumber + "の赤伝処理分";
                Creditplan.JournalDate = item.JournalDate;
                Creditplan.DepositFlag = item.DepositFlag;
                Creditplan.PaymentKindCode = item.PaymentKindCode;
                Creditplan.CommissionRate = item.CommissionRate;
                Creditplan.CommissionAmount = item.CommissionAmount * (-1);
                db.ReceiptPlan.InsertOnSubmit(Creditplan);
            }
            //元伝票の入金予定は入金済み（CompleteFlag = "1"）にする　これ以上元伝票に対して入金できないようにするため
            List<ReceiptPlan> headerCreditPlanList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "011");
            foreach (var CreditItem in headerCreditPlanList)
            {
                CreditItem.CompleteFlag = "1";
                CreditItem.LastUpdateEmployeeCode = sessionEmployeeCode;
                CreditItem.LastUpdateDate = DateTime.Now;
            }

            db.SubmitChanges();

            //Mod 2015/10/28 arc yano #3289 在庫管理対象の部品のみ行う
            //---------------------------------------------
            //納車済からの戻し
            //--------------------------------------------- 
            //Location shikakariLocation = stockService.GetShikakariLocation(header.DepartmentCode);
            //string shikakariLocationCode = shikakariLocation != null ? shikakariLocation.LocationCode : "";
            
            //Mod 2016/02/22 arc yano #3434 サービス伝票  消耗品(SP)の対応
            //Mod 2016/02/17 arc yano #3435_サービス伝票　原価０の部品の対応
            //在庫管理対象の部品の明細行だけ取得
            //var partsLinesDb = (header.ServiceSalesLine.Where(x => new PartsDao(db).IsInventoryParts(x.PartsNumber))).Select(x => new { PartsNumber = x.PartsNumber, ProvisionQuantity = x.ProvisionQuantity ?? 0 });
            var partsLinesDb = (header.ServiceSalesLine.Where(x => new PartsDao(db).IsInventoryParts(x.PartsNumber) && !x.StockStatus.Equals("998") && !x.StockStatus.Equals("997"))).Select(x => new { PartsNumber = x.PartsNumber, ProvisionQuantity = x.ProvisionQuantity ?? 0 });

            //明細行の引当済数を部品毎にグルーピング
            var partsList = partsLinesDb.GroupBy(x => x.PartsNumber).Select(x => new { PartsNumber = x.Key, sumQuantity = x.Select(y => y.ProvisionQuantity).Sum() });

            //Del 2017/02/08 arc yano #3620 コメントアウトされていた処理を削除

            //Add 2016/08/13 arc yano #3596
            //伝票の部門から倉庫を割り出す
            DepartmentWarehouse departmentWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
            foreach (var parts in partsList)
            {
                //Mod 2016/08/13 arc yano #3596
                //倉庫の在庫棚リストを取得する
                Location location = stockService.GetDefaultLocation(parts.PartsNumber, (departmentWarehouse != null ? departmentWarehouse.WarehouseCode : ""));

                //Mod 2017/02/08 arc yano #3620
                PartsStock stock = new PartsStockDao(db).GetByKey(parts.PartsNumber, location.LocationCode, true);

                //Mod 2016/08/13 arc yano #3596
                //仕掛ロケーションを取得する
                string shikakariLocationCode = "";
 
                Location shikakariLocation = stockService.GetShikakariLocation((departmentWarehouse != null ? departmentWarehouse.WarehouseCode : ""));  
                shikakariLocationCode = shikakariLocation.LocationCode;

                //移動伝票作成①(仕掛→フリーロケーション)
                PartsTransfer(shikakariLocationCode, location.LocationCode, parts.PartsNumber, parts.sumQuantity, header.SlipNumber, TTYPE_CANCELWORK, updatePartsStock: false);    //Add 2016/06/13 arc yano #3571

                //移動伝票作成②(フリーロケーション(引当済)→フリーロケーション(引当解除))
                PartsTransfer(location.LocationCode, location.LocationCode, parts.PartsNumber, parts.sumQuantity, header.SlipNumber, TTYPE_CANCELPROVISION, updatePartsStock: false);   //Add 2016/06/13 arc yano #3571
                
                if (stock != null)
                {
                    //削除データの場合は初期化
                    stock = new PartsStockDao(db).InitPartsStock(stock);        //Add 2017/02/08 arc yano #3620

                    
                    stock.Quantity += parts.sumQuantity;        //Mod 2015/10/28 arc yano #3289
                }
                else
                {
                    stock = new PartsStock();
                    stock.PartsNumber = parts.PartsNumber;
                    stock.Quantity = parts.sumQuantity;
                    stock.LocationCode = location.LocationCode;
                    stock.CreateDate = DateTime.Now;
                    stock.CreateEmployeeCode = sessionEmployeeCode;
                    stock.LastUpdateDate = DateTime.Now;
                    stock.LastUpdateEmployeeCode = sessionEmployeeCode;
                    stock.DelFlag = "0";
                    db.PartsStock.InsertOnSubmit(stock);
                }

                //Add 2018/05/14 arc yano #3880
                //------------------------
                //売上原価の取得
                //------------------------
                //納車済のサービス伝票で一番古いものを取得
                List<ServiceSalesHeader> shList = new ServiceSalesOrderDao(db).GetListByLessThanRevision(header.SlipNumber, header.RevisionNumber).Where(x => x.ServiceOrderStatus.Equals("006")).OrderByDescending(x => x.RevisionNumber).ToList();

                decimal? slCost = 0m;

                foreach (var sh in shList)
                {
                    ServiceSalesLine sl = sh.ServiceSalesLine.Where(x => !string.IsNullOrWhiteSpace(x.PartsNumber) && x.PartsNumber.Equals(parts.PartsNumber)).FirstOrDefault();

                    if (sl != null)
                    {
                        slCost = sl.UnitCost;
                        break;
                    }
                }

                //移動平均単価の更新
                new PartsMovingAverageCostDao(db).UpdateAverageCost(parts.PartsNumber, "001", parts.sumQuantity, slCost, ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode);   
            }

            return history;

        }
        #endregion

        #region 赤処理時の返金用入金予定作成
        /// <summary>
        /// 赤処理時の返金用入金予定作成
        /// </summary>
        /// <param name="SlipNumber">サービス伝票番号</param>
        /// <history>
        /// Add 2016/05/23 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法
        /// </history>
        public void CreateBackAmountAkaden(string SlipNumber)
        {
            //請求先別に元伝票と赤伝票の差分を取り、返金分の入金予定を作成する
            int ret = db.CreateBackAmountAkaden(SlipNumber, sessionEmployeeCode);
        }
        #endregion

        #region 黒伝作成
        /// <summary>
        /// 黒伝を作成する
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="lines">サービス伝票明細</param>
        /// <returns>サービス伝票ヘッダ</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 在庫リストの取得方法の変更(キー項目を倉庫から部門に変更)
        /// 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更 黒伝作成時に引当・仕掛処理を行う。
        /// </history>
        public ServiceSalesHeader CreateKuroden(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines)
        {
            // 値引を税抜に変換
            SetDiscountAmountWithoutTax(lines);

            header.SlipNumber = header.SlipNumber + "-2";
            header.RevisionNumber = 0;
            CreateServiceSalesOrder(header, lines);
            header.ServiceOrderStatus = "005";
            header.SalesDate = null;
            header.CreateDate = sessionDate;
            header.CreateEmployeeCode = sessionEmployeeCode;

            //Add 2015/10/28 arc yano #3289
            //----------------------------------
            //引当・仕掛処理
            //----------------------------------
            //明細の引当済数の初期化
            foreach (var l in lines)
            {
                l.ProvisionQuantity = 0;            //初期化(赤伝処理により、引当が解除されたため)
            }

            //部門から倉庫を特定する
            DepartmentWarehouse departmentWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
            
            //倉庫の仕掛ロケーションを取得する
            Location shikakariLocation = stockService.GetShikakariLocation(departmentWarehouse != null ? departmentWarehouse.WarehouseCode : "");
            //Location shikakariLocation = stockService.GetShikakariLocation(header.DepartmentCode);
            string shikakariLocationCode = shikakariLocation != null ? shikakariLocation.LocationCode : "";
            List<PartsPurchaseOrder> orderList = Hikiate(ref header, ref lines, shikakariLocationCode);
            header.ServiceSalesLine = lines;

            db.ServiceSalesHeader.InsertOnSubmit(header);

            //Add 2016/05/25 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法 入金予定を更新する前に実績を作成された黒伝票に振り替える
            TransferJournal(header.SlipNumber);

            // 入金予定作成(現金のみ)
            CreateReceiptPlan(header, lines, CreateServiceSalesPayment(header, lines));

            //　入金予定作成（カード会社からの入金予定）
            CreateKuroCreditPlan(header.SlipNumber);

            return header;
        }
        #endregion

        #region 黒伝票の入金予定作成前に入金実績を黒伝票に振り返る
        /// <summary>
        /// 黒伝票の入金予定作成前に入金実績を黒伝票に振り返る
        /// </summary>
        /// <param name="SlipNumber">黒伝票番号</param>
        /// <history>
        /// Add 2016/05/25 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法
        /// </history>
        private void TransferJournal(string SlipNumber)
        {
            //元伝票の伝票番号で検索するためreplaceする
            string OriginalSlipNumber = SlipNumber.Replace("-2", "");

            //元伝票の入金実績を全て黒伝票に振り替える
            List<Journal> OriginalJournalList = new JournalDao(db).GetListBySlipNumber(OriginalSlipNumber);

            foreach (var OriginJournal in OriginalJournalList)
            {
                OriginJournal.SlipNumber = SlipNumber; //黒伝票番号
                OriginJournal.LastUpdateEmployeeCode = sessionEmployeeCode;
                OriginJournal.LastUpdateDate = DateTime.Now;
            }

            db.SubmitChanges();

            //元伝票の入金実績が全て黒に振り替わったため、もとの入金予定の残高を全て入金前に戻す
            List<ReceiptPlan> OriginalPlanList = new ReceiptPlanDao(db).GetBySlipNumber(OriginalSlipNumber);
           
            foreach (var OriginalPlan in OriginalPlanList)
            {
                OriginalPlan.ReceivableBalance = OriginalPlan.Amount;
                OriginalPlan.CompleteFlag = "1"; //元伝票に対して入金させないため完了フラグを立てる
                OriginalPlan.LastUpdateEmployeeCode = sessionEmployeeCode;
                OriginalPlan.LastUpdateDate = DateTime.Now;
            }

        }

        #endregion

        #region カード会社からの入金予定を黒伝票分で作成する
        /// <summary>
        /// カード会社からの入金予定を黒伝票分で作成する
        /// </summary>
        /// <param name="SlipNumber">黒伝票番号</param>
        /// <history>
        /// Add 2016/05/25 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法
        /// </history>
        private void CreateKuroCreditPlan(string SlipNumber)
        {
            //元伝票の伝票番号で検索するためreplaceする
            string OriginalSlipNumber = SlipNumber.Replace("-2", "");

            //カード会社からの入金予定取得
            List<ReceiptPlan> CreditPlanList = new ReceiptPlanDao(db).GetCashBySlipNumber(OriginalSlipNumber, "011");

            foreach (var CreditPlan in CreditPlanList)
            {
                decimal? ReceivableBalance = CreditPlan.ReceivableBalance;
                //カード会社からの入金予定に対して入金実績があった場合は残高を更新する
                Journal JournalData = new JournalDao(db).GetByPlanIDAccountType(CreditPlan.ReceiptPlanId.ToString().ToUpper(), "011");
                if (JournalData != null)
                {
                    ReceivableBalance -= JournalData.Amount;
                }

                ReceiptPlan KuroCreditPlan = new ReceiptPlan();
                KuroCreditPlan.ReceiptPlanId = Guid.NewGuid();
                KuroCreditPlan.DepartmentCode = CreditPlan.DepartmentCode;
                KuroCreditPlan.OccurredDepartmentCode = CreditPlan.OccurredDepartmentCode;
                KuroCreditPlan.CustomerClaimCode = CreditPlan.CustomerClaimCode;
                KuroCreditPlan.SlipNumber = SlipNumber;
                KuroCreditPlan.ReceiptType = CreditPlan.ReceiptType;
                KuroCreditPlan.ReceiptPlanDate = CreditPlan.ReceiptPlanDate;
                KuroCreditPlan.AccountCode = CreditPlan.AccountCode;
                KuroCreditPlan.Amount = CreditPlan.Amount;
                KuroCreditPlan.ReceivableBalance = ReceivableBalance;
                if (ReceivableBalance.Equals(0m))
                {
                    KuroCreditPlan.CompleteFlag = "1";
                }else{
                    KuroCreditPlan.CompleteFlag = "0";
                }
                KuroCreditPlan.CreateEmployeeCode = sessionEmployeeCode;
                KuroCreditPlan.CreateDate = DateTime.Now;
                KuroCreditPlan.LastUpdateEmployeeCode = sessionEmployeeCode;
                KuroCreditPlan.LastUpdateDate = DateTime.Now;
                KuroCreditPlan.DelFlag = "0";
                KuroCreditPlan.Summary = CreditPlan.Summary;
                KuroCreditPlan.JournalDate = CreditPlan.JournalDate;
                KuroCreditPlan.DepositFlag = CreditPlan.DepositFlag;
                KuroCreditPlan.PaymentKindCode = CreditPlan.PaymentKindCode;
                KuroCreditPlan.CommissionRate = CreditPlan.CommissionRate;
                KuroCreditPlan.CommissionAmount = CreditPlan.CommissionAmount;
                KuroCreditPlan.CreditJournalId = CreditPlan.CreditJournalId;
                db.ReceiptPlan.InsertOnSubmit(KuroCreditPlan);

                //カード、カード会社からの入金、の実績があった場合入金予定IDを新しい入金予定IDに更新する
                List<Journal> CardJournalList = new JournalDao(db).GetByReceiptPlanID(CreditPlan.ReceiptPlanId.ToString());
                if (CardJournalList != null)
                {
                    foreach (var CardJournal in CardJournalList)
                    {
                        CardJournal.CreditReceiptPlanId = KuroCreditPlan.ReceiptPlanId.ToString().ToUpper();
                        CardJournal.LastUpdateDate = DateTime.Now;
                        CardJournal.LastUpdateEmployeeCode = sessionEmployeeCode;
                    }
                }

                //カード会社からの入金実績も振り替えるため、元伝票の「カード会社からの入金」に対する実績との紐付けを削除
                CreditPlan.CreditJournalId = "";
            }
            


        }
        #endregion

        #region 計算ロジック
        /// <summary>
        /// 明細金額を計算する
        /// (DBから取得した直後は値引を税込にしてから実行すること）
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        /// <history>
        /// 2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加 未使用のため、コメントアウト
        /// </history>
        //public void CalcLineAmount(ServiceSalesHeader header)
        //{
        //    //ADD 2014/02/20 ookubo
        //    System.Nullable<int> int_Rate = header.Rate;

        //    decimal engineerTotalAmount = 0m;
        //    decimal partsTotalAmount = 0m;
        //    decimal taxTotalAmount = 0m;

        //    foreach (var item in header.ServiceSalesLine)
        //    {
        //        switch (item.ServiceType)
        //        {
        //            //サービスメニュー
        //            case "002":
        //                //技術料
        //                item.TechnicalFeeAmount = Math.Truncate((item.ManPower ?? 0m) * (item.LaborRate ?? 0m));
        //                //課税
        //                if (string.IsNullOrEmpty(item.Classification1) || (item.Classification1 != null && !item.Classification1.Equals("002")))
        //                {
        //                    //値引
        //                    if (IsDiscountRecord(item.ServiceMenuCode))
        //                    {
        //                        //税込額から消費税額を計算
        //                        //MOD 2014/02/20 ookubo
        //                        item.TaxAmount = GetTaxAmount(item.TechnicalFeeAmount, int_Rate);
        //                        //消費税額から税抜額を計算
        //                        decimal technicalFeeAmount = (item.TechnicalFeeAmount ?? 0m) - (item.TaxAmount ?? 0m);
        //                        //技術料合計から技術料をマイナス
        //                        engineerTotalAmount -= technicalFeeAmount;
        //                        //消費税合計から消費税額をマイナス
        //                        taxTotalAmount -= item.TaxAmount ?? 0m;
        //                    }
        //                    else
        //                    {
        //                        //税抜額から消費税額を計算
        //                        //MOD 2014/02/20 ookubo
        //                        item.TaxAmount = CommonUtils.CalculateConsumptionTax(item.TechnicalFeeAmount, int_Rate);
        //                        //技術料合計に税抜額をプラス
        //                        engineerTotalAmount += item.TechnicalFeeAmount ?? 0m;
        //                        //消費税合計に消費税額をプラス
        //                        taxTotalAmount += item.TaxAmount ?? 0m;
        //                    }
        //                }
        //                else
        //                {
        //                    //値引
        //                    if (IsDiscountRecord(item.ServiceMenuCode))
        //                    {
        //                        //消費税額は0
        //                        item.TaxAmount = 0m;
        //                        //技術料合計から技術料をマイナス
        //                        engineerTotalAmount -= item.TechnicalFeeAmount ?? 0m;

        //                    }
        //                    else
        //                    {
        //                        //消費税額は0
        //                        item.TaxAmount = 0m;
        //                        //技術料合計に技術料をプラス
        //                        engineerTotalAmount += item.TechnicalFeeAmount ?? 0m;

        //                    }
        //                }
        //                break;
        //            case "003":
        //                //課税対象
        //                if (string.IsNullOrEmpty(item.Classification1) || (item.Classification1 != null && !item.Classification1.Equals("002")))
        //                {
        //                    //値引
        //                    if (IsDiscountRecord(item.PartsNumber))
        //                    {
        //                        //税込金額から消費税額を計算
        //                        //MOD 2014/02/20 ookubo
        //                        item.TaxAmount = GetTaxAmount(item.Amount, int_Rate);
        //                        //消費税額から税抜額を計算
        //                        decimal amount = (item.Amount ?? 0m) - (item.TaxAmount ?? 0m);
        //                        //部品合計から税抜値引額をマイナス
        //                        partsTotalAmount -= amount;
        //                        //消費税合計から消費税額をマイナス
        //                        taxTotalAmount -= item.TaxAmount ?? 0m;
        //                    }
        //                    else
        //                    {
        //                        //税抜金額から消費税額を計算
        //                        //MOD 2014/02/20 ookubo
        //                        item.TaxAmount = CommonUtils.CalculateConsumptionTax(item.Amount, int_Rate);
        //                        //部品合計に税抜額をプラス
        //                        partsTotalAmount += item.Amount ?? 0m;
        //                        //消費税合計に消費税額をプラス
        //                        taxTotalAmount += item.TaxAmount ?? 0m;
        //                    }
        //                    //非課税
        //                }
        //                else
        //                {
        //                    //値引
        //                    if (IsDiscountRecord(item.PartsNumber))
        //                    {
        //                        //部品合計から税抜値引額をマイナス
        //                        item.TaxAmount = 0m;
        //                        partsTotalAmount -= item.Amount ?? 0m;

        //                    }
        //                    else
        //                    {
        //                        //部品合計に税抜額をプラス
        //                        partsTotalAmount += item.Amount ?? 0m;
        //                        //消費税額は0
        //                        item.TaxAmount = 0m;
        //                    }
        //                }

        //                break;
        //        }
        //    }
        //    //諸費用合計
        //    header.CostTotalAmount = (header.CarWeightTax ?? 0m) + (header.CarLiabilityInsurance ?? 0m) + (header.FiscalStampCost ?? 0m)
        //        + (header.CarTax ?? 0m) + (header.NumberPlateCost ?? 0m) + (header.TaxFreeFieldValue ?? 0m);
        //    //部品合計
        //    header.PartsTotalAmount = partsTotalAmount;
        //    //技術料合計
        //    header.EngineerTotalAmount = engineerTotalAmount;
        //    //整備料小計
        //    header.SubTotalAmount = partsTotalAmount + engineerTotalAmount;
        //    //消費税合計
        //    header.TotalTaxAmount = taxTotalAmount;
        //    //整備料合計
        //    header.ServiceTotalAmount = partsTotalAmount + engineerTotalAmount + taxTotalAmount;
        //    //販売合計
        //    header.GrandTotalAmount = header.ServiceTotalAmount + header.CostTotalAmount ?? 0m;

        //    //支払合計
        //    decimal paymentTotalAmount = new decimal(0);
        //    foreach (var p in header.ServiceSalesPayment)
        //    {
        //        paymentTotalAmount += p.Amount ?? 0;
        //    }
        //    header.PaymentTotalAmount = paymentTotalAmount;

        //}

        /// <summary>
        /// 値引を税込額に変換する
        /// （DBから取得したデータを表示用に変換する）
        /// </summary>
        /// <param name="header">サービス伝票</param>
        public void SetDiscountAmountWithTax(EntitySet<ServiceSalesLine> lines)
        {
            foreach (var line in lines)
            {
                //課税対象のみ
                if (!string.IsNullOrEmpty(line.Classification1) && !line.Classification1.Equals("002"))
                {
                    //部品値引
                    if (IsDiscountRecord(line.PartsNumber))
                    {
                        line.Amount = (line.Amount ?? 0m) + (line.TaxAmount ?? 0m);
                    }
                    //工賃値引
                    if (IsDiscountRecord(line.ServiceMenuCode))
                    {
                        line.TechnicalFeeAmount = (line.TechnicalFeeAmount ?? 0m) + (line.TaxAmount ?? 0m);
                    }
                }
            }
        }
        /// <summary>
        /// 値引を税抜額に変換する
        /// （DB保存用に変換する）
        /// </summary>
        /// <param name="header">サービス伝票</param>
        public void SetDiscountAmountWithoutTax(EntitySet<ServiceSalesLine> lines)
        {
            foreach (var line in lines)
            {
                //課税対象のみ
                if (string.IsNullOrEmpty(line.Classification1) || (line.Classification1 != null && !line.Classification1.Equals("002")))
                {
                    //部品値引
                    if (IsDiscountRecord(line.PartsNumber))
                    {
                        line.Amount = line.Amount - line.TaxAmount;
                    }
                    //工賃値引
                    if (IsDiscountRecord(line.ServiceMenuCode))
                    {
                        line.TechnicalFeeAmount = line.TechnicalFeeAmount - line.TaxAmount;
                    }
                }
            }
        }

        #endregion

        #region 伝票ロック
        /// <summary>
        /// 伝票をロックする
        /// 条件１：既存の伝票であること
        /// 条件２：最新のリビジョンであること（DelFlag='0'）
        /// 条件３：既に自分がロックしていないこと
        /// </summary>
        /// <param name="header"></param>
        public void ProcessLock(ServiceSalesHeader header)
        {
            ServiceSalesHeader target = new ServiceSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            if (target != null && target.DelFlag.Equals("0"))
            {
                //&& 
                //(target.ProcessSessionControl==null || (target.ProcessSessionControl!=null && !target.ProcessSessionControl.EmployeeCode.Equals(((Employee)Session["Employee"]).EmployeeCode)))
                //){
                target.ProcessSessionControl = new ProcessSessionControl();
                target.ProcessSessionControl.ProcessSessionId = Guid.NewGuid();
                target.ProcessSessionControl.TableName = "ServiceSalesHeader";
                target.ProcessSessionControl.EmployeeCode = sessionEmployeeCode;
                target.ProcessSessionControl.CreateDate = sessionDate;

                db.SubmitChanges();
            }
        }

        /// <summary>
        /// 伝票ロック解除
        /// 条件１：ロックしているのが自分であること
        /// 条件２：もしくは、強制解除であること
        /// </summary>
        /// <param name="header"></param>
        public void ProcessUnLock(ServiceSalesHeader header)
        {
            ServiceSalesHeader target = new ServiceSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            if ((target.ProcessSessionControl != null && target.ProcessSessionControl.EmployeeCode.Equals(sessionEmployeeCode)))
            {
                ProcessSessionControl control = new ProcessSessionControlDao(db).GetByKey(target.ProcessSessionId);
                db.ProcessSessionControl.DeleteOnSubmit(control);
                target.ProcessSessionControl = null;

                db.SubmitChanges();
            }
        }
        /// <summary>
        /// 伝票がロックされているか
        /// 条件１：ProcessSessionId!=null
        /// 条件２：ProcessSessionControl!=null
        /// 条件３：自分以外がロックしている
        /// </summary>
        /// <param name="header"></param>
        public string GetProcessLockUser(ServiceSalesHeader header)
        {
            ServiceSalesHeader target = new ServiceSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            if (target != null && target.ProcessSessionId != null &&
                target.ProcessSessionControl != null &&
                !target.ProcessSessionControl.EmployeeCode.Equals(sessionEmployeeCode))
            {

                return target.ProcessSessionControl.Employee.EmployeeName;
            }
            return null;
        }

        /// <summary>
        /// ロックを自分のものにする
        /// </summary>
        /// <param name="header"></param>
        public void ProcessLockUpdate(ServiceSalesHeader header)
        {
            ServiceSalesHeader target = new ServiceSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            if (target.ProcessSessionControl != null)
            {
                target.ProcessSessionControl.EmployeeCode = sessionEmployeeCode;
                db.SubmitChanges();
            }
        }
        #endregion

        #region 入金予定作成
        /// <summary>
        /// 入金予定を作成する
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="lines">サービス伝票明細</param>
        /// <param name="paymentList">支払情報</param>
        /// <history>
        /// 2020/01/06 #4025【サービス伝票】諸費用の入金予定作成時の勘定奉行科目コードの設定値の変更
        /// 2019/09/02 yano 3991 【サービス伝票入力】カードで消込済伝票の再保存を行うと請求残高がリセットされてしまう。残高計算時の実績にカードを含める
        /// 2018/08/22 yano #3930 入金実績リスト　マイナスの入金予定ができている状態で実績削除した場合の残高不正 アクセス識別子を変更(private→public)
        /// 2017/12/28 arc yano #3821 サービス伝票－マイナスの入金予定、入金実績のあるサービス伝票の保存時不具合 
        /// 2016/06/29 arc nakayama #3593_伝票に対して同一請求先が複数あった場合の考慮
        /// 2016/04/05 arc yano #3441 カード入金消込時マイナスの入金予定ができてしまう 入金実績取得時は請求先種別 =　「クレジット」「ローン」は除く
        /// </history>
        public void CreateReceiptPlan(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines, List<ServiceSalesPayment> paymentList)
        {

            Account serviceAccount = new AccountDao(db).GetByUsageType("SR");

            //既存の入金予定を削除
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "001");
            foreach (var d in delList)
            {
                d.DelFlag = "1";
            }

            //**現金の入金予定を(再)作成する**
            //請求先、入金予定日の順で並び替え
            paymentList.Sort(delegate(ServiceSalesPayment x, ServiceSalesPayment y)
            {
                return
                    !x.CustomerClaimCode.Equals(y.CustomerClaimCode) ? x.CustomerClaimCode.CompareTo(y.CustomerClaimCode) : //請求先順
                    !x.PaymentPlanDate.Equals(y.PaymentPlanDate) ? DateTime.Compare(x.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MIN, y.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MAX) : //入金予定日順
                    (0);
            });

            //Add 2016/06/29 arc nakayama #3593
            //さらに金額の少ない順に並び替える
            paymentList = paymentList.OrderBy(a => a.CustomerClaimCode).ThenBy(b => b.Amount).ToList<ServiceSalesPayment>();

            //請求先一覧作成用
            List<string> customerClaimList = new List<string>();

            // 入金実績額
            //Addd 2017/12/28 arc yano #3821
            decimal PlusjournalAmount = 0m;
            decimal MinusjournalAmount = 0m;
            for (int i = 0; i < paymentList.Count; i++)
            {

                //請求先リストに追加
                customerClaimList.Add(paymentList[i].CustomerClaimCode);

                //請求先が変わったら入金済み金額を（再）取得する
                if (i == 0 || (i > 0 && !CommonUtils.DefaultString(paymentList[i - 1].CustomerClaimCode).Equals(CommonUtils.DefaultString(paymentList[i].CustomerClaimCode))))
                {
                    //Mod 2019/09/02 yano 3991
                    PlusjournalAmount = new JournalDao(db).GetPlusMinusTotalByCondition(header.SlipNumber, paymentList[i].CustomerClaimCode, true, true);
                    //PlusjournalAmount = new JournalDao(db).GetPlusMinusTotalByCondition(header.SlipNumber, paymentList[i].CustomerClaimCode, false, true);
                    MinusjournalAmount = new JournalDao(db).GetPlusMinusTotalByCondition(header.SlipNumber, paymentList[i].CustomerClaimCode, false, false);
                }

                // 売掛残
                decimal balanceAmount = 0m;

                if (paymentList[i].Amount >= 0)
                {

                    if (paymentList[i].Amount >= PlusjournalAmount)
                    {
                        // 予定額 >= 実績額
                        balanceAmount = ((paymentList[i].Amount ?? 0m) - PlusjournalAmount);
                        PlusjournalAmount = 0m;
                    }
                    else
                    {
                        // 予定額 < 実績額
                        balanceAmount = 0m;
                        PlusjournalAmount = PlusjournalAmount - (paymentList[i].Amount ?? 0m);

                        // 次の請求先が異なる場合、ここでマイナスの売掛金を作成しておく
                        if (i == paymentList.Count() - 1 || (i < paymentList.Count() - 1 && !CommonUtils.DefaultString(paymentList[i].CustomerClaimCode).Equals(CommonUtils.DefaultString(paymentList[i + 1].CustomerClaimCode))))
                        {
                            balanceAmount = PlusjournalAmount * (-1);
                        }
                    }
                }
                else
                {
                    if (paymentList[i].Amount >= MinusjournalAmount)
                    {
                        // 予定額 >= 実績額
                        balanceAmount = ((paymentList[i].Amount ?? 0m) - MinusjournalAmount);
                        MinusjournalAmount = MinusjournalAmount - (paymentList[i].Amount ?? 0m);

                        //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                        // 次の請求先が異なる場合、ここでマイナスの売掛金を作成しておく
                        if (i == paymentList.Count() - 1 || (i < paymentList.Count() - 1 && !CommonUtils.DefaultString(paymentList[i].CustomerClaimCode).Equals(CommonUtils.DefaultString(paymentList[i + 1].CustomerClaimCode))))
                        {
                            balanceAmount = MinusjournalAmount * (-1);
                        }

                    }
                    else
                    {
                        // 予定額 < 実績額
                        balanceAmount = ((paymentList[i].Amount ?? 0m) - MinusjournalAmount);
                        MinusjournalAmount = 0m;
                    }
                }

                InsertReceiptPlan(header, paymentList[i], balanceAmount, serviceAccount.AccountCode);
            }

            //Del 2017/12/28 arc yano #3821
           
            //Mod 2016/04/05 arc yano #3441
            //入金済みの請求先が今回の伝票からなくなっているものを通知
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, excludeList);
            foreach (Journal a in journalList)
            {
                if (!string.IsNullOrEmpty(a.CustomerClaimCode) && customerClaimList.IndexOf(a.CustomerClaimCode) < 0)
                {
                    TaskUtil task = new TaskUtil(db, sessionEmployee);
                    //task.CarOverReceive(header, a.CustomerClaimCode, a.Amount);

                    // マイナスで入金予定作成
                    ReceiptPlan plan = new ReceiptPlan();
                    plan.Amount = a.Amount * (-1m);
                    plan.ReceivableBalance = a.Amount * (-1m);
                    plan.ReceiptPlanId = Guid.NewGuid();
                    plan.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now));
                    plan.CreateEmployeeCode = sessionEmployeeCode;
                    plan.DelFlag = "0";
                    plan.CompleteFlag = "0";
                    plan.LastUpdateDate = DateTime.Now;
                    plan.LastUpdateEmployeeCode = sessionEmployeeCode;
                    plan.ReceiptType = "001"; // 現金
                    plan.SlipNumber = a.SlipNumber;
                    plan.OccurredDepartmentCode = a.DepartmentCode;
                    plan.AccountCode = a.AccountCode;
                    plan.CustomerClaimCode = a.CustomerClaimCode;
                    plan.DepartmentCode = a.DepartmentCode;
                    plan.ReceiptPlanDate = a.JournalDate;
                    plan.Summary = a.Summary;
                    plan.JournalDate = a.JournalDate;
                    db.ReceiptPlan.InsertOnSubmit(plan);
                }
            }
        }
        #endregion

        #endregion

        #region プライベート関数

        /// <summary>
        /// 支払予定作成
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="lines">サービス伝票明細</param>
        /// <returns>支払予定</returns>
        /// <history>
        /// 2023/09/05 #4162 インボイス対応　インボイス消費税の計算、登録処理の呼び出しの追加
        /// 2020/01/06 #4025_【サービス伝票】諸費用の入金予定作成時の勘定奉行科目コードの設定値の変更
        /// </history>
        private List<ServiceSalesPayment> CreateServiceSalesPayment(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines)
        {

            //Mod 2020/01/06 yano #4025
            //諸費用の支払予定を作成
            List<ServiceSalesPayment> paymentList = new List<ServiceSalesPayment>();

            paymentList = CreateExpenseServiceSalesPayment(header, lines, paymentList);

             //Dell 2023/09/05 #4162
        
            int lineNumber = paymentList.Count;

            //Dell 2023/09/05 #4162
            // 預り金の支払方法作成
          
            //明細レコードがある場合のみ実行する
            if (lines != null && lines.Count > 0)
            {
                string parentCustomerClaimCode = "";
                ServiceWork parentServiceWork = new ServiceWork();

                //請求先コードと主作業コードを全明細に紐付ける
                foreach (var l in lines)
                {
                    if (!string.IsNullOrEmpty(l.ServiceWorkCode))
                    {// && !string.IsNullOrEmpty(l.CustomerClaimCode)) {
                        parentCustomerClaimCode = l.CustomerClaimCode;
                        parentServiceWork = new ServiceWorkDao(db).GetByKey(l.ServiceWorkCode);
                    }
                    l.ParentCustomerClaimCode = parentCustomerClaimCode;
                    l.ServiceWork = parentServiceWork;
                }

                ////値引を税抜に変換
                //SetDiscountAmountWithoutTax(line);

                //明細を請求先ごとに合計金額を集計
                var query = from a in lines
                            group a by new { ParentCustomerClaimCode = a.ParentCustomerClaimCode, Classification2 = a.ServiceWork.Classification2 } into customerClaim
                            select new
                            {
                                customerClaim.Key,
                                TaxAmount = customerClaim.Sum(x => (IsDiscountRecord(x.PartsNumber) || IsDiscountRecord(x.ServiceMenuCode)) ? (-1) * x.TaxAmount : (x.TaxAmount ?? 0m)),
                                Amount = customerClaim.Sum(x => IsDiscountRecord(x.PartsNumber) ? (-1) * x.Amount : (x.Amount ?? 0m)),
                                TechnicalFeeAmount = customerClaim.Sum(y => IsDiscountRecord(y.ServiceMenuCode) ? (-1) * y.TechnicalFeeAmount : (y.TechnicalFeeAmount ?? 0m))
                            };

                //支払方法にセット
                foreach (var p in query)
                {
                    if (!string.IsNullOrEmpty(p.Key.ParentCustomerClaimCode))
                    {
                        lineNumber++;
                        ServiceSalesPayment payment = new ServiceSalesPayment();
                        payment.SlipNumber = header.SlipNumber;
                        payment.RevisionNumber = header.RevisionNumber;
                        payment.LineNumber = lineNumber;
                        payment.CreateDate = sessionDate;
                        payment.CreateEmployeeCode = sessionEmployeeCode;
                        payment.LastUpdateDate = sessionDate;
                        payment.LastUpdateEmployeeCode = sessionEmployeeCode;
                        payment.DelFlag = "0";
                        payment.DepositFlag = "0";
                        payment.CustomerClaimCode = p.Key.ParentCustomerClaimCode;
                        payment.Amount = p.Amount + p.TechnicalFeeAmount + p.TaxAmount;

                        if (p.Key.Classification2 != null && p.Key.Classification2.Equals("006"))
                        {
                            //ワランティの場合、翌月末を設定（システム日付＋2か月－1日）
                            payment.PaymentPlanDate = new DateTime(DateTime.Today.AddMonths(2).Year, DateTime.Today.AddMonths(2).Month, 1).AddDays(-1);
                        }
                        else
                        {
                            //その他は当月末を設定
                            payment.PaymentPlanDate = new DateTime(DateTime.Today.AddMonths(1).Year, DateTime.Today.AddMonths(1).Month, 1).AddDays(-1);
                        }

                        paymentList.Add(payment);
                    }
                }
            }

            if (paymentList.Count() > 0)
            {
                // テーブルに挿入
                db.ServiceSalesPayment.InsertAllOnSubmit(paymentList);

                // ヘッダに支払合計をセット
                header.PaymentTotalAmount = paymentList.Sum(x => x.Amount);

                //インボイス消費税の計算、登録
                InsertInvoiceConsumptionTax(header, lines, paymentList);    //Add 2023/09/05  #4162
            }

            return paymentList;
        }

        /// <summary>
        /// 諸費用の支払予定作成
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="lines">サービス伝票明細</param>
        /// <returns>支払予定</returns>
        /// <history>
        /// 2020/01/06 #4025_【サービス伝票】諸費用の入金予定作成時の勘定奉行科目コードの設定値の変更 新規作成
        /// </history>
        private List<ServiceSalesPayment> CreateExpenseServiceSalesPayment(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines, List<ServiceSalesPayment> paymentList)
        {
            int lineNumber = 0;

            //-------------------------------------------
            //諸費用は対応する勘定科目コード毎に分ける
            //-------------------------------------------
            string accountCode = "";

            List<AccountConv> list = new List<AccountConv>();
            AccountConv rec = null;

            //自動車税種別割
            if ((header.CarTax ?? 0m) > 0m)
            {
                accountCode = new AccountByAmountTypeDao(db).GetByKey("CarTax") != null ? new AccountByAmountTypeDao(db).GetByKey("CarTax").AccountCode : "";

                if (!string.IsNullOrWhiteSpace(accountCode))
                {
                    rec = new AccountConv();

                    rec.AccountCode = accountCode;
                    rec.Amount = (header.CarTax ?? 0m);

                    list.Add(rec);
                }
            }

            //自賠責保険料
            if ((header.CarLiabilityInsurance ?? 0m) > 0m)
            {
                accountCode = new AccountByAmountTypeDao(db).GetByKey("CarLiabilityInsurance") != null ? new AccountByAmountTypeDao(db).GetByKey("CarLiabilityInsurance").AccountCode : "";

                if (!string.IsNullOrWhiteSpace(accountCode))
                {
                    rec = new AccountConv();

                    rec.AccountCode = accountCode;
                    rec.Amount = (header.CarLiabilityInsurance ?? 0m);

                    list.Add(rec);
                }
            }

            //自動車重量税
            if ((header.CarWeightTax ?? 0m) > 0m)
            {
                accountCode = new AccountByAmountTypeDao(db).GetByKey("CarWeightTax") != null ? new AccountByAmountTypeDao(db).GetByKey("CarWeightTax").AccountCode : "";

                if (!string.IsNullOrWhiteSpace(accountCode))
                {
                    rec = new AccountConv();

                    rec.AccountCode = accountCode;
                    rec.Amount = (header.CarWeightTax ?? 0m);

                    list.Add(rec);
                }
            }

            //ナンバー代
            if ((header.NumberPlateCost ?? 0m) > 0m)
            {
                accountCode = new AccountByAmountTypeDao(db).GetByKey("NumberPlateCost") != null ? new AccountByAmountTypeDao(db).GetByKey("NumberPlateCost").AccountCode : "";

                if (!string.IsNullOrWhiteSpace(accountCode))
                {
                    rec = new AccountConv();

                    rec.AccountCode = accountCode;
                    rec.Amount = (header.NumberPlateCost ?? 0m);

                    list.Add(rec);
                }
            }

            //各種印紙代
            if ((header.FiscalStampCost ?? 0m) > 0m)
            {
                accountCode = new AccountByAmountTypeDao(db).GetByKey("FiscalStampCost") != null ? new AccountByAmountTypeDao(db).GetByKey("FiscalStampCost").AccountCode : "";

                if (!string.IsNullOrWhiteSpace(accountCode))
                {
                    rec = new AccountConv();

                    rec.AccountCode = accountCode;
                    rec.Amount = (header.FiscalStampCost ?? 0m);

                    list.Add(rec);
                }
            }

            //任意保険
            if ((header.OptionalInsurance ?? 0m) > 0m)
            {
                accountCode = new AccountByAmountTypeDao(db).GetByKey("OptionalInsurance") != null ? new AccountByAmountTypeDao(db).GetByKey("OptionalInsurance").AccountCode : "";

                if (!string.IsNullOrWhiteSpace(accountCode))
                {
                    rec = new AccountConv();

                    rec.AccountCode = accountCode;
                    rec.Amount = (header.OptionalInsurance ?? 0m);

                    list.Add(rec);
                }
            }

            //その他諸費用
            if ((header.TaxFreeFieldValue ?? 0m) > 0m)
            {
                accountCode = new AccountByAmountTypeDao(db).GetByKey("TaxFreeFieldValue") != null ? new AccountByAmountTypeDao(db).GetByKey("TaxFreeFieldValue").AccountCode : "";

                if (!string.IsNullOrWhiteSpace(accountCode))
                {
                    rec = new AccountConv();

                    rec.AccountCode = accountCode;
                    rec.Amount = (header.TaxFreeFieldValue ?? 0m);

                    list.Add(rec);
                }
            }

            //サービス加入料
            if ((header.SubscriptionFee ?? 0m) > 0m)
            {
                accountCode = new AccountByAmountTypeDao(db).GetByKey("SubscriptionFee") != null ? new AccountByAmountTypeDao(db).GetByKey("SubscriptionFee").AccountCode : "";

                if (!string.IsNullOrWhiteSpace(accountCode))
                {
                    rec = new AccountConv();

                    rec.AccountCode = accountCode;
                    rec.Amount = (header.SubscriptionFee ?? 0m);

                    list.Add(rec);
                }
            }

            //その他（課税）
            if ((header.TaxableFreeFieldValue ?? 0m) > 0m)
            {
                accountCode = new AccountByAmountTypeDao(db).GetByKey("TaxableFreeFieldValue") != null ? new AccountByAmountTypeDao(db).GetByKey("TaxableFreeFieldValue").AccountCode : "";

                if (!string.IsNullOrWhiteSpace(accountCode))
                {
                    rec = new AccountConv();

                    rec.AccountCode = accountCode;
                    rec.Amount = (header.TaxableFreeFieldValue ?? 0m);

                    list.Add(rec);
                }
            }

            if (list.Count > 0)
            {
                //Add 2023/05/01 openwave #xxxx
                string CustomerClaimCode = header.CustomerCode;
                if (!string.IsNullOrEmpty(header.CustomerClaimCode)) {
                    CustomerClaimCode = header.CustomerClaimCode;
                }
                foreach (var line in list)
                {
                    lineNumber++;
                    ServiceSalesPayment deposit = new ServiceSalesPayment();
                    deposit.SlipNumber = header.SlipNumber;
                    deposit.RevisionNumber = header.RevisionNumber;
                    deposit.LineNumber = lineNumber;
                    deposit.Amount = line.Amount;
                    //Mod 2023/05/01 openwave #xxxx
                    //Customer customer = new CustomerDao(db).GetByKey(header.CustomerCode);
                    //if (customer != null && customer.CustomerClaim != null)
                    //{
                    //    deposit.CustomerClaimCode = header.CustomerCode;
                    //}
                    deposit.CustomerClaimCode = CustomerClaimCode;
                    deposit.PaymentPlanDate = DateTime.Today;
                    deposit.DepositFlag = "1";
                    deposit.CreateDate = sessionDate;
                    deposit.CreateEmployeeCode = sessionEmployeeCode;
                    deposit.LastUpdateDate = sessionDate;
                    deposit.LastUpdateEmployeeCode = sessionEmployeeCode;
                    deposit.DelFlag = "0";
                    deposit.AmountType = line.AccountCode;
                    paymentList.Add(deposit);
                }
            }

            return paymentList;
        }

        /// <summary>
        /// 適格請求書(インボイス)用の消費税・差額の登録
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="lines">サービス伝票明細</param>
        /// <param name="paymentList">サービス伝票支払</param>
        /// <returns>支払予定</returns>
        /// <history>
        /// 2023/09/05 #4162 インボイス対応 新規作成
        /// </history>
        private void InsertInvoiceConsumptionTax(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines, List<ServiceSalesPayment> paymentList)
        {
            //登録リスト
            List<InvoiceConsumptionTax> registList = new List<InvoiceConsumptionTax>();

            //-----------------------
            //古いリストの削除
            //-----------------------
            List<InvoiceConsumptionTax> delList = new List<InvoiceConsumptionTax>();

            delList = new InvoiceConsumptionTaxDao(db).GetBySlipNumber(header.SlipNumber);

            foreach (var d in delList)
            {
                d.DelFlag = "1";
            }

            //-----------------------
            //新しいリスト登録
            //-----------------------
            //勘定科目(サービス加入料）
            string accountCodeSF = new AccountByAmountTypeDao(db).GetByKey("SubscriptionFee") != null ? new AccountByAmountTypeDao(db).GetByKey("SubscriptionFee").AccountCode : "";
          
            //勘定科目(その他課税）
            string accountCodeTF = new AccountByAmountTypeDao(db).GetByKey("TaxableFreeFieldValue") != null ? new AccountByAmountTypeDao(db).GetByKey("TaxableFreeFieldValue").AccountCode : "";

            List<Tuple<string, decimal?>> customerClaimList = new List<Tuple<string, decimal?>>();

            //データ取得のクエリ
            var query = paymentList.Where
                        (
                            x => x.DepositFlag.Equals("0") ||
                            (
                               x.DepositFlag.Equals("1") && 
                              (
                                  x.AmountType.Equals(accountCodeSF) || 
                                  x.AmountType.Equals(accountCodeTF)
                              )
                            )   
                        ).GroupBy
                        (
                            x => x.CustomerClaimCode).Select(x => new { CustomerClaimCode = x.Key, Amount = x.Sum(y => y.Amount)}
                        );

          　//請求先毎のインボイス消費税リストを登録する
            foreach(var rec in query)
            {
                decimal amountwithTax = rec.Amount ?? 0m;
                decimal amount = 0m;
                decimal taxmount= 0m;
                string customerclaimtype = "";
                int taxrate = 0;

                customerclaimtype = new CustomerClaimDao(db).GetByKey(rec.CustomerClaimCode) != null ? new CustomerClaimDao(db).GetByKey(rec.CustomerClaimCode).CustomerClaimType : "";

                //請求先≠社内の場合に消費税率を設定
                if(!customerclaimtype.Equals("005"))
                {
                   taxrate = (header.Rate ?? 0);
                }
               
                //税込金額から税抜金額を計算
                amount = CommonUtils.CalcAmountWithoutTax(amountwithTax, taxrate, 0);

                //消費税計算
                taxmount = amountwithTax - amount;

                InvoiceConsumptionTax  el = new InvoiceConsumptionTax();
                
                el.InvoiceConsumptionTaxId = Guid.NewGuid();                     //ユニークID
                el.SlipNumber = header.SlipNumber;                           //伝票番号
                el.CustomerClaimCode = rec.CustomerClaimCode;                //請求先コード
                el.Rate = header.Rate ?? 0;                                     //消費税率
                el.InvoiceConsumptionTaxAmount = taxmount;                   //インボイス消費税
                el.CreateEmployeeCode = sessionEmployeeCode;                 //作成者
                el.CreateDate = DateTime.Now;                                //作成日
                el.LastUpdateEmployeeCode = sessionEmployeeCode;             //最終更新者
                el.LastUpdateDate = DateTime.Now;                            //最終更新日
                el.DelFlag = "0";                                            //削除フラグ

                registList.Add(el);
            }

            //登録処理
            if(registList.Count > 0)
            {
                db.InvoiceConsumptionTax.InsertAllOnSubmit(registList);
            }
        }


        /// <summary>
        /// 入金予定登録
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="lines">サービス伝票支払</param>
        /// <returns>支払予定</returns>
        /// <history>
        /// 2020/01/06 #4025_【サービス伝票】諸費用の入金予定作成時の勘定奉行科目コードの設定値の変更 新規作成
        /// </history>
        private void InsertReceiptPlan(ServiceSalesHeader header, ServiceSalesPayment payment, decimal planAmount, string accountCode)
        {
            ReceiptPlan plan = new ReceiptPlan();
            plan.ReceiptPlanId = Guid.NewGuid();
            plan.CreateDate = DateTime.Now;
            plan.CreateEmployeeCode = sessionEmployeeCode;
            plan.SlipNumber = header.SlipNumber;
            plan.LastUpdateDate = DateTime.Now;
            plan.LastUpdateEmployeeCode = sessionEmployeeCode;
            plan.DelFlag = "0";
            plan.DepartmentCode = header.DepartmentCode;
            plan.ReceiptPlanDate = payment.PaymentPlanDate;
            plan.ReceiptType = "001";
            plan.Amount = payment.Amount;
            plan.CustomerClaimCode = payment.CustomerClaimCode;
            plan.OccurredDepartmentCode = header.DepartmentCode;
            plan.AccountCode = !string.IsNullOrWhiteSpace(payment.AmountType) ? payment.AmountType : accountCode;                        //Mod 2020/01/06 #4025
            plan.ReceivableBalance = planAmount;
            plan.Summary = payment.Memo;
            if (planAmount.Equals(0m))
            {
                plan.CompleteFlag = "1";
            }
            else
            {
                plan.CompleteFlag = "0";
            }
            plan.JournalDate = header.SalesOrderDate ?? DateTime.Today;
            if (payment.DepositFlag != null && payment.DepositFlag.Equals("1"))
            {
                plan.DepositFlag = "1";
            }
            db.ReceiptPlan.InsertOnSubmit(plan);
        }

        /// <summary>
        /// サービス伝票の引当済数の取得
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ情報</param>
        /// <param name="lines">サービス伝票明細情報</param>
        /// <hisotry>
        /// Add 2016/11/11 arc yano #3656 新規作成
        /// </hisotry>
        public void ResetProvisionQuantity(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines)
        {

           //DBから種別が部品のサービス伝票明細行を取得する
            List<ServiceSalesLine> dbLines = new ServiceSalesOrderDao(db).GetLineByPartsNumber(header.SlipNumber, null, null);

            var dbList = dbLines.Where(x => x.ServiceType.Equals("003") && !string.IsNullOrWhiteSpace(x.StockStatus) && !x.StockStatus.Equals("997") && !x.StockStatus.Equals("998"))
                                       .GroupBy(x => new { x.PartsNumber, x.StockStatus })
                                       .Select(x => new { PartsNumber = x.Key.PartsNumber, StockStatus = x.Key.StockStatus, sumProvisionQuantity = x.Sum(y => y.ProvisionQuantity)});
                                       

            //サービス伝票引当済数の設定
            foreach (var dbLine in dbList)
            {
                decimal? provisionQuantity = (dbLine.sumProvisionQuantity ?? 0);

                //画面から明細行を取得する
                var workLines = lines.Where(x => x.PartsNumber.Equals(dbLine.PartsNumber) && x.StockStatus.Equals(dbLine.StockStatus));

                //画面明細行の引当済数を順番に設定する
                foreach (var l in workLines)
                {
                    //画面明細の数量がDBの引当済数以上の場合
                    if (l.Quantity >= provisionQuantity)
                    {
                        l.ProvisionQuantity = provisionQuantity;        //DBの引当済数を全て引当
                        provisionQuantity = 0;

                        //これ以上画面の明細行の引当済数を更新できないため、次の行へ
                        break;
                    }
                    else
                    {
                        l.ProvisionQuantity = l.Quantity;               //画面明細の数量分引当
                        provisionQuantity -= l.Quantity;                //DBの引当済数を減算
                    }
                }
            }
        }

        /// サービス伝票の発注の再設定
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ情報</param>
        /// <param name="lines">サービス伝票明細情報</param>
        /// <history>
        /// 2018/09/18 yano #3925 サービス伝票 明細にマスタ未登録の部品が存在したまま納車できる
        /// 2018/05/30 arc yano #3889 サービス伝票発注部品が引当されない
        /// 2016/05/09 arc nakayama #3513_サービス伝票入力から発注取消を行うと、伝票上の発注数が更新されない 新規作成
        /// <summary>
        /// </history>
        private void ResetOrderQuantity(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines)
        {
            
            //種別が「部品」の行だけ抜き出す
            var PpoList = lines.Where(x => x.ServiceType.Equals("003") && !string.IsNullOrWhiteSpace(x.StockStatus) && !x.StockStatus.Equals("997") && !x.StockStatus.Equals("998") && !x.StockStatus.Equals("999"))
                                       .Select(x => new { x.PartsNumber, x.StockStatus })
                                       .GroupBy(x => new { x.PartsNumber, x.StockStatus });

            foreach (var l in PpoList)
            {
                //伝票番号、部品番号、発注区分で部品発注データを取得する
                List<PartsPurchaseOrder> ppo = new PartsPurchaseOrderDao(db).GetListByKeys(header.SlipNumber, l.Key.PartsNumber, l.Key.StockStatus);

                if (ppo != null && ppo.Count > 0)
                {
                    //発注されていた場合、発注データの発注数(合計)を取得
                    decimal? orderQuantity = ppo.Select(x => x.Quantity).Sum();

                    //受注伝票番号、部品番号、判断からサービス伝票の明細行の更新
                    List<ServiceSalesLine> workLines = lines.Where(x => !string.IsNullOrWhiteSpace(x.PartsNumber) && x.PartsNumber.Equals(l.Key.PartsNumber) && !string.IsNullOrWhiteSpace(x.StockStatus) && x.StockStatus.Equals(l.Key.StockStatus)).ToList();  //Mod 2018/05/30 arc yano #3889

                    //該当するサービス伝票明細の発注数に割り当てる。
                    for (int k = 0; k < workLines.Count; k++)
                    {
                        //明細最終行の場合は発注数を全て設定
                        if (k == (workLines.Count - 1))
                        {
                            workLines[k].OrderQuantity = orderQuantity;
                            orderQuantity = 0;
                            break;
                        }
                        else //最終行以外は、分割して更新する
                        {
                            //発注数がサービス伝票の販売数以上の場合は販売数を設定
                            if (orderQuantity >= workLines[k].Quantity)
                            {
                                workLines[k].OrderQuantity = workLines[k].Quantity;
                                orderQuantity -= workLines[k].Quantity;
                            }
                            else if (orderQuantity == 0)    //総発注数が０の場合は、明細の発注数も０にする
                            {
                                workLines[k].OrderQuantity = 0.00m;
                            }
                            else
                            {
                                workLines[k].OrderQuantity = orderQuantity;
                                orderQuantity = 0;
                            }
                        }
                    }
                }
                else //発注データが存在しない場合は、サービス伝票明細の発注数を0にする
                {
                     //受注伝票番号、部品番号、判断からサービス伝票の明細行の更新
                    List<ServiceSalesLine> workLines = lines.Where(x => !string.IsNullOrWhiteSpace(x.PartsNumber) && x.PartsNumber.Equals(l.Key.PartsNumber) && x.StockStatus.Equals(l.Key.StockStatus)).ToList();      //Mod 2018/09/18 yano #3925

                    //該当するサービス伝票明細の発注数に割り当てる。
                    for (int k = 0; k < workLines.Count; k++)
                    {
                        workLines[k].OrderQuantity = 0.00m;
                    }
                }
            }
        }

        #region 引当・仕掛処理(入荷時の引当処理)
        /// <summary>
        /// 入荷時の引当処理
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ情報</param>
        /// <param name="lines">サービス伝票明細情報</param>
        /// <param name="shikakariLocationCode">仕掛ロケーションコード</param>
        /// <param name="OrderType">発注区分</param>
        /// <param name="OrderPartsNumber">発注伝票番号</param>
        /// <param name="PurchasePartsNumber">入荷部品番号</param>
        /// <param name="PurchaseQuantity">入荷数</param>
        /// <param name="price">単価</param>
        /// <param name="Amount">金額</param>
        /// <param name="EmployeeCode">社員コード</param>
        /// <param name="SupplierCode">仕入先コード</param>
        /// <history>
        /// 
        /// 2018/09/10 yano #3941 部品入荷　受注伝票に同一発注種別、かつ同一部品番号の明細が複数存在する状態で、発注→入荷を実行するとシステムエラー
        /// 2018/05/30 arc yano #3889 サービス伝票発注部品が引当されない リビジョンを上げるように修正
        /// 2016/08/05 arc yano #3625 部品入荷入力　サービス伝票に紐づく入荷処理の引当処理 引当元のロケーションはフリーロケーションのリストではなく、入荷ロケーションに変更(入荷ロケーションにある分だけを引当)
        /// 
        /// 2016/01/29 arc yano #3416 部品入荷　    入荷確定時の引当の不具合対応　サービス伝票明細で「部品番号」かつ「判断」が
        ///                                         同一のレコードが複数存在する場合は、上から順番に引当し、引当を行う度に、
        ///                                         入荷数の減算を行うように修正
        ///                                             
        /// 2016/01/28 arc yano #3401 サービス伝票　代替品で分納時の明細行のソート処理の追加 
        ///                                         代替品のレコードを追加時には元部品レコードの次レコードに来るように表示順序を設定する
        /// </history>
        public void PurchaseHikiate(ref ServiceSalesHeader header, ref EntitySet<ServiceSalesLine> lines, string shikakariLocationCode, string OrderType, string OrderPartsNumber, string PurchasePartsNumber, decimal? PurchaseQuantity, decimal? price, decimal? Amount, string EmployeeCode, string SupplierCode, string PurchaseLocationCode)
        {
            EntitySet<ServiceSalesLine> newLines = new EntitySet<ServiceSalesLine>();
            ServiceSalesHeader newHeader = new ServiceSalesHeader();

            //Add 2018/05/30 arc yano #3889
            //サービス伝票のコピーを作成し付け替える
            CopyServiceSalesHeader(header, ref newHeader, ref newLines);

            //サービス伝票ヘッダ・明細の作成
            CreateServiceSalesOrder(newHeader, newLines);

            //サービス伝票支払情報の作成
            CreateServiceSalesPayment(newHeader, newLines);
               

            ServiceSalesLine svline = null;

            decimal? preProvisionQuantity = 0;      //入荷前の引当済数 　//Add 2016/01/29 arc yano #3416

            //----------------------------------------------------------------
            //サービス伝票明細行抜出(発注部品番号と発注区分で検索する)          
            //入荷の在庫更新による引当
            //----------------------------------------------------------------
            List<ServiceSalesLine> list =
                (
                 from a in newLines
                 where (!string.IsNullOrWhiteSpace(a.PartsNumber) && a.PartsNumber.Equals(OrderPartsNumber))
                 && (string.IsNullOrEmpty(OrderType) || a.StockStatus.Equals(OrderType))
                 orderby a.DisplayOrder
                 select a
                 ).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                if (PurchaseQuantity > 0)   //入荷数が0の場合は以降の処理は行わない
                {
                    ServiceSalesLine l = list[i];

                    //引当済数・発注数の初期化
                    //引当済数、発注数がnullの場合(初期登録の場合は０に設定する)
                    if (l.ProvisionQuantity == null)
                    {
                        l.ProvisionQuantity = 0;
                    }
                    if (l.OrderQuantity == null)
                    {
                        l.OrderQuantity = 0;
                    }

                    //マスタに存在する部品、かつ在庫管理対象の部品のみ引当を行う
                    if (new PartsDao(db).IsInventoryParts(OrderPartsNumber))
                    {
                        //在庫リストの初期化
                        List<PartsStock> stockList = new List<PartsStock>();

                        //今回必要数量の算出
                        decimal requireQuantity = 0;
                        //--------------------------------------------------------------------------
                        //発注した部品と入荷した部品が一致した場合、発注した部品で在庫リストを取得
                        //そうでなければ、入荷した部品番号で在庫リストを取得
                        //--------------------------------------------------------------------------
                        if (OrderPartsNumber == PurchasePartsNumber)
                        {
                            //発注通りの部品が入荷した場合
                            PartsStock rec = new PartsStockDao(db).GetByKey(OrderPartsNumber, PurchaseLocationCode);        //入荷ロケーションの在庫情報を取得
                            //取得した在庫情報を在庫リストに追加
                            stockList.Add(rec);
                            //stockList = new PartsStockDao(db).GetListByDepartment(header.DepartmentCode, OrderPartsNumber);   //Del 2016/08/05 arc yano #3625 

                            l.UnitCost = price;
                            l.Cost = (l.UnitCost ?? 0) * (l.Quantity ?? 0);

                            requireQuantity = (l.Quantity ?? 0) - (l.ProvisionQuantity ?? 0);

                            svline = l;
                        }
                        else
                        {
                            PartsStock rec = new PartsStockDao(db).GetByKey(PurchasePartsNumber, PurchaseLocationCode);        //入荷ロケーションの在庫情報を取得
                            //取得した在庫情報を在庫リストに追加
                            stockList.Add(rec);

                            //stockList = new PartsStockDao(db).GetListByDepartment(header.DepartmentCode, PurchasePartsNumber);    //Del 2016/08/05 arc yano #3625 

                            //---------------------------------------------------------------------------------------------
                            //　引当済数が0以上の場合：元々の明細の数量を更新して、入荷部品の新規レコードを作成する
                            //---------------------------------------------------------------------------------------------
                            if (l.ProvisionQuantity > 0)
                            {
                                //新しい明細の追加（入荷部品）
                                var query = from a in newLines select a.LineNumber;
                                int MaxLineNumber = query.Max();

                                ServiceSalesLine Newline = new ServiceSalesLine();
                                Newline.SlipNumber = l.SlipNumber;
                                Newline.RevisionNumber = l.RevisionNumber;
                                Newline.LineNumber = MaxLineNumber + 1; //一番最後の行に追加
                                Newline.ServiceType = l.ServiceType;
                                Newline.ServiceMenuCode = l.ServiceMenuCode;
                                Newline.ServiceWorkCode = l.ServiceWorkCode;
                                Newline.PartsNumber = PurchasePartsNumber; //入荷部品番号
                                Newline.LineContents = new PartsDao(db).GetByKey(PurchasePartsNumber).PartsNameJp; //入荷部品名
                                Newline.RequestComment = l.RequestComment;
                                Newline.WorkType = l.WorkType; //部品
                                Newline.LaborRate = l.LaborRate;
                                Newline.ManPower = l.ManPower;
                                Newline.TechnicalFeeAmount = l.TechnicalFeeAmount;
                                Newline.Quantity = l.Quantity - l.ProvisionQuantity; //元レコードの数量 - 元レコードの引当済数
                                Newline.Price = l.Price;
                                Newline.UnitCost = price;
                                Newline.Amount = (Newline.Quantity ?? 0) * (Newline.Price ?? 0);
                                Newline.Cost = (Newline.UnitCost ?? 0) * (Newline.Quantity ?? 0);
                                Newline.EmployeeCode = l.EmployeeCode;
                                Newline.SupplierCode = SupplierCode;
                                Newline.CustomerClaimCode = l.CustomerClaimCode;
                                Newline.StockStatus = OrderType;
                                Newline.Classification1 = l.Classification1;
                                Newline.TaxAmount = l.TaxAmount;
                                Newline.LineType = l.LineType;
                                Newline.ConsumptionTaxId = l.ConsumptionTaxId;
                                Newline.Rate = l.Rate;
                                Newline.ProvisionQuantity = 0; //この後の処理で更新される
                                Newline.OrderQuantity = 0; //発注数は0(ゼロ)
                                Newline.CreateEmployeeCode = EmployeeCode;
                                Newline.CreateDate = DateTime.Now;
                                Newline.LastUpdateEmployeeCode = EmployeeCode;
                                Newline.LastUpdateDate = DateTime.Now;
                                Newline.DelFlag = "0";
                                Newline.DisplayOrder = l.DisplayOrder + (decimal?)0.1;      //Mod 2016/01/28 arc yano #3401
                                db.ServiceSalesLine.InsertOnSubmit(Newline);
                                newLines.Add(Newline);
                                //db.SubmitChanges(); //Del 2018/05/30 arc yano #3889 移動

                                //元の明細更新
                                l.Quantity = l.ProvisionQuantity; //数量 = 引当済数
                                //金額・合計（原価）更新
                                l.Amount = (l.Quantity ?? 0) * (l.Price ?? 0);
                                l.Cost = (l.Quantity ?? 0) * (l.UnitCost ?? 0);

                                l.LastUpdateEmployeeCode = EmployeeCode;
                                l.LastUpdateDate = DateTime.Now;

                                //引当に必要な数
                                requireQuantity = (Newline.Quantity ?? 0) - (Newline.ProvisionQuantity ?? 0);

                                svline = Newline;
                            }
                            else
                            {
                                //--------------------------------------------------------------------------------------------------
                                //　入荷数 >= 元の数量 の場合、元データを新規部品で上書き（更新対象：部品番号・単価・金額）
                                //　それ以外は、全て入荷部品の新規レコードを作成する
                                //--------------------------------------------------------------------------------------------------
                                if (PurchaseQuantity >= l.Quantity)
                                {
                                    l.PartsNumber = PurchasePartsNumber;
                                    l.LineContents = new PartsDao(db).GetByKey(PurchasePartsNumber).PartsNameJp ?? "";
                                    l.UnitCost = price;
                                    l.Cost = (l.Quantity ?? 0) * (l.UnitCost ?? 0);

                                    //引当に必要な数
                                    requireQuantity = (l.Quantity ?? 0) - (l.ProvisionQuantity ?? 0);

                                    svline = l;
                                }
                                else
                                {
                                    //新規で入荷部品のレコード作成
                                    var query = from a in newLines select a.LineNumber;
                                    int MaxLineNumber = query.Max();

                                    ServiceSalesLine Newline = new ServiceSalesLine();
                                    Newline.SlipNumber = l.SlipNumber;
                                    Newline.RevisionNumber = l.RevisionNumber;
                                    Newline.LineNumber = MaxLineNumber + 1; //一番最後の行に追加
                                    Newline.ServiceType = l.ServiceType;
                                    Newline.ServiceMenuCode = l.ServiceMenuCode;
                                    Newline.ServiceWorkCode = l.ServiceWorkCode;
                                    Newline.PartsNumber = PurchasePartsNumber; //入荷部品番号
                                    Newline.LineContents = new PartsDao(db).GetByKey(PurchasePartsNumber).PartsNameJp; //入荷部品名
                                    Newline.RequestComment = l.RequestComment;
                                    Newline.WorkType = l.WorkType; //部品
                                    Newline.LaborRate = l.LaborRate;
                                    Newline.ManPower = l.ManPower;
                                    Newline.TechnicalFeeAmount = l.TechnicalFeeAmount;
                                    Newline.Quantity = PurchaseQuantity; //入荷数
                                    Newline.Price = l.Price;
                                    Newline.Amount = (Newline.Quantity ?? 0) * (Newline.Price ?? 0);
                                    Newline.UnitCost = price;
                                    Newline.Cost = (Newline.UnitCost ?? 0) * (Newline.Quantity ?? 0);
                                    Newline.EmployeeCode = l.EmployeeCode;
                                    Newline.SupplierCode = SupplierCode;
                                    Newline.CustomerClaimCode = l.CustomerClaimCode;
                                    Newline.StockStatus = OrderType;
                                    Newline.Classification1 = l.Classification1;
                                    Newline.TaxAmount = l.TaxAmount;
                                    Newline.LineType = l.LineType;
                                    Newline.ConsumptionTaxId = l.ConsumptionTaxId;
                                    Newline.Rate = l.Rate;
                                    Newline.ProvisionQuantity = 0; //この後の処理で更新される
                                    Newline.OrderQuantity = 0; //発注数は0(ゼロ)
                                    Newline.CreateEmployeeCode = EmployeeCode;
                                    Newline.CreateDate = DateTime.Now;
                                    Newline.LastUpdateEmployeeCode = EmployeeCode;
                                    Newline.LastUpdateDate = DateTime.Now;
                                    Newline.DelFlag = "0";
                                    Newline.DisplayOrder = l.DisplayOrder + (decimal?)0.1;      //Mod 2016/01/28 arc yano #3401
                                    db.ServiceSalesLine.InsertOnSubmit(Newline);
                                    newLines.Add(Newline);
                                    //db.SubmitChanges();   //Del 2018/05/30 arc yano #3889 移動


                                    //元レコード更新
                                    l.Quantity = l.Quantity - Newline.Quantity; //元数量 = 元数量 - 新数量
                                    //金額・原価(合計)を更新
                                    l.Amount = (l.Quantity ?? 0) * (l.Price ?? 0);
                                    l.Cost = (l.Quantity ?? 0) * (l.UnitCost ?? 0);

                                    requireQuantity = (Newline.Quantity ?? 0) - (Newline.ProvisionQuantity ?? 0);

                                    svline = Newline;
                                }
                            }
                        }

                        //Mod 2018/09/10 yano #3941 ループ外へ移動
                        //////Add 2018/05/30 arc yano #3889
                        //db.ServiceSalesHeader.InsertOnSubmit(newHeader);
                        //db.SubmitChanges();

                        //入荷前の引当済数を一度退避しておく
                        preProvisionQuantity = svline.ProvisionQuantity;        //Add 2016/01/28 arc yano #3401

                        //引当処理
                        Reserve(newHeader, ref svline, ref stockList, ref requireQuantity, shikakariLocationCode);

                        //入荷後の引当済数と入荷前引当済の差分(今回引当された数)分入荷数を減算
                        PurchaseQuantity -= (svline.ProvisionQuantity - preProvisionQuantity);  //Add 2016/01/28 arc yano #3401
                    }
                }
            }

            //Mod 2018/09/10 yano #3941
            db.ServiceSalesHeader.InsertOnSubmit(newHeader);
            db.SubmitChanges();

            /*
            //----------------------------------------------------------------
            //サービス伝票明細行抜出(発注部品番号と発注区分で検索する)          
            //入荷の在庫更新による引当
            //----------------------------------------------------------------
            List<ServiceSalesLine> list =
                (
                 from a in lines
                 where (!string.IsNullOrWhiteSpace(a.PartsNumber) && a.PartsNumber.Equals(OrderPartsNumber))
                 && (string.IsNullOrEmpty(OrderType) || a.StockStatus.Equals(OrderType))
                 orderby a.DisplayOrder
                 select a
                 ).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                if (PurchaseQuantity > 0)   //入荷数が0の場合は以降の処理は行わない
                {
                    ServiceSalesLine l = list[i];

                    //引当済数・発注数の初期化
                    //引当済数、発注数がnullの場合(初期登録の場合は０に設定する)
                    if (l.ProvisionQuantity == null)
                    {
                        l.ProvisionQuantity = 0;
                    }
                    if (l.OrderQuantity == null)
                    {
                        l.OrderQuantity = 0;
                    }

                    //マスタに存在する部品、かつ在庫管理対象の部品のみ引当を行う
                    if (new PartsDao(db).IsInventoryParts(OrderPartsNumber))
                    {
                        //在庫リストの初期化
                        List<PartsStock> stockList = new List<PartsStock>();

                        //今回必要数量の算出
                        decimal requireQuantity = 0;
                        //--------------------------------------------------------------------------
                        //発注した部品と入荷した部品が一致した場合、発注した部品で在庫リストを取得
                        //そうでなければ、入荷した部品番号で在庫リストを取得
                        //--------------------------------------------------------------------------
                        if (OrderPartsNumber == PurchasePartsNumber)
                        {
                            //発注通りの部品が入荷した場合
                            PartsStock rec = new PartsStockDao(db).GetByKey(OrderPartsNumber, PurchaseLocationCode);        //入荷ロケーションの在庫情報を取得
                            //取得した在庫情報を在庫リストに追加
                            stockList.Add(rec);
                            //stockList = new PartsStockDao(db).GetListByDepartment(header.DepartmentCode, OrderPartsNumber);   //Del 2016/08/05 arc yano #3625 
                            
                            l.UnitCost = price;
                            l.Cost = (l.UnitCost ?? 0) * (l.Quantity ?? 0);

                            requireQuantity = (l.Quantity ?? 0) - (l.ProvisionQuantity ?? 0);

                            svline = l;
                        }
                        else
                        {
                            PartsStock rec = new PartsStockDao(db).GetByKey(PurchasePartsNumber, PurchaseLocationCode);        //入荷ロケーションの在庫情報を取得
                            //取得した在庫情報を在庫リストに追加
                            stockList.Add(rec);

                            //stockList = new PartsStockDao(db).GetListByDepartment(header.DepartmentCode, PurchasePartsNumber);    //Del 2016/08/05 arc yano #3625 

                            //---------------------------------------------------------------------------------------------
                            //　引当済数が0以上の場合：元々の明細の数量を更新して、入荷部品の新規レコードを作成する
                            //---------------------------------------------------------------------------------------------
                            if (l.ProvisionQuantity > 0)
                            {
                                //新しい明細の追加（入荷部品）
                                var query = from a in lines select a.LineNumber;
                                int MaxLineNumber = query.Max();

                                ServiceSalesLine Newline = new ServiceSalesLine();
                                Newline.SlipNumber = l.SlipNumber;
                                Newline.RevisionNumber = l.RevisionNumber;
                                Newline.LineNumber = MaxLineNumber + 1; //一番最後の行に追加
                                Newline.ServiceType = l.ServiceType;
                                Newline.ServiceMenuCode = l.ServiceMenuCode;
                                Newline.ServiceWorkCode = l.ServiceWorkCode;
                                Newline.PartsNumber = PurchasePartsNumber; //入荷部品番号
                                Newline.LineContents = new PartsDao(db).GetByKey(PurchasePartsNumber).PartsNameJp; //入荷部品名
                                Newline.RequestComment = l.RequestComment;
                                Newline.WorkType = l.WorkType; //部品
                                Newline.LaborRate = l.LaborRate;
                                Newline.ManPower = l.ManPower;
                                Newline.TechnicalFeeAmount = l.TechnicalFeeAmount;
                                Newline.Quantity = l.Quantity - l.ProvisionQuantity; //元レコードの数量 - 元レコードの引当済数
                                Newline.Price = l.Price;
                                Newline.UnitCost = price;
                                Newline.Amount = (Newline.Quantity ?? 0) * (Newline.Price ?? 0);
                                Newline.Cost = (Newline.UnitCost ?? 0) * (Newline.Quantity ?? 0);
                                Newline.EmployeeCode = l.EmployeeCode;
                                Newline.SupplierCode = SupplierCode;
                                Newline.CustomerClaimCode = l.CustomerClaimCode;
                                Newline.StockStatus = OrderType;
                                Newline.Classification1 = l.Classification1;
                                Newline.TaxAmount = l.TaxAmount;
                                Newline.LineType = l.LineType;
                                Newline.ConsumptionTaxId = l.ConsumptionTaxId;
                                Newline.Rate = l.Rate;
                                Newline.ProvisionQuantity = 0; //この後の処理で更新される
                                Newline.OrderQuantity = 0; //発注数は0(ゼロ)
                                Newline.CreateEmployeeCode = EmployeeCode;
                                Newline.CreateDate = DateTime.Now;
                                Newline.LastUpdateEmployeeCode = EmployeeCode;
                                Newline.LastUpdateDate = DateTime.Now;
                                Newline.DelFlag = "0";
                                Newline.DisplayOrder = l.DisplayOrder + (decimal?)0.1;      //Mod 2016/01/28 arc yano #3401
                                db.ServiceSalesLine.InsertOnSubmit(Newline);
                                lines.Add(Newline);
                                //db.SubmitChanges(); //Del 2018/05/30 arc yano #3889 移動

                                //元の明細更新
                                l.Quantity = l.ProvisionQuantity; //数量 = 引当済数
                                //金額・合計（原価）更新
                                l.Amount = (l.Quantity ?? 0) * (l.Price ?? 0);
                                l.Cost = (l.Quantity ?? 0) * (l.UnitCost ?? 0);

                                l.LastUpdateEmployeeCode = EmployeeCode;
                                l.LastUpdateDate = DateTime.Now;

                                //引当に必要な数
                                requireQuantity = (Newline.Quantity ?? 0) - (Newline.ProvisionQuantity ?? 0);

                                svline = Newline;
                            }
                            else
                            {
                                //--------------------------------------------------------------------------------------------------
                                //　入荷数 >= 元の数量 の場合、元データを新規部品で上書き（更新対象：部品番号・単価・金額）
                                //　それ以外は、全て入荷部品の新規レコードを作成する
                                //--------------------------------------------------------------------------------------------------
                                if (PurchaseQuantity >= l.Quantity)
                                {
                                    l.PartsNumber = PurchasePartsNumber;
                                    l.LineContents = new PartsDao(db).GetByKey(PurchasePartsNumber).PartsNameJp ?? "";
                                    l.UnitCost = price;
                                    l.Cost = (l.Quantity ?? 0) * (l.UnitCost ?? 0);

                                    //引当に必要な数
                                    requireQuantity = (l.Quantity ?? 0) - (l.ProvisionQuantity ?? 0);

                                    svline = l;
                                }
                                else
                                {
                                    //新規で入荷部品のレコード作成
                                    var query = from a in lines select a.LineNumber;
                                    int MaxLineNumber = query.Max();

                                    ServiceSalesLine Newline = new ServiceSalesLine();
                                    Newline.SlipNumber = l.SlipNumber;
                                    Newline.RevisionNumber = l.RevisionNumber;
                                    Newline.LineNumber = MaxLineNumber + 1; //一番最後の行に追加
                                    Newline.ServiceType = l.ServiceType;
                                    Newline.ServiceMenuCode = l.ServiceMenuCode;
                                    Newline.ServiceWorkCode = l.ServiceWorkCode;
                                    Newline.PartsNumber = PurchasePartsNumber; //入荷部品番号
                                    Newline.LineContents = new PartsDao(db).GetByKey(PurchasePartsNumber).PartsNameJp; //入荷部品名
                                    Newline.RequestComment = l.RequestComment;
                                    Newline.WorkType = l.WorkType; //部品
                                    Newline.LaborRate = l.LaborRate;
                                    Newline.ManPower = l.ManPower;
                                    Newline.TechnicalFeeAmount = l.TechnicalFeeAmount;
                                    Newline.Quantity = PurchaseQuantity; //入荷数
                                    Newline.Price = l.Price;
                                    Newline.Amount = (Newline.Quantity ?? 0) * (Newline.Price ?? 0);
                                    Newline.UnitCost = price;
                                    Newline.Cost = (Newline.UnitCost ?? 0) * (Newline.Quantity ?? 0);
                                    Newline.EmployeeCode = l.EmployeeCode;
                                    Newline.SupplierCode = SupplierCode;
                                    Newline.CustomerClaimCode = l.CustomerClaimCode;
                                    Newline.StockStatus = OrderType;
                                    Newline.Classification1 = l.Classification1;
                                    Newline.TaxAmount = l.TaxAmount;
                                    Newline.LineType = l.LineType;
                                    Newline.ConsumptionTaxId = l.ConsumptionTaxId;
                                    Newline.Rate = l.Rate;
                                    Newline.ProvisionQuantity = 0; //この後の処理で更新される
                                    Newline.OrderQuantity = 0; //発注数は0(ゼロ)
                                    Newline.CreateEmployeeCode = EmployeeCode;
                                    Newline.CreateDate = DateTime.Now;
                                    Newline.LastUpdateEmployeeCode = EmployeeCode;
                                    Newline.LastUpdateDate = DateTime.Now;
                                    Newline.DelFlag = "0";
                                    Newline.DisplayOrder = l.DisplayOrder + (decimal?)0.1;      //Mod 2016/01/28 arc yano #3401
                                    db.ServiceSalesLine.InsertOnSubmit(Newline);
                                    lines.Add(Newline);
                                    //db.SubmitChanges();   //Del 2018/05/30 arc yano #3889 移動


                                    //元レコード更新
                                    l.Quantity = l.Quantity - Newline.Quantity; //元数量 = 元数量 - 新数量
                                    //金額・原価(合計)を更新
                                    l.Amount = (l.Quantity ?? 0) * (l.Price ?? 0);
                                    l.Cost = (l.Quantity ?? 0) * (l.UnitCost ?? 0);

                                    requireQuantity = (Newline.Quantity ?? 0) - (Newline.ProvisionQuantity ?? 0);

                                    svline = Newline;
                                }
                            }
                        }

                        //Add 2018/05/30 arc yano #3889
                        db.ServiceSalesHeader.InsertOnSubmit(header);
                        db.SubmitChanges();

                        //入荷前の引当済数を一度退避しておく
                        preProvisionQuantity = svline.ProvisionQuantity;        //Add 2016/01/28 arc yano #3401

                        //引当処理
                        Reserve(header, ref svline, ref stockList, ref requireQuantity, shikakariLocationCode);

                        //入荷後の引当済数と入荷前引当済の差分(今回引当された数)分入荷数を減算
                        PurchaseQuantity -= (svline.ProvisionQuantity - preProvisionQuantity);  //Add 2016/01/28 arc yano #3401
                    }
                }
            }
            */
           
            return;
        }
        #endregion

        //Add 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
        //                              引当ロケーションへの移動は止めて、代わりに引当済数を更新する
        #region 引当・仕掛処理
        /// <summary>
        /// 引当処理
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ情報</param>
        /// <param name="lines">サービス伝票明細情報</param>
        /// <return>発注情報</return>
        /// <history>
        /// 2018/02/24 arc yano #3831 サービス伝票入力　部品発注→部品入荷後の再発注時の発注数の不具合 発注処理を選択している場合は数量分全て発注とし、引当は行わない
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 まずはサービス伝票の引当履歴より在庫情報を取得する
        /// 2017/01/31 arc yano #3566 サービス伝票入力　部門変更時の在庫の再引当 DBに登録されている伝票の部門と画面に入力されている部門が異なる場合は引当を一旦解除する
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 在庫リストの取得方法の変更(部門→ロケーションではなく、部門→倉庫→ロケーションで取得する)
        /// 2016/02/22 arc yano #3434 サービス伝票  消耗品(SP)の対応　原価０の部品と同様の対応を行う
        /// 2016/02/17 arc yano #3435_サービス伝票　原価０(社達品)の部品の対応
        ///                         在庫判断で「社達」の場合は、引当を行わないように対応する
        ///                         また、当該部品が在庫管理対象かどうかの判定をメソッドで求めるように修正
        /// </history>
        public List<PartsPurchaseOrder> Hikiate(ref ServiceSalesHeader header, ref EntitySet<ServiceSalesLine> lines, string shikakariLocationCode)
        { 
            //部品マスタ
            Parts parts = null;

            //発注データ
            List<PartsPurchaseOrder> list = new List<PartsPurchaseOrder>();

            //Add 2017/01/31 arc yano #3566
            //------------------------------------------------
            //部門が変更されたかどうかをチェック
            //------------------------------------------------
            //DBに登録されている伝票を取得する
            ServiceSalesHeader dbheader = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);
            
            //画面の入力値とDBの登録値が異なる場合(=部門が変更された場合)
            if (dbheader != null && !dbheader.DepartmentCode.Equals(header.DepartmentCode))
            {
                ServiceSalesHeader wkheader = dbheader;
                EntitySet<ServiceSalesLine> dblines = new EntitySet<ServiceSalesLine>();
                string dbshikakariLocationCode = "";

                //仕掛ロケーションの取得
                DepartmentWarehouse dwhouse = new DepartmentWarehouseDao(db).GetByDepartment(dbheader.DepartmentCode);

                if (dwhouse != null)
                {
                    Location shikakariLocation = stockService.GetShikakariLocation(dwhouse.WarehouseCode);

                    // Mod 2017/02/08 arc yano #3620　バグ対応
                    if (shikakariLocation != null)  //仕掛ロケーションのレコードが存在する場合
                    {
                        dbshikakariLocationCode = shikakariLocation.LocationCode;
                    }
                }

                // Mod 2017/02/08 arc yano #3620　バグ対応
                //引当解除(引当前処理で行う)
                if (!string.IsNullOrWhiteSpace(dbshikakariLocationCode))  //仕掛ロケーションが存在する場合
                {
                    PreProcessHikiate(ref dbheader, ref dblines, dbshikakariLocationCode);      //変更前部門の引当を解除する
                }

                //引当解除を行ったので全て引当済数を０にする
                foreach (var l in lines)
                {
                    l.ProvisionQuantity = 0m;
                }
            }
            else
            {
                //引当て前処理実行
                PreProcessHikiate(ref header, ref lines, shikakariLocationCode);
            }

            //----------------------------------------------------------------
            //引当・引当解除処理(本処理)          
            //明細行の数量更新による引当・引当解除処理を
            //----------------------------------------------------------------
            for (int i = 0; i < lines.Count(); i++ )
            {
                //サービス伝票明細行抜出
                ServiceSalesLine l = lines[i];

                //引当済数・発注数の初期化
                //引当済数、発注数がnullの場合(初期登録の場合は０に設定する)
                if (l.ProvisionQuantity == null)
                {
                    l.ProvisionQuantity = 0;
                }
                if (l.OrderQuantity == null)
                {
                    l.OrderQuantity = 0;
                }

                //部品マスタを取得する
                parts = new PartsDao(db).GetByKey(l.PartsNumber);

                
                //Mod 2016/02/22 arc yano #3434
                //Mod 2016/02/17 arc yano #3435 当該部品が在庫管理対象かどうかの判定でメソッドを用いるように修正。
                //マスタに存在する部品、かつ在庫管理対象の部品のみ引当を行う
                //if (!string.IsNullOrEmpty(l.PartsNumber) && parts != null && (string.IsNullOrEmpty(parts.NonInventoryFlag) || !parts.NonInventoryFlag.Equals("1")))
                if (!string.IsNullOrEmpty(l.PartsNumber) && new PartsDao(db).IsInventoryParts(l.PartsNumber) && !l.StockStatus.Equals("998") && !l.StockStatus.Equals("997"))
                {
                    //Mod 2018/02/24 arc yano #3831 
                    //---------------------------------------------------
                    //発注の場合は引当処理を行わずに、入荷時に引当を行う
                    //---------------------------------------------------
                    //判断≠「在庫」の場合
                    if (!l.StockStatus.Equals(STS_STOCK))
                    {
                        //販売数 > 発注済数の場合、発注データを作成する（発注データの登録はここでは行わずに、発注画面へのパラメータを作成）
                        if ( !string.IsNullOrEmpty(l.StockStatus) && (l.OrderQuantity < l.Quantity))
                        {
                            list = SetPurchaseOrder(header.SlipNumber, l.StockStatus, l.PartsNumber, (l.Quantity ?? 0m) - (l.OrderQuantity ?? 0m), header.DepartmentCode, list, parts);
                        }

                        continue;
                    }
                    
                    List<PartsStock> stockList = new List<PartsStock>();

                    //Mod 2016/08/13 arc yano #3596
                    //移動元は在庫があるロケーションから順番に使う
                    //部門から倉庫を特定する
                    DepartmentWarehouse departmentWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
                   

                    //今回必要数量の算出（販売数－引当済数）
                    decimal requireQuantity = (l.Quantity ?? 0) - (l.ProvisionQuantity ?? 0);

                    //---------------------------------------------------------------------------------------------
                    //引当処理
                    //必要数 >= 0(未引当あり)の場合は、引当を行う
                    //---------------------------------------------------------------------------------------------
                    if (requireQuantity > 0)
                    {
                        //倉庫コードを元に在庫リストを取得　※削除済在庫情報は含まない
                        stockList = new PartsStockDao(db).GetListByWarehouse((departmentWarehouse != null ? departmentWarehouse.WarehouseCode : ""), l.PartsNumber, false);

                        Reserve(header, ref l, ref stockList, ref requireQuantity, shikakariLocationCode);
                    }
                    //---------------------------------------------------------------------------------------------
                    //引当解除処理
                    //必要数 < 0(※)の場合は引当解除を行う
                    //※明細の数量を減算した等により、明細の数量 < 引当済数となった場合
                    //---------------------------------------------------------------------------------------------
                    else if (requireQuantity < 0)
                    {
                        //Add 2017/02/08 arc yano #3620
                        //サービス伝票の引当履歴から引当元のロケーションの在庫情報を取得する
                        stockList = GetStockListByTransfer(header, l.PartsNumber);

                        //サービス伝票の引当履歴から引当元のロケーションの在庫情報を取得できない場合は、倉庫全体から在庫情報を取得する
                        if (stockList.Count() == 0)
                        {
                            //倉庫コードを元に在庫リストを取得　削除済在庫情報を含む
                            stockList = new PartsStockDao(db).GetListByWarehouse((departmentWarehouse != null ? departmentWarehouse.WarehouseCode : ""), l.PartsNumber, true);
                        }

                        ReleaseReserve(header, ref l, ref stockList, ref requireQuantity, shikakariLocationCode);
                    }
                    else
                    {
                        //何もしない
                    }

                    //必要数分引当できなかった場合は、残り数分の発注画面を表示する
                    if (requireQuantity > 0)
                    {
                        //判断＝「在庫」の場合はE/Oで発注データを作成する
                        if (!string.IsNullOrEmpty(l.StockStatus) && l.StockStatus.Equals(STS_STOCK))
                        {
                            list = SetPurchaseOrder(header.SlipNumber, TYPE_ORDER_EO, l.PartsNumber, requireQuantity, header.DepartmentCode, list, parts);
                        }
                    }
                }
            }
            return list;
        }
         /// <summary>
        /// 引当前処理処理(明細行が削除された等により、引当済在庫の戻し処理)
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ情報</param>
        /// <param name="lines">サービス伝票明細情報</param>
        /// <return>発注情報</return>
        /// <history>
        /// 2018/02/24 arc yano #3831 サービス伝票入力　部品発注→部品入荷後の再発注時の発注数の不具合 発注処理を選択している場合は数量分全て発注を行う
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 戻し先はまずサービス伝票の引当履歴より引当されたロケーションを取得する
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し 戻し先はまずは引当されたロケーションを取得する
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 在庫リストの取得方法の変更(部門→ロケーションではなく、部門→倉庫→ロケーションで取得する)
        /// 2016/02/22 arc yano #3434 サービス伝票  消耗品(SP)の対応 原価０の部品の対応と同様の対応を行う
        /// 2016/02/17 arc yano #3435_サービス伝票　原価０(社達品)の部品の対応
        ///                         在庫判断で「社達」の場合は、引当を行わないように対応する
        /// </history>
        public void PreProcessHikiate(ref ServiceSalesHeader header, ref EntitySet<ServiceSalesLine> lines, string shikakariLocationCode)
        {
            //----------------------------------------------------------------
            //引当解除処理(前処理)
            //①明細行が削除された
            //②部品番号が変更された
            //③判断が変更された(在庫→S/O等)
            //場合に
            //引当済数の変化分の引当解除を行う
            //※明細行の数量更新による引当解除はここでは行わない
            //----------------------------------------------------------------
            decimal difQuantity = 0;

            //--------------------------------------------
            //DBの最新リビジョンの明細行の取得
            //--------------------------------------------
            var ret = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

            //DBに登録されている場合のみ処理実行(新規作成時は前処理は必要なし)
            if (ret != null)
            {
                //Mod 2018/02/24 arc yano #3831
                //在庫分、発注分の２つに分けて処理を行う
                //---------------------------
                //在庫分の確認
                //---------------------------
                //Mod 2016/02/22 #3434
                //Mod 2016/02/17 #3435
                //在庫管理対象で判断が「在庫」のものを取得
                var partsLinesDb = (ret.ServiceSalesLine.Where(x => new PartsDao(db).IsInventoryParts(x.PartsNumber) && x.StockStatus.Equals(STS_STOCK))).Select(x => new { PartsNumber = x.PartsNumber, ProvisionQuantity = x.ProvisionQuantity ?? 0 });

                //明細行の引当済数を部品毎にグルーピング
                var lGroupByPartsDb = partsLinesDb.GroupBy(x => x.PartsNumber).Select(x => new { PartsNumber = x.Key, sumQuantity = x.Select(y => y.ProvisionQuantity).Sum() });

                //--------------------------------------------
                //エンティティの明細行(=画面の入力値)の取得
                //--------------------------------------------
                //Mod 2016/02/22 #3434
                //Mod 2016/02/17 #3435
                //在庫管理対象で判断が「在庫」のものを取得
                var partsLinesEntity = (lines.Where(x => (new PartsDao(db).IsInventoryParts(x.PartsNumber)) && x.StockStatus.Equals(STS_STOCK))).Select(x => new { PartsNumber = x.PartsNumber, ProvisionQuantity = x.ProvisionQuantity ?? 0 });
                //明細行の引当済数を部品毎にグルーピング
                var lGroupByPartsEntity = partsLinesEntity.GroupBy(x => x.PartsNumber).Select(x => new { PartsNumber = x.Key, sumQuantity = x.Select(y => y.ProvisionQuantity).Sum() });

                //エンティティと現リビジョンの引当済数を比較して、エンティティの引当済数が減っていた場合は減った分だけ引当解除処理を行う
                foreach (var l in lGroupByPartsDb)
                {
                    //対象部品の絞込み
                    var rec = (lGroupByPartsEntity.Where(x => x.PartsNumber.Equals(l.PartsNumber))).FirstOrDefault();

                    difQuantity = (rec == null ? 0 : rec.sumQuantity) - l.sumQuantity;

                    //エンティティの数量が現リビジョンの数量より少ない場合は引当解除実行
                    if (difQuantity < 0)
                    {
                        //Mod 2016/08/13 arc yano #3596
                        //部門から倉庫を特定する
                        DepartmentWarehouse departmentWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);

                        //Add 2017/02/08 arc yano #3620
                        List<PartsStock> releseStockList = new List<PartsStock>();

                        //サービス伝票の引当履歴より引当元ロケーションの在庫情報を取得する
                        releseStockList = GetStockListByTransfer(header, l.PartsNumber);

                        //サービス伝票の引当履歴より引当元ロケーションの在庫情報を取得できなかった場合 ※削除された在庫情報を含む
                        if (releseStockList.Count() == 0)
                        {
                            releseStockList = new PartsStockDao(db).GetListByWarehouse((departmentWarehouse != null ? departmentWarehouse.WarehouseCode : ""), l.PartsNumber, true);
                        }

                        //引当解除処理実行
                        ServiceSalesLine line = new ServiceSalesLine();
                        line.PartsNumber = l.PartsNumber;
                        ReleaseReserve(header, ref line, ref releseStockList, ref difQuantity, shikakariLocationCode);
                    }
                }

                difQuantity = 0;
                //---------------------------
                //発注分の確認
                //---------------------------
                //在庫管理対象で判断が「発注」のものを取得
                var orderPartsLinesDb = (ret.ServiceSalesLine.Where(x => new PartsDao(db).IsInventoryParts(x.PartsNumber) && !x.StockStatus.Equals("997") && !x.StockStatus.Equals("998") && !x.StockStatus.Equals(STS_STOCK))).Select(x => new { PartsNumber = x.PartsNumber, ProvisionQuantity = x.ProvisionQuantity ?? 0 });

                //明細行の引当済数を部品毎にグルーピング
                var lGroupByOrderPartsDb = orderPartsLinesDb.GroupBy(x => x.PartsNumber).Select(x => new { PartsNumber = x.Key, sumQuantity = x.Select(y => y.ProvisionQuantity).Sum() });

                //--------------------------------------------
                //エンティティの明細行(=画面の入力値)の取得
                //--------------------------------------------
                //在庫管理対象で判断が「発注」のものを取得
                var orderPartsLinesEntity = (lines.Where(x => (new PartsDao(db).IsInventoryParts(x.PartsNumber)) && !x.StockStatus.Equals("997") && !x.StockStatus.Equals("998") && !x.StockStatus.Equals(STS_STOCK))).Select(x => new { PartsNumber = x.PartsNumber, ProvisionQuantity = x.ProvisionQuantity ?? 0 });
                //明細行の引当済数を部品毎にグルーピング
                var lGroupByOrderPartsEntity = orderPartsLinesEntity.GroupBy(x => x.PartsNumber).Select(x => new { PartsNumber = x.Key, sumQuantity = x.Select(y => y.ProvisionQuantity).Sum() });

                //エンティティと現リビジョンの引当済数を比較して、エンティティの引当済数が減っていた場合は減った分だけ引当解除処理を行う
                foreach (var l in lGroupByOrderPartsDb)
                {
                    //対象部品の絞込み
                    var rec = (lGroupByOrderPartsEntity.Where(x => x.PartsNumber.Equals(l.PartsNumber))).FirstOrDefault();

                    difQuantity = (rec == null ? 0 : rec.sumQuantity) - l.sumQuantity;

                    //エンティティの数量が現リビジョンの数量より少ない場合は引当解除実行
                    if (difQuantity < 0)
                    {
                        //部門から倉庫を特定する
                        DepartmentWarehouse departmentWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);

                        List<PartsStock> releseStockList = new List<PartsStock>();

                        //サービス伝票の引当履歴より引当元ロケーションの在庫情報を取得する
                        releseStockList = GetStockListByTransfer(header, l.PartsNumber);

                        //サービス伝票の引当履歴より引当元ロケーションの在庫情報を取得できなかった場合 ※削除された在庫情報を含む
                        if (releseStockList.Count() == 0)
                        {
                            releseStockList = new PartsStockDao(db).GetListByWarehouse((departmentWarehouse != null ? departmentWarehouse.WarehouseCode : ""), l.PartsNumber, true);
                        }

                        //引当解除処理実行
                        ServiceSalesLine line = new ServiceSalesLine();
                        line.PartsNumber = l.PartsNumber;
                        ReleaseReserve(header, ref line, ref releseStockList, ref difQuantity, shikakariLocationCode);
                    }
                }
            }

            /*
            //--------------------------------------------
            //DBの最新リビジョンの明細行の取得
            //--------------------------------------------
            var ret = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

            //DBに登録されている場合のみ処理実行(新規作成時は前処理は必要なし)
            if (ret != null)
            {
                //Mod 2016/02/22 #3434
                //Mod 2016/02/17 #3435
                //在庫管理対象の部品の明細行だけ取得
                //var partsLinesDb = (ret.ServiceSalesLine.Where(x => new PartsDao(db).IsInventoryParts(x.PartsNumber))).Select(x => new { PartsNumber = x.PartsNumber, ProvisionQuantity = x.ProvisionQuantity ?? 0 });
                var partsLinesDb = (ret.ServiceSalesLine.Where(x => new PartsDao(db).IsInventoryParts(x.PartsNumber) && !x.StockStatus.Equals("998") && !x.StockStatus.Equals("997"))).Select(x => new { PartsNumber = x.PartsNumber, ProvisionQuantity = x.ProvisionQuantity ?? 0 });

                //明細行の引当済数を部品毎にグルーピング
                var lGroupByPartsDb = partsLinesDb.GroupBy(x => x.PartsNumber).Select(x => new { PartsNumber = x.Key, sumQuantity = x.Select(y => y.ProvisionQuantity).Sum() });

                //--------------------------------------------
                //エンティティの明細行(=画面の入力値)の取得
                //--------------------------------------------
                //Mod 2016/02/22 #3434
                //Mod 2016/02/17 #3435
                //在庫管理対象の部品の明細行だけ取得
                //var partsLinesEntity = (lines.Where(x => (new PartsDao(db).IsInventoryParts(x.PartsNumber)))).Select(x => new { PartsNumber = x.PartsNumber, ProvisionQuantity = x.ProvisionQuantity ?? 0 });
                var partsLinesEntity = (lines.Where(x => (new PartsDao(db).IsInventoryParts(x.PartsNumber)) && !x.StockStatus.Equals("998") && !x.StockStatus.Equals("997"))).Select(x => new { PartsNumber = x.PartsNumber, ProvisionQuantity = x.ProvisionQuantity ?? 0 });
                //明細行の引当済数を部品毎にグルーピング
                var lGroupByPartsEntity = partsLinesEntity.GroupBy(x => x.PartsNumber).Select(x => new { PartsNumber = x.Key, sumQuantity = x.Select(y => y.ProvisionQuantity).Sum() });

                //エンティティと現リビジョンの引当済数を比較して、エンティティの数量が減っていた場合は減った分だけ引当解除処理を行う
                foreach (var l in lGroupByPartsDb)
                {
                    //対象部品の絞込み
                    var rec = (lGroupByPartsEntity.Where(x => x.PartsNumber.Equals(l.PartsNumber))).FirstOrDefault();

                    difQuantity = (rec == null ? 0 : rec.sumQuantity) - l.sumQuantity;

                    //エンティティの数量が現リビジョンの数量より少ない場合は引当解除実行
                    if (difQuantity < 0)
                    {
                        //Mod 2016/08/13 arc yano #3596
                        //部門から倉庫を特定する
                        DepartmentWarehouse departmentWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);

                        //Add 2017/02/08 arc yano #3620
                        List<PartsStock> releseStockList = new List<PartsStock>();

                        //サービス伝票の引当履歴より引当元ロケーションの在庫情報を取得する
                        releseStockList = GetStockListByTransfer(header, l.PartsNumber);

                        //サービス伝票の引当履歴より引当元ロケーションの在庫情報を取得できなかった場合 ※削除された在庫情報を含む
                        if (releseStockList.Count() == 0)
                        {
                            releseStockList = new PartsStockDao(db).GetListByWarehouse((departmentWarehouse != null ? departmentWarehouse.WarehouseCode : ""), l.PartsNumber, true);
                        }

                        //引当解除処理実行
                        ServiceSalesLine line = new ServiceSalesLine();
                        line.PartsNumber = l.PartsNumber;
                        ReleaseReserve(header, ref line, ref releseStockList, ref difQuantity, shikakariLocationCode);
                    }
                }
            }
            */
        }

        /// <summary>
        /// 引当処理(明細１行単位の引当)
        /// </summary>
        /// <param name="header">サービス伝票(ヘッダ情報)</param>
        /// <param name="line">サービス伝票(明細１行)</param>
        /// <param name="stockList">在庫リスト</param>
        /// <param name="shikakariLocationCode">仕掛ロケーションコード</param>
        /// <history>
        /// 2016/06/13 arc yano #3571 仕掛処理のメソッド名と引数を変更、引当の場合も移動レコードを作成する
        /// </history>
        private void Reserve(ServiceSalesHeader header, ref ServiceSalesLine line, ref List<PartsStock> stockList,  ref decimal requireQuantity, string shikakariLocationCode)
        {
            decimal incOrDecQuantity = 0;             //増減数

            //必要数全てを引き当てられるか、全てのロケーションから引当終えたら終了
            for (int i = 0; i < stockList.Count; i++)
            {   
                //在庫リストの引当済数の初期化
                if (stockList[i].ProvisionQuantity == null)
                {
                    stockList[i].ProvisionQuantity = 0;     //在庫リストの引当済数がnullの場合は0に設定
                }
                
                //在庫の引当可能数を算出。
                decimal allowableQuantity = (stockList[i].Quantity ?? 0) - (stockList[i].ProvisionQuantity ?? 0);

                //引当可能数 <= 0の場合は次の在庫ロケーションへ
                if(allowableQuantity <= 0)
                {
                    continue;
                }

                //---------------------------------------------------------
                ///増減数の決定
                // ①対象ロケーションの引当可能数が必要数より少ない場合
                //   →増減数 = 引当可能数
                // ②対象ロケーションの引当可能数が必要数より多い場合
                //   →増減数 = 必要数
                //---------------------------------------------------------
                incOrDecQuantity = (allowableQuantity - requireQuantity < 0) ? allowableQuantity : requireQuantity;

                //増減数で明細行、在庫情報の引当済数と必要数を更新する
                line.ProvisionQuantity += incOrDecQuantity;
                stockList[i].ProvisionQuantity += incOrDecQuantity;
                requireQuantity -= incOrDecQuantity;

                PartsTransfer(stockList[i].LocationCode, stockList[i].LocationCode, stockList[i].PartsNumber, incOrDecQuantity, header.SlipNumber, TTYPE_PROVISION, updatePartsStock: false);  //Add 2016/06/13 arc yano #3571

                //---------------------------------------------------------------------
                //仕掛処理
                //伝票ステータス=「作業中」「作業終了」「納車確認書確認済」の場合は
                //仕掛へ移動も行う
                //---------------------------------------------------------------------
                if ( header.ServiceOrderStatus.Equals(STS_BEGINWORK) ||
                     header.ServiceOrderStatus.Equals(STS_ENDWORK)   ||
                     header.ServiceOrderStatus.Equals(STS_CONFIRMED))
                {
                    //仕掛処理を行う。
                    //WorkProgress(stockList[i].LocationCode, shikakariLocationCode, stockList[i].PartsNumber, incOrDecQuantity, header.SlipNumber, TTYPE_WORK);
                    PartsTransfer(stockList[i].LocationCode, shikakariLocationCode, stockList[i].PartsNumber, incOrDecQuantity, header.SlipNumber, TTYPE_WORK, updatePartsStock: true);  //Mod 2016/06/13 arc yano #3571
                }

                //必要数全部引当済の場合は終了
                if (requireQuantity == 0)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 引当解除処理
        /// </summary>
        /// <param name="header">サービス伝票(ヘッダ情報)</param>
        /// <param name="line">サービス伝票(明細１行)</param>
        /// <param name="stockList">在庫リスト</param>
        /// <param name="shikakariLocationCode">仕掛ロケーション</param>
        /// <history>
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 引当解除を行った在庫情報の削除フラグを0にする処理の追加
        /// 2016/06/13 arc yano #3571 仕掛処理のメソッド名と引数を変更、引当解除時も移動伝票を作成する
        /// 2016/06/09 arc yano #3571 引当時・引当解除時に移動伝票を作成する
        /// </history>
        private void ReleaseReserve(ServiceSalesHeader header, ref ServiceSalesLine line, ref List<PartsStock> stockList, ref decimal requireQuantity, string shikakariLocationCode)
        {
            decimal incOrDecQuantity = 0;             //増減数
            
            //正の数に変換
            requireQuantity = requireQuantity * (-1);
            
            //---------------------------------------------------------------------
            //仕掛解除処理
            //伝票ステータス=「作業中」「作業終了」「納車確認書確認済」の場合は
            //先に仕掛から在庫へ移動を行う。
            //戻し先は、対象の部品があるロケーションの一番最初のロケーション
            //---------------------------------------------------------------------
            if (header.ServiceOrderStatus.Equals(STS_BEGINWORK) ||
                 header.ServiceOrderStatus.Equals(STS_ENDWORK) ||
                 header.ServiceOrderStatus.Equals(STS_CONFIRMED))
            {
                //Add 2017/02/08 arc yano #3620
                //一番最初のロケーションが削除済データの場合は初期化した上で有効化
                stockList[0] = new PartsStockDao(db).InitPartsStock(stockList[0]);
                
                PartsTransfer(shikakariLocationCode, stockList[0].LocationCode, line.PartsNumber, requireQuantity, header.SlipNumber, TTYPE_CANCELWORK, updatePartsStock: true); //Mod 2016/06/13 arc yano #3571
            }

            //必要数全てを引当解除を行えたら終了
            for (int i = 0; i < stockList.Count; i++)
            {
                //Add 2017/02/08 arc yano #3620
                //削除済データの場合は数量・引当済数を初期化する
                if (!string.IsNullOrWhiteSpace(stockList[i].DelFlag) && stockList[i].DelFlag.Equals("1"))
                {
                    stockList[i].Quantity = 0;              //数量を初期化
                    stockList[i].ProvisionQuantity = 0;     //引当済数を初期化
                }

                //在庫の引当解除可能数を算出。
                decimal allowableQuantity = stockList[i].ProvisionQuantity ?? 0;

                //引当可能数 <= 0の場合は次の在庫ロケーションへ
                if (allowableQuantity <= 0)
                {
                    continue;
                }

                //---------------------------------------------------------
                ///増減数の決定
                // ①対象ロケーションの引当解除可能数が解除必要数より少ない場合
                //   →増減数 = 引当解除可能数
                // ②対象ロケーションの引当解除可能数が解除必要数より多い場合
                //   →増減数 = 解除必要数
                //---------------------------------------------------------
                incOrDecQuantity = (allowableQuantity - requireQuantity < 0) ? allowableQuantity : requireQuantity;

                //増減数で明細行、在庫情報の引当済数と必要数を更新する
                line.ProvisionQuantity -= incOrDecQuantity;
                stockList[i].ProvisionQuantity -= incOrDecQuantity; //在庫の引当済数は最後に行う
                requireQuantity -= incOrDecQuantity;

                //Add 2017/02/08 arc yano #3620
                if (!string.IsNullOrWhiteSpace(stockList[i].DelFlag) && stockList[i].DelFlag.Equals("1"))
                {
                    stockList[i].DelFlag = "0";     //在庫情報を有効化
                }

                PartsTransfer(stockList[i].LocationCode, stockList[i].LocationCode, line.PartsNumber, incOrDecQuantity, header.SlipNumber, TTYPE_CANCELPROVISION, updatePartsStock: false); //Add 2016/06/13 arc yano #3571


                //必要数全部引当済の場合は終了
                if (requireQuantity == 0)
                {
                    break;

                }
            }
        }

        /// <summary>
        /// 部品移動処理(移動伝票作成＆在庫更新)
        /// </summary>
        /// <param name="departureLocationCode">出発ロケーション</param>
        /// <param name="arrivalLocationCode">到着ロケーション</param>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="quantity">数量</param>
        /// <param name="transferType">移動種別</param>
        /// <param name="updatePartsStock">部品在庫更新フラグ(true:更新する false:更新しない)</param>
        /// <history>
        /// 2017/02/08 arc yano サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応
        /// 2016/06/13 arc yano #3571 メソッド名の変更と引数(updatePartsStock)の追加
        /// </history>
        public void PartsTransfer(string departureLocationCode, string arrivalLocationCode, string partsNumber, decimal quantity, string slipNumber, string transferType, bool updatePartsStock)
        {
            //在庫管理対象かつ数量が0より多い場合は移動伝票を作成する
            if (new PartsDao(db).IsInventoryParts(partsNumber) && quantity > 0) //Mod 2017/02/08 arc yano
            {
                Transfer transfer = new Transfer();
                transfer.ArrivalEmployeeCode = sessionEmployeeCode;
                transfer.ArrivalLocationCode = arrivalLocationCode;
                transfer.CarOrParts = TCLASS_PARTS;
                transfer.DepartureEmployeeCode = sessionEmployeeCode;
                transfer.DepartureLocationCode = departureLocationCode;
                transfer.PartsNumber = partsNumber;
                transfer.Quantity = quantity;
                transfer.SlipNumber = slipNumber;
                transfer.TransferType = transferType;

                //移動伝票登録＆在庫数更新
                new TransferDao(db).InsertTransfer(transfer, sessionEmployeeCode, updatePartsStock);
            }
        }

        /*
        /// <summary>
        /// 仕掛(解除)処理
        /// </summary>
        /// <param name="departureLocationCode">出発ロケーション</param>
        /// <param name="arrivalLocationCode">到着ロケーション</param>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="quantity">数量</param>
        /// <param name="transferType">移動種別</param>
        private void WorkProgress(string departureLocationCode, string arrivalLocationCode, string partsNumber, decimal quantity, string slipNumber, string transferType)
        {
           
            //在庫管理対象の部品のみ移動を行う
            if (new PartsDao(db).IsInventoryParts(partsNumber))
            {
                Transfer transfer = new Transfer();
                transfer.ArrivalEmployeeCode = sessionEmployeeCode;
                transfer.ArrivalLocationCode = arrivalLocationCode;
                transfer.CarOrParts = TCLASS_PARTS;
                transfer.DepartureEmployeeCode = sessionEmployeeCode;
                transfer.DepartureLocationCode = departureLocationCode;
                transfer.PartsNumber = partsNumber;
                transfer.Quantity = quantity;
                transfer.SlipNumber = slipNumber;
                transfer.TransferType = transferType;

                //移動伝票登録＆在庫数更新
                new TransferDao(db).InsertTransfer(transfer, sessionEmployeeCode, calcProvision: true);
            }
        }
        */

        //Mod 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更
        //                              作成した発注データをここでは登録しないように修正
        /// <summary>
        /// 発注データを作成する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="orderType">オーダー区分</param>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="quantity">発注数量</param>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="slipNumber">サービス伝票明細（１行分）</param>
        /// <param name="slipNumber">発注情報</param>
        /// <param name="parts">部品情報</param>
        private List<PartsPurchaseOrder> SetPurchaseOrder(string slipNumber, string orderType, string partsNumber, decimal orderQuantity, string departmentCode, List<PartsPurchaseOrder> list, Parts parts)
        {
            PartsPurchaseOrder order = new PartsPurchaseOrder();

            if (parts != null)
            {
                order.PurchaseOrderNumber = null;
                order.ServiceSlipNumber = slipNumber;                                                   //サービス伝票番号
                order.PartsNumber = partsNumber;                                                        //部品番号
                order.Quantity = orderQuantity;                                                         //発注数量
                order.Price = parts.SalesPrice;                                                         //価格
                order.Cost = parts.Cost;                                                                //原価
                order.Amount = parts.Cost * orderQuantity;                                              //金額                                                                         
                order.DepartmentCode = departmentCode;                                                  //部門コード
                order.OrderType = orderType;                                                            //判断
                order.PurchaseOrderStatus = "001";                                                      //発注ステータス=「未発注」
                order.CreateEmployeeCode = sessionEmployeeCode;                                         //作成者
                //order.CreateDate = DateTime.Now;                                                      //現在日時
                order.LastUpdateEmployeeCode = sessionEmployeeCode;                                     //最終更新者
                //order.LastUpdateDate = DateTime.Now;                                                  //最終更新時刻
                order.DelFlag = "0";                                                                    //削除フラグ
                order.GenuineType = parts.GenuineType;                                                  //純正区分
                list.Add(order);                                                                        //結果を追加
            }

            return list;
        }

        /// <summary>
        /// 仕掛移動　引当済部品を仕掛ロケーションへ移動する
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ情報</param>
        /// <param name="line">サービス伝票明細情報</param>
        /// <history>
        ///  2016/08/13 arc yano #3596 【大項目】部門棚統合対応 在庫リストの取得方法の変更(部門→ロケーションではなく、部門→倉庫→ロケーションで取得する)
        ///  2016/06/13 arc yano #3571 仕掛処理のメソッド名と引数を変更
        /// </history>
        private void StartWorking(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines, string shikakariLocationCode)
        {
            //必要数
            decimal requireQuantity = 0;
            //移動数量
            decimal transferQuantity = 0;
            
            //伝票に含まれる部品を移動
            foreach (ServiceSalesLine line in lines)
            {
                if (new PartsDao(db).IsInventoryParts(line.PartsNumber))
                {
                    //必要数の算出
                    requireQuantity = line.ProvisionQuantity ?? 0;

                    //サービス伝票明細行の引当済数分在庫ロケーションから仕掛ロケーションへ移動する
                    //移動元を特定する(※移動元は、対象の部品のロケーションかつ引当済数 > 0のもの)

                    //Mod 2016/08/13 arc yano #3596
                    //部門から倉庫を特定する
                    DepartmentWarehouse departmentWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);

                    //倉庫の在庫棚リストを取得する
                    List<PartsStock> stockList = new PartsStockDao(db).GetListByWarehouse(( departmentWarehouse != null ? departmentWarehouse.WarehouseCode : ""), line.PartsNumber).OrderByDescending(x => x.ProvisionQuantity).ToList();

                    for (int i = 0; i < stockList.Count(); i++)
                    {
                        //在庫の引当済数
                        decimal provisionQuantity = stockList[i].ProvisionQuantity ?? 0;

                        //在庫ロケーションの引当済数が必要数に満たない場合
                        transferQuantity = (provisionQuantity - requireQuantity < 0) ? provisionQuantity : requireQuantity; 
                        
                        //仕掛処理
                        PartsTransfer(stockList[i].LocationCode, shikakariLocationCode, line.PartsNumber, transferQuantity, header.SlipNumber, TTYPE_WORK, updatePartsStock: true); //Mod 2016/06/13 arc yano #3571
                        //必要数の更新
                        requireQuantity -= transferQuantity;

                        if (requireQuantity == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        //↓↓↓↓↓↓↓Del 2017/02/08 arc yano 未使用のため、削除
        //Mod 2016/08/13 arc yano 未使用のため、コメントアウト
        #endregion

        /// <summary>
        /// 車両マスタを更新する
        /// </summary>
        /// <param name="header">サービス伝票</param>
        private void UpdateSalesCar(ServiceSalesHeader header)
        {
            if (!string.IsNullOrEmpty(header.SalesCarNumber))
            {
                SalesCar targetSalesCar = new SalesCarDao(db).GetByKey(header.SalesCarNumber);
                if (targetSalesCar != null)
                {
                    //if (!string.IsNullOrEmpty(header.MorterViecleOfficialCode)) {
                    //    targetSalesCar.MorterViecleOfficialCode = header.MorterViecleOfficialCode;
                    //}
                    //if (!string.IsNullOrEmpty(header.RegistrationNumberType)) {
                    //    targetSalesCar.RegistrationNumberType = header.RegistrationNumberType;
                    //}
                    //if (!string.IsNullOrEmpty(header.RegistrationNumberKana)) {
                    //    targetSalesCar.RegistrationNumberKana = header.RegistrationNumberKana;
                    //}
                    //if (!string.IsNullOrEmpty(header.RegistrationNumberPlate)) {
                    //    targetSalesCar.RegistrationNumberPlate = header.RegistrationNumberPlate;
                    //}

                    //2016/04/08 arc nakayama #3197サービス伝票の車検有効期限＆次回点検日を車両マスタへコピーする際の確認メッセージ機能  次回点検日と車検有効期限は更新しないようにする
                    //if (header.NextInspectionDate != null)
                    //{
                    //    targetSalesCar.NextInspectionDate = header.NextInspectionDate;
                    //}
                    //if (header.InspectionExpireDate != null)
                    //{
                    //    targetSalesCar.ExpireType = "001";
                    //    targetSalesCar.ExpireDate = header.InspectionExpireDate;
                    //}
                    if (header.Mileage != null)
                    {
                        targetSalesCar.Mileage = header.Mileage;
                        targetSalesCar.MileageUnit = header.MileageUnit;
                    }
                    //if (!string.IsNullOrEmpty(header.FirstRegistration)) {
                    //    targetSalesCar.FirstRegistrationYear = header.FirstRegistration;
                    //}
                }
            }
        }

        /// <summary>
        /// 前回保存時からの差分をタスクに追加する
        /// （受注処理以降は必ず実行）
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        private void StockReserve(ServiceSalesHeader header)
        {
            EntitySet<ServiceSalesLine> line = header.ServiceSalesLine;

            TaskUtil task = new TaskUtil(db, sessionEmployee);

            // 部品の明細のみ抽出し、部品番号ごとに数量を集計
            var query = from a in line
                        where a.ServiceType.Equals("003")
                        group a by a.PartsNumber into prts
                        select new
                        {
                            PartsNumber = prts.Key,
                            Quantity = prts.Sum(x => x.Quantity)
                        };


            //前回との差分を取得
            foreach (var a in query)
            {
                //明細が部品のときだけ実行（値引以外）
                if (!IsDiscountRecord(a.PartsNumber))
                {
                    Dictionary<string, decimal> previous = new ServiceSalesOrderDao(db).GetPartsQuantityByPartsNumber(header.SlipNumber, header.RevisionNumber, a.PartsNumber);
                    decimal previousQuantity = previous["Quantity"];
                    decimal previousCount = previous["Count"];

                    //数量が0で件数が0の場合
                    if (previousCount == 0 && previousQuantity == 0)
                    {
                        //新しい部品が追加された
                        task.PartsChange(header, new PartsDao(db).GetByKey(a.PartsNumber), a.Quantity);
                    }
                    else
                    {
                        //追加された数量（今回の数量と前回の数量に差がある場合）
                        decimal? plusQuantity = (a.Quantity ?? 0) - previousQuantity;
                        if (plusQuantity != 0)
                        {
                            task.PartsChange(header, new PartsDao(db).GetByKey(a.PartsNumber), plusQuantity ?? 0);
                        }
                    }
                }
            }

            List<ServiceSalesLine> nonList = new ServiceSalesOrderDao(db).GetNotExistListByKey(header.SlipNumber, header.RevisionNumber, line);
            foreach (var b in nonList)
            {
                //削除された明細リスト
                task.PartsChange(header, new PartsDao(db).GetByKey(b.PartsNumber), 0);
            }

        }

        /// <summary>
        /// 黒伝票時の入金予定データ作成処理
        /// </summary>
        /// <param name="header">ヘッダ</param>
        /// <param name="lines">明細</param>
        /// <param name="payList">支払方法</param>
        /// <history>
        /// Mod 2016/04/05 arc yano #3441 カード入金消込時マイナスの入金予定ができてしまう 入金実績取得時は請求先種別 =「クレジット」「ローン」は除く
        /// </history>
        private void ResetReceiptPlan(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines, List<ServiceSalesPayment> payList)
        {

            Account serviceAccount = new AccountDao(db).GetByUsageType("SR");

            //既存の入金予定を削除
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "001");
            foreach (var d in delList)
            {
                d.DelFlag = "1";
            }

            //請求先一覧作成用
            List<string> customerClaimList = new List<string>();

            //**現金の入金予定を(再)作成する**
            //請求先、入金予定日の順で並び替え
            payList.Sort(delegate(ServiceSalesPayment x, ServiceSalesPayment y)
            {
                return
                    !x.CustomerClaimCode.Equals(y.CustomerClaimCode) ? x.CustomerClaimCode.CompareTo(y.CustomerClaimCode) : //請求先順
                    !x.PaymentPlanDate.Equals(y.PaymentPlanDate) ? DateTime.Compare(x.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MIN, y.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MAX) : //入金予定日順
                    (0);
            });

            decimal akaAmount = 0m;
            for (int i = 0; i < payList.Count; i++)
            {

                // 請求先リストに追加
                customerClaimList.Add(payList[i].CustomerClaimCode);

                // 初回or請求先が変わったとき、赤伝の入金予定金額を取得する
                if (i == 0 || (i > 0 && !payList[i - 1].CustomerClaimCode.Equals(payList[i].CustomerClaimCode)))
                {
                    akaAmount = new ReceiptPlanDao(db).GetAmountByCustomerClaim(header.SlipNumber.Replace("-2", "-1"), payList[i].CustomerClaimCode);
                }

                // 売掛残
                decimal balanceAmount = 0m;

                if (payList[i].Amount >= Math.Abs(akaAmount))
                {
                    // 予定額 >= 実績額
                    balanceAmount = ((payList[i].Amount ?? 0m) + akaAmount);
                    akaAmount = 0m;
                }
                else
                {
                    // 予定額 < 実績額
                    balanceAmount = 0m;
                    akaAmount = akaAmount + (payList[i].Amount ?? 0m);

                    // 次の請求先が異なる場合、ここでマイナスの売掛金を作成しておく
                    if (i == payList.Count() - 1 || (i < payList.Count() - 1 && !payList[i].CustomerClaimCode.Equals(payList[i + 1].CustomerClaimCode)))
                    {
                        balanceAmount = akaAmount;
                    }
                }

                InsertReceiptPlan(header, payList[i], balanceAmount, serviceAccount.AccountCode);
            }

            //Mod 2016/04/05 arc yano #3441
            //入金済みの請求先が今回の伝票からなくなっているものを通知
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, excludeList);
            foreach (Journal a in journalList)
            {
                if (!string.IsNullOrEmpty(a.CustomerClaimCode) && customerClaimList.IndexOf(a.CustomerClaimCode) < 0)
                {
                    TaskUtil task = new TaskUtil(db, sessionEmployee);
                    //task.CarOverReceive(header, a.CustomerClaimCode, a.Amount);

                    // マイナスで入金予定作成
                    ReceiptPlan plan = new ReceiptPlan();
                    plan.Amount = a.Amount * (-1m);
                    plan.ReceivableBalance = a.Amount * (-1m);
                    plan.ReceiptPlanId = Guid.NewGuid();
                    plan.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now));
                    plan.CreateEmployeeCode = sessionEmployeeCode;
                    plan.DelFlag = "0";
                    plan.CompleteFlag = "0";
                    plan.LastUpdateDate = DateTime.Now;
                    plan.LastUpdateEmployeeCode = sessionEmployeeCode;
                    plan.ReceiptType = "001"; // 現金
                    plan.SlipNumber = a.SlipNumber;
                    plan.OccurredDepartmentCode = a.DepartmentCode;
                    plan.AccountCode = a.AccountCode;
                    plan.CustomerClaimCode = a.CustomerClaimCode;
                    plan.DepartmentCode = a.DepartmentCode;
                    plan.ReceiptPlanDate = a.JournalDate;
                    plan.Summary = a.Summary;
                    plan.JournalDate = a.JournalDate;
                    db.ReceiptPlan.InsertOnSubmit(plan);
                }
            }

            //Add 2016/05/26 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法 赤伝票・元伝票の入金予定は削除しないようにする。
            // 赤伝の入金予定を削除
            /*
            List<ReceiptPlan> delPlan = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber.Replace("-2", ""), "001");
            foreach (var item in delPlan)
            {
                item.DelFlag = "1";
            }
            delPlan = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber.Replace("-2", "-1"), "001");
            foreach (var item in delPlan)
            {
                item.DelFlag = "1";
            }
            */
        }

        /// <summary>
        /// 税込金額から消費税額を取得
        /// </summary>
        /// <param name="AmountWithTax"></param>
        /// <returns></returns>
        private decimal GetTaxAmount(decimal? AmountWithTax, int? int_Rate)
        {
            //MOD 2014/02/20 ookubo
            return Math.Truncate((AmountWithTax ?? 0m) * (decimal)(int_Rate / (100 + int_Rate)));
            //return Math.Truncate((AmountWithTax ?? 0m) * 5 / 105);
        }
        /// <summary>
        /// 税込金額から税抜金額を取得
        /// </summary>
        /// <param name="AmountWithTax"></param>
        /// <returns></returns>
        //MOD 2014/02/20 ookubo
        private decimal GetAmountWithoutTax(decimal? AmountWithTax, int? int_Rate)
        {
            return (AmountWithTax ?? 0m) - GetTaxAmount(AmountWithTax ?? 0m, int_Rate);
            //return (AmountWithTax ?? 0m) - GetTaxAmount(AmountWithTax ?? 0m));
        }


        /// <summary>
        /// 値引レコードかどうかを取得する
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        //2014/03/19.ookubo #3006] 【サ】納車済みで数字が変わる対応 DISCNTにToupper追加
        public bool IsDiscountRecord(string code)
        {
            if (!string.IsNullOrEmpty(code) &&
                code.Length >= 6 &&
                code.Substring(0, 6).ToUpper() == "DISCNT")
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        //Add 2015/07/06 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑦ 在庫数量０の引当／仕掛部品のレコードは削除する
        /// <summary>
        /// 在庫数量が０のレコードを削除する
        /// </summary>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="locationCode">ロケーションコード</param>
        /// <returns></returns>
        public void DeletePartsStock (EntitySet<ServiceSalesLine> line, string locationCode)
        {
            foreach (var l in line)
            {
                //在庫情報取得
                PartsStock rec = new PartsStockDao(db).GetByKey(l.PartsNumber, locationCode);
                if (rec != null)
                {
                    //在庫数量が０の場合はレコード削除する
                    if (rec.Quantity == 0)
                    {
                        db.PartsStock.DeleteOnSubmit(rec);
                    }
                }
            }   
        }

        #endregion

        #region  権限のチェック
        //Add 2015/03/17 arc nakayama 伝票修正対応　特定の権限の人のみ[伝票修正]ボタンを表示させる
        //Mod 2015/05/26 arc nakayama 伝票修正_支店長対応　アプリケーションロールで権限をコントロールできるように対応    
        /// <summary>
        /// 権限のチェック
        /// </summary>
        /// <param name="code"></param>
        /// <returns>伝票修正許可権限をもっていれば:True  それ以外:False</returns>
        public bool CheckApplicationRole(string EmployeeCode)
        {
            //ログインユーザ情報取得
            Employee loginUser = new EmployeeDao(db).GetByKey(EmployeeCode);
            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "SlipModification"); //伝票修正許可権限があるかないか

            // 伝票修正許可権限があればtrueそうでなければfalse
            if (AppRole.EnableFlag)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region  伝票を修正中にする（レコード作成）
        //Add 2015/03/17 arc nakayama 伝票修正対応　伝票を修正中にする
        /// <summary>
        /// 伝票を修正中にする（レコード作成）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>なし</returns>
        public void ModificationStart(ServiceSalesHeader header)
        {
            ModificationControl Modification = new ModificationControl();
            Modification.SlipNumber = header.SlipNumber;
            Modification.RevisionNumber = header.RevisionNumber;
            Modification.SlipType = "1";
            Modification.SalesDate = header.SalesDate;
            Modification.CreateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
            Modification.CreateDate = DateTime.Now;
            Modification.LastUpdateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
            Modification.LastUpdateDate = DateTime.Now;
            Modification.DelFlag = "0";
            db.ModificationControl.InsertOnSubmit(Modification);
            db.SubmitChanges();

        }
        #endregion

        #region  伝票の修正をキャンセルする（修正を行えないようにする）
        //Add 2015/03/17 arc nakayama 伝票修正対応　伝票の修正をキャンセルする
        /// <summary>
        /// 伝票の修正をキャンセルする（修正を行えないようにする）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>なし</returns>
        public void ModificationCancel(ServiceSalesHeader header)
        {
            List<ModificationControl> ModifiRet = new ServiceSalesOrderDao(db).GetModificationStatusAll(header.SlipNumber);
            if (ModifiRet != null)
            {
                foreach (var ModRet in ModifiRet)
                {
                    db.ModificationControl.DeleteOnSubmit(ModRet);
                    
                }
                db.SubmitChanges();
            }
        }
        #endregion

        #region  修正中かどうかのチェック（修正中:True  それ以外:False）
        //Add 2015/03/17 arc nakayama 伝票修正対応　修正中かどうかそうでないかを返す
        /// <summary>
        /// 修正中かどうかのチェック（修正中:True  それ以外:False）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>修正中:True  それ以外:False</returns>
        public bool CheckModification(string SlipNumber, int RevisionNumber)
        {
            ModificationControl ModifiRet = new ServiceSalesOrderDao(db).GetModificationStatus(SlipNumber, RevisionNumber);

            if (ModifiRet != null){
                return true;
            }

            return false;
        }
        #endregion

        #region  過去に赤黒処理を行った元伝票でないかチェックする（赤黒経歴なし:True  それ以外:False）
        //Add 2015/03/17 arc nakayama 伝票修正対応　過去に赤黒処理を行った元伝票でないかチェックする
        /// <summary>
        /// 過去に赤黒処理を行った元伝票でないかチェックする（赤黒経歴なし:True  それ以外:False）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>赤黒経歴なし:True  赤黒経歴あり:False</returns>
        public bool AkakuroCheck(string SlipNumber)
        {
            AkakuroReason AkaKuroRec = new ServiceSalesOrderDao(db).GetAkakuroReason(SlipNumber);

            if(AkaKuroRec != null){
                return false;
            }
            return true;
            
        }
        #endregion

        #region 過去に修正処理を行った伝票でないかチェックする（修正履歴あり:True  修正履歴なし:False）
        //Add 2015/03/18 arc nakayama 伝票修正対応　過去に修正を行った伝票かどうかをチェックする
        /// <summary>
        /// 過去に修正処理を行った伝票でないかチェックする（修正履歴あり:True  修正履歴なし:False）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>修正履歴あり:True  修正履歴なし:False</returns>
        public bool CheckModifiedReason(string SlipNumber)
        {
            ModifiedReason ModifiedRec = new ServiceSalesOrderDao(db).GetLatestModifiedReason(SlipNumber);

            if (ModifiedRec != null)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region 修正履歴を取得する（該当伝票の全履歴）
        //Add 2015/03/18 arc nakayama 伝票修正対応　修正履歴を取得する
        /// <summary>
        /// 修正履歴を取得する（該当伝票の全履歴）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>修正履歴（修正時間・修正者・修正理由）</returns>
        /// <history>
        /// 2017/11/03 arc yano #3794 サービス伝票　削除された社員により修正されたサービス伝票を表示すると、システムエラー
        /// </history>
        public void GetModifiedHistory(ServiceSalesHeader header)
        {
            List<ModifiedReason> ModifiedRec = new ServiceSalesOrderDao(db).GetModifiedReason(header.SlipNumber);
            header.ModifiedReasonList = new List<ServiceModifiedReason>();

            if (ModifiedRec != null)
            {
                foreach (var Mod in ModifiedRec)
                {
                    ServiceModifiedReason ModData = new ServiceModifiedReason();
                    ModData.ModifiedTime = Mod.CreateDate;

                    //Mod 2017/11/03 arc yano #3794
                    Employee emp = new EmployeeDao(db).GetByKey(Mod.CreateEmployeeCode, true);

                    if (emp != null)
                    {
                        ModData.ModifiedEmployeeName = emp.EmployeeName;
                    }

                    ModData.ModifiedReason = Mod.Reason;
                    header.ModifiedReasonList.Add(ModData);
                }
            }
        }
        #endregion

        #region  修正履歴を作成する（レコード作成）
        //Add 2015/03/17 arc nakayama 伝票修正対応　修正履歴を作成する
        /// <summary>
        /// 修正履歴を作成する（レコード作成）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>なし</returns>
        public void CreateModifiedHistory(ServiceSalesHeader header)
        {
            ModifiedReason ModifiedHistory = new ModifiedReason();
            ModifiedHistory.SlipNumber = header.SlipNumber;
            ModifiedHistory.RevisionNumber = header.RevisionNumber;
            ModifiedHistory.SlipType = "1";
            ModifiedHistory.CreateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
            ModifiedHistory.CreateDate = DateTime.Now;
            ModifiedHistory.Reason = header.Reason;
            ModifiedHistory.DelFlag = "0";
            db.ModifiedReason.InsertOnSubmit(ModifiedHistory);
            db.SubmitChanges();
        }
        #endregion

        #region  対象年月が納車年月の修正中伝票情報を削除する
        //Add 2015/03/19 arc nakayama 伝票修正対応　経理締めが本締めになったら削除する（本締めしたのに伝票が修正されないようにするため）
        /// <summary>
        /// 対象年月が納車年月の伝票が修正中だった場合、伝票情報を削除する
        /// </summary>
        /// <param name="code">対象日付（YYYY/MM/DD）</param>
        /// <param name="code">部門コード</param>
        /// <returns>なし</returns>
        public void CloseEnd(DateTime targetMonth, string departmentCode)
        {

            //修正中の伝票情報を全件取得
            List<ModificationControl> ModifiRet = new ServiceSalesOrderDao(db).GetModificationStatusAll(null);
            DateTime ModSalesDate = DateTime.Today.Date;

            if (ModifiRet != null)
            {
                //その中で対象年月以前の日付が納車日になっている伝票をリスト化
                List<ModificationControl> NewModifiRet = new List<ModificationControl>();
                foreach (var ModRet in ModifiRet)
                {
                    //納車日が入っていたらリスト対象（車両伝票は納車前も修正対象になるため、納車日が入っていない場合がある）
                    if (ModRet.SalesDate != null)
                    {
                        ModSalesDate = DateTime.Parse(ModRet.SalesDate.Value.Year + "/" + ModRet.SalesDate.Value.Month + "/01");
                        ModificationControl Nmr = new ModificationControl();
                        if (ModSalesDate <= targetMonth)
                        {
                            Nmr.SlipNumber = ModRet.SlipNumber;
                            Nmr.RevisionNumber = ModRet.RevisionNumber;
                            NewModifiRet.Add(Nmr);
                        }
                    }
                }
                foreach (var TSlip in NewModifiRet)
                {
                    //削除する
                    ModificationControl ModSlip = new ServiceSalesOrderDao(db).GetModificationStatus(TSlip.SlipNumber, TSlip.RevisionNumber);
                    db.ModificationControl.DeleteOnSubmit(ModSlip);
                    db.SubmitChanges();
                }
            }
        }
        #endregion

        #region 引き継ぎメモと納車日を更新する
        //Add 2015/08/05 arc nakayama #3221_無効となっている部門や車両等が設定されている納車確認書が印刷出来ない
        /// <summary>
        /// 引き継ぎメモだけを更新する
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        /// <returns>なし</returns>
        public void UpDateMemoServiceSalesHeader(ServiceSalesHeader header)
        {
            ServiceSalesHeader ServiceHeader = new ServiceSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            ServiceHeader.SalesDate = header.SalesDate;
            ServiceHeader.Memo = header.Memo;
            ServiceHeader.LastUpdateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
            ServiceHeader.LastUpdateDate = DateTime.Now;

        }
        #endregion

        /// <summary>
        /// サービス伝票の引当の履歴より在庫情報を取得する
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="partsNumber">部品番号</param>
        /// <returns></returns>
        /// <history>
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 新規作成
        /// </history>
        public List<PartsStock> GetStockListByTransfer(ServiceSalesHeader header, string partsNumber)
        {
            //伝票番号から移動履歴を取得する。
            List<Transfer> tranList = new TransferDao(db).GetListBySlipNumber(header.SlipNumber);

            List<PartsStock> stockList = new List<PartsStock>();

            //伝票ヘッダの現在の部門コードから倉庫コードを割出す
            DepartmentWarehouse dWarehouse = new DepartmentWarehouseDao(db).GetByDepartment(header.DepartmentCode, false);

            string warehouseCode = dWarehouse != null ? dWarehouse.WarehouseCode : "";

            //本伝票の移動履歴(自動引当)を元に在庫情報を取得する(但し、部門コードが変更されてることも考慮し今入力されている部門の倉庫のロケーションで絞り込む)
            foreach (var tran in tranList.Where(x => x.PartsNumber.Equals(partsNumber) && x.TransferType.Equals("006") && x.DepartureLocation.WarehouseCode.Equals(warehouseCode)).OrderByDescending(x => x.Quantity))
            {
                PartsStock rec = new PartsStockDao(db).GetByKey(partsNumber, tran.DepartureLocationCode, false);

                if (rec != null)
                {
                    stockList.Add(rec);
                }
                
            }

            return stockList;

        }

        /// <summary>
        /// 既存のサービス伝票を複製して新規のサービス伝票を作成
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <returns></returns>
        /// <history>
        /// 2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
        /// 2018/05/30 arc yano #3889 サービス伝票発注部品が引当されない
        /// </history>
        private void CopyServiceSalesHeader(ServiceSalesHeader header, ref ServiceSalesHeader newHeader, ref EntitySet<ServiceSalesLine> newLines)
        {
            //--------------------
            //ヘッダ部の複製
            //--------------------
            newHeader.SlipNumber = header.SlipNumber;
            newHeader.RevisionNumber = header.RevisionNumber;
            newHeader.CarSlipNumber = header.CarSlipNumber;
            newHeader.CarSalesOrderDate = header.CarSalesOrderDate;
            newHeader.QuoteDate = header.QuoteDate;
            newHeader.QuoteExpireDate = header.QuoteExpireDate;
            newHeader.SalesOrderDate = header.SalesOrderDate;
            newHeader.ServiceOrderStatus = header.ServiceOrderStatus;
            newHeader.ArrivalPlanDate = header.ArrivalPlanDate;
            newHeader.ApprovalFlag = header.ApprovalFlag;
            newHeader.CampaignCode1 = header.CampaignCode1;
            newHeader.CampaignCode2 = header.CampaignCode2;
            newHeader.WorkingStartDate = header.WorkingStartDate;
            newHeader.WorkingEndDate = header.WorkingEndDate;
            newHeader.SalesDate = header.SalesDate;
            newHeader.CustomerCode = header.CustomerCode;
            newHeader.DepartmentCode = header.DepartmentCode;
            newHeader.CarEmployeeCode = header.CarEmployeeCode;
            newHeader.FrontEmployeeCode = header.FrontEmployeeCode;
            newHeader.ReceiptionEmployeeCode = header.ReceiptionEmployeeCode;
            newHeader.CarGradeCode = header.CarGradeCode;
            newHeader.CarBrandName = header.CarBrandName;
            newHeader.CarName = header.CarName;
            newHeader.CarGradeName = header.CarGradeName;
            newHeader.EngineType = header.EngineType;
            newHeader.ManufacturingYear = header.ManufacturingYear;
            newHeader.Vin = header.Vin;
            newHeader.ModelName = header.ModelName;
            newHeader.Mileage = header.Mileage;
            newHeader.MileageUnit = header.MileageUnit;
            newHeader.SalesPlanDate = header.SalesPlanDate;
            newHeader.FirstRegistration = header.FirstRegistration;
            newHeader.NextInspectionDate = header.NextInspectionDate;
            newHeader.MorterViecleOfficialCode = header.MorterViecleOfficialCode;
            newHeader.RegistrationNumberType = header.RegistrationNumberType;
            newHeader.RegistrationNumberKana = header.RegistrationNumberKana;
            newHeader.RegistrationNumberPlate = header.RegistrationNumberPlate;
            newHeader.MakerShipmentDate = header.MakerShipmentDate;
            newHeader.RegistrationPlanDate = header.RegistrationPlanDate;
            newHeader.RequestContent = header.RequestContent;
            newHeader.CarTax = header.CarTax;
            newHeader.CarLiabilityInsurance = header.CarLiabilityInsurance;
            newHeader.CarWeightTax = header.CarWeightTax;
            newHeader.FiscalStampCost = header.FiscalStampCost;
            newHeader.InspectionRegistCost = header.InspectionRegistCost;
            newHeader.ParkingSpaceCost = header.ParkingSpaceCost;
            newHeader.TradeInCost = header.TradeInCost;
            newHeader.ReplacementFee = header.ReplacementFee;
            newHeader.InspectionRegistFee = header.InspectionRegistFee;
            newHeader.ParkingSpaceFee = header.ParkingSpaceFee;
            newHeader.TradeInFee = header.TradeInFee;
            newHeader.PreparationFee = header.PreparationFee;
            newHeader.RecycleControlFee = header.RecycleControlFee;
            newHeader.RecycleControlFeeTradeIn = header.RecycleControlFeeTradeIn;
            newHeader.RequestNumberFee = header.RequestNumberFee;
            newHeader.CarTaxUnexpiredAmount = header.CarTaxUnexpiredAmount;
            newHeader.CarLiabilityInsuranceUnexpiredAmount = header.CarLiabilityInsuranceUnexpiredAmount;
            newHeader.LaborRate = header.LaborRate;
            newHeader.Memo = header.Memo;
            newHeader.EngineerTotalAmount = header.EngineerTotalAmount;
            newHeader.PartsTotalAmount = header.PartsTotalAmount;
            newHeader.SubTotalAmount = header.SubTotalAmount;
            newHeader.TotalTaxAmount = header.TotalTaxAmount;
            newHeader.ServiceTotalAmount = header.ServiceTotalAmount;
            newHeader.CostTotalAmount = header.CostTotalAmount;
            newHeader.GrandTotalAmount = header.GrandTotalAmount;
            newHeader.PaymentTotalAmount = header.PaymentTotalAmount;
            newHeader.CreateEmployeeCode = header.CreateEmployeeCode;
            newHeader.CreateDate = header.CreateDate;
            newHeader.LastUpdateEmployeeCode = header.LastUpdateEmployeeCode;
            newHeader.LastUpdateDate = header.LastUpdateDate;
            newHeader.DelFlag = header.DelFlag;
            newHeader.SalesCarNumber = header.SalesCarNumber;
            newHeader.InspectionExpireDate = header.InspectionExpireDate;
            newHeader.NumberPlateCost = header.NumberPlateCost;
            newHeader.TaxFreeFieldName = header.TaxFreeFieldName;
            newHeader.TaxFreeFieldValue = header.TaxFreeFieldValue;
            newHeader.UsVin = header.UsVin;
            newHeader.ProcessSessionId = header.ProcessSessionId;
            newHeader.ConsumptionTaxId = header.ConsumptionTaxId;
            newHeader.Rate = header.Rate;
            newHeader.KeepsCarFlag = header.KeepsCarFlag;

            //Add 2020/02/17 yano #4025------------------------------------------------------
            newHeader.OptionalInsurance = header.OptionalInsurance;
            newHeader.CarTaxMemo = header.CarTaxMemo;
            newHeader.CarLiabilityInsuranceMemo = header.CarLiabilityInsuranceMemo;
            newHeader.CarWeightTaxMemo = header.CarWeightTaxMemo;
            newHeader.NumberPlateCostMemo = header.NumberPlateCostMemo;
            newHeader.FiscalStampCostMemo = header.FiscalStampCostMemo;
            newHeader.OptionalInsuranceMemo = header.OptionalInsuranceMemo;
            newHeader.SubscriptionFee = header.SubscriptionFee;
            newHeader.SubscriptionFeeMemo = header.SubscriptionFeeMemo;
            newHeader.TaxableCostTotalAmount = header.TaxableCostTotalAmount;
            newHeader.TaxableFreeFieldValue = header.TaxableFreeFieldValue;
            newHeader.TaxableFreeFieldName = header.TaxableFreeFieldName;
            //Add 2023/05/01 openwave #xxxx
            newHeader.CustomerClaimCode = header.CustomerClaimCode;
            //-------------------------------------------------------------------------------
            //--------------------
            //明細部の退避
            //--------------------
            foreach (var l in header.ServiceSalesLine)
            {
                ServiceSalesLine line = new ServiceSalesLine();

                line.SlipNumber = l.SlipNumber;
                line.RevisionNumber = l.RevisionNumber;
                line.LineNumber = l.LineNumber;
                line.ServiceType = l.ServiceType;
                line.SetMenuCode = l.SetMenuCode;
                line.ServiceWorkCode = l.ServiceWorkCode;
                line.ServiceMenuCode = l.ServiceMenuCode;
                line.PartsNumber = l.PartsNumber;
                line.LineContents = l.LineContents;
                line.RequestComment = l.RequestComment;
                line.WorkType = l.WorkType;
                line.LaborRate = l.LaborRate;
                line.ManPower = l.ManPower;
                line.TechnicalFeeAmount = l.TechnicalFeeAmount;
                line.Quantity = l.Quantity;
                line.Price = l.Price;
                line.Amount = l.Amount;
                line.Cost = l.Cost;
                line.EmployeeCode = l.EmployeeCode;
                line.SupplierCode = l.SupplierCode;
                line.CustomerClaimCode = l.CustomerClaimCode;
                line.StockStatus = l.StockStatus;
                line.CreateEmployeeCode = l.CreateEmployeeCode;
                line.CreateDate = l.CreateDate;
                line.LastUpdateEmployeeCode = l.LastUpdateEmployeeCode;
                line.LastUpdateDate = l.LastUpdateDate;
                line.DelFlag = l.DelFlag;
                line.Classification1 = l.Classification1;
                line.TaxAmount = l.TaxAmount;
                line.UnitCost = l.UnitCost;
                line.LineType = l.LineType;
                line.ConsumptionTaxId = l.ConsumptionTaxId;
                line.Rate = l.Rate;
                line.ProvisionQuantity = l.ProvisionQuantity;
                line.OrderQuantity = l.OrderQuantity;
                line.DisplayOrder = l.DisplayOrder;
                line.OutputTargetFlag = l.OutputTargetFlag;
                line.OutputFlag = l.OutputFlag;

                //db.ServiceSalesLine.InsertOnSubmit(line);
                newLines.Add(line);
            }
        }
    }
}
