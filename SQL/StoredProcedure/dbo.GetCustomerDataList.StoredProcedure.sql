USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCustomerDataList]    Script Date: 2015/12/21 9:37:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






--2015/02/18 arc nakayama 顧客DM対応 顧客DMデータ取得をlinqからストプロに変更
--2015/04/08 arc nakayama 顧客DM対応 顧客コードと車両マスタのユーザーコードとの紐付をやめて、各伝票の顧客コードと紐付ける
--2015/04/10 arc nakayama 顧客DM指摘事項修正Part2 ナンバープレート情報をすべて出す 住所再確認を検索条件に加える
--2015/04/13 arc nakayama 顧客DM指摘事項修正Part3 顧客種別を検索条件に加える
--2015/05/20 arc nakayama 顧客DM項目追加　車両重量を出すようにする
--2015/06/22 arc nakayama 顧客DM修正　住所再確認の名称(要/不要)をストプロで返すように変更
--2015/07/01 arc nakayama #3220_顧客データリスト出力項目追加 DM発送先の顧客名
--2015/07/14 arc nakayama 伝票の抽出条件変更（受注以降　⇒　【車】納車前または納車済み　【サ】キャンセル/作業履歴/作業中止　以外）
--2015/07/15 arc nakayama 伝票の抽出条件変更　サービス伝票の抽出条件変更（納車年月日　⇒　入庫日）
--2015/07/17 arc nakayama #3224_「顧客データリスト」機能で携帯電話番号が出力されない  出力項目に携帯番号追加
--2015/07/22 arc nakayama ReadUncommitted追加
--2015/08/03 arc nakayama #3229_顧客データ抽出の不具合 出力項目に使用者を追加 
--2015/12/16 arc ookubo   #3318_メールアドレスを出力項目に追加 

CREATE PROCEDURE [dbo].[GetCustomerDataList]
	@DmFlag nvarchar(3),				 --DM可否
	@DepartmentCode2 nvarchar(3),		 --営業担当部門コード＜顧客情報＞
	@CarEmployeeCode nvarchar(50),		 --営業部門担当者コード＜顧客情報＞
	@ServiceDepartmentCode2 nvarchar(3), --サービス担当部門コード＜顧客情報＞
	@SalesDateFrom nvarchar(10),		 --車両伝票納車日From
	@SalesDateTo nvarchar(10),			 --車両伝票納車日To
	@ArrivalPlanDateFrom nvarchar(10),	 --サービス伝票入庫日From
	@ArrivalPlanDateTo nvarchar(10),	 --サービス伝票入庫日To
	@MakerName nvarchar(50),			 --メーカー名
	@CarName nvarchar(20),				 --車種名
	@FirstRegistrationDateFrom nvarchar(10), --初年度登録(YYYY/MM)From
	@FirstRegistrationDateTo nvarchar(10),	 --初年度登録(YYYY/MM)To 	
	@RegistrationDateFrom nvarchar(10),	 --登録年月日From
	@RegistrationDateTo nvarchar(10),	 --登録年月日To
	@CustomerAddressReconfirm bit,		 --住所再確認
	@CustomerKind nvarchar(3) 			 --顧客種別

AS
	
	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)
	
	/*-------------------------------------------*/
	/* ダーティーリードの設定					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/* 住所再確認要否文言マスタ					 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_CodeName_013 (
		[Code] Bit
	,	[ShortName] NVARCHAR(50)
	)
	INSERT INTO #temp_CodeName_013
	SELECT CASE [Code] WHEN '001' THEN 1 ELSE 0 END
    ,	[ShortName]
	FROM [c_CodeName]
	WHERE [CategoryCode] = '013'

	/*-------------------------------------------*/
	/* 顧客に関連する車両情報取得                */
	/*-------------------------------------------*/

	CREATE TABLE #temp_SalesCar_S (
		  SalesCarNumber nvarchar(50)
		, CarGradeCode nvarchar(30)
		, MorterViecleOfficialCode nvarchar(5)
		, RegistrationNumberType  nvarchar(3)
		, RegistrationNumberKana nvarchar(1)
		, RegistrationNumberPlate nvarchar(4)
		, RegistrationDate datetime
		, FirstRegistrationYear nvarchar(9)
		, Vin nvarchar(20)
		, [ExpireDate] datetime
		, NextInspectionDate datetime
		, UserCode nvarchar(10)
		, InspectGuidFlag nvarchar(3)
		, InspectGuidMemo nvarchar(100)
		, FirstRegistrationDate datetime
		, DelFlag nvarchar(2)
		, CarWeight int
	)

	SET @PARAM = '@RegistrationDateFrom nvarchar(10), @RegistrationDateTo nvarchar(10), @FirstRegistrationDateFrom nvarchar(10), @FirstRegistrationDateTo nvarchar(10)'

	

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_SalesCar_S' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  SalesCarNumber' + @CRLF
	SET @SQL = @SQL +'	, CarGradeCode' + @CRLF
	SET @SQL = @SQL +'	, MorterViecleOfficialCode' + @CRLF
    SET @SQL = @SQL +'	, RegistrationNumberType' + @CRLF
    SET @SQL = @SQL +'	, RegistrationNumberKana' + @CRLF
    SET @SQL = @SQL +'	, RegistrationNumberPlate' + @CRLF
	SET @SQL = @SQL +'	, RegistrationDate' + @CRLF
	SET @SQL = @SQL +'	, FirstRegistrationYear' + @CRLF
	SET @SQL = @SQL +'	, Vin' + @CRLF
	SET @SQL = @SQL +'	, ExpireDate' + @CRLF
	SET @SQL = @SQL +'	, NextInspectionDate' + @CRLF
	SET @SQL = @SQL +'	, UserCode' + @CRLF
	SET @SQL = @SQL +'	, InspectGuidFlag' + @CRLF
	SET @SQL = @SQL +'	, InspectGuidMemo' + @CRLF
	SET @SQL = @SQL +'	, FirstRegistrationDate' + @CRLF
	SET @SQL = @SQL +'	, DelFlag' + @CRLF
	SET @SQL = @SQL +'	, CarWeight' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.SalesCar' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    DelFlag = ''0''' + @CRLF 

	--登録年月日
	IF ((@RegistrationDateFrom is not null) AND (@RegistrationDateFrom <> '') AND ISDATE(@RegistrationDateFrom) = 1)
		IF ((@RegistrationDateTo is not null) AND (@RegistrationDateTo <> '') AND ISDATE(@RegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND RegistrationDate >= @RegistrationDateFrom AND RegistrationDate < DateAdd(d, 1, @RegistrationDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND RegistrationDate = @RegistrationDateFrom' + @CRLF 
		END
	ELSE
		IF ((@RegistrationDateTo is not null) AND (@RegistrationDateTo <> '') AND ISDATE(@RegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND RegistrationDate < DateAdd(d, 1, @RegistrationDateTo)' + @CRLF 
		END

	--初年度登録 FirstRegistrationDate
	IF ((@FirstRegistrationDateFrom is not null) AND (@FirstRegistrationDateFrom <> '') AND ISDATE(@FirstRegistrationDateFrom) = 1)
		IF ((@FirstRegistrationDateTo is not null) AND (@FirstRegistrationDateTo <> '') AND ISDATE(@FirstRegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND FirstRegistrationDate >= @FirstRegistrationDateFrom AND FirstRegistrationDate < DateAdd(d, 1, @FirstRegistrationDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND FirstRegistrationDate = @FirstRegistrationDateFrom' + @CRLF 
		END
	ELSE
		IF ((@FirstRegistrationDateTo is not null) AND (@FirstRegistrationDateTo <> '') AND ISDATE(@FirstRegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND FirstRegistrationDate < DateAdd(d, 1, @FirstRegistrationDateTo)' + @CRLF 
		END

	EXECUTE sp_executeSQL @SQL, @PARAM, @RegistrationDateFrom, @RegistrationDateTo, @FirstRegistrationDateFrom, @FirstRegistrationDateTo
	CREATE INDEX ix_temp_SalesCar_S ON #temp_SalesCar_S(UserCode)	--(DelFlag)


	/*-------------------------------------------*/
	/* 車両伝票の情報取得3(最終納車日特定)       */
	/*-------------------------------------------*/

	CREATE TABLE #temp_CarSalesHeader_4 (
			SalesCarNumber nvarchar(50)
		,	SalesDate datetime
		)

	SET @PARAM = '@SalesDateFrom nvarchar(10), @SalesDateTo nvarchar(10)'
	SET @SQL = ''
	SET @SQL = @SQL + 'INSERT INTO #temp_CarSalesHeader_4' + @CRLF
	SET @SQL = @SQL + 'SELECT' + @CRLF
	SET @SQL = @SQL + '  [SalesCarNumber]' + @CRLF
	SET @SQL = @SQL + ' ,MAX([SalesDate]) AS SalesDate' + @CRLF
	SET @SQL = @SQL + 'FROM' + @CRLF    
	SET @SQL = @SQL + '	dbo.CarSalesHeader' + @CRLF
	SET @SQL = @SQL + 'WHERE' + @CRLF
	SET @SQL = @SQL + '	 (DelFlag = ''0'')' + @CRLF
	SET @SQL = @SQL + ' AND SalesOrderStatus in (''004'', ''005'')' + @CRLF
	
	--SalesDate
	IF ((@SalesDateFrom is not null) AND (@SalesDateFrom <> '') AND ISDATE(@SalesDateFrom) = 1)
		IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND SalesDate >= @SalesDateFrom AND SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND SalesDate = @SalesDateFrom' + @CRLF 
		END
	ELSE
		IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF
		END

	SET @SQL = @SQL + 'GROUP BY' + @CRLF
	SET @SQL = @SQL + '  [SalesCarNumber]' + @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM, @SalesDateFrom, @SalesDateTo
	CREATE INDEX ix_temp_CarSalesHeader_H4 ON #temp_CarSalesHeader_4(SalesCarNumber, SalesDate)

	/*-------------------------------------------*/
	/* 車両伝票の情報取得2(最大リビジョン特定)   */
	/*-------------------------------------------*/
	CREATE TABLE #temp_CarSalesHeader_3 (
			SalesCarNumber nvarchar(50)
		,	SlipNumber nvarchar(50)
		,	RevisionNumber int
		)

	SET @PARAM = '@SalesDateFrom nvarchar(10), @SalesDateTo nvarchar(10)'
	SET @SQL = ''
	SET @SQL = @SQL + 'INSERT INTO #temp_CarSalesHeader_3' + @CRLF
	SET @SQL = @SQL + 'SELECT' + @CRLF
	SET @SQL = @SQL + '  h3.[SalesCarNumber]' + @CRLF
	SET @SQL = @SQL + ', h3.[SlipNumber]' + @CRLF
	SET @SQL = @SQL + ', MAX(h3.[RevisionNumber])' + @CRLF
	SET @SQL = @SQL + 'FROM' + @CRLF    
	SET @SQL = @SQL + '	dbo.CarSalesHeader h3' + @CRLF
	SET @SQL = @SQL + 'WHERE' + @CRLF
	SET @SQL = @SQL + '	 (h3.DelFlag = ''0'')' + @CRLF
	SET @SQL = @SQL + ' AND h3.SalesOrderStatus in (''004'', ''005'')' + @CRLF
	
	--h3.SalesDate
	IF ((@SalesDateFrom is not null) AND (@SalesDateFrom <> '') AND ISDATE(@SalesDateFrom) = 1)
		IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND h3.SalesDate >= @SalesDateFrom AND h3.SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND h3.SalesDate = @SalesDateFrom' + @CRLF 
		END
	ELSE
		IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND h3.SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF
		END

	SET @SQL = @SQL + ' AND EXISTS(SELECT 1 FROM #temp_CarSalesHeader_4 h4 WHERE h4.SalesCarNumber = h3.SalesCarNumber AND h4.SalesDate = h3.SalesDate)' + @CRLF
	SET @SQL = @SQL + 'GROUP BY' + @CRLF
	SET @SQL = @SQL + '  h3.[SalesCarNumber]' + @CRLF
	SET @SQL = @SQL + ', h3.[SlipNumber]' + @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM, @SalesDateFrom, @SalesDateTo
	CREATE INDEX ix_temp_CarSalesHeader_H3 ON #temp_CarSalesHeader_3(SlipNumber, RevisionNumber)

	/*-------------------------------------------------------------------------------------------------------------------*/
	/* 車両伝票の情報取得2																							     */
	/* ※1人の顧客が同じ車台番号に対して複数の伝票を切っていて、かつ、納車日が同じものを除外する						 */
	/*-------------------------------------------------------------------------------------------------------------------*/

	CREATE TABLE #temp_CarSalesHeader_2 (
			SalesCarNumber nvarchar(50)
		,	SlipNumber nvarchar(50)
		)

	SET @PARAM = ''
	SET @SQL = ''
	SET @SQL = @SQL + 'INSERT INTO #temp_CarSalesHeader_2' + @CRLF
	SET @SQL = @SQL + 'SELECT' + @CRLF
	SET @SQL = @SQL + '	h3.SalesCarNumber' + @CRLF
	SET @SQL = @SQL + ',MAX(h3.SlipNumber)' + @CRLF
	SET @SQL = @SQL + 'FROM #temp_CarSalesHeader_3 h3' + @CRLF    
	

	SET @SQL = @SQL + 'GROUP BY' + @CRLF
	SET @SQL = @SQL + ' h3.[SalesCarNumber]' + @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM
	CREATE INDEX ix_temp_CarSalesHeader_2 ON #temp_CarSalesHeader_2(SlipNumber)


	/*-------------------------------------------*/
	/* 車両伝票の情報取得1（伝票の特定）         */
	/*-------------------------------------------*/

	CREATE TABLE #temp_CarSalesHeader_1 (
		
		 SlipNumber nvarchar(50)
	   , SalesOrderStatus nvarchar(3)
	   , SalesDate datetime
	   , CustomerCode nvarchar(10)
	   , DepartmentCode nvarchar(3)
	   , EmployeeCode nvarchar(50)
	   , Vin nvarchar(20)
	   , SalesCarNumber nvarchar(50)
	   , DelFlag nvarchar(2)

		)

	SET @PARAM = '@SalesDateFrom nvarchar(10), @SalesDateTo nvarchar(10)'
	SET @SQL = ''
	SET @SQL = @SQL + 'INSERT INTO #temp_CarSalesHeader_1' + @CRLF
	SET @SQL = @SQL + 'SELECT' + @CRLF
	SET @SQL = @SQL + ' h.SlipNumber' + @CRLF
	SET @SQL = @SQL + ',h.SalesOrderStatus' + @CRLF
	SET @SQL = @SQL + ',h.SalesDate' + @CRLF
	SET @SQL = @SQL + ',h.CustomerCode' + @CRLF
	SET @SQL = @SQL + ',h.DepartmentCode' + @CRLF
	SET @SQL = @SQL + ',h.EmployeeCode' + @CRLF
	SET @SQL = @SQL + ',h.Vin' + @CRLF
	SET @SQL = @SQL + ',h.SalesCarNumber' + @CRLF
	SET @SQL = @SQL + ',h.DelFlag' + @CRLF
	SET @SQL = @SQL + 'FROM dbo.CarSalesHeader AS h' + @CRLF
	SET @SQL = @SQL + 'WHERE EXISTS(' + @CRLF
	SET @SQL = @SQL + '		SELECT 1 ' + @CRLF
	SET @SQL = @SQL + '		FROM #temp_CarSalesHeader_3 h3 ' + @CRLF
	SET @SQL = @SQL + '		INNER JOIN #temp_CarSalesHeader_2 h2 ON h2.SlipNumber = h3.SlipNumber' + @CRLF
	SET @SQL = @SQL + '		WHERE h3.SlipNumber = h.SlipNumber And h3.RevisionNumber = h.RevisionNumber)' + @CRLF

	--H.SalesDate
	IF ((@SalesDateFrom is not null) AND (@SalesDateFrom <> '') AND ISDATE(@SalesDateFrom) = 1)
		IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND H.SalesDate >= @SalesDateFrom AND H.SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND H.SalesDate = @SalesDateFrom' + @CRLF 
		END
	ELSE
		IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND H.SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF
		END

	EXECUTE sp_executeSQL @SQL, @PARAM, @SalesDateFrom, @SalesDateTo
	CREATE INDEX ix_temp_CarSalesHeader_H_1 ON #temp_CarSalesHeader_1(CustomerCode, SalesCarNumber)


	/*-------------------------------------------*/
	/* サービス伝票の情報取得3(最終納車日特定)　 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_ServiceSalesHeader_4 (
			SalesCarNumber nvarchar(50)
		,	ArrivalPlanDate datetime
		)
	
	SET @PARAM = '@ArrivalPlanDateFrom nvarchar(10), @ArrivalPlanDateTo nvarchar(10)'
	SET @SQL = ''
	SET @SQL = @SQL + 'INSERT INTO #temp_ServiceSalesHeader_4' + @CRLF
	SET @SQL = @SQL + 'SELECT' + @CRLF
	SET @SQL = @SQL + '  [SalesCarNumber]' + @CRLF
	SET @SQL = @SQL + ' ,MAX([ArrivalPlanDate]) AS ArrivalPlanDate' + @CRLF
	SET @SQL = @SQL + 'FROM' + @CRLF    
	SET @SQL = @SQL + '	dbo.ServiceSalesHeader' + @CRLF
	SET @SQL = @SQL + 'WHERE' + @CRLF
	SET @SQL = @SQL + '	 (DelFlag = ''0'')' + @CRLF
	SET @SQL = @SQL + ' AND ServiceOrderStatus in (''001'',''002'',''003'',''004'',''005'', ''006'')' + @CRLF

	--ArrivalPlanDate 
	IF ((@ArrivalPlanDateFrom is not null) AND (@ArrivalPlanDateFrom <> '') AND ISDATE(@ArrivalPlanDateFrom) = 1)
		IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND ArrivalPlanDate >= @ArrivalPlanDateFrom AND ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND ArrivalPlanDate = @ArrivalPlanDateFrom' + @CRLF 
		END
	ELSE
		IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF 
		END

	SET @SQL = @SQL + 'GROUP BY' + @CRLF
	SET @SQL = @SQL + '  [SalesCarNumber]' + @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM, @ArrivalPlanDateFrom, @ArrivalPlanDateTo
	CREATE INDEX ix_temp_ServiceSalesHeader_SH4 ON #temp_ServiceSalesHeader_4(SalesCarNumber, ArrivalPlanDate)

	/*---------------------------------------------*/
	/* サービス伝票の情報取得2(最大リビジョン特定) */
	/*---------------------------------------------*/

	CREATE TABLE #temp_ServiceSalesHeader_3 (
			SalesCarNumber nvarchar(50)
		,	SlipNumber nvarchar(50)
		,	RevisionNumber int
		)
	SET @PARAM = '@ArrivalPlanDateFrom nvarchar(10), @ArrivalPlanDateTo nvarchar(10)'
	SET @SQL = ''
	SET @SQL = @SQL + 'INSERT INTO #temp_ServiceSalesHeader_3' + @CRLF
	SET @SQL = @SQL + 'SELECT' + @CRLF
	SET @SQL = @SQL + '  h3.[SalesCarNumber]' + @CRLF
	SET @SQL = @SQL + ', h3.[SlipNumber]' + @CRLF
	SET @SQL = @SQL + ', MAX(h3.[RevisionNumber])' + @CRLF
	SET @SQL = @SQL + 'FROM' + @CRLF    
	SET @SQL = @SQL + '	dbo.ServiceSalesHeader h3' + @CRLF
	SET @SQL = @SQL + 'WHERE' + @CRLF
	SET @SQL = @SQL + '	 (h3.DelFlag = ''0'')' + @CRLF
	SET @SQL = @SQL + ' AND h3.ServiceOrderStatus in (''001'',''002'',''003'',''004'',''005'', ''006'')' + @CRLF

	--h3.ArrivalPlanDate
	IF ((@ArrivalPlanDateFrom is not null) AND (@ArrivalPlanDateFrom <> '') AND ISDATE(@ArrivalPlanDateFrom) = 1)
		IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND h3.ArrivalPlanDate >= @ArrivalPlanDateFrom AND h3.ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND h3.ArrivalPlanDate = @ArrivalPlanDateFrom' + @CRLF 
		END
	ELSE
		IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND h3.ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF 
		END

	SET @SQL = @SQL + ' AND EXISTS(SELECT 1 FROM #temp_ServiceSalesHeader_4 h4 WHERE h4.SalesCarNumber = h3.SalesCarNumber AND h4.ArrivalPlanDate = h3.ArrivalPlanDate)' + @CRLF

	SET @SQL = @SQL + 'GROUP BY' + @CRLF
	SET @SQL = @SQL + '  h3.[SalesCarNumber]' + @CRLF
	SET @SQL = @SQL + ', h3.[SlipNumber]' + @CRLF


	EXECUTE sp_executeSQL @SQL, @PARAM, @ArrivalPlanDateFrom, @ArrivalPlanDateTo
	CREATE INDEX ix_temp_ServiceSalesHeader_H3 ON #temp_ServiceSalesHeader_3(SlipNumber, RevisionNumber)

	/*-------------------------------------------------------------------------------------------------------------------*/
	/* サービス伝票の情報取得4																							 */
	/* 同じ管理番号に対して複数の伝票を切っていて、かつ、納車日が同じものを除外する						                 */
	/*-------------------------------------------------------------------------------------------------------------------*/

	CREATE TABLE #temp_ServiceSalesHeader_2 (
		  SalesCarNumber nvarchar(50)
		, SlipNumber nvarchar(50)
		)

	SET @PARAM = ''
	SET @SQL = ''
	SET @SQL = @SQL + 'INSERT INTO #temp_ServiceSalesHeader_2' + @CRLF
	SET @SQL = @SQL + 'SELECT' + @CRLF
	SET @SQL = @SQL + '  h3.SalesCarNumber' + @CRLF
	SET @SQL = @SQL + ', MAX(h3.SlipNumber)' + @CRLF
	SET @SQL = @SQL + 'FROM #temp_ServiceSalesHeader_3 AS h3' + @CRLF    

	SET @SQL = @SQL + 'GROUP BY' + @CRLF
	SET @SQL = @SQL + '  h3.[SalesCarNumber]' + @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM
	CREATE INDEX ix_temp_ServiceSalesHeader_2 ON #temp_ServiceSalesHeader_2(SlipNumber)



	/*-------------------------------------------*/
	/* サービス伝票の情報取得1（伝票の特定）	 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_ServiceSalesHeader_1 (
		  SlipNumber nvarchar(50)
		, ServiceOrderStatus nvarchar(3)
		, ArrivalPlanDate datetime
		, CustomerCode nvarchar(10)
		, DepartmentCode nvarchar(3)
		, ReceiptionEmployeeCode nvarchar(50)
		, Vin nvarchar(20)
		, SalesCarNumber nvarchar(50)
		, DelFlag nvarchar(2)
		)

	SET @PARAM = '@ArrivalPlanDateFrom nvarchar(10), @ArrivalPlanDateTo nvarchar(10)'
	SET @SQL = ''
	SET @SQL = @SQL + 'INSERT INTO #temp_ServiceSalesHeader_1' + @CRLF
	SET @SQL = @SQL + 'SELECT' + @CRLF
	SET @SQL = @SQL + '	 h.SlipNumber' + @CRLF
	SET @SQL = @SQL + ', h.ServiceOrderStatus' + @CRLF
	SET @SQL = @SQL + ', h.ArrivalPlanDate' + @CRLF
	SET @SQL = @SQL + ', h.CustomerCode' + @CRLF
	SET @SQL = @SQL + ', h.DepartmentCode' + @CRLF
	SET @SQL = @SQL + ', h.ReceiptionEmployeeCode' + @CRLF
	SET @SQL = @SQL + ', h.Vin' + @CRLF
	SET @SQL = @SQL + ', h.SalesCarNumber' + @CRLF
	SET @SQL = @SQL + ', h.DelFlag' + @CRLF
    
	SET @SQL = @SQL + 'FROM dbo.ServiceSalesHeader AS h' + @CRLF    
	SET @SQL = @SQL + 'WHERE EXISTS(' + @CRLF    
	SET @SQL = @SQL + '		SELECT 1' + @CRLF    
	SET @SQL = @SQL + '		FROM #temp_ServiceSalesHeader_3 h3' + @CRLF    
	SET @SQL = @SQL + '		INNER JOIN #temp_ServiceSalesHeader_2 h2 ON h2.SlipNumber = h3.SlipNumber' + @CRLF    
	SET @SQL = @SQL + '		WHERE h3.SlipNumber = h.SlipNumber And h3.RevisionNumber = h.RevisionNumber)' + @CRLF
	
	-- h.ArrivalPlanDate @ArrivalPlanDateDate
	IF ((@ArrivalPlanDateFrom is not null) AND (@ArrivalPlanDateFrom <> '') AND ISDATE(@ArrivalPlanDateFrom) = 1)
		IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND h.ArrivalPlanDate >= @ArrivalPlanDateFrom AND h.ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND h.ArrivalPlanDate = @ArrivalPlanDateFrom' + @CRLF 
		END
	ELSE
		IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND h.ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF 
		END

	EXECUTE sp_executeSQL @SQL, @PARAM, @ArrivalPlanDateFrom, @ArrivalPlanDateTo
	CREATE INDEX ix_temp_ServiceSalesHeader_1 ON #temp_ServiceSalesHeader_1(CustomerCode, SalesCarNumber)

	/*-------------------------------------------*/
	/* 車両マスタ取得			　　			 */
	/*-------------------------------------------*/
	
	SET @PARAM = '@MakerName nvarchar(50), @CarName nvarchar(20)'
	CREATE TABLE #temp_CarMaster_VC (
	  MakerName nvarchar(50)
	, CarName nvarchar(50)
	, CarGradeCode nvarchar(30)
    )

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_CarMaster_VC' + @CRLF

	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'  M.MakerName' + @CRLF
	SET @SQL = @SQL +', CA.CarName' + @CRLF
	SET @SQL = @SQL +', CG.CarGradeCode' + @CRLF
	SET @SQL = @SQL +'FROM'  + @CRLF
	SET @SQL = @SQL +'dbo.Brand AS B' + @CRLF
	SET @SQL = @SQL +'INNER JOIN dbo.Maker AS M ON B.MakerCode = M.MakerCode' + @CRLF
	SET @SQL = @SQL +'INNER JOIN dbo.Car AS CA ON B.CarBrandCode = CA.CarBrandCode' + @CRLF
	SET @SQL = @SQL +'INNER JOIN dbo.CarGrade CG ON CA.CarCode = CG.CarCode' + @CRLF
	SET @SQL = @SQL +'WHERE 1=1' + @CRLF

	IF ((@MakerName IS NOT NULL) AND (@MakerName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND M.MakerName = @MakerName' + @CRLF
	END

	IF ((@CarName IS NOT NULL) AND (@CarName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND CA.CarName = @CarName' + @CRLF
	END

	EXECUTE sp_executeSQL @SQL, @PARAM, @MakerName, @CarName
	CREATE INDEX ix_temp_CarMaster_VC ON #temp_CarMaster_VC(CarGradeCode)



	/*-------------------------------------------*/
	/* 車両伝票とサービス伝票の結果を結合        */
	/* 顧客コードと伝票の車両を紐付ける準備      */
	/*-------------------------------------------*/

		CREATE TABLE #temp_Header (
		  CustomerCode nvarchar(10)
		, SalesCarNumber nvarchar(50)
		, SalesDate datetime
	    , DepartmentCode nvarchar(3)
	    , EmployeeCode nvarchar(50)
		, ArrivalPlanDate datetime
		, ServiceDepartmentCode nvarchar(3)
		, ReceiptionEmployeeCode nvarchar(50)
		)

	SET @PARAM = ''		
	SET @SQL = ''	
	SET @SQL = @SQL + 'INSERT INTO #temp_Header' + @CRLF
	SET @SQL = @SQL + 'SELECT' + @CRLF
	SET @SQL = @SQL + 'CASE WHEN H.CustomerCode is null THEN sh.CustomerCode ' + @CRLF
	SET @SQL = @SQL + 'ELSE H.CustomerCode' + @CRLF
	SET @SQL = @SQL + 'END' + @CRLF
	SET @SQL = @SQL + ', CASE WHEN H.SalesCarNumber is null THEN sh.SalesCarNumber ' + @CRLF
	SET @SQL = @SQL + 'ELSE H.SalesCarNumber' + @CRLF
	SET @SQL = @SQL + 'END' + @CRLF
	SET @SQL = @SQL + '	, h.SalesDate' + @CRLF
	SET @SQL = @SQL + '	, h.DepartmentCode' + @CRLF
	SET @SQL = @SQL + '	, h.EmployeeCode' + @CRLF
	SET @SQL = @SQL + '	, sh.ArrivalPlanDate' + @CRLF
	SET @SQL = @SQL + '	, sh.DepartmentCode' + @CRLF
	SET @SQL = @SQL + '	, sh.ReceiptionEmployeeCode' + @CRLF


	SET @SQL = @SQL +'FROM'  + @CRLF
	SET @SQL = @SQL +'#temp_CarSalesHeader_1 AS H' + @CRLF
	SET @SQL = @SQL +'FULL OUTER JOIN #temp_ServiceSalesHeader_1 AS sh ON sh.CustomerCode = H.CustomerCode AND sh.SalesCarNumber = H.SalesCarNumber' + @CRLF


	EXECUTE sp_executeSQL @SQL, @PARAM
	CREATE INDEX ix_temp_ServiceCustomer ON #temp_Header(CustomerCode, SalesCarNumber)


	/*---------------------------------------------------------*/
	/* 伝票の管理番号と車両情報の管理番号に紐ずく顧客コード取得*/
	/*---------------------------------------------------------*/
		CREATE TABLE #temp_CustomerCar (
		  CustomerCode nvarchar(10)
		, SalesCarNumber nvarchar(50)
		, CarGradeCode nvarchar(30)
		, MorterViecleOfficialCode nvarchar(5)
		, RegistrationNumberType  nvarchar(3)
		, RegistrationNumberKana nvarchar(1)
		, RegistrationNumberPlate nvarchar(4)
		, RegistrationDate datetime
		, FirstRegistrationYear nvarchar(9)
		, Vin nvarchar(20)
		, [ExpireDate] datetime
		, NextInspectionDate datetime
		, InspectGuidFlag nvarchar(3)
		, InspectGuidMemo nvarchar(100)
		, FirstRegistrationDate datetime
		, SalesDate datetime
	    , DepartmentCode nvarchar(3)
	    , EmployeeCode nvarchar(50)
		, ArrivalPlanDate datetime
		, ServiceDepartmentCode nvarchar(3)
		, ReceiptionEmployeeCode nvarchar(50)
		, CarWeight int
		, UserCode nvarchar(10)
		
		)

SET @PARAM = ''
	SET @SQL = ''
	SET @SQL = @SQL + 'INSERT INTO #temp_CustomerCar' + @CRLF	
	SET @SQL = @SQL + 'SELECT' + @CRLF
	SET @SQL = @SQL + 'CASE WHEN h.CustomerCode is not null THEN h.CustomerCode' + @CRLF
	SET @SQL = @SQL + 'ELSE S.UserCode' + @CRLF
	SET @SQL = @SQL + 'END' + @CRLF

	SET @SQL = @SQL + '	, S.SalesCarNumber' + @CRLF
	SET @SQL = @SQL + '	, S.CarGradeCode' + @CRLF
	SET @SQL = @SQL + '	, S.MorterViecleOfficialCode' + @CRLF
    SET @SQL = @SQL +'	, S.RegistrationNumberType' + @CRLF
    SET @SQL = @SQL +'	, S.RegistrationNumberKana' + @CRLF
    SET @SQL = @SQL +'	, S.RegistrationNumberPlate' + @CRLF
	SET @SQL = @SQL + '	, S.RegistrationDate' + @CRLF
	SET @SQL = @SQL + '	, S.FirstRegistrationYear' + @CRLF
	SET @SQL = @SQL + '	, S.Vin' + @CRLF
	SET @SQL = @SQL + '	, S.[ExpireDate]' + @CRLF
	SET @SQL = @SQL + '	, S.NextInspectionDate' + @CRLF
	SET @SQL = @SQL + '	, S.InspectGuidFlag' + @CRLF
	SET @SQL = @SQL + '	, S.InspectGuidMemo' + @CRLF
	SET @SQL = @SQL + '	, S.FirstRegistrationDate' + @CRLF
	SET @SQL = @SQL + '	, h.SalesDate' + @CRLF
	SET @SQL = @SQL + '	, h.DepartmentCode' + @CRLF
	SET @SQL = @SQL + '	, h.EmployeeCode' + @CRLF
	SET @SQL = @SQL + '	, h.ArrivalPlanDate' + @CRLF
	SET @SQL = @SQL + '	, h.ServiceDepartmentCode' + @CRLF
	SET @SQL = @SQL + '	, h.ReceiptionEmployeeCode' + @CRLF
	SET @SQL = @SQL + '	, S.CarWeight' + @CRLF
	SET @SQL = @SQL + '	, S.UserCode' + @CRLF

	SET @SQL = @SQL +'FROM'  + @CRLF
	SET @SQL = @SQL +'#temp_SalesCar_S AS S' + @CRLF
	SET @SQL = @SQL +'LEFT OUTER JOIN #temp_Header AS h on h.SalesCarNumber = S.SalesCarNumber' + @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM
	CREATE INDEX ix_temp_CustomerCar ON #temp_CustomerCar(CustomerCode, SalesCarNumber)


	/*-------------------------------------------*/
	/* 顧客データリスト取得						 */
	/*-------------------------------------------*/

	SET @PARAM = '@DmFlag nvarchar(3),@DepartmentCode2 nvarchar(3),@CarEmployeeCode nvarchar(50), @ServiceDepartmentCode2 nvarchar(3), @SalesDateFrom nvarchar(10),@SalesDateTo nvarchar(10),@ArrivalPlanDateFrom nvarchar(10),@ArrivalPlanDateTo nvarchar(10),@MakerName nvarchar(50), @CarName nvarchar(20), @FirstRegistrationDateFrom nvarchar(10), @FirstRegistrationDateTo nvarchar(10), @RegistrationDateFrom nvarchar(10), @RegistrationDateTo nvarchar(10), @CustomerAddressReconfirm bit, @CustomerKind nvarchar(3)'

	SET @SQL = ''
	SET @SQL = @SQL + 'SELECT'+ @CRLF
	SET @SQL = @SQL + '   C.DmFlag'+ @CRLF
	SET @SQL = @SQL + ' , AO1.Name AS DmFlagName'+ @CRLF
	SET @SQL = @SQL + ' , C.DmMemo'+ @CRLF
	SET @SQL = @SQL + ' , S.InspectGuidFlag'+ @CRLF
	SET @SQL = @SQL + ' , AO2.Name AS InspectGuidFlagName'+ @CRLF
	SET @SQL = @SQL + ' , S.InspectGuidMemo'+ @CRLF
	SET @SQL = @SQL + ' , C.CustomerCode'+ @CRLF
	SET @SQL = @SQL + ' , C.CustomerName'+ @CRLF
	SET @SQL = @SQL + ' , C.CustomerKind'+ @CRLF
	SET @SQL = @SQL + ' , CK.Name AS CustomerKindName'+ @CRLF
	SET @SQL = @SQL + ' , C.DepartmentCode AS DepartmentCode2'+ @CRLF
	SET @SQL = @SQL + ' , D.DepartmentName AS DepartmentName2'+ @CRLF
	SET @SQL = @SQL + ' , C.CarEmployeeCode'+ @CRLF
	SET @SQL = @SQL + ' , E1.EmployeeName AS CarEmployeeName'+ @CRLF
	SET @SQL = @SQL + ' , C.ServiceDepartmentCode AS ServiceDepartmentCode2'+ @CRLF
	SET @SQL = @SQL + ' , D4.DepartmentName AS ServiceDepartmentName2'+ @CRLF
	SET @SQL = @SQL + ' , C.ServiceEmployeeCode AS ServiceEmployeeCode2'+ @CRLF
	SET @SQL = @SQL + ' , E2.EmployeeName AS ServiceEmployeeName2'+ @CRLF
	SET @SQL = @SQL + ' , C.PostCode'+ @CRLF
	SET @SQL = @SQL + ' , C.Prefecture'+ @CRLF
	SET @SQL = @SQL + ' , C.City'+ @CRLF
	SET @SQL = @SQL + ' , C.Address1'+ @CRLF
	SET @SQL = @SQL + ' , C.Address2'+ @CRLF
	SET @SQL = @SQL + ' , C.TelNumber'+ @CRLF
	SET @SQL = @SQL + ' , C.MobileNumber'+ @CRLF
	SET @SQL = @SQL + ' , CN013.ShortName AS CustomerAddressReconfirmName'+ @CRLF
	SET @SQL = @SQL + ' , CD.FirstName'+ @CRLF
	SET @SQL = @SQL + ' , CD.LastName'+ @CRLF
	SET @SQL = @SQL + ' , CD.PostCode AS CDPostCode'+ @CRLF
	SET @SQL = @SQL + ' , CD.Prefecture AS CDPrefecture'+ @CRLF
	SET @SQL = @SQL + ' , CD.City AS CDCity'+ @CRLF
	SET @SQL = @SQL + ' , CD.Address1 AS CDAddress1'+ @CRLF
	SET @SQL = @SQL + ' , CD.Address2 AS CDAddress2'+ @CRLF
	SET @SQL = @SQL + ' , CD.TelNumber AS CDTelNumber'+ @CRLF
	SET @SQL = @SQL + ' , S.SalesCarNumber'+ @CRLF
	SET @SQL = @SQL + ' , S.MorterViecleOfficialCode'+ @CRLF
    SET @SQL = @SQL +'	, S.RegistrationNumberType' + @CRLF
    SET @SQL = @SQL +'	, S.RegistrationNumberKana' + @CRLF
    SET @SQL = @SQL +'	, S.RegistrationNumberPlate' + @CRLF
	SET @SQL = @SQL + ' , S.Vin'+ @CRLF
	SET @SQL = @SQL + ' , VC.MakerName'+ @CRLF
	SET @SQL = @SQL + ' , VC.CarName'+ @CRLF
	SET @SQL = @SQL + ' , S.FirstRegistrationYear'+ @CRLF
	SET @SQL = @SQL + ' , S.RegistrationDate'+ @CRLF
	SET @SQL = @SQL + ' , S.NextInspectionDate'+ @CRLF
	SET @SQL = @SQL + ' , S.ExpireDate'+ @CRLF
	SET @SQL = @SQL + ' , S.DepartmentCode'+ @CRLF
	SET @SQL = @SQL + ' , D2.DepartmentName'+ @CRLF
	SET @SQL = @SQL + ' , S.EmployeeCode'+ @CRLF
	SET @SQL = @SQL + ' , E3.EmployeeName'+ @CRLF
	SET @SQL = @SQL + ' , S.SalesDate'+ @CRLF
	SET @SQL = @SQL + ' , S.ServiceDepartmentCode AS ServiceDepartmentCode'+ @CRLF
	SET @SQL = @SQL + ' , D3.DepartmentName AS ServiceDepartmentName'+ @CRLF
	SET @SQL = @SQL + ' , S.ReceiptionEmployeeCode'+ @CRLF
	SET @SQL = @SQL + ' , E4.EmployeeName AS ServiceEmployeeName'+ @CRLF
	SET @SQL = @SQL + ' , S.ArrivalPlanDate AS ArrivalPlanDate'+ @CRLF
	SET @SQL = @SQL + ' , S.FirstRegistrationDate'+ @CRLF
	SET @SQL = @SQL + ' , S.CarWeight'+ @CRLF
	SET @SQL = @SQL + ' , CD.DelFlag AS DmDelFlag'+ @CRLF
	SET @SQL = @SQL + '	, S.UserCode' + @CRLF
	SET @SQL = @SQL + '	, C.MailAddress' + @CRLF
	SET @SQL = @SQL + '	, C.MobileMailAddress' + @CRLF

    SET @SQL = @SQL + 'FROM dbo.Customer AS C'+ @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CustomerCar AS S ON C.CustomerCode = S.CustomerCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CarMaster_VC AS VC ON S.CarGradeCode = VC.CarGradeCode'+ @CRLF

	SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E3 ON S.EmployeeCode = E3.EmployeeCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D2 ON S.DepartmentCode = D2.DepartmentCode'+ @CRLF

    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E4 ON S.ReceiptionEmployeeCode = E4.EmployeeCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D3 ON S.ServiceDepartmentCode = D3.DepartmentCode'+ @CRLF


    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.CustomerClaim AS CC ON C.CustomerClaimCode = CC.CustomerClaimCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON C.DepartmentCode = D.DepartmentCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D4 ON C.ServiceDepartmentCode = D4.DepartmentCode'+ @CRLF

    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_Allowance AS C1 ON C.DmFlag = C1.Code'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E1 ON C.CarEmployeeCode = E1.EmployeeCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E2 ON C.ServiceEmployeeCode = E2.EmployeeCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS sp ON C.CustomerClaimCode = sp.SupplierCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.CustomerDM AS CD ON C.CustomerCode = CD.CustomerCode'+ @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_Allowance AS AO1 ON C.DmFlag = AO1.Code'+ @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_Allowance AS AO2 ON S.InspectGuidFlag = AO2.Code'+ @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_CustomerKind AS CK ON C.CustomerKind = CK.Code'+ @CRLF

	SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CodeName_013 AS CN013 ON C.AddressReconfirm = CN013.Code'+ @CRLF

	SET @SQL = @SQL + 'WHERE'+ @CRLF
	SET @SQL = @SQL + '   (C.DelFlag = ''0'')'+ @CRLF

	--DM可否
	IF ((@DmFlag != '000') AND (@DmFlag <>''))
		BEGIN
			SET @SQL = @SQL + 'AND C.DmFlag = @DmFlag'+ @CRLF
		END
	
	--住所再確認
	IF((@CustomerAddressReconfirm IS NOT NULL))
	BEGIN
		SET @SQL = @SQL +'AND C.AddressReconfirm = @CustomerAddressReconfirm' + @CRLF
	END

	--顧客種別
	IF ((@CustomerKind IS NOT NULL) AND (@CustomerKind <>''))
		BEGIN
			SET @SQL = @SQL + 'AND C.CustomerKind = @CustomerKind'+ @CRLF
		END

	IF ((@DepartmentCode2 IS NOT NULL) AND (@DepartmentCode2 <>''))
		BEGIN
			SET @SQL = @SQL + 'AND C.DepartmentCode = @DepartmentCode2'+ @CRLF
		END

	IF ((@CarEmployeeCode IS NOT NULL) AND (@CarEmployeeCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND C.CarEmployeeCode = @CarEmployeeCode'+ @CRLF
		END

	IF ((@ServiceDepartmentCode2 IS NOT NULL) AND (@ServiceDepartmentCode2 <>''))
		BEGIN
			SET @SQL = @SQL + 'AND C.ServiceDepartmentCode = @ServiceDepartmentCode2'+ @CRLF
		END
		
				
	IF ((@MakerName IS NOT NULL) AND (@MakerName <>''))
		BEGIN
			SET @SQL = @SQL + 'AND VC.MakerName = @MakerName'+ @CRLF
		END	

	IF ((@CarName IS NOT NULL) AND (@CarName <>''))
	BEGIN
		SET @SQL = @SQL + 'AND VC.CarName = @CarName'+ @CRLF
	END

	--車両伝票納車日 S.SalesDate  @SalesDate
	IF ((@SalesDateFrom is not null) AND (@SalesDateFrom <> '') AND ISDATE(@SalesDateFrom) = 1)
		IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.SalesDate >= @SalesDateFrom AND S.SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND S.SalesDate = @SalesDateFrom' + @CRLF 
		END
	ELSE
		IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF
		END

	--サービス伝票入庫日 S.ArrivalPlanDate  @ArrivalPlanDate
	IF ((@ArrivalPlanDateFrom is not null) AND (@ArrivalPlanDateFrom <> '') AND ISDATE(@ArrivalPlanDateFrom) = 1)
		IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.ArrivalPlanDate >= @ArrivalPlanDateFrom AND S.ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND S.ArrivalPlanDate = @ArrivalPlanDateFrom' + @CRLF 
		END
	ELSE
		IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF 
		END
	

	--登録年月日
	IF ((@RegistrationDateFrom is not null) AND (@RegistrationDateFrom <> '') AND ISDATE(@RegistrationDateFrom) = 1)
		IF ((@RegistrationDateTo is not null) AND (@RegistrationDateTo <> '') AND ISDATE(@RegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.RegistrationDate >= @RegistrationDateFrom AND S.RegistrationDate < DateAdd(d, 1, @RegistrationDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND S.RegistrationDate = @RegistrationDateFrom' + @CRLF 
		END
	ELSE
		IF ((@RegistrationDateTo is not null) AND (@RegistrationDateTo <> '') AND ISDATE(@RegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.RegistrationDate < DateAdd(d, 1, @RegistrationDateTo)' + @CRLF 
		END

	--初年度登録 FirstRegistrationDate
	IF ((@FirstRegistrationDateFrom is not null) AND (@FirstRegistrationDateFrom <> '') AND ISDATE(@FirstRegistrationDateFrom) = 1)
		IF ((@FirstRegistrationDateTo is not null) AND (@FirstRegistrationDateTo <> '') AND ISDATE(@FirstRegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.FirstRegistrationDate >= @FirstRegistrationDateFrom AND S.FirstRegistrationDate < DateAdd(d, 1, @FirstRegistrationDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND S.FirstRegistrationDate = @FirstRegistrationDateFrom' + @CRLF 
		END
	ELSE
		IF ((@FirstRegistrationDateTo is not null) AND (@FirstRegistrationDateTo <> '') AND ISDATE(@FirstRegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.FirstRegistrationDate < DateAdd(d, 1, @FirstRegistrationDateTo)' + @CRLF 
		END


	SET @SQL = @SQL +'ORDER BY C.CustomerCode, S.SalesCarNumber' + @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM,@DmFlag, @DepartmentCode2,@CarEmployeeCode, @ServiceDepartmentCode2, @SalesDateFrom, @SalesDateTo ,@ArrivalPlanDateFrom , @ArrivalPlanDateTo, @MakerName, @CarName, @FirstRegistrationDateFrom, @FirstRegistrationDateTo, @RegistrationDateFrom, @RegistrationDateTo, @CustomerAddressReconfirm, @CustomerKind


BEGIN

	BEGIN TRY
		DROP TABLE #temp_SalesCar_S
		DROP TABLE #temp_CarSalesHeader_4
		DROP TABLE #temp_CarSalesHeader_3
		DROP TABLE #temp_CarSalesHeader_2
		DROP TABLE #temp_CarSalesHeader_1
		DROP TABLE #temp_ServiceSalesHeader_4
		DROP TABLE #temp_ServiceSalesHeader_3
		DROP TABLE #temp_ServiceSalesHeader_2
		DROP TABLE #temp_ServiceSalesHeader_1
		DROP TABLE #temp_CarMaster_VC
		DROP TABLE #temp_CustomerCar
		DROP TABLE #temp_Header
		DROP TABLE #temp_CodeName_013 
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

END



GO


