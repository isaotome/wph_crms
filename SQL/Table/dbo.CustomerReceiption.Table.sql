USE [WPH_DB]
GO
/****** Object:  Table [dbo].[CustomerReceiption]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerReceiption](
	[CarReceiptionId] [uniqueidentifier] NOT NULL,
	[ReceiptionDate] [datetime] NOT NULL,
	[DepartmentCode] [nvarchar](3) NOT NULL,
	[CustomerCode] [nvarchar](10) NOT NULL,
	[EmployeeCode] [nvarchar](50) NOT NULL,
	[VisitOpportunity] [nvarchar](3) NULL,
	[ReceiptionType] [nvarchar](50) NULL,
	[CampaignCode1] [nvarchar](20) NULL,
	[CampaignCode2] [nvarchar](20) NULL,
	[KnowOpportunity] [nvarchar](3) NULL,
	[Purpose] [nvarchar](3) NULL,
	[Demand] [nvarchar](3) NULL,
	[AttractivePoint] [nvarchar](3) NULL,
	[Questionnarie] [nvarchar](3) NULL,
	[QuestionnarieEntryDate] [datetime] NULL,
	[MediaOpportunity] [nvarchar](50) NULL,
	[InterestedCar1] [nvarchar](50) NULL,
	[InterestedCar2] [nvarchar](50) NULL,
	[InterestedCar3] [nvarchar](50) NULL,
	[InterestedCar4] [nvarchar](50) NULL,
	[Memo1] [nvarchar](100) NULL,
	[Memo2] [nvarchar](100) NULL,
	[Memo3] [nvarchar](100) NULL,
	[Question1] [nvarchar](60) NULL,
	[Answer1] [nvarchar](100) NULL,
	[Question2] [nvarchar](60) NULL,
	[Answer2] [nvarchar](100) NULL,
	[Question3] [nvarchar](60) NULL,
	[Answer3] [nvarchar](100) NULL,
	[Question4] [nvarchar](60) NULL,
	[Answer4] [nvarchar](100) NULL,
	[Question5] [nvarchar](60) NULL,
	[Answer5] [nvarchar](100) NULL,
	[ArrivalPlanDate] [datetime] NULL,
	[RequestContent] [nvarchar](3) NULL,
	[RequestDetail] [nvarchar](200) NULL,
	[ReceiptionState] [nvarchar](3) NULL,
	[CarGradeCode] [nvarchar](30) NULL,
	[Vin] [nvarchar](20) NULL,
	[MorterViecleOfficialCode] [nvarchar](5) NULL,
	[RegistrationNumberType] [nvarchar](10) NULL,
	[RegistrationNumberKana] [nvarchar](10) NULL,
	[RegistrationNumberPlate] [nvarchar](10) NULL,
	[Mileage] [decimal](12, 2) NULL,
	[MileageUnit] [nvarchar](3) NULL,
	[SalesCarNumber] [nvarchar](50) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[RegistrationDate] [datetime] NULL,
	[FirstRegistrationYear] [nvarchar](9) NULL,
 CONSTRAINT [PK_CustomerReceiption] PRIMARY KEY CLUSTERED 
(
	[CarReceiptionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'店舗受付のユニークなID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'CarReceiptionId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'受付日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'ReceiptionDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'顧客コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'CustomerCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'担当者コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'来店きっかけ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'VisitOpportunity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'来店種別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'ReceiptionType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'イベントコード1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'CampaignCode1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'イベントコード2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'CampaignCode2'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ショールームを知ったきっかけ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'KnowOpportunity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'来店目的' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Purpose'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'今後の展望' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Demand'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ブランドの魅力' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'AttractivePoint'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'アンケートの二次使用（０：いいえ 1：はい）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Questionnarie'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'アンケート記入日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'QuestionnarieEntryDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'きっかけの媒体情報' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'MediaOpportunity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'興味のある商品1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'InterestedCar1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'興味のある商品2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'InterestedCar2'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'興味のある商品3' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'InterestedCar3'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'興味のある商品4' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'InterestedCar4'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Memo1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Memo2'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備考3' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Memo3'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自由質問1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Question1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自由回答1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Answer1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自由質問2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Question2'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自由回答2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Answer2'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自由質問3' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Question3'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自由回答3' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Answer3'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自由質問4' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Question4'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自由回答4' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Answer4'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自由質問5' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Question5'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'自由回答5' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Answer5'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'入庫予定日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'ArrivalPlanDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'依頼事項' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'RequestContent'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'依頼内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'RequestDetail'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'受付状況' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'ReceiptionState'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'グレードコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'CarGradeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車台番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Vin'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'陸運局コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'MorterViecleOfficialCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両登録番号（種別）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'RegistrationNumberType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両登録番号（かな）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'RegistrationNumberKana'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'車両登録番号（プレート）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'RegistrationNumberPlate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'走行距離' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'Mileage'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'走行距離単位' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'MileageUnit'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'管理番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'SalesCarNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'登録日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'RegistrationDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'初度登録年月' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption', @level2type=N'COLUMN',@level2name=N'FirstRegistrationYear'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'顧客受付' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トランザクション' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomerReceiption'
GO
