USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_PartsPurchaseOrderReport]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_PartsPurchaseOrderReport]
as
select
	l.SlipNumber,
	l.RevisionNumber,
	l.LineNumber,
	l.LineContents,
	l.PartsNumber,
	isnull(l.Quantity,0) as Quantity,
	e.EmployeeName
from
servicesalesline l 
left join servicesalesheader h
on l.slipnumber=h.slipnumber and l.revisionnumber=h.revisionnumber
left join parts p
on l.partsnumber=p.partsnumber
left join Employee e
on h.FrontEmployeeCode = e.EmployeeCode
where p.partsnumber is null
and servicetype='003'
GO
