USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCashBalance]    Script Date: 2015/03/31 11:48:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






--Add 2015/03/16 arc yano �����o�[���G�N�Z����
CREATE PROCEDURE [dbo].[GetCashBalance]
	 @TargetYear int,						--�Ώ۔N��(�N)
	 @TargetMonth int,						--�Ώ۔N��(��)
	 @OfficeCode nvarchar(3),				--��ЃR�[�h
	 @CashAccountCode nvarchar(3)			--���������R�[�h
	 	 
AS	
	DECLARE @CNT INT = 0

	--�����ݍ��擾
	DECLARE @targetDate datetime			--�Ώ۔N��
	DECLARE @tmpTargetDate datetime			--�Ώ۔N��(��Ɨp)
	DECLARE @preDate datetime				--�Ώ۔N���̑O��
	DECLARE @followDate datetime			--�Ώ۔N���̎���
	DECLARE @DAYS int						--�Ώ۔N���̓���


	--�Ώ۔N���̌���	
	SET @targetDate = CONVERT(datetime, (STR(@TargetYear) + '/' +  STR(@TargetMonth) + '/01'), 120)

	--�Ώ۔N���̎����̌���
	SET @followDate = DATEADD(m, 1 ,@targetDate)
	
	SET @tmpTargetDate = @targetDate

	--�Ώ۔N���̓���
	SET @DAYS = DATEDIFF(d, @targetDate, @followDate)

	BEGIN
	
		/*-------------------------------------------*/
		/* �����ݍ�					�@               */
		/*-------------------------------------------*/

		--�ꎞ�e�[�u���̕ۑ�
		CREATE TABLE #temp_CashBalance (
		  OfficeCode nvarchar(3)
		, ClosedDate datetime
		, CloseFlag nvarchar(2)
		, NumberOf10000 int
		, NumberOf5000 int
		, NumberOf2000 int
		, NumberOf1000 int
		, NumberOf500 int
		, NumberOf100 int
		, NumberOf50 int
		, NumberOf10 int
		, NumberOf5 int
		, NumberOf1 int
		, CheckAmount decimal(10, 0)
		, DelFlag nvarchar(2)
		, CashAccountCode nvarchar(3)
	)

		--�Ώ۔N���̂P�������̋�f�[�^��}��
		WHILE( @CNT < @DAYS)
			BEGIN
				INSERT INTO #temp_CashBalance
				
				SELECT TOP 1
					 @OfficeCode
					,DATEADD(d, @CNT, @tmpTargetDate)
					,'0'
					, 0
					, 0
					, 0
					, 0
					, 0
					, 0
					, 0
					, 0
					, 0
					, 0
					, 0
					, '0'
					,@CashAccountCode
				FROM
					dbo.CashBalance
			
			SET @CNT = @CNT + 1
			END

		--�C���f�b�N�X�͕s�v


		--dbo.CashBalance�̎��f�[�^�ƌ�������B

		SELECT
			  ISNULL(cb.NumberOf10000, 0) AS NumberOf10000
			, ISNULL(cb.NumberOf5000, 0) AS NumberOf5000
			, ISNULL(cb.NumberOf2000, 0) AS NumberOf2000
			, ISNULL(cb.NumberOf1000, 0) AS NumberOf1000
			, ISNULL(cb.NumberOf500, 0) AS NumberOf500
			, ISNULL(cb.NumberOf100, 0) AS NumberOf100
			, ISNULL(cb.NumberOf50, 0) AS NumberOf50
			, ISNULL(cb.NumberOf10, 0) AS NumberOf10
			, ISNULL(cb.NumberOf5, 0) AS NumberOf5
			, ISNULL(cb.NumberOf1, 0) AS NumberOf1
			, CASE WHEN cb.CloseFlag = '1' THEN '��' ELSE '' END AS ClosedStatus
			, ISNULL(cb.CheckAmount, 0) AS CheckAmount

		FROM
			#temp_CashBalance tcb LEFT OUTER JOIN
			dbo.CashBalance cb ON (tcb.OfficeCode = cb.OfficeCode) AND (tcb.CashAccountCode = cb.CashAccountCode) AND (tcb.ClosedDate = cb.ClosedDate) AND (cb.DelFlag = '0')
		ORDER BY
			tcb.ClosedDate
	END


GO


