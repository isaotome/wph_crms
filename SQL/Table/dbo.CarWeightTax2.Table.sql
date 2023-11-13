USE [WPH_DB]
GO
/****** Object:  Table [dbo].[CarWeightTax2]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarWeightTax2](
	[CarWeightTaxId] [uniqueidentifier] NOT NULL,
	[Amount] [decimal](10, 0) NOT NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[DelFlag] [nvarchar](2) NULL,
	[InspectionYear] [int] NOT NULL,
	[WeightFrom] [int] NOT NULL,
	[WeightTo] [int] NOT NULL,
	[ProgressYear] [int] NULL,
 CONSTRAINT [PK_CarWeightTax2] PRIMARY KEY CLUSTERED 
(
	[CarWeightTaxId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
