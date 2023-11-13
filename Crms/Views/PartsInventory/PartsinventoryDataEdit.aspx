<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.PartsInventory>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
棚卸在庫データ編集
<%
    //-----------------------------------------------------------------------------
    //機能　：部品在庫棚卸データ編集画面
    //作成日：2017/07/27 arc yano #3781 部品在庫棚卸（棚卸在庫の修正）新規作成
    //更新日：
    //-----------------------------------------------------------------------------
%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("DataEdit", "PartsInventory", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %>
<%=Html.Hidden("InventoryMonth", ViewData["InventoryMonth"]) %>
<%=Html.Hidden("WarehouseCode", ViewData["WarehouseCode"]) %>
<table class="command">
   <tr>
       <td onclick="window.close();"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="ClearValidateMessage('ValidBlock'); DisplayImage('UpdateMsg', '0'); document.getElementById('RequestFlag').value = '10'; formSubmit();"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>

<br/>

<div id="search-form" style="display:block">
<br />
<div id="ValidBlock">
<%=Html.ValidationSummary() %>
</div>

<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
</div>
<br />
<br />

<table class="list" style ="width:100%">
    <tr>
        <th style="width: 20px; height: 20px; text-align: center;">
            <img alt="行追加" src="/Content/Images/plus.gif" style="cursor: pointer" onclick="ClearValidateMessage('ValidBlock'); DisplayImage('UpdateMsg', '0'); document.getElementById('RequestFlag').value = '11'; formSubmit();" />
        </th>
        <th style ="white-space:nowrap">ロケーションコード</th>
        <th style ="white-space:nowrap">ロケーション名</th>
        <th style ="white-space:nowrap">部品番号</th>
        <th style ="white-space:nowrap">部品名</th>
        <th style ="white-space:nowrap">実棚数</th>
        <th style ="white-space:nowrap">(引当済数)</th>
        <th style ="white-space:nowrap">コメント</th>
     </tr>
   
    <% int i = 0; %>
    <%
        foreach(var a in Model){
            string namePrefix = string.Format("line[{0}].", i);
            string idPrefix = string.Format("line[{0}]_", i);
    %>
    <tr>
        <td style="width: 22px; text-align: center">
            <img alt="行削除" src="/Content/Images/minus.gif" style="cursor: pointer" onclick="if(confirm('部品棚卸データを削除します。よろしければ「OK」ボタンをクリックしてください')){ ClearValidateMessage('ValidBlock'); DisplayImage('UpdateMsg', '0'); document.getElementById('RequestFlag').value = '12'; document.getElementById('DelLine').value  = '<%=i %>';formSubmit()}" />
        </td>
        <td style ="white-space:nowrap"><%//ロケーションコード%>
            <%if (a.NewRecFlag.Equals(true)){ %>
                <%=Html.TextBox(namePrefix + "LocationCode", a.LocationCode, new { id = idPrefix + "LocationCode", @class="alphanumeric", size = "8", onchange = "GetNameFromCode('" + idPrefix + "LocationCode', '" + idPrefix + "LocationName', 'Location', null, null, null, null, setCarInventoryHidden, '" + idPrefix + "LocationName' )" }) %>
                <img alt="ロケーション検索" id="LocationSearch" style="cursor: pointer" src="/Content/Images/Search.jpg" onclick="var callback = function() { GetNameFromCode('<%=idPrefix%>LocationCode', '<%=idPrefix%>LocationName', 'Location', null, null, null, null, setCarInventoryHidden, '<%=idPrefix%>LocationName' )}; openSearchDialog('<%=idPrefix%>LocationCode','<%=idPrefix%>LocationName','/Location/CriteriaDialog?WarehouseCode=<%=ViewData["WarehouseCode"] %>', null, null, null, null, callback); GetNameFromCode('<%=idPrefix%>LocationCode', '<%=idPrefix%>LocationName', 'Location', null, null, null, null, setCarInventoryHidden, '<%=idPrefix%>LocationName' )" /><%//Mod 2022/01/10 yano #4121%>
                <%--<img alt="ロケーション検索" id="LocationSearch" style="cursor: pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('<%=idPrefix%>LocationCode','<%=idPrefix%>LocationName','/Location/CriteriaDialog?WarehouseCode=<%=ViewData["WarehouseCode"] %>'); GetNameFromCode('<%=idPrefix%>LocationCode', '<%=idPrefix%>LocationName', 'Location', null, null, null, null, setCarInventoryHidden, '<%=idPrefix%>LocationName' )" />--%>
            <%}else{ %>
                <%=Html.Encode(a.LocationCode) %>
                <%=Html.Hidden(namePrefix + "LocationCode" , a.LocationCode, new {id = idPrefix + "LocationCode"})%>
            <%} %>
        </td>
        <td style ="white-space:nowrap"><%//ロケーション名称%>
            <span id = '<%=idPrefix + "LocationName" %>'><%=Html.Encode(a.LocationName) %></span>
            <%=Html.Hidden(namePrefix + "LocationName" , a.LocationName, new {id = idPrefix + "LocationName_hd"})%>
        </td>
        <td style ="white-space:nowrap"><%//部品番号%>
             <%if(a.NewRecFlag.Equals(true)){ %>
                <%=Html.TextBox(namePrefix + "PartsNumber", a.PartsNumber, new { @class = "alphanumeric", id = idPrefix + "PartsNumber", size = 15 , maxlength = 25 , onchange = "GetNameFromCode('" + idPrefix + "PartsNumber','"  + idPrefix + "PartsNameJp','Parts', null, null, null, null , setCarInventoryHidden, '" + idPrefix + "PartsNameJp' )"})%>
                <img alt="部品検索" id="PartsSearch" style="cursor: pointer" src="/Content/Images/Search.jpg" onclick="var callback = function() { GetNameFromCode('<%=idPrefix%>PartsNumber', '<%=idPrefix%>PartsNameJp', 'Parts', null, null, null, null, setCarInventoryHidden, '<%=idPrefix%>PartsNameJp' )}; openSearchDialog('<%=idPrefix%>PartsNumber','<%=idPrefix%>PartsNameJp','/Parts/CriteriaDialog', null, null, null, null, callback); GetNameFromCode('<%=idPrefix%>PartsNumber', '<%=idPrefix%>PartsNameJp', 'Parts', null, null, null, null, setCarInventoryHidden, '<%=idPrefix%>PartsNameJp' )" /><%//Mod 2022/01/10 yano #4121%>
                <%--<img alt="部品検索" id="PartsSearch" style="cursor: pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('<%=idPrefix%>PartsNumber','<%=idPrefix%>PartsNameJp','/Parts/CriteriaDialog'); GetNameFromCode('<%=idPrefix%>PartsNumber', '<%=idPrefix%>PartsNameJp', 'Parts', null, null, null, null, setCarInventoryHidden, '<%=idPrefix%>PartsNameJp' )" />--%> 
            <%}else{ %>
                <%=Html.Encode(a.PartsNumber) %>
                <%=Html.Hidden(namePrefix + "PartsNumber" , a.PartsNumber, new {id = idPrefix + "PartsNumber"})%>
             <%} %>
        </td>
        <td style ="white-space:nowrap"><%//部品名%>
            <span id = '<%=idPrefix + "PartsNameJp" %>'><%=Html.Encode(a.PartsNameJp) %></span>
            <%=Html.Hidden(namePrefix + "PartsNameJp" , a.PartsNameJp, new {id = idPrefix + "PartsNameJp_hd"})%>
        </td>
        <td style="text-align:right;white-space:nowrap"><%//実棚数%>
            <%=Html.TextBox(namePrefix + "PhysicalQuantity" , string.Format("{0:F1}", a.PhysicalQuantity), new {id = idPrefix + "PhysicalQuantity", @class = "numeric", size="4", onchange = " var ret = ChkQuantity('"+ idPrefix +"PhysicalQuantity', '" + idPrefix + "SavedPhysicalQuantity', /^\\d{1,7}(\\.\\d{1,1})?$/, '数量', '正の数(整数7桁以内、小数1桁以内)');"})%>
            <%=Html.Hidden(namePrefix + "SavedPhysicalQuantity", string.Format("{0:F1}", a.PhysicalQuantity), new {id = idPrefix + "SavedPhysicalQuantity"})%>
        </td>
        <td style="text-align:right;white-space:nowrap"><%//引当済数%>
            <%=Html.TextBox(namePrefix + "ProvisionQuantity" , string.Format("{0:F1}", a.ProvisionQuantity), new {id = idPrefix + "ProvisionQuantity", @class = "numeric", size="4", onchange = " var ret = ChkQuantity('"+ idPrefix +"ProvisionQuantity', '" + idPrefix + "SavedProvisionQuantity', /^\\d{1,7}(\\.\\d{1,1})?$/, '引当済数', '正の数(整数7桁以内、小数1桁以内)');"})%>
            <%=Html.Hidden(namePrefix + "SavedProvisionQuantity", string.Format("{0:F1}", a.ProvisionQuantity), new {id = idPrefix + "SavedProvisionQuantity"})%>
        </td>
        <td style="text-align:right;white-space:nowrap"><%//コメント%>
            <%=Html.TextBox(namePrefix + "Comment", a.Comment, new { id = idPrefix + "Comment", size = 15, maxlength = 120 })%> 
        </td>
        <%=Html.Hidden(namePrefix + "NewRecFlag", a.NewRecFlag, new { id = idPrefix + "NewRecFlag"}) %><%//新規追加フラグ %>
        <%=Html.Hidden(namePrefix + "InventoryId", a.InventoryId, new { id = idPrefix + "InventoryId"}) %><%//棚卸ID %>
    </tr>
    <%
          i++;
    } %>

      <%//削除行 %>
      <%=Html.Hidden("DelLine", 0 ) %>
</table>
<%}%>
</asp:Content>
