<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.QuoteMessage>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	見積メッセージマスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "QuoteMessage", FormMethod.Post))
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
        <th rowspan="2" style="width:100px">会社 *</th>
        <td><%if (ViewData["update"].Equals("1"))
              { %><input type="text" id="CompanyCode" name="CompanyCode" value="<%=Model.CompanyCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("CompanyCode", Model.CompanyCode, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('CompanyCode','CompanyName','Company')" })%>
               <img alt="会社検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CompanyCode', 'CompanyName', '/Company/CriteriaDialog')" /><%} %>
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="CompanyName"><%=Html.Encode(ViewData["CompanyName"])%></span></td>       
      </tr>
      <tr>
        <th>見積種別 *</th>
        <td><%if (ViewData["update"].Equals("1"))
              { %><input type="text" id="QuoteType" name="QuoteType" value="<%if (Model.c_QuoteType != null) {%><%=Html.Encode(Model.c_QuoteType.Name)%><%} else {%>不明<%} %>" readonly="readonly" size="50" /><%}
              else
              { %><%=Html.DropDownList("QuoteType", (IEnumerable<SelectListItem>)ViewData["QuoteTypeList"])%><%} %>
        </td>
      </tr>
      <tr>
        <th>メッセージ</th>
        <td><%=Html.TextBox("Description", Model.Description, new { size = 50, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>ステータス</th>
      <%if (ViewData["update"].Equals("1"))
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
