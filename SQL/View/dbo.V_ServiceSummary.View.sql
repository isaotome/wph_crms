USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_ServiceSummary]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_ServiceSummary]
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
	a.QuoteDate,
	a.ServiceOrderStatus,
	case a.ServiceOrderStatus
		when '001' then a.QuoteDate
		when '002' then a.SalesOrderDate
		when '005' then a.SalesDate
	end as SlipDate,
	a.GrandTotalAmount,
	a.CarName,
	a.CarGradeName,
	cus.CustomerName,
	emp.EmployeeName
from ServiceSalesHeader a
inner join
(
select
	SlipNumber,
	MAX(RevisionNumber) as RevisionNumber,
	ServiceOrderStatus
from
	ServiceSalesHeader
where ServiceOrderStatus in ('001','002','005')
group by SlipNumber,ServiceOrderStatus
) b 
on a.SlipNumber = b.SlipNumber 
and a.RevisionNumber = b.RevisionNumber
and a.ServiceOrderStatus = b.ServiceOrderStatus
left join Department d on a.DepartmentCode=d.DepartmentCode
left join Office o on d.OfficeCode = o.OfficeCode
left join Company c on o.CompanyCode=c.CompanyCode
left join Customer cus on a.CustomerCode=cus.CustomerCode
left join Employee emp on a.FrontEmployeeCode=emp.EmployeeCode
GO
