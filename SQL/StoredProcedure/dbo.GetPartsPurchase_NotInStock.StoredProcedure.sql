USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsPurchase_NotInStock]    Script Date: 2018/04/02 11:39:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




--2018/03/26 arc yano #3863 ���i���ׁ@LinkEntry�捞���̒ǉ�
--2015/11/11 arc nakayama #3292_���i���׈ꗗ �V�K�쐬�@���׈ꗗ��ʂœ��׃X�e�[�^�X���u�����ׁv�Ō����������̌���


CREATE PROCEDURE [dbo].[GetPartsPurchase_NotInStock]

	@PurchaseNumberFrom nvarchar(50),		--���ד`�[�ԍ�From
	@PurchaseNumberTo nvarchar(50),			--���ד`�[�ԍ�To
	@PurchaseOrderNumberFrom nvarchar(50),	--�����`�[�ԍ�From
	@PurchaseOrderNumberTo nvarchar(50),	--�����`�[�ԍ�To
	@PurchaseOrderDateFrom nvarchar(10),	--������From
	@PurchaseOrderDateTo nvarchar(10),		--������To
	@SlipNumberFrom nvarchar(50),			--�󒍓`�[�ԍ�From
	@SlipNumberTo nvarchar(50),				--�󒍓`�[�ԍ�To
	@OrderType nvarchar(3),					--�����敪
	@CustomerCode nvarchar(10),				--�ڋq�R�[�h
	@PartsNumber nvarchar(25),				--���i�ԍ�
	@PurchasePlanDateFrom nvarchar(10),		--���ח\���From
	@PurchasePlanDateTo nvarchar(10),		--���ח\���To
	@DepartmentCode nvarchar(3),			--����R�[�h
	@EmployeeCode nvarchar(50),				--�Ј��R�[�h(�T�[�r�X�t�����g)
	@SupplierCode nvarchar(10),				--�d����R�[�h
	@WebOrderNumber nvarchar(50),			--WEB�I�[�_�[�ԍ�
	@MakerOrderNumber nvarchar(50),			--���[�J�[�I�[�_�[�ԍ�
	@InvoiceNo nvarchar(50),				--�C���{�C�X�ԍ�
	@LinkEntryCaptureDateFrom nvarchar(10),	--�捞��From	--Add 2018/03/26 arc yano #3863
	@LinkEntryCaptureDateTo nvarchar(10)	--�捞��To		--Add 2018/03/26 arc yano #3863

AS

BEGIN
	BEGIN TRY
		--temp�e�[�u���폜
		DROP TABLE #temp_ServiceSalesHeader
		DROP TABLE #temp_PartsPurchaseOrder
		DROP TABLE #temp_PartsPurchase
		DROP TABLE #temp_PartsPurchase_NotOrder
		DROP TABLE #temp_RemainingQuantity
		DROP TABLE #temp_StockPartsPurchase
		DROP TABLE #temp_NotStockPartsPurchase
		DROP TABLE #temp_PartsPurchase_ChangeParts
		DROP TABLE #temp_PartsPurchase_NotExistsOrder
	END TRY
	BEGIN CATCH
		--����
	END CATCH

	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; --�_�[�e�B�[���[�h�ݒ�

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)


	--�捞��to
	IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
	BEGIN
		SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
	END
	/*-------------------------------------------*/
	/* �T�[�r�X�`�[�̏��擾					 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_ServiceSalesHeader (
		 SlipNumber nvarchar(50)
	   , FrontEmployeeCode nvarchar(50)
	   , CustomerCode nvarchar(10)

		)

		SET @PARAM = '@SlipNumberFrom nvarchar(50), @SlipNumberTo nvarchar(50), @EmployeeCode nvarchar(50), @CustomerCode nvarchar(10)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_ServiceSalesHeader' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '	 h.SlipNumber' + @CRLF
		SET @SQL = @SQL + ', h.FrontEmployeeCode' + @CRLF
		SET @SQL = @SQL + ', h.CustomerCode' + @CRLF

		SET @SQL = @SQL + 'FROM dbo.ServiceSalesHeader h' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '      h.DelFlag = ''0''' + @CRLF
				
		--�`�[�ԍ�
		IF (@SlipNumberFrom is not null) AND (@SlipNumberFrom <> '')
			IF (@SlipNumberTo is not null) AND (@SlipNumberTo <> '')
			BEGIN
				SET @SQL = @SQL +'AND h.SlipNumber >= @SlipNumberFrom AND h.SlipNumber <= @SlipNumberTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND h.SlipNumber = @SlipNumberFrom' + @CRLF 
			END
		ELSE
			IF ((@SlipNumberTo is not null) AND (@SlipNumberTo <> ''))
			BEGIN
				SET @SQL = @SQL +'AND h.SlipNumber <= @SlipNumberTo' + @CRLF 
			END		

		--�t�����g�S����
		IF ((@EmployeeCode IS NOT NULL) AND (@EmployeeCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND h.FrontEmployeeCode = @EmployeeCode'+ @CRLF
		END

		--�ڋq�R�[�h
		IF ((@CustomerCode IS NOT NULL) AND (@CustomerCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND h.CustomerCode = @CustomerCode'+ @CRLF
		END


		EXECUTE sp_executeSQL @SQL, @PARAM, @SlipNumberFrom, @SlipNumberTo, @EmployeeCode, @CustomerCode
		CREATE INDEX ix_temp_ServiceSalesHeader ON #temp_ServiceSalesHeader(SlipNumber)

	/*-------------------------------------------------------------------*/
	/* �������擾�iPartsPurchaseOrder�j(�����c�����郌�R�[�h�̂�)		 */
	/*-------------------------------------------------------------------*/

	CREATE TABLE #temp_PartsPurchaseOrder (
	     PurchaseOrderNumber nvarchar(50)	--�����`�[�ԍ�
	   , PurchaseOrderDate datetime			--������
	   , ServiceSlipNumber nvarchar(50)		--�T�[�r�X�`�[�ԍ�
	   , PartsNumber nvarchar(25)			--���i�ԍ�
	   , Quantity decimal(10,2)				--������
	   , OrderType nvarchar(3)				--�����敪
	   , WebOrderNumber nvarchar(50)		--WEB�I�[�_�[�ԍ�
	   , DepartmentCode nvarchar(3)			--����R�[�h
	   , SupplierCode nvarchar(10)			--�d����R�[�h
	   , RemainingQuantity decimal(10,2)	--�����c
		)

		SET @PARAM = '@PurchaseOrderNumberFrom nvarchar(50), @PurchaseOrderNumberTo nvarchar(50), @PurchaseOrderDateFrom nvarchar(10), @PurchaseOrderDateTo nvarchar(10), @SlipNumberFrom nvarchar(50), @SlipNumberTo nvarchar(50), @OrderType nvarchar(3), @DepartmentCode nvarchar(3), @WebOrderNumber nvarchar(50), @SupplierCode nvarchar(10), @CustomerCode nvarchar(10), @EmployeeCode nvarchar(50), @PartsNumber nvarchar(25)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchaseOrder' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PPO.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PurchaseOrderDate' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.Quantity' + @CRLF
		SET @SQL = @SQL + ',PPO.OrderType' + @CRLF
		SET @SQL = @SQL + ',PPO.WebOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',PPO.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PPO.RemainingQuantity' + @CRLF
		SET @SQL = @SQL + 'FROM dbo.PartsPurchaseOrder AS PPO' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '       PPO.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '   AND PPO.RemainingQuantity > 0' + @CRLF

		--�����`�[�ԍ�
		IF (@PurchaseOrderNumberFrom is not null) AND (@PurchaseOrderNumberFrom <> '')
			IF (@PurchaseOrderNumberTo is not null) AND (@PurchaseOrderNumberTo <> '')
			BEGIN
				SET @SQL = @SQL +'AND PPO.PurchaseOrderNumber >= @PurchaseOrderNumberFrom AND PPO.PurchaseOrderNumber <= @PurchaseOrderNumberTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PPO.PurchaseOrderNumber = @PurchaseOrderNumberFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchaseOrderNumberTo is not null) AND (@PurchaseOrderNumberTo <> ''))
			BEGIN
				SET @SQL = @SQL +'AND PPO.PurchaseOrderNumber <= @PurchaseOrderNumberTo' + @CRLF 
			END

		--������
		IF ((@PurchaseOrderDateFrom is not null) AND (@PurchaseOrderDateFrom <> '') AND ISDATE(@PurchaseOrderDateFrom) = 1)
			IF ((@PurchaseOrderDateTo is not null) AND (@PurchaseOrderDateTo <> '') AND ISDATE(@PurchaseOrderDateTo) = 1)
			BEGIN
				SET @PurchaseOrderDateTo = DateAdd(d, 1, @PurchaseOrderDateTo)
				SET @SQL = @SQL +'AND PPO.PurchaseOrderDate >= @PurchaseOrderDateFrom AND PPO.PurchaseOrderDate < @PurchaseOrderDateTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PPO.PurchaseOrderDate = @PurchaseOrderDateFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchaseOrderDateTo is not null) AND (@PurchaseOrderDateTo <> '') AND ISDATE(@PurchaseOrderDateTo) = 1)
			BEGIN
				SET @PurchaseOrderDateTo = DateAdd(d, 1, @PurchaseOrderDateTo)
				SET @SQL = @SQL +'AND PPO.PurchaseOrderDate < @PurchaseOrderDateTo' + @CRLF 
			END

		--�󒍓`�[�ԍ�
		IF (@SlipNumberFrom is not null) AND (@SlipNumberFrom <> '')
			IF (@SlipNumberTo is not null) AND (@SlipNumberTo <> '')
			BEGIN
				SET @SQL = @SQL +'AND PPO.ServiceSlipNumber >= @SlipNumberFrom AND PPO.ServiceSlipNumber <= @SlipNumberTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PPO.ServiceSlipNumber = @SlipNumberFrom' + @CRLF 
			END
		ELSE
			IF ((@SlipNumberTo is not null) AND (@SlipNumberTo <> ''))
			BEGIN
				SET @SQL = @SQL +'AND PPO.ServiceSlipNumber <= @SlipNumberTo' + @CRLF 
			END

		--���i�ԍ�
		IF ((@PartsNumber IS NOT NULL) AND (@PartsNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PPO.PartsNumber = @PartsNumber'+ @CRLF
		END

		--�����敪
		IF ((@OrderType IS NOT NULL) AND (@OrderType <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PPO.OrderType = @OrderType'+ @CRLF
		END

		--WEB�I�[�_�[�ԍ�
		IF ((@WebOrderNumber IS NOT NULL) AND (@WebOrderNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PPO.WebOrderNumber = @WebOrderNumber'+ @CRLF
		END

		--����
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PPO.DepartmentCode = @DepartmentCode'+ @CRLF
		END

		--�d����
		IF ((@SupplierCode IS NOT NULL) AND (@SupplierCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PPO.SupplierCode = @SupplierCode'+ @CRLF
		END		
		
		--�t�����g�S����
		IF ((@EmployeeCode IS NOT NULL) AND (@EmployeeCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND EXISTS(SELECT 1 FROM #temp_ServiceSalesHeader h WHERE h.FrontEmployeeCode = @EmployeeCode AND h.SlipNumber = PPO.ServiceSlipNumber)'+ @CRLF
		END

		--�ڋq�R�[�h
		IF ((@CustomerCode IS NOT NULL) AND (@CustomerCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND EXISTS(SELECT 1 FROM #temp_ServiceSalesHeader h WHERE h.CustomerCode = @CustomerCode AND h.SlipNumber = PPO.ServiceSlipNumber)'+ @CRLF
		END				

		EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseOrderNumberFrom, @PurchaseOrderNumberTo, @PurchaseOrderDateFrom, @PurchaseOrderDateTo, @SlipNumberFrom, @SlipNumberTo, @OrderType, @DepartmentCode, @WebOrderNumber, @SupplierCode, @CustomerCode, @EmployeeCode, @PartsNumber
		CREATE INDEX ix_temp_PartsPurchaseOrder ON #temp_PartsPurchaseOrder(PurchaseOrderNumber, PartsNumber, ServiceSlipNumber)


	/*-------------------------------------------*/
	/* ���׏��擾�i�����`�[�ԍ���������́j	 */
	/*-------------------------------------------*/


	CREATE TABLE #temp_PartsPurchase (
		 PurchaseNumber nvarchar(50)		--���ד`�[�ԍ�
	   , PurchaseOrderNumber nvarchar(50)	--�����`�[�ԍ�
	   , Quantity decimal(10,2)				--���ח\�萔
	   , PurchasePlanDate datetime			--���ח\���
	   , MakerOrderNumber nvarchar(50)		--���[�J�[�I�[�_�[�ԍ�
	   , InvoiceNo nvarchar(50)				--�C���{�C�X�ԍ�
	   , PartsNumber nvarchar(25)			--���i�ԍ�
	   , DepartmentCode nvarchar(3)			--����R�[�h
	   , SupplierCode nvarchar(10)			--�d����R�[�h
	   , PurchaseStatus nvarchar(3)			--�d���X�e�[�^�X
	   , ServiceSlipNumber nvarchar(50)		--�󒍓`�[�ԍ�
	   , ChangePartsFlag nvarchar(2)		--��֕��i�t���O
	   , LinkEntryCaptureDate datetime		--�捞�� --Add 2018/03/26 arc yano #3863
		)

		SET @PARAM = '@PurchaseNumberFrom nvarchar(50), @PurchaseNumberTo nvarchar(50), @PurchaseOrderNumberFrom nvarchar(50), @PurchaseOrderNumberTo nvarchar(50), @PurchasePlanDateFrom nvarchar(10), @PurchasePlanDateTo nvarchar(10), @MakerOrderNumber nvarchar(50), @InvoiceNo nvarchar(50), @SlipNumberFrom nvarchar(50), @SlipNumberTo nvarchar(50), @SupplierCode nvarchar(10), @DepartmentCode nvarchar(3), @CustomerCode nvarchar(10), @EmployeeCode nvarchar(50), @PartsNumber nvarchar(25)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchase' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.Quantity' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PP.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',PP.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseStatus' + @CRLF
		SET @SQL = @SQL + ',PP.ServiceSlipNumber' + @CRLF
		SET @SQL = @SQL + ',PP.ChangePartsFlag' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863
		SET @SQL = @SQL + 'FROM dbo.PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + ' WHERE PP.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF
		SET @SQL = @SQL + '   AND (PP.PurchaseOrderNumber IS NOT NULL OR PP.PurchaseOrderNumber <> '''')' + @CRLF

		--���ד`�[�ԍ�
		IF (@PurchaseNumberFrom is not null) AND (@PurchaseNumberFrom <> '')
			IF (@PurchaseNumberTo is not null) AND (@PurchaseNumberTo <> '')
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseNumber >= @PurchaseNumberFrom AND PP.PurchaseNumber <= @PurchaseNumberTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseNumber = @PurchaseNumberFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchaseNumberTo is not null) AND (@PurchaseNumberTo <> ''))
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseNumber <= @PurchaseNumberTo' + @CRLF 
			END

		--���ח\���
		IF ((@PurchasePlanDateFrom is not null) AND (@PurchasePlanDateFrom <> '') AND ISDATE(@PurchasePlanDateFrom) = 1)
			IF ((@PurchasePlanDateTo is not null) AND (@PurchasePlanDateTo <> '') AND ISDATE(@PurchasePlanDateTo) = 1)
			BEGIN
				SET @PurchasePlanDateTo = DateAdd(d, 1, @PurchasePlanDateTo)
				SET @SQL = @SQL +'AND PP.PurchasePlanDate >= @PurchasePlanDateFrom AND PP.PurchasePlanDate < @PurchasePlanDateTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchasePlanDate = @PurchasePlanDateFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchasePlanDateTo is not null) AND (@PurchasePlanDateTo <> '') AND ISDATE(@PurchasePlanDateTo) = 1)
			BEGIN
				SET @PurchasePlanDateTo = DateAdd(d, 1, @PurchasePlanDateTo)
				SET @SQL = @SQL +'AND PP.PurchasePlanDate < @PurchasePlanDateTo' + @CRLF 
			END

		--���[�J�[�I�[�_�[�ԍ�
		IF ((@MakerOrderNumber IS NOT NULL) AND (@MakerOrderNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.MakerOrderNumber = @MakerOrderNumber'+ @CRLF
		END		
			
		--�C���{�C�X�ԍ�
		IF ((@InvoiceNo IS NOT NULL) AND (@InvoiceNo <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.InvoiceNo = @InvoiceNo'+ @CRLF
		END

		--���i�ԍ�
		IF ((@PartsNumber IS NOT NULL) AND (@PartsNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.PartsNumber = @PartsNumber'+ @CRLF
		END
		
		--����
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.DepartmentCode = @DepartmentCode'+ @CRLF
		END

		--�d����
		IF ((@SupplierCode IS NOT NULL) AND (@SupplierCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.SupplierCode = @SupplierCode'+ @CRLF
		END

		EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseNumberFrom, @PurchaseNumberTo, @PurchaseOrderNumberFrom, @PurchaseOrderNumberTo, @PurchasePlanDateFrom, @PurchasePlanDateTo, @MakerOrderNumber, @InvoiceNo, @SlipNumberFrom, @SlipNumberTo, @SupplierCode, @DepartmentCode, @CustomerCode, @EmployeeCode, @PartsNumber
		CREATE INDEX ix_temp_PartsPurchase ON #temp_PartsPurchase(PurchaseOrderNumber, PartsNumber)



	/*-------------------------------------------------*/
	/* �����ƈႤ���i�����ׂ���ꍇ�̓��׏��擾	   */
	/*-------------------------------------------------*/

		CREATE TABLE #temp_PartsPurchase_ChangeParts (
		 PurchaseNumber nvarchar(50)		--���ד`�[�ԍ�
	   , PurchaseOrderNumber nvarchar(50)	--�����`�[�ԍ�
	   , Quantity decimal(10,2)				--���ח\�萔
	   , PurchasePlanDate datetime			--���ח\���
	   , MakerOrderNumber nvarchar(50)		--���[�J�[�I�[�_�[�ԍ�
	   , InvoiceNo nvarchar(50)				--�C���{�C�X�ԍ�
	   , PartsNumber nvarchar(25)			--���i�ԍ�
	   , DepartmentCode nvarchar(3)			--����R�[�h
	   , SupplierCode nvarchar(10)			--�d����R�[�h
	   , PurchaseStatus nvarchar(3)			--�d���X�e�[�^�X
	   , ServiceSlipNumber nvarchar(50)		--�󒍓`�[�ԍ�
	   , LinkEntryCaptureDate datetime		--�捞�� --Add 2018/03/26 arc yano #3863
		)

		SET @PARAM = ''
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchase_ChangeParts' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.Quantity' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PP.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',PP.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseStatus' + @CRLF
		SET @SQL = @SQL + ',PP.ServiceSlipNumber' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863
		SET @SQL = @SQL + 'FROM #temp_PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + 'WHERE PP.ChangePartsFlag = ''1''' + @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM
		CREATE INDEX ix_temp_PartsPurchase_ChangeParts ON #temp_PartsPurchase_ChangeParts(PurchaseOrderNumber)


	/*-------------------------------------------*/
	/* ���׏��擾�i�����`�[�ԍ����Ȃ����́j	 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_PartsPurchase_NotOrder (
		 PurchaseNumber nvarchar(50)		--���ד`�[�ԍ�
	   , PurchaseOrderNumber nvarchar(50)	--�����`�[�ԍ�
	   , Quantity decimal(10,2)				--���ח\�萔
	   , PurchasePlanDate datetime			--���ח\���
	   , MakerOrderNumber nvarchar(50)		--���[�J�[�I�[�_�[�ԍ�
	   , InvoiceNo nvarchar(50)				--�C���{�C�X�ԍ�
	   , PartsNumber nvarchar(25)			-- ���i�ԍ�
	   , DepartmentCode nvarchar(3)			--����R�[�h
	   , SupplierCode nvarchar(10)			--�d����R�[�h
	   , ServiceSlipNumber nvarchar(50)		--�󒍓`�[�ԍ�
	   , LinkEntryCaptureDate datetime		--�捞�� --Add 2018/03/26 arc yano #3863
		)

		SET @PARAM = '@PurchaseNumberFrom nvarchar(50), @PurchaseNumberTo nvarchar(50), @PurchaseOrderNumberFrom nvarchar(50), @PurchaseOrderNumberTo nvarchar(50), @PurchasePlanDateFrom nvarchar(10), @PurchasePlanDateTo nvarchar(10), @MakerOrderNumber nvarchar(50), @InvoiceNo nvarchar(50), @SlipNumberFrom nvarchar(50), @SlipNumberTo nvarchar(50), @SupplierCode nvarchar(10), @DepartmentCode nvarchar(3), @CustomerCode nvarchar(10), @EmployeeCode nvarchar(50), @PartsNumber nvarchar(25)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchase_NotOrder' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.Quantity' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PP.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',PP.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PP.ServiceSlipNumber' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate' + @CRLF		--Add 2018/03/26 arc yano #3863
		SET @SQL = @SQL + 'FROM dbo.PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + ' WHERE PP.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF
		SET @SQL = @SQL + '   AND (PP.PurchaseOrderNumber IS NULL OR PP.PurchaseOrderNumber = '''')' + @CRLF

		--���ד`�[�ԍ�
		IF (@PurchaseNumberFrom is not null) AND (@PurchaseNumberFrom <> '')
			IF (@PurchaseNumberTo is not null) AND (@PurchaseNumberTo <> '')
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseNumber >= @PurchaseNumberFrom AND PP.PurchaseNumber <= @PurchaseNumberTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseNumber = @PurchaseNumberFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchaseNumberTo is not null) AND (@PurchaseNumberTo <> ''))
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseNumber <= @PurchaseNumberTo' + @CRLF 
			END


		--���ח\���
		IF ((@PurchasePlanDateFrom is not null) AND (@PurchasePlanDateFrom <> '') AND ISDATE(@PurchasePlanDateFrom) = 1)
			IF ((@PurchasePlanDateTo is not null) AND (@PurchasePlanDateTo <> '') AND ISDATE(@PurchasePlanDateTo) = 1)
			BEGIN
				SET @PurchasePlanDateTo = DateAdd(d, 1, @PurchasePlanDateTo)
				SET @SQL = @SQL +'AND PP.PurchasePlanDate >= @PurchasePlanDateFrom AND PP.PurchasePlanDate < @PurchasePlanDateTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchasePlanDate = @PurchasePlanDateFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchasePlanDateTo is not null) AND (@PurchasePlanDateTo <> '') AND ISDATE(@PurchasePlanDateTo) = 1)
			BEGIN
				SET @PurchasePlanDateTo = DateAdd(d, 1, @PurchasePlanDateTo)
				SET @SQL = @SQL +'AND PP.PurchasePlanDate < @PurchasePlanDateTo' + @CRLF 
			END

		--���[�J�[�I�[�_�[�ԍ�
		IF ((@MakerOrderNumber IS NOT NULL) AND (@MakerOrderNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.MakerOrderNumber = @MakerOrderNumber'+ @CRLF
		END		
			
		--�C���{�C�X�ԍ�
		IF ((@InvoiceNo IS NOT NULL) AND (@InvoiceNo <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.InvoiceNo = @InvoiceNo'+ @CRLF
		END

		--���i�ԍ�
		IF ((@PartsNumber IS NOT NULL) AND (@PartsNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.PartsNumber = @PartsNumber'+ @CRLF
		END

		--����
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.DepartmentCode = @DepartmentCode'+ @CRLF
		END

		--�d����
		IF ((@SupplierCode IS NOT NULL) AND (@SupplierCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.SupplierCode = @SupplierCode'+ @CRLF
		END

		EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseNumberFrom, @PurchaseNumberTo, @PurchaseOrderNumberFrom, @PurchaseOrderNumberTo, @PurchasePlanDateFrom, @PurchasePlanDateTo, @MakerOrderNumber, @InvoiceNo, @SlipNumberFrom, @SlipNumberTo, @SupplierCode, @DepartmentCode, @CustomerCode, @EmployeeCode, @PartsNumber
		CREATE INDEX ix_temp_PartsPurchase_NotOrder ON #temp_PartsPurchase_NotOrder(PurchaseOrderNumber, PartsNumber)


	--2016/04/25 arc yano #3502 �����`�[�ԍ��͑��݂��邪�A�����f�[�^�����݂��Ȃ����׃f�[�^���擾����悤�ɏC��
	/*-------------------------------------------------------------------*/
	/* ���׏��擾�i�����`�[�ԍ��͂��邪�����f�[�^�����݂��Ȃ����́j	 */
	/*-------------------------------------------------------------------*/
	CREATE TABLE #temp_PartsPurchase_NotExistsOrder (
		 PurchaseNumber nvarchar(50)		--���ד`�[�ԍ�
	   , PurchaseOrderNumber nvarchar(50)	--�����`�[�ԍ�
	   , Quantity decimal(10,2)				--���ח\�萔
	   , PurchasePlanDate datetime			--���ח\���
	   , MakerOrderNumber nvarchar(50)		--���[�J�[�I�[�_�[�ԍ�
	   , InvoiceNo nvarchar(50)				--�C���{�C�X�ԍ�
	   , PartsNumber nvarchar(25)			-- ���i�ԍ�
	   , DepartmentCode nvarchar(3)			--����R�[�h
	   , SupplierCode nvarchar(10)			--�d����R�[�h
	   , ServiceSlipNumber nvarchar(50)		--�󒍓`�[�ԍ�
	   , LinkEntryCaptureDate datetime		--�捞�� --Add 2018/03/26 arc yano #3863
		)

		SET @PARAM = '@PurchaseNumberFrom nvarchar(50), @PurchaseNumberTo nvarchar(50), @PurchaseOrderNumberFrom nvarchar(50), @PurchaseOrderNumberTo nvarchar(50), @PurchasePlanDateFrom nvarchar(10), @PurchasePlanDateTo nvarchar(10), @MakerOrderNumber nvarchar(50), @InvoiceNo nvarchar(50), @SlipNumberFrom nvarchar(50), @SlipNumberTo nvarchar(50), @SupplierCode nvarchar(10), @DepartmentCode nvarchar(3), @CustomerCode nvarchar(10), @EmployeeCode nvarchar(50), @PartsNumber nvarchar(25)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchase_NotOrder' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.Quantity' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PP.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',PP.SupplierCode' + @CRLF
		SET @SQL = @SQL + ',PP.ServiceSlipNumber' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863
		SET @SQL = @SQL + 'FROM dbo.PartsPurchase AS PP' + @CRLF
		SET @SQL = @SQL + ' WHERE PP.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF
		SET @SQL = @SQL + '   AND (PP.PurchaseOrderNumber IS NOT NULL AND PP.PurchaseOrderNumber <> '''')' + @CRLF	--�����`�[�ԍ���NULL�܂��͋󕶎��łȂ�
		SET @SQL = @SQL + '   AND NOT EXISTS (' + @CRLF
		SET @SQL = @SQL + '		SELECT ''x'' FROM dbo.PartsPurchaseOrder PPO WHERE PPO.DelFlag = ''0'' AND PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + '   )' + @CRLF

		--���ד`�[�ԍ�
		IF (@PurchaseNumberFrom is not null) AND (@PurchaseNumberFrom <> '')
			IF (@PurchaseNumberTo is not null) AND (@PurchaseNumberTo <> '')
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseNumber >= @PurchaseNumberFrom AND PP.PurchaseNumber <= @PurchaseNumberTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseNumber = @PurchaseNumberFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchaseNumberTo is not null) AND (@PurchaseNumberTo <> ''))
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchaseNumber <= @PurchaseNumberTo' + @CRLF 
			END


		--���ח\���
		IF ((@PurchasePlanDateFrom is not null) AND (@PurchasePlanDateFrom <> '') AND ISDATE(@PurchasePlanDateFrom) = 1)
			IF ((@PurchasePlanDateTo is not null) AND (@PurchasePlanDateTo <> '') AND ISDATE(@PurchasePlanDateTo) = 1)
			BEGIN
				SET @PurchasePlanDateTo = DateAdd(d, 1, @PurchasePlanDateTo)
				SET @SQL = @SQL +'AND PP.PurchasePlanDate >= @PurchasePlanDateFrom AND PP.PurchasePlanDate < @PurchasePlanDateTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.PurchasePlanDate = @PurchasePlanDateFrom' + @CRLF 
			END
		ELSE
			IF ((@PurchasePlanDateTo is not null) AND (@PurchasePlanDateTo <> '') AND ISDATE(@PurchasePlanDateTo) = 1)
			BEGIN
				SET @PurchasePlanDateTo = DateAdd(d, 1, @PurchasePlanDateTo)
				SET @SQL = @SQL +'AND PP.PurchasePlanDate < @PurchasePlanDateTo' + @CRLF 
			END

		--���[�J�[�I�[�_�[�ԍ�
		IF ((@MakerOrderNumber IS NOT NULL) AND (@MakerOrderNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.MakerOrderNumber = @MakerOrderNumber'+ @CRLF
		END		
			
		--�C���{�C�X�ԍ�
		IF ((@InvoiceNo IS NOT NULL) AND (@InvoiceNo <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.InvoiceNo = @InvoiceNo'+ @CRLF
		END

		--���i�ԍ�
		IF ((@PartsNumber IS NOT NULL) AND (@PartsNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.PartsNumber = @PartsNumber'+ @CRLF
		END

		--����
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.DepartmentCode = @DepartmentCode'+ @CRLF
		END

		--�d����
		IF ((@SupplierCode IS NOT NULL) AND (@SupplierCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND PP.SupplierCode = @SupplierCode'+ @CRLF
		END

		EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseNumberFrom, @PurchaseNumberTo, @PurchaseOrderNumberFrom, @PurchaseOrderNumberTo, @PurchasePlanDateFrom, @PurchasePlanDateTo, @MakerOrderNumber, @InvoiceNo, @SlipNumberFrom, @SlipNumberTo, @SupplierCode, @DepartmentCode, @CustomerCode, @EmployeeCode, @PartsNumber
		CREATE INDEX ix_PartsPurchase_NotExistsOrder ON #temp_PartsPurchase_NotExistsOrder(PurchaseOrderNumber, PartsNumber)
	
	/*-------------------------------------------*/
	/* �����ׂ̕��i���׏��擾					 */
	/*-------------------------------------------*/

		SET @PARAM = ''
		SET @SQL = ''

		--�����f�[�^������A���ח\�萔���������Ă��関���׃f�[�^
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PurchaseOrderDate AS PurchaseOrderDate' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL + ',C.CustomerName AS CustomerName' + @CRLF
		SET @SQL = @SQL + ',PPO.PartsNumber AS PartsNumber' + @CRLF
		SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',PPO.RemainingQuantity AS RemainingQuantity' + @CRLF	
		SET @SQL = @SQL + ',PP.Quantity AS PurchasePlanQuantity' + @CRLF
		SET @SQL = @SQL + ',COT.Name AS OrderTypeName' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PPO.WebOrderNumber AS WebOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
		SET @SQL = @SQL + ',E.EmployeeName AS EmployeeName' + @CRLF
		SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate AS LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863

		SET @SQL = @SQL + 'FROM #temp_PartsPurchaseOrder AS PPO' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_ServiceSalesHeader AS H ON H.SlipNumber = PPO.ServiceSlipNumber'+ @CRLF
		SET @SQL = @SQL + 'INNER JOIN #temp_PartsPurchase AS PP ON PP.PurchaseOrderNumber = PPO.PurchaseOrderNumber AND PP.PartsNumber = PPO.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Customer AS C ON C.CustomerCode = H.CustomerCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PPO.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_OrderType AS COT ON COT.Code = PPO.OrderType'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PPO.DepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E ON E.EmployeeCode = H.FrontEmployeeCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PPO.SupplierCode'+ @CRLF
		--Add 2018/03/26 arc yano #3863
		--LinkEntry�捞��
		SET @SQL = @SQL + 'WHERE' + @CRLF
		SET @SQL = @SQL +	'1 = 1' + @CRLF
		IF ((@LinkEntryCaptureDateFrom is not null) AND (@LinkEntryCaptureDateFrom <> '') AND ISDATE(@LinkEntryCaptureDateFrom) = 1)
			IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
			BEGIN
				--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate >= CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''') AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate = CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''')' + @CRLF 
			END
		ELSE
			IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
			BEGIN
				--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')'  + @CRLF 
			END
				
		SET @SQL = @SQL +'UNION' + @CRLF
		
		--2016/04/25 arc yano 
		--�������ƁA�ʂ̕��i�����ח\��ɂȂ��Ă��関���׃f�[�^
		SET @SQL = @SQL + 'SELECT DISTINCT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',NULL AS PurchaseOrderDate' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL + ',C.CustomerName AS CustomerName' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
		SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',NULL AS RemainingQuantity' + @CRLF	
		SET @SQL = @SQL + ',PP.Quantity AS PurchasePlanQuantity' + @CRLF
		SET @SQL = @SQL + ',COT.Name AS OrderTypeName' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ','''' AS WebOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
		SET @SQL = @SQL + ',E.EmployeeName AS EmployeeName' + @CRLF
		SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
		SET @SQL = @SQL + ',PP.LinkEntryCaptureDate AS LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863

		SET @SQL = @SQL + 'FROM #temp_PartsPurchase_ChangeParts AS PP' + @CRLF
		SET @SQL = @SQL + 'INNER JOIN #temp_PartsPurchaseOrder AS PPO ON PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_ServiceSalesHeader AS H ON H.SlipNumber = PPO.ServiceSlipNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Customer AS C ON C.CustomerCode = H.CustomerCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_OrderType AS COT ON COT.Code = PPO.OrderType'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PP.DepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E ON E.EmployeeCode = H.FrontEmployeeCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
		
		SET @SQL = @SQL + ' WHERE PP.PurchaseStatus = ''001''' + @CRLF
		SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF
		--Add 2018/03/26 arc yano #3863
		--LinkEntry�捞��
		IF ((@LinkEntryCaptureDateFrom is not null) AND (@LinkEntryCaptureDateFrom <> '') AND ISDATE(@LinkEntryCaptureDateFrom) = 1)
			IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
			BEGIN
				--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate >= CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''') AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate = CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''')' + @CRLF 
			END
		ELSE
			IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
			BEGIN
				--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
				SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')'  + @CRLF 
			END
		
		/*
		--�������ƁA�ʂ̕��i�����ח\��ɂȂ��Ă��関���׃f�[�^
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
		SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PPO.PurchaseOrderDate AS PurchaseOrderDate' + @CRLF
		SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL + ',C.CustomerName AS CustomerName' + @CRLF
		SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
		SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',NULL AS RemainingQuantity' + @CRLF	
		SET @SQL = @SQL + ',PP.Quantity AS PurchasePlanQuantity' + @CRLF
		SET @SQL = @SQL + ',COT.Name AS OrderTypeName' + @CRLF
		SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
		SET @SQL = @SQL + ',PPO.WebOrderNumber AS WebOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
		SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
		SET @SQL = @SQL + ',E.EmployeeName AS EmployeeName' + @CRLF
		SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF		

		SET @SQL = @SQL + 'FROM #temp_PartsPurchase_ChangeParts AS PP' + @CRLF
		SET @SQL = @SQL + 'INNER JOIN #temp_PartsPurchaseOrder AS PPO ON PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_ServiceSalesHeader AS H ON H.SlipNumber = PPO.ServiceSlipNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Customer AS C ON C.CustomerCode = H.CustomerCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_OrderType AS COT ON COT.Code = PPO.OrderType'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PP.DepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E ON E.EmployeeCode = H.FrontEmployeeCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
		
		SET @SQL = @SQL + ' WHERE PP.PurchaseStatus = ''001''' + @CRLF
		SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF		
		*/

		----�d���`�[�ԍ������������ɂȂ������甭����������ח\�肪�Ȃ������׃f�[�^��\������
		IF((@PurchaseNumberFrom is null) OR (@PurchaseNumberFrom = '')) AND ((@PurchaseNumberTo is null) OR (@PurchaseNumberTo = ''))
		AND ((@InvoiceNo is null) OR (@InvoiceNo = ''))
		AND ((@MakerOrderNumber is null) OR (@MakerOrderNumber = ''))
		BEGIN

			SET @SQL = @SQL +'UNION' + @CRLF

			--�����f�[�^������A���ח\�肪�������Ă��Ȃ������׃f�[�^
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' '''' AS PurchaseNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.PurchaseOrderDate AS PurchaseOrderDate' + @CRLF
			SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
			SET @SQL = @SQL + ',C.CustomerName AS CustomerName' + @CRLF
			SET @SQL = @SQL + ',PPO.PartsNumber AS PartsNumber' + @CRLF
			SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
			SET @SQL = @SQL + ',PPO.RemainingQuantity AS RemainingQuantity' + @CRLF
			SET @SQL = @SQL + ',NULL AS PurchasePlanQuantity' + @CRLF
			SET @SQL = @SQL + ',COT.Name AS OrderTypeName' + @CRLF
			SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
			SET @SQL = @SQL + ',PPO.WebOrderNumber AS WebOrderNumber' + @CRLF
			SET @SQL = @SQL + ','''' AS MakerOrderNumber' + @CRLF
			SET @SQL = @SQL + ','''' AS InvoiceNo' + @CRLF
			SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
			SET @SQL = @SQL + ',E.EmployeeName AS EmployeeName' + @CRLF
			SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
			SET @SQL = @SQL + ',PP.LinkEntryCaptureDate AS LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863

			SET @SQL = @SQL + 'FROM #temp_PartsPurchaseOrder AS PPO' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_ServiceSalesHeader AS H ON H.SlipNumber = PPO.ServiceSlipNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchase AS PP ON PP.PurchaseOrderNumber = PPO.PurchaseOrderNumber AND PP.PartsNumber = PPO.PartsNumber'+ @CRLF
			--SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_RemainingQuantity AS RQ ON RQ.PurchaseOrderNumber = PPO.PurchaseOrderNumber AND RQ.PartsNumber = PPO.PartsNumber'+ @CRLF		
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Customer AS C ON C.CustomerCode = H.CustomerCode'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PPO.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_OrderType AS COT ON COT.Code = PPO.OrderType'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PP.DepartmentCode'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E ON E.EmployeeCode = H.FrontEmployeeCode'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF				
			SET @SQL = @SQL + '   WHERE (PP.PurchaseNumber IS NULL OR PP.PurchaseNumber = '''')' + @CRLF
			--Add 2018/03/26 arc yano #3863
			--LinkEntry�捞��
			IF ((@LinkEntryCaptureDateFrom is not null) AND (@LinkEntryCaptureDateFrom <> '') AND ISDATE(@LinkEntryCaptureDateFrom) = 1)
				IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
				BEGIN
					--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate >= CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''') AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')' + @CRLF
				END
				ELSE
				BEGIN
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate = CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''')' + @CRLF 
				END
			ELSE
				IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
				BEGIN
					--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')'  + @CRLF 
				END
			
			--SET @SQL = @SQL + '   WHERE RQ.PurchaseStatus = ''001''' + @CRLF
			--SET @SQL = @SQL + '     AND RQ.RemainingQuantity2 IS NOT NULL' + @CRLF

		END

		--|-------------------�����Ɋւ���f�[�^------------------------------------|
		--[�����`�[�ԍ�]�E[�󒍓`�[�ԍ�]�E[������]�E[�����敪]�E[WEB�I�[�_�[�i���o�[]�����������ɂȂ������甭���̂Ȃ����׏���\������
		IF ((@PurchaseOrderNumberFrom is null) OR (@PurchaseOrderNumberFrom = '')) AND ((@PurchaseOrderNumberTo is null) OR (@PurchaseOrderNumberTo = ''))
		AND ((@PurchaseOrderDateFrom is null) OR (@PurchaseOrderDateFrom = '')) AND ((@PurchaseOrderDateTo is null) OR (@PurchaseOrderDateTo = ''))
		AND ((@OrderType is null) OR (@OrderType = ''))
		AND ((@WebOrderNumber is null) OR (@WebOrderNumber = ''))
		AND ((@SlipNumberFrom is null) OR (@SlipNumberFrom = '')) AND ((@SlipNumberTo is null) OR (@SlipNumberTo = ''))
		AND ((@CustomerCode is null) OR (@CustomerCode = ''))
		AND ((@EmployeeCode is null) OR (@EmployeeCode = ''))

		BEGIN
			SET @SQL = @SQL +'UNION' + @CRLF

			--�����̂Ȃ����׃f�[�^
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
			SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ',NULL AS PurchaseOrderDate' + @CRLF
			SET @SQL = @SQL + ','''' AS SlipNumber' + @CRLF
			SET @SQL = @SQL + ','''' AS CustomerName' + @CRLF
			SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
			SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
			SET @SQL = @SQL + ',NULL AS RemainingQuantity' + @CRLF
			SET @SQL = @SQL + ',PP.Quantity AS PurchasePlanQuantity' + @CRLF
			SET @SQL = @SQL + ','''' AS OrderTypeName' + @CRLF
			SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
			SET @SQL = @SQL + ','''' AS WebOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
			SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
			SET @SQL = @SQL + ','''' AS EmployeeName' + @CRLF
			SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
			SET @SQL = @SQL + ',PP.LinkEntryCaptureDate AS LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863

			SET @SQL = @SQL + 'FROM #temp_PartsPurchase_NotOrder AS PP' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PP.DepartmentCode'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
			--Add 2018/03/26 arc yano #3863
			--LinkEntry�捞��
			SET @SQL = @SQL + 'WHERE' + @CRLF
			SET @SQL = @SQL +	'1 = 1' + @CRLF
			IF ((@LinkEntryCaptureDateFrom is not null) AND (@LinkEntryCaptureDateFrom <> '') AND ISDATE(@LinkEntryCaptureDateFrom) = 1)
				IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
				BEGIN
					--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate >= CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''') AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')' + @CRLF
				END
				ELSE
				BEGIN
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate = CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''')' + @CRLF 
				END
			ELSE
				IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
				BEGIN
					--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')'  + @CRLF 
				END
			SET @SQL = @SQL +'UNION' + @CRLF

			--Add 2016/04/25 arc yano 
			--2016/04/25 arc yano #3502 �����`�[�ԍ��͑��݂��邪�A�����f�[�^�����݂��Ȃ����׃f�[�^���擾����悤�ɏC��
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
			SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ',NULL AS PurchaseOrderDate' + @CRLF
			SET @SQL = @SQL + ','''' AS SlipNumber' + @CRLF
			SET @SQL = @SQL + ','''' AS CustomerName' + @CRLF
			SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
			SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
			SET @SQL = @SQL + ',NULL AS RemainingQuantity' + @CRLF
			SET @SQL = @SQL + ',PP.Quantity AS PurchasePlanQuantity' + @CRLF
			SET @SQL = @SQL + ','''' AS OrderTypeName' + @CRLF
			SET @SQL = @SQL + ',PP.PurchasePlanDate AS PurchasePlanDate' + @CRLF
			SET @SQL = @SQL + ','''' AS WebOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
			SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
			SET @SQL = @SQL + ','''' AS EmployeeName' + @CRLF
			SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
			SET @SQL = @SQL + ',PP.LinkEntryCaptureDate AS LinkEntryCaptureDate' + @CRLF	--Add 2018/03/26 arc yano #3863

			SET @SQL = @SQL + 'FROM #temp_PartsPurchase_NotExistsOrder AS PP' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = PP.DepartmentCode'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
			--Add 2018/03/26 arc yano #3863
			--LinkEntry�捞��
			SET @SQL = @SQL + 'WHERE' + @CRLF
			SET @SQL = @SQL +	'1 = 1' + @CRLF
			IF ((@LinkEntryCaptureDateFrom is not null) AND (@LinkEntryCaptureDateFrom <> '') AND ISDATE(@LinkEntryCaptureDateFrom) = 1)
				IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
				BEGIN
					--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate >= CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''') AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')' + @CRLF
				END
				ELSE
				BEGIN
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate = CONVERT(datetime,''' + @LinkEntryCaptureDateFrom + ''')' + @CRLF 
				END
			ELSE
				IF ((@LinkEntryCaptureDateTo is not null) AND (@LinkEntryCaptureDateTo <> '') AND ISDATE(@LinkEntryCaptureDateTo) = 1)
				BEGIN
					--SET @LinkEntryCaptureDateTo = CONVERT(nvarchar(10), DateAdd(d, 1, CONVERT(datetime, @LinkEntryCaptureDateTo)), 111)
					SET @SQL = @SQL +'AND PP.LinkEntryCaptureDate <  CONVERT(datetime, ''' + @LinkEntryCaptureDateTo + ''')'  + @CRLF 
				END			
		END

		SET @SQL = @SQL +'ORDER BY PPO.PurchaseOrderNumber, PPO.PartsNumber, PP.PurchaseNumber desc' + @CRLF

		--debug
		--print @SQL

		EXECUTE sp_executeSQL @SQL, @PARAM	--Mod 2018/03/26 arc yano #3863
--*/

-- !!!!�߂�l�̌^��GetPurchase_Result��dbml�ɒ�`����Ă���

END




GO


