USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCashBalance]    Script Date: 2015/03/31 11:48:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






--Add 2015/03/16 arc yano 現金出納帳エクセル化
CREATE PROCEDURE [dbo].[GetCashBalance]
	 @TargetYear int,						--対象年月(年)
	 @TargetMonth int,						--対象年月(月)
	 @OfficeCode nvarchar(3),				--会社コード
	 @CashAccountCode nvarchar(3)			--現金口座コード
	 	 
AS	
	DECLARE @CNT INT = 0

	--現金在高取得
	DECLARE @targetDate datetime			--対象年月
	DECLARE @tmpTargetDate datetime			--対象年月(作業用)
	DECLARE @preDate datetime				--対象年月の前月
	DECLARE @followDate datetime			--対象年月の次月
	DECLARE @DAYS int						--対象年月の日数


	--対象年月の月初	
	SET @targetDate = CONVERT(datetime, (STR(@TargetYear) + '/' +  STR(@TargetMonth) + '/01'), 120)

	--対象年月の次月の月初
	SET @followDate = DATEADD(m, 1 ,@targetDate)
	
	SET @tmpTargetDate = @targetDate

	--対象年月の日数
	SET @DAYS = DATEDIFF(d, @targetDate, @followDate)

	BEGIN
	
		/*-------------------------------------------*/
		/* 現金在高					　               */
		/*-------------------------------------------*/

		--一時テーブルの保存
		CREATE TABLE #temp_CashBalance (
		  OfficeCode nvarchar(3)
		, ClosedDate datetime
		, CloseFlag nvarchar(2)
		, NumberOf10000 int
		, NumberOf5000 int
		, NumberOf2000 int
		, NumberOf1000 int
		, NumberOf500 int
		, NumberOf100 int
		, NumberOf50 int
		, NumberOf10 int
		, NumberOf5 int
		, NumberOf1 int
		, CheckAmount decimal(10, 0)
		, DelFlag nvarchar(2)
		, CashAccountCode nvarchar(3)
	)

		--対象年月の１ヶ月分の空データを挿入
		WHILE( @CNT < @DAYS)
			BEGIN
				INSERT INTO #temp_CashBalance
				
				SELECT TOP 1
					 @OfficeCode
					,DATEADD(d, @CNT, @tmpTargetDate)
					,'0'
					, 0
					, 0
					, 0
					, 0
					, 0
					, 0
					, 0
					, 0
					, 0
					, 0
					, 0
					, '0'
					,@CashAccountCode
				FROM
					dbo.CashBalance
			
			SET @CNT = @CNT + 1
			END

		--インデックスは不要


		--dbo.CashBalanceの実データと結合する。

		SELECT
			  ISNULL(cb.NumberOf10000, 0) AS NumberOf10000
			, ISNULL(cb.NumberOf5000, 0) AS NumberOf5000
			, ISNULL(cb.NumberOf2000, 0) AS NumberOf2000
			, ISNULL(cb.NumberOf1000, 0) AS NumberOf1000
			, ISNULL(cb.NumberOf500, 0) AS NumberOf500
			, ISNULL(cb.NumberOf100, 0) AS NumberOf100
			, ISNULL(cb.NumberOf50, 0) AS NumberOf50
			, ISNULL(cb.NumberOf10, 0) AS NumberOf10
			, ISNULL(cb.NumberOf5, 0) AS NumberOf5
			, ISNULL(cb.NumberOf1, 0) AS NumberOf1
			, CASE WHEN cb.CloseFlag = '1' THEN '締' ELSE '' END AS ClosedStatus
			, ISNULL(cb.CheckAmount, 0) AS CheckAmount

		FROM
			#temp_CashBalance tcb LEFT OUTER JOIN
			dbo.CashBalance cb ON (tcb.OfficeCode = cb.OfficeCode) AND (tcb.CashAccountCode = cb.CashAccountCode) AND (tcb.ClosedDate = cb.ClosedDate) AND (cb.DelFlag = '0')
		ORDER BY
			tcb.ClosedDate
	END


GO


