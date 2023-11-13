USE [WPH_DB]
GO
/****** Object:  Table [dbo].[PrintPdfHistory]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PrintPdfHistory](
	[PdfId] [uniqueidentifier] NOT NULL,
	[SlipNumber] [nvarchar](50) NULL,
	[DocName] [nvarchar](50) NULL,
	[FileName] [nvarchar](50) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PDFID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PrintPdfHistory', @level2type=N'COLUMN',@level2name=N'PdfId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'伝票番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PrintPdfHistory', @level2type=N'COLUMN',@level2name=N'SlipNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'出力書類名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PrintPdfHistory', @level2type=N'COLUMN',@level2name=N'DocName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ファイル名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PrintPdfHistory', @level2type=N'COLUMN',@level2name=N'FileName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PrintPdfHistory', @level2type=N'COLUMN',@level2name=N'CreateEmployeeCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作成日時' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PrintPdfHistory', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'PDF出力履歴' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PrintPdfHistory'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'トラン' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PrintPdfHistory'
GO
