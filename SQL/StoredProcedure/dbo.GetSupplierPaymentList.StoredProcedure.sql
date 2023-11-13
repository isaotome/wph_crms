USE [WPH_DB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/23 arc yano #3729 サブシステム移行（外注支払一覧）
-- Description:	<Description,,>
-- 外注支払一覧の取得
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetSupplierPaymentList] 
	    @Target nvarchar(1) = '0'						--検索対象(0:受注日 1:納車日)
	  , @TargetDateFrom datetime						--対象年月(From)
	  , @TargetDateTo datetime							--対象年月(To)
	  , @DepartmentCode nvarchar(3)						--部門コード
	  , @ServiceWorkCode nvarchar(5)					--主作業コード
	  , @SlipNumber nvarchar(50)						--伝票番号
	  , @Vin nvarchar(20)								--伝票番号
	  , @CustomerCode nvarchar(10)						--顧客コード
	  , @CustomerName nvarchar(80)						--顧客名
	  , @SupplierCode nvarchar(10)						--外注先コード
	  , @SupplierName nvarchar(80)						--外注先名
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	/*-------------------------------------------*/
	/* データ設定								 */
	/*-------------------------------------------*/
	
	SET @SQL = '' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  L.SupplierCode AS SupplierCode' + @CRLF								--外注先コード
	SET @SQL = @SQL +'	, S.SupplierName AS SupplierName' + @CRLF								--外注先名
	SET @SQL = @SQL +'	, H.SlipNumber AS SlipNumber' + @CRLF								--部門コード
	SET @SQL = @SQL +'	, W.Name AS ServiceWorkName' + @CRLF									--主作業名
    SET @SQL = @SQL +'	, L.LineContents AS LineContents' + @CRLF								--明細名称
    SET @SQL = @SQL +'	, L.TechnicalFeeAmount AS TechnicalFeeAmount' + @CRLF					--技術料
    SET @SQL = @SQL +'	, L.Cost AS Cost' + @CRLF												--原価
	SET @SQL = @SQL +'	, CC.CustomerClaimName AS CustomerClaimName' + @CRLF					--請求先名
	SET @SQL = @SQL +'	, C.CustomerName AS CustomerName' + @CRLF								--顧客名
	SET @SQL = @SQL +'	, C1.Name AS ServiceOrderStatusName' + @CRLF							--伝票ステータス名
	SET @SQL = @SQL +'	, H.DepartmentCode AS DepartmentCode' + @CRLF							--部門コード
	SET @SQL = @SQL +'	, D.DepartmentName AS DepartmentName' + @CRLF							--部門名
	SET @SQL = @SQL +'	, H.SalesDate AS SalesDate' + @CRLF										--納車日
	SET @SQL = @SQL +'	, H.SalesOrderDate AS SalesOrderDate' + @CRLF							--受注日
	SET @SQL = @SQL +'	, H.Vin AS Vin' + @CRLF													--車台番号
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	ServiceSalesHeader H INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	ServiceSalesLine L ON H.SlipNumber=L.SlipNumber and H.RevisionNumber=L.RevisionNumber INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	Department D ON H.DepartmentCode=D.DepartmentCode INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	c_ServiceOrderStatus C1 ON H.ServiceOrderStatus=C1.Code INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	Supplier S ON L.SupplierCode=S.SupplierCode LEFT OUTER JOIN' + @CRLF
	SET @SQL = @SQL +'	Customer C ON H.CustomerCode=C.CustomerCode LEFT OUTER JOIN' + @CRLF
	SET @SQL = @SQL +'	CustomerClaim CC ON L.CustomerClaimCode=CC.CustomerClaimCode LEFT OUTER JOIN' + @CRLF
	SET @SQL = @SQL +'	ServiceWork W ON L.ServiceWorkCode=W.ServiceWorkCode' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	H.DelFlag = ''0''' + @CRLF
	SET @SQL = @SQL +'	AND L.ServiceType=''002''' + @CRLF
	SET @SQL = @SQL +'	AND L.SupplierCode is not NULL' + @CRLF
	
	--検索条件で納車日が選択されている場合
	IF (@Target = '0')
	BEGIN
		SET @SQL = @SQL +' AND H.SalesDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
		SET @SQL = @SQL +' AND H.SalesDate <= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
	END
	--検索条件で受注日が選択されている場合
	ELSE
	BEGIN
		SET @SQL = @SQL +' AND H.SalesOrderDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
		SET @SQL = @SQL +' AND H.SalesOrderDate <= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
	END
	--検索条件で部門コードが入力されている場合
	IF (@DepartmentCode is not NULL AND @DepartmentCode <> '')
	BEGIN
		SET @SQL = @SQL +' AND H.DepartmentCode = ''' + @DepartmentCode + '''' + @CRLF
	END
	--検索条件で伝票番号が入力されている場合
	IF (@SlipNumber is not NULL AND @SlipNumber <> '')
	BEGIN
		SET @SQL = @SQL +' AND H.SlipNumber = ''' + @SlipNumber + '''' + @CRLF
	END
	--検索条件で車台番号が入力されている場合
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +' AND H.Vin LIKE ''%' + @Vin + '%''' + @CRLF
	END
	--検索条件で顧客コードが入力されている場合
	IF (@CustomerCode is not NULL AND @CustomerCode <> '')
	BEGIN
		SET @SQL = @SQL +' AND H.CustomerCode = ''' + @CustomerCode + '''' + @CRLF
	END
	--検索条件で顧客名が入力されている場合
	IF (@CustomerName is not NULL AND @CustomerName <> '')
	BEGIN
		SET @SQL = @SQL +'  AND C.CustomerName like ''%' + @CustomerName + '%''' + @CRLF
	END
	--検索条件で外注コードが入力されている場合
	IF (@SupplierCode is not NULL AND @SupplierCode <> '')
	BEGIN
		SET @SQL = @SQL +' AND L.SupplierCode = ''' + @SupplierCode + '''' + @CRLF
	END
	--検索条件で外注名が入力されている場合
	IF (@SupplierName is not NULL AND @SupplierName <> '')
	BEGIN
		SET @SQL = @SQL +'  AND S.SupplierName like ''%' + @SupplierName + '%''' + @CRLF
	END
	--検索条件で主作業が選択されている場合
	IF (@ServiceWorkCode is not NULL AND @ServiceWorkCode <> '')
	BEGIN
		SET @SQL = @SQL +' AND L.ServiceWorkCode = ''' + @ServiceWorkCode + '''' + @CRLF
	END

	--ソート
	SET @SQL = @SQL +'ORDER BY ' + @CRLF
	SET @SQL = @SQL +'	 L.SupplierCode ' + @CRLF
	SET @SQL = @SQL +'	,H.DepartmentCode ' + @CRLF
	SET @SQL = @SQL +'	,H.SlipNumber ' + @CRLF
	SET @SQL = @SQL +'	,L.LineNumber ' + @CRLF


	--DEBUG
	--PRINT @SQL

	EXECUTE sp_executeSQL @SQL
				
/*
	SELECT
	  CONVERT(nvarchar(10), '') AS SupplierCode
	, CONVERT(nvarchar(80), NULL) AS SupplierName
	, CONVERT(nvarchar(50), NULL) AS SlipNumber
	, CONVERT(nvarchar(20), NULL) AS ServiceWorkName
	, CONVERT(nvarchar(50), NULL) AS LineContents
	, CONVERT(decimal(10, 0), NULL) AS TechnicalFeeAmount
	, CONVERT(decimal(10, 0), NULL) AS Cost
	, CONVERT(nvarchar(80), NULL) AS CustomerClaimName
	, CONVERT(nvarchar(80), NULL) AS CustomerName
	, CONVERT(nvarchar(50), NULL) AS ServiceOrderStatusName
	, CONVERT(nvarchar(3), NULL) AS DepartmentCode
	, CONVERT(nvarchar(20), NULL) AS DepartmentName
	, CONVERT(datetime, NULL) AS SalesDate
	, CONVERT(datetime, NULL) AS SalesOrderDate
	, CONVERT(nvarchar(20), NULL) AS Vin
*/
END


GO
		--更新履歴テーブルに登録
		INSERT INTO [dbo].[DB_ReleaseHistory]
				([HistoryID]
				,[TicketNumber]
				,[QueryName]
				,[ReleaseDate]
				,[Summary]
				,[ExecEmployeeCode]
				,[ExecDate])
		VALUES
			(NEWID()
			,'3729'--チケット番号
			,'20170323_#3729_サブシステム移行（外注支払一覧）/04_Create_Procedure_GetSupplierPaymentList.sql'
			,CONVERT(datetime, '2017/03/??', 120)		--※※※※※※※※※※リリース日(WPH様入力)※※※※※※※※※※※
			,''--コメント
			,'arima.yuji'--実行者
			,GETDATE()--実行日
		)




