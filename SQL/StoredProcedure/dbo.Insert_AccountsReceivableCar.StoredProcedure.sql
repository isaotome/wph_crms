USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[Insert_AccountsReceivableCar]    Script Date: 2019/06/04 15:18:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- ==============================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2016/08/26 arc nakayama  #3630_�y�����z�ԗ����|���Ή� 
-- Update date
-- Mod 2019/05/22 yano #3954 �y�T�[�r�X���|���z�����[�Ԃ̔��|�����\������Ȃ� �ގ��Ή�
-- 2018/01/29 arc yano #3717 �T�[�r�X���|��_�`�[�����݂��Ȃ����̓������т̔��f �ގ�����
-- 2016/12/22 arc yano #3682 �ԗ����|���@�V�X�e���G���[ �������т̏W�v���@��ύX���ێ琫�̌���̂��߁A�ȑO�ɃR�����g�A�E�g���ꂽ�������폜

-- ==============================================================================================================================================
CREATE PROCEDURE [dbo].[Insert_AccountsReceivableCar] 

	@TargetMonth datetime,						--�w�茎
	@ActionFlag int = 0,						--����w��(0:��ʕ\��, 1:�X�i�b�v�V���b�g�ۑ�)
	@SalesDateFrom NVARCHAR(10) = NULL,			--�[�ԓ�(From)
	@SalesDateTo NVARCHAR(10) = NULL,			--�[�ԓ�(To)
	@SlipNumber NVARCHAR(50) = NULL,			--�`�[�ԍ�
	@DepartmentCode NVARCHAR(3) = NULL,			--����R�[�h
	@CustomerCode NVARCHAR(10) = NULL,			--�ڋq�R�[�h
	@Zerovisible NVARCHAR(1) = NULL				--�[���\���t���O(0:��\�� 1:�\��)
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-----------------------
	--�ȍ~�͋���
	-----------------------
	--�ϐ��錾
	DECLARE @NOW DATETIME = GETDATE()
	DECLARE @TODAY DATETIME
	DECLARE @THISMONTH DATETIME

	--��������
	DECLARE @ErrorMessage NVARCHAR(4000) = ''
	DECLARE @ErrorNumber INT = 0


	--���ꎞ�\�̍폜
	/*************************************************************************/

	IF OBJECT_ID(N'tempdb..#Temp_CodeList', N'U') IS NOT NULL
	DROP TABLE #Temp_CodeList;														--�R�[�h���X�g(�����\��^���тɑ��݂���S�Ă̓`�[�ԍ��A������R�[�h���̑g����)
	IF OBJECT_ID(N'tempdb..#Temp_CodeList', N'U') IS NOT NULL
	DROP TABLE #Temp_CodeNameList;													--�R�[�h�l�[�����X�g
	IF OBJECT_ID(N'tempdb..#Temp_CarriedBalance', N'U') IS NOT NULL
	DROP TABLE #Temp_CarriedBalance													--�O���J�z���X�g
	IF OBJECT_ID(N'tempdb..#Temp_ReceiptPlan', N'U') IS NOT NULL
	DROP TABLE #Temp_ReceiptPlan;													--�����\�胊�X�g
	IF OBJECT_ID(N'tempdb..#Temp_SalesOrderReceiptPlan', N'U') IS NOT NULL
	DROP TABLE #Temp_SalesOrderReceiptPlan;											--�����\�胊�X�g(�ԗ�)
	IF OBJECT_ID(N'tempdb..#Temp_Journal', N'U') IS NOT NULL
	DROP TABLE #Temp_Journal;														--�������у��X�g
	--IF OBJECT_ID(N'tempdb..#Temp_SalesOrderJournal', N'U') IS NOT NULL
	--DROP TABLE #Temp_SalesOrderJournal;											--�������у��X�g(�ԗ�)		--Del 2016/12/22 arc yano #3682
	IF OBJECT_ID(N'tempdb..#Temp_AccountsReceivableCar', N'U') IS NOT NULL
	DROP TABLE #Temp_AccountsReceivableCar;											--���|���f�[�^���X�g
	IF OBJECT_ID(N'tempdb..#Temp_CarSalesHeader', N'U') IS NOT NULL
	DROP TABLE #Temp_CarSalesHeader;												--�ԗ��`�[�w�b�_
	IF OBJECT_ID(N'tempdb..#Temp_SlipInfo', N'U') IS NOT NULL
	DROP TABLE #Temp_SlipInfo;														--�`�[���			--Add 2016/12/22 arc yano #3682
	IF OBJECT_ID(N'tempdb..#Temp_SlipList', N'U') IS NOT NULL
	DROP TABLE #Temp_SlipList;														--�`�[���			--Add 2018/01/27 arc yano #3717
	
	--�����ꎞ�\�̐錾
	/*************************************************************************/

	--�R�[�h���X�g
	CREATE TABLE #Temp_CodeList (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	)

	--�O���J�z���X�g
	CREATE TABLE #Temp_CarriedBalance (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[CarriedBalance] DECIMAL(10, 0)					-- �O���J�z��
	)
	CREATE UNIQUE INDEX IX_Temp_CarriedBalance ON #Temp_CarriedBalance ([SlipNumber])
			
	--�����\�胊�X�g
	CREATE TABLE #Temp_ReceiptPlan (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[Amount] DECIMAL(10, 0)							-- �����\��z
	,	[TradeInAmount] DECIMAL(10, 0)					-- ����d��
	,	[RemainDebtAmount] DECIMAL(10, 0)				-- �c���z
	,	[PresentMonth] DECIMAL(10, 0)					-- ��������(�����\��z�{����d�����z�{�c���z)
	)
	CREATE UNIQUE INDEX IX_Temp_ReceiptPlan ON #Temp_ReceiptPlan ([SlipNumber])

	--�����\�胊�X�g(�ԗ�)
	CREATE TABLE #Temp_SalesOrderReceiptPlan (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[Amount] DECIMAL(10, 0)							-- �����\��z
	,	[TradeInAmount] DECIMAL(10, 0)					-- ����d��
	,	[RemainDebtAmount] DECIMAL(10, 0)				-- �c��
	,	[PresentMonth] DECIMAL(10, 0)					-- ��������(�����\��z�{����p)
	)
	CREATE UNIQUE INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan ([SlipNumber])

	--�������у��X�g
	CREATE TABLE #Temp_Journal (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[TradeInAmount] DECIMAL(10, 0)					-- ����d��
	,	[RemainDebtAmount] DECIMAL(10, 0)				-- �c��
	,	[Amount] DECIMAL(10, 0)							-- �����z
	,   [TotalAmount] DECIMAL(10, 0)					-- �����z���v(����d�� + �c�� + �����z)
	)
	CREATE UNIQUE INDEX IX_Temp_Journal ON #Temp_Journal ([SlipNumber])

	--Del 2016/12/22 arc yano #3682 
	/*
	--�������у��X�g(�ԗ�)
	CREATE TABLE #Temp_SalesOrderJournal (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[TradeInAmount] DECIMAL(10, 0)					-- ����d��
	,	[RemainDebtAmount] DECIMAL(10, 0)				-- �c��
	,	[Amount] DECIMAL(10, 0)							-- ���̑��̓����z
	,   [TotalAmount] DECIMAL(10, 0)					-- �����z���v(����d�� + �c�� + �����z)
	)
	CREATE UNIQUE INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal ([SlipNumber])
	*/

	--���|���f�[�^
	CREATE TABLE #Temp_AccountsReceivableCar(
		CloseMonth datetime,				--����
		SlipNumber nvarchar(50),			--�`�[�ԍ�
		DepartmentCode nvarchar(3),			--����R�[�h
		DepartmentName nvarchar(20),		--���喼
		CustomerCode nvarchar(10),			--�ڋq�R�[�h
		CustomerName nvarchar(80),			--�ڋq��
		SalesDate datetime,					--�[�ԓ�
		CarriedBalance decimal(10,0),		--�O���J�z
		PresentMonth decimal(10,0),			--��������
		PaymentAmount decimal(10,0),		--��������
		CustomerBalance decimal(10,0),		--�ڋq�������̎c���i���v�j
		TradeBalance decimal(10,0),			--����d���c���i���v�j
		RemainDebtBalance decimal(10,0),	--����c�c���i���v�j
		BalanceAmount decimal(10,0),		--�c��
	)
	CREATE UNIQUE INDEX IX_Temp_AccountsReceivable ON #Temp_AccountsReceivableCar ([CloseMonth], [SlipNumber])

	CREATE TABLE #Temp_CarSalesHeader (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[SalesDate]	datetime							-- �[�ԓ�
	)
	CREATE UNIQUE INDEX IX_Temp_CarSalesHeader ON #Temp_CarSalesHeader ([SlipNumber])

	--Add 2016/12/22 arc yano #3682
	--�`�[���i�[
	CREATE TABLE #Temp_SlipInfo (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	)
	CREATE UNIQUE INDEX IX_Temp_SlipInfo ON #Temp_SlipInfo ([SlipNumber])

	--Add 2018/01/27 arc yano #3717
	--�`�[
	CREATE TABLE #Temp_SlipList (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	)
	CREATE UNIQUE INDEX IX_Temp_SlipList ON #Temp_SlipList ([SlipNumber])

	/*************************************************************************/

	--���ݓ���
	SET @NOW = GETDATE()
	--����1��
	SET @THISMONTH = CONVERT(DATETIME, CONVERT(NVARCHAR(7), @TODAY, 111) + '/01', 111)

	--�������Ώی��͈͂̐ݒ�
	--���u�������ߏ������߁v���S������܂��Ă��錎�̒��ōő匎�̗�����1��<=x<���������̗���1������(�܂��́A�����̏ꍇ�͓��������j
	--���������Ώی�From�̐ݒ�i���܂��Ă��錎�̒��ōő匎�̗���1���j
	DECLARE @TargetMonthFrom DATETIME = NULL
	
	--����w��ɂ��U����
	IF @ActionFlag = 0	--�\���̏ꍇ�́A�{���ߍŐV���̗����ɐݒ�
	BEGIN
		SELECT @TargetMonthFrom = DATEADD(m, 1, ISNULL(MAX(CONVERT(datetime, cm.[CloseMonth], 120)), @THISMONTH))
		FROM [CloseMonthControl] cm				--�o����
		WHERE cm.[CloseStatus] = '003'			--�{����
	END
	ELSE
	BEGIN
		SET @TargetMonthFrom = @TargetMonth --�X�i�b�v�V���b�g�ۑ��̏ꍇ�͎w�茎��ݒ肷��B
	END
	
	--�Ώی��������ɂȂ�ꍇ�A�����Ƃ���
	IF @TargetMonthFrom > @TODAY
		SET @TargetMonthFrom = @THISMONTH
	
	--���������Ώی�To�̐ݒ�(�w�茎)
	--�w�茎��NULL�̏ꍇ�A�w�茎=������ݒ肷��B�@�����W�b�N�Ƃ��Ă͒ʂ�Ȃ�
	IF @TargetMonth is null
		SET @TargetMonth = @THISMONTH
	
	--�w�茎��ݒ�(�w�茎�̗���1������)
	DECLARE @TargetMonthTo DATETIME = DATEADD(m, 1, @TargetMonth)

	--�����Ώی����^�����Ώی��O��
	DECLARE @TargetMonthCount INT = DATEDIFF(m, @TargetMonthFrom, @TargetMonthTo)
	DECLARE @TargetMonthPrevious DATETIME = DATEADD(m, -1, @TargetMonthFrom)

	--�����Ώۓ��t�͈�From�^�����Ώۓ��t�͈�To
	DECLARE @TargetDateFrom DATETIME = @TargetMonthFrom
	DECLARE @TargetDateTo DATETIME = DATEADD(m, 1, @TargetDateFrom)

	IF @TargetDateTo > @NOW		--���t�͈�TO���������̏ꍇ�A���݂ɂ���
		SET @TargetDateTo = @NOW
	
	--�g�����U�N�V�����J�n 
	BEGIN TRANSACTION
	BEGIN TRY
		
		--���O�����̃f�[�^���ꎞ�e�[�u���ɂ��炩���ߊi�[����B
		INSERT INTO #Temp_AccountsReceivableCar
		SELECT
			CloseMonth,				--����
			SlipNumber,				--�`�[�ԍ�
			DepartmentCode,			--����R�[�h
			DepartmentName,			--���喼
			CustomerCode,			--�ڋq�R�[�h
			CustomerName,			--�ڋq��
			SalesDate,				--�[�ԓ�
			CarriedBalance,			--�O���J�z
			PresentMonth,			--��������
			PaymentAmount,			--��������
			CustomerBalance,		--�ڋq�������̎c���i���v�j
			TradeBalance,			--����d���c���i���v�j
			RemainDebtBalance,		--����c�c���i���v�j
			BalanceAmount			--�c��

		FROM [dbo].AccountsReceivableCar
		
		WHERE CloseMonth = @TargetMonthPrevious	--�Ώ۔N���̑O��
		  AND (ISNULL(CustomerBalance, 0) <> 0 OR ISNULL(TradeBalance, 0) <> 0 OR ISNULL(RemainDebtBalance, 0) <> 0 OR ISNULL(BalanceAmount, 0) <> 0)
		  AND DelFlag = '0'

		--�������Ώی��������[�v
		WHILE @TargetMonthCount > 0
		BEGIN
			--�ꎟ�\������
			DELETE FROM  #Temp_CodeList							--�R�[�h���X�g
			DELETE FROM  #Temp_CarriedBalance					--�O���J�z���X�g
			DELETE FROM  #Temp_ReceiptPlan						--�����\�胊�X�g
			DELETE FROM  #Temp_SalesOrderReceiptPlan			--�����\�胊�X�g
			DELETE FROM  #Temp_Journal							--�������у��X�g
			--DELETE FROM  #Temp_SalesOrderJournal				--�������у��X�g
			DELETE FROM  #Temp_CarSalesHeader					--�ԗ��`�[(�ԓ`�E���`)���O�p
			DELETE FROM  #Temp_SlipInfo							--�`�[���									--Add 2016/12/22 arc yano #3682
			DELETE FROM  #Temp_SlipList							--�⊮�`�[���X�g						�@�@--Add 2018/01/27 arc yano #3717
			
			/********************
			�����O���J�z���X�g
			*********************/
			--�O�����̎c����0�łȂ����̂��擾
			INSERT INTO #Temp_CarriedBalance
			SELECT	[SlipNumber]								--�`�[�ԍ�
				,	[SalesDate]									--�[�ԓ�
				,	[DepartmentCode]							--����R�[�h
				,	[CustomerCode]								--�ڋq�R�[�h
				,	[BalanceAmount]								--�c��
			FROM
				#Temp_AccountsReceivableCar 					--�ꎞ�e�[�u������擾
			WHERE
				[CloseMonth] = @TargetMonthPrevious
				AND (
					ISNULL(CustomerBalance, 0) <> 0 OR 
					ISNULL(TradeBalance, 0) <> 0 OR 
					ISNULL(RemainDebtBalance, 0) <> 0 OR 
					ISNULL(BalanceAmount, 0) <> 0
					)
			
			/********************
			���������\�胊�X�g
			*********************/
			--�@�����\��f�[�^��`�[�ԍ��A������R�[�h�ŏW�v�������ʂ��i�[����B
			INSERT INTO #Temp_ReceiptPlan
			SELECT
					rp.[SlipNumber]																											--�`�[�ԍ�
				,	SUM(CASE WHEN  (rp.ReceiptType <> '012' AND rp.ReceiptType <> '013') THEN ISNULL(rp.Amount, 0) ELSE 0 END) AS [Amount]	--����E�c�ȊO�̓����\��z
				,	SUM(CASE WHEN  rp.ReceiptType = '013' THEN ISNULL(rp.Amount, 0) ELSE 0 END) AS [TradeInAmount]							--����d���̓����\����z
				,	SUM(CASE WHEN  rp.ReceiptType = '012' THEN ISNULL(rp.Amount, 0) ELSE 0 END) AS [RemainDebtAmount]						--�c�̓����\����z
				,	SUM(ISNULL(rp.[Amount], 0)) AS [PresentMonth]																			--��������(�����\��z�{����d�����z�{�c���z)
			FROM [dbo].[ReceiptPlan] rp inner join [dbo].[CustomerClaim] cc ON rp.[CustomerClaimCode] = cc.[CustomerClaimCode] 
			WHERE rp.[SlipNumber] is not NULL
			  AND rp.[SlipNumber] <> ''
			  AND rp.[CustomerClaimCode] is not NULL
			  AND rp.[CustomerClaimCode] <> ''
			  AND cc.CustomerClaimType <> '003'				--������^�C�v���u�N���W�b�g�v
			  AND rp.[DelFlag] = '0'

			GROUP BY
					rp.[SlipNumber]

			--�B�ԗ��̓����\�����ݒ肷��B
			--(1)�ԗ��`�[�őΏ۔N���̍��`�[(xxxxxxxx-2)��`�[�ԍ��̎}�Ԃ���������Ԃőޔ�			
			INSERT INTO #Temp_CarSalesHeader
			SELECT
					LEFT(sh.[SlipNumber], 8)						--�`�[�ԍ�(�}�Ԃ͎�菜��)
				,	sh.[DepartmentCode]								--����R�[�h
				,	sh.[CustomerCode]								--�ڋq�R�[�h
				,	sh.[SalesDate]									--�[�ԓ�
			FROM 
				[CarSalesHeader] sh									--�ԗ��`�[�w�b�_
			WHERE
				sh.[SlipNumber] like '%-2%' AND						--���`�[(xxxxxxxx-2) 
				sh.[SalesOrderStatus] = '005' AND					--�[�ԍ�
				sh.[SalesDate] >= @TargetDateFrom AND
				sh.[SalesDate] <  @TargetDateTo AND 					
				sh.[DelFlag] = '0'
				
			--(2)�ԗ��`�[�őΏ۔N���̍��`�[(xxxxxxxx-2)�̖����ԓ`�[(xxxxxxxx-1)��-1����菜�����`�[�ԍ��őޔ�
			INSERT INTO #Temp_CarSalesHeader
			SELECT
					LEFT(sh.[SlipNumber], 8)						--�`�[�ԍ�(�}�Ԃ͎�菜��)
				,	sh.[DepartmentCode]								--����R�[�h
				,	sh.[CustomerCode]								--�ڋq�R�[�h
				,	sh.[SalesDate]									--�[�ԓ�
			FROM 
				[CarSalesHeader] sh									--�ԗ��`�[�w�b�_
			WHERE
				sh.[SlipNumber] like '%-1%' AND						--���`�[(xxxxxxxx-1) 
				sh.[SalesOrderStatus] = '005' AND					--�[�ԍ�		
				sh.[SalesDate] >= @TargetDateFrom AND 				
				sh.[SalesDate] <  @TargetDateTo AND 				
				sh.[DelFlag] = '0' AND
				NOT EXISTS
				(
					select 'x' from #Temp_CarSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
				)
			
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_CarSalesHeader ON #Temp_CarSalesHeader
			CREATE UNIQUE INDEX IX_Temp_CarSalesHeader ON #Temp_CarSalesHeader ([SlipNumber])
			

			--(3)���`�[�݂̂̂���
			INSERT INTO #Temp_SalesOrderReceiptPlan
			SELECT
					sh.[SlipNumber] AS [SlipNumber]							--�`�[�ԍ�
				,	sh.[SalesDate]	AS [SalesDate]							--�[�ԓ�
				,	sh.[DepartmentCode]	AS [DepartmentCode]					--����R�[�h
				,	sh.[CustomerCode] AS [CustomerCode]						--�ڋq�R�[�h
				,	ISNULL(rp.[Amount], 0) AS [Amount]						--�����\��z
				,	ISNULL(rp.[TradeInAmount], 0) AS [TradeInAmount]		--����d��
				,	ISNULL(rp.[RemainDebtAmount], 0) AS [RemainDebtAmount]	--�c��
				,	ISNULL(rp.[PresentMonth], 0) AS [PresentMonth]			--��������(�����\��z �{ ����d�����z �{ �c���z)
			FROM 
				[CarSalesHeader] sh inner join								--�ԗ��`�[�w�b�_
				#Temp_ReceiptPlan rp										--�����\��
			ON 
				sh.[SlipNumber] = rp.[SlipNumber]
			WHERE 
				sh.[SalesOrderStatus] = '005' AND							--�[�ԍ�
				sh.[SalesDate] >= @TargetDateFrom AND 				
				sh.[SalesDate] <  @TargetDateTo AND 				
				sh.[DelFlag] = '0' AND
				NOT EXISTS
				(
					select 'x' from #Temp_CarSalesHeader tsh where tsh.SlipNumber = LEFT(sh.SlipNumber, 8)			---�ԓ`�E���`�[�����݂��Ȃ��`�[
				)
				
			--(3)�ԓ`�E�܂��͍��`�܂ł���ꍇ�̓T�}���[���Ċi�[(����R�[�h�A�ڋq�R�[�h���͐ԓ`�A�܂��͍��`�̂��̂��g�p����)
			INSERT INTO #Temp_SalesOrderReceiptPlan
			SELECT
					gsr.SlipNumber												--�`�[�ԍ�
				,	ttsh.SalesDate												--�[�ԓ�
				,	ttsh.DepartmentCode											--����R�[�h
				,	ttsh.CustomerCode											--�ڋq�R�[�h
				,	gsr.Amount													--�����\��z
				,	gsr.TradeInAmount											--����d��
				,	gsr.RemainDebtAmount										--�c��
				,	gsr.PresentMonth											--�������z()
			FROM
			(
				SELECT
						LEFT(sh.[SlipNumber], 8) AS [SlipNumber]					--�`�[�ԍ�
					,	SUM(ISNULL(rp.[Amount], 0)) AS [Amount]						--�����\��z
					,	SUM(ISNULL(rp.[TradeInAmount], 0)) AS [TradeInAmount]		--����p
					,	SUM(ISNULL(rp.[RemainDebtAmount], 0)) AS [RemainDebtAmount]	--����p
					,	SUM(ISNULL(rp.[PresentMonth], 0)) AS [PresentMonth]			--��������(�����\��z �{ ����d�����z �{ �c���z)
				FROM 
					[CarSalesHeader] sh inner join									--�T�[�r�X�`�[�w�b�_
					#Temp_ReceiptPlan rp											--�����\��
				ON 
					sh.[SlipNumber] = rp.[SlipNumber]
				WHERE 
					sh.[SalesOrderStatus] = '005' AND								--�[�ԍ�
					sh.[SalesDate] >= @TargetDateFrom AND 				
					sh.[SalesDate] <  @TargetDateTo AND 				
					sh.[DelFlag] = '0' AND
					EXISTS
					(
						select 'x' from #Temp_CarSalesHeader tsh where tsh.SlipNumber = LEFT(sh.SlipNumber, 8)			--�ԓ`�[�E���`�[�����݂���`�[
					)
				GROUP BY
					 LEFT(sh.[SlipNumber], 8)
			) gsr INNER JOIN #Temp_CarSalesHeader ttsh  ON gsr.SlipNumber = ttsh.SlipNumber

			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan
			CREATE UNIQUE INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan ([SlipNumber])

			/***************************
			�����������у��X�g
			****************************/
			--�@�������уf�[�^��`�[�ԍ��ŏW�v�������ʂ��i�[����B
			--2016/12/22 Mod arc yano #3682 �`�[�ԍ��A������P�ʂŏW�v���s���i�ԓ`�E���`���̎}�Ԃ����`�[�ԍ��Ƃ��ďW�v����j
			INSERT INTO #Temp_Journal
			SELECT
				 LEFT(j.SlipNumber, 8) AS SlipNumber
				,SUM(j.TradeInAmount) AS TradeInAmount
				,SUM(j.RemainDebtAmount) AS RemainDebtAmount
				,SUM(j.Amount) AS Amount
				,SUM(j.TotalAmount) AS TotalAmount
			FROM
			(
				SELECT
						jn.[SlipNumber]																										--�`�[�ԍ�
					,	CASE WHEN  jn.AccountType = '013' THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [TradeInAmount]							--����d���̓����\����z			
					,	CASE WHEN  jn.AccountType = '012' THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [RemainDebtAmount]						--�c�̓����\����z
					,	CASE WHEN  ( jn.AccountType <> '012' AND jn.AccountType <> '013') THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [Amount]	--����E�c�ȊO�̓����\��z
					,	ISNULL(jn.[Amount], 0) AS [TotalAmount]																				--��������(�����\��z�{����d�����z�{�c���z)
				FROM
					[dbo].[Journal] jn inner join
					[dbo].[CustomerClaim] cc
				ON
					jn.CustomerClaimCode = cc.CustomerClaimCode 
				WHERE
					jn.[SlipNumber] is not NULL AND
					jn.[SlipNumber] <> '' AND
					jn.[CustomerClaimCode] is not NULL AND
					jn.[CustomerClaimCode] <> '' AND
					jn.[JournalType] =  '001' AND					--���o���敪=�u�����v
					jn.[AccountType] <> '099' AND					--�����̎�ʂ��u�f�[�^�j���v�ȊO
					jn.[JournalDate] >= @TargetDateFrom	AND		
					jn.[JournalDate] <  @TargetDateTo	AND		
					jn.[DelFlag] = '0' AND
					cc.CustomerClaimType <> '003' 					--������^�C�v���u�N���W�b�g�v
			) j
				GROUP BY
					LEFT(j.[SlipNumber], 8)
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_Journal ON #Temp_Journal
			CREATE UNIQUE INDEX IX_Temp_Journal ON #Temp_Journal ([SlipNumber])


			/*
			INSERT INTO #Temp_Journal
			SELECT
				 j.SlipNumber AS SlipNumber
				,SUM(j.TradeInAmount) AS TradeInAmount
				,SUM(j.RemainDebtAmount) AS RemainDebtAmount
				,SUM(j.Amount) AS Amount
				,SUM(j.TotalAmount) AS TotalAmount
			FROM
			(
			
				SELECT
						jn.[SlipNumber]																										--�`�[�ԍ�
					,	CASE WHEN  jn.AccountType = '013' THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [TradeInAmount]							--����d���̓����\����z			
					,	CASE WHEN  jn.AccountType = '012' THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [RemainDebtAmount]						--�c�̓����\����z
					,	CASE WHEN  ( jn.AccountType <> '012' AND jn.AccountType <> '013') THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [Amount]	--����E�c�ȊO�̓����\��z
					,	ISNULL(jn.[Amount], 0) AS [TotalAmount]																			--��������(�����\��z�{����d�����z�{�c���z)
				FROM
					[dbo].[Journal] jn inner join
					[dbo].[CustomerClaim] cc
				ON
					jn.CustomerClaimCode = cc.CustomerClaimCode 
				WHERE
					jn.[SlipNumber] is not NULL AND
					jn.[SlipNumber] <> '' AND
					jn.[CustomerClaimCode] is not NULL AND
					jn.[CustomerClaimCode] <> '' AND
					jn.[JournalType] =  '001' AND					--���o���敪=�u�����v
					jn.[AccountType] <> '099' AND					--�����̎�ʂ��u�f�[�^�j���v�ȊO
					jn.[JournalDate] >= @TargetDateFrom	AND		
					jn.[JournalDate] <  @TargetDateTo	AND		
					jn.[DelFlag] = '0' AND
					cc.CustomerClaimType <> '003' 					--������^�C�v���u�N���W�b�g�v
				-- ADD 2016/12/16 arc yano �ԗ��Ǘ� �ߋ��f�[�^�Ή�(�ԓ`�̓��������ԓ`�쐬���ȑO�ɐݒ肳��Ă���f�[�^�̎擾)
				UNION					
				SELECT
						jn.[SlipNumber]																											--�`�[�ԍ�
					,	CASE WHEN  jn.AccountType = '013' THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [TradeInAmount]							--����d���̓����\����z			
					,	CASE WHEN  jn.AccountType = '012' THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [RemainDebtAmount]						--�c�̓����\����z
					,	CASE WHEN  ( jn.AccountType <> '012' AND jn.AccountType <> '013') THEN ISNULL(jn.Amount, 0) ELSE 0 END AS [Amount]	--����E�c�ȊO�̓����\��z
					,	ISNULL(jn.[Amount], 0) AS [TotalAmount]																			--��������(�����\��z�{����d�����z�{�c���z)
				FROM
					[dbo].[Journal] jn inner join
					[dbo].[CustomerClaim] cc
				ON
					jn.CustomerClaimCode = cc.CustomerClaimCode
				WHERE
					jn.[SlipNumber] is not NULL AND
					jn.[SlipNumber] <> '' AND
					jn.[CustomerClaimCode] is not NULL AND
					jn.[CustomerClaimCode] <> '' AND
					jn.[JournalType] =  '001' AND					--���o���敪=�u�����v
					jn.[AccountType] <> '099' AND					--�����̎�ʂ��u�f�[�^�j���v�ȊO
					jn.[JournalDate] <  @TargetDateTo	AND			--�������������ȑO
					jn.[DelFlag] = '0' AND
					cc.CustomerClaimType <> '003' AND				--������^�C�v���u�N���W�b�g�v
					EXISTS
					(
						select 
							'x' 
						from 
							dbo.CarSalesHeader sh 
						where
							sh.SalesOrderStatus = '005' and
							sh.SalesDate >= @TargetDateFrom and
							sh.SalesDate < @TargetDateTo and
							sh.DelFlag = '0' and
							sh.SlipNumber = jn.SlipNumber
					) 
			) j
				GROUP BY
					j.[SlipNumber]
			
			--�A�ԗ��̓������я���ݒ肷��B
			--(1)���`�̂�
			INSERT INTO #Temp_SalesOrderJournal
			SELECT 
					LEFT(sh.[SlipNumber], 8) AS [SlipNumber]					--�`�[�ԍ�
				,	sh.[SalesDate] AS [SalesDate]								--�[�ԓ�
				,	sh.[DepartmentCode]	AS [DepartmentCode]						--����R�[�h
				,	sh.[CustomerCode]	AS [CustomerCode]						--�ڋq�R�[�h
				,	SUM(ISNULL(jn.[TradeInAmount], 0)) AS [TradeInAmount]		--����d��
				,	SUM(ISNULL(jn.[RemainDebtAmount], 0)) AS [RemainDebtAmount]	--�c��
				,	SUM(ISNULL(jn.[Amount], 0)) AS [Amount]						--����E�c�ȊO�̓����z
				,	SUM(ISNULL(jn.[TotalAmount], 0)) AS [TotalAmount]			--��������(����E�c�ȊO�̓����z �{ ����d�����z �{ �c���z)
			FROM 
				dbo.CarSalesHeader sh inner join							--�ԗ��`�[�w�b�_
				#Temp_Journal jn											--��������
			ON 
				sh.[SlipNumber] = jn.[SlipNumber]
			WHERE
				sh.[SalesDate] >= @TargetDateFrom AND 						--2016/12/16 arc yano �`�[���t�ɂ��i�����s��		
				sh.[SalesDate] <  @TargetDateTo AND 						--2016/12/16 arc yano �`�[���t�ɂ��i�����s��
				sh.[DelFlag] = '0' AND
				NOT EXISTS
				(
					select 'x' from #Temp_CarSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
				)	
			GROUP BY 
				  sh.[SlipNumber]
				, sh.[SalesDate]
				, sh.[DepartmentCode]
				, sh.[CustomerCode]

			--(2)�ԓ`�A�܂��͍��`�܂ł�������\����T�}���[���Ċi�[(����R�[�h�A�ڋq�R�[�h���͐ԓ`�A�܂��͍��`�̂��̂��g�p����
			INSERT INTO #Temp_SalesOrderJournal
			SELECT
					jsh.SlipNumber									--�`�[�ԍ�
				,	tssh.SalesDate									--�[�ԓ�
				,	tssh.DepartmentCode								--����R�[�h
				,	tssh.CustomerCode								--�ڋq�R�[�h
				,	jsh.TradeInAmount								--����d��
				,	jsh.RemainDebtAmount							--�c��
				,	jsh.Amount										--����E�c�ȊO�̓���
				,	jsh.TotalAmount									--�������э��v
			FROM

			(
				SELECT 
						LEFT(sh.[SlipNumber], 8) AS [SlipNumber]				--�`�[�ԍ�
					,	SUM(ISNULL(jn.[Amount], 0)) AS [Amount]						--����E�c�ȊO�̓����z
					,	SUM(ISNULL(jn.[TradeInAmount], 0)) AS [TradeInAmount]		--����d��
					,	SUM(ISNULL(jn.[RemainDebtAmount], 0)) AS [RemainDebtAmount]	--�c��
					,	SUM(ISNULL(jn.[TotalAmount], 0)) AS [TotalAmount]			--��������(����E�c�ȊO�̓����z �{ ����d�����z �{ �c���z)
				FROM 
					dbo.CarSalesHeader sh inner join							--�T�[�r�X�`�[�w�b�_
					#Temp_Journal jn											--��������
				ON 
					sh.[SlipNumber] = jn.[SlipNumber]
				WHERE 
					sh.[DelFlag] = '0' AND
					EXISTS
					(
						select 'x' from #Temp_CarSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
					)	
				GROUP BY 
					LEFT(sh.[SlipNumber], 8)
			) jsh INNER JOIN #Temp_CarSalesHeader tssh ON jsh.SlipNumber = tssh.SlipNumber
		
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal
			CREATE UNIQUE INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal ([SlipNumber])
			*/

			--Mod 2018/01/27 arc yano #3717
			--Add 2016/12/22 arc yano #3682
			/********************
			�����`�[���
			*********************/
			--Step1.�W�v�������_�ōł��V�����`�[���擾����
			INSERT INTO #Temp_SlipInfo
			SELECT 
				  LEFT(sh.[SlipNumber], 8) AS SlipNumber	--�`�[�ԍ�
				, sh.[SalesDate]							--�[�ԓ�
				, sh.[DepartmentCode]						--����R�[�h
				, sh.[CustomerCode]							--�ڋq�R�[�h
			FROM
				dbo.CarSalesHeader sh INNER JOIN
				(
					SELECT
						MAX(SlipNumber) as SlipNumber 
					FROM 
						dbo.CarSalesHeader
					WHERE 
						DelFlag = '0' AND
						CreateDate < @TargetDateTo --AND
						--RevisionNumber = 1
					GROUP BY 
						LEFT(SlipNumber , 8)
				) sh2
			ON sh.SlipNumber = sh2.SlipNumber
			WHERE
				sh.DelFlag = '0'

			--Step2 Step1�Ŏ擾�ł��Ȃ������`�[�̓`�[�ԍ���ޔ�
			INSERT INTO #Temp_SlipList
			SELECT
				DISTINCT(tj.SlipNumber)
			FROM
				#Temp_Journal tj
			WHERE
				NOT EXISTS
				(
					select 'x' from #Temp_SlipInfo si where tj.SlipNumber = si.SlipNumber 
				)

			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_SlipList ON #Temp_SlipList
			CREATE UNIQUE INDEX IX_Temp_SlipList ON #Temp_SlipList ([SlipNumber])


			--Step3 Step1�Ŏ擾�ł��Ȃ������`�[�͍쐬���̏����Ȃ��Ŏ擾����
			INSERT INTO #Temp_SlipInfo
			SELECT 
				  sh.[SlipNumber] AS SlipNumber				--�`�[�ԍ�
				, sh.[SalesDate]							--�[�ԓ�
				, sh.[DepartmentCode]						--����R�[�h
				, sh.[CustomerCode]							--�ڋq�R�[�h
			FROM
				dbo.CarSalesHeader sh
			WHERE
				sh.DelFlag = '0' AND
				EXISTS
				(
					select 'x' from #Temp_SlipList tsl where sh.SlipNumber = tsl.SlipNumber
				)
	

			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_SlipInfo ON #Temp_SlipInfo
			CREATE UNIQUE INDEX IX_Temp_SlipInfo ON #Temp_SlipInfo ([SlipNumber])


			/********************
			�����R�[�h���X�g
			*********************/
			--�����\��^���у��X�g�ɑ��݂���A�`�[�ԍ��A������R�[�h���̑g�����̃��X�g���쐬����
			INSERT INTO #Temp_CodeList
			SELECT
					l.[SlipNumber]
				,	(CASE WHEN si.[SlipNumber] is null THEN (CASE WHEN rp.[SalesDate] is null THEN cb.[SalesDate] ELSE rp.[SalesDate] END) ELSE si.[SalesDate] END) AS [SalesDate]
				,	(CASE WHEN si.[SlipNumber] is null THEN (CASE WHEN rp.[DepartmentCode] is null THEN cb.[DepartmentCode] ELSE rp.[DepartmentCode] END) ELSE si.[DepartmentCode] END) AS [DepartmentCode]
				,	(CASE WHEN si.[SlipNumber] is null THEN (CASE WHEN rp.[CustomerCode] is null THEN cb.[CustomerCode] ELSE rp.[CustomerCode] END) ELSE si.[CustomerCode] END) AS [CustomerCode]
				/*
				,	(CASE WHEN rp.[SalesDate] is null THEN (CASE WHEN oj.[SalesDate] is null THEN  cb.[SalesDate] ELSE oj.[SalesDate] END) ELSE rp.[SalesDate] END) AS [SalesDate]
				,	(CASE WHEN rp.[DepartmentCode] is null THEN (CASE WHEN oj.[DepartmentCode] is null THEN cb.[DepartmentCode] ELSE oj.[DepartmentCode] END) ELSE rp.[DepartmentCode] END) AS [DepartmentCode]
				,	(CASE WHEN rp.[CustomerCode] is null THEN (CASE WHEN oj.[CustomerCode] is null THEN cb.[CustomerCode] ELSE oj.[CustomerCode] END) ELSE rp.[CustomerCode] END) AS [CustomerCode]
				*/
			FROM (
				SELECT
						[SlipNumber]
				FROM
					#Temp_SalesOrderReceiptPlan
				UNION
				SELECT
						[SlipNumber]
				
				FROM
					#Temp_Journal					--Mod 2016/12/22 arc yano #3682
				--	#Temp_SalesOrderJournal
				UNION
				
				SELECT
						[SlipNumber]
				FROM
					#Temp_CarriedBalance
			) AS l 
			LEFT OUTER JOIN
				#Temp_SalesOrderReceiptPlan rp
			ON
				l.SlipNumber = rp.SlipNumber
			LEFT OUTER JOIN
				#Temp_Journal oj					--Mod 2016/12/22 arc yano #3682
				--#Temp_SalesOrderJournal oj
			ON
				l.SlipNumber = oj.SlipNumber		
			LEFT OUTER JOIN
				#Temp_CarriedBalance cb
			ON
				l.SlipNumber = cb.SlipNumber
			LEFT OUTER JOIN
				#Temp_SlipInfo si					--Add 2016/12/22 arc yano #3682
			ON
				l.SlipNumber = si.SlipNumber

			/********************
			�������|���c�����X�g
			*********************/
			--�R�[�h���X�g�A�����\��^���у��X�g���A���|���c�����쐬����B					
			--����w��ɂ��
			IF @ActionFlag = 0		--����w�� =�u�\���v�̏ꍇ�͈ꎞ�\�Ɋi�[
			BEGIN
				INSERT INTO #Temp_AccountsReceivableCar

				SELECT
						@TargetDateFrom AS [CloseMonth]																														--����
					,	cl.[SlipNumber]																																		--�`�[�ԍ�
					,	cl.[DepartmentCode]																																	--����R�[�h
					,	dp.[DepartmentName]																																	--���喼��
					,	cl.[CustomerCode]																																	--�ڋq�R�[�h
					,	cu.[CustomerName]
					/*	--Mod 2016/12/22 arc yano #3682																															--�ڋq����
					,	CASE WHEN ISNULL(cb.[CarriedBalance], 0) = 0 
							  and ISNULL(rp.[Amount], 0) + ISNULL(rp.TradeInAmount, 0) + ISNULL(rp.RemainDebtAmount, 0) = 0 
							  and ISNULL(jn.[Amount], 0) > 0 
							 THEN NULL 
							 ELSE cl.[SalesDate]
					*/
						--Mod 2018/01/27 arc yano
					,	CASE WHEN cl.[SalesDate] is null
							 THEN NULL 
							 ELSE 
								CASE WHEN 
									DATEDIFF(d, cl.[SalesDate], @TargetDateTo) >= 0		--Mod 2019/05/22 yano #3954		
								THEN 
									cl.[SalesDate]
								ELSE 
									NULL
								END
						END  AS [SalesDate]																																	--�[�ԓ�
					,	ISNULL(cb.[CarriedBalance], 0) AS [CarriedBalance]																									--�O���J�z
					,	ISNULL(rp.[PresentMonth], 0) AS [PresentMonth]																										--��������
					,	ISNULL(jn.[TotalAmount], 0) AS [PaymentAmount]																										--��������
					,	ISNULL(rp.Amount, 0) - ISNULL(jn.Amount, 0) AS [CustomerBalance]																					--�c�E����ȊO�̎c��
					,	ISNULL(rp.TradeInAmount, 0) - ISNULL(jn.TradeInAmount, 0) AS [TradeBalance]																			--����d���c��
					,	ISNULL(rp.[RemainDebtAmount], 0) - ISNULL(jn.RemainDebtAmount, 0) AS [RemainDebtAmount]																--�c�c��
					,	ISNULL(cb.[CarriedBalance], 0) + ISNULL(rp.[PresentMonth], 0) - ISNULL(jn.[TotalAmount], 0) AS [BalanceAmount]										--�c��(�O���J�z�{���������|��������)
					
				FROM #Temp_CodeList cl																				--�R�[�h���X�g
				
				INNER JOIN [dbo].[Department] dp																	--����}�X�^
					ON cl.[DepartmentCode] = dp.[DepartmentCode]
				INNER JOIN [dbo].[Customer] cu																		--�ڋq�}�X�^
					ON cl.[CustomerCode] = cu.[CustomerCode]
				LEFT JOIN #Temp_CarriedBalance cb																	--�O���J�z���X�g
					ON cl.SlipNumber = cb.SlipNumber
				LEFT JOIN #Temp_SalesOrderReceiptPlan rp															--�����\�胊�X�g
					ON cl.SlipNumber = rp.SlipNumber
				LEFT JOIN #Temp_Journal jn																			--�������у��X�g	--Mod 2016/12/22 arc yano #3682
				--LEFT JOIN #Temp_SalesOrderJournal jn																--�������у��X�g
					ON cl.SlipNumber = jn.SlipNumber

			END
			ELSE	--����w��=�u�ۑ��v�̏ꍇ��DB�̃e�[�u���Ɋi�[
			BEGIN
				--�i�[����O�Ɉ�x�f�[�^���폜
				DELETE FROM 
					[dbo].[AccountsReceivableCar]
				WHERE 
					CloseMonth = @TargetDateFrom
						
				INSERT INTO [dbo].[AccountsReceivableCar]

				SELECT
						@TargetDateFrom AS [CloseMonth]																					--����
					,	cl.[SlipNumber]																									--�`�[�ԍ�
					,	cl.[DepartmentCode]																								--����R�[�h
					,	dp.[DepartmentName]																								--���喼��
					,	cl.[CustomerCode]																								--�ڋq�R�[�h
					,	cu.[CustomerName]																								--�ڋq����
					/* --Mod 2016/12/22 arc yano #3682	
					,	CASE WHEN ISNULL(cb.[CarriedBalance], 0) = 0 
							  and ISNULL(rp.[Amount], 0) + ISNULL(rp.TradeInAmount, 0) + ISNULL(rp.RemainDebtAmount, 0) = 0 
							  and ISNULL(jn.[Amount], 0) > 0 
							 THEN NULL
							 ELSE cl.[SalesDate] 
					*/
						--Mod 2018/01/27 arc yano
					,	CASE WHEN cl.[SalesDate] is null
							 THEN NULL 
							 ELSE 
								CASE WHEN 
									DATEDIFF(d, cl.[SalesDate], @TargetDateTo) >= 0		--Mod 2019/05/22 yano #3954	
								THEN 
									cl.[SalesDate]
								ELSE 
									NULL
								END
						END  AS [SalesDate]																								--�[�ԓ�
					,	ISNULL(cb.[CarriedBalance], 0) AS [CarriedBalance]																--�O���J�z
					,	ISNULL(rp.[PresentMonth], 0) AS [PresentMonth]																	--��������
					,	ISNULL(jn.[TotalAmount], 0) AS [PaymentAmount]																	--��������
					,	ISNULL(rp.Amount, 0) - ISNULL(jn.Amount, 0) AS [CustomerBalance]												--�c�E����ȊO�̎c��
					,	ISNULL(rp.TradeInAmount, 0) - ISNULL(jn.TradeInAmount, 0) AS [TradeBalance]										--����d���c��
					,	ISNULL(rp.[RemainDebtAmount], 0) - ISNULL(jn.RemainDebtAmount, 0) AS [RemainDebtAmount]							--�c�c��
					,	ISNULL(cb.[CarriedBalance], 0) + ISNULL(rp.[PresentMonth], 0) - ISNULL(jn.[TotalAmount], 0) AS [BalanceAmount]	--�c��(�O���J�z�{���������|��������)
					,	'sys' AS [CreateEmployeeCode]																					--�쐬��
					,	@NOW AS [CreateDate]																							--�쐬����
					,	'sys' AS [LastUpdateEmployeeCode]																				--�ŏI�X�V��
					,	@NOW AS [LastUpdateDate]																						--�ŏI�X�V����
					,	'0'	 AS [DelFlag]																								--�폜�t���O

				FROM #Temp_CodeList cl																				--�R�[�h���X�g
				
				INNER JOIN [dbo].[Department] dp																	--����}�X�^
					ON cl.[DepartmentCode] = dp.[DepartmentCode]
				INNER JOIN [dbo].[Customer] cu																		--�ڋq�}�X�^
					ON cl.[CustomerCode] = cu.[CustomerCode]
				LEFT JOIN #Temp_CarriedBalance cb																	--�O���J�z���X�g
					ON cl.SlipNumber = cb.SlipNumber
				LEFT JOIN #Temp_SalesOrderReceiptPlan rp															--�����\�胊�X�g
					ON cl.SlipNumber = rp.SlipNumber
				LEFT JOIN #Temp_Journal jn																			--�������у��X�g			--Mod 2016/12/22 arc yano #3682
				--LEFT JOIN #Temp_SalesOrderJournal jn																--�������у��X�g
					ON cl.SlipNumber = jn.SlipNumber

			END
			--���̏����Ώی�
			SET @TargetMonthCount = @TargetMonthCount - 1				--�c�����f�N�������g
			SET @TargetMonthPrevious = @TargetDateFrom					--�Ώی��O���C���N�������g(������̓����j
			SET @TargetDateFrom = DATEADD(m, 1, @TargetMonthPrevious)	--�Ώۓ�From�C���N�������g(������̑O���{�P�j
			SET @TargetDateTo = DATEADD(m, 1, @TargetDateFrom)			--�Ώۓ�To�C���N�������g(������̓����{�P�j
			IF @TargetDateTo > @NOW
				SET @TargetDateTo = @NOW
		--���[�v�G���h
		END

		/***************************************************/
		/*����w��=�u�\���v�̏ꍇ�̓f�[�^�擾���s��		   */
		/***************************************************/
		DECLARE @SQL NVARCHAR(MAX) = ''
		DECLARE @PARAM NVARCHAR(1024) = ''
		DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)		

		IF @ActionFlag = 0	--�\���̏ꍇ
		BEGIN
			SET @PARAM = '@TargetMonth datetime, @SalesDateFrom nvarchar(10), @SalesDateTo nvarchar(10), @SlipNumber NVARCHAR(50), @DepartmentCode NVARCHAR(3), @CustomerCode NVARCHAR(10), @Zerovisible NVARCHAR(1)'
					
			SET @SQL = ''
			SET @SQL = @SQL +'SELECT' + @CRLF
			SET @SQL = @SQL +'    ar.CloseMonth' + @CRLF
			SET @SQL = @SQL +'	, ar.SlipNumber' + @CRLF
			SET @SQL = @SQL +'	, ar.DepartmentCode' + @CRLF
			SET @SQL = @SQL +'	, ar.DepartmentName' + @CRLF
			SET @SQL = @SQL +'	, ar.CustomerCode' + @CRLF
			SET @SQL = @SQL +'	, ar.CustomerName' + @CRLF
			/* --Mod 2016/12/22 arc yano #3682	
			SET @SQL = @SQL +'	, CASE WHEN ISNULL(ar.[CarriedBalance], 0) = 0' + @CRLF 
			SET @SQL = @SQL +'		    and ISNULL(ar.[PresentMonth], 0) = 0' + @CRLF 
			SET @SQL = @SQL +'		    and ISNULL(ar.[PaymentAmount], 0) > 0 ' + @CRLF
			SET @SQL = @SQL +'		   THEN NULL ' + @CRLF
			SET @SQL = @SQL +'		   ELSE ar.SalesDate' + @CRLF
			SET @SQL = @SQL +'	  END AS SalesDate' + @CRLF
			*/
			SET @SQL = @SQL +'	, ar.SalesDate' + @CRLF
			SET @SQL = @SQL +'	, ar.CarriedBalance' + @CRLF
			SET @SQL = @SQL +'	, ar.PresentMonth' + @CRLF
			SET @SQL = @SQL +'	, ar.PaymentAmount' + @CRLF
			SET @SQL = @SQL +'	, ar.CustomerBalance' + @CRLF
			SET @SQL = @SQL +'	, ar.TradeBalance' + @CRLF
			SET @SQL = @SQL +'	, ar.RemainDebtBalance' + @CRLF
			SET @SQL = @SQL +'	, ar.BalanceAmount' + @CRLF
			SET @SQL = @SQL +'	, '''' AS CreateEmployeeCode' + @CRLF
			SET @SQL = @SQL +'	, NULL AS CreateDate' + @CRLF
			SET @SQL = @SQL +'	, '''' AS LastUpdateEmployeeCode' + @CRLF
			SET @SQL = @SQL +'	, NULL AS LastUpdateDate' + @CRLF
			SET @SQL = @SQL +'	, ''0'' AS DelFlag' + @CRLF
			SET @SQL = @SQL +'FROM' + @CRLF
			SET @SQL = @SQL +'	#Temp_AccountsReceivableCar ar' + @CRLF
			SET @SQL = @SQL +'WHERE' + @CRLF
			SET @SQL = @SQL +'    ar.CloseMonth = @TargetMonth' + @CRLF 
			

			--�[�ԓ�
			IF ((@SalesDateFrom is not null) AND (@SalesDateFrom <> '') AND ISDATE(@SalesDateFrom) = 1)
				IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
				BEGIN
					SET @SQL = @SQL +' AND ar.SalesDate >= @SalesDateFrom AND ar.SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF
				END
				ELSE
				BEGIN
					SET @SQL = @SQL +' AND ar.SalesDate = @SalesDateFrom' + @CRLF 
				END
			ELSE
				IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
				BEGIN
					SET @SQL = @SQL +' AND ar.SalesDate < DateAdd(d, 1, @SalesDateTo)' + @CRLF 
				END
			
			--�`�[�ԍ�
			IF ((@SlipNumber is not null) AND (@SlipNumber <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ar.SlipNumber = @SlipNumber' + @CRLF
			END
			--����R�[�h
			IF ((@DepartmentCode is not null) AND (@DepartmentCode <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ar.DepartmentCode = @DepartmentCode' + @CRLF
			END
			--�ڋq�R�[�h
			IF ((@CustomerCode is not null) AND (@CustomerCode <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ar.CustomerCode = @CustomerCode' + @CRLF
			END
			
			IF ((@Zerovisible is not null) AND (@Zerovisible <>'') AND (@Zerovisible = '0'))
			BEGIN
				SET @SQL = @SQL + 'AND (ar.CustomerBalance != 0 OR ar.TradeBalance != 0 OR ar.RemainDebtBalance != 0)'+ @CRLF
			END

			EXECUTE sp_executeSQL @SQL, @PARAM, @TargetMonth, @SalesDateFrom, @SalesDateTo, @SlipNumber, @DepartmentCode, @CustomerCode, @Zerovisible
			
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
		RAISERROR (@ErrorMessage, 16, 1)

	RETURN @ErrorNumber
			--�I��	
END


GO


