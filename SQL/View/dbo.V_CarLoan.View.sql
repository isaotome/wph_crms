USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_CarLoan]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_CarLoan]
as
select
	1 as Id,
	'A' as PlanName,
	LoanCodeA,
	SlipNumber,
	RevisionNumber,
	FirstAmountA,
	SecondAmountA,
	PaymentFrequencyA,
	BonusAmountA
from
	CarSalesHeader
union all
select
	2 as Id,
	'B' as PlanName,
	LoanCodeB,
	SlipNumber,
	RevisionNumber,
	FirstAmountB,
	SecondAmountB,
	PaymentFrequencyB,
	BonusAmountB
from
	CarSalesHeader
union all
select
	3 as Id,
	'C' as PlanName,
	LoanCodeC,
	SlipNumber,
	RevisionNumber,
	FirstAmountC,
	SecondAmountC,
	PaymentFrequencyC,
	BonusAmountC
from
	CarSalesHeader
GO
