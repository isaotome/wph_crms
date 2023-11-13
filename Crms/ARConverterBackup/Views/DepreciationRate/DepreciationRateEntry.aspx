<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.DepreciationRate>" %>
<%@ Import Namespace="CrmsDao" %> 
<%--
    機能：償却率マスタ入力
    作成日：2018/06/06 arc yano #3883 タマ表改善 新規作成
--%>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    償却率マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Entry", "DepreciationRate", FormMethod.Post))
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
<%=Html.ValidationSummary()%>
<br />
<table class="input-form" style="width:500px">
    <tr>
        <th>耐用年数 *</th>
        <td>
            <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %>
                <%=Html.TextBox("UsefulLives",Model.UsefulLives,new {@class="numeric",size="3",maxlength="3",@readonly="readonly"}) %>
            <%}else{ %>
                <%=Html.TextBox("UsefulLives",Model.UsefulLives,new {@class="numeric",size="3",maxlength="3"}) %>
            <%} %>
        </td>
        <th>償却率 *</th>
        <td><%=Html.TextBox("Rate",Model.Rate,new {@class="numeric", size="10",maxlength="10"}) %></td>
    </tr>
     <tr>
        <th>改訂償却率 </th>
        <td><%=Html.TextBox("RevisedRate",Model.RevisedRate,new {@class="numeric", size="10",maxlength="10"}) %></td>
         <th>保証率 </th>
        <td><%=Html.TextBox("SecurityRatio",Model.SecurityRatio,new {@class="numeric", size="10",maxlength="10"}) %></td>
    </tr>
</table>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
