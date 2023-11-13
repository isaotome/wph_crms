<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.Customer>" %>
<%@ Import Namespace="CrmsDao" %> 
<table class="input-form" style="width:700px">
    <tr>
        <th colspan="4" class="input-form-title">基本情報</th>
    </tr>
    <tr>
        <th style="width:150px">顧客コード</th>
        <td colspan="3">
            <%=Html.Encode(Model.CustomerCode) %><%=Html.Hidden("CustomerCode",Model.CustomerCode) %>
        </td>
    </tr>
    <tr>
        <th>顧客区分</th>
        <td colspan="3"><%=Html.DropDownList("CustomerType", (IEnumerable<SelectListItem>)ViewData["CustomerTypeList"])%></td>
    </tr>
    <tr>
        <th>前株・後株</th>
        <td colspan="3"><%=Html.DropDownList("CorporationType",(IEnumerable<SelectListItem>)ViewData["CorporationTypeList"])%></td>
    </tr>
    <tr>
        <th>顧客名1(姓) *</th>
        <% // Mod 2014/07/16 arc amii 既存バグ対応 姓と名にフル桁入力し、登録するとシステムエラーが発生した為、入力可能文字数を40→39に修正 %>
        <td colspan="3"><%=Html.TextBox("FirstName", Model.FirstName, new { size = 50, maxlength = 39 })%></td>
    </tr>
    <tr>
        <th>顧客名2(名)</th>
        <% // Mod 2014/07/16 arc amii 既存バグ対応 姓と名にフル桁入力し、登録するとシステムエラーが発生した為、入力可能文字数を40→39に修正 %>
        <td colspan="3"><%=Html.TextBox("LastName", Model.LastName, new { size = 50, maxlength = 39 })%></td>
    </tr>
    <tr>
        <th>顧客名1(姓カナ) *</th>
        <% // Mod 2014/07/16 arc amii 既存バグ対応 姓と名にフル桁入力し、登録するとシステムエラーが発生した為、入力可能文字数を40→39に修正 %>
        <td colspan="3"><%=Html.TextBox("FirstNameKana", Model.FirstNameKana, new { size = 50, maxlength = 39 })%></td>
    </tr>
    <tr>
        <th>顧客名2(名カナ)</th>
        <% // Mod 2014/07/16 arc amii 既存バグ対応 姓と名にフル桁入力し、登録するとシステムエラーが発生した為、入力可能文字数を40→39に修正 %>
        <td colspan="3"><%=Html.TextBox("LastNameKana", Model.LastNameKana, new { size = 50, maxlength = 39 })%></td>
    </tr>
    <tr>
        <th>郵便番号</th>
        <td colspan="3"><%=Html.TextBox("PostCode", Model.PostCode, new { @class = "alphanumeric", maxlength = 8 })%> <input type="button" id="SearchPostCode" name="SearchPostCode" value="郵便番号検索" onclick="getAddressFromPostCode()" /></td>
    </tr>
    <tr>
        <th>都道府県</th>
        <td colspan="3"><%=Html.TextBox("Prefecture", Model.Prefecture, new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>市区町村</th>
        <td colspan="3"><%=Html.TextBox("City", Model.City, new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>番地</th>
        <td colspan="3"><%=Html.TextBox("Address1", Model.Address1, new { size = 50, maxlength = 100 })%></td>
    </tr>
    <tr>
        <th>建物名・部屋番号</th>
        <td colspan="3"><%=Html.TextBox("Address2", Model.Address2, new { size = 50, maxlength = 100 })%></td>
    </tr>
    <% //Add 20115/03/06 arc iijima 基本情報にDM発送先の登録有無の追加 %>
    <tr>
        <th></th>
        <td colspan="3"><b><span id="DMEnabledmessage" style ="width:160px;color:#ff0000;"><%=Html.Encode(ViewData["DMEnabledmessage"]) %></span></b></td>
    </tr>

    <% //Add 2014/08/19 arc amii DMフラグ機能拡張対応 #3069 住所再確認チェックボックスを追加  %>
    <tr>
          <th></th>
         <td colspan="3">
             <% // Mod 2014/10/17 arc amii DM送付文言修正対応 #3116 項目名を変更 %>
             <%=Html.CheckBox("AddressReconfirm",Model.AddressReconfirm!=null && Model.AddressReconfirm == true ? true : false) %>住所要確認
            
         </td>
     </tr>
    <tr>
        <th>電話番号</th>
        <td><%=Html.TextBox("TelNumber", Model.TelNumber, new { @class = "alphanumeric", maxlength = 15 })%></td>
        <th style="width:150px">FAX番号</th>
        <td><%=Html.TextBox("FaxNumber", Model.FaxNumber, new { @class = "alphanumeric", maxlength = 15 })%></td>
    </tr>
    <tr>
        <th>初回来店日</th>
        <td><%=Html.TextBox("FirstReceiptionDate", string.Format("{0:yyyy/MM/dd}",Model.FirstReceiptionDate), new { @class = "alphanumeric", maxlength = 10 })%></td>
        <th>前回来店日</th>
        <td><%=Html.TextBox("LastReceiptionDate", string.Format("{0:yyyy/MM/dd}",Model.LastReceiptionDate), new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>備考</th>
        <% // Mod 2014/07/22 arc amii chromeでDB登録する際、改行コードも文字として登録してしまうのを修正 %>
        <td colspan="3" ><%=Html.TextArea("Memo", Model.Memo, 3, 50, new { wrap = "virtual", onblur = "checkTextLength('Memo', 200, '備考')" })%></td>
    </tr>
    <tr>
    <th>ステータス</th>
        <td colspan="3">
            <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
          <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
            {%>
            <%=Html.RadioButton("DelFlag", "0")%>有効<%=Html.RadioButton("DelFlag", "1")%>無効
          <%}
            else
            {%>
            <%=Html.RadioButton("DelFlag", "0", true)%>有効<%=Html.RadioButton("DelFlag", "1", new { disabled = true })%>無効
          <%} %>
        </td>
    </tr> 
</table>                            

