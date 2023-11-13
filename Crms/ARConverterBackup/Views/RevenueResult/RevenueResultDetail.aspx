<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Customer>" %>


<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	入金実績詳細
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Detail", "RevenueResult", FormMethod.Post))
  { %>
<%=Html.Hidden("CustomerCode", ViewData["CustomerCode"]) %>
<%=Html.Hidden("SlipNumber", ViewData["SlipNumber"]) %>
<%=Html.Hidden("indexid", ViewData["indexid"])%>
<table class="command">
   <tr>
       <td onclick=" window.close();"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td> 
   </tr>
</table>
<br />
<%=Html.ValidationSummary() %>
<br />

<% // --------顧客情報--------%>
<table class="input-form" style="width:50%">
     <tr>
        <th class="input-form-title" colspan="2">
            顧客情報
        </th>
    </tr>
      <tr>
        <th width="100px">
            顧客名
        </th>
        <td>
            <%=Html.Encode(@Model != null ? Model.CustomerName : "") %>
        </td>
    </tr>
    <tr>
        <th width="100px">
            住所
        </th>
        <td>
            <%=Html.Encode(@Model != null ? Model.PostCode : "")%><br/><%=Html.Encode(@Model != null ? Model.Prefecture + Model.City + Model.Address1 + Model.Address2 : "") %>
        </td>
    </tr>
    <tr>
        <th width="100px">
            連絡先
        </th>
        <td>
            <%=Html.Encode(@Model != null ? Model.TelNumber : "") %>
        </td>
    </tr>
</table>
<br/>
<% // --------車両情報--------%>
<%Html.RenderAction("CarInformation", "RevenueResult", new { customerCode = ViewData["CustomerCode"].ToString(), slipNumber = (ViewData["SlipNumber"] != null ? ViewData["SlipNumber"].ToString() : "") });%>
<br/>
<table>
    <tr>
        <td>
            <input type="button" value="▼請求先別残高" onclick="document.getElementById('indexid').value = 0; changeDisplayReceipt('SumPlan');" />
            <input type="button" value="▼入金実績" onclick="document.getElementById('indexid').value = 1; changeDisplayReceipt('Result');" />
            <input type="button" value="▼入金履歴" onclick="document.getElementById('indexid').value = 2; changeDisplayReceipt('History');" />
            <input type="button" value="▼カード入金実績" onclick="document.getElementById('indexid').value = 3; changeDisplayReceipt('ResultCard');" />
        </td>
    </tr>
</table>
<!--Add 2016/07/19 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）-->
<br />
<div id="ReceiptRadio"; style="visibility:hidden">
        <%=Html.RadioButton("ReceiptTargetFlag", "1", ViewData["ReceiptTargetFlag"] != null && ViewData["ReceiptTargetFlag"].ToString().Equals("1"), new { onclick = "document.getElementById('indexid').value = 4; changeDisplayReceipt('SumPlan')"})%>請求先の合計<%=Html.RadioButton("ReceiptTargetFlag", "0", ViewData["ReceiptTargetFlag"] != null && ViewData["ReceiptTargetFlag"].ToString().Equals("0"), new { onclick = "document.getElementById('indexid').value = 2; changeDisplayReceipt('Plan');"})%>明細
</div>
<br />
<% // --------入金予定(明細表示)-------%>
<div id="Plan" style="<%=!(bool)ViewData["ReceiptPlanListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("ReceiptPlanList", "RevenueResult", new { customerCode = ViewData["CustomerCode"].ToString(), slipNumber = (ViewData["SlipNumber"] != null ? ViewData["SlipNumber"].ToString(): "")});%>
</div>
<!--Add 2016/07/19 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）-->
<% // --------入金予定(サマリ表示)-------%>
<div id="SumPlan" style="<%=!(bool)ViewData["SumReceiptPlanListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("SumReceiptPlanList", "RevenueResult", new { customerCode = ViewData["CustomerCode"].ToString(), slipNumber = (ViewData["SlipNumber"] != null ? ViewData["SlipNumber"].ToString() : "") });%>
</div>
<% // --------入金実績-------%>
<div id="Result" style="<%=!(bool)ViewData["ReceiptResultListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("ReceiptResultList", "RevenueResult", new { customerCode = ViewData["CustomerCode"].ToString(), slipNumber = (ViewData["SlipNumber"] != null ? ViewData["SlipNumber"].ToString() : "") });%>
</div>
<% // --------入金履歴-------%>
<div id="History" style="<%=!(bool)ViewData["ReceiptHistoryListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("ReceiptHistoryList", "RevenueResult", new { customerCode = ViewData["CustomerCode"].ToString(), slipNumber = (ViewData["SlipNumber"] != null ? ViewData["SlipNumber"].ToString() : "") });%>
</div>
<% // --------カード入金実績-------%>
<div id="ResultCard" style="<%=!(bool)ViewData["ReceiptResultCardListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("ReceiptResultCardList", "RevenueResult", new { customerCode = ViewData["CustomerCode"].ToString(), slipNumber = (ViewData["SlipNumber"] != null ? ViewData["SlipNumber"].ToString() : "") });%>
</div>
<br/>
<br/>
<%} %>

</asp:Content>