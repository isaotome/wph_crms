USE [WPH_DB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[V_PettyCashJournalReport]
AS
SELECT            J.OfficeCode, LEFT(J.OfficeCode, 1) AS OfficeId, J.CashAccountCode, E.JournalCode, J.SlipNumber, CH.Rate AS CarSalesRate, SH.Rate AS ServiceSalesRate, J.JournalDate, 
                        CT.Rate AS JournalRate, '' AS Mark, RIGHT(YEAR(J.JournalDate) * 10000 + MONTH(J.JournalDate) * 100 + DAY(J.JournalDate), 6) AS JournalDate2, 
                        J.DepartmentCode AS DrDepartmentCode, CASE WHEN JournalType = '001' THEN RTRIM(E.Dr_AccountCode) ELSE LEFT(j.accountcode, 4) END AS DrAccountCode, 
                        CASE WHEN JournalType = '001' THEN RTRIM(DCA.DepartmentCodeSub) ELSE CASE WHEN LEFT(j.accountcode, 4) = '1120' THEN RTRIM(DCA.DepartmentCodeSub2) 
                        ELSE RIGHT(j.accountcode, 3) END END AS DrAccountSub, J.Amount AS DrAmount, CASE WHEN JournalType = '001' THEN 0 ELSE CASE WHEN LEFT(J.AccountCode, 4) 
                        IN ('7420', '7490') THEN 0 ELSE CASE WHEN LEFT(j.accountcode, 1) IN ('5', '6', '7', '8') THEN Amount ELSE 0 END END END AS DrCalcAmount, J.Amount AS DrTaxAmount, 
                        1 AS Flag1, CASE WHEN JournalType = '001' THEN 0 ELSE CASE WHEN LEFT(J.AccountCode, 4) IN ('7420', '7490') THEN 0 ELSE CASE WHEN LEFT(j.accountcode, 1) IN ('6', '7') 
                        THEN 1 ELSE CASE WHEN LEFT(j.accountcode, 1) IN ('5', '8') THEN 7 ELSE 0 END END END END AS DrTaxType, J.DepartmentCode AS CrDepartmentCode, 
                        CASE WHEN JournalType = '001' THEN LEFT(j.accountcode, 4) ELSE RTRIM(E.Dr_AccountCode) END AS CrAccountCode, CASE WHEN JournalType = '001' AND 
                        LEFT(j.accountcode, 4) = '1120' THEN RTRIM(DCA.DepartmentCodeSub2) ELSE CASE WHEN JournalType = '001' THEN RIGHT(j.accountcode, 3) 
                        ELSE RTRIM(DCA.DepartmentCodeSub) END END AS CrAccountSub, J.Amount AS CrAmount, CASE WHEN JournalType = '001' THEN CASE WHEN LEFT(J.AccountCode, 4) 
                        IN ('7420', '7490') THEN 0 ELSE CASE WHEN LEFT(j.accountcode, 1) IN ('5', '6', '7', '8') THEN Amount ELSE 0 END END ELSE 0 END AS CrCalcAmount, 
                        J.Amount AS CrTaxAmount, 1 AS Flag2, CASE WHEN JournalType = '001' THEN CASE WHEN LEFT(J.AccountCode, 4) IN ('7420', '7490') 
                        THEN 0 ELSE CASE WHEN LEFT(j.accountcode, 1) IN ('6', '7') THEN 1 ELSE CASE WHEN LEFT(j.accountcode, 1) IN ('5', '8') THEN 7 ELSE 0 END END END ELSE 0 END AS CrTaxType,
                         CASE WHEN J.SlipNumber IS NULL OR
                        RTRIM(J.SlipNumber) = '' THEN LEFT(J.Summary, 40) ELSE LEFT(ISNULL(j.slipnumber, ''), 8) 
                        + LEFT(replace(replace(replace(replace(replace(replace(replace(ISNULL(c.customerclaimname, ''), ' ', ''), '<MFY>', ''), '株式会社', '㈱'), '（株）', '㈱'), '有限会社', '㈲'), 
                        '（有）', '㈲'), '☆初検無料', ''), 10) + ' ' + LEFT(ISNULL(J.Summary, ''), 30 - LEN(LEFT(replace(replace(replace(replace(replace(replace(replace(ISNULL(c.customerclaimname, 
                        ''), ' ', ''), '<MFY>', ''), '株式会社', '㈱'), '（株）', '㈱'), '有限会社', '㈲'), '（有）', '㈲'), '☆初検無料', ''), 10))) END AS Description
FROM              dbo.Journal AS J LEFT OUTER JOIN
                        dbo.Account AS A ON J.AccountCode = A.AccountCode LEFT OUTER JOIN
                        dbo.CustomerClaim AS C ON J.CustomerClaimCode = C.CustomerClaimCode LEFT OUTER JOIN
                        dbo.Department AS D ON J.DepartmentCode = D.DepartmentCode LEFT OUTER JOIN
                        dbo.Office AS O ON J.OfficeCode = O.OfficeCode LEFT OUTER JOIN
                        dbo.c_JournalType AS T ON J.JournalType = T.Code CROSS JOIN
                        dbo.AccountCode AS E LEFT OUTER JOIN
                            (SELECT            OfficeCode, OfficeCashAccountCode, DepartmentCodeSub, DepartmentCodeSub2
                               FROM              dbo.DepartmentCashAccount
                               GROUP BY       OfficeCode, OfficeCashAccountCode, DepartmentCodeSub, DepartmentCodeSub2) AS DCA ON J.OfficeCode = DCA.OfficeCode AND 
                        J.CashAccountCode = DCA.OfficeCashAccountCode LEFT OUTER JOIN
                        dbo.CarSalesHeader AS CH ON J.SlipNumber = CH.SlipNumber AND CH.DelFlag = '0' LEFT OUTER JOIN
                        dbo.ServiceSalesHeader AS SH ON J.SlipNumber = SH.SlipNumber AND SH.DelFlag = '0' LEFT OUTER JOIN
                        dbo.ConsumptionTax AS CT ON CT.ConsumptionTaxId <> '001' AND CT.DelFlag = '0' AND J.JournalDate BETWEEN CT.FromAvailableDate AND CT.ToAvailableDate
WHERE             (J.AccountType = '001') AND (J.Amount <> 0) AND (J.DelFlag = '0')

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
         Begin Table = "E"
            Begin Extent = 
               Top = 726
               Left = 246
               Bottom = 870
               Right = 431
            End
            DisplayFlags = 280
            TopColumn = 0
         End
 ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_PettyCashJournalReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'        Begin Table = "CH"
            Begin Extent = 
               Top = 1014
               Left = 38
               Bottom = 1158
               Right = 376
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "SH"
            Begin Extent = 
               Top = 1158
               Left = 38
               Bottom = 1302
               Right = 354
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CT"
            Begin Extent = 
               Top = 1302
               Left = 38
               Bottom = 1446
               Right = 252
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "DCA"
            Begin Extent = 
               Top = 6
               Left = 318
               Bottom = 150
               Right = 543
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_PettyCashJournalReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_PettyCashJournalReport'
GO


