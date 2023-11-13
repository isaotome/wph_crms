USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetReceiptResult]    Script Date: 2015/02/09 14:47:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








--2015/02/09 arc yano #3153 入金実績リスト対応
CREATE PROCEDURE [dbo].[GetReceiptResult]
	@CustomerCode NVARCHAR(10),    --顧客コード
	@SlipNumber NVARCHAR(50)       --伝票番号
AS

BEGIN

	/*-------------------------------------------*/
	/* 一時テーブル                              */
	/*-------------------------------------------*/
	-- 入金(予定・実績)一覧テーブル
		CREATE TABLE #TempReceptResult_T
		(
			ST nvarchar(10),
			SlipNumber nvarchar(50),
			ReceiptDate  datetime,
			AccountType  varchar(3),
			AccountCode  varchar(50),
			AccountName  varchar(50),
			Amount decimal(10, 0)
		)
		
	/*-------------------------------------------*/
	/* 入金実績一覧                              */
	/*-------------------------------------------*/
	--共通

	DECLARE @STRORDER			AS VARCHAR(100)	--ORDER BY句
	DECLARE @STRSQL 			AS VARCHAR(2000)--実行するSQL文

	--ReceptPlanList取得用のSQL
	DECLARE @STRSQL_P 			AS VARCHAR(1000)	
	DECLARE @STRSELECT_P		AS VARCHAR(400)	--SELECT句
	DECLARE @STRCOMMONWHERE_P	AS VARCHAR(200)	--WHERE句(共通)
	DECLARE @STRWHERE_P 		AS VARCHAR(200)	--WHERE句(条件による分岐)
	DECLARE @STRGROUP_P			AS VARCHAR(200)	--GROUP BY句

	--ReceptResultList取得用のSQL
	DECLARE @STRSQL_R 			AS VARCHAR(1000)
	DECLARE @STRSELECT_R		AS VARCHAR(400)	--SELECT句
	DECLARE @STRCOMMONWHERE_R	AS VARCHAR(200)	--WHERE句(共通)
	DECLARE @STRWHERE_R 		AS VARCHAR(200)	--WHERE句(条件による分岐)
	DECLARE @STRGROUP_R			AS VARCHAR(200)	--GROUP BY句

	/*---------------------------
	//V_ReceiptPlanの取得
	-----------------------------*/
	--SELECT句の設定
	SET @STRSELECT_P = 'SELECT'
	SET @STRSELECT_P = @STRSELECT_P + ' ''0'' AS ST' 
	SET @STRSELECT_P = @STRSELECT_P + ',SlipNumber'
	SET @STRSELECT_P = @STRSELECT_P + ',ReceiptPlanDate AS ReceiptDate'
	SET @STRSELECT_P = @STRSELECT_P + ',Case when Code In (''001'',''002'',''003'',''004'') then Code else ''009'' end AccountType'
	SET @STRSELECT_P = @STRSELECT_P + ',AccountCode'
	SET @STRSELECT_P = @STRSELECT_P + ',AccountName'
	SET @STRSELECT_P = @STRSELECT_P + ',sum(Amount) as Amount'
	SET @STRSELECT_P = @STRSELECT_P + ' FROM V_ReceiptPlanList PList'

	--WHERE句(共通部分)の設定
	SET @STRCOMMONWHERE_P = ' WHERE'
	SET @STRCOMMONWHERE_P = @STRCOMMONWHERE_P + ' ( DepartmentCode = OccurredDepartmentCode or ReceiptType = ''004'')'

	--GOURP BY句の設定
	SET @STRGROUP_P= ' GROUP BY SlipNumber,ReceiptPlanDate'
	SET @STRGROUP_P= @STRGROUP_P + ' ,Case when Code In (''001'',''002'',''003'',''004'') then Code else ''009'' end'
	SET @STRGROUP_P= @STRGROUP_P + ' ,AccountCode,AccountName' 

	--条件によるWHERE句の設定
	IF ((@SlipNumber IS NOT NULL) AND (@SlipNumber <> ''))
		BEGIN
			SET @STRWHERE_P = ' AND Left(SlipNumber, 8) = Left(''' + @SlipNumber + ''', 8)'
		END
	ELSE
		BEGIN
			SET @STRWHERE_P = ' AND EXISTS (' 
			SET @STRWHERE_P = @STRWHERE_P + ' SELECT ''X'' FROM V_ALL_SalesOrderList' 
			SET @STRWHERE_P = @STRWHERE_P + ' WHERE CustomerCode = ''' + @CustomerCode + ''''
			SET @STRWHERE_P = @STRWHERE_P + ' AND SalesStatus <>''007'''
			SET @STRWHERE_P = @STRWHERE_P + ' AND SlipNumber = PList.SlipNumber )'
		END

	/*---------------------------
	//V_ReceiptResultの取得
	-----------------------------*/
	--SELECT句の設定
	SET @STRSELECT_R = 'SELECT'
	SET @STRSELECT_R = @STRSELECT_R + ' ''1'' AS ST' 
	SET @STRSELECT_R = @STRSELECT_R + ',SlipNumber'
	SET @STRSELECT_R = @STRSELECT_R + ',JournalDate AS ReceiptDate'
	SET @STRSELECT_R = @STRSELECT_R + ',Case when AccountType In (''001'',''002'',''003'',''004'') then AccountType else ''009'' end AccountType'
	SET @STRSELECT_R = @STRSELECT_R + ',AccountCode'
	SET @STRSELECT_R = @STRSELECT_R + ',AccountName'
	SET @STRSELECT_R = @STRSELECT_R + ',sum(Amount) as Amount'
	SET @STRSELECT_R = @STRSELECT_R + ' FROM V_ReceiptList RList'

	--WHERE句(共通部分)の設定
	SET @STRCOMMONWHERE_R = ' WHERE'
	SET @STRCOMMONWHERE_R = @STRCOMMONWHERE_R + ' AccountType <>''099'''
	SET @STRCOMMONWHERE_R = @STRCOMMONWHERE_R + ' AND AccountType <>''011'''

	--GOURP BY句の設定
	SET @STRGROUP_R= ' GROUP BY SlipNumber,JournalDate'
	SET @STRGROUP_R= @STRGROUP_R + ' ,Case when AccountType In (''001'',''002'',''003'',''004'') then AccountType else ''009'' end'
	SET @STRGROUP_R= @STRGROUP_R + ' ,AccountCode,AccountName' 

	--条件によるWHERE句の設定
	IF ((@SlipNumber IS NOT NULL) AND (@SlipNumber <> ''))
		BEGIN
			SET @STRWHERE_R = ' AND Left(SlipNumber, 8) = Left(''' + @SlipNumber + ''', 8)'
		END
	ELSE
		BEGIN
			SET @STRWHERE_R = ' AND EXISTS (' 
			SET @STRWHERE_R = @STRWHERE_R + ' SELECT ''X'' FROM V_ALL_SalesOrderList' 
			SET @STRWHERE_R = @STRWHERE_R + ' WHERE CustomerCode = ''' + @CustomerCode + ''''
			SET @STRWHERE_R = @STRWHERE_R + ' AND SlipNumber = RList.SlipNumber )'
		END
	
	/*---------------------------
	//共通
	-----------------------------*/
	SET @STRORDER = ' ORDER BY ST, SlipNumber, ReceiptDate, AccountType'


	/*---------------------------
	//SQL組立＋実行
	-----------------------------*/
	SET @STRSQL_P = @STRSELECT_P + @STRCOMMONWHERE_P + @STRWHERE_P + @STRGROUP_P
	SET @STRSQL_R = @STRSELECT_R + @STRCOMMONWHERE_R + @STRWHERE_R + @STRGROUP_R

	SET @STRSQL = 'INSERT INTO #TempReceptResult_T ' + @STRSQL_P + ' UNION ALL ' + @STRSQL_R + @STRORDER
	EXEC(@STRSQL)

	SELECT * FROM #TempReceptResult_T

	BEGIN TRY
		DROP TABLE #TempReceptResult_T
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

	--PRINT @STRSQL
END





GO


