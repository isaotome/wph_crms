USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_ServiceQuoteReport]    Script Date: 2020/06/09 16:38:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- ----------------------------------------------------------------------------------
-- 機能：サービス見積書
-- 作成日：???
-- 更新日：
--			2020/06/08 yano #3665【サービス】サービス伝票の見積もりへ振込先印刷
--			2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
--			2019/08/30 yano #3976 サービス伝票入力　受付担当の文言変更

-- ----------------------------------------------------------------------------------
CREATE view [dbo].[V_ServiceQuoteReport]
as
select
	h.SlipNumber,
	h.RevisionNumber,
	l.LineNumber,
	c2.CompanyName,
	o.OfficeName,
	o.FullName as OfficeFullName,
	h.DepartmentCode,
	d.DepartmentName,
	d.FullName as DepartmentFullName,
	d.PostCode as DepartmentPostCode,
	d.Prefecture as DepartmentPrefecture,
	d.City as DepartmentCity,
	d.Address1 as DepartmentAddress1,
	d.Address2 as DepartmentAddress2,
	d.TelNumber1 as DepartmentTelNumber1,
	d.FaxNumber as DepartmentFaxNumber,
	h.CustomerCode,
	c.CustomerName,
	c.PostCode as CustomerPostCode,
	c.Prefecture as CustomerPrefecture,
	c.City as CustomerCity,
	c.Address1 as CustomerAddress1,
	c.Address2 as CustomerAddress2,
	c.TelNumber as CustomerTelNumber,
	c.MobileNumber as CustomerMobileNumber,
	ce.EmployeeName as CarEmployeeName ,		--Add 2019/08/30 yano #3976
	h.SalesDate,
	re.EmployeeName as ReceiptionEmployeeName,	
	ee.EmployeeName as FrontEmployeeName,
	h.CarName,
	h.Mileage,
	mu.Name as MileageUnit,
	h.EngineType,
	h.FirstRegistration,
	h.NextInspectionDate,
	h.ModelName,
	h.Vin,
	sc.ClassificationTypeNumber,
	l.ServiceWorkCode,
	l.LineContents,
	--l.LineContents as LineContents2,
	case l.ServiceType 
		when '003' then isnull(l.PartsNumber,'') + ' : ' + l.LineContents
		else l.LineContents
	end as LineContents2,
	
	w.Name as WorkType,
	case 
		when sw.DisablePriceFlag='1' or l.TechnicalFeeAmount is null or l.ServiceType<>'002' then null
		when left(l.ServiceMenuCode,6)='DISCNT' then (-1)*(isnull(l.TechnicalFeeAmount,0)+isnull(l.TaxAmount,0))
		when sw.Classification1='002' then l.TechnicalFeeAmount
		else l.TechnicalFeeAmount + isnull(l.TaxAmount,0) 
	end as TechnicalFeeAmount,
	l.Quantity,
	case 
		when sw.DisablePriceFlag='1' or l.Price is null then null 
		when LEFT(l.PartsNumber,6)='DISCNT' then (-1)*(ISNULL(l.Price,0))
		when sw.Classification1='002' then l.Price
		else			
			case 
				when l.Quantity=0 then l.Price 
				-- 2012/01/25 税込単価がズレるので単価に5%加えて算出
				--else l.Price + Floor(ISNULL(l.TaxAmount,0)/l.Quantity)
				-- 2014/03/03 消費税対応
				--else ROUND(l.Price * 1.05,0,1)
				else ROUND(l.Price*(cast(100 + l.rate AS NUMERIC(18,4))/100),0,1)
			end
	end as Price,
	case 
		when sw.DisablePriceFlag='1' or l.Amount is null or l.ServiceType<>'003' then null 
		when LEFT(l.PartsNumber,6)='DISCNT' then (-1)*(ISNULL(l.Amount,0)+isnull(l.TaxAmount,0))
		when sw.Classification1='002' then l.Amount
		else l.Amount + isnull(l.TaxAmount,0) 
	end as Amount,
	h.SalesPlanDate,
	h.ConsumptionTaxId,
	h.Rate,
	--l.TaxAmount,
	eng.EmployeeName as EngineerEmployeeName,
	--h.SubTotalAmount + h.TotalTaxAmount as SubTotalAmount,
	--h.TotalTaxAmount,
	--h.ServiceTotalAmount,
	--h.GrandTotalAmount,
	--h.PartsTotalAmount,
	--h.EngineerTotalAmount,
	h.CostTotalAmount,
	b.CarBrandName,
	h.ArrivalPlanDate,
	l.SupplierCode,
	sup.SupplierName,
	--v.TaxFreeAmount,
	--v.TaxationAmount,
	--isnull(v.TaxFreeAmount,0) + isnull(v.TaxationAmount,0) as ServiceSubTotalAmount,
	v.TaxTotalAmount,
	v.EngineerTotalAmount,
	--v.TechnicalAmount,
	v.PartsTotalAmount,
	l.CustomerClaimCode,
	cc.CustomerClaimName,
	cc.PostCode as CustomerClaimPostCode,
	isnull(cc.Prefecture,'') as CustomerClaimPrefecture,
	isnull(cc.City,'') as CustomerClaimCity,
	isnull(cc.Address1,'') as CustomerClaimAddress1,
	isnull(cc.Address2,'') as CustomerClaimAddress2,
	cc.TelNumber1 as CustomerClaimTelNumber1,
	cc.TelNumber2 as CustomerClaimTelNumber2,
	cc.FaxNumber as CustomerClaimFaxNumber,
	sw.DisablePriceFlag,
	isnull(h.CarLiabilityInsurance,0) as CarLiabilityInsurance,
	isnull(h.CarWeightTax,0) as CarWeightTax,
	isnull(h.FiscalStampCost,0) as FiscalStampCost ,
	isnull(j.DepositTotalAmount,0) as DepositTotalAmount,
	ISNULL(h.GrandTotalAmount,0) - ISNULL(j.DepositTotalAmount,0) as ClaimTotalAmount,
	h.MorterViecleOfficialCode,
	h.RegistrationNumberType,
	h.RegistrationNumberKana,
	h.RegistrationNumberPlate,
	(select top(1) e.EmployeeName from ServiceSalesLine a1 join Employee e on a1.EmployeeCode=e.EmployeeCode where a1.EmployeeCode is not null and a1.SlipNumber=l.SlipNumber and a1.RevisionNumber=l.RevisionNumber order by a1.LineNumber) as TopEngineerName,
	h.CarTax,
	h.NumberPlateCost,
	h.TaxFreeFieldName,
	h.TaxFreeFieldValue,
	h.UsVin,
	h.InspectionExpireDate,

	-- Add 2020/02/17 yano #4025------------------------------
	h.OptionalInsurance,
	h.SubscriptionFee,
	h.TaxableCostTotalAmount,
	h.CarTaxMemo,
	h.CarLiabilityInsuranceMemo,
	h.CarWeightTaxMemo,
	h.NumberPlateCostMemo,
	h.FiscalStampCostMemo,
	h.OptionalInsuranceMemo,
	h.SubscriptionFeeMemo,
	h.TaxableFreeFieldValue,
	h.TaxableFreeFieldName,
	-- -------------------------------------------------------
	-- Add 2020/06/08 yano #3655---------------------------------------------------------------------
	d.BankName + ' ' + d.BranchName + ' ' + dk.Name + ' ' + d.AccountNumber AS AccountInformation, 
	d.AccountHolder AS AccountName
	-- ----------------------------------------------------------------------------------------------
from
	ServiceSalesLine l
left join ServiceSalesHeader h on l.SlipNumber = h.SlipNumber and l.RevisionNumber = h.RevisionNumber
left join Department d on h.DepartmentCode = d.DepartmentCode
left join Customer c on h.CustomerCode = c.CustomerCode
left join Office o on d.OfficeCode = o.OfficeCode
left join Company c2 on o.CompanyCode = c2.CompanyCode
left join Employee re on h.ReceiptionEmployeeCode = re.EmployeeCode
left join Employee ee on h.FrontEmployeeCode=ee.EmployeeCode
left join c_MileageUnit mu on h.MileageUnit=mu.Code
left join CarGrade cg on h.CarGradeCode = cg.CarGradeCode
left join c_WorkType w on l.WorkType=w.Code
left join Car on cg.CarCode = Car.CarCode
left join Brand b on Car.CarBrandCode=b.CarBrandCode
left join Employee eng on l.EmployeeCode=eng.EmployeeCode
left join Supplier sup on l.SUpplierCode = sup.SupplierCode
--left join V_ServiceSalesReportSum v on l.slipnumber=v.slipnumber and l.revisionnumber=v.revisionnumber and l.customerclaimcode=v.customerclaimcode
left join CustomerClaim cc on l.CustomerClaimCode=cc.CustomerClaimCode
left join ServiceWork sw on l.ServiceWorkCode=sw.ServiceWorkCode
left join SalesCar sc on h.SalesCarNumber=sc.SalesCarNumber
left join Employee ce on c.CarEmployeeCode = ce.EmployeeCode	--Add 2019/08/30
left join c_DepositKind AS dk ON d.DepositKind = dk.Code		--Add 2020/06/08 yano #3655
left join 
	(
		select slipnumber,sum(case journaltype when '001' then amount else (-1) * Amount end) as DepositTotalAmount from journal
		where JournalType='001' and DelFlag='0' and AccountCode in
		(select AccountCode from Account
		 where DepositFlag='1' and DelFlag='0')
		group by slipnumber
	) j on h.SlipNumber=j.SlipNumber
left join 
	(
		select a.slipnumber,a.revisionnumber,
		SUM(
			case
				when a.ServiceType='002' then
					case 
						when a.TechnicalFeeAmount is null then 0 
						when left(a.ServiceMenuCode,6)='DISCNT' then (-1)*(isnull(a.TechnicalFeeAmount,0)+isnull(a.TaxAmount,0))
						when b.Classification1='002' then a.TechnicalFeeAmount
						else isnull(a.TechnicalFeeAmount,0)+isnull(a.TaxAmount,0) 
					end
				else 0
			end
			) as EngineerTotalAmount,
		SUM(
			case
				when a.ServiceType='003' then
					case 
						when a.Amount is null then 0 
						when LEFT(a.PartsNumber,6)='DISCNT' then (-1)*(isnull(a.Amount,0)+isnull(a.TaxAmount,0))
						when b.Classification1='002' then a.Amount
						else isnull(a.Amount,0)+isnull(a.TaxAmount,0) 
					end
				else 0
			end
			) as PartsTotalAmount,
		SUM(
			case 
				when b.Classification1='002' then 0
				when LEFT(a.PartsNumber,6)='DISCNT' OR LEFT(a.ServiceMenuCode,6)='DISCNT' then (-1)*(isnull(a.TaxAmount,0))
				else isnull(a.TaxAmount,0) 
			end
			) as TaxTotalAmount
		from ServiceSalesLine a 
		left join servicework b on a.ServiceWorkCode=b.ServiceWorkCode
		left join ServiceSalesHeader c on a.SlipNumber=c.SlipNumber and a.RevisionNumber=c.RevisionNumber
		group by a.SlipNumber,a.RevisionNumber
	) v on l.SlipNumber=v.SlipNumber and l.RevisionNumber=v.RevisionNumber


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
         End
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ServiceQuoteReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'         Begin Table = "ee"
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
               Top = 114
               Left = 342
               Bottom = 192
               Right = 519
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "v"
            Begin Extent = 
               Top = 330
               Left = 291
               Bottom = 438
               Right = 474
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
      Begin ColumnWidths = 75
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ServiceQuoteReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane3', @value=N'1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ServiceQuoteReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=3 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ServiceQuoteReport'
GO


