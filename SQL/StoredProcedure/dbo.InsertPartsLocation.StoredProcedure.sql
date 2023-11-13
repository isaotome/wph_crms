USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[InsertPartsLocation]    Script Date: 2016/10/03 15:29:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- ==============================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2016/08/13	arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
-- 2016/03/17   arc yano #3477 ���i���P�[�V�����}�X�^�@���P�[�V�����}�X�^�̎����X�V �I���m�莞�ɕ��i���P�[�V�����}�X�^�ɔ��f����
-- Update date
-- Description:	
--				�݌ɏ������ɕ��i���P�[�V�����}�X�^���X�V����
-- ==============================================================================================================================================
CREATE PROCEDURE [dbo].[InsertPartsLocation] 
	 --@DepartmentCode nvarchar(3),
	 @WarehouseCode nvarchar(6),
	 @EmployeeCode nvarchar(50)
AS	
	BEGIN

		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;

		--��������
		DECLARE @ErrorMessage NVARCHAR(4000) = ''
		DECLARE @ErrorNumber INT = 0

		
		--�����ꎞ�\�̍폜
		/*************************************************************************/
		IF OBJECT_ID(N'tempdb..#Temp_PartsStock', N'U') IS NOT NULL
		DROP TABLE #Temp_PartsStock;											--���i�݌ɏ��
		
		
		--�����ꎞ�\�̐錾
		--���i�݌ɏ��
		CREATE TABLE #Temp_PartsStock (
			[PartsNumber] NVARCHAR(25) NOT NULL				-- ���i�ԍ�
		,	[Quantity] DECIMAL(10, 0)						-- ����
		)
		CREATE UNIQUE INDEX IX_Temp_PartsStock ON #Temp_PartsStock ([PartsNumber], [Quantity])

		--�g�����U�N�V�����J�n 
		BEGIN TRANSACTION
		BEGIN TRY
		
			-- ----------------------------------------
			-- ���i���P�[�V�����}�X�^�폜����
			-- ----------------------------------------
			--��x���i�Ώە���̃��P�[�V�������폜
			DELETE 
				pl
			FROM 
				dbo.PartsLocation AS pl
			WHERE
				--DepartmentCode = @DepartmentCode
				WarehouseCode = @WarehouseCode					--Mod 2016/08/13 arc yano #3596				
		
			-- ----------------------------------------
			-- ���i���P�[�V�����}�X�^�쐬����
			-- ----------------------------------------
			--���i�݌Ƀe�[�u������A���i���ɍł��݌ɐ����������̂��擾����
			INSERT INTO #Temp_PartsStock
				SELECT 
					DISTINCT gps.PartsNumber, gps.Quantity
				FROM
				(
					SELECT
						ps.[PartsNumber] as PartsNumber
					,	max(ps.Quantity) as Quantity
					FROM 
						[WPH_DB].[dbo].[PartsStock] ps
					WHERE 
						DelFlag = '0' 
					AND
					EXISTS
					(
						--SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.DepartmentCode = @DepartmentCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode
						SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode	--Mod 2016/08/13 arc yano #359
					)
					GROUP BY ps.PartsNumber
				) gps
			--�C���f�b�N�X�č쐬

		
			DROP INDEX IX_Temp_PartsStock ON #Temp_PartsStock
			CREATE UNIQUE INDEX IX_Temp_PartsStock ON #Temp_PartsStock ([PartsNumber], [Quantity])

			--���P�[�V�����}�X�^���f
			INSERT INTO dbo.PartsLocation
				
				SELECT 
					  ps.PartsNumber  AS PartsNumber			--���i�ԍ�
					, '' AS DepartmentCode						--����R�[�h ���󕶎�
					, MIN(ps.LocationCode) AS LocationCode		--���P�[�V�����R�[�h
					, @EmployeeCode AS CreateEmployeeCode		--�쐬��
					, GETDATE() AS CreateDate					--�쐬��
					, @EmployeeCode AS LastUpdateEmployeeCode	--�ŏI�X�V��
					, GETDATE()  AS LastUpdateDate				--�ŏI�X�V��
					, '0' AS DelFlag							--�폜�t���O
					, @WarehouseCode AS WarehouseCode			--�q�ɃR�[�h
				FROM 	
					PartsStock ps
				WHERE 
					EXISTS
					(
						SELECT 'X' FROM #Temp_PartsStock tps WHERE tps.PartsNumber = ps.PartsNumber AND tps.Quantity = ps.Quantity
					)
					AND ps.DelFlag = '0' AND
					/*
					EXISTS
					(
						SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.DepartmentCode = @DepartmentCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode 
					)
					*/
					EXISTS
					(
						SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and l.LocationType = '001' and l.LocationCode GO

/****** Object:  StoredProcedure [dbo].[InsertPartsLocation]    Script Date: 2020/11/20 19:20:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- ==============================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2016/03/17   arc yano #3477 ���i���P�[�V�����}�X�^�@���P�[�V�����}�X�^�̎����X�V �I���m�莞�ɕ��i���P�[�V�����}�X�^�ɔ��f����
-- Update date
-- 2020/11/06 yano #4036�y���i���ׁz���׍ϕ��i�̉ߋ��̃��P�[�V������񂪏�����
-- 2016/08/13	arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
-- Description:	
--				�݌ɏ������ɕ��i���P�[�V�����}�X�^���X�V����
-- ==============================================================================================================================================
CREATE PROCEDURE [dbo].[InsertPartsLocation] 
	 --@DepartmentCode nvarchar(3),
	 @WarehouseCode nvarchar(6),
	 @EmployeeCode nvarchar(50)
AS	
	BEGIN

		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;

		--��������
		DECLARE @ErrorMessage NVARCHAR(4000) = ''
		DECLARE @ErrorNumber INT = 0

		
		--�����ꎞ�\�̍폜
		/*************************************************************************/
		IF OBJECT_ID(N'tempdb..#Temp_PartsStock', N'U') IS NOT NULL
		DROP TABLE #Temp_PartsStock;											--���i�݌ɏ��
		
		
		--�����ꎞ�\�̐錾
		--���i�݌ɏ��
		CREATE TABLE #Temp_PartsStock (
			[PartsNumber] NVARCHAR(25) NOT NULL				-- ���i�ԍ�
		,	[Quantity] DECIMAL(10, 2)						-- ����				--Mod  2020/11/06 yano #4036
		)
		CREATE UNIQUE INDEX IX_Temp_PartsStock ON #Temp_PartsStock ([PartsNumber], [Quantity])

		--�g�����U�N�V�����J�n 
		BEGIN TRANSACTION
		BEGIN TRY
		
			-- ----------------------------------------
			-- ���i���P�[�V�����}�X�^�폜����
			-- ----------------------------------------
			--��x���i�Ώە���̃��P�[�V�������폜
			DELETE 
				pl
			FROM 
				dbo.PartsLocation AS pl
			WHERE
				--DepartmentCode = @DepartmentCode
				WarehouseCode = @WarehouseCode					--Mod 2016/08/13 arc yano #3596				
		
			-- ----------------------------------------
			-- ���i���P�[�V�����}�X�^�쐬����
			-- ----------------------------------------
			--���i�݌Ƀe�[�u������A���i���ɍł��݌ɐ����������̂��擾����
			INSERT INTO #Temp_PartsStock
				SELECT 
					DISTINCT gps.PartsNumber, gps.Quantity
				FROM
				(
					SELECT
						ps.[PartsNumber] as PartsNumber
					,	max(ps.Quantity) as Quantity
					FROM 
						[dbo].[PartsStock] ps
					WHERE 
						DelFlag = '0' 
					AND
					EXISTS
					(
						--SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.DepartmentCode = @DepartmentCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode
						SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode	--Mod 2016/08/13 arc yano #359
					)
					GROUP BY ps.PartsNumber
				) gps
			--�C���f�b�N�X�č쐬

		
			DROP INDEX IX_Temp_PartsStock ON #Temp_PartsStock
			CREATE UNIQUE INDEX IX_Temp_PartsStock ON #Temp_PartsStock ([PartsNumber], [Quantity])

			--���P�[�V�����}�X�^���f
			INSERT INTO dbo.PartsLocation
				
				SELECT 
					  ps.PartsNumber  AS PartsNumber			--���i�ԍ�
					, '' AS DepartmentCode						--����R�[�h ���󕶎�
					, MIN(ps.LocationCode) AS LocationCode		--���P�[�V�����R�[�h
					, @EmployeeCode AS CreateEmployeeCode		--�쐬��
					, GETDATE() AS CreateDate					--�쐬��
					, @EmployeeCode AS LastUpdateEmployeeCode	--�ŏI�X�V��
					, GETDATE()  AS LastUpdateDate				--�ŏI�X�V��
					, '0' AS DelFlag							--�폜�t���O
					, @WarehouseCode AS WarehouseCode			--�q�ɃR�[�h
				FROM 	
					PartsStock ps
				WHERE 
					EXISTS
					(
						SELECT 'X' FROM #Temp_PartsStock tps WHERE tps.PartsNumber = ps.PartsNumber AND tps.Quantity = ps.Quantity
					)
					AND ps.DelFlag = '0' AND
					/*
					EXISTS
					(
						SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.DepartmentCode = @DepartmentCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode 
					)
					*/
					EXISTS
					(
						SELECT 'X' FROM dbo.Location l where l.DelFlag = '0' and l.WarehouseCode = @WarehouseCode and l.LocationType = '001' and l.LocationCode = ps.LocationCode 
					)
				GROUP BY
					ps.PartsNumber

			--�g�����U�N�V�����I��
			COMMIT TRANSACTION
		END TRY
		BEGIN CATCH
			ROLLBACK TRANSACTION
			SELECT 
				@ErrorNumber = ERROR_NUMBER()
			,	@ErrorMessage = ERROR_MESSAGE()
		END CATCH
		
FINALLY:
		--�G���[����
	IF @ErrorNumber <> 0
		RAISERROR (@ErrorMessage, 16, 1)

	RETURN @ErrorNumber
			--�I��	
END

GO

