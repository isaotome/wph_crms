<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.SetMenu>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    セットメニュー入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using(Html.BeginForm("Entry","SetMenu",FormMethod.Post)){ %>
<%=Html.Hidden("close",ViewData["close"]) %>
<%=Html.Hidden("update",ViewData["update"]) %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.ValidationSummary() %>
<br />
<table class="input-form">
    <tr>
        <th>セットメニューコード</th>
        <td><%if (ViewData["update"]!=null && ViewData["update"].Equals("1")) { %>
                <%=Html.TextBox("SetMenuCode", Model.SetMenuCode, new { size = "15",@readonly="readonly" })%>
            <%} else { %>
                <%=Html.TextBox("SetMenuCode", Model.SetMenuCode, new { @class = "alphanumeric", size = "15", maxlength = "11", onchange = "IsExistCode('SetMenuCode','SetMenu')" })%>
            <%} %>
        </td>
            
    </tr>
    <tr>
        <th>セットメニュー名</th>
        <% // Mod 2014/07/15 arc amii 既存バグ対応 DB項目のlengthと一致していなかった為、入力可能文字数を100→50に修正 %>
        <td><%=Html.TextBox("SetMenuName",Model.SetMenuName,new {size="25",maxlength="50"}) %></td>
    </tr>
    <tr>
        <th rowspan="2">会社</th>
        <td><%=Html.TextBox("CompanyCode",Model.CompanyCode,new {@class="alphanumeric",size="10",maxlength="3",onchange="GetNameFromCode('CompanyCode','CompanyName','Company')"}) %>&nbsp;<img alt="会社検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CompanyCode','CompanyName','/Company/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <td><span id="CompanyName"><%=CommonUtils.DefaultNbsp(Model.Company!=null ? Model.Company.CompanyName : "") %></span></td>
    </tr>
</table>
<%} %>
</asp:Content>