USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_ReceivableManagement]    Script Date: 2015/06/29 19:40:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[V_ReceivableManagement]
AS
SELECT            R.InventoryMonth, R.SlipType, CN.Name AS SlipTypeName, R.SlipNumber, R.SalesDate, R.DepartmentCode, D.DepartmentName, C.CustomerCode, C.CustomerName, C1.Code, 
                        C1.Name, CC.CustomerClaimCode, CC.CustomerClaimName, R.Amount, R.AtoAmount, R.MaeAmount, R.BalanceAmount
FROM              dbo.ReceivableDetail AS R INNER JOIN
                            (SELECT            SlipNumber, SalesDate, SalesOrderDate, DepartmentCode, CustomerCode, DelFlag
                               FROM              dbo.CarSalesHeader
                               WHERE             (DelFlag = '0')
                               UNION ALL
                               SELECT            SlipNumber, SalesDate, SalesOrderDate, DepartmentCode, CustomerCode, DelFlag
                               FROM              dbo.ServiceSalesHeader
                               WHERE             (DelFlag = '0')) AS H ON H.SlipNumber = R.SlipNumber INNER JOIN
                        dbo.Customer AS C ON H.CustomerCode = C.CustomerCode INNER JOIN
                        dbo.CustomerClaim AS CC ON R.CustomerClaimCode = CC.CustomerClaimCode INNER JOIN
                        dbo.c_CustomerClaimType AS C1 ON CC.CustomerClaimType = C1.Code INNER JOIN
                        dbo.Department AS D ON R.DepartmentCode = D.DepartmentCode INNER JOIN
                            (SELECT            Code, Name
                               FROM              dbo.c_CodeName
                               WHERE             (CategoryCode = '014')) AS CN ON R.SlipType = CN.Code
WHERE             (H.DelFlag = '0') AND (NOT (R.SalesDate < '2012/7/1')) OR
                        (H.DelFlag = '0') AND (NOT (R.BalanceAmount = 0))

GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[34] 4[20] 2[17] 3) )"
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
         Begin Table = "R"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 150
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "H"
            Begin Extent = 
               Top = 6
               Left = 318
               Bottom = 150
               Right = 507
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "C"
            Begin Extent = 
               Top = 6
               Left = 545
               Bottom = 150
               Right = 802
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CC"
            Begin Extent = 
               Top = 6
               Left = 840
               Bottom = 150
               Right = 1082
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "C1"
            Begin Extent = 
               Top = 150
               Left = 38
               Bottom = 294
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "D"
            Begin Extent = 
               Top = 150
               Left = 246
               Bottom = 294
               Right = 488
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CN"
            Begin Extent = 
               Top = 198
               Left = 526
               Bottom = 302
               Right = 696
            End
            DisplayFlags = 280
            TopColumn = 0
         End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ReceivableManagement'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ReceivableManagement'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ReceivableManagement'
GO


