<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarPurchase>" %>
<%@ Import Namespace="CrmsDao" %>
<% 
   // -------------------------------------------------------------------------------------------------------------------------------------
   // Mod 2018/08/28 yano #3922 車両管理表(タマ表) 機能改善② 入庫区分を必須項目に変更
   // Mod 2017/04/23 arc yano #3755 車両伝票入力　金額欄の入力時のカーソル位置の不具合 金額欄のイベントハンドラをonblur→onchangeに変更
   // Mod 2017/03/06 arc yano #3640 車両仕入入力の計算がおかしい問題の対応
   //                               各項目の消費税欄を非活性化
   //                               計算項目の補足説明　レイアウトの変更(仕入価格、車両本体価格を２行にしたことによるレイアウトの変更)
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // -------------------------------------------------------------------------------------------------------------------------------------
%>
    <table class="input-form-slim">
      <tr>
        <!--<th class="input-form-title" colspan="10">伝票情報</th>-->
        <th class="input-form-title" colspan="12">伝票情報</th>
      </tr>
      <tr>
        <th style="width:100px">入庫区分 *</th><%//Mod 2018/08/28 yano #3922 %>
        <td><%=Html.DropDownList("CarPurchaseType",(IEnumerable<SelectListItem>)ViewData["CarPurchaseTypeList"]) %></td>
        <th style="width:100px">伝票番号</th>
        <td style="width:120px"><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "numeric" })%></td>
        <th></th>
        <th>税抜価格</th>
        <th>消費税</th>
        <th>税込価格</th>
        <th></th>
        <th>税抜価格</th>
        <th>消費税</th>
        <th>税込価格</th>
      </tr>
      <tr>
        <th>部門 *</th>
        <td colspan="3"><%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric", size = 10, maxlength = 3, onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'", "0" }); %>
            <%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { @class = "readonly", @readonly = "readonly", style = "width:200px" })%>
        </td>
        <% //Mod 2014/07/15 arc yano chrome対応 type追加 %>
        <th rowspan="2">仕入価格(総額) * (z)<button type="button" style="width:30px;height:16px" onclick="calcTotalPurchaseAmount()"><span style="font-size:xx-small">計算</span></button><br/>(a)～(k) の合計 </th><%// Mod 2017/03/06 arc yano #3640 %>
        <td><%=Html.TextBox("Amount", string.Format("{0:N0}", Model.Amount), new { @class = "money", style = "width:60px" }) %></td>
        <td><%=Html.TextBox("TaxAmount", string.Format("{0:N0}", Model.TaxAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:60px" }) %></td>
        <td><%=Html.TextBox("TotalAmount", string.Format("{0:N0}", Model.TotalAmount), new { @class = "money", style = "width:60px" }) %></td>
        <th>その他価格 * (e)</th>
        <td><%=Html.TextBox("OthersPrice", string.Format("{0:N0}", Model.OthersPrice), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcOthersPrice(1)" })%></td>
        <td><%=Html.TextBox("OthersTax", string.Format("{0:N0}", Model.OthersTax), new { @class = "readonly money", @readonly = "readonly", style = "width:60px", maxlength = 11, onchange = "calcOthersPrice(2)" })%></td>        
        <td><%=Html.TextBox("OthersAmount", string.Format("{0:N0}", Model.OthersAmount), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcOthersPrice(3)" })%></td>
      </tr>
      <tr>
        <th>仕入担当者 *</th>
        <td colspan="3">                
            <%=Html.TextBox("EmployeeNumber", Model.Employee != null ? Model.Employee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" })%>
            <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "alphanumeric", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "EmployeeCode", "EmployeeName", "'/Employee/CriteriaDialog'", "0" }); %>
            <%=Html.TextBox("EmployeeName", ViewData["EmployeeName"], new { @class = "readonly", style = "width:150px", @readonly = "readonly" })%>
        </td>
        <!-- 仕入価格（２行目） -->
        <th colspan="3"></th>
        <th>メタリック価格 * (f)</th>
        <td><%=Html.TextBox("MetallicPrice", string.Format("{0:N0}", Model.MetallicPrice), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcMetallicPrice(1)" })%></td>
        <td><%=Html.TextBox("MetallicTax", string.Format("{0:N0}", Model.MetallicTax), new { @class = "readonly money", @readonly = "readonly", style = "width:60px", maxlength = 11, onchange = "calcMetallicPrice(2)" })%></td>
        <td><%=Html.TextBox("MetallicAmount", string.Format("{0:N0}", Model.MetallicAmount), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcMetallicPrice(3)" })%></td>
      </tr>
      <tr>
        <th>仕入先 *</th>
        <td colspan="3"><%=Html.TextBox("SupplierCode", Model.SupplierCode, new { @class = "alphanumeric", size = 10, maxlength = 10, onblur = "GetNameFromCode('SupplierCode','SupplierName','Supplier')" })%>
            <%Html.RenderPartial("SearchButtonControl",new string[]{"SupplierCode", "SupplierName", "'/Supplier/CriteriaDialog'","0"});%>
            <%=Html.TextBox("SupplierName", ViewData["SupplierName"], new { @class = "readonly", @readonly = "readonly", style = "width:200px" })%>
        </td>
        <% //Mod 2014/07/15 arc yano chrome対応 type追加 %>
        <th rowspan="2">車両本体価格 * (a)<button type="button" style="width:30px;height:16px" onclick="calcVehicleAmountFromTotal()"><span style="font-size:xx-small">計算</span></button><br/>(z)－( (b)～(k)の合計 )</th><%// Mod 2017/03/06 arc yano #3640 %>
        <td><%=Html.TextBox("VehiclePrice", string.Format("{0:N0}", Model.VehiclePrice), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcVehiecleAmount(1)" })%></td>
        <td><%=Html.TextBox("VehicleTax", string.Format("{0:N0}", Model.VehicleTax), new { @class = "readonly money", @readonly = "readonly", style = "width:60px", maxlength = 11, onchange = "calcVehiecleAmount(2)" })%></td>
        <td><%=Html.TextBox("VehicleAmount", string.Format("{0:N0}", Model.VehicleAmount), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcVehiecleAmount(3)" })%></td>
        <th>オプション価格 * (g)</th>
        <td><%=Html.TextBox("OptionPrice", string.Format("{0:N0}", Model.OptionPrice), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcOptionPrice(1)" })%></td>
        <td><%=Html.TextBox("OptionTax", string.Format("{0:N0}", Model.OptionTax), new { @class = "readonly money", @readonly = "readonly", style = "width:60px", maxlength = 11, onchange = "calcOptionPrice(2)" })%></td>
        <td><%=Html.TextBox("OptionAmount", string.Format("{0:N0}", Model.OptionAmount), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcOptionPrice(3)" })%></td>
      </tr>
      <tr>
       <!--　MOD 仕入日と入庫日の表示位置入れ替え 2014/02/20 ookubo -->
       <!--　MOD 仕入日と入庫日の表示位置入れ替えを元に戻した 2014/03/12 ookubo -->
        <%=Html.Hidden("Rate",Model.Rate) %>
        <%=Html.Hidden("ConsumptionTaxId", ViewData["ConsumptionTaxId"])%>
        <%=Html.Hidden("ConsumptionTaxIdOld", ViewData["ConsumptionTaxIdOld"])%>
      <% //<!-- Add 2014/06/23 arc yano 税率変更バグ修正対応 -->  %>
        <%=Html.Hidden("DateOld1", ViewData["PurchasePlanDateOld"])%>
        <%=Html.Hidden("DateOld2", ViewData["PurchasePlanDateOld"])%>
        <%=Html.Hidden("canSave", 1)%>

        <!--　ツールチップは確定するまでコメントアウト
        <div title="入庫日・・・仕入れ計上日&#13;&#10;仕入日・・・店舗に車が来た日">入庫日 *</div></th>
        <td>
        <div title="入庫日・・・仕入れ計上日&#13;&#10;仕入日・・・店舗に車が来た日">
            <%=Html.TextBox("PurchaseDate", string.Format("{0:yyyy/MM/dd}", Model.PurchaseDate), new { @class = "alphanumeric", size = 10, maxlength = 10, onchange = "setTaxIdByDate(this, 'CarPurchase');" })%>
     　</div>
     　-->
        <th>仕入日</th>
        <td colspan="3"><%=Html.TextBox("SlipDate", string.Format("{0:yyyy/MM/dd}", Model.SlipDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <!-- 車両本体価格(２行目) -->
        <th colspan="3"></th>
        <th>ファーム価格 * (h)</th>
        <td><%=Html.TextBox("FirmPrice", string.Format("{0:N0}", Model.FirmPrice), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcFirmPrice(1)" })%></td>
        <td><%=Html.TextBox("FirmTax", string.Format("{0:N0}", Model.FirmTax), new { @class = "readonly money", @readonly = "readonly", style = "width:60px", maxlength = 11, onchange = "calcFirmPrice(2)" })%></td>
        <% //Mod 2014/08/07 arc yano #3068 データソースの誤りの修正%>
        <td><%=Html.TextBox("FirmAmount", string.Format("{0:N0}", Model.FirmAmount), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcFirmPrice(3)" })%></td>
      </tr>
      <tr>
       <!--　MOD 仕入日と入庫日の表示位置入れ替え 2014/02/20 ookubo -->
       <!--　MOD 仕入日と入庫日の表示位置入れ替えを元に戻した 2014/03/12 ookubo -->
        <!--　ツールチップは確定するまでコメントアウト
        <th><div title="入庫日・・・仕入れ計上日&#13;&#10;仕入日・・・店舗に車が来た日">仕入日</div></th>
        <td colspan="3"><div title="入庫日・・・仕入れ計上日&#13;&#10;仕入日・・・店舗に車が来た日"><%=Html.TextBox("SlipDate", string.Format("{0:yyyy/MM/dd}", Model.SlipDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></div></td>
        -->
        <th>入庫日 *</th><td>
            <%=Html.TextBox("PurchaseDate", string.Format("{0:yyyy/MM/dd}", Model.PurchaseDate), new { @class = "alphanumeric", size = 10, maxlength = 10, onchange = "setTaxIdByDate(this, this, 'CarPurchase');" })%>
        </td> 
        <!--　ADD 消費税率 2014/02/20 ookubo -->
        <th>消費税率 *</th>
        <td>
            <%if (Model.RateEnabled) { %>
            <% //<!--　Edit 2014/06/23 arc yano 消費税変更バグ対応 引数追加-->%>
                <%=Html.DropDownList("ConsumptionTaxList", (IEnumerable<SelectListItem>)ViewData["ConsumptionTaxList"], new { @class = "alphanumeric", style = "width:165px", size = "1", Value = Model.ConsumptionTaxId, onchange = "setTaxRateById(this, 'CarPurchase', null);" })%>
            <%}else{ %>
                <%=Html.DropDownList("ConsumptionTaxList", (IEnumerable<SelectListItem>)ViewData["ConsumptionTaxList"], new { @class = "readonly alphanumeric", style = "width:165px", size = "1", @disabled = "disabled", Value = Model.ConsumptionTaxId })%>
            <%} %>
        </td> 
        <th>オークション落札料 (b)</th><%// Mod 2017/03/06 arc yano #3640 %>
        <td><%=Html.TextBox("AuctionFeePrice", string.Format("{0:N0}", Model.AuctionFeePrice), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcAuctionFeeAmount(1)" })%></td>
        <td><%=Html.TextBox("AuctionFeeTax", string.Format("{0:N0}", Model.AuctionFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:60px", maxlength = 11, onchange = "calcAuctionFeeAmount(2)" })%></td>
        <td><%=Html.TextBox("AuctionFeeAmount", string.Format("{0:N0}", Model.AuctionFeeAmount), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcAuctionFeeAmount(3)" })%></td>
        <th>ディスカウント価格 * (i)</th>
        <td><%=Html.TextBox("DiscountPrice", string.Format("{0:N0}", Model.DiscountPrice), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcDiscountPrice(1)" })%></td>
        <td><%=Html.TextBox("DiscountTax", string.Format("{0:N0}", Model.DiscountTax), new { @class = "readonly money", @readonly = "readonly", style = "width:60px", maxlength = 11, onchange = "calcDiscountPrice(2)" })%></td>
        <td><%=Html.TextBox("DiscountAmount", string.Format("{0:N0}", Model.DiscountAmount), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcDiscountPrice(3)" })%></td>
      </tr>
      <tr>
        <th>入庫ロケーション *</th>
        <td colspan="3"><%=Html.TextBox("PurchaseLocationCode", Model.PurchaseLocationCode, new { @class = "alphanumeric", size = 10, maxlength = 12, onblur = "GetNameFromCode('PurchaseLocationCode','PurchaseLocationName','Location')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "PurchaseLocationCode", "PurchaseLocationName", "'/Location/CriteriaDialog?BusinessType=001,009'", "0" });%>
            <%=Html.TextBox("PurchaseLocationName", ViewData["PurchaseLocationName"], new { @class = "readonly", @readonly = "readonly", style = "width:200px" })%>
        </td>
        <th>自税充当 (c)</th><%// Mod 2017/03/06 arc yano #3640 %>
        <td><%=Html.TextBox("CarTaxAppropriatePrice", string.Format("{0:N0}", Model.CarTaxAppropriatePrice), new { @class = "readonly money", style = "width:60px", maxlength = 11, @readonly = "readonly"})%></td>
        <td><%=Html.TextBox("CarTaxAppropriateTax", string.Format("{0:N0}", Model.CarTaxAppropriateTax), new { @class = "readonly money", @readonly = "readonly", style = "width:60px"})%></td>
        <td><%=Html.TextBox("CarTaxAppropriateAmount", string.Format("{0:N0}", Model.CarTaxAppropriateAmount), new { @class = "money", style = "width:60px", onchange = "calcCarTaxAppropriatePrice()" })%></td>
        <th>加装価格 * (j)</th>
        <td><%=Html.TextBox("EquipmentPrice", string.Format("{0:N0}", Model.EquipmentPrice), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcEquipmentPrice(1)" })%></td>
        <td><%=Html.TextBox("EquipmentTax", string.Format("{0:N0}", Model.EquipmentTax), new { @class = "readonly money", @readonly = "readonly", style = "width:60px", maxlength = 11, onchange = "calcEquipmentPrice(2)" })%></td>
        <td><%=Html.TextBox("EquipmentAmount", string.Format("{0:N0}", Model.EquipmentAmount), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcEquipmentPrice(3)" })%></td>
      </tr>
     <tr>
        <!--ADD 2017/08/10 arc nakayama #3782_車両仕入_キャンセル機能追加-->
        <%if (ViewData["CancelFlag"] != null && ViewData["CancelFlag"].ToString().Equals("1")){ %>
            <th style="width:100px">キャンセル日</th>
            <td><%=Html.TextBox("CancelDate", string.Format("{0:yyyy/MM/dd}", Model.CancelDate), new { @class = "readonly", size = 10, maxlength = 10, @readonly = "readonly" })%></td>
            <!--ADD 2017/08/10 arc nakayama #3782_車両仕入_キャンセル機能追加-->
            <th style="width:100px">キャンセルメモ</th>
            <td style="width:120px"><%=Html.TextBox("CancelMemo", Model.CancelMemo, new { @class = "readonly", @readonly = "readonly"})%></td>
        <%}else{%>
            <th style="width:100px">キャンセル日</th>
            <td><%=Html.TextBox("CancelDate", string.Format("{0:yyyy/MM/dd}", Model.CancelDate), new { @class = "alphanumeric", size = 10, maxlength = 10, onchange = "return chkDate3(document.getElementById('CancelDate').value, 'CancelDate')" })%></td>
            <!--ADD 2017/08/10 arc nakayama #3782_車両仕入_キャンセル機能追加-->
            <th style="width:100px">キャンセルメモ</th>
            <td style="width:120px"><%=Html.TextBox("CancelMemo", Model.CancelMemo)%></td>
        <%}%>
        <th>リサイクル (d)</th><%// Mod 2017/03/06 arc yano #3640 %>
        <td><%=Html.TextBox("RecyclePrice", string.Format("{0:N0}", Model.RecyclePrice), new { @class = "money", style = "width:60px", maxlength = 11, onblur = "calcRecyclePrice()" })%></td>
        <td><%=Html.TextBox("RecycleTax",0, new { @class = "readonly money", style = "width:60px",@readonly="readonly" }) %></td>
        <td><%=Html.TextBox("RecycleAmount", string.Format("{0:N0}", Model.RecycleAmount), new { @class = "readonly money", style = "width:60px", @readonly = "readonly" })%></td>
        <th>加修価格 * (k)</th>
        <td><%=Html.TextBox("RepairPrice", string.Format("{0:N0}", Model.RepairPrice), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcRepairPrice(1)" })%></td>
        <td><%=Html.TextBox("RepairTax", string.Format("{0:N0}", Model.RepairTax), new { @class = "readonly money", @readonly = "readonly", style = "width:60px", maxlength = 11, onchange = "calcRepairPrice(2)" })%></td>
        <td><%=Html.TextBox("RepairAmount", string.Format("{0:N0}", Model.RepairAmount), new { @class = "money", style = "width:60px", maxlength = 11, onchange = "calcRepairPrice(3)" })%></td>
      </tr>
      <tr>
        <!-- Mod 2016/02/05 ARC Mikami #3212 テキストボックスをテキストエリアに変更。 -->
        <th colspan="2">備考（車輌買取契約書の備考へ印刷されます）</th>
        <td colspan="10">
            <!-- %=Html.TextBox("Memo",Model.Memo,new {size="100",maxlength="100"}) % -->
            <!-- maxlengthはHTML5のみ対応。バリデーションチェックの必要有り。 -->
            <%=Html.TextArea("Memo",Model.Memo,new {style = "width:900px;height:40px", wrap = "virtual" }) %>
        </td>
      </tr>
    </table>
<!--2014/02/20 ADD DropDownListの初期値（selectedvalue）が取得出来なかった為 ookubo -->
<script type="text/javascript">
    document.getElementById("ConsumptionTaxList").value = document.getElementById("ConsumptionTaxId").value;
</script>
