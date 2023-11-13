USE [WPH_DB]
GO
/****** Object:  Table [dbo].[CustomerUpdateLog]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerUpdateLog](
	[CustomerUpdateLogId] [uniqueidentifier] NOT NULL,
	[CustomerCode] [nvarchar](10) NULL,
	[UpdateColumn] [nvarchar](50) NULL,
	[UpdateValueFrom] [nvarchar](50) NULL,
	[UpdateValueTo] [nvarchar](50) NULL,
	[UpdateEmployeeCode] [nvarchar](50) NULL,
	[UpdateDate] [datetime] NULL,
 CONSTRAINT [PK_CustomerUpdateLog] PRIMARY KEY CLUSTERED 
(
	[CustomerUpdateLogId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'担当者推移ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerUpdateLog', @level2type=N'COLUMN',@level2name=N'CustomerUpdateLogId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerUpdateLog', @level2type=N'COLUMN',@level2name=N'CustomerCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'更新フィールド' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerUpdateLog', @level2type=N'COLUMN',@level2name=N'UpdateColumn'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'更新前の値' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerUpdateLog', @level2type=N'COLUMN',@level2name=N'UpdateValueFrom'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'更新後の値' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerUpdateLog', @level2type=N'COLUMN',@level2name=N'UpdateValueTo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'更新担当者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerUpdateLog', @level2type=N'COLUMN',@level2name=N'UpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerUpdateLog', @level2type=N'COLUMN',@level2name=N'UpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'担当者推移' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerUpdateLog'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerUpdateLog'
GO
