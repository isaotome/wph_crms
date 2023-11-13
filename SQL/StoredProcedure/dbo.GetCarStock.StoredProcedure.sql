USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCarStock]    Script Date: 2018/12/11 12:42:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- ==============================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2015/01/30 arc yano 車両管理対応 車両管理データ取得をlinqからストプロに変更
-- Update date

-- 2018/11/07 yano #3922 車両管理表(タマ表)　その他修正
-- 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善　全面的に改修
-- 2018/06/06 arc yano #3883 タマ表改善 財務価格列追加
-- 2017/08/21 arc yano #3782 車両キャンセル機能追加 仕入キャンセル列追加
-- 2017/03/01 arc yano #3659 ※暫定対応　当月実棚が複数ロケーションの場合はあとで更新されたものを持ってくる。
-- 2016/11/30 arc yano #3659 車両管理 当月実棚欄を追加
-- 2015/04/04 arc yano 車両管理対応
--					仕入減少(他ディーラ)削除、他勘定受入、リサイクル料の追加
--					自社登録の元車両データを取得する際は、DelFlag<> '1'をみない
--2015/02/18 arc yano 車両管理対応 在庫データの在庫拠点が表示されない
-- ==============================================================================================================================================
CREATE PROCEDURE [dbo].[GetCarStock] 
	 @StrTargetMonth NVARCHAR(7)				--指定月
	,@ActionFlag int = 0						--動作指定(0:画面表示, 1:スナップショット保存)
	,@DataType NVARCHAR(3)						--データ種別
	,@NewUsedType NVARCHAR(3) = NULL			--新中区分
	,@SupplierCode NVARCHAR(10) = NULL			--仕入先コード
	,@SalesCarNumber NVARCHAR(50) = NULL		--管理番号
	,@Vin NVARCHAR(20) = NULL					--車台番号
	,@EmployeeCode NVARCHAR(50) = NULL			--担当者
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-----------------------
	--以降は共通
	-----------------------
	--変数宣言
	DECLARE @NOW DATETIME = GETDATE()
	DECLARE @TODAY DATETIME
	DECLARE @THISMONTH DATETIME
	--指定月をdatetime型に変更
	DECLARE @TargetMonth datetime = CONVERT(datetime, @StrTargetMonth + '/01')

	--処理結果
	DECLARE @ErrorMessage NVARCHAR(4000) = ''
	DECLARE @ErrorNumber INT = 0

	
	--■一時表の削除
	/*************************************************************************/
	IF OBJECT_ID(N'tempdb..#temp_CodeList', N'U') IS NOT NULL						--コードリスト
	DROP TABLE #temp_CodeListt;														
	IF OBJECT_ID(N'tempdb..#temp_CarriedBalance', N'U') IS NOT NULL					--前月繰越リスト
	DROP TABLE #temp_CarriedBalance;										
	IF OBJECT_ID(N'tempdb..#temp_Transfer1', N'U') IS NOT NULL						--移動リスト①
	DROP TABLE #temp_Transfer1;
	IF OBJECT_ID(N'tempdb..#temp_Transfer2', N'U') IS NOT NULL						--移動リスト②
	DROP TABLE #temp_Transfer2;
	IF OBJECT_ID(N'tempdb..#temp_CarPurchaseDetail', N'U') IS NOT NULL				--車両仕入リスト
	DROP TABLE #temp_CarPurchaseDetail;
	IF OBJECT_ID(N'tempdb..#temp_CarPurchaseCancel', N'U') IS NOT NULL				--車両仕入キャンセル
	DROP TABLE #temp_CarPurchaseCancel;
	IF OBJECT_ID(N'tempdb..#temp_TradeVin', N'U') IS NOT NULL						--下取車伝票情報
	DROP TABLE #temp_TradeVin;
	IF OBJECT_ID(N'tempdb..#temp_Option', N'U') IS NOT NULL							--AAオプション情報
	DROP TABLE #temp_Option;
	IF OBJECT_ID(N'tempdb..#temp_AkaSlipList', N'U') IS NOT NULL					--赤伝票リスト
	DROP TABLE #temp_AkaSlipList;
	IF OBJECT_ID(N'tempdb..#temp_KuroSlipList', N'U') IS NOT NULL					--黒伝票リスト
	DROP TABLE #temp_KuroSlipList;
	IF OBJECT_ID(N'tempdb..#temp_CarSalesDetail', N'U') IS NOT NULL					--販売リスト
	DROP TABLE #temp_CarSalesDetail;
	IF OBJECT_ID(N'tempdb..#temp_SalesCar', N'U') IS NOT NULL						--車両情報						
	DROP TABLE #temp_SalesCar;
	IF OBJECT_ID(N'tempdb..#temp_InventoryStockCar', N'U') IS NOT NULL				--当月棚卸
	DROP TABLE #temp_InventoryStockCar;

	--■■一時表の宣言
	/*************************************************************************/
	--コードリスト
	CREATE TABLE #temp_CodeList (
		[SalesCarNumber] NVARCHAR(50) NOT NULL			-- 車両管理番号
	)
	CREATE INDEX ix_temp_CodeList ON #temp_CodeList (SalesCarNumber)

	--前月繰越リスト
	CREATE TABLE #temp_CarriedBalance (
		 [SalesCarNumber] nvarchar(50) NOT NULL			--車両管理番号
		,[BrandStore] nvarchar(50)						--取扱ブランド
		,[PurchaseDate] datetime						--入庫日
		,[PurchaseLocationCode] nvarchar(12)			--仕入ロケーションコード
		,[PurchaseLocationName] nvarchar(50)			--仕入ロケーション名
		,[CarPurchaseType] nvarchar(3)					--仕入区分
		,[SupplierCode] nvarchar(10)					--仕入先コード
		,[EndInventory] decimal(10, 0)					--月末在庫
		,[RecycleAmount] decimal(10, 0)					--リサイクル料金
		,[PurchaseAmount] decimal(10, 0)				--仕入金額
		,[CarPurchaseTypeName] nvarchar(50)				--仕入区分名
		,[SupplierName] nvarchar(80)					--仕入先名
		,[SelfRegistrationPurchaseDate] datetime		--自社登録前の時の入庫日
		,[MakerName] nvarchar(100)						--メーカー名
		,[CarName] nvarchar(100)						--車種名
	)
	CREATE INDEX ix_temp_CarriedBalance ON #temp_CarriedBalance (SalesCarNumber)

	--移動テーブル①	
	CREATE TABLE #temp_Transfer1 (
		SalesCarNumber	NVARCHAR(50)					--車両管理番号
	,	TransferNumber	NVARCHAR(50)
	)
	CREATE INDEX ix_temp_Transfer1 ON #temp_Transfer1 (SalesCarNumber)

	--移動テーブル②
	CREATE TABLE #temp_Transfer2 (
		SalesCarNumber	NVARCHAR(50)					--車両管理番号
	,	ArrivalLocationCode	NVARCHAR(12)
	)
	CREATE INDEX ix_temp_Transfer2 ON #temp_Transfer2 (SalesCarNumber)

	--車両仕入リスト
	CREATE TABLE #temp_CarPurchaseDetail (
		  PurchaseDate datetime							--仕入日
		, SalesCarNumber NVARCHAR(50)					--車両管理番号
		, DepartmentCode NVARCHAR(3)					--部門コード
		, PurchaseLocationCode NVARCHAR(12)				--仕入先ロケーションコード
		, PurchaseLocationName NVARCHAR(50)				--仕入先ロケーション名				--Mod 2016/11/30 arc yano #3659
		, CarPurchaseType NVARCHAR(3)					--仕入種別(コード)
		, CarPurchaseTypeName VARCHAR(50)				--仕入種別(名称)					--Add 2016/11/30 arc yano #3659
		, SupplierCode NVARCHAR(10)						--仕入先コード
		, VehiclePrice DECIMAL(10, 0)					--車両本体価格
		, VehicleTax DECIMAL(10, 0)						--車両本体消費税
		, VehicleAmount DECIMAL(10, 0)					--車両本体税込価格
		, AuctionFeePrice DECIMAL(10, 0)				--オークション落札料
		, AuctionFeeTax DECIMAL(10, 0)					--オークション落札料消費税
		, AuctionFeeAmount DECIMAL(10, 0)				--オークション落札料税込
		, RecycleAmount DECIMAL(10, 0)					--リサイクル金額					--Mod 2015/03/13 arc yano #3164
		, RecyclePrice DECIMAL(10, 0)					--リサイクル価格
		, CarTaxAppropriatePrice DECIMAL(10, 0)			--自税充当価格
		, CarTaxAppropriateTax DECIMAL(10, 0)			--自税充当消費税
		, CarTaxAppropriateAmount DECIMAL(10, 0)		--自税充当金額
		, Amount DECIMAL(10, 0)							--仕入金額
		, TaxAmount DECIMAL(10, 0)						--仕入消費税
		, TotalAmount DECIMAL(10, 0)					--仕入税込価格
		, FinancialAmount DECIMAL(10, 0)				--仕入価格(リサイクル料除く)		--Add 2018/06/06 arc yano #3883
		, OtherAccount DECIMAL(10, 0)					--他勘定受入						--Add 2018/08/28 yano #3922
		, CancelFlag NVARCHAR(2)						--キャンセルフラグ
		, BrandStore NVARCHAR(50)						--取扱ブランド
		, TradeCarSlipNumber NVARCHAR(50)				--下取車の伝票番号
		, OldSalesCarNumber  NVARCHAR(50)				--旧車両管理番号
		, SelfRegistrationPurchaseDate datetime			--自社登録前の時の入庫日
	)
	CREATE INDEX ix_temp_CarPurchaseDetail ON #temp_CarPurchaseDetail (SalesCarNumber)

	--車両仕入キャンセルリスト
	CREATE TABLE #temp_CarPurchaseCancel (
		  CancelDate datetime							--キャンセル日
		, SalesCarNumber NVARCHAR(50)					--車両管理番号
		, CancelAmount DECIMAL(10, 0)					--仕入価格(キャンセル)
	)
	CREATE INDEX ix_temp_CarPurchaseCancel ON #temp_CarPurchaseCancel (SalesCarNumber)

	--下取車の納車日情報
	CREATE TABLE #temp_TradeVin
	 (
		SalesCarNumber	NVARCHAR(50)					--管理番号
	,	SalesDate		DATETIME						--納車日
	)
	CREATE INDEX ix_temp_TradeVin ON #temp_TradeVin (SalesCarNumber)

	--オプション価格
	CREATE TABLE #temp_Option (
			SlipNumber NVARCHAR(50)						--伝票番号
		,	RevisionNumber INT							--改訂番号
		,	Amount DECIMAL(10, 0)						--金額
		,	TotalAmount DECIMAL(10, 0)					--合計金額
	)
	CREATE INDEX ix_temp_Option ON #temp_Option (SlipNumber)

	--赤伝リスト
	CREATE TABLE #temp_AkaSlipList (
		 SlipNumber NVARCHAR(50)
		,SalesCarNumber NVARCHAR(50)
	)
	CREATE INDEX ix_temp_AkaSlipList ON #temp_AkaSlipList (SlipNumber)

	--黒伝リスト
	CREATE TABLE #temp_KuroSlipList (
		 SlipNumber NVARCHAR(50)
		,KuroSlipNumber NVARCHAR(50)
		,SalesCarNumber NVARCHAR(50)
	)
	CREATE INDEX ix_temp_KuroSlipList ON #temp_KuroSlipList (SlipNumber)

	--販売リスト
	CREATE TABLE #temp_CarSalesDetail (
	  SalesDate datetime					--納車日
	, SlipNumber NVARCHAR(50)				--伝票番号
	, SalesCarNumber NVARCHAR(50)			--管理番号
	, SalesType NVARCHAR(3)					--販売区分
	, Vin NVARCHAR(20)						--車台番号
	, CustomerCode NVARCHAR(10)				--顧客コード
	, CustomerName NVARCHAR(80)				--顧客名
	, DepartmentCode NVARCHAR(3)			--部門コード
	, DepartmentName NVARCHAR(20)			--部門名
	, SalesPrice DECIMAL(10,0)				--販売価格
	, ShopOptionAmount DECIMAL(10,0)		--店舗オプション価格
	, SalesCostTotalAmount DECIMAL(10,0)	--諸費用合計
	, DiscountAmount DECIMAL(10,0)			--値引価格
	, SalesTotalAmount DECIMAL(10,0)		--販売合計価格
	, CarName NVARCHAR(50)					--車種名
	, CarBrandName NVARCHAR(50)				--車両ブランド名
	, AreaCode NVARCHAR(3)					--エリアコード
	, CustomerType NVARCHAR(3)				--顧客種別
	, MakerOptionAmount DECIMAL(10,0)		--メーカーオプション価格
	, BrandStore NVARCHAR(50)				--取扱ブランド
	, CustomerTypeName NVARCHAR(50)			--顧客種別名称
	)
	CREATE INDEX ix_temp_CarSalesDetail ON #temp_CarSalesDetail (SalesCarNumber)

	
	--車両マスタテーブル
	CREATE TABLE #temp_SalesCar (
		  SalesCarNumber NVARCHAR(50)
		, MakerName NVARCHAR(100)
		, NewUsedType NVARCHAR(3)
		, NewUsedTypeName VARCHAR(50)						--Add 2016/11/30 arc yano #3659
		, CarName NVARCHAR(100)
		, CarGradeCode NVARCHAR(30)
		, Vin NVARCHAR(20)
		, CarUsage NVARCHAR(3)
		, CompanyRegistrationFlag NVARCHAR(2)
		, BrandStore NVARCHAR(50)
	)
	CREATE INDEX ix_temp_SalesCar ON #temp_SalesCar (SalesCarNumber)

	--当月実棚データ
	CREATE TABLE #temp_InventoryStockCar (
			SalesCarNumber NVARCHAR(50)
		,	LocationCode NVARCHAR(12)
	)
	CREATE INDEX ix_temp_InventoryStockCar ON #temp_InventoryStockCar (SalesCarNumber)

	--車両管理データ
	CREATE TABLE #temp_CarStock(
		 [ProcessDate] datetime
		,[SalesCarNumber] nvarchar(50)
		,[BrandStore]  nvarchar(50)
		,[NewUsedType] nvarchar(3)
		,[PurchaseDate] datetime
		,[CarName] nvarchar(100)
		,[CarGradeCode] nvarchar(20)
		,[Vin] nvarchar(50)
		,[PurchaseLocationCode] nvarchar(12)
		,[CarPurchaseType] nvarchar(3)
		,[SupplierCode] nvarchar(10)
		,[BeginningInventory] decimal(10, 0)
		,[MonthPurchase] decimal(10, 0)
		,[SalesDate] datetime
		,[SlipNumber] nvarchar(50)
		,[SalesType] nvarchar(50)
		,[CustomerCode] nvarchar(50)
		,[SalesPrice] decimal(10, 0)
		,[DiscountAmount] decimal(10, 0)
		,[ShopOptionAmount] decimal(10, 0)
		,[SalesCostTotalAmount] decimal(10, 0)
		,[SalesTotalAmount] decimal(10, 0)
		,[SalesCostAmount] decimal(10, 0)
		,[SalesProfits] decimal(10, 0)
		,[ReductionTotal] decimal(10, 0)
		,[SelfRegistration] decimal(10, 0)
		,[OtherDealer] decimal(10, 0)
		,[DemoCar] decimal(10, 0)
		,[TemporaryCar] decimal(10, 0)
		,[EndInventory] decimal(10, 0)
		,[RecycleAmount] decimal(10, 0)
		,[OtherAccount] decimal(10, 0)
		,[RentalCar] decimal(10, 0)
		,[BusinessCar] decimal(10, 0)
		,[PRCar] decimal(10, 0)
		,[LocationCode] nvarchar(12)
		,[CancelPurchase] decimal(10, 0)
		,[CarPurchaseTypeName] nvarchar(50)
		,[MakerName] nvarchar(100)
		,[PurchaseLocationName] nvarchar(50)
		,[InventoryLocationName] nvarchar(50)
		,[SupplierName] nvarchar(50)
		,[SalesDepartmentCode] nvarchar(3)
		,[SalesDepartmentName] nvarchar(20)
		,[NewUsedTypeName] nvarchar(50)
		,[CustomerName] nvarchar(80)
		,[TradeCarSlipNumber] nvarchar(50)
		,[SelfRegistrationPurchaseDate] datetime
		,[CustomerTypeName] nvarchar(50)
	)
	CREATE UNIQUE INDEX ix_temp_CarStock ON #temp_CarStock (ProcessDate, SalesCarNumber)
	/*************************************************************************/
	--現在日時
	SET @NOW = GETDATE()
	--当月1日
	SET @THISMONTH = CONVERT(DATETIME, CONVERT(NVARCHAR(7), @TODAY, 111) + '/01', 111)

	--■処理対象月範囲の設定
	--※「月次締め処理締め」が全部門締まっている月の中で最大月の翌月の1日<=x<処理当日の翌月1日未満(または、月中の場合は当日未満）
	--■■処理対象月Fromの設定（締まっている月の中で最大月の翌月1日）
	DECLARE @TargetMonthFrom DATETIME = NULL
	
	--動作指定による振分け
	IF @ActionFlag = 0	--表示の場合は、本締め最新月の翌月に設定
	BEGIN
		SELECT 
			@TargetMonthFrom = DATEADD(m, 1, ISNULL(MAX(CONVERT(datetime, cm.[CloseMonth], 120)), @THISMONTH))
		FROM 
			[CloseMonthControlCarStock] cm		--車両管理締テーブル
		WHERE 
			cm.[CloseStatus] = '002'			--締済
	END
	ELSE
	BEGIN
		SET @TargetMonthFrom = @TargetMonth							--スナップショット保存の場合は指定月を設定する。
	END


	--対象月が未来になる場合、当月とする
	IF @TargetMonthFrom > @TODAY
		SET @TargetMonthFrom = @THISMONTH
	
	--■■処理対象月Toの設定(指定月)
	--指定月がNULLの場合、指定月=当月を設定する。　※ロジックとしては通らない
	IF @TargetMonth is null
		SET @TargetMonth = @THISMONTH
	
	--指定月を設定(指定月の翌月1日未満)
	DECLARE @TargetMonthTo DATETIME = DATEADD(m, 1, @TargetMonth)

	--処理対象月数／処理対象月前月
	DECLARE @TargetMonthCount INT = DATEDIFF(m, @TargetMonthFrom, @TargetMonthTo)
	DECLARE @TargetMonthPrevious DATETIME = DATEADD(m, -1, @TargetMonthFrom)

	--処理対象日付範囲From／処理対象日付範囲To
	DECLARE @TargetDateFrom DATETIME = @TargetMonthFrom
	DECLARE @TargetDateTo DATETIME = DATEADD(m, 1, @TargetDateFrom)

	IF @TargetDateTo > @NOW		--日付範囲TOが未来日の場合、現在にする
		SET @TargetDateTo = @NOW

	
	--トランザクション開始 
	BEGIN TRANSACTION
	BEGIN TRY
		--一旦前月分の車両管理表を入れておく
		INSERT INTO
			#Temp_CarStock
		SELECT
		   [ProcessDate]
		  ,[SalesCarNumber]
		  ,[BrandStore]
		  ,[NewUsedType]
		  ,[PurchaseDate]
		  ,[CarName]
		  ,[CarGradeCode]
		  ,[Vin]
		  ,[PurchaseLocationCode]
		  ,[CarPurchaseType]
		  ,[SupplierCode]
		  ,[BeginningInventory]
		  ,[MonthPurchase]
		  ,[SalesDate]
		  ,[SlipNumber]
		  ,[SalesType]
		  ,[CustomerCode]
		  ,[SalesPrice]
		  ,[DiscountAmount]
		  ,[ShopOptionAmount]
		  ,[SalesCostTotalAmount]
		  ,[SalesTotalAmount]
		  ,[SalesCostAmount]
		  ,[SalesProfits]
		  ,[ReductionTotal]
		  ,[SelfRegistration]
		  ,[OtherDealer]
		  ,[DemoCar]
		  ,[TemporaryCar]
		  ,[EndInventory]
		  ,[RecycleAmount]
		  ,[OtherAccount]
		  ,[RentalCar]
		  ,[BusinessCar]
		  ,[PRCar]
		  ,[LocationCode]
		  ,[CancelPurchase]
		  ,[CarPurchaseTypeName]
		  ,[MakerName]
		  ,[PurchaseLocationName]
		  ,[InventoryLocationName]
		  ,[SupplierName]
		  ,[SalesDepartmentCode]
		  ,[SalesDepartmentName]
		  ,[NewUsedTypeName]
		  ,[CustomerName]
		  ,'' AS TradeCarSlipNumber
		  ,[SelfRegistrationPurchaseDate]
		  ,[CustomerTypeName]
		FROM			
			dbo.CarStock
		WHERE
			ProcessDate = @TargetMonthPrevious AND
			DelFlag = '0'	

		--for debug
		--PRINT '前月分を退避'

		--■処理対象月数分ループ
		WHILE @TargetMonthCount > 0
		BEGIN
			--一時表初期化
			DELETE FROM  #temp_CodeList							--コードリスト
			DELETE FROM  #temp_CarriedBalance					--前月繰越リスト
			DELETE FROM  #temp_Transfer1						--移動テーブル１
			DELETE FROM  #temp_Transfer2						--移動テーブル２
			DELETE FROM  #temp_CarPurchaseDetail				--仕入リスト
			DELETE FROM  #temp_CarPurchaseCancel				--仕入キャンセルリスト
			DELETE FROM  #temp_TradeVin							--下取車リスト
			DELETE FROM  #temp_Option							--AAオプション
			DELETE FROM  #temp_AkaSlipList						--赤伝票リスト
			DELETE FROM  #temp_KuroSlipList						--黒伝票リスト
			DELETE FROM  #temp_CarSalesDetail					--販売リスト
			DELETE FROM  #temp_SalesCar							--車両情報リスト
			DELETE FROM  #temp_InventoryStockCar				--当月実棚リスト

			/***************************************************
			■■移動データ
			****************************************************/
			--移動データ
			INSERT INTO #temp_Transfer1
			SELECT 
				SalesCarNumber
			,	MAX(TransferNumber) AS tranferNumber
			FROM 
				dbo.[Transfer]
			WHERE										--Add 2018/08/28 yano #3922
				ArrivalDate >= @TargetDateFrom AND
				ArrivalDate < @TargetDateTo
			GROUP BY
				SalesCarNumber

			DROP INDEX ix_temp_Transfer1 ON  #temp_Transfer1
			CREATE INDEX ix_temp_Transfer1 ON #temp_Transfer1 (SalesCarNumber)

			--対象期間での移動先ロケーションを取得
			INSERT INTO #temp_Transfer2
			SELECT
				A.SalesCarNumber
			,	A.ArrivalLocationCode
			FROM 
				dbo.Transfer AS A 
			WHERE EXISTS(
				SELECT 'X'
				FROM #Temp_Transfer1 x
				WHERE x.TransferNumber = A.TransferNumber
				)

			DROP INDEX ix_temp_Transfer2 ON #temp_Transfer2
			CREATE INDEX ix_temp_Transfer2 ON #temp_Transfer2 (SalesCarNumber)

			--for debug
			--PRINT '移動データ作成'

			/***************************************************
			■■当月仕入データ
			****************************************************/
			--対象月の仕入データを取得する
			INSERT INTO
				#temp_CarPurchaseDetail
				SELECT            	    
					  P.PurchaseDate
					, P.SalesCarNumber
					, P.DepartmentCode
					, P.PurchaseLocationCode
					, L.LocationName AS PurchaseLocationName
					, P.CarPurchaseType
					--仕入区分名
					, CASE 
					  WHEN
						bg.ProcType = '006'	--自社登録
					  THEN
						FORMAT(P.PurchaseDate, 'yyyyMM') + '自社登' 
					  WHEN
						bg.ProcType = '010'	--除却
					  THEN
						'固定資産振替' 
					  ELSE
						cCP.Name
					  END AS CarPurchaseTypeName
					, P.SupplierCode
					, P.VehiclePrice
					, P.VehicleTax
					, P.VehicleAmount
					, P.AuctionFeePrice
					, P.AuctionFeeTax
					, P.AuctionFeeAmount
					, P.RecycleAmount								--2015/03/13 arc yano #3164
					, P.RecyclePrice
					, P.CarTaxAppropriatePrice
					, P.CarTaxAppropriateTax
					, P.CarTaxAppropriateAmount
					, P.Amount
					, P.TaxAmount
					, P.TotalAmount
					--会計上の仕入金額
					, CASE 
					  WHEN
						bg.ProcType = '010'	--除却
					  THEN
						NULL
					  ELSE
						P.FinancialAmount
					  END AS FinancialAmount
					--他勘定受入
					, CASE 
					  WHEN
						bg.ProcType = '010'	--除却
					  THEN
						P.FinancialAmount
					  ELSE
						NULL
					  END AS OtherAccount
					--キャンセルフラグ
					, P.CancelFlag
					--取扱ブランド
					, cn.Name AS BrandStore
					--下取車の伝票番号
					, CASE WHEN
						P.CarPurchaseType = '001'
					  THEN
						P.SlipNumber
					  ELSE
						NULL
					  END AS TradeCarStlipNumber
					--旧車両管理番号(自社登録の場合に設定)
					,CASE WHEN
						bg.ProcType = '006'	--自社登録
					 THEN
						bg.SalesCarNumber	--旧管理番号
					 ELSE
						NULL
					 END AS OldSalesCarNumber
					--自社登録前の車両仕入日
					, NULL AS SelfRegistrationPurchaseDate

				FROM              
					dbo.CarPurchase AS P INNER JOIN
					dbo.Location AS L ON P.PurchaseLocationCode = L.LocationCode LEFT OUTER JOIN
					dbo.c_CarPurchaseType cCP ON P.CarPurchaseType  = cCP.Code LEFT OUTER JOIN
					dbo.ConsumptionTax ct ON P.ConsumptionTaxId = ct.ConsumptionTaxId LEFT JOIN
					dbo.BackGroundDemoCar bg ON P.SalesCarNumber = bg.NewSalesCarNumber LEFT OUTER JOIN
					dbo.Department dp ON P.DepartmentCode = dp.DepartmentCode LEFT OUTER JOIN
					dbo.c_CodeName cn ON cn.CategoryCode = '002' AND dp.BrandStoreCode = cn.Code
				WHERE             
					P.DelFlag = '0' AND
					P.PurchaseStatus = '002' AND															--仕入ステータス=「仕入済」
					P.PurchaseDate >= @TargetDateFrom AND
					P.PurchaseDate < @TargetDateTo
				
				DROP INDEX ix_temp_CarPurchaseDetail ON #temp_CarPurchaseDetail
				CREATE INDEX ix_temp_CarPurchaseDetail ON #temp_CarPurchaseDetail (SalesCarNumber)


			--当月仕入した下取車両の注文書情報を取得する
			INSERT INTO
				#temp_TradeVin
			SELECT
				 sd.SalesCarNumber
				,Max(sd.SalesDate)
			FROM
			(
				SELECT
					 sc.SalesCarNumber
					,sh.SalesDate
				FROM
					dbo.CarSalesHeader sh INNER JOIN
					dbo.SalesCar sc ON sh.TradeInVin1 = sc.Vin and sc.DelFlag = '0'
				WHERE
					sh.TradeInVin1 is not null AND
					sh.TradeInVin1 <> '' AND
					sh.DelFlag = '0' AND
					sh.SalesOrderStatus = '005' AND
					EXISTS
					(
						select 'x' from #temp_CarPurchaseDetail cp where cp.CarPurchaseType = '001' and sc.SalesCarNumber = cp.SalesCarNumber
					)
				UNION
				SELECT
					 sc.SalesCarNumber
					,sh.SalesDate
				FROM
					dbo.CarSalesHeader sh INNER JOIN
					dbo.SalesCar sc ON sh.TradeInVin2 = sc.Vin and sc.DelFlag = '0'
				WHERE
					sh.TradeInVin2 is not null AND
					sh.TradeInVin2 <> '' AND
					sh.DelFlag = '0' AND
					sh.SalesOrderStatus = '005' AND
					EXISTS
					(
						select 'y' from #temp_CarPurchaseDetail cp where cp.CarPurchaseType = '001' and sc.SalesCarNumber = cp.SalesCarNumber
					)
				UNION
				SELECT
					 sc.SalesCarNumber
					,sh.SalesDate
				FROM
					dbo.CarSalesHeader sh INNER JOIN
					dbo.SalesCar sc ON sh.TradeInVin3 = sc.Vin and sc.DelFlag = '0'
				WHERE
					sh.TradeInVin3 is not null AND
					sh.TradeInVin3 <> '' AND
					sh.DelFlag = '0' AND
					sh.SalesOrderStatus = '005' AND
					EXISTS
					(
						select 'z' from #temp_CarPurchaseDetail cp where cp.CarPurchaseType = '001' and sc.SalesCarNumber = cp.SalesCarNumber
					)
			) sd
			GROUP BY
				 sd.SalesCarNumber
			
			DROP INDEX ix_temp_TradeVin ON #temp_TradeVin
			CREATE INDEX ix_temp_TradeVin ON #temp_TradeVin (SalesCarNumber)

			--for debug
			--PRINT '下取情報作成'

			--下取車の注文伝票の納車日以前に仕入を行っている場合は、仕入区分名は「下取車（先取）」とする
			UPDATE
				#temp_CarPurchaseDetail
			SET
				CarPurchaseTypeName = '下取車(先取)'
			FROM
				#temp_CarPurchaseDetail cp
			WHERE
				EXISTS
				(
					select 'x' from #temp_TradeVin tv where (tv.SalesDate is NULL OR tv.SalesDate > cp.PurchaseDate) AND cp.SalesCarNumber = tv.SalesCarNumber
				)

			--自社登録前の車両の仕入日を設定する
			UPDATE
				#temp_CarPurchaseDetail
			SET
				SelfRegistrationPurchaseDate = cp.PurchaseDate
			FROM
				#temp_CarPurchaseDetail tmcp INNER JOIN
				dbo.CarPurchase cp ON cp.DelFlag = '0' and cp.PurchaseStatus = '002' and tmcp.OldSalesCarNumber = cp.SalesCarNumber
				

			--for debug
			--PRINT '下取（先取）作成'
			/***************************************************************
			■■当月仕入キャンセルデータ
			****************************************************************/
			--対象月の仕入データを取得する
			INSERT INTO
				#temp_CarPurchaseCancel
			SELECT
				 cp.CancelDate
				,cp.SalesCarNumber
				,cp.FinancialAmount
			FROM
				dbo.CarPurchase cp
			WHERE
				cp.DelFlag = '0' AND	
				cp.CancelDate < @TargetDateTo AND
				cp.CancelDate >= @TargetDateFrom AND
				cp.PurchaseStatus = '003'
			
			DROP INDEX ix_temp_CarPurchaseCancel ON #temp_CarPurchaseCancel
			CREATE INDEX ix_temp_CarPurchaseCancel ON #temp_CarPurchaseCancel (SalesCarNumber)	

			--for debug
			--PRINT '仕入キャンセル作成'

			/**************************************************************
			■■当月販売データ
			***************************************************************/
			 --AA金額取得
			 INSERT INTO #temp_Option
			 SELECT
				 sl.SlipNumber
				,sl.RevisionNumber
				,SUM(ISNULL(sl.Amount, 0)) AS Amount
				,SUM(ISNULL(sl.Amount, 0) + ISNULL(TaxAmount, 0)) AS TotalAmount
			FROM
				dbo.CarSalesLine sl
			WHERE 
				sl.DelFlag = '0' AND 
				sl.OptionType = '001' AND 
				sl.CarOptionCode IN ('AA001', 'AA002') AND
				EXISTS
				(
					select 
						'x' 
					from 
						dbo.CarsalesHeader sh 
					where 
						sh.DelFlag = '0' and 
						sh.SalesOrderStatus = '005' and 
						sh.SalesDate >= @TargetDateFrom and 
						sh.SalesDate < @TargetDateTo and 
						sh.SlipNumber = sl.SlipNumber and 
						sh.RevisionNumber = sl.RevisionNumber
				)
			GROUP BY       
				 SlipNumber
				,RevisionNumber

			DROP INDEX ix_temp_Option ON #temp_Option
			CREATE INDEX ix_temp_Option ON #temp_Option (SlipNumber,RevisionNumber)

			--for debug
			--PRINT '当月販売情報作成'

			--当月黒伝リスト
			INSERT INTO #temp_KuroSlipList
			SELECT
				 LEFT(sh.SlipNumber, LEN(sh.SlipNumber)-2) as SlipNubmer
				,sh.SlipNumber
				,sh.SalesCarNumber
			FROM 
				dbo.CarSalesHeader sh
			WHERE
				sh.SlipNumber like '%-2%' AND
				sh.SalesOrderStatus = '005' AND
				sh.DelFlag = '0' AND
				-- Mod 2018/11/07 yano
				sh.SalesDate >= @TargetDateFrom 
				--AND
				--sh.SalesDate < @TargetDateTo
			
			DROP INDEX ix_temp_KuroSlipList ON #temp_KuroSlipList
			CREATE UNIQUE INDEX ix_temp_KuroSlipList ON #temp_KuroSlipList (SlipNumber)

			--for debug
			--PRINT '-当月黒伝情報作成'

			--当月赤伝(黒なし)リスト
			INSERT INTO #temp_AkaSlipList
			SELECT
				 LEFT(sh.SlipNumber, LEN(sh.SlipNumber)-2) as SlipNubmer
				,sh.SalesCarNumber
			FROM 
				dbo.CarSalesHeader sh
			WHERE
				sh.SlipNumber like '%-1%' AND
				sh.DelFlag = '0' AND
				sh.SalesDate >= @TargetDateFrom AND
				-- Mod 2018/11/07 yano
				--sh.SalesDate < @TargetDateTo AND
				NOT
				EXISTS
				(
					select 'x' from #temp_KuroSlipList kuro where LEFT(sh.SlipNumber, LEN(sh.SlipNumber)-2) = kuro.SlipNumber
				)
			DROP INDEX ix_temp_AkaSlipList ON #temp_AkaSlipList
			CREATE UNIQUE INDEX ix_temp_AkaSlipList ON #temp_AkaSlipList (SlipNumber)

			--for debug
			--PRINT '-当月赤伝情報作成'

			-- ------------------
			-- 当月販売
			-- ------------------
			INSERT INTO #temp_CarSalesDetail
			SELECT
				H.SalesDate
			,	H.SlipNumber
			,	H.SalesCarNumber
			,	H.SalesType
			,	H.Vin
			,	C.CustomerCode
			,	C.CustomerName											--Add 2016/11/30 arc yano #3659
			,	D.DepartmentCode
			,	D.DepartmentName										--Add 2016/11/30 arc yano #3659
			,	H.SalesPrice
			,	H.ShopOptionAmount - ISNULL(L.Amount, 0) AS ShopOptionAmount
			,	H.SalesCostTotalAmount
			,	ISNULL(H.DiscountAmount, 0) * - 1 AS DiscountAmount
			,	ISNULL(H.SalesPrice, 0) + ISNULL(H.DiscountAmount, 0) * - 1 + H.ShopOptionAmount + H.SalesCostTotalAmount AS SalesTotalAmount
			,	H.CarName
			,	H.CarBrandName
			,	D.AreaCode
			,	C.CustomerType
			,	H.MakerOptionAmount
			,	cn.Name AS BrandStore
			,	CASE WHEN 
					C.CustomerType = '201' OR
					C.CustomerType = '202'
				THEN 
					'業販'
				ELSE
					'一般'
				END AS CustomerTypeName
					
			FROM 
				dbo.CarSalesHeader AS H WITH(INDEX([IDX_CarSalesHeader])) LEFT OUTER JOIN
				dbo.Department AS D ON H.DepartmentCode = D.DepartmentCode LEFT OUTER JOIN
				dbo.c_CodeName cn ON cn.CategoryCode = '002' AND D.BrandStoreCode = cn.Code LEFT OUTER JOIN
				dbo.Customer AS C ON H.CustomerCode = C.CustomerCode LEFT OUTER JOIN
				#temp_Option L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber
			WHERE 
				(H.DelFlag = '0') AND 
				(H.SalesOrderStatus = '005') AND 
				EXISTS
				(
					select 'x' from dbo.Employee E where E.EmployeeCode = H.EmployeeCode
				) AND 
				NOT EXISTS	--当月以降に赤伝票のみ存在する場合は、販売から除外
				(
					select 'y' from #temp_AkaSlipList aka where LEFT(H.SlipNumber, LEN(H.SlipNumber)-2) = aka.SlipNumber
				) AND
				-- Mod 2018/11/07 yano
				NOT EXISTS　--当月赤電・黒伝票は除外
				(
					select 'z' from #temp_KuroSlipList kuro where H.SlipNumber = (kuro.SlipNumber + '-1') OR H.SlipNumber = (kuro.SlipNumber + '-2')
				) AND
				H.SalesDate >= @TargetDateFrom AND
				H.SalesDate < @TargetDateTo

			--Del 2018/11/07 yano 黒伝票は含まない
			--黒伝票のみを追加
			--INSERT INTO #temp_CarSalesDetail
			--SELECT
			--	H.SalesDate
			--,	H.SlipNumber
			--,	H.SalesCarNumber
			--,	H.SalesType
			--,	H.Vin
			--,	C.CustomerCode
			--,	C.CustomerName
			--,	D.DepartmentCode
			--,	D.DepartmentName
			--,	H.SalesPrice
			--,	H.ShopOptionAmount - ISNULL(L.Amount, 0) AS ShopOptionAmount
			--,	H.SalesCostTotalAmount
			--,	ISNULL(H.DiscountAmount, 0) * - 1 AS DiscountAmount
			--,	ISNULL(H.SalesPrice, 0) + ISNULL(H.DiscountAmount, 0) * - 1 + H.ShopOptionAmount + H.SalesCostTotalAmount AS SalesTotalAmount
			--,	H.CarName
			--,	H.CarBrandName
			--,	D.AreaCode
			--,	C.CustomerType
			--,	H.MakerOptionAmount
			--,	cn.Name AS BrandStore
			--,	CASE WHEN 
			--		C.CustomerType = '201' OR
			--		C.CustomerType = '202'
			--	THEN 
			--		'業販'
			--	ELSE
			--		'一般'
			--	END AS CustomerTypeName
			--FROM 
			--	dbo.CarSalesHeader AS H WITH(INDEX([IDX_CarSalesHeader])) LEFT OUTER JOIN
			--	dbo.Department AS D ON H.DepartmentCode = D.DepartmentCode LEFT OUTER JOIN
			--	dbo.c_CodeName cn ON cn.CategoryCode = '002' AND D.BrandStoreCode = cn.Code LEFT OUTER JOIN
			--	dbo.Customer AS C ON H.CustomerCode = C.CustomerCode LEFT OUTER JOIN
			--	#temp_Option L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber
			--WHERE
			--	EXISTS
			--	(
			--		select 'x' from #temp_KuroSlipList kuro where H.SlipNumber = kuro.KuroSlipNumber
			--	)
			--	AND
			--	H.DelFlag = '0'
			
			DROP INDEX ix_temp_CarSalesDetail ON #temp_CarSalesDetail
			CREATE UNIQUE INDEX ix_temp_CarSalesDetail ON #temp_CarSalesDetail (SalesCarNumber)

			--for debug
			--PRINT '-黒伝票追加'
			/***************************************************
			■■前月繰越
			****************************************************/
			--前月分の残高が0でないものを取得
			INSERT INTO 
				#temp_CarriedBalance
			SELECT
				 cs.SalesCarNumber
				,cs.BrandStore
				,cs.PurchaseDate
				,cs.PurchaseLocationCode
				,cs.PurchaseLocationName
				,cs.CarPurchaseType
				,cs.SupplierCode
				,cs.EndInventory
				,cs.RecycleAmount
				,P.FinancialAmount
				,cs.CarPurchaseTypeName
				,cs.SupplierName
				,cs.SelfRegistrationPurchaseDate
				,cs.MakerName
				,cs.CarName
			FROM
				#temp_CarStock cs LEFT JOIN
				(
					select 
						* 
					from 
						dbo.CarPurchase 
					where 
						PurchaseStatus = '002' and
						DelFlag = '0'
				) P ON  cs.SalesCarNumber = P.SalesCarNumber
			WHERE
				CONVERT(datetime, cs.ProcessDate) = @TargetMonthPrevious AND
				cs.EndInventory IS NOT NULL
					
			--for debug
			--PRINT '前月繰越リスト作成'

			--前月以前に販売されて、当月赤伝となった車両は復活させる
			INSERT INTO 
				#temp_CarriedBalance
			SELECT
				 cs.SalesCarNumber
				,cs.BrandStore
				,cs.PurchaseDate
				,cs.PurchaseLocationCode
				,cs.PurchaseLocationName
				,cs.CarPurchaseType
				,cs.SupplierCode
				,cs.EndInventory
				,cs.RecycleAmount
				,cs.MonthPurchase
				,cs.CarPurchaseTypeName
				,cs.SupplierName
				,cs.SelfRegistrationPurchaseDate
				,cs.MakerName
				,cs.CarName
			FROM
				dbo.CarStock cs
			WHERE
			    EXISTS
				(
					select
						'x'
					from
					(
						select 
							 cs2.SalesCarNumber
							,Max(cs2.ProcessDate) AS ProcessDate
						from 
							dbo.CarStock cs2 
						where 
							exists 
							( 
								select 
									'y' 
								from 
									#temp_AkaSlipList tas 
								where
									cs2.SalesCarNumber = tas.SalesCarNumber
							)
						group by
							 cs2.SalesCarNumber
					) gcs
					where
						cs.ProcessDate = gcs.ProcessDate and
						cs.SalesCarNumber = gcs.SalesCarNumber
				)
			
			DROP INDEX  ix_temp_CarriedBalance ON #temp_CarriedBalance
			CREATE INDEX ix_temp_CarriedBalance ON #temp_CarriedBalance (SalesCarNumber)

			/*************************************************************
			■■コードリスト
			**************************************************************/
			--前残・当月仕入
			INSERT INTO #temp_CodeList
			SELECT
				l.SalesCarNumber
			FROM 
			(
				SELECT
					SalesCarNumber
				FROM
					#Temp_CarriedBalance			--前月繰越
				UNION
				SELECT
					SalesCarNumber
				FROM
					#temp_CarPurchaseDetail			--当月仕入
				UNION
				SELECT
					SalesCarNumber
				FROM
					#Temp_CarSalesDetail			--当月販売
			) AS l

			DROP INDEX ix_temp_CodeList ON #temp_CodeList
			CREATE UNIQUE INDEX iX_temp_CodeList ON #temp_CodeList ([SalesCarNumber])

			--for debug
			--PRINT 'コードリスト作成'

			/*************************************************************
			■■車両情報リスト
			**************************************************************/
			INSERT INTO
				#temp_SalesCar
			SELECT
				  S.SalesCarNumber
				, S.MakerName
				, S.NewUsedType
				, NU.Name AS NewUsedTypeName
				, CM.CarName + ' ' + CM.CarGradeName				--車種名＋車両グレード名を車種名として登録
				, S.CarGradeCode
				, S.Vin
				, S.CarUsage
				, S.CompanyRegistrationFlag
				--ブランドストア
				,
				  CASE 
				  WHEN
					CM.MakerCode in ('CL', 'JP')
				  THEN
					'CJ'
				  WHEN
					CM.MakerCode in ('AB', 'AR', 'FI')
				  THEN
					'FA'
				  WHEN
					CM.MakerCode in ('JG', 'LR')
				  THEN
					'JLR'
				  ELSE
					NULL
				  END AS BrandStore
			FROM
				SalesCar S INNER JOIN
				dbo.V_CarMaster CM ON S.CarGradeCode = CM.CarGradeCode LEFT OUTER JOIN
				c_NewUsedType NU ON S.NewUsedType = NU.Code		--Add 2016/11/30 arc yano #3659
			WHERE
				exists
				(
					select 'x' from #Temp_CodeList tcl where tcl.SalesCarNumber = s.SalesCarNumber
				)
				 
			DROP INDEX ix_temp_SalesCar ON #temp_SalesCar
			CREATE INDEX ix_temp_SalesCar ON #temp_SalesCar (SalesCarNumber)

			--for debug
			--PRINT '車両情報リスト作成'

			/*************************************************************
			■■当月実棚リスト
			**************************************************************/
			INSERT INTO
				#temp_InventoryStockCar
			SELECT
				 SalesCarNumber AS SalesCarNumber
				,LocationCode AS LacationCode
			FROM
				dbo.InventoryStockCar
			WHERE
				InventoryMonth = @TargetDateFrom AND
				ISNULL(DelFlag, '0') <> '1' AND
				ISNULL(PhysicalQuantity, 0) = 1					--実棚が1のもの		
			
			DROP INDEX ix_temp_InventoryStockCar  ON #temp_InventoryStockCar
			CREATE INDEX ix_temp_InventoryStockCar ON #temp_InventoryStockCar (SalesCarNumber)

			--for debug
			--PRINT '当月実棚リスト作成'
			/************************************************************
			■■車両管理リスト
			*************************************************************/		
			--動作指定による
			IF @ActionFlag = 0		--動作指定 =「表示」の場合は一時表に格納
			BEGIN
				INSERT INTO
					#temp_CarStock
				SELECT
					--対象年月--------------------------------------------------------------------------------------------------------------------------------------------
					@TargetDateFrom																AS ProcessDate
					--管理番号--------------------------------------------------------------------------------------------------------------------------------------------
					,cl.SalesCarNumber															AS SalesCarNumber
					--取扱ブランドコード----------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN							--前残情報がある場合は、そこから取得
						cb.BrandStore IS NOT NULL
					 THEN
						cb.BrandStore
					 WHEN
						cp.BrandStore IS NOT NULL AND
						cp.BrandStore <> '本部' 
					 THEN
						cp.BrandStore
					 WHEN
						sc.BrandStore IS NOT NULL
					 THEN
						sc.BrandStore		--車両のメーカーから設定
					 ELSE
						cs.BrandStore		--販売店舗の取扱ブランド	
					 END																		AS BrandStore
					--新中区分--------------------------------------------------------------------------------------------------------------------------------------------
					,sc.NewUsedType																AS NewUsedType
					--仕入日----------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN							--前残情報がある場合は、そこから取得
						cb.PurchaseDate IS NOT NULL
					 THEN
						cb.PurchaseDate
					 ELSE
						cp.PurchaseDate
					 END																		AS PurchaseDate
					--車種名----------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cb.CarName IS NOT NULL AND cb.CarName <> ''
					 THEN
						cb.CarName
					 ElSE
						sc.CarName
					 END 																		AS CarName
					--グレードコード--------------------------------------------------------------------------------------------------------------------------------------
					,sc.CarGradeCode															AS CarGradeCode
					--車台番号--------------------------------------------------------------------------------------------------------------------------------------------
					,sc.Vin																		AS Vin
					--仕入・在庫拠点コード--------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN							--前残情報がある場合はそこから取得
						cb.PurchaseLocationCode IS NOT NULL
					 THEN
						cb.PurchaseLocationCode
					 ELSE
						CASE WHEN
							ts.ArrivalLocationCode IS NOT NULL
						THEN
							ts.ArrivalLocationCode
						ELSE
							cp.PurchaseLocationCode													
						END
					 END																		AS PurchaseLocationCode
					--仕入区分--------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN							--前残情報がある場合は、そこから取得
						cb.CarPurchaseType IS NOT NULL
					 THEN
						cb.CarPurchaseType
					 ELSE
						cp.CarPurchaseType
					 END																		AS CarPurchaseType
					--仕入先コード--------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN							--前残情報がある場合は、そこから取得
						cb.SupplierCode IS NOT NULL
					 THEN
						cb.SupplierCode
					 ELSE
						cp.SupplierCode
					 END																		AS SupplierCode
					--月初在庫------------------------------------------------------------------------------------------------------------------------------------------------
					,cb.EndInventory															AS BeginningInventory
					--当月仕入------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cb.EndInventory IS NOT NULL AND
						cb.EndInventory <> 0 AND
						cb.PurchaseAmount IS NOT NULL AND
						cb.EndInventory <> cb.PurchaseAmount
					 THEN
						cb.PurchaseAmount - cb.EndInventory									--仕入金額－前残
					 ELSE
						cp.FinancialAmount
					 END																		AS MonthPurchase
					--納車日---------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL
					 THEN
						SalesDate
					 ELSE
						NULL
					 END																		AS SalesDate
					--伝票番号---------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005')											--自社登等以外
					 THEN
						cs.SlipNumber
					 ELSE
						NULL
					 END																		AS SlipNumber
					--販売区分---------------------------------------------------------------------------------------------------------------------------------------------------
					,cs.SalesType																AS SalesType
					--顧客コード---------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')		--デモカー等以外
					 THEN
						cs.CustomerCode
					 ELSE
						NULL
					 END																		AS CustomerCode
					--販売価格--------------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')		--デモカー等以外
					 THEN
						ISNULL(cs.SalesPrice, 0)
					 ELSE
						NULL
					 END																		AS SalesPrice
					 --値引価格-------------------------------------------------------------------------------------------------------------------------------------------------------
					 ,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')		--デモカー等以外
					 THEN
						ISNULL(cs.DiscountAmount, 0)
					 ELSE
						NULL
					 END																		AS DiscountAmount			
					--オプション価格(店舗＋メーカー)-----------------------------------------------------------------------------------------------------------------------------------
					 ,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')		--デモカー等以外
					 THEN
						ISNULL(cs.ShopOptionAmount, 0) + ISNULL(cs.MakerOptionAmount, 0)
					 ELSE
						NULL
					 END																		AS ShopOptionAmount
					--販売諸費用合計---------------------------------------------------------------------------------------------------------------------------------------------------
					 ,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')		--デモカー等以外
					 THEN
						ISNULL(cs.SalesCostTotalAmount, 0)
					 ELSE
						NULL
					 END																		AS SalesCostTotalAmount
					 --売上総合計-------------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')		--デモカー等以外
					 THEN
						ISNULL(cs.SalesPrice, 0) + 
						ISNULL(cs.DiscountAmount, 0) + 
						ISNULL(cs.ShopOptionAmount, 0) + 
						ISNULL(cs.MakerOptionAmount, 0) + 
						ISNULL(cs.SalesCostTotalAmount, 0)
					 ELSE
						NULL
					 END																		AS SalesTotalAmount										
					--売上原価----------------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')			--デモカー等以外
					 THEN
						ISNULL(cb.EndInventory, 0) +
						ISNULL(cp.FinancialAmount, 0) +
						ISNULL(cp.OtherAccount, 0)
					 ELSE
						NULL
					 END																		AS SalesCostAmount			
					 --粗利※※※後で計算※※※------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS SalesProfits
					 --他勘定振替※※※後で計算※※※-----------------------------------------------------------------------------------------------------------------------------------		
					 ,NULL																		AS ReductionTotal
					 --自社登録※※※後で計算※※※-------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS SelfRegistration
					 --他ディーラー※※※後で計算※※※---------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS OtherDealer
					 --デモカー※※※後で計算※※※-------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS DemoCar
					 --代車※※※後で計算※※※-----------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS TemporaryCar
					 --月末在庫※※※後で計算※※※--------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS EndInventory
					 --リサイクル料金----------------------------------------------------------------------------------------------------------------------------------------------------
					 ,Case When
						cb.RecycleAmount IS NOT NULL
					  THEN
						cb.RecycleAmount
					  ELSE
						cp.RecycleAmount
					  END																		AS RecycleAmount
					 --他勘定受入----------------------------------------------------------------------------------------------------------------------------------------------------
					 ,cp.OtherAccount															AS OtherAccount
					 --レンタカー※※※後で計算※※※-------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS RentalCar
					 --業務車両※※※後で計算※※※-------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS BusinessCar
					 --PR車※※※後で計算※※※-----------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS PRCar
					 --当月実棚---------------------------------------------------------------------------------------------------------------------------------------------------------														
					 ,ivs.LocationCode															AS LocationCode
					--仕入キャンセル※※※後で計算※※※-----------------------------------------------------------------------------------------------------------------------------------
					 ,cpc.CancelAmount															AS CancelPurchas
					--仕入区分名----------------------------------------------------------------------------------------------------------------------------------------------------------															
					 ,CASE WHEN
						cb.CarPurchaseTypeName IS NOT NULL
					  THEN
						cb.CarPurchaseTypeName
					  ELSE
					    cp.CarPurchaseTypeName
					  END																		AS CarPurchaseTypeName
					--メーカー名----------------------------------------------------------------------------------------------------------------------------------------------------------
					 ,CASE WHEN
						cb.MakerName IS NOT NULL AND cb.MakerName <> ''
					  THEN
						cb.MakerName
					  ELSE
						sc.MakerName
					  END																		AS MakerName
					--仕入・在庫拠点名----------------------------------------------------------------------------------------------------------------------------------------------------
					 ,CASE WHEN
					   cb.PurchaseLocationName IS NOT NULL
					  THEN
					   cb.PurchaseLocationName
					  WHEN
					   lc.LocationName IS NOT NULL
					  THEN
					   lc.LocationName
					  ELSE
					   cp.PurchaseLocationName
					  END																		AS PurchaseLcationName
					--当月実棚ロケーション-------------------------------------------------------------------------------------------------------------------------------------------------
					 ,Case WHEN
						lc2.LocationName IS NOT NULL
					  THEN
						lc2.LocationName
					  ELSE
						'-'
					  END																		AS InventoryLocationName
					--仕入先名※前残分のみ。当月分は後で入力-------------------------------------------------------------------------------------------------------------------------------
					 ,cb.SupplierName															AS SupplierName
					--販売店舗コード-------------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')			--デモカー等以外
					 THEN
						cs.DepartmentCode
					 ELSE
						NULL
					 END																		AS SalesDepartmentCode
					--販売店舗名-----------------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')			--デモカー等以外
					 THEN
						CASE WHEN
							cs.DepartmentCode = '021' AND										--部門がFiatGroup課で販売区分が業販の場合
							cs.SalesType = '003'
						THEN
							'共通業販'
						ELSE
							cs.DepartmentName
						END
					 ELSE
						NULL
					 END																		AS SalesDepartmentName
					 --新中区分名-------------------------------------------------------------------------------------------------------------------------------------------------------
					 ,sc.NewUsedTypeName														AS NewUsedTypeName
					  --販売顧客名-------------------------------------------------------------------------------------------------------------------------------------------------------
					  ,CASE WHEN
							cs.SalesDate IS NOT NULL
					   THEN
							CASE
							WHEN
								cs.SalesType = '005'	--自社登録
							THEN
								'自社登'
							WHEN
								cs.SalesType IN ('006', '010', '011', '012', '013')	--デモカー等
							THEN
								'社有車'
							ELSE
							  cs.CustomerName
							END
							
					   ELSE
						 NULL
					   END																		AS CustomerName
					  --下取車伝票番号-----------------------------------------------------------------------------------------------------------------------------------------------------------
					  ,cp.TradeCarSlipNumber													AS TradeCarSlipNumber
					  --自社登録前の車両の仕入日-------------------------------------------------------------------------------------------------------------------------------------------------
					  ,CASE WHEN
						cb.SelfRegistrationPurchaseDate IS NOT NULL
					   THEN
						cb.SelfRegistrationPurchaseDate
					   ELSE
						cp.SelfRegistrationPurchaseDate
					   END																		AS SelfRegistrationPurchaseDate
					   --顧客種別名--------------------------------------------------------------------------------------------------------------------------------------------------------------
					  ,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')			--デモカー等以外
					   THEN
					　	cs.CustomerTypeName
					   ELSE
						NULL
					   END																		AS CustomerTypeName
				FROM
					#Temp_CodeList cl LEFT OUTER JOIN
					#Temp_CarriedBalance cb ON cl.SalesCarNumber = cb.SalesCarNumber LEFT OUTER JOIN
					#temp_CarPurchaseDetail cp ON cl.SalesCarNumber = cp.SalesCarNumber LEFT OUTER JOIN
					#temp_CarPurchaseCancel cpc ON cl.SalesCarNumber = cpc.SalesCarNumber LEFT OUTER JOIN
					#temp_CarSalesDetail cs ON cl.SalesCarNumber = cs.SalesCarNumber LEFT OUTER JOIN
					#temp_SalesCar sc ON cl.SalesCarNumber = sc.SalesCarNumber LEFT OUTER JOIN
					#temp_Transfer2 ts ON cl.SalesCarNumber = ts.SalesCarNumber LEFT OUTER JOIN
					#temp_InventoryStockCar ivs ON cl.SalesCarNumber = ivs.SalesCarNumber LEFT OUTER JOIN
					dbo.Location lc ON ts.ArrivalLocationCode = lc.LocationCode LEFT OUTER JOIN
					dbo.Location lc2 ON ivs.LocationCode = lc2.LocationCode --LEFT OUTER JOIN
					--dbo.Department dp ON cp.DepartmentCode = dp.DepartmentCode

				DROP INDEX ix_temp_CarStock ON #temp_CarStock
				CREATE UNIQUE INDEX ix_temp_CarStock ON #temp_CarStock (ProcessDate, SalesCarNumber)
				--for debug
				--PRINT '車両表に一旦格納'

				/****************************************************
				-- ■■集計・編集処理
				****************************************************/
				--■■集計１
				UPDATE
					#temp_CarStock
				SET
					--粗利
					 SalesProfits = (CASE WHEN SalesTotalAmount IS NOT NULL THEN SalesTotalAmount - SalesCostAmount ELSE NULL END)
					--自社登録
					,SelfRegistration = (CASE WHEN SalesType = '005' THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) ELSE NULL END)
					--デモカー
					,DemoCar = (CASE WHEN SalesType = '006' THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) ELSE NULL END)
					--代車
					,TemporaryCar = (CASE WHEN SalesType = '011' THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) ELSE NULL END)
					--レンタカー
					,RentalCar = (CASE WHEN SalesType = '010' THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) ELSE NULL END)
					--業務車両
					,BusinessCar = (CASE WHEN SalesType = '013' THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) ELSE NULL END)
					--広報車
					,PRCar = (CASE WHEN SalesType = '012' THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) ELSE NULL END)
					--仕入キャンセル
					,CancelPurchase = CASE WHEN CancelPurchase IS NOT NULL THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) + ISNULL(OtherAccount, 0) ELSE NULL END
				WHERE
					ProcessDate = @TargetDateFrom
		
				--for debug
				--PRINT '集計１'

				--■■各項目を計算(その２)
				UPDATE
					#temp_CarStock
				SET
					--他勘定振替
					ReductionTotal = CASE WHEN
										DemoCar IS NULL AND 
										TemporaryCar IS NULL AND 
										RentalCar IS NULL AND 
										BusinessCar IS NULL AND 
										PRCar IS NULL 
									THEN 
										NULL 
									ELSE
										ISNULL(DemoCar, 0) + 
										ISNULL(TemporaryCar, 0) + 
										ISNULL(RentalCar, 0) + 
										ISNULL(BusinessCar, 0) + 
										ISNULL(PRCar, 0)
									END
				WHERE
					ProcessDate = @TargetDateFrom
				--for debug
				--PRINT '集計２'

				--■■各項目を計算(その３)
				UPDATE
					#temp_CarStock
				SET
					--月末在庫
					EndInventory = CASE WHEN 
										SalesTotalAmount IS NULL AND
										ReductionTotal IS NULL AND
										SelfRegistration IS NULL AND
										CancelPurchase IS NULL
									THEN 
										ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) + ISNULL(OtherAccount, 0)
									ELSE
										NULL
									END
				WHERE
					ProcessDate = @TargetDateFrom
				--for debug
				--PRINT '集計３'

				--■■その他更新
				--仕入先名称の更新
				UPDATE
					#temp_CarStock
				SET
					SupplierName = 
						--仕入先名
						CASE  
						WHEN 
							cs.NewUsedType = 'N' AND 
							cs.SupplierCode IN ('9000000001', '9000000002', '002001')
						THEN
							'(' + FORMAT(cs.PurchaseDate, 'yyyyMM') + 'JACCS)'
						WHEN
							cs.NewUsedType = 'N' AND
			　				cs.SupplierCode IN ('KK00000770', 'KK00000843')
						THEN
							'(' + FORMAT(cs.PurchaseDate, 'yyyyMM') + 'GLコネクト)'
						WHEN
							cs.CarPurchaseTypeName = '下取車(先取)'
						THEN
							s.SupplierName + ' ' + cs.TradeCarSlipNumber
						ELSE
							s.SupplierName
						END
				FROM
					#temp_CarStock cs LEFT OUTER JOIN
					dbo.Supplier s ON cs.SupplierCode = s.SupplierCode
				WHERE
					cs.ProcessDate = @TargetDateFrom AND
					cs.SupplierName is null

				--他勘定受入の場合はリサイクル料金はNULL
				UPDATE
					#temp_CarStock
				SET
					RecycleAmount = NULL
				WHERE
					ProcessDate = @TargetDateFrom AND
					OtherAccount IS NOT NULL

					END
					--次の処理対象月
					SET @TargetMonthCount = @TargetMonthCount - 1				--残月数デクリメント
					SET @TargetMonthPrevious = @TargetDateFrom					--対象月前月インクリメント(＝今回の当月）
					SET @TargetDateFrom = DATEADD(m, 1, @TargetMonthPrevious)	--対象日Fromインクリメント(＝次回の前月＋１）
					SET @TargetDateTo = DATEADD(m, 1, @TargetDateFrom)			--対象日Toインクリメント(＝次回の当月＋１）
					IF @TargetDateTo > @NOW
						SET @TargetDateTo = @NOW
		--ループエンド
		END

		/***************************************************/
		/*動作指定=「表示」の場合はデータ取得を行う		   */
		/***************************************************/
		DECLARE @SQL NVARCHAR(MAX) = ''
		DECLARE @PARAM NVARCHAR(1024) = ''
		DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)		

		IF @ActionFlag = 0	--表示の場合
		BEGIN
			SET @PARAM = '@DataType nvarchar(3), @NewUsedType nvarchar(3), @SalesCarNumber NVARCHAR(50), @Vin NVARCHAR(3)'
					
			SET @SQL = ''
			SET @SQL = @SQL +'SELECT' + @CRLF
			SET @SQL = @SQL +'    cs.ProcessDate' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesCarNumber' + @CRLF
			SET @SQL = @SQL +'	, cs.BrandStore' + @CRLF
			SET @SQL = @SQL +'	, cs.NewUsedType' + @CRLF
			SET @SQL = @SQL +'	, cs.PurchaseDate' + @CRLF
			SET @SQL = @SQL +'	, cs.CarName' + @CRLF
			SET @SQL = @SQL +'	, cs.CarGradeCode' + @CRLF
			SET @SQL = @SQL +'	, cs.Vin' + @CRLF
			SET @SQL = @SQL +'	, cs.PurchaseLocationCode' + @CRLF
			SET @SQL = @SQL +'	, cs.CarPurchaseType' + @CRLF
			SET @SQL = @SQL +'	, cs.SupplierCode' + @CRLF
			SET @SQL = @SQL +'	, cs.BeginningInventory' + @CRLF
			SET @SQL = @SQL +'	, cs.MonthPurchase' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesDate' + @CRLF
			SET @SQL = @SQL +'	, cs.SlipNumber' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesType' + @CRLF
			SET @SQL = @SQL +'	, cs.CustomerCode' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesPrice' + @CRLF
			SET @SQL = @SQL +'	, cs.DiscountAmount' + @CRLF
			SET @SQL = @SQL +'	, cs.ShopOptionAmount' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesCostTotalAmount' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesTotalAmount' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesCostAmount' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesProfits' + @CRLF
			SET @SQL = @SQL +'	, cs.ReductionTotal' + @CRLF
			SET @SQL = @SQL +'	, cs.SelfRegistration' + @CRLF
			SET @SQL = @SQL +'	, cs.OtherDealer' + @CRLF
			SET @SQL = @SQL +'	, cs.DemoCar' + @CRLF
			SET @SQL = @SQL +'	, cs.TemporaryCar' + @CRLF
			SET @SQL = @SQL +'	, cs.EndInventory' + @CRLF
			SET @SQL = @SQL +'	, '''' AS CreateEmployeeCode' + @CRLF
			SET @SQL = @SQL +'	, NULL AS CreateDate' + @CRLF
			SET @SQL = @SQL +'	, '''' AS LastUpdateEmployeeCode' + @CRLF
			SET @SQL = @SQL +'	, NULL AS LastUpdateDate' + @CRLF
			SET @SQL = @SQL +'	, ''0'' AS DelFlag' + @CRLF
			SET @SQL = @SQL +'	, cs.RecycleAmount' + @CRLF
			SET @SQL = @SQL +'	, cs.OtherAccount' + @CRLF
			SET @SQL = @SQL +'	, cs.RentalCar' + @CRLF
			SET @SQL = @SQL +'	, cs.BusinessCar' + @CRLF
			SET @SQL = @SQL +'	, cs.PRCar' + @CRLF
			SET @SQL = @SQL +'	, cs.LocationCode' + @CRLF
			SET @SQL = @SQL +'	, cs.CancelPurchase' + @CRLF
			SET @SQL = @SQL +'	, cs.CarPurchaseTypeName' + @CRLF
			SET @SQL = @SQL +'	, cs.MakerName' + @CRLF
			SET @SQL = @SQL +'	, cs.PurchaseLocationName' + @CRLF
			SET @SQL = @SQL +'	, cs.InventoryLocationName' + @CRLF
			SET @SQL = @SQL +'	, cs.SupplierName' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesDepartmentCode' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesDepartmentName' + @CRLF
			SET @SQL = @SQL +'	, cs.NewUsedTypeName' + @CRLF
			SET @SQL = @SQL +'	, cs.CustomerName' + @CRLF
			SET @SQL = @SQL +'	, cs.SelfRegistrationPurchaseDate' + @CRLF
			SET @SQL = @SQL +'	, cs.CustomerTypeName' + @CRLF
			SET @SQL = @SQL +'FROM' + @CRLF
			SET @SQL = @SQL +'	#temp_CarStock cs' + @CRLF
			SET @SQL = @SQL +'WHERE' + @CRLF
			SET @SQL = @SQL +'    cs.ProcessDate = CONVERT(datetime, ''' + FORMAT(@TargetMonth, 'yyyyMMdd') + ''')' + @CRLF 
					
			--データ種別
			IF ((@DataType is not null) AND (@DataType = '001'))	--在庫の場合
			BEGIN
				SET @SQL = @SQL +' AND cs.EndInventory is not null' + @CRLF
			END
			ELSE IF((@DataType is not null) AND (@DataType = '002'))	--販売の場合
			BEGIN
				SET @SQL = @SQL +' AND cs.EndInventory is null' + @CRLF
			END
			ELSE IF((@DataType is not null) AND (@DataType = '004'))	--仕入の場合
			BEGIN
				SET @SQL = @SQL +' AND cs.PurchaseDate >= CONVERT(datetime, ''' +  FORMAT(@TargetMonth, 'yyyy/MM/dd') + ''')'+ @CRLF
				SET @SQL = @SQL +' AND cs.PurchaseDate < CONVERT(datetime, ''' +  FORMAT(@TargetMonthTo, 'yyyy/MM/dd') + ''')'+ @CRLF
			END
			--新中区分
			IF ((@NewUsedType is not null) AND (@NewUsedType <> ''))	
			BEGIN
				SET @SQL = @SQL +' AND cs.NewUsedType = ''' + @NewUsedType + '''' + @CRLF
			END
			--仕入先
			IF ((@Suppliercode is not null) AND (@Suppliercode <> ''))	
			BEGIN
				SET @SQL = @SQL +' AND cs.Suppliercode = ''' + @Suppliercode + '''' + @CRLF
			END
			--管理番号
			IF ((@SalesCarNumber is not null) AND (@SalesCarNumber <> ''))
			BEGIN
				SET @SQL = @SQL +' AND cs.SalesCarNumber = ''' + @SalesCarNumber + '''' + @CRLF
			END
			--車台番号
			IF ((@Vin is not null) AND (@Vin <> ''))
			BEGIN
				SET @SQL = @SQL +' AND cs.Vin = ''' + @Vin + '''' + @CRLF
			END

			SET @SQL = @SQL +'ORDER BY cs.PurchaseDate, cs.Vin' + @CRLF

			EXECUTE sp_executeSQL @SQL, @PARAM, @DataType, @NewUsedType, @SalesCarNumber, @Vin
		END
		ELSE		--動作指定が「スナップショット保存」の場合
		BEGIN
			--対象年月のデータをdbo.CarStockにINSERT
			INSERT INTO
				dbo.CarStock
			SELECT
			   cs.[ProcessDate]
			  ,cs.[SalesCarNumber]
			  ,cs.[BrandStore]
			  ,cs.[NewUsedType]
			  ,cs.[PurchaseDate]
			  ,cs.[CarName]
			  ,cs.[CarGradeCode]
			  ,cs.[Vin]
			  ,cs.[PurchaseLocationCode]
			  ,cs.[CarPurchaseType]
			  ,cs.[SupplierCode]
			  ,cs.[BeginningInventory]
			  ,cs.[MonthPurchase]
			  ,cs.[SalesDate]
			  ,cs.[SlipNumber]
			  ,cs.[SalesType]
			  ,cs.[CustomerCode]
			  ,cs.[SalesPrice]
			  ,cs.[DiscountAmount]
			  ,cs.[ShopOptionAmount]
			  ,cs.[SalesCostTotalAmount]
			  ,cs.[SalesTotalAmount]
			  ,cs.[SalesCostAmount]
			  ,cs.[SalesProfits]
			  ,cs.[ReductionTotal]
			  ,cs.[SelfRegistration]
			  ,cs.[OtherDealer]
			  ,cs.[DemoCar]
			  ,cs.[TemporaryCar]
			  ,cs.[EndInventory]
			  ,@EmployeeCode
			  ,GETDATE()
			  ,@EmployeeCode
			  ,GETDATE()
			  ,'0'
			  ,cs.[RecycleAmount]
			  ,cs.[OtherAccount]
			  ,cs.[RentalCar]
			  ,cs.[BusinessCar]
			  ,cs.[PRCar]
			  ,cs.[LocationCode]
			  ,cs.[CancelPurchase]
			  ,cs.[CarPurchaseTypeName]
			  ,cs.[MakerName]
			  ,cs.[PurchaseLocationName]
			  ,cs.[InventoryLocationName]
			  ,cs.[SupplierName]
			  ,cs.[SalesDepartmentCode]
			  ,cs.[SalesDepartmentName]
			  ,cs.[NewUsedTypeName]
			  ,cs.[CustomerName]
			  ,cs.[SelfRegistrationPurchaseDate]
			FROM
				#temp_CarStock cs
			WHERE
				cs.ProcessDate = @TargetMonthFrom
		END


	--トランザクション終了
		COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION
		SELECT 
			@ErrorNumber = ERROR_NUMBER()
		,	@ErrorMessage = ERROR_MESSAGE()
	END CATCH
		
FINALLY:
		--エラー判定
	IF @ErrorNumber <> 0
		RAISERROR (@ErrorMessage, 16, 1)

	RETURN @ErrorNumber
			--終了	
END


GO


