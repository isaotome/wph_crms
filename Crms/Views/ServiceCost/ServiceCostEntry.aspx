<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	サービス工数表設定
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
    CrmsLinqDataContext db = new CrmsLinqDataContext();    
%>
<%using(Html.BeginForm()){ %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<%=Html.Hidden("close", ViewData["close"]) %>
<br />
<%=Html.ValidationSummary()%>
<br />
<div style="text-align:left">&nbsp;&nbsp;(単位：h)</div>
<div id="input-form"  style="overflow-x:auto;overflow-y:auto;width:1000px;height:720px;">
<table class="input-form">
<tr>
    <th></th>
    <%List<CarClass> carClassList = (List<CarClass>)ViewData["CarClassList"];
      for (int i = 0; i < carClassList.Count; i++)
      { %>
    <% // Mod 2014/07/11 arc amii chrome対応 spanタグ追加し、style属性にchrome & IEで縦書き表示になるよう修正  %>
    <th style="text-align:center;vertical-align:top"><span style="-webkit-text-orientation:upright;-webkit-writing-mode:vertical-rl;writing-mode:tb-rl;direction: ltr"><%=Html.Encode(carClassList[i].CarClassName)%></span></th>
    <%} %>
</tr>
<%List<ServiceMenu> serviceMenuList = (List<ServiceMenu>)ViewData["ServiceMenuList"];
  Dictionary<string, string> serviceCostDic = (Dictionary<string, string>)ViewData["ServiceCostDic"];
  List<string> errorItemList = (List<string>)ViewData["ErrorItemList"];
  for (int j = 0; j < serviceMenuList.Count; j++)
  {%>
<tr>
    <th><%=Html.Encode(serviceMenuList[j].ServiceMenuName)%></th>
    <%string joinedKey, itemID, styleClass, cost;
      for (int k = 0; k < carClassList.Count; k++)
      {
          joinedKey = serviceMenuList[j].ServiceMenuCode + "_" + carClassList[k].CarClassCode;
          itemID = "Cost_" + joinedKey;
          if (errorItemList.Contains(itemID))
          {
              styleClass = "numeric input-validation-error";
          }
          else
          {
              styleClass = "numeric";
          }
          try
          {
              cost = CommonUtils.DefaultString(serviceCostDic[joinedKey], "0.00");
          }
          catch (KeyNotFoundException)
          {
              cost = "0.00";
          }%>
    <td><%=Html.TextBox(itemID, cost, new { @class = styleClass, size = "3" })%></td>
    <%} %>
</tr>
<%} %>    
</table>
<%} %>
</div>
<br />
</asp:Content>
