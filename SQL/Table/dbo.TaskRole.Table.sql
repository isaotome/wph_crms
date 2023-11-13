USE [WPH_DB]
GO
/****** Object:  Table [dbo].[TaskRole]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TaskRole](
	[TaskConfigId] [varchar](50) NOT NULL,
	[SecurityRoleCode] [nvarchar](50) NOT NULL,
	[EnableFlag] [bit] NOT NULL,
 CONSTRAINT [PK_TaskRole] PRIMARY KEY CLUSTERED 
(
	[TaskConfigId] ASC,
	[SecurityRoleCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'タスク設定ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskRole', @level2type=N'COLUMN',@level2name=N'TaskConfigId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'セキュリティロールコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskRole', @level2type=N'COLUMN',@level2name=N'SecurityRoleCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'有効/無効' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskRole', @level2type=N'COLUMN',@level2name=N'EnableFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'タスクロール' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskRole'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TaskRole'
GO
