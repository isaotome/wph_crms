using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// イベントテーブルアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CampaignController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "イベントマスタ";     // 画面名
        private static readonly string PROC_NAME = "イベント登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CampaignController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// イベント検索画面表示
        /// </summary>
        /// <returns>イベント検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// イベント検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>イベント検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Campaign> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CampaignCode"] = form["CampaignCode"];
            ViewData["CampaignName"] = form["CampaignName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["CampaignStartDateFrom"] = form["CampaignStartDateFrom"];
            ViewData["CampaignStartDateTo"] = form["CampaignStartDateTo"];
            ViewData["CampaignEndDateFrom"] = form["CampaignEndDateFrom"];
            ViewData["CampaignEndDateTo"] = form["CampaignEndDateTo"];

            // イベント検索画面の表示
            return View("CampaignCriteria", list);
        }

        /// <summary>
        /// イベント検索ダイアログ表示
        /// </summary>
        /// <returns>イベント検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// イベント検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>イベント検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CampaignCode"] = Request["CampaignCode"];
            form["CampaignName"] = Request["CampaignName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["CampaignStartDateFrom"] = Request["CampaignStartDateFrom"];
            form["CampaignStartDateTo"] = Request["CampaignStartDateTo"];
            form["CampaignEndDateFrom"] = Request["CampaignEndDateFrom"];
            form["CampaignEndDateTo"] = Request["CampaignEndDateTo"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Campaign> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CampaignCode"] = form["CampaignCode"];
            ViewData["CampaignName"] = form["CampaignName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["CampaignStartDateFrom"] = form["CampaignStartDateFrom"];
            ViewData["CampaignStartDateTo"] = form["CampaignStartDateTo"];
            ViewData["CampaignEndDateFrom"] = form["CampaignEndDateFrom"];
            ViewData["CampaignEndDateTo"] = form["CampaignEndDateTo"];

            // イベント検索画面の表示
            return View("CampaignCriteriaDialog", list);
        }

        /// <summary>
        /// イベントテーブル入力画面表示
        /// </summary>
        /// <param name="id">イベントコード(更新時のみ設定)</param>
        /// <returns>イベントテーブル入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Campaign campaign;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                campaign = new Campaign();
                campaign.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                campaign = new CampaignDao(db).GetByKey(id);
            }

            // その他表示データの取得
            GetEntryViewData(campaign);

            // 出口
            return View("CampaignEntry", campaign);
        }

        /// <summary>
        /// イベントテーブル追加更新
        /// </summary>
        /// <param name="campaign">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>イベントテーブル入力画面</returns>
        /// <history>
        /// 2017/02/14 arc yano #3641 金額欄のカンマ表示対応 ModelState.Clear()対応
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Campaign campaign, EntitySet<CampaignCar> line, FormCollection form)
        {

            // Add 2017/02/14 arc yano #3641
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }

            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // モデルデータの紐付け
            campaign.CampaignCar = line;

            // 行追加及び行削除処理
            string delLine = form["DelLine"];
            if (!string.IsNullOrEmpty(delLine))
            {
                // 指定行削除
                if (Int32.Parse(delLine) >= 0)
                {
                    campaign.CampaignCar.RemoveAt(Int32.Parse(delLine));
                }
                // 行追加
                else
                {
                    campaign.CampaignCar.Add(new CampaignCar());
                }

                // その他表示データの取得
                GetEntryViewData(campaign);

                // 出口
                ModelState.Clear();
                return View("CampaignEntry", campaign);
            }

            // データチェック
            ValidateCampaign(campaign);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(campaign);
                return View("CampaignEntry", campaign);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // Mod 2014/08/05 arc amii エラーログ対応 追加・更新・削除の各メソッド内のSubmitChangesを一か所に集約
            using (TransactionScope ts = new TransactionScope())
            {
                // データ更新処理
                if (form["update"].Equals("1"))
                {
                    Campaign targetCampaign = new CampaignDao(db).GetByKey(campaign.CampaignCode);

                    // イベントテーブルの論理削除
                    if (form["action"].Equals("delete"))
                    {
                        LogicalDeleteCampaign(targetCampaign);
                    }
                    // イベントテーブル及びイベント対象車両テーブルのデータ編集・更新
                    else
                    {
                        targetCampaign.CampaignCar = campaign.CampaignCar;
                        UpdateCampaign(targetCampaign);
                    }
                }
                // データ追加処理
                else
                {
                    // イベントテーブル及びイベント対象車両テーブルのデータ追加
                    InsertCampaign(campaign);
                }

                // Add 2014/08/05 arc amii エラーログ対応 DB更新処理の追加
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        // DBアクセスの実行
                        db.SubmitChanges();
                        // トランザクションのコミット
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException ce)
                    {
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
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
                    catch (SqlException se)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                            ModelState.AddModelError("CampaignCode", MessageUtils.GetMessage("E0010", new string[] { "イベントコード", "保存" }));
                            GetEntryViewData(campaign);
                            return View("CampaignEntry", campaign);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
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
                }
            }
            
            // 出口
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// イベントコードからイベント名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">イベントコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Campaign campaign = new CampaignDao(db).GetByKey(code);
                if (campaign != null)
                {
                    codeData.Code = campaign.CampaignCode;
                    codeData.Name = campaign.CampaignName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="campaign">モデルデータ</param>
        private void GetEntryViewData(Campaign campaign)
        {
            // ローン名の取得
            if (!string.IsNullOrEmpty(campaign.LoanCode))
            {
                LoanDao loanDao = new LoanDao(db);
                Loan loan = loanDao.GetByKey(campaign.LoanCode);
                if (loan != null)
                {
                    ViewData["LoanName"] = loan.LoanName;
                }
            }

            // 担当者名の取得
            EmployeeDao employeeDao = new EmployeeDao(db);
            Employee employee;
            if (!string.IsNullOrEmpty(campaign.EmployeeCode))
            {
                employee = employeeDao.GetByKey(campaign.EmployeeCode);
                if (employee != null)
                {
                    campaign.Employee = employee;
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
            }

            //セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["TargetServiceList"] = CodeUtils.GetSelectListByModel(dao.GetTargetServiceAll(false), campaign.TargetService, true);
            ViewData["CampaignTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCampaignTypeAll(false), campaign.CampaignType, true);

            // グレードリストの取得
            List<CarGrade> carGradeList = new List<CarGrade>();
            if (campaign.CampaignCar != null && campaign.CampaignCar.Count > 0)
            {
                CarGradeDao carGradeDao = new CarGradeDao(db);
                CarGrade carGrade;
                for (int i = 0; i < campaign.CampaignCar.Count; i++)
                {
                    carGrade = carGradeDao.GetByKey(campaign.CampaignCar[i].CarGradeCode);
                    if (carGrade == null)
                    {
                        carGradeList.Add(new CarGrade());
                    }
                    else
                    {
                        carGradeList.Add(carGrade);
                    }
                }
            }
            ViewData["CarGradeList"] = carGradeList;
        }

        /// <summary>
        /// イベントテーブル検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>イベントテーブル検索結果リスト</returns>
        private PaginatedList<Campaign> GetSearchResultList(FormCollection form)
        {
            CampaignDao campaignDao = new CampaignDao(db);
            Campaign campaignCondition = new Campaign();
            campaignCondition.CampaignCode = form["CampaignCode"];
            campaignCondition.CampaignName = form["CampaignName"];
            campaignCondition.CampaignStartDateFrom = CommonUtils.StrToDateTime(form["CampaignStartDateFrom"], DaoConst.SQL_DATETIME_MAX);
            campaignCondition.CampaignStartDateTo = CommonUtils.StrToDateTime(form["CampaignStartDateTo"], DaoConst.SQL_DATETIME_MIN);
            campaignCondition.CampaignEndDateFrom = CommonUtils.StrToDateTime(form["CampaignEndDateFrom"], DaoConst.SQL_DATETIME_MAX);
            campaignCondition.CampaignEndDateTo = CommonUtils.StrToDateTime(form["CampaignEndDateTo"], DaoConst.SQL_DATETIME_MIN);
            campaignCondition.Employee = new Employee();
            campaignCondition.Employee.EmployeeName = form["EmployeeName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                campaignCondition.DelFlag = form["DelFlag"];
            }

            return campaignDao.GetListByCondition(campaignCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="campaign">イベントデータ</param>
        /// <returns>イベントデータ</returns>
        private Campaign ValidateCampaign(Campaign campaign)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(campaign.CampaignCode))
            {
                ModelState.AddModelError("CampaignCode", MessageUtils.GetMessage("E0001", "イベントコード"));
            }
            if (string.IsNullOrEmpty(campaign.CampaignName))
            {
                ModelState.AddModelError("CampaignName", MessageUtils.GetMessage("E0001", "イベント名"));
            }
            if (string.IsNullOrEmpty(campaign.TargetService))
            {
                ModelState.AddModelError("TargetService", MessageUtils.GetMessage("E0001", "対象業務"));
            }
            if (string.IsNullOrEmpty(campaign.EmployeeCode))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "担当者"));
            }
            if (campaign.CampaignCar != null && campaign.CampaignCar.Count > 0)
            {
                bool alreadyOutputNotNullMsg = false;
                for (int i = 0; i < campaign.CampaignCar.Count; i++)
                {
                    if (string.IsNullOrEmpty(campaign.CampaignCar[i].CarGradeCode))
                    {
                        if (alreadyOutputNotNullMsg)
                        {
                            ModelState.AddModelError("line[" + CommonUtils.IntToStr(i) + "].CarGradeCode", "");
                        }
                        else
                        {
                            ModelState.AddModelError("line[" + CommonUtils.IntToStr(i) + "].CarGradeCode", MessageUtils.GetMessage("E0001", "グレードコード"));
                            alreadyOutputNotNullMsg = true;
                        }
                    }
                }
            }

            // 属性チェック
            if (!ModelState.IsValidField("PublishStartDate"))
            {
                ModelState.AddModelError("PublishStartDate", MessageUtils.GetMessage("E0005", "掲載開始日"));
            }
            if (!ModelState.IsValidField("PublishEndDate"))
            {
                ModelState.AddModelError("PublishEndDate", MessageUtils.GetMessage("E0005", "掲載終了日"));
            }
            if (!ModelState.IsValidField("CampaignStartDate"))
            {
                ModelState.AddModelError("CampaignStartDate", MessageUtils.GetMessage("E0005", "イベント開始日"));
            }
            if (!ModelState.IsValidField("CampaignEndDate"))
            {
                ModelState.AddModelError("CampaignEndDate", MessageUtils.GetMessage("E0005", "イベント終了日"));
            }
            if (!ModelState.IsValidField("Cost"))
            {
                ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { "費用", "正の整数のみ" }));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("CampaignCode") && !CommonUtils.IsAlphaNumeric(campaign.CampaignCode))
            {
                ModelState.AddModelError("CampaignCode", MessageUtils.GetMessage("E0012", "イベントコード"));
            }
            if (ModelState.IsValidField("Cost") && campaign.Cost != null)
            {
                if (!Regex.IsMatch(campaign.Cost.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { "費用", "正の整数のみ" }));
                }
            }

            // 重複チェック
            if (ModelState.IsValid && campaign.CampaignCar != null && campaign.CampaignCar.Count > 0)
            {
                bool alreadyOutputDuplicateMsg = false;
                for (int i = 0; i < campaign.CampaignCar.Count - 1; i++)
                {
                    for (int j = i + 1; j < campaign.CampaignCar.Count; j++)
                    {
                        //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                        if (CommonUtils.DefaultString(campaign.CampaignCar[i].CarGradeCode).Equals(CommonUtils.DefaultString(campaign.CampaignCar[j].CarGradeCode)))
                        {
                            if (alreadyOutputDuplicateMsg)
                            {
                                ModelState.AddModelError("line[" + CommonUtils.IntToStr(i) + "].CarGradeCode", "");
                                ModelState.AddModelError("line[" + CommonUtils.IntToStr(j) + "].CarGradeCode", "");
                            }
                            else
                            {
                                ModelState.AddModelError("line[" + CommonUtils.IntToStr(i) + "].CarGradeCode", MessageUtils.GetMessage("E0006", "グレードコード"));
                                ModelState.AddModelError("line[" + CommonUtils.IntToStr(j) + "].CarGradeCode", "");
                                alreadyOutputDuplicateMsg = true;
                            }
                        }
                    }
                }
            }

            return campaign;
        }

        /// <summary>
        /// イベントテーブル追加
        ///   ※イベント対象車両テーブルも同時に追加される。
        /// </summary>
        /// <param name="campaign">イベントデータ</param>
        private void InsertCampaign(Campaign campaign)
        {
            // データ編集
            campaign = EditCampaignForInsert(campaign);

            // データ追加
            db.Campaign.InsertOnSubmit(campaign);
            //db.SubmitChanges();
        }

        /// <summary>
        /// イベントテーブル更新
        ///   ※イベント対象車両テーブルも同時に削除＋追加される。
        /// </summary>
        /// <param name="campaign">イベントデータ</param>
        private void UpdateCampaign(Campaign campaign)
        {
            //using (TransactionScope ts = new TransactionScope())
            //{
            // イベント対象車両テーブルの削除
            List<CampaignCar> campaignCarList = new CampaignCarDao(db).GetByCampaign(campaign.CampaignCode);
            foreach (CampaignCar campaignCar in campaignCarList)
            {
                db.CampaignCar.DeleteOnSubmit(campaignCar);
            }

            // イベントテーブルのデータ編集・更新
            // 及びイベント対象車両テーブルのデータ編集・追加
            UpdateModel(campaign);
            EditCampaignForUpdate(campaign);

            //    // DBアクセスの実行
            //    db.SubmitChanges();

            //    // トランザクションのコミット
            //    ts.Complete();
            //}
        }

        /// <summary>
        /// イベントテーブル論理削除
        /// </summary>
        /// <param name="campaign">イベントデータ</param>
        private void LogicalDeleteCampaign(Campaign campaign)
        {
            // イベントテーブルの論理削除
            EditCampaignForLogicalDelete(campaign);
            //for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            //{
            //    EditCampaignForLogicalDelete(campaign);
            //    try
            //    {
            //        db.SubmitChanges();
            //        break;
            //    }
           
            //}
        }

        /// <summary>
        /// イベントテーブル追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="campaign">イベントデータ(登録内容)</param>
        /// <returns>イベントテーブルモデルクラス</returns>
        private Campaign EditCampaignForInsert(Campaign campaign)
        {
            campaign.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            campaign.CreateDate = DateTime.Now;
            campaign.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            campaign.LastUpdateDate = DateTime.Now;
            campaign.DelFlag = "0";
            if (campaign.CampaignCar != null && campaign.CampaignCar.Count > 0)
            {
                foreach (CampaignCar campaignCar in campaign.CampaignCar)
                {
                    campaignCar.CreateEmployeeCode = campaign.CreateEmployeeCode;
                    campaignCar.CreateDate = campaign.CreateDate;
                    campaignCar.LastUpdateEmployeeCode = campaign.LastUpdateEmployeeCode;
                    campaignCar.LastUpdateDate = campaign.LastUpdateDate;
                    campaignCar.DelFlag = "0";
                }
            }
            return campaign;
        }

        /// <summary>
        /// イベントテーブル更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="campaign">イベントデータ(登録内容)</param>
        /// <returns>イベントテーブルモデルクラス</returns>
        private Campaign EditCampaignForUpdate(Campaign campaign)
        {
            campaign.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            campaign.LastUpdateDate = DateTime.Now;
            if (campaign.CampaignCar != null && campaign.CampaignCar.Count > 0)
            {
                foreach (CampaignCar campaignCar in campaign.CampaignCar)
                {
                    campaignCar.CreateEmployeeCode = campaign.LastUpdateEmployeeCode;
                    campaignCar.CreateDate = campaign.LastUpdateDate;
                    campaignCar.LastUpdateEmployeeCode = campaign.LastUpdateEmployeeCode;
                    campaignCar.LastUpdateDate = campaign.LastUpdateDate;
                    campaignCar.DelFlag = "0";
                }
            }
            return campaign;
        }

        /// <summary>
        /// イベントテーブル論理削除データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="campaign">イベントデータ(登録内容)</param>
        /// <returns>イベントテーブルモデルクラス</returns>
        private Campaign EditCampaignForLogicalDelete(Campaign campaign)
        {
            campaign.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            campaign.LastUpdateDate = DateTime.Now;
            campaign.DelFlag = "1";
            return campaign;
        }
    }
}
