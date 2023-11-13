using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections;
using System.Text;
using Microsoft.VisualBasic;
using System.Configuration; //Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる
using System.Data;


namespace CrmsDao
{

    /// <summary>
    /// 共通ユーティリティクラス
    /// </summary>
    public class CommonUtils
    {

        /// <summary>
        /// 定数定義
        /// </summary>
        private static readonly string PTN_CANCEL_REGISTRATION = "2";             //登録・引当解除                   //Add 2018/08/07 yano #3911
        //private static readonly string PTN_CANCEL_RESERVATION = "99";             //引当解除                         //Add 2018/08/07 yano #3911

        private static readonly string CANCEL_FROM_CARSLIPSTATUSCHANGE = "1";     //ステータス戻しによる引当解除     //Add 2018/08/07 yano #3911
        private static readonly string CANCEL_FROM_CANCEL = "2";                  //受注後キャンセルによる引当解除   //Add 2018/08/07 yano #3911
        //private static readonly string CANCEL_FROM_AKADEN = "3";                  //赤伝処理による引当解除           //Add 2018/08/07 yano #3911

        /// <summary>
        /// 数値⇒文字列変換
        /// </summary>
        /// <param name="i">数値</param>
        /// <returns>引数と同値を表す文字列(引数NULLの場合は"")</returns>
        public static string IntToStr(int? i)
        {

            if (i == null)
            {
                return "";
            }
            else
            {
                return i.ToString();
            }
        }

        /// <summary>
        /// 文字列⇒数値変換
        /// </summary>
        /// <param name="str">数値を表す文字列</param>
        /// <returns>引数と同値を表す数値(引数NULL,空文字の場合はNULL)</returns>
        public static int? StrToInt(string str)
        {

            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            else
            {
                return int.Parse(str);
            }
        }

        /// <summary>
        /// 文字列⇒DateTime変換
        /// </summary>
        /// <param name="str">変換対象文字列</param>
        /// <returns>引数と同値を表すDateTime(変換対象がNULL/空文字、または変換不可能の場合はNULL)</returns>
        public static DateTime? StrToDateTime(string str)
        {
            return StrToDateTime(str, null);
        }

        /// <summary>
        /// 文字列⇒DateTime変換
        /// </summary>
        /// <param name="str">変換対象文字列</param>
        /// <param name="defaultDateTime">変換対象が変換不可能の場合の戻り値</param>
        /// <returns>引数と同値を表すDateTime(変換対象がNULL/空文字の場合はNULL)</returns>
        public static DateTime? StrToDateTime(string str, DateTime? defaultDateTime)
        {

            if (DefaultString(str).Equals(string.Empty))
            {
                return null;
            }
            else
            {
                DateTime dateTime;
                if (DateTime.TryParse(str, out dateTime))
                {
                    return dateTime;
                }
                else
                {
                    return defaultDateTime;
                }
            }
        }

        //DEL 2014/10/16 arc ishii 
        // 　車台番号  原動機型式で使用していたが、myReplacerに変更　
        //public static string myReplacer(Match m)
        //{
        //    return Strings.StrConv(m.Value, VbStrConv.Narrow, 0);
        //}
        /// <summary>
        /// 全角英数字カナを半角変換
        /// （"／"変換不可）
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        //public static string abc123ToHankaku(string s)
        //{

        //    Regex re = new Regex("[０-９Ａ-Ｚａ-ｚ：－　]+");
        //    string output = re.Replace(s, myReplacer);

        //    return output;
        //}

        
        /// <summary>
        /// //Mod 2015/02/16 arc yano パラメータがnullの場合を考慮
        /// 全角英数字カナを半角変換 ADD 2014/10/16 arc ishii 
        /// （"／"変換不可）
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// 
        public static string myReplacer(string s)
        {

            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            else
            {
                return Strings.StrConv(s, VbStrConv.Narrow, 0);
            }
        }

        /// <summary>
        /////Mod 2015/02/16 arc yano パラメータがnullの場合を考慮
        ///小文字から大文字に変換 ADD 2014/10/16 arc ishii 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string LowercaseToUppercase(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            else
            {
                return Strings.StrConv(str, VbStrConv.Uppercase);
            }   
        }

        /// <summary>
        /// 非NULL文字列変換
        /// </summary>
        /// <param name="obj">変換対象オブジェクト</param>
        /// <returns>非NULL文字列</returns>
        public static string DefaultString(object obj)
        {
            return DefaultString(obj, string.Empty);
        }

        /// <summary>
        /// 非NULL文字列変換
        /// </summary>
        /// <param name="obj">変換対象オブジェクト</param>
        /// <param name="defaultStr">変換対象がNULLの場合の戻り値</param>
        /// <returns>非NULL文字列</returns>
        public static string DefaultString(object obj, string defaultStr)
        {

            if (obj == null)
            {
                return DefaultString(defaultStr, string.Empty);
            }
            else
            {
                return obj.ToString();
            }
        }

        /// <summary>
        /// NULL空文字時のデフォルトHTML空白文字出力
        /// (NULL空文字以外はHTMLエンコード処理)
        /// </summary>
        /// <param name="obj">文字列</param>
        /// <param name="i">文字数</param>
        /// <returns>NULLまたは空文字の場合、1文字数分のnon-breaking space。左記以外の場合、HTMLエンコード処理された文字列</returns>
        public static string DefaultNbsp(Object obj)
        {
            return DefaultNbsp(obj, 1);
        }

        /// <summary>
        /// NULL空文字時のデフォルトHTML空白文字出力
        /// (NULL空文字以外はHTMLエンコード処理)
        /// </summary>
        /// <param name="obj">文字列</param>
        /// <param name="i">文字数</param>
        /// <returns>NULLまたは空文字の場合、指定された文字数分のnon-breaking space。左記以外の場合、HTMLエンコード処理された文字列</returns>
        public static string DefaultNbsp(Object obj, int i)
        {
            string str = CommonUtils.DefaultString(obj);
            return (string.IsNullOrEmpty(str) ? Nbsp(i) : HttpUtility.HtmlEncode(str));
        }

        /// <summary>
        /// HTML空白文字追加出力
        /// (HTMLエンコード処理あり)
        /// </summary>
        /// <param name="obj">文字列</param>
        /// <param name="len">文字数</param>
        /// <returns>指定された文字数までnon-breaking spaceを追加された、HTMLエンコード処理済み文字列</returns>
        public static string PadRightNbsp(Object obj, int len)
        {
            string str = CommonUtils.DefaultString(obj);
            return HttpUtility.HtmlEncode(str) + (len > str.Length ? Nbsp(len - str.Length) : "");
        }

        /// <summary>
        /// HTML空白文字出力
        /// </summary>
        /// <param name="i">文字数</param>
        /// <returns>文字数分のnon-breaking space</returns>
        public static string Nbsp(int i)
        {
            return new string(' ', i).Replace(" ", "&nbsp;");
        }

        /// <summary>
        /// 対象日の00:00:00.000取得
        ///   ※ミリ秒精度に関する処理はSQL使用前提。
        /// </summary>
        /// <param name="dtStr">対象日文字列</param>
        /// <returns>引数の00:00:00.000を表すDateTime
        ///          (対象日文字列変換対象がNULL/空文字または変換不可能の場合はNULL、
        ///           SQL使用可能日時最小値を下回る場合はSQL使用可能日時最小値)</returns>
        public static DateTime? GetDayStart(string dtStr)
        {
            return GetDayStart(dtStr, null);
        }

        /// <summary>
        /// 対象日の00:00:00.000取得
        ///   ※ミリ秒精度に関する処理はSQL使用前提。
        /// </summary>
        /// <param name="dtStr">対象日文字列</param>
        /// <param name="defaultDateTime">変換対象が変換不可能の場合の戻り値</param>
        /// <returns>引数の00:00:00.000を表すDateTime(対象日文字列がNULL/空文字の場合はNULL、SQL使用可能日時最小値を下回る場合はSQL使用可能日時最小値)</returns>
        public static DateTime? GetDayStart(string dtStr, DateTime? defaultDateTime)
        {

            if (DefaultString(dtStr).Equals(string.Empty))
            {
                return null;
            }

            DateTime? dt = StrToDateTime(dtStr);

            if (dt == null)
            {
                return defaultDateTime;
            }
            else
            {
                return GetDayStart(dt);
            }
        }

        /// <summary>
        /// 対象日の00:00:00.000取得
        ///   ※ミリ秒精度に関する処理はSQL使用前提。
        /// </summary>
        /// <param name="dt">対象日</param>
        /// <returns>引数の00:00:00.000を表すDateTime(対象日がNULLの場合はNULL、SQL使用可能日時最小値を下回る場合はSQL使用可能日時最小値)</returns>
        public static DateTime? GetDayStart(DateTime? dt)
        {

            if (dt == null)
            {
                return null;
            }
            else
            {
                DateTime ret = DateTime.Parse(string.Format("{0:yyyy/MM/dd}", dt) + " 00:00:00.000");
                if (ret.CompareTo(DaoConst.SQL_DATETIME_MIN) < 0)
                {
                    ret = DaoConst.SQL_DATETIME_MIN;
                }
                return ret;
            }
        }

        /// <summary>
        /// 対象日の23:59:59.997取得
        ///   ※ミリ秒精度に関する処理はSQL使用前提。
        /// </summary>
        /// <param name="dtStr">対象日文字列</param>
        /// <returns>引数の00:00:00.000を表すDateTime
        ///          (対象日文字列変換対象がNULL/空文字または変換不可能の場合はNULL、
        ///           SQL使用可能日時最大値を上回る場合はSQL使用可能日時最大値)</returns>
        public static DateTime? GetDayEnd(string dtStr)
        {
            return GetDayEnd(dtStr, null);
        }

        /// <summary>
        /// 対象日の23:59:59.997取得
        ///   ※ミリ秒精度に関する処理はSQL使用前提。
        /// </summary>
        /// <param name="dtStr">対象日文字列</param>
        /// <param name="defaultDateTime">変換対象が変換不可能の場合の戻り値</param>
        /// <returns>引数の00:00:00.000を表すDateTime(対象日文字列がNULL/空文字の場合はNULL、SQL使用可能日時最大値を上回る場合はSQL使用可能日時最大値)</returns>
        public static DateTime? GetDayEnd(string dtStr, DateTime? defaultDateTime)
        {

            if (DefaultString(dtStr).Equals(string.Empty))
            {
                return null;
            }

            DateTime? dt = StrToDateTime(dtStr);

            if (dt == null)
            {
                return defaultDateTime;
            }
            else
            {
                return GetDayEnd(dt);
            }
        }

        /// <summary>
        /// 対象日の23:59:59.997取得
        ///   ※ミリ秒精度に関する処理はSQL使用前提。
        ///     999でないのはSQLServerのミリ秒精度(3.33)により、999だと翌日の情報になってしまう為。
        ///     なお、上記精度により998は997に丸められる。
        /// </summary>
        /// <param name="dt">対象日</param>
        /// <returns>引数の23:59:59.997を表すDateTime(対象日がNULLの場合はNULL、SQL使用可能日時最大値を上回る場合はSQL使用可能日時最大値)</returns>
        public static DateTime? GetDayEnd(DateTime? dt)
        {

            if (dt == null)
            {
                return null;
            }
            else
            {
                DateTime ret = DateTime.Parse(string.Format("{0:yyyy/MM/dd}", dt) + " 23:59:59.997");
                if (ret.CompareTo(DaoConst.SQL_DATETIME_MAX) > 0)
                {
                    ret = DaoConst.SQL_DATETIME_MAX;
                }
                return ret;
            }
        }

        /// <summary>
        /// 半角英数チェック
        /// </summary>
        /// <param name="str">チェック対象文字列</param>
        /// <returns>True:チェック対象が半角英数のみ、または空文字/NULLの場合
        ///          False:チェック対象が半角英数以外の文字を含む場合</returns>
        public static bool IsAlphaNumeric(string str)
        {
            return Regex.IsMatch(DefaultString(str), @"^[0-9A-Za-z]*$");
        }

        /// <summary>
        /// 半角英数チェック(ハイフン許可)
        /// </summary>
        /// <param name="str">チェック対象文字列</param>
        /// <returns>True:チェック対象が半角英数のみ、または空文字/NULLの場合
        ///          False:チェック対象が半角英数以外の文字を含む場合</returns>
        public static bool IsAlphaNumericBar(string str)
        {
            return Regex.IsMatch(DefaultString(str), @"^[0-9A-Za-z\-]*$");
        }

        /// <summary>
        /// 半角英数チェック(ハイフン、アンダーバー許可)
        /// </summary>
        /// <param name="str">チェック対象文字列</param>
        /// <returns>True:チェック対象が半角英数のみ、または空文字/NULLの場合
        ///          False:チェック対象が半角英数以外の文字を含む場合</returns>
        /// <history>
        /// 2018/4/25 arc yano #3716 【車】グレードマスタのグレードコードに「－：ハイフン」「＿：アンダーバー」付き登録　新規追加
        /// </history>
        public static bool IsAlphaNumericBarUnderBar(string str)
        {
            return Regex.IsMatch(DefaultString(str), @"^[0-9A-Za-z\-_]*$");
        }
        /// <summary>
        /// 消費税額計算
        /// </summary>
        /// <param name="amount">計算対象金額</param>
        /// <returns>消費税額(計算対象金額がNULLの場合、NULL)</returns>
        /// MOD 2014/02/20 ookubo
        public static decimal? CalculateConsumptionTax(decimal? amount, int? rate)
        {
            return (amount == null ? (decimal?)null : Math.Truncate(decimal.Multiply((amount ?? 0m), (decimal)(rate / 100m))));
            //return (amount == null ? (decimal?)null : Math.Truncate(decimal.Multiply((amount ?? 0m), 0.05m)));
        }

        /// <summary>
        /// 指定した桁数で切り捨て処理を行う
        /// </summary>
        /// <param name="amount">数値</param>
        /// <param name="digit">桁数(千の位で切り捨てる場合は1000）</param>
        /// <returns>切り捨て後の数値</returns>
        public static decimal RoundDown(decimal amount, int digit)
        {
            return Math.Truncate(amount / digit) * digit;
        }

        /// <summary>
        /// 指定した月の末日を取得する
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns></returns>
        public static DateTime GetFinalDay(int year, int month)
        {
            DateTime firstDay = new DateTime(year, month, 1);
            return firstDay.AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// リフレクションでモデルのプロパティを取得する
        /// </summary>
        /// <param name="model">モデルのインスタンス</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>Value値</returns>
        public static object GetModelProperty(object model, string propertyName)
        {
            string[] properties = propertyName.Split('.');
            try
            {
                object refModel = model;
                for (int i = 0; i < properties.Length; i++)
                {
                    refModel = refModel.GetType().GetProperty(properties[i]).GetValue(refModel, null);
                }
                return refModel;
            }
            catch
            {
                return "";
            }

        }

        /// <summary>
        /// 入金予定日を取得する
        /// </summary>
        /// <param name="salesDate">決済日</param>
        /// <param name="customerClaim">請求先コード</param>
        /// <returns>入金予定日</returns>
        public static DateTime? GetPaymentPlanDate(DateTime salesDate, CustomerClaim customerClaim)
        {
            List<DateTime> planList = new List<DateTime>();

            //決済条件が複数ある場合直近の入金予定日とする
            if (customerClaim.CustomerClaimable != null)
            {
                for (int i = 0; i < customerClaim.CustomerClaimable.Count; i++)
                {

                    DateTime shimebi;

                    //当月の締め日
                    DateTime tougetsuShimebi;
                    int claimDay = customerClaim.CustomerClaimable[i].PaymentKind.ClaimDay;
                    if (claimDay == 0)
                    {
                        //末締め
                        tougetsuShimebi = GetFinalDay(salesDate.Year, salesDate.Month);
                    }
                    else
                    {
                        //指定日締め
                        tougetsuShimebi = new DateTime(salesDate.Year, salesDate.Month, claimDay);
                    }

                    //支払日
                    int paymentDay = customerClaim.CustomerClaimable[i].PaymentKind.PaymentDay;

                    //当月の締め日を過ぎているかどうか
                    if (DateTime.Today.CompareTo(tougetsuShimebi) < 0)
                    {
                        //当月の締め日で計算
                        shimebi = tougetsuShimebi;
                    }
                    else
                    {
                        //翌月の締め日で計算
                        shimebi = tougetsuShimebi.AddMonths(1);
                    }

                    //支払日を確定する
                    switch (customerClaim.CustomerClaimable[i].PaymentKind.PaymentType)
                    {
                        case "001": //当月払い
                            if (paymentDay == 0)
                            {
                                //末払い
                                planList.Add(GetFinalDay(salesDate.Year, salesDate.Month));
                            }
                            else
                            {
                                //指定日払い
                                planList.Add(new DateTime(salesDate.Year, salesDate.Month, paymentDay));
                            }
                            break;
                        case "002": //翌月払い
                            if (paymentDay == 0)
                            {
                                //末払い
                                planList.Add(GetFinalDay(salesDate.AddMonths(1).Year, salesDate.AddMonths(1).Month));
                            }
                            else
                            {
                                //指定日払い
                                planList.Add(new DateTime(salesDate.AddMonths(1).Year, salesDate.AddMonths(1).Month, paymentDay));
                            } break;
                        default:    //未設定
                            //planList.Add(new DateTime(shimebi.Year, shimebi.Month, shiharaibi));
                            break;
                    }
                }
            }

            return planList.Min<DateTime>();
        }

        /// <summary>
        /// 初度登録年月から経過年数を取得する
        /// </summary>
        /// <param name="yearMonth">初度登録年月(YYYY/MM)</param>
        /// <param name="requestregistdate">登録希望日</param>
        /// <history>
        /// 2019/10/17 yano #4022 【車両伝票入力】特定の条件下での環境性能割の計算の誤り
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業  全面的に改修
        /// </history>
        /// <returns></returns>
        public static decimal GetPassedYears(string yearMonth, DateTime? requestregistdate)
        {
            decimal yearsOld = 0m;

            string strregdate = yearMonth + "/1";

            //string birth = String.Format("{0:yyyyMMdd}", DateTime.Parse(strregdate));

            //string birth = yearMonth.Replace("/", "") + "01";

            //Mod 2019/10/17 yano #4022
            DateTime EndDate = (requestregistdate ?? DateTime.Today);       //取得日は登録希望日、または本日の日付

            //string today = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString("00") + "01";

            //Mod 2019/10/17 yano #4022
            if (DateTime.Parse(strregdate).Year == EndDate.Year) //初年度登録年と取得日が同一年の場合
            {
                yearsOld = 1m;
            }
            else
            {
                //初度登録年から取得日前年までの経過年数を算出
                yearsOld = (EndDate.Year - 1) - DateTime.Parse(strregdate).Year + 1;

                //算出した取得日前年までの経過年数に、以下を加算
                if (EndDate.CompareTo(new DateTime(EndDate.Year, 7, 1)) < 0) //取得日が
                {
                    yearsOld += 0.5m;
                }
                else //本日が7/1以降
                {
                    yearsOld += 1m;
                }
            }

            return yearsOld;

            //string strregdate = yearMonth + "/1";

            //string birth = String.Format("{0:yyyyMMdd}", DateTime.Parse(strregdate));

            ////string birth = yearMonth.Replace("/", "") + "01";
            //string today = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString("00") + "01";
            //decimal yearsOld = RoundDown((int.Parse(today) - int.Parse(birth)) / 10000, 1) + 1;

            ////今日が7月1日以前だったら0.5年、7月1日以降だったら1年足す
            //if (DateTime.Today.CompareTo(new DateTime(DateTime.Today.Year, 7, 1)) < 0)
            //{
            //    return yearsOld + 0.5m;
            //}
            //else
            //{
            //    return yearsOld + 1m;
            //}
        }

        /// <summary>
        /// 自動車税環境性能割を計算する
        /// </summary>
        /// <param name="amount">車両本体価格</param>
        /// <param name="optionAmount">メーカーオプション合計額</param>
        /// <param name="vehicleType">エンジン種別(001:普通、002:電気、003:ハイブリッド、004:ディーゼル、005:クリーンディーゼル、006:天然ガス)</param>
        /// <param name="newUsedType">新中区分(N:新車、U:中古車)</param>
        /// <param name="firstRegistYearMonth">初度登録年月</param>
        /// <param name="taxid">環境性能割税率ID</param>
        /// <param name="requestregistdate">登録希望日</param>
        /// <returns></returns>
        /// <history>
        /// 2020/11/17 yano #4065 【車両伝票入力】環境性能割・マスタの設定値が不正の場合の対応
        /// 2019/10/17 yano #4022 【車両伝票入力】特定の条件下での環境性能割の計算
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業　計算方法を全面的に変更
        /// </history>
        public static Tuple<string, decimal> GetAcquisitionTax(decimal amount, decimal optionAmount, string vehicleType, string newUsedType, string firstRegistYearMonth, string taxid = "", DateTime? requestregistdate = null)
        {
            decimal ? eprate = 0m;
            string epdiscounttaxid = "";
            decimal acquisitionTax = 0m;
            decimal rate = 0m;
            decimal baseAmount = 0m;                        //課税基準額
            decimal acquisitionPrice = 0m;                  //取得価格

            Tuple<string, decimal> retvalue = null;         //Mod 

            CrmsLinqDataContext db = new CrmsLinqDataContext();

            CodeDao dao = new CodeDao(db);

            //Mod 2019/09/04 yano #4011

            //------------------------
            //税率の設定
            //------------------------
            try
            {
                //-------------------------
                //本体車両価格の掛率の取得
                //-------------------------
                ConfigurationSettingDao configDao = new ConfigurationSettingDao(db);

                //本体車両価格に乗算する[設定値]の取得
                rate = (newUsedType.Equals("N") ? decimal.Parse(configDao.GetByKey("CarAcquisitionTaxNewRate").Value ?? "0") : decimal.Parse(configDao.GetByKey("CarAcquisitionTaxUsedRate").Value ?? "0"));

                //taxidがnullや空欄でない場合はその税率を設定する
                if (!string.IsNullOrWhiteSpace(taxid))
                {
                    epdiscounttaxid = taxid;
                    eprate = dao.GetEPDiscountTaxById(taxid).Rate;
                }
                else
                {
                    //-----------------------
                    //環境性能割・税率
                    //-----------------------
                    //エンジン種別
                    List<CodeData> epList = dao.GetEPDiscountTaxList(false, DateTime.Now);

                    CodeData rec = null;

                    switch (vehicleType)
                    {

                        //「電気」「クリーンディーゼル」「天然ガス」の場合は非課税 ※NULLは除く
                        case "002":
                        case "005":
                        case "006":
                            rec = epList.Where(x => !x.Value.Equals(null)).OrderBy(x => x.Value).FirstOrDefault();            //レートの一番低いものを選択
                            eprate = rec.Value;
                            epdiscounttaxid = rec.Code;
                            break;

                        //「普通」「ハイブリッド」「ディーゼル」「未設定」の場合はその時点の一番高いレートを選択(2019/10/01～2020/09/30は2%、以後は3%)
                        case "001":
                        case "003":
                        case "004":
                        default:
                            rec = epList.Where(x => !x.Value.Equals(null)).OrderByDescending(x => x.Value).FirstOrDefault();   //レートの一番高いものを選択
                            eprate = rec.Value;
                            epdiscounttaxid = rec.Code;
                            break;
                    }
                }
            }
            catch (NullReferenceException)
            {
            }

            //---------------------------
            //課税基準額の算出
            //---------------------------
            //(本体車両価格＋メーカーオプション価格）×掛率(=90%)
            baseAmount = (amount + optionAmount) * rate;

            //--------------------------
            //取得価格の算出
            //--------------------------
            //新車の場合
            if (newUsedType.Equals("N"))
            {
                //課税基準額 + メーカーオプション価格(1,000円未満は切り捨て）
                acquisitionPrice = RoundDown(baseAmount, 1000);
            }
            //中古車の場合は残価率を乗算する
            else
            {
                decimal yearsOld = 0;
                
                if (string.IsNullOrEmpty(firstRegistYearMonth))
                {
                    yearsOld = 1;
                }
                else
                {
                    yearsOld = GetPassedYears(firstRegistYearMonth, requestregistdate); //Mod 2019/10/17 yano #4022
                    //yearsOld = GetPassedYears(firstRegistYearMonth);

                    //Add 2020/11/17 yano #4065
                    //経過年数が不正（マイナスになる）場合はエラー値(マイナス)を設定してリターン
                    if (yearsOld <= 0)
                    {
                        epdiscounttaxid = "";
                        acquisitionTax = -99;

                        retvalue = new Tuple<string, decimal>(epdiscounttaxid, acquisitionTax);

                        return retvalue;
                    }
                }

                //経過年数7年以上は免税
                if (yearsOld >= 7)
                {
                    acquisitionPrice = 0;       //取得価格を0に設定
                }
                else
                {
                    //残価率
                    decimal remainRate = new CarAcquisitionTaxDao(db).GetRateByYears(yearsOld);

                    //取得価格 = 取得価格×残価率(1,000円未満は切り捨て）
                    acquisitionPrice = RoundDown(baseAmount * remainRate, 1000);
                }
            }

            //50万円以下は免税
            if (acquisitionPrice <= 500000)
            {
                acquisitionPrice = 0;   //取得価格を0に設定
            }
            

            //最後に環境性能割税率を乗算
            acquisitionTax = RoundDown((acquisitionPrice * (eprate ?? 0) / 100), 100);

            //リターン値を設定
            retvalue = new Tuple<string, decimal>(epdiscounttaxid, acquisitionTax);

            return retvalue;

            //try
            //{
            //    ConfigurationSettingDao configDao = new ConfigurationSettingDao(db);
            //    rate = decimal.Parse(configDao.GetByKey("CarAcquisitionTaxNewRate").Value ?? "0"); //9%
            //    hvRate = decimal.Parse(configDao.GetByKey("CarAcquisitionTaxHvRate").Value ?? "0");
            //    evRate = decimal.Parse(configDao.GetByKey("CarAcquisitionTaxEvRate").Value ?? "0");
            //    taxRate = decimal.Parse(configDao.GetByKey("CarAcquisitionTaxRate").Value ?? "0"); //5%
            //}
            //catch (NullReferenceException)
            //{
            //}

            //if (newUsedType.Equals("N"))
            //{
            //    //新車の場合


            //    switch (vehicleType)
            //    {
            //        case "001": //普通車
            //            acquisitionTax = CommonUtils.RoundDown((amount + optionAmount) * rate, 1000) * taxRate;
            //            break;
            //        case "002": //EV車
            //            acquisitionTax = amount * evRate;
            //            break;
            //        case "003": //HV車
            //            acquisitionTax = amount * hvRate;
            //            break;
            //        default: //設定なしは普通車
            //            acquisitionTax = RoundDown((amount + optionAmount) * rate, 1000) * taxRate;
            //            break;
            //    }
            //    //小数点以下切り捨て
            //    // 100円未満切り捨て(2011.07.11 T.Ryumura)
            //    acquisitionTax = RoundDown(acquisitionTax, 100);
            //}
            //else
            //{
            //    //中古車の場合
            //    decimal yearsOld = 0;
            //    if (string.IsNullOrEmpty(firstRegistYearMonth))
            //    {
            //        yearsOld = 1;
            //    }
            //    else
            //    {
            //        yearsOld = GetPassedYears(firstRegistYearMonth);
            //    }

            //    //経過年数7年以上は免税
            //    if (yearsOld >= 7)
            //    {
            //        acquisitionTax = 0;
            //    }
            //    else
            //    {
            //        //残価率
            //        decimal remainRate = new CarAcquisitionTaxDao(db).GetRateByYears(yearsOld);
            //        acquisitionTax = RoundDown((amount + optionAmount) * rate, 1000) * remainRate;

            //        //50万円以下は免税
            //        if (acquisitionTax <= 500000)
            //        {
            //            acquisitionTax = 0;
            //        }
            //        else
            //        {
            //            //小数点以下切り捨て
            //            // 100円未満切り捨て(2011.07.11 T.Ryumura)
            //            acquisitionTax = RoundDown(acquisitionTax * taxRate, 100);
            //        }
            //    }
            //}
            //return acquisitionTax;
        }

        /// <summary>
        /// 履歴テーブルにコピー
        /// </summary>
        /// <history>
        /// 2018/10/25 yano #3947 車両仕入入力　入力項目（古物取引時相手の確認方法）の追加
        /// 2015/10/14 arc nakayama #3257_車両マスタのデータ変更時車両履歴に項目をすべて反映する
        /// </history>
        public static void CopyToSalesCarHistory(CrmsLinqDataContext db, SalesCar salesCar)
        {
            SalesCarHistory salesCarHistory = new SalesCarHistory();
            salesCarHistory.SalesCarNumber = salesCar.SalesCarNumber;

            // 管理番号ごとにリビジョンを管理する
            List<SalesCarHistory> h_list = new SalesCarHistoryDao(db).GetByKey(salesCar.SalesCarNumber);
            int? rev = h_list.Count() == 0 ? 0 : h_list.Max(x => x.RevisionNumber);
            rev = (rev ?? 1) + 1;

            salesCarHistory.RevisionNumber = rev ?? 1;
            salesCarHistory.CarGradeCode = salesCar.CarGradeCode;
            salesCarHistory.NewUsedType = salesCar.NewUsedType;
            salesCarHistory.ColorType = salesCar.ColorType;
            salesCarHistory.ExteriorColorCode = salesCar.ExteriorColorCode;
            salesCarHistory.ExteriorColorName = salesCar.ExteriorColorName;
            salesCarHistory.ChangeColor = salesCar.ChangeColor;
            salesCarHistory.InteriorColorCode = salesCar.InteriorColorCode;
            salesCarHistory.InteriorColorName = salesCar.InteriorColorName;
            salesCarHistory.ManufacturingYear = salesCar.ManufacturingYear;
            salesCarHistory.CarStatus = salesCar.CarStatus;
            salesCarHistory.LocationCode = salesCar.LocationCode;
            salesCarHistory.OwnerCode = salesCar.OwnerCode;
            salesCarHistory.Steering = salesCar.Steering;
            salesCarHistory.SalesPrice = salesCar.SalesPrice;
            salesCarHistory.IssueDate = salesCar.IssueDate;
            salesCarHistory.MorterViecleOfficialCode = salesCar.MorterViecleOfficialCode;
            salesCarHistory.RegistrationNumberType = salesCar.RegistrationNumberType;
            salesCarHistory.RegistrationNumberKana = salesCar.RegistrationNumberKana;
            salesCarHistory.RegistrationNumberPlate = salesCar.RegistrationNumberPlate;
            salesCarHistory.RegistrationDate = salesCar.RegistrationDate;
            salesCarHistory.FirstRegistrationYear = salesCar.FirstRegistrationYear;
            salesCarHistory.CarClassification = salesCar.CarClassification;
            salesCarHistory.Usage = salesCar.Usage;
            salesCarHistory.UsageType = salesCar.UsageType;
            salesCarHistory.Figure = salesCar.Figure;
            salesCarHistory.MakerName = salesCar.MakerName;
            salesCarHistory.Capacity = salesCar.Capacity;
            salesCarHistory.MaximumLoadingWeight = salesCar.MaximumLoadingWeight;
            salesCarHistory.CarWeight = salesCar.CarWeight;
            salesCarHistory.TotalCarWeight = salesCar.TotalCarWeight;
            salesCarHistory.Vin = salesCar.Vin;
            salesCarHistory.Length = salesCar.Length;
            salesCarHistory.Width = salesCar.Width;
            salesCarHistory.Height = salesCar.Height;
            salesCarHistory.FFAxileWeight = salesCar.FFAxileWeight;
            salesCarHistory.FRAxileWeight = salesCar.FRAxileWeight;
            salesCarHistory.RFAxileWeight = salesCar.RFAxileWeight;
            salesCarHistory.RRAxileWeight = salesCar.RRAxileWeight;
            salesCarHistory.ModelName = salesCar.ModelName;
            salesCarHistory.EngineType = salesCar.EngineType;
            salesCarHistory.Displacement = salesCar.Displacement;
            salesCarHistory.Fuel = salesCar.Fuel;
            salesCarHistory.ModelSpecificateNumber = salesCar.ModelSpecificateNumber;
            salesCarHistory.ClassificationTypeNumber = salesCar.ClassificationTypeNumber;
            salesCarHistory.PossesorName = salesCar.PossesorName;
            salesCarHistory.PossesorAddress = salesCar.PossesorAddress;
            salesCarHistory.UserName = salesCar.UserName;
            salesCarHistory.UserAddress = salesCar.UserAddress;
            salesCarHistory.PrincipalPlace = salesCar.PrincipalPlace;
            salesCarHistory.ExpireType = salesCar.ExpireType;
            salesCarHistory.ExpireDate = salesCar.ExpireDate;
            salesCarHistory.Mileage = salesCar.Mileage;
            salesCarHistory.MileageUnit = salesCar.MileageUnit;
            salesCarHistory.Memo = salesCar.Memo;
            salesCarHistory.DocumentComplete = salesCar.DocumentComplete;
            salesCarHistory.DocumentRemarks = salesCar.DocumentRemarks;
            salesCarHistory.SalesDate = salesCar.SalesDate;
            salesCarHistory.InspectionDate = salesCar.InspectionDate;
            salesCarHistory.NextInspectionDate = salesCar.NextInspectionDate;
            salesCarHistory.UsVin = salesCar.UsVin;
            salesCarHistory.MakerWarranty = salesCar.MakerWarranty;
            salesCarHistory.RecordingNote = salesCar.RecordingNote;
            salesCarHistory.ProductionDate = salesCar.ProductionDate;
            salesCarHistory.ReparationRecord = salesCar.ReparationRecord;
            salesCarHistory.Oil = salesCar.Oil;
            salesCarHistory.Tire = salesCar.Tire;
            salesCarHistory.KeyCode = salesCar.KeyCode;
            salesCarHistory.AudioCode = salesCar.AudioCode;
            salesCarHistory.Import = salesCar.Import;
            salesCarHistory.Guarantee = salesCar.Guarantee;
            salesCarHistory.Instructions = salesCar.Instructions;
            salesCarHistory.Recycle = salesCar.Recycle;
            salesCarHistory.RecycleTicket = salesCar.RecycleTicket;
            salesCarHistory.CouponPresence = salesCar.CouponPresence;
            salesCarHistory.Light = salesCar.Light;
            salesCarHistory.Aw = salesCar.Aw;
            salesCarHistory.Aero = salesCar.Aero;
            salesCarHistory.Sr = salesCar.Sr;
            salesCarHistory.Cd = salesCar.Cd;
            salesCarHistory.Md = salesCar.Md;
            salesCarHistory.NaviType = salesCar.NaviType;
            salesCarHistory.NaviEquipment = salesCar.NaviEquipment;
            salesCarHistory.NaviDashboard = salesCar.NaviDashboard;
            salesCarHistory.SeatColor = salesCar.SeatColor;
            salesCarHistory.SeatType = salesCar.SeatType;
            salesCarHistory.Memo1 = salesCar.Memo1;
            salesCarHistory.Memo2 = salesCar.Memo2;
            salesCarHistory.Memo3 = salesCar.Memo3;
            salesCarHistory.Memo4 = salesCar.Memo4;
            salesCarHistory.Memo5 = salesCar.Memo5;
            salesCarHistory.Memo6 = salesCar.Memo6;
            salesCarHistory.Memo7 = salesCar.Memo7;
            salesCarHistory.Memo8 = salesCar.Memo8;
            salesCarHistory.Memo9 = salesCar.Memo9;
            salesCarHistory.Memo10 = salesCar.Memo10;
            salesCarHistory.DeclarationType = salesCar.DeclarationType;
            salesCarHistory.AcquisitionReason = salesCar.AcquisitionReason;
            salesCarHistory.TaxationTypeCarTax = salesCar.TaxationTypeCarTax;
            salesCarHistory.TaxationTypeAcquisitionTax = salesCar.TaxationTypeAcquisitionTax;
            salesCarHistory.CarTax = salesCar.CarTax;
            salesCarHistory.CarWeightTax = salesCar.CarWeightTax;
            salesCarHistory.CarLiabilityInsurance = salesCar.CarLiabilityInsurance;
            salesCarHistory.AcquisitionTax = salesCar.AcquisitionTax;
            salesCarHistory.RecycleDeposit = salesCar.RecycleDeposit;
            salesCarHistory.CreateEmployeeCode = salesCar.CreateEmployeeCode;
            salesCarHistory.CreateDate = salesCar.CreateDate;
            salesCarHistory.LastUpdateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
            salesCarHistory.LastUpdateDate = DateTime.Now;
            salesCarHistory.DelFlag = salesCar.DelFlag;
            salesCarHistory.EraseRegist = salesCar.EraseRegist;
            salesCarHistory.UserCode = salesCar.UserCode;
            salesCarHistory.ApprovedCarNumber = salesCar.ApprovedCarNumber;
            salesCarHistory.ApprovedCarWarrantyDateFrom = salesCar.ApprovedCarWarrantyDateFrom;
            salesCarHistory.ApprovedCarWarrantyDateTo = salesCar.ApprovedCarWarrantyDateTo;
            salesCarHistory.Finance = salesCar.Finance;
            //Add 2015/10/14 arc nakayama #3257_車両マスタのデータ変更時車両履歴に項目をすべて反映する (InspectGuidFlag, InspectGuidMemo, CarUsage, FirstRegistrationDate, CompanyRegistrationFlag)
            salesCarHistory.InspectGuidFlag = salesCar.InspectGuidFlag;
            salesCarHistory.InspectGuidMemo = salesCar.InspectGuidMemo;
            salesCarHistory.CarUsage = salesCar.CarUsage;
            salesCarHistory.FirstRegistrationDate = salesCar.FirstRegistrationDate;
            salesCarHistory.CompanyRegistrationFlag = salesCar.CompanyRegistrationFlag;

            salesCarHistory.OwnershipChangeType = salesCar.OwnershipChangeType;
            salesCarHistory.OwnershipChangeDate = salesCar.OwnershipChangeDate;
            salesCarHistory.OwnershipChangeMemo = salesCar.OwnershipChangeMemo;

            //Add 2018/10/25 yano #3947
            salesCarHistory.ConfirmDriverLicense = salesCar.ConfirmDriverLicense;
            salesCarHistory.ConfirmCertificationSeal = salesCar.ConfirmCertificationSeal;
            salesCarHistory.ConfirmOther = salesCar.ConfirmOther;

            db.SalesCarHistory.InsertOnSubmit(salesCarHistory);
        }
        public static void InsertAkakuroReason(CrmsLinqDataContext db, string slipNumber, string businessName, string reason)
        {


            AkakuroReason akakuroReason = new AkakuroReason();
            akakuroReason.SlipNumber = slipNumber;
            akakuroReason.BusinessName = businessName;
            akakuroReason.Reason = reason;
            akakuroReason.CreateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
            akakuroReason.CreateDate = DateTime.Now;
            db.AkakuroReason.InsertOnSubmit(akakuroReason);
        }


        // Add 2015/01/09 arc nakayama 顧客DM指摘事項④ 郵便番号の4桁目にハイフンがなければ入れる
        /// <summary>
        /// 郵便番号にハイフンを挿入する
        /// </summary>
        /// <param name="postcode">郵便番号</param>
        /// <returns></returns>

        public static string InsertHyphenInPostCode(string postcode)
        {
            if (!string.IsNullOrEmpty(postcode))
            {

                if (postcode.Substring(3, 1) != "-")
                {
                    postcode = postcode.Insert(3, "-");
                }
            }
            return postcode;
        }

        //Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる
        /// <summary>
        /// Webコンフィグに設定されているタイムアウト時間を取得する（Webコンフィグの設定値がDouble型に変換できなかった場合10分）
        /// </summary>
        /// <param name="postcode">なし</param>
        /// <returns></returns>
        public static double GetTimeOutMinutes()
        {
            double TimeOutMinutes = 0.0;
            double d;

            //Webコンフィグから設定値取得（String型）
            string stTimeOutMinutes = ConfigurationManager.AppSettings["CommonTimeOutMinutes"];
            if (double.TryParse(stTimeOutMinutes, out d)){
                TimeOutMinutes = double.Parse(stTimeOutMinutes);
            }else{
                //変換に失敗した場合は10分
                TimeOutMinutes = 10.0;
            }

            return TimeOutMinutes;
        }

       #region 部門コードから使用倉庫を取得する
       /// <summary>
       /// 部門コードから使用倉庫を取得する
       /// </summary>
       /// <param name="departmentCode"></param>
       /// <returns>倉庫</returns>
       /// <history>
       /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 新規追加
       /// </history>
       public static DepartmentWarehouse GetWarehouseFromDepartment(CrmsLinqDataContext db, string  deparmtmentCode)
       {
            //部門コードから使用倉庫を割出す
            DepartmentWarehouse dWarehouse = new DepartmentWarehouseDao(db).GetByDepartment(deparmtmentCode);

            return dWarehouse;
       }
      #endregion

       /// <summary>
       /// 更新ロックの設定
       /// </summary>
       /// <param name="db"></param>
       /// <param name="query"></param>
       /// <returns></returns>
       /// <history>
       /// 2017/04/11 arc yano #3753 Linqのクエリに更新ロックを設定
       /// </history>
       public static IEnumerable<T> SelectWithUpdlock<T>(System.Data.Linq.DataContext db, IQueryable<T> query)
       {
           var cmd = db.GetCommand(query.AsQueryable());
           string s = Regex.Replace(cmd.CommandText, @"(FROM .+ AS [^\s]+)\s", "$1 WITH (UPDLOCK)");
           var paras = cmd.Parameters.Cast<IDbDataParameter>().Select(p => p.Value).ToArray();
           return db.ExecuteQuery<T>(s, paras);
       }


       /// <summary>
       /// 現在日時の文字列作成
       /// </summary>
       /// <param name="db"></param>
       /// <param name="query"></param>
       /// <returns></returns>
       /// <history>
       /// 2018/04/09 arc yano #3757 キャッシュ問題 新規作成
       /// </history>
       public static string GetSystemTime()
       {
           string systetime = null;


           systetime = string.Format("{0:yyyyMMdd24hmmss}", DateTime.Now);

           return systetime;

       }

       #region 倉庫コードから部門を取得する
       /// <summary>
       /// 部門コードから使用倉庫を取得する
       /// </summary>
       /// <param name="warehouseCode"></param>
       /// <returns>部門</returns>
       /// <history>
       /// 2018/04/10 arc yano #3879 車両伝票　ロケーションマスタに部門コードを設定していないと、納車処理を行えない 新規作成
       /// </history>
       public static List<Department> GetDepartmentFromWarehouse(CrmsLinqDataContext db, string warehouseCode)
       {
           DepartmentWarehouse condition = new DepartmentWarehouse();

           condition.WarehouseCode = warehouseCode;

           List<Department> dList = new List<Department>();

           List<DepartmentWarehouse> dWarehouseList = new DepartmentWarehouseDao(db).GetByCondition(condition).OrderBy(x => x.DepartmentCode).ToList();

           foreach (var dw in dWarehouseList)
           {
               Department dep = new DepartmentDao(db).GetByKey(dw.DepartmentCode);

               if (dep != null)
               {
                   dList.Add(dep);
               }
           }

           return dList;
       }
       #endregion

       #region 車両引当解除メールの送信
       /// <summary>
       /// 引当解除メールの送信
       /// </summary>
       /// <param name="db">データコンテキスト</param>
       /// <param name="header">車両伝票ヘッダ</param>
       /// <param name="cancelPattern">キャンセル方法(2:登録・引当解除 99:引当解除</param>
       /// <param name="cause">引当解除となった要因</param>
       /// <param name="header">車両伝票ヘッダ</param>
       /// <history>
       /// 2018/08/07 yano #3911 登録済車両の車両伝票ステータス修正について　新規作成
       /// </history>
       public static void SendCancelReservationMail(CrmsLinqDataContext db, CarSalesHeader header, string cancelPattern, string cause)
       {
           // キャンセルメールを送信
           ConfigurationSetting config = new ConfigurationSettingDao(db).GetByKey("CancelReservatioMailAddress");

           if (config != null)
           {
               SendMail mail = new SendMail();

               Department department = new DepartmentDao(db).GetByKey(header.DepartmentCode);

               Employee employee = new EmployeeDao(db).GetByKey(header.EmployeeCode);

               c_NewUsedType newUsedType = new CodeDao(db).GetNewUsedTypeByKey(header.NewUsedType);

               Customer rec = new CustomerDao(db).GetByKey(header.CustomerCode);

               string customerName = rec != null ? rec.CustomerName : "";


               //キャンセル文言
               string strPattern = "";

               if (!string.IsNullOrWhiteSpace(cancelPattern) && cancelPattern.Equals(PTN_CANCEL_REGISTRATION))
               {
                   strPattern = "登録・引当";
               }
               else
               {
                   strPattern = "引当";
               }

               //要因
               string strCause = "";
               if (!string.IsNullOrWhiteSpace(cause) && cause.Equals(CANCEL_FROM_CARSLIPSTATUSCHANGE))
               {
                   strCause = "車両ステータス戻し処理による(受注→見積)";
               }
               else if (!string.IsNullOrWhiteSpace(cause) && cause.Equals(CANCEL_FROM_CANCEL))
               {
                   strCause = "受注後キャンセル処理による";
               }
                else
               {
                   strCause = "赤伝処理による";
               }

               string title = "【SYSTEM Information】  " + department.DepartmentName + "  " + strPattern + "解除";
               string msg = "■車両の" + strPattern + "が解除されました。\r\n";
               msg += "受注日   : " + string.Format("{0:yyyy/MM/dd}", header.SalesOrderDate) + "\r\n";
               msg += "伝票番号 : " + header.SlipNumber + "\r\n";
               msg += "顧客名 　: " + customerName + "\r\n";
               msg += "担当者 　: " + department.DepartmentName + ":" + employee.EmployeeName + "\r\n";
               msg += "車種     : " + header.MakerName + header.CarName + header.CarGradeName + "\r\n";
               msg += "色       : " + header.ExteriorColorName + "/" + header.InteriorColorName + "\r\n";
               msg += "車台番号 : " + header.Vin + "\r\n";
               msg += "新中区分 : " + newUsedType.Name + "\r\n";
               msg += "解除要因 : " + strCause + "\r\n";
               mail.Send(title, config.Value, msg);
           }
       }
       #endregion

       #region 税込金額から税抜金額を算出する
       /// <summary>
       /// 税込金額から税抜金額を算出する
       /// </summary>
       /// <param name="amountwithtax">税込金額</param>
       /// <param name="taxrate">消費税率(パーセント)</param>
       /// <param name="roundType">丸め処理方法(0:切り上げ 1:切り捨て 2:四捨五入)</param>
       /// <returns></returns>
       /// <history>
       /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 新規作成
       /// </history>
       public static decimal CalcAmountWithoutTax(decimal amountwithtax, int taxrate, int roundtype = 2)
       {
           decimal calcamount = 0m;

           //丸め処理方法により分岐
           switch (roundtype)
           {
               case 0:   //切り上げ
                   calcamount = (decimal)Math.Ceiling((double)amountwithtax * 100 / (100 + taxrate));
                   break;

               case 1:  //切り捨て
                   calcamount = (decimal)Math.Floor((double)amountwithtax * 100 / (100 + taxrate));
                   break;

               default: //四捨五入
                   calcamount = (decimal)Math.Round((double)amountwithtax * 100 / (100 + taxrate), MidpointRounding.AwayFromZero);
                   break; 
           }

           return calcamount;
       }
       #endregion
    }
}