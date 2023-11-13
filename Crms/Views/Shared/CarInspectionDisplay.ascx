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
                        <%=Html.DropDownList(Model.Prefix + "IssueDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["IssueGengouList"], new { @disabled = "disabled" })%>
                        <%=Html.Hidden(Model.Prefix + "IssueDateWareki.Gengou", Model.IssueDateWareki != null ? Model.IssueDateWareki.Gengou : null) %>
                        <%=Html.TextBox(Model.Prefix + "IssueDateWareki.Year", Model.IssueDateWareki != null ? Model.IssueDateWareki.Year : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>年
                        <%=Html.TextBox(Model.Prefix + "IssueDateWareki.Month", Model.IssueDateWareki != null ? Model.IssueDateWareki.Month : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly ="readonly" })%>月
                        <%=Html.TextBox(Model.Prefix + "IssueDateWareki.Day", Model.IssueDateWareki != null ? Model.IssueDateWareki.Day : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>日
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
                                    <%=Html.TextBox(Model.Prefix + "MorterViecleOfficialCode", Model.MorterViecleOfficialCode, new { style = "width:50px", maxlength = 5, title = "陸自局", @class = "readonly", @readonly = "readonly" })%>
                                    <%=Html.TextBox(Model.Prefix + "RegistrationNumberType", Model.RegistrationNumberType, new { @class = "alphanumeric readonly", style = "width:30px", maxlength = 3, title="種別",@readonly="readonly" })%>
                                    <%=Html.TextBox(Model.Prefix + "RegistrationNumberKana", Model.RegistrationNumberKana, new { style = "width:15px", maxlength = 1, title = "かな", @readonly = "readonly", @class = "readonly" })%>
                                    <%=Html.TextBox(Model.Prefix + "RegistrationNumberPlate", Model.RegistrationNumberPlate, new { @class = "alphanumeric readonly", style = "width:50px", maxlength = 4, title = "プレート番号", @readonly = "readonly" })%>
                                </td>
                                <td colspan="3">
                                    <%=Html.DropDownList(Model.Prefix + "RegistrationDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["RegistrationGengouList"], new { @disabled = "disabled" })%>
                                    <%=Html.Hidden(Model.Prefix + "RegistrationDateWareki.Gengou", Model.RegistrationDateWareki != null ? Model.RegistrationDateWareki.Gengou : null) %>
                                    <%=Html.TextBox(Model.Prefix + "RegistrationDateWareki.Year", Model.RegistrationDateWareki != null ? Model.RegistrationDateWareki.Year : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2,@readonly="readonly" })%>年
                                    <%=Html.TextBox(Model.Prefix + "RegistrationDateWareki.Month", Model.RegistrationDateWareki != null ? Model.RegistrationDateWareki.Month : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>月
                                    <%=Html.TextBox(Model.Prefix + "RegistrationDateWareki.Day", Model.RegistrationDateWareki != null ? Model.RegistrationDateWareki.Day : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>日
                                </td>
                                <td colspan="2">
                                    <%=Html.DropDownList(Model.Prefix + "FirstRegistrationDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["FirstRegistrationGengouList"], new { @disabled = "disabled" })%>
                                    <%=Html.Hidden(Model.Prefix + "FirstRegistrationDateWareki.Gengou", Model.FirstRegistrationDateWareki != null ? Model.FirstRegistrationDateWareki.Gengou : null) %>
                                    <%=Html.TextBox(Model.Prefix + "FirstRegistrationDateWareki.Year", Model.FirstRegistrationDateWareki != null ? Model.FirstRegistrationDateWareki.Year : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>年
                                    <%=Html.TextBox(Model.Prefix + "FirstRegistrationDateWareki.Month", Model.FirstRegistrationDateWareki != null ? Model.FirstRegistrationDateWareki.Month : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>月
                                </td>
                                <td>
                                    <%=Html.DropDownList(Model.Prefix + "CarClassification", (IEnumerable<SelectListItem>)ViewData["CarClassificationList"], new { style = "width:100%", @disabled = "disabled" })%>
                                    <%=Html.Hidden(Model.Prefix + "CarClassification", Model.CarClassification) %>
                                </td>
                                <td>
                                    <%=Html.DropDownList(Model.Prefix + "Usage", (IEnumerable<SelectListItem>)ViewData["UsageList"], new { style = "width:100%", @disabled = "disabled" })%>
                                    <%=Html.Hidden(Model.Prefix + "Usage", Model.Usage) %>
                                </td>
                                <td>
                                    <%=Html.DropDownList(Model.Prefix + "UsageType", (IEnumerable<SelectListItem>)ViewData["UsageTypeList"], new { style = "width:100%", @disabled = "disabled" })%>
                                    <%=Html.Hidden(Model.Prefix + "UsageType", Model.UsageType) %>
                                </td>
                                <td colspan="4">
                                    <%=Html.DropDownList(Model.Prefix + "Figure", (IEnumerable<SelectListItem>)ViewData["FigureList"], new { style = "width:100%", @disabled = "disabled" })%>
                                    <%=Html.Hidden(Model.Prefix + "Figure", Model.Figure) %>
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
                                    <%=Html.TextBox(Model.Prefix + "MakerName", Model.MakerName, new { style = "width:462px", maxlength = 50, @class = "readonly" , @readonly = "readonly" })%>
                                </td>
                                <td>
                                    <%=Html.TextBox(Model.Prefix + "Capacity", Model.Capacity, new { @class = "numeric readonly", style = "width:130px", maxlength = 9, @readonly = "readonly" })%> 人
                                </td>
                                <td colspan="2">
                                    <%=Html.TextBox(Model.Prefix + "MaximumLoadingWeight", Model.MaximumLoadingWeight, new { @class = "numeric readonly", style = "width:208px", maxlength = 9, @readonly = "readonly" })%> kg
                                </td>
                                <td colspan="2">
                                    <%=Html.TextBox(Model.Prefix + "CarWeight", Model.CarWeight, new { @class = "numeric readonly", style = "width:93px", maxlength = 9, @readonly = "readonly" })%> kg
                                </td>
                                <td colspan="2">
                                    <%=Html.TextBox(Model.Prefix + "TotalCarWeight", Model.TotalCarWeight, new { @class = "numeric readonly", style = "width:93px", maxlength = 9, @readonly = "readonly" })%> kg
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
                                    <%=Html.TextBox(Model.Prefix + "Vin", Model.Vin, new { style = "width:462px", maxlength = 20, @class = "readonly", @readonly = "readonly" })%>
                                </td>
                                <td>
                                    <%=Html.TextBox(Model.Prefix + "Length", Model.Length, new { @class = "numeric readonly", style = "width:130px", maxlength = 9, @readonly = "readonly" })%> cm
                                </td>
                                <td>
                                    <%=Html.TextBox(Model.Prefix + "Width", Model.Width, new { @class = "numeric readonly", style = "width:80px", maxlength = 9, @readonly = "readonly" })%> cm
                                </td>
                                <td>
                                    <%=Html.TextBox(Model.Prefix + "Height", Model.Height, new { @class = "numeric readonly", style = "width:101px", maxlength = 9, @readonly = "readonly" })%> cm
                                </td>
                                <td>
                                    <%=Html.TextBox(Model.Prefix + "FFAxileWeight", Model.FFAxileWeight, new { @class = "numeric readonly", style = "width:33px", maxlength = 9, @readonly = "readonly" })%> kg
                                </td>
                                <td>
                                    <%=Html.TextBox(Model.Prefix + "FRAxileWeight", Model.FRAxileWeight, new { @class = "numeric readonly", style = "width:33px", maxlength = 9, @readonly = "readonly" })%> kg
                                </td>
                                <td>
                                    <%=Html.TextBox(Model.Prefix + "RFAxileWeight", Model.RFAxileWeight, new { @class = "numeric readonly", style = "width:33px", maxlength = 9, @readonly = "readonly" })%> kg
                                </td>
                                <td>
                                    <%=Html.TextBox(Model.Prefix + "RRAxileWeight", Model.RRAxileWeight, new { @class = "numeric readonly", style = "width:33px", maxlength = 9, @readonly = "readonly" })%> kg
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
                                    <%=Html.TextBox(Model.Prefix + "ModelName", Model.ModelName, new { style = "width:250px", maxlength = 20, @class = "readonly" ,@readonly = "readonly" })%>
                                </td>
                                <td colspan="4">
                                    <%//漢字入力対応 MOD 2014/10/16 arc ishii%>
                                    <%//<%=Html.TextBox(Model.Prefix + "EngineType", Model.EngineType, new { @class = "alphanumeric readonly", style = "width:205px", maxlength = 10, @readonly = "readonly" })%>
                                    <%=Html.TextBox(Model.Prefix + "EngineType", Model.EngineType, new { @class = "readonly", style = "width:205px", maxlength = 25, @readonly = "readonly" })%><%//Mod 2020/11/27 yano #4072 15 -> 25%><%//Mod  2019/5/23 yano #3992%>            
                                </td>
                                <td>
                                    <p style="float:left"><%=Html.TextBox(Model.Prefix + "Displacement", Model.Displacement, new { @class = "numeric readonly", style = "width:130px", maxlength = 9, @readonly = "readonly" })%></p><p style="text-align:right">kw</p><p style="text-align:right">L</p><%//2018/06/22 arc yano  %>
                                </td>
                                <td colspan="2">
                                    <%=Html.DropDownList(Model.Prefix + "Fuel",(IEnumerable<SelectListItem>)ViewData["FuelList"], new { style = "width:228px", @disabled = "disabled" })%>
                                    <%=Html.Hidden(Model.Prefix + "Fuel",Model.Fuel)%>
                                </td>
                                <td colspan="2">
                                    <%=Html.TextBox(Model.Prefix + "ModelSpecificateNumber", Model.ModelSpecificateNumber, new { @class = "alphanumeric readonly", style = "width:113px", maxlength = 10, @readonly = "readonly" })%>
                                </td>
                                <td colspan="2">
                                    <%=Html.TextBox(Model.Prefix + "ClassificationTypeNumber", Model.ClassificationTypeNumber, new { @class = "alphanumeric readonly", style = "width:113px", maxlength = 10, @readonly = "readonly" })%>
                                </td>
                            </tr>
                            <tr>
                                <th>
                                    所有者の氏名又は名称
                                </th>
                                <td colspan="18">
                                    <%=Html.TextBox(Model.Prefix + "OwnerCode", Model.OwnerCode, new { @class = "alphanumeric readonly", style = "width:80px", maxlength = 10, @readonly = "readonly" })%>
                                    <%Html.RenderPartial("SearchButtonControl", new string[] { Model.Prefix + "OwnerCode", Model.Prefix + "PossesorName", "'/Customer/CriteriaDialog'", "1", "GetMasterDetailFromCode('" + Model.Prefix + "OwnerCode',new Array('" + Model.Prefix + "PossesorName','" + Model.Prefix + "PossesorAddress'),'Customer')" }); %>
                                    <%=Html.TextBox(Model.Prefix + "PossesorName", Model.PossesorName, new { style = "width:779px", maxlength = 40, @class = "readonly", @readonly = "readonly" })%>
                                </td>
                            </tr>
                            <tr>
                                <th>
                                    所有者の住所
                                </th>
                                <td colspan="18">
                                    <%=Html.TextBox(Model.Prefix + "PossesorAddress", Model.PossesorAddress, new { style = "width:890px", maxlength = 300, @class = "readonly", @readonly = "readonly" })%>
                                </td>
                            </tr>
                            <tr>
                                <th>
                                    使用者の氏名又は名称
                                </th>
                                <td colspan="18">
                                    <%=Html.TextBox(Model.Prefix + "UserCode", Model.UserCode, new { @class = "alphanumeric readonly", style = "width:80px", maxlength = 10, @readonly = "readonly" })%>
                                    <%Html.RenderPartial("SearchButtonControl", new string[] { Model.Prefix + "UserCode", Model.Prefix + "UserName", "'/Customer/CriteriaDialog'", "1", "GetMasterDetailFromCode('" + Model.Prefix + "UserCode',new Array('" + Model.Prefix + "UserName','" + Model.Prefix + "UserAddress'),'Customer')" }); %>
                                    <%=Html.TextBox(Model.Prefix + "UserName", Model.UserName, new { style = "width:779px", maxlength = 40, @class = "readonly", @readonly = "readonly" })%>
                                </td>
                            </tr>
                            <tr>
                                <th>
                                    使用者の住所
                                </th>
                                <td colspan="18">
                                    <%=Html.TextBox(Model.Prefix + "UserAddress", Model.UserAddress, new { style = "width:890px", maxlength = 300, @class = "readonly", @readonly = "readonly" })%>
                                </td>
                            </tr>
                            <tr>
                                <th>
                                    使用者の本拠の位置
                                </th>
                                <td colspan="18">
                                    <%=Html.TextBox(Model.Prefix + "PrincipalPlace", Model.PrincipalPlace, new { style = "width:890px", maxlength = 300, @class = "readonly", @readonly = "readonly" })%>
                                </td>
                            </tr>
                            <tr>
                                <th>
                                    有効期間の満了する日
                                </th>
                                <td colspan="18">
                                    <%=Html.Hidden(Model.Prefix + "ExpireType","001") %>
                                    <%=Html.DropDownList(Model.Prefix + "ExpireDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["ExpireGengouList"], new { @disabled = "disabled" })%><%//Mod 2019/05/15 yano #3989%>
                                    <%=Html.Hidden(Model.Prefix + "ExpireDateWareki.Gengou", Model.ExpireDateWareki != null ? Model.ExpireDateWareki.Gengou : null) %>
                                    <%=Html.TextBox(Model.Prefix + "ExpireDateWareki.Year", Model.ExpireDateWareki != null ? Model.ExpireDateWareki.Year : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2,@readonly="readonly" })%>年
                                    <%=Html.TextBox(Model.Prefix + "ExpireDateWareki.Month", Model.ExpireDateWareki != null ? Model.ExpireDateWareki.Month : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>月
                                    <%=Html.TextBox(Model.Prefix + "ExpireDateWareki.Day", Model.ExpireDateWareki != null ? Model.ExpireDateWareki.Day : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>日
                                </td>
                            </tr>
                            <tr>
                                <th>
                                    備考
                                </th>
                                <td colspan="18">
                                    <% // Mod 2014/07/22 arc amii chromeでDB登録する際、改行コードも文字として登録してしまうのを修正 %>
                                    <%=Html.TextArea(Model.Prefix + "Memo", Model.Memo, 3, 56, new { style = "width:890px;height:40px", wrap = "virtual", @readonly = "readonly", @class = "readonly" })%>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>
