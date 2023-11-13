USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[CreateBackAmountAkaden]    Script Date: 2016/07/01 16:57:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--機能：赤伝票作成後、元ととなった伝票と作成された赤伝の入金予定の差額を出して、差額が０でなければ差額分の入金予定を作成する	
--2016/05/26 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法  新規作成
--2016/07/01 arc nakayama #3593_伝票に対して同一請求先が複数あった場合の考慮
CREATE PROCEDURE [dbo].[CreateBackAmountAkaden]

	@SlipNumber nvarchar(50),	--伝票番号
	@EmployeeCode nvarchar(50)	--社員コード
AS
	SET NOCOUNT ON
	
	/*-------------------------------------------*/
	/* ダーティーリードの設定					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;


	BEGIN

	BEGIN TRY
		DROP TABLE #temp_ReceiptPlan
		DROP TABLE #temp_ReceiptPlan2
		DROP TABLE #temp_AkaReceiptPlan
		DROP TABLE #temp_NewReceiptPlan
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

	/*---------------------------------------------------------------*/
	/* 伝票番号をキーにして元伝票の入金予定を請求先別に取得			 */
	/*---------------------------------------------------------------*/
	BEGIN TRY
		CREATE TABLE #temp_ReceiptPlan(
			  SlipNumber nvarchar(50)
			, CustomerClaimCode nvarchar(10)
			, Amount decimal(10,0)
			, ReceivableBalance decimal(10,0)
		)

		INSERT INTO #temp_ReceiptPlan
		SELECT rp.[SlipNumber]							--伝票番号
			  ,rp.[CustomerClaimCode]					--請求先コード
			  ,SUM(ISNULL(rp.Amount, 0)) AS [Amount]	--金額合計(サマリ)
			  ,SUM(ISNULL(rp.ReceivableBalance, 0))		--残高（サマリ）
		FROM [dbo].[ReceiptPlan] rp
		WHERE rp.[SlipNumber] = @SlipNumber
		  AND rp.[CustomerClaimCode] is not NULL
		  AND rp.[CustomerClaimCode] <> ''
		  AND rp.[DelFlag] = '0'
		GROUP BY  rp.[SlipNumber], rp.[CustomerClaimCode]


		/*-----------------------------------------------------------*/
		/* 伝票番号をキーにして赤伝票の入金予定を請求先別に取得		 */
		/*-----------------------------------------------------------*/

		CREATE TABLE #temp_AkaReceiptPlan(
			  SlipNumber nvarchar(50)
			, CustomerClaimCode nvarchar(10)
			, Amount decimal(10,0)
			, ReceivableBalance decimal(10,0)
		)

		INSERT INTO #temp_AkaReceiptPlan
		SELECT rp.[SlipNumber]							--伝票番号
			  ,rp.[CustomerClaimCode]					--請求先コード
			  ,SUM(ISNULL(rp.Amount, 0)) AS [Amount]	--金額合計(サマリ)
			  ,SUM(ISNULL(rp.ReceivableBalance, 0))		--残高（サマリ）
		FROM [dbo].[ReceiptPlan] rp 
		WHERE rp.[SlipNumber] = @SlipNumber + '-1'		--赤伝の番号
		  AND rp.[CustomerClaimCode] is not NULL
		  AND rp.[CustomerClaimCode] <> ''
		  AND rp.[DelFlag] = '0'
		GROUP BY rp.[SlipNumber], rp.[CustomerClaimCode]


		/*-------------------------------------------------------------------*/
		/* 元伝票の金額から赤伝票の金額を引いた値で差額を求める				 */
		/*-------------------------------------------------------------------*/
		CREATE TABLE #temp_NewReceiptPlan(
			  SlipNumber nvarchar(50)
			, CustomerClaimCode nvarchar(10)
			, Amount decimal(10,0)
			, ReceivableBalance decimal(10,0)
		)

		INSERT INTO #temp_NewReceiptPlan
		SELECT rp.SlipNumber
			  ,rp.CustomerClaimCode
			  ,rp.Amount + (ap.Amount)
			  ,rp.ReceivableBalance + (ap.ReceivableBalance)

		FROM #temp_ReceiptPlan rp
		INNER JOIN #temp_AkaReceiptPlan ap on ap.CustomerClaimCode = rp.CustomerClaimCode


		/*-----------------------------------*/
		/* 返金分の入金予定を作成する		 */
		/*-----------------------------------*/


		CREATE TABLE #temp_ReceiptPlan2(
			  DepartmentCode nvarchar(3)
			, OccurredDepartmentCode nvarchar(3)
			, SlipNumber nvarchar(50)
			, CustomerClaimCode nvarchar(10)
			, ReceiptType nvarchar(3)
			, AccountCode nvarchar(50)
		)


		INSERT INTO #temp_ReceiptPlan2
		SELECT r.DepartmentCode
			 , r.OccurredDepartmentCode
			 , r.SlipNumber
			 , r.CustomerClaimCode
			 , r.ReceiptType
			 , r.AccountCode
		FROM WPH_DB.dbo.ReceiptPlan r
		WHERE r.DelFlag = '0'
		GROUP BY r.DepartmentCode, r.OccurredDepartmentCode, r.CustomerClaimCode, r.SlipNumber, r.ReceiptType, r.AccountCode




		INSERT INTO WPH_DB.dbo.ReceiptPlan
		SELECT NEWID() AS ReceiptPlanId
			   ,r.[DepartmentCode]
			   ,r.[OccurredDepartmentCode]
			   ,np.[CustomerClaimCode]
			   ,np.[SlipNumber]
			   ,r.[ReceiptType]
			   ,null --r.[ReceiptPlanDate]
			   ,r.[AccountCode]
			   ,np.[Amount]
			   ,np.[ReceivableBalance]
			   ,'0' AS CompleteFlag		-- 0固定
			   ,@EmployeeCode AS CreateEmployeeCode
			   ,GETDATE() AS CreateDate
			   ,@EmployeeCode AS LastUpdateEmployeeCode
			   ,GETDATE() AS LastUpdateDate
			   ,'0' AS DelFlag
			   ,'伝票番号' + REPLACE(np.SlipNumber, '-1', '') + 'の赤伝処理分' AS Summary
			   ,null --r.[JournalDate]
			   ,'0' --[DepositFlag] 諸費用も分もまとめて返金の予定にするため0固定
			   ,''--r.[PaymentKindCode]
			   ,null --r.[CommissionRate]
			   ,null --r.[CommissionAmount]
			   ,''

		FROM #temp_NewReceiptPlan np
		INNER JOIN  #temp_ReceiptPlan2 r on r.SlipNumber = np.SlipNumber and r.CustomerClaimCode = np.CustomerClaimCode
		--INNER JOIN (select TOP 1 rp.* FROM WPH_DB.dbo.ReceiptPlan rp where rp.DelFlag = '0') as r on r.SlipNumber = np.SlipNumber and r.CustomerClaimCode = np.CustomerClaimCode
 		WHERE np.ReceivableBalance != 0 --残高が０以外のもの
	END TRY

	BEGIN CATCH
		RETURN ERROR_NUMBER()
	END CATCH

	RETURN 0
END
GO


