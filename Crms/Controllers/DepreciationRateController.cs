using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.Linq;
using Crms.Models;
using System.Transactions;
using OfficeOpenXml;
using Microsoft.VisualBasic;

namespace Crms.Controllers
{

    /// <summary>
    /// 償却率マスタコントローラー
    /// </summary>
    /// <history>
    /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
    /// </history>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class DepreciationRateController : InheritedController
    {
        private static readonly string FORM_NAME = "償却率マスタ";                 　// 画面名
        private static readonly string PROC_NAME = "償却率マスタ登録";             // 処理名(登録)
        private static readonly string PROC_NAME_EXCELUPLOAD = "償却率マスタ登録"; // 処理名(Excel取込)

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DepreciationRateController()
        {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 償却率マスタ検索画面表示
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 償却率マスタ検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>償却率マスタ検索結果</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            DepreciationRate condition = new DepreciationRate();
            PaginatedList<DepreciationRate> list = new DepreciationRateDao(db).GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            return View("DepreciationRateCriteria", list);
        }
     
        /// <summary>
        /// 償却率マスタ入力画面表示
        /// </summary>
        /// <param name="usefulLives">耐用年数</param>
        /// <returns>償却率マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(int? UsefulLives)
        {
            DepreciationRate rec = new DepreciationRate();

            if (UsefulLives != null)
            {
                rec = new DepreciationRateDao(db).GetByKey(UsefulLives ?? 0);
                ViewData["update"] = "1";
            }
            else
            {
                ViewData["update"] = "0";
            }
            return View("DepreciationRateEntry", rec);
        }

        /// <summary>
        /// 償却率マスタ登録処理
        /// </summary>
        /// <param name="rec">償却率データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>償却率マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(DepreciationRate rec, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }

            ViewData["update"] = form["update"];
            ValidateDepreciationRate(rec);
            if (!ModelState.IsValid)
            {
                return View("DepreciationRateEntry", rec);
            }

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            if (form["update"].Equals("1"))
            {
                DepreciationRate target = new DepreciationRateDao(db).GetByKey(rec.UsefulLives);
                UpdateModel(target);
                EditForUpdate(target);
            }
            else
            {
                EditForInsert(rec);
                db.DepreciationRate.InsertOnSubmit(rec);
            }

            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    // DBアクセスの実行
                    db.SubmitChanges();
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
                        ModelState.AddModelError("UsefulLIves", MessageUtils.GetMessage("E0010", new string[] { "耐用年数", "保存" }));
                        return View("DepreciationRateEntry", rec);
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

            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }

            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            ViewData["update"] = "1";
            return View("DepreciationRateEntry", rec);
        }

        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="depreciationRate">償却率マスタデータ</param>
        private void ValidateDepreciationRate(DepreciationRate rec)
        {
            //-----------------------
            //耐用年数
            //-----------------------
            if (!ModelState.IsValidField("UsefulLives"))
            {
                ModelState["UsefulLives"].Errors.Clear();
            }

            if (rec.UsefulLives <= 0)
            {
                ModelState.AddModelError("UsefulLives", MessageUtils.GetMessage("E0004", new string[] { "耐用年数", "0以外正の整数3桁以内" }));
            }

            //----------------------
            //償却率
            //----------------------

            //必須・フォーマットチェック
            if (!ModelState.IsValidField("Rate") || rec.Rate == null)
            {
                ModelState["Rate"].Errors.Clear();
                ModelState.AddModelError("Rate", MessageUtils.GetMessage("E0004", new string[] { "償却率", "正の整数1桁以内かつ小数6桁以内" }));
            }
            //----------------------
            //改訂償却率
            //----------------------
            if (!ModelState.IsValidField("RevisedRate"))
            {
                ModelState["RevisedRate"].Errors.Clear();
                ModelState.AddModelError("RevisedRate", MessageUtils.GetMessage("E0004", new string[] { "改訂償却率", "正の整数1桁以内かつ小数6桁以内" }));
            }
            //----------------------
            //償却保障率
            //----------------------
            if (!ModelState.IsValidField("SecurityRatio"))
            {
                ModelState["SecurityRatio"].Errors.Clear();
                ModelState.AddModelError("SecurityRatio", MessageUtils.GetMessage("E0004", new string[] { "償却保障率", "正の整数1桁以内かつ小数6桁以内" }));
            }
        }

        /// <summary>
        /// 償却率マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="rec">償却率マスタデータ</param>
        /// <returns>償却率マスタデータ</returns>
        private DepreciationRate EditForUpdate(DepreciationRate rec)
        {
            rec.LastUpdateDate = DateTime.Now;
            rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            return rec;
        }

        /// <summary>
        /// 償却率マスタマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="rec">償却率マスタデータ</param>
        /// <returns>償却率マスタデータ</returns>
        private DepreciationRate EditForInsert(DepreciationRate rec)
        {
            rec.CreateDate = DateTime.Now;
            rec.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            rec.DelFlag = "0";
            return EditForUpdate(rec);
        }

        #region Excel取込処理
        /// <summary>
        /// Excel取込用ダイアログ表示
        /// </summary>
        [AuthFilter]
        public ActionResult ImportDialog()
        {
            List<DepreciationRate> ImportList = new List<DepreciationRate>();
            FormCollection form = new FormCollection();

            ViewData["ErrFlag"] = "1";

            return View("DepreciationRateImportDialog", ImportList);
        }

        /// <summary>
        /// Excel読み込み
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <param name="form">フォームデータ</param>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, FormCollection form)
        {
            List<DepreciationRateExcelImport> ImportList = new List<DepreciationRateExcelImport>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                //--------------
                //Excel読み込み
                //--------------
                case "1":
                    //Excel読み込み前のチェック
                    ValidateExcelFile(importFile, form);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("DepreciationRateImportDialog", ImportList);
                    }

                    //Excel読み込み
                    ImportList = ReadExcelData(importFile, ImportList);

                    //読み込み時に何かエラーがあればここでリターン
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("DepreciationRateImportDialog");
                    }

                    //Excelで読み込んだデータのバリデートチェック
                    ValidateImportList(ImportList, form);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("DepreciationRateImportDialog");
                    }

                    //DB登録
                    DBExecute(ImportList, form);

                    form["ErrFlag"] = "1";
                    SetDialogDataComponent(form);
                    return View("DepreciationRateImportDialog");

                //--------------
                //キャンセル
                //--------------
                case "2":
                    ImportList = new List<DepreciationRateExcelImport>();
                    ViewData["ErrFlag"] = "1";//[取り込み]ボタンが押せないようにするため
                    SetDialogDataComponent(form);
                    return View("DepreciationRateImportDialog", ImportList);

                //----------------------------------
                //その他(ここに到達することはない)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("DepreciationRateImportDialog");
            }
        }
        #region Excelデータ取得&設定
        /// Excelデータ取得&設定
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <param name="ImportList">ImportList</param>
        /// <returns>なし</returns>
        private List<DepreciationRateExcelImport> ReadExcelData(HttpPostedFileBase importFile, List<DepreciationRateExcelImport> ImportList)
        {
            using (var pck = new ExcelPackage())
            {
                try
                {
                    pck.Load(importFile.InputStream);
                }
                catch (System.IO.IOException ex)
                {
                    if (ex.Message.Contains("because it is being used by another process."))
                    {
                        ModelState.AddModelError("importFile", "対象のファイルが開かれています。ファイルを閉じてから、再度実行して下さい");
                        return ImportList;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("importFile", "エラーが発生しました。" + ex.Message);
                    return ImportList;
                }

                //-----------------------------
                // データシート取得
                //-----------------------------
                var ws = pck.Workbook.Worksheets[1];

                //--------------------------------------
                //読み込むシートが存在しなかった場合
                //--------------------------------------
                if (ws == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。確認して再度実行して下さい"));
                    return ImportList;
                }
                //------------------------------
                //読み込み行が0件の場合
                //------------------------------
                if (ws.Dimension == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。更新処理を終了しました"));
                    return ImportList;
                }

                //読み取りの開始位置と終了位置を取得
                int StartRow = ws.Dimension.Start.Row;　       //行の開始位置
                int EndRow = ws.Dimension.End.Row;             //行の終了位置
                int StartCol = ws.Dimension.Start.Column;      //列の開始位置
                int EndCol = ws.Dimension.End.Column;          //列の終了位置

                var headerRow = ws.Cells[StartRow, StartCol, StartRow, EndCol];

                //タイトル行、ヘッダ行がおかしい場合は即リターンする
                if (!ModelState.IsValid)
                {
                    return ImportList;
                }

                //------------------------------
                // 読み取り処理
                //------------------------------
                int datarow = 0;
                string[] Result = new string[ws.Dimension.End.Column];

                for (datarow = StartRow + 3; datarow < EndRow + 1; datarow++)
                {
                    //更新データの取得
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)
                    {
                        Result[col - 1] = !string.IsNullOrWhiteSpace(ws.Cells[datarow, col].Text) ? Strings.StrConv(ws.Cells[datarow, col].Text.Trim(), VbStrConv.Narrow, 0x0411).ToUpper() : "";
                    }

                    //----------------------------------------
                    // 読み取り結果を画面の項目にセットする
                    //----------------------------------------
                    ImportList = SetProperty(ref Result, ref ImportList);
                }
            }
            return ImportList;

        }
        #endregion

        #region Excel読み取り前のチェック
        /// <summary>
        /// Excel読み取り前のチェック
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        private void ValidateExcelFile(HttpPostedFileBase filePath, FormCollection form)
        {
            // 必須チェック
            if (filePath == null || string.IsNullOrEmpty(filePath.FileName))
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルを選択してください"));
            }
            else
            {
                // 拡張子チェック
                System.IO.FileInfo cFileInfo = new System.IO.FileInfo(filePath.FileName);
                string stExtension = cFileInfo.Extension;

                if (stExtension.IndexOf("xlsx") < 0)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルの拡張子がxlsxファイルではありません"));
                }
            }

            return;
        }
        #endregion

        #region 読み込み結果のバリデーションチェック
        /// <summary>
        /// 読み込み結果のバリデーションチェック
        /// </summary>
        /// <param name="ImportList">Excelデータ</param>
        /// <param name="form">フォーム入力値</param>
        /// <returns>なし</returns>
        public void ValidateImportList(List<DepreciationRateExcelImport> ImportList, FormCollection form)
        {

            for (int i = 0; i < ImportList.Count; i++)
            {
                //----------------
                //耐用年数
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].UsefulLives))  //必須チェック
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目の耐用年数が入力されていません。耐用年数"));
                }
                else
                {
                    int usefulLives = 0;

                    bool ret = int.TryParse(ImportList[i].UsefulLives, out usefulLives);

                    //整数値の変換に失敗した場合
                    if (ret != true && usefulLives <= 0)
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("", i + 1 + "行目の耐用年数には正の整数を入力して下さい"));
                    }
                }

                //----------------
                //償却率
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].Rate))  //必須チェック
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目の償却率が入力されていません。償却率"));
                }
                else
                {
                    //フォーマットチェック
                    if (!string.IsNullOrEmpty(ImportList[i].Rate) && !Regex.IsMatch(ImportList[i].Rate, @"^\d{1}\.\d{1,3}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage(MessageUtils.GetMessage("E0004", new string[] { "償却率", "正の整数1桁以内かつ少数3桁以内" })));
                    }
                }

                //----------------
                //改訂償却率
                //----------------
                //改訂償却率が入力されていて、
                if (!string.IsNullOrEmpty(ImportList[i].RevisedRate) && !Regex.IsMatch(ImportList[i].RevisedRate, @"^\d{1}\.\d{1,3}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage(MessageUtils.GetMessage("E0004", new string[] { "改訂償却率", "正の整数1桁以内かつ少数3桁以内" })));
                }

                //----------------
                //償却保証率
                //----------------
                //償却保証率が入力されていて、
                if (!string.IsNullOrEmpty(ImportList[i].SecurityRatio) && !Regex.IsMatch(ImportList[i].SecurityRatio, @"^\d{1}\.\d{1,5}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage(MessageUtils.GetMessage("E0004", new string[] { "償却保障率", "正の整数1桁以内かつ少数5桁以内" })));
                }
            }


            //-----------------
            //重複チェック
            //-----------------
            var rec = ImportList.GroupBy(x => x.UsefulLives).Select(c => new { UsefulLives = c.Key, Count = c.Count() }).Where(c => c.Count > 1);

            foreach (var a in rec)
            {
                if (!string.IsNullOrWhiteSpace(a.UsefulLives))
                {
                    ModelState.AddModelError("", "取込むファイルの中に耐用年数" + a.UsefulLives + "が複数定義されています");
                }
            }
        }
        #endregion

        #region Excelの読み取り結果をリストに設定する
        /// <summary>
        /// 結果をリストに設定する
        /// </summary>
        /// <param name="Result">Excelセルの値</param>
        /// <param name="ImportList">Excelセルの値の保存先</param>
        /// <returns></returns>
        public List<DepreciationRateExcelImport> SetProperty(ref string[] Result, ref List<DepreciationRateExcelImport> ImportList)
        {
            DepreciationRateExcelImport SetLine = new DepreciationRateExcelImport();

            // 耐用年数
            SetLine.UsefulLives = Result[0];
            // 償却率
            SetLine.Rate = Result[1];
            // 改訂償却率
            SetLine.RevisedRate= Result[2];
            // 償却保障率
            SetLine.SecurityRatio = Result[3];

            ImportList.Add(SetLine);

            return ImportList;
        }
        #endregion

        #region ダイアログのデータ付きコンポーネント設定
        /// <summary>
        /// ダイアログのデータ付きコンポーネント設定
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private void SetDialogDataComponent(FormCollection form)
        {
            ViewData["ErrFlag"] = form["ErrFlag"];
            ViewData["RequestFlag"] = form["RequestFlag"];
            ViewData["id"] = form["id"];
        }
        #endregion

        #region 読み込んだデータをDBに登録
        /// <summary>
        /// DB更新
        /// </summary>
        /// <returns>戻り値(0:正常 1:エラー(償却率マスタ取込画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        private void DBExecute(List<DepreciationRateExcelImport> ImportList, FormCollection form)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                //償却率の更新
                foreach (var LineData in ImportList)
                {
                    DepreciationRate rec = new DepreciationRateDao(db).GetByKey(int.Parse(LineData.UsefulLives), true);

                    DepreciationRate editData = rec;

                    //該当の耐用年数の償却率マスタに存在する
                    if (editData == null)
                    {
                        //償却率マスタ
                        editData = new DepreciationRate();
                        //耐用年数
                        editData.UsefulLives = int.Parse(LineData.UsefulLives);
                        //作成者
                        editData.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        //作成日
                        editData.CreateDate = DateTime.Now;
                        
                        db.DepreciationRate.InsertOnSubmit(editData);
                    }

                    //償却率
                    editData.Rate = decimal.Parse(LineData.Rate);
                    //改訂償却率
                    editData.RevisedRate = !string.IsNullOrWhiteSpace(LineData.RevisedRate) ? decimal.Parse(LineData.RevisedRate) : (Nullable<decimal>)(null);
                    //償却保障率
                    editData.SecurityRatio =  !string.IsNullOrWhiteSpace(LineData.SecurityRatio) ? decimal.Parse(LineData.SecurityRatio) : (Nullable<decimal>)(null);
                    //最終更新者
                    editData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    //最終更新日
                    editData.LastUpdateDate = DateTime.Now;
                    //削除フラグ
                    editData.DelFlag = "0";
                }
                try
                {
                    db.SubmitChanges();
                    ts.Complete();

                    //取り込み完了のメッセージを表示する
                    ModelState.AddModelError("", "取り込みが完了しました。");
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME_EXCELUPLOAD, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_EXCELUPLOAD, FORM_NAME, "");
                    ts.Dispose();
                }
            }
        }
        #endregion
        #endregion
    }
}
