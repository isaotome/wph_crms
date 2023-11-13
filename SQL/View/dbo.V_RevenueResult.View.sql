USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_RevenueResult]    Script Date: 2017/12/21 14:33:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




--Mod 2017/12/11 arc yano #3885 ì¸ã‡é¿ê—ï\é¶Å@ï\é¶ã‡äzèCê≥
--Add 2016/08/19 arc nakayama #3630_Åyêªë¢Åzé‘óºîÑä|ã‡ëŒâû

CREATE VIEW [dbo].[V_RevenueResult]
AS
SELECT 'é‘óº' AS ST
	   , SalesOrderDate
	   , H.SlipNumber
	   , H.DepartmentCode
	   , C.CustomerCode
	   , c.CustomerName
	   , H.MakerName AS Desc1
	   , H.CarName AS Desc2
	   , '' AS Desc3
	   , C1.Code as SalesStatus
	   , C1.Name as SalesStatusName
	   , '' AS ServiceStatus
	   , '' AS ServiceStatusName
	   , R.ReceiptAmount
	   , isnull(R.ReceiptAmount, 0) - (isnull(J.Amount,0)) AS ReceivableBalance	--Mod 2017/12/11 arc yano #3885
	   --, R.ReceivableBalance
	   , J.Amount
	   , J.JournalDate
FROM CarSalesHeader H
LEFT OUTER JOIN Customer C ON H.CustomerCode = c.CustomerCode
LEFT OUTER JOIN c_SalesOrderStatus C1 on H.SalesOrderStatus=C1.Code
LEFT OUTER JOIN (select SlipNumber,Sum(Amount) AS ReceiptAmount,Sum(ReceivableBalance) AS ReceivableBalance from ReceiptPlan where DelFlag='0' AND ReceiptType <> '011' group by SlipNumber) R on H.SlipNumber = R.SlipNumber
LEFT OUTER JOIN (select SlipNumber,Sum(Amount) AS Amount,MAX(JournalDate) AS JournalDate from Journal where DelFlag='0' AND JournalType = '001' AND AccountType <> '011' group by SlipNumber) J on H.SlipNumber = J.SlipNumber
WHERE h.DelFlag = '0'
UNION ALL
SELECT  'ÉTÅ[ÉrÉX' AS ST
		, SalesOrderDate
		, H.SlipNumber
		, H.DepartmentCode
		, C.CustomerCode, c.CustomerName
		, H.CarName AS Desc1
		, '' AS Desc2
		, w.Name AS Desc3
		, '' as SalesStatus
		, '' as SalesStatusName
		, C1.Code as ServiceStatus
		, C1.Name as ServiceStatusName
		, R.ReceiptAmount
		, isnull(R.ReceiptAmount, 0) - (isnull(J.Amount,0)) AS ReceivableBalance	--Mod 2017/12/11 arc yano #3885
		--, R.ReceivableBalance
		, J.Amount
		, J.JournalDate
FROM ServiceSalesHeader AS H
INNER JOIN ServiceSalesLine AS L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber
LEFT OUTER JOIN Customer AS C ON H.CustomerCode = C.CustomerCode
LEFT OUTER JOIN ServiceWork AS w ON L.ServiceWorkCode = w.ServiceWorkCode
LEFT OUTER JOIN c_ServiceOrderStatus C1 on H.ServiceOrderStatus=C1.Code
LEFT OUTER JOIN (select SlipNumber,Sum(Amount) AS ReceiptAmount,Sum(ReceivableBalance) AS ReceivableBalance from ReceiptPlan where DelFlag='0' AND ReceiptType <> '011' group by SlipNumber) R on H.SlipNumber = R.SlipNumber
LEFT OUTER JOIN (select SlipNumber,Sum(Amount) AS Amount,MAX(JournalDate) AS JournalDate from Journal where DelFlag='0' AND JournalType = '001' AND AccountType <> '011'group by SlipNumber) J on H.SlipNumber = J.SlipNumber
WHERE (H.DelFlag = '0') 
  AND L.LineNumber = '1' 





GO


