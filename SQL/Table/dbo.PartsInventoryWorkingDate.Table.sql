USE [WPH_DB]
GO

/****** Object:  Table [dbo].[PartsInventoryWorkingDate]    Script Date: 2015/04/30 9:15:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PartsInventoryWorkingDate](
	[InventoryWorkingDate] [datetime] NOT NULL,
	[InventoryMonth] [datetime] NULL,
 CONSTRAINT [PK_InventoryWorkingDate] PRIMARY KEY CLUSTERED 
(
	[InventoryWorkingDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ïîïiíIâµçÏã∆ì˙' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsInventoryWorkingDate', @level2type=N'COLUMN',@level2name=N'InventoryWorkingDate'
GO