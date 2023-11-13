USE [WPH_DB]
GO
/****** Object:  Table [dbo].[SupplierPayment]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SupplierPayment](
	[SupplierPaymentCode] [nvarchar](10) NOT NULL,
	[SupplierPaymentName] [nvarchar](80) NOT NULL,
	[SupplierPaymentType] [nvarchar](3) NOT NULL,
	[PaymentType] [nvarchar](3) NOT NULL,
	[PaymentDay] [int] NULL,
	[PaymentDayCount] [int] NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[PaymentPeriod1] [int] NULL,
	[PaymentPeriod2] [int] NULL,
	[PaymentPeriod3] [int] NULL,
	[PaymentPeriod4] [int] NULL,
	[PaymentPeriod5] [int] NULL,
	[PaymentPeriod6] [int] NULL,
	[PaymentRate1] [decimal](6, 3) NULL,
	[PaymentRate2] [decimal](6, 3) NULL,
	[PaymentRate3] [decimal](6, 3) NULL,
	[PaymentRate4] [decimal](6, 3) NULL,
	[PaymentRate5] [decimal](6, 3) NULL,
	[PaymentRate6] [decimal](6, 3) NULL,
	[BankCode] [nvarchar](4) NULL,
	[BranchCode] [nvarchar](3) NULL,
	[DepositKind] [nvarchar](3) NULL,
	[AccountNumber] [nvarchar](7) NULL,
	[AccountHolder] [nvarchar](30) NULL,
	[AccountHolderKana] [nvarchar](30) NULL,
 CONSTRAINT [PK_SupplierPayment] PRIMARY KEY CLUSTERED 
(
	[SupplierPaymentCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'SupplierPaymentCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払先名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'SupplierPaymentName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'１：メーカー　２：ファイナンス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'SupplierPaymentType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払区分（0：当月、1：翌月、2：日数指定）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentDay'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'日数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentDayCount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'猶予期間1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentPeriod1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'猶予期間2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentPeriod2'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'猶予期間3' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentPeriod3'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'猶予期間4' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentPeriod4'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'猶予期間5' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentPeriod5'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'猶予期間6' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentPeriod6'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金利1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentRate1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金利2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentRate2'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金利3' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentRate3'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金利4' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentRate4'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金利5' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentRate5'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金利6' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'PaymentRate6'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'銀行コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'BankCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支店コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'BranchCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'預金種目' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'DepositKind'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'口座番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'AccountNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'口座名義人' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'AccountHolder'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'口座名義人（カナ）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment', @level2type=N'COLUMN',@level2name=N'AccountHolderKana'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'支払先' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SupplierPayment'
GO
