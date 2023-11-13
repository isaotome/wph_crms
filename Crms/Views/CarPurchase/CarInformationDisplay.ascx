<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.SalesCar>" %>
<%@ Import Namespace="CrmsDao"  %>
    <table class="input-form" style="width:1235px">
      <tr>
        <th class="input-form-title" colspan="12">車両基本情報</th>
      </tr>
      <tr>
        <th>管理番号</th>
        <td><%=CommonUtils.PadRightNbsp(Model.SalesCarNumber, 20) %><%=Html.Hidden("SalesCarNumber",Model.SalesCarNumber) %></td>
        <th>グレード</th>
        <td colspan="5"><%try { %><%=CommonUtils.DefaultNbsp(Model.CarGrade.Car.Brand.CarBrandName, 5)%><%} catch (NullReferenceException) { %><%=CommonUtils.Nbsp(5) %><% } %>
                        &nbsp;<%try { %><%=CommonUtils.DefaultNbsp(Model.CarGrade.Car.CarName, 5)%><%} catch (NullReferenceException) { %><%=CommonUtils.Nbsp(5) %><% } %>
                        &nbsp;<%try { %><%=CommonUtils.DefaultNbsp(Model.CarGrade.CarGradeName, 5)%><%} catch (NullReferenceException) { %><%=CommonUtils.Nbsp(5) %><% } %>
                        &nbsp;(<%try { %><%=CommonUtils.DefaultNbsp(Model.CarGradeCode, 5)%><%} catch (NullReferenceException) { %><%=CommonUtils.Nbsp(5) %><% } %>)</td>
        <th>新中区分</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_NewUsedType == null ? "" : Model.c_NewUsedType.Name, 20)%></td>
        <th>販売価格</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:N0}", Model.SalesPrice), 20)%></td>
      </tr>
      <tr>
        <th>系統色</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_ColorCategory == null ? "" : Model.c_ColorCategory.Name, 20)%></td>
        <th>外装色</th>
        <td colspan="3"><%=CommonUtils.DefaultNbsp(Model.ExteriorColorName, 20)%>&nbsp;<%=CommonUtils.PadRightNbsp((string.IsNullOrEmpty(Model.ExteriorColorCode) ? Model.ExteriorColorCode : "(" + Model.ExteriorColorCode + ")"), 20)%></td>
        <th>色替</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff4 == null ? "" : Model.c_OnOff4.Name, 20)%></td>
        <th>内装色</th>
        <td colspan="3"><%=CommonUtils.DefaultNbsp(Model.InteriorColorName, 20)%>&nbsp;<%=CommonUtils.PadRightNbsp((string.IsNullOrEmpty(Model.InteriorColorCode) ? Model.ExteriorColorCode : "(" + Model.InteriorColorCode + ")"), 20)%></td>
      </tr>
      <tr>
        <th>在庫ステータス</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_CarStatus == null ? "" : Model.c_CarStatus.Name, 20)%></td>
        <th>在庫ロケーション</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Location == null ? "" : Model.Location.LocationName, 20)%></td>
        <th>年式</th>
        <td><%=CommonUtils.PadRightNbsp(Model.ManufacturingYear, 20)%></td>
        <th>ハンドル</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.c_Steering == null ? "" : Model.c_Steering.Name, 20)%></td>
      </tr>
    </table>
    <br />
    <table class="input-form" style="width:1235px">
      <tr>
        <th colspan="12" class="input-form-title">車検証情報</th>
      </tr>
      <tr>
        <th>陸運局</th>
        <td><%=CommonUtils.PadRightNbsp(Model.MorterViecleOfficialCode, 20)%></td>
        <th>登録番号(種別)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.RegistrationNumberType, 20)%></td>
        <th>登録番号(かな)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.RegistrationNumberKana, 20)%></td>
        <th>登録番号(プレート)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.RegistrationNumberPlate, 20)%></td>
        <th>登録日</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.RegistrationDate), 20)%></td>
        <th>初年度登録</th>
        <td><%=CommonUtils.PadRightNbsp(Model.FirstRegistrationYear, 20)%></td>
      </tr>
      <tr>
        <th>自動車種別</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_CarClassification == null ? "" : Model.c_CarClassification.Name, 20)%></td>
        <th>用途</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_Usage == null ? "" : Model.c_Usage.Name, 20)%></td>
        <th>事自区分</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_UsageType == null ? "" : Model.c_UsageType.Name, 20)%></td>
        <th>形状</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.c_Figure == null ? "" : Model.c_Figure.Name, 20)%></td>
      </tr>
      <tr>
        <th>車名</th>
        <td colspan="3"><%=CommonUtils.PadRightNbsp(Model.MakerName, 20)%></td>
        <th>定員</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Capacity, 20)%></td>
        <th>最大積載量</th>
        <td><%=CommonUtils.PadRightNbsp(Model.MaximumLoadingWeight, 20)%></td>
        <th>車両重量</th>
        <td><%=CommonUtils.PadRightNbsp(Model.CarWeight, 20)%></td>
        <th>車両総重量</th>
        <td><%=CommonUtils.PadRightNbsp(Model.TotalCarWeight, 20)%></td>
      </tr>
      <tr>
        <th>車台番号 *</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.Vin, 20)%></td>
        <th>長さ</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Length, 20)%></td>
        <th>幅</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Width, 20)%></td>
        <th>高さ</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Height, 20)%></td>
      </tr>
      <tr>
        <th>前前軸重</th>
        <td><%=CommonUtils.PadRightNbsp(Model.FFAxileWeight, 20)%></td>
        <th>前後軸重</th>
        <td><%=CommonUtils.PadRightNbsp(Model.FRAxileWeight, 20)%></td>
        <th>後前軸重</th>
        <td><%=CommonUtils.PadRightNbsp(Model.RFAxileWeight, 20)%></td>
        <th>後後軸重</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.RRAxileWeight, 20)%></td>
      </tr>
      <tr>
        <th>型式</th>
        <td><%=CommonUtils.PadRightNbsp(Model.ModelName, 20)%></td>
        <th>原動機型式</th>
        <td><%=CommonUtils.PadRightNbsp(Model.EngineType, 20)%></td>
        <th>排気量</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Displacement, 20)%></td>
        <th>燃料種類</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Fuel, 20)%></td>
        <th>型式指定番号</th>
        <td><%=CommonUtils.PadRightNbsp(Model.ModelSpecificateNumber, 20)%></td>
        <th>類別区分番号</th>
        <td><%=CommonUtils.PadRightNbsp(Model.ClassificationTypeNumber, 20)%></td>
      </tr>
      <tr>
        <th>所有者氏名</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.PossesorName, 20)%></td>
        <th>所有者住所</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.PossesorAddress, 20)%></td>
      </tr>
      <tr>
        <th>使用者氏名</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.UserName, 20)%></td>
        <th>使用者住所</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.UserAddress, 20)%></td>
      </tr>
      <tr>
        <th>本拠地</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.PrincipalPlace, 20)%></td>
        <th><%=CommonUtils.DefaultNbsp(Model.c_ExpireType == null ? "" : Model.c_ExpireType.Name)%>期限</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.ExpireDate), 20)%></td>
        <th>走行距離</th>
        <td colspan="3"><%=CommonUtils.DefaultNbsp(string.Format("{0:N2}", Model.Mileage), 20)%>&nbsp;<%=CommonUtils.DefaultNbsp(Model.c_MileageUnit == null ? "" : Model.c_MileageUnit.Name, 20)%></td>
      </tr>
      <tr>
        <th rowspan="2">備考</th>
        <td rowspan="2" colspan="5"><%=CommonUtils.PadRightNbsp(Model.Memo, 20)%></td>
        <th rowspan="2">書類備考</th>
        <td rowspan="2" colspan="3"><%=CommonUtils.PadRightNbsp(Model.DocumentRemarks, 20)%></td>
        <th>書類</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_DocumentComplete == null ? "" : Model.c_DocumentComplete.Name, 20)%></td>
      </tr>
      <tr>
        <th>発行日</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.IssueDate), 20)%></td>
      </tr>
    </table>
    <br />
    <table class="input-form" style="width:1235px">
      <tr>
        <th class="input-form-title" colspan="12">車両詳細情報及び法定費用</th>
      </tr>
      <tr>
        <th>納車日</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.SalesDate), 20)%></td>
        <th>点検日</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.InspectionDate), 20)%></td>
        <th>次回点検日</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.NextInspectionDate), 20)%></td>
        <th>VIN(北米用)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.UsVin, 20)%></td>
        <th>メーカー保証</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff == null ? "" : Model.c_OnOff.Name, 20)%></td>
        <th>記録簿</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff5 == null ? "" : Model.c_OnOff5.Name, 20)%></td>
      </tr>
      <tr>
        <th>生産日</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.ProductionDate), 20)%></td>
        <th>修復歴</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff6 == null ? "" : Model.c_OnOff6.Name, 20)%></td>
        <th>お客様指定オイル</th>
        <td colspan="3"><%=CommonUtils.PadRightNbsp(Model.Parts == null ? "" : Model.Parts.PartsNameJp, 20)%></td>
        <th>タイヤ</th>
        <td colspan="3"><%=CommonUtils.PadRightNbsp(Model.Parts1 == null ? "" : Model.Parts1.PartsNameJp, 20)%></td>
      </tr>
      <tr>
        <th>キーコード</th>
        <td><%=CommonUtils.PadRightNbsp(Model.KeyCode, 20)%></td>
        <th>オーディオコード</th>
        <td><%=CommonUtils.PadRightNbsp(Model.AudioCode, 20)%></td>
        <th>輸入</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_Import == null ? "" : Model.c_Import.Name, 20)%></td>
        <th>保証書</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff1 == null ? "" : Model.c_OnOff1.Name, 20)%></td>
        <th>取説</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff2 == null ? "" : Model.c_OnOff2.Name, 20)%></td>
        <th>クーポン</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff7 == null ? "" : Model.c_OnOff7.Name, 20)%></td>
      </tr>
      <tr>
        <th>リサイクル</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_Recycle == null ? "" : Model.c_Recycle.Name, 20)%></td>
        <th>備考1</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo1, 20)%></td>
        <th>備考2</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo2, 20)%></td>
        <th>備考3</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo3, 20)%></td>
        <th>備考4</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo4, 20)%></td>
        <th>備考5</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo5, 20)%></td>
      </tr>
      <tr>
        <th>リサイクル券</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff3 == null ? "" : Model.c_OnOff3.Name, 20)%></td>
        <th>備考6</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo6, 20)%></td>
        <th>備考7</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo7, 20)%></td>
        <th>備考8</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo8, 20)%></td>
        <th>備考9</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo9, 20)%></td>
        <th>備考10</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo10, 20)%></td>
      </tr>
      <tr>
        <th>認定中古車No</th>
        <td><%=CommonUtils.DefaultNbsp(Model.ApprovedCarNumber) %></td>
        <th>認定中古車保証期間</th>
        <td colspan="9"><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}", Model.ApprovedCarWarrantyDateFrom)) %>　～　<%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}", Model.ApprovedCarWarrantyDateTo)) %></td>
      </tr>
      <tr>
        <th>ライト</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_Light == null ? "" : Model.c_Light.Name, 20)%></td>
        <th>AW</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_GenuineType == null ? "" : Model.c_GenuineType.Name, 20)%></td>
        <th>エアロ</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_GenuineType1 == null ? "" : Model.c_GenuineType1.Name, 20)%></td>
        <th>SR</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_Sr == null ? "" : Model.c_Sr.Name, 20)%></td>
        <th>CD</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_GenuineType2 == null ? "" : Model.c_GenuineType2.Name, 20)%></td>
        <th>MD</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_GenuineType3 == null ? "" : Model.c_GenuineType3.Name, 20)%></td>
      </tr>
      <tr>
        <th>ナビ</th>
        <td colspan="3">製造：<%=CommonUtils.DefaultNbsp(Model.c_GenuineType4 == null ? "" : Model.c_GenuineType4.Name, 7)%>
                        &nbsp;媒体：<%=CommonUtils.DefaultNbsp(Model.c_NaviEquipment == null ? "" : Model.c_NaviEquipment.Name, 7)%>
                        &nbsp;位置：<%=CommonUtils.DefaultNbsp(Model.c_NaviDashboard == null ? "" : Model.c_NaviDashboard.Name, 7)%>
        </td>
        <th>シート(色)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.SeatColor, 20)%></td>
        <th>シート</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.c_SeatType == null ? "" : Model.c_SeatType.Name, 20)%></td>
      </tr>
      <tr>
        <th>申告区分</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_DeclarationType == null ? "" : Model.c_DeclarationType.Name, 20)%></td>
        <th>取得原因</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_AcquisitionReason == null ? "" : Model.c_AcquisitionReason.Name, 20)%></td>
        <th>課税区分(自動車税)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_TaxationType == null ? "" : Model.c_TaxationType.Name, 20)%></td>
        <th>課税区分(自動車取得税)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_TaxationType == null ? "" : Model.c_TaxationType.Name, 20)%></td>
        <th>抹消登録</th>
        <td colspan="3"><%=CommonUtils.PadRightNbsp(Model.c_EraseRegist == null ? "" : Model.c_EraseRegist.Name,20) %></td>
      </tr>
      <tr>
        <th>自動車税</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:N0}", Model.CarTax), 20)%></td>
        <th>自動車重量税</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:N0}", Model.CarWeightTax), 20)%></td>
        <th>自動車取得税</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:N0}", Model.AcquisitionTax), 20)%></td>
        <th>自賠責保険料</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:N0}", Model.CarLiabilityInsurance), 20)%></td>
        <th>リサイクル預託金</th>
        <td colspan="3"><%=CommonUtils.PadRightNbsp(string.Format("{0:N0}", Model.RecycleDeposit), 20)%></td>
      </tr>
    </table>
