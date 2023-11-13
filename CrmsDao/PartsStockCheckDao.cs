using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Data.SqlClient;


namespace CrmsDao
{
    public　class PartsStockCheckDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext"></param>
        public PartsStockCheckDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }


        #region 定数
        private const string STS_CONFIRMED = "002"; // 確定(在庫棚卸ステータス)

        private const string CTG_PARTSTOCKSTS = "011"; // 在庫棚卸ステータス(c_CodeNameのカテゴリコード)

        #endregion

        /// <summary>
        /// 受払データ取得
        /// </summary>
        /// <param name="targetDateY">対象年月(年)</param>
        /// <param name="targetDateM">対象年月(月)</param>
        /// <param name="summaryMode">表示対象</param>
        /// <param name="warehouseCode">倉庫コード</param>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="partsNameJp">部品名称</param>
        /// <returns>受払い表(合計金額付)</returns>
        /// <history>
        /// 2016/12/09 arc yano #3672 部品在庫確認画面　一覧の表示順序がおかしい　ソート処理を追加
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更(引数の変更 departmentCode→warehouseCode)
        /// 2015/07/17 arc ayno IPO対応(部品棚卸) 障害対応、仕様変更⑦ 引当在庫の追加、各項目の数量合計も計算する
        /// 2015/06/03 arc yano 理論在庫を追加
        /// 2015/03/20 arc yano 部品在庫確認のデータ取得先の変更
        /// </history>
        public TotalPartsStockCheck GetPartsBalance(int targetDateY, int targetDateM, int summaryMode, string warehouseCode, string partsNumber, string partsNameJp)
        {

            //ストアドプロシージャ実行
            var result = db.GetPartsBalance(targetDateY, targetDateM, summaryMode, warehouseCode, partsNumber, partsNameJp).OrderBy(a => a.DepartmentCode).ThenBy(a => a.PartsNumber);

            TotalPartsStockCheck table = new TotalPartsStockCheck();

            List<PartsBalance> templist = new List<PartsBalance>();

            //初期化
            table.TotalPreAmount = 0;                   //月初在庫(金額合計)
            table.TotalPurchaseAmount = 0;              //当月仕入(金額合計)
            table.TotalTransferArrivalAmount = 0;       //当月移動受入(金額合計)
            table.TotalShipAmount = 0;                  //当月納車(金額合計)
            table.TotalTransferDepartureAmount = 0;     //当月移動払出(金額合計)
            table.TotalUnitPriceDifference = 0;         //単価差額(金額合計)
            table.TotalCalculatedAmount = 0;            //理論在庫(金額合計)      //Add 2015/06/03 arc yano
            table.TotalDifferenceAmount = 0;            //棚差(金額合計)
            table.TotalPostAmount = 0;                  //月末在庫(金額合計)
            table.TotalReservationAmount = 0;           //引当在庫(金額合計)      //Add 2015/07/17 arc yano
            table.TotalInProcessAmount = 0;             //仕掛在庫(金額合計)

            table.TotalPreQuantity = 0;                 //月初在庫(数量合計)
            table.TotalPurchaseQuantity = 0;            //当月仕入(数量合計)
            table.TotalTransferArrivalQuantity = 0;     //当月移動受入(数量合計)
            table.TotalShipQuantity = 0;                //当月納車(数量合計)
            table.TotalTransferDepartureQuantity = 0;   //当月移動払出(数量合計)
            table.TotalCalculatedQuantity = 0;          //理論在庫(数量合計)
            table.TotalDifferenceQuantity = 0;          //棚差(数量合計)
            table.TotalPostQuantity = 0;                //月末在庫(数量合計)
            table.TotalReservationQuantity = 0;         //引当在庫(数量合計)
            table.TotalInProcessQuantity = 0;           //仕掛在庫(数量合計)


            foreach (PartsBalance a in result)
            {
                PartsBalance line = new PartsBalance();

                line.CloseMonth = a.CloseMonth;
                //Mod 2016/08/13 arc yano #3596
                //line.DepartmentCode = a.DepartmentCode;
                //line.DepartmentName = a.DepartmentName;
                line.WarehouseCode = a.WarehouseCode;
                line.WarehouseName = a.WarehouseName;
                line.PartsNumber = a.PartsNumber;
                line.PartsNameJp = a.PartsNameJp;
                line.PreCost = a.PreCost;
                line.PreQuantity = a.PreQuantity;
                line.PreAmount = a.PreAmount;
                line.PurchaseQuantity = a.PurchaseQuantity;
                line.PurchaseAmount = a.PurchaseAmount;
                line.TransferArrivalQuantity = a.TransferArrivalQuantity;
                line.TransferArrivalAmount = a.TransferArrivalAmount;
                line.ShipQuantity = a.ShipQuantity;
                line.ShipAmount = a.ShipAmount;
                line.TransferDepartureQuantity = a.TransferDepartureQuantity;
                line.TransferDepartureAmount = a.TransferDepartureAmount;
                line.DifferenceQuantity = a.DifferenceQuantity;
                line.DifferenceAmount = a.DifferenceAmount;
                line.UnitPriceDifference = a.UnitPriceDifference;
                line.PostCost = a.PostCost;
                line.PostQuantity = a.PostQuantity;
                line.PostAmount = a.PostAmount;
                line.InProcessQuantity = a.InProcessQuantity;
                line.InProcessAmount = a.InProcessAmount;
                line.PurchaseOrderPrice = a.PurchaseOrderPrice;
                line.CalculatedDate = a.CalculatedDate;
                line.CalculatedQuantity = a.CalculatedQuantity;         //理論在庫(数量)　//Add 2015/06/03 arc yano
                line.CalculatedAmount = a.CalculatedAmount;             //理論在庫(金額)　//Add 2015/06/03 arc yano
                line.ReservationQuantity = a.ReservationQuantity;       //引当在庫(数量)　//Add 2015/07/17 arc yano
                line.ReservationAmount = a.ReservationAmount;           //引当在庫(金額)　//Add 2015/07/17 arc yano

                //合計金額の計算
                //月初在庫
                if(a.PreAmount != null)
                {
                    table.TotalPreAmount += a.PreAmount;
                }
                //当月仕入
                if (a.PurchaseAmount != null)
                {
                    table.TotalPurchaseAmount += a.PurchaseAmount;
                }
                //当月移動受入
                if (a.TransferArrivalAmount != null)
                {
                    table.TotalTransferArrivalAmount += a.TransferArrivalAmount;
                }
                //当月納車
                if (a.ShipAmount != null)
                {
                    table.TotalShipAmount += a.ShipAmount;
                }
                //当月移動払出
                if (a.TransferDepartureAmount != null)
                {
                    table.TotalTransferDepartureAmount += a.TransferDepartureAmount;
                }
                //棚差
                if (a.DifferenceAmount != null)
                {
                    table.TotalDifferenceAmount += a.DifferenceAmount;
                }
                //単価差額
                if (a.UnitPriceDifference != null)
                {
                    table.TotalUnitPriceDifference += a.UnitPriceDifference;
                }
                //月末在庫
                if (a.PostAmount != null)
                {
                    table.TotalPostAmount += a.PostAmount;
                }
                //仕掛在庫
                if (a.InProcessAmount != null)
                {
                    table.TotalInProcessAmount += a.InProcessAmount;
                }
                //理論在庫      //Add 2015/06/03 arc yano
                if (a.CalculatedAmount != null)
                {
                    table.TotalCalculatedAmount += a.CalculatedAmount;
                }

                //Add 2015/07/17 arc yano
                //引当在庫
                if (a.ReservationAmount != null)
                {
                    table.TotalReservationAmount += a.ReservationAmount;
                }

                //Add 2015/07/17
                //合計数量の計算
                //月初在庫
                if (a.PreQuantity != null)
                {
                    table.TotalPreQuantity += a.PreQuantity;
                }
                //当月仕入
                if (a.PurchaseQuantity != null)
                {
                    table.TotalPurchaseQuantity += a.PurchaseQuantity;
                }
                //当月移動受入
                if (a.TransferArrivalQuantity != null)
                {
                    table.TotalTransferArrivalQuantity += a.TransferArrivalQuantity;
                }
                //当月納車
                if (a.ShipQuantity != null)
                {
                    table.TotalShipQuantity += a.ShipQuantity;
                }
                //当月移動払出
                if (a.TransferDepartureQuantity != null)
                {
                    table.TotalTransferDepartureQuantity += a.TransferDepartureQuantity;
                }
                //棚差
                if (a.DifferenceQuantity != null)
                {
                    table.TotalDifferenceQuantity += a.DifferenceQuantity;
                }
                //月末在庫
                if (a.PostQuantity != null)
                {
                    table.TotalPostQuantity += a.PostQuantity;
                }
                //仕掛在庫
                if (a.InProcessQuantity != null)
                {
                    table.TotalInProcessQuantity += a.InProcessQuantity;
                }
                //理論在庫
                if (a.CalculatedQuantity != null)
                {
                    table.TotalCalculatedQuantity += a.CalculatedQuantity;
                }
                //引当在庫
                if (a.ReservationQuantity != null)
                {
                    table.TotalReservationQuantity += a.ReservationQuantity;
                }

                templist.Add(line);                
            }
            table.list = templist.OrderBy(x => x.WarehouseCode).ThenBy(x => x.PartsNumber).ToList();        //Mod 2016/12/09 arc yano #3672
            return table;
        }

    }
}
