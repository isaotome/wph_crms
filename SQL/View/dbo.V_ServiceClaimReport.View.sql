USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_ServiceClaimReport]    Script Date: 2023/09/28 8:20:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--2023/09/05 yano #4162 インボイス制度の対応
--2023/07/26 yano #4141【サービス伝票入力】請求明細書関連の改修
--2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
--2019/08/30 yano #3976 サービス伝票入力　受付担当の文言変更
--2023/06/12 openwave #4157 サービス帳票（見積書、請求明細書）の諸費用欄の表示方法変更
CREATE VIEW [dbo].[V_ServiceClaimReport]
AS
SELECT  h.SlipNumber,
		h.RevisionNumber,
		l.LineNumber,
		c2.CompanyName,
		o.OfficeName,
		o.FullName AS OfficeFullName,
		h.DepartmentCode,
		d.FullName AS DepartmentFullName,
		d.DepartmentName,
		d.PostCode AS DepartmentPostCode,
		d.Prefecture AS DepartmentPrefecture,
		d.City AS DepartmentCity,
		d.Address1 AS DepartmentAddress1,
		d.Address2 AS DepartmentAddress2,
		d.TelNumber1 AS DepartmentTelNumber1,
		d.FaxNumber AS DepartmentFaxNumber,
		h.CustomerCode,
		c.CustomerName,
		c.PostCode AS CustomerPostCode,
		c.Prefecture AS CustomerPrefecture,
		c.City AS CustomerCity,
		c.Address1 AS CustomerAddress1,
		c.Address2 AS CustomerAddress2,
		c.TelNumber AS CustomerTelNumber,
		c.MobileNumber AS CustomerMobileNumber,
		ce.EmployeeName AS CarEmployeeName,
		h.SalesDate,
		re.EmployeeName AS ReceiptionEmployeeName,		--Mod 2019/08/30 yano #3976
		ee.EmployeeName AS FrontEmployeeName,
		h.CarName,
		h.Mileage,
		mu.Name AS MileageUnit,
		h.EngineType,
		h.FirstRegistration,
		h.NextInspectionDate,
		h.ModelName,
		h.Vin,
		sc.ClassificationTypeNumber,
		l.ServiceWorkCode,
		l.LineContents,
		CASE l.ServiceType WHEN '003' THEN isnull(l.PartsNumber, 0) + ' : ' + l.LineContents
			ELSE l.LineContents 
		END AS LineContents2,
		w.Name AS WorkType,
		CASE WHEN (sw.DisablePriceFlag = '1' AND l.CustomerClaimCode = c.CustomerCode) OR l.TechnicalFeeAmount IS NULL OR l.ServiceType <> '002' THEN NULL
			 WHEN LEFT(l.ServiceMenuCode, 6) = 'DISCNT' THEN (- 1) * (isnull(l.TechnicalFeeAmount, 0) + isnull(l.TaxAmount, 0)) 
			 WHEN sw.Classification1 = '002' THEN l.TechnicalFeeAmount
			 ELSE l.TechnicalFeeAmount + isnull(l.TaxAmount, 0)
		END AS TechnicalFeeAmount,
		l.Quantity,
		CASE WHEN (sw.DisablePriceFlag = '1' AND l.CustomerClaimCode = c.CustomerCode) OR l.Price IS NULL THEN NULL
			 WHEN LEFT(l.PartsNumber, 6) = 'DISCNT' THEN (- 1) * (ISNULL(l.Price, 0)) 
			 WHEN sw.Classification1 = '002' THEN l.Price
			 ELSE CASE WHEN l.Quantity = 0 THEN l.Price 
			 		   ELSE ROUND(l.Price * (CAST(100 + l.Rate AS NUMERIC(18, 4)) / 100), 0, 1) 
                  END
		END AS Price,
		CASE WHEN (sw.DisablePriceFlag = '1' AND l.CustomerClaimCode = c.CustomerCode) OR l.Amount IS NULL OR l.ServiceType <> '003' THEN NULL
			 WHEN LEFT(l.PartsNumber, 6) = 'DISCNT' THEN (- 1) * (ISNULL(l.Amount, 0) + isnull(l.TaxAmount, 0)) 
			 WHEN sw.Classification1 = '002' THEN l.Amount ELSE l.Amount + isnull(l.TaxAmount, 0)
		END AS Amount,
		h.SalesPlanDate,
		h.ConsumptionTaxId,
		h.Rate, 
		eng.EmployeeName AS EngineerEmployeeName,
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR l.ServiceWorkCode = '10101' OR cnt.ServiceWorkCount = 1 THEN h.CostTotalAmount * isnull(cff.Coefficient,0)
			 ELSE 0
		END AS CostTotalAmount,
		b.CarBrandName,
		h.ArrivalPlanDate,
		l.SupplierCode,
		sup.SupplierName,
		v.TaxTotalAmount, 	
		v.EngineerTotalAmount,
		v.PartsTotalAmount,
		l.CustomerClaimCode,
		cc.CustomerClaimName,
		cc.PostCode AS CustomerClaimPostCode,
		ISNULL(cc.Prefecture, N'') AS CustomerClaimPrefecture,
		ISNULL(cc.City, N'') AS CustomerClaimCity,
		ISNULL(cc.Address1, N'') AS CustomerClaimAddress1,
		ISNULL(cc.Address2, N'') AS CustomerClaimAddress2,
		cc.TelNumber1 AS CustomerClaimTelNumber1,
		cc.TelNumber2 AS CustomerClaimTelNumber2,
		cc.FaxNumber AS CustomerClaimFaxNumber, 
		sw.DisablePriceFlag,
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR cnt10101.ServiceWorkCount > 0 OR cnt.ServiceWorkCount = 1 THEN isnull(h.CarLiabilityInsurance, 0) * isnull(cff.Coefficient,0)
			 ELSE 0
		END AS CarLiabilityInsurance,	--自賠責保険料
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR cnt10101.ServiceWorkCount > 0 OR cnt.ServiceWorkCount = 1 THEN isnull(h.CarWeightTax, 0) * isnull(cff.Coefficient,0)
			 ELSE 0
		END AS CarWeightTax,			--自動車重量税
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR cnt10101.ServiceWorkCount > 0 OR cnt.ServiceWorkCount = 1 THEN isnull(h.FiscalStampCost, 0) * isnull(cff.Coefficient,0)
			 ELSE 0
		END AS FiscalStampCost,			--印紙代
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR cnt10101.ServiceWorkCount > 0 OR cnt.ServiceWorkCount = 1 THEN isnull(j.DepositTotalAmount, 0) * isnull(cff.Coefficient,0)
			 ELSE 0
		END AS DepositTotalAmount,		--入金合計
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR cnt10101.ServiceWorkCount > 0 OR cnt.ServiceWorkCount = 1 THEN isnull(h.CarTax, 0) * isnull(cff.Coefficient,0)
			 ELSE 0
		END AS CarTax,					--自動車税
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR cnt10101.ServiceWorkCount > 0 OR cnt.ServiceWorkCount = 1 THEN isnull(h.NumberPlateCost, 0) * isnull(cff.Coefficient,0)
			 ELSE 0
		END AS NumberPlateCost,			--ナンバー代
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR cnt10101.ServiceWorkCount > 0 OR cnt.ServiceWorkCount = 1 THEN h.TaxFreeFieldName
			 ELSE ''
		END AS TaxFreeFieldName,		--課税自由項目名
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR cnt10101.ServiceWorkCount > 0 OR cnt.ServiceWorkCount = 1 THEN isnull(h.TaxFreeFieldValue, 0) * isnull(cff.Coefficient,0)			--2023/09/05 #4162
			 ELSE 0
		END AS TaxFreeFieldValue, 		--課税自由項目金額
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR l.ServiceWorkCode = '10101' OR cnt.ServiceWorkCount = 1 THEN isnull(h.TaxableCostTotalAmount, 0) * isnull(cff.Coefficient,0)		--2023/09/05 #4162
			 ELSE 0
		END AS TaxableCostTotalAmount,	--諸費用(課税)
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR cnt10101.ServiceWorkCount > 0 OR cnt.ServiceWorkCount = 1 THEN isnull(h.OptionalInsurance, 0) * isnull(cff.Coefficient,0)			--2023/09/05 #4162
			 ELSE 0
		END AS OptionalInsurance,		--任意保険
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR cnt10101.ServiceWorkCount > 0 OR cnt.ServiceWorkCount = 1 THEN isnull(h.SubscriptionFee, 0) * isnull(cff.Coefficient,0)			--2023/09/05 #4162
			 ELSE 0
		END AS SubscriptionFee,			--サービス加入料
		CASE WHEN h.CustomerCode = l.CustomerClaimCode OR cnt10101.ServiceWorkCount > 0 OR cnt.ServiceWorkCount = 1 THEN isnull(h.TaxableFreeFieldValue, 0) * isnull(cff.Coefficient,0)		--2023/09/05 #4162
			 ELSE 0
		END AS TaxableFreeFieldValue,	--課税自由項目金額
		h.CarTaxMemo,					--自動車税備考
		h.CarLiabilityInsuranceMemo,	--自賠責事項
		h.CarWeightTaxMemo,				--自動車重量税備考
		h.NumberPlateCostMemo,			--ナンバー代備考
		h.FiscalStampCostMemo,			--印紙代備考
		h.OptionalInsuranceMemo,		--任意保険備考
		h.SubscriptionFeeMemo,			--サービス加入料備考
		h.TaxableFreeFieldName,			--課税自由項目名
		(ISNULL(h.GrandTotalAmount, 0) - ISNULL(j.DepositTotalAmount, 0)) * isnull(cff.Coefficient,0) AS ClaimTotalAmount, --諸費用合計(=請求合計 - 入金合計)
		h.MorterViecleOfficialCode,		--陸運局コード
		h.RegistrationNumberType,
		h.RegistrationNumberKana,
		h.RegistrationNumberPlate, 
		d.BankName + ' ' + d.BranchName + ' ' + dk.Name + ' ' + d.AccountNumber AS AccountInformation,
		d.AccountHolder AS AccountName,
		ISNULL(d.PrintFlag, N'0') AS PrintFlag,
	   (SELECT	TOP (1) e.EmployeeName
			FROM	dbo.ServiceSalesLine AS a1
			INNER	JOIN	dbo.Employee AS e ON a1.EmployeeCode = e.EmployeeCode
			WHERE	(a1.EmployeeCode IS NOT NULL) AND (a1.SlipNumber = l.SlipNumber) AND (a1.RevisionNumber = l.RevisionNumber)
			ORDER	BY	a1.LineNumber) AS TopEngineerName,
		h.UsVin,
		h.InspectionExpireDate,
		cnt.ServiceWorkCount, 
		CASE WHEN sw.Classification2 = '006' THEN 1
			 ELSE 0
		END AS WarrantyFlag
FROM	dbo.ServiceSalesLine AS l
INNER	JOIN	dbo.ServiceSalesHeader AS h ON l.SlipNumber = h.SlipNumber AND l.RevisionNumber = h.RevisionNumber
LEFT	OUTER	JOIN   (SELECT	SlipNumber, RevisionNumber, CASE WHEN (CustomerClaimCode is null OR CustomerClaimCode ='') THEN CustomerCode ELSE CustomerClaimCode END AS CustomerClaimCode, 1 AS Coefficient	--Mod 2023/08/03 yano #4141
--LEFT	OUTER	JOIN   (SELECT	SlipNumber, RevisionNumber, isnull(CustomerClaimCode,CustomerCode) AS CustomerClaimCode, 1 AS Coefficient
							FROM	dbo.ServiceSalesheader) AS cff
							ON	cff.SlipNumber = l.SlipNumber AND cff.RevisionNumber = l.RevisionNumber AND cff.CustomerClaimCode = l.CustomerClaimCode
LEFT	OUTER	JOIN   (SELECT	SlipNumber, RevisionNumber, COUNT(*) AS ServiceWorkCount
							FROM	dbo.ServiceSalesLine
							WHERE	(ServiceType = '001')
							GROUP	BY	SlipNumber, RevisionNumber) AS cnt
							ON	cnt.SlipNumber = h.SlipNumber AND cnt.RevisionNumber = h.RevisionNumber
LEFT	OUTER	JOIN   (SELECT	SlipNumber, RevisionNumber, CustomerClaimCode, COUNT(*) AS ServiceWorkCount
							FROM	dbo.ServiceSalesLine AS ServiceSalesLine_1
							WHERE	(ServiceType = '001') AND (ServiceWorkCode = '10101') AND (DelFlag = '0')
							GROUP	BY	SlipNumber, RevisionNumber, CustomerClaimCode) AS cnt10101
							ON	cnt10101.SlipNumber = l.SlipNumber AND cnt10101.RevisionNumber = l.RevisionNumber AND cnt10101.CustomerClaimCode = l.CustomerClaimCode
LEFT	OUTER	JOIN	dbo.Department AS d ON h.DepartmentCode = d.DepartmentCode
LEFT	OUTER	JOIN	dbo.Customer AS c ON h.CustomerCode = c.CustomerCode
LEFT	OUTER	JOIN	dbo.Office AS o ON d.OfficeCode = o.OfficeCode
LEFT	OUTER	JOIN	dbo.Company AS c2 ON o.CompanyCode = c2.CompanyCode
LEFT	OUTER	JOIN	dbo.Employee AS re ON h.ReceiptionEmployeeCode = re.EmployeeCode
LEFT	OUTER	JOIN	dbo.Employee AS ee ON h.FrontEmployeeCode = ee.EmployeeCode
LEFT	OUTER	JOIN	dbo.Employee AS ce ON c.CarEmployeeCode = ce.EmployeeCode
LEFT	OUTER	JOIN	dbo.c_MileageUnit AS mu ON h.MileageUnit = mu.Code
LEFT	OUTER	JOIN	dbo.CarGrade AS cg ON h.CarGradeCode = cg.CarGradeCode
LEFT	OUTER	JOIN	dbo.c_WorkType AS w ON l.WorkType = w.Code
LEFT	OUTER	JOIN	dbo.Car ON cg.CarCode = dbo.Car.CarCode
LEFT	OUTER	JOIN	dbo.Brand AS b ON dbo.Car.CarBrandCode = b.CarBrandCode
LEFT	OUTER	JOIN	dbo.Employee AS eng ON l.EmployeeCode = eng.EmployeeCode
LEFT	OUTER	JOIN	dbo.Supplier AS sup ON l.SupplierCode = sup.SupplierCode
LEFT	OUTER	JOIN	dbo.CustomerClaim AS cc ON l.CustomerClaimCode = cc.CustomerClaimCode
LEFT	OUTER	JOIN	dbo.ServiceWork AS sw ON l.ServiceWorkCode = sw.ServiceWorkCode
LEFT	OUTER	JOIN	dbo.c_DepositKind AS dk ON d.DepositKind = dk.Code
LEFT	OUTER	JOIN	dbo.SalesCar AS sc ON h.SalesCarNumber = sc.SalesCarNumber
LEFT	OUTER	JOIN   (SELECT	SlipNumber, CustomerClaimCode, SUM(CASE journaltype WHEN '001' THEN amount ELSE (- 1) * Amount END) AS DepositTotalAmount
							FROM	dbo.Journal
							WHERE	(JournalType = '001') AND (DelFlag = '0')
							AND		(AccountCode	IN	(SELECT	AccountCode
															FROM	dbo.Account
															WHERE	(DepositFlag = '1') AND (DelFlag = '0')))
							GROUP	BY	SlipNumber, CustomerClaimCode
						) AS j	ON l.SlipNumber = j.SlipNumber AND l.CustomerClaimCode = j.CustomerClaimCode
LEFT	OUTER	JOIN   (SELECT	a.SlipNumber, a.RevisionNumber, a.CustomerClaimCode,
								SUM(CASE WHEN a.ServiceType = '002' THEN CASE WHEN a.TechnicalFeeAmount IS NULL THEN 0
																			  WHEN LEFT(a.ServiceMenuCode, 6) = 'DISCNT' THEN (- 1) * (isnull(a.TechnicalFeeAmount, 0) + isnull(a.TaxAmount, 0)) 
																			  WHEN b.Classification1 = '002' THEN a.TechnicalFeeAmount
																			  ELSE isnull(a.TechnicalFeeAmount, 0) + isnull(a.TaxAmount, 0)
																		 END ELSE 0
									END) AS EngineerTotalAmount,
								SUM(CASE WHEN a.ServiceType = '003' THEN CASE WHEN a.Amount IS NULL THEN 0
																			  WHEN LEFT(a.PartsNumber, 6) = 'DISCNT' THEN (- 1) * (isnull(a.Amount, 0) + isnull(a.TaxAmount, 0))
																			  WHEN b.Classification1 = '002' THEN a.Amount
																			  ELSE isnull(a.Amount, 0) + isnull(a.TaxAmount, 0)
																		 END
										 ELSE 0
									END) AS PartsTotalAmount,
								SUM(CASE WHEN b.Classification1 = '002' THEN 0
										 WHEN LEFT(a.PartsNumber, 6) = 'DISCNT' OR LEFT(a.ServiceMenuCode, 6) = 'DISCNT' THEN (- 1) * (isnull(a.TaxAmount, 0))
										 ELSE isnull(a.TaxAmount, 0)
									END) AS TaxTotalAmount
							FROM	dbo.ServiceSalesLine AS a
							LEFT	OUTER	JOIN	dbo.ServiceWork AS b ON a.ServiceWorkCode = b.ServiceWorkCode
							LEFT	OUTER	JOIN	dbo.ServiceSalesHeader AS c ON a.SlipNumber = c.SlipNumber AND a.RevisionNumber = c.RevisionNumber
							GROUP	BY	a.SlipNumber, a.RevisionNumber, a.CustomerClaimCode
						) AS v	ON l.SlipNumber = v.SlipNumber AND l.RevisionNumber = v.RevisionNumber AND l.CustomerClaimCode = v.CustomerClaimCode
UNION ALL
SELECT  h.SlipNumber,
		h.RevisionNumber,
		(SELECT MAX(LineNumber) + 1 FROM ServiceSalesLine l WHERE l.SlipNumber = h.SlipNumber AND l.RevisionNumber = h.RevisionNumber),
		c2.CompanyName,
		o.OfficeName,
		o.FullName AS OfficeFullName,
		h.DepartmentCode,
		d.FullName AS DepartmentFullName,
		d.DepartmentName,
		d.PostCode AS DepartmentPostCode,
		d.Prefecture AS DepartmentPrefecture,
		d.City AS DepartmentCity,
		d.Address1 AS DepartmentAddress1,
		d.Address2 AS DepartmentAddress2,
		d.TelNumber1 AS DepartmentTelNumber1,
		d.FaxNumber AS DepartmentFaxNumber,
		h.CustomerCode,
		c.CustomerName,
		c.PostCode AS CustomerPostCode,
		c.Prefecture AS CustomerPrefecture,
		c.City AS CustomerCity,
		c.Address1 AS CustomerAddress1,
		c.Address2 AS CustomerAddress2,
		c.TelNumber AS CustomerTelNumber,
		c.MobileNumber AS CustomerMobileNumber,
		ce.EmployeeName AS CarEmployeeName,
		h.SalesDate,
		re.EmployeeName AS ReceiptionEmployeeName,		--Mod 2019/08/30 yano #3976
		ee.EmployeeName AS FrontEmployeeName,
		h.CarName,
		h.Mileage,
		mu.Name AS MileageUnit,
		h.EngineType,
		h.FirstRegistration,
		h.NextInspectionDate,
		h.ModelName,
		h.Vin,
		sc.ClassificationTypeNumber,
		null AS ServiceWorkCode,
		null AS LineContents,
		null AS LineContents2,
		null AS WorkType,
		null AS TechnicalFeeAmount,
		null AS Quantity,
		null AS Price,
		null AS Amount,
		h.SalesPlanDate,
		h.ConsumptionTaxId,
		h.Rate, 
		null AS EngineerEmployeeName,
		h.CostTotalAmount,
		b.CarBrandName,
		h.ArrivalPlanDate,
		null AS SupplierCode,
		null AS SupplierName,
		0 AS TaxTotalAmount, 	
		0 AS EngineerTotalAmount,
		0 AS PartsTotalAmount,
		CASE WHEN (h.CustomerClaimCode is null OR h. CustomerClaimCode ='') THEN h.CustomerCode ELSE h.CustomerClaimCode END	AS	CustomerClaimCode,		--Mod 2023/08/03 yano #4141
		--ISNULL(h.CustomerClaimCode,h.CustomerCode)	AS	CustomerClaimCode,
		cc.CustomerClaimName,
		cc.PostCode AS CustomerClaimPostCode,
		ISNULL(cc.Prefecture, N'') AS CustomerClaimPrefecture,
		ISNULL(cc.City, N'') AS CustomerClaimCity,
		ISNULL(cc.Address1, N'') AS CustomerClaimAddress1,
		ISNULL(cc.Address2, N'') AS CustomerClaimAddress2,
		cc.TelNumber1 AS CustomerClaimTelNumber1,
		cc.TelNumber2 AS CustomerClaimTelNumber2,
		cc.FaxNumber AS CustomerClaimFaxNumber, 
		null AS DisablePriceFlag,
		isnull(h.CarLiabilityInsurance, 0) AS CarLiabilityInsurance,			--自賠責保険料
		isnull(h.CarWeightTax, 0) AS CarWeightTax,								--自動車重量税
		isnull(h.FiscalStampCost, 0) AS FiscalStampCost,						--印紙代
		isnull(j.DepositTotalAmount, 0) AS DepositTotalAmount,					--入金合計
		isnull(h.CarTax, 0) AS CarTax,											--自動車税
		isnull(h.NumberPlateCost, 0) AS NumberPlateCost,						--ナンバー代
		h.TaxFreeFieldName AS TaxFreeFieldName,									--課税自由項目名
		isnull(h.TaxFreeFieldValue, 0) AS TaxFreeFieldValue, 					--課税自由項目金額		--2023/09/05 #4162
		isnull(h.TaxableCostTotalAmount, 0) AS TaxableCostTotalAmount,			--諸費用(課税)			--2023/09/05 #4162
		isnull(h.OptionalInsurance, 0) AS OptionalInsurance,					--任意保険				--2023/09/05 #4162
		isnull(h.SubscriptionFee, 0) AS SubscriptionFee,						--サービス加入料		--2023/09/05 #4162	
		isnull(h.TaxableFreeFieldValue, 0) AS TaxableFreeFieldValue,			--課税自由項目金額		--2023/09/05 #4162
		h.CarTaxMemo,					--自動車税備考
		h.CarLiabilityInsuranceMemo,	--自賠責事項
		h.CarWeightTaxMemo,				--自動車重量税備考
		h.NumberPlateCostMemo,			--ナンバー代備考
		h.FiscalStampCostMemo,			--印紙代備考
		h.OptionalInsuranceMemo,		--任意保険備考
		h.SubscriptionFeeMemo,			--サービス加入料備考
		h.TaxableFreeFieldName,			--課税自由項目名
		ISNULL(h.GrandTotalAmount, 0) - ISNULL(j.DepositTotalAmount, 0) AS ClaimTotalAmount, --諸費用合計(=請求合計 - 入金合計)
		h.MorterViecleOfficialCode,		--陸運局コード
		h.RegistrationNumberType,
		h.RegistrationNumberKana,
		h.RegistrationNumberPlate, 
		d.BankName + ' ' + d.BranchName + ' ' + dk.Name + ' ' + d.AccountNumber AS AccountInformation,
		d.AccountHolder AS AccountName,
		ISNULL(d.PrintFlag, N'0') AS PrintFlag,
		null AS TopEngineerName,
		h.UsVin,
		h.InspectionExpireDate,
		0 AS ServiceWorkCount, 
		0 AS WarrantyFlag
FROM	dbo.ServiceSalesHeader h
LEFT	OUTER	JOIN   (SELECT	SlipNumber, RevisionNumber, COUNT(*) AS ServiceWorkCount
							FROM	dbo.ServiceSalesLine
							WHERE	(ServiceType = '001')
							GROUP	BY	SlipNumber, RevisionNumber) AS cnt
							ON	cnt.SlipNumber = h.SlipNumber AND cnt.RevisionNumber = h.RevisionNumber
LEFT	OUTER	JOIN	dbo.Department AS d ON h.DepartmentCode = d.DepartmentCode
LEFT	OUTER	JOIN	dbo.Customer AS c ON h.CustomerCode = c.CustomerCode
LEFT	OUTER	JOIN	dbo.Office AS o ON d.OfficeCode = o.OfficeCode
LEFT	OUTER	JOIN	dbo.Company AS c2 ON o.CompanyCode = c2.CompanyCode
LEFT	OUTER	JOIN	dbo.Employee AS re ON h.ReceiptionEmployeeCode = re.EmployeeCode
LEFT	OUTER	JOIN	dbo.Employee AS ee ON h.FrontEmployeeCode = ee.EmployeeCode
LEFT	OUTER	JOIN	dbo.Employee AS ce ON c.CarEmployeeCode = ce.EmployeeCode
LEFT	OUTER	JOIN	dbo.c_MileageUnit AS mu ON h.MileageUnit = mu.Code
LEFT	OUTER	JOIN	dbo.CarGrade AS cg ON h.CarGradeCode = cg.CarGradeCode
LEFT	OUTER	JOIN	dbo.Car ON cg.CarCode = dbo.Car.CarCode
LEFT	OUTER	JOIN	dbo.Brand AS b ON dbo.Car.CarBrandCode = b.CarBrandCode
LEFT	OUTER	JOIN	dbo.CustomerClaim AS cc ON cc.CustomerClaimCode = CASE WHEN (h.CustomerClaimCode is null OR h.CustomerClaimCode ='') THEN h.CustomerCode ELSE h.CustomerClaimCode END	--Mod 2023/08/03 yano #4141
--LEFT	OUTER	JOIN	dbo.CustomerClaim AS cc ON cc.CustomerClaimCode = ISNULL(h.CustomerClaimCode,h.CustomerCode)
LEFT	OUTER	JOIN	dbo.c_DepositKind AS dk ON d.DepositKind = dk.Code
LEFT	OUTER	JOIN	dbo.SalesCar AS sc ON h.SalesCarNumber = sc.SalesCarNumber
LEFT	OUTER	JOIN   (SELECT	SlipNumber, CustomerClaimCode, SUM(CASE journaltype WHEN '001' THEN amount ELSE (- 1) * Amount END) AS DepositTotalAmount
							FROM	dbo.Journal
							WHERE	(JournalType = '001') AND (DelFlag = '0')
							AND		(AccountCode	IN	(SELECT	AccountCode
															FROM	dbo.Account
															WHERE	(DepositFlag = '1') AND (DelFlag = '0')))
							GROUP	BY	SlipNumber, CustomerClaimCode
						) AS j	ON h.SlipNumber = j.SlipNumber AND CASE WHEN (h.CustomerClaimCode is null OR h.CustomerClaimCode ='') THEN h.CustomerCode ELSE h.CustomerClaimCode END = j.CustomerClaimCode			--Mod 2023/08/03 yano #4141
						--) AS j	ON h.SlipNumber = j.SlipNumber AND ISNULL(h.CustomerClaimCode,h.CustomerCode) = j.CustomerClaimCode
WHERE	h.DelFlag	=	'0'
AND	   (ISNULL(h.CarTax,0)	<>	0	or	ISNULL(h.CarLiabilityInsurance,0)	<>	0	or	ISNULL(h.CarWeightTax,0)	<>	0
or		ISNULL(h.NumberPlateCost,0)	<>	0	or	ISNULL(h.FiscalStampCost,0)	<>	0	or	ISNULL(h.OptionalInsurance,0)	<>	0
or		ISNULL(h.TaxFreeFieldValue,0)	<>	0	or	ISNULL(h.SubscriptionFee,0)	<>	0	or	ISNULL(h.TaxableFreeFieldValue,0)	<>	0)
AND		NOT EXISTS	(SELECT	CustomerClaimCode
						FROM	ServiceSalesLine l
						WHERE	l.SlipNumber		=	h.SlipNumber
						AND		l.RevisionNumber	=	h.RevisionNumber
						AND		l.CustomerClaimCode	=	CASE WHEN (h.CustomerClaimCode is null OR h.CustomerClaimCode ='') THEN h.CustomerCode ELSE h.CustomerClaimCode END)		--Mod 2023/08/03 yano #4141
						--AND		l.CustomerClaimCode	=	ISNULL(h.CustomerClaimCode,h.CustomerCode))
;

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
         Begin Table = "l"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "h"
            Begin Extent = 
               Top = 114
               Left = 38
               Bottom = 222
               Right = 304
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cnt"
            Begin Extent = 
               Top = 6
               Left = 468
               Bottom = 130
               Right = 665
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cnt10101"
            Begin Extent = 
               Top = 6
               Left = 703
               Bottom = 150
               Right = 911
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "d"
            Begin Extent = 
               Top = 222
               Left = 38
               Bottom = 330
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "c"
            Begin Extent = 
               Top = 330
               Left = 38
               Bottom = 438
               Right = 253
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "o"
            Begin Extent = 
               Top = 438
               Left = 38
               Bottom = 546
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ServiceClaimReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'End
         Begin Table = "c2"
            Begin Extent = 
               Top = 546
               Left = 38
               Bottom = 654
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "re"
            Begin Extent = 
               Top = 654
               Left = 38
               Bottom = 762
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ee"
            Begin Extent = 
               Top = 762
               Left = 38
               Bottom = 870
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "mu"
            Begin Extent = 
               Top = 6
               Left = 282
               Bottom = 114
               Right = 430
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cg"
            Begin Extent = 
               Top = 870
               Left = 38
               Bottom = 978
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "w"
            Begin Extent = 
               Top = 222
               Left = 282
               Bottom = 330
               Right = 430
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Car"
            Begin Extent = 
               Top = 978
               Left = 38
               Bottom = 1086
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "b"
            Begin Extent = 
               Top = 1086
               Left = 38
               Bottom = 1194
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "eng"
            Begin Extent = 
               Top = 1194
               Left = 38
               Bottom = 1302
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "sup"
            Begin Extent = 
               Top = 1302
               Left = 38
               Bottom = 1410
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cc"
            Begin Extent = 
               Top = 1410
               Left = 38
               Bottom = 1518
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "sw"
            Begin Extent = 
               Top = 1518
               Left = 38
               Bottom = 1626
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "dk"
            Begin Extent = 
               Top = 330
               Left = 291
               Bottom = 438
               Right = 439
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "sc"
            Begin Extent = 
               Top = 1626
               Left = 38
               Bottom = 1734
               Right = 275
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "j"
            Begin Extent = 
               Top = 6
               Left = 949
               Bottom =' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ServiceClaimReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane3', @value=N' 130
               Right = 1158
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "v"
            Begin Extent = 
               Top = 132
               Left = 468
               Bottom = 276
               Right = 683
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ServiceClaimReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=3 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ServiceClaimReport'
GO


