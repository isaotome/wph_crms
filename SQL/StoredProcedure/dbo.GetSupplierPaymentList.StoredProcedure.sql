USE [WPH_DB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/23 arc yano #3729 �T�u�V�X�e���ڍs�i�O���x���ꗗ�j
-- Description:	<Description,,>
-- �O���x���ꗗ�̎擾
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetSupplierPaymentList] 
	    @Target nvarchar(1) = '0'						--�����Ώ�(0:�󒍓� 1:�[�ԓ�)
	  , @TargetDateFrom datetime						--�Ώ۔N��(From)
	  , @TargetDateTo datetime							--�Ώ۔N��(To)
	  , @DepartmentCode nvarchar(3)						--����R�[�h
	  , @ServiceWorkCode nvarchar(5)					--���ƃR�[�h
	  , @SlipNumber nvarchar(50)						--�`�[�ԍ�
	  , @Vin nvarchar(20)								--�`�[�ԍ�
	  , @CustomerCode nvarchar(10)						--�ڋq�R�[�h
	  , @CustomerName nvarchar(80)						--�ڋq��
	  , @SupplierCode nvarchar(10)						--�O����R�[�h
	  , @SupplierName nvarchar(80)						--�O���於
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	/*-------------------------------------------*/
	/* �f�[�^�ݒ�								 */
	/*-------------------------------------------*/
	
	SET @SQL = '' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  L.SupplierCode AS SupplierCode' + @CRLF								--�O����R�[�h
	SET @SQL = @SQL +'	, S.SupplierName AS SupplierName' + @CRLF								--�O���於
	SET @SQL = @SQL +'	, H.SlipNumber AS SlipNumber' + @CRLF								--����R�[�h
	SET @SQL = @SQL +'	, W.Name AS ServiceWorkName' + @CRLF									--���Ɩ�
    SET @SQL = @SQL +'	, L.LineContents AS LineContents' + @CRLF								--���ז���
    SET @SQL = @SQL +'	, L.TechnicalFeeAmount AS TechnicalFeeAmount' + @CRLF					--�Z�p��
    SET @SQL = @SQL +'	, L.Cost AS Cost' + @CRLF												--����
	SET @SQL = @SQL +'	, CC.CustomerClaimName AS CustomerClaimName' + @CRLF					--�����於
	SET @SQL = @SQL +'	, C.CustomerName AS CustomerName' + @CRLF								--�ڋq��
	SET @SQL = @SQL +'	, C1.Name AS ServiceOrderStatusName' + @CRLF							--�`�[�X�e�[�^�X��
	SET @SQL = @SQL +'	, H.DepartmentCode AS DepartmentCode' + @CRLF							--����R�[�h
	SET @SQL = @SQL +'	, D.DepartmentName AS DepartmentName' + @CRLF							--���喼
	SET @SQL = @SQL +'	, H.SalesDate AS SalesDate' + @CRLF										--�[�ԓ�
	SET @SQL = @SQL +'	, H.SalesOrderDate AS SalesOrderDate' + @CRLF							--�󒍓�
	SET @SQL = @SQL +'	, H.Vin AS Vin' + @CRLF													--�ԑ�ԍ�
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	ServiceSalesHeader H INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	ServiceSalesLine L ON H.SlipNumber=L.SlipNumber and H.RevisionNumber=L.RevisionNumber INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	Department D ON H.DepartmentCode=D.DepartmentCode INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	c_ServiceOrderStatus C1 ON H.ServiceOrderStatus=C1.Code INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	Supplier S ON L.SupplierCode=S.SupplierCode LEFT OUTER JOIN' + @CRLF
	SET @SQL = @SQL +'	Customer C ON H.CustomerCode=C.CustomerCode LEFT OUTER JOIN' + @CRLF
	SET @SQL = @SQL +'	CustomerClaim CC ON L.CustomerClaimCode=CC.CustomerClaimCode LEFT OUTER JOIN' + @CRLF
	SET @SQL = @SQL +'	ServiceWork W ON L.ServiceWorkCode=W.ServiceWorkCode' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	H.DelFlag = ''0''' + @CRLF
	SET @SQL = @SQL +'	AND L.ServiceType=''002''' + @CRLF
	SET @SQL = @SQL +'	AND L.SupplierCode is not NULL' + @CRLF
	
	--���������Ŕ[�ԓ����I������Ă���ꍇ
	IF (@Target = '0')
	BEGIN
		SET @SQL = @SQL +' AND H.SalesDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
		SET @SQL = @SQL +' AND H.SalesDate <= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
	END
	--���������Ŏ󒍓����I������Ă���ꍇ
	ELSE
	BEGIN
		SET @SQL = @SQL +' AND H.SalesOrderDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
		SET @SQL = @SQL +' AND H.SalesOrderDate <= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
	END
	--���������ŕ���R�[�h�����͂���Ă���ꍇ
	IF (@DepartmentCode is not NULL AND @DepartmentCode <> '')
	BEGIN
		SET @SQL = @SQL +' AND H.DepartmentCode = ''' + @DepartmentCode + '''' + @CRLF
	END
	--���������œ`�[�ԍ������͂���Ă���ꍇ
	IF (@SlipNumber is not NULL AND @SlipNumber <> '')
	BEGIN
		SET @SQL = @SQL +' AND H.SlipNumber = ''' + @SlipNumber + '''' + @CRLF
	END
	--���������Ŏԑ�ԍ������͂���Ă���ꍇ
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +' AND H.Vin LIKE ''%' + @Vin + '%''' + @CRLF
	END
	--���������Ōڋq�R�[�h�����͂���Ă���ꍇ
	IF (@CustomerCode is not NULL AND @CustomerCode <> '')
	BEGIN
		SET @SQL = @SQL +' AND H.CustomerCode = ''' + @CustomerCode + '''' + @CRLF
	END
	--���������Ōڋq�������͂���Ă���ꍇ
	IF (@CustomerName is not NULL AND @CustomerName <> '')
	BEGIN
		SET @SQL = @SQL +'  AND C.CustomerName like ''%' + @CustomerName + '%''' + @CRLF
	END
	--���������ŊO���R�[�h�����͂���Ă���ꍇ
	IF (@SupplierCode is not NULL AND @SupplierCode <> '')
	BEGIN
		SET @SQL = @SQL +' AND L.SupplierCode = ''' + @SupplierCode + '''' + @CRLF
	END
	--���������ŊO���������͂���Ă���ꍇ
	IF (@SupplierName is not NULL AND @SupplierName <> '')
	BEGIN
		SET @SQL = @SQL +'  AND S.SupplierName like ''%' + @SupplierName + '%''' + @CRLF
	END
	--���������Ŏ��Ƃ��I������Ă���ꍇ
	IF (@ServiceWorkCode is not NULL AND @ServiceWorkCode <> '')
	BEGIN
		SET @SQL = @SQL +' AND L.ServiceWorkCode = ''' + @ServiceWorkCode + '''' + @CRLF
	END

	--�\�[�g
	SET @SQL = @SQL +'ORDER BY ' + @CRLF
	SET @SQL = @SQL +'	 L.SupplierCode ' + @CRLF
	SET @SQL = @SQL +'	,H.DepartmentCode ' + @CRLF
	SET @SQL = @SQL +'	,H.SlipNumber ' + @CRLF
	SET @SQL = @SQL +'	,L.LineNumber ' + @CRLF


	--DEBUG
	--PRINT @SQL

	EXECUTE sp_executeSQL @SQL
				
/*
	SELECT
	  CONVERT(nvarchar(10), '') AS SupplierCode
	, CONVERT(nvarchar(80), NULL) AS SupplierName
	, CONVERT(nvarchar(50), NULL) AS SlipNumber
	, CONVERT(nvarchar(20), NULL) AS ServiceWorkName
	, CONVERT(nvarchar(50), NULL) AS LineContents
	, CONVERT(decimal(10, 0), NULL) AS TechnicalFeeAmount
	, CONVERT(decimal(10, 0), NULL) AS Cost
	, CONVERT(nvarchar(80), NULL) AS CustomerClaimName
	, CONVERT(nvarchar(80), NULL) AS CustomerName
	, CONVERT(nvarchar(50), NULL) AS ServiceOrderStatusName
	, CONVERT(nvarchar(3), NULL) AS DepartmentCode
	, CONVERT(nvarchar(20), NULL) AS DepartmentName
	, CONVERT(datetime, NULL) AS SalesDate
	, CONVERT(datetime, NULL) AS SalesOrderDate
	, CONVERT(nvarchar(20), NULL) AS Vin
*/
END


GO
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
			,'3729'--�`�P�b�g�ԍ�
			,'20170323_#3729_�T�u�V�X�e���ڍs�i�O���x���ꗗ�j/04_Create_Procedure_GetSupplierPaymentList.sql'
			,CONVERT(datetime, '2017/03/??', 120)		--�������������������������[�X��(WPH�l����)����������������������
			,''--�R�����g
			,'arima.yuji'--���s��
			,GETDATE()--���s��
		)




