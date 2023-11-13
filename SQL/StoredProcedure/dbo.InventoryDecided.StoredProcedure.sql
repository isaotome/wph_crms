USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[InventoryDecided]    Script Date: 2019/06/04 15:20:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[InventoryDecided]
	 @InventoryMonth datetime,			--�I����
	 @WarehouseCode nvarchar(6),		--�q�ɃR�[�h
	 @EmployeeCode nvarchar(50)			--�Ј��R�[�h
AS
BEGIN
	SET NOCOUNT ON;

	/*------------------------------------------------------------------------------------------------------------------------------------------------------
		Mod 2019/05/22 yano #3974 ���i�݌ɒI���@�I���m�莩�̕␳�����̕s� 
		Mod 2017/02/08 arc yano #3620 �T�[�r�X�`�[���́@�`�[�ۑ��A�폜�A�ԓ`���̕��i�̍݌ɂ̖߂��Ή�
		Mod 2016/08/13 arc yano #3596�y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
		Mod 2016/06/09 arc yano #3571 ���i�݌ɒI���@�I���m�莞�̕s�
		Mod 2016/02/08 arc yano #3409 ���i�I���e�[�u��(dbo.InventoryStock)�̃e�[�u���\���ύX(ProvisionQuantity�ǉ�)�ɔ����ASQL�̏C��
		Mod 2015/09/25 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�G �d�|�݌ɂ͌v�Z�ŏo���悤�ɏC��
		Mod 2015/06/16 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�E InventoryStock�APartsStock�̍݌ɐ��̍X�V���@�̕ύX
		Mod 2015/05/20 arc yano IPO�Ή�(���i�I��) �I����Ɠ��`�I���J�n���܂ł̍݌ɕϓ��̒���
		1.InventoryStock
		�@�@�Ώ۔N������23:59:59�ƒI���J�n�����̊Ԃɔ��������݌ɐ��̑������𔽉f������B

				�@�I���J�n���� >= �Ώ۔N�����̏ꍇ�c�Ώ۔N�����`�I���J�n�����̍݌ɑ������̐��ʂ����I�����獷�������B
				�A�Ώ۔N���� > �I���J�n�����̏ꍇ�c�I���J�n�����`�Ώ۔N�����̍݌ɑ������̐��ʂ����I�ɑ����B

		2.PartsStock
			�I���J�n�������_��PartsStock�̐��ʂƌv���������I�݌ɐ��̍�����PartsStock�̐��ʂɔ��f������B                                                         
	--------------------------------------------------------------------------------------------------------------------------------------------------------*/

	DECLARE @targetDateFrom datetime											--�Ώ۔͈�(From)
	DECLARE @targetDateTo datetime 												--�Ώ۔͈�(To)

	DECLARE @targetNextMonth datetime											--�Ώ۔N���̗���1��
	DECLARE @PartInventoryStart datetime										--�I���J�n����

	DECLARE @Now datetime = GETDATE()											--���ݓ���

	DECLARE @CalcMode int														--�␳���@(0:�݌ɑ����������Z�A1:�݌ɑ����������Z)


	--�Ώ۔N���̗������擾����
	SET @targetNextMonth =  DATEADD(m, 1, @InventoryMonth)

	
	--�I���J�n�������擾����B
	SELECT
		@PartInventoryStart = CONVERT(DATE, StartDate) 
	FROM
		dbo.InventoryScheduleParts
	WHERE
		InventoryMonth = @InventoryMonth AND
		WarehouseCode = @WarehouseCode											--Mod 2016/08/13 arc yano #3596


	--�Ώ۔͈�(From/To)�̐ݒ�
	IF @PartInventoryStart >= @targetNextMonth
	BEGIN
		SET @targetDateFrom = @targetNextMonth									--�Ώ۔͈�(From) = �Ώ۔N���̗���1��
		SET @targetDateTo = @PartInventoryStart									--�Ώ۔͈�(To) = �I���J�n����
		SET @CalcMode = 0														--�݌ɑ����������Z
	END
	ELSE	--�Ώ۔N���������̏ꍇ
	BEGIN
		SET @targetDateFrom = DATEADD(d, 1, @PartInventoryStart)				--�Ώ۔͈�(From) = �I���J�n���̗���		--Mod 2019/05/22 yano #3974
		SET @targetDateTo = @targetNextMonth									--�Ώ۔͈�(To) = �Ώ۔N���̗���1��
		SET @CalcMode = 1														--�݌ɑ����������Z
	END

	--�d�|���P�[�V�����R�[�h�̐ݒ�
	DECLARE @ShikakariLocationCode NVARCHAR(12) --�d�|���P�[�V�����R�[�h

	--�d�|���P�[�V�����R�[�h���擾
	SELECT 
		@ShikakariLocationCode = LocationCode
	FROM
		dbo.Location
	WHERE
		WarehouseCode = @WarehouseCode AND										--Mod 2016/08/13 arc yano #3596
		LocationType = '003'													--���P�[�V�������=�d�|
	

	--���i���I���X�g(���P�[�V�����^���i��)
	CREATE TABLE #InventoryStock (
		[LocationCode] NVARCHAR(12) NOT NULL			--���P�[�V�����R�[�h
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--���i�ԍ�
	,	[Quantity] DECIMAL(10, 2)						--�݌ɐ�(�I���J�n���_�̗��_�݌�)
	,	[PhysicalQuantity] DECIMAL(10, 2)				--���I��
	,	[ProvisionQuantity] DECIMAL(10, 2)				--�����ϐ�			--Mod 2016/02/08 arc yano #3409
	)
	CREATE UNIQUE INDEX IX_Temp_InventoryStock ON #InventoryStock ([LocationCode], [PartsNumber])

	--�d�����X�g(���P�[�V�����^���i��)
	CREATE TABLE #PartsPurchase (
		[LocationCode] NVARCHAR(12) NOT NULL			--���P�[�V�����R�[�h
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--���i�ԍ�
	,	[Quantity] DECIMAL(10, 2)						--����
	,	[ProvisionQuantity] DECIMAL(10, 2)				--�����ϐ�			--Mod 2016/06/09 arc yano #3571
	)
	CREATE UNIQUE INDEX IX_Temp_PartsPurchase ON #PartsPurchase ([LocationCode], [PartsNumber])

	--�ړ�������X�g(���P�[�V�����^���i��)
	CREATE TABLE #TransferArrival (
		[LocationCode] NVARCHAR(12) NOT NULL			--���P�[�V�����R�[�h
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--���i�ԍ�
	,	[Quantity] DECIMAL(10, 2)						--����
	,	[ProvisionQuantity] DECIMAL(10, 2)				--�����ϐ�			--Mod 2016/06/09 arc yano #3571
	)
	CREATE UNIQUE INDEX IX_Temp_TransferArrival ON #TransferArrival ([LocationCode], [PartsNumber])

	--�̔����X�g(���P�[�V�����^���i��)
	CREATE TABLE #ServiceSales (
		[LocationCode] NVARCHAR(12) NOT NULL			--���P�[�V�����R�[�h
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--���i�ԍ�
	,	[Quantity] DECIMAL(10, 2)						--����
	,	[ProvisionQuantity] DECIMAL(10, 2)				--�����ϐ�			--Mod 2016/06/09 arc yano #3571
	)
	CREATE UNIQUE INDEX IX_Temp_ServiceSales ON #ServiceSales ([PartsNumber])

	
	--�ړ����o���X�g(���P�[�V�����^���i��)
	CREATE TABLE #TransferDeparture (
		[LocationCode] NVARCHAR(12) NOT NULL			--���P�[�V�����R�[�h
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--���i�ԍ�
	,	[Quantity] DECIMAL(10, 2)						--����
	,	[ProvisionQuantity] DECIMAL(10, 2)				--�����ϐ�			--Mod 2016/06/09 arc yano #3571
	)
	CREATE UNIQUE INDEX IX_Temp_TransferDeparture ON #TransferDeparture ([LocationCode], [PartsNumber])


	--���I���������X�g(���P�[�V�����^���i��)
	CREATE TABLE #AdjustmentStock (
		[LocationCode] NVARCHAR(12) NOT NULL			--���P�[�V�����R�[�h
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--���i�ԍ�
	,	[Quantity] DECIMAL(10, 3)						--���I��(�␳�����O)
	,	[CalcQuantity] DECIMAL(10, 3)					--���I��(�␳������)
	,	[ProvisionQuantity] DECIMAL(10, 2)				--�����ϐ�			--Mod 2016/06/09 arc yano #3571
	)
	CREATE UNIQUE INDEX IX_Temp_AdjustmentStock ON #AdjustmentStock ([LocationCode], [PartsNumber], [Quantity])

	--Add 2015/07/17 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�F �d�|�̎��I���͌v�Z�ŏo���悤�ɕύX����B
	--�d�|���X�g(���P�[�V�����^���i��)
	CREATE TABLE #InProcess (
		[PartsNumber] NVARCHAR(50) NOT NULL				--���i�ԍ�
	,	[Quantity] DECIMAL(10, 3)						--���I��
	)
	CREATE UNIQUE INDEX IX_Temp_InProcess ON #InProcess ([PartsNumber], [Quantity])

	--�T�[�r�X�`�[�w�b�_(�ΏۊO�f�[�^)
	CREATE TABLE #Temp_ServiceSalesHeader_Exempt (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	)

	--Add 2016/06/09 arc yano #3571
	--�ړ�(����/��������)���X�g(���P�[�V�����^���i��)
	CREATE TABLE #TransferProvision (
		[LocationCode] NVARCHAR(12) NOT NULL			--���P�[�V�����R�[�h
	,	[PartsNumber] NVARCHAR(50) NOT NULL				--���i�ԍ�
	,	[Quantity] DECIMAL(10, 2)						--����
	,	[ProvisionQuantity] DECIMAL(10, 2)				--�����ϐ�			--Mod 2016/06/09 arc yano #3571
	)
	CREATE UNIQUE INDEX IX_Temp_TransferProvision ON #TransferProvision ([LocationCode], [PartsNumber])
	
	--Add 2016/08/13 arc yano #3596
	--����ꗗ
	CREATE TABLE #DepartmentListUseWarehouse (
		[DepartmentCode] NVARCHAR(3) NOT NULL			--����R�[�h
	,	[WarehouseCode] NVARCHAR(6) NOT NULL			--�q�ɃR�[�h
	)
	CREATE UNIQUE INDEX IX_Temp_DepartmentListUseWarehouse ON #DepartmentListUseWarehouse ([DepartmentCode], [WarehouseCode])

	/********************
	�������僊�X�g�擾
	*********************/
	--����E�q�ɂ̑g�������X�g���A�q�ɂ��g�p���Ă���S�ĕ�����擾����
	INSERT INTO #DepartmentListUseWarehouse
	SELECT
		 dw.DepartmentCode		--����R�[�h
		,dw.WarehouseCode		--�q�ɃR�[�h
	FROM
		dbo.DepartmentWarehouse dw
	WHERE
		dw.WarehouseCode = @WarehouseCode			--�q�ɃR�[�h

	--�C���f�b�N�X�Đ���	
	DROP INDEX IX_Temp_DepartmentListUseWarehouse ON #DepartmentListUseWarehouse
	CREATE UNIQUE INDEX IX_Temp_DepartmentListUseWarehouse ON #DepartmentListUseWarehouse ([DepartmentCode], [WarehouseCode])

	/****************
	�����d�|
	****************/
	--�d�|���ꎞ�e�[�u���ɑ}��
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


	INSERT INTO #InProcess
	SELECT
		sl.[PartsNumber]
	,	SUM(ISNULL(sl.[ProvisionQuantity], 0)) AS [Quantity]			--Mod 2016/02/08 arc yano #3409
	--,	SUM(ISNULL(sl.[Quantity], 0)) AS [Quantity]
	
	FROM [ServiceSalesHeader] sh										--�T�[�r�X�I�[�_�[�w�b�_
	INNER JOIN [ServiceSalesLine] sl									--�T�[�r�X�I�[�_�[����
		ON sl.[SlipNumber] = sh.[SlipNumber] 
		AND ISNULL(sl.[DelFlag], '0') <> '1'
	INNER JOIN Parts p													--���i�}�X�^
		ON p.[PartsNumber] = sl.[PartsNumber]
	--WHERE sh.[DepartmentCode] = @DepartmentCode						--Mod 2016/08/13 arc yano #3596
	WHERE
		EXISTS
		(	
			SELECT 'X' FROM #DepartmentListUseWarehouse dw WHERE sh.DepartmentCode = dw.DepartmentCode 
		)
	 
	AND sh.[WorkingStartDate] < @targetDateTo							--�����_�ō�ƊJ�n���Ă���`�[
	--AND NOT (ISNULL(sh.[SalesDate], @targetDateTo) < @targetDateTo)	--�����_�Ŕ[�Ԃ��Ă��Ȃ��`�[
	--�����ȑO�ɃL�����Z��/��ƒ��~�̂Ȃ��`�[
	AND NOT EXISTS(
		SELECT 'X'
		FROM #Temp_ServiceSalesHeader_Exempt sub
		WHERE sub.[SlipNumber] = sh.[SlipNumber]	
		)
	AND ISNULL(sh.DelFlag, '0') <> '1'
	GROUP BY sl.PartsNumber
	
	--�C���f�b�N�X�Đ���
	DROP INDEX IX_Temp_InProcess ON #InProcess
	CREATE UNIQUE INDEX IX_Temp_InProcess ON #InProcess ([PartsNumber], [Quantity])

	
	--�d�|�f�[�^�X�V
	--�I���e�[�u���̍X�V
	UPDATE
		dbo.InventoryStock
	SET
		  PhysicalQuantity = ip.Quantity		--���I���v�Z�݌ɐ��ʂōX�V
		, ProvisionQuantity = ip.Quantity		--�����ϐ��������ōX�V		--Add 2016/06/09 arc yano #3571
		, LastUpdateEmployeeCode = 'sys'
		, LastUpdateDate = @Now
	FROM
		dbo.InventoryStock ivs INNER JOIN
		#InProcess ip ON ip.PartsNumber = ivs.PartsNumber AND ivs.LocationCode = @ShikakariLocationCode
	WHERE
		WarehouseCode = @WarehouseCode AND								--Mod 2016/08/13 arc yano #3596
		InventoryMonth = @InventoryMonth AND
		LocationCode = @ShikakariLocationCode	--���P�[�V�����R�[�h���d�|�̂���

	--�d�|�f�[�^�X�V�Q �v�Z�݌ɂɂȂ��d�|���P�[�V�����̐��ʂ͂O�ɂ���
	--�I���e�[�u���̍X�V
	UPDATE
		dbo.InventoryStock
	SET
		  PhysicalQuantity = 0		--���I���v�Z�݌Ƀe�[�u���ɂȂ����̂͐��ʂ��O�ɂ���
		, ProvisionQuantity = 0		--�����ϐ��������ōX�V		--Add 2016/06/09 arc yano #3571
		, LastUpdateEmployeeCode = 'sys'
		, LastUpdateDate = @Now
	FROM
		dbo.InventoryStock ivs
	WHERE
		ivs.WarehouseCode = @WarehouseCode AND						--Mod 2016/08/13 arc yano #3596
		ivs.InventoryMonth = @InventoryMonth AND
		ivs.LocationCode = @ShikakariLocationCode AND	--���P�[�V�����R�[�h���d�|�̂���
		NOT EXISTS
		(
			SELECT 'X' FROM #InProcess ip WHERE ip.PartsNumber = ivs.PartsNumber
		)

	--�d�|�f�[�^�̑}��
	INSERT
		dbo.InventoryStock
	SELECT
		  NEWID()					AS [InventoryID]
		, ''						AS [DepartmentCode]			--Mod 2016/08/13 arc yano #3596 ����R�[�h�͋󕶎���
		, @InventoryMonth			AS [InventoryMonth]
		, @ShikakariLocationCode	AS [LocationCode]
		, @EmployeeCode				AS [EmployeeCode]
		, '002'						AS [InventoryType]
		, NULL						AS [SalesCarNumber]
		, ip.PartsNumber			AS [PartsNumber]
		, ip.Quantity				AS [Quantity]
		, 'sys'						AS [CreateEmployeeCode]
		, @Now						AS [CreateDate]
		, 'sys'						AS [LastUpdateEmployeeCode]
		, @Now						AS [LastUpdateDate]
		, '0'						AS [DelFlag]
		, NULL						AS [Summary]
		, ip.Quantity				AS [PhysicalQuantity]
		, NULL						AS [Comment]
		, ip.Quantity				AS [ProvisionQuantity]		--Mod 2016/02/08 arc yano #3409
		, @WarehouseCode			AS [WarehouseCode]			--Add 2016/08/13 arc yano #3596
	FROM
		#InProcess ip
	WHERE
		NOT EXISTS
		(
			SELECT 'X' From dbo.InventoryStock ivs WHERE ivs.LocationCode = @ShikakariLocationCode AND ivs.PartsNumber = ip.PartsNumber
		)

	/****************
	�������I
	****************/
	INSERT INTO
		#InventoryStock
	SELECT
		   LocationCode
		 , PartsNumber
		 , Quantity
		 , PhysicalQuantity
		 , ProvisionQuantity	--�����ϐ�	Mod 2016/02/08 arc yano #3409
	FROM
		dbo.InventoryStock
	WHERE
		WarehouseCode  = @WarehouseCode AND					--Mod 2016/08/13 arc yano
		InventoryMonth = @InventoryMonth

	--�C���f�b�N�X�Đ���
	DROP INDEX IX_Temp_InventoryStock ON #InventoryStock
	CREATE UNIQUE INDEX IX_Temp_InventoryStock ON #InventoryStock ([LocationCode], [PartsNumber])

	/***********************************************
	�����[�ԁ@���߂���͎d�|���P�[�V����
	************************************************/
	--�_�[�e�B�[���[�h�̐ݒ�
	--SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	--�[�ԏ����ꎞ�e�[�u���ɑ}��
	INSERT INTO #ServiceSales
	SELECT
		@ShikakariLocationCode
	,	sl.[PartsNumber] AS [PartsNumber]
	,	SUM(ISNULL(sl.[ProvisionQuantity], 0)) AS [Quantity]				--Mod 2016/06/09 arc yano #3571
	,	SUM(ISNULL(sl.[ProvisionQuantity], 0)) AS [ProvisionQuantity]		--Add 2016/06/09 arc yano #3571
	FROM [ServiceSalesHeader] sh											--�T�[�r�X�I�[�_�[�w�b�_
	INNER JOIN [ServiceSalesLine] sl										--�T�[�r�X�I�[�_�[����
		ON sl.[SlipNumber] = sh.[SlipNumber] 
		AND ISNULL(sl.[DelFlag], '0') <> '1'
	INNER JOIN Parts p														--���i�}�X�^
		ON p.[PartsNumber] = sl.[PartsNumber]
	WHERE sh.[SalesDate] >= @TargetDateFrom									--�����P��
	AND sh.[SalesDate] < @TargetDateTo										--�I���J�n��
	--AND sh.[DepartmentCode] = @DepartmentCode								--����R�[�h
	AND EXISTS																--Mod 2016/08/13 arc yano #3596
	(
		SELECT 'X' FROM #DepartmentListUseWarehouse dw WHERE sh.DepartmentCode = dw.DepartmentCode
	)
	AND sh.[ServiceOrderStatus] = '006'										--�[�ԍ�
	AND ISNULL(sh.DelFlag, '0') <> '1'
	GROUP BY
		sl.[PartsNumber]
		
	--�C���f�b�N�X�Đ���
	DROP INDEX IX_Temp_ServiceSales ON #ServiceSales
	CREATE UNIQUE INDEX IX_Temp_ServiceSales ON #ServiceSales ([LocationCode], [PartsNumber])


	/****************
	�����ړ����
	****************/
	--�ړ������ꎞ�e�[�u���ɑ}��
	INSERT INTO #TransferArrival
	SELECT
		tr.[ArrivalLocationCode]
	,	tr.[PartsNumber]
	,	SUM(ISNULL(tr.[Quantity], 0)) AS [Quantity]																	--����
	,	SUM(
			CASE WHEN 
				(tr.TransferType = '003' OR tr.TransferType = '007') THEN ISNULL(tr.[Quantity], 0)					--�ړ����=�u�d�|�v�܂��͎d�|����
			ELSE 
				0  
			END
			) 
		AS [ProvisionQuantity]		--�����ϐ�(�ړ���ʂ��u�d�|�v�u�d�|�����v�̏ꍇ�� ���ʁA����ȊO��0)		--Add 2016/06/09 arc yano #3571
	FROM [Transfer] tr
	INNER JOIN Parts p																								--���i�}�X�^
		ON p.[PartsNumber] = tr.[PartsNumber] AND (ISNULL(p.[DelFlag], '0') <> '1')
	INNER JOIN [Location] l																							--���۹����
		ON l.[LocationCode] = tr.[ArrivalLocationCode]
	INNER JOIN [Location] l2																						--�o��۹����
		ON l2.[LocationCode] = tr.[DepartureLocationCode]
	WHERE tr.[ArrivalDate] >= @TargetDateFrom
	AND tr.[ArrivalDate] < @TargetDateTo
	--AND l.[DepartmentCode] = @DepartmentCode
	AND l.[WarehouseCode] = @WarehouseCode																			--Mod 2016/08/13 arc yano #3596
	AND ISNULL(tr.[DelFlag], '0') <> '1'
	AND TransferType <> '006'																						--�ړ����=�u���������v
	AND TransferType <> '008'																						--�ړ����=�u���������v
	--AND l.DepartmentCode <> l2.DepartmentCode
	GROUP BY
		tr.[ArrivalLocationCode]
	,	tr.[PartsNumber]
	--�C���f�b�N�X�Đ���
	DROP INDEX IX_Temp_TransferArrival ON #TransferArrival
	CREATE UNIQUE INDEX IX_Temp_TransferArrival ON #TransferArrival ([LocationCode], [PartsNumber])

	/****************
	�����ړ����o
	****************/
	--�ړ������ꎞ�e�[�u���ɑ}��
	INSERT INTO #TransferDeparture
	SELECT
		tr.[DepartureLocationCode]
	,	tr.[PartsNumber]
	,	SUM(ISNULL(tr.[Quantity], 0)) AS [Quantity]
	,	SUM(
			CASE WHEN 
				(tr.TransferType = '003' OR tr.TransferType = '007') THEN ISNULL(tr.[Quantity], 0)					--�ړ����=�u�d�|�v�܂��͎d�|����
			ELSE 
				0  
			END
			) 
		AS [ProvisionQuantity]		--�����ϐ�(�ړ���ʂ��u�d�|�v�u�d�|�����v�̏ꍇ�� ���ʁA����ȊO��0)		--Add 2016/06/09 arc yano #3571
	FROM [Transfer] tr
	INNER JOIN Parts p																								--���i�}�X�^
		ON p.[PartsNumber] = tr.[PartsNumber] AND (ISNULL(p.[DelFlag], '0') <> '1')
	INNER JOIN [Location] l																							--�o��۹����
		ON l.[LocationCode] = tr.[DepartureLocationCode]
	INNER JOIN [Location] l2																						--���۹����
		ON l2.[LocationCode] = tr.[ArrivalLocationCode]
	WHERE tr.[DepartureDate] >= @TargetDateFrom
	AND tr.[DepartureDate] < @TargetDateTo
	--AND l.[DepartmentCode] = @DepartmentCode																		--Mod 2016/08/13 arc yano #3596
	AND l.[WarehouseCode] = @WarehouseCode
	AND ISNULL(tr.[DelFlag], '0') <> '1'
	AND TransferType <> '006'																						--�ړ����=�u���������v
	AND TransferType <> '008'																						--�ړ����=�u���������v
	--AND l.DepartmentCode <> l2.DepartmentCode
	GROUP BY
		tr.[DepartureLocationCode]
	,	tr.[PartsNumber]
	--�C���f�b�N�X�Đ���
	DROP INDEX IX_Temp_TransferDeparture ON #TransferDeparture
	CREATE UNIQUE INDEX IX_Temp_TransferDeparture ON #TransferDeparture ([LocationCode], [PartsNumber])


	/****************
	�����d��
	****************/
	--�d�������ꎞ�e�[�u���ɑ}��
	INSERT INTO #PartsPurchase
	SELECT
		pp.[LocationCode]
	,	pp.[PartsNumber]
	,	SUM(CASE pp.[PurchaseType] WHEN '001' THEN ISNULL(pp.[Quantity], 0) ELSE (ISNULL(pp.[Quantity], 0)* -1) END) AS [Quantity]
	,	0 AS [ProvisionQuantity]																										--�����ϐ�(0�Œ�)		--Add 2016/06/09 arc yano #3571
	FROM [PartsPurchase] pp
	WHERE pp.[PurchaseDate] >= @TargetDateFrom
	AND pp.[PurchaseDate] < @TargetDateTo
	--AND pp.[DepartmentCode] = @DepartmentCode																		--Mod 2016/08/13 arc yano #3596
	AND EXISTS
	(
		SELECT 'X' FROM #DepartmentListUseWarehouse dw WHERE pp.DepartmentCode = dw.DepartmentCode 
	)
	AND ISNULL(pp.DelFlag, '0') <> '1'
	AND pp.PurchaseStatus = '002'
	GROUP BY
		pp.[LocationCode]
	,	pp.[PartsNumber]
	--�C���f�b�N�X�Đ���
	DROP INDEX IX_Temp_PartsPurchase ON #PartsPurchase
	CREATE UNIQUE INDEX IX_Temp_PartsPurchase ON #PartsPurchase ([LocationCode], [PartsNumber])

	/****************
	�����������̎Z�o 
	*****************/
	--�ړ������ꎞ�e�[�u���ɑ}��
	INSERT INTO #TransferProvision
	SELECT
		tr.[ArrivalLocationCode]
	,	tr.[PartsNumber]
	,	0 AS [Quantity]																	--����
	,	SUM(CASE WHEN tr.TransferType = '006' THEN ISNULL(tr.[Quantity], 0) ELSE ISNULL(tr.[Quantity], 0) * (-1) END) AS [ProvisionQuantity]		--�����ϐ�(�ړ���ʂ��u���������v�̏ꍇ�̓}�C�i�X�̐�����)
	FROM [Transfer] tr
	INNER JOIN Parts p																								--���i�}�X�^
		ON p.[PartsNumber] = tr.[PartsNumber] AND (ISNULL(p.[DelFlag], '0') <> '1')
	INNER JOIN [Location] l																							--���۹����
		ON l.[LocationCode] = tr.[ArrivalLocationCode]
	INNER JOIN [Location] l2																						--�o��۹����
		ON l2.[LocationCode] = tr.[DepartureLocationCode]
	WHERE tr.[ArrivalDate] >= @TargetDateFrom
	AND tr.[ArrivalDate] < @TargetDateTo
	--AND l.[DepartmentCode] = @DepartmentCode
	AND l.[WarehouseCode] = @WarehouseCode																			--Mod 2016/08/13 arc yano #3596

	AND ISNULL(tr.[DelFlag], '0') <> '1'
	AND (TransferType = '006' OR TransferType = '008')																--�ړ����=�u��������or���������v

	--AND l.DepartmentCode <> l2.DepartmentCode
	GROUP BY
		tr.[ArrivalLocationCode]
	,	tr.[PartsNumber]
	--�C���f�b�N�X�Đ���
	DROP INDEX IX_Temp_TransferProvision ON #TransferProvision
	CREATE UNIQUE INDEX IX_Temp_TransferProvision ON #TransferProvision ([LocationCode], [PartsNumber])


	/****************
	�������I����
	****************/
	--���I������d���A�ړ�(����A���o��)�̍݌ɕϓ����̒�����̎��I�����i�[
	IF @CalcMode = 0			--�݌ɑ����������Z����ꍇ
	BEGIN
		INSERT INTO #AdjustmentStock
		SELECT
			iv.[LocationCode]
		,	iv.[PartsNumber]
		,   ISNULL(iv.[PhysicalQuantity], 0) AS [Quantity]																													  --����
		,	(ISNULL(iv.[PhysicalQuantity], 0) - (ISNULL(pp.[Quantity], 0) +  ISNULL(ta.Quantity, 0) - ISNULL(ss.[Quantity], 0) - ISNULL(td.Quantity, 0)))  AS [CalcQuantity]  --����(���I����݌ɑ����������Z)
		,	(ISNULL(iv.[ProvisionQuantity], 0) - (ISNULL(pp.[ProvisionQuantity], 0) +  ISNULL(ta.[ProvisionQuantity], 0) - ISNULL(ss.[ProvisionQuantity], 0) - ISNULL(td.[ProvisionQuantity], 0)) - ISNULL(tp.[ProvisionQuantity], 0))  AS [ProvisionQuantity]  --�����ϐ�	--Add 2016/06/09 arc yano #3571
		FROM 
			#InventoryStock iv
		LEFT JOIN #PartsPurchase pp																--�d��
			ON (iv.[LocationCode] = pp.[LocationCode] AND iv.[PartsNumber] = pp.[PartsNumber])
		LEFT JOIN #TransferArrival ta															--�ړ����
			ON (iv.[LocationCode] = ta.[LocationCode] AND iv.[PartsNumber] = ta.[PartsNumber])
		LEFT JOIN #TransferDeparture td															--�ړ����o
			ON (iv.[LocationCode] = td.[LocationCode] AND iv.[PartsNumber] = td.[PartsNumber])
		LEFT JOIN #ServiceSales ss																--�̔�
			ON (iv.[LocationCode] = ss.[LocationCode] AND iv.[PartsNumber] = ss.[PartsNumber])
		LEFT JOIN #TransferProvision tp															--�����ϒ���		--Add 2016/06/09 arc yano #3571
			ON (iv.[LocationCode] = tp.[LocationCode] AND iv.[PartsNumber] = tp.[PartsNumber])
	END
	ELSE
	BEGIN
		INSERT INTO #AdjustmentStock
			SELECT
				iv.[LocationCode]
			,	iv.[PartsNumber]
			,   ISNULL(iv.[PhysicalQuantity], 0) AS [Quantity]																													 --����
			,	(ISNULL(iv.[PhysicalQuantity], 0) + (ISNULL(pp.[Quantity], 0) + ISNULL(ta.Quantity, 0) - ISNULL(ss.[Quantity], 0) - ISNULL(td.Quantity, 0)))  AS [CalcQuantity]  --����(���I����݌ɑ����������Z)
			,	(ISNULL(iv.[ProvisionQuantity], 0) + (ISNULL(pp.[ProvisionQuantity], 0) + ISNULL(ta.[ProvisionQuantity], 0) - ISNULL(ss.[ProvisionQuantity], 0) - ISNULL(td.[ProvisionQuantity], 0)) + ISNULL(tp.[ProvisionQuantity], 0))  AS [ProvisionQuantity]  --�����ϐ�	--�����ϐ�(���I����݌ɑ����������Z)	--Add 2016/06/09 arc yano #3571
			FROM 
				#InventoryStock iv
			LEFT JOIN #PartsPurchase pp																--�d��
				ON (iv.[LocationCode] = pp.[LocationCode] AND iv.[PartsNumber] = pp.[PartsNumber])
			LEFT JOIN #TransferArrival ta															--�ړ����
				ON (iv.[LocationCode] = ta.[LocationCode] AND iv.[PartsNumber] = ta.[PartsNumber])
			LEFT JOIN #TransferDeparture td															--�ړ����o
				ON (iv.[LocationCode] = td.[LocationCode] AND iv.[PartsNumber] = td.[PartsNumber])
			LEFT JOIN #ServiceSales ss																--�̔�
				ON (iv.[LocationCode] = ss.[LocationCode] AND iv.[PartsNumber] = ss.[PartsNumber])
			LEFT JOIN #TransferProvision tp															--�����ϒ���		--Add 2016/06/09 arc yano #3571
			ON (iv.[LocationCode] = tp.[LocationCode] AND iv.[PartsNumber] = tp.[PartsNumber])

	END
	--�C���f�b�N�X�Đ���
	DROP INDEX IX_Temp_AdjustmentStock ON #AdjustmentStock
	CREATE UNIQUE INDEX IX_Temp_AdjustmentStock ON #AdjustmentStock ([LocationCode], [PartsNumber])


	--//////////////////////////////////////////////////////////////////////////////////////
	--PartsStock�쐬�E�X�V
	--�v�������݌ɐ��ʂƗ��_�݌�(�I���J�n��)�̍�����PartsStock�ɔ��f������B
	--/////////////////////////////////////////////////////////////////////////////////////
	
	--Add 2017/02/08 arc yano #3620
	--PartsStock��DelFlag = '1'�̃��R�[�h��Quantity��ProvisionQuantity��������(0�ŏ㏑��)����
	UPDATE
		dbo.PartsStock
	SET
		  Quantity = 0
		, ProvisionQuantity = 0
		, LastUpdateEmployeeCode = 'sys'
		, LastUpdateDate = @Now
	WHERE
		ISNULL(DelFlag, '0') = '1'


	--�X�V(UPDATE)
	UPDATE
		dbo.PartsStock
	SET
		  --���I(�␳�O)�Ɨ��_�݌ɐ�(�I���J�n��)�̍����𔽉f����
		  Quantity = ISNULL(ps.Quantity, 0) + (ivs.PhysicalQuantity - ISNULL(ivs.Quantity, 0))
		, DelFlag = '0'															--Mod 2017/02/08 arc yano #3620
		, LastUpdateEmployeeCode = @EmployeeCode
		, LastUpdateDate = @Now
	FROM
		dbo.PartsStock ps
	INNER JOIN
		#InventoryStock ivs
	ON
		ps.LocationCode = ivs.LocationCode AND
		ps.PartsNumber = ivs.PartsNumber
	
	--dbo.InventoryStock�Ƀ��R�[�h�����݂��AdboPartsStock�Ƀ��R�[�h�����݂��Ȃ��ꍇ�͐V�K�쐬���s���B
	--�V�K�쐬(INSERT)
	INSERT
		dbo.PartsStock
	SELECT
		  ivs.PartsNumber						--���i�ԍ�
		, ivs.LocationCode						--���P�[�V�����R�[�h
		, ivs.PhysicalQuantity					--���I
		, @EmployeeCode							--�쐬��
		, @Now									--�쐬��
		, @EmployeeCode							--�ŏI�X�V��
		, @Now									--�ŏI�X�V��
		, '0'									--�폜�t���O
		, ISNULL(ivs.ProvisionQuantity, 0)		--�����ϐ�		--Mod 2016/06/09 arc yano #3571 --Mod 2016/02/08 arc yano #3409 
	FROM
		#InventoryStock ivs
	WHERE
		NOT EXISTS 
		(
			SELECT 'X' FROM PartsStock eps WHERE eps.PartsNumber = ivs.PartsNumber AND eps.LocationCode = ivs.LocationCode 
		)

	--Add 2016/06/09 arc yano #3571
	--//////////////////////////////////////////////////////////////////////////////////////
	--PartsStock�E�d�|���P�[�V�����̈����ϐ��̍X�V
	--/////////////////////////////////////////////////////////////////////////////////////
	UPDATE
		dbo.PartsStock
	SET
		ProvisionQuantity = Quantity				--�����ϐ����݌ɐ��ʂɍ��킹��
	WHERE
		LocationCode = @ShikakariLocationCode

	--///////////////////////////////////////////////////////////////////////
	--InventoryStock�̍X�V
	--����1���`�I���m�莞�܂łɔ����������(�d���^�̔��^�ړ�)
	--�ɂ��݌ɕϓ�����߂�
	--///////////////////////////////////////////////////////////////////
	
	--///////////////////////////////////////////////////////////////////
	--�Ō�Ɉꎞ�e�[�u���̒�����̐��ʂ�InventoryStock�̎��I�����X�V����
	--///////////////////////////////////////////////////////////////////
	
	UPDATE
		dbo.InventoryStock 
	SET
		  PhysicalQuantity = ads.CalcQuantity
		, ProvisionQuantity = ads.ProvisionQuantity			--Add 2016/06/09 arc yano #3571
		, LastUpdateEmployeeCode = @EmployeeCode
		, LastUpdateDate = @Now
	FROM
		dbo.InventoryStock ivs
	INNER JOIN
		#AdjustmentStock ads
	ON
		ivs.LocationCode = ads.LocationCode AND
		ivs.PartsNumber = ads.PartsNumber
	WHERE
		--ivs.DepartmentCode = @DepartmentCode AND			--Mod 2016/08/13 arc yano #3596
		ivs.WarehouseCode = @WarehouseCode AND
		ivs.InventoryMonth = @InventoryMonth 
		--ads.CalcQuantity <> ads.Quantity

	--�����ꎞ�\�̐錾
	/*************************************************************************/
	BEGIN TRY
		DROP TABLE #InventoryStock
		DROP TABLE #PartsPurchase
		DROP TABLE #TransferArrival
		DROP TABLE #ServiceSales
		DROP TABLE #TransferDeparture
		DROP TABLE #AdjustmentStock
		DROP TABLE #InProcess
		DROP TABLE #TransferProvision
		DROP TABLE #DepartmentListUseWarehouse			--Add 2016/08/13 arc yano #3596
	END TRY
	BEGIN CATCH
		--����
	END CATCH
END


GO


