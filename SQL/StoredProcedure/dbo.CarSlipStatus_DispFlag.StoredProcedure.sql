USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[CarSlipStatus_DispFlag]    Script Date: 2017/06/29 11:32:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--2017/05/11 arc nakayama #3761_サブシステムの伝票戻しの移行　新規作成

CREATE PROCEDURE [dbo].[CarSlipStatus_DispFlag]

	@SlipNumber nvarchar(50),	--伝票番号
	@EmployeeCode nvarchar(50),	--社員コード(更新者)
	@ChangeStatus nchar(3),		--修正ステータス
	@StatusChangeCode nvarchar(36)	--修正ID
AS
	SET NOCOUNT ON

BEGIN
	BEGIN TRY
	/*-----------------------------*/
	/* 履歴テーブル更新			   */
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


