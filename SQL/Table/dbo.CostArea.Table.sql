USE [Work_Yano]
GO

/****** Object:  Table [dbo].[CostArea]    Script Date: 2023/08/22 15:00:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CostArea](
	[CostAreaCode] [nvarchar](3) NOT NULL,
	[CostAreaName] [nvarchar](50) NULL,
	[RequestNumberCost] [decimal](10, 0) NULL,
	[ParkingSpaceFeeWithTax] [decimal](10, 0) NULL,
	[InspectionRegistFeeWithTax] [decimal](10, 0) NULL,
	[TradeInFeeWithTax] [decimal](10, 0) NULL,
	[PreparationFeeWithTax] [decimal](10, 0) NULL,
	[AppraisalFee] [decimal](10, 0) NULL,
	[RecycleControlFeeWithTax] [decimal](10, 0) NULL,
	[RequestNumberFeeWithTax] [decimal](10, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[ParkingSpaceCost] [decimal](10, 0) NULL,
	[NumberPlateCost] [decimal](10, 0) NULL,
	[FarRegistFeeWithTax] [decimal](10, 0) NULL,
	[TradeInMaintenanceFeeWithTax] [decimal](10, 0) NULL,
	[InheritedInsuranceFeeWithTax] [decimal](10, 0) NULL,
	[OutJurisdictionRegistFeeWithTax] [decimal](10, 0) NULL,
 CONSTRAINT [PK_CostArea] PRIMARY KEY CLUSTERED 
(
	[CostAreaCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'諸費用設定エリアコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'CostAreaCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'諸費用設定エリア名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'CostAreaName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'希望番号費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'RequestNumberCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車庫証明費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'ParkingSpaceFeeWithTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'検査・登録費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'InspectionRegistFeeWithTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'下取費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'TradeInFeeWithTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'納車準備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'PreparationFeeWithTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'査定費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'AppraisalFee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'リサイクル管理費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'RecycleControlFeeWithTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'希望番号代行費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'RequestNumberFeeWithTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車庫証明証紙代' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'ParkingSpaceCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ﾅﾝﾊﾞｰﾌﾟﾚｰﾄ代(一般)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'NumberPlateCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'県外登録手続代行費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'FarRegistFeeWithTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'中古車点検・整備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'TradeInMaintenanceFeeWithTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'中古車継承整備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'InheritedInsuranceFeeWithTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'管轄外登録手続費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea', @level2type=N'COLUMN',@level2name=N'OutJurisdictionRegistFeeWithTax'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'諸費用設定エリア' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CostArea'
GO


