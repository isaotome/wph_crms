USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[CreateBackAmountAkaden]    Script Date: 2016/07/01 16:57:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--‹@”\FÔ“`•[ì¬ŒãAŒ³‚Æ‚Æ‚È‚Á‚½“`•[‚Æì¬‚³‚ê‚½Ô“`‚Ì“ü‹à—\’è‚Ì·Šz‚ğo‚µ‚ÄA·Šz‚ª‚O‚Å‚È‚¯‚ê‚Î·Šz•ª‚Ì“ü‹à—\’è‚ğì¬‚·‚é	
--2016/05/26 arc nakayama #3418_Ô•“`•[”­s‚Ì•“`•[‚Ì“ü‹à—\’èiReceiptPlanj‚Ìc‚‚ÌŒvZ•û–@  V‹Kì¬
--2016/07/01 arc nakayama #3593_“`•[‚É‘Î‚µ‚Ä“¯ˆê¿‹æ‚ª•¡”‚ ‚Á‚½ê‡‚Ìl—¶
CREATE PROCEDURE [dbo].[CreateBackAmountAkaden]

	@SlipNumber nvarchar(50),	--“`•[”Ô†
	@EmployeeCode nvarchar(50)	--ĞˆõƒR[ƒh
AS
	SET NOCOUNT ON
	
	/*-------------------------------------------*/
	/* ƒ_[ƒeƒB[ƒŠ[ƒh‚Ìİ’è					 */
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
		--–³‹
	END CATCH

	/*---------------------------------------------------------------*/
	/* “`•[”Ô†‚ğƒL[‚É‚µ‚ÄŒ³“`•[‚Ì“ü‹à—\’è‚ğ¿‹æ•Ê‚Éæ“¾			 */
	/*---------------------------------------------------------------*/
	BEGIN TRY
		CREATE TABLE #temp_ReceiptPlan(
			  SlipNumber nvarchar(50)
			, CustomerClaimCode nvarchar(10)
			, Amount decimal(10,0)
			, ReceivableBalance decimal(10,0)
		)

		INSERT INTO #temp_ReceiptPlan
		SELECT rp.[SlipNumber]							--“`•[”Ô†
			  ,rp.[CustomerClaimCode]					--¿‹æƒR[ƒh
			  ,SUM(ISNULL(rp.Amount, 0)) AS [Amount]	--‹àŠz‡Œv(ƒTƒ}ƒŠ)
			  ,SUM(ISNULL(rp.ReceivableBalance, 0))		--c‚iƒTƒ}ƒŠj
		FROM [dbo].[ReceiptPlan] rp
		WHERE rp.[SlipNumber] = @SlipNumber
		  AND rp.[CustomerClaimCode] is not NULL
		  AND rp.[CustomerClaimCode] <> ''
		  AND rp.[DelFlag] = '0'
		GROUP BY  rp.[SlipNumber], rp.[CustomerClaimCode]


		/*-----------------------------------------------------------*/
		/* “`•[”Ô†‚ğƒL[‚É‚µ‚ÄÔ“`•[‚Ì“ü‹à—\’è‚ğ¿‹æ•Ê‚Éæ“¾		 */
		/*-----------------------------------------------------------*/

		CREATE TABLE #temp_AkaReceiptPlan(
			  SlipNumber nvarchar(50)
			, CustomerClaimCode nvarchar(10)
			, Amount decimal(10,0)
			, ReceivableBalance decimal(10,0)
		)

		INSERT INTO #temp_AkaReceiptPlan
		SELECT rp.[SlipNumber]							--“`•[”Ô†
			  ,rp.[CustomerClaimCode]					--¿‹æƒR[ƒh
			  ,SUM(ISNULL(rp.Amount, 0)) AS [Amount]	--‹àŠz‡Œv(ƒTƒ}ƒŠ)
			  ,SUM(ISNULL(rp.ReceivableBalance, 0))		--c‚iƒTƒ}ƒŠj
		FROM [dbo].[ReceiptPlan] rp 
		WHERE rp.[SlipNumber] = @SlipNumber + '-1'		--Ô“`‚Ì”Ô†
		  AND rp.[CustomerClaimCode] is not NULL
		  AND rp.[CustomerClaimCode] <> ''
		  AND rp.[DelFlag] = '0'
		GROUP BY rp.[SlipNumber], rp.[CustomerClaimCode]


		/*-------------------------------------------------------------------*/
		/* Œ³“`•[‚Ì‹àŠz‚©‚çÔ“`•[‚Ì‹àŠz‚ğˆø‚¢‚½’l‚Å·Šz‚ğ‹‚ß‚é				 */
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
		/* •Ô‹à•ª‚Ì“ü‹à—\’è‚ğì¬‚·‚é		 */
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
			   ,'0' AS CompleteFlag		-- 0ŒÅ’è
			   ,@EmployeeCode AS CreateEmployeeCode
			   ,GETDATE() AS CreateDate
			   ,@EmployeeCode AS LastUpdateEmployeeCode
			   ,GETDATE() AS LastUpdateDate
			   ,'0' AS DelFlag
			   ,'“`•[”Ô†' + REPLACE(np.SlipNumber, '-1', '') + '‚ÌÔ“`ˆ—•ª' AS Summary
			   ,null --r.[JournalDate]
			   ,'0' --[DepositFlag] ””ï—p‚à•ª‚à‚Ü‚Æ‚ß‚Ä•Ô‹à‚Ì—\’è‚É‚·‚é‚½‚ß0ŒÅ’è
			   ,''--r.[PaymentKindCode]
			   ,null --r.[CommissionRate]
			   ,null --r.[CommissionAmount]
			   ,''

		FROM #temp_NewReceiptPlan np
		INNER JOIN  #temp_ReceiptPlan2 r on r.SlipNumber = np.SlipNumber and r.CustomerClaimCode = np.CustomerClaimCode
		--INNER JOIN (select TOP 1 rp.* FROM WPH_DB.dbo.ReceiptPlan rp where rp.DelFlag = '0') as r on r.SlipNumber = np.SlipNumber and r.CustomerClaimCode = np.CustomerClaimCode
 		WHERE np.ReceivableBalance != 0 --c‚‚ª‚OˆÈŠO‚Ì‚à‚Ì
	END TRY

	BEGIN CATCH
		RETURN ERROR_NUMBER()
	END CATCH

	RETURN 0
END
GO


