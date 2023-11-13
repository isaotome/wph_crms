USE [WPH_DB]
GO
/****** Object:  Table [dbo].[PaymentKind]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentKind](
	[PaymentKindCode] [nvarchar](10) NOT NULL,
	[PaymentKindName] [nvarchar](50) NOT NULL,
	[CommissionRate] [decimal](8, 5) NOT NULL,
	[ClaimDay] [int] NOT NULL,
	[PaymentType] [nvarchar](3) NOT NULL,
	[PaymentDay] [int] NOT NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_PaymentKind] PRIMARY KEY CLUSTERED 
(
	[PaymentKindCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'決済条件コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind', @level2type=N'COLUMN',@level2name=N'PaymentKindCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'決済条件名称（「20日締め翌10日払い」など' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind', @level2type=N'COLUMN',@level2name=N'PaymentKindName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'手数料率' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind', @level2type=N'COLUMN',@level2name=N'CommissionRate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'締め日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind', @level2type=N'COLUMN',@level2name=N'ClaimDay'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払区分（0:当月、1:翌月）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind', @level2type=N'COLUMN',@level2name=N'PaymentType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind', @level2type=N'COLUMN',@level2name=N'PaymentDay'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'支払種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentKind'
GO
