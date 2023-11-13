USE [WPH_DB]
GO

/****** Object:  Table [dbo].[InventoryScheduleParts]    Script Date: 2016/10/03 15:18:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[InventoryScheduleParts](
	[DepartmentCode] [nchar](3) NOT NULL,
	[InventoryMonth] [datetime] NOT NULL,
	[InventoryType] [nchar](3) NOT NULL,
	[InventoryStatus] [nchar](3) NOT NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[CreateEmployeeCode] [nchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nchar](2) NULL,
	[WarehouseCode] [nvarchar](6) NOT NULL DEFAULT ('033-01'),
 CONSTRAINT [PK_InventoryScheduleParts] PRIMARY KEY CLUSTERED 
(
	[WarehouseCode] ASC,
	[InventoryMonth] ASC,
	[InventoryType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'倉庫コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'InventoryScheduleParts', @level2type=N'COLUMN',@level2name=N'WarehouseCode'
GO


