USE [WPH_DB]
GO

/****** Object:  Table [dbo].[CarFixedAssets]    Script Date: 2018/06/29 11:42:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CarFixedAssets](
	[SalesCarNumber] [nvarchar](50) NOT NULL,
	[Vin] [nvarchar](20) NOT NULL,
	[UsefulLives] [int] NULL,
	[AcquisitionDate] [datetime] NULL,
	[LossDate] [datetime] NULL,
	[AcquisitionPrice] [decimal](10, 0) NULL,
	[UndepreciatedBalance] [decimal](10, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_CarFixedAssets] PRIMARY KEY CLUSTERED 
(
	[SalesCarNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ǘ��ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarFixedAssets', @level2type=N'COLUMN',@level2name=N'SalesCarNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԑ�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarFixedAssets', @level2type=N'COLUMN',@level2name=N'Vin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ϗp�N��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarFixedAssets', @level2type=N'COLUMN',@level2name=N'UsefulLives'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�擾��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarFixedAssets', @level2type=N'COLUMN',@level2name=N'AcquisitionDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���p��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarFixedAssets', @level2type=N'COLUMN',@level2name=N'LossDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�擾���i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarFixedAssets', @level2type=N'COLUMN',@level2name=N'AcquisitionPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����p�c��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarFixedAssets', @level2type=N'COLUMN',@level2name=N'UndepreciatedBalance'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarFixedAssets', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarFixedAssets', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarFixedAssets', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarFixedAssets', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�폜�t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarFixedAssets', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO


