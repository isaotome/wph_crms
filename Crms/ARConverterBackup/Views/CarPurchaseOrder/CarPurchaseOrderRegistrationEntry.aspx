<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarPurchaseOrder>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両発注処理
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
    bool reservationStatus = false;
    bool registrationStatus = false;
    if (Model.ReservationStatus != null && Model.ReservationStatus.Equals("1")) {
        reservationStatus = true;
    }
    if (Model.RegistrationStatus != null && Model.RegistrationStatus.Equals("1")) {
        registrationStatus = true;
    }
%>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
       <%if (reservationStatus && !registrationStatus) { %>
       <td onclick="document.forms[0].actionType.value='Registration';formSubmit()"><img src="/Content/Images/build.png" alt="登録処理" class="command_btn" />&nbsp;登録処理</td>
       <%} %>
   </tr>
</table>
<br />
<%using (Html.BeginForm("RegistrationEntry", "CarPurchaseOrder", FormMethod.Post)) { %>
<%=Html.Hidden("close", ViewData["close"])%>
<%=Html.Hidden("actionType","")%>
<%=Html.Hidden("CarPurchaseOrderNumber", Model.CarPurchaseOrderNumber) %>
<%=Html.Hidden("ReservationStatus",Model.ReservationStatus) %>
<%=Html.Hidden("SalesCarNumber",Model.SalesCarNumber) %>
<%=Html.ValidationSummary()%>
<br />
    <table class="input-form" style="width:98%">
      <tr>
        <th style="width:100px;height:20px">伝票番号</th>
        <td>
            <%=Html.Encode(Model.SlipNumber) %>
            <%=Html.Hidden("SlipNumber", Model.SlipNumber) %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">受注日</th>
        <td>
            <%=Html.Encode(string.Format("{0:yyyy/MM/dd}",Model.CarSalesHeader!=null ? Model.CarSalesHeader.SalesOrderDate : null)) %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">部門</th>
        <td>
            <%=Html.Encode(Model.CarSalesHeader!=null && Model.CarSalesHeader.Department!=null ? Model.CarSalesHeader.Department.DepartmentName : "")%>
        </td>
      </tr>
      <tr>
        <th style="height:20px">営業担当者</th>
        <td>
            <%=Html.Encode(Model.CarSalesHeader!=null && Model.CarSalesHeader.Employee!=null ? Model.CarSalesHeader.Employee.EmployeeName : "") %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">顧客名</th>
        <td>
            <%=Html.Encode(Model.CarSalesHeader!=null && Model.CarSalesHeader.Customer!=null ? Model.CarSalesHeader.Customer.CustomerName : "") %>
        </td>
      </tr>
        <% // Del 2014/09/25 arc amii 西暦和暦対応 #3082 登録予定月の表示をしない %>
      <%--<tr>
        <th style="height:20px">登録予定月</th>
        <td>
            <%if(registrationStatus){ %>
                <%=Html.TextBox("RegistrationPlanMonth", Model.RegistrationPlanMonth, new { @class = "readonly", size = "15", @readonly = "readonly" })%>
            <%}else{ %>
                <%=Html.TextBox("RegistrationPlanMonth", Model.RegistrationPlanMonth, new { size = "15", maxlength = "10" })%>
            <%} %>
        </td>
      </tr>--%>
      <tr>
          <% // Mod 2014/09/25 arc amii 西暦和暦対応 #3082 登録予定日の入力を和暦で行うよう修正 %>
        <th style="height:20px">登録予定日</th>
          <td>
            <%=Html.DropDownList("RegistrationPlanDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["RegistrationPlanGengouList"], new { id = "RegistrationPlanDateWareki.Gengou" })%>
            <%=Html.TextBox("RegistrationPlanDateWareki.Year", Model.RegistrationPlanDateWareki != null ? Model.RegistrationPlanDateWareki.Year : null, new { id = "RegistrationPlanDateWareki.Year", @class = "numeric", style = "width:15px", maxlength = 2 })%>年
            <%=Html.TextBox("RegistrationPlanDateWareki.Month", Model.RegistrationPlanDateWareki != null ? Model.RegistrationPlanDateWareki.Month : null, new {  id = "RegistrationPlanDateWareki.Month", @class = "numeric", style = "width:15px", maxlength = 2 })%>月
            <%=Html.TextBox( "RegistrationPlanDateWareki.Day", Model.RegistrationPlanDateWareki != null ? Model.RegistrationPlanDateWareki.Day : null, new { id = "RegistrationPlanDateWareki.Day", @class = "numeric", style = "width:15px", maxlength = 2 })%>日
         </td>
        <%--<td>
            <%if(registrationStatus){ %>
                <%=Html.TextBox("RegistrationPlanDate", string.Format("{0:yyyy/MM/dd}", Model.RegistrationPlanDate), new { @class = "alphanumeric readonly", size = "10", maxlength = "10", @readonly = "readonly" })%>
            <%}else{ %>
                <%=Html.TextBox("RegistrationPlanDate", string.Format("{0:yyyy/MM/dd}", Model.RegistrationPlanDate), new { @class = "alphanumeric", size = "10", maxlength = "10" })%>
            <%} %>
        </td>--%>
      </tr>
      <tr>
        <th style="height:20px">登録日</th>
          <% // Mod 2014/09/25 arc amii 西暦和暦対応 #3082 登録日の入力を和暦で行うよう修正 %>
        <td>
            <%=Html.DropDownList("RegistrationDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["RegistrationGengouList"], new { id = "RegistrationDateWareki.Gengou" })%>
            <%=Html.TextBox("RegistrationDateWareki.Year", Model.RegistrationDateWareki != null ? Model.RegistrationDateWareki.Year : null, new { id = "RegistrationDateWareki.Year", @class = "numeric", style = "width:15px", maxlength = 2 })%>年
            <%=Html.TextBox("RegistrationDateWareki.Month", Model.RegistrationDateWareki != null ? Model.RegistrationDateWareki.Month : null, new {  id = "RegistrationDateWareki.Month", @class = "numeric", style = "width:15px", maxlength = 2 })%>月
            <%=Html.TextBox( "RegistrationDateWareki.Day", Model.RegistrationDateWareki != null ? Model.RegistrationDateWareki.Day : null, new { id = "RegistrationDateWareki.Day", @class = "numeric", style = "width:15px", maxlength = 2 })%>日
         </td>
      </tr>
      <tr>
        <th style="height:20px">書類購入希望日</th>
        <td>
            <%=Html.TextBox("DocumentPurchaseRequestDate",string.Format("{0:yyyy/MM/dd}",Model.DocumentPurchaseRequestDate) , new { @class = "alphanumeric", size = "10",maxlength="10" })%>
        </td>
      </tr>
      <tr>
        <th style="height:20px">書類購入日</th>
        <td>
            <%=Html.Encode(string.Format("{0:yyyy/MM/dd}",Model.DocumentPurchaseDate)) %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">書類到着予定日</th>
        <td>
            <%=Html.TextBox("DocumentReceiptPlanDate", string.Format("{0:yyyy/MM/dd}", Model.DocumentReceiptDate), new { @class = "alphanumeric", size = "10", maxlength = "10" })%>
        </td>
      </tr>
      <tr>
        <th style="height:20px">書類到着日</th>
        <td>
            <%=Html.TextBox("DocumentReceiptDate", string.Format("{0:yyyy/MM/dd}", Model.DocumentReceiptDate), new { @class = "alphanumeric", size = "10", maxlength = "10" })%>
        </td>
      </tr>
      <tr>
        <th style="height:20px">メーカー</th>
        <td>
            <%=Html.Encode(Model.CarSalesHeader != null ? Model.CarSalesHeader.MakerName : "") %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">車種</th>
        <td><%=Html.Encode(Model.CarSalesHeader != null ? Model.CarSalesHeader.CarName : "") %></td>
      </tr>
      <tr>
        <th style="height:20px">モデルコード</th>
        <td><%=Html.Encode(Model.CarSalesHeader!=null && Model.CarSalesHeader.CarGrade!=null ? Model.CarSalesHeader.CarGrade.ModelCode : "") %></td>
      </tr>
      <tr>
        <th style="height:20px">型式</th>
        <td><%=Html.Encode(Model.CarSalesHeader != null ? Model.CarSalesHeader.ModelName : "") %></td>
      </tr>
      <tr>
        <th style="height:20px">グレード</th>
        <td><%=Html.Encode(Model.CarSalesHeader != null ? Model.CarSalesHeader.CarGradeName : "") %></td>
      </tr>
      <tr>
        <th style="height:20px">外装色</th>
        <td><%=Html.Encode(Model.CarSalesHeader != null ? Model.CarSalesHeader.ExteriorColorName : "") %></td>
      </tr>
      <tr>
        <th style="height:20px">内装色</th>
        <td><%=Html.Encode(Model.CarSalesHeader!= null ? Model.CarSalesHeader.InteriorColorName : "") %></td>
      </tr>
   </table>
<%} %>
</asp:Content>

