using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class MenuControlDao 
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

         /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public MenuControlDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// メニューデータを取得する（PK指定）
        /// </summary>
        /// <param name="menuControlCode">メニューコード</param>
        /// <returns>メニューデータ</returns>
        public MenuControl GetByKey(string menuControlCode)
        {
            var query =
                (from m in db.MenuControl
                 where m.MenuControlCode.Equals(menuControlCode)
                 select m).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// メニューコントロールデータ全件取得
        /// </summary>
        /// <returns></returns>
        public List<MenuControl> GetListAll()
        {
            var query =
                from m in db.MenuControl
                orderby m.MenuGroup.DisplayOrder,m.DisplayOrder
                select m;
            return query.ToList<MenuControl>();

        }

        /// <summary>
        /// メニューコントロールデータ取得（アプリケーションコード指定）
        /// </summary>
        /// <param name="ControllerName">アプリケーションコード</param>
        /// <returns>メニューコントロールデータ（1件）</returns>
        public MenuControl GetByControllerName(string applicationCode)
        {
            var query =
                (from m in db.MenuControl
                 where m.ApplicationCode.Equals(applicationCode)
                 select m).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// メニューコントロールデータ取得（URL指定）
        /// </summary>
        /// <param name="url">URL仮想パス</param>
        /// <returns>メニューコントロールデータ</returns>
        public MenuControl GetByUrl(string url) {
            var query =
                (from m in db.MenuControl
                 where m.NavigateUrl.Equals(url)
                 select m).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 利用可能メニューリストを取得
        /// </summary>
        /// <param name="MenuGroupCode">メニューグループコード</param>
        /// <param name="SecurityRoleCode">セキュリティロールコード</param>
        /// <returns>メニューリスト</returns>
        //Add 2015/10/19 arc nakayama #3274_不必要なメニューを削除の影響調査　メニュー表示時にDelFlagを見るようにする
        public List<MenuControl> GetAvailableListByGroupCode(string MenuGroupCode, string SecurityRoleCode) {
            var query =
                from m in db.MenuControl
                join a in db.ApplicationRole on new { m.ApplicationCode, SecurityRoleCode } equals new { a.ApplicationCode, a.SecurityRoleCode }
                orderby m.DisplayOrder
                where a.EnableFlag == true
                && m.MenuGroupCode.Equals(MenuGroupCode)
                && m.DelFlag.Equals("0")
                select m;
            return query.ToList<MenuControl>();

        }
    }
}
