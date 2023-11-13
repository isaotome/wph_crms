USE [WPH_DB]
GO
/****** Object:  Table [dbo].[Transfer]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Transfer](
	[TransferNumber] [nvarchar](50) NOT NULL,
	[TransferType] [nvarchar](3) NOT NULL,
	[SlipNumber] [nvarchar](50) NULL,
	[DepartureLocationCode] [nvarchar](12) NULL,
	[DepartureDate] [datetime] NOT NULL,
	[DepartureEmployeeCode] [nvarchar](50) NOT NULL,
	[ArrivalLocationCode] [nvarchar](12) NULL,
	[ArrivalPlanDate] [datetime] NOT NULL,
	[ArrivalDate] [datetime] NULL,
	[ArrivalEmployeeCode] [nvarchar](50) NULL,
	[PartsNumber] [nvarchar](25) NULL,
	[SalesCarNumber] [nvarchar](50) NULL,
	[Quantity] [decimal](10, 2) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_Transfer_1] PRIMARY KEY CLUSTERED 
(
	[TransferNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'移動伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'TransferNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'移動種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'TransferType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'出発ロケーションコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'DepartureLocationCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'出発日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'DepartureDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'出発担当者コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'DepartureEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'到着ロケーションコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'ArrivalLocationCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'到着予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'ArrivalPlanDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'到着日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'ArrivalDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'到着担当者コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'ArrivalEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'PartsNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'管理番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'SalesCarNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'Quantity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'ロケーション移動' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Transfer'
GO
