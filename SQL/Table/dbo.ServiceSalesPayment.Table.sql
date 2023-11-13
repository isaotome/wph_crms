USE [WPH_DB]
GO

/****** Object:  Table [dbo].[ServiceSalesPayment]    Script Date: 2020/02/21 19:10:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ServiceSalesPayment](
	[SlipNumber] [nvarchar](50) NOT NULL,
	[RevisionNumber] [int] NOT NULL,
	[LineNumber] [int] NOT NULL,
	[CustomerClaimCode] [nvarchar](10) NULL,
	[PaymentPlanDate] [datetime] NULL,
	[Amount] [decimal](10, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[Memo] [nvarchar](100) NULL,
	[DepositFlag] [nvarchar](2) NULL,
	[AmountType] [nvarchar](50) NULL,
 CONSTRAINT [PK_ServiceSalesPayment] PRIMARY KEY CLUSTERED 
(
	[SlipNumber] ASC,
	[RevisionNumber] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'改訂番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'RevisionNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'行番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'LineNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入金予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'PaymentPlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'預かり金フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'DepositFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'費用項目' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment', @level2type=N'COLUMN',@level2name=N'AmountType'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'サービス伝票支払' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesPayment'
GO


