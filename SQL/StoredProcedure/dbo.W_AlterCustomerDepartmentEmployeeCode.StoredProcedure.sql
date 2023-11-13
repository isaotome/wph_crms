USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_AlterCustomerDepartmentEmployeeCode]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[W_AlterCustomerDepartmentEmployeeCode]
AS
BEGIN

----------------
--車両担当拠点--
----------------
--拠点が空白
--select *
update Customer Set DepartmentCode=H.DepartmentCode
from CarSalesHeader H
	inner join Customer C On H.CustomerCode=C.CustomerCode
where H.DelFlag='0' and H.SalesOrderStatus='005'
	and ( C.DepartmentCode is null or RTRIM(C.DepartmentCode) = '')
	and LEFT(H.SlipNumber,1)<>'2'
	
--拠点があってない
--select *  
update Customer Set DepartmentCode=H.DepartmentCode
from CarSalesHeader H
	inner join Customer C On H.CustomerCode=C.CustomerCode
where H.DelFlag='0' and H.SalesOrderStatus='005'
	and H.DepartmentCode <> C.DepartmentCode
	and LEFT(H.SlipNumber,1)<>'2'
	and DATEDIFF(m,H.salesdate,GETDATE()) < 2
	and DATEDIFF(d,H.LastUpdateDate,Getdate()) < 1
--------------
--車両担当者--
--------------
--担当者が空白
--select *  
update Customer Set CarEmployeeCode=H.EmployeeCode
from CarSalesHeader H
	inner join Customer C On H.CustomerCode=C.CustomerCode
where H.DelFlag='0' and H.SalesOrderStatus='005'
	and ( C.CarEmployeeCode is null or RTRIM(C.CarEmployeeCode) = '')
	and LEFT(H.SlipNumber,1)<>'2'
--担当者があってない
--select *  
update Customer Set CarEmployeeCode=H.EmployeeCode
from CarSalesHeader H
	inner join Customer C On H.CustomerCode=C.CustomerCode
where H.DelFlag='0' and H.SalesOrderStatus='005'
	and H.EmployeeCode <> C.CarEmployeeCode
	and LEFT(H.SlipNumber,1)<>'2'
	and DATEDIFF(m,H.salesdate,GETDATE()) < 2
	and DATEDIFF(d,H.LastUpdateDate,Getdate()) < 1
--------------------
--サービス担当拠点--
--------------------
--select *  
update Customer Set ServiceDepartmentCode=H.DepartmentCode
from ServiceSalesHeader H
	inner join Customer C On H.CustomerCode=C.CustomerCode
where H.DelFlag='0' and H.ServiceOrderStatus='006'
	and (C.ServiceDepartmentCode is null or RTRIM(C.ServiceDepartmentCode)='')
	and DATEDIFF(m,H.salesdate,GETDATE()) < 2
------------------
--サービス担当者--
------------------
--select *  
update Customer Set ServiceEmployeeCode=H.FrontEmployeeCode
from ServiceSalesHeader H
	inner join Customer C On H.CustomerCode=C.CustomerCode
where H.DelFlag='0' and H.ServiceOrderStatus='006'
	and ( C.ServiceEmployeeCode is null or RTRIM(C.ServiceEmployeeCode)='')
	and DATEDIFF(m,H.salesdate,GETDATE()) < 2



END
GO
