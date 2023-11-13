USE [WPH_DB]
GO

/****** Object:  Table [dbo].[PartsPurchaseHistory]    Script Date: 2015/12/24 16:43:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PartsPurchaseHistory](
	[PurchaseNumber] [nvarchar](50) NOT NULL,
	[PurchaseOrderNumber] [nvarchar](50) NULL,
	[PurchaseType] [nvarchar](3) NOT NULL,
	[PurchasePlanDate] [datetime] NULL,
	[PurchaseDate] [datetime] NULL,
	[PurchaseStatus] [nvarchar](3) NOT NULL,
	[SupplierCode] [nvarchar](10) NOT NULL,
	[EmployeeCode] [nvarchar](50) NOT NULL,
	[DepartmentCode] [nvarchar](3) NOT NULL,
	[LocationCode] [nvarchar](12) NULL,
	[PartsNumber] [nvarchar](25) NOT NULL,
	[Price] [decimal](10, 0) NOT NULL,
	[Quantity] [decimal](10, 2) NULL,
	[Amount] [decimal](10, 0) NOT NULL,
	[ReceiptNumber] [nvarchar](50) NULL,
	[Memo] [nvarchar](100) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[ServiceSlipNumber] [nvarchar](50) NULL,
	[RevisionNumber] [int] NOT NULL,
	[InvoiceNo] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[PurchaseNumber] ASC,
	[RevisionNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


