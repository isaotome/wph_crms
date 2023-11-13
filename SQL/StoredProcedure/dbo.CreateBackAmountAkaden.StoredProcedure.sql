USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[CreateBackAmountAkaden]    Script Date: 2016/07/01 16:57:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--�@�\�F�ԓ`�[�쐬��A���ƂƂȂ����`�[�ƍ쐬���ꂽ�ԓ`�̓����\��̍��z���o���āA���z���O�łȂ���΍��z���̓����\����쐬����	
--2016/05/26 arc nakayama #3418_�ԍ��`�[���s���̍��`�[�̓����\��iReceiptPlan�j�̎c���̌v�Z���@  �V�K�쐬
--2016/07/01 arc nakayama #3593_�`�[�ɑ΂��ē��ꐿ���悪�����������ꍇ�̍l��
CREATE PROCEDURE [dbo].[CreateBackAmountAkaden]

	@SlipNumber nvarchar(50),	--�`�[�ԍ�
	@EmployeeCode nvarchar(50)	--�Ј��R�[�h
AS
	SET NOCOUNT ON
	
	/*-------------------------------------------*/
	/* �_�[�e�B�[���[�h�̐ݒ�					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;


	BEGIN

	BEGIN TRY
		DROP TABLE #temp_ReceiptPlan
		DROP TABLE #temp_ReceiptPlan2
		DROP TABLE #temp_AkaReceiptPlan
		DROP TABLE #temp_NewReceiptPlan
	END TRY
	BEGIN CATCH
		--����
	END CATCH

	/*---------------------------------------------------------------*/
	/* �`�[�ԍ����L�[�ɂ��Č��`�[�̓����\��𐿋���ʂɎ擾			 */
	/*---------------------------------------------------------------*/
	BEGIN TRY
		CREATE TABLE #temp_ReceiptPlan(
			  SlipNumber nvarchar(50)
			, CustomerClaimCode nvarchar(10)
			, Amount decimal(10,0)
			, ReceivableBalance decimal(10,0)
		)

		INSERT INTO #temp_ReceiptPlan
		SELECT rp.[SlipNumber]							--�`�[�ԍ�
			  ,rp.[CustomerClaimCode]					--������R�[�h
			  ,SUM(ISNULL(rp.Amount, 0)) AS [Amount]	--���z���v(�T�}��)
			  ,SUM(ISNULL(rp.ReceivableBalance, 0))		--�c���i�T�}���j
		FROM [dbo].[ReceiptPlan] rp
		WHERE rp.[SlipNumber] = @SlipNumber
		  AND rp.[CustomerClaimCode] is not NULL
		  AND rp.[CustomerClaimCode] <> ''
		  AND rp.[DelFlag] = '0'
		GROUP BY  rp.[SlipNumber], rp.[CustomerClaimCode]


		/*-----------------------------------------------------------*/
		/* �`�[�ԍ����L�[�ɂ��Đԓ`�[�̓����\��𐿋���ʂɎ擾		 */
		/*-----------------------------------------------------------*/

		CREATE TABLE #temp_AkaReceiptPlan(
			  SlipNumber nvarchar(50)
			, CustomerClaimCode nvarchar(10)
			, Amount decimal(10,0)
			, ReceivableBalance decimal(10,0)
		)

		INSERT INTO #temp_AkaReceiptPlan
		SELECT rp.[SlipNumber]							--�`�[�ԍ�
			  ,rp.[CustomerClaimCode]					--������R�[�h
			  ,SUM(ISNULL(rp.Amount, 0)) AS [Amount]	--���z���v(�T�}��)
			  ,SUM(ISNULL(rp.ReceivableBalance, 0))		--�c���i�T�}���j
		FROM [dbo].[ReceiptPlan] rp 
		WHERE rp.[SlipNumber] = @SlipNumber + '-1'		--�ԓ`�̔ԍ�
		  AND rp.[CustomerClaimCode] is not NULL
		  AND rp.[CustomerClaimCode] <> ''
		  AND rp.[DelFlag] = '0'
		GROUP BY rp.[SlipNumber], rp.[CustomerClaimCode]


		/*-------------------------------------------------------------------*/
		/* ���`�[�̋��z����ԓ`�[�̋��z���������l�ō��z�����߂�				 */
		/*-------------------------------------------------------------------*/
		CREATE TABLE #temp_NewReceiptPlan(
			  SlipNumber nvarchar(50)
			, CustomerClaimCode nvarchar(10)
			, Amount decimal(10,0)
			, ReceivableBalance decimal(10,0)
		)

		INSERT INTO #temp_NewReceiptPlan
		SELECT rp.SlipNumber
			  ,rp.CustomerClaimCode
			  ,rp.Amount + (ap.Amount)
			  ,rp.ReceivableBalance + (ap.ReceivableBalance)

		FROM #temp_ReceiptPlan rp
		INNER JOIN #temp_AkaReceiptPlan ap on ap.CustomerClaimCode = rp.CustomerClaimCode


		/*-----------------------------------*/
		/* �ԋ����̓����\����쐬����		 */
		/*-----------------------------------*/


		CREATE TABLE #temp_ReceiptPlan2(
			  DepartmentCode nvarchar(3)
			, OccurredDepartmentCode nvarchar(3)
			, SlipNumber nvarchar(50)
			, CustomerClaimCode nvarchar(10)
			, ReceiptType nvarchar(3)
			, AccountCode nvarchar(50)
		)


		INSERT INTO #temp_ReceiptPlan2
		SELECT r.DepartmentCode
			 , r.OccurredDepartmentCode
			 , r.SlipNumber
			 , r.CustomerClaimCode
			 , r.ReceiptType
			 , r.AccountCode
		FROM WPH_DB.dbo.ReceiptPlan r
		WHERE r.DelFlag = '0'
		GROUP BY r.DepartmentCode, r.OccurredDepartmentCode, r.CustomerClaimCode, r.SlipNumber, r.ReceiptType, r.AccountCode




		INSERT INTO WPH_DB.dbo.ReceiptPlan
		SELECT NEWID() AS ReceiptPlanId
			   ,r.[DepartmentCode]
			   ,r.[OccurredDepartmentCode]
			   ,np.[CustomerClaimCode]
			   ,np.[SlipNumber]
			   ,r.[ReceiptType]
			   ,null --r.[ReceiptPlanDate]
			   ,r.[AccountCode]
			   ,np.[Amount]
			   ,np.[ReceivableBalance]
			   ,'0' AS CompleteFlag		-- 0�Œ�
			   ,@EmployeeCode AS CreateEmployeeCode
			   ,GETDATE() AS CreateDate
			   ,@EmployeeCode AS LastUpdateEmployeeCode
			   ,GETDATE() AS LastUpdateDate
			   ,'0' AS DelFlag
			   ,'�`�[�ԍ�' + REPLACE(np.SlipNumber, '-1', '') + '�̐ԓ`������' AS Summary
			   ,null --r.[JournalDate]
			   ,'0' --[DepositFlag] ����p�������܂Ƃ߂ĕԋ��̗\��ɂ��邽��0�Œ�
			   ,''--r.[PaymentKindCode]
			   ,null --r.[CommissionRate]
			   ,null --r.[CommissionAmount]
			   ,''

		FROM #temp_NewReceiptPlan np
		INNER JOIN  #temp_ReceiptPlan2 r on r.SlipNumber = np.SlipNumber and r.CustomerClaimCode = np.CustomerClaimCode
		--INNER JOIN (select TOP 1 rp.* FROM WPH_DB.dbo.ReceiptPlan rp where rp.DelFlag = '0') as r on r.SlipNumber = np.SlipNumber and r.CustomerClaimCode = np.CustomerClaimCode
 		WHERE np.ReceivableBalance != 0 --�c�����O�ȊO�̂���
	END TRY

	BEGIN CATCH
		RETURN ERROR_NUMBER()
	END CATCH

	RETURN 0
END
GO


