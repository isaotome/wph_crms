USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsWipStock]    Script Date: 2017/11/07 16:20:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- ===================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/10/19 arc yano  #3803  �T�[�r�X�`�[ ���i�������̏o�� c_StockStatus��SelectedGenuineType�p�~�ɂ��C��
-- 2017/02/06 arc yano  #3645  ���i���ד��́@���׊m�莞�̃G���[ LineContents�̃T�C�Y��ύX(nvarchar(25)��nvarchar(50))
-- 2016/08/13 arc yano  #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
-- 2015/10/14 arc yano  ���i�݌ɒI���s��Ή�(���i�d�|�݌ɉ�ʂ̎d�|�f�[�^���o�s�)�A
-- 2015/09/11 arc yano  ���i�݌ɒI���s��Ή�(���i�d�|�݌ɉ�ʂ̎d�|�f�[�^���o�s�)
-- Description:	<Description,,>
-- �d�|�݌ɂ̕\���E�ۑ�
-- ===================================================================================================================
CREATE PROCEDURE [dbo].[GetPartsWipStock] 
	@ActionFlag int = 0,						--����w��(0:��ʕ\��, 1:�X�i�b�v�V���b�g�ۑ�)
	@TargetMonth datetime,						--�w�茎	
	@DepartmentCode NVARCHAR(3) ,				--����R�[�h
	@WarehouseCode NVARCHAR(6) ,				--�q�ɃR�[�h
	@ServiceType NVARCHAR(3) = NULL,			--���(002:�O���A 003:���i)
	@ArrivalPlanDateFrom NVARCHAR(10) = NULL,	--���ɓ�(From)
	@ArrivalPlanDateTo NVARCHAR(10) = NULL,		--���ɓ�(To)
	@SlipNumber NVARCHAR(50) = NULL,			--�`�[�ԍ�
	@PartsNumber NVARCHAR(25) = NULL,			--���i�ԍ�
	@PartsNameJp NVARCHAR(50) = NULL,			--���i��
	@Vin NVARCHAR(20) = NULL,					--�ԑ�ԍ�
	@CustomerName NVARCHAR(80) = NULL			--�ڋq��(�����^�J�i)
AS 
BEGIN
--�ϐ��錾
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-------------------------------
	--�Ώۃe�[�u����/�@�\�T�v���w��
	-------------------------------
	DECLARE @TABLE_NAME NVARCHAR(32) = 'InventoryParts_Shikakari'
	DECLARE @FUNC_NAME  NVARCHAR(32) = 'GetartsWipStock'

	-----------------------
	--�ȍ~�͋���
	-----------------------
	--�ϐ��錾
	DECLARE @TargetDateFrom datetime				--�Ώ۔N��(From)
	DECLARE @TargetDateTo datetime					--�Ώ۔N��(To)
	DECLARE @InventoryStatus NVARCHAR(3)			--�I���X�e�[�^�X
	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)
	

	--��������
	DECLARE @ErrorMessage NVARCHAR(4000) = ''
	DECLARE @ErrorNumber INT = 0

	--�����ꎞ�\�̍폜
	/*************************************************************************/
	IF OBJECT_ID(N'tempdb..#Temp_ServiceSalesHeader', N'U') IS NOT NULL
	DROP TABLE #Temp_ServiceSalesHeader;											--�T�[�r�X�`�[�w�b�_
	IF OBJECT_ID(N'tempdb..#Temp_ServiceSalesHeader_Exempt', N'U') IS NOT NULL
	DROP TABLE #Temp_ServiceSalesHeader_Exempt;										--�T�[�r�X�`�[(�ΏۊO)
	IF OBJECT_ID(N'tempdb..#Temp_ServiceSales', N'U') IS NOT NULL
	DROP TABLE #Temp_ServiceSales;
	IF OBJECT_ID(N'tempdb..#Temp_DepartmentListUseWarehouse', N'U') IS NOT NULL
	DROP TABLE #Temp_DepartmentListUseWarehouse;									--���僊�X�g
	
	/*************************************************************************/
	--�T�[�r�X�`�[�w�b�_
	CREATE TABLE #Temp_ServiceSalesHeader (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[RevisionNumber] int NOT NULL					-- ���r�W�����ԍ�
	,	[DepartmentCode]  NVARCHAR(3)					-- ����R�[�h
	,	[ServiceOrderStatus] NVARCHAR(3)				-- �`�[�X�e�[�^�X
	,	[FrontEmployeeCode] NVARCHAR(50)				-- �t�����g�S���҃R�[�h
	,	[CustomerCode]  NVARCHAR(10)					-- �ڋq�R�[�h
	,	[CustomerName] NVARCHAR(80)						-- �ڋq��
	,	[CarName]	NVARCHAR(50)						-- �Ԏ햼
	,	[Vin]	NVARCHAR(20)							-- �ԑ�ԍ�
	,	[ArrivalPlanDate] DATETIME						-- ���ɗ\���
	)
	CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader ([SlipNumber], [RevisionNumber])

	--�T�[�r�X�`�[�w�b�_(�ΏۊO�f�[�^)
	CREATE TABLE #Temp_ServiceSalesHeader_Exempt (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	)
	--CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader_Exempt ON #Temp_ServiceSalesHeader_Exempt ([SlipNumber])
	
	--�T�[�r�X�`�[����(�T�[�r�X�^���i)
	CREATE TABLE #Temp_ServiceSales (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	,	[RevisionNumber] int NOT NULL					-- ���r�W�����ԍ�
	,	[ServiceOrderStatus] NVARCHAR(3)				-- �`�[�X�e�[�^�X
	,	[FrontEmployeeCode] NVARCHAR(50)				-- �T�[�r�X�S����
	,	[CustomerCode] NVARCHAR(10)						-- �ڋq�R�[�h
	,	[CustomerName] NVARCHAR(80)						-- �ڋq��
	,	[CarName] NVARCHAR(50)							-- �Ԏ햼
	,	[Vin]  NVARCHAR(20)								-- �ԑ�ԍ�
	,	[ArrivalPlanDate] datetime						-- ���ɗ\���
	,	[LineNumber] int NOT NULL						-- �s�ԍ�
	,	[ServiceWorkCode] NVARCHAR(5)					-- ���ƃR�[�h
	,	[StockStatus] NVARCHAR(3)						-- �݌ɏ�	
	,	[ServiceType] NVARCHAR(3)						-- �T�[�r�X���
	,	[ServiceType1] NVARCHAR(50)						-- �T�[�r�X��ʖ�
	,	[PartsNumber] NVARCHAR(25)						-- ���i�ԍ�
	,	[LineContents1] NVARCHAR(50)					-- ���i����				--LineContents�̃T�C�Y�ύXnvarchar(25)��nvarchar(50)
	,	[Quantity] DECIMAL(10, 2)						-- ����
	,	[OutOrderCost] DECIMAL(10, 0)					-- �O����
	,	[DepartmentCode] NVARCHAR(3)					-- ����
	,	[EmployeeCode] NVARCHAR(50)						-- ���J�j�b�N�S����
	,	[SupplierCode] NVARCHAR(10)						-- �O����
	,	[LineContents2] NVARCHAR(50)					-- ��Ɩ�								--LineContents�̃T�C�Y�ύXnvarchar(25)��nvarchar(50)
	)
	CREATE UNIQUE INDEX IX_Temp_ServiceSales ON #Temp_ServiceSales ([SlipNumber], [RevisionNumber], [LineNumber])

	--Add 2016/08/13 arc yano #3596
	--����ꗗ
	CREATE TABLE #Temp_DepartmentListUseWarehouse (
		[DepartmentCode] NVARCHAR(3) NOT NULL			--����R�[�h
	,	[WarehouseCode] NVARCHAR(6) NOT NULL			--�q�ɃR�[�h
	)
	CREATE UNIQUE INDEX IX_Temp_DepartmentListUseWarehouse ON #Temp_DepartmentListUseWarehouse ([DepartmentCode], [WarehouseCode])

	---------------------
	-- �f�[�^�ݒ�
	---------------------
	--�Ώ۔N���͈̔͐ݒ�
	--����
	DECLARE @NOW DATETIME = GETDATE()

	--����1��
	DECLARE @THISMONTH DATETIME = CONVERT(DATETIME, CONVERT(NVARCHAR(7), @TargetMonth, 111) + '/01', 111)
	
	--DEBUG
	--PRINT @THISMONTH

	--�Ώ۔N��(From)�̐ݒ�
	SET @TargetDateFrom = @TargetMonth

	--�Ώ۔N��(From)���{���ȍ~�̏ꍇ�A�Ώ۔N��(To)�͖{����ݒ�
	IF(@TargetDateFrom > @NOW)
	SET @TargetDateFrom = @NOW

	--�Ώ۔N��(To)�̐ݒ�
	SET @TargetDateTo = DATEADD(m, 1, @TargetMonth)						--�Ώی��̗�����ݒ�

	--�Ώ۔N��(From)���{���ȍ~�̏ꍇ�A�Ώ۔N��(To)�͖{����ݒ�
	IF(@TargetDateTo > @NOW)
	SET @TargetDateTo = @NOW


	--�I���X�e�[�^�X�̎擾
	SELECT 
		@InventoryStatus = InventoryStatus
	FROM
		dbo.[InventoryScheduleParts]
	WHERE
		--DepartmentCode = @DepartmentCode
		WarehouseCode = @WarehouseCode									--Mod 2016/08/13 arc yano #3596
		AND InventoryMonth = @TargetMonth

	--�g�����U�N�V�����J�n 
	BEGIN TRANSACTION
	BEGIN TRY

		--Add 2016/08/13 arc yano #3596
		----------------------------------------------
		-- �q�ɃR�[�h��蕔�僊�X�g���擾����
		----------------------------------------------
		--����E�q�ɂ̑g�������X�g���A�q�ɂ��g�p���Ă���S�ĕ�����擾����
		INSERT INTO #Temp_DepartmentListUseWarehouse
		SELECT
			 dw.DepartmentCode		--����R�[�h
			,dw.WarehouseCode		--�q�ɃR�[�h
		FROM
			dbo.DepartmentWarehouse dw
		WHERE
			dw.WarehouseCode = @WarehouseCode			--�q�ɃR�[�h

		--�C���f�b�N�X�Đ���	
		DROP INDEX IX_Temp_DepartmentListUseWarehouse ON #Temp_DepartmentListUseWarehouse
		CREATE UNIQUE INDEX IX_Temp_DepartmentListUseWarehouse ON #Temp_DepartmentListUseWarehouse ([DepartmentCode], [WarehouseCode])


		--�Ώی����I���I��
		IF (ISNULL(@InventoryStatus, '001') <> '002')
		BEGIN
			
			----------------------------------------------------------------
			--�T�[�r�X�`�[�ΏۊO�f�[�^�̎擾(�Ώی��ɃL�����Z���A��ƒ��~)
			----------------------------------------------------------------
			INSERT INTO #Temp_ServiceSalesHeader_Exempt
			SELECT
					sh.[SlipNumber]											--�`�[�ԍ�
			FROM
				dbo.ServiceSalesHeader sh
			WHERE 
				sh.[ServiceOrderStatus] in ('007', '010')				--007:�L�����Z���A010:��ƒ��~
			AND sh.[CreateDate] < @TargetDateTo
			AND sh.[DelFlag] = '0'
			
			/*
			--�C���f�b�N�X�Đ���	
			DROP INDEX IX_Temp_ServiceSalesHeader_Exempt ON #Temp_ServiceSalesHeader_Exempt
			CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader_Exempt ON #Temp_ServiceSalesHeader_Exempt ([SlipNumber])
			*/

			--------------------------------------------------------------
			--�T�[�r�X�`�[�ΏۊO�f�[�^�̎擾(�Ώی��ɔ[�ԍ�)
			--------------------------------------------------------------
			INSERT INTO #Temp_ServiceSalesHeader_Exempt
			SELECT
					sh.[SlipNumber]											--�`�[�ԍ�
			FROM
				dbo.ServiceSalesHeader sh
			WHERE 
				sh.[ServiceOrderStatus] = '006'								--006:�[�ԍ�
			AND ISNULL(sh.[SalesDate], @TargetDateTo) < @TargetDateTo		--�������ɔ[�ԍ�
			AND sh.[DelFlag] = '0'
			
			/*
			--�C���f�b�N�X�Đ���	
			DROP INDEX IX_Temp_ServiceSalesHeader_Exempt ON #Temp_ServiceSalesHeader_Exempt
			CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader_Exempt ON #Temp_ServiceSalesHeader_Exempt ([SlipNumber])
			*/

			-------------------------------------------
			--�T�[�r�X�`�[�w�b�_�i�荞��
			-------------------------------------------
			SET @PARAM = '@DepartmentCode NVARCHAR(3), @TargetDateTo DATETIME,@ArrivalPlanDateFrom NVARCHAR(10), @ArrivalPlanDateTo NVARCHAR(10), @SlipNumber NVARCHAR(50), @Vin NVARCHAR(20), @CustomerName NVARCHAR(80)'
					
			SET @SQL = ''
			SET @SQL = @SQL +'INSERT INTO #Temp_ServiceSalesHeader' + @CRLF
			SET @SQL = @SQL +'SELECT' + @CRLF
			SET @SQL = @SQL +'     sh.[SlipNumber]' + @CRLF														--�`�[�ԍ�
			SET @SQL = @SQL +',    sh.[RevisionNumber]' + @CRLF													--���r�W�����ԍ�
			SET @SQL = @SQL +',    sh.[DepartmentCode]' + @CRLF													--����R�[�h
			SET @SQL = @SQL +',    sh.[ServiceOrderStatus]' + @CRLF												--�`�[�X�e�[�^�X
			SET @SQL = @SQL +',    sh.[FrontEmployeeCode]' + @CRLF												--�T�[�r�X�S����
			SET @SQL = @SQL +',    sh.[CustomerCode]' + @CRLF													--�ڋq�R�[�h
			SET @SQL = @SQL +',    c.[CustomerName]' + @CRLF													--�ڋq��
			SET @SQL = @SQL +',    sh.[CarName]' + @CRLF														--�Ԏ햼
			SET @SQL = @SQL +',    sh.[Vin]' + @CRLF															--�ԑ�ԍ�
			SET @SQL = @SQL +',    sh.[ArrivalPlanDate]' + @CRLF												--���ɗ\���
			SET @SQL = @SQL +'FROM' + @CRLF
			SET @SQL = @SQL +'	dbo.ServiceSalesHeader sh' + @CRLF
			SET @SQL = @SQL +'	INNER JOIN dbo.[Customer] c' + @CRLF	
			SET @SQL = @SQL +'		ON sh.[CustomerCode] = c.[CustomerCode]' + @CRLF		
			SET @SQL = @SQL +'WHERE' + @CRLF
			--SET @SQL = @SQL +'		sh.[DepartmentCode] = @DepartmentCode' + @CRLF
			SET @SQL = @SQL +'		sh.[DelFlag] = ''0''' + @CRLF
			SET @SQL = @SQL +'	AND sh.[WorkingStartDate] < @TargetDateTo' + @CRLF								--�ΏۏI�����O�ɍ�ƊJ�n���Ă���`�[
			SET @SQL = @SQL +'	AND	NOT EXISTS (' + @CRLF
			SET @SQL = @SQL +'		SELECT ''X''' + @CRLF
			SET @SQL = @SQL +'		FROM' + @CRLF
			SET @SQL = @SQL +'			#Temp_ServiceSalesHeader_Exempt she' + @CRLF
			SET @SQL = @SQL +'		WHERE' + @CRLF
			SET @SQL = @SQL +'			she.[SlipNumber] = sh.[SlipNumber]' + @CRLF
			SET @SQL = @SQL +'	)' + @CRLF
			
			--PRINT @SQL

			--���ɗ\���
			IF ((@ArrivalPlanDateFrom is not null) AND (@ArrivalPlanDateFrom <> '') AND ISDATE(@ArrivalPlanDateFrom) = 1)
				IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
				BEGIN
					SET @SQL = @SQL +' AND sh.[ArrivalPlanDate] >= @ArrivalPlanDateFrom AND sh.[ArrivalPlanDate] < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF
				END
				ELSE
				BEGIN
					SET @SQL = @SQL +' AND  sh.[ArrivalPlanDate] = @ArrivalPlanDateFrom' + @CRLF 
				END
			ELSE
				IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
				BEGIN
					SET @SQL = @SQL +' AND sh.[ArrivalPlanDate] < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF 
				END

			--�`�[�ԍ�
			IF ((@SlipNumber is not null) AND (@SlipNumber <> ''))
			BEGIN
				SET @SQL = @SQL +' AND sh.[SlipNumber] LIKE ''%' + @SlipNumber + '%''' + @CRLF
			END

			--�ԑ�ԍ�
			IF ((@Vin is not null) AND (@Vin <> ''))
			BEGIN
				SET @SQL = @SQL +' AND sh.[Vin] LIKE ''%' + @Vin + '%''' + @CRLF
			END

			--�ڋq��
			IF ((@CustomerName is not null) AND (@CustomerName <> ''))
			BEGIN
				SET @SQL = @SQL +' AND c.CustomerName LIKE ''%' + @CustomerName + '%''' + @CRLF
			END

			--Mod 2016/08/13 arc yano #3596�@����R�[�h������ꍇ�͕���R�[�h�Œ��o����
			--����R�[�h
			IF ((@DepartmentCode is not null) AND (@DepartmentCode <> ''))
			BEGIN
				SET @SQL = @SQL +' AND sh.[DepartmentCode] = @DepartmentCode' + @CRLF
			END
			ELSE --�����ꍇ�͑q�ɃR�[�h����擾��������ꗗ�Œ��o����
			BEGIN
				SET @SQL = @SQL +' AND	EXISTS (' + @CRLF
				SET @SQL = @SQL +'		SELECT ''X''' + @CRLF
				SET @SQL = @SQL +'		FROM' + @CRLF
				SET @SQL = @SQL +'			#Temp_DepartmentListUseWarehouse dw' + @CRLF
				SET @SQL = @SQL +'		WHERE' + @CRLF
				SET @SQL = @SQL +'			sh.[DepartmentCode] = dw.[DepartmentCode]' + @CRLF
				SET @SQL = @SQL +'	)' + @CRLF
			END

			--DEBUG
			--PRINT @SQL
			
			EXECUTE sp_executeSQL @SQL, @PARAM, @DepartmentCode, @TargetDateTo , @ArrivalPlanDateFrom, @ArrivalPlanDateTo, @SlipNumber, @Vin, @CustomerName

			--DEBUG
			--SELECT * FROM #Temp_ServiceSalesHeader

			--�C���f�b�N�X�Đ���	
			DROP INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader
			CREATE UNIQUE INDEX IX_Temp_ServiceSalesHeader ON #Temp_ServiceSalesHeader ([SlipNumber], [RevisionNumber])
			
			---------------------------------------------
			--�d�|�f�[�^�擾(�O���E���i�܂Ƃ߂Ď擾)
			---------------------------------------------
			SET @PARAM = '@ServiceType NVARCHAR(3), @PartsNumber NVARCHAR(25), @PartsNameJp NVARCHAR(50)'

				SET @SQL = ''
				SET @SQL = @SQL +'INSERT INTO #Temp_ServiceSales' + @CRLF
				SET @SQL = @SQL +'SELECT' + @CRLF
				SET @SQL = @SQL +'		sh.[SlipNumber]' + @CRLF														--�`�[�ԍ�
				SET @SQL = @SQL +',		sh.[RevisionNumber]' + @CRLF													--���r�W�����ԍ�
				SET @SQL = @SQL +',		sh.[ServiceOrderStatus]' + @CRLF												--�`�[�X�e�[�^�X
				SET @SQL = @SQL +',		sh.[FrontEmployeeCode]' + @CRLF													--�T�[�r�X�S����
				SET @SQL = @SQL +',		sh.[CustomerCode]' + @CRLF														--�ڋq�R�[�h
				SET @SQL = @SQL +',		sh.[CustomerName]' + @CRLF														--�ڋq�R�[�h
				SET @SQL = @SQL +',		sh.[CarName]' + @CRLF															--�Ԏ햼
				SET @SQL = @SQL +',		sh.[Vin]' + @CRLF																--�ԑ�ԍ�
				SET @SQL = @SQL +',		sh.[ArrivalPlanDate]' + @CRLF													--���ɗ\���
				SET @SQL = @SQL +',		sl.[LineNumber]' + @CRLF														--�s�ԍ�
				SET @SQL = @SQL +',		sl.[ServiceWorkCode]' + @CRLF													--���ƃR�[�h
				SET @SQL = @SQL +',		CASE WHEN sl.[ServiceType] = ''003'' THEN sl.[StockStatus]' + @CRLF				--�݌ɏ�	--Mod 2016/07/14 arc yano				
				SET @SQL = @SQL +'		ELSE '''' END AS StockStatus' + @CRLF
				SET @SQL = @SQL +',		sl.[ServiceType]' + @CRLF														--���(002:�T�[�r�X���j���[ 003:���i)
				SET @SQL = @SQL +',		CASE WHEN sl.ServiceType = ''003'' THEN ''���i''' + @CRLF						--��ʖ�					
				SET @SQL = @SQL +'		ELSE ''�O��'' END AS ServiceType1' + @CRLF
				SET @SQL = @SQL +',		CASE WHEN sl.ServiceType = ''003'' THEN sl.[PartsNumber]' + @CRLF				--���i�ԍ�					
				SET @SQL = @SQL +'		ELSE '''' END AS PartsNumber' + @CRLF
				SET @SQL = @SQL +',		CASE WHEN sl.ServiceType = ''003'' THEN sl.[LineContents]' + @CRLF				--���i����					
				SET @SQL = @SQL +'		ELSE '''' END AS LineContents1' + @CRLF
				SET @SQL = @SQL +',		ISNULL(sl.[Quantity], 0) AS Quantity' + @CRLF							--����
				SET @SQL = @SQL +',		CASE WHEN sl.ServiceType = ''003'' THEN 0' + @CRLF											
				SET @SQL = @SQL +'		ELSE ISNULL(sl.[Cost], 0) END AS OutOrderCost' + @CRLF					--�O����
				SET @SQL = @SQL +',		sh.[DepartmentCode]' + @CRLF											--����R�[�h
				SET @SQL = @SQL +',		RTRIM(ISNULL(sl.[EmployeeCode], '''')) AS EmployeeCode' + @CRLF			--���J�j�b�N�S����
				SET @SQL = @SQL +',		RTRIM(ISNULL(sl.[SupplierCode], '''')) AS SupplierCode' + @CRLF			--�O����
				SET @SQL = @SQL +',		CASE WHEN sl.ServiceType = ''003'' THEN ''''' + @CRLF					--��Ɩ�				
				SET @SQL = @SQL +'		ELSE sl.[LineContents] END AS LineContents2' + @CRLF
				SET @SQL = @SQL +'FROM' + @CRLF
				SET @SQL = @SQL +'	#Temp_ServiceSalesHeader sh' + @CRLF
				SET @SQL = @SQL +'	INNER JOIN dbo.[ServiceSalesLine] sl' + @CRLF
				SET @SQL = @SQL +'		ON sl.[SlipNumber] = sh.[SlipNumber] ' + @CRLF
				SET @SQL = @SQL +'		AND sl.[RevisionNumber] = sh.[RevisionNumber]' + @CRLF
				SET @SQL = @SQL +'WHERE' + @CRLF
				SET @SQL = @SQL +'	ISNULL(sl.[DelFlag], ''0'') = ''0''' + @CRLF
				SET @SQL = @SQL +'	AND (' + @CRLF
				SET @SQL = @SQL +'			(' + @CRLF
				SET @SQL = @SQL +'				sl.[ServiceType] = ''002''' + @CRLF
				SET @SQL = @SQL +'				AND sl.[SupplierCode] is not null' + @CRLF
				SET @SQL = @SQL +'			)' + @CRLF
				SET @SQL = @SQL +'			OR' + @CRLF
				SET @SQL = @SQL +'			(' + @CRLF
				SET @SQL = @SQL +'				sl.[ServiceType] = ''003''' + @CRLF
				SET @SQL = @SQL +'				AND ISNULL(sl.[WorkType], '''') <> ''015''' + @CRLF
				
				SET @SQL = @SQL +'			)' + @CRLF
				SET @SQL = @SQL +'		)' + @CRLF
						--���׎��
				IF ((@ServiceType is not null) AND (@ServiceType <> ''))
				BEGIN
					SET @SQL = @SQL +' AND sl.[ServiceType] = @ServiceType' + @CRLF
				END

				--���i�ԍ�
				IF ((@PartsNumber is not null) AND (@PartsNumber <> ''))
				BEGIN
					SET @SQL = @SQL +' AND sl.[PartsNumber] LIKE ''%' + @PartsNumber + '%''' + @CRLF
				END

				--���i��
				IF ((@PartsNameJp is not null) AND (@PartsNameJp <> ''))
				BEGIN
					SET @SQL = @SQL +' AND sl.[LineContents] LIKE ''%' + @PartsNameJp + '%''' + @CRLF
				END
				
				--DEBUG
				--PRINT @SQL

				EXECUTE sp_executeSQL @SQL, @PARAM, @ServiceType, @PartsNumber, @PartsNameJp
				
				--DEBUG
				--SELECT * FROM #Temp_ServiceSales


				--�C���f�b�N�X�Đ���	
				DROP INDEX IX_Temp_ServiceSales ON #Temp_ServiceSales
				CREATE UNIQUE INDEX IX_Temp_ServiceSales ON #Temp_ServiceSales ([SlipNumber], [RevisionNumber], [LineNumber])
			
			-----------------------
			--�������e
			-----------------------
			IF @ActionFlag = 0	--����w�肪�u��ʕ\���v
			BEGIN
				/***************************************************/
				/*����w��=�u�\���v�̏ꍇ�̓f�[�^�擾���s��		   */
				/***************************************************/
				SET @PARAM = '@TargetMonth datetime'
					
				SET @SQL = ''
				SET @SQL = @SQL +'SELECT' + @CRLF
				SET @SQL = @SQL +'    @TargetMonth AS InventoryMonth' + @CRLF
				SET @SQL = @SQL +'  , SS.DepartmentCode AS DepartmentCode' + @CRLF
				SET @SQL = @SQL +'  , SS.ArrivalPlanDate' + @CRLF
				SET @SQL = @SQL +'	, SS.SlipNumber' + @CRLF
				SET @SQL = @SQL +'	, SS.LineNumber' + @CRLF
				SET @SQL = @SQL +'	, SS.ServiceOrderStatus' + @CRLF
				SET @SQL = @SQL +'	, C1.Name AS ServiceOrderStatusName' + @CRLF
				SET @SQL = @SQL +'	, SS.ServiceWorkCode AS ServiceWorkCode' + @CRLF
				SET @SQL = @SQL +'	, W.Name AS ServiceWorksName' + @CRLF
				SET @SQL = @SQL +'	, E1.EmployeeName AS FrontEmployeeName' + @CRLF
				SET @SQL = @SQL +'	, E2.EmployeeName AS MekaEmployeeName' + @CRLF
				SET @SQL = @SQL +'	, SS.CustomerCode' + @CRLF
				SET @SQL = @SQL +'	, SS.CustomerName' + @CRLF
				SET @SQL = @SQL +'	, SS.CarName' + @CRLF
				SET @SQL = @SQL +'	, SS.Vin' + @CRLF
				SET @SQL = @SQL +'	, SS.ServiceType AS ServiceType' + @CRLF
				SET @SQL = @SQL +'	, SS.ServiceType1 AS ServiceTypeName' + @CRLF
				SET @SQL = @SQL +'	, C2.Name AS StockTypeName' + @CRLF
				SET @SQL = @SQL +'	, NULL	AS PurchaseOrderDate' + @CRLF
				SET @SQL = @SQL +'	, NULL AS PartsArrivalPlanDate' + @CRLF
				SET @SQL = @SQL +'	, NULL AS PurchaseDate' + @CRLF
				SET @SQL = @SQL +'	, SS.PartsNumber AS PartsNumber' + @CRLF
				SET @SQL = @SQL +'	, SS.LineContents1 AS LineContents1' + @CRLF
				SET @SQL = @SQL +'	, COALESCE(pa.Price, P.SoPrice ,P.Cost, 0) AS Price' + @CRLF
				SET @SQL = @SQL +'	, SS.Quantity AS Quantity' + @CRLF
				SET @SQL = @SQL +'	, COALESCE(pa.Price, P.SoPrice ,P.Cost, 0) * SS.Quantity AS Amount' + @CRLF
				SET @SQL = @SQL +'	, S.SupplierName' + @CRLF
				SET @SQL = @SQL +'	, SS.LineContents2 AS LineContents2' + @CRLF
				SET @SQL = @SQL +'	, SS.OutOrderCost AS Cost' + @CRLF
				SET @SQL = @SQL +'FROM' + @CRLF
				SET @SQL = @SQL +'	#Temp_ServiceSales SS' + @CRLF
				SET @SQL = @SQL +'INNER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.ServiceWork AS W' + @CRLF
				SET @SQL = @SQL +'	ON SS.ServiceWorkCode = W.ServiceWorkCode' + @CRLF
				SET @SQL = @SQL +'INNER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.c_ServiceOrderStatus AS C1' + @CRLF
				SET @SQL = @SQL +'	ON  SS.ServiceOrderStatus = C1.Code' + @CRLF
				SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.c_StockStatus AS C2' + @CRLF
				SET @SQL = @SQL +'	ON  SS.StockStatus = C2.Code' + @CRLF
				SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF			--Mod 2015/10/13 arc yano
				SET @SQL = @SQL +'	dbo.Parts AS P' + @CRLF
				SET @SQL = @SQL +'	ON  SS.PartsNumber = P.PartsNumber' + @CRLF
				SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.Employee AS E1 ' + @CRLF
				SET @SQL = @SQL +'	ON  SS.FrontEmployeeCode = E1.EmployeeCode' + @CRLF
				SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.Employee AS E2 ' + @CRLF
				SET @SQL = @SQL +'	ON  SS.EmployeeCode = E2.EmployeeCode' + @CRLF
				SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.Supplier AS S ' + @CRLF
				SET @SQL = @SQL +'	ON  SS.SupplierCode = S.SupplierCode' + @CRLF
				SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF
				SET @SQL = @SQL +'	dbo.PartsAverageCost AS pa' + @CRLF
				SET @SQL = @SQL +'	ON pa.PartsNumber = SS.PartsNumber' + @CRLF
				SET @SQL = @SQL +'	AND pa.CloseMonth = @TargetMonth' + @CRLF
				SET @SQL = @SQL +'WHERE' + @CRLF
				SET @SQL = @SQL +'	ISNULL(p.Delflag, ''0'') <> ''1''' + @CRLF						--Add 2015/10/13 arc yano
				SET @SQL = @SQL +'	AND ISNULL(p.NonInventoryFlag, ''0'') <> ''1''' + @CRLF			--Add 2015/10/13 arc yano
				SET @SQL = @SQL +'	AND (' + @CRLF													--Add 2016/07/14 arc yano
				SET @SQL = @SQL +'			P.PartsNumber is NULL' + @CRLF							--Add 2016/07/14 arc yano
				--SET @SQL = @SQL +'			OR C2.SelectedGenuineType = P.GenuineType' + @CRLF		--Add 2016/07/14 arc yano	--Mod 2017/10/19 arc yano #3803
				SET @SQL = @SQL +'		)' + @CRLF	
				--���ёւ�
				SET @SQL = @SQL +'ORDER BY' + @CRLF
				SET @SQL = @SQL +'	 SS.ServiceOrderStatus' + @CRLF
				SET @SQL = @SQL +'	,SS.SlipNumber' + @CRLF
				SET @SQL = @SQL +'	,SS.LineNumber' + @CRLF



			--DEBUG
			--PRINT @SQL
			
				EXECUTE sp_executeSQL @SQL, @PARAM, @TargetMonth
			END
			ELSE
			BEGIN
				--��x�Ώ۔N���̃f�[�^���폜
				
				--Mod 2016/08/13 arc yano #3596�@����R�[�h������ꍇ�͕���R�[�h�Œ��o����
				--����R�[�h
				IF ((@DepartmentCode is not null) AND (@DepartmentCode <> ''))
				BEGIN
					DELETE 
						ips 
					FROM 
						dbo.InventoryParts_Shikakari ips
					WHERE
			 			ips.InventoryMonth = @TargetMonth
					AND 
						DepartmentCode = @DepartmentCode
				END
				ELSE
				BEGIN
					DELETE 
						ips 
					FROM 
						dbo.InventoryParts_Shikakari ips
					WHERE
			 			ips.InventoryMonth = @TargetMonth
						AND 
						EXISTS
						(
							SELECT 'X' FROM #Temp_DepartmentListUseWarehouse dw WHERE ips.DepartmentCode = dw.DepartmentCode 
						)
				END

				INSERT INTO dbo.InventoryParts_Shikakari
				SELECT
					  @TargetMonth													--�Ώ۔N��
					, SS.DepartmentCode												--����R�[�h          
					, SS.ArrivalPlanDate											--���ɓ�
					, SS.SlipNumber													--�`�[�ԍ�
					, SS.LineNumber													--�s�ԍ�
					, SS.ServiceOrderStatus											--�`�[�X�e�[�^�X
					, C1.Name AS ServiceOrderStatusName								--�`�[�X�e�[�^�X��
					, SS.ServiceWorkCode											--���ƃR�[�h
					, W.Name AS ServiceWorksName									--���Ɩ�
					, E1.EmployeeName AS FrontEmployeeName							--�t�����g�S���Җ�
					, E2.EmployeeName AS MekaEmployeeName							--���J�j�b�N�S���Җ�
					, SS.CustomerCode												--�ڋq�R�[�h
					, SS.CustomerName												--�ڋq��
					, SS.CarName													--�Ԏ햼
					, SS.Vin														--�ԑ�ԍ�
					, SS.ServiceType												--�T�[�r�X���
					, SS.ServiceType1 AS ServiceTypeName							--�T�[�r�X��ʖ�
					, C2.Name AS StockTypeName										--�݌ɏ�
					, NULL AS PurchaseOrderDate
					, NULL AS PartsArrivalPlanDate
					, NULL AS PurchaseDate
					, SS.PartsNumber												--���i�ԍ�
					, SS.LineContents1 AS LineContents1								--���i��
					, 0 AS Price													--�P�� ���X�i�b�v�V���b�g�ۑ����͂O
					, SS.Quantity AS Quantity										--����
					, 0 AS TotalAmount												--���z ���X�i�b�v�V���b�g�ۑ����͂O
					, S.SupplierName												--�O����
					, SS.LineContents2 AS LineContents2								--�T�[�r�X��				
					, SS.OutOrderCost												--�T�[�r�X��
				FROM
					#Temp_ServiceSales AS SS 
					INNER JOIN dbo.ServiceWork AS W 
						ON SS.ServiceWorkCode = W.ServiceWorkCode 
					INNER JOIN dbo.c_ServiceOrderStatus AS C1 
						ON SS.ServiceOrderStatus = C1.Code 
					LEFT OUTER JOIN dbo.c_StockStatus AS C2 
						ON SS.StockStatus = C2.Code 
					LEFT OUTER JOIN dbo.Employee AS E1 
						ON SS.FrontEmployeeCode = E1.EmployeeCode 
					LEFT OUTER JOIN dbo.Employee AS E2 
						ON SS.EmployeeCode = E2.EmployeeCode
					LEFT OUTER JOIN dbo.Supplier AS S 
						ON SS.SupplierCode = S.SupplierCode
					LEFT OUTER JOIN dbo.Parts AS P								--ADD 2016/07/14 arc yano 
						ON SS.PartsNumber = P.PartsNumber
				WHERE															--ADD 2016/07/14 arc yano 
					ISNULL(P.Delflag, '0') <> '1' AND
					ISNULL(P.NonInventoryFlag, '0') <> '1' AND
				    (
						P.PartsNumber IS NULL --OR								--Del 2017/10/19 arc yano #3803
						--C2.SelectedGenuineType = P.GenuineType
					)
			END
		END
		ELSE
		BEGIN
			/***************************************************/
			/*�I�����I�������Ώی��̎d�|�݌ɂ̕\��		   */
			/***************************************************/
			SET @PARAM = '@TargetMonth datetime,@DepartmentCode NVARCHAR(3), @ServiceType NVARCHAR(3), @ArrivalPlanDateFrom NVARCHAR(10), @ArrivalPlanDateTo NVARCHAR(10), @SlipNumber NVARCHAR(50), @PartsNumber NVARCHAR(25), @PartsNameJp NVARCHAR(50), @Vin NVARCHAR(20), @CustomerName NVARCHAR(80)'
					
			SET @SQL = ''
			SET @SQL = @SQL +'SELECT' + @CRLF
			SET @SQL = @SQL +'    ips.InventoryMonth AS InventoryMonth' + @CRLF
			SET @SQL = @SQL +'  , ips.DepartmentCode AS DepartmentCode' + @CRLF
			SET @SQL = @SQL +'  , ips.ArrivalPlanDate' + @CRLF
			SET @SQL = @SQL +'	, ips.SlipNumber' + @CRLF
			SET @SQL = @SQL +'	, ips.LineNumber' + @CRLF
			SET @SQL = @SQL +'	, ips.ServiceOrderStatus' + @CRLF
			SET @SQL = @SQL +'	, ips.ServiceOrderStatusName' + @CRLF
			SET @SQL = @SQL +'	, ips.ServiceWorkCode' + @CRLF
			SET @SQL = @SQL +'	, ips.ServiceWorksName' + @CRLF
			SET @SQL = @SQL +'	, ips.FrontEmployeeName' + @CRLF
			SET @SQL = @SQL +'	, ips.MekaEmployeeName' + @CRLF
			SET @SQL = @SQL +'	, ips.CustomerCode' + @CRLF
			SET @SQL = @SQL +'	, ips.CustomerName' + @CRLF
			SET @SQL = @SQL +'	, ips.CarName' + @CRLF
			SET @SQL = @SQL +'	, ips.Vin' + @CRLF
			SET @SQL = @SQL +'	, ips.ServiceType' + @CRLF
			SET @SQL = @SQL +'	, ips.ServiceTypeName' + @CRLF
			SET @SQL = @SQL +'	, ips.StockTypeName'  + @CRLF
			SET @SQL = @SQL +'	, ips.PurchaseOrderDate' + @CRLF
			SET @SQL = @SQL +'	, ips.PartsArravalPlanDate as PartsArrivalPlanDate' + @CRLF
			SET @SQL = @SQL +'	, ips.PurchaseDate' + @CRLF
			SET @SQL = @SQL +'	, ips.PartsNumber' + @CRLF
			SET @SQL = @SQL +'	, ips.LineContents1' + @CRLF
			SET @SQL = @SQL +'	, COALESCE(pa.Price, p.SoPrice ,p.Cost, 0) AS Price' + @CRLF
			SET @SQL = @SQL +'	, ips.Quantity' + @CRLF
			SET @SQL = @SQL +'	, COALESCE(pa.Price, p.SoPrice ,p.Cost, 0) * ips.Quantity AS Amount' + @CRLF
			SET @SQL = @SQL +'	, ips.SupplierName' + @CRLF
			SET @SQL = @SQL +'	, ips.LineContents2' + @CRLF
			SET @SQL = @SQL +'	, ips.Cost AS Cost' + @CRLF
			SET @SQL = @SQL +'FROM' + @CRLF
			SET @SQL = @SQL +'	dbo.InventoryParts_Shikakari ips' + @CRLF
			SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF
			SET @SQL = @SQL +'	dbo.PartsAverageCost AS pa' + @CRLF
			SET @SQL = @SQL +'	ON pa.PartsNumber = ips.PartsNumber' + @CRLF
			SET @SQL = @SQL +'  AND  pa.CloseMonth = @TargetMonth' + @CRLF
			SET @SQL = @SQL +'LEFT OUTER JOIN' + @CRLF				--Mod 2015/10/13 arc yano
			SET @SQL = @SQL +'	dbo.Parts AS p' + @CRLF
			SET @SQL = @SQL +'	ON p.PartsNumber = ips.PartsNumber' + @CRLF
			SET @SQL = @SQL +'WHERE' + @CRLF
			SET @SQL = @SQL +'	ips.InventoryMonth = @TargetMonth' + @CRLF
			SET @SQL = @SQL +'	AND ips.DepartmentCode = @DepartmentCode' + @CRLF
			SET @SQL = @SQL +'	AND ISNULL(p.DelFlag, ''0'') <> ''1''' + @CRLF	--Add 2015/10/13 arc yano
			SET @SQL = @SQL +'	AND ISNULL(p.NonInventoryFlag, ''0'') <> ''1''' + @CRLF	--Add 2015/10/13 arc yano

			--���׎��
			IF ((@ServiceType is not null) AND (@ServiceType <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ips.ServiceType = @ServiceType' + @CRLF
			END

			--���ɗ\���
			IF ((@ArrivalPlanDateFrom is not null) AND (@ArrivalPlanDateFrom <> '') AND ISDATE(@ArrivalPlanDateFrom) = 1)
				IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
				BEGIN
					SET @SQL = @SQL +' AND ips.ArrivalPlanDate >= @ArrivalPlanDateFrom AND ips.ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF
				END
				ELSE
				BEGIN
					SET @SQL = @SQL +' AND  ips.ArrivalPlanDate = @ArrivalPlanDateFrom' + @CRLF 
				END
			ELSE
				IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
				BEGIN
					SET @SQL = @SQL +' AND ips.ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF 
				END

			--�`�[�ԍ�
			IF ((@SlipNumber is not null) AND (@SlipNumber <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ips.SlipNumber LIKE ''%' + @SlipNumber + '%''' + @CRLF
			END

			--���i�ԍ�
			IF ((@PartsNumber is not null) AND (@PartsNumber <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ips.PartsNumber LIKE ''%' + @PartsNumber + '%''' + @CRLF
			END

			--���i��
			IF ((@PartsNameJp is not null) AND (@PartsNameJp <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ips.LineContents1 LIKE ''%' + @PartsNameJp + '%''' + @CRLF
			END

			--�ԑ�ԍ�
			IF ((@Vin is not null) AND (@Vin <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ips.Vin LIKE ''%' + @Vin + '%''' + @CRLF
			END

			--�ڋq��
			IF ((@CustomerName is not null) AND (@CustomerName <> ''))
			BEGIN
				SET @SQL = @SQL +' AND ips.CustomerName LIKE ''%' + @CustomerName + '%''' + @CRLF
			END

			--���ёւ�
			SET @SQL = @SQL +'ORDER BY' + @CRLF
			SET @SQL = @SQL +'	 ips.ServiceOrderStatus' + @CRLF
			SET @SQL = @SQL +'	,ips.SlipNumber' + @CRLF
			SET @SQL = @SQL +'	,ips.LineNumber' + @CRLF

			--DEBUG
			--PRINT @SQL
			
			EXECUTE sp_executeSQL @SQL, @PARAM, @TargetMonth,@DepartmentCode , @ServiceType, @ArrivalPlanDateFrom, @ArrivalPlanDateTo, @SlipNumber, @PartsNumber, @PartsNameJp, @Vin, @CustomerName
		END

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


