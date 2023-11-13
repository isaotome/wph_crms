using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Data.SqlClient;
using System.Data.Linq;


//-----------------------------------------------------------------------------
//　機能　：車両追跡データアクセスクラス
//　作成日：2017/03/19 arc yano #3731 サブシステム機能移行(車両追跡)
//
//
//-----------------------------------------------------------------------------
namespace CrmsDao
{
    public class CarStatusCheckDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext"></param>
        public CarStatusCheckDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 車両基本情報取得
        /// </summary>
        /// <param name="salesCarNumber">管理番号</param>
        /// <param name="vin">車台番号</param>
        /// <returns>車両基本情報</returns>
        /// <history>
        /// 2017/03/19 arc yano #3721 サブシステム移行(車両追跡) 新規作成
        /// </history>
        public List<GetCarBasicInfoResult> GetCarBasicInfo(string salesCarNumber, string vin)
        {   
            //ストアドプロシージャ実行
            ISingleResult<GetCarBasicInfoResult> result = db.GetCarBasicInfo(salesCarNumber, vin);

            return result.ToList();
        }

        /// <summary>
        /// 車両遷移取得
        /// </summary>
        /// <param name="salesCarNumber">管理番号</param>
        /// <param name="vin">車台番号</param>
        /// <returns>車両遷移</returns>
        /// <history>
        /// 2017/03/19 arc yano #3721 サブシステム移行(車両追跡) 新規作成
        /// </history>
        public List<GetCarStatusTransitionResult> GetCarStatusTransition(string salesCarNumber, string vin)
        {
            //ストアドプロシージャ実行
            ISingleResult<GetCarStatusTransitionResult> result = db.GetCarStatusTransition(salesCarNumber, vin);

            return result.ToList();
        }

        /// <summary>
        /// 車両販売伝票
        /// </summary>
        /// <param name="salesCarNumber">管理番号</param>
        /// <param name="vin">車台番号</param>
        /// <returns>車両販売伝票</returns>
        /// <history>
        /// 2017/03/19 arc yano #3721 サブシステム移行(車両追跡) 新規作成
        /// </history>
        public List<GetCarSalesSlipResult> GetCarSalesSlip(string salesCarNumber, string vin)
        {
            //ストアドプロシージャ実行
            ISingleResult<GetCarSalesSlipResult> result = db.GetCarSalesSlip(salesCarNumber, vin);

            return result.ToList();
        }

    }
}
