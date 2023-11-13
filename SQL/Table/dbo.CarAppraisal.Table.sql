USE [WPH_DB]
GO

/****** Object:  Table [dbo].[CarAppraisal]    Script Date: 2020/12/01 12:11:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CarAppraisal](
	[CarAppraisalId] [uniqueidentifier] NOT NULL,
	[SlipNumber] [nvarchar](50) NULL,
	[PurchaseCreated] [nvarchar](3) NULL,
	[DepartmentCode] [nvarchar](3) NULL,
	[EmployeeCode] [nvarchar](50) NULL,
	[CarGradeCode] [nvarchar](30) NULL,
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
	[Vin] [nvarchar](20) NOT NULL,
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
	[PossesorName] [nvarchar](40) NULL,
	[PossesorAddress] [nvarchar](300) NULL,
	[UserName] [nvarchar](40) NULL,
	[UserAddress] [nvarchar](300) NULL,
	[PrincipalPlace] [nvarchar](300) NULL,
	[InspectionExpireDate] [datetime] NULL,
	[Mileage] [decimal](12, 2) NULL,
	[MileageUnit] [nvarchar](3) NULL,
	[Memo] [nvarchar](255) NULL,
	[DocumentComplete] [nvarchar](3) NULL,
	[DocumentRemarks] [nvarchar](100) NULL,
	[ModelYear] [nvarchar](20) NULL,
	[CarBrandName] [nvarchar](50) NULL,
	[CarName] [nvarchar](20) NULL,
	[CarGradeName] [nvarchar](50) NULL,
	[Door] [nvarchar](3) NULL,
	[TransMission] [nvarchar](3) NULL,
	[ExteriorColorName] [nvarchar](50) NULL,
	[ChangeColor] [nvarchar](3) NULL,
	[OriginalColorName] [nvarchar](50) NULL,
	[InteriorColorName] [nvarchar](50) NULL,
	[Guarantee] [nvarchar](3) NULL,
	[Instructions] [nvarchar](3) NULL,
	[Steering] [nvarchar](3) NULL,
	[Import] [nvarchar](3) NULL,
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
	[UsVin] [nvarchar](20) NULL,
	[Recycle] [nvarchar](3) NULL,
	[RecycleTicket] [nvarchar](3) NULL,
	[RecycleNumber] [nvarchar](12) NULL,
	[RecycleDeposit] [decimal](10, 0) NULL,
	[RemainDebt] [decimal](10, 0) NULL,
	[RemainDebtPayee] [nvarchar](40) NULL,
	[CarTaxUnexpiredAmount] [decimal](10, 0) NULL,
	[ExteriorEvaluation] [nvarchar](3) NULL,
	[InteriorEvaluation] [nvarchar](3) NULL,
	[ReparationRecord] [nvarchar](3) NULL,
	[Evaluation] [nvarchar](3) NULL,
	[AppraisalPrice] [decimal](10, 0) NULL,
	[Remarks] [nvarchar](500) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[EraseRegist] [nvarchar](3) NULL,
	[PurchasePlanDate] [datetime] NULL,
	[OwnerCode] [nvarchar](10) NULL,
	[UserCode] [nvarchar](10) NULL,
	[AppraisalDate] [datetime] NULL,
	[PurchaseAgreementDate] [datetime] NULL,
	[ConsumptionTaxId] [nvarchar](3) NULL,
	[Rate] [smallint] NULL,
	[LastEditScreen] [nvarchar](3) NOT NULL,
 CONSTRAINT [PK_CarAppraisal] PRIMARY KEY CLUSTERED 
(
	[CarAppraisalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CarAppraisal] ADD  DEFAULT ('000') FOR [LastEditScreen]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両査定ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarAppraisalId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入作成済みフラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'PurchaseCreated'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'担当者コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'グレードコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarGradeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車検証発行日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'IssueDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'陸事局コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'MorterViecleOfficialCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RegistrationNumberType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録種別（かな）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RegistrationNumberKana'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録ナンバー' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RegistrationNumberPlate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録年月日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RegistrationDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'初度登録年月' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'FirstRegistrationYear'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自動車の種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarClassification'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用途' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Usage'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自家用・事業用の別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'UsageType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車体の形状' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Figure'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メーカー名（車名）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'MakerName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'乗車定員' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Capacity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最大積載量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'MaximumLoadingWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両重量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両総重量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'TotalCarWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車台番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Vin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'長さ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Length'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'幅' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Width'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'高さ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Height'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'前前軸重' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'FFAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'前後軸重' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'FRAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'後前軸重' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RFAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'後後軸重' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RRAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'型式' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ModelName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'原動機の型式' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'EngineType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'総排気量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Displacement'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'燃料の種類' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Fuel'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'型式指定番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ModelSpecificateNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'類別区分番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ClassificationTypeNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'所有者の氏名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'PossesorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'所有者の住所' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'PossesorAddress'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'使用者の氏名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'UserName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'使用者の住所' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'UserAddress'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'本拠地' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'PrincipalPlace'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'有効期限' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'InspectionExpireDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'走行距離' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Mileage'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'走行距離単位' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'MileageUnit'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'書類完備・不備' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'DocumentComplete'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'書類備考' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'DocumentRemarks'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'年式' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ModelYear'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ブランド名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarBrandName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車種名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'グレード名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarGradeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ドア' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Door'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'トランスミッション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'TransMission'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'外装色名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ExteriorColorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'色替' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ChangeColor'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'元色名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'OriginalColorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'内装色名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'InteriorColorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'保証書' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Guarantee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'取説' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Instructions'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ハンドル' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Steering'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'輸入' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Import'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ライト' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Light'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AW' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Aw'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'エアロ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Aero'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SR' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Sr'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CD' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Cd'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'MD' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Md'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ナビ（純正・純性外）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'NaviType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ナビ（HDD、メモリ、DVD、CD）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'NaviEquipment'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ナビ（OnDash、InDash）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'NaviDashboard'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'シート（色）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'SeatColor'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'シート' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'SeatType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'VIN（北米用）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'UsVin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'リサイクル' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Recycle'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'リサイクル券' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RecycleTicket'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'リサイクル番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RecycleNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'リサイクル金額' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RecycleDeposit'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'残債' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RemainDebt'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'残債支払先' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RemainDebtPayee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'未払自動車税' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarTaxUnexpiredAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'外装評価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ExteriorEvaluation'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'内装評価' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'InteriorEvaluation'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'修復歴（有無）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ReparationRecord'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評価点' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Evaluation'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'査定価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'AppraisalPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'特記事項' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Remarks'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'抹消登録' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'EraseRegist'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'仕入予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'PurchasePlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'所有者コード(顧客コード)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'OwnerCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'使用者コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'UserCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'査定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'AppraisalDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'買取契約日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'PurchaseAgreementDate'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'査定' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal'
GO


