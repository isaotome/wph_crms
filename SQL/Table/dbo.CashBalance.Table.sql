USE [WPH_DB]
GO
/****** Object:  Table [dbo].[CashBalance]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CashBalance](
	[OfficeCode] [nvarchar](3) NOT NULL,
	[ClosedDate] [datetime] NOT NULL,
	[CloseFlag] [nvarchar](2) NOT NULL,
	[NumberOf10000] [int] NULL,
	[NumberOf5000] [int] NULL,
	[NumberOf2000] [int] NULL,
	[NumberOf1000] [int] NULL,
	[NumberOf500] [int] NULL,
	[NumberOf100] [int] NULL,
	[NumberOf50] [int] NULL,
	[NumberOf10] [int] NULL,
	[NumberOf5] [int] NULL,
	[NumberOf1] [int] NULL,
	[CheckAmount] [decimal](10, 0) NULL,
	[TotalAmount] [decimal](10, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[CashAccountCode] [nvarchar](3) NOT NULL,
 CONSTRAINT [PK_CashBalance] PRIMARY KEY CLUSTERED 
(
	[OfficeCode] ASC,
	[ClosedDate] ASC,
	[CashAccountCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'事業所コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'OfficeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'締め日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'ClosedDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'締めフラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'CloseFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'10000円札枚数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'NumberOf10000'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'5000円札枚数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'NumberOf5000'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'2000円札枚数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'NumberOf2000'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1000円札枚数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'NumberOf1000'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'500円玉枚数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'NumberOf500'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'100円玉枚数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'NumberOf100'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'50円玉枚数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'NumberOf50'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'10円玉枚数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'NumberOf10'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'5円玉枚数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'NumberOf5'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1円玉枚数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'NumberOf1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'小切手等金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'CheckAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'合計金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'TotalAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'現金口座コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance', @level2type=N'COLUMN',@level2name=N'CashAccountCode'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'現金在高' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CashBalance'
GO
