USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[Insert_AccountsReceivableCar]    Script Date: 2019/06/04 15:18:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- ==============================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2016/08/26 arc nakayama  #3630_【製造】車両売掛金対応 
-- Update date
-- Mod 2019/05/22 yano #3954 【サービス売掛金】当月納車の売掛金が表示されない 類似対応
-- 2018/01/29 arc yano #3717 サービス売掛金_伝票が存在しない月の入金実績の反映 類似処理
-- 2016/12/22 arc yano #3682 車両売掛金　システムエラー 入金実績の集計方法を変更＆保守性の向上のため、以前にコメントアウトされた処理を削除

-- ==============================================================================================================================================
CREATE PROCEDURE [dbo].[Insert_AccountsReceivableCar] 

	@TargetMonth datetime,						--指定月
	@ActionFlag int = 0,						--動作指定(0:画面表示, 1:スナップショット保存)
	@SalesDateFrom NVARCHAR(10) = NULL,			--納車日(From)
	@SalesDateTo NVARCHAR(10) = NULL,			--納車日(To)
	@SlipNumber NVARCHAR(50) = NULL,			--伝票番号
	@DepartmentCode NVARCHAR(3) = NULL,			--部門コード
	@CustomerCode NVARCHAR(10) = NULL,			--顧客コード
	@Zerovisible NVARCHAR(1) = NULL				--ゼロ表示フラグ(0:非表示 1:表示)
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

	--処理結果
	DECLARE @ErrorMessage NVARCHAR(4000) = ''
	DECLARE @ErrorNumber INT = 0


	--■一時表の削除
	/*************************************************************************/

	IF OBJECT_ID(N'tempdb..#Temp_CodeList', N'U') IS NOT NULL
	DROP TABLE #Temp_CodeList;														--コードリスト(入金予定／実績に存在する全ての伝票番号、請求先コード等の組合せ)
	IF OBJECT_ID(N'tempdb..#Temp_CodeList', N'U') IS NOT NULL
	DROP TABLE #Temp_CodeNameList;													--コードネームリスト
	IF OBJECT_ID(N'tempdb..#Temp_CarriedBalance', N'U') IS NOT NULL
	DROP TABLE #Temp_CarriedBalance													--前月繰越リスト
	IF OBJECT_ID(N'tempdb..#Temp_ReceiptPlan', N'U') IS NOT NULL
	DROP TABLE #Temp_ReceiptPlan;													--入金予定リスト
	IF OBJECT_ID(N'tempdb..#Temp_SalesOrderReceiptPlan', N'U') IS NOT NULL
	DROP TABLE #Temp_SalesOrderReceiptPlan;											--入金予定リスト(車両)
	IF OBJECT_ID(N'tempdb..#Temp_Journal', N'U') IS NOT NULL
	DROP TABLE #Temp_Journal;														--入金実績リスト
	--IF OBJECT_ID(N'tempdb..#Temp_SalesOrderJournal', N'U') IS NOT NULL
	--DROP TABLE #Temp_SalesOrderJournal;											--入金実績リスト(車両)		--Del 2016/12/22 arc yano #3682
	IF OBJECT_ID(N'tempdb..#Temp_AccountsReceivableCar', N'U') IS NOT NULL
	DROP TABLE #Temp_AccountsReceivableCar;											--売掛金データリスト
	IF OBJECT_ID(N'tempdb..#Temp_CarSalesHeader', N'U') IS NOT NULL
	DROP TABLE #Temp_CarSalesHeader;												--車両伝票ヘッダ
	IF OBJECT_ID(N'tempdb..#Temp_SlipInfo', N'U') IS NOT NULL
	DROP TABLE #Temp_SlipInfo;														--伝票情報			--Add 2016/12/22 arc yano #3682
	IF OBJECT_ID(N'tempdb..#Temp_SlipList', N'U') IS NOT NULL
	DROP TABLE #Temp_SlipList;														--伝票情報			--Add 2018/01/27 arc yano #3717
	
	--■■一時表の宣言
	/*************************************************************************/

	--コードリスト
	CREATE TABLE #Temp_CodeList (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[SalesDate]	datetime							-- 納車日
	,	[DepartmentCode] NVARCHAR(3)					-- 部門コード
	,	[CustomerCode] NVARCHAR(10)						-- 顧客コード
	)

	--前月繰越リスト
	CREATE TABLE #Temp_CarriedBalance (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[SalesDate]	datetime							-- 納車日
	,	[DepartmentCode] NVARCHAR(3)					-- 部門コード
	,	[CustomerCode] NVARCHAR(10)						-- 顧客コード
	,	[CarriedBalance] DECIMAL(10, 0)					-- 前月繰越金
	)
	CREATE UNIQUE INDEX IX_Temp_CarriedBalance ON #Temp_CarriedBalance ([SlipNumber])
			
	--入金予定リスト
	CREATE TABLE #Temp_ReceiptPlan (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[Amount] DECIMAL(10, 0)							-- 入金予定額
	,	[TradeInAmount] DECIMAL(10, 0)					-- 下取仕入
	,	[RemainDebtAmount] DECIMAL(10, 0)				-- 残債金額
	,	[PresentMonth] DECIMAL(10, 0)					-- 当月発生(入金予定額＋下取仕入金額＋残債金額)
	)
	CREATE UNIQUE INDEX IX_Temp_ReceiptPlan ON #Temp_ReceiptPlan ([SlipNumber])

	--入金予定リスト(車両)
	CREATE TABLE #Temp_SalesOrderReceiptPlan (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[SalesDate]	datetime							-- 納車日
	,	[DepartmentCode] NVARCHAR(3)					-- 部門コード
	,	[CustomerCode] NVARCHAR(10)						-- 顧客コード
	,	[Amount] DECIMAL(10, 0)							-- 入金予定額
	,	[TradeInAmount] DECIMAL(10, 0)					-- 下取仕入
	,	[RemainDebtAmount] DECIMAL(10, 0)				-- 残債
	,	[PresentMonth] DECIMAL(10, 0)					-- 当月発生(入金予定額＋諸費用)
	)
	CREATE UNIQUE INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan ([SlipNumber])

	--入金実績リスト
	CREATE TABLE #Temp_Journal (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[TradeInAmount] DECIMAL(10, 0)					-- 下取仕入
	,	[RemainDebtAmount] DECIMAL(10, 0)				-- 残債
	,	[Amount] DECIMAL(10, 0)							-- 入金額
	,   [TotalAmount] DECIMAL(10, 0)					-- 入金額合計(下取仕入 + 残債 + 入金額)
	)
	CREATE UNIQUE INDEX IX_Temp_Journal ON #Temp_Journal ([SlipNumber])

	--Del 2016/12/22 arc yano #3682 
	/*
	--入金実績リスト(車両)
	CREATE TABLE #Temp_SalesOrderJournal (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[SalesDate]	datetime							-- 納車日
	,	[DepartmentCode] NVARCHAR(3)					-- 部門コード
	,	[CustomerCode] NVARCHAR(10)						-- 顧客コード
	,	[TradeInAmount] DECIMAL(10, 0)					-- 下取仕入
	,	[RemainDebtAmount] DECIMAL(10, 0)				-- 残債
	,	[Amount] DECIMAL(10, 0)							-- その他の入金額
	,   [TotalAmount] DECIMAL(10, 0)					-- 入金額合計(下取仕入 + 残債 + 入金額)
	)
	CREATE UNIQUE INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal ([SlipNumber])
	*/

	--売掛金データ
	CREATE TABLE #Temp_AccountsReceivableCar(
		CloseMonth datetime,				--締月
		SlipNumber nvarchar(50),			--伝票番号
		DepartmentCode nvarchar(3),			--部門コード
		DepartmentName nvarchar(20),		--部門名
		CustomerCode nvarchar(10),			--顧客コード
		CustomerName nvarchar(80),			--顧客名
		SalesDate datetime,					--納車日
		CarriedBalance decimal(10,0),		--前月繰越
		PresentMonth decimal(10,0),			--当月発生
		PaymentAmount decimal(10,0),		--当月入金
		CustomerBalance decimal(10,0),		--顧客請求分の残高（合計）
		TradeBalance decimal(10,0),			--下取仕入残高（合計）
		RemainDebtBalance decimal(10,0),	--下取残債残高（合計）
		BalanceAmount decimal(10,0),		--残高
	)
	CREATE UNIQUE INDEX IX_Temp_AccountsReceivable ON #Temp_AccountsReceivableCar ([CloseMonth], [SlipNumber])

	CREATE TABLE #Temp_CarSalesHeader (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[DepartmentCode] NVARCHAR(3)					-- 部門コード
	,	[CustomerCode] NVARCHAR(10)						-- 顧客コード
	,	[SalesDate]	datetime							-- 納車日
	)
	CREATE UNIQUE INDEX IX_Temp_CarSalesHeader ON #Temp_CarSalesHeader ([SlipNumber])

	--Add 2016/12/22 arc yano #3682
	--伝票情報格納
	CREATE TABLE #Temp_SlipInfo (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[SalesDate]	datetime							-- 納車日
	,	[DepartmentCode] NVARCHAR(3)					-- 部門コード
	,	[CustomerCode] NVARCHAR(10)						-- 顧客コード
	)
	CREATE UNIQUE INDEX IX_Temp_SlipInfo ON #Temp_SlipInfo ([SlipNumber])

	--Add 2018/01/27 arc yano #3717
	--伝票
	CREATE TABLE #Temp_SlipList (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	)
	CREATE UNIQUE INDEX IX_Temp_SlipList ON #Temp_SlipList ([SlipNumber])

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
		SELECT @TargetMonthFrom = DATEADD(m, 1, ISNULL(MAX(CONVERT(datetime, cm.[CloseMonth], 120)), @THISMONTH))
		FROM [CloseMonthControl] cm				--経理締
		WHERE cm.[CloseStatus] = '003'			--本締済
	END
	ELSE
	BEGIN
		SET @TargetMonthFrom = @TargetMonth --スナップショット保存の場合は指定月を設定する。
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
		
		--■前月分のデータを一時テーブルにあらかじめ格納する。
		INSERT INTO #Temp_AccountsReceivableCar
		SELECT
			CloseMonth,				--締月
			SlipNumber,				--伝票番号
			DepartmentCode,			--部門コード
			DepartmentName,			--部門名
			CustomerCode,			--顧客コード
			CustomerName,			--顧客名
			SalesDate,				--納車日
			CarriedBalance,			--前月繰越
			PresentMonth,			--当月発生
			PaymentAmount,			--当月入金
			CustomerBalance,		--顧客請求分の残高（合計）
			TradeBalance,			--下取仕入残高（合計）
			RemainDebtBalance,		--下取残債残高（合計）
			BalanceAmount			--残高

		FROM [dbo].AccountsReceivableCar
		
		WHERE CloseMonth = @TargetMonthPrevious	--対象年月の前月
		  AND (ISNULL(CustomerBalance, 0) <> 0 OR ISNULL(TradeBalance, 0) <> 0 OR ISNULL(RemainDebtBalance, 0) <> 0 OR ISNULL(BalanceAmount, 0) <> 0)
		  AND DelFlag = '0'

		--■処理対象月数分ループ
		WHILE @TargetMonthCount > 0
		BEGIN
			--一次表初期化
			DELETE FROM  #Temp_CodeList							--コードリスト
			DELETE FROM  #Temp_CarriedBalance					--前月繰越リスト
			DELETE FROM  #Temp_ReceiptPlan						--入金予定リスト
			DELETE FROM  #Temp_SalesOrderReceiptPlan			--入金予定リスト
			DELETE FROM  #Temp_Journal							--入金実績リスト
			--DELETE FROM  #Temp_SalesOrderJournal				--入金実績リスト
			DELETE FROM  #Temp_CarSalesHeader					--車両伝票(赤伝・黒伝)除外用
			DELETE FROM  #Temp_SlipInfo							--伝票情報									--Add 2016/12/22 arc yano #3682
			DELETE FROM  #Temp_SlipList							--補完伝票リスト						　　--Add 2018/01/27 arc yano #3717
			
			/********************
			■■前月繰越リスト
			*********************/
			--前月分の残高が0でないものを取得
			INSERT INTO #Temp_CarriedBalance
			SELECT	[SlipNumber]								--伝票番号
				,	[SalesDate]									--納車日
				,	[DepartmentCode]							--部門コード
				,	[CustomerCode]								--顧客コード
				,	[BalanceAmount]								--残高
			FROM
				#Temp_AccountsReceivableCar 					--一時テーブルから取得
			WHERE
				[CloseMonth] = @TargetMonthPrevious
				AND (
					ISNULL(CustomerBalance, 0) <> 0 OR 
					ISNULL(TradeBalance, 0) <> 0 OR 
					ISNULL(RemainDebtBalance, 0) <> 0 OR 
					ISNULL(BalanceAmount, 0) <> 0
					)
			
			/********************
			■■入金予定リスト
			*********************/
			--①入金予定データを伝票番号、請求先コードで集計した結果を格納する。
			INSERT INTO #Temp_ReceiptPlan
			SELECT
					rp.[SlipNumber]																											--伝票番号
				,	SUM(CASE WHEN  (rp.ReceiptType <> '012' AND rp.ReceiptType <> '013') THEN ISNULL(rp.Amount, 0) ELSE 0 END) AS [Amount]	--下取・残債以外の入金予定額
				,	SUM(CASE WHEN  rp.ReceiptType = '013' THEN ISNULL(rp.Amount, 0) ELSE 0 END) AS [TradeInAmount]							--下取仕入の入金予定金額
				,	SUM(CASE WHEN  rp.ReceiptType = '012' THEN ISNULL(rp.Amount, 0) ELSE 0 END) AS [RemainDebtAmount]						--残債の入金予定金額
				,	SUM(ISNULL(rp.[Amount], 0)) AS [PresentMonth]																			--当月発生(入金予定額＋下取仕入金額＋残債金額)
			FROM [dbo].[ReceiptPlan] rp inner join [dbo].[CustomerClaim] cc ON rp.[CustomerClaimCode] = cc.[CustomerClaimCode] 
			WHERE rp.[SlipNumber] is not NULL
			  AND rp.[SlipNumber] <> ''
			  AND rp.[CustomerClaimCode] is not NULL
			  AND rp.[CustomerClaimCode] <> ''
			  AND cc.CustomerClaimType <> '003'				--請求先タイプ≠「クレジット」
			  AND rp.[DelFlag] = '0'

			GROUP BY
					rp.[SlipNumber]

			--③車両の入金予定情報を設定する。
			--(1)車両伝票で対象年月の黒伝票(xxxxxxxx-2)を伝票番号の枝番を除いた状態で退避			
			INSERT INTO #Temp_CarSalesHeader
			SELECT
					LEFT(sh.[SlipNumber], 8)						--伝票番号(枝番は取り除く)
				,	sh.[DepartmentCode]								--部門コード
				,	sh.[CustomerCode]								--顧客コード
				,	sh.[SalesDate]									--納車日
			FROM 
				[CarSalesHeader] sh									--車両伝票ヘッダ
			WHERE
				sh.[SlipNumber] like '%-2%' AND						--黒伝票(xxxxxxxx-2) 
				sh.[SalesOrderStatus] = '005' AND					--納車済
				sh.[SalesDate] >= @TargetDateFrom AND
				sh.[SalesDate] <  @TargetDateTo AND 					
				sh.[DelFlag] = '0'
				
			--(2)車両伝票で対象年月の黒伝票(xxxxxxxx-2)の無い赤伝票(xxxxxxxx-1)を-1を取り除いた伝票番号で退避
			INSERT INTO #Temp_CarSalesHeader
			SELECT
					LEFT(sh.[SlipNumber], 8)						--伝票番号(枝番は取り除く)
				,	sh.[DepartmentCode]								--部門コード
				,	sh.[CustomerCode]								--顧客コード
				,	sh.[SalesDate]									--納車日
			FROM 
				[CarSalesHeader] sh									--車両伝票ヘッダ
			WHERE
				sh.[SlipNumber] like '%-1%' AND						--黒伝票(xxxxxxxx-1) 
				sh.[SalesOrderStatus] = '005' AND					--納車済		
				sh.[SalesDate] >= @TargetDateFrom AND 				
				sh.[SalesDate] <  @TargetDateTo AND 				
				sh.[DelFlag] = '0' AND
				NOT EXISTS
				(
					select 'x' from #Temp_CarSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
				)
			
			--インデックス再生成
			DROP INDEX IX_Temp_CarSalesHeader ON #Temp_CarSalesHeader
			CREATE UNIQUE INDEX IX_Temp_CarSalesHeader ON #Temp_CarSalesHeader ([SlipNumber])
			

			--(3)元伝票のみのもの
			INSERT INTO #Temp_SalesOrderReceiptPlan
			SELECT
					sh.[SlipNumber] AS [SlipNumber]							--伝票番号
				,	sh.[SalesDate]	AS [SalesDate]							--納車日
				,	sh.[DepartmentCode]	AS [DepartmentCode]					--部門コード
				,	sh.[CustomerCode] AS [CustomerCode]						--顧客コード
				,	ISNULL(rp.[Amount], 0) AS [Amount]						--入金予定額
				,	ISNULL(rp.[TradeInAmount], 0) AS [TradeInAmount]		--下取仕入
				,	ISNULL(rp.[RemainDebtAmount], 0) AS [RemainDebtAmount]	--残債
				,	ISNULL(rp.[PresentMonth], 0) AS [PresentMonth]			--当月発生(入金予定額 ＋ 下取仕入金額 ＋ 残債金額)
			FROM 
				[CarSalesHeader] sh inner join								--車両伝票ヘッダ
				#Temp_ReceiptPlan rp										--入金予定
			ON 
				sh.[SlipNumber] = rp.[SlipNumber]
			WHERE 
				sh.[SalesOrderStatus] = '005' AND							--納車済
				sh.[SalesDate] >= @TargetDateFrom AND 				
				sh.[SalesDate] <  @TargetDateTo AND 				
				sh.[DelFlag] = '0' AND
				NOT EXISTS
				(
					select 'x' from #Temp_CarSalesHeader tsh where tsh.SlipNumber = LEFT(sh.SlipNumber, 8)			---赤伝・黒伝票が存在しない伝票
				)
				
			--(3)赤伝・または黒伝まである場合はサマリーして格納(部門コード、顧客コード等は赤伝、または黒伝のものを使用する)
			INSERT INTO #Temp_SalesOrderReceiptPlan
			SELECT
					gsr.SlipNumber												--伝票番号
				,	ttsh.SalesDate												--納車日
				,	ttsh.DepartmentCode											--部門コード
				,	ttsh.CustomerCode											--顧客コード
				,	gsr.Amount													--入金予定額
				,	gsr.TradeInAmount											--下取仕入
				,	gsr.RemainDebtAmount										--残債
				,	gsr.PresentMonth											--発生金額()
			FROM
			(
				SELECT
						LEFT(sh.[SlipNumber], 8) AS [SlipNumber]					--伝票番号
					,	SUM(ISNULL(rp.[Amount], 0)) AS [Amount]						--入金予定額
					,	SUM(ISNULL(rp.[TradeInAmount], 0)) AS [TradeInAmount]		--諸費用
					,	SUM(ISNULL(rp.[RemainDebtAmount], 0)) AS [RemainDebtAmount]	--諸費用
					,	SUM(ISNULL(rp.[PresentMonth], 0)) AS [PresentMonth]			--当月発生(入金予定額 ＋ 下取仕入金額 ＋ 残債金額)
				FROM 
					[CarSalesHeader] sh inner join									--サービス伝票ヘッダ
					#Temp_ReceiptPlan rp											--入金予定
				ON 
					sh.[SlipNumber] = rp.[SlipNumber]
				WHERE 
					sh.[SalesOrderStatus] = '005' AND								--納車済
					sh.[SalesDate] >= @TargetDateFrom AND 				
					sh.[SalesDate] <  @TargetDateTo AND 				
					sh.[DelFlag] = '0' AND
					EXISTS
					(
						select 'x' from #Temp_CarSalesHeader tsh where tsh.SlipNumber = LEFT(sh.SlipNumber, 8)			--赤伝票・黒伝票が存在する伝票
					)
				GROUP BY
					 LEFT(sh.[SlipNumber], 8)
			) gsr INNER JOIN #Temp_CarSalesHeader ttsh  ON gsr.SlipNumber = ttsh.SlipNumber

			--インデックス再生成
			DROP INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan
			CREATE UNIQUE INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan ([SlipNumber])

			/***************************
			■■入金実績リスト
			****************************/
			--①入金実績データを伝票番号で集計した結果を格納する。
			--2016/12/22 Mod arc yano #3682 伝票番号、請求先単位で集計を行う（赤伝・黒伝等の枝番も元伝票番号として集計する）
			INSERT INTO #Temp_Journal
			SELECT
				 LEFT(j.SlipNumber, 8) AS SlipNumber
				,SUM(j.TradeInAmount) AS TradeInAmount
				,SUM(j.RemainDebtAmount) AS RemainDebtAmount
				,SUM(j.Amount) AS Amount
				,SUM(j.TotalAmount) AS TotalAmount
			FROM
			(
				SELECT
						jn.[SlipNumber]																										--伝票番号
					,	CASE WHEN  jn.AccountType = '013' THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [TradeInAmount]							--下取仕入の入金予定金額			
					,	CASE WHEN  jn.AccountType = '012' THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [RemainDebtAmount]						--残債の入金予定金額
					,	CASE WHEN  ( jn.AccountType <> '012' AND jn.AccountType <> '013') THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [Amount]	--下取・残債以外の入金予定額
					,	ISNULL(jn.[Amount], 0) AS [TotalAmount]																				--当月発生(入金予定額＋下取仕入金額＋残債金額)
				FROM
					[dbo].[Journal] jn inner join
					[dbo].[CustomerClaim] cc
				ON
					jn.CustomerClaimCode = cc.CustomerClaimCode 
				WHERE
					jn.[SlipNumber] is not NULL AND
					jn.[SlipNumber] <> '' AND
					jn.[CustomerClaimCode] is not NULL AND
					jn.[CustomerClaimCode] <> '' AND
					jn.[JournalType] =  '001' AND					--入出金区分=「入金」
					jn.[AccountType] <> '099' AND					--口座の種別が「データ破棄」以外
					jn.[JournalDate] >= @TargetDateFrom	AND		
					jn.[JournalDate] <  @TargetDateTo	AND		
					jn.[DelFlag] = '0' AND
					cc.CustomerClaimType <> '003' 					--請求先タイプ≠「クレジット」
			) j
				GROUP BY
					LEFT(j.[SlipNumber], 8)
			--インデックス再生成
			DROP INDEX IX_Temp_Journal ON #Temp_Journal
			CREATE UNIQUE INDEX IX_Temp_Journal ON #Temp_Journal ([SlipNumber])


			/*
			INSERT INTO #Temp_Journal
			SELECT
				 j.SlipNumber AS SlipNumber
				,SUM(j.TradeInAmount) AS TradeInAmount
				,SUM(j.RemainDebtAmount) AS RemainDebtAmount
				,SUM(j.Amount) AS Amount
				,SUM(j.TotalAmount) AS TotalAmount
			FROM
			(
			
				SELECT
						jn.[SlipNumber]																										--伝票番号
					,	CASE WHEN  jn.AccountType = '013' THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [TradeInAmount]							--下取仕入の入金予定金額			
					,	CASE WHEN  jn.AccountType = '012' THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [RemainDebtAmount]						--残債の入金予定金額
					,	CASE WHEN  ( jn.AccountType <> '012' AND jn.AccountType <> '013') THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [Amount]	--下取・残債以外の入金予定額
					,	ISNULL(jn.[Amount], 0) AS [TotalAmount]																			--当月発生(入金予定額＋下取仕入金額＋残債金額)
				FROM
					[dbo].[Journal] jn inner join
					[dbo].[CustomerClaim] cc
				ON
					jn.CustomerClaimCode = cc.CustomerClaimCode 
				WHERE
					jn.[SlipNumber] is not NULL AND
					jn.[SlipNumber] <> '' AND
					jn.[CustomerClaimCode] is not NULL AND
					jn.[CustomerClaimCode] <> '' AND
					jn.[JournalType] =  '001' AND					--入出金区分=「入金」
					jn.[AccountType] <> '099' AND					--口座の種別が「データ破棄」以外
					jn.[JournalDate] >= @TargetDateFrom	AND		
					jn.[JournalDate] <  @TargetDateTo	AND		
					jn.[DelFlag] = '0' AND
					cc.CustomerClaimType <> '003' 					--請求先タイプ≠「クレジット」
				-- ADD 2016/12/16 arc yano 車両管理 過去データ対応(赤伝の入金日が赤伝作成日以前に設定されているデータの取得)
				UNION					
				SELECT
						jn.[SlipNumber]																											--伝票番号
					,	CASE WHEN  jn.AccountType = '013' THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [TradeInAmount]							--下取仕入の入金予定金額			
					,	CASE WHEN  jn.AccountType = '012' THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [RemainDebtAmount]						--残債の入金予定金額
					,	CASE WHEN  ( jn.AccountType <> '012' AND jn.AccountType <> '013') THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [Amount]	--下取・残債以外の入金予定額
					,	ISNULL(jn.[Amount], 0) AS [TotalAmount]																			--当月発生(入金予定額＋下取仕入金額＋残債金額)
				FROM
					[dbo].[Journal] jn inner join
					[dbo].[CustomerClaim] cc
				ON
					jn.CustomerClaimCode = cc.CustomerClaimCode
				WHERE
					jn.[SlipNumber] is not NULL AND
					jn.[SlipNumber] <> '' AND
					jn.[CustomerClaimCode] is not NULL AND
					jn.[CustomerClaimCode] <> '' AND
					jn.[JournalType] =  '001' AND					--入出金区分=「入金」
					jn.[AccountType] <> '099' AND					--口座の種別が「データ破棄」以外
					jn.[JournalDate] <  @TargetDateTo	AND			--入金日が当月以前
					jn.[DelFlag] = '0' AND
					cc.CustomerClaimType <> '003' AND				--請求先タイプ≠「クレジット」
					EXISTS
					(
						select 
							'x' 
						from 
							dbo.CarSalesHeader sh 
						where
							sh.SalesOrderStatus = '005' and
							sh.SalesDate >= @TargetDateFrom and
							sh.SalesDate < @TargetDateTo and
							sh.DelFlag = '0' and
							sh.SlipNumber = jn.SlipNumber
					) 
			) j
				GROUP BY
					j.[SlipNumber]
			
			--②車両の入金実績情報を設定する。
			--(1)元伝のみ
			INSERT INTO #Temp_SalesOrderJournal
			SELECT 
					LEFT(sh.[SlipNumber], 8) AS [SlipNumber]					--伝票番号
				,	sh.[SalesDate] AS [SalesDate]								--納車日
				,	sh.[DepartmentCode]	AS [DepartmentCode]						--部門コード
				,	sh.[CustomerCode]	AS [CustomerCode]						--顧客コード
				,	SUM(ISNULL(jn.[TradeInAmount], 0)) AS [TradeInAmount]		--下取仕入
				,	SUM(ISNULL(jn.[RemainDebtAmount], 0)) AS [RemainDebtAmount]	--残債
				,	SUM(ISNULL(jn.[Amount], 0)) AS [Amount]						--下取・残債以外の入金額
				,	SUM(ISNULL(jn.[TotalAmount], 0)) AS [TotalAmount]			--当月入金(下取・残債以外の入金額 ＋ 下取仕入金額 ＋ 残債金額)
			FROM 
				dbo.CarSalesHeader sh inner join							--車両伝票ヘッダ
				#Temp_Journal jn											--入金実績
			ON 
				sh.[SlipNumber] = jn.[SlipNumber]
			WHERE
				sh.[SalesDate] >= @TargetDateFrom AND 						--2016/12/16 arc yano 伝票日付による絞込を行う		
				sh.[SalesDate] <  @TargetDateTo AND 						--2016/12/16 arc yano 伝票日付による絞込を行う
				sh.[DelFlag] = '0' AND
				NOT EXISTS
				(
					select 'x' from #Temp_CarSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
				)	
			GROUP BY 
				  sh.[SlipNumber]
				, sh.[SalesDate]
				, sh.[DepartmentCode]
				, sh.[CustomerCode]

			--(2)赤伝、または黒伝まである入金予定をサマリーして格納(部門コード、顧客コード等は赤伝、または黒伝のものを使用する
			INSERT INTO #Temp_SalesOrderJournal
			SELECT
					jsh.SlipNumber									--伝票番号
				,	tssh.SalesDate									--納車日
				,	tssh.DepartmentCode								--部門コード
				,	tssh.CustomerCode								--顧客コード
				,	jsh.TradeInAmount								--下取仕入
				,	jsh.RemainDebtAmount							--残債
				,	jsh.Amount										--下取・残債以外の入金
				,	jsh.TotalAmount									--入金実績合計
			FROM

			(
				SELECT 
						LEFT(sh.[SlipNumber], 8) AS [SlipNumber]				--伝票番号
					,	SUM(ISNULL(jn.[Amount], 0)) AS [Amount]						--下取・残債以外の入金額
					,	SUM(ISNULL(jn.[TradeInAmount], 0)) AS [TradeInAmount]		--下取仕入
					,	SUM(ISNULL(jn.[RemainDebtAmount], 0)) AS [RemainDebtAmount]	--残債
					,	SUM(ISNULL(jn.[TotalAmount], 0)) AS [TotalAmount]			--当月入金(下取・残債以外の入金額 ＋ 下取仕入金額 ＋ 残債金額)
				FROM 
					dbo.CarSalesHeader sh inner join							--サービス伝票ヘッダ
					#Temp_Journal jn											--入金実績
				ON 
					sh.[SlipNumber] = jn.[SlipNumber]
				WHERE 
					sh.[DelFlag] = '0' AND
					EXISTS
					(
						select 'x' from #Temp_CarSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
					)	
				GROUP BY 
					LEFT(sh.[SlipNumber], 8)
			) jsh INNER JOIN #Temp_CarSalesHeader tssh ON jsh.SlipNumber = tssh.SlipNumber
		
			--インデックス再生成
			DROP INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal
			CREATE UNIQUE INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal ([SlipNumber])
			*/

			--Mod 2018/01/27 arc yano #3717
			--Add 2016/12/22 arc yano #3682
			/********************
			■■伝票情報
			*********************/
			--Step1.集計月末時点で最も新しい伝票を取得する
			INSERT INTO #Temp_SlipInfo
			SELECT 
				  LEFT(sh.[SlipNumber], 8) AS SlipNumber	--伝票番号
				, sh.[SalesDate]							--納車日
				, sh.[DepartmentCode]						--部門コード
				, sh.[CustomerCode]							--顧客コード
			FROM
				dbo.CarSalesHeader sh INNER JOIN
				(
					SELECT
						MAX(SlipNumber) as SlipNumber 
					FROM 
						dbo.CarSalesHeader
					WHERE 
						DelFlag = '0' AND
						CreateDate < @TargetDateTo --AND
						--RevisionNumber = 1
					GROUP BY 
						LEFT(SlipNumber , 8)
				) sh2
			ON sh.SlipNumber = sh2.SlipNumber
			WHERE
				sh.DelFlag = '0'

			--Step2 Step1で取得できなかった伝票の伝票番号を退避
			INSERT INTO #Temp_SlipList
			SELECT
				DISTINCT(tj.SlipNumber)
			FROM
				#Temp_Journal tj
			WHERE
				NOT EXISTS
				(
					select 'x' from #Temp_SlipInfo si where tj.SlipNumber = si.SlipNumber 
				)

			--インデックス再生成
			DROP INDEX IX_Temp_SlipList ON #Temp_SlipList
			CREATE UNIQUE INDEX IX_Temp_SlipList ON #Temp_SlipList ([SlipNumber])


			--Step3 Step1で取得できなかった伝票は作成日の条件なしで取得する
			INSERT INTO #Temp_SlipInfo
			SELECT 
				  sh.[SlipNumber] AS SlipNumber				--伝票番号
				, sh.[SalesDate]							--納車日
				, sh.[DepartmentCode]						--部門コード
				, sh.[CustomerCode]							--顧客コード
			FROM
				dbo.CarSalesHeader sh
			WHERE
				sh.DelFlag = '0' AND
				EXISTS
				(
					select 'x' from #Temp_SlipList tsl where sh.SlipNumber = tsl.SlipNumber
				)
	

			--インデックス再生成
			DROP INDEX IX_Temp_SlipInfo ON #Temp_SlipInfo
			CREATE UNIQUE INDEX IX_Temp_SlipInfo ON #Temp_SlipInfo ([SlipNumber])


			/********************
			■■コードリスト
			*********************/
			--入金予定／実績リストに存在する、伝票番号、請求先コード等の組合せのリストを作成する
			INSERT INTO #Temp_CodeList
			SELECT
					l.[SlipNumber]
				,	(CASE WHEN si.[SlipNumber] is null THEN (CASE WHEN rp.[SalesDate] is null THEN cb.[SalesDate] ELSE rp.[SalesDate] END) ELSE si.[SalesDate] END) AS [SalesDate]
				,	(CASE WHEN si.[SlipNumber] is null THEN (CASE WHEN rp.[DepartmentCode] is null THEN cb.[DepartmentCode] ELSE rp.[DepartmentCode] END) ELSE si.[DepartmentCode] END) AS [DepartmentCode]
				,	(CASE WHEN si.[SlipNumber] is null THEN (CASE WHEN rp.[CustomerCode] is null THEN cb.[CustomerCode] ELSE rp.[CustomerCode] END) ELSE si.[CustomerCode] END) AS [CustomerCode]
				/*
				,	(CASE WHEN rp.[SalesDate] is null THEN (CASE WHEN oj.[SalesDate] is null THEN  cb.[SalesDate] ELSE oj.[SalesDate] END) ELSE rp.[SalesDate] END) AS [SalesDate]
				,	(CASE WHEN rp.[DepartmentCode] is null THEN (CASE WHEN oj.[DepartmentCode] is null THEN cb.[DepartmentCode] ELSE oj.[DepartmentCode] END) ELSE rp.[DepartmentCode] END) AS [DepartmentCode]
				,	(CASE WHEN rp.[CustomerCode] is null THEN (CASE WHEN oj.[CustomerCode] is null THEN cb.[CustomerCode] ELSE oj.[CustomerCode] END) ELSE rp.[CustomerCode] END) AS [CustomerCode]
				*/
			FROM (
				SELECT
						[SlipNumber]
				FROM
					#Temp_SalesOrderReceiptPlan
				UNION
				SELECT
						[SlipNumber]
				
				FROM
					#Temp_Journal					--Mod 2016/12/22 arc yano #3682
				--	#Temp_SalesOrderJournal
				UNION
				
				SELECT
						[SlipNumber]
				FROM
					#Temp_CarriedBalance
			) AS l 
			LEFT OUTER JOIN
				#Temp_SalesOrderReceiptPlan rp
			ON
				l.SlipNumber = rp.SlipNumber
			LEFT OUTER JOIN
				#Temp_Journal oj					--Mod 2016/12/22 arc yano #3682
				--#Temp_SalesOrderJournal oj
			ON
				l.SlipNumber = oj.SlipNumber		
			LEFT OUTER JOIN
				#Temp_CarriedBalance cb
			ON
				l.SlipNumber = cb.SlipNumber
			LEFT OUTER JOIN
				#Temp_SlipInfo si					--Add 2016/12/22 arc yano #3682
			ON
				l.SlipNumber = si.SlipNumber

			/********************
			■■売掛金残高リスト
			*********************/
			--コードリスト、入金予定／実績リストより、売掛金残高を作成する。					
			--動作指定による
			IF @ActionFlag = 0		--動作指定 =「表示」の場合は一時表に格納
			BEGIN
				INSERT INTO #Temp_AccountsReceivableCar

				SELECT
						@TargetDateFrom AS [CloseMonth]																														--締月
					,	cl.[SlipNumber]																																		--伝票番号
					,	cl.[DepartmentCode]																																	--部門コード
					,	dp.[DepartmentName]																																	--部門名称
					,	cl.[CustomerCode]																																	--顧客コード
					,	cu.[CustomerName]
					/*	--Mod 2016/12/22 arc yano #3682																															--顧客名称
					,	CASE WHEN ISNULL(cb.[CarriedBalance], 0) = 0 
							  and ISNULL(rp.[Amount], 0) + ISNULL(rp.TradeInAmount, 0) + ISNULL(rp.RemainDebtAmount, 0) = 0 
							  and ISNULL(jn.[Amount], 0) > 0 
							 THEN NULL 
							 ELSE cl.[SalesDate]
					*/
						--Mod 2018/01/27 arc yano
					,	CASE WHEN cl.[SalesDate] is null
							 THEN NULL 
							 ELSE 
								CASE WHEN 
									DATEDIFF(d, cl.[SalesDate], @TargetDateTo) >= 0		--Mod 2019/05/22 yano #3954		
								THEN 
									cl.[SalesDate]
								ELSE 
									NULL
								END
						END  AS [SalesDate]																																	--納車日
					,	ISNULL(cb.[CarriedBalance], 0) AS [CarriedBalance]																									--前月繰越
					,	ISNULL(rp.[PresentMonth], 0) AS [PresentMonth]																										--当月発生
					,	ISNULL(jn.[TotalAmount], 0) AS [PaymentAmount]																										--当月入金
					,	ISNULL(rp.Amount, 0) - ISNULL(jn.Amount, 0) AS [CustomerBalance]																					--残債・下取以外の残高
					,	ISNULL(rp.TradeInAmount, 0) - ISNULL(jn.TradeInAmount, 0) AS [TradeBalance]																			--下取仕入残高
					,	ISNULL(rp.[RemainDebtAmount], 0) - ISNULL(jn.RemainDebtAmount, 0) AS [RemainDebtAmount]																--残債残高
					,	ISNULL(cb.[CarriedBalance], 0) + ISNULL(rp.[PresentMonth], 0) - ISNULL(jn.[TotalAmount], 0) AS [BalanceAmount]										--残高(前月繰越＋当月発生－当月入金)
					
				FROM #Temp_CodeList cl																				--コードリスト
				
				INNER JOIN [dbo].[Department] dp																	--部門マスタ
					ON cl.[DepartmentCode] = dp.[DepartmentCode]
				INNER JOIN [dbo].[Customer] cu																		--顧客マスタ
					ON cl.[CustomerCode] = cu.[CustomerCode]
				LEFT JOIN #Temp_CarriedBalance cb																	--前月繰越リスト
					ON cl.SlipNumber = cb.SlipNumber
				LEFT JOIN #Temp_SalesOrderReceiptPlan rp															--入金予定リスト
					ON cl.SlipNumber = rp.SlipNumber
				LEFT JOIN #Temp_Journal jn																			--入金実績リスト	--Mod 2016/12/22 arc yano #3682
				--LEFT JOIN #Temp_SalesOrderJournal jn																--入金実績リスト
					ON cl.SlipNumber = jn.SlipNumber

			END
			ELSE	--動作指定=「保存」の場合はDBのテーブルに格納
			BEGIN
				--格納する前に一度データを削除
				DELETE FROM 
					[dbo].[AccountsReceivableCar]
				WHERE 
					CloseMonth = @TargetDateFrom
						
				INSERT INTO [dbo].[AccountsReceivableCar]

				SELECT
						@TargetDateFrom AS [CloseMonth]																					--締月
					,	cl.[SlipNumber]																									--伝票番号
					,	cl.[DepartmentCode]																								--部門コード
					,	dp.[DepartmentName]																								--部門名称
					,	cl.[CustomerCode]																								--顧客コード
					,	cu.[CustomerName]																								--顧客名称
					/* --Mod 2016/12/22 arc yano #3682	
					,	CASE WHEN ISNULL(cb.[CarriedBalance], 0) = 0 
							  and ISNULL(rp.[Amount], 0) + ISNULL(rp.TradeInAmount, 0) + ISNULL(rp.RemainDebtAmount, 0) = 0 
							  and ISNULL(jn.[Amount], 0) > 0 
							 THEN NULL
							 ELSE cl.[SalesDate] 
					*/
						--Mod 2018/01/27 arc yano
					,	CASE WHEN cl.[SalesDate] is null
							 THEN NULL 
							 ELSE 
								CASE WHEN 
									DATEDIFF(d, cl.[SalesDate], @TargetDateTo) >= 0		--Mod 2019/05/22 yano #3954	
								THEN 
									cl.[SalesDate]
								ELSE 
									NULL
								END
						END  AS [SalesDate]																								--納車日
					,	ISNULL(cb.[CarriedBalance], 0) AS [CarriedBalance]																--前月繰越
					,	ISNULL(rp.[PresentMonth], 0) AS [PresentMonth]																	--当月発生
					,	ISNULL(jn.[TotalAmount], 0) AS [PaymentAmount]																	--当月入金
					,	ISNULL(rp.Amount, 0) - ISNULL(jn.Amount, 0) AS [CustomerBalance]												--残債・下取以外の残高
					,	ISNULL(rp.TradeInAmount, 0) - ISNULL(jn.TradeInAmount, 0) AS [TradeBalance]										--下取仕入残高
					,	ISNULL(rp.[RemainDebtAmount], 0) - ISNULL(jn.RemainDebtAmount, 0) AS [RemainDebtAmount]							--残債残高
					,	ISNULL(cb.[CarriedBalance], 0) + ISNULL(rp.[PresentMonth], 0) - ISNULL(jn.[TotalAmount], 0) AS [BalanceAmount]	--残高(前月繰越＋当月発生－当月入金)
					,	'sys' AS [CreateEmployeeCode]																					--作成者
					,	@NOW AS [CreateDate]																							--作成日時
					,	'sys' AS [LastUpdateEmployeeCode]																				--最終更新者
					,	@NOW AS [LastUpdateDate]																						--最終更新日時
					,	'0'	 AS [DelFlag]																								--削除フラグ

				FROM #Temp_CodeList cl																				--コードリスト
				
				INNER JOIN [dbo].[Department] dp																	--部門マスタ
					ON cl.[DepartmentCode] = dp.[DepartmentCode]
				INNER JOIN [dbo].[Customer] cu																		--顧客マスタ
					ON cl.[CustomerCode] = cu.[CustomerCode]
				LEFT JOIN #Temp_CarriedBalance cb																	--前月繰越リスト
					ON cl.SlipNumber = cb.SlipNumber
				LEFT JOIN #Temp_SalesOrderReceiptPlan rp															--入金予定リスト
					ON cl.SlipNumber = rp.SlipNumber
				LEFT JOIN #Temp_Journal jn																			--入金実績リスト			--Mod 2016/12/22 arc yano #3682
				--LEFT JOIN #Temp_SalesOrderJournal jn																--入金実績リスト
					ON cl.SlipNumber = jn.SlipNumber

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
			SET @PARAM = '@TargetMonth datetime, @SalesDateFrom nvarchar(10), @SalesDateTo nvarchar(10), @SlipNumber NVARCHAR(50), @DepartmentCode NVARCHAR(3), @CustomerCode NVARCHAR(10), @Zerovisible NVARCHAR(1)'
					
			SET @SQL = ''
			SET @SQL = @SQL +'SELECT' + @CRLF
			SET @SQL = @SQL +'    ar.CloseMonth' + @CRLF
			SET @SQL = @SQL +'	, ar.SlipNumber' + @CRLF
			SET @SQL = @SQL +'	, ar.DepartmentCode' + @CRLF
			SET @SQL = @SQL +'	, ar.DepartmentName' + @CRLF
			SET @SQL = @SQL +'	, ar.CustomerCode' + @CRLF
			SET @SQL = @SQL +'	, ar.CustomerName' + @CRLF
			/* --Mod 2016/12/22 arc yano #3682	
			SET @SQL = @SQL +'	, CASE WHEN ISNULL(ar.[CarriedBalance], 0) = 0' + @CRLF 
			SET @SQL = @SQL +'		    and ISNULL(ar.[PresentMonth], 0) = 0' + @CRLF 
			SET @SQL = @SQL +'		    and ISNULL(ar.[PaymentAmount], 0) > 0 ' + @CRLF
			SET @SQL = @SQL +'		   THEN NULL ' + @CRLF
			SET @SQL = @SQL +'		   ELSE ar.SalesDate' + @CRLF
			SET @SQL = @SQL +'	  END AS SalesDate' + @CRLF
			*/
			SET @SQL = @SQL +'	, ar.SalesDate' + @CRLF
			SET @SQL = @SQL +'	, ar.CarriedBalance' + @CRLF
			SET @SQL = @SQL +'	, ar.PresentMonth' + @CRLF
			SET @SQL = @SQL +'	, ar.PaymentAmount' + @CRLF
			SET @SQL = @SQL +'	, ar.CustomerBalance' + @CRLF
			SET @SQL = @SQL +'	, ar.TradeBalance' + @CRLF
			SET @SQL = @SQL +'	, ar.RemainDebtBalance' + @CRLF
			SET @SQL = @SQL +'	, ar.BalanceAmount' + @CRLF
			SET @SQL = @SQL +'	, '''' AS CreateEmployeeCode' + @CRLF
			SET @SQL = @SQL +'	, NULL AS CreateDate' + @CRLF
			SET @SQL = @SQL +'	, '''' AS LastUpdateEmployeeCode' + @CRLF
			SET @SQL = @SQL +'	, NULL AS LastUpdateDate' + @CRLF
			SET @SQL = @SQL +'	, ''0'' AS DelFlag' + @CRLF
			SET @SQL = @SQL +'FROM' + @CRLF
			SET @SQL = @SQL +'	#Temp_AccountsReceivableCar ar' + @CRLF
			SET @SQL = @SQL +'WHERE' + @CRLF
			SET @SQL = @SQL +'    ar.CloseMonth = @TargetMonth' + @CRLF 
			

			--納車日
			IF ((@SalesDateFrom is not null) AND (@SalesDateFrom <> '') AND ISDATE(@SalesDateFrom) = 1)
				IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
				BEGIN
					SET @SQL = @SQL +' AND ar.SalesDate >= @SalesDateFrom AND ar.SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF
				END
				ELSE
				BEGIN
					SET @SQL = @SQL +' AND ar.SalesDate = @SalesDateFrom' + @CRLF 
				END
			ELSE
				IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
				BEGIN
					SET @SQL = @SQL +' AND ar.SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF 
				END
			
			--伝票番号
			IF ((@SlipNumber is not null) AND (@SlipNumber <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ar.SlipNumber = @SlipNumber' + @CRLF
			END
			--部門コード
			IF ((@DepartmentCode is not null) AND (@DepartmentCode <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ar.DepartmentCode = @DepartmentCode' + @CRLF
			END
			--顧客コード
			IF ((@CustomerCode is not null) AND (@CustomerCode <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ar.CustomerCode = @CustomerCode' + @CRLF
			END
			
			IF ((@Zerovisible is not null) AND (@Zerovisible <>'') AND (@Zerovisible = '0'))
			BEGIN
				SET @SQL = @SQL + 'AND (ar.CustomerBalance != 0 OR ar.TradeBalance != 0 OR ar.RemainDebtBalance != 0)'+ @CRLF
			END

			EXECUTE sp_executeSQL @SQL, @PARAM, @TargetMonth, @SalesDateFrom, @SalesDateTo, @SlipNumber, @DepartmentCode, @CustomerCode, @Zerovisible
			
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


