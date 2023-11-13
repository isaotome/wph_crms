USE [WPH_DB]
GO
/****** Object:  View [dbo].[WV_PartsInventoryAverage]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[WV_PartsInventoryAverage]
AS
SELECT
	I.InventoryMonth, I.DepartmentCode, D.DepartmentName, I.LocationCode, L.LocationName, I.PartsNumber, 
	CASE WHEN P.PartsNameJp IS NULL THEN '(マスター未登録)' ELSE P.PartsNameJp END AS PartsName, I.Quantity, 
	CASE WHEN A.Price IS NULL THEN ISNULL(p.Cost,0) ELSE A.Price END AS cost, 
	CASE WHEN A.Price IS NULL THEN ISNULL(p.Cost,0)*I.Quantity ELSE A.Price * I.Quantity END AS TotalCost, I.Summary,
	CASE WHEN A.Price IS null then '未計算' else '計算済' end AS AverageCalc
FROM dbo.InventoryStock AS I 
	LEFT OUTER JOIN Parts AS P			ON I.PartsNumber = P.PartsNumber
	INNER JOIN		Department AS D		ON I.DepartmentCode = D.DepartmentCode
	LEFT OUTER JOIN Location AS L		ON I.LocationCode = L.LocationCode
	Left Join		PartsAverageCost as A	ON P.PartsNumber=A.PartsNumber and Year(I.InventoryMonth) = Year(A.CloseMonth) and month(I.InventoryMonth) = month(A.CloseMonth)
WHERE (I.DelFlag = '0') AND (I.InventoryType = '002')
GO
