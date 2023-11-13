using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class SetMenuDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">データコンテキスト</param>
        public SetMenuDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// セットメニューマスタ検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns>セットメニューリスト</returns>
        public PaginatedList<SetMenu> GetListByCondition(SetMenu condition, int pageIndex, int pageSize) {
            string setMenuCode = condition.SetMenuCode;
            string setMenuName = condition.SetMenuName;
            string companyCode = condition.CompanyCode;

            IOrderedQueryable<SetMenu> setMenu =
                from a in db.SetMenu
                where (string.IsNullOrEmpty(setMenuCode) || a.SetMenuCode.Contains(setMenuCode))
                && (string.IsNullOrEmpty(setMenuName) || a.SetMenuName.Contains(setMenuName))
                && (string.IsNullOrEmpty(companyCode) || a.CompanyCode.Contains(companyCode))
                orderby a.CompanyCode, a.SetMenuCode
                select a;

            return new PaginatedList<SetMenu>(setMenu, pageIndex, pageSize);
        }

        /// <summary>
        /// セットメニューを取得する（PK指定）
        /// </summary>
        /// <param name="setMenuCode">セットメニューコード</param>
        /// <returns>セットメニューデータ</returns>
        public SetMenu GetByKey(string setMenuCode) {
            SetMenu setMenu =
                (from a in db.SetMenu
                 where a.SetMenuCode.Equals(setMenuCode)
                 select a).FirstOrDefault();
            return setMenu;
        }
    }
}
