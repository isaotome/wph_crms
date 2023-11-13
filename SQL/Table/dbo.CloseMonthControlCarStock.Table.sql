USE [WPH_DB]
GO

/****** Object:  Table [dbo].[CloseMonthControlCarStock]    Script Date: 2015/04/08 13:28:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- 2015/04/08 arc yano é‘óºä«óùëŒâûáC
CREATE TABLE [dbo].[CloseMonthControlCarStock](
	[CloseMonth] [nvarchar](8) NOT NULL,
	[CloseStatus] [nchar](3) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_CloseMonthControlCarStock] PRIMARY KEY CLUSTERED 
(
	[CloseMonth] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
