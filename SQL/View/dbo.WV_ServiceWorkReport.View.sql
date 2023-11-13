USE [WPH_DB]
GO
/****** Object:  View [dbo].[WV_ServiceWorkReport]    Script Date: 08/04/2014 09:03:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[WV_ServiceWorkReport]
AS
SELECT                  H.SalesDate, H.SlipNumber, H.RevisionNumber, L.LineNumber, H.ServiceOrderStatus, cS2.Name AS OrderStatusName, L.ServiceWorkCode, w.Name, 
                                  L.ServiceType, cS1.Name AS ServiceTypeName, L.LineContents, CASE WHEN LEFT(ServiceMenuCode, 6) = 'DISCNT' OR
                                  LEFT(PartsNumber, 6) = 'DISCNT' THEN (L.Amount + L.TechnicalFeeAmount) * - 1 ELSE L.Amount + L.TechnicalFeeAmount END AS Amount, L.Cost, 
                                  H.DepartmentCode, D.DepartmentName, H.FrontEmployeeCode, E.EmployeeName, H.CarName, H.Vin
FROM                     dbo.ServiceSalesHeader AS H INNER JOIN
                                  dbo.ServiceSalesLine AS L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber LEFT OUTER JOIN
                                  dbo.c_ServiceType AS cS1 ON cS1.Code = L.ServiceType LEFT OUTER JOIN
                                  dbo.c_ServiceOrderStatus AS cS2 ON cS2.Code = H.ServiceOrderStatus LEFT OUTER JOIN
                                  dbo.ServiceWork AS w ON L.ServiceWorkCode = w.ServiceWorkCode LEFT OUTER JOIN
                                  dbo.Department AS D ON H.DepartmentCode = D.DepartmentCode LEFT OUTER JOIN
                                  dbo.Employee AS E ON H.FrontEmployeeCode = E.EmployeeCode LEFT OUTER JOIN
                                  dbo.CustomerClaim AS C ON L.CustomerClaimCode = C.CustomerClaimCode
WHERE                   (H.DelFlag = '0')
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[32] 4[30] 2[29] 3) )"
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
               Top = 6
               Left = 298
               Bottom = 114
               Right = 580
            End
            DisplayFlags = 280
            TopColumn = 24
         End
         Begin Table = "L"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 260
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cS1"
            Begin Extent = 
               Top = 6
               Left = 618
               Bottom = 114
               Right = 766
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cS2"
            Begin Extent = 
               Top = 114
               Left = 664
               Bottom = 222
               Right = 812
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "w"
            Begin Extent = 
               Top = 144
               Left = 35
               Bottom = 252
               Right = 257
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "D"
            Begin Extent = 
               Top = 126
               Left = 404
               Bottom = 234
               Right = 626
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "E"
            Begin Extent = 
               Top = 222
               Left = 664
               Bottom = 330
               Right = 870
            End
            DisplayFlags = 280
            TopColumn = 0
         E' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_ServiceWorkReport'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'nd
         Begin Table = "C"
            Begin Extent = 
               Top = 145
               Left = 314
               Bottom = 253
               Right = 520
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
      Begin ColumnWidths = 18
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
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 2220
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_ServiceWorkReport'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_ServiceWorkReport'
GO
