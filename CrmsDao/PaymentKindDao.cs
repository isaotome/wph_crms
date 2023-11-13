using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 支払種別マスタアクセスクラス
    ///   支払種別マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class PaymentKindDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public PaymentKindDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 支払種別マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="paymentKindCode">支払種別コード</param>
        /// <returns>支払種別マスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　デフォルト引数追加（削除フラグを考慮するかどうかのフラグ追加（デフォルト考慮する））
        public PaymentKind GetByKey(string paymentKindCode, bool includeDeleted = false)
        {
            // 支払種別データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            PaymentKind paymentKind =
                (from a in db.PaymentKind
                 where a.PaymentKindCode.Equals(paymentKindCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (paymentKind != null)
            {
                paymentKind = EditModel(paymentKind);
            }

            // 支払種別データの返却
            return paymentKind;
        }

        /// <summary>
        /// 支払種別マスタデータ検索
        /// </summary>
        /// <param name="paymentKindCondition">支払種別検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>支払種別マスタデータ検索結果</returns>
        public PaginatedList<PaymentKind> GetListByCondition(PaymentKind paymentKindCondition, int? pageIndex, int? pageSize)
        {
            string paymentKindCode = paymentKindCondition.PaymentKindCode;
            string paymentKindName = paymentKindCondition.PaymentKindName;
            string delFlag = paymentKindCondition.DelFlag;

            // 支払種別データの取得
            IOrderedQueryable<PaymentKind> paymentKindList =
                    from a in db.PaymentKind
                    where (string.IsNullOrEmpty(paymentKindCode) || a.PaymentKindCode.Contains(paymentKindCode))
                    && (string.IsNullOrEmpty(paymentKindName) || a.PaymentKindName.Contains(paymentKindName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.PaymentKindCode
                    select a;

            // ページング制御情報を付与した支払種別データの返却
            PaginatedList<PaymentKind> ret = new PaginatedList<PaymentKind>(paymentKindList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="paymentKind">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private PaymentKind EditModel(PaymentKind paymentKind)
        {
            // 内部コード項目の名称情報取得
            paymentKind.ClaimDayName = CodeUtils.GetName(CodeUtils.Day, CommonUtils.IntToStr(paymentKind.ClaimDay));
            paymentKind.PaymentDayName = CodeUtils.GetName(CodeUtils.Day, CommonUtils.IntToStr(paymentKind.PaymentDay));
            paymentKind.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, paymentKind.DelFlag);

            // 出口
            return paymentKind;
        }
    }
}
