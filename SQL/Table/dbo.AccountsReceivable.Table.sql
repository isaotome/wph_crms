USE [WPH_DB]
GO

/****** Object:  Table [dbo].[AccountsReceivable]    Script Date: 2015/11/12 16:06:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AccountsReceivable](
	[CloseMonth] [datetime] NOT NULL,
	[SlipNumber] [nvarchar](50) NOT NULL,
	[CustomerClaimCode] [nvarchar](10) NOT NULL,
	[CustomerClaimName] [nvarchar](80) NOT NULL,
	[CustomerClaimType] [nvarchar](3) NOT NULL,
	[CustomerClaimTypeName] [nvarchar](50) NOT NULL,
	[DepartmentCode] [nvarchar](3) NULL,
	[DepartmentName] [nvarchar](20) NULL,
	[CustomerCode] [nvarchar](10) NOT NULL,
	[CustomerName] [nvarchar](80) NOT NULL,
	[SlipType] [nvarchar](1) NULL,
	[SlipTypeName] [nvarchar](50) NULL,
	[SalesDate] [datetime] NULL,
	[CarriedBalance] [decimal](10, 0) NULL,
	[PresentMonth] [decimal](10, 0) NULL,
	[Expendes] [decimal](10, 0) NULL,
	[TotalAmount] [decimal](10, 0) NULL,
	[Payment] [decimal](10, 0) NULL,
	[BalanceAmount] [decimal](10, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[ChargesPayment] [decimal](10, 0) NULL,
 CONSTRAINT [PK_AccountsReceivable] PRIMARY KEY CLUSTERED 
(
	[CloseMonth] ASC,
	[SlipNumber] ASC,
	[CustomerClaimCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CloseMonth'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�`�[�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'������R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����於' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CustomerClaimName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'������敪�R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CustomerClaimType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'������敪��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CustomerClaimTypeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���喼' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'DepartmentName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ڋq�R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CustomerCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ڋq��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CustomerName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�`�[���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'SlipType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�`�[��ʖ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'SlipTypeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�[�ԓ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'SalesDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O���J�z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CarriedBalance'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'PresentMonth'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����p' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'Expendes'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���v' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'TotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'Payment'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�c��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'BalanceAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�폜�t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO


