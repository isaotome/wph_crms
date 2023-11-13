USE [WPH_DB]
GO
/****** Object:  View [dbo].[WV_CarWeightTaxClalc]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[WV_CarWeightTaxClalc]
AS
select 
	SalesCarNumber,Vin,W.CarName,W.CarBrandName,W.CarGradeCode,
	s.CarWeight,s.FirstRegistrationYear,
	Case when S.FirstRegistrationYear is null or S.CarWeight is null or rtrim(S.FirstRegistrationYear)='' or rtrim(S.CarWeight)='' then null else
-- 500
	Case when S.CarWeight <= 500 then
		Case when DATEDIFF(yyyy,convert(date,FirstRegistrationYear+'/01'),GETDATE()) >= 18 then 12600 else
		Case when DATEDIFF(yyyy,convert(date,FirstRegistrationYear+'/01'),GETDATE()) >= 12 then 10000 else 8200
	end end else
-- 1000
	Case when S.CarWeight <= 1000 then
		Case when DATEDIFF(yyyy,convert(date,FirstRegistrationYear+'/01'),GETDATE()) >= 18 then 25200 else
		Case when DATEDIFF(yyyy,convert(date,FirstRegistrationYear+'/01'),GETDATE()) >= 12 then 20000 else 16400
	end end else
-- 1500
	Case when S.CarWeight <= 1500 then
		Case when DATEDIFF(yyyy,convert(date,FirstRegistrationYear+'/01'),GETDATE()) >= 18 then 37800 else
		Case when DATEDIFF(yyyy,convert(date,FirstRegistrationYear+'/01'),GETDATE()) >= 12 then 30000 else 24600
	end end else
-- 2000
	Case when S.CarWeight <= 2000 then
		Case when DATEDIFF(yyyy,convert(date,FirstRegistrationYear+'/01'),GETDATE()) >= 18 then 50400 else
		Case when DATEDIFF(yyyy,convert(date,FirstRegistrationYear+'/01'),GETDATE()) >= 12 then 40000 else 32800
	end end else
-- 2500
	Case when S.CarWeight <= 2500 then
		Case when DATEDIFF(yyyy,convert(date,FirstRegistrationYear+'/01'),GETDATE()) >= 18 then 63000 else
		Case when DATEDIFF(yyyy,convert(date,FirstRegistrationYear+'/01'),GETDATE()) >= 12 then 50000 else 41000
	end end else
-- 3000
	Case when S.CarWeight <= 1000 then
		Case when DATEDIFF(yyyy,convert(date,FirstRegistrationYear+'/01'),GETDATE()) >= 18 then 75600 else
		Case when DATEDIFF(yyyy,convert(date,FirstRegistrationYear+'/01'),GETDATE()) >= 12 then 60000 else 49200
	end end else 0
	end end end end end end end as CalWeightTax,CarWeightTax
from SalesCar S
	inner join WV_CarMaster W on S.CarGradeCode=W.CarGradeCode
where DelFlag='0' and CarStatus='001' and NewUsedType='U'
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
         Begin Table = "S"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 125
               Right = 284
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "W"
            Begin Extent = 
               Top = 6
               Left = 322
               Bottom = 125
               Right = 484
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_CarWeightTaxClalc'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'WV_CarWeightTaxClalc'
GO
