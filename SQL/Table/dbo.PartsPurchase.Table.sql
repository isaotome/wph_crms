USE [WPH_DB]
GO

/****** Object:  Table [dbo].[PartsPurchase]    Script Date: 2018/04/02 11:24:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PartsPurchase](
	[PurchaseNumber] [nvarchar](50) NOT NULL,
	[PurchaseOrderNumber] [nvarchar](50) NULL,
	[PurchaseType] [nvarchar](3) NOT NULL,
	[PurchasePlanDate] [datetime] NULL,
	[PurchaseDate] [datetime] NULL,
	[PurchaseStatus] [nvarchar](3) NOT NULL,
	[SupplierCode] [nvarchar](10) NOT NULL,
	[EmployeeCode] [nvarchar](50) NOT NULL,
	[DepartmentCode] [nvarchar](3) NOT NULL,
	[LocationCode] [nvarchar](12) NULL,
	[PartsNumber] [nvarchar](25) NOT NULL,
	[Price] [decimal](10, 0) NOT NULL,
	[Quantity] [decimal](10, 2) NULL,
	[Amount] [decimal](10, 0) NOT NULL,
	[ReceiptNumber] [nvarchar](50) NULL,
	[Memo] [nvarchar](100) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[ServiceSlipNumber] [nvarchar](50) NULL,
	[RevisionNumber] [int] NOT NULL DEFAULT ((1)),
	[InvoiceNo] [nvarchar](50) NULL,
	[MakerOrderNumber] [nvarchar](50) NULL,
	[ChangePartsFlag] [nvarchar](2) NULL,
	[OrderPartsNumber] [nvarchar](25) NULL DEFAULT (''),
	[WebOrderNumber] [nvarchar](50) NULL DEFAULT (''),
	[LinkEntryCaptureDate] [datetime] NULL,
 CONSTRAINT [PK_PartsPurchase] PRIMARY KEY CLUSTERED 
(
	[PurchaseNumber] ASC,
	[RevisionNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入荷伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発注番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseOrderNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入荷予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PurchasePlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入荷日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入ステータス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入先' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'SupplierCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'担当者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ロケーションコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'LocationCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PartsNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入単価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'Price'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'Quantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'納品書番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'ReceiptNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'サービス伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'ServiceSlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Webオーダー番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'WebOrderNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'部品仕入' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase'
GO


