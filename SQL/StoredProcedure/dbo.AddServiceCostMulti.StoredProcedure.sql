USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[AddServiceCostMulti]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddServiceCostMulti]
	@ServiceMenuCode nvarchar(8)
	,@CarClassCode nvarchar(30)
	,@EmployeeCode nvarchar(50)
AS

	IF @ServiceMenuCode IS NOT NULL
		INSERT INTO dbo.ServiceCost (
				ServiceMenuCode
				,CarClassCode
				,Cost
				,CreateEmployeeCode
				,CreateDate
				,LastUpdateEmployeeCode
				,LastUpdateDate
				,DelFlag
				)
		SELECT	@ServiceMenuCode
				, a.CarClassCode
				, 0
				, @EmployeeCode
				, SYSDATETIME()
				, @EmployeeCode
				, SYSDATETIME()
				, a.DelFlag
		FROM	dbo.CarClass a;

	IF @CarClassCode IS NOT NULL
		INSERT INTO dbo.ServiceCost (
				ServiceMenuCode
				,CarClassCode
				,Cost
				,CreateEmployeeCode
				,CreateDate
				,LastUpdateEmployeeCode
				,LastUpdateDate
				,DelFlag
				)
		SELECT	a.ServiceMenuCode
				, @CarClassCode
				, 0
				, @EmployeeCode
				, SYSDATETIME()
				, @EmployeeCode
				, SYSDATETIME()
				, a.DelFlag
		FROM	dbo.ServiceMenu a;
GO
