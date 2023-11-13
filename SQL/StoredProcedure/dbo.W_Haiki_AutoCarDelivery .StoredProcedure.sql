USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[W_Haiki_AutoCarDelivery ]    Script Date: 2017/09/22 11:43:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[W_Haiki_AutoCarDelivery ]
AS
BEGIN

--2017/09/22 arc yano #3795 自動納車バッチ　車両伝票の納車日には支払予定日ではなく、納車予定日を設定するように変更
/*
	依頼廃棄一括処理
*/
--Location Check
update SalesCar
Set LocationCode='021'
From SalesCar S
	inner join CarSalesHeader H on H.SalesCarNumber=S.SalesCarNumber
	inner join Customer C on H.CustomerCode = C.CustomerCode
where H.DelFlag='0' and C.CustomerType='202' and H.SalesOrderStatus='002' and H.SalesType='008'
	and S.LocationCode is null
--CarSalesHeader
update CarSalesHeader 
set ApprovalFlag='1',
	--SalesOrderDate=P.PaymentPlanDate,SalesDate=P.PaymentPlanDate,
	SalesOrderDate=P.PaymentPlanDate,SalesDate=SalesPlanDate,	--Mod 2017/09/22 arc yano #3795
	--DepartmentCode=left(S.LocationCode,3), Del 2017/05/30_#3635 arc nakayama【車】依廃分の車両伝票の納車処理時更新内容修正
	LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='SYS(W_Haiki_AutoCarDelivery)'
from CarSalesHeader H
	inner join CarSalesPayment P on H.SlipNumber=P.SlipNumber and H.RevisionNumber=P.RevisionNumber
	inner join Customer C on H.CustomerCode = C.CustomerCode
	left outer join SalesCar S on H.SalesCarNumber = s.SalesCarNumber
where H.DelFlag='0' and P.DelFlag='0' and C.CustomerType='202' and H.SalesOrderStatus='002' and H.SalesType='008'
--CarSalePayment
/*
update CarSalesPayment
set PaymentPlanDate = DATEADD(d,7,PaymentPlanDate)
From CarSalesPayment P
inner join CarSalesHeader H on H.SlipNumber=P.SlipNumber
inner join Customer C on H.CustomerCode = C.CustomerCode
where H.DelFlag='0' and P.DelFlag='0' and C.CustomerType='202' and H.SalesOrderStatus='002' and H.SalesType='008'
*/
--CarPurchaseOrder
update CarPurchaseOrder
Set	PurchaseOrderStatus='1',ReservationStatus='1',PurchasePlanStatus='1',RegistrationStatus='1',
	SalesCarNumber=H.SalesCarNumber,Vin=H.Vin,RegistrationDate=H.SalesOrderDate,
	LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='SYS(W_Haiki_AutoCarDelivery)'
from CarPurchaseOrder P
	inner join CarSalesHeader H on H.SlipNumber=P.SlipNumber
	inner join Customer C on H.CustomerCode = C.CustomerCode
where H.DelFlag='0' and P.DelFlag='0' and C.CustomerType='202' and H.SalesOrderStatus='002' and H.SalesType='008'
--SalesCar
update SalesCar
Set CarStatus='006',LocationCode=null,OwnerCode=C.CustomerCode,UserCode=c.CustomerCode,
	UserName=C.CustomerName,PossesorName=C.CustomerName,RegistrationDate=H.SalesOrderDate,
	SalesDate=H.SalesDate,LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='SYS(W_Haiki_AutoCarDelivery)'
from SalesCar S
	inner join CarSalesHeader H on H.SalesCarNumber=S.SalesCarNumber
	inner join Customer C on H.CustomerCode = C.CustomerCode
where H.DelFlag='0' and S.DelFlag='0' and C.CustomerType='202' and H.SalesOrderStatus='002' and H.SalesType='008'
--最後にステータスの処理
update CarSalesHeader 
	set SalesOrderStatus='005'
from CarSalesHeader H
	inner join CarSalesPayment P on H.SlipNumber=P.SlipNumber and H.RevisionNumber=P.RevisionNumber
	inner join Customer C on H.CustomerCode = C.CustomerCode
where H.DelFlag='0' and P.DelFlag='0' and C.CustomerType='202' and H.SalesOrderStatus='002' and H.SalesType='008'

END


GO


