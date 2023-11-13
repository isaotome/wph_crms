USE [WPH_DB]
GO
/****** Object:  Table [dbo].[PaymentPlan]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentPlan](
	[PaymentPlanId] [uniqueidentifier] NOT NULL,
	[SupplierPaymentCode] [nvarchar](10) NOT NULL,
	[DepartmentCode] [nvarchar](3) NULL,
	[OccurredDepartmentCode] [nvarchar](3) NULL,
	[PaymentPlanDate] [datetime] NULL,
	[SlipNumber] [nvarchar](50) NULL,
	[AccountCode] [nvarchar](50) NULL,
	[Amount] [decimal](10, 0) NULL,
	[PaymentableBalance] [decimal](10, 0) NULL,
	[CompleteFlag] [nvarchar](2) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[PurchaseOrderNumber] [nvarchar](50) NULL,
	[CarPurchaseOrderNumber] [nvarchar](50) NULL,
 CONSTRAINT [PK_PaymentPlan] PRIMARY KEY CLUSTERED 
(
	[PaymentPlanId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払予定ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'PaymentPlanId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'SupplierPaymentCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発生部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'OccurredDepartmentCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'PaymentPlanDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'科目コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'AccountCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'Amount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払残高' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'PaymentableBalance'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'完了フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'CompleteFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部品発注番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'PurchaseOrderNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両発注番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan', @level2type=N'COLUMN',@level2name=N'CarPurchaseOrderNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'支払予定' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentPlan'
GO
