USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsStatus]    Script Date: 2017/11/07 16:29:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/16 arc yano #3726 サブシステム移行(パーツステータス) 新規作成
-- Update date: <Update Date,,>
-- 2017/11/06 arc yano #3809 パーツステータス管理　引当済数の追加
-- Description:	<Description,,>
-- 整備履歴の取得
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetPartsStatus] 
	  @Target nvarchar(1)							--検索対象項目(0:指定無し 1:入庫日 2:作業開始日 3:納車日)
	, @TargetDateFrom datetime						--対象年月From
	, @DepartmentCode nvarchar(3)					--部門コード
	, @ServiceOrderStatus nvarchar(3)				--伝票ステータス
	, @PartsNumber nvarchar(25)						--部品番号
AS 
BEGIN

--/*

	SET NOCOUNT ON;

	
	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @TableNameLine nvarchar(50)			--検索するテーブル名（明細

	--対象年月Toを対象年月Fromの1ヶ月後に設定
	DECLARE @TargetDateTo datetime = DATEADD(m, 1, @TargetDateFrom)		--対象年月To
		


	/*-------------------------------------------*/
	/* データ取得								 */
	/*-------------------------------------------*/
	SET @SQL = ''
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  H.SlipNumber AS SlipNumber' + @CRLF
	SET @SQL = @SQL +'	, H.ServiceOrderStatus AS ServiceOrderStatus' + @CRLF
	SET @SQL = @SQL +'	, Left(c.Name,5) AS ServiceOrderStatusName' + @CRLF
	SET @SQL = @SQL +'	, L.PartsNumber AS PartsNumber' + @CRLF
	SET @SQL = @SQL +'	, L.LineContents AS LineContents' + @CRLF
	SET @SQL = @SQL +'	, L.Quantity AS Quantity' + @CRLF
	SET @SQL = @SQL +'	, L.ProvisionQuantity AS ProvisionQuantity' + @CRLF			--Add 2017/11/06 arc yano #3809
	SET @SQL = @SQL +'	, H.ArrivalPlanDate AS ArrivalPlanDate' + @CRLF
	SET @SQL = @SQL +'	, H.SalesOrderDate AS SalesOrderDate' + @CRLF
	SET @SQL = @SQL +'	, H.WorkingStartDate AS WorkingStartDate' + @CRLF
	SET @SQL = @SQL +'	, H.WorkingEndDate AS WorkingEndDate' + @CRLF
	SET @SQL = @SQL +'	, H.SalesDate AS SalesDate' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	ServiceSalesHeader H INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	ServiceSalesLine L ON H.SlipNumber=L.SlipNumber AND H.RevisionNumber=L.RevisionNumber INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	c_ServiceOrderStatus C ON H.ServiceOrderStatus = C.Code' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	H.DelFlag = ''0''' + @CRLF
	SET @SQL = @SQL +'	AND L.DelFlag = ''0''' + @CRLF
	SET @SQL = @SQL +'	AND L.ServiceType = ''003''' + @CRLF

	--日付の絞込み
	IF (@Target is not null AND @Target <> '')
	BEGIN
		IF (@Target =  '1')				--入庫日
		BEGIN
			SET @SQL = @SQL +'	AND H.ArrivalPlanDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
			SET @SQL = @SQL +'	AND H.ArrivalPlanDate < CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
		END
		ELSE IF (@Target =  '2')				--受注日
		BEGIN
			SET @SQL = @SQL +'	AND H.SalesOrderDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
			SET @SQL = @SQL +'	AND H.SalesOrderDate < CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
		END
		ELSE IF(@Target =  '3')			--作業開始日
		BEGIN
			SET @SQL = @SQL +'	AND H.WorkingStartDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
			SET @SQL = @SQL +'	AND H.WorkingStartDate < CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
		END
		ELSE IF(@Target = '4')			--作業終了日
		BEGIN
			SET @SQL = @SQL +'	AND H.WorkingEndDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
			SET @SQL = @SQL +'	AND H.WorkingEndDate < CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
		END
		ELSE IF(@Target = '5')			--納車日
		BEGIN
			SET @SQL = @SQL +'	AND H.SalesDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
			SET @SQL = @SQL +'	AND H.SalesDate < CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
		END
	END

	--部門コードによる絞込み
	IF (@DepartmentCode is not null AND @DepartmentCode <> '')
	BEGIN
		SET @SQL = @SQL +'	AND H.DepartmentCode = ''' + @DepartmentCode + '''' + @CRLF
	END

	--伝票ステータスによる絞込み
	IF (@ServiceOrderStatus is not null AND @ServiceOrderStatus <> '')
	BEGIN
		SET @SQL = @SQL +'	AND H.ServiceOrderStatus = ''' + @ServiceOrderStatus + '''' + @CRLF
	END

	--部品番号による絞込み
	IF (@PartsNumber is not null AND @PartsNumber <> '')
	BEGIN
		SET @SQL = @SQL +'	AND L.PartsNumber = ''' + @PartsNumber + '''' + @CRLF
	END

	SET @SQL = @SQL +'	ORDER BY' + @CRLF
	SET @SQL = @SQL +'	H.SlipNumber, L.LineNumber' + @CRLF

	EXECUTE sp_executeSQL @SQL
--*/
/*

	SELECT
		  CONVERT(nvarchar(50), '') AS SlipNumber
		, CONVERT(nvarchar(3), NULL) AS ServiceOrderStatus
		, CONVERT(varchar(50), NULL) AS ServiceOrderStatusName
		, CONVERT(nvarchar(25), NULL) AS PartsNumber
		, CONVERT(nvarchar(50), NULL) AS LineContents
		, CONVERT(decimal(10, 2), NULL) AS Quantity
		, CONVERT(decimal(10, 2), NULL) AS ProvisionQuantity
		, CONVERT(datetime, NULL) AS ArrivalPlanDate
		, CONVERT(datetime, NULL) AS SalesOrderDate
		, CONVERT(datetime, NULL) AS WorkingStartDate
		, CONVERT(datetime, NULL) AS WorkingEndDate
		, CONVERT(datetime, NULL) AS SalesDate
*/		

END




GO


