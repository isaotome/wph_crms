USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_CarPurchaseAgreementReport]    Script Date: 2023/09/28 8:27:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--Mod 2023/09/05  #4162 インボイス対応
CREATE VIEW [dbo].[V_CarPurchaseAgreementReport]
AS
SELECT
	  a1.CarAppraisalId										--査定ID
	, a1.PurchaseAgreementDate								--買取契約日
	, a1.AppraisalPrice										--査定価格
	, a3.SupplierName										--仕入先
	, a3.PostCode											--郵便番号
	, a3.Prefecture											--都道府県
	, a3.City												--市町村
	, a3.Address1											--住所１
	, a3.Address2											--住所２
	, a9.Name AS Occupation									--職業
	, a4.Birthday											--誕生日
	, a3.TelNumber1											--電話番号
	, a4.MobileNumber										--携帯番号
	, a1.MakerName											--メーカー名
	, a1.CarGradeName										--グレード名
	, a1.MorterViecleOfficialCode							--陸運局コード
	, a1.RegistrationNumberType								--ナンバープレート（種別）
	, a1.RegistrationNumberKana								--ナンバープレート（かな）
	, a1.RegistrationNumberPlate							--ナンバープレート（番号）
	, a1.ModelName											--モデル名
	, a1.Vin												--車台番号
	, a1.Mileage											--走行距離
	, a1.MileageUnit										--走行距離・単位
	, a1.ExteriorColorName									--外装色コード
	, b1.BankName											--銀行名
	, b2.BranchName											--支店名
	, a7.DepositKind										--口座種別	
	, a5.AccountNumber										--口座番号	
	, a5.AccountHolderKana									--口座名義人(カナ)
	, a5.AccountHolder										--口座名銀人
	, a1.Cd													--CD
	, a1.Aw													--AW
	, a1.NaviType											--NaviType
	, a2.PurchaseDate										--入庫データ
	, a8.CompanyName										--会社名
	, a7.FullName AS OfficeFullName							--事業所正式名称
	, a1.ModelYear											--モデル年
	, a10.Name AS MileageUnitName							--走行距離単位名
	, a2.Memo AS Memo										--メモ
	, a3.IssueCompanyNumber AS IssueCompanyNumber			--発行事業者登録番号		--Add  2023/09/05  #4162
	, a2.Rate AS Rate										--消費税率					--Add  2023/09/05  #4162
	, CASE WHEN												--リサイクル預託金			--Add  2023/09/05  #4162
		a1.RecycleDeposit IS NOT NULL
	  THEN
		a1.RecycleDeposit
	  ELSE
		a2.RecycleAmount
	  END AS RecycleDeposit
FROM
	dbo.CarAppraisal AS a1 LEFT OUTER JOIN
	dbo.CarPurchase AS a2 ON a1.CarAppraisalId = a2.CarAppraisalId LEFT OUTER JOIN
    dbo.Supplier AS a3 ON a2.SupplierCode = a3.SupplierCode LEFT OUTER JOIN
    dbo.Customer AS a4 ON a3.SupplierCode = a4.CustomerCode LEFT OUTER JOIN
    dbo.SupplierPayment AS a5 ON a3.SupplierCode = a5.SupplierPaymentCode LEFT OUTER JOIN
    dbo.Bank AS b1 ON a5.BankCode = b1.BankCode LEFT OUTER JOIN
    dbo.Branch AS b2 ON a5.BankCode = b2.BankCode AND a5.BranchCode = b2.BranchCode LEFT OUTER JOIN
    dbo.c_DepositKind AS c1 ON a5.DepositKind = c1.Code LEFT OUTER JOIN
    dbo.Department AS a6 ON a1.DepartmentCode = a6.DepartmentCode LEFT OUTER JOIN
    dbo.Office AS a7 ON a6.OfficeCode = a7.OfficeCode LEFT OUTER JOIN
    dbo.Company AS a8 ON a7.CompanyCode = a8.CompanyCode LEFT OUTER JOIN
    dbo.c_Occupation AS a9 ON a4.Occupation = a9.Code LEFT OUTER JOIN
	dbo.c_MileageUnit AS a10 ON a10.Code = a1.MileageUnit

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
         Begin Table = "a1"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 150
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a2"
            Begin Extent = 
               Top = 150
               Left = 38
               Bottom = 294
               Right = 283
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a3"
            Begin Extent = 
               Top = 294
               Left = 38
               Bottom = 438
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a4"
            Begin Extent = 
               Top = 438
               Left = 38
               Bottom = 582
               Right = 295
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a5"
            Begin Extent = 
               Top = 582
               Left = 38
               Bottom = 726
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "b1"
            Begin Extent = 
               Top = 726
               Left = 38
               Bottom = 870
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "b2"
            Begin Extent = 
               Top = 870
               Left = 38
               Bottom = 1014
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
        ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_CarPurchaseAgreementReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N' End
         Begin Table = "c1"
            Begin Extent = 
               Top = 1014
               Left = 38
               Bottom = 1158
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a6"
            Begin Extent = 
               Top = 1158
               Left = 38
               Bottom = 1302
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a7"
            Begin Extent = 
               Top = 1302
               Left = 38
               Bottom = 1446
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a8"
            Begin Extent = 
               Top = 1446
               Left = 38
               Bottom = 1590
               Right = 280
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a9"
            Begin Extent = 
               Top = 1014
               Left = 246
               Bottom = 1158
               Right = 416
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_CarPurchaseAgreementReport'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'V_CarPurchaseAgreementReport'
GO


