<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	権限エラー
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div style="margin-top:200px">
    <center>
        <table style="border:solid 1px Red;background-color:#ffeeee;width:400px">
            <tr>
                <td style="padding:20px 20px 20px 20px">
                    この帳票を閲覧する権限がありません。<br />
                    恐れ入りますがシステム管理者までお問い合わせください<br />
                </td>
            </tr>
        </table>
    </center>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
