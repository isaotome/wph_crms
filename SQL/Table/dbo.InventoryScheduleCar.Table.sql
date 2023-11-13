USE [WPH_DB]
GO
/****** Object:  Table [dbo].[Account]    Script Date: 08/04/2014 09:03:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
USE [WPH_DB]
GO

/****** Object:  Table [dbo].[InventoryMonthControlCar]    Script Date: 2017/06/28 17:52:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[InventoryScheduleCar](
	[DepartmentCode] [nvarchar](3) NOT NULL,
	[WarehouseCode] [nvarchar](6) NOT NULL,
	[InventoryMonth] [datetime] NOT NULL,
	[InventoryStatus] [nvarchar](3) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [varchar](2) NULL,
 CONSTRAINT [PK_InventoryScheduleCar] PRIMARY KEY CLUSTERED 
(
	[WarehouseCode] ASC,
	[InventoryMonth] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO