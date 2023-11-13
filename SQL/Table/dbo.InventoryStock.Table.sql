USE [WPH_DB]
GO

/****** Object:  Table [dbo].[InventoryStock]    Script Date: 2016/10/03 15:15:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[InventoryStock](
	[InventoryId] [uniqueidentifier] NOT NULL,
	[DepartmentCode] [nvarchar](3) NULL,
	[InventoryMonth] [datetime] NOT NULL,
	[LocationCode] [nvarchar](12) NULL,
	[EmployeeCode] [nvarchar](50) NOT NULL,
	[InventoryType] [nvarchar](3) NOT NULL,
	[SalesCarNumber] [nvarchar](50) NULL,
	[PartsNumber] [nvarchar](25) NULL,
	[Quantity] [decimal](10, 3) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[Summary] [nvarchar](200) NULL,
	[PhysicalQuantity] [decimal](10, 3) NULL,
	[Comment] [varchar](255) NULL,
	[ProvisionQuantity] [decimal](10, 2) NULL,
	[WarehouseCode] [nvarchar](6) NULL,
 CONSTRAINT [PK_InventoryStock] PRIMARY KEY CLUSTERED 
(
	[InventoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'InventoryStock', @level2type=N'COLUMN',@level2name=N'Summary'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'引当済数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'InventoryStock', @level2type=N'COLUMN',@level2name=N'ProvisionQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'倉庫コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'InventoryStock', @level2type=N'COLUMN',@level2name=N'WarehouseCode'
GO


