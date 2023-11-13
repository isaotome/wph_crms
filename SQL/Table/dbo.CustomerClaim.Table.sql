USE [WPH_DB]
GO

/****** Object:  Table [dbo].[CustomerClaim]    Script Date: 2018/05/11 15:44:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CustomerClaim](
	[CustomerClaimCode] [nvarchar](10) NOT NULL,
	[CustomerClaimName] [nvarchar](80) NULL,
	[CustomerClaimType] [nvarchar](10) NOT NULL,
	[PaymentKindType] [nvarchar](8) NULL,
	[RoundType] [nvarchar](3) NULL,
	[PostCode] [nvarchar](8) NULL,
	[Prefecture] [nvarchar](50) NULL,
	[City] [nvarchar](50) NULL,
	[Address1] [nvarchar](100) NULL,
	[Address2] [nvarchar](100) NULL,
	[TelNumber1] [nvarchar](15) NULL,
	[TelNumber2] [nvarchar](15) NULL,
	[FaxNumber] [nvarchar](15) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[DisplayOrder] [int] NULL,
 CONSTRAINT [PK_CustomerClaim] PRIMARY KEY CLUSTERED 
(
	[CustomerClaimCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'CustomerClaimName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1：個人　2：法人　3：クレジット　4：ローン' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'CustomerClaimType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0：締めなし　1：締めあり' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'PaymentKindType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'小数点以下の端数処理（0:切り捨て　1:四捨五入）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'RoundType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'郵便番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'PostCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'都道府県' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'Prefecture'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'市区町村' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'City'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'住所１' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'Address1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'住所２' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'Address2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'電話番号１' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'TelNumber1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'電話番号２' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'TelNumber2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FAX番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'FaxNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'表示順序' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim', @level2type=N'COLUMN',@level2name=N'DisplayOrder'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'請求' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerClaim'
GO


