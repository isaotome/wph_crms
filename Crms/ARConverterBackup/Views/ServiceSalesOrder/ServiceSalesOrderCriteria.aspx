<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.ServiceSalesHeader>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	サービス伝票検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria","ServiceSalesOrder",FormMethod.Post)){%>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultDelFlag", "0") %>
<%=Html.Hidden("DefaultDepartmentCode", ViewData["DefaultDepartmentCode"]) %>
<%=Html.Hidden("DefaultDepartmentName", ViewData["DefaultDepartmentName"]) %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">伝票番号</th>
        <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "alphanumeric", maxlength = "50" })%></td>
        <%// mod 2015/05/18 arc nakayama ルックアップ見直し対応　DelFlagが検索条件にあるダイアログに変更 %>
        <th style="width:100px" rowspan="2">部門</th>
        <td><%=Html.TextBox("DepartmentCode",ViewData["DepartmentCode"],new {@class="alphanumeric",maxlength="3",onblur="GetNameFromCodeDelflagNoCheck('DepartmentCode','DepartmentName','Department')"})%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog5')" /></td>   
    </tr>
    <tr>
        <th>見積日付</th>
        <td><%=Html.TextBox("QuoteDateFrom", ViewData["QuoteDateFrom"], new { @class="alphanumeric",size = "8",maxlength="10" })%> ～ <%=Html.TextBox("QuoteDateTo", ViewData["QuoteDateTo"], new { @class="alphanumeric",size = "8",maxlength="10" })%></td>
        <td style="height:20px"><span id="DepartmentName"><%=CommonUtils.DefaultNbsp(ViewData["DepartmentName"])%></span></td>
    </tr>
    <tr>
        <th>受注日付</th>
        <td><%=Html.TextBox("SalesOrderDateFrom", ViewData["SalesOrderDateFrom"], new { @class="alphanumeric", size = "8", maxlength="10" })%> ～ <%=Html.TextBox("SalesOrderDateTo", ViewData["SalesOrderDateTo"], new { @class="alphanumeric", size = "8", maxlength="10" })%></td>
        <th style="width:100px" rowspan="2">担当者</th>
        <td><%=Html.TextBox("EmployeeCode", ViewData["EmployeeCode"], new { @class = "alphanumeric", maxlength = "50",onblur="GetNameFromCodeDelflagNoCheck('EmployeeCode','EmployeeName','Employee')" })%>&nbsp;<img alt="担当者コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('EmployeeCode','EmployeeName','/Employee/CriteriaDialog2')" /></td>
    </tr>
    <tr>
        <th>ブランド名</th>
        <td><%=Html.TextBox("CarBrandName", ViewData["CarBrandName"], new { size = "10", maxlength = "50" })%></td>
        <td style="height:20px"><span id="EmployeeName"><%=CommonUtils.DefaultNbsp(ViewData["EmployeeName"])%></span></td>
    </tr>
    <tr>
        <th>登録番号</th>
        <td><%=Html.TextBox("PlateNumber",ViewData["PlateNumber"],new {size="8",maxlength="4",@class="alphanumeric"}) %></td>
        <th>顧客コード</th>
        <td><%=Html.TextBox("CustomerCode", ViewData["CustomerCode"], new { @class = "alphanumeric", size = "10", maxlength = "10" })%>&nbsp;<img alt="顧客コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerCode','CustomerName','/Customer/CriteriaDialog/?CustomerName='+encodeURIComponent(document.getElementById('CustomerName').value))" /></td>
    </tr>
    <tr>
        <th>電話番号(下4桁）</th>
        <td><%=Html.TextBox("TelNumber", ViewData["TelNumber"], new { @class="alphanumeric", size = "5", maxlength = "4" })%></td>
        <th>顧客名</th>
        <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { size = "20", maxlength = "80" })%></td>
    </tr>
    <tr>
        <th>伝票ステータス</th>
        <td><%=Html.DropDownList("ServiceOrderStatus",(IEnumerable<SelectListItem>)ViewData["ServiceOrderStatusList"]) %></td>
        <th>顧客名(カナ)</th>
        <td><%=Html.TextBox("CustomerNameKana", ViewData["CustomerNameKana"], new { size = "20", maxlength = "80" })%></td>
    </tr>
    <tr>
        <th>車台番号</th>
        <% // Del arc amii 2014/09/08 車台番号入力制限対応 #3085 下8桁入力の制限を削除 %>
        <td><%=Html.TextBox("Vin",ViewData["Vin"],new { size = 20, maxlength = 20, @class="alphanumeric"}) %></td>
        <th>請求先コード</th>
        <td><%=Html.TextBox("CustomerClaimCode", ViewData["CustomerClaimCode"], new { @class = "alphanumeric", size = "10", maxlength = "10" }) %>&nbsp;<img alt="請求先コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerClaimCode','CustomerClaimName','/CustomerClaim/CriteriaDialog/?CustomerClaimName='+encodeURIComponent(document.getElementById('CustomerClaimName').value))" /></td>
    </tr>
    <tr>
        <th rowspan="2">主作業</th>
        <td><%=Html.TextBox("ServiceWorkCode",ViewData["ServiceWorkCode"],new {@class="alphanumeric",size="10",maxlength="5",onchange="GetNameFromCode('ServiceWorkCode','ServiceWorkName','ServiceWork')"}) %>&nbsp;<img alt="主作業コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('ServiceWorkCode','ServiceWorkName','/ServiceWork/CriteriaDialog')" /></td>
        <th>請求先名</th>
        <td><%=Html.TextBox("CustomerClaimName", ViewData["CustomerClaimName"], new { size = "20", maxlength = "80" }) %></td>
    </tr>
    <tr>
        <td style="height:20px"><span id="ServiceWorkName"><%=CommonUtils.DefaultNbsp(ViewData["ServiceWorkName"]) %></span></td>
        <th rowspan="2">イベント</th>
        <% // Mod 2014/07/24 arc amii 既存バグ対応 手入力してフォーカスが外れたとき、コードに対応する名称をDBから取得するよう修正 %>
        <td><%=Html.TextBox("CampaignCode", ViewData["CampaignCode"], new { @class = "alphanumeric", size = "10", maxlength = "20",  onchange="GetNameFromCode('CampaignCode','CampaignName','Campaign')" })%>&nbsp;<img alt="イベントコード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CampaignCode','CampaignName','/Campaign/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <th>ステータス</th>
        <td><%=Html.RadioButton("DelFlag", "9", ViewData["DelFlag"])%>全て<%=Html.RadioButton("DelFlag", "0", ViewData["DelFlag"])%>有効<%=Html.RadioButton("DelFlag", "1", ViewData["DelFlag"])%>無効</td>
        <td style="height:20px"><span id="CampaignName"><%=CommonUtils.DefaultNbsp(ViewData["CampaignName"])%></span></td>
   </tr>
    <tr>
        <th></th>
        <td colspan="3">
            <%--// Mod 2015/06/30 arc ishii 検索時インジケーターを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="displaySearchList()" />--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()" />
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('SlipNumber','DepartmentCode','DepartmentName','QuoteDateFrom','QuoteDateTo','SalesOrderDateFrom','SalesOrderDateTo','EmployeeCode','EmployeeName','CarBrandName','PlateNumber','CustomerCode','CustomerName','TelNumber','ServiceOrderStatus','CustomerNameKana','Vin','CustomerClaimCode','CustomerClaimName','ServiceWorkCode','ServiceWorkName','CampaignCode','CampaignName','DelFlag'))" />
        </td>
    </tr>
</table>
</div>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケーターの表示するよう修正---%>
<div id ="UpdateMsg" style ="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/ServiceSalesOrder/Entry')" />
<%} %>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th colspan="2">伝票番号</th>
        <th>部門名</th>
        <th>ステータス</th>
        <th>受注日付</th>
        <th>納車日</th>
        <th>顧客名</th>
        <th>車種名</th>
        <th>登録番号</th>
        <th>年式</th>
        <th>走行距離</th>
        <th>車台番号</th>
        <th>主作業</th>
        <th>電話番号</th>
        <th>住所</th>
        <th></th>
    </tr>
    <%foreach (var a in Model) { %>
    <tr>
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/ServiceSalesOrder/Entry/?SlipNo=<%=a.SlipNumber%>&RevNo=<%=a.RevisionNumber %>')"><%=a.SlipNumber %></a></td>
        <td style="white-space:nowrap">
            <%if(a.SlipNumber!=null && a.SlipNumber.Substring(a.SlipNumber.Length-2,2).Equals("-1")){%>
                <span style="color:Red">赤伝</span>
            <%} else if (a.SlipNumber!=null && a.SlipNumber.Substring(a.SlipNumber.Length-2,2).Equals("-2")) {%>
                修正分
            <%} %>
        </td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(a.Department != null ? a.Department.DepartmentName : "") %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(a.c_ServiceOrderStatus != null ? a.c_ServiceOrderStatus.Name : "")%></td>
        <td style="white-space:nowrap"><%=string.Format("{0:yyyy/MM/dd}", a.SalesOrderDate) %></td>
        <td style="white-space:nowrap"><%=string.Format("{0:yyyy/MM/dd}", a.SalesDate) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(a.Customer == null ? "" : a.Customer.CustomerName)%></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(a.CarName)%></td>
        <td style="white-space:nowrap"><%=a.MorterViecleOfficialCode + " " + a.RegistrationNumberType + " " + a.RegistrationNumberKana + " " + a.RegistrationNumberPlate %></td>
        <td style="white-space:nowrap"><%=a.ManufacturingYear %></td>
        <td style="white-space:nowrap"><%=a.Mileage != null ? (a.Mileage + (a.c_MileageUnit != null ? "(" + a.c_MileageUnit.Name + ")" : "")) : ""%></td>
        <td style="white-space:nowrap"><%=a.Vin %></td>
        <td style="white-space:nowrap"><%foreach(var d in a.ServiceSalesLine){
                  if(!string.IsNullOrEmpty(d.ServiceType) && d.ServiceType.Equals("001") && !string.IsNullOrEmpty(d.ServiceWorkCode)){
                      %><%=CommonUtils.DefaultNbsp(d.ServiceWork!=null ? d.ServiceWork.Name : "")%><br /><%
                  }
              }
            %>
        </td>
        <td style="white-space:nowrap"><%=a.Customer!=null ? a.Customer.TelNumber : "" %></td>
        <td style="white-space:nowrap"><%=a.Customer!=null ? Html.Encode(a.Customer.Prefecture + a.Customer.City) : "" %></td>
        <td><input type="button" value="コピー" onclick="openModalAfterRefresh('/ServiceSalesOrder/Copy/?SlipNo=<%=a.SlipNumber %>&RevNo=<%=a.RevisionNumber %>')" style="width:50px" /></td>
    </tr>
    <%} %>
</table>
</asp:Content>
