/// <summary>
/// CSV(カンマ区切り)、TSV(タブ区切り)形式のファイルを書き出すクラスです。
/// </summary>
/// <remarks>
/// ・SQL Server 2005 Books Online CSV 出力のデザイン
///   http://msdn2.microsoft.com/ja-jp/library/ms155919.aspx
///     ・先頭のレコードに、レポートのすべての列のヘッダーが保持されます。 
///     ・すべての行の列数は同じです。
///     ・既定のフィールド区切り記号は、コンマ(,)です。
///     ・レコード区切り記号は、キャリッジリターンとラインフィードの組み合わせ(CRLF) です。
///     ・テキスト修飾子は、二重引用符(")です。
///     ・テキストに区切り記号や修飾子が埋め込まれている場合は、テキスト修飾子でテキストが囲まれ、テキスト中に埋め込まれた修飾子は2つ重ねて使用されます。
///     ・書式およびレイアウトは無視されます。
/// ・Common Format and MIME Type for Comma-Separated Values (CSV) Files
///   http://www.ietf.org/rfc/rfc4180.txt
///
/// 上記を踏まえた上でこのクラスのCSV出力仕様を決めます。
///   ・フィールド区切り記号は、カンマ(,)とする
///   ・レコード区切り記号は、キャリッジリターンとラインフィードの組み合わせ(CRLF)とする
///   ・テキスト修飾子は、ダブルクォート(")とする
///   ・テキストが以下の何れかを含む場合はテキスト修飾子(")で括る
///       ・キャリッジリターン(CR)
///       ・ラインフィード(LF)
///       ・テキスト修飾子(")
///       ・フィールド区切り記号(,)
///   ・テキストに含まれる空白(前、中、後)はそのまま出力する
///   ・行の最後にフィールド区切り記号(,)は付かない
///   ・テキストにテキスト修飾子(")が含まれる場合はエスケープ("")する
/// </remarks>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Data;
using System.Reflection;
using System.Xml.Linq;

namespace CrmsDao {

        public class SeparatedValueWriter {

            #region 新しいSeparatedValueWriterを構築します
            /// <summary>
            /// 新しいSeparatedValueWriterを構築します。
            /// </summary>
            /// <param name="context">データコンテキスト</param>
            /// <param name="separator">フィールド区切り記号</param>
            public SeparatedValueWriter(FieldSeparator separator) {
                FieldSeparator = separator.ToString();
            }
            #endregion

            #region GetBuffer
            /// <summary>
            /// モデルの要素をSeparated-Value形式に変換して取得します
            /// </summary>
            /// <param name="model">モデルデータ</param>
            /// <param name="fields">XMLフィールド定義リスト</param>
            public string GetBuffer<T>(List<T> model, IEnumerable<XElement> fields, DocumentExportCondition condition) {

                //戻り値用バッファ
                string buffer = "";

                //出力バッファ用コレクション
                StringCollection collection = new StringCollection();

                //抽出条件出力

                //Add 2014/08/20 arc yano IPO対応 「種別」追加
                if (!string.IsNullOrEmpty(condition.DataType))
                {
                    collection.Add(string.Format("種別={0}", condition.DataType));
                }
                if (condition.TermFrom!=null || condition.TermTo!=null) {
                    collection.Add(string.Format("期間{0:yyyy/MM/dd}～{1:yyyy/MM/dd}",condition.TermFrom,condition.TermTo));
                }
                if(!string.IsNullOrEmpty(condition.SalesOrderStatus)){
                    collection.Add(string.Format("車両伝票ステータス={0}:{1}",condition.SalesOrderStatus,condition.SalesOrderStatusName));
                }
                if(!string.IsNullOrEmpty(condition.ServiceOrderStatus)){
                    collection.Add(string.Format("サービス伝票ステータス={0}:{1}",condition.ServiceOrderStatus,condition.ServiceOrderStatusName));
                }
                if (!string.IsNullOrEmpty(condition.CarStatus)) {
                    collection.Add(string.Format("車両ステータス={0}:{1}",condition.CarStatus,condition.CarStatusName));
                }
                if(!string.IsNullOrEmpty(condition.NewUsedType)){
                    collection.Add(string.Format("新中区分={0}:{1}",condition.NewUsedType,condition.NewUsedTypeName));
                }
                if(!string.IsNullOrEmpty(condition.CustomerRank)){
                    collection.Add(string.Format("顧客ランク={0}:{1}",condition.CustomerRank,condition.CustomerRankName));
                }
                if(!string.IsNullOrEmpty(condition.CustomerType)){
                    collection.Add(string.Format("顧客区分={0}:{1}",condition.CustomerType,condition.CustomerTypeName));
                }
                if(!string.IsNullOrEmpty(condition.CustomerClaimType)){
                    collection.Add(string.Format("請求先種別={0}:{1}",condition.CustomerClaimType,condition.CustomerClaimTypeName));
                }
                if(!string.IsNullOrEmpty(condition.ReceiptPlanType)){
                    collection.Add(string.Format("入金予定種別={0}:{1}",condition.ReceiptPlanType,condition.ReceiptPlanTypeName));
                }
                if(!string.IsNullOrEmpty(condition.PaymentPlanType)){
                    collection.Add(string.Format("支払予定種別={0}:{1}",condition.PaymentPlanType,condition.PaymentPlanTypeName));
                }
                if(!string.IsNullOrEmpty(condition.EmployeeCode)){
                    collection.Add(string.Format("担当者={0}:{1}",condition.EmployeeCode,condition.EmployeeName));
                }
                if(!string.IsNullOrEmpty(condition.DepartmentCode)){
                    collection.Add(string.Format("部門={0}:{1}",condition.DepartmentCode,condition.DepartmentName));
                }
                if(!string.IsNullOrEmpty(condition.OfficeCode)){
                    collection.Add(string.Format("事業所={0}:{1}",condition.OfficeCode,condition.OfficeName));
                }
                if(!string.IsNullOrEmpty(condition.SlipNumber)){
                    collection.Add(string.Format("伝票番号={0}",condition.SlipNumber));
                }
                if(!string.IsNullOrEmpty(condition.CustomerName)){
                    collection.Add(string.Format("顧客名={0}",condition.CustomerName));
                }
                if(!string.IsNullOrEmpty(condition.SupplierCode)){
                    collection.Add(string.Format("仕入先={0}:{1}",condition.SupplierCode,condition.SupplierName));
                }
                if(condition.LastUpdateDate!=null){
                    collection.Add(string.Format("最終更新日={0:yyyy/MM/dd}",condition.LastUpdateDate));
                }
                if(!string.IsNullOrEmpty(condition.LocationType)){
                    collection.Add(string.Format("ロケーション種別={0}:{1}",condition.LocationType,condition.LocationTypeNane));
                }
                if(!string.IsNullOrEmpty(condition.CustomerClaimCode)){
                    collection.Add(string.Format("請求先={0}:{1}",condition.CustomerClaimCode,condition.CustomerClaimName));
                }
                if (condition.DepartmentList != null) {
                    for(int i=0;i<condition.DepartmentList.Count;i++) {
                        collection.Add(string.Format("拠点{0}={1}:{2}", i+1, condition.DepartmentList[i].DepartmentCode, condition.DepartmentList[i].DepartmentName));
                    }
                }
                if (!string.IsNullOrEmpty(condition.FirstRegistrationFrom) || !string.IsNullOrEmpty(condition.FirstRegistrationTo)) {
                    collection.Add(string.Format("初年度登録={0}～{1}", condition.FirstRegistrationFrom, condition.FirstRegistrationTo));
                }
                if (condition.ExpireDateFrom != null || condition.ExpireDateTo != null) {
                    collection.Add(string.Format("次回車検日（車検有効期限）={0:yyyy/MM/dd}～{1:yyyy/MM/dd}", condition.ExpireDateFrom, condition.ExpireDateTo));
                }
                if (condition.NextInspectionDateFrom != null || condition.NextInspectionDateTo != null) {
                    collection.Add(string.Format("次回点検日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}", condition.NextInspectionDateFrom, condition.NextInspectionDateTo));
                }
                if (condition.RegistrationDateFrom != null || condition.RegistrationDateTo != null) {
                    collection.Add(string.Format("登録年月日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}", condition.RegistrationDateFrom, condition.RegistrationDateTo));
                }
                if (condition.SalesDateFrom != null || condition.SalesDateTo != null) {
                    collection.Add(string.Format("納車年月日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}", condition.SalesDateFrom, condition.SalesDateTo));
                }
                if (condition.SalesOrderDateFrom != null || condition.SalesOrderDateTo != null) {
                    collection.Add(string.Format("受注年月日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}", condition.SalesOrderDateFrom, condition.SalesOrderDateTo));
                }
                if (!string.IsNullOrEmpty(condition.DmFlag)) {
                    collection.Add(string.Format("営業DM可否={0}", condition.DmFlagName));
                }
                if (condition.FirstReceiptionDateFrom != null || condition.FirstReceiptionDateTo != null) {
                    collection.Add(string.Format("初回来店日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}", condition.FirstReceiptionDateFrom, condition.FirstReceiptionDateTo));
                }
                if (condition.LastReceiptionDateFrom != null || condition.LastReceiptionDateTo != null) {
                    collection.Add(string.Format("前回来店日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}", condition.LastReceiptionDateFrom, condition.LastReceiptionDateTo));
                }
                if (condition.InterestedCustomer) {
                    collection.Add(string.Format("見込み客={0}", condition.InterestedCustomer));
                    if (condition.CarList != null) {
                        for (int i = 0; i < condition.CarList.Count; i++) {
                            collection.Add(string.Format("商談対象車両{0}={1}:{2}", i+1, condition.CarList[i].CarCode, condition.CarList[i].CarName));
                        }
                    }
                }
                if (!string.IsNullOrEmpty(condition.MorterViecleOfficialCode)) {
                    collection.Add(string.Format("ナンバープレート={0}", condition.MorterViecleOfficialCode));
                }
                if (!string.IsNullOrEmpty(condition.CarBrandCode)) {
                    collection.Add(string.Format("ブランド={0}:{1}", condition.CarBrandCode, condition.CarBrandName));
                }
                if (!string.IsNullOrEmpty(condition.CarCode)) {
                    collection.Add(string.Format("車種={0}:{1}", condition.CarCode, condition.CarName));
                }
                if (!string.IsNullOrEmpty(condition.CarGradeCode)) {
                    collection.Add(string.Format("グレード={0}:{1}", condition.CarGradeCode, condition.CarGradeName));
                }
                if (!string.IsNullOrEmpty(condition.CompanyCode)) {
                    collection.Add(string.Format("会社={0}:{1}", condition.CompanyCode, condition.CompanyName));
                }
                if (!string.IsNullOrEmpty(condition.TargetMonth)) {
                    collection.Add(string.Format("対象月={0}", condition.TargetMonth));
                }
                //Add 2014/08/06 arc yano IPO対応 csv出力項目追加
                if (!string.IsNullOrEmpty(condition.SalesCarNumber))
                {
                    collection.Add(string.Format("管理番号={0}", condition.SalesCarNumber));
                }
                if (!string.IsNullOrEmpty(condition.Vin))
                {
                    collection.Add(string.Format("車台番号={0}", condition.Vin));
                }
                //Add 2014/12/08 arc yano IPO対応(部品棚卸) csv出力項目追加
                if (condition.TargetDate != null)
                {
                    collection.Add(string.Format("対象年月={0:yyyy/MM/dd}", condition.TargetDate));
                }
                if (!string.IsNullOrEmpty(condition.LocationCode))
                {
                    collection.Add(string.Format("ロケーションコード={0}:{1}", condition.LocationCode, condition.LocationName));
                }
                if (string.IsNullOrEmpty(condition.LocationCode) && !string.IsNullOrEmpty(condition.LocationName))
                {
                    collection.Add(string.Format("ロケーション名={0}", condition.LocationName));
                }
                if (!string.IsNullOrEmpty(condition.PartsNumber))
                {
                    collection.Add(string.Format("部品番号={0}:{1}", condition.PartsNumber, condition.PartsNameJp));
                }
                if (string.IsNullOrEmpty(condition.PartsNumber) && !string.IsNullOrEmpty(condition.PartsNameJp))
                {
                    collection.Add(string.Format("部品名={0}", condition.PartsNameJp));
                }
                //Add 2014/12/18 arc nakayama 顧客DM対応 csv出力項目追加
                //Mod 2015/01/07 arc nakayama 顧客DM指摘事項⑩　備考のコメントアウト
                /*if (!string.IsNullOrEmpty(condition.DmMemo))
                {
                    collection.Add(string.Format("備考={0}", condition.DmMemo));
                }
                */
                if (!string.IsNullOrEmpty(condition.InspectGuidFlag))
                {
                    collection.Add(string.Format("車検案内DM可否={0}", condition.InspectGuidFlagName));
                }
                //Mod 2015/01/07 arc nakayama 顧客DM指摘事項⑩　備考のコメントアウト
                /*if (!string.IsNullOrEmpty(condition.InspectGuidMemo))
                {
                    collection.Add(string.Format("備考={0}", condition.InspectGuidMemo));
                }
                */
                if (!string.IsNullOrEmpty(condition.MakerName))
                {
                    collection.Add(string.Format("メーカー名={0}", condition.MakerName));
                }
                if (!string.IsNullOrEmpty(condition.CarName))
                {
                    collection.Add(string.Format("車種名={0}", condition.CarName));
                }
                if (condition.DtFirstRegistrationFrom != null || condition.DtFirstRegistrationTo != null)
                {
                    collection.Add(string.Format("初年度登録={0:yyyy/MM}～{1:yyyy/MM}", condition.DtFirstRegistrationFrom, condition.DtFirstRegistrationTo));
                }
                if (condition.ExpireDateFromForDm != null || condition.ExpireDateToForDm != null)
                {
                    collection.Add(string.Format("車検満了日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}", condition.ExpireDateFromForDm, condition.ExpireDateToForDm));
                }
                //Mod 2015/01/08 arc nakayama 顧客DM指摘事項⑦　登録年月日・次回点検日・車検満了日をFrom～Toで検索できるようにする すでに存在していたため初回で対応したコードは削除
                if (!string.IsNullOrEmpty(condition.SalesDepartmentCode))
                {
                    collection.Add(string.Format("営業部門コード={0}", condition.SalesDepartmentCode));
                }
                if (!string.IsNullOrEmpty(condition.SalesDepartmentName))
                {
                    collection.Add(string.Format("営業部門名={0}", condition.SalesDepartmentName));
                }
                if (!string.IsNullOrEmpty(condition.SalesEmployeeName))
                {
                    collection.Add(string.Format("担当者＜営業＞={0}", condition.SalesEmployeeName));
                }
                if (condition.SalesDateFromForDm != null || condition.SalesDateToForDm != null)
                {
                    collection.Add(string.Format("車両伝票納車日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}", condition.SalesDateFromForDm, condition.SalesDateToForDm));
                }
                if (!string.IsNullOrEmpty(condition.ServiceDepartmentCode))
                {
                    collection.Add(string.Format("サービス部門コード={0}", condition.ServiceDepartmentCode));
                }
                if (!string.IsNullOrEmpty(condition.ServiceDepartmentName))
                {
                    collection.Add(string.Format("サービス部門名={0}", condition.ServiceDepartmentName));
                }
                if (condition.ArrivalPlanDateFrom != null || condition.ArrivalPlanDateTo != null)
                {
                    collection.Add(string.Format("サービス伝票納車日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}", condition.ArrivalPlanDateFrom, condition.ArrivalPlanDateTo));
                }
                // Add 2015/02/13 arc nakayama 売掛金管理対応1
                if (!string.IsNullOrEmpty(condition.CustomerCode))
                {
                    collection.Add(string.Format("顧客コード={0}", condition.CustomerCode));
                }
                if (!string.IsNullOrEmpty(condition.SlipType))
                {
                    collection.Add(string.Format("伝票タイプ={0}", condition.SlipTypeName));
                }
                //Add 2015/02/16 arc iijima 現金出納帳出力検索項目追加
                if (!string.IsNullOrEmpty(condition.CashAccountCode))
                {
                    collection.Add(string.Format("現金口座名={0}", condition.CashAccountName));
                }
                //Add 2015/02/17 arc nakayama 顧客DM仕様変更対応（顧客マスタに紐づく項目追加、営業担当部門コード・営業担当者コード・営業担当者コード)
                if (!string.IsNullOrEmpty(condition.DepartmentCode2))
                {
                    collection.Add(string.Format("営業担当部門コード={0}:{1}", condition.DepartmentCode2, condition.DepartmentName2));
                }
                if (!string.IsNullOrEmpty(condition.CarEmployeeCode))
                {
                    collection.Add(string.Format("営業担当者={0}:{1}", condition.CarEmployeeCode, condition.CarEmployeeName));
                }
                if (!string.IsNullOrEmpty(condition.ServiceDepartmentCode2))
                {
                    collection.Add(string.Format("サービス担当部門コード={0}:{1}", condition.ServiceDepartmentCode2, condition.ServiceDepartmentName2));
                }
                //Add 2015/04/10 arc nakayama 顧客DM指摘事項修正Part2　住所再確認を検索条件に入れる
                if (condition.CustomerAddressReconfirm != null)
                {
                    collection.Add(string.Format("住所再確認={0}", condition.SearchAddressReconfirmName));
                }
                //Add 2015/04/14 arc nakayama 顧客DM追加項目　顧客種別を検索条件に入れる
                if (!string.IsNullOrEmpty(condition.CustomerKind))
                {
                    collection.Add(string.Format("顧客種別={0}", condition.CustomerKindName));
                }

                string[] separatedVlues3 = new string[collection.Count];
                collection.CopyTo(separatedVlues3, 0);
                buffer += string.Join(FieldSeparator, separatedVlues3) + RecordSeparator;

                collection.Clear();

                //ヘッダ出力
                foreach (var header in fields) {
                    collection.Add(header.Value);
                }
                string[] separatedValues = new string[collection.Count];
                collection.CopyTo(separatedValues, 0);

                //文字列を追加
                buffer += String.Join(FieldSeparator, separatedValues) + RecordSeparator;

                //明細レコードの出力
                foreach (var data in model) {
                    collection.Clear();
                    foreach (var line in fields) {
                        string value = "";
                        object ret = CommonUtils.GetModelProperty(data, line.Attribute("ID").Value);
                        //Edit vs2012対応 2014/05/09 arc yano 
                        //if (ret != null) {
                        if (ret != null)
                        {   
                            if (ret.ToString() != "")
                            {　                            
                                if (ret.GetType().Name.Contains("DateTime"))
                                {
                                    value = string.Format("{0:yyyy/MM/dd}", ret);
                                }
                                else
                                {
                                    value = ret.ToString();
                                }
                            }
                            else //空文字の場合
                            {
                                //Add vs2012対応 2014/04/24 arc ookubo
                                value = String.Empty;
                            }
                        }
                        else  //nullの場合
                        {
                            //Add vs2012対応 2014/05/09 arc yano
                            value = String.Empty;
                        }
                        // Mod 2015/01/14 arc nakayama 顧客DM指摘事項　出力結果にダブルコーテーションを入れる  
                        
                            //ダブルコーテーションあり
                            collection.Add(GetSeparatedValue2(value));
                    }
                    string[] separatedValues2 = new string[collection.Count];
                    collection.CopyTo(separatedValues2, 0);

                    //文字列を追加
                    buffer += String.Join(FieldSeparator, separatedValues2) + RecordSeparator;
                }

                return buffer;
            }
            #endregion
            #region GetBuffer2
            // Add 2014/10/17 arc amii サブシステム仕訳機能移行対応 専用の出力メソッド追加
            /// <summary>
            /// モデルの要素をSeparated-Value形式に変換して取得します
            /// </summary>
            /// <param name="model">モデルデータ</param>
            /// <param name="fields">XMLフィールド定義リスト</param>
            /// <param name="headerFlag">true:ヘッダを出力する(CSV)  false:ヘッダを出力しない(TEXT)</param>
            public string GetBuffer2<T>(List<T> model, IEnumerable<XElement> fields, bool headerFlag)
            {

                //戻り値用バッファ
                string buffer = "";

                //出力バッファ用コレクション
                StringCollection collection = new StringCollection();

                if (headerFlag == true)
                {
                    //ヘッダ出力
                    foreach (var header in fields)
                    {
                        collection.Add(header.Value);
                    }
                    string[] separatedValues = new string[collection.Count];
                    collection.CopyTo(separatedValues, 0);

                    //文字列を追加
                    buffer += String.Join(FieldSeparator, separatedValues) + RecordSeparator;
                }
                
                //明細レコードの出力
                foreach (var data in model)
                {
                    collection.Clear();
                    foreach (var line in fields)
                    {
                        string value = "";
                        object ret = CommonUtils.GetModelProperty(data, line.Attribute("ID").Value);
                        //Edit vs2012対応 2014/05/09 arc yano 
                        //if (ret != null) {
                        if (ret != null)
                        {
                            if (ret.ToString() != "")
                            {
                                if (ret.GetType().Name.Contains("DateTime"))
                                {
                                    value = string.Format("{0:yyyy/MM/dd}", ret);
                                }
                                else
                                {
                                    value = ret.ToString();
                                }
                            }
                            else //空文字の場合
                            {
                                value = String.Empty;
                            }
                        }
                        else  //nullの場合
                        {
                            value = String.Empty;
                        }

                        if (headerFlag == true)
                        {
                            // CSV出力の場合、ダブルコーテーションを文字列の開始と終了に付与した値を設定する
                            collection.Add(GetSeparatedValue2(value));

                        }
                        else
                        {
                            // TEXT出力の場合、ダブルコーテーション無の値を設定する
                            collection.Add(GetSeparatedValue(value));
                        }
                        
                    }
                    string[] separatedValues2 = new string[collection.Count];
                    collection.CopyTo(separatedValues2, 0);

                    //文字列を追加
                    buffer += String.Join(FieldSeparator, separatedValues2) + RecordSeparator;
                }

                return buffer;
            }
            #endregion
            #region モデルの各要素をSeparated-Value形式に変換します
            /// <summary>
            /// モデルの各要素をSeparated-Value形式に変換ます。
            /// </summary>
            /// <remarks>
            /// 要素は文字列に変換可能(ToString)である必要があります。
            /// 要素がnullの場合は空の文字列(String.Empty)として書き出します。
            /// </remarks>
            /// <param name="objectArray">書き出す配列</param>
            public string GetLine(object[] line) {
                StringCollection collection = new StringCollection();
                foreach (var data in line) {
                    collection.Add(GetSeparatedValue(data));
                }

                string[] separatedValues = new string[collection.Count];
                collection.CopyTo(separatedValues, 0);

                return String.Join(FieldSeparator, separatedValues) + RecordSeparator;
            }
            #endregion

            //========================================================================================================================
            //========================================================================================================================

            #region 引数のオブジェクトの文字列表現(ToString)をSeparated-Value形式で取得します
            /// <summary>
            /// 引数のオブジェクトの文字列表現(ToString)をSeparated-Value形式で取得します。
            /// </summary>
            /// <remarks>
            /// 値は文字列に変換可能(ToString)である必要があります。
            /// 値がnullの場合は空の文字列(String.Empty)として書き出します。
            /// 値がDBNullの場合は空の文字列(String.Empty)として書き出します。
            /// 
            /// ・NULL値の処理
            ///   http://msdn2.microsoft.com/ja-jp/library/ms172138.aspx
            /// ・DBNull.ToString()
            ///   http://msdn2.microsoft.com/ja-JP/library/zkbzs85t.aspx
            /// </remarks>
            /// <param name="obj">文字列表現(ToString)を取得するオブジェクト</param>
            /// <returns>Separated-Value形式の文字列</returns>
            private string GetSeparatedValue(object obj) {

                string separatedValue = null;

                if (obj == null) {
                    separatedValue = String.Empty;
                } else {
                    string value = obj.ToString();

                    if (ContainsLineBreaks(value) || ContainsTextModifier(value) || ContainsFieldSeparator(value)) {
                        value = value.Replace(TextModifier, TextModifier + TextModifier);
                        value = TextModifier + value + TextModifier;
                    }

                    separatedValue = value;
                }

                return separatedValue;
            }

            // Add 2014/10/17 arc amii サブシステム仕訳機能移行対応 専用の出力メソッド追加
            /// <summary>
            /// 引数のオブジェクトの文字列表現(ToString)をSeparated-Value形式で取得します。
            /// </summary>
            /// <remarks>
            /// 値は文字列に変換可能(ToString)である必要があります。
            /// 値がnullの場合は空の文字列(String.Empty)として書き出します。
            /// 値がDBNullの場合は空の文字列(String.Empty)として書き出します。
            /// 
            /// ・NULL値の処理
            ///   http://msdn2.microsoft.com/ja-jp/library/ms172138.aspx
            /// ・DBNull.ToString()
            ///   http://msdn2.microsoft.com/ja-JP/library/zkbzs85t.aspx
            /// </remarks>
            /// <param name="obj">文字列表現(ToString)を取得するオブジェクト</param>
            /// <returns>Separated-Value形式の文字列</returns>
            private string GetSeparatedValue2(object obj)
            {

                string separatedValue = null;

                if (obj == null)
                {
                    //separatedValue = String.Empty;
                    separatedValue = TextModifier + String.Empty + TextModifier;
                }
                else
                {
                    string value = obj.ToString();

                    if (ContainsLineBreaks(value) || ContainsTextModifier(value) || ContainsFieldSeparator(value))
                    {
                        value = value.Replace(TextModifier, TextModifier + TextModifier);
                        //value = TextModifier + value + TextModifier;
                    }

                    separatedValue = value = TextModifier + value + TextModifier; ;
                }

                return separatedValue;
            }
            #endregion

            /// <summary>
            /// プロパティの型が文字列型かどうかを判定します。
            /// </summary>
            /// <param name="model"></param>
            /// <param name="field"></param>
            /// <returns></returns>
            private bool IsStringField(PropertyInfo info) {
                string fieldType = info.PropertyType.Name;
                if (fieldType.Contains("String")) {
                    return true;
                } else {
                    return false;
                }
            }
            #region 引数の文字列がキャリッジリターン(CR)またはラインフィード(LF)を含むかどうかを判定します
            /// <summary>
            /// 引数の文字列がキャリッジリターン(CR)またはラインフィード(LF)を含むかどうかを判定します。
            /// </summary>
            /// <param name="value">判定する文字列</param>
            /// <returns>キャリッジリターン(CR)またはラインフィード(LF)を含む場合はtrueを返す。</returns>
            private bool ContainsLineBreaks(string value) {
                bool contains = false;

                if (value.Contains("\r") || value.Contains("\n")) {
                    contains = true;
                }

                return contains;
            }
            #endregion

            #region 引数の文字列がテキスト修飾子を含むかどうかを判定します
            /// <summary>
            /// 引数の文字列がテキスト修飾子を含むかどうかを判定します。
            /// </summary>
            /// <param name="value">判定する文字列</param>
            /// <returns>テキスト修飾子を含む場合はtrueを返す。</returns>
            private bool ContainsTextModifier(string value) {
                bool contains = false;

                if (value.Contains(TextModifier)) {
                    contains = true;
                }

                return contains;
            }
            #endregion

            #region 引数の文字列がフィールド区切り記号を含むかどうかを判定します
            /// <summary>
            /// 引数の文字列がフィールド区切り記号を含むかどうかを判定します。
            /// </summary>
            /// <param name="value">判定する文字列</param>
            /// <returns>フィールド区切り記号を含む場合はtrueを返す。</returns>
            private bool ContainsFieldSeparator(string value) {
                bool contains = false;

                if (value.Contains(FieldSeparator)) {
                    contains = true;
                }

                return contains;
            }
            #endregion

            //========================================================================================================================
            //========================================================================================================================

            #region テキスト修飾子です
            /// <summary>
            /// テキスト修飾子です。
            /// </summary>
            private const string TextModifier = "\"";
            #endregion

            #region レコード区切り記号です
            /// <summary>
            /// レコード区切り記号です。
            /// </summary>
            private const string RecordSeparator = "\r\n";
            #endregion

            #region フィールド区切り記号です
            /// <summary>
            /// フィールド区切り記号です。
            /// </summary>
            private readonly string FieldSeparator;
            #endregion
        }

        #region フィールド区切り記号を表すクラスです
        /// <summary>
        /// フィールド区切り記号を表すクラスです。
        /// </summary>
        public class FieldSeparator {
            #region CSV(カンマ区切り)のフィールド区切り記号を取得します
            /// <summary>
            /// CSV(カンマ区切り)のフィールド区切り記号を取得します。
            /// </summary>
            /// <remarks>
            /// このプロパティは読み取り専用です。
            /// </remarks>
            public static FieldSeparator CSV {
                get {
                    return new FieldSeparator(",");
                }
            }
            #endregion

            #region TSV(タブ区切り)のフィールド区切り記号を取得します
            /// <summary>
            /// TSV(タブ区切り)のフィールド区切り記号を取得します。
            /// </summary>
            /// <remarks>
            /// このプロパティは読み取り専用です。
            /// </remarks>
            public static FieldSeparator TSV {
                get {
                    return new FieldSeparator("\t");
                }
            }
            #endregion

            #region フィールド区切り記号の文字列表現を取得します
            /// <summary>
            /// フィールド区切り記号の文字列表現を取得します。
            /// </summary>
            /// <returns>フィールド区切り記号の文字列表現</returns>
            public override string ToString() {
                return separator;
            }
            #endregion

            #region 新しいFieldSeparatorを構築します
            /// <summary>
            /// 新しいFieldSeparatorを構築します。
            /// </summary>
            /// <param name="separator">フィールド区切り記号</param>
            private FieldSeparator(string separator) {
                this.separator = separator;
            }
            #endregion

            #region フィールド区切り記号です
            /// <summary>
            /// フィールド区切り記号です。
            /// </summary>
            private readonly string separator;
            #endregion
        }
        #endregion
    
}
