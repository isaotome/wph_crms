USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetReceiptPlan]    Script Date: 2016/07/28 15:07:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--2015/02/09 arc yano #3153 入金実績リスト対応
--Mod 2016/07/19 #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）

CREATE PROCEDURE [dbo].[GetReceiptPlan]
	@CustomerCode NVARCHAR(10),    --顧客コード
	@SlipNumber NVARCHAR(50),      --伝票番号
	@SummaryFlag bit = 1		   --サマリで取得するかどうか（０：しない　１：する）
AS

BEGIN
	/*-------------------------------------------*/
	/* 一時テーブル                              */
	/*-------------------------------------------*/
	-- 入金予定一覧テーブル
		CREATE TABLE #TempReceptPlan_T
		(
			ReceiptPlanId uniqueidentifier,
			SlipNumber nvarchar(50),
			ReceiptPlanDate  datetime,
			Code  varchar(3),
			Name  varchar(50),
			DepartmentCode  nvarchar(3),
			DepartmentName  nvarchar(20),
			CustomerClaimCode nvarchar(10),
			CustomerClaimName nvarchar(80),
			AccountCode nvarchar(50),
			AccountName nvarchar(50),
			Amount decimal(10, 0),
			ReceivableBalance decimal(10, 0),
			CompleteFlag nvarchar(2),
			OccurredDepartmentCode nvarchar(3),
			ReceiptType nvarchar(3)
		)
		

	/*-------------------------------------------*/
	/* 入金予定一覧                              */
	/*-------------------------------------------*/
	DECLARE @STRSQL 		AS VARCHAR(900)	--実行するSQL文
	DECLARE @STRSELECT 		AS VARCHAR(400)	--SELECT句
	DECLARE @STRCOMMONWHERE	AS VARCHAR(200)	--WHERE句(共通)
	DECLARE @STRWHERE 		AS VARCHAR(200)	--WHERE句(条件による分岐)
	DECLARE @STRORDER		AS VARCHAR(100)	--ORDER句

	if(@SummaryFlag = 0) --サマリしない
	BEGIN
		--SELECT句の設定
		SET @STRSELECT = 'SELECT'
		SET @STRSELECT = @STRSELECT + ' [ReceiptPlanId]' 
		SET @STRSELECT = @STRSELECT + ',[SlipNumber]' 
		SET @STRSELECT = @STRSELECT + ',[ReceiptPlanDate]'
		SET @STRSELECT = @STRSELECT + ',[Code]'
		SET @STRSELECT = @STRSELECT + ',[Name]'
		SET @STRSELECT = @STRSELECT + ',[DepartmentCode]'
		SET @STRSELECT = @STRSELECT + ',[DepartmentName]'
		SET @STRSELECT = @STRSELECT + ',[CustomerClaimCode]'
		SET @STRSELECT = @STRSELECT + ',[CustomerClaimName]'
		SET @STRSELECT = @STRSELECT + ',[AccountCode]'
		SET @STRSELECT = @STRSELECT + ',[AccountName]'
		SET @STRSELECT = @STRSELECT + ',[Amount]'
		SET @STRSELECT = @STRSELECT + ',[ReceivableBalance]'
		SET @STRSELECT = @STRSELECT + ',[CompleteFlag]'
		SET @STRSELECT = @STRSELECT + ',[OccurredDepartmentCode]'
		SET @STRSELECT = @STRSELECT + ',[ReceiptType]'
		SET @STRSELECT = @STRSELECT + ' FROM V_ReceiptPlanList RList'

		--WHERE句(共通部分)の設定
		SET @STRCOMMONWHERE = ' WHERE'
		SET @STRCOMMONWHERE = @STRCOMMONWHERE + ' ( DepartmentCode = OccurredDepartmentCode or ReceiptType = ''004'')'
		SET @STRCOMMONWHERE = @STRCOMMONWHERE + ' AND Left(DepartmentCode,1) <> ''0'''

		--ORDER句の設定
		SET @STRORDER = ' ORDER BY SlipNumber, ReceiptPlanDate, CustomerClaimCode'


		--条件によるWHERE句の設定
		IF ((@SlipNumber IS NOT NULL) AND (@SlipNumber <> ''))
			BEGIN
				SET @STRWHERE = ' AND Left(SlipNumber, 8) = Left(''' + @SlipNumber + ''', 8)'
			END
		ELSE
			BEGIN
				SET @STRWHERE = ' AND EXISTS (' 
				SET @STRWHERE = @STRWHERE + ' SELECT ''X'' FROM V_ALL_SalesOrderList' 
				SET @STRWHERE = @STRWHERE + ' WHERE CustomerCode = ''' + @CustomerCode + ''''
				SET @STRWHERE = @STRWHERE + ' AND SlipNumber = Rlist.SlipNumber )'
			END

		SET @STRSQL = 'INSERT INTO #TempReceptPlan_T ' + @STRSELECT + @STRCOMMONWHERE + @STRWHERE + @STRORDER

		EXEC(@STRSQL)

	END
	ELSE --サマリする
	BEGIN
		
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
		WHERE rp.[CustomerClaimCode] is not NULL
		  AND rp.[CustomerClaimCode] <> ''
		  AND rp.[DelFlag] = '0'
		  AND Left(rp.DepartmentCode,1) <> '0'
		  AND Left(rp.SlipNumber, 8) = Left(@SlipNumber, 8)

		GROUP BY  rp.[SlipNumber], rp.[CustomerClaimCode]
		ORDER BY SlipNumber,CustomerClaimCode


		INSERT INTO #TempReceptPlan_T
		SELECT NEWID() --ReceiptPlanId
			  ,r.SlipNumber
			  ,null --ReceiptPlanDate
			  ,''	--Code
			  ,''	--Name
			  ,''	--DepartmentCode
			  ,''	--DepartmentName
			  ,r.CustomerClaimCode
			  ,cc.CustomerClaimName
			  ,''	--AccountCode
			  ,''	--AccountName
			  ,r.Amount
			  ,r.ReceivableBalance
			  ,''	--CompleteFlag
			  ,''	--OccurredDepartmentCode
			  ,''	--ReceiptType

		FROM #temp_ReceiptPlan r
		INNER JOIN WPH_DB.dbo.CustomerClaim cc on cc.CustomerClaimCode = r.CustomerClaimCode
	END

	SELECT * FROM #TempReceptPlan_T

	--PRINT @STRSQL

	BEGIN TRY
		DROP TABLE #TempReceptPlan_T
		DROP TABLE #temp_ReceiptPlan
	END TRY
	BEGIN CATCH
		--無視
	END CATCH
END






GO


