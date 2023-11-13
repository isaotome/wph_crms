<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage2.Master" Inherits="System.Web.Mvc.ViewPage<List<string[]>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	連携ファイル取込
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% using (Html.BeginForm("Criteria", "DataServiceReceive", FormMethod.Post, new { enctype = "multipart/form-data" })) {%>
<table class="input-form">
    <tr>
        <th>取込ファイル指定</th>
        <td><input type="file"  name="attachedFile" style="width:300px" /></td>
    </tr>
    <tr>
        <th>ファイル種別</th>
        <td><%=Html.DropDownList("FileType",((IEnumerable<SelectListItem>)ViewData["FileTypeList"])) %></td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="取込処理" onclick="document.forms[0].submit()" /></td>
    </tr>
</table>
<%} %>
<br />
<%=ViewData["message"] %>
<br />
<br />
<%if(Model!=null){ %>
<table class="list">
    <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
    <%if (CommonUtils.DefaultString(ViewData["FileType"]).Equals("TOPS車両"))
      { %>
    <tr>
        <th>MY</th>
        <th>モデル</th>
        <th>OP</th>
        <th>カラー</th>
        <th>内装</th>
        <th>コミッション</th>
        <th>Vin No</th>
        <th>ディーラー名称</th>
        <th>ステータス</th>
        <th>配車月</th>
    </tr>
    <%foreach(var item in Model){ %>
    <tr>
        <td><%=item[0] %></td>
        <td><%=item[1] %></td>
        <td><%=item[2] %></td>
        <td><%=item[3] %></td>
        <td><%=item[4] %></td>
        <td><%=item[5] %></td>
        <td><%=item[6] %></td>
        <td><%=item[7] %></td>
        <td><%=item[8] %></td>
        <td><%=item[9] %></td>
    </tr>
    <%} %>
    <%} %>
    <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
    <%if (CommonUtils.DefaultString(ViewData["FileType"]).Equals("TOPSパーツ"))
      { %>
    <tr>
        <th>区分</th>
        <th>請求先</th>
        <th>直送先</th>
        <th>出荷パーツNO</th>
        <th>売上NO</th>
        <th>売上日</th>
        <th>出荷数</th>
        <th>仕切短歌</th>
        <th>金額</th>
        <th>エリア</th>
        <th>直送</th>
    </tr>
    <%foreach (var item in Model) { %>
    <tr>
        <td><%=item[0] %></td>
        <td><%=item[1] %></td>
        <td><%=item[2] %></td>
        <td><%=item[3] %></td>
        <td><%=item[4] %></td>
        <td><%=item[5] %></td>
        <td><%=item[6] %></td>
        <td><%=item[7] %></td>
        <td><%=item[8] %></td>
        <td><%=item[9] %></td>
        <td><%=item[10] %></td>
    </tr>
    <%} %>
    <%} %>
</table>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
