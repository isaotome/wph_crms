<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
        <table class="input-form">
            <tr>
                <th colspan="2" class="input-form-title">所有者情報</th>
            </tr>
            <tr>
                <th style="width: 100px">所有者コード</th>
                <td style="width: 310px">
                    <%if(Model.OwnerEnabled){ %>
                        <%=Html.TextBox("PossesorCode", Model.PossesorCode, new { @class = "alphanumeric", size = "15" ,
                        onblur = "GetMasterDetailFromCode('PossesorCode',new Array('PossesorName','PossesorAddress'),'Customer')"})%>
                        &nbsp;<img alt="所有者" style="cursor: pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('PossesorCode','','/Customer/CriteriaDialog');$('#UserCode').focus();" />
                    <%}else{ %>
                        <%=CommonUtils.DefaultNbsp(Model.PossesorCode) %>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>所有者氏名</th>
                <td><span id="PossesorName"><%=CommonUtils.DefaultNbsp(ViewData["PossesorName"]) %></span></td>
            </tr>
            <tr>
                <th>所有者住所</th>
                <td><span id="PossesorAddress"><%=CommonUtils.DefaultNbsp(ViewData["PossesorAddress"]) %></span></td>
            </tr>
            <tr>
                <th>使用者コード</th>
                <td>
                    <%if (Model.OwnerEnabled) { %>
                        <%=Html.TextBox("UserCode", Model.UserCode, new { @class = "alphanumeric", size = "15", onblur = "GetMasterDetailFromCode('UserCode',new Array('UserName','UserAddress','PrincipalPlace'),'Customer')" })%>
                        &nbsp;<img alt="使用者検索" style="cursor: pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('UserCode','','/Customer/CriteriaDialog');$('#PrincipalPlace').focus();" />
                    <%}else{ %>
                        <%=CommonUtils.DefaultNbsp(Model.UserCode) %>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>使用者氏名</th>
                <td><span id="UserName"><%=CommonUtils.DefaultNbsp(ViewData["UserName"]) %></span></td>
            </tr>
            <tr>
                <th>使用者住所</th>
                <td><span id="UserAddress"><%=CommonUtils.DefaultNbsp(ViewData["UserAddress"]) %></span></td>
            </tr>
            <tr>
                <th>使用者本拠地</th>
                <td>
                    <%if(Model.OwnerEnabled){ %>
                        <%=Html.TextBox("PrincipalPlace", Model.PrincipalPlace, new { size="45" })%>
                    <%}else{ %>
                        <%=CommonUtils.DefaultNbsp(Model.PrincipalPlace) %>
                    <%} %>
                </td>
            </tr>
        </table>