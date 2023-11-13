USE [WPH_DB]
GO
/****** Object:  View [dbo].[WV_CarMaster]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[WV_CarMaster]
AS
SELECT                  dbo.Maker.MakerCode, dbo.Maker.MakerName, dbo.Brand.CarBrandCode, dbo.Brand.CarBrandName, dbo.Car.CarCode, dbo.Car.CarName, 
                                  dbo.CarGrade.CarGradeCode, dbo.CarGrade.CarGradeName, dbo.Maker.DelFlag AS MakerDelFlag, dbo.Brand.DelFlag AS BrandDelFlag, 
                                  dbo.Car.DelFlag AS CarDelFlag, dbo.CarGrade.DelFlag AS GradeDelFlag
FROM                     dbo.Brand INNER JOIN
                                  dbo.Maker ON dbo.Brand.MakerCode = dbo.Maker.MakerCode INNER JOIN
                                  dbo.Car ON dbo.Brand.CarBrandCode = dbo.Car.CarBrandCode INNER JOIN
                                  dbo.CarGrade ON dbo.Car.CarCode = dbo.CarGrade.CarCode
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[20] 4[47] 2[1] 3) )"
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
         Begin Table = "Brand"
            Begin Extent = 
               Top = 28
               Left = 272
               Bottom = 187
               Right = 478
            End
            DisplayFlags = 280
            TopColumn = 3
         End
         Begin Table = "Maker"
            Begin Extent = 
               Top = 0
               Left = 31
               Bottom = 108
               Right = 237
            End
            DisplayFlags = 280
            TopColumn = 4
         End
         Begin Table = "Car"
            Begin Extent = 
               Top = 181
               Left = 479
               Bottom = 289
               Right = 685
            End
            DisplayFlags = 280
            TopColumn = 4
         End
         Begin Table = "CarGrade"
            Begin Extent = 
               Top = 21
               Left = 535
               Bottom = 201
               Right = 741
            End
            DisplayFlags = 280
            TopColumn = 35
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_CarMaster'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_CarMaster'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_CarMaster'
GO
