USE [WPH_DB]
GO

/****** Object:  Table [dbo].[InventoryMonthControlPartsBalance]    Script Date: 2015/04/17 15:39:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[InventoryMonthControlPartsBalance](
	[InventoryMonth] [nvarchar](8) NOT NULL,
	[InventoryStatus] [nchar](3) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_InventoryMonthControlPartsBalance] PRIMARY KEY CLUSTERED 
(
	[InventoryMonth] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


