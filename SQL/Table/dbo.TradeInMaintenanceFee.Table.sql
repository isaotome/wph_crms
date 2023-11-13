USE [WPH_DB]
GO
/****** Object:  Table [dbo].[TradeInMaintenanceFee]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TradeInMaintenanceFee](
	[CarGradeCode] [nvarchar](30) NOT NULL,
	[U12] [decimal](10, 0) NULL,
	[U24] [decimal](10, 0) NULL,
	[U30] [decimal](10, 0) NULL,
	[U36] [decimal](10, 0) NULL,
	[U60] [decimal](10, 0) NULL,
	[U72] [decimal](10, 0) NULL,
	[U84] [decimal](10, 0) NULL,
	[O84] [decimal](10, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_TradeInMaintenanceFee] PRIMARY KEY CLUSTERED 
(
	[CarGradeCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'グレードコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'CarGradeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'12ヶ月未満金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'U12'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'24ヶ月未満金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'U24'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'30ヶ月未満金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'U30'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'36ヶ月未満金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'U36'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'60ヶ月未満金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'U60'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'72ヶ月未満金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'U72'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'84ヶ月未満金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'U84'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'84ヶ月以上金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'O84'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'中古車点検・整備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TradeInMaintenanceFee'
GO
