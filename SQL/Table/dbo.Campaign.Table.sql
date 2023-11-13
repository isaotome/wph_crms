USE [WPH_DB]
GO
/****** Object:  Table [dbo].[Campaign]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Campaign](
	[CampaignCode] [nvarchar](20) NOT NULL,
	[CampaignName] [nvarchar](100) NOT NULL,
	[TargetService] [nvarchar](3) NOT NULL,
	[EmployeeCode] [nvarchar](50) NOT NULL,
	[AdvertisingMedia] [nvarchar](50) NULL,
	[PublishStartDate] [datetime] NULL,
	[PublishEndDate] [datetime] NULL,
	[CampaignStartDate] [datetime] NULL,
	[CampaignEndDate] [datetime] NULL,
	[CampaignType] [nvarchar](3) NULL,
	[LoanCode] [nvarchar](10) NULL,
	[Cost] [decimal](10, 0) NULL,
	[MakerSupport] [nvarchar](50) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_Campaign] PRIMARY KEY CLUSTERED 
(
	[CampaignCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'イベントコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'CampaignCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'イベント名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'CampaignName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'対象業務' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'TargetService'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'担当者コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'広告媒体' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'AdvertisingMedia'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'掲載開始日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'PublishStartDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'掲載終了日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'PublishEndDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'イベント開始日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'CampaignStartDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'イベント終了日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'CampaignEndDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'イベント種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'CampaignType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'施策ローンコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'LoanCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'予算' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'Cost'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカーサポート' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'MakerSupport'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'イベント' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Campaign'
GO
