USE [WPH_DB]
GO
/****** Object:  View [dbo].[WV_T_Doble_Customer]    Script Date: 08/04/2014 09:03:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[WV_T_Doble_Customer]
AS
SELECT                  TOP (100) PERCENT CustomerCode, CustomerRank, CustomerKind, CustomerName, CustomerNameKana, CustomerType, PaymentKind, Sex, Birthday, 
                                  Occupation, CarOwner, PostCode, Prefecture, City, Address1, Address2, TelNumber, FaxNumber, MailAddress, MobileNumber, MobileMailAddress, 
                                  CustomerClaimCode, DmFlag, DmMemo, WorkingCompanyName, WorkingCompanyAddress, WorkingCompanyTelNumber, PositionName, 
                                  CustomerEmployeeName, AccountEmployeeName, DepartmentCode, CarEmployeeCode, ServiceEmployeeCode, FirstReceiptionDate, 
                                  LastReceiptionDate, Memo, CreateEmployeeCode, CreateDate, LastUpdateEmployeeCode, LastUpdateDate, DelFlag, FirstName, LastName, 
                                  FirstNameKana, LastNameKana, CorporationType
FROM                     dbo.Customer
WHERE                   (REPLACE(CustomerName, ' ', '') IN
                                      (SELECT                  CustomerName
                                            FROM                     (SELECT                  REPLACE(CustomerName, ' ', '') AS CustomerName, COUNT(*) AS CNT
                                                                                FROM                     dbo.Customer AS Customer_1
                                                                                WHERE                   (DelFlag = '0')
                                                                                GROUP BY          REPLACE(CustomerName, ' ', '')) AS TBL
                                            WHERE                   (CNT > 1)))
ORDER BY           REPLACE(CustomerName, ' ', ''), CustomerCode
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
         Begin Table = "Customer"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 253
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_T_Doble_Customer'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_T_Doble_Customer'
GO
