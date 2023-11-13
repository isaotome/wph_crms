USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_CarPurchaseList]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_CarPurchaseList]
as
select
	a.RecordType
	,a.SalesCarNumber
	,a.PurchasePlanDate
	,a.PurchaseDate
	,a.SlipDate
	,m.MakerName
	,c.CarName
	,a.EmployeeCode
	,e.EmployeeName
	,a.PurchaseStatus
	,a.PurchaseStatusName
	,s.Vin
	,d.DepartmentCode
	,d.DepartmentName
	,n.Name as NewUsedType
	,a.SupplierCode
	,a.SupplierName
from
(
	select
		'仕入' as RecordType
		,p.SalesCarNumber
		,p.EmployeeCode
		,p.PurchaseDate as PurchasePlanDate
		,case 
			when p.PurchaseStatus <> '002' then null
			else p.PurchaseDate
		end as PurchaseDate
		,p.SlipDate
		,p.PurchaseStatus
		,case
			when p.PurchaseStatus = '001' then '未仕入'
			when p.PurchaseStatus = '002' then '仕入済'
		end as PurchaseStatusName
		,p.DepartmentCode
		,p.SupplierCode
		,s.SupplierName
	from
		CarPurchase p 
		left join Supplier s on p.SupplierCode = s.SupplierCode
	union all
	select
		'移動' as RecordType
		,t.SalesCarNumber
		,t.ArrivalEmployeeCode
		,t.ArrivalPlanDate
		,t.ArrivalDate
		,null as PurchaseDate
		,case
			when t.ArrivalDate is not null then '002'
			else '001'
		end as ArrivalStatus
		,case
			when t.ArrivalDate is not null then '入庫済'
			else '未入庫'
		end as ArrivalStatusName
		,l.DepartmentCode
		,null
		,d.LocationName
	from 
		Transfer t
		left join Location l on t.ArrivalLocationCode = l.LocationCode
		left join Location d on t.DepartureLocationCode = d.LocationCode
	where
		t.SalesCarNumber is not null and t.SalesCarNumber <> ''
) a
	left join SalesCar s on a.SalesCarNumber = s.SalesCarNumber
	left join CarGrade g on s.CarGradeCode = g.CarGradeCode
	left join Car c on g.CarCode = c.CarCode
	left join Brand b on c.CarBrandCode = b.CarBrandCode
	left join Maker m on b.MakerCode = m.MakerCode
	left join Employee e on a.EmployeeCode = e.EmployeeCode
	left join Department d on a.DepartmentCode = d.DepartmentCode
	left join c_NewUsedType n on s.NewUsedType = n.Code
GO
