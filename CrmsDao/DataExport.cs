/// <summary>
/// エクセル用のデータを作成するクラスです。
/// </summary>
/// <remarks>
/// </remarks>
using System;
using System.Linq;
using System.Web;
using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml;
using System.Reflection;
using System.IO;
using System.Data;


namespace CrmsDao
{
    public class DataExport
    {

        //Add 2015/04/23 arc yano IPO対応(部品棚卸)
        /// <summary>
        /// エクセルファイルを作成する(テンプレートファイルなし)
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>excelFile</returns>
        public ExcelPackage MakeExcel(string fileName)
        {
            //エクセルファイル
            ExcelPackage excelFile;

            excelFile = new ExcelPackage();

            return excelFile;
        }

        /// <summary>
        /// エクセルファイルを作成する
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="templatePath">テンプレートファイルパス</param>
        /// <returns>excelFile</returns>
        public ExcelPackage MakeExcel(string fileName, string templatePath, ref bool tFileExists)
        {
            //エクセルファイル
            ExcelPackage excelFile;

            //テンプレートあり
            if (File.Exists(templatePath))
            {
                //FileInfo template = new FileInfo(templatePath);                 //テンプレートファイル
                //FileInfo newFile = new FileInfo(fileName);                      //ファイル

                
                FileStream newFile = new System.IO.FileStream(
                    fileName,
                    FileMode.Create,
                    System.IO.FileAccess.ReadWrite,
                    System.IO.FileShare.Read);

                //テンプレートを開く
                FileStream template = new System.IO.FileStream(
                    templatePath,
                    System.IO.FileMode.Open,
                    System.IO.FileAccess.Read,
                    System.IO.FileShare.ReadWrite);

                tFileExists = true;
                excelFile = new ExcelPackage(newFile, template);
            }
            //テンプレートなし
            else
            {
                tFileExists = false;
                excelFile = new ExcelPackage(); 
            }

            return excelFile;
        }

        /// <summary>
        /// エクセルファイルを削除する
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>excelFile</returns>
        public void DeleteFileStream(string fileName)
        {
            File.Delete(fileName);
        }


        /*
        /// <summary>
        /// エクセルデータの作成(テンプレート付)
        /// </summary>
        /// <param name="excelData">エクセル出力用データ(out)</param>
        /// <param name="dataList">データ</param>
        /// <param name="fileName">ファイル名</param>
        /// <param name="templatePath">テンプレートファイルのパス</param>
        /// <returns></returns>
        private bool MakeExcelDataIncludedTemplate(ref byte[] excelData, ArrayList dataList, string fileName, string templatePath)
        {

            FileInfo template = new FileInfo(templatePath);                 //テンプレートファイル
            FileInfo newFile = new FileInfo(fileName);                      //ファイル

            ExcelWorksheet configsheet = null;                              //設定シート
            ExcelWorksheet worksheet = null;                                //作業シート

            ConfigLine configLine = null;                                   //設定値(1行単位)

            //テンプレート付のファイルを作成する。
            using (var excelFile = new ExcelPackage(newFile, template))
            {
                //コンフィグシート読取
                configsheet = excelFile.Workbook.Worksheets["config"];

                //configシートなし
                if (configsheet == null)
                {
                    //エラーメッセージ表示
                    return false;
                }
                else　　　　　　　　　　　//configシートあり
                {
                    int i = 1;             //カウンタ

                    //設定値を取得
                    while (!string.IsNullOrWhiteSpace(configsheet.Cells[i, 1].Value.ToString()))
                    {

                        //設定値の取得
                        configLine = GetConfigLine(configsheet, i);

                        //設定ファイルが読み込めない場合はエラーを返す
                        if (configLine == null)
                        {
                            return false;
                        }

                        //作業ファイル設定
                        worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

                        //---------------------
                        //データ設定
                        //---------------------
                        switch (i)
                        {
                            //シート1
                            case 1:
                                SetData<T>(dataList, configLine, ref worksheet);
                                break;
                            //シート2
                            case 2:
                                SetData<T2>(dataList, configLine, ref worksheet);
                                break;

                            //シート3
                            case 3:
                                SetData<T3>(dataList, configLine, ref worksheet);
                                break;

                            //シート4
                            case 4:
                                SetData<T4>(dataList, configLine, ref worksheet);
                                break;

                            //シート5
                            case 5:
                                SetData<T5>(dataList, configLine, ref worksheet);
                                break;

                            //シート6
                            case 6:
                                SetData<T6>(dataList, configLine, ref worksheet);
                                break;

                            //シート7
                            case 7:
                                SetData<T7>(dataList, configLine, ref worksheet);
                                break;

                            //シート8
                            case 8:
                                SetData<T8>(dataList, configLine, ref worksheet);
                                break;

                            //シート9
                            case 9:
                                SetData<T9>(dataList, configLine, ref worksheet);
                                break;

                            default:
                                //何もしない
                                break;
                        }

                        i++;        //カウントアップ
                    }

                    if (i == 1) //configシートの設定値が1件も読み込めなかった場合、
                    {
                        return false;
                    }
                }

                excelData = excelFile.GetAsByteArray();
            }

            return true;
        }

        //テンプレートなし
        private bool MakeExcelData(ref byte[] excelData, ArrayList dataList)
        {
            //作業用シート
            ExcelWorksheet worksheet;
            bool ret = false;

            //エクセルファイルを作成する。
            using (var excelFile = new ExcelPackage())
            {
                //データ数分シートに出力
                for (int i = 0; i < dataList.Count; i++)
                {
                    //シート名の設定
                    string sheetName = "sheet" + i;

                    //シート追加
                    worksheet = excelFile.Workbook.Worksheets.Add(sheetName);

                    //configの設定
                    ConfigLine configLine = new ConfigLine();

                    //データソースのインデックス
                    configLine.DIndex = i;

                    //表示形式
                    configLine.Type = 0;

                    //開始位置
                    configLine.SetPos[0] = "A1";        //A1固定

                    switch (i)
                    {
                        //シート1
                        case 1:
                            SetData<T1>(dataList, configLine, ref worksheet);
                            break;
                        //シート2
                        case 2:
                            SetData<T2>(dataList, configLine, ref worksheet);
                            break;

                        //シート3
                        case 3:
                            SetData<T3>(dataList, configLine, ref worksheet);
                            break;

                        //シート4
                        case 4:
                            SetData<T4>(dataList, configLine, ref worksheet);
                            break;

                        //シート5
                        case 5:
                            SetData<T5>(dataList, configLine, ref worksheet);
                            break;

                        //シート6
                        case 6:
                            SetData<T6>(dataList, configLine, ref worksheet);
                            break;

                        //シート7
                        case 7:
                            SetData<T7>(dataList, configLine, ref worksheet);
                            break;

                        //シート8
                        case 8:
                            SetData<T8>(dataList, configLine, ref worksheet);
                            break;

                        //シート9
                        case 9:
                            SetData<T9>(dataList, configLine, ref worksheet);
                            break;

                        default:
                            //何もしない
                            break;
                    }
                }

                excelData = excelFile.GetAsByteArray();
            }

            return ret;
        }
        */

        /// <summary>
        /// configシートから読み取った設定値を返却する
        /// </summary>
        /// <param name="configSheet">configシート</param>
        /// <param name="linecnt">行数</param>
        /// <returns>設定値（シート単位）</returns>
        /// <history>
        /// 2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) データセット(行番号、列番号指定)を追加
        /// </history>
        public ConfigLine GetConfigLine(ExcelWorksheet configSheet, int columnLine)
        {
            ConfigLine configListLine = new ConfigLine();
            int i = 1;          //行の位置

            //データのインデックス
            configListLine.DIndex = configSheet.Cells[i, columnLine].GetValue<int>();
            i++;                //2

            //ワークシート名
            if (!string.IsNullOrWhiteSpace(configSheet.Cells[i, columnLine].GetValue<string>()))
            {
                configListLine.SheetName = configSheet.Cells[i, columnLine].GetValue<string>();
            }
            i++;                //3

            //タイプ
            if (!string.IsNullOrWhiteSpace(configSheet.Cells[i, columnLine].GetValue<string>()))
            {
                configListLine.Type = configSheet.Cells[i, columnLine].GetValue<int>();
            }
            i++;                //4

            //データセット位置
            while (!string.IsNullOrWhiteSpace(configSheet.Cells[i, columnLine].GetValue<string>()))
            {
                string confval = configSheet.Cells[i, columnLine].GetValue<string>();

                //Mod 2017/03/07 arc yano #3731
                //取得した値がアドレス指定(A1)か行／列番号指定(1,1)かをチェックする
                if (confval.Contains(","))      //行／列番号指定(1,1)
                {
                    //カンマ区切りで配列を作成
                    string[] strpos = confval.Split(',');

                    Tuple<int, int> pos = new Tuple<int, int>(int.Parse(strpos[0]), int.Parse(strpos[1]));

                    //設定値をリストに追加
                    configListLine.SetPosRowCol.Add(pos);
                }
                else //アドレス指定(A1)
                {
                //設定値をリストに追加
                configListLine.SetPos.Add(confval);
                }

                //次の行へ
                i++;
            }

            return configListLine;
        }

        /// <summary>
        /// configシートが無い場合の設定価の設定
        /// </summary>
        /// <param name="dIndex">インデックス</param>
        /// <param name="sheetName">シート名称</param>
        /// <param name="type">データタイプ</param>
        /// <param name="setPos">データ貼付位置</param>
        /// <returns>設定値（シート単位）</returns>
        public ConfigLine GetDefaultConfigLine(int dIndex, string sheetName, int type, string setPos)
        {
            ConfigLine configListLine = new ConfigLine();
            
            //データのインデックス
            configListLine.DIndex = dIndex;

            //ワークシート名
            configListLine.SheetName = sheetName;

            configListLine.Type = type;
           
            //設定値をリストに追加
            configListLine.SetPos.Add(setPos);

            return configListLine;
        }

        /// <summary>
        /// 設定値を元にデータをシートに書き込む(変換なし)
        /// </summary>
        /// <param name="excelFile">エクセルファイル(out)</param>
        /// <param name="dataList">データソース</param>
        /// <param name="configLine">設定値</param> 
        /// <returns>リターンコード(true/false)</returns>
        /// <history>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加 新規作成
        /// </history>
        public bool SetData<Tres>(ref ExcelPackage excelFile, List<Tres> dataList, ConfigLine configLine)
        {
            //リターン値
            bool ret = true;

            //データチェック(データの数が0の場合)
            if (dataList.Count <= 0)
            {
                //何もせずにリターン
                return ret;
            }
            
            //ワークシート
            ExcelWorksheet worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            //指定の名前のワークシートが存在しない場合は追加する。
            if (worksheet == null)
            {
                //ワークシート名がnullまたは空文字の場合
                if (string.IsNullOrWhiteSpace(configLine.SheetName))
                {
                    ret = false;
                    return ret;
                }
                worksheet = excelFile.Workbook.Worksheets.Add(configLine.SheetName);
            }

            //データ形式
            if (configLine.Type == 0)       //帳票形式
            {
                ExcelRange range;

                var fmt = dataList.ToArray();

                //Mod 2017/03/07 arc yano #3731
                //開始位置の設定
                //アドレス指定
                if (configLine.SetPos.Count > 0)
                {
                    range = worksheet.Cells[configLine.SetPos[0]];  //アドレス指定
                }
                else
                {
                    range = worksheet.Cells[configLine.SetPosRowCol[0].Item1, configLine.SetPosRowCol[0].Item2];    //行・列番号指定
                }

                //データ書き込み
                range.LoadFromCollection(Collection: fmt, PrintHeaders: false);
            }
            else if (configLine.Type == 1)  //単票形式
            {
                //渡されたクラスの型情報を取得する。
                Type t = typeof(Tres);

                //渡されたクラスのメンバ情報を取得する
                MemberInfo[] members = t.GetMembers(BindingFlags.Public | BindingFlags.DeclaredOnly |
                                                    BindingFlags.Instance | BindingFlags.Static);
                int k = 0;

                //行数
                for (int i = 0; i < dataList.Count; i++)
                {
                    //1レコード分取り出し
                    Tres slip = dataList[i];

                    //プロパティ値を取得する
                    foreach (MemberInfo m in members)
                    {
                        //メンバ型がプロパティなら
                        if (m.MemberType.ToString().Equals("Property"))
                        {
                            PropertyInfo pr = t.GetProperty(m.Name);

                            //Mod 2017/03/07 arc yano #3731
                            //開始位置の設定
                            //アドレス指定
                            if (configLine.SetPos.Count > 0)
                            {
                                //メンバの値を設定
                                worksheet.Cells[configLine.SetPos[k]].Value = (pr.GetValue(slip, null) == null ? "" : pr.GetValue(slip, null));

                                k++;

                                //Mod 2017/10/19 arc yano #3803 判定位置変更
                                //設定するデータの数が、configシートの定義数より多い場合
                                if (k >= configLine.SetPos.Count)
                                {
                                    ret = false;
                                    break;
                                }

                            }
                            else
                            {
                                worksheet.Cells[configLine.SetPosRowCol[k].Item1, configLine.SetPosRowCol[k].Item2].Value = (pr.GetValue(slip, null) == null ? "" : pr.GetValue(slip, null));    //行・列番号指定

                                //Add 2018/01/18 arc yano #3834
                                k++;

                                //Mod 2017/10/19 arc yano #3803 判定位置変更
                                //設定するデータの数が、configシートの定義数より多い場合
                                if (k >= configLine.SetPosRowCol.Count)
                                {
                                    ret = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else  //特殊帳票形式        //Add 2017/03/07 arc yano #3731
            {

                //初期設定
                int row = 0;            //行の位置
                int col = 0;            //列の位置
                int k = 0;              //カウンタ


                //移動行の設定(データセットの最後の定義の行番号－データセットの最初の定義の行番号 ※これにより１レコードにつきExcelで何行移動するかを算出)
                int maxRow = configLine.SetPosRowCol.Max(x => x.Item1);
                int minRow = configLine.SetPosRowCol.Min(x => x.Item1);

                int moverowcnt = maxRow - (minRow - 1);

                //渡されたクラスの型情報を取得する。
                Type t = typeof(Tres);

                //渡されたクラスのメンバ情報を取得する
                MemberInfo[] members = t.GetMembers(BindingFlags.Public | BindingFlags.DeclaredOnly |
                                                    BindingFlags.Instance | BindingFlags.Static);


                //行数
                for (int i = 0, rowcnt = 0; i < dataList.Count; i++, rowcnt += moverowcnt)
                {
                    k = 0;      //初期化
                    //1レコード分取り出し
                    Tres slip = dataList[i];

                    //プロパティ値を取得する
                    foreach (MemberInfo m in members)
                    {
                        //メンバ型がプロパティなら
                        if (m.MemberType.ToString().Equals("Property"))
                        {
                            PropertyInfo pr = t.GetProperty(m.Name);

                            //行・列番号の設定
                            row = configLine.SetPosRowCol[k].Item1 + rowcnt;
                            col = configLine.SetPosRowCol[k].Item2;

                            //メンバの値を設定
                            worksheet.Cells[row, col].Value = (pr.GetValue(slip, null) == null ? "" : pr.GetValue(slip, null));
                            k++;

                            //Mod 2017/10/19 arc yano #3803 判定位置変更
                            //設定するデータの数が、configシートの定義数より多い場合
                            if (k >= configLine.SetPosRowCol.Count)
                            {
                                ret = false;
                                break;
                            }
                        }
                    }
                }
            }



            return ret;
        }

        /// <summary>
        /// 設定値を元にデータをシートに書き込む
        /// </summary>
        /// <param name="excelFile">エクセルファイル(out)</param>
        /// <param name="dataList">データソース</param>
        /// <param name="configLine">設定値</param> 
        /// <returns>リターンコード(true/false)</returns>
        /// <history>
        /// 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 帳票形式でも行・列番号指定時のデータ設定処理を追加
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 データのプロパティが設定値より多い場合は設定できるところまで設定する
        /// 2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) 特殊帳票の処理を追加
        /// </history>
        public bool SetData<T, Tres>(ref ExcelPackage excelFile, List<T> dataList, ConfigLine configLine)
        {
            //リターン値
            bool ret = true;

            //データチェック(データの数が0の場合)
            if (dataList.Count <= 0)
            {
                //何もせずにリターン
                return ret;
            }

            //ワークシート
            ExcelWorksheet worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            //指定の名前のワークシートが存在しない場合は追加する。
            if (worksheet == null)
            {
                //ワークシート名がnullまたは空文字の場合
                if (string.IsNullOrWhiteSpace(configLine.SheetName))
                {
                    ret = false;
                    return ret;
                }
                worksheet = excelFile.Workbook.Worksheets.Add(configLine.SheetName);
            }

            //データ形式
            if (configLine.Type == 0)       //帳票形式
            {
                ExcelRange range;

                //Excel出力フォーマット用クラスにマッピング
                var fmt = Mapping<T>.ToArray<Tres>(dataList.ToArray());

                //Mod 2017/03/07 arc yano #3731
                //開始位置の設定
                //アドレス指定
                if (configLine.SetPos.Count > 0)
                {
                    range = worksheet.Cells[configLine.SetPos[0]];  //アドレス指定
                }
                else
                {
                    range = worksheet.Cells[configLine.SetPosRowCol[0].Item1, configLine.SetPosRowCol[0].Item2];    //行・列番号指定
                }

                //データ書き込み
                range.LoadFromCollection(Collection: fmt, PrintHeaders: false);
            }
            else if (configLine.Type == 1)  //単票形式
            {
                //渡されたクラスの型情報を取得する。
                Type t = typeof(T);

                //渡されたクラスのメンバ情報を取得する
                MemberInfo[] members = t.GetMembers(BindingFlags.Public | BindingFlags.DeclaredOnly |
                                                    BindingFlags.Instance | BindingFlags.Static);
                int k = 0;

                //行数
                for (int i = 0; i < dataList.Count; i++)
                {
                    //1レコード分取り出し
                    T slip = dataList[i];

                    //プロパティ値を取得する
                    foreach (MemberInfo m in members)
                    {
                        //メンバ型がプロパティなら
                        if (m.MemberType.ToString().Equals("Property"))
                        {
                            PropertyInfo pr = t.GetProperty(m.Name);

                            //Mod 2017/03/07 arc yano #3731
                            //開始位置の設定
                            //アドレス指定
                            if (configLine.SetPos.Count > 0)
                            {
                                //メンバの値を設定
                                worksheet.Cells[configLine.SetPos[k]].Value = (pr.GetValue(slip, null) == null ? "" : pr.GetValue(slip, null));

                                k++;

                                //Mod 2017/10/19 arc yano #3803 判定位置変更
                                //設定するデータの数が、configシートの定義数より多い場合
                                if (k >= configLine.SetPos.Count)
                                {
                                    ret = false;
                                    break;
                                }

                            }
                            else
                            {
                                worksheet.Cells[configLine.SetPosRowCol[k].Item1, configLine.SetPosRowCol[k].Item2].Value = (pr.GetValue(slip, null) == null ? "" : pr.GetValue(slip, null));    //行・列番号指定

                                //Add 2018/01/18 arc yano #3834
                                k++;

                                //Mod 2017/10/19 arc yano #3803 判定位置変更
                                //設定するデータの数が、configシートの定義数より多い場合
                                if (k >= configLine.SetPosRowCol.Count)
                                {
                                    ret = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else  //特殊帳票形式        //Add 2017/03/07 arc yano #3731
            {

                //初期設定
                int row = 0;            //行の位置
                int col = 0;            //列の位置
                int k = 0;              //カウンタ


                //移動行の設定(データセットの最後の定義の行番号－データセットの最初の定義の行番号 ※これにより１レコードにつきExcelで何行移動するかを算出)
                int maxRow = configLine.SetPosRowCol.Max(x => x.Item1);
                int minRow = configLine.SetPosRowCol.Min(x => x.Item1);

                int moverowcnt = maxRow - (minRow - 1);

                //渡されたクラスの型情報を取得する。
                Type t = typeof(T);

                //渡されたクラスのメンバ情報を取得する
                MemberInfo[] members = t.GetMembers(BindingFlags.Public | BindingFlags.DeclaredOnly |
                                                    BindingFlags.Instance | BindingFlags.Static);


                //行数
                for (int i = 0, rowcnt = 0; i < dataList.Count; i++, rowcnt += moverowcnt)
                {
                    k = 0;      //初期化
                    //1レコード分取り出し
                    T slip = dataList[i];

                    //プロパティ値を取得する
                    foreach (MemberInfo m in members)
                    {
                        //メンバ型がプロパティなら
                        if (m.MemberType.ToString().Equals("Property"))
                        {
                            PropertyInfo pr = t.GetProperty(m.Name);

                            //行・列番号の設定
                            row = configLine.SetPosRowCol[k].Item1 + rowcnt;
                            col = configLine.SetPosRowCol[k].Item2;

                            //メンバの値を設定
                            worksheet.Cells[row, col].Value = (pr.GetValue(slip, null) == null ? "" : pr.GetValue(slip, null));
                            k++;

                            //Mod 2017/10/19 arc yano #3803 判定位置変更
                            //設定するデータの数が、configシートの定義数より多い場合
                            if (k >= configLine.SetPosRowCol.Count)
                            {
                                ret = false;
                                break;
                            }
                        }
                    }
                }
            }



            return ret;
        }


        //Add 2015/04/23 arc yano DataTalbe型でもExcel出力できるように対応
        /// <summary>
        /// 設定値を元にデータをシートに書き込む
        /// </summary>
        /// <param name="excelFile">エクセルファイル(out)</param>
        /// <param name="dataList">データソース</param>
        /// <param name="configLine">設定値</param> 
        /// <returns>リターンコード(true/false)</returns>
        public bool SetData(ref ExcelPackage excelFile, DataTable dataList, ConfigLine configLine)
        {
            //リターン値
            bool ret = true;

            //データチェック(行数が0の場合)
            if (dataList.Rows.Count <= 0)
            {
                //何もせずにリターン
                return ret;
            }

            //ワークシート
            ExcelWorksheet worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            //指定の名前のワークシートが存在しない場合は追加する。
            if (worksheet == null)
            {
                //ワークシート名がnullまたは空文字の場合
                if (string.IsNullOrWhiteSpace(configLine.SheetName))
                {
                    ret = false;
                    return ret;
                }
                worksheet = excelFile.Workbook.Worksheets.Add(configLine.SheetName);
            }

            //データ形式
            if (configLine.Type == 0)       //帳票形式
            {

                //開始位置の設定
                var range = worksheet.Cells[configLine.SetPos[0]];

                //データ書き込み
                range.LoadFromDataTable(dataList, PrintHeaders: false);
            }
            else  //単票形式
            {
                int k = 0;

                //行数
                foreach (DataRow row in dataList.Rows)
                {
                    //設定するデータの数が、configシートの定義数より多い場合
                    if (k >= configLine.SetPos.Count)
                    {
                        ret = false;
                        break;
                    }
                    
                    //プロパティ値を取得する
                    foreach (DataColumn column in dataList.Columns)
                    {
                        //カラムの値を設定
                        worksheet.Cells[configLine.SetPos[k]].Value = row[column];
                        k++;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 行挿入
        /// </summary>
        /// <param name="excelFile">エクセルファイル(out)</param>
        /// <param name="configLine">設定値</param>
        /// <param name="inserttPos">挿入位置</param>
        /// <param name="insertRowCount">挿入行数</param>
        /// <param name="copyRow">書式をコピーする行</param>
        /// <returns>リターンコード(true/false)</returns>
        /// <history>
        /// 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 帳票形式でも行・列番号指定時のデータ設定処理を追加
        /// </history>
        public bool InsertRow(ref ExcelPackage excelFile, ConfigLine configLine, int insertPos,  int insertRowCount, int ? copyRow = null)
        {
            //リターン値
            bool ret = true;

            //行数チェックチェック(行数が0以下の場合)
            if (insertRowCount <= 0)
            {
                //何もせずにリターン
                ret = false;
            }
            else
            {
                //ワークシート
                ExcelWorksheet worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

                //コピー元行数が設定されている場合
                if (copyRow != null)
                {
                    worksheet.InsertRow(insertPos, insertRowCount, (copyRow ?? 0));
                }
                else
                {
                    worksheet.InsertRow(insertPos, insertRowCount);
                }
            }

            return ret;
        }

    }
}
