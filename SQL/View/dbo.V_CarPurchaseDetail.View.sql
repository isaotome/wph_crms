USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_CarPurchaseDetail]    Script Date: 2014/09/09 18:37:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[V_CarPurchaseDetail]
AS
SELECT            	    TOP (100) PERCENT P.PurchaseDate, P.SalesCarNumber, P.PurchaseLocationCode, P.CarPurchaseType, L.LocationName, D.DepartmentCode, D.BrandStoreCode, C.NewUsedType, 
                        Cn.Name, C.Vin, CM.MakerName, CM.CarBrandName, CM.CarName, CM.CarGradeCode, S.SupplierCode, P.VehiclePrice, P.VehicleTax, P.VehicleAmount, P.AuctionFeePrice, 
                        P.AuctionFeeTax, P.AuctionFeeAmount, P.RecyclePrice, P.CarTaxAppropriatePrice, P.CarTaxAppropriateTax, P.CarTaxAppropriateAmount, P.Amount, P.TaxAmount, P.TotalAmount, 
                        E.EmployeeName, E2.EmployeeName AS Expr1, P.CancelFlag
FROM              dbo.CarPurchase AS P INNER JOIN
                        dbo.SalesCar AS C ON P.SalesCarNumber = C.SalesCarNumber LEFT OUTER JOIN
                        dbo.Location AS L ON P.PurchaseLocationCode = L.LocationCode LEFT OUTER JOIN
                        dbo.Department AS D ON P.DepartmentCode = D.DepartmentCode LEFT OUTER JOIN
                        dbo.Supplier AS S ON P.SupplierCode = S.SupplierCode LEFT OUTER JOIN
                        dbo.c_NewUsedType AS Cn ON C.NewUsedType = Cn.Code LEFT OUTER JOIN
                        dbo.WV_CarMaster AS CM ON C.CarGradeCode = CM.CarGradeCode LEFT OUTER JOIN
                        dbo.Employee AS E ON P.EmployeeCode = E.EmployeeCode LEFT OUTER JOIN
                        dbo.Employee AS E2 ON P.CreateEmployeeCode = E2.EmployeeCode
WHERE             (P.DelFlag = '0')
ORDER BY       P.PurchaseDate
GO


