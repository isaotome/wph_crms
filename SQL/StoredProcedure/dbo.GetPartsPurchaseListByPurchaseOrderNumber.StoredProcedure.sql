USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsPurchaseListByPurchaseOrderNumber]    Script Date: 2017/11/07 16:35:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--2017/11/06 arc yano #3808 部品入荷入力 Webオーダー番号欄の追加
--2017/08/10 arc yano #3783 部品入荷入力 入荷取消・キャンセル機能　発注部品番号もDBに保存する
--2016/08/13 arc yano #3596 【大項目】部門棚統合対応 検索条件(部門コード)は不要のため、廃止
--2015/11/11 arc nakayama #3292_部品入荷一覧 新規作成　入荷一覧画面から入荷処理を行うための未入荷リスト取得

CREATE PROCEDURE [dbo].[GetPartsPurchaseListByPurchaseOrderNumber]

	@PurchaseOrderNumber nvarchar(50),	--発注伝票番号
	@DepartmentCode nvarchar(3) --部門コード
AS

BEGIN
--/*
	--一時表の削除
	IF OBJECT_ID(N'tempdb..#temp_PartsPurchaseOrder', N'U') IS NOT NULL
	DROP TABLE #temp_PartsPurchaseOrder;												--発注情報リスト
	IF OBJECT_ID(N'tempdb..#temp_PartsPurchase', N'U') IS NOT NULL
	DROP TABLE #temp_PartsPurchase;														--入荷情報リスト	
	IF OBJECT_ID(N'tempdb..#temp_PartsPurchase_ChangeParts', N'U') IS NOT NULL
	DROP TABLE #temp_PartsPurchase_ChangeParts;											--入荷情報リスト(代替品)
	IF OBJECT_ID(N'tempdb..#temp_PartsLocation', N'U') IS NOT NULL
	DROP TABLE #temp_PartsLocation;														--部品ロケーションリスト

	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; --ダーティーリード設定

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	/*-------------------------------------------*/
	/* 部品ロケーションテーブル					 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_PartsLocation (
	     PartsNumber nvarchar(25) NOT NULL			--部品番号
	   , LocationCode nvarchar(12)					--ロケーションコード
	   , WarehouseCode nvarchar(6) NOT NULL			--倉庫コード
		)

	INSERT INTO #temp_PartsLocation
	SELECT
		 pl.PartsNumber								--部品番号
		,pl.LocationCode							--ロケーションコード
		,pl.WarehouseCode							--倉庫コード
	FROM
		dbo.PartsLocation pl
	WHERE
		EXISTS
		(
			SELECT 'X' FROM dbo.DepartmentWarehouse dw WHERE dw.DepartmentCode = @DepartmentCode AND dw.DelFlag = '0' AND pl.WarehouseCode = dw.WarehouseCode
		)
	CREATE INDEX ix_temp_PartsLocation ON #temp_PartsLocation(PartsNumber, WarehouseCode)


	/*-------------------------------------------*/
	/* 発注情報取得（PartsPurchaseOrder）		 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_PartsPurchaseOrder (
	     PurchaseOrderNumber nvarchar(50)	--発注伝票番号
	   , ServiceSlipNumber nvarchar(50)	    --受注伝票番号
	   , PartsNumber nvarchar(25)			--部品番号
	   , WebOrderNumber nvarchar(50)		--WEBオーダー番号
	   , SupplierCode nvarchar(10)			--仕入先コード
	   , RemainingQuantity decimal(10,2)	--発注残
		)

		SET @PARAM = '@PurchaseOrderNumber nvarchar(50)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchaseOrder' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PPO.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.WebOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PPO.RemainingQuantity' + @CRLF
		SET @SQL = @SQL + 'FROM dbo.PartsPurchaseOrder AS PPO' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '      PPO.DelFlag = ''0''' + @CRLF

		--発注伝票番号
		IF ((@PurchaseOrderNumber IS NOT NULL) AND (@PurchaseOrderNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PPO.PurchaseOrderNumber = @PurchaseOrderNumber'+ @CRLF
		END

		EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseOrderNumber
		CREATE INDEX ix_temp_PartsPurchaseOrder ON #temp_PartsPurchaseOrder(PurchaseOrderNumber)


	/*-------------------------------------------*/
	/* 入荷情報取得 （PartsPurchaseOrder）		 */
	/*-------------------------------------------*/


	CREATE TABLE #temp_PartsPurchase (
		 PurchaseNumber nvarchar(50)		--入荷伝票番号
	   , PurchaseOrderNumber nvarchar(50)	--発注伝票番号
	   , PartsNumber nvarchar(25)			--部品番号
	   , Quantity decimal(10,2)				--入荷予定数
	   , Price decimal(10,0)				--入荷単価
	   , Amount decimal(10,0)				--入荷金額
	   , SupplierCode nvarchar(10)			--仕入先コード
	   , PurchaseType nvarchar(3)			--入荷伝票区分
	   , MakerOrderNumber nvarchar(50)		--メーカーオーダー番号
	   , WebOrderNumber nvarchar(50)		--Webオーダー番号
	   , InvoiceNo nvarchar(50)				--インボイス番号
	   , ReceiptNumber nvarchar(50)			--納品書番号
	   , Memo nvarchar(100)					--メモ
	   , ChangePartsFlag nvarchar(2)		--代替部品フラグ
	   , OrderPartsNumber nvarchar(25)		--発注部品番号			--Add 2017/08/10 arc yano #3783            
		)

		SET @PARAM = ''
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchase' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '  PP.PurchaseNumber' + @CRLF
	    SET @SQL = @SQL + ', PP.PurchaseOrderNumber' + @CRLF
	    SET @SQL = @SQL + ', PP.PartsNumber' + @CRLF
	    SET @SQL = @SQL + ', PP.Quantity' + @CRLF
	    SET @SQL = @SQL + ', PP.Price' + @CRLF
	    SET @SQL = @SQL + ', PP.Amount' + @CRLF
	    SET @SQL = @SQL + ', PP.SupplierCode' + @CRLF
		SET @SQL = @SQL + ', PP.PurchaseType' + @CRLF
	    SET @SQL = @SQL + ', PP.MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ', PP.WebOrderNumber' + @CRLF		--Add 2017/11/06 arc yano #3808
	    SET @SQL = @SQL + ', PP.InvoiceNo' + @CRLF
		SET @SQL = @SQL + ', PP.ReceiptNumber' + @CRLF
		SET @SQL = @SQL + ', PP.Memo' + @CRLF
		SET @SQL = @SQL + ', PP.ChangePartsFlag' + @CRLF
		SET @SQL = @SQL + ', PP.OrderPartsNumber' + @CRLF					--Add 2017/08/10 arc yano #3783
		SET @SQL = @SQL + ' FROM dbo.PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + ' WHERE PP.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM
		CREATE INDEX ix_temp_PartsPurchase ON #temp_PartsPurchase(PurchaseOrderNumber, PartsNumber)


	/*-------------------------------------------------*/
	/* 発注と違う部品が入荷する場合の入荷情報取得	   */
	/*-------------------------------------------------*/

		CREATE TABLE #temp_PartsPurchase_ChangeParts (
		 PurchaseNumber nvarchar(50)		--入荷伝票番号
	   , PurchaseOrderNumber nvarchar(50)	--発注伝票番号
	   , PartsNumber nvarchar(25)			--部品番号
	   , Quantity decimal(10,2)				--入荷予定数
	   , Price decimal(10,0)				--入荷単価
	   , Amount decimal(10,0)				--入荷金額
	   , SupplierCode nvarchar(10)			--仕入先コード
	   , PurchaseType nvarchar(3)			--入荷伝票区分
	   , MakerOrderNumber nvarchar(50)		--メーカーオーダー番号
	   , WebOrderNumber nvarchar(50)		--Webオーダー番号		--Add 2017/11/06 arc yano #3808
	   , InvoiceNo nvarchar(50)				--インボイス番号
	   , ReceiptNumber nvarchar(50)			--納品書番号
	   , Memo nvarchar(100)					--メモ
	   , ChangePartsFlag nvarchar(2)		--代替部品フラグ
	   , OrderPartsNumber nvarchar(25)		--発注部品番号			--Add 2017/08/10 arc yano #3783
		)

		SET @PARAM = '@PurchaseOrderNumber nvarchar(50)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchase_ChangeParts' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PP.Quantity' + @CRLF
		SET @SQL = @SQL + ',PP.Price' + @CRLF
		SET @SQL = @SQL + ',PP.Amount' + @CRLF
		SET @SQL = @SQL + ',PP.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseType' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ', PP.WebOrderNumber' + @CRLF		--Add 2017/11/06 arc yano #3808
		SET @SQL = @SQL + ',PP.InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',PP.ReceiptNumber' + @CRLF
		SET @SQL = @SQL + ',PP.Memo' + @CRLF
		SET @SQL = @SQL + ',PP.ChangePartsFlag' + @CRLF
		SET @SQL = @SQL + ', PP.OrderPartsNumber' + @CRLF					--Add 2017/08/10 arc yano #3783
		
		SET @SQL = @SQL + 'FROM #temp_PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + 'WHERE NOT EXISTS(SELECT 1 FROM #temp_PartsPurchaseOrder AS PPO WHERE PPO.PartsNumber = PP.PartsNumber)' + @CRLF

		--発注伝票番号
		IF ((@PurchaseOrderNumber IS NOT NULL) AND (@PurchaseOrderNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.PurchaseOrderNumber = @PurchaseOrderNumber'+ @CRLF
		END


		EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseOrderNumber
		CREATE INDEX ix_temp_PartsPurchase_ChangeParts ON #temp_PartsPurchase_ChangeParts(PurchaseOrderNumber)

	/*-------------------------------------------*/
	/* 未入荷の部品入荷情報取得					 */
	/*-------------------------------------------*/

		SET @PARAM = ''
		SET @SQL = ''
		
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',PPO.PartsNumber AS PartsNumber' + @CRLF
		SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',PL.LocationCode AS LocationCode' + @CRLF
		SET @SQL = @SQL + ',L.LocationName AS LocationName' + @CRLF
		SET @SQL = @SQL + ',PPO.RemainingQuantity AS RemainingQuantity' + @CRLF			
		SET @SQL = @SQL + ',PP.Quantity AS PurchaseQuantity' + @CRLF
		SET @SQL = @SQL + ',PP.Price AS Price' + @CRLF
		SET @SQL = @SQL + ',PP.Amount AS Amount' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',CASE WHEN (PP.WebOrderNumber IS NULL OR PP.WebOrderNumber = '''') THEN PPO.WebOrderNumber ELSE PP.WebOrderNumber END AS WebOrderNumber' + @CRLF	--Add 2017/11/06 arc yano #3808
		SET @SQL = @SQL + ',PPO.SupplierCode AS SupplierCode' + @CRLF
		SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
		SET @SQL = @SQL + ',PP.ReceiptNumber AS ReceiptNumber'+ @CRLF
		SET @SQL = @SQL + ',PP.PurchaseType AS PurchaseType' + @CRLF
		SET @SQL = @SQL + ',PP.Memo' + @CRLF
		SET @SQL = @SQL + ',PP.ChangePartsFlag AS ChangePartsFlag' + @CRLF
		SET @SQL = @SQL + ',PP.OrderPartsNumber AS OrderPartsNumber' + @CRLF					--Add 2017/08/10 arc yano #3783

		SET @SQL = @SQL + 'FROM #temp_PartsPurchaseOrder AS PPO' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchase AS PP ON PP.PurchaseOrderNumber = PPO.PurchaseOrderNumber AND PP.PartsNumber = PPO.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PPO.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PPO.SupplierCode'+ @CRLF
		--SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.PartsLocation AS PL ON PL.PartsNumber = PPO.PartsNumber AND PL.DepartmentCode = @DepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsLocation AS PL ON PL.PartsNumber = PPO.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Location AS L ON PL.LocationCode = L.LocationCode'+ @CRLF

		SET @SQL = @SQL +'UNION' + @CRLF
		
		--発注時と、別の部品が入荷予定になっている未入荷データ
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
		SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',PL.LocationCode AS LocationCode' + @CRLF
		SET @SQL = @SQL + ',L.LocationName AS LocationName' + @CRLF
		SET @SQL = @SQL + ',null AS RemainingQuantity' + @CRLF
		SET @SQL = @SQL + ',PP.Quantity AS PurchaseQuantity' + @CRLF
		SET @SQL = @SQL + ',PP.Price AS Price' + @CRLF
		SET @SQL = @SQL + ',PP.Amount AS Amount' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',CASE WHEN (PP.WebOrderNumber IS NULL OR PP.WebOrderNumber = '''') THEN PPO.WebOrderNumber ELSE PP.WebOrderNumber END AS WebOrderNumber' + @CRLF	--Add 2017/11/06 arc yano #3808
		SET @SQL = @SQL + ',PP.SupplierCode AS SupplierCode' + @CRLF
		SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
		SET @SQL = @SQL + ',PP.ReceiptNumber AS ReceiptNumber'+ @CRLF
		SET @SQL = @SQL + ',PP.PurchaseType AS PurchaseType' + @CRLF
		SET @SQL = @SQL + ',PP.Memo' + @CRLF
		SET @SQL = @SQL + ',PP.ChangePartsFlag AS ChangePartsFlag' + @CRLF
		SET @SQL = @SQL + ',PP.OrderPartsNumber AS OrderPartsNumber' + @CRLF					--Add 2017/08/10 arc yano #3783

		SET @SQL = @SQL + 'FROM #temp_PartsPurchase_ChangeParts AS PP' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchaseOrder AS PPO ON PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
		--SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.PartsLocation AS PL ON PL.PartsNumber = PP.PartsNumber AND PL.DepartmentCode = @DepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsLocation AS PL ON PL.PartsNumber = PP.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Location AS L ON PL.LocationCode = L.LocationCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF

		SET @SQL = @SQL +'ORDER BY PPO.PurchaseOrderNumber desc' + @CRLF
		
		EXECUTE sp_executeSQL @SQL, @PARAM--, @DepartmentCode
--*/
/*
			SELECT
			 CONVERT(nvarchar(50),'') AS PurchaseNumber
			,CONVERT(nvarchar(50),'') AS PurchaseOrderNumber
			,CONVERT(nvarchar(50),'') AS SlipNumber
			,CONVERT(nvarchar(50),'') AS InvoiceNo
			,CONVERT(nvarchar(25),'') AS PartsNumber
			,CONVERT(nvarchar(50),'') AS PartsNameJp
			,CONVERT(nvarchar(12),'') AS LocationCode
			,CONVERT(nvarchar(50),'') AS LocationName
			,CONVERT(decimal(10, 2), null) AS RemainingQuantity
			,CONVERT(decimal(10, 2), null) AS PurchaseQuantity
			,CONVERT(decimal(10, 0), null) AS Price
			,CONVERT(decimal(10, 0), null) AS Amount
			,CONVERT(nvarchar(50),'') AS MakerOrderNumber
			,CONVERT(nvarchar(50),'') AS WebOrderNumber
			,CONVERT(nvarchar(10),'') AS SupplierCode
			,CONVERT(nvarchar(80),'') AS SupplierName
			,CONVERT(nvarchar(50),'') AS ReceiptNumber
			,CONVERT(nvarchar(3),'') AS PurchaseType
			,CONVERT(nvarchar(100),'') AS Memo
			,CONVERT(nvarchar(2),'') AS ChangePartsFlag
			,CONVERT(nvarchar(25),'') AS OrderPartsNumber
*/
END




GO


