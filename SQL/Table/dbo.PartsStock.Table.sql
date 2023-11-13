USE [WPH_DB]
GO

/****** Object:  Table [dbo].[PartsStock]    Script Date: 2015/12/24 16:45:08 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PartsStock](
	[PartsNumber] [nvarchar](25) NOT NULL,
	[LocationCode] [nvarchar](12) NOT NULL,
	[Quantity] [decimal](10, 2) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[ProvisionQuantity] [decimal](10, 2) NULL,
 CONSTRAINT [PK_PartsStock] PRIMARY KEY CLUSTERED 
(
	[PartsNumber] ASC,
	[LocationCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���i�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsStock', @level2type=N'COLUMN',@level2name=N'PartsNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���P�[�V�����R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsStock', @level2type=N'COLUMN',@level2name=N'LocationCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsStock', @level2type=N'COLUMN',@level2name=N'Quantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsStock', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsStock', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsStock', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsStock', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�폜�t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsStock', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����ϐ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsStock', @level2type=N'COLUMN',@level2name=N'ProvisionQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'���i�݌�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsStock'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'�g�����U�N�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsStock'
GO


