using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class CustomerDMDao {
        private CrmsLinqDataContext db;
        public CustomerDMDao(CrmsLinqDataContext context) {
            db = context;
        }

        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　デフォルト引数追加（削除フラグを考慮するかどうかのフラグ追加（デフォルト考慮する））
        public CustomerDM GetByKey(string code, bool includeDeleted = false)
        {
            var query = (from a in db.CustomerDM
                         where code.Equals(a.CustomerCode)
                         && ((includeDeleted) || a.DelFlag.Equals("0"))
                        select a).FirstOrDefault();
            return query;
        }
    }
}
