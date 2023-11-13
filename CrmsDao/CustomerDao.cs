using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;

namespace CrmsDao
{
    /// <summary>
    /// 顧客マスタアクセスクラス
    ///   顧客マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CustomerDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CustomerDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 顧客マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <returns>顧客マスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　デフォルト引数追加（削除フラグを考慮するかどうかのフラグ追加（デフォルト考慮する））
        public Customer GetByKey(string customerCode, bool includeDeleted = false)
        {

            // 顧客データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            Customer customer =
                (from a in db.Customer
                 where a.CustomerCode.Equals(customerCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (customer != null)
            {
                customer = EditModel(customer);
            }

            // 顧客データの返却
            return customer;
        }


        /// <summary>
        /// 顧客マスタデータ検索
        /// （ページング対応）
        /// </summary>
        /// <param name="customerCondition">顧客検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>顧客マスタデータ検索結果</returns>
        public PaginatedList<Customer> GetListByCondition(Customer customerCondition, int? pageIndex, int? pageSize)
        {
            // ページング制御情報を付与した顧客データの返却
            PaginatedList<Customer> ret = new PaginatedList<Customer>(GetQueryable(customerCondition), pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }
        
        /// <summary>
        /// 顧客マスタデータ検索
        /// （ページング非対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<Customer> GetListByCondition(DocumentExportCondition condition){
            Customer customerCondition = new Customer();
            customerCondition.CustomerType = condition.CustomerType;
            customerCondition.CustomerRank = condition.CustomerRank;
            customerCondition.CustomerKind = condition.CustomerKind;
            customerCondition.CustomerClaim = new CustomerClaim();
            customerCondition.CustomerClaim.CustomerClaimType = condition.CustomerClaimType;
            customerCondition.LastUpdateDate = condition.LastUpdateDate;
            customerCondition.DmFlag = condition.DmFlag;
            customerCondition.FirstReceiptionDateFrom = condition.FirstReceiptionDateFrom;
            customerCondition.FirstReceiptionDateTo = condition.FirstReceiptionDateTo;
            customerCondition.LastReceiptionDateFrom = condition.LastReceiptionDateFrom;
            customerCondition.LastReceiptionDateTo = condition.LastReceiptionDateTo;
            customerCondition.FirstRegistrationFrom = condition.FirstRegistrationFrom;
            customerCondition.FirstRegistrationTo = condition.FirstRegistrationTo;
            customerCondition.ExpireDateFrom = condition.ExpireDateFrom;
            customerCondition.ExpireDateTo = condition.ExpireDateTo;
            customerCondition.NextInspectionDateFrom = condition.NextInspectionDateFrom;
            customerCondition.NextInspectionDateTo = condition.NextInspectionDateTo;
            customerCondition.RegistrationDateFrom = condition.RegistrationDateFrom;
            customerCondition.RegistrationDateTo = condition.RegistrationDateTo;
            customerCondition.DelFlag = "0";
            customerCondition.InterestedCustomer = condition.InterestedCustomer;
            customerCondition.DepartmentList = condition.DepartmentList;
            customerCondition.CarList = condition.CarList;
            customerCondition.CarBrandCode = condition.CarBrandCode;
            customerCondition.CarCode = condition.CarCode;
            customerCondition.CarGradeCode = condition.CarGradeCode;
            customerCondition.MorterViecleOfficialCode = condition.MorterViecleOfficialCode;
            return GetQueryableForDM(customerCondition, GetQueryable(customerCondition)).ToList<Customer>();
        }

        private IQueryable<Customer> GetQueryableForDM(Customer customerCondition,IQueryable<Customer> customerList) {
            string firstRegistrationFrom = customerCondition.FirstRegistrationFrom;
            string firstRegistrationTo = customerCondition.FirstRegistrationTo;
            DateTime? expireDateFrom = customerCondition.ExpireDateFrom;
            DateTime? expireDateTo = customerCondition.ExpireDateTo;
            DateTime? registrationDateFrom = customerCondition.RegistrationDateFrom;
            DateTime? registrationDateTo = customerCondition.RegistrationDateTo;
            DateTime? nextInspectionDateFrom = customerCondition.NextInspectionDateFrom;
            DateTime? nextInspectionDateTo = customerCondition.NextInspectionDateTo;
            DateTime? salesDateFrom = customerCondition.SalesDateFrom;
            DateTime? salesDateTo = customerCondition.SalesDateTo;
            DateTime? salesOrderDateFrom = customerCondition.SalesOrderDateFrom;
            DateTime? salesOrderDateTo = customerCondition.SalesOrderDateTo;

            string carBrandCode = customerCondition.CarBrandCode;
            string carCode = customerCondition.CarCode;
            string carGradeCode = customerCondition.CarGradeCode;
            string morterViecleOfficialCode = customerCondition.MorterViecleOfficialCode;

            // 車検有効期限・登録日・次回点検日・初度登録が指定されている場合
            if (expireDateFrom != null || expireDateTo != null
                || registrationDateFrom != null || registrationDateTo != null
                || nextInspectionDateFrom != null || nextInspectionDateTo != null
                || !string.IsNullOrEmpty(firstRegistrationFrom) || !string.IsNullOrEmpty(firstRegistrationTo)
                ) {
                customerList = from a in customerList
                               where (
                                from b in db.SalesCar
                                where b.DelFlag.Equals("0")
                                && (expireDateFrom == null || DateTime.Compare(b.ExpireDate ?? DaoConst.SQL_DATETIME_MIN, expireDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                                && (expireDateTo == null || DateTime.Compare(b.ExpireDate ?? DaoConst.SQL_DATETIME_MAX, expireDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                                && (registrationDateFrom == null || DateTime.Compare(b.RegistrationDate ?? DaoConst.SQL_DATETIME_MIN, registrationDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                                && (registrationDateTo == null || DateTime.Compare(b.RegistrationDate ?? DaoConst.SQL_DATETIME_MAX, registrationDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                                && (nextInspectionDateFrom == null || DateTime.Compare(b.NextInspectionDate ?? DaoConst.SQL_DATETIME_MIN, nextInspectionDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                                && (nextInspectionDateTo == null || DateTime.Compare(b.NextInspectionDate ?? DaoConst.SQL_DATETIME_MAX, nextInspectionDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                                && (string.IsNullOrEmpty(firstRegistrationFrom) || b.FirstRegistrationYear.CompareTo(firstRegistrationFrom) >= 0)
                                && (string.IsNullOrEmpty(firstRegistrationTo) || b.FirstRegistrationYear.CompareTo(firstRegistrationTo) <= 0)
                                select b.UserCode
                                ).Contains(a.CustomerCode)
                               select a;
            }

            // 受注日・納車日が指定されている場合
            if (salesDateFrom != null || salesDateTo != null
                || salesOrderDateFrom != null || salesOrderDateTo != null
                ) {
                customerList = from a in customerList
                               where (
                  from c in db.CarSalesHeader
                  where c.DelFlag.Equals("0")
                  && (salesDateFrom == null || DateTime.Compare(c.SalesDate ?? DaoConst.SQL_DATETIME_MIN, salesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                  && (salesDateTo == null || DateTime.Compare(c.SalesDate ?? DaoConst.SQL_DATETIME_MAX, salesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                  && (salesOrderDateFrom == null || DateTime.Compare(c.SalesOrderDate ?? DaoConst.SQL_DATETIME_MIN, salesOrderDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                  && (salesOrderDateTo == null || DateTime.Compare(c.SalesOrderDate ?? DaoConst.SQL_DATETIME_MIN, salesOrderDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                  select c.CustomerCode
                  ).Contains(a.CustomerCode)
                               select a;
            }

            // 対象拠点が指定されている場合
            if (customerCondition.DepartmentList != null && customerCondition.DepartmentList.Count() > 0) {
                customerList = from a in customerList
                               where (
                               from b in customerCondition.DepartmentList
                               select b.DepartmentCode).Contains(a.DepartmentCode)
                               select a;
            }

            // 見込み客の場合
            if (customerCondition.InterestedCustomer) {
                customerList = from a in customerList
                               where (
                               from b in db.CustomerReceiption
                               where b.InterestedCar1 != null && b.InterestedCar1 != ""
                               || b.InterestedCar2 != null && b.InterestedCar2 != ""
                               || b.InterestedCar3 != null && b.InterestedCar3 != ""
                               || b.InterestedCar4 != null && b.InterestedCar4 != ""
                               select b.CustomerCode
                               ).Contains(a.CustomerCode)
                               select a;

                // 商談車種が指定されている場合
                if (customerCondition.CarList != null && customerCondition.CarList.Count() > 0) {
                    customerList = from a in customerList
                                   where (
                                       from b in db.CustomerReceiption
                                       where (
                                           from c in customerCondition.CarList
                                           select c.CarCode
                                           ).Contains(b.InterestedCar1)
                                           || (
                                           from c in customerCondition.CarList
                                           select c.CarCode
                                           ).Contains(b.InterestedCar2)
                                           || (
                                           from c in customerCondition.CarList
                                           select c.CarCode
                                           ).Contains(b.InterestedCar3)
                                           || (
                                           from c in customerCondition.CarList
                                           select c.CarCode
                                           ).Contains(b.InterestedCar4)
                                       select b.CustomerCode
                                   ).Contains(a.CustomerCode)
                                   select a;
                }
            }

            // サービスDMでブランド・車種・グレード、ナンバープレートが指定されている場合
            if (!string.IsNullOrEmpty(carBrandCode) || !string.IsNullOrEmpty(carCode) || !string.IsNullOrEmpty(carGradeCode) || !string.IsNullOrEmpty(morterViecleOfficialCode)) {
                customerList = from a in customerList
                               where (
                                    from b in db.SalesCar
                                    where (string.IsNullOrEmpty(carBrandCode) || b.CarGrade.Car.CarBrandCode.Equals(carBrandCode))
                                    && (string.IsNullOrEmpty(carCode) || b.CarGrade.CarCode.Equals(carCode))
                                    && (string.IsNullOrEmpty(carGradeCode) || b.CarGradeCode.Equals(carGradeCode))
                                    && (string.IsNullOrEmpty(morterViecleOfficialCode) || b.MorterViecleOfficialCode.Contains(morterViecleOfficialCode))
                                    && b.DelFlag.Equals("0")
                                    select b.UserCode
                                    ).Contains(a.CustomerCode)
                               select a;
            }
            return customerList;
        }

        /// <summary>
        /// 顧客検索式を取得する
        /// </summary>
        /// <param name="customerCondition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2018/04/26 arc yano #3650 顧客一覧で「上様」を一番上に表示したい
        /// </history>
        private IQueryable<Customer> GetQueryable(Customer customerCondition){

            string customerCode = customerCondition.CustomerCode;
            string customerRank = customerCondition.CustomerRank;
            string customerKind = customerCondition.CustomerKind;
            string customerName = customerCondition.CustomerName;
            string customerNameKana = customerCondition.CustomerNameKana;
            string customerKbn = customerCondition.CustomerType;
            string sex = customerCondition.Sex;
            DateTime? birthdayFrom = customerCondition.BirthdayFrom;
            DateTime? birthdayTo = customerCondition.BirthdayTo;
            string occupation = customerCondition.Occupation;
            string carOwner = customerCondition.CarOwner;
            string telNumber = customerCondition.TelNumber;
            string telNumber4 = customerCondition.TelNumber4;
            DateTime? firstReceiptionDateFrom = customerCondition.FirstReceiptionDateFrom;
            DateTime? firstReceiptionDateTo = customerCondition.FirstReceiptionDateTo;
            DateTime? lastReceiptionDateFrom = customerCondition.LastReceiptionDateFrom;
            DateTime? lastReceiptionDateTo = customerCondition.LastReceiptionDateTo;
            DateTime? lastUpdateDate = customerCondition.LastUpdateDate;
            string dmFlag = customerCondition.DmFlag;
            string delFlag = customerCondition.DelFlag;

            string customerClaimType = "";
            try { customerClaimType = customerCondition.CustomerClaim.CustomerClaimType; } catch (NullReferenceException) { }



            // 顧客データの取得
            IQueryable<Customer> customerList =
                    from a in db.Customer
                    orderby (a.DisplayOrder ?? 9999999), a.CustomerCode  //Add 2018/04/26 arc yano #3650
                    where (string.IsNullOrEmpty(customerCode) || a.CustomerCode.Contains(customerCode))
                    && (string.IsNullOrEmpty(customerName) || a.CustomerName.Contains(customerName))
                    && (string.IsNullOrEmpty(customerNameKana) || a.CustomerNameKana.Contains(customerNameKana))
                    && (string.IsNullOrEmpty(customerRank) || a.CustomerRank.Equals(customerRank))
                    && (string.IsNullOrEmpty(customerKind) || a.CustomerKind.Equals(customerKind))
                    && (string.IsNullOrEmpty(customerKbn) || a.CustomerType.Equals(customerKbn))
                    && (string.IsNullOrEmpty(sex) || a.Sex.Equals(sex))
                    && (birthdayFrom == null || DateTime.Compare(a.Birthday ?? DaoConst.SQL_DATETIME_MIN, birthdayFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (birthdayTo == null || DateTime.Compare(a.Birthday ?? DaoConst.SQL_DATETIME_MAX, birthdayTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(occupation) || a.Occupation.Equals(occupation))
                    && (string.IsNullOrEmpty(carOwner) || a.CarOwner.Equals(carOwner))
                    && (string.IsNullOrEmpty(telNumber) || a.TelNumber.Contains(telNumber) || a.MobileNumber.Contains(telNumber))
                    && (string.IsNullOrEmpty(telNumber4) || a.TelNumber.Substring(a.TelNumber.Length - 4, 4).Equals(telNumber4) || a.MobileNumber.Substring(a.MobileNumber.Length - 4, 4).Equals(telNumber4))
                    && (firstReceiptionDateFrom == null || DateTime.Compare(a.FirstReceiptionDate ?? DaoConst.SQL_DATETIME_MIN, firstReceiptionDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (firstReceiptionDateTo == null || DateTime.Compare(a.FirstReceiptionDate ?? DaoConst.SQL_DATETIME_MAX, firstReceiptionDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (lastReceiptionDateFrom == null || DateTime.Compare(a.LastReceiptionDate ?? DaoConst.SQL_DATETIME_MIN, lastReceiptionDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (lastReceiptionDateTo == null || DateTime.Compare(a.LastReceiptionDate ?? DaoConst.SQL_DATETIME_MAX, lastReceiptionDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (lastUpdateDate == null || DateTime.Compare(a.LastUpdateDate ?? DaoConst.SQL_DATETIME_MIN, lastUpdateDate ?? DaoConst.SQL_DATETIME_MAX) >=0)
                    && (string.IsNullOrEmpty(dmFlag) || a.DmFlag.Equals(dmFlag))
                    && (string.IsNullOrEmpty(customerClaimType) || a.CustomerClaim.CustomerClaimType.Equals(customerClaimType))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    select a;

            return customerList;
        }

        public List<Customer> GetListAll() {
            var query =
                from a in db.Customer
                select a;
            return query.ToList();
            //return new PaginatedList<Customer>(query, 0, 1000);
        }
        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="customer">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Customer EditModel(Customer customer)
        {
            // 内部コード項目の名称情報取得
            customer.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, customer.DelFlag);

            // 出口
            return customer;
        }


        /// <summary>
        /// 顧客DMリスト検索
        /// </summary>
        /// <param name="customerCondition">顧客検索条件</param>
        /// <returns>顧客DMリスト検索結果</returns>
        /// <history>
        /// 2018/04/25 arc yano #3842 顧客データリスト（営業案内）　住所による絞込の追加
        /// </history>
        public PaginatedList<CustomerDataResult> GetCustomerDataListByCondition(CustomerDataSearch condition, int? pageIndex, int? pageSize)
        {
 
            //Mod　2015/02/25 arc nakayama 検索速度向上のためストプロでデータ取得するように変更
            //Add 2015/04/10 arc nakayama 顧客DM指摘事項修正Part2　住所再確認を検索条件に入れる
            var CustomerDataList = db.GetCustomerDataList(condition.DmFlag, //DM可否
                                                         condition.DepartmentCode2, //営業担当部門コード
                                                         condition.CarEmployeeCode, //営業部門担当者コード
                                                         condition.ServiceDepartmentCode2, //サービス担当部門コード
                                                         string.Format("{0:yyyy/MM/dd}", condition.SalesDateFrom), //車両伝票納車日From
                                                         string.Format("{0:yyyy/MM/dd}", condition.SalesDateTo), //車両伝票納車日To
                                                         string.Format("{0:yyyy/MM/dd}", condition.ArrivalPlanDateFrom), //サービス伝票納車日From
                                                         string.Format("{0:yyyy/MM/dd}", condition.ArrivalPlanDateTo),   //サービス伝票納車日To                                                      
                                                         condition.MakerName, //メーカー名
                                                         condition.CarName,   //車種名
                                                         string.Format("{0:yyyy/MM/dd}", condition.FirstRegistrationFrom), //初年度登録From
                                                         string.Format("{0:yyyy/MM/dd}", condition.FirstRegistrationTo),   //初年度登録To
                                                         string.Format("{0:yyyy/MM/dd}", condition.RegistrationDateFrom),  //登録年月日From
                                                         string.Format("{0:yyyy/MM/dd}", condition.RegistrationDateTo),    //登録年月日To 
                                                         condition.CustomerAddressReconfirm, //住所再確認
                                                         condition.CustomerKind //顧客種別
                                                         );

            List<CustomerDataResult> list = new List<CustomerDataResult>();

            foreach (var CustomerData in CustomerDataList)
            {
                CustomerDataResult CustomerDataResultList = new CustomerDataResult();

                //営業(DM可否)　Nullなら空文字をセット
                if (string.IsNullOrEmpty(CustomerData.DmFlag))
                {
                    CustomerDataResultList.DmFlag = "";
                }
                else{
                    CustomerDataResultList.DmFlag = CustomerData.DmFlag;                
                }

                CustomerDataResultList.DmMemo = CustomerData.DmMemo;        //備考(DM可否)
                
                //Del 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合 車検案内に関する条件/項目を削除

                CustomerDataResultList.CustomerCode = CustomerData.CustomerCode;         //顧客コード(顧客情報)
                CustomerDataResultList.CustomerName = CustomerData.CustomerName + "　様";  //顧客名称(顧客情報)
                CustomerDataResultList.DepartmentName2 = CustomerData.DepartmentName2;  //営業担当部門名
                CustomerDataResultList.CarEmployeeName = CustomerData.CarEmployeeName;  //営業担当者名
                CustomerDataResultList.ServiceDepartmentName2 = CustomerData.ServiceDepartmentName2; //サービス担当部門名
                CustomerDataResultList.ServiceEmployeeName2 = CustomerData.ServiceEmployeeName2;     //サービス担当者名

                //郵便番号(顧客住所)　Nullなら空文字をセット
                if (string.IsNullOrEmpty(CustomerData.PostCode)){
                    CustomerDataResultList.CustomerPostCode = "";
                }else{
                    CustomerDataResultList.CustomerPostCode = CustomerData.PostCode;
                }
                CustomerDataResultList.CustomerPrefecture = CustomerData.Prefecture;//都道府県(顧客住所)
                // Mod 2015/01/07 arc nakayama 顧客DM指摘事項③ 都道府県と住所をつなげる
                CustomerDataResultList.CustomerAddress = CustomerData.Prefecture + CustomerData.City + CustomerData.Address1 + CustomerData.Address2;//住所(顧客住所)
                CustomerDataResultList.CustomerTelNumber = CustomerData.TelNumber;//電話番号(顧客住所)
                //Add 2015/07/17 arc nakayama #3224_「顧客データリスト」機能で携帯電話番号が出力されない
                CustomerDataResultList.MobileNumber = CustomerData.MobileNumber;//携帯電話番号(顧客住所)
                CustomerDataResultList.MailAddress = CustomerData.MailAddress;
                //Add 2015/12/16 arc ookubo #3318_メールアドレスを追加
                if (string.IsNullOrEmpty(CustomerData.MailAddress)){
                    CustomerDataResultList.MailAddress = CustomerData.MobileMailAddress;
                }else{
                    CustomerDataResultList.MailAddress = CustomerData.MailAddress;
                }
                // Mod 2015/06/22 arc nakayama 名称もストプロから取得してくるように変更 
                //CustomerDataResultList.CustomerAddressReconfirm = CustomerData.AddressReconfirm;//住所再確認(顧客住所)
                CustomerDataResultList.CustomerAddressReconfirmName = CustomerData.CustomerAddressReconfirmName;
                // Mod 2015/06/08 arc nakayama Null落ち防止対策のため変更(結果がNullの場合は空文字を入れる)
                if (CustomerData.CustomerKind == null){
                    CustomerDataResultList.CustomerKind = "";
                }else{
                    CustomerDataResultList.CustomerKind = CustomerData.CustomerKind; //顧客種別(顧客情報)
                }
                //郵便番号(DM発送先住所) Nullなら空文字をセット(「〒」を表示させないため)
                if (string.IsNullOrEmpty(CustomerData.CDPostCode))
                {
                    CustomerDataResultList.DmPostCode = "";
                }else{
                    CustomerDataResultList.DmPostCode = CustomerData.CDPostCode;
                }
                //Add 2015/07/01 arc nakayama #3220_顧客データリスト出力項目追加 DM発送先の顧客名
                if (!string.IsNullOrEmpty(CustomerData.FirstName) && !string.IsNullOrEmpty(CustomerData.LastName)){
                    CustomerDataResultList.DmCustomerName = CustomerData.FirstName + "　" + CustomerData.LastName + "　様";
                }else if (!string.IsNullOrEmpty(CustomerData.FirstName) && string.IsNullOrEmpty(CustomerData.LastName)){
                    CustomerDataResultList.DmCustomerName = CustomerData.FirstName + "　様";
                }
                CustomerDataResultList.DmPrefecture = CustomerData.CDPrefecture;    //都道府県(DM発送先住所)
                // Mod 2015/01/07 arc nakayama 顧客DM指摘事項③ 都道府県と住所をつなげる
                CustomerDataResultList.DmCustomerDmAddress = CustomerData.CDPrefecture + CustomerData.CDCity + CustomerData.CDAddress1 + CustomerData.CDAddress2;   //住所(DM発送先住所)
                CustomerDataResultList.DmTelNumber = CustomerData.CDTelNumber;      //電話番号(DM発送先住所)

                //Mod 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合
                //使用者と顧客が一致した場合のみ車両情報を設定する
                if (CustomerData.CustomerCode == CustomerData.UserCode)
                {
                    CustomerDataResultList.SalesCarNumber = CustomerData.SalesCarNumber;//管理番号(車両情報)
                    // Mod 2015/04/10 arc nakayama 顧客DM指摘事項修正Part2　陸運局だけじゃなくナンバープレートの情報をすべて出す
                    CustomerDataResultList.MorterViecleOfficialCode = CustomerData.MorterViecleOfficialCode + " " + CustomerData.RegistrationNumberType + " " + CustomerData.RegistrationNumberKana + " " + CustomerData.RegistrationNumberPlate;  //車両登録番号(車両情報)
                    CustomerDataResultList.Vin = CustomerData.Vin;  //車台番号(車両情報)
                    CustomerDataResultList.MakerName = CustomerData.MakerName;  //メーカー(車両情報)
                    CustomerDataResultList.CarName = CustomerData.CarName;  //車種名称(車両情報)
                    CustomerDataResultList.FirstRegistrationDate = CustomerData.FirstRegistrationDate;  //初年度登録(車両情報)
                    CustomerDataResultList.RegistrationDate = CustomerData.RegistrationDate;   //登録年月日(車両情報)
                    CustomerDataResultList.NextInspectionDate = CustomerData.NextInspectionDate; //次回点検日(車両情報)
                    CustomerDataResultList.ExpireDate = CustomerData.ExpireDate;               //車検満了日(車両情報)
                    //Add 2015/05/20 arc nakayama #3199_顧客データリスト出力項目追加
                    CustomerDataResultList.CarWeight = CustomerData.CarWeight;  // 車両重量(車両情報)
                }
                else
                {
                    CustomerDataResultList.SalesCarNumber = "";            //管理番号(車両情報)
                    CustomerDataResultList.MorterViecleOfficialCode = "";  //車両登録番号(車両情報)
                    CustomerDataResultList.Vin = "";                       //車台番号(車両情報)
                    CustomerDataResultList.MakerName = "";                 //メーカー(車両情報)
                    CustomerDataResultList.CarName = "";                   //車種名称(車両情報)
                    CustomerDataResultList.FirstRegistrationDate = null;   //初年度登録(車両情報)
                    CustomerDataResultList.RegistrationDate = null;        //登録年月日(車両情報)
                    CustomerDataResultList.NextInspectionDate = null;      //次回点検日(車両情報)
                    CustomerDataResultList.ExpireDate = null;              //車検満了日(車両情報)
                    CustomerDataResultList.CarWeight = null;               //車両重量(車両情報)
                }


                CustomerDataResultList.SalesDepartmentName = CustomerData.DepartmentName;   //部門(営業担当)
                CustomerDataResultList.SalesEmployeeName = CustomerData.EmployeeName;       //担当者(営業担当)
                CustomerDataResultList.SalesDate = CustomerData.SalesDate;             //納車年月日(営業担当)
                CustomerDataResultList.ServiceDepartmentName = CustomerData.ServiceDepartmentName;  //部門(サービス担当)
                CustomerDataResultList.ServiceEmployeeName = CustomerData.ServiceEmployeeName;      //担当者(サービス担当)
                //Mod 2015/07/17 arc nakayama サービス伝票納車日を入庫日に変更（ServiceSalesDate ⇒ ArrivalPlanDate）
                CustomerDataResultList.ArrivalPlanDate = CustomerData.ArrivalPlanDate;        //納車日(サービス担当)

                //DM発送先住所が未登録(NULLまたは空文字)　だった場合は顧客住所を設定する
                //※郵便番号(DM発送先住所)が空文字またはNULL　かつ　都道府県(DM発送先住所)が空文字またはNULL　かつ　住所(DM発送先住所)が空文字またはNULL　の場合
                //Add 2015/01/07 arc nakayama 顧客DM指摘事項⑤ DM発送先住所の電話番号が空文字またはNULLなら顧客情報の電話番号を入れる
                //Mod 2015/07/01 arc nakayama #3220_顧客データリスト出力項目追加 DM発送先の顧客名
                //Mod 2015/07/09 arc nakayama #3220_顧客データリスト出力項目追加 DM発送先に顧客情報を置き換える条件を変更
                if (string.IsNullOrEmpty(CustomerData.DmDelFlag) || CustomerData.DmDelFlag.Equals("1"))
                {
                    CustomerDataResultList.DmCustomerName = CustomerDataResultList.CustomerName;            //顧客名(DM発送先住所)
                    CustomerDataResultList.DmTelNumber = CustomerDataResultList.CustomerTelNumber;          //電話番号(DM発送先住所)
                    CustomerDataResultList.DmPostCode = CustomerDataResultList.CustomerPostCode;            //郵便番号(DM発送先住所)
                    CustomerDataResultList.DmPrefecture = CustomerDataResultList.CustomerPrefecture;        //都道府県(DM発送先住所)
                    CustomerDataResultList.DmCustomerDmAddress = CustomerDataResultList.CustomerAddress;    //住所(DM発送先住所)
                }

                //リスト追加
                list.Add(CustomerDataResultList);
            }

            //Add 2018/04/25 arc yano #3842
            //顧客住所で絞込み
            if (!string.IsNullOrWhiteSpace(condition.CustomerAddress))
            {
                list = list.Where(x => ((x.DmPrefecture ?? "") + (x.DmCustomerDmAddress ?? "")).Contains(condition.CustomerAddress)).ToList();
            }

            return new PaginatedList<CustomerDataResult>(list.AsQueryable<CustomerDataResult>(), pageIndex ?? 0, pageSize ?? 0);

        }


        /// <summary>
        /// 顧客DMリスト検索(CSV出力用)
        /// </summary>
        /// <param name="customerCondition">顧客検索条件</param>
        /// <returns>顧客DMリスト検索結果</returns>
        /// <history>
        /// 2018/04/25 arc yano #3842 顧客データリスト（営業案内）　住所による絞込の追加
        /// </history>
        public List<CustomerDataExcelResult> GetCustomerDataListByConditionForCSV(DocumentExportCondition condition)
        {
            //Mod　2015/02/25 arc nakayama 検索速度向上のためストプロでデータ取得するように変更
            //Add 2015/04/10 arc nakayama 顧客DM指摘事項修正Part2　住所再確認を検索条件に入れる
            var CustomerDataList = db.GetCustomerDataList(condition.DmFlag,
                                                         condition.DepartmentCode2,
                                                         condition.CarEmployeeCode,
                                                         condition.ServiceDepartmentCode2,
                                                         string.Format("{0:yyyy/MM/dd}", condition.SalesDateFromForDm),
                                                         string.Format("{0:yyyy/MM/dd}", condition.SalesDateToForDm),
                                                         string.Format("{0:yyyy/MM/dd}", condition.ArrivalPlanDateFrom),
                                                         string.Format("{0:yyyy/MM/dd}", condition.ArrivalPlanDateTo),                                                         
                                                         condition.MakerName,
                                                         condition.CarName,
                                                         string.Format("{0:yyyy/MM/dd}", condition.DtFirstRegistrationFrom),
                                                         string.Format("{0:yyyy/MM/dd}", condition.DtFirstRegistrationTo),
                                                         string.Format("{0:yyyy/MM/dd}",condition.RegistrationDateFrom),
                                                         string.Format("{0:yyyy/MM/dd}",condition.RegistrationDateTo),
                                                         condition.CustomerAddressReconfirm,
                                                         condition.CustomerKind
                                                        );

            List<CustomerDataExcelResult> list = new List<CustomerDataExcelResult>();
            CodeDao dao = new CodeDao(db);

            foreach (var CustomerData in CustomerDataList)
            {
                CustomerDataExcelResult CustomerDataResultList = new CustomerDataExcelResult();

                //Mod 2015/06/17 arc nakayama 名称をマスタから取得するように変更(ストプロ側で)
                CustomerDataResultList.DmFlagName = CustomerData.DmFlagName;

                CustomerDataResultList.DmMemo = CustomerData.DmMemo;        //備考(DM可否)

                //Mod 2015/06/17 arc nakayama 名称をマスタから取得するように変更
                //Del 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合 車検案内に関する条件/項目を削除
                CustomerDataResultList.CustomerCode = CustomerData.CustomerCode;         //顧客コード(顧客情報)
                CustomerDataResultList.CustomerName = CustomerData.CustomerName + "　様";  //顧客名称(顧客情報)
                CustomerDataResultList.DepartmentName2 = CustomerData.DepartmentName2;  //営業担当部門名
                CustomerDataResultList.CarEmployeeName = CustomerData.CarEmployeeName;  //営業担当者名
                CustomerDataResultList.ServiceDepartmentName2 = CustomerData.ServiceDepartmentName2; //サービス担当部門名
                CustomerDataResultList.ServiceEmployeeName2 = CustomerData.ServiceEmployeeName2;     //サービス担当者名

                //郵便番号(顧客住所)　Nullなら空文字をセット(「〒」を表示させないため)
                if (string.IsNullOrEmpty(CustomerData.PostCode))
                {
                    CustomerDataResultList.CustomerPostCode = "";
                }
                else
                {
                    CustomerDataResultList.CustomerPostCode = CustomerData.PostCode;
                }
                CustomerDataResultList.CustomerPrefecture = CustomerData.Prefecture;//都道府県(顧客住所)
                // Mod 2015/01/07 arc nakayama 顧客DM指摘事項③ 都道府県と住所をつなげる
                CustomerDataResultList.CustomerAddress = CustomerData.Prefecture + CustomerData.City + CustomerData.Address1 + CustomerData.Address2;//住所(顧客住所)
                CustomerDataResultList.CustomerTelNumber = CustomerData.TelNumber;//電話番号(顧客住所)
                //Add 2015/07/17 arc nakayama #3224_「顧客データリスト」機能で携帯電話番号が出力されない
                CustomerDataResultList.MobileNumber = CustomerData.MobileNumber;//電話番号(顧客住所)
                //Add 2015/12/16 arc ookubo #3318_メールアドレスを追加
                if (string.IsNullOrEmpty(CustomerData.MailAddress))
                {
                    CustomerDataResultList.MailAddress = CustomerData.MobileMailAddress;
                } else {
                    CustomerDataResultList.MailAddress = CustomerData.MailAddress;
                }

                // Mod 2015/04/10 arc nakayama 顧客DM指摘事項修正Part2 CSV出力の時は「要」の場合のみ出力する
                // Mod 2015/06/22 arc nakayama 名称もストプロから取得してくるように変更
                //if (CustomerData.AddressReconfirm){          //住所再確認(顧客住所)
                //    CustomerDataResultList.CustomerAddressReconfirmName = "要";
                //}else{
                //    CustomerDataResultList.CustomerAddressReconfirmName = "";
                //}
                CustomerDataResultList.CustomerAddressReconfirmName = CustomerData.CustomerAddressReconfirmName;

                //Mod 2015/06/17 arc nakayama 名称をマスタから取得するように変更
                CustomerDataResultList.CustomerKindName = CustomerData.CustomerKindName;

                //郵便番号(DM発送先住所) Nullなら空文字をセット(「〒」を表示させないため)
                if (string.IsNullOrEmpty(CustomerData.CDPostCode))
                {
                    CustomerDataResultList.DmPostCode = "";
                }
                else
                {
                    CustomerDataResultList.DmPostCode = CustomerData.CDPostCode;
                }
                //Add 2015/07/01 arc nakayama #3220_顧客データリスト出力項目追加 DM発送先の顧客名
                if (!string.IsNullOrEmpty(CustomerData.FirstName) && !string.IsNullOrEmpty(CustomerData.LastName))
                {
                    CustomerDataResultList.DmCustomerName = CustomerData.FirstName + "　" + CustomerData.LastName + "　様";
                }
                else if (!string.IsNullOrEmpty(CustomerData.FirstName) && string.IsNullOrEmpty(CustomerData.LastName))
                {
                    CustomerDataResultList.DmCustomerName = CustomerData.FirstName + "　様";
                }
                CustomerDataResultList.DmPrefecture = CustomerData.CDPrefecture;    //都道府県(DM発送先住所)
                // Mod 2015/01/07 arc nakayama 顧客DM指摘事項③ 都道府県と住所をつなげる
                CustomerDataResultList.DmCustomerDmAddress = CustomerData.CDPrefecture + CustomerData.CDCity + CustomerData.CDAddress1 + CustomerData.CDAddress2;   //住所(DM発送先住所)
                CustomerDataResultList.DmTelNumber = CustomerData.CDTelNumber;      //電話番号(DM発送先住所)

                //Mod 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合
                //使用者と顧客が一致した場合のみ車両情報を設定する
                if (CustomerData.CustomerCode == CustomerData.UserCode)
                {
                    CustomerDataResultList.SalesCarNumber = CustomerData.SalesCarNumber;//管理番号(車両情報)
                    CustomerDataResultList.MorterViecleOfficialCode = CustomerData.MorterViecleOfficialCode + " " + CustomerData.RegistrationNumberType + " " + CustomerData.RegistrationNumberKana + " " + CustomerData.RegistrationNumberPlate; //車両登録番号(車両情報)
                    CustomerDataResultList.Vin = CustomerData.Vin;  //車台番号(車両情報)
                    CustomerDataResultList.MakerName = CustomerData.MakerName;  //メーカー(車両情報)
                    CustomerDataResultList.CarName = CustomerData.CarName;  //車種名称(車両情報)
                    CustomerDataResultList.DtFirstRegistrationDate = string.Format("{0:yyyy/MM}", CustomerData.FirstRegistrationDate);  //初年度登録(車両情報)
                    CustomerDataResultList.RegistrationDate = string.Format("{0:yyyy/MM/dd}", CustomerData.RegistrationDate);   //登録年月日(車両情報)
                    CustomerDataResultList.NextInspectionDate = string.Format("{0:yyyy/MM/dd}", CustomerData.NextInspectionDate); //次回点検日(車両情報)
                    CustomerDataResultList.ExpireDate = string.Format("{0:yyyy/MM/dd}", CustomerData.ExpireDate);               //車検満了日(車両情報)
                    //Add 2015/05/20 arc nakayama #3199_顧客データリスト出力項目追加
                    CustomerDataResultList.CarWeight = CustomerData.CarWeight;  // 車両重量(車両情報)
                }
                else
                {
                    CustomerDataResultList.SalesCarNumber = "";           //管理番号(車両情報)
                    CustomerDataResultList.MorterViecleOfficialCode = ""; //車両登録番号(車両情報)
                    CustomerDataResultList.Vin = "";                      //車台番号(車両情報)
                    CustomerDataResultList.MakerName = "";                //メーカー(車両情報)
                    CustomerDataResultList.CarName = "";                  //車種名称(車両情報)
                    CustomerDataResultList.DtFirstRegistrationDate = "";  //初年度登録(車両情報)
                    CustomerDataResultList.RegistrationDate = "";         //登録年月日(車両情報)
                    CustomerDataResultList.NextInspectionDate = "";       //次回点検日(車両情報)
                    CustomerDataResultList.ExpireDate = "";               //車検満了日(車両情報)
                    CustomerDataResultList.CarWeight = null;              //車両重量(車両情報)
                }


                CustomerDataResultList.SalesDepartmentName = CustomerData.DepartmentName;   //部門(営業担当)
                CustomerDataResultList.SalesEmployeeName = CustomerData.EmployeeName;       //担当者(営業担当)
                CustomerDataResultList.SalesDate = string.Format("{0:yyyy/MM/dd}", CustomerData.SalesDate);             //納車年月日(営業担当)
                CustomerDataResultList.ServiceDepartmentName = CustomerData.ServiceDepartmentName;  //部門(サービス担当)
                CustomerDataResultList.ServiceEmployeeName = CustomerData.ServiceEmployeeName;      //担当者(サービス担当)
                //Mod 2015/07/17 arc nakayama サービス伝票納車日を入庫日に変更（ServiceSalesDate ⇒ ArrivalPlanDate）
                CustomerDataResultList.ArrivalPlanDate = string.Format("{0:yyyy/MM/dd}", CustomerData.ArrivalPlanDate);        //納車日(サービス担当)

                //DM発送先住所が未登録(NULLまたは空文字)　だった場合は顧客住所を設定する
                //※郵便番号(DM発送先住所)が空文字またはNULL　かつ　都道府県(DM発送先住所)が空文字またはNULL　かつ　住所(DM発送先住所)が空文字またはNULL　の場合
                //Add 2015/01/07 arc nakayama 顧客DM指摘事項⑤ DM発送先住所の電話番号が空文字またはNULLなら顧客情報の電話番号を入れる
                //Mod 2015/07/01 arc nakayama #3220_顧客データリスト出力項目追加 DM発送先の顧客名
                //Mod 2015/07/09 arc nakayama #3220_顧客データリスト出力項目追加 DM発送先に顧客情報を置き換える条件を変更
                if (string.IsNullOrEmpty(CustomerData.DmDelFlag) || CustomerData.DmDelFlag.Equals("1"))
                {
                    CustomerDataResultList.DmCustomerName = CustomerDataResultList.CustomerName;            //顧客名(DM発送先住所)
                    CustomerDataResultList.DmTelNumber = CustomerDataResultList.CustomerTelNumber;          //電話番号(DM発送先住所)
                    CustomerDataResultList.DmPostCode = CustomerDataResultList.CustomerPostCode;            //郵便番号(DM発送先住所)
                    CustomerDataResultList.DmPrefecture = CustomerDataResultList.CustomerPrefecture;        //都道府県(DM発送先住所)
                    CustomerDataResultList.DmCustomerDmAddress = CustomerDataResultList.CustomerAddress;    //住所(DM発送先住所)
                }

                //リスト追加
                list.Add(CustomerDataResultList);
            }

            //Add 2018/04/25 arc yano #3842
            //顧客住所で絞込み
            if (!string.IsNullOrWhiteSpace(condition.CustomerAddress))
            {
                list = list.Where(x => ((x.DmPrefecture ?? "") + (x.DmCustomerDmAddress ?? "")).Contains(condition.CustomerAddress)).ToList();
            }

            return list;

        }


        /// <summary>
        /// 顧客車検案内リスト検索
        /// </summary>
        /// <param name="customerCondition">顧客車検案内リスト検索条件</param>
        /// <returns>顧客車検案内リスト検索結果</returns>
        /// <history>
        /// 2018/04/25 arc yano #3842 顧客データリスト（営業案内）　住所による絞込の追加
        /// </history>
        public PaginatedList<CustomerDataResult> GetInspectGuidListByCondition(CustomerDataSearch condition, int? pageIndex, int? pageSize)
        {
            
            var InspectGuidList = db.GetInspectGuidList(condition.InspectGuidFlag, //車検案内
                                                         condition.DepartmentCode2, //営業担当部門コード
                                                         condition.CarEmployeeCode, //営業部門担当者コード
                                                         condition.ServiceDepartmentCode2, //サービス担当部門コード
                                                         condition.MakerName, //メーカー名
                                                         condition.CarName,   //車種名
                                                         string.Format("{0:yyyy/MM/dd}", condition.FirstRegistrationFrom), //初年度登録From
                                                         string.Format("{0:yyyy/MM/dd}", condition.FirstRegistrationTo),   //初年度登録To
                                                         string.Format("{0:yyyy/MM/dd}", condition.RegistrationDateFrom),  //登録年月日From
                                                         string.Format("{0:yyyy/MM/dd}", condition.RegistrationDateTo),    //登録年月日To 
                                                         string.Format("{0:yyyy/MM/dd}", condition.NextInspectionDateFrom),//次回点検日From  
                                                         string.Format("{0:yyyy/MM/dd}", condition.NextInspectionDateTo),  //次回点検日To
                                                         string.Format("{0:yyyy/MM/dd}", condition.ExpireDateFrom),        //車検満了日From
                                                         string.Format("{0:yyyy/MM/dd}", condition.ExpireDateTo),          //車検満了日To
                                                         condition.CustomerAddressReconfirm, //住所再確認
                                                         condition.CustomerKind //顧客種別
                                                         );

           
            List<CustomerDataResult> list = new List<CustomerDataResult>();

            foreach (var CustomerData in InspectGuidList)
            {
                CustomerDataResult InspectGuidResultList = new CustomerDataResult();

                //営業(DM可否)　Nullなら空文字をセット
                if (string.IsNullOrEmpty(CustomerData.InspectGuidFlag))
                {
                    InspectGuidResultList.InspectGuidFlag = "";
                }
                else
                {
                    InspectGuidResultList.InspectGuidFlag = CustomerData.InspectGuidFlag;
                }

                //Add 2016/03/08 arc nakayama #3452_車検点検リスト項目追加(#3451_【大項目】車検活動リスト作成)
                //営業(DM可否)　Nullなら空文字をセット
                if (string.IsNullOrEmpty(CustomerData.InspectGuidFlag))
                {
                    InspectGuidResultList.DmFlag = "";
                }
                else
                {
                    InspectGuidResultList.DmFlag = CustomerData.DmFlag;
                }
                InspectGuidResultList.DmMemo = CustomerData.DmMemo;
                InspectGuidResultList.InspectGuidMemo = CustomerData.InspectGuidMemo;        //備考(DM可否)
                InspectGuidResultList.CustomerCode = CustomerData.CustomerCode;         //顧客コード(顧客情報)
                InspectGuidResultList.CustomerName = CustomerData.CustomerName + "　様";  //顧客名称(顧客情報)
                InspectGuidResultList.DepartmentName2 = CustomerData.DepartmentName2;  //営業担当部門名
                InspectGuidResultList.CarEmployeeName = CustomerData.CarEmployeeName;  //営業担当者名
                InspectGuidResultList.ServiceDepartmentName2 = CustomerData.ServiceDepartmentName2; //サービス担当部門名
                InspectGuidResultList.ServiceEmployeeName2 = CustomerData.ServiceEmployeeName2;     //サービス担当者名

                //郵便番号(顧客住所)　Nullなら空文字をセット
                if (string.IsNullOrEmpty(CustomerData.PostCode))
                {
                    InspectGuidResultList.CustomerPostCode = "";
                }
                else
                {
                    InspectGuidResultList.CustomerPostCode = CustomerData.PostCode;
                }
                InspectGuidResultList.CustomerPrefecture = CustomerData.Prefecture;//都道府県(顧客住所)
                InspectGuidResultList.CustomerAddress = CustomerData.Prefecture + CustomerData.City + CustomerData.Address1 + CustomerData.Address2;//住所(顧客住所)
                InspectGuidResultList.CustomerTelNumber = CustomerData.TelNumber;//電話番号(顧客住所)
                InspectGuidResultList.MobileNumber = CustomerData.MobileNumber;//携帯電話番号(顧客住所)
                //Add 2015/12/16 arc ookubo #3318_メールアドレスを追加
                if (string.IsNullOrEmpty(CustomerData.MailAddress))
                {
                    InspectGuidResultList.MailAddress = CustomerData.MobileMailAddress;
                } else {
                    InspectGuidResultList.MailAddress = CustomerData.MailAddress;
                }

                InspectGuidResultList.CustomerAddressReconfirmName = CustomerData.CustomerAddressReconfirmName;
                if (CustomerData.CustomerKind == null)
                {
                    InspectGuidResultList.CustomerKind = "";
                }
                else
                {
                    InspectGuidResultList.CustomerKind = CustomerData.CustomerKind; //顧客種別(顧客情報)
                }
                //郵便番号(DM発送先住所) Nullなら空文字をセット(「〒」を表示させないため)
                if (string.IsNullOrEmpty(CustomerData.CDPostCode))
                {
                    InspectGuidResultList.DmPostCode = "";
                }
                else
                {
                    InspectGuidResultList.DmPostCode = CustomerData.CDPostCode;
                }
                if (!string.IsNullOrEmpty(CustomerData.FirstName) && !string.IsNullOrEmpty(CustomerData.LastName))
                {
                    InspectGuidResultList.DmCustomerName = CustomerData.FirstName + "　" + CustomerData.LastName + "　様";
                }
                else if (!string.IsNullOrEmpty(CustomerData.FirstName) && string.IsNullOrEmpty(CustomerData.LastName))
                {
                    InspectGuidResultList.DmCustomerName = CustomerData.FirstName + "　様";
                }
                InspectGuidResultList.DmPrefecture = CustomerData.CDPrefecture;    //都道府県(DM発送先住所)
                InspectGuidResultList.DmCustomerDmAddress = CustomerData.CDPrefecture + CustomerData.CDCity + CustomerData.CDAddress1 + CustomerData.CDAddress2;   //住所(DM発送先住所)
                InspectGuidResultList.DmTelNumber = CustomerData.CDTelNumber;      //電話番号(DM発送先住所)
                InspectGuidResultList.SalesCarNumber = CustomerData.SalesCarNumber;//管理番号(車両情報)
                InspectGuidResultList.MorterViecleOfficialCode = CustomerData.MorterViecleOfficialCode + " " + CustomerData.RegistrationNumberType + " " + CustomerData.RegistrationNumberKana + " " + CustomerData.RegistrationNumberPlate;  //車両登録番号(車両情報)
                InspectGuidResultList.Vin = CustomerData.Vin;  //車台番号(車両情報)
                InspectGuidResultList.MakerName = CustomerData.MakerName;  //メーカー(車両情報)
                InspectGuidResultList.CarName = CustomerData.CarName;  //車種名称(車両情報)
                InspectGuidResultList.FirstRegistrationDate = CustomerData.FirstRegistrationDate;  //初年度登録(車両情報)
                InspectGuidResultList.RegistrationDate = CustomerData.RegistrationDate;   //登録年月日(車両情報)
                InspectGuidResultList.NextInspectionDate = CustomerData.NextInspectionDate; //次回点検日(車両情報)
                InspectGuidResultList.ExpireDate = CustomerData.ExpireDate;               //車検満了日(車両情報)
                InspectGuidResultList.CarWeight = CustomerData.CarWeight;  // 車両重量(車両情報)
                //Add 2016/03/08 arc nakayama #3452_車検点検リスト項目追加(#3451_【大項目】車検活動リスト作成)
                InspectGuidResultList.CarGradeName = CustomerData.CarGradeName; //グレード名
                InspectGuidResultList.LocationName = CustomerData.LocationName; //車両在庫ロケーション
                InspectGuidResultList.SalesDate = CustomerData.SalesDate; //納車日
                InspectGuidResultList.SalesOrderStatusName = CustomerData.SalesOrderStatusName; //車両伝票ステータス
                InspectGuidResultList.ArrivalPlanDate = CustomerData.ArrivalPlanDate; //最終入庫日
                InspectGuidResultList.ServiceDepartmentName3 = CustomerData.ServiceDepartmentName3; //最終入庫ロケーション

                //DM発送先住所が未登録(NULLまたは空文字)　だった場合は顧客住所を設定する
                if (string.IsNullOrEmpty(CustomerData.DmDelFlag) || CustomerData.DmDelFlag.Equals("1"))
                {
                    InspectGuidResultList.DmCustomerName = InspectGuidResultList.CustomerName;            //顧客名(DM発送先住所)
                    InspectGuidResultList.DmTelNumber = InspectGuidResultList.CustomerTelNumber;          //電話番号(DM発送先住所)
                    InspectGuidResultList.DmPostCode = InspectGuidResultList.CustomerPostCode;            //郵便番号(DM発送先住所)
                    InspectGuidResultList.DmPrefecture = InspectGuidResultList.CustomerPrefecture;        //都道府県(DM発送先住所)
                    InspectGuidResultList.DmCustomerDmAddress = InspectGuidResultList.CustomerAddress;    //住所(DM発送先住所)
                }

                //リスト追加
                list.Add(InspectGuidResultList);
            }

            //Add 2018/04/25 arc yano #3842
            //顧客住所で絞込み
            if (!string.IsNullOrWhiteSpace(condition.CustomerAddress))
            {
                list = list.Where(x => ((x.DmPrefecture ?? "") + (x.DmCustomerDmAddress ?? "")).Contains(condition.CustomerAddress)).ToList();
            }

            return new PaginatedList<CustomerDataResult>(list.AsQueryable<CustomerDataResult>(), pageIndex ?? 0, pageSize ?? 0);

        }


        /// <summary>
        /// 顧客車検案内リスト検索(Excel出力用)
        /// </summary>
        /// <param name="customerCondition">顧客車検案内リスト検索条件</param>
        /// <returns>顧客車検案内リスト検索結果</returns>
        /// <history>
        /// 2018/04/25 arc yano #3842 顧客データリスト（営業案内）　住所による絞込の追加
        /// </history>
        public List<InspectGuidDataExcelResult> GetInspectGuidListByConditionForExcel(DocumentExportCondition condition)
        {
            var InspectGuidList = db.GetInspectGuidList(condition.InspectGuidFlag,
                                                         condition.DepartmentCode2,
                                                         condition.CarEmployeeCode,
                                                         condition.ServiceDepartmentCode2,
                                                         condition.MakerName,
                                                         condition.CarName,
                                                         string.Format("{0:yyyy/MM/dd}", condition.DtFirstRegistrationFrom),
                                                         string.Format("{0:yyyy/MM/dd}", condition.DtFirstRegistrationTo),
                                                         string.Format("{0:yyyy/MM/dd}", condition.RegistrationDateFrom),
                                                         string.Format("{0:yyyy/MM/dd}", condition.RegistrationDateTo),
                                                         string.Format("{0:yyyy/MM/dd}", condition.NextInspectionDateFrom),
                                                         string.Format("{0:yyyy/MM/dd}", condition.NextInspectionDateTo),
                                                         string.Format("{0:yyyy/MM/dd}", condition.ExpireDateFromForDm),
                                                         string.Format("{0:yyyy/MM/dd}", condition.ExpireDateToForDm),
                                                         condition.CustomerAddressReconfirm,
                                                         condition.CustomerKind
                                                         );

           
            List<InspectGuidDataExcelResult> list = new List<InspectGuidDataExcelResult>();
            CodeDao dao = new CodeDao(db);

            foreach (var CustomerData in InspectGuidList)
            {
                InspectGuidDataExcelResult InspectGuidResultList = new InspectGuidDataExcelResult();
                //Add 2016/03/08 arc nakayama #3452_車検点検リスト項目追加(#3451_【大項目】車検活動リスト作成)
                InspectGuidResultList.DmFlagName = CustomerData.DmFlagName;
                InspectGuidResultList.DmMemo = CustomerData.DmMemo;
                InspectGuidResultList.InspectGuidFlagName = CustomerData.InspectGuidFlagName;
                InspectGuidResultList.InspectGuidMemo = CustomerData.InspectGuidMemo;        //備考(DM可否)
                InspectGuidResultList.CustomerCode = CustomerData.CustomerCode;         //顧客コード(顧客情報)
                InspectGuidResultList.CustomerName = CustomerData.CustomerName + "　様";  //顧客名称(顧客情報)
                InspectGuidResultList.DepartmentName2 = CustomerData.DepartmentName2;  //営業担当部門名
                InspectGuidResultList.CarEmployeeName = CustomerData.CarEmployeeName;  //営業担当者名
                InspectGuidResultList.ServiceDepartmentName2 = CustomerData.ServiceDepartmentName2; //サービス担当部門名
                InspectGuidResultList.ServiceEmployeeName2 = CustomerData.ServiceEmployeeName2;     //サービス担当者名
                //郵便番号(顧客住所)　Nullなら空文字をセット(「〒」を表示させないため)
                if (string.IsNullOrEmpty(CustomerData.PostCode))
                {
                    InspectGuidResultList.CustomerPostCode = "";
                }
                else
                {
                    InspectGuidResultList.CustomerPostCode = CustomerData.PostCode;
                }
                InspectGuidResultList.CustomerPrefecture = CustomerData.Prefecture;//都道府県(顧客住所)
                InspectGuidResultList.CustomerAddress = CustomerData.Prefecture + CustomerData.City + CustomerData.Address1 + CustomerData.Address2;//住所(顧客住所)
                InspectGuidResultList.CustomerTelNumber = CustomerData.TelNumber;//電話番号(顧客住所)
                InspectGuidResultList.MobileNumber = CustomerData.MobileNumber;//電話番号(顧客住所)
                //Add 2015/12/16 arc ookubo #3318_メールアドレスを追加
                if (string.IsNullOrEmpty(CustomerData.MailAddress))
                {
                    InspectGuidResultList.MailAddress = CustomerData.MobileMailAddress;
                } else {
                    InspectGuidResultList.MailAddress = CustomerData.MailAddress;
                }
                InspectGuidResultList.CustomerAddressReconfirmName = CustomerData.CustomerAddressReconfirmName;
                InspectGuidResultList.CustomerKindName = CustomerData.CustomerKindName;

                //郵便番号(DM発送先住所) Nullなら空文字をセット(「〒」を表示させないため)
                if (string.IsNullOrEmpty(CustomerData.CDPostCode))
                {
                    InspectGuidResultList.DmPostCode = "";
                }
                else
                {
                    InspectGuidResultList.DmPostCode = CustomerData.CDPostCode;
                }
                if (!string.IsNullOrEmpty(CustomerData.FirstName) && !string.IsNullOrEmpty(CustomerData.LastName))
                {
                    InspectGuidResultList.DmCustomerName = CustomerData.FirstName + "　" + CustomerData.LastName + "　様";
                }
                else if (!string.IsNullOrEmpty(CustomerData.FirstName) && string.IsNullOrEmpty(CustomerData.LastName))
                {
                    InspectGuidResultList.DmCustomerName = CustomerData.FirstName + "　様";
                }
                InspectGuidResultList.DmPrefecture = CustomerData.CDPrefecture;    //都道府県(DM発送先住所)
                InspectGuidResultList.DmCustomerDmAddress = CustomerData.CDPrefecture + CustomerData.CDCity + CustomerData.CDAddress1 + CustomerData.CDAddress2;   //住所(DM発送先住所)
                InspectGuidResultList.DmTelNumber = CustomerData.CDTelNumber;      //電話番号(DM発送先住所)
                InspectGuidResultList.SalesCarNumber = CustomerData.SalesCarNumber;//管理番号(車両情報)
                InspectGuidResultList.MorterViecleOfficialCode = CustomerData.MorterViecleOfficialCode + " " + CustomerData.RegistrationNumberType + " " + CustomerData.RegistrationNumberKana + " " + CustomerData.RegistrationNumberPlate; //車両登録番号(車両情報)
                InspectGuidResultList.Vin = CustomerData.Vin;  //車台番号(車両情報)
                InspectGuidResultList.MakerName = CustomerData.MakerName;  //メーカー(車両情報)
                InspectGuidResultList.CarName = CustomerData.CarName;  //車種名称(車両情報)
                InspectGuidResultList.DtFirstRegistrationDate = string.Format("{0:yyyy/MM}", CustomerData.FirstRegistrationDate);  //初年度登録(車両情報)
                InspectGuidResultList.RegistrationDate = string.Format("{0:yyyy/MM/dd}", CustomerData.RegistrationDate);   //登録年月日(車両情報)
                InspectGuidResultList.NextInspectionDate = string.Format("{0:yyyy/MM/dd}", CustomerData.NextInspectionDate); //次回点検日(車両情報)
                InspectGuidResultList.ExpireDate = string.Format("{0:yyyy/MM/dd}", CustomerData.ExpireDate);               //車検満了日(車両情報)
                InspectGuidResultList.CarWeight = CustomerData.CarWeight;  // 車両重量(車両情報)
	                //Add 2016/03/08 arc nakayama #3452_車検点検リスト項目追加(#3451_【大項目】車検活動リスト作成)
                InspectGuidResultList.CarGradeName = CustomerData.CarGradeName; //グレード名
                InspectGuidResultList.LocationName = CustomerData.LocationName; //車両在庫ロケーション
                InspectGuidResultList.SalesDate = string.Format("{0:yyyy/MM/dd}",CustomerData.SalesDate); //納車日
                InspectGuidResultList.SalesOrderStatusName = CustomerData.SalesOrderStatusName; //車両伝票ステータス
                InspectGuidResultList.ArrivalPlanDate = string.Format("{0:yyyy/MM/dd}",CustomerData.ArrivalPlanDate); //最終入庫日
                InspectGuidResultList.ServiceDepartmentName3 = CustomerData.ServiceDepartmentName3; //最終入庫ロケーション

                //DM発送先住所が未登録(NULLまたは空文字)　だった場合は顧客住所を設定する
                if (string.IsNullOrEmpty(CustomerData.DmDelFlag) || CustomerData.DmDelFlag.Equals("1"))
                {
                    InspectGuidResultList.DmCustomerName = InspectGuidResultList.CustomerName;            //顧客名(DM発送先住所)
                    InspectGuidResultList.DmTelNumber = InspectGuidResultList.CustomerTelNumber;          //電話番号(DM発送先住所)
                    InspectGuidResultList.DmPostCode = InspectGuidResultList.CustomerPostCode;            //郵便番号(DM発送先住所)
                    InspectGuidResultList.DmPrefecture = InspectGuidResultList.CustomerPrefecture;        //都道府県(DM発送先住所)
                    InspectGuidResultList.DmCustomerDmAddress = InspectGuidResultList.CustomerAddress;    //住所(DM発送先住所)
                }

                //リスト追加
                list.Add(InspectGuidResultList);
            }

            //Add 2018/04/25 arc yano #3842
            //顧客住所で絞込み
            if (!string.IsNullOrWhiteSpace(condition.CustomerAddress))
            {
                list = list.Where(x => ((x.DmPrefecture ?? "") + (x.DmCustomerDmAddress ?? "")).Contains(condition.CustomerAddress)).ToList();
            }

            return list;

        }

        /// <summary>
        /// 名寄せツール（顧客情報取得）
        /// </summary>
        /// <param name="CustomerCord">顧客コード</param>
        /// <returns>名寄せツール用顧客情報取得結果</returns>
        public GetCustomerIntegrateDataResult GetCustomerIntegrateData(string CustomerCord)
        {
            var Ret = db.GetCustomerIntegrateData(CustomerCord).FirstOrDefault();

            return Ret;
        }
    }
}
