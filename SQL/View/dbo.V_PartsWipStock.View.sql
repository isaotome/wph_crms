USE [WPH_DB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE VIEW [dbo].[V_PartsWipStock]
AS
SELECT            H.ArrivalPlanDate, H.SlipNumber, L.LineNumber, RIGHT(C1.Name, 4) AS ServiceOrderStatus, W.Name, E1.EmployeeName, E2.EmployeeName AS EmployeeName2, C.CustomerName, 
                        H.CarName, H.Vin, L.ServiceType,CASE WHEN L.ServiceType = '003' THEN 'ïîïi' ELSE CASE WHEN L.ServiceType = '002' THEN 'äOíç' ELSE '' END END AS ServiceType1, 
                        C2.Name AS StockStatus, PP.PurchaseOrderDate, PP.ArrivalPlanDate AS PPArrivalPlanDate, PP2.PurchaseDate, L.PartsNumber, 
                        CASE WHEN L.ServiceType = '003' THEN L.LineContents ELSE '' END AS ServiceType2, ISNULL(PC_1.Price, 0) AS Price, ISNULL(L.Quantity, 0) AS Quantity, ISNULL(PC_1.Price, 0) 
                        * ISNULL(L.Quantity, 0) AS TotalAmount, S.SupplierName, CASE WHEN L.ServiceType = '002' THEN L.LineContents ELSE '' END AS LineContents, 
                        CASE WHEN L.ServiceType = '002' THEN L.Cost ELSE 0 END AS OutOrderCost, PC_1.CloseMonth, H.DepartmentCode, H.DelFlag, P.PartsNameJp
FROM              dbo.ServiceSalesHeader AS H INNER JOIN
                        dbo.ServiceSalesLine AS L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber INNER JOIN
                        dbo.Customer AS C ON H.CustomerCode = C.CustomerCode INNER JOIN
                        dbo.ServiceWork AS W ON L.ServiceWorkCode = W.ServiceWorkCode INNER JOIN
                        dbo.c_ServiceOrderStatus AS C1 ON H.ServiceOrderStatus = C1.Code LEFT OUTER JOIN
                        dbo.c_StockStatus AS C2 ON L.StockStatus = C2.Code LEFT OUTER JOIN
                        dbo.Parts AS P ON L.PartsNumber = P.PartsNumber LEFT OUTER JOIN
                        dbo.PartsPurchaseOrder AS PP ON H.SlipNumber = PP.ServiceSlipNumber AND L.PartsNumber = PP.PartsNumber LEFT OUTER JOIN
                        dbo.PartsPurchase AS PP2 ON PP.PurchaseOrderNumber = PP2.PurchaseOrderNumber LEFT OUTER JOIN
                        dbo.Employee AS E1 ON H.FrontEmployeeCode = E1.EmployeeCode LEFT OUTER JOIN
                            (SELECT            SlipNumber, RevisionNumber, MAX(EmployeeCode) AS EmployeeCode
                               FROM              dbo.ServiceSalesLine
                               WHERE             (DelFlag = '0') AND (EmployeeCode IS NOT NULL) AND (RTRIM(EmployeeCode) <> '')
                               GROUP BY       SlipNumber, RevisionNumber) AS LE ON H.SlipNumber = LE.SlipNumber AND H.RevisionNumber = LE.RevisionNumber LEFT OUTER JOIN
                        dbo.Employee AS E2 ON LE.EmployeeCode = E2.EmployeeCode LEFT OUTER JOIN
                            (SELECT            SlipNumber, RevisionNumber, MAX(SupplierCode) AS SupplierCode
                               FROM              dbo.ServiceSalesLine AS ServiceSalesLine_1
                               WHERE             (DelFlag = '0') AND (SupplierCode IS NOT NULL) AND (RTRIM(SupplierCode) <> '')
                               GROUP BY       SlipNumber, RevisionNumber) AS LS ON H.SlipNumber = LS.SlipNumber AND H.RevisionNumber = LS.RevisionNumber LEFT OUTER JOIN
                        dbo.Supplier AS S ON LS.SupplierCode = S.SupplierCode LEFT OUTER JOIN
                            (SELECT            PC.PartsNumber, PC.Price, PC.CloseMonth
                               FROM              dbo.PartsAverageCost AS PC INNER JOIN
                                                           (SELECT            PartsNumber, MAX(CloseMonth) AS CM
                                                              FROM              dbo.PartsAverageCost
                                                              GROUP BY       PartsNumber) AS PC2 ON PC2.PartsNumber = PC.PartsNumber AND PC2.CM = PC.CloseMonth) AS PC_1 ON 
                        L.PartsNumber = PC_1.PartsNumber
WHERE             
							(
									H.DelFlag = '0' 
								AND L.DelFlag = '0' 
								AND L.ServiceType = '003' 
								AND P.PartsNumber IS NOT NULL 
								AND L.PartsNumber <> 'DISCNT02' 
								AND (
										L.WorkType <> '015' OR L.WorkType IS NULL
									) 
								AND 
									(
										(
												H.ServiceOrderStatus >= '003' 
											AND H.ServiceOrderStatus <= '005'
										)
										OR 
										(
											H.ServiceOrderStatus = '006' 
											AND H.SalesDate > GETDATE() 
										)
									)
							)
						OR
							(
									 H.DelFlag = '0' 
								AND  L.DelFlag = '0' 
								AND  L.ServiceType = '002' 
								AND 
								( 
									(
											H.ServiceOrderStatus >= '003' 
										AND H.ServiceOrderStatus <= '005'
									)
									OR 
									(
										H.ServiceOrderStatus = '006' 
										AND H.SalesDate > GETDATE() 
									)
								)
								AND  L.SupplierCode IS NOT NULL
							)




GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[11] 2[29] 3) )"
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
         Begin Table = "H"
            Begin Extent = 
               Top = 11
               Left = 19
               Bottom = 155
               Right = 335
            End
            DisplayFlags = 280
            TopColumn = 69
         End
         Begin Table = "L"
            Begin Extent = 
               Top = 6
               Left = 392
               Bottom = 150
               Right = 634
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "C"
            Begin Extent = 
               Top = 6
               Left = 672
               Bottom = 150
               Right = 929
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "W"
            Begin Extent = 
               Top = 6
               Left = 967
               Bottom = 150
               Right = 1209
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "C1"
            Begin Extent = 
               Top = 182
               Left = 0
               Bottom = 326
               Right = 170
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "C2"
            Begin Extent = 
               Top = 150
               Left = 246
               Bottom = 294
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "P"
            Begin Extent = 
               Top = 150
               Left = 454
               Bottom = 294
               Right = 696
            End
            DisplayFlags = 280
            TopColumn = 0
         End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_PartsWipStock'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'
         Begin Table = "PP"
            Begin Extent = 
               Top = 150
               Left = 734
               Bottom = 294
               Right = 976
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "PP2"
            Begin Extent = 
               Top = 294
               Left = 38
               Bottom = 438
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "E1"
            Begin Extent = 
               Top = 294
               Left = 318
               Bottom = 438
               Right = 560
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "LE"
            Begin Extent = 
               Top = 150
               Left = 1014
               Bottom = 274
               Right = 1198
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "E2"
            Begin Extent = 
               Top = 294
               Left = 598
               Bottom = 438
               Right = 840
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "LS"
            Begin Extent = 
               Top = 276
               Left = 1014
               Bottom = 400
               Right = 1198
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "S"
            Begin Extent = 
               Top = 402
               Left = 878
               Bottom = 546
               Right = 1120
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "PC_1"
            Begin Extent = 
               Top = 438
               Left = 38
               Bottom = 562
               Right = 208
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
      Begin ColumnWidths = 27
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_PartsWipStock'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_PartsWipStock'
GO


