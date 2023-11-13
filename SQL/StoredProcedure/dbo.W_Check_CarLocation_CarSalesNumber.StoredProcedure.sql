USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[W_Check_CarLocation_CarSalesNumber]    Script Date: 2019/09/25 17:04:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- -----------------------------------------------------------------------------------------------------------
-- 機能：納車時のロケーション修正、車台番号整合性
-- 作成日：???
-- 更新日：
--		   2019/08/26 yano #4006 納車時のロケーションチェックバッチ】納車前伝票の車両の自動移動処理の廃止 
--		   2019/02/19 yano #3965 WE版新システム対応（Web.configによる処理の分岐)
--　　　　 2017/02/21 arc yano #3711　最終更新日、最終更新者の更新
--                                    ※最終更新者は「SYS(処理名)」で登録
-- -----------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[W_Check_CarLocation_CarSalesNumber]
AS
BEGIN

--引当処理にVINがなかったらいれちゃう
update 
	CarPurchaseOrder 
set 
	  Vin = M.Vin
	, LastUpdateEmployeeCode = 'SYS(W_Check_CarLocation_CarSalesNumber)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
from 
	CarPurchaseOrder P inner join SalesCar M on P.SalesCarNumber = M.SalesCarNumber
where 
	P.DelFlag='0' and 
	P.ReservationStatus='1' and 
	P.Vin is null and 
	P.SalesCarNumber is not null and 
	P.SlipNumber is not null

--伝票は引当のデータに合わせる
update
	CarSalesHeader 
set 
	  Vin = P.Vin 
	, SalesCarNumber=P.SalesCarNumber
	, LastUpdateEmployeeCode = 'SYS(W_Check_CarLocation_CarSalesNumber)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()										--Add 2017/02/21 arc yano #3708
from
	CarSalesHeader H inner join 
	CarPurchaseOrder P on H.SlipNumber = P.SlipNumber inner join 
	c_SalesOrderStatus Cs on H.SalesOrderStatus = Cs.Code
where
	P.DelFlag='0' and 
	P.ReservationStatus='1' and 
	H.DelFlag='0' and 
	(
		H.Vin = '' or 
		H.SalesCarNumber='' or 
		H.Vin <> P.Vin or 
		H.SalesCarNumber <> P.SalesCarNumber
	)

--Mod 2019/08/26 yano #4006 移動処理廃止
----ロケ違いを車両移動する。
--insert into 
--	Transfer 
--	(
--		  TransferNumber
--		, TransferType
--		, SlipNumber
--		, DepartureLocationCode
--		, DepartureDate
--		, DepartureEmployeeCode
--		, ArrivalLocationCode
--		, ArrivalPlanDate
--		, ArrivalDate
--		, ArrivalEmployeeCode
--		, PartsNumber
--		, SalesCarNumber
--		, Quantity
--		, CreateEmployeeCode
--		, CreateDate
--		, LastUpdateEmployeeCode
--		, LastUpdateDate,DelFlag
--	)
--	select 
--		  right((ROW_NUMBER() over (order by H.salescarnumber)+C.SequenceNumber)*100000001,8)
--		, '001'
--		, h.SlipNumber
--		, s.locationcode
--		, GETDATE()
--		, 'SYS(W_Check_CarLocation_CarSalesNumber)'
--		, h.DepartmentCode
--		, GETDATE()
--		, GETDATE()
--		, 'SYS(W_Check_CarLocation_CarSalesNumber)'
--		, null
--		, h.SalesCarNumber
--		, 1
--		, 'SYS(W_Check_CarLocation_CarSalesNumber)'
--		, GETDATE()
--		, 'SYS(W_Check_CarLocation_CarSalesNumber)'
--		, GETDATE()
--		, '0'	
--	from 
--		CarSalesHeader h inner join 
--		SalesCar S on h.SalesCarNumber=s.SalesCarNumber Cross join 
--	(
--		select
--			SequenceNumber
--		from
--			SerialNumber
--		where
--			SerialCode='Transfer'
--	) C 
--	where
--		h.DelFlag='0' and 
--		s.delflag='0' and 
--		h.SalesOrderStatus='004' and 
--		h.SalesCarNumber <>'' and 
--		s.vin <>'' and 
--		(
--			s.locationcode is null or 
--			h.DepartmentCode <> S.locationcode
--		)

--declare @MaxTransfar as numeric
--set @MaxTransfar = (select MAX(transfernumber) from Transfer)

----Mod 2019/02/19 yano #3965
----移動伝票番号が取得できなかった場合は1を設定
--IF @MaxTransfar is null
--	SET @MaxTransfar = 1

--update
--	SerialNumber
--Set 
--	  SequenceNumber=@MaxTransfar
--	, LastUpdateEmployeeCode = 'SYS(W_Check_CarLocation_CarSalesNumber)'
--	, LastUpdateDate = GETDATE()
--where
--	SerialCode='Transfer'

----ロケーションを無理やり合わせる
--update 
--	SalesCar 
--set
--	  locationcode = h.DepartmentCode
--	, LastUpdateEmployeeCode = 'SYS(W_Check_CarLocation_CarSalesNumber)'
--	, LastUpdateDate = GETDATE()
--from
--	CarSalesHeader h inner join SalesCar S on h.SalesCarNumber=s.SalesCarNumber
--where
--	h.DelFlag='0' and s.delflag='0' and 
--	h.SalesOrderStatus='004' and 
--	h.SalesCarNumber <>'' and 
--	s.vin <>'' and 
--	(
--		s.locationcode is null or 
--		h.DepartmentCode <> S.locationcode
--	)


END




GO


