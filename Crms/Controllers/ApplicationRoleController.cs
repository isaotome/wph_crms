using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Collections;
using System.Data.Linq;
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    [ExceptionFilter]
    [OutputCache(Duration=0,VaryByParam="null")]
    [AuthFilter]
    public class ApplicationRoleController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "アプリケーションロール";     // 画面名
        private static readonly string PROC_NAME = "アプリケーションロール登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ApplicationRoleController()
        {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// アプリケーションロール検索画面表示(セキュリティロールリストを表示）
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            SecurityRoleDao dao = new SecurityRoleDao(db);
            List<SecurityRole> list = dao.GetListAll();
            return View("ApplicationRoleCriteria",list);
        }

        /// <summary>
        /// アプリケーションロール入力画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Entry()
        {
            List<Application> header = new ApplicationDao(db).GetListAll();
            
            return View("ApplicationRoleEntry",header);
        }

        /// <summary>
        /// アプリケーションロール更新
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(FormCollection form)
        {
            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            //セキュリティレベルを更新する
            List<SecurityRole> secList = new SecurityRoleDao(db).GetListAll();
            foreach(var s in secList){
                s.SecurityLevelCode = form[string.Format("sec[{0}].SecurityLevelCode", s.SecurityRoleCode)];
            }

            //メニューリストを取得する
            List<Application> menuList = new ApplicationDao(db).GetListAll();
            for (int i = 0; i < menuList.Count; i++) {
                for (int j = 0; j < menuList[i].ApplicationRole.Count; j++) {
                    menuList[i].ApplicationRole[j].EnableFlag = form[string.Format("role[{0}].ApplicationRole[{1}].{2}",i,j,"EnableFlag")].Contains("true") ? true : false;
                }
            }

            //Add 2014/08/05 arc amii エラーログ対応 SubmitChangesにtry文を追加
            try
            {
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                // セッションにSQL文を登録
                Session["ExecSQL"] = OutputLogData.sqlText;
                // ログに出力
                OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                // エラーページに遷移
                return View("Error");
            }
            //MOD 2014/10/28 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //ViewData["close"] = "1";
            return View("ApplicationRoleEntry", menuList);
        }
    }
}
