USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCarStatusTransition]    Script Date: 2018/05/17 16:09:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/19 arc yano #3721 サブシステム移行(車両追跡) 新規作成
-- Update date: <Update Date,,>
-- 2018/05/17 arc yano #3885 車両追跡　実棚「0」でも追跡画面に表示される
-- 2018/02/21 arc yano #3856 車両追跡　車両棚卸結果の履歴が表示されない
-- Description:	<Description,,>
-- 車両ステータス遷移情報の取得
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetCarStatusTransition] 
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
	SET @SQL = @SQL +'	  ''【仕】'' AS SlipTypeName' + @CRLF																				--区分
	SET @SQL = @SQL +'	, P.CarPurchaseType AS SlipTypeCode' + @CRLF																		--仕入種別
	SET @SQL = @SQL +'	, CASE WHEN P.CarPurchaseType IS NULL THEN ''仕入'' ELSE C2.Name END AS SlipType' + @CRLF							--伝票タイプ
	SET @SQL = @SQL +'	, P.PurchaseDate AS SlipDate' + @CRLF																				--仕入日
    SET @SQL = @SQL +'	, P.PurchaseLocationCode AS LocationCode' + @CRLF																	--入庫ロケーション
    SET @SQL = @SQL +'	, L.LocationName AS LocationName' + @CRLF																			--入庫ロケーション名
    SET @SQL = @SQL +'	, P.SupplierCode AS CustomerCode' + @CRLF																			--仕入先コード
	SET @SQL = @SQL +'	, S.SupplierName AS CustomerName' + @CRLF																			--仕入先名
	SET @SQL = @SQL +'	, P.PurchaseStatus AS SlipStatusCode' + @CRLF																		--仕入ステータスコード
	SET @SQL = @SQL +'	, C.Name AS SlipStatus' + @CRLF																						--仕入ステータス名
	SET @SQL = @SQL +'	, P.EmployeeCode AS EmployeeCode' + @CRLF																			--仕入担当者コード
	SET @SQL = @SQL +'	, E.EmployeeName AS EmployeeName' + @CRLF																			--仕入担当者名
	SET @SQL = @SQL +'	, P.SalesCarNumber AS SalesCarNumber' + @CRLF																		--車両管理番号
	SET @SQL = @SQL +'	, '''' AS SlipNumber' + @CRLF																						--伝票番号
	SET @SQL = @SQL +'	, P.VehiclePrice AS VehiclePrice' + @CRLF																			--車両本体価格
	SET @SQL = @SQL +'	, P.Amount AS Amount' + @CRLF																						--仕入金額
	SET @SQL = @SQL +'	, 1 AS SlipTypeOrder' + @CRLF																						--
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	CarPurchase P LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Location L ON P.PurchaseLocationCode=L.LocationCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Employee E ON P.EmployeeCode=E.EmployeeCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Supplier S ON P.SupplierCode=S.SupplierCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_PurchaseStatus C ON P.PurchaseStatus=C.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_CarPurchaseType C2 on P.CarPurchaseType = C2.Code' + @CRLF
	

	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	P.DelFlag=''0''' + @CRLF
	SET @SQL = @SQL +'	AND P.SalesCarNumber is not NULL' + @CRLF
	SET @SQL = @SQL +'	AND P.SalesCarNumber <> ''''' + @CRLF

	--検索条件の車台番号が入力されていた場合
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	EXISTS' + @CRLF
		SET @SQL = @SQL +'	(' + @CRLF
		SET @SQL = @SQL +'		SELECT ''x'' FROM SalesCar S WHERE S.Vin =''' + @Vin + '''' + 'AND P.SalesCarNumber = S.SalesCarNumber' + @CRLF
		SET @SQL = @SQL +'	)' + @CRLF
	END
	ELSE IF(@SalesCarNumber is not NULL AND @SalesCarNumber <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	P.SalesCarNumber = '''  + @SalesCarNumber + '''' + @CRLF
	END

	--結合
	SET @SQL = @SQL +'UNION ALL' + @CRLF

	--販売情報
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  ''【売】'' AS SlipTypeName' + @CRLF																				--区分
	SET @SQL = @SQL +'	, H.SalesType AS SlipTypeCode' + @CRLF																				--販売区分
	SET @SQL = @SQL +'	, C2.Name AS SlipType' + @CRLF																						--伝票区分
	SET @SQL = @SQL +'	, H.SalesDate AS SlipDate' + @CRLF																					--納車日
    SET @SQL = @SQL +'	, H.DepartmentCode AS LocationCode' + @CRLF																			--部門コード
    SET @SQL = @SQL +'	, D.DepartmentName AS LocationName' + @CRLF																			--部門名
    SET @SQL = @SQL +'	, H.CustomerCode AS CustomerCode' + @CRLF																			--顧客コード
	SET @SQL = @SQL +'	, C.CustomerName AS CustomerName' + @CRLF																			--顧客名
	SET @SQL = @SQL +'	, H.SalesOrderStatus AS SlipStatusCode' + @CRLF																		--伝票ステータス
	SET @SQL = @SQL +'	, C1.Name AS SlipStatus' + @CRLF																					--伝票ステータス名
	SET @SQL = @SQL +'	, H.EmployeeCode AS EmployeeCode' + @CRLF																			--担当者コード
	SET @SQL = @SQL +'	, E.employeename AS EmployeeName' + @CRLF																			--当者名
	SET @SQL = @SQL +'	, H.SalesCarNumber AS SalesCarNumber' + @CRLF																		--車両管理番号
	SET @SQL = @SQL +'	, H.SlipNumber AS SlipNumber' + @CRLF																				--伝票番号
	SET @SQL = @SQL +'	, H.SalesPrice AS VehiclePrice' + @CRLF																				--車両本体価格
	SET @SQL = @SQL +'	, (H.GrandTotalAmount - H.Totaltaxamount) AS Amount' + @CRLF														--金額
	SET @SQL = @SQL +'	, 0 AS SlipTypeOrder' + @CRLF																						--
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	CarSalesHeader H LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Department D on H.DepartmentCode=D.DepartmentCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Employee E on H.EmployeeCode=E.EmployeeCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Customer C on H.CustomerCode=C.CustomerCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_SalesOrderStatus C1 on H.SalesOrderStatus=C1.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_SalesType C2 on H.SalesType=C2.Code' + @CRLF

	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	H.DelFlag=''0''' + @CRLF
	SET @SQL = @SQL +'	AND H.SalesOrderStatus in (''005'') ' + @CRLF
	SET @SQL = @SQL +'	AND H.SalesCarNumber is not NULL' + @CRLF
	SET @SQL = @SQL +'	AND H.SalesCarNumber <> ''''' + @CRLF

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

	--結合
	SET @SQL = @SQL +'UNION ALL' + @CRLF

	--移動情報
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  ''【移】'' AS SlipTypeName' + @CRLF																				--区分
	SET @SQL = @SQL +'	, '''' AS SlipTypeCode' + @CRLF																						--販売区分
	SET @SQL = @SQL +'	, C1.Name AS SlipType' + @CRLF																						--伝票区分
	SET @SQL = @SQL +'	, T.ArrivalDate AS SlipDate' + @CRLF																				--入庫日
    SET @SQL = @SQL +'	, T.DepartureLocationCode AS LocationCode' + @CRLF																	--出発ロケーションコード
    SET @SQL = @SQL +'	, L1.LocationName AS LocationName' + @CRLF																			--出発ロケーション名
    SET @SQL = @SQL +'	, T.ArrivalLocationCode AS CustomerCode' + @CRLF																	--到着ロケーションコード
	SET @SQL = @SQL +'	, '' → '' + isnull(L2.LocationName,'''') AS CustomerName' + @CRLF													--到着ロケーション名
	SET @SQL = @SQL +'	, '''' AS SlipStatusCode' + @CRLF																					--
	SET @SQL = @SQL +'	, '''' AS SlipStatus' + @CRLF																						--
	SET @SQL = @SQL +'	, T.ArrivalEmployeeCode AS EmployeeCode' + @CRLF																	--到着担当者コード
	SET @SQL = @SQL +'	, E.employeename AS EmployeeName' + @CRLF																			--当者名
	SET @SQL = @SQL +'	, T.SalesCarNumber AS SalesCarNumber' + @CRLF																		--車両管理番号
	SET @SQL = @SQL +'	, T.TransferNumber AS SlipNumber' + @CRLF																			--移動伝票番号
	SET @SQL = @SQL +'	, NULL AS VehiclePrice' + @CRLF																						--車用本体価格
	SET @SQL = @SQL +'	, NULL AS Amount' + @CRLF																							--金額
	SET @SQL = @SQL +'	, 2 AS SlipTypeOrder' + @CRLF																						--
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	Transfer T LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Location L1 on T.DepartureLocationCode=L1.LocationCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Location L2 on T.ArrivalLocationCode=L2.LocationCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_TransferType C1 on T.TransferType=C1.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Employee E on T.ArrivalEmployeeCode=E.EmployeeCode' + @CRLF

	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	T.DelFlag=''0''' + @CRLF
	SET @SQL = @SQL +'	AND T.ArrivalDate is not NULL' + @CRLF
	SET @SQL = @SQL +'	AND T.SalesCarNumber is not NULL' + @CRLF
	SET @SQL = @SQL +'	AND T.SalesCarNumber <> ''''' + @CRLF

	--検索条件の車台番号が入力されていた場合
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	EXISTS' + @CRLF
		SET @SQL = @SQL +'	(' + @CRLF
		SET @SQL = @SQL +'		SELECT ''x'' FROM SalesCar S WHERE S.Vin =''' + @Vin + '''' + 'AND T.SalesCarNumber = S.SalesCarNumber' + @CRLF
		SET @SQL = @SQL +'	)' + @CRLF
	END
	ELSE IF(@SalesCarNumber is not NULL AND @SalesCarNumber <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	T.SalesCarNumber = '''  + @SalesCarNumber + '''' + @CRLF
	END

	--結合
	SET @SQL = @SQL +'UNION ALL' + @CRLF

	--棚卸情報
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  ''【棚】'' AS SlipTypeName' + @CRLF																				--区分
	SET @SQL = @SQL +'	, I.InventoryType AS SlipTypeCode' + @CRLF																			--棚卸区分
	SET @SQL = @SQL +'	, C1.Name AS SlipType' + @CRLF																						--棚卸区分名
	SET @SQL = @SQL +'	, DATEADD(d, -1, DATEADD(m, 1, I.InventoryMonth)) AS SlipDate' + @CRLF																				--入庫日
    SET @SQL = @SQL +'	, I.LocationCode AS LocationCode' + @CRLF																			--棚卸ロケーションコード
    SET @SQL = @SQL +'	, L.LocationName AS LocationName' + @CRLF																			--棚卸ロケーション名
    SET @SQL = @SQL +'	, '''' AS CustomerCode' + @CRLF																						--
	SET @SQL = @SQL +'	, Summary AS CustomerName' + @CRLF																					--摘要
	SET @SQL = @SQL +'	, '''' AS SlipStatusCode' + @CRLF																					--
	SET @SQL = @SQL +'	, '''' AS SlipStatus' + @CRLF																						--
	SET @SQL = @SQL +'	, '''' AS EmployeeCode' + @CRLF																						--
	SET @SQL = @SQL +'	, '''' AS EmployeeName' + @CRLF																						--
	SET @SQL = @SQL +'	, I.SalesCarNumber AS SalesCarNumber' + @CRLF																		--車両管理番号
	SET @SQL = @SQL +'	, '''' AS SlipNumber' + @CRLF																						--
	SET @SQL = @SQL +'	, NULL AS VehiclePrice' + @CRLF																						--車用本体価格
	SET @SQL = @SQL +'	, NULL AS Amount' + @CRLF																							--金額
	SET @SQL = @SQL +'	, 3 AS SlipTypeOrder' + @CRLF																						--
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	InventoryStock I INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	c_InventoryType C1 on I.InventoryType=C1.Code INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	Location L on I.LocationCode=L.LocationCode' + @CRLF

	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	I.DelFlag=''0''' + @CRLF
	SET @SQL = @SQL +'	AND I.Quantity = 1' + @CRLF
	SET @SQL = @SQL +'	AND I.InventoryType <> ''002''' + @CRLF
	SET @SQL = @SQL +'	AND I.SalesCarNumber is not NULL' + @CRLF
	SET @SQL = @SQL +'	AND I.SalesCarNumber <> ''''' + @CRLF

	--検索条件の車台番号が入力されていた場合
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	EXISTS' + @CRLF
		SET @SQL = @SQL +'	(' + @CRLF
		SET @SQL = @SQL +'		SELECT ''x'' FROM SalesCar S WHERE S.Vin =''' + @Vin + '''' + 'AND I.SalesCarNumber = S.SalesCarNumber' + @CRLF
		SET @SQL = @SQL +'	)' + @CRLF
	END
	ELSE IF(@SalesCarNumber is not NULL AND @SalesCarNumber <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	I.SalesCarNumber = '''  + @SalesCarNumber + '''' + @CRLF
	END

	--Add 2018/02/21 arc yano #3856
	--結合
	SET @SQL = @SQL +'UNION ALL' + @CRLF

	--棚卸情報
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  ''【棚】'' AS SlipTypeName' + @CRLF																				--区分
	SET @SQL = @SQL +'	, ''001'' AS SlipTypeCode' + @CRLF																					--棚卸区分
	SET @SQL = @SQL +'	, ''車両'' AS SlipType' + @CRLF																						--棚卸区分名
	SET @SQL = @SQL +'	, DATEADD(d, -1, DATEADD(m, 1, I.InventoryMonth)) AS SlipDate' + @CRLF											    --入庫日
    SET @SQL = @SQL +'	, I.LocationCode AS LocationCode' + @CRLF																			--棚卸ロケーションコード
    SET @SQL = @SQL +'	, L.LocationName AS LocationName' + @CRLF																			--棚卸ロケーション名
    SET @SQL = @SQL +'	, '''' AS CustomerCode' + @CRLF																						--
	SET @SQL = @SQL +'	, Summary AS CustomerName' + @CRLF																					--摘要
	SET @SQL = @SQL +'	, '''' AS SlipStatusCode' + @CRLF																					--
	SET @SQL = @SQL +'	, '''' AS SlipStatus' + @CRLF																						--
	SET @SQL = @SQL +'	, '''' AS EmployeeCode' + @CRLF																						--
	SET @SQL = @SQL +'	, '''' AS EmployeeName' + @CRLF																						--
	SET @SQL = @SQL +'	, I.SalesCarNumber AS SalesCarNumber' + @CRLF																		--車両管理番号
	SET @SQL = @SQL +'	, '''' AS SlipNumber' + @CRLF																						--
	SET @SQL = @SQL +'	, NULL AS VehiclePrice' + @CRLF																						--車用本体価格
	SET @SQL = @SQL +'	, NULL AS Amount' + @CRLF																							--金額
	SET @SQL = @SQL +'	, 3 AS SlipTypeOrder' + @CRLF																						--
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	InventoryStockCar I INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	Location L on I.LocationCode=L.LocationCode' + @CRLF

	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	I.DelFlag=''0''' + @CRLF
	SET @SQL = @SQL +'	AND I.PhysicalQuantity = 1' + @CRLF												--2018/05/17 arc yano #3885
	SET @SQL = @SQL +'	AND I.SalesCarNumber is not NULL' + @CRLF
	SET @SQL = @SQL +'	AND I.SalesCarNumber <> ''''' + @CRLF

	--検索条件の車台番号が入力されていた場合
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	EXISTS' + @CRLF
		SET @SQL = @SQL +'	(' + @CRLF
		SET @SQL = @SQL +'		SELECT ''x'' FROM SalesCar S WHERE S.Vin =''' + @Vin + '''' + 'AND I.SalesCarNumber = S.SalesCarNumber' + @CRLF
		SET @SQL = @SQL +'	)' + @CRLF
	END
	ELSE IF(@SalesCarNumber is not NULL AND @SalesCarNumber <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	I.SalesCarNumber = '''  + @SalesCarNumber + '''' + @CRLF
	END


	SET @SQL = @SQL +'	ORDER BY' + @CRLF
	SET @SQL = @SQL +'	 SlipDate, SlipTypeOrder' + @CRLF	

	EXECUTE sp_executeSQL @SQL

	--PRINT @SQL

/*
	SELECT
	  CONVERT(nvarchar(10), '') AS SlipTypeName
	, CONVERT(nvarchar(10), NULL) AS SlipTypeCode
	, CONVERT(nvarchar(50), NULL) AS SlipType
	, CONVERT(datetime, NULL) AS SlipDate
	, CONVERT(nvarchar(12), NULL) AS LocationCode
	, CONVERT(nvarchar(50), NULL) AS LocationName
	, CONVERT(nvarchar(20), NULL) AS CustomerCode
	, CONVERT(nvarchar(200), NULL) AS CustomerName
	, CONVERT(nvarchar(3), NULL) AS SlipStatusCode
	, CONVERT(nvarchar(50), NULL) AS SlipStatus
	, CONVERT(nvarchar(50), NULL) AS EmployeeCode
	, CONVERT(nvarchar(40), NULL) AS EmployeeName
	, CONVERT(nvarchar(50), NULL) AS SalesCarNumber
	, CONVERT(nvarchar(50), NULL) AS SlipNumber
	, CONVERT(decimal(10, 0), NULL) AS VehiclePrice
	, CONVERT(decimal(10, 0), NULL) AS Amount
	, CONVERT(int, NULL) AS SlipTypeOrder
*/

END


GO


