<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.Customer>" %>
<table class="input-form" style="width:700px">
    <tr>
        <th colspan="4" class="input-form-title">営業情報</th>
    </tr>
    <tr>
        <th style="width:150px">顧客ランク</th>
        <td><%=Html.DropDownList("CustomerRank", (IEnumerable<SelectListItem>)ViewData["CustomerRankList"])%></td>
        <th style="width:150px">顧客種別</th>
        <td><%=Html.DropDownList("CustomerKind", (IEnumerable<SelectListItem>)ViewData["CustomerKindList"])%></td>
    </tr>
    <tr>
        <th></th>
        <td></td>
        <th>支払方法</th>
        <td><%=Html.DropDownList("PaymentKind", (IEnumerable<SelectListItem>)ViewData["PaymentKindList"])%></td>
    </tr>
    <tr>
        <th>性別</th>
        <td><%=Html.DropDownList("Sex", (IEnumerable<SelectListItem>)ViewData["SexList"])%></td>
        <th>生年月日</th>
        <td><%=Html.TextBox("Birthday", string.Format("{0:yyyy/MM/dd}", Model.Birthday), new { @class = "alphanumeric", maxlength = 10, title = "和暦入力例:H23.12.24" }) %></td>
    </tr>
    <tr>
        <th>職業</th>
        <td><%=Html.DropDownList("Occupation", (IEnumerable<SelectListItem>)ViewData["OccupationList"])%></td>
        <th>車の所有</th>
        <td><%=Html.DropDownList("CarOwner", (IEnumerable<SelectListItem>)ViewData["CarOwnerList"])%></td>
    </tr>
    <tr>
        <th>メールアドレス</th>
        <td colspan="3"><%=Html.TextBox("MailAddress", Model.MailAddress, new { @class = "alphanumeric", size = 50, maxlength = 100 })%></td>
    </tr>
    <tr>
        <th>携帯電話番号</th>
        <td colspan="3"><%=Html.TextBox("MobileNumber", Model.MobileNumber, new { @class = "alphanumeric", maxlength = 15 })%></td>
    </tr>
    <tr>
        <th>携帯メールアドレス</th>
        <td colspan="3"><%=Html.TextBox("MobileMailAddress", Model.MobileMailAddress, new { @class = "alphanumeric", size = 50, maxlength = 100 })%></td>
    </tr>
      <tr>
          <% // Mod 2014/10/20 arc amii DM送付文言修正対応 #3116 項目名を修正 %>
        <th>営業DM可否</th>
        <td><%=Html.DropDownList("DmFlag", (IEnumerable<SelectListItem>)ViewData["DmFlagList"])%></td>
        <th>営業DM発送備考欄</th>
        <td><%=Html.TextBox("DmMemo", Model.DmMemo, new { maxlength = 100 })%></td>
      </tr>
    <% // Add 2014/08/20 arc amii DMフラグ機能拡張対応 #3069 DM可否についてのコメント行を追加 %>
    <tr>
        <th></th>
        <td colspan="3">
             <% // Mod 2014/10/20 arc amii DM送付文言修正対応 #3116 文言修正 %>
            ※車検案内DM送付の可否については車両マスタの「車検案内」にて設定して下さい
        </td>
    </tr>
      <tr>
        <th>部門 *</th>
        <td colspan="3">
        <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>
        <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode', 'DepartmentName', '/Department/CriteriaDialog')" />
        <span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"])%></span>
        </td>
      </tr>
      <tr>
        <th>営業担当者</th>
        <td colspan="3">
        <%=Html.TextBox("CarEmployeeCode", Model.CarEmployeeCode, new { @class = "alphanumeric", size = 20, maxlength = 50, onblur = "GetNameFromCode('CarEmployeeCode','CarEmployeeName','Employee')" })%>
        <img alt="社員検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarEmployeeCode', 'CarEmployeeName', '/Employee/CriteriaDialog')" />
        &nbsp;&nbsp;<span id="CarEmployeeName"><%=Html.Encode(ViewData["CarEmployeeName"])%></span>
        </td>
      </tr>
      <tr>
        <th>サービス担当部門 *</th>
        <td colspan="3">
        <%=Html.TextBox("ServiceDepartmentCode", Model.ServiceDepartmentCode, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('ServiceDepartmentCode','ServiceDepartmentName','Department')" }) %>
        <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('ServiceDepartmentCode','ServiceDepartmentName','/Department/CriteriaDialog?BusinessType=002')" />
        <span id="ServiceDepartmentName"><%=Html.Encode(ViewData["ServiceDepartmentName"]) %></span>
        </td>
      </tr>
      <tr>
        <th>サービス担当者</th>
        <td colspan="3"><%=Html.TextBox("ServiceEmployeeCode", Model.ServiceEmployeeCode, new { @class = "alphanumeric", size = 20, maxlength = 50, onblur = "GetNameFromCode('ServiceEmployeeCode','ServiceEmployeeName','Employee')" })%>
        <img alt="社員検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('ServiceEmployeeCode', 'ServiceEmployeeName', '/Employee/CriteriaDialog')" />
        &nbsp;&nbsp;<span id="ServiceEmployeeName"><%=Html.Encode(ViewData["ServiceEmployeeName"])%></span>
        </td>
      </tr>
      <tr>
        <th>勤務先名</th>
        <td colspan="3"><%=Html.TextBox("WorkingCompanyName", Model.WorkingCompanyName, new { size = 50, maxlength = 40 })%></td>
      </tr>
      <tr>
        <th>勤務先住所</th>
        <td colspan="3"><%=Html.TextBox("WorkingCompanyAddress", Model.WorkingCompanyAddress, new { size = 50, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>勤務先電話番号</th>
        <td><%=Html.TextBox("WorkingCompanyTelNumber", Model.WorkingCompanyTelNumber, new { @class = "alphanumeric", maxlength = 15 })%></td>
        <th>役職名</th>
        <td><%=Html.TextBox("PositionName", Model.PositionName, new { maxlength = 20 })%></td>
      </tr>
      <tr>
        <th>取引先担当者名</th>
        <td><%=Html.TextBox("CustomerEmployeeName", Model.CustomerEmployeeName, new { maxlength = 40 })%></td>
        <th>経理担当者名</th>
        <td><%=Html.TextBox("AccountEmployeeName", Model.AccountEmployeeName, new { maxlength = 40 })%></td>
      </tr>
</table>

