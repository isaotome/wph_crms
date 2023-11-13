USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[InsertPartsLocation]    Script Date: 2016/10/03 15:29:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- ==============================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2016/08/13	arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
-- 2016/03/17   arc yano #3477 部品ロケーションマスタ　ロケーションマスタの自動更新 棚卸確定時に部品ロケーションマスタに反映する
-- Update date
-- Description:	
--				在庫情報を元に部品ロケーションマスタを更新する
-- ==============================================================================================================================================
CREATE PROCEDURE [dbo].[InsertPartsLocation] 
	 --@DepartmentCode nvarchar(3),
	 @WarehouseCode nvarchar(6),
	 @EmployeeCode nvarchar(50)
AS	
	BEGIN

		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;

		--処理結果
		DECLARE @ErrorMessage NVARCHAR(4000) = ''
		DECLARE @ErrorNumber INT = 0

		
		--■■一時表の削除
		/*************************************************************************/
		IF OBJECT_ID(N'tempdb..#Temp_PartsStock', N'U') IS NOT NULL
		DROP TABLE #Temp_PartsStock;											--部品在庫情報
		
		
		--■■一時表の宣言
		--部品在庫情報
		CREATE TABLE #Temp_PartsStock (
			[PartsNumber] NVARCHAR(25) NOT NULL				-- 部品番号
		,	[Quantity] DECIMAL(10, 0)						-- 数量
		)
		CREATE UNIQUE INDEX IX_Temp_PartsStock ON #Temp_PartsStock ([PartsNumber], [Quantity])

		--トランザクション開始 
		BEGIN TRANSACTION
		BEGIN TRY
		
			-- ----------------------------------------
			-- 部品ロケーションマスタ削除処理
			-- ----------------------------------------
			--一度部品対象部門のロケーションを削除
			DELETE 
				pl
			FROM 
				dbo.PartsLocation AS pl
			WHERE
				--DepartmentCode = @DepartmentCode
				WarehouseCode = @WarehouseCode					--Mod 2016/08/13 arc yano #3596				
		
			-- ----------------------------------------
			-- 部品ロケーションマスタ作成処理
			-- ----------------------------------------
			--部品在庫テーブルから、部品毎に最も在庫数が多いものを取得する
			INSERT INTO #Temp_PartsStock
				SELECT 
					DISTINCT gps.PartsNumber, gps.Quantity
				FROM
				(
					SELECT
						ps.[PartsNumber] as PartsNumber
					,	max(ps.Quantity) as Quantity
					FROM 
						[WPH_DB].[dbo].[PartsStock] ps
					WHERE 
						DelFlag = '0' 
					AND
					EXISTS
					(
						--SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.DepartmentCode = @DepartmentCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode
						SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode	--Mod 2016/08/13 arc yano #359
					)
					GROUP BY ps.PartsNumber
				) gps
			--インデックス再作成

		
			DROP INDEX IX_Temp_PartsStock ON #Temp_PartsStock
			CREATE UNIQUE INDEX IX_Temp_PartsStock ON #Temp_PartsStock ([PartsNumber], [Quantity])

			--ロケーションマスタ反映
			INSERT INTO dbo.PartsLocation
				
				SELECT 
					  ps.PartsNumber  AS PartsNumber			--部品番号
					, '' AS DepartmentCode						--部門コード ※空文字
					, MIN(ps.LocationCode) AS LocationCode		--ロケーションコード
					, @EmployeeCode AS CreateEmployeeCode		--作成者
					, GETDATE() AS CreateDate					--作成日
					, @EmployeeCode AS LastUpdateEmployeeCode	--最終更新者
					, GETDATE()  AS LastUpdateDate				--最終更新日
					, '0' AS DelFlag							--削除フラグ
					, @WarehouseCode AS WarehouseCode			--倉庫コード
				FROM 	
					PartsStock ps
				WHERE 
					EXISTS
					(
						SELECT 'X' FROM #Temp_PartsStock tps WHERE tps.PartsNumber = ps.PartsNumber AND tps.Quantity = ps.Quantity
					)
					AND ps.DelFlag = '0' AND
					/*
					EXISTS
					(
						SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.DepartmentCode = @DepartmentCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode 
					)
					*/
					EXISTS
					(
						SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and l.LocationType = '001' and l.LocationCode GO

/****** Object:  StoredProcedure [dbo].[InsertPartsLocation]    Script Date: 2020/11/20 19:20:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- ==============================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2016/03/17   arc yano #3477 部品ロケーションマスタ　ロケーションマスタの自動更新 棚卸確定時に部品ロケーションマスタに反映する
-- Update date
-- 2020/11/06 yano #4036【部品入荷】入荷済部品の過去のロケーション情報が消える
-- 2016/08/13	arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
-- Description:	
--				在庫情報を元に部品ロケーションマスタを更新する
-- ==============================================================================================================================================
CREATE PROCEDURE [dbo].[InsertPartsLocation] 
	 --@DepartmentCode nvarchar(3),
	 @WarehouseCode nvarchar(6),
	 @EmployeeCode nvarchar(50)
AS	
	BEGIN

		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;

		--処理結果
		DECLARE @ErrorMessage NVARCHAR(4000) = ''
		DECLARE @ErrorNumber INT = 0

		
		--■■一時表の削除
		/*************************************************************************/
		IF OBJECT_ID(N'tempdb..#Temp_PartsStock', N'U') IS NOT NULL
		DROP TABLE #Temp_PartsStock;											--部品在庫情報
		
		
		--■■一時表の宣言
		--部品在庫情報
		CREATE TABLE #Temp_PartsStock (
			[PartsNumber] NVARCHAR(25) NOT NULL				-- 部品番号
		,	[Quantity] DECIMAL(10, 2)						-- 数量				--Mod  2020/11/06 yano #4036
		)
		CREATE UNIQUE INDEX IX_Temp_PartsStock ON #Temp_PartsStock ([PartsNumber], [Quantity])

		--トランザクション開始 
		BEGIN TRANSACTION
		BEGIN TRY
		
			-- ----------------------------------------
			-- 部品ロケーションマスタ削除処理
			-- ----------------------------------------
			--一度部品対象部門のロケーションを削除
			DELETE 
				pl
			FROM 
				dbo.PartsLocation AS pl
			WHERE
				--DepartmentCode = @DepartmentCode
				WarehouseCode = @WarehouseCode					--Mod 2016/08/13 arc yano #3596				
		
			-- ----------------------------------------
			-- 部品ロケーションマスタ作成処理
			-- ----------------------------------------
			--部品在庫テーブルから、部品毎に最も在庫数が多いものを取得する
			INSERT INTO #Temp_PartsStock
				SELECT 
					DISTINCT gps.PartsNumber, gps.Quantity
				FROM
				(
					SELECT
						ps.[PartsNumber] as PartsNumber
					,	max(ps.Quantity) as Quantity
					FROM 
						[dbo].[PartsStock] ps
					WHERE 
						DelFlag = '0' 
					AND
					EXISTS
					(
						--SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.DepartmentCode = @DepartmentCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode
						SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode	--Mod 2016/08/13 arc yano #359
					)
					GROUP BY ps.PartsNumber
				) gps
			--インデックス再作成

		
			DROP INDEX IX_Temp_PartsStock ON #Temp_PartsStock
			CREATE UNIQUE INDEX IX_Temp_PartsStock ON #Temp_PartsStock ([PartsNumber], [Quantity])

			--ロケーションマスタ反映
			INSERT INTO dbo.PartsLocation
				
				SELECT 
					  ps.PartsNumber  AS PartsNumber			--部品番号
					, '' AS DepartmentCode						--部門コード ※空文字
					, MIN(ps.LocationCode) AS LocationCode		--ロケーションコード
					, @EmployeeCode AS CreateEmployeeCode		--作成者
					, GETDATE() AS CreateDate					--作成日
					, @EmployeeCode AS LastUpdateEmployeeCode	--最終更新者
					, GETDATE()  AS LastUpdateDate				--最終更新日
					, '0' AS DelFlag							--削除フラグ
					, @WarehouseCode AS WarehouseCode			--倉庫コード
				FROM 	
					PartsStock ps
				WHERE 
					EXISTS
					(
						SELECT 'X' FROM #Temp_PartsStock tps WHERE tps.PartsNumber = ps.PartsNumber AND tps.Quantity = ps.Quantity
					)
					AND ps.DelFlag = '0' AND
					/*
					EXISTS
					(
						SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.DepartmentCode = @DepartmentCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode 
					)
					*/
					EXISTS
					(
						SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode 
					)
				GROUP BY
					ps.PartsNumber

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

