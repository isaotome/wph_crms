using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class CrmsDataContext
    {
        #region インスタンス作成共通メソッド
        public static CrmsLinqDataContext GetDataContext()
        {
            string connectionString = CrmsDao.Properties.Settings.Default.CRMSConnectionString;
            return new CrmsLinqDataContext(connectionString);
        }
        #endregion
    }
}
