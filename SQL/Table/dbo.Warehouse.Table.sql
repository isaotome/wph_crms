USE [WPH_DB]
GO

/****** Object:  Table [dbo].[Warehouse]    Script Date: 2016/10/03 15:10:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Warehouse](
	[WarehouseCode] [nvarchar](6) NOT NULL,
	[WarehouseName] [nvarchar](20) NULL,
	[DepartmentCode] [nvarchar](3) NOT NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_Warehouse] PRIMARY KEY CLUSTERED 
(
	[WarehouseCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'倉庫コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Warehouse', @level2type=N'COLUMN',@level2name=N'WarehouseCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'倉庫名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Warehouse', @level2type=N'COLUMN',@level2name=N'WarehouseName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'管理部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Warehouse', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Warehouse', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Warehouse', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Warehouse', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Warehouse', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Warehouse', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO


