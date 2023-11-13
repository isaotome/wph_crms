USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetDuplicationCarInventory]    Script Date: 2017/06/28 18:01:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
-- Description:	<Description,,>
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetDuplicationCarInventory] 
	   @InventoryMonth datetime						--棚卸月
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

	
	--■一時表の削除
	/*************************************************************************/

	IF OBJECT_ID(N'tempdb..#Temp_InventoryStock', N'U') IS NOT NULL
	DROP TABLE #Temp_InventoryStock;

	/*************************************************************************/
	

	/*-------------------------------------------*/
	/* ■■一時表の宣言							 */
	/*-------------------------------------------*/
	--車両棚卸リスト
	CREATE TABLE #Temp_InventoryStock (
		 [InventoryMonth] Datetime NOT NULL			--棚卸月
		,[Vin] NVARCHAR(20) NOT NULL				--車台番号
		,[RecCount] int NOT NULL					--重複レコード数
	)

	/*-------------------------------------------*/
	/* データ取得								 */
	/*-------------------------------------------*/

	--トランザクション開始 
	BEGIN TRANSACTION
	BEGIN TRY
		
		--車両リストの挿入
		INSERT INTO #Temp_InventoryStock
		SELECT
			 isc.InventoryMonth	AS InventoryMonth
			,isc.Vin			AS Vin
			,COUNT(isc.Vin)		AS RecCount
		FROM
			dbo.InventoryStockCar isc
		WHERE
			isc.InventoryMonth = @InventoryMonth AND
			isc.PhysicalQuantity = 1 AND
			isc.DelFlag = '0'
		GROUP BY
			 isc.InventoryMonth
			,isc.Vin

		CREATE INDEX IDX_Temp_InventoryStock ON #Temp_InventoryStock(InventoryMonth, Vin)

		-- ----------------------
		-- 棚卸データへ登録
		-- ----------------------
		SELECT
			 ivs.InventoryId AS InventoryId
			,ivs.InventoryMonth AS InventoryMonth
			,ivs.DepartmentCode AS DepartmentCode
			,ivs.WarehouseCode AS WarehouseCode
			,ivs.LocationCode AS LocationCode
			,lc.LocationName AS LocationName
			,ivs.SalesCarNumber AS SalesCarNumber
			,ivs.Vin AS Vin
			,ivs.NewUsedType AS NewUsedType
			,cn.Name AS NewUsedTypeName
			,ivs.CarUsage AS CarUsage
			,cn2.Name AS CarStatusName
			,cm.CarBrandName AS CarBrandName
			,cm.CarName AS CarName
			,cc.Name AS ColorType
			,sc.ExteriorColorCode AS ExteriorColorCode
			,sc.ExteriorColorName AS ExteriorColorName
			,(sc.MorterViecleOfficialCode + ' ' + sc.RegistrationNumberType + ' ' + sc.RegistrationNumberKana + ' ' + sc.RegistrationNumberPlate) AS RegistrationNumber
			,sc.MorterViecleOfficialCode AS MorterViecleOfficialCode
			,sc.RegistrationNumberType AS RegistrationNumberType
			,sc.RegistrationNumberKana AS RegistrationNumberKana
			,sc.RegistrationNumberPlate AS RegistrationNumberPlate
			,ivs.PhysicalQuantity AS PhysicalQuantity
			,ivs.Comment AS Comment
			,cn3.Name AS CommentName
			,ivs.Summary AS Summary
		FROM
			dbo.InventoryStockCar ivs INNER JOIN
			dbo.Location lc ON ivs.LocationCode = lc.LocationCode INNER JOIN
			dbo.SalesCar sc ON ivs.SalesCarNumber = sc.SalesCarNumber INNER JOIN
			dbo.c_NewUsedType cn ON ivs.NewUsedType = cn.Code INNER JOIN
			dbo.V_CarMaster cm ON sc.CarGradeCode = cm.CarGradeCode LEFT OUTER JOIN
			dbo.c_ColorCategory cc ON sc.ColorType = cc.Code INNER JOIN
			dbo.c_CodeName cn2 ON ivs.CarUsage = cn2.Code AND cn2.CategoryCode = '020' LEFT OUTER JOIN
			dbo.c_CodeName cn3 ON ivs.Comment = cn3.Code AND cn3.CategoryCode = '022'
		WHERE
			ivs.InventoryMonth = InventoryMonth AND
			ivs.DelFlag = '0' AND
			lc.DelFlag = '0' AND
			--sc.DelFlag = '0' AND
			EXISTS
			(
				select 'x' from #Temp_InventoryStock tisc where tisc.RecCount > 1 and tisc.InventoryMonth = ivs.InventoryMonth and tisc.Vin = ivs.Vin
			)
		ORDER BY
			ivs.Vin			

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
			 NEWID() AS InventoryID
			,GETDATE() AS InventoryMonth
			,CONVERT(nvarchar(3), '') AS DepartmentCode
			,CONVERT(nvarchar(6), '') AS WarehouseCode
			,CONVERT(nvarchar(12), '') AS LocationCode
			,CONVERT(nvarchar(50), '') AS LocationName
			,CONVERT(nvarchar(50), '') AS SalesCarNumber
			,CONVERT(nvarchar(20), '') AS Vin
			,CONVERT(nvarchar(3), '') AS NewUsedType
			,CONVERT(nvarchar(50), '') AS NewUsedTypeName
			,CONVERT(nvarchar(3), '') AS CarUsage
			,CONVERT(nvarchar(50), '') AS CarStatusName
			,CONVERT(nvarchar(3), '50') AS CarBrandName
			,CONVERT(nvarchar(20), '') AS CarName
			,CONVERT(nvarchar(50), '') AS ColorType
			,CONVERT(nvarchar(8), '') AS ExteriorColorCode
			,CONVERT(nvarchar(50), '') AS ExteriorColorName
			,CONVERT(nvarchar(50), '') AS RegistrationNumber
			,CONVERT(nvarchar(5), '') AS MorterViecleOfficialCode
			,CONVERT(nvarchar(3), '') AS RegistrationNumberType
			,CONVERT(nvarchar(1), '') AS RegistrationNumberKana
			,CONVERT(nvarchar(4), '') AS RegistrationNumberPlate
			,CONVERT(decimal(10, 3), '') AS PhysicalQuantity
			,CONVERT(nvarchar(255), '') AS Comment
			,CONVERT(nvarchar(50), '') AS CommentName
			,CONVERT(nvarchar(255), '') AS Summary
*/

END


GO


