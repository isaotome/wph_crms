USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetShikakariSummary]    Script Date: 2017/06/29 9:10:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- ????/??/?? 
-- Description:	<Description,,>
-- 指定月の仕掛中の部品代、外注費を部門毎にサマリ
-- Mod 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 部品棚卸フラグによる絞込に変更
-- Mod 2017/03/27 arc yano #3735 仕掛品在庫表（暫定）修正対応 全部門が出るように対応する
-- ======================================================================================================================================

CREATE PROCEDURE [dbo].[GetShikakariSummary]
	@TargetDate datetime = NULL			--対象年月
,	@NowFlag nvarchar(2) = NULL			--現在の仕掛かどうか（1：現在　0：過去）
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @ERR_NUMBER INT = 0
	DECLARE @ERR_MESSAGE NVARCHAR(4000) = ''

	
	
	--■一時表の削除
	/*************************************************************************/
	IF OBJECT_ID(N'tempdb..#temp_PartsWipStock', N'U') IS NOT NULL
	DROP TABLE #temp_PartsWipStock;														--仕掛テーブル
	--IF OBJECT_ID(N'tempdb..#temp_PartsAverageCost2', N'U') IS NOT NULL
	--DROP TABLE #temp_PartsAverageCost2;													--移動平均単価テーブル
	
	IF OBJECT_ID(N'tempdb..#temp_InProcess', N'U') IS NOT NULL
	DROP TABLE #temp_InProcess;															--仕掛在庫テーブル

	BEGIN TRY

		--仕掛部品在庫の金額
		CREATE TABLE #temp_InProcess(
			 WarehouseCode nvarchar(6)
			,InProcessAmount decimal(10,0)
		)

		--仕掛中の外注費
			CREATE TABLE #temp_PartsWipStock(
				 DepartmentCode nvarchar(3)
			--	,TotalAmount decimal(21,2)
				,OutOrderCost decimal(10, 0) 
			)

		--データ挿入
		INSERT INTO #temp_InProcess
		SELECT
			 pb.WarehouseCode
			,SUM(ISNULL(pb.InProcessAmount, 0)) AS InProcessAmount
		FROM
			dbo.PartsBalance pb
		WHERE
			pb.CloseMonth = @TargetDate
		GROUP BY
			pb.WarehouseCode

		CREATE UNIQUE INDEX idx_temp_InProcess ON #temp_InProcess(WarehouseCode)


		IF @NowFlag = '1'
		--棚卸ステータスが未実施または実施中の場合の検索
		BEGIN

			/*
			DECLARE @closeMonth DATETIME
			
			SELECT 
				@closeMonth = MAX(CloseMonth)
			FROM 
				dbo.PartsAverageCost
			
			--最新月の部品の単価
			CREATE TABLE #temp_PartsAverageCost2(
				 PartsNumber nvarchar(25)
				,Price decimal(10,0)
			)

			INSERT INTO 
				#temp_PartsAverageCost2
				SELECT
					 PC.PartsNumber
					,PC.Price
				FROM
					dbo.PartsAverageCost AS PC
				WHERE 
					PC.CloseMonth = @closeMonth
			*/


			/*
			--仕掛中の部品の金額(作業中～納車確認書印刷済み)
			INSERT INTO
				#temp_PartsWipStock
			SELECT
				 H.DepartmentCode
				,ISNULL(PC_1.Price, 0) * ISNULL(L.Quantity, 0) AS TotalAmount
				,0 AS OutOrderCost
			FROM
				dbo.ServiceSalesHeader AS H INNER JOIN 
				dbo.ServiceSalesLine AS L ON L.SlipNumber = H.SlipNumber AND L.RevisionNumber = H.RevisionNumber LEFT OUTER JOIN  
				#temp_PartsAverageCost2 AS PC_1 ON  PC_1.PartsNumber = L.PartsNumber
			WHERE
				H.ServiceOrderStatus >= '003' AND
				H.ServiceOrderStatus <= '005' AND
				H.DelFlag = '0' AND	
				L.ServiceType = '003' AND --サービスタイプ=「部品」
				L.WorkType <> '015'AND 
				EXISTS
				(				
					SELECT 'x' FROM dbo.Parts p WHERE p.PartsNumber = L.PartsNumber AND p.NonInventoryFlag <> 1
				)
				AND L.DelFlag = '0'
					

			insert into #temp_PartsWipStock
			select
				H.DepartmentCode
				,ISNULL(PC_1.Price, 0) * ISNULL(L.Quantity, 0) AS TotalAmount
				,0 AS OutOrderCost
			FROM   dbo.ServiceSalesHeader AS H 
			INNER JOIN dbo.ServiceSalesLine AS L ON L.SlipNumber = H.SlipNumber AND L.RevisionNumber = H.RevisionNumber
			LEFT OUTER JOIN  #temp_PartsAverageCost2 AS PC_1 ON  PC_1.PartsNumber = L.PartsNumber
			WHERE H.ServiceOrderStatus >= '003' 
				AND  H.ServiceOrderStatus <= '005'
				AND	H.DelFlag = '0' 
				AND	L.ServiceType = '003' --サービスタイプ=「部品」
				AND L.WorkType IS NULL
				AND EXISTS(				
						SELECT 'x' FROM dbo.Parts p WHERE p.PartsNumber = L.PartsNumber AND p.NonInventoryFlag <> 1
					)
				AND L.DelFlag = '0'


			--仕掛中の部品の金額(納車済みで納車日が未来日)
			insert into #temp_PartsWipStock
			select
				H.DepartmentCode
				,ISNULL(PC_1.Price, 0) * ISNULL(L.Quantity, 0) AS TotalAmount
				,0 AS OutOrderCost
			FROM   dbo.ServiceSalesHeader AS H 
			INNER JOIN dbo.ServiceSalesLine AS L ON L.SlipNumber = H.SlipNumber AND L.RevisionNumber = H.RevisionNumber
			LEFT OUTER JOIN  #temp_PartsAverageCost2 AS PC_1 ON  PC_1.PartsNumber = L.PartsNumber
			WHERE H.DelFlag = '0' 
				AND	H.ServiceOrderStatus = '006' 
				AND H.SalesDate > GETDATE()
				AND	L.ServiceType = '003' --サービスタイプ=「部品」 
				AND L.WorkType <> '015'
				AND EXISTS(				
						SELECT 'x' FROM dbo.Parts p WHERE p.PartsNumber = L.PartsNumber AND p.NonInventoryFlag <> 1
					)
				AND L.DelFlag = '0'
					

			insert into #temp_PartsWipStock
			select
				H.DepartmentCode
				,ISNULL(PC_1.Price, 0) * ISNULL(L.Quantity, 0) AS TotalAmount
				,0 AS OutOrderCost
			FROM   dbo.ServiceSalesHeader AS H 
			INNER JOIN dbo.ServiceSalesLine AS L ON L.SlipNumber = H.SlipNumber AND L.RevisionNumber = H.RevisionNumber
			LEFT OUTER JOIN  #temp_PartsAverageCost2 AS PC_1 ON  PC_1.PartsNumber = L.PartsNumber
			WHERE   H.DelFlag = '0' 
				AND	H.ServiceOrderStatus = '006' 
				AND H.SalesDate > GETDATE()
				AND	L.ServiceType = '003' --サービスタイプ=「部品」 
				AND L.WorkType IS NULL
				AND EXISTS(				
						SELECT 'x' FROM dbo.Parts p WHERE p.PartsNumber = L.PartsNumber AND p.NonInventoryFlag <> 1
					)
				AND L.DelFlag = '0'
			*/
					
			--仕掛中の外注費(作業中～納車確認書印刷済み)
			INSERT INTO 
				#temp_PartsWipStock
			SELECT
				 H.DepartmentCode
				--,0 AS TotalAmount
				,SUM(ISNULL(L.Cost, 0)) AS OutOrderCost
			FROM
				dbo.ServiceSalesHeader AS H  INNER JOIN 
				dbo.ServiceSalesLine AS L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber
			--LEFT OUTER JOIN  #temp_PartsAverageCost2 AS PC_1 ON  L.PartsNumber = PC_1.PartsNumber
			WHERE
				H.DelFlag = '0'  AND
				(
					( 
					  H.ServiceOrderStatus >= '003' AND 
					  H.ServiceOrderStatus <= '005'
					)
					OR
					(
						H.ServiceOrderStatus = '006' AND
						H.SalesDate > GETDATE() 
					)
				) AND
				L.DelFlag = '0' AND 
				L.ServiceType = '002' AND --サービスタイプ=「サービスメニュー」
				L.SupplierCode IS NOT NULL
			GROUP BY
				H.DepartmentCode

			CREATE UNIQUE INDEX idx_temp_PartsWipStock ON #temp_PartsWipStock(DepartmentCode)
			
			/*
			--仕掛中の外注費金額(納車済みで納車日が未来日)
			INSERT INTO
				#temp_PartsWipStock
			SELECT
				 H.DepartmentCode
				--,0 AS TotalAmount
				,L.Cost AS OutOrderCost
			FROM
				dbo.ServiceSalesHeader AS H INNER JOIN 
				dbo.ServiceSalesLine AS L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber 
				--LEFT OUTER JOIN #temp_PartsAverageCost2 AS PC_1 ON  L.PartsNumber = PC_1.PartsNumber
			WHERE	H.DelFlag = '0' 
				AND H.ServiceOrderStatus = '006' 
				AND H.SalesDate > GETDATE()
				AND L.DelFlag = '0'
				AND L.ServiceType = '002' --サービスタイプ=「サービスメニュー」
				AND L.SupplierCode IS NOT NULL
			*/


			--部門毎の部品、外注費、合計
			SELECT
				 B.DepartmentCode
				,B.DepartmentName
				,ISNULL(IP.InProcessAmount,0) AS PartsTotalAmount
				,ISNULL(A.OutOrderCost, 0) AS TotalCost
				,(ISNULL(IP.InProcessAmount,0) + ISNULL(A.OutOrderCost, 0)) AS GrandTotalAmount
			FROM
				dbo.Department B INNER JOIN
				dbo.DepartmentWarehouse DW ON B.DepartmentCode = DW.DepartmentCode AND DW.DelFlag = '0' LEFT JOIN
				#temp_InProcess IP ON DW.WarehouseCode = IP.WarehouseCode LEFT JOIN 
				#temp_PartsWipStock A on B.DepartmentCode = A.DepartmentCode
			WHERE 
				B.PartsInventoryFlag = '1' AND	-- Mod 2017/05/10 arc yano #3762
				--B.CloseMonthFlag = '2' AND
				ISNULL(B.DelFlag, '0') = '0' 
			ORDER BY 
				B.DepartmentCode	
		END
		ELSE
			--棚卸ステータスが確定の場合の検索
		BEGIN 

			--仕掛中の外注費(作業中～納車確認書印刷済み)
			INSERT INTO 
				#temp_PartsWipStock
			SELECT
				 A.DepartmentCode
				,SUM(ISNULL(A.Cost, 0)) AS OutOrderCost
			FROM
				dbo.InventoryParts_Shikakari A
			WHERE
				A.InventoryMonth = @TargetDate
			GROUP BY
				A.DepartmentCode

			CREATE UNIQUE INDEX idx_temp_PartsWipStock ON #temp_PartsWipStock(DepartmentCode)


			SELECT 
				 B.DepartmentCode
				,B.DepartmentName
				,ISNULL(IP.InProcessAmount,0) AS PartsTotalAmount
				,ISNULL(A.OutOrderCost, 0) AS TotalCost
				,(ISNULL(IP.InProcessAmount,0) + ISNULL(A.OutOrderCost, 0)) AS GrandTotalAmount
			FROM
				dbo.Department B INNER JOIN
				dbo.DepartmentWarehouse DW ON B.DepartmentCode = DW.DepartmentCode AND DW.DelFlag = '0' LEFT JOIN
				#temp_InProcess IP ON DW.WarehouseCode = IP.WarehouseCode LEFT JOIN 
				#temp_PartsWipStock A ON B.DepartmentCode = A.DepartmentCode 			
			WHERE
				ISNULL(B.PartsInventoryFlag, '0') = '1'  AND -- Mod 2017/05/10 arc yano #3762
				--ISNULL(B.CloseMonthFlag, '0') = '2' AND 
				ISNULL(B.DelFlag , '0') = '0'
			ORDER BY 
				B.DepartmentCode


			
			/*
			SELECT 
				B.DepartmentCode,
				B.DepartmentName,
				SUM(isnull(A.Amount,0)) as PartsTotalAmount,
				SUM(isnull(A.Cost,0)) as TotalCost,
				SUM(isnull(A.Amount,0))+SUM(isnull(A.Cost,0)) as GrandTotalAmount
			FROM
				WPH_DB.dbo.Department B LEFT JOIN		--Mod 2017/03/27 arc yano #3735
				WPH_DB.dbo.InventoryParts_Shikakari A ON 
						B.DepartmentCode = A.DepartmentCode AND 
						DATEDIFF(m,A.InventoryMonth, @TargetDate)=0
			WHERE 
				ISNULL(B.CloseMonthFlag, '0') = '2' AND 
				ISNULL(B.DelFlag , '0') = '0'
			Group By 
				  B.DepartmentCode
				, B.DepartmentName
			ORDER BY 
				B.DepartmentCode
			*/
		END

	END TRY
	BEGIN CATCH
		SELECT
			@ERR_NUMBER = ERROR_NUMBER()
		,	@ERR_MESSAGE = ERROR_MESSAGE()
	END CATCH

	RETURN @ERR_NUMBER
END



GO


