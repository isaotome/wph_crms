<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CostArea>" %>
<%@ Import Namespace="CrmsDao" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    諸費用設定エリア入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% 
   // -----------------------------------------------------------------------------------------------------
   // Mod 2023/08/15 yano #4176 販売諸費用の修正
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とするOutJurisdictionRegistFee
   // ----------------------------------------------------------------------------------------------------- 
%>
<%using(Html.BeginForm("Entry","CostArea",FormMethod.Post)){ %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <%// Mod 2022/01/27 yano #4126 %>
       <%if (ViewData["ButtonVisible"] != null && (bool)ViewData["ButtonVisible"].Equals(true))
         { %>
            <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
        <%} %>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.ValidationSummary()%>
<br />
<table class="input-form" style="width:700px">
    <tr>
        <th style="width:150px">諸費用設定エリアコード *</th>
        <td>
            <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
            <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %>
                <%=Html.TextBox("CostAreaCode",Model.CostAreaCode,new {@class="alphanumeric",size="10",maxlength="3",@readonly="readonly"}) %>
            <%}else{ %>
                <%=Html.TextBox("CostAreaCode",Model.CostAreaCode,new {@class="alphanumeric",size="10",maxlength="3",onchange="IsExistCode('CostAreaCode','CostArea')"}) %>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>諸費用設定エリア名 *</th>
        <td><%=Html.TextBox("CostAreaName",Model.CostAreaName,new {size="50",maxlength="50"}) %></td>
    </tr>
</table>
<br />
<table class="input-form" style="width:700px">
    <tr>
        <th colspan="2" class="input-form-title">非課税項目</th>
    </tr>
    <tr><%//Add 2020/01/07 yano #4029%>
        <th style="width:150px">車庫証明証紙代</th>
        <td><%=Html.TextBox("ParkingSpaceCost", string.Format("{0:N0}", Model.ParkingSpaceCost),new {@class="money",size="10",maxlength="10"}) %></td>
    </tr> 
    <tr><%//Add 2020/01/07 yano #4029%>
        <th style="width:150px">ﾅﾝﾊﾞｰﾌﾟﾚｰﾄ代(一般)</th>
        <td><%=Html.TextBox("NumberPlateCost", string.Format("{0:N0}", Model.NumberPlateCost),new {@class="money",size="10",maxlength="10"}) %></td>
    </tr> 
    <tr>
        <th style="width:150px">ﾅﾝﾊﾞｰﾌﾟﾚｰﾄ代(希望)</th>
        <td><%=Html.TextBox("RequestNumberCost", string.Format("{0:N0}", Model.RequestNumberCost),new {@class="money",size="10",maxlength="10"}) %></td>
    </tr>
</table>
<br />
<table class="input-form" style="width:700px">
    <tr>
        <th colspan="4" class="input-form-title">課税項目(税込金額)</th>
    </tr>
    <%--Mod 2023/10/05 yano #4184 販売諸費用の[中古車点検・整備費用][中古車継承整備費用]の削除に伴うレイアウト変更--%>
    <%--Mod 2023/08/15 yano #4176 課税項目追加に伴いレイアウト・カラム名変更変更--%>
    <tr>
        <th style="width:150px">検査登録手続代行費用</th>
        <td><%=Html.TextBox("InspectionRegistFeeWithTax",  string.Format("{0:N0}", Model.InspectionRegistFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
        <th style="width:150px">希望ナンバー申請手数料</th>
        <td><%=Html.TextBox("RequestNumberFeeWithTax",  string.Format("{0:N0}", Model.RequestNumberFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
    </tr>
    <tr>
        <th style="width:150px">納車費用</th>
        <td><%=Html.TextBox("PreparationFeeWithTax",  string.Format("{0:N0}", Model.PreparationFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
        <th style="width:150px">下取車所有権解除手続費用</th>
        <td><%=Html.TextBox("TradeInFeeWithTax",  string.Format("{0:N0}", Model.TradeInFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
    </tr>
    <tr>
        <th style="width:150px">管轄外登録手続費用</th>
        <td><%=Html.TextBox("OutJurisdictionRegistFeeWithTax",  string.Format("{0:N0}", Model.OutJurisdictionRegistFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
        <th style="width:150px">県外登録手続代行費用</th>
        <td><%=Html.TextBox("FarRegistFeeWithTax",  string.Format("{0:N0}", Model.FarRegistFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
    </tr>
    <tr>
        <th style="width:150px">車庫証明手続代行費用</th>
        <td><%=Html.TextBox("ParkingSpaceFeeWithTax", string.Format("{0:N0}", Model.ParkingSpaceFeeWithTax),new {@class="money",size="10",maxlength="10"}) %></td><%//Mod 2020/01/07 yano #4029 列名の変更%>
        <th style="width:150px">リサイクル資金管理料</th>
        <td><%=Html.TextBox("RecycleControlFeeWithTax", string.Format("{0:N0}", Model.RecycleControlFeeWithTax),new {@class="money",size="10",maxlength="10"}) %></td>
    </tr>
   
    <!--Add 2017/02/08 arc nakayama #3019_基本マスタの諸費設定にある、車庫証明手続き代行費用の記述間違い-->

    <%-- Mod 2023/10/05 yano #4184 --%>
    <%--<tr>
        <th style="width:150px">検査登録手続代行費用</th>
        <td><%=Html.TextBox("InspectionRegistFeeWithTax",  string.Format("{0:N0}", Model.InspectionRegistFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
        <th style="width:150px">希望ナンバー申請手数料</th>
        <td><%=Html.TextBox("RequestNumberFeeWithTax",  string.Format("{0:N0}", Model.RequestNumberFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
    </tr>
    <tr>
        <th style="width:150px">納車費用</th>
        <td><%=Html.TextBox("PreparationFeeWithTax",  string.Format("{0:N0}", Model.PreparationFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
        <th style="width:150px">中古車点検・整備費用</th>
        <td><%=Html.TextBox("TradeInMaintenanceFeeWithTax",  string.Format("{0:N0}", Model.TradeInMaintenanceFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
    </tr>
    <tr>
        <th style="width:150px">管轄外登録手続費用</th>
        <td><%=Html.TextBox("OutJurisdictionRegistFeeWithTax",  string.Format("{0:N0}", Model.OutJurisdictionRegistFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
        <th style="width:150px">下取車所有権解除手続費用</th>
        <td><%=Html.TextBox("TradeInFeeWithTax",  string.Format("{0:N0}", Model.TradeInFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>

    </tr>
    <tr>
        <th style="width:150px">県外登録手続代行費用</th>
        <td><%=Html.TextBox("FarRegistFeeWithTax",  string.Format("{0:N0}", Model.FarRegistFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
        <th style="width:150px">リサイクル資金管理料</th>
        <td><%=Html.TextBox("RecycleControlFeeWithTax", string.Format("{0:N0}", Model.RecycleControlFeeWithTax),new {@class="money",size="10",maxlength="10"}) %></td>
    </tr>
    <tr>
        <th style="width:150px">車庫証明手続代行費用</th>
        <td><%=Html.TextBox("ParkingSpaceFeeWithTax", string.Format("{0:N0}", Model.ParkingSpaceFeeWithTax),new {@class="money",size="10",maxlength="10"}) %></td><%//Mod 2020/01/07 yano #4029 列名の変更%>
        <th style="width:150px">中古車継承整備費用</th>
        <td><%=Html.TextBox("InheritedInsuranceFeeWithTax",  string.Format("{0:N0}", Model.InheritedInsuranceFeeWithTax), new { @class = "money", size = "10", maxlength = "10" })%></td>
    </tr>--%>

</table>
<br />
<table class="input-form" style="width:700px">
    <tr>
        <th style="width:150px">ステータス</th>
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
