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

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ�����ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarAppraisalId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ��`�[�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d���쐬�ς݃t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'PurchaseCreated'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�S���҃R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O���[�h�R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarGradeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ԍ��ؔ��s��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'IssueDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����ǃR�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'MorterViecleOfficialCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�^���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RegistrationNumberType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�^��ʁi���ȁj' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RegistrationNumberKana'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�^�i���o�[' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RegistrationNumberPlate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�o�^�N����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RegistrationDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���x�o�^�N��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'FirstRegistrationYear'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����Ԃ̎��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarClassification'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�p�r' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Usage'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���Ɨp�E���Ɨp�̕�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'UsageType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԑ̂̌`��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Figure'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���[�J�[���i�Ԗ��j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'MakerName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��Ԓ��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Capacity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ő�ύڗ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'MaximumLoadingWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ��d��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԗ����d��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'TotalCarWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ԑ�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Vin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Length'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Width'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Height'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O�O���d' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'FFAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O�㎲�d' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'FRAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��O���d' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RFAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'��㎲�d' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RRAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�^��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ModelName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����@�̌^��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'EngineType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���r�C��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Displacement'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�R���̎��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Fuel'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�^���w��ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ModelSpecificateNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ޕʋ敪�ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ClassificationTypeNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���L�҂̎���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'PossesorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���L�҂̏Z��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'PossesorAddress'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�g�p�҂̎���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'UserName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�g�p�҂̏Z��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'UserAddress'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�{���n' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'PrincipalPlace'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�L������' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'InspectionExpireDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���s����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Mileage'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���s�����P��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'MileageUnit'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���l' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Memo'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ފ����E�s��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'DocumentComplete'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���ޔ��l' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'DocumentRemarks'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�N��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ModelYear'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�u�����h��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarBrandName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�Ԏ햼' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O���[�h��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarGradeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�h�A' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Door'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�g�����X�~�b�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'TransMission'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O���F��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ExteriorColorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�F��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ChangeColor'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���F��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'OriginalColorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����F��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'InteriorColorName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ۏ؏�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Guarantee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Instructions'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�n���h��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Steering'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�A��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Import'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���C�g' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Light'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AW' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Aw'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�G�A��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Aero'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SR' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Sr'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CD' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Cd'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'MD' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Md'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�i�r�i�����E�����O�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'NaviType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�i�r�iHDD�A�������ADVD�ACD�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'NaviEquipment'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�i�r�iOnDash�AInDash�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'NaviDashboard'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�V�[�g�i�F�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'SeatColor'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�V�[�g' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'SeatType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'VIN�i�k�ėp�j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'UsVin'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���T�C�N��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Recycle'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���T�C�N����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RecycleTicket'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���T�C�N���ԍ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RecycleNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���T�C�N�����z' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RecycleDeposit'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�c��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RemainDebt'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�c�x����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'RemainDebtPayee'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���������Ԑ�' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CarTaxUnexpiredAmount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�O���]��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ExteriorEvaluation'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����]��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'InteriorEvaluation'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�C�����i�L���j' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'ReparationRecord'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�]���_' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Evaluation'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���艿�i' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'AppraisalPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���L����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'Remarks'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�쐬����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V��' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�ŏI�X�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�폜�t���O' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����o�^' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'EraseRegist'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�d���\���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'PurchasePlanDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'���L�҃R�[�h(�ڋq�R�[�h)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'OwnerCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�g�p�҃R�[�h' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'UserCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'�����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'AppraisalDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'����_���' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal', @level2type=N'COLUMN',@level2name=N'PurchaseAgreementDate'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'�g�����U�N�V����' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarAppraisal'
GO


