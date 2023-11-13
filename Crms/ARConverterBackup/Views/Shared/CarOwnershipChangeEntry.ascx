<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.SalesCar>" %>

    <table class="input-form-slim">
        <tr>
            <th class="input-form-title" colspan="4">
                変更情報
            </th>
        </tr>
        <tr>
            <th style="width:100px">変更日</th>
            <td><%=Html.TextBox("OwnershipChangeDate", string.Format("{0:yyyy/MM/dd}", Model.OwnershipChangeDate), new { @class = "alphanumeric",style="width:80px" }) %></td>
            <th style="width:100px">変更区分</th>
            <td><%=Html.DropDownList("OwnershipChangeType", (IEnumerable<SelectListItem>)ViewData["OwnershipChangeTypeList"])%></td>
        </tr>
        <tr>
            <th>変更理由</th>
            <td colspan="3"><%=Html.TextBox("OwnershipChangeMemo", Model.OwnershipChangeMemo, new { style = "width:500px" }) %></td>
        </tr>
    </table>
