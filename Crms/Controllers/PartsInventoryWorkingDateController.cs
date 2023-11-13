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
using Microsoft.VisualBasic;
using Crms.Models;
namespace Crms.Controllers
{
    /// <summary>
    /// 棚卸基準日設定コントローラクラス
    /// </summary>
    /// <history>
    /// 2017/05/10 arc yano #3762 車両棚卸機能追加　棚卸基準日設定を車両・部品で共用する
    /// </history>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsInventoryWorkingDateController : Controller
    {
		//Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④ 棚卸作業日⇒棚卸基準日　に変更
        private static readonly string FORM_NAME = "棚卸基準日登録画面";     // 画面名（ログ出力用）
        private static readonly string PROC_NAME_INSERT = "棚卸基準日登録";  // 処理名：登録（ログ出力用）
        
        
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsInventoryWorkingDateController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 当月のステータス一覧を表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();
            return Criteria(form);
        }

        /// <summary>
        /// 指定月のステータス一覧を表示
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 車両の棚卸状況も追加
        /// 2015/04/22 arc nakayama 部品棚卸作業日登録画面修正 最新確定月の次の年月を表示
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {

            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_INSERT);

            List<PartsInventoryWorkingDate> list = new PartsInventoryWorkingDateDao(db).GetAll();

            //-----------------------------------------
            //部品棚卸ステータスが確定の最新月１件取得
            //-----------------------------------------
            DateTime? TargetInventoryMonthParts = null;

            InventoryMonthControlParts rec = new InventoryMonthControlPartsDao(db).GetLatestCloseInventoryMonth();

            if (rec != null)
            {
                TargetInventoryMonthParts = DateTime.ParseExact(rec.InventoryMonth, "yyyyMMdd", null);
            }
           
            //Add 2017/05/10 arc yano #3762
            //-----------------------------------------
            //車両棚卸ステータスが確定の最新月１件取得
            //-----------------------------------------
            DateTime? TargetInventoryMonthCar = null;
            
            InventoryMonthControlCar ret = new InventoryMonthControlCarDao(db).GetLatestCloseInventoryMonth();

            if (ret != null)
            {
                TargetInventoryMonthCar = DateTime.ParseExact(ret.InventoryMonth, "yyyyMMdd", null);
            }
            else
            {   //確定月が取得できなかった場合は、確定していない月のレコードがあるかをチェックし、あった場合はその前月を設定する。
                InventoryMonthControlCar aret = new InventoryMonthControlCarDao(db).GetLatestInventoryMonth();

                if (aret != null)
                {
                    TargetInventoryMonthCar = DateTime.ParseExact(aret.InventoryMonth, "yyyyMMdd", null).AddDays(-1);
                }
            }

            DateTime TargetInventoryMonth;

            //車両の棚卸確定最新月と部品の棚卸確定最新月を比較して古い方を採用する。
            if (TargetInventoryMonthCar == null)
            {
                TargetInventoryMonth = (TargetInventoryMonthParts ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
            }
            else if (TargetInventoryMonthParts == null)
            {
                TargetInventoryMonth = (TargetInventoryMonthCar ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
            }
            else
            {
                TargetInventoryMonth = (DateTime)((TargetInventoryMonthCar <= TargetInventoryMonthParts) ? TargetInventoryMonthCar : TargetInventoryMonthParts);
            }

            //Mod 2017/05/10 arc yano #3762
            //対象年月の棚卸ステータス取得
            //Mod 2015/04/22 arc nakayama 部品棚卸作業日登録画面修正 最新確定月の次の月の棚卸状況が未実施なら[登録]ボタンを活性にする
            string strTargetDate = TargetInventoryMonth.ToString(string.Format("{0:yyyyMMdd}", TargetInventoryMonth.AddMonths(1)));
            
            //部品棚卸状況
            InventoryMonthControlParts TargetMonthStatusParts = new InventoryMonthControlPartsDao(db).GetByKey(strTargetDate);

            //車両棚卸状況
            InventoryMonthControlCar TargetMonthStatusCar = new InventoryMonthControlCarDao(db).GetByKey(strTargetDate);


            if (TargetMonthStatusParts == null && TargetMonthStatusCar == null)
            {
                ViewData["EntryButton"] = "1";
            }
            else
            {
                ViewData["EntryButton"] = "0";
            }

           
            if (criteriaInit)
            {
                // 検索画面の表示(初回表示)
                //最新締め月の次の月
                ViewData["TargetInventoryMonth"] = TargetInventoryMonth.AddMonths(1);
                return View("PartsInventoryWorkingDateCriteria", list);
            }
            else
            {
                //入力チェック
                ValidateSearch(form);
                if (!ModelState.IsValid)
                {
                    // 検索画面の表示
                    SetDataComponent(form);
                    return View("PartsInventoryWorkingDateCriteria", list);
                }
                
                //削除
                delWorkingDate();
                //更新
                InsertWorkingDate(form);
                
                list = new PartsInventoryWorkingDateDao(db).GetAll();
                
            }
            
            // 部品在庫確認画面の表示
            SetDataComponent(form);
            return View("PartsInventoryWorkingDateCriteria", list);
        }

        private void SetDataComponent(FormCollection form)
        {
            //Mod 2015/04/22 arc nakayama 部品棚卸作業日登録画面修正 最新確定月の次の年月を表示 
            //登録作業日のセット
            ViewData["InputInventoryWorkingDate"] = form["InputInventoryWorkingDate"]; //部品棚卸作業日(yyyy/MM/dd)
            ViewData["TargetInventoryMonth"] = form["TargetInventoryMonth"];    //対象年月(yyyy/MM）
        }


        /*  //Mod 2017/05/10 arc yano 未使用のため、コメントアウト
        /// <summary>
        /// 部品棚卸状況取得
        /// </summary>
        /// <param name="form">全件検索</param>
        /// <returns>部品棚卸リスト</returns>
        private InventoryMonthControlParts GetInventoryMonthControlPartsList()
        {
            InventoryMonthControlPartsDao InventoryMonthControlPartsDao = new CrmsDao.InventoryMonthControlPartsDao(db);
            InventoryMonthControlParts InventoryMonthControlParts = new InventoryMonthControlParts();

            //部品棚卸最新月１レコード取得
            InventoryMonthControlParts = InventoryMonthControlPartsDao.GetLatestInventoryMonth();

            return InventoryMonthControlParts;
            

        }
        */


        /// <summary>
        /// 部品棚卸作業日を削除(更新のため削除を行う処理)
        /// </summary>
        /// <returns>なし</returns>
        private void  delWorkingDate()
        {
            List<PartsInventoryWorkingDate> delWorkingDateList = new PartsInventoryWorkingDateDao(db).GetAll();
             
            foreach (var wd in delWorkingDateList)
            {
                db.PartsInventoryWorkingDate.DeleteOnSubmit(wd);
                db.SubmitChanges();
            }
            


        }

        /// <summary>
        /// 部品棚卸作業日を更新する(更新処理)
        /// </summary>
        /// <returns>なし</returns>
        private void InsertWorkingDate(FormCollection form)
        {
            PartsInventoryWorkingDate WorkingDate = new PartsInventoryWorkingDate();
            
            CodeDao dao = new CodeDao(db);
            DateTime TargetDate = DateTime.Parse(form["InputInventoryWorkingDate"]);
            DateTime InventoryMonth = DateTime.Parse(form["TargetInventoryMonth"]);

            WorkingDate.InventoryWorkingDate = TargetDate;
            WorkingDate.InventoryMonth = InventoryMonth;
            db.PartsInventoryWorkingDate.InsertOnSubmit(WorkingDate);
            db.SubmitChanges();

        }


        #region 入力チェック
        /// <summary>
        /// 検索時の入力チェック
        /// </summary>
        /// <param name="form"></param>
        /// <history>
        /// 2019/01/18 yano #3965 WE版新システム対応（Web.configによる処理の分岐)
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 車両の棚卸状況も追加のため、文言、チェック対象の変更
        /// </history>
        private void ValidateSearch(FormCollection form)
        {
            ModelState.Clear();

            //対象日付 必須チェック
            if (form["InputInventoryWorkingDate"] == null || form["InputInventoryWorkingDate"] =="")
            {
            	//Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④ 棚卸作業日⇒棚卸基準日　に変更
                ModelState.AddModelError("InputInventoryWorkingDate", MessageUtils.GetMessage("E0001", "棚卸基準日"));   //Mod  2017/05/10 arc yano #3762
                return;
            }

            //過去日チェック、当日以前ならエラーにする
            DateTime TargetDate = DateTime.ParseExact(form["InputInventoryWorkingDate"], "yyyy/MM/dd", null).Date;
            DateTime ToDate = DateTime.Today.Date;
            if (TargetDate < ToDate)
            {
            	//Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④ 棚卸作業日⇒棚卸基準日　に変更
                ModelState.AddModelError("InputInventoryWorkingDate", MessageUtils.GetMessage("E0028", "棚卸基準日"));
            }

            //-----------------------
            //部品の棚卸状況の取得
            //-----------------------
            //対象年月が実施中かどうか
            TargetDate = DateTime.ParseExact(form["TargetInventoryMonth"] + "/01", "yyyy/MM/dd", null).Date;
            
            InventoryMonthControlParts InventoryMonthParts = new InventoryMonthControlPartsDao(db).GetLatestInventoryMonth();

            //Mod 2019/01/18 yano #3965
            DateTime inventoryDateParts = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);     //初期値として当月1日を設定する
            if (InventoryMonthParts != null)
            {
                inventoryDateParts = DateTime.ParseExact(InventoryMonthParts.InventoryMonth, "yyyyMMdd", null);
            }
            //Add 2017/05/10 arc yano #3762
            //-----------------------
            //車両の棚卸状況の取得
            //-----------------------
            InventoryMonthControlCar InventoryMonthCar = new InventoryMonthControlCarDao(db).GetLatestInventoryMonth();

            DateTime inventoryDateCar = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);     //初期値として当月1日を設定する

            if (InventoryMonthCar != null)
            {
                inventoryDateCar = DateTime.ParseExact(InventoryMonthCar.InventoryMonth, "yyyyMMdd", null);
            }

            //InventryMonthControlPartsの最新月＝当該年月、かつ、InventryMonthControlPartsのステータスが実施中(001)だった場合 ⇒エラー、そうでなければ更新
            if (TargetDate.Year == inventoryDateParts.Year && TargetDate.Month == inventoryDateParts.Month && (InventoryMonthParts != null ? InventoryMonthParts.InventoryStatus : "") == "001")     //Mod 2019/01/18 yano #3965
            {
                //エラー
                ModelState.AddModelError("InputInventoryWorkingDate", MessageUtils.GetMessage("E0025", ""));
            }
            else if (TargetDate.Year == inventoryDateCar.Year && TargetDate.Month == inventoryDateCar.Month && (InventoryMonthCar != null ? InventoryMonthCar.InventoryStatus : "") == "001")
            {
                //エラー
                ModelState.AddModelError("InputInventoryWorkingDate", MessageUtils.GetMessage("E0025", ""));
            }

            //対象年月が確定かどうか
            //部品
            if (TargetDate.Year == inventoryDateParts.Year && TargetDate.Month == inventoryDateParts.Month && (InventoryMonthParts != null ? InventoryMonthParts.InventoryStatus : "") == "002")  //Mod 2019/01/18 yano #3965
            {
                //エラー
                ModelState.AddModelError("InputInventoryWorkingDate", MessageUtils.GetMessage("E0029", ""));
            }
            //車両
            else if (TargetDate.Year == inventoryDateCar.Year && TargetDate.Month == inventoryDateCar.Month && ((InventoryMonthCar != null ? InventoryMonthCar.InventoryStatus : "") == "002" || (InventoryMonthCar != null ? InventoryMonthCar.InventoryStatus : "") == "003"))
            {
                //エラー
                ModelState.AddModelError("InputInventoryWorkingDate", MessageUtils.GetMessage("E0029", ""));
            }

            //前月が確定かどうか
            TargetDate = DateTime.ParseExact(form["TargetInventoryMonth"] + "/01", "yyyy/MM/dd", null).AddMonths(-1).Date;
            TargetDate = new DateTime(TargetDate.Year, TargetDate.Month, 1);
            string strTargetDate = TargetDate.ToString(string.Format("{0:yyyyMMdd}", TargetDate));

            //部品の棚卸状況
            //Mod 2019/01/18 yano #3965
            string PrevMonthStatusParts = ( new InventoryMonthControlPartsDao(db).GetByKey(strTargetDate) != null ? new InventoryMonthControlPartsDao(db).GetByKey(strTargetDate).InventoryStatus : "");

            //車両の棚卸状況
            string PrevMonthStatusCar =  ( new InventoryMonthControlCarDao(db).GetByKey(strTargetDate) != null ? new InventoryMonthControlCarDao(db).GetByKey(strTargetDate).InventoryStatus : "");

            if (PrevMonthStatusParts != "002" && PrevMonthStatusCar != "003")
            {
                ModelState.AddModelError("InputInventoryWorkingDate", MessageUtils.GetMessage("E0027", ""));
            }

        }
        #endregion

    }
}