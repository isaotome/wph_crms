USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_PartsInventoryInProcessSrc]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_PartsInventoryInProcessSrc] as
	select
		isnull(a.DepartmentCode, b.DepartmentCode) as DepartmentCode
		, isnull(a.InventoryMonth, b.InventoryMonth) as InventoryMonth
		, isnull(a.InventoryStatus, '002') as InventoryStatus
		, isnull(a.PartsNumber, b.PartsNumber) as PartsNumber
		, a.LocationCode as LogicalLocationCode
 		, a.Quantity as LogicalQuantity
		, b.InventoryId
		, b.LocationCode as InventoryLocationCode
		, b.Quantity as InventoryQuantity
		, abs(isnull(a.Quantity, 0) - isnull(b.Quantity, 0)) as DifferentialQuantity
	from
		(
		 select a1.DepartmentCode
			  , a1.InventoryMonth
			  , a1.InventoryStatus
			  , a1.LastUpdateDate
			  , a2.LocationCode
			  , a3.PartsNumber
 			  , a3.Quantity
		   from InventorySchedule a1
		   inner join Location a2
		     on a1.DepartmentCode = a2.DepartmentCode
		    and '0' = a2.DelFlag
		   inner join V_PartsInventorySrc a3
		     on a2.LocationCode = a3.LocationCode
		     and a1.InventoryMonth = a3.InventoryMonth
		  where a1.DelFlag = '0'
		    and a1.InventoryType = '002'
		    and a1.InventoryStatus in ('001', '002')
		) a
		full outer join
		(
		 select b1.InventoryId
			  , b1.DepartmentCode
			  , b1.InventoryMonth
			  , b1.LocationCode
			  , b1.PartsNumber
 			  , b1.Quantity
		   from Inventory b1
		  where b1.DelFlag = '0'
		    and b1.InventoryType = '002'
		) b
		 on a.DepartmentCode = b.DepartmentCode
		and a.InventoryMonth = b.InventoryMonth
		and a.LocationCode = b.LocationCode
		and a.PartsNumber = b.PartsNumber
GO
