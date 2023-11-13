USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[AddServiceCost]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddServiceCost]
	@ServiceMenuCode nvarchar(8)
	,@CarClassCode nvarchar(30)
	,@Cost decimal(5,2)
	,@EmployeeCode nvarchar(50)
AS

DECLARE @TranCreated BIT = 0
BEGIN
	IF @@TRANCOUNT = 0
		BEGIN
			SET @TranCreated = 1
			BEGIN TRANSACTION
		END
END	

BEGIN TRY
	INSERT INTO dbo.ServiceCost(
			ServiceMenuCode
			,CarClassCode
			,Cost
			,CreateEmployeeCode
			,CreateDate
			,LastUpdateEmployeeCode
			,LastUpdateDate
			,DelFlag
			)
	VALUES (
			@ServiceMenuCode
			, @CarClassCode
			, @Cost
			, @EmployeeCode
			, SYSDATETIME()
			, @EmployeeCode
			, SYSDATETIME()
			, '0'
	)
END TRY

BEGIN CATCH
	BEGIN
		IF ERROR_NUMBER() = 2627
			BEGIN
				BEGIN TRY
					UPDATE dbo.ServiceCost
					SET Cost = @Cost
					, LastUpdateEmployeeCode = @EmployeeCode
					, LastUpdateDate = SYSDATETIME()
					WHERE ServiceMenuCode = @ServiceMenuCode
					AND CarClassCode = @CarClassCode
				END TRY
				BEGIN CATCH
					Goto ERROR_HANDLER
				END CATCH
			END
		ELSE
			BEGIN
				Goto ERROR_HANDLER
			END
	END
END CATCH

IF @TranCreated = 1
	COMMIT TRANSACTION

RETURN 0

ERROR_HANDLER:

IF @TranCreated = 1
	ROLLBACK TRANSACTION
DECLARE @ErrorMessage NVARCHAR(4000)
DECLARE @ErrorSeverity INT
DECLARE @ErrorState INT
SELECT 
	@ErrorMessage = ERROR_MESSAGE(),
	@ErrorSeverity = ERROR_SEVERITY(),
	@ErrorState = ERROR_STATE()
RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState)
GO
