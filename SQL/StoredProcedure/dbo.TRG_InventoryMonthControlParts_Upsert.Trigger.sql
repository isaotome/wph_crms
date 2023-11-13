USE [WPH_DB]
GO

/****** Object:  Trigger [dbo].[TRG_InvnentoryMonthControlParts_Upsert]    Script Date: 2015/03/10 14:27:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER [dbo].[TRG_InvnentoryMonthControlParts_Upsert]
   ON [dbo].[InventoryScheduleParts]
   AFTER INSERT,UPDATE
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	
	-------------------------------
	--�Ώۃe�[�u����/�@�\�T�v���w��
	-------------------------------
	DECLARE @TABLE_NAME NVARCHAR(32) = 'InventorySchedule'
	DECLARE @FUNC_NAME NVARCHAR(32) = '�I���X�P�W���[��'

	-----------------------
	--�ȍ~�͋���
	-----------------------
	--�ϐ��錾
	DECLARE @NOW DATETIME = GETDATE()	
	DECLARE @CRLF NVARCHAR(2) = CHAR(13) + CHAR(10)
	DECLARE @SQL NVARCHAR(256) = N''
	DECLARE @INVENTORYMONTH DATETIME
	DECLARE @CLOSEMONTH NVARCHAR(8)
	DECLARE @CEMPCODE NVARCHAR(50)
	DECLARE @LEMPCODE NVARCHAR(50)
	DECLARE @MIN NVARCHAR(5)
	DECLARE @MAX NVARCHAR(5)
	DECLARE @RCNT DECIMAL(4,0)
	DECLARE @INVENTORYCNT DECIMAL(4,0)
	DECLARE @DEPARTMENTCNT DECIMAL(4,0)
	DECLARE @CLOSESTS NVARCHAR(5)
	
	BEGIN TRY

		--�ꎞ�e�[�u���̍쐬
		SELECT TOP 0 CAST(NULL AS NCHAR(1)) AS x, i.* INTO #TEMP_INS FROM inserted i
		
		--�}�����R�[�h�̃C���T�[�g
		INSERT INTO #TEMP_INS SELECT 'i' x, * FROM inserted WHERE inserted.InventoryType = '002' and inserted.DepartmentCode in (SELECT DepartmentCode FROM Department WHERE CloseMonthFlag = '2') order by 1
		IF @@ROWCOUNT = 0	--���i�I���̃��R�[�h�łȂ��ꍇ�̓X�L�b�v
			RETURN
		
		BEGIN
			--�ϐ�������
			SET @MIN = N''
			SET @MAX = N''
			
			--InventorySchedule.InventoryMonth,InventoryMonthControlParts.InventoryMonth
			SELECT TOP 1 
				@INVENTORYMONTH = InventoryMonth, 
				@CLOSEMONTH = CONVERT(VARCHAR, InventoryMonth,112), 
				@CEMPCODE = CreateEmployeeCode, 
				@LEMPCODE = LastUpdateEmployeeCode
			FROM 
				#TEMP_INS

			--Department
			SELECT 
				@DEPARTMENTCNT = COUNT(DepartmentCode)
			FROM 
				dbo.Department
			WHERE 
				ISNULL(CloseMonthFlag, '0') = '2'	--closeMonthFlag = '2'(���i�I���݌�)
			
			--MAX,MIN
			--Mod 2015/03/10 arc yano #3162 �ގ��������ACloseMonthFlag = '2'��InventoryStatus�̂݃`�F�b�N����
			SELECT 
				@INVENTORYCNT = COUNT(InventoryMonth),
				@MIN = MIN(InventoryStatus), 
				@MAX = MAX(InventoryStatus) 
			FROM 
				dbo.InventoryScheduleParts sp
			WHERE 
					InventoryMonth = @INVENTORYMONTH 
				AND InventoryType = '002'
				AND EXISTS
					(
						SELECT DepartmentCode FROM Department WHERE CloseMonthFlag = '2' AND DepartmentCode = sp.DepartmentCode
					)
					

			--CloseStatus�̐ݒ�
			--�擾����InventorySchedule�̈ꗗ�̃X�e�[�^�X��MAX != MIN
			IF (@INVENTORYCNT < @DEPARTMENTCNT) OR (ISNULL(@MIN, '') <> ISNULL(@MAX, ''))
				SET @CLOSESTS = N'001'	--inventoryMonthControlParts.InventoryStatus = '001'(���s��)
			ELSE
				SET @CLOSESTS = @MAX
			
			--InvnentoryMonthControlParts�擾
			SELECT @RCNT = COUNT(InventoryMonth)
			FROM InventoryMonthControlParts
			WHERE InventoryMonth = @CLOSEMONTH

			IF @RCNT = 0
				BEGIN
				INSERT INTO InventoryMonthControlParts (
					[InventoryMonth]
				,	[InventoryStatus]
				,	[CreateEmployeeCode]
				,	[CreateDate]
				,	[LastUpdateEmployeeCode]
				,	[LastUpdateDate]
				,	[DelFlag]
					)
				SELECT
					@CLOSEMONTH
				,	@CLOSESTS
				,	@CEMPCODE
				,	@NOW
				,	@LEMPCODE
				,	@NOW
				,	N'0'
				END
			ELSE
				BEGIN
				UPDATE InventoryMonthControlParts 
				SET 
					[InventoryStatus] = @CLOSESTS
				,	[LastUpdateEmployeeCode] = @LEMPCODE
				,	[LastUpdateDate] = @NOW
				WHERE 
					[InventoryMonth] = @CLOSEMONTH
				END
		END
	END TRY
	BEGIN CATCH
		--�G���[�͖���
	END CATCH
FINALLY:
	--�I��	
	

END






GO


