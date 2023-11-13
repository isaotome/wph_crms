using Crms.Models;                      //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為に追加
using CrmsDao;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

namespace Crms.Controllers
{
  [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class HomeController : Controller
    {
        //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "ログイン";   // 画面名
        private static readonly string PROC_NAME = "ログイン認証"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HomeController()
        {
            db = new CrmsLinqDataContext();
        }

        public ActionResult Start()
        {
            return View("Start");
        }

        /// <summary>
        /// ログインページを表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/04/09 arc yano #3757 キャッシュ問題
        /// </history>
        public ActionResult Index()
        {
            //セッション変数を明示的にクリア
            Session.Abandon();

            //Add 2014/09/04 arc amii 変更履歴追加対応 #3081 テキストファイルから履歴を取得・設定する
            GetRevisionHistory();

            SetComponent();         //Add 2018/04/09 arc yano #3757

            //ログイン画面を表示
            return View();
        }

        /// <summary>
        /// ログイン認証処理
        /// </summary>
        /// <param name="userid">ユーザーID</param>
        /// <param name="password">パスワード</param>
        /// <returns></returns>
        /// <history>
        /// 2019/05/22 yano #3987 【ログイン画面】ログイン成功時に最終ログイン日時が登録されない
        /// 2018/12/21 yano #3965 WE版新システム対応（Web.configによる処理の分岐)
        /// 2018/06/19 arc yano #3867 ログイン画面　新システムからActive Directoryのユーザアカウントのパスワード変更機能の追加
        /// 2014/09/04 arc amii 変更履歴追加対応 #3081 テキストファイルから履歴を取得・設定する
        /// 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
        /// 2014/05/08 vs2012対応 arc yano warning対応(ConfigurationSettingsクラスは古い.netバージョン(v1.0,1.1)用であるため、変更する。)
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(string userid, string password)
        {
            // Mod 2019/05/22 yano #3987
            // Add 2014/08/11 arc amii エラーログ対応
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            ViewData["userid"] = userid;

            GetRevisionHistory();       //Add 2014/09/04 arc amii #3081

            //Validationチェック
            if (!ValidateLogOn(userid, password))
            {
                return View();
            }

            //フォームに入力された情報でユーザーを検索する
            Employee employee = (new EmployeeDao(db)).GetByKey(userid);

            //DBにユーザーが存在しなければエラー
            if (employee == null)
            {
                ModelState.AddModelError("", "ログオン失敗: ユーザー名を認識できないか、またはパスワードが間違っています。");
                return View();
            }

            #region ドメインユーザー認証

            //string domainAuth = ConfigurationSettings.AppSettings["DomainAuth"];      // Mod 2014/05/08 vs2012対応
            string domainAuth = ConfigurationManager.AppSettings["DomainAuth"];
            if (!string.IsNullOrEmpty(domainAuth) && domainAuth.Equals("true"))
            {
                //LdapAuthentication ldAuth = new LdapAuthentication("LDAP://" + ConfigurationSettings.AppSettings["DomainName"]);
                LdapAuthentication ldAuth = new LdapAuthentication("LDAP://" + ConfigurationManager.AppSettings["DomainName"]);

                try
                {
                    //Mod 2018/06/19 arc yano #3867
                    //戻り値によって表示する画面を変更
                    int ret = ldAuth.IsAuthenticated(ConfigurationManager.AppSettings["DomainName"], userid, password);

                    switch (ret)
                    {
                        case -2:　//ドメイン情報無
                            ModelState.AddModelError("", "ドメイン情報を取得できません");
                            return View();

                        case -1:    //ユーザ情報無
                            ModelState.AddModelError("", "ログオン失敗: ユーザー名を認識できないか、またはパスワードが間違っています。");
                            return View();

                        case 1:     //有効期限１週間以内
                            ViewData["ExpireMsg"] = "パスワードを変更する必要があります";
                            Session["UserName"] = employee.EmployeeCode;
                            return View("ChangePassword");

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    //Add 2014/08/12 arc amii エラーログ対応 Exception発生時、ログ出力する処理追加
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogError(ex, PROC_NAME, FORM_NAME, "");

                    ModelState.AddModelError("", ex.Message);
                    return View();
                }
            }
            #endregion

            employee.LastLoginDateTime = DateTime.Now;
            db.SubmitChanges();

            //認証成功したらセッション変数に格納
            Session["Employee"] = employee;
            Session["TaskList"] = new TaskDao(db).GetIdListByEmployeeCode(employee.EmployeeCode);

            //Add 2018/12/21 yano #3965 
            Session["ConnectDB"] = (db != null ? db.Connection != null ? db.Connection.Database : "": "");

            //トップページを表示
            return View("Top");
        }

        /// <summary>
        /// パスワード変更画面表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/06/19 arc yano #3867 ログイン画面　新システムからActive Directoryのユーザアカウントのパスワード変更機能の追加 新規作成
        /// </history>
        public ActionResult ChangePassword(string userid)
        {
            Session["UserName"] = userid;

            return View("ChangePassword");
        }

        /// <summary>
        /// パスワード変更
        /// </summary>
        /// <param name="username">ユーザーID</param>
        /// <param name="oldpassword">現在のパスワード</param>
        /// <param name="newpassword">新しいパスワード</param>
        /// <param name="confirmpassword">新しいパスワード(確認)</param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/19 arc yano #3867 ログイン画面　新システムからActive Directoryのユーザアカウントのパスワード変更機能の追加 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ChangePassword(string oldpwd, string newpwd, string confirmpwd, FormCollection form)
        {
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            switch (form["RequestFlag"])
            {
                case "1":   //キャンセル
                    return  View("Index");

                default:
                    
                    //Validationチェック
                    if (!ValidateChangePassword(oldpwd, newpwd, confirmpwd))
                    {
                        return View("ChangePassword");
                    }

                    #region パスワード変更

                    LdapAuthentication ldAuth = new LdapAuthentication("LDAP://" + ConfigurationManager.AppSettings["DomainName"]);

                    try
                    {
                        if (!ldAuth.ChangePassword(ConfigurationManager.AppSettings["DomainName"], Session["UserName"].ToString(), oldpwd, newpwd, ModelState))
                        {
                            return View("ChangePassword");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/12 arc amii エラーログ対応 Exception発生時、ログ出力する処理追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogError(ex, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("", ex.Message);
                        return View();
                    }
                    #endregion


                    Employee employee = (new EmployeeDao(db)).GetByKey(Session["UserName"].ToString());

                    if (employee != null)
                    {
                        employee.LastLoginDateTime = DateTime.Now;
                        db.SubmitChanges();
                    }

                    //パスワード変更が成功したらセッション変数に格納
                    Session["Employee"] = employee;
                    Session["TaskList"] = new TaskDao(db).GetIdListByEmployeeCode(employee.EmployeeCode);


                    //パスワード変更画面に戻る
                    ViewData["ChangedPassword"] = true;
                    return View("ChangePassword");
            }
        }

          /// <summary>
        /// トップページ遷移
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/06/19 arc yano #3867 ログイン画面　新システムからActive Directoryのユーザアカウントのパスワード変更機能の追加 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ChangeTransfer()
        {
            //トップページを表示
            return View("Top");
        }

        ///// <summary>
        ///// ログイン認証処理
        ///// </summary>
        ///// <param name="userid">ユーザーID</param>
        ///// <param name="password">パスワード</param>
        ///// <returns></returns>
        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Index(string userid, string password)
        //{
        //    ViewData["userid"] = userid;

        //    //Add 2014/09/04 arc amii 変更履歴追加対応 #3081 テキストファイルから履歴を取得・設定する
        //    GetRevisionHistory();

        //    //Validationチェック
        //    if (!ValidateLogOn(userid, password))
        //    {
        //        return View();
        //    }

        //    //フォームに入力された情報でユーザーを検索する
        //    Employee employee = (new EmployeeDao(db)).GetByKey(userid);

        //    //DBにユーザーが存在しなければエラー
        //    if (employee == null)
        //    {
        //        ModelState.AddModelError("", "ログオン失敗: ユーザー名を認識できないか、またはパスワードが間違っています。");
        //        return View();
        //    }

        //    // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
        //    db = new CrmsLinqDataContext();
        //    db.Log = new OutputWriter();

        //    #region ドメインユーザー認証
        //    // 2014/05/08 vs2012対応 arc yano warning対応(ConfigurationSettingsクラスは古い.netバージョン(v1.0,1.1)用であるため、変更する。)
        //    //string domainAuth = ConfigurationSettings.AppSettings["DomainAuth"];
        //    string domainAuth = ConfigurationManager.AppSettings["DomainAuth"];
        //    if (!string.IsNullOrEmpty(domainAuth) && domainAuth.Equals("true"))
        //    {
        //        //LdapAuthentication ldAuth = new LdapAuthentication("LDAP://" + ConfigurationSettings.AppSettings["DomainName"]);
        //        LdapAuthentication ldAuth = new LdapAuthentication("LDAP://" + ConfigurationManager.AppSettings["DomainName"]);
        //        try
        //        {
        //            //if (!ldAuth.IsAuthenticated(ConfigurationSettings.AppSettings["DomainName"], userid, password)) {
        //            if (!ldAuth.IsAuthenticated(ConfigurationManager.AppSettings["DomainName"], userid, password))
        //            {
        //                ModelState.AddModelError("", "ログオン失敗: ユーザー名を認識できないか、またはパスワードが間違っています。");
        //                return View();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            //Add 2014/08/12 arc amii エラーログ対応 Exception発生時、ログ出力する処理追加
        //            // セッションにSQL文を登録
        //            Session["ExecSQL"] = OutputLogData.sqlText;
        //            // ログに出力
        //            OutputLogger.NLogError(ex, PROC_NAME, FORM_NAME, "");

        //            ModelState.AddModelError("", ex.Message);
        //            return View();
        //        }
        //    }
        //    #endregion

        //    employee.LastLoginDateTime = DateTime.Now;
        //    db.SubmitChanges();

        //    //認証成功したらセッション変数に格納
        //    Session["Employee"] = employee;
        //    Session["TaskList"] = new TaskDao(db).GetIdListByEmployeeCode(employee.EmployeeCode);

        //    //トップページを表示
        //    return View("Top");
        //}

        /// <summary>
        /// ヘッダ情報表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/04/09 arc yano #3757 キャッシュ問題
        /// </history>
        public ActionResult Header()
        {
            SetComponent();         //Add 2018/04/09 arc yano #3757

            return View("Header");
        }

        /// <summary>
        /// メニューフレームの表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/04/09 arc yano #3757 キャッシュ問題
        /// </history>
        public ActionResult Menu()
        {
            List<MenuGroup> list = new MenuGroupDao(db).GetListAll();

            SetComponent();         //Add 2018/04/09 arc yano #3757   

            return View("Menu", list);
        }
        
        /// <summary>
        /// トップ画面を表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Top()
        {
            return View();
        }
        
        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool ValidateLogOn(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError("userid", "ユーザーIDを入力してください");
            }
            if (String.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("password", "パスワードを入力してください");
            }

            return ModelState.IsValid;
        }

        /// <summary>
        /// パスワード変更時のValidationチェック
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool ValidateChangePassword(string oldpwd, string newpwd, string confirmpwd)
        {
            if (String.IsNullOrEmpty(oldpwd))
            {
                ModelState.AddModelError("oldpwd", "現在のパスワードを入力して下さい");
            }
            if (String.IsNullOrEmpty(newpwd))
            {
                ModelState.AddModelError("newpwd", "新しいパスワードを入力してください");
            }
            if (String.IsNullOrEmpty(confirmpwd))
            {
                ModelState.AddModelError("confirmpwd", "新しいパスワード(確認)を入力してください");
            }
            //新しいパスワードと新しいパスワード(確認)が異なっていた場合
            if (!String.IsNullOrEmpty(newpwd) && !String.IsNullOrEmpty(confirmpwd) && !newpwd.Equals(confirmpwd))
            {
                ModelState.AddModelError("newpwd", "新しいパスワードと新しいパスワード(確認)が一致するように入力してください");
                ModelState.AddModelError("confirmpwd", "");
            }

            return ModelState.IsValid;
        }

        /// <summary>
        /// タスクの残数と新規タスク追加フラグの取得（Ajax用）
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTaskCount() {
            if (Request.IsAjaxRequest()) {
                if (Session["Employee"] != null) {
                    List<Guid> sessionTaskList = (List<Guid>)Session["TaskList"];
                    List<Guid> taskList = new TaskDao(db).GetIdListByEmployeeCode(((Employee)Session["Employee"]).EmployeeCode);
                    Dictionary<string, string> ret = new Dictionary<string, string>();
                    string cnt = taskList.Count.ToString();
                    string flag = "0";
                    foreach (var t in taskList) {
                        if (sessionTaskList.IndexOf(t) < 0) {
                            flag = "1";
                            continue;
                        }
                    }

                    Session["TaskList"] = taskList;
                    ret.Add("Count", cnt);
                    ret.Add("Flag", flag);
                    return Json(ret);
                }
            }
            return new EmptyResult();
        }

        //Add 2014/09/04 arc amii 変更履歴追加対応 #3081 テキストファイルから履歴を取得・設定する
        /// <summary>
        /// テキストファイルから変更履歴情報を取得し、ログイン画面に表示する
        /// </summary>
        private void GetRevisionHistory()
        {
            string history = "";  // 履歴
            string filePath = ""; // テキストファイルパス
            string ChangePlan = "";  // 変更予告
            string CPfilePath = ""; // テキストファイルパス
            System.IO.StreamReader reader = null;
            System.IO.StreamReader CPreader = null;

            // web.configからパスを取得
            filePath = ConfigurationManager.AppSettings["History"];
            CPfilePath = ConfigurationManager.AppSettings["ChangePlan"];
            ViewData["history"] = "";
            ViewData["ChangePlan"] = "";

            //変更履歴と変更予告
            // Mod 2015/10/08 arc nakayama #3271_変更予告画面の追加
            if (string.IsNullOrWhiteSpace(filePath) == true || string.IsNullOrWhiteSpace(CPfilePath) == true)
            {
                // web.configに設定されていなかった場合、取得せず終了
                return;
            }
            else if (System.IO.File.Exists(filePath) == false || System.IO.File.Exists(CPfilePath) == false)
            {
                // ファイルが存在しなかった場合、取得せず終了
                return;
            }

            // テキストファイルを開く
            reader = new System.IO.StreamReader(filePath, System.Text.Encoding.GetEncoding("shift_jis"));
            CPreader = new System.IO.StreamReader(CPfilePath, System.Text.Encoding.GetEncoding("shift_jis"));

            // 読み込み
            history = reader.ReadToEnd();
            ChangePlan = CPreader.ReadToEnd();
            //取得した情報を変更履歴項目に設定する
            ViewData["history"] = history;
            ViewData["ChangePlan"] = ChangePlan;
            reader.Close();
            CPreader.Close();
            
            reader = null;
            CPreader = null;
        }

        #region システム変更履歴(予告)編集
        public ActionResult EditSystemHistoryCriteria()
        {
            return EditSystemHistoryCriteria(new FormCollection());
        }

        /// <summary>
        /// システム変更履歴(予告)編集
        /// </summary>
        /// <param name="form">フォーム入力値</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditSystemHistoryCriteria(FormCollection form)
        {

            GetRevisionHistory();


            return View("EditSystemHistoryCriteria");
        }
#endregion


        /// <summary>
        /// 画面項目の設定
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/04/09 arc yano #3757 キャッシュ問題 新規作成
        /// </history>
        private void SetComponent()
        {
            ViewData["QueryString"] = "?version=" + CommonUtils.GetSystemTime();
        }

    }
}
