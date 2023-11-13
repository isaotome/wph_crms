//*------------------------------------------------------------------------------------------------------------------
//* 機能：検索ダイアログ用フレーム枠
//*       showmodaldialog()で開いた子画面でsubmitを行うと、親画面と同期を取ることができなくなる。
//*       このため、submitを行う部分をフレーム化する。
//* 作成日：2014/07/14 arc yano chrome対応
//* 更新履歴：
//*
//*------------------------------------------------------------------------------------------------------------------  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Crms.Controllers
{


    //Add 2015/01/14 arc yano 他のコントローラと同じく、フィルタ属性(例外、セキュリティ、出力キャッシュ)を追加		
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class iFrameController : Controller
    {
        
        //----------------------------------
        // 定数定義
        //----------------------------------
        static class Constants
        {
            public class WindowTitle
            {
                public string strUrl;
                public string strTitle;
                public WindowTitle(string strurl, string strtitle)
                {
                    strUrl = strurl;
                    strTitle = strtitle;
                }
            }

            public static readonly WindowTitle[] windowTitle = new WindowTitle[]{
            new WindowTitle("ServiceWork", "主作業検索ダイアログ"),
            new WindowTitle("SetMenu", "セットメニュー検索"),
            new WindowTitle("ServiceMenu", "サービスメニュー検索ダイアログ"),
            new WindowTitle("PartsStock", "部品在庫検索ダイアログ"),
            new WindowTitle("Maker", "メーカー検索ダイアログ"),
            new WindowTitle("Company", "会社検索ダイアログ"),
            new WindowTitle("Loan", "ローン検索ダイアログ"),
            new WindowTitle("CarGrade", "グレード検索ダイアログ"),
            new WindowTitle("Brand", "ブランド検索ダイアログ"),
            new WindowTitle("Car", "車種検索ダイアログ"),
            new WindowTitle("CarClass", "車両クラス検索ダイアログ"),
            new WindowTitle("Department", "部門検索ダイアログ"),
            new WindowTitle("Supplier", "仕入先検索ダイアログ"),
            new WindowTitle("CarColor", "車両カラー検索ダイアログ"),
            new WindowTitle("Parts", "部品検索ダイアログ"),
            new WindowTitle("Employee", "社員検索ダイアログ"),
            new WindowTitle("SupplierPayment", "支払先検索ダイアログ"),
            new WindowTitle("SalesCar", "車両検索ダイアログ"),
            new WindowTitle("Office", "事業所検索ダイアログ"),
            new WindowTitle("Location", "ロケーション検索ダイアログ"),
            new WindowTitle("Campaign", "イベント検索ダイアログ"),
            new WindowTitle("Customer", "顧客検索ダイアログ"),
            new WindowTitle("Account", "科目検索ダイアログ"),
            new WindowTitle("CustomerClaim", "請求先検索ダイアログ"),
            new WindowTitle("PaymentKind", "支払種別検索ダイアログ"),
            new WindowTitle("Area", "エリア検索ダイアログ"),
            new WindowTitle("CarOption", "オプション検索ダイアログ"),
            new WindowTitle("CostArea", "諸費用設定エリア検索ダイアログ"),
        };


        }
        
        // GET: /iFrame/

        public ActionResult IFrame()
        {
            int i =0;
            int startPos = 0;
            int endPos = 0;
            string ctlName = null;
            string searchWord = "/";
            string searchWord2 = "&";
            string searchWord3 = "width";
            string qstring = null;

            ViewData["url"] = Request["url"];
            ViewData["width"] = Request["width"];
            ViewData["height"] = Request["height"];
            ViewData["title"] = "";

            //開始位置を検索
            startPos = ViewData["url"].ToString().IndexOf(searchWord);

            //終了位置を検索
            endPos = ViewData["url"].ToString().IndexOf(searchWord, startPos + searchWord.Length);

            if (startPos != -1 && endPos != -1) //開始位置、終了位置共に見つかった場合
            {
                ctlName = ViewData["url"].ToString().Substring(startPos + 1, endPos - 1);
            }

            if(!string.IsNullOrEmpty(ctlName))
            {
                //ウィンドウタイトル検索
                for (i = 0; i < Constants.windowTitle.Length; i++)
                {
                    if (ctlName.Equals(Constants.windowTitle[i].strUrl))
                    {
                        ViewData["title"] = Constants.windowTitle[i].strTitle;
                        break;
                    }
                }
            }
            
            //URL編集(本来のリクエストURLに編集する)
            qstring = Request.RawUrl;
            
            //開始位置を取得
            startPos = qstring.IndexOf(searchWord2);

            if (startPos != -1)
            {
                qstring = qstring.Substring(startPos + 1);
                //width,heightは削除する。
                startPos = qstring.IndexOf(searchWord3);
                if (startPos == 0)  //width,height以外にクエリ文字はない
                {
                    //何もしない
                }
                else
                {
                    qstring = qstring.Remove(startPos - 1, (qstring.Length - (startPos - 1)));
                    ViewData["url"] = ViewData["url"] + "?" + qstring;
                }
            }
            

            return View();
        }

    }
}
