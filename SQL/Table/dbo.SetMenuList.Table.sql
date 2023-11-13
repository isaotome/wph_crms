USE [WPH_DB]
GO
/****** Object:  Table [dbo].[SetMenuList]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SetMenuList](
	[SetMenuCode] [nvarchar](11) NOT NULL,
	[DetailsNumber] [int] NOT NULL,
	[ServiceType] [nvarchar](3) NULL,
	[ServiceWorkCode] [nchar](11) NULL,
	[ServiceMenuCode] [nvarchar](8) NULL,
	[PartsNumber] [nvarchar](25) NULL,
	[Comment] [nvarchar](100) NULL,
	[WorkType] [nvarchar](3) NULL,
	[Quantity] [decimal](10, 2) NULL,
	[AutoSetAmount] [nvarchar](3) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_SetMenu] PRIMARY KEY CLUSTERED 
(
	[SetMenuCode] ASC,
	[DetailsNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'セットメニューコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'SetMenuCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'行番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'DetailsNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'明細種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'ServiceType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'主作業コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'ServiceWorkCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'サービスメニューコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'ServiceMenuCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'PartsNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'コメント' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'Comment'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作業区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'WorkType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'Quantity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金額自動設定フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'AutoSetAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'セットメニューリスト' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetMenuList'
GO
