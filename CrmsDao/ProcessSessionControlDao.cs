using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class ProcessSessionControlDao {
        private CrmsLinqDataContext db;
        public ProcessSessionControlDao(CrmsLinqDataContext context) {
            db = context;
        }
        public ProcessSessionControl GetByKey(Guid? processSessionId) {
            var query = (from a in db.ProcessSessionControl
                         where a.ProcessSessionId.Equals(processSessionId)
                         select a).FirstOrDefault();
            return query;
        }
    }
}
