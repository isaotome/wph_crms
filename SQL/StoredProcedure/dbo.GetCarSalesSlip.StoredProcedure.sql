USE [WPH_DB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/19 arc yano #3721 サブシステム移行(車両追跡) 新規作成
-- Description:	<Description,,>
-- 車両ステータス遷移情報の取得
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetCarSalesSlip] 
	    @SalesCarNumber nvarchar(50) = ''			--車両管理番号
	  , @Vin nvarchar(20) = ''						--車台番号
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	/*-------------------------------------------*/
	/* データ取得								 */
	/*-------------------------------------------*/
	--仕入情報
	SET @SQL = ''
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  H.SlipNumber AS SlipNumber' + @CRLF																					--伝票番号
	SET @SQL = @SQL +'	, H.SalesCarNumber AS SalesCarNumber' + @CRLF																			--管理番号
	SET @SQL = @SQL +'	, C1.Name AS SalesOrderStatusName' + @CRLF																				--伝票ステータス名
	SET @SQL = @SQL +'	, E.EmployeeName AS EmployeeName' + @CRLF																				--担当者名
	SET @SQL = @SQL +'	, C.CustomerName AS CustomerName' + @CRLF																				--顧客名
	SET @SQL = @SQL +'	, D.DepartmentName' + @CRLF																								--部門名											--
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	CarSalesHeader H LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Department D on H.DepartmentCode=D.DepartmentCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Employee E on H.EmployeeCode=E.EmployeeCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_SalesOrderStatus C1 on H.SalesOrderStatus=C1.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Customer C on H.CustomerCode=C.CustomerCode' + @CRLF
	

	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	H.DelFlag=''0''' + @CRLF

	--検索条件の車台番号が入力されていた場合
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	EXISTS' + @CRLF
		SET @SQL = @SQL +'	(' + @CRLF
		SET @SQL = @SQL +'		SELECT ''x'' FROM SalesCar S WHERE S.Vin =''' + @Vin + '''' + 'AND H.SalesCarNumber = S.SalesCarNumber' + @CRLF
		SET @SQL = @SQL +'	)' + @CRLF
	END
	ELSE IF(@SalesCarNumber is not NULL AND @SalesCarNumber <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	H.SalesCarNumber = '''  + @SalesCarNumber + '''' + @CRLF
	END

	EXECUTE sp_executeSQL @SQL
	
	--DEBUG
	--PRINT @SQL

	/*
	SELECT
	  CONVERT(nvarchar(50), '') AS SlipNumber
	, CONVERT(nvarchar(20), NULL) AS SalesCarNumber
	, CONVERT(varchar(50), NULL) AS SalesOrderStatusName
	, CONVERT(nvarchar(40), NULL) AS EmployeeName
	, CONVERT(varchar(80), NULL) AS CustomerName
	, CONVERT(varchar(20), NULL) AS DepartmentName
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
			,'3721'--チケット番号
			,'20170319_#3721_サブシステム移行（車両追跡）/06_Create_Procedure_GetCarSalesSlip.sql'
			,CONVERT(datetime, '2017/03/19', 120)		--※※※※※※※※※※リリース日(WPH様入力)※※※※※※※※※※※
			,''--コメント
			,'arima.yuji'--実行者
			,GETDATE()--実行日
		)




