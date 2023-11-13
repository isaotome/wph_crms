USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsRetentionYears]    Script Date: 2017/07/14 17:56:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	部品年輪表(在庫滞留表)作成処理
-- 2017/07/14 arc yano 年輪表修正
-- =============================================
CREATE PROCEDURE [dbo].[GetPartsRetentionYears]
	@p_Mode INT = 0		---1：集計, 1:明細, 0:同時
,	@p_dtTargetMonth DATETIME = NULL	
AS
BEGIN
	SET NOCOUNT ON;
PRINT 'start:' + convert(nvarchar(20), GETDATE(), 121)

	--定数--------------------------------------------------
	DECLARE @InitialDate DATE = '2010/06/01'	--暫定処理日
	--------------------------------------------------------

	--■■一時表の宣言
	/*************************************************************************/
	BEGIN TRY
		DROP TABLE #InventoryStock
		DROP TABLE #PartsAverageCost
		DROP TABLE #PartsPurchase
		DROP TABLE #ServiceSales
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

	DECLARE @ErrorMessage NVARCHAR(4000) = ''
	DECLARE @ErrorNumber INT = 0
	--対象日付を指定月の1日にする
	DECLARE @dtTargetDate DATE = CONVERT(DATETIME, CONVERT(NVARCHAR(7), @p_dtTargetMonth, 111) + '/01', 111)
	--対象月の末日（経過月算出基準日)を取得
	DECLARE @dtCompareDate DATETIME = DATEADD(d, -1, DATEADD(m, 1, @dtTargetDate))

PRINT 'step-1:' + convert(nvarchar(20), GETDATE(), 121)

	BEGIN TRY
		--■■一時表へデータ挿入
		/*************************************************************************/
		--部品の在庫
		CREATE TABLE #InventoryStock (
			[PartsNumber] NVARCHAR(25) NOT NULL
		,	[PartsNameJp] NVARCHAR(50)
		,	[Quantity] DECIMAL(10 ,2)
		,	[PartsPrice] DECIMAL(10, 0)	--定価(部品マスタ)
		)

		--部品原価
		CREATE TABLE #PartsAverageCost (
			[PartsNumber] NVARCHAR(50) NOT NULL
		,	[Price] DECIMAL(10, 0)
		)
		CREATE UNIQUE INDEX UX_Temp_PartsAverageCost ON #PartsAverageCost ([PartsNumber])


		--部品の最終入庫
		CREATE TABLE #PartsPurchase (
			[PartsNumber] NVARCHAR(50) NOT NULL
		,	[PurchaseDate] DATETIME
		)
		CREATE UNIQUE INDEX UX_Temp_PartsPurchase ON #PartsPurchase ([PartsNumber])


		--部品の最終出庫
		CREATE TABLE #ServiceSales (
			[PartsNumber] NVARCHAR(50) NOT NULL
		,	[SalesDate] DATETIME
		)
		CREATE UNIQUE INDEX UX_Temp_ServiceSales ON #ServiceSales ([PartsNumber])

		--最後の取引日
		CREATE TABLE #LastLispDate (
			[PartsNumber] NVARCHAR(50) NOT NULL
		,	[PurchaseDate] DATETIME
		,	[SalesDate] DATETIME
		,	[LastLispDate] DATETIME
		)

		--経過年
		CREATE TABLE #Retension (
			[PartsNumber] NVARCHAR(50) NOT NULL
		,	[PartsNameJp] NVARCHAR(50)
		,	[Quantity] DECIMAL(10 ,2)
		,	[PartsPrice] DECIMAL(10, 0)	--定価(部品マスタ)
		,	[Price] DECIMAL(10, 0)
		,	[PurchaseDate] DATETIME
		,	[SalesDate] DATETIME
		,	[LastLispDate] DATETIME
		,	[RetensionMonthes] INT
		)


		--集計表
		CREATE TABLE #RetensionSummary (
			[SECTION]	INT NOT NULL
		,	[LEVEL1]	NVARCHAR(30)
		,	[LEVEL2]	NVARCHAR(30)
		,	[LEVEL3]	DECIMAL(13, 0)
		,	[LEVEL4]	NVARCHAR(30)
		)
		CREATE INDEX IX_Temp_RetensionSummary ON #RetensionSummary ([SECTION])
PRINT 'step-2:' + convert(nvarchar(20), GETDATE(), 121)

		/*************************************************************************/
		-- 部品の在庫	
		INSERT INTO #InventoryStock
		SELECT
			S.PartsNumber
		,	M.PartsNameJp
		,	SUM(S.PhysicalQuantity) AS Quantity		--Mod 2017/07/14 arc yano 理論在庫数ではなく実棚在庫数を取得
		,   M.Price AS PartsPrice					--Mod 2017/07/14 arc yano 部品定価の追加
		FROM InventoryStock S	
		INNER JOIN Parts M ON S.PartsNumber = M.PartsNumber
		WHERE S.DelFlag='0' 
		AND S.InventoryMonth = @dtTargetDate
		GROUP BY 
			S.PartsNumber
		,	M.PartsNameJp
		,	M.Price
		HAVING SUM(S.PhysicalQuantity)> 0			--Mod 2017/07/14 arc yano 理論在庫数ではなく実棚在庫数を取得
PRINT 'step-3:' + convert(nvarchar(20), GETDATE(), 121)

		-- 部品原価
		INSERT INTO #PartsAverageCost
		SELECT 
			PartsNumber
		,	Price 
		FROM PartsAverageCost 	
		WHERE DelFlag='0' 
		AND CloseMonth = @dtTargetDate
		and Price <> 0
PRINT 'step-4:' + convert(nvarchar(20), GETDATE(), 121)

		-- 部品の最終入庫	
		INSERT INTO #PartsPurchase
		SELECT 	
			PartsNumber
		,	MAX(ISNULL(PurchaseDate, @InitialDate)) AS PurchaseDate
		FROM PartsPurchase	
		WHERE PurchaseStatus='002' 
		AND PurchaseType='001'	
		AND PurchaseDate <= @dtCompareDate
		GROUP BY 
			PartsNumber	
PRINT 'step-5:' + convert(nvarchar(20), GETDATE(), 121)

		-- 部品の最終出庫	
		INSERT INTO #ServiceSales
		SELECT 	
			L.PartsNumber
		,	MAX(ISNULL(h.SalesDate, @InitialDate)) AS SalesDate
		FROM ServiceSalesHeader H	
		INNER JOIN ServiceSalesLine L ON H.SlipNumber=L.SlipNumber AND H.RevisionNumber=L.RevisionNumber
		WHERE H.DelFlag='0' 
		AND H.ServiceOrderStatus='006'	
		AND H.SalesDate <= @dtCompareDate
		AND L.PartsNumber IS NOT NULL
		GROUP BY 
			L.PartsNumber	
PRINT 'step-6:' + convert(nvarchar(20), GETDATE(), 121)

		-- 最後の取引日
		INSERT INTO #LastLispDate
		SELECT 		
			I.PartsNumber
		,	ISNULL(P.PurchaseDate, @InitialDate) AS PurchaseDate	
		,	ISNULL(S.SalesDate,@InitialDate) AS SalesDate
		,	CASE 
			WHEN ISNULL(P.PurchaseDate, @InitialDate) > ISNULL(S.SalesDate, @InitialDate) THEN ISNULL(P.PurchaseDate, @InitialDate)
			ELSE ISNULL(S.SalesDate, @InitialDate) 
			END As LastLispDate
		FROM #InventoryStock I	-- 部品の在庫	
		LEFT JOIN #PartsPurchase P				-- 部品の最終入庫	
			ON P.PartsNumber = I.PartsNumber	
		LEFT JOIN #ServiceSales S				-- 部品の最終出庫	
			ON S.PartsNumber=I.PartsNumber	
PRINT 'step-7:' + convert(nvarchar(20), GETDATE(), 121)

		--経過月数
		INSERT INTO #Retension
		SELECT 		
			I.PartsNumber AS [PartsNumber]
		,	I.PartsNameJp AS[PartsNameJp]
		,	I.Quantity AS [Quantity]
		,	I.PartsPrice AS [PartsPrice]
		,	C.Price AS [Price]
		,	L.PurchaseDate AS [PurchaseDate]
		,	L.SalesDate AS [SalesDate]
		,	L.LastLispDate AS [LastLispDate]
		,	DATEDIFF(m, L.PurchaseDate, @dtCompareDate) + 1 AS [RetensionMonthes]
		FROM #InventoryStock I	-- 部品の在庫
		INNER JOIN #PartsAverageCost C			-- 部品原価	
			ON C.PartsNumber = I.PartsNumber	
		INNER JOIN #LastLispDate L
			ON L.PartsNumber = I.PartsNumber

PRINT 'step-8:' + convert(nvarchar(20), GETDATE(), 121)


		/*************************************************************************/
		--■■最終出力データ抽出
		/*************************************************************************/
		DECLARE @Y1 INT = 12
		DECLARE @Y2 INT = 24
		DECLARE @Y3 INT = 36
		DECLARE @Y4 INT = 48
		DECLARE @Y5 INT = 60

		IF @p_Mode <= 0		--集計
		BEGIN
			INSERT INTO #RetensionSummary
			SELECT 
				1 AS [SECTION]
			,	'在庫年輪表' AS [LEVEL1]
			,	null AS [LEVEL2]
			,	null AS [LEVEL3]
			,	null AS [LEVEL4]
			UNION ALL
			SELECT 
				2 AS [SECTION]
			,	null AS [LEVEL1]
			,	CONVERT(NVARCHAR(4), YEAR(@p_dtTargetMonth)) + '年' + CONVERT(NVARCHAR(2), MONTH(@p_dtTargetMonth)) + '月末　現在' AS [LEVEL2]
			,	null AS [LEVEL3]
			,	null AS [LEVEL4]
			UNION ALL
			SELECT 
				3 AS [SECTION]
			,	null AS [LEVEL1]
			,	CONVERT(NVARCHAR(10), GETDATE(), 111) + '　抽出' AS [LEVEL2]
			,	null AS [LEVEL3]
			,	null AS [LEVEL4]
			UNION ALL
			SELECT 
				4 AS [SECTION]
			,	null AS [LEVEL1]
			,	null AS [LEVEL2]
			,	null AS [LEVEL3]
			,	null AS [LEVEL4]
			UNION ALL
			SELEcT
				5 AS [SECTION]
			,	'最終取引からの経過年数' AS [LEVEL1]
			,	null AS [LEVEL2]
			,	null AS [LEVEL3]
			,	null AS [LEVEL4]
			UNION ALL
			SELECT 
				6 AS [SECTION]
			,	null AS [LEVEL1]
			,	'1年以上' AS [LEVEL2]
			,	SUM(R.Price * R.Quantity) AS [LEVEL3]
			,	'13ヶ月以上 24ヶ月以内' AS [LEVEL4]
			FROM #Retension R
			WHERE RetensionMonthes > @Y1 AND R.RetensionMonthes <= @Y2
			UNION ALL
			SELECT 
				7 AS [SECTION]
			,	null AS [LEVEL1]
			,	'2年以上' AS [LEVEL2]
			,	SUM(R.Price * R.Quantity) AS [LEVEL3]
			,	'25ヶ月以上 36ヶ月以内' AS [LEVEL4]
			FROM #Retension R
			WHERE RetensionMonthes > @Y2 AND R.RetensionMonthes <= @Y3
			UNION ALL
			SELECT 
				8 AS [SECTION]
			,	null AS [LEVEL1]
			,	'3年以上' AS [LEVEL2]
			,	SUM(R.Price * R.Quantity) AS [LEVEL3]
			,	'37ヶ月以上 48ヶ月以内' AS [LEVEL4]
			FROM #Retension R
			WHERE RetensionMonthes > @Y3 AND R.RetensionMonthes <= @Y4
			UNION ALL
			SELECT 
				9 AS [SECTION]
			,	null AS [LEVEL1]
			,	'4年以上' AS [LEVEL2]
			,	SUM(R.Price * R.Quantity) AS [LEVEL3]
			,	'49ヶ月以上' AS [LEVEL4]
			FROM #Retension R
			WHERE RetensionMonthes > @Y4

			SELECT 
				[LEVEL1]
			,	[LEVEL2]
			,	[LEVEL3]
			,	[LEVEL4] 
			FROM #RetensionSummary 
			ORDER BY
				[SECTION]

PRINT 'step-9:' + convert(nvarchar(20), GETDATE(), 121)

		END

		IF @p_Mode >= 0		--明細
		BEGIN
			SELECT 
				R.PartsNumber
			,	R.PartsNameJp
			,	R.Quantity
			,	R.PartsPrice
			,	R.Price
			,	R.PurchaseDate	
			,	R.SalesDate
			,	R.LastLispDate
			,	R.RetensionMonthes
			,	CASE WHEN R.RetensionMonthes > @Y1 AND R.RetensionMonthes <= @Y2 THEN R.Price * R.Quantity ELSE 0 END AS [Y1]
			,	CASE WHEN R.RetensionMonthes > @Y2 AND R.RetensionMonthes <= @Y3 THEN R.Price * R.Quantity ELSE 0 END AS [Y2]
			,	CASE WHEN R.RetensionMonthes > @Y3 AND R.RetensionMonthes <= @Y4 THEN R.Price * R.Quantity ELSE 0 END AS [Y3]
			,	CASE WHEN R.RetensionMonthes > @Y4 THEN R.Price * R.Quantity ELSE 0 END AS [Y4]
			FROM #Retension R
			ORDER BY 
				R.PartsNumber
PRINT 'step-10:' + convert(nvarchar(20), GETDATE(), 121)

		END

	END TRY
	BEGIN CATCH
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


