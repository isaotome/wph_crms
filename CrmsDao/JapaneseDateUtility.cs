using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace CrmsDao {
    public static class JapaneseDateUtility {
                
        public static JapaneseDate GetJapaneseDate(DateTime? datetime) {

            CultureInfo ci = CultureInfo.CreateSpecificCulture("ja-JP");
            ci.DateTimeFormat.Calendar = new JapaneseCalendar();
            if (datetime != null) {
                JapaneseDate jDate = new JapaneseDate();
                jDate.Gengou = ci.DateTimeFormat.Calendar.GetEra(datetime ?? DateTime.Parse("1900/01/01"));
                jDate.Year = ci.DateTimeFormat.Calendar.GetYear(datetime ?? DateTime.Parse("1900/01/01"));
                jDate.Month = datetime.Value.Month;
                jDate.Day = datetime.Value.Day;
                return jDate;
            }
            return null;
        }

        /// <summary>
        /// 西暦時刻取得
        /// </summary>
        /// <param name="gengou">元号</param>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="day">日</param>
        /// <param name="db">dbコンテキスト</param>
        /// <returns>西暦</returns>
        /// <history>
        /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
        /// </history>
        public static DateTime? GetGlobalDate(int? gengou, int? year, int? month, int? day, CrmsLinqDataContext db) {
            return GetGlobalDate(new JapaneseDate { Gengou = gengou, Year = year, Month = month, Day = day }, db);
        }
        /// <summary>
        /// 和暦→西暦変換
        /// </summary>
        /// <param name="jDate">和暦</param>
        /// <param name="db">dbコンテキスト</param>
        /// <returns>西暦</returns>
        /// <history>
        /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
        /// </history>
        public static DateTime? GetGlobalDate(JapaneseDate jDate, CrmsLinqDataContext db) {
            CultureInfo ci = CultureInfo.CreateSpecificCulture("ja-JP");
            ci.DateTimeFormat.Calendar = new JapaneseCalendar();
            if (!jDate.IsNull) {
                string gengouName = CodeUtils.GetName(CodeUtils.GetGengouList(db), jDate.Gengou.ToString());    // 2018/06/22 arc yano #3891
                //string gengouName = CodeUtils.GetName(CodeUtils.GetGengouList(), jDate.Gengou.ToString());
                DateTime dt = DateTime.ParseExact(string.Format("{0}{1}年{2}月{3}日", gengouName, jDate.Year, jDate.Month, jDate.Day), "ggyy年M月d日", ci);
                return dt;
            }
            return null;
        }
        /// <summary>
        /// 和暦→西暦変換変換チェック
        /// </summary>
        /// <param name="jDate">和暦</param>
        /// <param name="db">dbコンテキスト</param>
        /// <returns>変換可能かどうか</returns>
        /// <history>
        /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
        /// </history>
        public static bool GlobalDateTryParse(JapaneseDate jDate, CrmsLinqDataContext db) {
            CultureInfo ci = CultureInfo.CreateSpecificCulture("ja-JP");
            ci.DateTimeFormat.Calendar = new JapaneseCalendar();
            if (!jDate.IsNull) {
                string gengouName = CodeUtils.GetName(CodeUtils.GetGengouList(db), jDate.Gengou.ToString()); // 2018/06/22 arc yano #3891
                //string gengouName = CodeUtils.GetName(CodeUtils.GetGengouList(), jDate.Gengou.ToString());
                DateTime dt;
                return DateTime.TryParseExact(string.Format("{0}{1}年{2}月{3}日", gengouName, jDate.Year, jDate.Month, jDate.Day), "ggyy年M月d日", ci,DateTimeStyles.None,out dt);
            }
            return false;
        }
    }
    public class JapaneseDate {
        public DateTime? Seireki { get; set; }
        public int? Gengou { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public bool IsNull {
            get {
                if (Seireki == null && (Year == null || Month == null || Day == null)) {
                    return true;
                }
                return false;
            }
        }
    }
}
