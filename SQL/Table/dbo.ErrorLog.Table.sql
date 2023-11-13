USE [WPH_DB]
GO
/****** Object:  Table [dbo].[ErrorLog]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ErrorLog](
	[CreateEmployeeCode] [nvarchar](50) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[Uri] [nvarchar](255) NULL,
	[Controller] [nvarchar](50) NULL,
	[Action] [nvarchar](50) NULL,
	[Message] [nvarchar](max) NULL,
	[KeyData] [nvarchar](max) NULL,
	[Stack] [nvarchar](max) NULL,
 CONSTRAINT [PK_ErrorLog_1] PRIMARY KEY CLUSTERED 
(
	[CreateEmployeeCode] ASC,
	[CreateDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
