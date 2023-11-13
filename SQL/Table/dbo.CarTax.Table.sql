USE [WPH_DB]
GO

/****** Object:  Table [dbo].[CarTax]    Script Date: 2019/10/21 13:38:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CarTax](
	[CarTaxId] [uniqueidentifier] NOT NULL,
	[CarTaxName] [nvarchar](100) NOT NULL,
	[FromDisplacement] [decimal](10, 2) NOT NULL,
	[ToDisplacement] [decimal](10, 2) NOT NULL,
	[Amount] [decimal](10, 0) NOT NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[RegistMonth] [int] NULL,
	[FromAvailableDate] [date] NOT NULL DEFAULT (getdate()),
	[ToAvailableDate] [date] NOT NULL DEFAULT (getdate()),
 CONSTRAINT [PK_CarTax] PRIMARY KEY CLUSTERED 
(
	[CarTaxId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自動車税ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax', @level2type=N'COLUMN',@level2name=N'CarTaxId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自動車税表示名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax', @level2type=N'COLUMN',@level2name=N'CarTaxName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'総排気量（FROM）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax', @level2type=N'COLUMN',@level2name=N'FromDisplacement'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'総排気量（TO）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax', @level2type=N'COLUMN',@level2name=N'ToDisplacement'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録月' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax', @level2type=N'COLUMN',@level2name=N'RegistMonth'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'自動車税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarTax'
GO


