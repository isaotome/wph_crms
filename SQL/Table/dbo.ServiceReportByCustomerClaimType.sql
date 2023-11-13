USE [WPH_DB]
GO

/****** Object:  Table [dbo].[ServiceReportByCutomerClaimType]    Script Date: 2021/04/05 13:17:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ServiceReportByCutomerClaimType](
	[CusotmerClaimType] [nvarchar](3) NOT NULL,
	[ServiceClaimReport] [bit] NOT NULL,
	[ServiceClaimDetailReport] [bit] NOT NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_c_ServiceReportByCutomerClaimType] PRIMARY KEY CLUSTERED 
(
	[CusotmerClaimType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceReportByCutomerClaimType', @level2type=N'COLUMN',@level2name=N'CusotmerClaimType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'納品請求書' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceReportByCutomerClaimType', @level2type=N'COLUMN',@level2name=N'ServiceClaimReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求明細書' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceReportByCutomerClaimType', @level2type=N'COLUMN',@level2name=N'ServiceClaimDetailReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceReportByCutomerClaimType', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceReportByCutomerClaimType', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceReportByCutomerClaimType', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceReportByCutomerClaimType', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceReportByCutomerClaimType', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO


