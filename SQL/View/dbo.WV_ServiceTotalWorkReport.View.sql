USE [WPH_DB]
GO

/****** Object:  View [dbo].[WV_ServiceTotalWorkReport]    Script Date: 2017/04/13 15:05:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[WV_ServiceTotalWorkReport]
AS
SELECT H.SalesDate,
	   H.SlipNumber,
	   H.RevisionNumber,
	   L.LineNumber,
	   H.ServiceOrderStatus,
	   cS2.Name AS ServiceOrderStatusName,
	   L.ServiceWorkCode,
	   w.Name AS ServiceWorkName,
	   L.ServiceType,
	   cS1.Name AS ServiceTypeName,
	   L.LineContents,
	   CASE WHEN LEFT(L.ServiceMenuCode, 6) = 'DISCNT' OR LEFT(L.PartsNumber, 6) = 'DISCNT' THEN (L.Amount + L.TechnicalFeeAmount) * - 1 ELSE L.Amount + L.TechnicalFeeAmount END AS Amount, 
       CASE WHEN LEFT(L.ServiceMenuCode, 6) = 'DISCNT' OR LEFT(L.PartsNumber, 6) = 'DISCNT' THEN (L.TaxAmount) * - 1 ELSE L.TaxAmount END AS TaxAmount,
	   L.cost,
	   H.DepartmentCode,
	   D.DepartmentName,
	   H.FrontEmployeeCode,
	   E.EmployeeName,
	   CASE WHEN LEFT(l.ServiceWorkCode, 3) = '101' THEN '01.車検' ELSE 
	   CASE WHEN (LEFT(l.ServiceWorkCode, 3) = '102' OR L.ServiceWorkCode = '30000') THEN '02.定期点検' ELSE 
	   CASE WHEN LEFT(l.ServiceWorkCode, 3) = '103' THEN '03.一般整備' ELSE
	   CASE WHEN LEFT(l.ServiceWorkCode, 3) = '106' THEN '09.部品その他' ELSE
	   CASE WHEN LEFT(l.ServiceWorkCode, 3) = '104' THEN '05.鈑金塗装' ELSE
	   CASE WHEN LEFT(l.ServiceWorkCode, 3) = '105' THEN '06.ワランティ' ELSE
	   CASE WHEN LEFT(l.ServiceWorkCode, 3) = '199' THEN '09.部品その他' ELSE
	   CASE WHEN LEFT(l.ServiceWorkCode, 1) = '2' THEN '10.社内' ELSE '' 
	   END END END END END END END END AS DocumantName,
	   C.CustomerClaimCode,
	   C.CustomerClaimName,
	   H.WorkingEndDate,
	   H.QuoteDate,
	   H.CarName,
	   H.Vin,
	   H.ArrivalPlanDate,
	   S.SupplierCode,
	   S.SupplierName,
	   E2.EmployeeCode,
	   E2.EmployeeName AS MekaEmployeeName,
	   H.CustomerCode,
	   Cu.CustomerName,
	   H.CarBrandName,
	   L.PartsNumber,
	   L.WorkType,
	   Cw.Name AS WorkTypeName,
	   L.LineType,
	   L.Quantity,
	   L.Price,
	   L.UnitCost,
	   L.ConsumptionTaxId,
	   L.Rate,
	   w.AccountClassCode,
	   dbo.f_getMechanicEmployeeName(L.SlipNumber, L.RevisionNumber, L.ServiceWorkCode) AS MechanicEmployeeName
FROM   dbo.ServiceSalesHeader AS H 
	   INNER JOIN (SELECT SlipNumber,
						  RevisionNumber,
						  LineNumber,
						  ServiceType,
						  ServiceWorkCode,
						  LineContents,
						  ServiceMenuCode,
						  CustomerClaimCode,
						  PartsNumber,
						  CASE WHEN amount IS NULL THEN 0 ELSE amount END AS Amount,
						  CASE WHEN TaxAmount IS NULL THEN 0 ELSE TaxAmount END AS TaxAmount,
						  CASE WHEN TechnicalFeeAmount IS NULL THEN 0 ELSE TechnicalFeeAmount END AS TechnicalFeeAmount,
						  CASE WHEN cost IS NULL THEN 0 ELSE Cost END AS cost,
						  EmployeeCode,
						  SupplierCode,
						  WorkType,
						  LineType,
						  Quantity,
						  Price,
						  UnitCost,
						  ConsumptionTaxId,
						  Rate
                    FROM  dbo.ServiceSalesLine) AS L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber
		LEFT OUTER JOIN dbo.Employee AS E2 ON L.EmployeeCode = E2.EmployeeCode
		LEFT OUTER JOIN dbo.Supplier AS S ON L.SupplierCode = S.SupplierCode
		LEFT OUTER JOIN dbo.c_ServiceType AS cS1 ON cS1.Code = L.ServiceType
		LEFT OUTER JOIN dbo.c_ServiceOrderStatus AS cS2 ON cS2.Code = H.ServiceOrderStatus
		LEFT OUTER JOIN dbo.ServiceWork AS w ON L.ServiceWorkCode = w.ServiceWorkCode
		LEFT OUTER JOIN dbo.Department AS D ON H.DepartmentCode = D.DepartmentCode
		LEFT OUTER JOIN dbo.Employee AS E ON H.FrontEmployeeCode = E.EmployeeCode
		LEFT OUTER JOIN dbo.CustomerClaim AS C ON L.CustomerClaimCode = C.CustomerClaimCode
		LEFT OUTER JOIN dbo.Customer AS Cu ON H.CustomerCode = Cu.CustomerCode
		LEFT OUTER JOIN dbo.c_WorkType AS Cw ON L.WorkType = Cw.Code

WHERE   (H.DelFlag = '0') AND (L.ServiceType IN ('001', '002', '003'))



GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[26] 4[21] 2[23] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1[60] 2[15] 3) )"
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
         Begin Table = "H"
            Begin Extent = 
               Top = 142
               Left = 394
               Bottom = 513
               Right = 660
            End
            DisplayFlags = 280
            TopColumn = 28
         End
         Begin Table = "E2"
            Begin Extent = 
               Top = 342
               Left = 880
               Bottom = 450
               Right = 1086
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "S"
            Begin Extent = 
               Top = 34
               Left = 883
               Bottom = 142
               Right = 1089
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cS1"
            Begin Extent = 
               Top = 220
               Left = 869
               Bottom = 328
               Right = 1017
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cS2"
            Begin Extent = 
               Top = 114
               Left = 224
               Bottom = 222
               Right = 372
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "w"
            Begin Extent = 
               Top = 114
               Left = 410
               Bottom = 222
               Right = 616
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "D"
            Begin Extent = 
               Top = 222
               Left = 38
               Bottom = 330
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
 ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_ServiceTotalWorkReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'        End
         Begin Table = "E"
            Begin Extent = 
               Top = 222
               Left = 282
               Bottom = 330
               Right = 488
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "C"
            Begin Extent = 
               Top = 330
               Left = 38
               Bottom = 438
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Cu"
            Begin Extent = 
               Top = 438
               Left = 38
               Bottom = 582
               Right = 295
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Cw"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 150
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "L"
            Begin Extent = 
               Top = 450
               Left = 698
               Bottom = 594
               Right = 908
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
      Begin ColumnWidths = 25
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
         Column = 1320
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_ServiceTotalWorkReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_ServiceTotalWorkReport'
GO


