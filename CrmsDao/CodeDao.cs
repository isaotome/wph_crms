using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// コード各種マスタマスタアクセスクラス
    ///   コード各種マスタマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CodeDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CodeDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        /// <summary>
        /// 口座種別コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>口座種別コードリスト</returns>
        public List<c_AccountType> GetAccountTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_AccountType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_AccountType>();
        }

        /// <summary>
        /// 口座種別コードリスト取得　権限別に取得する（経理権限の有無）
        /// </summary>
        /// <param name="ListType">リストタイプ（001：入金管理画面用 002：店舗入金消込画面用 003：店舗入金消込（個別）画面用）</param>
        /// <returns>口座種別コードリスト</returns>
        /// <history>Add 2016/05/16 arc nakayama #3544_入金種別をカード・ローンには変更させない 権限の有無ととダイアログで表示の有無を条件に入れたメソッド追加</history>
        public List<c_AccountType> GetAccountType(string ListType)
        {
            var query = db.GetAccountTypeList(ListType);
            return query.ToList<c_AccountType>();
        }

        /// <summary>
        /// 取得原因コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>取得原因コードリスト</returns>
        public List<c_AcquisitionReason> GetAcquisitionReasonAll(bool includeDeleted) {
            var query =
                from a in db.c_AcquisitionReason
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_AcquisitionReason>();
        }

        /// <summary>
        /// 可否コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>可否コードリスト</returns>
        public List<c_Allowance> GetAllowanceAll(bool includeDeleted) {
            var query =
                from a in db.c_Allowance
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Allowance>();
        }

        /// ADD #3069  2014/08/22 ookubo
        /// <summary>
        /// 要否コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>可否コードリスト</returns>
        public List<c_Needed> GetNeededAll(bool includeDeleted)
        {
            var query =
                from a in db.c_Needed
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Needed>();
        }

        /// <summary>
        /// ブランドの魅力コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>ブランドの魅力コードリスト</returns>
        public List<c_AttractivePoint> GetAttractivePointAll(bool includeDeleted) {
            var query =
                from a in db.c_AttractivePoint
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_AttractivePoint>();
        }

        /// <summary>
        /// イベントタイプコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>イベントタイプコードリスト</returns>
        public List<c_CampaignType> GetCampaignTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_CampaignType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CampaignType>();
        }

        /// <summary>
        /// 自動車種別コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>自動車種別コードリスト</returns>
        public List<c_CarClassification> GetCarClassificationAll(bool includeDeleted) {
            var query =
                from a in db.c_CarClassification
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CarClassification>();
        }

        /// <summary>
        /// 車の所有コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>車の所有コードリスト</returns>
        public List<c_CarOwner> GetCarOwnerAll(bool includeDeleted) {
            var query =
                from a in db.c_CarOwner
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CarOwner>();
        }

        /// <summary>
        /// 在庫ステータスコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>在庫ステータスコードリスト</returns>
        public List<c_CarStatus> GetCarStatusAll(bool includeDeleted) {
            var query =
                from a in db.c_CarStatus
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CarStatus>();
        }

        /// <summary>
        /// 在庫ステータスを1件取得する(PK指定)
        /// </summary>
        /// <param name="code">コード</param>
        /// <returns></returns>
        public c_CarStatus GetCarStatusByKey(string code) {
            var query =
                (from a in db.c_CarStatus
                 where a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 系統色コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>系統色コードリスト</returns>
        public List<c_ColorCategory> GetColorCategoryAll(bool includeDeleted) {
            var query =
                from a in db.c_ColorCategory
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_ColorCategory>();
        }

        /// <summary>
        /// 請求種別コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <param name="customerClaimFilter">請求先種別フィルタ</param>
        /// <returns>請求種別コードリスト</returns>
        /// <history>
        /// 2016/05/18 arc yano #3558 入金管理 請求先タイプの絞込み追加
        /// </history>
        public List<c_CustomerClaimType> GetCustomerClaimTypeAll(bool includeDeleted, string customerClaimFilter = null ) {
            var query =
                from a in db.c_CustomerClaimType
                where ((includeDeleted) || a.DelFlag.Equals("0"))
                && (string.IsNullOrWhiteSpace(customerClaimFilter) || customerClaimFilter.Equals(a.CustomerClaimFilter))
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CustomerClaimType>();
        }

        /// <summary>
        /// 顧客種別コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>顧客種別コードリスト</returns>
        public List<c_CustomerKind> GetCustomerKindAll(bool includeDeleted) {
            var query =
                from a in db.c_CustomerKind
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CustomerKind>();
        }

        /// <summary>
        /// 顧客ランクコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>顧客ランクコードリスト</returns>
        public List<c_CustomerRank> GetCustomerRankAll(bool includeDeleted) {
            var query =
                from a in db.c_CustomerRank
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CustomerRank>();
        }

        /// <summary>
        /// 顧客区分コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>顧客区分コードリスト</returns>
        public List<c_CustomerType> GetCustomerTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_CustomerType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CustomerType>();
        }

        /// <summary>
        /// 申告区分コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>申告区分コードリスト</returns>
        public List<c_DeclarationType> GetDeclarationTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_DeclarationType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_DeclarationType>();
        }

        /// <summary>
        /// 今後の展望コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>今後の展望コードリスト</returns>
        public List<c_Demand> GetDemandAll(bool includeDeleted) {
            var query =
                from a in db.c_Demand
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Demand>();
        }

        /// <summary>
        /// 預金種目コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>預金種目コードリスト</returns>
        public List<c_DepositKind> GetDepositKindAll(bool includeDeleted) {
            var query =
                from a in db.c_DepositKind
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_DepositKind>();
        }

        /// <summary>
        /// 書類完備コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>書類完備コードリスト</returns>
        public List<c_DocumentComplete> GetDocumentCompleteAll(bool includeDeleted) {
            var query =
                from a in db.c_DocumentComplete
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_DocumentComplete>();
        }

        /// <summary>
        /// 駆動名称コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>駆動名称コードリスト</returns>
        public List<c_DrivingName> GetDrivingNameAll(bool includeDeleted) {
            var query =
                from a in db.c_DrivingName
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_DrivingName>();
        }

        /// <summary>
        /// 雇用種別コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>雇用種別コードリスト</returns>
        public List<c_EmployeeType> GetEmployeeTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_EmployeeType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_EmployeeType>();
        }

        /// <summary>
        /// 有効期限種別コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>有効期限種別コードリスト</returns>
        public List<c_ExpireType> GetExpireTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_ExpireType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_ExpireType>();
        }

        /// <summary>
        /// 形状コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>形状コードリスト</returns>
        public List<c_Figure> GetFigureAll(bool includeDeleted) {
            var query =
                from a in db.c_Figure
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Figure>();
        }

        /// <summary>
        /// 純正区分コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>純正区分コードリスト</returns>
        public List<c_GenuineType> GetGenuineTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_GenuineType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_GenuineType>();
        }

        /// <summary>
        /// 輸入コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>輸入コードリスト</returns>
        public List<c_Import> GetImportAll(bool includeDeleted) {
            var query =
                from a in db.c_Import
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Import>();
        }

        /// <summary>
        /// 棚卸ステータスコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>棚卸ステータスコードリスト</returns>
        public List<c_InventoryStatus> GetInventoryStatusAll(bool includeDeleted) {
            var query =
                from a in db.c_InventoryStatus
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_InventoryStatus>();
        }

        /// <summary>
        /// 棚卸種別コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>棚卸種別コードリスト</returns>
        public List<c_InventoryType> GetInventoryTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_InventoryType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_InventoryType>();
        }

        /// <summary>
        /// 入出金区分コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>入出金区分コードリスト</returns>
        public List<c_JournalType> GetJournalTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_JournalType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_JournalType>();
        }

        /// <summary>
        /// ショールームを知ったきっかけコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>ショールームを知ったきっかけコードリスト</returns>
        public List<c_KnowOpportunity> GetKnowOpportunityAll(bool includeDeleted) {
            var query =
                from a in db.c_KnowOpportunity
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_KnowOpportunity>();
        }

        /// <summary>
        /// ライトコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>ライトコードリスト</returns>
        public List<c_Light> GetLightAll(bool includeDeleted) {
            var query =
                from a in db.c_Light
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Light>();
        }

        /// <summary>
        /// 走行距離単位コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>走行距離単位コードリスト</returns>
        public List<c_MileageUnit> GetMileageUnitAll(bool includeDeleted) {
            var query =
                from a in db.c_MileageUnit
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_MileageUnit>();
        }

        //Add 2015/04/02 arc nakayama バグ修正　走行距離の単位が表示されないタイミングがある
        /// <summary>
        /// 走行距離単位コード取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>走行距離単位コードリスト</returns>
        public c_MileageUnit GetMileageUnit(string code) {
            c_MileageUnit query =
                (from a in db.c_MileageUnit
                where (string.IsNullOrEmpty(code) || code.Equals(a.Code))
                select a).FirstOrDefault();

            return query;
        }

        /// <summary>
        /// ナビ設置箇所コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>ナビ設置箇所コードリスト</returns>
        public List<c_NaviDashboard> GetNaviDashboardAll(bool includeDeleted) {
            var query =
                from a in db.c_NaviDashboard
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_NaviDashboard>();
        }

        /// <summary>
        /// ナビ媒体コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>ナビ媒体コードリスト</returns>
        public List<c_NaviEquipment> GetNaviEquipmentAll(bool includeDeleted) {
            var query =
                from a in db.c_NaviEquipment
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_NaviEquipment>();
        }

        /// <summary>
        /// 新中区分コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>新中区分コードリスト</returns>
        public List<c_NewUsedType> GetNewUsedTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_NewUsedType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_NewUsedType>();
        }

        /// <summary>
        /// 職業コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>職業コードリスト</returns>
        public List<c_Occupation> GetOccupationAll(bool includeDeleted) {
            var query =
                from a in db.c_Occupation
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Occupation>();
        }

        /// <summary>
        /// 有無コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>有無コードリスト</returns>
        public List<c_OnOff> GetOnOffAll(bool includeDeleted) {
            var query =
                from a in db.c_OnOff
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_OnOff>();
        }

        /// <summary>
        /// 支払方法コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>支払方法コードリスト</returns>
        public List<c_PaymentKind> GetPaymentKindAll(bool includeDeleted) {
            var query =
                from a in db.c_PaymentKind
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_PaymentKind>();
        }

        /// <summary>
        /// 支払区分コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>支払区分コードリスト</returns>
        public List<c_PaymentType> GetPaymentTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_PaymentType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_PaymentType>();
        }

        /// <summary>
        /// 支払区分(日数とセット)コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>支払区分(日数とセット)コードリスト</returns>
        public List<c_PaymentType2> GetPaymentType2All(bool includeDeleted) {
            var query =
                from a in db.c_PaymentType2
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_PaymentType2>();
        }

        /// <summary>
        /// 仕入ステータスコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>来店目的コードリスト</returns>
        public List<c_PurchaseStatus> GetPurchaseStatusAll(bool includeDeleted) {
            var query =
                from a in db.c_PurchaseStatus
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_PurchaseStatus>();
        }

        /// <summary>
        /// 来店目的コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>来店目的コードリスト</returns>
        public List<c_Purpose> GetPurposeAll(bool includeDeleted) {
            var query =
                from a in db.c_Purpose
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Purpose>();
        }

        /// <summary>
        /// 見積種別コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>支払区分コードリスト</returns>
        public List<c_QuoteType> GetQuoteTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_QuoteType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_QuoteType>();
        }

        /// <summary>
        /// 受付状況コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>受付状況コードリスト</returns>
        public List<c_ReceiptionState> GetReceiptionStateAll(bool includeDeleted) {
            var query =
                from a in db.c_ReceiptionState
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_ReceiptionState>();
        }

        /// <summary>
        /// 来店種別コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>来店種別コードリスト</returns>
        public List<c_ReceiptionType> GetReceiptionTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_ReceiptionType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_ReceiptionType>();
        }

        /// <summary>
        /// リサイクルコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>リサイクルコードリスト</returns>
        public List<c_Recycle> GetRecycleAll(bool includeDeleted) {
            var query =
                from a in db.c_Recycle
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Recycle>();
        }

        /// <summary>
        /// 依頼事項コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>依頼事項コードリスト</returns>
        public List<c_RequestContent> GetRequestContentAll(bool includeDeleted) {
            var query =
                from a in db.c_RequestContent
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_RequestContent>();
        }

        /// <summary>
        /// 小数点以下の端数処理コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>小数点以下の端数処理コードリスト</returns>
        public List<c_RoundType> GetRoundTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_RoundType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_RoundType>();
        }

        /// <summary>
        /// シートコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>シートコードリスト</returns>
        public List<c_SeatType> GetSeatTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_SeatType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_SeatType>();
        }

        /// <summary>
        /// 主作業大分類コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>主作業大分類コードリスト</returns>
        public List<c_ServiceWorkClass1> GetServiceWorkClass1All(bool includeDeleted) {
            var query =
                from a in db.c_ServiceWorkClass1
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_ServiceWorkClass1>();
        }

        /// <summary>
        /// 主作業中分類コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>主作業中分類コードリスト</returns>
        public List<c_ServiceWorkClass2> GetServiceWorkClass2All(bool includeDeleted) {
            var query =
                from a in db.c_ServiceWorkClass2
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_ServiceWorkClass2>();
        }

        /// <summary>
        /// 性別コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>性別コードリスト</returns>
        public List<c_Sex> GetSexAll(bool includeDeleted) {
            var query =
                from a in db.c_Sex
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Sex>();
        }

        /// <summary>
        /// SRコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>SRコードリスト</returns>
        public List<c_Sr> GetSrAll(bool includeDeleted) {
            var query =
                from a in db.c_Sr
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Sr>();
        }

        /// <summary>
        /// ハンドルコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>ハンドルコードリスト</returns>
        public List<c_Steering> GetSteeringAll(bool includeDeleted) {
            var query =
                from a in db.c_Steering
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Steering>();
        }

        /// <summary>
        /// 支払先種別コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>支払先種別コードリスト</returns>
        public List<c_SupplierPaymentType> GetSupplierPaymentTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_SupplierPaymentType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_SupplierPaymentType>();
        }

        /// <summary>
        /// 対象業務コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>対象業務コードリスト</returns>
        public List<c_TargetService> GetTargetServiceAll(bool includeDeleted) {
            var query =
                from a in db.c_TargetService
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_TargetService>();
        }

        /// <summary>
        /// 課税区分コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>課税区分コードリスト</returns>
        public List<c_TaxationType> GetTaxationTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_TaxationType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_TaxationType>();
        }

        /// <summary>
        /// MTコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>MTコードリスト</returns>
        public List<c_TransMission> GetTransMissionAll(bool includeDeleted) {
            var query =
                from a in db.c_TransMission
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_TransMission>();
        }

        /// <summary>
        /// 用途区分コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>用途区分コードリスト</returns>
        public List<c_Usage> GetUsageAll(bool includeDeleted) {
            var query =
                from a in db.c_Usage
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Usage>();
        }

        /// <summary>
        /// 事自区分コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>事自区分コードリスト</returns>
        public List<c_UsageType> GetUsageTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_UsageType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_UsageType>();
        }

        /// <summary>
        /// 来店きっかけコードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>来店きっかけコードリスト</returns>
        public List<c_VisitOpportunity> GetVisitOpportunityAll(bool includeDeleted) {
            var query =
                from a in db.c_VisitOpportunity
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_VisitOpportunity>();
        }

        /// <summary>
        /// 伝票ステータスリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>伝票ステータスリスト</returns>
        public List<c_SalesOrderStatus> GetSalesOrderStatusAll(bool includeDeleted) {
            var query =
                from a in db.c_SalesOrderStatus
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_SalesOrderStatus>();
        }

        /// <summary>
        /// 販売区分リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>販売区分データリスト</returns>
        public List<c_SalesType> GetSalesTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_SalesType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_SalesType>();
        }
        /// <summary>
        /// 登録種別リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>登録種別データリスト</returns>
        public List<c_RegistrationType> GetRegistrationTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_RegistrationType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_RegistrationType>();
        }
        /// <summary>
        /// オプション種別リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>オプション種別データリスト</returns>
        public List<c_OptionType> GetOptionTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_OptionType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_OptionType>();
        }
        /// <summary>
        /// 自賠責保険加入区分リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>自賠責保険加入区分データリスト</returns>
        public List<c_CarLiabilityInsuranceType> GetCarLiabilityInsuranceTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_CarLiabilityInsuranceType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CarLiabilityInsuranceType>();
        }
        /// <summary>
        /// 所有権留保区分リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>所有権留保区分データリスト</returns>
        public List<c_OwnershipReservation> GetOwnershipReservationAll(bool includeDeleted) {
            var query =
                from a in db.c_OwnershipReservation
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_OwnershipReservation>();
        }
        /// <summary>
        /// 任意保険加入区分リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>任意保険加入区分データリスト</returns>
        public List<c_VoluntaryInsuranceType> GetVoluntaryInsuranceTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_VoluntaryInsuranceType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_VoluntaryInsuranceType>();
        }
        /// <summary>
        /// セキュリティレベルリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>セキュリティレベルデータリスト</returns>
        public List<c_SecurityLevel> GetSecurityLevelAll(bool includeDeleted) {
            var query =
                from a in db.c_SecurityLevel
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_SecurityLevel>();
        }

        /// <summary>
        /// 作業区分リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>作業区分データリスト</returns>
        /// <history>
        ///  2018/02/22 arc yano #3471 サービス伝票　区分の絞込みの対応  サービスメニュー絞込み 引数(serviceType)の削除
        ///  2016/03/17 arc yano #3471 サービス伝票　区分の絞込みの対応　種別による区分の絞込みを追加　→2016/04/19 arc yano 不具合発生のため、一旦削除
        /// </history>
        public List<c_WorkType> GetWorkTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_WorkType
                where 
                ((includeDeleted) || a.DelFlag.Equals("0"))
                orderby a.DisplayOrder
                select a;

            return query.ToList<c_WorkType>();
        }

        /// <summary>
        /// 作業種別リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>作業種別データリスト</returns>
        public List<c_ServiceType> GetServiceTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_ServiceType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_ServiceType>();
        }

        /// <summary>
        /// 作業種別データを取得する（PK指定）
        /// </summary>
        /// <param name="code">作業種別コード</param>
        /// <returns>作業種別データ</returns>
        public c_ServiceType GetServiceTypeByKey(string code) {
            var query =
                (from a in db.c_ServiceType
                 where a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }


        /// <summary>
        /// 在庫判断リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>在庫判断データリスト</returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 新規作成
        /// </history>
        public c_StockStatus GetStockStatus(bool includeDeleted, string code, string statusType)
        {
            var query =
                from a in db.c_StockStatus
                where ((includeDeleted) || a.DelFlag.Equals("0"))
                && a.Code.Equals(code)
                && (string.IsNullOrWhiteSpace(statusType) || a.StatusType.Equals(statusType))
                orderby a.DisplayOrder
                select a;
            return query.FirstOrDefault();
        }
        
        // Mod 2015/10/28 arc yano #3289 サービス伝票 純正品、社外品により、取得するリストを変える
        /// <summary>
        /// 在庫判断リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>在庫判断データリスト</returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 純正品or社外品の分類の削除
        /// </history>
        public List<c_StockStatus> GetStockStatusAll(bool includeDeleted)
        {
            var query =
                from a in db.c_StockStatus
                where ((includeDeleted) || a.DelFlag.Equals("0"))
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_StockStatus>();
        }
        /*
        public List<c_StockStatus> GetStockStatusAll(bool includeDeleted)
        {
            var query =
                from a in db.c_StockStatus
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_StockStatus>();
        }
        */
        /// <summary>
        /// サービス伝票ステータス判断リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>サービス伝票ステータスデータリスト</returns>
        public List<c_ServiceOrderStatus> GetServiceOrderStatusAll(bool includeDeleted) {
            var query =
                from a in db.c_ServiceOrderStatus
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_ServiceOrderStatus>();
        }

        /// <summary>
        /// 保適区分リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>保適区分リスト</returns>
        public List<c_OwnershipChange> GetOwnershipChangeAll(bool includeDeleted) {
            var query =
                from a in db.c_OwnershipChange
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_OwnershipChange>();
        }

        /// <summary>
        /// ロケーション種別リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>ロケーション種別リスト</returns>
        public List<c_LocationType> GetLocationTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_LocationType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_LocationType>();
        }
        
        /// <summary>
        /// 移動種別リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>移動種別リスト</returns>
        /// <history>
        /// 2016/06/20 arc yano #3583 部品移動入力 移動種別の絞込み追加
        /// </history>
        public List<c_TransferType> GetTransferTypeAll(bool includeDeleted, string editNarrowFlag = "") {
            var query =
                from a in db.c_TransferType
                where (includeDeleted) || a.DelFlag.Equals("0")
                && (string.IsNullOrWhiteSpace(editNarrowFlag) || a.EditNarrowFlag.Equals("1"))     //Add 2016/06/20 arc yano #3583
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_TransferType>();
        }
        /// <summary>
        /// オーダー区分リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>オーダー区分リスト</returns>
        public List<c_OrderType> GetOrderTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_OrderType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_OrderType>();
        }

        /// <summary>
        /// 仕入伝票区分リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>仕入伝票区分リスト</returns>
        public List<c_PurchaseType> GetPurchaseTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_PurchaseType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_PurchaseType>();
        }

        /// <summary>
        /// 月内リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>月内リスト</returns>
        public List<c_RegistMonth> GetRegistMonthAll(bool includeDeleted) {
            var query =
                from a in db.c_RegistMonth
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_RegistMonth>();
        }
        /// <summary>
        /// エンジン種別リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>伝人種別リスト</returns>
        public List<c_VehicleType> GetVehicleTypeAll(bool includeDeleted) {
            var query =
               from a in db.c_VehicleType
               where (includeDeleted) || a.DelFlag.Equals("0")
               orderby a.DisplayOrder
               select a;
            return query.ToList<c_VehicleType>();
        }
        /// <summary>
        /// 前株・後株リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>前株・後株リスト</returns>
        public List<c_CorporationType> GetCorporationTypeAll(bool includeDeleted)
        {
            var query =
               from a in db.c_CorporationType
               where (includeDeleted) || a.DelFlag.Equals("0")
               orderby a.DisplayOrder
               select a;
            return query.ToList<c_CorporationType>();
        }
        /// <summary>
        /// ファームリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>ファームリスト</returns>
        public List<c_Firm> GetFirmAll(bool includeDeleted) {
            var query =
               from a in db.c_Firm
               where (includeDeleted) || a.DelFlag.Equals("0")
               orderby a.DisplayOrder
               select a;
            return query.ToList<c_Firm>();
        }
        /// <summary>
        /// プロパティ名から日本語名を取得する
        /// </summary>
        /// <param name="tableName">テーブル名(モデル名)</param>
        /// <param name="fieldName">フィールド名(プロパティ名)</param>
        /// <returns></returns>
        public string GetJpNameFromFieldName(string tableName, string fieldName) {
            var query =
                (from a in db.V_SchemaInfo
                where a.TableName.Equals(tableName)
                && a.ColumnName.Equals(fieldName)
                select a).FirstOrDefault();
            if (query != null) {
                return query.JpName.ToString();
            } else {
                return null;
            }
        }

        /// <summary>
        /// 発注ステータス一覧を取得する
        /// </summary>
        /// <param name="includeDeleted">削除済みデータを含むかどうか</param>
        /// <returns></returns>
        public List<c_PurchaseOrderStatus> GetPurchaseOrderStatusAll(bool includeDeleted) {
            var query =
                from a in db.c_PurchaseOrderStatus
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_PurchaseOrderStatus>();
        }

        /// <summary>
        /// 抹消登録一覧を取得する
        /// </summary>
        /// <param name="includeDeleted">削除済みデータを含むかどうか</param>
        /// <returns></returns>
        public List<c_EraseRegist> GetEraseRegistAll(bool includeDeleted) {
            var query =
                from a in db.c_EraseRegist
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_EraseRegist>();
        }

        /// <summary>
        /// 科目利用種別一覧を取得する
        /// </summary>
        /// <param name="includeDeleted">削除済みデータを含むかどうか</param>
        /// <returns></returns>
        public List<c_AccountUsageType> GetAccountUsageTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_AccountUsageType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_AccountUsageType>();
        }

        /// <summary>
        /// 入庫区分一覧を取得する
        /// </summary>
        /// <param name="includeDeleted">削除済みデータを含むかどうか</param>
        /// <returns></returns>
        public List<c_CarPurchaseType> GetCarPurchaseTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_CarPurchaseType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CarPurchaseType>();
        }

        /// <summary>
        /// 入金種別一覧を取得する
        /// </summary>
        /// <param name="includeDeleted">削除済みデータを含むかどうか</param>
        /// <returns></returns>
        public List<c_ReceiptType> GetReceiptTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_ReceiptType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_ReceiptType>();
        }

        /// <summary>
        /// 燃料一覧を取得する
        /// </summary>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public List<c_Fuel> GetFuelTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_Fuel
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Fuel>();
        }

         /// <summary>
        /// 新中区分を1件取得する(PK指定)
        /// </summary>
        /// <param name="code">コード</param>
        /// <returns></returns>
        public c_NewUsedType GetNewUsedTypeByKey(string code) {
            var query =
                (from a in db.c_NewUsedType
                 where a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// ロケーション種別を1件取得する(PK指定)
        /// </summary>
        /// <param name="code">コード</param>
        /// <returns></returns>
        public c_LocationType GetLocationTypeByKey(string code) {
            var query =
                (from a in db.c_LocationType
                 where a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }

        public c_CustomerRank GetCustomerRankByKey(string code) {
            var query =
                (from a in db.c_CustomerRank
                 where a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }

        public c_CustomerType GetCustomerTypeByKey(string code) {
            var query =
                (from a in db.c_CustomerType
                 where a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }

        public c_SalesOrderStatus GetSalesOrderStatusByKey(string code) {
            var query =
                (from a in db.c_SalesOrderStatus
                 where a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }

        public c_ServiceOrderStatus GetServiceOrderStatusByKey(string code) {
            var query =
                (from a in db.c_ServiceOrderStatus
                 where a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }

        public c_CustomerClaimType GetCustomerClaimTypeByKey(string code) {
            var query =
                (from a in db.c_CustomerClaimType
                 where a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }


        public c_CustomerKind GetCustomerKindByKey(string code) {
            var query =
                (from a in db.c_CustomerKind
                 where a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 業務区分一覧取得する
        /// </summary>
        /// <param name="includeDeleted">削除済みデータを含むかどうか</param>
        /// <returns></returns>
        public List<c_BusinessType> GetBusinessTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_BusinessType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_BusinessType>();
        }

        public List<c_OwnershipChangeType> GetOwnershipChangeTypeAll(bool includeDeleted) {
            var query =
                from a in db.c_OwnershipChangeType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_OwnershipChangeType>();
        }

        public List<CodeData> GetCustomerClaimList(string slipNumber) {
            List<CodeData> list = new List<CodeData>();
            var query =
                (from a in db.CarSalesPayment
                 where a.SlipNumber.Equals(slipNumber) && a.DelFlag.Equals("0")
                 select new { a.CustomerClaimCode, a.CustomerClaim.CustomerClaimName })
                .Union
                (from b in db.ServiceSalesPayment
                 where b.SlipNumber.Equals(slipNumber) && b.DelFlag.Equals("0")
                 select new { b.CustomerClaimCode, b.CustomerClaim.CustomerClaimName });
            
            foreach (var item in query.Distinct()) {
                list.Add(new CodeData { Code = item.CustomerClaimCode, Name = item.CustomerClaimName });
            }
            return list;
        }
        
        /// 消費税率リスト取得  ADD 2014/02/20 ookubo
        public List<CodeData> GetConsumptionTaxList(bool includeDeleted) {
            {
                List<CodeData> list = new List<CodeData>();
                var query =
                    (from a in db.ConsumptionTax
                     where (includeDeleted) || a.DelFlag.Equals("0")
                     select new { a.ConsumptionTaxId, a.RateName });

                foreach (var item in query.Distinct())
                {
                    list.Add(new CodeData { Code = item.ConsumptionTaxId, Name = item.RateName });
                }
                return list;
            }
        }
    
        /// Add 2014/08/19 arc yano IPO対応
        /// <summary>
        /// データ種別コードリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>データ種別コードリスト</returns>
        public List<c_CarStockDataType> GetCarStockDataTypeAll(bool includeDeleted)
        {
            var query =
                from a in db.c_CarStockDataType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CarStockDataType>();
        }
        /// <summary>
        /// データ種別コード取得(PK指定)
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>データ種別コード</returns>
        public c_CarStockDataType GetCarStockDataType(bool includeDeleted, string code)
        {
            var query =
                (from a in db.c_CarStockDataType
                where (includeDeleted) || a.DelFlag.Equals("0")
                && a.Code.Equals(code)
                select a).FirstOrDefault();
            return query;
        }

        //Add 2014/08/29 arc yano IPO対応その２(月締め状況対応)　締め処理タイプ、年、月のリスト取得の処理を追加
        /// <summary>
        /// 締め処理タイプリスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>締め処理タイプコードリスト</returns>
        public List<c_CloseType> GetCloseTypeAll(bool includeDeleted)
        {
            var query =
                from a in db.c_CloseType
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CloseType>();
        }
        /// <summary>
        /// 西暦リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>西暦コードリスト</returns>
        public List<c_Year> GetYearAll(bool includeDeleted)
        {
            var query =
                from a in db.c_Year
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Year>();
        }
        /// <summary>
        /// 月リスト取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>月コードリスト</returns>
        public List<c_Month> GetMonthAll(bool includeDeleted)
        {
            var query =
                from a in db.c_Month
                where (includeDeleted) || a.DelFlag.Equals("0")
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_Month>();
        }

        /// <summary>
        /// 西暦コード取得(PK指定)
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>西暦コード</returns>
        public c_Year GetYear(bool includeDeleted, string code)
        {
            var query =
                (from a in db.c_Year
                 where ((includeDeleted) || a.DelFlag.Equals("0"))
                 && a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 西暦コード取得(PK指定 名前からコードを取得)
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>西暦コード</returns>
        /// <history>Add 2017/04/14 arc nakayama #3751_納車リスト(明細)画面＿検索時の初期値誤り</history>
        public c_Year GetYearByName(bool includeDeleted, string name)
        {
            var query =
                (from a in db.c_Year
                 where ((includeDeleted) || a.DelFlag.Equals("0"))
                 && a.Name.Equals(name)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 月コード取得(PK指定)
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>月コード</returns>
        public c_Month GetMonth(bool includeDeleted, string code)
        {
            var query =
                (from a in db.c_Month
                 where ((includeDeleted) || a.DelFlag.Equals("0"))
                 && a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// コードと名称を取得
        /// </summary>
        /// <param name="CategoryCode">カテゴリコード</param>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴) where句の条件のバグの修正
        /// </history>
        public List<c_CodeName> GetCodeName(string CategoryCode, bool includeDeleted)
        {
            var query =
                from a in db.c_CodeName
                where ((includeDeleted) || a.DelFlag.Equals("0"))
                  && (string.IsNullOrEmpty(CategoryCode) || a.CategoryCode.Equals(CategoryCode))
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_CodeName>();
        }

        //Add 2014/11/07 arc yano 車両ステータス変更対応
        /// <summary>
        /// コードと名称を取得(PK指定)
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <param name="CategoryCode">カテゴリコード</param>
        /// <param name="code">コード</param>
        /// <returns></returns>
        public c_CodeName GetCodeNameByKey(string categoryCode, string code , bool includeDeleted)
        {
            var query =
                (from a in db.c_CodeName
                where (a.CategoryCode.Equals(categoryCode)
                  && (a.Code.Equals(code))
                  && (includeDeleted || a.DelFlag.Equals("0")))
                orderby a.DisplayOrder
                select a).FirstOrDefault();

            return query;
        }

        // Add 2015/01/19 arc nakayama 勘定奉行データファイル名変更対応
        /// <summary>
        /// コードから名称取得
        /// </summary>
        /// <param name="CategoryCode">カテゴリコード</param>
        /// <param name="code">コード</param>
        /// <returns></returns>
        public c_CodeName GetCodeName2(string CategoryCode, string code)
        {
            var query =
                (from a in db.c_CodeName
                where a.DelFlag.Equals("0")
                  && (string.IsNullOrEmpty(CategoryCode) || a.CategoryCode.Equals(CategoryCode))
                  && (string.IsNullOrEmpty(code) || a.Code.Equals(code))
                select a).FirstOrDefault();

            return query;
        }
        //Add 2015/06/17 arc nakayama 名称をマスタから取得するように変更
        /// <summary>
        /// 可否コードリストから名称取得
        /// </summary>
        /// <param name="code">可否コード</param>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>可否名称</returns>
        public c_Allowance GetAllowanceName(string code, bool includeDeleted)
        {
            var query =
                (from a in db.c_Allowance
                where ((string.IsNullOrEmpty(code)) || code.Equals(a.Code))
                && ((includeDeleted) || a.DelFlag.Equals("0"))
                select a).FirstOrDefault();

            return query;
        }

        //Add 2015/06/17 arc nakayama 名称をマスタから取得するように変更
        /// <summary>
        /// 顧客種別リストから名称取得
        /// </summary>
        /// <param name="code">可否コード</param>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>可否名称</returns>
        public c_CustomerKind GetCustomerKindName(string code, bool includeDeleted)
        {
            var query =
                (from a in db.c_CustomerKind
                 where ((string.IsNullOrEmpty(code)) || code.Equals(a.Code))
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a).FirstOrDefault();

            return query;
        }

        //Add 2015/06/24 src nakayama 経理からの要望対応③　仕掛在庫表対応
        /// <summary>
        /// 作業種別リスト取得(部品仕掛在庫検索画面専用)
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>作業種別データリスト</returns>
        public List<c_ServiceType> GetServiceTypeAllForPartsShikakari(bool includeDeleted)
        {
            var query =
                from a in db.c_ServiceType
                where (includeDeleted) || a.DelFlag.Equals("0")
                && (a.Code.Equals("002")) || (a.Code.Equals("003"))
                orderby a.DisplayOrder
                select a;
            return query.ToList<c_ServiceType>();
        }

        ///2015/08/11 arc yano  #3233 売掛金帳票対応
        /// <summary>
        /// 締ステータス文言の取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <param name="code">コード</param>
        /// <returns>締め処理タイプコードリスト</returns>
        public c_CloseStatus GetCloseStatus(bool includeDeleted, string code)
        {
            var query =
                from a in db.c_CloseStatus
                where (includeDeleted) || a.DelFlag.Equals("0")
                && a.Code.Equals(code)
                select a;
            return query.FirstOrDefault();
        }

        ///2015/10/27 arc nakayama  #3286 入金実績修正画面で請求先が表示されない
        /// <summary>
        /// JournalIDから入金実績の請求先を取得する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>請求先リスト</returns>
        public List<CodeData> GetCustomerClaimListByjournal(Guid JournalId)
        {
            List<CodeData> list = new List<CodeData>();
            var query =
                (from a in db.Journal
                 where a.JournalId.Equals(JournalId) && a.DelFlag.Equals("0")
                 select new { a.CustomerClaimCode, a.CustomerClaim.CustomerClaimName });

            foreach (var item in query.Distinct())
            {
                list.Add(new CodeData { Code = item.CustomerClaimCode, Name = item.CustomerClaimName });
            }
            return list;
        }

        /// <summary>
        /// JournalIDと伝票番号から請求先を取得する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>請求先リスト</returns>
        /// <history>
        /// 2018/12/28 yano #3970 現金出納帳　実績データが削除できない 引数と処理の変更
        /// 2018/10/30 yano #3943 入金消込後の伝票番号の請求先変更対応 新規作成
        /// </history>
        public List<CodeData> GetCustomerClaimListByjournalReceiptPlan(Journal journal)
        {
            
            List<CodeData> list = new List<CodeData>();
            //Mod 2018/12/18 yano #3970
            var query =
                (
                    from 
                        a in db.Journal join 
                        b in db.ReceiptPlan on a.SlipNumber equals b.SlipNumber into bjoin
                        from bj in bjoin.DefaultIfEmpty()                   
                    where 
                        a.JournalId.Equals(journal.JournalId) 
                     && a.DelFlag.Equals("0")
                     && bj.CustomerClaimCode != null
                     && bj.DelFlag.Equals("0")
                    select new { CustomerClaimCode = bj.CustomerClaimCode, CustomerClaimName = (bj.CustomerClaim != null ? bj.CustomerClaim.CustomerClaimName : "") }
                 );

            foreach (var item in query.Distinct())
            {
                list.Add(new CodeData { Code = item.CustomerClaimCode, Name = item.CustomerClaimName });
            }

            //Add 2018/12/28 yano #3970 実績の請求先がリストにない場合は追加
            if(list.Count == 0 || list.Where(x => x.Code.Equals(journal.CustomerClaimCode)).Count() == 0)
            {
                list.Add(new CodeData { Code = journal.CustomerClaimCode, Name = (journal.CustomerClaim != null ? journal.CustomerClaim.CustomerClaimName : new CustomerClaimDao(db).GetByKey(journal.CustomerClaimCode) != null ? new CustomerClaimDao(db).GetByKey(journal.CustomerClaimCode).CustomerClaimName : "") });
            }
            
            return list;
        }

        /// <summary>
        /// 環境性能割税率マスタを取得する
        /// </summary>
        /// <param name="includeDeleted">削除済みデータを含むかどうか</param>
        /// <returns></returns>
        /// <history>
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
        /// </history>
        public List<CodeData> GetEPDiscountTaxList(bool includeDeleted, DateTime acquisitionDate)
        {
            List<CodeData> list = new List<CodeData>();

            var query =
                from a in db.EPDiscountTax
                orderby a.DisplayOrder
                where a.FromAvailableDate <= acquisitionDate
                && a.ToAvailableDate >= acquisitionDate
                && ((includeDeleted) || a.DelFlag.Equals("0"))
                select a;

            foreach (var item in query)
            {
                list.Add(new CodeData { Code = item.EPDiscountTaxId, Name = item.RateName, Value = item.Rate });
            }

            return list;
        }

        /// <summary>
        /// 発注ステータスを取得する
        /// </summary>
        /// <param name="includeDeleted">削除済みデータを含むかどうか</param>
        /// <returns></returns>
        /// <history>
        /// Add 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 新規追加
        /// </history>
        public c_PurchaseOrderStatus GetPurchaseOrderStatus(bool includeDeleted, string code)
        {
            var query =
                from a in db.c_PurchaseOrderStatus
                where a.Code.Equals(code)
                && ((includeDeleted) || a.DelFlag.Equals("0"))
                select a;

            return query.FirstOrDefault();
        }

        /// <summary>
        /// オーダー区分リスト取得
        /// </summary>
        /// <param name="code">コード</param>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>オーダー区分リスト</returns>
        /// <history>
        /// Add 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 新規追加
        /// </history>
        public c_OrderType GetOrderType(string code,  bool includeDeleted = false)
        {
            var query =
                from a in db.c_OrderType
                where (includeDeleted) || a.DelFlag.Equals("0")
                && a.Code.Equals(code)
                select a;
            return query.FirstOrDefault();
        }
		/// <summary>
        /// 入荷ステータス取得（1件）
        /// </summary>
        /// <param name="includeDeleted">削除済みデータを含むかどうか</param>
        /// <returns></returns>
        /// <history>Add 2015/12/15 arc nakayama // Add 2015/12/09 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)</history>
        public c_PurchaseStatus GetPurchaseStatus(string code, bool includeDeleted)
        {
            var query =
                from a in db.c_PurchaseStatus
                where a.Code.Equals(code)
                && ((includeDeleted) || a.DelFlag.Equals("0"))
                select a;

            return query.FirstOrDefault();
        }

        /// <summary>
        /// 口座種別コードデータ取得　(PK指定)
        /// </summary>
        /// <param name="string">口座種別コード</param>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>口座種別コードデータ（1件）</returns>
        /// <history>Add 2016/05/19 arc nakayama #3544_入金種別をカード・ローンには変更させない</history>
        public c_AccountType GetAccountTypeByKey(string Code, bool includeDeleted)
        {
            var query =
                from a in db.c_AccountType
                where (includeDeleted) || a.DelFlag.Equals("0")
                && (a.Code.Equals(Code))
                select a;
            return query.FirstOrDefault();
        }

		/// <summary>
        /// 入庫区分を取得する(PK指定)
        /// </summary>
        /// <param name="includeDeleted">削除済みデータを含むかどうか</param>
        /// <returns></returns>
        /// <history>
        /// 2016/11/30 arc yano #3659 新規作成
        /// </history>
        public c_CarPurchaseType GetCarPurchaseType(string Code, bool includeDeleted)
        {
            var query =
                from a in db.c_CarPurchaseType
                where (includeDeleted) || a.DelFlag.Equals("0")
                &&(a.Code.Equals(Code))
                orderby a.DisplayOrder
                select a;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// 系統色コードデータ取得（PK指定）
        /// </summary>
        /// <param name="string">系統色名</param>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>系統色コードデータ（1件）</returns>
        /// <history>Add 2016/12/01 arc nakayama #3663_【製造】車両仕入　Excel取込対応</history>
        public c_ColorCategory GetColorCategoryByKey(string Name, bool includeDeleted)
        {
            var query =
                from a in db.c_ColorCategory
                where (includeDeleted) || a.DelFlag.Equals("0")
                && (a.Name.Equals(Name))
                select a;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// 名称からコード取得
        /// </summary>
        /// <param name="CategoryCode">カテゴリコード</param>
        /// <param name="Name">名称</param>
        /// <returns>レコード</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public c_CodeName GetCodeFromName(string CategoryCode, string name)
        {
            var query =
                (from a in db.c_CodeName
                 where a.DelFlag.Equals("0")
                   && (string.IsNullOrEmpty(CategoryCode) || a.CategoryCode.Equals(CategoryCode))
                   && a.Name.Equals(name)
                 select a).FirstOrDefault();

            return query;
        }

        /// <summary>
        /// 系統色コードデータ取得（PK指定）
        /// </summary>
        /// <param name="string">系統色コード</param>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>系統色コードデータ（1件）</returns>
        /// <history>2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成</history>
        public c_ColorCategory GetColorCategoryByCode(string Code, bool includeDeleted)
        {
            var query =
                from a in db.c_ColorCategory
                where (includeDeleted) || a.DelFlag.Equals("0")
                && (a.Code.Equals(Code))
                select a;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// 新中区分のコードを名称から取得する
        /// </summary>
        /// <param name="code">コード</param>
        /// <returns></returns>
        /// <history>2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成</history>
        public c_NewUsedType GetNewUsedTypeByName(string name)
        {
            var query =
                (from a in db.c_NewUsedType
                 where a.Name.Equals(name)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 環境性能割税率をIDから取得する
        /// </summary>
        /// <param name="includeDeleted">削除済みデータを含むかどうか</param>
        /// <returns></returns>
        /// <history>
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業　新規作成
        /// </history>
        public EPDiscountTax GetEPDiscountTaxById(string taxid)
        {
            var query =
                from a in db.EPDiscountTax
                where a.EPDiscountTaxId.Equals(taxid)
                select a;

            return query.FirstOrDefault();
        }
    }
}
