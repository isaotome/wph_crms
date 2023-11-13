USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[Get_CarSlipStatusChange]    Script Date: 2017/06/29 11:31:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--2017/05/11 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs�@�V�K�쐬

CREATE PROCEDURE [dbo].[Get_CarSlipStatusChange]

	@ChangeStatus nvarchar(2)	--�i�s���F�P�@�����F�Q

AS
	SET NOCOUNT ON
	
	/*-------------------------------------------*/
	/* �_�[�e�B�[���[�h�̐ݒ�					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/* �`�[�̏C���f�[�^���擾					 */
	/*-------------------------------------------*/
	
	SELECT TOP 500 
	   w.[SlipNumber]
      ,w.[SalesOrderStatus]
      ,w.[RequestUserName]
      ,w.[CreateEmployeeCode]
      ,w.[CreateDate]
      ,w.[LastUpdateEmployeeCode]
      ,w.[LastUpdateDate]
      ,w.[ChangeStatus]
      ,w.[StatusChangeCode]
	  ,E.EmployeeName
	FROM W_CarSlipStatusChange w
	INNER JOIN Employee E ON W.LastUpdateEmployeeCode=E.EmployeeCode
	WHERE ChangeStatus = @ChangeStatus
	ORDER BY W.LastUpdateDate DESC


GO


