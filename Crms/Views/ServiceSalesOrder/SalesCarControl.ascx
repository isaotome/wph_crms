<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.ServiceSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<table class="input-form-slim">
    <tr>
        <th colspan="10" class="input-form-title">
            車両情報
        </th>
    </tr>
    <tr>
        <th style="width: 80px">
            ブランド名
        </th>
        <td style="width: 100px">
            <%=Html.TextBox("CarBrandName", Model.CarBrandName, new { style = "width:94px", maxlength = "50", @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th style="width: 80px">
            車種名
        </th>
        <td style="width: 100px">
            <%=Html.TextBox("CarName", Model.CarName, new { style = "width:94px", maxlength = "50", @class="readonly", @readonly="readonly" })%>
        </td>
        <th style="width: 80px">
            グレードコード
        </th>
        <td style="width: 100px">
            <%//2014/07/11 chrome対応 グレードコードテキストボックスのwidthを72px→67pxに変更 %>
            <%//2015/10/30 nishimura.akira グレードコードからの選択を禁止にする サーチボタンをONにするには1->0に変更%>
            <%if(Model.CarEnabled){ %>
                <% //=Html.TextBox("CarGradeCode", Model.CarGradeCode, new { style = "width:67px", @class = "alphanumeric", maxlength = "30", onblur = "GetMasterDetailFromCode('CarGradeCode',new Array('CarBrandName','CarName','CarGradeName','LaborRate','ModelName','EngineType'),'CarGrade')" })%>
                <%=Html.TextBox("CarGradeCode", Model.CarGradeCode, new { style = "width:67px", @class = "alphanumeric readonly", @readonly="readonly", maxlength = "30" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CarGradeCode", "CarGradeName", "'/CarGrade/CriteriaDialog'", "1" }); %>
            <%}else{ %>
                <%=Html.TextBox("CarGradeCode", Model.CarGradeCode, new { style = "width:67px", @class = "alphanumeric readonly", @readonly="readonly", maxlength = "30" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CarGradeCode", "CarGradeName", "'/CarGrade/CriteriaDialog'", "1" }); %>
            <%} %>            
        </td>
        <th style="width: 100px">
            グレード名
        </th>
        <td colspan="3">
            <%=Html.TextBox("CarGradeName", Model.CarGradeName, new { style = "width:276px", maxlength = "50", @class = "readonly", @readonly = "readonly" })%>
        </td>
    </tr>
    <tr>
        <th>
            原動機型式
        </th>
        <td>
            <%=Html.TextBox("EngineType", Model.EngineType, new { style = "width:94px", maxlength = "25", @class = "readonly", @readonly = "readonly" })%><%//Mod 2020/11/27 yano #4072 15 -> 25%> <%//Mod  2019/5/23 yano #3992%>
        </td>
        <th>
            型式
        </th>
        <td>
            <%=Html.TextBox("ModelName", Model.ModelName, new { style = "width:94px", maxlength = "20", @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            初年度登録
        </th>
        <td>
            <%=Html.TextBox("FirstRegistration", Model.FirstRegistration, new { style = "width:94px", maxlength = "9", @class="readonly", @readonly="readonly" })%>
        </td>
        <!--2016/04/08 arc nakayama #3197サービス伝票の車検有効期限＆次回点検日を車両マスタへコピーする際の確認メッセージ機能  次回点検日と車検有効期限は編集不可にする-->
        <th>
            次回点検日
        </th>
        <td>
           <%=Html.TextBox("NextInspectionDate", string.Format("{0:yyyy/MM/dd}", Model.NextInspectionDate), new { style = "width:94px", @class = "alphanumeric readonly", @readonly="readonly", maxlength = "10" })%>
        </td>
        <th>
            車検有効期限
        </th>
        <td>
           <%=Html.TextBox("InspectionExpireDate", string.Format("{0:yyyy/MM/dd}", Model.InspectionExpireDate), new { style = "width:94px", @class = "alphanumeric readonly", maxlength = "10", @readonly = "readonly" })%>
        </td>
    </tr>
    <tr>
        <th>
            車台番号
        </th>
        <td colspan="3">
            <%if(Model.CarEnabled){ %>
                <% // Mod 2015/09/24 arc yano #3263 車台番号検索のダイアログで何も選ばず消すとリストボックスの項目が消える GetSalesCarInfoの引数変更につき、呼び出し元も変更する%>
                <% // Mod 2014/07/24 arc amii 既存バグ対応 車台番号から管理番号を取得し、車両情報を取得・表示するイベントを発せさせるよう修正 %>
                <%=Html.TextBox("Vin", Model.Vin, new { style = "width:130px", maxlength = "20", onchange = "GetSalesCarInfoForSalesOrder('Vin','SalesCarNumber', new Array('Vin', 'CarGradeCode','CarGradeName','CarBrandName','CarName','Mileage','MileageUnit','ModelName','SalesCarNumber','EngineType','FirstRegistration','NextInspectionDate','InspectionExpireDate','UsVin','MorterViecleOfficialCode','RegistrationNumberType','RegistrationNumberKana','RegistrationNumberPlate','CustomerCode','CustomerName','CustomerNameKana','CustomerAddress','LaborRate', 'CustomerTelNumber', 'CustomerMemo'), null, null, new Array('MasterForSalesCar', 'HistoryForSalesCar'), null, null, null, '2', dispCustomerInfo, dispCustomerInfo);" }) %><%//Mod 2022/07/06 yano #4145 %><%//Mod 2022/01/08 yano #4121 %>
                <%--<%=Html.TextBox("Vin", Model.Vin, new { style = "width:130px", maxlength = "20", onchange = "GetSalesCarInfoForSalesOrder('Vin','SalesCarNumber', new Array('Vin', 'CarGradeCode','CarGradeName','CarBrandName','CarName','Mileage','MileageUnit','ModelName','SalesCarNumber','EngineType','FirstRegistration','NextInspectionDate','InspectionExpireDate','UsVin','MorterViecleOfficialCode','RegistrationNumberType','RegistrationNumberKana','RegistrationNumberPlate','CustomerCode','CustomerName','CustomerNameKana','CustomerAddress','LaborRate', 'CustomerTelNumber'), null, null, new Array('MasterForSalesCar', 'HistoryForSalesCar'), null, null, null, '2');" }) %>--%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "SalesCarNumber", "Vin", "'/SalesCar/CriteriaDialog'", "0", "GetSalesCarMasterDetailFromCode('SalesCarNumber',new Array('Vin', 'CarGradeCode','CarGradeName','CarBrandName','CarName','Mileage','MileageUnit','ModelName','SalesCarNumber','EngineType','FirstRegistration','NextInspectionDate','InspectionExpireDate','UsVin','MorterViecleOfficialCode','RegistrationNumberType','RegistrationNumberKana','RegistrationNumberPlate','CustomerCode','CustomerName','CustomerNameKana','CustomerAddress','LaborRate', 'CustomerTelNumber', 'CustomerMemo'),'SalesCar', null, null, null, null, '2', dispCustomerInfo, dispCustomerInfo);" }); %><%//Mod 2022/07/06 yano #4145 %><%//Mod 2022/01/08 yano #4121 %>
                <%--<%Html.RenderPartial("SearchButtonControl", new string[] { "SalesCarNumber", "Vin", "'/SalesCar/CriteriaDialog'", "0", "GetSalesCarMasterDetailFromCode('SalesCarNumber',new Array('Vin', 'CarGradeCode','CarGradeName','CarBrandName','CarName','Mileage','MileageUnit','ModelName','SalesCarNumber','EngineType','FirstRegistration','NextInspectionDate','InspectionExpireDate','UsVin','MorterViecleOfficialCode','RegistrationNumberType','RegistrationNumberKana','RegistrationNumberPlate','CustomerCode','CustomerName','CustomerNameKana','CustomerAddress','LaborRate', 'CustomerTelNumber'),'SalesCar', null, null, null, null, '2');" }); %>--%>
            <%}else{ %>
                <%=Html.TextBox("Vin", Model.Vin, new { style = "width:130px", maxlength = "20", @class = "readonly", @readonly = "readonly", onchange = "GetSalesCarInfo('Vin','SalesCarNumber', new Array('CarGradeCode','CarGradeName','CarBrandName','CarName','Mileage','MileageUnit','ModelName','SalesCarNumber','EngineType','FirstRegistration','NextInspectionDate','InspectionExpireDate','UsVin','MorterViecleOfficialCode','RegistrationNumberType','RegistrationNumberKana','RegistrationNumberPlate','CustomerCode','CustomerName','CustomerNameKana','CustomerAddress','LaborRate', 'CustomerTelNumber'), null, null, new Array('MasterForSalesCar', 'HistoryForSalesCar'));" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "SalesCarNumber", "Vin", "'/SalesCar/CriteriaDialog'", "1" }); %>
           <%} %>
            <%
                //Mod 2015/09/11 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動 
                //                              ①マスタボタンをクリック時に車台番号が入っていない場合は、新規で車両マスタ画面を表示する 
                //                              ②ボタンidを追加                                   
            %>
            <%// #3255_サービス伝票画面の車両のマスタボタンで車両データ開閉後、伝票上の車両情報が消える マスタに存在していない場合、メッセージを出さず、入力内容をクリアしない%>
            <button type="button" id="MasterForSalesCar" style="width:60px;height:20px" onclick="if($('#SalesCarNumber').val()!=''){ var callback = function(){ GetMasterDetailFromCode('SalesCarNumber',new Array('CarGradeCode','CarGradeName','CarBrandName','CarName','Mileage','MileageUnit','ModelName','EngineType','FirstRegistration','NextInspectionDate','InspectionExpireDate','MorterViecleOfficialCode','RegistrationNumberType','RegistrationNumberKana','RegistrationNumberPlate','Vin'),'SalesCar','','','1')}; openModalDialogNotMask('/SalesCar/Entry/'+$('#SalesCarNumber').val(), null, null, null, null, callback);}else{openModalDialog2('/SalesCar/Entry/');}">マスタ</button><%//Mod 2022/07/28 yano #4146%>
            <%--<button type="button" id="MasterForSalesCar" style="width:60px;height:20px" onclick="if($('#SalesCarNumber').val()!=''){ openModalDialog2('/SalesCar/Entry/'+$('#SalesCarNumber').val());GetMasterDetailFromCode('SalesCarNumber',new Array('CarGradeCode','CarGradeName','CarBrandName','CarName','Mileage','MileageUnit','ModelName','EngineType','FirstRegistration','NextInspectionDate','InspectionExpireDate','MorterViecleOfficialCode','RegistrationNumberType','RegistrationNumberKana','RegistrationNumberPlate','Vin'),'SalesCar','','','1');}else{openModalDialog2('/SalesCar/Entry/');}">マスタ</button>--%>

            <%//Mod 2015/09/11 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動 履歴ボタンをクリック時に車台番号が入っていない場合は、メッセージダイアログを表示する %>
            <button type="button" id="HistoryForSalesCar" style="width:60px;height:20px" onclick="$('#Vin').val()!='' ? openModalDialog('/ServiceSalesOrder/CriteriaDialog?vin='+$('#Vin').val()) : alert('車台番号を入力して下さい'); return false">履歴</button>
        </td>
        <th>
            シリアル
        </th>
        <td>
            <%=Html.TextBox("UsVin", Model.UsVin, new { style = "width:94px", maxlength = 20, @class = "alphanumeric readonly", @readonly = "readonly" }) %>
        </td>
        <th>
            走行距離
        </th>
        <td colspan="3">
            <%if(Model.CarEnabled){ %>
                <%=Html.TextBox("Mileage", Model.Mileage, new { maxlength = "11", style = "width:94px", @class = "numeric" })%>&nbsp;
                <%=Html.DropDownList("MileageUnit", (IEnumerable<SelectListItem>)ViewData["MileageUnitList"], new { style = "width:50px;height:20px" })%>
            <%}else{ %>
                <!--Mod 2015/08/13 arc nakayama #3240_サービス伝票のレバレート、走行距離の単位、支払合計の表示不具合 ステータス納車済みで保存し再描画される時に、データがバインドされないため 名前と同じデータソース追加　（初回表示：DBの値<Model.c_MileageUnit.Name>　再描画：画面の値<Model.MileageUnitName>）-->
                <%=Html.TextBox("Mileage", Model.Mileage, new { maxlength = "11", style = "width:100px", @class = "numeric readonly", @readonly = "readonly" })%>&nbsp;
                <%=Html.TextBox("MileageUnitName", Model.MileageUnitName ?? Model.c_MileageUnit.Name, new { @class = "readonly", @readonly = "readonly", style = "width:50px;height:15px" })%>
                <%=Html.Hidden("MileageUnit",Model.MileageUnit) %>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            陸事局コード
        </th>
        <td>
            <%=Html.TextBox("MorterViecleOfficialCode", Model.MorterViecleOfficialCode, new { style = "width:94px", maxlength = "5", @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            登録番号(種別)
        </th>
        <td>
            <%=Html.TextBox("RegistrationNumberType", Model.RegistrationNumberType, new { style = "width:94px", maxlength = "3", @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            登録番号(かな)
        </th>
        <td>
            <%=Html.TextBox("RegistrationNumberKana", Model.RegistrationNumberKana, new { style = "width:94px", maxlength = "1", @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            登録番号(ﾌﾟﾚｰﾄ)
        </th>
        <td colspan="3">
            <%=Html.TextBox("RegistrationNumberPlate", Model.RegistrationNumberPlate, new { style = "width:94px", maxlength = "4", @class = "readonly", @readonly = "readonly" })%>
        </td>
    </tr>
    <tr>
        <th>
            メーカー出荷日
        </th>
        <td>
            <%=Html.TextBox("MakerShipmentDate", string.Format("{0:yyyy/MM/dd}", Model.MakerShipmentDate), new { @class = "readonly", @readonly = "readonly", style = "width:94px" })%>
        </td>
        <th>
            登録予定日
        </th>
        <td>
            <%=Html.TextBox("RegistrationPlanDate", string.Format("{0:yyyy/MM/dd}", Model.RegistrationPlanDate), new { @class = "readonly", @readonly = "readonly", style = "width:94px" })%>
        </td>
   <!--  DEL 2014/20/20 ookubo
        <th>
            納車予定日
        </th>
        <td>
            <%if(Model.CarEnabled){ %>
                <%=Html.TextBox("SalesPlanDate", string.Format("{0:yyyy/MM/dd}", Model.SalesPlanDate), new { @class = "alphanumeric", style = "width:94px", maxlength = "10" })%>
            <%}else{ %>
                <%=Html.TextBox("SalesPlanDate", string.Format("{0:yyyy/MM/dd}", Model.SalesPlanDate), new { @class = "alphanumeric readonly", @readonly = "readonly", style = "width:94px", maxlength = "10" })%>
            <%} %>
        </td>
        <td colspan="3">
    -->
        <th>
            管理番号
        </th>
   <!--  DEL 2014/20/20 ookubo
        <td colspan="3">
    -->
        <td style="width: 100px">
            <%=Html.TextBox("SalesCarNumber", Model.SalesCarNumber, new { @class = "readonly", @readonly = "readonly", style = "width:94px" })%>
        </td>
    </tr>
</table>
