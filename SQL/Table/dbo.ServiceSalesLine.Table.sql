USE [WPH_DB]
GO

/****** Object:  Table [dbo].[ServiceSalesLine]    Script Date: 2017/11/07 16:17:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ServiceSalesLine](
	[SlipNumber] [nvarchar](50) NOT NULL,
	[RevisionNumber] [int] NOT NULL,
	[LineNumber] [int] NOT NULL,
	[ServiceType] [nvarchar](3) NOT NULL,
	[SetMenuCode] [nvarchar](11) NULL,
	[ServiceWorkCode] [nvarchar](5) NULL,
	[ServiceMenuCode] [nvarchar](8) NULL,
	[PartsNumber] [nvarchar](25) NULL,
	[LineContents] [nvarchar](50) NULL,
	[RequestComment] [nvarchar](100) NULL,
	[WorkType] [nvarchar](3) NULL,
	[LaborRate] [int] NULL,
	[ManPower] [decimal](5, 2) NULL,
	[TechnicalFeeAmount] [decimal](10, 0) NULL,
	[Quantity] [decimal](10, 2) NULL,
	[Price] [decimal](10, 0) NULL,
	[Amount] [decimal](10, 0) NULL,
	[Cost] [decimal](10, 0) NULL,
	[EmployeeCode] [nvarchar](50) NULL,
	[SupplierCode] [nvarchar](10) NULL,
	[CustomerClaimCode] [nvarchar](10) NULL,
	[StockStatus] [nvarchar](3) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[Classification1] [nvarchar](3) NULL,
	[TaxAmount] [decimal](10, 0) NULL,
	[UnitCost] [decimal](10, 0) NULL,
	[LineType] [nvarchar](3) NULL,
	[ConsumptionTaxId] [nvarchar](3) NULL,
	[Rate] [smallint] NULL,
	[ProvisionQuantity] [decimal](10, 2) NULL,
	[OrderQuantity] [decimal](10, 2) NULL,
	[DisplayOrder] [decimal](4, 1) NULL,
	[OutputTargetFlag] [nvarchar](1) NULL DEFAULT ('0'),
	[OutputFlag] [nvarchar](1) NULL DEFAULT ('0'),
 CONSTRAINT [PK_ServiceSalesLine] PRIMARY KEY CLUSTERED 
(
	[SlipNumber] ASC,
	[RevisionNumber] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�`�[�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�`�[�����ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'RevisionNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�s�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'LineNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���׎��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'ServiceType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Z�b�g���j���[�R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'SetMenuCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ƃR�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'ServiceWorkCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�T�[�r�X���j���[�R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'ServiceMenuCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���i�R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'PartsNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���׎���͒l' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'LineContents'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�R�����g' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'RequestComment'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��Ƌ敪' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'WorkType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���o���[�g' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'LaborRate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�H��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'ManPower'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Z�p��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'TechnicalFeeAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'Quantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�P��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'Price'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����(���v)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'Cost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���J�j�b�N�S���҃R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d����i�O����j�R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'SupplierCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'������R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'CustomerClaimCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�݌ɏ󋵁i�݌ɁA���j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'StockStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�폜�t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�啪��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'Classification1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'TaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����(�P��)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'UnitCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O�����J����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'LineType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����ϐ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'ProvisionQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'OrderQuantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�\������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'DisplayOrder'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����Ώۃt���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'OutputTargetFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����σt���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine', @level2type=N'COLUMN',@level2name=N'OutputFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'�T�[�r�X�`�[����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'�g�����U�N�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ServiceSalesLine'
GO


