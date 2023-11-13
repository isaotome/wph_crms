<%-- ******************************************************** --%>
<%-- * ページングの処理を記述した共通コントロール             --%>
<%-- ******************************************************** --%>
<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.PaginatedProperty>" %>
<%@ Import Namespace="CrmsDao"  %>
<%if (Model != null)
  {%>
    <span style="font-size:12pt">
        <%-- MOD 2014/06/10 arc uchida ページ番号の参照変更 --%>
   <%-- <b><%=Model.Count>0 ? Model.PageIndex*DaoConst.PAGE_SIZE+1 : 0%> - <%=Model.PageIndex*DaoConst.PAGE_SIZE + Model.Count %> / <%=Model.TotalCount%> </b>&nbsp; --%>
    <b><%=Model.Count>0 ? Model.PageIndex*Model.PageSize +1 : 0%> - <%=Model.PageIndex*Model.PageSize + Model.Count %> / <%=Model.TotalCount%> </b>&nbsp;
    <%if (Model.PageIndex != 0)
      {%>
        <a href="javascript:displaySearchList(<%=Model.PageIndex - 1%>)">&lt&lt前ページ</a>&nbsp;
    <%} %>
    <% for (int i = Model.StartPageIndex; i < Model.TotalPages && i <= Model.PageIndex + 10; i++)
       {
           if (Model.PageIndex != i)
           {%>
                <a href="javascript:displaySearchList(<%=i%>)"><%=(i + 1).ToString()%></a>&nbsp;
           <%}
           else
           {%>
                <%=i + 1%>&nbsp;
           <%} %>
       <%} %>
    <%if (Model.PageIndex != Model.TotalPages - 1 && Model.TotalPages != 0)
      {%>
        <a href="javascript:displaySearchList(<%=Model.PageIndex + 1%>)">&gt&gt次ページ</a>
    <%} %>
    </span>
<%} %>

