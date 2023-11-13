USE [WPH_DB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/19 arc yano #3721 �T�u�V�X�e���ڍs(�ԗ��ǐ�) �V�K�쐬
-- Description:	<Description,,>
-- �ԗ���{���̎擾
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetCarBasicInfo] 
	    @SalesCarNumber nvarchar(50) = ''			--�ԗ��Ǘ��ԍ�
	  , @Vin nvarchar(20) = ''						--�ԑ�ԍ�
AS 
BEGIN

	SET NOCOUNT ON;

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	/*-------------------------------------------*/
	/* �f�[�^�擾								 */
	/*-------------------------------------------*/
	
	SET @SQL = ''
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  S.SalesCarNumber AS SalesCarNumber' + @CRLF										--�Ǘ��ԍ�
	SET @SQL = @SQL +'	, S.Vin AS Vin' + @CRLF																--�ԑ�ԍ�
	SET @SQL = @SQL +'	, C1.Name as NewUsedName' + @CRLF													--�V���敪
    SET @SQL = @SQL +'	, L.LocationName AS LocationName' + @CRLF											--���P�[�V������
    SET @SQL = @SQL +'	, C2.Name AS CarStatusName' + @CRLF													--�݌ɃX�e�[�^�X
    SET @SQL = @SQL +'	, W.MakerName AS MakerName' + @CRLF													--���[�J�[��
	SET @SQL = @SQL +'	, W.CarBrandName AS CarBrandNam' + @CRLF											--�u�����h��
	SET @SQL = @SQL +'	, W.CarName AS CarName' + @CRLF														--�Ԏ햼
	SET @SQL = @SQL +'	, S.PossesorName AS PossesorName' + @CRLF											--���L��
	SET @SQL = @SQL +'	, S.UserName AS UserName' + @CRLF													--�g�p��
	SET @SQL = @SQL +'	, CASE WHEN S.DelFlag=''0'' THEN ''�L��'' ELSE ''����'' END AS DelName' + @CRLF		--�L���^����

	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	SalesCar S LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Location L ON S.LocationCode=L.LocationCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_NewUsedType C1 ON S.NewUsedType=C1.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_CarStatus C2 ON S.CarStatus=C2.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	V_CarMaster W ON S.CarGradeCode=W.CarGradeCode' + @CRLF
	
	--���������Ŏԑ�ԍ������͂���Ă���ꍇ
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    S.Vin like ''%' + @Vin + '%''' + @CRLF
	END
	--���������ŊǗ��ԍ����I������Ă���ꍇ
	ELSE IF (@SalesCarNumber is not NULL AND @SalesCarNumber <> '')
	BEGIN
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    S.SalesCarNumber like ''%' + @SalesCarNumber + '%''' + @CRLF
	END

	EXECUTE sp_executeSQL @SQL

	--DEBUG
	--PRINT @SQL

/*
	SELECT
	  CONVERT(nvarchar(50), '') AS SalesCarNumber
	, CONVERT(nvarchar(20), NULL) AS Vin
	, CONVERT(varchar(50), NULL) AS NewUsedName
	, CONVERT(nvarchar(50), NULL) AS LocationName
	, CONVERT(varchar(50), NULL) AS CarStatusName
	, CONVERT(nvarchar(50), NULL) AS MakerName
	, CONVERT(nvarchar(50), NULL) AS CarBrandName
	, CONVERT(nvarchar(20), NULL) AS CarName
	, CONVERT(nvarchar(80), NULL) AS PossesorName
	, CONVERT(nvarchar(80), NULL) AS UserName
	, CONVERT(nvarchar(5), NULL) AS DelName
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
			,'3721'--�`�P�b�g�ԍ�
			,'20170319_#3721_�T�u�V�X�e���ڍs�i�ԗ��ǐՁj/04_Create_Procedure_GetCarBasicInfo.sql'
			,CONVERT(datetime, '2017/03/??', 120)		--�������������������������[�X��(WPH�l����)����������������������
			,''--�R�����g
			,'arima.yuji'--���s��
			,GETDATE()--���s��
		)




