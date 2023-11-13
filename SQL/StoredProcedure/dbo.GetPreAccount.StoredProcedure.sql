USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsWipStock]    Script Date: 2017/11/07 16:20:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- ===================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/10/19 arc yano  #3803  サービス伝票 部品発注書の出力 c_StockStatusのSelectedGenuineType廃止による修正
-- 2017/02/06 arc yano  #3645  部品入荷入力　入荷確定時のエラー LineContentsのサイズを変更(nvarchar(25)→nvarchar(50))
-- 2016/08/13 arc yano  #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
-- 2015/10/14 arc yano  部品在庫棚卸不具合対応(部品仕掛在庫画面の仕掛データ抽出不具合)②
-- 2015/09/11 arc yano  部品在庫棚卸不具合対応(部品仕掛在庫画面の仕掛データ抽出不具合)
-- Description:	<Description,,>
-- 仕掛在庫の表示・保存
-- ===================================================================================================================
CREATE PROCEDURE [dbo].[GetPartsWipStock] 
	@ActionFlag int = 0,						--動作指定(0:画面表示, 1:スナップショット保存)
	@TargetMonth datetime,						--指定月	
	@DepartmentCode NVARCHAR(3) ,				--部門コード
	@WarehouseCode NVARCHAR(6) ,				--倉庫コード
	@ServiceType NVARCHAR(3) = NULL,			--種別(002:外注、 003:部品)
	@ArrivalPlanDateFrom NVARCHAR(10) = NULL,	--入庫日(From)
	@ArrivalPlanDateTo NVARCHAR(10) = NULL,		--入庫日(To)
	@SlipNumber NVARCHAR(50) = NULL,			--伝票番号
	@PartsNumber NVARCHAR(25) = NULL,			--部品番号
	@PartsNameJp NVARCHAR(50) = NULL,			--部品名
	@Vin NVARCHAR(20) = NULL,					--車台番号
	@CustomerName NVARCHAR(80) = NULL			--顧客名(漢字／カナ)
AS 
BEGIN
--変数宣言
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-------------------------------
	--対象テーブル名/機能概要を指定
	-------------------------------
	DECLARE @TABLE_NAME NVARCHAR(32) = 'InventoryParts_Shikakari'
	DECLARE @FUNC_NAME  NVARCHAR(32) = 'GetartsWipStock'

	-----------------------
	--以降は共通
	-----------------------
	--変数宣言
	DECLARE @TargetDateFrom datetime				--対象年月(From)
	DECLARE @TargetDateTo datetime					--対象年月(To)
	DECLARE @InventoryStatus NVARCHAR(3)			--棚卸ステータス
	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)
	

	--処理結果
	DECLARE @ErrorMessage NVARCHAR(4000) = ''
	DECLARE @ErrorNumber INT = 0

	--■■一時表の削除
	/*************************************************************************/
	IF OBJECT_ID(N'tempdb..#Temp_ServiceSalesHeader', N'U') IS NOT NULL
	DROP TABLE #Temp_ServiceSalesHeader;											--サービス伝票ヘッダ
	IF OBJECT_ID(N'tempdb..#Temp_ServiceSalesHeader_Exempt', N'U') IS NOT NULL
	DROP TABLE #Temp_ServiceSalesHeader_Exempt;										--サービス伝票(対象外)
	IF OBJECT_ID(N'tempdb..#Temp_ServiceSales', N'U') IS NOT NULL
	DROP TABLE #Temp_ServiceSales;
	IF OBJECT_ID(N'tempdb..#Temp_DepartmentListUseWarehouse', N'U') IS NOT NULL
	DROP TABLE #Temp_DepartmentListUseWarehouse;									--部門リスト
	
	/*************************************************************************/
	--サービス伝票ヘッダ
	CREATE TABLE #Temp_ServiceSalesHeader (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[RevisionNumber] int NOT NULL					-- リビジョン番号
	,	[DepartmentCode]  NVARCHAR(3)					-- 部門コード
	,	[ServiceOrderStatus] NVARCHAR(3)				-- 伝票ステータス
	,	[FrontEmployeeCode] NVARCHAR(50)				-- フロント担当者コード
	,	[CustomerCode]  NVARCHAR(10)					-- 顧客コード
	,	[CustomerName] NVARCHAR(80)						-- 顧客名
	,	[CarName]	NVARCHAR(50)						-- 車種名
	,	[Vin]	NVARCHAR(20)							-- 車台番号
	,	[ArrivalPlanDate] DATETIME						-- 入庫予定日
	)
	CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader ([SlipNumber], [RevisionNumber])

	--サービス伝票ヘッダ(対象外データ)
	CREATE TABLE #Temp_ServiceSalesHeader_Exempt (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	)
	--CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader_Exempt ON #Temp_ServiceSalesHeader_Exempt ([SlipNumber])
	
	--サービス伝票明細(サービス／部品)
	CREATE TABLE #Temp_ServiceSales (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[RevisionNumber] int NOT NULL					-- リビジョン番号
	,	[ServiceOrderStatus] NVARCHAR(3)				-- 伝票ステータス
	,	[FrontEmployeeCode] NVARCHAR(50)				-- サービス担当者
	,	[CustomerCode] NVARCHAR(10)						-- 顧客コード
	,	[CustomerName] NVARCHAR(80)						-- 顧客名
	,	[CarName] NVARCHAR(50)							-- 車種名
	,	[Vin]  NVARCHAR(20)								-- 車台番号
	,	[ArrivalPlanDate] datetime						-- 入庫予定日
	,	[LineNumber] int NOT NULL						-- 行番号
	,	[ServiceWorkCode] NVARCHAR(5)					-- 主作業コード
	,	[StockStatus] NVARCHAR(3)						-- 在庫状況	
	,	[ServiceType] NVARCHAR(3)						-- サービス種別
	,	[ServiceType1] NVARCHAR(50)						-- サービス種別名
	,	[PartsNumber] NVARCHAR(25)						-- 部品番号
	,	[LineContents1] NVARCHAR(50)					-- 部品名称				--LineContentsのサイズ変更nvarchar(25)→nvarchar(50)
	,	[Quantity] DECIMAL(10, 2)						-- 数量
	,	[OutOrderCost] DECIMAL(10, 0)					-- 外注費
	,	[DepartmentCode] NVARCHAR(3)					-- 部門
	,	[EmployeeCode] NVARCHAR(50)						-- メカニック担当者
	,	[SupplierCode] NVARCHAR(10)						-- 外注先
	,	[LineContents2] NVARCHAR(50)					-- 作業名								--LineContentsのサイズ変更nvarchar(25)→nvarchar(50)
	)
	CREATE UNIQUE INDEX IX_Temp_ServiceSales ON #Temp_ServiceSales ([SlipNumber], [RevisionNumber], [LineNumber])

	--Add 2016/08/13 arc yano #3596
	--部門一覧
	CREATE TABLE #Temp_DepartmentListUseWarehouse (
		[DepartmentCode] NVARCHAR(3) NOT NULL			--部門コード
	,	[WarehouseCode] NVARCHAR(6) NOT NULL			--倉庫コード
	)
	CREATE UNIQUE INDEX IX_Temp_DepartmentListUseWarehouse ON #Temp_DepartmentListUseWarehouse ([DepartmentCode], [WarehouseCode])

	---------------------
	-- データ設定
	---------------------
	--対象年月の範囲設定
	--現在
	DECLARE @NOW DATETIME = GETDATE()

	--当月1日
	DECLARE @THISMONTH DATETIME = CONVERT(DATETIME, CONVERT(NVARCHAR(7), @TargetMonth, 111) + '/01', 111)
	
	--DEBUG
	--PRINT @THISMONTH

	--対象年月(From)の設定
	SET @TargetDateFrom = @TargetMonth

	--対象年月(From)が本日以降の場合、対象年月(To)は本日を設定
	IF(@TargetDateFrom > @NOW)
	SET @TargetDateFrom = @NOW

	--対象年月(To)の設定
	SET @TargetDateTo = DATEADD(m, 1, @TargetMonth)						--対象月の翌月を設定

	--対象年月(From)が本日以降の場合、対象年月(To)は本日を設定
	IF(@TargetDateTo > @NOW)
	SET @TargetDateTo = @NOW


	--棚卸ステータスの取得
	SELECT 
		@InventoryStatus = InventoryStatus
	FROM
		dbo.[InventoryScheduleParts]
	WHERE
		--DepartmentCode = @DepartmentCode
		WarehouseCode = @WarehouseCode									--Mod 2016/08/13 arc yano #3596
		AND InventoryMonth = @TargetMonth

	--トランザクション開始 
	BEGIN TRANSACTION
	BEGIN TRY

		--Add 2016/08/13 arc yano #3596
		----------------------------------------------
		-- 倉庫コードより部門リストを取得する
		----------------------------------------------
		--部門・倉庫の組合せリストより、倉庫を使用している全て部門を取得する
		INSERT INTO #Temp_DepartmentListUseWarehouse
		SELECT
			 dw.DepartmentCode		--部門コード
			,dw.WarehouseCode		--倉庫コード
		FROM
			dbo.DepartmentWarehouse dw
		WHERE
			dw.WarehouseCode = @WarehouseCode			--倉庫コード

		--インデックス再生成	
		DROP INDEX IX_Temp_DepartmentListUseWarehouse ON #Temp_DepartmentListUseWarehouse
		CREATE UNIQUE INDEX IX_Temp_DepartmentListUseWarehouse ON #Temp_DepartmentListUseWarehouse ([DepartmentCode], [WarehouseCode])


		--対象月≠棚卸終了
		IF (ISNULL(@InventoryStatus, '001') <> '002')
		BEGIN
			
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
			
			/*
			--インデックス再生成	
			DROP INDEX IX_Temp_ServiceSalesHeader_Exempt ON #Temp_ServiceSalesHeader_Exempt
			CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader_Exempt ON #Temp_ServiceSalesHeader_Exempt ([SlipNumber])
			*/

			-------------------------------------------
			--サービス伝票ヘッダ絞り込み
			-------------------------------------------
			SET @PARAM = '@DepartmentCode NVARCHAR(3), @TargetDateTo DATETIME,@ArrivalPlanDateFrom NVARCHAR(10), @ArrivalPlanDateTo NVARCHAR(10), @SlipNumber NVARCHAR(50), @Vin NVARCHAR(20), @CustomerName NVARCHAR(80)'
					
			SET @SQL = ''
			SET @SQL = @SQL +'INSERT INTO #Temp_ServiceSalesHeader' + @CRLF
			SET @SQL = @SQL +'SELECT' + @CRLF
			SET @SQL = @SQL +'     sh.[SlipNumber]' + @CRLF														--伝票番号
			SET @SQL = @SQL +',    sh.[RevisionNumber]' + @CRLF													--リビジョン番号
			SET @SQL = @SQL +',    sh.[DepartmentCode]' + @CRLF													--部門コード
			SET @SQL = @SQL +',    sh.[ServiceOrderStatus]' + @CRLF												--伝票ステータス
			SET @SQL = @SQL +',    sh.[FrontEmployeeCode]' + @CRLF												--サービス担当者
			SET @SQL = @SQL +',    sh.[CustomerCode]' + @CRLF													--顧客コード
			SET @SQL = @SQL +',    c.[CustomerName]' + @CRLF													--顧客名
			SET @SQL = @SQL +',    sh.[CarName]' + @CRLF														--車種名
			SET @SQL = @SQL +',    sh.[Vin]' + @CRLF															--車台番号
			SET @SQL = @SQL +',    sh.[ArrivalPlanDate]' + @CRLF												--入庫予定日
			SET @SQL = @SQL +'FROM' + @CRLF
			SET @SQL = @SQL +'	dbo.ServiceSalesHeader sh' + @CRLF
			SET @SQL = @SQL +'	INNER JOIN dbo.[Customer] c' + @CRLF	
			SET @SQL = @SQL +'		ON sh.[CustomerCode] = c.[CustomerCode]' + @CRLF		
			SET @SQL = @SQL +'WHERE' + @CRLF
			--SET @SQL = @SQL +'		sh.[DepartmentCode] = @DepartmentCode' + @CRLF
			SET @SQL = @SQL +'		sh.[DelFlag] = ''0''' + @CRLF
			SET @SQL = @SQL +'	AND sh.[WorkingStartDate] < @TargetDateTo' + @CRLF								--対象終了日前に作業開始している伝票
			SET @SQL = @SQL +'	AND	NOT EXISTS (' + @CRLF
			SET @SQL = @SQL +'		SELECT ''X''' + @CRLF
			SET @SQL = @SQL +'		FROM' + @CRLF
			SET @SQL = @SQL +'			#Temp_ServiceSalesHeader_Exempt she' + @CRLF
			SET @SQL = @SQL +'		WHERE' + @CRLF
			SET @SQL = @SQL +'			she.[SlipNumber] = sh.[SlipNumber]' + @CRLF
			SET @SQL = @SQL +'	)' + @CRLF
			
			--PRINT @SQL

			--入庫予定日
			IF ((@ArrivalPlanDateFrom is not null) AND (@ArrivalPlanDateFrom <> '') AND ISDATE(@ArrivalPlanDateFrom) = 1)
				IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
				BEGIN
					SET @SQL = @SQL +' AND sh.[ArrivalPlanDate] >= @ArrivalPlanDateFrom AND sh.[ArrivalPlanDate] < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF
				END
				ELSE
				BEGIN
					SET @SQL = @SQL +' AND  sh.[ArrivalPlanDate] = @ArrivalPlanDateFrom' + @CRLF 
				END
			ELSE
				IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
				BEGIN
					SET @SQL = @SQL +' AND sh.[ArrivalPlanDate] < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF 
				END

			--伝票番号
			IF ((@SlipNumber is not null) AND (@SlipNumber <> ''))
			BEGIN
				SET @SQL = @SQL +' AND sh.[SlipNumber] LIKE ''%' + @SlipNumber + '%''' + @CRLF
			END

			--車台番号
			IF ((@Vin is not null) AND (@Vin <> ''))
			BEGIN
				SET @SQL = @SQL +' AND sh.[Vin] LIKE ''%' + @Vin + '%''' + @CRLF
			END

			--顧客名
			IF ((@CustomerName is not null) AND (@CustomerName <> ''))
			BEGIN
				SET @SQL = @SQL +' AND c.CustomerName LIKE ''%' + @CustomerName + '%''' + @CRLF
			END

			--Mod 2016/08/13 arc yano #3596　部門コードがある場合は部門コードで抽出する
			--部門コード
			IF ((@DepartmentCode is not null) AND (@DepartmentCode <> ''))
			BEGIN
				SET @SQL = @SQL +' AND sh.[DepartmentCode] = @DepartmentCode' + @CRLF
			END
			ELSE --無い場合は倉庫コードから取得した部門一覧で抽出する
			BEGIN
				SET @SQL = @SQL +' AND	EXISTS (' + @CRLF
				SET @SQL = @SQL +'		SELECT ''X''' + @CRLF
				SET @SQL = @SQL +'		FROM' + @CRLF
				SET @SQL = @SQL +'			#Temp_DepartmentListUseWarehouse dw' + @CRLF
				SET @SQL = @SQL +'		WHERE' + @CRLF
				SET @SQL = @SQL +'			sh.[DepartmentCode] = dw.[DepartmentCode]' + @CRLF
				SET @SQL = @SQL +'	)' + @CRLF
			END

			--DEBUG
			--PRINT @SQL
			
			EXECUTE sp_executeSQL @SQL, @PARAM, @DepartmentCode, @TargetDateTo , @ArrivalPlanDateFrom, @ArrivalPlanDateTo, @SlipNumber, @Vin, @CustomerName

			--DEBUG
			--SELECT * FROM #Temp_ServiceSalesHeader

			--インデックス再生成	
			DROP INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader
			CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader ([SlipNumber], [RevisionNumber])
			
			---------------------------------------------
			--仕掛データ取得(外注・部品まとめて取得)
			---------------------------------------------
			SET @PARAM = '@ServiceType NVARCHAR(3), @PartsNumber NVARCHAR(25), @PartsNameJp NVARCHAR(50)'

				SET @SQL = ''
				SET @SQL = @SQL +'INSERT INTO #Temp_ServiceSales' + @CRLF
				SET @SQL = @SQL +'SELECT' + @CRLF
				SET @SQL = @SQL +'		sh.[SlipNumber]' + @CRLF														--伝票番号
				SET @SQL = @SQL +',		sh.[RevisionNumber]' + @CRLF													--リビジョン番号
				SET @SQL = @SQL +',		sh.[ServiceOrderStatus]' + @CRLF												--伝票ステータス
				SET @SQL = @SQL +',		sh.[FrontEmployeeCode]' + @CRLF													--サービス担当者
				SET @SQL = @SQL +',		sh.[CustomerCode]' + @CRLF														--顧客コード
				SET @SQL = @SQL +',		sh.[CustomerName]' + @CRLF														--顧客コード
				SET @SQL = @SQL +',		sh.[CarName]' + @CRLF															--車種名
				SET @SQL = @SQL +',		sh.[Vin]' + @CRLF																--車台番号
				SET @SQL = @SQL +',		sh.[ArrivalPlanDate]' + @CRLF													--入庫予定日
				SET @SQL = @SQL +',		sl.[LineNumber]' + @CRLF														--行番号
				SET @SQL = @SQL +',		sl.[ServiceWorkCode]' + @CRLF													--主作業コード
				SET @SQL = @SQL +',		CASE WHEN sl.[ServiceType] = ''003'' THEN sl.[StockStatus]' + @CRLF				--在庫状況	--Mod 2016/07/14 arc yano				
				SET @SQL = @SQL +'		ELSE '''' END AS StockStatus' + @CRLF
				SET @SQL = @SQL +',		sl.[ServiceType]' + @CRLF														--種別(002:サービスメニュー 003:部品)
				SET @SQL = @SQL +',		CASE WHEN sl.ServiceType = ''003'' THEN ''部品''' + @CRLF						--種別名					
				SET @SQL = @SQL +'		ELSE ''外注'' END AS ServiceType1' + @CRLF
				SET @SQL = @SQL +',		CASE WHEN sl.ServiceType = ''003'' THEN sl.[PartsNumber]' + @CRLF				--部品番号					
				SET @SQL = @SQL +'		ELSE '''' END AS PartsNumber' + @CRLF
				SET @SQL = @SQL +',		CASE WHEN sl.ServiceType = ''003'' THEN sl.[LineContents]' + @CRLF				--部品名称					
				SET @SQL = @SQL +'		ELSE '''' END AS LineContents1' + @CRLF
				SET @SQL = @SQL +',		ISNULL(sl.[Quantity], 0) AS Quantity' + @CRLF							--数量
				SET @SQL = @SQL +',		CASE WHEN sl.ServiceType = ''003'' THEN 0' + @CRLF											
				SET @SQL = @SQL +'		ELSE ISNULL(sl.[Cost], 0) END AS OutOrderCost' + @CRLF					--外注費
				SET @SQL = @SQL +',		sh.[DepartmentCode]' + @CRLF											--部門コード
				SET @SQL = @SQL +',		RTRIM(ISNULL(sl.[EmployeeCode], '''')) AS EmployeeCode' + @CRLF			--メカニック担当者
				SET @SQL = @SQL +',		RTRIM(ISNULL(sl.[SupplierCode], '''')) AS SupplierCode' + @CRLF			--外注先
				SET @SQL = @SQL +',		CASE WHEN sl.ServiceType = ''003'' THEN ''''' + @CRLF					--作業名				
				SET @SQL = @SQL +'		ELSE sl.[LineContents] END AS LineContents2' + @CRLF
				SET @SQL = @SQL +'FROM' + @CRLF
				SET @SQL = @SQL +'	#Temp_ServiceSalesHeader sh' + @CRLF
				SET @SQL = @SQL +'	INNER JOIN dbo.[ServiceSalesLine] sl' + @CRLF
				SET @SQL = @SQL +'		ON sl.[SlipNumber] = sh.[SlipNumber] ' + @CRLF
				SET @SQL = @SQL +'		AND sl.[RevisionNumber] = sh.[RevisionNumber]' + @CRLF
				SET @SQL = @SQL +'WHERE' + @CRLF
				SET @SQL = @SQL +'	ISNULL(sl.[DelFlag], ''0'') = ''0''' + @CRLF
				SET @SQL = @SQL +'	AND (' + @CRLF
				SET @SQL = @SQL +'			(' + @CRLF
				SET @SQL = @SQL +'				sl.[ServiceType] = ''002''' + @CRLF
				SET @SQL = @SQL +'				AND sl.[SupplierCode] is not null' + @CRLF
				SET @SQL = @SQL +'			)' + @CRLF
				SET @SQL = @SQL +'			OR' + @CRLF
				SET @SQL = @SQL +'			(' + @CRLF
				SET @SQL = @SQL +'				sl.[ServiceType] = ''003''' + @CRLF
				SET @SQL = @SQL +'				AND ISNULL(sl.[WorkType], '''') <> ''015''' + @CRLF
				
				SET @SQL = @SQL +'			)' + @CRLF
				SET @SQL = @SQL +'		)' + @CRLF
						--明細種別
				IF ((@ServiceType is not null) AND (@ServiceType <> ''))
				BEGIN
					SET @SQL = @SQL +' AND sl.[ServiceType] = @ServiceType' + @CRLF
				END

				--部品番号
				IF ((@PartsNumber is not null) AND (@PartsNumber <> ''))
				BEGIN
					SET @SQL = @SQL +' AND sl.[PartsNumber] LIKE ''%' + @PartsNumber + '%''' + @CRLF
				END

				--部品名
				IF ((@PartsNameJp is not null) AND (@PartsNameJp <> ''))
				BEGIN
					SET @SQL = @SQL +' AND sl.[LineContents] LIKE ''%' + @PartsNameJp + '%''' + @CRLF
				END
				
				--DEBUG
				--PRINT @SQL

				EXECUTE sp_executeSQL @SQL, @PARAM, @ServiceType, @PartsNumber, @PartsNameJp
				
				--DEBUG
				--SELECT * FROM #Temp_ServiceSales


				--インデックス再生成	
				DROP INDEX IX_Temp_ServiceSales ON #Temp_ServiceSales
				CREATE UNIQUE INDEX IX_Temp_ServiceSales ON #Temp_ServiceSales ([SlipNumber], [RevisionNumber], [LineNumber])
			
			-----------------------
			--処理内容
			-----------------------
			IF @ActionFlag = 0	--動作指定が「画面表示」
			BEGIN
				/***************************************************/
				/*動作指定=「表示」の場合はデータ取得を行う		   */
				/***************************************************/
				SET @PARAM = '@TargetMonth datetime'
					
				SET @SQL = ''
				SET @SQL = @SQL +'SELECT' + @CRLF
				SET @SQL = @SQL +'    @TargetMonth AS InventoryMonth' + @CRLF
				SET @SQL = @SQL +'  , SS.DepartmentCode AS DepartmentCode' + @CRLF
				SET @SQL = @SQL +'  , SS.ArrivalPlanDate' + @CRLF
				SET @SQL = @SQL +'	, SS.SlipNumber' + @CRLF
				SET @SQL = @SQL +'	, SS.LineNumber' + @CRLF
				SET @SQL = @SQL +'	, SS.ServiceOrderStatus' + @CRLF
				SET @SQL = @SQL +'	, C1.Name AS ServiceOrderStatusName' + @CRLF
				SET @SQL = @SQL +'	, SS.ServiceWorkCode AS ServiceWorkCode' + @CRLF
				SET @SQL = @SQL +'	, W.Name AS ServiceWorksName' + @CRLF
				SET @SQL = @SQL +'	, E1.EmployeeName AS FrontEmployeeName' + @CRLF
				SET @SQL = @SQL +'	, E2.EmployeeName AS MekaEmployeeName' + @CRLF
				SET @SQL = @SQL +'	, SS.CustomerCode' + @CRLF
				SET @SQL = @SQL +'	, SS.CustomerName' + @CRLF
				SET @SQL = @SQL +'	, SS.CarName' + @CRLF
				SET @SQL = @SQL +'	, SS.Vin' + @CRLF
				SET @SQL = @SQL +'	, SS.ServiceType AS ServiceType' + @CRLF
				SET @SQL = @SQL +'	, SS.ServiceType1 AS ServiceTypeName' + @CRLF
				SET @SQL = @SQL +'	, C2.Name AS StockTypeName' + @CRLF
				SET @SQL = @SQL +'	, NULL	AS PurchaseOrderDate' + @CRLF
				SET @SQL = @SQL +'	, NULL AS PartsArrivalPlanDate' + @CRLF
				SET @SQL = @SQL +'	, NULL AS PurchaseDate' + @CRLF
				SET @SQL = @SQL +'	, SS.PartsNumber AS PartsNumber' + @CRLF
				SET @SQL = @SQL +'	, SS.LineContents1 AS LineContents1' + @CRLF
				SET @SQL = @SQL +'	, COALESCE(pa.Price, P.SoPrice ,P.Cost, 0) AS Price' + @CRLF
				SET @SQL = @SQL +'	, SS.Quantity AS Quantity' + @CRLF
				SET @SQL = @SQL +'	, COALESCE(pa.Price, P.SoPrice ,P.Cost, 0) * SS.Quantity AS Amount' + @CRLF
				SET @SQL = @SQL +'	, S.SupplierName' + @CRLF
				SET @SQL = @SQL +'	, SS.LineContents2 AS LineContents2' + @CRLF
				SET @SQL = @SQL +'	, SS.OutOrderCost AS Cost' + @CRLF
				SET @SQL = @SQL +'FROM' + @CRLF
				SET @SQL = @SQL +'	#Temp_ServiceSales SS' + @CRLF
				SET @SQL = @SQL +'INNER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.ServiceWork AS W' + @CRLF
				SET @SQL = @SQL +'	ON SS.ServiceWorkCode = W.ServiceWorkCode' + @CRLF
				SET @SQL = @SQL +'INNER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.c_ServiceOrderStatus AS C1' + @CRLF
				SET @SQL = @SQL +'	ON  SS.ServiceOrderStatus = C1.Code' + @CRLF
				SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.c_StockStatus AS C2' + @CRLF
				SET @SQL = @SQL +'	ON  SS.StockStatus = C2.Code' + @CRLF
				SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF			--Mod 2015/10/13 arc yano
				SET @SQL = @SQL +'	dbo.Parts AS P' + @CRLF
				SET @SQL = @SQL +'	ON  SS.PartsNumber = P.PartsNumber' + @CRLF
				SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.Employee AS E1 ' + @CRLF
				SET @SQL = @SQL +'	ON  SS.FrontEmployeeCode = E1.EmployeeCode' + @CRLF
				SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.Employee AS E2 ' + @CRLF
				SET @SQL = @SQL +'	ON  SS.EmployeeCode = E2.EmployeeCode' + @CRLF
				SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.Supplier AS S ' + @CRLF
				SET @SQL = @SQL +'	ON  SS.SupplierCode = S.SupplierCode' + @CRLF
				SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.PartsAverageCost AS pa' + @CRLF
				SET @SQL = @SQL +'	ON pa.PartsNumber = SS.PartsNumber' + @CRLF
				SET @SQL = @SQL +'	AND pa.CloseMonth = @TargetMonth' + @CRLF
				SET @SQL = @SQL +'WHERE' + @CRLF
				SET @SQL = @SQL +'	ISNULL(p.Delflag, ''0'') <> ''1''' + @CRLF						--Add 2015/10/13 arc yano
				SET @SQL = @SQL +'	AND ISNULL(p.NonInventoryFlag, ''0'') <> ''1''' + @CRLF			--Add 2015/10/13 arc yano
				SET @SQL = @SQL +'	AND (' + @CRLF													--Add 2016/07/14 arc yano
				SET @SQL = @SQL +'			P.PartsNumber is NULL' + @CRLF							--Add 2016/07/14 arc yano
				--SET @SQL = @SQL +'			OR C2.SelectedGenuineType = P.GenuineType' + @CRLF		--Add 2016/07/14 arc yano	--Mod 2017/10/19 arc yano #3803
				SET @SQL = @SQL +'		)' + @CRLF	
				--並び替え
				SET @SQL = @SQL +'ORDER BY' + @CRLF
				SET @SQL = @SQL +'	 SS.ServiceOrderStatus' + @CRLF
				SET @SQL = @SQL +'	,SS.SlipNumber' + @CRLF
				SET @SQL = @SQL +'	,SS.LineNumber' + @CRLF



			--DEBUG
			--PRINT @SQL
			
				EXECUTE sp_executeSQL @SQL, @PARAM, @TargetMonth
			END
			ELSE
			BEGIN
				--一度対象年月のデータを削除
				
				--Mod 2016/08/13 arc yano #3596　部門コードがある場合は部門コードで抽出する
				--部門コード
				IF ((@DepartmentCode is not null) AND (@DepartmentCode <> ''))
				BEGIN
					DELETE 
						ips 
					FROM 
						dbo.InventoryParts_Shikakari ips
					WHERE
			 			ips.InventoryMonth = @TargetMonth
					AND 
						DepartmentCode = @DepartmentCode
				END
				ELSE
				BEGIN
					DELETE 
						ips 
					FROM 
						dbo.InventoryParts_Shikakari ips
					WHERE
			 			ips.InventoryMonth = @TargetMonth
						AND 
						EXISTS
						(
							SELECT 'X' FROM #Temp_DepartmentListUseWarehouse dw WHERE ips.DepartmentCode = dw.DepartmentCode 
						)
				END

				INSERT INTO dbo.InventoryParts_Shikakari
				SELECT
					  @TargetMonth													--対象年月
					, SS.DepartmentCode												--部門コード          
					, SS.ArrivalPlanDate											--入庫日
					, SS.SlipNumber													--伝票番号
					, SS.LineNumber													--行番号
					, SS.ServiceOrderStatus											--伝票ステータス
					, C1.Name AS ServiceOrderStatusName								--伝票ステータス名
					, SS.ServiceWorkCode											--主作業コード
					, W.Name AS ServiceWorksName									--主作業名
					, E1.EmployeeName AS FrontEmployeeName							--フロント担当者名
					, E2.EmployeeName AS MekaEmployeeName							--メカニック担当者名
					, SS.CustomerCode												--顧客コード
					, SS.CustomerName												--顧客名
					, SS.CarName													--車種名
					, SS.Vin														--車台番号
					, SS.ServiceType												--サービス種別
					, SS.ServiceType1 AS ServiceTypeName							--サービス種別名
					, C2.Name AS StockTypeName										--在庫状況
					, NULL AS PurchaseOrderDate
					, NULL AS PartsArrivalPlanDate
					, NULL AS PurchaseDate
					, SS.PartsNumber												--部品番号
					, SS.LineContents1 AS LineContents1								--部品名
					, 0 AS Price													--単価 ※スナップショット保存時は０
					, SS.Quantity AS Quantity										--数量
					, 0 AS TotalAmount												--金額 ※スナップショット保存時は０
					, S.SupplierName												--外注先
					, SS.LineContents2 AS LineContents2								--サービス名				
					, SS.OutOrderCost												--サービス料
				FROM
					#Temp_ServiceSales AS SS 
					INNER JOIN dbo.ServiceWork AS W 
						ON SS.ServiceWorkCode = W.ServiceWorkCode 
					INNER JOIN dbo.c_ServiceOrderStatus AS C1 
						ON SS.ServiceOrderStatus = C1.Code 
					LEFT OUTER JOIN dbo.c_StockStatus AS C2 
						ON SS.StockStatus = C2.Code 
					LEFT OUTER JOIN dbo.Employee AS E1 
						ON SS.FrontEmployeeCode = E1.EmployeeCode 
					LEFT OUTER JOIN dbo.Employee AS E2 
						ON SS.EmployeeCode = E2.EmployeeCode
					LEFT OUTER JOIN dbo.Supplier AS S 
						ON SS.SupplierCode = S.SupplierCode
					LEFT OUTER JOIN dbo.Parts AS P								--ADD 2016/07/14 arc yano 
						ON SS.PartsNumber = P.PartsNumber
				WHERE															--ADD 2016/07/14 arc yano 
					ISNULL(P.Delflag, '0') <> '1' AND
					ISNULL(P.NonInventoryFlag, '0') <> '1' AND
				    (
						P.PartsNumber IS NULL --OR								--Del 2017/10/19 arc yano #3803
						--C2.SelectedGenuineType = P.GenuineType
					)
			END
		END
		ELSE
		BEGIN
			/***************************************************/
			/*棚卸が終了した対象月の仕掛在庫の表示		   */
			/***************************************************/
			SET @PARAM = '@TargetMonth datetime,@DepartmentCode NVARCHAR(3), @ServiceType NVARCHAR(3), @ArrivalPlanDateFrom NVARCHAR(10), @ArrivalPlanDateTo NVARCHAR(10), @SlipNumber NVARCHAR(50), @PartsNumber NVARCHAR(25), @PartsNameJp NVARCHAR(50), @Vin NVARCHAR(20), @CustomerName NVARCHAR(80)'
					
			SET @SQL = ''
			SET @SQL = @SQL +'SELECT' + @CRLF
			SET @SQL = @SQL +'    ips.InventoryMonth AS InventoryMonth' + @CRLF
			SET @SQL = @SQL +'  , ips.DepartmentCode AS DepartmentCode' + @CRLF
			SET @SQL = @SQL +'  , ips.ArrivalPlanDate' + @CRLF
			SET @SQL = @SQL +'	, ips.SlipNumber' + @CRLF
			SET @SQL = @SQL +'	, ips.LineNumber' + @CRLF
			SET @SQL = @SQL +'	, ips.ServiceOrderStatus' + @CRLF
			SET @SQL = @SQL +'	, ips.ServiceOrderStatusName' + @CRLF
			SET @SQL = @SQL +'	, ips.ServiceWorkCode' + @CRLF
			SET @SQL = @SQL +'	, ips.ServiceWorksName' + @CRLF
			SET @SQL = @SQL +'	, ips.FrontEmployeeName' + @CRLF
			SET @SQL = @SQL +'	, ips.MekaEmployeeName' + @CRLF
			SET @SQL = @SQL +'	, ips.CustomerCode' + @CRLF
			SET @SQL = @SQL +'	, ips.CustomerName' + @CRLF
			SET @SQL = @SQL +'	, ips.CarName' + @CRLF
			SET @SQL = @SQL +'	, ips.Vin' + @CRLF
			SET @SQL = @SQL +'	, ips.ServiceType' + @CRLF
			SET @SQL = @SQL +'	, ips.ServiceTypeName' + @CRLF
			SET @SQL = @SQL +'	, ips.StockTypeName'  + @CRLF
			SET @SQL = @SQL +'	, ips.PurchaseOrderDate' + @CRLF
			SET @SQL = @SQL +'	, ips.PartsArravalPlanDate as PartsArrivalPlanDate' + @CRLF
			SET @SQL = @SQL +'	, ips.PurchaseDate' + @CRLF
			SET @SQL = @SQL +'	, ips.PartsNumber' + @CRLF
			SET @SQL = @SQL +'	, ips.LineContents1' + @CRLF
			SET @SQL = @SQL +'	, COALESCE(pa.Price, p.SoPrice ,p.Cost, 0) AS Price' + @CRLF
			SET @SQL = @SQL +'	, ips.Quantity' + @CRLF
			SET @SQL = @SQL +'	, COALESCE(pa.Price, p.SoPrice ,p.Cost, 0) * ips.Quantity AS Amount' + @CRLF
			SET @SQL = @SQL +'	, ips.SupplierName' + @CRLF
			SET @SQL = @SQL +'	, ips.LineContents2' + @CRLF
			SET @SQL = @SQL +'	, ips.Cost AS Cost' + @CRLF
			SET @SQL = @SQL +'FROM' + @CRLF
			SET @SQL = @SQL +'	dbo.InventoryParts_Shikakari ips' + @CRLF
			SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF
			SET @SQL = @SQL +'	dbo.PartsAverageCost AS pa' + @CRLF
			SET @SQL = @SQL +'	ON pa.PartsNumber = ips.PartsNumber' + @CRLF
			SET @SQL = @SQL +'  AND  pa.CloseMonth = @TargetMonth' + @CRLF
			SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF				--Mod 2015/10/13 arc yano
			SET @SQL = @SQL +'	dbo.Parts AS p' + @CRLF
			SET @SQL = @SQL +'	ON p.PartsNumber = ips.PartsNumber' + @CRLF
			SET @SQL = @SQL +'WHERE' + @CRLF
			SET @SQL = @SQL +'	ips.InventoryMonth = @TargetMonth' + @CRLF
			SET @SQL = @SQL +'	AND ips.DepartmentCode = @DepartmentCode' + @CRLF
			SET @SQL = @SQL +'	AND ISNULL(p.DelFlag, ''0'') <> ''1''' + @CRLF	--Add 2015/10/13 arc yano
			SET @SQL = @SQL +'	AND ISNULL(p.NonInventoryFlag, ''0'') <> ''1''' + @CRLF	--Add 2015/10/13 arc yano

			--明細種別
			IF ((@ServiceType is not null) AND (@ServiceType <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ips.ServiceType = @ServiceType' + @CRLF
			END

			--入庫予定日
			IF ((@ArrivalPlanDateFrom is not null) AND (@ArrivalPlanDateFrom <> '') AND ISDATE(@ArrivalPlanDateFrom) = 1)
				IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
				BEGIN
					SET @SQL = @SQL +' AND ips.ArrivalPlanDate >= @ArrivalPlanDateFrom AND ips.ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF
				END
				ELSE
				BEGIN
					SET @SQL = @SQL +' AND  ips.ArrivalPlanDate = @ArrivalPlanDateFrom' + @CRLF 
				END
			ELSE
				IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
				BEGIN
					SET @SQL = @SQL +' AND ips.ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF 
				END

			--伝票番号
			IF ((@SlipNumber is not null) AND (@SlipNumber <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ips.SlipNumber LIKE ''%' + @SlipNumber + '%''' + @CRLF
			END

			--部品番号
			IF ((@PartsNumber is not null) AND (@PartsNumber <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ips.PartsNumber LIKE ''%' + @PartsNumber + '%''' + @CRLF
			END

			--部品名
			IF ((@PartsNameJp is not null) AND (@PartsNameJp <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ips.LineContents1 LIKE ''%' + @PartsNameJp + '%''' + @CRLF
			END

			--車台番号
			IF ((@Vin is not null) AND (@Vin <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ips.Vin LIKE ''%' + @Vin + '%''' + @CRLF
			END

			--顧客名
			IF ((@CustomerName is not null) AND (@CustomerName <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ips.CustomerName LIKE ''%' + @CustomerName + '%''' + @CRLF
			END

			--並び替え
			SET @SQL = @SQL +'ORDER BY' + @CRLF
			SET @SQL = @SQL +'	 ips.ServiceOrderStatus' + @CRLF
			SET @SQL = @SQL +'	,ips.SlipNumber' + @CRLF
			SET @SQL = @SQL +'	,ips.LineNumber' + @CRLF

			--DEBUG
			--PRINT @SQL
			
			EXECUTE sp_executeSQL @SQL, @PARAM, @TargetMonth,@DepartmentCode , @ServiceType, @ArrivalPlanDateFrom, @ArrivalPlanDateTo, @SlipNumber, @PartsNumber, @PartsNameJp, @Vin, @CustomerName
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


