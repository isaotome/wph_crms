USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetServiceSalesSlip]    Script Date: 2018/01/31 14:48:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2018/01/18 arc yano #3834 ワランティ作業納品書発行移行 新規作成
-- Description:	<Description,,>
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetServiceSalesSlip] 
	   @SlipNumber nvarchar(50) = ''				--伝票番号
	  ,@ArrivalPlanDateFrom nvarchar(10)			--入庫日From
	  ,@ArrivalPlanDateTo nvarchar(10)				--入庫日To
	  ,@DepartmentCode nvarchar(6) = ''				--部門コード
	  ,@ServiceOrderStatus nvarchar(3) = ''			--伝票ステータス
	  ,@ServiceWorkCode nvarchar(5) = ''			--主作業コード	  
	  ,@CustomerCode nvarchar(10) = ''				--顧客コード
	  ,@CustomerName nvarchar(80) = ''				--顧客名
AS 
BEGIN

--/*
	SET NOCOUNT ON;

	/*-------------------------------------------*/
	/* ■■定数の宣言							 */
	/*-------------------------------------------*/
	--処理結果
	DECLARE @ErrorMessage NVARCHAR(4000) = ''
    DECLARE @ErrorNumber INT = 0

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @TableName NVARCHAR(50) = ''

	

	--■一時表の削除
	/*************************************************************************/

	IF OBJECT_ID(N'tempdb..#Temp_ServiceSalesLine', N'U') IS NOT NULL
	DROP TABLE #Temp_ServiceSalesLine;

	/*************************************************************************/
	
	/*-------------------------------------------*/
	/* ■■一時表の宣言							 */
	/*-------------------------------------------*/
	--サービス伝票明細
	CREATE TABLE #Temp_ServiceSalesLine (
		 [SlipNumber] NVARCHAR(50) NOT NULL			--部門コード
		,[RevisionNumber] int NOT NULL				--リビジョン番号
		,[ServiceWorkCode] NVARCHAR(5)				--主作業コード
		,[CustomerClaimCode]  NVARCHAR(10)			--請求先コード
	)


	/*-------------------------------------------*/
	/* データ取得								 */
	/*-------------------------------------------*/
	--トランザクション開始 
	BEGIN TRANSACTION
	BEGIN TRY

		-- ----------------------
		-- サービス伝票明細
		-- ----------------------
		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #Temp_ServiceSalesLine' + @CRLF
		SET @SQL = @SQL +'SELECT' + @CRLF
		SET @SQL = @SQL +'	  sl.SlipNumber' + @CRLF
		SET @SQL = @SQL +'	, sl.RevisionNumber' + @CRLF
		SET @SQL = @SQL +'	, sl.ServiceWorkCode' + @CRLF
		SET @SQL = @SQL +'	, sl.CustomerClaimCode' + @CRLF
		SET @SQL = @SQL +'FROM' + @CRLF
		SET @SQL = @SQL +'	dbo.ServiceSalesLine sl' + @CRLF
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    sl.DelFlag = ''0''' + @CRLF 
		IF ((@SlipNumber is not null) AND (@SlipNumber <> ''))
		BEGIN
			SET @SQL = @SQL +'AND sl.SlipNumber LIKE ''%' + @SlipNumber + '%''' + @CRLF
		END
		IF ((@ServiceWorkCode is not null) AND (@ServiceWorkCode <> ''))
		BEGIN
			SET @SQL = @SQL +'AND sl.ServiceWorkCode = ''' + @ServiceWorkCode + '''' + @CRLF
		END

		SET @SQL = @SQL +'GROUP BY' + @CRLF
		SET @SQL = @SQL +'    sl.SlipNumber' + @CRLF
		SET @SQL = @SQL +'   ,sl.RevisionNumber' + @CRLF
		SET @SQL = @SQL +'   ,sl.ServiceWorkCode' + @CRLF
		SET @SQL = @SQL +'   ,sl.CustomerClaimCode' + @CRLF

		--DEBUG
		--PRINT @SQL

		EXECUTE sp_executeSQL @SQL
		CREATE UNIQUE INDEX IX_Temp_ServiceSalesLine ON #Temp_ServiceSalesLine(SlipNumber, RevisionNumber, ServiceWorkCode, CustomerClaimCode)

		-- ----------------------
		-- サービス伝票取得
		-- ----------------------
		SET @SQL = ''
		SET @SQL = @SQL +'SELECT' + @CRLF
		SET @SQL = @SQL +'	  sh.SlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL +'	, sh.RevisionNumber AS RevisionNumber' + @CRLF
		SET @SQL = @SQL +'	, sh.ServiceOrderStatus AS ServiceOrderStatus' + @CRLF
		SET @SQL = @SQL +'	, cso.Name AS ServiceOrderStatusName' + @CRLF
		SET @SQL = @SQL +'	, sh.SalesDate AS SalesDate' + @CRLF
		SET @SQL = @SQL +'	, sh.ArrivalPlanDate AS ArrivalPlanDate' + @CRLF
		SET @SQL = @SQL +'	, sh.CustomerCode AS CustomerCode' + @CRLF
		SET @SQL = @SQL +'	, cs.CustomerName AS CustomerName' + @CRLF
		SET @SQL = @SQL +'	, sl.ServiceWorkCode AS ServiceWorkCode' + @CRLF
		SET @SQL = @SQL +'	, sw.Name AS ServiceWorkName' + @CRLF
		SET @SQL = @SQL +'	, sl.CustomerClaimCode AS CustomerClaimCode' + @CRLF
		SET @SQL = @SQL +'	, cc.CustomerClaimName AS CustomerClaimName' + @CRLF
		SET @SQL = @SQL +'	, d.DepartmentCode AS DepartmentCode' + @CRLF
		SET @SQL = @SQL +'	, d.DepartmentName AS DepartmentName' + @CRLF
		SET @SQL = @SQL +'FROM' + @CRLF
		SET @SQL = @SQL +'	dbo.ServiceSalesHeader sh' + @CRLF
		SET @SQL = @SQL +'INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	#Temp_ServiceSalesLine sl ON sh.SlipNumber = sl.SlipNumber AND sh.RevisionNumber = sl.RevisionNumber' + @CRLF
		SET @SQL = @SQL +'INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.Customer cs ON sh.CustomerCode = cs.CustomerCode' + @CRLF
		SET @SQL = @SQL +'INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.c_ServiceOrderStatus cso ON sh.ServiceOrderStatus = cso.Code' + @CRLF
		SET @SQL = @SQL +'LEFT JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.Department d ON sh.DepartmentCode = d.DepartmentCode' + @CRLF
		SET @SQL = @SQL +'INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.ServiceWork sw ON sl.ServiceWorkCode = sw.ServiceWorkCode' + @CRLF
		SET @SQL = @SQL +'INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.CustomerClaim cc ON sl.CustomerClaimCode = cc.CustomerClaimCode' + @CRLF

		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    sh.DelFlag = ''0''' + @CRLF 
		SET @SQL = @SQL +'AND sh.ServiceOrderStatus in (''003'', ''004'', ''005'', ''006'', ''009'')' + @CRLF

		--伝票番号
		IF ((@SlipNumber is not null) AND (@SlipNumber <> ''))
		BEGIN
			SET @SQL = @SQL +'AND sh.SlipNumber LIKE ''%' + @SlipNumber + '%''' + @CRLF
		END
		--入庫日(From)
		IF ((@ArrivalPlanDateFrom is not null) AND (@ArrivalPlanDateFrom <> ''))
		BEGIN
			SET @SQL = @SQL +'AND sh.ArrivalPlanDate >= CONVERT(datetime,  ''' + @ArrivalPlanDateFrom + ''')' + @CRLF
		END
		--入庫日(To)
		IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> ''))
		BEGIN
			SET @SQL = @SQL +'AND sh.ArrivalPlanDate < CONVERT(datetime,  ''' + @ArrivalPlanDateTo + ''')' + @CRLF
		END
		--部門コード
		IF ((@DepartmentCode is not null) AND (@DepartmentCode <> ''))
		BEGIN
			SET @SQL = @SQL +'AND sh.DepartmentCode = ''' + @DepartmentCode + '''' + @CRLF
		END
		--伝票ステータス
		IF ((@ServiceOrderStatus is not null) AND (@ServiceOrderStatus <> ''))
		BEGIN
			SET @SQL = @SQL +'AND sh.ServiceOrderStatus = ''' + @ServiceOrderStatus + '''' + @CRLF
		END
		--顧客コード
		IF ((@CustomerCode is not null) AND (@CustomerCode <> ''))
		BEGIN
			SET @SQL = @SQL +'AND sh.CustomerCode = ''' + @CustomerCode + '''' + @CRLF
		END
		--顧客名
		IF ((@CustomerName is not null) AND (@CustomerName <> ''))
		BEGIN
			SET @SQL = @SQL +'AND cs.CustomerName LIKE ''%' + @CustomerName + '%''' + @CRLF
		END


		SET @SQL = @SQL +'ORDER BY' + @CRLF
		SET @SQL = @SQL +'    sh.SlipNumber' + @CRLF

		--DEBUG
		--PRINT @SQL

		EXECUTE sp_executeSQL @SQL

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
			--終了	
--*/
/*
SELECT
			 convert(nvarchar(8), '') as SlipNumber
			,convert(int, null) as RevisionNumber
			,convert(nvarchar(3), '') as ServiceOrderStatus
			,convert(nvarchar(50), '') as ServiceOrderStatusName
			,convert(datetime, null) as SalesDate
			,convert(datetime, null) as ArrivalPlanDate
			,convert(nvarchar(10), '') as CustomerCode
			,convert(nvarchar(80), '') as CustomerName
			,convert(nvarchar(5), '') as ServiceWorkCode
			,convert(nvarchar(20), '') as ServiceWorkName
			,convert(nvarchar(10), '') as CustomerClaimCode
			,convert(nvarchar(80), '') as CustomerClaimName
			,convert(nvarchar(3), '') as DepartmentCode
			,convert(nvarchar(20), '') as DepartmentName

*/
END



GO


