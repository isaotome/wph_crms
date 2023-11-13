USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[W_FAG_AutoCarDelivery ]    Script Date: 2021/10/18 18:33:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[W_FAG_AutoCarDelivery ]
AS
BEGIN

--2021/10/18 yano #4106【業販自動納車バッチ（W_FAG_AutoCarDelivery ） 】受注日の設定の仕様変更（受注日は更新しない）
--2019/08/26 yano #4007 【業販伝票自動納車バッチ】自動で進める伝票ステータスを納車済から納車前に変更
--2017/09/22 arc yano #3795 自動納車バッチ　車両伝票の納車日には支払予定日ではなく、納車予定日を設定するように変更
/*
業者販売一括納車処理
*/
	update CarSalesHeader 
	set ApprovalFlag='1',
		--SalesOrderDate=PaymentPlanDate,SalesDate=P.PaymentPlanDate,
		--SalesOrderDate=PaymentPlanDate,	--Mod 2021/10/18 yano #4106
		--SalesDate=SalesPlanDate,			--Mod 2017/09/22 arc yano #3795	--Del 2019/08/26 yano #4007
		LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='SYS(W_FAG_AutoCarDelivery)'
	from CarSalesHeader H
		inner join CarSalesPayment P on H.SlipNumber=P.SlipNumber and H.RevisionNumber=P.RevisionNumber
		inner join Customer C on H.CustomerCode = C.CustomerCode
	where H.DelFlag='0' and P.DelFlag='0' and H.SalesOrderStatus='002' and H.SalesType in ('003','009')
		and H.SalesCarNumber is not null and RTRIM(H.SalesCarNumber) <> ''
	--CarPurchaseOrder
	update CarPurchaseOrder
	Set	PurchaseOrderStatus='1',ReservationStatus='1',PurchasePlanStatus='1',RegistrationStatus='1',
		SalesCarNumber=H.SalesCarNumber,Vin=H.Vin,RegistrationDate=H.SalesOrderDate,
		LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='SYS(W_FAG_AutoCarDelivery)'
	from CarPurchaseOrder P
		inner join CarSalesHeader H on H.SlipNumber=P.SlipNumber
		inner join Customer C on H.CustomerCode = C.CustomerCode
	where H.DelFlag='0' and P.DelFlag='0' and H.SalesOrderStatus='002' and H.SalesType in ('003','009')
		and H.SalesCarNumber is not null and RTRIM(H.SalesCarNumber) <> ''
	
	--Mod 2019/08/26 yano #4007 在庫ステータスを「納車済」→「引当済」ロケーションコードは販売伝票の部門で設定
	--SalesCar
	update SalesCar
	Set CarStatus='003',
		LocationCode=H.DepartmentCode,
		OwnerCode=C.CustomerCode,UserCode=c.CustomerCode,
		UserName=C.CustomerName,PossesorName=C.CustomerName,RegistrationDate=H.SalesOrderDate,
		SalesDate=H.SalesDate,LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='SYS(W_FAG_AutoCarDelivery)'
	from SalesCar S
		inner join CarSalesHeader H on H.SalesCarNumber=S.SalesCarNumber
		inner join Customer C on H.CustomerCode = C.CustomerCode
	where H.DelFlag='0' and S.DelFlag='0' and H.SalesOrderStatus='002' and H.SalesType in ('003','009')
		and H.SalesCarNumber is not null and RTRIM(H.SalesCarNumber) <> ''

	--最後にステータスの処理
	--Mod 2019/08/26 yano #4007 伝票ステータスを「納車済」→「納車前」
	update CarSalesHeader 
	set SalesOrderStatus='004'
	from CarSalesHeader H
		inner join CarSalesPayment P on H.SlipNumber=P.SlipNumber and H.RevisionNumber=P.RevisionNumber
		inner join Customer C on H.CustomerCode = C.CustomerCode
	where H.DelFlag='0' and P.DelFlag='0' and H.SalesOrderStatus='002' and H.SalesType in ('003','009')
		and H.SalesCarNumber is not null and RTRIM(H.SalesCarNumber) <> ''
END


GO


