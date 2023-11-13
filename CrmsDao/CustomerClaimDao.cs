using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;

namespace CrmsDao
{
    /// <summary>
    /// 請求先マスタアクセスクラス
    ///   請求先マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CustomerClaimDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CustomerClaimDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 請求先マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="customerClaimCode">請求先コード</param>
        /// <returns>請求先マスタデータ(1件)</returns>
        public CustomerClaim GetByKey(string customerClaimCode, bool includeDeleted = false)
        {
            // 請求先データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            CustomerClaim customerClaim =
                (from a in db.CustomerClaim
                 where a.CustomerClaimCode.Equals(customerClaimCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (customerClaim != null)
            {
                customerClaim = EditModel(customerClaim);
            }

            // 請求先データの返却
            return customerClaim;
        }

        /// <summary>
        /// 請求先マスタデータ検索
        /// </summary>
        /// <param name="customerClaimCondition">請求先検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>請求先マスタデータ検索結果</returns>
        /// <history>
        /// 2018/04/26 arc yano #3650 顧客一覧で「上様」を一番上に表示したい
        /// 2016/04/13 arc yano #3480 サービス伝票　サービス伝票の請求先を主作業の内容により切り分ける クエリストリングの大分類により、請求先を絞り込む
        /// </history>
        public PaginatedList<CustomerClaim> GetListByCondition(CustomerClaim customerClaimCondition, int? pageIndex, int? pageSize)
        {
            string customerClaimCode = customerClaimCondition.CustomerClaimCode;
            string customerClaimName = customerClaimCondition.CustomerClaimName;
            string delFlag = customerClaimCondition.DelFlag;

            
            //ADD #3111 請求先検索ダイアログに請求種別追加対応　2014/10/20 arc ishii
            string customerClaimType = customerClaimCondition.CustomerClaimType;

            List<string> customerClaimTypeList = customerClaimCondition.CustomerClaimTypeList;    //Add 2016/04/14 arc yano #3480
            IOrderedQueryable<CustomerClaim> customerClaimList = null;

            // 請求先データの取得
            //ADD #3111 請求先検索ダイアログに請求種別追加対応　2014/10/20 arc ishii
            //IOrderedQueryable<CustomerClaim> customerClaimList =
            var query = 
                    from a in db.CustomerClaim
                    where (string.IsNullOrEmpty(customerClaimCode) || a.CustomerClaimCode.Contains(customerClaimCode))
                    && (string.IsNullOrEmpty(customerClaimName) || a.CustomerClaimName.Contains(customerClaimName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    && (string.IsNullOrEmpty(customerClaimType) || a.CustomerClaimType.Equals(customerClaimType))
                    //orderby a.CustomerClaimCode
                    select a;

            //Mod 2018/04/26 arc yano #3650
            //Mod 2016/04/14 arc yano #3480
            if (customerClaimTypeList != null)
            {
                customerClaimList = query.Where(x => customerClaimTypeList.Contains(x.CustomerClaimType)).OrderBy(x => (x.DisplayOrder ?? 9999999)).ThenBy(x => x.CustomerClaimCode);
                //customerClaimList = query.Where(x => customerClaimTypeList.Contains(x.CustomerClaimType)).OrderBy(x => x.CustomerClaimCode);
            }
            else
            {
                customerClaimList = query.OrderBy(x => (x.DisplayOrder ?? 9999999)).ThenBy(x => x.CustomerClaimCode);//Add 2018/04/26 arc yano #3650
                //customerClaimList = query.OrderBy(x => x.CustomerClaimCode);
            }

            // ページング制御情報を付与した請求先データの返却
            PaginatedList<CustomerClaim> ret = new PaginatedList<CustomerClaim>(customerClaimList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="customerClaim">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private CustomerClaim EditModel(CustomerClaim customerClaim)
        {
            // 内部コード項目の名称情報取得
            customerClaim.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, customerClaim.DelFlag);

            // 出口
            return customerClaim;
        }
    }
}
