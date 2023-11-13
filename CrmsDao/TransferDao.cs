using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Microsoft.VisualBasic;

namespace CrmsDao {
    public class TransferDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">データコンテキスト</param>
        public TransferDao(CrmsLinqDataContext context) {
            db = context;
        }
        public Transfer GetByKey(string transferNumber) {
            var query =
                (from a in db.Transfer
                 where a.TransferNumber.Equals(transferNumber)
                 select a).FirstOrDefault();
            return query;
        }
        /// <summary>
        /// 移動データ検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        public PaginatedList<Transfer> GetListByCondition(Transfer condition,int pageIndex,int pageSize) {
            return new PaginatedList<Transfer>(GetQueryable(condition), pageIndex, pageSize);
        }

        /// <summary>
        /// 移動データ検索
        /// （ページング非対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<Transfer> GetListByCondition(DocumentExportCondition condition){
            Transfer transferCondition = new Transfer();
            transferCondition.DepartureDateFrom = condition.TermFrom;
            transferCondition.DepartureDateTo = condition.TermTo;
            transferCondition.DepartureLocation = new Location();
            transferCondition.DepartureLocation.DepartmentCode = condition.DepartmentCode;
            transferCondition.SetAuthCondition(condition.AuthEmployee);
            return GetQueryable(transferCondition).ToList();
        }

        /// <summary>
        /// 移動データ検索条件式を取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2019/02/12 yano #3977 【部品移動検索】特定の入庫ロケーション、または出庫ロケーションの移動履歴が表示されない
        /// 2018/02/23 arc yano #3860 車両移動　作成した移動データが表示されない。
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 倉庫コードによる絞込の追加
        /// </history>
        private IQueryable<Transfer> GetQueryable(Transfer condition){
            DateTime? departureDateFrom = condition.DepartureDateFrom;
            DateTime? departureDateTo = condition.DepartureDateTo;
            DateTime? arrivalDateFrom = condition.ArrivalDateFrom;
            DateTime? arrivalDateTo = condition.ArrivalDateTo;
            bool transferConfirm = condition.TransferConfirm;
            bool transferUnConfirm = condition.TransferUnConfirm;
            string vin = "";
            if (condition.SalesCar != null) {
                vin = condition.SalesCar.Vin;
            }
            string authCompanyCode = condition.AuthCompanyCode;

            List<string> wlist = new List<string>();

            //Add 2019/02/12 yano #3977
            if(!string.IsNullOrWhiteSpace(authCompanyCode))
            {
                List<Department> dlist = (from a in db.Department
                                         where a.Office.CompanyCode.Equals(authCompanyCode)
                                         select a).ToList();

                foreach (var d in dlist)
                {
                    //部門コードから倉庫を取得
                    DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, d.DepartmentCode);

                    if (dWarehouse != null)
                    {
                        wlist.Add(dWarehouse.WarehouseCode);
                    }

                }
            }
            

            //string authOfficeCode = condition.AuthOfficeCode;
            //string authOfficeCode1 = condition.AuthOfficeCode1;
            //string authOfficeCode2 = condition.AuthOfficeCode2;
            //string authOfficeCode3 = condition.AuthOfficeCode3;
            //string authDepartmentCode = condition.AuthDepartmentCode;
            //string authDepartmentCode1 = condition.AuthDepartmentCode1;
            //string authDepartmentCode2 = condition.AuthDepartmentCode2;
            //string authDepartmentCode3 = condition.AuthDepartmentCode3;

            //Del 2019/02/12 yano #3977 
            //string departureDepartmentCode = null;
            //try { departureDepartmentCode = condition.DepartureLocation.DepartmentCode; } catch (NullReferenceException) { }
            //string departmentCode = condition.DepartmentCode;

            string slipNumber = condition.SlipNumber;       //Add 2016/06/20 arc yano #3584

            string warehouseCode = condition.WarehouseCode;     //Add 2016/08/13 arc yano #3596

            var query =
                from a in db.Transfer
                where (string.IsNullOrEmpty(condition.PartsNumber) || a.PartsNumber.Contains(condition.PartsNumber))
                && (string.IsNullOrEmpty(condition.TransferType) || a.TransferType.Equals(condition.TransferType))
                && (departureDateFrom == null || DateTime.Compare(a.DepartureDate, departureDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                && (departureDateTo == null || DateTime.Compare(a.DepartureDate, departureDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                && (arrivalDateFrom == null || DateTime.Compare(a.ArrivalDate ?? DaoConst.SQL_DATETIME_MAX, arrivalDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                && (arrivalDateTo == null || DateTime.Compare(a.ArrivalDate ?? DaoConst.SQL_DATETIME_MIN, arrivalDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                && (string.IsNullOrEmpty(condition.DepartureLocationCode) || a.DepartureLocationCode.Equals(condition.DepartureLocationCode))
                && (string.IsNullOrEmpty(condition.ArrivalLocationCode) || a.ArrivalLocationCode.Equals(condition.ArrivalLocationCode))
                && ((transferConfirm==true && transferUnConfirm==true) || (transferConfirm==false || (transferConfirm == true &&  a.ArrivalDate!=null)) 
                && (transferUnConfirm==false || (transferUnConfirm == true && a.ArrivalDate==null)))
                //Del 2019/02/12 yano #3977 
                //&& (string.IsNullOrEmpty(departureDepartmentCode) || a.DepartureLocation.DepartmentCode.Equals(departureDepartmentCode))
                //&& (string.IsNullOrEmpty(departmentCode) || a.DepartureLocation.DepartmentCode.Equals(departmentCode) || a.ArrivalLocation.DepartmentCode.Equals(departmentCode))
                && (condition.CarOrParts!=null && condition.CarOrParts.Equals("1") ? 
                    (a.SalesCarNumber!=null && (string.IsNullOrEmpty(vin) || a.SalesCar.Vin.Contains(vin))) : 
                    a.PartsNumber!=null)
                && (string.IsNullOrEmpty(authCompanyCode) || wlist.Contains(a.DepartureLocation.WarehouseCode) || wlist.Contains(a.ArrivalLocation.WarehouseCode))
                //&& (string.IsNullOrEmpty(authCompanyCode) || a.DepartureLocation.Department.Office.CompanyCode.Equals(authCompanyCode) || a.ArrivalLocation.Department.Office.CompanyCode.Equals(authCompanyCode))

                && (string.IsNullOrEmpty(slipNumber) || (a.PartsNumber != null && a.SlipNumber.Equals(slipNumber)))  //Add 2016/06/20 arc yano #3584
                //&& ((string.IsNullOrEmpty(authOfficeCode) || a.DepartureLocation.Department.OfficeCode.Equals(authOfficeCode) || a.ArrivalLocation.Department.OfficeCode.Equals(authOfficeCode))
                //|| (string.IsNullOrEmpty(authOfficeCode1) || a.DepartureLocation.Department.OfficeCode.Equals(authOfficeCode1) || a.ArrivalLocation.Department.OfficeCode.Equals(authOfficeCode1))
                //|| (string.IsNullOrEmpty(authOfficeCode2) || a.DepartureLocation.Department.OfficeCode.Equals(authOfficeCode2) || a.ArrivalLocation.Department.OfficeCode.Equals(authOfficeCode2))
                //|| (string.IsNullOrEmpty(authOfficeCode3) || a.DepartureLocation.Department.OfficeCode.Equals(authOfficeCode3) || a.ArrivalLocation.Department.OfficeCode.Equals(authOfficeCode3)))
                //&& ((string.IsNullOrEmpty(authDepartmentCode) || a.DepartureLocation.DepartmentCode.Equals(authDepartmentCode) || a.ArrivalLocation.DepartmentCode.Equals(authDepartmentCode))
                //|| (string.IsNullOrEmpty(authDepartmentCode1) || a.DepartureLocation.DepartmentCode.Equals(authDepartmentCode1) || a.ArrivalLocation.DepartmentCode.Equals(authDepartmentCode1))
                //|| (string.IsNullOrEmpty(authDepartmentCode2) || a.DepartureLocation.DepartmentCode.Equals(authDepartmentCode2) || a.ArrivalLocation.DepartmentCode.Equals(authDepartmentCode2))
                //|| (string.IsNullOrEmpty(authDepartmentCode3) || a.DepartureLocation.DepartmentCode.Equals(authDepartmentCode3) || a.ArrivalLocation.DepartmentCode.Equals(authDepartmentCode3)))
                && (string.IsNullOrEmpty(warehouseCode) || a.DepartureLocation.WarehouseCode.Equals(warehouseCode) || a.ArrivalLocation.WarehouseCode.Equals(warehouseCode))     //Add 2016/08/13 arc yano #3596
                && a.DelFlag.Equals("0")
            //    orderby a.TransferNumber descending
                select a;

            ParameterExpression param = Expression.Parameter(typeof(Transfer), "x");

            //Mod 2018/02/23 arc yano #3860
            Expression wareExpression = condition.CreateExpressionForWarehouse(param, new string[] { "DepartureLocation", "WarehouseCode" }, new string[] { "ArrivalLocation", "WarehouseCode" });
            if (wareExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<Transfer, bool>>(wareExpression, param));
            }           

            /*
            Expression depExpression = condition.CreateExpressionForDepartment(param, new string[] { "DepartureLocation", "DepartmentCode" }, new string[] { "ArrivalLocation", "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<Transfer, bool>>(depExpression, param));
            }
            Expression offExpression = condition.CreateExpressionForOffice(param, new string[] { "DepartureLocation", "Department", "OfficeCode" }, new string[] { "ArrivalLocation", "Department", "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<Transfer, bool>>(offExpression, param));
            }
            Expression comExpression = condition.CreateExpressionForCompany(param, new string[] { "DepartureLocation", "Department", "Office", "CompanyCode" }, new string[] { "ArrivalLocation", "Department", "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<Transfer, bool>>(comExpression, param));
            }
            */

            return query.OrderByDescending(x=>x.TransferNumber);
            
        }

        /// <summary>
        /// 受注伝票番号に該当する移動データを取得する
        /// </summary>
        /// <param name="slipNumber">受注伝票番号</param>
        /// <returns>移動データ</returns>
        public List<Transfer> GetListBySlipNumber(string slipNumber) {
            var query =
                from a in db.Transfer
                where a.SlipNumber.Equals(slipNumber)
                && !a.DelFlag.Equals("1")
                select a;
            return query.ToList<Transfer>();
        }


        
        /// 移動伝票を作成し、在庫数を更新する
        /// </summary>
        /// <param name="trans">移動伝票</param>
        /// <param name="updateEmployeeCode">更新者</param>
        /// <param name="updatePartsStock">部品在庫更新フラグ(true:更新する false:更新しない)</param>
        /// <history>
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 削除データも取得する
        /// 2016/06/13 arc yano #3571 引数(在庫を更新するかどうか)を追加、引数(calcProvision)削除
        /// 2016/04/13 arc yano #3488 サービス伝票　納車時のシステムエラー insertした場合はsubmitchangeを追加
        /// 2015/11/02 arc yano #3289 サービス伝票 引当在庫の管理方法の変更　引当済数更新処理の追加
        /// <summary>
        /// </history>
        public void InsertTransfer(Transfer trans,string updateEmployeeCode, bool updatePartsStock) {

            Transfer target = new Transfer();
            //移動伝票番号採番
            target.TransferNumber = new SerialNumberDao(db).GetNewTransferNumber();
            
            //共通部分
            target.TransferType = trans.TransferType;
            target.DepartureEmployeeCode = trans.DepartureEmployeeCode;
            target.DepartureLocationCode = trans.DepartureLocationCode;
            target.ArrivalEmployeeCode = trans.ArrivalEmployeeCode;
            target.ArrivalLocationCode = trans.ArrivalLocationCode;
            target.PartsNumber = Strings.StrConv(trans.PartsNumber, VbStrConv.Narrow, 0);
            target.Quantity = trans.Quantity;
            target.SalesCarNumber = trans.SalesCarNumber;
            target.SlipNumber = trans.SlipNumber;
            target.CreateDate = DateTime.Now;
            target.CreateEmployeeCode = updateEmployeeCode;
            target.LastUpdateDate = DateTime.Now;
            target.LastUpdateEmployeeCode = updateEmployeeCode;
            target.DelFlag = "0";

            //通常以外は当日日付をセットする
            if (trans.TransferType.Equals("001")) {
                target.ArrivalDate = trans.ArrivalDate;
                target.ArrivalPlanDate = trans.ArrivalPlanDate;
                target.DepartureDate = trans.DepartureDate;
            } else {
                target.ArrivalDate = DateTime.Today;
                target.ArrivalPlanDate = DateTime.Today;
                target.DepartureDate = DateTime.Today;
            }
            db.Transfer.InsertOnSubmit(target);


            //部品在庫を更新する場合
            if (updatePartsStock == true)   //Add 2016/06/13 arc yano #3571
            {

                //部品在庫を更新
                if (!string.IsNullOrEmpty(target.PartsNumber))
                {

                    //部品在庫
                    //出庫側を減らす
                    PartsStock fromStock = new PartsStockDao(db).GetByKey(trans.PartsNumber, trans.DepartureLocationCode);
                    if (fromStock != null)
                    {
                        fromStock.Quantity = fromStock.Quantity - trans.Quantity;
                        fromStock.LastUpdateDate = DateTime.Now;
                        fromStock.LastUpdateEmployeeCode = updateEmployeeCode;

                        fromStock.ProvisionQuantity -= trans.Quantity;          //引当済数を更新する

                    }
                    //Mod 2017/02/08 arc yano #3620 ここは通らないためコメントアウト
                    /*
                    else
                    {
                        fromStock = new PartsStock();
                        fromStock.Quantity = (-1) * trans.Quantity;
                        fromStock.CreateDate = DateTime.Now;
                        fromStock.CreateEmployeeCode = updateEmployeeCode;
                        fromStock.DelFlag = "0";
                        fromStock.LastUpdateDate = DateTime.Now;
                        fromStock.LastUpdateEmployeeCode = updateEmployeeCode;
                        fromStock.LocationCode = trans.DepartureLocationCode;
                        fromStock.PartsNumber = Strings.StrConv(trans.PartsNumber, VbStrConv.Narrow, 0);

                        fromStock.ProvisionQuantity = (-1) * trans.Quantity;          //引当済数を更新する

                        db.PartsStock.InsertOnSubmit(fromStock);
                    }
                    */
                    
                    //通常の移動以外は自動的に入庫側の在庫を増やす
                    if (!trans.TransferType.Equals("001"))
                    {
                        //入庫側を増やす
                        //入庫側の在庫情報を取得(削除データも取得する)
                        PartsStock toStock = new PartsStockDao(db).GetByKey(trans.PartsNumber, trans.ArrivalLocationCode, true);
                        if (toStock != null)
                        {
                            //削除データの場合は初期化
                            toStock = new PartsStockDao(db).InitPartsStock(toStock);	//Add 016/06/13 arc yano #3571

                            toStock.Quantity = toStock.Quantity + trans.Quantity;
                            toStock.LastUpdateEmployeeCode = updateEmployeeCode;
                            toStock.LastUpdateDate = DateTime.Now;

                            toStock.ProvisionQuantity = (toStock.ProvisionQuantity ?? 0) + trans.Quantity;          //引当済数を更新する
                            toStock.ProvisionQuantity = toStock.ProvisionQuantity;

                        }
                        else
                        {
                            toStock = new PartsStock();
                            toStock.Quantity = trans.Quantity;
                            toStock.CreateDate = DateTime.Now;
                            toStock.CreateEmployeeCode = updateEmployeeCode;
                            toStock.DelFlag = "0";
                            toStock.LastUpdateDate = DateTime.Now;
                            toStock.LastUpdateEmployeeCode = updateEmployeeCode;
                            toStock.LocationCode = trans.ArrivalLocationCode;
                            toStock.PartsNumber = Strings.StrConv(trans.PartsNumber, VbStrConv.Narrow, 0);

                            toStock.ProvisionQuantity = trans.Quantity;          //引当済数を更新する

                            db.PartsStock.InsertOnSubmit(toStock);
                        }
                    }

                    db.SubmitChanges(); //Add 2016/04/13 arc yano #3488
                }

                //車両在庫を更新
                if (!string.IsNullOrEmpty(trans.SalesCarNumber))
                {
                    SalesCar car = new SalesCarDao(db).GetByKey(trans.SalesCarNumber);
                    if (car != null)
                    {
                        car.LocationCode = trans.ArrivalLocationCode;
                        car.LastUpdateDate = DateTime.Now;
                        car.LastUpdateEmployeeCode = updateEmployeeCode;
                    }
                }
            }
        }

        /// 移動伝票を作成し、在庫数を更新する
        /// </summary>
        /// <param name="trans">移動伝票</param>
        /// <param name="updateEmployeeCode">更新者</param>
        /// <param name="CalcProvision">引当済数更新フラグ</param>
        /// <history>
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 削除データも取得する
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 入庫側の在庫数更新処理の判定の変更(同一部門ではなく同一倉庫へ変更)
        /// 2016/06/20 arc yano #3586 部品移動入力　自拠点移動時の入庫処理の省略
        /// 2016/06/13 arc yano #3571 引数違いのメソッドを追加
        /// <summary>
        /// </history>
        public void InsertTransfer(Transfer trans, string updateEmployeeCode)
        {

            Transfer target = new Transfer();
            //移動伝票番号採番
            target.TransferNumber = new SerialNumberDao(db).GetNewTransferNumber();

            //Add 2016/06/20 arc yano #3586
            Location departureLocation = new LocationDao(db).GetByKey(trans.DepartureLocationCode);
            Location arrivalLocation = new LocationDao(db).GetByKey(trans.ArrivalLocationCode);
          
            //共通部分
            target.TransferType = trans.TransferType;
            target.DepartureEmployeeCode = trans.DepartureEmployeeCode;
            target.DepartureLocationCode = trans.DepartureLocationCode;
            target.ArrivalEmployeeCode = trans.ArrivalEmployeeCode;
            target.ArrivalLocationCode = trans.ArrivalLocationCode;
            target.PartsNumber = Strings.StrConv(trans.PartsNumber, VbStrConv.Narrow, 0);
            target.Quantity = trans.Quantity;
            target.SalesCarNumber = trans.SalesCarNumber;
            target.SlipNumber = trans.SlipNumber;
            target.CreateDate = DateTime.Now;
            target.CreateEmployeeCode = updateEmployeeCode;
            target.LastUpdateDate = DateTime.Now;
            target.LastUpdateEmployeeCode = updateEmployeeCode;
            target.DelFlag = "0";

            //Mod  2016/08/13 arc yano #3596
            //Mod 2016/06/20 arc yano
            //通常以外、または同一倉庫内の移動の場合は当日日付をセットする
            //if (trans.TransferType.Equals("001")  && !departureLocation.DepartmentCode.Equals(arrivalLocation.DepartmentCode))                   
            if (trans.TransferType.Equals("001") && !departureLocation.WarehouseCode.Equals(arrivalLocation.WarehouseCode))
            {
                target.ArrivalDate = trans.ArrivalDate;
                target.ArrivalPlanDate = trans.ArrivalPlanDate;
                target.DepartureDate = trans.DepartureDate;
            }
            else
            {
                target.ArrivalDate = DateTime.Today;
                target.ArrivalPlanDate = DateTime.Today;
                target.DepartureDate = DateTime.Today;
            }
            db.Transfer.InsertOnSubmit(target);

            //部品在庫を更新
            if (!string.IsNullOrEmpty(target.PartsNumber))
            {

                //部品在庫
                //出庫側を減らす
                PartsStock fromStock = new PartsStockDao(db).GetByKey(trans.PartsNumber, trans.DepartureLocationCode);
                if (fromStock != null)
                {
                    fromStock.Quantity = fromStock.Quantity - trans.Quantity;
                    fromStock.LastUpdateDate = DateTime.Now;
                    fromStock.LastUpdateEmployeeCode = updateEmployeeCode;
                }
                //Mod 2017/02/08 arc yano #3620
                //この処理は通らないためコメントアウト
                /*
                else
                {
                    fromStock = new PartsStock();
                    fromStock.Quantity = (-1) * trans.Quantity;
                    fromStock.CreateDate = DateTime.Now;
                    fromStock.CreateEmployeeCode = updateEmployeeCode;
                    fromStock.DelFlag = "0";
                    fromStock.LastUpdateDate = DateTime.Now;
                    fromStock.LastUpdateEmployeeCode = updateEmployeeCode;
                    fromStock.LocationCode = trans.DepartureLocationCode;
                    fromStock.PartsNumber = Strings.StrConv(trans.PartsNumber, VbStrConv.Narrow, 0);

                    fromStock.ProvisionQuantity = 0;        //2016/06/20 arc yano #3586

                    db.PartsStock.InsertOnSubmit(fromStock);
                }
                */
                //Mod 2017/02/08 arc yano #3620
                //Mod 2016/08/13 arc yano #3596
                //Mod 2016/06/20 arc yano
                //通常の移動以外または、自部門の移動の場合は自動的に入庫側の在庫を増やす
                //if (!trans.TransferType.Equals("001") || departureLocation.DepartmentCode.Equals(arrivalLocation.DepartmentCode))
                if (!trans.TransferType.Equals("001") || departureLocation.WarehouseCode.Equals(arrivalLocation.WarehouseCode))
                {
                    //入庫側を増やす
                    //入庫側の在庫情報を取得する(※削除データも取得する)
                    PartsStock toStock = new PartsStockDao(db).GetByKey(trans.PartsNumber, trans.ArrivalLocationCode, true);
                    if (toStock != null)
                    {
                        //削除データの場合は初期化
                        toStock = new PartsStockDao(db).InitPartsStock(toStock);	//Add 016/06/13 arc yano #3571

                        toStock.Quantity = toStock.Quantity + trans.Quantity;
                        toStock.LastUpdateEmployeeCode = updateEmployeeCode;
                        toStock.LastUpdateDate = DateTime.Now;
                    }
                    else
                    {
                        toStock = new PartsStock();
                        toStock.Quantity = trans.Quantity;
                        toStock.CreateDate = DateTime.Now;
                        toStock.CreateEmployeeCode = updateEmployeeCode;
                        toStock.DelFlag = "0";
                        toStock.LastUpdateDate = DateTime.Now;
                        toStock.LastUpdateEmployeeCode = updateEmployeeCode;
                        toStock.LocationCode = trans.ArrivalLocationCode;
                        toStock.PartsNumber = Strings.StrConv(trans.PartsNumber, VbStrConv.Narrow, 0);

                        toStock.ProvisionQuantity = 0;      //2016/06/20 arc yano #3586

                        db.PartsStock.InsertOnSubmit(toStock);
                    }
                }
            }

            //車両在庫を更新
            if (!string.IsNullOrEmpty(trans.SalesCarNumber))
            {
                SalesCar car = new SalesCarDao(db).GetByKey(trans.SalesCarNumber);
                if (car != null)
                {
                    car.LocationCode = trans.ArrivalLocationCode;
                    car.LastUpdateDate = DateTime.Now;
                    car.LastUpdateEmployeeCode = updateEmployeeCode;
                }
            }
        }

        /// <summary>
        /// 管理番号に該当する移動履歴を取得する
        /// </summary>
        /// <param name="salesCarNumber">管理番号</param>
        /// <returns></returns>
        public List<Transfer> GetBySalesCarNumber(string salesCarNumber, bool includeDeleted = false) {
            var query =
                from a in db.Transfer
                orderby a.TransferNumber descending
                where a.SalesCarNumber.Equals(salesCarNumber)
                && ((includeDeleted) || a.DelFlag.Equals("0"))
                select a;
            return query.ToList();
        }
    }
}
