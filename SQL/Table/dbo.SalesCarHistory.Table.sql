USE [WPH_DB]
GO

/****** Object:  Table [dbo].[SalesCarHistory]    Script Date: 2020/12/01 12:13:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SalesCarHistory](
	[SalesCarNumber] [nvarchar](50) NOT NULL,
	[RevisionNumber] [int] NOT NULL,
	[CarGradeCode] [nvarchar](30) NOT NULL,
	[NewUsedType] [nvarchar](3) NOT NULL,
	[ColorType] [nvarchar](3) NULL,
	[ExteriorColorCode] [nvarchar](8) NULL,
	[ExteriorColorName] [nvarchar](50) NULL,
	[ChangeColor] [nvarchar](3) NULL,
	[InteriorColorCode] [nvarchar](8) NULL,
	[InteriorColorName] [nvarchar](50) NULL,
	[ManufacturingYear] [nvarchar](10) NULL,
	[CarStatus] [nvarchar](3) NULL,
	[LocationCode] [nvarchar](12) NULL,
	[OwnerCode] [nvarchar](10) NULL,
	[Steering] [nvarchar](3) NULL,
	[SalesPrice] [decimal](10, 0) NULL,
	[IssueDate] [datetime] NULL,
	[MorterViecleOfficialCode] [nvarchar](5) NULL,
	[RegistrationNumberType] [nvarchar](3) NULL,
	[RegistrationNumberKana] [nvarchar](1) NULL,
	[RegistrationNumberPlate] [nvarchar](4) NULL,
	[RegistrationDate] [datetime] NULL,
	[FirstRegistrationYear] [nvarchar](9) NULL,
	[CarClassification] [nvarchar](3) NULL,
	[Usage] [nvarchar](3) NULL,
	[UsageType] [nvarchar](3) NULL,
	[Figure] [nvarchar](3) NULL,
	[MakerName] [nvarchar](50) NULL,
	[Capacity] [int] NULL,
	[MaximumLoadingWeight] [int] NULL,
	[CarWeight] [int] NULL,
	[TotalCarWeight] [int] NULL,
	[Vin] [nvarchar](20) NULL,
	[Length] [int] NULL,
	[Width] [int] NULL,
	[Height] [int] NULL,
	[FFAxileWeight] [int] NULL,
	[FRAxileWeight] [int] NULL,
	[RFAxileWeight] [int] NULL,
	[RRAxileWeight] [int] NULL,
	[ModelName] [nvarchar](20) NULL,
	[EngineType] [nvarchar](25) NULL,
	[Displacement] [decimal](10, 2) NULL,
	[Fuel] [nvarchar](10) NULL,
	[ModelSpecificateNumber] [nvarchar](10) NULL,
	[ClassificationTypeNumber] [nvarchar](10) NULL,
	[PossesorName] [nvarchar](80) NULL,
	[PossesorAddress] [nvarchar](300) NULL,
	[UserName] [nvarchar](80) NULL,
	[UserAddress] [nvarchar](300) NULL,
	[PrincipalPlace] [nvarchar](300) NULL,
	[ExpireType] [nvarchar](3) NULL,
	[ExpireDate] [datetime] NULL,
	[Mileage] [decimal](12, 2) NULL,
	[MileageUnit] [nvarchar](3) NULL,
	[Memo] [nvarchar](255) NULL,
	[DocumentComplete] [nvarchar](3) NULL,
	[DocumentRemarks] [nvarchar](100) NULL,
	[SalesDate] [datetime] NULL,
	[InspectionDate] [datetime] NULL,
	[NextInspectionDate] [datetime] NULL,
	[UsVin] [nvarchar](20) NULL,
	[MakerWarranty] [nvarchar](3) NULL,
	[RecordingNote] [nvarchar](3) NULL,
	[ProductionDate] [datetime] NULL,
	[ReparationRecord] [nvarchar](3) NULL,
	[Oil] [nvarchar](25) NULL,
	[Tire] [nvarchar](25) NULL,
	[KeyCode] [nvarchar](50) NULL,
	[AudioCode] [nvarchar](50) NULL,
	[Import] [nvarchar](3) NULL,
	[Guarantee] [nvarchar](3) NULL,
	[Instructions] [nvarchar](3) NULL,
	[Recycle] [nvarchar](3) NULL,
	[RecycleTicket] [nvarchar](3) NULL,
	[CouponPresence] [nvarchar](3) NULL,
	[Light] [nvarchar](3) NULL,
	[Aw] [nvarchar](3) NULL,
	[Aero] [nvarchar](3) NULL,
	[Sr] [nvarchar](3) NULL,
	[Cd] [nvarchar](3) NULL,
	[Md] [nvarchar](3) NULL,
	[NaviType] [nvarchar](3) NULL,
	[NaviEquipment] [nvarchar](3) NULL,
	[NaviDashboard] [nvarchar](3) NULL,
	[SeatColor] [nvarchar](10) NULL,
	[SeatType] [nvarchar](3) NULL,
	[Memo1] [nvarchar](100) NULL,
	[Memo2] [nvarchar](100) NULL,
	[Memo3] [nvarchar](100) NULL,
	[Memo4] [nvarchar](100) NULL,
	[Memo5] [nvarchar](100) NULL,
	[Memo6] [nvarchar](100) NULL,
	[Memo7] [nvarchar](100) NULL,
	[Memo8] [nvarchar](100) NULL,
	[Memo9] [nvarchar](100) NULL,
	[Memo10] [nvarchar](100) NULL,
	[DeclarationType] [nvarchar](3) NULL,
	[AcquisitionReason] [nvarchar](3) NULL,
	[TaxationTypeCarTax] [nvarchar](3) NULL,
	[TaxationTypeAcquisitionTax] [nvarchar](3) NULL,
	[CarTax] [decimal](10, 0) NULL,
	[CarWeightTax] [decimal](10, 0) NULL,
	[CarLiabilityInsurance] [decimal](10, 0) NULL,
	[AcquisitionTax] [decimal](10, 0) NULL,
	[RecycleDeposit] [decimal](10, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[EraseRegist] [nvarchar](3) NULL,
	[UserCode] [nvarchar](10) NULL,
	[ApprovedCarNumber] [nvarchar](50) NULL,
	[ApprovedCarWarrantyDateFrom] [datetime] NULL,
	[ApprovedCarWarrantyDateTo] [datetime] NULL,
	[Finance] [nvarchar](3) NULL,
	[OwnershipChangeType] [nvarchar](3) NULL,
	[OwnershipChangeDate] [datetime] NULL,
	[OwnershipChangeMemo] [nvarchar](1024) NULL,
	[InspectGuidFlag] [nvarchar](3) NULL,
	[InspectGuidMemo] [nvarchar](100) NULL,
	[CarUsage] [nvarchar](3) NULL,
	[FirstRegistrationDate] [datetime] NULL,
	[CompanyRegistrationFlag] [nvarchar](2) NULL,
	[ConfirmDriverLicense] [bit] NULL,
	[ConfirmCertificationSeal] [bit] NULL,
	[ConfirmOther] [nvarchar](100) NULL,
 CONSTRAINT [PK_SalesCarHistory] PRIMARY KEY CLUSTERED 
(
	[SalesCarNumber] ASC,
	[RevisionNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'管理番号(自動採番)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'SalesCarNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'リビジョン' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'RevisionNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両グレードコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'CarGradeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'新中区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'NewUsedType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系統色' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ColorType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'外装色コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ExteriorColorCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'外装色名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ExteriorColorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'色替' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ChangeColor'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'内装色コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'InteriorColorCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'内装色名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'InteriorColorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'年式' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ManufacturingYear'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'在庫ステータス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'CarStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'在庫ロケーション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'LocationCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'所有者コード(顧客コード)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'OwnerCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ハンドル' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Steering'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'販売価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'SalesPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車検証発行日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'IssueDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'陸運局コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'MorterViecleOfficialCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両登録番号(種別)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'RegistrationNumberType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両登録番号(かな)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'RegistrationNumberKana'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両登録番号(プレート)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'RegistrationNumberPlate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'RegistrationDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'初年度登録' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'FirstRegistrationYear'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自動車種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'CarClassification'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用途' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Usage'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'事自区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'UsageType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'形状' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Figure'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカー名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'MakerName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'定員' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Capacity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最大積載量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'MaximumLoadingWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両重量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'CarWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両総重量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'TotalCarWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車台番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Vin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'長さ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Length'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'幅' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Width'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'高さ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Height'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'前前軸重' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'FFAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'前後軸重' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'FRAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'後前軸重' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'RFAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'後後軸重' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'RRAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'型式' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ModelName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'原動機型式' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'EngineType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'排気量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Displacement'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'燃料種類' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Fuel'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'型式指定番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ModelSpecificateNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'類別区分番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ClassificationTypeNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'所有者氏名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'PossesorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'所有者住所' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'PossesorAddress'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'使用者氏名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'UserName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'使用者住所' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'UserAddress'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'本拠地' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'PrincipalPlace'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'有効期限種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ExpireType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'有効期限' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ExpireDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'走行距離' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Mileage'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'走行距離単位' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'MileageUnit'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車検証備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'書類完備' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'DocumentComplete'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'書類備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'DocumentRemarks'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'納車日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'SalesDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'点検日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'InspectionDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'次回点検日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'NextInspectionDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'VIN(北米用)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'UsVin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカー保証' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'MakerWarranty'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'記録簿(有無)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'RecordingNote'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'生産日(MDH)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ProductionDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'修復歴(有無)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ReparationRecord'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'お客様指定オイル(部品番号)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Oil'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'タイヤ(部品番号)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Tire'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'キーコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'KeyCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'オーディオコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'AudioCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'輸入' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Import'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'保証書' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Guarantee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'取説' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Instructions'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'リサイクル' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Recycle'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'リサイクル券' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'RecycleTicket'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'クーポン有無' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'CouponPresence'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ライト' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Light'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ＡＷ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Aw'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'エアロ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Aero'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ＳＲ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Sr'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ＣＤ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Cd'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ＭＤ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Md'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ナビ(純正・純正外)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'NaviType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ナビ(HDD、メモリ、DVD、CD)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'NaviEquipment'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ナビ(OnDash、InDash)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'NaviDashboard'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'シート(色)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'SeatColor'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'シート' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'SeatType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考１' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Memo1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考２' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Memo2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考３' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Memo3'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考４' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Memo4'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考５' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Memo5'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考６' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Memo6'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考７' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Memo7'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考８' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Memo8'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考９' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Memo9'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考１０' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Memo10'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'申告区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'DeclarationType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'取得原因' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'AcquisitionReason'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'課税区分(自動車税)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'TaxationTypeCarTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'課税区分(自動車取得税)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'TaxationTypeAcquisitionTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自動車税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'CarTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自動車重量税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'CarWeightTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自賠責保険料' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'CarLiabilityInsurance'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自動車取得税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'AcquisitionTax'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'リサイクル預託金' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'RecycleDeposit'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'抹消登録' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'EraseRegist'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'使用者コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'UserCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'認定中古車No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ApprovedCarNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'認定中古車保証期間FROM' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ApprovedCarWarrantyDateFrom'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'認定中古車保証期間TO' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ApprovedCarWarrantyDateTo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ファイナンス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'Finance'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'変更区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'OwnershipChangeType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'変更日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'OwnershipChangeDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'変更備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'OwnershipChangeMemo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'古物取引確認（運転免許証）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ConfirmDriverLicense'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'古物取引確認（印鑑証明）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ConfirmCertificationSeal'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'古物取引確認（その他）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory', @level2type=N'COLUMN',@level2name=N'ConfirmOther'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'車両履歴' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SalesCarHistory'
GO


