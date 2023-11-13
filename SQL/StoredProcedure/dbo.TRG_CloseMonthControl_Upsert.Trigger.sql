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
	--対象テーブル名/機能概要を指定
	-------------------------------
	DECLARE @TABLE_NAME NVARCHAR(32) = 'InventorySchedule'
	DECLARE @FUNC_NAME NVARCHAR(32) = '棚卸スケジュール'

	-----------------------
	--以降は共通
	-----------------------
	--変数宣言
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

		--一時テーブルの作成
		SELECT TOP 0 CAST(NULL AS NCHAR(1)) AS x, i.* INTO #TEMP_INS FROM inserted i
		
		--挿入レコードのインサート
		INSERT INTO #TEMP_INS SELECT 'i' x, * FROM inserted WHERE inserted.InventoryType = '001' and inserted.DepartmentCode in (SELECT DepartmentCode from Department where CloseMonthFlag in ('1', '2')) order by 1
		IF @@ROWCOUNT = 0	--InventoryType = '001'(車両棚卸)かつ、DepartmentCode=車両棚卸対象の部門以外の更新の場合は処理をスキップ
			RETURN
		
		BEGIN
			--変数初期化
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
			WHERE ISNULL(CloseMonthFlag, '0') in ('1', '2')		--棚卸対象部門の場合
			
			--MAX,MIN
			SELECT 
				@INVENTORYCNT = COUNT(InventoryMonth),
				@MIN = MIN(InventoryStatus), 
				@MAX = MAX(InventoryStatus) 
			FROM dbo.InventorySchedule as a
			WHERE InventoryMonth = @INVENTORYMONTH
			AND InventoryType = '001'
			--Mod 2015/03/09 arc yano #3162 CloseMonthFlag = '1'または'2'のInventoryStatusのみチェックする
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

			--CloseStatusの設定
			IF (@INVENTORYCNT < @DEPARTMENTCNT) OR (ISNULL(@MIN, '') <> ISNULL(@MAX, ''))
				SET @CLOSESTS = N'004'	--closeMonthContorol.CloseStatus = '004'(仮締め中)
			ELSE
				SET @CLOSESTS = @MAX
			
			--CloseMonthControl取得
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
		--エラーは無視
	END CATCH
FINALLY:
	--終了	
	

END








GO


