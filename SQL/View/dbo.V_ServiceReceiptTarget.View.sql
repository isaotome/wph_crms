USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_ServiceReceiptTarget]    Script Date: 2019/06/04 15:23:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- -------------------------------------------------------------------------------------------------------------------
-- 機能：サービス承り書
-- 作成日：???
-- 更新日：
--		  2019/05/22 yano #3988 【サービス受付】承り書（車両修正・受付票）出力時に顧客住所が表示されない　類似対応
-- ------------------------------------------------------------------------------------------------------------------
CREATE VIEW [dbo].[V_ServiceReceiptTarget]
AS
SELECT
	  a.CustomerCode
	, a.CustomerName
	, a.CustomerNameKana
	, ISNULL(a.Prefecture, '') + ISNULL(a.City, '') AS CustomerAddress		--Mod  2019/05/22 yano #3988
	, a.TelNumber
	, a.MobileNumber
	, a.FirstReceiptionDate
	, a.LastReceiptionDate
	, b.SalesCarNumber
	, b.Vin
	, b.MorterViecleOfficialCode
	, b.RegistrationNumberType
	, b.RegistrationNumberKana
	, b.RegistrationNumberPlate
	, b.ModelName
	, c.CarGradeName
	, d.CarName
	, f.MakerName
	, g.EmployeeName
	, (CASE WHEN a.CustomerCode IS NULL THEN '2' ELSE '1' END) AS CustomerNameCtrl
	, (CASE WHEN f.MakerName IS NULL THEN '2' ELSE '1' END) AS MakerNameCtrl
	, (CASE WHEN d .CarName IS NULL THEN '2' ELSE '1' END) AS CarNameCtrl
	, (CASE WHEN c.CarGradeName IS NULL THEN '2' ELSE '1' END) AS CarGradeNameCtrl
	, (CASE WHEN b.RegistrationNumberPlate IS NULL THEN '2' ELSE '1' END) AS RegistrationNumberPlateCtrl
	, (CASE WHEN b.Vin IS NULL THEN '2' ELSE '1' END) AS VinCtrl
FROM
	dbo.Customer AS a LEFT OUTER JOIN
	dbo.SalesCar AS b ON a.CustomerCode = b.UserCode AND ISNULL(b.DelFlag, N'0') = '0' LEFT OUTER JOIN
	dbo.CarGrade AS c ON b.CarGradeCode = c.CarGradeCode LEFT OUTER JOIN
	dbo.Car AS d ON c.CarCode = d.CarCode LEFT OUTER JOIN
	dbo.Brand AS e ON d.CarBrandCode = e.CarBrandCode LEFT OUTER JOIN
	dbo.Maker AS f ON e.MakerCode = f.MakerCode LEFT OUTER JOIN
	dbo.Employee AS g ON a.ServiceEmployeeCode = g.EmployeeCode
WHERE
	(ISNULL(a.DelFlag, N'0') = '0')


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
         Begin Table = "a"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 150
               Right = 295
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "b"
            Begin Extent = 
               Top = 150
               Left = 38
               Bottom = 294
               Right = 316
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "c"
            Begin Extent = 
               Top = 294
               Left = 38
               Bottom = 438
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "d"
            Begin Extent = 
               Top = 438
               Left = 38
               Bottom = 582
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "e"
            Begin Extent = 
               Top = 582
               Left = 38
               Bottom = 726
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "f"
            Begin Extent = 
               Top = 726
               Left = 38
               Bottom = 870
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "g"
            Begin Extent = 
               Top = 870
               Left = 38
               Bottom = 1014
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
 ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ServiceReceiptTarget'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'     End
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ServiceReceiptTarget'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ServiceReceiptTarget'
GO


