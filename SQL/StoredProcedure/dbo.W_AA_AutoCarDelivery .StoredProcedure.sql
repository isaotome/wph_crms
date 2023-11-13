USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[W_AA_AutoCarDelivery ]    Script Date: 2017/09/22 11:41:51 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[W_AA_AutoCarDelivery ]
AS
BEGIN


--2017/09/22 arc yano #3795 自動納車バッチ　車両伝票の納車日には支払予定日ではなく、納車予定日を設定するように変更
/*
AA販売一括納車処理
	
ロケーションから売上計上部門を自動作成する処理を入れること
*/
--Location Check
update SalesCar
	Set LocationCode='021'
From SalesCar S
	inner join CarSalesHeader H on H.SalesCarNumber=S.SalesCarNumber
	inner join Customer C on H.CustomerCode = C.CustomerCode
where H.DelFlag='0' and C.CustomerType='201' and H.SalesOrderStatus='002' and H.SalesType='004'
	and S.LocationCode is null
--CarSalesHeader

update CarSalesHeader 
set ApprovalFlag='1',
	--SalesOrderDate=P.PaymentPlanDate,SalesDate=P.PaymentPlanDate,
	SalesOrderDate=P.PaymentPlanDate,SalesDate=SalesPlanDate,			--Mod 2017/09/22 arc yano #3795
	LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='SYS(W_AA_AutoCarDelivery )'
from CarSalesHeader H
	inner join CarSalesPayment P on H.SlipNumber=P.SlipNumber and H.RevisionNumber=P.RevisionNumber
	inner join Customer C on H.CustomerCode = C.CustomerCode
where H.DelFlag='0' and P.DelFlag='0' and C.CustomerType='201' and H.SalesOrderStatus='002' and H.SalesType='004'
--CarSalePayment
update CarSalesPayment
	set PaymentPlanDate = DATEADD(d,7,PaymentPlanDate)
From CarSalesPayment P
	inner join CarSalesHeader H on H.SlipNumber=P.SlipNumber
	inner join Customer C on H.CustomerCode = C.CustomerCode
where H.DelFlag='0' and P.DelFlag='0' and C.CustomerType='201' and H.SalesOrderStatus='002' and H.SalesType='004'
--CarPurchaseOrder
update CarPurchaseOrder
Set	PurchaseOrderStatus='1',ReservationStatus='1',PurchasePlanStatus='1',RegistrationStatus='1',
	SalesCarNumber=H.SalesCarNumber,Vin=H.Vin,RegistrationDate=H.SalesOrderDate,
	LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='SYS(W_AA_AutoCarDelivery )'
from CarPurchaseOrder P
	inner join CarSalesHeader H on H.SlipNumber=P.SlipNumber
	inner join Customer C on H.CustomerCode = C.CustomerCode
where H.DelFlag='0' and P.DelFlag='0' and C.CustomerType='201' and H.SalesOrderStatus='002' and H.SalesType='004'
--SalesCar
update SalesCar
Set CarStatus='006',LocationCode=null,OwnerCode=C.CustomerCode,UserCode=c.CustomerCode,
	UserName=C.CustomerName,PossesorName=C.CustomerName,RegistrationDate=H.SalesOrderDate,
	SalesDate=H.SalesDate,LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='SYS(W_AA_AutoCarDelivery )'
from SalesCar S
	inner join CarSalesHeader H on H.SalesCarNumber=S.SalesCarNumber
	inner join Customer C on H.CustomerCode = C.CustomerCode
where H.DelFlag='0' and S.DelFlag='0' and C.CustomerType='201' and H.SalesOrderStatus='002' and H.SalesType='004'
--最後にステータスの処理
update CarSalesHeader 
	set SalesOrderStatus='005'
from CarSalesHeader H
	inner join CarSalesPayment P on H.SlipNumber=P.SlipNumber and H.RevisionNumber=P.RevisionNumber
	inner join Customer C on H.CustomerCode = C.CustomerCode
where H.DelFlag='0' and P.DelFlag='0' and C.CustomerType='201' and H.SalesOrderStatus='002' and H.SalesType='004'



 
END

GO


