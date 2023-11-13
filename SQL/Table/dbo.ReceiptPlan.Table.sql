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

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����\��̃��j�[�N��ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'ReceiptPlanId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��������R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'OccurredDepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'������R�[�h�i�ڋq�E�N���W�b�g��Њ܂ށj' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�`�[�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'������ʁi1:�����@2:�U���j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'ReceiptType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����\���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'ReceiptPlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ȖڃR�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'AccountCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����\����z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���|���c��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'ReceivableBalance'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���������t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'CompleteFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�폜�t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�E�v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'Summary'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ϓ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'JournalDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�a������t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'DepositFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�x����ʃR�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'PaymentKindCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�萔����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'CommissionRate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�萔��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan', @level2type=N'COLUMN',@level2name=N'CommissionAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'�����\��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'�g�����U�N�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceiptPlan'
GO


