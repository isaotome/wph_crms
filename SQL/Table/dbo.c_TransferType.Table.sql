USE [WPH_DB]
GO

--Mod 2016/06/20 arc yano #3583 ïîïià⁄ìÆì¸óÕÅ@à⁄ìÆéÌï ÇÃçiçûÇ›
/****** Object:  Table [dbo].[c_TransferType]    Script Date: 2016/06/20 14:42:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[c_TransferType](
	[Code] [varchar](3) NOT NULL,
	[Name] [varchar](50) NULL,
	[ShortName] [varchar](50) NULL,
	[DisplayOrder] [int] NULL,
	[DelFlag] [varchar](2) NULL,
	[EditNarrowFlag] [nvarchar](2) NOT NULL DEFAULT ('0'),
 CONSTRAINT [PK_c_TransferType] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


