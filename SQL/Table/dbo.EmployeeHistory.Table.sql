USE [WPH_DB]
GO

/****** Object:  Table [dbo].[EmployeeHistory]    Script Date: 2019/06/04 16:36:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EmployeeHistory](
	[EmployeeCode] [nvarchar](50) NOT NULL,
	[RevisionNumber] [int] NOT NULL,
	[EmployeeNumber] [nvarchar](20) NULL,
	[EmployeeName] [nvarchar](40) NOT NULL,
	[EmployeeNameKana] [nvarchar](40) NULL,
	[DepartmentCode] [nvarchar](3) NOT NULL,
	[SecurityRoleCode] [nvarchar](50) NOT NULL,
	[MobileNumber] [nvarchar](15) NULL,
	[MobileMailAddress] [nvarchar](100) NULL,
	[MailAddress] [nvarchar](100) NULL,
	[EmployeeType] [nvarchar](3) NULL,
	[Birthday] [datetime] NULL,
	[LastLoginDateTime] [datetime] NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[DepartmentCode1] [nvarchar](3) NULL,
	[DepartmentCode2] [nvarchar](3) NULL,
	[DepartmentCode3] [nvarchar](3) NULL,
 CONSTRAINT [PK_EmployeeHistory] PRIMARY KEY CLUSTERED 
(
	[EmployeeCode] ASC,
	[RevisionNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'社員コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'EmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'改訂番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'RevisionNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'社員番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'EmployeeNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'氏名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'EmployeeName'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'氏名（カナ）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'EmployeeNameKana'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部門コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'DepartmentCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'セキュリティロールコード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'SecurityRoleCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'携帯電話番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'MobileNumber'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'携帯メールアドレス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'MobileMailAddress'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'メールアドレス' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'MailAddress'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'雇用区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'EmployeeType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'誕生日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'Birthday'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終ログイン日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'LastLoginDateTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'LastUpdateEmployeeCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最終更新日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'LastUpdateDate'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'削除フラグ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'DelFlag'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'兼務部門コード1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'DepartmentCode1'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'兼務部門コード2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'DepartmentCode2'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'兼務部門コード3' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory', @level2type=N'COLUMN',@level2name=N'DepartmentCode3'
GO

EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'社員' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory'
GO

EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeHistory'
GO


