using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;    //Add 2014/05/08 arc yano vs2012対応
using System.Data.Linq;
using System.Linq.Expressions;
using System.Configuration;
using CrmsDao.Properties;

namespace CrmsDao
{
    partial class CrmsLinqDataContext
    {
        partial void OnCreated()
        {
            this.CommandTimeout = Settings.Default.SqlTimeout;
        }
    }
    public interface ICrmsModel
    {
    }
    #region 閲覧権限クラス

    /// <summary>
    /// 閲覧権限クラス
    /// </summary>
    /// <history>
    /// 2018/02/24 arc #3860 車両移動入力 作成した移動データが表示されない。
    /// </history>
    public class CrmsAuth
    {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;


        public string AuthCompanyCode { get; set; }
        public List<string> AuthOfficeCode { get; set; }
        public List<string> AuthDepartmentCode { get; set; }

        public List<string> AuthWarehouseCode { get; set; }             //Add 2018/02/23 arc #3860

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CrmsAuth()
        {
            AuthOfficeCode = new List<string>();
            AuthDepartmentCode = new List<string>();

            //閲覧可能倉庫リスト
            AuthWarehouseCode = new List<string>();                     //Add 2018/02/23 arc #3860

            //DBコンテキスト
            db = new CrmsLinqDataContext();
        }
        /// <summary>
        /// 閲覧可能範囲をセットする
        /// </summary>
        /// <param name="employee">社員データ</param>
        public void SetAuthCondition(Employee employee)
        {
            if (employee != null)
            {
                switch (employee.SecurityRole.SecurityLevelCode)
                {
                    case "001": //部門内
                        if (!string.IsNullOrEmpty(employee.DepartmentCode))
                        {
                            AuthDepartmentCode.Add(employee.DepartmentCode);
                            SetWarehouseCodeList(employee.DepartmentCode, false);   //Add 2018/02/23 arc #3860
                        }
                        if (!string.IsNullOrEmpty(employee.DepartmentCode1))
                        {
                            AuthDepartmentCode.Add(employee.DepartmentCode1);
                            SetWarehouseCodeList(employee.DepartmentCode1, false);   //Add 2018/02/23 arc #3860
                        }
                        if (!string.IsNullOrEmpty(employee.DepartmentCode2))
                        {
                            AuthDepartmentCode.Add(employee.DepartmentCode2);
                            SetWarehouseCodeList(employee.DepartmentCode2, false);   //Add 2018/02/23 arc #3860
                        }
                        if (!string.IsNullOrEmpty(employee.DepartmentCode3))
                        {
                            AuthDepartmentCode.Add(employee.DepartmentCode3);
                            SetWarehouseCodeList(employee.DepartmentCode3, false);   //Add 2018/02/23 arc #3860
                        }
                        break;
                    case "002": //事業部内
                        if (employee.Department1 != null && !string.IsNullOrEmpty(employee.Department1.OfficeCode))
                        {
                            AuthOfficeCode.Add(employee.Department1.OfficeCode);
                            SetWarehouseCodeList(employee.Department1.OfficeCode, true);   //Add 2018/02/23 arc #3860
                        }
                        if (employee.AdditionalDepartment1 != null && !string.IsNullOrEmpty(employee.AdditionalDepartment1.OfficeCode))
                        {
                            AuthOfficeCode.Add(employee.AdditionalDepartment1.OfficeCode);
                            SetWarehouseCodeList(employee.AdditionalDepartment1.OfficeCode, true);   //Add 2018/02/23 arc #3860
                        }
                        if (employee.AdditionalDepartment2 != null && !string.IsNullOrEmpty(employee.AdditionalDepartment2.OfficeCode))
                        {
                            AuthOfficeCode.Add(employee.AdditionalDepartment2.OfficeCode);
                            SetWarehouseCodeList(employee.AdditionalDepartment2.OfficeCode, true);   //Add 2018/02/23 arc #3860
                        }
                        if (employee.AdditionalDepartment3 != null && !string.IsNullOrEmpty(employee.AdditionalDepartment3.OfficeCode))
                        {
                            AuthOfficeCode.Add(employee.AdditionalDepartment3.OfficeCode);
                            SetWarehouseCodeList(employee.AdditionalDepartment3.OfficeCode, true);   //Add 2018/02/23 arc #3860
                        }
                        break;
                    case "003": //会社内
                        AuthCompanyCode = employee.Department1 != null && employee.Department1.Office != null ? employee.Department1.Office.CompanyCode : "";
                        break;
                    case "004": //ALL
                        break;
                }
            }
        }

        /// <summary>
        /// 部門権限抽出
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Expression CreateExpressionForDepartment(ParameterExpression param, string[] PropertyNames)
        {
            Expression bodyExpr = null;
            foreach (string dep in AuthDepartmentCode)
            {
                MemberExpression member = null;
                foreach (string propertyName in PropertyNames)
                {
                    if (member != null)
                    {
                        member = Expression.Property(member, propertyName);
                    }
                    else
                    {
                        member = Expression.Property(param, propertyName);
                    }
                }
                BinaryExpression binary = Expression.Equal(member, Expression.Constant(dep));
                if (bodyExpr == null)
                {
                    bodyExpr = binary;
                }
                else
                {
                    bodyExpr = Expression.OrElse(bodyExpr, binary);
                }
            }
            return bodyExpr;
        }
        public Expression CreateExpressionForDepartment(ParameterExpression param, string[] PropertyNames, string[] OrPropertyNames)
        {
            Expression bodyExpr = null;
            foreach (string dep in AuthDepartmentCode)
            {
                MemberExpression member = null;
                MemberExpression member2 = null;
                foreach (string propertyName in PropertyNames)
                {
                    if (member != null)
                    {
                        member = Expression.Property(member, propertyName);
                    }
                    else
                    {
                        member = Expression.Property(param, propertyName);
                    }
                }
                foreach (string propertyName in OrPropertyNames)
                {
                    if (member2 != null)
                    {
                        member2 = Expression.Property(member2, propertyName);
                    }
                    else
                    {
                        member2 = Expression.Property(param, propertyName);
                    }
                }
                BinaryExpression binary = Expression.Equal(member, Expression.Constant(dep));
                BinaryExpression binary2 = Expression.Equal(member2, Expression.Constant(dep));
                binary = Expression.OrElse(binary, binary2);

                if (bodyExpr == null)
                {
                    bodyExpr = binary;
                }
                else
                {
                    bodyExpr = Expression.OrElse(bodyExpr, binary);
                }
            }
            return bodyExpr;
        }
        /// <summary>
        /// 事業所権限抽出
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Expression CreateExpressionForOffice(ParameterExpression param, string[] PropertyNames)
        {
            Expression bodyExpr = null;
            foreach (string office in AuthOfficeCode)
            {
                MemberExpression member = null;
                foreach (string propertyName in PropertyNames)
                {
                    if (member != null)
                    {
                        member = Expression.Property(member, propertyName);
                    }
                    else
                    {
                        member = Expression.Property(param, propertyName);
                    }
                }
                BinaryExpression binary = Expression.Equal(member, Expression.Constant(office));
                if (bodyExpr == null)
                {
                    bodyExpr = binary;
                }
                else
                {
                    bodyExpr = Expression.OrElse(bodyExpr, binary);
                }
            }
            return bodyExpr;
        }
        public Expression CreateExpressionForOffice(ParameterExpression param, string[] PropertyNames, string[] OrPropertyNames)
        {
            Expression bodyExpr = null;
            foreach (string office in AuthOfficeCode)
            {
                MemberExpression member = null;
                foreach (string propertyName in PropertyNames)
                {
                    if (member != null)
                    {
                        member = Expression.Property(member, propertyName);
                    }
                    else
                    {
                        member = Expression.Property(param, propertyName);
                    }
                }
                BinaryExpression binary = Expression.Equal(member, Expression.Constant(office));
                MemberExpression member2 = null;
                foreach (string propertyName in OrPropertyNames)
                {
                    if (member2 != null)
                    {
                        member2 = Expression.Property(member2, propertyName);
                    }
                    else
                    {
                        member2 = Expression.Property(param, propertyName);
                    }
                }
                BinaryExpression binary2 = Expression.Equal(member2, Expression.Constant(office));
                binary = Expression.OrElse(binary, binary2);

                if (bodyExpr == null)
                {
                    bodyExpr = binary;
                }
                else
                {
                    bodyExpr = Expression.OrElse(bodyExpr, binary);
                }
            }
            return bodyExpr;
        }
        /// <summary>
        /// 会社権限抽出
        /// </summary>
        /// <param name="param"></param>
        /// <param name="PropertyNames"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/02/24 arc #3860 車両移動入力 作成した移動データが表示されない。
        /// </history>
        public Expression CreateExpressionForCompany(ParameterExpression param, string[] PropertyNames)
        {
            Expression bodyExpr = null;
            if (!string.IsNullOrEmpty(AuthCompanyCode))
            {
                MemberExpression member = null;
                foreach (string propertyName in PropertyNames)
                {
                    if (member != null)
                    {
                        member = Expression.Property(member, propertyName);
                    }
                    else
                    {
                        member = Expression.Property(param, propertyName);
                    }
                }
                bodyExpr = Expression.Equal(member, Expression.Constant(AuthCompanyCode));
            }
            return bodyExpr;
        }
        public Expression CreateExpressionForCompany(ParameterExpression param, string[] PropertyNames, string[] OrPropertyNames)
        {
            Expression bodyExpr = null;
            if (!string.IsNullOrEmpty(AuthCompanyCode))
            {
                MemberExpression member = null;
                foreach (string propertyName in PropertyNames)
                {
                    if (member != null)
                    {
                        member = Expression.Property(member, propertyName);
                    }
                    else
                    {
                        member = Expression.Property(param, propertyName);
                    }
                }
                BinaryExpression binary = Expression.Equal(member, Expression.Constant(AuthCompanyCode));

                MemberExpression member2 = null;
                foreach (string propertyName in OrPropertyNames)
                {
                    if (member2 != null)
                    {
                        member2 = Expression.Property(member2, propertyName);
                    }
                    else
                    {
                        member2 = Expression.Property(param, propertyName);
                    }
                }
                BinaryExpression binary2 = Expression.Equal(member2, Expression.Constant(AuthCompanyCode));
                bodyExpr = Expression.OrElse(binary, binary2);
            }
            return bodyExpr;
        }

        /// <summary>
        /// 倉庫権限抽出
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/02/23 arc #3860 車両移動入力 作成した移動データが表示されない。
        /// </history>
        public Expression CreateExpressionForWarehouse(ParameterExpression param, string[] PropertyNames)
        {
            Expression bodyExpr = null;
            foreach (string warehouse in AuthWarehouseCode)
            {
                MemberExpression member = null;
                foreach (string propertyName in PropertyNames)
                {
                    if (member != null)
                    {
                        member = Expression.Property(member, propertyName);
                    }
                    else
                    {
                        member = Expression.Property(param, propertyName);
                    }
                }
                BinaryExpression binary = Expression.Equal(member, Expression.Constant(warehouse));
                if (bodyExpr == null)
                {
                    bodyExpr = binary;
                }
                else
                {
                    bodyExpr = Expression.OrElse(bodyExpr, binary);
                }
            }
            return bodyExpr;
        }

        /// <summary>
        /// 倉庫権限抽出
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/02/23 arc #3860 車両移動入力 作成した移動データが表示されない。
        /// </history>
        public Expression CreateExpressionForWarehouse(ParameterExpression param, string[] PropertyNames, string[] OrPropertyNames)
        {
            Expression bodyExpr = null;
            foreach (string warehouse in AuthWarehouseCode)
            {
                MemberExpression member = null;
                foreach (string propertyName in PropertyNames)
                {
                    if (member != null)
                    {
                        member = Expression.Property(member, propertyName);
                    }
                    else
                    {
                        member = Expression.Property(param, propertyName);
                    }
                }
                BinaryExpression binary = Expression.Equal(member, Expression.Constant(warehouse));
                MemberExpression member2 = null;
                foreach (string propertyName in OrPropertyNames)
                {
                    if (member2 != null)
                    {
                        member2 = Expression.Property(member2, propertyName);
                    }
                    else
                    {
                        member2 = Expression.Property(param, propertyName);
                    }
                }
                BinaryExpression binary2 = Expression.Equal(member2, Expression.Constant(warehouse));
                binary = Expression.OrElse(binary, binary2);

                if (bodyExpr == null)
                {
                    bodyExpr = binary;
                }
                else
                {
                    bodyExpr = Expression.OrElse(bodyExpr, binary);
                }
            }
            return bodyExpr;
        }


        /// <summary>
        /// 倉庫コードリスト設定
        /// </summary>
        /// <param name="param"></param>
        /// <param name="PropertyNames"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/02/24 arc #3860 車両移動入力 作成した移動データが表示されない。
        /// </history>
        private void SetWarehouseCodeList(string code, bool officeFlag)
        {
            List<string> departmentCodeList = new List<string>();

            //事業所内の閲覧権限の場合
            if (officeFlag)
            {
                departmentCodeList = new DepartmentDao(db).GetListByAuthCondition(this).Select(x => x.DepartmentCode).ToList();
            }
            else
            {
                departmentCodeList.Add(code);
            }

            foreach (var d in departmentCodeList)
            {
                //部門・倉庫組合せマスタ取得
                DepartmentWarehouse dwarehouse = new DepartmentWarehouseDao(db).GetByDepartment(d);

                if (dwarehouse != null)
                {
                    AuthWarehouseCode.Add(dwarehouse.WarehouseCode);
                }
            }
        }

    }
    #endregion
    public class MonthlyStatus
    {
        public Department Department { get; set; }
        public CashBalance CashBalance { get; set; }
        // Mod 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)
        public InventoryScheduleParts InventoryPartsSchedule { get; set; }
        public InventorySchedule InventorySchedule { get; set; }
        public InventoryScheduleCar InventoryCarSchedule { get; set; }     //Add 2017/05/10 arc yano #3762
    }

    //Mod 2015/07/17 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑦ 引当在庫、各数量の合計を追加を追加
    //Mod 2015/06/03 arc yano 理論在庫の合計を追加
    //Mod 2015/03/20 arc yano 部品在庫確認修正(受払テーブル作成)
    //Add 2014/11/29 arc yano 部品在庫確認画面追加対応　部品在庫確認クラス(合計)の追加
    public class TotalPartsStockCheck
    {
        public decimal? TotalPreQuantity { get; set; }                      //月初在庫(数量)
        public decimal? TotalPurchaseQuantity { get; set; }                 //当月仕入(数量)
        public decimal? TotalTransferArrivalQuantity { get; set; }          //当月移動受入(数量)
        public decimal? TotalShipQuantity { get; set; }                     //当月納車(数量)
        public decimal? TotalTransferDepartureQuantity { get; set; }        //当月移動払出  (数量) 
        public decimal? TotalCalculatedQuantity { get; set; }               //理論在庫(数量)
        public decimal? TotalDifferenceQuantity { get; set; }               //棚差(数量)
        public decimal? TotalPostQuantity { get; set; }                     //月末在庫(数量)
        public decimal? TotalInProcessQuantity { get; set; }                //仕掛在庫(数量)
        public decimal? TotalReservationQuantity { get; set; }              //引当在庫(数量)
        public decimal? TotalPreAmount { get; set; }                        //月初在庫(金額)
        public decimal? TotalPurchaseAmount { get; set; }                   //当月仕入(金額)
        public decimal? TotalTransferArrivalAmount { get; set; }            //当月移動受入(金額)
        public decimal? TotalShipAmount { get; set; }                       //当月納車(金額)
        public decimal? TotalTransferDepartureAmount { get; set; }          //当月移動払出(金額)
        public decimal? TotalUnitPriceDifference { get; set; }              //単価差額   (金額)
        public decimal? TotalCalculatedAmount { get; set; }                 //理論在庫(金額)
        public decimal? TotalDifferenceAmount { get; set; }                 //棚差(金額)
        public decimal? TotalPostAmount { get; set; }                       //月末在庫(金額)
        public decimal? TotalInProcessAmount { get; set; }                  //仕掛在庫(金額)
        public decimal? TotalReservationAmount { get; set; }                //引当在庫(金額)
        public List<PartsBalance> list { get; set; }                        //受払表
        public PaginatedList<PartsBalance> plist { get; set; }              //受払表(ページング対応)

        //初期処理
        public TotalPartsStockCheck()
        {
            list = new List<PartsBalance>();
            plist = new PaginatedList<PartsBalance>();
        }


    }

    /* //Del 2015/03/20 arc yano 部品在庫確認のデータ取得先を変更のため、削除
    //Add 2014/11/20 arc nakayama 部品在庫確認画面追加対応　部品在庫確認クラスの追加
    public class PartsStockCheck{
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }
        public decimal? BeginningInventoryAmount { get; set; }
        public decimal? DeliveredAmount { get; set; }
        public decimal? WipInventoryAmount { get; set; }
        public decimal? StockAmount { get; set; }
        public decimal? ActualShelfAmount { get; set; }
        public decimal? InventoryVarianceAmount { get; set; }
        
        //[DisplayFormat(ConvertEmptyStringToNull = false)]
        //public string  InventoryStatus { get; set; }
        
    }
    */
    // Add 2014/12/10 arc nakayama 顧客DM対応 顧客情報クラス(検索結果の格納先)の追加
    public class CustomerDataResult
    {
        public string DmFlag { get; set; }					//営業(DM可否)
        public string DmMemo { get; set; }					//備考(DM可否)
        public string InspectGuidFlag { get; set; }			//車検案内(DM可否)
        public string InspectGuidMemo { get; set; }			//備考(DM可否)
        public string CustomerCode { get; set; }			//顧客コード(顧客情報)
        public string CustomerName { get; set; }			//顧客名称(顧客情報)
        //Add 2015/04/10 arc nakayama 顧客DM追加項目　顧客種別を検索条件に入れる
        public string CustomerKind { get; set; }            //顧客種別(顧客情報)
        public string CustomerPostCode { get; set; }		//郵便番号(顧客住所)
        public string CustomerPrefecture { get; set; }		//都道府県(顧客住所)
        public string CustomerAddress { get; set; }			//住所(顧客住所)
        public string CustomerTelNumber { get; set; }		//電話番号(顧客住所)
        public string MobileNumber { get; set; }		//携帯電話番号(顧客住所)
        //Add 2015/12/16 arc ookubo #3318_メールアドレス追加
        public string MailAddress { get; set; }			　　//メールアドレス(顧客住所)
        public bool CustomerAddressReconfirm { get; set; }  //住所再確認(顧客住所)
        public string CustomerAddressReconfirmCSV { get; set; } //住所再確認(顧客住所)　CSV出力用
        public string DmPostCode { get; set; }				//郵便番号(DM発送先住所)
        public string DmPrefecture { get; set; }			//都道府県(DM発送先住所)
        public string DmCustomerDmAddress { get; set; }		//住所(DM発送先住所)
        public string DmTelNumber { get; set; }				//電話番号(DM発送先住所)
        public string SalesCarNumber { get; set; }			//管理番号(車両情報)
        public string MorterViecleOfficialCode { get; set; }//陸運事務局名称(車両情報)
        public string Vin { get; set; }						//車台番号(車両情報)
        public string MakerName { get; set; }				//メーカー(車両情報)
        public string CarName { get; set; }					//車種名称(車両情報)
        public string FirstRegistrationYear { get; set; }	//初年度登録(車両情報)
        public DateTime? FirstRegistrationDate { get; set; } //初年度登録日(車両情報)
        public DateTime? RegistrationDate { get; set; }		//登録年月日(車両情報)
        public DateTime? NextInspectionDate { get; set; }	//次回点検日(車両情報)
        public DateTime? ExpireDate { get; set; }			//車検満了日(車両情報)
        public string SalesDepartmentName { get; set; }		//部門名(営業担当)
        public string SalesEmployeeName { get; set; }		//担当者名(営業担当)
        public DateTime? SalesDate { get; set; }		//納車年月日(営業担当)
        public string ServiceDepartmentName { get; set; }	//部門名(サービス担当)
        public string ServiceEmployeeName { get; set; }	    //担当者名(サービス担当)
        public DateTime? ArrivalPlanDate { get; set; }		//納車日(サービス担当)
        //Add 2015/02/17 arc nakayama 顧客DM仕様変更対応（顧客マスタに紐づく項目追加、営業担当部門名・営業担当者名・サービス担当部門名）
        public string DepartmentName2 { get; set; }             //営業担当部門名
        public string CarEmployeeName { get; set; }             //営業担当者名
        public string ServiceDepartmentName2 { get; set; }      //サービス担当部門名
        public string ServiceEmployeeName2 { get; set; }      //サービス担当者名
        //Add 2015/05/20 arc nakayama #3199_顧客データリスト出力項目追加
        public int? CarWeight { get; set; }      //車両重量
        public string CustomerAddressReconfirmName { get; set; }      //住所再確認の名称（要/不要）
        public string DmCustomerName { get; set; }			//顧客名(DM発送先住所)
        //Add 2016/03/08 arc nakayama #3452_車検点検リスト項目追加(#3451_【大項目】車検活動リスト作成)
        public string CarGradeName { get; set; }			//グレード名
        public string LocationName { get; set; }			//車両在庫ロケーション
        public string SalesOrderStatusName { get; set; }	//車両伝票ステータス
        public string ServiceDepartmentName3 { get; set; }	//最終入庫ロケーション

    }

    // Add 2014/12/10 arc nakayama 顧客DM対応 顧客情報クラス(検索項目の格納先)の追加
    public class CustomerDataSearch
    {

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DmFlag { get; set; }					    //営業(DM可否)
        //Mod 2015/01/07 arc nakayama 顧客DM指摘事項⑩　備考のコメントアウト
        //public string DmMemo { get; set; }					    //備考(営業) 
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string InspectGuidFlag { get; set; }			    //車検案内(DM可否)
        //Mod 2015/01/07 arc nakayama 顧客DM指摘事項⑩　備考のコメントアウト
        //public string InspectGuidMemo { get; set; }			    //備考(車検案内)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesDepartmentCode { get; set; }		    //営業部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesDepartmentName { get; set; }	        //営業部門名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceDepartmentCode { get; set; }	    //サービス部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceDepartmentName { get; set; }	    //サービス部門名
        //Add 2015/01/08 arc nakayama 顧客DM指摘事項⑨　検索項目の追加（車種名・メーカー名・納車日(営業/サービス)・担当者(営業/サービス)）
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesEmployeeName { get; set; }		    //担当者名(営業担当)
        public DateTime? SalesDateFrom { get; set; }		//納車年月日(営業担当)From
        public DateTime? SalesDateTo { get; set; }		    //納車年月日(営業担当)To
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceEmployeeName { get; set; }	        //担当者名(サービス担当)
        public DateTime? ArrivalPlanDateFrom { get; set; }		//納車日(サービス担当)From
        public DateTime? ArrivalPlanDateTo { get; set; }		//納車日(サービス担当)To
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerName { get; set; }				    //メーカー(車両情報)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }					    //車種名称(車両情報)
        public DateTime? FirstRegistrationFrom { get; set; }	    //初年度登録From
        public DateTime? FirstRegistrationTo { get; set; }	    //初年度登録To
        //Mod 2015/01/08 arc nakayama 顧客DM指摘事項⑦　登録年月日・次回点検日・車検満了日をFrom～Toで検索できるようにする
        public DateTime? RegistrationDateFrom { get; set; }		//登録年月日From
        public DateTime? RegistrationDateTo { get; set; }		//登録年月日To
        public DateTime? NextInspectionDateFrom { get; set; }	//次回点検日From
        public DateTime? NextInspectionDateTo { get; set; }	    //次回点検日To
        public DateTime? ExpireDateFrom { get; set; }			//車検満了日From
        public DateTime? ExpireDateTo { get; set; }			    //車検満了日TO
        //Add 2015/02/17 arc nakayama 顧客DM仕様変更対応（顧客マスタに紐づく項目追加、営業担当部門コード・営業担当部門名・営業担当者コード・営業担当者名・サービス担当部門コード・サービス担当部門名）
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode2 { get; set; }             //営業担当部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName2 { get; set; }             //営業担当部門名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarEmployeeCode { get; set; }             //営業担当者コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarEmployeeName { get; set; }             //営業担当者名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceDepartmentCode2 { get; set; }      //サービス担当部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceDepartmentName2 { get; set; }      //サービス担当部門名
        //Add 2015/04/10 arc nakayama 顧客DM指摘事項修正Part2　住所再確認を検索条件に入れる
        public bool? CustomerAddressReconfirm { get; set; }  //住所再確認(顧客住所)
        //Add 2015/04/10 arc nakayama 顧客DM追加項目　顧客種別を検索条件に入れる
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerKind { get; set; }  //顧客種別(顧客情報)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerAddress { get; set; }  //顧客住所(都道府県市区町村)   //Add 2018/04/25 arc yano #3842
    }

    //Add 2014/09/24 arc yano #3080(顧客検索機能の新設) 販売実績クラスの追加
    public class SalesResult
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String CustomerCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String CustomerName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String SalesCarNumber { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String Vin { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String CarSlipNumber { get; set; }
        public int? CarRevisionNumber { get; set; }
        public DateTime? CarSalesDate { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String ServiceSlipNumber { get; set; }
        public int? ServiceRevisionNumber { get; set; }
        public DateTime? ServiceSalesDate { get; set; }
        public DateTime? CarSalesDateFrom { get; set; }
        public DateTime? CarSalesDateTo { get; set; }
        public DateTime? ServiceSalesDateFrom { get; set; }
        public DateTime? ServiceSalesDateTo { get; set; }
        //Add 2014/10/06 arc yano #3080(顧客検索機能の新設その２) プロパティ追加(顧客名(カナ)、両登録番号【種別、かな、プレート】、型式追加)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String CustomerNameKana { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String RegistrationNumberType { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String RegistrationNumberKana { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String RegistrationNumberPlate { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String ModelName { get; set; }
        //Add 2014/10/29 arc yano #3080(顧客検索機能の新設その３) プロパティ追加(陸運局コード)追加
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public String MorterViecleOfficialCode { get; set; }
    }
    public partial class Account : ICrmsModel
    {
    }
    public partial class Area : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class Bank : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class Brand : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class Campaign : ICrmsModel
    {
        public DateTime? CampaignStartDateFrom { get; set; }
        public DateTime? CampaignStartDateTo { get; set; }
        public DateTime? CampaignEndDateFrom { get; set; }
        public DateTime? CampaignEndDateTo { get; set; }
    }
    public partial class CampaignCar : ICrmsModel
    {
    }
    public partial class Car : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class CarAppraisal : ICrmsModel
    {
        public bool RegetVin { get; set; }
        public bool RateEnabled { get; set; }           //ADD 2014/02/20 ookubo
        public string LastEditMessage { get; set; } //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
    }
    public partial class CarClass : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class CarColor : ICrmsModel
    {
        public string CarGradeCode { get; set; }
        public string DelFlagName { get; set; }
    }
    public partial class CarGrade : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class CarOption : ICrmsModel
    {
        public string DelFlagName { get; set; }
        //Add 2016/02/22 arc nakayama #3415_車両伝票作成時のオプションのデフォルト設定
        public string CarGradeName { get; set; }
        public string MakerName { get; set; }
        public string ActionFlag { get; set; }
    }
    public partial class GetCarOptionMaster_Result : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class CarPurchase : ICrmsModel
    {
        public DateTime? PurchaseOrderDateFrom { get; set; }
        public DateTime? PurchaseOrderDateTo { get; set; }
        public DateTime? PurchaseDateFrom { get; set; }
        public DateTime? PurchaseDateTo { get; set; }
        public DateTime? SlipDateFrom { get; set; }
        public DateTime? SlipDateTo { get; set; }
        public void calculate()
        {
            decimal totalAmount;
            totalAmount = VehiclePrice;
            totalAmount = decimal.Add(totalAmount, MetallicPrice);
            totalAmount = decimal.Add(totalAmount, OptionPrice);
            totalAmount = decimal.Add(totalAmount, FirmPrice);
            totalAmount = decimal.Add(totalAmount, DiscountPrice);
            totalAmount = decimal.Add(totalAmount, EquipmentPrice);
            totalAmount = decimal.Add(totalAmount, RepairPrice);
            totalAmount = decimal.Add(totalAmount, OthersPrice);
            Amount = totalAmount;
        }
        public bool RegetVin { get; set; }
        public bool RateEnabled { get; set; }           //ADD 2014/02/20 ookubo
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LastEditMessage { get; set; } //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CalcResultMessage { get; set; } //Add 2017/03/06 arc yano #3640 車両仕入金額のチェック
    }
    public partial class CarPurchaseOrder : CrmsAuth, ICrmsModel
    {
        public string DelFlagName { get; set; }
        // ADD 2014/05/26 vs2012対応 arc uchida データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumberFrom { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumberTo { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumberFrom { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumberTo { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerOrderNumberFrom { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerOrderNumberTo { get; set; }
        public DateTime? SalesOrderDateFrom { get; set; }
        public DateTime? SalesOrderDateTo { get; set; }
        public DateTime? RegistrationPlanDateFrom { get; set; }
        public DateTime? RegistrationPlanDateTo { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }
        public string MakerCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerName { get; set; }
        public string CarCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ModelCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ModelName { get; set; }
        public CarSalesHeader CarSalesHeader { get; set; }
        public decimal ReceiptAmount { get; set; }
        public string CarGradeCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ExteriorColorCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string InteriorColorCode { get; set; }
        public DateTime? DocumentPurchaseRequestDateFrom { get; set; }
        public DateTime? DocumentPurchaseRequestDateTo { get; set; }
        public DateTime? DocumentPurchaseDateFrom { get; set; }
        public DateTime? DocumentPurchaseDateTo { get; set; }
        public DateTime? DocumentReceiptDateFrom { get; set; }
        public DateTime? DocumentReceiptDateTo { get; set; }
        /*public decimal? FirmMargin { get; set; }
        public decimal? MetallicPrice { get; set; }
        public decimal? OptionPrice { get; set; }
        public decimal? Amount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? VehiclePrice { get; set; }
        */
        public int PaymentPeriod1 { get; set; }
        public int PaymentPeriod2 { get; set; }
        public int PaymentPeriod3 { get; set; }
        public int PaymentPeriod4 { get; set; }
        public int PaymentPeriod5 { get; set; }
        public int PaymentPeriod6 { get; set; }
        public decimal? PaymentAmount1 { get; set; }
        public decimal? PaymentAmount2 { get; set; }
        public decimal? PaymentAmount3 { get; set; }
        public decimal? PaymentAmount4 { get; set; }
        public decimal? PaymentAmount5 { get; set; }
        public decimal? PaymentAmount6 { get; set; }
        public bool TradeInPurchaseFlag { get; set; }
        public int? PaymentExpiredate { get; set; }
        public List<System.Web.Mvc.SelectListItem> FirmList { get; set; }
        public List<System.Web.Mvc.SelectListItem> RegistMonthList { get; set; }
        public string Parts1 { get; set; }
        public string Parts2 { get; set; }
        public string Parts3 { get; set; }
        public string Parts4 { get; set; }
        public string Parts5 { get; set; }
        public string EventCode1 { get; set; }
        public string ReportName1 { get; set; }
        public decimal? ReportAmount1 { get; set; }
        public string EventCode2 { get; set; }
        public string ReportName2 { get; set; }
        public decimal? ReportAmount2 { get; set; }
        public string EventCode3 { get; set; }
        public string ReportName3 { get; set; }
        public decimal? ReportAmount3 { get; set; }
        public string EventCode4 { get; set; }
        public string ReportName4 { get; set; }
        public decimal? ReportAmount4 { get; set; }
        public string EventCode5 { get; set; }
        public string ReportName5 { get; set; }
        public decimal? ReportAmount5 { get; set; }
        public string EventCode6 { get; set; }
        public string ReportName6 { get; set; }
        public decimal? ReportAmount6 { get; set; }
        public string EventCode7 { get; set; }
        public string ReportName7 { get; set; }
        public decimal? ReportAmount7 { get; set; }
        public string EventCode8 { get; set; }
        public string ReportName8 { get; set; }
        public decimal? ReportAmount8 { get; set; }
        public string EventCode9 { get; set; }
        public string ReportName9 { get; set; }
        public decimal? ReportAmount9 { get; set; }
        public string EventCode10 { get; set; }
        public string ReportName10 { get; set; }
        public decimal? ReportAmount10 { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TradeInBrandName { get; set; }
        public string TradeInCarName { get; set; }
        public string TradeInModelYear { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string LoanCompanyName { get; set; }
        public int? LoanFrequency { get; set; }
        public decimal? LoanRate { get; set; }
        public decimal? LoanPrincipal { get; set; }
        public decimal? RemainAmount { get; set; }
        public DateTime? RemainFinalMonth { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AuthenticationNumber { get; set; }
        public decimal? LoanCampaign { get; set; }
        public string PurchaseMemo { get; set; }
        public string NoRegistration { get; set; }
        public string CancelFlag { get; set; }
        public string NoReservation { get; set; }
        // Add 2014/09/25 arc amii 西暦和暦対応 #3082 登録日と登録予定日の和暦を追加
        public JapaneseDate RegistrationPlanDateWareki { get; set; }
        public JapaneseDate RegistrationDateWareki { get; set; }
    }
    public partial class CashBalance : CrmsAuth, ICrmsModel
    {
        public decimal AmountOf10000 { get; set; }
        public decimal AmountOf5000 { get; set; }
        public decimal AmountOf2000 { get; set; }
        public decimal AmountOf1000 { get; set; }
        public decimal AmountOf500 { get; set; }
        public decimal AmountOf100 { get; set; }
        public decimal AmountOf50 { get; set; }
        public decimal AmountOf10 { get; set; }
        public decimal AmountOf5 { get; set; }
        public decimal AmountOf1 { get; set; }
        public void calculate()
        {
            AmountOf10000 = (this.NumberOf10000 == null ? 0m : decimal.Multiply(10000m, this.NumberOf10000 ?? 0m));
            AmountOf5000 = (this.NumberOf5000 == null ? 0m : decimal.Multiply(5000m, this.NumberOf5000 ?? 0m));
            AmountOf2000 = (this.NumberOf2000 == null ? 0m : decimal.Multiply(2000m, this.NumberOf2000 ?? 0m));
            AmountOf1000 = (this.NumberOf1000 == null ? 0m : decimal.Multiply(1000m, this.NumberOf1000 ?? 0m));
            AmountOf500 = (this.NumberOf500 == null ? 0m : decimal.Multiply(500m, this.NumberOf500 ?? 0m));
            AmountOf100 = (this.NumberOf100 == null ? 0m : decimal.Multiply(100m, this.NumberOf100 ?? 0m));
            AmountOf50 = (this.NumberOf50 == null ? 0m : decimal.Multiply(50m, this.NumberOf50 ?? 0m));
            AmountOf10 = (this.NumberOf10 == null ? 0m : decimal.Multiply(10m, this.NumberOf10 ?? 0m));
            AmountOf5 = (this.NumberOf5 == null ? 0m : decimal.Multiply(5m, this.NumberOf5 ?? 0m));
            AmountOf1 = (this.NumberOf1 == null ? 0m : decimal.Multiply(1m, this.NumberOf1 ?? 0m));
            decimal totalAmount;
            totalAmount = AmountOf10000;
            totalAmount = decimal.Add(totalAmount, AmountOf5000);
            totalAmount = decimal.Add(totalAmount, AmountOf2000);
            totalAmount = decimal.Add(totalAmount, AmountOf1000);
            totalAmount = decimal.Add(totalAmount, AmountOf500);
            totalAmount = decimal.Add(totalAmount, AmountOf100);
            totalAmount = decimal.Add(totalAmount, AmountOf50);
            totalAmount = decimal.Add(totalAmount, AmountOf10);
            totalAmount = decimal.Add(totalAmount, AmountOf5);
            totalAmount = decimal.Add(totalAmount, AmountOf1);
            totalAmount = decimal.Add(totalAmount, CheckAmount ?? 0m);
            TotalAmount = totalAmount;
        }
    }
    public partial class Company : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    //ADD 2014/02/20 ookubo
    public partial class ConsumptionTax : CrmsAuth, ICrmsModel
    {
        public string DelFlagName { get; set; }
    }

    public partial class Coupon : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class Customer : ICrmsModel
    {
        public string TelNumber4 { get; set; }
        public string DelFlagName { get; set; }
        public DateTime? BirthdayFrom { get; set; }
        public DateTime? BirthdayTo { get; set; }
        public DateTime? FirstReceiptionDateFrom { get; set; }
        public DateTime? FirstReceiptionDateTo { get; set; }
        public DateTime? LastReceiptionDateFrom { get; set; }
        public DateTime? LastReceiptionDateTo { get; set; }
        // ADD 2014/05/26 vs2012対応 arc uchida データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FirstRegistrationFrom { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FirstRegistrationTo { get; set; }
        public DateTime? ExpireDateFrom { get; set; }
        public DateTime? ExpireDateTo { get; set; }
        public DateTime? RegistrationDateFrom { get; set; }
        public DateTime? RegistrationDateTo { get; set; }
        public DateTime? NextInspectionDateFrom { get; set; }
        public DateTime? NextInspectionDateTo { get; set; }
        public DateTime? SalesDateFrom { get; set; }
        public DateTime? SalesDateTo { get; set; }
        public DateTime? SalesOrderDateFrom { get; set; }
        public DateTime? SalesOrderDateTo { get; set; }
        public List<Department> DepartmentList { get; set; }
        public List<Car> CarList { get; set; }
        public bool InterestedCustomer { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarBrandCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarGradeCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MorterViecleOfficialCode { get; set; }
        public bool BasicHasErrors { get; set; }
        public bool CustomerHasErrors { get; set; }
        public bool CustomerClaimHasErrors { get; set; }
        public bool SupplierHasErrors { get; set; }
        public bool SupplierPaymentHasErrors { get; set; }
        public bool CustomerDMHasErrors { get; set; }

    }
    //Mod 2016/04/14 arc yano #3480 請求先タイプリストの検索条件を追加
    public partial class CustomerClaim : ICrmsModel
    {
        public string DelFlagName { get; set; }
        public List<string> CustomerClaimTypeList { get; set; }     //請求先リスト
    }
    public partial class CustomerClaimable : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class CustomerReceiption : CrmsAuth, ICrmsModel
    {
        public DateTime? ReceiptionDateFrom { get; set; }
        public DateTime? ReceiptionDateTo { get; set; }
    }
    public partial class ServiceReceiptionHistory
    {
        public Customer Customer { get; set; }
        public SalesCar SalesCar { get; set; }
        //public List<V_ServiceReceiptionHistory> V_ServiceReceiptionHistory { get; set; }
        public PaginatedList<ServiceSalesHeader> ServiceSalesHeader { get; set; }
    }
    public partial class Department : CrmsAuth, ICrmsModel
    {
        public string DelFlagName { get; set; }
        public List<V_CarSummary> CarQuoteList { get; set; }
        public List<V_CarSummary> CarSalesOrderList { get; set; }
        public List<V_CarSummary> CarSalesList { get; set; }
        public List<V_ServiceSummary> ServiceQuoteList { get; set; }
        public List<V_ServiceSummary> ServiceSalesOrderList { get; set; }
        public List<V_ServiceSummary> ServiceSalesList { get; set; }
    }
    public partial class DocumentExportCondition : ICrmsModel
    {
        /// <summary>
        /// 期間FROM
        /// </summary>
        public DateTime? TermFrom { get; set; }

        /// <summary>
        /// 期間TO
        /// </summary>
        public DateTime? TermTo { get; set; }

        /// <summary>
        /// 車両伝票ステータス
        /// </summary>
        // ADD 2014/05/26 vs2012対応 arc uchida データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesOrderStatus { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesOrderStatusName { get; set; }

        /// <summary>
        /// サービス伝票ステータス
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceOrderStatus { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceOrderStatusName { get; set; }

        /// <summary>
        /// 車両ステータス
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarStatus { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarStatusName { get; set; }

        /// <summary>
        /// 新中区分
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedType { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedTypeName { get; set; }

        /// <summary>
        /// 顧客ランク
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerRank { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerRankName { get; set; }

        /// <summary>
        /// 顧客区分
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerType { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerTypeName { get; set; }

        /// <summary>
        /// 顧客種別
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerKind { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerKindName { get; set; }

        /// <summary>
        /// 担当者
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }

        /// <summary>
        /// 部門コード
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }

        /// <summary>
        /// 最終更新日
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }

        /// <summary>
        /// ロケーション種別
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationType { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationTypeNane { get; set; }

        /// <summary>
        /// 請求先種別
        /// </summary>
        public string CustomerClaimType { get; set; }
        public string CustomerClaimTypeName { get; set; }

        /// <summary>
        /// 入金予定種別
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ReceiptPlanType { get; set; }
        public string ReceiptPlanTypeName { get; set; }

        /// <summary>
        /// 支払予定種別
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PaymentPlanType { get; set; }
        public string PaymentPlanTypeName { get; set; }

        /// <summary>
        /// 閲覧権限
        /// </summary>
        public Employee AuthEmployee { get; set; }

        /// <summary>
        /// DMフラグ
        /// </summary>
        public string DmFlag { get; set; }
        public string DmFlagName { get; set; }

        /// <summary>
        /// 預かり
        /// </summary>
        public string StopFlag { get; set; }

        /// <summary>
        /// 事業所
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OfficeCode { get; set; }
        public string OfficeName { get; set; }

        /// <summary>
        /// 顧客名
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }

        /// <summary>
        /// 仕入先
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }

        /// <summary>
        /// 伝票番号
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }

        /// <summary>
        /// 会社
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }

        /// <summary>
        /// 対象月
        /// </summary>
        public string TargetMonth { get; set; }

        //Add 2014/08/20 arc yano IPO対応
        /// <summary>
        /// 種別
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 請求先コード
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerClaimCode { get; set; }
        public string CustomerClaimName { get; set; }

        /// <summary>
        /// 初年度登録FROM
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FirstRegistrationFrom { get; set; }

        /// <summary>
        /// 初年度登録TO
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FirstRegistrationTo { get; set; }

        /// <summary>
        /// 次回車検日FROM
        /// </summary>
        public DateTime? ExpireDateFrom { get; set; }

        /// <summary>
        /// 次回車検日TO
        /// </summary>
        public DateTime? ExpireDateTo { get; set; }

        /// <summary>
        /// 次回点検日FROM
        /// </summary>
        public DateTime? NextInspectionDateFrom { get; set; }

        /// <summary>
        /// 次回点検日TO
        /// </summary>
        public DateTime? NextInspectionDateTo { get; set; }

        /// <summary>
        /// 登録年月日FROM
        /// </summary>
        public DateTime? RegistrationDateFrom { get; set; }

        /// <summary>
        /// 登録年月日TO
        /// </summary>
        public DateTime? RegistrationDateTo { get; set; }

        /// <summary>
        /// 納車年月日FROM
        /// </summary>
        public DateTime? SalesDateFrom { get; set; }

        /// <summary>
        /// 納車年月日TO
        /// </summary>
        public DateTime? SalesDateTo { get; set; }

        /// <summary>
        /// 受注年月日FROM
        /// </summary>
        public DateTime? SalesOrderDateFrom { get; set; }

        /// <summary>
        /// 受注年月日TO
        /// </summary>
        public DateTime? SalesOrderDateTo { get; set; }

        /// <summary>
        /// 初回来店日FROM
        /// </summary>
        public DateTime? FirstReceiptionDateFrom { get; set; }

        /// <summary>
        /// 初回来店日TO
        /// </summary>
        public DateTime? FirstReceiptionDateTo { get; set; }

        /// <summary>
        /// 前回来店日FROM
        /// </summary>
        public DateTime? LastReceiptionDateFrom { get; set; }

        /// <summary>
        /// 前回来店日TO
        /// </summary>
        public DateTime? LastReceiptionDateTo { get; set; }

        /// <summary>
        /// 見込み客
        /// </summary>
        public bool InterestedCustomer { get; set; }

        /// <summary>
        /// ナンバープレート
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MorterViecleOfficialCode { get; set; }

        /// <summary>
        /// ブランドコード
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarBrandCode { get; set; }
        public string CarBrandName { get; set; }

        /// <summary>
        /// 車種コード
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarCode { get; set; }
        public string CarName { get; set; }

        /// <summary>
        /// グレードコード
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarGradeCode { get; set; }
        public string CarGradeName { get; set; }

        /// <summary>
        /// 部門リスト
        /// </summary>
        public List<Department> DepartmentList { get; set; }

        /// <summary>
        /// 車種リスト
        /// </summary>
        public List<Car> CarList { get; set; }

        //Add 2014/08/05 arc yano IPO対応
        /// <summary>
        /// 管理番号
        /// </summary>
        public string SalesCarNumber { get; set; }

        /// <summary>
        /// 車台番号
        /// </summary>
        public string Vin { get; set; }

        //Add 2014/12/08 arc yano IPO対応(部品棚卸対応)
        /// <summary>
        /// 対象年月
        /// </summary>
        public DateTime? TargetDate { get; set; }

        /// <summary>
        /// ロケーション
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationName { get; set; }

        /// <summary>
        /// 部品番号
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNameJp { get; set; }
        /// <summary>
        /// 在庫数量あり／なし判定
        /// </summary>
        public bool StockZeroVisibility { get; set; }

        //Add 2014/12/18 arc nakayama 顧客DM対応
        /// <summary>
        /// 備考(DM可否)
        /// </summary>
        public string DmMemo { get; set; }

        /// <summary>
        /// 車検案内(DM可否)
        /// </summary>
        public string InspectGuidFlag { get; set; }
        public string InspectGuidFlagName { get; set; }

        /// <summary>
        /// 備考(DM可否)
        /// </summary>
        public string InspectGuidMemo { get; set; }

        /// <summary>
        /// 顧客コード(顧客情報)
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 郵便番号(顧客住所)
        /// </summary>
        public string CustomerPostCode { get; set; }

        /// <summary>
        /// 都道府県(顧客住所)
        /// </summary>
        public string CustomerPrefecture { get; set; }

        /// <summary>
        /// 住所(顧客住所)
        /// </summary>
        public string CustomerAddress { get; set; }

        /// <summary>
        /// 電話番号(顧客住所)
        /// </summary>
        public string CustomerTelNumber { get; set; }

        /// <summary>
        /// 住所再確認(顧客住所)
        /// </summary>
        public bool? CustomerAddressReconfirm { get; set; }
        public string SearchAddressReconfirmName { get; set; }

        /// <summary>
        /// 住所再確認(顧客住所)
        /// </summary>
        public string CustomerAddressReconfirmName { get; set; }

        /// <summary>
        /// 郵便番号(DM発送先住所)
        /// </summary>
        public string DmPostCode { get; set; }

        /// <summary>
        /// 都道府県(DM発送先住所)
        /// </summary>
        public string DmPrefecture { get; set; }

        /// <summary>
        /// 住所(DM発送先住所)
        /// </summary>
        public string DmCustomerDmAddress { get; set; }

        /// <summary>
        /// 電話番号(DM発送先住所)
        /// </summary>
        public string DmTelNumber { get; set; }

        /// <summary>
        /// メーカー(車両情報)
        /// </summary>
        public string MakerName { get; set; }

        /// <summary>
        /// 初年度登録FROM
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public DateTime? DtFirstRegistrationFrom { get; set; }

        /// <summary>
        /// 初年度登録TO
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public DateTime? DtFirstRegistrationTo { get; set; }

        /// <summary>
        /// 初年度登録日
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DtFirstRegistrationDate { get; set; }

        /// <summary>
        /// 登録年月日(車両情報)
        /// </summary>
        public DateTime? RegistrationDate { get; set; }

        /// <summary>
        /// 次回点検日(車両情報)
        /// </summary>
        public DateTime? NextInspectionDate { get; set; }

        /// <summary>
        /// 車検満了日(車両情報)
        /// </summary>
        public DateTime? ExpireDate { get; set; }

        /// <summary>
        /// 部門コード(営業担当)
        /// </summary>
        public string SalesDepartmentCode { get; set; }

        /// <summary>
        /// 部門名(営業担当)
        /// </summary>
        public string SalesDepartmentName { get; set; }

        /// <summary>
        /// 担当者名(営業担当)
        /// </summary>
        public string SalesEmployeeName { get; set; }

        /// <summary>
        /// 納車年月日(営業担当)
        /// </summary>
        public DateTime? SalesDate { get; set; }

        /// <summary>
        /// 部門名(サービス)
        /// </summary>
        public string ServiceDepartmentCode { get; set; }

        /// <summary>
        /// 部門名(サービス)
        /// </summary>
        public string ServiceDepartmentName { get; set; }

        /// <summary>
        /// 担当者名(サービス)
        /// </summary>
        public string ServiceEmployeeName { get; set; }

        /// <summary>
        /// 納車日(サービス)
        /// </summary>
        public DateTime? ArrivalPlanDate { get; set; }

        /// <summary>
        /// 納車日（営業）From
        /// </summary>
        public DateTime? SalesDateFromForDm { get; set; }

        /// <summary>
        /// 納車日（営業）To
        /// </summary>
        public DateTime? SalesDateToForDm { get; set; }

        //Add 2015/01/08 arc nakayama 顧客DM指摘事項⑨　検索項目の追加（車種名・メーカー名・納車日(営業/サービス)・担当者(営業/サービス)）
        /// <summary>
        /// 納車日(サービス)From
        /// </summary>
        public DateTime? ArrivalPlanDateFrom { get; set; }

        /// <summary>
        /// 納車日(サービス)To
        /// </summary>
        public DateTime? ArrivalPlanDateTo { get; set; }

        /// <summary>
        /// 次回車検日FROM(顧客DMリスト専用)
        /// </summary>
        public DateTime? ExpireDateFromForDm { get; set; }

        /// <summary>
        /// 次回車検日TO(顧客DMリスト専用)
        /// </summary>
        public DateTime? ExpireDateToForDm { get; set; }

        //Add 2015/02/17 arc nakayama 顧客DM仕様変更対応（顧客マスタに紐づく項目追加、営業担当部門コード・営業担当部門名・営業担当者コード・営業担当者名・サービス担当部門コード・サービス担当部門名）

        /// <summary>
        /// 営業担当部門コード
        /// </summary>
        public string DepartmentCode2 { get; set; }

        /// <summary>
        /// 営業担当部門名
        /// </summary>
        public string DepartmentName2 { get; set; }

        /// <summary>
        /// 営業担当者コード
        /// </summary>
        public string CarEmployeeCode { get; set; }

        /// <summary>
        /// 営業担当者名
        /// </summary>
        public string CarEmployeeName { get; set; }

        /// <summary>
        /// サービス担当部門コード
        /// </summary>
        public string ServiceDepartmentCode2 { get; set; }

        /// <summary>
        /// サービス担当部門名
        /// </summary>
        public string ServiceDepartmentName2 { get; set; }

        /// <summary>
        /// サービス担当者名
        /// </summary>
        public string ServiceEmployeeName2 { get; set; }


        /// <summary>
        /// 伝票タイプ
        /// </summary>
        public string SlipType { get; set; }
        public string SlipTypeName { get; set; }

        /// <summary>
        /// 請求先区分
        /// </summary>
        public string Code { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// 入金予定金額
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// 前金
        /// </summary>
        public string MaeAmount { get; set; }

        /// <summary>
        /// 前金
        /// </summary>
        public string AtoAmount { get; set; }

        /// <summary>
        /// 残高
        /// </summary>
        public string BalanceAmount { get; set; }


        //Add 2015/02/16 arc iijima 現金出納帳出力検索項目追加
        /// <summary>
        /// 現金出納帳出力(店舗共通)
        /// </summary>
        public DateTime? Lastdate { get; set; }
        public string CashAccountCode { get; set; }
        public string CashAccountName { get; set; }

        //Add 2015/05/20 arc nakayama #3199_顧客データリスト出力項目追加
        /// <summary>
        /// 車両重量(kg)
        /// </summary>
        public int? CarWeight { get; set; }

        /// <summary>
        /// 預かり車両
        /// </summary>
        /// <history>
        /// 2017/09/04 arc yano #3786 預かり車Excle出力対応 新規追加
        /// </history>
        public bool? KeepsCarFlag { get; set; }

        /// <summary>
        /// 削除フラグ
        /// </summary>
        /// <history>
        /// 2020/11/16 yano #4070 【店舗管理帳票】車両伝票一覧のデータ抽出条件の誤り
        /// </history>
        public string DelFlag { get; set; }


    }
    public partial class Employee : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class InstallableOption : ICrmsModel
    {
    }
    public partial class InstallableParts : ICrmsModel
    {
    }
    public partial class Inventory : CrmsAuth, ICrmsModel
    {
    }
    public partial class InventorySchedule : CrmsAuth, ICrmsModel
    {
    }
    //Add 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)
    public partial class InventoryScheduleParts : CrmsAuth, ICrmsModel
    {
    }
    //Mod 2015/04/23 arc yano 部品棚卸 棚卸有無を追加
    public partial class InventoryStock : CrmsAuth, ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNameJp { get; set; }
        public bool DiffQuantity { get; set; }         //棚卸有無
    }
    public partial class Journal : CrmsAuth, ICrmsModel
    {
        public DateTime? JournalDateFrom { get; set; }
        public DateTime? JournalDateTo { get; set; }

        /// <summary>
        /// 締め処理済みかどうか
        /// </summary>
        public bool IsClosed { get; set; }

        public DateTime? CondJournalDate { get; set; }
    }
    public partial class Loan : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    //2016/08/13 arc yano #3596 【大項目】部門棚統合対応
    public partial class Location : ICrmsModel
    {
        public string DelFlagName { get; set; }
        public string DepartmentName { get; set; }
        public string WarehouseName { get; set; }
        public string LocationTypeName { get; set; }
        public string BusinessType { get; set; }
    }
    public partial class Maker : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class MenuGroup : CrmsAuth, ICrmsModel
    {
    }
    public partial class Office : CrmsAuth, ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class Parts : ICrmsModel
    {
        public string DelFlagName { get; set; }
        public decimal Quantity { get; set; }
        public bool? InstallableFlag { get; set; }
        // ADD 2014/05/26 vs2012対応 arc uchida データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarBrandCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarBrandName { get; set; }
    }
    public partial class PartsAverageCostControl : ICrmsModel
    {
    }
    //2016/08/13 arc yano #3596 【大項目】部門棚統合対応
    public partial class PartsLocation : ICrmsModel
    {
        public string DelFlagName { get; set; }
        public string DepartmentName { get; set; }
        public string WarehouseName { get; set; }
        public string PartsNameJp { get; set; }
        public string LocationName { get; set; }
    }

    //Add 2016/08/13 arc yano #3596 【大項目】部門棚統合対応
    public partial class PartsStock : ICrmsModel
    {
        public string DepartmentCode { get; set; }
        public string StockZeroVisibility { get; set; }
    }

    public partial class PartsPurchase : CrmsAuth, ICrmsModel
    {
        public DateTime? PurchasePlanDateFrom { get; set; }
        public DateTime? PurchasePlanDateTo { get; set; }
        public DateTime? PurchaseDateFrom { get; set; }
        public DateTime? PurchaseDateTo { get; set; }
        // ADD 2014/05/26 vs2012対応 arc uchida データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OfficeCode { get; set; }
        public bool IsClosedMonth { get; set; }
        public bool IsStank { get; set; }                 // Add 2014/09/10 arc amii 部品入荷履歴対応  入荷日と棚卸日の比較結果追加
        public DateTime? LinkEntryCaptureDateFromFrom { get; set; }     //Add 2018/03/26 arc yano #3863
        public DateTime? LinkEntryCaptureDateFromTo { get; set; }       //Add 2018/03/26 arc yano #3863
    }
    public partial class PartsPurchaseOrder : CrmsAuth, ICrmsModel
    {
        public DateTime? PurchaseOrderDateFrom { get; set; }
        public DateTime? PurchaseOrderDateTo { get; set; }
        // ADD 2014/05/26 vs2012対応 arc uchida データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseOrderNumberFrom { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseOrderNumberTo { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string WebOrderNumberFrom { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string WebOrderNumberTo { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerOrderNumberFrom { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerOrderNumberTo { get; set; }
        public ServiceSalesHeader ServiceSalesHeader { get; set; }
        //Mod 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string GenuineType { get; set; }
        //Add 2015/11/09 #3291 arc yano 部品仕入機能改善(部品発注入力)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNameJp { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierPaymentName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EmployeeName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OrderTypeName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerPartsNumber { get; set; }
    }
    public partial class PartsStock : CrmsAuth, ICrmsModel
    {
    }
    public partial class PaymentKind : ICrmsModel
    {
        public string ClaimDayName { get; set; }
        public string PaymentDayName { get; set; }
        public string DelFlagName { get; set; }
    }
    public partial class PaymentPlan : CrmsAuth, ICrmsModel
    {
        public DateTime? PaymentPlanDateFrom { get; set; }
        public DateTime? PaymentPlanDateTo { get; set; }
    }
    public partial class QuoteMessage : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class ReceiptPlan : CrmsAuth, ICrmsModel
    {
        public DateTime? JournalDateFrom { get; set; }
        public DateTime? JournalDateTo { get; set; }
        // ADD 2014/05/26 vs2012対応 arc uchida データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OfficeCode { get; set; }
        public DateTime? ReceiptPlanDateFrom { get; set; }
        public DateTime? ReceiptPlanDateTo { get; set; }
        public CarSalesHeader CarSalesHeader { get; set; }
        public ServiceSalesHeader ServiceSalesHeader { get; set; }
        public DateTime? SalesDateFrom { get; set; }
        public DateTime? SalesDateTo { get; set; }
        //Add 2016/01/07 arc nakayama #3303_入金消込登録でチェックが付いていないものも処理されてしまう
        public string targetFlag { get; set; }
        public bool Changetarget { get; set; }
        //2016/05/18 arc yano #3558 入金管理 請求先タイプの絞込み追加
        public string CustomerClaimFilter { get; set; }
        //Add 2016/07/19 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）表示単位を変更できる件条件追加
        public string DispType { get; set; }
        public string accountUsageType { get; set; }
        public DateTime? SalesDate { get; set; }
        public DateTime? SalesPlanDate { get; set; }
        public string strReceiptPlanId { get; set; }
        public bool IsShopDeposit { get; set; }
        public string OfficeName { get; set; }      //Add 2019/02/14 #3978
    }
    public partial class SalesCar : ICrmsModel
    {
        public string DelFlagName { get; set; }
        public DateTime? ExpireDateFrom { get; set; }
        public DateTime? ExpireDateTo { get; set; }
        public DateTime? NextInspectionDateFrom { get; set; }
        public DateTime? NextInspectionDateTo { get; set; }
        public string Prefix { get; set; }
        public string CarBrandName { get; set; }
        public string CarGradeName { get; set; }
        public string CarName { get; set; }
        public string LocationName { get; set; }
        // ADD 2014/05/26 vs2012対応 arc uchida データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OwnershipChangeType { get; set; }
        public DateTime? OwnershipChangeDate { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OwnershipChangeMemo { get; set; }
        public JapaneseDate IssueDateWareki { get; set; }
        public JapaneseDate RegistrationDateWareki { get; set; }
        public JapaneseDate FirstRegistrationDateWareki { get; set; }
        public JapaneseDate ExpireDateWareki { get; set; }
        public JapaneseDate SalesDateWareki { get; set; }
        public JapaneseDate InspectionDateWareki { get; set; }
        public JapaneseDate NextInspectionDateWareki { get; set; }
        public bool CarStatusEnabled { get; set; }                 //Add 2014/08/14 arc amii 在庫ステータス変更対応 #3071 在庫ステータスのgetsetメソッド追加
        public bool CarUsageEnabled { get; set; }                  //Add 2015/07/29 arc nakayama #3217_デモカーが販売できてしまう問題の改善　使用用途の変更可/不可を切り替えるフラグ追加
    }
    public partial class CarSalesHeader : CrmsAuth, ICrmsModel
    {
        public DateTime? QuoteDateFrom { get; set; }
        public DateTime? QuoteDateTo { get; set; }
        public DateTime? SalesOrderDateFrom { get; set; }
        public DateTime? SalesOrderDateTo { get; set; }
        public bool BasicEnabled { get; set; }
        public bool CarEnabled { get; set; }
        public bool OptionEnabled { get; set; }
        public bool CostEnabled { get; set; }
        public bool UsedEnabled { get; set; }
        public bool LoanEnabled { get; set; }
        public bool PaymentEnabled { get; set; }
        public bool InsuranceEnabled { get; set; }
        public bool CustomerEnabled { get; set; }
        public bool TotalEnabled { get; set; }
        public bool OwnerEnabled { get; set; }
        public bool SalesDateEnabled { get; set; }
        public bool RegistEnabled { get; set; }
        public bool RequestEnabled { get; set; }
        public bool ReasonEnabled { get; set; }      //Add  2017/11/10 arc yano #3787
        public int ShowMenuIndex { get; set; }
        public CarPurchaseOrder CarPurchaseOrder { get; set; }
        public CarPurchase CarPurchase { get; set; }
        public bool SalesOrderDateEnabled { get; set; }
        public bool BasicHasErrors { get; set; }
        public bool UsedHasErrors { get; set; }
        public bool InvoiceHasErrors { get; set; }
        public bool LoanHasErrors { get; set; }
        public bool VoluntaryInsuranceHasErrors { get; set; }
        public DateTime? SalesDateFrom { get; set; }
        public DateTime? SalesDateTo { get; set; }
        // ADD 2014/05/16 vs2012対応 arc yano データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Reason { get; set; }
        public bool IsClosed { get; set; }
        public bool IsCreated { get; set; }
        public bool IsAkaKuro { get; set; }
        public CarSalesHeader OriginalCarSalesHeader { get; set; }
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
        public bool SalesPlanDateEnabled { get; set; }  //ADD 2014/02/20 ookubo
        public bool RateEnabled { get; set; }           //ADD 2014/02/20 ookubo
        public bool CarRegEnabled { get; set; }         //ADD 2016/07/08 nishimura
        public string LastEditMessage { get; set; } //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
        public bool DispPersonalInfo { get; set; }      //ADD 2017/01/21 arc yano #3657
        public bool PInfoChekEnabled { get; set; }      //ADD 2017/01/21 arc yano #3657
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ActionType { get; set; }                               //Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行
        public bool ModificationControlCommit { get; set; }                  //Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行
        public bool ModificationEnabled { get; set; }                        //Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行  伝票修正ボタンの表示/非表示
        public bool ModificationControl { get; set; }                        //Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行  修正中かどうか（true:修正中 false:修正中でない）
        public bool ModificationControlCancel { get; set; }                  //Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行  修正中の[修正キャンセル]ボタンの表示/非表示
        public bool ModifiedReasonEnabled { get; set; }                      //Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行  修正履歴の表示/非表示（true:表示 false:非表示）
        public List<CarSalesModifiedReason> ModifiedReasonList { get; set; } //Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行  修正履歴
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LockEmployeeName { get; set; }                         //Add 2017/11/11 arc yano #3787 車両伝票で古い伝票で上書き防止する機能
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CampaignName1 { get; set; }                            //Add 2017/11/21 arc yano 
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CampaignName2 { get; set; }                            //Add 2017/11/21 arc yano

        public bool CarVinEnabled { get; set; }                              //車台番号活性フラグ   Add 2018/08/07 yano #3911
        public bool RegistButtonVisible { get; set; }                        //登録ボタン表示フラグ Add 2018/08/07 yano #3911

        public bool CostAreaEnabled { get; set; }                            //Add 2020/01/06 yano #4029

        public bool LoanCompleted { get; set; }                              //2021/08/06 #4088

        public bool FromCopy { get; set; }                                  //Add 2022/05/20 yano #4069
    }

    //Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行
    public class CarSalesModifiedReason
    {
        /// <summary>
        /// 修正時間
        /// </summary>
        public DateTime? ModifiedTime { get; set; }
        /// <summary>
        /// 修正者名
        /// </summary>
        public string ModifiedEmployeeName { get; set; }
        /// <summary>
        /// 修正理由
        /// </summary>
        public string ModifiedReason { get; set; }
    }
    public partial class SecurityRole : ICrmsModel
    {
    }
    public partial class ServiceCost : ICrmsModel
    {
    }
    public partial class ServiceMenu : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    // ADD 2014/05/28 vs2012対応 arc uchida データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
    public partial class ServiceRequest : CrmsAuth, ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EmployeeCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }
        public DateTime? ArrivalPlanDateFrom { get; set; }
        public DateTime? ArrivalPlanDateTo { get; set; }
        public CarPurchaseOrder CarPurchaseOrder { get; set; }
        public CarSalesHeader CarSalesHeader { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }
        public DateTime? DeliveryRequirementFrom { get; set; }
        public DateTime? DeliveryRequirementTo { get; set; }
    }

    // ADD 2014/05/26 vs2012対応 arc uchida データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
    public partial class ServiceSalesHeader : CrmsAuth, ICrmsModel
    {
        public DateTime? QuoteDateFrom { get; set; }
        public DateTime? QuoteDateTo { get; set; }
        public DateTime? SalesOrderDateFrom { get; set; }
        public DateTime? SalesOrderDateTo { get; set; }
        public string ServiceType { get; set; }
        public bool SlipEnabled { get; set; }
        public bool CarEnabled { get; set; }
        public bool CostEnabled { get; set; }
        public bool LineEnabled { get; set; }
        public bool PaymentEnabled { get; set; }
        public bool CustomerEnabled { get; set; }
        public bool SalesDateEnabled { get; set; }
        public int ShowMenuIndex { get; set; }
        public string VinFull { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceWorkCode { get; set; }
        public decimal TotalCost { get; set; }
        public bool BasicHasErrors { get; set; }
        public bool InvoiceHasErrors { get; set; }
        public bool TaxHasErrors { get; set; }
        public DateTime? SalesDateFrom { get; set; }
        public DateTime? SalesDateTo { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Reason { get; set; }
        public bool IsClosed { get; set; }
        public bool IsCreated { get; set; }
        public bool WithoutAkaden { get; set; }
        // ADD 2014/05/08 vs2012対応 arc yano データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ActionType { get; set; }
        //[DisplayFormat(ConvertEmptyStringToNull = false)]
        //public string CustomerClaimCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerClaimName { get; set; }
        public List<ServiceClaimable> ServiceClaimable { get; set; }
        public string Closing { get; set; }
        public string LockEmployeeName { get; set; }
        public bool SalesPlanDateEnabled { get; set; }  //ADD 2014/02/20 ookubo
        public bool RateEnabled { get; set; }           //ADD 2014/02/20 ookubo
        public bool KeepsCarFlagEnabled { get; set; }   //ADD 2014/11/25 #3135 ookubo
        public bool ModificationEnabled { get; set; }   //ADD 2015/03/17 arc nakayama 伝票修正対応  伝票修正ボタンの表示/非表示
        public bool ModificationControl { get; set; }   //ADD 2015/03/17 arc nakayama 伝票修正対応  修正中かどうか（true:修正中 false:修正中でない）
        public bool ModificationControlCommit { get; set; }   //ADD 2015/03/17 arc nakayama 伝票修正対応  修正中の[保存]ボタンの表示/非表示
        public bool ModificationControlCancel { get; set; }   //ADD 2015/03/17 arc nakayama 伝票修正対応  修正中の[修正キャンセル]ボタンの表示/非表示
        public bool ModifiedReasonEnabled { get; set; } //ADD 2015/03/18 arc nakayama 伝票修正対応  修正履歴の表示/非表示（true:表示 false:非表示）
        public List<ServiceModifiedReason> ModifiedReasonList { get; set; }    //ADD 2015/03/18 arc nakayama 伝票修正対応  修正履歴
        public string MileageUnitName { get; set; }
        //Add 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更 注文リストの追加
        public List<string> orderUrlList { get; set; }
        public bool DispPersonalInfo { get; set; }      //ADD 2017/01/21 arc yano #3657
        public bool PInfoChekEnabled { get; set; }      //ADD 2017/01/21 arc yano #3657

        public bool? Output { get; set; }               //Add 2017/10/19 arc yano #3803
        public string StockCode { get; set; }           //Add 2017/10/19 arc yano #3803
        public decimal TaxableCostSubTotalAmount { get; set; }         //Add 2020/02/17 yano #4025
        public decimal TaxableCostSubTotalTaxAmount { get; set; }      //Add 2020/02/17 yano #4025
        public bool ClaimReportOutPut { get; set; }                    //Add 2021/03/23 yano #4078
        public bool ClaimReportChecked { get; set; }                   //Add 2021/03/23 yano #4078
    }
    public partial class ServiceSalesLine : ICrmsModel, ICloneable
    {
        public decimal PartsStock { get; set; }
        public string WorkTypeName { get; set; }
        public string ServiceTypeName { get; set; }
        // ADD 2014/05/26 vs2012対応 arc uchida データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ParentCustomerClaimCode { get; set; }
        //Add 2014/06/12 arc yano 高速化対応
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CliDelFlag { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]       //Add 2016/05/09 arc yano #3480
        public string SWCustomerClaimClass { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]       //Add 2016/05/09 arc yano #3480
        public string CCCustomerClaimClass { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OutputTarget { get; set; }                 //Add 2017/10/19 arc yano #3803
        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    //ADD 2015/03/18 arc nakayama 伝票修正対応  修正履歴リスト
    public class ServiceModifiedReason
    {
        /// <summary>
        /// 修正時間
        /// </summary>
        public DateTime? ModifiedTime { get; set; }
        /// <summary>
        /// 修正者名
        /// </summary>
        public string ModifiedEmployeeName { get; set; }
        /// <summary>
        /// 修正理由
        /// </summary>
        public string ModifiedReason { get; set; }
    }

    //ADD 2015/03/19 arc nakayama 伝票修正対応  部品の在庫リスト（再引き当て用）
    public class MaxPartsLocation
    {
        public string LocationCode { get; set; }
        public string PartsNumber { get; set; }
        public decimal? Quantity { get; set; }
    }

    public class ServiceClaimable
    {
        /// <summary>
        /// 請求先コード
        /// </summary>
        public string CustomerClaimCode { get; set; }

        /// <summary>
        /// 請求先マスタ
        /// </summary>
        public CustomerClaim CustomerClaim { get; set; }

        /// <summary>
        /// 主作業コード
        /// </summary>
        public string ServiceWorkCode { get; set; }

        /// <summary>
        /// 主作業マスタ
        /// </summary>
        public ServiceWork ServiceWork { get; set; }

        /// <summary>
        /// 工賃売上
        /// </summary>
        public decimal TechnicalFeeAmount { get; set; }

        /// <summary>
        /// 部品売上
        /// </summary>
        public decimal PartsAmount { get; set; }

        /// <summary>
        /// 売上計
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 売上計（税込）
        /// </summary>
        public decimal AmountWithTax { get; set; }

        /// <summary>
        /// 工賃原価
        /// </summary>
        public decimal TechnicalCost { get; set; }

        /// <summary>
        /// 部品原価
        /// </summary>
        public decimal PartsCost { get; set; }

        /// <summary>
        /// 原価計
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// 工賃粗利
        /// </summary>
        public decimal TechnicalMargin { get; set; }

        /// <summary>
        /// 部品粗利
        /// </summary>
        public decimal PartsMargin { get; set; }

        /// <summary>
        /// 粗利計
        /// </summary>
        public decimal Margin { get; set; }

        /// <summary>
        /// 粗利率
        /// </summary>
        public decimal MarginRate { get; set; }

        /// <summary>
        /// 諸費用計
        /// </summary>
        public decimal DepositAmount { get; set; }
    }
    public partial class ServiceWork : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class SetMenuList : ICrmsModel
    {
        public int? InputDetailsNumber { get; set; }
    }
    public partial class Supplier : ICrmsModel
    {
        public string OutsourceFlagName { get; set; }
        public string DelFlagName { get; set; }
    }
    public partial class SupplierPayment : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class Task : ICrmsModel
    {
        public bool TaskStatus { get; set; }
        public string CustomerName { get; set; }
    }
    public partial class TaskConfig : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    //Mod 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 検索条件(倉庫コード)追加
    public partial class Transfer : CrmsAuth, ICrmsModel
    {
        public DateTime? DepartureDateFrom { get; set; }
        public DateTime? DepartureDateTo { get; set; }
        public DateTime? ArrivalDateFrom { get; set; }
        public DateTime? ArrivalDateTo { get; set; }
        public bool TransferConfirm { get; set; }
        public bool TransferUnConfirm { get; set; }
        public string CarOrParts { get; set; }
        // ADD 2014/05/26 vs2012対応 arc uchida データフィールドが空文字の場合は、空文字でバインドする(null変換しない)ようにプロパティを修正
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }
        public bool IsClosedMonth { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string WarehouseCode { get; set; }           //倉庫コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ActionType { get; set; }  //動作種別
    }
    public partial class TransportBranchOffice : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }
    public partial class V_CarAppraisal : ICrmsModel
    {
        public DateTime? CreateDateFrom { get; set; }
        public DateTime? CreateDateTo { get; set; }
    }
    public partial class V_CarInventoryInProcess : CrmsAuth, ICrmsModel
    {
        public bool DefferenceSelect { get; set; }
    }
    public partial class V_CarInventorySummary : CrmsAuth, ICrmsModel
    {
    }
    public partial class V_PartsInventoryInProcess : CrmsAuth, ICrmsModel
    {
        public bool DefferenceSelect { get; set; }
    }
    public partial class V_PartsInventorySummary : CrmsAuth, ICrmsModel
    {
    }
    public partial class V_CarPurchaseList : CrmsAuth, ICrmsModel
    {
        public DateTime? PurchasePlanDateFrom { get; set; }
        public DateTime? PurchasePlanDateTo { get; set; }
    }
    public partial class V_ServiceReceiptTarget : ICrmsModel
    {
        public DateTime? FirstReceiptionDateFrom { get; set; }
        public DateTime? FirstReceiptionDateTo { get; set; }
        public DateTime? LastReceiptionDateFrom { get; set; }
        public DateTime? LastReceiptionDateTo { get; set; }
    }
    public partial class c_AccountUsageType : ICrmsModel
    {
    }
    public partial class c_AccountType : ICrmsModel
    {
    }
    public partial class c_AcquisitionReason : ICrmsModel
    {
    }
    public partial class c_Allowance : ICrmsModel
    {
    }
    public partial class c_AttractivePoint : ICrmsModel
    {
    }
    public partial class c_BusinessType : ICrmsModel
    {
    }
    public partial class c_CampaignType : ICrmsModel
    {
    }
    public partial class c_CarClassification : ICrmsModel
    {
    }
    public partial class c_CarLiabilityInsuranceType : ICrmsModel
    {
    }
    public partial class c_CarOwner : ICrmsModel
    {
    }
    public partial class c_CarPurchaseType : ICrmsModel
    {
    }
    public partial class c_CarStatus : ICrmsModel
    {
    }
    public partial class c_ColorCategory : ICrmsModel
    {
    }
    public partial class c_CorporationType : ICrmsModel
    {
    }
    public partial class c_CustomerClaimType : ICrmsModel
    {
    }
    public partial class c_CustomerKind : ICrmsModel
    {
    }
    public partial class c_CustomerRank : ICrmsModel
    {
    }
    public partial class c_CustomerType : ICrmsModel
    {
    }
    public partial class c_DeclarationType : ICrmsModel
    {
    }
    public partial class c_Demand : ICrmsModel
    {
    }
    public partial class c_DepositKind : ICrmsModel
    {
    }
    public partial class c_DocumentComplete : ICrmsModel
    {
    }
    public partial class c_DrivingName : ICrmsModel
    {
    }
    public partial class c_EmployeeType : ICrmsModel
    {
    }
    public partial class c_EraseRegist : ICrmsModel
    {
    }
    public partial class c_ExpireType : ICrmsModel
    {
    }
    public partial class c_Figure : ICrmsModel
    {
    }
    public partial class c_Firm : ICrmsModel
    {
    }
    public partial class c_Fuel : ICrmsModel
    {
    }
    public partial class c_GenuineType : ICrmsModel
    {
    }
    public partial class c_Import : ICrmsModel
    {
    }
    public partial class c_InventoryStatus : ICrmsModel
    {
    }
    public partial class c_InventoryType : ICrmsModel
    {
    }
    public partial class c_JournalType : ICrmsModel
    {
    }
    public partial class c_KnowOpportunity : ICrmsModel
    {
    }
    public partial class c_Light : ICrmsModel
    {
    }
    public partial class c_LocationType : ICrmsModel
    {
    }
    public partial class c_MileageUnit : ICrmsModel
    {
    }
    public partial class c_NaviDashboard : ICrmsModel
    {
    }
    public partial class c_NaviEquipment : ICrmsModel
    {
    }
    public partial class c_NewUsedType : ICrmsModel
    {
    }
    public partial class c_Occupation : ICrmsModel
    {
    }
    public partial class c_OnOff : ICrmsModel
    {
    }
    public partial class c_OptionType : ICrmsModel
    {
    }
    public partial class c_OrderType : ICrmsModel
    {
    }
    public partial class c_OwnershipChange : ICrmsModel
    {
    }
    public partial class c_OwnershipChangeType : ICrmsModel
    {
    }
    public partial class c_OwnershipReservation : ICrmsModel
    {
    }
    public partial class c_PaymentKind : ICrmsModel
    {
    }
    public partial class c_PaymentType : ICrmsModel
    {
    }
    public partial class c_PaymentType2 : ICrmsModel
    {
    }
    public partial class c_PurchaseOrderStatus : ICrmsModel
    {
    }
    public partial class c_PurchaseStatus : ICrmsModel
    {
    }
    public partial class c_PurchaseType : ICrmsModel
    {
    }
    public partial class c_Purpose : ICrmsModel
    {
    }
    public partial class c_QuoteType : ICrmsModel
    {
    }
    public partial class c_ReceiptionState : ICrmsModel
    {
    }
    public partial class c_ReceiptionType : ICrmsModel
    {
    }
    public partial class c_ReceiptType : ICrmsModel
    {
    }
    public partial class c_Recycle : ICrmsModel
    {
    }
    public partial class c_RegistMonth : ICrmsModel
    {
    }
    public partial class c_RegistrationType : ICrmsModel
    {
    }
    public partial class c_RequestContent : ICrmsModel
    {
    }
    public partial class c_RoundType : ICrmsModel
    {
    }
    public partial class c_SalesOrderStatus : ICrmsModel
    {
    }
    public partial class c_SalesType : ICrmsModel
    {
    }
    public partial class c_SeatType : ICrmsModel
    {
    }
    public partial class c_SecurityLevel : ICrmsModel
    {
    }
    public partial class c_ServiceOrderStatus : ICrmsModel
    {
    }
    public partial class c_ServiceType : ICrmsModel
    {
    }
    public partial class c_ServiceWorkClass1 : ICrmsModel
    {
    }
    public partial class c_ServiceWorkClass2 : ICrmsModel
    {
    }
    public partial class c_Sex : ICrmsModel
    {
    }
    public partial class c_Sr : ICrmsModel
    {
    }
    public partial class c_Steering : ICrmsModel
    {
    }
    public partial class c_StockStatus : ICrmsModel
    {
    }
    public partial class c_SupplierPaymentType : ICrmsModel
    {
    }
    public partial class c_TargetService : ICrmsModel
    {
    }
    public partial class c_TransferType : ICrmsModel
    {
    }
    public partial class c_TaxationType : ICrmsModel
    {
    }
    public partial class c_TransMission : ICrmsModel
    {
    }
    public partial class c_Usage : ICrmsModel
    {
    }
    public partial class c_UsageType : ICrmsModel
    {
    }
    public partial class c_VehicleType : ICrmsModel
    {
    }
    public partial class c_VisitOpportunity : ICrmsModel
    {
    }
    public partial class c_VoluntaryInsuranceType : ICrmsModel
    {
    }
    public partial class c_WorkType : ICrmsModel
    {
    }
    // Add 2014/10/10 arc amii サブシステム仕訳機能移行対応 新規追加
    public partial class c_CodeName : ICrmsModel
    {
    }
    //2014/07/25 arc yano IPO対応 クラス定義(CarStock)を追加
    public partial class CarStock : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TargetMonth { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DataKind { get; set; }


        //Del 2018/08/28 yano #3922
        //public DateTime? DateFrom { get; set; }
        //public DateTime? DateTo { get; set; }
        //[DisplayFormat(ConvertEmptyStringToNull = false)]
        //public string PurchaseLocationName { get; set; }
        //[DisplayFormat(ConvertEmptyStringToNull = false)]
        //public string CarStockLocationName { get; set; }
        //[DisplayFormat(ConvertEmptyStringToNull = false)]
        //public string CarPurchaseTypeName { get; set; }
        //[DisplayFormat(ConvertEmptyStringToNull = false)]
        //public string SupplierName { get; set; }
        //[DisplayFormat(ConvertEmptyStringToNull = false)]
        //public string PurchaseDepartmentName { get; set; }
        //[DisplayFormat(ConvertEmptyStringToNull = false)]
        //public string CustomerName { get; set; }
        //[DisplayFormat(ConvertEmptyStringToNull = false)]
        //public string NewUsedTypeName { get; set; }     //Add 2016/11/30 arc yano #3659
        //[DisplayFormat(ConvertEmptyStringToNull = false)]
        //public string CarCarName { get; set; }             //Add 2016/11/30 arc yano #3659
    }
    // Add 2014/08/19 IPO対応 
    public partial class c_CarStockDataType : ICrmsModel
    {
    }
    //2014/08/22 arc amii #3069 クラス定義(c_Needed)を追加
    public partial class c_Needed : ICrmsModel
    {
    }
    // Add 2014/08/29 IPO対応 c_Year,c_Month,c_Closetypeを追加 
    public partial class c_Year : ICrmsModel
    {
    }
    public partial class c_Month : ICrmsModel
    {
    }
    public partial class c_CloseType : ICrmsModel
    {
    }
    public partial class JournalExPortsCondition : ICrmsModel
    {
        public DateTime? SalesDateFrom { get; set; }
        public DateTime? SalesDateTo { get; set; }
        public DateTime? PurchaseDateFrom { get; set; }
        public DateTime? PurchaseDateTo { get; set; }
        public DateTime JournalDateFrom { get; set; }
        public DateTime JournalDateTo { get; set; }
        public string DepartmentCode { get; set; }
        public string OfficeCode { get; set; }
        public string DivisionType { get; set; }
        public string PurchaseStatus { get; set; }
    }
    // Add 2014/10/16 車両ステータス追加対応
    public partial class c_CarUsage : ICrmsModel
    {
    }

    //Mod 2017/07/18 arc yano #3779 部品在庫（部品在庫の修正） 新規レコードフラグの追加
    //Mod 2016/02/01 arc yano #3409 フリー在庫表示対応 引当済数、ロケーション種別の追加
    //Add 2014/12/04 arc nakayama 新規部品在庫検索対応　部品在庫クラス(検索結果の格納先)の追加
    public class CommonPartsStock
    {
        public decimal? PhysicalQuantity { get; set; }      //実棚
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Comment { get; set; }                 //コメント
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        //Add 2015/02/22 arc iijima 部門名称出力対応
        public string DepartmentCode { get; set; }          //部門コード         //2018/06/01 arc yano #3900
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }
        public string LocationCode { get; set; }            //ロケーションコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationName { get; set; }            //ロケーション名称
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }             //部品番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNameJp { get; set; }             //部品名(日本語)
        public decimal? Quantity { get; set; }              //数量
        public decimal? ProvisionQuantity { get; set; }     //引当済数量     //Mod 2016/02/01 arc yano #3409
        public decimal? SaveQuantity { get; set; }          //棚卸開始時の数量
        public decimal? StandardPrice { get; set; }         //標準原価
        public decimal? MoveAverageUnitPrice { get; set; }  //移動平均単価
        public decimal? Price { get; set; }                 //金額
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string InventoryStatus { get; set; }         //全体の棚卸ステータス
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationType { get; set; }            //ロケーション種別
        public bool NewRecFlag { get; set; }                //新規レコードフラグ
    }


    //Add 2014/12/04 arc nakayama 新規部品在庫検索対応　部品在庫クラス(検索項目の格納先)の追加
    public class CommonPartsStockSearch
    {
        public DateTime TargetDate { get; set; }              //対象日付
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }          //部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }          //部門名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationCode { get; set; }            //ロケーションコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationName { get; set; }            //ロケーション名称
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }             //部品番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNameJp { get; set; }             //部品名(日本語)
        //Add 2016/08/13 arc yano #3596 【大項目】部門棚統合対応
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string StockZeroVisibility { get; set; }     //在庫０表示判断

    }
    //Add 2015/09/16 arc yano 部品仕掛在庫検索　不具合対応(ビューの削除)

    //Add 2015/01/09 arc yano 部品在庫棚卸対応　部品仕掛在庫検索画面の検索条件の入庫日を、範囲指定に変更
    public partial class InventoryParts_Shikakari
    {
        public DateTime? ArrivalPlanDateFrom { get; set; }              //入庫日(From)
        public DateTime? ArrivalPlanDateTo { get; set; }                //入庫日(To)
    }
    //Add 2015/01/26 arc ishii #3153 入金実績リスト追加対応
    public partial class V_RevenueResult : CrmsAuth, ICrmsModel
    {
        public DateTime? SalesOrderDateFrom { get; set; }              //日付(From)
        public DateTime? SalesOrderDateTo { get; set; }                //日付(To)

        public DateTime? JournalDateFrom { get; set; }                  //日付(From)
        public DateTime? JournalDateTo { get; set; }                    //日付(From)
    }

    //Add 2015/08/10 arc yano #3233 売掛金帳票対応
    public partial class AccountsReceivable
    {
        public string SalesDateFrom { get; set; }               //納車日(From)
        public string SalesDateTo { get; set; }                 //納車日(To)
        public string Zerovisible { get; set; }                 //残高ゼロ表示
        public string Classification { get; set; }              //区分
        public string ClassificationName { get; set; }          //区分名(社外/社内)    //Add 2020/05/22 yano #4032
        public int SummaryPattern { get; set; }                 //集計方式             //Add 2020/05/22 yano #4032
    }


    //Add 2015/01/26 arc yano #3153 入金実績リスト追加対応
    //入金予定・実績(1レコード)
    public class RecieptPlanReSult
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ST { get; set; }                         //請求・入金フラグ(0=請求、1=入金)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string STName { get; set; }                  //請求金額or入金金額
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }              //伝票番号
        public DateTime? ReceiptDate { get; set; }          //入金日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AccountType { get; set; }             //口座種別
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AccountCode { get; set; }             //科目コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AccountName { get; set; }             //科目名
        public decimal? Amount { get; set; }                //金額
    }
    //入金予定・実績一覧
    public class RecieptPlanReSultList
    {
        public List<RecieptPlanReSult> list { get; set; }                  //入金予定・実績一覧
        public PaginatedList<RecieptPlanReSult> pagelist { get; set; }     //入金予定・実績一覧(ページング対応)
        public Decimal? AmountCash { get; set; }                           //合計(現金)
        public Decimal? AmountTransfer { get; set; }                       //合計(振込)
        public Decimal? AmountCard { get; set; }                           //合計(カード)
        public Decimal? AmountLoan { get; set; }                           //合計(ローン)
        public Decimal? AmountOther { get; set; }                          //合計(その他)
        public Decimal? AmountTrade { get; set; }                          //合計(下取仕入)
        public Decimal? AmountRemainDebt { get; set; }                     //合計(残債)

        //コンストラクタ
        public RecieptPlanReSultList()
        {
            list = new List<RecieptPlanReSult>();  //リストの初期化
            AmountCash = 0;
            AmountTransfer = 0;
            AmountCard = 0;
            AmountLoan = 0;
            AmountOther = 0;
            AmountTrade = 0;
            AmountRemainDebt = 0;
        }
    }

    // Add 2015/02/13 arc nakayama 売掛金管理対応
    public partial class V_ReceivableManagement : ICrmsModel
    {
        public DateTime? SalesDateFrom { get; set; }
        public DateTime? SalesDateTo { get; set; }
    }

    //Mod 2015/02/23 arc yano 現金出納帳出力対応 クラス名の変更
    //Add 2015/02/16 arc    iijima 現金出納帳出力対応
    public partial class T_CashJournalOutput : ICrmsModel
    {
        public string StrLastMonthBalance { get; set; }
        public string StrThisMonthJournal { get; set; }
        public string StrThisMonthPayment { get; set; }
        public string StrThisMonthBalance { get; set; }
    }

    //Add 2016/08/13 arc yano #3596 【大項目】部門棚統合対応
    public partial class Warehouse : ICrmsModel
    {
        public string DepartmentName { get; set; }        //部門名(検索条件)
        public string DelFlagName { get; set; }
    }

    //Add 2016/08/13 arc yano #3596 【大項目】部門棚統合対応
    public partial class DepartmentWarehouse : ICrmsModel
    {
        public string DepartmentName { get; set; }        //部門名(検索条件)
        public string WarehouseName { get; set; }        //倉庫名(検索条件)
        public string DelFlagName { get; set; }
    }

    //Add 2016/08/13 arc yano #3596 【大項目】部門棚統合対応
    public partial class GetLocationListResult : ICrmsModel
    {
        public string DelFlagName { get; set; }
    }

    //Mod 2017/03/07 arc yano #3731 セルの位置(行,列)形式を追加
    //Add 2015/03/17 arc yano 現金出納帳出力(エクセル)対応
    public class ConfigLine
    {
        public int DIndex { get; set; }              //セットするデータのインデックス
        public string SheetName { get; set; }        //シート名
        public int Type { get; set; }             //タイプ(List/Slip)
        public List<string> SetPos { get; set; }                        //データセット位置(A1形式)

        public List<Tuple<int, int>> SetPosRowCol { get; set; }          //データセット位置((1,1)形式)

        public ConfigLine()
        {
            //リストの初期化
            DIndex = 0;
            SheetName = "";
            SetPos = new List<string>();
            SetPosRowCol = new List<Tuple<int, int>>();
        }
    }

    //Mod 2018/04/02 arc yano 現金出納帳出力　六口座対応
    //Add 2015/03/17 arc yano 現金出納帳出力(エクセル)対応　commonシートのクラスを追加
    public class CommonSheet
    {
        public string OfficeCode { get; set; }               //事務所コード
        public string CashAccountName1 { get; set; }        //現金口座種別名１
        public string CashAccountName2 { get; set; }        //現金口座種別名２
        public string CashAccountName3 { get; set; }        //現金口座種別名３
        public string CashAccountName4 { get; set; }        //現金口座種別名４
        public string CashAccountName5 { get; set; }        //現金口座種別名５
        public string CashAccountName6 { get; set; }        //現金口座種別名６

        public int TargetDateY { get; set; }                //対象年月(年)
        public int TargetDateM { get; set; }                //対象年月(月)

        public decimal? PreAccount1 { get; set; }           //前月繰越(現金口座種別１)
        public decimal? PreAccount2 { get; set; }           //前月繰越(現金口座種別２)
        public decimal? PreAccount3 { get; set; }           //前月繰越(現金口座種別３)
        public decimal? PreAccount4 { get; set; }           //前月繰越(現金口座種別４)
        public decimal? PreAccount5 { get; set; }           //前月繰越(現金口座種別５)
        public decimal? PreAccount6 { get; set; }           //前月繰越(現金口座種別６)

    }


    //Add 2015/03/17 arc yano 現金出納帳出力(エクセル)対応　commonシートのクラスを追加
    public class CashJournalSheet
    {
        public int TargetDateY { get; set; }
        public int TargetDateM { get; set; }
        public int TargetDateD { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string DepartmentCode { get; set; }
        public string SlipNumber { get; set; }
        public string CustomerClaimName { get; set; }
        public string Blank1 { get; set; }
        public string Blank2 { get; set; }
        public string Blank3 { get; set; }
        public string Summary { get; set; }
        public string Blank4 { get; set; }
        public string Blank5 { get; set; }
        public decimal InAmount { get; set; }
        public decimal OutAmount { get; set; }
    }

    //Mod 2017/07/27 arc yano #3781 部品在庫棚卸（棚卸在庫の修正）新規レコードフラグ、引当済数を追加
    //Mod 2015/07/15 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑦ 理論在庫の単価、金額を追加
    //Add 2015/04/24 arc yano IPO対応(部品棚卸) 部品棚卸画面追加
    public class PartsInventory : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationCode { get; set; }                //ロケーションコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationName { get; set; }                //ロケーション名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }                 //部品番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNameJp { get; set; }                 //部品名
        public decimal? Quantity { get; set; }                  //理論数
        public decimal? PostCost { get; set; }                  //理論在庫単価        //Add 2015/07/15
        public decimal? CalcAmount { get; set; }                //理論在庫金額        //Add 2015/07/15

        public decimal? PhysicalQuantity { get; set; }          //実棚数
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Comment { get; set; }                     //コメント
        public decimal? ProvisionQuantity { get; set; }         //引当済数           //Add 2017/07/27 arc yano #3781
        public bool NewRecFlag { get; set; }                    //新規登録フラグ     //Add 2017/07/27 arc yano #3781
        public Guid InventoryId { get; set; }                   //棚卸ID             //Add 2017/07/27 arc yano #3781
    }

    //Add 2017/11/08 arc yano #3781 部品在庫棚卸（棚卸在庫の修正）Excel出力用のクラスを追加
    public class PartsInventoryForExcel : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationCode { get; set; }                //ロケーションコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationName { get; set; }                //ロケーション名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }                 //部品番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNameJp { get; set; }                 //部品名
        public decimal? Quantity { get; set; }                  //理論数
        public decimal? PostCost { get; set; }                  //理論在庫単価
        public decimal? CalcAmount { get; set; }                //理論在庫金額

        public decimal? PhysicalQuantity { get; set; }          //実棚数
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Comment { get; set; }                     //コメント
    }


    //Mod 2015/08/11 arc yano #3233 売掛金帳票対応 エクセル出力用のクラス構成の変更
    //Add 2015/06/18 arc nakayama  売掛金対応① Excel対応
    public class ReceivableManagementExcelResult
    {
        //public string SlipTypeName { get; set; }        //伝票タイプ名
        public string SlipNumber { get; set; }          //伝票番号
        public string SalesDate { get; set; }                 //納車日
        public string DepartmentCode { get; set; }      //部門コード
        public string DepartmentName { get; set; }      //部門名
        public string CustomerCode { get; set; }        //顧客コード
        public string CustomerName { get; set; }        //顧客名
        public string CustomerClaimType { get; set; }           //請求先区分コード
        public string CustomerClaimTypeName { get; set; }       //請求先区分コード
        public string CustomerClaimCode { get; set; }   //請求先コード
        public string CustomerClaimName { get; set; }   //請求先名
        public string CarriedBalance { get; set; }            //前月繰越
        public string PresentMonth { get; set; }              //当月発生(諸費用以外)
        public string Expendes { get; set; }                  //当月発生(諸費用)
        public string TotalAmount { get; set; }               //合計
        public string Payment { get; set; }                   //当月入金(諸費用以外)
        public string ChargesPayment { get; set; }            //当月入金(諸費用)
        public string BalanceAmount { get; set; }               //残高
    }

    //Add 2020/05/22 yano #4032
    public class ReceivableManagementExcelResultByCustomerClaim
    {
        public string CustomerClaimCode { get; set; }         //請求先コード
        public string CustomerClaimName { get; set; }         //請求先名
        public string CustomerClaimType { get; set; }         //請求先区分コード
        public string CustomerClaimTypeName { get; set; }     //請求先区分コード

        public string SlipNumber { get; set; }                //伝票番号
        public DateTime? SalesDate { get; set; }              //納車日
        public string DepartmentCode { get; set; }            //部門コード
        public string DepartmentName { get; set; }            //部門名
      
        public decimal? CarriedBalance { get; set; }          //前月繰越
        public decimal? PresentMonth { get; set; }            //当月発生(諸費用以外)
        public decimal? Expendes { get; set; }                //当月発生(諸費用)
        public decimal? TotalAmount { get; set; }             //合計
        public decimal? Payment { get; set; }                 //当月入金(諸費用以外)
        public decimal? ChargesPayment { get; set; }          //当月入金(諸費用)
        public decimal? BalanceAmount { get; set; }           //残高
    }

    //Add 2020/05/22 yano #4032
    public class ReceivableManagementExcelResultByDepartment
    {
        public string DepartmentCode { get; set; }            //部門コード
        public string DepartmentName { get; set; }            //部門名
        public string CustomerClaimCode { get; set; }         //請求先コード
        public string CustomerClaimName { get; set; }         //請求先名
        public string CustomerClaimType { get; set; }         //請求先区分コード
        public string CustomerClaimTypeName { get; set; }     //請求先区分コード
        public string SlipNumber { get; set; }                //伝票番号
        public DateTime? SalesDate { get; set; }              //
        public decimal? CarriedBalance { get; set; }          //前月繰越
        public decimal? PresentMonth { get; set; }            //当月発生(諸費用以外)
        public decimal? Expendes { get; set; }                //当月発生(諸費用)
        public decimal? TotalAmount { get; set; }             //合計
        public decimal? Payment { get; set; }                 //当月入金(諸費用以外)
        public decimal? ChargesPayment { get; set; }          //当月入金(諸費用)
        public decimal? BalanceAmount { get; set; }           //残高
    }


    // Add 2014/12/10 arc nakayama 顧客DM対応 顧客情報クラス(検索項目の格納先)の追加
    public class CustomerDataExcelResult
    {
        /// <summary>
        /// DMフラグ
        /// </summary>
        public string DmFlagName { get; set; }

        /// <summary>
        /// 備考(DM可否)
        /// </summary>
        public string DmMemo { get; set; }

        //DEL 2015/09/11 arc nakayama 顧客検索バグ修正　車検案内DM発送可否と車検案内備考欄を出力項目から削除

        /// <summary>
        /// 顧客コード(顧客情報)
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 顧客種別
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerKindName { get; set; }

        /// <summary>
        /// 顧客名
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }

        /// <summary>
        /// 営業担当部門名
        /// </summary>
        public string DepartmentName2 { get; set; }

        /// <summary>
        /// 営業担当者名
        /// </summary>
        public string CarEmployeeName { get; set; }

        /// <summary>
        /// サービス担当部門名
        /// </summary>
        public string ServiceDepartmentName2 { get; set; }

        /// <summary>
        /// サービス担当者名
        /// </summary>
        public string ServiceEmployeeName2 { get; set; }

        /// <summary>
        /// 郵便番号(顧客住所)
        /// </summary>
        public string CustomerPostCode { get; set; }

        /// <summary>
        /// 都道府県(顧客住所)
        /// </summary>
        public string CustomerPrefecture { get; set; }

        /// <summary>
        /// 住所(顧客住所)
        /// </summary>
        public string CustomerAddress { get; set; }

        /// <summary>
        /// 電話番号(顧客住所)
        /// </summary>
        public string CustomerTelNumber { get; set; }

        //Add 2015/07/17 arc nakayama #3224_「顧客データリスト」機能で携帯電話番号が出力されない
        /// <summary>
        /// 携帯電話番号(顧客住所)
        /// </summary>
        public string MobileNumber { get; set; }

        /// <summary>
        /// メールアドレス(顧客住所)　//Add 2015/12/16 arc ookubo #3318_メールアドレスを追加
        /// </summary>
        public string MailAddress { get; set; }

        /// <summary>
        /// 住所再確認(顧客住所)
        /// </summary>
        public string CustomerAddressReconfirmName { get; set; }

        /// <summary>
        /// 顧客名(DM発送先住所)
        /// </summary>
        public string DmCustomerName { get; set; }

        /// <summary>
        /// 郵便番号(DM発送先住所)
        /// </summary>
        public string DmPostCode { get; set; }

        /// <summary>
        /// 都道府県(DM発送先住所)
        /// </summary>
        public string DmPrefecture { get; set; }

        /// <summary>
        /// 住所(DM発送先住所)
        /// </summary>
        public string DmCustomerDmAddress { get; set; }

        /// <summary>
        /// 電話番号(DM発送先住所)
        /// </summary>
        public string DmTelNumber { get; set; }

        /// <summary>
        /// 管理番号
        /// </summary>
        public string SalesCarNumber { get; set; }

        /// <summary>
        /// 車両登録番号
        /// </summary>
        public string MorterViecleOfficialCode { get; set; }

        /// <summary>
        /// 車台番号
        /// </summary>
        public string Vin { get; set; }

        /// <summary>
        /// メーカー(車両情報)
        /// </summary>
        public string MakerName { get; set; }

        /// <summary>
        /// 車種コード
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }

        /// <summary>
        /// 初年度登録日
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DtFirstRegistrationDate { get; set; }

        /// <summary>
        /// 登録年月日
        /// </summary>
        public string RegistrationDate { get; set; }

        /// <summary>
        /// 次回点検日
        /// </summary>
        public string NextInspectionDate { get; set; }

        /// <summary>
        /// 車検満了日(車両情報)
        /// </summary>
        public string ExpireDate { get; set; }

        /// <summary>
        /// 車両重量(kg)
        /// </summary>
        public int? CarWeight { get; set; }

        /// <summary>
        /// 部門名(車両伝票)
        /// </summary>
        public string SalesDepartmentName { get; set; }

        /// <summary>
        /// 担当者名(車両伝票)
        /// </summary>
        public string SalesEmployeeName { get; set; }

        /// <summary>
        /// 納車年月日(車両伝票)
        /// </summary>
        public string SalesDate { get; set; }

        /// <summary>
        /// 部門名(サービス伝票)
        /// </summary>
        public string ServiceDepartmentName { get; set; }

        /// <summary>
        /// 担当者名(サービス伝票)
        /// </summary>
        public string ServiceEmployeeName { get; set; }

        /// <summary>
        /// 入庫日(サービス伝票)
        /// </summary>
        public string ArrivalPlanDate { get; set; }

    }


    // Add 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合  車検案内の結果項目クラスの追加
    public class InspectGuidDataExcelResult
    {
        //Add 2016/03/08 arc nakayama #3452_車検点検リスト項目追加(#3451_【大項目】車検活動リスト作成)
        /// <summary>
        /// DMフラグ
        /// </summary>
        public string DmFlagName { get; set; }

        /// <summary>
        /// 備考(DM可否)
        /// </summary>
        public string DmMemo { get; set; }

        /// <summary>
        /// 車検案内(DM可否)
        /// </summary>
        public string InspectGuidFlagName { get; set; }

        /// <summary>
        /// 備考(車検案内)
        /// </summary>
        public string InspectGuidMemo { get; set; }

        /// <summary>
        /// 顧客コード(顧客情報)
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// 顧客種別
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerKindName { get; set; }

        /// <summary>
        /// 顧客名
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }

        /// <summary>
        /// 営業担当部門名
        /// </summary>
        public string DepartmentName2 { get; set; }

        /// <summary>
        /// 営業担当者名
        /// </summary>
        public string CarEmployeeName { get; set; }

        /// <summary>
        /// サービス担当部門名
        /// </summary>
        public string ServiceDepartmentName2 { get; set; }

        /// <summary>
        /// サービス担当者名
        /// </summary>
        public string ServiceEmployeeName2 { get; set; }

        /// <summary>
        /// 郵便番号(顧客住所)
        /// </summary>
        public string CustomerPostCode { get; set; }

        /// <summary>
        /// 都道府県(顧客住所)
        /// </summary>
        public string CustomerPrefecture { get; set; }

        /// <summary>
        /// 住所(顧客住所)
        /// </summary>
        public string CustomerAddress { get; set; }

        /// <summary>
        /// 電話番号(顧客住所)
        /// </summary>
        public string CustomerTelNumber { get; set; }

        /// <summary>
        /// 携帯電話番号(顧客住所)
        /// </summary>
        public string MobileNumber { get; set; }

        /// <summary>
        /// メールアドレス(顧客住所)　//Add 2015/12/16 arc ookubo #3318_メールアドレスを追加
        /// </summary>
        public string MailAddress { get; set; }

        /// <summary>
        /// 住所再確認(顧客住所)
        /// </summary>
        public string CustomerAddressReconfirmName { get; set; }

        /// <summary>
        /// 顧客名(DM発送先住所)
        /// </summary>
        public string DmCustomerName { get; set; }

        /// <summary>
        /// 郵便番号(DM発送先住所)
        /// </summary>
        public string DmPostCode { get; set; }

        /// <summary>
        /// 都道府県(DM発送先住所)
        /// </summary>
        public string DmPrefecture { get; set; }

        /// <summary>
        /// 住所(DM発送先住所)
        /// </summary>
        public string DmCustomerDmAddress { get; set; }

        /// <summary>
        /// 電話番号(DM発送先住所)
        /// </summary>
        public string DmTelNumber { get; set; }

        /// <summary>
        /// 管理番号
        /// </summary>
        public string SalesCarNumber { get; set; }

        /// <summary>
        /// 車両登録番号
        /// </summary>
        public string MorterViecleOfficialCode { get; set; }

        /// <summary>
        /// 車台番号
        /// </summary>
        public string Vin { get; set; }

        /// <summary>
        /// メーカー(車両情報)
        /// </summary>
        public string MakerName { get; set; }

        /// <summary>
        /// 車種コード
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }

        //Add 2016/03/08 arc nakayama #3452_車検点検リスト項目追加(#3451_【大項目】車検活動リスト作成)
        /// <summary>
        /// グレード名
        /// </summary>
        public string CarGradeName { get; set; }

        /// <summary>
        /// 車両在庫ロケーション
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// 初年度登録日
        /// </summary>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DtFirstRegistrationDate { get; set; }

        /// <summary>
        /// 登録年月日
        /// </summary>
        public string RegistrationDate { get; set; }

        /// <summary>
        /// 次回点検日
        /// </summary>
        public string NextInspectionDate { get; set; }

        /// <summary>
        /// 車検満了日(車両情報)
        /// </summary>
        public string ExpireDate { get; set; }

        /// <summary>
        /// 車両重量(kg)
        /// </summary>
        public int? CarWeight { get; set; }

        //Add 2016/03/08 arc nakayama #3452_車検点検リスト項目追加(#3451_【大項目】車検活動リスト作成)
        /// <summary>
        /// 車両伝票ステータス
        /// </summary>
        public string SalesOrderStatusName { get; set; }

        /// <summary>
        /// 納車年月日(車両伝票)
        /// </summary>
        public string SalesDate { get; set; }

        /// <summary>
        /// 最終入庫ロケーション
        /// </summary>
        public string ServiceDepartmentName3 { get; set; }

        /// <summary>
        /// 最終入庫日
        /// </summary>
        public string ArrivalPlanDate { get; set; }

    }


    // Add 2015/06/25 arc nakayama 経理からの要望③　仕掛在庫表
    public class PartsWipStockAmount
    {
        public decimal? SumTotalAmount { get; set; }       //部品の合計(全部門)
        public decimal? SumTotalCost { get; set; }         //外注費の合計(全部門)
        public decimal? SumGrandTotalAmount { get; set; }  //部品の合計(全部門) + 外注費の合計(全部門)
        public List<GetShikakariSummaryResult> list { get; set; }             //仕掛在庫表
        public PaginatedList<GetShikakariSummaryResult> plist { get; set; }   //仕掛在庫表(ページング対応)

        //初期処理
        public PartsWipStockAmount()
        {
            list = new List<GetShikakariSummaryResult>();
            plist = new PaginatedList<GetShikakariSummaryResult>();
        }
    }

    //Add 2015/09/11 arc nakayama #3165_サービス集計表対応
    public class ServiceSalesSearchCondition
    {
        public string SalesDateFrom { get; set; }  //納車日From
        public string SalesDateTo { get; set; }    //納車日To
        public string DepartmentCode { get; set; } //部門コード
        public string WorkTypeCode { get; set; }   //区分(社内/社外)
    }

    //Add 2015/09/11 arc nakayama #3165_サービス集計表対応
    public class ServiceSalesAmount
    {
        public decimal? SumTotalServiceAmount { get; set; } //工賃売上の合計
        public decimal? SumTotalPartsAmount { get; set; }   //部品売上の合計
        public decimal? SumTotalTaxAmount { get; set; }     //消費税の合計
        public decimal? SumGrandTotalAmount { get; set; }   //売上の総合計
        public decimal? SumTotalServiceCost { get; set; }   //工賃原価の合計
        public decimal? SumTotalPartsCost { get; set; }     //部品原価の合計
        public decimal? SumGrandTotalCost { get; set; }     //原価の総合計
        public decimal? SumTotalServiceProfits { get; set; }//工賃粗利の合計
        public decimal? SumTotalPartsProfits { get; set; }  //部品粗利の合計
        public decimal? SumGrandTotalProfits { get; set; }  //粗利の総合計
        public List<GetServiceSalesReportResult> list { get; set; }             //サービス集計表
        public PaginatedList<GetServiceSalesReportResult> plist { get; set; }   //サービス集計表(ページング対応)

        //初期処理
        public ServiceSalesAmount()
        {
            list = new List<GetServiceSalesReportResult>();
            plist = new PaginatedList<GetServiceSalesReportResult>();
        }
    }

    //Add 2015/09/11 arc nakayama #3165_サービス集計表対応 Excel出力クラス
    public class ServiceSalesReportExcelResult
    {
        public string SalesDate { get; set; }           //納車日
        public string QuoteDate { get; set; }           //見積日
        public string ArrivalPlanDate { get; set; }     //入庫日
        public string WorkingEndDate { get; set; }      //作業終了日
        public string SlipNumber { get; set; }          //伝票番号
        public string ServiceWorkCode { get; set; }     //主作業コード
        public string WorkTypeName { get; set; }        //区分
        public string ServiceWorkName { get; set; }     //主作業名
        public string DepartmentCode { get; set; }      //部門コード
        public string DepartmentName { get; set; }      //部門名
        public string ServiceAmount { get; set; }       //工賃売上
        public string PartsAmount { get; set; }         //部品売上
        public string TaxAmount { get; set; }           //消費税
        public string TotalAmount { get; set; }         //売上合計
        public string ServiceCost { get; set; }         //工賃原価
        public string PartsCost { get; set; }           //部品原価
        public string TotalCost { get; set; }           //原価合計
        public string ServiceProfits { get; set; }      //工賃粗利
        public string PartsProfits { get; set; }        //部品粗利
        public string TotalProfits { get; set; }        //粗利合計
        public string FrontEmployeeName { get; set; }   //フロント担当者
        public string MechanicEmployeeName { get; set; }//メカニック担当者
        public string CustomerClaimName { get; set; }   //請求先名
    }



    //Add 2015/09/11 arc nakayama #3165_サービス集計表対応
    public class PartsStockSearchCondition
    {
        public string MakerCode { get; set; } //メーカーコード
        public string MakerName { get; set; } //メーカー名
        public string CarBrandCode { get; set; } //ブランドコード
        public string CarBrandName { get; set; } //ブランド名
        public string PartsNumber { get; set; } //パーツナンバー
        public string PartsNameJp { get; set; } //パーツ名
        public string DepartmentCode { get; set; } //部門コード
        public string SupplierCode { get; set; } //仕入先コード
    }

    //Add 2015/11/09 #3291 arc yano 部品仕入機能改善(部品発注入力) ↓↓
    public class ParamOrder
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseOrderNumber { get; set; }     //発注伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceSlipNumber { get; set; }       //受注伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }          //部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OrderType { get; set; }               //オーダー区分
        public List<InfoParts> partsList { get; set; }      //部品リスト
        /*
        ParamOrder()
        {
            partsList = new List<InfoParts>();
        }
        */
    }

    public class InfoParts
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }         //部品
        public decimal? Quantity { get; set; }          //数量
    }

    public class InfoPartsExcel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerPartsNumber { get; set; }
        public decimal? Quantity { get; set; }               //数量
    }

    public class PartsPurchaseOrderHeader
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseOrderNumber { get; set; }     //発注伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceSlipNumber { get; set; }       //受注伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierName { get; set; }            //仕入先名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierPaymentName { get; set; }     //支払先名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EmployeeName { get; set; }            //社員名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }          //部門名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OrderTypeName { get; set; }           //オーダー区分名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }          //車台番号
    }
    //Add 2015/11/10 arc nakayama #3292_部品入荷一覧  検索条件クラス追加
    public class PartsPurchaseSearchCondition
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseNumberFrom { get; set; }      // 入荷伝票番号From
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseNumberTo { get; set; }        // 入荷伝票番号To
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseOrderNumberFrom { get; set; } // 発注伝票番号From
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseOrderNumberTo { get; set; }   // 発注伝票番号To
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseOrderDateFrom { get; set; }   // 発注日From
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseOrderDateTo { get; set; }     // 発注日To
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumberFrom { get; set; }          // 伝票番号From
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumberTo { get; set; }            // 伝票番号To
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseType { get; set; }            // 仕入伝票区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OrderType { get; set; }               // 発注区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerCode { get; set; }            // 顧客コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }             // 部品番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchasePlanDateFrom { get; set; }    // 入荷予定日From
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchasePlanDateTo { get; set; }      // 入荷予定日To
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseDateFrom { get; set; }        // 入荷日From
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseDateTo { get; set; }          // 入荷日To
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }          // 部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EmployeeCode { get; set; }            // 社員コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierCode { get; set; }            // 仕入先コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string WebOrderNumber { get; set; }          // WEBオーダー番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerOrderNumber { get; set; }        // メーカーオーダー番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string InvoiceNo { get; set; }               // インボイス番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseStatus { get; set; }          // 仕入ステータス
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LinkEntryCaptureDateFrom { get; set; } // 取込日From    //Add 2018/03/26 arc yano #3863
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LinkEntryCaptureDateTo { get; set; }   // 取込日To      //Add 2018/03/26 arc yano #3863

    }

    // Add 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
    public class PartsPurchase_PurchaseList
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }        // 部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }        // 部門名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseType { get; set; }          // 仕入伝票区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseDate { get; set; }          // 入荷日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EmployeeCode { get; set; }          // 社員コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EmployeeName { get; set; }          // 社員名

        public List<GetPartsPurchaseList_Result> line { get; set; }

        public PartsPurchase_PurchaseList()
        {
            line = new List<GetPartsPurchaseList_Result>();
        }

    }
    // Add 2015/12/09 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
    public class PurchaseEntryKeyList
    {
        public string PurchaseNumber { get; set; }          // 入荷伝票番号
        public string PurchaseOrderNumber { get; set; }     // 発注伝票番号
        public string PartsNumber { get; set; }          　 // 部品番号
    }

    // Add 2016/02/11 arc nakayama #3424_部品入荷検索画面　入荷確定後の再検索(#3397_部品仕入機能改善 課題管理表対応)
    public class PurchaseOrderEntryKeyList
    {
        public string PurchaseOrderNumber { get; set; }     // 発注伝票番号
        public string PartsNumber { get; set; }          　 // 部品番号
        public decimal? RemainingQuantity { get; set; }      // 発注残
    }
    public partial class GetPartsPurchaseList_Result : ICrmsModel
    {
        public bool ChangeParts { get; set; }               //代替部品フラグ
        public string ChangePartsNumber { get; set; }       //代替部品番号
        public string ChangePartsNameJp { get; set; }       //代替部品名
    }

    // Mod  2018/01/15 arc yano #3832 メーカーオーダー番号追加
    // Add 2015/12/09 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
    public class PurchaseExcelImportList
    {
        public int LineNumber { get; set; }             // ラインナンバー
        public string InvoiceNo { get; set; }           // インボイス番号
        public string PurchasePlanDate { get; set; }    // 入荷予定日
        //public string WebOrderNumber { get; set; }      // WEBオーダー番号
        public string MakerOrderNumber { get; set; }    // メーカーオーダー番号   //Add 2018/01/15 arc yano #3832
        public string PurchaseOrderNumber { get; set; } // 発注伝票番号
        public string MakerPartsNumber { get; set; }    // メーカー部品番号
        public string PartsNumber { get; set; }         // 部品番号
        public string PartsNameJp { get; set; }         // 部品名
        public string Quantity { get; set; }            // 数量
        public string Price { get; set; }               // 単価
        public string Amount { get; set; }              // 金額
        public string SupplierCode { get; set; }        // 仕入先コード
        public string SupplierName { get; set; }        // 仕入先名
        public string DepartmentCode { get; set; }      // 部門コード
        public string SlipNumber { get; set; }          // 受注伝票番号
        public string EmployeeCode { get; set; }        // 社員コード
        public string PurchaseStatus { get; set; }      // 入荷ステータス
        public string PurchaseStatusName { get; set; }  // 入荷ステータス名

    }

    //Add 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加
    public class ExcelCommonPartsStock
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationCode { get; set; }            //ロケーションコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationName { get; set; }            //ロケーション名称
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }             //部品番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNameJp { get; set; }             //部品名(日本語) 
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Quantity { get; set; }                //数量
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ProvisionQuantity { get; set; }       //引当済数
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FreeQuantity { get; set; }           //フリー在庫数
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string StandardPrice { get; set; }          //標準原価
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MoveAverageUnitPrice { get; set; }   //移動平均単価
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Price { get; set; }                  //金額
    }

    //Mod 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
    //Add 2016/07/12 arc yano #3599 部品在庫確認　Excel出力機能追加
    public class ExcelPartsBalance
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string WarehouseCode { get; set; }              //部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string WarehouseName { get; set; }              //部門名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }                //部品番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNameJp { get; set; }                //部品名(日本語) 
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PreCost { get; set; }                   //前月単価
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PreQuantity { get; set; }               //数量(月初在庫)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PreAmount { get; set; }                 //金額(月初在庫)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseQuantity { get; set; }          //数量(当月仕入)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseAmount { get; set; }            //金額(当月仕入)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TransferArrivalQuantity { get; set; }   //数量(当月移動受入)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TransferArrivalAmount { get; set; }     //金額(当月移動受入)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ShipQuantity { get; set; }              //数量(当月納車)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ShipAmount { get; set; }                //金額(当月納車)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TransferDepartureQuantity { get; set; } //数量(当月移動払出)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TransferDepartureAmount { get; set; }   //金額(当月移動払出)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string UnitPriceDifference { get; set; }       //金額(単価差額)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CalculatedQuantity { get; set; }        //数量(理論在庫)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CalculatedAmount { get; set; }          //金額(理論在庫)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DifferencetCost { get; set; }           //単価(棚差)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DifferenceQuantity { get; set; }        //数量(棚差)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DifferenceAmount { get; set; }          //金額(棚差)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PostCost { get; set; }                  //単価(月末在庫)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PostQuantity { get; set; }              //数量(月末在庫)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PostAmount { get; set; }                //金額(月末在庫)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ReservationQuantity { get; set; }       //数量(引当在庫)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ReservationAmount { get; set; }         //金額(引当在庫)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string InProcessQuantity { get; set; }         //数量(仕掛在庫)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string InProcessAmount { get; set; }           //金額(仕掛在庫)
    }

    //Add 2016/07/12 arc yano #3599 部品在庫確認　Excel出力機能追加
    public class ExcelPartsBalanceTotal
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalPreCost { get; set; }                          //月初単価
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalPreQuantity { get; set; }                      //月初在庫(数量)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalPreAmount { get; set; }                        //月初在庫(金額)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalPurchaseQuantity { get; set; }                 //当月仕入(数量)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalPurchaseAmount { get; set; }                   //当月仕入(金額)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalTransferArrivalQuantity { get; set; }          //当月移動受入(数量)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalTransferArrivalAmount { get; set; }            //当月移動受入(金額)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalShipQuantity { get; set; }                     //当月納車(数量)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalShipAmount { get; set; }                       //当月納車(金額)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalTransferDepartureQuantity { get; set; }        //当月移動払出  (数量)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalTransferDepartureAmount { get; set; }          //当月移動払出(金額)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalUnitPriceDifference { get; set; }              //単価差額   (金額)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalCalculatedQuantity { get; set; }               //理論在庫(数量)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalCalculatedAmount { get; set; }                 //理論在庫(金額)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalDifferenceCost { get; set; }                     //棚差(単価)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalDifferenceQuantity { get; set; }               //棚差(数量)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalDifferenceAmount { get; set; }                 //棚差(金額)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalPostCost { get; set; }                         //月末在庫(単価)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalPostQuantity { get; set; }                     //月末在庫(数量)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalPostAmount { get; set; }                       //月末在庫(金額)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalReservationQuantity { get; set; }              //引当在庫(数量)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalReservationAmount { get; set; }                //引当在庫(金額)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalInProcessQuantity { get; set; }                //仕掛在庫(数量)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalInProcessAmount { get; set; }                  //仕掛在庫(金額)
    }

    //Add 2016/07/14 arc yano #3600 仕掛在庫検索　Excel出力機能追加
    public class ExcelInventory_Shikakari
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ArrivalPlanDate { get; set; }                       //入庫日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }                            //伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LineNumber { get; set; }                            //明細行番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceOrderStatusName { get; set; }                //伝票ステータス名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceWorksName { get; set; }                      //主作業名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FrontEmployeeName { get; set; }                     //フロント担当者
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MekaEmployeeName { get; set; }                      //メカニック担当者
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }                          //顧客名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }                               //車種名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                                 　//車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceTypeName { get; set; }                     　//明細種別名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string StockTypeName { get; set; }                         //状況
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseOrderDate { get; set; }                     //発注日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseDate { get; set; }                          //入荷日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsArravalPlanDate { get; set; }                  //入荷予定
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }                           //部品番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LineContents1 { get; set; }                         //部品名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Price { get; set; }                               　//単価
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Quantity { get; set; }                              //数量
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Amount { get; set; }                              　//金額
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierName { get; set; }                          //外注先
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LineContents2 { get; set; }                         //作業内容
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Cost { get; set; }                                  //外注原価
    }

    //Mod 2018/08/28 yano #3922 構造の変更
    //Add 2016/11/30 #3659 車両管理　ファイル出力形式の変更(CSV→EXCEL)
    public class ExcelCarStockForNewCar
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string BrandStore { get; set; }                              //取扱ブランド
        public DateTime?  PurchaseDate { get; set; }                        //入庫日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }                          //管理番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerName { get; set; }                               //メーカー名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }                                 //車種名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                                     //車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseLocationName { get; set; }                    //仕入・在庫拠点
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string InventoryLocationName { get; set; }                   //実棚
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierName { get; set; }                            //仕入先名
        public decimal? BeginningInventory { get; set; }                    //月初在庫
        public decimal? MonthPurchase { get; set; }                         //当月仕入
        public DateTime? SalesDate { get; set; }                            //納車日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }                              //伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesDepartmentName { get; set; }                     //販売店舗
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }                            //販売先
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public decimal? SalesPrice { get; set; }                            //車輛本体価格
        public decimal? DiscountAmount { get; set; }                        //値引
        public decimal? ShopOptionAmount { get; set; }                      //付属品
        public decimal? SalesCostTotalAmount { get; set; }                  //諸費用
        public decimal? SalesTotalAmount { get; set; }                      //売上総合計
        public decimal? SalesCostAmount { get; set; }                       //売上原価
        public decimal? SalesProfits { get; set; }         　               //粗利
        public decimal? SelfRegistration { get; set; }                      //自社登録
        public decimal? ReductionTotal { get; set; }                        //他勘定振替
        public decimal? EndInventory { get; set; }                          //月末在庫
    }

    //Add 2018/08/28 yano #3922
    public class ExcelCarStockForOldCar
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string BrandStore { get; set; }                              //取扱ブランド
        public DateTime? PurchaseDate { get; set; }                         //入庫日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }                          //管理番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerName { get; set; }                               //メーカー名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }                                 //車種名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                                     //車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseLocationName { get; set; }                    //仕入・在庫拠点
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string InventoryLocationName { get; set; }                   //実棚
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarPurchaseTypeName { get; set; }                     //仕入区分名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierName { get; set; }                            //仕入先名
        public decimal? BeginningInventory { get; set; }                    //月初在庫
        public decimal? MonthPurchase { get; set; }                         //当月仕入
        public decimal? OtherAccount { get; set; }                          //他勘定受入
        public decimal? RecycleAmount { get; set; }                         //リサイクル料
        public DateTime? SalesDate { get; set; }                            //納車日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }                              //伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesDepartmentName { get; set; }                     //販売店舗
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerTypeName { get; set; }                        //販売先区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }                            //販売先
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public decimal? SalesPrice { get; set; }                            //車輛本体価格
        public decimal? DiscountAmount { get; set; }                        //値引
        public decimal? ShopOptionAmount { get; set; }                      //付属品
        public decimal? SalesCostTotalAmount { get; set; }                  //諸費用
        public decimal? SalesTotalAmount { get; set; }                      //売上総合計
        public decimal? SalesCostAmount { get; set; }                       //売上原価
        public decimal? SalesProfits { get; set; }         　               //粗利
        public decimal? ReductionTotal { get; set; }                        //他勘定振替
        public decimal? CancelPurchase { get; set; }                        //仕入キャンセル
        public decimal? EndInventory { get; set; }                          //月末在庫
    }

    //Add 2016/08/17 arc nakayama #3595 【大項目】車両売掛金機能改善
    public class GetCreditJournalSearchCondition
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string JournalDateFrom { get; set; }          //決済日From
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string JournalDateTo { get; set; }            //決済日To
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesDateFrom { get; set; }            //納車日From
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesDateTo { get; set; }              //納車日To
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipType { get; set; }                 //伝票タイプ
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }               //伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }           //部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerCode { get; set; }             //顧客コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerClaimCode { get; set; }        //請求先コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CompleteFlag { get; set; }            //入金状況

    }
    //Add 2016/08/17 arc nakayama #3595 【大項目】車両売掛金機能改善
    public class GetCreditJournal_ExcelResult
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipTypeName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string StatusName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OccurredDepartmentCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }
        public DateTime? SalesOrderDate { get; set; }
        public DateTime? SalesDate { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }
        public DateTime? JournalDate { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerClaimCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerClaimName { get; set; }
        public decimal? Amount { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CompleteFlagName { get; set; }
        //Add 2017/03/23 arc nakayama #3737_クレジット入金確認　検索結果項目追加
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AccountCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AccountName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Summary { get; set; }


    }

    //Add 2016/09/14 arc nakayama #3630_【製造】車両売掛金対応
    public partial class AccountsReceivableCar : ICrmsModel
    {
        public string SalesDateFrom { get; set; }   //納車日(From)
        public string SalesDateTo { get; set; }     //納車日(To)
        public string Zerovisible { get; set; }     //納車日(To)
    }

    //Add 2016/09/14 arc nakayama #3630_【製造】車両売掛金対応
    public class AccountsReceivableCarExcelResult
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }              //伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesDate { get; set; }               //納車日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }          //部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }          //部門名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerCode { get; set; }            //顧客コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }            //顧客名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarriedBalance { get; set; }          //前月繰越
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PresentMonth { get; set; }            //当月発生
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PaymentAmount { get; set; }           //当月入金
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string BalanceAmount { get; set; }           //残高
    }

    //Add 2016/09/16 arc nakayama #3630_【製造】車両売掛金対応
    public class TradeInVinList
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }   //下取車の車台番号
    }

    // Add 2016/11/30 arc nakayama #3663_【製造】車両仕入　Excel取込対応
    public class CarPurchaseExcelImportList
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }              //管理番号("A"で自動)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                         //車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string UsVin { get; set; }                       //VIN(シリアル)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerName { get; set; }                   //メーカー名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarGradeCode { get; set; }                //車両グレードコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedType { get; set; }                 //新中区分("N"or"U")
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ColorType { get; set; }                   //系統色
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ExteriorColorCode { get; set; }           //外装色コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ExteriorColorName { get; set; }           //外装色名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string InteriorColorCode { get; set; }           //内装色コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string InteriorColorName { get; set; }           //内装色名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ManufacturingYear { get; set; }           //年式
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Steering { get; set; }                    //ハンドル
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesPrice { get; set; }                  //販売価格(税抜）
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ModelName { get; set; }                   //型式
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EngineType { get; set; }                  //原動機型式
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Displacement { get; set; }                //排気量
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ModelSpecificateNumber { get; set; }      //型式指定番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ClassificationTypeNumber { get; set; }    //類別区分番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo1 { get; set; }                       //備考１
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo2 { get; set; }                       //備考２
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo3 { get; set; }                       //備考３
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo4 { get; set; }                       //備考４
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo5 { get; set; }                       //備考５
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo6 { get; set; }                       //備考６
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo7 { get; set; }                       //備考７
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo8 { get; set; }                       //備考８
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo9 { get; set; }                       //備考９
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo10 { get; set; }                      //備考１０
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarTax { get; set; }                      //自動車税種別割
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarWeightTax { get; set; }                //自動車重量税
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarLiabilityInsurance { get; set; }       //自賠責保険料
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AcquisitionTax { get; set; }              //自動車税環境性能割
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RecycleDeposit { get; set; }              //リサイクル預託金
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ApprovedCarNumber { get; set; }           //認定中古車No
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ApprovedCarWarrantyDateFrom { get; set; } //認定中古車保証期間FROM
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ApprovedCarWarrantyDateTo { get; set; }   //認定中古車保証期間TO
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipDate { get; set; }                    //仕入日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseDate { get; set; }                //入庫予定日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierCode { get; set; }                //仕入先コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseLocationCode { get; set; }        //仕入ロケーションコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string VehiclePrice { get; set; }                //車両本体価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string VehicleTax { get; set; }                  //車両本体消費税
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string VehicleAmount { get; set; }               //車両本体税込価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OptionPrice { get; set; }                 //オプション価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OptionTax { get; set; }                   //オプション消費税
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OptionAmount { get; set; }                //オプション税込価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DiscountPrice { get; set; }               //ディスカウント価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DiscountTax { get; set; }                 //ディスカウント消費税
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DiscountAmount { get; set; }              //ディスカウント税込価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FirmPrice { get; set; }                   //ファーム価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FirmTax { get; set; }                     //ファーム消費税
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FirmAmount { get; set; }                  //ファーム税込価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MetallicPrice { get; set; }               //メタリック価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MetallicTax { get; set; }                 //メタリック消費税
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MetallicAmount { get; set; }              //メタリック税込価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EquipmentPrice { get; set; }              //加装価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RepairPrice { get; set; }                 //加修価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OthersPrice { get; set; }                 //その他価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OthersTax { get; set; }                   //その他消費税
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OthersAmount { get; set; }                //その他税込価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarTaxAppropriatePrice { get; set; }      //自税充当
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RecyclePrice { get; set; }                //リサイクル
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AuctionFeePrice { get; set; }             //オークション落札料
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AuctionFeeTax { get; set; }               //オークション落札料消費税
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AuctionFeeAmount { get; set; }            //オークション落札料税込
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Amount { get; set; }                      //仕入価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TaxAmount { get; set; }                   //消費税
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TotalAmount { get; set; }                 //仕入税込価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo { get; set; }                        //備考
    }

    //Mod 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
    //Add 2017/03/09 arc nakayama #3723_納車リスト
    public class CarSalesHeaderListSearchCondition
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SelectYear { get; set; }  //納車日From
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SelectMonth { get; set; }    //納車日To
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; } //部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedType { get; set; }    //新中区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AAType { get; set; }          //AA含む
    }

    
  
    /// <summary>
    ///　納車リストExcel出力データ形式
    /// </summary>
    /// <history>
    ///2023/09/18 yano #4181【車両伝票】オプション区分追加（サーチャージ）
    ///2023/01/16 yano #4138【納車リスト】集計項目の追加（メンテナンスパッケージ、延長保証）
    ///2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
    ///2017/03/09 arc nakayama #3723_納車リスト
    /// </history>
    public class GetCarSalesHeaderList_ExcelResult
    {
        public DateTime? SalesDate { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedTypeName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Employeename { get; set; }
        public decimal? SalesPrice { get; set; }
        public decimal? ShopOptionAmountWithTax { get; set; }
        public decimal? MaintenancePackageAmount { get; set; }              //Add 2023/01/16 yano #4138
        public decimal? ExtendedWarrantyAmount { get; set; }                //Add 2023/01/16 yano #4138
        public decimal? SurchargeAmount { get; set; }                      //Add 2023/09/18 yano #4181
        public decimal? MakerOptionAmount { get; set; }
        public decimal? AAAmount { get; set; }                              //Add 2017/10/14 arc yano #3790
        public decimal? SalesCostTotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? OtherCostTotalAmount { get; set; }
        public decimal? TaxFreeTotalAmount { get; set; }
        public decimal? CarLiabilityInsurance { get; set; }
        public decimal? RecycleDeposit { get; set; }
        public decimal? GrandTotalAmount { get; set; }
        public decimal? TradeInTotalAmountNotTax { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TradeInVin1 { get; set; }                             //Add 2017/10/14 arc yano #3790
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TradeInVin2 { get; set; }                             //Add 2017/10/14 arc yano #3790
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TradeInVin3 { get; set; }                             //Add 2017/10/14 arc yano #3790
        public decimal? TradeInUnexpiredCarTaxTotalAmount { get; set; }
        public decimal? TradeInRemainDebtTotalAmount { get; set; }
        public decimal? TradeInAppropriationTotalAmount { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarBrandName { get; set; }
    }

    public class TotalCarSalesEmployeeList_Result
    {
        public int? TotalJul_Cnt { get; set; }
        public int? TotalAug_Cnt { get; set; }
        public int? TotalSep_Cnt { get; set; }
        public int? TotalOct_Cnt { get; set; }
        public int? TotalNov_Cnt { get; set; }
        public int? TotalDec_Cnt { get; set; }
        public int? TotalJan_Cnt { get; set; }
        public int? TotalFeb_Cnt { get; set; }
        public int? TotalMar_Cnt { get; set; }
        public int? TotalApr_Cnt { get; set; }
        public int? TotalMay_Cnt { get; set; }
        public int? TotalJun_Cnt { get; set; }

        public List<GetCarSalesEmployeeList_Result> list { get; set; }

        //初期処理
        public TotalCarSalesEmployeeList_Result()
        {
            list = new List<GetCarSalesEmployeeList_Result>();
        }

    }

    public class CarPurchaseExcel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarPurchaseTypeName { get; set; }//仕入区分名       //Add 2018/06/06 arc yano #3883 タマ表改善
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }//管理番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }//部門
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedTypeName { get; set; }//新中区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierName { get; set; }//仕入先
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseEmployeeName { get; set; }//仕入担当
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationName { get; set; }//仕入ロケーション
        public DateTime? PurchaseOrderDate { get; set; }//発注日
        public DateTime? SlipDate { get; set; }//仕入日
        public DateTime? PurchaseDate { get; set; }//入庫日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerName { get; set; }//メーカー
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarBrandName { get; set; }//ブランド
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }//車種
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarGradeName { get; set; }//グレード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }//車台番号
        public decimal? VehiclePrice { get; set; }//車両本体価格-税抜
        public decimal? VehicleTax { get; set; }//車両本体価格-消費税
        public decimal? VehicleAmount { get; set; }//車両本体価格-税込
        public decimal? AuctionFeePrice { get; set; }//落札料-税抜
        public decimal? AuctionFeeTax { get; set; }//落札料-消費税
        public decimal? AuctionFeeAmount { get; set; }//落札料-税込
        public decimal? RecycleAmount { get; set; }//リサイクル
        public decimal? CarTaxAppropriatePrice { get; set; }//自税充当-税抜
        public decimal? CarTaxAppropriateTax { get; set; }//自税充当-消費税
        public decimal? CarTaxAppropriateAmount { get; set; }//自税充当-税込
        public decimal? Amount { get; set; }//仕入金額-税抜
        public decimal? TaxAmount { get; set; }//仕入金額-消費税
        public decimal? TotalAmount { get; set; }//仕入金額-税込
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LastUpdateEmployeeName { get; set; }//最終更新者

    }
    //2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳)
    public class AntiqueLedger_ExcelResult
    {
        public DateTime PurchaseDate { get; set; }                  //仕入日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseStatus { get; set; }                  //仕入区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Article { get; set; }                         //品名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerCarName { get; set; }                    //メーカー＋車種
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }                  //管理番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                             //車台番号
        public decimal Quantity { get; set; }                       //数量
        public decimal Amount { get; set; }                         //金額
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OccupationName { get; set; }                  //職業 
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierName { get; set; }                    //仕入先
        public DateTime? Birthday { get; set; }                     //誕生日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string S_PrefectureCity { get; set; }                //仕入先住所(都道府県市区町村)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string S_Address { get; set; }                       //仕入先住所
        public DateTime? SalesDate { get; set; }                     //納車日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesTypeName { get; set; }                   //販売区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }                    //販売先
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string C_PrefectureCity { get; set; }                //販売先住所(都道府県市区町村)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string C_Address { get; set; }                       //販売先住所
        public bool? ConfirmDriverLicense { get; set; }             //古物取引時の確認方法(免許証)       //Add 2018/10/25 yano #3947
        public bool? ConfirmCertificationSeal { get; set; }         //古物取引時の確認方法(印鑑証明)     //Add 2018/10/25 yano #3947
        [DisplayFormat(ConvertEmptyStringToNull = false)]           //古物取引時の確認方法(その他)       //Add 2018/10/25 yano #3947
        public string ConfirmOther { get; set; }                     
    }

    //Add 2017/03/13 arc yano #3725
    public class ServiceSalesHistoryCondition
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DivType { get; set; }                         //部門タイプ(1:CJ 2:FA)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }                  //部門名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }                      //伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                             //車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RegistNumber { get; set; }                    //ナンバープレート
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }                    //顧客名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerNameKana { get; set; }                //顧客名（かな） 
    }

    //Add 2017/03/13 arc yano #3725
    public class ServiceSalesHistoryRet
    {
        public PaginatedList<GetServiceSalesHistoryHeaderResult> headerList { get; set; }
        public PaginatedList<GetServiceSalesHistoryLineResult> lineList { get; set; }

        public ServiceSalesHistoryRet()
        {
            headerList = new PaginatedList<GetServiceSalesHistoryHeaderResult>();
            lineList = new PaginatedList<GetServiceSalesHistoryLineResult>();
        }
    }

    //Add 2017/03/16 arc yano #3726
    public class PartsStatusCondition
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Target { get; set; }                          //検索対象(0:指定無 1:入庫日 2:作業開始日 3:納車日)
        public DateTime? TargetDateFrom { get; set; }               //対象年月From
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }                  //部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceOrderStatus { get; set; }              //伝票ステータス
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }                     //部品番号
    }


    //Add 2017/03/16 arc yano #3726
    public class PartsStatus_ExcelResult
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }                       //伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceOrderStatusName { get; set; }           //伝票ステータス
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }                      //部品番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LineContents { get; set; }                     //部品名
        public decimal? Quantity { get; set; }                       //数量
        public decimal? ProvisionQuantity { get; set; }              //引当済数     //Add 2017/11/06 arc yano #3809
        public DateTime? ArrivalPlanDate { get; set; }               //入庫日
        public DateTime? SalesOrderDate { get; set; }                //受注日
        public DateTime? WorkingStartDate { get; set; }              //作業開始日
        public DateTime? WorkingEndDate { get; set; }                //作業終了日
        public DateTime? SalesDate { get; set; }                　　 //納車日

    }


    //Add 2017/03/19 arc yano #3721 サブシステム移行(車両追跡) 新規作成
    public class CarStatusCheck
    {
        public List<GetCarBasicInfoResult> GetCarBasicInfoResult { get; set; }                     //車両基本情報
        public List<GetCarStatusTransitionResult> GetCarStatusTransitionResult { get; set; }       //遷移
        public List<GetCarSalesSlipResult> GetCarSalesSlipResult { get; set; }                     //車両販売伝票

        //コンストラクタ定義
        public CarStatusCheck()
        {
            GetCarBasicInfoResult = new List<GetCarBasicInfoResult>();

            GetCarStatusTransitionResult = new List<GetCarStatusTransitionResult>();

            GetCarSalesSlipResult = new List<GetCarSalesSlipResult>();
        }

    }

    //Add 2017/03/23 arc yano #3729
    public class GetSupplierPaymentListCondition
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Target { get; set; }                          //検索対象(0:納車日 1:受注日)
        public DateTime? TargetDateFrom { get; set; }               //対象年月From
        public DateTime? TargetDateTo { get; set; }                 //対象年月To
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }                  //部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceWorkCode { get; set; }                 //主作業コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }                      //伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                             //車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerCode { get; set; }                    //顧客コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }                    //顧客名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierCode { get; set; }                    //外注コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierName { get; set; }                    //外注名
    }

    //Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
    public class CarInventory : ICrmsModel
    {
        public Guid? InventoryId { get; set; }                    //棚卸ID
        public DateTime InventoryMonth { get; set; }            //棚卸月
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }              //部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string WarehouseCode { get; set; }               //倉庫コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationCode { get; set; }                //ロケーションコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationName { get; set; }                //ロケーション名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }              //管理番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                         //車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedType { get; set; }                 //新中区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedTypeName { get; set; }             //新中区分名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarStatus { get; set; }                   //在庫区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarStatusName { get; set; }               //在庫区分名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarBrandName { get; set; }                //ブランド名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }                     //車種名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ColorType { get; set; }                   //系統色
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ExteriorColorCode { get; set; }           //車両カラーコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ExteriorColorName { get; set; }           //車両カラー名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RegistrationNumber { get; set; }          //登録番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MorterViecleOfficialCode { get; set; }　  //陸運局コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RegistrationNumberType { get; set; }      //登録番号(種別)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RegistrationNumberKana { get; set; }      //登録番号(かな)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RegistrationNumberPlate { get; set; }     //登録番号(プレート)
        public decimal? PhysicalQuantity { get; set; }          //実棚数
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Comment { get; set; }                     //コメント
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CommentName { get; set; }                 //コメント名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Summary { get; set; }                     //備考
        public bool NewRecFlag { get; set; }                    //新規登録フラグ
        public int LineNumber { get; set; }                     //行インデックス
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarGradeName { get; set; }                //グレード名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string StockFlag { get; set; }                   //在庫有無
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string strInventoryMonth { get; set; }            //棚卸月
        public bool InventoryDiff { get; set; }                  //棚差差異      //Add 2017/09/07 arc yano #3784
        public decimal? Quantity { get; set; }                   //理論数        //Add 2017/09/07 arc yano #3784

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string hdNewUsedType { get; set; }                 //新中区分(隠し項目)    //Add 2021/02/22 yano #4081

    }


    //Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
    public class CarInventoryForExcel : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }              //管理番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                         //車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedTypeName { get; set; }             //新中区分名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarStatusName { get; set; }               //在庫区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarBrandName { get; set; }                //ブランド名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }                     //車種名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ColorType { get; set; }                   //系統色
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ExteriorColorCode { get; set; }           //車両カラーコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ExteriorColorName { get; set; }           //車両カラー名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MorterViecleOfficialCode { get; set; }　  //陸運局コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RegistrationNumberType { get; set; }      //登録番号(種別)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RegistrationNumberKana { get; set; }      //登録番号(かな)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RegistrationNumberPlate { get; set; }     //登録番号(プレート)
        public decimal? PhysicalQuantity { get; set; }          //実棚数
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CommentName { get; set; }                     //コメント(誤差理由)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Summary { get; set; }                     //備考
    }

    //Add 2017/09/07 arc yano #3784 車両在庫棚卸　新規追加
    public class CarInventoryForExcelList : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationCode { get; set; }                //ロケーションコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationName { get; set; }                //ロケーション名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }              //管理番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                         //車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedTypeName { get; set; }             //新中区分名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarStatusName { get; set; }               //在庫区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarBrandName { get; set; }                //ブランド名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }                     //車種名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ColorType { get; set; }                   //系統色
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ExteriorColorCode { get; set; }           //車両カラーコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ExteriorColorName { get; set; }           //車両カラー名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RegistrationNumber { get; set; }          //登録番号
        public decimal? PhysicalQuantity { get; set; }          //実棚数
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CommentName { get; set; }                 //コメント(誤差理由)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Summary { get; set; }                     //備考
    }


    //Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
    public class LocationInfo : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationCode { get; set; }                        //ロケーションコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationName { get; set; }                        //ロケーション名
    }

    //Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
    public class DescriptionBlock : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }                        //説明文
    }

    //Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
    public class CarInventoryExcelImportList : ICrmsModel
    {
        public int LineNumber { get; set; }                     //行番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationCode { get; set; }                //ロケーションコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LocationName { get; set; }                //ロケーション名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }              //管理番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                         //車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedType { get; set; }                 //新中区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedTypeName { get; set; }             //新中区分名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarStatus { get; set; }                   //在庫区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarStatusName { get; set; }               //在庫区分名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarBrandName { get; set; }                //ブランド名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }                     //車種名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ColorType { get; set; }                   //系統色
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ExteriorColorCode { get; set; }           //車両カラーコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ExteriorColorName { get; set; }           //車両カラー名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RegistrationNumber { get; set; }          //車両登録番号
        public decimal? PhysicalQuantity { get; set; }          //実棚数
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Comment { get; set; }                     //コメント(誤差理由)・コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CommentName { get; set; }                 //コメント(誤差理由)・名称
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Summary { get; set; }                     //サマリー
    }


    //Add 2017/09/04 arc yano #3786 預かり車Excle出力対応
    public class CarStorageList : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string StrCarStorage { get; set; }               //「預り」文言
        public decimal Index { get; set; }                      // 行番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }              //管理番号
        public DateTime? ArrivalPlanDate { get; set; }          //入庫日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }                     //車種名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RegistrationNumber { get; set; }          //登録番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                         //車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }                //顧客名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }                  //伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ServiceOrderStatusName { get; set; }      //伝票ステータス名
    }

    //Add 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
    public class CarSalseList : ICrmsModel
    {
        public int? TotalJul_Cnt { get; set; }
        public int? TotalAug_Cnt { get; set; }
        public int? TotalSep_Cnt { get; set; }
        public int? TotalOct_Cnt { get; set; }
        public int? TotalNov_Cnt { get; set; }
        public int? TotalDec_Cnt { get; set; }
        public int? TotalJan_Cnt { get; set; }
        public int? TotalFeb_Cnt { get; set; }
        public int? TotalMar_Cnt { get; set; }
        public int? TotalApr_Cnt { get; set; }
        public int? TotalMay_Cnt { get; set; }
        public int? TotalJun_Cnt { get; set; }
        public int? TotalCount { get; set; }

        public List<GetCarSalesList_Result> RetLine { get; set; }

        //コンストラクタ定義
        public CarSalseList()
        {
            TotalJul_Cnt = 0;
            TotalAug_Cnt = 0;
            TotalSep_Cnt = 0;
            TotalOct_Cnt = 0;
            TotalNov_Cnt = 0;
            TotalDec_Cnt = 0;
            TotalJan_Cnt = 0;
            TotalFeb_Cnt = 0;
            TotalMar_Cnt = 0;
            TotalApr_Cnt = 0;
            TotalMay_Cnt = 0;
            TotalJun_Cnt = 0;
            TotalCount = 0;

            RetLine = new List<GetCarSalesList_Result>();
        }

    }

    //Add 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力
    public class PurchaseOrderExcelHeader : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentName { get; set; }              // 部門名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FrontEmployeeName { get; set; }           // フロント担当者名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EmployeeSupplierName { get; set; }        // メカニック or外注者名
        public DateTime PurchaseOrderDate { get; set; }         // 発注日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }                  //伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }                //顧客名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }                     //車種名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ModelName { get; set; }                   //型式
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FirstRegistration { get; set; }           //初年度登録
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                         //車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OrderTypeName { get; set; }               //オーダー区分名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerOrderNumber { get; set; }            //メーカーオーダー番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string InvoiceNumber { get; set; }               //インボイス番号
        public DateTime? ArrivalPlanDate { get; set; }          //入庫日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo { get; set; }                        //引き継ぎメモ
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseOrderNumber { get; set; }         //部品発注番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string sheetNameIdx { get; set; }                //シート名のインデックス

        public List<PurchaseOrderExcelLine> line { get; set; }  //明細

        //コンストラクタ定義
        public PurchaseOrderExcelHeader()
        {
            line = new List<PurchaseOrderExcelLine>();
        }
    }

    //Add 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力
    public class PurchaseOrderExcelLine : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OrderPartsNumber { get; set; }            // 発注番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OrderPartsNameJp { get; set; }            // 発注部品名
        public decimal? Quantity { get; set; }                  // 数量
        public decimal? Price { get; set; }                     // 定価(部品マスタの定価 => サービス伝票の単価)
        public decimal? Cost { get; set; }                      // 原価
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Memo { get; set; }                        // 備考
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string StockStatus { get; set; }                 // 判断
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }                 // 部品番号
    }


    //Add 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 新規作成
    public class ServiceSalesSlipHeader : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PostCode { get; set; }                    // 郵便番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Prefecture { get; set; }                  // 都道府県
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Address1 { get; set; }                    // 住所１
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Address2 { get; set; }                    // 住所２
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }                // 顧客名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TelNumber { get; set; }                   // 電話番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MobileNumber { get; set; }                // 携帯電話番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentFullName { get; set; }          // 部門名称
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string D_PostCode { get; set; }                  // 郵便番号（部門）
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string D_Adress { get; set; }                    // 住所(部門) 都道府県＋住所１＋住所２
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerCode { get; set; }                // 顧客コード
        public DateTime? SalesDate { get; set; }                // 売上日(=納車日)
        public DateTime? ArrivalPlanDate { get; set; }          // 入庫日
        public DateTime? SalesDate2 { get; set; }               // 出庫日(=納車日)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipRevNumber { get; set; }               // 伝票番号＋リビジョン番号
        public int PageCnt { get; set; }                        // ページ番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RegistNumber { get; set; }                // 登録番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }                     // 車種名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EmployeeName { get; set; }                // 担当者名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EngineType { get; set; }                  // 原動機型式
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                         // 車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ModelName { get; set; }                   // 型式
        public decimal? Mileage { get; set; }                   // 走行距離
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FirstRegistration { get; set; }           // 初年度登録
        public DateTime? NextInspectionDate { get; set; }          // 次回点検日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarTaxTitle { get; set; }                 // 自動車税種別割タイトル
        public decimal? CarTax { get; set; }                    // 自動車税種別割
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarLiabilityInsuranceTitle { get; set; }  // 自賠責保険料タイトル
        public decimal? CarLiabilityInsurance { get; set; }     // 自賠責保険料
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarWeightTaxTitle { get; set; }           // 自動車重量税タイトル
        public decimal? CarWeightTax { get; set; }              // 自動車重量税
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FiscalStampCostTitle { get; set; }        // 印紙代タイトル
        public decimal? FiscalStampCost { get; set; }           // 印紙代
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NumberPlateCostTitle { get; set; }        // ナンバー代タイトル
        public decimal? NumberPlateCost { get; set; }           // ナンバー代
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MechanicEmployeeName { get; set; }        // メカ担当者

        public decimal? EngineerTotalAmount { get; set; }       // 工賃合計
        public decimal? PartsTotalAmount { get; set; }          // 部品合計
        public decimal? SubTotalAmount { get; set; }            // 整備料合計
        public decimal? TotalTaxAmount { get; set; }            // 消費税合計
        public decimal? CostTotalAmount { get; set; }           // 諸費用合計
        public decimal? TotalBalance { get; set; }              // 請求残(GrandTotalAmount - TotalInAmount)
        public decimal? TotalInAmount { get; set; }             // 入金合計

        public List<ServiceSalesSlipLine> list { get; set; }           //明細

        //コンストラクタ
        public ServiceSalesSlipHeader()
        {
            list = new List<ServiceSalesSlipLine>();
        }

        //コピーメソッド
        public ServiceSalesSlipHeader Clone()
        {
            return (ServiceSalesSlipHeader)MemberwiseClone();
        }
    }

    //Add 2018/01/18 arc yano #3834
    public class ServiceSalesSlipLine : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LineContents { get; set; }                // 明細の内容
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string WorkTypeName { get; set; }                // 区分名
        public decimal? FeeAmount { get; set; }                 // 技術料    ※現状は未使用
        public decimal? Quantity { get; set; }                  // 数量
        public decimal? Price { get; set; }                     // 金額      ※現状は未使用
        public decimal? PartAmount { get; set; }                // 部品金額  ※現状は未使用
    }

    //Add 2018/01/18 arc yano #3834
    public partial class GetServiceSalesSlipResult : ICrmsModel
    {
        public string ArrivalPlanDateFrom { get; set; }      // 入庫日(From)
        public string ArrivalPlanDateTo { get; set; }        // 入庫日(To)
    }

    //Add 2018/01/23 arc yano  #3836
    public class JournalTransfer : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }                  // 伝票番号
        public DateTime? JournalDate { get; set; }              // 入金日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }                // 顧客名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesStatusName { get; set; }             // 伝票ステータス名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AccountTypeName { get; set; }              // 入金種別名
        public decimal? Amount { get; set; }                    // 入金額
        public bool TransferCheck { get; set; }                  // 振替実行チェック
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TransferStlipNumber { get; set; }         // 振替先伝票番号
        public Guid JournalId { get; set; }                     // 入金実績ID
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }              // 部門コード
    }

    //Add 2018/01/23 arc yano  #3836
    public class JournalTransferList : ICrmsModel
    {
        public bool InitFlag { get; set; }                             // 初回表示フラグ

        public List<JournalTransfer> list { get; set; }                // 入金振替

        public JournalTransferList(bool criteriaInit)
        {
            list = new List<JournalTransfer>();                        //入金振替リストの初期化

            InitFlag = criteriaInit;                                   //初期表示フラグの設定           
        }
    }

    //Add 2018/05/22 arc yano #3887
    public class PartsExcelImportList
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }                     // 部品番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNameJp { get; set; }                     // 部品名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNameEn { get; set; }                     // 部品名(英語)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerCode { get; set; }                       // メーカーコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerPartsNumber { get; set; }                // メーカー部品番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerPartsNameJp { get; set; }                // メーカー部品名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerPartsNameEn { get; set; }                // メーカー部品名(英語)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Price { get; set; }                           // 価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesPrice { get; set; }                      // 販売価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SoPrice { get; set; }                         // S/O価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Cost { get; set; }                            // 原価
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ClaimPrice { get; set; }                      // クレーム申請部品代
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MpPrice { get; set; }                         // MP価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EoPrice { get; set; }                         // EO価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string GenuineType { get; set; }                     // 純正区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string UnitCD1 { get; set; }                         // 単位
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string QuantityPerUnit1 { get; set; }                // 単位辺りの数量
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NonInventoryFlag { get; set; }                // 棚卸対象外フラグ
        public bool NewPartsFlag { get; set; }                      // 新規部品
    }

    //Add 2018/05/25 arc yano #3888
    public class InspectionDate
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }                  // 車両管理番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                             // 車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PreviousDate { get; set; }                 // 日付(変更前) 
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FollowDate { get; set; }                   // 日付(変更後)
    }

    // Add 2018/05/28 arc yano #3886
    public class JournalExcelImport
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string JournalType { get; set; }                     // 入金種別
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DepartmentCode { get; set; }                  // 部門コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerClaimCode { get; set; }               // 請求先コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }                      // 伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string JournalDate { get; set; }                     // 入金日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AccountType { get; set; }                     // 口座種別
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AccountCode { get; set; }                     // 口座コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Amount { get; set; }                          // 入金額
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ReceiptPlanFlag { get; set; }                 // 入金消込フラグ
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string TransferFlag { get; set; }                    // 転記フラグ
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OfficeCode { get; set; }                      // 事業所コード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CashAccountCode { get; set; }                 // 現金口座コード
    }

    //Add 2018/06/06 arc yano #3883
    public class DepreciationRateExcelImport
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string UsefulLives { get; set; }                     // 耐用年数
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Rate { get; set; }                            // 償却率
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RevisedRate { get; set; }                     // 改訂償却率
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SecurityRatio { get; set; }                   // 償却保証率
    }

    //Add 2018/06/06 arc yano #3883
    public class FinancialDataExcelImport
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }                  // 車両管理番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                             // 車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FinancialAmount { get; set; }                // 財務価格(Excel取込価格・税込)
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public decimal CalcFinancialAmount { get; set; }            // 財務価格(Excel取込価格・演算結果)
    }


    //Add 2018/06/06 arc yano #3911
    public partial class Get_CarSlipStatusChangeResult
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesOrderStatusName { get; set; }            // 伝票ステータス名
    }

    //Mod 2018/08/28 yano #3922
    public class CarStockForExcelImport
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ProcessDate { get; set; }                             //対象年月日       
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string BrandStore { get; set; }                              //取扱ブランド
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseDate { get; set; }                            //入庫日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCarNumber { get; set; }                          //管理番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerName { get; set; }                               //メーカー名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarName { get; set; }                                 //車種名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Vin { get; set; }                                     //車台番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PurchaseLocationName { get; set; }                    //仕入・在庫拠点
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string InventoryLocationName { get; set; }                   //実棚
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CarPurchaseTypeName { get; set; }                     //仕入区分名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SupplierName { get; set; }                            //仕入先名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string BeginningInventory { get; set; }                      //月初在庫
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MonthPurchase { get; set; }                           //当月仕入
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OtherAccount { get; set; }                            //他勘定受入
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RecycleAmount { get; set; }                           //リサイクル料
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesDate { get; set; }                            //納車日
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SlipNumber { get; set; }                              //伝票番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesDepartmentName { get; set; }                     //販売店舗
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerTypeName { get; set; }                        //販売先区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CustomerName { get; set; }                            //販売先
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesPrice { get; set; }                              //車輛本体価格
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DiscountAmount { get; set; }                          //値引
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ShopOptionAmount { get; set; }                        //付属品
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCostTotalAmount { get; set; }                    //諸費用
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesTotalAmount { get; set; }                        //売上総合計
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesCostAmount { get; set; }                         //売上原価
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SalesProfits { get; set; }         　                 //粗利
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SelfRegistration { get; set; }                        //自社登録
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string ReductionTotal { get; set; }                          //他勘定振替
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CancelPurchase { get; set; }                          //仕入キャンセル
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EndInventory { get; set; }                            //月末在庫
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string NewUsedType { get; set; }                             //新中区分
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string SheetName { get; set; }                               //シート名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public int SeparateCnt { get; set; }                                //新車・中古車区切行数

    }

    //Add 2020/01/06 #4025
    public class AccountConv
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string AccountCode{ get; set; }                            //勘定奉行科目コード
        public decimal Amount { get; set; }                               //金額
    }

    //Add 2021/02/22 yano #4083
    public class PartsCriteria : ICrmsModel
    {
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNumber { get; set; }                         //部品番号
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PartsNameJp { get; set; }                         //部品名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerCode { get; set; }                           //メーカーコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string MakerName { get; set; }                           //メーカー名
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DelFlag { get; set; }                             //ステータスコード
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string DelFlagName { get; set; }                         //ステータス（有効/無効）
    }
}
