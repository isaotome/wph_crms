<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Customer>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	顧客マスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "Customer", new { id = 0 }, FormMethod.Post)) { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultDelFlag", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">顧客コード</th>
        <td>
            <%=Html.TextBox("CustomerCode", ViewData["CustomerCode"], new { @class = "alphanumeric", maxlength = 10 })%>
        </td>
    </tr>
    <tr>
        <th>顧客ランク</th>
        <td>
            <%=Html.DropDownList("CustomerRank", (IEnumerable<SelectListItem>)ViewData["CustomerRankList"])%>
        </td>
    </tr>
    <tr>
        <th>顧客種別</th>
        <td><%=Html.DropDownList("CustomerKind", (IEnumerable<SelectListItem>)ViewData["CustomerKindList"])%></td>
    </tr>
    <tr>
        <th>顧客名</th>
        <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { size = 50, maxlength = 40 })%></td>
    </tr>
    <tr>
        <th>顧客名(カナ)</th>
        <td><%=Html.TextBox("CustomerNameKana",ViewData["CustomerNameKana"],new {size=50,maxlength=40}) %></td>
    </tr>
    <tr>
        <th>顧客区分</th>
        <td><%=Html.DropDownList("CustomerType", (IEnumerable<SelectListItem>)ViewData["CustomerTypeList"])%></td>
    </tr>
    <tr>
        <th>性別</th>
        <td><%=Html.DropDownList("Sex", (IEnumerable<SelectListItem>)ViewData["SexList"])%></td>
    </tr>
    <tr>
        <th>生年月日(YYYY/MM/DD)</th>
        <td><%=Html.TextBox("BirthdayFrom", ViewData["BirthdayFrom"], new { @class = "alphanumeric", maxlength = 10 })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("BirthdayTo", ViewData["BirthdayTo"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>職業</th>
        <td><%=Html.DropDownList("Occupation", (IEnumerable<SelectListItem>)ViewData["OccupationList"])%></td>
    </tr>
    <tr>
        <th>車の所有</th>
        <td><%=Html.DropDownList("CarOwner", (IEnumerable<SelectListItem>)ViewData["CarOwnerList"])%></td>
    </tr>
    <tr>
        <th>電話番号</th>
        <td><%=Html.TextBox("TelNumber", ViewData["TelNumber"], new { @class = "alphanumeric", maxlength = 15 })%></td>
    </tr>
    <tr>
        <th>ステータス</th>
        <td><%=Html.RadioButton("DelFlag", "9", ViewData["DelFlag"])%>全て<%=Html.RadioButton("DelFlag", "0", ViewData["DelFlag"])%>有効<%=Html.RadioButton("DelFlag", "1", ViewData["DelFlag"])%>無効</td>
    </tr>
    <tr>
        <th></th>
        <td><%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="displaySearchList()"/>--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('CustomerCode','CustomerRank','CustomerKind','CustomerName','CustomerNameKana','CustomerType','Sex','BirthdayFrom','BirthdayTo','Occupation','CarOwner','TelNumber','DelFlag'))" />
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/Customer/IntegrateEntry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>顧客コード</th>
        <th>顧客ランク</th>
        <th>顧客種別</th>
        <th>顧客名</th>
        <th>顧客区分</th>
        <th>性別</th>
        <th>生年月日</th>
        <th>職業</th>
        <th>車の所有</th>
        <th>電話番号</th>
        <th>携帯電話番号</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var customer in Model)
    
      { %>
    <tr>
        <td style="white-space:nowrap"><a href="javascript:openModalAfterRefresh('/Customer/IntegrateEntry/' + '<%=customer.CustomerCode%>')">詳細</a></td>
        <td style="white-space:nowrap"><%=Html.Encode(customer.CustomerCode)%></td>
        <td style="white-space:nowrap"><%if (customer.c_CustomerRank != null) {%><%=Html.Encode(customer.c_CustomerRank.Name)%><%} %></td>
        <td style="white-space:nowrap"><%if (customer.c_CustomerKind != null) {%><%=Html.Encode(customer.c_CustomerKind.Name)%><%} %></td>
        <td style="white-space:nowrap"><%=Html.Encode(customer.CustomerName)%></td>
        <td style="white-space:nowrap"><%if (customer.c_CustomerType != null) {%><%=Html.Encode(customer.c_CustomerType.Name)%><%} %></td>
        <td style="white-space:nowrap"><%if (customer.c_Sex != null) {%><%=Html.Encode(customer.c_Sex.Name)%><%} %></td>
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", customer.Birthday))%></td>
        <td style="white-space:nowrap"><%if (customer.c_Occupation != null) {%><%=Html.Encode(customer.c_Occupation.Name)%><%} %></td>
        <td style="white-space:nowrap"><%if (customer.c_CarOwner != null) {%><%=Html.Encode(customer.c_CarOwner.Name)%><%} %></td>
        <td style="white-space:nowrap"><%=Html.Encode(customer.TelNumber)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(customer.MobileNumber)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(customer.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
