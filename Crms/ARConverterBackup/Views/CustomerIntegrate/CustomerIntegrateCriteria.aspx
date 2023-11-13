<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    名寄せツール
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<%
//---------------------------------------------
// 機能  ：名寄せツール画面
// 作成者：arc nakayama
// 作成日：2017/03/18
// 更新日：
//---------------------------------------------
%>

<%using (Html.BeginForm("Criteria", "CustomerIntegrate", new { id = 0 }, FormMethod.Post))
  { %>
    <%=Html.Hidden("id", "0") %>
    <br />
    <br />
    <b><a>重複した顧客データをひとつのコードに寄せるためのツールです。</a></b>
    <br />
    <b><a>顧客情報、販売履歴、整備履歴などが統合することができます。</a></b>
    <br />
    <br />
    <%=Html.ValidationSummary() %>
    <div id="UpdateMsg" style="display:none">
        <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
    </div>
    <table class="input-form">
        <tr>
            <td colspan="2">こっちに、<a style="color:blue">残したい</a>顧客コードを入力してください。</td>
            <td colspan="2">こっちに、<a style="color:red">削除したい</a>顧客コードを入力してください。</td>
        </tr>
        <tr>
            <td style="background-color:lightblue"; colspan="2"; align="center">残す顧客</td>
            <td style="background-color:lightpink"; colspan="2"; align="center">消す顧客</td>
        </tr>
        <tr>
            <th>顧客コード</th>
            <td><%=Html.TextBox("CustomerCode1", ViewData["CustomerCode1"], new { @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetNameFromCodeForCustomerIntegrate('CustomerCode1',new Array('CustomerName1','TelNumber1', 'MobileNumber1', 'PostCode1','Prefecture1', 'City1', 'Address1_1', 'Address2_1', 'CarCnt1', 'ServiceCnt1'),'Customer')"})%>&nbsp;<img alt="顧客コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="var callback = function() { GetNameFromCodeForCustomerIntegrate('CustomerCode1',new Array('CustomerName1','TelNumber1', 'MobileNumber1', 'PostCode1','Prefecture1', 'City1', 'Address1_1', 'Address2_1', 'CarCnt1', 'ServiceCnt1'),'Customer');}; openSearchDialog('CustomerCode1','CustomerName1','/Customer/CriteriaDialog/?CustomerName1='+encodeURIComponent(document.getElementById('CustomerName1').value), null, null, null, null, callback);GetNameFromCodeForCustomerIntegrate('CustomerCode1',new Array('CustomerName1','TelNumber1', 'MobileNumber1', 'PostCode1','Prefecture1', 'City1', 'Address1_1', 'Address2_1', 'CarCnt1', 'ServiceCnt1'),'Customer')" /></td><%//Mod 2022/01/10 yano #4121%>
            <%--<td><%=Html.TextBox("CustomerCode1", ViewData["CustomerCode1"], new { @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetNameFromCodeForCustomerIntegrate('CustomerCode1',new Array('CustomerName1','TelNumber1', 'MobileNumber1', 'PostCode1','Prefecture1', 'City1', 'Address1_1', 'Address2_1', 'CarCnt1', 'ServiceCnt1'),'Customer')"})%>&nbsp;<img alt="顧客コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerCode1','CustomerName1','/Customer/CriteriaDialog/?CustomerName1='+encodeURIComponent(document.getElementById('CustomerName1').value));GetNameFromCodeForCustomerIntegrate('CustomerCode1',new Array('CustomerName1','TelNumber1', 'MobileNumber1', 'PostCode1','Prefecture1', 'City1', 'Address1_1', 'Address2_1', 'CarCnt1', 'ServiceCnt1'),'Customer')" /></td>--%>
            <th style="background-color:pink">顧客コード</th>
            <td><%=Html.TextBox("CustomerCode2", ViewData["CustomerCode2"], new { @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetNameFromCodeForCustomerIntegrate('CustomerCode2',new Array('CustomerName2','TelNumber2', 'MobileNumber2', 'PostCode2','Prefecture2', 'City2', 'Address1_2', 'Address2_2', 'CarCnt2', 'ServiceCnt2'),'Customer')"})%>&nbsp;<img alt="顧客コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="var callback = function() { GetNameFromCodeForCustomerIntegrate('CustomerCode2',new Array('CustomerName2','TelNumber2', 'MobileNumber2', 'PostCode2','Prefecture2', 'City2', 'Address1_2', 'Address2_2', 'CarCnt2', 'ServiceCnt2'),'Customer')}; openSearchDialog('CustomerCode2','CustomerName2','/Customer/CriteriaDialog/?CustomerName2='+encodeURIComponent(document.getElementById('CustomerName2').value), null, null, null, null, callback);GetNameFromCodeForCustomerIntegrate('CustomerCode2',new Array('CustomerName2','TelNumber2', 'MobileNumber2', 'PostCode2','Prefecture2', 'City2', 'Address1_2', 'Address2_2', 'CarCnt2', 'ServiceCnt2'),'Customer')" /></td><%//Mod 2022/01/10 yano #4121%>
            <%--<td><%=Html.TextBox("CustomerCode2", ViewData["CustomerCode2"], new { @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetNameFromCodeForCustomerIntegrate('CustomerCode2',new Array('CustomerName2','TelNumber2', 'MobileNumber2', 'PostCode2','Prefecture2', 'City2', 'Address1_2', 'Address2_2', 'CarCnt2', 'ServiceCnt2'),'Customer')"})%>&nbsp;<img alt="顧客コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerCode2','CustomerName2','/Customer/CriteriaDialog/?CustomerName2='+encodeURIComponent(document.getElementById('CustomerName2').value));GetNameFromCodeForCustomerIntegrate('CustomerCode2',new Array('CustomerName2','TelNumber2', 'MobileNumber2', 'PostCode2','Prefecture2', 'City2', 'Address1_2', 'Address2_2', 'CarCnt2', 'ServiceCnt2'),'Customer')" /></td>--%>
        </tr>
        <tr>
            <th>顧客名</th>
            <td><span id="CustomerName1"><%=Html.Encode(ViewData["CustomerName1"])%></span></td>
            <th style="background-color:pink">顧客名</th>
            <td><span id="CustomerName2"><%=Html.Encode(ViewData["CustomerName2"])%></span></td>
        </tr>
        <tr>
            <th>電話番号</th>
            <td><a>(Tel)</a><span id="TelNumber1"><%=Html.Encode(ViewData["TelNumber1"])%></span>
                <br />
                <a>(携帯)</a><span id="MobileNumber1"><%=Html.Encode(ViewData["MobileNumber1"])%></span>
            </td>
            <th style="background-color:pink">電話番号</th>
            <td><a>(Tel)</a><span id="TelNumber2"><%=Html.Encode(ViewData["TelNumber2"])%></span>
                <br />
                <a>(携帯)</a><span id="MobileNumber2"><%=Html.Encode(ViewData["MobileNumber2"])%></span>
            </td>
        </tr>
        <tr>
            <th>住所</th>
            <td>
                <span id="PostCode1"><%=Html.Encode(ViewData["PostCode1"])%></span>
                <br />
                <span id="Prefecture1"><%=Html.Encode(ViewData["Prefecture1"])%></span>&nbsp;<span id="City1" ><%=Html.Encode(ViewData["City1"])%></span>
                <span id="Address1_1"><%=Html.Encode(ViewData["Address1_1"])%></span>&nbsp;<span id="Address2_1" ><%=Html.Encode(ViewData["Address2_1"])%></span>
            </td>
            <th style="background-color:pink">住所</th>
            <td>
                <span id="PostCode2"><%=Html.Encode(ViewData["PostCode2"])%></span>
                <br />
                <span id="Prefecture2"><%=Html.Encode(ViewData["Prefecture2"])%></span>&nbsp;<span id="City2"><%=Html.Encode(ViewData["City2"])%></span>
                <span id="Address1_2" ><%=Html.Encode(ViewData["Address1_2"])%></span>&nbsp;<span id="Address2_2" ><%=Html.Encode(ViewData["Address2_2"])%></span>
            </td>
        </tr>
        <tr>
            <th>車両伝票枚数</th>
            <td>
                <span id="CarCnt1"><%=Html.Encode(ViewData["CarCnt1"])%></span>
            </td>
            <th style="background-color:pink">車両伝票枚数</th>
            <td>
                <span id="CarCnt2"><%=Html.Encode(ViewData["CarCnt2"])%></span>
            </td>
        </tr>
        <tr>
            <th>サービス伝票枚数</th>
            <td>
                <span id="ServiceCnt1"><%=Html.Encode(ViewData["ServiceCnt1"])%></span>
            </td>
            <th style="background-color:pink">サービス伝票枚数</th>
            <td>
                <span id="ServiceCnt2"><%=Html.Encode(ViewData["ServiceCnt2"])%></span>
            </td>
        </tr>
        <tr>
            <td colspan="4">
                <center>
                <input type="button" value="　←　名寄せする　←　" style="width:150px;height:50px;font-size:14px;"onclick="CustomerIntegrateCheck('CustomerCode1', 'CustomerCode2');" />
                </center>
            </td>
        </tr>
    </table>
<%} %>
</asp:Content>

