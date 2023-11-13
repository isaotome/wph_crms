USE [WPH_DB]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[V_CashJournalOutput]
AS
SELECT            X.Lastdate, O.OfficeCode, C.CashAccountCode, O.OfficeName, C.CashAccountName, ISNULL(X.TotalAmount, 0) AS LastMonthBalance, ISNULL(Y.Totalamount, 0) 
                        AS ThisMonthJournal, ISNULL(Z.Totalamount, 0) AS ThisMonthPayment, ISNULL(X.TotalAmount, 0) + ISNULL(Y.Totalamount, 0) - ISNULL(Z.Totalamount, 0) 
                        AS ThisMonthBalance
FROM              dbo.CashAccount AS C INNER JOIN
                        dbo.Office AS O ON C.OfficeCode = O.OfficeCode LEFT OUTER JOIN
                            (SELECT            OfficeCode, CashAccountCode, Jd, SUM(ABS(Amount)) AS Totalamount, CONVERT(datetime, Jd + '01', 120) AS K
                               FROM              (SELECT            OfficeCode, CashAccountCode, CONVERT(VARCHAR(6), JournalDate, 112) AS Jd, Amount
                                                        FROM              dbo.Journal AS J
                                                        WHERE             (DelFlag = '0') AND (AccountType = '001') AND (JournalType = '001') AND (Amount >= 0) OR
                                                                                (DelFlag = '0') AND (AccountType = '001') AND (JournalType = '002') AND (Amount < 0)) AS Jo
                               GROUP BY       OfficeCode, CashAccountCode, Jd) AS Y ON C.OfficeCode = Y.OfficeCode AND C.CashAccountCode = Y.CashAccountCode LEFT OUTER JOIN
                            (SELECT            OfficeCode, CashAccountCode, Jd, SUM(ABS(Amount)) AS Totalamount, CONVERT(datetime, Jd + '01', 120) AS K
                               FROM              (SELECT            OfficeCode, CashAccountCode, CONVERT(VARCHAR(6), JournalDate, 112) AS Jd, Amount
                                                        FROM              dbo.Journal AS J
                                                        WHERE             (DelFlag = '0') AND (AccountType = '001') AND (JournalType = '002') AND (Amount >= 0) OR
                                                                                (DelFlag = '0') AND (AccountType = '001') AND (JournalType = '001') AND (Amount < 0)) AS MP
                               GROUP BY       OfficeCode, CashAccountCode, Jd) AS Z ON C.OfficeCode = Z.OfficeCode AND C.CashAccountCode = Z.CashAccountCode AND Z.K = Y.K LEFT OUTER JOIN
                            (SELECT            OfficeCode, TotalAmount, CashAccountCode, M, MAX(ClosedDate) AS Lastdate, DATEADD(M, 1, CONVERT(datetime, M + '01', 120)) AS K
                               FROM              (SELECT            OfficeCode, CONVERT(VARCHAR(6), ClosedDate, 112) AS M, ClosedDate, TotalAmount, CashAccountCode
                                                        FROM              dbo.CashBalance
                                                        WHERE             (DelFlag = '0') AND (CloseFlag = '1')) AS Cb
                               GROUP BY       OfficeCode, M, CashAccountCode, TotalAmount) AS X ON C.OfficeCode = X.OfficeCode AND C.CashAccountCode = X.CashAccountCode AND X.K = Y.K
WHERE             (C.DelFlag = '0') AND (O.OfficeCode IN
                            (SELECT            OfficeCode
                               FROM              dbo.Department
                               WHERE             (BusinessType IN ('001', '002')))) AND (NOT (ISNULL(X.TotalAmount, 0) = 0 AND ISNULL(Y.Totalamount, 0) = 0 AND ISNULL(Z.Totalamount, 0) = 0)) OR
                        (C.DelFlag = '0') AND (O.OfficeCode IN
                            (SELECT            OfficeCode
                               FROM              dbo.Department AS Department_1
                               WHERE             (BusinessType IN ('001', '002')))) AND (NOT (ISNULL(X.TotalAmount, 0) + ISNULL(Y.Totalamount, 0) - ISNULL(Z.Totalamount, 0) = 0))

GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[21] 4[22] 2[40] 3) )"
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
         Begin Table = "C"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 150
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "O"
            Begin Extent = 
               Top = 6
               Left = 318
               Bottom = 150
               Right = 560
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Y"
            Begin Extent = 
               Top = 6
               Left = 598
               Bottom = 150
               Right = 790
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "X"
            Begin Extent = 
               Top = 6
               Left = 1058
               Bottom = 150
               Right = 1250
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Z"
            Begin Extent = 
               Top = 6
               Left = 828
               Bottom = 150
               Right = 1020
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_CashJournalOutput'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_CashJournalOutput'
GO


