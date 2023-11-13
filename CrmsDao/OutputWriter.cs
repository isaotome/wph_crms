using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class OutputWriter : System.IO.TextWriter
    {
        public override System.Text.Encoding Encoding
        {
            get { return null; }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public OutputWriter()
        {
            OutputLogData.sqlText = "";
            OutputLogData.exLog = null;
            OutputLogData.procName = "";
        }

        public override void WriteLine(string value)
        {
            // ［出力］ウィンドウに表示
            System.Diagnostics.Debug.WriteLine(value);

            // Add 2014/08/01 arc amii エラーログ対応 ログに出力するSQL文を組み立てる処理追加
            // 既に設定されていた場合、次の値に改行コードを含める(ログ出力用)
            if (string.IsNullOrWhiteSpace(OutputLogData.sqlText) == false)
            {
                value = "\r\n" + value;
            }
            // SQL文 + パラメータを設定
            OutputLogData.sqlText = OutputLogData.sqlText + value;
        }
    }

}
