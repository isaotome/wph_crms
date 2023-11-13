USE [WPH_DB]
GO

/****** Object:  Table [dbo].[ReceiptPlan]    Script Date: 2016/05/17 13:37:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ReceiptPlan](
	[ReceiptPlanId] [uniqueidentifier] NOT NULL,
	[DepartmentCode] [nvarchar](3) NOT NULL,
	[OccurredDepartmentCode] [nvarchar](3) NOT NULL,
	[CustomerClaimCode] [nvarchar](10) NOT NULL,
	[SlipNumber] [nvarchar](50) NULL,
	[ReceiptType] [nvarchar](3) NULL,
	[ReceiptPlanDate] [datetime] NULL,
	[AccountCode] [nvarchar](50) NULL,
	[Amount] [decimal](10, 0) NULL,
	[ReceivableBalance] [decimal](10, 0) NULL,
	[CompleteFlag] [nvarchar](2) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[Summary] [nvarchar](50) NULL,
	[JournalDate] [datetime] NULL,
	[DepositFlag] [nvarchar](2) NULL,
	[PaymentKindCode] [nvarchar](10) NULL,
	[CommissionRate] [decimal](8, 5) NULL,
	[CommissionAmount] [decimal](10, 0) NULL,
	[CreditJournalId] [nvarchar](32) NULL,
 CONSTRAINT [PK_ReceiptPlan] PRIMARY KEY CLUSTERED 
(
	[ReceiptPlanId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入金予定のユニークなID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'ReceiptPlanId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発生部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'OccurredDepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先コード（顧客・クレジット会社含む）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入金種別（1:現金　2:振込）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'ReceiptType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入金予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'ReceiptPlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'科目コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'AccountCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入金予定金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'売掛金残高' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'ReceivableBalance'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入金完了フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'CompleteFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'摘要' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'Summary'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'決済日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'JournalDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'預かり金フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'DepositFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払種別コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'PaymentKindCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'手数料率' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'CommissionRate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'手数料' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'CommissionAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'入金予定' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan'
GO


