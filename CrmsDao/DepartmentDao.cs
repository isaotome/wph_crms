using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Linq.Expressions;

namespace CrmsDao {

    /// <summary>
    /// 部門マスタアクセスクラス
    ///   部門マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class DepartmentDao {
    
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public DepartmentDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        //Mod 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② closeMonthFlagを追加
        /// <summary>
        /// 部門マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <returns>部門マスタデータ(1件)</returns>
        public Department GetByKey(string departmentCode, bool includeDeleted = false, string closeMonthFlag = "")
        {
            Department departmentCondition = new Department();
            departmentCondition.DepartmentCode = departmentCode;
            //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため)
            return GetByKey(departmentCondition, includeDeleted, closeMonthFlag);
        }

        //Mod 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② closeMonthFlagを追加
        /// <summary>
        /// 部門マスタデータ取得(PK及び権限指定)
        /// </summary>
        /// <param name="departmentCondition">検索条件(PK及び権限情報)</param>
        /// <returns>部門マスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため)
        public Department GetByKey(Department departmentCondition, bool includeDeleted = false, string closeMonthFlag = "")
        {

            string departmentCode = departmentCondition.DepartmentCode;

            // 部門データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            var query =
                from a in db.Department
                where (a.DepartmentCode.Equals(departmentCode))
                && ((includeDeleted) || a.DelFlag.Equals("0"))
                && ((string.IsNullOrWhiteSpace(closeMonthFlag)) || a.CloseMonthFlag.Equals(closeMonthFlag))
                select a;

           /* ParameterExpression param = Expression.Parameter(typeof(Department), "x");
            Expression depExpression = departmentCondition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<Department, bool>>(depExpression, param));
            }
            Expression offExpression = departmentCondition.CreateExpressionForOffice(param, new string[] { "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<Department, bool>>(offExpression, param));
            }
            Expression comExpression = departmentCondition.CreateExpressionForCompany(param, new string[] { "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<Department, bool>>(comExpression, param));
            }
            */
            Department department = query.FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (department != null) {
                department = EditModel(department);
            }

            // 部門データの返却
            return department;
        }

        /// <summary>
        /// 部門マスタデータ検索
        /// </summary>
        /// <param name="DepartmentCondition">部門検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <param name="searchIsNot">検索方法の指定(true=Equal検索、false:NotEqual検索)</param>
        /// <returns>部門マスタデータ検索結果</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両棚卸フラグ、部品棚卸フラグによる絞り込みを追加
        /// 2015/06/11 arc yano その他 GetListByCondition, GetListByCondition2統合[デフォルト引数追加(Not検索指定)、CloseMonthFlag検索]
        /// </history>
        public PaginatedList<Department> GetListByCondition(Department departmentCondition, int? pageIndex, int? pageSize, bool searchIsNot = false) {

            

            string departmentCode = departmentCondition.DepartmentCode;
            string departmentName = departmentCondition.DepartmentName;
            string officeCode = null;
            try { officeCode = departmentCondition.Office.OfficeCode; } catch (NullReferenceException) { }
            string officeName = null;
            try { officeName = departmentCondition.Office.OfficeName; } catch (NullReferenceException) { }
            string companyCode = null;
            try { companyCode = departmentCondition.Office.Company.CompanyCode; } catch (NullReferenceException) { }
            string companyName = null;
            try { companyName = departmentCondition.Office.Company.CompanyName; } catch (NullReferenceException) { }
            string employeeName = null;
            try { employeeName = departmentCondition.Employee.EmployeeName; } catch (NullReferenceException) { }
            string delFlag = departmentCondition.DelFlag;
            string businessType = departmentCondition.BusinessType;

            string carInventoryFlag = departmentCondition.CarInventoryFlag;
            string partsInventoryFlag = departmentCondition.PartsInventoryFlag;




            IOrderedQueryable<Department> OdepartmentList;
            string closeMonthFlag = departmentCondition.CloseMonthFlag;     //Mod 2015/06/11

            // 部門データの取得
            var  departmentList =
                    from a in db.Department
                    where (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Contains(departmentCode))
                    && (string.IsNullOrEmpty(departmentName) || a.DepartmentName.Contains(departmentName))
                    && (string.IsNullOrEmpty(officeCode) || a.OfficeCode.Contains(officeCode))
                    && (string.IsNullOrEmpty(officeName) || a.Office.OfficeName.Contains(officeName))
                    && (string.IsNullOrEmpty(companyCode) || a.Office.CompanyCode.Contains(companyCode))
                    && (string.IsNullOrEmpty(companyName) || a.Office.Company.CompanyName.Contains(companyName))
                    && (string.IsNullOrEmpty(employeeName) || a.Employee.EmployeeName.Contains(employeeName))
                    && (string.IsNullOrEmpty(businessType) || a.BusinessType.Equals(businessType))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    && (string.IsNullOrEmpty(carInventoryFlag) || a.CarInventoryFlag.Equals(carInventoryFlag))
                    && (string.IsNullOrEmpty(partsInventoryFlag) || a.PartsInventoryFlag.Equals(partsInventoryFlag))
                    //orderby a.Office.CompanyCode, a.OfficeCode, a.DepartmentCode
                    select a;

            //Mod 2015/06/11
            if (!string.IsNullOrWhiteSpace(closeMonthFlag))     //締め処理状況表示フラグがnull、空文字でない場合
            {
                if (searchIsNot == true)
                {
                    departmentList = departmentList.Where(x => !x.CloseMonthFlag.Equals(closeMonthFlag));    //Not Equal検索
                }
                else
                {
                    departmentList = departmentList.Where(x => x.CloseMonthFlag.Equals(closeMonthFlag));     //Equal検索
                }
            }

            OdepartmentList = departmentList.OrderBy(x => x.Office.CompanyCode).ThenBy(x => x.OfficeCode).ThenBy(x => x.DepartmentCode);
           
            // ページング制御情報を付与した部門データの返却
            PaginatedList<Department> ret = new PaginatedList<Department>(OdepartmentList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++) {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }

        //Mod 2015/06/11 arc yano その他 GetListByCondition, GetListByCondition2統合のため、GetListByCondition2はコメントアウト
        //Mod 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② closeMonthFlagが設定されていた場合は、設定された値を検索条件とするように修正する
        //Add 2014/09/08 arc yano IPO対応その２ 月締め処理状況画面用の検索処理追加
        /*
        /// <summary>
        /// 部門マスタデータ検索(月締め処理状況画面専用)
        /// </summary>
        /// <param name="DepartmentCondition">部門検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>部門マスタデータ検索結果</returns>
        public PaginatedList<Department> GetListByCondition2(Department departmentCondition, int? pageIndex, int? pageSize)
        {

            string departmentCode = departmentCondition.DepartmentCode;
            string departmentName = departmentCondition.DepartmentName;
            string officeCode = null;
            try { officeCode = departmentCondition.Office.OfficeCode; }
            catch (NullReferenceException) { }
            string officeName = null;
            try { officeName = departmentCondition.Office.OfficeName; }
            catch (NullReferenceException) { }
            string companyCode = null;
            try { companyCode = departmentCondition.Office.Company.CompanyCode; }
            catch (NullReferenceException) { }
            string companyName = null;
            try { companyName = departmentCondition.Office.Company.CompanyName; }
            catch (NullReferenceException) { }
            string employeeName = null;
            try { employeeName = departmentCondition.Employee.EmployeeName; }
            catch (NullReferenceException) { }
            string delFlag = departmentCondition.DelFlag;
            string businessType = departmentCondition.BusinessType;

            //Mod 2015/05/27 arc yano
            string closeMonthFlag = departmentCondition.CloseMonthFlag;

            // 部門データの取得
            IOrderedQueryable<Department> departmentList =
                    from a in db.Department
                    where (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Contains(departmentCode))
                    && (string.IsNullOrEmpty(departmentName) || a.DepartmentName.Contains(departmentName))
                    && (string.IsNullOrEmpty(officeCode) || a.OfficeCode.Contains(officeCode))
                    && (string.IsNullOrEmpty(officeName) || a.Office.OfficeName.Contains(officeName))
                    && (string.IsNullOrEmpty(companyCode) || a.Office.CompanyCode.Contains(companyCode))
                    && (string.IsNullOrEmpty(companyName) || a.Office.Company.CompanyName.Contains(companyName))
                    && (string.IsNullOrEmpty(employeeName) || a.Employee.EmployeeName.Contains(employeeName))
                    && (string.IsNullOrEmpty(businessType) || a.BusinessType.Equals(businessType))
                    && ((a.CloseMonthFlag != null && a.CloseMonthFlag.Equals(closeMonthFlag)))	//Mod 2015/05/27 arc yano
                    orderby a.Office.CompanyCode, a.OfficeCode, a.DepartmentCode
                    select a;

            // ページング制御情報を付与した部門データの返却
            PaginatedList<Department> ret = new PaginatedList<Department>(departmentList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }
        */

        //Add 2014/10/16 arc amii サブシステム仕訳機能移行対応 車両仕入リスト専用の検索ダイアログ追加
        /// <summary>
        /// 部門マスタデータ検索
        /// </summary>
        /// <param name="DepartmentCondition">部門検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>部門マスタデータ検索結果</returns>
        /// <history>
        /// 2018/04/05 arc yano #3878 車両用途変更　ロケーションマスタのダイアログにJLR店舗のロケーションが表示されない
        /// </history>
        public PaginatedList<Department> GetListByCondition3(Department departmentCondition, int? pageIndex, int? pageSize)
        {

            string departmentCode = departmentCondition.DepartmentCode;
            string departmentName = departmentCondition.DepartmentName;
            string officeCode = null;
            try { officeCode = departmentCondition.Office.OfficeCode; }
            catch (NullReferenceException) { }
            string officeName = null;
            try { officeName = departmentCondition.Office.OfficeName; }
            catch (NullReferenceException) { }
            string companyCode = null;
            try { companyCode = departmentCondition.Office.Company.CompanyCode; }
            catch (NullReferenceException) { }
            string companyName = null;
            try { companyName = departmentCondition.Office.Company.CompanyName; }
            catch (NullReferenceException) { }
            string employeeName = null;
            try { employeeName = departmentCondition.Employee.EmployeeName; }
            catch (NullReferenceException) { }
            string delFlag = departmentCondition.DelFlag;
            string businessType = departmentCondition.BusinessType;


            // 部門データの取得
            // Mod 2018/04/05 arc yano #3878
            IOrderedQueryable<Department> departmentList =
                    from a in db.Department
                    where
                       (a.BrandStoreCode != null && !a.BrandStoreCode.Equals("") && !a.BrandStoreCode.Equals("0"))
                    && (a.CloseMonthFlag != null && a.CloseMonthFlag.Equals("1"))
                    && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Contains(departmentCode))
                    && (string.IsNullOrEmpty(departmentName) || a.DepartmentName.Contains(departmentName))
                    && (string.IsNullOrEmpty(officeCode) || a.OfficeCode.Contains(officeCode))
                    && (string.IsNullOrEmpty(officeName) || a.Office.OfficeName.Contains(officeName))
                    && (string.IsNullOrEmpty(companyCode) || a.Office.CompanyCode.Contains(companyCode))
                    && (string.IsNullOrEmpty(companyName) || a.Office.Company.CompanyName.Contains(companyName))
                    && (string.IsNullOrEmpty(employeeName) || a.Employee.EmployeeName.Contains(employeeName))
                    && (string.IsNullOrEmpty(businessType) || a.BusinessType.Equals(businessType))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.Office.CompanyCode, a.OfficeCode, a.DepartmentCode
                    select a;
                    /*
                    where
                       (a.BrandStoreCode.Equals("1") || a.BrandStoreCode.Equals("2"))
                    && a.DepartmentCode.EndsWith("1")
                    && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Contains(departmentCode))
                    && (string.IsNullOrEmpty(departmentName) || a.DepartmentName.Contains(departmentName))
                    && (string.IsNullOrEmpty(officeCode) || a.OfficeCode.Contains(officeCode))
                    && (string.IsNullOrEmpty(officeName) || a.Office.OfficeName.Contains(officeName))
                    && (string.IsNullOrEmpty(companyCode) || a.Office.CompanyCode.Contains(companyCode))
                    && (string.IsNullOrEmpty(companyName) || a.Office.Company.CompanyName.Contains(companyName))
                    && (string.IsNullOrEmpty(employeeName) || a.Employee.EmployeeName.Contains(employeeName))
                    && (string.IsNullOrEmpty(businessType) || a.BusinessType.Equals(businessType))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.Office.CompanyCode, a.OfficeCode, a.DepartmentCode
                    select a;
                    */

                     
            // ページング制御情報を付与した部門データの返却
            PaginatedList<Department> ret = new PaginatedList<Department>(departmentList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }

        /// <summary>
        /// 部門マスタデータ検索(サービス部門のみ)
        /// </summary>
        /// <param name="DepartmentCondition">部門検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>部門マスタデータ検索結果(サービス部門のみ)</returns>
        public PaginatedList<Department> GetListByConditionForParts(Department departmentCondition, int? pageIndex, int? pageSize)
        {

            string departmentCode = departmentCondition.DepartmentCode;
            string departmentName = departmentCondition.DepartmentName;
            string officeCode = null;
            try { officeCode = departmentCondition.Office.OfficeCode; }
            catch (NullReferenceException) { }
            string officeName = null;
            try { officeName = departmentCondition.Office.OfficeName; }
            catch (NullReferenceException) { }
            string companyCode = null;
            try { companyCode = departmentCondition.Office.Company.CompanyCode; }
            catch (NullReferenceException) { }
            string companyName = null;
            try { companyName = departmentCondition.Office.Company.CompanyName; }
            catch (NullReferenceException) { }
            string employeeName = null;
            try { employeeName = departmentCondition.Employee.EmployeeName; }
            catch (NullReferenceException) { }
            string delFlag = departmentCondition.DelFlag;
            string businessType = departmentCondition.BusinessType;

            // 部門データの取得
            IOrderedQueryable<Department> departmentList =
                    from a in db.Department
                    where (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Contains(departmentCode))
                    && (string.IsNullOrEmpty(departmentName) || a.DepartmentName.Contains(departmentName))
                    && (string.IsNullOrEmpty(officeCode) || a.OfficeCode.Contains(officeCode))
                    && (string.IsNullOrEmpty(officeName) || a.Office.OfficeName.Contains(officeName))
                    && (string.IsNullOrEmpty(companyCode) || a.Office.CompanyCode.Contains(companyCode))
                    && (string.IsNullOrEmpty(companyName) || a.Office.Company.CompanyName.Contains(companyName))
                    && (string.IsNullOrEmpty(employeeName) || a.Employee.EmployeeName.Contains(employeeName))
                    && (string.IsNullOrEmpty(businessType) || a.BusinessType.Equals(businessType))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    && (a.BusinessType.Equals("002"))
                    && (a.CloseMonthFlag != "1" || a.CloseMonthFlag == null )
                    orderby a.Office.CompanyCode, a.OfficeCode, a.DepartmentCode
                    select a;

            // ページング制御情報を付与した部門データの返却
            PaginatedList<Department> ret = new PaginatedList<Department>(departmentList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }


        /// <summary>
        /// 閲覧可能な部門リストを取得する
        /// </summary>
        /// <param name="auth">権限設定データ</param>
        /// <returns>部門リスト</returns>
        public List<Department> GetListByAuthCondition(CrmsAuth auth) {
            var query =
                from a in db.Department
                where (string.IsNullOrEmpty(auth.AuthCompanyCode) || a.Office.CompanyCode.Equals(auth.AuthCompanyCode))
                select a;

            ParameterExpression param = Expression.Parameter(typeof(Department), "x");
            Expression depExpression = auth.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<Department, bool>>(depExpression, param));
            }
            Expression offExpression = auth.CreateExpressionForOffice(param, new string[] { "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<Department, bool>>(offExpression, param));
            }
            Expression comExpression = auth.CreateExpressionForCompany(param, new string[] { "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<Department, bool>>(comExpression, param));
            }

            return query.OrderBy(x => x.DepartmentCode).ToList();
        }
        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="department">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Department EditModel(Department department) {

            // 内部コード項目の名称情報取得
            department.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, department.DelFlag);

            // 出口
            return department;
        }

        /// <summary>
        /// 全部門を取得する
        /// </summary>
        /// <returns>部門データリスト</returns>
        public List<Department> GetListAll() {
            IQueryable<Department> query =
                from a in db.Department
                orderby a.DepartmentCode
                where a.DelFlag.Equals("0")
                select a;
            return query.ToList<Department>();
        }

        //Mod 2014/12/16 arc yano IPO対応(部品検索) CloseMonthFlag が 1または2のものを取得するように変更。
        //Add 2014/09/05 arc yano IPO対応 CloseMonthFlag が 1でないものを取得する。
        /// <summary>
        /// 全部門を取得する(CloseMonthFlagが0でないものを取得する。)
        /// </summary>
        /// <returns>部門データリスト</returns>
        public List<Department> GetListAllCloseMonthFlag()
        {
            IQueryable<Department> query =
                from a in db.Department
                orderby a.DepartmentCode
                where ((!string.IsNullOrEmpty(a.CloseMonthFlag)) && (a.CloseMonthFlag.Equals("1") || a.CloseMonthFlag.Equals("2")))
                select a;
            return query.ToList<Department>();
        }

        public List<Department> GetListAll(string businessType) {
            var query = from a in db.Department
                        orderby a.DepartmentCode
                        where a.DelFlag.Equals("0")
                        && (string.IsNullOrEmpty(businessType) || a.BusinessType.Equals(businessType))
                        select a;
            return query.ToList<Department>();
        }

        /// <summary>
        /// 部門マスタデータ取得(PK及び権限指定)
        /// </summary>
        /// <param name="departmentCondition">検索条件(PK及び権限情報)</param>
        /// <returns>部門マスタデータ(1件)</returns>
        public List<Department> GetListByKey(string departmentcode)
        {

            // 部門データの取得
            var query =
                from a in db.Department
                where (a.DepartmentCode.Equals(departmentcode))
                select a;


            // 部門データの返却
            return query.ToList<Department>();
        }

        /// <summary>
        /// 部門マスタデータ取得(PK及び権限指定)
        /// </summary>
        /// <param name="departmentCondition">検索条件(PK及び権限情報)</param>
        /// <returns>部門マスタデータ(1件)</returns>
        /// <history>
        /// 2018/04/05 arc yano  #3878 車両用途変更　ロケーションマスタのダイアログにJLR店舗のロケーションが表示されない
        /// </history>
        public Department GetByCarDepartment(string departmentCode)
        {
            // 車両の部門データのみ取得
            // Mod 2018/04/05 arc yano #3878
            var query =
                from a in db.Department
                where (a.BrandStoreCode != null && !a.BrandStoreCode.Equals("") && !a.BrandStoreCode.Equals("0"))
                    && a.CloseMonthFlag.Equals("1")
                    && a.DelFlag.Equals("0")
                    && (a.DepartmentCode.Equals(departmentCode))
                select a;
            /*
              var query =
                from a in db.Department
                where (a.BrandStoreCode.Equals("1") || a.BrandStoreCode.Equals("2"))
                    && a.DepartmentCode.EndsWith("1")
                    && a.DelFlag.Equals("0")
                    && (a.DepartmentCode.Equals(departmentCode))
                select a;
             */

            Department department = query.FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (department != null)
            {
                department = EditModel(department);
            }

            // 部門データの返却
            return department;
        }
        

        /// <summary>
        /// LinkEntryの顧客コードによる部門の絞込み
        /// </summary>
        /// <param name="leUserCode">LinkEntryの顧客コード</param>
        /// <returns>部門マスタデータ(1件)</returns>
        /// <history>
        /// Add 2016/03/03 arc yano #3413 部品マスタ メーカー部品番号の重複
        /// </history>
        public Department GetByLEUserCode(string leUserCode)
        {

            // 部門データの取得
            var query =
                from a in db.Department
                where a.LEUserCode.Equals(leUserCode)
                    && a.DelFlag.Equals("0")
                select a;

            Department department = query.FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (department != null)
            {
                department = EditModel(department);
            }

            // 部門データの返却
            return department;
        }


        /// <summary>
        ///　車両棚卸対象の部門を取得する
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="carinventoryFlag">車両棚卸フラグ</param>
        /// <returns>部門マスタデータ(1件)</returns>
        public Department GetByCarInventory(string departmentCode)
        {

            // 部門データの取得
            var query =
                from a in db.Department
                where
                     a.DepartmentCode.Equals(departmentCode)
                  && a.CarInventoryFlag.Equals("1")
                    && a.DelFlag.Equals("0")
                select a;

            Department department = query.FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (department != null)
            {
                department = EditModel(department);
            }

            // 部門データの返却
            return department;
        }

        /// <summary>
        ///　部品棚卸対象の部門を取得する
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="carinventoryFlag">車両棚卸フラグ</param>
        /// <returns>部門マスタデータ(1件)</returns>
        public Department GetByPartsInventory(string departmentCode)
        {

            // 部門データの取得
            var query =
                from a in db.Department
                where
                     a.DepartmentCode.Equals(departmentCode)
                  && a.PartsInventoryFlag.Equals("1")
                    && a.DelFlag.Equals("0")
                select a;

            Department department = query.FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (department != null)
            {
                department = EditModel(department);
            }

            // 部門データの返却
            return department;
        }

    }
}
