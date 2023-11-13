USE [WPH_DB]
GO

/****** Object:  Table [dbo].[EPDiscountTax]    Script Date: 2019/09/25 17:12:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EPDiscountTax](
	[EPDiscountTaxId] [nvarchar](3) NOT NULL,
	[FromAvailableDate] [date] NOT NULL,
	[ToAvailableDate] [date] NOT NULL,
	[Rate] [int] NULL,
	[RateName] [nvarchar](50) NOT NULL,
	[DisplayOrder] [int] NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_EPDiscountTaxId] PRIMARY KEY CLUSTERED 
(
	[EPDiscountTaxId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'環境性能割税率ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EPDiscountTax', @level2type=N'COLUMN',@level2name=N'EPDiscountTaxId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'摘要開始日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EPDiscountTax', @level2type=N'COLUMN',@level2name=N'FromAvailableDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'摘要期限日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EPDiscountTax', @level2type=N'COLUMN',@level2name=N'ToAvailableDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'税率' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EPDiscountTax', @level2type=N'COLUMN',@level2name=N'Rate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'税率名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EPDiscountTax', @level2type=N'COLUMN',@level2name=N'RateName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'表示順序' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EPDiscountTax', @level2type=N'COLUMN',@level2name=N'DisplayOrder'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EPDiscountTax', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EPDiscountTax', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EPDiscountTax', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO


