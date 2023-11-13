using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Transactions;

namespace Crms.Controllers {

    /// <summary>
    /// 店舗入金消込機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ShopDepositController : DepositController {

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShopDepositController() : base() {
            criteriaName = "ShopDepositCriteria";
            isShopDeposit = true;
        }
    }
}
