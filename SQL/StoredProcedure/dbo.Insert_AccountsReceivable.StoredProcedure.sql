USE [WPH_DB_CHECK]
GO

/****** Object:  StoredProcedure [dbo].[Insert_AccountsReceivable]    Script Date: 2020/03/23 12:25:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2015/08/11 arc yano  #3233 ���|�����[�Ή�
-- Description:	<Description,,>
-- �w�茎�̔��|���f�[�^�̍쐬�E�\��
-- Mod 2020/02/26 yano #4040 �T�[�r�X���|���z�O����̎��̔[�ԓ��̕\���̕s��Ή�
-- Mod 2019/05/22 yano #3954 �y�T�[�r�X���|���z�����[�Ԃ̔��|�����\������Ȃ�
-- Mod 2018/01/27 arc yano #3717 �T�[�r�X���|��_�`�[�����݂��Ȃ����̓������т̔��f
-- Mod 2016/12/22 arc yano #3682 �ԗ����|���@�V�X�e���G���[ �ގ����������ێ琫����̂��߁A�ȑO����R�����g�A�E�g����Ă����������폜
-- Mod 2016/06/02 arc yano #3532 �T�[�r�X�`�[�̏W�v���Ɂu-1�v�u-2�v�̎}�Ԃ��T������
-- Mod 2015/11/06 arc nakayama #3302_���|���Ǘ���ʂ̍��ڒǉ��E�폜 (�����O����𓖌������ɓ������āA��������������p/����p�ȊO�ɕ�����)
-- Mod Add 2016/09/14 arc nakayama #3630_�y�����z�ԗ����|���Ή� �T�[�r�X�Ǝԗ��̔��|���𕪂���@���̃X�g�v���̓T�[�r�X��p�ɂ���
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[Insert_AccountsReceivable] 
--declare

	@TargetMonth datetime,						--�w�茎
	@ActionFlag int = 0,						--����w��(0:��ʕ\��, 1:�X�i�b�v�V���b�g�ۑ�)
	@SalesDateFrom NVARCHAR(10) = NULL,			--�[�ԓ�(From)
	@SalesDateTo NVARCHAR(10) = NULL,			--�[�ԓ�(To)
	@SlipNumber NVARCHAR(50) = NULL,			--�`�[�ԍ�
	@DepartmentCode NVARCHAR(3) = NULL,			--����R�[�h
	@CustomerCode NVARCHAR(10) = NULL,			--�ڋq�R�[�h
	@Zerovisible NVARCHAR(1) = NULL,			--�[���\���t���O(0:��\�� 1:�\��)
	@Classification NVARCHAR(3) = NULL			--�敪("":�w��Ȃ��@1:�Г��ȊO�@2:�Г�)
AS 
BEGIN
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
	DROP TABLE #Temp_SalesOrderReceiptPlan;											--�����\�胊�X�g(�T�[�r�X)
	IF OBJECT_ID(N'tempdb..#Temp_Journal', N'U') IS NOT NULL
	DROP TABLE #Temp_Journal;														--�������у��X�g
	--IF OBJECT_ID(N'tempdb..#Temp_SalesOrderJournal', N'U') IS NOT NULL											--Del 2016/12/22 arc yano #3682
	--DROP TABLE #Temp_SalesOrderJournal;											--�������у��X�g(�T�[�r�X)
	IF OBJECT_ID(N'tempdb..#Temp_AccountsReceivable', N'U') IS NOT NULL
	DROP TABLE #Temp_AccountsReceivable;											--���|���f�[�^���X�g
	IF OBJECT_ID(N'tempdb..#Temp_ServiceSalesHeader', N'U') IS NOT NULL
	DROP TABLE #Temp_ServiceSalesHeader;											--�T�[�r�X�`�[�w�b�_
	IF OBJECT_ID(N'tempdb..#Temp_SlipInfo', N'U') IS NOT NULL
	DROP TABLE #Temp_SlipInfo;														--�`�[���			--Add 2016/12/22 arc yano #3682
	IF OBJECT_ID(N'tempdb..#Temp_SlipList', N'U') IS NOT NULL
	DROP TABLE #Temp_SlipList;														--�`�[���			--Add 2018/01/27 arc yano #3717

	--�����ꎞ�\�̐錾
	/*************************************************************************/

	--�R�[�h���X�g
	CREATE TABLE #Temp_CodeList (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- ������R�[�h
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	)

	--�O���J�z���X�g
	CREATE TABLE #Temp_CarriedBalance (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- ������R�[�h
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[CustomerClaimType] NVARCHAR(3)					-- ������敪
	,	[CarriedBalance] DECIMAL(10, 0)					-- �O���J�z��
	)
	CREATE UNIQUE INDEX IX_Temp_CarriedBalance ON #Temp_CarriedBalance ([SlipNumber], [CustomerClaimCode])
			
	--�����\�胊�X�g
	CREATE TABLE #Temp_ReceiptPlan (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- ������R�[�h
	,	[CustomerClaimType] NVARCHAR(10)				-- ������敪
	,	[Amount] DECIMAL(10, 0)							-- �����\��z
	,	[Expendes] DECIMAL(10, 0)						-- ����p
	,	[PresentMonth] DECIMAL(10, 0)					-- ��������(�����\��z�{����p)
	)
	CREATE UNIQUE INDEX IX_Temp_ReceiptPlan ON #Temp_ReceiptPlan ([SlipNumber], [CustomerClaimCode])

	--�����\�胊�X�g(�T�[�r�X)
	CREATE TABLE #Temp_SalesOrderReceiptPlan (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- ������R�[�h
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[CustomerClaimType] NVARCHAR(3)					-- ������敪
	,	[Amount] DECIMAL(10, 0)							-- �����\��z
	,	[Expendes] DECIMAL(10, 0)						-- ����p
	,	[PresentMonth] DECIMAL(10, 0)					-- ��������(�����\��z�{����p)
	)
	CREATE UNIQUE INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan ([SlipNumber], [CustomerClaimCode])

	--�������у��X�g
	CREATE TABLE #Temp_Journal (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- ������R�[�h
	,	[CustomerClaimType] NVARCHAR(10)				-- ������敪
	,	[Amount] DECIMAL(10, 0)							-- �����z
	,	[ChargesAmount] DECIMAL(10, 0)					-- �����z(����p)	--Add 2016/12/22 arc yano #3682 
	--,	[AccountCode] NVARCHAR(50)						-- ����ȖڃR�[�h Add 2015/11/06 arc nakayama #3302_���|���Ǘ���ʂ̍��ڒǉ��E�폜 ����Ȗڂœ��������𕪂���
	)
	CREATE UNIQUE INDEX IX_Temp_Journal ON #Temp_Journal ([SlipNumber], [CustomerClaimCode])

	--Del 2016/12/22 arc yano #3682 
	/*
	--�������у��X�g(�T�[�r�X)
	CREATE TABLE #Temp_SalesOrderJournal (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- ������R�[�h	
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[CustomerClaimType] NVARCHAR(3)					-- ������敪
	,	[Amount] DECIMAL(10, 0)							-- �����z(����p�ȊO)
	,	[ChargesAmount] DECIMAL(10, 0)					-- �����z(����p)
--	,	[AccountCode] NVARCHAR(50)						-- ����ȖڃR�[�h Add 2015/11/06 arc nakayama #3302_���|���Ǘ���ʂ̍��ڒǉ��E�폜 ����Ȗڂœ��������𕪂���
	)
	CREATE UNIQUE INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal ([SlipNumber], [CustomerClaimCode])
	*/

	--���|���f�[�^
	CREATE TABLE #Temp_AccountsReceivable (
		[CloseMonth] datetime NOT NULL					-- ����
	,	[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- ������R�[�h
	,	[CustomerClaimName] NVARCHAR(80)				-- �����於
	,	[CustomerClaimType] NVARCHAR(3)					-- ������敪
	,	[CustomerClaimTypeName] NVARCHAR(50)			-- ������敪��
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[DepartmentName] NVARCHAR(20)					-- ���喼
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[CustomerName] NVARCHAR(80)						-- �ڋq��
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[CarriedBalance] decimal(10, 0)					-- �O���J�z
	,	[PresentMonth] decimal(10, 0)					-- ��������(����p�ȊO)
	,	[Expendes] decimal(10, 0)						-- ��������(����p)
	,	[TotalAmount] decimal(10, 0)					-- ���v(�O���J�z�{��������)
	--Mod 2015/11/06 arc nakayama #3302_���|���Ǘ���ʂ̍��ڒǉ��E�폜  ���������ɓ����O����𓝍��A������������(��������)������Ȗڂŏ���p�Ə���p�ȊO�ɕ�����
	,	[Payment] decimal(10, 0)						-- ��������(����p�ȊO)
	,	[BalanceAmount] decimal(10, 0)					-- �c��
	,	[ChargesPayment] decimal(10, 0)					-- ��������(����p)
	)
	CREATE UNIQUE INDEX IX_Temp_AccountsReceivable ON #Temp_AccountsReceivable ([CloseMonth], [SlipNumber], [CustomerClaimCode])

	--�T�[�r�X�`�[�w�b�_ //Add 2016/06/02
	CREATE TABLE #Temp_ServiceSalesHeader (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[ServiceOrderStatus] NVARCHAR(3)				-- �`�[�X�e�[�^�X
	)
	CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader ([SlipNumber])

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
	BEGIN--�X�i�b�v�V���b�g�ۑ��̏ꍇ�͎w�茎��ݒ肷��B
		SET @TargetMonthFrom = @TargetMonth
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
		--Mod 2020/02/26 yano #4040
		SET @TargetDateTo = CONVERT(datetime, CONVERT(nvarchar, DATEADD(d, 1, @NOW), 111))
		--SET @TargetDateTo = @NOW
	
	--�g�����U�N�V�����J�n 
	BEGIN TRANSACTION
	BEGIN TRY
		
		--���O�����̃f�[�^���ꎞ�e�[�u���ɂ��炩���ߊi�[����B
		INSERT INTO #Temp_AccountsReceivable
		SELECT
			ar.[CloseMonth]								--����
		,	ar.[SlipNumber]								--�`�[�ԍ�
		,	ar.[CustomerClaimCode]						--������R�[�h
		,	ar.[CustomerClaimName]						--�����於
		,	ar.[CustomerClaimType]						--������敪
		,	ar.[CustomerClaimTypeName]					--������敪��
		,	ar.[DepartmentCode]							--����R�[�h
		,	ar.[DepartmentName]							--���喼
		,	ar.[CustomerCode]							--�ڋq�R�[�h
		,	ar.[CustomerName]							--�ڋq��
		,	ar.[SalesDate]								--�[�ԓ�
		,	ar.[CarriedBalance]							--�O���J�z
		,	ar.[PresentMonth]							--��������(����p�ȊO)
		,	ar.[Expendes]								--��������(����p)
		,	ar.[TotalAmount]							--���v
		,	ar.[Payment]								--��������
		,	ar.[BalanceAmount]							--�c��
		,	ar.[ChargesPayment]							--��������
		FROM
			[dbo].AccountsReceivable ar
		WHERE
			[CloseMonth] = @TargetMonthPrevious AND	--�Ώ۔N���̑O��
			[BalanceAmount] <> 0 AND				--�c�����O�łȂ�(�}�C�i�X���Ώ�)
			[DelFlag] = '0' AND
			EXISTS(select 1 from dbo.ServiceSalesHeader sh where sh.DelFlag = '0' and sh.SlipNumber = ar.SlipNumber)
				

		--�������Ώی��������[�v
		WHILE @TargetMonthCount > 0
		BEGIN
			--�ꎟ�\������		
			DELETE FROM  #Temp_CodeList							--�R�[�h���X�g
			DELETE FROM  #Temp_CarriedBalance					--�O���J�z���X�g
			DELETE FROM  #Temp_ReceiptPlan						--�����\�胊�X�g
			DELETE FROM  #Temp_SalesOrderReceiptPlan			--�����\�胊�X�g(�T�[�r�X)
			DELETE FROM  #Temp_Journal							--�������у��X�g
			--DELETE FROM  #Temp_SalesOrderJournal				--�������у��X�g(�T�[�r�X)
			DELETE FROM	 #Temp_ServiceSalesHeader				--�T�[�r�X�`�[(�ԓ`�E���`���O�p�e�[�u��)
			DELETE FROM  #Temp_SlipInfo							--�`�[���									--Add 2016/12/22 arc yano #3682
			DELETE FROM  #Temp_SlipList							--�⊮�`�[���X�g						�@�@--Add 2018/01/27 arc yano #3717
			
			/********************
			�����O���J�z���X�g
			*********************/
			--�O�����̎c����0�łȂ����̂��擾
			INSERT INTO #Temp_CarriedBalance
			SELECT
			 		[SlipNumber]								--�`�[�ԍ�
				,	[CustomerClaimCode]							--������R�[�h
				,	[SalesDate]									--�[�ԓ�
				,	[DepartmentCode]							--����R�[�h
				,	[CustomerCode]								--�ڋq�R�[�h
				,	[CustomerClaimType]							--������敪
				,	[BalanceAmount]								--�c��
			FROM
				#Temp_AccountsReceivable 						--�ꎞ�e�[�u������擾
			WHERE
				[CloseMonth] = @TargetMonthPrevious AND
				[BalanceAmount] <> 0
			
			/********************
			���������\�胊�X�g
			*********************/
			--�@�����\��f�[�^��`�[�ԍ��A������R�[�h�ŏW�v�������ʂ��i�[����B
			INSERT INTO #Temp_ReceiptPlan
			SELECT
					rp.[SlipNumber]																											--�`�[�ԍ�
				,	rp.[CustomerClaimCode]																									--������R�[�h
				,	cc.[CustomerClaimType]																									--������敪
				,	SUM(CASE WHEN  ( rp.DepositFlag is null OR rp.DepositFlag <> '1') THEN ISNULL(rp.Amount, 0) ELSE 0 END) AS [Amount]		--����p�ȊO(�T�}��)
				,	SUM(CASE WHEN  rp.DepositFlag = '1' THEN ISNULL(rp.Amount, 0) ELSE 0 END) AS [Expendes]									--����p(�T�}��)
				,	SUM(ISNULL(rp.[Amount], 0)) AS [PresentMonth]																			--���v(�T�}��)
			FROM
				[dbo].[ReceiptPlan] rp inner join
				[dbo].[CustomerClaim] cc
			ON
				rp.[CustomerClaimCode] = cc.[CustomerClaimCode] 
			WHERE
				rp.[SlipNumber] is not NULL AND
				rp.[SlipNumber] <> '' AND
				rp.[CustomerClaimCode] is not NULL AND
				rp.[CustomerClaimCode] <> '' AND
				cc.CustomerClaimType <> '003' AND																							--������^�C�v���u�N���W�b�g�v
				rp.[DelFlag] = '0'
			GROUP BY
					rp.[SlipNumber]
				,	rp.[CustomerClaimCode]
				,	cc.[CustomerClaimType]


			--�A�T�[�r�X�̓����\�����ݒ肷��B
			--Mod 2016/06/02 arc yano #3532 �}�Ԃ̂��̂̓T�}�����s���B
			--(1)�T�[�r�X�`�[�őΏ۔N���̍��`�[(xxxxxxxx-2)��`�[�ԍ��̎}�Ԃ���������Ԃőޔ�		
			INSERT INTO #Temp_ServiceSalesHeader
			SELECT
					LEFT(sh.[SlipNumber], 8)						--�`�[�ԍ�(�}�Ԃ͎�菜��)
				,	sh.[DepartmentCode]								--����R�[�h
				,	sh.[CustomerCode]								--�ڋq�R�[�h
				,	sh.[SalesDate]									--�[�ԓ�
				,	sh.[ServiceOrderStatus]							--�`�[�X�e�[�^�X
			FROM 
				[ServiceSalesHeader] sh								--�T�[�r�X�`�[�w�b�_
			WHERE
				sh.[SlipNumber] like '%-2%' AND						--���`�[(xxxxxxxx-2) 
				sh.[ServiceOrderStatus] = '006' AND				--�[�ԍ�		
				sh.[SalesDate] >= @TargetDateFrom AND 				
				sh.[SalesDate] <  @TargetDateTo AND 					
				sh.[DelFlag] = '0'
				
			--(2)�T�[�r�X�`�[�őΏ۔N���̍��`�[(xxxxxxxx-2)�̖����ԓ`�[(xxxxxxxx-1)��-1����菜�����`�[�ԍ��őޔ�
			INSERT INTO #Temp_ServiceSalesHeader
			SELECT
					LEFT(sh.[SlipNumber], 8)						--�`�[�ԍ�(�}�Ԃ͎�菜��)
				,	sh.[DepartmentCode]								--����R�[�h
				,	sh.[CustomerCode]								--�ڋq�R�[�h
				,	sh.[SalesDate]									--�[�ԓ�
				,	sh.[ServiceOrderStatus]							--�`�[�X�e�[�^�X
			FROM 
				[ServiceSalesHeader] sh								--�T�[�r�X�`�[�w�b�_
			WHERE
				sh.[SlipNumber] like '%-1%' AND						--���`�[(xxxxxxxx-1) 
				sh.[ServiceOrderStatus] = '006' AND					--�[�ԍ�		
				sh.[SalesDate] >= @TargetDateFrom AND 				
				sh.[SalesDate] <  @TargetDateTo AND 				
				sh.[DelFlag] = '0' AND
				NOT EXISTS
				(
					select 'x' from #Temp_ServiceSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
				)
			
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader
			CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader ([SlipNumber])
			

			--(3)���`�[�݂̂̂���
			INSERT INTO #Temp_SalesOrderReceiptPlan
			SELECT
					sh.[SlipNumber] AS [SlipNumber]						--�`�[�ԍ�
				,	rp.[CustomerClaimCode]	AS [CustomerClaimCode]		--������R�[�h
				,	sh.[SalesDate]	AS [SalesDate]						--�[�ԓ�
				,	sh.[DepartmentCode]	AS [DepartmentCode]				--����R�[�h
				,	sh.[CustomerCode] AS [CustomerCode]					--�ڋq�R�[�h
				,	rp.[CustomerClaimType]	[CustomerClaimType]			--������敪
				,	ISNULL(rp.[Amount], 0) AS [Amount]					--�����\��z
				,	ISNULL(rp.[Expendes], 0) AS [Expendes]				--����p
				,	ISNULL(rp.[PresentMonth], 0) AS [PresentMonth]		--��������(����p�{����p�ȊO)
			FROM 
				[ServiceSalesHeader] sh inner join						--�T�[�r�X�`�[�w�b�_
				#Temp_ReceiptPlan rp									--�����\��
			ON 
				sh.[SlipNumber] = rp.[SlipNumber]
			WHERE 
				sh.[ServiceOrderStatus] = '006' AND						--�[�ԍ�
				sh.[SalesDate] >= @TargetDateFrom AND 				
				sh.[SalesDate] <  @TargetDateTo AND 				
				sh.[DelFlag] = '0' AND
				NOT EXISTS
				(
					select 'x' from #Temp_ServiceSalesHeader tsh where tsh.SlipNumber = LEFT(sh.SlipNumber, 8)			---�ԓ`�E���`�[�����݂��Ȃ����̂̂�
				)
				
			--(3)�ԓ`�E�܂��͍��`�܂ł���ꍇ�̓T�}���[���Ċi�[(����R�[�h�A�ڋq�R�[�h���͐ԓ`�A�܂��͍��`�̂��̂��g�p����)
			INSERT INTO #Temp_SalesOrderReceiptPlan
			SELECT
					gsr.SlipNumber												--�`�[�ԍ�
				,	gsr.CustomerClaimCode										--������R�[�h
				,	ttsh.SalesDate												--�[�ԓ�
				,	ttsh.DepartmentCode											--����R�[�h
				,	ttsh.CustomerCode											--�ڋq�R�[�h
				,	gsr.CustomerClaimType										--������敪
				,	gsr.Amount													--�����\��z
				,	gsr.Expendes												--����p
				,	gsr.PresentMonth											--��������(����p�{����p�ȊO)
			FROM
			(
				SELECT
						LEFT(sh.[SlipNumber], 8) AS [SlipNumber]				--�`�[�ԍ�
					,	rp.[CustomerClaimCode]	AS [CustomerClaimCode]			--������R�[�h
					,	rp.[CustomerClaimType]	[CustomerClaimType]				--������敪
					,	SUM(ISNULL(rp.[Amount], 0)) AS [Amount]					--�����\��z
					,	SUM(ISNULL(rp.[Expendes], 0)) AS [Expendes]				--����p
					,	SUM(ISNULL(rp.[PresentMonth], 0)) AS [PresentMonth]		--��������(����p�{����p�ȊO)
				FROM 
					[ServiceSalesHeader] sh inner join							--�T�[�r�X�`�[�w�b�_
					#Temp_ReceiptPlan rp										--�����\��
				ON 
					sh.[SlipNumber] = rp.[SlipNumber]
				WHERE 
					sh.[ServiceOrderStatus] = '006' AND							--�[�ԍ�
					sh.[SalesDate] >= @TargetDateFrom AND 				
					sh.[SalesDate] <  @TargetDateTo AND 				
					sh.[DelFlag] = '0' AND
					EXISTS
					(
						select 'x' from #Temp_ServiceSalesHeader tsh where tsh.SlipNumber = LEFT(sh.SlipNumber, 8)		--�ԓ`�[�E���`�[�����݂��Ă�����̂̂�
					)
				GROUP BY
					 LEFT(sh.[SlipNumber], 8)
					,rp.[CustomerClaimCode]
					,rp.[CustomerClaimType]
			) gsr INNER JOIN #Temp_ServiceSalesHeader ttsh  ON gsr.SlipNumber = ttsh.SlipNumber

			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan
			CREATE UNIQUE INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan ([SlipNumber], [CustomerClaimCode])

			/***************************
			�����������у��X�g
			****************************/
			--2016/12/22 Mod arc yano #3682 �`�[�ԍ��A������P�ʂŏW�v���s���i�ԓ`�E���`���̎}�Ԃ����`�[�ԍ��Ƃ��ďW�v����j
			--�@�������уf�[�^��`�[�ԍ��A������R�[�h�ŏW�v�������ʂ��i�[����B
			INSERT INTO #Temp_Journal
			SELECT
				  LEFT(j.[SlipNumber], 8) AS SlipNumber																					--�`�[�ԍ�
				 ,j.CustomerClaimCode																									--������R�[�h
				 ,j.CustomerClaimType																									--������敪
				 ,SUM(j.Amount) AS Amount																								--�����z(����p�ȊO)
				 ,SUM(j.ChargesAmount) AS ChargesAmount																					--�����z(����p)
				FROM
				(
					 SELECT
						 jn.SlipNumber AS SlipNumber																						--�`�[�ԍ�
						,jn.CustomerClaimCode AS CustomerClaimCode																			--������R�[�h
						,cc.CustomerClaimType AS CustomerClaimType																			--������敪
						,CASE WHEN LEFT(jn.[AccountCode], 4) != '3410' THEN ISNULL(jn.[Amount], 0) ELSE 0 END AS [Amount]					--�����z(����p�ȊO)
						,CASE WHEN LEFT(jn.[AccountCode], 4) = '3410' THEN  ISNULL(jn.[Amount], 0) ELSE 0 END AS [ChargesAmount]			--�����z(����p)																					--���z
						,jn.AccountCode AS AccountCode																						--����ȖڃR�[�h
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
				,	j.[CustomerClaimCode]
				,	j.[CustomerClaimType]
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_Journal ON #Temp_Journal
			CREATE UNIQUE INDEX IX_Temp_Journal ON #Temp_Journal ([SlipNumber], [CustomerClaimCode])

			--Del 2016/12/22 arc yano #3682
			
			--Mod 2018/01/27 arc yano #3717
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
				dbo.ServiceSalesHeader sh INNER JOIN
				(
					SELECT
						MAX(SlipNumber) as SlipNumber 
					FROM 
						dbo.ServiceSalesHeader
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
				dbo.ServiceSalesHeader sh
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
				,	l.[CustomerClaimCode]
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
					,	[CustomerClaimCode]
				FROM
					#Temp_SalesOrderReceiptPlan
				UNION
				SELECT
						[SlipNumber]
					,	[CustomerClaimCode]
				FROM
					#Temp_Journal									--Mod 2016/12/22 arc yano #3682
				   --#Temp_SalesOrderJournal
				UNION
				SELECT
						[SlipNumber]
					,	[CustomerClaimCode]
				FROM
					#Temp_CarriedBalance
			) AS l 
			LEFT OUTER JOIN
				#Temp_SalesOrderReceiptPlan rp
			ON
				l.SlipNumber = rp.SlipNumber AND
				l.CustomerClaimCode = rp.CustomerClaimCode
			LEFT OUTER JOIN
				#Temp_Journal oj										--Mod 2016/12/22 arc yano #3682
				--#Temp_SalesOrderJournal oj
			ON
				l.SlipNumber = oj.SlipNumber AND
				l.CustomerClaimCode = oj.CustomerClaimCode
			LEFT OUTER JOIN
				#Temp_CarriedBalance cb
			ON
				l.SlipNumber = cb.SlipNumber AND
				l.CustomerClaimCode = cb.CustomerClaimCode
			LEFT OUTER JOIN
				#Temp_SlipInfo si
			ON
				l.SlipNumber = si.SlipNumber

			/********************
			�������|���c�����X�g
			*********************/
			--�R�[�h���X�g�A�����\��^���у��X�g���A���|���c�����쐬����B					
			--����w��ɂ��
			IF @ActionFlag = 0		--����w�� =�u�\���v�̏ꍇ�͈ꎞ�\�Ɋi�[
			BEGIN
				INSERT INTO #Temp_AccountsReceivable

				SELECT
						@TargetDateFrom AS [CloseMonth]																														--����
					,	cl.[SlipNumber]																																		--�`�[�ԍ�
					,	cl.[CustomerClaimCode]																																--������R�[�h
					,	cc.[CustomerClaimName]																																--�����於��
					,	cc.[CustomerClaimType]																																--������敪
					,	ct.[Name]																																			--�����於��
					,	cl.[DepartmentCode]																																	--����R�[�h
					,	dp.[DepartmentName]																																	--���喼��
					,	cl.[CustomerCode]																																	--�ڋq�R�[�h
					,	cu.[CustomerName]																																	--�ڋq����
					/*	--Mod 2016/12/22 arc yano #3682
					,	CASE WHEN ISNULL(cb.[CarriedBalance], 0) = 0 
							  and ISNULL(rp.[Amount], 0) + ISNULL(rp.[Expendes], 0) = 0 
							  and ISNULL(jn.[Amount], 0) > 0 
							 THEN NULL 
							 ELSE cl.[SalesDate]
						END  AS [SalesDate]																																	--�[�ԓ�
					*/
						--Mod 2018/01/27 arc yano
					,	CASE WHEN cl.[SalesDate] is null
							 THEN NULL 
							 ELSE 
								CASE WHEN
									DATEDIFF(d, cl.[SalesDate], @TargetDateTo) > 0								--Mod 2020/02/26 yano #4040
									--DATEDIFF(d, cl.[SalesDate], @TargetDateTo) >= 0							--Mod 2019/05/22 yano #3954
								THEN 
									cl.[SalesDate]
								ELSE 
									NULL
								END
						END AS [SalesDate]
					,	ISNULL(cb.[CarriedBalance], 0) AS [CarriedBalance]																									--�O���J�z
					,	ISNULL(rp.[Amount], 0) AS [PresentMonth]																											--��������(����p�ȊO)
					,	ISNULL(rp.[Expendes], 0) AS [Expendes]																												--����p(����p)
					,	ISNULL(cb.[CarriedBalance], 0) + ISNULL(rp.[PresentMonth], 0) AS [TotalAmount]																		--���v(�O���J�z�{��������)					
					,	ISNULL(jn.[Amount], 0) AS [Payment]																													--��������(����p�ȊO)
					,	ISNULL(cb.[CarriedBalance], 0) + ISNULL(rp.[PresentMonth], 0) - ISNULL(jn.[Amount], 0) - ISNULL(jn.[ChargesAmount], 0) AS [BalanceAmount]			--�c��(�O���J�z�{���������|��������)
					,	ISNULL(jn.ChargesAmount, 0) AS [ChargesPayment]																										--��������(����p)
					
				FROM #Temp_CodeList cl																				--�R�[�h���X�g
				
				INNER JOIN [dbo].[Department] dp																	--����}�X�^
					ON cl.[DepartmentCode] = dp.[DepartmentCode]
				INNER JOIN [dbo].[CustomerClaim] cc																	--������}�X�^
					ON cl.[CustomerClaimCode] = cc.[CustomerClaimCode]
				INNER JOIN [dbo].[c_CustomerClaimType] ct															--������敪�R�[�h�}�X�^
					ON cc.[CustomerClaimType] = ct.[Code]
				INNER JOIN [dbo].[Customer] cu																		--�ڋq�}�X�^
					ON cl.[CustomerCode] = cu.[CustomerCode]
				LEFT JOIN #Temp_CarriedBalance cb																	--�O���J�z���X�g
					ON cl.SlipNumber = cb.SlipNumber AND cl.CustomerClaimCode = cb.CustomerClaimCode
				LEFT JOIN #Temp_SalesOrderReceiptPlan rp															--�����\�胊�X�g
					ON cl.SlipNumber = rp.SlipNumber AND cl.CustomerClaimCode = rp.CustomerClaimCode
				LEFT JOIN #Temp_Journal jn																			--�������у��X�g	--Mod 2016/12/22 arc yano #3682
				--LEFT JOIN #Temp_SalesOrderJournal jn																--�������у��X�g
					ON cl.SlipNumber = jn.SlipNumber AND cl.CustomerClaimCode = jn.CustomerClaimCode

			END
			ELSE	--����w��=�u�ۑ��v�̏ꍇ��DB�̃e�[�u���Ɋi�[
			BEGIN
				--�i�[����O�Ɉ�x�f�[�^���폜
				DELETE FROM 
					[dbo].[AccountsReceivable]
				WHERE 
					CloseMonth = @TargetDateFrom
						
				INSERT INTO [dbo].[AccountsReceivable]

				SELECT
						@TargetDateFrom AS [CloseMonth]																														--����
					,	cl.[SlipNumber]																																		--�`�[�ԍ�
					,	cl.[CustomerClaimCode]																																--������R�[�h
					,	cc.[CustomerClaimName]																																--�����於��
					,	cc.[CustomerClaimType]																																--������敪
					,	ct.[Name]																																			--�����於��
					,	cl.[DepartmentCode]																																	--����R�[�h
					,	dp.[DepartmentName]																																	--���喼��
					,	cl.[CustomerCode]																																	--�ڋq�R�[�h
					,	cu.[CustomerName]
					/*	--Mod 2016/12/22 arc yano #3682																														--�ڋq����
					,	CASE WHEN ISNULL(cb.[CarriedBalance], 0) = 0 
							  and ISNULL(rp.[Amount], 0) + ISNULL(rp.[Expendes], 0) = 0 
							  and ISNULL(jn.[Amount], 0) > 0 
							 THEN NULL 
							 ELSE cl.[SalesDate]
						END  AS [SalesDate]																																	--�[�ԓ�
					*/
						--Mod 2018/01/27 arc yano
					,	CASE WHEN cl.[SalesDate] is null
							 THEN NULL 
							 ELSE 
								CASE WHEN
									DATEDIFF(d, cl.[SalesDate], @TargetDateTo) > 0								--Mod 2020/02/26 yano #4040
									--DATEDIFF(d, cl.[SalesDate], @TargetDateTo) >= 0			--Mod 2019/05/22 yano #3954
								THEN 
									cl.[SalesDate]
								ELSE 
									NULL
								END
						END AS [SalesDate]
					,	ISNULL(cb.[CarriedBalance], 0) AS [CarriedBalance]																									--�O���J�z
					,	ISNULL(rp.[Amount], 0) AS [PresentMonth]																											--��������
					,	ISNULL(rp.[Expendes], 0) AS [Expendes]																												--����p
					,	ISNULL(cb.[CarriedBalance], 0) + ISNULL(rp.[PresentMonth], 0) AS [TotalAmount]																		--���v(�O���J�z�{��������)			
					,	ISNULL(jn.[Amount], 0) AS [Payment]																											--��������(����p�ȊO)
					,	ISNULL(cb.[CarriedBalance], 0) + ISNULL(rp.[PresentMonth], 0) - ISNULL(jn.[Amount], 0) - ISNULL(jn.[ChargesAmount], 0) AS [BalanceAmount]	--�c��(�O���J�z�{���������|���������|�����O���)
					,	'sys' AS [CreateEmployeeCode]																														--�쐬��
					,	@NOW AS [CreateDate]																																--�쐬����
					,	'sys' AS [LastUpdateEmployeeCode]																													--�ŏI�X�V��
					,	@NOW AS [LastUpdateDate]																															--�ŏI�X�V����
					,	'0'	 AS [DelFlag]																																	--�폜�t���O
					,	ISNULL(jn.ChargesAmount, 0) AS [ChargesPayment]																								--��������(����p)
	
				FROM #Temp_CodeList cl																				--�R�[�h���X�g
				INNER JOIN [dbo].[Department] dp																	--����}�X�^
					ON cl.[DepartmentCode] = dp.[DepartmentCode]
				INNER JOIN [dbo].[CustomerClaim] cc																	--������}�X�^
					ON cl.[CustomerClaimCode] = cc.[CustomerClaimCode]
				INNER JOIN [dbo].[c_CustomerClaimType] ct															--������敪�R�[�h�}�X�^
					ON cc.[CustomerClaimType] = ct.[Code]
				INNER JOIN [dbo].[Customer] cu																		--�ڋq�}�X�^
					ON cl.[CustomerCode] = cu.[CustomerCode] 
				LEFT JOIN #Temp_CarriedBalance cb																	--�O���J�z���X�g
					ON cl.SlipNumber = cb.SlipNumber AND cl.CustomerClaimCode = cb.CustomerClaimCode
				LEFT JOIN #Temp_SalesOrderReceiptPlan rp															--�����\�胊�X�g
					ON cl.SlipNumber = rp.SlipNumber AND cl.CustomerClaimCode = rp.CustomerClaimCode
				LEFT JOIN #Temp_Journal jn																			--�������у��X�g		--Mod 2016/12/22 arc yano #3682
				--LEFT JOIN #Temp_SalesOrderJournal jn																--�������у��X�g
					ON cl.SlipNumber = jn.SlipNumber AND cl.CustomerClaimCode = jn.CustomerClaimCode

			END

			--���̏����Ώی�
			SET @TargetMonthCount = @TargetMonthCount - 1				--�c�����f�N�������g
			SET @TargetMonthPrevious = @TargetDateFrom					--�Ώی��O���C���N�������g(������̓����j
			SET @TargetDateFrom = DATEADD(m, 1, @TargetMonthPrevious)	--�Ώۓ�From�C���N�������g(������̑O���{�P�j
			SET @TargetDateTo = DATEADD(m, 1, @TargetDateFrom)			--�Ώۓ�To�C���N�������g(������̓����{�P�j
			IF @TargetDateTo > @NOW
				--Mod 2020/02/26 yano #4040
				SET @TargetDateTo = CONVERT(datetime, CONVERT(nvarchar, DATEADD(d, 1, @NOW), 111))
				--SET @TargetDateTo = @NOW
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
			SET @PARAM = '@TargetMonth datetime, @SalesDateFrom nvarchar(10), @SalesDateTo nvarchar(10), @SlipNumber NVARCHAR(50), @DepartmentCode NVARCHAR(3), @CustomerCode NVARCHAR(10), @Zerovisible NVARCHAR(1), @Classification NVARCHAR(3)'
					
			SET @SQL = ''
			SET @SQL = @SQL +'SELECT' + @CRLF
			SET @SQL = @SQL +'    ar.CloseMonth' + @CRLF
			SET @SQL = @SQL +'	, ar.SlipNumber' + @CRLF
			SET @SQL = @SQL +'	, ar.CustomerClaimCode' + @CRLF
			SET @SQL = @SQL +'	, ar.CustomerClaimName' + @CRLF
			SET @SQL = @SQL +'	, ar.CustomerClaimType' + @CRLF
			SET @SQL = @SQL +'	, ar.CustomerClaimTypeName' + @CRLF
			SET @SQL = @SQL +'	, ar.DepartmentCode' + @CRLF
			SET @SQL = @SQL +'	, ar.DepartmentName' + @CRLF
			SET @SQL = @SQL +'	, ar.CustomerCode' + @CRLF
			SET @SQL = @SQL +'	, ar.CustomerName' + @CRLF
			/* --Mod 2016/12/22 arc yano #3682
			SET @SQL = @SQL +'	, CASE WHEN ISNULL(ar.[CarriedBalance], 0) = 0' + @CRLF 
			SET @SQL = @SQL +'		    and ISNULL(ar.[PresentMonth], 0) + ISNULL(ar.[Expendes], 0) = 0' + @CRLF 
			SET @SQL = @SQL +'		    and ISNULL(ar.[Payment], 0) + ISNULL(ar.[ChargesPayment], 0) > 0 ' + @CRLF
			SET @SQL = @SQL +'		   THEN NULL ' + @CRLF
			SET @SQL = @SQL +'		   ELSE ar.SalesDate' + @CRLF
			SET @SQL = @SQL +'	  END AS SalesDate' + @CRLF
			*/
			SET @SQL = @SQL +'	, ar.SalesDate' + @CRLF 
			SET @SQL = @SQL +'	, ar.CarriedBalance' + @CRLF
			SET @SQL = @SQL +'	, ar.PresentMonth' + @CRLF
			SET @SQL = @SQL +'	, ar.Expendes' + @CRLF
			SET @SQL = @SQL +'	, ar.TotalAmount' + @CRLF
			SET @SQL = @SQL +'	, ar.Payment' + @CRLF
			SET @SQL = @SQL +'	, ar.BalanceAmount' + @CRLF
			SET @SQL = @SQL +'	, '''' AS CreateEmployeeCode' + @CRLF
			SET @SQL = @SQL +'	, NULL AS CreateDate' + @CRLF
			SET @SQL = @SQL +'	, '''' AS LastUpdateEmployeeCode' + @CRLF
			SET @SQL = @SQL +'	, NULL AS LastUpdateDate' + @CRLF
			SET @SQL = @SQL +'	, ''0'' AS DelFlag' + @CRLF
			SET @SQL = @SQL +'	, ar.ChargesPayment' + @CRLF
			SET @SQL = @SQL +'FROM' + @CRLF
			SET @SQL = @SQL +'	#Temp_AccountsReceivable ar' + @CRLF
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
			
			--�[���\��
			IF ((@Zerovisible is not null) AND (@Zerovisible <> '') AND (@Zerovisible = '0'))
			BEGIN
				SET @SQL = @SQL +' AND ar.BalanceAmount != 0' + @CRLF
			END

			--�敪
			IF ((@Classification is not null) AND (@Classification <> ''))
			BEGIN
				SET @SQL = @SQL +' AND EXISTS(select 1 FROM dbo.c_CustomerClaimType cc WHERE cc.DelFlag = ''0'' AND cc.CustomerClaimClass = @Classification AND cc.Code = ar.CustomerClaimType)' + @CRLF
			END

			EXECUTE sp_executeSQL @SQL, @PARAM, @TargetMonth, @SalesDateFrom, @SalesDateTo, @SlipNumber, @DepartmentCode, @CustomerCode, @Zerovisible, @Classification

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


