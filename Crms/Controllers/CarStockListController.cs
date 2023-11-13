using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;
using System.Data.SqlClient;
using Crms.Models;
using System.Data.Linq;
using System.Transactions;
using OfficeOpenXml;
using System.Configuration;
using System.Data;  //Add 2020/1/20 yano #4033


namespace Crms.Controllers
{
    /// <summary>
    /// 車両在庫表機能コントローラクラス
    /// </summary>
    /// <history>
    /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
    /// </history>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarStockListController : Controller
    {
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
        public CarStockListController()
        {
            db = new CrmsLinqDataContext();
            //タイムアウト値の設定
            db.CommandTimeout = 300;
        }

        private static readonly string FORM_NAME = "車両在庫表";                             // 画面名
        private static readonly string PROC_NAME_SEARCH = "車両在庫検索"; 			         // 処理名（ログ出力用）
        private static readonly string PROC_NAME_EXCELDOWNLOAD = "Excel出力";                // 処理名(Excel出力)
        private static readonly string PROC_NAME_EXCELLISTDOWNLOAD = "画面リスト出力";       // 処理名(画面リスト出力)   //Add 2020/1/20 yano #4033

        private static readonly string CATEGORY_CARSTOCK = "020";                   // カテゴリコード(在庫区分)
        private static readonly string DESCRIPTION_EXCELDOWNLOAD = "( リストに存在しない車両はここから下に入力 管理番号・車体番号は必ず入力してください。管理番号がわからない場合には必ず車体番号を全桁入力してください )";  

        /// <summary>
        /// 検索画面初期表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {

            criteriaInit = true;
            FormCollection form = new FormCollection();
           
            //初期値の設定
            form["TargetRange"] = "0";                                              //0(対象年月指定なし)
            form["DefaultTargetRange"] = form["TargetRange"];                       //0(対象年月指定なし)


            form["StockZeroVisibility"] = "0";
            form["DisplayStockVisility"] = form["StockZeroVisibility"];             //0(在庫有無 = 全て表示)
            form["DefaultDisplayStockVisility"] = form["StockZeroVisibility"];      //0(在庫有無 = 全て表示)
            
            return Criteria(form);
        }

        /// <summary>
        /// 車両在庫画面表示
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2019/12/28 yano #4033【車両在庫表】全部門分の在庫表の出力機能の追加
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {

            ModelState.Clear();

            ActionResult ret = null;

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            PaginatedList<GetStockCarListResult> list = new PaginatedList<GetStockCarListResult>();

            //初回表示の場合
            if (criteriaInit)
            {
                //画面設定
                SetComponent(form);  
                ret = View("CarStockListCriteria", list);
            }
            //検索 or Excel出力
            else
            {
                //入力値のチェック
                ValidateForSearch(form);

                if (!ModelState.IsValid)
                {
                    //画面設定
                    SetComponent(form);
                    // 検索画面の表示
                    return View("CarStockListCriteria", list);
                }

                switch (form["RequestFlag"])
                {
                    //Excel出力
                    case "1": //車両棚卸表出力
                    //Add 2019/12/28 yano #4033
                    case "2": //画面リスト出力         

                        if (form["RequestFlag"].Equals("2"))
                        {
                            ValidateForListDownload(form);

                            if (!ModelState.IsValid)
                            {
                                //画面設定
                                SetComponent(form);

                                // 検索画面の表示
                                return View("CarStockListCriteria", list);
                            }
                        }

                        ret = Download(form);

                        break;

                    default: //検索処理

                        list = SearchList(form);
                        ret = View("CarStockListCriteria", list);
                        break;
                }
            }

            return ret;
        }

        /// <summary>
        /// 車両在庫検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2020/1/20 yano #4033 【車両在庫表】全部門分の在庫表の出力機能の追加
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        private PaginatedList<GetStockCarListResult> SearchList(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);
            PaginatedList<GetStockCarListResult> list = new PaginatedList<GetStockCarListResult>();

            //全体の棚卸ステータス取得
            CodeDao dao = new CodeDao(db);

            DateTime targetDate = DateTime.Now;

            //棚卸月の取得
            if(dao.GetYear(true, form["TargetDateY"]) != null && dao.GetMonth(true, form["TargetDateM"]) != null)
            {
                targetDate = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");
            }

            //Mod 2020/1/20 yano #4033
            CarInventory condition = SetCondition(form, targetDate);

           ////車両棚卸
           // CarInventory condition = new CarInventory();

           // condition.strInventoryMonth  = string.Format("{0:yyyy/MM/dd}", targetDate);     //棚卸月
           // condition.DepartmentCode     = form["DepartmentCode"];                          //部門コード
           // condition.LocationCode       = form["LocationCode"];                            //ロケーションコード
           // condition.SalesCarNumber     = form["SalesCarNumber"];                          //管理番号
           // condition.Vin                = form["Vin"];                                     //車台番号
           // condition.NewUsedType        = form["NewUsedType"];                             //新中区分
           // condition.CarStatus          = form["CarStatus"];                               //在庫区分
           // condition.CarBrandName       = form["CarBrandName"];                            //ブランド名
           // condition.CarName            = form["CarName"];                                 //車種名
           // condition.CarGradeName       = form["CarGradeName"];                            //グレード名
           // condition.RegistrationNumber = form["RegistrationNumber"];                      //登録番号
           // condition.StockFlag          = form["StockZeroVisibility"];                     //在庫有無
            

            //ページ取得
            list = new CarStockListDao(db).GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            
            //画面設定
            SetComponent(form);

            return list;
        }
       
        #region 個別処理用Validationcheck
        /// <summary>
        /// 検索時のValidationチェック
        /// </summary>
        /// <param name="form"></param>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void ValidateForSearch(FormCollection form)
        {
            //対象年月(年=未選択、月=選択)
            if (string.IsNullOrEmpty(form["TargetDateY"]) && !string.IsNullOrEmpty(form["TargetDateM"]))
            {
                ModelState.AddModelError("TargetDateY", "対象年月(年)を選択してください");
            }
            //対象年月(年=選択、月=未選択)
            if (!string.IsNullOrEmpty(form["TargetDateY"]) && string.IsNullOrEmpty(form["TargetDateM"]))
            {
                ModelState.AddModelError("TargetDateM", "対象年月(月)を選択してください");
            }
            //対象年月指定ありかつ、対象年月(年=選択、月=未選択)
            if ((form["TargetRange"] == "1") && (string.IsNullOrEmpty(form["TargetDateY"]) && string.IsNullOrEmpty(form["TargetDateM"])))
            {
                ModelState.AddModelError("TargetDateY", "対象年月指定ありの場合は、対象年月を選択してください");
                ModelState.AddModelError("TargetDateM", "");
            }

            return;
        }

        /// <summary>
        /// 棚卸リスト出力時のValidationチェック
        /// </summary>
        /// <param name="form"></param>
        /// <history>
        /// 2020/1/20 yano #4033 【車両在庫表】全部門分の在庫表の出力機能の追加
        /// </history>
        private void ValidateForListDownload(FormCollection form)
        {
            string inventoryMonth = "";

            string inventoryStatus = "";
            
            CodeDao dao = new CodeDao(db);

            //棚卸月の設定
            if (dao.GetYear(true, form["TargetDateY"]) != null && dao.GetMonth(true, form["TargetDateM"]) != null)
            {
                inventoryMonth = dao.GetYear(true, form["TargetDateY"]).Name.PadLeft(4, '0') + dao.GetMonth(true, form["TargetDateM"]).Name.PadLeft(2, '0') + "01";
            }

            //対象の棚卸月のステータスをチェックする
            inventoryStatus = new InventoryMonthControlCarDao(db).GetByKey(inventoryMonth) != null ? new InventoryMonthControlCarDao(db).GetByKey(inventoryMonth).InventoryStatus : "";

            if (string.IsNullOrWhiteSpace(inventoryStatus) || !inventoryStatus.Equals("003"))
            {
                ModelState.AddModelError("TargetDateY", "棚卸が完了していないため、棚卸リストを出力出来ません。");
                ModelState.AddModelError("TargetDateM", "");
            }

            return;
        }

        #endregion

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void SetComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //対象年月指定
            ViewData["TargetRange"] = form["TargetRange"];
            ViewData["DefaultTargetRange"] = form["DefaultTargetRange"];

            //対象年月
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(false), form["TargetDateY"], true);
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(false), form["TargetDateM"], true);


            //棚卸月の取得
            DateTime? targetDate = null;
            
            if(dao.GetYear(true, form["TargetDateY"]) != null && dao.GetMonth(true, form["TargetDateM"]) != null)
            {
                targetDate = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");
            }

            ViewData["InventoryMonth"] = targetDate;

            //部門コード
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            //部門名
            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"], false);

                ViewData["DepartmentName"] = dep != null ? dep.DepartmentName : ""; 
            }
            //ロケーションコード
            ViewData["LocationCode"] = form["LocationCode"];
            
            //ロケーション名
            if (!string.IsNullOrWhiteSpace(form["LocationCode"]))
            {
                Location loc = new LocationDao(db).GetByKey(form["LocationCode"], false);

                ViewData["LocationName"] = loc != null ? loc.LocationName : "";
            }
            //管理番号
            ViewData["SalesCarNumber"] = form["SalesCarNumber"];
            //車台番号
            ViewData["Vin"] = form["Vin"];
            //新中区分
            ViewData["NewUsedType"] = form["NewUsedType"];                                                                                          //新中区分
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), form["NewUsedType"], true);      //新中区分リスト
            //在庫区分
            ViewData["CarStatus"] = form["CarStatus"];                                                                                              //在庫区分
            ViewData["CarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName(CATEGORY_CARSTOCK, false), form["CarStatus"], true);         //在庫区分リス
            //ブランド名
            ViewData["CarBrandName"] = form["CarBrandName"];
            //車種名
            ViewData["CarName"] = form["CarName"];
            //グレード名
            ViewData["CarGradeName"] = form["CarGradeName"];
            //車両登録番号
            ViewData["RegistrationNumber"] = form["RegistrationNumber"];
            //在庫有無
            ViewData["StockZeroVisibility"] = string.IsNullOrWhiteSpace(form["StockZeroVisibility"]) ? "0" : form["StockZeroVisibility"];

            //現在年月
            form["NowTargetDateY"] = DateTime.Today.Year.ToString().Substring(1, 3);            //部門別検索用当日年
            form["NowTargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');           //部門別検索用当日月
            ViewData["NowTargetDateY"] = form["NowTargetDateY"];
            ViewData["NowTargetDateM"] = form["NowTargetDateM"];

            return;
        }
        #endregion


        #region Excel出力処理
        /// <summary>
        /// Excelダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2020/1/20 yano #4033【車両在庫表】全部門分の在庫表の出力機能の追加
        /// 2017/11/16 arc yano  #3827 車両在庫検索　Excel出力を行うとシステムエラー
        /// </history>
        private ActionResult Download(FormCollection form)
        {
            //-------------------------------
            //初期処理
            //-------------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCELDOWNLOAD);

            CodeDao dao = new CodeDao(db);

            ModelState.Clear();

            //検索結果の設定
            PaginatedList<GetStockCarListResult> list = new PaginatedList<GetStockCarListResult>();

            //対象年月の設定
            DateTime ? inventoryMonth = null;
            
            if(dao.GetYear(true, form["TargetDateY"]) != null && dao.GetMonth(true, form["TargetDateM"]) != null)
            {
                inventoryMonth = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");
            }

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            //ファイル名(xxx(部門コード)_xxxx(部門名)_yyyyMM(対象年月)_CarInventoryList_yyyyMMddhhmiss(ダウンロード時刻))
            Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

            string filedepartmentCode = dep != null ? dep.DepartmentCode : "";        //Add 2020/1/20 yano #4033
            string filedepartmentName = dep != null ? dep.DepartmentName : "全部門";  //Add 2020/1/20 yano #4033


            //Mod 2020/1/20 yano #4033
            //-------------------------------
            //Excel出力処理
            //-------------------------------
            string excelName = "";
            string tfilePathName = "";

            string fileName = "";
            string filePathName = "";

            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            byte[] excelData = null;

            //車両棚卸リスト出力
            if (form["RequestFlag"].Equals("1"))
            {
                excelName = "CarInventoryList";

                tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForCarInventoryList"]) ? "" : ConfigurationManager.AppSettings["TemplateForCarInventoryList"];   //Add 2017/11/16 arc yano  #3827

                fileName = filedepartmentCode + "_" + filedepartmentName + "_" + string.Format("{0:yyyyMM}", inventoryMonth) + "_" + excelName + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";
                filePathName = filePath + fileName;

                //エクセルデータ作成
                excelData = MakeExcelData(form, inventoryMonth, filePathName, tfilePathName);
            }
            else //画面リスト出力
            {
                excelName = "CarInventory";
                tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForCarInventory"]) ? "" : ConfigurationManager.AppSettings["TemplateForCarInventory"];

                fileName = filedepartmentCode + "_" + filedepartmentName + "_" + string.Format("{0:yyyyMM}", inventoryMonth) + "_" + excelName + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";
                filePathName = filePath + fileName;

                //エクセルデータ作成
                excelData = MakeExcelDataCarInventory(form, inventoryMonth, filePathName, tfilePathName);
            }

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetComponent(form);
                return View("CarStockListCriteria", list);
            }

            //-----------------------
            //出力ファイルパス設定
            //-----------------------
            //string fileName = dep.DepartmentCode + "_" + dep.DepartmentName + "_" + string.Format("{0:yyyyMM}", inventoryMonth) + "_" + excelName + "_" +  string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";
            
            //ワークフォルダ取得
            //string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];
            //string filePathName = filePath + fileName;

            //エクセルデータ作成
            //byte[] excelData = MakeExcelData(form, inventoryMonth, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetComponent(form);
                return View("CarStockListCriteria", list);
            }

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);
        }


        /// エクセルデータ作成(テンプレートファイルあり)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="fileName">帳票名</param>
        /// <param name="tfileName">帳票テンプレート</param>
        /// <returns>エクセルデータ</returns>
        /// <history>
        /// 2020/01/20 yano #4033 【車両在庫表】全部門分の在庫表の出力機能の追加
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private byte[] MakeExcelData(FormCollection form, DateTime? inventoryMonth, string fileName, string tfileName)
        {

            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                  //設定値
            byte[] excelData = null;                //エクセルデータ
            bool ret = false;
            bool tFileExists = true;                //テンプレートファイルあり／なし(実際にあるかどうか)

            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルあり)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //テンプレートファイルが無かった場合
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "テンプレートファイルが見つかりませんでした。");

                //ファイル削除
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //----------------------------
            // 設定シート取得
            //----------------------------
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //設定データを取得(config)
            if (config != null)
            {
                configLine = dExport.GetConfigLine(config, 2);
            }
            else //configシートが無い場合はエラー
            {
                ModelState.AddModelError("", "テンプレートファイルのconfigシートがみつかりません");

                excelData = excelFile.GetAsByteArray();

                //ファイル削除
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //ワークシートオープン
            var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];


            ConfigLine cl = new ConfigLine();

            cl.DIndex = configLine.DIndex;
            cl.SheetName = configLine.SheetName;
            cl.Type = configLine.Type;

            //------------------------
            // ロケーションコード
            //------------------------

            //設定位置
            string pos = "Q2";

            cl.SetPos.Add(pos);

            //タイプ
            cl.Type = 0;

            List<LocationInfo> slist = new List<LocationInfo>();

            LocationInfo line = new LocationInfo();

            //ロケーション取得
            Location rec = new Location();

            rec = GetLocation(form["DepartmentCode"], form);

            line.LocationCode = rec != null ? rec.LocationCode : "";

            line.LocationName = rec != null ? rec.LocationName : "";

            slist.Add(line);

            //データ設定
            ret = dExport.SetData<LocationInfo, LocationInfo>(ref excelFile, slist, cl);

            //----------------------------
            // データ行出力
            //----------------------------
            //Mod 2020/1/20 yano #4033　検索条件設定処理を外出し
            CarInventory condition = SetCondition(form, (inventoryMonth ?? DateTime.Now));

            ////車両棚卸
            //CarInventory condition = new CarInventory();
            //condition.strInventoryMonth = string.Format("{0:yyyy/MM/dd}", inventoryMonth);     //棚卸月
            //condition.DepartmentCode = form["DepartmentCode"];                          //部門コード
            //condition.LocationCode = form["LocationCode"];                            //ロケーションコード
            //condition.SalesCarNumber = form["SalesCarNumber"];                          //管理番号
            //condition.Vin = form["Vin"];                                     //車台番号
            //condition.NewUsedType = form["NewUsedType"];                             //新中区分
            //condition.CarStatus = form["CarStatus"];                               //在庫区分
            //condition.CarBrandName = form["CarBrandName"];                            //ブランド名
            //condition.CarName = form["CarName"];                                 //車種名
            //condition.CarGradeName = form["CarGradeName"];                            //グレード名
            //condition.RegistrationNumber = form["RegistrationNumber"];                      //登録番号
            //condition.StockFlag = form["StockZeroVisibility"];                     //在庫有無
            
            List<GetStockCarListResult> list = new CarStockListDao(db).GetListByCondition(condition).ToList();

            //データ設定
            ret = dExport.SetData<GetStockCarListResult, CarInventoryForExcel>(ref excelFile, list, configLine);

            //----------------------------
            // 説明行出力
            //----------------------------

            //説明文設定
            List<DescriptionBlock> dblist = new List<DescriptionBlock>();

            DescriptionBlock block = new DescriptionBlock();

            block.Description = DESCRIPTION_EXCELDOWNLOAD;

            dblist.Add(block);

            for (int i = 0; i < configLine.SetPosRowCol.Count; i++)
            {
                Tuple<int, int> setpos = new Tuple<int, int>(configLine.SetPosRowCol[i].Item1 + list.Count, configLine.SetPosRowCol[i].Item2);

                configLine.SetPosRowCol[i] = setpos;
            }

            //データ設定
            ret = dExport.SetData<DescriptionBlock, DescriptionBlock>(ref excelFile, dblist, configLine);

            excelFile.Workbook.Worksheets["masterList"].Cells[27, 2].Formula = excelFile.Workbook.Worksheets["masterList"].Cells[27, 2].Formula;

            excelData = excelFile.GetAsByteArray();

            //ワークファイル削除
            try
            {
                excelFile.Stream.Close();
                excelFile.Dispose();
                dExport.DeleteFileStream(fileName);
            }
            catch
            {
                //
            }

            return excelData;
        }

        /// 車両棚卸リストデータ作成(テンプレートファイルあり)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="fileName">帳票名</param>
        /// <param name="tfileName">帳票テンプレート</param>
        /// <returns>エクセルデータ</returns>
        /// <history>
        /// 2020/01/20 yano #4033 【車両在庫表】全部門分の在庫表の出力機能の追加 新規追加
        /// </history>
        private byte[] MakeExcelDataCarInventory(FormCollection form, DateTime? inventoryMonth, string fileName, string tfileName)
        {
            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                  //設定値
            byte[] excelData = null;                //エクセルデータ
            bool ret = false;
            bool tFileExists = true;                //テンプレートファイルあり／なし(実際にあるかどうか)


            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルあり)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //テンプレートファイルが無かった場合
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "テンプレートファイルが見つかりませんでした。");

                //ファイル削除
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //----------------------------
            // 設定シート取得
            //----------------------------
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //設定データを取得(config)
            if (config != null)
            {
                configLine = dExport.GetConfigLine(config, 2);
            }
            else //configシートが無い場合はエラー
            {
                ModelState.AddModelError("", "テンプレートファイルのconfigシートがみつかりません");

                excelData = excelFile.GetAsByteArray();

                //ファイル削除
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }


            //ワークシートオープン
            var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];


            ConfigLine cl = new ConfigLine();

            cl.DIndex = configLine.DIndex;
            cl.SheetName = configLine.SheetName;
            cl.Type = configLine.Type;

            //----------------------------
            // データ行出力
            //----------------------------
            //検索条件の設定
            CarInventory condition = SetCondition(form, (inventoryMonth ?? DateTime.Now));

            List<CarInventory> list = new InventoryStockCarDao(db).GetListByCondition(condition).ToList();

            //データ設定
            ret = dExport.SetData<CarInventory, CarInventoryForExcelList>(ref excelFile, list, configLine);

            //----------------------------
            // 検索条件出力
            //----------------------------
            configLine.SetPos[0] = "A1";
 
            //検索条件文字列を作成
            DataTable dtCondtion = MakeConditionRow(condition);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);
            
            excelData = excelFile.GetAsByteArray();
          
            //ワークファイル削除
            try
            {
                excelFile.Stream.Close();
                excelFile.Dispose();
                dExport.DeleteFileStream(fileName);
            }
            catch
            {
                //
            }

            return excelData;
        }

        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2020/1/20 yano #4033 【車両在庫表】全部門分の在庫表の出力機能の追加
        /// </history>
        private DataTable MakeConditionRow(CarInventory condition)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();
            String conditionText = "";
            
            string departmentName = "";
            string warehouseName = "";

            //部門名取得
            Department dep = new DepartmentDao(db).GetByKey(condition.DepartmentCode, false);
            departmentName = dep != null ? dep.DepartmentName : "";


            //倉庫名取得
            Warehouse wareh = new WarehouseDao(db).GetByKey(condition.WarehouseCode);
            warehouseName = wareh != null ? wareh.WarehouseName : "";

            //---------------------
            //　列定義
            //---------------------
            //１つの列を設定  
            if (condition.InventoryMonth != null)
            {
                dt.Columns.Add("CondisionText", Type.GetType("System.String"));
            }

            //---------------
            //データ設定
            //---------------
            DataRow row = dt.NewRow();
            //部門コード
            if (!string.IsNullOrEmpty(condition.DepartmentCode))
            {
                conditionText += string.Format("部門={0}:{1}　", condition.DepartmentCode, departmentName);
            }
            //Add 2016/08/13 arc yano #3596
            //倉庫コード     
            if (!string.IsNullOrEmpty(condition.WarehouseCode))
            {
                conditionText += string.Format("倉庫={0}:{1}　", condition.WarehouseCode, warehouseName);
            }
            if (condition.InventoryMonth != null)
            {
                conditionText += string.Format("対象年月={0:yyyy/MM}　", condition.InventoryMonth);
            }
            //ロケーションコード
            if (!string.IsNullOrEmpty(condition.LocationCode))
            {
                conditionText += string.Format("ロケーションコード={0}　", condition.LocationCode);
            }
            //ロケーション名
            if (!string.IsNullOrEmpty(condition.LocationName))
            {
                conditionText += string.Format("ロケーション名={0}　", condition.LocationName);
            }
            //管理番号
            if (!string.IsNullOrEmpty(condition.SalesCarNumber))
            {
                conditionText += string.Format("管理番号={0}　", condition.SalesCarNumber);
            }
            //車台番号
            if (!string.IsNullOrEmpty(condition.Vin))
            {
                conditionText += string.Format("車台番号={0}　", condition.Vin);
            }
            //新中区分
            if (!string.IsNullOrEmpty(condition.NewUsedTypeName))
            {
                conditionText += string.Format("新中区分={0}　", condition.NewUsedTypeName);
            }
            //在庫区分
            if (!string.IsNullOrEmpty(condition.CarStatusName))
            {
                conditionText += string.Format("在庫区分={0}　", condition.CarStatusName);
            }
            //棚差有無
            if (condition.InventoryDiff.Equals(true))
            {
                conditionText += string.Format("棚差有無=棚差があるレコードのみ表示　");
            }

            //作成したテキストをカラムに設定
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);

            return dt;
        }

        /// <summary>
        /// 検索条件設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>検索条件</returns>
        /// <history>
        /// 2020/1/20 yano #4033 【車両在庫表】全部門分の在庫表の出力機能の追加 新規追加
        /// </history>
        private CarInventory SetCondition(FormCollection form, DateTime inventoryMonth)
        {

            CarInventory condition = new CarInventory();

            condition.InventoryMonth = inventoryMonth;
            condition.strInventoryMonth = string.Format("{0:yyyy/MM/dd}", inventoryMonth);     //棚卸月
            condition.DepartmentCode = form["DepartmentCode"];                          //部門コード
            condition.LocationCode = form["LocationCode"];                            //ロケーションコード
            condition.SalesCarNumber = form["SalesCarNumber"];                          //管理番号
            condition.Vin = form["Vin"];                                     //車台番号
            condition.NewUsedType = form["NewUsedType"];                             //新中区分
            condition.CarStatus = form["CarStatus"];                               //在庫区分
            condition.CarBrandName = form["CarBrandName"];                            //ブランド名
            condition.CarName = form["CarName"];                                 //車種名
            condition.CarGradeName = form["CarGradeName"];                            //グレード名
            condition.RegistrationNumber = form["RegistrationNumber"];                      //登録番号
            condition.StockFlag = form["StockZeroVisibility"];                     //在庫有無

            return condition;
        }

        #endregion


        #region その他共通処理
        /// <summary>
        /// ロケーションコード取得処理
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private Location GetLocation(string deparmtnetCode, FormCollection form)
        {
            Department dep = new DepartmentDao(db).GetByKey(deparmtnetCode);

            Location rec = new Location();

            DepartmentWarehouse dwarehouse = CommonUtils.GetWarehouseFromDepartment(db, deparmtnetCode);

            //ロケーションコードが入力されている場合
            if (!string.IsNullOrWhiteSpace(form["LocationCode"]))
            {
                rec = new LocationDao(db).GetByKey(form["LocationCode"]);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dep.CloseMonthFlag) && dep.CloseMonthFlag.Equals("2"))
                {
                    rec = new LocationDao(db).GetListByLocationType("003", dwarehouse.WarehouseCode, "").FirstOrDefault();
                }
                else
                {
                    rec = new LocationDao(db).GetListByLocationType("001", dwarehouse.WarehouseCode, "").FirstOrDefault();
                }
            }

            return rec;
        }
        #endregion

        /// <summary>
        /// 処理中かどうかを取得する。(Ajax専用）
        /// </summary>
        /// <param name="processType">処理種別</param>
        /// <returns>アイドリング状態</returns>
        /// <history>
        /// Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public ActionResult GetProcessed(string processType)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> retParts = new Dictionary<string, string>();

                retParts.Add("ProcessedTime", "アイドリング中…");

                return Json(retParts);
            }
            return new EmptyResult();
        }
    }
}
