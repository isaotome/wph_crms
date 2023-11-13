USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[CarSlipStatusChange]    Script Date: 2023/10/17 16:10:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--2023/09/28 yano #4183 �C���{�C�X�Ή�(�o���Ή�)
--2023/09/18 yano #4181�y�ԗ��`�[�z�I�v�V�����敪�ǉ��i�T�[�`���[�W�j
--2023/08/15 yano #4176�y�ԗ��`�[���́z�̔�����p�̏C��
--2022/06/23 yano #4140�y�ԗ��`�[���́z�������̓o�^���`�l���\������Ȃ��s��̑Ή�
--2021/08/03 yano #4090�y�ԗ��`�[�X�e�[�^�X�C���z���ςɖ߂������̐����f�[�^�̍X�V�R�� 
--2021/08/02 yano #4097�y�O���[�h�}�X�^���́z�N���̕ۑ��̊g���@�\�i�N�I�[�^�[�Ή��j
--2021/06/09 yano #4091 �y�ԗ��`�[�z�I�v�V�����s�̋敪�ǉ�(�����e�i�X�E�����ۏ�)
--2020/01/06 yano #4029 �i���o�[�v���[�g�i��ʁj�̒n�斈�̊Ǘ�
--2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���
--2018/12/06 yano #3961 �ԗ��`�[�X�e�[�^�X�C���@�[�ԍς���X�e�[�^�X�߂��������ɁA�ēx�[�ԍςɂł��Ȃ�
--2018/08/23 yano #3931 �ԗ��`�[�X�e�[�^�X�C�� ��������̑Ή�
--2018/08/07 yano #3911 �o�^�ώԗ��̎ԗ��`�[�X�e�[�^�X�C���ɂ���
--2018/02/28 arc yano  #3869 �ԗ��`�[�X�e�[�^�X�C���@���ςɖ߂����ۂɎԗ��̈�������������Ȃ�
--2017/11/10 arc yano  #3787 �ԗ��`�[�ŌÂ�Revision�ŏ㏑���h�~�@�\�ǉ�
--2017/08/17 arc nakayama #3789_�ԗ��`�[�X�e�[�^�X�C����� �����f�[�^�Ǝԗ��}�X�^�X�V���e�ԈႢ
--2017/05/11 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs�@�V�K�쐬


--RetValue   0:���� 1:����(������������) -1�F�G���[
CREATE PROCEDURE [dbo].[CarSlipStatusChange]

	@SlipNumber nvarchar(50),		--�`�[�ԍ�
	@EmployeeCode nvarchar(50),		--�Ј��R�[�h(�X�V��)
	@SalesOrderStatus nvarchar(3),	--�`�[�X�e�[�^�X
	@RequestUserName nvarchar(100)	--�C���˗���
AS
	SET NOCOUNT ON

BEGIN
	BEGIN TRY
	/*-------------------------------------------*/
	/* �萔��`									 */
	/*-------------------------------------------*/
	DECLARE @RetValue int = 0		--Add 2018/08/07 yano #3911

	/*-------------------------------------------*/
	/* �ԗ��`�[�f�[�^���擾						 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_CarSalesHeader_H1 (
		SlipNumber nvarchar(50),
		RevisionNumber int,
		QuoteDate datetime,
		QuoteExpireDate datetime,
		SalesOrderDate datetime,
		SalesOrderStatus nvarchar(3),
		ApprovalFlag nvarchar(3),
		SalesDate datetime,
		CustomerCode nvarchar(10),
		DepartmentCode nvarchar(3),
		EmployeeCode nvarchar(50),
		CampaignCode1 nvarchar(20),
		CampaignCode2 nvarchar(20),
		NewUsedType nvarchar(3),
		SalesType nvarchar(3),
		MakerName nvarchar(50),
		CarBrandName nvarchar(50),
		CarName nvarchar(50),
		CarGradeName nvarchar(100),
		CarGradeCode nvarchar(30),
		ManufacturingYear nvarchar(10),		--Mod 2021/08/02 yano #4097 4 --> 10
		ExteriorColorCode nvarchar(8),
		ExteriorColorName nvarchar(50),
		InteriorColorCode nvarchar(8),
		InteriorColorName nvarchar(50),
		Vin nvarchar(20),
		UsVin nvarchar(20),
		ModelName nvarchar(20),
		Mileage decimal(12,2),
		MileageUnit nvarchar(3),
		RequestPlateNumber nvarchar(10),
		RegistPlanDate nvarchar(10),
		HotStatus nvarchar(3),
		SalesCarNumber nvarchar(50),
		RequestRegistDate datetime,
		SalesPlanDate datetime,
		RegistrationType nvarchar(3),
		MorterViecleOfficialCode nvarchar(10),
		OwnershipReservation nvarchar(3),
		CarLiabilityInsuranceType nvarchar(3),
		SealSubmitDate datetime,
		ProxySubmitDate datetime,
		ParkingSpaceSubmitDate datetime,
		CarLiabilityInsuranceSubmitDate datetime,
		OwnershipReservationSubmitDate datetime,
		Memo nvarchar(100),
		SalesPrice decimal(10,0),
		DiscountAmount decimal(10,0),
		TaxationAmount decimal(10,0),
		TaxAmount decimal(10,0),
		ShopOptionAmount decimal(10,0),
		ShopOptionTaxAmount decimal(10,0),
		MakerOptionAmount decimal(10,0),
		MakerOptionTaxAmount decimal(10,0),
		OutSourceAmount decimal(10,0),
		OutSourceTaxAmount decimal(10,0),
		SubTotalAmount decimal(10,0),
		CarTax decimal(10,0),
		CarLiabilityInsurance decimal(10,0),
		CarWeightTax decimal(10,0),
		AcquisitionTax decimal(10,0),
		InspectionRegistCost decimal(10,0),
		ParkingSpaceCost decimal(10,0),
		TradeInCost decimal(10,0),
		RecycleDeposit decimal(10,0),
		RecycleDepositTradeIn decimal(10,0),
		NumberPlateCost decimal(10,0),
		RequestNumberCost decimal(10,0),
		TradeInFiscalStampCost decimal(10,0),
		TaxFreeFieldName nvarchar(50),
		TaxFreeFieldValue decimal(10,0),
		TaxFreeTotalAmount decimal(10,0),
		InspectionRegistFee decimal(10,0),
		ParkingSpaceFee decimal(10,0),
		TradeInFee decimal(10,0),
		PreparationFee decimal(10,0),
		RecycleControlFee decimal(10,0),
		RecycleControlFeeTradeIn decimal(10,0),
		RequestNumberFee decimal(10,0),
		CarTaxUnexpiredAmount decimal(10,0),
		CarLiabilityInsuranceUnexpiredAmount decimal(10,0),
		TradeInAppraisalFee decimal(10,0),
		FarRegistFee decimal(10,0),
		TradeInMaintenanceFee decimal(10,0),
		InheritedInsuranceFee decimal(10,0),
		TaxationFieldName nvarchar(50),
		TaxationFieldValue decimal(10,0),
		SalesCostTotalAmount decimal(10,0),
		SalesCostTotalTaxAmount decimal(10,0),
		OtherCostTotalAmount decimal(10,0),
		CostTotalAmount decimal(10,0),
		TotalTaxAmount decimal(10,0),
		GrandTotalAmount decimal(10,0),
		PossesorCode nvarchar(10),
		UserCode nvarchar(10),
		PrincipalPlace nvarchar(100),
		VoluntaryInsuranceType nvarchar(3),
		VoluntaryInsuranceCompanyName nvarchar(50),
		VoluntaryInsuranceAmount decimal(10,0),
		VoluntaryInsuranceTermFrom datetime,
		VoluntaryInsuranceTermTo datetime,
		PaymentPlanType nvarchar(3),
		TradeInAmount1 decimal(10,0),
		TradeInTax1 decimal(10,0),
		TradeInUnexpiredCarTax1 decimal(10,0),
		TradeInRemainDebt1 decimal(10,0),
		TradeInAppropriation1 decimal(10,0),
		TradeInRecycleAmount1 decimal(10,0),
		TradeInMakerName1 nvarchar(50),
		TradeInCarName1 nvarchar(50),
		TradeInClassificationTypeNumber1 nvarchar(50),
		TradeInModelSpecificateNumber1 nvarchar(50),
		TradeInManufacturingYear1 nvarchar(50),
		TradeInInspectionExpiredDate1 datetime,
		TradeInMileage1 decimal(10,2),
		TradeInMileageUnit1 nvarchar(3),
		TradeInVin1 nvarchar(20),
		TradeInRegistrationNumber1 nvarchar(20),
		TradeInUnexpiredLiabilityInsurance1 decimal(10,0),
		TradeInAmount2 decimal(10,0),
		TradeInTax2 decimal(10,0),
		TradeInUnexpiredCarTax2 decimal(10,0),
		TradeInRemainDebt2 decimal(10,0),
		TradeInAppropriation2 decimal(10,0),
		TradeInRecycleAmount2 decimal(10,0),
		TradeInMakerName2 nvarchar(50),
		TradeInCarName2 nvarchar(50),
		TradeInClassificationTypeNumber2 nvarchar(50),
		TradeInModelSpecificateNumber2 nvarchar(50),
		TradeInManufacturingYear2 nvarchar(50),
		TradeInInspectionExpiredDate2 datetime,
		TradeInMileage2 decimal(10,2),
		TradeInMileageUnit2 nvarchar(3),
		TradeInVin2 nvarchar(20),
		TradeInRegistrationNumber2 nvarchar(20),
		TradeInUnexpiredLiabilityInsurance2 decimal(10,0),
		TradeInAmount3 decimal(10,0),
		TradeInTax3 decimal(10,0),
		TradeInUnexpiredCarTax3 decimal(10,0),
		TradeInRemainDebt3 decimal(10,0),
		TradeInAppropriation3 decimal(10,0),
		TradeInRecycleAmount3 decimal(10,0),
		TradeInMakerName3 nvarchar(50),
		TradeInCarName3 nvarchar(50),
		TradeInClassificationTypeNumber3 nvarchar(50),
		TradeInModelSpecificateNumber3 nvarchar(50),
		TradeInManufacturingYear3 nvarchar(50),
		TradeInInspectionExpiredDate3 datetime,
		TradeInMileage3 decimal(10,2),
		TradeInMileageUnit3 nvarchar(3),
		TradeInVin3 nvarchar(20),
		TradeInRegistrationNumber3 nvarchar(20),
		TradeInUnexpiredLiabilityInsurance3 decimal(10,0),
		TradeInTotalAmount decimal(10,0),
		TradeInTaxTotalAmount decimal(10,0),
		TradeInUnexpiredCarTaxTotalAmount decimal(10,0),
		TradeInRemainDebtTotalAmount decimal(10,0),
		TradeInAppropriationTotalAmount decimal(10,0),
		PaymentTotalAmount decimal(10,0),
		PaymentCashTotalAmount decimal(10,0),
		LoanPrincipalAmount decimal(10,0),
		LoanFeeAmount decimal(10,0),
		LoanTotalAmount decimal(10,0),
		LoanCodeA nvarchar(10),
		PaymentFrequencyA int,
		PaymentTermFromA datetime,
		PaymentTermToA datetime,
		BonusMonthA1 int,
		BonusMonthA2 int,
		FirstAmountA decimal(10,0),
		SecondAmountA decimal(10,0),
		BonusAmountA decimal(10,0),
		CashAmountA decimal(10,0),
		LoanPrincipalA decimal(10,0),
		LoanFeeA decimal(10,0),
		LoanTotalAmountA decimal(10,0),
		AuthorizationNumberA nvarchar(20),
		FirstDirectDebitDateA datetime,
		SecondDirectDebitDateA int,
		LoanCodeB nvarchar(10),
		PaymentFrequencyB int,
		PaymentTermFromB datetime,
		PaymentTermToB datetime,
		BonusMonthB1 int,
		BonusMonthB2 int,
		FirstAmountB decimal(10,0),
		SecondAmountB decimal(10,0),
		BonusAmountB decimal(10,0),
		CashAmountB decimal(10,0),
		LoanPrincipalB decimal(10,0),
		LoanFeeB decimal(10,0),
		LoanTotalAmountB decimal(10,0),
		AuthorizationNumberB nvarchar(20),
		FirstDirectDebitDateB datetime,
		SecondDirectDebitDateB int,
		LoanCodeC nvarchar(10),
		PaymentFrequencyC int,
		PaymentTermFromC datetime,
		PaymentTermToC datetime,
		BonusMonthC1 int,
		BonusMonthC2 int,
		FirstAmountC decimal(10,0),
		SecondAmountC decimal(10,0),
		BonusAmountC decimal(10,0),
		CashAmountC decimal(10,0),
		LoanPrincipalC decimal(10,0),
		LoanFeeC decimal(10,0),
		LoanTotalAmountC decimal(10,0),
		AuthorizationNumberC nvarchar(20),
		FirstDirectDebitDateC datetime,
		SecondDirectDebitDateC int,
		CancelDate datetime,
		CreateEmployeeCode nvarchar(50),
		CreateDate datetime,
		LastUpdateEmployeeCode nvarchar(50),
		LastUpdateDate datetime,
		DelFlag nvarchar(2),
		InspectionRegistFeeTax decimal(10,0),
		ParkingSpaceFeeTax decimal(10,0),
		TradeInFeeTax decimal(10,0),
		PreparationFeeTax decimal(10,0),
		RecycleControlFeeTax decimal(10,0),
		RecycleControlFeeTradeInTax decimal(10,0),
		RequestNumberFeeTax decimal(10,0),
		CarTaxUnexpiredAmountTax decimal(10,0),
		CarLiabilityInsuranceUnexpiredAmountTax decimal(10,0),
		TradeInAppraisalFeeTax decimal(10,0),
		FarRegistFeeTax decimal(10,0),
		TradeInMaintenanceFeeTax decimal(10,0),
		InheritedInsuranceFeeTax decimal(10,0),
		TaxationFieldValueTax decimal(10,0),
		TradeInEraseRegist1 nvarchar(3),
		TradeInEraseRegist2 nvarchar(3),
		TradeInEraseRegist3 nvarchar(3),
		RemainAmountA decimal(10,0),
		RemainAmountB decimal(10,0),
		RemainAmountC decimal(10,0),
		RemainFinalMonthA datetime,
		RemainFinalMonthB datetime,
		RemainFinalMonthC datetime,
		LoanRateA decimal(6,3),
		LoanRateB decimal(6,3),
		LoanRateC decimal(6,3),
		SalesTax decimal(10,0),
		DiscountTax decimal(10,0),
		TradeInPrice1 decimal(10,0),
		TradeInPrice2 decimal(10,0),
		TradeInPrice3 decimal(10,0),
		TradeInRecycleTotalAmount decimal(10,0),
		ConsumptionTaxId nvarchar(3),
		Rate smallint,
		RevenueStampCost decimal(10,0),
		TradeInCarTaxDeposit decimal(10,0),
		LastEditScreen nvarchar(3),
		PaymentSecondFrequencyA int,
		PaymentSecondFrequencyB int,
		PaymentSecondFrequencyC int,
		ProcessSessionId uniqueidentifier,		--Add 2017/11/10 arc yano  #3787
		EPDiscountTaxId nvarchar(3),			--Add 2019/09/04 yano #4011
		CostAreaCode nvarchar(3),				--Add 2020/01/06 yano #4029
		--Add 2021/06/09 yano #4091
		MaintenancePackageAmount decimal(10, 0),
		MaintenancePackageTaxAmount decimal(10, 0),
		ExtendedWarrantyAmount decimal(10, 0),
		ExtendedWarrantyTaxAmount decimal(10, 0)

	  , TradeInHolderName1 nvarchar(80) 			--Add 2022/06/23 yano #4140
	  , TradeInHolderName2 nvarchar(80)				--Add 2022/06/23 yano #4140
	  , TradeInHolderName3 nvarchar(80)				--Add 2022/06/23 yano #4140

	  , OutJurisdictionRegistFee decimal(10, 0)		--Add 2023/08/15 yano #4176
	  , OutJurisdictionRegistFeeTax decimal(10, 0)	--Add 2023/08/15 yano #4176

	  , SurchargeAmount decimal(10, 0)				--Add 2023/09/18 yano #4181
	  , SurchargeTaxAmount decimal(10, 0)			--Add 2023/09/18 yano #4181

	  , SuspendTaxRecv decimal(10, 0)				--2023/09/28 yano #4183

	)
	
	INSERT INTO #temp_CarSalesHeader_H1
	SELECT
		SlipNumber,
		RevisionNumber,
		QuoteDate,
		QuoteExpireDate,
		SalesOrderDate,
		SalesOrderStatus,
		ApprovalFlag,
		SalesDate,
		CustomerCode,
		DepartmentCode,
		EmployeeCode,
		CampaignCode1,
		CampaignCode2,
		NewUsedType,
		SalesType,
		MakerName,
		CarBrandName,
		CarName,
		CarGradeName,
		CarGradeCode,
		ManufacturingYear,
		ExteriorColorCode,
		ExteriorColorName,
		InteriorColorCode,
		InteriorColorName,
		Vin,
		UsVin,
		ModelName,
		Mileage,
		MileageUnit,
		RequestPlateNumber,
		RegistPlanDate,
		HotStatus,
		SalesCarNumber,
		RequestRegistDate,
		SalesPlanDate,
		RegistrationType,
		MorterViecleOfficialCode,
		OwnershipReservation,
		CarLiabilityInsuranceType,
		SealSubmitDate,
		ProxySubmitDate,
		ParkingSpaceSubmitDate,
		CarLiabilityInsuranceSubmitDate,
		OwnershipReservationSubmitDate,
		Memo,
		SalesPrice,
		DiscountAmount,
		TaxationAmount,
		TaxAmount,
		ShopOptionAmount,
		ShopOptionTaxAmount,
		MakerOptionAmount,
		MakerOptionTaxAmount,
		OutSourceAmount,
		OutSourceTaxAmount,
		SubTotalAmount,
		CarTax,
		CarLiabilityInsurance,
		CarWeightTax,
		AcquisitionTax,
		InspectionRegistCost,
		ParkingSpaceCost,
		TradeInCost,
		RecycleDeposit,
		RecycleDepositTradeIn,
		NumberPlateCost,
		RequestNumberCost,
		TradeInFiscalStampCost,
		TaxFreeFieldName,
		TaxFreeFieldValue,
		TaxFreeTotalAmount,
		InspectionRegistFee,
		ParkingSpaceFee,
		TradeInFee,
		PreparationFee,
		RecycleControlFee,
		RecycleControlFeeTradeIn,
		RequestNumberFee,
		CarTaxUnexpiredAmount,
		CarLiabilityInsuranceUnexpiredAmount,
		TradeInAppraisalFee,
		FarRegistFee,
		TradeInMaintenanceFee,
		InheritedInsuranceFee,
		TaxationFieldName,
		TaxationFieldValue,
		SalesCostTotalAmount,
		SalesCostTotalTaxAmount,
		OtherCostTotalAmount,
		CostTotalAmount,
		TotalTaxAmount,
		GrandTotalAmount,
		PossesorCode,
		UserCode,
		PrincipalPlace,
		VoluntaryInsuranceType,
		VoluntaryInsuranceCompanyName,
		VoluntaryInsuranceAmount,
		VoluntaryInsuranceTermFrom,
		VoluntaryInsuranceTermTo,
		PaymentPlanType,
		TradeInAmount1,
		TradeInTax1,
		TradeInUnexpiredCarTax1,
		TradeInRemainDebt1,
		TradeInAppropriation1,
		TradeInRecycleAmount1,
		TradeInMakerName1,
		TradeInCarName1,
		TradeInClassificationTypeNumber1,
		TradeInModelSpecificateNumber1,
		TradeInManufacturingYear1,
		TradeInInspectionExpiredDate1,
		TradeInMileage1,
		TradeInMileageUnit1,
		TradeInVin1,
		TradeInRegistrationNumber1,
		TradeInUnexpiredLiabilityInsurance1,
		TradeInAmount2,
		TradeInTax2,
		TradeInUnexpiredCarTax2,
		TradeInRemainDebt2,
		TradeInAppropriation2,
		TradeInRecycleAmount2,
		TradeInMakerName2,
		TradeInCarName2,
		TradeInClassificationTypeNumber2,
		TradeInModelSpecificateNumber2,
		TradeInManufacturingYear2,
		TradeInInspectionExpiredDate2,
		TradeInMileage2,
		TradeInMileageUnit2,
		TradeInVin2,
		TradeInRegistrationNumber2,
		TradeInUnexpiredLiabilityInsurance2,
		TradeInAmount3,
		TradeInTax3,
		TradeInUnexpiredCarTax3,
		TradeInRemainDebt3,
		TradeInAppropriation3,
		TradeInRecycleAmount3,
		TradeInMakerName3,
		TradeInCarName3,
		TradeInClassificationTypeNumber3,
		TradeInModelSpecificateNumber3,
		TradeInManufacturingYear3,
		TradeInInspectionExpiredDate3,
		TradeInMileage3,
		TradeInMileageUnit3,
		TradeInVin3,
		TradeInRegistrationNumber3,
		TradeInUnexpiredLiabilityInsurance3,
		TradeInTotalAmount,
		TradeInTaxTotalAmount,
		TradeInUnexpiredCarTaxTotalAmount,
		TradeInRemainDebtTotalAmount,
		TradeInAppropriationTotalAmount,
		PaymentTotalAmount,
		PaymentCashTotalAmount,
		LoanPrincipalAmount,
		LoanFeeAmount,
		LoanTotalAmount,
		LoanCodeA,
		PaymentFrequencyA,
		PaymentTermFromA,
		PaymentTermToA,
		BonusMonthA1,
		BonusMonthA2,
		FirstAmountA,
		SecondAmountA,
		BonusAmountA,
		CashAmountA,
		LoanPrincipalA,
		LoanFeeA,
		LoanTotalAmountA,
		AuthorizationNumberA,
		FirstDirectDebitDateA,
		SecondDirectDebitDateA,
		LoanCodeB,
		PaymentFrequencyB,
		PaymentTermFromB,
		PaymentTermToB,
		BonusMonthB1,
		BonusMonthB2,
		FirstAmountB,
		SecondAmountB,
		BonusAmountB,
		CashAmountB,
		LoanPrincipalB,
		LoanFeeB,
		LoanTotalAmountB,
		AuthorizationNumberB,
		FirstDirectDebitDateB,
		SecondDirectDebitDateB,
		LoanCodeC,
		PaymentFrequencyC,
		PaymentTermFromC,
		PaymentTermToC,
		BonusMonthC1,
		BonusMonthC2,
		FirstAmountC,
		SecondAmountC,
		BonusAmountC,
		CashAmountC,
		LoanPrincipalC,
		LoanFeeC,
		LoanTotalAmountC,
		AuthorizationNumberC,
		FirstDirectDebitDateC,
		SecondDirectDebitDateC,
		CancelDate,
		CreateEmployeeCode,
		CreateDate,
		LastUpdateEmployeeCode,
		LastUpdateDate,
		DelFlag,
		InspectionRegistFeeTax,
		ParkingSpaceFeeTax,
		TradeInFeeTax,
		PreparationFeeTax,
		RecycleControlFeeTax,
		RecycleControlFeeTradeInTax,
		RequestNumberFeeTax,
		CarTaxUnexpiredAmountTax,
		CarLiabilityInsuranceUnexpiredAmountTax,
		TradeInAppraisalFeeTax,
		FarRegistFeeTax,
		TradeInMaintenanceFeeTax,
		InheritedInsuranceFeeTax,
		TaxationFieldValueTax,
		TradeInEraseRegist1,
		TradeInEraseRegist2,
		TradeInEraseRegist3,
		RemainAmountA,
		RemainAmountB,
		RemainAmountC,
		RemainFinalMonthA,
		RemainFinalMonthB,
		RemainFinalMonthC,
		LoanRateA,
		LoanRateB,
		LoanRateC,
		SalesTax,
		DiscountTax,
		TradeInPrice1,
		TradeInPrice2,
		TradeInPrice3,
		TradeInRecycleTotalAmount,
		ConsumptionTaxId,
		Rate,
		RevenueStampCost,
		TradeInCarTaxDeposit,
		LastEditScreen,
		PaymentSecondFrequencyA,
		PaymentSecondFrequencyB,
		PaymentSecondFrequencyC,
		ProcessSessionId,					--Add 2017/11/10 arc yano  #3787
		EPDiscountTaxId,					--Add 2019/09/04 yano #4011
		CostAreaCode,						--Add 2020/01/06 yano #4029
		--Add 2021/06/09 yano #4091
		MaintenancePackageAmount,
		MaintenancePackageTaxAmount,
		ExtendedWarrantyAmount,
		ExtendedWarrantyTaxAmount

		, TradeInHolderName1 	--Add 2022/06/23 yano #4140
	    , TradeInHolderName2	--Add 2022/06/23 yano #4140
	    , TradeInHolderName3	--Add 2022/06/23 yano #4140

		, OutJurisdictionRegistFee 		--Add 2023/08/15 yano #4176
		, OutJurisdictionRegistFeeTax	--Add 2023/08/15 yano #4176

		, SurchargeAmount				--Add 2023/09/18 yano #4181
		, SurchargeTaxAmount			--Add 2023/09/18 yano #4181

		, SuspendTaxRecv				--2023/09/28 yano #4183

	FROM [dbo].[CarSalesHeader]
	WHERE DelFlag = '0'
	  AND SlipNumber = @SlipNumber

	/*-------------------------------------------*/
	/* �Â��ԗ��`�[�f�[�^��_���폜				 */
	/*-------------------------------------------*/

	UPDATE [dbo].[CarSalesHeader]
	SET DelFlag = '1'
	WHERE DelFlag = '0'
	  AND SlipNumber = @SlipNumber

	/*-------------------------------------------------*/
	/* �X�e�[�^�X��߂����ԗ��`�[�f�[�^��INSERT		   */
	/*-------------------------------------------------*/
	INSERT INTO [dbo].[CarSalesHeader]
	SELECT 
		SlipNumber,
		RevisionNumber + 1,
		QuoteDate,
		QuoteExpireDate,
		SalesOrderDate,
		CASE WHEN SalesOrderStatus = '002' THEN '001' ELSE '002' END AS SalesOrderStatus,
		ApprovalFlag,
		SalesDate,
		CustomerCode,
		DepartmentCode,
		EmployeeCode,
		CampaignCode1,
		CampaignCode2,
		NewUsedType,
		SalesType,
		MakerName,
		CarBrandName,
		CarName,
		CarGradeName,
		CarGradeCode,
		ManufacturingYear,
		ExteriorColorCode,
		ExteriorColorName,
		InteriorColorCode,
		InteriorColorName,
		Vin,
		UsVin,
		ModelName,
		Mileage,
		MileageUnit,
		RequestPlateNumber,
		RegistPlanDate,
		HotStatus,
		SalesCarNumber,
		RequestRegistDate,
		SalesPlanDate,
		RegistrationType,
		MorterViecleOfficialCode,
		OwnershipReservation,
		CarLiabilityInsuranceType,
		SealSubmitDate,
		ProxySubmitDate,
		ParkingSpaceSubmitDate,
		CarLiabilityInsuranceSubmitDate,
		OwnershipReservationSubmitDate,
		Memo,
		SalesPrice,
		DiscountAmount,
		TaxationAmount,
		TaxAmount,
		ShopOptionAmount,
		ShopOptionTaxAmount,
		MakerOptionAmount,
		MakerOptionTaxAmount,
		OutSourceAmount,
		OutSourceTaxAmount,
		SubTotalAmount,
		CarTax,
		CarLiabilityInsurance,
		CarWeightTax,
		AcquisitionTax,
		InspectionRegistCost,
		ParkingSpaceCost,
		TradeInCost,
		RecycleDeposit,
		RecycleDepositTradeIn,
		NumberPlateCost,
		RequestNumberCost,
		TradeInFiscalStampCost,
		TaxFreeFieldName,
		TaxFreeFieldValue,
		TaxFreeTotalAmount,
		InspectionRegistFee,
		ParkingSpaceFee,
		TradeInFee,
		PreparationFee,
		RecycleControlFee,
		RecycleControlFeeTradeIn,
		RequestNumberFee,
		CarTaxUnexpiredAmount,
		CarLiabilityInsuranceUnexpiredAmount,
		TradeInAppraisalFee,
		FarRegistFee,
		TradeInMaintenanceFee,
		InheritedInsuranceFee,
		TaxationFieldName,
		TaxationFieldValue,
		SalesCostTotalAmount,
		SalesCostTotalTaxAmount,
		OtherCostTotalAmount,
		CostTotalAmount,
		TotalTaxAmount,
		GrandTotalAmount,
		PossesorCode,
		UserCode,
		PrincipalPlace,
		VoluntaryInsuranceType,
		VoluntaryInsuranceCompanyName,
		VoluntaryInsuranceAmount,
		VoluntaryInsuranceTermFrom,
		VoluntaryInsuranceTermTo,
		PaymentPlanType,
		TradeInAmount1,
		TradeInTax1,
		TradeInUnexpiredCarTax1,
		TradeInRemainDebt1,
		TradeInAppropriation1,
		TradeInRecycleAmount1,
		TradeInMakerName1,
		TradeInCarName1,
		TradeInClassificationTypeNumber1,
		TradeInModelSpecificateNumber1,
		TradeInManufacturingYear1,
		TradeInInspectionExpiredDate1,
		TradeInMileage1,
		TradeInMileageUnit1,
		TradeInVin1,
		TradeInRegistrationNumber1,
		TradeInUnexpiredLiabilityInsurance1,
		TradeInAmount2,
		TradeInTax2,
		TradeInUnexpiredCarTax2,
		TradeInRemainDebt2,
		TradeInAppropriation2,
		TradeInRecycleAmount2,
		TradeInMakerName2,
		TradeInCarName2,
		TradeInClassificationTypeNumber2,
		TradeInModelSpecificateNumber2,
		TradeInManufacturingYear2,
		TradeInInspectionExpiredDate2,
		TradeInMileage2,
		TradeInMileageUnit2,
		TradeInVin2,
		TradeInRegistrationNumber2,
		TradeInUnexpiredLiabilityInsurance2,
		TradeInAmount3,
		TradeInTax3,
		TradeInUnexpiredCarTax3,
		TradeInRemainDebt3,
		TradeInAppropriation3,
		TradeInRecycleAmount3,
		TradeInMakerName3,
		TradeInCarName3,
		TradeInClassificationTypeNumber3,
		TradeInModelSpecificateNumber3,
		TradeInManufacturingYear3,
		TradeInInspectionExpiredDate3,
		TradeInMileage3,
		TradeInMileageUnit3,
		TradeInVin3,
		TradeInRegistrationNumber3,
		TradeInUnexpiredLiabilityInsurance3,
		TradeInTotalAmount,
		TradeInTaxTotalAmount,
		TradeInUnexpiredCarTaxTotalAmount,
		TradeInRemainDebtTotalAmount,
		TradeInAppropriationTotalAmount,
		PaymentTotalAmount,
		PaymentCashTotalAmount,
		LoanPrincipalAmount,
		LoanFeeAmount,
		LoanTotalAmount,
		LoanCodeA,
		PaymentFrequencyA,
		PaymentTermFromA,
		PaymentTermToA,
		BonusMonthA1,
		BonusMonthA2,
		FirstAmountA,
		SecondAmountA,
		BonusAmountA,
		CashAmountA,
		LoanPrincipalA,
		LoanFeeA,
		LoanTotalAmountA,
		AuthorizationNumberA,
		FirstDirectDebitDateA,
		SecondDirectDebitDateA,
		LoanCodeB,
		PaymentFrequencyB,
		PaymentTermFromB,
		PaymentTermToB,
		BonusMonthB1,
		BonusMonthB2,
		FirstAmountB,
		SecondAmountB,
		BonusAmountB,
		CashAmountB,
		LoanPrincipalB,
		LoanFeeB,
		LoanTotalAmountB,
		AuthorizationNumberB,
		FirstDirectDebitDateB,
		SecondDirectDebitDateB,
		LoanCodeC,
		PaymentFrequencyC,
		PaymentTermFromC,
		PaymentTermToC,
		BonusMonthC1,
		BonusMonthC2,
		FirstAmountC,
		SecondAmountC,
		BonusAmountC,
		CashAmountC,
		LoanPrincipalC,
		LoanFeeC,
		LoanTotalAmountC,
		AuthorizationNumberC,
		FirstDirectDebitDateC,
		SecondDirectDebitDateC,
		CancelDate,
		@EmployeeCode,
		GETDATE(),
		@EmployeeCode,
		GETDATE(),
		'0' AS DelFlag,
		InspectionRegistFeeTax,
		ParkingSpaceFeeTax,
		TradeInFeeTax,
		PreparationFeeTax,
		RecycleControlFeeTax,
		RecycleControlFeeTradeInTax,
		RequestNumberFeeTax,
		CarTaxUnexpiredAmountTax,
		CarLiabilityInsuranceUnexpiredAmountTax,
		TradeInAppraisalFeeTax,
		FarRegistFeeTax,
		TradeInMaintenanceFeeTax,
		InheritedInsuranceFeeTax,
		TaxationFieldValueTax,
		TradeInEraseRegist1,
		TradeInEraseRegist2,
		TradeInEraseRegist3,
		RemainAmountA,
		RemainAmountB,
		RemainAmountC,
		RemainFinalMonthA,
		RemainFinalMonthB,
		RemainFinalMonthC,
		LoanRateA,
		LoanRateB,
		LoanRateC,
		SalesTax,
		DiscountTax,
		TradeInPrice1,
		TradeInPrice2,
		TradeInPrice3,
		TradeInRecycleTotalAmount,
		ConsumptionTaxId,
		Rate,
		RevenueStampCost,
		TradeInCarTaxDeposit,
		LastEditScreen,
		PaymentSecondFrequencyA,
		PaymentSecondFrequencyB,
		PaymentSecondFrequencyC,
		ProcessSessionId,					--Add 2017/11/10 arc yano  #3787
		EPDiscountTaxId,					--Add 2019/09/04 yano #4011
		CostAreaCode,						--Add 2020/01/06 yano #4029
		--Add 2021/06/09 yano #4091
		MaintenancePackageAmount,
		MaintenancePackageTaxAmount,
		ExtendedWarrantyAmount,
		ExtendedWarrantyTaxAmount

		, TradeInHolderName1 	--Add 2022/06/23 yano #4140
	    , TradeInHolderName2	--Add 2022/06/23 yano #4140
	    , TradeInHolderName3	--Add 2022/06/23 yano #4140

		, OutJurisdictionRegistFee		--Add 2023/08/15 yano #4176
		, OutJurisdictionRegistFeeTax	--Add 2023/08/15 yano #4176

		, SurchargeAmount				--Add 2023/09/18 yano #4181
		, SurchargeTaxAmount			--Add 2023/09/18 yano #4181

		, SuspendTaxRecv 				--2023/09/28 yano #4183

	FROM #temp_CarSalesHeader_H1

	/*-------------------------------------------*/
	/* �ԗ����׃f�[�^���擾						 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_CarSalesLine_L1 (
		SlipNumber nvarchar(50),
		RevisionNumber int,
		LineNumber int,
		CarOptionCode nvarchar(25),
		CarOptionName nvarchar(100),
		OptionType nvarchar(3),
		Amount decimal(10,0),
		CreateEmployeeCode nvarchar(50),
		CreateDate datetime,
		LastUpdateEmployeeCode nvarchar(50),
		LastUpdateDate datetime,
		DelFlag nvarchar(2),
		TaxAmount decimal(10,0),
		ConsumptionTaxId nvarchar(3),
		Rate smallint
	)
	
	INSERT INTO #temp_CarSalesLine_L1
	SELECT
		SlipNumber,
		RevisionNumber,
		LineNumber,
		CarOptionCode,
		CarOptionName,
		OptionType,
		Amount,
		CreateEmployeeCode,
		CreateDate,
		LastUpdateEmployeeCode,
		LastUpdateDate,
		DelFlag,
		TaxAmount,
		ConsumptionTaxId,
		Rate
	FROM [dbo].[CarSalesLine]
	WHERE DelFlag = '0'
	  AND SlipNumber = @SlipNumber

	/*-------------------------------------------*/
	/* �Â��ԗ��`�[�f�[�^��_���폜				 */
	/*-------------------------------------------*/

	UPDATE [dbo].[CarSalesLine]
	SET DelFlag = '1'
	WHERE DelFlag = '0'
	  AND SlipNumber = @SlipNumber

	/*-------------------------------------------------*/
	/* �X�e�[�^�X��߂����ԗ��`�[�f�[�^��INSERT		   */
	/*-------------------------------------------------*/

	INSERT INTO [dbo].[CarSalesLine]
	SELECT 
		SlipNumber,
		RevisionNumber + 1,
		LineNumber,
		CarOptionCode,
		CarOptionName,
		OptionType,
		Amount,
		@EmployeeCode,
		GETDATE(),
		@EmployeeCode,
		GETDATE(),
		'0' AS DelFlag,
		TaxAmount,
		ConsumptionTaxId,
		Rate
	FROM #temp_CarSalesLine_L1


	/*-------------------------------------------*/
	/* �ԗ��x���f�[�^���擾						 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_CarSalesPayment_P1 (
		SlipNumber nvarchar(50),
		RevisionNumber int,
		LineNumber int,
		CustomerClaimCode nvarchar(10),
		PaymentPlanDate datetime,
		Amount decimal(10,0),
		CreateEmployeeCode nvarchar(50),
		CreateDate datetime,
		LastUpdateEmployeeCode nvarchar(50),
		LastUpdateDate datetime,
		DelFlag nvarchar(2),
		Memo nvarchar(100),
	)
	
		
	INSERT INTO #temp_CarSalesPayment_P1
	SELECT
		SlipNumber,
		RevisionNumber,
		LineNumber,
		CustomerClaimCode,
		PaymentPlanDate,
		Amount,
		CreateEmployeeCode,
		CreateDate,
		LastUpdateEmployeeCode,
		LastUpdateDate,
		DelFlag,
		Memo

	FROM [dbo].[CarSalesPayment]
	WHERE DelFlag = '0'
	  AND SlipNumber = @SlipNumber

	/*-------------------------------------------*/
	/* �Â��ԗ��x���f�[�^��_���폜				 */
	/*-------------------------------------------*/

	UPDATE [dbo].[CarSalesPayment]
	SET DelFlag = '1'
	WHERE DelFlag = '0'
	  AND SlipNumber = @SlipNumber

	/*-------------------------------------------------*/
	/* �X�e�[�^�X��߂����ԗ��x���f�[�^��INSERT		   */
	/*-------------------------------------------------*/
	INSERT INTO [dbo].[CarSalesPayment]
	SELECT 
		SlipNumber,
		RevisionNumber + 1,
		LineNumber,
		CustomerClaimCode,
		PaymentPlanDate,
		Amount,
		@EmployeeCode,
		GETDATE(),
		@EmployeeCode,
		GETDATE(),
		'0' AS DelFlag,
		Memo
	FROM #temp_CarSalesPayment_P1


	--Add 2018/12/06 yano #3961
	--�`�[�X�e�[�^�X=�[�ԍς���X�e�[�^�X��߂����ꍇ
	--�ԗ��}�X�^�̍݌ɃX�e�[�^�X�A���P�[�V������߂�
	IF (@SalesOrderStatus = '005')
	BEGIN
		UPDATE 
			[dbo].[SalesCar]
		SET 
			CarStatus = '004'		--�d�|��
		   ,LocationCode = CASE WHEN LocationCode IS NOT NULL THEN LocationCode ELSE CASE WHEN h.DepartmentCode = '052' THEN '' ELSE h.DepartmentCode END END
		   ,LastUpdateEmployeeCode = @EmployeeCode
		   ,LastUpdateDate = GETDATE()
		FROM 
			[dbo].[SalesCar] sc INNER JOIN 
			[dbo].[CarSalesHeader] h on h.DelFlag = '0' and h.SlipNumber = @SlipNumber and h.SalesCarNumber = sc.SalesCarNumber
		WHERE
			sc.DelFlag = '0'
	END

	--DEL 2018/08/22 #3391 �����������Ȃ�
	/*----------------------------------*/
	/* �ԗ��̈�������(�ԗ������e�[�u��) */
	/*----------------------------------*/
	--Mod 2018/08/07 arc yano #3911 �o�^�ς̏ꍇ�͉��������Ȃ��B
	--DECLARE @RegistrationStatus NVARCHAR(3)
	--DECLARE @ReservationStatus NVARCHAR(3)

	--�Ώۂ̔���
	--SELECT
	--	 @RegistrationStatus = ISNULL(RegistrationStatus, '0')
	--	,@ReservationStatus = ISNULL(ReservationStatus, '0')
	--FROM
	--	[dbo].[CarPurchaseOrder]
	--WHERE
	--	DelFlag='0' AND 
	--	SlipNumber = @SlipNumber
		
	--�`�[�X�e�[�^�X=�󒍁A���o�^�ςłȂ��ꍇ�̂݉���
	--IF (@SalesOrderStatus = '002') AND (@ReservationStatus = '1') AND (@RegistrationStatus <> '1')
	--BEGIN
	--	UPDATE 
	--		[dbo].[CarPurchaseOrder]
	--	SET 
	--		 ReservationStatus = '0'
	--		,SalesCarNumber = ''
	--		,Vin = ''
	--		,LastUpdateEmployeeCode = @EmployeeCode
	--		,LastUpdateDate = GETDATE()
	--	WHERE
	--		DelFlag='0' AND 
	--		SlipNumber = @SlipNumber

		--�ԋp�l��1��ݒ�
	--	SET @RetValue = 1
	--END


	--IF (@SalesOrderStatus = '002')
	--BEGIN

	--	UPDATE [dbo].[CarPurchaseOrder]
	--	SET DelFlag = '1'
	--	WHERE DelFlag='0'
	--	  AND SlipNumber = @SlipNumber
	--END

	--IF (@SalesOrderStatus = '003' or @SalesOrderStatus = '004' or @SalesOrderStatus = '005')
	--BEGIN
	--	UPDATE [dbo].[CarPurchaseOrder]
	--	SET ReservationStatus = '0'
	--	WHERE DelFlag='0' 
	--	  AND SlipNumber = @SlipNumber
	--	  AND EXISTS(select 1 from [dbo].[CarSalesHeader] h where DelFlag = '0' and h.SlipNumber = SlipNumber and h.SalesOrderStatus = '001')
	--END

	 --DEL 2018/08/22 #3391
	 --Mod 2018/08/07 arc yano #3911
	 --Mod 2018/02/28 arc yano #3869
	/*----------------------------------*/
	/* �ԗ��̈�������(�ԗ��}�X�^)		*/
	/*----------------------------------*/
	--IF (@SalesOrderStatus = '002')
	--IF (@SalesOrderStatus = '002' AND @RegistrationStatus <> '1')	--�`�[�X�e�[�^�X���󒍂��o�^�ςłȂ��ꍇ
	--BEGIN
	--	UPDATE 
	--		[dbo].[SalesCar]
	--	SET 
	--		CarStatus = '001'
	--	   ,LocationCode = CASE WHEN LocationCode IS NOT NULL THEN LocationCode ELSE CASE WHEN h.DepartmentCode = '052' THEN '' ELSE h.DepartmentCode END END
	--	   --,LocationCode = CASE WHEN h.DepartmentCode = '052' THEN '' ELSE h.DepartmentCode END
	--	   ,LastUpdateEmployeeCode = @EmployeeCode
	--	   ,LastUpdateDate = GETDATE()
	--	FROM 
	--		[dbo].[SalesCar] sc INNER JOIN 
	--		[dbo].[CarSalesHeader] h on h.DelFlag = '0' and h.SlipNumber = @SlipNumber and h.SalesCarNumber = sc.SalesCarNumber
	--	WHERE
	--		sc.DelFlag = '0' AND 
	--		EXISTS
	--		(
	--			select 1 from [dbo].[CarSalesHeader] H where H.DelFlag = '0' and H.SlipNumber = @SlipNumber and H.SalesCarNumber = SalesCarNumber
	--		)
	--END
	/*-----------------------------*/
	/* �����e�[�u����INSERT		   */
	/*-----------------------------*/
	-- Mod 2018/08/07 arc yano #3911
	-- �󒍂��猩�ςɖ߂����ꍇ�͐i�s�����X�g����폜
	insert W_CarSlipStatusChange 
	(
		SlipNumber,
		SalesOrderStatus,
		RequestUserName,
		CreateEmployeeCode,
		CreateDate,
		LastUpdateEmployeeCode,
		LastUpdateDate,
		ChangeStatus,
		StatusChangeCode
	)
	values(
		@SlipNumber,
		@SalesOrderStatus,
		@RequestUserName,
		@EmployeeCode,
		GETDATE(),
		@EmployeeCode,
		GETDATE(),
		'1'
		,NEWID()
	)

	IF (@SalesOrderStatus = '002')
	BEGIN

		UPDATE 
			W_CarSlipStatusChange
		SET 
			ChangeStatus ='2'
			--,LastUpdateDate=GETDATE()
			--,LastUpdateEmployeeCode=@EmployeeCode
		WHERE 
			SlipNumber = @SlipNumber AND
			ChangeStatus = '1'

		--Add 2021/08/03 yano #4090
		--�����\��f�[�^�̍폜
		UPDATE
			dbo.ReceiptPlan
		SET
			 DelFlag = '1'
			,LastUpdateEmployeeCode = @EmployeeCode
			,LastUpdateDate = GETDATE()
		WHERE
			SlipNumber = @SlipNumber AND
			DelFlag = '0'

	END
	

	BEGIN
		DROP TABLE #temp_CarSalesHeader_H1
		DROP TABLE #temp_CarSalesLine_L1
		DROP TABLE #temp_CarSalesPayment_P1
	END

	END TRY
		BEGIN CATCH
		RETURN ERROR_NUMBER()
	END CATCH

	RETURN @RetValue

END

GO


