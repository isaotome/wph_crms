USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[Insert_ReceivableBalance]    Script Date: 2016/06/06 16:04:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







-- ================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2015/08/10 arc yano #3233 売掛金帳票対応 売掛残高作成用のストアド追加
-- Update date: 
-- 2016/06/03 arc yano #3532 サービス伝票の集計時に「-1」「-2」の枝番を控除する
-- 2016/02/25 arc yano #3302_売掛金管理画面の項目追加・削除での対応漏れの修正
--
-- Description:	指定した年月の売掛残高の作成(※対象年月の指定がない場合は、締めステータス=「本締め」の最新月が対象月となる)
-- ==================================================================================================================================
CREATE PROCEDURE [dbo].[Insert_ReceivableBalance]
	@TargetMonth datetime = NULL					--残高の計算対象年月
AS
BEGIN

	SET NOCOUNT ON;

	--定数----------------------------
	DECLARE @NOW datetime					--現在日時
	DECLARE @TODAY datetime					--本日
	DECLARE @THISMONTH datetime				--当月
	DECLARE @TargetMonthEnd datetime		--対象年月月末

	----------------------------------

	--■■一時表の宣言
	/*************************************************************************/
	BEGIN TRY
		DROP TABLE #Temp_CodeList				--コードリスト(入金予定／実績に存在する全ての伝票番号、請求先コード等の組合せ)
		DROP TABLE #Temp_ReceiptPlan			--入金予定リスト
		DROP TABLE #Temp_SalesOrderReceiptPlan	--入金予定リスト(車両／サービス)
		DROP TABLE #Temp_Journal				--入金実績リスト
		DROP TABLE #Temp_SalesOrderJournal		--入金実績リスト(車両／サービス)
		DROP TABLE #Temp_ServiceSalesHeader		--サービス伝票ヘッダ
	END TRY
	BEGIN CATCH
		--無視
	END CATCH


	--コードリスト
	CREATE TABLE #Temp_CodeList (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- 請求先コード
	,	[SlipType]	NVARCHAR(1)							-- 伝票タイプ(0:車両／1:サービス)	
	,	[SalesDate]	datetime							-- 納車日
	,	[DepartmentCode] NVARCHAR(3)					-- 部門コード
	,	[CustomerCode] NVARCHAR(10)						-- 顧客コード
	,	[CustomerClaimType] NVARCHAR(3)					-- 請求先区分	
	)

	--入金予定リスト
	CREATE TABLE #Temp_ReceiptPlan (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- 請求先コード
	,	[CustomerClaimType] NVARCHAR(10)				-- 請求先区分
	,	[Amount] DECIMAL(10, 0)							-- 入金予定額
	)
	CREATE UNIQUE INDEX IX_Temp_ReceiptPlan ON #Temp_ReceiptPlan ([SlipNumber], [CustomerClaimCode])

	--入金予定リスト(車両／サービス)
	CREATE TABLE #Temp_SalesOrderReceiptPlan (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- 請求先コード
	,	[SlipType]	NVARCHAR(1)							-- 伝票タイプ(0:車両／1:サービス)	
	,	[SalesDate]	datetime							-- 納車日
	,	[DepartmentCode] NVARCHAR(3)					-- 部門コード
	,	[CustomerCode] NVARCHAR(10)						-- 顧客コード
	,	[CustomerClaimType] NVARCHAR(3)					-- 請求先区分
	,	[Amount] DECIMAL(10, 0)							-- 入金予定額
	)
	CREATE UNIQUE INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan ([SlipNumber], [CustomerClaimCode])

	--入金実績リスト
	CREATE TABLE #Temp_Journal (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- 請求先コード
	,	[CustomerClaimType] NVARCHAR(10)				-- 請求先区分
	,	[Amount] DECIMAL(10, 0)							-- 入金額
	)
	CREATE UNIQUE INDEX IX_Temp_Journal ON #Temp_Journal ([SlipNumber], [CustomerClaimCode])

	--入金実績リスト(車両／サービス)
	CREATE TABLE #Temp_SalesOrderJournal (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- 請求先コード
	,	[SlipType]	NVARCHAR(1)							-- 伝票タイプ(0:車両／1:サービス)	
	,	[SalesDate]	datetime							-- 納車日
	,	[DepartmentCode] NVARCHAR(3)					-- 部門コード
	,	[CustomerCode] NVARCHAR(10)						-- 顧客コード
	,	[CustomerClaimType] NVARCHAR(3)					-- 入金額
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal ([SlipNumber], [CustomerClaimCode])

	--サービス伝票ヘッダ //Add 2016/06/02
	CREATE TABLE #Temp_ServiceSalesHeader(
		[SlipNumber] NVARCHAR(50) NOT NULL				-- 伝票番号
	,	[DepartmentCode] NVARCHAR(3)					-- 部門コード
	,	[CustomerCode] NVARCHAR(10)						-- 顧客コード
	,	[SalesDate]	datetime							-- 納車日
	)
	CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader ([SlipNumber])

	/*************************************************************************/

	--処理結果
	DECLARE @ErrorMessage NVARCHAR(4000) = ''
    DECLARE @ErrorNumber INT = 0


	--現在日時
	SET @NOW = GETDATE()
	--当日
	SET @TODAY = CONVERT(DATETIME, CONVERT(NVARCHAR(10), GETDATE(), 111), 111)
	--当月1日
	SET @THISMONTH = CONVERT(DATETIME, CONVERT(NVARCHAR(7), @TODAY, 111) + '/01', 111)
	
	--■処理対象月の設定
	-- 対象年月が指定されていない場合は、月締め処理状況が「本締め」の月の中で最新月を処理対象年月に設定する。
	IF ((@TargetMonth is NULL) OR (@TargetMonth = CONVERT(datetime, '1900-01-01', 120)))
	BEGIN
		SELECT @TargetMonth = ISNULL(MAX(CONVERT(datetime, cmc.[CloseMonth], 120)), @THISMONTH)
		FROM [CloseMonthControl] cmc					--月締め処理状況
		WHERE cmc.[CloseStatus] = '003'					--「本締め」
	END

	--対象年月月末の設定(翌月1日を設定)
	SET @TargetMonthEnd = DATEADD( m, 1, @TargetMonth) 

	--トランザクション開始 
	BEGIN TRANSACTION
	BEGIN TRY

		BEGIN
			--一度売掛金テーブルの全レコードを削除
			TRUNCATE TABLE
				[dbo].[AccountsReceivable]
			
			--一次表初期化		
			DELETE FROM  #Temp_CodeList							--コードリスト
			DELETE FROM  #Temp_ReceiptPlan						--入金予定リスト
			DELETE FROM  #Temp_SalesOrderReceiptPlan			--入金予定リスト(車両／サービス)
			DELETE FROM  #Temp_Journal							--入金実績リスト
			DELETE FROM  #Temp_SalesOrderJournal				--入金実績リスト(車両／サービス)
			
			-----------------------------

			/********************
			■■入金予定リスト
			*********************/
			--①入金予定データを伝票番号、請求先コードで集計した結果を格納する。
			INSERT INTO #Temp_ReceiptPlan
			SELECT
					rp.[SlipNumber]								--伝票番号(枝番がある場合は集計する)
				,	rp.[CustomerClaimCode]						--請求先コード
				,	cc.[CustomerClaimType]						--請求先区分
				,	SUM(ISNULL(rp.Amount, 0)) as [Amount]		--金額(サマリ)
			FROM
				[dbo].[ReceiptPlan] rp inner join
				[dbo].[CustomerClaim] cc
			ON
				rp.	[CustomerClaimCode] = cc.[CustomerClaimCode] 
			WHERE
				rp.[SlipNumber] is not NULL AND
				rp.[SlipNumber] <> '' AND
				rp.[CustomerClaimCode] is not NULL AND
				rp.[CustomerClaimCode] <> '' AND
				cc.[CustomerClaimType] <> '003' and  --請求先タイプ≠「クレジット」
				rp.[DelFlag] = '0'
			GROUP BY
					rp.[SlipNumber]
				,	rp.[CustomerClaimCode]
				,	cc.[CustomerClaimType]

			--②車両の入金予定情報を設定する。
			INSERT INTO #Temp_SalesOrderReceiptPlan
			SELECT
					sh.[SlipNumber]									--伝票番号
				,	rp.[CustomerClaimCode]							--請求先コード
				,	'0' AS [SlipType]								--伝票タイプ(0:車両固定)
				,	sh.[SalesDate]									--納車日
				,	sh.[DepartmentCode]								--部門コード
				,	sh.[CustomerCode]								--顧客コード
				,	rp.[CustomerClaimType]							--請求先区分
				,	rp.[Amount]										--入金予定額
			FROM 
				[CarSalesHeader] sh inner join						--車両伝票ヘッダ
				#Temp_ReceiptPlan rp								--入金予定
			ON 
				sh.[SlipNumber] = rp.[SlipNumber]
			WHERE 
				sh.[SalesOrderStatus] = '005' AND					--納車済
				sh.[SalesDate] < @TargetMonthEnd AND 				--対象年月翌月１日より前
				sh.[DelFlag] = '0'		

			--③サービスの入金予定情報を設定する。
			--(1)サービス伝票で対象年月の黒伝票(xxxxxxxx-2)を-2を取り除いた伝票番号で退避
			INSERT INTO #Temp_ServiceSalesHeader
			SELECT
					LEFT(sh.[SlipNumber], 8)						--伝票番号(枝番は取り除く)
				,	sh.[DepartmentCode]								--部門コード
				,	sh.[CustomerCode]								--顧客コード
				,	sh.[SalesDate]									--納車日
			FROM 
				[ServiceSalesHeader] sh								--サービス伝票ヘッダ
			WHERE
				sh.[SlipNumber] like '%-2%' AND						--黒伝票(xxxxxxxx-2) 
				sh.[ServiceOrderStatus] = '006' AND					--納車済		
				sh.[SalesDate] < @TargetMonthEnd AND 				--対象年月翌月１日より前			
				sh.[DelFlag] = '0'
			
				--インデックス再生成
			DROP INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader
			CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader ([SlipNumber])

			--(2)サービス伝票で対象年月の黒伝票(xxxxxxxx-2)の無い赤伝票(xxxxxxxx-1)を-1を取り除いた伝票番号で退避
			INSERT INTO #Temp_ServiceSalesHeader
			SELECT
					LEFT(sh.[SlipNumber], 8)						--伝票番号(枝番は取り除く)
				,	sh.[DepartmentCode]								--部門コード
				,	sh.[CustomerCode]								--顧客コード
				,	sh.[SalesDate]									--納車日
			FROM 
				[ServiceSalesHeader] sh								--サービス伝票ヘッダ
			WHERE
				sh.[SlipNumber] like '%-1%' AND						--黒伝票(xxxxxxxx-1) 
				sh.[ServiceOrderStatus] = '006' AND					--納車済		
				sh.[SalesDate] < @TargetMonthEnd AND 				--対象年月翌月１日より前			
				sh.[DelFlag] = '0' AND
				NOT EXISTS
				(
					select 'x' from #Temp_ServiceSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
				)
			
			--インデックス再生成
			DROP INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader
			CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader ([SlipNumber])

			--(3)元伝のみしか存在しない
			INSERT INTO #Temp_SalesOrderReceiptPlan
			
			SELECT
					sh.[SlipNumber] AS [SlipNumber]							--伝票番号
				,	rp.[CustomerClaimCode]	AS [CustomerClaimCode]			--請求先コード
				,	'1' AS [SlipType]										--伝票タイプ
				,	sh.[SalesDate]	AS [SalesDate]							--納車日
				,	sh.[DepartmentCode]	AS [DepartmentCode]					--部門コード
				,	sh.[CustomerCode] AS [CustomerCode]						--顧客コード
				,	rp.[CustomerClaimType]	[CustomerClaimType]				--請求先区分
				,	ISNULL(rp.[Amount], 0) AS [Amount]						--入金予定額
			FROM 
				[ServiceSalesHeader] sh inner join							--サービス伝票ヘッダ
				#Temp_ReceiptPlan rp										--入金予定
			ON 
				sh.[SlipNumber] = rp.[SlipNumber]
			WHERE 
				sh.[ServiceOrderStatus] = '006' AND					--納車済
				sh.[SalesDate] < @TargetMonthEnd AND 				--対象年月翌月１日より前
				sh.[DelFlag] = '0'	AND				
				NOT EXISTS
				(
					select 'x' from #Temp_ServiceSalesHeader tsh where tsh.SlipNumber = LEFT(sh.SlipNumber, 8)			--黒伝票が存在しないもののみ
				)
			
			--(3)赤伝・または黒伝まである場合はサマリーして格納(部門コード、顧客コード等は赤伝、または黒伝のものを使用する)
			INSERT INTO #Temp_SalesOrderReceiptPlan
			SELECT
					gsr.SlipNumber
				,	gsr.CustomerClaimCode
				,	'1' AS [SlipType]
				,	ttsh.SalesDate
				,	ttsh.DepartmentCode
				,	ttsh.CustomerCode
				,	gsr.CustomerClaimType
				,	gsr.Amount
			FROM
			(
				SELECT
						LEFT(sh.[SlipNumber], 8) AS [SlipNumber]				--伝票番号
					,	rp.[CustomerClaimCode]	AS [CustomerClaimCode]			--請求先コード
					,	rp.[CustomerClaimType]	[CustomerClaimType]				--請求先区分
					,	SUM(ISNULL(rp.[Amount], 0)) AS [Amount]					--入金予定額
				FROM 
					[ServiceSalesHeader] sh inner join							--サービス伝票ヘッダ
					#Temp_ReceiptPlan rp										--入金予定
				ON 
					sh.[SlipNumber] = rp.[SlipNumber]
				WHERE 
					sh.[ServiceOrderStatus] = '006' AND							--納車済
					sh.[SalesDate] < @TargetMonthEnd AND 						--対象年月翌月１日より前				
					sh.[DelFlag] = '0' AND
					EXISTS
					(
						select 'x' from #Temp_ServiceSalesHeader tsh where tsh.SlipNumber = LEFT(sh.SlipNumber, 8)			--黒伝票が存在するもののみ
					)
				GROUP BY
						LEFT(sh.[SlipNumber], 8)
					,	rp.[CustomerClaimCode]
					,	rp.[CustomerClaimType]

			) gsr INNER JOIN #Temp_ServiceSalesHeader ttsh  ON gsr.SlipNumber = ttsh.SlipNumber

			--インデックス再生成
			DROP INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan
			CREATE UNIQUE INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan ([SlipNumber], [CustomerClaimCode])

			/********************
			■■入金実績リスト
			*********************/
			--①入金実績データを伝票番号、請求先コードで集計した結果を格納する。
			INSERT INTO #Temp_Journal
			SELECT
					jn.[SlipNumber]								--伝票番号
				,	jn.[CustomerClaimCode]						--請求先コード
				,	cc.[CustomerClaimType]						--請求先区分
				,	SUM(ISNULL(jn.Amount, 0)) as [Amount]		--金額(サマリ)
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
				jn.[JournalType] =  '001' AND				--入出金区分=「入金」
				jn.[AccountType] <> '099' AND				--口座の種別が「データ破棄」以外
				jn.[JournalDate] < @TargetMonthEnd	AND		--入金日が対象年月翌月１日より前
				jn.[DelFlag] = '0' AND
				cc.CustomerClaimType <> '003' 				--請求先タイプ≠「クレジット」

			GROUP BY
					jn.[SlipNumber]
				,	jn.[CustomerClaimCode]
				,	cc.[CustomerClaimType]		
			
			--②車両の入金実績情報を格納する。
			INSERT INTO #Temp_SalesOrderJournal
			SELECT 
					sh.[SlipNumber]									--伝票番号
				,	jn.[CustomerClaimCode]							--請求先コード
				,	'0' AS [SlipType]								--伝票タイプ(0:車両固定)
				,	sh.[SalesDate]									--納車日
				,	sh.[DepartmentCode]								--部門コード
				,	sh.[CustomerCode]								--顧客コード
				,	jn.[CustomerClaimType]							--請求先区分
				,	jn.[Amount]										--入金額	
			FROM 
				[CarSalesHeader] sh inner join						--車両伝票ヘッダ
				#Temp_Journal jn									--入金実績
			ON 
				sh.[SlipNumber] = jn.[SlipNumber]
			WHERE 	
				sh.[DelFlag] = '0'
			
			--③サービスの入金実績情報を設定する。
			--(1)元伝のみ
			INSERT INTO #Temp_SalesOrderJournal
			SELECT 
					sh.[SlipNumber] AS [SlipNumber]												--伝票番号
				,	jn.[CustomerClaimCode]	AS [CustomerClaimCode]								--請求先コード
				,	'1' AS [SlipType]															--伝票タイプ(1:サービス固定)
				,	sh.[SalesDate] AS [SalesDate]												--納車日
				,	sh.[DepartmentCode]	AS [DepartmentCode]										--部門コード
				,	sh.[CustomerCode]	AS [CustomerCode]										--顧客コード
				,	jn.[CustomerClaimType]  AS [CustomerClaimType]								--請求先区分
				,	ISNULL(jn.[Amount], 0) AS [Amount]											--入金額
			FROM 
				dbo.ServiceSalesHeader sh inner join											--サービス伝票ヘッダ
				#Temp_Journal jn																--入金実績
			ON 
				sh.[SlipNumber] = jn.[SlipNumber]
			WHERE 
				sh.[DelFlag] = '0' AND
				NOT EXISTS
				(
					select 'x' from #Temp_ServiceSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
				)

			--(2)赤伝、または黒伝まである入金予定をサマリーして格納(部門コード、顧客コード等は赤伝、または黒伝のものを使用する
			INSERT INTO #Temp_SalesOrderJournal
			SELECT
					jsh.SlipNumber
				,	jsh.CustomerClaimCode
				,	jsh.SlipType
				,	tssh.SalesDate
				,	tssh.DepartmentCode
				,	tssh.CustomerCode
				,	jsh.CustomerClaimType
				,	jsh.Amount
			FROM

			(
				SELECT 
						LEFT(sh.[SlipNumber], 8) AS [SlipNumber]																		--伝票番号
					,	jn.[CustomerClaimCode]	AS [CustomerClaimCode]																	--請求先コード
					,	'1' AS [SlipType]																								--伝票タイプ(1:サービス固定)
					,	jn.[CustomerClaimType]  AS [CustomerClaimType]																	--請求先区分
					,	SUM(ISNULL(jn.[Amount], 0))AS [Amount]																			--入金額(諸費用以外)
				FROM 
					dbo.ServiceSalesHeader sh inner join																				--サービス伝票ヘッダ
					#Temp_Journal jn																									--入金実績
				ON 
					sh.[SlipNumber] = jn.[SlipNumber]
				WHERE 
					sh.[DelFlag] = '0' AND
					EXISTS
					(
						select 'x' from #Temp_ServiceSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
					)	
				GROUP BY 
					LEFT(sh.[SlipNumber], 8)
					, jn.[CustomerClaimCode]
					, jn.[CustomerClaimType]
			) jsh INNER JOIN #Temp_ServiceSalesHeader tssh ON jsh.SlipNumber = tssh.SlipNumber
			
			--インデックス再生成
			DROP INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal
			CREATE UNIQUE INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal ([SlipNumber], [CustomerClaimCode])
			/********************
			■■コードリスト
			*********************/
			--入金予定／実績リストに存在する、伝票番号、請求先コード等の組合せのリストを作成する
			INSERT INTO #Temp_CodeList
			SELECT
					l.[SlipNumber]
				,	l.[CustomerClaimCode]
				,	l.[SlipType]
				,	l.[SalesDate]
				,	l.[DepartmentCode]
				,	l.[CustomerCode]
				,	l.[CustomerClaimType]
			FROM (
				SELECT
						[SlipNumber]
					,	[CustomerClaimCode]
					,	[SlipType]
					,	[SalesDate]
					,	[DepartmentCode]
					,	[CustomerCode]
					,	[CustomerClaimType]
				FROM
					#Temp_SalesOrderReceiptPlan
				UNION
				SELECT
						[SlipNumber]
					,	[CustomerClaimCode]
					,	[SlipType]
					,	[SalesDate]
					,	[DepartmentCode]
					,	[CustomerCode]
					,	[CustomerClaimType]
				FROM
					#Temp_SalesOrderJournal
			) AS l
	
			/********************
			■■売掛金残高リスト
			*********************/
			--コードリスト、入金予定／実績リストより、売掛金残高を作成する。
			INSERT INTO [dbo].[AccountsReceivable]

			SELECT
					@TargetMonth AS [CloseMonth]												--締月
				,	cl.[SlipNumber]																--伝票番号
				,	cl.[CustomerClaimCode]														--請求先コード
				,	cc.[CustomerClaimName]														--請求先名称
				,	cl.[CustomerClaimType]														--請求先区分
				,	ct.[Name]																	--請求先名称
				,	cl.[DepartmentCode]															--部門コード
				,	dp.[DepartmentName]															--部門名称
				,	cl.[CustomerCode]															--顧客コード
				,	cu.[CustomerName]															--顧客名称
				,	cl.[SlipType]																--伝票タイプ
				,	cn.[Name]																	--伝票タイプ名
				,	cl.[SalesDate]																--納車日
				,	NULL AS [CarriedBalance]													--前月繰越
				,	NULL AS [PresentMonth]														--当月発生
				,	NULL AS [Expendes]															--諸費用
				,	NULL AS [TotalAmount]														--合計
				,	NULL AS [Payment]															--当月入金(諸費用以外)
				--,	NULL AS [AdvancesReceived]													--当月前受金		-- Del 2016/02/25 ARC YANO 不具合対応
				,	(ISNULL(rp.[Amount], 0) - ISNULL(jn.[Amount], 0)) AS [BalanceAmount]		--残高
				,	'sys' AS [CreateEmployeeCode]												--作成者
				,	@NOW AS [CreateDate]														--作成日時
				,	'sys' AS [LastUpdateEmployeeCode]											--最終更新者
				,	@NOW AS [LastUpdateDate]													--最終更新日時
				,	'0'	 AS [DelFlag]															--削除フラグ
				,   NULL AS [ChargesPayment]													--当月入金(諸費用)	-- Add 2016/02/25 ARC YANO 不具合対応
			FROM #Temp_CodeList cl																--コードリスト
			INNER JOIN [dbo].[Department] dp																	--部門マスタ
				ON cl.[DepartmentCode] = dp.[DepartmentCode]
			INNER JOIN [dbo].[CustomerClaim] cc																	--請求先マスタ
				ON cl.[CustomerClaimCode] = cc.[CustomerClaimCode]
			INNER JOIN [dbo].[c_CustomerClaimType] ct															--請求先区分コードマスタ
				ON cl.[CustomerClaimType] = ct.[Code]
			INNER JOIN [dbo].[Customer] cu																		--顧客マスタ
				ON cl.[CustomerCode] = cu.[CustomerCode] 
			INNER JOIN [dbo].[c_CodeName] cn																	--顧客マスタ
				ON cl.[SlipType] = cn.[Code] AND cn.[CategoryCode] = '014'										--カテゴリコードは「014」
			LEFT JOIN #Temp_SalesOrderReceiptPlan rp															--入金予定リスト
				ON cl.SlipNumber = rp.SlipNumber AND cl.CustomerClaimCode = rp.CustomerClaimCode
			LEFT JOIN #Temp_SalesOrderJournal jn																--入金実績リスト
				ON cl.SlipNumber = jn.SlipNumber AND cl.CustomerClaimCode = jn.CustomerClaimCode			
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


