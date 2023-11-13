USE [WPH_DB]
GO

/****** Object:  Table [dbo].[ServiceSalesHeader]    Script Date: 2023/08/12 9:46:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ServiceSalesHeader](
	[SlipNumber] [nvarchar](50) NOT NULL,
	[RevisionNumber] [int] NOT NULL,
	[CarSlipNumber] [nvarchar](50) NULL,
	[CarSalesOrderDate] [datetime] NULL,
	[QuoteDate] [datetime] NULL,
	[QuoteExpireDate] [datetime] NULL,
	[SalesOrderDate] [datetime] NULL,
	[ServiceOrderStatus] [nvarchar](3) NULL,
	[ArrivalPlanDate] [datetime] NULL,
	[ApprovalFlag] [nvarchar](3) NULL,
	[CampaignCode1] [nvarchar](20) NULL,
	[CampaignCode2] [nvarchar](20) NULL,
	[WorkingStartDate] [datetime] NULL,
	[WorkingEndDate] [datetime] NULL,
	[SalesDate] [datetime] NULL,
	[CustomerCode] [nvarchar](10) NOT NULL,
	[DepartmentCode] [nvarchar](3) NOT NULL,
	[CarEmployeeCode] [nvarchar](50) NULL,
	[FrontEmployeeCode] [nvarchar](50) NULL,
	[ReceiptionEmployeeCode] [nvarchar](50) NULL,
	[CarGradeCode] [nvarchar](20) NULL,
	[CarBrandName] [nvarchar](50) NULL,
	[CarName] [nvarchar](50) NULL,
	[CarGradeName] [nvarchar](100) NULL,
	[EngineType] [nvarchar](25) NULL,
	[ManufacturingYear] [nvarchar](4) NULL,
	[Vin] [nvarchar](20) NULL,
	[ModelName] [nvarchar](20) NULL,
	[Mileage] [decimal](10, 2) NULL,
	[MileageUnit] [nvarchar](3) NULL,
	[SalesPlanDate] [datetime] NULL,
	[FirstRegistration] [nvarchar](10) NULL,
	[NextInspectionDate] [datetime] NULL,
	[MorterViecleOfficialCode] [nvarchar](5) NULL,
	[RegistrationNumberType] [nvarchar](3) NULL,
	[RegistrationNumberKana] [nvarchar](1) NULL,
	[RegistrationNumberPlate] [nvarchar](4) NULL,
	[MakerShipmentDate] [datetime] NULL,
	[RegistrationPlanDate] [datetime] NULL,
	[RequestContent] [nvarchar](200) NULL,
	[CarTax] [decimal](10, 0) NULL,
	[CarLiabilityInsurance] [decimal](10, 0) NULL,
	[CarWeightTax] [decimal](10, 0) NULL,
	[FiscalStampCost] [decimal](10, 0) NULL,
	[InspectionRegistCost] [decimal](10, 0) NULL,
	[ParkingSpaceCost] [decimal](10, 0) NULL,
	[TradeInCost] [decimal](10, 0) NULL,
	[ReplacementFee] [decimal](10, 0) NULL,
	[InspectionRegistFee] [decimal](10, 0) NULL,
	[ParkingSpaceFee] [decimal](10, 0) NULL,
	[TradeInFee] [decimal](10, 0) NULL,
	[PreparationFee] [decimal](10, 0) NULL,
	[RecycleControlFee] [decimal](10, 0) NULL,
	[RecycleControlFeeTradeIn] [decimal](10, 0) NULL,
	[RequestNumberFee] [decimal](10, 0) NULL,
	[CarTaxUnexpiredAmount] [decimal](10, 0) NULL,
	[CarLiabilityInsuranceUnexpiredAmount] [decimal](10, 0) NULL,
	[LaborRate] [decimal](10, 0) NULL,
	[Memo] [nvarchar](400) NULL,
	[EngineerTotalAmount] [decimal](10, 0) NULL,
	[PartsTotalAmount] [decimal](10, 0) NULL,
	[SubTotalAmount] [decimal](10, 0) NULL,
	[TotalTaxAmount] [decimal](10, 0) NULL,
	[ServiceTotalAmount] [decimal](10, 0) NULL,
	[CostTotalAmount] [decimal](10, 0) NULL,
	[GrandTotalAmount] [decimal](10, 0) NULL,
	[PaymentTotalAmount] [decimal](10, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[SalesCarNumber] [nvarchar](50) NULL,
	[InspectionExpireDate] [datetime] NULL,
	[NumberPlateCost] [decimal](10, 0) NULL,
	[TaxFreeFieldName] [nvarchar](50) NULL,
	[TaxFreeFieldValue] [decimal](10, 0) NULL,
	[UsVin] [nvarchar](20) NULL,
	[ProcessSessionId] [uniqueidentifier] NULL,
	[ConsumptionTaxId] [nvarchar](3) NULL,
	[Rate] [smallint] NULL,
	[KeepsCarFlag] [bit] NOT NULL,
	[OptionalInsurance] [decimal](10, 0) NULL,
	[CarTaxMemo] [nvarchar](50) NULL,
	[CarLiabilityInsuranceMemo] [nvarchar](50) NULL,
	[CarWeightTaxMemo] [nvarchar](50) NULL,
	[NumberPlateCostMemo] [nvarchar](50) NULL,
	[FiscalStampCostMemo] [nvarchar](50) NULL,
	[OptionalInsuranceMemo] [nvarchar](50) NULL,
	[SubscriptionFee] [decimal](10, 0) NULL,
	[SubscriptionFeeMemo] [nvarchar](50) NULL,
	[TaxableFreeFieldValue] [decimal](10, 0) NULL,
	[TaxableFreeFieldName] [nvarchar](50) NULL,
	[TaxableCostTotalAmount] [decimal](10, 0) NULL,
	[CustomerClaimCode] [nvarchar](10) NULL,
 CONSTRAINT [PK_ServiceSalesHeader] PRIMARY KEY CLUSTERED 
(
	[SlipNumber] ASC,
	[RevisionNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ServiceSalesHeader] ADD  DEFAULT ((0)) FOR [KeepsCarFlag]
GO

ALTER TABLE [dbo].[ServiceSalesHeader] ADD  DEFAULT ('') FOR [CustomerClaimCode]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'見積改訂番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'RevisionNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarSlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両受注日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarSalesOrderDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'見積日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'QuoteDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'見積有効期限' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'QuoteExpireDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'受注日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesOrderDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票ステータス（見積・受注・納車・・・）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'ServiceOrderStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入庫日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'ArrivalPlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'承認フラグ（承認済み、未承認）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'ApprovalFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'イベントコード１' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CampaignCode1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'イベントコード２' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CampaignCode2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作業開始日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'WorkingStartDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作業終了日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'WorkingEndDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'納車日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CustomerCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'営業担当者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'フロント担当者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'FrontEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'受付担当者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'ReceiptionEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'グレードコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarGradeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ブランドコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarBrandName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車種名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'グレード名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarGradeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'原動機型式' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'EngineType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'年式' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'ManufacturingYear'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車台番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'Vin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'型式' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'ModelName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'走行距離' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'Mileage'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'走行距離単位（Km,Mile）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'MileageUnit'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'納車予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesPlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'初年度登録月(yyyymm)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'FirstRegistration'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'次回点検日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'NextInspectionDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'陸自局コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'MorterViecleOfficialCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録番号（種別）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'RegistrationNumberType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録番号（かな）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'RegistrationNumberKana'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録番号（プレート）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'RegistrationNumberPlate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカー出荷日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'MakerShipmentDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'RegistrationPlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'依頼内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'RequestContent'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【不課税】自動車税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【不課税】自賠責保険料' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarLiabilityInsurance'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【不課税】自動車重量税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarWeightTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【不課税】印紙代' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'FiscalStampCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【非課税】検査登録費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'InspectionRegistCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【非課税】車庫証明費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'ParkingSpaceCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【非課税】下取車費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【非課税】代行費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'ReplacementFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【課税】検査・登録手続代' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'InspectionRegistFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【課税】車庫証明手続代' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'ParkingSpaceFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【課税】下取車諸手続代' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'TradeInFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【課税】納車準備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'PreparationFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【課税】リサイクル資金管理料' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'RecycleControlFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【課税】リサイクル資金管理料（下取）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'RecycleControlFeeTradeIn'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【課税】希望番号費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'RequestNumberFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【課税】自動車税未経過相当額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarTaxUnexpiredAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'【課税】自賠責未経過相当額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarLiabilityInsuranceUnexpiredAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'レバレート' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'LaborRate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'引き継ぎメモ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'技術料合計' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'EngineerTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品合計' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'PartsTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'整備料小計' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'SubTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'消費税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'TotalTaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'整備料合計' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'ServiceTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'諸費用合計' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CostTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求合計' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'GrandTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払合計' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'PaymentTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'管理番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'SalesCarNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車検有効期限' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'InspectionExpireDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ナンバー代' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'NumberPlateCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'非課税自由項目名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxFreeFieldName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'非課税自由項目値' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxFreeFieldValue'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'VIN(北米用)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'UsVin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票ロックID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'ProcessSessionId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'任意保険' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'OptionalInsurance'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自動車税備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarTaxMemo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自賠責備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarLiabilityInsuranceMemo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自動車重量税備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CarWeightTaxMemo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ナンバー代備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'NumberPlateCostMemo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'印紙代備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'FiscalStampCostMemo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'任意保険備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'OptionalInsuranceMemo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'サービス加入料' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'SubscriptionFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'サービス加入料備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'SubscriptionFeeMemo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'課税自由項目金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxableFreeFieldValue'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'課税自由項目名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxableFreeFieldName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'諸費用（課税）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'TaxableCostTotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'サービス伝票ヘッダ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesHeader'
GO


