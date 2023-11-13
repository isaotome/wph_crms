USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCreditJournal]    Script Date: 2017/03/23 10:57:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




--Create 2016/08/16 arc nakayama #3595_【大項目】車両売掛金機能改善 クレジット入金確認

CREATE PROCEDURE [dbo].[GetCreditJournal]

	@JournalDateFrom nvarchar(10),	 --決済日From
	@JournalDateTo nvarchar(10),	 --決済日To	
	@SalesDateFrom nvarchar(10),	 --納車日From
	@SalesDateTo nvarchar(10),		 --納車日To
	@SlipType nvarchar(3),			 --伝票タイプ
	@SlipNumber nvarchar(50),		 --伝票番号
	@DepartmentCode nvarchar(3),	 --部門コード
	@CustomerCode nvarchar(10),		 --顧客コード
	@CustomerClaimCode nvarchar(10), --請求先コード
	@CompleteFlag nvarchar(2)		 --入金状況

AS	
	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)
	
	/*-------------------------------------------*/
	/* ダーティーリードの設定					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/* 伝票種別文言マスタ						 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_CodeName_014 (
		[Code] NVARCHAR(3)
	,	[Name] NVARCHAR(50)
	)
	INSERT INTO #temp_CodeName_014
	SELECT [Code]
		  ,[Name]
	FROM [c_CodeName]
	WHERE [CategoryCode] = '014'	

	/*-------------------------------------------*/
	/* 入金状況文言マスタ						 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_CodeName_017 (
		[Code] NVARCHAR(3)
	,	[Name] NVARCHAR(50)
	)
	INSERT INTO #temp_CodeName_017
	SELECT [Code]
		  ,[Name]
	FROM [c_CodeName]
	WHERE [CategoryCode] = '017'

	/*--------------------------------------------------------------*/
	/* 車両/サービスの伝票情報取得								 　 */
	/*--------------------------------------------------------------*/
		CREATE TABLE #temp_SlipData (
		  SlipType nvarchar(3) 
		, SlipNumber nvarchar(50)
		, SalesOrderDate datetime
		, SalesDate datetime
		, OrderStatus nvarchar(3)
		, StatusName nvarchar(50)
		)
	
	IF ((@SlipType IS NULL) OR (@SlipType =''))
		BEGIN

			--車両伝票情報取得
			SET @PARAM = '@SalesDateFrom nvarchar(10), @SalesDateTo nvarchar(10)'
			SET @SQL = ''
			SET @SQL = @SQL + 'INSERT INTO #temp_SlipData' + @CRLF
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' 0' + @CRLF
			SET @SQL = @SQL + ',H.SlipNumber' + @CRLF
			SET @SQL = @SQL + ',H.SalesOrderDate' + @CRLF
			SET @SQL = @SQL + ',H.SalesDate' + @CRLF
			SET @SQL = @SQL + ',H.SalesOrderStatus' + @CRLF
			SET @SQL = @SQL + ',CS.Name' + @CRLF
			SET @SQL = @SQL + 'FROM dbo.CarSalesHeader H' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_SalesOrderStatus CS ON CS.Code = H.SalesOrderStatus' + @CRLF
			SET @SQL = @SQL + 'WHERE H.DelFlag = ''0''' + @CRLF

			EXECUTE sp_executeSQL @SQL, @PARAM, @SalesDateFrom, @SalesDateTo

			--サービス伝票情報取得
			SET @PARAM = '@SalesDateFrom nvarchar(10), @SalesDateTo nvarchar(10)'
			SET @SQL = ''
			SET @SQL = @SQL + 'INSERT INTO #temp_SlipData' + @CRLF
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' 1' + @CRLF
			SET @SQL = @SQL + ',SH.SlipNumber' + @CRLF
			SET @SQL = @SQL + ',SH.SalesOrderDate' + @CRLF
			SET @SQL = @SQL + ',SH.SalesDate' + @CRLF
			SET @SQL = @SQL + ',SH.ServiceOrderStatus' + @CRLF
			SET @SQL = @SQL + ',CS.Name' + @CRLF
			SET @SQL = @SQL + 'FROM dbo.ServiceSalesHeader AS SH' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_ServiceOrderStatus CS ON CS.Code = SH.ServiceOrderStatus' + @CRLF
			SET @SQL = @SQL + 'WHERE SH.DelFlag = ''0''' + @CRLF

			EXECUTE sp_executeSQL @SQL, @PARAM, @SalesDateFrom, @SalesDateTo
		END

		IF ((@SlipType IS NOT NULL) AND (@SlipType <>'') AND (@SlipType = '0'))
		BEGIN
			--車両伝票情報取得
			SET @PARAM = '@SalesDateFrom nvarchar(10), @SalesDateTo nvarchar(10)'
			SET @SQL = ''
			SET @SQL = @SQL + 'INSERT INTO #temp_SlipData' + @CRLF
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' 0' + @CRLF
			SET @SQL = @SQL + ',H.SlipNumber' + @CRLF
			SET @SQL = @SQL + ',H.SalesOrderDate' + @CRLF
			SET @SQL = @SQL + ',H.SalesDate' + @CRLF
			SET @SQL = @SQL + ',H.SalesOrderStatus' + @CRLF
			SET @SQL = @SQL + ',CS.Name' + @CRLF
			SET @SQL = @SQL + 'FROM dbo.CarSalesHeader AS H' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_SalesOrderStatus CS ON CS.Code = H.SalesOrderStatus' + @CRLF
			SET @SQL = @SQL + 'WHERE H.DelFlag = ''0''' + @CRLF

			EXECUTE sp_executeSQL @SQL, @PARAM, @SalesDateFrom, @SalesDateTo

		END

		IF ((@SlipType IS NOT NULL) AND (@SlipType <>'') AND (@SlipType = '1'))
		BEGIN
			--サービス伝票情報取得
			SET @PARAM = '@SalesDateFrom nvarchar(10), @SalesDateTo nvarchar(10)'
			SET @SQL = ''
			SET @SQL = @SQL + 'INSERT INTO #temp_SlipData' + @CRLF
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' 1' + @CRLF
			SET @SQL = @SQL + ',SH.SlipNumber' + @CRLF
			SET @SQL = @SQL + ',SH.SalesOrderDate' + @CRLF
			SET @SQL = @SQL + ',SH.SalesDate' + @CRLF
			SET @SQL = @SQL + ',SH.ServiceOrderStatus' + @CRLF
			SET @SQL = @SQL + ',CS.Name' + @CRLF
			SET @SQL = @SQL + 'FROM dbo.ServiceSalesHeader AS SH' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_ServiceOrderStatus CS ON CS.Code = SH.ServiceOrderStatus' + @CRLF
			SET @SQL = @SQL + 'WHERE SH.DelFlag = ''0''' + @CRLF
			

			EXECUTE sp_executeSQL @SQL, @PARAM, @SalesDateFrom, @SalesDateTo
		END

		CREATE INDEX ix_temp_SlipData ON #temp_SlipData(SlipNumber)

	/*---------------------------------------------*/
	/* 決済日からカードの入金(決済)実績を取得する  */
	/*---------------------------------------------*/

	CREATE TABLE #temp_CustomerJournal (
		 SlipNumber nvarchar(50)
	   , DepartmentCode nvarchar(3)
	   , CustomerClaimCode nvarchar(10)
	   , JournalDate datetime
	   , AccountCode nvarchar(50)
	   , Amount decimal(10,0)
	   , CreditReceiptPlanId nvarchar(36)
		)

		SET @PARAM = '@JournalDateFrom nvarchar(10), @JournalDateTo nvarchar(10)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_CustomerJournal' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' J.SlipNumber' + @CRLF
		SET @SQL = @SQL + ',J.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',J.CustomerClaimCode' + @CRLF
		SET @SQL = @SQL + ',J.JournalDate' + @CRLF
		SET @SQL = @SQL + ',J.AccountCode' + @CRLF
		SET @SQL = @SQL + ',J.Amount' + @CRLF
		SET @SQL = @SQL + ',J.CreditReceiptPlanId' + @CRLF
		SET @SQL = @SQL + 'FROM dbo.Journal AS J' + @CRLF
		SET @SQL = @SQL + 'WHERE J.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '	 AND J.AccountType = ''003''' + @CRLF


	--JournalDate
	IF ((@JournalDateFrom is not null) AND (@JournalDateFrom <> '') AND ISDATE(@JournalDateFrom) = 1)
		IF ((@JournalDateTo is not null) AND (@JournalDateTo <> '') AND ISDATE(@JournalDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND J.JournalDate >= @JournalDateFrom AND J.JournalDate < DateAdd(d, 1, @JournalDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND J.JournalDate = @JournalDateFrom' + @CRLF 
		END
	ELSE
		IF ((@JournalDateTo is not null) AND (@JournalDateTo <> '') AND ISDATE(@JournalDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND J.JournalDate < DateAdd(d, 1, @JournalDateTo)' + @CRLF
		END

	EXECUTE sp_executeSQL @SQL, @PARAM, @JournalDateFrom, @JournalDateTo
	CREATE INDEX ix_temp_CustomerJournal ON #temp_CustomerJournal(CreditReceiptPlanId)


	/*--------------------------------------------------------------*/
	/* カード決済した入金実績に紐ずくカード会社からの入金予定取得　 */
	/*--------------------------------------------------------------*/

	CREATE TABLE #temp_CreditReceiptPlan (
		  ReceiptPlanId nvarchar(36)
		, DepartmentCode nvarchar(3)
		, OccurredDepartmentCode nvarchar(3)
		, CustomerClaimCode nvarchar(10)
		, SlipNumber nvarchar(50)
		, Amount decimal(10,0)
		, CompleteFlag nvarchar(2)
		, CreditJournalId nvarchar(36)
		, Summary nvarchar(50)
		)
	
	SET @PARAM = ''
	SET @SQL = ''
	SET @SQL = @SQL + 'INSERT INTO #temp_CreditReceiptPlan' + @CRLF
	SET @SQL = @SQL + 'SELECT' + @CRLF
	SET @SQL = @SQL + '  R.ReceiptPlanId' + @CRLF
	SET @SQL = @SQL + ' ,R.DepartmentCode' + @CRLF
	SET @SQL = @SQL + ' ,R.OccurredDepartmentCode' + @CRLF
	SET @SQL = @SQL + ' ,R.CustomerClaimCode' + @CRLF
	SET @SQL = @SQL + ' ,R.SlipNumber' + @CRLF
	SET @SQL = @SQL + ' ,R.Amount' + @CRLF
	SET @SQL = @SQL + ' ,R.CompleteFlag' + @CRLF
	SET @SQL = @SQL + ' ,R.CreditJournalId' + @CRLF
	SET @SQL = @SQL + ' ,R.Summary' + @CRLF
	SET @SQL = @SQL + 'FROM dbo.ReceiptPlan R' + @CRLF    
	SET @SQL = @SQL + 'WHERE R.DelFlag = ''0''' + @CRLF
	SET @SQL = @SQL + '  AND R.ReceiptType = ''011'' ' + @CRLF --カード会社からの入金

	EXECUTE sp_executeSQL @SQL, @PARAM
	CREATE INDEX ix_temp_CreditReceiptPlan ON #temp_CreditReceiptPlan(ReceiptPlanId)

	/*-------------------------------------------*/
	/* カード会社からの入金実績を取得する		 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_CreditJournal (
		 JournalId nvarchar(36)
	   , SlipNumber nvarchar(50)
	   , DepartmentCode nvarchar(3)
	   , CustomerClaimCode nvarchar(10)
	   , JournalDate datetime
	   , AccountCode nvarchar(50)
	   , Amount decimal(10,0)
	   , CreditReceiptPlanId nvarchar(36)
		)

		SET @PARAM = ''
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_CreditJournal' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' J.JournalId' + @CRLF
		SET @SQL = @SQL + ',J.SlipNumber' + @CRLF
		SET @SQL = @SQL + ',J.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',J.CustomerClaimCode' + @CRLF
		SET @SQL = @SQL + ',J.JournalDate' + @CRLF
		SET @SQL = @SQL + ',J.AccountCode' + @CRLF
		SET @SQL = @SQL + ',J.Amount' + @CRLF
		SET @SQL = @SQL + ',J.CreditReceiptPlanId' + @CRLF
		SET @SQL = @SQL + 'FROM dbo.Journal AS J' + @CRLF
		SET @SQL = @SQL + 'WHERE J.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '	 AND J.AccountType = ''011''' + @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM
		CREATE INDEX ix_temp_CreditJournal ON #temp_CreditJournal(CreditReceiptPlanId)


	/*-------------------------------------------*/
	/* カード入金状況検索						 */
	/*-------------------------------------------*/

	SET @PARAM = '@JournalDateFrom nvarchar(10), @JournalDateTo nvarchar(10), @SalesDateFrom nvarchar(10), @SalesDateTo nvarchar(10), @SlipType nvarchar(3), @SlipNumber nvarchar(50), @DepartmentCode nvarchar(3), @CustomerCode nvarchar(10), @CustomerClaimCode nvarchar(10), @CompleteFlag nvarchar(2)'

	SET @SQL = ''
	SET @SQL = @SQL + 'SELECT'+ @CRLF
	SET @SQL = @SQL + ' C14.Name AS SlipTypeName' + @CRLF		--伝票タイプ
	SET @SQL = @SQL + ',CR.SlipNumber AS SlipNumber' + @CRLF	--伝票番号
	SET @SQL = @SQL + ',S.StatusName AS StatusName' + @CRLF	--伝票ステータス
	SET @SQL = @SQL + ',CR.OccurredDepartmentCode AS OccurredDepartmentCode' + @CRLF	--部門コード
	SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF	--部門名
	SET @SQL = @SQL + ',S.SalesOrderDate AS SalesOrderDate' + @CRLF	--受注日
	SET @SQL = @SQL + ',S.SalesDate AS SalesDate' + @CRLF	--納車日
	SET @SQL = @SQL + ',J.CustomerClaimCode AS CustomerCode' + @CRLF	--顧客コード
	SET @SQL = @SQL + ',CC.CustomerClaimName AS CustomerName' + @CRLF	--顧客名
	SET @SQL = @SQL + ',J.JournalDate AS JournalDate' + @CRLF	--決済日
	SET @SQL = @SQL + ',J.AccountCode AS AccountCode' + @CRLF	--科目コード
	SET @SQL = @SQL + ',A.AccountName AS AccountName' + @CRLF	--科目名
	SET @SQL = @SQL + ',CR.CustomerClaimCode AS CustomerClaimCode' + @CRLF	--請求先コード
	SET @SQL = @SQL + ',CC2.CustomerClaimName AS CustomerClaimName' + @CRLF	--請求先名
	SET @SQL = @SQL + ',J.Amount AS Amount' + @CRLF	--決済金額
	SET @SQL = @SQL + ',CASE WHEN C17.Name IS NULL THEN ''未入金'' ELSE C17.Name END AS CompleteFlagName' + @CRLF	--入金状況
	SET @SQL = @SQL + ',CR.Summary AS Summary' + @CRLF	--摘要
	SET @SQL = @SQL + 'FROM #temp_CustomerJournal J' + @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CreditReceiptPlan AS CR ON CR.ReceiptPlanId = J.CreditReceiptPlanId' + @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CreditJournal AS CJ ON CJ.JournalId = CR.CreditJournalId' + @CRLF
	SET @SQL = @SQL + 'INNER JOIN #temp_SlipData AS S ON S.SlipNumber = CR.SlipNumber' + @CRLF
	
	SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CodeName_014 AS C14 ON C14.Code = S.SlipType' + @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CodeName_017 AS C17 ON C17.Code = CR.CompleteFlag' + @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = CR.OccurredDepartmentCode' + @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.CustomerClaim CC ON CC.CustomerClaimCode = J.CustomerClaimCode' + @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.CustomerClaim CC2 ON CC2.CustomerClaimCode = CR.CustomerClaimCode' + @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN (select AccountCode, AccountName from dbo.Account where DelFlag = ''0'') AS A ON A.AccountCode = J.AccountCode' + @CRLF

	SET @SQL = @SQL + 'WHERE 1 = 1'+ @CRLF

	--伝票種別
	IF ((@SlipType IS NOT  NULL) AND (@SlipType <>''))
		BEGIN
			SET @SQL = @SQL + 'AND S.SlipType = @SlipType'+ @CRLF
		END
	
	--伝票番号
	IF((@SlipNumber IS NOT NULL) AND (@SlipNumber <>''))
	BEGIN
		SET @SQL = @SQL +'AND CR.SlipNumber = @SlipNumber' + @CRLF
	END

	--部門コード
	IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))
	BEGIN
		SET @SQL = @SQL + 'AND CR.OccurredDepartmentCode = @DepartmentCode'+ @CRLF
	END

	--納車日
	IF ((@SalesDateFrom is not null) AND (@SalesDateFrom <> '') AND ISDATE(@SalesDateFrom) = 1)
		IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.SalesDate >= @SalesDateFrom AND S.SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND S.SalesDate = @SalesDateFrom' + @CRLF 
		END
	ELSE
		IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF
		END

	--顧客コード
	IF ((@CustomerCode IS NOT NULL) AND (@CustomerCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND J.CustomerClaimCode = @CustomerCode'+ @CRLF
		END
	--請求先コード
	IF ((@CustomerClaimCode IS NOT NULL) AND (@CustomerClaimCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND CR.CustomerClaimCode = @CustomerClaimCode'+ @CRLF
		END
	--入金完了フラグ
	IF ((@CompleteFlag IS NOT NULL) AND (@CompleteFlag <>''))
		BEGIN
			SET @SQL = @SQL + 'AND CR.CompleteFlag = @CompleteFlag'+ @CRLF
		END

	SET @SQL = @SQL + 'ORDER BY J.JournalDate, CR.OccurredDepartmentCode, CR.SlipNumber'+ @CRLF
	
	EXECUTE sp_executeSQL @SQL, @PARAM, @JournalDateFrom, @JournalDateTo, @SalesDateFrom, @SalesDateTo, @SlipType, @SlipNumber, @DepartmentCode, @CustomerCode, @CustomerClaimCode, @CompleteFlag


BEGIN

	BEGIN TRY
		DROP TABLE #temp_CodeName_014
		DROP TABLE #temp_CodeName_017
		DROP TABLE #temp_CreditJournal
		DROP TABLE #temp_CreditReceiptPlan
		DROP TABLE #temp_CustomerJournal
		DROP TABLE #temp_SlipData
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

END




GO


