USE [WPH_DB]
GO
/****** Object:  Table [dbo].[Zip]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Zip](
	[ZipCode7] [nvarchar](7) NOT NULL,
	[Prefecture] [nvarchar](50) NULL,
	[City] [nvarchar](50) NULL,
	[Town] [nvarchar](50) NULL
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'郵便番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Zip', @level2type=N'COLUMN',@level2name=N'ZipCode7'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'都道府県' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Zip', @level2type=N'COLUMN',@level2name=N'Prefecture'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'市区町村' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Zip', @level2type=N'COLUMN',@level2name=N'City'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'町域' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Zip', @level2type=N'COLUMN',@level2name=N'Town'
GO
EXEC sys.sp_addextendedproperty @name=N'JpName', @value=N'郵便番号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Zip'
GO
EXEC sys.sp_addextendedproperty @name=N'TableType', @value=N'マスタ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Zip'
GO
