USE [WPH_DB]
GO

/****** Object:  Table [dbo].[CarPurchase]    Script Date: 2018/06/29 11:41:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[CarPurchase](
	[CarPurchaseId] [uniqueidentifier] NOT NULL,
	[CarPurchaseOrderNumber] [varchar](50) NULL,
	[CarAppraisalId] [uniqueidentifier] NULL,
	[PurchaseStatus] [nvarchar](3) NULL,
	[PurchaseDate] [datetime] NULL,
	[SupplierCode] [nvarchar](10) NULL,
	[PurchaseLocationCode] [nvarchar](12) NULL,
	[DepartmentCode] [nvarchar](3) NULL,
	[EmployeeCode] [nvarchar](50) NULL,
	[VehiclePrice] [decimal](10, 0) NOT NULL,
	[MetallicPrice] [decimal](10, 0) NOT NULL,
	[OptionPrice] [decimal](10, 0) NOT NULL,
	[FirmPrice] [decimal](10, 0) NOT NULL,
	[DiscountPrice] [decimal](10, 0) NOT NULL,
	[EquipmentPrice] [decimal](10, 0) NOT NULL,
	[RepairPrice] [decimal](10, 0) NOT NULL,
	[OthersPrice] [decimal](10, 0) NOT NULL,
	[Amount] [decimal](10, 0) NOT NULL,
	[TaxAmount] [decimal](10, 0) NOT NULL,
	[SalesCarNumber] [nvarchar](50) NOT NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[EraseRegist] [nvarchar](3) NULL,
	[Memo] [nvarchar](100) NULL,
	[SlipDate] [datetime] NULL,
	[CarTaxAppropriateAmount] [decimal](10, 0) NULL,
	[RecycleAmount] [decimal](10, 0) NULL,
	[CarPurchaseType] [nvarchar](3) NULL,
	[VehicleTax] [decimal](10, 0) NULL,
	[VehicleAmount] [decimal](10, 0) NULL,
	[MetallicTax] [decimal](10, 0) NULL,
	[MetallicAmount] [decimal](10, 0) NULL,
	[OptionTax] [decimal](10, 0) NULL,
	[OptionAmount] [decimal](10, 0) NULL,
	[FirmTax] [decimal](10, 0) NULL,
	[FirmAmount] [decimal](10, 0) NULL,
	[DiscountTax] [decimal](10, 0) NULL,
	[DiscountAmount] [decimal](10, 0) NULL,
	[OthersTax] [decimal](10, 0) NULL,
	[OthersAmount] [decimal](10, 0) NULL,
	[TotalAmount] [decimal](10, 0) NULL,
	[AuctionFeePrice] [decimal](10, 0) NULL,
	[AuctionFeeTax] [decimal](10, 0) NULL,
	[AuctionFeeAmount] [decimal](10, 0) NULL,
	[CarTaxAppropriatePrice] [decimal](10, 0) NULL,
	[RecyclePrice] [decimal](10, 0) NULL,
	[EquipmentTax] [decimal](10, 0) NULL,
	[EquipmentAmount] [decimal](10, 0) NULL,
	[RepairTax] [decimal](10, 0) NULL,
	[RepairAmount] [decimal](10, 0) NULL,
	[CarTaxAppropriateTax] [decimal](10, 0) NULL,
	[ConsumptionTaxId] [nvarchar](3) NULL,
	[Rate] [smallint] NULL,
	[CancelFlag] [nvarchar](2) NULL,
	[SlipNumber] [nvarchar](50) NULL DEFAULT (''),
	[LastEditScreen] [nvarchar](3) NOT NULL DEFAULT ('000'),
	[RegistOwnFlag] [nvarchar](2) NULL DEFAULT ('0'),
	[CancelDate] [datetime] NULL,
	[CancelCarPurchaseId] [nvarchar](36) NULL,
	[CancelMemo] [nvarchar](100) NULL,
	[FinancialAmount] [decimal](10, 0) NULL,
 CONSTRAINT [PK_CarPurchase] PRIMARY KEY CLUSTERED 
(
	[CarPurchaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ��d��ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarPurchaseId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ���������ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarPurchaseOrderNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ�����ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarAppraisalId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d���X�e�[�^�X' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d����R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'SupplierCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d�����P�[�V�����R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'PurchaseLocationCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�S���҃R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ��{�̉��i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'VehiclePrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���^���b�N���i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'MetallicPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�I�v�V�������i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'OptionPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�t�@�[�����i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'FirmPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�f�B�X�J�E���g���i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'DiscountPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�������i(�Ŕ�)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'EquipmentPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���C���i(�Ŕ�)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'RepairPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���̑����i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'OthersPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d�����i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'Amount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'TaxAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ǘ��ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'SalesCarNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�폜�t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����o�^' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'EraseRegist'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���l' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'SlipDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ŏ[���ō�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarTaxAppropriateAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���T�C�N���ō�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'RecycleAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ɋ敪' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarPurchaseType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ��{�̏����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'VehicleTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ��{�̐ō����i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'VehicleAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���^���b�N�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'MetallicTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���^���b�N�ō����i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'MetallicAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�I�v�V���������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'OptionTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�I�v�V�����ō����i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'OptionAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�t�@�[�������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'FirmTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�t�@�[���ō����i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'FirmAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�f�B�X�J�E���g�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'DiscountTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�f�B�X�J�E���g�ō����i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'DiscountAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���̑������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'OthersTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���̑��ō����i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'OthersAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d���ō����i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'TotalAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�I�[�N�V�������D��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'AuctionFeePrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�I�[�N�V�������D�������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'AuctionFeeTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�I�[�N�V�������D���ō�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'AuctionFeeAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ŏ[��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarTaxAppropriatePrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���T�C�N��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'RecyclePrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�������i(�����)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'EquipmentTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�������i(�ō�)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'EquipmentAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���C���i(�����)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'RepairTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���C���i(�ō�)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'RepairAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ŏ[�������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'CarTaxAppropriateTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�������i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase', @level2type=N'COLUMN',@level2name=N'FinancialAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'�ԗ��d��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'�g�����U�N�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchase'
GO


