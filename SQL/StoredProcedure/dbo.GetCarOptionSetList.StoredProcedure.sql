USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCarOptionSetList]    Script Date: 2016/03/07 16:33:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- 2016/02/09 arc nakayama �ԗ��`�[�쐬���̃I�v�V�����̃f�t�H���g�ݒ� �V�K�쐬
-- 2016/03/07 arc nakayama �ԗ��`�[�쐬���̃I�v�V�����̃f�t�H���g�ݒ� �Ԏ�R�[�h����O���[�h�R�[�h�ɕύX

CREATE PROCEDURE [dbo].[GetCarOptionSetList] 
	@CarGradeCode nvarchar(30)	--�Ԏ�R�[�h
	,@MakerCode nvarchar(5)	--���[�J�[�R�[�h
AS
BEGIN
	SET NOCOUNT ON;
	
	/*-------------------------------------------*/
	/* �_�[�e�B�[���[�h�̐ݒ�					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/* �Y���Ԏ�Ɋւ���K�{�I�v�V�����擾		 */
	/*-------------------------------------------*/

	SELECT [CarOptionCode]
		  ,[CarOptionName]
		  ,[MakerCode]
		  ,[Cost]
		  ,[SalesPrice]
		  ,[OptionType]
		  ,[CarGradeCode]
	FROM [WPH_DB].[dbo].[CarOption]
	WHERE DelFlag = '0'
	  AND CarGradeCode = @CarGradeCode
	  AND MakerCode = @MakerCode
	  AND RequiredFlag = '1'
END


GO


