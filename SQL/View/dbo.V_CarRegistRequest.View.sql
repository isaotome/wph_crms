USE [WPH_DB_CHECK]
GO

/****** Object:  View [dbo].[V_CarRegistRequest]    Script Date: 2021/08/18 12:34:55 ******/
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
-- Mod 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応）
-- Mod 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
-- Mod 2018/01/30 arc yano #3696 【車＆サ】注文書類の拠点名称について
-- ======================================================================================================================================
CREATE VIEW [dbo].[V_CarRegistRequest]
as
select
--CarSalesHeader
	a1.SlipNumber,
	a1.RevisionNumber,
	a1.SalesPlanDate,
	a1.MakerName,
	a1.CarBrandName,
	a1.CarName,
	a1.CarGradeName,
	a1.RequestRegistDate,
	a1.RequestPlateNumber,
	a1.MorterViecleOfficialCode AS RequestMorterViecleOfficialCode,
	a1.Vin,
	GETDATE() AS RequestDate,
	a1.SealSubmitDate,
	a1.ProxySubmitDate,
	a1.ParkingSpaceSubmitDate,
	a1.OwnershipReservationSubmitDate,
	a1.CarLiabilityInsuranceSubmitDate,
	a1.Memo,
	a1.PrincipalPlace,
	a1.CarTax,
	a1.CarWeightTax,
	a1.AcquisitionTax,
	a1.TradeInFiscalStampCost,
	a1.CarLiabilityInsurance,
	a1.RequestNumberCost,
	a1.TaxFreeTotalAmount,
	--a1.TaxationAmount,
	a1.InspectionRegistCost,
	a1.NumberPlateCost,
	a1.NewUsedType,		--Add 2021/08/02 yano #4097
	case 
		when a1.NewUsedType = 'N' then (isnull(a1.SalesPrice,0) + isnull(a1.MakerOptionAmount,0)) * 0.9 
		else null
	end as TaxationAmount,
	a1.Rate AS CarSalesOrderRate,		--Add Mod 2019/09/04 yano #4011
	
--Office
	a3.OfficeName,

--SalesCar
	a4.SalesCarNumber,
	a4.MorterViecleOfficialCode,
	a4.RegistrationNumberType,
	a4.RegistrationNumberKana,
	a4.RegistrationNumberPlate,
	a4.ManufacturingYear,			--Add 2021/08/02 yano #4097
	
--Employee
	a6.EmployeeName,

--c_RegistrationType
	a7.Name as RegistrationType,

--c_OwnershipReservation
	a8.Name as OwnershipReservation,

--c_CarLiabilityInsuranceType
	a9.Name as CarLiabilityInsuranceType,

--CarGrade
	a10.ModelYear,

--CarSalesPayment
	(select MAX(PaymentPlanDate) from CarSalesPayment where SlipNumber=a1.SlipNumber and RevisionNumber=a1.RevisionNumber) as ReceiptPlanDate,

--Customer(Possesor)
	isnull(a11.CustomerName,'') as PossesorName,
	a11.CustomerNameKana as PossesorNameKana,
	isnull(a11.Prefecture,'') as PossesorPrefecture,
	isnull(a11.City,'') as PossesorCity,
	isnull(a11.Address1,'') as PossesorAddress1,
	isnull(a11.Address2,'') as PossesorAddress2,

--Customer(User)
	isnull(a12.CustomerName,'') as UserName,
	a12.CustomerNameKana as UserNameKana,
	isnull(a12.Prefecture,'') as UserPrefecture,
	isnull(a12.City,'') as UserCity,
	isnull(a12.Address1,'') as UserAddress1,
	isnull(a12.Address2,'') as UserAddress2,

--Add 2018/01/30 arc yano #3696
--Department
	a2.DepartmentCode as DepartmentCode,
	a2.DepartmentName as DepartmentName,
	a2.FullName as DepartmentFullName
	
from
	CarSalesHeader a1
	left join Department a2 on a1.DepartmentCode=a2.DepartmentCode
	left join Office a3 on a2.OfficeCode=a3.OfficeCode
	left join SalesCar a4 on a1.SalesCarNumber=a4.SalesCarNumber
	left join Customer a5 on a1.CustomerCode=a5.CustomerCode
	left join Employee a6 on a1.EmployeeCode=a6.EmployeeCode
	left join c_RegistrationType a7 on a1.RegistrationType=a7.Code
	left join c_OwnershipReservation a8 on a1.OwnershipReservation=a8.Code
	left join c_CarLiabilityInsuranceType a9 on a1.CarLiabilityInsuranceType=a9.Code
	left join CarGrade a10 on a1.CarGradeCode=a10.CarGradeCode
	left join Customer a11 on a1.PossesorCode=a11.CustomerCode
	left join Customer a12 on a1.UserCode=a12.CustomerCode

GO


