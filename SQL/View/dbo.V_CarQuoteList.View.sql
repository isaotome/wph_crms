USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_CarQuoteList]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[V_CarQuoteList]
AS
SELECT                  TOP (100) PERCENT dbo.CarSalesHeader.QuoteDate, dbo.CarSalesHeader.SlipNumber, dbo.CarSalesHeader.RevisionNumber, 
                                  dbo.Customer.CustomerName, dbo.c_CustomerRank.Name AS CustomerRank, dbo.c_CustomerType.Name AS CustomerType, 
                                  dbo.CarSalesHeader.SalesCarNumber, dbo.c_NewUsedType.Name AS NewUsedType, dbo.CarSalesHeader.CarBrandName, 
                                  dbo.CarSalesHeader.CarName, dbo.CarSalesHeader.CarGradeName, dbo.CarSalesHeader.Vin, dbo.CarSalesHeader.ManufacturingYear, 
                                  dbo.Employee.EmployeeName, dbo.CarSalesHeader.SalesPlanDate, dbo.CarSalesHeader.SalesPrice, 
                                  dbo.CarSalesHeader.DiscountAmount, dbo.CarSalesHeader.ShopOptionAmount, dbo.CarSalesHeader.MakerOptionAmount, 
                                  dbo.CarSalesHeader.OutSourceAmount, dbo.CarSalesHeader.SalesCostTotalAmount, dbo.CarSalesHeader.TaxFreeTotalAmount, 
                                  dbo.CarSalesHeader.CostTotalAmount, dbo.CarSalesHeader.TotalTaxAmount, dbo.CarSalesHeader.GrandTotalAmount
FROM                     dbo.Company LEFT OUTER JOIN
                                  dbo.Office ON dbo.Company.CompanyCode = dbo.Office.CompanyCode LEFT OUTER JOIN
                                  dbo.Department ON dbo.Office.OfficeCode = dbo.Department.OfficeCode LEFT OUTER JOIN
                                  dbo.Employee ON dbo.Department.DepartmentCode = dbo.Employee.DepartmentCode LEFT OUTER JOIN
                                  dbo.CarSalesHeader ON dbo.Employee.EmployeeCode = dbo.CarSalesHeader.EmployeeCode LEFT OUTER JOIN
                                  dbo.Customer ON dbo.CarSalesHeader.CustomerCode = dbo.Customer.CustomerCode LEFT OUTER JOIN
                                  dbo.c_SalesOrderStatus ON dbo.CarSalesHeader.SalesOrderStatus = dbo.c_SalesOrderStatus.Code LEFT OUTER JOIN
                                  dbo.c_CustomerType ON dbo.Customer.CustomerType = dbo.c_CustomerType.Code LEFT OUTER JOIN
                                  dbo.c_NewUsedType ON dbo.CarSalesHeader.NewUsedType = dbo.c_NewUsedType.Code LEFT OUTER JOIN
                                  dbo.c_CustomerRank ON dbo.Customer.CustomerRank = dbo.c_CustomerRank.Code
WHERE                   (dbo.CarSalesHeader.SalesOrderStatus = '001')
ORDER BY           dbo.CarSalesHeader.SlipNumber, dbo.CarSalesHeader.RevisionNumber
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[30] 4[21] 2[31] 3) )"
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
         Top = -243
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Company"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 121
               Right = 245
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Office"
            Begin Extent = 
               Top = 6
               Left = 283
               Bottom = 121
               Right = 490
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Department"
            Begin Extent = 
               Top = 6
               Left = 528
               Bottom = 121
               Right = 735
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Employee"
            Begin Extent = 
               Top = 126
               Left = 38
               Bottom = 241
               Right = 245
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CarSalesHeader"
            Begin Extent = 
               Top = 126
               Left = 283
               Bottom = 241
               Right = 550
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Customer"
            Begin Extent = 
               Top = 246
               Left = 38
               Bottom = 361
               Right = 254
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "c_SalesOrderStatus"
            Begin Extent = 
               Top = 126
               Left = 588
               Bottom = 241
               Right = 737
            End
        ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_CarQuoteList'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'    DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "c_CustomerType"
            Begin Extent = 
               Top = 246
               Left = 292
               Bottom = 361
               Right = 441
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "c_NewUsedType"
            Begin Extent = 
               Top = 246
               Left = 479
               Bottom = 361
               Right = 628
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "c_CustomerRank"
            Begin Extent = 
               Top = 366
               Left = 38
               Bottom = 481
               Right = 187
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
      Begin ColumnWidths = 26
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_CarQuoteList'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_CarQuoteList'
GO
