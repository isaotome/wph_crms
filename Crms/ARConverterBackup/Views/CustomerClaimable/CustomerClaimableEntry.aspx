<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CustomerClaimable>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	決済条件マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "CustomerClaimable", FormMethod.Post))
      { %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.Hidden("CustomerClaimCode", Model.CustomerClaimCode)%>
<%=Html.Hidden("close", ViewData["close"]) %>
<div id="input-form">
    <%=Html.ValidationSummary()%>
    <br />
    <table class="input-form" style="width:700px">
      <tr>
        <th rowspan="2" style="width:100px">支払種別 *</th>
        <td><%=Html.TextBox("PaymentKindCode", Model.PaymentKindCode, new { @class = "alphanumeric", maxlength = 10, onblur = "GetNameFromCode('PaymentKindCode','PaymentKindName','PaymentKind');IsExistCode2('CustomerClaimCode','PaymentKindCode','CustomerClaimable', 'PaymentKindName')" })%>
            <img alt="支払種別検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('PaymentKindCode', 'PaymentKindName', '/PaymentKind/CriteriaDialog')" />
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="PaymentKindName"><%=Html.Encode(ViewData["PaymentKindName"])%></span></td>       
      </tr>
    </table>
</div>
<%} %>
<br />
</asp:Content>
