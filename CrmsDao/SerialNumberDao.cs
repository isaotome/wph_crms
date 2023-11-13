using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.Linq;
using System.Data.SqlClient;

namespace CrmsDao
{

    /// <summary>
    /// 採番管理マスタアクセスクラス
    ///   採番管理マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class SerialNumberDao
    {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public SerialNumberDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;

            // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
            db.Log = new OutputWriter();
        }

        /// <summary>
        /// 採番管理マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="serialCode">シリアルコード</param>
        /// <returns>採番管理マスタデータ(1件)</returns>
        /// <history>
        /// 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        /// </history>
        public SerialNumber GetByKey(string serialCode)
        {
            IQueryable<SerialNumber> query =
                from a in db.SerialNumber
                where a.SerialCode.Equals(serialCode)
                select a;
                
            var result = CommonUtils.SelectWithUpdlock(db, query);

            return result.FirstOrDefault();

            // 採番管理データの取得
           /*
            return
                (from a in db.SerialNumber
                 where a.SerialCode.Equals(serialCode)
                 select a
                ).FirstOrDefault();
            */


            

            
        }

        /// <summary>
        /// 伝票番号採番
        /// </summary>
        /// <returns>伝票番号</returns>
        public string GetNewSlipNumber()
        {
            // Add 2014/08/12 arc amii エラーログ対応 ログに出力する処理名を設定
            OutputLogData.procName = "伝票番号採番";
            return string.Format("{0:00000000}", GetNewSequenceNumber("SALES"));
        }

        /// <summary>
        /// 移動伝票番号採番
        /// </summary>
        /// <returns>移動伝票番号</returns>
        public string GetNewTransferNumber()
        {
            // Add 2014/08/12 arc amii エラーログ対応 ログに出力する処理名を設定
            OutputLogData.procName = "移動伝票番号採番";
            return string.Format("{0:00000000}", GetNewSequenceNumber("TRANSFER"));
        }
        /// <summary>
        /// 発注番号採番
        /// </summary>
        /// <returns>発注番号</returns>
        public string GetNewPartsPurchaseOrderNumber()
        {
            // Add 2014/08/12 arc amii エラーログ対応 ログに出力する処理名を設定
            OutputLogData.procName = "発注番号採番";
            return string.Format("{0:00000000}", GetNewSequenceNumber("PARTS_PURCHASE_ORDER"));
        }

        /// <summary>
        /// 車両発注依頼番号採番
        /// </summary>
        /// <returns>発注番号</returns>
        public string GetNewCarPurchaseOrderNumber()
        {
            // Add 2014/08/12 arc amii エラーログ対応 ログに出力する処理名を設定
            OutputLogData.procName = "車両発注依頼番号採番";
            return string.Format("{0:00000000}", GetNewSequenceNumber("CAR_PURCHASE_ORDER"));
        }

        /// <summary>
        /// 仕入伝票番号採番
        /// </summary>
        /// <returns>仕入伝票番号</returns>
        public string GetNewPurchaseNumber()
        {
            // Add 2014/08/12 arc amii エラーログ対応 ログに出力する処理名を設定
            OutputLogData.procName = "仕入伝票番号採番";
            return string.Format("{0:00000000}", GetNewSequenceNumber("PURCHASE"));
        }
        /// <summary>
        /// 管理番号採番
        /// </summary>
        /// <param name="companyCode">会社コード</param>
        /// <param name="newUsedType">新中区分</param>
        /// <returns>管理番号</returns>
        public string GetNewSalesCarNumber(string companyCode, string newUsedType)
        {

            string prefix = companyCode.PadLeft(3, '0') + newUsedType;
            string serialCode = "SALES_CAR_" + prefix;

            // Add 2014/08/12 arc amii エラーログ対応 ログに出力する処理名を設定
            OutputLogData.procName = "管理番号採番";

            // 新しい連番の採番(既存の管理レコードが存在しない場合、採番されない)
            decimal? newSeqNo = GetNewSequenceNumber(serialCode);

            // 既存の管理レコードが存在しない場合、レコード追加＋一意制約時連番カウントアップ
            if (newSeqNo == null)
            {
                db.AddSerialNumber(serialCode, "管理番号(" + prefix + ")", null, null, decimal.Parse("1"), "SerialNumberDao", ref newSeqNo);
            }

            // 管理番号の返却
            return prefix + string.Format("{0:0000000}", (newSeqNo ?? 0m));
        }

        /// <summary>
        /// 顧客コード採番
        /// </summary>
        /// <param name="companyCode">会社コード</param>
        /// <param name="newUsedType">新中区分</param>
        /// <returns>顧客コード</returns>
        public string GetNewCustomerCode(string departmentCode)
        {

            string prefix = departmentCode;
            string serialCode = "CUSTOMER_" + prefix;

            // Add 2014/08/12 arc amii エラーログ対応 ログに出力する処理名を設定
            OutputLogData.procName = "顧客コード採番";

            // 新しい連番の採番(既存の管理レコードが存在しない場合、採番されない)
            decimal? newSeqNo = GetNewSequenceNumber(serialCode);

            // 既存の管理レコードが存在しない場合、レコード追加＋一意制約時連番カウントアップ
            if (newSeqNo == null)
            {
                db.AddSerialNumber(serialCode, "顧客コード(" + prefix + ")", null, null, decimal.Parse("1"), "SerialNumberDao", ref newSeqNo);
            }

            // 管理番号の返却
            return prefix + string.Format("{0:0000000}", (newSeqNo ?? 0m));
        }

        /// <summary>
        /// 採番管理テーブル更新
        /// </summary>
        /// <param name="serialCode">シリアルコード</param>
        /// <returns>新しく採番された連番</returns>
        private decimal? GetNewSequenceNumber(string serialCode)
        {

            decimal? newSeqNo = null;

            // 採番管理テーブルの取得
            SerialNumber serialNumber = GetByKey(serialCode);
            if (serialNumber == null)
            {
                return newSeqNo;
            }

            // 採番管理テーブルの更新
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                newSeqNo = ++serialNumber.SequenceNumber;
                serialNumber.LastUpdateEmployeeCode = "SerialNumberDao";
                serialNumber.LastUpdateDate = DateTime.Now;
                try
                {
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException e)
                {
                    if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                    {
                        //Add 2014/08/12 arc amii エラーログ対応 Exceptionを設定
                        OutputLogData.exLog = e;
                        // 2014/08/12 arc amii throwを使用してエラー画面に遷移する
                        throw e;
                    }
                    foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                    {
                        occ.Resolve(RefreshMode.OverwriteCurrentValues);
                    }
                }
            }

            // 出口
            return newSeqNo;
        }
        /// <summary>
        /// 指定された管理番号が利用可能な範囲かどうか
        /// </summary>
        /// <param name="salesCarNumber">管理番号</param>
        /// <returns></returns>
        public bool CanUseSalesCarNumber(string salesCarNumber)
        {
            if (string.IsNullOrEmpty(salesCarNumber) || salesCarNumber.Length < 4)
            {
                return false;
            }
            string prefix = salesCarNumber.Substring(0, 4);
            var query =
                from a in db.Company
                where (a.CompanyCode + "N").Equals(prefix) || (a.CompanyCode + "U").Equals(prefix)
                select a;
            if (query.Count() > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 指定された会社コードが利用可能な範囲かどうか
        /// </summary>
        /// <param name="companyCode">会社コード</param>
        /// <returns></returns>
        public bool CanUserCompanyCode(string companyCode)
        {
            var query =
                from a in db.SalesCar
                where a.SalesCarNumber.Substring(0, 4).Equals(companyCode + "N") ||
                a.SalesCarNumber.Substring(0, 4).Equals(companyCode + "U")
                select a;
            return query.Count() == 0;
        }
    }
}
