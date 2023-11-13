using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CashAccountController : Controller
    {
        private CrmsLinqDataContext db;
        public CashAccountController() {
            db = new CrmsLinqDataContext();
        }
        public void GetMasterDetail(string code) {
            if (Request.IsAjaxRequest()) {
                Office office = new OfficeDao(db).GetByKey(code);
                CodeDataList codeDataList = new CodeDataList();
                if (office != null) {
                    codeDataList.Code = office.OfficeCode;
                    codeDataList.Name = office.OfficeName;
                    codeDataList.DataList = new List<CodeData>();
                    List<CashAccount> cashAccountList = new CashAccountDao(db).GetListByOfficeCode(code);
                    foreach (var a in cashAccountList) {
                        codeDataList.DataList.Add(new CodeData() { Code = a.CashAccountCode, Name = a.CashAccountName });
                    }

                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, codeDataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
        }

    }
}
