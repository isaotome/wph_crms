USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_PartsInventorySrc]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--
-- 修正履歴(2010/11/01以降)
-- 2010/11/01 QuantityのNULL対応、サービス伝票は納車済み後に更新されることもあるので抽出条件にDelFlagを追加
--
CREATE view [dbo].[V_PartsInventorySrc]
as 
select 
	InventoryMonth,
	LocationCode,
	PartsNumber,
	Sum(isnull(Quantity,0)) as Quantity
from
(
-- 前回棚卸数量を取得
select
	DateAdd(m,1,InventoryMonth) as InventoryMonth,
	LocationCode,
	PartsNumber,
	isnull(Quantity,0) as Quantity
from
	InventoryStock
where InventoryType='002'
	
union all

-- 仕入数量を取得
select
	convert(datetime,convert(nvarchar,Year(PurchaseDate)) + '/' + convert(nvarchar,Month(PurchaseDate)) + '/1') as InventoryMonth,
	LocationCode,
	PartsNumber,
	case PurchaseType 
		when '001' then isnull(Quantity,0)
		when '002' then (-1) * isnull(Quantity,0)
	end as Quantity
from
	PartsPurchase
where 
	PurchaseStatus='002'
	
union all

-- 納車数量を取得
select
	convert(datetime,convert(nvarchar,Year(SalesDate)) + '/' + convert(nvarchar,Month(SalesDate)) + '/1') as InventoryMonth,
	(select LocationCode from Location where DepartmentCode=h.DepartmentCode and LocationType='003') as LocationCode,
	PartsNumber,
	(-1) * isnull(Quantity,0) as Quantity
from
	ServiceSalesLine l left join ServiceSalesHeader h
	on l.SlipNumber=h.SlipNumber
	and l.RevisionNumber=h.RevisionNumber
where 
	h.ServiceOrderStatus='006'
	and l.PartsNumber is not null
	and l.PartsNumber <> ''
	and l.DelFlag='0'	-- 2010/11/01追加
	
union all

-- 出庫数量を取得
select
	convert(datetime,convert(nvarchar,Year(DepartureDate)) + '/' + convert(nvarchar,Month(DepartureDate)) + '/1') as InventoryMonth,
	DepartureLocationCode as LocationCode,
	PartsNumber,
	(-1) * isnull(Quantity,0) as Quantity
from
	Transfer
where 
	PartsNumber is not null

union all

-- 入庫数量を取得
select
	convert(datetime,convert(nvarchar,Year(ArrivalDate)) + '/' + convert(nvarchar,Month(ArrivalDate)) + '/1') as InventoryMonth,
	ArrivalLocationCode as LocationCode,
	PartsNumber,
	isnull(Quantity,0)
from
	Transfer
where 
	PartsNumber is not null
	and ArrivalDate is not null
) a 
group by InventoryMonth,LocationCode,PartsNumber
having sum(isnull(Quantity,0))<>0
GO
