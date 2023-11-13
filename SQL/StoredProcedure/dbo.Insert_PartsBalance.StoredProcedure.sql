USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[Insert_PartsBalance]    Script Date: 2019/02/25 13:20:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- ===================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Update date:
-- 2019/02/09 :  yano #3969 部品在庫確認画面　当月納車の数量が不正
-- 2018/05/14 :  arc yano  #3880 売上原価計算及び棚卸評価法の変更
-- 2016/08/13 :  arc yano  #3596 【大項目】倉庫棚統合対応 棚卸の管理を倉庫単位から倉庫単位に変更
-- 2016/02/03 :  arc yano  #3402 部品在庫確認　引当在庫の算出方法の変更 引当在庫の算出元を引当ロケーションの在庫数→引当済数に変更
-- 2015/09/16 :  arc yano  IPO対応(部品棚卸) 障害対応　仕掛在庫の金額の不一致(部品在庫確認⇔部品仕掛在庫一覧)
-- 2015/07/17 :  arc yano  IPO対応(部品棚卸) 障害対応、仕様変更⑦ 引当在庫欄追加
-- 2015/06/18 :  arc yano  IPO対応(部品棚卸) 障害対応、仕様変更⑥ 作成者、最終更新者に「sys」を設定する
-- 2015/06/02 :  arc yano  IPO対応(部品棚卸) 障害対応、仕様変更③ 理論在庫追加
--
-- Description:	移動平均単価計算＆受払表作成処理
-- ====================================================================================================================================
CREATE PROCEDURE [dbo].[Insert_PartsBalance]
	@p_CompanyCode nvarchar(3) = '001'		--会社コード
,	@p_isShowAllZero bit = 0				--単価を除く全ての金額が0の部品も登録するかどうか[0:しない、1:する]
AS
BEGIN
	SET NOCOUNT ON;

	--定数----------------------
	DECLARE @RoundLength int = 0			--丸め有効桁数
	DECLARE @RoundMode int = 0				--0:四捨五入, 1:切り捨て
	----------------------------



	--■■一時表の宣言
	/*************************************************************************/
	BEGIN TRY
		DROP TABLE #Previous
		DROP TABLE #PreviousAll
		DROP TABLE #PartsPurchase
		DROP TABLE #PartsPurchaseAll
		DROP TABLE #TransferArrival
		DROP TABLE #ServiceSales
		DROP TABLE #TransferDeparture
		DROP TABLE #InventoryStock
		DROP TABLE #PartsList
		DROP TABLE #PartsListAll
		DROP TABLE #QuantityCalc
		DROP TABLE #QuantityPost
		DROP TABLE #QuantityDiff
		DROP TABLE #AverageCost
		DROP TABLE #DiffCost
		DROP TABLE #Reservation		--Add 2015/07/17 arc yano 
		DROP TABLE #InProcess
		DROP TABLE #PartsBalance
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

	--前残リスト(倉庫／部品別)
	CREATE TABLE #Previous (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Cost] DECIMAL(10, 0)
	,	[Quantity] DECIMAL(10, 3)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_Previous ON #Previous ([WarehouseCode], [PartsNumber])

	--前残リスト(全社合計／部品別)
	CREATE TABLE #PreviousAll (
		[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Cost] DECIMAL(10, 0)
	,	[Quantity] DECIMAL(10, 3)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_PreviousAll ON #PreviousAll ([PartsNumber])

	--仕入リスト(倉庫／部品別)
	CREATE TABLE #PartsPurchase (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_PartsPurchase ON #PartsPurchase ([WarehouseCode], [PartsNumber])

	--仕入リスト(全社合計／部品別)
	CREATE TABLE #PartsPurchaseAll (
		[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_PartsPurchaseAll ON #PartsPurchaseAll ([PartsNumber])

	--移動受入リスト(倉庫／部品別)
	CREATE TABLE #TransferArrival (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_TransferArrival ON #TransferArrival ([WarehouseCode], [PartsNumber])


	--納車リスト(倉庫／部品別)
	CREATE TABLE #ServiceSales (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_ServiceSales ON #ServiceSales ([WarehouseCode], [PartsNumber])

	--移動払出リスト(倉庫／部品別)
	CREATE TABLE #TransferDeparture (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_TransferDeparture ON #TransferDeparture ([WarehouseCode], [PartsNumber])

	--棚卸リスト(倉庫／部品別)
	CREATE TABLE #InventoryStock (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_InventoryStock ON #InventoryStock ([WarehouseCode], [PartsNumber])

	--倉庫／部品リスト
	CREATE TABLE #PartsList (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[WarehouseName] NVARCHAR(20)
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[PartsNameJp] NVARCHAR(50)
	,	[Cost] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_PartsList ON #PartsList ([WarehouseCode], [PartsNumber])

	--部品リスト
	CREATE TABLE #PartsListAll (
		[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Cost] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_PartsListAll ON #PartsListAll ([PartsNumber])

	--理論在庫(倉庫／部品別)
	CREATE TABLE #QuantityCalc (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	)
	CREATE UNIQUE INDEX IX_Temp_QuantityCalc ON #QuantityCalc ([WarehouseCode], [PartsNumber])

	--月末在庫(倉庫／部品別)
	CREATE TABLE #QuantityPost (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	)
	CREATE UNIQUE INDEX IX_Temp_QuantityPost ON #QuantityPost ([WarehouseCode], [PartsNumber])

	--棚差(倉庫／部品別)
	CREATE TABLE #QuantityDiff (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_QuantityDiff ON #QuantityDiff ([WarehouseCode], [PartsNumber])

	--平均単価(全社合計／部品別)
	CREATE TABLE #AverageCost (
		[PartsNumber] NVARCHAR(50) NOT NULL
	,	[AverageCost] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_AverageCost ON #AverageCost ([PartsNumber])

	--単価差額(倉庫／部品別)
	CREATE TABLE #DiffCost (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[DiffCost] DECIMAL(18, 0)
	)
	CREATE UNIQUE INDEX IX_DiffCost ON #DiffCost ([WarehouseCode], [PartsNumber])

	--Add 2015/07/17 arc yano
	--引当(倉庫／部品別)
	CREATE TABLE #Reservation (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_Reservation ON #Reservation ([WarehouseCode], [PartsNumber])

	--Add 2015/09/16 arc yano
	--サービス伝票ヘッダ(対象外データ)
	CREATE TABLE #Temp_ServiceSalesHeader_Exempt (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	)

	--仕掛(倉庫／部品別)
	CREATE TABLE #InProcess (
		[WarehouseCode] NVARCHAR(6) NOT NULL			--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_InProcess ON #InProcess ([WarehouseCode], [PartsNumber])

	--受払表(倉庫／部品別)
	CREATE TABLE #PartsBalance (
		[CloseMonth] [datetime] NOT NULL
	,	[DepartmentCode] [nvarchar](3) NOT NULL			--Mod 2016/08/13 arc yano #3596
	,	[DepartmentName] [nvarchar](50)					--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] [nvarchar](25) NOT NULL
	,	[PartsNameJp] [nvarchar](50)
	,	[PreCost] [decimal](10, 0)
	,	[PreQuantity] [decimal](10, 3)
	,	[PreAmount] [decimal](10, 0)
	,	[PurchaseQuantity] [decimal](10, 3)
	,	[PurchaseAmount] [decimal](10, 0)
	,	[TransferArrivalQuantity] [decimal](10, 3)
	,	[TransferArrivalAmount] [decimal](10, 0)
	,	[ShipQuantity] [decimal](10, 3)
	,	[ShipAmount] [decimal](10, 0)
	,	[TransferDepartureQuantity] [decimal](10, 3)
	,	[TransferDepartureAmount] [decimal](10, 0)
	,	[DifferenceQuantity] [decimal](10, 3)
	,	[DifferenceAmount] [decimal](10, 0)
	,	[UnitPriceDifference] [decimal](10, 0)
	,	[PostCost] [decimal](10, 0)
	,	[PostQuantity] [decimal](10, 3)
	,	[PostAmount] [decimal](10, 0)
	,	[InProcessQuantity] [decimal](10, 3)
	,	[InProcessAmount] [decimal](10, 0)
	,	[PurchaseOrderPrice] [decimal](10, 0)
	,	[CalculatedDate] [datetime]
	,	[CreateEmployeeCode] [nvarchar](50)
	,	[CreateDate] [datetime]
	,	[LastUpdateEmployeeCode] [nvarchar](50)
	,	[LastUpdateDate] [datetime]
	,	[DelFlag] [nvarchar](2)
	,	[QuantityCalc] [decimal](10, 2)					--Add 2015/06/02 arc yano
	,	[AmountCalc] [decimal](10, 0)					--Add 2015/06/02 arc yano
	,	[ReservationQuantity] [decimal](10, 2)			--Add 2015/07/17 arc yano 
	,	[ReservationAmount] [decimal](10, 0)			--Add 2015/07/17 arc yano 
	,	[WarehouseCode] [nvarchar](6) NOT NULL			--Mod 2016/08/13 arc yano #3596
	,	[WarehouseName] [nvarchar](50)					--Mod 2016/08/13 arc yano #3596
	)
	/*************************************************************************/


	--処理結果
	DECLARE @ErrorMessage NVARCHAR(4000) = ''
    DECLARE @ErrorNumber INT = 0

	--当日
	DECLARE @TODAY DATETIME = CONVERT(DATETIME, CONVERT(NVARCHAR(10), GETDATE(), 111), 111)
	--当月1日
	DECLARE @THISMONTH DATETIME = CONVERT(DATETIME, CONVERT(NVARCHAR(7), @TODAY, 111) + '/01', 111)

	--■処理対象月範囲の設定
	--※「受払締め」が締まっている月の中で最大月の翌月の1日<=x<処理当日の翌月1日未満(または、月中の場合は当日未満）
	--■■処理対象月Fromの設定（締まっている月の中で最大月の翌月1日）
	DECLARE @TargetMonthFrom DATETIME = NULL
	SELECT @TargetMonthFrom = DATEADD(m, 1, ISNULL(MAX(CONVERT(datetime, mc.[InventoryMonth], 120)), @THISMONTH))
	FROM [InventoryMonthControlPartsBalance] mc	--受払締
	WHERE mc.[InventoryStatus] = '002'			--本締済
	
	--ADD 2015/05/19 arc yano 不具合対応
	--対象月が未来＝初めて実行、または棚卸締後に初めて実行の場合、
	--実施中のレコードのなかで、最も古い月を取得する。
	--※これは受払い締月の前月も受払い締済であることを前提としている
	IF @TargetMonthFrom > @TODAY
	BEGIN
		SELECT @TargetMonthFrom = ISNULL(MIN(CONVERT(datetime, mc.[InventoryMonth], 120)), DATEADD(m, 1, @THISMONTH))
		FROM [InventoryMonthControlPartsBalance] mc	--棚卸締
		WHERE mc.[InventoryStatus] = '001'			--実施中
	END
	
	--対象追記が未来=初めて実行の場合
	--棚卸の締まっている月の中で最大月の翌月の1日<=x<処理当日の翌月1日未満(または、月中の場合は当日未満）
	IF @TargetMonthFrom > @TODAY
	BEGIN
		SELECT @TargetMonthFrom = DATEADD(m, 1, ISNULL(MAX(CONVERT(DATETIME, mc.[InventoryMonth], 120)), @THISMONTH))
		FROM [InventoryMonthControlParts] mc	--棚卸締
		WHERE mc.[InventoryStatus] = '002'		--本締済
	END
	--それでも対象月が未来になる場合、当月とする（ロジックとしては通らないはず）
	IF @TargetMonthFrom > @TODAY
		SET @TargetMonthFrom = @THISMONTH

	--■■処理対象月Toの設定(処理当日の翌月1日未満)
	DECLARE @TargetMonthTo DATETIME = DATEADD(m, 1, @THISMONTH)

	--処理対象月数／処理対象月前月
	DECLARE @TargetMonthCount INT = DATEDIFF(m, @TargetMonthFrom, @TargetMonthTo)
	DECLARE @TargetMonthPrevious DATETIME = DATEADD(m, -1, @TargetMonthFrom)

	--処理対象日付範囲From／処理対象日付範囲To
	DECLARE @TargetDateFrom DATETIME = @TargetMonthFrom
	DECLARE @TargetDateTo DATETIME = DATEADD(m, 1, @TargetDateFrom)
	IF @TargetDateTo > @TODAY		--日付範囲TOが未来日の場合、今日にする
		SET @TargetDateTo = @TODAY
	--算出日(処理対象日付範囲TOの前日)
	DECLARE @CalcDate DATETIME = DATEADD(d, -1, @TargetDateTo)

	--トランザクション開始 
	BEGIN TRANSACTION
	BEGIN TRY

		--■処理対象月数分ループ
		WHILE @TargetMonthCount > 0
		BEGIN

			--一次表初期化		
			DELETE FROM  #Previous			--前残リスト(倉庫／部品別)
			DELETE FROM  #PreviousAll		--前残リスト(全社合計／部品別)
			DELETE FROM  #PartsPurchase		--仕入リスト(倉庫／部品別)
			DELETE FROM  #PartsPurchaseAll	--仕入リスト(全社合計／部品別)
			DELETE FROM  #ServiceSales		--納車リスト(倉庫／部品別)
			DELETE FROM  #TransferArrival	--移動受入リスト(倉庫／部品別)
			DELETE FROM  #TransferDeparture	--移動払出リスト(倉庫／部品別)
			DELETE FROM  #InventoryStock	--棚卸リスト(倉庫／部品別)
			DELETE FROM  #PartsList			--倉庫／部品リスト
			DELETE FROM  #PartsListAll		--部品リスト
			DELETE FROM  #QuantityCalc		--理論在庫(倉庫／部品別)
			DELETE FROM  #QuantityPost		--月末在庫(倉庫／部品別)
			DELETE FROM  #QuantityDiff		--棚差(倉庫／部品別)
			DELETE FROM  #AverageCost		--平均単価(全社合計／部品別)
			DELETE FROM  #DiffCost			--単価差額(倉庫／部品別)
			DELETE FROM  #Reservation		--引当(倉庫／部品別)
			DELETE FROM  #InProcess			--仕掛(倉庫／部品別)
			DELETE FROM  #PartsBalance		--受払表(倉庫／部品別)
			-----------------------------

			/****************
			■■前残リスト
			****************/
			--前月数量／前月単価を一時テーブルに挿入
			--step1.倉庫別
			INSERT INTO #Previous
			SELECT 
				pb.[WarehouseCode]										--倉庫
			,	pb.[PartsNumber]										--部品番号
			,	pb.[PostCost] AS [Cost]									--単価
			,	pb.[PostQuantity] AS [Quantity] 						--数量
			,	pb.[PostAmount] AS [Amount]								--金額
			FROM [PartsBalance] pb
			WHERE pb.[CloseMonth] = @TargetMonthPrevious				--対象月の前月


			--★[PartsBalace]が存在しない場合（初めての場合）
			IF NOT EXISTS(SELECT 'X' FROM #Previous)
			BEGIN

				INSERT INTO #Previous
				SELECT 
					s.[WarehouseCode]																						--倉庫
				,	s.[PartsNumber]																							--部品番号
				,	ISNULL(pac.[Price], 0) AS [Cost]																		--単価
				,	SUM(ISNULL(s.[Quantity], 0)) AS [Quantity]																--数量
				,	SUM(ROUND(ISNULL(s.[Quantity], 0) * ISNULL(pac.[Price], 0), @RoundLength, @RoundMode)) AS [Amount]		--ROUND(数量*単価)=金額
				FROM [InventoryStock] s																						--部品棚卸
				LEFT JOIN [PartsAverageCost] pac																			--平均単価
					ON pac.CompanyCode = @p_CompanyCode																		--固定？
					AND pac.[CloseMonth] = s.[InventoryMonth]																--対象月の前月
					AND pac.[PartsNumber] = s.[PartsNumber]																	--部品番号
					AND ISNULL(pac.[DelFlag], '0') <> '1'																	--削除フラグ
				WHERE s.[InventoryMonth] = @TargetMonthPrevious																--対象月の前月
				AND ISNULL(s.[DelFlag], '0') <> '1'
				GROUP BY
					s.[WarehouseCode]
				,	s.[PartsNumber]
				,	ISNULL(pac.[Price], 0)

			END
			--インデックス再生成
			DROP INDEX IX_Temp_Previous ON #Previous
			CREATE UNIQUE INDEX IX_Temp_Previous ON #Previous ([WarehouseCode], [PartsNumber])


			--step2.全社
			INSERT INTO #PreviousAll
			SELECT 
				p.[PartsNumber]											--部品番号
			,	p.[Cost]												--単価
			,	SUM(p.[Quantity]) AS [Quantity] 						--数量
			,	SUM(p.[Amount]) AS [Amount]								--金額
			FROM #Previous p
			GROUP BY
				p.[PartsNumber]
			,	p.[Cost]
			--前残のない部品単価を追加
			INSERT INTO #PreviousAll
			SELECT
				pac.[PartsNumber]
			,	ISNULL(pac.[Price], 0) AS [Cost]
			,	0 AS [Quantity] 										--数量
			,	0 AS [Amount]											--金額
			FROM [PartsAverageCost] pac
			WHERE pac.CompanyCode = @p_CompanyCode						--固定？
			AND pac.[CloseMonth] = @TargetMonthPrevious					--対象月の前月
			AND NOT EXISTS(
					SELECT 'X'
					FROM #PreviousAll pa
					WHERE pa.[PartsNumber] = pac.[PartsNumber]			--部品番号
					)
			AND ISNULL(pac.[DelFlag], '0') <> '1'


			--インデックス再生成
			DROP INDEX IX_Temp_PreviousAll ON #PreviousAll
			CREATE UNIQUE INDEX IX_Temp_PreviousAll ON #PreviousAll ([PartsNumber])

		
			/****************
			■■当月仕入
			****************/
			--仕入情報を一時テーブルに挿入
			--step1.倉庫別
			INSERT INTO #PartsPurchase
			SELECT
				l.[WarehouseCode]									--Mod 2016/08/13 arc yano #3596
			,	pp.[PartsNumber]
			,	SUM(CASE pp.[PurchaseType] WHEN '001' THEN ISNULL(pp.[Quantity], 0) ELSE (ISNULL(pp.[Quantity], 0)* -1) END) AS [Quantity]
			,	SUM(ROUND((CASE pp.[PurchaseType] WHEN '001' THEN ISNULL(pp.[Quantity], 0) ELSE (ISNULL(pp.[Quantity], 0)* -1) END) * ISNULL(pp.[Price], 0), @RoundLength, @RoundMode)) AS [Amount] 		--ROUND(数量*単価)=金額
			FROM [PartsPurchase] pp INNER JOIN
			dbo.Location l ON pp.LocationCode = l.LocationCode		--Mod 2016/08/13 arc yano #3596
			WHERE pp.[PurchaseDate] >= @TargetDateFrom
			AND pp.[PurchaseDate] < @TargetDateTo
			AND ISNULL(pp.DelFlag, '0') <> '1'
			AND pp.PurchaseStatus = '002'
			AND l.DelFlag = '0'										--Add 2016/08/13 arc yano #3596
			
			GROUP BY
				l.[WarehouseCode]
			,	pp.[PartsNumber]
			--インデックス再生成
			DROP INDEX IX_Temp_PartsPurchase ON #PartsPurchase
			CREATE UNIQUE INDEX IX_Temp_PartsPurchase ON #PartsPurchase ([WarehouseCode], [PartsNumber])

			--step2.全社
			INSERT INTO #PartsPurchaseAll
			SELECT
				pp.[PartsNumber]
			,	SUM(pp.[Quantity]) AS [Quantity]
			,	SUM(pp.[Amount]) AS [Amount]
			FROM #PartsPurchase pp
			GROUP BY
				pp.[PartsNumber]
			--インデックス再生成
			DROP INDEX IX_Temp_PartsPurchaseAll ON #PartsPurchaseAll
			CREATE UNIQUE INDEX IX_Temp_PartsPurchaseAll ON #PartsPurchaseAll ([PartsNumber])

			/****************
			■■当月移動受入
			****************/
			--移動情報を一時テーブルに挿入
			INSERT INTO #TransferArrival
			SELECT
				l.[WarehouseCode]								--Mod 2016/08/13 arc yano #3596
			,	tr.[PartsNumber]
			,	SUM(ISNULL(tr.[Quantity], 0)) AS [Quantity]
			,	SUM(ROUND(ISNULL(tr.[Quantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]	--ROUND(数量*単価)=金額
			FROM [Transfer] tr
			INNER JOIN Parts p							--部品マスタ
				ON p.[PartsNumber] = tr.[PartsNumber]
			INNER JOIN [Location] l				--受入ﾛｹｰｼｮﾝ
				ON l.[LocationCode] = tr.[ArrivalLocationCode]
			INNER JOIN [Location] l2				--出荷ﾛｹｰｼｮﾝ
				ON l2.[LocationCode] = tr.[DepartureLocationCode]
			LEFT JOIN #PreviousAll pa					--前残情報
				ON pa.[PartsNumber] = tr.[PartsNumber]
			WHERE tr.[ArrivalDate] >= @TargetDateFrom
			AND tr.[ArrivalDate] < @TargetDateTo
			AND ISNULL(tr.[DelFlag], '0') <> '1'
			AND l.WarehouseCode <> l2.WarehouseCode
			GROUP BY
				l.[WarehouseCode]								--Mod 2016/08/13 arc yano #3596
			,	tr.[PartsNumber]
			--インデックス再生成
			DROP INDEX IX_Temp_TransferArrival ON #TransferArrival
			CREATE UNIQUE INDEX IX_Temp_TransferArrival ON #TransferArrival ([WarehouseCode], [PartsNumber])

			/****************
			■■当月納車
			****************/
			--納車情報を一時テーブルに挿入
			INSERT INTO #ServiceSales
			SELECT
				dw.[WarehouseCode]												--Mod 2016/08/13 arc yano #3596
			,	sl.[PartsNumber]
			,	SUM(ISNULL(sl.[ProvisionQuantity], 0)) AS [Quantity]			--Mod 2019/02/09 yano #3969
			--,	SUM(ISNULL(sl.[Quantity], 0)) AS [Quantity]
			,	SUM(ROUND(ISNULL(sl.[ProvisionQuantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]		--ROUND(数量*単価)=金額	--Mod 2019/02/09 yano #3954
			--,	SUM(ROUND(ISNULL(sl.[Quantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]
			FROM [ServiceSalesHeader] sh										--サービスオーダーヘッダ
			INNER JOIN [ServiceSalesLine] sl									--サービスオーダー明細
				ON sl.[SlipNumber] = sh.[SlipNumber] 
				AND ISNULL(sl.[DelFlag], '0') <> '1'
			INNER JOIN Parts p													--部品マスタ
				ON p.[PartsNumber] = sl.[PartsNumber]
			INNER JOIN DepartmentWarehouse dw									--部門・倉庫組合せマスタ		--Mod 2016/08/13 arc yano #3596
				ON sh.[DepartmentCode] = dw.[DepartmentCode]
			LEFT JOIN #PreviousAll pa											--前残情報
				ON pa.[PartsNumber] = sl.[PartsNumber]
			WHERE sh.[SalesDate] >= @TargetDateFrom								--当月1日以上
			AND sh.[SalesDate] < @TargetDateTo									--処理日未満
			AND sh.[ServiceOrderStatus] = '006'									--納車済
			AND ISNULL(sh.DelFlag, '0') <> '1'
			AND ISNULL(dw.DelFlag, '0') <> '1'
			GROUP BY
				dw.[WarehouseCode]												--Mod 2016/08/13 arc yano #3596
			,	sl.[PartsNumber]
			--インデックス再生成
			DROP INDEX IX_Temp_ServiceSales ON #ServiceSales
			CREATE UNIQUE INDEX IX_Temp_ServiceSales ON #ServiceSales ([WarehouseCode], [PartsNumber])

			/****************
			■■当月移動払出
			****************/
			--移動情報を一時テーブルに挿入
			INSERT INTO #TransferDeparture
			SELECT
				l.[WarehouseCode]
			,	tr.[PartsNumber]
			,	SUM(ISNULL(tr.[Quantity], 0)) AS [Quantity]
			,	SUM(ROUND(ISNULL(tr.[Quantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]--ROUND(数量*単価)=金額
			FROM [Transfer] tr
			INNER JOIN Parts p																		--部品マスタ
				ON p.[PartsNumber] = tr.[PartsNumber]
			INNER JOIN [Location] l																	--出荷ﾛｹｰｼｮﾝ
				ON l.[LocationCode] = tr.[DepartureLocationCode]
			INNER JOIN [Location] l2																--受入ﾛｹｰｼｮﾝ
				ON l2.[LocationCode] = tr.[ArrivalLocationCode]
			LEFT JOIN #PreviousAll pa																--前残情報
				ON pa.[PartsNumber] = tr.[PartsNumber]
			WHERE tr.[DepartureDate] >= @TargetDateFrom
			AND tr.[DepartureDate] < @TargetDateTo
			AND ISNULL(tr.[DelFlag], '0') <> '1'
			AND l.WarehouseCode <> l2.WarehouseCode
			GROUP BY
				l.[WarehouseCode]
			,	tr.[PartsNumber]
			--インデックス再生成
			DROP INDEX IX_Temp_TransferDeparture ON #TransferDeparture
			CREATE UNIQUE INDEX IX_Temp_TransferDeparture ON #TransferDeparture ([WarehouseCode], [PartsNumber])


			/****************
			■■当月棚卸
			****************/
			--棚卸完了ステータス（締め状況）
			DECLARE @isExistsInventoryStock bit = 0			--0：未、1:締
			DECLARE @CloseDateTime DATETIME = NULL
			--当月が締まっているかどうかを判定
			SELECT 
				@isExistsInventoryStock = CASE mc.[InventoryStatus] WHEN '002' THEN 1 ELSE 0 END				--002:棚卸完了
			,	@CloseDateTime = CASE mc.[InventoryStatus] WHEN '002' THEN mc.LastUpdateDate ELSE @CalcDate END	--棚卸確定日時(暫定)
			FROM [InventoryMonthControlParts] mc
			WHERE mc.InventoryMonth = @TargetDateFrom
			--締まっている場合のみデータ取得
			IF @isExistsInventoryStock = 1
			BEGIN
				--棚卸情報を一時テーブルに挿入
				INSERT INTO #InventoryStock
				SELECT
					s.[WarehouseCode]																			--Mod 2016/08/13 arc yano #3596
				,	s.[PartsNumber]
				,	SUM(ISNULL(s.[PhysicalQuantity], 0)) AS [Quantity]
				,	SUM(ROUND(ISNULL(s.[PhysicalQuantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]--ROUND(数量*単価)=金額
				FROM [InventoryStock] s
				INNER JOIN Parts p							--部品マスタ
					ON p.[PartsNumber] = s.[PartsNumber]
				LEFT JOIN #PreviousAll pa					--前残情報
					ON pa.[PartsNumber] = s.[PartsNumber]
				WHERE s.InventoryMonth = @TargetDateFrom
				AND ISNULL(s.[DelFlag], '0') <> '1'
				GROUP BY
					s.[WarehouseCode]																			--Mod 2016/08/13 arc yano #3596
				,	s.[PartsNumber]
				--インデックス再生成
				DROP INDEX IX_Temp_InventoryStock ON #InventoryStock
				CREATE UNIQUE INDEX IX_Temp_InventoryStock ON #InventoryStock ([WarehouseCode], [PartsNumber])
			END

			
			--Add 2015/07/17
			/****************
			■■引当
			****************/
			--引当は棚卸完了まではPartsStockから、棚卸完了後はInventoryStockから算出する
			--※仕掛と異なり、単純にサービス伝票の情報では計算できない
			IF @isExistsInventoryStock = 1											--棚卸完了
			BEGIN
				INSERT INTO #Reservation
				SELECT
					ivs.[WarehouseCode]																			--Mod 2016/08/13 arc yano #3596
				,	ivs.[PartsNumber]
				,	SUM(ISNULL(ivs.[ProvisionQuantity], 0)) AS [Quantity]		--Mod 2016/02/03
				,	SUM(ROUND(ISNULL(ivs.[ProvisionQuantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]	--ROUND(数量*単価)=金額	--Mod 2016/02/03
				FROM [InventoryStock] ivs						--部品棚卸テーブル
				INNER JOIN Parts p								--部品マスタ
					ON p.[PartsNumber] = ivs.[PartsNumber]
				LEFT JOIN #PreviousAll pa						--前残情報
					ON pa.[PartsNumber] = ivs.[PartsNumber]
				WHERE
					ivs.InventoryMonth = @TargetDateFrom
					AND exists
					(
						SELECT 'X' FROM dbo.Location l where l.LocationType <> '003' AND ISNULL(l.DelFlag, '0') <> '1' AND l.LocationCode = ivs.LocationCode
					)
					AND ISNULL(ivs.DelFlag, '0') <> '1'
					AND ISNULL(p.DelFlag, '0') <> '1'
					AND ISNULL(p.NonInventoryFlag, '0') <> '1'
				GROUP BY
					ivs.[WarehouseCode]																			--Mod 2016/08/13 arc yano #3596
				,	ivs.[PartsNumber]
			END
			ELSE	--未確定時はPartsStockから算出
			BEGIN
				INSERT INTO #Reservation
				SELECT
					l.[WarehouseCode]																			--Mod 2016/08/13 arc yano #3596
				,	ps.[PartsNumber]
				,	SUM(ISNULL(ps.[ProvisionQuantity], 0)) AS [Quantity] --Mod 2016/02/03
				,	SUM(ROUND(ISNULL(ps.[ProvisionQuantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]	--ROUND(数量*単価)=金額
				FROM [PartsStock] ps							--部品棚卸テーブル
				INNER JOIN Parts p								--部品マスタ
					ON p.[PartsNumber] = ps.[PartsNumber]
				INNER JOIN Location l							--ロケーションマスタ
					ON l.[LocationCode] = ps.[LocationCode]
				LEFT JOIN #PreviousAll pa						--前残情報
					ON pa.[PartsNumber] = ps.[PartsNumber]
				WHERE
					l.LocationType <> '003'						--ロケーションタイプ≠「仕掛」
					AND ISNULL(ps.DelFlag, '0') <> '1'
					AND ISNULL(p.DelFlag, '0') <> '1'
					AND ISNULL(p.NonInventoryFlag, '0') <> '1'
					AND ISNULL(l.DelFlag, '0') <> '1'
				GROUP BY
					l.[WarehouseCode]																			--Mod 2016/08/13 arc yano #3596
				,	ps.[PartsNumber]
			END
			--インデックス再生成
			DROP INDEX IX_Temp_Reservation ON #Reservation
			CREATE UNIQUE INDEX IX_Temp_Reservation ON #Reservation ([WarehouseCode], [PartsNumber])			--Mod 2016/08/13 arc yano #3596

			/****************
			■■仕掛
			****************/
			--Add 2015/09/16 arc yano
			--サービス伝票対象外データを取得
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
			
			--------------------------------------------------------------
			--サービス伝票対象外データの取得(対象月に納車済)
			--------------------------------------------------------------
			INSERT INTO #Temp_ServiceSalesHeader_Exempt
			SELECT
				sh.[SlipNumber]												--伝票番号
			FROM
				dbo.ServiceSalesHeader sh
			WHERE 
				sh.[ServiceOrderStatus] = '006'								--006:納車済
			AND ISNULL(sh.[SalesDate], @TargetDateTo) < @TargetDateTo		--当月中に納車済
			AND sh.[DelFlag] = '0'
			
			--仕掛を一時テーブルに挿入
			INSERT INTO #InProcess
			SELECT
				dw.[WarehouseCode]											--Mod 2016/08/13 arc yano #3596											
			,	sl.[PartsNumber]
			,	SUM(ISNULL(sl.[ProvisionQuantity], 0)) AS [Quantity]		--Mod 2016/02/03
			,	SUM(ROUND(ISNULL(sl.[ProvisionQuantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]--ROUND(数量*単価)=金額		--Mod 2016/02/03
			FROM [ServiceSalesHeader] sh									--サービスオーダーヘッダ
			INNER JOIN [ServiceSalesLine] sl								--サービスオーダー明細
				ON sl.[SlipNumber] = sh.[SlipNumber] 
				AND ISNULL(sl.[DelFlag], '0') <> '1'
			INNER JOIN [DepartmentWarehouse] dw								--部門・倉庫組合せマスタ	--Add 2016/08/13 arc yano #3596
				ON sh.[DepartmentCode] = dw.[DepartmentCode] 
			INNER JOIN Parts p												--部品マスタ
				ON p.[PartsNumber] = sl.[PartsNumber]
			LEFT JOIN #PreviousAll pa										--前残情報
				ON pa.[PartsNumber] = sl.[PartsNumber]
			WHERE sh.[WorkingStartDate] < @TargetDateTo	--対象終了日前に作業開始している伝票
			--当月以前にキャンセル/作業中止のない伝票
			AND NOT EXISTS(
				SELECT 'X'
				FROM #Temp_ServiceSalesHeader_Exempt sub
				WHERE sub.[SlipNumber] = sh.[SlipNumber]
				)
			AND ISNULL(sh.DelFlag, '0') <> '1'
			AND ISNULL(dw.DelFlag, '0') <> '1'
			GROUP BY
				dw.[WarehouseCode]											--Mod 2016/08/13 arc yano #3596											
			,	sl.[PartsNumber]
		
			--インデックス再生成
			DROP INDEX IX_Temp_InProcess ON #InProcess
			CREATE UNIQUE INDEX IX_Temp_InProcess ON #InProcess ([WarehouseCode], [PartsNumber])

			----------------------------------------------------------------------------------------------------------------
			-- 2015/09/16 arc yano 部品在庫確認不具合(在庫の変動が、仕掛、引当のみの場合、在庫の計算対象に含まれない)の対応
			----------------------------------------------------------------------------------------------------------------
			/****************
			■■部品リスト
			****************/
			--部品リスト
			INSERT INTO #PartsList
			SELECT 
				w.[WarehouseCode]										--倉庫コード
			,	w.[WarehouseName]										--倉庫名
			,	p.[PartsNumber]											--部品番号
			,	p.[PartsNameJp]											--部品名
			,	COALESCE(p.[SoPrice], p.[Cost], 0) AS [Cost]			--単価(StockOrderPrice=通常発注単価⇒標準原価⇒0)
			FROM (
				--存在する倉庫／部品の組み合わせを取得
				SELECT [WarehouseCode], [PartsNumber]		
				FROM #Previous											--前残
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #PartsPurchase										--当月仕入
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #TransferArrival									--当月移動受入
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #ServiceSales										--当月納車
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #TransferDeparture									--当月移動払出
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #InventoryStock									--当月棚卸
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #Reservation										--当月引当	--add 2015/09/16
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #InProcess											--当月仕掛	--add 2015/09/16
				) dp
			INNER JOIN [Warehouse] w									--倉庫
				ON w.[WarehouseCode] = dp.[WarehouseCode]
			INNER JOIN [Parts] p										--部品
				ON p.[PartsNumber] = dp.[PartsNumber] 
				AND ISNULL(p.[DelFlag], 0) <> '1'
				AND ISNULL(p.NonInventoryFlag, '0') <> '1'				--在庫管理対象外ではない
				AND ISNULL(w.[DelFlag], 0) <> '1'

			--インデックス再生成
			DROP INDEX IX_Temp_PartsList ON #PartsList
			CREATE UNIQUE INDEX IX_Temp_PartsList ON #PartsList ([WarehouseCode], [PartsNumber])

			/****************
			■■部品リスト(ALL）
			****************/
			--部品リスト
			INSERT INTO #PartsListAll
			SELECT
				p.[PartsNumber]											--部品番号
			,	COALESCE(p.[SoPrice], p.[Cost], 0) AS [Cost]			--単価
			FROM [Parts] p
			WHERE ISNULL(p.[DelFlag], 0) <> '1'
			AND ISNULL(p.NonInventoryFlag, '0') <> '1'					--在庫管理対象外ではない

			--インデックス再生成
			DROP INDEX IX_Temp_PartsListAll ON #PartsListAll
			CREATE UNIQUE INDEX IX_Temp_PartsListAll ON #PartsListAll ([PartsNumber])

			/****************
			■■理論在庫
			****************/
			--（前月末在庫＋当月仕入＋当月移動受入－当月納車－当月移動払出）＝理論在庫情報を一時テーブルに挿入
			INSERT INTO #QuantityCalc
			SELECT
				pl.[WarehouseCode]
			,	pl.[PartsNumber]
			,	SUM(ISNULL(p.[Quantity], 0) 
				+	ISNULL(pp.[Quantity], 0)
				+	ISNULL(ta.[Quantity], 0)
				-	ISNULL(ss.[Quantity], 0)
				-	ISNULL(td.[Quantity], 0)
				) AS [Quantity]
			FROM #PartsList pl				--部品リスト
			LEFT JOIN #Previous p			--前残
				ON p.[WarehouseCode] = pl.WarehouseCode AND p.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #PartsPurchase pp		--仕入
				ON pp.[WarehouseCode] = pl.WarehouseCode AND pp.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #TransferArrival ta	--移動受入
				ON ta.[WarehouseCode] = pl.WarehouseCode AND ta.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #ServiceSales ss		--納車
				ON ss.[WarehouseCode] = pl.WarehouseCode AND ss.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #TransferDeparture td	--移動払出
				ON td.[WarehouseCode] = pl.WarehouseCode AND td.[PartsNumber] = pl.[PartsNumber]
			GROUP BY
				pl.[WarehouseCode]
			,	pl.[PartsNumber]
			--インデックス再生成
			DROP INDEX IX_Temp_QuantityCalc ON #QuantityCalc
			CREATE UNIQUE INDEX IX_Temp_QuantityCalc ON #QuantityCalc ([WarehouseCode], [PartsNumber])

			/****************
			■■実棚在庫
			****************/
			--締まっている場合、棚卸数量を月末数とする
			
			IF @isExistsInventoryStock = 1
			BEGIN
				--棚卸情報を一時テーブルに挿入
				INSERT INTO #QuantityPost
				SELECT
					s.[WarehouseCode]
				,	s.[PartsNumber]
				,	s.[Quantity]
				FROM #InventoryStock s				--棚卸
			END
			
			--締まっていない場合、理論数量を月末数とする
			--Mod 2015/06/02 arc yano
			--ELSE
			/*
			BEGIN
				--（前月末在庫＋当月仕入－当月納車）＝理論在庫情報を一時テーブルに挿入
				INSERT INTO #QuantityPost
				SELECT
					s.[WarehouseCode]
				,	s.[PartsNumber]
				,	s.[Quantity]
				FROM #QuantityCalc s				--理論在庫
			END
			*/
			--インデックス再生成
			DROP INDEX IX_Temp_QuantityPost ON #QuantityPost
			CREATE UNIQUE INDEX IX_Temp_QuantityPost ON #QuantityPost ([WarehouseCode], [PartsNumber])

			/****************
			■■平均単価
			****************/

			--新単価情報(月次平均)を一時テーブルに挿入
			INSERT INTO #AverageCost
			SELECT
				pla.PartsNumber
			,	CASE 
				--ゼロ0割が発生する場合、または仕入がない場合、前月単価のままとする 
				WHEN ((ISNULL(pa.[Quantity], 0) + ISNULL(pp.Quantity, 0)) = 0) OR (ISNULL(pp.Quantity, 0) = 0) THEN COALESCE(pa.[Cost], pla.[Cost], 0)
				--平均単価算出
				ELSE ROUND((ISNULL(pa.[Amount], 0) + ISNULL(pp.[Amount], 0)) / (ISNULL(pa.[Quantity], 0) + ISNULL(pp.Quantity, 0)), @RoundLength, @RoundMode)--ROUND(数量*単価)=金額
				END AS [AverageCost]
			FROM #PartsListAll pla
			LEFT JOIN #PreviousAll pa				--前残（全社）
				ON pa.PartsNumber = pla.PartsNumber
			LEFT JOIN #PartsPurchaseAll pp			--仕入（全社）
				ON pp.PartsNumber = pla.PartsNumber
			--インデックス再生成
			DROP INDEX IX_AverageCost ON #AverageCost
			CREATE UNIQUE INDEX IX_AverageCost ON #AverageCost ([PartsNumber])

			/****************
			■■棚差
			****************/
			--Mod 2015/06/02 棚差は棚卸確定時のみ表示
			IF @isExistsInventoryStock = 1
			BEGIN
				INSERT INTO #QuantityDiff
				SELECT
					pl.[WarehouseCode]
				,	pl.[PartsNumber]
				,	(ISNULL(qp.[Quantity], 0) - ISNULL(qc.[Quantity], 0)) AS [Quantity]	--棚差（実棚数－理論数)
				,	ROUND((ISNULL(qp.[Quantity], 0) - ISNULL(qc.[Quantity], 0)) * COALESCE(ac.[AverageCost], pl.[Cost], 0), @RoundLength, @RoundMode) AS [Amount]	--棚差額--ROUND(数量*単価)=金額
				FROM #PartsList pl
				LEFT JOIN #AverageCost ac				--Mod 2015/06/01 arc yano 棚差の単価は月初単価ではなく月末単価で計算 
					ON ac.[PartsNumber] = pl.[PartsNumber]
				LEFT JOIN #QuantityCalc qc			--論理数
					ON qc.WarehouseCode = pl.WarehouseCode AND qc.PartsNumber = pl.PartsNumber
				LEFT JOIN #QuantityPost qp			--実棚数
					ON qp.WarehouseCode = pl.WarehouseCode AND qp.PartsNumber = pl.PartsNumber
			END
			--インデックス再生成
			DROP INDEX IX_Temp_QuantityDiff ON #QuantityDiff
			CREATE UNIQUE INDEX IX_Temp_QuantityDiff ON #QuantityDiff ([WarehouseCode], [PartsNumber])

			/****************
			■■単価差額
			****************/
			--Mod 2015/06/02 単価差額の計算式から、棚差を削除。また大元のデータを実棚在庫から理論在庫に変更
			--																 
			--単価差額を一時テーブルに挿入
			INSERT INTO #DiffCost
			SELECT
				pl.[WarehouseCode]
			,	pl.[PartsNumber]
			,	SUM(
				  --(ISNULL(qp.[Quantity], 0) * ISNULL(ac.[AverageCost], 0))
				  (ISNULL(qc.[Quantity], 0) * ISNULL(ac.[AverageCost], 0)) 
				- (ISNULL(p.[Amount], 0)	--前残
				 + ISNULL(pp.[Amount], 0)	--仕入
				 + ISNULL(ta.[Amount], 0)	--移動受入
				 - ISNULL(ss.[Amount], 0)	--納車
				 - ISNULL(td.[Amount], 0)	--移動払出
				-- + ISNULL(qd.[Amount], 0)	--棚差
				 )
				) AS [DiffCost]
			FROM #PartsList pl
			--INNER JOIN #QuantityPost qp					--月末在庫
			--	ON qp.[WarehouseCode] = pl.[WarehouseCode] AND qp.[PartsNumber] = pl.[PartsNumber]
			INNER JOIN #QuantityCalc qc						--理論在庫
				ON qc.[WarehouseCode] = pl.[WarehouseCode] AND qc.[PartsNumber] = pl.[PartsNumber]
			INNER JOIN #AverageCost ac					--新単価
				ON ac.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #Previous p						--前残
				ON p.[WarehouseCode] = pl.[WarehouseCode] AND p.PartsNumber = pl.PartsNumber
			LEFT JOIN #PartsPurchase pp					--仕入(受入）
				ON pp.[WarehouseCode] = pl.[WarehouseCode] AND pp.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #TransferArrival ta				--移動受入
				ON ta.[WarehouseCode] = pl.[WarehouseCode] AND ta.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #ServiceSales ss					--納車(払出）
				ON ss.[WarehouseCode] = pl.[WarehouseCode] AND ss.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #TransferDeparture td				--移動払出
				ON td.[WarehouseCode] = pl.[WarehouseCode] AND td.[PartsNumber] = pl.[PartsNumber]
			/*	--Del 2015/06/02 arc yano
			LEFT JOIN #QuantityDiff  qd					--棚差
				ON qd.[WarehouseCode] = pl.[WarehouseCode] AND qd.[PartsNumber] = pl.[PartsNumber]
			*/
			GROUP BY
				pl.[WarehouseCode]
			,	pl.[PartsNumber]
			--インデックス再生成
			DROP INDEX IX_DiffCost ON #DiffCost
			CREATE UNIQUE INDEX IX_DiffCost ON #DiffCost ([WarehouseCode], [PartsNumber])
			
			/****************
			■■受払表
			****************/
			--受払情報を一時テーブルに挿入
			INSERT INTO #PartsBalance
			SELECT 
				@TargetDateFrom AS [CloseMonth]
			,	'' AS [DepartmentCode]
			,	'' AS [DepartmentName]
			,	pl.[PartsNumber] AS [PartsNumber]
			,	pl.[PartsNameJp] AS [PartsNameJp]
			,	COALESCE(pa.[Cost], pl.[Cost], 0) AS [PreCost]									--前残単価
			,	ISNULL(p.[Quantity], 0) AS [PreQuantity]										--前残数
			,	ISNULL(p.[Amount], 0) AS [PreAmount]											--前残金額
			,	ISNULL(pp.[Quantity], 0) AS [PurchaseQuantity]									--仕入数量
			,	ISNULL(pp.[Amount], 0) AS [PurchaseAmount]										--仕入金額
			,	ISNULL(ta.[Quantity], 0) AS [TransferArrivalQuantity]							--移動受入数量
			,	ISNULL(ta.[Amount], 0) AS [TransferArrivalAmount]								--移動受入金額
			,	ISNULL(ss.[Quantity], 0) AS [ShipQuantity]										--納車数量
			,	ISNULL(ss.[Amount], 0) AS [ShipAmount]											--納車金額
			,	ISNULL(td.[Quantity], 0) AS [TransferDepartureQuantity]							--移動払出数量
			,	ISNULL(td.[Amount], 0) AS [TransferDepartureAmount]								--移動払出金額
			,	ISNULL(qd.[Quantity], 0) AS [DifferenceQuantity]								--棚差数
			,	ISNULL(qd.[Amount], 0) AS [DifferenceAmount]									--棚差金額
			,	ISNULL(dc.[DiffCost], 0) AS [UnitPriceDifference]								--単価差額
			,	ISNULL(ac.[AverageCost], 0) AS [PostCost]										--月末単価
			,	ISNULL(qp.[Quantity], 0) AS [PostQuantity]										--月末数
			,	ISNULL(qp.[Quantity] * ac.[AverageCost], 0) AS [PostAmount]						--月末金額
			,	ISNULL(ip.[Quantity], 0) AS [InProcessQuantity]									--仕掛数
			,	ISNULL(ip.[Quantity], 0) * ISNULL(ac.[AverageCost], 0) AS [InProcessAmount]		--仕掛金額
			,	ISNULL(pl.[Cost], 0) AS [PurcharceOrderPrice]									--発注単価
			,	@CalcDate AS [CalcDate]															--算出日
			,	'sys' AS [CreateEmployeeCode]													--作成者	--Mod 2015/06/18
			,	GETDATE() AS [CreateDate]														--作成日
			,	'sys' AS [LastUpdateEmployeeCode]												--更新者	--Mod 2015/06/18
			,	GETDATE() AS [LastUpdateDate]													--更新日
			,	'0' AS [DelFlag]																--削除フラグ
			,	ISNULL(qc.[Quantity], 0) AS [QuantityCalc]										--理論数量	--Mod 2015/06/02
			,	ISNULL(qc.[Quantity] * ac.[AverageCost], 0) AS [AmountCalc]						--理論金額	--Mod 2015/06/02
			,	ISNULL(rs.[Quantity], 0) AS [ReservationQuantity]								--引当数	--Mod 2015/07/17
			,	ISNULL(rs.[Quantity], 0) * ISNULL(ac.[AverageCost], 0) AS [ReservationAmount]	--引当金額	--Mod 2015/07/17
			,	pl.[WarehouseCode]																--倉庫コード--Mod 2016/08/13 arc yano #3596
			,	pl.[WarehouseName]																--倉庫名	--Mod 2016/08/13 arc yano #3596
			FROM #PartsList pl									--部品リスト
			LEFT JOIN #PreviousAll pa							--前残リストALL
				ON pa.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #Previous p								--前残
				ON p.[WarehouseCode] = pl.[WarehouseCode] AND p.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #PartsPurchase pp							--仕入(受入）
				ON pp.[WarehouseCode] = pl.[WarehouseCode] AND pp.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #TransferArrival ta						--移動受入
				ON ta.[WarehouseCode] = pl.[WarehouseCode] AND ta.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #ServiceSales ss							--納車(払出）
				ON ss.[WarehouseCode] = pl.[WarehouseCode] AND ss.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #TransferDeparture td						--移動払出
				ON td.[WarehouseCode] = pl.[WarehouseCode] AND td.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #QuantityDiff  qd							--棚差
				ON qd.[WarehouseCode] = pl.[WarehouseCode] AND qd.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #DiffCost dc								--単価差
				ON dc.[WarehouseCode] = pl.[WarehouseCode] AND dc.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #AverageCost ac							--新単価
				ON ac.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #QuantityPost qp							--月末在庫
				ON qp.[WarehouseCode] = pl.[WarehouseCode] AND qp.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #InProcess ip								--仕掛
				ON ip.[WarehouseCode] = pl.[WarehouseCode] AND ip.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #QuantityCalc qc							--理論在庫					--Mod 2015/06/02 arc yano
				ON qc.[WarehouseCode] = pl.[WarehouseCode] AND qc.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #Reservation rs							--引当						--Mod 2015/07/17 arc yano
				ON rs.[WarehouseCode] = pl.[WarehouseCode] AND rs.[PartsNumber] = pl.[PartsNumber]



			--★受払テーブル更新
			--処理対象月の既存データ削除(対象年月以降を削除）
			DELETE FROM [PartsBalance] WHERE [CloseMonth] >= @TargetDateFrom
			--処理対象月のデータ挿入
			INSERT INTO [PartsBalance]
			SELECT pb.* 
			FROM #PartsBalance pb
			WHERE @p_isShowAllZero = 1
			OR (
				@p_isShowAllZero = 0
				AND NOT (
					pb.[PreQuantity] = 0
				AND	pb.[PurchaseQuantity] = 0
				AND	pb.[TransferArrivalQuantity] = 0
				AND	pb.[ShipQuantity] = 0
				AND	pb.[TransferDepartureQuantity] = 0
				AND	pb.[DifferenceQuantity] = 0
				AND	pb.[UnitPriceDifference] = 0
				AND	pb.[PostQuantity] = 0
				AND	pb.[PostQuantity] = 0
				AND pb.[InProcessQuantity] = 0
				AND pb.[QuantityCalc] = 0				--Add 2015/06/02
				AND pb.[ReservationQuantity] = 0		--Add 2015/07/17
				)
			)

			--受払締管理テーブル更新
			IF NOT EXISTS (
					SELECT 'X' 
					FROM [InventoryMonthControlPartsBalance] mc 
					WHERE mc.[InventoryMonth] = CONVERT(NVARCHAR(8), @TargetDateFrom, 112)
					)
			BEGIN
				INSERT INTO [InventoryMonthControlPartsBalance]
				SELECT
					CONVERT(NVARCHAR(8), @TargetDateFrom, 112) AS [InventoryMonth]
				,	CASE @isExistsInventoryStock 
					WHEN 1 THEN '002'
					ELSE '001' END AS  [InventoryStatus]	--棚卸がしまっているかどうか(002:締まっている、以外：締まっていない）
				,	'sys' AS [CreateEmployeeCode]			--Mod 2015/06/18
				,	GETDATE() AS [CreateDate]
				,	'sys' AS [LastUpdateEmployeeCode]		--Mod 2015/06/18
				,	GETDATE() AS [LastUpdateDate]
				,	'0' AS [DelFlag]
			END
			ELSE
			BEGIN
				UPDATE [InventoryMonthControlPartsBalance] SET
					[InventoryStatus] = CASE @isExistsInventoryStock 
										WHEN 1 THEN '002'
										ELSE '001' END
				,	[LastUpdateEmployeeCode] = 'sys'
				,	[LastUpdateDate] = GETDATE()
				WHERE [InventoryMonth] = CONVERT(NVARCHAR(8), @TargetDateFrom, 112)
			END

			
			--★移動平均単価テーブル更新
			--※棚卸が確定している場合のみ実施
			IF @isExistsInventoryStock = 1
			BEGIN
				--処理対象月の既存データ削除(対象年月以降を削除）
				DELETE FROM [PartsAverageCost] WHERE [CloseMonth] >= @TargetDateFrom
				--処理対象月のデータ挿入
				INSERT INTO [PartsAverageCost] 
				SELECT DISTINCT
					@p_CompanyCode AS [CompanyCode]
				,	@TargetDateFrom AS [CloseMonth]
				,	ac.[PartsNumber]
				,	ac.[AverageCost] AS [Price]
				,	@CloseDateTime AS [CloseDateTime]
				,	GETDATE() AS [CreateDate]
				,	'sys' AS [CreateEmployeeCode]			--Mod 2015/06/18
				,	GETDATE() AS [LastUpdateDate]
				,	'sys' AS [LastUpdateEmployeeCode]		--Mod 2015/06/18
				,	'0' AS [DelFlag]
				FROM #AverageCost ac
				WHERE ac.[AverageCost] <> 0

			END

			--次の処理対象月
			SET @TargetMonthCount = @TargetMonthCount - 1				--残月数デクリメント
			SET @TargetMonthPrevious = @TargetDateFrom					--対象月前月インクリメント(＝今回の当月）
			SET @TargetDateFrom = DATEADD(m, 1, @TargetMonthPrevious)	--対象日Fromインクリメント(＝次回の前月＋１）
			SET @TargetDateTo = DATEADD(m, 1, @TargetDateFrom)			--対象日Toインクリメント(＝次回の当月＋１）
			IF @TargetDateTo > @TODAY
				SET @TargetDateTo = @TODAY
			SET @CalcDate = DATEADD(d, -1, @TargetDateTo)				--算出日
		--ループエンド
		END


		--Add 2018/05/14 arc yano #3880
		-- ---------------------------------------------------
		-- 最新月の移動平均単価を移動平均単価テーブルに登録
		-- --------------------------------------------------
		IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PartsMovingAverageCost]') AND type in (N'U'))
		BEGIN
		
			--①移動平均単価テーブル(#AverageCost)に存在する場合は更新を行う
			UPDATE
				dbo.PartsMovingAverageCost
			SET
				 Price = ac.AverageCost
				,DelFlag = '0'
				,LastUpdateEmployeeCode = 'Insert_PartsBalance'
				,LastUpdateDate = GETDATE()
			FROM
				dbo.PartsMovingAverageCost mac INNER JOIN
				#AverageCost ac ON mac.PartsNumber = ac.PartsNumber
			WHERE
				DelFlag = '0'

			--②移動平均単価テーブルに存在しない場合は新規作成
			INSERT INTO
				dbo.PartsMovingAverageCost
			SELECT
				  @p_CompanyCode
				 ,ac.PartsNumber
				 ,ac.AverageCost
				 ,CreateEmployeeCode = 'Insert_PartsBalance'
				 ,CreateDate = GETDATE()
				 ,LastUpdateEmployeeCode = 'Insert_PartsBalance'
				 ,LastUpdateDate = GETDATE()
				 ,'0'
			FROM
				#AverageCost ac
			WHERE
				NOT EXISTS
				(
					select 'x' from dbo.PartsMovingAverageCost mac where ac.PartsNumber = mac.PartsNumber
				)
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
END



GO


