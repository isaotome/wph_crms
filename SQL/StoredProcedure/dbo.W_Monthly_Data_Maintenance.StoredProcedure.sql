USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_Monthly_Data_Maintenance]    Script Date: 2015/05/29 16:01:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[W_Monthly_Data_Maintenance]
AS
BEGIN

/* 
　　2015/05/29 arc.ookubo
    ・部品棚卸確定処理でPartsAverageCostを更新するように変更した為、当JOBではコメントアウト
    ・また0件削除の処理も不要のため、コメントアウト
	・棚卸リストの自動生成については車両棚卸リストがある為、残す。

/*
	移動平均単価計算
	@InventoryStock　対象となる前月の棚データを参照する
　　@PurchaseMonth　この月で計算をする
*/

Declare @InventoryMonth datetime,@PurchaseMonth datetime
Declare @CompanyCode nvarchar(50)
--計算対象月の1ヶ月前を入れる（前月の数値を確認）
Set @InventoryMonth=DATEADD(m,-2,getdate())
set @CompanyCode='001'
--計算当月の仕入月を入れる
Set @PurchaseMonth = DATEADD(m,1,@InventoryMonth)

delete PartsAverageCostControl where datediff(m,closeMonth,@PurchaseMonth)=0
delete PartsAverageCost where datediff(m,closeMonth,@PurchaseMonth)=0


insert into PartsAverageCostControl
values (@PurchaseMonth,'0')

insert into PartsAverageCost
Select 
	@CompanyCode,@PurchaseMonth,
	ISNULL(X.PartsNumber,P.partsNumber) as PartsNumber,
	Case when Quantity=0 or X.Quantity IS null then isnull(P.cost,0) else Floor(Amount/Quantity) end,
	GETDATE(),GETDATE(),'kamachi.akira',GETDATE(),'kamachi.akira','0'
From Parts P 
full outer join 
	(
	Select 
		PartsNumber,SUM(Amount) as Amount,SUM(Quantity) as Quantity
	From 
		(
		select 
			PartsNumber,
			case when PurchaseType='002' then Quantity*-1 else Quantity end as Quantity,
			case when PurchaseType='002' then Price*Quantity*-1 else Price*Quantity end as Amount
		from PartsPurchase where PurchaseStatus='002'
			and Year(PurchaseDate) = YEAR(@PurchaseMonth)
			and month(PurchaseDate) = month(@PurchaseMonth)
			and DepartmentCode In (
				select DepartmentCode from Department D
					inner join Office O on D.OfficeCode=O.OfficeCode
				where O.CompanyCode=@CompanyCode
				)
		union all
		select 
			I.PartsNumber,I.Quantity ,A.Price*I.Quantity  as Amount
		from InventoryStock I
			inner join PartsAverageCost A on I.InventoryMonth=A.CloseMonth and I.PartsNumber=A.PartsNumber
		where A.CompanyCode=@CompanyCode
			and datediff(m,I.InventoryMonth,@InventoryMonth)=0
			and I.Quantity is not null and A.Price is not null
		) A
	Group By PartsNumber
	) X on P.PartsNumber=X.PartsNumber


---------------------------------
--不要な0件データの削除
---------------------------------

delete PartsStock
from PartsStock P
	inner join Location L on P.LocationCode=L.LocationCode
	inner join (
		select DepartmentCode,PartsNumber from eucdb.dbo.PartsStockNonDisplayList
		group by DepartmentCode,PartsNumber 
		) N on P.PartsNumber=N.PartsNumber and L.DepartmentCode=N.DepartmentCode
where P.Quantity=0

*/

---------------------------------
--棚卸リストの自動生成
---------------------------------
insert into eucdb.dbo.ExcelDetail
select 
	NEWID(),A.ExcelNumber,GETDATE(),'system','自動生成',B.ScriptPath,
	A.ScriptParam,'','001',null,
	RTRIM(convert(char,YEAR(dateadd(m,-1,getdate()))))+'年'
	+RTRIM(convert(char,Month(dateadd(m,-1,getdate()))))+'月度'+B.ExcelName
	+'('+DepartmentName+')','0'
from  eucdb.dbo.MakeInventoryListExcel A
	inner join eucdb.dbo.ExcelMain B on A.ExcelNumber=B.ExcelNumber
	inner join WPH_DB.dbo.Department D on A.DepartmentCode=D.DepartmentCode



END

