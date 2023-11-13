USE [WPH_DB]
GO
/****** Object:  Table [dbo].[SecurityLevel]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SecurityLevel](
	[SecurityLevelCode] [int] NOT NULL,
	[SecurityLevelName] [nvarchar](50) NULL,
	[DelFlag] [int] NULL,
 CONSTRAINT [PK_SecurityLevel] PRIMARY KEY CLUSTERED 
(
	[SecurityLevelCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'セキュリティレベルコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SecurityLevel', @level2type=N'COLUMN',@level2name=N'SecurityLevelCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'セキュリティレベル名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SecurityLevel', @level2type=N'COLUMN',@level2name=N'SecurityLevelName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SecurityLevel', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'セキュリティレベル' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SecurityLevel'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SecurityLevel'
GO
