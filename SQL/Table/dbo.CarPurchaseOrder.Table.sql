USE [WPH_DB]
GO
/****** Object:  Table [dbo].[CarPurchaseOrder]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarPurchaseOrder](
	[CarPurchaseOrderNumber] [nvarchar](50) NOT NULL,
	[SlipNumber] [nvarchar](50) NULL,
	[PurchaseOrderStatus] [nvarchar](3) NULL,
	[ReservationStatus] [nvarchar](3) NULL,
	[PurchasePlanStatus] [nvarchar](3) NULL,
	[RegistrationStatus] [nvarchar](3) NULL,
	[SalesCarNumber] [nvarchar](50) NULL,
	[PurchaseOrderDate] [datetime] NULL,
	[MakerOrderNumber] [nvarchar](20) NULL,
	[EmployeeCode] [nvarchar](50) NULL,
	[OrderType] [nvarchar](3) NULL,
	[ArrangementNumber] [nvarchar](20) NULL,
	[SupplierCode] [nvarchar](10) NULL,
	[SupplierPaymentCode] [nvarchar](10) NULL,
	[PDIDepartureDate] [datetime] NULL,
	[MakerShipmentDate] [datetime] NULL,
	[Vin] [nvarchar](30) NULL,
	[ReserveLocationCode] [nvarchar](12) NULL,
	[InspectionDate] [datetime] NULL,
	[InspectionInformation] [nvarchar](50) NULL,
	[IncentiveOfficeCode] [nvarchar](3) NULL,
	[ArrivalLocationCode] [nvarchar](12) NULL,
	[ArrivalPlanDate] [datetime] NULL,
	[RegistrationPlanMonth] [nvarchar](10) NULL,
	[RegistrationPlanDate] [datetime] NULL,
	[RegistrationDate] [datetime] NULL,
	[DocumentReceiptPlanDate] [datetime] NULL,
	[DocumentReceiptDate] [datetime] NULL,
	[GracePeriod] [nvarchar](10) NULL,
	[PayDueDate] [datetime] NULL,
	[StopFlag] [nvarchar](3) NULL,
	[InvoiceDate] [datetime] NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[MakerShipmentPlanDate] [datetime] NULL,
	[ReInspectionDate] [datetime] NULL,
	[RegistMonth] [nvarchar](2) NULL,
	[DocumentPurchaseRequestDate] [datetime] NULL,
	[Firm] [nvarchar](3) NULL,
	[DocumentPurchaseDate] [datetime] NULL,
	[VehiclePrice] [decimal](10, 0) NULL,
	[DiscountAmount] [decimal](10, 0) NULL,
	[MetallicPrice] [decimal](10, 0) NULL,
	[OptionPrice] [decimal](10, 0) NULL,
	[Amount] [decimal](10, 0) NULL,
	[FirmMargin] [decimal](10, 0) NULL,
 CONSTRAINT [PK_CarPurchaseOrder] PRIMARY KEY CLUSTERED 
(
	[CarPurchaseOrderNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両発注依頼番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'CarPurchaseOrderNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発注ステータス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'PurchaseOrderStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'引当ステータス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'ReservationStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入予定作成ステータス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'PurchasePlanStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録ステータス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'RegistrationStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'管理番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'SalesCarNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発注日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'PurchaseOrderDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカー発注番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'MakerOrderNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'発注担当者コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'オーダー区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'OrderType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'整理番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'ArrangementNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'SupplierCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払先コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'SupplierPaymentCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PDI出荷日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'PDIDepartureDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカー出荷日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'MakerShipmentDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車台番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'Vin'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'引当ロケーションコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'ReserveLocationCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'検査取得日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'InspectionDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'検査情報' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'InspectionInformation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'インセン対象店' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'IncentiveOfficeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入庫ロケーション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'ArrivalLocationCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'ArrivalPlanDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録予定月' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'RegistrationPlanMonth'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'RegistrationPlanDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'RegistrationDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'書類到着予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'DocumentReceiptPlanDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'書類到着日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'DocumentReceiptDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'金利猶予期間' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'GracePeriod'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'支払期限' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'PayDueDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'預り' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'StopFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'InvoiceDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカー出荷予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'MakerShipmentPlanDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'再予備検取得日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'ReInspectionDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'月内' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'RegistMonth'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'書類購入希望日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'DocumentPurchaseRequestDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ファーム' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'Firm'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'書類購入日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'DocumentPurchaseDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両本体価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'VehiclePrice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'値引金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'DiscountAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メタリック' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'MetallicPrice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'オプション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'OptionPrice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'Amount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ファームマージン' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder', @level2type=N'COLUMN',@level2name=N'FirmMargin'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'車両発注引当' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarPurchaseOrder'
GO
