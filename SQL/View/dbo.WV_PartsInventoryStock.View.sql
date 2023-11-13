USE [WPH_DB]
GO
/****** Object:  View [dbo].[WV_PartsInventoryStock]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[WV_PartsInventoryStock]
AS
SELECT
	I.InventoryMonth, I.DepartmentCode, D.DepartmentName, I.LocationCode, L.LocationName, I.PartsNumber, 
	CASE WHEN P.PartsNameJp IS NULL THEN '(マスター未登録)' ELSE P.PartsNameJp END AS PartsName, I.Quantity, 
	CASE WHEN P.Cost IS NULL THEN 0 ELSE P.Cost END AS cost, 
	CASE WHEN P.Cost IS NULL THEN 0 ELSE P.Cost * I.Quantity END AS TotalCost, I.Summary
FROM dbo.InventoryStock AS I LEFT OUTER JOIN
	Parts AS P ON I.PartsNumber = P.PartsNumber INNER JOIN
	Department AS D ON I.DepartmentCode = D.DepartmentCode LEFT OUTER JOIN
	Location AS L ON I.LocationCode = L.LocationCode
WHERE (I.DelFlag = '0') AND (I.InventoryType = '002')
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[30] 4[31] 2[20] 3) )"
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
         Begin Table = "I"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 11
         End
         Begin Table = "P"
            Begin Extent = 
               Top = 6
               Left = 282
               Bottom = 114
               Right = 488
            End
            DisplayFlags = 280
            TopColumn = 17
         End
         Begin Table = "D"
            Begin Extent = 
               Top = 6
               Left = 526
               Bottom = 114
               Right = 732
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "L"
            Begin Extent = 
               Top = 145
               Left = 391
               Bottom = 253
               Right = 597
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_PartsInventoryStock'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_PartsInventoryStock'
GO
