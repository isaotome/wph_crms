USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_CarInventorySrc]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_CarInventorySrc]
as 
select 
	InventoryMonth,
	LocationCode,
	SalesCarNumber,
	Sum(Quantity) as Quantity
from
(
-- 前回棚卸数量を取得
select
	DateAdd(m,1,InventoryMonth) as InventoryMonth,
	LocationCode,
	SalesCarNumber,
	isnull(Quantity,0) as Quantity
from
	InventoryStock
where InventoryType='001'
	
union all

-- 仕入数量を取得
select
	convert(datetime,convert(nvarchar,Year(PurchaseDate)) + '/' + convert(nvarchar,Month(PurchaseDate)) + '/1'),
	PurchaseLocationCode,
	SalesCarNumber,
	1
from
	CarPurchase
where 
	PurchaseStatus='002'
	
union all

-- 納車数量を取得
select
	convert(datetime,convert(nvarchar,Year(SalesDate)) + '/' + convert(nvarchar,Month(SalesDate)) + '/1'),
	(select top(1) LocationCode from Location where DepartmentCode=h.DepartmentCode) AS LocationCode,
	SalesCarNumber,
	-1
from
	CarSalesHeader h
where 
	SalesOrderStatus='005'

union all

-- 出庫数量を取得
select
	convert(datetime,convert(nvarchar,Year(DepartureDate)) + '/' + convert(nvarchar,Month(DepartureDate)) + '/1'),
	DepartureLocationCode,
	SalesCarNumber,
	-1
from
	Transfer
where 
	SalesCarNumber is not null

union all

-- 入庫数量を取得
select
	convert(datetime,convert(nvarchar,Year(ArrivalDate)) + '/' + convert(nvarchar,Month(ArrivalDate)) + '/1'),
	ArrivalLocationCode,
	SalesCarNumber,
	1
from
	Transfer
where 
	SalesCarNumber is not null
	and ArrivalDate is not null
) a 
group by InventoryMonth,LocationCode,SalesCarNumber
having sum(Quantity)<>0
GO
