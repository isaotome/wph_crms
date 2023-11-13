<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<CrmsDao.ServiceClaimable>>" %>
<table class="input-form">
    <tr>
        <th>請求先コード</th>
        <th>主作業</th>
        <th>請求先名</th>
        <th>工賃売上</th>
        <th>部品売上</th>
        <th>売上計</th>
        <th>売上計（税込）</th>
        <th>工賃原価</th>
        <th>部品原価</th>
        <th>原価計</th>
        <th>工賃粗利</th>
        <th>部品粗利</th>
        <th>粗利計</th>
        <th>粗利率</th>
        <th>諸費用計</th>
    </tr>
    <%foreach (var item in Model) { %>
    <tr>
        <td><%=item.CustomerClaimCode%></td>
        <td><%=item.ServiceWork!=null ? item.ServiceWork.Name : "" %></td>
        <td><%=item.CustomerClaim!=null ? item.CustomerClaim.CustomerClaimName : "" %></td>
        <td style="text-align:right"><%=string.Format("{0:N0}",item.TechnicalFeeAmount) %></td>
        <td style="text-align:right"><%=string.Format("{0:N0}",item.PartsAmount) %></td>
        <td style="text-align:right;font-weight:bold"><%=string.Format("{0:N0}",item.Amount) %></td>
        <td style="text-align:right;font-weight:bold"><%=string.Format("{0:N0}",item.AmountWithTax) %></td>
        <td style="text-align:right"><%=string.Format("{0:N0}",item.TechnicalCost) %></td>
        <td style="text-align:right"><%=string.Format("{0:N0}",item.PartsCost) %></td>
        <td style="text-align:right"><%=string.Format("{0:N0}",item.Cost) %></td>
        <td style="text-align:right"><%=string.Format("{0:N0}",item.TechnicalMargin) %></td>
        <td style="text-align:right"><%=string.Format("{0:N0}",item.PartsMargin) %></td>
        <td style="text-align:right"><%=string.Format("{0:N0}",item.Margin) %></td>
        <td style="text-align:right"><%=item.MarginRate %>%</td>
        <td style="text-align:right"><%=string.Format("{0:N0}", item.DepositAmount) %></td>
    </tr>
    <%} %>
    <tr>
        <td style="border-top:solid 3px;text-align:right" colspan="3">合計</td>
        <td style="text-align:right;border-top:solid 3px"><%=string.Format("{0:N0}", Model.Sum(x => x.TechnicalFeeAmount)) %></td>
        <td style="text-align:right;border-top:solid 3px"><%=string.Format("{0:N0}", Model.Sum(x => x.PartsAmount)) %></td>
        <td style="text-align:right;border-top:solid 3px;font-weight:bold"><%=string.Format("{0:N0}", Model.Sum(x => x.Amount)) %></td>
        <td style="text-align:right;border-top:solid 3px;font-weight:bold"><%=string.Format("{0:N0}", Model.Sum(x => x.AmountWithTax)) %></td>
        <td style="text-align:right;border-top:solid 3px"><%=string.Format("{0:N0}", Model.Sum(x => x.TechnicalCost)) %></td>
        <td style="text-align:right;border-top:solid 3px"><%=string.Format("{0:N0}", Model.Sum(x => x.PartsCost)) %></td>
        <td style="text-align:right;border-top:solid 3px"><%=string.Format("{0:N0}", Model.Sum(x => x.Cost)) %></td>
        <td style="text-align:right;border-top:solid 3px"><%=string.Format("{0:N0}", Model.Sum(x => x.TechnicalMargin)) %></td>
        <td style="text-align:right;border-top:solid 3px"><%=string.Format("{0:N0}", Model.Sum(x => x.PartsMargin)) %></td>
        <td style="text-align:right;border-top:solid 3px"><%=string.Format("{0:N0}", Model.Sum(x => x.Margin)) %></td>
        <td style="text-align:right;border-top:solid 3px"></td>
        <td style="text-align:right;border-top:solid 3px"><%=string.Format("{0:N0}", Model.Sum(x=>x.DepositAmount))%></td>
    </tr>
</table>
