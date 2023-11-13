USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetStockCarList]    Script Date: 2020/01/30 9:41:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
-- Update date: <Update Date,,>
-- 2017/12/15 arc yano #3839 車両在庫棚卸　デモカーの場合の新中区分の表示変更
-- Description:	<Description,,>
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetStockCarList] 
	   @InventoryMonth nvarchar(10) = ''			--棚卸月
	  ,@DepartmentCode nvarchar(6) = ''				--部門コード
	  ,@LocationCode nvarchar(12) = ''				--ロケーションコード
	  ,@SalesCarNumber nvarchar(50) = ''			--管理番号
	  ,@Vin nvarchar(20)  = ''						--車台番号
	  ,@NewUsedType nvarchar(3)	  = ''				--新中区分
	  ,@CarStatus nvarchar(3) = ''					--在庫区分
	  ,@CarBrandName nvarchar(50) = ''				--ブランド名
	  ,@CarName nvarchar(20) = ''					--車種名
	  ,@GradeName nvarchar(50) = ''					--グレード名
	  ,@RegistrationNumber nvarchar(20) = ''		--車両登録番号
	  ,@SockFlag nvarchar(1) = '0'					--在庫有無(0:全て 1:有り 2:無し)
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

	--棚卸ステ－タス
	DECLARE @InventoryStatus NVARCHAR(3) = ''

	--対象年月From
	DECLARE @TargetDateFrom datetime = @InventoryMonth

	--備考欄
	DECLARE @ColumnName nvarchar(255)
	

	--■一時表の削除
	/*************************************************************************/

	IF OBJECT_ID(N'tempdb..#Temp_DepartmentWarehouse', N'U') IS NOT NULL
	DROP TABLE #Temp_DepartmentWarehouse;
	IF OBJECT_ID(N'tempdb..#Temp_CarInfoList', N'U') IS NOT NULL
	DROP TABLE #Temp_CarInfoList;
	IF OBJECT_ID(N'tempdb..#Temp_CarMaster', N'U') IS NOT NULL
	DROP TABLE #Temp_CarMaster;

	/*************************************************************************/
	
	/*-------------------------------------------*/
	/* ■■一時表の宣言							 */
	/*-------------------------------------------*/
	--仕入リスト
	CREATE TABLE #Temp_DepartmentWarehouse (
		 [DepartmentCode] NVARCHAR(3) NOT NULL		--部門コード
		,[WarehouseCode] NVARCHAR(6) NOT NULL		--倉庫コード
	)

	--車両情報リスト
	CREATE TABLE #Temp_CarInfoList (
		 [LocationCode] NVARCHAR(12) 					--ロケーションコード
		,[SalesCarNumber] NVARCHAR(50) NOT NULL			--管理番号
		,[Vin] NVARCHAR(50)								--車台番号
		,[NewUsedType] NVARCHAR(3) NOT NULL				--新中区分
		,[CarStatus] NVARCHAR(3) NOT NULL				--在庫区分
		,[CarGradeCode] nvarchar(30) NULL				--グレードコード
		,[SalesPrice] decimal(10, 0) NULL				--販売価格
		,[ColorType] NVARCHAR(3) NULL					--系統色
		,[ExteriorColorCode] NVARCHAR(8) NULL			--外装色コード
		,[ExteriorColorName] NVARCHAR(50) NULL			--外装色名
		,[MorterViecleOfficialCode] NVARCHAR(5) NULL	--陸運局コード
		,[RegistrationNumberType] NVARCHAR(3) NULL		--登録番号(種別)
		,[RegistrationNumberKana] NVARCHAR(1) NULL		--登録番号(かな)
		,[RegistrationNumberPlate] NVARCHAR(4) NULL		--登録番号(プレート)
		,[RegistrationNumber] NVARCHAR(20) NULL			--車両登録番号
		,[Quantity] DECIMAL(10, 3) NULL					--数量
		,[PhysicalQuantity] DECIMAL(10, 3) NULL			--実棚
		,[Summary] NVARCHAR(255) NULL					--備考
	)

	--車両情報リスト２
	CREATE TABLE #Temp_CarMaster (
		 [CarGradeCode] NVARCHAR(30) NOT NULL		--グレードコード
		,[CarGradeName] NVARCHAR(50) NOT NULL		--グレード名
		,[CarBrandCode] NVARCHAR(30) NOT NULL		--ブランドコード
		,[CarBrandName] NVARCHAR(50) NOT NULL		--ブランド名
		,[CarCode] NVARCHAR(30) NOT NULL			--車種コード
		,[CarName] NVARCHAR(20) NOT NULL			--車種名
	)

	/*-------------------------------------------*/
	/* データ取得								 */
	/*-------------------------------------------*/

	--トランザクション開始 
	BEGIN TRANSACTION
	BEGIN TRY

		--棚卸ステータス取得

		IF (@InventoryMonth is not null AND @InventoryMonth <> '')
		BEGIN
			SELECT 
				@InventoryStatus = InventoryStatus
			FROM
				dbo.InventoryMonthControlCar
			WHERE
				InventoryMonth = REPLACE(@InventoryMonth, '/', '')
		END

		--PRINT '@InventoryStatus=' + @InventoryStatus

		-- ----------------------
		-- 部門リストを出力
		-- ----------------------
		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #Temp_DepartmentWarehouse' + @CRLF
		SET @SQL = @SQL +'SELECT' + @CRLF
		SET @SQL = @SQL +'	  dw.DepartmentCode' + @CRLF
		SET @SQL = @SQL +'	, dw.WarehouseCode' + @CRLF
		SET @SQL = @SQL +'FROM' + @CRLF
		SET @SQL = @SQL +'	dbo.DepartmentWarehouse dw' + @CRLF
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    DelFlag = ''0''' + @CRLF 
		IF ((@DepartmentCode is not null) AND (@DepartmentCode <> ''))
		BEGIN
			SET @SQL = @SQL +'AND DepartmentCode = ''' + @DepartmentCode + '''' + @CRLF
		END

		--PRINT 'SQL = ' +  @SQL

		EXECUTE sp_executeSQL @SQL
		CREATE INDEX IX_Temp_DepartmentWarehouse ON #Temp_DepartmentWarehouse(DepartmentCode, WarehouseCode)

		-- ----------------------
		-- 車両棚卸データ
		-- ----------------------
		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #Temp_CarInfoList' + @CRLF
		SET @SQL = @SQL +' SELECT' + @CRLF
		
		-- 対象年月が存在する場合は、車両棚卸テーブルを参照する
		IF (@InventoryStatus is null OR @InventoryMonth = '' OR @InventoryStatus <> '003')
		BEGIN
			SET @SQL = @SQL +'	  sc.LocationCode' + @CRLF
			SET @SQL = @SQL +'	, sc.SalesCarNumber' + @CRLF
			SET @SQL = @SQL +'	, sc.Vin' + @CRLF
			SET @SQL = @SQL +'	, sc.NewUsedType' + @CRLF
			SET @SQL = @SQL +'	, ''999'' AS CarStatus' + @CRLF
			SET @SQL = @SQL +'	, sc.CarGradeCode' + @CRLF
			SET @SQL = @SQL +'	, sc.SalesPrice' + @CRLF
			SET @SQL = @SQL +'	, sc.ColorType' + @CRLF
			SET @SQL = @SQL +'	, sc.ExteriorColorCode' + @CRLF
			SET @SQL = @SQL +'	, sc.ExteriorColorName' + @CRLF
			SET @SQL = @SQL +'	, sc.MorterViecleOfficialCode' + @CRLF
			SET @SQL = @SQL +'	, sc.RegistrationNumberType' + @CRLF
			SET @SQL = @SQL +'	, sc.RegistrationNumberKana' + @CRLF
			SET @SQL = @SQL +'	, sc.RegistrationNumberPlate' + @CRLF
			SET @SQL = @SQL +'	, sc.MorterViecleOfficialCode + '' '' + sc.RegistrationNumberType + '' '' + sc.RegistrationNumberKana + '' '' + RegistrationNumberPlate AS RegistrationNumber' + @CRLF
			SET @SQL = @SQL +'	, 1' + @CRLF
			SET @SQL = @SQL +'	, 1' + @CRLF
			SET @SQL = @SQL +'	, sc.Memo' + @CRLF
			SET @SQL = @SQL +'FROM' + @CRLF
			SET @SQL = @SQL +'	dbo.SalesCar sc' + @CRLF
			SET @SQL = @SQL +'WHERE' + @CRLF
			SET @SQL = @SQL +'    sc.DelFlag = ''0''' + @CRLF
			SET @SQL = @SQL +'    AND (sc.CarStatus is not null AND sc.CarStatus <> '''' AND sc.CarStatus <> ''006'')' + @CRLF							--在庫ステータス≠「納車済」
			SET @SQL = @SQL +'    AND (sc.CarUsage is null OR sc.CarUsage = '''')' + @CRLF		--利用用途
			SET @SQL = @SQL +'    AND NOT EXISTS' + @CRLF
			SET @SQL = @SQL +'    (' + @CRLF
			SET @SQL = @SQL +'			select ''x'' from dbo.BackGroundDemoCar bg where bg.DelFlag = ''0'' and bg.ProcType = ''006'' and sc.SalesCarNumber = bg.NewSalesCarNumber'  + @CRLF		--自社登録は除く
			SET @SQL = @SQL +'    )' + @CRLF
			SET @SQL = @SQL +' UNION' + @CRLF
			SET @SQL = @SQL +' SELECT' + @CRLF
			SET @SQL = @SQL +'	  sc.LocationCode' + @CRLF
			SET @SQL = @SQL +'	, sc.SalesCarNumber' + @CRLF
			SET @SQL = @SQL +'	, sc.Vin' + @CRLF
			SET @SQL = @SQL +'	, sc.NewUsedType' + @CRLF
			SET @SQL = @SQL +'	, ''006'' AS CarStatus' + @CRLF
			SET @SQL = @SQL +'	, sc.CarGradeCode' + @CRLF
			SET @SQL = @SQL +'	, sc.SalesPrice' + @CRLF
			SET @SQL = @SQL +'	, sc.ColorType' + @CRLF
			SET @SQL = @SQL +'	, sc.ExteriorColorCode' + @CRLF
			SET @SQL = @SQL +'	, sc.ExteriorColorName' + @CRLF
			SET @SQL = @SQL +'	, sc.MorterViecleOfficialCode' + @CRLF
			SET @SQL = @SQL +'	, sc.RegistrationNumberType' + @CRLF
			SET @SQL = @SQL +'	, sc.RegistrationNumberKana' + @CRLF
			SET @SQL = @SQL +'	, sc.RegistrationNumberPlate' + @CRLF
			SET @SQL = @SQL +'	, sc.MorterViecleOfficialCode + '' '' + sc.RegistrationNumberType + '' '' + sc.RegistrationNumberKana + '' '' + RegistrationNumberPlate AS RegistrationNumber' + @CRLF
			SET @SQL = @SQL +'	, 1' + @CRLF
			SET @SQL = @SQL +'	, 1' + @CRLF
			SET @SQL = @SQL +'	, sc.Memo' + @CRLF
			SET @SQL = @SQL +'FROM' + @CRLF
			SET @SQL = @SQL +'	dbo.SalesCar sc' + @CRLF
			SET @SQL = @SQL +'WHERE' + @CRLF
			SET @SQL = @SQL +'    sc.DelFlag = ''0''' + @CRLF
			SET @SQL = @SQL +'    AND (sc.CarStatus is not null AND sc.CarStatus <> '''' AND sc.CarStatus <> ''006'')' + @CRLF							--在庫ステータス≠「納車済」
			SET @SQL = @SQL +'    AND (sc.CarUsage is null OR sc.CarUsage = '''')' + @CRLF		--利用用途
			SET @SQL = @SQL +'    AND EXISTS' + @CRLF
			SET @SQL = @SQL +'    (' + @CRLF
			SET @SQL = @SQL +'			select ''x'' from dbo.BackGroundDemoCar bg where bg.DelFlag = ''0'' and bg.ProcType = ''006'' and sc.SalesCarNumber = bg.NewSalesCarNumber' + @CRLF		--自社登録
			SET @SQL = @SQL +'    )' + @CRLF
			SET @SQL = @SQL +' UNION' + @CRLF
			SET @SQL = @SQL +' SELECT' + @CRLF
			SET @SQL = @SQL +'	  sc.LocationCode' + @CRLF
			SET @SQL = @SQL +'	, sc.SalesCarNumber' + @CRLF
			SET @SQL = @SQL +'	, sc.Vin' + @CRLF
			SET @SQL = @SQL +'	, sc.NewUsedType' + @CRLF
			SET @SQL = @SQL +'	, sc.CarUsage' + @CRLF
			SET @SQL = @SQL +'	, sc.CarGradeCode' + @CRLF
			SET @SQL = @SQL +'	, sc.SalesPrice' + @CRLF
			SET @SQL = @SQL +'	, sc.ColorType' + @CRLF
			SET @SQL = @SQL +'	, sc.ExteriorColorCode' + @CRLF
			SET @SQL = @SQL +'	, sc.ExteriorColorName' + @CRLF
			SET @SQL = @SQL +'	, sc.MorterViecleOfficialCode' + @CRLF
			SET @SQL = @SQL +'	, sc.RegistrationNumberType' + @CRLF
			SET @SQL = @SQL +'	, sc.RegistrationNumberKana' + @CRLF
			SET @SQL = @SQL +'	, sc.RegistrationNumberPlate' + @CRLF
			SET @SQL = @SQL +'	, sc.MorterViecleOfficialCode + '' '' + sc.RegistrationNumberType + '' '' + sc.RegistrationNumberKana + '' '' + sc.RegistrationNumberPlate AS RegistrationNumber' + @CRLF
			SET @SQL = @SQL +'	, 1' + @CRLF
			SET @SQL = @SQL +'	, 1' + @CRLF
			SET @SQL = @SQL +'	, sc.Memo' + @CRLF
			SET @SQL = @SQL +'FROM' + @CRLF
			SET @SQL = @SQL +'	dbo.SalesCar sc' + @CRLF
			SET @SQL = @SQL +'WHERE' + @CRLF
			SET @SQL = @SQL +'    sc.DelFlag = ''0''' + @CRLF
			SET @SQL = @SQL +'    AND sc.CarUsage is not null' + @CRLF
			SET @SQL = @SQL +'    AND sc.CarUsage <> ''''' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'	  sc.LocationCode' + @CRLF
			SET @SQL = @SQL +'	, sc.SalesCarNumber' + @CRLF
			SET @SQL = @SQL +'	, sc.Vin' + @CRLF
			SET @SQL = @SQL +'	, sc.NewUsedType' + @CRLF
			SET @SQL = @SQL +'	, sc.CarUsage' + @CRLF
			SET @SQL = @SQL +'	, s.CarGradeCode' + @CRLF
			SET @SQL = @SQL +'	, NULL' + @CRLF
			SET @SQL = @SQL +'	, s.ColorType' + @CRLF
			SET @SQL = @SQL +'	, s.ExteriorColorCode' + @CRLF
			SET @SQL = @SQL +'	, s.ExteriorColorName' + @CRLF
			SET @SQL = @SQL +'	, s.MorterViecleOfficialCode' + @CRLF
			SET @SQL = @SQL +'	, s.RegistrationNumberType' + @CRLF
			SET @SQL = @SQL +'	, s.RegistrationNumberKana' + @CRLF
			SET @SQL = @SQL +'	, s.RegistrationNumberPlate' + @CRLF
			SET @SQL = @SQL +'	, s.MorterViecleOfficialCode + '' '' + s.RegistrationNumberType + '' '' + s.RegistrationNumberKana + '' '' + s.RegistrationNumberPlate AS RegistrationNumber' + @CRLF
			SET @SQL = @SQL +'	, sc.Quantity' + @CRLF
			SET @SQL = @SQL +'	, sc.PhysicalQuantity' + @CRLF
			SET @SQL = @SQL +'	, sc.Summary' + @CRLF
			SET @SQL = @SQL +'FROM' + @CRLF
			--Mod 2020/1/20 yano #4033
			SET @SQL = @SQL +'	dbo.InventoryStockCar sc LEFT JOIN' + @CRLF 
			--SET @SQL = @SQL +'	dbo.InventoryStockCar sc INNER JOIN' + @CRLF 
			SET @SQL = @SQL +'	dbo.SalesCar s ON sc.SalesCarNumber = s.SalesCarNumber' + @CRLF
			SET @SQL = @SQL +'WHERE' + @CRLF
			--Mod 2020/1/20 yano #4033
			SET @SQL = @SQL +'	  InventoryMonth = CONVERT(datetime, ''' + @InventoryMonth + ''')' + @CRLF
			--SET @SQL = @SQL +'    sc.DelFlag = ''0''' + @CRLF
			--SET @SQL = @SQL +'	  AND InventoryMonth = CONVERT(datetime, ''' + @InventoryMonth + ''')' + @CRLF
		END
		
		--DEBUG
		--PRINT 'SQL = ' +  @SQL

		EXECUTE sp_executeSQL @SQL
		CREATE INDEX IX_Temp_CarInfoList ON #Temp_CarInfoList(SalesCarNumber)

		-- ----------------------
		-- 車種マスタ
		-- ----------------------
		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #Temp_CarMaster' + @CRLF
		SET @SQL = @SQL +'SELECT' + @CRLF
		SET @SQL = @SQL +'	  cm.CarGradeCode' + @CRLF
		SET @SQL = @SQL +'	, cm.CarGradeName' + @CRLF
		SET @SQL = @SQL +'	, cm.CarBrandCode' + @CRLF
		SET @SQL = @SQL +'	, cm.CarBrandName' + @CRLF
		SET @SQL = @SQL +'	, cm.CarCode' + @CRLF
		SET @SQL = @SQL +'	, cm.CarName' + @CRLF
		SET @SQL = @SQL +'FROM' + @CRLF
		SET @SQL = @SQL +'	dbo.V_CarMaster cm' + @CRLF
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    1 = 1' + @CRLF

		IF ((@CarBrandName is not null) AND (@CarBrandName <> ''))
		BEGIN
			SET @SQL = @SQL +'AND cm.CarBrandName like ''%' + @CarBrandName + '%''' + @CRLF
		END
		IF ((@CarName is not null) AND (@CarName <> ''))
		BEGIN
			SET @SQL = @SQL +'AND cm.CarName like ''%' + @CarName + '%''' + @CRLF
		END
		IF ((@GradeName is not null) AND (@GradeName <> ''))
		BEGIN
			SET @SQL = @SQL +'AND cm.CarGradeName like ''%' + @GradeName + '%''' + @CRLF
		END

		--DEBUG
		--PRINT @SQL

		EXECUTE sp_executeSQL @SQL
		CREATE INDEX IDX_Temp_CarMaster ON #Temp_CarMaster(CarGradeCode)


		-- ----------------------
		-- 車両在庫データの取得
		-- ----------------------
		SET @SQL = ''
		SET @SQL = @SQL +' SELECT' + @CRLF
		SET @SQL = @SQL +'	  il.LocationCode AS LocationCode' + @CRLF
		SET @SQL = @SQL +'	, l.LocationName AS LocationName' + @CRLF
		SET @SQL = @SQL +'	, il.SalesCarNumber AS SalesCarNumber' + @CRLF
		SET @SQL = @SQL +'	, il.Vin AS Vin' + @CRLF
		SET @SQL = @SQL +'	, CASE WHEN (il.CarStatus = ''999'' OR il.CarStatus = ''006'') THEN il.NewUsedType ELSE NULL END AS NewUsedType' + @CRLF		--Mod 2017/12/15 arc yano #3839
		SET @SQL = @SQL +'	, CASE WHEN (il.CarStatus = ''999'' OR il.CarStatus = ''006'') THEN nu.Name ELSE ''--''  END AS NewUsedTypeName' + @CRLF			--Mod 2017/12/15 arc yano #3839
		SET @SQL = @SQL +'	, il.CarStatus AS CarStatus' + @CRLF
		SET @SQL = @SQL +'	, cn.Name AS CarStatusName' + @CRLF
		SET @SQL = @SQL +'	, cm.CarBrandName AS CarBrandName' + @CRLF
		SET @SQL = @SQL +'	, cm.CarName AS CarName' + @CRLF
		SET @SQL = @SQL +'	, cm.CarGradeName AS CarGradeName' + @CRLF
		SET @SQL = @SQL +'	, il.SalesPrice AS SalesPrice' + @CRLF
		SET @SQL = @SQL +'	, cc.Name AS ColorType' + @CRLF
		SET @SQL = @SQL +'	, il.ExteriorColorCode AS ExteriorColorCode' + @CRLF
		SET @SQL = @SQL +'	, il.ExteriorColorName AS ExteriorColorName' + @CRLF
		SET @SQL = @SQL +'	, il.MorterViecleOfficialCode AS MorterViecleOfficialCode' + @CRLF
		SET @SQL = @SQL +'	, il.RegistrationNumberType AS RegistrationNumberType' + @CRLF
		SET @SQL = @SQL +'	, il.RegistrationNumberKana AS RegistrationNumberKana' + @CRLF
		SET @SQL = @SQL +'	, il.RegistrationNumberPlate AS RegistrationNumberPlate' + @CRLF
		SET @SQL = @SQL +'	, il.RegistrationNumber AS RegistrationNumber' + @CRLF
		SET @SQL = @SQL +'	, il.Quantity AS Quantity' + @CRLF
		SET @SQL = @SQL +'	, il.PhysicalQuantity AS PhysicalQuantity' + @CRLF
		SET @SQL = @SQL +'	, il.Summary AS Summary' + @CRLF
		SET @SQL = @SQL +'FROM' + @CRLF
		SET @SQL = @SQL +'	#Temp_CarInfoList il INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	#Temp_CarMaster cm ON il.CarGradeCode = cm.CarGradeCode INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.Location l ON il.LocationCode = l.LocationCode INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	c_NewUsedType nu ON il.NewUsedType = nu.Code INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	c_CodeName cn ON il.CarStatus = cn.Code AND cn.CategoryCode = ''020'' LEFT OUTER JOIN' + @CRLF
		SET @SQL = @SQL +'	c_ColorCategory cc ON il.ColorType = cc.Code' + @CRLF
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +' 1 = 1' + @CRLF
		IF ((@RegistrationNumber is not null) AND (@RegistrationNumber <> ''))
		BEGIN
			
			SET @SQL = @SQL +' AND il.RegistrationNumber LIKE ''%' + @RegistrationNumber + '%''' + @CRLF
		END

		IF ((@LocationCode is not null) AND (@LocationCode <> ''))
			BEGIN
				SET @SQL = @SQL +'AND il.LocationCode = ''' + @LocationCode + '''' + @CRLF 
			END
		IF ((@SalesCarNumber is not null) AND (@SalesCarNumber <> ''))
			BEGIN
				SET @SQL = @SQL +'AND il.SalesCarNumber = ''' + @SalesCarNumber + '''' + @CRLF 
			END
		IF ((@Vin is not null) AND (@Vin <> ''))
			BEGIN
				SET @SQL = @SQL +'AND il.Vin like ''%' + @Vin + '%''' + @CRLF 
			END
		IF ((@NewUsedType is not null) AND (@NewUsedType <> ''))
			BEGIN
				SET @SQL = @SQL +'AND il.NewUsedType = ''' + @NewUsedType + '''' + @CRLF 
			END
		IF ((@CarStatus is not null) AND (@CarStatus <> ''))
			BEGIN
				SET @SQL = @SQL +'AND il.CarStatus = ''' + @CarStatus + '''' + @CRLF 
			END
		IF ((@DepartmentCode is not null) AND (@DepartmentCode <> ''))
			BEGIN
				SET @SQL = @SQL +'AND EXISTS' + @CRLF
				SET @SQL = @SQL +'	( select ''x'' from dbo.Location l where l.DelFlag = ''0'' and exists ( select ''y'' from #Temp_DepartmentWarehouse dw  where l.warehouseCode = dw.WarehouseCode ) and il.LocationCode = l.LocationCode )' + @CRLF
			END
		
		IF ((@SockFlag is not null) AND (@SockFlag <> '') AND @SockFlag = '1')
			BEGIN
				SET @SQL = @SQL +'AND il.PhysicalQuantity > 0' + @CRLF
			END
		ELSE IF ((@SockFlag is not null) AND (@SockFlag <> '') AND @SockFlag = '2')
			BEGIN
				SET @SQL = @SQL +'AND il.PhysicalQuantity = 0' + @CRLF
			END

		SET @SQL = @SQL +'ORDER BY' + @CRLF
		SET @SQL = @SQL +' il.LocationCode ,cn.DisplayOrder, il.SalesCarNumber' + @CRLF
		

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
			 convert(nvarchar(8), '') as LocationCode
			,convert(nvarchar(50), '') as LocationName
			,convert(nvarchar(50), '') as SalesCarNumber
			,convert(nvarchar(20), '') as Vin
			,convert(nvarchar(3), '') as NewUsedType
			,convert(nvarchar(50), '') as NewUsedTypeName
			,convert(nvarchar(3), '') as CarStatus
			,convert(nvarchar(50), '') as CarStatusName
			,convert(nvarchar(50), '') as CarBrandName
			,convert(nvarchar(20), '') as CarName
			,convert(nvarchar(50), '') as CarGradeName
			,convert(decimal(10), null) as SalesPrice
			,convert(nvarchar(50), '') as ColorType
			,convert(nvarchar(8), '') as ExteriorColorCode
			,convert(nvarchar(50), '') as ExteriorColorName
			,convert(nvarchar(5), '') as MorterViecleOfficialCode
			,convert(nvarchar(3), '') as RegistrationNumberType
			,convert(nvarchar(1), '') as RegistrationNumberKana
			,convert(nvarchar(4), '') as RegistrationNumberPlate
			,convert(nvarchar(20), '') as RegistrationNumber
			,convert(decimal(10, 3), '') as Quantity
			,convert(decimal(10, 3), '') as PhysicalQuantity
			,convert(nvarchar(200), '') as Summary
*/
END


GO


