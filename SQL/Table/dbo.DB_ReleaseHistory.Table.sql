USE [WPH_DB]
GO

/****** Object:  Table [dbo].[DB_ReleaseHistory]    Script Date: 2017/03/07 10:15:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DB_ReleaseHistory](
	[HistoryID] [uniqueidentifier] NOT NULL,
	[TicketNumber] [nvarchar](7) NULL,
	[QueryName] [nvarchar](200) NULL,
	[ReleaseDate] [datetime] NULL,
	[Summary] [nvarchar](max) NULL,
	[ExecEmployeeCode] [nvarchar](50) NULL,
	[ExecDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[HistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


