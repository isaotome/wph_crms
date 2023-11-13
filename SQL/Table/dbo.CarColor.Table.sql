USE [WPH_DB]
GO
/****** Object:  Table [dbo].[CarColor]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarColor](
	[CarColorCode] [nvarchar](8) NOT NULL,
	[CarColorName] [nvarchar](50) NOT NULL,
	[MakerCode] [nvarchar](5) NOT NULL,
	[ColorCategory] [nvarchar](50) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[MakerColorCode] [nvarchar](50) NOT NULL,
	[InteriorColorFlag] [nvarchar](1) NULL,
	[ExteriorColorFlag] [nvarchar](1) NULL,
 CONSTRAINT [PK_CarColor] PRIMARY KEY CLUSTERED 
(
	[CarColorCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両カラーコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor', @level2type=N'COLUMN',@level2name=N'CarColorCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両カラー名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor', @level2type=N'COLUMN',@level2name=N'CarColorName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカーコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor', @level2type=N'COLUMN',@level2name=N'MakerCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系統色' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor', @level2type=N'COLUMN',@level2name=N'ColorCategory'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカーカラーコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor', @level2type=N'COLUMN',@level2name=N'MakerColorCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'内装色フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor', @level2type=N'COLUMN',@level2name=N'InteriorColorFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'外装色フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor', @level2type=N'COLUMN',@level2name=N'ExteriorColorFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'車両カラー' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarColor'
GO
ALTER TABLE [dbo].[CarColor] ADD  DEFAULT ('') FOR [MakerColorCode]
GO
