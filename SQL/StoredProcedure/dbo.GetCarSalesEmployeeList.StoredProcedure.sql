USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCarSalesEmployeeList]    Script Date: 2017/03/29 13:15:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





--2017/03/09 arc nakayama #3723_納車リスト　新規作成

CREATE PROCEDURE [dbo].[GetCarSalesEmployeeList]
	@SelectYear nvarchar(4),		--年度（YYYY）
	@DepartmentCode nvarchar(3),	--部門コード
	@SalesType nvarchar(1) = '0'	--販売区分

AS
	SET NOCOUNT ON

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)
	
	/*-------------------------------------------*/
	/* ダーティーリードの設定					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/* 検索年度設定								 */
	/*-------------------------------------------*/
	DECLARE @StartMonth nvarchar(2) = '7'	--期首設定
	DECLARE @StartDate datetime = CONVERT(datetime, @SelectYear + '/' + @StartMonth + '/01', 120) --指定年度の期首を設定
	DECLARE @EndDate datetime = DATEADD(DAY, -1, DATEADD(YEAR, 1, @StartDate)) --指定年度の期末を設定

	/*-------------------------------------------*/
	/* 部門で月毎の納車数を取得					 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_SalesCarEmployeeList1(
		EmployeeNumber nvarchar(20),
		EmployeeCode nvarchar(50),
		EmployeeName nvarchar(40),
		SetYear nvarchar(4),
		SetMonth nvarchar(2),
		DataCnt int
	)

	SET @PARAM = '@StartDate datetime, @EndDate datetime, @DepartmentCode nvarchar(3)'

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_SalesCarEmployeeList1' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	 E.EmployeeNumber' + @CRLF
	SET @SQL = @SQL +'	,H.EmployeeCode' + @CRLF
	SET @SQL = @SQL +'	,E.EmployeeName' + @CRLF
	SET @SQL = @SQL +'	,Year(H.SalesDate) AS SetYear ' + @CRLF
	SET @SQL = @SQL +'	,MONTH(H.SalesDate) as SetMonth' + @CRLF
	SET @SQL = @SQL +'	,Count(*) as DataCnt' + @CRLF

	SET @SQL = @SQL +'FROM CarSalesHeader H' + @CRLF
	SET @SQL = @SQL +'INNER JOIN Employee E on H.EmployeeCode = E.EmployeeCode' + @CRLF
	SET @SQL = @SQL +'WHERE h.DelFlag=''0'' ' + @CRLF
	SET @SQL = @SQL +'  AND H.SalesOrderStatus in (''005'')' + @CRLF
	SET @SQL = @SQL +'  AND H.SalesDate between @StartDate and @EndDate' + @CRLF

	--販売区分
	if(@SalesType = '1')
	BEGIN
		SET @SQL = @SQL +'and H.SalesType in (''001'',''002'')' + @CRLF
	END

	if(@SalesType = '2')
	BEGIN
		SET @SQL = @SQL +'and H.SalesType in (''003'',''004'',''009'')' + @CRLF
	END

	if(@SalesType = '3')
	BEGIN
		SET @SQL = @SQL +'and H.SalesType in (''005'',''006'')' + @CRLF
	END

	if(@SalesType = '4')
	BEGIN
		SET @SQL = @SQL +'and H.SalesType in (''007'',''008'')' + @CRLF
	END

	IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND H.DepartmentCode = @DepartmentCode'+ @CRLF
		END

	SET @SQL = @SQL +' GROUP BY E.EmployeeNumber,H.EmployeeCode,E.EmployeeName,Year(H.SalesDate),MONTH(H.SalesDate)' + @CRLF
	SET @SQL = @SQL +'ORDER BY E.EmployeeNumber,Year(H.SalesDate),MONTH(H.SalesDate)' + @CRLF


	EXECUTE sp_executeSQL @SQL, @PARAM,@StartDate, @EndDate, @DepartmentCode

	/*-------------------------------------------*/
	/* 部門を一意にする							 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_SalesCarEmployeeList2(
		EmployeeNumber nvarchar(20),
		EmployeeCode nvarchar(50),
		EmployeeName nvarchar(40),

	)

	SET @PARAM = ''
	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_SalesCarEmployeeList2' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	 EmployeeNumber' + @CRLF
	SET @SQL = @SQL +'	,EmployeeCode' + @CRLF
	SET @SQL = @SQL +'	,EmployeeName' + @CRLF
	SET @SQL = @SQL +'FROM #temp_SalesCarEmployeeList1' + @CRLF
	SET @SQL = @SQL +'GROUP BY EmployeeNumber, EmployeeCode, EmployeeName' + @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM

	/*-------------------------------------------*/
	/* 期首から期末までを1レコードにまとめる	 */
	/*-------------------------------------------*/


	CREATE TABLE #temp_SalesCarEmployeeList3(
		EmployeeNumber nvarchar(20),
		EmployeeCode nvarchar(50),
		EmployeeName nvarchar(40),
		Jul_Cnt int,
		Aug_Cnt int,
		Sep_Cnt int,
		Oct_Cnt int,
		Nov_Cnt int,
		Dec_Cnt int,
		Jan_Cnt int,
		Feb_Cnt int,
		Mar_Cnt int,
		Apr_Cnt int,
		May_Cnt int,
		Jun_Cnt int,
		)
	
		INSERT INTO #temp_SalesCarEmployeeList3
		SELECT
		  L2.EmployeeNumber
		 ,L2.EmployeeCode
		 ,L2.EmployeeName
		 ,CASE WHEN L1.SetYear = YEAR(@StartDate) AND L1.SetMonth = 7 THEN L1.DataCnt ELSE NULL END
		 ,CASE WHEN L1.SetYear = YEAR(@StartDate) AND L1.SetMonth = 8 THEN L1.DataCnt ELSE NULL END
		 ,CASE WHEN L1.SetYear = YEAR(@StartDate) AND L1.SetMonth = 9 THEN L1.DataCnt ELSE NULL END
		 ,CASE WHEN L1.SetYear = YEAR(@StartDate) AND L1.SetMonth = 10 THEN L1.DataCnt ELSE NULL END
		 ,CASE WHEN L1.SetYear = YEAR(@StartDate) AND L1.SetMonth = 11 THEN L1.DataCnt ELSE NULL END
		 ,CASE WHEN L1.SetYear = YEAR(@StartDate) AND L1.SetMonth = 12 THEN L1.DataCnt ELSE NULL END
		 ,CASE WHEN L1.SetYear = YEAR(@EndDate) AND L1.SetMonth = 1 THEN L1.DataCnt ELSE NULL END
		 ,CASE WHEN L1.SetYear = YEAR(@EndDate) AND L1.SetMonth = 2 THEN L1.DataCnt ELSE NULL END
		 ,CASE WHEN L1.SetYear = YEAR(@EndDate) AND L1.SetMonth = 3 THEN L1.DataCnt ELSE NULL END
		 ,CASE WHEN L1.SetYear = YEAR(@EndDate) AND L1.SetMonth = 4 THEN L1.DataCnt ELSE NULL END
		 ,CASE WHEN L1.SetYear = YEAR(@EndDate) AND L1.SetMonth = 5 THEN L1.DataCnt ELSE NULL END
		 ,CASE WHEN L1.SetYear = YEAR(@EndDate) AND L1.SetMonth = 6 THEN L1.DataCnt ELSE NULL END
	FROM #temp_SalesCarEmployeeList2 L2
	INNER JOIN #temp_SalesCarEmployeeList1 AS L1 ON L1.EmployeeCode = L2.EmployeeCode

	SELECT 
		 EmployeeNumber
		,EmployeeCode
		,EmployeeName
		,MAX(Jul_Cnt) AS 'Jul_Cnt'
		,MAX(Aug_Cnt) AS 'Aug_Cnt'
		,MAX(Sep_Cnt) AS 'Sep_Cnt'
		,MAX(Oct_Cnt) AS 'Oct_Cnt'
		,MAX(Nov_Cnt) AS 'Nov_Cnt'
		,MAX(Dec_Cnt) AS 'Dec_Cnt'
		,MAX(Jan_Cnt) AS 'Jan_Cnt'
		,MAX(Feb_Cnt) AS 'Feb_Cnt'
		,MAX(Mar_Cnt) AS 'Mar_Cnt'
		,MAX(Apr_Cnt) AS 'Apr_Cnt'
		,MAX(May_Cnt) AS 'May_Cnt'
		,MAX(Jun_Cnt) AS 'Jun_Cnt'
	FROM #temp_SalesCarEmployeeList3
	GROUP BY EmployeeNumber, EmployeeCode,EmployeeName
	ORDER BY EmployeeNumber

BEGIN
	DROP TABLE #temp_SalesCarEmployeeList1
	DROP TABLE #temp_SalesCarEmployeeList2
	DROP TABLE #temp_SalesCarEmployeeList3
END
GO


