<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<% 
   // -----------------------------------------------------------------------------------------------------
   // MOd 2022/06/10 yano #4139 【車両伝票入力】車両本体価格・編集不可の処理漏れの対応
   // Mod 2023/09/18 yano #4181【車両伝票】オプション区分追加（サーチャージ）
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>        

        <table class="input-form-slim">
            <tr>
                <th colspan="4" class="input-form-title">合計金額</th>
            </tr>
            <tr>
                <th style="width: 100px"></th>
                <th style="width: 96px">税抜</th>
                <th style="width: 96px">消費税</th>
                <th style="width: 96px">税込</th>
            </tr>
            <tr>
                <th>車両本体価格</th>
                <td>
                    <%if (Model.TotalEnabled) { %>
                        <%=Html.TextBox("SalesPrice", string.Format("{0:N0}", Model.SalesPrice), new { @class = "money", style = "width:96px", onchange = "calcSalesPrice(1)" })%>
                    <%}else{ %>
                        <%=Html.TextBox("SalesPrice", string.Format("{0:N0}", Model.SalesPrice), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%><%//Mod 2022/06/10 yano #4139%>
                    <%} %>
                </td>
                <td>
                    <%if (Model.TotalEnabled) { %>
                        <%=Html.TextBox("SalesTax", string.Format("{0:N0}", Model.SalesTax), new { @class = "money", maxlength = 10, style = "width:96px", onchange = "calcSalesPrice(2)" })%>    
                    <%} else { %>
                        <%=Html.TextBox("SalesTax", string.Format("{0:N0}", Model.SalesTax), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%><%//Mod 2022/06/10 yano #4139%>
                    <%} %>
                </td>
                <td>
                    <%if (Model.TotalEnabled) { %>
                        <%=Html.TextBox("SalesPriceWithTax", string.Format("{0:N0}", Model.SalesPrice + Model.SalesTax), new { @class = "money", maxlength = 10, style = "width:96px", onchange = "calcSalesPrice(3)" })%>
                    <%}else{ %>
                        <%=Html.TextBox("SalesPriceWithTax", string.Format("{0:N0}", Model.SalesPrice + Model.SalesTax), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%><%//Mod 2022/06/10 yano #4139%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>値引金額</th>
                <td>
                    <%=Html.TextBox("DiscountAmount", string.Format("{0:N0}", Model.DiscountAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%>
                </td>
                <td>
                    <%if(Model.TotalEnabled){ %>
                        <%=Html.TextBox("DiscountTax", string.Format("{0:N0}", Model.DiscountTax), new { @class = "money", maxlength = 10, style = "width:96px", onkeyup = "calcDiscountAmount(false)", onchange = "calcDiscountAmount(false)" })%><%//Mod 2022/06/08 yano #4137 %>
                        <%--<%=Html.TextBox("DiscountTax", string.Format("{0:N0}", Model.DiscountTax), new { @class = "money", maxlength = 10, style = "width:96px", onkeyup = "calcDiscountAmount(false)" })%>--%>
                    <%}else{ %>
                        <%=Html.TextBox("DiscountTax", string.Format("{0:N0}", Model.DiscountTax), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%>
                    <%} %>
                </td>
                <td>
                    <%if(Model.TotalEnabled){ %>
                        <%=Html.TextBox("DiscountAmountWithTax", string.Format("{0:N0}", Model.DiscountAmount + Model.DiscountTax), new { @class = "money", style = "width:96px", onkeyup="calcDiscountAmount(true)", onchange="calcDiscountAmount(true)"  })%><%//Mod 2022/06/08 yano #4137 %>
                        <%--<%=Html.TextBox("DiscountAmountWithTax", string.Format("{0:N0}", Model.DiscountAmount + Model.DiscountTax), new { @class = "money", style = "width:96px", onkeyup="calcDiscountAmount(true)" })%>--%>
                    <%}else{ %>
                        <%=Html.TextBox("DiscountAmountWithTax", string.Format("{0:N0}", Model.DiscountAmount + Model.DiscountTax), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>メーカーオプション</th>
                <td><%=Html.TextBox("MakerOptionAmount", string.Format("{0:N0}", Model.MakerOptionAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
                <td><%=Html.TextBox("MakerOptionTaxAmount", string.Format("{0:N0}", Model.MakerOptionTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
                <td><%=Html.TextBox("MakerOptionAmountWithTax", string.Format("{0:N0}", Model.MakerOptionAmount + Model.MakerOptionTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
            </tr>
            <tr>
                <th style="background-color:#CCCC99">課税対象額</th>
                <td><%=Html.TextBox("TaxationAmount", string.Format("{0:N0}", Model.TaxationAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px;background-color:#CCCC99" })%></td>
                <td><%=Html.TextBox("TaxationAmountTax", string.Format("{0:N0}", Model.SalesTax - Model.DiscountTax + Model.MakerOptionTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px;background-color:#CCCC99" })%></td>
                <td><%=Html.TextBox("TaxationAmountWithTax", string.Format("{0:N0}", Model.TaxationAmount + Model.SalesTax - Model.DiscountTax + Model.MakerOptionTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px;background-color:#CCCC99" })%></td>
            </tr>
            <tr>
                <th>販売店オプション</th>
                <td><%=Html.TextBox("ShopOptionAmount", string.Format("{0:N0}", Model.ShopOptionAmount), new { @class = "readonly money", @readonly = "readonly", style="width:96px" })%></td>
                <td><%=Html.TextBox("ShopOptionTaxAmount", string.Format("{0:N0}", Model.ShopOptionTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
                <td><%=Html.TextBox("ShopOptionAmountWithTax", string.Format("{0:N0}", Model.ShopOptionAmount + Model.ShopOptionTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
            </tr>
            <tr>
                <th style="background-color:#CCCC99">車両販売計</th>
                <td><%=Html.TextBox("SubTotalAmount", string.Format("{0:N0}", Model.SubTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px;background-color:#CCCC99" })%></td>
                <td><%=Html.TextBox("SubTotalTaxAmount", string.Format("{0:N0}", Model.SalesTax - Model.DiscountTax + Model.MakerOptionTaxAmount + Model.ShopOptionTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px;background-color:#CCCC99" })%></td>
                <td><%=Html.TextBox("SubTotalAmountWithTax", string.Format("{0:N0}", Model.SubTotalAmount + (Model.SalesTax - Model.DiscountTax + Model.MakerOptionTaxAmount + Model.ShopOptionTaxAmount)), new { @class = "readonly money", @readonly = "readonly", style = "width:96px;background-color:#CCCC99" })%></td>
            </tr>
            <tr>
                <th>販売諸費用</th>
                <td><%=Html.TextBox("SalesCostTotalAmount", string.Format("{0:N0}", Model.SalesCostTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
                <td><%=Html.TextBox("SalesCostTotalTaxAmount", string.Format("{0:N0}", Model.SalesCostTotalTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
                <td><%=Html.TextBox("SalesCostTotalAmountWithTax", string.Format("{0:N0}", Model.SalesCostTotalAmount + Model.SalesCostTotalTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
            </tr>
            <tr>
                <th>税金等</th>
                <td><%=Html.TextBox("TaxFreeTotalAmount", string.Format("{0:N0}", Model.TaxFreeTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
                <td><%=Html.TextBox("TaxFreeTotalTaxAmount", string.Format("{0:N0}", 0), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
                <td><%=Html.TextBox("TaxFreeTotalAmountWithTax", string.Format("{0:N0}", Model.TaxFreeTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
            </tr>
            <tr>
                <th>その他非課税</th>
                <td><%=Html.TextBox("OtherCostTotalAmount", string.Format("{0:N0}", Model.OtherCostTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
                <td><%=Html.TextBox("OtherCostTotalTaxAmount", string.Format("{0:N0}", 0), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
                <td><%=Html.TextBox("OtherCostTotalAmountWithTax", string.Format("{0:N0}", Model.OtherCostTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px" })%></td>
            </tr>
            <tr>
                <th style="background-color:#CCCC99">諸費用計</th>
                <td><%=Html.TextBox("CostTotalAmount", string.Format("{0:N0}", Model.CostTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px;background-color:#CCCC99" })%></td>
                <td><%=Html.TextBox("CostTotalTaxAmount", string.Format("{0:N0}", Model.SalesCostTotalTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px;background-color:#CCCC99" })%></td>
                <td><%=Html.TextBox("CostTotalAmountWithTax", string.Format("{0:N0}", Model.CostTotalAmount + Model.SalesCostTotalTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px;background-color:#CCCC99" })%></td>
            </tr>
            <tr>
                <th style="background-color:#CCCC99"><span style="font-weight:bold">現金販売合計</span></th>
                <td><%=Html.TextBox("TotalPrice", string.Format("{0:N0}", (Model.SubTotalAmount ?? 0) + (Model.CostTotalAmount ?? 0)) , new { @class = "readonly money", @readonly = "readonly", style = "width:96px;font-weight:bold;background-color:#CCCC99" })%></td>
                <td><%=Html.TextBox("TotalTaxAmount", string.Format("{0:N0}", Model.TotalTaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px;font-weight:bold;background-color:#CCCC99" })%></td>
                <td><%=Html.TextBox("GrandTotalAmount", string.Format("{0:N0}", Model.GrandTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:96px;font-weight:bold;background-color:#CCCC99" })%></td>
            </tr>
        </table>
        <%//Mod 2021/06/09 yano #4091%>
        <%=Html.Hidden("MaintenancePackageAmount", string.Format("{0:N0}", Model.MaintenancePackageAmount), new { @class = "readonly money", @readonly = "readonly", id = "MaintenancePackageAmount"}) %>
        <%=Html.Hidden("MaintenancePackageTaxAmount", string.Format("{0:N0}", Model.MaintenancePackageTaxAmount), new { @class = "readonly money", @readonly = "readonly", id = "MaintenancePackageTaxAmount"}) %>
        <%=Html.Hidden("ExtendedWarrantyAmount", string.Format("{0:N0}", Model.ExtendedWarrantyAmount), new { @class = "readonly money", @readonly = "readonly", id = "ExtendedWarrantyAmount"}) %>
        <%=Html.Hidden("ExtendedWarrantyTaxAmount", string.Format("{0:N0}", Model.ExtendedWarrantyTaxAmount), new { @class = "readonly money", @readonly = "readonly", id = "ExtendedWarrantyTaxAmount"}) %>

        <%//Add 2023/09/18 yano #4181%>
        <%=Html.Hidden("SurchargeAmount", string.Format("{0:N0}", Model.SurchargeAmount), new { @class = "readonly money", @readonly = "readonly", id = "SurchargeAmount"}) %>
        <%=Html.Hidden("SurchargeTaxAmount", string.Format("{0:N0}", Model.SurchargeTaxAmount), new { @class = "readonly money", @readonly = "readonly", id = "SurchargeTaxAmount"}) %>