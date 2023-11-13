USE [WPH_DB]
GO

/****** Object:  Table [dbo].[PartsPurchase]    Script Date: 2018/04/02 11:24:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PartsPurchase](
	[PurchaseNumber] [nvarchar](50) NOT NULL,
	[PurchaseOrderNumber] [nvarchar](50) NULL,
	[PurchaseType] [nvarchar](3) NOT NULL,
	[PurchasePlanDate] [datetime] NULL,
	[PurchaseDate] [datetime] NULL,
	[PurchaseStatus] [nvarchar](3) NOT NULL,
	[SupplierCode] [nvarchar](10) NOT NULL,
	[EmployeeCode] [nvarchar](50) NOT NULL,
	[DepartmentCode] [nvarchar](3) NOT NULL,
	[LocationCode] [nvarchar](12) NULL,
	[PartsNumber] [nvarchar](25) NOT NULL,
	[Price] [decimal](10, 0) NOT NULL,
	[Quantity] [decimal](10, 2) NULL,
	[Amount] [decimal](10, 0) NOT NULL,
	[ReceiptNumber] [nvarchar](50) NULL,
	[Memo] [nvarchar](100) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[ServiceSlipNumber] [nvarchar](50) NULL,
	[RevisionNumber] [int] NOT NULL DEFAULT ((1)),
	[InvoiceNo] [nvarchar](50) NULL,
	[MakerOrderNumber] [nvarchar](50) NULL,
	[ChangePartsFlag] [nvarchar](2) NULL,
	[OrderPartsNumber] [nvarchar](25) NULL DEFAULT (''),
	[WebOrderNumber] [nvarchar](50) NULL DEFAULT (''),
	[LinkEntryCaptureDate] [datetime] NULL,
 CONSTRAINT [PK_PartsPurchase] PRIMARY KEY CLUSTERED 
(
	[PurchaseNumber] ASC,
	[RevisionNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ד`�[�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseOrderNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�`�[�敪' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ח\���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PurchasePlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ד�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d���X�e�[�^�X' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'SupplierCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�S����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���P�[�V�����R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'LocationCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���i�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'PartsNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d���P��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'Price'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'Quantity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�[�i���ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'ReceiptNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���l' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�폜�t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�T�[�r�X�`�[�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'ServiceSlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Web�I�[�_�[�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase', @level2type=N'COLUMN',@level2name=N'WebOrderNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'���i�d��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'�g�����U�N�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PartsPurchase'
GO


