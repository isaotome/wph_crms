<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.V_CarAppraisal>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    車両査定検索
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Criteria", "CarAppraisal", new { id = 0 }, FormMethod.Post)) { %>
    <%=Html.Hidden("id", "0") %>
    <%=Html.Hidden("DefaultPurchaseStatus", ViewData["DefaultPurchaseStatus"]) %>
    <a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
    <div id="search-form">
        <br />
        <table class="input-form">
            <tr>
                <th style="width: 100px">
                    車台番号
                </th>
                <td>
                    <%=Html.TextBox("Vin", ViewData["Vin"], new { maxlength = 20 })%>
                </td>
                <th style="width: 100px">
                    伝票番号
                </th>
                <td>
                    <%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "alphanumeric", maxlength = 50 })%>
                </td>
            </tr>
            <tr>
                <th>
                    データ作成日
                </th>
                <td colspan="3">
                    <%=Html.TextBox("CreateDateFrom", ViewData["CreateDateFrom"], new { @class = "alphanumeric", maxlength = 10 })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("CreateDateTo", ViewData["CreateDateTo"], new { @class = "alphanumeric", maxlength = 10 })%>
                </td>
            </tr>
            <tr>
                <th>
                    仕入ステータス
                </th>
                <td colspan="3">
                    <%=Html.DropDownList("PurchaseStatus", (IEnumerable<SelectListItem>)ViewData["PurchaseStatusList"])%>
                </td>
            </tr>
            <tr>
                <th>
                </th>
                <td colspan="3">
                    <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
                    <%--<input type="button" value="検索" onclick="displaySearchList()" />--%>
                    <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()" />
                    <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('Vin','SlipNumber','CreateDateFrom','CreateDateTo','PurchaseStatus'))" />
                </td>
            </tr>
        </table>
    </div>
    <%} %>
    <br />
    <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
    <div id="UpdateMsg" style="display:none">
        <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
    </div>
    <br />
    <input type="button" value="新規作成" onclick="openModalAfterRefresh('/CarAppraisal/Entry')" />
    <br />
    <br />
    <%Html.RenderPartial("PagerControl", Model.PageProperty); %>
    <br />
    <br />
    <table class="list">
        <tr>
            <th style="width: 30px">
            </th>
            <th>
                受注日
            </th>
            <th>
                伝票番号
            </th>
            <th>
                作成日
            </th>
            <th>
                査定担当者
            </th>
            <th>
                受注担当者
            </th>
            <th>
                車台番号
            </th>
            <th>
                顧客名
            </th>
        </tr>
        <%foreach (var v_CarAppraisal in Model) { %>
        <tr>
            <td>
                <a href="javascript:void(0);" onclick="openModalAfterRefresh('/CarAppraisal/Entry/' + '<%=v_CarAppraisal.CarAppraisalId.ToString() %>');return false;">
                    詳細</a>
            </td>
            <td style="white-space:nowrap">
                <%=Html.Encode(string.Format("{0:yyyy/MM/dd}", v_CarAppraisal.SalesOrderDate))%>
            </td>
            <td style="white-space:nowrap">
                <%=Html.Encode(v_CarAppraisal.SlipNumber)%>
            </td>
            <td style="white-space:nowrap">
                <%=Html.Encode(string.Format("{0:yyyy/MM/dd}", v_CarAppraisal.CreateDate))%>
            </td>
            <td>
                <%=Html.Encode(v_CarAppraisal.AppraisalEmployeeName)%>
            </td>
            <td>
                <%=Html.Encode(v_CarAppraisal.OrderEmployeeName)%>
            </td>
            <td style="white-space:nowrap">
                <%=Html.Encode(v_CarAppraisal.Vin)%>
            </td>
            <td>
                <%=Html.Encode(v_CarAppraisal.CustomerName)%>
            </td>
        </tr>
        <%} %>
    </table>
    <br />
</asp:Content>
