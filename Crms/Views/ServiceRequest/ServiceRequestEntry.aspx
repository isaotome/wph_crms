<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master"
    Inherits="System.Web.Mvc.ViewPage<CrmsDao.ServiceRequest>" %>

<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    作業依頼作成
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <table class="command">
        <tr>
            <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
            <td onclick="document.forms[0].PrintReport.value='ServiceRequest';formSubmit();"><img src="/Content/Images/pdf.png" alt="車両作業依頼書" class="command_btn" />&nbsp;車両作業依頼書</td>
        </tr>
    </table>
    <br />
    <!--Add 2017/04/13 arc nakayama #3749_作業依頼書編集画面　コメント追加-->
    <a><b>この画面で入力したコメントと有償選択は保存後に再表示されません。<br />保存結果はサービス＞ 車両作業依頼画面から確認できます。</b></a>
    <br />
    <%=Html.ValidationSummary() %>
    <%using (Html.BeginForm("Entry", "ServiceRequest", FormMethod.Post)) { %>
    <%=Html.Hidden("close",ViewData["close"]) %>
    <%=Html.Hidden("OriginalSlipNumber",Model.OriginalSlipNumber) %>
    <%=Html.Hidden("ServiceRequestId",Model.ServiceRequestId) %>
    <%=Html.Hidden("PrintReport", "")%>
    <%=Html.Hidden("reportName", ViewData["reportName"])%>
    <%=Html.Hidden("reportParam", Model.OriginalSlipNumber)%>
    <%
        CarSalesHeader salesHeader = Model.CarSalesHeader;
        string locationName = "";
        try { locationName = salesHeader.SalesCar.Location.LocationName; } catch (NullReferenceException) { }
        string newUsedType = "";
        try { newUsedType = salesHeader.c_NewUsedType.Name; } catch (NullReferenceException) { }
        string salesType = "";
        try { salesType = salesHeader.c_SalesType.Name; } catch (NullReferenceException) { }
        string carGradeCode = "";
        try { carGradeCode = salesHeader.CarGradeCode; } catch (NullReferenceException) { }
        string makerName = "";
        try { makerName = salesHeader.MakerName; } catch (NullReferenceException) { }
        string carBrandName = "";
        try { carBrandName = salesHeader.CarBrandName; } catch (NullReferenceException) { }
        string carName = "";
        try { carName = salesHeader.CarName; } catch (NullReferenceException) { }
        string carGradeName = "";
        try { carGradeName = salesHeader.CarGradeName; } catch (NullReferenceException) { }
        string exteriorColorName = "";
        try { exteriorColorName = salesHeader.ExteriorColorName; } catch (NullReferenceException) { }
        string interiorColorName = "";
        try { interiorColorName = salesHeader.InteriorColorName; } catch (NullReferenceException) { }
        string vin = "";
        try { vin = salesHeader.Vin; } catch (NullReferenceException) { }
        string modelName = "";
        try { modelName = salesHeader.ModelName; } catch (NullReferenceException) { }
        decimal? mileage = 0m;
        try { mileage = salesHeader.Mileage; } catch (NullReferenceException) { }
        string mileageUnit = "";
        try { mileageUnit = salesHeader.c_MileageUnit.Name; } catch (NullReferenceException) { }
        Customer customer = salesHeader.Customer;
        string customerCode = "";
        try { customerCode = salesHeader.CustomerCode; } catch (NullReferenceException) { }
        string customerName = "";
        try { customerName = customer.CustomerName; } catch (NullReferenceException) { }
        string customerNameKana = "";
        try { customerNameKana = customer.CustomerNameKana; } catch (NullReferenceException) { }
        string customerAddress = "";
        try { customerAddress = customer.Prefecture + customer.City + customer.Address1 + customer.Address2; } catch (NullReferenceException) { }
        DateTime? makerShipmentDate = null;
        try { makerShipmentDate = Model.CarPurchaseOrder.MakerShipmentDate; } catch (NullReferenceException) { }
        DateTime? arrivalPlanDate = null;
        try { arrivalPlanDate = Model.CarPurchaseOrder.ArrivalPlanDate; } catch (NullReferenceException) { }
        DateTime? registrationPlanDate = null;
        try { registrationPlanDate = Model.CarPurchaseOrder.RegistrationPlanDate; } catch (NullReferenceException) { }
        DateTime? salesPlanDate = null;
        try { salesPlanDate = salesHeader.SalesPlanDate; } catch (NullReferenceException) { }
    %>
    <div style="margin:auto;height: 100px">
        <br />
        <table class="input-form">
            <tr>
                <th class="input-form-title" colspan="2">
                    依頼情報
                </th>
            </tr>
            <tr>
                <th rowspan="2">
                    依頼先部門&nbsp;*
                </th>
                <td>
                    <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { size = "8", @class = "alphanumeric", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img
                        alt="部門検索" style="cursor: pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" />
                </td>
            </tr>
            <tr>
                <td style="height: 20px">
                    <span id="DepartmentName">
                        <%=ViewData["DepartmentName"] %></span>
                </td>
            </tr>
            <tr>
                <th style="width: 70px"><% //Mod 2014/07/16 arc yano chrome対応 幅調整 100px→70px %>
                    備考
                </th>
                <td style="width: 230px">
                    <%=Html.TextBox("Memo", Model.Memo, new { size="37" ,maxlength="100"})%>
                </td>
            </tr>
        </table>
    </div>
    <br />
    <br />
    <table class="input-form">
        <tr>
            <th colspan="6" class="input-form-title">
                販売車両情報
            </th>
        </tr>
        <tr>
            <th style="width: 80px; height: 20px">
                新中区分
            </th>
            <td style="width: 200px; height: 20px">
                <%=Html.Encode(newUsedType)%>
            </td>
            <th style="width: 80px; height: 20px">
                販売区分
            </th>
            <td style="width: 200px; height: 20px">
                <%=Html.Encode(salesType)%>
            </td>
            <th style="width: 80px; height: 20px">
                グレードコード
            </th>
            <td style="width: 200px; height: 20px">
                <%=Html.Encode(carGradeCode) %>
            </td>
        </tr>
        <tr>
            <th style="height: 20px">
                メーカー名
            </th>
            <td>
                <%=Html.Encode(makerName) %>
            </td>
            <th style="height: 20px">
                ブランド名
            </th>
            <td>
                <%=Html.Encode(carBrandName) %>
            </td>
            <th style="height: 20px">
                車種名
            </th>
            <td>
                <%=Html.Encode(carName) %>
            </td>
        </tr>
        <tr>
            <th style="height: 20px">
                グレード名
            </th>
            <td>
                <%=Html.Encode(carGradeName) %>
            </td>
            <th style="height: 20px">
                外装色
            </th>
            <td>
                <%=Html.Encode(exteriorColorName) %>
            </td>
            <th style="height: 20px">
                内装色
            </th>
            <td>
                <%=Html.Encode(interiorColorName)%>
            </td>
        </tr>
        <tr>
            <th style="height: 20px">
                車台番号
            </th>
            <td>
                <%=Html.Encode(vin) %>
            </td>
            <th style="height: 20px">
                型式
            </th>
            <td>
                <%=Html.Encode(modelName) %>
            </td>
            <th style="height: 20px">
                走行距離
            </th>
            <td>
                <%=Html.Encode(mileage) %><%=Html.Encode(mileageUnit) %>
            </td>
        </tr>
    </table>
    <div>
        <br />
        <table class="input-form">
            <tr>
                <th colspan="4" class="input-form-title">
                    顧客情報
                </th>
            </tr>
            <tr>
                <th style="width: 100px; height: 20px">
                    顧客コード
                </th>
                <td colspan="3" style="height: 20px">
                    <%=Html.Encode(customerCode) %>
                </td>
            </tr>
            <tr>
                <th style="width: 100px; height: 20px">
                    顧客名
                </th>
                <td style="width: 150px; height: 20px">
                    <%=Html.Encode(customerName) %>
                </td>
                <th style="width: 100px; height: 20px">
                    顧客名(カナ)
                </th>
                <td style="width: 150px; height: 20px">
                    <%=Html.Encode(customerNameKana) %>
                </td>
            </tr>
            <tr>
                <th style="height: 20px">
                    住所
                </th>
                <td style="height: 20px" colspan="3">
                    <%=Html.Encode(customerAddress) %>
                </td>
            </tr>
        </table>
        <br />
    </div>
    <div>
        <table class="input-form">
            <tr>
                <th colspan="6" class="input-form-title">
                    発注車両情報
                </th>
            </tr>
            <tr>
                <th style="width: 80px; height: 20px">
                    現在地
                </th>
                <td style="width: 150px; height: 20px">
                    <%=Html.Encode(locationName) %>
                </td>
                <th style="width: 80px; height: 20px">
                    出荷予定日
                </th>
                <td style="width: 150px; height: 20px">
                    <%=Html.Encode(string.Format("{0:yyyy/MM/dd}", makerShipmentDate))%>
                </td>
                <th style="width: 80px; height: 20px">
                    到着予定日
                </th>
                <td style="width: 150px; height: 20px">
                    <%=Html.Encode(string.Format("{0:yyyy/MM/dd}", arrivalPlanDate))%>
                </td>
            </tr>
            <tr>
                <th style="height: 20px">
                    登録予定日
                </th>
                <td style="height: 20px">
                    <%=Html.Encode(string.Format("{0:yyyy/MM/dd}", registrationPlanDate))%>
                </td>
                <th style="height: 20px">
                    納車予定日
                </th>
                <td style="height: 20px">
                    <%=Html.Encode(string.Format("{0:yyyy/MM/dd}",salesPlanDate))%>
                </td>
                <th style="height: 20px">
                    区分
                </th>
                <td>
                    <%=Html.DropDownList("OwnershipChange",(IEnumerable<SelectListItem>)ViewData["OwnershipChangeList"]) %>
                </td>
            </tr>
            <tr>
                <th style="height: 20px">
                    12ヶ月点検
                </th>
                <td style="height: 20px">
                    <%=Html.DropDownList("AnnualInspection",(IEnumerable<SelectListItem>)ViewData["AnnualInspectionList"]) %>
                </td>
                <th style="height: 20px">
                    保証継承
                </th>
                <td style="height: 20px">
                    <%=Html.DropDownList("InsuranceInheritance",(IEnumerable<SelectListItem>)ViewData["InsuranceInheritanceList"])%>
                </td>
                <th style="height: 20px">
                    希望納期&nbsp;*
                </th>
                <td style="height: 20px">
                    <%=Html.TextBox("DeliveryRequirement", string.Format("{0:yyyy/MM/dd}", Model.DeliveryRequirement), new { @class="alphanumeric",size="10",maxlength="10",onchange ="return chkDate3(document.getElementById('DeliveryRequirement').value, 'DeliveryRequirement')" })%>
                </td>
            </tr>
        </table>
        <br />
        <table class="input-form">
            <tr>
                <th colspan="6" class="input-form-title">
                    オプション
                </th>
            </tr>
            <tr>
                <th style="width: 100px">
                    <div style="text-align: center">
                        区分</div>
                </th>
                <th style="width: 170px">
                    <div style="text-align: center">
                        品番</div>
                </th>
                <th style="width: 250px">
                    <div style="text-align: center">
                        品名</div>
                </th>
                <th style="width: 63px">
                    <div style="text-align: center">
                        金額</div>
                </th>
                <th style="width: 200px">
                    <div style="text-align: center">
                        コメント</div>
                </th>
                <th style="width: 50px">
                    <div style="text-align: center">
                        有償</div>
                </th>
            </tr>
        </table>
        <div style="overflow-y: scroll; width: 895px; height: 300px">
            <table class="input-form">
            <%if(salesHeader!=null && salesHeader.CarSalesLine!=null && salesHeader.CarSalesLine.Count>0){ %>
                <%for (int i = 0; i < Model.CarSalesHeader.CarSalesLine.Count; i++) { 
                  //2014/05/30 vs2012対応 arc yano 各コントロールにid追加
                      string nameprefix = string.Format("line[{0}].", i);
                      string idprefix = string.Format("line[{0}]_", i);
                %>
                <tr>
                    <% // Mod 2014/07/17 arc amii chrome対応 文字列が収まりきらなかった場合、改行するよう修正 %>
                    <td style="width: 100px;white-space:normal">
                        <%=Html.Hidden(nameprefix + "LineNumber", Model.ServiceRequestLine[i].LineNumber, new { id = idprefix + "LineNumber"})%>
                        <%=Model.ServiceRequestLine[i].c_OptionType.Name %>
                        <%=Html.Hidden(nameprefix + "OptionType", Model.ServiceRequestLine[i].c_OptionType.Code, new { id = idprefix + "OptionType"})%>
                    </td>
                    <td style="width: 170px;white-space:normal">
                        <%=Model.ServiceRequestLine[i].CarOptionCode %><%=Html.Hidden( nameprefix + "CarOptionCode", Model.ServiceRequestLine[i].CarOptionCode, new { id = idprefix + "CarOptionCode"})%>
                    </td>
                    <td style="width: 250px;white-space:normal">
                        <%=Model.ServiceRequestLine[i].CarOptionName %><%=Html.Hidden( nameprefix + "CarOptionName", Model.ServiceRequestLine[i].CarOptionName, new { id = idprefix + "CarOptionName"})%>
                    </td>
                    <td style="width: 63px; text-align: right;white-space:normal">
                        <%=string.Format("{0:N0}",Model.CarSalesHeader.CarSalesLine[i].Amount) %><% //2014/08/04 id追加漏れ対応 %><%=Html.Hidden( nameprefix + "Amount", Model.ServiceRequestLine[i].Amount, new{ id = idprefix + "Amount"}) %>
                    </td>
                    <td style="width: 200px;white-space:normal">
                        <%=Html.TextBox( nameprefix + "RequestComment", Model.ServiceRequestLine[i].RequestComment,new { id = idprefix + "RequestComment", size="25",maxlength="100"}) %><% //2014/08/04 id追加漏れ対応 %>
                    </td>
                    <td style="width: 50px; text-align: center;white-space:normal">
                        <%=Html.CheckBox( nameprefix + "ClaimType", Model.ServiceRequestLine[i].ClaimType != null && Model.ServiceRequestLine[i].ClaimType.Equals(true)) %>
                    </td>
                </tr>
                <%} %>
            <%} %>
            </table>
        </div>
        <table class="input-form">
            <tr>
                <th style="width: 535px">
                    合計
                </th>
                <th style="width: 63px; text-align: right">
                    <%=CommonUtils.DefaultNbsp(string.Format("{0:N0}", Model.CarSalesHeader.ShopOptionAmount + Model.CarSalesHeader.MakerOptionAmount)) %>
                </th>
                <th style="width: 255px">
                </th>
            </tr>
        </table>
    </div>
    <%} %>
</asp:Content>
