USE [WPH_DB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[InventoryParts_Shikakari](
	[InventoryMonth] [datetime] NOT NULL,
	[DepartmentCode] [nvarchar](50) NOT NULL,
	[ArrivalPlanDate] [datetime] NULL,
	[SlipNumber] [nvarchar](50) NOT NULL,
	[LineNumber] [int] NOT NULL,
	[ServiceOrderStatus] [nvarchar](3) NULL,
	[ServiceOrderStatusName] [varchar](50) NULL,
	[ServiceWorkCode] [nvarchar](5) NULL,
	[ServiceWorksName] [nvarchar](20) NULL,
	[FrontEmployeeName] [nvarchar](40) NULL,
	[MekaEmployeeName] [nvarchar](40) NULL,
	[CustomerCode] [nvarchar](10) NULL,
	[CustomerName] [nvarchar](80) NULL,
	[CarName] [nvarchar](50) NULL,
	[Vin] [nvarchar](20) NULL,
	[ServiceType] [nvarchar](3) NULL,
	[ServiceTypeName] [varchar](4) NULL,
	[StockTypeName] [varchar](50) NULL,
	[PurchaseOrderDate] [datetime] NULL,
	[PartsArravalPlanDate] [datetime] NULL,
	[PurchaseDate] [datetime] NULL,
	[PartsNumber] [nvarchar](25) NULL,
	[LineContents1] [nvarchar](50) NULL,
	[Price] [decimal](10, 0) NULL,
	[Quantity] [decimal](10, 2) NULL,
	[Amount] [decimal](21, 2) NULL,
	[SupplierName] [nvarchar](80) NULL,
	[LineContents2] [nvarchar](50) NULL,
	[Cost] [decimal](10, 0) NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


