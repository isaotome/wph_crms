	USE[WPH_DB]

	BEGIN
		--��������
		DECLARE @ErrorMessage NVARCHAR(4000) = ''
		DECLARE @ErrorNumber INT = 0

		BEGIN TRANSACTION
		BEGIN TRY

			DECLARE @RecCnt int = 0
			--���݃`�F�b�N
			SELECT 
				@RecCnt = COUNT(id) 
			FROM
				dbo.sysobjects
			WHERE
				id = object_id(N'T_CM_ServiceSalesHeader')


			--���o�^�̏ꍇ�݈̂ȍ~�̏������s��(�o�^�ς̏ꍇ�͍s��Ȃ�)
			IF @RecCnt = 0
			BEGIN
				
				--�e�[�u���쐬
				CREATE TABLE [dbo].[T_CM_ServiceSalesHeader](
					[DepartmentName] [varchar](50) NULL,
					[SystemName] [varchar](50) NULL,
					[SlipNumber] [varchar](50) NULL,
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
					[ServiceWorkName] [varchar](50) NULL,
					[FrontEmployeeCode] [varchar](50) NULL,
					[FrontEmployeeName] [varchar](50) NULL,
					[MechanicEmployeeCode] [varchar](50) NULL,
					[MechanicEmployeeName] [varchar](50) NULL,
					[TechnicalFeeAmount] [varchar](50) NULL,
					[PartsAmount] [varchar](50) NULL,
					[VariousAmount] [varchar](50) NULL,
					[TechnicalFeeTaxAmount] [varchar](50) NULL,
					[VariousTaxAmount] [varchar](50) NULL,
					[TotalTaxAmount] [varchar](50) NULL,
					[RunningData] [varchar](50) NULL,
					[vin] [nvarchar](50) NULL
				) ON [PRIMARY]

				--EUCDB�̃f�[�^��WPH_DB�Ɉڍs
				INSERT INTO
					[WPH_DB].[dbo].[T_CM_ServiceSalesHeader]
					SELECT
						*
					FROM
						[EUCDB].[dbo].[T_CM_ServiceSalesHeader]


				--�X�V�����e�[�u���ɓo�^
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
					   ,'3725'--�`�P�b�g�ԍ�
					   ,'20170313_#3725_�T�u�V�X�e���ڍs�i���������j/04_Create_Table_T_CM_ServiceSalesHeader.sql'
					   ,CONVERT(datetime, '2017/03/??', 120)		--�������������������������[�X��(WPH�l����)����������������������
					   ,''--�R�����g
					   ,'arima.yuji'	--���s��
					   ,GETDATE()		--���s��
				   )
			END

		--�g�����U�N�V�����I��
		COMMIT TRANSACTION
		END TRY
		BEGIN CATCH
			ROLLBACK TRANSACTION
			SELECT 
				@ErrorNumber = ERROR_NUMBER()
			,	@ErrorMessage = ERROR_MESSAGE()
		END CATCH

	FINALLY:
			--�G���[����
		IF @ErrorNumber <> 0
		BEGIN
			RAISERROR (@ErrorMessage, 16, 1)
			PRINT '�f�[�^�����[���o�b�N����܂���'
		END
	END
