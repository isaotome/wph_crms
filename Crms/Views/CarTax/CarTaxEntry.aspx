<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarTax>" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    自動車税種別割入力   <%-- Mod 2019/09/04 yano #4011 --%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using(Html.BeginForm("Entry","CarTax",FormMethod.Post)){ %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <%//Mod 2022/01/27 yano #4125 %>
       <%if (ViewData["ButtonVisible"] != null && (bool)ViewData["ButtonVisible"].Equals(true))
         { %>
            <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
       <%} %>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.Hidden("CarTaxId",Model.CarTaxId) %>
<%=Html.ValidationSummary()%>
<br />
<table class="input-form" style="width:700px">
    <tr>
        <th style="width:100px">表示名 *</th>
        <td><%=Html.TextBox("CarTaxName",Model.CarTaxName,new {size="30",maxlength="100"}) %></td>
    </tr>
    <tr>
        <th>金額(円) *</th>
        <% // Mod 2014/07/14 arc amii 既存バグ対応 DBの桁数と入力可能文字数が一致していなかったので、入力可能文字数を11 → 10 に修正 %>
        <td><%=Html.TextBox("Amount",Model.Amount,new {@class="numeric",size="10",maxlength="10"}) %></td>
    </tr>
    <tr>
        <th>登録月 *</th>
        <td><%=Html.TextBox("RegistMonth",Model.RegistMonth,new {@class="numeric",size="1",maxlength="2"}) %></td>
    </tr>
    <tr>
        <th>総排気量FROM(cc)</th>
        <td><%=Html.TextBox("FromDisplacement",Model.FromDisplacement,new {@class="numeric",size="10",maxlength="4"}) %></td>
    </tr>
    <tr>
        <th>総排気量TO(cc)</th>
        <td><%=Html.TextBox("ToDisplacement",Model.ToDisplacement,new {@class="numeric",size="10",maxlength="4"}) %></td>
    </tr>
    <%//Add 2019/10/21 yano #4023 【車両伝票入力】中古車の自動車種別割の計算の誤り%>
     <tr>
        <th>適用日FROM *</th>
        <td><%=Html.TextBox("FromAvailableDate", Model.FromAvailableDate != DateTime.Parse("0001/01/01") ? string.Format("{0:yyyy/MM/dd}", Model.FromAvailableDate) : "", new { @class = "alphanumeric", style = "width:112px", maxlength = "10" })%></td>
    </tr>
     <tr>
        <th>適用日TO *</th>
        <td><%=Html.TextBox("ToAvailableDate", Model.ToAvailableDate != DateTime.Parse("0001/01/01")  ? string.Format("{0:yyyy/MM/dd}", Model.ToAvailableDate) : "", new { @class = "alphanumeric", style = "width:112px", maxlength = "10" })%></td>
    </tr>
    
    <tr>
        <th>ステータス</th>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
            {%>
                <%=Html.RadioButton("DelFlag", "0")%>有効<%=Html.RadioButton("DelFlag", "1")%>無効
            <%} else {%>
                <%=Html.RadioButton("DelFlag", "0", true)%>有効<%=Html.RadioButton("DelFlag", "1", new { disabled = true })%>無効
            <%} %>
        </td>
  </tr>
</table>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
