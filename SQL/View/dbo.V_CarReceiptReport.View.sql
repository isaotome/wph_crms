USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_CarReceiptReport]    Script Date: 2018/01/31 15:08:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 
-- Description:	<Description,,>
-- 車両登録依頼書
-- Mod 2018/01/30 arc yano #3696 【車＆サ】注文書類の拠点名称について　部門名を取得するように修正
-- ======================================================================================================================================
CREATE view [dbo].[V_CarReceiptReport]
as
select

-- CarSalesHeader
a1.SlipNumber,
a1.RevisionNumber,
a1.SalesCarNumber,
a1.Vin,
a1.RequestRegistDate,
case PaymentPlanType
	when 'A' then (select CustomerClaimName from Loan a join CustomerClaim b on a.CustomerClaimCode=b.CustomerClaimCode where a.LoanCode=LoanCodeA)
	when 'B' then (select CustomerClaimName from Loan a join CustomerClaim b on a.CustomerClaimCode=b.CustomerClaimCode where a.LoanCode=LoanCodeB)
	when 'C' then (select CustomerClaimName from Loan a join CustomerClaim b on a.CustomerClaimCode=b.CustomerClaimCode where a.LoanCode=LoanCodeC)
end as LoanCustomerName,
case PaymentPlanType
	when 'A' then a1.AuthorizationNumberA
	when 'B' then a1.AuthorizationNumberB
	when 'C' then a1.AuthorizationNumberC
end as AuthorizationNumber,
a1.CostTotalAmount,
a1.SalesCostTotalTaxAmount,
a1.Memo,
a1.GrandTotalAmount,
a1.DelFlag,

-- CarPurchase
a2.PurchaseDate,
a2.Amount as PurchaseAmount,

-- Office
a4.OfficeName,

-- Employee
a1.EmployeeCode,
a5.EmployeeName,

-- Customer
a6.CustomerName,

-- ReceiptPlan
a7.ReceiptPlanDate,
a7.Amount as LoanAmount,

-- Add 2018/01/30 arc yano #3696
-- Department
a3.DepartmentCode as DepartmentCode,
a3.DepartmentName as DepartmentName,
a3.FullName as DepartmentFullName

from
CarSalesHeader a1
left join 
(
select b.SlipNumber, a.PurchaseDate, a.Amount, a.CarAppraisalId, a.PurchaseStatus from CarPurchase a
inner join CarAppraisal b
on a.CarAppraisalId=b.CarAppraisalId
where a.PurchaseStatus='002'
and b.SlipNumber is not null and b.SlipNumber<>''
) a2
on a1.SlipNumber=a2.SlipNumber
left join
Department a3
on a1.DepartmentCode=a3.DepartmentCode
left join
Office a4
on a3.OfficeCode = a4.OfficeCode
left join
Employee a5
on a1.EmployeeCode = a5.EmployeeCode
left join
Customer a6
on a1.CustomerCode = a6.CustomerCode
left join
(
	select SlipNumber,JournalDate,ReceiptPlanDate,Amount from ReceiptPlan
	where ReceiptType='003' and DelFlag='0'
) a7
on a1.SlipNumber = a7.SlipNumber
where a1.DelFlag='0'





GO


