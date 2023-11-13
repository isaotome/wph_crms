USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[Insert_PartsBalance]    Script Date: 2019/02/25 13:20:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- ===================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Update date:
-- 2019/02/09 :  yano #3969 ���i�݌Ɋm�F��ʁ@�����[�Ԃ̐��ʂ��s��
-- 2018/05/14 :  arc yano  #3880 ���㌴���v�Z�y�ђI���]���@�̕ύX
-- 2016/08/13 :  arc yano  #3596 �y�區�ځz�q�ɒI�����Ή� �I���̊Ǘ���q�ɒP�ʂ���q�ɒP�ʂɕύX
-- 2016/02/03 :  arc yano  #3402 ���i�݌Ɋm�F�@�����݌ɂ̎Z�o���@�̕ύX �����݌ɂ̎Z�o�����������P�[�V�����̍݌ɐ��������ϐ��ɕύX
-- 2015/09/16 :  arc yano  IPO�Ή�(���i�I��) ��Q�Ή��@�d�|�݌ɂ̋��z�̕s��v(���i�݌Ɋm�F�̕��i�d�|�݌Ɉꗗ)
-- 2015/07/17 :  arc yano  IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�F �����݌ɗ��ǉ�
-- 2015/06/18 :  arc yano  IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�E �쐬�ҁA�ŏI�X�V�҂Ɂusys�v��ݒ肷��
-- 2015/06/02 :  arc yano  IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�B ���_�݌ɒǉ�
--
-- Description:	�ړ����ϒP���v�Z���󕥕\�쐬����
-- ====================================================================================================================================
CREATE PROCEDURE [dbo].[Insert_PartsBalance]
	@p_CompanyCode nvarchar(3) = '001'		--��ЃR�[�h
,	@p_isShowAllZero bit = 0				--�P���������S�Ă̋��z��0�̕��i���o�^���邩�ǂ���[0:���Ȃ��A1:����]
AS
BEGIN
	SET NOCOUNT ON;

	--�萔----------------------
	DECLARE @RoundLength int = 0			--�ۂߗL������
	DECLARE @RoundMode int = 0				--0:�l�̌ܓ�, 1:�؂�̂�
	----------------------------



	--�����ꎞ�\�̐錾
	/*************************************************************************/
	BEGIN TRY
		DROP TABLE #Previous
		DROP TABLE #PreviousAll
		DROP TABLE #PartsPurchase
		DROP TABLE #PartsPurchaseAll
		DROP TABLE #TransferArrival
		DROP TABLE #ServiceSales
		DROP TABLE #TransferDeparture
		DROP TABLE #InventoryStock
		DROP TABLE #PartsList
		DROP TABLE #PartsListAll
		DROP TABLE #QuantityCalc
		DROP TABLE #QuantityPost
		DROP TABLE #QuantityDiff
		DROP TABLE #AverageCost
		DROP TABLE #DiffCost
		DROP TABLE #Reservation		--Add 2015/07/17 arc yano 
		DROP TABLE #InProcess
		DROP TABLE #PartsBalance
	END TRY
	BEGIN CATCH
		--����
	END CATCH

	--�O�c���X�g(�q�Ɂ^���i��)
	CREATE TABLE #Previous (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Cost] DECIMAL(10, 0)
	,	[Quantity] DECIMAL(10, 3)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_Previous ON #Previous ([WarehouseCode], [PartsNumber])

	--�O�c���X�g(�S�Ѝ��v�^���i��)
	CREATE TABLE #PreviousAll (
		[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Cost] DECIMAL(10, 0)
	,	[Quantity] DECIMAL(10, 3)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_PreviousAll ON #PreviousAll ([PartsNumber])

	--�d�����X�g(�q�Ɂ^���i��)
	CREATE TABLE #PartsPurchase (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_PartsPurchase ON #PartsPurchase ([WarehouseCode], [PartsNumber])

	--�d�����X�g(�S�Ѝ��v�^���i��)
	CREATE TABLE #PartsPurchaseAll (
		[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_PartsPurchaseAll ON #PartsPurchaseAll ([PartsNumber])

	--�ړ�������X�g(�q�Ɂ^���i��)
	CREATE TABLE #TransferArrival (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_TransferArrival ON #TransferArrival ([WarehouseCode], [PartsNumber])


	--�[�ԃ��X�g(�q�Ɂ^���i��)
	CREATE TABLE #ServiceSales (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_ServiceSales ON #ServiceSales ([WarehouseCode], [PartsNumber])

	--�ړ����o���X�g(�q�Ɂ^���i��)
	CREATE TABLE #TransferDeparture (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_TransferDeparture ON #TransferDeparture ([WarehouseCode], [PartsNumber])

	--�I�����X�g(�q�Ɂ^���i��)
	CREATE TABLE #InventoryStock (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_InventoryStock ON #InventoryStock ([WarehouseCode], [PartsNumber])

	--�q�Ɂ^���i���X�g
	CREATE TABLE #PartsList (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[WarehouseName] NVARCHAR(20)
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[PartsNameJp] NVARCHAR(50)
	,	[Cost] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_PartsList ON #PartsList ([WarehouseCode], [PartsNumber])

	--���i���X�g
	CREATE TABLE #PartsListAll (
		[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Cost] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_PartsListAll ON #PartsListAll ([PartsNumber])

	--���_�݌�(�q�Ɂ^���i��)
	CREATE TABLE #QuantityCalc (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	)
	CREATE UNIQUE INDEX IX_Temp_QuantityCalc ON #QuantityCalc ([WarehouseCode], [PartsNumber])

	--�����݌�(�q�Ɂ^���i��)
	CREATE TABLE #QuantityPost (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	)
	CREATE UNIQUE INDEX IX_Temp_QuantityPost ON #QuantityPost ([WarehouseCode], [PartsNumber])

	--�I��(�q�Ɂ^���i��)
	CREATE TABLE #QuantityDiff (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_QuantityDiff ON #QuantityDiff ([WarehouseCode], [PartsNumber])

	--���ϒP��(�S�Ѝ��v�^���i��)
	CREATE TABLE #AverageCost (
		[PartsNumber] NVARCHAR(50) NOT NULL
	,	[AverageCost] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_AverageCost ON #AverageCost ([PartsNumber])

	--�P�����z(�q�Ɂ^���i��)
	CREATE TABLE #DiffCost (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[DiffCost] DECIMAL(18, 0)
	)
	CREATE UNIQUE INDEX IX_DiffCost ON #DiffCost ([WarehouseCode], [PartsNumber])

	--Add 2015/07/17 arc yano
	--����(�q�Ɂ^���i��)
	CREATE TABLE #Reservation (
		[WarehouseCode] NVARCHAR(6) NOT NULL	--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_Reservation ON #Reservation ([WarehouseCode], [PartsNumber])

	--Add 2015/09/16 arc yano
	--�T�[�r�X�`�[�w�b�_(�ΏۊO�f�[�^)
	CREATE TABLE #Temp_ServiceSalesHeader_Exempt (
		[SlipNumber] NVARCHAR(50) NOT NULL				-- �`�[�ԍ�
	)

	--�d�|(�q�Ɂ^���i��)
	CREATE TABLE #InProcess (
		[WarehouseCode] NVARCHAR(6) NOT NULL			--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] NVARCHAR(50) NOT NULL
	,	[Quantity] DECIMAL(10, 2)
	,	[Amount] DECIMAL(10, 0)
	)
	CREATE UNIQUE INDEX IX_Temp_InProcess ON #InProcess ([WarehouseCode], [PartsNumber])

	--�󕥕\(�q�Ɂ^���i��)
	CREATE TABLE #PartsBalance (
		[CloseMonth] [datetime] NOT NULL
	,	[DepartmentCode] [nvarchar](3) NOT NULL			--Mod 2016/08/13 arc yano #3596
	,	[DepartmentName] [nvarchar](50)					--Mod 2016/08/13 arc yano #3596
	,	[PartsNumber] [nvarchar](25) NOT NULL
	,	[PartsNameJp] [nvarchar](50)
	,	[PreCost] [decimal](10, 0)
	,	[PreQuantity] [decimal](10, 3)
	,	[PreAmount] [decimal](10, 0)
	,	[PurchaseQuantity] [decimal](10, 3)
	,	[PurchaseAmount] [decimal](10, 0)
	,	[TransferArrivalQuantity] [decimal](10, 3)
	,	[TransferArrivalAmount] [decimal](10, 0)
	,	[ShipQuantity] [decimal](10, 3)
	,	[ShipAmount] [decimal](10, 0)
	,	[TransferDepartureQuantity] [decimal](10, 3)
	,	[TransferDepartureAmount] [decimal](10, 0)
	,	[DifferenceQuantity] [decimal](10, 3)
	,	[DifferenceAmount] [decimal](10, 0)
	,	[UnitPriceDifference] [decimal](10, 0)
	,	[PostCost] [decimal](10, 0)
	,	[PostQuantity] [decimal](10, 3)
	,	[PostAmount] [decimal](10, 0)
	,	[InProcessQuantity] [decimal](10, 3)
	,	[InProcessAmount] [decimal](10, 0)
	,	[PurchaseOrderPrice] [decimal](10, 0)
	,	[CalculatedDate] [datetime]
	,	[CreateEmployeeCode] [nvarchar](50)
	,	[CreateDate] [datetime]
	,	[LastUpdateEmployeeCode] [nvarchar](50)
	,	[LastUpdateDate] [datetime]
	,	[DelFlag] [nvarchar](2)
	,	[QuantityCalc] [decimal](10, 2)					--Add 2015/06/02 arc yano
	,	[AmountCalc] [decimal](10, 0)					--Add 2015/06/02 arc yano
	,	[ReservationQuantity] [decimal](10, 2)			--Add 2015/07/17 arc yano 
	,	[ReservationAmount] [decimal](10, 0)			--Add 2015/07/17 arc yano 
	,	[WarehouseCode] [nvarchar](6) NOT NULL			--Mod 2016/08/13 arc yano #3596
	,	[WarehouseName] [nvarchar](50)					--Mod 2016/08/13 arc yano #3596
	)
	/*************************************************************************/


	--��������
	DECLARE @ErrorMessage NVARCHAR(4000) = ''
    DECLARE @ErrorNumber INT = 0

	--����
	DECLARE @TODAY DATETIME = CONVERT(DATETIME, CONVERT(NVARCHAR(10), GETDATE(), 111), 111)
	--����1��
	DECLARE @THISMONTH DATETIME = CONVERT(DATETIME, CONVERT(NVARCHAR(7), @TODAY, 111) + '/01', 111)

	--�������Ώی��͈͂̐ݒ�
	--���u�󕥒��߁v�����܂��Ă��錎�̒��ōő匎�̗�����1��<=x<���������̗���1������(�܂��́A�����̏ꍇ�͓��������j
	--���������Ώی�From�̐ݒ�i���܂��Ă��錎�̒��ōő匎�̗���1���j
	DECLARE @TargetMonthFrom DATETIME = NULL
	SELECT @TargetMonthFrom = DATEADD(m, 1, ISNULL(MAX(CONVERT(datetime, mc.[InventoryMonth], 120)), @THISMONTH))
	FROM [InventoryMonthControlPartsBalance] mc	--�󕥒�
	WHERE mc.[InventoryStatus] = '002'			--�{����
	
	--ADD 2015/05/19 arc yano �s��Ή�
	--�Ώی������������߂Ď��s�A�܂��͒I������ɏ��߂Ď��s�̏ꍇ�A
	--���{���̃��R�[�h�̂Ȃ��ŁA�ł��Â������擾����B
	--������͎󕥂������̑O�����󕥂����ςł��邱�Ƃ�O��Ƃ��Ă���
	IF @TargetMonthFrom > @TODAY
	BEGIN
		SELECT @TargetMonthFrom = ISNULL(MIN(CONVERT(datetime, mc.[InventoryMonth], 120)), DATEADD(m, 1, @THISMONTH))
		FROM [InventoryMonthControlPartsBalance] mc	--�I����
		WHERE mc.[InventoryStatus] = '001'			--���{��
	END
	
	--�ΏےǋL������=���߂Ď��s�̏ꍇ
	--�I���̒��܂��Ă��錎�̒��ōő匎�̗�����1��<=x<���������̗���1������(�܂��́A�����̏ꍇ�͓��������j
	IF @TargetMonthFrom > @TODAY
	BEGIN
		SELECT @TargetMonthFrom = DATEADD(m, 1, ISNULL(MAX(CONVERT(DATETIME, mc.[InventoryMonth], 120)), @THISMONTH))
		FROM [InventoryMonthControlParts] mc	--�I����
		WHERE mc.[InventoryStatus] = '002'		--�{����
	END
	--����ł��Ώی��������ɂȂ�ꍇ�A�����Ƃ���i���W�b�N�Ƃ��Ă͒ʂ�Ȃ��͂��j
	IF @TargetMonthFrom > @TODAY
		SET @TargetMonthFrom = @THISMONTH

	--���������Ώی�To�̐ݒ�(���������̗���1������)
	DECLARE @TargetMonthTo DATETIME = DATEADD(m, 1, @THISMONTH)

	--�����Ώی����^�����Ώی��O��
	DECLARE @TargetMonthCount INT = DATEDIFF(m, @TargetMonthFrom, @TargetMonthTo)
	DECLARE @TargetMonthPrevious DATETIME = DATEADD(m, -1, @TargetMonthFrom)

	--�����Ώۓ��t�͈�From�^�����Ώۓ��t�͈�To
	DECLARE @TargetDateFrom DATETIME = @TargetMonthFrom
	DECLARE @TargetDateTo DATETIME = DATEADD(m, 1, @TargetDateFrom)
	IF @TargetDateTo > @TODAY		--���t�͈�TO���������̏ꍇ�A�����ɂ���
		SET @TargetDateTo = @TODAY
	--�Z�o��(�����Ώۓ��t�͈�TO�̑O��)
	DECLARE @CalcDate DATETIME = DATEADD(d, -1, @TargetDateTo)

	--�g�����U�N�V�����J�n 
	BEGIN TRANSACTION
	BEGIN TRY

		--�������Ώی��������[�v
		WHILE @TargetMonthCount > 0
		BEGIN

			--�ꎟ�\������		
			DELETE FROM  #Previous			--�O�c���X�g(�q�Ɂ^���i��)
			DELETE FROM  #PreviousAll		--�O�c���X�g(�S�Ѝ��v�^���i��)
			DELETE FROM  #PartsPurchase		--�d�����X�g(�q�Ɂ^���i��)
			DELETE FROM  #PartsPurchaseAll	--�d�����X�g(�S�Ѝ��v�^���i��)
			DELETE FROM  #ServiceSales		--�[�ԃ��X�g(�q�Ɂ^���i��)
			DELETE FROM  #TransferArrival	--�ړ�������X�g(�q�Ɂ^���i��)
			DELETE FROM  #TransferDeparture	--�ړ����o���X�g(�q�Ɂ^���i��)
			DELETE FROM  #InventoryStock	--�I�����X�g(�q�Ɂ^���i��)
			DELETE FROM  #PartsList			--�q�Ɂ^���i���X�g
			DELETE FROM  #PartsListAll		--���i���X�g
			DELETE FROM  #QuantityCalc		--���_�݌�(�q�Ɂ^���i��)
			DELETE FROM  #QuantityPost		--�����݌�(�q�Ɂ^���i��)
			DELETE FROM  #QuantityDiff		--�I��(�q�Ɂ^���i��)
			DELETE FROM  #AverageCost		--���ϒP��(�S�Ѝ��v�^���i��)
			DELETE FROM  #DiffCost			--�P�����z(�q�Ɂ^���i��)
			DELETE FROM  #Reservation		--����(�q�Ɂ^���i��)
			DELETE FROM  #InProcess			--�d�|(�q�Ɂ^���i��)
			DELETE FROM  #PartsBalance		--�󕥕\(�q�Ɂ^���i��)
			-----------------------------

			/****************
			�����O�c���X�g
			****************/
			--�O�����ʁ^�O���P�����ꎞ�e�[�u���ɑ}��
			--step1.�q�ɕ�
			INSERT INTO #Previous
			SELECT 
				pb.[WarehouseCode]										--�q��
			,	pb.[PartsNumber]										--���i�ԍ�
			,	pb.[PostCost] AS [Cost]									--�P��
			,	pb.[PostQuantity] AS [Quantity] 						--����
			,	pb.[PostAmount] AS [Amount]								--���z
			FROM [PartsBalance] pb
			WHERE pb.[CloseMonth] = @TargetMonthPrevious				--�Ώی��̑O��


			--��[PartsBalace]�����݂��Ȃ��ꍇ�i���߂Ă̏ꍇ�j
			IF NOT EXISTS(SELECT 'X' FROM #Previous)
			BEGIN

				INSERT INTO #Previous
				SELECT 
					s.[WarehouseCode]																						--�q��
				,	s.[PartsNumber]																							--���i�ԍ�
				,	ISNULL(pac.[Price], 0) AS [Cost]																		--�P��
				,	SUM(ISNULL(s.[Quantity], 0)) AS [Quantity]																--����
				,	SUM(ROUND(ISNULL(s.[Quantity], 0) * ISNULL(pac.[Price], 0), @RoundLength, @RoundMode)) AS [Amount]		--ROUND(����*�P��)=���z
				FROM [InventoryStock] s																						--���i�I��
				LEFT JOIN [PartsAverageCost] pac																			--���ϒP��
					ON pac.CompanyCode = @p_CompanyCode																		--�Œ�H
					AND pac.[CloseMonth] = s.[InventoryMonth]																--�Ώی��̑O��
					AND pac.[PartsNumber] = s.[PartsNumber]																	--���i�ԍ�
					AND ISNULL(pac.[DelFlag], '0') <> '1'																	--�폜�t���O
				WHERE s.[InventoryMonth] = @TargetMonthPrevious																--�Ώی��̑O��
				AND ISNULL(s.[DelFlag], '0') <> '1'
				GROUP BY
					s.[WarehouseCode]
				,	s.[PartsNumber]
				,	ISNULL(pac.[Price], 0)

			END
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_Previous ON #Previous
			CREATE UNIQUE INDEX IX_Temp_Previous ON #Previous ([WarehouseCode], [PartsNumber])


			--step2.�S��
			INSERT INTO #PreviousAll
			SELECT 
				p.[PartsNumber]											--���i�ԍ�
			,	p.[Cost]												--�P��
			,	SUM(p.[Quantity]) AS [Quantity] 						--����
			,	SUM(p.[Amount]) AS [Amount]								--���z
			FROM #Previous p
			GROUP BY
				p.[PartsNumber]
			,	p.[Cost]
			--�O�c�̂Ȃ����i�P����ǉ�
			INSERT INTO #PreviousAll
			SELECT
				pac.[PartsNumber]
			,	ISNULL(pac.[Price], 0) AS [Cost]
			,	0 AS [Quantity] 										--����
			,	0 AS [Amount]											--���z
			FROM [PartsAverageCost] pac
			WHERE pac.CompanyCode = @p_CompanyCode						--�Œ�H
			AND pac.[CloseMonth] = @TargetMonthPrevious					--�Ώی��̑O��
			AND NOT EXISTS(
					SELECT 'X'
					FROM #PreviousAll pa
					WHERE pa.[PartsNumber] = pac.[PartsNumber]			--���i�ԍ�
					)
			AND ISNULL(pac.[DelFlag], '0') <> '1'


			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_PreviousAll ON #PreviousAll
			CREATE UNIQUE INDEX IX_Temp_PreviousAll ON #PreviousAll ([PartsNumber])

		
			/****************
			���������d��
			****************/
			--�d�������ꎞ�e�[�u���ɑ}��
			--step1.�q�ɕ�
			INSERT INTO #PartsPurchase
			SELECT
				l.[WarehouseCode]									--Mod 2016/08/13 arc yano #3596
			,	pp.[PartsNumber]
			,	SUM(CASE pp.[PurchaseType] WHEN '001' THEN ISNULL(pp.[Quantity], 0) ELSE (ISNULL(pp.[Quantity], 0)* -1) END) AS [Quantity]
			,	SUM(ROUND((CASE pp.[PurchaseType] WHEN '001' THEN ISNULL(pp.[Quantity], 0) ELSE (ISNULL(pp.[Quantity], 0)* -1) END) * ISNULL(pp.[Price], 0), @RoundLength, @RoundMode)) AS [Amount] 		--ROUND(����*�P��)=���z
			FROM [PartsPurchase] pp INNER JOIN
			dbo.Location l ON pp.LocationCode = l.LocationCode		--Mod 2016/08/13 arc yano #3596
			WHERE pp.[PurchaseDate] >= @TargetDateFrom
			AND pp.[PurchaseDate] < @TargetDateTo
			AND ISNULL(pp.DelFlag, '0') <> '1'
			AND pp.PurchaseStatus = '002'
			AND l.DelFlag = '0'										--Add 2016/08/13 arc yano #3596
			
			GROUP BY
				l.[WarehouseCode]
			,	pp.[PartsNumber]
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_PartsPurchase ON #PartsPurchase
			CREATE UNIQUE INDEX IX_Temp_PartsPurchase ON #PartsPurchase ([WarehouseCode], [PartsNumber])

			--step2.�S��
			INSERT INTO #PartsPurchaseAll
			SELECT
				pp.[PartsNumber]
			,	SUM(pp.[Quantity]) AS [Quantity]
			,	SUM(pp.[Amount]) AS [Amount]
			FROM #PartsPurchase pp
			GROUP BY
				pp.[PartsNumber]
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_PartsPurchaseAll ON #PartsPurchaseAll
			CREATE UNIQUE INDEX IX_Temp_PartsPurchaseAll ON #PartsPurchaseAll ([PartsNumber])

			/****************
			���������ړ����
			****************/
			--�ړ������ꎞ�e�[�u���ɑ}��
			INSERT INTO #TransferArrival
			SELECT
				l.[WarehouseCode]								--Mod 2016/08/13 arc yano #3596
			,	tr.[PartsNumber]
			,	SUM(ISNULL(tr.[Quantity], 0)) AS [Quantity]
			,	SUM(ROUND(ISNULL(tr.[Quantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]	--ROUND(����*�P��)=���z
			FROM [Transfer] tr
			INNER JOIN Parts p							--���i�}�X�^
				ON p.[PartsNumber] = tr.[PartsNumber]
			INNER JOIN [Location] l				--���۹����
				ON l.[LocationCode] = tr.[ArrivalLocationCode]
			INNER JOIN [Location] l2				--�o��۹����
				ON l2.[LocationCode] = tr.[DepartureLocationCode]
			LEFT JOIN #PreviousAll pa					--�O�c���
				ON pa.[PartsNumber] = tr.[PartsNumber]
			WHERE tr.[ArrivalDate] >= @TargetDateFrom
			AND tr.[ArrivalDate] < @TargetDateTo
			AND ISNULL(tr.[DelFlag], '0') <> '1'
			AND l.WarehouseCode <> l2.WarehouseCode
			GROUP BY
				l.[WarehouseCode]								--Mod 2016/08/13 arc yano #3596
			,	tr.[PartsNumber]
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_TransferArrival ON #TransferArrival
			CREATE UNIQUE INDEX IX_Temp_TransferArrival ON #TransferArrival ([WarehouseCode], [PartsNumber])

			/****************
			���������[��
			****************/
			--�[�ԏ����ꎞ�e�[�u���ɑ}��
			INSERT INTO #ServiceSales
			SELECT
				dw.[WarehouseCode]												--Mod 2016/08/13 arc yano #3596
			,	sl.[PartsNumber]
			,	SUM(ISNULL(sl.[ProvisionQuantity], 0)) AS [Quantity]			--Mod 2019/02/09 yano #3969
			--,	SUM(ISNULL(sl.[Quantity], 0)) AS [Quantity]
			,	SUM(ROUND(ISNULL(sl.[ProvisionQuantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]		--ROUND(����*�P��)=���z	--Mod 2019/02/09 yano #3954
			--,	SUM(ROUND(ISNULL(sl.[Quantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]
			FROM [ServiceSalesHeader] sh										--�T�[�r�X�I�[�_�[�w�b�_
			INNER JOIN [ServiceSalesLine] sl									--�T�[�r�X�I�[�_�[����
				ON sl.[SlipNumber] = sh.[SlipNumber] 
				AND ISNULL(sl.[DelFlag], '0') <> '1'
			INNER JOIN Parts p													--���i�}�X�^
				ON p.[PartsNumber] = sl.[PartsNumber]
			INNER JOIN DepartmentWarehouse dw									--����E�q�ɑg�����}�X�^		--Mod 2016/08/13 arc yano #3596
				ON sh.[DepartmentCode] = dw.[DepartmentCode]
			LEFT JOIN #PreviousAll pa											--�O�c���
				ON pa.[PartsNumber] = sl.[PartsNumber]
			WHERE sh.[SalesDate] >= @TargetDateFrom								--����1���ȏ�
			AND sh.[SalesDate] < @TargetDateTo									--����������
			AND sh.[ServiceOrderStatus] = '006'									--�[�ԍ�
			AND ISNULL(sh.DelFlag, '0') <> '1'
			AND ISNULL(dw.DelFlag, '0') <> '1'
			GROUP BY
				dw.[WarehouseCode]												--Mod 2016/08/13 arc yano #3596
			,	sl.[PartsNumber]
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_ServiceSales ON #ServiceSales
			CREATE UNIQUE INDEX IX_Temp_ServiceSales ON #ServiceSales ([WarehouseCode], [PartsNumber])

			/****************
			���������ړ����o
			****************/
			--�ړ������ꎞ�e�[�u���ɑ}��
			INSERT INTO #TransferDeparture
			SELECT
				l.[WarehouseCode]
			,	tr.[PartsNumber]
			,	SUM(ISNULL(tr.[Quantity], 0)) AS [Quantity]
			,	SUM(ROUND(ISNULL(tr.[Quantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]--ROUND(����*�P��)=���z
			FROM [Transfer] tr
			INNER JOIN Parts p																		--���i�}�X�^
				ON p.[PartsNumber] = tr.[PartsNumber]
			INNER JOIN [Location] l																	--�o��۹����
				ON l.[LocationCode] = tr.[DepartureLocationCode]
			INNER JOIN [Location] l2																--���۹����
				ON l2.[LocationCode] = tr.[ArrivalLocationCode]
			LEFT JOIN #PreviousAll pa																--�O�c���
				ON pa.[PartsNumber] = tr.[PartsNumber]
			WHERE tr.[DepartureDate] >= @TargetDateFrom
			AND tr.[DepartureDate] < @TargetDateTo
			AND ISNULL(tr.[DelFlag], '0') <> '1'
			AND l.WarehouseCode <> l2.WarehouseCode
			GROUP BY
				l.[WarehouseCode]
			,	tr.[PartsNumber]
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_TransferDeparture ON #TransferDeparture
			CREATE UNIQUE INDEX IX_Temp_TransferDeparture ON #TransferDeparture ([WarehouseCode], [PartsNumber])


			/****************
			���������I��
			****************/
			--�I�������X�e�[�^�X�i���ߏ󋵁j
			DECLARE @isExistsInventoryStock bit = 0			--0�F���A1:��
			DECLARE @CloseDateTime DATETIME = NULL
			--���������܂��Ă��邩�ǂ����𔻒�
			SELECT 
				@isExistsInventoryStock = CASE mc.[InventoryStatus] WHEN '002' THEN 1 ELSE 0 END				--002:�I������
			,	@CloseDateTime = CASE mc.[InventoryStatus] WHEN '002' THEN mc.LastUpdateDate ELSE @CalcDate END	--�I���m�����(�b��)
			FROM [InventoryMonthControlParts] mc
			WHERE mc.InventoryMonth = @TargetDateFrom
			--���܂��Ă���ꍇ�̂݃f�[�^�擾
			IF @isExistsInventoryStock = 1
			BEGIN
				--�I�������ꎞ�e�[�u���ɑ}��
				INSERT INTO #InventoryStock
				SELECT
					s.[WarehouseCode]																			--Mod 2016/08/13 arc yano #3596
				,	s.[PartsNumber]
				,	SUM(ISNULL(s.[PhysicalQuantity], 0)) AS [Quantity]
				,	SUM(ROUND(ISNULL(s.[PhysicalQuantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]--ROUND(����*�P��)=���z
				FROM [InventoryStock] s
				INNER JOIN Parts p							--���i�}�X�^
					ON p.[PartsNumber] = s.[PartsNumber]
				LEFT JOIN #PreviousAll pa					--�O�c���
					ON pa.[PartsNumber] = s.[PartsNumber]
				WHERE s.InventoryMonth = @TargetDateFrom
				AND ISNULL(s.[DelFlag], '0') <> '1'
				GROUP BY
					s.[WarehouseCode]																			--Mod 2016/08/13 arc yano #3596
				,	s.[PartsNumber]
				--�C���f�b�N�X�Đ���
				DROP INDEX IX_Temp_InventoryStock ON #InventoryStock
				CREATE UNIQUE INDEX IX_Temp_InventoryStock ON #InventoryStock ([WarehouseCode], [PartsNumber])
			END

			
			--Add 2015/07/17
			/****************
			��������
			****************/
			--�����͒I�������܂ł�PartsStock����A�I���������InventoryStock����Z�o����
			--���d�|�ƈقȂ�A�P���ɃT�[�r�X�`�[�̏��ł͌v�Z�ł��Ȃ�
			IF @isExistsInventoryStock = 1											--�I������
			BEGIN
				INSERT INTO #Reservation
				SELECT
					ivs.[WarehouseCode]																			--Mod 2016/08/13 arc yano #3596
				,	ivs.[PartsNumber]
				,	SUM(ISNULL(ivs.[ProvisionQuantity], 0)) AS [Quantity]		--Mod 2016/02/03
				,	SUM(ROUND(ISNULL(ivs.[ProvisionQuantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]	--ROUND(����*�P��)=���z	--Mod 2016/02/03
				FROM [InventoryStock] ivs						--���i�I���e�[�u��
				INNER JOIN Parts p								--���i�}�X�^
					ON p.[PartsNumber] = ivs.[PartsNumber]
				LEFT JOIN #PreviousAll pa						--�O�c���
					ON pa.[PartsNumber] = ivs.[PartsNumber]
				WHERE
					ivs.InventoryMonth = @TargetDateFrom
					AND exists
					(
						SELECT 'X' FROM dbo.Location l where l.LocationType <> '003' AND ISNULL(l.DelFlag, '0') <> '1' AND l.LocationCode = ivs.LocationCode
					)
					AND ISNULL(ivs.DelFlag, '0') <> '1'
					AND ISNULL(p.DelFlag, '0') <> '1'
					AND ISNULL(p.NonInventoryFlag, '0') <> '1'
				GROUP BY
					ivs.[WarehouseCode]																			--Mod 2016/08/13 arc yano #3596
				,	ivs.[PartsNumber]
			END
			ELSE	--���m�莞��PartsStock����Z�o
			BEGIN
				INSERT INTO #Reservation
				SELECT
					l.[WarehouseCode]																			--Mod 2016/08/13 arc yano #3596
				,	ps.[PartsNumber]
				,	SUM(ISNULL(ps.[ProvisionQuantity], 0)) AS [Quantity] --Mod 2016/02/03
				,	SUM(ROUND(ISNULL(ps.[ProvisionQuantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]	--ROUND(����*�P��)=���z
				FROM [PartsStock] ps							--���i�I���e�[�u��
				INNER JOIN Parts p								--���i�}�X�^
					ON p.[PartsNumber] = ps.[PartsNumber]
				INNER JOIN Location l							--���P�[�V�����}�X�^
					ON l.[LocationCode] = ps.[LocationCode]
				LEFT JOIN #PreviousAll pa						--�O�c���
					ON pa.[PartsNumber] = ps.[PartsNumber]
				WHERE
					l.LocationType <> '003'						--���P�[�V�����^�C�v���u�d�|�v
					AND ISNULL(ps.DelFlag, '0') <> '1'
					AND ISNULL(p.DelFlag, '0') <> '1'
					AND ISNULL(p.NonInventoryFlag, '0') <> '1'
					AND ISNULL(l.DelFlag, '0') <> '1'
				GROUP BY
					l.[WarehouseCode]																			--Mod 2016/08/13 arc yano #3596
				,	ps.[PartsNumber]
			END
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_Reservation ON #Reservation
			CREATE UNIQUE INDEX IX_Temp_Reservation ON #Reservation ([WarehouseCode], [PartsNumber])			--Mod 2016/08/13 arc yano #3596

			/****************
			�����d�|
			****************/
			--Add 2015/09/16 arc yano
			--�T�[�r�X�`�[�ΏۊO�f�[�^���擾
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
			
			--------------------------------------------------------------
			--�T�[�r�X�`�[�ΏۊO�f�[�^�̎擾(�Ώی��ɔ[�ԍ�)
			--------------------------------------------------------------
			INSERT INTO #Temp_ServiceSalesHeader_Exempt
			SELECT
				sh.[SlipNumber]												--�`�[�ԍ�
			FROM
				dbo.ServiceSalesHeader sh
			WHERE 
				sh.[ServiceOrderStatus] = '006'								--006:�[�ԍ�
			AND ISNULL(sh.[SalesDate], @TargetDateTo) < @TargetDateTo		--�������ɔ[�ԍ�
			AND sh.[DelFlag] = '0'
			
			--�d�|���ꎞ�e�[�u���ɑ}��
			INSERT INTO #InProcess
			SELECT
				dw.[WarehouseCode]											--Mod 2016/08/13 arc yano #3596											
			,	sl.[PartsNumber]
			,	SUM(ISNULL(sl.[ProvisionQuantity], 0)) AS [Quantity]		--Mod 2016/02/03
			,	SUM(ROUND(ISNULL(sl.[ProvisionQuantity], 0) * COALESCE(pa.[Cost], p.[SoPrice] ,p.[Cost], 0), @RoundLength, @RoundMode)) AS [Amount]--ROUND(����*�P��)=���z		--Mod 2016/02/03
			FROM [ServiceSalesHeader] sh									--�T�[�r�X�I�[�_�[�w�b�_
			INNER JOIN [ServiceSalesLine] sl								--�T�[�r�X�I�[�_�[����
				ON sl.[SlipNumber] = sh.[SlipNumber] 
				AND ISNULL(sl.[DelFlag], '0') <> '1'
			INNER JOIN [DepartmentWarehouse] dw								--����E�q�ɑg�����}�X�^	--Add 2016/08/13 arc yano #3596
				ON sh.[DepartmentCode] = dw.[DepartmentCode] 
			INNER JOIN Parts p												--���i�}�X�^
				ON p.[PartsNumber] = sl.[PartsNumber]
			LEFT JOIN #PreviousAll pa										--�O�c���
				ON pa.[PartsNumber] = sl.[PartsNumber]
			WHERE sh.[WorkingStartDate] < @TargetDateTo	--�ΏۏI�����O�ɍ�ƊJ�n���Ă���`�[
			--�����ȑO�ɃL�����Z��/��ƒ��~�̂Ȃ��`�[
			AND NOT EXISTS(
				SELECT 'X'
				FROM #Temp_ServiceSalesHeader_Exempt sub
				WHERE sub.[SlipNumber] = sh.[SlipNumber]
				)
			AND ISNULL(sh.DelFlag, '0') <> '1'
			AND ISNULL(dw.DelFlag, '0') <> '1'
			GROUP BY
				dw.[WarehouseCode]											--Mod 2016/08/13 arc yano #3596											
			,	sl.[PartsNumber]
		
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_InProcess ON #InProcess
			CREATE UNIQUE INDEX IX_Temp_InProcess ON #InProcess ([WarehouseCode], [PartsNumber])

			----------------------------------------------------------------------------------------------------------------
			-- 2015/09/16 arc yano ���i�݌Ɋm�F�s�(�݌ɂ̕ϓ����A�d�|�A�����݂̂̏ꍇ�A�݌ɂ̌v�Z�ΏۂɊ܂܂�Ȃ�)�̑Ή�
			----------------------------------------------------------------------------------------------------------------
			/****************
			�������i���X�g
			****************/
			--���i���X�g
			INSERT INTO #PartsList
			SELECT 
				w.[WarehouseCode]										--�q�ɃR�[�h
			,	w.[WarehouseName]										--�q�ɖ�
			,	p.[PartsNumber]											--���i�ԍ�
			,	p.[PartsNameJp]											--���i��
			,	COALESCE(p.[SoPrice], p.[Cost], 0) AS [Cost]			--�P��(StockOrderPrice=�ʏ픭���P���˕W��������0)
			FROM (
				--���݂���q�Ɂ^���i�̑g�ݍ��킹���擾
				SELECT [WarehouseCode], [PartsNumber]		
				FROM #Previous											--�O�c
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #PartsPurchase										--�����d��
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #TransferArrival									--�����ړ����
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #ServiceSales										--�����[��
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #TransferDeparture									--�����ړ����o
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #InventoryStock									--�����I��
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #Reservation										--��������	--add 2015/09/16
				UNION
				SELECT [WarehouseCode], [PartsNumber]
				FROM #InProcess											--�����d�|	--add 2015/09/16
				) dp
			INNER JOIN [Warehouse] w									--�q��
				ON w.[WarehouseCode] = dp.[WarehouseCode]
			INNER JOIN [Parts] p										--���i
				ON p.[PartsNumber] = dp.[PartsNumber] 
				AND ISNULL(p.[DelFlag], 0) <> '1'
				AND ISNULL(p.NonInventoryFlag, '0') <> '1'				--�݌ɊǗ��ΏۊO�ł͂Ȃ�
				AND ISNULL(w.[DelFlag], 0) <> '1'

			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_PartsList ON #PartsList
			CREATE UNIQUE INDEX IX_Temp_PartsList ON #PartsList ([WarehouseCode], [PartsNumber])

			/****************
			�������i���X�g(ALL�j
			****************/
			--���i���X�g
			INSERT INTO #PartsListAll
			SELECT
				p.[PartsNumber]											--���i�ԍ�
			,	COALESCE(p.[SoPrice], p.[Cost], 0) AS [Cost]			--�P��
			FROM [Parts] p
			WHERE ISNULL(p.[DelFlag], 0) <> '1'
			AND ISNULL(p.NonInventoryFlag, '0') <> '1'					--�݌ɊǗ��ΏۊO�ł͂Ȃ�

			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_PartsListAll ON #PartsListAll
			CREATE UNIQUE INDEX IX_Temp_PartsListAll ON #PartsListAll ([PartsNumber])

			/****************
			�������_�݌�
			****************/
			--�i�O�����݌Ɂ{�����d���{�����ړ�����|�����[�ԁ|�����ړ����o�j�����_�݌ɏ����ꎞ�e�[�u���ɑ}��
			INSERT INTO #QuantityCalc
			SELECT
				pl.[WarehouseCode]
			,	pl.[PartsNumber]
			,	SUM(ISNULL(p.[Quantity], 0) 
				+	ISNULL(pp.[Quantity], 0)
				+	ISNULL(ta.[Quantity], 0)
				-	ISNULL(ss.[Quantity], 0)
				-	ISNULL(td.[Quantity], 0)
				) AS [Quantity]
			FROM #PartsList pl				--���i���X�g
			LEFT JOIN #Previous p			--�O�c
				ON p.[WarehouseCode] = pl.WarehouseCode AND p.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #PartsPurchase pp		--�d��
				ON pp.[WarehouseCode] = pl.WarehouseCode AND pp.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #TransferArrival ta	--�ړ����
				ON ta.[WarehouseCode] = pl.WarehouseCode AND ta.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #ServiceSales ss		--�[��
				ON ss.[WarehouseCode] = pl.WarehouseCode AND ss.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #TransferDeparture td	--�ړ����o
				ON td.[WarehouseCode] = pl.WarehouseCode AND td.[PartsNumber] = pl.[PartsNumber]
			GROUP BY
				pl.[WarehouseCode]
			,	pl.[PartsNumber]
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_QuantityCalc ON #QuantityCalc
			CREATE UNIQUE INDEX IX_Temp_QuantityCalc ON #QuantityCalc ([WarehouseCode], [PartsNumber])

			/****************
			�������I�݌�
			****************/
			--���܂��Ă���ꍇ�A�I�����ʂ��������Ƃ���
			
			IF @isExistsInventoryStock = 1
			BEGIN
				--�I�������ꎞ�e�[�u���ɑ}��
				INSERT INTO #QuantityPost
				SELECT
					s.[WarehouseCode]
				,	s.[PartsNumber]
				,	s.[Quantity]
				FROM #InventoryStock s				--�I��
			END
			
			--���܂��Ă��Ȃ��ꍇ�A���_���ʂ��������Ƃ���
			--Mod 2015/06/02 arc yano
			--ELSE
			/*
			BEGIN
				--�i�O�����݌Ɂ{�����d���|�����[�ԁj�����_�݌ɏ����ꎞ�e�[�u���ɑ}��
				INSERT INTO #QuantityPost
				SELECT
					s.[WarehouseCode]
				,	s.[PartsNumber]
				,	s.[Quantity]
				FROM #QuantityCalc s				--���_�݌�
			END
			*/
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_QuantityPost ON #QuantityPost
			CREATE UNIQUE INDEX IX_Temp_QuantityPost ON #QuantityPost ([WarehouseCode], [PartsNumber])

			/****************
			�������ϒP��
			****************/

			--�V�P�����(��������)���ꎞ�e�[�u���ɑ}��
			INSERT INTO #AverageCost
			SELECT
				pla.PartsNumber
			,	CASE 
				--�[��0������������ꍇ�A�܂��͎d�����Ȃ��ꍇ�A�O���P���̂܂܂Ƃ��� 
				WHEN ((ISNULL(pa.[Quantity], 0) + ISNULL(pp.Quantity, 0)) = 0) OR (ISNULL(pp.Quantity, 0) = 0) THEN COALESCE(pa.[Cost], pla.[Cost], 0)
				--���ϒP���Z�o
				ELSE ROUND((ISNULL(pa.[Amount], 0) + ISNULL(pp.[Amount], 0)) / (ISNULL(pa.[Quantity], 0) + ISNULL(pp.Quantity, 0)), @RoundLength, @RoundMode)--ROUND(����*�P��)=���z
				END AS [AverageCost]
			FROM #PartsListAll pla
			LEFT JOIN #PreviousAll pa				--�O�c�i�S�Ёj
				ON pa.PartsNumber = pla.PartsNumber
			LEFT JOIN #PartsPurchaseAll pp			--�d���i�S�Ёj
				ON pp.PartsNumber = pla.PartsNumber
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_AverageCost ON #AverageCost
			CREATE UNIQUE INDEX IX_AverageCost ON #AverageCost ([PartsNumber])

			/****************
			�����I��
			****************/
			--Mod 2015/06/02 �I���͒I���m�莞�̂ݕ\��
			IF @isExistsInventoryStock = 1
			BEGIN
				INSERT INTO #QuantityDiff
				SELECT
					pl.[WarehouseCode]
				,	pl.[PartsNumber]
				,	(ISNULL(qp.[Quantity], 0) - ISNULL(qc.[Quantity], 0)) AS [Quantity]	--�I���i���I���|���_��)
				,	ROUND((ISNULL(qp.[Quantity], 0) - ISNULL(qc.[Quantity], 0)) * COALESCE(ac.[AverageCost], pl.[Cost], 0), @RoundLength, @RoundMode) AS [Amount]	--�I���z--ROUND(����*�P��)=���z
				FROM #PartsList pl
				LEFT JOIN #AverageCost ac				--Mod 2015/06/01 arc yano �I���̒P���͌����P���ł͂Ȃ������P���Ōv�Z 
					ON ac.[PartsNumber] = pl.[PartsNumber]
				LEFT JOIN #QuantityCalc qc			--�_����
					ON qc.WarehouseCode = pl.WarehouseCode AND qc.PartsNumber = pl.PartsNumber
				LEFT JOIN #QuantityPost qp			--���I��
					ON qp.WarehouseCode = pl.WarehouseCode AND qp.PartsNumber = pl.PartsNumber
			END
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_Temp_QuantityDiff ON #QuantityDiff
			CREATE UNIQUE INDEX IX_Temp_QuantityDiff ON #QuantityDiff ([WarehouseCode], [PartsNumber])

			/****************
			�����P�����z
			****************/
			--Mod 2015/06/02 �P�����z�̌v�Z������A�I�����폜�B�܂��匳�̃f�[�^�����I�݌ɂ��痝�_�݌ɂɕύX
			--																 
			--�P�����z���ꎞ�e�[�u���ɑ}��
			INSERT INTO #DiffCost
			SELECT
				pl.[WarehouseCode]
			,	pl.[PartsNumber]
			,	SUM(
				  --(ISNULL(qp.[Quantity], 0) * ISNULL(ac.[AverageCost], 0))
				  (ISNULL(qc.[Quantity], 0) * ISNULL(ac.[AverageCost], 0)) 
				- (ISNULL(p.[Amount], 0)	--�O�c
				 + ISNULL(pp.[Amount], 0)	--�d��
				 + ISNULL(ta.[Amount], 0)	--�ړ����
				 - ISNULL(ss.[Amount], 0)	--�[��
				 - ISNULL(td.[Amount], 0)	--�ړ����o
				-- + ISNULL(qd.[Amount], 0)	--�I��
				 )
				) AS [DiffCost]
			FROM #PartsList pl
			--INNER JOIN #QuantityPost qp					--�����݌�
			--	ON qp.[WarehouseCode] = pl.[WarehouseCode] AND qp.[PartsNumber] = pl.[PartsNumber]
			INNER JOIN #QuantityCalc qc						--���_�݌�
				ON qc.[WarehouseCode] = pl.[WarehouseCode] AND qc.[PartsNumber] = pl.[PartsNumber]
			INNER JOIN #AverageCost ac					--�V�P��
				ON ac.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #Previous p						--�O�c
				ON p.[WarehouseCode] = pl.[WarehouseCode] AND p.PartsNumber = pl.PartsNumber
			LEFT JOIN #PartsPurchase pp					--�d��(����j
				ON pp.[WarehouseCode] = pl.[WarehouseCode] AND pp.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #TransferArrival ta				--�ړ����
				ON ta.[WarehouseCode] = pl.[WarehouseCode] AND ta.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #ServiceSales ss					--�[��(���o�j
				ON ss.[WarehouseCode] = pl.[WarehouseCode] AND ss.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #TransferDeparture td				--�ړ����o
				ON td.[WarehouseCode] = pl.[WarehouseCode] AND td.[PartsNumber] = pl.[PartsNumber]
			/*	--Del 2015/06/02 arc yano
			LEFT JOIN #QuantityDiff  qd					--�I��
				ON qd.[WarehouseCode] = pl.[WarehouseCode] AND qd.[PartsNumber] = pl.[PartsNumber]
			*/
			GROUP BY
				pl.[WarehouseCode]
			,	pl.[PartsNumber]
			--�C���f�b�N�X�Đ���
			DROP INDEX IX_DiffCost ON #DiffCost
			CREATE UNIQUE INDEX IX_DiffCost ON #DiffCost ([WarehouseCode], [PartsNumber])
			
			/****************
			�����󕥕\
			****************/
			--�󕥏����ꎞ�e�[�u���ɑ}��
			INSERT INTO #PartsBalance
			SELECT 
				@TargetDateFrom AS [CloseMonth]
			,	'' AS [DepartmentCode]
			,	'' AS [DepartmentName]
			,	pl.[PartsNumber] AS [PartsNumber]
			,	pl.[PartsNameJp] AS [PartsNameJp]
			,	COALESCE(pa.[Cost], pl.[Cost], 0) AS [PreCost]									--�O�c�P��
			,	ISNULL(p.[Quantity], 0) AS [PreQuantity]										--�O�c��
			,	ISNULL(p.[Amount], 0) AS [PreAmount]											--�O�c���z
			,	ISNULL(pp.[Quantity], 0) AS [PurchaseQuantity]									--�d������
			,	ISNULL(pp.[Amount], 0) AS [PurchaseAmount]										--�d�����z
			,	ISNULL(ta.[Quantity], 0) AS [TransferArrivalQuantity]							--�ړ��������
			,	ISNULL(ta.[Amount], 0) AS [TransferArrivalAmount]								--�ړ�������z
			,	ISNULL(ss.[Quantity], 0) AS [ShipQuantity]										--�[�Ԑ���
			,	ISNULL(ss.[Amount], 0) AS [ShipAmount]											--�[�ԋ��z
			,	ISNULL(td.[Quantity], 0) AS [TransferDepartureQuantity]							--�ړ����o����
			,	ISNULL(td.[Amount], 0) AS [TransferDepartureAmount]								--�ړ����o���z
			,	ISNULL(qd.[Quantity], 0) AS [DifferenceQuantity]								--�I����
			,	ISNULL(qd.[Amount], 0) AS [DifferenceAmount]									--�I�����z
			,	ISNULL(dc.[DiffCost], 0) AS [UnitPriceDifference]								--�P�����z
			,	ISNULL(ac.[AverageCost], 0) AS [PostCost]										--�����P��
			,	ISNULL(qp.[Quantity], 0) AS [PostQuantity]										--������
			,	ISNULL(qp.[Quantity] * ac.[AverageCost], 0) AS [PostAmount]						--�������z
			,	ISNULL(ip.[Quantity], 0) AS [InProcessQuantity]									--�d�|��
			,	ISNULL(ip.[Quantity], 0) * ISNULL(ac.[AverageCost], 0) AS [InProcessAmount]		--�d�|���z
			,	ISNULL(pl.[Cost], 0) AS [PurcharceOrderPrice]									--�����P��
			,	@CalcDate AS [CalcDate]															--�Z�o��
			,	'sys' AS [CreateEmployeeCode]													--�쐬��	--Mod 2015/06/18
			,	GETDATE() AS [CreateDate]														--�쐬��
			,	'sys' AS [LastUpdateEmployeeCode]												--�X�V��	--Mod 2015/06/18
			,	GETDATE() AS [LastUpdateDate]													--�X�V��
			,	'0' AS [DelFlag]																--�폜�t���O
			,	ISNULL(qc.[Quantity], 0) AS [QuantityCalc]										--���_����	--Mod 2015/06/02
			,	ISNULL(qc.[Quantity] * ac.[AverageCost], 0) AS [AmountCalc]						--���_���z	--Mod 2015/06/02
			,	ISNULL(rs.[Quantity], 0) AS [ReservationQuantity]								--������	--Mod 2015/07/17
			,	ISNULL(rs.[Quantity], 0) * ISNULL(ac.[AverageCost], 0) AS [ReservationAmount]	--�������z	--Mod 2015/07/17
			,	pl.[WarehouseCode]																--�q�ɃR�[�h--Mod 2016/08/13 arc yano #3596
			,	pl.[WarehouseName]																--�q�ɖ�	--Mod 2016/08/13 arc yano #3596
			FROM #PartsList pl									--���i���X�g
			LEFT JOIN #PreviousAll pa							--�O�c���X�gALL
				ON pa.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #Previous p								--�O�c
				ON p.[WarehouseCode] = pl.[WarehouseCode] AND p.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #PartsPurchase pp							--�d��(����j
				ON pp.[WarehouseCode] = pl.[WarehouseCode] AND pp.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #TransferArrival ta						--�ړ����
				ON ta.[WarehouseCode] = pl.[WarehouseCode] AND ta.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #ServiceSales ss							--�[��(���o�j
				ON ss.[WarehouseCode] = pl.[WarehouseCode] AND ss.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #TransferDeparture td						--�ړ����o
				ON td.[WarehouseCode] = pl.[WarehouseCode] AND td.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #QuantityDiff  qd							--�I��
				ON qd.[WarehouseCode] = pl.[WarehouseCode] AND qd.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #DiffCost dc								--�P����
				ON dc.[WarehouseCode] = pl.[WarehouseCode] AND dc.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #AverageCost ac							--�V�P��
				ON ac.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #QuantityPost qp							--�����݌�
				ON qp.[WarehouseCode] = pl.[WarehouseCode] AND qp.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #InProcess ip								--�d�|
				ON ip.[WarehouseCode] = pl.[WarehouseCode] AND ip.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #QuantityCalc qc							--���_�݌�					--Mod 2015/06/02 arc yano
				ON qc.[WarehouseCode] = pl.[WarehouseCode] AND qc.[PartsNumber] = pl.[PartsNumber]
			LEFT JOIN #Reservation rs							--����						--Mod 2015/07/17 arc yano
				ON rs.[WarehouseCode] = pl.[WarehouseCode] AND rs.[PartsNumber] = pl.[PartsNumber]



			--���󕥃e�[�u���X�V
			--�����Ώی��̊����f�[�^�폜(�Ώ۔N���ȍ~���폜�j
			DELETE FROM [PartsBalance] WHERE [CloseMonth] >= @TargetDateFrom
			--�����Ώی��̃f�[�^�}��
			INSERT INTO [PartsBalance]
			SELECT pb.* 
			FROM #PartsBalance pb
			WHERE @p_isShowAllZero = 1
			OR (
				@p_isShowAllZero = 0
				AND NOT (
					pb.[PreQuantity] = 0
				AND	pb.[PurchaseQuantity] = 0
				AND	pb.[TransferArrivalQuantity] = 0
				AND	pb.[ShipQuantity] = 0
				AND	pb.[TransferDepartureQuantity] = 0
				AND	pb.[DifferenceQuantity] = 0
				AND	pb.[UnitPriceDifference] = 0
				AND	pb.[PostQuantity] = 0
				AND	pb.[PostQuantity] = 0
				AND pb.[InProcessQuantity] = 0
				AND pb.[QuantityCalc] = 0				--Add 2015/06/02
				AND pb.[ReservationQuantity] = 0		--Add 2015/07/17
				)
			)

			--�󕥒��Ǘ��e�[�u���X�V
			IF NOT EXISTS (
					SELECT 'X' 
					FROM [InventoryMonthControlPartsBalance] mc 
					WHERE mc.[InventoryMonth] = CONVERT(NVARCHAR(8), @TargetDateFrom, 112)
					)
			BEGIN
				INSERT INTO [InventoryMonthControlPartsBalance]
				SELECT
					CONVERT(NVARCHAR(8), @TargetDateFrom, 112) AS [InventoryMonth]
				,	CASE @isExistsInventoryStock 
					WHEN 1 THEN '002'
					ELSE '001' END AS  [InventoryStatus]	--�I�������܂��Ă��邩�ǂ���(002:���܂��Ă���A�ȊO�F���܂��Ă��Ȃ��j
				,	'sys' AS [CreateEmployeeCode]			--Mod 2015/06/18
				,	GETDATE() AS [CreateDate]
				,	'sys' AS [LastUpdateEmployeeCode]		--Mod 2015/06/18
				,	GETDATE() AS [LastUpdateDate]
				,	'0' AS [DelFlag]
			END
			ELSE
			BEGIN
				UPDATE [InventoryMonthControlPartsBalance] SET
					[InventoryStatus] = CASE @isExistsInventoryStock 
										WHEN 1 THEN '002'
										ELSE '001' END
				,	[LastUpdateEmployeeCode] = 'sys'
				,	[LastUpdateDate] = GETDATE()
				WHERE [InventoryMonth] = CONVERT(NVARCHAR(8), @TargetDateFrom, 112)
			END

			
			--���ړ����ϒP���e�[�u���X�V
			--���I�����m�肵�Ă���ꍇ�̂ݎ��{
			IF @isExistsInventoryStock = 1
			BEGIN
				--�����Ώی��̊����f�[�^�폜(�Ώ۔N���ȍ~���폜�j
				DELETE FROM [PartsAverageCost] WHERE [CloseMonth] >= @TargetDateFrom
				--�����Ώی��̃f�[�^�}��
				INSERT INTO [PartsAverageCost] 
				SELECT DISTINCT
					@p_CompanyCode AS [CompanyCode]
				,	@TargetDateFrom AS [CloseMonth]
				,	ac.[PartsNumber]
				,	ac.[AverageCost] AS [Price]
				,	@CloseDateTime AS [CloseDateTime]
				,	GETDATE() AS [CreateDate]
				,	'sys' AS [CreateEmployeeCode]			--Mod 2015/06/18
				,	GETDATE() AS [LastUpdateDate]
				,	'sys' AS [LastUpdateEmployeeCode]		--Mod 2015/06/18
				,	'0' AS [DelFlag]
				FROM #AverageCost ac
				WHERE ac.[AverageCost] <> 0

			END

			--���̏����Ώی�
			SET @TargetMonthCount = @TargetMonthCount - 1				--�c�����f�N�������g
			SET @TargetMonthPrevious = @TargetDateFrom					--�Ώی��O���C���N�������g(������̓����j
			SET @TargetDateFrom = DATEADD(m, 1, @TargetMonthPrevious)	--�Ώۓ�From�C���N�������g(������̑O���{�P�j
			SET @TargetDateTo = DATEADD(m, 1, @TargetDateFrom)			--�Ώۓ�To�C���N�������g(������̓����{�P�j
			IF @TargetDateTo > @TODAY
				SET @TargetDateTo = @TODAY
			SET @CalcDate = DATEADD(d, -1, @TargetDateTo)				--�Z�o��
		--���[�v�G���h
		END


		--Add 2018/05/14 arc yano #3880
		-- ---------------------------------------------------
		-- �ŐV���̈ړ����ϒP�����ړ����ϒP���e�[�u���ɓo�^
		-- --------------------------------------------------
		IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PartsMovingAverageCost]') AND type in (N'U'))
		BEGIN
		
			--�@�ړ����ϒP���e�[�u��(#AverageCost)�ɑ��݂���ꍇ�͍X�V���s��
			UPDATE
				dbo.PartsMovingAverageCost
			SET
				 Price = ac.AverageCost
				,DelFlag = '0'
				,LastUpdateEmployeeCode = 'Insert_PartsBalance'
				,LastUpdateDate = GETDATE()
			FROM
				dbo.PartsMovingAverageCost mac INNER JOIN
				#AverageCost ac ON mac.PartsNumber = ac.PartsNumber
			WHERE
				DelFlag = '0'

			--�A�ړ����ϒP���e�[�u���ɑ��݂��Ȃ��ꍇ�͐V�K�쐬
			INSERT INTO
				dbo.PartsMovingAverageCost
			SELECT
				  @p_CompanyCode
				 ,ac.PartsNumber
				 ,ac.AverageCost
				 ,CreateEmployeeCode = 'Insert_PartsBalance'
				 ,CreateDate = GETDATE()
				 ,LastUpdateEmployeeCode = 'Insert_PartsBalance'
				 ,LastUpdateDate = GETDATE()
				 ,'0'
			FROM
				#AverageCost ac
			WHERE
				NOT EXISTS
				(
					select 'x' from dbo.PartsMovingAverageCost mac where ac.PartsNumber = mac.PartsNumber
				)
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
END



GO


