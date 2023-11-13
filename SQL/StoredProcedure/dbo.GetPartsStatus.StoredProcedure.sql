USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsStatus]    Script Date: 2017/11/07 16:29:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/16 arc yano #3726 �T�u�V�X�e���ڍs(�p�[�c�X�e�[�^�X) �V�K�쐬
-- Update date: <Update Date,,>
-- 2017/11/06 arc yano #3809 �p�[�c�X�e�[�^�X�Ǘ��@�����ϐ��̒ǉ�
-- Description:	<Description,,>
-- ���������̎擾
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetPartsStatus] 
	  @Target nvarchar(1)							--�����Ώۍ���(0:�w�薳�� 1:���ɓ� 2:��ƊJ�n�� 3:�[�ԓ�)
	, @TargetDateFrom datetime						--�Ώ۔N��From
	, @DepartmentCode nvarchar(3)					--����R�[�h
	, @ServiceOrderStatus nvarchar(3)				--�`�[�X�e�[�^�X
	, @PartsNumber nvarchar(25)						--���i�ԍ�
AS 
BEGIN

--/*

	SET NOCOUNT ON;

	
	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @TableNameLine nvarchar(50)			--��������e�[�u�����i����

	--�Ώ۔N��To��Ώ۔N��From��1������ɐݒ�
	DECLARE @TargetDateTo datetime = DATEADD(m, 1, @TargetDateFrom)		--�Ώ۔N��To
		


	/*-------------------------------------------*/
	/* �f�[�^�擾								 */
	/*-------------------------------------------*/
	SET @SQL = ''
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  H.SlipNumber AS SlipNumber' + @CRLF
	SET @SQL = @SQL +'	, H.ServiceOrderStatus AS ServiceOrderStatus' + @CRLF
	SET @SQL = @SQL +'	, Left(c.Name,5) AS ServiceOrderStatusName' + @CRLF
	SET @SQL = @SQL +'	, L.PartsNumber AS PartsNumber' + @CRLF
	SET @SQL = @SQL +'	, L.LineContents AS LineContents' + @CRLF
	SET @SQL = @SQL +'	, L.Quantity AS Quantity' + @CRLF
	SET @SQL = @SQL +'	, L.ProvisionQuantity AS ProvisionQuantity' + @CRLF			--Add 2017/11/06 arc yano #3809
	SET @SQL = @SQL +'	, H.ArrivalPlanDate AS ArrivalPlanDate' + @CRLF
	SET @SQL = @SQL +'	, H.SalesOrderDate AS SalesOrderDate' + @CRLF
	SET @SQL = @SQL +'	, H.WorkingStartDate AS WorkingStartDate' + @CRLF
	SET @SQL = @SQL +'	, H.WorkingEndDate AS WorkingEndDate' + @CRLF
	SET @SQL = @SQL +'	, H.SalesDate AS SalesDate' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	ServiceSalesHeader H INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	ServiceSalesLine L ON H.SlipNumber=L.SlipNumber AND H.RevisionNumber=L.RevisionNumber INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	c_ServiceOrderStatus C ON H.ServiceOrderStatus = C.Code' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	H.DelFlag = ''0''' + @CRLF
	SET @SQL = @SQL +'	AND L.DelFlag = ''0''' + @CRLF
	SET @SQL = @SQL +'	AND L.ServiceType = ''003''' + @CRLF

	--���t�̍i����
	IF (@Target is not null AND @Target <> '')
	BEGIN
		IF (@Target =  '1')				--���ɓ�
		BEGIN
			SET @SQL = @SQL +'	AND H.ArrivalPlanDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
			SET @SQL = @SQL +'	AND H.ArrivalPlanDate < CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
		END
		ELSE IF (@Target =  '2')				--�󒍓�
		BEGIN
			SET @SQL = @SQL +'	AND H.SalesOrderDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
			SET @SQL = @SQL +'	AND H.SalesOrderDate < CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
		END
		ELSE IF(@Target =  '3')			--��ƊJ�n��
		BEGIN
			SET @SQL = @SQL +'	AND H.WorkingStartDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
			SET @SQL = @SQL +'	AND H.WorkingStartDate < CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
		END
		ELSE IF(@Target = '4')			--��ƏI����
		BEGIN
			SET @SQL = @SQL +'	AND H.WorkingEndDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
			SET @SQL = @SQL +'	AND H.WorkingEndDate < CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
		END
		ELSE IF(@Target = '5')			--�[�ԓ�
		BEGIN
			SET @SQL = @SQL +'	AND H.SalesDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateFrom) + ''')' + @CRLF
			SET @SQL = @SQL +'	AND H.SalesDate < CONVERT(datetime, ''' + CONVERT(nvarchar, @TargetDateTo) + ''')' + @CRLF
		END
	END

	--����R�[�h�ɂ��i����
	IF (@DepartmentCode is not null AND @DepartmentCode <> '')
	BEGIN
		SET @SQL = @SQL +'	AND H.DepartmentCode = ''' + @DepartmentCode + '''' + @CRLF
	END

	--�`�[�X�e�[�^�X�ɂ��i����
	IF (@ServiceOrderStatus is not null AND @ServiceOrderStatus <> '')
	BEGIN
		SET @SQL = @SQL +'	AND H.ServiceOrderStatus = ''' + @ServiceOrderStatus + '''' + @CRLF
	END

	--���i�ԍ��ɂ��i����
	IF (@PartsNumber is not null AND @PartsNumber <> '')
	BEGIN
		SET @SQL = @SQL +'	AND L.PartsNumber = ''' + @PartsNumber + '''' + @CRLF
	END

	SET @SQL = @SQL +'	ORDER BY' + @CRLF
	SET @SQL = @SQL +'	H.SlipNumber, L.LineNumber' + @CRLF

	EXECUTE sp_executeSQL @SQL
--*/
/*

	SELECT
		  CONVERT(nvarchar(50), '') AS SlipNumber
		, CONVERT(nvarchar(3), NULL) AS ServiceOrderStatus
		, CONVERT(varchar(50), NULL) AS ServiceOrderStatusName
		, CONVERT(nvarchar(25), NULL) AS PartsNumber
		, CONVERT(nvarchar(50), NULL) AS LineContents
		, CONVERT(decimal(10, 2), NULL) AS Quantity
		, CONVERT(decimal(10, 2), NULL) AS ProvisionQuantity
		, CONVERT(datetime, NULL) AS ArrivalPlanDate
		, CONVERT(datetime, NULL) AS SalesOrderDate
		, CONVERT(datetime, NULL) AS WorkingStartDate
		, CONVERT(datetime, NULL) AS WorkingEndDate
		, CONVERT(datetime, NULL) AS SalesDate
*/		

END




GO


