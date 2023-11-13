USE [WPH_DB]
GO

/*
	2015/01/27 arc yano #3153 入金実績リスト追加対
	dbo.WV_ALL_SalesOrderListから移行
*/

/****** Object:  View [dbo].[V_ALL_SalesOrderList]    Script Date: 2015/01/26 17:43:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[V_ALL_SalesOrderList]
AS
SELECT
	'車' AS ST, '0' AS STD, SalesOrderDate, SlipNumber, 
	H.DepartmentCode, C.CustomerCode, CustomerName, CustomerNameKana, 
	H.MakerName AS Desc1, H.CarName AS desc2,
	C1.Code as SalesStatus,	C1.Name as SalesStatusName,SalesDate
FROM CarSalesHeader H 
	LEFT OUTER JOIN Customer C ON H.CustomerCode = c.CustomerCode
	Left OUTER JOIN c_SalesOrderStatus C1 on H.SalesOrderStatus=C1.Code
WHERE h.DelFlag = '0'
UNION ALL
SELECT
	'サ' AS ST, '1' AS STD, SalesOrderDate, H.SlipNumber, 
	H.DepartmentCode, C.CustomerCode, c.CustomerName, CustomerNameKana,
	w.Name AS desc1, H.CarName AS desc2,
	C1.Code as SalesStatus,	C1.Name as SalesStatusName,SalesDate
FROM ServiceSalesHeader AS H 
	INNER JOIN ServiceSalesLine AS L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber 
	LEFT OUTER JOIN Customer AS C ON H.CustomerCode = C.CustomerCode 
	LEFT OUTER JOIN ServiceWork AS w ON L.ServiceWorkCode = w.ServiceWorkCode
	left Join c_ServiceOrderStatus C1 on H.ServiceOrderStatus=C1.Code
WHERE (H.DelFlag = '0') AND L.LineNumber = '1' 



GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[19] 4[16] 2[47] 3) )"
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ALL_SalesOrderList'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_ALL_SalesOrderList'
GO


