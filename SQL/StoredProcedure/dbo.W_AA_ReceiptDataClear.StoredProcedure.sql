USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_AA_ReceiptDataClear]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[W_AA_ReceiptDataClear]
AS
BEGIN

--AAの入金実績を自動作成する
insert into Journal 
(JournalId,JournalType,DepartmentCode,CustomerClaimCode,SlipNumber,JournalDate,AccountType,AccountCode,Amount,Summary,CreateEmployeeCode,CreateDate,LastUpdateEmployeeCode,LastUpdateDate,DelFlag,OfficeCode,CashAccountCode)
select
	NEWID(),'001',R.DepartmentCode,C.CustomerClaimCode,R.SlipNumber,
	Case when R.ReceiptPlanDate IS null then R.CreateDate else R.ReceiptPlanDate end ,
	'006',R.AccountCode,R.ReceivableBalance,
	'（AUTO）AA入金自動消込','kamachi.akira',GETDATE(),'kamachi.akira',GETDATE(),'0',O.OfficeCode,'001'
from ReceiptPlan R 
	inner join CustomerClaim C on R.CustomerClaimCode=C.CustomerClaimCode
	inner join Department D on R.DepartmentCode=D.DepartmentCode
	inner join Office O on D.OfficeCode=O.OfficeCode
	inner join (
		select SlipNumber From CarSalesHeader where DelFlag='0' and SalesOrderStatus in ('004','005')
		union all
		select SlipNumber From ServiceSalesHeader where  DelFlag='0' and ServiceOrderStatus in ('006')
		) A on R.SlipNumber=A.SlipNumber
where C.CustomerClaimType='201'
	and R.DelFlag='0' and R.CompleteFlag='0' 
	and R.ReceivableBalance<>0
--社内売掛の削除
insert into Journal 
(JournalId,JournalType,DepartmentCode,CustomerClaimCode,SlipNumber,JournalDate,AccountType,AccountCode,Amount,Summary,CreateEmployeeCode,CreateDate,LastUpdateEmployeeCode,LastUpdateDate,DelFlag,OfficeCode,CashAccountCode)
select
	NEWID(),'001',R.DepartmentCode,C.CustomerClaimCode,R.SlipNumber,
	Case when R.ReceiptPlanDate IS null then R.CreateDate else R.ReceiptPlanDate end ,
	'008',R.AccountCode,R.ReceivableBalance,
	'（AUTO）社内入金自動消込','kamachi.akira',GETDATE(),'kamachi.akira',GETDATE(),'0',O.OfficeCode,'001'
from ReceiptPlan R 
	inner join CustomerClaim C on R.CustomerClaimCode=C.CustomerClaimCode
	inner join Department D on R.DepartmentCode=D.DepartmentCode
	inner join Office O on D.OfficeCode=O.OfficeCode
	inner join (
		select SlipNumber From CarSalesHeader where DelFlag='0' and SalesOrderStatus in ('004','005')
		union all
		select SlipNumber From ServiceSalesHeader where  DelFlag='0' and ServiceOrderStatus in ('006')
		) A on R.SlipNumber=A.SlipNumber
where C.CustomerClaimType='005'
	and R.DelFlag='0' and R.CompleteFlag='0' 
	and R.ReceivableBalance<>0

END
GO
