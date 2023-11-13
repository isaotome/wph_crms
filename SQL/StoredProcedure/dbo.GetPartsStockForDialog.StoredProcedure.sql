USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsStockForDialog]    Script Date: 2016/10/03 15:35:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--2016/08/13 arc yano #3596 【大項目】部門棚統合対応 検索条件(部門コード)は不要のため、廃止
CREATE PROCEDURE [dbo].[GetPartsStockForDialog]
	@MakerCode nvarchar(5),			--メーカーコード
	@MakerName nvarchar(50),		--メーカー名
	@CarBrandCode nvarchar(30),		--ブランドコード
	@CarBrandName nvarchar(50),		--ブランド名
	@PartsNumber nvarchar(25),		--パーツナンバー
	@PartsNameJp nvarchar(50),		--パーツ名
	@DepartmentCode nvarchar(3),	--部門コード
	@SupplierCode nvarchar(10)		--仕入先コード

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
	/* ロケーションテーブル						 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_Location (
	     LocationCode nvarchar(12) NOT NULL			--ロケーションコード
	   , WarehouseCode nvarchar(6) NOT NULL			--倉庫コード
	   , LocationType nvarchar(3) NOT NULL			--ロケーション種別
		)

	INSERT INTO #temp_Location
	SELECT
		 l.LocationCode								--ロケーションコード
		,l.WarehouseCode							--倉庫コード
		,l.LocationType								--ロケーション種別
	FROM
		dbo.Location l
	WHERE
		EXISTS
		(
			SELECT 'X' FROM dbo.DepartmentWarehouse dw WHERE dw.DepartmentCode = @DepartmentCode AND dw.DelFlag = '0' AND l.WarehouseCode = dw.WarehouseCode
		)
	CREATE INDEX ix_temp_Location ON #temp_Location(LocationCode, WarehouseCode)


	/*-------------------------------------------*/
	/* メーカー情報								 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_Maker_M (
		  MakerCode nvarchar(5)
		, MakerName nvarchar(50)

	)

	SET @PARAM = '@CarBrandCode nvarchar(30), @CarBrandName nvarchar(50), @MakerCode nvarchar(5), @MakerName nvarchar(50)'

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_Maker_M' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  M.MakerCode' + @CRLF
	SET @SQL = @SQL +'	, M.MakerName' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.Maker AS M' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    M.DelFlag = ''0''' + @CRLF
	
	
	--ブランドコード
	IF ((@CarBrandCode IS NOT NULL) AND (@CarBrandCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND EXISTS(SELECT 1 FROM dbo.Brand AS B WHERE B.DelFlag = ''0'' AND B.CarBrandCode LIKE ''%' + @CarBrandCode + '%'' AND B.MakerCode = M.MakerCode)' + @CRLF
	END

	--ブランド名
	IF ((@CarBrandName IS NOT NULL) AND (@CarBrandName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND EXISTS(SELECT 1 FROM dbo.Brand AS B WHERE B.DelFlag = ''0'' AND B.CarBrandName LIKE ''%' + @CarBrandName + '%'' AND B.MakerCode = M.MakerCode)' + @CRLF
	END
		
	--メーカーコード
	IF ((@MakerCode IS NOT NULL) AND (@MakerCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND M.MakerCode LIKE ''%' + @MakerCode + '%''' + @CRLF
	END

	--メーカー名
	IF ((@MakerName IS NOT NULL) AND (@MakerName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND M.MakerName LIKE ''%' + @MakerName + '%''' + @CRLF
	END

	SET @SQL = @SQL +'GROUP BY M.MakerCode, M.MakerName' + @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM, @CarBrandCode, @CarBrandName, @MakerCode, @MakerName
	CREATE INDEX ix_temp_Brand_B ON #temp_Maker_M(MakerCode)




	/*-------------------------------------------*/
	/* 部品情報									 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_Parts_P (
		  PartsNumber nvarchar(25)
		, PartsNameJp nvarchar(50)
		, MakerCode nvarchar(5)
	)

	SET @PARAM = '@PartsNumber nvarchar(25), @PartsNameJp nvarchar(50), @MakerName nvarchar(5)'

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_Parts_P' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  P.PartsNumber' + @CRLF
	SET @SQL = @SQL +'	, P.PartsNameJp' + @CRLF
	SET @SQL = @SQL +'	, P.MakerCode' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.Parts AS P' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    P.DelFlag = ''0''' + @CRLF 

	--部品番号
	IF ((@PartsNumber IS NOT NULL) AND (@PartsNumber <> ''))
	BEGIN
		SET @SQL = @SQL +'AND P.PartsNumber LIKE ''%' + @PartsNumber + '%''' + @CRLF
	END
	--部品名
	IF ((@PartsNameJp IS NOT NULL) AND (@PartsNameJp <> ''))
	BEGIN
		SET @SQL = @SQL +'AND P.PartsNameJp LIKE ''%' + @PartsNameJp + '%''' + @CRLF
	END
	
	--メーカーコード
	IF ((@MakerCode IS NOT NULL) AND (@MakerCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND P.MakerCode LIKE ''%' + @MakerCode+ '%''' + @CRLF
	END

	EXECUTE sp_executeSQL @SQL, @PARAM, @PartsNumber, @PartsNameJp, @MakerCode

	/*-------------------------------------------*/
	/* 仕入実績のある仕入先情報					 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_PartsPurchase_PP (
		  SupplierCode nvarchar(10)
		, PartsNumber nvarchar(50)
	)

	SET @PARAM = '@SupplierCode nvarchar(10), @DepartmentCode nvarchar(3)'
	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_PartsPurchase_PP' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  PP.SupplierCode' + @CRLF
	SET @SQL = @SQL +'	, PP.PartsNumber' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.PartsPurchase AS PP' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    PP.DelFlag = ''0''' + @CRLF
	SET @SQL = @SQL +'AND PP.PurchaseStatus = ''002''' + @CRLF
	
	--部門コード
	SET @SQL = @SQL +'AND PP.DepartmentCode = @DepartmentCode' + @CRLF

	SET @SQL = @SQL + 'GROUP BY PP.SupplierCode, PP.PartsNumber' + @CRLF
	
	EXECUTE sp_executeSQL @SQL, @PARAM, @SupplierCode, @DepartmentCode
	CREATE INDEX ix_temp_PartsPurchase_PP ON #temp_PartsPurchase_PP(PartsNumber)


	/*-------------------------------------------*/
	/* 在庫数量									 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_PartsStock_PS (
		  PartsNumber nvarchar(25)
		, Quantity decimal(10,2)
	)

	SET @PARAM = ''
	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_PartsStock_PS' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  PS.PartsNumber' + @CRLF
	SET @SQL = @SQL +'	, SUM(ISNULL(PS.Quantity, 0)) - SUM(ISNULL(PS.ProvisionQuantity, 0)) AS Quantity' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.PartsStock AS PS' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    PS.DelFlag = ''0''' + @CRLF 
	--部門コード
	--SET @SQL = @SQL +'AND EXISTS(SELECT 1 FROM dbo.Location AS L WHERE L.LocationType = ''001'' AND L.DepartmentCode = @DepartmentCode AND L.LocationCode = PS.LocationCode)' + @CRLF
	SET @SQL = @SQL +'AND EXISTS(SELECT 1 FROM #temp_Location AS L WHERE L.LocationType = ''001'' AND L.LocationCode = PS.LocationCode)' + @CRLF		--Mod 2016/08/13 arc yano #3596

	
	SET @SQL = @SQL +'GROUP BY PS.PartsNumber' + @CRLF 

	EXECUTE sp_executeSQL @SQL, @PARAM
	CREATE INDEX ix_temp_PartsStock_PS ON #temp_PartsStock_PS(PartsNumber)

	/*-------------------------------------------*/
	/* 部品在庫検索								 */
	/*-------------------------------------------*/

	SET @PARAM = '@MakerName nvarchar(50), @SupplierCode nvarchar(10)'

	SET @SQL = ''
	SET @SQL = @SQL + 'SELECT'+ @CRLF
	SET @SQL = @SQL + '   M.MakerName AS MakerName'+ @CRLF
	SET @SQL = @SQL + ' , P.PartsNumber AS PartsNumber'+ @CRLF
	SET @SQL = @SQL + ' , P.PartsNameJp AS PartsNameJp'+ @CRLF
	SET @SQL = @SQL + ' , ISNULL(PS.Quantity, 0) AS Quantity'+ @CRLF
	SET @SQL = @SQL + ' , SP.SupplierName AS SupplierName'+ @CRLF

    SET @SQL = @SQL + 'FROM #temp_Parts_P AS P'+ @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchase_PP AS PP ON PP.PartsNumber = P.PartsNumber'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsStock_PS AS PS ON PS.PartsNumber = P.PartsNumber'+ @CRLF
	SET @SQL = @SQL + 'INNER JOIN #temp_Maker_M AS M ON M.MakerCode = P.MakerCode'+ @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS SP ON SP.SupplierCode = PP.SupplierCode'+ @CRLF

	SET @SQL = @SQL + 'WHERE 1 = 1'+ @CRLF		

	--仕入先コード
	IF ((@SupplierCode IS NOT NULL) AND (@SupplierCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND PP.SupplierCode = @SupplierCode' + @CRLF
	END

	SET @SQL = @SQL + 'ORDER BY P.MakerCode, P.PartsNumber'+ @CRLF


	EXECUTE sp_executeSQL @SQL, @PARAM, @MakerName, @SupplierCode


BEGIN

	BEGIN TRY
		DROP TABLE #temp_Maker_M
		DROP TABLE #temp_Parts_P
		DROP TABLE #temp_PartsPurchase_PP
		DROP TABLE #temp_PartsStock_PS
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

END




GO


