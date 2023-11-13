USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_PartsInventorySummary]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_PartsInventorySummary] as
	select a.DepartmentCode
		 , a.InventoryMonth
		 , a.InventoryStatus
		 , a.LastUpdateDate
		 , isnull(b.DifferentialQuantity, 0) as DifferentialQuantity
		 , c.DepartmentName
		 , c.OfficeCode
		 , d.CompanyCode
		 , e.Name as InventoryStatusName
	 from InventorySchedule a
	 left outer join
	 (
	  select b1.DepartmentCode
	       , b1.InventoryMonth
	       , sum(b1.DifferentialQuantity) as DifferentialQuantity
	    from V_PartsInventoryInProcessSrc b1
	   group by b1.DepartmentCode, b1.InventoryMonth
	 ) b
	   on a.DepartmentCode = b.DepartmentCode
	  and a.InventoryMonth = b.InventoryMonth
	 left outer join Department c
	   on a.DepartmentCode = c.DepartmentCode
	 left outer join Office d
	   on c.OfficeCode = d.OfficeCode
	 left outer join c_InventoryStatus e
	   on a.InventoryStatus = e.Code
	where a.DelFlag = '0'
	  and a.InventoryType = '002'
GO
