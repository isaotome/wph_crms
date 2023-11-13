using System;
using System.Collections.Generic;
using System.Linq;

namespace CrmsDao
{

  /// <summary>
  /// 適格請求書（インボイス）用の消費税額登録
  /// </summary>
  /// <history>
  /// 2023/09/05 #4162 インボイス対応
  /// </history>
  public class InvoiceConsumptionTaxDao
  {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public InvoiceConsumptionTaxDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

         /// <summary>
        /// インボイス消費税データ取得(PKによる取得)
        /// </summary>
        /// <param name="invoiceConsumptionTaxId">ユニークID</param>
        /// <returns>インボイス消費税テーブル(1件)</returns>
        public InvoiceConsumptionTax GetByKey(Guid invoiceConsumptionTaxId ) {

            // インボイス消費税データの取得
            InvoiceConsumptionTax invoiceconsumptiontax =
                (from a in db.InvoiceConsumptionTax
                 where (a.InvoiceConsumptionTaxId.Equals(invoiceConsumptionTaxId))
                 select a
                ).FirstOrDefault();

            // インボイス消費税データの返却
            return invoiceconsumptiontax;
        }

        /// <summary>
        /// インボイス消費税データ取得
        /// </summary>
        /// <param name="SlipNumber">伝票番号</param>
        /// <param name="CustomerClaimCode">請求先コード</param>
        /// <param name="Rate">消費税率</param>
        /// <returns>インボイス消費税テーブル(1件)</returns>
        public InvoiceConsumptionTax GetByKey(string slipNumber, string customerClaimCode, int rate) {

            // インボイス消費税データの取得
            InvoiceConsumptionTax invoiceconsumptiontax =
                (from a in db.InvoiceConsumptionTax
                 where (a.SlipNumber.Equals(slipNumber))
                 && a.CustomerClaimCode.Equals(customerClaimCode)
                 && a.Rate.Equals(rate)
                 && a.DelFlag.Equals("0")
                 select a
                ).FirstOrDefault();

            // インボイス消費税データの返却
            return invoiceconsumptiontax;
        }

        /// <summary>
        /// 伝票番号によるリスト取得
        /// </summary>
        /// <param name="SlipNumber">伝票番号</param>
        /// <returns>インボイス消費税リスト</returns>
        public List<InvoiceConsumptionTax> GetBySlipNumber(string slipNumber) {

            // インボイス消費税データの取得
            List<InvoiceConsumptionTax> list =
            (
                from a in db.InvoiceConsumptionTax
                 where (a.SlipNumber.Equals(slipNumber))
                 && (a.DelFlag.Equals("0"))
                 select a
                ).ToList();

            // インボイス消費税データの返却
            return list;
        }
  }
}
