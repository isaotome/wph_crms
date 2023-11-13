<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarPurchase>" %>
<%@ Import Namespace="CrmsDao" %>

<% 
   // -------------------------------------------------------------------------------------------------------------------------------------
   // Mod 2018/08/28 yano #3922 車両管理表(タマ表) 機能改善② 入庫区分を必須項目に変更
   // Mod 2017/03/06 arc yano #3640 車両仕入入力の計算がおかしい問題の対応
   //                               計算項目の補足説明　レイアウトの変更(仕入価格、車両本体価格を２行にしたことによるレイアウトの変更)
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ------------------------------------------------------------------------------------------------------------------------------------- 
%>
    <table class="input-form-slim">
      <tr>
        <th class="input-form-title" colspan="12">伝票情報</th>
      </tr>
      <tr>
        <th style="width:100px">入庫区分*</th><%//Mod 2018/08/28 yano #3922 %>
        <td><%=Html.Encode(Model.c_CarPurchaseType!=null ? Model.c_CarPurchaseType.Name : "")%></td>
         <th style="width:100px">伝票番号</th>
        <td style="width:120px"><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "readonly numeric" , @readonly = "readonly"})%></td>
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
        <td colspan="3"><%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric readonly", size = 10, @readonly = "readonly" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'", "1" }); %>
            <%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { @class = "readonly", @readonly = "readonly", style = "width:200px" })%>
        </td>
        <th rowspan="2">仕入価格(総額) * (z)<input type="button" value="計算" style="width:30px" disabled="disabled" /><br/>(a)～(k) の合計 </th>
        <td><%=Html.TextBox("Amount", string.Format("{0:N0}", Model.Amount), new { @class = "money readonly", style = "width:60px", @readonly = "readonly"  }) %></td>
        <td><%=Html.TextBox("TaxAmount", string.Format("{0:N0}", Model.TaxAmount), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" }) %></td>
        <td><%=Html.TextBox("TotalAmount", string.Format("{0:N0}", Model.TotalAmount), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" }) %></td>
        <th>その他価格 * (e)</th>
        <td><%=Html.TextBox("OthersPrice",  string.Format("{0:N0}", Model.OthersPrice), new { @class = "money readonly", style = "width:60px",  @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("OthersTax",  string.Format("{0:N0}", Model.OthersTax), new { @class = "money readonly", style = "width:60px",  @readonly = "readonly" })%></td>        
        <td><%=Html.TextBox("OthersAmount",  string.Format("{0:N0}", Model.OthersAmount), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
      </tr>
      <tr>
        <th>仕入担当者 *</th>
        <td colspan="3">
            <%=Html.TextBox("EmployeeNumber", Model.Employee != null ? Model.Employee.EmployeeNumber : "", new { @class = "readonly alphanumeric", style = "width:50px", maxlength = "20", @readonly = "readonly" })%>
            <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "readonly alphanumeric", style = "width:80px", maxlength = "50", @readonly = "readonly" }) %>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "EmployeeCode", "EmployeeName", "'/Employee/CriteriaDialog'", "1" }); %>
            <%=Html.TextBox("EmployeeName", ViewData["EmployeeName"], new { @class = "readonly", style = "width:150px", @readonly = "readonly" })%>
        </td>
        <!--仕入価格（総額）２行目 -->
        <th colspan="3"></th>   
        <th>メタリック価格 * (f)</th>
        <td><%=Html.TextBox("MetallicPrice", string.Format("{0:N0}", Model.MetallicPrice), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("MetallicTax",  string.Format("{0:N0}", Model.MetallicTax), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("MetallicAmount",  string.Format("{0:N0}", Model.MetallicAmount), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>  
      </tr>
      <tr>
        <th>仕入先 *</th>
        <td colspan="3"><%=Html.TextBox("SupplierCode", Model.SupplierCode, new { @class = "alphanumeric readonly", size = 10, @readonly = "readonly" })%>
            <%Html.RenderPartial("SearchButtonControl",new string[]{"SupplierCode", "SupplierName", "'/Supplier/CriteriaDialog'","1"});%>
            <%=Html.TextBox("SupplierName", ViewData["SupplierName"], new { @class = "readonly", @readonly = "readonly", style = "width:200px" })%>
        </td>
        <!-- 車両本体価格（２行目） -->
        <th rowspan ="2">車両本体価格 * (a)<input type="button" value="計算" style="width:30px" disabled = "disabled"/> <br/>(z)－( (b)～(k)の合計 )</th>
        <td><%=Html.TextBox("VehiclePrice",  string.Format("{0:N0}", Model.VehiclePrice), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("VehicleTax",  string.Format("{0:N0}", Model.VehicleTax), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("VehicleAmount",  string.Format("{0:N0}", Model.VehicleAmount), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <th>オプション価格 * (g)</th>
        <td><%=Html.TextBox("OptionPrice",  string.Format("{0:N0}", Model.OptionPrice), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("OptionTax",  string.Format("{0:N0}", Model.OptionTax), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("OptionAmount",  string.Format("{0:N0}", Model.OptionAmount), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td> 
      </tr>
      <tr>
       <!--　MOD 仕入日と入庫日の表示位置入れ替え 2014/02/20 ookubo -->
        <%=Html.Hidden("Rate",Model.Rate) %>
        <%=Html.Hidden("ConsumptionTaxId", ViewData["ConsumptionTaxId"])%>
        <%=Html.Hidden("ConsumptionTaxIdOld", ViewData["ConsumptionTaxIdOld"])%>
        <%=Html.Hidden("DateOld1", ViewData["SlipDate"])%>
        <%=Html.Hidden("DateOld2", ViewData["SlipDate"])%>
        <%=Html.Hidden("canSave", 1)%>
        <!--　ツールチップは確定するまでコメントアウト
        <div title="入庫日・・・仕入れ計上日&#13;&#10;仕入日・・・店舗に車が来た日">入庫日 *</div></th>
        <td>
        <div title="入庫日・・・仕入れ計上日&#13;&#10;仕入日・・・店舗に車が来た日">
            <%=Html.TextBox("PurchaseDate", string.Format("{0:yyyy/MM/dd}", Model.PurchaseDate), new { @class = "alphanumeric", size = 10, maxlength = 10, onchange = "setTaxIdByDate(this, 'CarPurchase');" })%>
     　</div>
     　-->
        <th>仕入日</th>
        <td colspan="3"><%=Html.TextBox("SlipDate", string.Format("{0:yyyy/MM/dd}", Model.SlipDate), new { @class = "alphanumeric readonly", size = 10, @readonly = "readonly" })%></td>
        <th colspan="3"></th>
        <th>ファーム価格 * (h)</th>
        <td><%=Html.TextBox("FirmPrice",  string.Format("{0:N0}", Model.FirmPrice), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("FirmTax",  string.Format("{0:N0}", Model.FirmTax), new { @class = "money readonly", style = "width:60px", maxlength = 11, @readonly = "readonly" })%></td>
         <% //Mod 2014/08/07 arc yano #3068 データソースの誤りの修正%>
        <td><%=Html.TextBox("FirmAmount",  string.Format("{0:N0}", Model.FirmAmount), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
      </tr>
      <tr>
       <!--　MOD 仕入日と入庫日の表示位置入れ替え 2014/02/20 ookubo -->
        <!--　ツールチップは確定するまでコメントアウト
        <th><div title="入庫日・・・仕入れ計上日&#13;&#10;仕入日・・・店舗に車が来た日">仕入日</div></th>
        <td colspan="3"><div title="入庫日・・・仕入れ計上日&#13;&#10;仕入日・・・店舗に車が来た日"><%=Html.TextBox("SlipDate", string.Format("{0:yyyy/MM/dd}", Model.SlipDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></div></td>
        -->
        <th>入庫日 *</th><td>
            <%=Html.TextBox("PurchaseDate", string.Format("{0:yyyy/MM/dd}", Model.PurchaseDate), new { @class = "alphanumeric readonly", size = 10, @readonly = "readonly" })%>
        </td> 
        <!--　ADD 消費税率 2014/02/20 ookubo -->
        <th>消費税率 *</th>
        <td>
            <%=Html.DropDownList("ConsumptionTaxList", (IEnumerable<SelectListItem>)ViewData["ConsumptionTaxList"], new { @class = "readonly alphanumeric", style = "width:165px", size = "1", @disabled = "disabled", Value = Model.ConsumptionTaxId })%>
        </td>
        <th>オークション落札料 (b)</th>
        <td><%=Html.TextBox("AuctionFeePrice",  string.Format("{0:N0}", Model.AuctionFeePrice), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("AuctionFeeTax",  string.Format("{0:N0}", Model.AuctionFeeTax), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("AuctionFeeAmount",  string.Format("{0:N0}", Model.AuctionFeeAmount), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <th>ディスカウント価格 * (i)</th>
        <td><%=Html.TextBox("DiscountPrice",  string.Format("{0:N0}", Model.DiscountPrice), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("DiscountTax",  string.Format("{0:N0}", Model.DiscountTax), new { @class = "money readonly", style = "width:60px", maxlength = 11, @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("DiscountAmount",  string.Format("{0:N0}", Model.DiscountAmount), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>       
      </tr>
      <tr>
        <th>入庫ロケーション *</th>
        <td colspan="3"><%=Html.TextBox("PurchaseLocationCode", Model.PurchaseLocationCode, new { @class = "alphanumeric readonly", size = 10, @readonly = "readonly" })%>
            <%Html.RenderPartial("SearchButtonControl",new string[]{"PurchaseLocationCode", "PurchaseLocationName", "'/Location/CriteriaDialog'","1"});%>
            <%=Html.TextBox("PurchaseLocationName", ViewData["PurchaseLocationName"], new { @class = "readonly", @readonly = "readonly", style = "width:200px" })%>
        </td>
        <th>自税充当 (c)</th>
        <td><%=Html.TextBox("CarTaxAppropriatePrice",  string.Format("{0:N0}", Model.CarTaxAppropriatePrice), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("CarTaxAppropriateTax", 0, new { @class = "readonly money", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("CarTaxAppropriateAmount",  string.Format("{0:N0}", Model.CarTaxAppropriateAmount), new { @class = "readonly money", style = "width:60px", @readonly = "readonly" })%></td>
        <th>加装価格 * (j)</th>
        <td><%=Html.TextBox("EquipmentPrice",  string.Format("{0:N0}", Model.EquipmentPrice), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("EquipmentTax",  string.Format("{0:N0}", Model.EquipmentTax), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("EquipmentAmount",  string.Format("{0:N0}", Model.EquipmentAmount), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
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
        
        <th>リサイクル (d)</th>
        <td><%=Html.TextBox("RecyclePrice",  string.Format("{0:N0}", Model.RecyclePrice), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("RecycleTax",0, new { @class = "readonly money", style = "width:60px",@readonly="readonly" }) %></td>
        <td><%=Html.TextBox("RecycleAmount",  string.Format("{0:N0}", Model.RecycleAmount), new { @class = "readonly money", style = "width:60px", @readonly = "readonly" })%></td>
        <th>加修価格 * (k)</th>
        <td><%=Html.TextBox("RepairPrice",  string.Format("{0:N0}", Model.RepairPrice), new { @class = "money readonly", style = "width:60px",  @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("RepairTax",  string.Format("{0:N0}", Model.RepairTax), new { @class = "money readonly", style = "width:60px",  @readonly = "readonly" })%></td>
        <td><%=Html.TextBox("RepairAmount",  string.Format("{0:N0}", Model.RepairAmount), new { @class = "money readonly", style = "width:60px", @readonly = "readonly" })%></td>
      </tr>
      <tr>
        <!-- Mod 2016/02/05 ARC Mikami #3212 テキストボックスをテキストエリアに変更。 -->
        <th colspan="2">備考（車輌買取契約書の備考へ印刷されます）</th>
        <td colspan="10">
            <!-- %=Html.TextBox("Memo",Model.Memo,new {size="100",maxlength="100"}) % -->
            <!-- maxlengthはHTML5のみ対応。バリデーションチェックの必要有り。 -->
            <%=Html.TextArea("Memo",Model.Memo,new { @readonly="readonly", @class="readonly", style = "width:900px;height:40px", wrap = "virtual" }) %>
        </td>
      </tr>
    </table>
<!--2014/02/20 ADD DropDownListの初期値（selectedvalue）が取得出来なかった為 ookubo -->
<script type="text/javascript">
    document.getElementById("ConsumptionTaxList").value = document.getElementById("ConsumptionTaxId").value;
</script>
