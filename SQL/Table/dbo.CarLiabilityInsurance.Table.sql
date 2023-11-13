USE [WPH_DB]
GO
/****** Object:  Table [dbo].[CarLiabilityInsurance]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarLiabilityInsurance](
	[CarLiabilityInsuranceId] [uniqueidentifier] NOT NULL,
	[CarLiabilityInsuranceName] [nvarchar](100) NOT NULL,
	[Amount] [decimal](10, 0) NOT NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployee] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[NewDefaultFlag] [nvarchar](2) NULL,
	[UsedDefaultFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_CarLiabilityInsurance] PRIMARY KEY CLUSTERED 
(
	[CarLiabilityInsuranceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自賠責保険料ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarLiabilityInsurance', @level2type=N'COLUMN',@level2name=N'CarLiabilityInsuranceId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自賠責保険料表示名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarLiabilityInsurance', @level2type=N'COLUMN',@level2name=N'CarLiabilityInsuranceName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自賠責保険料' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarLiabilityInsurance', @level2type=N'COLUMN',@level2name=N'Amount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarLiabilityInsurance', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarLiabilityInsurance', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarLiabilityInsurance', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployee'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarLiabilityInsurance', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarLiabilityInsurance', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'新車デフォルトフラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarLiabilityInsurance', @level2type=N'COLUMN',@level2name=N'NewDefaultFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'中古車デフォルトフラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarLiabilityInsurance', @level2type=N'COLUMN',@level2name=N'UsedDefaultFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'自賠責保険' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarLiabilityInsurance'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarLiabilityInsurance'
GO
