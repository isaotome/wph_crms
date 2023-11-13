<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.GetCarSalesEmployeeList_Result>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    納車リスト（担当者別）
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("CarSalesDetailEmployeeCriteriaDialog", "CarSalesList", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("SelectYearCode", ViewData["SelectYearCode"])%>
<%=Html.Hidden("DepartmentCode", ViewData["DepartmentCode"])%>
<%=Html.Hidden("DepartmentName", ViewData["DepartmentName"]) %>
<%=Html.Hidden("Jul_Cnt", ViewData["Jul_Cnt"]) %>
<%=Html.Hidden("Aug_Cnt", ViewData["Aug_Cnt"]) %>
<%=Html.Hidden("Sep_Cnt", ViewData["Sep_Cnt"]) %>
<%=Html.Hidden("Oct_Cnt", ViewData["Oct_Cnt"]) %>
<%=Html.Hidden("Nov_Cnt", ViewData["Nov_Cnt"]) %>
<%=Html.Hidden("Dec_Cnt", ViewData["Dec_Cnt"]) %>
<%=Html.Hidden("Jan_Cnt", ViewData["Jan_Cnt"]) %>
<%=Html.Hidden("Feb_Cnt", ViewData["Feb_Cnt"]) %>
<%=Html.Hidden("Mar_Cnt", ViewData["Mar_Cnt"]) %>
<%=Html.Hidden("Apr_Cnt", ViewData["Apr_Cnt"]) %>
<%=Html.Hidden("May_Cnt", ViewData["May_Cnt"]) %>
<%=Html.Hidden("Jun_Cnt", ViewData["Jun_Cnt"]) %>
<br />
<br />
<table>
    <tr>
        <td>
            <input type="button" value="すべて" onclick="changeDisplayCarSales('0');" />
            <input type="button" value="一般" onclick="changeDisplayCarSales('1');" />
            <input type="button" value="ＡＡ・業販" onclick="changeDisplayCarSales('2');" />
            <input type="button" value="デモ・自登" onclick="changeDisplayCarSales('3');" />
            <input type="button" value="依廃・他" onclick="changeDisplayCarSales('4');" />
        </td>
    </tr>
</table>
<br />
<% // --------すべて(担当者別)-------%>
<div id="0" style="<%=!(bool)ViewData["0_ListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("Ret0_EmployeeList", "CarSalesList", 
          new { TargetYearCode = ViewData["SelectYearCode"].ToString(),
                DepartmentCode = ViewData["DepartmentCode"].ToString(),
                DepartmentName = ViewData["DepartmentName"].ToString(),
                Jul_Cnt = (ViewData["Jul_Cnt"] != null ? ViewData["Jul_Cnt"].ToString() : ""),
                Aug_Cnt = (ViewData["Aug_Cnt"] != null ? ViewData["Aug_Cnt"].ToString() : ""),
                Sep_Cnt = (ViewData["Sep_Cnt"] != null ? ViewData["Sep_Cnt"].ToString() : ""),
                Oct_Cnt = (ViewData["Oct_Cnt"] != null ? ViewData["Oct_Cnt"].ToString() : ""),
                Nov_Cnt = (ViewData["Nov_Cnt"] != null ? ViewData["Nov_Cnt"].ToString() : ""),
                Dec_Cnt = (ViewData["Dec_Cnt"] != null ? ViewData["Dec_Cnt"].ToString() : ""),
                Jan_Cnt = (ViewData["Jan_Cnt"] != null ? ViewData["Jan_Cnt"].ToString() : ""),
                Feb_Cnt = (ViewData["Feb_Cnt"] != null ? ViewData["Feb_Cnt"].ToString() : ""),
                Mar_Cnt = (ViewData["Mar_Cnt"] != null ? ViewData["Mar_Cnt"].ToString() : ""),
                Apr_Cnt = (ViewData["Apr_Cnt"] != null ? ViewData["Apr_Cnt"].ToString() : ""),
                May_Cnt = (ViewData["May_Cnt"] != null ? ViewData["May_Cnt"].ToString() : ""),
                Jun_Cnt = (ViewData["Jun_Cnt"] != null ? ViewData["Jun_Cnt"].ToString() : "")
          });%>
</div>
<% // --------一般(担当者別)-------%>
<div id="1" style="<%=!(bool)ViewData["1_ListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("Ret1_EmployeeList", "CarSalesList",
          new { TargetYearCode = ViewData["SelectYearCode"].ToString(),
                DepartmentCode = ViewData["DepartmentCode"].ToString(),
                DepartmentName = ViewData["DepartmentName"].ToString(),
                Jul_Cnt = (ViewData["Jul_Cnt"] != null ? ViewData["Jul_Cnt"].ToString() : ""),
                Aug_Cnt = (ViewData["Aug_Cnt"] != null ? ViewData["Aug_Cnt"].ToString() : ""),
                Sep_Cnt = (ViewData["Sep_Cnt"] != null ? ViewData["Sep_Cnt"].ToString() : ""),
                Oct_Cnt = (ViewData["Oct_Cnt"] != null ? ViewData["Oct_Cnt"].ToString() : ""),
                Nov_Cnt = (ViewData["Nov_Cnt"] != null ? ViewData["Nov_Cnt"].ToString() : ""),
                Dec_Cnt = (ViewData["Dec_Cnt"] != null ? ViewData["Dec_Cnt"].ToString() : ""),
                Jan_Cnt = (ViewData["Jan_Cnt"] != null ? ViewData["Jan_Cnt"].ToString() : ""),
                Feb_Cnt = (ViewData["Feb_Cnt"] != null ? ViewData["Feb_Cnt"].ToString() : ""),
                Mar_Cnt = (ViewData["Mar_Cnt"] != null ? ViewData["Mar_Cnt"].ToString() : ""),
                Apr_Cnt = (ViewData["Apr_Cnt"] != null ? ViewData["Apr_Cnt"].ToString() : ""),
                May_Cnt = (ViewData["May_Cnt"] != null ? ViewData["May_Cnt"].ToString() : ""),
                Jun_Cnt = (ViewData["Jun_Cnt"] != null ? ViewData["Jun_Cnt"].ToString() : "")
          });%>
</div>
<% // --------ＡＡ・業販(担当者別)-------%>
<div id="2" style="<%=!(bool)ViewData["2_ListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("Ret2_EmployeeList", "CarSalesList",
          new { TargetYearCode = ViewData["SelectYearCode"].ToString(),
                DepartmentCode = ViewData["DepartmentCode"].ToString(),
                DepartmentName = ViewData["DepartmentName"].ToString(),
                Jul_Cnt = (ViewData["Jul_Cnt"] != null ? ViewData["Jul_Cnt"].ToString() : ""),
                Aug_Cnt = (ViewData["Aug_Cnt"] != null ? ViewData["Aug_Cnt"].ToString() : ""),
                Sep_Cnt = (ViewData["Sep_Cnt"] != null ? ViewData["Sep_Cnt"].ToString() : ""),
                Oct_Cnt = (ViewData["Oct_Cnt"] != null ? ViewData["Oct_Cnt"].ToString() : ""),
                Nov_Cnt = (ViewData["Nov_Cnt"] != null ? ViewData["Nov_Cnt"].ToString() : ""),
                Dec_Cnt = (ViewData["Dec_Cnt"] != null ? ViewData["Dec_Cnt"].ToString() : ""),
                Jan_Cnt = (ViewData["Jan_Cnt"] != null ? ViewData["Jan_Cnt"].ToString() : ""),
                Feb_Cnt = (ViewData["Feb_Cnt"] != null ? ViewData["Feb_Cnt"].ToString() : ""),
                Mar_Cnt = (ViewData["Mar_Cnt"] != null ? ViewData["Mar_Cnt"].ToString() : ""),
                Apr_Cnt = (ViewData["Apr_Cnt"] != null ? ViewData["Apr_Cnt"].ToString() : ""),
                May_Cnt = (ViewData["May_Cnt"] != null ? ViewData["May_Cnt"].ToString() : ""),
                Jun_Cnt = (ViewData["Jun_Cnt"] != null ? ViewData["Jun_Cnt"].ToString() : "")
          });%>
</div>
<% // --------デモ・自登(担当者別)-------%>
<div id="3" style="<%=!(bool)ViewData["3_ListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("Ret3_EmployeeList", "CarSalesList",
          new { TargetYearCode = ViewData["SelectYearCode"].ToString(),
                DepartmentCode = ViewData["DepartmentCode"].ToString(),
                DepartmentName = ViewData["DepartmentName"].ToString(),
                Jul_Cnt = (ViewData["Jul_Cnt"] != null ? ViewData["Jul_Cnt"].ToString() : ""),
                Aug_Cnt = (ViewData["Aug_Cnt"] != null ? ViewData["Aug_Cnt"].ToString() : ""),
                Sep_Cnt = (ViewData["Sep_Cnt"] != null ? ViewData["Sep_Cnt"].ToString() : ""),
                Oct_Cnt = (ViewData["Oct_Cnt"] != null ? ViewData["Oct_Cnt"].ToString() : ""),
                Nov_Cnt = (ViewData["Nov_Cnt"] != null ? ViewData["Nov_Cnt"].ToString() : ""),
                Dec_Cnt = (ViewData["Dec_Cnt"] != null ? ViewData["Dec_Cnt"].ToString() : ""),
                Jan_Cnt = (ViewData["Jan_Cnt"] != null ? ViewData["Jan_Cnt"].ToString() : ""),
                Feb_Cnt = (ViewData["Feb_Cnt"] != null ? ViewData["Feb_Cnt"].ToString() : ""),
                Mar_Cnt = (ViewData["Mar_Cnt"] != null ? ViewData["Mar_Cnt"].ToString() : ""),
                Apr_Cnt = (ViewData["Apr_Cnt"] != null ? ViewData["Apr_Cnt"].ToString() : ""),
                May_Cnt = (ViewData["May_Cnt"] != null ? ViewData["May_Cnt"].ToString() : ""),
                Jun_Cnt = (ViewData["Jun_Cnt"] != null ? ViewData["Jun_Cnt"].ToString() : "")
          });%>
</div>
<% // --------依廃・他(担当者別)-------%>
<div id="4" style="<%=!(bool)ViewData["4_ListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("Ret4_EmployeeList", "CarSalesList",
          new { TargetYearCode = ViewData["SelectYearCode"].ToString(),
                DepartmentCode = ViewData["DepartmentCode"].ToString(),
                DepartmentName = ViewData["DepartmentName"].ToString(),
                Jul_Cnt = (ViewData["Jul_Cnt"] != null ? ViewData["Jul_Cnt"].ToString() : ""),
                Aug_Cnt = (ViewData["Aug_Cnt"] != null ? ViewData["Aug_Cnt"].ToString() : ""),
                Sep_Cnt = (ViewData["Sep_Cnt"] != null ? ViewData["Sep_Cnt"].ToString() : ""),
                Oct_Cnt = (ViewData["Oct_Cnt"] != null ? ViewData["Oct_Cnt"].ToString() : ""),
                Nov_Cnt = (ViewData["Nov_Cnt"] != null ? ViewData["Nov_Cnt"].ToString() : ""),
                Dec_Cnt = (ViewData["Dec_Cnt"] != null ? ViewData["Dec_Cnt"].ToString() : ""),
                Jan_Cnt = (ViewData["Jan_Cnt"] != null ? ViewData["Jan_Cnt"].ToString() : ""),
                Feb_Cnt = (ViewData["Feb_Cnt"] != null ? ViewData["Feb_Cnt"].ToString() : ""),
                Mar_Cnt = (ViewData["Mar_Cnt"] != null ? ViewData["Mar_Cnt"].ToString() : ""),
                Apr_Cnt = (ViewData["Apr_Cnt"] != null ? ViewData["Apr_Cnt"].ToString() : ""),
                May_Cnt = (ViewData["May_Cnt"] != null ? ViewData["May_Cnt"].ToString() : ""),
                Jun_Cnt = (ViewData["Jun_Cnt"] != null ? ViewData["Jun_Cnt"].ToString() : "")
          });%>
</div>
<br />
<%} %>
</asp:Content>