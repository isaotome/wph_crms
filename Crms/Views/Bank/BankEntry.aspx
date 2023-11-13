<%@ Page Title="銀行マスタ入力" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Bank>" %>
<%@ Import Namespace="CrmsDao" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	銀行マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Entry", "Bank", FormMethod.Post)) {%>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.ValidationSummary()%>
<br />
<table class="input-form" style="width:700px">
    <tr>
        <th style="width:100px">銀行コード *</th>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              {%>
                <%=Html.TextBox("BankCode",Model.BankCode,new {@class = "readonly", size = 10, @readonly = "readonly"})%>
            <%}else{%>
                <%=Html.TextBox("BankCode", Model.BankCode, new { @class = "alphanumeric", size = 10, maxlength = 4, onblur = "IsExistCode('BankCode','Bank')" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>銀行名</th>
        <td>
            <%=Html.TextBox("BankName", Model.BankName, new { size = 30, maxlength = 50 }) %>
        </td>
    </tr>
    <tr>
        <th>ステータス</th>
         <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
          {%>
        <td><%=Html.RadioButton("DelFlag", "0")%>有効<%=Html.RadioButton("DelFlag", "1")%>無効</td>
        <%} else {%>
        <td><%=Html.RadioButton("DelFlag", "0", true)%>有効<%=Html.RadioButton("DelFlag", "1", new { disabled = true })%>無効</td>
        <%} %>
    </tr>
</table>
<br />
<br />
<table class="input-form" style="width:700px">
    <tr>
        <th class="input-form-title" colspan="3">支店リスト</th>
    </tr>
    <tr>
        <th style="width:15px">
            <img alt="追加" style="cursor: pointer" src="/Content/Images/plus.gif" onclick="document.forms[0].action='/Bank/AddBranch';formSubmit()" />
        </th>
        <th style="width:80px">支店コード</th>
        <th>支店名</th>
    </tr>
</table>
<div style="width: 718px; height: 300px; overflow-y: scroll">
    <table class="input-form" style="width: 700px">
        <%if (Model.Branch != null) { %>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <% List<CrmsDao.Branch> branches = Model.Branch.Where(x => CommonUtils.DefaultString(x.DelFlag).Equals("0")).ToList(); %>
            <%for (int i = 0; i < branches.Count(); i++) { %>
            <%string prefix = string.Format("branches[{0}].", i); %>
            <% // <!--//2014/05/29 vs2012対応 arc yano 各コントロールにidを追加 %>
            <%string idprefix = string.Format("branches[{0}]_", i); %>
            <tr>
                <td style="width:15px">
                    <img alt="削除" style="cursor: pointer" src="/Content/Images/minus.gif" onclick="document.forms[0].action='/Bank/DelBranch/<%=i %>';formSubmit()" />
                    <%=Html.Hidden(prefix + "BankCode", branches[i].BankCode, new { id = idprefix + "BankCode" })%>
                    <%=Html.Hidden(prefix + "DelFlag",branches[i].DelFlag, new { id = idprefix + "DelFlag" }) %>
                </td>
                <td style="width:80px">
                    <%=Html.TextBox(prefix + "BranchCode", branches[i].BranchCode, new { id = idprefix + "BranchCode", @class = "alphanumeric", size = 3, maxlength = 3 })%>
                </td>
                <td>
                    <%=Html.TextBox(prefix + "BranchName", branches[i].BranchName, new {  id = idprefix + "BranchName", size = 30, maxlength = 50 })%>
                </td>
            </tr>
            <%} %>
        <%} %>
    </table>
</div>
<%} %>     
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
