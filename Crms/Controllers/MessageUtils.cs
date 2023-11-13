using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Crms.Controllers
{
    /// <summary>
    /// メッセージユーティリティクラス
    /// </summary>
    public class MessageUtils
    {
        /// <summary>
        /// メッセージ定義
        /// </summary>
        private static readonly Dictionary<string, string> msg = new Dictionary<string, string>()
        {
            {"E0001", "{0}は入力必須です"},
            {"E0002", "{0}は、数値({1})入力必須です"},
            {"E0003", "{0}は、日付(YYYY/MM/DD)入力必須です"},
            {"E0004", "{0}には、数値({1})を入力して下さい"},
            {"E0005", "{0}には、日付(YYYY/MM/DD)を入力して下さい"},
            {"E0006", "登録データのキーが重複しています。キー項目({0})を変更して下さい"},
            {"E0007", "{0}の場合、{1}は入力必須です"},
            {"E0008", "{0}の場合、{1}は、数値({2})入力必須です"},
            {"E0009", "{0}の場合、{1}は、日付(YYYY/MM/DD)入力必須です"},
            {"E0010", "キー重複のデータが同時に登録されました。キー項目({0})を変更して再度{1}ボタンを押下して下さい"},
            {"E0011", "キー重複のデータが同時に登録されました。再度{0}ボタンを押下して下さい(データ内容変更不要です)"},
            {"E0012", "{0}には、半角英数字を入力して下さい"},
            {"E0013", "{0}には、{1}の日付(YYYY/MM/DD)を入力して下さい"},
            {"E0014", "{0}には、{1}行しか追加出来ません"},
            {"E0015", "{0}には、半角英数字及び.(ドット)を入力して下さい"},
            {"E0016", "{0}はマスタに存在しないコードです"},
            {"E0017", "既に締め処理が終了しています"},
            {"E0018", "{0}が大き過ぎて登録できません"},
            {"E0019", "{0}には、年月(YYYY/MM)を入力して下さい"},
            {"E0020", "{0}には、半角英数字または「- (ハイフン)」を入力して下さい"},
            {"E0021", "{0}には、和暦で正しい日付を入力して下さい"},
            {"E0022", "{0}の場合、{1}"},        //Add 2014/06/28 arc yano サービス伝票チェック新システム対応
            {"E0023", "{0}既に他ユーザによって登録されています。"},        //Add 2014/08/07 arc amii エラーログ対応
            {"E0024", "{0}"},                                              //Add 2014/08/27 arc yano IPO対応その２
            {"E0025", "{0}対象年月の部品棚卸が実施中です。部品棚卸基準日を変更することはできません。"}, //Add 2014/11/25 arc nakayama 部品棚卸作業日登録対応
            {"E0026", "{0}はハイフンなしの数字７桁、または、ハイフンありの8桁で入力して下さい"}, // Add 2015/01/09 arc nakayama 顧客DM指摘事項④ 登録時の郵便番号はハイフンなしで登録する
            {"E0027", "{0}対象年月前月の部品棚卸が実施中です。部品棚卸基準日を変更することはできません。"}, //Add 2015/04/22 arc nakayama 部品棚卸作業日登録画面修正
            {"E0028", "{0}には本日以降の日付を入力してください"}, //Add 2015/04/22 arc nakayama 部品棚卸作業日登録画面修正
            {"E0029", "{0}対象年月の部品棚卸が確定しています。部品棚卸基準日を変更することはできません。"}, //Add 2015/04/23 arc nakayama 部品棚卸作業日登録画面修正
            {"E0030", "{0}には、西暦で正しい日付を入力して下さい"},      // ADD 2016/02/08 ARC Mikami #3428_顧客マスタ_生年月日バリデーションチェック
            {"E0031", "{0}には、半角英数字または「- (ハイフン)」「_(アンダーバー)」を入力して下さい"},          //Add 2018/04/25 arc yano #3716
            {"E0032", "入力文字が長すぎます。{0}には、{1}文字以内で入力して下さい"},           //Add 2018/05/24 arc yano #3896 
            {"I0001", "保存しました"},                                   //Add 2014/10/23 arc ishii 保存ボタン対応
            {"I0002", "削除しました"},                                   //Add 2014/10/23 arc ishii 保存ボタン対応
            {"I0003", "{0}を行いました"},                      //Add 2014/10/23 arc ishii 保存ボタン対応
            {"dummy", "dummy"}
        };

        /// <summary>
        /// メッセージ取得
        /// </summary>
        /// <param name="msgID">メッセージID</param>
        /// <returns>メッセージ</returns>
        public static string GetMessage(string msgID)
        {
            return GetMessage(msgID, new string[]{});
        }

        /// <summary>
        /// メッセージ取得
        /// </summary>
        /// <param name="msgID">メッセージID</param>
        /// <param name="arg">置換文字列</param>
        /// <returns>メッセージ</returns>
        public static string GetMessage(string msgID, string arg)
        {
            return GetMessage(msgID, new string[]{arg});
        }

        /// <summary>
        /// メッセージ取得
        /// </summary>
        /// <param name="msgID">メッセージID</param>
        /// <param name="args">置換文字列</param>
        /// <returns>メッセージ</returns>
        public static string GetMessage(string msgID, string[] args)
        {
            return string.Format(msg[msgID] ?? "", args);
        }
    }
}
