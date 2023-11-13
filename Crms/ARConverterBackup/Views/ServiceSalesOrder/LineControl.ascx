<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.ServiceSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
        <table class="input-form-line">
   <%  
        //*------------------------------------------------------------------------------------------------------------------
        //* 機能：サービス伝票・作業内容明細行
        //* 作成日：???
        //* 更新履歴：
        //*   2018/02/24 arc yano #3831 在庫判断を発注に変更した場合、引当済数 > 0の場合、0にリセットする
        //*   2017/11/03 arc yano #3775 サービス伝票　引当されない 部品番号の空白、タブスペースを削除
        //*   2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 発注書出力対象のチェックボックス追加
        //*   2017/02/14 arc yano #3641 金額欄のカンマ表示対応
        //*                             ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
        //*                             ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
            //*   2016/04/14 arc yano #3480 サービス伝票　サービス伝票の請求先を主作業の内容により切り分ける 主作業⇔請求先コード絞込み
        //*   2016/03/17 arc yano #3471 サービス伝票　区分の絞込みの対応
        //*   2016/02/22 arc yano #3434 サービス伝票  消耗品(SP)の対応
        //*   2016/02/19 arc yano #3435 サービス伝票　原価０の部品の対応 在庫判断で「社達」が選択された場合、引当済数を０にする
        //*   2016/02/12 arc yano #3429 サービス伝票　判断の活性／非活性の制御の追加
        //*                       　　　　　　　　　 その明細の部品に対して発注済の場合は在庫判断の変更不可            
        //*   2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更　
        //*                        ①引当済数、発注数の追加
        //*   2014/08/04 arc yano Chrome対応
        //*                        ①コントロールのid追加漏れの対応                
        //*   2014/07/04 arc yano Chrome対応 
        //*						   ①各コントロールのid のprefixを統一する。("line[x]_xxxx"となるようにする)               
        //*						   ②明細行の調整
        //*   2014/06/06 arc yano 高速化対応 行追加・削除の処理をjavaScriptで処理する
        //*                        ①tbodyタグを追加
        //*                        ②検索ダイアログ用のボタン(SearchButtonControl)にidを追加
        //*                        ③行削除、追加等のボタンクリック時にsubmitせずに、javaScriptで処理する。
        //*                        ④サーバ側で削除を行うための隠しフィールド("CliDelFlag")を追加。
        //*   2014/05/29 arc yano vs2012対応 各コントロールにid追加
        //*------------------------------------------------------------------------------------------------------------------  
    %>
        <tbody id="salesline-tbody"><% //Add 2014/06/06 arc yano %>


    <%for(int i=0;i<Model.ServiceSalesLine.Count;i++){ %>
    <%
               
        ServiceSalesLine line = Model.ServiceSalesLine[i];
        string namePrefix = string.Format("line[{0}].",i);
        string idPrefix = string.Format("line[{0}]_", i); //Add 2014/05/29 arc yano
    
        //Add 2014/06/10 arc yano 明細行各行の表示をline.delflgで切り替える。
        string dispTr = "table-row";
        if((line.CliDelFlag != null) && (line.CliDelFlag == "1")){    //削除対象行の場合は表示しない。
            dispTr = "none";
        }
        string dispWork = "none";
        string dispService = "none";
        string dispParts = "none";
        string dispSetMenu = "none";
        string dispWorkType = "none";
        //string lineContentWidth = "190px";
        string lineContentWidth = "185px";
        string dispPartsStock = "none";
        string lineContentColor = "#000000";
        switch (CommonUtils.DefaultString(line.ServiceType)) {
            case "001":
                dispWork = "inline";
                //dispWorkType = "inline";      //Mod 2016/03/17 arc yano #3471
                break;
            case "002":
                dispService = "inline";
                dispWorkType = "inline";
                lineContentColor = "#0000FF";
                break;
            case "003":
                dispParts = "inline";
                dispWorkType = "inline";
                dispPartsStock = "inline";
                lineContentColor = "#FF00FF";
                break;
            case "004":
                lineContentWidth = "315px"; //Mod 2015/10/28 arc yano #3289 323px→315px
                lineContentColor = "#006600";
                break;
            case "005":
                dispSetMenu = "inlCustomerClaimCodeine";
                break;
        }
    %>
        <tr style="display:<%= dispTr %>">  <% //Edit 2014/06/10 arc yano 高速化対応 %>
            <td nowrap style="width:100px"><a name="<%=i %>" /><%=Html.Hidden(namePrefix + "LineNumber", i + 1, new { id = idPrefix + "LineNumber" })%>
                <%=Html.Hidden(namePrefix + "ParentCustomerClaimCode", line.ParentCustomerClaimCode, new { id = idPrefix + "ParentCustomerClaimCode" })%>
                <%=Html.Hidden(namePrefix + "TaxAmount", line.TaxAmount, new { id = idPrefix + "TaxAmount", @class = "money"})%><%//2017/04/23 arc yano #3755 %>
                <%=Html.Hidden(namePrefix + "AmountWithoutTax", (line.Amount ?? 0m) - (line.TaxAmount ?? 0m), new { id = idPrefix + "AmountWithoutTax", @class = "money"})%><%//2017/04/23 arc yano #3755 %>
                <%=Html.Hidden(namePrefix + "TechnicalFeeAmountWithoutTax", (line.TechnicalFeeAmount ?? 0m) - (line.TaxAmount ?? 0m), new { id = idPrefix + "TechnicalFeeAmountWithoutTax", @class = "money" }) %><%//2017/04/23 arc yano #3755 %>
                <%=Html.Hidden(namePrefix + "Classification1", line.Classification1, new { id = idPrefix + "Classification1" }) %>
                <%if(Model.LineEnabled){ %>
                <img id="<%=idPrefix %>DelLine" alt="行削除" src="/Content/Images/delete.png" style="cursor:pointer" onclick="$('#EditType').val('delete');$('#EditLine').val('<%=i%>');document.getElementById('lineScroll').value = document.getElementById('line').scrollTop;editList(this);" /><% //Edit 2014/06/06 arc yano %>
                <img id="<%=idPrefix %>InsLine" alt="行挿入" src="/Content/Images/insert.png" style="cursor:pointer" onclick="$('#EditType').val('insert');$('#EditLine').val('<%=i%>');document.getElementById('lineScroll').value = document.getElementById('line').scrollTop;editList(this);" /><% //Edit 2014/06/06 arc yano %>
                <img id="<%=idPrefix %>CopyLine" alt="行コピー" src="/Content/Images/copy.png" style="cursor:pointer" onclick="$('#EditType').val('copy');$('#EditLine').val('<%=i%>');document.getElementById('lineScroll').value = document.getElementById('line').scrollTop;editList(this);" /><% //Edit 2014/06/06 arc yano %>
                <img id="<%=idPrefix %>UpLine" alt="上に移動" src="/Content/Images/up.png" style="cursor:pointer" onclick="$('#EditType').val('up');$('#EditLine').val('<%=i%>');document.getElementById('lineScroll').value = document.getElementById('line').scrollTop;editList(this);" /><% //Edit 2014/06/06 arc yano %>
                <img id="<%=idPrefix %>DownlLine" alt="下に移動" src="/Content/Images/down.png" style="cursor:pointer" onclick="$('#EditType').val('down');$('#EditLine').val('<%=i%>');document.getElementById('lineScroll').value = document.getElementById('line').scrollTop;editList(this);" /><% //Edit 2014/06/06 arc yano %>
                <%} %>
            </td>
            <%if(Model.LineEnabled){ %>
            <!-- 種別 -->
            <td nowrap style="width:100px"><%//Mod 2015/10/28 arc yano #3289 93px→100px%>
                <%=Html.DropDownList(namePrefix + "ServiceType", ((List<IEnumerable<SelectListItem>>)ViewData["ServiceTypeLineList"])[i], new { id = idPrefix + "ServiceType", onchange = "ChangeServiceType("+i+")" })%>
                <%=Html.Hidden(namePrefix + "ServiceTypeName", line.ServiceTypeName, new { id = idPrefix + "ServiceTypeName"})%>
            </td> 
            <!-- コード -->
            <td nowrap style="width:330px">
                <span id="<%=idPrefix %>InputServiceWorkCode" style="display:<%=dispWork%>">
                <%//Mod 2017/05/02 arc nakayama #3760_サービス伝票　請求先を選択した状態で主作業のルックアップを開くとシステムエラー%>
				<%//Mod 2016/04/14 arc yano #3480 請求先が入力されている場合は、主作業検索ダイアログの一覧を絞り込む %>
                    <%=Html.TextBox(namePrefix + "ServiceWorkCode", line.ServiceWorkCode, new { id = idPrefix + "ServiceWorkCode", @class = "alphanumeric" , style = "width:100px", maxlength = "5", onblur = "GetServiceWork(" + i + ")" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "ServiceWorkCode", idPrefix + "LineContents", "'/ServiceWork/CriteriaDialog?CCCustomerClaimClass=' + document.getElementById('" + idPrefix + "CCCustomerClaimClass').value", "0", "GetServiceWork(" + i + ")", idPrefix + "SerchServiceWork" }); %><%=Html.Hidden(namePrefix + "SWCustomerClaimClass", line.SWCustomerClaimClass, new { id = idPrefix + "SWCustomerClaimClass" }) %><%//2016/04/14 arc yano #3480 %>
                </span>
                <span id="<%=idPrefix %>InputServiceMenuCode" style="display:<%=dispService%>">
                    <%=Html.TextBox(namePrefix + "ServiceMenuCode", line.ServiceMenuCode, new { id = idPrefix + "ServiceMenuCode", style="color:#0000ff;width:100px", maxlength="8" , onchange = "GetCostFromServiceMenu('" + i + "')" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "ServiceMenuCode", idPrefix + "LineContents", "'/ServiceMenu/CriteriaDialog'", "0", "setTimeout('GetCostFromServiceMenu("+i+")',500);" , idPrefix + "SerchCost"}); %>
                </span>    
                <span id="<%=idPrefix %>InputPartsNumber" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "PartsNumber", line.PartsNumber, new { id = idPrefix + "PartsNumber", style = "color:#ff00ff;width:100px", maxlength = "25", onchange = "removalSpace(this); GetPartsStockMasterFromCode(" + i + ")" })%><%//Mod 2017/11/03 arc yano #3775 %>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "PartsNumber", idPrefix + "LineContents", "'/PartsStock/CriteriaDialog?CarBrandName='+encodeURI(document.getElementById('CarBrandName').value)", "0", "setTimeout('GetPartsStockMasterFromCode(" + i + ")',500)", idPrefix + "SerchPartsStock" });%>
                </span>
                <span id="<%=idPrefix %>InputSetMenuCode" style="display:<%=dispSetMenu%>">
                    <%=Html.TextBox(namePrefix + "SetMenuCode", line.SetMenuCode, new { id = idPrefix + "SetMenuCode", @class = "alphanumeric", style = "width:100px", maxlength = "11" })%>
                    <button id="<%=idPrefix %>SetMenu" type="button" style="width:21px;height:21px;" onclick="openSetMenu(<%=i %>)"><img alt="検索" style="width:12px;height:12px" src="/Content/Images/Search.jpg" /></button>
               </span>
               <%// Mod 2017/02/08 arc yano #3645 部品マスタの名称のサイズと明細の名称のサイズの不一致によるエラー対応 ＤＢの明細の名称のサイズをnvarchar(50)に変更したので、lengthを25→50に変更 %>
               <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
               <%if (CommonUtils.DefaultString(line.ServiceType).Equals("001"))
                 { %>
                    <%=Html.TextBox(namePrefix + "LineContents", line.LineContents, new { id = idPrefix + "LineContents", @class="readonly", @readonly="readonly", style = "color:"+lineContentColor + ";width:"+lineContentWidth, maxlength = "50" })%>
               <%}else{ %>
                    <%=Html.TextBox(namePrefix + "LineContents", line.LineContents, new { id = idPrefix + "LineContents", style = "color:" + lineContentColor + ";width:" + lineContentWidth, maxlength = "50" }) %>
               <%} %>
            </td>
            <!-- 区分 -->                    
            <td nowrap style="width:60px">
                <span id="<%=idPrefix %>InputWorkType" style="display:<%=dispWorkType%>">
                    <%=Html.DropDownList(namePrefix + "WorkType", ((List<IEnumerable<SelectListItem>>)ViewData["WorkTypeList"])[i], new { id = idPrefix + "WorkType", style = "width:55px;color:"+lineContentColor, onchange = "changeStockStatus(this, '" + idPrefix + "', 'StockStatus', 'hdStockStatus', 'OrderQuantity')" })%><%//Mod 2016/02/22 arc yano #3434 %>
                </span>
            </td>
            <!-- 工数(h) -->
            <td nowrap style="width:70px">
                <span id="<%=idPrefix %>InputManPower" style="display:<%=dispService%>">
                    <%=Html.TextBox(namePrefix + "ManPower", line.ManPower, new { id = idPrefix + "ManPower", style = "color:#0000ff;width:63px", maxlength = "10", @class = "numeric", onchange = "calcTotalServiceAmount()" })%>
                </span>
            </td>
            <!-- レバレート -->
            <td nowrap style="width:50px">
                <span id="<%=idPrefix %>InputLaborRate" style="display:<%=dispService%>">
                    <%=Html.TextBox(namePrefix + "LaborRate", string.Format("{0:N0}", line.LaborRate), new { id = idPrefix + "LaborRate", style = "color:#0000ff;width:43px", maxlength = "10", @class = "money", onchange = "calcTotalServiceAmount()" })%>
                </span>
            </td>

            <!-- 技術料 -->
            <td nowrap style="width:70px">
                <span id="<%=idPrefix %>InputTechnicalFeeAmount" style="display:<%=dispService%>">
                    <%=Html.TextBox(namePrefix + "TechnicalFeeAmount", string.Format("{0:N0}", line.TechnicalFeeAmount), new { id = idPrefix + "TechnicalFeeAmount", style = !string.IsNullOrEmpty(line.ServiceMenuCode) && line.ServiceMenuCode.Length >= 6 && line.ServiceMenuCode.Substring(0, 6).Equals("DISCNT") ? "color:#ff0000;width:63px" : "color:#0000ff;width:63px", @class = "money", maxlength = "10" })%>
                </span>
            </td>
            <!-- 在庫 -->
            <td nowrap style="width:60px;text-align:right"><%//Mod 2015/10/28 arc yano #3289 40px→60px%>
                    <%=Html.TextBox(namePrefix + "PartsStock", line.PartsStock, new { id = idPrefix + "PartsStock", style = "color:#ff00ff;width:50px;display:"+dispPartsStock, @class = "numeric readonly", @readonly = "readonly" })%><%//Mod 2015/10/28 arc yano #3289 33px→50px%>
            </td>
            <!-- 在庫判断 --><%// 2016/02/22 arc yano #3434 %><%// 2016/02/19 arc yano #3435 width 60px → 90px%><% //2016/02/12 arc yano #3429 %>
            <td nowrap style="width:90px">
                <span id="<%=idPrefix %>InputStockStatus" style="display:<%=dispParts%>"> 
                    <% if(line.OrderQuantity == null || line.OrderQuantity <= 0){ //発注数が0より多い場合は非活性%> 
                    <%  if(string.IsNullOrWhiteSpace(line.WorkType) || !line.WorkType.Equals("015")) { //区分≠SPの場合%>
                            <%=Html.DropDownList(namePrefix + "StockStatus", ((List<IEnumerable<SelectListItem>>)ViewData["StockStatus"])[i], new { id = idPrefix + "StockStatus", style = "color:#ff00ff;width:83px", onchange = "resetProvisionQuantity(this, '" + idPrefix + "ProvisionQuantity'); IsOrdered('" + idPrefix + "', 'SlipNumber', 'LineNumber', 'StockStatus', 'StockStatusNote', 'ProvisionQuantity', 'ServiceSalesOrder')" })%>
                            <%=Html.Hidden(namePrefix + "StockStatus", line.StockStatus, new { id = idPrefix + "hdStockStatus", @disabled = "disabled" })%>
                    <%  }else{ %>
                            <%=Html.DropDownList(namePrefix + "StockStatus", ((List<IEnumerable<SelectListItem>>)ViewData["StockStatus"])[i], new { id = idPrefix + "StockStatus", style = "color:#ff00ff;width:83px", @disabled = "disabled" , onchange = "resetProvisionQuantity(this, '" + idPrefix + "ProvisionQuantity'); IsOrdered('" + idPrefix + "', 'SlipNumber', 'LineNumber', 'StockStatus', 'StockStatusNote', 'ProvisionQuantity', 'ServiceSalesOrder')" })%>
                            <%=Html.Hidden(namePrefix + "StockStatus", line.StockStatus, new { id = idPrefix + "hdStockStatus" })%>
                    <%  } %>
                    <%}else{ %>
                        <%=Html.DropDownList(namePrefix + "StockStatus", ((List<IEnumerable<SelectListItem>>)ViewData["StockStatus"])[i], new { id = idPrefix + "StockStatus", style = "color:#ff00ff;width:83px", @disabled = "disabled" , onchange = "resetProvisionQuantity(this, '" + idPrefix + "ProvisionQuantity'); IsOrdered('" + idPrefix + "', 'SlipNumber', 'LineNumber', 'StockStatus', 'StockStatusNote', 'ProvisionQuantity', 'ServiceSalesOrder')" }) %>
                        <%=Html.Hidden(namePrefix + "StockStatus", line.StockStatus, new { id = idPrefix + "hdStockStatus" })%>
                    <%} %>
                    <%=Html.Hidden(namePrefix + "StockStatusNote", line.StockStatus, new { id = idPrefix + "StockStatusNote" })%>
               </span>
            </td>
             <!-- 発注書出力 --><%//Add 2017/10/19 arc yano #3803 %>
            <td nowrap style="width:100px;">
                <span id="<%=idPrefix %>InputOutputTargetFlag" style="display:<%=dispParts%>">
                    <%=Html.CheckBox(namePrefix + "OutputTarget", (string.IsNullOrWhiteSpace(line.OutputTargetFlag) && line.Parts != null && line.Parts.GenuineType.Equals("001")) || (!string.IsNullOrWhiteSpace(line.OutputTargetFlag) && line.OutputTargetFlag.Equals("1")) , new { id = idPrefix + "OutputTarget"})%>
                    <%=Html.Hidden(namePrefix + "OutputTargetFlag", line.OutputTargetFlag, new { id = idPrefix + "OutputTargetFlag"})%>
                </span>
            </td>
            <!-- 数量 -->
            <td nowrap style="width:60px">
                <span id="<%=idPrefix %>InputQuantity" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "Quantity", line.Quantity, new { id = idPrefix + "Quantity", style = "color:#ff00ff;width:53px", @class = "numeric", maxlength = "11", onchange = "calcLineCost(" + i + ")" })%>
                </span>
            </td>
            <%//Add 2015/10/28 arc yano #3289 %>
            <!-- 引当済数 -->
            <td nowrap style="width:60px">
                <span id="<%=idPrefix %>InputProvisionQuantity" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "ProvisionQuantity", line.ProvisionQuantity, new { id = idPrefix + "ProvisionQuantity", style = "color:#ff00ff;width:53px", @class = "numeric readonly", @readonly="readonly" , maxlength = "11"})%>
                </span>
            </td>
             <!-- 発注数 -->
            <td nowrap style="width:60px">
                <span id="<%=idPrefix %>InputOrderQuantity" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "OrderQuantity", line.OrderQuantity, new { id = idPrefix + "OrderQuantity", style = "color:#ff00ff;width:53px", @class = "numeric readonly", @readonly="readonly" , maxlength = "11" })%>
                </span>
            </td>
            <!-- 単価 -->
            <td nowrap style="width:70px">
                <span id="<%=idPrefix %>InputPrice" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "Price", string.Format("{0:N0}", line.Price), new { id = idPrefix + "Price", style = "color:#ff00ff;width:63px", @class = "money", maxlength = "10", onchange = "calcTotalServiceAmount()" })%>
                </span>
            </td>
            <!-- 金額 -->
            <td nowrap style="width:70px">
                <span id="<%=idPrefix %>InputAmount" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "Amount", string.Format("{0:N0}", line.Amount), new { id = idPrefix + "Amount", style = !string.IsNullOrEmpty(line.PartsNumber) && line.PartsNumber.Length >= 6 && line.PartsNumber.Substring(0, 6).Equals("DISCNT") ? "color:#ff0000;width:63px" : "color:#ff00ff;width:63px", @class = "money readonly", size = "4", maxlength = "10", @readonly = "readonly" })%>
                </span>
            </td>
            <!-- 原価（単価）-->
            <td nowrap style="width:70px">
                <span id="<%=idPrefix %>InputUnitCost" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "UnitCost", string.Format("{0:N0}", line.UnitCost), new { id = idPrefix + "UnitCost", style = "color:#ff00ff;width:63px", @class = "money", maxlength = "10", onchange = "calcLineCost(" + i + ")" })%>
                </span>
            </td>
            <!-- 原価（合計）-->
            <td nowrap style="width:70px">
                <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
                <% //Mod 2015/05/21 arc nakayama #3206_サービス伝票の原価(合計)欄入力制御　原価（合計）を部品の時だけリードオンリーにする %>
                <span id="<%=idPrefix%>InputCost" style="display:<%=CommonUtils.DefaultString(line.ServiceType).Equals("002") || CommonUtils.DefaultString(line.ServiceType).Equals("003") ? "inline" : "none"%>">
                    <%if (CommonUtils.DefaultString(line.ServiceType).Equals("003")){ %>
                        <%=Html.TextBox(namePrefix + "Cost", string.Format("{0:N0}", line.Cost), new { id = idPrefix + "Cost", style = "color:#0000ff;width:63px", @class = "money readonly", maxlength = "10", onchange = "calcTotalServiceAmount()" , @readonly = "readonly"})%>
                    <%}else{ %>
                        <%=Html.TextBox(namePrefix + "Cost", string.Format("{0:N0}", line.Cost), new { id = idPrefix + "Cost", style = "color:#0000ff;width:63px", @class = "money", maxlength = "10", onchange = "calcTotalServiceAmount()" })%>
                    <%} %>              
                </span>
            </td>
            <!-- メカニック/外注先/請求先 -->
            <td nowrap style="width: 340px">
                <span id="<%=idPrefix %>InputCustomerClaim" style="display: <%=dispWork%>">
               <%// Mod 2016/07/11 arc yano #3617 サービス伝票入力　明細行・請求先マスタチェック漏れ 引数の誤りの修正(false → 'false') %>
               <%// Mod 2016/04/13 arc yano #3480 %>
                請求先<%=Html.TextBox(namePrefix + "CustomerClaimCode", line.CustomerClaimCode, new { id = idPrefix + "CustomerClaimCode", @class = "alphanumeric", style="width:80px", maxlength = "10", onblur = "GetNameFromCode('" + idPrefix + "CustomerClaimCode','" + idPrefix + "CustomerClaimName','CustomerClaim', 'false', CheckCustomerClaimType, new Array('" + idPrefix + "SWCustomerClaimClass', '" + idPrefix + "CCCustomerClaimClass', '" + idPrefix + "CustomerClaimCode', '"  + idPrefix + "CustomerClaimName', '" + idPrefix + "ServiceWorkCode'), '"  + idPrefix + "CCCustomerClaimClass');calcTotalServiceAmount()" })%><%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "CustomerClaimCode", idPrefix + "CustomerClaimName", "'/CustomerClaim/CriteriaDialog?SWCustomerClaimClass=' + document.getElementById('" + idPrefix + "SWCustomerClaimClass').value", "0", "calcTotalServiceAmount()", idPrefix + "SerchCustomer" });%><%//Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "CustomerClaimCode", idPrefix + "CustomerClaimName", "'/CustomerClaim/CriteriaDialog'", "0", "calcTotalServiceAmount()", idPrefix + "SerchCustomer" });%><%=Html.TextBox(namePrefix + "CustomerClaimName", line.CustomerClaim != null ? line.CustomerClaim.CustomerClaimName : "", new { id = idPrefix + "CustomerClaimName" , @class="readonly", @readonly="readonly", style = "width:180px" })%>
                    <%=Html.Hidden(namePrefix + "CCCustomerClaimClass", line.CCCustomerClaimClass, new { id = idPrefix + "CCCustomerClaimClass"})%>
                </span>
                <span id="<%=idPrefix %>InputSupplier" style="display:<%=dispService%>">
                    <%=Html.DropDownList(namePrefix + "LineType", ((List<IEnumerable<SelectListItem>>)ViewData["LineTypeList"])[i], new { id = idPrefix + "LineType", style = "width:50px", onchange = "changeOutsourceEmployee("+i+")" }) %>
                    <span id="<%=idPrefix %>Employee" style="display:<%=line.LineType==null || line.LineType.Equals("001") ? "inline" : "none"%>">
                        <%=Html.TextBox(namePrefix + "EmployeeNumber", line.Employee != null ? line.Employee.EmployeeNumber : "", new { id = idPrefix + "EmployeeNumber", @class = "alphanumeric", style = "color:#0000ff;width:35px", maxlength = "20", onblur = "GetEngineerFromCode("+i+",'EmployeeNumber')" })%><%=Html.TextBox(namePrefix + "EmployeeCode", line.EmployeeCode, new { id = idPrefix + "EmployeeCode", style = "color:#0000ff;width:50px", @class = "alphanumeric", maxlength = "50", onblur = "GetEngineerFromCode("+i+",'EmployeeCode')" })%><%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "EmployeeCode", idPrefix + "EmployeeName", "'/Employee/CriteriaDialog'", "0", "", idPrefix + "SerchEmployee" });%><%=Html.TextBox(namePrefix + "EmployeeName", line.Employee != null ? line.Employee.EmployeeName : "", new { id = idPrefix + "EmployeeName", @class="readonly", @readonly="readonly", style = "color:#0000ff;width:150px" })%>
                    </span>
                    <span id="<%=idPrefix %>Supplier" style="display:<%=line.LineType!=null && line.LineType.Equals("002") ? "inline" : "none"%>">
                        <%=Html.TextBox(namePrefix + "SupplierCode", line.SupplierCode, new { id = idPrefix + "SupplierCode", style = "color:#0000ff;width:91px", @class = "alphanumeric", maxlength = "10", onchange = "GetNameFromCode('" + idPrefix + "SupplierCode','" + idPrefix + "SupplierName','Supplier')" })%><%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "SupplierCode", idPrefix + "SupplierName", "'/Supplier/CriteriaDialog/?OutsourceFlag=1'", "0", "", idPrefix + "SerchSupplier"  });%><%=Html.TextBox(namePrefix + "SupplierName", line.Supplier != null ? line.Supplier.SupplierName : "", new { id = idPrefix + "SupplierName", @class="readonly", @readonly="readonly", style = "color:#0000ff;width:150px" })%>
                    </span>
                </span>
            </td>
            <!-- 連絡事項 -->
            <td nowrap style="width:100px">
                <% // 2015.10.30 readonlyのクラスを使用しない %>
                <%=Html.TextBox(namePrefix + "RequestComment", line.RequestComment, new { id = idPrefix + "RequestComment", @class = "color:" + lineContentColor, style = "width:93px" }) %>
            </td>            
            <%}else{ %>


            <% // 赤伝を押したとき %>
            <!-- 種別 -->
            <td nowrap style="width:100px"><%//Mod 2015/10/28 arc yano #3289 93px→100px%>
                <%=Html.DropDownList(namePrefix + "ServiceType", ((List<IEnumerable<SelectListItem>>)ViewData["ServiceTypeLineList"])[i], new { id = idPrefix + "ServiceType", onchange = "ChangeServiceType(" + i + ")", @disabled = "disabled" }) %>
                <%=Html.Hidden(namePrefix + "ServiceType", line.ServiceType, new { id = idPrefix + "ServiceType" })%>
                <%=Html.Hidden(namePrefix + "ServiceTypeName", line.ServiceTypeName, new { id = idPrefix + "ServiceTypeName"})%>
            </td>
            <!-- コード -->        
            <td nowrap style="width:330px">
                <span id="Span1" style="display:<%=dispWork%>">
					<%//Mod 2016/04/14 arc yano #3480 請求先が入力されている場合は、主作業検索ダイアログの一覧を絞り込む %>
                    <%=Html.TextBox(namePrefix + "ServiceWorkCode", line.ServiceWorkCode, new { id = idPrefix + "ServiceWorkCode", @class = "alphanumeric readonly" , @readonly="readonly" , style = "width:100px", maxlength = "5" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "ServiceWorkCode", idPrefix + "LineContents", "'/ServiceWork/CriteriaDialog?CCCustomerClaimClass=' + document.getElementById('" + idPrefix + "CCCustomerClaimClass').value", "1" }); %><%=Html.Hidden(namePrefix + "SWCustomerClaimClass", line.SWCustomerClaimClass, new { id = idPrefix + "SWCustomerClaimClass" }) %><%//2016/04/14 arc yano #3480 %>
                </span>
                <span id="Span2" style="display:<%=dispService%>">
                    <%=Html.TextBox(namePrefix + "ServiceMenuCode", line.ServiceMenuCode, new { id = idPrefix + "ServiceMenuCode", style="color:#0000ff;width:100px", maxlength="8" , @readonly="readonly" , @class="readonly" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "ServiceMenuCode", idPrefix + "LineContents", "'/ServiceMenu/CriteriaDialog'", "1" });%>
                </span>    
                <span id="Span3" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "PartsNumber", line.PartsNumber, new { id = idPrefix + "PartsNumber", style = "color:#ff00ff;width:100px", maxlength = "25", @class = "readonly", @readonly = "readonly" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "PartsNumber", idPrefix + "LineContents", "'/PartsStock/CriteriaDialog?CarBrandName='+encodeURI(document.getElementById('CarBrandName').value)", "1" });%>
                </span>
                <span id="Span4" style="display:<%=dispSetMenu%>">
                    <%=Html.TextBox(namePrefix + "SetMenuCode", line.SetMenuCode, new { id = idPrefix + "SetMenuCode", @class = "alphanumeric readonly", style = "width:100px", maxlength = "11" })%>
                    <button id="Button2" type="button" style="width:21px;height:21px;" disabled="disabled"><img alt="検索" style="width:12px;height:12px" src="/Content/Images/Search.jpg" /></button>
                </span>

                <%// Mod 2017/02/08 arc yano #3645 部品マスタの名称のサイズと明細の名称のサイズの不一致によるエラー対応 ＤＢの明細の名称のサイズをnvarchar(50)に変更したので、lengthを25→50に変更 %>
                <% // Mod 2014/07/17 arc amii chrome対応 赤伝(mode=1)で表示した時、レイアウトが崩れるのを修正 %>
                <%=Html.TextBox(namePrefix + "LineContents", line.LineContents, new { id = idPrefix + "LineContents", @class="readonly", @readonly="readonly", style = "color:"+lineContentColor + ";width:"+lineContentWidth, maxlength = "50" })%>
            </td>  

            <!-- 区分 -->                    
            <td nowrap style="width:60px">
                <span id="Span20" style="display:<%=dispWorkType%>"><%//Mod 2016/03/17 arc ynao #3471 %>
                    <%=Html.DropDownList(namePrefix + "WorkType", ((List<IEnumerable<SelectListItem>>)ViewData["WorkTypeList"])[i], new { id = idPrefix + "WorkType", style = "width:55px;color:"+lineContentColor, @disabled = "disabled" , onchange = "changeStockStatus(this, '" + idPrefix + "', 'StockStatus', 'hdStockStatus', 'OrderQuantity')" }) %><%//Mod 2016/02/22 arc yano #3434 %>
                    <%=Html.Hidden(namePrefix + "WorkType", line.WorkType, new { id = idPrefix + "WorkType"})%>
                </span>

            </td>
            <!-- 工数(h) -->
            <td nowrap style="width:70px">
                <span id="Span5" style="display:<%=dispService%>">
                    <%=Html.TextBox(namePrefix + "ManPower", line.ManPower, new { id = idPrefix + "ManPower", style = "color:#0000ff;width:63px", maxlength = "10", @class = "numeric readonly", @readonly="readonly" })%>
                </span>
            </td>
            <!-- レバレート -->
            <td nowrap style="width:50px">
                <span id="Span6" style="display:<%=dispService%>">
                    <%=Html.TextBox(namePrefix + "LaborRate", string.Format("{0:N0}", line.LaborRate), new { id = idPrefix + "LaborRate", style = "color:#0000ff;width:43px", maxlength = "10", @class = "money readonly", @readonly="readonly" })%>
                </span>
            </td>
            <!-- 技術料 -->
            <td nowrap style="width:70px">
                <span id="Span7" style="display:<%=dispService%>">
                    <%=Html.TextBox(namePrefix + "TechnicalFeeAmount", string.Format("{0:N0}", line.TechnicalFeeAmount), new { id = idPrefix + "TechnicalFeeAmount", style = !string.IsNullOrEmpty(line.ServiceMenuCode) && line.ServiceMenuCode.Length >= 6 && line.ServiceMenuCode.Substring(0, 6).Equals("DISCNT") ? "color:#ff0000;width:63px" : "color:#0000ff;width:63px", @class = "money readonly",@readonly="readonly", maxlength = "10" })%>
                </span>
            </td>
            <!-- 在庫 -->
            <td nowrap style="width:60px;text-align:right"><%//Mod 2015/10/28 arc yano #3289 40px→60px%>
                <span id="Span8" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "PartsStock", line.PartsStock, new { id = idPrefix + "PartsStock", style = "color:#ff00ff;width:50px", @class = "numeric readonly", @readonly = "readonly" })%><%//Mod 2015/10/28 arc yano #3289 33px→50px%>
                </span>
            </td>
            <!-- 在庫判断 -->
            <td nowrap style="width:90px"><%// 2016/02/19 arc yano #3435 width 60px → 100px%>
                <span id="Span9" style="display:<%=dispParts%>">
                    <%=Html.DropDownList(namePrefix + "StockStatus", ((List<IEnumerable<SelectListItem>>)ViewData["StockStatus"])[i], new { id = idPrefix + "StockStatus", style = "color:#ff00ff;width:83px", @disabled = "disabled", onchange = "resetProvisionQuantity(this, '" + idPrefix + "ProvisionQuantity'); IsOrdered('" + idPrefix + "', 'SlipNumber', 'LineNumber', 'StockStatus', 'StockStatusNote', 'ProvisionQuantity', 'ServiceSalesOrder')" })%><%//Mod 2016/02/12 arc yano #3429%>
                    <%=Html.Hidden(namePrefix + "StockStatus", line.StockStatus, new { id = idPrefix + "hdStockStatus" })%>
                    <%=Html.Hidden(namePrefix + "StockStatusNote", line.StockStatus, new { id = idPrefix + "StockStatusNote" })%>
               </span>
            </td>
              <!-- 発注書出力 --><%//Add 2017/10/19 arc yano #3803 %>
            <td nowrap style="width:100px;"><%//Mod 2017/12/11 arc yano 60px->100px %>
                <span id="Span21" style="display:<%=dispParts%>">
                    <%=Html.CheckBox(namePrefix + "OutputTarget", (!string.IsNullOrWhiteSpace(line.OutputTargetFlag)  &&  line.OutputTargetFlag.Equals("1")) , new { id = idPrefix + "OutputTarget", @disabled = "disabled"})%>
                    <%=Html.Hidden(namePrefix + "OutputTargetFlag", line.OutputTargetFlag, new { id = idPrefix + "OutputTargetFlag"})%>
                </span>
            </td>
            <!-- 数量 -->
            <td nowrap style="width:60px">
                <span id="Span10" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "Quantity", line.Quantity, new { id = idPrefix + "Quantity", style = "color:#ff00ff;width:53px", @class = "numeric readonly", maxlength = "11", @readonly = "readonly" })%>
                </span>
            </td>
             <%//Add 2015/10/28 arc yano #3289 %>
            <!-- 引当済数 -->
            <td nowrap style="width:60px">
                <span id="Span18" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "ProvisionQuantity", line.ProvisionQuantity, new { id = idPrefix + "ProvisionQuantity", style = "color:#ff00ff;width:53px", @class = "numeric readonly", @readonly="readonly" , maxlength = "11"})%>
                </span>
            </td>
             <!-- 発注数 -->
            <td nowrap style="width:60px">
                <span id="Span19" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "OrderQuantity", line.OrderQuantity, new { id = idPrefix + "OrderQuantity", style = "color:#ff00ff;width:53px", @class = "numeric readonly", @readonly="readonly" , maxlength = "11" })%>
                </span>
            </td>
            <!-- 単価 -->
            <td nowrap style="width:60px">
                <span id="Span11" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "Price", string.Format("{0:N0}", line.Price), new { id = idPrefix + "Price", style = "color:#ff00ff;width:63px", @class = "money readonly", @readonly="readonly",  maxlength = "10" })%>
                </span>
            </td>
            <!-- 金額 -->
            <td nowrap style="width:70px">
                <span id="Span12" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "Amount", string.Format("{0:N0}", line.Amount), new { id = idPrefix + "Amount", style = !string.IsNullOrEmpty(line.PartsNumber) && line.PartsNumber.Length >= 6 && line.PartsNumber.Substring(0, 6).Equals("DISCNT") ? "color:#ff0000;width:63px" : "color:#ff00ff;width:63px", @class = "money readonly", size = "4", maxlength = "10", @readonly = "readonly" })%>
                </span>
            </td>
            <!-- 原価（単価）-->
            <td nowrap style="width:70px">
                <span id="Span13" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "UnitCost", string.Format("{0:N0}", line.UnitCost), new { id = idPrefix + "UnitCost", style = "color:#ff00ff;width:63px", @class = "money readonly", maxlength = "10", @readonly = "readonly" })%>
                </span>
            </td>
            <!-- 原価（合計）-->
            <td nowrap style="width:70px">
                <span id="Span14" style="display:<%=dispService%>">
                    <%=Html.TextBox(namePrefix + "Cost", string.Format("{0:N0}", line.Cost), new { id = idPrefix + "Cost", style = "color:#0000ff;width:63px", @class = "money readonly",@readonly="readonly", maxlength = "10" })%>
                </span>
                <span id="Span15" style="display:<%=dispParts%>">
                    <%=Html.TextBox(namePrefix + "Cost", string.Format("{0:N0}", line.Cost), new { id = idPrefix + "Cost", style = "color:#ff00ff;width:63px", @class = "money readonly",  maxlength = "10", @readonly="readonly" })%>
                </span>
            </td>
            <!-- メカニック/外注先/請求先 -->
            <td nowrap style="width: 340px">
                <span id="Span16" style="display: <%=dispWork%>">
                    <%// Mod 2016/04/13 arc yano #3480 %>
                請求先<%=Html.TextBox(namePrefix + "CustomerClaimCode", line.CustomerClaimCode, new { id = idPrefix + "CustomerClaimCode", @class = "alphanumeric readonly",@readonly="readonly", style="width:80px", maxlength = "10" })%><%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "CustomerClaimCode", idPrefix + "CustomerClaimName", "'/CustomerClaim/CriteriaDialog?SWCustomerClaimClass=' + document.getElementById('" + idPrefix + "SWCustomerClaimClass').value", "0", "calcTotalServiceAmount()", idPrefix + "SerchCustomer" });%><%//Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "CustomerClaimCode", idPrefix + "CustomerClaimName", "'/CustomerClaim/CriteriaDialog'", "1"});%><%//2014/08/03 arc yano id追加漏れ対応 %><%=Html.TextBox(namePrefix + "CustomerClaimName", line.CustomerClaim != null ? line.CustomerClaim.CustomerClaimName : "", new { id = idPrefix + "CustomerClaimName", @class="readonly", @readonly="readonly", style = "width:180px" })%>
                    <%=Html.Hidden(namePrefix + "CCCustomerClaimClass", line.CCCustomerClaimClass, new { id = idPrefix + "CCCustomerClaimClass"})%>
                </span>
                <span id="Span17" style="display:<%=dispService%>">
                    <%=Html.DropDownList(namePrefix + "LineType", ((List<IEnumerable<SelectListItem>>)ViewData["LineTypeList"])[i], new { id = idPrefix + "LineType", style = "width:50px", @disabled = "disabled" }) %>
                    <%=Html.Hidden(namePrefix + "LineType", line.LineType, new { id = idPrefix + "LineType"})%>
                    <%if(line.LineType!=null && line.LineType.Equals("001")){ %>
                        <%=Html.TextBox(namePrefix + "EmployeeNumber", line.Employee != null ? line.Employee.EmployeeNumber : "", new { id = idPrefix + "EmployeeNumber", @class = "alphanumeric readonly", style = "color:#0000ff;width:35px", maxlength = "20", onblur = "GetEngineerFromCode("+i+",'EmployeeNumber')" })%><%=Html.TextBox(namePrefix + "EmployeeCode", line.EmployeeCode, new { style = "color:#0000ff;width:50px", @class = "alphanumeric readonly", maxlength = "50", @readonly="readonly" })%><%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "EmployeeCode", idPrefix + "EmployeeName", "'/Employee/CriteriaDialog'", "1" });%><%//2014/08/03 arc yano id追加漏れ対応 %><%=Html.TextBox(namePrefix + "EmployeeName", line.Employee != null ? line.Employee.EmployeeName : "", new { id = idPrefix + "EmployeeName", @class="readonly", @readonly="readonly", style = "color:#0000ff;width:150px" })%>
                    <%}else{ %>
                        <%=Html.TextBox(namePrefix + "SupplierCode", line.SupplierCode, new { id = idPrefix + "SupplierCode", style = "color:#0000ff;width:91px", @class = "alphanumeric readonly", maxlength = "10",@readonly="readonly" })%><%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "SupplierCode", idPrefix + "SupplierName", "'/Supplier/CriteriaDialog/?OutsourceFlag=1'", "1" });%><%//2014/08/03 arc yano id追加漏れ対応 %><%=Html.TextBox(namePrefix + "SupplierName", line.Supplier != null ? line.Supplier.SupplierName : "", new { id = idPrefix + "SupplierName", @class="readonly", @readonly="readonly", style = "color:#0000ff;width:150px" })%>
                    <%} %>
                </span>
            </td>
            <!-- 連絡事項 -->
            <td nowrap style="width:100px">
                <%// Add 2015/08/05 arc nakayama #3221_無効となっている部門や車両等が設定されている納車確認書が印刷出来ない  連絡事項だけが入力可能だったのでリードオンリーに修正%>
                <%=Html.TextBox(namePrefix + "RequestComment", line.RequestComment, new { id = idPrefix + "RequestComment", @class = "readonly", @readonly="readonly",style = "width:93px" }) %>
            </td> 
           <%} %>
            <% // Add 2014/06/09 arc yano 高速化対応 削除用のフラグを追加%>
            <!-- 削除フラグ -->
           <%=Html.Hidden(namePrefix + "CliDelFlag", line.CliDelFlag, new{ id = idPrefix + "CliDelFlag" })%>
        </tr>
    <%} %>
    <%--ookubo.add--%>
    <%=Html.Hidden("LineCount", Model.ServiceSalesLine.Count)%>
    </tbody><% //Add 2014/06/06 arc yano %>
    </table>