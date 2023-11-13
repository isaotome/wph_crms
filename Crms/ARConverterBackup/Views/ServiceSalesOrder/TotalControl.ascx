<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.ServiceSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<% 
   // -----------------------------------------------------------------------------------------------------
   // Add 2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
   // Mod 2018/03/26 arc yano #3817 サービス伝票 支払金額欄表示変更 
   //                               合計欄の変更(請求額合計→請求額、支払合計→入金額、入金実績→残金) 
   //                               合わせてロジックも変更
   // Mod 2017/04/23 arc yano #3755 車両伝票入力　金額欄の入力時のカーソル位置の不具合
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>
<br />
<table class="input-form-slim" style="float:left">
    <tr>
        <th class="input-form-title" colspan="6">合計</th>
    </tr>
    <tr>
        <th></th>
        <th>税抜</th>
        <th>消費税</th>
        <th>税込</th>
        <th>原価</th>
        <th>粗利</th>
    </tr>
    <tr>
        <th>技術料小計</th>
        <td><%=Html.TextBox("EngineerTotalAmount", string.Format("{0:N0}",  Model.EngineerTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("EngineerTotalTaxAmount", string.Format("{0:N0}",  Model.ServiceSalesLine.Where(x => x.ServiceType == "002").Sum(x => !string.IsNullOrEmpty(x.ServiceMenuCode) && x.ServiceMenuCode.Length >= 6 && x.ServiceMenuCode.Substring(0, 6).Equals("DISCNT") ? -1 * x.TaxAmount : x.TaxAmount)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("EngineerTotalAmountWithTax", string.Format("{0:N0}", Model.EngineerTotalAmount + Model.ServiceSalesLine.Where(x => x.ServiceType == "002").Sum(x => !string.IsNullOrEmpty(x.ServiceMenuCode) && x.ServiceMenuCode.Length >= 6 && x.ServiceMenuCode.Substring(0, 6).Equals("DISCNT") ? (-1) * x.TaxAmount : x.TaxAmount)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("EngineerTotalCost", string.Format("{0:N0}", Model.ServiceSalesLine.Where(x => x.ServiceType == "002").Sum(x => x.Cost)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("EngineerGrossSalesAmount", string.Format("{0:N0}", Model.EngineerTotalAmount - Model.ServiceSalesLine.Where(x => x.ServiceType == "002").Sum(x => x.Cost)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
    </tr>
    <tr>
        <th>部品小計</th>
        <td><%=Html.TextBox("PartsTotalAmount", string.Format("{0:N0}", Model.PartsTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("PartsTotalTaxAmount", string.Format("{0:N0}", Model.ServiceSalesLine.Where(x => x.ServiceType == "003").Sum(x => !string.IsNullOrEmpty(x.PartsNumber) && x.PartsNumber.Length >= 6 && x.PartsNumber.Substring(0, 6).Equals("DISCNT") ? (-1) * x.TaxAmount : x.TaxAmount)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("PartsTotalAmountWithTax", string.Format("{0:N0}", Model.PartsTotalAmount + Model.ServiceSalesLine.Where(x => x.ServiceType == "003").Sum(x => !string.IsNullOrEmpty(x.PartsNumber) && x.PartsNumber.Length >= 6 && x.PartsNumber.Substring(0, 6).Equals("DISCNT") ? (-1) * x.TaxAmount : x.TaxAmount)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("PartsTotalCost", string.Format("{0:N0}", Model.ServiceSalesLine.Where(x => x.ServiceType == "003").Sum(x => x.Cost)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("PartsGrossSalesAmount", string.Format("{0:N0}", Model.PartsTotalAmount - Model.ServiceSalesLine.Where(x => x.ServiceType == "003").Sum(x => x.Cost)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
    </tr>
    <tr>
        <th>諸費用小計</th>
        <td><%=Html.TextBox("CostSubTotalAmount", string.Format("{0:N0}", Model.CostTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>    
        <td><%=Html.TextBox("CostSubTotalTaxAmount", 0, new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("CostTotalAmount", string.Format("{0:N0}", Model.CostTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("CostSubTotalCost", 0, new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("CostGrossSalesAmount", 0, new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
    </tr>
    <%//Add 2020/02/17 yano #4025 %>
    <tr>
        <th>諸費用(課税)小計</th>
        <td><%=Html.TextBox("TaxableCostSubTotalAmount", string.Format("{0:N0}", Model.TaxableCostSubTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>    
        <td><%=Html.TextBox("TaxableCostSubTotalTaxAmount", Model.TaxableCostSubTotalTaxAmount, new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("TaxableCostTotalAmount", string.Format("{0:N0}", Model.TaxableCostTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("TaxableCostSubTotalCost", 0, new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("TaxableCostGrossSalesAmount", 0, new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
    </tr>
    <tr>
        <th>合計</th>
        <td><%=Html.TextBox("TotalAmountWithoutTax", string.Format("{0:N0}", Model.EngineerTotalAmount + Model.PartsTotalAmount + Model.CostTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("TotalTaxAmount", string.Format("{0:N0}", Model.TotalTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("TotalAmountWithTax", string.Format("{0:N0}", Model.EngineerTotalAmount + Model.PartsTotalAmount + Model.CostTotalAmount + Model.TotalTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("TotalCost", string.Format("{0:N0}", Model.ServiceSalesLine.Where(x=>x.ServiceType=="002" || x.ServiceType=="003").Sum(x => x.Cost)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
        <td><%=Html.TextBox("TotalGrossSalesAmount", string.Format("{0:N0}", (Model.EngineerTotalAmount + Model.PartsTotalAmount) - (Model.ServiceSalesLine.Where(x => x.ServiceType == "002" || x.ServiceType == "003").Sum(x => x.Cost))), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%></td>
    </tr>
</table>
<table class="input-form-slim" style="float:left;margin-left:20px">
    <tr>
        <th colspan="3" class="input-form-title">支払</th>
    </tr>
    <tr>
        <%// Mod 2018/03/26 arc yano #3817 %>
        <th>請求額合計</th>
        <th>入金額合計</th>
        <th>請求残高</th>
    </tr>
    <tr>
        <td style="width:60px"><%=Html.TextBox("GrandTotalAmount", string.Format("{0:N0}", Model.GrandTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:60px" })%></td>
        <td style="width:60px"><%=Html.TextBox("JournalTotalAmount", string.Format("{0:N0}", ViewData["JournalTotalAmount"]), new { @class = "readonly money", @readonly = "readonly", style = "width:60px" })%></td>
        <td style="width:60px"><%=Html.TextBox("ReceivableBalance", string.Format("{0:N0}", (Model.GrandTotalAmount ?? 0) - decimal.Parse(ViewData["JournalTotalAmount"] != null ? ViewData["JournalTotalAmount"].ToString() : "0")), new { @class = "readonly money", @readonly = "readonly", style = "width:60px" })%></td>
        <%=Html.Hidden("PaymentTotalAmount", string.Format("{0:N0}", Model.PaymentTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:60px" })%>


        <%/*
        <td style="width:60px"><%=Html.TextBox("PaymentTotalAmount", string.Format("{0:N0}", Model.PaymentTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:60px" })</td>
        <td style="width:60px"><%=Html.TextBox("JournalTotalAmount", string.Format("{0:N0}", ViewData["JournalTotalAmount"]), new { @class = "readonly money", @readonly = "readonly", style = "width:60px" })</td>
        */%>
    </tr>
</table>
<table class="input-form-slim" style="float:left;margin-left:20px">
    <tr>
        <th class="input-form-title" style="width:340px">引き継ぎメモ</th>
    </tr>
    <tr>
        <td style="width:340px"><%=Html.TextArea("Memo", Model.Memo, 4, 30, new { style = "height:100px;width:340px; resize:none; overflow-x:hidden; overflow-y:auto;" })%></td>
    </tr>
</table>
<%=Html.Hidden("SubTotalAmount", Model.SubTotalAmount, new { @class = "money" })%>
<%=Html.Hidden("TotalTaxAmount",Model.TotalTaxAmount, new { @class = "money" }) %>
<%=Html.Hidden("ServiceTotalAmount",Model.ServiceTotalAmount, new { @class = "money" }) %>
<!--2014/03/19.ookubo #3006] 【サ】納車済みで数字が変わる対応 -->
<script type="text/javascript">calcTotalServiceAmount();</script>
