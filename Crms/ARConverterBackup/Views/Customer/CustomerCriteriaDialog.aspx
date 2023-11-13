<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master"
    Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Customer>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    顧客検索ダイアログ
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("CriteriaDialog", "Customer", new { id = 0 }, FormMethod.Post)) { %>
    <%=Html.Hidden("id", "0") %>
    <a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
    <div id="search-form">
        <br />
        <table class="input-form">
            <tr>
                <th style="width: 100px">
                    顧客コード
                </th>
                <td>
                    <%=Html.TextBox("CustomerCode", ViewData["CustomerCode"], new { @class = "alphanumeric", maxlength = 10 })%>
                </td>
                <th style="width: 100px">
                    顧客ランク
                </th>
                <td>
                    <%=Html.DropDownList("CustomerRank", (IEnumerable<SelectListItem>)ViewData["CustomerRankList"])%>
                </td>
            </tr>
            <tr>
                <th>
                    顧客種別
                </th>
                <td>
                    <%=Html.DropDownList("CustomerKind", (IEnumerable<SelectListItem>)ViewData["CustomerKindList"])%>
                </td>
                <th>
                    顧客名
                </th>
                <td>
                    <%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { maxlength = 80 })%>
                </td>
            </tr>
            <tr>
                <th>
                    顧客区分
                </th>
                <td>
                    <%=Html.DropDownList("CustomerType", (IEnumerable<SelectListItem>)ViewData["CustomerTypeList"])%>
                </td>
                <th>
                    顧客名(カナ)
                </th>
                <td>
                    <%=Html.TextBox("CustomerNameKana",ViewData["CustomerNameKana"],new {maxlength=80}) %>
                </td>
            </tr>
            <tr>
                <th>
                    生年月日
                </th>
                <td colspan="3">
                    <%=Html.TextBox("BirthdayFrom", ViewData["BirthdayFrom"], new { @class = "alphanumeric", maxlength = 10 })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("BirthdayTo", ViewData["BirthdayTo"], new { @class = "alphanumeric", maxlength = 10 })%>
                </td>
            </tr>
            <tr>
                <th>
                    職業
                </th>
                <td>
                    <%=Html.DropDownList("Occupation", (IEnumerable<SelectListItem>)ViewData["OccupationList"])%>
                </td>
                <th>
                    性別
                </th>
                <td>
                    <%=Html.DropDownList("Sex", (IEnumerable<SelectListItem>)ViewData["SexList"])%>
                </td>
            </tr>
            <tr>
                <th>
                    電話番号
                </th>
                <td>
                    <%=Html.TextBox("TelNumber", ViewData["TelNumber"], new { @class = "alphanumeric", maxlength = 15 })%>
                </td>
                <th>
                    車の所有
                </th>
                <td>
                    <%=Html.DropDownList("CarOwner", (IEnumerable<SelectListItem>)ViewData["CarOwnerList"])%>
                </td>
            </tr>
            <tr>
                <th>
                </th>
                <td colspan="3">
                    <input type="button" value="検索" onclick="displaySearchList()" />
                </td>
            </tr>
        </table>
    </div>
    <%} %>
    <br />
    <%Html.RenderPartial("PagerControl", Model.PageProperty); %>
    <br />
    <br />
    <table class="list">
        <tr>
            <th style="width: 30px">
            </th>
            <th style="white-space:nowrap">
                顧客コード
            </th>
            <th style="white-space:nowrap">
                顧客ランク
            </th>
            <th style="white-space:nowrap">
                顧客種別
            </th>
            <th style="white-space:nowrap">
                顧客名
            </th>
            <th style="white-space:nowrap">
                顧客区分
            </th>
            <th style="white-space:nowrap">
                性別
            </th>
            <th style="white-space:nowrap">
                生年月日
            </th>
            <th style="white-space:nowrap">
                職業
            </th>
            <th style="white-space:nowrap">
                車の所有
            </th>
            <th style="white-space:nowrap">
                電話番号
            </th>
            <th style="white-space:nowrap">
                携帯電話番号
            </th>
        </tr>
        <%foreach (var customer in Model) { %>
        <tr>
            <td style="white-space:nowrap">
                <a href="javascript:selectedCriteriaDialog('<%=customer.CustomerCode %>','CustomerName')">
                    選択</a>
            </td>
            <td style="white-space:nowrap">
                <%=Html.Encode(customer.CustomerCode)%>
            </td>
            <td style="white-space:nowrap">
                <%if (customer.c_CustomerRank != null) {%><%=Html.Encode(customer.c_CustomerRank.Name)%><%} %>
            </td>
            <td style="white-space:nowrap">
                <%if (customer.c_CustomerKind != null) {%><%=Html.Encode(customer.c_CustomerKind.Name)%><%} %>
            </td>
            <td>
                <span id="<%="CustomerName_" + customer.CustomerCode%>">
                    <%=Html.Encode(customer.CustomerName)%></span>
            </td>
            <td style="white-space:nowrap">
                <%if (customer.c_CustomerType != null) {%><%=Html.Encode(customer.c_CustomerType.Name)%><%} %>
            </td>
            <td style="white-space:nowrap">
                <%if (customer.c_Sex != null) {%><%=Html.Encode(customer.c_Sex.Name)%><%} %>
            </td>
            <td style="white-space:nowrap">
                <%=Html.Encode(string.Format("{0:yyyy/MM/dd}", customer.Birthday))%>
            </td>
            <td style="white-space:nowrap">
                <%if (customer.c_Occupation != null) {%><%=Html.Encode(customer.c_Occupation.Name)%><%} %>
            </td>
            <td style="white-space:nowrap">
                <%if (customer.c_CarOwner != null) {%><%=Html.Encode(customer.c_CarOwner.Name)%><%} %>
            </td>
            <td style="white-space:nowrap">
                <%=Html.Encode(customer.TelNumber)%>
            </td>
            <td style="white-space:nowrap">
                <%=Html.Encode(customer.MobileNumber)%>
            </td>
        </tr>
        <%} %>
    </table>
    <br />
</asp:Content>
