<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarSalesHeader>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	CarReservationCancelCriteria
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<%using (Html.BeginForm("Criteria","CarReservationCancel",new {id = "0"},FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("ActionType","") %>
<%=Html.Hidden("RowId","") %>
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:250px">ロケーション</th>
        <th style="width:100px"></th>
        <th>元伝票番号</th>
        <th>元伝票納車日</th>
        <th>赤伝票番号</th>
        <th>赤伝納車日</th>
        <th>車台番号</th>
        <th>ブランド</th>
        <th>車種</th>
    </tr>
    <%for(int i = 0 ;i<Model.Count();i++){
        string prefix = string.Format("item[{0}].",Model[i].SalesCarNumber);
        CrmsDao.CarSalesHeader item = Model[i];
        //2014/05/30 vs2012対応 arc yano 各コントロールにid追加
        string idprefix = string.Format("item[{0}]_",Model[i].SalesCarNumber);
          
    %> 
    <tr>
        <td>
            <% //Mod 2014/07/22 arc yano chrome対応 パラメータをname→idに変更 %>
            <%=Html.TextBox(prefix + "LocationCode", item.LocationCode, new { id = idprefix + "LocationCode", @class = "alphanumeric", style = "width:50px", onchange = "GetLocationNameFromCode('" + idprefix + "LocationCode','" + idprefix + "LocationName',new Array('001','009'),'001')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { idprefix + "LocationCode", idprefix + "LocationName", "'/Location/CriteriaDialog?BusinessType=001,009&LocationType=001'", "0" }); %>
            <%=Html.TextBox(prefix+"LocationName",item.LocationName,new { id = idprefix+"LocationName", @class="readonly",@readonly="readonly",style="width:150px"})%>
        </td>
        <td><input type="button" value="在庫に戻す" onclick="if(document.getElementById('<%=idprefix %>LocationCode').value==''){ alert('ロケーションを入力して下さい');return false;}else{document.forms[0].ActionType.value='update';document.forms[0].RowId.value='<%=Model[i].SalesCarNumber %>';document.forms[0].submit();}" /></td>
        <td><%=item.OriginalCarSalesHeader.SlipNumber %></td>
        <td><%=string.Format("{0:yyyy/MM/dd}", item.OriginalCarSalesHeader.SalesDate) %></td>
        <td><%=item.SlipNumber %></td>
        <td><%=string.Format("{0:yyyy/MM/dd}", item.SalesDate) %></td>
        <td><%=item.SalesCar.Vin %><%=Html.Hidden(prefix+"SalesCarNumber",item.SalesCarNumber, new { id = idprefix+"SalesCarNumber" }) %></td>
        <td><%=item.SalesCar.CarGrade.Car.Brand.CarBrandName %></td>
        <td><%=item.SalesCar.CarGrade.Car.CarName %></td>
    </tr>   
    <%} %>
</table>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
