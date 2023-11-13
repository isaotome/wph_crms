<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	管理帳票出力検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<%
    //-------------------------------------------------------------------------- 
    //機能：管理帳票出力
    //作成日： ????/??/?? ??????
    //更新日： 2017/09/04 arc yano #3786 預かり車Excle出力対応
    //
    //-------------------------------------------------------------------------- 
%>
<%using(Html.BeginForm("Criteria","DocumentExport",FormMethod.Post)){ %>
<%=Html.Hidden("id",ViewData["id"] )%>
<%=Html.ValidationSummary() %><%///2017/09/04 arc yano #3786 %>
<table class="input-form">
    <tr>
        <th style="width:150px">帳票</th>
        <td style="width:300px"><%=Html.DropDownList("DocumentName", (IEnumerable<SelectListItem>)ViewData["DocumentList"], new { onchange = "DocumentSelected(document.forms[0].DocumentName.value,1)" })%></td>
    </tr>
    <tbody id="Term">
    <tr>
        <th>期間指定</th>
        <td><%=Html.TextBox("TermFrom",string.Format("{0:yyyy/MM/dd}",ViewData["TermFrom"]),new {@class="alphanumeric",size="10",maxlength="10"}) %>～<%=Html.TextBox("TermTo",string.Format("{0:yyyy/MM/dd}",ViewData["TermTo"]),new {@class="alphanumeric",size="10",maxlength="10"}) %></td>
    </tr>
    </tbody>
    <% // add 2016/05/20 nihsimura akira  %>
    <tbody id="TargetYearMonth">
    <tr>
        <th>対象年月</th>
        <td>
            <%=Html.DropDownList("TargetDateY", (IEnumerable<SelectListItem>)ViewData["TargetYearList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetDateM", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"])%>&nbsp;月&nbsp;
        </td>
    </tr>
    </tbody>
    <tbody id="SalesOrder">
    <tr>
        <th>車両伝票ステータス</th>
        <td><%=Html.DropDownList("SalesOrderStatus",(IEnumerable<SelectListItem>)ViewData["SalesOrderStatusList"]) %></td>
    </tr>
    </tbody>
    <tbody id="ServiceOrder">
    <tr>
        <th>サービス伝票ステータス</th>
        <td><%=Html.DropDownList("ServiceOrderStatus",(IEnumerable<SelectListItem>)ViewData["ServiceOrderStatusList"]) %></td>
    </tr>
    </tbody>
    <tbody id="Car">
    <tr>
        <th>車両ステータス</th>
        <td><%=Html.DropDownList("CarStatus",(IEnumerable<SelectListItem>)ViewData["CarStatusList"]) %></td>
    </tr>
    </tbody>
    <tbody id="NewUsed">
    <tr>
        <th>新中区分</th>
        <td><%=Html.DropDownList("NewUsedType",(IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"]) %></td>
    </tr>
    </tbody>
    <tbody id="Rank">
    <tr>
        <th>顧客ランク</th>
        <td><%=Html.DropDownList("CustomerRank",(IEnumerable<SelectListItem>)ViewData["CustomerRankList"]) %></td>
    </tr>
    </tbody>
    <tbody id="Type">
    <tr>
        <th>顧客区分</th>
        <td><%=Html.DropDownList("CustomerType",(IEnumerable<SelectListItem>)ViewData["CustomerTypeList"]) %></td>
    </tr>
    </tbody>
    <tbody id="CustomerKind">
    <tr>
        <th>顧客種別</th>
        <td><%=Html.DropDownList("CustomerKind", (IEnumerable<SelectListItem>)ViewData["CustomerKindList"]) %></td>
    </tr>
    </tbody>
    <tbody id="CustomerClaim">
    <tr>
        <th>請求先種別</th>
        <td><%=Html.DropDownList("CustomerClaimType",(IEnumerable<SelectListItem>)ViewData["CustomerClaimTypeList"]) %></td>
    </tr>
    </tbody>
    <tbody id="ReceiptPlanAccount">
    <tr>
        <th>入金予定種別</th>
        <td><%=Html.DropDownList("ReceiptPlanType",(IEnumerable<SelectListItem>)ViewData["ReceiptPlanTypeList"]) %></td>
    </tr>
    </tbody>
    <tbody id="PaymentPlanAccount">
    <tr>
        <th>支払予定種別</th>
        <td><%=Html.DropDownList("PaymentPlanType",(IEnumerable<SelectListItem>)ViewData["PaymentPlanTypeList"]) %></td>
    </tr>
    </tbody>
    <tbody id="Employee">
    <tr>
        <th>担当者</th>
        <td><%=Html.TextBox("EmployeeCode",ViewData["EmployeeCode"],new {@class="alphanumeric",size="20",maxlength="50",onchange="GetNameFromCode('EmployeeCode','EmployeeName','Employee')"}) %>&nbsp;<img alt="担当者検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('EmployeeCode','EmployeeName','/Employee/CriteriaDialog')" />&nbsp;<span id="EmployeeName"><%=CommonUtils.DefaultNbsp(ViewData["EmployeeName"]) %></span></td>
    </tr>
    </tbody>
    <tbody id="Department">
    <tr>
        <th>部門</th>
        <td><%=Html.TextBox("DepartmentCode",ViewData["DepartmentCode"],new {@class="alphanumeric",size="20",maxlength="3",onchange="GetNameFromCode('DepartmentCode','DepartmentName','Department')"}) %>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" />&nbsp;<span id="DepartmentName"><%=CommonUtils.DefaultNbsp(ViewData["DepartmentName"]) %></span></td>
    </tr>
    </tbody>
    <tbody id="Office">
    <tr>
        <th>事業所</th>
        <td><%=Html.TextBox("OfficeCode",ViewData["OfficeCode"],new {@class="alphanumeric",size="20",maxlength="3",onchange="GetNameFromCode('OfficeCode','OfficeName','Office')"}) %>&nbsp;<img alt="事業所検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('OfficeCode','OfficeName','/Office/CriteriaDialog')" />&nbsp;<span id="OfficeName"><%=CommonUtils.DefaultNbsp(ViewData["OfficeName"]) %></span></td>
    </tr>
    </tbody>
    <tbody id="Slip">
    <tr>
        <th>伝票番号</th>
        <td><%=Html.TextBox("SlipNumber",ViewData["SlipNumber"],new {@class="alphanumeric",size="20",maxlength="50"}) %></td>
    </tr>
    </tbody>
    <tbody id="Customer">
    <tr>
        <th>顧客名</th>
        <td><%=Html.TextBox("CustomerName",ViewData["CustomerName"],new {size="20",maxlength="40"}) %></td>
    </tr>
    </tbody>
    <tbody id="Supplier">
    <tr>
        <th>仕入先</th>
        <td><%=Html.TextBox("SupplierCode",ViewData["SupplierCode"],new {@class="alphanumeric",size="20",maxlength="10",onchange="GetNameFromCode('SupplierCode','SupplierName','Supplier')"}) %>&nbsp;<img alt="仕入先検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('SupplierCode','SupplierName','/Supplier/CriteriaDialog')" />&nbsp;<span id="SupplierName"><%=CommonUtils.DefaultNbsp(ViewData["SupplierName"]) %></span></td>
    </tr>
    </tbody>
    <tbody id="LastUpdate">
    <tr>
        <th>最終更新日</th>
        <td><%=Html.TextBox("LastUpdateDate",ViewData["LastUpdateDate"],new {@class="alphanumeric",size="10",maxlength="10"}) %> より<span id="LastUpdateFlag"></span></td>
    </tr>
    </tbody>
    <tbody id="Location">
    <tr>
        <th>ロケーション種別</th>
        <td><%=Html.DropDownList("LocationType",(IEnumerable<SelectListItem>)ViewData["LocationTypeList"]) %></td>
    </tr>
    </tbody>
    <tbody id="CustomerClaimCd">
    <tr>
        <th>請求先</th>
        <td><%=Html.TextBox("CustomerClaimCode",ViewData["CustomerClaimCode"],new {@class="alphanumeric",size="10",maxlength="10",onblur="GetNameFromCode('CustomerClaimCode','CustomerClaimName','CustomerClaim')"}) %>&nbsp;<img alt="請求先検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('CustomerClaimCode','CustomerClaimName','/CustomerClaim/CriteriaDialog')" /><span id="CustomerClaimName"><%=Html.Encode(ViewData["CustomerClaimName"]) %></span></td>
    </tr>
    </tbody>
    <tbody id="CommonDM">
    <tr>
        <th>担当拠点</th>
        <td>
            <div style="overflow-y:scroll;width:300px;height:150px">
                <table style="width:90%">
                    <%foreach(Department dep in (List<Department>)ViewData["DepartmentList"]){%>
                      <tr>
                        <td><%=Html.CheckBox(string.Format("dep[{0}]",dep.DepartmentCode),((Dictionary<string,bool>)ViewData["DepartmentCheckList"])[dep.DepartmentCode])%>&nbsp;<%=CommonUtils.DefaultNbsp(dep.DepartmentName) %></td>                        
                      </tr>
                    <%} %>
                </table>
            </div>
        </td>    
    </tr>
    <tr>
        <th>初年度登録(YYYY/MM)</th>
        <td><%=Html.TextBox("FirstRegistrationDateFrom",string.Format("{0:yyyy/MM/dd}",ViewData["FirstRegistrationDateFrom"])) %>～<%=Html.TextBox("FirstRegistrationDateTo",string.Format("{0:yyyy/MM/dd}",ViewData["FirstRegistrationDateTo"])) %></td>
    </tr>
    <tr>
        <th>次回車検日(車検有効期限)</th>
        <td><%=Html.TextBox("ExpireDateFrom",string.Format("{0:yyyy/MM/dd}",ViewData["ExpireDateFrom"])) %>～<%=Html.TextBox("ExpireDateTo",string.Format("{0:yyyy/MM/dd}",ViewData["ExpireDateTo"])) %></td>
    </tr>
    <tr>
        <th>次回点検日</th>
        <td><%=Html.TextBox("NextInspectionDateFrom",string.Format("{0:yyyy/MM/dd}",ViewData["NextInspectionDateFrom"])) %>～<%=Html.TextBox("NextInspectionDateTo",string.Format("{0:yyyy/MM/dd}",ViewData["NextInspectionDateTo"])) %></td>
    </tr>
    <tr>
        <th>登録年月日</th>
        <td><%=Html.TextBox("RegistrationDateFrom",string.Format("{0:yyyy/MM/dd}",ViewData["RegistrationDateFrom"]))%>～<%=Html.TextBox("RegistrationDateTo",string.Format("{0:yyyy/MM/dd}",ViewData["RegistrationDateTo"])) %></td>
    </tr>
    <tr>
        <th>納車年月日</th>
        <td><%=Html.TextBox("SalesDateFrom",string.Format("{0:yyyy/MM/dd}",ViewData["SalesDateFrom"])) %>～<%=Html.TextBox("SalesDateTo",string.Format("{0:yyyy/MM/dd}",ViewData["SalesDateTo"])) %></td>
    </tr>
    <tr>
        <th>受注年月日</th>
        <td><%=Html.TextBox("SalesOrderDateFrom",string.Format("{0:yyyy/MM/dd}",ViewData["SalesOrderDateFrom"])) %>～<%=Html.TextBox("SalesOrderDateTo",string.Format("{0:yyyy/MM/dd}",ViewData["SalesOrderDateTo"])) %></td>
    </tr>
    <tr>
        <th>DM可否</th>
        <td><%=Html.RadioButton("DmFlag","001",ViewData["DmFlag"]!=null && ViewData["DmFlag"].Equals("001")) %>可　
            <%=Html.RadioButton("DmFlag","002",ViewData["DmFlag"]!=null && ViewData["DmFlag"].Equals("002")) %>不可
            <%=Html.RadioButton("DmFlag","",ViewData["DmFlag"] == null || ViewData["DmFlag"].Equals("")) %>全て
        </td>
    </tr>
    </tbody>
    <tbody id="CarDM">
    <tr>
        <th>初回来店日</th>
        <td><%=Html.TextBox("FirstReceiptionDateFrom",string.Format("{0:yyyy/MM/dd}",ViewData["FirstReceiptionDateFrom"])) %>～<%=Html.TextBox("FirstReceiptionDateTo",string.Format("{0:yyyy/MM/dd}",ViewData["FirstReceiptionDateTo"])) %></td>
    </tr>
    <tr>
        <th>前回来店日</th>
        <td><%=Html.TextBox("LastReceiptionDateFrom",string.Format("{0:yyyy/MM/dd}",ViewData["LastReceiptionDateFrom"]))%>～<%=Html.TextBox("LastReceiptionDateTo",string.Format("{0:yyyy/MM/dd}",ViewData["LastReceiptionDateTo"])) %></td>
    </tr>
    <tr>
        <th>見込み客</th>
        <td><%=Html.CheckBox("InterestedCustomer", ViewData["InterestedCustomer"])%></td>
    </tr>
    <tr>
        <th>商談対象車両</th>
        <td>
            <div style="overflow-y:scroll;width:300px;height:150px">
                <table style="width:90%">
                    <%foreach(Car car in (List<Car>)ViewData["CarList"]){%>
                      <tr>
                        <td><%=Html.CheckBox(string.Format("car[{0}]",car.CarCode),((Dictionary<string,bool>)ViewData["CarCheckList"])[car.CarCode])%>&nbsp;<%=CommonUtils.DefaultNbsp((car.Brand!=null ? car.Brand.CarBrandName : "") + " " + car.CarName) %></td>                        
                      </tr>
                    <%} %>
                </table>
            </div>
        </td>    
    </tr>
    </tbody>
    <tbody id="ServiceDM">
    <tr>
        <th>ナンバープレート</th>
        <td><%=Html.DropDownList("MorterViecleOfficialCode", (IEnumerable<SelectListItem>)ViewData["MorterViecleOfficialCodeList"])%></td>
    </tr>
    <tr>
        <th>ブランド</th>
        <% // Mod 2014/07/23 arc amii 既存バグ対応 コードを手入力した時、名称が表示されないのを修正 %>
        <td><%=Html.TextBox("CarBrandCode",ViewData["CarBrandCode"],new {@class="alphanumeric",size="10",maxlength="30",onchange="GetNameFromCode('CarBrandCode','CarBrandName','Brand')"}) %>&nbsp;<img alt="ブランド検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarBrandCode','CarBrandName','/Brand/CriteriaDialog')" />&nbsp;<span id="CarBrandName"><%=CommonUtils.DefaultNbsp(ViewData["CarBrandName"]) %></span></td>
    </tr>
    <tr>
        <th>車種</th>
        <% // Mod 2014/07/23 arc amii 既存バグ対応 コードを手入力した時、名称が表示されないのを修正 %>
        <td><%=Html.TextBox("CarCode",ViewData["CarCode"],new {@class="alphanumeric",size="10",maxlength="30",onchange="GetNameFromCode('CarCode','CarName','Car')"}) %>&nbsp;<img alt="車種検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarCode','CarName','/Car/CriteriaDialog')" />&nbsp;<span id="CarName"><%=CommonUtils.DefaultNbsp(ViewData["CarName"]) %></span></td>
    </tr>
    <tr>
        <th>グレード</th>
        <% // Mod 2014/07/23 arc amii 既存バグ対応 コードを手入力した時、名称が表示されないのを修正 %>
        <td><%=Html.TextBox("CarGradeCode",ViewData["CarGradeCode"],new {@class="alphanumeric",size="10",maxlength="30",onchange="GetNameFromCode('CarGradeCode','CarGradeName','CarGrade')"}) %>&nbsp;<img alt="グレード検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarGradeCode','CarGradeName','/CarGrade/CriteriaDialog')" />&nbsp;<span id="CarGradeName"><%=CommonUtils.DefaultNbsp(ViewData["CarGradeName"]) %></span></td>
    </tr>
    </tbody>

    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="formSubmit();" /></td>
    </tr>
</table>
<br />
<br />
<%} %>
<%using(Html.BeginForm("Download","DocumentExport",FormMethod.Post)){ %>
<%=Html.Hidden("id",ViewData["id"]) %> <%//Add 2017/09/04 arc yano #3786 %>
<%=Html.Hidden("TargetName",ViewData["TargetName"]) %>
<%=Html.Hidden("TermFrom",ViewData["TermFrom"]) %>
<%=Html.Hidden("TermTo",ViewData["TermTo"]) %>
<%=Html.Hidden("SalesOrderStatus",ViewData["SalesOrderStatus"]) %>
<%=Html.Hidden("ServiceOrderStatus",ViewData["ServiceOrderStatus"]) %>
<%=Html.Hidden("CarStatus",ViewData["CarStatus"]) %>
<%=Html.Hidden("NewUsedType",ViewData["NewUsedType"])%>
<%=Html.Hidden("CustomerRank",ViewData["CustomerRank"])%>
<%=Html.Hidden("CustomerType",ViewData["CustomerType"])%>
<%=Html.Hidden("EmployeeCode",ViewData["EmployeeCode"])%>
<%=Html.Hidden("DepartmentCode",ViewData["DepartmentCode"])%>
<%=Html.Hidden("LastUpdateDate",ViewData["LastUpdateDate"])%>
<%=Html.Hidden("LocationType",ViewData["LocationType"])%>
<%=Html.Hidden("CustomerClaimType",ViewData["CustomerClaimType"]) %>
<%=Html.Hidden("ReceiptPlanType",ViewData["ReceiptPlanType"]) %>
<%=Html.Hidden("PaymentPlanType",ViewData["PaymentPlanType"]) %>
<%=Html.Hidden("OfficeCode",ViewData["OfficeCode"]) %>
<%=Html.Hidden("CustomerName",ViewData["CustomerName"]) %>
<%=Html.Hidden("SupplierCode",ViewData["SupplierCode"]) %>
<%=Html.Hidden("SlipNumber",ViewData["SlipNumber"]) %>
<%=Html.Hidden("CustomerClaimCode",ViewData["CustomerClaimCode"]) %>
<%=Html.Hidden("FirstReceiptionDateFrom",ViewData["FirstReceiptionDateFrom"]) %>
<%=Html.Hidden("FirstReceiptionDateTo",ViewData["FirstReceiptionDateTo"]) %>
<%=Html.Hidden("LastReceiptionDateFrom",ViewData["LastReceiptionDateFrom"]) %>
<%=Html.Hidden("LastReceiptionDateTo",ViewData["LastReceiptionDateTo"]) %>
<%=Html.Hidden("FirstRegistrationDateFrom",ViewData["FirstRegistrationDateFrom"]) %>
<%=Html.Hidden("FirstRegistrationDateTo",ViewData["FirstRegistrationDateTo"]) %>
<%=Html.Hidden("ExpireDateFrom",ViewData["ExpireDateFrom"]) %>
<%=Html.Hidden("ExpireDateTo",ViewData["ExpireDateTo"]) %>
<%=Html.Hidden("NextInspectionDateFrom",ViewData["NextInspectionDateFrom"]) %>
<%=Html.Hidden("NextInspectionDateTo",ViewData["NextInspectionDateTo"]) %>
<%=Html.Hidden("RegistrationDateFrom",ViewData["RegistrationDateFrom"]) %>
<%=Html.Hidden("RegistrationDateTo",ViewData["RegistrationDateTo"]) %>
<%=Html.Hidden("SalesDateFrom",ViewData["SalesDateFrom"]) %>
<%=Html.Hidden("SalesDateTo",ViewData["SalesDateTo"]) %>
<%=Html.Hidden("SalesOrderDateFrom",ViewData["SalesOrderDateFrom"]) %>
<%=Html.Hidden("SalesOrderDateTo",ViewData["SalesOrderDateTo"]) %>
<%=Html.Hidden("DmFlag",ViewData["DmFlag"]) %>
<%=Html.Hidden("InterestedCustomer",ViewData["InterestedCustomer"]) %>
<%foreach(Department dep in (List<Department>)ViewData["DepartmentList"]){%>
<%=Html.Hidden(string.Format("dep[{0}]",dep.DepartmentCode),((Dictionary<string,bool>)ViewData["DepartmentCheckList"])[dep.DepartmentCode])%>
<%} %>
<%foreach(Car car in (List<Car>)ViewData["CarList"]){%>
<%=Html.Hidden(string.Format("car[{0}]",car.CarCode),((Dictionary<string,bool>)ViewData["CarCheckList"])[car.CarCode])%>
<%} %>
<%=Html.Hidden("MorterViecleOfficialCode", ViewData["MorterViecleOfficialCode"])%>
<%=Html.Hidden("CarBrandCode",ViewData["CarBrandCode"]) %>
<%=Html.Hidden("CarCode",ViewData["CarCode"]) %>
<%=Html.Hidden("CarGradeCode",ViewData["CarGradeCode"]) %>
<%=Html.Hidden("TargetDateY",ViewData["TargetDateY"]) %>
<%=Html.Hidden("TargetDateM",ViewData["TargetDateM"]) %>
<%=Html.Hidden("TemplateUse",ViewData["TemplateUse"]) %>    <%//Add 2017/09/04 arc yano #3786 %>
<fieldset class="frameborder" style="width:450px;">
    <legend style="color:#069;">検索結果</legend>
    <div style="height:100px;text-align:center">
    <br />
    <br />
    <%if (ViewData["ItemCount"] == null){ %>
        検索条件を指定してください。
    <%}else if (Int32.Parse(ViewData["ItemCount"].ToString()) > 0){ %>
        対象データは <span style="font-weight:bold;font-size:larger"><%=ViewData["ItemCount"]%>件</span> 存在します。
        <br />
        <br />
        <input type="submit" value="ダウンロード" />
        <br />
    <%}else{ %>
        対象データが存在しません。
    <%} %>
    </div>
</fieldset>
<%} %>
<script type="text/javascript">
    DocumentSelected(document.forms[0].DocumentName.value,0);
</script>
</asp:Content>
