USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_CarTradeIn]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_CarTradeIn]
as
select
	1 as Id,
	SlipNumber,
	RevisionNumber,
	TradeInMakerName1,
	TradeInCarName1,
	TradeInVin1,
	TradeInInspectionExpiredDate1,
	TradeInMileage1,
	c_MileageUnit.Name AS TradeInMileageUnit1,
	TradeInUnexpiredCarTax1,
	TradeInRemainDebt1,
	TradeInModelSpecificateNumber1,
	TradeInRegistrationNumber1,
	TradeInUnexpiredLiabilityInsurance1
from
	CarSalesHeader left join c_MileageUnit
	on CarSalesHeader.MileageUnit=c_MileageUnit.Code
union all
select
	2 as Id,
	SlipNumber,
	RevisionNumber,
	TradeInMakerName2,
	TradeInCarName2,
	TradeInVin2,
	TradeInInspectionExpiredDate2,
	TradeInMileage2,
	c_MileageUnit.Name AS TradeInMileageUnit2,
	TradeInUnexpiredCarTax2,
	TradeInRemainDebt2,
	TradeInModelSpecificateNumber2,
	TradeInRegistrationNumber2,
	TradeInUnexpiredLiabilityInsurance2
from
	CarSalesHeader left join c_MileageUnit
	on CarSalesHeader.MileageUnit=c_MileageUnit.Code
union all
select
	3 as Id,
	SlipNumber,
	RevisionNumber,
	TradeInMakerName3,
	TradeInCarName3,
	TradeInVin3,
	TradeInInspectionExpiredDate3,
	TradeInMileage3,
	c_MileageUnit.Name AS TradeInMileageUnit3,
	TradeInUnexpiredCarTax3,
	TradeInRemainDebt3,
	TradeInModelSpecificateNumber3,
		TradeInRegistrationNumber3,
	TradeInUnexpiredLiabilityInsurance3
from
	CarSalesHeader left join c_MileageUnit
	on CarSalesHeader.MileageUnit=c_MileageUnit.Code
GO
