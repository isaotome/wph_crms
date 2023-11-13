using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class PartsAverageCostDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public PartsAverageCostDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 部品平均単価を検索する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        public PaginatedList<PartsAverageCost> GetListByMonth(PartsAverageCost condition,int pageIndex,int pageSize){
            return new PaginatedList<PartsAverageCost>(GetQuery(condition), pageIndex, pageSize);
        }

        /// <summary>
        /// 部品平均単価を検索する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<PartsAverageCost> GetListByMonth(PartsAverageCost condition) {
            return GetQuery(condition).ToList();
        }

        /// <summary>
        /// 検索クエリを取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        private IQueryable<PartsAverageCost> GetQuery(PartsAverageCost condition) {
            DateTime month = condition.CloseMonth;
            string companyCode = condition.CompanyCode;

            var query =
                from a in db.PartsAverageCost
                where a.CloseMonth.Equals(month)
                && (string.IsNullOrEmpty(companyCode) || a.CompanyCode.Equals(companyCode))
                select a;
            return query;
        }
        //Del 2015/08/20 arc nakayama #3243_サービス伝票入力画面明細欄の原価や、部品マスタの移動平均の値が表示されない  PartsAverageCostControlテーブルを削除するため参照しているメソッドも削除
/*      /// <summary>
        /// 対象月リストを取得する
        /// </summary>
        /// <returns></returns>
        public List<PartsAverageCostControl> GetMonthList() {
            var query =
                from a in db.PartsAverageCostControl
                orderby a.CloseMonth descending
                select a;
            return query.ToList();
        }

        /// <summary>
        /// 対象月の処理レコードを取得する（存在すれば処理済み）
        /// </summary>
        /// <param name="targetMonth">対象月</param>
        /// <returns></returns>
        public PartsAverageCostControl GetByKey(DateTime targetMonth) {
            var query =
                (from a in db.PartsAverageCostControl
                 where a.CloseMonth.Equals(targetMonth)
                 select a).FirstOrDefault();
            return query;
        }
*/
        /// <summary>
        /// 最新の移動平均単価を取得する
        /// </summary>
        /// <param name="condition">CompanyCode,PartsNumber指定</param>
        /// <returns></returns>
        public PartsAverageCost GetByKey(PartsAverageCost condition) {
            var query = (from a in db.PartsAverageCost
                        where a.CompanyCode.Equals(condition.CompanyCode)
                        && a.PartsNumber.Equals(condition.PartsNumber)
                        orderby a.CloseMonth descending
                        select a).FirstOrDefault();
            return query;
        }

        // Add 2014/12/24 arc nakayama 移動平均単価以降
        /// <summary>
        /// 部品ナンバーをキーにレコード取得(１件)
        /// </summary>
        /// <param name="condition">PartsNumber指定</param>
        /// <returns></returns>
        public PartsAverageCost GetByKeyPartsNumber(Parts Parts)
        {
            PartsAverageCost query = (from a in db.PartsAverageCost
                                      where a.PartsNumber.Equals(Parts.PartsNumber)
                                      && a.CompanyCode.Equals("001")    //カンパニーコードは001固定(暫定)
                                      orderby a.CloseMonth descending
                                      select a).FirstOrDefault();
            return query;
        }
    }
}
