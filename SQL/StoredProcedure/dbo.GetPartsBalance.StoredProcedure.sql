USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsBalance]    Script Date: 2017/06/29 9:08:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 部品棚卸フラグによる絞込に変更
-- 2016/08/13 arc yano #3596 【大項目】倉庫棚統合対応 棚卸の管理を倉庫単位から倉庫単位に変更
CREATE PROCEDURE [dbo].[GetPartsBalance]
	@TargetYear int = NULL				--対象年
,	@TargetMonth int = NULL				--対象月
,	@SummaryMode int = 0				--0:倉庫毎、1:部品毎、2:倉庫／部品毎
,	@WarehouseCode	nvarchar(6) = NULL	--倉庫コード
,	@PartsNumber nvarchar(25) = NULL	--部品番号
,	@PartsNameJp nvarchar(50) = NULL	--部品名称
AS
BEGIN

	--■■一時表の削除
	/*************************************************************************/
	IF OBJECT_ID(N'tempdb..#Temp_WarehouseList', N'U') IS NOT NULL
	DROP TABLE #Temp_WarehouseList;											--倉庫リスト
		
	--■■一時表の宣言
	--倉庫リスト
	CREATE TABLE #Temp_WarehouseList (
		[DepartmentCode] NVARCHAR(3) NOT NULL				-- 部門コード
	,	[WarehouseCode] NVARCHAR(6) NOT NULL				-- 倉庫コード
	)
	CREATE INDEX IX_Temp_WarehouseList ON #Temp_WarehouseList ([DepartmentCode], [WarehouseCode])

	INSERT INTO #Temp_WarehouseList
	SELECT
		 dw.DepartmentCode
		,dw.WarehouseCode
	FROM
	   dbo.DepartmentWarehouse dw
	WHERE
		dw.DelFlag = '0' AND
		EXISTS
		(
			SELECT 'X' FROM dbo.Department d WHERE d.DelFlag = '0' AND d.PartsInventoryFlag = '1' AND dw.DepartmentCode = d.DepartmentCode
			--SELECT 'X' FROM dbo.Department d WHERE d.DelFlag = '0' AND d.CloseMonthFlag = '2' AND dw.DepartmentCode = d.DepartmentCode
		)

	DROP INDEX IX_Temp_WarehouseList ON #Temp_WarehouseList
	CREATE UNIQUE INDEX IX_Temp_WarehouseList ON #Temp_WarehouseList ([DepartmentCode], [WarehouseCode])

	PRINT 'TEST1'

	--対象年月
	DECLARE @CloseMonth DATETIME
	IF ISDATE(CONVERT(NVARCHAR(4), ISNULL(@TargetYear, 0)) + '/' + CONVERT(NVARCHAR(2), ISNULL(@TargetMonth, 0)) + '/01') = 1
	BEGIN
		SET @CloseMonth = CONVERT(NVARCHAR(4), ISNULL(@TargetYear, 0)) + '/' + CONVERT(NVARCHAR(2), ISNULL(@TargetMonth, 0)) + '/01'
	END


	PRINT 'TEST2'
	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13) + CHAR(10)

	SET @SQL = @SQL + 'SELECT' + @CRLF
	SET @SQL = @SQL + ' case when pb.[CloseMonth] is null then @CloseMonth else pb.[CloseMonth] end AS CloseMonth' + @CRLF

	IF @SummaryMode = 1	--部品毎の場合、倉庫はマージ＝NULL
	BEGIN
		SET @SQL = @SQL + ',NULL AS [WarehouseCode]' + @CRLF
		SET @SQL = @SQL + ',NULL AS [WarehouseName]' + @CRLF
	END
	ELSE
	BEGIN
		SET @SQL = @SQL + ',wh.[WarehouseCode]' + @CRLF
		SET @SQL = @SQL + ',wh.[WarehouseName]' + @CRLF
	END

	IF @SummaryMode = 0	--倉庫毎の場合、部品はマージ＝NULL
	BEGIN
		SET @SQL = @SQL + ',NULL AS [PartsNumber]' + @CRLF
		SET @SQL = @SQL + ',NULL AS [PartsNameJp]' + @CRLF
	END
	ELSE
	BEGIN
		SET @SQL = @SQL + ',pb.[PartsNumber]' + @CRLF
		SET @SQL = @SQL + ',pb.[PartsNameJp]' + @CRLF
	END

	IF @SummaryMode = 0	--倉庫毎の場合、前月単価はマージ＝NULL
		SET @SQL = @SQL + ',NULL AS [PreCost]' + @CRLF
	ELSE
		SET @SQL = @SQL + ',pb.[PreCost]' + @CRLF
	
	IF @SummaryMode <> 2	--マージあり
	BEGIN
		SET @SQL = @SQL + ',SUM(pb.[PreQuantity]) AS [PreQuantity]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[PreAmount]) AS [PreAmount]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[PurchaseQuantity]) AS [PurchaseQuantity]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[PurchaseAmount]) AS [PurchaseAmount]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[TransferArrivalQuantity]) AS [TransferArrivalQuantity]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[TransferArrivalAmount]) AS [TransferArrivalAmount]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[ShipQuantity]) AS [ShipQuantity]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[ShipAmount]) AS [ShipAmount]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[TransferDepartureQuantity]) AS [TransferDepartureQuantity]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[TransferDepartureAmount]) AS [TransferDepartureAmount]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[DifferenceQuantity]) AS [DifferenceQuantity]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[DifferenceAmount]) AS [DifferenceAmount]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[UnitPriceDifference]) AS [UnitPriceDifference]' + @CRLF
	END
	ELSE
	BEGIN
		SET @SQL = @SQL + ',pb.[PreQuantity]' + @CRLF
		SET @SQL = @SQL + ',pb.[PreAmount]' + @CRLF
		SET @SQL = @SQL + ',pb.[PurchaseQuantity]' + @CRLF
		SET @SQL = @SQL + ',pb.[PurchaseAmount]' + @CRLF
		SET @SQL = @SQL + ',pb.[TransferArrivalQuantity]' + @CRLF
		SET @SQL = @SQL + ',pb.[TransferArrivalAmount]' + @CRLF
		SET @SQL = @SQL + ',pb.[ShipQuantity]' + @CRLF
		SET @SQL = @SQL + ',pb.[ShipAmount]' + @CRLF
		SET @SQL = @SQL + ',pb.[TransferDepartureQuantity]' + @CRLF
		SET @SQL = @SQL + ',pb.[TransferDepartureAmount]' + @CRLF
		SET @SQL = @SQL + ',pb.[DifferenceQuantity]' + @CRLF
		SET @SQL = @SQL + ',pb.[DifferenceAmount]' + @CRLF
		SET @SQL = @SQL + ',pb.[UnitPriceDifference]' + @CRLF
	END
		
	IF @SummaryMode = 0	--倉庫毎の場合、前月単価はマージ＝NULL
		SET @SQL = @SQL + ',NULL AS [PostCost]' + @CRLF
	ELSE
		SET @SQL = @SQL + ',pb.[PostCost]' + @CRLF

	IF @SummaryMode <> 2	--マージあり
	BEGIN
		SET @SQL = @SQL + ',SUM(pb.[PostQuantity]) AS [PostQuantity]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[PostAmount]) AS  [PostAmount]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[InProcessQuantity]) AS [InProcessQuantity]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[InProcessAmount]) AS [InProcessAmount]' + @CRLF
		--Mod 2015/06/02 arc yano IPO対応(部品棚卸) 理論在庫(数量、金額)追加
		SET @SQL = @SQL + ',SUM(pb.[CalculatedQuantity]) AS [CalculatedQuantity]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[CalculatedAmount]) AS  [CalculatedAmount]' + @CRLF
		--Mod 2015/07/21 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑦ 引当在庫(数量、金額)追加
		SET @SQL = @SQL + ',SUM(pb.[ReservationQuantity]) AS [ReservationQuantity]' + @CRLF
		SET @SQL = @SQL + ',SUM(pb.[ReservationAmount]) AS  [ReservationAmount]' + @CRLF
	END
	ELSE
	BEGIN
		SET @SQL = @SQL + ',pb.[PostQuantity]' + @CRLF
		SET @SQL = @SQL + ',pb.[PostAmount]' + @CRLF
		SET @SQL = @SQL + ',pb.[InProcessQuantity]' + @CRLF
		SET @SQL = @SQL + ',pb.[InProcessAmount]' + @CRLF
		--Mod 2015/06/02 arc yano IPO対応(部品棚卸) 理論在庫(数量、金額)追加
		SET @SQL = @SQL + ',pb.[CalculatedQuantity]' + @CRLF
		SET @SQL = @SQL + ',pb.[CalculatedAmount]' + @CRLF
		--Mod 2015/07/21 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑦ 引当在庫(数量、金額)追加
		SET @SQL = @SQL + ',pb.[ReservationQuantity]' + @CRLF
		SET @SQL = @SQL + ',pb.[ReservationAmount]' + @CRLF
	END

	IF @SummaryMode = 0	--倉庫毎の場合、発注単価はマージ＝NULL
		SET @SQL = @SQL + ',NULL AS [PurchaseOrderPrice]' + @CRLF
	ELSE
		SET @SQL = @SQL + ',pb.[PurchaseOrderPrice]' + @CRLF

	SET @SQL = @SQL + ',pb.[CalculatedDate]' + @CRLF
	SET @SQL = @SQL + 'FROM [dbo].[PartsBalance] pb' + @CRLF
	SET @SQL = @SQL + 'RIGHT JOIN [dbo].[Warehouse] wh ON pb.[WarehouseCode] = wh.[WarehouseCode]'  + @CRLF
	SET @SQL = @SQL + ' AND pb.[CloseMonth] = @CloseMonth' + @CRLF
	SET @SQL = @SQL + ' WHERE wh.[DelFlag] = ''0''' + @CRLF
	SET @SQL = @SQL +'	AND	EXISTS (' + @CRLF
	SET @SQL = @SQL +'		SELECT ''X''' + @CRLF
	SET @SQL = @SQL +'		FROM' + @CRLF
	SET @SQL = @SQL +'			#Temp_WarehouseList twl' + @CRLF
	SET @SQL = @SQL +'		WHERE' + @CRLF
	SET @SQL = @SQL +'			pb.[WarehouseCode] = twl.[WarehouseCode]' + @CRLF
	SET @SQL = @SQL +'	)' + @CRLF
	--SET @SQL = @SQL + ' AND dp.[CloseMonthFlag] = ''2''' + @CRLF --Mod 2015/06/03 arc yano IPO対応(部品棚卸) 部品棚卸対象の倉庫の絞込み

	--倉庫コード条件（任意）
	IF ISNULL(@WarehouseCode, '') <> ''
		SET @SQL = @SQL + 'AND wh.[WarehouseCode] = @WarehouseCode' + @CRLF
	--部品番号条件（任意）
	IF ISNULL(@PartsNumber, '') <> ''
		SET @SQL = @SQL + 'AND pb.[PartsNumber] = @PartsNumber' + @CRLF
	--部品名称条件（任意）
	IF ISNULL(@PartsNameJp, '') <> ''
		SET @SQL = @SQL + 'AND pb.[PartsNameJp] like ''%' + @PartsNameJp + '%''' + @CRLF

	IF @SummaryMode <> 2	--マージあり
	BEGIN
		SET @SQL = @SQL + 'GROUP BY' + @CRLF
		SET @SQL = @SQL + ' pb.[CloseMonth]' + @CRLF
		IF @SummaryMode = 1	--部品毎の場合、倉庫マージ＝NULL
		BEGIN
			SET @SQL = @SQL + ',pb.[PartsNumber]' + @CRLF
			SET @SQL = @SQL + ',pb.[PartsNameJp]' + @CRLF
			SET @SQL = @SQL + ',pb.[PreCost]' + @CRLF
			SET @SQL = @SQL + ',pb.[PostCost]' + @CRLF
			SET @SQL = @SQL + ',pb.[PurchaseOrderPrice]' + @CRLF
		END
		ELSE IF @SummaryMode = 0	--倉庫毎の場合、部品/単価マージ＝NULL
		BEGIN
			SET @SQL = @SQL + ',wh.[WarehouseCode]' + @CRLF
			SET @SQL = @SQL + ',wh.[WarehouseName]' + @CRLF
		END
		SET @SQL = @SQL + ',pb.[CalculatedDate]' + @CRLF
	END

	DECLARE @PARAM NVARCHAR(1000) = '@CloseMonth DATETIME, @WarehouseCode NVARCHAR(6), @PartsNumber NVARCHAR(25), @PartsNameJp NVARCHAR(50)'

	--PRINT @SQL

	EXECUTE sp_executeSQL @SQL, @PARAM, @CloseMonth, @WarehouseCode, @PartsNumber, @PartsNameJp

END


GO


