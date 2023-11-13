USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_CarInventoryInProcessSrc]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_CarInventoryInProcessSrc] as
	select
		isnull(a.DepartmentCode, b.DepartmentCode) as DepartmentCode
		, isnull(a.InventoryMonth, b.InventoryMonth) as InventoryMonth
		, isnull(a.InventoryStatus, '002') as InventoryStatus
		, isnull(a.SalesCarNumber, b.SalesCarNumber) as SalesCarNumber
		, isnull(a.CarGradeCode, b.CarGradeCode) as CarGradeCode
		, isnull(a.Vin, b.Vin) as Vin
		, isnull(a.CarStatus, b.CarStatus) as CarStatus
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
			  , a3.SalesCarNumber
			  , a3.CarGradeCode
			  , a3.Vin
			  , a3.CarStatus
			  , a4.Quantity
		   from InventorySchedule a1
		   inner join Location a2
		     on a1.DepartmentCode = a2.DepartmentCode
		    and '0' = a2.DelFlag
		   inner join V_CarInventorySrc a4
		     on a1.InventoryMonth = a4.InventoryMonth
		     and a2.LocationCode = a4.LocationCode
		     
		   inner join SalesCar a3
		     on 
		     --a4.LocationCode = a3.LocationCode
		    --and 
		    '0' = a3.DelFlag
		    --and a3.CarStatus in ('001', '002', '003', '004', '005')
		    and a4.SalesCarNumber = a3.SalesCarNumber
		  where a1.DelFlag = '0'
		    and a1.InventoryType = '001'
		    and a1.InventoryStatus in ('001', '002')
		) a
		full outer join
		(
		 select b1.InventoryId
			  , b1.DepartmentCode
			  , b1.InventoryMonth
			  , b1.LocationCode
			  , b1.SalesCarNumber
 			  , b1.Quantity
			  , b2.CarGradeCode
			  , b2.Vin
			  , b2.CarStatus
		   from Inventory b1
		   left outer join SalesCar b2
		     on b1.SalesCarNumber = b2.SalesCarNumber
		  where b1.DelFlag = '0'
		    and b1.InventoryType = '001'
		) b
		 on a.DepartmentCode = b.DepartmentCode
		and a.InventoryMonth = b.InventoryMonth
		and a.LocationCode = b.LocationCode
		and a.SalesCarNumber = b.SalesCarNumber
GO
