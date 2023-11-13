using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Data.SqlClient;
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

namespace Crms.Controllers
{
    /// <summary>
    /// メーカーマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class MakerController : Controller
    {

        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "メーカーマスタ";     // 画面名
        private static readonly string PROC_NAME = "メーカーマスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;
        protected bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MakerController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// メーカー検索画面表示
        /// </summary>
        /// <returns>メーカー検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// メーカー検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>メーカー検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Maker> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // メーカー検索画面の表示
            return View("MakerCriteria", list);
        }

        /// <summary>
        /// メーカー検索ダイアログ表示
        /// </summary>
        /// <returns>メーカー検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// メーカー検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>メーカー検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["MakerCode"] = Request["MakerCode"];
            form["MakerName"] = Request["MakerName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Maker> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];

            // メーカー検索画面の表示
            return View("MakerCriteriaDialog", list);
        }

        /// <summary>
        /// メーカーマスタ入力画面表示
        /// </summary>
        /// <param name="id">メーカーコード(更新時のみ設定)</param>
        /// <returns>メーカーマスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Maker maker;

            // 表示データ設定(追加の場合)
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                maker = new Maker();
            }
            // 表示データ設定(更新の場合)
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                maker = new MakerDao(db).GetByKey(id, true);
            }

            // 出口
            return View("MakerEntry", maker);
        }

        /// <summary>
        /// メーカーマスタ追加更新
        /// </summary>
        /// <param name="maker">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>メーカーマスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Maker maker, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateMaker(maker);
            if (!ModelState.IsValid)
            {
                return View("MakerEntry", maker);
            }

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                Maker targetMaker = new MakerDao(db).GetByKey(maker.MakerCode, true);
                UpdateModel(targetMaker);
                EditMakerForUpdate(targetMaker);
            }
            // データ追加処理
            else
            {
                // データ編集
                maker = EditMakerForInsert(maker);

                // データ追加
                db.Maker.InsertOnSubmit(maker);
            }

            // Add 2014/08/04 arc amii エラーログ対応 submitChangeを一本化 + エラーログ出力
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException e)
                {
                    foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                    {
                        occ.Resolve(RefreshMode.KeepCurrentValues);
                    }
                    if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                    {
                        //Mod 2014/08/04 arc amii エラーログ対応 『theow e』からエラーページ遷移に変更
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (SqlException e)
                {
                    //Add 2014/08/04 arc amii エラーログ対応 エラーログ出力処理追加
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ログに出力
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0010", new string[] { "メーカーコード", "保存" }));
                        return View("MakerEntry", maker);
                    }
                    else
                    {
                        //Mod 2014/08/04 arc amii エラーログ対応 『theow e』からエラーページ遷移に変更
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
                    //Add 2014/08/04 arc amii エラーログ対応 ChangeConflictException以外の時のエラー処理追加
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
                    return View("Error");
                }
            }
            //MOD 2014/10/24 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(maker.MakerCode);
        }

        /// <summary>
        /// メーカーコードからメーカー名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">メーカーコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Maker maker = new MakerDao(db).GetByKey(code);
                if (maker != null)
                {
                    codeData.Code = maker.MakerCode;
                    codeData.Name = maker.MakerName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }



        /// <summary>
        /// メーカーマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>メーカーマスタ検索結果リスト</returns>
        private PaginatedList<Maker> GetSearchResultList(FormCollection form)
        {
            MakerDao makerDao = new MakerDao(db);
            Maker makerCondition = new Maker();
            makerCondition.MakerCode = form["MakerCode"];
            makerCondition.MakerName = form["MakerName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                makerCondition.DelFlag = form["DelFlag"];
            }
            return makerDao.GetListByCondition(makerCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="maker">メーカーデータ</param>
        /// <returns>メーカーデータ</returns>
        private Maker ValidateMaker(Maker maker)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(maker.MakerCode))
            {
                ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0001", "メーカーコード"));
            }
            if (string.IsNullOrEmpty(maker.MakerName))
            {
                ModelState.AddModelError("MakerName", MessageUtils.GetMessage("E0001", "メーカー名"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("MakerCode") && !CommonUtils.IsAlphaNumeric(maker.MakerCode))
            {
                ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0012", "メーカーコード"));
            }

            return maker;
        }

        /// <summary>
        /// メーカーマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="maker">メーカーデータ(登録内容)</param>
        /// <returns>メーカーマスタモデルクラス</returns>
        private Maker EditMakerForInsert(Maker maker)
        {
            maker.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            maker.CreateDate = DateTime.Now;
            maker.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            maker.LastUpdateDate = DateTime.Now;
            maker.DelFlag = "0";
            return maker;
        }

        /// <summary>
        /// メーカーマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="maker">メーカーデータ(登録内容)</param>
        /// <returns>メーカーマスタモデルクラス</returns>
        private Maker EditMakerForUpdate(Maker maker)
        {
            maker.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            maker.LastUpdateDate = DateTime.Now;
            return maker;
        }


        //Add 2015/01/21 arc nakayama  顧客DM指摘事項　車種名の一覧をメーカーコードから取得
        /// <summary>
        /// メーカー名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">なし</param>
        /// <returns>メーカー一覧</returns>
        public void GetCarMasterList(string code, string code2)
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();
                List<V_CarMaster> CarList = new List<V_CarMaster>();
                if (!string.IsNullOrEmpty(code))
                {
                    CarList = new V_CarMasterDao(db).GetListBykey(code);
                }
                else
                {
                    if (code2.Equals("0")){
                        CarList = new V_CarMasterDao(db).GetCarListBykey(null, null);
                    }else{
                        CarList = new V_CarMasterDao(db).GetCarListBykey(null, code2);
                    }
                }

                Maker maker = new MakerDao(db).GetByKey(code);
                if (maker == null)
                {
                    maker = new Maker();
                    codeDataList.Code = "";
                    codeDataList.Name = "";
                }
                else
                {
                    codeDataList.Code = maker.MakerCode;
                    codeDataList.Name = maker.MakerName;
                }
                codeDataList.DataList.Add(new CodeData { Code = "", Name = "" });
                foreach (var car in CarList)
                {
                    codeDataList.DataList.Add(new CodeData { Code = car.CarCode, Name = car.CarName });
                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, codeDataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
        }

        
        //Add 2015/02/27 arc nakayama  車種名の一覧を全件取得
        /// <summary>
        /// 車種名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">なし</param>
        /// <returns>車種一覧</returns>
        public void GetCarMasterListAll()
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();
                List<V_CarMaster> CarList = new List<V_CarMaster>();
                
               CarList = new V_CarMasterDao(db).GetCarListBykey(null);
                
                Maker maker = new MakerDao(db).GetByKey(null);
                if (maker == null)
                {
                    maker = new Maker();
                    codeDataList.Code = "";
                    codeDataList.Name = "";
                }
                else
                {
                    codeDataList.Code = maker.MakerCode;
                    codeDataList.Name = maker.MakerName;
                }
                codeDataList.DataList.Add(new CodeData { Code = "", Name = "" });
                foreach (var car in CarList)
                {
                    codeDataList.DataList.Add(new CodeData { Code = car.CarCode, Name = car.CarName });
                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, codeDataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
        }

        //GetPrivateCarList

        //Add 2015/02/27 arc nakayama  車種名の一覧を全件取得
        /// <summary>
        /// 車種名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">なし</param>
        /// <returns>車種一覧</returns>
        public void GetPrivateCarList(string PrivateFlag)
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();
                List<V_CarMaster> CarList = new List<V_CarMaster>();

                CarList = new V_CarMasterDao(db).GetCarListBykey(null, PrivateFlag);

                Maker maker = new MakerDao(db).GetByKey(null);
                if (maker == null)
                {
                    maker = new Maker();
                    codeDataList.Code = "";
                    codeDataList.Name = "";
                }
                else
                {
                    codeDataList.Code = maker.MakerCode;
                    codeDataList.Name = maker.MakerName;
                }
                codeDataList.DataList.Add(new CodeData { Code = "", Name = "" });
                foreach (var car in CarList)
                {
                    codeDataList.DataList.Add(new CodeData { Code = car.CarCode, Name = car.CarName });
                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, codeDataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
        }



        //Add 2015/01/21 arc nakayama  顧客DM指摘事項　メーカ名の一覧を取得
        /// <summary>
        /// メーカー名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">なし</param>
        /// <returns>メーカー一覧</returns>
        public void GetMakerMasterList(string PrivateFlag = null)
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();
                List<V_CarMaster> MakerList = new V_CarMasterDao(db).GetPrivateListBykey(null, PrivateFlag);
                codeDataList.Code = "1";
                codeDataList.Name = "1";
                codeDataList.DataList.Add(new CodeData { Code = "", Name = "" });
                foreach (var Maker in MakerList)
                {
                    codeDataList.DataList.Add(new CodeData { Code = Maker.MakerCode, Name = Maker.MakerName });
                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, codeDataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
        }

        /// <summary>
        /// メーカー検索ダイアログ表示
        /// </summary>
        /// <returns>メーカー検索ダイアログ</returns>
        public ActionResult CriteriaDialog2()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            //デフォルトでログイン担当者の会社コードをセット
            Employee employee = (Employee)Session["Employee"];
            string companyCode = "";
            try { companyCode = employee.Department1.Office.CompanyCode; }
            catch (NullReferenceException) { }
            //form["CompanyCode"] = companyCode;

            return CriteriaDialog2(form);
        }

        //Add 2015/01/21 arc nakayama  顧客DM指摘事項　メーカ名から車種を検索した一覧を取得
        /// <summary>
        /// メーカー検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>メーカー検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog2(FormCollection form)
        {
            // 検索結果リストの取得
            PaginatedList<V_CarMaster> list;
            if (criteriaInit)
            {
                list = new PaginatedList<V_CarMaster>();
                ViewData["PrivateFlag"] = "0";
            }
            else
            {
                list = GetSearchResultListForDialog(form);
            }

            // その他出力項目の設定
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["CarCode"] = form["CarCode"];
            ViewData["PrivateFlag"] = form["PrivateFlag"];

            List<CodeData> data = new List<CodeData>();
            List<Maker> MakerList = new MakerDao(db).GetMakerBykey();
            foreach (var item in MakerList)
            {
                data.Add(new CodeData { Code = item.MakerCode, Name = item.MakerName });
            }
            ViewData["MakerList"] = CodeUtils.GetSelectListByModel(data, form["MakerCode"], false);

            List<CodeData> carData = new List<CodeData>();
            if (form["MakerCode"] != null)
            {
                List<V_CarMaster> CarMaster = new V_CarMasterDao(db).GetListBykey(form["MakerCode"]);
                foreach (var car in CarMaster)
                {
                    carData.Add(new CodeData { Code = car.CarCode, Name = car.CarName });
                }
            }
            ViewData["CarList"] = CodeUtils.GetSelectListByModel(carData, form["CarCode"], false);


            // グレード検索画面の表示
            return View("MakerCriteriaDialog2", list);
        }

        private PaginatedList<V_CarMaster> GetSearchResultListForDialog(FormCollection form)
        {
            V_CarMaster V_CarMasterCondition = new V_CarMaster();

            V_CarMasterCondition.MakerCode = form["MakerCode"];
            V_CarMasterCondition.CarCode = form["CarCode"];

            //自社取りつかいのメーカのみを出力するか、全データを出力するかを分ける
            if (form["PrivateFlag"] == "0"){
                return new V_CarMasterDao(db).GetListByCondition(V_CarMasterCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE); 
            }
            else{
                return new V_CarMasterDao(db).GetPrivateListByCondition(V_CarMasterCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            }
        }

        /// <summary>
        /// メーカーコードから車両情報を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">メーカーコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMasterDetail(string code)
        {

            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                V_CarMaster CarMaster = new V_CarMasterDao(db).GetBykey(code);
                if (CarMaster != null && CarMaster.MakerDelFlag.Equals("0"))
                {
                    ret.Add("MakerName", CarMaster.CarGradeCode);
                    ret.Add("CarName", CarMaster.CarGradeName);
                }
                return Json(ret);
            }
            return new EmptyResult();
        }

    }
}
