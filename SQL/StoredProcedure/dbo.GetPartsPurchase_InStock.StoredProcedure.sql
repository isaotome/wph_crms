USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsPurchase_InStock]    Script Date: 2018/04/02 11:47:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--2018/03/26 arc yano #3863 部品入荷　LinkEntry取込日の追加
--2017/11/06 arc yano #3808 部品入荷 Webオーダー番号欄の追加
--2015/11/11 arc nakayama #3292_部品入荷一覧 新規作成　入荷一覧画面で入荷ステータスを「入荷済み」で検索した時の検索
--2016/04/22 arc nkayama #3500_部品入荷一覧　入荷済みの検索結果を入荷日の降順に並び替える


CREATE PROCEDURE [dbo].[GetPartsPurchase_InStock]

	@PurchaseNumberFrom nvarchar(50),		--入荷伝票番号From
	@PurchaseNumberTo nvarchar(50),			--入荷伝票番号To
	@PurchaseOrderNumberFrom nvarchar(50),	--発注伝票番号From
	@PurchaseOrderNumberTo nvarchar(50),	--発注伝票番号To
	@PurchaseOrderDateFrom nvarchar(10),	--発注日From
	@PurchaseOrderDateTo nvarchar(10),		--発注日To
	@SlipNumberFrom nvarchar(50),			--受注伝票番号From
	@SlipNumberTo nvarchar(50),				--受注伝票番号To
	@PurchaseType nvarchar(3),				--入荷伝票区分
	@OrderType nvarchar(3),					--発注区分
	@CustomerCode nvarchar(10),				--顧客コード
	@PartsNumber nvarchar(25),				--部品番号
	@PurchasePlanDateFrom nvarchar(10),		--入荷予定日From
	@PurchasePlanDateTo nvarchar(10),		--入荷予定日To
	@PurchaseDateFrom nvarchar(10),			--入荷日From
	@PurchaseDateTo nvarchar(10),			--入荷日To
	@DepartmentCode nvarchar(3),			--部門コード
	@EmployeeCode nvarchar(50),				--社員コード(サービスフロント)
	@SupplierCode nvarchar(10),				--仕入先コード
	@WebOrderNumber nvarchar(50),			--WEBオーダー番号
	@MakerOrderNumber nvarchar(50),			--メーカーオーダー番号
	@InvoiceNo nvarchar(50),				--インボイス番号
	@LinkEntryCaptureDateFrom nvarchar(10),	--取込日From	--Add 2018/03/26 arc yano #3863
	@LinkEntryCaptureDateTo nvarchar(10)	--取込日To		--Add 2018/03/26 arc yano #3863

AS

BEGIN
--/*
	BEGIN TRY
		--tempテーブル削除
		DROP TABLE #temp_ServiceSalesHeader
		DROP TABLE #temp_PartsPurchaseOrder
		DROP TABLE #temp_PartsPurchase
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; --ダーティーリード設定

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	/*-------------------------------------------*/
	/* サービス伝票の情報取得					 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_ServiceSalesHeader (
		 SlipNumber nvarchar(50)
	   , FrontEmployeeCode nvarchar(50)
	   , CustomerCode nvarchar(10)

		)

		SET @PARAM = '@SlipNumberFrom nvarchar(50), @SlipNumberTo nvarchar(50), @EmployeeCode nvarchar(50), @CustomerCode nvarchar(10)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_ServiceSalesHeader' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '	 h.SlipNumber' + @CRLF
		SET @SQL = @SQL + ', h.FrontEmployeeCode' + @CRLF
		SET @SQL = @SQL + ', h.CustomerCode' + @CRLF

		SET @SQL = @SQL + 'FROM dbo.ServiceSalesHeader h' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '      h.DelFlag = ''0''' + @CRLF
				
		--伝票番号
		IF (@SlipNumberFrom is not null) AND (@SlipNumberFrom <> '')
			IF (@SlipNumberTo is not null) AND (@SlipNumberTo <> '')
			BEGIN
				SET @SQL = @SQL +'AND h.SlipNumber >= @SlipNumberFrom AND h.SlipNumber < @SlipNumberTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND h.SlipNumber = @SlipNumberFrom' + @CRLF 
			END
		ELSE
			IF ((@SlipNumberTo is not null) AND (@SlipNumberTo <> '') AND ISDATE(@SlipNumberTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND h.SlipNumber < @SlipNumberTo' + @CRLF 
			END		

		--フロント担当者
		IF ((@EmployeeCode IS NOT NULL) AND (@EmployeeCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND h.FrontEmployeeCode = @EmployeeCode'+ @CRLF
		END

		--顧客コード
		IF ((@CustomerCode IS NOT NULL) AND (@CustomerCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND h.CustomerCode = @CustomerCode'+ @CRLF
		END

		EXECUTE sp_executeSQL @SQL, @PARAM, @SlipNumberFrom, @SlipNumberTo, @EmployeeCode, @CustomerCode
		CREATE INDEX ix_temp_ServiceSalesHeader ON #temp_ServiceSalesHeader(SlipNumber)

	/*-------------------------------------------*/
	/* 発注情報取得（PartsPurchaseOrder）		 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_PartsPurchaseOrder (
	     PurchaseOrderNumber nvarchar(50)	--発注伝票番号
	   , PurchaseOrderDate datetime			--発注日
	   , ServiceSlipNumber nvarchar(50)		--サービス伝票番号
	   , PartsNumber nvarchar(25)			--部品番号
	   , OrderType nvarchar(3)				--発注区分
	   , WebOrderNumber nvarchar(50)		--WEBオーダー番号
	   , Quantity decimal(10,2)	--発注残
		)

		SET @PARAM = '@PurchaseOrderNumberFrom nvarchar(50), @PurchaseOrderNumberTo nvarchar(50), @PurchaseOrderDateFrom nvarchar(10), @PurchaseOrderDateTo nvarchar(10), @SlipNumberFrom nvarchar(50), @SlipNumberTo nvarchar(50), @OrderType nvarchar(3), @DepartmentCode nvarchar(3), @WebOrderNumber nvarchar(50), @SupplierCode nvarchar(10), @CustomerCode nvarchar(10), @EmployeeCode nvarchar(50), @PartsNumber nvarchar(25)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchaseOrder' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PPO.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PurchaseOrderDate' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.OrderType' + @CRLF
		SET @SQL = @SQL + ',PPO.WebOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.Quantity' + @CRLF
		SET @SQL = @SQL + 'FROM dbo.PartsPurchaseOrder AS PPO' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '      PPO.DelFlag = ''0''' + @CRLF

		--発注伝票番号
		IF (@PurchaseOrderNumberFrom is not null) AND (@PurchaseOrderNumberFrom <> '')
			IF (@PurchaseOrderNumberTo is not null) AND (@PurchaseOrderNumberTo <> '')
			BEGIN
				SET @SQL = @SQL +'AND PPO.PurchaseOrderNumber >= @PurchaseOrderNumberFrom AND PPO.PurchaseOrderNumber <= @PurchaseOrderNumberTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PPO.PurchaseOrderNumber = @PurchaseOrderNumberFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchaseOrderNumberTo is not null) AND (@PurchaseOrderNumberTo <> ''))
			BEGIN
				SET @SQL = @SQL +'AND PPO.PurchaseOrderNumber <= @PurchaseOrderNumberTo' + @CRLF 
			END

		--発注日
		IF ((@PurchaseOrderDateFrom is not null) AND (@PurchaseOrderDateFrom <> '') AND ISDATE(@PurchaseOrderDateFrom) = 1)
			IF ((@PurchaseOrderDateTo is not null) AND (@PurchaseOrderDateTo <> '') AND ISDATE(@PurchaseOrderDateTo) = 1)
			BEGIN
				SET @PurchaseOrderDateTo = DateAdd(d, 1, @PurchaseOrderDateTo)
				SET @SQL = @SQL +'AND PPO.PurchaseOrderDate >= @PurchaseOrderDateFrom AND PPO.PurchaseOrderDate < @PurchaseOrderDateTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PPO.PurchaseOrderDate = @PurchaseOrderDateFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchaseOrderDateTo is not null) AND (@PurchaseOrderDateTo <> '') AND ISDATE(@PurchaseOrderDateTo) = 1)
			BEGIN
				SET @PurchaseOrderDateTo = DateAdd(d, 1, @PurchaseOrderDateTo)
				SET @SQL = @SQL +'AND PPO.PurchaseOrderDate < @PurchaseOrderDateTo' + @CRLF 
			END

		--部品番号
		IF ((@PartsNumber IS NOT NULL) AND (@PartsNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PPO.PartsNumber = @PartsNumber'+ @CRLF
		END

		--発注区分
		IF ((@OrderType IS NOT NULL) AND (@OrderType <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PPO.OrderType = @OrderType'+ @CRLF
		END

		--WEBオーダー番号
		IF ((@WebOrderNumber IS NOT NULL) AND (@WebOrderNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PPO.WebOrderNumber = @WebOrderNumber'+ @CRLF
		END

		--受注伝票番号
		IF (@SlipNumberFrom is not null) AND (@SlipNumberFrom <> '')
			IF (@SlipNumberTo is not null) AND (@SlipNumberTo <> '')
			BEGIN
				SET @SQL = @SQL +'AND PPO.ServiceSlipNumber >= @SlipNumberFrom AND PPO.ServiceSlipNumber <= @SlipNumberTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PPO.ServiceSlipNumber = @SlipNumberFrom' + @CRLF 
			END
		ELSE
			IF ((@SlipNumberTo is not null) AND (@SlipNumberTo <> ''))
			BEGIN
				SET @SQL = @SQL +'AND PPO.ServiceSlipNumber <= @SlipNumberTo' + @CRLF 
			END

		--サービス伝票に関する検索条件が[受注伝票番号]以外にあった場合、その検索条件に一致する発注情報のみを取得する
		--フロント担当者
		IF ((@EmployeeCode IS NOT NULL) AND (@EmployeeCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND EXISTS(SELECT 1 FROM #temp_ServiceSalesHeader h WHERE h.FrontEmployeeCode = @EmployeeCode AND h.SlipNumber = PPO.ServiceSlipNumber)'+ @CRLF
		END

		--顧客コード
		IF ((@CustomerCode IS NOT NULL) AND (@CustomerCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND EXISTS(SELECT 1 FROM #temp_ServiceSalesHeader h WHERE h.CustomerCode = @CustomerCode AND h.SlipNumber = PPO.ServiceSlipNumber)'+ @CRLF
		END


		EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseOrderNumberFrom, @PurchaseOrderNumberTo, @PurchaseOrderDateFrom, @PurchaseOrderDateTo, @SlipNumberFrom, @SlipNumberTo, @OrderType, @DepartmentCode, @WebOrderNumber, @SupplierCode, @CustomerCode, @EmployeeCode, @PartsNumber
		CREATE INDEX ix_temp_PartsPurchaseOrder ON #temp_PartsPurchaseOrder(PurchaseOrderNumber, PartsNumber, ServiceSlipNumber)


	/*-------------------------------------------*/
	/* 入荷情報取得								 */
	/*-------------------------------------------*/


	CREATE TABLE #temp_PartsPurchase (
		 PurchaseNumber nvarchar(50)		--入荷伝票番号
	   , PurchaseOrderNumber nvarchar(50)	--発注伝票番号
	   , PurchaseType nvarchar(3)			--入荷伝票区分
	   , Quantity decimal(10,2)				--入荷数
	   , PurchasePlanDate datetime			--入荷予定日
	   , PurchaseDate datetime				--入荷日
	   , MakerOrderNumber nvarchar(50)		--メーカーオーダー番号
	   , WebOrderNumber nvarchar(50)		--Webオーダー番号			--Add 2017/11/06 arc yano #3808
	   , InvoiceNo nvarchar(50)				--インボイス番号
	   , PartsNumber nvarchar(25)			--部品番号
	   , DepartmentCode nvarchar(3)			--部門コード
	   , SupplierCode nvarchar(10)			--仕入先コード
	   , LinkEntryCaptureDate datetime		--取込日 --Add 2018/03/26 arc yano #3863
		)

		SET @PARAM = '@PurchaseNumberFrom nvarchar(50), @PurchaseNumberTo nvarchar(50), @PurchaseOrderNumberFrom nvarchar(50), @PurchaseOrderNumberTo nvarchar(50), @PurchasePlanDateFrom nvarchar(10), @PurchasePlanDateTo nvarchar(10), @InvoiceNo nvarchar(50), @PurchaseType nvarchar(3), @SlipNumberFrom nvarchar(50), @SlipNumberTo nvarchar(50), @PartsNumber nvarchar(25), @PurchaseDateFrom nvarchar(10), @PurchaseDateTo nvarchar(10), @DepartmentCode nvarchar(3), @SupplierCode nvarchar(10), @MakerOrderNumber nvarchar(50), @WebOrderNumber nvarchar(50)'	--Mod 2017/11/06 arc yano #3808
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchase' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PurchaseType' + @CRLF
		SET @SQL = @SQL + ',PP.Quantity' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseDate' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.WebOrderNumber' + @CRLF			--Add 2017/11/06 arc yano #3808
		SET @SQL = @SQL + ',PP.InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PP.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',PP.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863	
		SET @SQL = @SQL + 'FROM dbo.PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + ' WHERE PP.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '   AND PurchaseStatus = ''002''' + @CRLF --入荷済みのみ取得

		--入荷伝票番号
		IF (@PurchaseNumberFrom is not null) AND (@PurchaseNumberFrom <> '')
			IF (@PurchaseNumberTo is not null) AND (@PurchaseNumberTo <> '')
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseNumber >= @PurchaseNumberFrom AND PP.PurchaseNumber <= @PurchaseNumberTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseNumber = @PurchaseNumberFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchaseNumberTo is not null) AND (@PurchaseNumberTo <> ''))
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseNumber <= @PurchaseNumberTo' + @CRLF 
			END

		--発注伝票番号
		IF (@PurchaseOrderNumberFrom is not null) AND (@PurchaseOrderNumberFrom <> '')
			IF (@PurchaseOrderNumberTo is not null) AND (@PurchaseOrderNumberTo <> '')
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseOrderNumber >= @PurchaseOrderNumberFrom AND PP.PurchaseOrderNumber <= @PurchaseOrderNumberTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseOrderNumber = @PurchaseOrderNumberFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchaseOrderNumberTo is not null) AND (@PurchaseOrderNumberTo <> ''))
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseOrderNumber <= @PurchaseOrderNumberTo' + @CRLF 
			END

		--入荷予定日
		IF ((@PurchasePlanDateFrom is not null) AND (@PurchasePlanDateFrom <> '') AND ISDATE(@PurchasePlanDateFrom) = 1)
			IF ((@PurchasePlanDateTo is not null) AND (@PurchasePlanDateTo <> '') AND ISDATE(@PurchasePlanDateTo) = 1)
			BEGIN
				SET @PurchasePlanDateTo = DateAdd(d, 1, @PurchasePlanDateTo)
				SET @SQL = @SQL +'AND PP.PurchasePlanDate >= @PurchasePlanDateFrom AND PP.PurchasePlanDate < @PurchasePlanDateTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchasePlanDate = @PurchasePlanDateFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchasePlanDateTo is not null) AND (@PurchasePlanDateTo <> '') AND ISDATE(@PurchasePlanDateTo) = 1)
			BEGIN
				SET @PurchasePlanDateTo = DateAdd(d, 1, @PurchasePlanDateTo)
				SET @SQL = @SQL +'AND PP.PurchasePlanDate < @PurchasePlanDateTo' + @CRLF 
			END
		
		--入荷日
		IF ((@PurchaseDateFrom is not null) AND (@PurchaseDateFrom <> '') AND ISDATE(@PurchaseDateFrom) = 1)
			IF ((@PurchaseDateTo is not null) AND (@PurchaseDateTo <> '') AND ISDATE(@PurchaseDateTo) = 1)
			BEGIN
				SET @PurchaseDateTo = DateAdd(d, 1, @PurchaseDateTo)
				SET @SQL = @SQL +'AND PP.PurchaseDate >= @PurchaseDateFrom AND PP.PurchaseDate < @PurchaseDateTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseDate = @PurchaseDateFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchaseDateTo is not null) AND (@PurchaseDateTo <> '') AND ISDATE(@PurchaseDateTo) = 1)
			BEGIN
				SET @PurchaseDateTo = DateAdd(d, 1, @PurchaseDateTo)
				SET @SQL = @SQL +'AND PP.PurchaseDate < @PurchaseDateTo' + @CRLF 
			END

		--メーカーオーダー番号
		IF ((@MakerOrderNumber IS NOT NULL) AND (@MakerOrderNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.MakerOrderNumber = @MakerOrderNumber'+ @CRLF
		END

		--Add 2017/11/06 arc yano #3808
		--Webオーダー番号
		IF ((@WebOrderNumber IS NOT NULL) AND (@WebOrderNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.WebOrderNumber = @WebOrderNumber'+ @CRLF
		END		
			
		--インボイス番号
		IF ((@InvoiceNo IS NOT NULL) AND (@InvoiceNo <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.InvoiceNo = @InvoiceNo'+ @CRLF
		END

		--部品番号
		IF ((@PartsNumber IS NOT NULL) AND (@PartsNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.PartsNumber = @PartsNumber'+ @CRLF
		END
		
		--部門
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.DepartmentCode = @DepartmentCode'+ @CRLF
		END

		--仕入先
		IF ((@SupplierCode IS NOT NULL) AND (@SupplierCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.SupplierCode = @SupplierCode'+ @CRLF
		END

		--仕入伝票区分
		IF ((@PurchaseType IS NOT NULL) AND (@PurchaseType <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.PurchaseType = @PurchaseType'+ @CRLF
		END


		EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseNumberFrom, @PurchaseNumberTo, @PurchaseOrderNumberFrom, @PurchaseOrderNumberTo, @PurchasePlanDateFrom, @PurchasePlanDateTo, @InvoiceNo, @PurchaseType, @SlipNumberFrom, @SlipNumberTo, @PartsNumber, @PurchaseDateFrom, @PurchaseDateTo, @DepartmentCode, @SupplierCode, @MakerOrderNumber, @WebOrderNumber --Add 2017/11/06 arc yano #3808
		CREATE INDEX ix_temp_PartsPurchase ON #temp_PartsPurchase(PurchaseOrderNumber, PartsNumber)

	/*-------------------------------------------*/
	/* 未入荷の部品入荷情報取得					 */
	/*-------------------------------------------*/

		SET @PARAM = ''
		SET @SQL = ''

		--[発注日]・[発注区分]・[WEBオーダーナンバー]が検索条件になかったら発注データがないデータを含む入荷情報を表示する
		IF ((@PurchaseOrderDateFrom is null) OR (@PurchaseOrderDateFrom = '')) AND ((@PurchaseOrderDateTo is null) OR (@PurchaseOrderDateTo = ''))
		AND ((@OrderType is null) OR (@OrderType = ''))
		AND ((@WebOrderNumber is null) OR (@WebOrderNumber = ''))
		AND ((@SlipNumberFrom is null) OR (@SlipNumberFrom = '')) AND ((@SlipNumberTo is null) OR (@SlipNumberTo = ''))
		AND ((@CustomerCode is null) OR (@CustomerCode = ''))
		AND ((@EmployeeCode is null) OR (@EmployeeCode = '')) 
		BEGIN 

		--発注データがないデータを含む入荷データ
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PurchaseOrderDate AS PurchaseOrderDate' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL + ',CPT.Name AS PurchaseTypeName' + @CRLF
		SET @SQL = @SQL + ',C.CustomerName AS CustomerName' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
		SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',PPO.Quantity AS PurchaseOrderQuantity' + @CRLF	
		SET @SQL = @SQL + ',PP.Quantity AS PurchaseQuantity' + @CRLF
		SET @SQL = @SQL + ',COT.Name AS OrderTypeName' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseDate AS PurchaseDate' + @CRLF
		SET @SQL = @SQL + ',CASE WHEN (PP.WebOrderNumber IS NULL OR PP.WebOrderNumber = '''') THEN PPO.WebOrderNumber ELSE PP.WebOrderNumber END AS WebOrderNumber' + @CRLF	--Add 2017/11/06 arc yano #3808
		SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
		SET @SQL = @SQL + ',E.EmployeeName AS EmployeeName' + @CRLF
		SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate AS LinkEntryCaptureDate' + @CRLF		--Add 2018/03/26 arc yano #3863

		SET @SQL = @SQL + 'FROM #temp_PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchaseOrder AS PPO ON PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber AND PP.PartsNumber = PPO.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_ServiceSalesHeader AS H ON H.SlipNumber = PPO.ServiceSlipNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Customer AS C ON C.CustomerCode = H.CustomerCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_PurchaseType AS CPT ON CPT.Code = PP.PurchaseType'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_OrderType AS COT ON COT.Code = PPO.OrderType'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PP.DepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E ON E.EmployeeCode = H.FrontEmployeeCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
		--Add 2018/03/26 arc yano #3863
		--LinkEntry取込日
		SET @SQL = @SQL + 'WHERE' + @CRLF
		SET @SQL = @SQL +	'1 = 1' + @CRLF
		IF ((@LinkEntryCaptureDateFrom is not null) AND (@LinkEntryCaptureDateFrom <> '') AND ISDATE(@LinkEntryCaptureDateFrom) = 1)
			IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
			BEGIN
				SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate >= CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''') AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate = CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''')' + @CRLF 
			END
		ELSE
			IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
			BEGIN
				SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')'  + @CRLF 
			END
				
		END
		ELSE
		BEGIN

		--発注データがある入荷データ
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PurchaseOrderDate AS PurchaseOrderDate' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL + ',CPT.Name AS PurchaseTypeName' + @CRLF
		SET @SQL = @SQL + ',C.CustomerName AS CustomerName' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
		SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',PPO.Quantity AS PurchaseOrderQuantity' + @CRLF	
		SET @SQL = @SQL + ',PP.Quantity AS PurchaseQuantity' + @CRLF
		SET @SQL = @SQL + ',COT.Name AS OrderTypeName' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseDate AS PurchaseDate' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',CASE WHEN (PP.WebOrderNumber IS NULL OR PP.WebOrderNumber = '''') THEN PPO.WebOrderNumber ELSE PP.WebOrderNumber END AS WebOrderNumber' + @CRLF	--Add 2017/11/06 arc yano #3808
		SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
		SET @SQL = @SQL + ',E.EmployeeName AS EmployeeName' + @CRLF
		SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate AS LinkEntryCaptureDate' + @CRLF		--Add 2018/03/26 arc yano #3863

		SET @SQL = @SQL + 'FROM #temp_PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + 'INNER JOIN #temp_PartsPurchaseOrder AS PPO ON PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber AND PP.PartsNumber = PPO.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_ServiceSalesHeader AS H ON H.SlipNumber = PPO.ServiceSlipNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Customer AS C ON C.CustomerCode = H.CustomerCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_PurchaseType AS CPT ON CPT.Code = PP.PurchaseType'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_OrderType AS COT ON COT.Code = PPO.OrderType'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PP.DepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E ON E.EmployeeCode = H.FrontEmployeeCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
		--Add 2018/03/26 arc yano #3863
		--LinkEntry取込日
		SET @SQL = @SQL + 'WHERE' + @CRLF
		SET @SQL = @SQL +	'1 = 1' + @CRLF
		IF ((@LinkEntryCaptureDateFrom is not null) AND (@LinkEntryCaptureDateFrom <> '') AND ISDATE(@LinkEntryCaptureDateFrom) = 1)
			IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
			BEGIN
				SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate >= CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''') AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate = CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''')' + @CRLF 
			END
		ELSE
			IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
			BEGIN
				SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')'  + @CRLF 
			END

		END

		SET @SQL = @SQL +'ORDER BY PP.PurchaseDate desc' + @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM
--*/

		---- !!!!戻り値の型のGetPurchase_Resultはdbmlに定義されている
	


END





GO


