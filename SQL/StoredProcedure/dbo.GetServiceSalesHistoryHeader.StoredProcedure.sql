USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetServiceSalesHistoryHeader]    Script Date: 2017/03/16 18:44:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/13 arc yano #3725 �T�u�V�X�e���ڍs(��������) �V�K�쐬
-- Description:	<Description,,>
-- ���������̎擾
-- ======================================================================================================================================
USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetServiceSalesHistoryHeader]    Script Date: 2017/03/27 19:46:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/13 arc yano #3725 �T�u�V�X�e���ڍs(��������) �V�K�쐬
-- Description:	<Description,,>
-- ���������̎擾
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetServiceSalesHistoryHeader] 

	@DivType nvarchar(3) = '018',					--CJ/FA
	@DepartmentName varchar(50) = '',				--���喼
	@SlipNumber varchar(50) = '',				    --�`�[�ԍ�
	@vin nvarchar(50) = '',							--�ԑ�ԍ�
	@RegistNumber varchar(50) = '',					--�o�^�i���o�[
	@CustomerName varchar(50) = '',					--�ڋq��
	@CustomerNameKana varchar(50) = ''				--�ڋq��(����)
AS 
BEGIN

	SET NOCOUNT ON;

	
	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @TableName nvarchar(50)				--��������e�[�u�����i�w�b�_�j

	IF @DivType = '019'
		SET @TableName = 'T_CM_ServiceSalesHeader'
	ELSE
		SET @TableName = 'T_FC_ServiceSalesHeader'
	
	/*-------------------------------------------*/
	/* �f�[�^�}��								 */
	/*-------------------------------------------*/
	SET @SQL = ''
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  DepartmentName AS H_DepartmentName' + @CRLF
	SET @SQL = @SQL +'	, SystemName AS H_SystemName' + @CRLF
	SET @SQL = @SQL +'	, SlipNumber AS H_SlipNumber' + @CRLF
    SET @SQL = @SQL +'	, SalesInputDate AS H_SalesInputDate' + @CRLF
    SET @SQL = @SQL +'	, RedSlipType AS H_RedSlipType' + @CRLF
    SET @SQL = @SQL +'	, CustomerCode AS H_CustomerCode' + @CRLF
	SET @SQL = @SQL +'	, CustomerNameKana AS H_CustomerNameKana' + @CRLF
	SET @SQL = @SQL +'	, CustomerName1 AS H_CustomerName1' + @CRLF
	SET @SQL = @SQL +'	, CustomerName2 AS H_CustomerName2' + @CRLF
	SET @SQL = @SQL +'	, RegistName AS H_RegistName' + @CRLF
	SET @SQL = @SQL +'	, RegistType AS H_RegistType' + @CRLF
	SET @SQL = @SQL +'	, RegistNumberKana AS H_RegistNumberKana' + @CRLF
	SET @SQL = @SQL +'	, RegistNumber AS H_RegistNumber' + @CRLF
	SET @SQL = @SQL +'	, ServiceWorkCode AS H_ServiceWorkCode' + @CRLF
	SET @SQL = @SQL +'	, ServiceWorkName AS H_ServiceWorkName' + @CRLF
	SET @SQL = @SQL +'	, FrontEmployeeCode AS H_FrontEmployeeCode' + @CRLF
	SET @SQL = @SQL +'	, FrontEmployeeName AS H_FrontEmployeeName' + @CRLF
	SET @SQL = @SQL +'	, MechanicEmployeeCode AS H_MechanicEmployeeCode' + @CRLF
	SET @SQL = @SQL +'	, MechanicEmployeeName AS H_MechanicEmployeeName' + @CRLF
	SET @SQL = @SQL +'	, TechnicalFeeAmount AS H_TechnicalFeeAmount' + @CRLF
	SET @SQL = @SQL +'	, PartsAmount AS H_PartsAmount' + @CRLF
	SET @SQL = @SQL +'	, VariousAmount AS H_VariousAmount' + @CRLF
	SET @SQL = @SQL +'	, TechnicalFeeTaxAmount AS H_TechnicalFeeTaxAmount' + @CRLF
	SET @SQL = @SQL +'	, VariousTaxAmount AS H_VariousTaxAmount' + @CRLF
	SET @SQL = @SQL +'	, TotalTaxAmount AS H_TotalTaxAmount' + @CRLF
	SET @SQL = @SQL +'	, RunningData AS H_RunningData' + @CRLF
	SET @SQL = @SQL +'	, vin AS H_Vin' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL + @TableName + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    CustomerName1 IS NOT NULL' + @CRLF
	
	--���_�ɂ��i������������ꍇ
	IF (@DepartmentName is not null AND @DepartmentName <> '')
	BEGIN
		SET @SQL = @SQL +'  AND DepartmentName LIKE ''%' + @DepartmentName + '%''' + @CRLF
	END
	--�`�[�ԍ��ɂ��i������������ꍇ
	IF (@SlipNumber is not null AND @SlipNumber <> '')
	BEGIN
		SET @SQL = @SQL +'  AND SlipNumber = ''' + @SlipNumber + '''' + @CRLF
	END 
	--�ԑ�ԍ��ɂ��i������������ꍇ
	IF (@vin is not null AND @vin <> '')
	BEGIN
		SET @SQL = @SQL +'  AND vin LIKE ''%' + @vin + '%''' + @CRLF
	END 
	--�i���o�[�v���[�g�ɂ��i������������ꍇ
	IF (@RegistNumber is not null AND @RegistNumber <> '')
	BEGIN
		SET @SQL = @SQL +'  AND ' + @CRLF
		SET @SQL = @SQL +'	(' + @CRLF
		SET @SQL = @SQL +'		RTRIM(RegistName) LIKE ''%' + @RegistNumber + '%''' + @CRLF
		SET @SQL = @SQL +'		OR ' + @CRLF
		SET @SQL = @SQL +'		RTRIM(RegistType) LIKE ''%' + @RegistNumber + '%''' + @CRLF
		SET @SQL = @SQL +'		OR ' + @CRLF
		SET @SQL = @SQL +'		RTRIM(RegistNumberKana) LIKE ''%' + @RegistNumber + '%''' + @CRLF
		SET @SQL = @SQL +'		OR ' + @CRLF
		SET @SQL = @SQL +'		RTRIM(RegistNumber) LIKE ''%' + @RegistNumber + '%''' + @CRLF
	
		SET @SQL = @SQL +'  )' + @CRLF
	END
	--�ڋq���ɂ��i������������ꍇ
	IF (@CustomerName is not null AND @CustomerName <> '')
	BEGIN
		SET @SQL = @SQL +'  AND ' + @CRLF
		SET @SQL = @SQL +'  (' + @CRLF
		SET @SQL = @SQL +'		CustomerName1 LIKE ''%' + @CustomerName + '%''' + @CRLF
		SET @SQL = @SQL +'		OR' + @CRLF
		SET @SQL = @SQL +'		CustomerName2 LIKE ''%' + @CustomerName + '%''' + @CRLF
		SET @SQL = @SQL +'  )' + @CRLF
	END 
		--�ڋq���i���ȁj�ɂ��i������������ꍇ
	IF (@CustomerNameKana is not null AND @CustomerNameKana <> '')
	BEGIN
		SET @SQL = @SQL +'  AND CustomerNameKana LIKE ''%' + @CustomerNameKana + '%''' + @CRLF
	END 
	
	SET @SQL = @SQL +'  ORDER BY ' + @CRLF	
	SET @SQL = @SQL +'		SalesInputDate desc' + @CRLF

	EXECUTE sp_executeSQL @SQL
	/*
	SELECT
		  CONVERT(varchar(50), NULL) AS H_DepartmentName
		, CONVERT(varchar(50), NULL) AS H_SystemName
		, CONVERT(varchar(50), NULL) AS H_SlipNumber
		, CONVERT(varchar(50), NULL) AS H_SalesInputDate
		, CONVERT(varchar(50), NULL) AS H_RedSlipType
		, CONVERT(varchar(50), NULL) AS H_CustomerCode
		, CONVERT(varchar(50), NULL) AS H_CustomerNameKana
		, CONVERT(varchar(50), NULL) AS H_CustomerName1
		, CONVERT(varchar(50), NULL) AS H_CustomerName2
		, CONVERT(varchar(50), NULL) AS H_RegistName
		, CONVERT(varchar(50), NULL) AS H_RegistType
		, CONVERT(varchar(50), NULL) AS H_RegistNumberKana
		, CONVERT(varchar(50), NULL) AS H_RegistNumber
		, CONVERT(varchar(50), NULL) AS H_ServiceWorkCode
		, CONVERT(varchar(50), NULL) AS H_ServiceWorkName
		, CONVERT(varchar(50), NULL) AS H_FrontEmployeeCode
		, CONVERT(varchar(50), NULL) AS H_FrontEmployeeName
		, CONVERT(varchar(50), NULL) AS H_MechanicEmployeeCode
		, CONVERT(varchar(50), NULL) AS H_MechanicEmployeeName
		, CONVERT(varchar(50), NULL) AS H_TechnicalFeeAmount
		, CONVERT(varchar(50), NULL) AS H_PartsAmount
		, CONVERT(varchar(50), NULL) AS H_VariousAmount
		, CONVERT(varchar(50), NULL) AS H_TechnicalFeeTaxAmount
		, CONVERT(varchar(50), NULL) AS H_VariousTaxAmount
		, CONVERT(varchar(50), NULL) AS H_TotalTaxAmount
		, CONVERT(varchar(50), NULL) AS H_RunningData
		, CONVERT(varchar(50), NULL) AS H_Vin
		, CONVERT(varchar(50), NULL) AS L_DepartmentName
		, CONVERT(varchar(50), NULL) AS L_SystemName
		, CONVERT(varchar(50), NULL) AS L_SlipNumber
		, CONVERT(varchar(50), NULL) AS L_LineNumber
		, CONVERT(varchar(50), NULL) AS L_SalesInputDate
		, CONVERT(varchar(50), NULL) AS L_RedSlipType
		, CONVERT(varchar(50), NULL) AS L_CustomerCode
		, CONVERT(varchar(50), NULL) AS L_CustomerNameKana
		, CONVERT(varchar(50), NULL) AS L_CustomerName1
		, CONVERT(varchar(50), NULL) AS L_CustomerName2
		, CONVERT(varchar(50), NULL) AS L_RegistName
		, CONVERT(varchar(50), NULL) AS L_RegistType
		, CONVERT(varchar(50), NULL) AS L_RegistNumberKana
		, CONVERT(varchar(50), NULL) AS L_RegistNumber
		, CONVERT(varchar(50), NULL) AS L_ServiceWorkCode
		, CONVERT(varchar(500), NULL) AS L_ServiceWorkName
		, CONVERT(varchar(500), NULL) AS L_ContentsName
		, CONVERT(varchar(500), NULL) AS L_ContentsType
		, CONVERT(varchar(500), NULL) AS L_FrontEmployeeCode
		, CONVERT(varchar(50), NULL) AS L_FrontEmployeeName
		, CONVERT(varchar(50), NULL) AS L_MechanicEmployeeCode
		, CONVERT(varchar(50), NULL) AS L_MechanicEmployeeName
		, CONVERT(varchar(50), NULL) AS L_MechanicEmployeeCodeDetail
		, CONVERT(varchar(50), NULL) AS L_MechanicEmployeeNameDetaL
		, CONVERT(varchar(50), NULL) AS L_Quantity
		, CONVERT(varchar(50), NULL) AS L_TechnicalFeeAmount
		, CONVERT(varchar(50), NULL) AS L_PartsAmount
		, CONVERT(varchar(50), NULL) AS L_VariousAmount
		, CONVERT(varchar(50), NULL) AS L_ServiceName
		, CONVERT(varchar(50), NULL) AS L_TotalTechnicalFeeAmount
		, CONVERT(varchar(50), NULL) AS L_TotalPartsAmount
		, CONVERT(varchar(50), NULL) AS L_TotalVariousAmount
		, CONVERT(varchar(50), NULL) AS L_TechnicalFeeTaxAmount
		, CONVERT(varchar(50), NULL) AS L_VariousTaxAmount
		, CONVERT(varchar(50), NULL) AS L_TotalTaxAmount
		, CONVERT(varchar(50), NULL) AS L_RunnnigData
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
			,'3725'--�`�P�b�g�ԍ�
			,'20170313_#3725_�T�u�V�X�e���ڍs�i���������j/08_Create_Procedure_GetServiceSalesHistoryHeader.sql'
			,CONVERT(datetime, '2017/03/??', 120)		--�������������������������[�X��(WPH�l����)����������������������
			,''--�R�����g
			,'arima.yuji'--���s��
			,GETDATE()--���s��
		)

