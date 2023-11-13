USE [WPH_DB]
GO
/****** Object:  Table [dbo].[PartsAverageCostControl]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PartsAverageCostControl](
	[CloseMonth] [datetime] NOT NULL,
	[ExecuteFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_PartsAverageCostControl] PRIMARY KEY CLUSTERED 
(
	[CloseMonth] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'締め処理月' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsAverageCostControl', @level2type=N'COLUMN',@level2name=N'CloseMonth'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'処理フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsAverageCostControl', @level2type=N'COLUMN',@level2name=N'ExecuteFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'部品平均単価コントロール' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsAverageCostControl'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsAverageCostControl'
GO
