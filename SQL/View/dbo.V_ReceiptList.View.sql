USE [WPH_DB]
GO

/*
	2015/01/27 arc yano #3153 ì¸ã‡é¿ê—ÉäÉXÉgí«â¡ëŒ
	dbo.WV_ReceiptListÇ©ÇÁà⁄çs
*/


/****** Object:  View [dbo].[V_ReceiptList]    Script Date: 2015/01/27 9:04:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[V_ReceiptList]
AS
SELECT                  TOP (100) PERCENT J.JournalType, cj.Name AS JournalName, O.OfficeCode, O.OfficeName, J.JournalDate, J.AccountType, ca.Name, J.SlipNumber, 
                                  J.CustomerClaimCode, C.CustomerClaimName, J.AccountCode, A.AccountName, J.Summary, J.DepartmentCode, D.DepartmentName, 
                                  J.Amount
FROM                     dbo.Journal AS J LEFT OUTER JOIN
                                  dbo.Department AS D ON J.DepartmentCode = D.DepartmentCode LEFT OUTER JOIN
                                  dbo.Office AS O ON D.OfficeCode = O.OfficeCode LEFT OUTER JOIN
                                  dbo.c_JournalType AS cj ON cj.Code = J.JournalType LEFT OUTER JOIN
                                  dbo.c_AccountType AS ca ON ca.Code = J.AccountType LEFT OUTER JOIN
                                  dbo.CustomerClaim AS C ON C.CustomerClaimCode = J.CustomerClaimCode LEFT OUTER JOIN
                                  dbo.Account AS A ON A.AccountCode = J.AccountCode
WHERE                   (J.DelFlag = '0') AND (J.JournalType = '001')
ORDER BY           J.JournalDate

GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[24] 4[38] 2[20] 3) )"
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
         Begin Table = "J"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "D"
            Begin Extent = 
               Top = 7
               Left = 294
               Bottom = 115
               Right = 500
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "O"
            Begin Extent = 
               Top = 17
               Left = 528
               Bottom = 125
               Right = 734
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "cj"
            Begin Extent = 
               Top = 142
               Left = 457
               Bottom = 250
               Right = 605
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ca"
            Begin Extent = 
               Top = 222
               Left = 38
               Bottom = 330
               Right = 186
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "C"
            Begin Extent = 
               Top = 222
               Left = 224
               Bottom = 330
               Right = 430
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "A"
            Begin Extent = 
               Top = 330
               Left = 38
               Bottom = 438
               Right = 244
            End
            DisplayFlags = 280
            TopColumn = 0
         End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ReceiptList'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ReceiptList'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ReceiptList'
GO


