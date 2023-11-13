using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace CrmsDao
{
    public class DaoConst
    {
        /// <summary>
        /// SqlDateTime最小値
        /// </summary>
        public static readonly DateTime SQL_DATETIME_MIN = DateTime.Parse("1753/01/01 12:00:00.000");

        /// <summary>
        /// SqlDateTime最大値
        /// </summary>
        public static readonly DateTime SQL_DATETIME_MAX = DateTime.Parse("9999/12/31 23:59:59.997");

        /// <summary>
        /// 一意制約違反エラー番号
        /// </summary>
        public static readonly int DUP_VAL_ON_INDEX_ERROR = 2627;

        /// <summary>
        /// リトライ回数上限
        /// </summary>
        public static readonly int MAX_RETRY_COUNT = 10;

        /// <summary>
        /// 1ページ当りの表示件数
        /// </summary>
        /// 2014/07/04 arc ookubo warning対応(ConfigurationSettingsクラスは古い.netバージョン(v1.0,1.1)用であるため、変更する。)
        /// public static readonly int PAGE_SIZE = int.Parse(ConfigurationSettings.AppSettings["PageSize"] ?? "10");
        public static readonly int PAGE_SIZE = int.Parse(ConfigurationManager.AppSettings["PageSize"] ?? "10");

        /// <summary>
        /// 最初の締め日
        /// </summary>
        public static readonly DateTime FIRST_TARGET_DATE = new DateTime(2010, 6, 30);

        /// <summary>
        /// タスク管理用IDリスト
        /// </summary>
        public class TaskConfigId {

            /// <summary>
            /// 車両発注承認
            /// </summary>
            public static readonly string CAR_PURCHASE_APPROVAL = "101";

            /// <summary>
            /// 車両受注速報
            /// </summary>
            public static readonly string CAR_SALES_NEWS = "102";

            /// <summary>
            /// 車両引当依頼
            /// </summary>
            public static readonly string CAR_RESERVATION_REQUEST = "103";

            /// <summary>
            /// 車両引当確認
            /// </summary>
            public static readonly string CAR_RESERVATION_CONFIRM = "104";

            /// <summary>
            /// 車両登録確認
            /// </summary>
            public static readonly string CAR_REGISTRATION_CONFIRM = "105";

            /// <summary>
            /// 車両入庫予定
            /// </summary>
            public static readonly string CAR_RECEIPT_PLAN = "106";

            /// <summary>
            /// アフターフォロー（車両）
            /// </summary>
            public static readonly string CAR_AFTER_FOLLOW = "107";

            /// <summary>
            /// 車両査定
            /// </summary>
            public static readonly string CAR_APPRAISAL = "108";

            /// <summary>
            /// 車両仕入予定
            /// </summary>
            public static readonly string CAR_PURCHASE_PLAN = "109";

            /// <summary>
            /// 車両注文キャンセル
            /// </summary>
            public static readonly string CAR_CANCEL = "110";

            /// <summary>
            /// 車両見積期限超過
            /// </summary>
            public static readonly string CAR_QUOTE_EXPIRE = "111";

            /// <summary>
            /// 車両伝票過入金通知
            /// </summary>
            public static readonly string CAR_OVER_RECEIVE = "112";

            /// <summary>
            /// 部品発注承認
            /// </summary>
            public static readonly string PARTS_PURCHASE_APPROVAL = "201";

            /// <summary>
            /// 部品発注依頼
            /// </summary>
            public static readonly string PARTS_PURCHASE_REQUEST = "202";

            /// <summary>
            /// 部品仕入予定
            /// </summary>
            public static readonly string PARTS_PURCHASE_PLAN = "203";
            
            /// <summary>
            /// 部品入庫予定
            /// </summary>
            public static readonly string PARTS_RECEIPT_PLAN = "204";

            /// <summary>
            /// サービス伝票修正確認
            /// </summary>
            public static readonly string SERVICE_SALES_CHANGE = "205";

            /// <summary>
            /// 車両作業依頼
            /// </summary>
            public static readonly string SERVICE_REQUEST = "206";

            /// <summary>
            /// アフターフォロー（サービス）
            /// </summary>
            public static readonly string SERVICE_AFTER_FOLLOW = "207";

            /// <summary>
            /// サービスキャンセル
            /// </summary>
            public static readonly string SERVICE_CANCEL = "208";

            /// <summary>
            /// 部品変更通知
            /// </summary>
            public static readonly string PARTS_CHANGE = "209";

            /// <summary>
            /// サービス過入金通知
            /// </summary>
            public static readonly string SERVICE_OVER_RECEIVE = "210";
            
            /// <summary>
            /// ホット通知
            /// </summary>
            public static readonly string HOT = "901";

            /// <summary>
            /// 売掛金未回収
            /// </summary>
            public static readonly string RECEIPT_PLAN_EXPIRE = "902";

            /// <summary>
            /// 車検点検通知（営業）
            /// </summary>
            public static readonly string CAR_INSPECTION = "903";

            /// <summary>
            /// 車検点検通知（サービス）
            /// </summary>
            public static readonly string SERVICE_INSPECTION = "904";
        }
    }
}
