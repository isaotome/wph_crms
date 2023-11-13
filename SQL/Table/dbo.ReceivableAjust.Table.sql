USE [WPH_DB]
GO

/****** Object:  Table [dbo].[ReceivableAjust]    Script Date: 2015/02/13 18:36:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ReceivableAjust](
	[InventoryMonth] [date] NULL,
	[SlipNumber] [nvarchar](50) NULL,
	[SlipType] [nchar](1) NULL,
	[Amount] [decimal](18, 0) NULL,
	[MaeAmount] [decimal](18, 0) NULL,
	[AtoAmount] [decimal](18, 0) NULL,
	[BalanceAmount] [decimal](18, 0) NULL,
	[Description] [nvarchar](50) NULL
) ON [PRIMARY]

GO


