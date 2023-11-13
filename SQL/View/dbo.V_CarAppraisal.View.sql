USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_CarAppraisal]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_CarAppraisal] as
select
a.CarAppraisalId
, a.Vin
, a.CreateDate
, b.SlipNumber
, b.SalesOrderDate
, (case when c.PurchaseStatus is null then '001' else c.PurchaseStatus end) as PurchaseStatus
, d.EmployeeName as AppraisalEmployeeName
, e.EmployeeName as OrderEmployeeName
, f.CustomerName
, (case when b.SlipNumber is null then '2' else '1' end) as SlipNumberCtrl
, a.DelFlag
from CarAppraisal a left outer join CarSalesHeader b on a.SlipNumber = b.SlipNumber and a.DelFlag = '0' and '0' = b.DelFlag
left outer join CarPurchase c on a.CarAppraisalId = c.CarAppraisalId and '0' = c.DelFlag
left outer join Employee d on a.EmployeeCode = d.EmployeeCode
left outer join Employee e on b.EmployeeCode = e.EmployeeCode
left outer join Customer f on b.CustomerCode = f.CustomerCode
GO
