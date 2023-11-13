USE [WPH_DB]
GO

/****** Object:  Table [dbo].[Customer]    Script Date: 2018/05/11 15:42:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Customer](
	[CustomerCode] [nvarchar](10) NOT NULL,
	[CustomerRank] [nvarchar](3) NULL,
	[CustomerKind] [nvarchar](3) NULL,
	[CustomerName] [nvarchar](80) NULL,
	[CustomerNameKana] [nvarchar](80) NULL,
	[CustomerType] [nvarchar](3) NULL,
	[PaymentKind] [nvarchar](3) NULL,
	[Sex] [nvarchar](3) NULL,
	[Birthday] [datetime] NULL,
	[Occupation] [nvarchar](3) NULL,
	[CarOwner] [nvarchar](3) NULL,
	[PostCode] [nvarchar](8) NULL,
	[Prefecture] [nvarchar](50) NULL,
	[City] [nvarchar](50) NULL,
	[Address1] [nvarchar](100) NULL,
	[Address2] [nvarchar](100) NULL,
	[TelNumber] [nvarchar](15) NULL,
	[FaxNumber] [nvarchar](15) NULL,
	[MailAddress] [nvarchar](100) NULL,
	[MobileNumber] [nvarchar](15) NULL,
	[MobileMailAddress] [nvarchar](100) NULL,
	[CustomerClaimCode] [nvarchar](10) NULL,
	[DmFlag] [nvarchar](3) NULL,
	[DmMemo] [nvarchar](100) NULL,
	[WorkingCompanyName] [nvarchar](40) NULL,
	[WorkingCompanyAddress] [nvarchar](100) NULL,
	[WorkingCompanyTelNumber] [nvarchar](15) NULL,
	[PositionName] [nvarchar](20) NULL,
	[CustomerEmployeeName] [nvarchar](40) NULL,
	[AccountEmployeeName] [nvarchar](40) NULL,
	[DepartmentCode] [nvarchar](3) NULL,
	[CarEmployeeCode] [nvarchar](50) NULL,
	[ServiceEmployeeCode] [nvarchar](50) NULL,
	[FirstReceiptionDate] [datetime] NULL,
	[LastReceiptionDate] [datetime] NULL,
	[Memo] [nvarchar](200) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[FirstName] [nvarchar](40) NULL,
	[LastName] [nvarchar](40) NULL,
	[FirstNameKana] [nvarchar](40) NULL,
	[LastNameKana] [nvarchar](40) NULL,
	[CorporationType] [nvarchar](3) NULL,
	[ServiceDepartmentCode] [nvarchar](3) NULL,
	[AddressReconfirm] [bit] NOT NULL DEFAULT ((0)),
	[DisplayOrder] [int] NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[CustomerCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CustomerCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客ランク' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CustomerRank'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CustomerKind'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CustomerName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客名称カナ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CustomerNameKana'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CustomerType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払方法' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'PaymentKind'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'性別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'Sex'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'生年月日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'Birthday'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'職業' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'Occupation'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車の所有' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CarOwner'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'郵便番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'PostCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'都道府県' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'Prefecture'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'市区町村' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'City'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'住所１' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'Address1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'住所２' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'Address2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'電話番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'TelNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FAX番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'FaxNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メールアドレス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'MailAddress'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'携帯電話番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'MobileNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'携帯メールアドレス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'MobileMailAddress'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'DM可否フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'DmFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'DM発送備考欄' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'DmMemo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'勤務先名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'WorkingCompanyName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'勤務先住所' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'WorkingCompanyAddress'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'勤務先電話番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'WorkingCompanyTelNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'役職名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'PositionName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'取引先担当者名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CustomerEmployeeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'経理担当者名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'AccountEmployeeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'営業担当者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CarEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'サービス担当者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'ServiceEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'初回来店日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'FirstReceiptionDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'前回来店日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'LastReceiptionDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'姓' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'FirstName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'LastName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'姓（カナ）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'FirstNameKana'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'名（カナ）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'LastNameKana'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'前株・後株' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'CorporationType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'サービス担当部門' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'ServiceDepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'住所再確認' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'AddressReconfirm'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'表示順序' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer', @level2type=N'COLUMN',@level2name=N'DisplayOrder'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'顧客' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customer'
GO


