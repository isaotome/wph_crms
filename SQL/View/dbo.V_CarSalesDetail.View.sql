USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_CarSalesDetail]    Script Date: 2014/09/09 18:37:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[V_CarSalesDetail]
AS
SELECT            TOP (100) PERCENT H.SalesDate, H.SlipNumber, H.SalesCarNumber, H.SalesType, H.Vin, C.CustomerCode, D.DepartmentCode, H.SalesPrice, 
                        H.ShopOptionAmount - ISNULL(L.Amount, 0) AS ShopOptionAmount, H.SalesCostTotalAmount, ISNULL(H.DiscountAmount, 0) * - 1 AS DiscountAmount, ISNULL(H.SalesPrice, 0) 
                        + ISNULL(H.DiscountAmount, 0) * - 1 + H.ShopOptionAmount + H.SalesCostTotalAmount AS SalesTotalAmount, H.CarName, H.CarBrandName, D.AreaCode, C.CustomerType, 
                        TR.ArrivalLocationCode
FROM              dbo.CarSalesHeader AS H INNER JOIN
                        dbo.Department AS D ON H.DepartmentCode = D.DepartmentCode INNER JOIN
                        dbo.Employee AS E ON H.EmployeeCode = E.EmployeeCode INNER JOIN
                        dbo.Customer AS C ON H.CustomerCode = C.CustomerCode INNER JOIN
                        dbo.c_NewUsedType AS Cn ON H.NewUsedType = Cn.Code INNER JOIN
                        dbo.c_SalesOrderStatus AS Cs ON H.SalesOrderStatus = Cs.Code LEFT OUTER JOIN
                            (SELECT            SlipNumber, RevisionNumber, SUM(Amount) AS Amount, SUM(Amount + TaxAmount) AS TotalAmount
                               FROM              dbo.CarSalesLine
                               WHERE             (DelFlag = '0') AND (OptionType = '001') AND (CarOptionCode IN ('AA001', 'AA002'))
                               GROUP BY       SlipNumber, RevisionNumber) AS L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber LEFT OUTER JOIN
                            (SELECT            A.TransferNumber, A.SalesCarNumber, A.ArrivalLocationCode
                               FROM              dbo.Transfer AS A INNER JOIN
                                                           (SELECT            MAX(TransferNumber) AS tranferNumber, SalesCarNumber
                                                              FROM              dbo.Transfer
                                                              GROUP BY       SalesCarNumber) AS B ON A.TransferNumber = B.tranferNumber AND A.SalesCarNumber = B.SalesCarNumber) AS TR ON 
                        H.SalesCarNumber = TR.SalesCarNumber
WHERE             (H.DelFlag = '0') AND (H.SalesOrderStatus IN ('005'))
ORDER BY       H.DepartmentCode, Cn.Name, H.SalesDate
GO


