USE [WPH_DB]
GO

/****** Object:  Table [dbo].[CarSalesHeader]    Script Date: 2023/10/17 16:07:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CarSalesHeader](
	[SlipNumber] [nvarchar](50) NOT NULL,
	[RevisionNumber] [int] NOT NULL,
	[QuoteDate] [datetime] NULL,
	[QuoteExpireDate] [datetime] NULL,
	[SalesOrderDate] [datetime] NULL,
	[SalesOrderStatus] [nvarchar](3) NULL,
	[ApprovalFlag] [nvarchar](3) NULL,
	[SalesDate] [datetime] NULL,
	[CustomerCode] [nvarchar](10) NULL,
	[DepartmentCode] [nvarchar](3) NULL,
	[EmployeeCode] [nvarchar](50) NULL,
	[CampaignCode1] [nvarchar](20) NULL,
	[CampaignCode2] [nvarchar](20) NULL,
	[NewUsedType] [nvarchar](3) NULL,
	[SalesType] [nvarchar](3) NULL,
	[MakerName] [nvarchar](50) NULL,
	[CarBrandName] [nvarchar](50) NULL,
	[CarName] [nvarchar](50) NULL,
	[CarGradeName] [nvarchar](100) NULL,
	[CarGradeCode] [nvarchar](30) NULL,
	[ManufacturingYear] [nvarchar](10) NULL,
	[ExteriorColorCode] [nvarchar](8) NULL,
	[ExteriorColorName] [nvarchar](50) NULL,
	[InteriorColorCode] [nvarchar](8) NULL,
	[InteriorColorName] [nvarchar](50) NULL,
	[Vin] [nvarchar](20) NULL,
	[UsVin] [nvarchar](20) NULL,
	[ModelName] [nvarchar](20) NULL,
	[Mileage] [decimal](12, 2) NULL,
	[MileageUnit] [nvarchar](3) NULL,
	[RequestPlateNumber] [nvarchar](10) NULL,
	[RegistPlanDate] [nvarchar](10) NULL,
	[HotStatus] [nvarchar](3) NULL,
	[SalesCarNumber] [nvarchar](50) NULL,
	[RequestRegistDate] [datetime] NULL,
	[SalesPlanDate] [datetime] NULL,
	[RegistrationType] [nvarchar](3) NULL,
	[MorterViecleOfficialCode] [nvarchar](10) NULL,
	[OwnershipReservation] [nvarchar](3) NULL,
	[CarLiabilityInsuranceType] [nvarchar](3) NULL,
	[SealSubmitDate] [datetime] NULL,
	[ProxySubmitDate] [datetime] NULL,
	[ParkingSpaceSubmitDate] [datetime] NULL,
	[CarLiabilityInsuranceSubmitDate] [datetime] NULL,
	[OwnershipReservationSubmitDate] [datetime] NULL,
	[Memo] [nvarchar](100) NULL,
	[SalesPrice] [decimal](10, 0) NULL,
	[DiscountAmount] [decimal](10, 0) NULL,
	[TaxationAmount] [decimal](10, 0) NULL,
	[TaxAmount] [decimal](10, 0) NULL,
	[ShopOptionAmount] [decimal](10, 0) NULL,
	[ShopOptionTaxAmount] [decimal](10, 0) NULL,
	[MakerOptionAmount] [decimal](10, 0) NULL,
	[MakerOptionTaxAmount] [decimal](10, 0) NULL,
	[OutSourceAmount] [decimal](10, 0) NULL,
	[OutSourceTaxAmount] [decimal](10, 0) NULL,
	[SubTotalAmount] [decimal](10, 0) NULL,
	[CarTax] [decimal](10, 0) NULL,
	[CarLiabilityInsurance] [decimal](10, 0) NULL,
	[CarWeightTax] [decimal](10, 0) NULL,
	[AcquisitionTax] [decimal](10, 0) NULL,
	[InspectionRegistCost] [decimal](10, 0) NULL,
	[ParkingSpaceCost] [decimal](10, 0) NULL,
	[TradeInCost] [decimal](10, 0) NULL,
	[RecycleDeposit] [decimal](10, 0) NULL,
	[RecycleDepositTradeIn] [decimal](10, 0) NULL,
	[NumberPlateCost] [decimal](10, 0) NULL,
	[RequestNumberCost] [decimal](10, 0) NULL,
	[TradeInFiscalStampCost] [decimal](10, 0) NULL,
	[TaxFreeFieldName] [nvarchar](50) NULL,
	[TaxFreeFieldValue] [decimal](10, 0) NULL,
	[TaxFreeTotalAmount] [decimal](10, 0) NULL,
	[InspectionRegistFee] [decimal](10, 0) NULL,
	[ParkingSpaceFee] [decimal](10, 0) NULL,
	[TradeInFee] [decimal](10, 0) NULL,
	[PreparationFee] [decimal](10, 0) NULL,
	[RecycleControlFee] [decimal](10, 0) NULL,
	[RecycleControlFeeTradeIn] [decimal](10, 0) NULL,
	[RequestNumberFee] [decimal](10, 0) NULL,
	[CarTaxUnexpiredAmount] [decimal](10, 0) NULL,
	[CarLiabilityInsuranceUnexpiredAmount] [decimal](10, 0) NULL,
	[TradeInAppraisalFee] [decimal](10, 0) NULL,
	[FarRegistFee] [decimal](10, 0) NULL,
	[TradeInMaintenanceFee] [decimal](10, 0) NULL,
	[InheritedInsuranceFee] [decimal](10, 0) NULL,
	[TaxationFieldName] [nvarchar](50) NULL,
	[TaxationFieldValue] [decimal](10, 0) NULL,
	[SalesCostTotalAmount] [decimal](10, 0) NULL,
	[SalesCostTotalTaxAmount] [decimal](10, 0) NULL,
	[OtherCostTotalAmount] [decimal](10, 0) NULL,
	[CostTotalAmount] [decimal](10, 0) NULL,
	[TotalTaxAmount] [decimal](10, 0) NULL,
	[GrandTotalAmount] [decimal](10, 0) NULL,
	[PossesorCode] [nvarchar](10) NULL,
	[UserCode] [nvarchar](10) NULL,
	[PrincipalPlace] [nvarchar](100) NULL,
	[VoluntaryInsuranceType] [nvarchar](3) NULL,
	[VoluntaryInsuranceCompanyName] [nvarchar](50) NULL,
	[VoluntaryInsuranceAmount] [decimal](10, 0) NULL,
	[VoluntaryInsuranceTermFrom] [datetime] NULL,
	[VoluntaryInsuranceTermTo] [datetime] NULL,
	[PaymentPlanType] [nvarchar](3) NULL,
	[TradeInAmount1] [decimal](10, 0) NULL,
	[TradeInTax1] [decimal](10, 0) NULL,
	[TradeInUnexpiredCarTax1] [decimal](10, 0) NULL,
	[TradeInRemainDebt1] [decimal](10, 0) NULL,
	[TradeInAppropriation1] [decimal](10, 0) NULL,
	[TradeInRecycleAmount1] [decimal](10, 0) NULL,
	[TradeInMakerName1] [nvarchar](50) NULL,
	[TradeInCarName1] [nvarchar](50) NULL,
	[TradeInClassificationTypeNumber1] [nvarchar](50) NULL,
	[TradeInModelSpecificateNumber1] [nvarchar](50) NULL,
	[TradeInManufacturingYear1] [nvarchar](50) NULL,
	[TradeInInspectionExpiredDate1] [datetime] NULL,
	[TradeInMileage1] [decimal](10, 2) NULL,
	[TradeInMileageUnit1] [nvarchar](3) NULL,
	[TradeInVin1] [nvarchar](20) NULL,
	[TradeInRegistrationNumber1] [nvarchar](20) NULL,
	[TradeInUnexpiredLiabilityInsurance1] [decimal](10, 0) NULL,
	[TradeInAmount2] [decimal](10, 0) NULL,
	[TradeInTax2] [decimal](10, 0) NULL,
	[TradeInUnexpiredCarTax2] [decimal](10, 0) NULL,
	[TradeInRemainDebt2] [decimal](10, 0) NULL,
	[TradeInAppropriation2] [decimal](10, 0) NULL,
	[TradeInRecycleAmount2] [decimal](10, 0) NULL,
	[TradeInMakerName2] [nvarchar](50) NULL,
	[TradeInCarName2] [nvarchar](50) NULL,
	[TradeInClassificationTypeNumber2] [nvarchar](50) NULL,
	[TradeInModelSpecificateNumber2] [nvarchar](50) NULL,
	[TradeInManufacturingYear2] [nvarchar](50) NULL,
	[TradeInInspectionExpiredDate2] [datetime] NULL,
	[TradeInMileage2] [decimal](10, 2) NULL,
	[TradeInMileageUnit2] [nvarchar](3) NULL,
	[TradeInVin2] [nvarchar](20) NULL,
	[TradeInRegistrationNumber2] [nvarchar](20) NULL,
	[TradeInUnexpiredLiabilityInsurance2] [decimal](10, 0) NULL,
	[TradeInAmount3] [decimal](10, 0) NULL,
	[TradeInTax3] [decimal](10, 0) NULL,
	[TradeInUnexpiredCarTax3] [decimal](10, 0) NULL,
	[TradeInRemainDebt3] [decimal](10, 0) NULL,
	[TradeInAppropriation3] [decimal](10, 0) NULL,
	[TradeInRecycleAmount3] [decimal](10, 0) NULL,
	[TradeInMakerName3] [nvarchar](50) NULL,
	[TradeInCarName3] [nvarchar](50) NULL,
	[TradeInClassificationTypeNumber3] [nvarchar](50) NULL,
	[TradeInModelSpecificateNumber3] [nvarchar](50) NULL,
	[TradeInManufacturingYear3] [nvarchar](50) NULL,
	[TradeInInspectionExpiredDate3] [datetime] NULL,
	[TradeInMileage3] [decimal](10, 2) NULL,
	[TradeInMileageUnit3] [nvarchar](3) NULL,
	[TradeInVin3] [nvarchar](20) NULL,
	[TradeInRegistrationNumber3] [nvarchar](20) NULL,
	[TradeInUnexpiredLiabilityInsurance3] [decimal](10, 0) NULL,
	[TradeInTotalAmount] [decimal](10, 0) NULL,
	[TradeInTaxTotalAmount] [decimal](10, 0) NULL,
	[TradeInUnexpiredCarTaxTotalAmount] [decimal](10, 0) NULL,
	[TradeInRemainDebtTotalAmount] [decimal](10, 0) NULL,
	[TradeInAppropriationTotalAmount] [decimal](10, 0) NULL,
	[PaymentTotalAmount] [decimal](10, 0) NULL,
	[PaymentCashTotalAmount] [decimal](10, 0) NULL,
	[LoanPrincipalAmount] [decimal](10, 0) NULL,
	[LoanFeeAmount] [decimal](10, 0) NULL,
	[LoanTotalAmount] [decimal](10, 0) NULL,
	[LoanCodeA] [nvarchar](10) NULL,
	[PaymentFrequencyA] [int] NULL,
	[PaymentTermFromA] [datetime] NULL,
	[PaymentTermToA] [datetime] NULL,
	[BonusMonthA1] [int] NULL,
	[BonusMonthA2] [int] NULL,
	[FirstAmountA] [decimal](10, 0) NULL,
	[SecondAmountA] [decimal](10, 0) NULL,
	[BonusAmountA] [decimal](10, 0) NULL,
	[CashAmountA] [decimal](10, 0) NULL,
	[LoanPrincipalA] [decimal](10, 0) NULL,
	[LoanFeeA] [decimal](10, 0) NULL,
	[LoanTotalAmountA] [decimal](10, 0) NULL,
	[AuthorizationNumberA] [nvarchar](20) NULL,
	[FirstDirectDebitDateA] [datetime] NULL,
	[SecondDirectDebitDateA] [int] NULL,
	[LoanCodeB] [nvarchar](10) NULL,
	[PaymentFrequencyB] [int] NULL,
	[PaymentTermFromB] [datetime] NULL,
	[PaymentTermToB] [datetime] NULL,
	[BonusMonthB1] [int] NULL,
	[BonusMonthB2] [int] NULL,
	[FirstAmountB] [decimal](10, 0) NULL,
	[SecondAmountB] [decimal](10, 0) NULL,
	[BonusAmountB] [decimal](10, 0) NULL,
	[CashAmountB] [decimal](10, 0) NULL,
	[LoanPrincipalB] [decimal](10, 0) NULL,
	[LoanFeeB] [decimal](10, 0) NULL,
	[LoanTotalAmountB] [decimal](10, 0) NULL,
	[AuthorizationNumberB] [nvarchar](20) NULL,
	[FirstDirectDebitDateB] [datetime] NULL,
	[SecondDirectDebitDateB] [int] NULL,
	[LoanCodeC] [nvarchar](10) NULL,
	[PaymentFrequencyC] [int] NULL,
	[PaymentTermFromC] [datetime] NULL,
	[PaymentTermToC] [datetime] NULL,
	[BonusMonthC1] [int] NULL,
	[BonusMonthC2] [int] NULL,
	[FirstAmountC] [decimal](10, 0) NULL,
	[SecondAmountC] [decimal](10, 0) NULL,
	[BonusAmountC] [decimal](10, 0) NULL,
	[CashAmountC] [decimal](10, 0) NULL,
	[LoanPrincipalC] [decimal](10, 0) NULL,
	[LoanFeeC] [decimal](10, 0) NULL,
	[LoanTotalAmountC] [decimal](10, 0) NULL,
	[AuthorizationNumberC] [nvarchar](20) NULL,
	[FirstDirectDebitDateC] [datetime] NULL,
	[SecondDirectDebitDateC] [int] NULL,
	[CancelDate] [datetime] NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[InspectionRegistFeeTax] [decimal](10, 0) NULL,
	[ParkingSpaceFeeTax] [decimal](10, 0) NULL,
	[TradeInFeeTax] [decimal](10, 0) NULL,
	[PreparationFeeTax] [decimal](10, 0) NULL,
	[RecycleControlFeeTax] [decimal](10, 0) NULL,
	[RecycleControlFeeTradeInTax] [decimal](10, 0) NULL,
	[RequestNumberFeeTax] [decimal](10, 0) NULL,
	[CarTaxUnexpiredAmountTax] [decimal](10, 0) NULL,
	[CarLiabilityInsuranceUnexpiredAmountTax] [decimal](10, 0) NULL,
	[TradeInAppraisalFeeTax] [decimal](10, 0) NULL,
	[FarRegistFeeTax] [decimal](10, 0) NULL,
	[TradeInMaintenanceFeeTax] [decimal](10, 0) NULL,
	[InheritedInsuranceFeeTax] [decimal](10, 0) NULL,
	[TaxationFieldValueTax] [decimal](10, 0) NULL,
	[TradeInEraseRegist1] [nvarchar](3) NULL,
	[TradeInEraseRegist2] [nvarchar](3) NULL,
	[TradeInEraseRegist3] [nvarchar](3) NULL,
	[RemainAmountA] [decimal](10, 0) NULL,
	[RemainAmountB] [decimal](10, 0) NULL,
	[RemainAmountC] [decimal](10, 0) NULL,
	[RemainFinalMonthA] [datetime] NULL,
	[RemainFinalMonthB] [datetime] NULL,
	[RemainFinalMonthC] [datetime] NULL,
	[LoanRateA] [decimal](6, 3) NULL,
	[LoanRateB] [decimal](6, 3) NULL,
	[LoanRateC] [decimal](6, 3) NULL,
	[SalesTax] [decimal](10, 0) NULL,
	[DiscountTax] [decimal](10, 0) NULL,
	[TradeInPrice1] [decimal](10, 0) NULL,
	[TradeInPrice2] [decimal](10, 0) NULL,
	[TradeInPrice3] [decimal](10, 0) NULL,
	[TradeInRecycleTotalAmount] [decimal](10, 0) NULL,
	[ConsumptionTaxId] [nvarchar](3) NULL,
	[Rate] [smallint] NULL,
	[RevenueStampCost] [decimal](10, 0) NULL,
	[TradeInCarTaxDeposit] [decimal](10, 0) NULL,
	[LastEditScreen] [nvarchar](3) NOT NULL,
	[PaymentSecondFrequencyA] [int] NULL,
	[PaymentSecondFrequencyB] [int] NULL,
	[PaymentSecondFrequencyC] [int] NULL,
	[ProcessSessionId] [uniqueidentifier] NULL,
	[EPDiscountTaxId] [nvarchar](3) NULL,
	[CostAreaCode] [nvarchar](3) NULL,
	[MaintenancePackageAmount] [decimal](10, 0) NULL,
	[MaintenancePackageTaxAmount] [decimal](10, 0) NULL,
	[ExtendedWarrantyAmount] [decimal](10, 0) NULL,
	[ExtendedWarrantyTaxAmount] [decimal](10, 0) NULL,
	[TradeInHolderName1] [nvarchar](80) NULL,
	[TradeInHolderName2] [nvarchar](80) NULL,
	[TradeInHolderName3] [nvarchar](80) NULL,
	[OutJurisdictionRegistFee] [decimal](10, 0) NULL,
	[OutJurisdictionRegistFeeTax] [decimal](10, 0) NULL,
	[SurchargeAmount] [decimal](10, 0) NULL,
	[SurchargeTaxAmount] [decimal](10, 0) NULL,
	[SuspendTaxRecv] [decimal](10, 0) NULL,
 CONSTRAINT [PK_CarSalesHeader] PRIMARY KEY CLUSTERED 
(
	[SlipNumber] ASC,
	[RevisionNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CarSalesHeader] ADD  DEFAULT ('000') FOR [LastEditScreen]
GO

ALTER TABLE [dbo].[CarSalesHeader] ADD  DEFAULT ('') FOR [ProcessSessionId]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ��`�[�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RevisionNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ϓ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'QuoteDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ϗL������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'QuoteExpireDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�󒍓�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesOrderDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ��`�[�X�e�[�^�X' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesOrderStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���F�t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ApprovalFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�[�ԓ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ڋq�R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CustomerCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�S���҃R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�C�x���g�R�[�h�P' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CampaignCode1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�C�x���g�R�[�h�Q' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CampaignCode2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�V���敪' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'NewUsedType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�̔��敪' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���[�J�[��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'MakerName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�u�����h��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarBrandName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ԏ햼' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O���[�h��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarGradeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O���[�h�R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarGradeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�N��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ManufacturingYear'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O���F�R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ExteriorColorCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O���F��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ExteriorColorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����F�R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'InteriorColorCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����F��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'InteriorColorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԑ�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'Vin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԑ�ԍ��i�k�ėp�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'UsVin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�^��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ModelName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���s����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'Mileage'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���s�����P��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'MileageUnit'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��]�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RequestPlateNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�^�\���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RegistPlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�z�b�g�Ǘ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'HotStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ǘ��ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesCarNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�^��]��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RequestRegistDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�[�ԗ\���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesPlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�^���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RegistrationType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�^�x�ǃR�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'MorterViecleOfficialCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���L������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'OwnershipReservation'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����ӕی�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarLiabilityInsuranceType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��ӏؖ���o��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SealSubmitDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ϔC���o��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ProxySubmitDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ԍɏؖ���o��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ParkingSpaceSubmitDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����ӕی���o��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarLiabilityInsuranceSubmitDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���L�����ۏ��ޒ�o��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'OwnershipReservationSubmitDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���l' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ��{�̉��i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�l���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'DiscountAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ېőΏۊz' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxationAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����Ŋz' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�̔��X�I�v�V�������v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ShopOptionAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�̔��X�I�v�V���������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ShopOptionTaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���[�J�[�I�v�V�������v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'MakerOptionAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���[�J�[�I�v�V���������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'MakerOptionTaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����E���C���v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'OutSourceAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����E���C�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'OutSourceTaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�I�v�V�������v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SubTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����Ԑ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����ӕی���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarLiabilityInsurance'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����ԏd�ʐ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarWeightTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����Ԏ擾��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'AcquisitionTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����o�^�󎆑�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'InspectionRegistCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ԍɏؖ��؎���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ParkingSpaceCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����ԓo�^�󎆑�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���T�C�N���a����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RecycleDeposit'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���T�C�N���a�����i����j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RecycleDepositTradeIn'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�i���o�[�v���[�g��i��ʁj' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'NumberPlateCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�i���o�[�v���[�g��i��]�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RequestNumberCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����Ԉ󎆑�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInFiscalStampCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��ېŎ��R���ږ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxFreeFieldName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��ېō��ڒl' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxFreeFieldValue'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŋ������v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxFreeTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����o�^�葱��s��p' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'InspectionRegistFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ԍɏؖ��葱��s��p' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ParkingSpaceFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����ԏ��葱��p' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�[�ԏ�����p' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PreparationFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���T�C�N�������Ǘ���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RecycleControlFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���T�C�N���Ǘ���p�i����j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RecycleControlFeeTradeIn'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��]�i���o�[�\���萔��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RequestNumberFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���o�ߎ����Ԑ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarTaxUnexpiredAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���o�ߎ����ӕی���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarLiabilityInsuranceUnexpiredAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����ԍ����p' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInAppraisalFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����o�^��s��p' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'FarRegistFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���Îԓ_���E������p' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInMaintenanceFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ÎԌp��������p' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'InheritedInsuranceFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ېŎ��R���ږ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxationFieldName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ېŎ��R���ڒl' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxationFieldValue'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�̔�����p���v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesCostTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�̔�����p�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesCostTotalTaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���̑���p���v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'OtherCostTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����p���v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CostTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����ō��v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TotalTaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����̔����v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'GrandTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���L�҃R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PossesorCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�g�p�҃R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'UserCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�{���n' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PrincipalPlace'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�C�ӕی�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'VoluntaryInsuranceType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�C�ӕی���Ж�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'VoluntaryInsuranceCompanyName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�C�ӕی����i�N�z�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'VoluntaryInsuranceAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�C�ӕی��J�n��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'VoluntaryInsuranceTermFrom'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�C�ӕی��I����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'VoluntaryInsuranceTermTo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���[���v����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentPlanType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����ԉ��i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInAmount1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����ԏ����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInTax1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����Ԗ��������Ԑ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInUnexpiredCarTax1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����Ԏc��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInRemainDebt1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����[���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInAppropriation1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����ԃ��T�C�N������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInRecycleAmount1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����ԃ��[�J�[��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInMakerName1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����ԎԎ햼' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInCarName1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����ԗޕʋ敪' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInClassificationTypeNumber1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����Ԍ^���w��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInModelSpecificateNumber1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����ԔN��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInManufacturingYear1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����ԎԌ��L������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInInspectionExpiredDate1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����ԑ��s����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInMileage1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����ԑ��s�����P��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInMileageUnit1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����Ԏԑ�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInVin1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz�o�^�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInRegistrationNumber1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz�����Ӗ��o�ߕ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInUnexpiredLiabilityInsurance1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����ԉ��i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInAmount2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����ԏ����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInTax2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����Ԗ��������Ԑ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInUnexpiredCarTax2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����Ԏc��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInRemainDebt2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����[���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInAppropriation2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����ԃ��T�C�N������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInRecycleAmount2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����ԃ��[�J�[��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInMakerName2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����ԎԎ햼' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInCarName2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����ԗޕʋ敪' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInClassificationTypeNumber2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����Ԍ^���w��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInModelSpecificateNumber2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����ԔN��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInManufacturingYear2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz�Ԍ��L������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInInspectionExpiredDate2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����ԑ��s����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInMileage2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����ԑ��s�����P��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInMileageUnit2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����Ԏԑ�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInVin2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz�o�^�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInRegistrationNumber2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz�����Ӗ��o�ߕ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInUnexpiredLiabilityInsurance2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����ԉ��i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInAmount3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����ԏ����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInTax3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����Ԗ��������Ԑ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInUnexpiredCarTax3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����Ԏc��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInRemainDebt3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����[���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInAppropriation3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����ԃ��T�C�N������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInRecycleAmount3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����ԃ��[�J�[��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInMakerName3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����ԎԎ햼' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInCarName3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����ԗޕʋ敪' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInClassificationTypeNumber3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����Ԍ^���w��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInModelSpecificateNumber3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����ԔN��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInManufacturingYear3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz�Ԍ��L������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInInspectionExpiredDate3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����ԑ��s����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInMileage3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����ԑ��s�����P��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInMileageUnit3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����Ԏԑ�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInVin3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz�o�^�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInRegistrationNumber3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz�����Ӗ��o�ߕ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInUnexpiredLiabilityInsurance3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����ԍ��v���i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����ԏ���ō��v�z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInTaxTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����Ԗ��������Ԑō��v�z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInUnexpiredCarTaxTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����Ԏc���v�z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInRemainDebtTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����[�����v�z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInAppropriationTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����x�����z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�x���\�葍�z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentCashTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���[������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanPrincipalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���[���萔��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanFeeAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���[�����v���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z���[���R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanCodeA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z�x����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentFrequencyA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z�x�����ԁiFROM�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentTermFromA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z�x�����ԁiTO�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentTermToA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z�{�[�i�X���P' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'BonusMonthA1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z�{�[�i�X���Q' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'BonusMonthA2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z������z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'FirstAmountA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z2��ڈȍ~���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SecondAmountA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z�{�[�i�X�����z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'BonusAmountA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z�����i�\�������܂ށj' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CashAmountA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z���[������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanPrincipalA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z���[���萔��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanFeeA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z���[�����v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanTotalAmountA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z���F�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'AuthorizationNumberA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z���������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'FirstDirectDebitDateA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z2��ڈȍ~������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SecondDirectDebitDateA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z���[���R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanCodeB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z�x����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentFrequencyB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z�x�����ԁiFROM�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentTermFromB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z�x�����ԁiTO�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentTermToB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z�{�[�i�X���P' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'BonusMonthB1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z�{�[�i�X���Q' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'BonusMonthB2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z������z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'FirstAmountB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z2��ڈȍ~���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SecondAmountB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z�{�[�i�X�����z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'BonusAmountB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z�����i�\�������܂ށj' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CashAmountB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z���[������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanPrincipalB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z���[���萔��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanFeeB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z���[�����v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanTotalAmountB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z���F�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'AuthorizationNumberB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z���������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'FirstDirectDebitDateB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z2��ڈȍ~������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SecondDirectDebitDateB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z���[���R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanCodeC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z�x����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentFrequencyC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z�x�����ԁiFROM�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentTermFromC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z�x�����ԁiTO�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentTermToC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z�{�[�i�X���P' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'BonusMonthC1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z�{�[�i�X���Q' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'BonusMonthC2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z������z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'FirstAmountC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z2��ڈȍ~���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SecondAmountC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z�{�[�i�X�����z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'BonusAmountC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z�����i�\�������܂ށj' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CashAmountC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z���[������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanPrincipalC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z���[���萔��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanFeeC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z���[�����v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanTotalAmountC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z���F�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'AuthorizationNumberC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z���������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'FirstDirectDebitDateC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z2��ڈȍ~������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SecondDirectDebitDateC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�L�����Z����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CancelDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�폜�t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����E�o�^�葱�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'InspectionRegistFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ԍɏؖ��葱�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ParkingSpaceFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����葱�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�[�ԏ�����p�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'PreparationFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���T�C�N���Ǘ���p�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RecycleControlFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���T�C�N���Ǘ���p(����)�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RecycleControlFeeTradeInTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��]�ԍ���p�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RequestNumberFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���o�ߎ����Ԑŏ����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarTaxUnexpiredAmountTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���o�ߎ����ӕی��������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CarLiabilityInsuranceUnexpiredAmountTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���捸���p�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInAppraisalFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����o�^��p�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'FarRegistFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ÎԐ�����p�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInMaintenanceFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ۏ،p��������p�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'InheritedInsuranceFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ېŎ��R���ڏ����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxationFieldValueTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����Ԗ����o�^1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInEraseRegist1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����Ԗ����o�^2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInEraseRegist2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����Ԗ����o�^3' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInEraseRegist3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z�c���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RemainAmountA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z�c���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RemainAmountB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z�c���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RemainAmountC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z�c���ŏI��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RemainFinalMonthA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z�c���ŏI��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RemainFinalMonthB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z�c���ŏI��3' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'RemainFinalMonthC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����A�z���[������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanRateA'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����B�z���[������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanRateB'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y�v����C�z���[������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'LoanRateC'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ��{�̏����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�l�������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'DiscountTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y1��ځz����ԉ��i(�Ŕ�)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInPrice1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y2��ځz����ԉ��i(�Ŕ�)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInPrice2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�y3��ځz����ԉ��i(�Ŕ�)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInPrice3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����ԃ��T�C�N�����v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInRecycleTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�`�[���b�NID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ProcessSessionId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�^�s���{��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'CostAreaCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�T�[�r�X�v���O�����������i�����e�i���X�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'MaintenancePackageAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�T�[�r�X�v���O�����������i�����e�i���X�j�Ŋz' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'MaintenancePackageTaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�T�[�r�X�v���O�����������i�����ۏ؁j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ExtendedWarrantyAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�T�[�r�X�v���O�����������i�����ۏ؁j�Ŋz' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'ExtendedWarrantyTaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�^���`�l(����ԂP���)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInHolderName1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�^���`�l(����ԂQ���)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInHolderName2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�^���`�l(����ԂR���)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInHolderName3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ǌ��O�o�^�葱��p' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'OutJurisdictionRegistFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ǌ��O�o�^�葱��p�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'OutJurisdictionRegistFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ʃT�[�`���[�W' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SurchargeAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ʃT�[�`���[�W�Ŋz' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SurchargeTaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader', @level2type=N'COLUMN',@level2name=N'SuspendTaxRecv'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'�ԗ��`�[�w�b�_' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'�g�����U�N�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesHeader'
GO


