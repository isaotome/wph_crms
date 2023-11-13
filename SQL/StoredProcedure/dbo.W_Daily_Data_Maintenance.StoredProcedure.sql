USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[W_Daily_Data_Maintenance]    Script Date: 2019/02/25 13:28:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- ---------------------------------------------------------------------------------------------------------
-- 機能：日次処理
-- 作成日：???
-- 更新日：
--		   2019/02/19 yano #3965 WE版新システム対応・CM/WEによる処理の分岐
--		   2017/11/03 arc yano #3805 車両伝票ステータス修正　バッチで更新した修正履歴が画面に表示されない
--　　　　 2017/02/21 arc yano #3711　最終更新日、最終更新者の更新
--                                    ※最終更新者は「SYS(処理名)」で登録
-- -----------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[W_Daily_Data_Maintenance]
AS
BEGIN

-- 不正なロケーションを修正

--Add 2019/01/18 yano #3965	--DB名を取得
declare @dbName nvarchar(50)
declare @eucdbName nvarchar(50)
declare @SQL nvarchar(MAX) = ''
declare @PARAM nvarchar(1024) = ''
declare @CRLF nvarchar(2) = char(13)+char(10)

select @dbName = DB_NAME()

IF @dbName = 'WPH_DB'
	SET @eucdbName = 'EUCDB'
ELSE IF @dbName = 'WE_DB'
	SET @eucdbName = 'EUCDB_WE'
	

update
	SalesCar
set
 	  LocationCode='999'
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()									--Add 2017/02/21 arc yano #3708
where
	DelFlag='0' and 
	CarStatus='001' and 
	LocationCode is null


---------------------------------
--車両移動：自動入庫処理
---------------------------------
exec W_MakeTransfar

--Del 2019/01/18 yano #3965
---------------------------------
--兼務者への受注承認タスクの移動
---------------------------------
--exec W_Make_ApprovalTasks

--2016/05/16 arc yano #3533 バッチ処理　入金消込チェック処理の停止
---------------------------------
--入金消込フラグのチェック
---------------------------------
--exec W_Make_DepositFLag

---------------------------------
--納車時のロケーション修正、車台番号整合性
---------------------------------
exec W_Check_CarLocation_CarSalesNumber

---------------------------------------
--顧客データコピー　To　請求先、仕入先、支払先
---------------------------------------
exec W_Make_CustomerClain_Supplier

---------------------------------------
--出納帳の顧客コードがNullだったら
---------------------------------------
exec W_Alt_Customer_Journal

----------------------------------------------
--引当が済んだ車両はSalesCarのUserCodeを更新す
----------------------------------------------
--2011/8/4　システムがリリースされているのでバッチの必要はなくなった
--exec W_Alt_Customer_UserCode_in_SalesCar

--------------------------------------------------
-- AA / 依頼廃棄 / FAG作成の受注伝票を納車処理する
--------------------------------------------------
exec W_AutoCarDelivery

--------------------------------------------------
-- たまってきた伝票修正ステータスを解消する
--------------------------------------------------
--Del 2017/11/03 arc yano #3805
/*
update
	W_CarSlipStatusChange 
Set
	  ChangeStatus='2'
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'					--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
where
	ChangeStatus='1' and 
	(
		SalesOrderStatus='002' or 
		SalesOrderStatus='007'
	)
*/
--------------------------------------------------
--タスクのクリア
--------------------------------------------------
exec W_TaskClear

--------------------------------------------------
--AA・社内の売掛データ自動消し込み
--------------------------------------------------
exec W_AA_ReceiptDataClear

--------------------------------------------------
-- サービス伝票にて見積期日が2ヶ月以上前の「無効」
-- なデータは削除する。修正履歴のデータは
-- WPH_DB_Backup に保存する
--------------------------------------------------
EXEC W_Compress_ServiceSalesDatas

---------------------------------
-- 車両担当、サービス担当の登録--
---------------------------------
-- 顧客マスタの担当者、担当拠点を伝票から引っ張って登録する
exec W_AlterCustomerDepartmentEmployeeCode

--引当間違いの修正
update
	  CarSalesHeader
Set
	  Vin = S.Vin
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
from
	CarSalesHeader H inner join 
	SalesCar S on H.SalesCarNumber=S.SalesCarNumber
where
	H.DelFlag='0' and 
	S.DelFlag='0' and 
	H.Vin <> S.Vin

update
	CarPurchaseOrder
set
	  Vin = S.Vin
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
from
	CarPurchaseOrder H inner join SalesCar S on H.SalesCarNumber=S.SalesCarNumber
where 
	H.DelFlag='0' and 
	S.DelFlag='0' and 
	H.Vin <> S.Vin

--サービス伝票とマスタの差異を修正

update
	ServiceSalesHeader
Set
	  SalesCarNumber = S.SalesCarNumber
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
from
	ServiceSalesHeader H inner join SalesCar S on H.Vin = S.Vin
where
	H.SalesCarNumber ='' and 
	H.vin <> '' and 
	H.DelFlag='0' and 
	S.DelFlag='0'

update
	ServiceSalesHeader
set 
	  Vin = S.Vin
	, MorterViecleOfficialCode=S.MorterViecleOfficialCode
	, RegistrationNumberKana=S.RegistrationNumberKana
	, RegistrationNumberPlate=S.RegistrationNumberPlate
	, RegistrationNumberType=S.RegistrationNumberType
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
from
	ServiceSalesHeader H inner join SalesCar S on H.SalesCarNumber=S.SalesCarNumber
where
	( H.Vin <> S.Vin) or 
	(H.MorterViecleOfficialCode<>S.MorterViecleOfficialCode) or 
	(H.RegistrationNumberKana<>S.RegistrationNumberKana) or 
	(H.RegistrationNumberPlate<>S.RegistrationNumberPlate) or 
	(H.RegistrationNumberType<>S.RegistrationNumberType)

-- 顧客の携帯番号と請求先の連絡先２を連動
update
	CustomerClaim
set
	  TelNumber2 = C.MobileNumber
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
from
	Customer C inner join CustomerClaim CC on C.CustomerClaimCode=CC.CustomerClaimCode
where
	( 
		RTRIM(C.MobileNumber)<>'' and 
		C.MobileNumber is not  null
	) and 
	( 
		Rtrim(CC.TelNumber2)='' or 
		CC.TelNumber2 is null
	) 

--フォードサービス入金の計上部門を自動的に222に修正
/*
select JournalDate,SlipNumber,Amount,AccountType,DepartmentCode,Summary from Journal 
where DelFlag='0' and DepartmentCode='282' and AccountType='002'
and datediff(m,JournalDate,getdate())=0
order by JournalDate
*/
--更新


--Mod 2019/01/18 yano #3965
IF @dbName = 'WPH_DB'
BEGIN

	update
		Journal 
	set
		  DepartmentCode='222' 
		, Summary='(282)'+ISNULL(summary,'')
		, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
	where
		DelFlag='0' and 
		DepartmentCode='282' and 
	--and AccountType in ('001','002')
		datediff(m,JournalDate,getdate())<=1

	--Add 2015/01/14 arc yano #3149  CJ藤沢湘南サービス入金の計上部門を自動的にFA藤沢湘南サービスに修正
	update
		Journal 
	set
		  DepartmentCode='292'
		, Summary='(162)'+ISNULL(summary,'')
		, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
	where
		DelFlag='0' and 
		DepartmentCode='162' and
	--and AccountType in ('001','002')
		datediff(m,JournalDate,getdate())<=1
END

-- サービス伝票のロック解除　60分以上ロックしているものだけ
update
	ServiceSalesHeader
set
	  ProcessSessionId = null
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
where
	ProcessSessionId in
	(
		select
			ProcessSessionId
		From
			ProcessSessionControl
		where
			DATEDIFF(n,CreateDate,GETDATE()) > 60
	)

delete From 
	ProcessSessionControl
where
	DATEDIFF(n,CreateDate,GETDATE()) > 60

--------------------
--車両仕入価格の修正
--------------------
exec W_Edit_Carpurchase

--------------------
--サブシステム用　主作業追加されたらこっちもてきなやつ
--------------------

--Mod 2019/01/18 yano #3965
	SET @SQL = ''
	SET @SQL = @SQL +'insert into' + @CRLF
	SET @SQL = @SQL + @eucdbName + '.dbo.AccountClassification' + @CRLF
	SET @SQL = @SQL +'select' + @CRLF
	SET @SQL = @SQL +' W.Classification1' + @CRLF
	SET @SQL = @SQL +' , W.ServiceWorkCode' + @CRLF
	SET @SQL = @SQL +' , ''SYS(W_Daily_Data_Maintenance)''' + @CRLF
	SET @SQL = @SQL +', GETDATE()' + @CRLF
	SET @SQL = @SQL +' , ''SYS(W_Daily_Data_Maintenance)''' + @CRLF
	SET @SQL = @SQL +', GETDATE()' + @CRLF
	SET @SQL = @SQL +', ''0''' + @CRLF
	SET @SQL = @SQL +'from' + @CRLF
	SET @SQL = @SQL + @eucdbName + '.dbo.AccountClassification A right join' + @CRLF
	SET @SQL = @SQL +'ServiceWork W on A.ServiceWorkCode=W.ServiceWorkCode' + @CRLF
	SET @SQL = @SQL +'where' + @CRLF
	SET @SQL = @SQL +'	A.ServiceWorkCode is null and ' + @CRLF
	SET @SQL = @SQL +'  W.DelFlag=''0''' + @CRLF
	
	EXECUTE sp_executeSQL @SQL

--insert into
--	eucdb.dbo.AccountClassification
--select 
--	  W.Classification1
--	, W.ServiceWorkCode
--	, 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
--	, GETDATE()
--	, 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
--	, GETDATE()
--	, '0'
--from 
--	eucdb.dbo.AccountClassification A right join
--	ServiceWork W on A.ServiceWorkCode=W.ServiceWorkCode
--where
--	A.ServiceWorkCode is null and 
--	W.DelFlag='0'
---

--- 入金実績が存在していないのに、売掛残高が0になっているデータの修正
update
	ReceiptPlan
set
	  CompleteFlag='0' 
	, ReceivableBalance=R.Amount
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()									--Add 2017/02/21 arc yano #3708
from
	ReceiptPlan R left join 
	(
		Select 
			*
		from
			Journal
		where
			DelFlag='0'
	) J on left(R.SlipNumber,8)=left(J.SlipNumber,8)
where
	R.DelFlag='0' and 
	R.CompleteFlag='1' and 
	R.ReceivableBalance=0 and 
	J.SlipNumber is null and 
	R.Amount <> 0


-- 数量がNULLになる部品在庫修正
update
	PartsStock
set
	  Quantity=0
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()									--Add 2017/02/21 arc yano #3708
where
	Quantity is null
--Del 2015/03/17 wph uesugi #3161 部品入荷処理時に部品在庫テーブルのDelFlagがnullになるバグ修正
--update PartsStock Set DelFlag='0' where DelFlag is null

--Excel Status Update
--Mod 2019/01/18 yano #3965
SET @SQL = ''
SET @SQL = @SQL +'update' + @CRLF
SET @SQL = @SQL + @eucdbName + '.dbo.ExcelDetail' + @CRLF
SET @SQL = @SQL +'Set' + @CRLF
SET @SQL = @SQL +' MakeStatus=''003''' + @CRLF
SET @SQL = @SQL +'where' + @CRLF
SET @SQL = @SQL +' MakeStatus=''002'' and ' + @CRLF
SET @SQL = @SQL +'datediff(d,CompleteDate,GETDATE()) >=7' + @CRLF
	
EXECUTE sp_executeSQL @SQL

--update
--	eucdb.dbo.ExcelDetail
--Set
--	MakeStatus='003'
--where
--	MakeStatus='002' and 
--	datediff(d,CompleteDate,GETDATE()) >=7

END



GO


