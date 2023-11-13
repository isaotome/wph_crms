USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[W_Make_CustomerClain_Supplier]    Script Date: 2019/02/25 13:29:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- --------------------------------------------------------------------------------
-- 機能：顧客データ整合性チェック
-- 作成日：???
-- 更新日：
--         2019/02/19 yano #3965 不要な処理の削除
--　　　　 2017/02/21 arc yano #3708　最終更新日、最終更新者の更新
--                                    ※最終更新者は「SYS(処理名)」とする
-- --------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[W_Make_CustomerClain_Supplier]
AS
BEGIN


--請求先が空白になっているものは埋める
update 
	Customer 
set 
	  CustomerClaimcode = CustomerCode
	, LastUpdateEmployeeCode = 'SYS(W_Make_CustomerClain_Supplier)'
	, LastUpdateDate = GETDATE() 
where 
	CustomerClaimCode is null		--Mod 2017/02/21 arc yano #3708

update 
	Customer 
set 
	  CustomerClaimcode = CustomerCode
	, LastUpdateEmployeeCode = 'SYS(W_Make_CustomerClain_Supplier)'
	, LastUpdateDate = GETDATE()
where 
	RTRIM(CustomerClaimCode) = ''	--Mod 2017/02/21 arc yano #3708


--Del 2019/01/18 yano #3965
--システム上で登録可能であるため、補完しない

----顧客情報があるのに請求先情報がないものは作成する
--insert into 
--	CustomerClaim
--select 
--	  CustomerClaimCode
--	, CustomerName
--	, case when CorporationType in ('001','002') then '002' else '001' end
--	, '002'
--	, '001'
--	, PostCode
--	, Prefecture
--	, City
--	, Address1
--	, Address2
--	, TelNumber
--	, null
--	, FaxNumber
--	, 'SYS(W_Make_CustomerClain_Supplier)'
--	, GETDATE()
--	, 'SYS(W_Make_CustomerClain_Supplier)'
--	, GETDATE()
--	, '0'
--from
--	Customer 
--where 
--	CustomerCode In 
--	(
--		select 
--			c.customercode 
--		from 
--			Customer c full outer join CustomerClaim cc on c.CustomerClaimCode=cc.CustomerClaimCode
--		where
--			cc.CustomerClaimCode is null
--	)

---- 顧客から仕入先データを作成
--insert into 
--	Supplier
--select 
--	  CustomerCode
--	, left(CustomerName,50)
--	, PostCode
--	, Prefecture
--	, City
--	, Address1
--	, Address2
--	, TelNumber
--	, ''
--	, FaxNumber
--	, left(CustomerName,20) as ContractName
--	, '0'
--	, 'SYS(W_Make_CustomerClain_Supplier)'			--Mod 2017/02/21 arc yano #3708
--	, GETDATE()
--	, 'SYS(W_Make_CustomerClain_Supplier)'			--Mod 2017/02/21 arc yano #3708
--	, GETDATE(),
--	 '0'	
--from
--	customer
--where
--	CustomerCode in 
--	(
--		select 
--			C.CustomerCode 
--		from 
--			Customer C left outer join Supplier S on C.CustomerCode=S.SupplierCode 
--		where
--			S.SupplierCode is null
--	)

----顧客から支払先データを作成
--insert into 
--	SupplierPayment
--select
--	  CustomerCode
--	, left(CustomerName,20)
--	, '003'
--	, '001'
--	, 0
--	, null
--	, 'SYS(W_Make_CustomerClain_Supplier)'	--Mod 2017/02/21 arc yano #3708
--	, GETDATE()
--	, 'SYS(W_Make_CustomerClain_Supplier)'	--Mod 2017/02/21 arc yano #3708
--	, GETDATE()
--	, '0'
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--	, null
--from
--	Customer
--where
--	CustomerCode in 
--	(
--		select 
--			C.CustomerCode 
--		from
--			Customer C left outer join SupplierPayment P on C.CustomerCode=P.SupplierPaymentCode
--		where
--			P.SupplierPaymentCode is null
--	)

-- 引当が完了している車両伝票から顧客情報を取り出し、車両マスタに取り込む
/*
update SalesCar set UserCode=H.CustomerCode
from 
CarSalesHeader H
inner join SalesCar S on H.SalesCarNumber=S.SalesCarNumber
where H.DelFlag='0' and H.SalesOrderStatus in ('003','004','005')
and H.CustomerCode <> S.UserCode
*/

END



GO


