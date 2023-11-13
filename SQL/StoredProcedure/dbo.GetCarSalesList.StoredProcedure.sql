USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCarSalesList]    Script Date: 2017/03/29 13:14:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





--2017/03/09 arc nakayama #3723_�[�ԃ��X�g�@�V�K�쐬

CREATE PROCEDURE [dbo].[GetCarSalesList]

	@SelectYear nvarchar(4),		--�N�x�iYYYY�j
	@SalesType nvarchar(1) = '0'	--�̔��敪

AS
	SET NOCOUNT ON

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)
	
	/*-------------------------------------------*/
	/* �_�[�e�B�[���[�h�̐ݒ�					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/* �����N�x�ݒ�								 */
	/*-------------------------------------------*/
	DECLARE @StartMonth nvarchar(2) = '7'	--����ݒ�
	DECLARE @StartDate datetime = CONVERT(datetime, @SelectYear + '/' + @StartMonth + '/01', 120) --�w��N�x�̊����ݒ�
	DECLARE @EndDate datetime = DATEADD(DAY, -1, DATEADD(YEAR, 1, @StartDate)) --�w��N�x�̊�����ݒ�

	/*-------------------------------------------*/
	/* ����Ō����̔[�Ԑ����擾					 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_SalesCarList1(
		DepartmentCode nvarchar(3),
		DepartmentName nvarchar(20),
		SetYear nvarchar(4),
		SetMonth nvarchar(2),
		DataCnt int
	)

	SET @PARAM = '@StartDate datetime, @EndDate datetime'

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_SalesCarList1' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	 H.DepartmentCode' + @CRLF
	SET @SQL = @SQL +'	,D.DepartmentName' + @CRLF
	SET @SQL = @SQL +'	,Year(H.SalesDate) AS SetYear ' + @CRLF
	SET @SQL = @SQL +'	,MONTH(H.SalesDate) as SetMonth' + @CRLF
	SET @SQL = @SQL +'	,Count(*) as DataCnt' + @CRLF
	SET @SQL = @SQL +'FROM CarSalesHeader H' + @CRLF
	SET @SQL = @SQL +'INNER JOIN Department D on H.DepartmentCode = D.DepartmentCode' + @CRLF
	SET @SQL = @SQL +'WHERE h.DelFlag=''0'' ' + @CRLF
	SET @SQL = @SQL +'  AND H.SalesOrderStatus in (''005'')' + @CRLF
	SET @SQL = @SQL +'  AND H.SalesDate between @StartDate and @EndDate' + @CRLF

	--�̔��敪
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

	SET @SQL = @SQL +' GROUP BY H.DepartmentCode,D.DepartmentName,Year(H.SalesDate),MONTH(H.SalesDate)' + @CRLF
	SET @SQL = @SQL +'ORDER BY h.DepartmentCode,Year(H.SalesDate),MONTH(H.SalesDate)' + @CRLF


	EXECUTE sp_executeSQL @SQL, @PARAM,@StartDate, @EndDate

	/*-------------------------------------------*/
	/* �������ӂɂ���							 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_SalesCarList2(
		DepartmentCode nvarchar(3),
		DepartmentName nvarchar(20),

	)

	SET @PARAM = ''
	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_SalesCarList2' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	 DepartmentCode' + @CRLF
	SET @SQL = @SQL +'	,DepartmentName' + @CRLF
	SET @SQL = @SQL +'FROM #temp_SalesCarList1' + @CRLF
	SET @SQL = @SQL +'GROUP BY DepartmentCode, DepartmentName' + @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM

	/*-------------------------------------------*/
	/* ���񂩂�����܂ł�1���R�[�h�ɂ܂Ƃ߂�	 */
	/*-------------------------------------------*/


	CREATE TABLE #temp_SalesCarList3(
		DepartmentCode nvarchar(3),
		DepartmentName nvarchar(20),
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
	
		INSERT INTO #temp_SalesCarList3
		SELECT
		  L2.DepartmentCode
		 ,L2.DepartmentName
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
	FROM #temp_SalesCarList2 L2
	INNER JOIN #temp_SalesCarList1 AS L1 ON L1.DepartmentCode = L2.DepartmentCode

	SELECT 
		 DepartmentCode
		,DepartmentName
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
	FROM #temp_SalesCarList3
	GROUP BY DepartmentCode, DepartmentName
	ORDER BY DepartmentCode

BEGIN
	DROP TABLE #temp_SalesCarList1
	DROP TABLE #temp_SalesCarList2
	DROP TABLE #temp_SalesCarList3
END
GO


