using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 消費税マスタアクセスクラス
    ///   消費税マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class ConsumptionTaxDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public ConsumptionTaxDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 消費税マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="ConsumptionTaxId">消費税ID</param>
        /// <returns>消費税マスタデータ(1件)</returns>
        public ConsumptionTax GetByKey(string ConsumptionTaxId)
        {
            // 消費税データの取得
            ConsumptionTax ConsumptionTax =
                (from a in db.ConsumptionTax
                 where a.ConsumptionTaxId.Equals(ConsumptionTaxId)
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (ConsumptionTax != null)
            {
                ConsumptionTax = EditModel(ConsumptionTax);
            }

            // 消費税データの返却
            return ConsumptionTax;
        }

        /// <summary>
        /// 消費税マスタデータ検索
        /// </summary>
        /// <param name="ConsumptionTaxCondition">消費税検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>消費税マスタデータ検索結果</returns>
        public PaginatedList<ConsumptionTax> GetListByCondition(ConsumptionTax ConsumptionTaxCondition, int? pageIndex, int? pageSize)
        {
            string ConsumptionTaxId = ConsumptionTaxCondition.ConsumptionTaxId;
            string RateName = ConsumptionTaxCondition.RateName;
            string CreateEmployeeCode = null;
            try { CreateEmployeeCode = ConsumptionTaxCondition.CreateEmployeeCode; }
            catch (NullReferenceException) { }
            string delFlag = ConsumptionTaxCondition.DelFlag;

            // 消費税データの取得
            IOrderedQueryable<ConsumptionTax> ConsumptionTaxList =
                    from a in db.ConsumptionTax
                    where (string.IsNullOrEmpty(ConsumptionTaxId) || a.ConsumptionTaxId.Contains(ConsumptionTaxId))
                    && (string.IsNullOrEmpty(RateName) || a.RateName.Contains(RateName))
                    && (string.IsNullOrEmpty(CreateEmployeeCode) || a.CreateEmployeeCode.Contains(CreateEmployeeCode))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.ConsumptionTaxId
                    select a;

            // ページング制御情報を付与した消費税データの返却
            PaginatedList<ConsumptionTax> ret = new PaginatedList<ConsumptionTax>(ConsumptionTaxList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="ConsumptionTax">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private ConsumptionTax EditModel(ConsumptionTax ConsumptionTax)
        {
            // 内部コード項目の名称情報取得
            ConsumptionTax.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, ConsumptionTax.DelFlag);

            // 出口
            return ConsumptionTax;
        }
        /// 消費税率テーブルのIDから税率を取得  ADD 2014/02/20 ookubo
        public string GetConsumptionTaxRateByKey(string ConsumptionTaxId)
        {
            var query =
                (from a in db.ConsumptionTax
                 where a.ConsumptionTaxId.Equals(ConsumptionTaxId)
                 select a).FirstOrDefault();
            if (query != null)
            {
                return query.Rate.ToString();
            }
            else
            {
                return null;
            }
        }

        /// 消費税率テーブルの利用開始日付からIDを取得  ADD 2014/02/20 ookubo
        public string GetConsumptionTaxIDByDate(DateTime? dt)
        {
            var query =
                (from a in db.ConsumptionTax
                 where a.FromAvailableDate <= dt
                 && a.ToAvailableDate >= dt
                 && a.Rate > 0
                 select a).FirstOrDefault();
            if (query != null)
            {
                return query.ConsumptionTaxId.ToString();
            }
            else
            {
                return null;
            }
        }

        /// 消費税率テーブルの利用開始日付から税率を取得  ADD 2014/02/20 ookubo
        public string GetConsumptionTaxRateByDate(DateTime? dt)
        {
            var id = GetConsumptionTaxIDByDate(dt);
            return GetConsumptionTaxRateByKey(id);
        }

    
    }
}
