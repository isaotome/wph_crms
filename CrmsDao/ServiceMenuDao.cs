using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// サービスメニューマスタアクセスクラス
    ///   サービスメニューマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class ServiceMenuDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public ServiceMenuDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// サービスメニューマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="serviceMenuCode">サービスメニューコード</param>
        /// <returns>サービスメニューマスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため
        public ServiceMenu GetByKey(string serviceMenuCode, bool includeDeleted = false)
        {
            // サービスメニューデータの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            ServiceMenu serviceMenu =
                (from a in db.ServiceMenu
                 where a.ServiceMenuCode.Equals(serviceMenuCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (serviceMenu != null)
            {
                serviceMenu = EditModel(serviceMenu);
            }

            // サービスメニューデータの返却
            return serviceMenu;
        }

        /// <summary>
        /// サービスメニューマスタデータ全件取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>サービスメニューマスタデータ(全件)</returns>
        public List<ServiceMenu> GetAll(bool includeDeleted)
        {
            // 車両クラスデータの取得
            List<ServiceMenu> ret =
                (from a in db.ServiceMenu
                 where (includeDeleted) || a.DelFlag.Equals("0")
                 select a
                ).ToList<ServiceMenu>();

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 車両クラスデータの返却
            return ret;
        }

        /// <summary>
        /// サービスメニューマスタデータ検索
        /// </summary>
        /// <param name="serviceMenuCondition">サービスメニュー検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>サービスメニューマスタデータ検索結果</returns>
        public PaginatedList<ServiceMenu> GetListByCondition(ServiceMenu serviceMenuCondition, int? pageIndex, int? pageSize)
        {
            string serviceMenuCode = serviceMenuCondition.ServiceMenuCode;
            string serviceMenuName = serviceMenuCondition.ServiceMenuName;
            string delFlag = serviceMenuCondition.DelFlag;

            // サービスメニューデータの取得
            IOrderedQueryable<ServiceMenu> serviceMenuList =
                    from a in db.ServiceMenu
                    where (string.IsNullOrEmpty(serviceMenuCode) || a.ServiceMenuCode.Contains(serviceMenuCode))
                    && (string.IsNullOrEmpty(serviceMenuName) || a.ServiceMenuName.Contains(serviceMenuName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.ServiceMenuCode
                    select a;

            // ページング制御情報を付与したサービスメニューデータの返却
            PaginatedList<ServiceMenu> ret = new PaginatedList<ServiceMenu>(serviceMenuList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="serviceMenu">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private ServiceMenu EditModel(ServiceMenu serviceMenu)
        {
            // 内部コード項目の名称情報取得
            serviceMenu.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, serviceMenu.DelFlag);

            // 出口
            return serviceMenu;
        }
    }
}
