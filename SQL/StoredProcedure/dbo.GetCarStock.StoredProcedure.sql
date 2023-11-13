USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCarStock]    Script Date: 2018/12/11 12:42:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- ==============================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2015/01/30 arc yano �ԗ��Ǘ��Ή� �ԗ��Ǘ��f�[�^�擾��linq����X�g�v���ɕύX
-- Update date

-- 2018/11/07 yano #3922 �ԗ��Ǘ��\(�^�}�\)�@���̑��C��
-- 2018/08/28 yano #3922 �ԗ��Ǘ��\(�^�}�\)�@�@�\���P�@�S�ʓI�ɉ��C
-- 2018/06/06 arc yano #3883 �^�}�\���P �������i��ǉ�
-- 2017/08/21 arc yano #3782 �ԗ��L�����Z���@�\�ǉ� �d���L�����Z����ǉ�
-- 2017/03/01 arc yano #3659 ���b��Ή��@�������I���������P�[�V�����̏ꍇ�͂��ƂōX�V���ꂽ���̂������Ă���B
-- 2016/11/30 arc yano #3659 �ԗ��Ǘ� �������I����ǉ�
-- 2015/04/04 arc yano �ԗ��Ǘ��Ή�
--					�d������(���f�B�[��)�폜�A���������A���T�C�N�����̒ǉ�
--					���Гo�^�̌��ԗ��f�[�^���擾����ۂ́ADelFlag<> '1'���݂Ȃ�
--2015/02/18 arc yano �ԗ��Ǘ��Ή� �݌Ƀf�[�^�̍݌ɋ��_���\������Ȃ�
-- ==============================================================================================================================================
CREATE PROCEDURE [dbo].[GetCarStock] 
	 @StrTargetMonth NVARCHAR(7)				--�w�茎
	,@ActionFlag int = 0						--����w��(0:��ʕ\��, 1:�X�i�b�v�V���b�g�ۑ�)
	,@DataType NVARCHAR(3)						--�f�[�^���
	,@NewUsedType NVARCHAR(3) = NULL			--�V���敪
	,@SupplierCode NVARCHAR(10) = NULL			--�d����R�[�h
	,@SalesCarNumber NVARCHAR(50) = NULL		--�Ǘ��ԍ�
	,@Vin NVARCHAR(20) = NULL					--�ԑ�ԍ�
	,@EmployeeCode NVARCHAR(50) = NULL			--�S����
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-----------------------
	--�ȍ~�͋���
	-----------------------
	--�ϐ��錾
	DECLARE @NOW DATETIME = GETDATE()
	DECLARE @TODAY DATETIME
	DECLARE @THISMONTH DATETIME
	--�w�茎��datetime�^�ɕύX
	DECLARE @TargetMonth datetime = CONVERT(datetime, @StrTargetMonth + '/01')

	--��������
	DECLARE @ErrorMessage NVARCHAR(4000) = ''
	DECLARE @ErrorNumber INT = 0

	
	--���ꎞ�\�̍폜
	/*************************************************************************/
	IF OBJECT_ID(N'tempdb..#temp_CodeList', N'U') IS NOT NULL						--�R�[�h���X�g
	DROP TABLE #temp_CodeListt;														
	IF OBJECT_ID(N'tempdb..#temp_CarriedBalance', N'U') IS NOT NULL					--�O���J�z���X�g
	DROP TABLE #temp_CarriedBalance;										
	IF OBJECT_ID(N'tempdb..#temp_Transfer1', N'U') IS NOT NULL						--�ړ����X�g�@
	DROP TABLE #temp_Transfer1;
	IF OBJECT_ID(N'tempdb..#temp_Transfer2', N'U') IS NOT NULL						--�ړ����X�g�A
	DROP TABLE #temp_Transfer2;
	IF OBJECT_ID(N'tempdb..#temp_CarPurchaseDetail', N'U') IS NOT NULL				--�ԗ��d�����X�g
	DROP TABLE #temp_CarPurchaseDetail;
	IF OBJECT_ID(N'tempdb..#temp_CarPurchaseCancel', N'U') IS NOT NULL				--�ԗ��d���L�����Z��
	DROP TABLE #temp_CarPurchaseCancel;
	IF OBJECT_ID(N'tempdb..#temp_TradeVin', N'U') IS NOT NULL						--����ԓ`�[���
	DROP TABLE #temp_TradeVin;
	IF OBJECT_ID(N'tempdb..#temp_Option', N'U') IS NOT NULL							--AA�I�v�V�������
	DROP TABLE #temp_Option;
	IF OBJECT_ID(N'tempdb..#temp_AkaSlipList', N'U') IS NOT NULL					--�ԓ`�[���X�g
	DROP TABLE #temp_AkaSlipList;
	IF OBJECT_ID(N'tempdb..#temp_KuroSlipList', N'U') IS NOT NULL					--���`�[���X�g
	DROP TABLE #temp_KuroSlipList;
	IF OBJECT_ID(N'tempdb..#temp_CarSalesDetail', N'U') IS NOT NULL					--�̔����X�g
	DROP TABLE #temp_CarSalesDetail;
	IF OBJECT_ID(N'tempdb..#temp_SalesCar', N'U') IS NOT NULL						--�ԗ����						
	DROP TABLE #temp_SalesCar;
	IF OBJECT_ID(N'tempdb..#temp_InventoryStockCar', N'U') IS NOT NULL				--�����I��
	DROP TABLE #temp_InventoryStockCar;

	--�����ꎞ�\�̐錾
	/*************************************************************************/
	--�R�[�h���X�g
	CREATE TABLE #temp_CodeList (
		[SalesCarNumber] NVARCHAR(50) NOT NULL			-- �ԗ��Ǘ��ԍ�
	)
	CREATE INDEX ix_temp_CodeList ON #temp_CodeList (SalesCarNumber)

	--�O���J�z���X�g
	CREATE TABLE #temp_CarriedBalance (
		 [SalesCarNumber] nvarchar(50) NOT NULL			--�ԗ��Ǘ��ԍ�
		,[BrandStore] nvarchar(50)						--�戵�u�����h
		,[PurchaseDate] datetime						--���ɓ�
		,[PurchaseLocationCode] nvarchar(12)			--�d�����P�[�V�����R�[�h
		,[PurchaseLocationName] nvarchar(50)			--�d�����P�[�V������
		,[CarPurchaseType] nvarchar(3)					--�d���敪
		,[SupplierCode] nvarchar(10)					--�d����R�[�h
		,[EndInventory] decimal(10, 0)					--�����݌�
		,[RecycleAmount] decimal(10, 0)					--���T�C�N������
		,[PurchaseAmount] decimal(10, 0)				--�d�����z
		,[CarPurchaseTypeName] nvarchar(50)				--�d���敪��
		,[SupplierName] nvarchar(80)					--�d���於
		,[SelfRegistrationPurchaseDate] datetime		--���Гo�^�O�̎��̓��ɓ�
		,[MakerName] nvarchar(100)						--���[�J�[��
		,[CarName] nvarchar(100)						--�Ԏ햼
	)
	CREATE INDEX ix_temp_CarriedBalance ON #temp_CarriedBalance (SalesCarNumber)

	--�ړ��e�[�u���@	
	CREATE TABLE #temp_Transfer1 (
		SalesCarNumber	NVARCHAR(50)					--�ԗ��Ǘ��ԍ�
	,	TransferNumber	NVARCHAR(50)
	)
	CREATE INDEX ix_temp_Transfer1 ON #temp_Transfer1 (SalesCarNumber)

	--�ړ��e�[�u���A
	CREATE TABLE #temp_Transfer2 (
		SalesCarNumber	NVARCHAR(50)					--�ԗ��Ǘ��ԍ�
	,	ArrivalLocationCode	NVARCHAR(12)
	)
	CREATE INDEX ix_temp_Transfer2 ON #temp_Transfer2 (SalesCarNumber)

	--�ԗ��d�����X�g
	CREATE TABLE #temp_CarPurchaseDetail (
		  PurchaseDate datetime							--�d����
		, SalesCarNumber NVARCHAR(50)					--�ԗ��Ǘ��ԍ�
		, DepartmentCode NVARCHAR(3)					--����R�[�h
		, PurchaseLocationCode NVARCHAR(12)				--�d���惍�P�[�V�����R�[�h
		, PurchaseLocationName NVARCHAR(50)				--�d���惍�P�[�V������				--Mod 2016/11/30 arc yano #3659
		, CarPurchaseType NVARCHAR(3)					--�d�����(�R�[�h)
		, CarPurchaseTypeName VARCHAR(50)				--�d�����(����)					--Add 2016/11/30 arc yano #3659
		, SupplierCode NVARCHAR(10)						--�d����R�[�h
		, VehiclePrice DECIMAL(10, 0)					--�ԗ��{�̉��i
		, VehicleTax DECIMAL(10, 0)						--�ԗ��{�̏����
		, VehicleAmount DECIMAL(10, 0)					--�ԗ��{�̐ō����i
		, AuctionFeePrice DECIMAL(10, 0)				--�I�[�N�V�������D��
		, AuctionFeeTax DECIMAL(10, 0)					--�I�[�N�V�������D�������
		, AuctionFeeAmount DECIMAL(10, 0)				--�I�[�N�V�������D���ō�
		, RecycleAmount DECIMAL(10, 0)					--���T�C�N�����z					--Mod 2015/03/13 arc yano #3164
		, RecyclePrice DECIMAL(10, 0)					--���T�C�N�����i
		, CarTaxAppropriatePrice DECIMAL(10, 0)			--���ŏ[�����i
		, CarTaxAppropriateTax DECIMAL(10, 0)			--���ŏ[�������
		, CarTaxAppropriateAmount DECIMAL(10, 0)		--���ŏ[�����z
		, Amount DECIMAL(10, 0)							--�d�����z
		, TaxAmount DECIMAL(10, 0)						--�d�������
		, TotalAmount DECIMAL(10, 0)					--�d���ō����i
		, FinancialAmount DECIMAL(10, 0)				--�d�����i(���T�C�N��������)		--Add 2018/06/06 arc yano #3883
		, OtherAccount DECIMAL(10, 0)					--��������						--Add 2018/08/28 yano #3922
		, CancelFlag NVARCHAR(2)						--�L�����Z���t���O
		, BrandStore NVARCHAR(50)						--�戵�u�����h
		, TradeCarSlipNumber NVARCHAR(50)				--����Ԃ̓`�[�ԍ�
		, OldSalesCarNumber  NVARCHAR(50)				--���ԗ��Ǘ��ԍ�
		, SelfRegistrationPurchaseDate datetime			--���Гo�^�O�̎��̓��ɓ�
	)
	CREATE INDEX ix_temp_CarPurchaseDetail ON #temp_CarPurchaseDetail (SalesCarNumber)

	--�ԗ��d���L�����Z�����X�g
	CREATE TABLE #temp_CarPurchaseCancel (
		  CancelDate datetime							--�L�����Z����
		, SalesCarNumber NVARCHAR(50)					--�ԗ��Ǘ��ԍ�
		, CancelAmount DECIMAL(10, 0)					--�d�����i(�L�����Z��)
	)
	CREATE INDEX ix_temp_CarPurchaseCancel ON #temp_CarPurchaseCancel (SalesCarNumber)

	--����Ԃ̔[�ԓ����
	CREATE TABLE #temp_TradeVin
	 (
		SalesCarNumber	NVARCHAR(50)					--�Ǘ��ԍ�
	,	SalesDate		DATETIME						--�[�ԓ�
	)
	CREATE INDEX ix_temp_TradeVin ON #temp_TradeVin (SalesCarNumber)

	--�I�v�V�������i
	CREATE TABLE #temp_Option (
			SlipNumber NVARCHAR(50)						--�`�[�ԍ�
		,	RevisionNumber INT							--�����ԍ�
		,	Amount DECIMAL(10, 0)						--���z
		,	TotalAmount DECIMAL(10, 0)					--���v���z
	)
	CREATE INDEX ix_temp_Option ON #temp_Option (SlipNumber)

	--�ԓ`���X�g
	CREATE TABLE #temp_AkaSlipList (
		 SlipNumber NVARCHAR(50)
		,SalesCarNumber NVARCHAR(50)
	)
	CREATE INDEX ix_temp_AkaSlipList ON #temp_AkaSlipList (SlipNumber)

	--���`���X�g
	CREATE TABLE #temp_KuroSlipList (
		 SlipNumber NVARCHAR(50)
		,KuroSlipNumber NVARCHAR(50)
		,SalesCarNumber NVARCHAR(50)
	)
	CREATE INDEX ix_temp_KuroSlipList ON #temp_KuroSlipList (SlipNumber)

	--�̔����X�g
	CREATE TABLE #temp_CarSalesDetail (
	  SalesDate datetime					--�[�ԓ�
	, SlipNumber NVARCHAR(50)				--�`�[�ԍ�
	, SalesCarNumber NVARCHAR(50)			--�Ǘ��ԍ�
	, SalesType NVARCHAR(3)					--�̔��敪
	, Vin NVARCHAR(20)						--�ԑ�ԍ�
	, CustomerCode NVARCHAR(10)				--�ڋq�R�[�h
	, CustomerName NVARCHAR(80)				--�ڋq��
	, DepartmentCode NVARCHAR(3)			--����R�[�h
	, DepartmentName NVARCHAR(20)			--���喼
	, SalesPrice DECIMAL(10,0)				--�̔����i
	, ShopOptionAmount DECIMAL(10,0)		--�X�܃I�v�V�������i
	, SalesCostTotalAmount DECIMAL(10,0)	--����p���v
	, DiscountAmount DECIMAL(10,0)			--�l�����i
	, SalesTotalAmount DECIMAL(10,0)		--�̔����v���i
	, CarName NVARCHAR(50)					--�Ԏ햼
	, CarBrandName NVARCHAR(50)				--�ԗ��u�����h��
	, AreaCode NVARCHAR(3)					--�G���A�R�[�h
	, CustomerType NVARCHAR(3)				--�ڋq���
	, MakerOptionAmount DECIMAL(10,0)		--���[�J�[�I�v�V�������i
	, BrandStore NVARCHAR(50)				--�戵�u�����h
	, CustomerTypeName NVARCHAR(50)			--�ڋq��ʖ���
	)
	CREATE INDEX ix_temp_CarSalesDetail ON #temp_CarSalesDetail (SalesCarNumber)

	
	--�ԗ��}�X�^�e�[�u��
	CREATE TABLE #temp_SalesCar (
		  SalesCarNumber NVARCHAR(50)
		, MakerName NVARCHAR(100)
		, NewUsedType NVARCHAR(3)
		, NewUsedTypeName VARCHAR(50)						--Add 2016/11/30 arc yano #3659
		, CarName NVARCHAR(100)
		, CarGradeCode NVARCHAR(30)
		, Vin NVARCHAR(20)
		, CarUsage NVARCHAR(3)
		, CompanyRegistrationFlag NVARCHAR(2)
		, BrandStore NVARCHAR(50)
	)
	CREATE INDEX ix_temp_SalesCar ON #temp_SalesCar (SalesCarNumber)

	--�������I�f�[�^
	CREATE TABLE #temp_InventoryStockCar (
			SalesCarNumber NVARCHAR(50)
		,	LocationCode NVARCHAR(12)
	)
	CREATE INDEX ix_temp_InventoryStockCar ON #temp_InventoryStockCar (SalesCarNumber)

	--�ԗ��Ǘ��f�[�^
	CREATE TABLE #temp_CarStock(
		 [ProcessDate] datetime
		,[SalesCarNumber] nvarchar(50)
		,[BrandStore]  nvarchar(50)
		,[NewUsedType] nvarchar(3)
		,[PurchaseDate] datetime
		,[CarName] nvarchar(100)
		,[CarGradeCode] nvarchar(20)
		,[Vin] nvarchar(50)
		,[PurchaseLocationCode] nvarchar(12)
		,[CarPurchaseType] nvarchar(3)
		,[SupplierCode] nvarchar(10)
		,[BeginningInventory] decimal(10, 0)
		,[MonthPurchase] decimal(10, 0)
		,[SalesDate] datetime
		,[SlipNumber] nvarchar(50)
		,[SalesType] nvarchar(50)
		,[CustomerCode] nvarchar(50)
		,[SalesPrice] decimal(10, 0)
		,[DiscountAmount] decimal(10, 0)
		,[ShopOptionAmount] decimal(10, 0)
		,[SalesCostTotalAmount] decimal(10, 0)
		,[SalesTotalAmount] decimal(10, 0)
		,[SalesCostAmount] decimal(10, 0)
		,[SalesProfits] decimal(10, 0)
		,[ReductionTotal] decimal(10, 0)
		,[SelfRegistration] decimal(10, 0)
		,[OtherDealer] decimal(10, 0)
		,[DemoCar] decimal(10, 0)
		,[TemporaryCar] decimal(10, 0)
		,[EndInventory] decimal(10, 0)
		,[RecycleAmount] decimal(10, 0)
		,[OtherAccount] decimal(10, 0)
		,[RentalCar] decimal(10, 0)
		,[BusinessCar] decimal(10, 0)
		,[PRCar] decimal(10, 0)
		,[LocationCode] nvarchar(12)
		,[CancelPurchase] decimal(10, 0)
		,[CarPurchaseTypeName] nvarchar(50)
		,[MakerName] nvarchar(100)
		,[PurchaseLocationName] nvarchar(50)
		,[InventoryLocationName] nvarchar(50)
		,[SupplierName] nvarchar(50)
		,[SalesDepartmentCode] nvarchar(3)
		,[SalesDepartmentName] nvarchar(20)
		,[NewUsedTypeName] nvarchar(50)
		,[CustomerName] nvarchar(80)
		,[TradeCarSlipNumber] nvarchar(50)
		,[SelfRegistrationPurchaseDate] datetime
		,[CustomerTypeName] nvarchar(50)
	)
	CREATE UNIQUE INDEX ix_temp_CarStock ON #temp_CarStock (ProcessDate, SalesCarNumber)
	/*************************************************************************/
	--���ݓ���
	SET @NOW = GETDATE()
	--����1��
	SET @THISMONTH = CONVERT(DATETIME, CONVERT(NVARCHAR(7), @TODAY, 111) + '/01', 111)

	--�������Ώی��͈͂̐ݒ�
	--���u�������ߏ������߁v���S������܂��Ă��錎�̒��ōő匎�̗�����1��<=x<���������̗���1������(�܂��́A�����̏ꍇ�͓��������j
	--���������Ώی�From�̐ݒ�i���܂��Ă��錎�̒��ōő匎�̗���1���j
	DECLARE @TargetMonthFrom DATETIME = NULL
	
	--����w��ɂ��U����
	IF @ActionFlag = 0	--�\���̏ꍇ�́A�{���ߍŐV���̗����ɐݒ�
	BEGIN
		SELECT 
			@TargetMonthFrom = DATEADD(m, 1, ISNULL(MAX(CONVERT(datetime, cm.[CloseMonth], 120)), @THISMONTH))
		FROM 
			[CloseMonthControlCarStock] cm		--�ԗ��Ǘ����e�[�u��
		WHERE 
			cm.[CloseStatus] = '002'			--����
	END
	ELSE
	BEGIN
		SET @TargetMonthFrom = @TargetMonth							--�X�i�b�v�V���b�g�ۑ��̏ꍇ�͎w�茎��ݒ肷��B
	END


	--�Ώی��������ɂȂ�ꍇ�A�����Ƃ���
	IF @TargetMonthFrom > @TODAY
		SET @TargetMonthFrom = @THISMONTH
	
	--���������Ώی�To�̐ݒ�(�w�茎)
	--�w�茎��NULL�̏ꍇ�A�w�茎=������ݒ肷��B�@�����W�b�N�Ƃ��Ă͒ʂ�Ȃ�
	IF @TargetMonth is null
		SET @TargetMonth = @THISMONTH
	
	--�w�茎��ݒ�(�w�茎�̗���1������)
	DECLARE @TargetMonthTo DATETIME = DATEADD(m, 1, @TargetMonth)

	--�����Ώی����^�����Ώی��O��
	DECLARE @TargetMonthCount INT = DATEDIFF(m, @TargetMonthFrom, @TargetMonthTo)
	DECLARE @TargetMonthPrevious DATETIME = DATEADD(m, -1, @TargetMonthFrom)

	--�����Ώۓ��t�͈�From�^�����Ώۓ��t�͈�To
	DECLARE @TargetDateFrom DATETIME = @TargetMonthFrom
	DECLARE @TargetDateTo DATETIME = DATEADD(m, 1, @TargetDateFrom)

	IF @TargetDateTo > @NOW		--���t�͈�TO���������̏ꍇ�A���݂ɂ���
		SET @TargetDateTo = @NOW

	
	--�g�����U�N�V�����J�n 
	BEGIN TRANSACTION
	BEGIN TRY
		--��U�O�����̎ԗ��Ǘ��\�����Ă���
		INSERT INTO
			#Temp_CarStock
		SELECT
		   [ProcessDate]
		  ,[SalesCarNumber]
		  ,[BrandStore]
		  ,[NewUsedType]
		  ,[PurchaseDate]
		  ,[CarName]
		  ,[CarGradeCode]
		  ,[Vin]
		  ,[PurchaseLocationCode]
		  ,[CarPurchaseType]
		  ,[SupplierCode]
		  ,[BeginningInventory]
		  ,[MonthPurchase]
		  ,[SalesDate]
		  ,[SlipNumber]
		  ,[SalesType]
		  ,[CustomerCode]
		  ,[SalesPrice]
		  ,[DiscountAmount]
		  ,[ShopOptionAmount]
		  ,[SalesCostTotalAmount]
		  ,[SalesTotalAmount]
		  ,[SalesCostAmount]
		  ,[SalesProfits]
		  ,[ReductionTotal]
		  ,[SelfRegistration]
		  ,[OtherDealer]
		  ,[DemoCar]
		  ,[TemporaryCar]
		  ,[EndInventory]
		  ,[RecycleAmount]
		  ,[OtherAccount]
		  ,[RentalCar]
		  ,[BusinessCar]
		  ,[PRCar]
		  ,[LocationCode]
		  ,[CancelPurchase]
		  ,[CarPurchaseTypeName]
		  ,[MakerName]
		  ,[PurchaseLocationName]
		  ,[InventoryLocationName]
		  ,[SupplierName]
		  ,[SalesDepartmentCode]
		  ,[SalesDepartmentName]
		  ,[NewUsedTypeName]
		  ,[CustomerName]
		  ,'' AS TradeCarSlipNumber
		  ,[SelfRegistrationPurchaseDate]
		  ,[CustomerTypeName]
		FROM			
			dbo.CarStock
		WHERE
			ProcessDate = @TargetMonthPrevious AND
			DelFlag = '0'	

		--for debug
		--PRINT '�O������ޔ�'

		--�������Ώی��������[�v
		WHILE @TargetMonthCount > 0
		BEGIN
			--�ꎞ�\������
			DELETE FROM  #temp_CodeList							--�R�[�h���X�g
			DELETE FROM  #temp_CarriedBalance					--�O���J�z���X�g
			DELETE FROM  #temp_Transfer1						--�ړ��e�[�u���P
			DELETE FROM  #temp_Transfer2						--�ړ��e�[�u���Q
			DELETE FROM  #temp_CarPurchaseDetail				--�d�����X�g
			DELETE FROM  #temp_CarPurchaseCancel				--�d���L�����Z�����X�g
			DELETE FROM  #temp_TradeVin							--����ԃ��X�g
			DELETE FROM  #temp_Option							--AA�I�v�V����
			DELETE FROM  #temp_AkaSlipList						--�ԓ`�[���X�g
			DELETE FROM  #temp_KuroSlipList						--���`�[���X�g
			DELETE FROM  #temp_CarSalesDetail					--�̔����X�g
			DELETE FROM  #temp_SalesCar							--�ԗ���񃊃X�g
			DELETE FROM  #temp_InventoryStockCar				--�������I���X�g

			/***************************************************
			�����ړ��f�[�^
			****************************************************/
			--�ړ��f�[�^
			INSERT INTO #temp_Transfer1
			SELECT 
				SalesCarNumber
			,	MAX(TransferNumber) AS tranferNumber
			FROM 
				dbo.[Transfer]
			WHERE										--Add 2018/08/28 yano #3922
				ArrivalDate >= @TargetDateFrom AND
				ArrivalDate < @TargetDateTo
			GROUP BY
				SalesCarNumber

			DROP INDEX ix_temp_Transfer1 ON  #temp_Transfer1
			CREATE INDEX ix_temp_Transfer1 ON #temp_Transfer1 (SalesCarNumber)

			--�Ώۊ��Ԃł̈ړ��惍�P�[�V�������擾
			INSERT INTO #temp_Transfer2
			SELECT
				A.SalesCarNumber
			,	A.ArrivalLocationCode
			FROM 
				dbo.Transfer AS A 
			WHERE EXISTS(
				SELECT 'X'
				FROM #Temp_Transfer1 x
				WHERE x.TransferNumber = A.TransferNumber
				)

			DROP INDEX ix_temp_Transfer2 ON #temp_Transfer2
			CREATE INDEX ix_temp_Transfer2 ON #temp_Transfer2 (SalesCarNumber)

			--for debug
			--PRINT '�ړ��f�[�^�쐬'

			/***************************************************
			���������d���f�[�^
			****************************************************/
			--�Ώی��̎d���f�[�^���擾����
			INSERT INTO
				#temp_CarPurchaseDetail
				SELECT            	    
					  P.PurchaseDate
					, P.SalesCarNumber
					, P.DepartmentCode
					, P.PurchaseLocationCode
					, L.LocationName AS PurchaseLocationName
					, P.CarPurchaseType
					--�d���敪��
					, CASE 
					  WHEN
						bg.ProcType = '006'	--���Гo�^
					  THEN
						FORMAT(P.PurchaseDate, 'yyyyMM') + '���Гo' 
					  WHEN
						bg.ProcType = '010'	--���p
					  THEN
						'�Œ莑�Y�U��' 
					  ELSE
						cCP.Name
					  END AS CarPurchaseTypeName
					, P.SupplierCode
					, P.VehiclePrice
					, P.VehicleTax
					, P.VehicleAmount
					, P.AuctionFeePrice
					, P.AuctionFeeTax
					, P.AuctionFeeAmount
					, P.RecycleAmount								--2015/03/13 arc yano #3164
					, P.RecyclePrice
					, P.CarTaxAppropriatePrice
					, P.CarTaxAppropriateTax
					, P.CarTaxAppropriateAmount
					, P.Amount
					, P.TaxAmount
					, P.TotalAmount
					--��v��̎d�����z
					, CASE 
					  WHEN
						bg.ProcType = '010'	--���p
					  THEN
						NULL
					  ELSE
						P.FinancialAmount
					  END AS FinancialAmount
					--��������
					, CASE 
					  WHEN
						bg.ProcType = '010'	--���p
					  THEN
						P.FinancialAmount
					  ELSE
						NULL
					  END AS OtherAccount
					--�L�����Z���t���O
					, P.CancelFlag
					--�戵�u�����h
					, cn.Name AS BrandStore
					--����Ԃ̓`�[�ԍ�
					, CASE WHEN
						P.CarPurchaseType = '001'
					  THEN
						P.SlipNumber
					  ELSE
						NULL
					  END AS TradeCarStlipNumber
					--���ԗ��Ǘ��ԍ�(���Гo�^�̏ꍇ�ɐݒ�)
					,CASE WHEN
						bg.ProcType = '006'	--���Гo�^
					 THEN
						bg.SalesCarNumber	--���Ǘ��ԍ�
					 ELSE
						NULL
					 END AS OldSalesCarNumber
					--���Гo�^�O�̎ԗ��d����
					, NULL AS SelfRegistrationPurchaseDate

				FROM              
					dbo.CarPurchase AS P INNER JOIN
					dbo.Location AS L ON P.PurchaseLocationCode = L.LocationCode LEFT OUTER JOIN
					dbo.c_CarPurchaseType cCP ON P.CarPurchaseType  = cCP.Code LEFT OUTER JOIN
					dbo.ConsumptionTax ct ON P.ConsumptionTaxId = ct.ConsumptionTaxId LEFT JOIN
					dbo.BackGroundDemoCar bg ON P.SalesCarNumber = bg.NewSalesCarNumber LEFT OUTER JOIN
					dbo.Department dp ON P.DepartmentCode = dp.DepartmentCode LEFT OUTER JOIN
					dbo.c_CodeName cn ON cn.CategoryCode = '002' AND dp.BrandStoreCode = cn.Code
				WHERE             
					P.DelFlag = '0' AND
					P.PurchaseStatus = '002' AND															--�d���X�e�[�^�X=�u�d���ρv
					P.PurchaseDate >= @TargetDateFrom AND
					P.PurchaseDate < @TargetDateTo
				
				DROP INDEX ix_temp_CarPurchaseDetail ON #temp_CarPurchaseDetail
				CREATE INDEX ix_temp_CarPurchaseDetail ON #temp_CarPurchaseDetail (SalesCarNumber)


			--�����d����������ԗ��̒����������擾����
			INSERT INTO
				#temp_TradeVin
			SELECT
				 sd.SalesCarNumber
				,Max(sd.SalesDate)
			FROM
			(
				SELECT
					 sc.SalesCarNumber
					,sh.SalesDate
				FROM
					dbo.CarSalesHeader sh INNER JOIN
					dbo.SalesCar sc ON sh.TradeInVin1 = sc.Vin and sc.DelFlag = '0'
				WHERE
					sh.TradeInVin1 is not null AND
					sh.TradeInVin1 <> '' AND
					sh.DelFlag = '0' AND
					sh.SalesOrderStatus = '005' AND
					EXISTS
					(
						select 'x' from #temp_CarPurchaseDetail cp where cp.CarPurchaseType = '001' and sc.SalesCarNumber = cp.SalesCarNumber
					)
				UNION
				SELECT
					 sc.SalesCarNumber
					,sh.SalesDate
				FROM
					dbo.CarSalesHeader sh INNER JOIN
					dbo.SalesCar sc ON sh.TradeInVin2 = sc.Vin and sc.DelFlag = '0'
				WHERE
					sh.TradeInVin2 is not null AND
					sh.TradeInVin2 <> '' AND
					sh.DelFlag = '0' AND
					sh.SalesOrderStatus = '005' AND
					EXISTS
					(
						select 'y' from #temp_CarPurchaseDetail cp where cp.CarPurchaseType = '001' and sc.SalesCarNumber = cp.SalesCarNumber
					)
				UNION
				SELECT
					 sc.SalesCarNumber
					,sh.SalesDate
				FROM
					dbo.CarSalesHeader sh INNER JOIN
					dbo.SalesCar sc ON sh.TradeInVin3 = sc.Vin and sc.DelFlag = '0'
				WHERE
					sh.TradeInVin3 is not null AND
					sh.TradeInVin3 <> '' AND
					sh.DelFlag = '0' AND
					sh.SalesOrderStatus = '005' AND
					EXISTS
					(
						select 'z' from #temp_CarPurchaseDetail cp where cp.CarPurchaseType = '001' and sc.SalesCarNumber = cp.SalesCarNumber
					)
			) sd
			GROUP BY
				 sd.SalesCarNumber
			
			DROP INDEX ix_temp_TradeVin ON #temp_TradeVin
			CREATE INDEX ix_temp_TradeVin ON #temp_TradeVin (SalesCarNumber)

			--for debug
			--PRINT '������쐬'

			--����Ԃ̒����`�[�̔[�ԓ��ȑO�Ɏd�����s���Ă���ꍇ�́A�d���敪���́u����ԁi���j�v�Ƃ���
			UPDATE
				#temp_CarPurchaseDetail
			SET
				CarPurchaseTypeName = '�����(���)'
			FROM
				#temp_CarPurchaseDetail cp
			WHERE
				EXISTS
				(
					select 'x' from #temp_TradeVin tv where (tv.SalesDate is NULL OR tv.SalesDate > cp.PurchaseDate) AND cp.SalesCarNumber = tv.SalesCarNumber
				)

			--���Гo�^�O�̎ԗ��̎d������ݒ肷��
			UPDATE
				#temp_CarPurchaseDetail
			SET
				SelfRegistrationPurchaseDate = cp.PurchaseDate
			FROM
				#temp_CarPurchaseDetail tmcp INNER JOIN
				dbo.CarPurchase cp ON cp.DelFlag = '0' and cp.PurchaseStatus = '002' and tmcp.OldSalesCarNumber = cp.SalesCarNumber
				

			--for debug
			--PRINT '����i���j�쐬'
			/***************************************************************
			���������d���L�����Z���f�[�^
			****************************************************************/
			--�Ώی��̎d���f�[�^���擾����
			INSERT INTO
				#temp_CarPurchaseCancel
			SELECT
				 cp.CancelDate
				,cp.SalesCarNumber
				,cp.FinancialAmount
			FROM
				dbo.CarPurchase cp
			WHERE
				cp.DelFlag = '0' AND	
				cp.CancelDate < @TargetDateTo AND
				cp.CancelDate >= @TargetDateFrom AND
				cp.PurchaseStatus = '003'
			
			DROP INDEX ix_temp_CarPurchaseCancel ON #temp_CarPurchaseCancel
			CREATE INDEX ix_temp_CarPurchaseCancel ON #temp_CarPurchaseCancel (SalesCarNumber)	

			--for debug
			--PRINT '�d���L�����Z���쐬'

			/**************************************************************
			���������̔��f�[�^
			***************************************************************/
			 --AA���z�擾
			 INSERT INTO #temp_Option
			 SELECT
				 sl.SlipNumber
				,sl.RevisionNumber
				,SUM(ISNULL(sl.Amount, 0)) AS Amount
				,SUM(ISNULL(sl.Amount, 0) + ISNULL(TaxAmount, 0)) AS TotalAmount
			FROM
				dbo.CarSalesLine sl
			WHERE 
				sl.DelFlag = '0' AND 
				sl.OptionType = '001' AND 
				sl.CarOptionCode IN ('AA001', 'AA002') AND
				EXISTS
				(
					select 
						'x' 
					from 
						dbo.CarsalesHeader sh 
					where 
						sh.DelFlag = '0' and 
						sh.SalesOrderStatus = '005' and 
						sh.SalesDate >= @TargetDateFrom and 
						sh.SalesDate < @TargetDateTo and 
						sh.SlipNumber = sl.SlipNumber and 
						sh.RevisionNumber = sl.RevisionNumber
				)
			GROUP BY       
				 SlipNumber
				,RevisionNumber

			DROP INDEX ix_temp_Option ON #temp_Option
			CREATE INDEX ix_temp_Option ON #temp_Option (SlipNumber,RevisionNumber)

			--for debug
			--PRINT '�����̔����쐬'

			--�������`���X�g
			INSERT INTO #temp_KuroSlipList
			SELECT
				 LEFT(sh.SlipNumber, LEN(sh.SlipNumber)-2) as SlipNubmer
				,sh.SlipNumber
				,sh.SalesCarNumber
			FROM 
				dbo.CarSalesHeader sh
			WHERE
				sh.SlipNumber like '%-2%' AND
				sh.SalesOrderStatus = '005' AND
				sh.DelFlag = '0' AND
				-- Mod 2018/11/07 yano
				sh.SalesDate >= @TargetDateFrom 
				--AND
				--sh.SalesDate < @TargetDateTo
			
			DROP INDEX ix_temp_KuroSlipList ON #temp_KuroSlipList
			CREATE UNIQUE INDEX ix_temp_KuroSlipList ON #temp_KuroSlipList (SlipNumber)

			--for debug
			--PRINT '-�������`���쐬'

			--�����ԓ`(���Ȃ�)���X�g
			INSERT INTO #temp_AkaSlipList
			SELECT
				 LEFT(sh.SlipNumber, LEN(sh.SlipNumber)-2) as SlipNubmer
				,sh.SalesCarNumber
			FROM 
				dbo.CarSalesHeader sh
			WHERE
				sh.SlipNumber like '%-1%' AND
				sh.DelFlag = '0' AND
				sh.SalesDate >= @TargetDateFrom AND
				-- Mod 2018/11/07 yano
				--sh.SalesDate < @TargetDateTo AND
				NOT
				EXISTS
				(
					select 'x' from #temp_KuroSlipList kuro where LEFT(sh.SlipNumber, LEN(sh.SlipNumber)-2) = kuro.SlipNumber
				)
			DROP INDEX ix_temp_AkaSlipList ON #temp_AkaSlipList
			CREATE UNIQUE INDEX ix_temp_AkaSlipList ON #temp_AkaSlipList (SlipNumber)

			--for debug
			--PRINT '-�����ԓ`���쐬'

			-- ------------------
			-- �����̔�
			-- ------------------
			INSERT INTO #temp_CarSalesDetail
			SELECT
				H.SalesDate
			,	H.SlipNumber
			,	H.SalesCarNumber
			,	H.SalesType
			,	H.Vin
			,	C.CustomerCode
			,	C.CustomerName											--Add 2016/11/30 arc yano #3659
			,	D.DepartmentCode
			,	D.DepartmentName										--Add 2016/11/30 arc yano #3659
			,	H.SalesPrice
			,	H.ShopOptionAmount - ISNULL(L.Amount, 0) AS ShopOptionAmount
			,	H.SalesCostTotalAmount
			,	ISNULL(H.DiscountAmount, 0) * - 1 AS DiscountAmount
			,	ISNULL(H.SalesPrice, 0) + ISNULL(H.DiscountAmount, 0) * - 1 + H.ShopOptionAmount + H.SalesCostTotalAmount AS SalesTotalAmount
			,	H.CarName
			,	H.CarBrandName
			,	D.AreaCode
			,	C.CustomerType
			,	H.MakerOptionAmount
			,	cn.Name AS BrandStore
			,	CASE WHEN 
					C.CustomerType = '201' OR
					C.CustomerType = '202'
				THEN 
					'�Ɣ�'
				ELSE
					'���'
				END AS CustomerTypeName
					
			FROM 
				dbo.CarSalesHeader AS H WITH(INDEX([IDX_CarSalesHeader])) LEFT OUTER JOIN
				dbo.Department AS D ON H.DepartmentCode = D.DepartmentCode LEFT OUTER JOIN
				dbo.c_CodeName cn ON cn.CategoryCode = '002' AND D.BrandStoreCode = cn.Code LEFT OUTER JOIN
				dbo.Customer AS C ON H.CustomerCode = C.CustomerCode LEFT OUTER JOIN
				#temp_Option L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber
			WHERE 
				(H.DelFlag = '0') AND 
				(H.SalesOrderStatus = '005') AND 
				EXISTS
				(
					select 'x' from dbo.Employee E where E.EmployeeCode = H.EmployeeCode
				) AND 
				NOT EXISTS	--�����ȍ~�ɐԓ`�[�̂ݑ��݂���ꍇ�́A�̔����珜�O
				(
					select 'y' from #temp_AkaSlipList aka where LEFT(H.SlipNumber, LEN(H.SlipNumber)-2) = aka.SlipNumber
				) AND
				-- Mod 2018/11/07 yano
				NOT EXISTS�@--�����ԓd�E���`�[�͏��O
				(
					select 'z' from #temp_KuroSlipList kuro where H.SlipNumber = (kuro.SlipNumber + '-1') OR H.SlipNumber = (kuro.SlipNumber + '-2')
				) AND
				H.SalesDate >= @TargetDateFrom AND
				H.SalesDate < @TargetDateTo

			--Del 2018/11/07 yano ���`�[�͊܂܂Ȃ�
			--���`�[�݂̂�ǉ�
			--INSERT INTO #temp_CarSalesDetail
			--SELECT
			--	H.SalesDate
			--,	H.SlipNumber
			--,	H.SalesCarNumber
			--,	H.SalesType
			--,	H.Vin
			--,	C.CustomerCode
			--,	C.CustomerName
			--,	D.DepartmentCode
			--,	D.DepartmentName
			--,	H.SalesPrice
			--,	H.ShopOptionAmount - ISNULL(L.Amount, 0) AS ShopOptionAmount
			--,	H.SalesCostTotalAmount
			--,	ISNULL(H.DiscountAmount, 0) * - 1 AS DiscountAmount
			--,	ISNULL(H.SalesPrice, 0) + ISNULL(H.DiscountAmount, 0) * - 1 + H.ShopOptionAmount + H.SalesCostTotalAmount AS SalesTotalAmount
			--,	H.CarName
			--,	H.CarBrandName
			--,	D.AreaCode
			--,	C.CustomerType
			--,	H.MakerOptionAmount
			--,	cn.Name AS BrandStore
			--,	CASE WHEN 
			--		C.CustomerType = '201' OR
			--		C.CustomerType = '202'
			--	THEN 
			--		'�Ɣ�'
			--	ELSE
			--		'���'
			--	END AS CustomerTypeName
			--FROM 
			--	dbo.CarSalesHeader AS H WITH(INDEX([IDX_CarSalesHeader])) LEFT OUTER JOIN
			--	dbo.Department AS D ON H.DepartmentCode = D.DepartmentCode LEFT OUTER JOIN
			--	dbo.c_CodeName cn ON cn.CategoryCode = '002' AND D.BrandStoreCode = cn.Code LEFT OUTER JOIN
			--	dbo.Customer AS C ON H.CustomerCode = C.CustomerCode LEFT OUTER JOIN
			--	#temp_Option L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber
			--WHERE
			--	EXISTS
			--	(
			--		select 'x' from #temp_KuroSlipList kuro where H.SlipNumber = kuro.KuroSlipNumber
			--	)
			--	AND
			--	H.DelFlag = '0'
			
			DROP INDEX ix_temp_CarSalesDetail ON #temp_CarSalesDetail
			CREATE UNIQUE INDEX ix_temp_CarSalesDetail ON #temp_CarSalesDetail (SalesCarNumber)

			--for debug
			--PRINT '-���`�[�ǉ�'
			/***************************************************
			�����O���J�z
			****************************************************/
			--�O�����̎c����0�łȂ����̂��擾
			INSERT INTO 
				#temp_CarriedBalance
			SELECT
				 cs.SalesCarNumber
				,cs.BrandStore
				,cs.PurchaseDate
				,cs.PurchaseLocationCode
				,cs.PurchaseLocationName
				,cs.CarPurchaseType
				,cs.SupplierCode
				,cs.EndInventory
				,cs.RecycleAmount
				,P.FinancialAmount
				,cs.CarPurchaseTypeName
				,cs.SupplierName
				,cs.SelfRegistrationPurchaseDate
				,cs.MakerName
				,cs.CarName
			FROM
				#temp_CarStock cs LEFT JOIN
				(
					select 
						* 
					from 
						dbo.CarPurchase 
					where 
						PurchaseStatus = '002' and
						DelFlag = '0'
				) P ON  cs.SalesCarNumber = P.SalesCarNumber
			WHERE
				CONVERT(datetime, cs.ProcessDate) = @TargetMonthPrevious AND
				cs.EndInventory IS NOT NULL
					
			--for debug
			--PRINT '�O���J�z���X�g�쐬'

			--�O���ȑO�ɔ̔�����āA�����ԓ`�ƂȂ����ԗ��͕���������
			INSERT INTO 
				#temp_CarriedBalance
			SELECT
				 cs.SalesCarNumber
				,cs.BrandStore
				,cs.PurchaseDate
				,cs.PurchaseLocationCode
				,cs.PurchaseLocationName
				,cs.CarPurchaseType
				,cs.SupplierCode
				,cs.EndInventory
				,cs.RecycleAmount
				,cs.MonthPurchase
				,cs.CarPurchaseTypeName
				,cs.SupplierName
				,cs.SelfRegistrationPurchaseDate
				,cs.MakerName
				,cs.CarName
			FROM
				dbo.CarStock cs
			WHERE
			    EXISTS
				(
					select
						'x'
					from
					(
						select 
							 cs2.SalesCarNumber
							,Max(cs2.ProcessDate) AS ProcessDate
						from 
							dbo.CarStock cs2 
						where 
							exists 
							( 
								select 
									'y' 
								from 
									#temp_AkaSlipList tas 
								where
									cs2.SalesCarNumber = tas.SalesCarNumber
							)
						group by
							 cs2.SalesCarNumber
					) gcs
					where
						cs.ProcessDate = gcs.ProcessDate and
						cs.SalesCarNumber = gcs.SalesCarNumber
				)
			
			DROP INDEX  ix_temp_CarriedBalance ON #temp_CarriedBalance
			CREATE INDEX ix_temp_CarriedBalance ON #temp_CarriedBalance (SalesCarNumber)

			/*************************************************************
			�����R�[�h���X�g
			**************************************************************/
			--�O�c�E�����d��
			INSERT INTO #temp_CodeList
			SELECT
				l.SalesCarNumber
			FROM 
			(
				SELECT
					SalesCarNumber
				FROM
					#Temp_CarriedBalance			--�O���J�z
				UNION
				SELECT
					SalesCarNumber
				FROM
					#temp_CarPurchaseDetail			--�����d��
				UNION
				SELECT
					SalesCarNumber
				FROM
					#Temp_CarSalesDetail			--�����̔�
			) AS l

			DROP INDEX ix_temp_CodeList ON #temp_CodeList
			CREATE UNIQUE INDEX iX_temp_CodeList ON #temp_CodeList ([SalesCarNumber])

			--for debug
			--PRINT '�R�[�h���X�g�쐬'

			/*************************************************************
			�����ԗ���񃊃X�g
			**************************************************************/
			INSERT INTO
				#temp_SalesCar
			SELECT
				  S.SalesCarNumber
				, S.MakerName
				, S.NewUsedType
				, NU.Name AS NewUsedTypeName
				, CM.CarName + ' ' + CM.CarGradeName				--�Ԏ햼�{�ԗ��O���[�h�����Ԏ햼�Ƃ��ēo�^
				, S.CarGradeCode
				, S.Vin
				, S.CarUsage
				, S.CompanyRegistrationFlag
				--�u�����h�X�g�A
				,
				  CASE 
				  WHEN
					CM.MakerCode in ('CL', 'JP')
				  THEN
					'CJ'
				  WHEN
					CM.MakerCode in ('AB', 'AR', 'FI')
				  THEN
					'FA'
				  WHEN
					CM.MakerCode in ('JG', 'LR')
				  THEN
					'JLR'
				  ELSE
					NULL
				  END AS BrandStore
			FROM
				SalesCar S INNER JOIN
				dbo.V_CarMaster CM ON S.CarGradeCode = CM.CarGradeCode LEFT OUTER JOIN
				c_NewUsedType NU ON S.NewUsedType = NU.Code		--Add 2016/11/30 arc yano #3659
			WHERE
				exists
				(
					select 'x' from #Temp_CodeList tcl where tcl.SalesCarNumber = s.SalesCarNumber
				)
				 
			DROP INDEX ix_temp_SalesCar ON #temp_SalesCar
			CREATE INDEX ix_temp_SalesCar ON #temp_SalesCar (SalesCarNumber)

			--for debug
			--PRINT '�ԗ���񃊃X�g�쐬'

			/*************************************************************
			�����������I���X�g
			**************************************************************/
			INSERT INTO
				#temp_InventoryStockCar
			SELECT
				 SalesCarNumber AS SalesCarNumber
				,LocationCode AS LacationCode
			FROM
				dbo.InventoryStockCar
			WHERE
				InventoryMonth = @TargetDateFrom AND
				ISNULL(DelFlag, '0') <> '1' AND
				ISNULL(PhysicalQuantity, 0) = 1					--���I��1�̂���		
			
			DROP INDEX ix_temp_InventoryStockCar  ON #temp_InventoryStockCar
			CREATE INDEX ix_temp_InventoryStockCar ON #temp_InventoryStockCar (SalesCarNumber)

			--for debug
			--PRINT '�������I���X�g�쐬'
			/************************************************************
			�����ԗ��Ǘ����X�g
			*************************************************************/		
			--����w��ɂ��
			IF @ActionFlag = 0		--����w�� =�u�\���v�̏ꍇ�͈ꎞ�\�Ɋi�[
			BEGIN
				INSERT INTO
					#temp_CarStock
				SELECT
					--�Ώ۔N��--------------------------------------------------------------------------------------------------------------------------------------------
					@TargetDateFrom																AS ProcessDate
					--�Ǘ��ԍ�--------------------------------------------------------------------------------------------------------------------------------------------
					,cl.SalesCarNumber															AS SalesCarNumber
					--�戵�u�����h�R�[�h----------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN							--�O�c��񂪂���ꍇ�́A��������擾
						cb.BrandStore IS NOT NULL
					 THEN
						cb.BrandStore
					 WHEN
						cp.BrandStore IS NOT NULL AND
						cp.BrandStore <> '�{��' 
					 THEN
						cp.BrandStore
					 WHEN
						sc.BrandStore IS NOT NULL
					 THEN
						sc.BrandStore		--�ԗ��̃��[�J�[����ݒ�
					 ELSE
						cs.BrandStore		--�̔��X�܂̎戵�u�����h	
					 END																		AS BrandStore
					--�V���敪--------------------------------------------------------------------------------------------------------------------------------------------
					,sc.NewUsedType																AS NewUsedType
					--�d����----------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN							--�O�c��񂪂���ꍇ�́A��������擾
						cb.PurchaseDate IS NOT NULL
					 THEN
						cb.PurchaseDate
					 ELSE
						cp.PurchaseDate
					 END																		AS PurchaseDate
					--�Ԏ햼----------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cb.CarName IS NOT NULL AND cb.CarName <> ''
					 THEN
						cb.CarName
					 ElSE
						sc.CarName
					 END 																		AS CarName
					--�O���[�h�R�[�h--------------------------------------------------------------------------------------------------------------------------------------
					,sc.CarGradeCode															AS CarGradeCode
					--�ԑ�ԍ�--------------------------------------------------------------------------------------------------------------------------------------------
					,sc.Vin																		AS Vin
					--�d���E�݌ɋ��_�R�[�h--------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN							--�O�c��񂪂���ꍇ�͂�������擾
						cb.PurchaseLocationCode IS NOT NULL
					 THEN
						cb.PurchaseLocationCode
					 ELSE
						CASE WHEN
							ts.ArrivalLocationCode IS NOT NULL
						THEN
							ts.ArrivalLocationCode
						ELSE
							cp.PurchaseLocationCode													
						END
					 END																		AS PurchaseLocationCode
					--�d���敪--------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN							--�O�c��񂪂���ꍇ�́A��������擾
						cb.CarPurchaseType IS NOT NULL
					 THEN
						cb.CarPurchaseType
					 ELSE
						cp.CarPurchaseType
					 END																		AS CarPurchaseType
					--�d����R�[�h--------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN							--�O�c��񂪂���ꍇ�́A��������擾
						cb.SupplierCode IS NOT NULL
					 THEN
						cb.SupplierCode
					 ELSE
						cp.SupplierCode
					 END																		AS SupplierCode
					--�����݌�------------------------------------------------------------------------------------------------------------------------------------------------
					,cb.EndInventory															AS BeginningInventory
					--�����d��------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cb.EndInventory IS NOT NULL AND
						cb.EndInventory <> 0 AND
						cb.PurchaseAmount IS NOT NULL AND
						cb.EndInventory <> cb.PurchaseAmount
					 THEN
						cb.PurchaseAmount - cb.EndInventory									--�d�����z�|�O�c
					 ELSE
						cp.FinancialAmount
					 END																		AS MonthPurchase
					--�[�ԓ�---------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL
					 THEN
						SalesDate
					 ELSE
						NULL
					 END																		AS SalesDate
					--�`�[�ԍ�---------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005')											--���Гo���ȊO
					 THEN
						cs.SlipNumber
					 ELSE
						NULL
					 END																		AS SlipNumber
					--�̔��敪---------------------------------------------------------------------------------------------------------------------------------------------------
					,cs.SalesType																AS SalesType
					--�ڋq�R�[�h---------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')		--�f���J�[���ȊO
					 THEN
						cs.CustomerCode
					 ELSE
						NULL
					 END																		AS CustomerCode
					--�̔����i--------------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')		--�f���J�[���ȊO
					 THEN
						ISNULL(cs.SalesPrice, 0)
					 ELSE
						NULL
					 END																		AS SalesPrice
					 --�l�����i-------------------------------------------------------------------------------------------------------------------------------------------------------
					 ,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')		--�f���J�[���ȊO
					 THEN
						ISNULL(cs.DiscountAmount, 0)
					 ELSE
						NULL
					 END																		AS DiscountAmount			
					--�I�v�V�������i(�X�܁{���[�J�[)-----------------------------------------------------------------------------------------------------------------------------------
					 ,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')		--�f���J�[���ȊO
					 THEN
						ISNULL(cs.ShopOptionAmount, 0) + ISNULL(cs.MakerOptionAmount, 0)
					 ELSE
						NULL
					 END																		AS ShopOptionAmount
					--�̔�����p���v---------------------------------------------------------------------------------------------------------------------------------------------------
					 ,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')		--�f���J�[���ȊO
					 THEN
						ISNULL(cs.SalesCostTotalAmount, 0)
					 ELSE
						NULL
					 END																		AS SalesCostTotalAmount
					 --���㑍���v-------------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')		--�f���J�[���ȊO
					 THEN
						ISNULL(cs.SalesPrice, 0) + 
						ISNULL(cs.DiscountAmount, 0) + 
						ISNULL(cs.ShopOptionAmount, 0) + 
						ISNULL(cs.MakerOptionAmount, 0) + 
						ISNULL(cs.SalesCostTotalAmount, 0)
					 ELSE
						NULL
					 END																		AS SalesTotalAmount										
					--���㌴��----------------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')			--�f���J�[���ȊO
					 THEN
						ISNULL(cb.EndInventory, 0) +
						ISNULL(cp.FinancialAmount, 0) +
						ISNULL(cp.OtherAccount, 0)
					 ELSE
						NULL
					 END																		AS SalesCostAmount			
					 --�e����������Ōv�Z������------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS SalesProfits
					 --������U�ց�������Ōv�Z������-----------------------------------------------------------------------------------------------------------------------------------		
					 ,NULL																		AS ReductionTotal
					 --���Гo�^��������Ōv�Z������-------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS SelfRegistration
					 --���f�B�[���[��������Ōv�Z������---------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS OtherDealer
					 --�f���J�[��������Ōv�Z������-------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS DemoCar
					 --��ԁ�������Ōv�Z������-----------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS TemporaryCar
					 --�����݌Ɂ�������Ōv�Z������--------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS EndInventory
					 --���T�C�N������----------------------------------------------------------------------------------------------------------------------------------------------------
					 ,Case When
						cb.RecycleAmount IS NOT NULL
					  THEN
						cb.RecycleAmount
					  ELSE
						cp.RecycleAmount
					  END																		AS RecycleAmount
					 --��������----------------------------------------------------------------------------------------------------------------------------------------------------
					 ,cp.OtherAccount															AS OtherAccount
					 --�����^�J�[��������Ōv�Z������-------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS RentalCar
					 --�Ɩ��ԗ���������Ōv�Z������-------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS BusinessCar
					 --PR�ԁ�������Ōv�Z������-----------------------------------------------------------------------------------------------------------------------------------------
					 ,NULL																		AS PRCar
					 --�������I---------------------------------------------------------------------------------------------------------------------------------------------------------														
					 ,ivs.LocationCode															AS LocationCode
					--�d���L�����Z����������Ōv�Z������-----------------------------------------------------------------------------------------------------------------------------------
					 ,cpc.CancelAmount															AS CancelPurchas
					--�d���敪��----------------------------------------------------------------------------------------------------------------------------------------------------------															
					 ,CASE WHEN
						cb.CarPurchaseTypeName IS NOT NULL
					  THEN
						cb.CarPurchaseTypeName
					  ELSE
					    cp.CarPurchaseTypeName
					  END																		AS CarPurchaseTypeName
					--���[�J�[��----------------------------------------------------------------------------------------------------------------------------------------------------------
					 ,CASE WHEN
						cb.MakerName IS NOT NULL AND cb.MakerName <> ''
					  THEN
						cb.MakerName
					  ELSE
						sc.MakerName
					  END																		AS MakerName
					--�d���E�݌ɋ��_��----------------------------------------------------------------------------------------------------------------------------------------------------
					 ,CASE WHEN
					   cb.PurchaseLocationName IS NOT NULL
					  THEN
					   cb.PurchaseLocationName
					  WHEN
					   lc.LocationName IS NOT NULL
					  THEN
					   lc.LocationName
					  ELSE
					   cp.PurchaseLocationName
					  END																		AS PurchaseLcationName
					--�������I���P�[�V����-------------------------------------------------------------------------------------------------------------------------------------------------
					 ,Case WHEN
						lc2.LocationName IS NOT NULL
					  THEN
						lc2.LocationName
					  ELSE
						'-'
					  END																		AS InventoryLocationName
					--�d���於���O�c���̂݁B�������͌�œ���-------------------------------------------------------------------------------------------------------------------------------
					 ,cb.SupplierName															AS SupplierName
					--�̔��X�܃R�[�h-------------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')			--�f���J�[���ȊO
					 THEN
						cs.DepartmentCode
					 ELSE
						NULL
					 END																		AS SalesDepartmentCode
					--�̔��X�ܖ�-----------------------------------------------------------------------------------------------------------------------------------------------------------
					,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')			--�f���J�[���ȊO
					 THEN
						CASE WHEN
							cs.DepartmentCode = '021' AND										--���傪FiatGroup�ۂŔ̔��敪���Ɣ̂̏ꍇ
							cs.SalesType = '003'
						THEN
							'���ʋƔ�'
						ELSE
							cs.DepartmentName
						END
					 ELSE
						NULL
					 END																		AS SalesDepartmentName
					 --�V���敪��-------------------------------------------------------------------------------------------------------------------------------------------------------
					 ,sc.NewUsedTypeName														AS NewUsedTypeName
					  --�̔��ڋq��-------------------------------------------------------------------------------------------------------------------------------------------------------
					  ,CASE WHEN
							cs.SalesDate IS NOT NULL
					   THEN
							CASE
							WHEN
								cs.SalesType = '005'	--���Гo�^
							THEN
								'���Гo'
							WHEN
								cs.SalesType IN ('006', '010', '011', '012', '013')	--�f���J�[��
							THEN
								'�ЗL��'
							ELSE
							  cs.CustomerName
							END
							
					   ELSE
						 NULL
					   END																		AS CustomerName
					  --����ԓ`�[�ԍ�-----------------------------------------------------------------------------------------------------------------------------------------------------------
					  ,cp.TradeCarSlipNumber													AS TradeCarSlipNumber
					  --���Гo�^�O�̎ԗ��̎d����-------------------------------------------------------------------------------------------------------------------------------------------------
					  ,CASE WHEN
						cb.SelfRegistrationPurchaseDate IS NOT NULL
					   THEN
						cb.SelfRegistrationPurchaseDate
					   ELSE
						cp.SelfRegistrationPurchaseDate
					   END																		AS SelfRegistrationPurchaseDate
					   --�ڋq��ʖ�--------------------------------------------------------------------------------------------------------------------------------------------------------------
					  ,CASE WHEN
						cs.SalesDate IS NOT NULL AND
						cs.SalesType IS NOT NULL AND
						cs.SalesType NOT IN ('005', '006', '010', '011', '012', '013')			--�f���J�[���ȊO
					   THEN
					�@	cs.CustomerTypeName
					   ELSE
						NULL
					   END																		AS CustomerTypeName
				FROM
					#Temp_CodeList cl LEFT OUTER JOIN
					#Temp_CarriedBalance cb ON cl.SalesCarNumber = cb.SalesCarNumber LEFT OUTER JOIN
					#temp_CarPurchaseDetail cp ON cl.SalesCarNumber = cp.SalesCarNumber LEFT OUTER JOIN
					#temp_CarPurchaseCancel cpc ON cl.SalesCarNumber = cpc.SalesCarNumber LEFT OUTER JOIN
					#temp_CarSalesDetail cs ON cl.SalesCarNumber = cs.SalesCarNumber LEFT OUTER JOIN
					#temp_SalesCar sc ON cl.SalesCarNumber = sc.SalesCarNumber LEFT OUTER JOIN
					#temp_Transfer2 ts ON cl.SalesCarNumber = ts.SalesCarNumber LEFT OUTER JOIN
					#temp_InventoryStockCar ivs ON cl.SalesCarNumber = ivs.SalesCarNumber LEFT OUTER JOIN
					dbo.Location lc ON ts.ArrivalLocationCode = lc.LocationCode LEFT OUTER JOIN
					dbo.Location lc2 ON ivs.LocationCode = lc2.LocationCode --LEFT OUTER JOIN
					--dbo.Department dp ON cp.DepartmentCode = dp.DepartmentCode

				DROP INDEX ix_temp_CarStock ON #temp_CarStock
				CREATE UNIQUE INDEX ix_temp_CarStock ON #temp_CarStock (ProcessDate, SalesCarNumber)
				--for debug
				--PRINT '�ԗ��\�Ɉ�U�i�['

				/****************************************************
				-- �����W�v�E�ҏW����
				****************************************************/
				--�����W�v�P
				UPDATE
					#temp_CarStock
				SET
					--�e��
					 SalesProfits = (CASE WHEN SalesTotalAmount IS NOT NULL THEN SalesTotalAmount - SalesCostAmount ELSE NULL END)
					--���Гo�^
					,SelfRegistration = (CASE WHEN SalesType = '005' THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) ELSE NULL END)
					--�f���J�[
					,DemoCar = (CASE WHEN SalesType = '006' THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) ELSE NULL END)
					--���
					,TemporaryCar = (CASE WHEN SalesType = '011' THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) ELSE NULL END)
					--�����^�J�[
					,RentalCar = (CASE WHEN SalesType = '010' THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) ELSE NULL END)
					--�Ɩ��ԗ�
					,BusinessCar = (CASE WHEN SalesType = '013' THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) ELSE NULL END)
					--�L���
					,PRCar = (CASE WHEN SalesType = '012' THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) ELSE NULL END)
					--�d���L�����Z��
					,CancelPurchase = CASE WHEN CancelPurchase IS NOT NULL THEN ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) + ISNULL(OtherAccount, 0) ELSE NULL END
				WHERE
					ProcessDate = @TargetDateFrom
		
				--for debug
				--PRINT '�W�v�P'

				--�����e���ڂ��v�Z(���̂Q)
				UPDATE
					#temp_CarStock
				SET
					--������U��
					ReductionTotal = CASE WHEN
										DemoCar IS NULL AND 
										TemporaryCar IS NULL AND 
										RentalCar IS NULL AND 
										BusinessCar IS NULL AND 
										PRCar IS NULL 
									THEN 
										NULL 
									ELSE
										ISNULL(DemoCar, 0) + 
										ISNULL(TemporaryCar, 0) + 
										ISNULL(RentalCar, 0) + 
										ISNULL(BusinessCar, 0) + 
										ISNULL(PRCar, 0)
									END
				WHERE
					ProcessDate = @TargetDateFrom
				--for debug
				--PRINT '�W�v�Q'

				--�����e���ڂ��v�Z(���̂R)
				UPDATE
					#temp_CarStock
				SET
					--�����݌�
					EndInventory = CASE WHEN 
										SalesTotalAmount IS NULL AND
										ReductionTotal IS NULL AND
										SelfRegistration IS NULL AND
										CancelPurchase IS NULL
									THEN 
										ISNULL(BeginningInventory, 0) + ISNULL(MonthPurchase, 0) + ISNULL(OtherAccount, 0)
									ELSE
										NULL
									END
				WHERE
					ProcessDate = @TargetDateFrom
				--for debug
				--PRINT '�W�v�R'

				--�������̑��X�V
				--�d���於�̂̍X�V
				UPDATE
					#temp_CarStock
				SET
					SupplierName = 
						--�d���於
						CASE  
						WHEN 
							cs.NewUsedType = 'N' AND 
							cs.SupplierCode IN ('9000000001', '9000000002', '002001')
						THEN
							'(' + FORMAT(cs.PurchaseDate, 'yyyyMM') + 'JACCS)'
						WHEN
							cs.NewUsedType = 'N' AND
			�@				cs.SupplierCode IN ('KK00000770', 'KK00000843')
						THEN
							'(' + FORMAT(cs.PurchaseDate, 'yyyyMM') + 'GL�R�l�N�g)'
						WHEN
							cs.CarPurchaseTypeName = '�����(���)'
						THEN
							s.SupplierName + ' ' + cs.TradeCarSlipNumber
						ELSE
							s.SupplierName
						END
				FROM
					#temp_CarStock cs LEFT OUTER JOIN
					dbo.Supplier s ON cs.SupplierCode = s.SupplierCode
				WHERE
					cs.ProcessDate = @TargetDateFrom AND
					cs.SupplierName is null

				--���������̏ꍇ�̓��T�C�N��������NULL
				UPDATE
					#temp_CarStock
				SET
					RecycleAmount = NULL
				WHERE
					ProcessDate = @TargetDateFrom AND
					OtherAccount IS NOT NULL

					END
					--���̏����Ώی�
					SET @TargetMonthCount = @TargetMonthCount - 1				--�c�����f�N�������g
					SET @TargetMonthPrevious = @TargetDateFrom					--�Ώی��O���C���N�������g(������̓����j
					SET @TargetDateFrom = DATEADD(m, 1, @TargetMonthPrevious)	--�Ώۓ�From�C���N�������g(������̑O���{�P�j
					SET @TargetDateTo = DATEADD(m, 1, @TargetDateFrom)			--�Ώۓ�To�C���N�������g(������̓����{�P�j
					IF @TargetDateTo > @NOW
						SET @TargetDateTo = @NOW
		--���[�v�G���h
		END

		/***************************************************/
		/*����w��=�u�\���v�̏ꍇ�̓f�[�^�擾���s��		   */
		/***************************************************/
		DECLARE @SQL NVARCHAR(MAX) = ''
		DECLARE @PARAM NVARCHAR(1024) = ''
		DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)		

		IF @ActionFlag = 0	--�\���̏ꍇ
		BEGIN
			SET @PARAM = '@DataType nvarchar(3), @NewUsedType nvarchar(3), @SalesCarNumber NVARCHAR(50), @Vin NVARCHAR(3)'
					
			SET @SQL = ''
			SET @SQL = @SQL +'SELECT' + @CRLF
			SET @SQL = @SQL +'    cs.ProcessDate' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesCarNumber' + @CRLF
			SET @SQL = @SQL +'	, cs.BrandStore' + @CRLF
			SET @SQL = @SQL +'	, cs.NewUsedType' + @CRLF
			SET @SQL = @SQL +'	, cs.PurchaseDate' + @CRLF
			SET @SQL = @SQL +'	, cs.CarName' + @CRLF
			SET @SQL = @SQL +'	, cs.CarGradeCode' + @CRLF
			SET @SQL = @SQL +'	, cs.Vin' + @CRLF
			SET @SQL = @SQL +'	, cs.PurchaseLocationCode' + @CRLF
			SET @SQL = @SQL +'	, cs.CarPurchaseType' + @CRLF
			SET @SQL = @SQL +'	, cs.SupplierCode' + @CRLF
			SET @SQL = @SQL +'	, cs.BeginningInventory' + @CRLF
			SET @SQL = @SQL +'	, cs.MonthPurchase' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesDate' + @CRLF
			SET @SQL = @SQL +'	, cs.SlipNumber' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesType' + @CRLF
			SET @SQL = @SQL +'	, cs.CustomerCode' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesPrice' + @CRLF
			SET @SQL = @SQL +'	, cs.DiscountAmount' + @CRLF
			SET @SQL = @SQL +'	, cs.ShopOptionAmount' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesCostTotalAmount' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesTotalAmount' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesCostAmount' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesProfits' + @CRLF
			SET @SQL = @SQL +'	, cs.ReductionTotal' + @CRLF
			SET @SQL = @SQL +'	, cs.SelfRegistration' + @CRLF
			SET @SQL = @SQL +'	, cs.OtherDealer' + @CRLF
			SET @SQL = @SQL +'	, cs.DemoCar' + @CRLF
			SET @SQL = @SQL +'	, cs.TemporaryCar' + @CRLF
			SET @SQL = @SQL +'	, cs.EndInventory' + @CRLF
			SET @SQL = @SQL +'	, '''' AS CreateEmployeeCode' + @CRLF
			SET @SQL = @SQL +'	, NULL AS CreateDate' + @CRLF
			SET @SQL = @SQL +'	, '''' AS LastUpdateEmployeeCode' + @CRLF
			SET @SQL = @SQL +'	, NULL AS LastUpdateDate' + @CRLF
			SET @SQL = @SQL +'	, ''0'' AS DelFlag' + @CRLF
			SET @SQL = @SQL +'	, cs.RecycleAmount' + @CRLF
			SET @SQL = @SQL +'	, cs.OtherAccount' + @CRLF
			SET @SQL = @SQL +'	, cs.RentalCar' + @CRLF
			SET @SQL = @SQL +'	, cs.BusinessCar' + @CRLF
			SET @SQL = @SQL +'	, cs.PRCar' + @CRLF
			SET @SQL = @SQL +'	, cs.LocationCode' + @CRLF
			SET @SQL = @SQL +'	, cs.CancelPurchase' + @CRLF
			SET @SQL = @SQL +'	, cs.CarPurchaseTypeName' + @CRLF
			SET @SQL = @SQL +'	, cs.MakerName' + @CRLF
			SET @SQL = @SQL +'	, cs.PurchaseLocationName' + @CRLF
			SET @SQL = @SQL +'	, cs.InventoryLocationName' + @CRLF
			SET @SQL = @SQL +'	, cs.SupplierName' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesDepartmentCode' + @CRLF
			SET @SQL = @SQL +'	, cs.SalesDepartmentName' + @CRLF
			SET @SQL = @SQL +'	, cs.NewUsedTypeName' + @CRLF
			SET @SQL = @SQL +'	, cs.CustomerName' + @CRLF
			SET @SQL = @SQL +'	, cs.SelfRegistrationPurchaseDate' + @CRLF
			SET @SQL = @SQL +'	, cs.CustomerTypeName' + @CRLF
			SET @SQL = @SQL +'FROM' + @CRLF
			SET @SQL = @SQL +'	#temp_CarStock cs' + @CRLF
			SET @SQL = @SQL +'WHERE' + @CRLF
			SET @SQL = @SQL +'    cs.ProcessDate = CONVERT(datetime, ''' + FORMAT(@TargetMonth, 'yyyyMMdd') + ''')' + @CRLF 
					
			--�f�[�^���
			IF ((@DataType is not null) AND (@DataType = '001'))	--�݌ɂ̏ꍇ
			BEGIN
				SET @SQL = @SQL +' AND cs.EndInventory is not null' + @CRLF
			END
			ELSE IF((@DataType is not null) AND (@DataType = '002'))	--�̔��̏ꍇ
			BEGIN
				SET @SQL = @SQL +' AND cs.EndInventory is null' + @CRLF
			END
			ELSE IF((@DataType is not null) AND (@DataType = '004'))	--�d���̏ꍇ
			BEGIN
				SET @SQL = @SQL +' AND cs.PurchaseDate >= CONVERT(datetime, ''' +  FORMAT(@TargetMonth, 'yyyy/MM/dd') + ''')'+ @CRLF
				SET @SQL = @SQL +' AND cs.PurchaseDate < CONVERT(datetime, ''' +  FORMAT(@TargetMonthTo, 'yyyy/MM/dd') + ''')'+ @CRLF
			END
			--�V���敪
			IF ((@NewUsedType is not null) AND (@NewUsedType <> ''))	
			BEGIN
				SET @SQL = @SQL +' AND cs.NewUsedType = ''' + @NewUsedType + '''' + @CRLF
			END
			--�d����
			IF ((@Suppliercode is not null) AND (@Suppliercode <> ''))	
			BEGIN
				SET @SQL = @SQL +' AND cs.Suppliercode = ''' + @Suppliercode + '''' + @CRLF
			END
			--�Ǘ��ԍ�
			IF ((@SalesCarNumber is not null) AND (@SalesCarNumber <> ''))
			BEGIN
				SET @SQL = @SQL +' AND cs.SalesCarNumber = ''' + @SalesCarNumber + '''' + @CRLF
			END
			--�ԑ�ԍ�
			IF ((@Vin is not null) AND (@Vin <> ''))
			BEGIN
				SET @SQL = @SQL +' AND cs.Vin = ''' + @Vin + '''' + @CRLF
			END

			SET @SQL = @SQL +'ORDER BY cs.PurchaseDate, cs.Vin' + @CRLF

			EXECUTE sp_executeSQL @SQL, @PARAM, @DataType, @NewUsedType, @SalesCarNumber, @Vin
		END
		ELSE		--����w�肪�u�X�i�b�v�V���b�g�ۑ��v�̏ꍇ
		BEGIN
			--�Ώ۔N���̃f�[�^��dbo.CarStock��INSERT
			INSERT INTO
				dbo.CarStock
			SELECT
			   cs.[ProcessDate]
			  ,cs.[SalesCarNumber]
			  ,cs.[BrandStore]
			  ,cs.[NewUsedType]
			  ,cs.[PurchaseDate]
			  ,cs.[CarName]
			  ,cs.[CarGradeCode]
			  ,cs.[Vin]
			  ,cs.[PurchaseLocationCode]
			  ,cs.[CarPurchaseType]
			  ,cs.[SupplierCode]
			  ,cs.[BeginningInventory]
			  ,cs.[MonthPurchase]
			  ,cs.[SalesDate]
			  ,cs.[SlipNumber]
			  ,cs.[SalesType]
			  ,cs.[CustomerCode]
			  ,cs.[SalesPrice]
			  ,cs.[DiscountAmount]
			  ,cs.[ShopOptionAmount]
			  ,cs.[SalesCostTotalAmount]
			  ,cs.[SalesTotalAmount]
			  ,cs.[SalesCostAmount]
			  ,cs.[SalesProfits]
			  ,cs.[ReductionTotal]
			  ,cs.[SelfRegistration]
			  ,cs.[OtherDealer]
			  ,cs.[DemoCar]
			  ,cs.[TemporaryCar]
			  ,cs.[EndInventory]
			  ,@EmployeeCode
			  ,GETDATE()
			  ,@EmployeeCode
			  ,GETDATE()
			  ,'0'
			  ,cs.[RecycleAmount]
			  ,cs.[OtherAccount]
			  ,cs.[RentalCar]
			  ,cs.[BusinessCar]
			  ,cs.[PRCar]
			  ,cs.[LocationCode]
			  ,cs.[CancelPurchase]
			  ,cs.[CarPurchaseTypeName]
			  ,cs.[MakerName]
			  ,cs.[PurchaseLocationName]
			  ,cs.[InventoryLocationName]
			  ,cs.[SupplierName]
			  ,cs.[SalesDepartmentCode]
			  ,cs.[SalesDepartmentName]
			  ,cs.[NewUsedTypeName]
			  ,cs.[CustomerName]
			  ,cs.[SelfRegistrationPurchaseDate]
			FROM
				#temp_CarStock cs
			WHERE
				cs.ProcessDate = @TargetMonthFrom
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


