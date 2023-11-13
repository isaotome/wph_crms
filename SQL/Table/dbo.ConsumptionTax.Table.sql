USE [WPH_DB]
GO
/****** Object:  Table [dbo].[ConsumptionTax]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConsumptionTax](
	[ConsumptionTaxId] [nvarchar](3) NOT NULL,
	[FromAvailableDate] [date] NOT NULL,
	[ToAvailableDate] [date] NOT NULL,
	[Rate] [smallint] NOT NULL,
	[RateName] [nvarchar](50) NOT NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
 CONSTRAINT [PK_ConsumptionTaxId] PRIMARY KEY CLUSTERED 
(
	[ConsumptionTaxId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
