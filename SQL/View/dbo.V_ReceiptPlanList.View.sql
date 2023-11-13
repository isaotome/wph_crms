USE [WPH_DB]
GO

/*
	2015/01/27 arc yano #3153 ì¸ã‡é¿ê—ÉäÉXÉgí«â¡ëŒ
	dbo.WV_ReceiptPlanListÇ©ÇÁà⁄çs
*/

USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_ReceiptPlanList]    Script Date: 2015/01/26 17:40:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[V_ReceiptPlanList]
AS
SELECT
	R.ReceiptPlanId, R.SlipNumber, R.ReceiptPlanDate, cR.Code, cR.Name, D.DepartmentCode, D.DepartmentName, 
	C.CustomerClaimCode, C.CustomerClaimName, A.AccountCode, A.AccountName, R.Amount, R.ReceivableBalance, R.CompleteFlag, 
	R.OccurredDepartmentCode, R.ReceiptType
FROM dbo.ReceiptPlan AS R 
	LEFT OUTER JOIN dbo.Department AS D ON R.DepartmentCode = D.DepartmentCode 
	LEFT OUTER JOIN dbo.CustomerClaim AS C ON R.CustomerClaimCode = C.CustomerClaimCode 
	LEFT OUTER JOIN dbo.c_AccountType AS cR ON R.ReceiptType = cR.Code 
	LEFT OUTER JOIN dbo.Account AS A ON R.AccountCode = A.AccountCode
WHERE  (R.DelFlag = '0')

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
         Begin Table = "R"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 3
         End
         Begin Table = "D"
            Begin Extent = 
               Top = 6
               Left = 282
               Bottom = 114
               Right = 488
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "C"
            Begin Extent = 
               Top = 185
               Left = 100
               Bottom = 293
               Right = 306
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cR"
            Begin Extent = 
               Top = 6
               Left = 526
               Bottom = 114
               Right = 674
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "A"
            Begin Extent = 
               Top = 195
               Left = 426
               Bottom = 303
               Right = 632
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
         Output' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ReceiptPlanList'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N' = 720
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ReceiptPlanList'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ReceiptPlanList'
GO


