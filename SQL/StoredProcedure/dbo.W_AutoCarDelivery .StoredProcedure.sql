USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_AutoCarDelivery ]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[W_AutoCarDelivery ]
AS
BEGIN
	
	exec W_AA_AutoCarDelivery 
	exec W_Haiki_AutoCarDelivery 
	exec W_FAG_AutoCarDelivery 
	
END
GO
