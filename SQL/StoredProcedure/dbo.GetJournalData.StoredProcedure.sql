USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetJournalData]    Script Date: 2015/06/30 11:28:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--Add 2015/06/24 arc yano �o���Ή��A �������я��̒��o
CREATE PROCEDURE [dbo].[GetJournalData]
	 @JournalDateFrom nvarchar(10),					--������(From)
	 @JournalDateTo   nvarchar(10)					--������(To)
	 	 
AS	
	BEGIN
		SET NOCOUNT ON;

		--/*
		DECLARE @SQL NVARCHAR(MAX) = ''
		DECLARE @PARAM NVARCHAR(1024) = ''
		DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

		--�����ꎞ�\�̐錾
		/*************************************************************************/
		
		--�������я��
		CREATE TABLE #Journal (
			[JournalDate]			DATETIME NOT NULL				--������
		,	[DepartmentCode]		NVARCHAR(3)						--����R�[�h
		,	[DepartmentName]		NVARCHAR(20)					--���喼
		,	[CustomerClaimCode]		NVARCHAR(10)					--������R�[�h
		,	[CustomerClaimName]		NVARCHAR(80)					--�����於
		,	[SlipNumber]			NVARCHAR(50)					--�`�[�ԍ�
		,	[Amount]				DECIMAL(10, 0)					--���z
		,	[AccountType]			VARCHAR(50)						--�������
		,	[Summary]				NVARCHAR(50)					--�E�v
		,	[AccountCode]			NVARCHAR(50)					--����ȖڃR�[�h
		,	[AccountName]			NVARCHAR(80)					--����Ȗږ�
		)
		
		--�̔��`�[���(�ԗ��^�T�[�r�X)
		CREATE TABLE #SalesOrder (
			[SlipNumber]	NVARCHAR(50) NOT NULL			--�`�[�ԍ�
		,	[OrderStatus]	NVARCHAR(50)					--�`�[�X�e�[�^�X(����)
		,	[SalesDate]		DATETIME						--�[�ԓ�
		,	[CustomerCode]	NVARCHAR(10)					--�ڋq�R�[�h
		,	[CustomerName]	NVARCHAR(80)					--�ڋq��
		)
		CREATE UNIQUE INDEX IX_Temp_SalesOrder ON #SalesOrder ([SlipNumber])

		--�_�[�e�B�[���[�h�̐ݒ�
		SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

		--�������я��(���t�ɂ��i�荞��)
		SET @PARAM = '@JournalDateFrom nvarchar(10), @JournalDateTo nvarchar(10)'

		SET @SQL = ''
		SET @SQL = @SQL +'INSERT INTO #Journal' + @CRLF
		SET @SQL = @SQL +'SELECT' + @CRLF
		SET @SQL = @SQL +'   jn.JournalDate AS JournalDate' + @CRLF
		SET @SQL = @SQL +' , jn.DepartmentCode AS DepartmentCode' + @CRLF
		SET @SQL = @SQL +' , dp.DepartmentName AS DepartmentName' + @CRLF
		SET @SQL = @SQL +' , jn.CustomerClaimCode AS CustomerClaimCode' + @CRLF
		SET @SQL = @SQL +' , cc.CustomerClaimName AS CustomerClaimName' + @CRLF
		SET @SQL = @SQL +' , jn.SlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL +' , jn.Amount AS Amount' + @CRLF
		SET @SQL = @SQL +' , at.Name AS AccountType' + @CRLF
		SET @SQL = @SQL +' , jn.Summary AS  Summary' + @CRLF
		SET @SQL = @SQL +' , jn.AccountCode AS AccountCode' + @CRLF
		SET @SQL = @SQL +' , ac.AccountName AS AccountName' + @CRLF
		SET @SQL = @SQL +'FROM' + @CRLF
		SET @SQL = @SQL +'	dbo.Journal jn INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.c_AccountType at ON at.Code = jn.AccountType INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.Department dp ON jn.departmentcode = dp.DepartmentCode INNER JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.Account ac ON jn.AccountCode = ac.AccountCode LEFT JOIN' + @CRLF
		SET @SQL = @SQL +'	dbo.CustomerClaim cc ON jn.CustomerClaimCode = cc.CustomerClaimCode' + @CRLF
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'  jn.JournalType = ''001''' + @CRLF
		SET @SQL = @SQL +'  AND jn.SlipNumber != ''''' + @CRLF
		SET @SQL = @SQL +'  AND jn.DelFlag = ''0''' + @CRLF
		

		--������
		IF ((@JournalDateFrom is not null) AND (@JournalDateFrom <> '') AND (ISDATE(@JournalDateFrom) = 1))
			IF ((@JournalDateTo is not null) AND (@JournalDateTo <> '') AND (ISDATE(@JournalDateTo) = 1) )
			BEGIN
				SET @SQL = @SQL +'AND jn.JournalDate >= @JournalDateFrom AND jn.JournalDate < DateAdd(d, 1, @JournalDateTo)' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND jn.JournalDate = @JournalDateFrom' + @CRLF 
			END
		ELSE
			IF ((@JournalDateTo is not null) AND (@JournalDateTo <> '') AND ISDATE(@JournalDateTo) = 1)
			BEGIN
				SET @SQL = @SQL +'AND jn.JournalDate < DateAdd(d, 1, @JournalDateTo)' + @CRLF 
			END
		
		--SQL���s
		EXECUTE sp_executeSQL @SQL, @PARAM, @JournalDateFrom, @JournalDateTo

		--For Debug
		--PRINT @SQL

		--�`�[���e�[�u���Ɏԗ��`�[�����Z�b�g
		INSERT INTO 
			#SalesOrder 
		SELECT 
			  csh.SlipNumber AS SlipNumber
			, ISNULL(sos.Name, '') AS OrderStatus
			, csh.SalesDate AS SalesDate
			, cu.CustomerCode AS CustomerCode
			, cu.CustomerName AS CustomerName
		FROM
			dbo.CarSalesHeader csh  LEFT JOIN
			dbo.c_SalesOrderStatus sos ON csh.SalesOrderStatus = sos.Code LEFT JOIN
			dbo.Customer cu ON csh.CustomerCode = cu.CustomerCode
		WHERE
			exists
			(
				SELECT 'X' FROM #Journal jn WHERE  jn.SlipNumber = csh.SlipNumber
			)
			AND
			csh.DelFlag = '0'

		--�`�[���e�[�u���ɃT�[�r�X�`�[�����Z�b�g
		INSERT INTO 
			#SalesOrder 
		SELECT 
			  ssh.SlipNumber AS SlipNumber
			, ISNULL(sos.Name, '') AS OrderStatus
			, ssh.SalesDate AS SalesDate
			, cu.CustomerCode AS CustomerCode
			, cu.CustomerName AS CustomerName
		FROM
			dbo.ServiceSalesHeader ssh  LEFT JOIN
			dbo.c_ServiceOrderStatus sos ON ssh.ServiceOrderStatus = sos.Code LEFT JOIN
			dbo.Customer cu ON ssh.CustomerCode = cu.CustomerCode 
		WHERE
			exists
			(
				SELECT 'X' FROM #Journal jn WHERE  jn.SlipNumber = ssh.SlipNumber
			)
			and
			ssh.DelFlag = '0'
		
		--�C���f�b�N�X�Đ���
		DROP INDEX IX_Temp_SalesOrder ON #SalesOrder
		CREATE UNIQUE INDEX IX_Temp_SalesOrder ON #SalesOrder ([SlipNumber])


		--�ꎞ�e�[�u�����f�[�^���擾
		SELECT
			  jn.JournalDate as JournalDate
			, jn.DepartmentCode as DepartmentCode
			, jn.DepartmentName as DepartmentName
			, jn.CustomerClaimCode as CustomerClaimCode
			, jn.CustomerClaimName as CustomerClaimName
			, jn.SlipNumber as SlipNumber
			, so.OrderStatus AS OrderStatus
			, so.SalesDate AS SalesDate
			, so.CustomerCode AS CustomerCode
			, so.CustomerName AS CustomerName
			, jn.Amount AS Amount
			, jn.AccountType AS AccountType
			, jn.Summary AS  Summary
			, jn.AccountCode AS AccountCode
			, jn.AccountName as AccountName

		FROM
			#Journal jn LEFT JOIN
			#SalesOrder so ON jn.SlipNumber = so.SlipNumber
		ORDER BY
			  jn.JournalDate
			, jn.DepartmentCode
			, jn.SlipNumber
--/*
		BEGIN TRY
			DROP TABLE #Journal
			DROP TABLE #SalesOrder
		END TRY
		BEGIN CATCH
			--����
		END CATCH
--*/
	END
GO


