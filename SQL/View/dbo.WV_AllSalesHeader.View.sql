USE [WPH_DB]
GO
/****** Object:  View [dbo].[WV_AllSalesHeader]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[WV_AllSalesHeader]
AS
Select 
	'0' as SlipType,
	SlipNumber,RevisionNumber,ApprovalFlag,
	CustomerCode,DepartmentCode,
	QuoteDate,QuoteExpireDate,SalesOrderDate,SalesDate,
	SalesCarNumber,Vin,ModelName,ManufacturingYear,
	CarGradeCode,CarBrandName,CarGradeName,CarName,Mileage,
	SubTotalAmount,CostTotalAmount,TotalTaxAmount,GrandTotalAmount,PaymentTotalAmount,
	CreateEmployeeCode,CreateDate,LastUpdateEmployeeCode,LastUpdateDate,DelFlag
From CarSalesHeader
union all
Select 
	'1' as SlipType,
	SlipNumber,RevisionNumber,ApprovalFlag,
	CustomerCode,DepartmentCode,
	QuoteDate,QuoteExpireDate,SalesOrderDate,SalesDate,
	SalesCarNumber,Vin,ModelName,ManufacturingYear,
	CarGradeCode,CarBrandName,CarGradeName,CarName,Mileage,
	SubTotalAmount,CostTotalAmount,TotalTaxAmount,GrandTotalAmount,PaymentTotalAmount,
	CreateEmployeeCode,CreateDate,LastUpdateEmployeeCode,LastUpdateDate,DelFlag
From ServiceSalesHeader
GO
