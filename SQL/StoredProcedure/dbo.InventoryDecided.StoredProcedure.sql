USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[InventoryDecided]    Script Date: 2019/06/04 15:20:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[InventoryDecided]
	 @InventoryMonth datetime,			--棚卸月
	 @WarehouseCode nvarchar(6),		--倉庫コード
	 @EmployeeCode nvarchar(50)			--社員コード
AS
BEGIN
	SET NOCOUNT ON;

	/*------------------------------------------------------------------------------------------------------------------------------------------------------
		Mod 2019/05/22 yano #3974 部品在庫棚卸　棚卸確定自の補正処理の不具合 
		Mod 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応
		Mod 2016/08/13 arc yano #3596【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
		Mod 2016/06/09 arc yano #3571 部品在庫棚卸　棚卸確定時の不具合
		Mod 2016/02/08 arc yano #3409 部品棚卸テーブル(dbo.InventoryStock)のテーブル構成変更(ProvisionQuantity追加)に伴い、SQLの修正
		Mod 2015/09/25 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑧ 仕掛在庫は計算で出すように修正
		Mod 2015/06/16 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑥ InventoryStock、PartsStockの在庫数の更新方法の変更
		Mod 2015/05/20 arc yano IPO対応(部品棚卸) 棚卸作業日～棚卸開始日までの在庫変動の調整
		1.InventoryStock
		　　対象年月月末23:59:59と棚卸開始日時の間に発生した在庫数の増減分を反映させる。

				①棚卸開始日時 >= 対象年月末の場合…対象年月末～棚卸開始日時の在庫増減分の数量を実棚数から差し引く。
				②対象年月末 > 棚卸開始日次の場合…棚卸開始日時～対象年月末の在庫増減分の数量を実棚に足す。

		2.PartsStock
			棚卸開始した時点のPartsStockの数量と計測した実棚在庫数の差分をPartsStockの数量に反映させる。                                                         
	--------------------------------------------------------------------------------------------------------------------------------------------------------*/

	DECLARE @targetDateFrom datetime											--対象範囲(From)
	DECLARE @targetDateTo datetime 												--対象範囲(To)

	DECLARE @targetNextMonth datetime											--対象年月の翌月1日
	DECLARE @PartInventoryStart datetime										--棚卸開始日時

	DECLARE @Now datetime = GETDATE()											--現在日時

	DECLARE @CalcMode int														--補正方法(0:在庫増減分を減算、1:在庫増減分を加算)


	--対象年月の翌月を取得する
	SET @targetNextMonth =  DATEADD(m, 1, @InventoryMonth)

	
	--棚卸開始日時を取得する。
	SELECT
		@PartInventoryStart = CONVERT(DATE, StartDate) 
	FROM
		dbo.InventoryScheduleParts
	WHERE
		InventoryMonth = @InventoryMonth AND
		WarehouseCode = @WarehouseCode											--Mod 2016/08/13 arc yano #3596


	--対象範囲(From/To)の設定
	IF @PartInventoryStart >= @targetNextMonth
	BEGIN
		SET @targetDateFrom = @targetNextMonth									--対象範囲(From) = 対象年月の翌月1日
		SET @targetDateTo = @PartInventoryStart									--対象範囲(To) = 棚卸開始日時
		SET @CalcMode = 0														--在庫増減分を減算
	END
	ELSE	--対象年月が当月の場合
	BEGIN
		SET @targetDateFrom = DATEADD(d, 1, @PartInventoryStart)				--対象範囲(From) = 棚卸開始日の翌日		--Mod 2019/05/22 yano #3974
		SET @targetDateTo = @targetNextMonth									--対象範囲(To) = 対象年月の翌月1日
		SET @CalcMode = 1														--在庫増減分を加算
	END

	--仕掛ロケーションコードの設定
	DECLARE @ShikakariLocationCode NVARCHAR(12) --仕掛ロケーションコード

	--仕掛ロケーションコードを取得
	SELECT 
		@ShikakariLocationCode = LocationCode
	FROM
		dbo.Location
	WHERE
		WarehouseCode = @WarehouseCode AND										--Mod 2016/08/13 arc yano #3596
		LocationType = '003'													--ロケーション種別=仕掛
	

	--部品実棚リスト(ロケーション／部品別)
	CREATE TABLE #InventoryStock (
		[LocationCode] NVARCHAR(12) NOT NULL			--ロケーションコード
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--部品番号
	,	[Quantity] DECIMAL(10, 2)						--在庫数(棚卸開始時点の理論在庫)
	,	[PhysicalQuantity] DECIMAL(10, 2)				--実棚数
	,	[ProvisionQuantity] DECIMAL(10, 2)				--引当済数			--Mod 2016/02/08 arc yano #3409
	)
	CREATE UNIQUE INDEX IX_Temp_InventoryStock ON #InventoryStock ([LocationCode], [PartsNumber])

	--仕入リスト(ロケーション／部品別)
	CREATE TABLE #PartsPurchase (
		[LocationCode] NVARCHAR(12) NOT NULL			--ロケーションコード
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--部品番号
	,	[Quantity] DECIMAL(10, 2)						--数量
	,	[ProvisionQuantity] DECIMAL(10, 2)				--引当済数			--Mod 2016/06/09 arc yano #3571
	)
	CREATE UNIQUE INDEX IX_Temp_PartsPurchase ON #PartsPurchase ([LocationCode], [PartsNumber])

	--移動受入リスト(ロケーション／部品別)
	CREATE TABLE #TransferArrival (
		[LocationCode] NVARCHAR(12) NOT NULL			--ロケーションコード
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--部品番号
	,	[Quantity] DECIMAL(10, 2)						--数量
	,	[ProvisionQuantity] DECIMAL(10, 2)				--引当済数			--Mod 2016/06/09 arc yano #3571
	)
	CREATE UNIQUE INDEX IX_Temp_TransferArrival ON #TransferArrival ([LocationCode], [PartsNumber])

	--販売リスト(ロケーション／部品別)
	CREATE TABLE #ServiceSales (
		[LocationCode] NVARCHAR(12) NOT NULL			--ロケーションコード
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--部品番号
	,	[Quantity] DECIMAL(10, 2)						--数量
	,	[ProvisionQuantity] DECIMAL(10, 2)				--引当済数			--Mod 2016/06/09 arc yano #3571
	)
	CREATE UNIQUE INDEX IX_Temp_ServiceSales ON #ServiceSales ([PartsNumber])

	
	--移動払出リスト(ロケーション／部品別)
	CREATE TABLE #TransferDeparture (
		[LocationCode] NVARCHAR(12) NOT NULL			--ロケーションコード
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--部品番号
	,	[Quantity] DECIMAL(10, 2)						--数量
	,	[ProvisionQuantity] DECIMAL(10, 2)				--引当済数			--Mod 2016/06/09 arc yano #3571
	)
	CREATE UNIQUE INDEX IX_Temp_TransferDeparture ON #TransferDeparture ([LocationCode], [PartsNumber])


	--実棚数調整リスト(ロケーション／部品別)
	CREATE TABLE #AdjustmentStock (
		[LocationCode] NVARCHAR(12) NOT NULL			--ロケーションコード
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--部品番号
	,	[Quantity] DECIMAL(10, 3)						--実棚数(補正処理前)
	,	[CalcQuantity] DECIMAL(10, 3)					--実棚数(補正処理後)
	,	[ProvisionQuantity] DECIMAL(10, 2)				--引当済数			--Mod 2016/06/09 arc yano #3571
	)
	CREATE UNIQUE INDEX IX_Temp_AdjustmentStock ON #AdjustmentStock ([LocationCode], [PartsNumber], [Quantity])

	--Add 2015/07/17 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑦ 仕掛の実棚数は計算で出すように変更する。
	--仕掛リスト(ロケーション／部品別)
	CREATE TABLE #InProcess (
		[PartsNumber] NVARCHAR(50) NOT NULL				--部品番号
	,	[Quantity] DECIMAL(10, 3)						--実棚数
	)
	CREATE UNIQUE INDEX IX_Temp_InProcess ON #InProcess ([PartsNumber], [Quantity])

	--サービス伝票ヘッダ(対象外データ)
	CREATE TABLE #Temp_ServiceSalesHeader_Exempt (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	)

	--Add 2016/06/09 arc yano #3571
	--移動(引当/引当解除)リスト(ロケーション／部品別)
	CREATE TABLE #TransferProvision (
		[LocationCode] NVARCHAR(12) NOT NULL			--ロケーションコード
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--部品番号
	,	[Quantity] DECIMAL(10, 2)						--数量
	,	[ProvisionQuantity] DECIMAL(10, 2)				--引当済数			--Mod 2016/06/09 arc yano #3571
	)
	CREATE UNIQUE INDEX IX_Temp_TransferProvision ON #TransferProvision ([LocationCode], [PartsNumber])
	
	--Add 2016/08/13 arc yano #3596
	--部門一覧
	CREATE TABLE #DepartmentListUseWarehouse (
		[DepartmentCode] NVARCHAR(3) NOT NULL			--部門コード
	,	[WarehouseCode] NVARCHAR(6) NOT NULL			--倉庫コード
	)
	CREATE UNIQUE INDEX IX_Temp_DepartmentListUseWarehouse ON #DepartmentListUseWarehouse ([DepartmentCode], [WarehouseCode])

	/********************
	■■部門リスト取得
	*********************/
	--部門・倉庫の組合せリストより、倉庫を使用している全て部門を取得する
	INSERT INTO #DepartmentListUseWarehouse
	SELECT
		 dw.DepartmentCode		--部門コード
		,dw.WarehouseCode		--倉庫コード
	FROM
		dbo.DepartmentWarehouse dw
	WHERE
		dw.WarehouseCode = @WarehouseCode			--倉庫コード

	--インデックス再生成	
	DROP INDEX IX_Temp_DepartmentListUseWarehouse ON #DepartmentListUseWarehouse
	CREATE UNIQUE INDEX IX_Temp_DepartmentListUseWarehouse ON #DepartmentListUseWarehouse ([DepartmentCode], [WarehouseCode])

	/****************
	■■仕掛
	****************/
	--仕掛を一時テーブルに挿入
	----------------------------------------------------------------
	--サービス伝票対象外データの取得(対象月にキャンセル、作業中止)
	----------------------------------------------------------------
	INSERT INTO #Temp_ServiceSalesHeader_Exempt
	SELECT
			sh.[SlipNumber]											--伝票番号
	FROM
		dbo.ServiceSalesHeader sh
	WHERE 
		sh.[ServiceOrderStatus] in ('007', '010')				--007:キャンセル、010:作業中止
	AND sh.[CreateDate] < @TargetDateTo
	AND sh.[DelFlag] = '0'
			
	/*
	--インデックス再生成	
	DROP INDEX IX_Temp_ServiceSalesHeader_Exempt ON #Temp_ServiceSalesHeader_Exempt
	CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader_Exempt ON #Temp_ServiceSalesHeader_Exempt ([SlipNumber])
	*/

	--------------------------------------------------------------
	--サービス伝票対象外データの取得(対象月に納車済)
	--------------------------------------------------------------
	INSERT INTO #Temp_ServiceSalesHeader_Exempt
	SELECT
			sh.[SlipNumber]											--伝票番号
	FROM
		dbo.ServiceSalesHeader sh
	WHERE 
		sh.[ServiceOrderStatus] = '006'								--006:納車済
	AND ISNULL(sh.[SalesDate], @TargetDateTo) < @TargetDateTo		--当月中に納車済
	AND sh.[DelFlag] = '0'


	INSERT INTO #InProcess
	SELECT
		sl.[PartsNumber]
	,	SUM(ISNULL(sl.[ProvisionQuantity], 0)) AS [Quantity]			--Mod 2016/02/08 arc yano #3409
	--,	SUM(ISNULL(sl.[Quantity], 0)) AS [Quantity]
	
	FROM [ServiceSalesHeader] sh										--サービスオーダーヘッダ
	INNER JOIN [ServiceSalesLine] sl									--サービスオーダー明細
		ON sl.[SlipNumber] = sh.[SlipNumber] 
		AND ISNULL(sl.[DelFlag], '0') <> '1'
	INNER JOIN Parts p													--部品マスタ
		ON p.[PartsNumber] = sl.[PartsNumber]
	--WHERE sh.[DepartmentCode] = @DepartmentCode						--Mod 2016/08/13 arc yano #3596
	WHERE
		EXISTS
		(	
			SELECT 'X' FROM #DepartmentListUseWarehouse dw WHERE sh.DepartmentCode = dw.DepartmentCode 
		)
	 
	AND sh.[WorkingStartDate] < @targetDateTo							--現時点で作業開始している伝票
	--AND NOT (ISNULL(sh.[SalesDate], @targetDateTo) < @targetDateTo)	--現時点で納車していない伝票
	--当月以前にキャンセル/作業中止のない伝票
	AND NOT EXISTS(
		SELECT 'X'
		FROM #Temp_ServiceSalesHeader_Exempt sub
		WHERE sub.[SlipNumber] = sh.[SlipNumber]	
		)
	AND ISNULL(sh.DelFlag, '0') <> '1'
	GROUP BY sl.PartsNumber
	
	--インデックス再生成
	DROP INDEX IX_Temp_InProcess ON #InProcess
	CREATE UNIQUE INDEX IX_Temp_InProcess ON #InProcess ([PartsNumber], [Quantity])

	
	--仕掛データ更新
	--棚卸テーブルの更新
	UPDATE
		dbo.InventoryStock
	SET
		  PhysicalQuantity = ip.Quantity		--実棚を計算在庫数量で更新
		, ProvisionQuantity = ip.Quantity		--引当済数も同数で更新		--Add 2016/06/09 arc yano #3571
		, LastUpdateEmployeeCode = 'sys'
		, LastUpdateDate = @Now
	FROM
		dbo.InventoryStock ivs INNER JOIN
		#InProcess ip ON ip.PartsNumber = ivs.PartsNumber AND ivs.LocationCode = @ShikakariLocationCode
	WHERE
		WarehouseCode = @WarehouseCode AND								--Mod 2016/08/13 arc yano #3596
		InventoryMonth = @InventoryMonth AND
		LocationCode = @ShikakariLocationCode	--ロケーションコードが仕掛のもの

	--仕掛データ更新２ 計算在庫にない仕掛ロケーションの数量は０にする
	--棚卸テーブルの更新
	UPDATE
		dbo.InventoryStock
	SET
		  PhysicalQuantity = 0		--実棚を計算在庫テーブルにないものは数量を０にする
		, ProvisionQuantity = 0		--引当済数も同数で更新		--Add 2016/06/09 arc yano #3571
		, LastUpdateEmployeeCode = 'sys'
		, LastUpdateDate = @Now
	FROM
		dbo.InventoryStock ivs
	WHERE
		ivs.WarehouseCode = @WarehouseCode AND						--Mod 2016/08/13 arc yano #3596
		ivs.InventoryMonth = @InventoryMonth AND
		ivs.LocationCode = @ShikakariLocationCode AND	--ロケーションコードが仕掛のもの
		NOT EXISTS
		(
			SELECT 'X' FROM #InProcess ip WHERE ip.PartsNumber = ivs.PartsNumber
		)

	--仕掛データの挿入
	INSERT
		dbo.InventoryStock
	SELECT
		  NEWID()					AS [InventoryID]
		, ''						AS [DepartmentCode]			--Mod 2016/08/13 arc yano #3596 部門コードは空文字に
		, @InventoryMonth			AS [InventoryMonth]
		, @ShikakariLocationCode	AS [LocationCode]
		, @EmployeeCode				AS [EmployeeCode]
		, '002'						AS [InventoryType]
		, NULL						AS [SalesCarNumber]
		, ip.PartsNumber			AS [PartsNumber]
		, ip.Quantity				AS [Quantity]
		, 'sys'						AS [CreateEmployeeCode]
		, @Now						AS [CreateDate]
		, 'sys'						AS [LastUpdateEmployeeCode]
		, @Now						AS [LastUpdateDate]
		, '0'						AS [DelFlag]
		, NULL						AS [Summary]
		, ip.Quantity				AS [PhysicalQuantity]
		, NULL						AS [Comment]
		, ip.Quantity				AS [ProvisionQuantity]		--Mod 2016/02/08 arc yano #3409
		, @WarehouseCode			AS [WarehouseCode]			--Add 2016/08/13 arc yano #3596
	FROM
		#InProcess ip
	WHERE
		NOT EXISTS
		(
			SELECT 'X' From dbo.InventoryStock ivs WHERE ivs.LocationCode = @ShikakariLocationCode AND ivs.PartsNumber = ip.PartsNumber
		)

	/****************
	■■実棚
	****************/
	INSERT INTO
		#InventoryStock
	SELECT
		   LocationCode
		 , PartsNumber
		 , Quantity
		 , PhysicalQuantity
		 , ProvisionQuantity	--引当済数	Mod 2016/02/08 arc yano #3409
	FROM
		dbo.InventoryStock
	WHERE
		WarehouseCode  = @WarehouseCode AND					--Mod 2016/08/13 arc yano
		InventoryMonth = @InventoryMonth

	--インデックス再生成
	DROP INDEX IX_Temp_InventoryStock ON #InventoryStock
	CREATE UNIQUE INDEX IX_Temp_InventoryStock ON #InventoryStock ([LocationCode], [PartsNumber])

	/***********************************************
	■■納車　※戻し先は仕掛ロケーション
	************************************************/
	--ダーティーリードの設定
	--SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	--納車情報を一時テーブルに挿入
	INSERT INTO #ServiceSales
	SELECT
		@ShikakariLocationCode
	,	sl.[PartsNumber] AS [PartsNumber]
	,	SUM(ISNULL(sl.[ProvisionQuantity], 0)) AS [Quantity]				--Mod 2016/06/09 arc yano #3571
	,	SUM(ISNULL(sl.[ProvisionQuantity], 0)) AS [ProvisionQuantity]		--Add 2016/06/09 arc yano #3571
	FROM [ServiceSalesHeader] sh											--サービスオーダーヘッダ
	INNER JOIN [ServiceSalesLine] sl										--サービスオーダー明細
		ON sl.[SlipNumber] = sh.[SlipNumber] 
		AND ISNULL(sl.[DelFlag], '0') <> '1'
	INNER JOIN Parts p														--部品マスタ
		ON p.[PartsNumber] = sl.[PartsNumber]
	WHERE sh.[SalesDate] >= @TargetDateFrom									--当月１日
	AND sh.[SalesDate] < @TargetDateTo										--棚卸開始日
	--AND sh.[DepartmentCode] = @DepartmentCode								--部門コード
	AND EXISTS																--Mod 2016/08/13 arc yano #3596
	(
		SELECT 'X' FROM #DepartmentListUseWarehouse dw WHERE sh.DepartmentCode = dw.DepartmentCode
	)
	AND sh.[ServiceOrderStatus] = '006'										--納車済
	AND ISNULL(sh.DelFlag, '0') <> '1'
	GROUP BY
		sl.[PartsNumber]
		
	--インデックス再生成
	DROP INDEX IX_Temp_ServiceSales ON #ServiceSales
	CREATE UNIQUE INDEX IX_Temp_ServiceSales ON #ServiceSales ([LocationCode], [PartsNumber])


	/****************
	■■移動受入
	****************/
	--移動情報を一時テーブルに挿入
	INSERT INTO #TransferArrival
	SELECT
		tr.[ArrivalLocationCode]
	,	tr.[PartsNumber]
	,	SUM(ISNULL(tr.[Quantity], 0)) AS [Quantity]																	--数量
	,	SUM(
			CASE WHEN 
				(tr.TransferType = '003' OR tr.TransferType = '007') THEN ISNULL(tr.[Quantity], 0)					--移動種別=「仕掛」または仕掛解除
			ELSE 
				0  
			END
			) 
		AS [ProvisionQuantity]		--引当済数(移動種別が「仕掛」「仕掛解除」の場合は 数量、それ以外は0)		--Add 2016/06/09 arc yano #3571
	FROM [Transfer] tr
	INNER JOIN Parts p																								--部品マスタ
		ON p.[PartsNumber] = tr.[PartsNumber] AND (ISNULL(p.[DelFlag], '0') <> '1')
	INNER JOIN [Location] l																							--受入ﾛｹｰｼｮﾝ
		ON l.[LocationCode] = tr.[ArrivalLocationCode]
	INNER JOIN [Location] l2																						--出荷ﾛｹｰｼｮﾝ
		ON l2.[LocationCode] = tr.[DepartureLocationCode]
	WHERE tr.[ArrivalDate] >= @TargetDateFrom
	AND tr.[ArrivalDate] < @TargetDateTo
	--AND l.[DepartmentCode] = @DepartmentCode
	AND l.[WarehouseCode] = @WarehouseCode																			--Mod 2016/08/13 arc yano #3596
	AND ISNULL(tr.[DelFlag], '0') <> '1'
	AND TransferType <> '006'																						--移動種別=「自動引当」
	AND TransferType <> '008'																						--移動種別=「引当解除」
	--AND l.DepartmentCode <> l2.DepartmentCode
	GROUP BY
		tr.[ArrivalLocationCode]
	,	tr.[PartsNumber]
	--インデックス再生成
	DROP INDEX IX_Temp_TransferArrival ON #TransferArrival
	CREATE UNIQUE INDEX IX_Temp_TransferArrival ON #TransferArrival ([LocationCode], [PartsNumber])

	/****************
	■■移動払出
	****************/
	--移動情報を一時テーブルに挿入
	INSERT INTO #TransferDeparture
	SELECT
		tr.[DepartureLocationCode]
	,	tr.[PartsNumber]
	,	SUM(ISNULL(tr.[Quantity], 0)) AS [Quantity]
	,	SUM(
			CASE WHEN 
				(tr.TransferType = '003' OR tr.TransferType = '007') THEN ISNULL(tr.[Quantity], 0)					--移動種別=「仕掛」または仕掛解除
			ELSE 
				0  
			END
			) 
		AS [ProvisionQuantity]		--引当済数(移動種別が「仕掛」「仕掛解除」の場合は 数量、それ以外は0)		--Add 2016/06/09 arc yano #3571
	FROM [Transfer] tr
	INNER JOIN Parts p																								--部品マスタ
		ON p.[PartsNumber] = tr.[PartsNumber] AND (ISNULL(p.[DelFlag], '0') <> '1')
	INNER JOIN [Location] l																							--出荷ﾛｹｰｼｮﾝ
		ON l.[LocationCode] = tr.[DepartureLocationCode]
	INNER JOIN [Location] l2																						--受入ﾛｹｰｼｮﾝ
		ON l2.[LocationCode] = tr.[ArrivalLocationCode]
	WHERE tr.[DepartureDate] >= @TargetDateFrom
	AND tr.[DepartureDate] < @TargetDateTo
	--AND l.[DepartmentCode] = @DepartmentCode																		--Mod 2016/08/13 arc yano #3596
	AND l.[WarehouseCode] = @WarehouseCode
	AND ISNULL(tr.[DelFlag], '0') <> '1'
	AND TransferType <> '006'																						--移動種別=「自動引当」
	AND TransferType <> '008'																						--移動種別=「引当解除」
	--AND l.DepartmentCode <> l2.DepartmentCode
	GROUP BY
		tr.[DepartureLocationCode]
	,	tr.[PartsNumber]
	--インデックス再生成
	DROP INDEX IX_Temp_TransferDeparture ON #TransferDeparture
	CREATE UNIQUE INDEX IX_Temp_TransferDeparture ON #TransferDeparture ([LocationCode], [PartsNumber])


	/****************
	■■仕入
	****************/
	--仕入情報を一時テーブルに挿入
	INSERT INTO #PartsPurchase
	SELECT
		pp.[LocationCode]
	,	pp.[PartsNumber]
	,	SUM(CASE pp.[PurchaseType] WHEN '001' THEN ISNULL(pp.[Quantity], 0) ELSE (ISNULL(pp.[Quantity], 0)* -1) END) AS [Quantity]
	,	0 AS [ProvisionQuantity]																										--引当済数(0固定)		--Add 2016/06/09 arc yano #3571
	FROM [PartsPurchase] pp
	WHERE pp.[PurchaseDate] >= @TargetDateFrom
	AND pp.[PurchaseDate] < @TargetDateTo
	--AND pp.[DepartmentCode] = @DepartmentCode																		--Mod 2016/08/13 arc yano #3596
	AND EXISTS
	(
		SELECT 'X' FROM #DepartmentListUseWarehouse dw WHERE pp.DepartmentCode = dw.DepartmentCode 
	)
	AND ISNULL(pp.DelFlag, '0') <> '1'
	AND pp.PurchaseStatus = '002'
	GROUP BY
		pp.[LocationCode]
	,	pp.[PartsNumber]
	--インデックス再生成
	DROP INDEX IX_Temp_PartsPurchase ON #PartsPurchase
	CREATE UNIQUE INDEX IX_Temp_PartsPurchase ON #PartsPurchase ([LocationCode], [PartsNumber])

	/****************
	■■引当数の算出 
	*****************/
	--移動情報を一時テーブルに挿入
	INSERT INTO #TransferProvision
	SELECT
		tr.[ArrivalLocationCode]
	,	tr.[PartsNumber]
	,	0 AS [Quantity]																	--数量
	,	SUM(CASE WHEN tr.TransferType = '006' THEN ISNULL(tr.[Quantity], 0) ELSE ISNULL(tr.[Quantity], 0) * (-1) END) AS [ProvisionQuantity]		--引当済数(移動種別が「引当解除」の場合はマイナスの数慮う)
	FROM [Transfer] tr
	INNER JOIN Parts p																								--部品マスタ
		ON p.[PartsNumber] = tr.[PartsNumber] AND (ISNULL(p.[DelFlag], '0') <> '1')
	INNER JOIN [Location] l																							--受入ﾛｹｰｼｮﾝ
		ON l.[LocationCode] = tr.[ArrivalLocationCode]
	INNER JOIN [Location] l2																						--出荷ﾛｹｰｼｮﾝ
		ON l2.[LocationCode] = tr.[DepartureLocationCode]
	WHERE tr.[ArrivalDate] >= @TargetDateFrom
	AND tr.[ArrivalDate] < @TargetDateTo
	--AND l.[DepartmentCode] = @DepartmentCode
	AND l.[WarehouseCode] = @WarehouseCode																			--Mod 2016/08/13 arc yano #3596

	AND ISNULL(tr.[DelFlag], '0') <> '1'
	AND (TransferType = '006' OR TransferType = '008')																--移動種別=「自動引当or引当解除」

	--AND l.DepartmentCode <> l2.DepartmentCode
	GROUP BY
		tr.[ArrivalLocationCode]
	,	tr.[PartsNumber]
	--インデックス再生成
	DROP INDEX IX_Temp_TransferProvision ON #TransferProvision
	CREATE UNIQUE INDEX IX_Temp_TransferProvision ON #TransferProvision ([LocationCode], [PartsNumber])


	/****************
	■■実棚調整
	****************/
	--実棚数から仕入、移動(受入、払出分)の在庫変動分の調整後の実棚数を格納
	IF @CalcMode = 0			--在庫増減分を減算する場合
	BEGIN
		INSERT INTO #AdjustmentStock
		SELECT
			iv.[LocationCode]
		,	iv.[PartsNumber]
		,   ISNULL(iv.[PhysicalQuantity], 0) AS [Quantity]																													  --数量
		,	(ISNULL(iv.[PhysicalQuantity], 0) - (ISNULL(pp.[Quantity], 0) +  ISNULL(ta.Quantity, 0) - ISNULL(ss.[Quantity], 0) - ISNULL(td.Quantity, 0)))  AS [CalcQuantity]  --数量(実棚から在庫増減分を減算)
		,	(ISNULL(iv.[ProvisionQuantity], 0) - (ISNULL(pp.[ProvisionQuantity], 0) +  ISNULL(ta.[ProvisionQuantity], 0) - ISNULL(ss.[ProvisionQuantity], 0) - ISNULL(td.[ProvisionQuantity], 0)) - ISNULL(tp.[ProvisionQuantity], 0))  AS [ProvisionQuantity]  --引当済数	--Add 2016/06/09 arc yano #3571
		FROM 
			#InventoryStock iv
		LEFT JOIN #PartsPurchase pp																--仕入
			ON (iv.[LocationCode] = pp.[LocationCode] AND iv.[PartsNumber] = pp.[PartsNumber])
		LEFT JOIN #TransferArrival ta															--移動受入
			ON (iv.[LocationCode] = ta.[LocationCode] AND iv.[PartsNumber] = ta.[PartsNumber])
		LEFT JOIN #TransferDeparture td															--移動払出
			ON (iv.[LocationCode] = td.[LocationCode] AND iv.[PartsNumber] = td.[PartsNumber])
		LEFT JOIN #ServiceSales ss																--販売
			ON (iv.[LocationCode] = ss.[LocationCode] AND iv.[PartsNumber] = ss.[PartsNumber])
		LEFT JOIN #TransferProvision tp															--引当済調整		--Add 2016/06/09 arc yano #3571
			ON (iv.[LocationCode] = tp.[LocationCode] AND iv.[PartsNumber] = tp.[PartsNumber])
	END
	ELSE
	BEGIN
		INSERT INTO #AdjustmentStock
			SELECT
				iv.[LocationCode]
			,	iv.[PartsNumber]
			,   ISNULL(iv.[PhysicalQuantity], 0) AS [Quantity]																													 --数量
			,	(ISNULL(iv.[PhysicalQuantity], 0) + (ISNULL(pp.[Quantity], 0) + ISNULL(ta.Quantity, 0) - ISNULL(ss.[Quantity], 0) - ISNULL(td.Quantity, 0)))  AS [CalcQuantity]  --数量(実棚から在庫増減分を加算)
			,	(ISNULL(iv.[ProvisionQuantity], 0) + (ISNULL(pp.[ProvisionQuantity], 0) + ISNULL(ta.[ProvisionQuantity], 0) - ISNULL(ss.[ProvisionQuantity], 0) - ISNULL(td.[ProvisionQuantity], 0)) + ISNULL(tp.[ProvisionQuantity], 0))  AS [ProvisionQuantity]  --引当済数	--引当済数(実棚から在庫増減分を減算)	--Add 2016/06/09 arc yano #3571
			FROM 
				#InventoryStock iv
			LEFT JOIN #PartsPurchase pp																--仕入
				ON (iv.[LocationCode] = pp.[LocationCode] AND iv.[PartsNumber] = pp.[PartsNumber])
			LEFT JOIN #TransferArrival ta															--移動受入
				ON (iv.[LocationCode] = ta.[LocationCode] AND iv.[PartsNumber] = ta.[PartsNumber])
			LEFT JOIN #TransferDeparture td															--移動払出
				ON (iv.[LocationCode] = td.[LocationCode] AND iv.[PartsNumber] = td.[PartsNumber])
			LEFT JOIN #ServiceSales ss																--販売
				ON (iv.[LocationCode] = ss.[LocationCode] AND iv.[PartsNumber] = ss.[PartsNumber])
			LEFT JOIN #TransferProvision tp															--引当済調整		--Add 2016/06/09 arc yano #3571
			ON (iv.[LocationCode] = tp.[LocationCode] AND iv.[PartsNumber] = tp.[PartsNumber])

	END
	--インデックス再生成
	DROP INDEX IX_Temp_AdjustmentStock ON #AdjustmentStock
	CREATE UNIQUE INDEX IX_Temp_AdjustmentStock ON #AdjustmentStock ([LocationCode], [PartsNumber])


	--//////////////////////////////////////////////////////////////////////////////////////
	--PartsStock作成・更新
	--計測した在庫数量と理論在庫(棚卸開始時)の差分をPartsStockに反映させる。
	--/////////////////////////////////////////////////////////////////////////////////////
	
	--Add 2017/02/08 arc yano #3620
	--PartsStockでDelFlag = '1'のレコードのQuantityとProvisionQuantityを初期化(0で上書き)する
	UPDATE
		dbo.PartsStock
	SET
		  Quantity = 0
		, ProvisionQuantity = 0
		, LastUpdateEmployeeCode = 'sys'
		, LastUpdateDate = @Now
	WHERE
		ISNULL(DelFlag, '0') = '1'


	--更新(UPDATE)
	UPDATE
		dbo.PartsStock
	SET
		  --実棚(補正前)と理論在庫数(棚卸開始時)の差分を反映する
		  Quantity = ISNULL(ps.Quantity, 0) + (ivs.PhysicalQuantity - ISNULL(ivs.Quantity, 0))
		, DelFlag = '0'															--Mod 2017/02/08 arc yano #3620
		, LastUpdateEmployeeCode = @EmployeeCode
		, LastUpdateDate = @Now
	FROM
		dbo.PartsStock ps
	INNER JOIN
		#InventoryStock ivs
	ON
		ps.LocationCode = ivs.LocationCode AND
		ps.PartsNumber = ivs.PartsNumber
	
	--dbo.InventoryStockにレコードが存在し、dboPartsStockにレコードが存在しない場合は新規作成を行う。
	--新規作成(INSERT)
	INSERT
		dbo.PartsStock
	SELECT
		  ivs.PartsNumber						--部品番号
		, ivs.LocationCode						--ロケーションコード
		, ivs.PhysicalQuantity					--実棚
		, @EmployeeCode							--作成者
		, @Now									--作成日
		, @EmployeeCode							--最終更新者
		, @Now									--最終更新日
		, '0'									--削除フラグ
		, ISNULL(ivs.ProvisionQuantity, 0)		--引当済数		--Mod 2016/06/09 arc yano #3571 --Mod 2016/02/08 arc yano #3409 
	FROM
		#InventoryStock ivs
	WHERE
		NOT EXISTS 
		(
			SELECT 'X' FROM PartsStock eps WHERE eps.PartsNumber = ivs.PartsNumber AND eps.LocationCode = ivs.LocationCode 
		)

	--Add 2016/06/09 arc yano #3571
	--//////////////////////////////////////////////////////////////////////////////////////
	--PartsStock・仕掛ロケーションの引当済数の更新
	--/////////////////////////////////////////////////////////////////////////////////////
	UPDATE
		dbo.PartsStock
	SET
		ProvisionQuantity = Quantity				--引当済数を在庫数量に合わせる
	WHERE
		LocationCode = @ShikakariLocationCode

	--///////////////////////////////////////////////////////////////////////
	--InventoryStockの更新
	--当月1日～棚卸確定時までに発生した取引(仕入／販売／移動)
	--による在庫変動分を戻す
	--///////////////////////////////////////////////////////////////////
	
	--///////////////////////////////////////////////////////////////////
	--最後に一時テーブルの調整後の数量でInventoryStockの実棚数を更新する
	--///////////////////////////////////////////////////////////////////
	
	UPDATE
		dbo.InventoryStock 
	SET
		  PhysicalQuantity = ads.CalcQuantity
		, ProvisionQuantity = ads.ProvisionQuantity			--Add 2016/06/09 arc yano #3571
		, LastUpdateEmployeeCode = @EmployeeCode
		, LastUpdateDate = @Now
	FROM
		dbo.InventoryStock ivs
	INNER JOIN
		#AdjustmentStock ads
	ON
		ivs.LocationCode = ads.LocationCode AND
		ivs.PartsNumber = ads.PartsNumber
	WHERE
		--ivs.DepartmentCode = @DepartmentCode AND			--Mod 2016/08/13 arc yano #3596
		ivs.WarehouseCode = @WarehouseCode AND
		ivs.InventoryMonth = @InventoryMonth 
		--ads.CalcQuantity <> ads.Quantity

	--■■一時表の宣言
	/*************************************************************************/
	BEGIN TRY
		DROP TABLE #InventoryStock
		DROP TABLE #PartsPurchase
		DROP TABLE #TransferArrival
		DROP TABLE #ServiceSales
		DROP TABLE #TransferDeparture
		DROP TABLE #AdjustmentStock
		DROP TABLE #InProcess
		DROP TABLE #TransferProvision
		DROP TABLE #DepartmentListUseWarehouse			--Add 2016/08/13 arc yano #3596
	END TRY
	BEGIN CATCH
		--無視
	END CATCH
END


GO


