USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_PartsAverageCostTarget]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_PartsAverageCostTarget]
as
select 
	ISNULL(inv.StartInventoryMonth,purchase.PurchaseMonth) as TargetMonth,
	ISNULL(inv.CompanyCode,purchase.CompanyCode) as CompanyCode,
	ISNULL(inv.PartsNumber,purchase.PartsNumber) as PartsNumber,
	(ISNULL(inv.Price,0)*isnull(inv.StartQuantity,0)) as StartAmount,
	ISNULL(purchase.Amount,0) as PurchaseAmount,
	isnull(inv.StartQuantity,0) as StartQuantity,
	ISNULL(purchase.Quantity,0) as PurchaseQuantity,
	
	case
		when (ISNULL(purchase.Quantity,0) + isnull(inv.StartQuantity,0)) >0 then
			(ISNULL(inv.Price,0)*isnull(inv.StartQuantity,0)) + (ISNULL(purchase.Amount,0)) / 
			(ISNULL(purchase.Quantity,0)+isnull(inv.StartQuantity,0)) 
		else 0 
	end 
	as AveragePrice
from
(select
DateAdd(m,1,a1.InventoryMonth) as StartInventoryMonth,
a4.CompanyCode,
a1.PartsNumber,
a5.Price,
SUM(isnull(a1.Quantity,0)) as StartQuantity
from
Inventory a1
left join Location a2 on a1.LocationCode=a2.LocationCode
left join Department a3 on a2.DepartmentCode=a3.DepartmentCode
left join Office a4 on a3.OfficeCode=a4.OfficeCode
left join PartsAverageCost a5 on a4.CompanyCode=a5.CompanyCode and a1.InventoryMonth=a5.CloseMonth and a1.PartsNumber=a5.PartsNumber
where a1.PartsNumber is not null 
and a1.InventoryType='002' -- 2011/12/08追加
group by a4.CompanyCode,a1.PartsNumber,a1.InventoryMonth,a5.Price
) inv
full outer join
(select
CONVERT(DateTime,convert(varchar,YEAR(a1.PurchaseDate))+'/'+convert(varchar,MONTH(a1.PurchaseDate))+'/01') as PurchaseMonth,
a3.CompanyCode,
a1.PartsNumber,
sum(a1.Quantity) as Quantity,
SUM(a1.Amount) as Amount
from
PartsPurchase a1
left join Department a2 on a1.DepartmentCode=a2.DepartmentCode
left join Office a3 on a2.OfficeCode=a3.OfficeCode
where PurchaseStatus='002'
group by a3.CompanyCode,a1.PartsNumber,CONVERT(DateTime,convert(varchar,YEAR(a1.PurchaseDate))+'/'+convert(varchar,MONTH(a1.PurchaseDate))+'/01')) purchase
on inv.CompanyCode=purchase.CompanyCode
and inv.PartsNumber=purchase.PartsNumber
and inv.StartInventoryMonth=purchase.PurchaseMonth
GO
