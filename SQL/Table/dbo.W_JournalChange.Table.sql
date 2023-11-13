USE [WPH_DB]
GO

/****** Object:  Table [dbo].[W_JournalChange]    Script Date: 2018/01/31 14:53:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[W_JournalChange](
	[JournalId] [uniqueidentifier] NOT NULL,
	[SlipNumber_old] [nvarchar](50) NULL,
	[SlipNumber_new] [nvarchar](50) NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[JournalChangeId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_W_JournalChange] PRIMARY KEY CLUSTERED 
(
	[JournalChangeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'出納帳のユニークなID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'W_JournalChange', @level2type=N'COLUMN',@level2name=N'JournalId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票番号（改訂番号は含まない）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'W_JournalChange', @level2type=N'COLUMN',@level2name=N'SlipNumber_old'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'W_JournalChange', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'W_JournalChange', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO


