USE [WPH_DB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[V_ReceiptTransferJournalReport]
AS
SELECT            J.JournalDate, J.OfficeCode, LEFT(J.OfficeCode, 1) AS OfficeId, J.DepartmentCode, E.JournalCode, '' AS Mark, RIGHT(YEAR(J.JournalDate) * 10000 + MONTH(J.JournalDate) 
                        * 100 + DAY(J.JournalDate), 6) AS JournalDate2, J.DepartmentCode AS DrDepartmentCode, RTRIM(E.Dr_AccountCode) AS DrAccountCode, 
                        CASE WHEN G.CashAccountCode IS NULL THEN 0 ELSE G.CashAccountCode END AS DrAccountSub, 
                        CASE WHEN journaltype = '002' THEN Amount * - 1 ELSE Amount END AS DrAmount, 0 AS DrTaxAmount, 1 AS Flag1, 0 AS DrTaxType, 
                        J.DepartmentCode AS CrDepartmentCode, LEFT(J.AccountCode, 4) AS CrAccountCode, RIGHT(J.AccountCode, 3) AS CrAccountSub, 
                        CASE WHEN journaltype = '002' THEN Amount * - 1 ELSE Amount END AS CrAmount, 0 AS CrTaxAmount, 1 AS Flag2, 0 AS CrTaxType, CASE WHEN J.SlipNumber IS NULL OR
                        RTRIM(J.SlipNumber) = '' THEN LEFT(J.Summary, 40) ELSE LEFT(ISNULL(j.slipnumber, ''), 8) 
                        + LEFT(replace(replace(replace(replace(replace(replace(replace(ISNULL(c.customerclaimname, ''), ' ', ''), '<MFY>', ''), '株式会社', '㈱'), '（株）', '㈱'), '有限会社', '㈲'), 
                        '（有）', '㈲'), '☆初検無料', ''), 10) + ' ' + LEFT(ISNULL(J.Summary, ''), 30 - LEN(LEFT(replace(replace(replace(replace(replace(replace(replace(ISNULL(c.customerclaimname, 
                        ''), ' ', ''), '<MFY>', ''), '株式会社', '㈱'), '（株）', '㈱'), '有限会社', '㈲'), '（有）', '㈲'), '☆初検無料', ''), 10))) END AS Description
FROM              dbo.Journal AS J LEFT OUTER JOIN
                        dbo.Account AS A ON J.AccountCode = A.AccountCode LEFT OUTER JOIN
                        dbo.CustomerClaim AS C ON J.CustomerClaimCode = C.CustomerClaimCode LEFT OUTER JOIN
                        dbo.Department AS D ON J.DepartmentCode = D.DepartmentCode LEFT OUTER JOIN
                        dbo.Office AS O ON J.OfficeCode = O.OfficeCode LEFT OUTER JOIN
                        dbo.c_JournalType AS T ON J.JournalType = T.Code LEFT OUTER JOIN
                        dbo.DepartmentCashAccount AS G ON J.DepartmentCode = G.DepartmentCode CROSS JOIN
                        dbo.AccountCode AS E
WHERE             (J.AccountType = '002') AND (J.DelFlag = '0') AND (C.CustomerClaimType NOT IN ('003', '004', '005', '006'))

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
         Begin Table = "J"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 150
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "A"
            Begin Extent = 
               Top = 150
               Left = 38
               Bottom = 294
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "C"
            Begin Extent = 
               Top = 294
               Left = 38
               Bottom = 438
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "D"
            Begin Extent = 
               Top = 438
               Left = 38
               Bottom = 582
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "O"
            Begin Extent = 
               Top = 582
               Left = 38
               Bottom = 726
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T"
            Begin Extent = 
               Top = 726
               Left = 38
               Bottom = 870
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "G"
            Begin Extent = 
               Top = 726
               Left = 246
               Bottom = 870
               Right = 471
            End
            DisplayFlags = 280
            TopColumn = 0
         End
 ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ReceiptTransferJournalReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'        Begin Table = "E"
            Begin Extent = 
               Top = 870
               Left = 38
               Bottom = 1014
               Right = 223
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ReceiptTransferJournalReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ReceiptTransferJournalReport'
GO


