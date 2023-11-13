USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[ChangeServiceCostMulti]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ChangeServiceCostMulti]
	@ServiceMenuCode nvarchar(5)
	,@CarClassCode nvarchar(30)
	,@DelFlag nvarchar(2)
	,@EmployeeCode nvarchar(50)
AS

	IF @ServiceMenuCode IS NOT NULL
		UPDATE dbo.ServiceCost
		SET		DelFlag = @DelFlag
		WHERE	ServiceMenuCode = @ServiceMenuCode;

	IF @CarClassCode IS NOT NULL
		UPDATE dbo.ServiceCost
		SET		DelFlag = @DelFlag
		WHERE	CarClassCode = @CarClassCode;
GO
