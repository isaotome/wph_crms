USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCashJournalOutput]    Script Date: 2015/02/23 16:13:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






--2015/02/23 arc yano 現金出納帳対応
CREATE PROCEDURE [dbo].[GetCashJournalOutput]
	@TargetDate datetime   		   --対象年月
AS


BEGIN
	/*-------------------------------------------*/
	/* 一時テーブル(現金在高テーブル)            */
	/*-------------------------------------------*/
	-- 現金在高テーブル
		CREATE TABLE #temp_CashBalance_1
		(
			OfficeCode 		nvarchar(3),	--事業所コード
			M				nvarchar(6),	--ClosedDateの月までを文字列で抜出
			ClosedDate		datetime,		--締め日
			TotalAmount		decimal(10, 0),	--総合計
			CashAccountCode nvarchar(3)		--現金口座コード
		)

	--データ挿入
	INSERT INTO #temp_CashBalance_1
	SELECT	
		 OfficeCode
		,CONVERT(VARCHAR(6), ClosedDate, 112)
		,ClosedDate
		,TotalAmount
		,CashAccountCode
	FROM
		dbo.CashBalance
	WHERE
		DelFlag = '0' AND
		CloseFlag = '1'

	-- 現金在高テーブル
		CREATE TABLE #temp_CashBalance_2
		(
			OfficeCode 		nvarchar(3),	--事業所コード
			TotalAmount		decimal(10, 0),	--総合計
			CashAccountCode nvarchar(3),	--現金口座コード
			M				nvarchar(6),	--ClosedDateの月までを文字列で抜出
			LastDate		datetime,		--締め日
			K				datetime		--締日(パラメータと比較用)
		)

	--データ挿入
	INSERT INTO #temp_CashBalance_2
	SELECT	
		 OfficeCode
		,TotalAmount
		,CashAccountCode
		,M
		,MAX(ClosedDate)
		,DATEADD(M, 1, CONVERT(datetime, M + '01', 120))
	FROM
		#temp_CashBalance_1
	GROUP BY
		OfficeCode, M, CashAccountCode, TotalAmount
	
	--インデックスの作成
	CREATE INDEX ix_temp_CashBalance_2 ON #temp_CashBalance_2(OfficeCode, M, CashAccountCode, TotalAmount)
	
	-- 現金在高テーブル
		CREATE TABLE #temp_CashBalance_3
		(
			OfficeCode 		nvarchar(3),	--事業所コード
			CashAccountCode nvarchar(3),	--現金口座コード
			M				nvarchar(6),	--ClosedDateの月までを文字列で抜出
			LastDate		datetime,		--締め日
			K				datetime		--締日(パラメータと比較用)
		)

	--データ挿入
	INSERT INTO #temp_CashBalance_3
	SELECT	
		 OfficeCode
		,CashAccountCode
		,M
		,MAX(LastDate)
		,K
	FROM
		#temp_CashBalance_2
	GROUP BY 
		OfficeCode, CashAccountCode, M, K
		
	-- 現金在高テーブル
		CREATE TABLE #temp_CashBalance_4
		(
			OfficeCode 		nvarchar(3),	--事業所コード
			TotalAmount		decimal(10, 0),	--総合計
			CashAccountCode nvarchar(3),	--現金口座コード
			M				nvarchar(6),	--ClosedDateの月までを文字列で抜出
			LastDate		datetime,		--締め日
			K				datetime		--締日(パラメータと比較用)
		)

	--データ挿入
	INSERT INTO #temp_CashBalance_4
	SELECT	
		 OfficeCode
		,TotalAmount
		,CashAccountCode
		,M
		,LastDate
		,K
	FROM
		#temp_CashBalance_2 a
	WHERE
		EXISTS
		(
			SELECT 'X'
			FROM
				#temp_CashBalance_3
			WHERE A.OfficeCode = OfficeCode AND
				  A.CashAccountCode = CashAccountCode AND
				  A.M = M AND
				  A.LastDate = LastDate AND
				  A.K = K
		)				
	/*-------------------------------------------*/
	/* 一時テーブル(入出金テーブル)            */
	/*-------------------------------------------*/
	-- 入出金テーブル１
		CREATE TABLE #temp_Journal_1
		(
			OfficeCode 		nvarchar(3),	--事業所コード
			CashAccountCode	nvarchar(3),	--ClosedDateの月までを文字列で抜出
			Jd				nvarchar(6),	--伝票日付の年月まで抜出
			Amount		decimal(10, 0)		--金額
		)

	--データ挿入
	INSERT INTO #temp_Journal_1
	SELECT	
		 OfficeCode
		,CashAccountCode
		,CONVERT(VARCHAR(6), JournalDate, 112)
		,Amount
	FROM
		dbo.Journal
	WHERE
		DelFlag = '0' AND
		AccountType='001' AND
		(
			(JournalType='001' AND Amount >= 0) OR 
			(JournalType='002' and Amount < 0)
		)
	
	-- 入出金テーブル２
		CREATE TABLE #temp_Journal_2
		(
			OfficeCode 		nvarchar(3),	--事業所コード
			CashAccountCode nvarchar(3),	--現金口座コード
			Jd				nvarchar(6),	--伝票日付の年月まで抜出
			TotalAmount		decimal(10, 0),	--総合計		
			K				datetime		--締日(パラメータと比較用)
		)

	--データ挿入
	INSERT INTO #temp_Journal_2
	SELECT	
		 OfficeCode
		,CashAccountCode
		,Jd
		,SUM(ABS(Amount))
		,CONVERT(datetime, Jd + '01', 120)
	FROM
		#temp_Journal_1
	GROUP BY
		OfficeCode, CashAccountCode, Jd
	
	--インデックスの作成
	CREATE INDEX ix_temp_Journal_2 ON #temp_Journal_2(OfficeCode,CashAccountCode, Jd)

	/*-------------------------------------------*/
	/* 一時テーブル(入出金テーブルその２)        */
	/*-------------------------------------------*/
	-- 入出金テーブル３
		CREATE TABLE #temp_Journal_3
		(
			OfficeCode 		nvarchar(3),	--事業所コード
			CashAccountCode	nvarchar(3),	--現金口座コード
			Jd				nvarchar(6),	--伝票日付の年月まで抜出
			Amount		decimal(10, 0)		--金額
		)

	--データ挿入
	INSERT INTO #temp_Journal_3
	SELECT	
		 OfficeCode
		,CashAccountCode
		,CONVERT(VARCHAR(6), JournalDate, 112)
		,Amount
	FROM
		dbo.Journal
	WHERE
		DelFlag = '0' AND
		AccountType='001' AND
		(
			(JournalType='002' AND Amount >= 0) OR 
			(JournalType='001' and Amount < 0)
		)
	
	-- 入出金テーブル４
		CREATE TABLE #temp_Journal_4
		(
			OfficeCode 		nvarchar(3),	--事業所コード
			CashAccountCode nvarchar(3),	--現金口座コード
			Jd				nvarchar(6), 	--伝票日付の年月まで抜出
			TotalAmount		decimal(10, 0),	--総合計		
			K				datetime		--締日(パラメータと比較用)
		)

	--データ挿入
	INSERT INTO #temp_Journal_4
	SELECT	
		 OfficeCode
		,CashAccountCode
		,Jd
		,SUM(ABS(Amount))
		,CONVERT(datetime, Jd + '01', 120)
	FROM
		#temp_Journal_3
	GROUP BY
		OfficeCode, CashAccountCode, Jd
	
	--インデックスの作成
	CREATE INDEX ix_temp_Journal_4 ON #temp_Journal_4(OfficeCode, CashAccountCode, Jd)


	
	/*-------------------------------------------*/
	/* 現金出納帳一覧                            */
	/*-------------------------------------------*/
	SELECT
		  X.Lastdate
		, O.OfficeCode
		, C.CashAccountCode
		, O.OfficeName
		, C.CashAccountName
		, ISNULL(X.TotalAmount, 0) AS LastMonthBalance
		, ISNULL(Y.Totalamount, 0) AS ThisMonthJournal
		, ISNULL(Z.Totalamount, 0) AS ThisMonthPayment
		, ( ISNULL(X.TotalAmount, 0) + ISNULL(Y.Totalamount, 0) - ISNULL(Z.Totalamount, 0)) AS ThisMonthBalance
	FROM
		dbo.CashAccount AS C INNER JOIN
	    dbo.Office AS O ON C.OfficeCode = O.OfficeCode LEFT OUTER JOIN
		    (
		    	SELECT 
					  OfficeCode
					, CashAccountCode
					, Jd
					, Totalamount
					, K
				FROM 
					#temp_Journal_2 AS YY
			 	WHERE 
					YY.K = @TargetDate
			) AS Y ON C.OfficeCode = Y.OfficeCode AND C.CashAccountCode = Y.CashAccountCode LEFT OUTER JOIN
	        (
				SELECT 
					  OfficeCode
					, CashAccountCode
					, Jd
					, Totalamount
					, K
				FROM
					#temp_Journal_4 AS ZZ
				WHERE 
					ZZ.K = @TargetDate
			) AS Z ON C.OfficeCode = Z.OfficeCode AND C.CashAccountCode = Z.CashAccountCode LEFT OUTER JOIN
			(
			SELECT 
				  OfficeCode
				, TotalAmount
				, CashAccountCode
				, M
				, Lastdate
				, K
			FROM 
				#temp_CashBalance_4 AS XX
			WHERE 
				XX.K = @TargetDate
			) AS X ON C.OfficeCode = X.OfficeCode AND C.CashAccountCode = X.CashAccountCode


	WHERE             
		C.DelFlag = '0' AND 
		O.OfficeCode IN 
		(
			SELECT
				OfficeCode
			FROM
				dbo.Department
			WHERE             
				(BusinessType IN ('001', '002'))
		)
	AND 
	(NOT (ISNULL(X.TotalAmount, 0) = 0 AND ISNULL(Y.Totalamount, 0) = 0 AND ISNULL(Z.Totalamount, 0) = 0)) OR
	                        (C.DelFlag = '0') AND (O.OfficeCode IN
	                            (SELECT            OfficeCode
	                               FROM              dbo.Department AS Department_1
	                               WHERE             (BusinessType IN ('001', '002')))) AND (NOT (ISNULL(X.TotalAmount, 0) + ISNULL(Y.Totalamount, 0) - ISNULL(Z.Totalamount, 0) = 0))
	ORDER BY
		  O.OfficeCode
		, C.CashAccountCode
	--PRINT @STRSQL

	BEGIN TRY
		DROP TABLE #temp_CashBalance_1
		DROP TABLE #temp_CashBalance_2
		DROP TABLE #temp_CashBalance_3
		DROP TABLE #temp_CashBalance_4
		DROP TABLE #temp_Journal_1
		DROP TABLE #temp_Journal_2
		DROP TABLE #temp_Journal_3
		DROP TABLE #temp_Journal_4
	END TRY
	BEGIN CATCH
		--無視
	END CATCH
END


GO


