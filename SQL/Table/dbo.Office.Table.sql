USE [WPH_DB]
GO
/****** Object:  Table [dbo].[Office]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Office](
	[OfficeCode] [nvarchar](3) NOT NULL,
	[OfficeName] [nvarchar](20) NOT NULL,
	[CompanyCode] [nvarchar](3) NOT NULL,
	[EmployeeCode] [nvarchar](50) NOT NULL,
	[PostCode] [nvarchar](8) NULL,
	[Prefecture] [nvarchar](50) NULL,
	[City] [nvarchar](50) NULL,
	[Address1] [nvarchar](100) NULL,
	[Address2] [nvarchar](100) NULL,
	[TelNumber1] [nvarchar](15) NULL,
	[TelNumber2] [nvarchar](15) NULL,
	[FaxNumber] [nvarchar](15) NULL,
	[BankName] [nvarchar](15) NULL,
	[BranchName] [nvarchar](15) NULL,
	[DepositKind] [nvarchar](3) NULL,
	[AccountNumber] [nvarchar](7) NULL,
	[AccountHolder] [nvarchar](30) NULL,
	[CostAreaCode] [nvarchar](3) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[PrintFlagCar] [nvarchar](2) NULL,
	[PrintFlagService] [nvarchar](2) NULL,
	[FullName] [nvarchar](50) NULL,
	[SupplierCode] [nvarchar](10) NULL,
 CONSTRAINT [PK_Office] PRIMARY KEY CLUSTERED 
(
	[OfficeCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'事業所コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'OfficeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'事業所名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'OfficeName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'会社コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'CompanyCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'事業部長（社員コード）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'郵便番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'PostCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'都道府県' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'Prefecture'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'市区町村' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'City'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'住所１' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'Address1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'住所２' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'Address2'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'電話番号１' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'TelNumber1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'電話番号２' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'TelNumber2'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FAX番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'FaxNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'銀行名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'BankName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支店名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'BranchName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'預金種目' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'DepositKind'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'口座番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'AccountNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'口座名義人' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'AccountHolder'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'諸費用エリアコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'CostAreaCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両伝票印刷フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'PrintFlagCar'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'サービス伝票印刷フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'PrintFlagService'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'正式名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'FullName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'デフォルト仕入先' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office', @level2type=N'COLUMN',@level2name=N'SupplierCode'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'事業所' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Office'
GO
