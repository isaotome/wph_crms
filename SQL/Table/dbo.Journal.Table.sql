USE [WPH_DB]
GO

/****** Object:  Table [dbo].[Journal]    Script Date: 2017/10/05 11:43:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Journal](
	[JournalId] [uniqueidentifier] NOT NULL,
	[JournalType] [nvarchar](3) NOT NULL,
	[DepartmentCode] [nvarchar](3) NOT NULL,
	[CustomerClaimCode] [nvarchar](10) NULL,
	[SlipNumber] [nvarchar](50) NULL,
	[JournalDate] [datetime] NOT NULL,
	[AccountType] [nvarchar](3) NOT NULL,
	[AccountCode] [nvarchar](50) NOT NULL,
	[Amount] [decimal](10, 0) NOT NULL,
	[Summary] [nvarchar](50) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[ReceiptPlanFlag] [nvarchar](2) NULL,
	[TransferFlag] [nvarchar](2) NULL,
	[OfficeCode] [nvarchar](3) NULL,
	[CashAccountCode] [nvarchar](3) NULL,
	[PaymentKindCode] [nvarchar](3) NULL,
	[CreditReceiptPlanId] [nvarchar](36) NULL,
	[TradeVin] [nvarchar](20) NULL DEFAULT (''),
 CONSTRAINT [PK_Journal] PRIMARY KEY CLUSTERED 
(
	[JournalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'出納帳のユニークなID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'JournalId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入出金区分(001:入金、002:出金）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'JournalType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票番号（改訂番号は含まない）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票日付' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'JournalDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'口座種別（001：現金　002：振込）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'AccountType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'科目コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'AccountCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'摘要' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'Summary'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入金消込フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'ReceiptPlanFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'振替フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'TransferFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'事業所コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'OfficeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'現金口座コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'CashAccountCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払種別コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'PaymentKindCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'下取車車台番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'TradeVin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'カードで入金を行った入金予定のID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'CreditReceiptPlanId'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'入出金' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal'
GO



