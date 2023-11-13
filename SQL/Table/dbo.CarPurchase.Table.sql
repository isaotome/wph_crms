USE [WPH_DB]
GO

/****** Object:  Table [dbo].[CarPurchase]    Script Date: 2018/06/29 11:41:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[CarPurchase](
	[CarPurchaseId] [uniqueidentifier] NOT NULL,
	[CarPurchaseOrderNumber] [varchar](50) NULL,
	[CarAppraisalId] [uniqueidentifier] NULL,
	[PurchaseStatus] [nvarchar](3) NULL,
	[PurchaseDate] [datetime] NULL,
	[SupplierCode] [nvarchar](10) NULL,
	[PurchaseLocationCode] [nvarchar](12) NULL,
	[DepartmentCode] [nvarchar](3) NULL,
	[EmployeeCode] [nvarchar](50) NULL,
	[VehiclePrice] [decimal](10, 0) NOT NULL,
	[MetallicPrice] [decimal](10, 0) NOT NULL,
	[OptionPrice] [decimal](10, 0) NOT NULL,
	[FirmPrice] [decimal](10, 0) NOT NULL,
	[DiscountPrice] [decimal](10, 0) NOT NULL,
	[EquipmentPrice] [decimal](10, 0) NOT NULL,
	[RepairPrice] [decimal](10, 0) NOT NULL,
	[OthersPrice] [decimal](10, 0) NOT NULL,
	[Amount] [decimal](10, 0) NOT NULL,
	[TaxAmount] [decimal](10, 0) NOT NULL,
	[SalesCarNumber] [nvarchar](50) NOT NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[EraseRegist] [nvarchar](3) NULL,
	[Memo] [nvarchar](100) NULL,
	[SlipDate] [datetime] NULL,
	[CarTaxAppropriateAmount] [decimal](10, 0) NULL,
	[RecycleAmount] [decimal](10, 0) NULL,
	[CarPurchaseType] [nvarchar](3) NULL,
	[VehicleTax] [decimal](10, 0) NULL,
	[VehicleAmount] [decimal](10, 0) NULL,
	[MetallicTax] [decimal](10, 0) NULL,
	[MetallicAmount] [decimal](10, 0) NULL,
	[OptionTax] [decimal](10, 0) NULL,
	[OptionAmount] [decimal](10, 0) NULL,
	[FirmTax] [decimal](10, 0) NULL,
	[FirmAmount] [decimal](10, 0) NULL,
	[DiscountTax] [decimal](10, 0) NULL,
	[DiscountAmount] [decimal](10, 0) NULL,
	[OthersTax] [decimal](10, 0) NULL,
	[OthersAmount] [decimal](10, 0) NULL,
	[TotalAmount] [decimal](10, 0) NULL,
	[AuctionFeePrice] [decimal](10, 0) NULL,
	[AuctionFeeTax] [decimal](10, 0) NULL,
	[AuctionFeeAmount] [decimal](10, 0) NULL,
	[CarTaxAppropriatePrice] [decimal](10, 0) NULL,
	[RecyclePrice] [decimal](10, 0) NULL,
	[EquipmentTax] [decimal](10, 0) NULL,
	[EquipmentAmount] [decimal](10, 0) NULL,
	[RepairTax] [decimal](10, 0) NULL,
	[RepairAmount] [decimal](10, 0) NULL,
	[CarTaxAppropriateTax] [decimal](10, 0) NULL,
	[ConsumptionTaxId] [nvarchar](3) NULL,
	[Rate] [smallint] NULL,
	[CancelFlag] [nvarchar](2) NULL,
	[SlipNumber] [nvarchar](50) NULL DEFAULT (''),
	[LastEditScreen] [nvarchar](3) NOT NULL DEFAULT ('000'),
	[RegistOwnFlag] [nvarchar](2) NULL DEFAULT ('0'),
	[CancelDate] [datetime] NULL,
	[CancelCarPurchaseId] [nvarchar](36) NULL,
	[CancelMemo] [nvarchar](100) NULL,
	[FinancialAmount] [decimal](10, 0) NULL,
 CONSTRAINT [PK_CarPurchase] PRIMARY KEY CLUSTERED 
(
	[CarPurchaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両仕入ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarPurchaseId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両発注引当ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarPurchaseOrderNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両査定ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarAppraisalId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入ステータス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'SupplierCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入ロケーションコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseLocationCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'担当者コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両本体価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'VehiclePrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メタリック価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'MetallicPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'オプション価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'OptionPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ファーム価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'FirmPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ディスカウント価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'DiscountPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'加装価格(税抜)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'EquipmentPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'加修価格(税抜)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'RepairPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'その他価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'OthersPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'消費税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'TaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'管理番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'SalesCarNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'抹消登録' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'EraseRegist'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'SlipDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自税充当税込' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarTaxAppropriateAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'リサイクル税込' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'RecycleAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入庫区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarPurchaseType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両本体消費税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'VehicleTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両本体税込価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'VehicleAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メタリック消費税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'MetallicTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メタリック税込価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'MetallicAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'オプション消費税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'OptionTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'オプション税込価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'OptionAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ファーム消費税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'FirmTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ファーム税込価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'FirmAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ディスカウント消費税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'DiscountTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ディスカウント税込価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'DiscountAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'その他消費税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'OthersTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'その他税込価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'OthersAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入税込価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'TotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'オークション落札料' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'AuctionFeePrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'オークション落札料消費税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'AuctionFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'オークション落札料税込' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'AuctionFeeAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自税充当' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarTaxAppropriatePrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'リサイクル' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'RecyclePrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'加装価格(消費税)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'EquipmentTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'加装価格(税込)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'EquipmentAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'加修価格(消費税)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'RepairTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'加修価格(税込)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'RepairAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自税充当消費税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarTaxAppropriateTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'財務価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'FinancialAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'車両仕入' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase'
GO


