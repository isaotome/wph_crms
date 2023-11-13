<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarOption>" %>
<%@ Import Namespace="CrmsDao" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	オプションマスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "CarOption", FormMethod.Post))
      { %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("close", ViewData["close"]) %>
<div id="input-form">
    <%=Html.ValidationSummary()%>
    <br />
    <table class="input-form" style="width:700px">
      <tr>
        <th style="width:100px">オプションコード *</th>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="CarOptionCode" name="CarOptionCode" value="<%=Model.CarOptionCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("CarOptionCode", Model.CarOptionCode, new { @class = "alphanumeric", maxlength = 25, onblur = "IsExistCode('CarOptionCode','CarOption')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>オプション名 *</th>
        <td><%=Html.TextBox("CarOptionName", Model.CarOptionName, new { size = 50, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th rowspan="2">メーカー *</th>
        <td><%=Html.TextBox("MakerCode", Model.MakerCode, new { @class = "alphanumeric", maxlength = 5, onblur = "GetNameFromCode('MakerCode','MakerName','Maker')" })%>
            <img alt="メーカー検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('MakerCode', 'MakerName', '/Maker/CriteriaDialog')" />
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="MakerName"><%=Html.Encode(ViewData["MakerName"])%></span></td>       
      </tr>
      <tr>
        <th rowspan="2">グレードコード</th>
        <td><%=Html.TextBox("CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric", size = 30, maxlength = 30, onblur = "GetNameFromCode('CarGradeCode','CarGradeName','CarGrade');CheckCarCode();" })%>&nbsp;<img alt="車種検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarGradeCode', 'CarGradeName', '/CarGrade/CriteriaDialog')" /><span id="CarGradeName" style ="width:160px"><%=Html.Encode(Model.CarGradeName) %></span></td>
      </tr>
      <tr>
          <td>グレードコードを空白で保存すると、メーカー共通のオプションになります。</td>
      </tr>
      <tr>
        <th>区分 * </th>
        <td>
        <%=Html.DropDownList("OptionType", (IEnumerable<SelectListItem>)ViewData["OptionTypeList"], new { id="OptionType", style="width:80px"})%>
        </td>
      </tr>
      <tr>
        <th>原価</th>
        <td><%=Html.TextBox("Cost", Model.Cost, new { @class = "numeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>販売価格</th>
        <td><%=Html.TextBox("SalesPrice", Model.SalesPrice, new { @class = "numeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th rowspan="3">必須設定 * </th>
        <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
        {%> 
            <td><%=Html.RadioButton("RequiredFlag", "1")%>必須<%=Html.RadioButton("RequiredFlag", "0")%>任意</td>
        <%}else{%>
            <td><%=Html.RadioButton("RequiredFlag", "1", false)%>必須<%=Html.RadioButton("RequiredFlag", "0", true)%>任意</td>
        <%}%>        
      </tr>
      <tr>
          <td>グレードコードを空白にすると、任意オプションになります。</td>
      </tr>
    　<tr>
          <td>車両伝票で新車を選択した場合、必須設定がされているオプションが自動で設定されます。</td>
      </tr>
      <tr>
        <th>ステータス</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
      <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
        {%>
        <td><%=Html.RadioButton("DelFlag", "0")%>有効<%=Html.RadioButton("DelFlag", "1")%>無効</td>
      <%}
        else
        {%>
        <td><%=Html.RadioButton("DelFlag", "0", true)%>有効<%=Html.RadioButton("DelFlag", "1", new { disabled = true })%>無効</td>
      <%} %>
      </tr>
    </table>
</div>
<%} %>
<br />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
<script type="text/javascript">
    $(document).ready(function (e) {
        CheckCarCode();
    });
</script>
</asp:Content>
