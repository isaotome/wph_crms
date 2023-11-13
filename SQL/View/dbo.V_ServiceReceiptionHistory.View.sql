USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_ServiceReceiptionHistory]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create view [dbo].[V_ServiceReceiptionHistory]
as
select 
	a1.CustomerCode,
	a1.SalesCarNumber,
	'' as SlipNumber,
	ReceiptionDate,
	a2.Name as ReceiptionStatus,
	a3.Name as ReceiptionType,
	a4.Name as VisitOpportunity,
	a1.RequestDetail as RequestDetail,
	a5.EmployeeName 
from
CustomerReceiption a1
left join c_ReceiptionState a2
on a1.ReceiptionState=a2.Code
left join c_ReceiptionType a3
on a1.ReceiptionType = a3.Code
left join c_VisitOpportunity a4
on a1.VisitOpportunity=a4.Code
left join Employee a5
on a1.EmployeeCode = a5.EmployeeCode
union all

select 
	a1.CustomerCode,
	a1.SalesCarNumber,
	a1.SlipNumber,
	a1.ArrivalPlanDate as ReceiptionDate,
	a2.Name as ReceiptionStatus,
	(select top(1) LineContents from ServiceSalesLine
	where SlipNumber=a1.SlipNumber and RevisionNumber=a1.RevisionNumber
	and ServiceType='001') as ReceiptionType,
	'' as VisitOpportunity,
	'' as RequestDetail,
	a3.EmployeeName
from
ServiceSalesHeader a1
left join c_ServiceOrderStatus a2
on a1.ServiceOrderStatus=a2.Code
left join Employee a3
on a1.FrontEmployeeCode = a3.EmployeeCode
where a1.DelFlag='0'
GO
