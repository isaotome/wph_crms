<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Loan>" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	割賦金マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "Loan", FormMethod.Post))
      { %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("close", ViewData["close"]) %>
<div id="input-form">
    <%=Html.ValidationSummary()%>
    <br />
    <table class="input-form" style="width:700px">
      <tr>
        <th style="width:100px">ローンコード *</th>
           <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="LoanCode" name="LoanCode" value="<%=Model.LoanCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("LoanCode", Model.LoanCode, new { @class = "alphanumeric", maxlength = 10, onblur = "IsExistCode('LoanCode','Loan')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>ローン名 *</th>
        <td><%=Html.TextBox("LoanName", Model.LoanName, new { size = 40, maxlength = 20 })%></td>
      </tr>
      <tr>
        <th rowspan="2">請求先 *</th>
        <td><%=Html.TextBox("CustomerClaimCode", Model.CustomerClaimCode, new { @class = "alphanumeric", maxlength = 10, onblur = "GetNameFromCode('CustomerClaimCode','CustomerClaimName','CustomerClaim')" })%>
            <img alt="請求先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerClaimCode', 'CustomerClaimName', '/CustomerClaim/CriteriaDialog')" />
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="CustomerClaimName"><%=Html.Encode(ViewData["CustomerClaimName"])%></span></td>       
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

</asp:Content>
