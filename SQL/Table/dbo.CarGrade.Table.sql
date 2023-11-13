USE [WPH_DB]
GO

/****** Object:  Table [dbo].[CarGrade]    Script Date: 2020/12/01 12:11:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CarGrade](
	[CarGradeCode] [nvarchar](30) NOT NULL,
	[CarGradeName] [nvarchar](50) NOT NULL,
	[ModelCode] [nvarchar](10) NULL,
	[CarCode] [nvarchar](30) NOT NULL,
	[CarClassCode] [nvarchar](30) NOT NULL,
	[ModelYear] [nvarchar](20) NULL,
	[Door] [nvarchar](3) NULL,
	[TransMission] [nvarchar](3) NULL,
	[Capacity] [int] NULL,
	[SalesPrice] [decimal](10, 0) NULL,
	[SalesStartDate] [datetime] NULL,
	[SalesEndDate] [datetime] NULL,
	[MaximumLoadingWeight] [int] NULL,
	[CarWeight] [int] NULL,
	[TotalCarWeight] [int] NULL,
	[DrivingName] [nvarchar](3) NULL,
	[ClassificationTypeNumber] [nvarchar](10) NULL,
	[ModelSpecificateNumber] [nvarchar](10) NULL,
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
	[VehicleType] [nvarchar](3) NULL,
	[InspectionRegistCost] [decimal](10, 0) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[RecycleDeposit] [decimal](10, 0) NULL,
	[Under24] [decimal](10, 0) NULL,
	[Under26] [decimal](10, 0) NULL,
	[Under28] [decimal](10, 0) NULL,
	[Under30] [decimal](10, 0) NULL,
	[Under36] [decimal](10, 0) NULL,
	[Under72] [decimal](10, 0) NULL,
	[Under84] [decimal](10, 0) NULL,
	[Over84] [decimal](10, 0) NULL,
	[CarClassification] [nvarchar](3) NULL,
	[Usage] [nvarchar](3) NULL,
	[UsageType] [nvarchar](3) NULL,
	[Figure] [nvarchar](3) NULL,
 CONSTRAINT [PK_CarGrade] PRIMARY KEY CLUSTERED 
(
	[CarGradeCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'グレードコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'CarGradeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'グレード名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'CarGradeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'モデルコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'ModelCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車種コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'CarCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両クラスコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'CarClassCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'モデル年' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'ModelYear'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ドア' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Door'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'トランスミッション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'TransMission'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'乗車定員' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Capacity'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'販売価格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'SalesPrice'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'販売開始日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'SalesStartDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'販売終了日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'SalesEndDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最大積載量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'MaximumLoadingWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両重量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'CarWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両総重量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'TotalCarWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'駆動名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'DrivingName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'類別区分番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'ClassificationTypeNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'型式指定番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'ModelSpecificateNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'長さ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Length'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'幅' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Width'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'高さ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Height'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'前前軸重' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'FFAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'前後軸重' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'FRAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'後前軸重' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'RFAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'後後軸重' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'RRAxileWeight'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'型式' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'ModelName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'原動機型式' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'EngineType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'総排気量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Displacement'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'燃料の種類' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Fuel'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'エンジン種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'VehicleType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'検査・登録費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'InspectionRegistCost'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'リサイクル預託金' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'RecycleDeposit'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'経過年数24ヶ月未満の中古車点検・整備費用及び中古車継承整備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Under24'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'経過年数26ヶ月未満の中古車点検・整備費用及び中古車継承整備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Under26'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'経過年数28ヶ月未満の中古車点検・整備費用及び中古車継承整備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Under28'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'経過年数30ヶ月未満の中古車点検・整備費用及び中古車継承整備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Under30'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'経過年数36ヶ月未満の中古車点検・整備費用及び中古車継承整備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Under36'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'経過年数72ヶ月未満の中古車点検・整備費用及び中古車継承整備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Under72'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'経過年数84ヶ月未満の中古車点検・整備費用及び中古車継承整備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Under84'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'経過年数84ヶ月以上の中古車点検・整備費用及び中古車継承整備費用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Over84'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自動車の種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'CarClassification'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用途' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Usage'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自家用・事業用の別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'UsageType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車体の形状' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade', @level2type=N'COLUMN',@level2name=N'Figure'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'グレード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CarGrade'
GO


