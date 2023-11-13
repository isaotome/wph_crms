USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetJournalData]    Script Date: 2015/06/30 11:28:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--Add 2015/06/24 arc yano 経理対応② 入金実績情報の抽出
CREATE PROCEDURE [dbo].[GetJournalData]
	 @JournalDateFrom nvarchar(10),					--入金日(From)
	 @JournalDateTo   nvarchar(10)					--入金日(To)
	 	 
AS	
	BEGIN
		SET NOCOUNT ON;

		--/*
		DECLARE @SQL NVARCHAR(MAX) = ''
		DECLARE @PARAM NVARCHAR(1024) = ''
		DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

		--■■一時表の宣言
		/*************************************************************************/
		
		--入金実績情報
		CREATE TABLE #Journal (
			[JournalDate]			DATETIME NOT NULL				--入金日
		,	[DepartmentCode]		NVARCHAR(3)						--部門コード
		,	[DepartmentName]		NVARCHAR(20)					--部門名
		,	[CustomerClaimCode]		NVARCHAR(10)					--請求先コード
		,	[CustomerClaimName]		NVARCHAR(80)					--請求先名
		,	[SlipNumber]			NVARCHAR(50)					--伝票番号
		,	[Amount]				DECIMAL(10, 0)					--金額
		,	[AccountType]			VARCHAR(50)						--口座種別
		,	[Summary]				NVARCHAR(50)					--摘要
		,	[AccountCode]			NVARCHAR(50)					--勘定科目コード
		,	[AccountName]			NVARCHAR(80)					--勘定科目名
		)
		
		--販売伝票情報(車両／サービス)
		CREATE TABLE #SalesOrder (
			[SlipNumber]	NVARCHAR(50) NOT NULL			--伝票番号
		,	[OrderStatus]	NVARCHAR(50)					--伝票ステータス(名称)
		,	[SalesDate]		DATETIME						--納車日
		,	[CustomerCode]	NVARCHAR(10)					--顧客コード
		,	[CustomerName]	NVARCHAR(80)					--顧客名
		)
		CREATE UNIQUE INDEX IX_Temp_SalesOrder ON #SalesOrder ([SlipNumber])

		--ダーティーリードの設定
		SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

		--入金実績情報(日付による絞り込み)
		SET @PARAM = '@JournalDateFrom nvarchar(10), @JournalDateTo nvarchar(10)'

		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #Journal' + @CRLF
		SET @SQL = @SQL +'SELECT' + @CRLF
		SET @SQL = @SQL +'   jn.JournalDate AS JournalDate' + @CRLF
		SET @SQL = @SQL +' , jn.DepartmentCode AS DepartmentCode' + @CRLF
		SET @SQL = @SQL +' , dp.DepartmentName AS DepartmentName' + @CRLF
		SET @SQL = @SQL +' , jn.CustomerClaimCode AS CustomerClaimCode' + @CRLF
		SET @SQL = @SQL +' , cc.CustomerClaimName AS CustomerClaimName' + @CRLF
		SET @SQL = @SQL +' , jn.SlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL +' , jn.Amount AS Amount' + @CRLF
		SET @SQL = @SQL +' , at.Name AS AccountType' + @CRLF
		SET @SQL = @SQL +' , jn.Summary AS  Summary' + @CRLF
		SET @SQL = @SQL +' , jn.AccountCode AS AccountCode' + @CRLF
		SET @SQL = @SQL +' , ac.AccountName AS AccountName' + @CRLF
		SET @SQL = @SQL +'FROM' + @CRLF
		SET @SQL = @SQL +'	dbo.Journal jn INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.c_AccountType at ON at.Code = jn.AccountType INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.Department dp ON jn.departmentcode = dp.DepartmentCode INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.Account ac ON jn.AccountCode = ac.AccountCode LEFT JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.CustomerClaim cc ON jn.CustomerClaimCode = cc.CustomerClaimCode' + @CRLF
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'  jn.JournalType = ''001''' + @CRLF
		SET @SQL = @SQL +'  AND jn.SlipNumber != ''''' + @CRLF
		SET @SQL = @SQL +'  AND jn.DelFlag = ''0''' + @CRLF
		

		--入金日
		IF ((@JournalDateFrom is not null) AND (@JournalDateFrom <> '') AND (ISDATE(@JournalDateFrom) = 1))
			IF ((@JournalDateTo is not null) AND (@JournalDateTo <> '') AND (ISDATE(@JournalDateTo) = 1) )
			BEGIN
				SET @SQL = @SQL +'AND jn.JournalDate >= @JournalDateFrom AND jn.JournalDate < DateAdd(d, 1, @JournalDateTo)' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND jn.JournalDate = @JournalDateFrom' + @CRLF 
			END
		ELSE
			IF ((@JournalDateTo is not null) AND (@JournalDateTo <> '') AND ISDATE(@JournalDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND jn.JournalDate < DateAdd(d, 1, @JournalDateTo)' + @CRLF 
			END
		
		--SQL実行
		EXECUTE sp_executeSQL @SQL, @PARAM, @JournalDateFrom, @JournalDateTo

		--For Debug
		--PRINT @SQL

		--伝票情報テーブルに車両伝票情報をセット
		INSERT INTO 
			#SalesOrder 
		SELECT 
			  csh.SlipNumber AS SlipNumber
			, ISNULL(sos.Name, '') AS OrderStatus
			, csh.SalesDate AS SalesDate
			, cu.CustomerCode AS CustomerCode
			, cu.CustomerName AS CustomerName
		FROM
			dbo.CarSalesHeader csh  LEFT JOIN
			dbo.c_SalesOrderStatus sos ON csh.SalesOrderStatus = sos.Code LEFT JOIN
			dbo.Customer cu ON csh.CustomerCode = cu.CustomerCode
		WHERE
			exists
			(
				SELECT 'X' FROM #Journal jn WHERE  jn.SlipNumber = csh.SlipNumber
			)
			AND
			csh.DelFlag = '0'

		--伝票情報テーブルにサービス伝票情報をセット
		INSERT INTO 
			#SalesOrder 
		SELECT 
			  ssh.SlipNumber AS SlipNumber
			, ISNULL(sos.Name, '') AS OrderStatus
			, ssh.SalesDate AS SalesDate
			, cu.CustomerCode AS CustomerCode
			, cu.CustomerName AS CustomerName
		FROM
			dbo.ServiceSalesHeader ssh  LEFT JOIN
			dbo.c_ServiceOrderStatus sos ON ssh.ServiceOrderStatus = sos.Code LEFT JOIN
			dbo.Customer cu ON ssh.CustomerCode = cu.CustomerCode 
		WHERE
			exists
			(
				SELECT 'X' FROM #Journal jn WHERE  jn.SlipNumber = ssh.SlipNumber
			)
			and
			ssh.DelFlag = '0'
		
		--インデックス再生成
		DROP INDEX IX_Temp_SalesOrder ON #SalesOrder
		CREATE UNIQUE INDEX IX_Temp_SalesOrder ON #SalesOrder ([SlipNumber])


		--一時テーブルよりデータを取得
		SELECT
			  jn.JournalDate as JournalDate
			, jn.DepartmentCode as DepartmentCode
			, jn.DepartmentName as DepartmentName
			, jn.CustomerClaimCode as CustomerClaimCode
			, jn.CustomerClaimName as CustomerClaimName
			, jn.SlipNumber as SlipNumber
			, so.OrderStatus AS OrderStatus
			, so.SalesDate AS SalesDate
			, so.CustomerCode AS CustomerCode
			, so.CustomerName AS CustomerName
			, jn.Amount AS Amount
			, jn.AccountType AS AccountType
			, jn.Summary AS  Summary
			, jn.AccountCode AS AccountCode
			, jn.AccountName as AccountName

		FROM
			#Journal jn LEFT JOIN
			#SalesOrder so ON jn.SlipNumber = so.SlipNumber
		ORDER BY
			  jn.JournalDate
			, jn.DepartmentCode
			, jn.SlipNumber
--/*
		BEGIN TRY
			DROP TABLE #Journal
			DROP TABLE #SalesOrder
		END TRY
		BEGIN CATCH
			--無視
		END CATCH
--*/
	END
GO


