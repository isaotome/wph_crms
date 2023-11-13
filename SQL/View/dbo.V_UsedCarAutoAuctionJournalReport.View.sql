USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_ServiceSalesReportSum]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_ServiceSalesReportSum]
as
select
	SlipNumber,
	RevisionNumber,
	CustomerClaimCode,
	sum(isnull(TaxFreeAmount,0)+isnull(TaxFreeTechnicalAmount,0)) as TaxFreeAmount,
	sum(isnull(taxationamount,0)+isnull(TaxationTechnicalAmount,0)) as TaxationAmount,
	-- 2014/03/03 消費税対応
	--floor(sum(isnull(taxationamount,0)+isnull(TaxationTechnicalAmount,0))*0.05) as TaxAmount,
	--floor(sum(isnull(taxationamount,0)+isnull(TaxationTechnicalAmount,0))*1.05) + sum(isnull(TaxFreeAmount,0)+isnull(TaxFreeTechnicalAmount,0)) as TotalAmount,
	floor(sum(isnull(taxationamount,0)+isnull(TaxationTechnicalAmount,0))*(cast(Rate AS NUMERIC(18,4))/100)) as TaxAmount,
	floor(sum(isnull(taxationamount,0)+isnull(TaxationTechnicalAmount,0))*(cast(100 + Rate AS NUMERIC(18,4))/100)) + sum(isnull(TaxFreeAmount,0)+isnull(TaxFreeTechnicalAmount,0)) as TotalAmount,
	sum(TechnicalFeeAmount) as TechnicalAmount,
	sum(Amount) as PartsAmount
from
(
	select
	slipnumber,
	revisionnumber,
	isnull(customerclaimcode,'') as customerclaimcode,
	case Classification1
		when '002' then TechnicalFeeAmount
		else 0
	end as TaxFreeTechnicalAmount,
	case Classification1
		when '002' then Amount
		else 0
	end as TaxFreeAmount,
	case Classification1 
		when '002' then 0
		else isnull(Amount,0)
	end as TaxationAmount,
	case Classification1
		when '002' then 0
		else isnull(TechnicalFeeAmount,0)
	end as TaxationTechnicalAmount,
	TechnicalFeeAmount,
	Amount,
	Rate
	from
	servicesalesline
) a
group by slipnumber,revisionnumber,customerclaimcode,Rate
GO
