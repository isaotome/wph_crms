<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.SalesCar>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	在庫表
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("List", "SalesCar", new { id = 0 }, FormMethod.Post)) { %>
<%=Html.Hidden("id","0") %>
<table class="input-form">
    <tr>
        <th>部門</th>
        <td><%=Html.DropDownList("DepartmentCode", (IEnumerable<SelectListItem>)ViewData["DepartmentList"]) %></td>
    </tr>
    <tr>
        <td colspan="2">
            <input type="button" value="新車" onclick="document.getElementById('NewUsedType').value='N';displaySearchList()" />
            <input type="button" value="中古車" onclick="document.getElementById('NewUsedType').value='U';displaySearchList()" />
        </td>
    </tr>
</table>
<%=Html.Hidden("NewUsedType",ViewData["NewUsedType"]) %>
<%} %>
<br />
<br />
<%Html.RenderPartial("PagerControl", Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="white-space:nowrap">見積</th>
        <th style="white-space:nowrap">新中区分</th>
        <th style="white-space:nowrap">型式</th>
        <th style="white-space:nowrap">ブランド</th>
        <th style="white-space:nowrap">車種</th>
        <th style="white-space:nowrap">グレード</th>
        <th style="white-space:nowrap">モデル年</th>
        <th style="white-space:nowrap">年式</th>
        <th style="white-space:nowrap">外装色</th>
        <th style="white-space:nowrap">ハンドル</th>
        <th style="white-space:nowrap">販売価格(千円)</th>
        <th style="white-space:nowrap">走行距離</th>
        <th style="white-space:nowrap">車検</th>
        <th style="white-space:nowrap">ﾐｯｼｮﾝ</th>
        <th style="white-space:nowrap">シート</th>
        <th style="white-space:nowrap">セールスポイント</th>
        <th style="white-space:nowrap"></th>
        <th style="white-space:nowrap">認定中古車番号</th>
        <th style="white-space:nowrap">入庫日</th>
        <th style="white-space:nowrap">ロケーション</th>
        <th style="white-space:nowrap">管理番号</th>
        <th style="white-space:nowrap">車台番号</th>
        <th style="white-space:nowrap">所有者</th>
        <th style="white-space:nowrap">リサイクル</th>
        <th style="white-space:nowrap">ステータス</th>
        <th style="white-space:nowrap">ファイナンス</th>
        
    </tr>
    <%foreach(var item in Model){
          DateTime? purchaseDate = null;
          string salesPoint = "";
          if (item.CarPurchase != null && item.CarPurchase.Count() > 0) {
              var purchase = item.CarPurchase.Where(x => x.PurchaseLocationCode!=null && x.PurchaseLocationCode.Equals(item.LocationCode)).FirstOrDefault();
              if (purchase != null) {
                  purchaseDate = purchase.PurchaseDate;
                  salesPoint = purchase.CarAppraisal != null ? purchase.CarAppraisal.Remarks : "";
              }
          }
    %>
    <tr>
        <td style="white-space:nowrap"><a href="javascript:void(0)" onclick="openModalDialog('/CarSalesOrder/Entry?salesCarNumber=<%=item.SalesCarNumber %>')">見積作成</a></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.c_NewUsedType!=null ? item.c_NewUsedType.Name : "")%></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.ModelName)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.CarGrade!=null && item.CarGrade.Car!=null && item.CarGrade.Car.Brand!=null ? item.CarGrade.Car.Brand.CarBrandName : "")%></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.CarGrade!=null && item.CarGrade.Car!=null ? item.CarGrade.Car.CarName : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.CarGrade!=null ? item.CarGrade.CarGradeName : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.CarGrade!=null ? item.CarGrade.ModelYear : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.ManufacturingYear)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.ExteriorColorName) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.c_Steering!=null ? item.c_Steering.Name : "") %></td>
        <td style="white-space:nowrap;font-size:14pt;color:Red;text-align:right;font-weight:bold"><%=Html.Encode(string.Format("{0:N1}",(item.SalesPrice ?? 0m) / 1000 )) %></td>
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.Mileage)) %> <%=Html.Encode(item.Mileage!=null && item.c_MileageUnit!=null ? item.c_MileageUnit.Name : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.InspectionDate)) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.CarGrade!=null && item.CarGrade.c_TransMission!=null ? item.CarGrade.c_TransMission.Name : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.c_SeatType!=null ? item.c_SeatType.Name : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(salesPoint)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.Memo)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.ApprovedCarNumber) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",purchaseDate))%></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.Location!=null ? item.Location.LocationName : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.SalesCarNumber) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.Vin) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.PossesorName)%></td>
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.RecycleDeposit))%></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.c_CarStatus!=null ? item.c_CarStatus.Name : "")%></td>
        <td><%=Html.Encode(item.c_Finance!=null ? item.c_Finance.Name : "") %></td>
    </tr>
    <%} %>
</table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
