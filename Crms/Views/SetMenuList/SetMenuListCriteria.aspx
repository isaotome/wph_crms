<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.SetMenuList>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	セットメニュー検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "SetMenuList", new { id = 0 }, FormMethod.Post)) { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("action", "") %>
<%=Html.Hidden("DelLine","") %>
<%=Html.Hidden("SearchedServiceWorkCode", ViewData["ServiceWorkCode"])%>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form" style="display:block">
<br />
    <% // Mod 2014/07/15 arc amii chrome対応 style属性の「white-space:nowrap」が機能していなかったので記述を削除 %>
<table class="input-form">
    <tr>
        <th rowspan="2" style="width:100px">セットメニュー</th>
        <td><%=Html.TextBox("SetMenuCode", ViewData["SetMenuCode"], new { @class = "alphanumeric", maxlength = 11, onchange = "GetNameFromCode('SetMenuCode','SetMenuName','SetMenu')" })%>
            <img alt="セットメニュー検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SetMenuCode', 'SetMenuName', '/SetMenu/CriteriaDialog')" />
        </td>
    </tr>
    <tr>
        <td><span id="SetMenuName"><%=CommonUtils.DefaultNbsp(ViewData["SetMenuName"])%></span></td>       
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="displaySearchList()"/></td>
    </tr>
</table>
</div>
<br />
<%=Html.ValidationSummary()%>
<br />
<%if (!string.IsNullOrEmpty(CommonUtils.DefaultString(ViewData["SetMenuCode"]))) { %>
<table class="input-form">
    <tr>
        <th style="width:15px;text-align:center"><img alt="行追加" src="/Content/Images/plus.gif" style="cursor:pointer" onclick="document.forms[0].action.value='line';$('#DelLine').val('-1');formSubmit();" /></th>
        <th style="width:100px">種類 *</th>
        <% //  Mod 2014/07/15 arc amii chrome対応 部品検索でフル桁データを表示するとレイアウトが崩れるのを修正 %>
        <th style="width:620px">サービスメニュー/部品 *</th>
        <th style="width:80px">区分</th>
        <th style="width:50px">数量</th>
        <th style="width:60px">金額自動</th>
        <th style="width:50px">表示順 *</th>
    </tr>
</table>
<% // Mod 2014/07/15 arc amii chrome対応 部品検索でフル桁データを表示するとレイアウトが崩れるのを修正 %>
<div style="overflow-y:scroll;width:1055px;height:350px">
<table class="input-form" style="width:1000px;table-layout:fixed">
<%for (int i = 0; i < Model.Count; i++)
  {
      string namePrefix = string.Format("line[{0}].", i);
      string namePrefix2 = string.Format("line[{0}]_", i);%>
    <tr>
        <td style="width:15px;text-align:center"><%=Html.Hidden(namePrefix + "SetMenuCode", Model[i].SetMenuCode, new { id = namePrefix2 + "SetMenuCode"})%><%=Html.Hidden(namePrefix + "LineNumber", i + 1, new { id = namePrefix2 + "LineNumber"})%><img alt="行削除" src="/Content/Images/minus.gif" style="cursor:pointer" onclick="document.forms[0].action.value='line';$('#DelLine').val('<%=i %>');formSubmit();" /></td>
        <td style="width:100px"><%=Html.DropDownList(namePrefix + "ServiceType", ((List<IEnumerable<SelectListItem>>)ViewData["ServiceTypeList"])[i], new { id = namePrefix2 + "ServiceType", onchange = "changeSetMenuDetailsType(" + CommonUtils.IntToStr(i) + ")" })%></td>
        <% // Mod 2014/07/15 arc amii chrome対応 部品検索でフル桁データを表示するとレイアウトが崩れるのを修正 %>
        <td style="width:620px;">
            <%
              string dispWork = "none";
              string dispService = "none";
              string dispParts = "none";
              string dispComment = "none";
              switch(CommonUtils.DefaultString(Model[i].ServiceType)){
                  case "001" :
                    dispWork = "inline";
                      break;
                  case "002" : 
                    dispService = "inline";  
                      break;
                  case "003":
                      dispParts = "inline";
                      break;
                  case "004":
                      dispComment = "inline";
                      break;
              }
               
              %>
            <% // <!--//2014/05/29 vs2012対応 arc yano 各コントロールにidを追加 %>
                  <% // Mod 2014/07/10 arc amii chrome対応 spanタグのidが他タグと違っていたので、統一するよう修正 %>
            <span id="<%=namePrefix2 %>InputWork" style="display:<%=dispWork%>">
                <%//2014/07/16 arc yano chrome対応 パラメータをname→idに変更 %>
                <%=Html.TextBox(namePrefix + "ServiceWorkCode", Model[i].ServiceWorkCode, new { id = namePrefix2 + "ServiceWorkCode", @class = "alphanumeric", maxlength = 5, onblur = "GetNameFromCode('" + namePrefix2 + "ServiceWorkCode','" + namePrefix2 + "ServiceWorkName', 'ServiceWork')" })%>
                <img alt="主作業検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('<%=namePrefix2 + "ServiceWorkCode" %>','<%=namePrefix2 + "ServiceWorkName" %>','/ServiceWork/CriteriaDialog')" />
                
                <span id="<%=namePrefix2 + "ServiceWorkName" %>"><%=Html.Encode(((List<string>)ViewData["ServiceWorkNameList"])[i]) %></span>
            </span>
            <span id="<%=namePrefix2 %>InputService" style="display:<%=dispService%>">
                <%//2014/07/16 arc yano chrome対応 パラメータをname→idに変更 %>
                <%=Html.TextBox(namePrefix + "ServiceMenuCode", Model[i].ServiceMenuCode, new { id = namePrefix2 + "ServiceMenuCode", @class = "alphanumeric", maxlength = 8, onblur = "GetNameFromCode('" + namePrefix2 + "ServiceMenuCode','" + namePrefix2 + "ServiceMenuName', 'ServiceMenu')" })%>

                <img alt="サービスメニュー検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('<%=namePrefix2 + "ServiceMenuCode" %>','<%=namePrefix2 + "ServiceMenuName" %>','/ServiceMenu/CriteriaDialog')" />
               
                 <span id="<%=namePrefix2 + "ServiceMenuName" %>"><%=Html.Encode(((List<string>)ViewData["ServiceMenuNameList"])[i]) %></span>
            </span>
            <span id="<%=namePrefix2 %>InputParts" style="display:<%=dispParts%>">
                <%//2014/07/16 arc yano chrome対応 パラメータをname→idに変更 %>
                <%=Html.TextBox(namePrefix + "PartsNumber", Model[i].PartsNumber, new { id = namePrefix2 + "PartsNumber", @class = "alphanumeric", maxlength = 25, onblur = "GetNameFromCode('" + namePrefix2 + "PartsNumber','" + namePrefix2 + "PartsNameJp', 'Parts')" })%>
                <img alt="部品検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('<%=namePrefix2 + "PartsNumber" %>','<%=namePrefix2 + "PartsNameJp" %>','/Parts/CriteriaDialog')" />
                
                <span id="<%=namePrefix2 + "PartsNameJp" %>"><%=Html.Encode(((List<string>)ViewData["PartsNameJpList"])[i]) %></span>
            </span>
            <span id="<%=namePrefix2 %>InputComment" style="display:<%=dispComment%>">
                <%=Html.TextBox(namePrefix + "Comment", Model[i].Comment, new { id = namePrefix2 + "Comment", size="40",maxlength = 25})%>
            </span>
        </td>

        <td style="width:80px">
            <%=Html.DropDownList(namePrefix + "WorkType", ((List<IEnumerable<SelectListItem>>)ViewData["WorkTypeList"])[i], new { id = namePrefix2 + "WorkType" })%>
        </td>

        <td style="width:50px">
            <span id="<%=namePrefix2 %>InputQuantity" style="display:<%=dispParts%>">
                <%=Html.TextBox(namePrefix + "Quantity",Model[i].Quantity,new { id = namePrefix2 + "Quantity", @class="numeric",size="1",maxlength="8"}) %>
            </span>
        </td>
        <td style="width:60px"><%=Html.DropDownList(namePrefix + "AutoSetAmount", ((List<IEnumerable<SelectListItem>>)ViewData["AutoSetAmountList"])[i], new { id = namePrefix2 + "AutoSetAmount"})%></td>
        <td style="width:50px"><%=Html.TextBox(namePrefix + "InputDetailsNumber", Model[i].InputDetailsNumber, new { id = namePrefix2 + "InputDetailsNumber", @class = "numeric", size = 1, maxlength = 9 })%></td>
   </tr>
<%} %>
</table>
</div>
<br />
<input type="button" value="登録" onclick="document.forms[0].action.value='regist';displaySearchList()" />
<%} %>
<br />
<%} %>
<br />
</asp:Content>
