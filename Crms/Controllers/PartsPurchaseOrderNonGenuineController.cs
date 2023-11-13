using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Transactions;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Crms.Models;
using System.Data.Linq;
using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;
using System.Text;

/*-----------------------------------------------------------------------------------------------
 * 機能：部品発注入力画面(社外品)に関するアクション
 *     ：部品発注入力(PartsPurchaseOrderController)からの派生クラス
 * 更新履歴：
 *   2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 　新規作成
 * ---------------------------------------------------------------------------------------------*/
namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsPurchaseOrderNonGenuineController : PartsPurchaseOrderController
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsPurchaseOrderNonGenuineController(): base()
        {
            FORM_NAME = "部品発注入力(社外)";                 // 画面名
            PROC_NAME = "部品発注登録(社外)";                 // 処理名
            PROC_NAME_EXCELDOWNLOAD = "Excel出力(社外)";      // 処理名

            gGenuine = "002";                   //純正区分 =　「社外」

            viewName = "PartsPurchaseOrderNonGenuineEntry";
            
        }

        #region 発注データの編集
        /// <summary>
        /// 部品発注データの編集(社外品専用)
        /// </summary>
        /// <param name="orderList">部品発注データ</param>
        /// <param name="form">フォームの入力値</param>
        /// <returns></returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力　社外品のオーダー区分の変更
        /// 2015/11/09 arc yano #3291 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        protected override List<PartsPurchaseOrder> SetOrderList(List<PartsPurchaseOrder> orderList, FormCollection form)
        {
            DateTime workDate;
            bool ret;
            
            if (orderList != null && orderList.Count > 0)
            {
                for (int i = 0; i < orderList.Count; i++)
                {
                    //-------------------------------------------
                    //共通部分はformから設定
                    //-------------------------------------------
                    //受注伝票番号
                    orderList[i].ServiceSlipNumber = form["ServiceSlipNumber"];
                    //発注日
                    ret = DateTime.TryParse(form["PurchaseOrderDate"], out workDate);
                    orderList[i].PurchaseOrderDate = !string.IsNullOrWhiteSpace(form["PurchaseOrderDate"]) ? (Nullable<DateTime>)DateTime.Parse(form["PurchaseOrderDate"].ToString()) : null;
                    //発注ステータス
                    orderList[i].PurchaseOrderStatus = form["PurchaseOrderStatus"];
                    //部門コード
                    orderList[i].DepartmentCode = form["DepartmentCode"];
                    //担当者コード
                    orderList[i].EmployeeCode = form["EmployeeCode"];
                    
                    //-----------------------------------------------
                    //個別の部分は新規の場合のみ設定が必要
                    //-----------------------------------------------
                    //発注番号
                    if(string.IsNullOrWhiteSpace(orderList[i].PurchaseOrderNumber))
                    {
                        orderList[i].PurchaseOrderNumber = new SerialNumberDao(db).GetNewPartsPurchaseOrderNumber();
                    }
                    if (!string.IsNullOrWhiteSpace(orderList[i].SupplierCode))
                    {
                        //仕入先の設定
                        if (string.IsNullOrWhiteSpace(orderList[i].SupplierName))
                        {
                            orderList[i].SupplierName = new SupplierDao(db).GetByKey(orderList[i].SupplierCode) != null ? new SupplierDao(db).GetByKey(orderList[i].SupplierCode).SupplierName : "";
                        }
                        //支払先の設定
                        if (string.IsNullOrWhiteSpace(orderList[i].SupplierPaymentName))
                        {
                            orderList[i].SupplierPaymentName = new SupplierPaymentDao(db).GetByKey(orderList[i].SupplierCode) != null ? new SupplierPaymentDao(db).GetByKey(orderList[i].SupplierCode).SupplierPaymentName : "";
                        }
                    }
                    //オーダー区分
                    orderList[i].OrderType = (orderList[i].OrderType ?? "010");         //オーダー区分はE/Oで登録する
                    
                }
            }

            return orderList;
        }
        #endregion

        /// <summary>
        /// 発注処理時のValidateチェック
        /// </summary>
        /// <param name="form">フォーム入力値</param>
        /// <param name="order">発注情報</param>
        /// <return></return>
        /// <history>
        /// Add 2015/11/09 arc yano #3291 新規作成
        /// </history>
        protected override void ValidatePurchaseOrder(FormCollection form, PartsPurchaseOrder line, int lineNo)
        {
            string prefix = "line[" + lineNo + "].";        //コントロールのnameprefix
            
            //---------------------
            //必須チェック
            //---------------------
            //仕入先
            if (string.IsNullOrEmpty(line.SupplierCode))
            {
                ModelState.AddModelError(prefix + "SupplierCode", MessageUtils.GetMessage("E0001", "仕入先"));
            }
            //支払先
            if (string.IsNullOrEmpty(line.SupplierPaymentCode))
            {
                ModelState.AddModelError(prefix + "SupplierPaymentCode", MessageUtils.GetMessage("E0001", "支払先"));
            }
            //発注日
            if (string.IsNullOrWhiteSpace(form["PurchaseOrderDate"]))
            {
                ModelState.AddModelError("PurchaseOrderDate", MessageUtils.GetMessage("E0009", new string[] { "発注処理", "発注日" }));
            }
            //到着予定日
            if (line.ArrivalPlanDate == null)
            {
                ModelState.AddModelError(prefix + "ArrivalPlanDate", MessageUtils.GetMessage("E0009", new string[] { "発注処理", "入荷予定日" }));
            }
            //支払予定日
            if (line.PaymentPlanDate == null)
            {
                ModelState.AddModelError(prefix + "PaymentPlanDate", MessageUtils.GetMessage("E0009", new string[] { "発注処理", "支払予定日" }));
            }

            return;
        }
        /// <summary>
        /// Excelファイルのダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="line">発注データ明細</param> 
        /// <returns>Excelデータ</returns>
        ///<history>
        /// 2017/11/07 arc yano #3806 部品発注入力　１０行ボタンの追加
        /// 2015/11/09 arc yano #3291 新規作成
        /// </history>
        public override ActionResult Download(FormCollection form, List<PartsPurchaseOrder> line)
        {
            //ファイルパス
            string filePath = ConfigurationManager.AppSettings["TemporaryExcelExportPartPurchaseOrder"] ?? "";

            //テンプレートファイルパス取得
            string tfilePathName = ConfigurationManager.AppSettings["TemplateForPartsPurchaseOrder"] ?? "";

            string fileName = "";       //ファイル名
            string filePathName = "";   //ファイルパス＋ファイル名
            string zipName = "PartsPurchaseOrder" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".zip";        //圧縮ファイル

            //ファイル圧縮用クラス
            var zip = new Ionic.Zip.ZipFile();  
            //メモリストリーム
            var ms = new MemoryStream();

            DataExport ds = new DataExport();
            
            //-------------------------------
            //初期処理
            //-------------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCELDOWNLOAD);

            ModelState.Clear();

            //データ成形
            line = SetOrderList(line, form);
            line = SetOrderListForExcel(line, form);    //Excel出力用

            //Mod 2017/11/07 arc yano #3806
            if (line == null)
            {
                line = new List<PartsPurchaseOrder>();
            }
                foreach (var l in line)
                {
                    //-------------------------------
                    //データ成形
                    //-------------------------------
                    List<PartsPurchaseOrder> list = new List<PartsPurchaseOrder>();
                    list.Add(l);

                    //-------------------------------
                    //Excel出力処理
                    //-------------------------------         
                    //ファイル名(PartsPurchaseOrder_xxx(発注番号)_yyyyMMddhhmiss(ダウンロード時刻))
                    fileName = "PartsPurchaseOrder" + "_" + l.PurchaseOrderNumber + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

                    filePathName = filePath + fileName;

                    //テンプレートファイルのパスが設定されていない場合
                    if (tfilePathName.Equals(""))
                    {
                        ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                        SetDataComponent(form);
                        return View(viewName, list);
                    }

                    //エクセルデータ作成
                    byte[] excelData = MakeExcelData(form, list, filePathName, tfilePathName);

                    if (!ModelState.IsValid)
                    {
                        SetDataComponent(form);
                        return View(viewName, list);
                    }

                    //作成したExcelファイルをzipリストに追加
                    zip.AddEntry(fileName, excelData);
                }

            zip.Save(ms);

            ms.Position = 0;

            return File(ms, System.Net.Mime.MediaTypeNames.Application.Zip, zipName);           
        }
    }
}
