using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Collections;
using System.ComponentModel;

namespace CrmsDao
{
    /// <summary>
    /// コードユーティリティクラス
    /// </summary>
    public class CodeUtils
    {
        /// <summary>
        /// 日
        /// </summary>
        private static readonly List<CodeData> _Day = new List<CodeData>() {
            new CodeData { Code = "0", Name = "月末"},
            new CodeData { Code = "1", Name = "1日"},
            new CodeData { Code = "2", Name = "2日"},
            new CodeData { Code = "3", Name = "3日"},
            new CodeData { Code = "4", Name = "4日"},
            new CodeData { Code = "5", Name = "5日"},
            new CodeData { Code = "6", Name = "6日"},
            new CodeData { Code = "7", Name = "7日"},
            new CodeData { Code = "8", Name = "8日"},
            new CodeData { Code = "9", Name = "9日"},
            new CodeData { Code = "10", Name = "10日"},
            new CodeData { Code = "11", Name = "11日"},
            new CodeData { Code = "12", Name = "12日"},
            new CodeData { Code = "13", Name = "13日"},
            new CodeData { Code = "14", Name = "14日"},
            new CodeData { Code = "15", Name = "15日"},
            new CodeData { Code = "16", Name = "16日"},
            new CodeData { Code = "17", Name = "17日"},
            new CodeData { Code = "18", Name = "18日"},
            new CodeData { Code = "19", Name = "19日"},
            new CodeData { Code = "20", Name = "20日"},
            new CodeData { Code = "21", Name = "21日"},
            new CodeData { Code = "22", Name = "22日"},
            new CodeData { Code = "23", Name = "23日"},
            new CodeData { Code = "24", Name = "24日"},
            new CodeData { Code = "25", Name = "25日"},
            new CodeData { Code = "26", Name = "26日"},
            new CodeData { Code = "27", Name = "27日"},
            new CodeData { Code = "28", Name = "28日"},
            new CodeData { Code = "29", Name = "29日"},
            new CodeData { Code = "30", Name = "30日"},
            new CodeData { Code = "31", Name = "31日"}
        };

        public static List<CodeData> Day
        {
            get { return CodeUtils._Day; }
        }

        /// <summary>
        /// 削除フラグ
        /// </summary>
        private static readonly List<CodeData> _DelFlag = new List<CodeData>() {
            new CodeData { Code = "0", Name = "有効"},
            new CodeData { Code = "1", Name = "無効"}
        };

        public static List<CodeData> DelFlag
        {
            get { return CodeUtils._DelFlag; }
        }

        /// <summary>
        /// 外注フラグ
        /// </summary>
        private static readonly List<CodeData> _OutsourceFlag = new List<CodeData>() {
            new CodeData { Code = "0", Name = "仕入先"},
            new CodeData { Code = "1", Name = "外注先"}
        };

        public static List<CodeData> OutsourceFlag
        {
            get { return CodeUtils._OutsourceFlag; }
        }

        /// <summary>
        /// 直近年月(1年分)リスト取得
        /// </summary>
        public static List<CodeData> GetYearMonthsList() {
            DateTime dateTime = DateTime.Now;
            return new List<CodeData>() {
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime), Name = string.Format("{0:yyyy/MM}", dateTime) },
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-1)), Name = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-1)) },
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-2)), Name = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-2)) },
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-3)), Name = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-3)) },
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-4)), Name = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-4)) },
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-5)), Name = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-5)) },
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-6)), Name = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-6)) },
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-7)), Name = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-7)) },
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-8)), Name = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-8)) },
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-9)), Name = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-9)) },
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-10)), Name = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-10)) },
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-11)), Name = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-11)) }
            };
        }

        /// <summary>
        /// 棚卸年月リスト取得
        /// </summary>
        public static List<CodeData> GetInventoryMonthsList() {
            DateTime dateTime = DateTime.Now;
            return new List<CodeData>() {
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-1)), Name = string.Format("{0:yyyy/MM}", dateTime.AddMonths(-1)) },
                new CodeData { Code = string.Format("{0:yyyy/MM}", dateTime), Name = string.Format("{0:yyyy/MM}", dateTime) }
            };
        }

        /// <summary>
        /// 管理帳票リスト取得
        /// </summary>
        /// <param name="id">ID</param>
        /// <history>
        /// 2019/01/07 yano #3965 WE版新システム対応（Web.configによる処理の分岐)
        /// 2017/09/04 arc yano #3786 預かり車Excle出力対応
        /// </history>
        public static List<CodeData> GetDocumentList(string id, CrmsLinqDataContext db) {
            
             //Mod 2019/01/07 yano #3965 DB定義に変更
             return new DocumentListDao(db).GetListByCategoryCode(id).OrderBy(x => x.DisplayOrder).Select(x => new CodeData { Code = x.DocumentCode , Name = x.DocumentName }).ToList();

            //switch (id) {
            //    case "1": //共通
            //        return new List<CodeData>(){
            //            new CodeData { Code = "ReceiptPlanList" , Name = "入金予定一覧"},
            //            new CodeData { Code = "ReceiptList", Name = "入金実績一覧" },
            //            new CodeData { Code = "PaymentPlanList" , Name = "支払予定一覧"},
            //            new CodeData { Code = "CarSalesList", Name = "車両伝票一覧"},
            //            new CodeData { Code = "ServiceSalesList", Name = "サービス伝票一覧"}
            //        };
                    
            //    case "2": //店舗
            //        return new List<CodeData>(){
            //            new CodeData { Code = "NewCustomerList", Name = "顧客増減分析" },
            //            new CodeData { Code = "ReceiptPlanList", Name = "入金予定一覧" },
            //            new CodeData { Code = "ReceiptionList", Name = "受付一覧" },
            //            new CodeData { Code = "CarSalesList", Name = "車両伝票一覧"},
            //            new CodeData { Code = "ServiceSalesList" , Name = "サービス伝票一覧"},
            //            new CodeData { Code = "PartsPurchaseList", Name = "部品入荷予定" },
            //            new CodeData { Code = "CarDM", Name = "車両DM" },
            //            new CodeData { Code = "ServiceDM", Name = "サービスDM" },
            //            new CodeData { Code = "ServiceDailyReport", Name = "サービス日報"},
            //            new CodeData { Code = "CSSurveyGR", Name = "CSサーベイ用入庫"},
            //            new CodeData { Code = "JournalList", Name = "入金消込一覧"},
            //            new CodeData { Code = "AccountReceivableBalanceList", Name = "売掛残高一覧"},
            //            new CodeData { Code = "CardReceiptPlanList", Name = "入金カード予定一覧"},
            //            new CodeData { Code = "PartsAverageCostList", Name = "移動平均単価一覧"}
            //        };
            //    case "3": //サービス(預かり車一覧)    //Add 2017/09/04 arc yano #3786
            //        return new List<CodeData>(){
            //            new CodeData { Code = "CarStorageList", Name = "預かり車一覧"}                
            //        };
            //    //DELETE arc ookubo #3132 経理財務＞業務帳票をメニューから削除
            //    //case "3": //業務用
            //    //    return new List<CodeData>(){
            //    //        new CodeData { Code = "CarStockList", Name = "車両在庫表" },
            //    //        new CodeData { Code = "PartsStockList", Name = "部品在庫表" },
            //    //        new CodeData { Code = "CarPurchaseOrderList", Name = "発注EXCELデータ" },
            //    //        new CodeData { Code = "CarStopList", Name = "預かり車両一覧" },
            //    //        new CodeData { Code = "PartsTransferList", Name = "部品出庫集計表" },
            //    //        new CodeData { Code = "DeadStockPartsList", Name = "デッドストック一覧" }
            //    //    };
            //    default:
            //        return null;
                
            //}
        }

        /// <summary>
        /// ナンバープレート種別取得
        /// </summary>
        /// <returns></returns>
        public static List<CodeData> GetRegistraionNumberTypeList() {
            return new List<CodeData>(){
                new CodeData { Code = "福岡", Name = "福岡"},
                new CodeData { Code = "北九州", Name = "久留米"},
                new CodeData { Code = "筑豊", Name = "筑豊"},
                new CodeData { Code = "品川", Name = "品川"},
                new CodeData { Code = "練馬", Name = "練馬"},
                new CodeData { Code = "足立", Name = "足立"},
                new CodeData { Code = "八王子", Name = "八王子"},
                new CodeData { Code = "多摩", Name = "多摩"},
                new CodeData { Code = "横浜", Name = "横浜"},
                new CodeData { Code = "川崎", Name = "川崎"},
                new CodeData { Code = "湘南", Name = "湘南"},
                new CodeData { Code = "相模", Name = "相模"}
            };
        }

        public static List<CodeData> GetFileTypeList() {
            return new List<CodeData>(){
                new CodeData{Code="TOPS車両",Name="TOPS車両"},
                new CodeData{Code="TOPSパーツ",Name="TOPSパーツ"}
            };
        }

        public static List<CodeData> GetLineTypeList() {
            return new List<CodeData>(){
                new CodeData{Code="001",Name="メカ"},
                new CodeData{Code="002",Name="外注"}
            };
        }

        /// <summary>
        /// 元号リスト取得
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
        /// </history>
        public static List<CodeData> GetGengouList(CrmsLinqDataContext db) {

            List<c_CodeName> list = new CodeDao(db).GetCodeName("025", false);

            List<CodeData> ret = new List<CodeData>();

            foreach (var l in list)
            {
                CodeData rec = new CodeData();

                rec.Code = l.Code;
                rec.Name = l.Name;

                ret.Add(rec);
            }

            //return new List<CodeData>(){

            //    //new CodeData{Code="1",Name="明治"},
            //    //new CodeData{Code="2",Name="大正"},
            //    //new CodeData{Code="4",Name="平成"},
            //    //new CodeData{Code="3",Name="昭和"}
                
            //};

            return ret;
        }

        //Add 2015/04/10 arc nakayama 顧客DM指摘事項修正Part2　住所再確認を検索条件に入れる
        //住所再確認
        public static List<CodeData> CustomerAddressReconfirmList()
        {
            return new List<CodeData>(){
                new CodeData{Code="",Name=""},
                new CodeData{Code="001",Name="要"},
                new CodeData{Code="002",Name="不要"}                
            };
        }

        //Add 2015/02/10 arc nakayama 売掛金管理対応
        //伝票種別(タイプ)

        public static List<CodeData> SlipTypeList()
        {
            return new List<CodeData>(){
                new CodeData{Code="2",Name=""},
                new CodeData{Code="0",Name="車両"},
                new CodeData{Code="1",Name="サービス"}
                
            };
        }

        /// <summary>
        /// コード名称取得
        /// </summary>
        /// <param name="codeList">コードデータリスト</param>
        /// <param name="code">コード値</param>
        /// <returns>コード名(存在しない場合"")</returns>
        public static string GetName(List<CodeData> codeList, string code)
        {
            foreach (CodeData data in codeList)
            {
                if (data.Code.Equals(code))
                {
                    return data.Name;
                }
            }
            return "";
        }

        /// <summary>
        /// モデルデータからのセレクトリスト取得
        ///   このメソッドは、コード項目名:"Code"，名称項目名:"Name"の場合に使用可能です。
        /// </summary>
        /// <param name="modelList">モデルデータ</param>
        /// <param name="selectedCode">選択済み要素のコード値</param>
        /// <param name="addBlank">空白行追加</param>
        /// <returns>セレクトリスト(引数のコード値の要素がselected状態)</returns>
        public static List<SelectListItem> GetSelectListByModel<T>(List<T> modelList, string selectedCode, bool addBlank) where T : ICrmsModel
        {
            if (modelList.Count == 0)
            {
                return new List<SelectListItem>();
            }
            else
            {
                string className = modelList[0].GetType().Name;
                return GetSelectListByModel(modelList, "Code", "Name", selectedCode, addBlank);
            }
        }

        /// <summary>
        /// モデルデータからのセレクトリスト取得
        /// </summary>
        /// <param name="modelList">モデルデータ</param>
        /// <param name="codeItem">モデルのコード値項目名</param>
        /// <param name="nameItem">モデルのコード名称項目名</param>
        /// <param name="selectedCode">選択済み要素のコード値</param>
        /// <param name="addBlank">空白行追加</param>
        /// <returns>セレクトリスト(引数のコード値の要素がselected状態)</returns>
        public static List<SelectListItem> GetSelectListByModel<T>(List<T> modelList, string codeItem, string nameItem, string selectedCode, bool addBlank) where T : ICrmsModel
        {
            List<CodeData> list = new List<CodeData>();
            foreach (Object model in modelList)
            {
                CodeData data = new CodeData();
                data.Code = (string)model.GetType().GetProperty(codeItem).GetValue(model, null);
                data.Name = (string)model.GetType().GetProperty(nameItem).GetValue(model, null);
                list.Add(data);
            }
            return GetSelectList(list, selectedCode, addBlank);
        }

        /// <summary>
        /// CodeDataからのセレクトリスト取得
        /// </summary>
        /// <param name="list">CodeDataリスト</param>
        /// <param name="code">選択済み要素のコード値</param>
        /// <param name="addBlank">空白行追加</param>
        /// <returns>セレクトリスト(引数のコード値の要素がselected状態)</returns>
        public static List<SelectListItem> GetSelectList(List<CodeData> list, string code, bool addBlank)
        {
            List<SelectListItem> ret = new List<SelectListItem>();
            if (addBlank)
            {
                ret.Add(new SelectListItem { Text = "", Value = "" });
            }
            foreach (CodeData data in list)
            {
                SelectListItem item = new SelectListItem { Text = data.Name, Value = data.Code };
                if ((!string.IsNullOrEmpty(code)) && (data.Code.Equals(code)))
                {
                    item.Selected = true;
                }
                ret.Add(item);
            }
            return ret;
        }

    }
}
