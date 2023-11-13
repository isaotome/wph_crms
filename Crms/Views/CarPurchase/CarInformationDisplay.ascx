<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.SalesCar>" %>
<%@ Import Namespace="CrmsDao"  %>
    <table class="input-form" style="width:1235px">
      <tr>
        <th class="input-form-title" colspan="12">�ԗ���{���</th>
      </tr>
      <tr>
        <th>�Ǘ��ԍ�</th>
        <td><%=CommonUtils.PadRightNbsp(Model.SalesCarNumber, 20) %><%=Html.Hidden("SalesCarNumber",Model.SalesCarNumber) %></td>
        <th>�O���[�h</th>
        <td colspan="5"><%try { %><%=CommonUtils.DefaultNbsp(Model.CarGrade.Car.Brand.CarBrandName, 5)%><%} catch (NullReferenceException) { %><%=CommonUtils.Nbsp(5) %><% } %>
                        &nbsp;<%try { %><%=CommonUtils.DefaultNbsp(Model.CarGrade.Car.CarName, 5)%><%} catch (NullReferenceException) { %><%=CommonUtils.Nbsp(5) %><% } %>
                        &nbsp;<%try { %><%=CommonUtils.DefaultNbsp(Model.CarGrade.CarGradeName, 5)%><%} catch (NullReferenceException) { %><%=CommonUtils.Nbsp(5) %><% } %>
                        &nbsp;(<%try { %><%=CommonUtils.DefaultNbsp(Model.CarGradeCode, 5)%><%} catch (NullReferenceException) { %><%=CommonUtils.Nbsp(5) %><% } %>)</td>
        <th>�V���敪</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_NewUsedType == null ? "" : Model.c_NewUsedType.Name, 20)%></td>
        <th>�̔����i</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:N0}", Model.SalesPrice), 20)%></td>
      </tr>
      <tr>
        <th>�n���F</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_ColorCategory == null ? "" : Model.c_ColorCategory.Name, 20)%></td>
        <th>�O���F</th>
        <td colspan="3"><%=CommonUtils.DefaultNbsp(Model.ExteriorColorName, 20)%>&nbsp;<%=CommonUtils.PadRightNbsp((string.IsNullOrEmpty(Model.ExteriorColorCode) ? Model.ExteriorColorCode : "(" + Model.ExteriorColorCode + ")"), 20)%></td>
        <th>�F��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff4 == null ? "" : Model.c_OnOff4.Name, 20)%></td>
        <th>�����F</th>
        <td colspan="3"><%=CommonUtils.DefaultNbsp(Model.InteriorColorName, 20)%>&nbsp;<%=CommonUtils.PadRightNbsp((string.IsNullOrEmpty(Model.InteriorColorCode) ? Model.ExteriorColorCode : "(" + Model.InteriorColorCode + ")"), 20)%></td>
      </tr>
      <tr>
        <th>�݌ɃX�e�[�^�X</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_CarStatus == null ? "" : Model.c_CarStatus.Name, 20)%></td>
        <th>�݌Ƀ��P�[�V����</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Location == null ? "" : Model.Location.LocationName, 20)%></td>
        <th>�N��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.ManufacturingYear, 20)%></td>
        <th>�n���h��</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.c_Steering == null ? "" : Model.c_Steering.Name, 20)%></td>
      </tr>
    </table>
    <br />
    <table class="input-form" style="width:1235px">
      <tr>
        <th colspan="12" class="input-form-title">�Ԍ��؏��</th>
      </tr>
      <tr>
        <th>���^��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.MorterViecleOfficialCode, 20)%></td>
        <th>�o�^�ԍ�(���)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.RegistrationNumberType, 20)%></td>
        <th>�o�^�ԍ�(����)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.RegistrationNumberKana, 20)%></td>
        <th>�o�^�ԍ�(�v���[�g)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.RegistrationNumberPlate, 20)%></td>
        <th>�o�^��</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.RegistrationDate), 20)%></td>
        <th>���N�x�o�^</th>
        <td><%=CommonUtils.PadRightNbsp(Model.FirstRegistrationYear, 20)%></td>
      </tr>
      <tr>
        <th>�����Ԏ��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_CarClassification == null ? "" : Model.c_CarClassification.Name, 20)%></td>
        <th>�p�r</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_Usage == null ? "" : Model.c_Usage.Name, 20)%></td>
        <th>�����敪</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_UsageType == null ? "" : Model.c_UsageType.Name, 20)%></td>
        <th>�`��</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.c_Figure == null ? "" : Model.c_Figure.Name, 20)%></td>
      </tr>
      <tr>
        <th>�Ԗ�</th>
        <td colspan="3"><%=CommonUtils.PadRightNbsp(Model.MakerName, 20)%></td>
        <th>���</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Capacity, 20)%></td>
        <th>�ő�ύڗ�</th>
        <td><%=CommonUtils.PadRightNbsp(Model.MaximumLoadingWeight, 20)%></td>
        <th>�ԗ��d��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.CarWeight, 20)%></td>
        <th>�ԗ����d��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.TotalCarWeight, 20)%></td>
      </tr>
      <tr>
        <th>�ԑ�ԍ� *</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.Vin, 20)%></td>
        <th>����</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Length, 20)%></td>
        <th>��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Width, 20)%></td>
        <th>����</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Height, 20)%></td>
      </tr>
      <tr>
        <th>�O�O���d</th>
        <td><%=CommonUtils.PadRightNbsp(Model.FFAxileWeight, 20)%></td>
        <th>�O�㎲�d</th>
        <td><%=CommonUtils.PadRightNbsp(Model.FRAxileWeight, 20)%></td>
        <th>��O���d</th>
        <td><%=CommonUtils.PadRightNbsp(Model.RFAxileWeight, 20)%></td>
        <th>��㎲�d</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.RRAxileWeight, 20)%></td>
      </tr>
      <tr>
        <th>�^��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.ModelName, 20)%></td>
        <th>�����@�^��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.EngineType, 20)%></td>
        <th>�r�C��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Displacement, 20)%></td>
        <th>�R�����</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Fuel, 20)%></td>
        <th>�^���w��ԍ�</th>
        <td><%=CommonUtils.PadRightNbsp(Model.ModelSpecificateNumber, 20)%></td>
        <th>�ޕʋ敪�ԍ�</th>
        <td><%=CommonUtils.PadRightNbsp(Model.ClassificationTypeNumber, 20)%></td>
      </tr>
      <tr>
        <th>���L�Ҏ���</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.PossesorName, 20)%></td>
        <th>���L�ҏZ��</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.PossesorAddress, 20)%></td>
      </tr>
      <tr>
        <th>�g�p�Ҏ���</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.UserName, 20)%></td>
        <th>�g�p�ҏZ��</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.UserAddress, 20)%></td>
      </tr>
      <tr>
        <th>�{���n</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.PrincipalPlace, 20)%></td>
        <th><%=CommonUtils.DefaultNbsp(Model.c_ExpireType == null ? "" : Model.c_ExpireType.Name)%>����</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.ExpireDate), 20)%></td>
        <th>���s����</th>
        <td colspan="3"><%=CommonUtils.DefaultNbsp(string.Format("{0:N2}", Model.Mileage), 20)%>&nbsp;<%=CommonUtils.DefaultNbsp(Model.c_MileageUnit == null ? "" : Model.c_MileageUnit.Name, 20)%></td>
      </tr>
      <tr>
        <th rowspan="2">���l</th>
        <td rowspan="2" colspan="5"><%=CommonUtils.PadRightNbsp(Model.Memo, 20)%></td>
        <th rowspan="2">���ޔ��l</th>
        <td rowspan="2" colspan="3"><%=CommonUtils.PadRightNbsp(Model.DocumentRemarks, 20)%></td>
        <th>����</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_DocumentComplete == null ? "" : Model.c_DocumentComplete.Name, 20)%></td>
      </tr>
      <tr>
        <th>���s��</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.IssueDate), 20)%></td>
      </tr>
    </table>
    <br />
    <table class="input-form" style="width:1235px">
      <tr>
        <th class="input-form-title" colspan="12">�ԗ��ڍ׏��y�і@���p</th>
      </tr>
      <tr>
        <th>�[�ԓ�</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.SalesDate), 20)%></td>
        <th>�_����</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.InspectionDate), 20)%></td>
        <th>����_����</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.NextInspectionDate), 20)%></td>
        <th>VIN(�k�ėp)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.UsVin, 20)%></td>
        <th>���[�J�[�ۏ�</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff == null ? "" : Model.c_OnOff.Name, 20)%></td>
        <th>�L�^��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff5 == null ? "" : Model.c_OnOff5.Name, 20)%></td>
      </tr>
      <tr>
        <th>���Y��</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:yyyy/MM/dd}", Model.ProductionDate), 20)%></td>
        <th>�C����</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff6 == null ? "" : Model.c_OnOff6.Name, 20)%></td>
        <th>���q�l�w��I�C��</th>
        <td colspan="3"><%=CommonUtils.PadRightNbsp(Model.Parts == null ? "" : Model.Parts.PartsNameJp, 20)%></td>
        <th>�^�C��</th>
        <td colspan="3"><%=CommonUtils.PadRightNbsp(Model.Parts1 == null ? "" : Model.Parts1.PartsNameJp, 20)%></td>
      </tr>
      <tr>
        <th>�L�[�R�[�h</th>
        <td><%=CommonUtils.PadRightNbsp(Model.KeyCode, 20)%></td>
        <th>�I�[�f�B�I�R�[�h</th>
        <td><%=CommonUtils.PadRightNbsp(Model.AudioCode, 20)%></td>
        <th>�A��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_Import == null ? "" : Model.c_Import.Name, 20)%></td>
        <th>�ۏ؏�</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff1 == null ? "" : Model.c_OnOff1.Name, 20)%></td>
        <th>���</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff2 == null ? "" : Model.c_OnOff2.Name, 20)%></td>
        <th>�N�[�|��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff7 == null ? "" : Model.c_OnOff7.Name, 20)%></td>
      </tr>
      <tr>
        <th>���T�C�N��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_Recycle == null ? "" : Model.c_Recycle.Name, 20)%></td>
        <th>���l1</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo1, 20)%></td>
        <th>���l2</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo2, 20)%></td>
        <th>���l3</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo3, 20)%></td>
        <th>���l4</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo4, 20)%></td>
        <th>���l5</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo5, 20)%></td>
      </tr>
      <tr>
        <th>���T�C�N����</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_OnOff3 == null ? "" : Model.c_OnOff3.Name, 20)%></td>
        <th>���l6</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo6, 20)%></td>
        <th>���l7</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo7, 20)%></td>
        <th>���l8</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo8, 20)%></td>
        <th>���l9</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo9, 20)%></td>
        <th>���l10</th>
        <td><%=CommonUtils.PadRightNbsp(Model.Memo10, 20)%></td>
      </tr>
      <tr>
        <th>�F�蒆�Î�No</th>
        <td><%=CommonUtils.DefaultNbsp(Model.ApprovedCarNumber) %></td>
        <th>�F�蒆�Îԕۏ؊���</th>
        <td colspan="9"><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}", Model.ApprovedCarWarrantyDateFrom)) %>�@�`�@<%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}", Model.ApprovedCarWarrantyDateTo)) %></td>
      </tr>
      <tr>
        <th>���C�g</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_Light == null ? "" : Model.c_Light.Name, 20)%></td>
        <th>AW</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_GenuineType == null ? "" : Model.c_GenuineType.Name, 20)%></td>
        <th>�G�A��</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_GenuineType1 == null ? "" : Model.c_GenuineType1.Name, 20)%></td>
        <th>SR</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_Sr == null ? "" : Model.c_Sr.Name, 20)%></td>
        <th>CD</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_GenuineType2 == null ? "" : Model.c_GenuineType2.Name, 20)%></td>
        <th>MD</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_GenuineType3 == null ? "" : Model.c_GenuineType3.Name, 20)%></td>
      </tr>
      <tr>
        <th>�i�r</th>
        <td colspan="3">�����F<%=CommonUtils.DefaultNbsp(Model.c_GenuineType4 == null ? "" : Model.c_GenuineType4.Name, 7)%>
                        &nbsp;�}�́F<%=CommonUtils.DefaultNbsp(Model.c_NaviEquipment == null ? "" : Model.c_NaviEquipment.Name, 7)%>
                        &nbsp;�ʒu�F<%=CommonUtils.DefaultNbsp(Model.c_NaviDashboard == null ? "" : Model.c_NaviDashboard.Name, 7)%>
        </td>
        <th>�V�[�g(�F)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.SeatColor, 20)%></td>
        <th>�V�[�g</th>
        <td colspan="5"><%=CommonUtils.PadRightNbsp(Model.c_SeatType == null ? "" : Model.c_SeatType.Name, 20)%></td>
      </tr>
      <tr>
        <th>�\���敪</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_DeclarationType == null ? "" : Model.c_DeclarationType.Name, 20)%></td>
        <th>�擾����</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_AcquisitionReason == null ? "" : Model.c_AcquisitionReason.Name, 20)%></td>
        <th>�ېŋ敪(�����Ԑ�)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_TaxationType == null ? "" : Model.c_TaxationType.Name, 20)%></td>
        <th>�ېŋ敪(�����Ԏ擾��)</th>
        <td><%=CommonUtils.PadRightNbsp(Model.c_TaxationType == null ? "" : Model.c_TaxationType.Name, 20)%></td>
        <th>�����o�^</th>
        <td colspan="3"><%=CommonUtils.PadRightNbsp(Model.c_EraseRegist == null ? "" : Model.c_EraseRegist.Name,20) %></td>
      </tr>
      <tr>
        <th>�����Ԑ�</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:N0}", Model.CarTax), 20)%></td>
        <th>�����ԏd�ʐ�</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:N0}", Model.CarWeightTax), 20)%></td>
        <th>�����Ԏ擾��</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:N0}", Model.AcquisitionTax), 20)%></td>
        <th>�����ӕی���</th>
        <td><%=CommonUtils.PadRightNbsp(string.Format("{0:N0}", Model.CarLiabilityInsurance), 20)%></td>
        <th>���T�C�N���a����</th>
        <td colspan="3"><%=CommonUtils.PadRightNbsp(string.Format("{0:N0}", Model.RecycleDeposit), 20)%></td>
      </tr>
    </table>
