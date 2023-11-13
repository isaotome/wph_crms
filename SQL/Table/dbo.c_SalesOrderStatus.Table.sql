USE [WPH_DB]
GO
/****** Object:  Table [dbo].[c_SalesOrderStatus]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[c_SalesOrderStatus](
	[Code] [varchar](3) NOT NULL,
	[Name] [varchar](50) NULL,
	[ShortName] [varchar](50) NULL,
	[DisplayOrder] [int] NULL,
	[DelFlag] [varchar](2) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
