USE [WPH_DB]
GO

/****** Object:  Table [dbo].[ServiceWork]    Script Date: 2016/06/07 9:56:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ServiceWork](
	[ServiceWorkCode] [nvarchar](5) NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
	[Price] [decimal](10, 0) NULL,
	[Classification1] [nvarchar](3) NULL,
	[Classification2] [nvarchar](3) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[DisablePriceFlag] [nvarchar](2) NULL,
	[AccountClassCode] [nvarchar](10) NULL,
	[CustomerClaimClass] [nvarchar](3) NULL,
 CONSTRAINT [PK_ServiceWork] PRIMARY KEY CLUSTERED 
(
	[ServiceWorkCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'主作業コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork', @level2type=N'COLUMN',@level2name=N'ServiceWorkCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'主作業の名称（小分類）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork', @level2type=N'COLUMN',@level2name=N'Name'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'サービス料金' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork', @level2type=N'COLUMN',@level2name=N'Price'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'大分類名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork', @level2type=N'COLUMN',@level2name=N'Classification1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'中分類名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork', @level2type=N'COLUMN',@level2name=N'Classification2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金額表示の有無' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork', @level2type=N'COLUMN',@level2name=N'DisablePriceFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先分類' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork', @level2type=N'COLUMN',@level2name=N'CustomerClaimClass'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'主作業' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceWork'
GO


