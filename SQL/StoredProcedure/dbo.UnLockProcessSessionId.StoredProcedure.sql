USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[UnLockProcessSessionId]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		T.Ryumura
-- Create date: 2012/02/21
-- Description:	伝票ロックを強制解除する
-- =============================================
CREATE PROCEDURE [dbo].[UnLockProcessSessionId]
	@SlipNumber nvarchar(50)
	,@RevisionNumber int
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ProcessSessionId UniqueIdentifier
	
	SELECT @ProcessSessionId = ProcessSessionId FROM ServiceSalesHeader
	WHERE SlipNumber=@SlipNumber and RevisionNumber=@RevisionNumber
		
	UPDATE ServiceSalesHeader
	SET ProcessSessionId = null
	WHERE SlipNumber=@SlipNumber and RevisionNumber=@RevisionNumber
	
	DELETE FROM ProcessSessionControl
	WHERE ProcessSessionId = @ProcessSessionId
	
END
GO
