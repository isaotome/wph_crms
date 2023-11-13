USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCashJournalOutput]    Script Date: 2015/02/23 16:13:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






--2015/02/23 arc yano �����o�[���Ή�
CREATE PROCEDURE [dbo].[GetCashJournalOutput]
	@TargetDate datetime   		   --�Ώ۔N��
AS


BEGIN
	/*-------------------------------------------*/
	/* �ꎞ�e�[�u��(�����ݍ��e�[�u��)            */
	/*-------------------------------------------*/
	-- �����ݍ��e�[�u��
		CREATE TABLE #temp_CashBalance_1
		(
			OfficeCode 		nvarchar(3),	--���Ə��R�[�h
			M				nvarchar(6),	--ClosedDate�̌��܂ł𕶎���Ŕ��o
			ClosedDate		datetime,		--���ߓ�
			TotalAmount		decimal(10, 0),	--�����v
			CashAccountCode nvarchar(3)		--���������R�[�h
		)

	--�f�[�^�}��
	INSERT INTO #temp_CashBalance_1
	SELECT	
		 OfficeCode
		,CONVERT(VARCHAR(6), ClosedDate, 112)
		,ClosedDate
		,TotalAmount
		,CashAccountCode
	FROM
		dbo.CashBalance
	WHERE
		DelFlag = '0' AND
		CloseFlag = '1'

	-- �����ݍ��e�[�u��
		CREATE TABLE #temp_CashBalance_2
		(
			OfficeCode 		nvarchar(3),	--���Ə��R�[�h
			TotalAmount		decimal(10, 0),	--�����v
			CashAccountCode nvarchar(3),	--���������R�[�h
			M				nvarchar(6),	--ClosedDate�̌��܂ł𕶎���Ŕ��o
			LastDate		datetime,		--���ߓ�
			K				datetime		--����(�p�����[�^�Ɣ�r�p)
		)

	--�f�[�^�}��
	INSERT INTO #temp_CashBalance_2
	SELECT	
		 OfficeCode
		,TotalAmount
		,CashAccountCode
		,M
		,MAX(ClosedDate)
		,DATEADD(M, 1, CONVERT(datetime, M + '01', 120))
	FROM
		#temp_CashBalance_1
	GROUP BY
		OfficeCode, M, CashAccountCode, TotalAmount
	
	--�C���f�b�N�X�̍쐬
	CREATE INDEX ix_temp_CashBalance_2 ON #temp_CashBalance_2(OfficeCode, M, CashAccountCode, TotalAmount)
	
	-- �����ݍ��e�[�u��
		CREATE TABLE #temp_CashBalance_3
		(
			OfficeCode 		nvarchar(3),	--���Ə��R�[�h
			CashAccountCode nvarchar(3),	--���������R�[�h
			M				nvarchar(6),	--ClosedDate�̌��܂ł𕶎���Ŕ��o
			LastDate		datetime,		--���ߓ�
			K				datetime		--����(�p�����[�^�Ɣ�r�p)
		)

	--�f�[�^�}��
	INSERT INTO #temp_CashBalance_3
	SELECT	
		 OfficeCode
		,CashAccountCode
		,M
		,MAX(LastDate)
		,K
	FROM
		#temp_CashBalance_2
	GROUP BY 
		OfficeCode, CashAccountCode, M, K
		
	-- �����ݍ��e�[�u��
		CREATE TABLE #temp_CashBalance_4
		(
			OfficeCode 		nvarchar(3),	--���Ə��R�[�h
			TotalAmount		decimal(10, 0),	--�����v
			CashAccountCode nvarchar(3),	--���������R�[�h
			M				nvarchar(6),	--ClosedDate�̌��܂ł𕶎���Ŕ��o
			LastDate		datetime,		--���ߓ�
			K				datetime		--����(�p�����[�^�Ɣ�r�p)
		)

	--�f�[�^�}��
	INSERT INTO #temp_CashBalance_4
	SELECT	
		 OfficeCode
		,TotalAmount
		,CashAccountCode
		,M
		,LastDate
		,K
	FROM
		#temp_CashBalance_2 a
	WHERE
		EXISTS
		(
			SELECT 'X'
			FROM
				#temp_CashBalance_3
			WHERE A.OfficeCode = OfficeCode AND
				  A.CashAccountCode = CashAccountCode AND
				  A.M = M AND
				  A.LastDate = LastDate AND
				  A.K = K
		)				
	/*-------------------------------------------*/
	/* �ꎞ�e�[�u��(���o���e�[�u��)            */
	/*-------------------------------------------*/
	-- ���o���e�[�u���P
		CREATE TABLE #temp_Journal_1
		(
			OfficeCode 		nvarchar(3),	--���Ə��R�[�h
			CashAccountCode	nvarchar(3),	--ClosedDate�̌��܂ł𕶎���Ŕ��o
			Jd				nvarchar(6),	--�`�[���t�̔N���܂Ŕ��o
			Amount		decimal(10, 0)		--���z
		)

	--�f�[�^�}��
	INSERT INTO #temp_Journal_1
	SELECT	
		 OfficeCode
		,CashAccountCode
		,CONVERT(VARCHAR(6), JournalDate, 112)
		,Amount
	FROM
		dbo.Journal
	WHERE
		DelFlag = '0' AND
		AccountType='001' AND
		(
			(JournalType='001' AND Amount >= 0) OR 
			(JournalType='002' and Amount < 0)
		)
	
	-- ���o���e�[�u���Q
		CREATE TABLE #temp_Journal_2
		(
			OfficeCode 		nvarchar(3),	--���Ə��R�[�h
			CashAccountCode nvarchar(3),	--���������R�[�h
			Jd				nvarchar(6),	--�`�[���t�̔N���܂Ŕ��o
			TotalAmount		decimal(10, 0),	--�����v		
			K				datetime		--����(�p�����[�^�Ɣ�r�p)
		)

	--�f�[�^�}��
	INSERT INTO #temp_Journal_2
	SELECT	
		 OfficeCode
		,CashAccountCode
		,Jd
		,SUM(ABS(Amount))
		,CONVERT(datetime, Jd + '01', 120)
	FROM
		#temp_Journal_1
	GROUP BY
		OfficeCode, CashAccountCode, Jd
	
	--�C���f�b�N�X�̍쐬
	CREATE INDEX ix_temp_Journal_2 ON #temp_Journal_2(OfficeCode,CashAccountCode, Jd)

	/*-------------------------------------------*/
	/* �ꎞ�e�[�u��(���o���e�[�u�����̂Q)        */
	/*-------------------------------------------*/
	-- ���o���e�[�u���R
		CREATE TABLE #temp_Journal_3
		(
			OfficeCode 		nvarchar(3),	--���Ə��R�[�h
			CashAccountCode	nvarchar(3),	--���������R�[�h
			Jd				nvarchar(6),	--�`�[���t�̔N���܂Ŕ��o
			Amount		decimal(10, 0)		--���z
		)

	--�f�[�^�}��
	INSERT INTO #temp_Journal_3
	SELECT	
		 OfficeCode
		,CashAccountCode
		,CONVERT(VARCHAR(6), JournalDate, 112)
		,Amount
	FROM
		dbo.Journal
	WHERE
		DelFlag = '0' AND
		AccountType='001' AND
		(
			(JournalType='002' AND Amount >= 0) OR 
			(JournalType='001' and Amount < 0)
		)
	
	-- ���o���e�[�u���S
		CREATE TABLE #temp_Journal_4
		(
			OfficeCode 		nvarchar(3),	--���Ə��R�[�h
			CashAccountCode nvarchar(3),	--���������R�[�h
			Jd				nvarchar(6), 	--�`�[���t�̔N���܂Ŕ��o
			TotalAmount		decimal(10, 0),	--�����v		
			K				datetime		--����(�p�����[�^�Ɣ�r�p)
		)

	--�f�[�^�}��
	INSERT INTO #temp_Journal_4
	SELECT	
		 OfficeCode
		,CashAccountCode
		,Jd
		,SUM(ABS(Amount))
		,CONVERT(datetime, Jd + '01', 120)
	FROM
		#temp_Journal_3
	GROUP BY
		OfficeCode, CashAccountCode, Jd
	
	--�C���f�b�N�X�̍쐬
	CREATE INDEX ix_temp_Journal_4 ON #temp_Journal_4(OfficeCode, CashAccountCode, Jd)


	
	/*-------------------------------------------*/
	/* �����o�[���ꗗ                            */
	/*-------------------------------------------*/
	SELECT
		  X.Lastdate
		, O.OfficeCode
		, C.CashAccountCode
		, O.OfficeName
		, C.CashAccountName
		, ISNULL(X.TotalAmount, 0) AS LastMonthBalance
		, ISNULL(Y.Totalamount, 0) AS ThisMonthJournal
		, ISNULL(Z.Totalamount, 0) AS ThisMonthPayment
		, ( ISNULL(X.TotalAmount, 0) + ISNULL(Y.Totalamount, 0) - ISNULL(Z.Totalamount, 0)) AS ThisMonthBalance
	FROM
		dbo.CashAccount AS C INNER JOIN
	    dbo.Office AS O ON C.OfficeCode = O.OfficeCode LEFT OUTER JOIN
		    (
		    	SELECT 
					  OfficeCode
					, CashAccountCode
					, Jd
					, Totalamount
					, K
				FROM 
					#temp_Journal_2 AS YY
			 	WHERE 
					YY.K = @TargetDate
			) AS Y ON C.OfficeCode = Y.OfficeCode AND C.CashAccountCode = Y.CashAccountCode LEFT OUTER JOIN
	        (
				SELECT 
					  OfficeCode
					, CashAccountCode
					, Jd
					, Totalamount
					, K
				FROM
					#temp_Journal_4 AS ZZ
				WHERE 
					ZZ.K = @TargetDate
			) AS Z ON C.OfficeCode = Z.OfficeCode AND C.CashAccountCode = Z.CashAccountCode LEFT OUTER JOIN
			(
			SELECT 
				  OfficeCode
				, TotalAmount
				, CashAccountCode
				, M
				, Lastdate
				, K
			FROM 
				#temp_CashBalance_4 AS XX
			WHERE 
				XX.K = @TargetDate
			) AS X ON C.OfficeCode = X.OfficeCode AND C.CashAccountCode = X.CashAccountCode


	WHERE             
		C.DelFlag = '0' AND 
		O.OfficeCode IN 
		(
			SELECT
				OfficeCode
			FROM
				dbo.Department
			WHERE             
				(BusinessType IN ('001', '002'))
		)
	AND 
	(NOT (ISNULL(X.TotalAmount, 0) = 0 AND ISNULL(Y.Totalamount, 0) = 0 AND ISNULL(Z.Totalamount, 0) = 0)) OR
	                        (C.DelFlag = '0') AND (O.OfficeCode IN
	                            (SELECT            OfficeCode
	                               FROM              dbo.Department AS Department_1
	                               WHERE             (BusinessType IN ('001', '002')))) AND (NOT (ISNULL(X.TotalAmount, 0) + ISNULL(Y.Totalamount, 0) - ISNULL(Z.Totalamount, 0) = 0))
	ORDER BY
		  O.OfficeCode
		, C.CashAccountCode
	--PRINT @STRSQL

	BEGIN TRY
		DROP TABLE #temp_CashBalance_1
		DROP TABLE #temp_CashBalance_2
		DROP TABLE #temp_CashBalance_3
		DROP TABLE #temp_CashBalance_4
		DROP TABLE #temp_Journal_1
		DROP TABLE #temp_Journal_2
		DROP TABLE #temp_Journal_3
		DROP TABLE #temp_Journal_4
	END TRY
	BEGIN CATCH
		--����
	END CATCH
END


GO


