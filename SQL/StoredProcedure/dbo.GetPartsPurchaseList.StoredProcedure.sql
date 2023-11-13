USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsPurchaseList]    Script Date: 2017/11/07 16:35:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





--2017/11/06 arc yano #3808 部品入荷 Webオーダー番号の追加
--2017/08/10 arc yano #3783 部品入荷入力 入荷取消・キャンセル機能　発注部品番号もDBに保存する
--2016/08/13 arc yano #3596 【大項目】部門棚統合対応 検索条件(部門コード)は不要のため、廃止
--2016/06/20 arc yano #3585 部品入荷一覧　引数追加(PurchaseStatus) 
--2015/11/11 arc nakayama #3292_部品入荷一覧 新規作成　入荷一覧画面から入荷処理を行うための未入荷リスト取得


CREATE PROCEDURE [dbo].[GetPartsPurchaseList]

	@PurchaseNumber nvarchar(50),		            --入荷伝票番号
	@PurchaseOrderNumber nvarchar(50),				--発注伝票番号
	@PartsNumber nvarchar(25),						--部品番号 
	@DepartmentCode nvarchar(3),					--部門コード
	@PurchaseStatus nvarchar(3)						--入荷ステータス

AS

BEGIN

--/*	
	--一時表の削除
	IF OBJECT_ID(N'tempdb..#temp_PartsPurchaseOrder', N'U') IS NOT NULL
	DROP TABLE #temp_PartsPurchaseOrder;												--発注情報リスト
	IF OBJECT_ID(N'tempdb..#temp_PartsPurchase', N'U') IS NOT NULL
	DROP TABLE #temp_PartsPurchase;														--入荷情報リスト	
	IF OBJECT_ID(N'tempdb..#temp_PartsLocation', N'U') IS NOT NULL
	DROP TABLE #temp_PartsLocation;														--部品ロケーションリスト
	

	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; --ダーティーリード設定

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @ROWCNT INT = 0			--行数


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

		--発注伝票番号がある場合のみデータ取得　※発注伝票番号があるということは、部品番号もある前提
		IF (
				((@PurchaseOrderNumber IS NOT NULL) AND (@PurchaseOrderNumber <>''))
				AND 
				((@PartsNumber IS NOT NULL) AND (@PartsNumber <>''))
			)
		BEGIN
			SET @PARAM = '@PurchaseOrderNumber nvarchar(50), @PartsNumber nvarchar(25)'
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
			SET @SQL = @SQL + 'AND PPO.PurchaseOrderNumber = @PurchaseOrderNumber'+ @CRLF
			SET @SQL = @SQL + 'AND PPO.PartsNumber = @PartsNumber'+ @CRLF

			EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseOrderNumber, @PartsNumber

			SELECT @ROWCNT = COUNT(ppo.PurchaseOrderNumber) FROM #temp_PartsPurchaseOrder ppo

			IF (@ROWCNT = 0)
			BEGIN
				SET @PARAM = '@PurchaseOrderNumber nvarchar(50)'
				SET @SQL = ''
				SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchaseOrder' + @CRLF
				SET @SQL = @SQL + 'SELECT TOP 1' + @CRLF
				SET @SQL = @SQL + ' PPO.PurchaseOrderNumber' + @CRLF
				SET @SQL = @SQL + ',PPO.ServiceSlipNumber' + @CRLF
				SET @SQL = @SQL + ',NULL AS PartsNumber' + @CRLF
				SET @SQL = @SQL + ',PPO.WebOrderNumber' + @CRLF
				SET @SQL = @SQL + ',PPO.SupplierCode' + @CRLF
				SET @SQL = @SQL + ',PPO.RemainingQuantity' + @CRLF
				SET @SQL = @SQL + 'FROM dbo.PartsPurchaseOrder AS PPO' + @CRLF
				SET @SQL = @SQL + ' WHERE' + @CRLF
				SET @SQL = @SQL + '      PPO.DelFlag = ''0''' + @CRLF
				SET @SQL = @SQL + 'AND PPO.PurchaseOrderNumber = @PurchaseOrderNumber'+ @CRLF

				EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseOrderNumber
				CREATE INDEX ix_temp_PartsPurchaseOrder ON #temp_PartsPurchaseOrder(PurchaseOrderNumber)
			END

		END

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
	   , WebOrderNumber nvarchar(50)		--Webオーダー番号		--Add 2017/11/06 arc yano #3808
	   , InvoiceNo nvarchar(50)				--インボイス番号
	   , ReceiptNumber nvarchar(50)			--納品書番号
	   , Memo nvarchar(100)					--メモ
	   , ChangePartsFlag nvarchar(2)		--代替部品フラグ
	   , LocationCode nvarchar(12)			--ロケーションコード	--Add 2016/06/28
	   , OrderPartsNumber nvarchar(25)		--発注部品番号			--Add 2017/08/10 arc yano #3783
		)

		--入荷伝票番号がある場合のみデータ取得
		IF ((@PurchaseNumber IS NOT NULL) AND (@PurchaseNumber <>''))
		BEGIN
			SET @PARAM = '@PurchaseNumber nvarchar(50)'
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
			SET @SQL = @SQL + ', PP.WebOrderNumber' + @CRLF			--Add 2017/11/06 arc yano #3808
			SET @SQL = @SQL + ', PP.InvoiceNo' + @CRLF
			SET @SQL = @SQL + ', PP.ReceiptNumber' + @CRLF
			SET @SQL = @SQL + ', PP.Memo' + @CRLF
			SET @SQL = @SQL + ', PP.ChangePartsFlag' + @CRLF
			SET @SQL = @SQL + ', PP.LocationCode' + @CRLF						--Add 2016/06/28 
			SET @SQL = @SQL + ', PP.OrderPartsNumber' + @CRLF					--Add 2017/08/10 arc yano #3783
			SET @SQL = @SQL + ' FROM dbo.PartsPurchase AS PP' + @CRLF
			SET @SQL = @SQL + ' WHERE PP.DelFlag = ''0''' + @CRLF
			--SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF
			SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''' + @PurchaseStatus + '''' + @CRLF		--入荷ステータスは引数で貰う
			SET @SQL = @SQL + 'AND PP.PurchaseNumber = @PurchaseNumber'+ @CRLF

			EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseNumber
			CREATE INDEX ix_temp_PartsPurchase ON #temp_PartsPurchase(PurchaseOrderNumber, PartsNumber)
		END

	/*-------------------------------------------*/
	/* 未入荷の部品入荷情報取得					 */
	/*-------------------------------------------*/

		SET @PARAM = ''
		SET @SQL = ''

		--チェックした項目の編集の場合(発注伝票番号)
		--入荷伝票番号がキーだった場合
		IF((@PurchaseNumber IS NOT NULL) AND (@PurchaseNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
			SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
			SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
			SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
			SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
			--Mod 2016/06/28 arc yano
			IF(@PurchaseStatus = '001')
			BEGIN
				SET @SQL = @SQL + ',PL.LocationCode AS LocationCode' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL + ',PP.LocationCode AS LocationCode' + @CRLF		--Add 2016/06/28 arc yano
			END
			SET @SQL = @SQL + ',L.LocationName AS LocationName' + @CRLF
			SET @SQL = @SQL + ',PPO.RemainingQuantity AS RemainingQuantity' + @CRLF
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
			SET @SQL = @SQL + ',PP.OrderPartsNumber AS OrderPartsNumber' + @CRLF --Add 2017/08/10 arc yano #3783

			SET @SQL = @SQL + 'FROM #temp_PartsPurchase AS PP' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchaseOrder AS PPO ON PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber AND PPO.PartsNumber = PP.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
			
			--Mod 2016/06/28 arc yano
			IF(@PurchaseStatus = '001')
			BEGIN
				--SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.PartsLocation AS PL ON PL.PartsNumber = PP.PartsNumber AND PL.DepartmentCode = @DepartmentCode'+ @CRLF
				SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsLocation AS PL ON PL.PartsNumber = PP.PartsNumber'+ @CRLF
				SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Location AS L ON PL.LocationCode = L.LocationCode'+ @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Location AS L ON PP.LocationCode = L.LocationCode'+ @CRLF
			END
			SET @SQL = @SQL + 'WHERE ISNULL(PP.ChangePartsFlag, ''0'') <> ''1'''+ @CRLF

			--SET @SQL = @SQL +'ORDER BY PP.PurchaseOrderNumber desc' + @CRLF

			SET @SQL = @SQL +'UNION' + @CRLF
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
			SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
			SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
			SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
			SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
			
			--Mod 2016/06/28 arc yano
			IF(@PurchaseStatus = '001')
			BEGIN
				SET @SQL = @SQL + ',PL.LocationCode AS LocationCode' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL + ',PP.LocationCode AS LocationCode' + @CRLF		
			END
			
			SET @SQL = @SQL + ',L.LocationName AS LocationName' + @CRLF
			SET @SQL = @SQL + ',NULL AS RemainingQuantity' + @CRLF
			SET @SQL = @SQL + ',PP.Quantity AS PurchaseQuantity' + @CRLF
			SET @SQL = @SQL + ',PP.Price AS Price' + @CRLF
			SET @SQL = @SQL + ',PP.Amount AS Amount' + @CRLF
			SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PP.WebOrderNumber AS WebOrderNumber' + @CRLF		--Add 2017/11/06 arc yano #3808
			SET @SQL = @SQL + ',PP.SupplierCode AS SupplierCode' + @CRLF
			SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
			SET @SQL = @SQL + ',PP.ReceiptNumber AS ReceiptNumber'+ @CRLF
			SET @SQL = @SQL + ',PP.PurchaseType AS PurchaseType' + @CRLF
			SET @SQL = @SQL + ',PP.Memo' + @CRLF
			SET @SQL = @SQL + ',PP.ChangePartsFlag AS ChangePartsFlag' + @CRLF
			SET @SQL = @SQL + ',PP.OrderPartsNumber AS OrderPartsNumber' + @CRLF --Add 2017/08/10 arc yano #3783

			SET @SQL = @SQL + 'FROM #temp_PartsPurchase AS PP' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchaseOrder AS PPO ON PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
			--Mod 2016/06/28 arc yano
			IF(@PurchaseStatus = '001')
			BEGIN
				--Mod 2016/08/13 arc yano
				--SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.PartsLocation AS PL ON PL.PartsNumber = PP.PartsNumber AND PL.DepartmentCode = @DepartmentCode'+ @CRLF
				SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsLocation AS PL ON PL.PartsNumber = PP.PartsNumber'+ @CRLF
				SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Location AS L ON PL.LocationCode = L.LocationCode'+ @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Location AS L ON PP.LocationCode = L.LocationCode'+ @CRLF
			END
			SET @SQL = @SQL + 'WHERE ISNULL(PP.ChangePartsFlag, ''0'') = ''1'''+ @CRLF

			SET @SQL = @SQL +'ORDER BY PP.PurchaseOrderNumber desc' + @CRLF
		END
		--発注伝票番号がキーだった場合
		ELSE
		BEGIN
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
			SET @SQL = @SQL + ',PP.OrderPartsNumber AS OrderPartsNumber' + @CRLF --Add 2017/08/10 arc yano #3783


			SET @SQL = @SQL + 'FROM #temp_PartsPurchaseOrder AS PPO' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchase AS PP ON PP.PurchaseOrderNumber = PPO.PurchaseOrderNumber AND PP.PartsNumber = PPO.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PPO.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PPO.SupplierCode'+ @CRLF
			--Mod 2016/08/13 arc yano #3596
			--SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.PartsLocation AS PL ON PL.PartsNumber = PPO.PartsNumber AND PL.DepartmentCode = @DepartmentCode'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsLocation AS PL ON PL.PartsNumber = PPO.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Location AS L ON PL.LocationCode = L.LocationCode'+ @CRLF
		END
		--PRINT @SQL
		EXECUTE sp_executeSQL @SQL, @PARAM
--*/
		/*	
		--テーブル構成
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


