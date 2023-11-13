USE [WPH_DB]
GO

/****** Object:  Table [dbo].[c_CustomerClaimType]    Script Date: 2016/06/07 9:54:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[c_CustomerClaimType](
	[Code] [varchar](3) NOT NULL,
	[Name] [varchar](50) NULL,
	[ShortName] [varchar](50) NULL,
	[DisplayOrder] [int] NULL,
	[DelFlag] [varchar](2) NULL,
	[CustomerClaimClass] [nvarchar](3) NULL,
	[CustomerClaimFilter] [nvarchar](3) NULL,
 CONSTRAINT [PK_c_CustomerClaimType] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請求先分類' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'c_CustomerClaimType', @level2type=N'COLUMN',@level2name=N'CustomerClaimClass'
GO


