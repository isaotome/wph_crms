USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[CarSlipStatus_Return]    Script Date: 2023/10/17 16:12:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--2023/09/28 yano #4183 インボイス対応(経理対応)
--2023/09/18 yano #4181【車両伝票】オプション区分追加（サーチャージ）
--2023/08/15 yano #4176【車両伝票入力】販売諸費用の修正
--2022/06/23 yano #4140【車両伝票入力】注文書の登録名義人が表示されない不具合の対応
--2021/06/09 yano #4091 【車両伝票】オプション行の区分追加(メンテナス・延長保証)
--2020/01/06 yano #4029 ナンバープレート（一般）の地域毎の管理
--2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 
--2018/12/06 yano #3961 車両伝票ステータス修正　納車済からステータス戻しした時に、再度納車済にできない
--2017/11/10 arc yano  #3787 車両伝票で古いRevisionで上書き防止機能追加
--2017/05/11 arc nakayama #3761_サブシステムの伝票戻しの移行　新規作成

CREATE PROCEDURE [dbo].[CarSlipStatus_Return]

	@SlipNumber nvarchar(50),		--伝票番号
	@EmployeeCode nvarchar(50),		--社員コード(更新者)
	@SalesOrderStatus nvarchar(3),	--伝票ステータス
	@StatusChangeCode nvarchar(36)	--修正ID
AS
	SET NOCOUNT ON

BEGIN
	BEGIN TRY
	/*-------------------------------------------*/
	/* 車両伝票データを取得						 */
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
		ManufacturingYear nvarchar(4),
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

	  , SuspendTaxRecv decimal(10, 0)				--Add 2023/09/28 yano #4183

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
		ProcessSessionId,						--Add 2017/11/10 arc yano  #3787
		EPDiscountTaxId,						--Add 2019/09/04 yano #4011
		CostAreaCode,							--Add 2020/01/06 yano #4029
		--Add 2021/06/09 yano #4091
		MaintenancePackageAmount,
		MaintenancePackageTaxAmount,
		ExtendedWarrantyAmount,
		ExtendedWarrantyTaxAmount

	  , TradeInHolderName1 	--Add 2022/06/23 yano #4140
	  , TradeInHolderName2	--Add 2022/06/23 yano #4140
	  , TradeInHolderName3	--Add 2022/06/23 yano #4140

	  , OutJurisdictionRegistFee		--Add 2023/08/15 yano #4176
	  , OutJurisdictionRegistFeeTax		--Add 2023/08/15 yano #4176

	  , SurchargeAmount							--Add 2023/09/18 yano #4181
	  , SurchargeTaxAmount						--Add 2023/09/18 yano #4181

	  , SuspendTaxRecv							--Add 2023/09/28 yano #4183

	FROM [dbo].[CarSalesHeader]
	WHERE DelFlag = '0'
	  AND SlipNumber = @SlipNumber

	/*-------------------------------------------*/
	/* 古い車両伝票データを論理削除				 */
	/*-------------------------------------------*/

	UPDATE [dbo].[CarSalesHeader]
	SET DelFlag = '1'
	WHERE DelFlag = '0'
	  AND SlipNumber = @SlipNumber

	/*-------------------------------------------------*/
	/* ステータスを戻した車両伝票データをINSERT		   */
	/*-------------------------------------------------*/

	INSERT INTO [dbo].[CarSalesHeader]
	SELECT 
		H1.SlipNumber,
		H1.RevisionNumber + 1,
		H1.QuoteDate,
		H1.QuoteExpireDate,
		H1.SalesOrderDate,
		w.SalesOrderStatus,
		H1.ApprovalFlag,
		H1.SalesDate,
		H1.CustomerCode,
		H1.DepartmentCode,
		H1.EmployeeCode,
		H1.CampaignCode1,
		H1.CampaignCode2,
		H1.NewUsedType,
		H1.SalesType,
		H1.MakerName,
		H1.CarBrandName,
		H1.CarName,
		H1.CarGradeName,
		H1.CarGradeCode,
		H1.ManufacturingYear,
		H1.ExteriorColorCode,
		H1.ExteriorColorName,
		H1.InteriorColorCode,
		H1.InteriorColorName,
		H1.Vin,
		H1.UsVin,
		H1.ModelName,
		H1.Mileage,
		H1.MileageUnit,
		H1.RequestPlateNumber,
		H1.RegistPlanDate,
		H1.HotStatus,
		H1.SalesCarNumber,
		H1.RequestRegistDate,
		H1.SalesPlanDate,
		H1.RegistrationType,
		H1.MorterViecleOfficialCode,
		H1.OwnershipReservation,
		H1.CarLiabilityInsuranceType,
		H1.SealSubmitDate,
		H1.ProxySubmitDate,
		H1.ParkingSpaceSubmitDate,
		H1.CarLiabilityInsuranceSubmitDate,
		H1.OwnershipReservationSubmitDate,
		H1.Memo,
		H1.SalesPrice,
		H1.DiscountAmount,
		H1.TaxationAmount,
		H1.TaxAmount,
		H1.ShopOptionAmount,
		H1.ShopOptionTaxAmount,
		H1.MakerOptionAmount,
		H1.MakerOptionTaxAmount,
		H1.OutSourceAmount,
		H1.OutSourceTaxAmount,
		H1.SubTotalAmount,
		H1.CarTax,
		H1.CarLiabilityInsurance,
		H1.CarWeightTax,
		H1.AcquisitionTax,
		H1.InspectionRegistCost,
		H1.ParkingSpaceCost,
		H1.TradeInCost,
		H1.RecycleDeposit,
		H1.RecycleDepositTradeIn,
		H1.NumberPlateCost,
		H1.RequestNumberCost,
		H1.TradeInFiscalStampCost,
		H1.TaxFreeFieldName,
		H1.TaxFreeFieldValue,
		H1.TaxFreeTotalAmount,
		H1.InspectionRegistFee,
		H1.ParkingSpaceFee,
		H1.TradeInFee,
		H1.PreparationFee,
		H1.RecycleControlFee,
		H1.RecycleControlFeeTradeIn,
		H1.RequestNumberFee,
		H1.CarTaxUnexpiredAmount,
		H1.CarLiabilityInsuranceUnexpiredAmount,
		H1.TradeInAppraisalFee,
		H1.FarRegistFee,
		H1.TradeInMaintenanceFee,
		H1.InheritedInsuranceFee,
		H1.TaxationFieldName,
		H1.TaxationFieldValue,
		H1.SalesCostTotalAmount,
		H1.SalesCostTotalTaxAmount,
		H1.OtherCostTotalAmount,
		H1.CostTotalAmount,
		H1.TotalTaxAmount,
		H1.GrandTotalAmount,
		H1.PossesorCode,
		H1.UserCode,
		H1.PrincipalPlace,
		H1.VoluntaryInsuranceType,
		H1.VoluntaryInsuranceCompanyName,
		H1.VoluntaryInsuranceAmount,
		H1.VoluntaryInsuranceTermFrom,
		H1.VoluntaryInsuranceTermTo,
		H1.PaymentPlanType,
		H1.TradeInAmount1,
		H1.TradeInTax1,
		H1.TradeInUnexpiredCarTax1,
		H1.TradeInRemainDebt1,
		H1.TradeInAppropriation1,
		H1.TradeInRecycleAmount1,
		H1.TradeInMakerName1,
		H1.TradeInCarName1,
		H1.TradeInClassificationTypeNumber1,
		H1.TradeInModelSpecificateNumber1,
		H1.TradeInManufacturingYear1,
		H1.TradeInInspectionExpiredDate1,
		H1.TradeInMileage1,
		H1.TradeInMileageUnit1,
		H1.TradeInVin1,
		H1.TradeInRegistrationNumber1,
		H1.TradeInUnexpiredLiabilityInsurance1,
		H1.TradeInAmount2,
		H1.TradeInTax2,
		H1.TradeInUnexpiredCarTax2,
		H1.TradeInRemainDebt2,
		H1.TradeInAppropriation2,
		H1.TradeInRecycleAmount2,
		H1.TradeInMakerName2,
		H1.TradeInCarName2,
		H1.TradeInClassificationTypeNumber2,
		H1.TradeInModelSpecificateNumber2,
		H1.TradeInManufacturingYear2,
		H1.TradeInInspectionExpiredDate2,
		H1.TradeInMileage2,
		H1.TradeInMileageUnit2,
		H1.TradeInVin2,
		H1.TradeInRegistrationNumber2,
		H1.TradeInUnexpiredLiabilityInsurance2,
		H1.TradeInAmount3,
		H1.TradeInTax3,
		H1.TradeInUnexpiredCarTax3,
		H1.TradeInRemainDebt3,
		H1.TradeInAppropriation3,
		H1.TradeInRecycleAmount3,
		H1.TradeInMakerName3,
		H1.TradeInCarName3,
		H1.TradeInClassificationTypeNumber3,
		H1.TradeInModelSpecificateNumber3,
		H1.TradeInManufacturingYear3,
		H1.TradeInInspectionExpiredDate3,
		H1.TradeInMileage3,
		H1.TradeInMileageUnit3,
		H1.TradeInVin3,
		H1.TradeInRegistrationNumber3,
		H1.TradeInUnexpiredLiabilityInsurance3,
		H1.TradeInTotalAmount,
		H1.TradeInTaxTotalAmount,
		H1.TradeInUnexpiredCarTaxTotalAmount,
		H1.TradeInRemainDebtTotalAmount,
		H1.TradeInAppropriationTotalAmount,
		H1.PaymentTotalAmount,
		H1.PaymentCashTotalAmount,
		H1.LoanPrincipalAmount,
		H1.LoanFeeAmount,
		H1.LoanTotalAmount,
		H1.LoanCodeA,
		H1.PaymentFrequencyA,
		H1.PaymentTermFromA,
		H1.PaymentTermToA,
		H1.BonusMonthA1,
		H1.BonusMonthA2,
		H1.FirstAmountA,
		H1.SecondAmountA,
		H1.BonusAmountA,
		H1.CashAmountA,
		H1.LoanPrincipalA,
		H1.LoanFeeA,
		H1.LoanTotalAmountA,
		H1.AuthorizationNumberA,
		H1.FirstDirectDebitDateA,
		H1.SecondDirectDebitDateA,
		H1.LoanCodeB,
		H1.PaymentFrequencyB,
		H1.PaymentTermFromB,
		H1.PaymentTermToB,
		H1.BonusMonthB1,
		H1.BonusMonthB2,
		H1.FirstAmountB,
		H1.SecondAmountB,
		H1.BonusAmountB,
		H1.CashAmountB,
		H1.LoanPrincipalB,
		H1.LoanFeeB,
		H1.LoanTotalAmountB,
		H1.AuthorizationNumberB,
		H1.FirstDirectDebitDateB,
		H1.SecondDirectDebitDateB,
		H1.LoanCodeC,
		H1.PaymentFrequencyC,
		H1.PaymentTermFromC,
		H1.PaymentTermToC,
		H1.BonusMonthC1,
		H1.BonusMonthC2,
		H1.FirstAmountC,
		H1.SecondAmountC,
		H1.BonusAmountC,
		H1.CashAmountC,
		H1.LoanPrincipalC,
		H1.LoanFeeC,
		H1.LoanTotalAmountC,
		H1.AuthorizationNumberC,
		H1.FirstDirectDebitDateC,
		H1.SecondDirectDebitDateC,
		H1.CancelDate,
		@EmployeeCode,
		GETDATE(),
		@EmployeeCode,
		GETDATE(),
		'0' AS DelFlag,
		H1.InspectionRegistFeeTax,
		H1.ParkingSpaceFeeTax,
		H1.TradeInFeeTax,
		H1.PreparationFeeTax,
		H1.RecycleControlFeeTax,
		H1.RecycleControlFeeTradeInTax,
		H1.RequestNumberFeeTax,
		H1.CarTaxUnexpiredAmountTax,
		H1.CarLiabilityInsuranceUnexpiredAmountTax,
		H1.TradeInAppraisalFeeTax,
		H1.FarRegistFeeTax,
		H1.TradeInMaintenanceFeeTax,
		H1.InheritedInsuranceFeeTax,
		H1.TaxationFieldValueTax,
		H1.TradeInEraseRegist1,
		H1.TradeInEraseRegist2,
		H1.TradeInEraseRegist3,
		H1.RemainAmountA,
		H1.RemainAmountB,
		H1.RemainAmountC,
		H1.RemainFinalMonthA,
		H1.RemainFinalMonthB,
		H1.RemainFinalMonthC,
		H1.LoanRateA,
		H1.LoanRateB,
		H1.LoanRateC,
		H1.SalesTax,
		H1.DiscountTax,
		H1.TradeInPrice1,
		H1.TradeInPrice2,
		H1.TradeInPrice3,
		H1.TradeInRecycleTotalAmount,
		H1.ConsumptionTaxId,
		H1.Rate,
		H1.RevenueStampCost,
		H1.TradeInCarTaxDeposit,
		H1.LastEditScreen,
		H1.PaymentSecondFrequencyA,
		H1.PaymentSecondFrequencyB,
		H1.PaymentSecondFrequencyC,
		H1.ProcessSessionId,				--Add 2017/11/10 arc yano  #3787
		H1.EPDiscountTaxId,					--Add 2019/09/04 yano #4011
		H1.CostAreaCode,					--Add 2020/01/06 yano #4029
		--Add 2021/06/09 yano #4091
		H1.MaintenancePackageAmount,
		H1.MaintenancePackageTaxAmount,
		H1.ExtendedWarrantyAmount,
		H1.ExtendedWarrantyTaxAmount

	  , H1.TradeInHolderName1 	--Add 2022/06/23 yano #4140
	  , H1.TradeInHolderName2	--Add 2022/06/23 yano #4140
	  , H1.TradeInHolderName3	--Add 2022/06/23 yano #4140

	  , OutJurisdictionRegistFee		--Add 2023/08/15 yano #4176
	  , OutJurisdictionRegistFeeTax		--Add 2023/08/15 yano #4176

	  , SurchargeAmount							--Add 2023/09/18 yano #4181
	  , SurchargeTaxAmount						--Add 2023/09/18 yano #4181

	  , SuspendTaxRecv							--Add 2023/09/28 yano #4183

	FROM #temp_CarSalesHeader_H1 AS H1
	INNER JOIN W_CarSlipStatusChange AS W on W.StatusChangeCode = @StatusChangeCode

	/*-------------------------------------------*/
	/* 車両明細データを取得						 */
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
	/* 古い車両伝票データを論理削除				 */
	/*-------------------------------------------*/

	UPDATE [dbo].[CarSalesLine]
	SET DelFlag = '1'
	WHERE DelFlag = '0'
	  AND SlipNumber = @SlipNumber

	/*-------------------------------------------------*/
	/* ステータスを戻した車両伝票データをINSERT		   */
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
	/* 車両支払データを取得						 */
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
	/* 古い車両支払データを論理削除				 */
	/*-------------------------------------------*/

	UPDATE [dbo].[CarSalesPayment]
	SET DelFlag = '1'
	WHERE DelFlag = '0'
	  AND SlipNumber = @SlipNumber

	/*-------------------------------------------------*/
	/* ステータスを戻した車両支払データをINSERT		   */
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
	/*-------------------------------------------------*/
	/* 納車済へ戻す場合は車両マスタも更新		   　　*/
	/*-------------------------------------------------*/
	--伝票ステータス=納車済からステータスを戻した場合
	--車両マスタの在庫ステータス、ロケーションを戻す
	IF (@SalesOrderStatus = '005')
	BEGIN
		UPDATE 
			[dbo].[SalesCar]
		SET 
			CarStatus = '006'		--納車済み
		   ,LocationCode = ''		--ロケーションコードは空文字
		   ,LastUpdateEmployeeCode = @EmployeeCode
		   ,LastUpdateDate = GETDATE()
		FROM 
			[dbo].[SalesCar] sc INNER JOIN 
			[dbo].[CarSalesHeader] h on h.DelFlag = '0' and h.SlipNumber = @SlipNumber and h.SalesCarNumber = sc.SalesCarNumber
		WHERE
			sc.DelFlag = '0'
	END

	/*-----------------------------*/
	/* 履歴テーブル更新			   */
	/*-----------------------------*/

	UPDATE W_CarSlipStatusChange
	SET ChangeStatus ='2',
		LastUpdateDate=GETDATE(),
		LastUpdateEmployeeCode=@EmployeeCode
	WHERE StatusChangeCode = @StatusChangeCode

	BEGIN
		DROP TABLE #temp_CarSalesHeader_H1
		DROP TABLE #temp_CarSalesLine_L1
		DROP TABLE #temp_CarSalesPayment_P1
	END

	END TRY
		BEGIN CATCH
		RETURN ERROR_NUMBER()
	END CATCH

	RETURN 0

END


GO


