USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[InsertInventoryStockCar]    Script Date: 2018/01/31 15:11:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
-- Update date: <Update Date,,>
-- 2018/01/30 arc yano #3841 棚卸開始時　棚卸リストで特定の車両が表示されない 棚卸中に移動中の車両は移動元のロケーションの車両とする
-- 2017/12/18 arc yano #3840 
-- Description:	<Description,,>
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[InsertInventoryStockCar] 
	   @InventoryMonth datetime						--棚卸月
	  ,@WarehouseCode nvarchar(6)					--倉庫コード
	  ,@EmployeeCode nvarchar(50)					--担当者コード
AS 
BEGIN

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

	--対象年月From
	DECLARE @TargetDateFrom datetime = @InventoryMonth
	--対象年月To
	DECLARE @TargetDateTo datetime = DATEADD(m, 1, @TargetDateFrom)

	
	DECLARE @LocationCode NVARCHAR(12)			--ロケーションコード

	--■一時表の削除
	/*************************************************************************/

	IF OBJECT_ID(N'tempdb..#Temp_DepartmentList', N'U') IS NOT NULL
	DROP TABLE #Temp_DepartmentList;
	IF OBJECT_ID(N'tempdb..#Temp_PurchaseCarList', N'U') IS NOT NULL
	DROP TABLE #Temp_PurchaseCarList;
	IF OBJECT_ID(N'tempdb..#Temp_CarSalesList', N'U') IS NOT NULL
	DROP TABLE #Temp_CarSalesList;
	IF OBJECT_ID(N'tempdb..#Temp_CarSalesList_CarUsage', N'U') IS NOT NULL
	DROP TABLE #Temp_CarSalesList_CarUsage;
	IF OBJECT_ID(N'tempdb..#Temp_TransferArrival', N'U') IS NOT NULL
	DROP TABLE #Temp_TransferArrival;
	IF OBJECT_ID(N'tempdb..#Temp_TransferDeparture', N'U') IS NOT NULL
	DROP TABLE #Temp_TransferDeparture;
	IF OBJECT_ID(N'tempdb..#Temp_InventoryStockCar', N'U') IS NOT NULL
	DROP TABLE #Temp_InventoryStockCar;
	IF OBJECT_ID(N'tempdb..#Temp_ExceptTransferDeparture', N'U') IS NOT NULL		--Add 2018/01/30 arc yano #3841
	DROP TABLE #Temp_ExceptTransferDeparture;

	/*************************************************************************/
	

	/*-------------------------------------------*/
	/* ■■一時表の宣言							 */
	/*-------------------------------------------*/
	--部門リスト
	CREATE TABLE #Temp_DepartmentList (
		[DepartmentCode] NVARCHAR(3) NOT NULL		--部門コード
	)

	--仕入リスト
	CREATE TABLE #Temp_PurchaseCarList (
		 [DepartmentCode] NVARCHAR(3) NOT NULL		--部門コード
		,[SalesCarNumber] NVARCHAR(50) NOT NULL		--管理番号
	)

	--納車リスト
	CREATE TABLE #Temp_CarSalesList (
		 [DepartmentCode] NVARCHAR(3) NOT NULL		--部門コード
		,[SalesCarNumber] NVARCHAR(50) NOT NULL		--管理番号
	)

	--納車リスト
	CREATE TABLE #Temp_CarSalesList_CarUsage (
		 [DepartmentCode] NVARCHAR(3) NOT NULL		--部門コード
		,[SalesCarNumber] NVARCHAR(50) NOT NULL		--管理番号
	)

	--移動受入リスト
	CREATE TABLE #Temp_TransferArrival (
		 [LocationCode]   NVARCHAR(12) NOT NULL		--ロケーションコード
		,[SalesCarNumber] NVARCHAR(50) NOT NULL		--管理番号
	)

	--移動払出リスト
	CREATE TABLE #Temp_TransferDeparture (
		 [LocationCode]   NVARCHAR(12) NOT NULL		--ロケーションコード
		,[SalesCarNumber] NVARCHAR(50) NOT NULL		--管理番号
	)

	--車両棚卸リスト
	CREATE TABLE #Temp_InventoryStockCar (
		 [WarehouseCode]  NVARCHAR(6) NOT NULL		--倉庫コード
		,[LocationCode]   NVARCHAR(12) NOT NULL		--倉庫コード
		,[SalesCarNumber] NVARCHAR(50) NOT NULL		--管理番号
		,[Vin]			  NVARCHAR(20) NOT NULL		--車台番号
		,[CarStatus]	  NVARCHAR(3)				--在庫区分
		,[NewUsedType]    NVARCHAR(3) NOT NULL		--新中区分
	)

	--Add 2018/01/30 arc yano #3841
	--移動払出除外リスト
	CREATE TABLE #Temp_ExceptTransferDeparture (
		 [LocationCode]   NVARCHAR(12) NOT NULL		--ロケーションコード
		,[SalesCarNumber] NVARCHAR(50) NOT NULL		--管理番号
	)

	/*-------------------------------------------*/
	/* データ取得								 */
	/*-------------------------------------------*/

	--トランザクション開始 
	BEGIN TRANSACTION
	BEGIN TRY
		
		--部門リストの挿入
		INSERT INTO #Temp_DepartmentList
		SELECT
			DepartmentCode
		FROM
			dbo.DepartmentWarehouse
		WHERE
			WarehouseCode = @WarehouseCode AND
			DelFlag = '0'

		CREATE UNIQUE INDEX IDX_Temp_DepartmentList ON #Temp_DepartmentList(DepartmentCode)

		-- ---------------------------------
		-- 棚卸月末以降に仕入された車両 
		-- ---------------------------------
		INSERT INTO #Temp_PurchaseCarList
		SELECT
			  cp.DepartmentCode								--部門コード
			 ,cp.SalesCarNumber								--管理番号
		FROM
			dbo.CarPurchase cp
		WHERE
			cp.PurchaseStatus = '002' AND					--仕入ステータス=仕入済
			cp.DelFlag = '0' AND
			cp.PurchaseDate >= @TargetDateTo
		GROUP BY --※本来同一管理番号の仕入済データは複数存在しないはずだが、存在するため、GROUP BYを使う
			  cp.DepartmentCode								--部門コード
			 ,cp.SalesCarNumber								--管理番号
			
		CREATE UNIQUE INDEX IDX_Temp_PurchaseCarList ON #Temp_PurchaseCarList(SalesCarNumber)

		-- ----------------------------------
		-- 棚卸月末以降に販売された車両
		-- ----------------------------------
		INSERT INTO #Temp_CarSalesList
		SELECT
			  cs.DepartmentCode AS DepartmentCode			--部門コード
			 ,cs.SalesCarNumber AS SalesCarNumber			--管理番号
		FROM
			dbo.CarSalesHeader cs
		WHERE
			cs.SalesOrderStatus = '005' AND					--伝票ステータス=納車済
			cs.DelFlag = '0' AND
			cs.SalesDate >= @TargetDateTo and
			EXISTS
			(
				select 'x' from #Temp_DepartmentList dl where cs.DepartmentCode = dl.DepartmentCode
			)
			AND cs.SlipNumber NOT LIKE '%-1%'
		GROUP BY
			 cs.DepartmentCode
			,cs.SalesCarNumber

		CREATE UNIQUE INDEX IDX_Temp_CarSalesList ON #Temp_CarSalesList(SalesCarNumber)

		-- ----------------------------------------------------
		-- 棚卸月末以降に販売された車両(デモカー、レンタカー用)
		-- ----------------------------------------------------
		INSERT INTO #Temp_CarSalesList_CarUsage
		SELECT
			  cs.DepartmentCode AS DepartmentCode			--部門コード
			 ,cs.SalesCarNumber AS SalesCarNumber			--管理番号
		FROM
			dbo.CarSalesHeader cs
		WHERE
			cs.SalesOrderStatus = '005' AND					--伝票ステータス=納車済
			cs.DelFlag = '0' AND
			cs.SalesDate >= @TargetDateTo and
			cs.DepartmentCode = '021' AND 					--FiatGroup課
			cs.SlipNumber NOT LIKE '%-1%'
		GROUP BY
			 cs.DepartmentCode
			,cs.SalesCarNumber

		CREATE UNIQUE INDEX IDX_Temp_CarSalesList_CarUsage ON #Temp_CarSalesList_CarUsage(SalesCarNumber)

		-- ----------------------------------
		-- 棚卸月末以降に移動受入された車両
		-- ----------------------------------
		INSERT INTO #Temp_TransferArrival
		SELECT
			 tr.ArrivalLocationCode
			,tr.SalesCarNumber
		FROM
			dbo.Transfer tr
		WHERE
			tr.ArrivalDate >= @TargetDateTo AND
			tr.DelFlag = '0' AND
			tr.SalesCarNumber is not null AND
			tr.SalesCarNumber <> '' AND
			EXISTS
			(
				select 'x' from dbo.Location l where /*l.LocationType = '001' and */l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and tr.ArrivalLocationCode = l.LocationCode
			)
		GROUP BY
			 tr.ArrivalLocationCode
			,tr.SalesCarNumber


		CREATE UNIQUE INDEX IDX_Temp_TransferArrival ON #Temp_TransferArrival(SalesCarNumber)

		-- ----------------------------------
		-- 棚卸月末以降に移動払出された車両
		-- ----------------------------------
		INSERT INTO #Temp_TransferDeparture
		SELECT
			 tr.DepartureLocationCode
			,tr.SalesCarNumber
		FROM
			dbo.Transfer tr
		WHERE
			tr.DepartureDate >= @TargetDateTo AND
			tr.DelFlag = '0' AND
			tr.SalesCarNumber is not null AND
			tr.SalesCarNumber <> '' AND
			EXISTS
			(
				select 'x' from dbo.Location l where /*l.LocationType = '001' and */l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and tr.DepartureLocationCode = l.LocationCode
			)
		GROUP BY
			 tr.DepartureLocationCode
			,tr.SalesCarNumber
			

		CREATE UNIQUE INDEX IDX_Temp_TransferDeparture ON #Temp_TransferDeparture(SalesCarNumber)


		--Add 2018/01/30 arc yano #3841
		-- -------------------------------------------------------
		-- 棚卸月末以前に移動払出されて、
		-- かつ棚卸月末以降に他部門のロケーション
		-- に入庫される車両(=移動中の車両)は移動元の車両とする
		-- ------------------------------------------------------
		INSERT INTO #Temp_ExceptTransferDeparture
		SELECT
			 tr.DepartureLocationCode
			,tr.SalesCarNumber
		FROM
			dbo.Transfer tr inner join
			dbo.SalesCar sc ON tr.SalesCarNumber = sc.SalesCarNumber
		WHERE
			tr.DepartureDate < @TargetDateTo AND							--出発日が棚卸月末以前
			(tr.ArrivalDate >= @TargetDateTo) AND							--到着日がnullまたは棚卸月末以降
			tr.DelFlag = '0' AND
			sc.DelFlag = '0' AND 
			tr.SalesCarNumber is not null AND
			tr.SalesCarNumber <> '' AND
			EXISTS
			(
				select 'x' from dbo.Location l where /*l.LocationType = '001' and */l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and tr.DepartureLocationCode = l.LocationCode
			) AND
			NOT EXISTS
			(
				select 'y' from dbo.Location l where l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and sc.LocationCode = l.LocationCode			--車両マスタのロケーションコードが対象倉庫のロケーション以外
			)
		GROUP BY
			 tr.DepartureLocationCode
			,tr.SalesCarNumber
			
		CREATE UNIQUE INDEX IDX_Temp_ExceptTransferDeparture ON #Temp_ExceptTransferDeparture(SalesCarNumber)

		-- ---------------------------------------------------------------------------
		-- 棚卸データの登録(在庫) 棚卸開始時=在庫 棚卸月月末時=在庫(or自社登録)
		-- ---------------------------------------------------------------------------
		INSERT INTO #Temp_InventoryStockCar
		SELECT
			 @WarehouseCode																--倉庫コード
			,sc.LocationCode															--ロケーションコード
			,sc.SalesCarNumber															--管理番号
			,sc.Vin																		--車台番号
			,CASE WHEN bg.NewSalesCarNumber is null THEN '999' ELSE	'006' END			--在庫区分＝在庫or自社登録
			,sc.NewUsedType																--新中区分
			
		FROM
			dbo.SalesCar sc LEFT OUTER JOIN
			dbo.BackGroundDemoCar bg ON sc.SalesCarNumber = bg.NewSalesCarNumber AND bg.ProcType = '006'
		WHERE
			(
				sc.CarUsage is NULL OR sc.CarUsage = ''
			)
			AND 
			isnull(sc.CarStatus, '') <> '006' AND 		--在庫ステータス≠「納車済」
			sc.DelFlag = '0' AND
			EXISTS	--対象の倉庫のロケーションのもの
			(
				select 'a' from Location l where  /*l.LocationType = '001' and */ l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and sc.LocationCode = l.LocationCode
			) 
			AND	
			NOT EXISTS--棚卸月末以降に入荷された車両以外
			(
				select 'b' from #Temp_PurchaseCarList pc where sc.SalesCarNumber = pc.SalesCarNumber
			) 
			AND	
			NOT EXISTS--棚卸月末以降に移動してきた車両以外
			(
				select 'c' from #Temp_TransferArrival ta where sc.SalesCarNumber = ta.SalesCarNumber
			)
			
		-- --------------------------------------------------------------------------------------------------------------
		-- 棚卸データの登録(在庫)　棚卸開始時=他部門ロケーションの在庫　棚卸月月末時=自部門ロケーションの在庫or自社登録
		-- -------------------------------------------------------------------------------------------------------------
		INSERT INTO #Temp_InventoryStockCar
		SELECT
			 @WarehouseCode																--倉庫コード
			,td.LocationCode															--ロケーションコード
			,sc.SalesCarNumber															--管理番号
			,sc.Vin																		--車台番号
			,CASE WHEN bg.NewSalesCarNumber is null THEN '999' ELSE	'006' END			--在庫区分＝在庫or自社登録
			,sc.NewUsedType																--新中区分
		FROM
			dbo.SalesCar sc INNER JOIN
			#Temp_TransferDeparture td ON sc.SalesCarNumber = td.SalesCarNumber LEFT OUTER JOIN
			dbo.BackGroundDemoCar bg ON sc.SalesCarNumber = bg.NewSalesCarNumber AND bg.ProcType = '006'
		WHERE 
			(
				sc.CarUsage is NULL OR sc.CarUsage = ''
			) 
			AND isnull(sc.CarStatus, '') <> '006'	AND 		--在庫ステータス≠「納車済」
			sc.DelFlag = '0' AND
			NOT EXISTS	--対象の倉庫のロケーション以外
			(
				select 'a' from Location l where /*l.LocationType = '001' and */ l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and sc.LocationCode = l.LocationCode
			) 
			AND
			NOT EXISTS	--棚卸月末以降に入荷された車両以外
			(
				select 'b' from #Temp_PurchaseCarList pc where sc.SalesCarNumber = pc.SalesCarNumber
			) 
			AND
			NOT EXISTS	--棚卸月末以降に移動してきた車両以外
			(
				select 'c' from #Temp_TransferArrival ta where sc.SalesCarNumber = ta.SalesCarNumber
			) 

		-- --------------------------------------------------------------------
		-- 棚卸データの登録(在庫) 棚卸開始時=納車済(通常販売) 棚卸月末時=在庫
		-- --------------------------------------------------------------------
		INSERT INTO #Temp_InventoryStockCar
		SELECT
			 @WarehouseCode																--倉庫コード
			,cs.DepartmentCode															--ロケーションコード
			,sc.SalesCarNumber															--管理番号
			,sc.Vin																		--車台番号
			,CASE WHEN bg.NewSalesCarNumber is null THEN '999' ELSE	'006' END			--在庫区分＝在庫or自社登録
			,sc.NewUsedType																--新中区分
		FROM
			dbo.SalesCar sc INNER JOIN
			#Temp_CarSalesList cs ON sc.SalesCarNumber = cs.SalesCarNumber LEFT OUTER JOIN
			dbo.BackGroundDemoCar bg ON sc.SalesCarNumber = bg.NewSalesCarNumber AND bg.ProcType = '006'
		WHERE
			(
				sc.CarUsage is NULL OR sc.CarUsage = ''			--利用用途=なし
			)
			AND 
			isnull(sc.CarStatus, '') = '006' AND 				--在庫ステータス＝「納車済」
			sc.DelFlag = '0' AND
			NOT EXISTS --棚卸月末以降に入荷された車両以外
			(
				select 'a' from #Temp_PurchaseCarList pc where sc.SalesCarNumber = pc.SalesCarNumber
			) 
			AND	
			NOT EXISTS	--棚卸月末以降に移動してきた車両以外
			(
				select 'b' from #Temp_TransferArrival ta where sc.SalesCarNumber = ta.SalesCarNumber
			)

		-- -----------------------------------------------------------------------
		-- 棚卸データの登録(在庫) 棚卸開始時=納車済(デモカー等) 棚卸月末時=在庫
		-- -----------------------------------------------------------------------
		INSERT INTO #Temp_InventoryStockCar
		SELECT
			 @WarehouseCode																--倉庫コード
			,sc.LocationCode															--ロケーションコード
			,sc.SalesCarNumber															--管理番号
			,sc.Vin																		--車台番号
			,CASE WHEN bg.NewSalesCarNumber is null THEN '999' ELSE	'006' END			--在庫区分＝在庫or自社登録
			,sc.NewUsedType																--新中区分
		FROM
			dbo.SalesCar sc INNER JOIN
			#Temp_CarSalesList_CarUsage cs ON sc.SalesCarNumber = cs.SalesCarNumber LEFT OUTER JOIN
			dbo.BackGroundDemoCar bg ON sc.SalesCarNumber = bg.NewSalesCarNumber AND bg.ProcType = '006'
		WHERE
			isnull(sc.CarStatus, '') = '006' AND 				--在庫ステータス＝「納車済」
		NOT EXISTS --棚卸月末以降に入荷された車両以外
		(
			select 'a' from #Temp_PurchaseCarList pc where sc.SalesCarNumber = pc.SalesCarNumber
		) AND	
		NOT EXISTS	--棚卸月末以降に移動してきた車両
		(
			select 'b' from #Temp_TransferArrival ta where sc.SalesCarNumber = ta.SalesCarNumber
		) AND --入庫ロケーションが自部門のもの
		EXISTS
		(
			select 'c' from dbo.Location l where /* l.LocationType = '001' and */ l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and sc.LocationCode = l.LocationCode
		)
		
		-- ---------------------------------------------------------------------------------------------------------------------
		-- 棚卸データの登録(デモカー、レンタカー、業務車、広報車、代車)　棚卸開始時=「デモカー等」棚卸月末時=「デモカー等」
		-- ---------------------------------------------------------------------------------------------------------------------
		INSERT INTO #Temp_InventoryStockCar
		SELECT
			 @WarehouseCode
			,sc.LocationCode	
			,sc.SalesCarNumber
			,sc.Vin																--車台番号
			,sc.CarUsage														--在庫区分＝「デモカー」「レンタカー」「業務車」「広報車」「代車」
			,sc.NewUsedType
		FROM
			dbo.SalesCar sc
		WHERE
			sc.CarUsage in ('001', '002', '003', '004', '005') AND				--利用用途=「デモカー」「レンタカー」「業務車」「広報車」「代車」
			sc.CarStatus = '006' AND											--在庫ステータス=「納車済」
			sc.DelFlag = '0' AND
			EXISTS	--対象の倉庫のロケーションのもの
			(
				select 'a' from Location l where /* l.LocationType = '001' and */ l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and sc.LocationCode = l.LocationCode
			) 
			AND		
			NOT EXISTS --棚卸月末以降に納車された車両以外
			(
				select 'b' from #Temp_CarSalesList_CarUsage cs where sc.SalesCarNumber = cs.SalesCarNumber
			)
		-- ----------------------------------------------------------------------------------------------------------------------
		-- 棚卸データの登録(デモカー、レンタカー、業務車、広報車、代車) ※棚卸開始時=除却により削除済、棚卸月末時=デモカー等
		-- ----------------------------------------------------------------------------------------------------------------------
		INSERT INTO #Temp_InventoryStockCar
		SELECT
			 @WarehouseCode
			,sc.LocationCode	
			,sc.SalesCarNumber
			,sc.Vin																		--車台番号
			,sc.CarUsage														--在庫区分＝「デモカー」「レンタカー」「業務車」「広報車」「代車」
			,sc.NewUsedType
		FROM
			dbo.SalesCar sc
		WHERE
			sc.CarUsage in ('001', '002', '003', '004', '005') AND				--利用用途=「デモカー」「レンタカー」「業務車」「広報車」「代車」
			sc.CarStatus = '006' AND											--在庫ステータス=「納車済」
			sc.DelFlag = '1' AND												--現時点では除却により削除されたもの
			EXISTS	--入庫ロケーションが対象の倉庫のロケーションのもの
			(
				select 'a' from Location l where /* l.LocationType = '001' and */ l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and sc.LocationCode = l.LocationCode
			) 
			AND		
			NOT EXISTS --棚卸月末以降に入荷された車両以外
			(
				select 'b' from #Temp_PurchaseCarList pc where sc.SalesCarNumber = pc.SalesCarNumber
			) 
			AND	
			NOT EXISTS	--棚卸月末以降に移動してきた車両
			(
				select 'c' from #Temp_TransferArrival ta where sc.SalesCarNumber = ta.SalesCarNumber
			) 
			AND --棚卸月末以降にデモカー等になった車両
			NOT EXISTS
			(
				select 'd' from #Temp_CarSalesList_CarUsage cs where sc.SalesCarNumber = cs.SalesCarNumber
			)
			AND --棚卸月末以降に除却となったもの
			EXISTS
			(
				select 'e' from dbo.BackGroundDemoCar bg where bg.ProcType = '010' and bg.ProcDate >= @TargetDateTo and sc.SalesCarNumber = bg.SalesCarNumber
			)

		-- Add 2018/01/30 arc yano #3841
		-- ----------------------------------------------------------------
		-- 出発日が棚卸月末以前、到着日が棚卸月末以降(=移動中)の車両の取得
		-- ----------------------------------------------------------------
		INSERT INTO #Temp_InventoryStockCar
		SELECT
			 @WarehouseCode																--倉庫コード
			,td.LocationCode															--ロケーションコード
			,sc.SalesCarNumber															--管理番号
			,sc.Vin																		--車台番号
			,CASE WHEN bg.NewSalesCarNumber is null THEN '999' ELSE	'006' END			--在庫区分＝在庫or自社登録
			,sc.NewUsedType																--新中区分
		FROM
			dbo.SalesCar sc INNER JOIN
			#Temp_ExceptTransferDeparture td ON sc.SalesCarNumber = td.SalesCarNumber LEFT OUTER JOIN
			dbo.BackGroundDemoCar bg ON sc.SalesCarNumber = bg.NewSalesCarNumber AND bg.ProcType = '006'
		WHERE 
			sc.DelFlag = '0'

		-- ---------------------
		--　index作成
		-- ---------------------
		CREATE UNIQUE INDEX IDX_Temp_InventoryStockCar ON #Temp_InventoryStockCar(Vin)



		-- ----------------------
		-- 棚卸データへ登録
		-- ----------------------
		INSERT INTO dbo.InventoryStockCar
		SELECT
			 NEWID()			AS InventoryId
			,''					AS DepartmentCode
			,@InventoryMonth	AS InventoryMonth
			,isc.LocationCode	AS LocationCode
			,@EmployeeCode		AS EmployeeCode
			,isc.SalesCarNumber AS SalesCarNumber
			,sc.Vin				AS Vin
			,isc.NewUsedType	AS NewUsedType
			,isc.CarStatus		AS CarUsage
			,1					AS Quantity
			,@EmployeeCode		AS CreateEmployeeCode
			,GETDATE()			AS CreateDate
			,@EmployeeCode		AS LastUpdateEmployeeCode
			,GETDATE()			AS LastUpdateDate
			,'0'				AS DelFlag
			,''					AS Summary
			,null				AS PhysicalQuantity		--Mod 2017/12/18 arc yano #3840
			,''					AS Comment
			,@WarehouseCode		AS WarehouseCode
		FROM
			#Temp_InventoryStockCar isc LEFT OUTER JOIN
			dbo.SalesCar sc ON isc.SalesCarNumber = sc.SalesCarNumber
			

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

END




GO


