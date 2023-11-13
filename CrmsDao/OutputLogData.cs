using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    /// <summary>
    /// エラーログに出力する項目を格納するクラス
    /// 
    /// 新規作成 2014/08/01 arc amii エラーログ対応
    /// </summary>
    public class OutputLogData
    {
        // SQL文
        public static string sqlText = "";

        // Exception
        public static Exception exLog = null;

        // 処理名
        public static string procName = "";

    }
}
