USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[CarSlipStatus_DispFlag]    Script Date: 2017/06/29 11:32:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--2017/05/11 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs�@�V�K�쐬

CREATE PROCEDURE [dbo].[CarSlipStatus_DispFlag]

	@SlipNumber nvarchar(50),	--�`�[�ԍ�
	@EmployeeCode nvarchar(50),	--�Ј��R�[�h(�X�V��)
	@ChangeStatus nchar(3),		--�C���X�e�[�^�X
	@StatusChangeCode nvarchar(36)	--�C��ID
AS
	SET NOCOUNT ON

BEGIN
	BEGIN TRY
	/*-----------------------------*/
	/* �����e�[�u���X�V			   */
	/*-----------------------------*/

	UPDATE W_CarSlipStatusChange
	SET ChangeStatus = @ChangeStatus,
		LastUpdateDate = GETDATE(),
		LastUpdateEmployeeCode = @EmployeeCode
	WHERE StatusChangeCode = @StatusChangeCode

	END TRY
		BEGIN CATCH
		RETURN ERROR_NUMBER()
	END CATCH

	RETURN 0

END
GO


