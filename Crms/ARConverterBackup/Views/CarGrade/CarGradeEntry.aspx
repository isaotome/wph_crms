<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarGrade>" %>
<%@ Import Namespace="CrmsDao" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	グレードマスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "CarGrade", FormMethod.Post))
      { %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.Hidden("DelLine","") %>
<%=Html.ValidationSummary()%>
<input type="button" value="▼車両情報" onclick="changeDisplayCarGrade('Basic');" />
<input type="button" value="▼車両カラー" onclick="changeDisplayCarGrade('Color');" />
<div id="Basic" style="<%=!(bool)ViewData["BasicDisplay"] ? "display:none" : "display:block"%>">
<div style="float:left">
    <br />
    <table class="input-form" style="width:500px">
        <tr>
            <th colspan="2" class="input-form-title">車両情報</th>
        </tr>
      <tr>
        <th style="width:100px">グレードコード *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="CarGradeCode" name="CarGradeCode" value="<%=Model.CarGradeCode%>" readonly="readonly", size="30" /><%}
              else
              { %><%=Html.TextBox("CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric", size = 30, maxlength = 30, onblur = "IsExistCode('CarGradeCode','CarGrade')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>グレード名 *</th>
        <td><%=Html.TextBox("CarGradeName", Model.CarGradeName, new { size = 50, maxlength = 50 })%></td>
      </tr>
      <tr>
        <th>モデルコード</th>
        <td><%=Html.TextBox("ModelCode", Model.ModelCode, new { @class = "alphanumeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th rowspan="2">車種 *</th>
        <td><%=Html.TextBox("CarCode", Model.CarCode, new { @class = "alphanumeric", size = 30, maxlength = 30, onblur = "GetNameFromCode('CarCode','CarName','Car')" })%>
            <img alt="車種検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarCode', 'CarName', '/Car/CriteriaDialog')" /></td>
      </tr>
      <tr>
        <td style="height:20px"><span id="CarName"><%=Html.Encode(ViewData["CarName"])%></span></td>       
      </tr>
      <tr>
        <th rowspan="2">車両クラス *</th>
        <td><%=Html.TextBox("CarClassCode", Model.CarClassCode, new { @class = "alphanumeric", size = 30, maxlength = 30, onblur = "GetNameFromCode('CarClassCode','CarClassName','CarClass')" })%>
            <img alt="車両クラス検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarClassCode', 'CarClassName', '/CarClass/CriteriaDialog')" /></td>
      </tr>
      <tr>
        <td style="height:20px"><span id="CarClassName"><%=Html.Encode(ViewData["CarClassName"])%></span></td>       
      </tr>
      <tr>
        <%//Mod 2021/08/02 yano #4097 入力可能文字数を4→10に変更%>
        <th>モデル年</th>
        <td><%=Html.TextBox("ModelYear", Model.ModelYear, new { @class = "alphanumeric", maxlength = 10})%></td>
      </tr>
      <tr>
        <th>ドア</th>
        <td><%=Html.TextBox("Door", Model.Door, new { @class = "alphanumeric", maxlength = 3 })%></td>
      </tr>
      <tr>
        <th>トランスミッション</th>
        <td><%=Html.DropDownList("TransMission", (IEnumerable<SelectListItem>)ViewData["TransMissionList"])%></td>
      </tr>
      <tr>
        <th>販売価格</th>
        <td><%=Html.TextBox("SalesPrice", Model.SalesPrice, new { @class = "numeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>販売開始日(YYYY/MM/DD)</th>
        <td><%=Html.TextBox("SalesStartDate", string.Format("{0:yyyy/MM/dd}", Model.SalesStartDate), new { @class = "alphanumeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>販売終了日(YYYY/MM/DD)</th>
        <td><%=Html.TextBox("SalesEndDate", string.Format("{0:yyyy/MM/dd}", Model.SalesEndDate), new { @class = "alphanumeric", maxlength = 10 })%></td>
      </tr>
       
      <tr>
        <th>駆動方式</th>
        <td><%=Html.DropDownList("DrivingName", (IEnumerable<SelectListItem>)ViewData["DrivingNameList"])%></td>
      </tr>
      <tr>
        <th>エンジン種別</th>
        <td><%=Html.DropDownList("VehicleType",(IEnumerable<SelectListItem>)ViewData["VehicleTypeList"]) %></td>
      </tr>

      <tr>
        <th>自動車の種別</th>
        <td><%=Html.DropDownList( "CarClassification", (IEnumerable<SelectListItem>)ViewData["CarClassificationList"])%></td>
      </tr>
      <tr>
        <th>用途</th>
        <td><%=Html.DropDownList("Usage", (IEnumerable<SelectListItem>)ViewData["UsageList"])%></td>
      </tr>
      <tr>
        <th>自家用・事業用の別</th>
        <td><%=Html.DropDownList("UsageType", (IEnumerable<SelectListItem>)ViewData["UsageTypeList"])%></td>
      </tr>
      <tr>
        <th>車体の形状</th>
        <td><%=Html.DropDownList("Figure", (IEnumerable<SelectListItem>)ViewData["FigureList"])%></td>
      </tr>
      <tr>
        <th>乗車定員</th>
        <td><%=Html.TextBox("Capacity", Model.Capacity, new { @class = "numeric", maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>最大積載量</th>
        <td><%=Html.TextBox("MaximumLoadingWeight", Model.MaximumLoadingWeight, new { @class = "numeric", maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>車両重量</th>
        <td><%=Html.TextBox("CarWeight", Model.CarWeight, new { @class = "numeric", maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>車両総重量</th>
        <td><%=Html.TextBox("TotalCarWeight", Model.TotalCarWeight, new { @class = "numeric", maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>長さ</th>
        <td><%=Html.TextBox("Length", Model.Length, new { @class = "numeric", maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>幅</th>
        <td><%=Html.TextBox("Width", Model.Width, new { @class = "numeric", maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>高さ</th>
        <td><%=Html.TextBox("Height", Model.Height, new { @class = "numeric", maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>前前軸重</th>
        <td><%=Html.TextBox("FFAxileWeight", Model.FFAxileWeight, new { @class = "numeric", maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>前後軸重</th>
        <td><%=Html.TextBox("FRAxileWeight", Model.FRAxileWeight, new { @class = "numeric", maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>後前軸重</th>
        <td><%=Html.TextBox("RFAxileWeight", Model.RFAxileWeight, new { @class = "numeric", maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>後後軸重</th>
        <td><%=Html.TextBox("RRAxileWeight", Model.RRAxileWeight, new { @class = "numeric", maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>型式</th>
        <td><%=Html.TextBox("ModelName", Model.ModelName, new { size = 40, maxlength = 20 })%></td>
      </tr>
      <tr>
        <th>原動機の型式</th>
        <td><%=Html.TextBox("EngineType", Model.EngineType, new { @class = "alphanumeric", maxlength = 25 })%></td><%//Mod 2020/11/27 yano #4072 15 -> 25 %><%//Mod  2019/5/23 yano #3992%>
      </tr>
      <tr>
        <th>総排気量又は定格出力</th>
          <% // Mod 2014/07/14 arc amii 既存バグ対応 フル桁で登録すると、登録桁数より多くなり、システムエラーになっていたのを修正  %>
        <td><%=Html.TextBox("Displacement", Model.Displacement, new { @class = "numeric", maxlength = 8 })%></td>
      </tr>
      <tr>
        <th>燃料の種類</th>
        <td><%=Html.DropDownList("Fuel", (IEnumerable<SelectListItem>)ViewData["FuelList"])%></td>
      </tr>
      <tr>
        <th>型式指定番号</th>
        <td><%=Html.TextBox("ModelSpecificateNumber", Model.ModelSpecificateNumber, new { @class = "alphanumeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>類別区分番号</th>
        <td><%=Html.TextBox("ClassificationTypeNumber", Model.ClassificationTypeNumber, new { @class = "alphanumeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>検査登録費用</th>
        <td><%=Html.TextBox("InspectionRegistCost",Model.InspectionRegistCost,new {@class="numeric",size="10",maxlength="10"}) %></td>
      </tr>
      <tr>
        <th>リサイクル預託金</th>
        <td><%=Html.TextBox("RecycleDeposit",Model.RecycleDeposit,new {@class="numeric",size="10",maxlength="10"}) %></td>
      </tr>
      <tr>
        <th>ステータス</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
      <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
        {%>
        <td><%=Html.RadioButton("DelFlag", "0")%>有効<%=Html.RadioButton("DelFlag", "1")%>無効</td>
      <%}
        else
        {%>
        <td><%=Html.RadioButton("DelFlag", "0", true)%>有効<%=Html.RadioButton("DelFlag", "1", new { disabled = true })%>無効</td>
      <%} %>
      </tr>
    </table>
</div>
<div style="float:left;margin-left:10px">
    <br />
    <table class="input-form">
        <tr>
            <th colspan="2" class="input-form-title">中古車点検・整備費用及び保証継承整備費用</th>
        </tr>
        <tr>
            <th style="width:150px">24ヶ月未満</th>
            <td style="width:100px"><%=Html.TextBox("Under24",Model.Under24,new {@class="numeric",size="10",maxlength="10"}) %></td>
        </tr>
        <tr>
            <th>26ヵ月未満</th>
            <td><%=Html.TextBox("Under26",Model.Under26,new {@class="numeric",size="10",maxlength="10"}) %></td>
        </tr>
        <tr>
            <th>28ヵ月未満</th>
            <td><%=Html.TextBox("Under28",Model.Under28,new {@class="numeric",size="10",maxlength="10"}) %></td>
        </tr>
        <tr>
            <th>30ヶ月未満</th>
            <td><%=Html.TextBox("Under30",Model.Under30,new {@class="numeric",size="10",maxlength="10"}) %></td>
        </tr>
        <tr>
            <th>36ヶ月未満</th>
            <td><%=Html.TextBox("Under36",Model.Under36,new {@class="numeric",size="10",maxlength="10"}) %></td>
        </tr>
        <tr>
            <th>72ヶ月未満</th>
            <td><%=Html.TextBox("Under72",Model.Under72,new {@class="numeric",size="10",maxlength="10"}) %></td>
        </tr>
        <tr>
            <th>84ヶ月未満</th>
            <td><%=Html.TextBox("Under84",Model.Under84,new {@class="numeric",size="10",maxlength="10"}) %></td>
        </tr>
        <tr>
            <th>84ヶ月以上</th>
            <td><%=Html.TextBox("Over84",Model.Over84,new {@class="numeric",size="10",maxlength="10"}) %></td>
        </tr>
    </table>
</div>
</div>
<div id="Color" style="<%=!(bool)ViewData["ColorDisplay"] ? "display:none" : "display:block"%>">
<br />

    <table class="input-form-line">
        <tr>
            <th nowrap style="width:20px;height:21px;padding:0px"><img alt="行追加" src="/Content/Images/plus.gif" style="cursor:pointer" onclick="$('#DelLine').val('-1');document.forms[0].action='/CarGrade/EditLine';formSubmit();" /></th>
            <th nowrap style="width:110px">車両カラーコード</th>
            <th nowrap style="width:400px">車両カラー名</th>
        </tr>
    </table>
    <div style="overflow-y:scroll;height:500px;width:563px">
    <table class="input-form-line">
        <%for(int i=0;i<Model.CarAvailableColor.Count();i++){
              string prefix = string.Format("availableColor[{0}].", i);
              var item = Model.CarAvailableColor[i];
              //2014/05/29 vs2012対応 arc yano 各コントロールにid追加
              string idprefix = string.Format("availableColor[{0}]_", i);
        %>
        <tr>
            <th nowrap style="width:20px;height:21px;padding:0px">                           
                <img alt="行削除" src="/Content/Images/minus.gif" style="cursor:pointer" onclick="$('#DelLine').val('<%=i %>');document.forms[0].action='/CarGrade/EditLine';formSubmit();" />
            </th>
            <td nowrap style="width:116px">
                <% // Mod 2014/07/24 arc amii 既存バグ対応 コードに対応する名称を取得するイベント処理を追加 %>
                <%=Html.TextBox(prefix + "CarColorCode", item.CarColorCode, new { id = idprefix + "CarColorCode", @class = "alphanumeric", style = "width:80px" , onblur = "GetNameFromCode('" + idprefix + "CarColorCode','" + idprefix + "CarColorName','CarColor')" }) %>
                <% //Mod 2014/07/15 arc yano chrome対応 パラメータをname→idに変更 %>
                <%Html.RenderPartial("SearchButtonControl", new string[] { idprefix + "CarColorCode", idprefix + "CarColorName", "'/CarColor/CriteriaDialog'", "0" }); %>
            </td>
            <td nowrap style="width:400px">
                <%=Html.TextBox(prefix + "CarColorName", item.CarColor != null ? item.CarColor.CarColorName : "", new { id = idprefix + "CarColorName", @class = "readonly", style = "width:400px" }) %>
            </td>
        </tr>
        <%} %>
    </table>
    </div>
</div>
<%} %>
<br />
</asp:Content>
