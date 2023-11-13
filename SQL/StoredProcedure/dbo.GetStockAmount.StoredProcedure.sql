USE [WPH_DB]
GO


-- 2014/12/22 arc yano 部品確認対応 EOMONTHはSqlServer2008に未対応のため、使用中止。DATEADDで代用する。
/****** Object:  StoredProcedure [dbo].[GetStockAmount]    Script Date: 2014/12/02 13:05:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO










CREATE PROCEDURE [dbo].[GetStockAmount]
	 @TargetDate datetime, 
	 @InventoryStatusParts NCHAR(3),
	 @CompanyCode nvarchar(3)
	 	 
AS	
	BEGIN
		
		
		-- 仕掛在庫テーブル
		DECLARE @StockShikakariTable TABLE
		(
			DepartmentCode nvarchar(3),
			WipInventoryAmount  decimal(13, 3)
		)
		

		-- 在庫テーブル
		DECLARE @StockAmountTable TABLE
		(
			DepartmentCode nvarchar(3),
			StockAmount  decimal(13, 3)
		)

		--部品棚卸スターテス
		IF @InventoryStatusParts = '002'
			
			-- 仕掛在庫データを内部のテーブルに格納
			INSERT INTO @StockShikakariTable (DepartmentCode, WipInventoryAmount)

				SELECT 
					ipssl.DepartmentCode, sum(ipssl.Amount) as WipInventoryAmount
				FROM
					(
					SELECT
						ips.InventoryMonth, ips.DepartmentCode, ips.PartsNumber, (sl.Price * ips.Quantity) AS Amount
					FROM
						(
						SELECT 
							InventoryMonth,
							DepartmentCode,
							SlipNumber,
							LineNumber,
							Quantity,
							PartsNumber
						FROM
							dbo.InventoryParts_Shikakari
						WHERE
							InventoryMonth = @TargetDate
							and PartsNumber is not null
							and PartsNumber <>''
						) AS ips INNER JOIN
						(
						SELECT
							CloseMonth,
							PartsNumber,
							Price,
							CloseDateTime,
							CreateDate,
							CreateEmployeeCode,
							LastUpdateDate,
							LastUpdateEmployeeCode,
							DelFlag
						FROM
							dbo.PartsAverageCost
						WHERE
							CloseMonth = DATEADD(m, -1, @TargetDate) 
							and CompanyCode = @CompanyCode
							and DelFlag = '0'
						) AS sl ON (ips.PartsNumber = sl.PartsNumber)
					) AS ipssl
					GROUP BY ipssl.InventoryMonth , ipssl.DepartmentCode
		ELSE

			INSERT INTO @StockShikakariTable (DepartmentCode, WipInventoryAmount)

				SELECT 
						shlpa.DepartmentCode, sum(shlpa.Amount) as WipInventoryAmount
					FROM
						(
						SELECT
							pa.CloseMonth, shsl.DepartmentCode, shsl.PartsNumber, (pa.Price * shsl.Quantity) AS Amount
						FROM
							(
								SELECT 
									sh.SalesDate,
									sh.SlipNumber, 
									sh.RevisionNumber, 
									sh.DepartmentCode,
									sl.PartsNumber,
									sl.Quantity
								FROM
								(
									SELECT
										SlipNumber, RevisionNumber, SalesDate, DepartmentCode
									FROM
										dbo.ServiceSalesHeader
									WHERE
										DelFlag = '0' 
										and ServiceOrderStatus in ('003', '004', '005')
								) AS sh INNER JOIN
								(
									SELECT 
										SlipNumber, RevisionNumber, PartsNumber, Quantity
									FROM
										dbo.ServiceSalesLine
									WHERE
										DelFlag = '0' 
										and PartsNumber is not null
										and PartsNumber <> ''
								) AS sl ON (sh.SlipNumber = sl.SlipNumber AND sh.RevisionNumber = sl.RevisionNumber)
							) AS shsl inner join
							(
								SELECT
									CloseMonth,
									PartsNumber,
									Price,
									CloseDateTime,
									CreateDate,
									CreateEmployeeCode,
									LastUpdateDate,
									LastUpdateEmployeeCode,
									DelFlag
								FROM
									dbo.PartsAverageCost
								WHERE
									CloseMonth = DATEADD(m, -1, @TargetDate) 
									and CompanyCode = @CompanyCode
									and DelFlag = '0'
							) as pa on (shsl.PartsNumber = pa.PartsNumber)
						) as shlpa
					GROUP BY shlpa.CloseMonth, shlpa.DepartmentCode
		
		
		--部品棚卸スターテス
		IF @InventoryStatusParts = '002'
			-- 在庫テーブル

			INSERT INTO @StockAmountTable (DepartmentCode, StockAmount)
			(
				SELECT 
					ivpa.DepartmentCode, sum(ivpa.Amount) as StockAmount
				FROM
				(
					SELECT 
						iv.InventoryMonth, iv.DepartmentCode, iv.PartsNumber, iv.Quantity * pa.Price AS Amount
				
					FROM
					(
						SELECT 
							InventoryMonth, DepartmentCode, PartsNumber, Quantity
						FROM
							dbo.InventoryStock
						WHERE
							InventoryMonth = @TargetDate
							AND InventoryType = '002' 
							AND DelFlag = '0'
					) AS iv inner join
					(
						SELECT
							CloseMonth, PartsNumber, Price				
						FROM
							dbo.PartsAverageCost
						WHERE
							CloseMonth = DATEADD(m, -1, @TargetDate) AND
							DelFlag = '0'
					) AS pa ON iv.PartsNumber = pa.PartsNumber
				) as ivpa
				GROUP BY ivpa.InventoryMonth, ivpa.DepartmentCode
			)
		ELSE
			INSERT INTO @StockAmountTable (DepartmentCode, StockAmount)
			(
				SELECT 
					pspa.DepartmentCode, sum(pspa.Amount) as StockAmount
					
				FROM
				(
					SELECT 
						psla.DepartmentCode, psla.PartsNumber, psla.Quantity * pa.Price AS Amount
				
					FROM
					(
						SELECT la.DepartmentCode, ps.PartsNumber, ps.Quantity
						FROM
						(
							SELECT 
								PartsNumber, Quantity, LocationCode
							FROM
								dbo.PartsStock
							WHERE
								DelFlag = '0'
						) as ps left outer join
						dbo.Location as la  ON ps.LocationCode = la.LocationCode
					) AS psla inner join
					(
						SELECT
							CloseMonth, PartsNumber, Price				
						FROM
							dbo.PartsAverageCost
						WHERE
							CloseMonth = DATEADD(m, -1, @TargetDate) AND
							DelFlag = '0'
					) AS pa ON psla.PartsNumber = pa.PartsNumber
				) as pspa
				GROUP BY pspa.DepartmentCode
			)
	 
		SELECT 
			bi.DepartmentCode AS biDepartmentCode,
			dl.DepartmentCode AS dlDepartmentCode,
			wi.DepartmentCode AS wiDepartmentCode,
			sc.DepartmentCode AS scDepartmentCode,
			ac.DepartmentCode AS acDepartmentCode,
			bi.BeginningInventoryAmount, 
			dl.DeliveredAmount, 
			wi.WipInventoryAmount, 
			sc.StockAmount,
			ac.ActualShelfAmount
		FROM
		
		--月初在庫データ取得
		
		(
			SELECT 
				ivpa.DepartmentCode, sum(ivpa.Amount) as BeginningInventoryAmount
			FROM
				(
				SELECT
					iv.InventoryMonth, iv.DepartmentCode, iv.PartsNumber, iv.Quantity, pa.Price, iv.Quantity * pa.Price as Amount
				FROM
					(
						SELECT
						InventoryMonth,
						DepartmentCode,
						InventoryType,
						PartsNumber,
						Quantity
					FROM
						dbo.InventoryStock
					WHERE
						InventoryMonth = DATEADD(m, -1, @TargetDate) and
						DelFlag = '0' and
						InventoryType = '002'
					) AS iv inner join
					(
					SELECT
						CloseMonth,
						PartsNumber,
						Price,
						CloseDateTime,
						CreateDate,
						CreateEmployeeCode,
						LastUpdateDate,
						LastUpdateEmployeeCode,
						DelFlag
					FROM
						dbo.PartsAverageCost
					WHERE
						CloseMonth = DATEADD(m, -1, @TargetDate)
						and CompanyCode = @CompanyCode 
						and DelFlag = '0'
					) as pa on (iv.PartsNumber = pa.PartsNumber)
				) as ivpa
			GROUP BY ivpa.InventoryMonth, ivpa.DepartmentCode
		
		) AS bi  full outer join 

		--納車済データ取得
		(
			SELECT 
				shlpa.DepartmentCode, sum(shlpa.Amount) as DeliveredAmount
			FROM
				(
				SELECT
					pa.CloseMonth, shsl.DepartmentCode, shsl.PartsNumber, (pa.Price * shsl.Quantity) AS Amount
				FROM
					(
						SELECT 
							sh.SalesDate,
							sh.SlipNumber, 
							sh.RevisionNumber, 
							sh.DepartmentCode,
							sl.PartsNumber,
							sl.Quantity
						FROM
						(
							SELECT
								SlipNumber, RevisionNumber, SalesDate, DepartmentCode
							FROM
								dbo.ServiceSalesHeader
							WHERE
								ServiceOrderStatus = '006' 
								and DelFlag = '0' 
						) AS sh INNER JOIN
						(
							SELECT 
								SlipNumber, RevisionNumber, PartsNumber, Quantity
							FROM
								dbo.ServiceSalesLine
							WHERE
								DelFlag = '0' 
								and PartsNumber is not null
								and PartsNumber <> ''
						) AS sl ON (sh.SlipNumber = sl.SlipNumber AND sh.RevisionNumber = sl.RevisionNumber)
						WHERE
							sh.SalesDate >=  @TargetDate 
							--and sh.SalesDate <=  EOMONTH(@TargetDate) 2014/12/22 MOD SqlServer2008ではEOMONTHを使用できないため、DATEADDで代用する。
							and sh.SalesDate <=  DATEADD(day,DAY(DATEADD(m, 1, @TargetDate)) * - 1 ,DATEADD(m, 1, CONVERT(datetime, @TargetDate)))
							
					) AS shsl inner join
					(
						SELECT
							CloseMonth,
							PartsNumber,
							Price,
							CloseDateTime,
							CreateDate,
							CreateEmployeeCode,
							LastUpdateDate,
							LastUpdateEmployeeCode,
							DelFlag
						FROM
							dbo.PartsAverageCost
						WHERE
							CloseMonth = DATEADD(m, -1, @TargetDate) 
							and CompanyCode = @CompanyCode
							and DelFlag = '0'
					) as pa on (shsl.PartsNumber = pa.PartsNumber)
				) as shlpa
			GROUP BY shlpa.CloseMonth, shlpa.DepartmentCode

			
			) as dl ON bi.DepartmentCode = dl.DepartmentCode full outer join

		-- 仕掛在庫テーブル
			@StockShikakariTable as wi on bi.DepartmentCode = wi.DepartmentCode full outer join
		-- 在庫テーブル
			@StockAmountTable as sc on bi.DepartmentCode = sc.DepartmentCode full outer join

		--実棚テーブル
			(
			SELECT 
				invspa.DepartmentCode, sum(invspa.Amount) AS ActualShelfAmount
			FROM
			(
	
				SELECT invs.InventoryMonth, invs.DepartmentCode, invs.PartsNumber, invs.Quantity*pa.Price AS Amount
	
				FROM
				(
					SELECT 
						InventoryMonth, DepartmentCode, PartsNumber, Quantity
					FROM
						dbo.InventoryStock
					WHERE
						DelFlag = '0' 
						AND InventoryType = '002' 
						AND InventoryMonth = @TargetDate
				) AS invs inner join
				(
					SELECT CloseMonth, PartsNumber, Price
					FROM	
						dbo.PartsAverageCost
					WHERE
						CloseMonth = @TargetDate
						AND CompanyCode = @CompanyCode
						AND DelFlag = '0'
				) AS pa ON  invs.PartsNumber = pa.PartsNumber
	
			) AS invspa
			GROUP BY invspa.DepartmentCode
		
		) as ac on bi.DepartmentCode = ac.DepartmentCode
	END











GO


