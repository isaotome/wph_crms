using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 主作業マスタアクセスクラス
    ///   主作業マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class ServiceWorkDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public ServiceWorkDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 主作業マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="serviceWorkCode">主作業コード</param>
        /// <returns>主作業マスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため
        public ServiceWork GetByKey(string serviceWorkCode, bool includeDeleted = false)
        {
            // 主作業データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            ServiceWork serviceWork =
                (from a in db.ServiceWork
                 where a.ServiceWorkCode.Equals(serviceWorkCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (serviceWork != null)
            {
                serviceWork = EditModel(serviceWork);
            }

            // 主作業データの返却
            return serviceWork;
        }

        /// <summary>
        /// 主作業マスタデータ検索(ページ情報付)
        /// </summary>
        /// <param name="serviceWorkCondition">主作業検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>主作業マスタデータ検索結果</returns>
        /// <history>
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注支払一覧) クエリ結果を取得する処理を外出し(GetQueryable)
        /// 2016/04/14 arc yano #3480 サービス伝票　サービス伝票の請求先を主作業の内容により切り分ける 主作業⇔請求先コード絞込み
        /// </history>
        public PaginatedList<ServiceWork> GetListByCondition(ServiceWork serviceWorkCondition, int? pageIndex, int? pageSize)
        {
           
            // ページング制御情報を付与した主作業データの返却
            PaginatedList<ServiceWork> ret = new PaginatedList<ServiceWork>(GetQueryable(serviceWorkCondition), pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }

        /// <summary>
        /// 主作業マスタデータ検索
        /// </summary>
        /// <param name="serviceWorkCondition">主作業検索条件</param>
        /// <returns>主作業マスタデータ検索結果</returns>
        /// <history>
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注支払一覧) 新規追加
        /// </history>
        public IQueryable<ServiceWork> GetQueryable(ServiceWork serviceWorkCondition)
        {
            string serviceWorkCode = serviceWorkCondition.ServiceWorkCode;
            string name = serviceWorkCondition.Name;
            string classification1 = serviceWorkCondition.Classification1;
            string classification2 = serviceWorkCondition.Classification2;
            string delFlag = serviceWorkCondition.DelFlag;
            string customerClaimClass = serviceWorkCondition.CustomerClaimClass;            

            IOrderedQueryable<ServiceWork> serviceWorkList = null;

            // 主作業データの取得
            var query =
                    from a in db.ServiceWork
                    where (string.IsNullOrEmpty(serviceWorkCode) || a.ServiceWorkCode.Contains(serviceWorkCode))
                    && (string.IsNullOrEmpty(name) || a.Name.Contains(name))
                    && (string.IsNullOrEmpty(classification1) || a.Classification1.Equals(classification1))
                    && (string.IsNullOrEmpty(classification2) || a.Classification2.Equals(classification2))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    select a;
                    

            //Add 2016/04/14 arc yano #3480
            if (!string.IsNullOrEmpty(customerClaimClass))
            {
                //Mod 201705/02 arc nakayama #3760_サービス伝票_請求先を選択した状態で主作業のルックアップを開くとシステムエラー
                serviceWorkList = query.Where(x => (x.CustomerClaimClass == null || x.CustomerClaimClass == "") || x.CustomerClaimClass.Equals(customerClaimClass)).AsQueryable().OrderBy(x => x.ServiceWorkCode).ThenBy(x => x.Classification2).ThenBy(x => x.Classification1);
            }
            else
            {
                serviceWorkList = query.AsQueryable().OrderBy(x => x.ServiceWorkCode).ThenBy(x => x.Classification2).ThenBy(x => x.Classification1);
            }

            // 出口
            return serviceWorkList;
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="serviceWork">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private ServiceWork EditModel(ServiceWork serviceWork)
        {
            // 内部コード項目の名称情報取得
            serviceWork.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, serviceWork.DelFlag);

            // 出口
            return serviceWork;
        }
    }
}
