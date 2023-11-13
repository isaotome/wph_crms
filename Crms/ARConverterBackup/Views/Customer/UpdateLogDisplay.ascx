<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Data.Linq.EntitySet<CrmsDao.CustomerUpdateLog>>" %>
<table class="input-form">
    <tr>
        <th colspan="6" class="input-form-title">担当者推移</th>
    </tr>
    <tr>
        <th nowrap style="width:120px">日時</th>
        <th nowrap style="width:100px">変更箇所</th>
        <th nowrap style="width:100px">更新者</th>
        <th nowrap style="width:150px">変更前</th>
        <th nowrap style="width:15px"></th>
        <th nowrap style="width:150px">変更後</th>
    </tr>
</table>
<div style="width:695px;height:600px;overflow-y:scroll">
<table class="input-form">
    <%
        var Target = Model.OrderBy(x => x.UpdateDate).ToList();
        for (int i = 0; i < Target.Count(); i++) {
            var item = Target[i];
            string prefix = string.Format("updateLog[{0}].",i);
            //2014/05/29 vs2012対応 arc yano 各コントロールにid追加
            string idprefix = string.Format("updateLog[{0}]_", i);
    %>
    <tr>
        <td nowrap style="width:120px"><%=Html.Encode(string.Format("{0:yyyy/MM/dd HH:mm:ss}", item.UpdateDate))%></td>
        <td nowrap style="width:100px"><%=Html.Encode(item.UpdateColumn)%></td>
        <td nowrap style="width:100px"><%=Html.Encode(item.Employee != null ? item.Employee.EmployeeName : "")%></td>
        <td nowrap style="width:150px"><%=Html.Encode(item.UpdateValueFrom)%></td>
        <td nowrap style="width:15px">→</td>
        <td nowrap style="width:150px"><%=Html.Encode(item.UpdateValueTo)%></td>
    </tr>
    <%=Html.Hidden(prefix + "CustomerCode", item.CustomerCode, new { id = idprefix + "CustomerCode"})%>
    <%=Html.Hidden(prefix + "CustomerUpdateLogId", item.CustomerUpdateLogId, new { id = idprefix + "CustomerUpdateLogId"})%>
    <%=Html.Hidden(prefix + "UpdateDate", item.UpdateDate, new { id = idprefix + "UpdateDate"})%>
    <%=Html.Hidden(prefix + "UpdateColumn", item.UpdateColumn, new { id = idprefix + "UpdateColumn"})%>
    <%=Html.Hidden(prefix + "UpdateEmployeeCode", item.UpdateEmployeeCode, new { id = idprefix + "UpdateEmployeeCode"})%>
    <%=Html.Hidden(prefix + "UpdateValueFrom", item.UpdateValueFrom, new { id = idprefix + "UpdateValueFrom"})%>
    <%=Html.Hidden(prefix + "UpdateValueTo", item.UpdateValueTo, new { id = idprefix + "UpdateValueTo"})%>
    <%} %>
</table>
</div>