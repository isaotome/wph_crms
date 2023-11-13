using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// サービス受付対象ビューアクセスクラス
    ///   サービス受付対象ビューの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class V_ServiceReceiptTargetDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_ServiceReceiptTargetDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// サービス受付対象ビューデータ検索
        /// </summary>
        /// <param name="v_ServiceReceiptTargetCondition">サービス受付対象検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>サービス受付対象ビューデータ検索結果</returns>
        public PaginatedList<V_ServiceReceiptTarget> GetListByCondition(V_ServiceReceiptTarget v_ServiceReceiptTargetCondition, int? pageIndex, int? pageSize)
        {
            string customerNameKana = v_ServiceReceiptTargetCondition.CustomerNameKana;
            string customerName = v_ServiceReceiptTargetCondition.CustomerName;
            string telNumber = v_ServiceReceiptTargetCondition.TelNumber;
            string morterViecleOfficialCode = v_ServiceReceiptTargetCondition.MorterViecleOfficialCode;
            string registrationNumberType = v_ServiceReceiptTargetCondition.RegistrationNumberType;
            string registrationNumberKana = v_ServiceReceiptTargetCondition.RegistrationNumberKana;
            string registrationNumberPlate = v_ServiceReceiptTargetCondition.RegistrationNumberPlate;
            string vin = v_ServiceReceiptTargetCondition.Vin;
            string modelName = v_ServiceReceiptTargetCondition.ModelName;
            DateTime? firstReceiptionDateFrom = v_ServiceReceiptTargetCondition.FirstReceiptionDateFrom;
            DateTime? firstReceiptionDateTo = v_ServiceReceiptTargetCondition.FirstReceiptionDateTo;
            DateTime? lastReceiptionDateFrom = v_ServiceReceiptTargetCondition.LastReceiptionDateFrom;
            DateTime? lastReceiptionDateTo = v_ServiceReceiptTargetCondition.LastReceiptionDateTo;

            // サービス受付対象データの取得
            IOrderedQueryable<V_ServiceReceiptTarget> v_ServiceReceiptTargetList =
                    from a in db.V_ServiceReceiptTarget
                    where (string.IsNullOrEmpty(customerNameKana) || a.CustomerNameKana.Contains(customerNameKana))
                    && (string.IsNullOrEmpty(customerName) || a.CustomerName.Contains(customerName))
                    && (string.IsNullOrEmpty(telNumber) || a.TelNumber.Substring(a.TelNumber.Length - 4, 4).Equals(telNumber) || a.MobileNumber.Substring(a.MobileNumber.Length - 4, 4).Equals(telNumber))
                    && (string.IsNullOrEmpty(morterViecleOfficialCode) || a.MorterViecleOfficialCode.Contains(morterViecleOfficialCode))
                    && (string.IsNullOrEmpty(registrationNumberType) || a.RegistrationNumberType.Contains(registrationNumberType))
                    && (string.IsNullOrEmpty(registrationNumberKana) || a.RegistrationNumberKana.Contains(registrationNumberKana))
                    && (string.IsNullOrEmpty(registrationNumberPlate) || a.RegistrationNumberPlate.Equals(registrationNumberPlate))
                    && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
                    && (string.IsNullOrEmpty(modelName) || a.ModelName.Contains(modelName))
                    && (firstReceiptionDateFrom == null || DateTime.Compare(a.FirstReceiptionDate ?? DaoConst.SQL_DATETIME_MIN, firstReceiptionDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (firstReceiptionDateTo == null || DateTime.Compare(a.FirstReceiptionDate ?? DaoConst.SQL_DATETIME_MAX, firstReceiptionDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (lastReceiptionDateFrom == null || DateTime.Compare(a.LastReceiptionDate ?? DaoConst.SQL_DATETIME_MIN, lastReceiptionDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (lastReceiptionDateTo == null || DateTime.Compare(a.LastReceiptionDate ?? DaoConst.SQL_DATETIME_MAX, lastReceiptionDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    orderby a.CustomerNameCtrl, a.CustomerName
                    , a.MakerNameCtrl, a.MakerName
                    , a.CarNameCtrl, a.CarName
                    , a.CarGradeNameCtrl, a.CarGradeName
                    , a.RegistrationNumberPlateCtrl, a.RegistrationNumberPlate
                    , a.VinCtrl, a.Vin
                    select a;

            // ページング制御情報を付与したサービス受付対象データの返却
            return new PaginatedList<V_ServiceReceiptTarget>(v_ServiceReceiptTargetList, pageIndex ?? 0, pageSize ?? 0);
        }
    }
}
