<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PartsLocation>" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品ロケーションマスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "PartsLocation", FormMethod.Post))
      { %>

<% //Mod 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 登録項目の変更(部門→倉庫) %>
<% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("fixedParts", ViewData["fixedParts"])%>
<%=Html.Hidden("fixedWhouse", ViewData["fixedWhouse"])%><% //Mod 2016/08/13 arc yano #3596 %>
<%=Html.Hidden("close", ViewData["close"]) %>
<div id="input-form">
    <%=Html.ValidationSummary()%>
    <br />
    <table class="input-form" style="width:700px">
      <tr>
        <th rowspan="2" style="width:100px">部品 *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if ((CommonUtils.DefaultString(ViewData["update"]).Equals("1")) || (CommonUtils.DefaultString(ViewData["fixedParts"]).Equals("1")))
              { %><input type="text" id="PartsNumber" name="PartsNumber" value="<%=Model.PartsNumber%>" readonly="readonly" class="readonly" size="25" /><%}
              else
              { %><%=Html.TextBox("PartsNumber", Model.PartsNumber, new { @class = "alphanumeric", size = 25, maxlength = 25, onblur = "GetNameFromCode('PartsNumber','PartsNameJp','Parts')" })%>
                  <img alt="部品検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('PartsNumber', 'PartsNameJp', '/Parts/CriteriaDialog')" /><%} %>
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="PartsNameJp"><%=Html.Encode(ViewData["PartsNameJp"])%></span></td>       
      </tr>
      <tr>
        <th rowspan="2">倉庫 *</th>
        <td>
            <%if ((CommonUtils.DefaultString(ViewData["update"]).Equals("1")) || (CommonUtils.DefaultString(ViewData["fixedWhouse"]).Equals("1")))
             { %><input type="text" id="WarehouseCode" name="WarehouseCode" value="<%=Model.WarehouseCode%>" readonly="readonly" class="readonly"/><%}
              else
              { %><%=Html.TextBox("WarehouseCode", Model.WarehouseCode, new { @class = "alphanumeric", maxlength = 6, onblur = "GetNameFromCode('WarehouseCode','WarehouseName','Warehouse')" })%>
                  <img alt="倉庫検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('WarehouseCode', 'WarehouseName', '/Warehouse/CriteriaDialog')" />
            <%} %>
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="WarehouseName"><%=Html.Encode(ViewData["WarehouseName"])%></span></td>       
      </tr>
      <tr>
        <th rowspan="2" style="width:100px">ロケーション</th>
           <td>
        <%--//MOD 2014/11/12 arc yano 不具合対応 新規作成時にはステータスをチェックしないように対応、また編集不可時にはテキストボックスの背景色を灰色に表示する--%>
        <%--//MOD 2014/10/30 ishii ステータスが無効の情報はロケーションを編集不可に変更--%>
      <%if (!string.IsNullOrEmpty(Model.DelFlag) && Model.DelFlag.Equals("1"))
        { %><input type="text" id="LocationCode" name="LocationCode" value="<%=Model.LocationCode%>" class ="readonly" readonly="readonly"/><%}
          else
        {%>
         <%=Html.TextBox("LocationCode", Model.LocationCode, new { @class = "alphanumeric", maxlength = 12, onblur = "GetNameFromCode('LocationCode','LocationName','Location')" })%>
         <img alt="ロケーション検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('LocationCode', 'LocationName', '/Location/CriteriaDialog')" />
         <%}%>
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="LocationName"><%=Html.Encode(ViewData["LocationName"])%></span></td>       
      </tr>
      <tr>
        <th>ステータス</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
      <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
        {%>
        <td><%=Html.RadioButton("DelFlag", "0")%>有効<%=Html.RadioButton("DelFlag", "1")%>無効</td>
      <%}
        else
        {%>
        <td><%=Html.RadioButton("DelFlag", "0", true)%>有効<%=Html.RadioButton("DelFlag", "1", new { disabled = true })%>無効</td>
      <%} %>
      </tr>
    </table>
</div>
<%} %>
<br />
</asp:Content>
