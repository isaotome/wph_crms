using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Linq.Expressions;

namespace CrmsDao
{
    /// <summary>
    /// 事業所マスタアクセスクラス
    ///   事業所マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class OfficeDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public OfficeDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 事業所マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="OfficeCode">事業所コード</param>
        /// <returns>事業所マスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため)
        public Office GetByKey(string officeCode, bool includeDeleted = false)
        {
            // 事業所データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            Office office =
                (from a in db.Office
                 where a.OfficeCode.Equals(officeCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (office != null)
            {
                office = EditModel(office);
            }

            // 事業所データの返却
            return office;
        }

        //Add 2014/10/27 arc amii サブシステム仕訳機能移行対応 勘定奉行データ出力画面専用の検索ダイアログ追加
        /// <summary>
        /// 事業所マスタデータ取得(PK + 拠点指定)
        /// </summary>
        /// <param name="OfficeCode">事業所コード</param>
        /// <returns>事業所マスタデータ(1件)</returns>
        public Office GetByDivisionKey(string officeCode, string divisionType)
        {
            // 事業所データの取得
            Office office =
                (from a in db.Office
                 where a.OfficeCode.Equals(officeCode)
                 && a.DelFlag.Equals("0")
                 && (a.OfficeCode.StartsWith(divisionType))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (office != null)
            {
                office = EditModel(office);
            }

            // 事業所データの返却
            return office;
        }

        /// <summary>
        /// 事業所マスタデータ取得(PK及び権限指定)
        /// </summary>
        /// <param name="officeCondition">検索条件(PK及び権限情報)</param>
        /// <returns>事業所マスタデータ(1件)</returns>
        public Office GetByKey(Office officeCondition) {

            string officeCode = officeCondition.OfficeCode;

            // 事業所データの取得
            var query =
                from a in db.Office
                where (a.OfficeCode.Equals(officeCode))
                select a;

            ParameterExpression param = Expression.Parameter(typeof(Office), "x");
            Expression offExpression = officeCondition.CreateExpressionForOffice(param, new string[] { "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<Office, bool>>(offExpression, param));
            }
            Expression comExpression = officeCondition.CreateExpressionForCompany(param, new string[] { "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<Office, bool>>(comExpression, param));
            }

            Office office = query.FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (office != null) {
                office = EditModel(office);
            }

            // 事業所データの返却
            return office;
        }
        /// <summary>
        /// 事業所マスタデータ検索
        /// </summary>
        /// <param name="OfficeCondition">事業所検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>事業所マスタデータ検索結果</returns>
        public PaginatedList<Office> GetListByCondition(Office officeCondition, int? pageIndex, int? pageSize)
        {
            string officeCode = officeCondition.OfficeCode;
            string officeName = officeCondition.OfficeName;
            string companyCode = null;
            try { companyCode = officeCondition.Company.CompanyCode; } catch (NullReferenceException) { }
            string companyName = null;
            try { companyName = officeCondition.Company.CompanyName; } catch (NullReferenceException) { }
            string employeeName = null;
            try { employeeName = officeCondition.Employee.EmployeeName; } catch (NullReferenceException) { }
            string delFlag = officeCondition.DelFlag;

            // 事業所データの取得
            IOrderedQueryable<Office> officeList =
                    from a in db.Office
                    where (string.IsNullOrEmpty(officeCode) || a.OfficeCode.Contains(officeCode))
                    && (string.IsNullOrEmpty(officeName) || a.OfficeName.Contains(officeName))
                    && (string.IsNullOrEmpty(companyCode) || a.CompanyCode.Contains(companyCode))
                    && (string.IsNullOrEmpty(companyName) || a.Company.CompanyName.Contains(companyName))
                    && (string.IsNullOrEmpty(employeeName) || a.Employee.EmployeeName.Contains(employeeName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.CompanyCode, a.OfficeCode
                    select a;

            // ページング制御情報を付与した事業所データの返却
            PaginatedList<Office> ret = new PaginatedList<Office>(officeList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }
        /// <summary>
        /// 閲覧可能な事業所リストを取得する
        /// </summary>
        /// <param name="auth">権限設定データ</param>
        /// <returns>事業所リスト</returns>
        public List<Office> GetListByAuthCondition(CrmsAuth auth) {
            var query =
                from a in db.Office
                select a;

            ParameterExpression param = Expression.Parameter(typeof(Office), "x");
            Expression offExpression = auth.CreateExpressionForOffice(param, new string[] { "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<Office, bool>>(offExpression, param));
            }
            Expression comExpression = auth.CreateExpressionForCompany(param, new string[] { "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<Office, bool>>(comExpression, param));
            }

            return query.OrderBy(x => x.OfficeCode).ToList();
        }
        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="office">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Office EditModel(Office office)
        {
            // 内部コード項目の名称情報取得
            office.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, office.DelFlag);

            // 出口
            return office;
        }

        //Add 2014/10/27 arc amii サブシステム仕訳機能移行対応 勘定奉行データ出力画面専用の検索ダイアログ追加
        /// <summary>
        /// 事業所マスタデータ検索(拠点指定)
        /// </summary>
        /// <param name="OfficeCondition">事業所検索条件</param>
        /// <param name="divType">拠点</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>事業所マスタデータ検索結果</returns>
        public PaginatedList<Office> GetListDivCondition(Office officeCondition,string divType, int? pageIndex, int? pageSize)
        {
            string officeCode = officeCondition.OfficeCode;
            string officeName = officeCondition.OfficeName;
            string companyCode = null;
            try { companyCode = officeCondition.Company.CompanyCode; }
            catch (NullReferenceException) { }
            string companyName = null;
            try { companyName = officeCondition.Company.CompanyName; }
            catch (NullReferenceException) { }
            string employeeName = null;
            try { employeeName = officeCondition.Employee.EmployeeName; }
            catch (NullReferenceException) { }
            string delFlag = officeCondition.DelFlag;

            // 事業所データの取得
            IOrderedQueryable<Office> officeList =
                    from a in db.Office
                    where (string.IsNullOrEmpty(officeCode) || a.OfficeCode.Contains(officeCode))
                    && (string.IsNullOrEmpty(officeName) || a.OfficeName.Contains(officeName))
                    && (string.IsNullOrEmpty(companyCode) || a.CompanyCode.Contains(companyCode))
                    && (string.IsNullOrEmpty(companyName) || a.Company.CompanyName.Contains(companyName))
                    && (string.IsNullOrEmpty(employeeName) || a.Employee.EmployeeName.Contains(employeeName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    && (a.OfficeCode.StartsWith(divType))
                    orderby a.CompanyCode, a.OfficeCode
                    select a;

            // ページング制御情報を付与した事業所データの返却
            PaginatedList<Office> ret = new PaginatedList<Office>(officeList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }
    }
}
