<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarSalesHeader>>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両伝票検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria","CarSalesOrder",new {id = "0"},FormMethod.Post)){ %>
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
        <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { maxlength = 50 })%></td>
        <%// mod 2015/05/18 arc nakayama ルックアップ見直し対応　DelFlagが検索条件にあるダイアログに変更 %>
        <th style="width:100px" rowspan="2">部門</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { onblur = "GetNameFromCodeDelflagNoCheck('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog5')" /></td>
    </tr>
    <tr>
        <th>見積日付</th>
        <td><%=Html.TextBox("QuoteDateFrom", ViewData["QuoteDateFrom"], new { size = "8" })%> ～ <%=Html.TextBox("QuoteDateTo", ViewData["QuoteDateTo"], new { size = "8" })%></td>
        <td style="height:20px"><span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"]) %></span></td>
    </tr>
    <tr>
        <th>受注日付</th>
        <td><%=Html.TextBox("SalesOrderDateFrom", ViewData["SalesOrderDateFrom"], new { size = "8" })%> ～ <%=Html.TextBox("SalesOrderDateTo", ViewData["SalesOrderDateTo"], new { size = "8" })%></td>
        <th rowspan="2">担当者</th>
        <td><%=Html.TextBox("EmployeeCode", ViewData["EmployeeCode"], new { onblur = "GetNameFromCodeDelflagNoCheck('EmployeeCode','EmployeeName','Employee')" })%>&nbsp;<img alt="担当者コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('EmployeeCode','EmployeeName','/Employee/CriteriaDialog2')" /></td>
    </tr>
    <tr>
        <th>ブランド名</th>
        <td><%=Html.TextBox("CarBrandName")%></td>
        <td style="height:20px"><span id="EmployeeName"><%=Html.Encode(ViewData["EmployeeName"]) %></span></td>
   </tr>

    <%//Mod 2020/01/14 yano #3982 検索条件に使用者を追加%>
    <tr>
        <th>顧客コード</th>
        <td><%=Html.TextBox("CustomerCode",ViewData["CustomerCode"])%>&nbsp;<img alt="顧客コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerCode','CustomerName','/Customer/CriteriaDialog/?CustomerName='+encodeURIComponent(document.getElementById('CustomerName').value))" /></td>
        <th>使用者コード</th>
        <td><%=Html.TextBox("UserCode", ViewData["UserCode"], new { onblur = "GetNameFromCode('UserCode','UserName','Customer')" })%>&nbsp;<img alt="使用者コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('UserCode','UserName','/Customer/CriteriaDialog/?CustomerName='+encodeURIComponent(document.getElementById('UserName').value))" /></td>
    </tr>
    <tr>
        <th>顧客名</th>
        <td><%=Html.TextBox("CustomerName",ViewData["CustomerName"])%></td>
        <th>使用者名</th>
        <td><%=Html.TextBox("UserName",ViewData["UserName"])%></td>
    </tr>
    <tr>
        <th>電話番号(下4桁）</th>
        <td><%=Html.TextBox("TelNumber", ViewData["TelNumber"], new { size = "5", maxlength = "4" })%></td>
        <th>伝票ステータス</th>
        <td><%=Html.DropDownList("SalesOrderStatus",(IEnumerable<SelectListItem>)ViewData["SalesOrderStatusList"]) %></td>
    </tr>
    <tr>
        <th>車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"], new { maxlength = 20 })%></td>
        <th rowspan="2">イベント</th>
        <% // Mod 2014/07/24 arc amii 既存バグ対応 手入力してフォーカスが外れたとき、コードに対応する名称をDBから取得するよう修正 %>
        <td><%=Html.TextBox("CampaignCode", ViewData["CampaignCode"], new { onblur = "GetNameFromCode('CampaignCode','CampaignName','Campaign')" })%>&nbsp;<img alt="イベントコード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CampaignCode','CampaignName','/Campaign/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <th>ステータス</th>
        <td><%=Html.RadioButton("DelFlag", "9", ViewData["DelFlag"])%>全て<%=Html.RadioButton("DelFlag", "0", ViewData["DelFlag"])%>有効<%=Html.RadioButton("DelFlag", "1", ViewData["DelFlag"])%>無効</td>
        <td style="height:20px"><span id="CampaignName"><%=ViewData["CampaignName"] %></span></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
            <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="DisplayImage('UpdateMsg', '0'); displaySearchList()" />--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg', '0');displaySearchList()" />
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('SlipNumber','DepartmentCode','DepartmentName','QuoteDateFrom','QuoteDateTo','SalesOrderDateFrom','SalesOrderDateTo','EmployeeCode','EmployeeName','CarBrandName','TelNumber','CustomerCode','CustomerName','SalesOrderStatus','Vin','CampaignCode','CampaignName','DelFlag', 'UserCode', 'UserName'))" /><%//Mod 2020/01/14 yano #3982%>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/CarSalesOrder/Entry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th>ステータス</th>
        <th rowspan="2" colspan="2">伝票番号</th>
        <th>見積日付</th>
        <th rowspan="2">営業担当者</th>
        <th rowspan="2">販売車両</th>
        <th rowspan="2">顧客情報</th>
        <th>ローン</th>
        <th rowspan="2">販売報告</th>
        <th rowspan="2"></th><%// Add 2017/11/08 arc yano #3553 %>
    </tr>
    <tr>
        <th>HOT</th>
        <th>受注日付</th>
        <th>任意保険</th>
    </tr>
    <%for(int i = 0 ; i<Model.Count;i++){
          var item = Model[i];
          string backColor = "#ffffff";
          if (i % 2 == 1) {
              backColor = "#f5f5f5";
          }
    %>
    <tr>
        <td style="background-color:<%=backColor%>"><%=Html.Encode(item.c_SalesOrderStatus!=null ? item.c_SalesOrderStatus.Name : "")%></td>
        <td style="white-space:nowrap;background-color:<%=backColor%>"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/CarSalesOrder/Entry?SlipNo=<%=item.SlipNumber%>&RevNo=<%=item.RevisionNumber%>')"><%=Html.Encode(item.SlipNumber)%></a></td>
        <td style="white-space:nowrap;background-color:<%=backColor%>" rowspan="2"><%=Html.Encode(item.RevisionNumber) %></td>
        <td style="white-space:nowrap;background-color:<%=backColor%>"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.QuoteDate)) %></td>
        <td style="background-color:<%=backColor%>" rowspan="2"><%=Html.Encode(item.Employee!=null ? item.Employee.EmployeeName : "") %></td>
        <td style="background-color:<%=backColor%>" rowspan="2"><%=Html.Encode(item.CarName)%><br /><%=Html.Encode(item.CarGradeName) %><br /><%=Html.Encode(item.Vin) %></td>
        <td style="background-color:<%=backColor%>" rowspan="2"><%=item.Customer!=null ? Html.Encode(item.Customer.CustomerName) + "<br />" + Html.Encode(item.Customer.TelNumber) + "<br />" + Html.Encode(item.Customer.Prefecture+item.Customer.City+item.Customer.Address1+item.Customer.Address2) : "" %></td>
        <td style="background-color:<%=backColor%>"><%=Html.Encode(item.PaymentPlanType) %></td>
        <td style="background-color:<%=backColor%>" rowspan="2"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/CarSalesOrder/Report?SlipNo=<%=item.SlipNumber%>&RevNo=<%=item.RevisionNumber%>',1080,470)">入力</a></td>
        <td style="background-color:<%=backColor%>" rowspan="2"><input type="button" value="コピー" onclick="openModalAfterRefresh('/CarSalesOrder/Copy/?SlipNo=<%=item.SlipNumber %>&RevNo=<%=item.RevisionNumber %>')" style="width:50px" /></td><%// Add 2017/11/08 arc yano #3553 %>
    </tr>
    <tr>
        <td style="background-color:<%=backColor%>"><%=Html.Encode(item.SalesOrderStatus!=null && item.SalesOrderStatus.Equals("001") ? item.HotStatus : "")%></td>
        <td style="white-space:nowrap;background-color:<%=backColor%>">
            <%if(item.SlipNumber!=null && item.SlipNumber.Substring(item.SlipNumber.Length-2,2).Equals("-1")){%>
                <span style="color:Red">赤伝</span>
            <%} else if (item.SlipNumber!=null && item.SlipNumber.Substring(item.SlipNumber.Length-2,2).Equals("-2")) {%>
                修正分
            <%} %>
        </td>
        <td style="white-space:nowrap;background-color:<%=backColor%>"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.SalesOrderDate)) %></td>
        <td style="background-color:<%=backColor%>"><%=Html.Encode(item.c_VoluntaryInsuranceType!=null ? item.c_VoluntaryInsuranceType.Name : "") %></td>
    </tr>
    <%} %>
    </table>
</asp:Content>
