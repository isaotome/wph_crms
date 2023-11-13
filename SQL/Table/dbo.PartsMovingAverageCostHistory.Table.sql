USE [WPH_DB]
GO

/****** Object:  Table [dbo].[PartsMovingAverageCostHistory]    Script Date: 2019/06/04 15:25:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PartsMovingAverageCostHistory](
	[HistoryId] [uniqueidentifier] NOT NULL,
	[CompanyCode] [nvarchar](3) NOT NULL,
	[PartsNumber] [nvarchar](25) NOT NULL,
	[PreCost] [decimal](10, 0) NULL,
	[StockQuantity] [decimal](10, 2) NULL,
	[ChangeCost] [decimal](10, 0) NULL,
	[ChangeQuantity] [decimal](10, 2) NULL,
	[PostCost] [decimal](10, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_PartsMovingAverageCostHistory] PRIMARY KEY CLUSTERED 
(
	[HistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'履歴ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsMovingAverageCostHistory', @level2type=N'COLUMN',@level2name=N'HistoryId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'会社コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsMovingAverageCostHistory', @level2type=N'COLUMN',@level2name=N'CompanyCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsMovingAverageCostHistory', @level2type=N'COLUMN',@level2name=N'PartsNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'計算前単価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsMovingAverageCostHistory', @level2type=N'COLUMN',@level2name=N'PreCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'在庫数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsMovingAverageCostHistory', @level2type=N'COLUMN',@level2name=N'StockQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'増減単価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsMovingAverageCostHistory', @level2type=N'COLUMN',@level2name=N'ChangeCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'増減数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsMovingAverageCostHistory', @level2type=N'COLUMN',@level2name=N'ChangeQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'計算後単価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsMovingAverageCostHistory', @level2type=N'COLUMN',@level2name=N'PostCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsMovingAverageCostHistory', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsMovingAverageCostHistory', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'部品移動平均単価履歴' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsMovingAverageCostHistory'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsMovingAverageCostHistory'
GO


