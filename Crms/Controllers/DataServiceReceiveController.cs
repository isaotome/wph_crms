using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.IO;
using System.Text;
using CrmsDao;

namespace Crms.Controllers
{
    public class DataServiceReceiveController : Controller
    {
        public ActionResult Criteria()
        {
            SetDataComponent(new FormCollection());
            return View("DataServiceReceiveCriteria");
            
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form, HttpPostedFileBase attachedFile) {
            string path = HttpContext.Server.MapPath("/UploadFiles/") + Guid.NewGuid() + "/";
            DirectoryInfo uploadDirectory = new DirectoryInfo(path);
            if (!uploadDirectory.Exists) {
                uploadDirectory.Create();
            }
            string uploadPath = uploadDirectory + Path.GetFileName(attachedFile.FileName);
            attachedFile.SaveAs(uploadPath);
            List<string[]> list = new List<string[]>();
            using (StreamReader sr = new StreamReader(uploadPath,Encoding.Default)) {
                sr.ReadLine();
                while (sr.Peek() != -1) {
                    string[] stringBuffer;
                    stringBuffer = sr.ReadLine().Split(',');
                    list.Add(stringBuffer);
                }
                ViewData["message"] = list.Count() + "åèÅAéÊçûèàóùÇ™äÆóπÇµÇ‹ÇµÇΩ";
            }
            SetDataComponent(form);
            return View("DataServiceReceiveCriteria",list);
        }

        private void SetDataComponent(FormCollection form) {
            ViewData["FileType"] = CodeUtils.GetSelectListByModel(CodeUtils.GetFileTypeList(), form["FileType"], true);
        }

    }
}
