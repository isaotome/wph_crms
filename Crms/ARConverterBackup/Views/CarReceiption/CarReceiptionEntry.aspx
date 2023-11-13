<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master"
    Inherits="System.Web.Mvc.ViewPage<CrmsDao.CustomerReceiption>" %>

<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    店舗受付入力
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "CarReceiption", FormMethod.Post)) { %>
    <table class="command">
        <tr>
            <td onclick="formClose()">
                <img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる
            </td>
            <td onclick="document.forms[0].action.value = 'save';formSubmit();">
                <img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存
            </td>
            <td onclick="document.forms[0].action.value = 'saveQuote';formSubmit()">
                <img src="/Content/Images/build.png" alt="見積作成" class="command_btn" />&nbsp;見積作成
            </td>
        </tr>
    </table>
    <br />
    <%=Html.Hidden("close", ViewData["close"]) %>
    <%=Html.Hidden("action", "") %>
    <%=Html.Hidden("url",ViewData["url"]) %>
        <%=Html.ValidationSummary()%>
        <br />
        <table class="input-form-slim">
            <tr>
                <th colspan="4" class="input-form-title">
                    顧客情報
                </th>
            </tr>
            <tr>
                <th style="width: 150px">
                    顧客コード *
                </th>
                <td style="width: 200px">
                     <% //Mod 2015/09/14 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動(類似対応) 詳細情報取得中に非活性にする項目を引数として追加%> 
                    <%=Html.TextBox("CustomerCode", Model.CustomerCode, new { @class = "alphanumeric", maxlength = 10, style="width:100px", onblur = "GetMasterDetailFromCode('CustomerCode',new Array('CustomerRankName','CustomerName','CustomerNameKana','Prefecture','City','Address1','Address2'),'Customer', null, new Array('MasterForCustomer', 'HistoryForCustomer'))" })%> 
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "CustomerCode", "CustomerName", "'/Customer/CriteriaDialog'", "0", "GetMasterDetailFromCode('CustomerCode',new Array('CustomerRankName','CustomerName','CustomerNameKana','Prefecture','City','Address1','Address2'),'Customer', null, new Array('MasterForCustomer', 'HistoryForCustomer') )" });%>
                    <%
                        //Mod 2015/09/14 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動(類似対応) 
                        //①マスタボタンをクリック時に顧客コードが入っていない場合は、顧客マスタの新規登録画面を表示する
                        //②id追加
                    %>
                    <button type="button" id="MasterForCustomer" style="width:50px;height:20px" onclick="$('#CustomerCode').val()!='' ? openModalDialog('/Customer/IntegrateEntry/'+$('#CustomerCode').val()) : openModalDialog('/Customer/IntegrateEntry/'); return false; GetMasterDetailFromCode('CustomerCode',new Array('CustomerRankName','CustomerName','CustomerNameKana','Prefecture','City','Address1','Address2'),'Customer', null, new Array('MasterForCustomer', 'HisotryForCustomer'))" >マスタ</button>
                </td>
                <th style="width: 150px">
                    顧客ランク
                </th>
                <td style="width: 200px">
                    <%=Html.TextBox("CustomerRankName", Model.Customer != null && Model.Customer.c_CustomerRank!=null ? Model.Customer.c_CustomerRank.Name : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
            </tr>
            <tr>
                <th>
                    顧客名
                </th>
                <td>
                    <%=Html.TextBox("CustomerName", Model.Customer != null ? Model.Customer.CustomerName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
                <th>
                    顧客名(カナ)
                </th>
                <td>
                    <%=Html.TextBox("CustomerNameKana", Model.Customer != null ? Model.Customer.CustomerNameKana : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
            </tr>
            <tr>
                <th>
                    都道府県
                </th>
                <td>
                    <%=Html.TextBox("Prefecture", Model.Customer != null ? Model.Customer.Prefecture : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
                <th>
                    市区町村
                </th>
                <td>
                    <%=Html.TextBox("City", Model.Customer != null ? Model.Customer.City : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
            </tr>
            <tr>
                <th>
                    住所１
                </th>
                <td colspan="3">
                    <% // Mod 2014/07/04 arc amii chrome対応 style指定に「box-sizing: border-box;」を追加し、欄からはみ出るのを修正 %>
                    <%=Html.TextBox("Address1", Model.Customer != null ? Model.Customer.Address1 : "", new { @class = "readonly", style = "width:100%; box-sizing: border-box;", @readonly = "readonly" })%>
                </td>
            </tr>
            <tr>
                <th>
                    住所２
                </th>
                <td colspan="3">
                    <% // Mod 2014/07/04 arc amii chrome対応 style指定に「box-sizing: border-box;」を追加し、欄からはみ出るのを修正 %>
                    <%=Html.TextBox("Address2", Model.Customer != null ? Model.Customer.Address2 : "", new { @class = "readonly", style = "width:100%; box-sizing: border-box;", @readonly = "readonly" })%>
                </td>
            </tr>
        </table>
        <br />
        <table class="input-form-slim">
            <tr>
                <th colspan="4" class="input-form-title">
                    受付内容
                </th>
            </tr>
            <tr>
                <th style="width: 150px">
                    受付日 *
                </th>
                <td style="width: 200px">
                    <%=Html.TextBox("ReceiptionDate", string.Format("{0:yyyy/MM/dd}", Model.ReceiptionDate), new { @class = "alphanumeric", maxlength = 10, size=8 })%>
                     <% //Mod 2015/09/14 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動(類似対応) id追加 %>
                    <button type="button" id="HistoryForCustomer" style="width:50px;height:20px" onclick="$('#CustomerCode').val()!='' ? openModalDialog('/ServiceSalesOrder/CriteriaDialog?customerCode='+$('#CustomerCode').val()) : false;">履歴</button>
                </td>
                <th style="width: 150px" rowspan="2">
                    部門 *
                </th>
                <td style="width: 200px">
                    <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric", size = "10", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'", "0" });%>
                </td>
            </tr>
            <tr>
                <th>
                    来店種別
                </th>
                <td>
                    <%=Html.DropDownList("ReceiptionType", (IEnumerable<SelectListItem>)ViewData["ReceiptionTypeList"])%>
                </td>
                <td>
                    <%=Html.TextBox("DepartmentName", Model.Department != null ? Model.Department.DepartmentName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
            </tr>
            <tr>
                <th>
                    来店目的
                </th>
                <td>
                    <%=Html.DropDownList("Purpose", (IEnumerable<SelectListItem>)ViewData["PurposeList"])%>
                </td>
                <th rowspan="2">
                    担当者 *
                </th>
                <td>
                    <%=Html.TextBox("EmployeeNumber", Model.Employee != null ? Model.Employee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" })%><%=Html.TextBox("EmployeeCode",Model.EmployeeCode,new {@class="alphanumeric",style="width:80px", maxlength="50",onblur="GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')"})%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "EmployeeCode", "EmployeeName", "'/Employee/CriteriaDialog/?DepartmentCode="+((Employee)Session["Employee"]).DepartmentCode+"'", "0" });%>
                </td>
            </tr>
            <tr>
                <th>
                    来店きっかけ
                </th>
                <td>
                    <%=Html.DropDownList("VisitOpportunity", (IEnumerable<SelectListItem>)ViewData["VisitOpportunityList"])%>
                </td>
                <td>
                    <%=Html.TextBox("EmployeeName", Model.Employee != null ? Model.Employee.EmployeeName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
            </tr>
            <tr>
                <th>
                    ショールームを知るきっかけ
                </th>
                <td>
                    <%=Html.DropDownList("KnowOpportunity", (IEnumerable<SelectListItem>)ViewData["KnowOpportunityList"])%>
                </td>
                <th rowspan="2">
                    イベント１
                </th>
                <td>
                    <%=Html.TextBox("CampaignCode1", Model.CampaignCode1, new { @class = "alphanumeric", maxlength = 20, onblur = "GetNameFromCode('CampaignCode1','CampaignName1','Campaign')" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "CampaignCode1", "CampaignName1", "'/Campaign/CriteriaDialog'", "0" });%>
                </td>
            </tr>
            <tr>
                <th>
                    ブランドの魅力
                </th>
                <td>
                    <%=Html.DropDownList("AttractivePoint", (IEnumerable<SelectListItem>)ViewData["AttractivePointList"])%>
                </td>
                <td>
                    <%=Html.TextBox("CampaignName1", Model.Campaign1 != null ? Model.Campaign1.CampaignName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
            </tr>
            <tr>
                <th>
                    今後の展望
                </th>
                <td>
                    <%=Html.DropDownList("Demand", (IEnumerable<SelectListItem>)ViewData["DemandList"])%>
                </td>
                <th rowspan="2">
                    イベント２
                </th>
                <td>
                    <%=Html.TextBox("CampaignCode2", Model.CampaignCode1, new { @class = "alphanumeric", maxlength = 20, onblur = "GetNameFromCode('CampaignCode2','CampaignName2','Campaign')" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "CampaignCode2", "CampaignName2", "'/Campaign/CriteriaDialog'", "0" });%>
                </td>
            </tr>
            <tr>
                <th>
                    アンケートの２次利用
                </th>
                <td>
                    <%=Html.DropDownList("Questionnarie", (IEnumerable<SelectListItem>)ViewData["QuestionnarieList"])%>
                </td>
                <td>
                    <%=Html.TextBox("CampaignName2", Model.Campaign2 != null ? Model.Campaign2.CampaignName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
            </tr>
        </table>
        <br />
        <table class="input-form-slim">
            <tr>
                <th colspan="4" class="input-form-title">
                    アンケート詳細情報
                </th>
            </tr>
            <tr>
                <th style="width: 150px">
                    アンケート記入日
                </th>
                <td style="width: 200px">
                    <%=Html.TextBox("QuestionnarieEntryDate", string.Format("{0:yyyy/MM/dd}", Model.QuestionnarieEntryDate), new { @class = "alphanumeric", maxlength = 10, size=10 })%>
                </td>
                <th style="width: 150px">
                    きっかけの媒体情報
                </th>
                <td style="width: 200px">
                    <%=Html.TextBox("MediaOpportunity", Model.MediaOpportunity, new { maxlength = 50 })%>
                </td>
            </tr>
            <tr>
                <th rowspan="2">
                    興味のある商品１
                </th>
                <td>
                    <%=Html.TextBox("InterestedCar1", Model.InterestedCar1, new { @class = "alphanumeric", maxlength = 30, onblur = "GetNameFromCode('InterestedCar1','InterestedCarName1','Car')" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "InterestedCar1", "InterestedCarName1", "'/Car/CriteriaDialog'", "0" });%>
                </td>
                <th rowspan="2">
                    興味のある商品２
                </th>
                <td>
                    <%=Html.TextBox("InterestedCar2", Model.InterestedCar2, new { @class = "alphanumeric", maxlength = 30, onblur = "GetNameFromCode('InterestedCar2','InterestedCarName2','Car')" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "InterestedCar2", "InterestedCarName2", "'/Car/CriteriaDialog'", "0" });%>
                </td>
            </tr>
            <tr>
                <td style="height: 20px">
                    <%=Html.TextBox("InterestedCarName1", Model.Car1 != null ? Model.Car1.CarName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
                <td style="height: 20px">
                    <%=Html.TextBox("InterestedCarName2", Model.Car2 != null ? Model.Car2.CarName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
            </tr>
            <tr>
                <th rowspan="2">
                    興味のある商品３
                </th>
                <td>
                    <%=Html.TextBox("InterestedCar3", Model.InterestedCar3, new { @class = "alphanumeric", maxlength = 30, onblur = "GetNameFromCode('InterestedCar3','InterestedCarName3','Car')" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "InterestedCar3", "InterestedCarName3", "'/Car/CriteriaDialog'", "0" });%>
                </td>
                <th rowspan="2">
                    興味のある商品４
                </th>
                <td>
                    <%=Html.TextBox("InterestedCar4", Model.InterestedCar4, new { @class = "alphanumeric", maxlength = 30, onblur = "GetNameFromCode('InterestedCar4','InterestedCarName4','Car')" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "InterestedCar4", "InterestedCarName4", "'/Car/CriteriaDialog'", "0" });%>
                </td>
            </tr>
            <tr>
                <td style="height: 20px">
                    <%=Html.TextBox("InterestedCarName3", Model.Car3 != null ? Model.Car3.CarName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
                <td style="height: 20px">
                    <%=Html.TextBox("InterestedCarName4", Model.Car4 != null ? Model.Car4.CarName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
                </td>
            </tr>
            <tr>
                <th>
                    備考１
                </th>
                <td colspan="3">
                    <%=Html.TextBox("Memo1", Model.Memo1, new { size = "80", maxsize = 100 })%>
                </td>
            </tr>
            <tr>
                <th>
                    備考２
                </th>
                <td colspan="3">
                    <%=Html.TextBox("Memo2", Model.Memo2, new { size = "80", maxsize = 100 })%>
                </td>
            </tr>
            <tr>
                <th>
                    備考３
                </th>
                <td colspan="3">
                    <%=Html.TextBox("Memo3", Model.Memo3, new { size = "80", maxsize = 100 })%>
                </td>
            </tr>
            <tr>
                <th rowspan="2">
                    自由質問１
                </th>
                <td>
                    &nbsp;質問：<%=Html.TextBox("Question1", Model.Question1, new { maxsize = 60 })%>
                </td>
                <th rowspan="2">
                    自由質問２
                </th>
                <td>
                     &nbsp;質問：<%=Html.TextBox("Question2", Model.Question2, new { maxsize = 60 })%>
                </td>
            </tr>
            <tr>
                <td>
                     &nbsp;回答：<%=Html.TextBox("Answer1", Model.Answer1, new { maxsize = 100 })%>
                </td>
                <td>
                     &nbsp;回答：<%=Html.TextBox("Answer2", Model.Answer2, new { maxsize = 100 })%>
                </td>
            </tr>
            <tr>
                <th rowspan="2">
                    自由質問３
                </th>
                <td>
                     &nbsp;質問：<%=Html.TextBox("Question3", Model.Question3, new { maxsize = 60 })%>
                </td>
                <th rowspan="2">
                    自由質問４
                </th>
                <td>
                     &nbsp;質問：<%=Html.TextBox("Question4", Model.Question4, new { maxsize = 60 })%>
                </td>
            </tr>
            <tr>
                <td>
                     &nbsp;回答：<%=Html.TextBox("Answer3", Model.Answer3, new { maxsize = 100 })%>
                </td>
                <td>
                     &nbsp;回答：<%=Html.TextBox("Answer4", Model.Answer4, new { maxsize = 100 })%>
                </td>
            </tr>
            <tr>
                <th rowspan="2">
                    自由質問５
                </th>
                <td>
                     &nbsp;質問：<%=Html.TextBox("Question5", Model.Question5, new { maxsize = 60 })%>
                </td>
                <td colspan="2" rowspan="2">
                </td>
            </tr>
            <tr>
                <td>
                     &nbsp;回答：<%=Html.TextBox("Answer5", Model.Answer5, new { maxsize = 100 })%>
                </td>
            </tr>
        </table>
    <%} %>
    <br />
</asp:Content>
