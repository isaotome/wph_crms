<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.BackGroundDemoCar>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両用途変更履歴
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%CrmsLinqDataContext db = new CrmsLinqDataContext(); %>
<%using (Html.BeginForm("History", "CarUsageSetting", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<table class="command">
   <tr>
       <td onclick=" window.close();"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td> 
   </tr>
</table>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:100px; white-space:nowrap">振替日</th>
        <th style="width:100px; white-space:nowrap">処理種別</th>
        <th style="width:100px; white-space:nowrap">受注伝票番号</th>
        <th style="width:100px; white-space:nowrap">入庫先ID</th>
        <th style="width:100px; white-space:nowrap">対象の管理番号</th>
        <th style="width:100px; white-space:nowrap">新しい管理番号</th>
        <th style="width:100px; white-space:nowrap">最終更新日</th>
        <th style="width:100px; white-space:nowrap">最終更新者</th>
    </tr>
    <%foreach (var rec in Model)
      { %>

    <%
           c_CodeName codeName;
           string procName = "";

           codeName = new CodeDao(db).GetCodeNameByKey("005", rec.ProcType, true);

           if (codeName != null)
           {
               procName = codeName.Name;
           }
           else
           {
               codeName = new CodeDao(db).GetCodeNameByKey("006", rec.ProcType, true);

               if (codeName != null)
               {
                   procName = codeName.Name;
               }
           }
           
        
    %>

    <tr>
        <td style ="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", rec.ProcDate))%></td>
        <td style ="white-space:nowrap"><%=Html.Encode(procName)%></td>
        <td style ="white-space:nowrap"><%=Html.Encode(rec.SlipNumber)%></td>
        <td style ="white-space:nowrap"><%=Html.Encode(rec.LocationCode)%></td>
        <td style ="white-space:nowrap"><%=Html.Encode(rec.SalesCarNumber)%></td>
        <td style ="white-space:nowrap"><%=Html.Encode(rec.NewSalesCarNumber)%></td>
        <td style ="white-space:nowrap"><%=Html.Encode(rec.LastUpdateDate)%></td>
        <td style ="white-space:nowrap"><%=Html.Encode(rec.LastUpdateEmployeeCode)%></td>
    </tr>
    <%} %>
</table>
<br />
<%} %>
</asp:Content>
