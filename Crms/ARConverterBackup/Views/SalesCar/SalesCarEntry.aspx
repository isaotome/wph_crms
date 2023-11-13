<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.SalesCar>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "SalesCar", FormMethod.Post))
      { %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"])%>
<%=Html.Hidden("close", ViewData["close"]) %>
<div id="input-form">
    <%=Html.ValidationSummary()%>
    <br />
    <%if(ViewData["ErrorSalesCar"]!=null){ %>
            <%Html.RenderPartial("VinErrorControl", ViewData["ErrorSalesCar"]); %>
    <%} %>
    <%if(ViewData["update"]!=null && ViewData["update"].Equals("1")){ %>
        <%Html.RenderPartial("CarOwnershipChangeEntry",Model); %>
        <br />
    <%} %>
    <%Html.RenderPartial("CarBasicInformationEntry", Model); %>
    <br />
   <%Html.RenderPartial("CarInspectionEntry", Model); %>
    <br />
    <%Html.RenderPartial("CarDetailInformationEntry", Model); %>
    <br />
    <table class="input-form">
      <tr>
        <th class="input-form-title" colspan="2">管理情報</th>
      </tr>
      <tr>
        <th style="width:100px">ステータス</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
      <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
        {%>
        <td><%=Html.RadioButton("DelFlag", "0")%>有効<%=Html.RadioButton("DelFlag", "1")%>無効</td>
      <%} else {%>
        <td><%=Html.RadioButton("DelFlag", "0", true)%>有効<%=Html.RadioButton("DelFlag", "1", new { disabled = true })%>無効</td>
      <%} %>
      </tr>
    </table>
</div>
<%} %>
<br />
</asp:Content>
