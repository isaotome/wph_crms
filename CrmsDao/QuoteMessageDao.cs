using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 見積メッセージマスタアクセスクラス
    ///   見積メッセージマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class QuoteMessageDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public QuoteMessageDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 見積メッセージマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="companyCode">会社コード</param>
        /// <param name="quoteType">見積種別</param>
        /// <returns>見積メッセージマスタデータ(1件)</returns>
        public QuoteMessage GetByKey(string companyCode, string quoteType)
        {
            // 見積メッセージデータの取得
            QuoteMessage quoteMessage =
                (from a in db.QuoteMessage
                 where a.CompanyCode.Equals(companyCode)
                 && a.QuoteType.Equals(quoteType)
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (quoteMessage != null)
            {
                quoteMessage = EditModel(quoteMessage);
            }

            // 見積メッセージデータの返却
            return quoteMessage;
        }

        /// <summary>
        /// 見積メッセージマスタデータ検索
        /// </summary>
        /// <param name="quoteMessageCondition">見積メッセージ検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>見積メッセージマスタデータ検索結果</returns>
        public PaginatedList<QuoteMessage> GetListByCondition(QuoteMessage quoteMessageCondition, int? pageIndex, int? pageSize)
        {
            string companyCode = null;
            try { companyCode = quoteMessageCondition.Company.CompanyCode; } catch (NullReferenceException) { }
            string companyName = null;
            try { companyName = quoteMessageCondition.Company.CompanyName; } catch (NullReferenceException) { }
            string quoteType = quoteMessageCondition.QuoteType;
            string delFlag = quoteMessageCondition.DelFlag;

            // 見積メッセージデータの取得
            IOrderedQueryable<QuoteMessage> quoteMessageList =
                    from a in db.QuoteMessage
                    where (string.IsNullOrEmpty(companyCode) || a.CompanyCode.Contains(companyCode))
                    && (string.IsNullOrEmpty(companyName) || a.Company.CompanyName.Contains(companyName))
                    && (string.IsNullOrEmpty(quoteType) || a.QuoteType.Contains(quoteType))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.CompanyCode, a.QuoteType
                    select a;

            // ページング制御情報を付与した見積メッセージデータの返却
            PaginatedList<QuoteMessage> ret = new PaginatedList<QuoteMessage>(quoteMessageList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="quoteMessage">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private QuoteMessage EditModel(QuoteMessage quoteMessage)
        {
            // 内部コード項目の名称情報取得
            quoteMessage.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, quoteMessage.DelFlag);

            // 出口
            return quoteMessage;
        }
    }
}
