using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class PartsMovingAverageCostDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        /// <history>
        /// 2018/05/14 arc yano #3880 売上原価計算及び棚卸評価法の変更 新規作成
        /// </history>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        /// <history>
        /// 2018/05/14 arc yano #3880 売上原価計算及び棚卸評価法の変更 新規作成
        /// </history>
        public PartsMovingAverageCostDao(CrmsLinqDataContext context)
        {
            db = context;
        }

        /// <summary>
        /// 移動平均単価を取得する
        /// </summary>
        /// <param name="condition">CompanyCode,PartsNumber指定</param>
        /// <returns></returns>
        public PartsMovingAverageCost GetByKey(PartsMovingAverageCost condition)
        {
            var query = (from a in db.PartsMovingAverageCost
                         where a.CompanyCode.Equals(condition.CompanyCode)
                         && a.PartsNumber.Equals(condition.PartsNumber)
                         select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 部品移動平均単価を検索する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/14 arc yano #3880 売上原価計算及び棚卸評価法の変更 新規作成
        /// </history>
        public List<PartsMovingAverageCost> GetListByMonth(PartsMovingAverageCost condition)
        {
            return GetQuery(condition).ToList();
        }

        /// <summary>
        /// 検索クエリを取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/14 arc yano #3880 売上原価計算及び棚卸評価法の変更 新規作成
        /// </history>
        private IQueryable<PartsMovingAverageCost> GetQuery(PartsMovingAverageCost condition)
        {
            string companyCode = condition.CompanyCode;

            var query =
                from a in db.PartsMovingAverageCost
                where (string.IsNullOrEmpty(companyCode) || a.CompanyCode.Equals(companyCode))
                select a;
            return query;
        }

        /// <summary>
        /// 部品の数量(受入/払出)、単価(受入/払出)を元に移動平均単価を再計算
        /// </summary>
        /// <param name="condition">CompanyCode,PartsNumber指定</param>
        /// <returns></returns>
        /// <history>
        /// 2019/05/23 yano #3990【部品入荷入力】移動平均の計算結果の履歴機能の追加
        /// 2018/12/28 yano #3968 部品入荷入力　新規部品入荷時の移動平均単価の不正
        /// </history>
        public void UpdateAverageCost(string partsNumber, string companyCode, decimal? quantity, decimal? price, string employeeCode)
        {
            decimal? preCost = null;


            //在庫数の取得
            decimal? stockQuantity = 
                (
                    from a in db.PartsStock
                    where a.PartsNumber.Equals(partsNumber)
                    && a.DelFlag.Equals("0")
                    select a.Quantity
                ).Sum();


            //対象の移動平均単価の取得
            var rec =
                (
                    from a in db.PartsMovingAverageCost
                    where a.CompanyCode.Equals(companyCode)
                    && a.PartsNumber.Equals(partsNumber)
                    select a
                ).FirstOrDefault();

            //登録されていない場合は入荷単価で新規作成
            if (rec == null)
            {
                PartsMovingAverageCost newrec = new PartsMovingAverageCost();

                newrec.CompanyCode = companyCode;
                newrec.PartsNumber = partsNumber;
                newrec.Price = price;               //Mod  2018/12/28 yano #3968
                newrec.CreateEmployeeCode = employeeCode;
                newrec.CreateDate = DateTime.Now;
                newrec.LastUpdateEmployeeCode = employeeCode;
                newrec.LastUpdateDate = DateTime.Now;
                newrec.DelFlag = "0";

                db.PartsMovingAverageCost.InsertOnSubmit(newrec);
            }
            else
            {
                preCost = (rec.Price ?? 0);     //Add 2019/05/23 yano #3990

                decimal totalQuantity = (stockQuantity ?? 0) + (quantity ?? 0);

                decimal totalAmount = ((stockQuantity ?? 0) * (rec.Price ?? 0) + (quantity ?? 0) * (price ?? 0));

                if (totalQuantity != 0)
                {
                    rec.Price = Decimal.Round(totalAmount / totalQuantity, 0, MidpointRounding.AwayFromZero);
                }
                else
                {
                    rec.Price = rec.Price;  //数量が0の場合は何もしない
                }

                rec.LastUpdateEmployeeCode = employeeCode;
                rec.LastUpdateDate = DateTime.Now;
                if (string.IsNullOrWhiteSpace(rec.DelFlag) || !rec.DelFlag.Equals("0"))
                {
                    rec.DelFlag = "0";
                }
            }

            //Add 2019/05/23 yano #3990
            //-----------------------------------------
            //履歴テーブルに計算結果を登録する
            //-----------------------------------------
            PartsMovingAverageCostHistory newhistory = new PartsMovingAverageCostHistory();

            newhistory.HistoryId = Guid.NewGuid();
            newhistory.CompanyCode = companyCode;
            newhistory.PartsNumber = partsNumber;
            newhistory.PreCost = preCost;
            newhistory.StockQuantity = (stockQuantity ?? 0);
            newhistory.ChangeCost = (price ?? 0);
            newhistory.ChangeQuantity = (quantity ?? 0);
            newhistory.PostCost = (rec != null ? rec.Price : newhistory.ChangeCost);
            newhistory.CreateEmployeeCode = employeeCode;
            newhistory.CreateDate = DateTime.Now;

            db.PartsMovingAverageCostHistory.InsertOnSubmit(newhistory);
        }
    }
}
