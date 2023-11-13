using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class PartsAverageCostList
    {
        // 締日
        public string CloseMonth { get; set; }

        // 部門コード
        public string PartsNumber { get; set; }

        // 部門名
        public string PartsNameJP { get; set; }

        // 顧客コード
        public decimal? PriceList { get; set; }

        // 顧客名
        public decimal? Price { get; set; }
    }
    public class PartsAverageCostListDao
    {
        private CrmsLinqDataContext db;
        public PartsAverageCostListDao(CrmsLinqDataContext context)
        {
            db = context;
        }
        public List<PartsAverageCostList> GetPartsAverageCostList(DateTime? TargetDate)
        {
            var query = from PAC in db.PartsAverageCost
                        join P in db.Parts on new { a = PAC.PartsNumber, b = "0" } equals new { a = P.PartsNumber, b = P.DelFlag } into GP
                        from LP in GP.DefaultIfEmpty()
                        where PAC.DelFlag.Equals("0") &&
                              PAC.CloseMonth == TargetDate 
                        orderby PAC.PartsNumber
                        select new PartsAverageCostList
                        {
                            CloseMonth = PAC.CloseMonth.ToShortDateString(),
                            PartsNumber = PAC.PartsNumber,
                            PartsNameJP = ( LP.PartsNameJp == null ? String.Empty : LP.PartsNameJp),
                            PriceList = ( LP.Price == null ? 0 :  LP.Price ),
                            Price = (PAC.Price == null ? 0 : PAC.Price)
                        };
            List<PartsAverageCostList> list = query.ToList<PartsAverageCostList>();
            return list;
        }
    }
}
