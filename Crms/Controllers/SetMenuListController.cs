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
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers {

    /// <summary>
    /// セットメニューマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SetMenuListController : Controller {

        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "セットメニューリストマスタ";     // 画面名
        private static readonly string PROC_NAME = "セットメニューリストマスタ登録"; // 処理名
        private string errorFlg = ""; // エラー画面遷移情報 0:なし 1:元画面 2：エラー画面

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SetMenuListController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 入金消込検索画面表示
        /// </summary>
        /// <returns>入金消込検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new List<SetMenuList>(), new FormCollection());
        }

        /// <summary>
        /// セットメニュー検索画面表示
        /// </summary>
        /// <param name="line">モデルデータ(複数件の登録内容)</param>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>入金消込検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(List<SetMenuList> line, FormCollection form) {

            List<SetMenuList> list = null;

            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["action"] = (form["action"] == null ? "" : form["action"]);

            // 操作ボタンによる処理制御
            switch (form["action"]) {

                // 明細行追加及び行削除処理
                case "line":

                    if (line == null) {
                        line = new List<SetMenuList>();
                    }

                    string delLine = form["DelLine"];
                    if (!string.IsNullOrEmpty(delLine)) {

                        // 指定行削除または最終行追加
                        if (Int32.Parse(delLine) >= 0) {
                            line.RemoveAt(Int32.Parse(delLine));
                        } else {
                            SetMenuList setMenu = new SetMenuList();
                            setMenu.SetMenuCode = form["SetMenuCode"];

                            //2行目以降なら前行の種類をデフォルトセット
                            if (line.Count > 0 && line[line.Count - 1] != null) {
                                setMenu.ServiceType = line[line.Count - 1].ServiceType;
                            } else {
                                setMenu.ServiceType = "001";
                            }

                            line.Add(setMenu);
                        }

                        // 明細行操作結果を検索結果リストとする
                        list = line;
                    }

                    break;

                // セットメニュー登録処理
                case "regist":

                    if (line != null && line.Count > 0) {

                        // データチェック
                        ValidateSetMenu(line, form);
                        if (!ModelState.IsValid) {
                            GetCriteriaViewData(line, form);
                            return View("SetMenuListCriteria", line);
                        }

                        // データ洗替処理
                        ReplaceSetMenu(line);

                        // Add 2014/08/05 arc amii エラーログ対応 洗替処理でエラーの場合、対応したエラー画面に遷移する
                        if ("1".Equals(errorFlg)) {
                            // セットメニューリスト画面に遷移する
                            GetCriteriaViewData(line, form);
                            return View("SetMenuListCriteria", line);
                        }
                        else if ("2".Equals(errorFlg))
                        {
                            // エラー画面に遷移する
                            return View("Error");
                        }

                    }

                    // 検索結果リストの取得
                    list = GetSearchResultList(form);

                    break;

                // セットメニュー検索処理
                default:

                    // 検索結果リストの取得
                    list = GetSearchResultList(form);

                    break;
            }

            // その他出力項目の設定
            GetCriteriaViewData(list, form);

            // 出口
            ModelState.Clear();
            return View("SetMenuListCriteria", list);
        }

        /// <summary>
        /// 検索画面表示データの取得
        /// </summary>
        /// <param name="line">モデルデータ(複数件の登録内容)</param>
        /// <param name="form">フォームデータ(検索条件)</param>
        private void GetCriteriaViewData(List<SetMenuList> line, FormCollection form) {

            CodeDao dao = new CodeDao(db);

            // 検索条件部の画面表示データ取得
            ViewData["SetMenuCode"] = form["SetMenuCode"];
            if (!string.IsNullOrEmpty(form["SetMenuCode"])) {
                SetMenu setMenu = new SetMenuDao(db).GetByKey(form["SetMenuCode"]);
                if (setMenu != null) {
                    ViewData["SetMenuName"] = setMenu.SetMenuName;
                }
            }

            // セットメニュー明細部の画面表示データ取得
            List<IEnumerable<SelectListItem>> serviceTypeList = new List<IEnumerable<SelectListItem>>();
            List<IEnumerable<SelectListItem>> workTypeList = new List<IEnumerable<SelectListItem>>();
            List<IEnumerable<SelectListItem>> autoSetAmountList = new List<IEnumerable<SelectListItem>>();
            List<string> serviceWorkNameList = new List<string>();
            List<string> serviceMenuNameList = new List<string>();
            List<string> partsNameJpList = new List<string>();
            List<c_ServiceType> serviceTypeListSrc = dao.GetServiceTypeAll(false);
            List<c_WorkType> workTypeListSrc = dao.GetWorkTypeAll(false);
            List<c_OnOff> autoSetAmountSrc = dao.GetOnOffAll(false);
            ServiceWorkDao serviceWorkDao = new ServiceWorkDao(db);
            ServiceMenuDao serviceMenuDao = new ServiceMenuDao(db);
            PartsDao partsDao = new PartsDao(db);
            for (int i = 0; i < line.Count; i++) {
                
                //サービス種別リスト
                serviceTypeList.Add(CodeUtils.GetSelectListByModel(serviceTypeListSrc, line[i].ServiceType, false));
                workTypeList.Add(CodeUtils.GetSelectListByModel(workTypeListSrc, line[i].WorkType, true));
                autoSetAmountList.Add(CodeUtils.GetSelectListByModel(autoSetAmountSrc, line[i].AutoSetAmount, false));

                //主作業名
                string serviceWorkName = "";
                if (!string.IsNullOrEmpty(line[i].ServiceWorkCode)) {
                    ServiceWork serviceWork = serviceWorkDao.GetByKey(line[i].ServiceWorkCode);
                    if (serviceWork != null) {
                        serviceWorkName = serviceWork.Name;
                    }
                }
                serviceWorkNameList.Add(serviceWorkName);

                //サービスメニュー名
                string serviceManuName = "";
                if (!string.IsNullOrEmpty(line[i].ServiceMenuCode)) {
                    ServiceMenu serviceMenu = serviceMenuDao.GetByKey(line[i].ServiceMenuCode);
                    if (serviceMenu != null) {
                        serviceManuName = serviceMenu.ServiceMenuName;
                    }
                }
                serviceMenuNameList.Add(serviceManuName);

                //部品名
                string partsNameJp = "";
                if (!string.IsNullOrEmpty(line[i].PartsNumber)) {
                    Parts parts = partsDao.GetByKey(line[i].PartsNumber);
                    if (parts != null) {
                        partsNameJp = parts.PartsNameJp;
                    }
                }

                partsNameJpList.Add(partsNameJp);
            }
            ViewData["ServiceWorkNameList"] = serviceWorkNameList;
            ViewData["ServiceTypeList"] = serviceTypeList;
            ViewData["WorkTypeList"] = workTypeList;
            ViewData["ServiceMenuNameList"] = serviceMenuNameList;
            ViewData["PartsNameJpList"] = partsNameJpList;
            ViewData["AutoSetAmountList"] = autoSetAmountList;
        }

        /// <summary>
        /// セットメニュー検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>入金消込検索結果リスト</returns>
        private List<SetMenuList> GetSearchResultList(FormCollection form) {

            SetMenuList setMenuCondition = new SetMenuList();
            setMenuCondition.SetMenuCode = form["SetMenuCode"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                setMenuCondition.DelFlag = form["DelFlag"];
            }
            return new SetMenuListDao(db).GetListByCondition(setMenuCondition);
        }

        /// <summary>
        /// セットメニュー入力チェック
        /// </summary>
        /// <param name="line">セットメニュー明細データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>入金消込データ</returns>
        private List<SetMenuList> ValidateSetMenu(List<SetMenuList> line, FormCollection form) {

            bool alreadyOutputMsgE0001a = false;
            bool alreadyOutputMsgE0001b = false;
            bool alreadyOutputMsgE0002 = false;
            bool alreadyOutputMsgE0001c = false;
            bool alreadyOutputMsgE0003 = false;

            for (int i = 0; i < line.Count; i++) {

                SetMenuList setMenu = line[i];
                string prefix = "line[" + CommonUtils.IntToStr(i) + "].";
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                if (((CommonUtils.DefaultString(setMenu.ServiceType).Equals("001")) && (!string.IsNullOrEmpty(setMenu.ServiceWorkCode)))
                    || ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("002")) && (!string.IsNullOrEmpty(setMenu.ServiceMenuCode)))
                    || ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("003")) && (!string.IsNullOrEmpty(setMenu.PartsNumber)))
                    || (setMenu.InputDetailsNumber != null)) {

                        //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    // 必須チェック(数値/日付項目は属性チェックを兼ねる)
                    if ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("001")) && (string.IsNullOrEmpty(setMenu.ServiceWorkCode))) {
                        ModelState.AddModelError(prefix + "ServiceWorkCode", (alreadyOutputMsgE0001c ? "" : MessageUtils.GetMessage("E0001", "主作業")));
                        alreadyOutputMsgE0001a = true;
                    }
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("002")) && (string.IsNullOrEmpty(setMenu.ServiceMenuCode))) {
                        ModelState.AddModelError(prefix + "ServiceMenuCode", (alreadyOutputMsgE0001a ? "" : MessageUtils.GetMessage("E0001", "サービスメニュー")));
                        alreadyOutputMsgE0001a = true;
                    }
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("003")) && (string.IsNullOrEmpty(setMenu.PartsNumber))) {
                        ModelState.AddModelError(prefix + "PartsNumber", (alreadyOutputMsgE0001b ? "" : MessageUtils.GetMessage("E0001", "部品")));
                        alreadyOutputMsgE0001b = true;
                    }
                    if (setMenu.InputDetailsNumber == null) {
                        ModelState.AddModelError(prefix + "InputDetailsNumber", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "表示順", "正の整数のみ" })));
                        alreadyOutputMsgE0002 = true;
                    }

                    // 値チェック
                    if (ModelState.IsValidField(prefix + "InputDetailsNumber")) {
                        if (setMenu.InputDetailsNumber < 0) {
                            ModelState.AddModelError(prefix + "InputDetailsNumber", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "表示順", "正の整数のみ" })));
                            alreadyOutputMsgE0002 = true;
                        }
                    }
                    if (setMenu.Quantity != null) {
                        if ((Regex.IsMatch(setMenu.Quantity.ToString(), @"^\d{1,7}\.\d{1,2}$") || (Regex.IsMatch(setMenu.Quantity.ToString(), @"^\d{1,7}$")))) {
                        } else {
                            ModelState.AddModelError(prefix + "Quantity", (alreadyOutputMsgE0003 ? "" : MessageUtils.GetMessage("E0002", new string[] { "数量", "正の整数7桁以内かつ小数2桁以内" })));
                            alreadyOutputMsgE0003 = true;
                        }
                    }
                }
            }

            return line;
        }

        /// <summary>
        /// セットメニュー洗替処理
        /// </summary>
        /// <param name="line">セットメニューデータ</param>
        private void ReplaceSetMenu(List<SetMenuList> line) {

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            
            using (TransactionScope ts = new TransactionScope()) {

                // 既存レコードの削除
                db.RemoveSetMenuList(line[0].SetMenuCode);

                // 新データの編集
                line = EditSetMenuForReplace(line);

                // 新データの登録
                for (int i = 0; i < line.Count; i++) {
                    db.SetMenuList.InsertOnSubmit(line[i]);
                }

                // Add 2014/08/05 arc amii エラーログ対応 ログ出力する為にtry catch文追加
                try
                {
                    errorFlg = "0";
                    // トランザクションのコミット
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");

                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ログに出力
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                        errorFlg = "1";
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        errorFlg = "2";
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
                    errorFlg = "2";
                }
            }
        }

        /// <summary>
        /// セットメニューマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="list">セットメニューデータ全明細(登録内容)</param>
        /// <returns>セットメニューマスタモデルクラス</returns>
        private List<SetMenuList> EditSetMenuForReplace(List<SetMenuList> list) {

            // 入力表示順によるソート
            list.Sort(delegate(SetMenuList x, SetMenuList y) { return (x.InputDetailsNumber ?? 0).CompareTo((y.InputDetailsNumber ?? 0)); });

            // 入力のあった全明細行の追加
            int detailsNumber = 0;
            for (int i = 0; i < list.Count; i++) {

                SetMenuList setMenu = list[i];
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                if (((CommonUtils.DefaultString(setMenu.ServiceType).Equals("001")) && (!string.IsNullOrEmpty(setMenu.ServiceWorkCode)))
                    || ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("002")) && (!string.IsNullOrEmpty(setMenu.ServiceMenuCode)))
                    || ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("003")) && (!string.IsNullOrEmpty(setMenu.PartsNumber)))
                    || (setMenu.InputDetailsNumber != null)) {

                    list[i].DetailsNumber = ++detailsNumber;
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if (CommonUtils.DefaultString(setMenu.ServiceType).Equals("001")) {
                        setMenu.PartsNumber = null;
                        setMenu.ServiceMenuCode = null;
                        setMenu.Comment = null;
                    }
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if (CommonUtils.DefaultString(setMenu.ServiceType).Equals("002")) {
                        setMenu.ServiceWorkCode = null;
                        setMenu.PartsNumber = null;
                        setMenu.Comment = null;
                    }
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if (CommonUtils.DefaultString(setMenu.ServiceType).Equals("003")) {
                        setMenu.ServiceWorkCode = null;
                        setMenu.ServiceMenuCode = null;
                        setMenu.Comment = null;
                    }
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if (CommonUtils.DefaultString(setMenu.ServiceType).Equals("004")) {
                        setMenu.ServiceWorkCode = null;
                        setMenu.ServiceMenuCode = null;
                        setMenu.PartsNumber = null;
                    }
                    setMenu.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    setMenu.CreateDate = DateTime.Now;
                    setMenu.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    setMenu.LastUpdateDate = DateTime.Now;
                    setMenu.DelFlag = "0";
                }
            }

            return list;
        }
    }
}
