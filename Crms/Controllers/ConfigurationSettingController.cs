using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.Linq;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{

    //Mod 2014/07/30 arc yano 権限の無いユーザの場合、車両伝票の下取車登録印紙代が取得できなくなるため
    //                        ユーザ認証をクラスではなく、メソッド毎に設定する。
    [ExceptionFilter]
    //[AuthFilter]        
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ConfigurationSettingController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "アプリケーション設定";     // 画面名
        private static readonly string PROC_NAME = "アプリケーション設定更新"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ConfigurationSettingController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 検索＆入力画面
        /// </summary>
        /// <returns></returns>
        [AuthFilter]    //Add 2014/07/30 arc yano
        public ActionResult Criteria() {
            List<ConfigurationSetting> list = new ConfigurationSettingDao(db).GetListAll();
            return View("ConfigurationSettingCriteria", list);
        }

        [AuthFilter]    //Add 2014/07/30 arc yano
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(List<ConfigurationSetting> data) {
            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            ConfigurationSettingDao dao = new ConfigurationSettingDao(db);
            using (TransactionScope ts = new TransactionScope()) {
                foreach (var a in data) {
                    ConfigurationSetting target = dao.GetByKey(a.Code);
                    target.Value = a.Value;
                }

                // Mod 2014/08/05 arc amii エラーログ対応 ChangeConflictExceptionを追加し、『throw e』をエラー出力処理に変更する
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException ce)
                    {
                        // 更新時、クライアントの読み取り以降にDB値が更新された時、ローカルの値をDB値で上書きする
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        // リトライ回数を超えた場合、エラーとする
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(ce, PROC_NAME, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // 上記以外の例外の場合、エラーログ出力し、エラー画面に遷移する
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }
            return Criteria();
        }

        /// <summary>
        /// アプリケーション設定値を取得する
        /// </summary>
        /// <param name="key">設定キー</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]   //Add 2014/05/27 arc yano vs2012対応
        public ActionResult GetMasterDetail() {
            if (Request.IsAjaxRequest()) {
                ConfigurationSettingDao dao = new ConfigurationSettingDao(db);
                List<ConfigurationSetting> settings = dao.GetListAll();
                Dictionary<string, string> ret = new Dictionary<string, string>();
                foreach (var a in settings) {
                    ret.Add(a.Code, a.Value);
                }
                if (ret.Count > 0) {
                    return Json(ret);
                }
            }
            return new EmptyResult();
        }
    }
}
