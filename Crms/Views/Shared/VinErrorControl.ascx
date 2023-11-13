<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<CrmsDao.SalesCar>>" %>
    <%for(int i=0;i<Model.Count();i++){ %>
<% //Mod 2014/07/15 arc yano chrome対応 type追加 %>    
<button type="submit" onclick="openModalDialog('/SalesCar/Entry/<%=Model[i].SalesCarNumber %>?Master=1')">
    車台番号：<%=Html.Encode(Model[i].Vin)%> (<%=i+1 %>) の情報を表示する
    </button>
    <br />
    <%} %>
<br />
