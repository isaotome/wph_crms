USE [WPH_DB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ReceivableDetail](
	[ReceivableCode] [uniqueidentifier] NULL,
	[InventoryMonth] [date] NOT NULL,
	[SlipNumber] [nvarchar](50) NOT NULL,
	[CustomerClaimCode] [nvarchar](10) NOT NULL,
	[DepartmentCode] [nvarchar](3) NULL,
	[SlipType] [char](1) NULL,
	[SalesDate] [date] NULL,
	[Amount] [decimal](18, 0) NULL,
	[MaeAmount] [decimal](18, 0) NULL,
	[AtoAmount] [decimal](18, 0) NULL,
	[BalanceAmount] [decimal](18, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[TotalBalanceAmount] [decimal](18, 0) NULL,
 CONSTRAINT [PK_ReceivableDetail] PRIMARY KEY CLUSTERED 
(
	[InventoryMonth] ASC,
	[SlipNumber] ASC,
	[CustomerClaimCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'������R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceivableDetail', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceivableDetail', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceivableDetail', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceivableDetail', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceivableDetail', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�폜�t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ReceivableDetail', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO


