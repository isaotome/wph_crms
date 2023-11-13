USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCarStatusTransition]    Script Date: 2018/05/17 16:09:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/19 arc yano #3721 �T�u�V�X�e���ڍs(�ԗ��ǐ�) �V�K�쐬
-- Update date: <Update Date,,>
-- 2018/05/17 arc yano #3885 �ԗ��ǐՁ@���I�u0�v�ł��ǐՉ�ʂɕ\�������
-- 2018/02/21 arc yano #3856 �ԗ��ǐՁ@�ԗ��I�����ʂ̗������\������Ȃ�
-- Description:	<Description,,>
-- �ԗ��X�e�[�^�X�J�ڏ��̎擾
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetCarStatusTransition] 
	    @SalesCarNumber nvarchar(50) = ''			--�ԗ��Ǘ��ԍ�
	  , @Vin nvarchar(20) = ''						--�ԑ�ԍ�
AS 
BEGIN

	SET NOCOUNT ON;

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	/*-------------------------------------------*/
	/* �f�[�^�擾								 */
	/*-------------------------------------------*/
	--�d�����
	SET @SQL = ''
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  ''�y�d�z'' AS SlipTypeName' + @CRLF																				--�敪
	SET @SQL = @SQL +'	, P.CarPurchaseType AS SlipTypeCode' + @CRLF																		--�d�����
	SET @SQL = @SQL +'	, CASE WHEN P.CarPurchaseType IS NULL THEN ''�d��'' ELSE C2.Name END AS SlipType' + @CRLF							--�`�[�^�C�v
	SET @SQL = @SQL +'	, P.PurchaseDate AS SlipDate' + @CRLF																				--�d����
    SET @SQL = @SQL +'	, P.PurchaseLocationCode AS LocationCode' + @CRLF																	--���Ƀ��P�[�V����
    SET @SQL = @SQL +'	, L.LocationName AS LocationName' + @CRLF																			--���Ƀ��P�[�V������
    SET @SQL = @SQL +'	, P.SupplierCode AS CustomerCode' + @CRLF																			--�d����R�[�h
	SET @SQL = @SQL +'	, S.SupplierName AS CustomerName' + @CRLF																			--�d���於
	SET @SQL = @SQL +'	, P.PurchaseStatus AS SlipStatusCode' + @CRLF																		--�d���X�e�[�^�X�R�[�h
	SET @SQL = @SQL +'	, C.Name AS SlipStatus' + @CRLF																						--�d���X�e�[�^�X��
	SET @SQL = @SQL +'	, P.EmployeeCode AS EmployeeCode' + @CRLF																			--�d���S���҃R�[�h
	SET @SQL = @SQL +'	, E.EmployeeName AS EmployeeName' + @CRLF																			--�d���S���Җ�
	SET @SQL = @SQL +'	, P.SalesCarNumber AS SalesCarNumber' + @CRLF																		--�ԗ��Ǘ��ԍ�
	SET @SQL = @SQL +'	, '''' AS SlipNumber' + @CRLF																						--�`�[�ԍ�
	SET @SQL = @SQL +'	, P.VehiclePrice AS VehiclePrice' + @CRLF																			--�ԗ��{�̉��i
	SET @SQL = @SQL +'	, P.Amount AS Amount' + @CRLF																						--�d�����z
	SET @SQL = @SQL +'	, 1 AS SlipTypeOrder' + @CRLF																						--
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	CarPurchase P LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Location L ON P.PurchaseLocationCode=L.LocationCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Employee E ON P.EmployeeCode=E.EmployeeCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Supplier S ON P.SupplierCode=S.SupplierCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_PurchaseStatus C ON P.PurchaseStatus=C.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_CarPurchaseType C2 on P.CarPurchaseType = C2.Code' + @CRLF
	

	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	P.DelFlag=''0''' + @CRLF
	SET @SQL = @SQL +'	AND P.SalesCarNumber is not NULL' + @CRLF
	SET @SQL = @SQL +'	AND P.SalesCarNumber <> ''''' + @CRLF

	--���������̎ԑ�ԍ������͂���Ă����ꍇ
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	EXISTS' + @CRLF
		SET @SQL = @SQL +'	(' + @CRLF
		SET @SQL = @SQL +'		SELECT ''x'' FROM SalesCar S WHERE S.Vin =''' + @Vin + '''' + 'AND P.SalesCarNumber = S.SalesCarNumber' + @CRLF
		SET @SQL = @SQL +'	)' + @CRLF
	END
	ELSE IF(@SalesCarNumber is not NULL AND @SalesCarNumber <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	P.SalesCarNumber = '''  + @SalesCarNumber + '''' + @CRLF
	END

	--����
	SET @SQL = @SQL +'UNION ALL' + @CRLF

	--�̔����
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  ''�y���z'' AS SlipTypeName' + @CRLF																				--�敪
	SET @SQL = @SQL +'	, H.SalesType AS SlipTypeCode' + @CRLF																				--�̔��敪
	SET @SQL = @SQL +'	, C2.Name AS SlipType' + @CRLF																						--�`�[�敪
	SET @SQL = @SQL +'	, H.SalesDate AS SlipDate' + @CRLF																					--�[�ԓ�
    SET @SQL = @SQL +'	, H.DepartmentCode AS LocationCode' + @CRLF																			--����R�[�h
    SET @SQL = @SQL +'	, D.DepartmentName AS LocationName' + @CRLF																			--���喼
    SET @SQL = @SQL +'	, H.CustomerCode AS CustomerCode' + @CRLF																			--�ڋq�R�[�h
	SET @SQL = @SQL +'	, C.CustomerName AS CustomerName' + @CRLF																			--�ڋq��
	SET @SQL = @SQL +'	, H.SalesOrderStatus AS SlipStatusCode' + @CRLF																		--�`�[�X�e�[�^�X
	SET @SQL = @SQL +'	, C1.Name AS SlipStatus' + @CRLF																					--�`�[�X�e�[�^�X��
	SET @SQL = @SQL +'	, H.EmployeeCode AS EmployeeCode' + @CRLF																			--�S���҃R�[�h
	SET @SQL = @SQL +'	, E.employeename AS EmployeeName' + @CRLF																			--���Җ�
	SET @SQL = @SQL +'	, H.SalesCarNumber AS SalesCarNumber' + @CRLF																		--�ԗ��Ǘ��ԍ�
	SET @SQL = @SQL +'	, H.SlipNumber AS SlipNumber' + @CRLF																				--�`�[�ԍ�
	SET @SQL = @SQL +'	, H.SalesPrice AS VehiclePrice' + @CRLF																				--�ԗ��{�̉��i
	SET @SQL = @SQL +'	, (H.GrandTotalAmount - H.Totaltaxamount) AS Amount' + @CRLF														--���z
	SET @SQL = @SQL +'	, 0 AS SlipTypeOrder' + @CRLF																						--
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	CarSalesHeader H LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Department D on H.DepartmentCode=D.DepartmentCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Employee E on H.EmployeeCode=E.EmployeeCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Customer C on H.CustomerCode=C.CustomerCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_SalesOrderStatus C1 on H.SalesOrderStatus=C1.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_SalesType C2 on H.SalesType=C2.Code' + @CRLF

	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	H.DelFlag=''0''' + @CRLF
	SET @SQL = @SQL +'	AND H.SalesOrderStatus in (''005'') ' + @CRLF
	SET @SQL = @SQL +'	AND H.SalesCarNumber is not NULL' + @CRLF
	SET @SQL = @SQL +'	AND H.SalesCarNumber <> ''''' + @CRLF

	--���������̎ԑ�ԍ������͂���Ă����ꍇ
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	EXISTS' + @CRLF
		SET @SQL = @SQL +'	(' + @CRLF
		SET @SQL = @SQL +'		SELECT ''x'' FROM SalesCar S WHERE S.Vin =''' + @Vin + '''' + 'AND H.SalesCarNumber = S.SalesCarNumber' + @CRLF
		SET @SQL = @SQL +'	)' + @CRLF
	END
	ELSE IF(@SalesCarNumber is not NULL AND @SalesCarNumber <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	H.SalesCarNumber = '''  + @SalesCarNumber + '''' + @CRLF
	END

	--����
	SET @SQL = @SQL +'UNION ALL' + @CRLF

	--�ړ����
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  ''�y�ځz'' AS SlipTypeName' + @CRLF																				--�敪
	SET @SQL = @SQL +'	, '''' AS SlipTypeCode' + @CRLF																						--�̔��敪
	SET @SQL = @SQL +'	, C1.Name AS SlipType' + @CRLF																						--�`�[�敪
	SET @SQL = @SQL +'	, T.ArrivalDate AS SlipDate' + @CRLF																				--���ɓ�
    SET @SQL = @SQL +'	, T.DepartureLocationCode AS LocationCode' + @CRLF																	--�o�����P�[�V�����R�[�h
    SET @SQL = @SQL +'	, L1.LocationName AS LocationName' + @CRLF																			--�o�����P�[�V������
    SET @SQL = @SQL +'	, T.ArrivalLocationCode AS CustomerCode' + @CRLF																	--�������P�[�V�����R�[�h
	SET @SQL = @SQL +'	, '' �� '' + isnull(L2.LocationName,'''') AS CustomerName' + @CRLF													--�������P�[�V������
	SET @SQL = @SQL +'	, '''' AS SlipStatusCode' + @CRLF																					--
	SET @SQL = @SQL +'	, '''' AS SlipStatus' + @CRLF																						--
	SET @SQL = @SQL +'	, T.ArrivalEmployeeCode AS EmployeeCode' + @CRLF																	--�����S���҃R�[�h
	SET @SQL = @SQL +'	, E.employeename AS EmployeeName' + @CRLF																			--���Җ�
	SET @SQL = @SQL +'	, T.SalesCarNumber AS SalesCarNumber' + @CRLF																		--�ԗ��Ǘ��ԍ�
	SET @SQL = @SQL +'	, T.TransferNumber AS SlipNumber' + @CRLF																			--�ړ��`�[�ԍ�
	SET @SQL = @SQL +'	, NULL AS VehiclePrice' + @CRLF																						--�ԗp�{�̉��i
	SET @SQL = @SQL +'	, NULL AS Amount' + @CRLF																							--���z
	SET @SQL = @SQL +'	, 2 AS SlipTypeOrder' + @CRLF																						--
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	Transfer T LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Location L1 on T.DepartureLocationCode=L1.LocationCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Location L2 on T.ArrivalLocationCode=L2.LocationCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_TransferType C1 on T.TransferType=C1.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Employee E on T.ArrivalEmployeeCode=E.EmployeeCode' + @CRLF

	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	T.DelFlag=''0''' + @CRLF
	SET @SQL = @SQL +'	AND T.ArrivalDate is not NULL' + @CRLF
	SET @SQL = @SQL +'	AND T.SalesCarNumber is not NULL' + @CRLF
	SET @SQL = @SQL +'	AND T.SalesCarNumber <> ''''' + @CRLF

	--���������̎ԑ�ԍ������͂���Ă����ꍇ
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	EXISTS' + @CRLF
		SET @SQL = @SQL +'	(' + @CRLF
		SET @SQL = @SQL +'		SELECT ''x'' FROM SalesCar S WHERE S.Vin =''' + @Vin + '''' + 'AND T.SalesCarNumber = S.SalesCarNumber' + @CRLF
		SET @SQL = @SQL +'	)' + @CRLF
	END
	ELSE IF(@SalesCarNumber is not NULL AND @SalesCarNumber <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	T.SalesCarNumber = '''  + @SalesCarNumber + '''' + @CRLF
	END

	--����
	SET @SQL = @SQL +'UNION ALL' + @CRLF

	--�I�����
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  ''�y�I�z'' AS SlipTypeName' + @CRLF																				--�敪
	SET @SQL = @SQL +'	, I.InventoryType AS SlipTypeCode' + @CRLF																			--�I���敪
	SET @SQL = @SQL +'	, C1.Name AS SlipType' + @CRLF																						--�I���敪��
	SET @SQL = @SQL +'	, DATEADD(d, -1, DATEADD(m, 1, I.InventoryMonth)) AS SlipDate' + @CRLF																				--���ɓ�
    SET @SQL = @SQL +'	, I.LocationCode AS LocationCode' + @CRLF																			--�I�����P�[�V�����R�[�h
    SET @SQL = @SQL +'	, L.LocationName AS LocationName' + @CRLF																			--�I�����P�[�V������
    SET @SQL = @SQL +'	, '''' AS CustomerCode' + @CRLF																						--
	SET @SQL = @SQL +'	, Summary AS CustomerName' + @CRLF																					--�E�v
	SET @SQL = @SQL +'	, '''' AS SlipStatusCode' + @CRLF																					--
	SET @SQL = @SQL +'	, '''' AS SlipStatus' + @CRLF																						--
	SET @SQL = @SQL +'	, '''' AS EmployeeCode' + @CRLF																						--
	SET @SQL = @SQL +'	, '''' AS EmployeeName' + @CRLF																						--
	SET @SQL = @SQL +'	, I.SalesCarNumber AS SalesCarNumber' + @CRLF																		--�ԗ��Ǘ��ԍ�
	SET @SQL = @SQL +'	, '''' AS SlipNumber' + @CRLF																						--
	SET @SQL = @SQL +'	, NULL AS VehiclePrice' + @CRLF																						--�ԗp�{�̉��i
	SET @SQL = @SQL +'	, NULL AS Amount' + @CRLF																							--���z
	SET @SQL = @SQL +'	, 3 AS SlipTypeOrder' + @CRLF																						--
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	InventoryStock I INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	c_InventoryType C1 on I.InventoryType=C1.Code INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	Location L on I.LocationCode=L.LocationCode' + @CRLF

	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	I.DelFlag=''0''' + @CRLF
	SET @SQL = @SQL +'	AND I.Quantity = 1' + @CRLF
	SET @SQL = @SQL +'	AND I.InventoryType <> ''002''' + @CRLF
	SET @SQL = @SQL +'	AND I.SalesCarNumber is not NULL' + @CRLF
	SET @SQL = @SQL +'	AND I.SalesCarNumber <> ''''' + @CRLF

	--���������̎ԑ�ԍ������͂���Ă����ꍇ
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	EXISTS' + @CRLF
		SET @SQL = @SQL +'	(' + @CRLF
		SET @SQL = @SQL +'		SELECT ''x'' FROM SalesCar S WHERE S.Vin =''' + @Vin + '''' + 'AND I.SalesCarNumber = S.SalesCarNumber' + @CRLF
		SET @SQL = @SQL +'	)' + @CRLF
	END
	ELSE IF(@SalesCarNumber is not NULL AND @SalesCarNumber <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	I.SalesCarNumber = '''  + @SalesCarNumber + '''' + @CRLF
	END

	--Add 2018/02/21 arc yano #3856
	--����
	SET @SQL = @SQL +'UNION ALL' + @CRLF

	--�I�����
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  ''�y�I�z'' AS SlipTypeName' + @CRLF																				--�敪
	SET @SQL = @SQL +'	, ''001'' AS SlipTypeCode' + @CRLF																					--�I���敪
	SET @SQL = @SQL +'	, ''�ԗ�'' AS SlipType' + @CRLF																						--�I���敪��
	SET @SQL = @SQL +'	, DATEADD(d, -1, DATEADD(m, 1, I.InventoryMonth)) AS SlipDate' + @CRLF											    --���ɓ�
    SET @SQL = @SQL +'	, I.LocationCode AS LocationCode' + @CRLF																			--�I�����P�[�V�����R�[�h
    SET @SQL = @SQL +'	, L.LocationName AS LocationName' + @CRLF																			--�I�����P�[�V������
    SET @SQL = @SQL +'	, '''' AS CustomerCode' + @CRLF																						--
	SET @SQL = @SQL +'	, Summary AS CustomerName' + @CRLF																					--�E�v
	SET @SQL = @SQL +'	, '''' AS SlipStatusCode' + @CRLF																					--
	SET @SQL = @SQL +'	, '''' AS SlipStatus' + @CRLF																						--
	SET @SQL = @SQL +'	, '''' AS EmployeeCode' + @CRLF																						--
	SET @SQL = @SQL +'	, '''' AS EmployeeName' + @CRLF																						--
	SET @SQL = @SQL +'	, I.SalesCarNumber AS SalesCarNumber' + @CRLF																		--�ԗ��Ǘ��ԍ�
	SET @SQL = @SQL +'	, '''' AS SlipNumber' + @CRLF																						--
	SET @SQL = @SQL +'	, NULL AS VehiclePrice' + @CRLF																						--�ԗp�{�̉��i
	SET @SQL = @SQL +'	, NULL AS Amount' + @CRLF																							--���z
	SET @SQL = @SQL +'	, 3 AS SlipTypeOrder' + @CRLF																						--
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	InventoryStockCar I INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	Location L on I.LocationCode=L.LocationCode' + @CRLF

	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'	I.DelFlag=''0''' + @CRLF
	SET @SQL = @SQL +'	AND I.PhysicalQuantity = 1' + @CRLF												--2018/05/17 arc yano #3885
	SET @SQL = @SQL +'	AND I.SalesCarNumber is not NULL' + @CRLF
	SET @SQL = @SQL +'	AND I.SalesCarNumber <> ''''' + @CRLF

	--���������̎ԑ�ԍ������͂���Ă����ꍇ
	IF (@Vin is not NULL AND @Vin <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	EXISTS' + @CRLF
		SET @SQL = @SQL +'	(' + @CRLF
		SET @SQL = @SQL +'		SELECT ''x'' FROM SalesCar S WHERE S.Vin =''' + @Vin + '''' + 'AND I.SalesCarNumber = S.SalesCarNumber' + @CRLF
		SET @SQL = @SQL +'	)' + @CRLF
	END
	ELSE IF(@SalesCarNumber is not NULL AND @SalesCarNumber <> '')
	BEGIN
		SET @SQL = @SQL +'	AND ' + @CRLF
		SET @SQL = @SQL +'	I.SalesCarNumber = '''  + @SalesCarNumber + '''' + @CRLF
	END


	SET @SQL = @SQL +'	ORDER BY' + @CRLF
	SET @SQL = @SQL +'	 SlipDate, SlipTypeOrder' + @CRLF	

	EXECUTE sp_executeSQL @SQL

	--PRINT @SQL

/*
	SELECT
	  CONVERT(nvarchar(10), '') AS SlipTypeName
	, CONVERT(nvarchar(10), NULL) AS SlipTypeCode
	, CONVERT(nvarchar(50), NULL) AS SlipType
	, CONVERT(datetime, NULL) AS SlipDate
	, CONVERT(nvarchar(12), NULL) AS LocationCode
	, CONVERT(nvarchar(50), NULL) AS LocationName
	, CONVERT(nvarchar(20), NULL) AS CustomerCode
	, CONVERT(nvarchar(200), NULL) AS CustomerName
	, CONVERT(nvarchar(3), NULL) AS SlipStatusCode
	, CONVERT(nvarchar(50), NULL) AS SlipStatus
	, CONVERT(nvarchar(50), NULL) AS EmployeeCode
	, CONVERT(nvarchar(40), NULL) AS EmployeeName
	, CONVERT(nvarchar(50), NULL) AS SalesCarNumber
	, CONVERT(nvarchar(50), NULL) AS SlipNumber
	, CONVERT(decimal(10, 0), NULL) AS VehiclePrice
	, CONVERT(decimal(10, 0), NULL) AS Amount
	, CONVERT(int, NULL) AS SlipTypeOrder
*/

END


GO


