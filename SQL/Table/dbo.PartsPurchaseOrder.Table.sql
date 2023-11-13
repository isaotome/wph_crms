USE [WPH_DB]
GO

/****** Object:  Table [dbo].[PartsPurchaseOrder]    Script Date: 2015/12/24 16:42:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PartsPurchaseOrder](
	[PurchaseOrderNumber] [nvarchar](50) NOT NULL,
	[ServiceSlipNumber] [nvarchar](50) NULL,
	[SupplierCode] [nvarchar](10) NULL,
	[SupplierPaymentCode] [nvarchar](10) NULL,
	[EmployeeCode] [nvarchar](50) NULL,
	[DepartmentCode] [nvarchar](3) NULL,
	[WebOrderNumber] [nvarchar](50) NULL,
	[PurchaseOrderDate] [datetime] NULL,
	[PurchaseOrderStatus] [nvarchar](3) NULL,
	[PartsNumber] [nvarchar](25) NOT NULL,
	[OrderType] [nvarchar](3) NOT NULL,
	[Quantity] [decimal](10, 2) NULL,
	[Cost] [decimal](10, 0) NULL,
	[Price] [decimal](10, 0) NULL,
	[Amount] [decimal](10, 0) NULL,
	[ArrivalPlanDate] [datetime] NULL,
	[PaymentPlanDate] [datetime] NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[Memo] [nvarchar](100) NULL,
	[RemainingQuantity] [decimal](10, 2) NULL,
 CONSTRAINT [PK_PartsPurchaseOrder] PRIMARY KEY CLUSTERED 
(
	[PurchaseOrderNumber] ASC,
	[PartsNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発注番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'PurchaseOrderNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'サービス伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'ServiceSlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'SupplierCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'SupplierPaymentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'担当者コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'WEBオーダー番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'WebOrderNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発注日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'PurchaseOrderDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発注ステータス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'PurchaseOrderStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'PartsNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発注区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'OrderType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'Quantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'原価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'Cost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'定価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'Price'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入荷予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'ArrivalPlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'PaymentPlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'部品発注' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchaseOrder'
GO


