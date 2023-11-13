USE [WPH_DB]
GO

/****** Object:  Table [dbo].[ServiceSalesLine]    Script Date: 2017/11/07 16:17:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ServiceSalesLine](
	[SlipNumber] [nvarchar](50) NOT NULL,
	[RevisionNumber] [int] NOT NULL,
	[LineNumber] [int] NOT NULL,
	[ServiceType] [nvarchar](3) NOT NULL,
	[SetMenuCode] [nvarchar](11) NULL,
	[ServiceWorkCode] [nvarchar](5) NULL,
	[ServiceMenuCode] [nvarchar](8) NULL,
	[PartsNumber] [nvarchar](25) NULL,
	[LineContents] [nvarchar](50) NULL,
	[RequestComment] [nvarchar](100) NULL,
	[WorkType] [nvarchar](3) NULL,
	[LaborRate] [int] NULL,
	[ManPower] [decimal](5, 2) NULL,
	[TechnicalFeeAmount] [decimal](10, 0) NULL,
	[Quantity] [decimal](10, 2) NULL,
	[Price] [decimal](10, 0) NULL,
	[Amount] [decimal](10, 0) NULL,
	[Cost] [decimal](10, 0) NULL,
	[EmployeeCode] [nvarchar](50) NULL,
	[SupplierCode] [nvarchar](10) NULL,
	[CustomerClaimCode] [nvarchar](10) NULL,
	[StockStatus] [nvarchar](3) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[Classification1] [nvarchar](3) NULL,
	[TaxAmount] [decimal](10, 0) NULL,
	[UnitCost] [decimal](10, 0) NULL,
	[LineType] [nvarchar](3) NULL,
	[ConsumptionTaxId] [nvarchar](3) NULL,
	[Rate] [smallint] NULL,
	[ProvisionQuantity] [decimal](10, 2) NULL,
	[OrderQuantity] [decimal](10, 2) NULL,
	[DisplayOrder] [decimal](4, 1) NULL,
	[OutputTargetFlag] [nvarchar](1) NULL DEFAULT ('0'),
	[OutputFlag] [nvarchar](1) NULL DEFAULT ('0'),
 CONSTRAINT [PK_ServiceSalesLine] PRIMARY KEY CLUSTERED 
(
	[SlipNumber] ASC,
	[RevisionNumber] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票改訂番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'RevisionNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'行番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'LineNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'明細種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'ServiceType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'セットメニューコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'SetMenuCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'主作業コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'ServiceWorkCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'サービスメニューコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'ServiceMenuCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'PartsNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'明細手入力値' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'LineContents'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'コメント' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'RequestComment'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作業区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'WorkType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'レバレート' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'LaborRate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'ManPower'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'技術料' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'TechnicalFeeAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'Quantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'単価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'Price'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'原価(合計)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'Cost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メカニック担当者コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入先（外注先）コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'SupplierCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'在庫状況（在庫、取寄）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'StockStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'大分類' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'Classification1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'消費税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'TaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'原価(単価)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'UnitCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'外注メカ請求' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'LineType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'引当済数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'ProvisionQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発注数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'OrderQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'表示順序' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'DisplayOrder'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発注対象フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'OutputTargetFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発注済フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'OutputFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'サービス伝票明細' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine'
GO


