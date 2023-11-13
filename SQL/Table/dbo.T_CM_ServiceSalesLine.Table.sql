	--2017/03/11 arc yano #3725 サブシステム移行(整備履歴)
	USE[WPH_DB]

	BEGIN
		--処理結果
		DECLARE @ErrorMessage NVARCHAR(4000) = ''
		DECLARE @ErrorNumber INT = 0

		BEGIN TRANSACTION
		BEGIN TRY

			DECLARE @RecCnt int = 0
			--存在チェック
			SELECT 
				@RecCnt = COUNT(id) 
			FROM
				dbo.sysobjects
			WHERE
				id = object_id(N'T_CM_ServiceSalesLine')


			--未登録の場合のみ以降の処理を行う(登録済の場合は行わない)
			IF @RecCnt = 0
			BEGIN
				
				--テーブル作成
				CREATE TABLE [dbo].[T_CM_ServiceSalesLine](
					[DepartmentName] [varchar](50) NULL,
					[SystemName] [varchar](50) NULL,
					[SlipNumber] [varchar](50) NULL,
					[LineNumber] [varchar](50) NULL,
					[SalesInputDate] [varchar](50) NULL,
					[RedSlipType] [varchar](50) NULL,
					[CustomerCode] [varchar](50) NULL,
					[CustomerNameKana] [varchar](50) NULL,
					[CustomerName1] [varchar](50) NULL,
					[CustomerName2] [varchar](50) NULL,
					[RegistName] [varchar](50) NULL,
					[RegistType] [varchar](50) NULL,
					[RegistNumberKana] [varchar](50) NULL,
					[RegistNumber] [varchar](50) NULL,
					[ServiceWorkCode] [varchar](50) NULL,
					[ServiceWorkName] [varchar](500) NULL,
					[ContentsName] [varchar](500) NULL,
					[ContentsType] [varchar](500) NULL,
					[FrontEmployeeCode] [varchar](500) NULL,
					[FrontEmployeeName] [varchar](50) NULL,
					[MechanicEmployeeCode] [varchar](50) NULL,
					[MechanicEmployeeName] [varchar](50) NULL,
					[MechanicEmployeeCodeDetail] [varchar](50) NULL,
					[MechanicEmployeeNameDetail] [varchar](50) NULL,
					[Quantity] [varchar](50) NULL,
					[TechnicalFeeAmount] [varchar](50) NULL,
					[PartsAmount] [varchar](50) NULL,
					[VariousAmount] [varchar](50) NULL,
					[ServiceName] [varchar](50) NULL,
					[TotalTechnicalFeeAmount] [varchar](50) NULL,
					[TotalPartsAmount] [varchar](50) NULL,
					[TotalVariousAmount] [varchar](50) NULL,
					[TechnicalFeeTaxAmount] [varchar](50) NULL,
					[VariousTaxAmount] [varchar](50) NULL,
					[TotalTaxAmount] [varchar](50) NULL,
					[RunningData] [varchar](50) NULL
				) ON [PRIMARY]


				--EUCDBのデータをWPH_DBに移行
				INSERT INTO
					[WPH_DB].[dbo].[T_CM_ServiceSalesLine]
					SELECT
						*
					FROM
						[EUCDB].[dbo].[T_CM_ServiceSalesLine]


				--更新履歴テーブルに登録
				INSERT INTO [dbo].[DB_ReleaseHistory]
					   ([HistoryID]
					   ,[TicketNumber]
					   ,[QueryName]
					   ,[ReleaseDate]
					   ,[Summary]
					   ,[ExecEmployeeCode]
					   ,[ExecDate])
				 VALUES
					   (NEWID()
					   ,'3725'--チケット番号
					   ,'20170313_#3725_サブシステム移行（整備履歴）/05_Create_Table_T_CM_ServiceSalesLine.sql'
					   ,CONVERT(datetime, '2017/03/13', 120)		--※※※※※※※※※※リリース日(WPH様入力)※※※※※※※※※※※
					   ,''--コメント
					   ,'arima.yuji'	--実行者
					   ,GETDATE()		--実行日
				   )
			END

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
		BEGIN
			RAISERROR (@ErrorMessage, 16, 1)
			PRINT 'データがロールバックされました'
		END
	END
