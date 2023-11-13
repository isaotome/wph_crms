USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsPurchase_NotInStock]    Script Date: 2018/04/02 11:39:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




--2018/03/26 arc yano #3863 部品入荷　LinkEntry取込日の追加
--2015/11/11 arc nakayama #3292_部品入荷一覧 新規作成　入荷一覧画面で入荷ステータスを「未入荷」で検索した時の検索


CREATE PROCEDURE [dbo].[GetPartsPurchase_NotInStock]

	@PurchaseNumberFrom nvarchar(50),		--入荷伝票番号From
	@PurchaseNumberTo nvarchar(50),			--入荷伝票番号To
	@PurchaseOrderNumberFrom nvarchar(50),	--発注伝票番号From
	@PurchaseOrderNumberTo nvarchar(50),	--発注伝票番号To
	@PurchaseOrderDateFrom nvarchar(10),	--発注日From
	@PurchaseOrderDateTo nvarchar(10),		--発注日To
	@SlipNumberFrom nvarchar(50),			--受注伝票番号From
	@SlipNumberTo nvarchar(50),				--受注伝票番号To
	@OrderType nvarchar(3),					--発注区分
	@CustomerCode nvarchar(10),				--顧客コード
	@PartsNumber nvarchar(25),				--部品番号
	@PurchasePlanDateFrom nvarchar(10),		--入荷予定日From
	@PurchasePlanDateTo nvarchar(10),		--入荷予定日To
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
	BEGIN TRY
		--tempテーブル削除
		DROP TABLE #temp_ServiceSalesHeader
		DROP TABLE #temp_PartsPurchaseOrder
		DROP TABLE #temp_PartsPurchase
		DROP TABLE #temp_PartsPurchase_NotOrder
		DROP TABLE #temp_RemainingQuantity
		DROP TABLE #temp_StockPartsPurchase
		DROP TABLE #temp_NotStockPartsPurchase
		DROP TABLE #temp_PartsPurchase_ChangeParts
		DROP TABLE #temp_PartsPurchase_NotExistsOrder
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; --ダーティーリード設定

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)


	--取込日to
	IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
	BEGIN
		SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
	END
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
				SET @SQL = @SQL +'AND h.SlipNumber >= @SlipNumberFrom AND h.SlipNumber <= @SlipNumberTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND h.SlipNumber = @SlipNumberFrom' + @CRLF 
			END
		ELSE
			IF ((@SlipNumberTo is not null) AND (@SlipNumberTo <> ''))
			BEGIN
				SET @SQL = @SQL +'AND h.SlipNumber <= @SlipNumberTo' + @CRLF 
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

	/*-------------------------------------------------------------------*/
	/* 発注情報取得（PartsPurchaseOrder）(発注残があるレコードのみ)		 */
	/*-------------------------------------------------------------------*/

	CREATE TABLE #temp_PartsPurchaseOrder (
	     PurchaseOrderNumber nvarchar(50)	--発注伝票番号
	   , PurchaseOrderDate datetime			--発注日
	   , ServiceSlipNumber nvarchar(50)		--サービス伝票番号
	   , PartsNumber nvarchar(25)			--部品番号
	   , Quantity decimal(10,2)				--発注数
	   , OrderType nvarchar(3)				--発注区分
	   , WebOrderNumber nvarchar(50)		--WEBオーダー番号
	   , DepartmentCode nvarchar(3)			--部門コード
	   , SupplierCode nvarchar(10)			--仕入先コード
	   , RemainingQuantity decimal(10,2)	--発注残
		)

		SET @PARAM = '@PurchaseOrderNumberFrom nvarchar(50), @PurchaseOrderNumberTo nvarchar(50), @PurchaseOrderDateFrom nvarchar(10), @PurchaseOrderDateTo nvarchar(10), @SlipNumberFrom nvarchar(50), @SlipNumberTo nvarchar(50), @OrderType nvarchar(3), @DepartmentCode nvarchar(3), @WebOrderNumber nvarchar(50), @SupplierCode nvarchar(10), @CustomerCode nvarchar(10), @EmployeeCode nvarchar(50), @PartsNumber nvarchar(25)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchaseOrder' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PPO.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PurchaseOrderDate' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.Quantity' + @CRLF
		SET @SQL = @SQL + ',PPO.OrderType' + @CRLF
		SET @SQL = @SQL + ',PPO.WebOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',PPO.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PPO.RemainingQuantity' + @CRLF
		SET @SQL = @SQL + 'FROM dbo.PartsPurchaseOrder AS PPO' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '       PPO.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '   AND PPO.RemainingQuantity > 0' + @CRLF

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

		--部門
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PPO.DepartmentCode = @DepartmentCode'+ @CRLF
		END

		--仕入先
		IF ((@SupplierCode IS NOT NULL) AND (@SupplierCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PPO.SupplierCode = @SupplierCode'+ @CRLF
		END		
		
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
	/* 入荷情報取得（発注伝票番号があるもの）	 */
	/*-------------------------------------------*/


	CREATE TABLE #temp_PartsPurchase (
		 PurchaseNumber nvarchar(50)		--入荷伝票番号
	   , PurchaseOrderNumber nvarchar(50)	--発注伝票番号
	   , Quantity decimal(10,2)				--入荷予定数
	   , PurchasePlanDate datetime			--入荷予定日
	   , MakerOrderNumber nvarchar(50)		--メーカーオーダー番号
	   , InvoiceNo nvarchar(50)				--インボイス番号
	   , PartsNumber nvarchar(25)			--部品番号
	   , DepartmentCode nvarchar(3)			--部門コード
	   , SupplierCode nvarchar(10)			--仕入先コード
	   , PurchaseStatus nvarchar(3)			--仕入ステータス
	   , ServiceSlipNumber nvarchar(50)		--受注伝票番号
	   , ChangePartsFlag nvarchar(2)		--代替部品フラグ
	   , LinkEntryCaptureDate datetime		--取込日 --Add 2018/03/26 arc yano #3863
		)

		SET @PARAM = '@PurchaseNumberFrom nvarchar(50), @PurchaseNumberTo nvarchar(50), @PurchaseOrderNumberFrom nvarchar(50), @PurchaseOrderNumberTo nvarchar(50), @PurchasePlanDateFrom nvarchar(10), @PurchasePlanDateTo nvarchar(10), @MakerOrderNumber nvarchar(50), @InvoiceNo nvarchar(50), @SlipNumberFrom nvarchar(50), @SlipNumberTo nvarchar(50), @SupplierCode nvarchar(10), @DepartmentCode nvarchar(3), @CustomerCode nvarchar(10), @EmployeeCode nvarchar(50), @PartsNumber nvarchar(25)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchase' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.Quantity' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PP.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',PP.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseStatus' + @CRLF
		SET @SQL = @SQL + ',PP.ServiceSlipNumber' + @CRLF
		SET @SQL = @SQL + ',PP.ChangePartsFlag' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863
		SET @SQL = @SQL + 'FROM dbo.PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + ' WHERE PP.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF
		SET @SQL = @SQL + '   AND (PP.PurchaseOrderNumber IS NOT NULL OR PP.PurchaseOrderNumber <> '''')' + @CRLF

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

		--メーカーオーダー番号
		IF ((@MakerOrderNumber IS NOT NULL) AND (@MakerOrderNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.MakerOrderNumber = @MakerOrderNumber'+ @CRLF
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

		EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseNumberFrom, @PurchaseNumberTo, @PurchaseOrderNumberFrom, @PurchaseOrderNumberTo, @PurchasePlanDateFrom, @PurchasePlanDateTo, @MakerOrderNumber, @InvoiceNo, @SlipNumberFrom, @SlipNumberTo, @SupplierCode, @DepartmentCode, @CustomerCode, @EmployeeCode, @PartsNumber
		CREATE INDEX ix_temp_PartsPurchase ON #temp_PartsPurchase(PurchaseOrderNumber, PartsNumber)



	/*-------------------------------------------------*/
	/* 発注と違う部品が入荷する場合の入荷情報取得	   */
	/*-------------------------------------------------*/

		CREATE TABLE #temp_PartsPurchase_ChangeParts (
		 PurchaseNumber nvarchar(50)		--入荷伝票番号
	   , PurchaseOrderNumber nvarchar(50)	--発注伝票番号
	   , Quantity decimal(10,2)				--入荷予定数
	   , PurchasePlanDate datetime			--入荷予定日
	   , MakerOrderNumber nvarchar(50)		--メーカーオーダー番号
	   , InvoiceNo nvarchar(50)				--インボイス番号
	   , PartsNumber nvarchar(25)			--部品番号
	   , DepartmentCode nvarchar(3)			--部門コード
	   , SupplierCode nvarchar(10)			--仕入先コード
	   , PurchaseStatus nvarchar(3)			--仕入ステータス
	   , ServiceSlipNumber nvarchar(50)		--受注伝票番号
	   , LinkEntryCaptureDate datetime		--取込日 --Add 2018/03/26 arc yano #3863
		)

		SET @PARAM = ''
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchase_ChangeParts' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.Quantity' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PP.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',PP.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseStatus' + @CRLF
		SET @SQL = @SQL + ',PP.ServiceSlipNumber' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863
		SET @SQL = @SQL + 'FROM #temp_PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + 'WHERE PP.ChangePartsFlag = ''1''' + @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM
		CREATE INDEX ix_temp_PartsPurchase_ChangeParts ON #temp_PartsPurchase_ChangeParts(PurchaseOrderNumber)


	/*-------------------------------------------*/
	/* 入荷情報取得（発注伝票番号がないもの）	 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_PartsPurchase_NotOrder (
		 PurchaseNumber nvarchar(50)		--入荷伝票番号
	   , PurchaseOrderNumber nvarchar(50)	--発注伝票番号
	   , Quantity decimal(10,2)				--入荷予定数
	   , PurchasePlanDate datetime			--入荷予定日
	   , MakerOrderNumber nvarchar(50)		--メーカーオーダー番号
	   , InvoiceNo nvarchar(50)				--インボイス番号
	   , PartsNumber nvarchar(25)			-- 部品番号
	   , DepartmentCode nvarchar(3)			--部門コード
	   , SupplierCode nvarchar(10)			--仕入先コード
	   , ServiceSlipNumber nvarchar(50)		--受注伝票番号
	   , LinkEntryCaptureDate datetime		--取込日 --Add 2018/03/26 arc yano #3863
		)

		SET @PARAM = '@PurchaseNumberFrom nvarchar(50), @PurchaseNumberTo nvarchar(50), @PurchaseOrderNumberFrom nvarchar(50), @PurchaseOrderNumberTo nvarchar(50), @PurchasePlanDateFrom nvarchar(10), @PurchasePlanDateTo nvarchar(10), @MakerOrderNumber nvarchar(50), @InvoiceNo nvarchar(50), @SlipNumberFrom nvarchar(50), @SlipNumberTo nvarchar(50), @SupplierCode nvarchar(10), @DepartmentCode nvarchar(3), @CustomerCode nvarchar(10), @EmployeeCode nvarchar(50), @PartsNumber nvarchar(25)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchase_NotOrder' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.Quantity' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PP.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',PP.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PP.ServiceSlipNumber' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate' + @CRLF		--Add 2018/03/26 arc yano #3863
		SET @SQL = @SQL + 'FROM dbo.PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + ' WHERE PP.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF
		SET @SQL = @SQL + '   AND (PP.PurchaseOrderNumber IS NULL OR PP.PurchaseOrderNumber = '''')' + @CRLF

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

		--メーカーオーダー番号
		IF ((@MakerOrderNumber IS NOT NULL) AND (@MakerOrderNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.MakerOrderNumber = @MakerOrderNumber'+ @CRLF
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

		EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseNumberFrom, @PurchaseNumberTo, @PurchaseOrderNumberFrom, @PurchaseOrderNumberTo, @PurchasePlanDateFrom, @PurchasePlanDateTo, @MakerOrderNumber, @InvoiceNo, @SlipNumberFrom, @SlipNumberTo, @SupplierCode, @DepartmentCode, @CustomerCode, @EmployeeCode, @PartsNumber
		CREATE INDEX ix_temp_PartsPurchase_NotOrder ON #temp_PartsPurchase_NotOrder(PurchaseOrderNumber, PartsNumber)


	--2016/04/25 arc yano #3502 発注伝票番号は存在するが、発注データが存在しない入荷データを取得するように修正
	/*-------------------------------------------------------------------*/
	/* 入荷情報取得（発注伝票番号はあるが発注データが存在しないもの）	 */
	/*-------------------------------------------------------------------*/
	CREATE TABLE #temp_PartsPurchase_NotExistsOrder (
		 PurchaseNumber nvarchar(50)		--入荷伝票番号
	   , PurchaseOrderNumber nvarchar(50)	--発注伝票番号
	   , Quantity decimal(10,2)				--入荷予定数
	   , PurchasePlanDate datetime			--入荷予定日
	   , MakerOrderNumber nvarchar(50)		--メーカーオーダー番号
	   , InvoiceNo nvarchar(50)				--インボイス番号
	   , PartsNumber nvarchar(25)			-- 部品番号
	   , DepartmentCode nvarchar(3)			--部門コード
	   , SupplierCode nvarchar(10)			--仕入先コード
	   , ServiceSlipNumber nvarchar(50)		--受注伝票番号
	   , LinkEntryCaptureDate datetime		--取込日 --Add 2018/03/26 arc yano #3863
		)

		SET @PARAM = '@PurchaseNumberFrom nvarchar(50), @PurchaseNumberTo nvarchar(50), @PurchaseOrderNumberFrom nvarchar(50), @PurchaseOrderNumberTo nvarchar(50), @PurchasePlanDateFrom nvarchar(10), @PurchasePlanDateTo nvarchar(10), @MakerOrderNumber nvarchar(50), @InvoiceNo nvarchar(50), @SlipNumberFrom nvarchar(50), @SlipNumberTo nvarchar(50), @SupplierCode nvarchar(10), @DepartmentCode nvarchar(3), @CustomerCode nvarchar(10), @EmployeeCode nvarchar(50), @PartsNumber nvarchar(25)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchase_NotOrder' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.Quantity' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PP.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',PP.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PP.ServiceSlipNumber' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863
		SET @SQL = @SQL + 'FROM dbo.PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + ' WHERE PP.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF
		SET @SQL = @SQL + '   AND (PP.PurchaseOrderNumber IS NOT NULL AND PP.PurchaseOrderNumber <> '''')' + @CRLF	--発注伝票番号がNULLまたは空文字でない
		SET @SQL = @SQL + '   AND NOT EXISTS (' + @CRLF
		SET @SQL = @SQL + '		SELECT ''x'' FROM dbo.PartsPurchaseOrder PPO WHERE PPO.DelFlag = ''0'' AND PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + '   )' + @CRLF

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

		--メーカーオーダー番号
		IF ((@MakerOrderNumber IS NOT NULL) AND (@MakerOrderNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.MakerOrderNumber = @MakerOrderNumber'+ @CRLF
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

		EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseNumberFrom, @PurchaseNumberTo, @PurchaseOrderNumberFrom, @PurchaseOrderNumberTo, @PurchasePlanDateFrom, @PurchasePlanDateTo, @MakerOrderNumber, @InvoiceNo, @SlipNumberFrom, @SlipNumberTo, @SupplierCode, @DepartmentCode, @CustomerCode, @EmployeeCode, @PartsNumber
		CREATE INDEX ix_PartsPurchase_NotExistsOrder ON #temp_PartsPurchase_NotExistsOrder(PurchaseOrderNumber, PartsNumber)
	
	/*-------------------------------------------*/
	/* 未入荷の部品入荷情報取得					 */
	/*-------------------------------------------*/

		SET @PARAM = ''
		SET @SQL = ''

		--発注データがあり、入荷予定数が分かっている未入荷データ
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PurchaseOrderDate AS PurchaseOrderDate' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL + ',C.CustomerName AS CustomerName' + @CRLF
		SET @SQL = @SQL + ',PPO.PartsNumber AS PartsNumber' + @CRLF
		SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',PPO.RemainingQuantity AS RemainingQuantity' + @CRLF	
		SET @SQL = @SQL + ',PP.Quantity AS PurchasePlanQuantity' + @CRLF
		SET @SQL = @SQL + ',COT.Name AS OrderTypeName' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PPO.WebOrderNumber AS WebOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
		SET @SQL = @SQL + ',E.EmployeeName AS EmployeeName' + @CRLF
		SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate AS LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863

		SET @SQL = @SQL + 'FROM #temp_PartsPurchaseOrder AS PPO' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_ServiceSalesHeader AS H ON H.SlipNumber = PPO.ServiceSlipNumber'+ @CRLF
		SET @SQL = @SQL + 'INNER JOIN #temp_PartsPurchase AS PP ON PP.PurchaseOrderNumber = PPO.PurchaseOrderNumber AND PP.PartsNumber = PPO.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Customer AS C ON C.CustomerCode = H.CustomerCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PPO.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_OrderType AS COT ON COT.Code = PPO.OrderType'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PPO.DepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E ON E.EmployeeCode = H.FrontEmployeeCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PPO.SupplierCode'+ @CRLF
		--Add 2018/03/26 arc yano #3863
		--LinkEntry取込日
		SET @SQL = @SQL + 'WHERE' + @CRLF
		SET @SQL = @SQL +	'1 = 1' + @CRLF
		IF ((@LinkEntryCaptureDateFrom is not null) AND (@LinkEntryCaptureDateFrom <> '') AND ISDATE(@LinkEntryCaptureDateFrom) = 1)
			IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
			BEGIN
				--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate >= CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''') AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate = CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''')' + @CRLF 
			END
		ELSE
			IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
			BEGIN
				--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')'  + @CRLF 
			END
				
		SET @SQL = @SQL +'UNION' + @CRLF
		
		--2016/04/25 arc yano 
		--発注時と、別の部品が入荷予定になっている未入荷データ
		SET @SQL = @SQL + 'SELECT DISTINCT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',NULL AS PurchaseOrderDate' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL + ',C.CustomerName AS CustomerName' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
		SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',NULL AS RemainingQuantity' + @CRLF	
		SET @SQL = @SQL + ',PP.Quantity AS PurchasePlanQuantity' + @CRLF
		SET @SQL = @SQL + ',COT.Name AS OrderTypeName' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ','''' AS WebOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
		SET @SQL = @SQL + ',E.EmployeeName AS EmployeeName' + @CRLF
		SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate AS LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863

		SET @SQL = @SQL + 'FROM #temp_PartsPurchase_ChangeParts AS PP' + @CRLF
		SET @SQL = @SQL + 'INNER JOIN #temp_PartsPurchaseOrder AS PPO ON PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_ServiceSalesHeader AS H ON H.SlipNumber = PPO.ServiceSlipNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Customer AS C ON C.CustomerCode = H.CustomerCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_OrderType AS COT ON COT.Code = PPO.OrderType'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PP.DepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E ON E.EmployeeCode = H.FrontEmployeeCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
		
		SET @SQL = @SQL + ' WHERE PP.PurchaseStatus = ''001''' + @CRLF
		SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF
		--Add 2018/03/26 arc yano #3863
		--LinkEntry取込日
		IF ((@LinkEntryCaptureDateFrom is not null) AND (@LinkEntryCaptureDateFrom <> '') AND ISDATE(@LinkEntryCaptureDateFrom) = 1)
			IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
			BEGIN
				--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate >= CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''') AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate = CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''')' + @CRLF 
			END
		ELSE
			IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
			BEGIN
				--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')'  + @CRLF 
			END
		
		/*
		--発注時と、別の部品が入荷予定になっている未入荷データ
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PurchaseOrderDate AS PurchaseOrderDate' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL + ',C.CustomerName AS CustomerName' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
		SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',NULL AS RemainingQuantity' + @CRLF	
		SET @SQL = @SQL + ',PP.Quantity AS PurchasePlanQuantity' + @CRLF
		SET @SQL = @SQL + ',COT.Name AS OrderTypeName' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PPO.WebOrderNumber AS WebOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
		SET @SQL = @SQL + ',E.EmployeeName AS EmployeeName' + @CRLF
		SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF		

		SET @SQL = @SQL + 'FROM #temp_PartsPurchase_ChangeParts AS PP' + @CRLF
		SET @SQL = @SQL + 'INNER JOIN #temp_PartsPurchaseOrder AS PPO ON PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_ServiceSalesHeader AS H ON H.SlipNumber = PPO.ServiceSlipNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Customer AS C ON C.CustomerCode = H.CustomerCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_OrderType AS COT ON COT.Code = PPO.OrderType'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PP.DepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E ON E.EmployeeCode = H.FrontEmployeeCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
		
		SET @SQL = @SQL + ' WHERE PP.PurchaseStatus = ''001''' + @CRLF
		SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF		
		*/

		----仕入伝票番号が検索条件になかったら発注があり入荷予定がない未入荷データを表示する
		IF((@PurchaseNumberFrom is null) OR (@PurchaseNumberFrom = '')) AND ((@PurchaseNumberTo is null) OR (@PurchaseNumberTo = ''))
		AND ((@InvoiceNo is null) OR (@InvoiceNo = ''))
		AND ((@MakerOrderNumber is null) OR (@MakerOrderNumber = ''))
		BEGIN

			SET @SQL = @SQL +'UNION' + @CRLF

			--発注データがあり、入荷予定が分かっていない未入荷データ
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' '''' AS PurchaseNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.PurchaseOrderDate AS PurchaseOrderDate' + @CRLF
			SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
			SET @SQL = @SQL + ',C.CustomerName AS CustomerName' + @CRLF
			SET @SQL = @SQL + ',PPO.PartsNumber AS PartsNumber' + @CRLF
			SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
			SET @SQL = @SQL + ',PPO.RemainingQuantity AS RemainingQuantity' + @CRLF
			SET @SQL = @SQL + ',NULL AS PurchasePlanQuantity' + @CRLF
			SET @SQL = @SQL + ',COT.Name AS OrderTypeName' + @CRLF
			SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
			SET @SQL = @SQL + ',PPO.WebOrderNumber AS WebOrderNumber' + @CRLF
			SET @SQL = @SQL + ','''' AS MakerOrderNumber' + @CRLF
			SET @SQL = @SQL + ','''' AS InvoiceNo' + @CRLF
			SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
			SET @SQL = @SQL + ',E.EmployeeName AS EmployeeName' + @CRLF
			SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
			SET @SQL = @SQL + ',PP.LinkEntryCaptureDate AS LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863

			SET @SQL = @SQL + 'FROM #temp_PartsPurchaseOrder AS PPO' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_ServiceSalesHeader AS H ON H.SlipNumber = PPO.ServiceSlipNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchase AS PP ON PP.PurchaseOrderNumber = PPO.PurchaseOrderNumber AND PP.PartsNumber = PPO.PartsNumber'+ @CRLF
			--SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_RemainingQuantity AS RQ ON RQ.PurchaseOrderNumber = PPO.PurchaseOrderNumber AND RQ.PartsNumber = PPO.PartsNumber'+ @CRLF		
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Customer AS C ON C.CustomerCode = H.CustomerCode'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PPO.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_OrderType AS COT ON COT.Code = PPO.OrderType'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PP.DepartmentCode'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E ON E.EmployeeCode = H.FrontEmployeeCode'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF				
			SET @SQL = @SQL + '   WHERE (PP.PurchaseNumber IS NULL OR PP.PurchaseNumber = '''')' + @CRLF
			--Add 2018/03/26 arc yano #3863
			--LinkEntry取込日
			IF ((@LinkEntryCaptureDateFrom is not null) AND (@LinkEntryCaptureDateFrom <> '') AND ISDATE(@LinkEntryCaptureDateFrom) = 1)
				IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
				BEGIN
					--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate >= CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''') AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')' + @CRLF
				END
				ELSE
				BEGIN
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate = CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''')' + @CRLF 
				END
			ELSE
				IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
				BEGIN
					--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')'  + @CRLF 
				END
			
			--SET @SQL = @SQL + '   WHERE RQ.PurchaseStatus = ''001''' + @CRLF
			--SET @SQL = @SQL + '     AND RQ.RemainingQuantity2 IS NOT NULL' + @CRLF

		END

		--|-------------------発注に関するデータ------------------------------------|
		--[発注伝票番号]・[受注伝票番号]・[発注日]・[発注区分]・[WEBオーダーナンバー]が検索条件になかったら発注のない入荷情報を表示する
		IF ((@PurchaseOrderNumberFrom is null) OR (@PurchaseOrderNumberFrom = '')) AND ((@PurchaseOrderNumberTo is null) OR (@PurchaseOrderNumberTo = ''))
		AND ((@PurchaseOrderDateFrom is null) OR (@PurchaseOrderDateFrom = '')) AND ((@PurchaseOrderDateTo is null) OR (@PurchaseOrderDateTo = ''))
		AND ((@OrderType is null) OR (@OrderType = ''))
		AND ((@WebOrderNumber is null) OR (@WebOrderNumber = ''))
		AND ((@SlipNumberFrom is null) OR (@SlipNumberFrom = '')) AND ((@SlipNumberTo is null) OR (@SlipNumberTo = ''))
		AND ((@CustomerCode is null) OR (@CustomerCode = ''))
		AND ((@EmployeeCode is null) OR (@EmployeeCode = ''))

		BEGIN
			SET @SQL = @SQL +'UNION' + @CRLF

			--発注のない入荷データ
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
			SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ',NULL AS PurchaseOrderDate' + @CRLF
			SET @SQL = @SQL + ','''' AS SlipNumber' + @CRLF
			SET @SQL = @SQL + ','''' AS CustomerName' + @CRLF
			SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
			SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
			SET @SQL = @SQL + ',NULL AS RemainingQuantity' + @CRLF
			SET @SQL = @SQL + ',PP.Quantity AS PurchasePlanQuantity' + @CRLF
			SET @SQL = @SQL + ','''' AS OrderTypeName' + @CRLF
			SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
			SET @SQL = @SQL + ','''' AS WebOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
			SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
			SET @SQL = @SQL + ','''' AS EmployeeName' + @CRLF
			SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
			SET @SQL = @SQL + ',PP.LinkEntryCaptureDate AS LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863

			SET @SQL = @SQL + 'FROM #temp_PartsPurchase_NotOrder AS PP' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PP.DepartmentCode'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
			--Add 2018/03/26 arc yano #3863
			--LinkEntry取込日
			SET @SQL = @SQL + 'WHERE' + @CRLF
			SET @SQL = @SQL +	'1 = 1' + @CRLF
			IF ((@LinkEntryCaptureDateFrom is not null) AND (@LinkEntryCaptureDateFrom <> '') AND ISDATE(@LinkEntryCaptureDateFrom) = 1)
				IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
				BEGIN
					--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate >= CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''') AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')' + @CRLF
				END
				ELSE
				BEGIN
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate = CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''')' + @CRLF 
				END
			ELSE
				IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
				BEGIN
					--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')'  + @CRLF 
				END
			SET @SQL = @SQL +'UNION' + @CRLF

			--Add 2016/04/25 arc yano 
			--2016/04/25 arc yano #3502 発注伝票番号は存在するが、発注データが存在しない入荷データを取得するように修正
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
			SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ',NULL AS PurchaseOrderDate' + @CRLF
			SET @SQL = @SQL + ','''' AS SlipNumber' + @CRLF
			SET @SQL = @SQL + ','''' AS CustomerName' + @CRLF
			SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
			SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
			SET @SQL = @SQL + ',NULL AS RemainingQuantity' + @CRLF
			SET @SQL = @SQL + ',PP.Quantity AS PurchasePlanQuantity' + @CRLF
			SET @SQL = @SQL + ','''' AS OrderTypeName' + @CRLF
			SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
			SET @SQL = @SQL + ','''' AS WebOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
			SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
			SET @SQL = @SQL + ','''' AS EmployeeName' + @CRLF
			SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
			SET @SQL = @SQL + ',PP.LinkEntryCaptureDate AS LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863

			SET @SQL = @SQL + 'FROM #temp_PartsPurchase_NotExistsOrder AS PP' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PP.DepartmentCode'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
			--Add 2018/03/26 arc yano #3863
			--LinkEntry取込日
			SET @SQL = @SQL + 'WHERE' + @CRLF
			SET @SQL = @SQL +	'1 = 1' + @CRLF
			IF ((@LinkEntryCaptureDateFrom is not null) AND (@LinkEntryCaptureDateFrom <> '') AND ISDATE(@LinkEntryCaptureDateFrom) = 1)
				IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
				BEGIN
					--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate >= CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''') AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')' + @CRLF
				END
				ELSE
				BEGIN
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate = CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''')' + @CRLF 
				END
			ELSE
				IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
				BEGIN
					--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')'  + @CRLF 
				END			
		END

		SET @SQL = @SQL +'ORDER BY PPO.PurchaseOrderNumber, PPO.PartsNumber, PP.PurchaseNumber desc' + @CRLF

		--debug
		--print @SQL

		EXECUTE sp_executeSQL @SQL, @PARAM	--Mod 2018/03/26 arc yano #3863
--*/

-- !!!!戻り値の型のGetPurchase_Resultはdbmlに定義されている

END




GO


