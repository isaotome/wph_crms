USE [WPH_DB]
GO
/****** Object:  Table [dbo].[SerialNumber]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SerialNumber](
	[SerialCode] [nvarchar](50) NOT NULL,
	[SerialName] [nvarchar](50) NOT NULL,
	[PrefixCode] [nvarchar](50) NULL,
	[SuffixCode] [nvarchar](50) NULL,
	[SequenceNumber] [decimal](10, 0) NOT NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_SerialNumber] PRIMARY KEY CLUSTERED 
(
	[SerialCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'採番管理コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SerialNumber', @level2type=N'COLUMN',@level2name=N'SerialCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'採番管理名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SerialNumber', @level2type=N'COLUMN',@level2name=N'SerialName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'接頭文字列' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SerialNumber', @level2type=N'COLUMN',@level2name=N'PrefixCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'接尾文字列' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SerialNumber', @level2type=N'COLUMN',@level2name=N'SuffixCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'連番' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SerialNumber', @level2type=N'COLUMN',@level2name=N'SequenceNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SerialNumber', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SerialNumber', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SerialNumber', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SerialNumber', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SerialNumber', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'採番管理' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SerialNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SerialNumber'
GO
