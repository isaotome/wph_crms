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
-- 車両基本情報の取得
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetCarBasicInfo] 
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
	
	SET @SQL = ''
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  S.SalesCarNumber AS SalesCarNumber' + @CRLF										--管理番号
	SET @SQL = @SQL +'	, S.Vin AS Vin' + @CRLF																--車台番号
	SET @SQL = @SQL +'	, C1.Name as NewUsedName' + @CRLF													--新中区分
    SET @SQL = @SQL +'	, L.LocationName AS LocationName' + @CRLF											--ロケーション名
    SET @SQL = @SQL +'	, C2.Name AS CarStatusName' + @CRLF													--在庫ステータス
    SET @SQL = @SQL +'	, W.MakerName AS MakerName' + @CRLF													--メーカー名
	SET @SQL = @SQL +'	, W.CarBrandName AS CarBrandNam' + @CRLF											--ブランド名
	SET @SQL = @SQL +'	, W.CarName AS CarName' + @CRLF														--車種名
	SET @SQL = @SQL +'	, S.PossesorName AS PossesorName' + @CRLF											--所有者
	SET @SQL = @SQL +'	, S.UserName AS UserName' + @CRLF													--使用者
	SET @SQL = @SQL +'	, CASE WHEN S.DelFlag=''0'' THEN ''有効'' ELSE ''無効'' END AS DelName' + @CRLF		--有効／無効

	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	SalesCar S LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Location L ON S.LocationCode=L.LocationCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_NewUsedType C1 ON S.NewUsedType=C1.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_CarStatus C2 ON S.CarStatus=C2.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	V_CarMaster W ON S.CarGradeCode=W.CarGradeCode' + @CRLF
	
	--検索条件で車台番号が入力されている場合
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    S.Vin like ''%' + @Vin + '%''' + @CRLF
	END
	--検索条件で管理番号が選択されている場合
	ELSE IF (@SalesCarNumber is not NULL AND @SalesCarNumber <> '')
	BEGIN
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    S.SalesCarNumber like ''%' + @SalesCarNumber + '%''' + @CRLF
	END

	EXECUTE sp_executeSQL @SQL

	--DEBUG
	--PRINT @SQL

/*
	SELECT
	  CONVERT(nvarchar(50), '') AS SalesCarNumber
	, CONVERT(nvarchar(20), NULL) AS Vin
	, CONVERT(varchar(50), NULL) AS NewUsedName
	, CONVERT(nvarchar(50), NULL) AS LocationName
	, CONVERT(varchar(50), NULL) AS CarStatusName
	, CONVERT(nvarchar(50), NULL) AS MakerName
	, CONVERT(nvarchar(50), NULL) AS CarBrandName
	, CONVERT(nvarchar(20), NULL) AS CarName
	, CONVERT(nvarchar(80), NULL) AS PossesorName
	, CONVERT(nvarchar(80), NULL) AS UserName
	, CONVERT(nvarchar(5), NULL) AS DelName
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
			,'20170319_#3721_サブシステム移行（車両追跡）/04_Create_Procedure_GetCarBasicInfo.sql'
			,CONVERT(datetime, '2017/03/??', 120)		--※※※※※※※※※※リリース日(WPH様入力)※※※※※※※※※※※
			,''--コメント
			,'arima.yuji'--実行者
			,GETDATE()--実行日
		)




