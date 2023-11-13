USE [WPH_DB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ModifiedReason](
	[SlipNumber] [nvarchar](50) NOT NULL,
	[RevisionNumber] [int] NOT NULL,
	[SlipType] [varchar](2) NOT NULL,
	[Reason] [nvarchar](1024) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_ModifiedReason] PRIMARY KEY CLUSTERED 
(
	[SlipNumber] ASC,
	[RevisionNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ì`ï[î‘çÜ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ModifiedReason', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO


