USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_CarSummary]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_CarSummary]
as
select 
	a.SlipNumber,
	a.RevisionNumber,
	c.CompanyCode,
	c.CompanyName,
	o.OfficeCode,
	o.OfficeName,
	d.DepartmentName,
	d.DepartmentCode,
	case a.SalesOrderStatus
		when '001' then a.QuoteDate
		when '002' then a.SalesOrderDate
		when '003' then a.SalesOrderDate
		when '004' then a.SalesOrderDate
		when '005' then a.SalesDate
	end as SlipDate,
	a.SalesOrderStatus,
	a.GrandTotalAmount,
	a.CarName,
	a.CarGradeName,
	cus.CustomerName,
	emp.EmployeeName
from carsalesheader a
inner join
(
select
	slipnumber,
	MAX(revisionnumber) as RevisionNumber,
	salesorderstatus
from
	CarSalesHeader
group by SlipNumber,SalesOrderStatus
) b 
on a.SlipNumber = b.SlipNumber 
and a.RevisionNumber = b.RevisionNumber
and a.SalesOrderStatus = b.SalesOrderStatus
left join Department d on a.DepartmentCode=d.DepartmentCode
left join Office o on d.OfficeCode = o.OfficeCode
left join Company c on o.CompanyCode=c.CompanyCode
left join Customer cus on a.CustomerCode=cus.CustomerCode
left join Employee emp on a.EmployeeCode=emp.EmployeeCode
GO
