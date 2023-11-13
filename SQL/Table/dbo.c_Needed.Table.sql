USE [WPH_DB]
GO
/****** Object:  Table [dbo].[c_Needed]    Script Date: 2014/08/22 16:07:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[c_Needed](
	[Code] [varchar](3) NOT NULL,
	[Name] [varchar](50) NULL,
	[ShortName] [varchar](50) NULL,
	[DisplayOrder] [int] NULL,
	[DelFlag] [varchar](2) NULL,
 CONSTRAINT [PK_c_Needed] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO


