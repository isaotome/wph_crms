USE [WPH_DB]
GO
/****** Object:  Table [dbo].[CarSalesPayment]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarSalesPayment](
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
 CONSTRAINT [PK_CarSalesPayment] PRIMARY KEY CLUSTERED 
(
	[SlipNumber] ASC,
	[RevisionNumber] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両伝票改訂番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment', @level2type=N'COLUMN',@level2name=N'RevisionNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'行番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment', @level2type=N'COLUMN',@level2name=N'LineNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入金予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment', @level2type=N'COLUMN',@level2name=N'PaymentPlanDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入金予定金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment', @level2type=N'COLUMN',@level2name=N'Amount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment', @level2type=N'COLUMN',@level2name=N'Memo'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'車両伝票支払' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarSalesPayment'
GO
