USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_CarArrivalReport]    Script Date: 2021/04/05 13:19:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- ????/??/??
-- Description:	<Description,,>
-- ì¸å…òAóçï[çÏê¨ópÇÃView
-- 2021/03/22 yano #4085Åyé‘óºç∏íËì¸óÕÅzîÉéÊé‘ÇÃì¸å…òAóçï[ÇÃécç¬ï\é¶òRÇÍëŒâû
-- 2020/03/20 #4039Åyé‘óºédì¸ÅEé‘óºç∏íËÅzì¸å…òAóçï[ÅEécç¬ã‡äzÇÃï\é¶ïsãÔçáëŒâû
-- ======================================================================================================================================
CREATE VIEW [dbo].[V_CarArrivalReport]
AS
SELECT a2.CarPurchaseId
	 , a9.CarAppraisalId
	 , a2.SupplierCode
	 , a4.SupplierName
	 , c2.CarName
	 , c1.CarGradeName
	 , a8.MorterViecleOfficialCode
	 , a8.RegistrationNumberType
	 , a8.RegistrationNumberKana
	 , a8.RegistrationNumberPlate
	 , a8.Vin
	 , a2.TotalAmount
	 , a2.CarTaxAppropriateAmount
	 , a2.TaxAmount
	 , a2.ConsumptionTaxId
	 , a2.Rate
	 , a2.RecyclePrice
	 , a2.VehiclePrice
	 , a2.VehicleAmount
	 , a2.PurchaseDate
	 , a2.EmployeeCode
	 , a5.EmployeeName
	 , CASE WHEN (a2.SlipNumber IS NOT NULL OR a2.SlipNumber <>'') THEN a2.SlipNumber ELSE a9.SlipNumber END AS SlipNumber
	 , a2.CarPurchaseType
	 , CASE WHEN a3.RequestRegistDate is null THEN c3.RequestRegistDate END AS RequestRegistDate	--Mod 2020/03/20 #4039
	 , a2.SalesCarNumber
	 , a7.OfficeName
	 , a6.FullName AS DepartmentFullName		--Add 2018/06/22 #3893
	 
	   --Mod 2021/03/22 yano  #4085
	 , --Mod 2020/03/20 #4039
	    
		CASE WHEN a9.RemainDebt IS NOT NULL THEN a9.RemainDebt
		ELSE
			CASE WHEN a8.Vin = a3.TradeInVin1 THEN isnull(a3.TradeInRemainDebt1, 0) 
				 WHEN a8.Vin = a3.TradeInVin2 THEN isnull(a3.TradeInRemainDebt2, 0) 
				 WHEN a8.Vin = a3.TradeInVin3 THEN isnull(a3.TradeInRemainDebt3, 0)
			ELSE 
				CASE 
					WHEN a8.Vin = c3.TradeInVin1 THEN isnull(c3.TradeInRemainDebt1, 0) 
					WHEN a8.Vin = c3.TradeInVin2 THEN isnull(c3.TradeInRemainDebt2, 0) 
					WHEN a8.Vin = c3.TradeInVin3 THEN isnull(c3.TradeInRemainDebt3, 0)
				ELSE
					0
				END
			END
		END AS TradeInRemainDebt
	 , a8.Mileage
	 , a8.ExteriorColorName
	 , a8.ExpireDate
	 , a8.ConfirmDriverLicense													--Add 2018/10/25 yano #3947
	 , a8.ConfirmCertificationSeal												--Add 2018/10/25 yano #3947
	 , a8.ConfirmOther															--Add 2018/10/25 yano #3947
FROM dbo.CarPurchase AS a2 
LEFT OUTER JOIN dbo.Supplier AS a4 ON a2.SupplierCode = a4.SupplierCode 
LEFT OUTER JOIN dbo.Employee AS a5 ON a2.EmployeeCode = a5.EmployeeCode 
LEFT OUTER JOIN dbo.Department AS a6 ON a2.DepartmentCode = a6.DepartmentCode
LEFT OUTER JOIN dbo.Office AS a7 ON a6.OfficeCode = a7.OfficeCode
LEFT OUTER JOIN dbo.SalesCar AS a8 ON a2.SalesCarNumber = a8.SalesCarNumber
LEFT OUTER JOIN dbo.CarAppraisal AS a9 ON a2.CarAppraisalId = a9.CarAppraisalId
LEFT OUTER JOIN dbo.CarSalesHeader AS a3 ON a9.SlipNumber = a3.SlipNumber AND a3.DelFlag = '0'
LEFT OUTER JOIN dbo.CarGrade AS c1 ON a8.CarGradeCode = c1.CarGradeCode
LEFT OUTER JOIN dbo.Car AS c2 ON c1.CarCode = c2.CarCode
--Add 2020/03/20 #4039
LEFT OUTER JOIN (
	select 
		  slipnumber
		, RequestRegistDate
		, TradeInVin1
		, TradeInVin2
		, TradeInVin3
		, TradeInRemainDebt1
		, TradeInRemainDebt2
		, TradeInRemainDebt3
	from
		dbo.CarSalesHeader
	where
		DelFlag = '0'
	) AS c3 ON a2.SlipNumber = c3.SlipNumber
GO


