<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.CarInventory>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    確定データ編集画面
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
    // ------------------------------------------------------------------
        // 画面名：確定データ編集画面
    // 作成日：2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
    // 更新日：
    // ------------------------------------------------------------------
%>
    <%using (Html.BeginForm("DecidedDialog", "CarInventory", FormMethod.Post))
    { %>
    <%=Html.Hidden("UpdateButtonDisable", ViewData["UpdateButtonDisable"]) %>
    <%=Html.Hidden("ButtonDisable", ViewData["ButtonDisable"]) %>
    <%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:データ更新＆確定前チェック / 2:確定 )%>
    <%=Html.Hidden("InventoryMonth", ViewData["InventoryMonth"]) %>
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
            <td style="width: 22px; text-align: center">
                <img alt="行削除" src="/Content/Images/minus.gif" style="cursor: pointer" onclick="document.getElementById('RequestFlag').value = '1'; document.forms[0].action = '/CarInventory/DelLineDecided?targetId=<%=i %>';formSubmit()" />
            </td>
            <%//ロケーションコード%>
            <td style="white-space: nowrap"><%=Html.TextBox(namePrefix + "LocationCode", a.LocationCode, new { id = idPrefix + "LocationCode", @class="alphanumeric", size = "5", onchange = "GetNameFromCode('" + idPrefix + "LocationCode', '" + idPrefix + "LocationName', 'Location', null, null, null, null, setCarInventoryHidden, '" + idPrefix + "LocationName' )"}) %></td>
            <%//ロケーション名称%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>LocationName"><%=Html.Encode(a.LocationName) %></span><%=Html.Hidden(namePrefix + "LocationName", a.LocationName, new { id = idPrefix + "LocationName_hd"}) %></td>
            <%//管理番号%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>SalesCarNumber"><%=Html.Encode(a.SalesCarNumber) %></span><%=Html.Hidden(namePrefix + "SalesCarNumber", a.SalesCarNumber, new { id = idPrefix + "SalesCarNumber_hd"}) %></td>
            <td style="white-space: nowrap"><%//車台番号 %>
            <%=Html.TextBox(namePrefix + "Vin", a.Vin, new { style = "width:130px", maxlength = "20", id = idPrefix + "Vin", onchange = "GetSalesCarInfo('" + idPrefix + "Vin','" + idPrefix + "SalesCarNumber', new Array('"+ idPrefix + "CarBrandName','" + idPrefix + "CarName','" + idPrefix + "ColorType','" + idPrefix + "ExteriorColorCode','" + idPrefix + "ExteriorColorName','" + idPrefix + "SalesCarNumber','" + idPrefix + "RegistrationNumber'), null, null, null,null, null, setCarInventoryHidden);" }) %>
            <img alt="車両検索" id="SalesCarSearch" style="cursor: pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('<%=idPrefix%>SalesCarNumber','<%=idPrefix%>Vin','/SalesCar/CriteriaDialog'); GetMasterDetailFromCode('<%=idPrefix%>SalesCarNumber',new Array('<%=idPrefix%>CarBrandName','<%=idPrefix%>CarName','<%=idPrefix%>ColorType','<%=idPrefix%>SalesCarNumber','<%=idPrefix%>ExteriorColorCode','<%=idPrefix%>ExteriorColorName','<%=idPrefix%>RegistrationNumber'),'SalesCar', null, null, null, setCarInventoryHidden);" />     
            </td>
            <td style="white-space: nowrap"><%=Html.DropDownList(namePrefix + "NewUsedType", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["NewUsedTypeListLine"])[i], new { id = idPrefix + "NewUsedType" })%></td>
            <%//新中区分%>
            <td style="white-space: nowrap"><%=Html.DropDownList(namePrefix + "CarStatus", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["CarStatusListLine"])[i], new { id = idPrefix + "CarStatus" })%></td>
            <%//在庫区分%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>CarBrandName"><%=Html.Encode(a.CarBrandName) %></span><%=Html.Hidden(namePrefix + "CarBrandName", a.CarBrandName, new { id = idPrefix + "CarBrandName_hd"}) %></td>
            <%//ブランド名%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>CarName"><%=Html.Encode(a.CarName) %></span><%=Html.Hidden(namePrefix + "CarName", a.CarName, new { id = idPrefix + "CarName_hd"}) %></td>
            <%//車種名%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>ColorType"><%=Html.Encode(a.ColorType) %></span><%=Html.Hidden(namePrefix + "ColorType", a.ColorType, new { id = idPrefix + "ColorType_hd"}) %></td>
            <%//系統色%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>ExteriorColorCode"><%=Html.Encode(a.ExteriorColorCode) %></span><%=Html.Hidden(namePrefix + "ExteriorColorCode", a.ExteriorColorCode, new { id = idPrefix + "ExteriorColorCode_hd"}) %></td>
            <%//車両カラーコード%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>ExteriorColorName"><%=Html.Encode(a.ExteriorColorName) %></span><%=Html.Hidden(namePrefix + "ExteriorColorName", a.ExteriorColorName, new { id = idPrefix + "ExteriorColorName_hd"}) %></td>
            <%//車両カラー名%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>RegistrationNumber"><%=Html.Encode(a.RegistrationNumber) %></span><%=Html.Hidden(namePrefix + "RegistrationNumber", a.RegistrationNumber, new { id = idPrefix + "RegistrationNumber_hd"}) %></td>
            <%//車両登録番号%>
            <td style="text-align: right"><%//実棚%>
                <%=Html.TextBox(namePrefix + "PhysicalQuantity", string.Format("{0:F0}",a.PhysicalQuantity), new { @class = "numeric", id = idPrefix + "PhysicalQuantity", size = 1, maxlength = 1, onchange ="var ret = ChkPhysicalQuantityForCarInventory('"+ idPrefix +"PhysicalQuantity', '" + idPrefix + "SavedPhsycalQuantity', false)"})%><%//Mod 2017/12/18 arc yano #3840 %>
                <%=Html.Hidden(namePrefix + "SavedPhsycalQuantity", string.Format("{0:F0}", a.PhysicalQuantity), new {id = idPrefix + "SavedPhsycalQuantity"})%>
            </td>
            <td style="white-space: nowrap""><%//誤差理由%>
                <%=Html.DropDownList(namePrefix + "Comment", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["CommentList"])[i], new { id = idPrefix + "Comment" })%>
            </td>
            <td style="white-space: nowrap""><%//備考%>
                <%=Html.TextBox(namePrefix + "Summary", a.Summary, new { id = idPrefix + "Summary", size = 15, maxlength = 255 })%>
            </td>
            <%=Html.Hidden(namePrefix + "InventoryId", a.InventoryId) %>
        <%i++;
          } %>
        <tr>
            <td style="text-align:right" colspan="16">
                <%if (ViewData["UpdateButtonDisable"].Equals("0")){ %>
                    <input type="button" value="更新" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); formSubmit()" />
                <%}else{ %>
                    <input type="button" value="更新" disabled="disabled" />
                <%} %>
                <%if (ViewData["ButtonDisable"].Equals("0"))
                  {%>
                    <input type="button" value="確定" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); formSubmit()" />
                <%}else{ %>
                    <input type="button" value="確定" disabled="disabled" />
                <%} %>

                <!--<input type="button" value="キャンセル" onclick="document.getElementById('RequestFlag').value = '3'; formSubmit()" />-->
            </td>
        </tr>
    </table>

<br />
    <%} %>
</asp:Content>