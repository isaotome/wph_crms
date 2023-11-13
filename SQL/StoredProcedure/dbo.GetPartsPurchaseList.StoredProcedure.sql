USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsPurchaseList]    Script Date: 2017/11/07 16:35:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





--2017/11/06 arc yano #3808 ���i���� Web�I�[�_�[�ԍ��̒ǉ�
--2017/08/10 arc yano #3783 ���i���ד��� ���׎���E�L�����Z���@�\�@�������i�ԍ���DB�ɕۑ�����
--2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ��������(����R�[�h)�͕s�v�̂��߁A�p�~
--2016/06/20 arc yano #3585 ���i���׈ꗗ�@�����ǉ�(PurchaseStatus) 
--2015/11/11 arc nakayama #3292_���i���׈ꗗ �V�K�쐬�@���׈ꗗ��ʂ�����׏������s�����߂̖����׃��X�g�擾


CREATE PROCEDURE [dbo].[GetPartsPurchaseList]

	@PurchaseNumber nvarchar(50),		            --���ד`�[�ԍ�
	@PurchaseOrderNumber nvarchar(50),				--�����`�[�ԍ�
	@PartsNumber nvarchar(25),						--���i�ԍ� 
	@DepartmentCode nvarchar(3),					--����R�[�h
	@PurchaseStatus nvarchar(3)						--���׃X�e�[�^�X

AS

BEGIN

--/*	
	--�ꎞ�\�̍폜
	IF OBJECT_ID(N'tempdb..#temp_PartsPurchaseOrder', N'U') IS NOT NULL
	DROP TABLE #temp_PartsPurchaseOrder;												--������񃊃X�g
	IF OBJECT_ID(N'tempdb..#temp_PartsPurchase', N'U') IS NOT NULL
	DROP TABLE #temp_PartsPurchase;														--���׏�񃊃X�g	
	IF OBJECT_ID(N'tempdb..#temp_PartsLocation', N'U') IS NOT NULL
	DROP TABLE #temp_PartsLocation;														--���i���P�[�V�������X�g
	

	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; --�_�[�e�B�[���[�h�ݒ�

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @ROWCNT INT = 0			--�s��


	/*-------------------------------------------*/
	/* ���i���P�[�V�����e�[�u��					 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_PartsLocation (
	     PartsNumber nvarchar(25) NOT NULL			--���i�ԍ�
	   , LocationCode nvarchar(12)					--���P�[�V�����R�[�h
	   , WarehouseCode nvarchar(6) NOT NULL			--�q�ɃR�[�h
		)

	INSERT INTO #temp_PartsLocation
	SELECT
		 pl.PartsNumber								--���i�ԍ�
		,pl.LocationCode							--���P�[�V�����R�[�h
		,pl.WarehouseCode							--�q�ɃR�[�h
	FROM
		dbo.PartsLocation pl
	WHERE
		EXISTS
		(
			SELECT 'X' FROM dbo.DepartmentWarehouse dw WHERE dw.DepartmentCode = @DepartmentCode AND dw.DelFlag = '0' AND pl.WarehouseCode = dw.WarehouseCode
		)
	CREATE INDEX ix_temp_PartsLocation ON #temp_PartsLocation(PartsNumber, WarehouseCode)

	/*-------------------------------------------*/
	/* �������擾�iPartsPurchaseOrder�j		 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_PartsPurchaseOrder (
	     PurchaseOrderNumber nvarchar(50)	--�����`�[�ԍ�
	   , ServiceSlipNumber nvarchar(50)	    --�󒍓`�[�ԍ�
	   , PartsNumber nvarchar(25)			--���i�ԍ�
	   , WebOrderNumber nvarchar(50)		--WEB�I�[�_�[�ԍ�
	   , SupplierCode nvarchar(10)			--�d����R�[�h
	   , RemainingQuantity decimal(10,2)	--�����c
		)

		--�����`�[�ԍ�������ꍇ�̂݃f�[�^�擾�@�������`�[�ԍ�������Ƃ������Ƃ́A���i�ԍ�������O��
		IF (
				((@PurchaseOrderNumber IS NOT NULL) AND (@PurchaseOrderNumber <>''))
				AND 
				((@PartsNumber IS NOT NULL) AND (@PartsNumber <>''))
			)
		BEGIN
			SET @PARAM = '@PurchaseOrderNumber nvarchar(50), @PartsNumber nvarchar(25)'
			SET @SQL = ''
			SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchaseOrder' + @CRLF
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' PPO.PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.ServiceSlipNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.PartsNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.WebOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.SupplierCode' + @CRLF
			SET @SQL = @SQL + ',PPO.RemainingQuantity' + @CRLF
			SET @SQL = @SQL + 'FROM dbo.PartsPurchaseOrder AS PPO' + @CRLF
			SET @SQL = @SQL + ' WHERE' + @CRLF
			SET @SQL = @SQL + '      PPO.DelFlag = ''0''' + @CRLF
			SET @SQL = @SQL + 'AND PPO.PurchaseOrderNumber = @PurchaseOrderNumber'+ @CRLF
			SET @SQL = @SQL + 'AND PPO.PartsNumber = @PartsNumber'+ @CRLF

			EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseOrderNumber, @PartsNumber

			SELECT @ROWCNT = COUNT(ppo.PurchaseOrderNumber) FROM #temp_PartsPurchaseOrder ppo

			IF (@ROWCNT = 0)
			BEGIN
				SET @PARAM = '@PurchaseOrderNumber nvarchar(50)'
				SET @SQL = ''
				SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchaseOrder' + @CRLF
				SET @SQL = @SQL + 'SELECT TOP 1' + @CRLF
				SET @SQL = @SQL + ' PPO.PurchaseOrderNumber' + @CRLF
				SET @SQL = @SQL + ',PPO.ServiceSlipNumber' + @CRLF
				SET @SQL = @SQL + ',NULL AS PartsNumber' + @CRLF
				SET @SQL = @SQL + ',PPO.WebOrderNumber' + @CRLF
				SET @SQL = @SQL + ',PPO.SupplierCode' + @CRLF
				SET @SQL = @SQL + ',PPO.RemainingQuantity' + @CRLF
				SET @SQL = @SQL + 'FROM dbo.PartsPurchaseOrder AS PPO' + @CRLF
				SET @SQL = @SQL + ' WHERE' + @CRLF
				SET @SQL = @SQL + '      PPO.DelFlag = ''0''' + @CRLF
				SET @SQL = @SQL + 'AND PPO.PurchaseOrderNumber = @PurchaseOrderNumber'+ @CRLF

				EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseOrderNumber
				CREATE INDEX ix_temp_PartsPurchaseOrder ON #temp_PartsPurchaseOrder(PurchaseOrderNumber)
			END

		END

	/*-------------------------------------------*/
	/* ���׏��擾 �iPartsPurchaseOrder�j		 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_PartsPurchase (
		 PurchaseNumber nvarchar(50)		--���ד`�[�ԍ�
	   , PurchaseOrderNumber nvarchar(50)	--�����`�[�ԍ�
	   , PartsNumber nvarchar(25)			--���i�ԍ�
	   , Quantity decimal(10,2)				--���ח\�萔
	   , Price decimal(10,0)				--���גP��
	   , Amount decimal(10,0)				--���׋��z
	   , SupplierCode nvarchar(10)			--�d����R�[�h
	   , PurchaseType nvarchar(3)			--���ד`�[�敪
	   , MakerOrderNumber nvarchar(50)		--���[�J�[�I�[�_�[�ԍ�
	   , WebOrderNumber nvarchar(50)		--Web�I�[�_�[�ԍ�		--Add 2017/11/06 arc yano #3808
	   , InvoiceNo nvarchar(50)				--�C���{�C�X�ԍ�
	   , ReceiptNumber nvarchar(50)			--�[�i���ԍ�
	   , Memo nvarchar(100)					--����
	   , ChangePartsFlag nvarchar(2)		--��֕��i�t���O
	   , LocationCode nvarchar(12)			--���P�[�V�����R�[�h	--Add 2016/06/28
	   , OrderPartsNumber nvarchar(25)		--�������i�ԍ�			--Add 2017/08/10 arc yano #3783
		)

		--���ד`�[�ԍ�������ꍇ�̂݃f�[�^�擾
		IF ((@PurchaseNumber IS NOT NULL) AND (@PurchaseNumber <>''))
		BEGIN
			SET @PARAM = '@PurchaseNumber nvarchar(50)'
			SET @SQL = ''
			SET @SQL = @SQL + 'INSERT INTO #temp_PartsPurchase' + @CRLF
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + '  PP.PurchaseNumber' + @CRLF
			SET @SQL = @SQL + ', PP.PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ', PP.PartsNumber' + @CRLF
			SET @SQL = @SQL + ', PP.Quantity' + @CRLF
			SET @SQL = @SQL + ', PP.Price' + @CRLF
			SET @SQL = @SQL + ', PP.Amount' + @CRLF
			SET @SQL = @SQL + ', PP.SupplierCode' + @CRLF
			SET @SQL = @SQL + ', PP.PurchaseType' + @CRLF
			SET @SQL = @SQL + ', PP.MakerOrderNumber' + @CRLF
			SET @SQL = @SQL + ', PP.WebOrderNumber' + @CRLF			--Add 2017/11/06 arc yano #3808
			SET @SQL = @SQL + ', PP.InvoiceNo' + @CRLF
			SET @SQL = @SQL + ', PP.ReceiptNumber' + @CRLF
			SET @SQL = @SQL + ', PP.Memo' + @CRLF
			SET @SQL = @SQL + ', PP.ChangePartsFlag' + @CRLF
			SET @SQL = @SQL + ', PP.LocationCode' + @CRLF						--Add 2016/06/28 
			SET @SQL = @SQL + ', PP.OrderPartsNumber' + @CRLF					--Add 2017/08/10 arc yano #3783
			SET @SQL = @SQL + ' FROM dbo.PartsPurchase AS PP' + @CRLF
			SET @SQL = @SQL + ' WHERE PP.DelFlag = ''0''' + @CRLF
			--SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''001''' + @CRLF
			SET @SQL = @SQL + '   AND PP.PurchaseStatus = ''' + @PurchaseStatus + '''' + @CRLF		--���׃X�e�[�^�X�͈����ŖႤ
			SET @SQL = @SQL + 'AND PP.PurchaseNumber = @PurchaseNumber'+ @CRLF

			EXECUTE sp_executeSQL @SQL, @PARAM, @PurchaseNumber
			CREATE INDEX ix_temp_PartsPurchase ON #temp_PartsPurchase(PurchaseOrderNumber, PartsNumber)
		END

	/*-------------------------------------------*/
	/* �����ׂ̕��i���׏��擾					 */
	/*-------------------------------------------*/

		SET @PARAM = ''
		SET @SQL = ''

		--�`�F�b�N�������ڂ̕ҏW�̏ꍇ(�����`�[�ԍ�)
		--���ד`�[�ԍ����L�[�������ꍇ
		IF((@PurchaseNumber IS NOT NULL) AND (@PurchaseNumber <>''))
		BEGIN
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
			SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
			SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
			SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
			SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
			--Mod 2016/06/28 arc yano
			IF(@PurchaseStatus = '001')
			BEGIN
				SET @SQL = @SQL + ',PL.LocationCode AS LocationCode' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL + ',PP.LocationCode AS LocationCode' + @CRLF		--Add 2016/06/28 arc yano
			END
			SET @SQL = @SQL + ',L.LocationName AS LocationName' + @CRLF
			SET @SQL = @SQL + ',PPO.RemainingQuantity AS RemainingQuantity' + @CRLF
			SET @SQL = @SQL + ',PP.Quantity AS PurchaseQuantity' + @CRLF
			SET @SQL = @SQL + ',PP.Price AS Price' + @CRLF
			SET @SQL = @SQL + ',PP.Amount AS Amount' + @CRLF
			SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
			SET @SQL = @SQL + ',CASE WHEN (PP.WebOrderNumber IS NULL OR PP.WebOrderNumber = '''') THEN PPO.WebOrderNumber ELSE PP.WebOrderNumber END AS WebOrderNumber' + @CRLF	--Add 2017/11/06 arc yano #3808
			SET @SQL = @SQL + ',PP.SupplierCode AS SupplierCode' + @CRLF
			SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
			SET @SQL = @SQL + ',PP.ReceiptNumber AS ReceiptNumber'+ @CRLF
			SET @SQL = @SQL + ',PP.PurchaseType AS PurchaseType' + @CRLF
			SET @SQL = @SQL + ',PP.Memo' + @CRLF
			SET @SQL = @SQL + ',PP.ChangePartsFlag AS ChangePartsFlag' + @CRLF
			SET @SQL = @SQL + ',PP.OrderPartsNumber AS OrderPartsNumber' + @CRLF --Add 2017/08/10 arc yano #3783

			SET @SQL = @SQL + 'FROM #temp_PartsPurchase AS PP' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchaseOrder AS PPO ON PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber AND PPO.PartsNumber = PP.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
			
			--Mod 2016/06/28 arc yano
			IF(@PurchaseStatus = '001')
			BEGIN
				--SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.PartsLocation AS PL ON PL.PartsNumber = PP.PartsNumber AND PL.DepartmentCode = @DepartmentCode'+ @CRLF
				SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsLocation AS PL ON PL.PartsNumber = PP.PartsNumber'+ @CRLF
				SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Location AS L ON PL.LocationCode = L.LocationCode'+ @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Location AS L ON PP.LocationCode = L.LocationCode'+ @CRLF
			END
			SET @SQL = @SQL + 'WHERE ISNULL(PP.ChangePartsFlag, ''0'') <> ''1'''+ @CRLF

			--SET @SQL = @SQL +'ORDER BY PP.PurchaseOrderNumber desc' + @CRLF

			SET @SQL = @SQL +'UNION' + @CRLF
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
			SET @SQL = @SQL + ',PP.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
			SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
			SET @SQL = @SQL + ',PP.PartsNumber AS PartsNumber' + @CRLF
			SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
			
			--Mod 2016/06/28 arc yano
			IF(@PurchaseStatus = '001')
			BEGIN
				SET @SQL = @SQL + ',PL.LocationCode AS LocationCode' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL + ',PP.LocationCode AS LocationCode' + @CRLF		
			END
			
			SET @SQL = @SQL + ',L.LocationName AS LocationName' + @CRLF
			SET @SQL = @SQL + ',NULL AS RemainingQuantity' + @CRLF
			SET @SQL = @SQL + ',PP.Quantity AS PurchaseQuantity' + @CRLF
			SET @SQL = @SQL + ',PP.Price AS Price' + @CRLF
			SET @SQL = @SQL + ',PP.Amount AS Amount' + @CRLF
			SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PP.WebOrderNumber AS WebOrderNumber' + @CRLF		--Add 2017/11/06 arc yano #3808
			SET @SQL = @SQL + ',PP.SupplierCode AS SupplierCode' + @CRLF
			SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
			SET @SQL = @SQL + ',PP.ReceiptNumber AS ReceiptNumber'+ @CRLF
			SET @SQL = @SQL + ',PP.PurchaseType AS PurchaseType' + @CRLF
			SET @SQL = @SQL + ',PP.Memo' + @CRLF
			SET @SQL = @SQL + ',PP.ChangePartsFlag AS ChangePartsFlag' + @CRLF
			SET @SQL = @SQL + ',PP.OrderPartsNumber AS OrderPartsNumber' + @CRLF --Add 2017/08/10 arc yano #3783

			SET @SQL = @SQL + 'FROM #temp_PartsPurchase AS PP' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchaseOrder AS PPO ON PPO.PurchaseOrderNumber = PP.PurchaseOrderNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PP.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PP.SupplierCode'+ @CRLF
			--Mod 2016/06/28 arc yano
			IF(@PurchaseStatus = '001')
			BEGIN
				--Mod 2016/08/13 arc yano
				--SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.PartsLocation AS PL ON PL.PartsNumber = PP.PartsNumber AND PL.DepartmentCode = @DepartmentCode'+ @CRLF
				SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsLocation AS PL ON PL.PartsNumber = PP.PartsNumber'+ @CRLF
				SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Location AS L ON PL.LocationCode = L.LocationCode'+ @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Location AS L ON PP.LocationCode = L.LocationCode'+ @CRLF
			END
			SET @SQL = @SQL + 'WHERE ISNULL(PP.ChangePartsFlag, ''0'') = ''1'''+ @CRLF

			SET @SQL = @SQL +'ORDER BY PP.PurchaseOrderNumber desc' + @CRLF
		END
		--�����`�[�ԍ����L�[�������ꍇ
		ELSE
		BEGIN
			SET @SQL = @SQL + 'SELECT' + @CRLF
			SET @SQL = @SQL + ' PP.PurchaseNumber AS PurchaseNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.PurchaseOrderNumber AS PurchaseOrderNumber' + @CRLF
			SET @SQL = @SQL + ',PPO.ServiceSlipNumber AS SlipNumber' + @CRLF
			SET @SQL = @SQL + ',PP.InvoiceNo AS InvoiceNo' + @CRLF
			SET @SQL = @SQL + ',PPO.PartsNumber AS PartsNumber' + @CRLF
			SET @SQL = @SQL + ',P.PartsNameJp AS PartsNameJp' + @CRLF
			SET @SQL = @SQL + ',PL.LocationCode AS LocationCode' + @CRLF
			SET @SQL = @SQL + ',L.LocationName AS LocationName' + @CRLF
			SET @SQL = @SQL + ',PPO.RemainingQuantity AS RemainingQuantity' + @CRLF			
			SET @SQL = @SQL + ',PP.Quantity AS PurchaseQuantity' + @CRLF
			SET @SQL = @SQL + ',PP.Price AS Price' + @CRLF
			SET @SQL = @SQL + ',PP.Amount AS Amount' + @CRLF
			SET @SQL = @SQL + ',PP.MakerOrderNumber AS MakerOrderNumber' + @CRLF
			SET @SQL = @SQL + ',CASE WHEN (PP.WebOrderNumber IS NULL OR PP.WebOrderNumber = '''') THEN PPO.WebOrderNumber ELSE PP.WebOrderNumber END AS WebOrderNumber' + @CRLF	--Add 2017/11/06 arc yano #3808
			SET @SQL = @SQL + ',PPO.SupplierCode AS SupplierCode' + @CRLF
			SET @SQL = @SQL + ',S.SupplierName AS SupplierName' + @CRLF
			SET @SQL = @SQL + ',PP.ReceiptNumber AS ReceiptNumber'+ @CRLF
			SET @SQL = @SQL + ',PP.PurchaseType AS PurchaseType' + @CRLF
			SET @SQL = @SQL + ',PP.Memo' + @CRLF
			SET @SQL = @SQL + ',PP.ChangePartsFlag AS ChangePartsFlag' + @CRLF
			SET @SQL = @SQL + ',PP.OrderPartsNumber AS OrderPartsNumber' + @CRLF --Add 2017/08/10 arc yano #3783


			SET @SQL = @SQL + 'FROM #temp_PartsPurchaseOrder AS PPO' + @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchase AS PP ON PP.PurchaseOrderNumber = PPO.PurchaseOrderNumber AND PP.PartsNumber = PPO.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Parts AS P ON P.PartsNumber = PPO.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS S ON S.SupplierCode = PPO.SupplierCode'+ @CRLF
			--Mod 2016/08/13 arc yano #3596
			--SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.PartsLocation AS PL ON PL.PartsNumber = PPO.PartsNumber AND PL.DepartmentCode = @DepartmentCode'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsLocation AS PL ON PL.PartsNumber = PPO.PartsNumber'+ @CRLF
			SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Location AS L ON PL.LocationCode = L.LocationCode'+ @CRLF
		END
		--PRINT @SQL
		EXECUTE sp_executeSQL @SQL, @PARAM
--*/
		/*	
		--�e�[�u���\��
		SELECT
			 CONVERT(nvarchar(50),'') AS PurchaseNumber
			,CONVERT(nvarchar(50),'') AS PurchaseOrderNumber
			,CONVERT(nvarchar(50),'') AS SlipNumber
			,CONVERT(nvarchar(50),'') AS InvoiceNo
			,CONVERT(nvarchar(25),'') AS PartsNumber
			,CONVERT(nvarchar(50),'') AS PartsNameJp
			,CONVERT(nvarchar(12),'') AS LocationCode
			,CONVERT(nvarchar(50),'') AS LocationName
			,CONVERT(decimal(10, 2), null) AS RemainingQuantity
			,CONVERT(decimal(10, 2), null) AS PurchaseQuantity
			,CONVERT(decimal(10, 0), null) AS Price
			,CONVERT(decimal(10, 0), null) AS Amount
			,CONVERT(nvarchar(50),'') AS MakerOrderNumber
			,CONVERT(nvarchar(50),'') AS WebOrderNumber
			,CONVERT(nvarchar(10),'') AS SupplierCode
			,CONVERT(nvarchar(80),'') AS SupplierName
			,CONVERT(nvarchar(50),'') AS ReceiptNumber
			,CONVERT(nvarchar(3),'') AS PurchaseType
			,CONVERT(nvarchar(100),'') AS Memo
			,CONVERT(nvarchar(2),'') AS ChangePartsFlag
			,CONVERT(nvarchar(25),'') AS OrderPartsNumber
		*/

END




GO


