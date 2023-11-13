<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<table class="input-form-slim">
    <%foreach(var ModData in Model.ModifiedReasonList) {
          string ModifyReason = ModData.ModifiedReason.Replace("\r\n", "&#13;&#10;");
          %>    
    <tr>
        <td><%=Html.TextBox("ModifiedTime", string.Format("{0:yyyy/MM/dd HH:mm:ss}",ModData.ModifiedTime), new { @class = "readonly", @readonly = "readonly", style = "width:130px" })%></td>
        <td><%=Html.TextBox("ModifiedEmployeeName",ModData.ModifiedEmployeeName, new { @class = "readonly", @readonly = "readonly", style = "width:100px" })%></td>
        <td>
            <div title=<%=ModifyReason%>>
            <%=Html.TextArea("ModifiedReason",ModData.ModifiedReason, new { @class = "readonly", @readonly = "readonly", style = "width:750px;height:16px",  })%>
            </div>
        </td>
    </tr>
    <%} %>
</table>

