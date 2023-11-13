USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetSummaryReceiptPlan]    Script Date: 2017/03/08 17:54:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




--Create 2016/07/19 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）

CREATE PROCEDURE [dbo].[GetSummaryReceiptPlan]

	@officeCode nvarchar(3),			--事業所コード
	@salesDateFrom nvarchar(10),		--納車日From
	@salesDateTo nvarchar(10),			--納車日To
	@journalDateFrom nvarchar(10),		--決済日From
	@journalDateTo nvarchar(10),		--決済日To
	@receiptPlanDateFrom nvarchar(10),	--入金予定日From
    @receiptPlanDateTo nvarchar(10),	--入金予定日To
	@slipNumber nvarchar(50),			--伝票番号
	@customerClaimCode nvarchar(10),	--請求先コード
	@customerClaimType nvarchar(10),	--請求先種別
	@accountUsageType varchar(3),		--営業・サービス（CR：車両　SR：サービス）
	@receiptType varchar(3),			--入金種別
	@paymentKindCode nvarchar(10),		--決済種別
	@customerClaimFilter nvarchar(3),	--検索フィルター
	@isShopDeposit bit					--店舗入金消込画面かどうか

AS
	SET NOCOUNT ON

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)
	
	/*-------------------------------------------*/
	/* ダーティーリードの設定					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*----------------------------------------------------------------------------------------------------*/
	/* 車両だった場合は車両伝票、サービスだった場合はサービス伝票、何も指定がなければ両方　の伝票情報取得 */
	/*----------------------------------------------------------------------------------------------------*/
	CREATE TABLE #temp_SlipData_S (
		SlipNumber nvarchar(50),
		SalesDate datetime,
		SalesPlanDate datetime,
		accountUsageType varchar(3)
		)
	
	--▼車両伝票情報のみ取得
	IF (@accountUsageType = 'CR')
	BEGIN

		SET @PARAM = '@salesDateFrom nvarchar(10), @salesDateTo nvarchar(10), @accountUsageType varchar(3)'

		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #temp_SlipData_S' + @CRLF
		SET @SQL = @SQL +'SELECT' + @CRLF
		SET @SQL = @SQL +'	  SlipNumber' + @CRLF
		SET @SQL = @SQL +'	, SalesDate' + @CRLF
		SET @SQL = @SQL +'	, SalesPlanDate' + @CRLF
		SET @SQL = @SQL +'	, ''CR''' + @CRLF
		SET @SQL = @SQL +'FROM' + @CRLF
		SET @SQL = @SQL +'	dbo.CarSalesHeader' + @CRLF
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    DelFlag = ''0''' + @CRLF 

		--納車日 SalesDate
		IF ((@salesDateFrom is not null) AND (@salesDateFrom <> '') AND ISDATE(@salesDateFrom) = 1)
			IF ((@salesDateTo is not null) AND (@salesDateTo <> '') AND ISDATE(@salesDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND SalesDate >= @salesDateFrom AND SalesDate < DateAdd(d, 1, @salesDateTo)' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND SalesDate = @salesDateFrom' + @CRLF 
			END
		ELSE
			IF ((@salesDateTo is not null) AND (@salesDateTo <> '') AND ISDATE(@salesDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND SalesDate < DateAdd(d, 1, @salesDateTo)' + @CRLF 
			END

		EXECUTE sp_executeSQL @SQL, @PARAM,@salesDateFrom, @salesDateTo, @accountUsageType

	END
	
	--▼サービス伝票情報のみ取得
	IF (@accountUsageType = 'SR')
	BEGIN
		
		SET @PARAM = '@salesDateFrom nvarchar(10), @salesDateTo nvarchar(10), @accountUsageType varchar(3)'

		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #temp_SlipData_S' + @CRLF
		SET @SQL = @SQL +'SELECT' + @CRLF
		SET @SQL = @SQL +'	  SlipNumber' + @CRLF
		SET @SQL = @SQL +'	, SalesDate' + @CRLF
		SET @SQL = @SQL +'	, SalesPlanDate' + @CRLF
		SET @SQL = @SQL +'	, ''SR''' + @CRLF
		SET @SQL = @SQL +'FROM' + @CRLF
		SET @SQL = @SQL +'	dbo.ServiceSalesHeader' + @CRLF
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    DelFlag = ''0''' + @CRLF 

		--納車日 SalesDate
		IF ((@salesDateFrom is not null) AND (@salesDateFrom <> '') AND ISDATE(@salesDateFrom) = 1)
			IF ((@salesDateTo is not null) AND (@salesDateTo <> '') AND ISDATE(@salesDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND SalesDate >= @salesDateFrom AND SalesDate < DateAdd(d, 1, @salesDateTo)' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND SalesDate = @salesDateFrom' + @CRLF 
			END
		ELSE
			IF ((@salesDateTo is not null) AND (@salesDateTo <> '') AND ISDATE(@salesDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND SalesDate < DateAdd(d, 1, @salesDateTo)' + @CRLF 
			END

		EXECUTE sp_executeSQL @SQL, @PARAM,@salesDateFrom, @salesDateTo, @accountUsageType

	END

	--車両・サービス　両方取得
	IF ((@accountUsageType is null) OR (@accountUsageType = ''))
	BEGIN
		
		--車両
		SET @PARAM = '@salesDateFrom nvarchar(10), @salesDateTo nvarchar(10), @accountUsageType varchar(3)'

		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #temp_SlipData_S' + @CRLF
		SET @SQL = @SQL +'SELECT' + @CRLF
		SET @SQL = @SQL +'	  SlipNumber' + @CRLF
		SET @SQL = @SQL +'	, SalesDate' + @CRLF
		SET @SQL = @SQL +'	, SalesPlanDate' + @CRLF
		SET @SQL = @SQL +'	, ''CR''' + @CRLF
		SET @SQL = @SQL +'FROM' + @CRLF
		SET @SQL = @SQL +'	dbo.CarSalesHeader' + @CRLF
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    DelFlag = ''0''' + @CRLF 

		--納車日 SalesDate
		IF ((@salesDateFrom is not null) AND (@salesDateFrom <> '') AND ISDATE(@salesDateFrom) = 1)
			IF ((@salesDateTo is not null) AND (@salesDateTo <> '') AND ISDATE(@salesDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND SalesDate >= @salesDateFrom AND SalesDate < DateAdd(d, 1, @salesDateTo)' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND SalesDate = @salesDateFrom' + @CRLF 
			END
		ELSE
			IF ((@salesDateTo is not null) AND (@salesDateTo <> '') AND ISDATE(@salesDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND SalesDate < DateAdd(d, 1, @salesDateTo)' + @CRLF 
			END

		EXECUTE sp_executeSQL @SQL, @PARAM,@salesDateFrom, @salesDateTo, @accountUsageType

		
		--サービス
		SET @PARAM = '@salesDateFrom nvarchar(10), @salesDateTo nvarchar(10), @accountUsageType varchar(3)'

		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #temp_SlipData_S' + @CRLF
		SET @SQL = @SQL +'SELECT' + @CRLF
		SET @SQL = @SQL +'	  SlipNumber' + @CRLF
		SET @SQL = @SQL +'	, SalesDate' + @CRLF
		SET @SQL = @SQL +'	, SalesPlanDate' + @CRLF
		SET @SQL = @SQL +'	, ''SR''' + @CRLF
		SET @SQL = @SQL +'FROM' + @CRLF
		SET @SQL = @SQL +'	dbo.ServiceSalesHeader' + @CRLF
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    DelFlag = ''0''' + @CRLF 

		--納車日 SalesDate
		IF ((@salesDateFrom is not null) AND (@salesDateFrom <> '') AND ISDATE(@salesDateFrom) = 1)
			IF ((@salesDateTo is not null) AND (@salesDateTo <> '') AND ISDATE(@salesDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND SalesDate >= @salesDateFrom AND SalesDate < DateAdd(d, 1, @salesDateTo)' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND SalesDate = @salesDateFrom' + @CRLF 
			END
		ELSE
			IF ((@salesDateTo is not null) AND (@salesDateTo <> '') AND ISDATE(@salesDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND SalesDate < DateAdd(d, 1, @salesDateTo)' + @CRLF 
			END

		EXECUTE sp_executeSQL @SQL, @PARAM,@salesDateFrom, @salesDateTo, @accountUsageType

	END	

	CREATE INDEX ix_temp_SlipData_S ON #temp_SlipData_S(SlipNumber)

	--請求先区分・検索フィルターで請求先情報を絞り込む
	CREATE TABLE #temp_CustomerClaimType_CT (
		Code varchar(3),
		CustomerClaimFilter nvarchar(3)
		)

	SET @PARAM = '@customerClaimType nvarchar(10), @customerClaimFilter nvarchar(3)'

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_CustomerClaimType_CT' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  Code' + @CRLF
	SET @SQL = @SQL +'	, CustomerClaimFilter' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.c_CustomerClaimType' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    DelFlag = ''0''' + @CRLF 

	--検索フィルター
	IF ((@customerClaimFilter IS NOT NULL) AND (@customerClaimFilter <>''))
	BEGIN
		SET @SQL = @SQL + 'AND CustomerClaimFilter = @customerClaimFilter'+ @CRLF
	END

	--請求先区分
	IF ((@customerClaimType IS NOT NULL) AND (@customerClaimType <>''))
	BEGIN
		SET @SQL = @SQL + 'AND Code = @customerClaimType'+ @CRLF
	END

	EXECUTE sp_executeSQL @SQL, @PARAM, @customerClaimType, @customerClaimFilter
	CREATE INDEX ix_temp_CustomerClaimType_CT ON #temp_CustomerClaimType_CT(Code)

	--結果を格納するテーブル
	CREATE TABLE #temp_ReceiptPlanResult (
		ReceiptPlanId nvarchar(36),
		OfficeCode nvarchar(3),
		OfficeName nvarchar(20),
		DepartmentCode nvarchar(3),
		OccurredDepartmentCode nvarchar(3),
		CustomerClaimCode nvarchar(10),
		CustomerClaimName nvarchar(80),
		SlipNumber nvarchar(50),
		ReceiptType nvarchar(3),
		ReceiptPlanDate datetime,
		AccountCode nvarchar(50),
		Amount decimal(10,0),
		ReceivableBalance decimal(10,0),
		Summary	nvarchar(50),
		JournalDate datetime,
		PaymentKindCode nvarchar(10),
		CommissionRate decimal(8,5),
		CommissionAmount decimal(10,0),
		CreditJournalId nvarchar(36),
		SalesDate datetime,
		SalesPlanDate datetime,
		accountUsageType varchar(3)
		)

	/*■■■■■■■■■■■■■■■■■■■■■■*/
	/*											  */
	/* カード会社からの入金・ローンの予定を取得	  */
	/*											  */
	/*■■■■■■■■■■■■■■■■■■■■■■*/

	IF(@isShopDeposit = '0')
	BEGIN

		SET @PARAM = '@officeCode nvarchar(3), @journalDateFrom nvarchar(10), @journalDateTo nvarchar(10), @receiptPlanDateFrom nvarchar(10), @receiptPlanDateTo nvarchar(10), @slipNumber nvarchar(50), @customerClaimCode nvarchar(10), @receiptType varchar(3), @paymentKindCode nvarchar(10)'

		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #temp_ReceiptPlanResult' + @CRLF
		SET @SQL = @SQL + 'SELECT'+ @CRLF
		SET @SQL = @SQL + '   R.ReceiptPlanId'+ @CRLF
		SET @SQL = @SQL + ' , D.OfficeCode'+ @CRLF
		SET @SQL = @SQL + ' , o.OfficeName'+ @CRLF
		SET @SQL = @SQL + ' , R.DepartmentCode'+ @CRLF
		SET @SQL = @SQL + ' , R.OccurredDepartmentCode'+ @CRLF
		SET @SQL = @SQL + ' , R.CustomerClaimCode'+ @CRLF
		SET @SQL = @SQL + ' , CC.CustomerClaimName'+ @CRLF
		SET @SQL = @SQL + ' , R.SlipNumber'+ @CRLF
		SET @SQL = @SQL + ' , R.ReceiptType'+ @CRLF
		SET @SQL = @SQL + ' , R.ReceiptPlanDate'+ @CRLF
		SET @SQL = @SQL + ' , R.AccountCode'+ @CRLF
		SET @SQL = @SQL + ' , R.Amount'+ @CRLF
		SET @SQL = @SQL + ' , R.ReceivableBalance'+ @CRLF
		SET @SQL = @SQL + ' , R.Summary'+ @CRLF
		SET @SQL = @SQL + ' , R.JournalDate'+ @CRLF
		SET @SQL = @SQL + ' , R.PaymentKindCode'+ @CRLF
		SET @SQL = @SQL + ' , R.CommissionRate'+ @CRLF
		SET @SQL = @SQL + ' , R.CommissionAmount'+ @CRLF
		SET @SQL = @SQL + ' , R.CreditJournalId'+ @CRLF
		SET @SQL = @SQL + ' , S.SalesDate'+ @CRLF
		SET @SQL = @SQL + ' , S.SalesPlanDate'+ @CRLF
		SET @SQL = @SQL + ' , S.accountUsageType'+ @CRLF

		SET @SQL = @SQL + 'FROM dbo.ReceiptPlan AS R'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = R.OccurredDepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.CustomerClaim AS CC ON CC.CustomerClaimCode = R.CustomerClaimCode'+ @CRLF
		SET @SQL = @SQL + 'INNER JOIN #temp_SlipData_S AS S ON S.SlipNumber = R.SlipNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Office AS o ON o.OfficeCode = D.OfficeCode'+ @CRLF

		SET @SQL = @SQL + 'WHERE'+ @CRLF
		SET @SQL = @SQL + '	   R.DelFlag = ''0'''+ @CRLF
		SET @SQL = @SQL + 'AND R.CompleteFlag  = ''0'''+ @CRLF
		SET @SQL = @SQL + 'AND R.ReceiptType IN (''011'',''004'', ''012'')'+ @CRLF
		SET @SQL = @SQL + 'AND EXISTS(SELECT 1 FROM #temp_CustomerClaimType_CT AS CT WHERE CT.Code = CC.CustomerClaimType)'+ @CRLF
	
		--伝票番号
		IF ((@SlipNumber IS NOT NULL) AND (@SlipNumber <> ''))
		BEGIN
			SET @SQL = @SQL +'AND R.SlipNumber LIKE ''%' + @SlipNumber + '%''' + @CRLF
		END

		--事業所コード
		IF ((@officeCode IS NOT NULL) AND (@officeCode <>''))
			BEGIN
				SET @SQL = @SQL + 'AND D.OfficeCode = @officeCode'+ @CRLF
			END

		--請求先コード
		IF ((@customerClaimCode IS NOT NULL) AND (@customerClaimCode <>''))
			BEGIN
				SET @SQL = @SQL + 'AND R.CustomerClaimCode = @customerClaimCode'+ @CRLF
			END
	
		--入金種別
		IF ((@receiptType IS NOT NULL) AND (@receiptType <>''))
			BEGIN
				SET @SQL = @SQL + 'AND R.ReceiptType = @receiptType'+ @CRLF
			END
	
		--決済種別
		IF ((@paymentKindCode IS NOT NULL) AND (@paymentKindCode <>''))
			BEGIN
				SET @SQL = @SQL + 'AND R.PaymentKindCode = @paymentKindCode'+ @CRLF
			END

		--決済日
		IF ((@journalDateFrom is not null) AND (@journalDateFrom <> '') AND ISDATE(@journalDateFrom) = 1)
			IF ((@journalDateTo is not null) AND (@journalDateTo <> '') AND ISDATE(@journalDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND R.journalDateDate >= @journalDateFrom AND R.journalDate < DateAdd(d, 1, @RjournalDateTo)' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND R.journalDate = @journalDateFrom' + @CRLF 
			END
		ELSE
			IF ((@journalDateTo is not null) AND (@journalDateTo <> '') AND ISDATE(@journalDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND R.journalDate < DateAdd(d, 1, @journalDateTo)' + @CRLF 
			END

		--入金予定日
		IF ((@receiptPlanDateFrom is not null) AND (@receiptPlanDateFrom <> '') AND ISDATE(@receiptPlanDateFrom) = 1)
			IF ((@receiptPlanDateTo is not null) AND (@receiptPlanDateTo <> '') AND ISDATE(@receiptPlanDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND R.ReceiptPlanDate >= @receiptPlanDateFrom AND R.ReceiptPlanDate < DateAdd(d, 1, @receiptPlanDateTo)' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND R.ReceiptPlanDate = @receiptPlanDateFrom' + @CRLF 
			END
		ELSE
			IF ((@receiptPlanDateTo is not null) AND (@receiptPlanDateTo <> '') AND ISDATE(@receiptPlanDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND R.ReceiptPlanDate < DateAdd(d, 1, @receiptPlanDateTo)' + @CRLF
			END

		EXECUTE sp_executeSQL @SQL, @PARAM, @officeCode, @journalDateFrom, @journalDateTo, @receiptPlanDateFrom, @receiptPlanDateTo, @slipNumber, @customerClaimCode, @receiptType, @paymentKindCode

	END

	/*■■■■■■■■■■■■■■■■■■■■■■*/
	/* カード・ローンが選択されていない時だけ	  */
	/* カード会社からの入金予定以外を集計して取得 */
	/*											  */
	/*■■■■■■■■■■■■■■■■■■■■■■*/
	
	if((@customerClaimFilter <> '001') OR (@customerClaimFilter is null) OR (@customerClaimFilter = ''))
	BEGIN
		--伝票番号と請求先で金額を集計 金額は請求金額の合計を出すため入金完了フラグを無視する
		CREATE TABLE #temp_SumReceiptPlan1_SR1(
				  SlipNumber nvarchar(50)
				, CustomerClaimCode nvarchar(10)
				, Amount decimal(10,0)
				--, ReceivableBalance decimal(10,0)
			)
	
		SET @PARAM = ''
		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #temp_SumReceiptPlan1_SR1' + @CRLF
		SET @SQL = @SQL + 'SELECT'+ @CRLF
		SET @SQL = @SQL + '   LEFT(R.SlipNumber, 8)'+ @CRLF
		SET @SQL = @SQL + ' , R.CustomerClaimCode'+ @CRLF
		SET @SQL = @SQL + ' , SUM(ISNULL(R.Amount, 0))'+ @CRLF
		--SET @SQL = @SQL + ' , SUM(ISNULL(R.ReceivableBalance, 0))'+ @CRLF

		SET @SQL = @SQL + 'FROM dbo.ReceiptPlan AS R'+ @CRLF

		SET @SQL = @SQL + 'WHERE'+ @CRLF
		SET @SQL = @SQL + '		 R.DelFlag = ''0'''+ @CRLF
		--SET @SQL = @SQL + '	 AND R.CompleteFlag  = ''0'''+ @CRLF
		SET @SQL = @SQL + '	 AND R.ReceiptType not in (''011'', ''004'',''013'', 012)'+ @CRLF
		SET @SQL = @SQL + '	 AND EXISTS(SELECT 1 FROM #temp_SlipData_S AS S WHERE S.SlipNumber = R.SlipNumber)'+ @CRLF
		SET @SQL = @SQL + 'GROUP BY LEFT(R.SlipNumber, 8), R.CustomerClaimCode'+ @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM
		CREATE INDEX ix_temp_SumReceiptPlan1_SR1 ON #temp_SumReceiptPlan1_SR1(SlipNumber, CustomerClaimCode)


		----伝票番号と請求先で残高を集計 残高は入金完了フラグを考慮する
		CREATE TABLE #temp_SumReceiptPlan2_SR2(
		  SlipNumber nvarchar(50)
		, CustomerClaimCode nvarchar(10)
		--, Amount decimal(10,0)
		, ReceivableBalance decimal(10,0)
			)
	
		SET @PARAM = ''
		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #temp_SumReceiptPlan2_SR2' + @CRLF
		SET @SQL = @SQL + 'SELECT'+ @CRLF
		SET @SQL = @SQL + '   LEFT(R.SlipNumber, 8)'+ @CRLF
		SET @SQL = @SQL + ' , R.CustomerClaimCode'+ @CRLF
		--SET @SQL = @SQL + ' , SUM(ISNULL(R.Amount, 0))'+ @CRLF
		SET @SQL = @SQL + ' , SUM(ISNULL(R.ReceivableBalance, 0))'+ @CRLF

		SET @SQL = @SQL + 'FROM dbo.ReceiptPlan AS R'+ @CRLF

		SET @SQL = @SQL + 'WHERE'+ @CRLF
		SET @SQL = @SQL + '		 R.DelFlag = ''0'''+ @CRLF
		SET @SQL = @SQL + '	 AND R.CompleteFlag  = ''0'''+ @CRLF
		SET @SQL = @SQL + '	 AND R.ReceiptType not in (''011'', ''004'',''012'', ''013'')'+ @CRLF
		SET @SQL = @SQL + '	 AND EXISTS(SELECT 1 FROM #temp_SlipData_S AS S WHERE S.SlipNumber = R.SlipNumber)'+ @CRLF
		SET @SQL = @SQL + 'GROUP BY LEFT(R.SlipNumber, 8), R.CustomerClaimCode'+ @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM
		CREATE INDEX ix_temp_SumReceiptPlan2_SR2 ON #temp_SumReceiptPlan2_SR2(SlipNumber, CustomerClaimCode)

		--その他項目を一意にする（現状の仕様上一意になる）
		CREATE TABLE #temp_SumReceiptPlan3_SR3(
				  DepartmentCode nvarchar(3)
				, OccurredDepartmentCode nvarchar(3)
				, SlipNumber nvarchar(50)
				, CustomerClaimCode nvarchar(10)
				, ReceiptType nvarchar(3)
				, AccountCode nvarchar(50)
				, PaymentKindCode nvarchar(10)
				, CommissionRate decimal(8,5)
				, CommissionAmount decimal(10,0)
				, journalDate datetime 
			)

		SET @PARAM = ''
		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #temp_SumReceiptPlan3_SR3' + @CRLF
		SET @SQL = @SQL + 'SELECT'+ @CRLF
		SET @SQL = @SQL + '   R.DepartmentCode'+ @CRLF
		SET @SQL = @SQL + ' , R.OccurredDepartmentCode'+ @CRLF
		SET @SQL = @SQL + ' , R.SlipNumber'+ @CRLF
		SET @SQL = @SQL + ' , R.CustomerClaimCode'+ @CRLF
		SET @SQL = @SQL + ' , R.ReceiptType'+ @CRLF
		SET @SQL = @SQL + ' , R.AccountCode'+ @CRLF
		SET @SQL = @SQL + ' , R.PaymentKindCode'+ @CRLF
		SET @SQL = @SQL + ' , R.CommissionRate'+ @CRLF
		SET @SQL = @SQL + ' , R.CommissionAmount'+ @CRLF
		SET @SQL = @SQL + ' , R.journalDate'+ @CRLF

		SET @SQL = @SQL + 'FROM dbo.ReceiptPlan AS R'+ @CRLF

		SET @SQL = @SQL + 'WHERE'+ @CRLF
		SET @SQL = @SQL + '		 R.DelFlag = ''0'''+ @CRLF
		SET @SQL = @SQL + '	 AND R.CompleteFlag  = ''0'''+ @CRLF
		SET @SQL = @SQL + '	 AND R.ReceiptType not in (''011'', ''004'', ''012'', ''013'')'+ @CRLF
		
		SET @SQL = @SQL + '	 AND EXISTS(SELECT 1 FROM #temp_SlipData_S AS S WHERE S.SlipNumber = R.SlipNumber)'+ @CRLF

		SET @SQL = @SQL + 'GROUP BY R.DepartmentCode, R.OccurredDepartmentCode, R.SlipNumber, R.CustomerClaimCode, R.ReceiptType, R.AccountCode, R.PaymentKindCode, R.CommissionRate, R.CommissionAmount, R.journalDate'+ @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM
		CREATE INDEX ix_temp_SumReceiptPlan3_SR3 ON #temp_SumReceiptPlan3_SR3(SlipNumber, CustomerClaimCode)



		--集計したレコードを結果にinsert
		SET @PARAM = '@officeCode nvarchar(3), @journalDateFrom nvarchar(10), @journalDateTo nvarchar(10), @slipNumber nvarchar(50), @customerClaimCode nvarchar(10), @receiptType varchar(3), @paymentKindCode nvarchar(10)'

		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #temp_ReceiptPlanResult' + @CRLF
		SET @SQL = @SQL + 'SELECT'+ @CRLF
		SET @SQL = @SQL + '   '''''+ @CRLF
		SET @SQL = @SQL + ' , D.OfficeCode'+ @CRLF
		SET @SQL = @SQL + ' , o.OfficeName'+ @CRLF
		SET @SQL = @SQL + ' , SR3.DepartmentCode'+ @CRLF
		SET @SQL = @SQL + ' , SR3.OccurredDepartmentCode'+ @CRLF
		SET @SQL = @SQL + ' , SR1.CustomerClaimCode'+ @CRLF
		SET @SQL = @SQL + ' , CC.CustomerClaimName'+ @CRLF
		SET @SQL = @SQL + ' , SR3.SlipNumber'+ @CRLF
		SET @SQL = @SQL + ' , SR3.ReceiptType'+ @CRLF
		SET @SQL = @SQL + ' , null'+ @CRLF
		SET @SQL = @SQL + ' , SR3.AccountCode'+ @CRLF
		SET @SQL = @SQL + ' , SR1.Amount'+ @CRLF
		SET @SQL = @SQL + ' , SR2.ReceivableBalance'+ @CRLF
		SET @SQL = @SQL + ' , '''''+ @CRLF
		SET @SQL = @SQL + ' , SR3.JournalDate'+ @CRLF
		SET @SQL = @SQL + ' , SR3.PaymentKindCode'+ @CRLF
		SET @SQL = @SQL + ' , SR3.CommissionRate'+ @CRLF
		SET @SQL = @SQL + ' , SR3.CommissionAmount'+ @CRLF
		SET @SQL = @SQL + ' , '''''+ @CRLF
		SET @SQL = @SQL + ' , S.SalesDate'+ @CRLF
		SET @SQL = @SQL + ' , S.SalesPlanDate'+ @CRLF
		SET @SQL = @SQL + ' , S.accountUsageType'+ @CRLF

		SET @SQL = @SQL + 'FROM #temp_SumReceiptPlan2_SR2 AS SR2'+ @CRLF
		SET @SQL = @SQL + 'INNER JOIN #temp_SumReceiptPlan1_SR1 AS SR1 ON LEFT(SR1.SlipNumber, 8) = LEFT(SR2.SlipNumber, 8) AND SR1.CustomerClaimCode = SR2.CustomerClaimCode'+ @CRLF
		SET @SQL = @SQL + 'INNER JOIN #temp_SumReceiptPlan3_SR3 AS SR3 ON LEFT(SR3.SlipNumber, 8) = LEFT(SR2.SlipNumber, 8) AND SR3.CustomerClaimCode = SR2.CustomerClaimCode'+ @CRLF
		SET @SQL = @SQL + 'INNER JOIN #temp_SlipData_S AS S ON S.SlipNumber = SR1.SlipNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = SR3.OccurredDepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.CustomerClaim AS CC ON CC.CustomerClaimCode = SR1.CustomerClaimCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Office AS o ON o.OfficeCode = D.OfficeCode'+ @CRLF
	
		SET @SQL = @SQL + 'WHERE EXISTS(SELECT 1 FROM #temp_CustomerClaimType_CT AS CT WHERE CT.Code = CC.CustomerClaimType)'+ @CRLF
		SET @SQL = @SQL + '  AND SR3.ReceiptType <>''013'''+ @CRLF --下取は弾く（消込画面に表示しない）

		--店舗入金消込の場合は残債を弾く
		IF(@isShopDeposit = '1')
		BEGIN
			SET @SQL = @SQL + '  AND SR3.ReceiptType <>''012'''+ @CRLF
		END

		--伝票番号
		IF ((@SlipNumber IS NOT NULL) AND (@SlipNumber <> ''))
		BEGIN
			SET @SQL = @SQL +'AND SR3.SlipNumber LIKE ''%' + @SlipNumber + '%''' + @CRLF
		END

		--請求先コード
		IF ((@customerClaimCode IS NOT NULL) AND (@customerClaimCode <>''))
			BEGIN
				SET @SQL = @SQL + 'AND SR1.CustomerClaimCode = @customerClaimCode'+ @CRLF
			END
	
		--事業所コード
		IF ((@officeCode IS NOT NULL) AND (@officeCode <>''))
			BEGIN
				SET @SQL = @SQL + 'AND D.OfficeCode = @officeCode'+ @CRLF
			END

		--入金種別
		IF ((@receiptType IS NOT NULL) AND (@receiptType <>''))
			BEGIN
				SET @SQL = @SQL + 'AND SR3.ReceiptType = @receiptType'+ @CRLF
			END
	
		--決済種別
		IF ((@paymentKindCode IS NOT NULL) AND (@paymentKindCode <>''))
			BEGIN
				SET @SQL = @SQL + 'AND SR3.PaymentKindCode = @paymentKindCode'+ @CRLF
			END

		--決済日
		IF ((@journalDateFrom is not null) AND (@journalDateFrom <> '') AND ISDATE(@journalDateFrom) = 1)
			IF ((@journalDateTo is not null) AND (@journalDateTo <> '') AND ISDATE(@journalDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND SR3.journalDateDate >= @journalDateFrom AND SR3.journalDate < DateAdd(d, 1, @RjournalDateTo)' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND SR3.journalDate = @journalDateFrom' + @CRLF 
			END
		ELSE
			IF ((@journalDateTo is not null) AND (@journalDateTo <> '') AND ISDATE(@journalDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND SR3.journalDate < DateAdd(d, 1, @journalDateTo)' + @CRLF 
			END

		EXECUTE sp_executeSQL @SQL, @PARAM, @officeCode, @journalDateFrom, @journalDateTo, @slipNumber, @customerClaimCode, @receiptType, @paymentKindCode

	END

	SELECT * FROM #temp_ReceiptPlanResult
	ORDER BY accountUsageType 
			, OfficeCode
			,CASE WHEN SalesDate IS NOT NULL THEN SalesDate END


BEGIN

	BEGIN TRY
		DROP TABLE #temp_SlipData_S
		DROP TABLE #temp_CustomerClaimType_CT
		DROP TABLE #temp_ReceiptPlanResult
		DROP TABLE #temp_SumReceiptPlan1_SR1
		DROP TABLE #temp_SumReceiptPlan2_SR2
		DROP TABLE #temp_SumReceiptPlan3_SR3
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

END







GO


