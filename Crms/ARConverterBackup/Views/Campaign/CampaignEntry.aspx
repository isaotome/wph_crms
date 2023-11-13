<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Campaign>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	イベント入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% 
   // -----------------------------------------------------------------------------------------------------
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>
<%using (Html.BeginForm("Entry", "Campaign", new { id = 0 }, FormMethod.Post))
  { %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
       <% if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
          {%> 
       <td onclick="document.forms[0].action.value = 'delete';formSubmit();"><img src="/Content/Images/cancel.png" alt="削除" class="command_btn" />&nbsp;削除</td>
       <%} %>
       <td onclick="document.forms[0].action.value = 'save';formSubmit();"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"])%>
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.Hidden("action", "") %>
<%=Html.Hidden("DelLine","") %>
<div id="input-form">
    <%=Html.ValidationSummary()%>
    <br />
<table class="input-form">
    <tr>
        <th style="width:100px">イベントコード *</th>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="CampaignCode" name="CampaignCode" value="<%=Model.CampaignCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("CampaignCode", Model.CampaignCode, new { @class = "alphanumeric", maxlength = 20 })%><%} %>
        </td>
        <th style="width:100px">イベント名 *</th>
        <td><%=Html.TextBox("CampaignName", Model.CampaignName, new { maxlength = 100 })%></td>
        <th style="width:100px">対象業務 *</th>
        <td><%=Html.DropDownList("TargetService", (IEnumerable<SelectListItem>)ViewData["TargetServiceList"])%></td>
    </tr>
    <tr>
        <th>イベント開始日</th>
        <td><%=Html.TextBox("CampaignStartDate", string.Format("{0:yyyy/MM/dd}", Model.CampaignStartDate), new { @class = "alphanumeric", maxlength = 10 })%></td>
        <th>イベント終了日</th>
        <td><%=Html.TextBox("CampaignEndDate", string.Format("{0:yyyy/MM/dd}", Model.CampaignEndDate), new { @class = "alphanumeric", maxlength = 10 })%></td>
        <th rowspan="2">担当者 *</th>
        <td>            
            <%=Html.TextBox("EmployeeNumber", Model.Employee != null ? Model.Employee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" })%>
            <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "alphanumeric", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "EmployeeCode", "EmployeeName", "'/Employee/CriteriaDialog'", "0" }); %>
        </td>
    </tr>
    <tr>
        <th>イベントタイプ</th>
        <td><%=Html.DropDownList("CampaignType", (IEnumerable<SelectListItem>)ViewData["CampaignTypeList"])%></td>
        <th>広告媒体</th>
        <td><%=Html.TextBox("AdvertisingMedia", Model.AdvertisingMedia, new { maxlength = 50 })%></td>
        <td style="height:20px">            
            <%=Html.TextBox("EmployeeName", Model.Employee!=null ? Model.Employee.EmployeeName : "", new { @class = "readonly", style = "width:150px", @readonly = "readonly" })%>
        </td>       
    </tr>
    <tr>
        <th>掲載開始日</th>
        <td><%=Html.TextBox("PublishStartDate", string.Format("{0:yyyy/MM/dd}", Model.PublishStartDate), new { @class = "alphanumeric", maxlength = 10 })%></td>
        <th>掲載終了日</th>
        <td><%=Html.TextBox("PublishEndDate", string.Format("{0:yyyy/MM/dd}", Model.PublishEndDate), new { @class = "alphanumeric", maxlength = 10 })%></td>
        <th rowspan="2">施策ローン</th>
        <td><%=Html.TextBox("LoanCode", Model.LoanCode, new { @class = "alphanumeric", maxlength = 10, onblur = "GetNameFromCode('LoanCode','LoanName','Loan')" })%>
            <img alt="ローン検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('LoanCode', 'LoanName', '/Loan/CriteriaDialog')" />
        </td>
    </tr>
    <tr>
        <th>費用</th>
        <td><%=Html.TextBox("Cost", string.Format("{0:N0}", Model.Cost), new { @class = "money", maxlength = 10 })%></td>
        <th>メーカーサポート</th>
        <td><%=Html.TextBox("MakerSupport", Model.MakerSupport, new { maxlength = 50 })%></td>
        <td style="height:20px"><span id="LoanName"><%=Html.Encode(ViewData["LoanName"])%></span></td>       
    </tr>
</table>
<br />
<br />
<table class="input-form">
    <tr>
        <th style="width: 15px;text-align:center"><img alt="行追加" src="/Content/Images/plus.gif" style="cursor:pointer" onclick="$('#DelLine').val('-1');formSubmit();" /></th>
        <th style="width:185px">グレードコード *</th>
        <th style="width:100px">メーカー名</th>
        <th style="width:100px">モデルコード</th>
        <th style="width:150px">車種名</th>
        <th style="width:300px">グレード名</th>
    </tr>
</table>
<div style="overflow-y:scroll;width:910px;height:300px">
<table class="input-form">
<%for (int i = 0; i < Model.CampaignCar.Count; i++)
  {
      string namePrefix = string.Format("line[{0}].", i);
      CarGrade carGrade = ((List<CarGrade>)ViewData["CarGradeList"])[i];
      string makerName = "";
      //2014/05/29 vs2012対応 arc yano 各コントロールにid追加
      string idPrefix = string.Format("line[{0}]_", i);
      
      try {
          makerName = carGrade.Car.Brand.Maker.MakerName;
      } catch (NullReferenceException) {
      }
      string carName = "";
      try {
          carName = carGrade.Car.CarName;
      } catch (NullReferenceException) {
      }%>
    <tr>
        <td style="width:15px;text-align:center"><%=Html.Hidden(namePrefix + "LineNumber", i + 1, new { id = idPrefix + "LineNumber"})%><img alt="行削除" src="/Content/Images/minus.gif" style="cursor:pointer" onclick="$('#DelLine').val('<%=i %>');formSubmit();" /></td>
        <td style="width:185px">
            <%=Html.TextBox(namePrefix + "CarGradeCode", Model.CampaignCar[i].CarGradeCode, new { id = idPrefix + "CarGradeCode", @class = "alphanumeric", maxlength = 30, onblur = "GetGradeMasterFromCode(" + CommonUtils.IntToStr(i) + ")" })%>
            <% //Mod 2014/07/15 arc yano chrome対応 openSearchDialogに渡すパラメータをname→idに %>
            <%//Mod 2022/01/08 yano #4121 %>
            <img alt="グレード検索" src="/Content/Images/search.jpg" onclick="var callback = function() {GetGradeMasterFromCode(<%=i%>)}; openSearchDialog('<%=idPrefix + "CarGradeCode" %>','','/CarGrade/CriteriaDialog', null, null, null, null, callback);GetGradeMasterFromCode(<%=i%>);" />
            <%--<img alt="グレード検索" src="/Content/Images/search.jpg" onclick="openSearchDialog('<%=idPrefix + "CarGradeCode" %>','','/CarGrade/CriteriaDialog');GetGradeMasterFromCode(<%=i%>);" />--%>
        </td>
        <% //Mod 2014/07/15 arc yano chrome対応 openSearchDialogに渡すパラメータをname→idに %>
        <% //Mod 2014/07/16 arc amii chrome対応 white-space:normalを追加し、フル桁表示しても文字列折り返しが行われるよう修正 %>
        <td style="width:100px;white-space:normal"><span id="<%=idPrefix + "MakerName"%>"><%=Html.Encode(makerName)%></span></td>
        <td style="width:100px;white-space:normal"><span id="<%=idPrefix + "ModelCode"%>"><%=Html.Encode(carGrade.ModelCode)%></span></td>
        <td style="width:150px;white-space:normal"><span id="<%=idPrefix + "CarName"%>"><%=Html.Encode(carName)%></span></td>
        <td style="width:300px;white-space:normal"><span id="<%=idPrefix + "CarGradeName"%>"><%=Html.Encode(carGrade.CarGradeName)%></span></td>
   </tr>
<%} %>
</table>
</div>
</div>
<%} %>
<br />
</asp:Content>
