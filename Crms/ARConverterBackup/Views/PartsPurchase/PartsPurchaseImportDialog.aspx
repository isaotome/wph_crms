<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.PurchaseExcelImportList>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Excel(LinkEntry)取込画面
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% 
/*-----------------------------------------------------------------------------------------------------------------------------*/
/* 更新日：                                                                                                                    */
/*          Mod 2018/01/15 arc yano #3832 LinkEntryの[オーダー番号]を部品入荷データの[メーカーオーダー番号]に設定              */
/*          Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応                                                               */
/*                                        ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更                           */
/*                                        ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする                          */
/*-----------------------------------------------------------------------------------------------------------------------------*/        
%>
    <%using (Html.BeginForm("ImportDialog", "PartsPurchase", FormMethod.Post, new { enctype = "multipart/form-data" }))
      { %>
    <%=Html.Hidden("ErrFlag", ViewData["ErrFlag"]) %>
    <%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:読み込み / 2:取り込み )%>
<table class="command">
   <tr>
       <td onclick="window.close();"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
   </tr>
</table>
<div id="import-form">
<br />
<div id ="validSummary">
    <%=Html.ValidationSummary()%>
</div>
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">ファイルパス</th>
        <td><input type="file" name="importFile" size="50" />&nbsp<input type="button" value="読み取り" onclick = "document.getElementById('validSummary').style.display = 'none'; document.getElementById('RequestFlag').value = '1'; dispProgresseddispTimer('PartsPurchase', 'UpdateMsg'); formSubmit();"/></td>
    </tr>
</table>
</div>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>

<br />
    <table class="list">
        <tr>
            <th></th>
            <th style="white-space:nowrap;text-align:left">
                インボイス番号
            </th>
            <th style="white-space:nowrap;text-align:left">
                発注伝票番号
            </th>
            <th style="white-space:nowrap;text-align:left">
                入荷予定日
            </th>
            <th style="white-space:nowrap;text-align:left">
                入荷ステータス
            </th>
            <%//Add 2018/01/15 arc yano #3832 %>
            <th style="white-space:nowrap;text-align:left">
                メーカーオーダー番号
            </th>
            <th style="white-space:nowrap;text-align:left">
                仕入先名
            </th>
            <th style="white-space:nowrap;text-align:left">
                メーカー部品番号
            </th>
            <th style="white-space:nowrap;text-align:left">
                部品名
            </th>
            <th style="white-space:nowrap;text-align:left">
                仕入単価
            </th>
            <th style="white-space:nowrap;text-align:left">
                数量
            </th>
            <th style="white-space:nowrap;text-align:left">
                仕入金額
            </th>
        </tr>


        <%int i = 0;%>
        <%foreach (var a in Model)
          {
              string namePrefix = string.Format("ImportLine[{0}].", i);
              string idPrefix = string.Format("ImportLine[{0}]_", i);
        %>
        <tr>
            <!--行番号-->
            <td style="white-space:nowrap;text-align:left;">
                <%=Html.TextBox(namePrefix + "LineNumber", (i + 1).ToString(), new { id = idPrefix + "LineNumber", style = "width:20px", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--インボイス番号-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "InvoiceNo", a.InvoiceNo, new { id = idPrefix + "InvoiceNo", @class = "readonly", @readonly = "readonly" ,style = "width:120px"})%>
            </td>
            <!--発注伝票番号-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "PurchaseOrderNumber", a.PurchaseOrderNumber, new { id = idPrefix + "PurchaseOrderNumber", @class = "readonly", @readonly = "readonly" ,style = "width:120px;"})%>
            </td>
            <!--入荷予定日-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "PurchasePlanDate", a.PurchasePlanDate, new { id = idPrefix + "PurchasePlanDate", @class = "readonly", @readonly = "readonly" ,size="10", style = "text-align:right;"})%>
            </td>
            <!--入荷ステータス-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "PurchaseStatusName", a.PurchaseStatusName, new { id = idPrefix + "PurchaseStatus", @class = "readonly", @readonly = "readonly" ,style = "width:120px;"})%>
            </td>
            <!--メーカーオーダー番号--> <%//Add 2018/01/15 arc yano #3832 %>
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "MakerOrderNumber", a.MakerOrderNumber, new { id = idPrefix + "MakerOrderNumber", @class = "readonly", @readonly = "readonly" ,style = "width:120px;"})%>
            </td>
            <!--仕入先名-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "SupplierName", a.SupplierName, new { id = idPrefix + "SupplierName", @class = "readonly", @readonly = "readonly" ,style = "width:330px;"})%>
            </td>
            <!--メーカー部品番号-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "MakerPartsNumber", a.MakerPartsNumber, new { id = idPrefix + "MakerPartsNumber", @class = "readonly", @readonly = "readonly" ,style = "width:120px;"})%>
            </td>
            <!--部品名-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "PartsNameJp", a.PartsNameJp, new { id = idPrefix + "PartsNameJp", @class = "readonly", @readonly = "readonly" ,style = "width:330px"})%>
            </td>
            <!--単価-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Price", string.Format("{0:N0}", a.Price), new { id = idPrefix + "Price", @class = "readonly money", @readonly = "readonly" ,style = "width:80px;text-align:right;"})%>
            </td>
            <!--数量-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Quantity", string.Format("{0:F2}",a.Quantity), new { id = idPrefix + "Quantity", @class = "readonly", @readonly = "readonly" ,style = "width:80px;text-align:right;"})%>
            </td>
            <!--金額-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Amount", string.Format("{0:N0}", a.Amount), new { id = idPrefix + "Amount", @class = "readonly money", @readonly = "readonly" ,style = "width:80px;text-align:right;"})%>
            </td>

            <!--非表示項目-->
            <!--Webオーダー番号-->
            <%//Del 2018/01/15 arc yano #3832%>
            <%//=Html.Hidden(namePrefix + "WebOrderNumber", a.WebOrderNumber, new { id = idPrefix + "WebOrderNumber"})%>
            <!--部品番号-->
            <%=Html.Hidden(namePrefix + "PartsNumber", a.PartsNumber, new { id = idPrefix + "PartsNumber"})%>
            <!--仕入先コード-->
            <%=Html.Hidden(namePrefix + "SupplierCode", a.SupplierCode, new { id = idPrefix + "SupplierCode"})%>
            <!--部門コード-->
            <%=Html.Hidden(namePrefix + "DepartmentCode", a.DepartmentCode, new { id = idPrefix + "DepartmentCode"})%>
            <!--受注伝票番号-->
            <%=Html.Hidden(namePrefix + "SlipNumber", a.SlipNumber, new { id = idPrefix + "SlipNumber"})%>
        </tr>
        <%i++;
          } %>
        <tr>
            <td style="text-align:right" colspan="12">
                <%if(ViewData["ErrFlag"].Equals("0")) {%>
                    <input type="button" value="取込実行" onclick="document.forms[0].enctype.value = 'application/x-www-form-urlencoded'; document.getElementById('RequestFlag').value = '2'; formSubmit()" />
                <%}else{ %>
                    <input type="button" value="取込実行" disabled="disabled" />
                <%} %>

                <input type="button" value="キャンセル" onclick="document.forms[0].enctype.value = 'application/x-www-form-urlencoded'; document.getElementById('RequestFlag').value = '3'; formSubmit()" />
            </td>
        </tr>
    </table>
    <div id="Div1" style="display:none">
    <img id="Img1" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
    </div>

<br />
    <%} %>
</asp:Content>