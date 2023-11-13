	USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_Edit_Carpurchase ]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[W_Edit_Carpurchase ]
AS
BEGIN
	-----------------------------
	--Firm価格修正(リリースまで）
	-----------------------------
	-- 管理番号の割当
	update 
		eucdb.dbo.CarPurchase
	Set
		SalesCarNumber=S.SalesCarNumber
	from
		eucdb.dbo.CarPurchase P inner join 
		eucdb.dbo.SalesCar S on P.CarPurchaseId=S.CarPurchaseId
	where
		P.SalesCarNumber is null
	
	--ファーム価格の修正
	update
		CarPurchase
	set
		  FirmPrice = B.FirmPrice
		, FirmTax = B.FirmTax 
		, FirmAmount=B.FirmAmount
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()									--Add 2017/02/21 arc yano #3708
	from
		CarPurchase A left join 
		EUCDB.dbo.CarPurchase B on A.SalesCarNumber =B.SalesCarNumber
	where
		A.FirmPrice<>B.FirmPrice and 
		A.FirmPrice+A.FirmTax<>A.FirmAmount and B.DelFlag='0'
	
	update
		CarPurchase
	set
		  FirmPrice = B.FirmPrice
		, FirmTax = B.FirmTax
		, FirmAmount=B.FirmAmount
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	from
		CarPurchase A left join 
		EUCDB.dbo.CarPurchase B on A.SalesCarNumber =B.SalesCarNumber
	where
		A.FirmAmount<>B.FirmAmount and 
		A.FirmPrice+A.FirmTax<>A.FirmAmount and 
		B.DelFlag='0'
	
	-----------------
	-- 変な価格の修正
	-----------------
	-- まずNULLをなくす
	update
		CarPurchase
	Set
		  VehicleAmount=0
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where
		VehicleAmount is null

	update
		CarPurchase
	Set
		 VehiclePrice=0
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where
		VehiclePrice is null
	
	update
		CarPurchase
	Set
		  VehicleTax=0
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where
		VehicleTax is null

	update
		CarPurchase
	Set
		  TotalAmount=0
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where
		TotalAmount is null
	
	update
		CarPurchase
	Set
		  Amount=0
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where
		Amount is null

	update
		CarPurchase
	Set
		  TaxAmount=0
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where
		TaxAmount is null
	
	--本体価格の修正
	update
		CarPurchase
	set
		  VehiclePrice=VehicleAmount-VehicleTax
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where
		VehiclePrice=0 and 
		VehicleAmount<> 0
		
	update
		CarPurchase
	set
		  VehicleAmount=VehiclePrice+VehicleTax
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where
		VehiclePrice <>0 and 
		VehicleAmount= 0

	--合計の修正
	update
		CarPurchase
	set
		  Amount=TotalAmount-TaxAmount
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where
		Amount=0 and 
		TotalAmount<> 0

	update
		CarPurchase
	set
		  TotalAmount=Amount+TaxAmount
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where
		Amount<>0 and 
		TotalAmount= 0
	
	--------------
	-- 全体整合性
	--------------
	-- 仕入価格から車両本体価格を算出する
	--Price
	update
		carpurchase
	set
		  VehiclePrice=isnull(Amount,0)-(isnull(OptionPrice,0)+isnull(DiscountPrice,0)+isnull(FirmPrice,0)+isnull(MetallicPrice,0)+isnull(EquipmentPrice,0)+isnull(RepairPrice,0)+isnull(OthersPrice,0)+isnull(CarTaxAppropriatePrice,0)+isnull(RecyclePrice,0)+isnull(AuctionFeePrice,0))
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where
		isnull(VehiclePrice,0)<>isnull(Amount,0)-(isnull(OptionPrice,0)+isnull(DiscountPrice,0)+isnull(FirmPrice,0)+isnull(MetallicPrice,0)+isnull(EquipmentPrice,0)+isnull(RepairPrice,0)+isnull(OthersPrice,0)+isnull(CarTaxAppropriatePrice,0)+isnull(RecyclePrice,0)+isnull(AuctionFeePrice,0))

	--Tax
	update
		carpurchase
	set
		  VehicleTax = isnull(TaxAmount,0)-(isnull(OptionTax,0)+isnull(DiscountTax,0)+isnull(FirmTax,0)+isnull(MetallicTax,0)+isnull(EquipmentTax,0)+isnull(RepairTax,0)+isnull(OthersTax,0)+isnull(CarTaxAppropriateTax,0)+isnull(AuctionFeeTax,0))
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where 
		isnull(VehicleTax,0) <> isnull(TaxAmount,0)-(isnull(OptionTax,0)+isnull(DiscountTax,0)+isnull(FirmTax,0)+isnull(MetallicTax,0)+isnull(EquipmentTax,0)+isnull(RepairTax,0)+isnull(OthersTax,0)+isnull(CarTaxAppropriateTax,0)+isnull(AuctionFeeTax,0))

	--Amount
	update
		carpurchase
	set
		  VehicleAmount=isnull(TotalAmount,0)-(isnull(OptionAmount,0)+isnull(DiscountAmount,0)+isnull(FirmAmount,0)+isnull(MetallicAmount,0)+isnull(EquipmentAmount,0)+isnull(RepairAmount,0)+isnull(OthersAmount,0)+isnull(CarTaxAppropriateAmount,0)+isnull(RecycleAmount,0)+isnull(AuctionFeeAmount,0))
		, LastUpdateEmployeeCode = 'SYS(W_Edit_Carpurchase)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()								--Add 2017/02/21 arc yano #3708
	where 
		isnull(VehicleAmount,0) <> isnull(TotalAmount,0)-(isnull(OptionAmount,0)+isnull(DiscountAmount,0)+isnull(FirmAmount,0)+isnull(MetallicAmount,0)+isnull(EquipmentAmount,0)+isnull(RepairAmount,0)+isnull(OthersAmount,0)+isnull(CarTaxAppropriateAmount,0)+isnull(RecycleAmount,0)+isnull(AuctionFeeAmount,0))

END
GO
