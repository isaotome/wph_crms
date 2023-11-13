USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCarOptionMaster]    Script Date: 2016/03/07 16:32:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- 2016/02/09 arc nakayama 車両伝票作成時のオプションのデフォルト設定 新規作成
-- 2016/03/07 arc nakayama 車両伝票作成時のオプションのデフォルト設定 車種コードからグレードコードに変更

CREATE PROCEDURE [dbo].[GetCarOptionMaster]

	@MakerCode nvarchar(5),			--メーカーコード
	@MakerName nvarchar(50),		--メーカー名
	@CarOptionCode nvarchar(25),	--オプションコード
	@CarOptionName nvarchar(100),	--オプション名
	@CarGradeCode nvarchar(30),		--グレードコード
	@RequiredFlag nvarchar(2),		--必須フラグ
	@DelFlag nvarchar(2),			--削除フラグ
	@ActionFlag nvarchar(2)			--アクションフラグ（0:マスタ画面検索　1:ダイアログ検索）

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
	/* メーカー情報								 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_Maker_M (
		  MakerCode nvarchar(5)
		, MakerName nvarchar(50)

	)

	SET @PARAM = '@MakerName nvarchar(50)'

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_Maker_M' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  M.MakerCode' + @CRLF
	SET @SQL = @SQL +'	, M.MakerName' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.Maker AS M' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    M.DelFlag = ''0''' + @CRLF

	--メーカー名
	IF ((@MakerName IS NOT NULL) AND (@MakerName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND M.MakerName LIKE ''%' + @MakerName + '%''' + @CRLF
	END

	EXECUTE sp_executeSQL @SQL, @PARAM, @MakerName
	CREATE INDEX ix_temp_Maker_M ON #temp_Maker_M(MakerCode)

	/*-------------------------------------------*/
	/* 車種情報									 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_CarGrade_C(
		  CarGradeCode nvarchar(30)
		, CarGradeName nvarchar(50)
	)

	SET @PARAM = '@CarGradeCode nvarchar(30)'

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_CarGrade_C' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  C.CarGradeCode' + @CRLF
	SET @SQL = @SQL +'	, C.CarGradeName' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.CarGrade AS C' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    C.DelFlag = ''0''' + @CRLF 

	--車種コード
	IF ((@CarGradeCode IS NOT NULL) AND (@CarGradeCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND C.CarGradeCode = @CarGradeCode' + @CRLF
	END	

	EXECUTE sp_executeSQL @SQL, @PARAM, @CarGradeCode
	CREATE INDEX ix_temp_Car_C ON #temp_CarGrade_C(CarGradeCode)

	/*-------------------------------------------*/
	/* 車両オプション検索						 */
	/*-------------------------------------------*/

	SET @PARAM = '@MakerCode nvarchar(5), @CarOptionCode nvarchar(25), @CarOptionName nvarchar(100), @CarGradeCode nvarchar(30), @RequiredFlag nvarchar(2), @DelFlag nvarchar(2)'

	SET @SQL = ''
	SET @SQL = @SQL + 'SELECT'+ @CRLF
	SET @SQL = @SQL + '   OP.MakerCode AS MakerCode'+ @CRLF
	SET @SQL = @SQL + ' , M.MakerName AS MakerName'+ @CRLF
	SET @SQL = @SQL + ' , OP.CarOptionCode AS CarOptionCode'+ @CRLF
	SET @SQL = @SQL + ' , OP.CarOptionName AS CarOptionName'+ @CRLF
	SET @SQL = @SQL + ' , OP.OptionType AS OptionType'+ @CRLF
	SET @SQL = @SQL + ' , ISNULL(C.CarGradeName, ''共通'') AS CarGradeName'+ @CRLF
	SET @SQL = @SQL + ' , OP.RequiredFlag'+ @CRLF
	SET @SQL = @SQL + ' , OP.DelFlag AS DelFlag'+ @CRLF

    SET @SQL = @SQL + 'FROM dbo.CarOption AS OP'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CarGrade_C AS C ON C.CarGradeCode = OP.CarGradeCode'+ @CRLF
	SET @SQL = @SQL + 'INNER JOIN #temp_Maker_M AS M ON M.MakerCode = OP.MakerCode'+ @CRLF

	SET @SQL = @SQL + 'WHERE 1 = 1'+ @CRLF		

	--メーカーコード
	IF ((@MakerCode IS NOT NULL) AND (@MakerCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND M.MakerCode LIKE ''%' + @MakerCode + '%''' + @CRLF
	END

	--オプションコード
	IF ((@CarOptionCode IS NOT NULL) AND (@CarOptionCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND OP.CarOptionCode LIKE ''%' + @CarOptionCode + '%''' + @CRLF
	END

	--オプション名
	IF ((@CarOptionName IS NOT NULL) AND (@CarOptionName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND OP.CarOptionName LIKE ''%' + @CarOptionName + '%''' + @CRLF
	END

	IF @ActionFlag = '0'
	BEGIN
		--車種コード
		IF ((@CarGradeCode IS NOT NULL) AND (@CarGradeCode <> ''))
		BEGIN
			SET @SQL = @SQL +'AND OP.CarGradeCode = @CarGradeCode' + @CRLF
		END
	END
	ELSE
	BEGIN
		--車種コード
		IF ((@CarGradeCode IS NOT NULL) AND (@CarGradeCode <> ''))
		BEGIN
			SET @SQL = @SQL +'AND OP.CarGradeCode in (@CarGradeCode, '''')' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND OP.CarGradeCode = ''''' + @CRLF
		END
	END
		
	--必須フラグ
	IF ((@RequiredFlag IS NOT NULL) AND (@RequiredFlag <> ''))
	BEGIN
		SET @SQL = @SQL +'AND OP.RequiredFlag = @RequiredFlag' + @CRLF
	END	

	--削除フラグ
	IF ((@DelFlag IS NOT NULL) AND (@DelFlag <> ''))
	BEGIN
		SET @SQL = @SQL +'AND OP.DelFlag = @DelFlag' + @CRLF
	END

	SET @SQL = @SQL + 'ORDER BY OP.MakerCode, OP.CarOptionCode'+ @CRLF


	EXECUTE sp_executeSQL @SQL, @PARAM, @MakerCode, @CarOptionCode, @CarOptionName, @CarGradeCode ,@RequiredFlag, @DelFlag


BEGIN

	BEGIN TRY
		DROP TABLE #temp_Maker_M
		DROP TABLE #temp_CarGrade_C
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

END





GO


