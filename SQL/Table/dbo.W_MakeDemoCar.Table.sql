USE [WPH_DB]
GO
/****** Object:  Table [dbo].[W_MakeDemoCar]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[W_MakeDemoCar](
	[SlipNumber] [nchar](8) NULL,
	[ProcType] [nchar](3) NULL,
	[ProcDate] [datetime] NULL,
	[LocationCode] [nchar](20) NULL,
	[SalesCarNumner] [nchar](20) NULL,
	[NewSalesCarNumner] [nchar](20) NULL,
	[CreateEmployeeCode] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[LastUpdateEmployeeCode] [nvarchar](50) NULL,
	[LastUpdateDate] [datetime] NULL,
	[ChangeStatus] [nchar](1) NULL
) ON [PRIMARY]
GO
