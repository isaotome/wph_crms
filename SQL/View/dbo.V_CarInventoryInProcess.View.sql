USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_CarInventoryInProcess]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_CarInventoryInProcess] as
	select
		a.DepartmentCode
		, h.DepartmentName
		, a.InventoryMonth
		, a.InventoryStatus
		, a.SalesCarNumber
		, a.CarGradeCode
		, a.Vin
		, a.CarStatus
		, a.LogicalLocationCode as LocationCode
		, a.InventoryId
		, a.InventoryQuantity as Quantity
		, a.DifferentialQuantity
		, b.LocationName
		, c.CarGradeName
		, c.CarCode
		, d.CarName
		, d.CarBrandCode
		, e.CarBrandName
		, e.MakerCode
		, f.MakerName
		, g.Name as CarStatusName
	 from V_CarInventoryInProcessSrc a
	 left outer join Location b
	   on a.LogicalLocationCode = b.LocationCode
	 left outer join CarGrade c
	   on a.CarGradeCode = c.CarGradeCode
	 left outer join Car d
	   on c.CarCode = d.CarCode
	 left outer join Brand e
	   on d.CarBrandCode = e.CarBrandCode
	 left outer join Maker f
	   on e.MakerCode = f.MakerCode
	 left outer join c_CarStatus g
	   on a.CarStatus = g.Code
	 left outer join Department h
	   on a.DepartmentCode=h.DepartmentCode
GO
