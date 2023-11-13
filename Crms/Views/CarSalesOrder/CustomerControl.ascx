<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
        <table class="input-form-slim">
            <tr>
                <th colspan="2" class="input-form-title">顧客情報</th>
            </tr>
            <tr>
                <th style="width: 100px">
                    顧客コード *
                </th>
                <td style="text-align:left">
                    <%if (Model.CustomerEnabled) { %>
                        <% //Add 2014/10/29 arc amii 住所再確認メッセージ対応 #3119 住所再確認メッセージをArrayに追加 %>
                        <%=Html.TextBox("CustomerCode", Model.CustomerCode, new { @class = "alphanumeric",style="width:100px",maxlength = "10", onblur = "GetMasterDetailFromCode('CustomerCode',new Array('CustomerName','CustomerAddress','AddressReconfirm','ReconfirmMessage'),'Customer')"})%>
                        <%Html.RenderPartial("SearchButtonControl", new string[] { "CustomerCode", "CustomerName", "'/Customer/CriteriaDialog'", "0" }); %>
                    <%}else{ %>
                        <%=Html.TextBox("CustomerCode", Model.CustomerCode, new { @class = "readonly alphanumeric", style = "width:300px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>顧客名</th>
                <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { @class = "readonly", Style = "width:300px", @readonly = "readonly" })%></td>
            </tr>
            <tr>
                <th>住所</th>
                <td><%=Html.TextBox("CustomerAddress", ViewData["CustomerAddress"], new { @class = "readonly", style = "width:300px", @readonly = "readonly" })%></td>
            </tr>
        </table>
<%=Html.Hidden("AddressReconfirm",ViewData["AddressReconfirm"]) %>
