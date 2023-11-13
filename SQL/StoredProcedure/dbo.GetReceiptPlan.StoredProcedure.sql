USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetReceiptPlan]    Script Date: 2016/07/28 15:07:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--2015/02/09 arc yano #3153 �������у��X�g�Ή�
--Mod 2016/07/19 #3580_�����\��̃T�}���\���i�������у��X�g�o�́E�X�ܓ����E�����Ǘ��j

CREATE PROCEDURE [dbo].[GetReceiptPlan]
	@CustomerCode NVARCHAR(10),    --�ڋq�R�[�h
	@SlipNumber NVARCHAR(50),      --�`�[�ԍ�
	@SummaryFlag bit = 1		   --�T�}���Ŏ擾���邩�ǂ����i�O�F���Ȃ��@�P�F����j
AS

BEGIN
	/*-------------------------------------------*/
	/* �ꎞ�e�[�u��                              */
	/*-------------------------------------------*/
	-- �����\��ꗗ�e�[�u��
		CREATE TABLE #TempReceptPlan_T
		(
			ReceiptPlanId uniqueidentifier,
			SlipNumber nvarchar(50),
			ReceiptPlanDate  datetime,
			Code  varchar(3),
			Name  varchar(50),
			DepartmentCode  nvarchar(3),
			DepartmentName  nvarchar(20),
			CustomerClaimCode nvarchar(10),
			CustomerClaimName nvarchar(80),
			AccountCode nvarchar(50),
			AccountName nvarchar(50),
			Amount decimal(10, 0),
			ReceivableBalance decimal(10, 0),
			CompleteFlag nvarchar(2),
			OccurredDepartmentCode nvarchar(3),
			ReceiptType nvarchar(3)
		)
		

	/*-------------------------------------------*/
	/* �����\��ꗗ                              */
	/*-------------------------------------------*/
	DECLARE @STRSQL 		AS VARCHAR(900)	--���s����SQL��
	DECLARE @STRSELECT 		AS VARCHAR(400)	--SELECT��
	DECLARE @STRCOMMONWHERE	AS VARCHAR(200)	--WHERE��(����)
	DECLARE @STRWHERE 		AS VARCHAR(200)	--WHERE��(�����ɂ�镪��)
	DECLARE @STRORDER		AS VARCHAR(100)	--ORDER��

	if(@SummaryFlag = 0) --�T�}�����Ȃ�
	BEGIN
		--SELECT��̐ݒ�
		SET @STRSELECT = 'SELECT'
		SET @STRSELECT = @STRSELECT + ' [ReceiptPlanId]' 
		SET @STRSELECT = @STRSELECT + ',[SlipNumber]' 
		SET @STRSELECT = @STRSELECT + ',[ReceiptPlanDate]'
		SET @STRSELECT = @STRSELECT + ',[Code]'
		SET @STRSELECT = @STRSELECT + ',[Name]'
		SET @STRSELECT = @STRSELECT + ',[DepartmentCode]'
		SET @STRSELECT = @STRSELECT + ',[DepartmentName]'
		SET @STRSELECT = @STRSELECT + ',[CustomerClaimCode]'
		SET @STRSELECT = @STRSELECT + ',[CustomerClaimName]'
		SET @STRSELECT = @STRSELECT + ',[AccountCode]'
		SET @STRSELECT = @STRSELECT + ',[AccountName]'
		SET @STRSELECT = @STRSELECT + ',[Amount]'
		SET @STRSELECT = @STRSELECT + ',[ReceivableBalance]'
		SET @STRSELECT = @STRSELECT + ',[CompleteFlag]'
		SET @STRSELECT = @STRSELECT + ',[OccurredDepartmentCode]'
		SET @STRSELECT = @STRSELECT + ',[ReceiptType]'
		SET @STRSELECT = @STRSELECT + ' FROM V_ReceiptPlanList RList'

		--WHERE��(���ʕ���)�̐ݒ�
		SET @STRCOMMONWHERE = ' WHERE'
		SET @STRCOMMONWHERE = @STRCOMMONWHERE + ' ( DepartmentCode = OccurredDepartmentCode or ReceiptType = ''004'')'
		SET @STRCOMMONWHERE = @STRCOMMONWHERE + ' AND Left(DepartmentCode,1) <> ''0'''

		--ORDER��̐ݒ�
		SET @STRORDER = ' ORDER BY SlipNumber, ReceiptPlanDate, CustomerClaimCode'


		--�����ɂ��WHERE��̐ݒ�
		IF ((@SlipNumber IS NOT NULL) AND (@SlipNumber <> ''))
			BEGIN
				SET @STRWHERE = ' AND Left(SlipNumber, 8) = Left(''' + @SlipNumber + ''', 8)'
			END
		ELSE
			BEGIN
				SET @STRWHERE = ' AND EXISTS (' 
				SET @STRWHERE = @STRWHERE + ' SELECT ''X'' FROM V_ALL_SalesOrderList' 
				SET @STRWHERE = @STRWHERE + ' WHERE CustomerCode = ''' + @CustomerCode + ''''
				SET @STRWHERE = @STRWHERE + ' AND SlipNumber = Rlist.SlipNumber )'
			END

		SET @STRSQL = 'INSERT INTO #TempReceptPlan_T ' + @STRSELECT + @STRCOMMONWHERE + @STRWHERE + @STRORDER

		EXEC(@STRSQL)

	END
	ELSE --�T�}������
	BEGIN
		
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
		WHERE rp.[CustomerClaimCode] is not NULL
		  AND rp.[CustomerClaimCode] <> ''
		  AND rp.[DelFlag] = '0'
		  AND Left(rp.DepartmentCode,1) <> '0'
		  AND Left(rp.SlipNumber, 8) = Left(@SlipNumber, 8)

		GROUP BY  rp.[SlipNumber], rp.[CustomerClaimCode]
		ORDER BY SlipNumber,CustomerClaimCode


		INSERT INTO #TempReceptPlan_T
		SELECT NEWID() --ReceiptPlanId
			  ,r.SlipNumber
			  ,null --ReceiptPlanDate
			  ,''	--Code
			  ,''	--Name
			  ,''	--DepartmentCode
			  ,''	--DepartmentName
			  ,r.CustomerClaimCode
			  ,cc.CustomerClaimName
			  ,''	--AccountCode
			  ,''	--AccountName
			  ,r.Amount
			  ,r.ReceivableBalance
			  ,''	--CompleteFlag
			  ,''	--OccurredDepartmentCode
			  ,''	--ReceiptType

		FROM #temp_ReceiptPlan r
		INNER JOIN WPH_DB.dbo.CustomerClaim cc on cc.CustomerClaimCode = r.CustomerClaimCode
	END

	SELECT * FROM #TempReceptPlan_T

	--PRINT @STRSQL

	BEGIN TRY
		DROP TABLE #TempReceptPlan_T
		DROP TABLE #temp_ReceiptPlan
	END TRY
	BEGIN CATCH
		--����
	END CATCH
END






GO


