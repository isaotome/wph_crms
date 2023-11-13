USE [WPH_DB]
GO
/****** Object:  Table [dbo].[W_CustomerIntegradeHistory]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[W_CustomerIntegradeHistory](
	[IntegradeCode] [uniqueidentifier] NOT NULL,
	[CustomerCode1] [nvarchar](50) NULL,
	[CustomerCode2] [nvarchar](50) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_W_CustomerIntegradeHistory] PRIMARY KEY CLUSTERED 
(
	[IntegradeCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
