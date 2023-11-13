USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCarOptionMaster]    Script Date: 2016/03/07 16:32:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- 2016/02/09 arc nakayama �ԗ��`�[�쐬���̃I�v�V�����̃f�t�H���g�ݒ� �V�K�쐬
-- 2016/03/07 arc nakayama �ԗ��`�[�쐬���̃I�v�V�����̃f�t�H���g�ݒ� �Ԏ�R�[�h����O���[�h�R�[�h�ɕύX

CREATE PROCEDURE [dbo].[GetCarOptionMaster]

	@MakerCode nvarchar(5),			--���[�J�[�R�[�h
	@MakerName nvarchar(50),		--���[�J�[��
	@CarOptionCode nvarchar(25),	--�I�v�V�����R�[�h
	@CarOptionName nvarchar(100),	--�I�v�V������
	@CarGradeCode nvarchar(30),		--�O���[�h�R�[�h
	@RequiredFlag nvarchar(2),		--�K�{�t���O
	@DelFlag nvarchar(2),			--�폜�t���O
	@ActionFlag nvarchar(2)			--�A�N�V�����t���O�i0:�}�X�^��ʌ����@1:�_�C�A���O�����j

AS
	SET NOCOUNT ON
	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)
	
	/*-------------------------------------------*/
	/* �_�[�e�B�[���[�h�̐ݒ�					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;


	/*-------------------------------------------*/
	/* ���[�J�[���								 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_Maker_M (
		  MakerCode nvarchar(5)
		, MakerName nvarchar(50)

	)

	SET @PARAM = '@MakerName nvarchar(50)'

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_Maker_M' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  M.MakerCode' + @CRLF
	SET @SQL = @SQL +'	, M.MakerName' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.Maker AS M' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    M.DelFlag = ''0''' + @CRLF

	--���[�J�[��
	IF ((@MakerName IS NOT NULL) AND (@MakerName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND M.MakerName LIKE ''%' + @MakerName + '%''' + @CRLF
	END

	EXECUTE sp_executeSQL @SQL, @PARAM, @MakerName
	CREATE INDEX ix_temp_Maker_M ON #temp_Maker_M(MakerCode)

	/*-------------------------------------------*/
	/* �Ԏ���									 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_CarGrade_C(
		  CarGradeCode nvarchar(30)
		, CarGradeName nvarchar(50)
	)

	SET @PARAM = '@CarGradeCode nvarchar(30)'

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_CarGrade_C' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  C.CarGradeCode' + @CRLF
	SET @SQL = @SQL +'	, C.CarGradeName' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.CarGrade AS C' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    C.DelFlag = ''0''' + @CRLF 

	--�Ԏ�R�[�h
	IF ((@CarGradeCode IS NOT NULL) AND (@CarGradeCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND C.CarGradeCode = @CarGradeCode' + @CRLF
	END	

	EXECUTE sp_executeSQL @SQL, @PARAM, @CarGradeCode
	CREATE INDEX ix_temp_Car_C ON #temp_CarGrade_C(CarGradeCode)

	/*-------------------------------------------*/
	/* �ԗ��I�v�V��������						 */
	/*-------------------------------------------*/

	SET @PARAM = '@MakerCode nvarchar(5), @CarOptionCode nvarchar(25), @CarOptionName nvarchar(100), @CarGradeCode nvarchar(30), @RequiredFlag nvarchar(2), @DelFlag nvarchar(2)'

	SET @SQL = ''
	SET @SQL = @SQL + 'SELECT'+ @CRLF
	SET @SQL = @SQL + '   OP.MakerCode AS MakerCode'+ @CRLF
	SET @SQL = @SQL + ' , M.MakerName AS MakerName'+ @CRLF
	SET @SQL = @SQL + ' , OP.CarOptionCode AS CarOptionCode'+ @CRLF
	SET @SQL = @SQL + ' , OP.CarOptionName AS CarOptionName'+ @CRLF
	SET @SQL = @SQL + ' , OP.OptionType AS OptionType'+ @CRLF
	SET @SQL = @SQL + ' , ISNULL(C.CarGradeName, ''����'') AS CarGradeName'+ @CRLF
	SET @SQL = @SQL + ' , OP.RequiredFlag'+ @CRLF
	SET @SQL = @SQL + ' , OP.DelFlag AS DelFlag'+ @CRLF

    SET @SQL = @SQL + 'FROM dbo.CarOption AS OP'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CarGrade_C AS C ON C.CarGradeCode = OP.CarGradeCode'+ @CRLF
	SET @SQL = @SQL + 'INNER JOIN #temp_Maker_M AS M ON M.MakerCode = OP.MakerCode'+ @CRLF

	SET @SQL = @SQL + 'WHERE 1 = 1'+ @CRLF		

	--���[�J�[�R�[�h
	IF ((@MakerCode IS NOT NULL) AND (@MakerCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND M.MakerCode LIKE ''%' + @MakerCode + '%''' + @CRLF
	END

	--�I�v�V�����R�[�h
	IF ((@CarOptionCode IS NOT NULL) AND (@CarOptionCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND OP.CarOptionCode LIKE ''%' + @CarOptionCode + '%''' + @CRLF
	END

	--�I�v�V������
	IF ((@CarOptionName IS NOT NULL) AND (@CarOptionName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND OP.CarOptionName LIKE ''%' + @CarOptionName + '%''' + @CRLF
	END

	IF @ActionFlag = '0'
	BEGIN
		--�Ԏ�R�[�h
		IF ((@CarGradeCode IS NOT NULL) AND (@CarGradeCode <> ''))
		BEGIN
			SET @SQL = @SQL +'AND OP.CarGradeCode = @CarGradeCode' + @CRLF
		END
	END
	ELSE
	BEGIN
		--�Ԏ�R�[�h
		IF ((@CarGradeCode IS NOT NULL) AND (@CarGradeCode <> ''))
		BEGIN
			SET @SQL = @SQL +'AND OP.CarGradeCode in (@CarGradeCode, '''')' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND OP.CarGradeCode = ''''' + @CRLF
		END
	END
		
	--�K�{�t���O
	IF ((@RequiredFlag IS NOT NULL) AND (@RequiredFlag <> ''))
	BEGIN
		SET @SQL = @SQL +'AND OP.RequiredFlag = @RequiredFlag' + @CRLF
	END	

	--�폜�t���O
	IF ((@DelFlag IS NOT NULL) AND (@DelFlag <> ''))
	BEGIN
		SET @SQL = @SQL +'AND OP.DelFlag = @DelFlag' + @CRLF
	END

	SET @SQL = @SQL + 'ORDER BY OP.MakerCode, OP.CarOptionCode'+ @CRLF


	EXECUTE sp_executeSQL @SQL, @PARAM, @MakerCode, @CarOptionCode, @CarOptionName, @CarGradeCode ,@RequiredFlag, @DelFlag


BEGIN

	BEGIN TRY
		DROP TABLE #temp_Maker_M
		DROP TABLE #temp_CarGrade_C
	END TRY
	BEGIN CATCH
		--����
	END CATCH

END





GO


