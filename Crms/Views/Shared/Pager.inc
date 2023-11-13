<!-- ページング処理 -->
<%if (Model != null && Model.PageProperty != null)
  {%>
    <%if (Model.PageProperty.PageIndex != 0)
      {%>
        <a href="javascript:displaySearchList(<%=Model.PageProperty.PageIndex - 1%>)">&lt&lt</a>&nbsp;
    <%} %>
    <% for (int i = Model.PageProperty.StartPageIndex; i < Model.PageProperty.TotalPages && i <= Model.PageProperty.PageIndex + 10; i++)
       {
           if (Model.PageProperty.PageIndex != i)
           {%>
                <a href="javascript:displaySearchList(<%=i%>)"><%=(i + 1).ToString()%></a>&nbsp;
           <%}
           else
           {%>
                <%=i + 1%>&nbsp;
           <%} %>
       <%} %>
    <%if (Model.PageProperty.PageIndex != Model.PageProperty.TotalPages - 1 && Model.PageProperty.TotalPages != 0)
      {%>
        <a href="javascript:displaySearchList(<%=Model.PageProperty.PageIndex + 1%>)">&gt&gt</a>
    <%} %>
<%} %>
<!-- ページング処理 -->
