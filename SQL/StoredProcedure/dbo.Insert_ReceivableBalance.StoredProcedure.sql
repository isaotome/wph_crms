USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[Insert_ReceivableBalance]    Script Date: 2016/06/06 16:04:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







-- ================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2015/08/10 arc yano #3233 ���|�����[�Ή� ���|�c���쐬�p�̃X�g�A�h�ǉ�
-- Update date: 
-- 2016/06/03 arc yano #3532 �T�[�r�X�`�[�̏W�v���Ɂu-1�v�u-2�v�̎}�Ԃ��T������
-- 2016/02/25 arc yano #3302_���|���Ǘ���ʂ̍��ڒǉ��E�폜�ł̑Ή��R��̏C��
--
-- Description:	�w�肵���N���̔��|�c���̍쐬(���Ώ۔N���̎w�肪�Ȃ��ꍇ�́A���߃X�e�[�^�X=�u�{���߁v�̍ŐV�����Ώی��ƂȂ�)
-- ==================================================================================================================================
CREATE PROCEDURE [dbo].[Insert_ReceivableBalance]
	@TargetMonth datetime = NULL					--�c���̌v�Z�Ώ۔N��
AS
BEGIN

	SET NOCOUNT ON;

	--�萔----------------------------
	DECLARE @NOW datetime					--���ݓ���
	DECLARE @TODAY datetime					--�{��
	DECLARE @THISMONTH datetime				--����
	DECLARE @TargetMonthEnd datetime		--�Ώ۔N������

	----------------------------------

	--�����ꎞ�\�̐錾
	/*************************************************************************/
	BEGIN TRY
		DROP TABLE #Temp_CodeList				--�R�[�h���X�g(�����\��^���тɑ��݂���S�Ă̓`�[�ԍ��A������R�[�h���̑g����)
		DROP TABLE #Temp_ReceiptPlan			--�����\�胊�X�g
		DROP TABLE #Temp_SalesOrderReceiptPlan	--�����\�胊�X�g(�ԗ��^�T�[�r�X)
		DROP TABLE #Temp_Journal				--�������у��X�g
		DROP TABLE #Temp_SalesOrderJournal		--�������у��X�g(�ԗ��^�T�[�r�X)
		DROP TABLE #Temp_ServiceSalesHeader		--�T�[�r�X�`�[�w�b�_
	END TRY
	BEGIN CATCH
		--����
	END CATCH


	--�R�[�h���X�g
	CREATE TABLE #Temp_CodeList (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- ������R�[�h
	,	[SlipType]	NVARCHAR(1)							-- �`�[�^�C�v(0:�ԗ��^1:�T�[�r�X)	
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[CustomerClaimType] NVARCHAR(3)					-- ������敪	
	)

	--�����\�胊�X�g
	CREATE TABLE #Temp_ReceiptPlan (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- ������R�[�h
	,	[CustomerClaimType] NVARCHAR(10)				-- ������敪
	,	[Amount] DECIMAL(10, 0)							-- �����\��z
	)
	CREATE UNIQUE INDEX IX_Temp_ReceiptPlan ON #Temp_ReceiptPlan ([SlipNumber], [CustomerClaimCode])

	--�����\�胊�X�g(�ԗ��^�T�[�r�X)
	CREATE TABLE #Temp_SalesOrderReceiptPlan (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- ������R�[�h
	,	[SlipType]	NVARCHAR(1)							-- �`�[�^�C�v(0:�ԗ��^1:�T�[�r�X)	
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[CustomerClaimType] NVARCHAR(3)					-- ������敪
	,	[Amount] DECIMAL(10, 0)							-- �����\��z
	)
	CREATE UNIQUE INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan ([SlipNumber], [CustomerClaimCode])

	--�������у��X�g
	CREATE TABLE #Temp_Journal (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- ������R�[�h
	,	[CustomerClaimType] NVARCHAR(10)				-- ������敪
	,	[Amount] DECIMAL(10, 0)							-- �����z
	)
	CREATE UNIQUE INDEX IX_Temp_Journal ON #Temp_Journal ([SlipNumber], [CustomerClaimCode])

	--�������у��X�g(�ԗ��^�T�[�r�X)
	CREATE TABLE #Temp_SalesOrderJournal (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[CustomerClaimCode] NVARCHAR(10) NOT NULL		-- ������R�[�h
	,	[SlipType]	NVARCHAR(1)							-- �`�[�^�C�v(0:�ԗ��^1:�T�[�r�X)	
	,	[SalesDate]	datetime							-- �[�ԓ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[CustomerClaimType] NVARCHAR(3)					-- �����z
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal ([SlipNumber], [CustomerClaimCode])

	--�T�[�r�X�`�[�w�b�_ //Add 2016/06/02
	CREATE TABLE #Temp_ServiceSalesHeader(
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[DepartmentCode] NVARCHAR(3)					-- ����R�[�h
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[SalesDate]	datetime							-- �[�ԓ�
	)
	CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader ([SlipNumber])

	/*************************************************************************/

	--��������
	DECLARE @ErrorMessage NVARCHAR(4000) = ''
    DECLARE @ErrorNumber INT = 0


	--���ݓ���
	SET @NOW = GETDATE()
	--����
	SET @TODAY = CONVERT(DATETIME, CONVERT(NVARCHAR(10), GETDATE(), 111), 111)
	--����1��
	SET @THISMONTH = CONVERT(DATETIME, CONVERT(NVARCHAR(7), @TODAY, 111) + '/01', 111)
	
	--�������Ώی��̐ݒ�
	-- �Ώ۔N�����w�肳��Ă��Ȃ��ꍇ�́A�����ߏ����󋵂��u�{���߁v�̌��̒��ōŐV���������Ώ۔N���ɐݒ肷��B
	IF ((@TargetMonth is NULL) OR (@TargetMonth = CONVERT(datetime, '1900-01-01', 120)))
	BEGIN
		SELECT @TargetMonth = ISNULL(MAX(CONVERT(datetime, cmc.[CloseMonth], 120)), @THISMONTH)
		FROM [CloseMonthControl] cmc					--�����ߏ�����
		WHERE cmc.[CloseStatus] = '003'					--�u�{���߁v
	END

	--�Ώ۔N�������̐ݒ�(����1����ݒ�)
	SET @TargetMonthEnd = DATEADD( m, 1, @TargetMonth) 

	--�g�����U�N�V�����J�n 
	BEGIN TRANSACTION
	BEGIN TRY

		BEGIN
			--��x���|���e�[�u���̑S���R�[�h���폜
			TRUNCATE TABLE
				[dbo].[AccountsReceivable]
			
			--�ꎟ�\������		
			DELETE FROM  #Temp_CodeList							--�R�[�h���X�g
			DELETE FROM  #Temp_ReceiptPlan						--�����\�胊�X�g
			DELETE FROM  #Temp_SalesOrderReceiptPlan			--�����\�胊�X�g(�ԗ��^�T�[�r�X)
			DELETE FROM  #Temp_Journal							--�������у��X�g
			DELETE FROM  #Temp_SalesOrderJournal				--�������у��X�g(�ԗ��^�T�[�r�X)
			
			-----------------------------

			/********************
			���������\�胊�X�g
			*********************/
			--�@�����\��f�[�^��`�[�ԍ��A������R�[�h�ŏW�v�������ʂ��i�[����B
			INSERT INTO #Temp_ReceiptPlan
			SELECT
					rp.[SlipNumber]								--�`�[�ԍ�(�}�Ԃ�����ꍇ�͏W�v����)
				,	rp.[CustomerClaimCode]						--������R�[�h
				,	cc.[CustomerClaimType]						--������敪
				,	SUM(ISNULL(rp.Amount, 0)) as [Amount]		--���z(�T�}��)
			FROM
				[dbo].[ReceiptPlan] rp inner join
				[dbo].[CustomerClaim] cc
			ON
				rp.	[CustomerClaimCode] = cc.[CustomerClaimCode] 
			WHERE
				rp.[SlipNumber] is not NULL AND
				rp.[SlipNumber] <> '' AND
				rp.[CustomerClaimCode] is not NULL AND
				rp.[CustomerClaimCode] <> '' AND
				cc.[CustomerClaimType] <> '003' and  --������^�C�v���u�N���W�b�g�v
				rp.[DelFlag] = '0'
			GROUP BY
					rp.[SlipNumber]
				,	rp.[CustomerClaimCode]
				,	cc.[CustomerClaimType]

			--�A�ԗ��̓����\�����ݒ肷��B
			INSERT INTO #Temp_SalesOrderReceiptPlan
			SELECT
					sh.[SlipNumber]									--�`�[�ԍ�
				,	rp.[CustomerClaimCode]							--������R�[�h
				,	'0' AS [SlipType]								--�`�[�^�C�v(0:�ԗ��Œ�)
				,	sh.[SalesDate]									--�[�ԓ�
				,	sh.[DepartmentCode]								--����R�[�h
				,	sh.[CustomerCode]								--�ڋq�R�[�h
				,	rp.[CustomerClaimType]							--������敪
				,	rp.[Amount]										--�����\��z
			FROM 
				[CarSalesHeader] sh inner join						--�ԗ��`�[�w�b�_
				#Temp_ReceiptPlan rp								--�����\��
			ON 
				sh.[SlipNumber] = rp.[SlipNumber]
			WHERE 
				sh.[SalesOrderStatus] = '005' AND					--�[�ԍ�
				sh.[SalesDate] < @TargetMonthEnd AND 				--�Ώ۔N�������P�����O
				sh.[DelFlag] = '0'		

			--�B�T�[�r�X�̓����\�����ݒ肷��B
			--(1)�T�[�r�X�`�[�őΏ۔N���̍��`�[(xxxxxxxx-2)��-2����菜�����`�[�ԍ��őޔ�
			INSERT INTO #Temp_ServiceSalesHeader
			SELECT
					LEFT(sh.[SlipNumber], 8)						--�`�[�ԍ�(�}�Ԃ͎�菜��)
				,	sh.[DepartmentCode]								--����R�[�h
				,	sh.[CustomerCode]								--�ڋq�R�[�h
				,	sh.[SalesDate]									--�[�ԓ�
			FROM 
				[ServiceSalesHeader] sh								--�T�[�r�X�`�[�w�b�_
			WHERE
				sh.[SlipNumber] like '%-2%' AND						--���`�[(xxxxxxxx-2) 
				sh.[ServiceOrderStatus] = '006' AND					--�[�ԍ�		
				sh.[SalesDate] < @TargetMonthEnd AND 				--�Ώ۔N�������P�����O			
				sh.[DelFlag] = '0'
			
				--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader
			CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader ([SlipNumber])

			--(2)�T�[�r�X�`�[�őΏ۔N���̍��`�[(xxxxxxxx-2)�̖����ԓ`�[(xxxxxxxx-1)��-1����菜�����`�[�ԍ��őޔ�
			INSERT INTO #Temp_ServiceSalesHeader
			SELECT
					LEFT(sh.[SlipNumber], 8)						--�`�[�ԍ�(�}�Ԃ͎�菜��)
				,	sh.[DepartmentCode]								--����R�[�h
				,	sh.[CustomerCode]								--�ڋq�R�[�h
				,	sh.[SalesDate]									--�[�ԓ�
			FROM 
				[ServiceSalesHeader] sh								--�T�[�r�X�`�[�w�b�_
			WHERE
				sh.[SlipNumber] like '%-1%' AND						--���`�[(xxxxxxxx-1) 
				sh.[ServiceOrderStatus] = '006' AND					--�[�ԍ�		
				sh.[SalesDate] < @TargetMonthEnd AND 				--�Ώ۔N�������P�����O			
				sh.[DelFlag] = '0' AND
				NOT EXISTS
				(
					select 'x' from #Temp_ServiceSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
				)
			
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader
			CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader ([SlipNumber])

			--(3)���`�݂̂������݂��Ȃ�
			INSERT INTO #Temp_SalesOrderReceiptPlan
			
			SELECT
					sh.[SlipNumber] AS [SlipNumber]							--�`�[�ԍ�
				,	rp.[CustomerClaimCode]	AS [CustomerClaimCode]			--������R�[�h
				,	'1' AS [SlipType]										--�`�[�^�C�v
				,	sh.[SalesDate]	AS [SalesDate]							--�[�ԓ�
				,	sh.[DepartmentCode]	AS [DepartmentCode]					--����R�[�h
				,	sh.[CustomerCode] AS [CustomerCode]						--�ڋq�R�[�h
				,	rp.[CustomerClaimType]	[CustomerClaimType]				--������敪
				,	ISNULL(rp.[Amount], 0) AS [Amount]						--�����\��z
			FROM 
				[ServiceSalesHeader] sh inner join							--�T�[�r�X�`�[�w�b�_
				#Temp_ReceiptPlan rp										--�����\��
			ON 
				sh.[SlipNumber] = rp.[SlipNumber]
			WHERE 
				sh.[ServiceOrderStatus] = '006' AND					--�[�ԍ�
				sh.[SalesDate] < @TargetMonthEnd AND 				--�Ώ۔N�������P�����O
				sh.[DelFlag] = '0'	AND				
				NOT EXISTS
				(
					select 'x' from #Temp_ServiceSalesHeader tsh where tsh.SlipNumber = LEFT(sh.SlipNumber, 8)			--���`�[�����݂��Ȃ����̂̂�
				)
			
			--(3)�ԓ`�E�܂��͍��`�܂ł���ꍇ�̓T�}���[���Ċi�[(����R�[�h�A�ڋq�R�[�h���͐ԓ`�A�܂��͍��`�̂��̂��g�p����)
			INSERT INTO #Temp_SalesOrderReceiptPlan
			SELECT
					gsr.SlipNumber
				,	gsr.CustomerClaimCode
				,	'1' AS [SlipType]
				,	ttsh.SalesDate
				,	ttsh.DepartmentCode
				,	ttsh.CustomerCode
				,	gsr.CustomerClaimType
				,	gsr.Amount
			FROM
			(
				SELECT
						LEFT(sh.[SlipNumber], 8) AS [SlipNumber]				--�`�[�ԍ�
					,	rp.[CustomerClaimCode]	AS [CustomerClaimCode]			--������R�[�h
					,	rp.[CustomerClaimType]	[CustomerClaimType]				--������敪
					,	SUM(ISNULL(rp.[Amount], 0)) AS [Amount]					--�����\��z
				FROM 
					[ServiceSalesHeader] sh inner join							--�T�[�r�X�`�[�w�b�_
					#Temp_ReceiptPlan rp										--�����\��
				ON 
					sh.[SlipNumber] = rp.[SlipNumber]
				WHERE 
					sh.[ServiceOrderStatus] = '006' AND							--�[�ԍ�
					sh.[SalesDate] < @TargetMonthEnd AND 						--�Ώ۔N�������P�����O				
					sh.[DelFlag] = '0' AND
					EXISTS
					(
						select 'x' from #Temp_ServiceSalesHeader tsh where tsh.SlipNumber = LEFT(sh.SlipNumber, 8)			--���`�[�����݂�����̂̂�
					)
				GROUP BY
						LEFT(sh.[SlipNumber], 8)
					,	rp.[CustomerClaimCode]
					,	rp.[CustomerClaimType]

			) gsr INNER JOIN #Temp_ServiceSalesHeader ttsh  ON gsr.SlipNumber = ttsh.SlipNumber

			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan
			CREATE UNIQUE INDEX IX_Temp_SalesOrderReceiptPlan ON #Temp_SalesOrderReceiptPlan ([SlipNumber], [CustomerClaimCode])

			/********************
			�����������у��X�g
			*********************/
			--�@�������уf�[�^��`�[�ԍ��A������R�[�h�ŏW�v�������ʂ��i�[����B
			INSERT INTO #Temp_Journal
			SELECT
					jn.[SlipNumber]								--�`�[�ԍ�
				,	jn.[CustomerClaimCode]						--������R�[�h
				,	cc.[CustomerClaimType]						--������敪
				,	SUM(ISNULL(jn.Amount, 0)) as [Amount]		--���z(�T�}��)
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
				jn.[JournalType] =  '001' AND				--���o���敪=�u�����v
				jn.[AccountType] <> '099' AND				--�����̎�ʂ��u�f�[�^�j���v�ȊO
				jn.[JournalDate] < @TargetMonthEnd	AND		--���������Ώ۔N�������P�����O
				jn.[DelFlag] = '0' AND
				cc.CustomerClaimType <> '003' 				--������^�C�v���u�N���W�b�g�v

			GROUP BY
					jn.[SlipNumber]
				,	jn.[CustomerClaimCode]
				,	cc.[CustomerClaimType]		
			
			--�A�ԗ��̓������я����i�[����B
			INSERT INTO #Temp_SalesOrderJournal
			SELECT 
					sh.[SlipNumber]									--�`�[�ԍ�
				,	jn.[CustomerClaimCode]							--������R�[�h
				,	'0' AS [SlipType]								--�`�[�^�C�v(0:�ԗ��Œ�)
				,	sh.[SalesDate]									--�[�ԓ�
				,	sh.[DepartmentCode]								--����R�[�h
				,	sh.[CustomerCode]								--�ڋq�R�[�h
				,	jn.[CustomerClaimType]							--������敪
				,	jn.[Amount]										--�����z	
			FROM 
				[CarSalesHeader] sh inner join						--�ԗ��`�[�w�b�_
				#Temp_Journal jn									--��������
			ON 
				sh.[SlipNumber] = jn.[SlipNumber]
			WHERE 	
				sh.[DelFlag] = '0'
			
			--�B�T�[�r�X�̓������я���ݒ肷��B
			--(1)���`�̂�
			INSERT INTO #Temp_SalesOrderJournal
			SELECT 
					sh.[SlipNumber] AS [SlipNumber]												--�`�[�ԍ�
				,	jn.[CustomerClaimCode]	AS [CustomerClaimCode]								--������R�[�h
				,	'1' AS [SlipType]															--�`�[�^�C�v(1:�T�[�r�X�Œ�)
				,	sh.[SalesDate] AS [SalesDate]												--�[�ԓ�
				,	sh.[DepartmentCode]	AS [DepartmentCode]										--����R�[�h
				,	sh.[CustomerCode]	AS [CustomerCode]										--�ڋq�R�[�h
				,	jn.[CustomerClaimType]  AS [CustomerClaimType]								--������敪
				,	ISNULL(jn.[Amount], 0) AS [Amount]											--�����z
			FROM 
				dbo.ServiceSalesHeader sh inner join											--�T�[�r�X�`�[�w�b�_
				#Temp_Journal jn																--��������
			ON 
				sh.[SlipNumber] = jn.[SlipNumber]
			WHERE 
				sh.[DelFlag] = '0' AND
				NOT EXISTS
				(
					select 'x' from #Temp_ServiceSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
				)

			--(2)�ԓ`�A�܂��͍��`�܂ł�������\����T�}���[���Ċi�[(����R�[�h�A�ڋq�R�[�h���͐ԓ`�A�܂��͍��`�̂��̂��g�p����
			INSERT INTO #Temp_SalesOrderJournal
			SELECT
					jsh.SlipNumber
				,	jsh.CustomerClaimCode
				,	jsh.SlipType
				,	tssh.SalesDate
				,	tssh.DepartmentCode
				,	tssh.CustomerCode
				,	jsh.CustomerClaimType
				,	jsh.Amount
			FROM

			(
				SELECT 
						LEFT(sh.[SlipNumber], 8) AS [SlipNumber]																		--�`�[�ԍ�
					,	jn.[CustomerClaimCode]	AS [CustomerClaimCode]																	--������R�[�h
					,	'1' AS [SlipType]																								--�`�[�^�C�v(1:�T�[�r�X�Œ�)
					,	jn.[CustomerClaimType]  AS [CustomerClaimType]																	--������敪
					,	SUM(ISNULL(jn.[Amount], 0))AS [Amount]																			--�����z(����p�ȊO)
				FROM 
					dbo.ServiceSalesHeader sh inner join																				--�T�[�r�X�`�[�w�b�_
					#Temp_Journal jn																									--��������
				ON 
					sh.[SlipNumber] = jn.[SlipNumber]
				WHERE 
					sh.[DelFlag] = '0' AND
					EXISTS
					(
						select 'x' from #Temp_ServiceSalesHeader tsh WHERE tsh.SlipNumber = LEFT(sh.SlipNumber, 8)
					)	
				GROUP BY 
					LEFT(sh.[SlipNumber], 8)
					, jn.[CustomerClaimCode]
					, jn.[CustomerClaimType]
			) jsh INNER JOIN #Temp_ServiceSalesHeader tssh ON jsh.SlipNumber = tssh.SlipNumber
			
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal
			CREATE UNIQUE INDEX IX_Temp_SalesOrderJournal ON #Temp_SalesOrderJournal ([SlipNumber], [CustomerClaimCode])
			/********************
			�����R�[�h���X�g
			*********************/
			--�����\��^���у��X�g�ɑ��݂���A�`�[�ԍ��A������R�[�h���̑g�����̃��X�g���쐬����
			INSERT INTO #Temp_CodeList
			SELECT
					l.[SlipNumber]
				,	l.[CustomerClaimCode]
				,	l.[SlipType]
				,	l.[SalesDate]
				,	l.[DepartmentCode]
				,	l.[CustomerCode]
				,	l.[CustomerClaimType]
			FROM (
				SELECT
						[SlipNumber]
					,	[CustomerClaimCode]
					,	[SlipType]
					,	[SalesDate]
					,	[DepartmentCode]
					,	[CustomerCode]
					,	[CustomerClaimType]
				FROM
					#Temp_SalesOrderReceiptPlan
				UNION
				SELECT
						[SlipNumber]
					,	[CustomerClaimCode]
					,	[SlipType]
					,	[SalesDate]
					,	[DepartmentCode]
					,	[CustomerCode]
					,	[CustomerClaimType]
				FROM
					#Temp_SalesOrderJournal
			) AS l
	
			/********************
			�������|���c�����X�g
			*********************/
			--�R�[�h���X�g�A�����\��^���у��X�g���A���|���c�����쐬����B
			INSERT INTO [dbo].[AccountsReceivable]

			SELECT
					@TargetMonth AS [CloseMonth]												--����
				,	cl.[SlipNumber]																--�`�[�ԍ�
				,	cl.[CustomerClaimCode]														--������R�[�h
				,	cc.[CustomerClaimName]														--�����於��
				,	cl.[CustomerClaimType]														--������敪
				,	ct.[Name]																	--�����於��
				,	cl.[DepartmentCode]															--����R�[�h
				,	dp.[DepartmentName]															--���喼��
				,	cl.[CustomerCode]															--�ڋq�R�[�h
				,	cu.[CustomerName]															--�ڋq����
				,	cl.[SlipType]																--�`�[�^�C�v
				,	cn.[Name]																	--�`�[�^�C�v��
				,	cl.[SalesDate]																--�[�ԓ�
				,	NULL AS [CarriedBalance]													--�O���J�z
				,	NULL AS [PresentMonth]														--��������
				,	NULL AS [Expendes]															--����p
				,	NULL AS [TotalAmount]														--���v
				,	NULL AS [Payment]															--��������(����p�ȊO)
				--,	NULL AS [AdvancesReceived]													--�����O���		-- Del 2016/02/25 ARC YANO �s��Ή�
				,	(ISNULL(rp.[Amount], 0) - ISNULL(jn.[Amount], 0)) AS [BalanceAmount]		--�c��
				,	'sys' AS [CreateEmployeeCode]												--�쐬��
				,	@NOW AS [CreateDate]														--�쐬����
				,	'sys' AS [LastUpdateEmployeeCode]											--�ŏI�X�V��
				,	@NOW AS [LastUpdateDate]													--�ŏI�X�V����
				,	'0'	 AS [DelFlag]															--�폜�t���O
				,   NULL AS [ChargesPayment]													--��������(����p)	-- Add 2016/02/25 ARC YANO �s��Ή�
			FROM #Temp_CodeList cl																--�R�[�h���X�g
			INNER JOIN [dbo].[Department] dp																	--����}�X�^
				ON cl.[DepartmentCode] = dp.[DepartmentCode]
			INNER JOIN [dbo].[CustomerClaim] cc																	--������}�X�^
				ON cl.[CustomerClaimCode] = cc.[CustomerClaimCode]
			INNER JOIN [dbo].[c_CustomerClaimType] ct															--������敪�R�[�h�}�X�^
				ON cl.[CustomerClaimType] = ct.[Code]
			INNER JOIN [dbo].[Customer] cu																		--�ڋq�}�X�^
				ON cl.[CustomerCode] = cu.[CustomerCode] 
			INNER JOIN [dbo].[c_CodeName] cn																	--�ڋq�}�X�^
				ON cl.[SlipType] = cn.[Code] AND cn.[CategoryCode] = '014'										--�J�e�S���R�[�h�́u014�v
			LEFT JOIN #Temp_SalesOrderReceiptPlan rp															--�����\�胊�X�g
				ON cl.SlipNumber = rp.SlipNumber AND cl.CustomerClaimCode = rp.CustomerClaimCode
			LEFT JOIN #Temp_SalesOrderJournal jn																--�������у��X�g
				ON cl.SlipNumber = jn.SlipNumber AND cl.CustomerClaimCode = jn.CustomerClaimCode			
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
END













GO


