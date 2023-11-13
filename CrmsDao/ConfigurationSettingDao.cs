using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class ConfigurationSettingDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public ConfigurationSettingDao(CrmsLinqDataContext context){
            db = context;
        }

        /// <summary>
        /// 設定データを取得する
        /// </summary>
        /// <param name="keyName">キー名</param>
        /// <returns>設定データ</returns>
        public ConfigurationSetting GetByKey(string code) {
            var query =
                (from a in db.ConfigurationSetting
                 where a.Code.Equals(code)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 設定データリストを取得する
        /// </summary>
        /// <returns>設定データ</returns>
        public List<ConfigurationSetting> GetListAll() {
            var query =
                from a in db.ConfigurationSetting
                 where !a.DelFlag.Equals("1")
                 select a;
            return query.ToList();
        }
    }
}
