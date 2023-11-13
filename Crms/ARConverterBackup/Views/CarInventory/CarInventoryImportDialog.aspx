<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.CarInventoryExcelImportList>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    棚卸表取込画面
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
    // ------------------------------------------------------------------
    // 画面名：棚卸表取込画面
    // 作成日：2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
    // 更新日：
    // ------------------------------------------------------------------
%>
    <%using (Html.BeginForm("ImportDialog", "CarInventory", FormMethod.Post, new { enctype = "multipart/form-data" }))
      { %>
    <%=Html.Hidden("ErrFlag", ViewData["ErrFlag"]) %>
    <%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:読み込み / 2:取り込み )%>
    <%=Html.Hidden("InventoryMonth", ViewData["InventoryMonth"]) %>
    <%=Html.Hidden("WarehouseCode", ViewData["WarehouseCode"]) %>
    <%//=Html.Hidden("LocationCode", ViewData["LocationCode"]) %>
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
        <td><input type="file" name="importFile" size="50" />&nbsp<input type="button" value="読み取り" onclick = "document.getElementById('validSummary').style.display = 'none'; document.getElementById('RequestFlag').value = '1'; dispProgresseddispTimer('CarInventory', 'UpdateMsg'); DisplayImage('UpdateMsg', '0'); formSubmit();"/></td>
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
                ロケーションコード
            </th>
             <th style="white-space:nowrap;text-align:left">
                ロケーション名
            </th>
            <th style="white-space:nowrap;text-align:left">
                管理番号
            </th>
            <th style="white-space:nowrap;text-align:left">
                車台番号
            </th>
            <th style="white-space:nowrap;text-align:left">
                新中区分
            </th>
            <th style="white-space:nowrap;text-align:left">
                在庫区分
            </th>
            <th style="white-space:nowrap;text-align:left">
                ブランド名
            </th>
            <th style="white-space:nowrap;text-align:left">
                車種名
            </th>
            <th style="white-space:nowrap;text-align:left">
                系統色
            </th>
            <th style="white-space:nowrap;text-align:left">
                車両カラーコード
            </th>
            <th style="white-space:nowrap;text-align:left">
                車両カラー名
            </th>
            <th style="white-space:nowrap;text-align:left">
                車両登録番号
            </th>
            <th style="white-space:nowrap;text-align:left">
                実棚
            </th>
            <th style="white-space:nowrap;text-align:left">
                誤差理由
            </th>
            <th style="white-space:nowrap;text-align:left">
                備考
            </th>
        </tr>

        <%int i = 0;%>
        <%foreach (var a in Model)
          {
              string namePrefix = string.Format("line[{0}].", i);
              string idPrefix = string.Format("line[{0}]_", i);
        %>
        <tr>
            <!--行番号-->
            <td style="white-space:nowrap;text-align:left;">
                <%=Html.TextBox(namePrefix + "LineNumber", (i + 1).ToString(), new { id = idPrefix + "LineNumber", style = "width:20px", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--ロケーションコード-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "LocationCode", a.LocationCode, new { id = idPrefix + "LocationCode",size="5", @class = "readonly", @readonly = "readonly"})%>
            </td>
             <!--ロケーション名-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "LocationName", a.LocationName, new { id = idPrefix + "LocationName",size="20", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--管理番号-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "SalesCarNumber", a.SalesCarNumber, new { id = idPrefix + "SalesCarNumber",size="10", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--車台番号-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Vin", a.Vin, new { id = idPrefix + "Vin", size = "20", @class = "readonly", @readonly = "readonly" })%>
            </td>
            <!--新中区分-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "NewUsedTypeName", a.NewUsedTypeName, new { id = idPrefix + "NewUsedTypeName", size="3", @class = "readonly", @readonly = "readonly" })%>
                <%=Html.Hidden(namePrefix + "NewUsedType", a.NewUsedType, new { id = idPrefix + "NewUsedType" })%>
            </td>
            <!--在庫区分-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "CarStatusName", a.CarStatusName, new { id = idPrefix + "CarStatusName" , size="5", @class = "readonly", @readonly = "readonly" })%>
                <%=Html.Hidden(namePrefix + "CarStatus", a.CarStatus, new { id = idPrefix + "CarStatus" })%>
            </td>
            <!--ブランド名-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "CarBrandName", a.CarBrandName, new { id = idPrefix + "CarBrandName", @class = "readonly", @readonly = "readonly" , size="5"})%>
            </td>
            <!--車種名-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "CarName", a.CarName, new { id = idPrefix + "CarName", @class = "readonly", @readonly = "readonly" , size="20"})%>
            </td>
            <!--系統色-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ColorType",  a.ColorType, new { id = idPrefix + "ColorType", @class = "readonly", @readonly = "readonly" , size="3"})%>
            </td>
             <!--車両カラーコード-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ExteriorColorCode",  a.ExteriorColorCode, new { id = idPrefix + "ExteriorColorCode", @class = "readonly", @readonly = "readonly" , size="5"})%>
            </td>
            <!--車両カラ－名-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ExteriorColorName",  a.ExteriorColorName, new { id = idPrefix + "ExteriorColorName", @class = "readonly", @readonly = "readonly" , size="20"})%>
            </td>
             <!--車両登録番号-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "RegistrationNumber",  a.RegistrationNumber, new { id = idPrefix + "RegistrationNumber", @class = "readonly", @readonly = "readonly" , size="20"})%>
            </td>
            <!--実棚-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "PhysicalQuantity", string.Format("{0:F0}",a.PhysicalQuantity), new { id = idPrefix + "PhysicalQuantity", style = "text-align:right;" , size = "3", @class = "readonly numeric", @readonly = "readonly", maxlength = 1})%>
            </td>
             <!--誤差理由-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "CommentName",  a.CommentName, new { id = idPrefix + "CommentName", @class = "readonly", @readonly = "readonly" ,style = "width:80px"})%>
                <%=Html.Hidden(namePrefix + "Comment", a.Comment, new { id = idPrefix + "Comment" })%>
            </td>
             <!--備考-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Summary",  a.Summary, new { id = idPrefix + "Summary", @class = "readonly", @readonly = "readonly" ,style = "width:80px"})%>
            </td>
           
        </tr>
        <%i++;
          } %>
        <tr>
            <td style="text-align:right" colspan="16">
                <%if(ViewData["ErrFlag"].Equals("0")) {%>
                    <input type="button" value="取込実行" onclick="document.forms[0].enctype.value = 'application/x-www-form-urlencoded'; document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); formSubmit()" />
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