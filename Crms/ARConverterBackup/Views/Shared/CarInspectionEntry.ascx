<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.SalesCar>" %>
<table class="input-form-slim">
    <tr>
        <th class="input-form-title">
           車検証情報
        </th>
    </tr>
    <tr>
        <td>
        <table style="border:solid 1px black;background-color:White">
    <tr>
        <td style="text-align:left;width:750px">
            <span style="font-weight:bold;font-size:large;padding:0px 0px 3px 0px">　　自 動 車 検 査 証</span>
        </td>
        <td style="text-align:right;width:100px">発行日</td>
        <td style="text-align:right;width:200px">
            <%  //Add 2014/07/22 arc yano chrome対応 id追加
                string idPrefix = "";

                if (Model.Prefix != null)
                {
                    idPrefix = Model.Prefix.Replace(".", "_");
                }  
            %>

            <%=Html.DropDownList(Model.Prefix + "IssueDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["IssueGengouList"], new { id = idPrefix + "IssueDateWareki.Gengou" })%>
            <%=Html.TextBox(Model.Prefix + "IssueDateWareki.Year", Model.IssueDateWareki != null ? Model.IssueDateWareki.Year : null, new { id = idPrefix + "IssueDateWareki.Year", @class = "numeric", style = "width:15px", maxlength = 2 })%>年
            <%=Html.TextBox(Model.Prefix + "IssueDateWareki.Month", Model.IssueDateWareki != null ? Model.IssueDateWareki.Month : null, new {  id = idPrefix + "IssueDateWareki.Month", @class = "numeric", style = "width:15px", maxlength = 2 })%>月
            <%=Html.TextBox(Model.Prefix + "IssueDateWareki.Day", Model.IssueDateWareki != null ? Model.IssueDateWareki.Day : null, new { id = idPrefix + "IssueDateWareki.Day", @class = "numeric", style = "width:15px", maxlength = 2 })%>日
        </td>
    </tr>
    <tr>
        <td style="padding:5px 5px 5px 5px" colspan="3">
            <table class="inspection">
                <tr>
                    <th style="width:200px">
                        自動車登録番号又は車両番号
                    </th>
                    <th colspan="3" style="width:150px">
                        登録年月日／交付年月日
                    </th>
                    <th colspan="2" style="width:100px">
                        初度登録年月
                    </th>
                    <th style="width:150px">
                        自動車の種別
                    </th>
                    <th style="width:100px">
                        用途
                    </th>
                    <th style="width:120px">
                        自家用・事業用の別
                    </th>
                    <th colspan="4" style="width:220px">
                        車体の形状
                    </th>
                </tr>
                <tr>
                    <td>
                        <%=Html.TextBox(Model.Prefix + "MorterViecleOfficialCode", Model.MorterViecleOfficialCode, new { id = idPrefix + "MorterViecleOfficialCode", style = "width:50px", maxlength = 5, title = "陸自局" }) %>
                        <%=Html.TextBox(Model.Prefix + "RegistrationNumberType", Model.RegistrationNumberType, new { id = idPrefix + "RegistrationNumberType", @class = "alphanumeric", style = "width:30px", maxlength = 3, title="種別" })%>
                        <%=Html.TextBox(Model.Prefix + "RegistrationNumberKana", Model.RegistrationNumberKana, new { id = idPrefix + "RegistrationNumberKana", style = "width:15px", maxlength = 1 , title="かな"}) %>
                        <%=Html.TextBox(Model.Prefix + "RegistrationNumberPlate", Model.RegistrationNumberPlate, new { id = idPrefix + "RegistrationNumberPlate", @class = "alphanumeric", style = "width:50px", maxlength = 4, title="プレート番号" }) %>
                    </td>
                    <td colspan="3">
                        <%=Html.DropDownList(Model.Prefix + "RegistrationDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["RegistrationGengouList"], new { id = idPrefix + "RegistrationDateWareki.Gengou"})%>
                        <%=Html.TextBox(Model.Prefix + "RegistrationDateWareki.Year", Model.RegistrationDateWareki != null ? Model.RegistrationDateWareki.Year : null, new { id = idPrefix + "RegistrationDateWareki.Year", @class = "numeric", style = "width:15px", maxlength = 2 })%>年
                        <%=Html.TextBox(Model.Prefix + "RegistrationDateWareki.Month", Model.RegistrationDateWareki != null ? Model.RegistrationDateWareki.Month : null, new { id = idPrefix + "RegistrationDateWareki.Month", @class = "numeric", style = "width:15px", maxlength = 2 })%>月
                        <%=Html.TextBox(Model.Prefix + "RegistrationDateWareki.Day", Model.RegistrationDateWareki != null ? Model.RegistrationDateWareki.Day : null, new { id = idPrefix + "RegistrationDateWareki.Day", @class = "numeric", style = "width:15px", maxlength = 2 })%>日
                    </td>
                    <td colspan="2">
                        <%=Html.DropDownList(Model.Prefix + "FirstRegistrationDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["FirstRegistrationGengouList"], new { id = idPrefix + "FirstRegistrationDateWareki.Gengou"})%>
                        <%=Html.TextBox(Model.Prefix + "FirstRegistrationDateWareki.Year", Model.FirstRegistrationDateWareki != null ? Model.FirstRegistrationDateWareki.Year : null, new { id = idPrefix + "FirstRegistrationDateWareki.Year", @class = "numeric", style = "width:15px", maxlength = 2 })%>年
                        <%=Html.TextBox(Model.Prefix + "FirstRegistrationDateWareki.Month", Model.FirstRegistrationDateWareki != null ? Model.FirstRegistrationDateWareki.Month : null, new { id = idPrefix + "FirstRegistrationDateWareki.Month", @class = "numeric", style = "width:15px", maxlength = 2 })%>月
                    </td>
                    <td>
                        <%=Html.DropDownList(Model.Prefix + "CarClassification", (IEnumerable<SelectListItem>)ViewData["CarClassificationList"], new { id = idPrefix + "CarClassification", style = "width:100%" })%>
                    </td>
                    <td>
                        <%=Html.DropDownList(Model.Prefix + "Usage", (IEnumerable<SelectListItem>)ViewData["UsageList"], new { id = idPrefix + "Usage", style = "width:100%" })%>
                    </td>
                    <td>
                        <%=Html.DropDownList(Model.Prefix + "UsageType", (IEnumerable<SelectListItem>)ViewData["UsageTypeList"], new { id = idPrefix + "UsageType", style = "width:100%" })%>
                    </td>
                    <td colspan="4">
                        <%=Html.DropDownList(Model.Prefix + "Figure", (IEnumerable<SelectListItem>)ViewData["FigureList"], new { id = idPrefix + "Figure", style = "width:100%" })%>
                    </td>
                </tr>
                <tr>
                    <th colspan="6" style="width:462px">
                        車　　　　　名
                    </th>
                    <th style="width:150px">
                        乗車定員
                    </th>
                    <th colspan="2" style="width:228px">
                        最大積載量
                    </th>
                    <th colspan="2" style="width:113px">
                        車両重量
                    </th>
                    <th colspan="2" style="width:113px">
                        車両総重量
                    </th>
                </tr>
                <tr>
                    <td colspan="6">
                        <%=Html.TextBox(Model.Prefix + "MakerName", Model.MakerName, new { id = idPrefix + "MakerName", style = "width:462px", maxlength = 50 })%>
                    </td>
                    <td>
                        <%=Html.TextBox(Model.Prefix + "Capacity", Model.Capacity, new { id = idPrefix + "Capacity", @class = "numeric", style = "width:130px", maxlength = 9 })%> 人
                    </td>
                    <td colspan="2">
                        <%=Html.TextBox(Model.Prefix + "MaximumLoadingWeight", Model.MaximumLoadingWeight, new { id = idPrefix + "MaximumLoadingWeight", @class = "numeric", style = "width:208px", maxlength = 9 })%> kg
                    </td>
                    <td colspan="2">
                        <%=Html.TextBox(Model.Prefix + "CarWeight", Model.CarWeight, new { id = idPrefix + "CarWeight", @class = "numeric", style = "width:93px", maxlength = 9, onchange = "calcRRAxileWeight()" })%> kg
                    </td>
                    <td colspan="2">
                        <%=Html.TextBox(Model.Prefix + "TotalCarWeight", Model.TotalCarWeight, new { id = idPrefix + "TotalCarWeight", @class = "numeric", style = "width:93px", maxlength = 9 })%> kg
                    </td>
                </tr>
                <tr>
                    <th colspan="6" style="width:462px">
                        車　台　番　号
                    </th>
                    <th>
                        長さ
                    </th>
                    <th>
                        幅
                    </th>
                    <th>
                        高さ
                    </th>
                    <th style="width:55px">
                        前前軸重
                    </th>
                    <th style="width:55px">
                        前後軸重
                    </th>
                    <th style="width:55px">
                        後前軸重
                    </th>
                    <th style="width:55px">
                        後後軸重
                    </th>
                </tr>
                <tr>
                    <td colspan="6">
                        <%=Html.TextBox(Model.Prefix + "Vin", Model.Vin, new { id = idPrefix + "Vin", style = "width:462px", maxlength = 20 })%>
                    </td>
                    <td>
                        <%=Html.TextBox(Model.Prefix + "Length", Model.Length, new { id = idPrefix + "Length", @class = "numeric", style = "width:130px", maxlength = 9 })%> cm
                    </td>
                    <td>
                        <%=Html.TextBox(Model.Prefix + "Width", Model.Width, new { id = idPrefix + "Width", @class = "numeric", style = "width:80px", maxlength = 9 })%> cm
                    </td>
                    <td>
                        <%=Html.TextBox(Model.Prefix + "Height", Model.Height, new { id = idPrefix + "Height", @class = "numeric", style = "width:101px", maxlength = 9 })%> cm
                    </td>
                    <td>
                        <%=Html.TextBox(Model.Prefix + "FFAxileWeight", Model.FFAxileWeight, new { id = idPrefix + "FFAxileWeight", @class = "numeric", style = "width:33px", maxlength = 9, onchange="calcRRAxileWeight()" })%> kg
                    </td>
                    <td>
                        <%=Html.TextBox(Model.Prefix + "FRAxileWeight", Model.FRAxileWeight, new { id = idPrefix + "FRAxileWeight", @class = "numeric", style = "width:33px", maxlength = 9 })%> kg
                    </td>
                    <td>
                        <%=Html.TextBox(Model.Prefix + "RFAxileWeight", Model.RFAxileWeight, new { id = idPrefix + "RFAxileWeight", @class = "numeric", style = "width:33px", maxlength = 9 })%> kg
                    </td>
                    <td>
                        <%=Html.TextBox(Model.Prefix + "RRAxileWeight", Model.RRAxileWeight, new { id = idPrefix + "RRAxileWeight", @class = "numeric", style = "width:33px", maxlength = 9 })%> kg
                    </td>
                </tr>
                <tr>
                    <th colspan="2" style="width:250px">
                        型　　式
                    </th>
                    <th colspan="4">
                        原動機の型式
                    </th>
                    <th>
                        総排気量又は定格出力
                    </th>
                    <th colspan="2">
                        燃料の種類
                    </th>
                    <th colspan="2" style="width:113px">
                        型式指定番号
                    </th>
                    <th colspan="2" style="width:113px">
                        類別区分番号
                    </th>
                </tr>
                <tr>
                    <td colspan="2">
                        <%=Html.TextBox(Model.Prefix + "ModelName", Model.ModelName, new { id = idPrefix + "ModelName", style = "width:250px", maxlength = 20 })%>
                    </td>
                    <td colspan="4">
                    	<%//漢字入力対応 MOD 2014/10/16 arc ishii%>
                        <%=Html.TextBox(Model.Prefix + "EngineType", Model.EngineType, new { id = idPrefix + "EngineType", style = "ime-mode:inactive; width:205px", maxlength = 25 })%>><%//Mod 2020/11/27 yano #4072 15 -> 25%><%//Mod  2019/5/23 yano #3992%>
                    </td>
                    <td>
                        <p style="float:left"><%=Html.TextBox(Model.Prefix + "Displacement", Model.Displacement, new { id = idPrefix + "Displacement", @class = "numeric", style = "width:130px", maxlength = 9 })%></p><p style="text-align:right">kw</p><p style="text-align:right">L</p><%//2018/06/22 arc yano  %>
                    </td>
                    <td colspan="2">
                        <%=Html.DropDownList(Model.Prefix + "Fuel", (IEnumerable<SelectListItem>)ViewData["FuelList"], new { id = idPrefix + "Fuel", style = "width:228px"})%>
                    </td>
                    <td colspan="2">
                        <%=Html.TextBox(Model.Prefix + "ModelSpecificateNumber", Model.ModelSpecificateNumber, new { id = idPrefix + "ModelSpecificateNumber", @class = "alphanumeric", style = "width:113px", maxlength = 10 })%>
                    </td>
                    <td colspan="2">
                        <%=Html.TextBox(Model.Prefix + "ClassificationTypeNumber", Model.ClassificationTypeNumber, new { id = idPrefix + "ClassificationTypeNumber", @class = "alphanumeric", style = "width:113px", maxlength = 10 })%>
                    </td>
                </tr>
                <tr>
                    <th>
                        所有者の氏名又は名称
                    </th>
                    <td colspan="18">
                        <%--//MOD 2015/02/13 arc yano 車両マスタの「車検案内」デフォルト設定 顧客コードによって、車検案内発送備考欄の可否を制御--%> 
                        <%--//MOD 2015/02/09 arc ishii #3147 車両マスタの「車検案内」デフォルト設定 顧客コードによって、車検案内の可否を制御--%>
                        <%--//Mod 2015/07/02 arc nakayama #3163_車両マスタで所有者名と使用者名が入力出来る状態になっている 使用者名また所有者名をリードオンリーにする--%>
                        <%--<%=Html.TextBox(Model.Prefix + "OwnerCode", Model.OwnerCode, new { id = idPrefix + "OwnerCode", @class = "alphanumeric", style = "width:80px", maxlength = 10, onchange = "if(confirm('入力されたコードで名前・住所を取得しますか？')){GetMasterDetailFromCode('" + idPrefix + "OwnerCode',new Array('" + idPrefix + "PossesorName','" + idPrefix + "PossesorAddress'),'Customer');}"})%>--%>
                        <%=Html.TextBox(Model.Prefix + "OwnerCode", Model.OwnerCode, new { id = idPrefix + "OwnerCode", @class = "alphanumeric", style = "width:80px", maxlength = 10, onchange = "if(confirm('入力されたコードで名前・住所を取得しますか？')){GetMasterDetailFromCode('" + idPrefix + "OwnerCode',new Array('" + idPrefix + "PossesorName','" + idPrefix + "PossesorAddress'),'Customer');} chkInspect('" + idPrefix + "OwnerCode','" + idPrefix + "InspectGuidFlag'); setInspectGuidMemo('" + idPrefix + "OwnerCode','" + idPrefix + "InspectGuidMemo')"})%>
                        <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "OwnerCode", idPrefix + "PossesorName", "'/Customer/CriteriaDialog'", "0", "GetMasterDetailFromCode('" + idPrefix + "OwnerCode',new Array('" + idPrefix + "PossesorName','" + idPrefix + "PossesorAddress'),'Customer'); chkInspect('" + idPrefix + "OwnerCode','" + idPrefix + "InspectGuidFlag') ; setInspectGuidMemo('" + idPrefix + "OwnerCode','" + idPrefix + "InspectGuidMemo')" }); %>
                        <%=Html.TextBox(Model.Prefix + "PossesorName", Model.PossesorName, new { id = idPrefix + "PossesorName", style = "width:779px", maxlength = 40 , @class = "readonly", @readOnly = "readOnly"})%>
                    </td>
                </tr>
                <tr>
                    <th>
                        所有者の住所
                    </th>
                    <td colspan="18">
                        <%--//Mod 2015/07/02 arc nakayama #3163_車両マスタで所有者名と使用者名が入力出来る状態になっている 所有者の住所をリードオンリーにする--%>
                        <%=Html.TextBox(Model.Prefix + "PossesorAddress", Model.PossesorAddress, new { id = idPrefix + "PossesorAddress", style = "width:890px", maxlength = 300, @class = "readonly", @readOnly = "readOnly" })%>
                    </td>
                </tr>
                <tr>
                    <th>
                        使用者の氏名又は名称
                    </th>
                    <td colspan="18">
                        <%--//MOD 2015/02/13 arc yano 車両マスタの「車検案内」デフォルト設定 顧客コードによって、車検案内発送備考欄の可否を制御--%> 
                        <%--//MOD 2015/02/09 arc ishii #3147 車両マスタの「車検案内」デフォルト設定 顧客コードによって、車検案内の可否を制御--%>
                        <%--//Mod 2015/07/02 arc nakayama #3163_車両マスタで所有者名と使用者名が入力出来る状態になっている 使用者名また所有者名をリードオンリーにする--%> 
                        <%--<%=Html.TextBox(Model.Prefix + "UserCode", Model.UserCode, new { id = idPrefix + "UserCode", @class = "alphanumeric", style = "width:80px", maxlength = 10, onchange = "if(confirm('入力されたコードで名前・住所を取得しますか？')){GetMasterDetailFromCode('" + Model.Prefix + "UserCode',new Array('" + Model.Prefix + "UserName','" + Model.Prefix + "UserAddress'),'Customer');}"})%>--%>
                        <%=Html.TextBox(Model.Prefix + "UserCode", Model.UserCode, new { id = idPrefix + "UserCode", @class = "alphanumeric", style = "width:80px", maxlength = 10, onchange = "if(confirm('入力されたコードで名前・住所を取得しますか？')){GetMasterDetailFromCode('" + idPrefix + "UserCode',new Array('" + idPrefix + "UserName','" + idPrefix + "UserAddress'),'Customer');} chkInspect('" + idPrefix + "UserCode','" + idPrefix + "InspectGuidFlag'); setInspectGuidMemo('" + idPrefix + "UserCode','" + idPrefix + "InspectGuidMemo')"})%><%//Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更 %>
                        <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "UserCode", idPrefix + "UserName", "'/Customer/CriteriaDialog'", "0", "GetMasterDetailFromCode('" + idPrefix + "UserCode',new Array('" + idPrefix + "UserName','" + idPrefix + "UserAddress'),'Customer');chkInspect('" + idPrefix + "UserCode','" + idPrefix + "InspectGuidFlag'); setInspectGuidMemo('" + idPrefix + "UserCode','" + idPrefix + "InspectGuidMemo') " }); %>
                        <%=Html.TextBox(Model.Prefix + "UserName", Model.UserName, new { id = idPrefix + "UserName", style = "width:779px", maxlength = 40, @class = "readonly", @readOnly = "readOnly"})%>
                    </td>
                </tr>
                <tr>
                    <th>
                        使用者の住所
                    </th>
                    <td colspan="18">
                        <%--//Mod 2015/07/02 arc nakayama #3163_車両マスタで所有者名と使用者名が入力出来る状態になっている 所有者の住所をリードオンリーにする--%>
                        <%=Html.TextBox(Model.Prefix + "UserAddress", Model.UserAddress, new { id = idPrefix + "UserAddress", style = "width:890px", maxlength = 300, @class = "readonly", @readOnly = "readOnly" })%>
                    </td>
                </tr>
                <tr>
                    <th>
                        使用者の本拠の位置
                    </th>
                    <td colspan="18">
                        <%=Html.TextBox(Model.Prefix + "PrincipalPlace", Model.PrincipalPlace, new { id = idPrefix + "PrincipalPlace", style = "width:890px", maxlength = 300 })%>
                    </td>
                </tr>
                <tr>
                    <th>
                        有効期間の満了する日
                    </th>
                    <td colspan="18">
                        <%=Html.Hidden(Model.Prefix + "ExpireType", "001", new { id = idPrefix + "ExpireType"})%>
                        <%=Html.DropDownList(Model.Prefix + "ExpireDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["ExpireGengouList"], new { id = idPrefix + "ExpireDateWareki.Gengou", onchange = "getNextInspectionDate('" + idPrefix + "')" })%><%//Mod 2019/05/15 yano #3989%><%//Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更 %>
                        <%=Html.TextBox(Model.Prefix + "ExpireDateWareki.Year", Model.ExpireDateWareki != null ? Model.ExpireDateWareki.Year : null, new { id = idPrefix + "ExpireDateWareki.Year", @class = "numeric", style = "width:15px", maxlength = 2, onchange = "getNextInspectionDate('" + idPrefix + "')" })%>年<%//Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更 %>
                        <%=Html.TextBox(Model.Prefix + "ExpireDateWareki.Month", Model.ExpireDateWareki != null ? Model.ExpireDateWareki.Month : null, new { id = idPrefix + "ExpireDateWareki.Month", @class = "numeric", style = "width:15px", maxlength = 2, onchange = "getNextInspectionDate('" + idPrefix + "')" })%>月<%//Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更 %>
                        <%=Html.TextBox(Model.Prefix + "ExpireDateWareki.Day", Model.ExpireDateWareki != null ? Model.ExpireDateWareki.Day : null, new { id = idPrefix + "ExpireDateWareki.Day", @class = "numeric", style = "width:15px", maxlength = 2, onchange = "getNextInspectionDate('" + idPrefix + "')" })%>日<%//Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更 %>
                    </td>
                </tr>
                <tr>
                    <th>
                        備考
                    </th>
                    <td colspan="18">
                        <% // Mod 2014/07/22 arc amii chromeでDB登録する際、改行コードも文字として登録してしまうのを修正 %>
                        <%=Html.TextArea(Model.Prefix + "Memo", Model.Memo, 3, 56, new { id = idPrefix + "Memo", style = "width:890px;height:40px", wrap = "virtual", onblur = "checkTextLength('"+idPrefix+"Memo', 255, '備考')" })%><%//Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更 %>
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>
        </td>
    </tr>
</table>
