<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    納車リスト
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
//---------------------------------------------
// 機能  ：車両納車リスト検索画面
// 作成者：arc nakayama
// 作成日：2017/03/09
// 更新日：
//---------------------------------------------
%>

<%using (Html.BeginForm("Criteria", "CarSalesList", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("indexid", ViewData["indexid"])%>
<table class="input-form">
   <tr>
        <th>指定年度</th>
        <td style="width:auto; white-space:nowrap"><%=Html.DropDownList("TargetYear", (IEnumerable<SelectListItem>)ViewData["TargetYearList"], new { onchange = "formSubmit();"})%>&nbsp;年度</td>
   </tr>
</table>
<%} %>
<br />
<table>
    <tr>
        <td>
            <input type="button" value="すべて" onclick="document.getElementById('indexid').value = 0; changeDisplayCarSales('0');" />
            <input type="button" value="一般" onclick="document.getElementById('indexid').value = 1; changeDisplayCarSales('1');" />
            <input type="button" value="ＡＡ・業販" onclick="document.getElementById('indexid').value = 2; changeDisplayCarSales('2');" />
            <input type="button" value="デモ・自登" onclick="document.getElementById('indexid').value = 3; changeDisplayCarSales('3');" />
            <input type="button" value="依廃・他" onclick="document.getElementById('indexid').value = 4; changeDisplayCarSales('4');" />
        </td>
    </tr>
</table>
<br />
<% // --------すべて-------%>
<div id="0" style="<%=!(bool)ViewData["0_ListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("Ret0_List", "CarSalesList", new { TargetYearCode = ViewData["TargetYear"].ToString() });%>
</div>
<% // --------一般-------%>
<div id="1" style="<%=!(bool)ViewData["1_ListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("Ret1_List", "CarSalesList", new { TargetYearCode = ViewData["TargetYear"].ToString() });%>
</div>
<% // --------ＡＡ・業販-------%>
<div id="2" style="<%=!(bool)ViewData["2_ListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("Ret2_List", "CarSalesList", new { TargetYearCode = ViewData["TargetYear"].ToString() });%>
</div>
<% // --------デモ・自登-------%>
<div id="3" style="<%=!(bool)ViewData["3_ListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("Ret3_List", "CarSalesList", new { TargetYearCode = ViewData["TargetYear"].ToString() });%>
</div>
<% // --------依廃・他-------%>
<div id="4" style="<%=!(bool)ViewData["4_ListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("Ret4_List", "CarSalesList", new { TargetYearCode = ViewData["TargetYear"].ToString() });%>
</div>
<br/>
<br/>
</asp:Content>

