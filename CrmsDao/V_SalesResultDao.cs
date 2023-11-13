using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;
//Add 2015/07/21 arc nakayama ダーティーリード（ReadUncommitted）追加
using System.Transactions;

namespace CrmsDao
{
     /// <summary>
    /// 販売実績アクセスクラス
    ///   販売実績の各種検索メソッドを提供します。
    /// </summary>
    public class V_SalesResultDao
    {
         /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_SalesResultDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 販売実績データ検索
        /// （ページング対応）
        /// </summary>
        /// <param name="Condition">販売実績検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <param name="condition">検索条件</param>
        /// <returns>顧客マスタデータ検索結果</returns>
        public PaginatedList<SalesResult> GetResult(int? pageIndex, int? pageSize, SalesResult condition)
        {
            // ページング制御情報を付与した顧客データの返却
            PaginatedList<SalesResult> ret = new PaginatedList<SalesResult>(GetSalseResultList(condition), pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }
        /// <summary>
        /// 販売実績一覧を取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        private IQueryable<SalesResult> GetSalseResultList(SalesResult condition)
        {
            
            // 販売実績データの取得
            //Mod 2014/11/04 arc yano #3080_顧客検索機能の新設対応その３ 指摘事項の反映　検索条件の追加(陸運局コード)
            //Mod 2014/10/07 arc yano #3080_顧客検索機能の新設対応その２ 経理課の要望事項の反映　検索条件の追加(顧客名(カナ)、登録番号【種別、かな、プレート】、型式)
            //Add 2015/07/21 arc nakayama ダーティーリード（ReadUncommitted）追加
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                var query =
                        (from a in db.V_SalesResult
                         where (string.IsNullOrEmpty(condition.CustomerCode) || a.CustomerCode.Equals(condition.CustomerCode))
                         && ((string.IsNullOrEmpty(condition.CustomerName) || a.CustomerName.Contains(condition.CustomerName))
                         || (string.IsNullOrEmpty(condition.CustomerNameKana) || a.CustomerNameKana.Contains(condition.CustomerNameKana)))
                         && (string.IsNullOrEmpty(condition.SalesCarNumber) || a.SalesCarNumber.Contains(condition.SalesCarNumber))
                         && (string.IsNullOrEmpty(condition.Vin) || a.Vin.Contains(condition.Vin))
                         && (string.IsNullOrEmpty(condition.RegistrationNumberType) || a.RegistrationNumberType.Contains(condition.RegistrationNumberType))
                         && (string.IsNullOrEmpty(condition.RegistrationNumberKana) || a.RegistrationNumberKana.Contains(condition.RegistrationNumberKana))
                         && (string.IsNullOrEmpty(condition.RegistrationNumberPlate) || a.RegistrationNumberPlate.Contains(condition.RegistrationNumberPlate))
                         && (string.IsNullOrEmpty(condition.ModelName) || a.ModelName.Contains(condition.ModelName))
                         && (string.IsNullOrEmpty(condition.CarSlipNumber) || a.CarSlipNumber.Contains(condition.CarSlipNumber))
                         && (condition.CarSalesDateFrom == null || DateTime.Compare(a.CarSalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.CarSalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                         && (condition.CarSalesDateTo == null || DateTime.Compare(a.CarSalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.CarSalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                         && (string.IsNullOrEmpty(condition.ServiceSlipNumber) || a.ServiceSlipNumber.Contains(condition.ServiceSlipNumber))
                         && (condition.ServiceSalesDateFrom == null || DateTime.Compare(a.ServiceSalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.ServiceSalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                         && (condition.ServiceSalesDateTo == null || DateTime.Compare(a.ServiceSalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.ServiceSalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                         && (string.IsNullOrEmpty(condition.MorterViecleOfficialCode) || a.MorterViecleOfficialCode.Contains(condition.MorterViecleOfficialCode))
                         select a
                        ).Distinct().OrderBy(x => x.CustomerCode).ThenBy(x => x.CarSlipNumber).ThenBy(x => x.ServiceSlipNumber);


                PaginatedList<SalesResult> list = new PaginatedList<SalesResult>();

                foreach (var a in query)
                {
                    SalesResult rec = new SalesResult();
                    rec.CustomerCode = a.CustomerCode;
                    rec.CustomerName = a.CustomerName;
                    rec.SalesCarNumber = a.SalesCarNumber;
                    rec.Vin = a.Vin;
                    rec.CarSlipNumber = a.CarSlipNumber;
                    rec.CarRevisionNumber = a.CarRevisionNumber;
                    rec.CarSalesDate = a.CarSalesDate;
                    rec.ServiceSlipNumber = a.ServiceSlipNumber;
                    rec.ServiceRevisionNumber = a.ServiceRevisionNumber;
                    rec.ServiceSalesDate = a.ServiceSalesDate;
                    //Add 2014/10/07 arc yano 顧客検索新機能その２ 登録番号、型式追加
                    rec.RegistrationNumberType = a.RegistrationNumberType;
                    rec.RegistrationNumberKana = a.RegistrationNumberKana;
                    rec.RegistrationNumberPlate = a.RegistrationNumberPlate;
                    rec.ModelName = a.ModelName;
                    //Add 2014/11/04 arc yano 顧客検索新機能その３ 指摘事項の反映 陸運局コードの追加
                    rec.MorterViecleOfficialCode = a.MorterViecleOfficialCode;
                    list.Add(rec);
                }

                IQueryable<SalesResult> salesResultList = list.AsQueryable();

                return salesResultList;
            }
        }

    }


}
