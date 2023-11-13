USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[AddSerialNumber]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddSerialNumber]
	@SerialCode nvarchar(50)
	,@SerialName nvarchar(50)
	,@PrefixCode nvarchar(50)
	,@SuffixCode nvarchar(50)
	,@SequenceNumber decimal(10,0)
	,@EmployeeCode nvarchar(50)
	,@NewSequenceNumber decimal(10,0) OUTPUT
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
	SET @NewSequenceNumber = @SequenceNumber
	INSERT INTO dbo.SerialNumber (
			SerialCode
			,SerialName
			,PrefixCode
			,SuffixCode
			,SequenceNumber
			,CreateEmployeeCode
			,CreateDate
			,LastUpdateEmployeeCode
			,LastUpdateDate
			,DelFlag
			)
	VALUES (
			@SerialCode
			, @SerialName
			, @PrefixCode
			, @SuffixCode
			, @SequenceNumber
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
					UPDATE dbo.SerialNumber
					SET SequenceNumber = SequenceNumber + 1
					, LastUpdateEmployeeCode = @EmployeeCode
					, LastUpdateDate = SYSDATETIME()
					WHERE SerialCode = @SerialCode
					SET @NewSequenceNumber = (SELECT a.SequenceNumber FROM dbo.SerialNumber a WHERE a.SerialCode = @SerialCode)
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
