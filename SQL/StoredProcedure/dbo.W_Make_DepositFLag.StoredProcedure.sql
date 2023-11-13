USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_Make_DepositFLag]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[W_Make_DepositFLag]
AS
BEGIN

update ReceiptPlan
set DepositFlag='1'
where ReceiptPlanId in (
Select B.ReceiptPlanId
from (
select
 SlipNumber,
 Case when CarLiabilityInsurance Is null then 0 else CarLiabilityInsurance end AS D1,
 Case when CarWeightTax Is null then 0 else CarWeightTax end AS D2,
 Case when FiscalStampCost Is null then 0 else FiscalStampCost end AS D3 
from ServiceSalesHeader where DelFlag='0'
and (
CarLiabilityInsurance is not null or
CarWeightTax is not null or
FiscalStampCost is not null )) A 
inner join 
(select ReceiptPlanId,SlipNumber,Amount,ReceivableBalance,DepositFlag from ReceiptPlan
where DelFlag='0' and CompleteFlag <>'1' and DepositFlag is null and DepartmentCode<>'042') B
on A.SlipNumber =B.SlipNumber
where B.Amount = B.ReceivableBalance
and (
A.D1=B.Amount or A.D2=B.Amount or A.D3=B.Amount or 
A.D1+A.D2 = B.Amount or A.D2+A.D3 = B.Amount or A.D1+A.D3 = B.Amount or 
A.D1+A.D2+A.D3 = B.Amount
)
)


update ReceiptPlan
set DepositFlag='1'
where ReceiptPlanId in (
Select B.ReceiptPlanId
from (
select
 SlipNumber,
 Case when CarLiabilityInsurance Is null then 0 else CarLiabilityInsurance end AS D1,
 Case when CarWeightTax Is null then 0 else CarWeightTax end AS D2,
 Case when FiscalStampCost Is null then 0 else FiscalStampCost end AS D3 
from ServiceSalesHeader where DelFlag='0'
and (
CarLiabilityInsurance is not null or
CarWeightTax is not null or
FiscalStampCost is not null )) A 
inner join 
(select ReceiptPlanId,SlipNumber,Amount,ReceivableBalance,DepositFlag from ReceiptPlan
where DelFlag='0' and CompleteFlag <>'1' and DepositFlag is null and DepartmentCode<>'042') B
on A.SlipNumber =B.SlipNumber
inner join ( 
select slipnumber,min(amount) as amount from ReceiptPlan 
where DelFlag='0' and CompleteFlag <>'1' and DepositFlag is null
group by SlipNumber) C on B.SlipNumber=c.SlipNumber and B.Amount=c.amount
where B.Amount = B.ReceivableBalance
and A.D1+A.D2+A.D3 < B.Amount

)
--入金予定が一個しなければね、いっかな
update ReceiptPlan
Set DepositFlag ='1'
From ReceiptPlan R
inner join (select SlipNumber,COUNT(*) as DataCnt from ReceiptPlan where DelFlag='0' and CompleteFlag='0' and DepartmentCode<>'042' group by SlipNumber) A
on A.SlipNumber= R.SlipNumber
where A.DataCnt ='1' and DelFlag='0' and DepositFlag is null and DepartmentCode<>'042'

END
GO
