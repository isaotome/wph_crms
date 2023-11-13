using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class MenuGroupDao
    {
        private CrmsLinqDataContext db;
        public MenuGroupDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }
        public List<MenuGroup> GetListAll()
        {
            var query =
                from g in db.MenuGroup
                select g;
            return query.ToList<MenuGroup>();
        }
    }
}
