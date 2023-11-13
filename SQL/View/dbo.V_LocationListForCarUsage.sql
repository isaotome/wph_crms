USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_LocationListForCarUsage]    Script Date: 2018/05/11 15:39:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- 2018/04/05 arc yano #3878 車両用途変更　ロケーションマスタのダイアログにJLR店舗のロケーションが表示されない
-- 2017/07/04 arc yano #3736 車両移動入力　サービス部門のロケーションが選択できない

CREATE view [dbo].[V_LocationListForCarUsage]
as
	select 
		 D.DepartmentCode as LocationCode
		,DepartmentName as LocationName
	From 
		Department D inner join 
		Location L on D.DepartmentCode=L.LocationCode
	where 
		D.BusinessType in ('001','002') and
		D.BrandStoreCode is not null and									--Mod 2018/04/05 arc yano #3878
		D.BrandStoreCode <> '' and
		D.BrandStoreCode <> '0' and
		--LEFT(D.DepartmentCode,1) in ('1','2','3','4', '5') and 
		D.DelFlag='0'		
	union all
	select 
		 L.LocationCode
		,L.LocationName 
	from 
		Location L
	where 
		L.LocationType='001' and 
		EXISTS																--Mod 2018/04/05 arc yano #3878
		(
			select 
				'x' 
			from 
				dbo.DepartmentWarehouse dw inner join
				dbo.Department d on dw.DepartmentCode = d.DepartmentCode
			where
				d.BrandStoreCode = '0' and
				d.DelFlag = '0' and
				l.WarehouseCode = dw.WarehouseCode
		) and
		--LEFT(DepartmentCode,1)='0' and 
		L.DelFlag = '0'


GO


