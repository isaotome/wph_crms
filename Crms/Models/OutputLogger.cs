using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NLog;
using System.Web.Mvc;

namespace Crms.Models
{
    /// <summary>
    /// ログにメッセージを出力するクラス
    /// 
    /// 新規作成 2014/08/01 arc amii エラーログ対応
    /// </summary>
    public class OutputLogger
    {

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 発生した例外を「エラーLv：FATAL」として出力する
        /// </summary>
        /// <param name="ex">発生した例外</param>
        /// <param name="procName">処理名</param>
        /// <param name="formName">画面名</param>
        /// <param name="slipNo">伝票番号</param>
        public static void NLogFatal(Exception ex, string procName, string formName, string slipNo)
        {
            String message = "";

            if (string.IsNullOrEmpty(procName) == false)
            {
                message = procName + "処理でエラーが発生しました。";
            }

            if (string.IsNullOrEmpty(slipNo) == false)
            {
                message += "\r\n" + " 伝票番号： " + slipNo;
            }

            if (string.IsNullOrEmpty(formName) == false)
            {
                message += "\r\n" + " 画面名　：" + formName;
            }

            Logger.Fatal(message, ex);

        }

        /// <summary>
        /// 発生した例外を「エラーLv：FATAL」として出力する(Insert or Update以外の例外の場合のみ使用)
        /// </summary>
        /// <param name="ex">発生した例外</param>
        /// <param name="procName">処理名</param>
        /// <param name="uri">画面URL</param>
        public static void NLogFatalAttribute(Exception ex, string procName, string uri)
        {
            String message = "";

            if (string.IsNullOrEmpty(procName) == false)
            {
                message = procName + "処理でエラーが発生しました。";
            }

            if (string.IsNullOrEmpty(uri) == false)
            {
                message += "\r\n" + " URI     ： " + uri;
            }

            Logger.Fatal(message, ex);

        }

        /// <summary>
        /// 発生した例外を「エラーLv：ERROR」として出力する
        /// </summary>
        /// <param name="ex">発生した例外</param>
        /// <param name="procName">処理名</param>
        /// <param name="formName">画面名</param>
        /// <param name="slipNo">伝票番号</param>
        public static void NLogError(Exception ex, string procName, string formName, string slipNo)
        {
            String message = "";

            if (string.IsNullOrEmpty(procName) == false)
            {
                message = procName + "処理でエラーが発生しました。";
            }

            if (string.IsNullOrEmpty(slipNo) == false)
            {
                message += "\r\n" + " 伝票番号： " + slipNo;
            }

            if (string.IsNullOrEmpty(formName) == false)
            {
                message += "\r\n" + " 画面名　：" + formName;
            }

            Logger.Error(message, ex);
        }

        

        /// <summary>
        /// 設定した情報を「エラーLv：INFO」として出力する
        /// </summary>
        /// <param name="message"></param>
        public static void NLogInputDataInfo(FormCollection form, string formName, string procName)
        {
            string infoMessage = ""; // 出力メッセージ
            string key = "";
            string inputValue = "";
            infoMessage += "画面名　：" + formName;
            infoMessage += "\r\n" + " 処理名　：" + procName;

            infoMessage += "\r\n" + "--------------------　入力値　--------------------";
            
            for (int index = 0; index < form.Keys.Count; index++)
            {
                key = form.AllKeys[index];
                inputValue = form[key];

                infoMessage +=  "\r\n " + key + " : " + inputValue;
            }

            infoMessage += "\r\n" + "--------------------------------------------------";
            Logger.Info(infoMessage);
        }
    }
}