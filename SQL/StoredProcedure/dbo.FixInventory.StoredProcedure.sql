USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[FixInventory]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[FixInventory]
	@DepartmentCode nvarchar(3)
	,@InventoryMonth datetime
	,@InventoryType nvarchar(3)
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
	BEGIN
		INSERT INTO dbo.InventoryStock (
				InventoryId
				, DepartmentCode
				,InventoryMonth
				,LocationCode
				,EmployeeCode
				,InventoryType
				,SalesCarNumber
				,PartsNumber
				,Quantity
				,CreateEmployeeCode
				,CreateDate
				,LastUpdateEmployeeCode
				,LastUpdateDate
				,DelFlag
				,Summary
				)
		SELECT	NEWID()
				, DepartmentCode
				,InventoryMonth
				,LocationCode
				,EmployeeCode
				,InventoryType
				,SalesCarNumber
				,PartsNumber
				,Quantity
				,@EmployeeCode
				,SYSDATETIME()
				,@EmployeeCode
				,SYSDATETIME()
				,'0'
				,Summary
		FROM	dbo.Inventory
		WHERE	DepartmentCode = @DepartmentCode
		AND		InventoryMonth = @InventoryMonth
		AND		InventoryType = @InventoryType
		AND		Quantity <> 0
	END
	BEGIN
		DELETE	dbo.Inventory
		WHERE	DepartmentCode = @DepartmentCode
		AND		InventoryMonth = @InventoryMonth
		AND		InventoryType = @InventoryType
	END
	BEGIN
		UPDATE	dbo.InventorySchedule
		SET		InventoryStatus = '003'
				, StartDate = (case when StartDate is null then CONVERT(datetime, CONVERT(date, SYSDATETIME())) else StartDate end)
				, EndDate = CONVERT(datetime, CONVERT(date, SYSDATETIME()))
				, LastUpdateEmployeeCode = @EmployeeCode
				, LastUpdateDate = SYSDATETIME()
		WHERE	DepartmentCode = @DepartmentCode
		AND		InventoryMonth = @InventoryMonth
		AND		InventoryType = @InventoryType
	END
END TRY

BEGIN CATCH
	Goto ERROR_HANDLER
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
