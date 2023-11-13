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

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'締月' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CloseMonth'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CustomerClaimName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先区分コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CustomerClaimType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先区分名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CustomerClaimTypeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'DepartmentName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CustomerCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CustomerName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'SlipType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票種別名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'SlipTypeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'納車日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'SalesDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'前月繰越' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CarriedBalance'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当月発生' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'PresentMonth'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'諸費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'Expendes'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'合計' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'TotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当月入金' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'Payment'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'残高' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'BalanceAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountsReceivable', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO


