USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_PartsInventoryInProcess]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_PartsInventoryInProcess] as
	select
		a.DepartmentCode
		, e.DepartmentName
		, a.InventoryMonth
		, a.InventoryStatus
		, a.PartsNumber
		, a.LogicalLocationCode as LocationCode
		, a.InventoryId
		, a.LogicalQuantity
		, a.InventoryQuantity as Quantity
		, a.DifferentialQuantity
		, b.LocationName
		, c.PartsNameJp
		, c.MakerCode
		, d.MakerName
	 from V_PartsInventoryInProcessSrc a
	 left outer join Location b
	   on a.LogicalLocationCode = b.LocationCode
	 left outer join Parts c
	   on a.PartsNumber = c.PartsNumber
	 left outer join Maker d
	   on c.MakerCode = d.MakerCode
	 left outer join Department e
	   on a.DepartmentCode=e.DepartmentCode
GO
