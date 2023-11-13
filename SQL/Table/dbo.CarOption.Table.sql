USE [WPH_DB]
GO

/****** Object:  Table [dbo].[CarOption]    Script Date: 2016/03/30 16:30:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CarOption](
	[CarOptionCode] [nvarchar](25) NOT NULL,
	[MakerCode] [nvarchar](5) NOT NULL,
	[CarOptionName] [nvarchar](100) NOT NULL,
	[Cost] [decimal](10, 0) NULL,
	[SalesPrice] [decimal](10, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[OptionType] [nvarchar](3) NOT NULL DEFAULT ('001'),
	[RequiredFlag] [nvarchar](2) NOT NULL DEFAULT ('0'),
	[CarGradeCode] [nvarchar](30) NOT NULL DEFAULT (''),
 CONSTRAINT [PK_CarOption] PRIMARY KEY CLUSTERED 
(
	[CarOptionCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'オプションコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarOption', @level2type=N'COLUMN',@level2name=N'CarOptionCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカーコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarOption', @level2type=N'COLUMN',@level2name=N'MakerCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'オプション名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarOption', @level2type=N'COLUMN',@level2name=N'CarOptionName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'原価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarOption', @level2type=N'COLUMN',@level2name=N'Cost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'販売価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarOption', @level2type=N'COLUMN',@level2name=N'SalesPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarOption', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarOption', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarOption', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarOption', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarOption', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'オプション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarOption'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarOption'
GO


