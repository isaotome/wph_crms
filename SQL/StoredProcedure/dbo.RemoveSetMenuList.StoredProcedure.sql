USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[RemoveSetMenuList]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RemoveSetMenuList]
	@SetMenuCode nvarchar(11)
AS
		DELETE dbo.SetMenuList
		WHERE	SetMenuCode = @SetMenuCode;
GO
