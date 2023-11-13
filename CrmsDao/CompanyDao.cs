using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 会社マスタアクセスクラス
    ///   会社マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CompanyDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CompanyDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 会社マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="companyCode">会社コード</param>
        /// <returns>会社マスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため
        public Company GetByKey(string companyCode, bool includeDeleted = false)
        {
            // 会社データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            Company company =
                (from a in db.Company
                 where a.CompanyCode.Equals(companyCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (company != null)
            {
                company = EditModel(company);
            }

            // 会社データの返却
            return company;
        }

        /// <summary>
        /// 会社マスタデータ検索
        /// </summary>
        /// <param name="companyCondition">会社検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>会社マスタデータ検索結果</returns>
        public PaginatedList<Company> GetListByCondition(Company companyCondition, int? pageIndex, int? pageSize)
        {
            string companyCode = companyCondition.CompanyCode;
            string companyName = companyCondition.CompanyName;
            string employeeName = null;
            try { employeeName = companyCondition.Employee.EmployeeName; } catch (NullReferenceException) { }
            string delFlag = companyCondition.DelFlag;

            // 会社データの取得
            IOrderedQueryable<Company> companyList =
                    from a in db.Company
                    where (string.IsNullOrEmpty(companyCode) || a.CompanyCode.Contains(companyCode))
                    && (string.IsNullOrEmpty(companyName) || a.CompanyName.Contains(companyName))
                    && (string.IsNullOrEmpty(employeeName) || a.Employee.EmployeeName.Contains(employeeName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.CompanyCode
                    select a;

            // ページング制御情報を付与した会社データの返却
            PaginatedList<Company> ret = new PaginatedList<Company>(companyList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="company">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Company EditModel(Company company)
        {
            // 内部コード項目の名称情報取得
            company.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, company.DelFlag);

            // 出口
            return company;
        }
    }
}
