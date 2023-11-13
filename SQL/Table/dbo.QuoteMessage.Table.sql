USE [WPH_DB]
GO
/****** Object:  Table [dbo].[QuoteMessage]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuoteMessage](
	[CompanyCode] [nvarchar](3) NOT NULL,
	[QuoteType] [nvarchar](3) NOT NULL,
	[Description] [nvarchar](100) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_QuoteMessage] PRIMARY KEY CLUSTERED 
(
	[CompanyCode] ASC,
	[QuoteType] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[QuoteMessage]  WITH CHECK ADD  CONSTRAINT [FK_QuoteMessage_Company] FOREIGN KEY([CompanyCode])
REFERENCES [dbo].[Company] ([CompanyCode])
GO
ALTER TABLE [dbo].[QuoteMessage] CHECK CONSTRAINT [FK_QuoteMessage_Company]
GO
