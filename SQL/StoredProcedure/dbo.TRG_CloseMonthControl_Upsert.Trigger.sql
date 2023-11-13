USE [WPH_DB]
GO

/****** Object:  Trigger [dbo].[TRG_CloseMonthControl_Upsert]    Script Date: 2015/03/09 13:47:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER [dbo].[TRG_CloseMonthControl_Upsert]
   ON [dbo].[InventorySchedule]
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
		INSERT INTO #TEMP_INS SELECT 'i' x, * FROM inserted WHERE inserted.InventoryType = '001' and inserted.DepartmentCode in (SELECT DepartmentCode from Department where CloseMonthFlag in ('1', '2')) order by 1
		IF @@ROWCOUNT = 0	--InventoryType = '001'(�ԗ��I��)���ADepartmentCode=�ԗ��I���Ώۂ̕���ȊO�̍X�V�̏ꍇ�͏������X�L�b�v
			RETURN
		
		BEGIN
			--�ϐ�������
			SET @MIN = N''
			SET @MAX = N''
			
			--InventoryMonth,CloseMonth
			SELECT TOP 1 
				@INVENTORYMONTH = InventoryMonth, 
				@CLOSEMONTH = CONVERT(VARCHAR, InventoryMonth,112), 
				@CEMPCODE = CreateEmployeeCode, 
				@LEMPCODE = LastUpdateEmployeeCode
			FROM #TEMP_INS

			--Department
			SELECT 
				@DEPARTMENTCNT = COUNT(DepartmentCode)
			FROM dbo.Department
			WHERE ISNULL(CloseMonthFlag, '0') in ('1', '2')		--�I���Ώە���̏ꍇ
			
			--MAX,MIN
			SELECT 
				@INVENTORYCNT = COUNT(InventoryMonth),
				@MIN = MIN(InventoryStatus), 
				@MAX = MAX(InventoryStatus) 
			FROM dbo.InventorySchedule as a
			WHERE InventoryMonth = @INVENTORYMONTH
			AND InventoryType = '001'
			--Mod 2015/03/09 arc yano #3162 CloseMonthFlag = '1'�܂���'2'��InventoryStatus�̂݃`�F�b�N����
			AND EXISTS
			(
				SELECT 
					DepartmentCode 
				FROM 
					Department 
				WHERE 
						CloseMonthFlag in ( '1', '2') 
					AND DepartmentCode = a.DepartmentCode
			)

			--CloseStatus�̐ݒ�
			IF (@INVENTORYCNT < @DEPARTMENTCNT) OR (ISNULL(@MIN, '') <> ISNULL(@MAX, ''))
				SET @CLOSESTS = N'004'	--closeMonthContorol.CloseStatus = '004'(�����ߒ�)
			ELSE
				SET @CLOSESTS = @MAX
			
			--CloseMonthControl�擾
			SELECT @RCNT = COUNT(CloseMonth)
			FROM CloseMonthControl
			WHERE CloseMonth = @CLOSEMONTH

			IF @RCNT = 0
				BEGIN
				INSERT INTO CloseMonthControl (
					[CloseMonth]
				,	[CloseStatus]
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
				UPDATE CloseMonthControl 
				SET 
					[CloseStatus] = @CLOSESTS
				,	[LastUpdateEmployeeCode] = @LEMPCODE
				,	[LastUpdateDate] = @NOW
				WHERE 
					[CloseMonth] = @CLOSEMONTH
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


