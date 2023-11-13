USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_Make_ApprovalTasks]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[W_Make_ApprovalTasks]
AS
BEGIN

----------------------
--森本さんのデータ承認
----------------------
update Task set EmployeeCode ='morimoto.osamu'
,LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='system'
where TaskConfigId='101' and DepartmentCode='121' and Employeecode='system'
----------------------
--前田さんのデータ承認
----------------------
update Task set EmployeeCode ='maeda.kenshi'
,LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='system'
where EmployeeCode='maeda.kenshi2'

END
GO
