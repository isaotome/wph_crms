USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_CarSalesReport]    Script Date: 2023/09/28 8:26:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 
-- Description:	<Description,,>
-- Mod 2023/09/05 yano #4162 ÉCÉìÉ{ÉCÉXëŒâû
-- Mod 2023/08/15 yano #4176 îÃîÑèîîÔópÇÃèCê≥
-- Mod 2022/06/23 yano #4140Åyé‘óºì`ï[ì¸óÕÅzíçï∂èëÇÃìoò^ñºã`êlÇ™ï\é¶Ç≥ÇÍÇ»Ç¢ïsãÔçáÇÃëŒâû
-- Mod 2021/08/02 yano #4097ÅyÉOÉåÅ[ÉhÉ}ÉXÉ^ì¸óÕÅzîNéÆÇÃï€ë∂ÇÃägí£ã@î\ÅiÉNÉIÅ[É^Å[ëŒâûÅj
-- Mod 2020/08/31 yano #4066 Åyé‘óºì`ï[Åzè¡îÔê≈ÇOÅìÇê›íËÇ∑ÇÈÇ∆îÃîÑèîîÔópÇ™í†ï[Ç…ï\é¶Ç≥ÇÍÇ»Ç¢ÅB
-- Mod 2019/09/04 yano #4011 è¡îÔê≈ÅAé©ìÆé‘ê≈ÅAé©ìÆé‘éÊìæê≈ïœçXÇ…î∫Ç§â¸èCçÏã∆
-- Mod 2018/01/30 arc yano #3696 Åyé‘ÅïÉTÅzíçï∂èëóﬁÇÃãíì_ñºèÃÇ…Ç¬Ç¢Çƒ
-- ======================================================================================================================================
CREATE VIEW [dbo].[V_CarSalesReport]
AS
SELECT  a1.SlipNumber
	  , a1.RevisionNumber
	  , a1.QuoteDate
	  , a1.QuoteExpireDate
	  , a1.SalesOrderDate
	  , a1.MakerName
	  , a1.CarBrandName
	  , a1.CarName
	  , a1.CarGradeName
	  , a1.ModelName
	  , a1.Vin
	  , a1.Mileage
	  , ISNULL(a1.SalesPrice, 0) + ISNULL(a1.SalesTax, 0) AS SalesPrice
	  , ISNULL(a1.SalesPrice, 0) + ISNULL(a1.SalesTax, 0) - ISNULL(a1.DiscountAmount, 0) - ISNULL(a1.DiscountTax, 0) + ISNULL(a1.MakerOptionAmount, 0) + ISNULL(a1.MakerOptionTaxAmount, 0) AS TaxationAmount
	  , ISNULL(a1.DiscountAmount, 0) + ISNULL(a1.DiscountTax, 0) AS DiscountAmount
	  , ISNULL(a1.SalesTax, 0) AS TaxAmount															--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.ShopOptionAmount, 0) + ISNULL(a1.ShopOptionTaxAmount, 0) AS ShopOptionAmount		--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.MakerOptionAmount, 0) + ISNULL(a1.MakerOptionTaxAmount, 0) AS MakerOptionAmount	--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.OutSourceAmount, 0) + ISNULL(a1.OutSourceTaxAmount, 0) AS OutSourceAmount			--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.OutSourceTaxAmount, 0) AS OutSourceTaxAmount		--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.ShopOptionTaxAmount, 0) AS ShopOptionTaxAmount	--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.MakerOptionTaxAmount, 0) AS MakerOptionTaxAmount	--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.ShopOptionAmount, 0) + ISNULL(a1.ShopOptionTaxAmount, 0) + ISNULL(a1.MakerOptionAmount, 0) + ISNULL(a1.MakerOptionTaxAmount, 0) AS OptionTotalAmount	--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.SalesPrice, 0) + ISNULL(a1.SalesTax, 0) - ISNULL(a1.DiscountAmount, 0) - ISNULL(a1.DiscountTax, 0) + a1.ShopOptionAmount + a1.ShopOptionTaxAmount + a1.MakerOptionAmount + a1.MakerOptionTaxAmount AS SubTotalAmount
	  , ISNULL(a1.CarTax, 0) AS CarTax									--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.CarLiabilityInsurance, 0)	AS CarLiabilityInsurance	--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.CarWeightTax, 0)	AS CarWeightTax						--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.AcquisitionTax, 0) AS AcquisitionTax					--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.InspectionRegistCost, 0) AS InspectionRegistCost		--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.ParkingSpaceCost, 0)	AS ParkingSpaceCost				--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.TradeInCost, 0) AS TradeInCost						--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.RecycleDeposit, 0) AS RecycleDeposit					--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.RecycleDepositTradeIn, 0) AS RecycleDepositTradeIn	--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.NumberPlateCost, 0) AS NumberPlateCost				--Mod 2020/08/31 yano #4066
	  , a1.TaxFreeFieldName
	  , a1.TaxFreeFieldValue
	  , ISNULL(a1.TaxFreeTotalAmount, 0) AS TaxFreeTotalAmount			--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.InspectionRegistFee, 0) + ISNULL(a1.InspectionRegistFeeTax, 0) AS InspectionRegistFee						-- Mod 2020/08/31 yano #4066
	  , ISNULL(a1.ParkingSpaceFee, 0) + ISNULL(a1.ParkingSpaceFeeTax, 0) AS ParkingSpaceFee									-- Mod 2020/08/31 yano #4066
	  , ISNULL(a1.TradeInFee, 0) + ISNULL(a1.TradeInFeeTax, 0) AS TradeInFee												-- Mod 2020/08/31 yano #4066
	  , ISNULL(a1.PreparationFee, 0) + ISNULL(a1.PreparationFeeTax, 0) AS PreparationFee									-- Mod 2020/08/31 yano #4066
	  , ISNULL(a1.RecycleControlFee, 0) + ISNULL(a1.RecycleControlFeeTax, 0) AS RecycleControlFee							-- Mod 2020/08/31 yano #4066
	  , ISNULL(a1.RecycleControlFeeTradeIn, 0) + ISNULL(a1.RecycleControlFeeTradeInTax, 0) AS RecycleControlFeeTradeIn		-- Mod 2020/08/31 yano #4066
	  , ISNULL(a1.RequestNumberFee, 0) + ISNULL(a1.RequestNumberFeeTax, 0) AS RequestNumberFee								-- Mod 2020/08/31 yano #4066
	  , ISNULL(a1.CarTaxUnexpiredAmount, 0) + ISNULL(a1.CarTaxUnexpiredAmountTax, 0) AS CarTaxUnexpiredAmount				-- Mod 2020/08/31 yano #4066
	  , ISNULL(a1.CarLiabilityInsuranceUnexpiredAmount, 0) + ISNULL(a1.CarLiabilityInsuranceUnexpiredAmountTax, 0) AS CarLiabilityInsuranceUnexpiredAmount-- Mod 2020/08/31 yano #4066
	  , a1.TaxationFieldName
	  , ISNULL(a1.TaxationFieldValue, 0) + ISNULL(a1.TaxationFieldValueTax, 0) AS TaxationFieldValue	-- Mod 2020/08/31 yano #4066
	  , a1.SalesCostTotalAmount + a1.SalesCostTotalTaxAmount AS SalesCostTotalAmount
	  , a1.SalesCostTotalTaxAmount
	  , a1.OtherCostTotalAmount
	  , a1.CostTotalAmount + a1.SalesCostTotalTaxAmount AS CostTotalAmount
	  , a1.TotalTaxAmount
	  , a1.GrandTotalAmount
	  , ISNULL(a1.VoluntaryInsuranceAmount, 0) AS VoluntaryInsuranceAmount	-- Mod 2020/08/31 yano #4066
	  , a1.PaymentCashTotalAmount
	  , a1.RequestNumberCost
	  , a1.TradeInFiscalStampCost
	  , ISNULL(a1.OutJurisdictionRegistFee, 0) + ISNULL(a1.OutJurisdictionRegistFeeTax, 0) AS OutJurisdictionRegistFee		-- Mod 2023/08/15 yano #4176
	  , ISNULL(a1.TradeInAppraisalFee, 0) + ISNULL(a1.TradeInAppraisalFeeTax, 0) AS TradeInAppraisalFee						-- Mod 2020/08/31 yano #4066
	  , ISNULL(a1.FarRegistFee, 0) + ISNULL(a1.FarRegistFeeTax, 0) AS FarRegistFee											-- Mod 2020/08/31 yano #4066
	  , ISNULL(a1.TradeInMaintenanceFee, 0) + ISNULL(a1.TradeInMaintenanceFeeTax, 0) AS TradeInMaintenanceFee				-- Mod 2020/08/31 yano #4066
	  , ISNULL(a1.InheritedInsuranceFee, 0) + ISNULL(a1.InheritedInsuranceFeeTax, 0) AS InheritedInsuranceFee				-- Mod 2020/08/31 yano #4066
	  , ISNULL(a1.TradeInAmount1, 0) + ISNULL(a1.TradeInAmount2, 0) + ISNULL(a1.TradeInAmount3, 0) AS TradeInAmount
	  , ISNULL(a1.TradeInTax1, 0) + ISNULL(a1.TradeInTax2, 0) + ISNULL(a1.TradeInTax3, 0) AS TradeInTax
	  , ISNULL(a1.TradeInUnexpiredCarTax1, 0) + ISNULL(a1.TradeInUnexpiredCarTax2, 0) + ISNULL(a1.TradeInUnexpiredCarTax3, 0) AS TradeInUnexpiredCarTax
	  , ISNULL(a1.TradeInRemainDebt1, 0) + ISNULL(a1.TradeInRemainDebt2, 0) + ISNULL(a1.TradeInRemainDebt3, 0) AS TradeInRemainDebt
	  , ISNULL(a1.TradeInAppropriation1, 0) + ISNULL(a1.TradeInAppropriation2, 0) + ISNULL(a1.TradeInAppropriation3, 0) AS TradeInAppropriation
	  , ISNULL(a1.TradeInRecycleAmount1, 0) + ISNULL(a1.TradeInRecycleAmount2, 0) + ISNULL(a1.TradeInRecycleAmount3, 0) AS TradeInRecycleAmount
	  , a1.LoanFeeAmount
	  , a1.FirstAmountA
	  , a1.SecondAmountA
	  , a1.PaymentFrequencyA
	  , a1.PaymentSecondFrequencyA
	  , a1.BonusAmountA
	  , a1.FirstAmountB
	  , a1.SecondAmountB
	  , a1.PaymentFrequencyB
	  , a1.PaymentSecondFrequencyB
	  , a1.BonusAmountB
	  , a1.FirstAmountC
	  , a1.SecondAmountC
	  , a1.PaymentFrequencyC
	  , a1.PaymentSecondFrequencyC
	  , a1.BonusAmountC
	  , CASE PaymentPlanType WHEN '' THEN 'åªã‡' WHEN NULL THEN 'åªã‡' ELSE 'ÉçÅ[Éì' END AS PaymentType
	  , a1.VoluntaryInsuranceCompanyName
	  , a1.VoluntaryInsuranceTermTo
	  , a1.DelFlag
	  , a1.ExteriorColorName
	  , a1.InteriorColorName
	  , a1.CustomerCode
	  , a1.UserCode
	  , a1.ConsumptionTaxId
	  , a1.Rate
	  , a1.RequestRegistDate	--Mod 2019/09/04 yano #4011
	  , a2.DepartmentName
	  , a2.PostCode AS DepartmentPostCode
	  , a2.Prefecture AS DepartmentPrefecture
	  , a2.City AS DepartmentCity
	  , a2.Address1 AS DepartmentAddress1
	  , a2.Address2 AS DepartmentAddress2
	  , a2.TelNumber1 AS DepartmentTelNumber1
	  , a2.TelNumber2 AS DepartmentTelNumber2
	  , a2.FaxNumber AS DepartmentFaxNumber
	  , a2.FullName AS DepartmentFullName
	  , a3.OfficeName, a3.FullName AS OfficeFullName
	  , a3.TelNumber1 AS OfficeTelNumber1
	  , a3.TelNumber2 AS OfficeTelNumber2
	  , a4.CompanyName
	  , a4.PostCode AS CompanyPostCode
	  , a4.Prefecture + a4.City + a4.Address1 + a4.Address2 AS CompanyAddress
	  , a4.TelNumber1 AS CompanyTelNumber
	  , a21.EmployeeName AS PresidentName
	  , a5.EmployeeName
	  , a6.Door
	  , a6.Displacement
	  , a6.Fuel
	  , a6.ModelYear			--Add 2021/08/02 yano #4097
	  , a8.CustomerName
	  , a8.CustomerNameKana
	  , a8.CustomerType
	  , a8.FirstName
	  , a8.LastName
	  , a8.Birthday AS CustomerBirthday
	  , a8.PostCode AS CustomerPostCode
	  , a8.Prefecture AS CustomerPrefecture
	  , a8.City AS CustomerCity
	  , a8.Address1 AS CustomerAddress1
	  , a8.Address2 AS CustomerAddress2
	  , a8.TelNumber AS CustomerTelNumber
	  , a8.MobileNumber AS CustomerMobileNumber
	  , a8.FaxNumber AS CustomerFaxNumber
	  , a8.MailAddress AS CustomerMailAddress
	  , a8.Sex
	  , a8.WorkingCompanyName
	  , a8.WorkingCompanyAddress
	  , a8.WorkingCompanyTelNumber
	  , a8.PositionName
	  , a8.CorporationType
	  , a1.NewUsedType AS NewUsedTypeCode
	  , a9.Name AS NewUsedType
	  , a10.Name AS TransMission
	  , a11.Name AS CustomerSex
	  , a12.MorterViecleOfficialCode
	  , a12.RegistrationNumberType
	  , a12.RegistrationNumberKana
	  , a12.RegistrationNumberPlate
	  , a12.SalesCarNumber
	  , a12.ExpireDate AS InspectionExpireDate
	  , a12.ManufacturingYear
	  , a22.Name AS Steering
	  , CASE WHEN a1.Mileage IS NULL OR a1.Mileage = 0 THEN NULL ELSE a14.Name END AS MileageUnitName
	  , a15.Name AS RegistrationType
	  , a16.Name AS VoluntaryInsuranceType
	  , a17.LoanName
	  , a18.CustomerClaimName AS LoanCompanyName
	  , a23.CustomerName AS UserName
	  , a23.PostCode AS UserPostCode
	  , a23.Prefecture AS UserPrefecture
	  , a23.City AS UserCity
	  , a23.Address1 AS UserAddress1
	  , a23.Address2 AS UserAddress2
	  , CASE a1.PaymentPlanType WHEN 'A' THEN LoanPrincipalA WHEN 'B' THEN LoanPrincipalB WHEN 'C' THEN LoanPrincipalC END AS LoanPrincipalAmount
	  , CASE a1.PaymentPlanType WHEN 'A' THEN LoanPrincipalA + ISNULL(LoanFeeA, 0) WHEN 'B' THEN LoanPrincipalB + ISNULL(LoanFeeB, 0) WHEN 'C' THEN LoanPrincipalC + ISNULL(LoanFeeC, 0) END AS LoanTotalAmount
	  , CASE a1.PaymentPlanType WHEN 'A' THEN PaymentTermFromA WHEN 'B' THEN PaymentTermFromB WHEN 'C' THEN PaymentTermFromC ELSE NULL END AS PaymentTermFrom
	  , CASE a1.PaymentPlanType WHEN 'A' THEN PaymentTermToA WHEN 'B' THEN PaymentTermToB WHEN 'C' THEN PaymentTermToC ELSE NULL END AS PaymentTermTo
	  , CASE a1.PaymentPlanType WHEN 'A' THEN PaymentFrequencyA WHEN 'B' THEN PaymentFrequencyB WHEN 'C' THEN PaymentFrequencyC ELSE NULL END AS PaymentFrequency
	  , CASE a1.PaymentPlanType WHEN 'A' THEN PaymentSecondFrequencyA WHEN 'B' THEN PaymentSecondFrequencyB WHEN 'C' THEN PaymentSecondFrequencyC ELSE NULL END AS PaymentFrequency2
	  , CASE a1.PaymentPlanType WHEN 'A' THEN FirstAmountA WHEN 'B' THEN FirstAmountB WHEN 'C' THEN FirstAmountC ELSE NULL END AS FirstAmount
	  , CASE a1.PaymentPlanType WHEN 'A' THEN SecondAmountA WHEN 'B' THEN SecondAmountB WHEN 'C' THEN SecondAmountC ELSE NULL END AS SecondAmount
	  , CASE a1.PaymentPlanType WHEN 'A' THEN BonusMonthA1 WHEN 'B' THEN BonusMonthB1 WHEN 'C' THEN BonusMonthC1 ELSE NULL END AS BonusMonth1
	  , CASE a1.PaymentPlanType WHEN 'A' THEN BonusMonthA2 WHEN 'B' THEN BonusMonthB2 WHEN 'C' THEN BonusMonthC2 ELSE NULL END AS BonusMonth2
	  , CASE a1.PaymentPlanType WHEN 'A' THEN BonusAmountA WHEN 'B' THEN BonusAmountB WHEN 'C' THEN BonusAmountC ELSE NULL END AS BonusAmount
	  , CASE a1.PaymentPlanType WHEN 'A' THEN RemainAmountA WHEN 'B' THEN RemainAmountB WHEN 'C' THEN RemainAmountC ELSE NULL END AS RemainAmount
	  , CASE a1.PaymentPlanType WHEN 'A' THEN LoanRateA WHEN 'B' THEN LoanRateB WHEN 'C' THEN LoanRateC END AS LoanRate
	  , a1.TradeInMakerName1
	  , a1.TradeInCarName1
	  , a1.TradeInManufacturingYear1
	  , a1.TradeInModelSpecificateNumber1
	  , a1.TradeInRegistrationNumber1
	  , a1.TradeInInspectionExpiredDate1
	  , CASE WHEN a1.TradeInMileage1 = 0 THEN NULL ELSE a1.TradeInMileage1 END AS TradeInMileage1
	  , CASE WHEN a1.TradeInMileage1 IS NULL THEN NULL WHEN a1.TradeInMileage1 = 0 THEN NULL ELSE a19.Name END AS TradeInMileageUnit1
	  , a1.TradeInVin1
	  , CASE WHEN a1.TradeInVin1 <> '' THEN 1 ELSE 0 END + CASE WHEN a1.TradeInVin2 <> '' THEN 1 ELSE 0 END + CASE WHEN a1.TradeInVin3 <> '' THEN 1 ELSE 0 END AS TradeInCount
	  , a2.BankName + ' ' + a2.BranchName + ' ' + a20.Name + ' ' + a2.AccountNumber AS AccountInformation
	  , a2.AccountHolder AS AccountName
	  , ISNULL(a2.PrintFlag, N'0') AS PrintFlag
	  , ISNULL(a1.RevenueStampCost, 0) AS RevenueStampCost			--Mod 2020/08/31 yano #4066
	  , ISNULL(a1.TradeInCarTaxDeposit, 0) AS TradeInCarTaxDeposit	--Mod 2020/08/31 yano #4066
	  , a1.SalesTax - a1.DiscountTax + a1.MakerOptionTaxAmount AS TaxationAmountTax
	  , a1.TradeInHolderName1 	--Add 2022/06/23 yano #4140
	  , a1.TradeInHolderName2	--Add 2022/06/23 yano #4140
	  , a1.TradeInHolderName3	--Add 2022/06/23 yano #4140
	  , a24.IssueCompanyNumber	--Add 2023/09/05 yano #4162

FROM dbo.CarSalesHeader AS a1
LEFT OUTER JOIN dbo.Department AS a2 ON a1.DepartmentCode = a2.DepartmentCode 
LEFT OUTER JOIN dbo.Office AS a3 ON a2.OfficeCode = a3.OfficeCode
LEFT OUTER JOIN dbo.Company AS a4 ON a3.CompanyCode = a4.CompanyCode
LEFT OUTER JOIN dbo.CarGrade AS a6 ON a1.CarGradeCode = a6.CarGradeCode
LEFT OUTER JOIN dbo.CarColor AS a7 ON a1.ExteriorColorCode = a7.CarColorCode
LEFT OUTER JOIN dbo.CarColor AS a13 ON a1.InteriorColorCode = a13.CarColorCode
LEFT OUTER JOIN dbo.Customer AS a8 ON a1.CustomerCode = a8.CustomerCode
LEFT OUTER JOIN dbo.c_NewUsedType AS a9 ON a1.NewUsedType = a9.Code
LEFT OUTER JOIN dbo.c_TransMission AS a10 ON a6.TransMission = a10.Code
LEFT OUTER JOIN dbo.c_Sex AS a11 ON a8.Sex = a11.Code
LEFT OUTER JOIN dbo.SalesCar AS a12 ON a1.SalesCarNumber = a12.SalesCarNumber
LEFT OUTER JOIN dbo.c_MileageUnit AS a14 ON a1.MileageUnit = a14.Code
LEFT OUTER JOIN dbo.c_RegistrationType AS a15 ON a1.RegistrationType = a15.Code 
LEFT OUTER JOIN dbo.c_VoluntaryInsuranceType AS a16 ON a1.VoluntaryInsuranceType = a16.Code
LEFT OUTER JOIN dbo.Loan AS a17 ON (CASE a1.PaymentPlanType WHEN 'A' THEN a1.LoanCodeA WHEN 'B' THEN a1.LoanCodeB WHEN 'C' THEN a1.LoanCodeC END) = a17.LoanCode
LEFT OUTER JOIN dbo.CustomerClaim AS a18 ON a17.CustomerClaimCode = a18.CustomerClaimCode
LEFT OUTER JOIN dbo.c_MileageUnit AS a19 ON a1.TradeInMileageUnit1 = a19.Code
LEFT OUTER JOIN dbo.c_DepositKind AS a20 ON a2.DepositKind = a20.Code
LEFT OUTER JOIN dbo.Employee AS a21 ON a4.EmployeeCode = a21.EmployeeCode
LEFT OUTER JOIN dbo.c_Steering AS a22 ON a12.Steering = a22.Code
LEFT OUTER JOIN dbo.Employee AS a5 ON a1.EmployeeCode = a5.EmployeeCode
LEFT OUTER JOIN dbo.Customer AS a23 ON a1.UserCode = a23.CustomerCode
LEFT OUTER JOIN dbo.Supplier AS a24 ON a8.CustomerCode = a24.SupplierCode		--Add 2023/09/05 #4162

GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "a1"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 322
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a2"
            Begin Extent = 
               Top = 114
               Left = 38
               Bottom = 222
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a3"
            Begin Extent = 
               Top = 222
               Left = 38
               Bottom = 330
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a4"
            Begin Extent = 
               Top = 330
               Left = 38
               Bottom = 438
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a6"
            Begin Extent = 
               Top = 438
               Left = 38
               Bottom = 546
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a7"
            Begin Extent = 
               Top = 546
               Left = 38
               Bottom = 654
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a13"
            Begin Extent = 
               Top = 654
               Left = 38
               Bottom = 762
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
        ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_CarSalesReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N' End
         Begin Table = "a8"
            Begin Extent = 
               Top = 762
               Left = 38
               Bottom = 870
               Right = 253
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a9"
            Begin Extent = 
               Top = 114
               Left = 282
               Bottom = 222
               Right = 430
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a10"
            Begin Extent = 
               Top = 222
               Left = 282
               Bottom = 330
               Right = 430
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a11"
            Begin Extent = 
               Top = 330
               Left = 282
               Bottom = 438
               Right = 430
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a12"
            Begin Extent = 
               Top = 870
               Left = 38
               Bottom = 978
               Right = 253
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a14"
            Begin Extent = 
               Top = 438
               Left = 282
               Bottom = 546
               Right = 430
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a15"
            Begin Extent = 
               Top = 546
               Left = 282
               Bottom = 654
               Right = 430
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a16"
            Begin Extent = 
               Top = 654
               Left = 282
               Bottom = 762
               Right = 430
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a17"
            Begin Extent = 
               Top = 978
               Left = 38
               Bottom = 1086
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a18"
            Begin Extent = 
               Top = 1086
               Left = 38
               Bottom = 1194
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a19"
            Begin Extent = 
               Top = 762
               Left = 291
               Bottom = 870
               Right = 439
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a20"
            Begin Extent = 
               Top = 870
               Left = 291
               Bottom = 978
               Right = 439
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a21"
            Begin Extent = 
               Top = 1194
               Left = 38
               Bottom = 1302
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a22"
            Begin Extent = 
               Top = 978
               Left = 282
               Bottom = 1086
               Right = 430
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a5"
            Begin Extent = 
               Top = 1302
               Left = 38
       ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_CarSalesReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane3', @value=N'        Bottom = 1410
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a23"
            Begin Extent = 
               Top = 1410
               Left = 38
               Bottom = 1554
               Right = 295
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_CarSalesReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=3 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_CarSalesReport'
GO


