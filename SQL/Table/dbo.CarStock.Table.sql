USE [WPH_DB]
GO

/****** Object:  Table [dbo].[CarStock]    Script Date: 2018/10/16 12:23:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CarStock](
	[ProcessDate] [datetime] NOT NULL,
	[SalesCarNumber] [nvarchar](50) NOT NULL,
	[BrandStore] [nvarchar](50) NULL,
	[NewUsedType] [nvarchar](3) NULL,
	[PurchaseDate] [datetime] NULL,
	[CarName] [nvarchar](100) NULL,
	[CarGradeCode] [nvarchar](20) NULL,
	[Vin] [nvarchar](50) NULL,
	[PurchaseLocationCode] [nvarchar](12) NULL,
	[CarPurchaseType] [nvarchar](3) NULL,
	[SupplierCode] [nvarchar](10) NULL,
	[BeginningInventory] [decimal](10, 0) NULL,
	[MonthPurchase] [decimal](10, 0) NULL,
	[SalesDate] [datetime] NULL,
	[SlipNumber] [nvarchar](50) NULL,
	[SalesType] [nvarchar](50) NULL,
	[CustomerCode] [nvarchar](50) NULL,
	[SalesPrice] [decimal](10, 0) NULL,
	[DiscountAmount] [decimal](10, 0) NULL,
	[ShopOptionAmount] [decimal](10, 0) NULL,
	[SalesCostTotalAmount] [decimal](10, 0) NULL,
	[SalesTotalAmount] [decimal](10, 0) NULL,
	[SalesCostAmount] [decimal](10, 0) NULL,
	[SalesProfits] [decimal](10, 0) NULL,
	[ReductionTotal] [decimal](10, 0) NULL,
	[SelfRegistration] [decimal](10, 0) NULL,
	[OtherDealer] [decimal](10, 0) NULL,
	[DemoCar] [decimal](10, 0) NULL,
	[TemporaryCar] [decimal](10, 0) NULL,
	[EndInventory] [decimal](10, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[RecycleAmount] [decimal](10, 0) NULL,
	[OtherAccount] [decimal](10, 0) NULL,
	[RentalCar] [decimal](10, 0) NULL,
	[BusinessCar] [decimal](10, 0) NULL,
	[PRCar] [decimal](10, 0) NULL,
	[LocationCode] [nvarchar](12) NULL,
	[CancelPurchase] [decimal](10, 0) NULL,
	[CarPurchaseTypeName] [nvarchar](50) NULL,
	[MakerName] [nvarchar](100) NULL,
	[PurchaseLocationName] [nvarchar](50) NULL,
	[InventoryLocationName] [nvarchar](50) NULL,
	[SupplierName] [nvarchar](80) NULL,
	[SalesDepartmentCode] [nvarchar](3) NULL,
	[SalesDepartmentName] [nvarchar](20) NULL,
	[NewUsedTypeName] [nvarchar](50) NULL,
	[CustomerName] [nvarchar](80) NULL,
	[SelfRegistrationPurchaseDate] [datetime] NULL,
	[CustomerTypeName] [nvarchar](50) NULL,
 CONSTRAINT [PK_CarStock] PRIMARY KEY CLUSTERED 
(
	[ProcessDate] ASC,
	[SalesCarNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当月実棚' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarStock', @level2type=N'COLUMN',@level2name=N'LocationCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入区分名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarStock', @level2type=N'COLUMN',@level2name=N'CarPurchaseTypeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカー名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarStock', @level2type=N'COLUMN',@level2name=N'MakerName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入・在庫拠点名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarStock', @level2type=N'COLUMN',@level2name=N'PurchaseLocationName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当月実棚' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarStock', @level2type=N'COLUMN',@level2name=N'InventoryLocationName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入先名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarStock', @level2type=N'COLUMN',@level2name=N'SupplierName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'販売部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarStock', @level2type=N'COLUMN',@level2name=N'SalesDepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'販売部門名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarStock', @level2type=N'COLUMN',@level2name=N'SalesDepartmentName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'新中区分名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarStock', @level2type=N'COLUMN',@level2name=N'NewUsedTypeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarStock', @level2type=N'COLUMN',@level2name=N'CustomerName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自社登録前の車両仕入日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarStock', @level2type=N'COLUMN',@level2name=N'SelfRegistrationPurchaseDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客種別名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarStock', @level2type=N'COLUMN',@level2name=N'CustomerTypeName'
GO


