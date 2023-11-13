USE [WPH_DB]
GO

/****** Object:  Table [dbo].[PartsBalance]    Script Date: 2016/10/03 15:17:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PartsBalance](
	[CloseMonth] [datetime] NOT NULL,
	[DepartmentCode] [nvarchar](3) NOT NULL,
	[DepartmentName] [nvarchar](50) NULL,
	[PartsNumber] [nvarchar](25) NOT NULL,
	[PartsNameJp] [nvarchar](50) NULL,
	[PreCost] [decimal](10, 0) NULL,
	[PreQuantity] [decimal](10, 3) NULL,
	[PreAmount] [decimal](10, 0) NULL,
	[PurchaseQuantity] [decimal](10, 3) NULL,
	[PurchaseAmount] [decimal](10, 0) NULL,
	[TransferArrivalQuantity] [decimal](10, 3) NULL,
	[TransferArrivalAmount] [decimal](10, 0) NULL,
	[ShipQuantity] [decimal](10, 3) NULL,
	[ShipAmount] [decimal](10, 0) NULL,
	[TransferDepartureQuantity] [decimal](10, 3) NULL,
	[TransferDepartureAmount] [decimal](10, 0) NULL,
	[DifferenceQuantity] [decimal](10, 3) NULL,
	[DifferenceAmount] [decimal](10, 0) NULL,
	[UnitPriceDifference] [decimal](10, 0) NULL,
	[PostCost] [decimal](10, 0) NULL,
	[PostQuantity] [decimal](10, 3) NULL,
	[PostAmount] [decimal](10, 0) NULL,
	[InProcessQuantity] [decimal](10, 3) NULL,
	[InProcessAmount] [decimal](10, 0) NULL,
	[PurchaseOrderPrice] [decimal](10, 0) NULL,
	[CalculatedDate] [datetime] NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[CalculatedQuantity] [decimal](10, 2) NULL,
	[CalculatedAmount] [decimal](10, 0) NULL,
	[ReservationQuantity] [decimal](10, 2) NULL,
	[ReservationAmount] [decimal](10, 0) NULL,
	[WarehouseCode] [nvarchar](6) NOT NULL DEFAULT ('033-01'),
	[WarehouseName] [nvarchar](20) NULL,
 CONSTRAINT [PK_PartsBalance] PRIMARY KEY CLUSTERED 
(
	[CloseMonth] ASC,
	[WarehouseCode] ASC,
	[PartsNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'処理年月' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'CloseMonth'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'DepartmentName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'PartsNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'PartsNameJp'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'月初単価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'PreCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'月初在庫数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'PreQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'月初在庫金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'PreAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当月仕入数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'PurchaseQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当月仕入金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'PurchaseAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当月移動受入数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'TransferArrivalQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当月移動受入金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'TransferArrivalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当月納車数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'ShipQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当月納車金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'ShipAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当月移動払出数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'TransferDepartureQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当月移動払出金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'TransferDepartureAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'棚差数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'DifferenceQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'棚差金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'DifferenceAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'単価差額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'UnitPriceDifference'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'月末単価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'PostCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'月末在庫数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'PostQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'月末在庫金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'PostAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕掛在庫数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'InProcessQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕掛在庫金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'InProcessAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発注単価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'PurchaseOrderPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'算出日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'CalculatedDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'理論在庫数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'CalculatedQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'理論在庫金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'CalculatedAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'引当在庫数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'ReservationQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'引当在庫金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'ReservationAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'倉庫コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'WarehouseCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'倉庫名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsBalance', @level2type=N'COLUMN',@level2name=N'WarehouseName'
GO


