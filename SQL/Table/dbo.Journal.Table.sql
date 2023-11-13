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

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�[���̃��j�[�N��ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'JournalId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���o���敪(001:�����A002:�o���j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'JournalType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'������R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�`�[�ԍ��i�����ԍ��͊܂܂Ȃ��j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�`�[���t' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'JournalDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'������ʁi001�F�����@002�F�U���j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'AccountType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ȖڃR�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'AccountCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�E�v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'Summary'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�폜�t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���������t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'ReceiptPlanFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�U�փt���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'TransferFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���Ə��R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'OfficeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���������R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'CashAccountCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�x����ʃR�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'PaymentKindCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����Ԏԑ�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'TradeVin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�J�[�h�œ������s���������\���ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal', @level2type=N'COLUMN',@level2name=N'CreditReceiptPlanId'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'���o��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'�g�����U�N�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Journal'
GO



