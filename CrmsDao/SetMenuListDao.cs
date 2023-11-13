using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao {

    /// <summary>
    /// セットメニューマスタアクセスクラス
    ///   セットメニューマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class SetMenuListDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public SetMenuListDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        /// <summary>
        /// セットメニューマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="setMenuCode">セットメニューコード</param>
        /// <param name="setMenuCode">セットメニューコード</param>
        /// <returns>セットメニューマスタデータ(1件)</returns>
        public SetMenuList GetByKey(string setMenuCode, int detailsNumber) {

            // セットメニューデータの取得
            SetMenuList setMenu =
                (from a in db.SetMenuList
                 where a.SetMenuCode.Equals(setMenuCode)
                 && a.DetailsNumber == detailsNumber
                 select a
                ).FirstOrDefault();

            // 非DB格納項目の編集
            if (setMenu != null) {
                setMenu = EditModel(setMenu);
            }

            // セットメニューデータの返却
            return setMenu;
        }

        /// <summary>
        /// セットメニューマスタデータ検索
        /// </summary>
        /// <param name="setMenuCondition">セットメニュー検索条件</param>
        /// <returns>セットメニューマスタデータ検索結果</returns>
        public List<SetMenuList> GetListByCondition(SetMenuList setMenuCondition) {

            string setMenuCode = setMenuCondition.SetMenuCode;
            string delFlag = setMenuCondition.DelFlag;

            // セットメニューデータの取得
            List<SetMenuList> setMenuList =
                    (from a in db.SetMenuList
                     where (string.IsNullOrEmpty(setMenuCode) || a.SetMenuCode.Equals(setMenuCode))
                     && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                     orderby a.SetMenuCode, a.DetailsNumber
                     select a).ToList<SetMenuList>();

            // 非DB格納項目の編集
            for (int i = 0; i < setMenuList.Count; i++) {
                setMenuList[i] = EditModel(setMenuList[i]);
            }

            // 出口
            return setMenuList.ToList<SetMenuList>();
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="setMenu">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private SetMenuList EditModel(SetMenuList setMenuList) {

            setMenuList.InputDetailsNumber = setMenuList.DetailsNumber * 10;

            // 出口
            return setMenuList;
        }
    }
}
