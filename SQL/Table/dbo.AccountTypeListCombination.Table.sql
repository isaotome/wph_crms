USE [WPH_DB]
GO

/****** Object:  Table [dbo].[AccountTypeListCombination]    Script Date: 2016/06/10 15:14:08 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[AccountTypeListCombination](
	[ListType] [varchar](3) NOT NULL,
	[AccountTypeCode] [varchar](3) NOT NULL,
	[DelFlag] [varchar](2) NULL,
PRIMARY KEY CLUSTERED 
(
	[ListType] ASC,
	[AccountTypeCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


