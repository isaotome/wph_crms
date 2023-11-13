 USE [WPH_DB]
GO

/****** Object:  Table [dbo].[Parts]    Script Date: 2015/03/27 20:12:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Parts](
	[PartsNumber] [nvarchar](25) NOT NULL,
	[PartsNameJp] [nvarchar](50) NOT NULL,
	[PartsNameEn] [nvarchar](50) NULL,
	[MakerCode] [nvarchar](5) NOT NULL,
	[MakerPartsNumber] [nvarchar](25) NULL,
	[MakerPartsNameJp] [nvarchar](50) NULL,
	[MakerPartsNameEn] [nvarchar](50) NULL,
	[Price] [decimal](10, 0) NULL,
	[SalesPrice] [decimal](10, 0) NULL,
	[SoPrice] [decimal](10, 0) NULL,
	[Cost] [decimal](10, 0) NULL,
	[ClaimPrice] [decimal](10, 0) NULL,
	[MpPrice] [decimal](10, 0) NULL,
	[EoPrice] [decimal](10, 0) NULL,
	[GenuineType] [nvarchar](3) NULL,
	[Memo] [nvarchar](50) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[UnitCD1] [nvarchar](3) NULL,
	[QuantityPerUnit1] [decimal](10, 0) NULL,
	[NonInventoryFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_Parts] PRIMARY KEY CLUSTERED 
(
	[PartsNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Parts] ADD  DEFAULT ('0') FOR [NonInventoryFlag]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'PartsNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品名称（J）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'PartsNameJp'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品名称（E）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'PartsNameEn'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカーコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'MakerCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカー部品番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'MakerPartsNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカー部品名称（J）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'MakerPartsNameJp'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカー部品名称（E）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'MakerPartsNameEn'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'定価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'Price'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'販売価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'SalesPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'S/O価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'SoPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'原価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'Cost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'クレーム申請部品代' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'ClaimPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'MP価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'MpPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'E/O価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'EoPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'純正区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'GenuineType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考（旧部品番号など）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'部品' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Parts'
GO


