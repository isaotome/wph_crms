USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsLocationList]    Script Date: 2016/10/03 15:20:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




--2016/08/13 arc yano #3596 【大項目】部門棚統合対応 新規作成 
CREATE PROCEDURE [dbo].[GetPartsLocationList]

	@PartsNumber nvarchar(25),						--部品番号
	@DepartmentCode nvarchar(3),		            --部門コード
	@WarehouseCode nvarchar(6),			            --倉庫コード
	@LocationCode nvarchar(12),						--ロケーションコード
	@DelFlag nvarchar(2)							--削除フラグ
AS

BEGIN

	SET NOCOUNT ON

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @ROWCNT INT = 0			--行数

	/*-------------------------------------------*/
	/* 部品ロケーション取得 （PartsLocation)	 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_PartsLocation (
		 PartsNumber       nvarchar(25)			--部品番号
	   , PartsNameJp	   nvarchar(50)			--部品名
	   , WarehouseCode     nvarchar(6)			--倉庫コード
	   , WarehouseName     nvarchar(20)			--倉庫名
	   , LocationCode      nvarchar(12)			--ロケーションコード
	   , LocationName      nvarchar(50)			--ロケーションコード
	   , DelFlag           nvarchar(2)			--削除フラグ         
		)

	BEGIN
		SET @PARAM = '@PartsNumber nvarchar(25),  @WarehouseCode nvarchar(6), @LocationCode nvarchar(12), @DelFlag nvarchar(2)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsLocation' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '  PL.PartsNumber' + @CRLF
		SET @SQL = @SQL + ', P.PartsNameJp' + @CRLF
		SET @SQL = @SQL + ', PL.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ', W.WarehouseName' + @CRLF
		SET @SQL = @SQL + ', PL.LocationCode' + @CRLF
		SET @SQL = @SQL + ', L.LocationName' + @CRLF
		SET @SQL = @SQL + ', PL.DelFlag' + @CRLF
		SET @SQL = @SQL + ' FROM dbo.PartsLocation AS PL' + @CRLF
		SET @SQL = @SQL + ' LEFT OUTER JOIN dbo.Warehouse AS W ON PL.WarehouseCode = W.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ' LEFT OUTER JOIN dbo.Location AS L ON PL.LocationCode = L.LocationCode' + @CRLF
		SET @SQL = @SQL + ' LEFT OUTER JOIN dbo.Parts AS P ON PL.PartsNumber = P.PartsNumber' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + ' 1 = 1 '+ @CRLF
		
		IF ((@PartsNumber IS NOT NULL) AND (@PartsNumber <>''))													--部品番号による絞込
		BEGIN
			SET @SQL = @SQL + 'AND PL.PartsNumber = @PartsNumber'+ @CRLF
		END

		IF ((@WarehouseCode IS NOT NULL) AND (@WarehouseCode <>''))												--倉庫コードによる絞込
		BEGIN
			SET @SQL = @SQL + 'AND PL.WarehouseCode = @WarehouseCode'+ @CRLF
		END

		IF ((@LocationCode IS NOT NULL) AND (@LocationCode <>''))												--ロケーションコードによる絞込
		BEGIN
			SET @SQL = @SQL + 'AND PL.LocationCode = @LocationCode'+ @CRLF
		END
		
		IF ((@DelFlag IS NOT NULL) AND (@DelFlag <>''))															--削除フラグによる絞込
		BEGIN
			SET @SQL = @SQL + 'AND PL.DelFlag = @DelFlag'+ @CRLF
		END
		
		EXECUTE sp_executeSQL @SQL, @PARAM, @PartsNumber, @WarehouseCode, @LocationCode, @DelFlag
		CREATE INDEX ix_temp_PartsLocation ON #temp_PartsLocation(PartsNumber, WarehouseCode, LocationCode)
	END


	/*-------------------------------------------*/
	/* 部門・倉庫組合せマスタ取得				 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_DepartmentWarehouse (
	     DepartmentCode nvarchar(3)			--部門コード
	   , DepartmentName nvarchar(20)		--部門名
	   , WarehouseCode nvarchar(6)			--倉庫コード
		)

	BEGIN
		SET @PARAM = '@DepartmentCode nvarchar(3), @WarehouseCode nvarchar(6)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_DepartmentWarehouse' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '	DW.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName' + @CRLF
		SET @SQL = @SQL + ',DW.WarehouseCode' + @CRLF
		SET @SQL = @SQL + 'FROM dbo.DepartmentWarehouse AS DW' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON DW.DepartmentCode = D.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '		DW.DelFlag = ''0''' + @CRLF
		
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))				--部門コードによる絞込
		BEGIN
			SET @SQL = @SQL + 'AND DW.DepartmentCode = @DepartmentCode'+ @CRLF
		END
		IF ((@WarehouseCode IS NOT NULL) AND (@WarehouseCode <>''))					--倉庫コードによる絞込
		BEGIN
			SET @SQL = @SQL + 'AND DW.WarehouseCode = @WarehouseCode'+ @CRLF
		END

		EXECUTE sp_executeSQL @SQL, @PARAM, @DepartmentCode, @WarehouseCode 
		CREATE INDEX ix_temp_DepartmentWarehouse ON #temp_DepartmentWarehouse(WarehouseCode)
	END


	/*-------------------------------------------*/
	/* 部品ロケーション情報の取得					 */
	/*-------------------------------------------*/
	BEGIN
		SET @PARAM = '@DepartmentCode nvarchar(3)'
		SET @SQL = ''
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '	PL.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PL.PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',DW.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',DW.DepartmentName' + @CRLF
		SET @SQL = @SQL + ',PL.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ',PL.WarehouseName' + @CRLF
		SET @SQL = @SQL + ',PL.LocationCode' + @CRLF
		SET @SQL = @SQL + ',PL.LocationName' + @CRLF
		SET @SQL = @SQL + ',PL.DelFlag' + @CRLF
		SET @SQL = @SQL + 'FROM #temp_PartsLocation AS PL ' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_DepartmentWarehouse AS DW ON PL.WarehouseCode = DW.WarehouseCode ' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '		1 = 1' + @CRLF
		
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))				--部門コードによる絞込
		BEGIN
			SET @SQL = @SQL + 'AND DW.DepartmentCode IS NOT NULL AND DW.DepartmentCode = @DepartmentCode'+ @CRLF
		END

		SET @SQL = @SQL + 'ORDER BY'+ @CRLF
		SET @SQL = @SQL + 'PL.PartsNumber, DW.DepartmentCode'+ @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM, @DepartmentCode
	END


	/*
	SELECT
		 PL.PartsNumber					--部品
		,PL.PartsNameJp					--部品名
		,DW.DepartmentCode				--部門コード
		,DW.DepartmentName				--部門名
		,PL.WarehouseCode				--倉庫コード
		,PL.WarehouseName				--倉庫名
		,PL.LocationCode				--ロケーションコード
		,PL.LocationName				--ロケーション名
		,PL.DelFlag						--削除フラグ
	FROM
		#temp_PartsLocation PL
	LEFT OUTER JOIN
		#temp_DepartmentWarehouse DW ON PL.WarehouseCode = DW.WarehouseCode 
	ORDER BY
		PL.PartsNumber,
		DW.DepartmentCode
	*/

	BEGIN TRY
		--tempテーブル削除
		DROP TABLE #temp_DepartmentWarehouse
		DROP TABLE #temp_PartsLocation
	END TRY
	BEGIN CATCH
		--無視
	END CATCH
	
END




GO


