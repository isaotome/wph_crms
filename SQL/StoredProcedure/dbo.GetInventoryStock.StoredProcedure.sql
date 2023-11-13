USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetInventoryStock]    Script Date: 2018/12/11 13:24:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--2018/10/25 yano #3951 ���ˑ������Ō����ł��Ȃ�
--2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �V�K�쐬 

CREATE PROCEDURE [dbo].[GetInventoryStock]

	@DepartmentCode nvarchar(3),		            --����R�[�h
	@WarehouseCode nvarchar(6),						--�q�ɃR�[
	@InventoryMonth datetime,						--�I����
	@LocationCode nvarchar(12),						--���P�[�V�����R�[�h
	@LocationName nvarchar(50),						--���P�[�V������
	@PartsNumber nvarchar(25),						--���i�ԍ�
	@PartsNameJp nvarchar(50),						--���i��
	@StockZeroVisibility nvarchar(1)				--�݌ɂO�\���t���O(1:�݌ɐ��O���\���Ώ� 0:�݌ɐ��O�͕\���ΏۊO)
AS

BEGIN
/*	--�߂�l�̌^
	SELECT
		  GETDATE() AS InventoryMonth
		, '' AS PartsNumber
		, '' AS LocationCode
		, CONVERT(decimal(10, 3), null) AS Quantity
		, CONVERT(decimal(10, 3), null) AS PhysicalQuantity
		, '' as Comment
		, CONVERT(decimal(10, 2), null) AS ProvisionQuantity
		, '' AS WarehouseCode
		, '' AS LocationName
		, '' AS LocationType
		, '' AS PartsNameJp
		, CONVERT(decimal(10, 0), null) AS AverageCost
		, CONVERT(decimal(10, 0), null) AS StandardPrice
		, '' AS DepartmentCode
		, '' AS DepartmentName
*/

--/*
	SET NOCOUNT ON

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @ROWCNT INT = 0			--�s��

	/*-------------------------------------------*/
	/* ���i�݌ɒI�����擾 �iInventoryStock)	 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_InventoryStock (
	     InventoryMonth		   datetime				--�I����
	   , PartsNumber		   nvarchar(25)			--���i�ԍ�
	   , LocationCode		   nvarchar(12)			--���P�[�V�����R�[�h
	   , Quantity			   decimal(10, 2)		--����
	   , PhysicalQuantity	   decimal(10, 2)		--���I��
	   , Comment			   nvarchar(200)		--�R�����g
	   , ProvisionQuantity	   decimal(10, 2)		--�����ϐ���
	   , WarehouseCode		   nvarchar(6)			--�q�ɃR�[�h
	   , LocationName		   nvarchar(50)			--���P�[�V������
	   , LocationType		   nvarchar(3)			--���P�[�V�������
	   , PartsNameJp		   nvarchar(50)			--���i��
	   , AverageCost		   decimal(10, 0)		--�ړ����ό���
	   , StandardPrice		   decimal(10, 0)		--�W�����i
		)

	BEGIN
		SET @PARAM = '@DepartmentCode nvarchar(3), @InventoryMonth datetime, @LocationCode nvarchar(12), @LocationName nvarchar(50), @PartsNumber nvarchar(25),  @PartsNameJp nvarchar(50),  @WarehouseCode nvarchar(6), @StockZeroVisibility nvarchar(1)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_InventoryStock' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF		
		SET @SQL = @SQL + '  IV.InventoryMonth' + @CRLF
		SET @SQL = @SQL + ', IV.PartsNumber' + @CRLF
		SET @SQL = @SQL + ', IV.LocationCode' + @CRLF
		SET @SQL = @SQL + ', IV.Quantity' + @CRLF
		SET @SQL = @SQL + ', IV.PhysicalQuantity' + @CRLF
		SET @SQL = @SQL + ', IV.Comment' + @CRLF
		SET @SQL = @SQL + ', IV.ProvisionQuantity' + @CRLF
		SET @SQL = @SQL + ', IV.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ', L.LocationName' + @CRLF
		SET @SQL = @SQL + ', L.LocationType' + @CRLF
		SET @SQL = @SQL + ', P.PartsNameJp' + @CRLF
		SET @SQL = @SQL + ', PA.Price AS AverageCost' + @CRLF
		SET @SQL = @SQL + ', P.Cost AS StandardPrice' + @CRLF
		SET @SQL = @SQL + ' FROM dbo.InventoryStock AS IV' + @CRLF
		SET @SQL = @SQL + '  LEFT OUTER JOIN dbo.Location AS L ON IV.LocationCode = L.LocationCode' + @CRLF
		SET @SQL = @SQL + '  LEFT OUTER JOIN dbo.Parts AS P ON IV.PartsNumber = P.PartsNumber' + @CRLF
		SET @SQL = @SQL + '  LEFT OUTER JOIN dbo.PartsAverageCost AS PA ON IV.InventoryMonth = PA.CloseMonth AND IV.PartsNumber = PA.PartsNumber' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '  IV.DelFlag = ''0'''+ @CRLF
		SET @SQL = @SQL + '  AND IV.InventoryType = ''002'''+ @CRLF
		
		/*
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))											--����R�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND IV.DepartmentCode = @DepartmentCode'+ @CRLF
		END
		*/
		IF (@InventoryMonth IS NOT NULL)																		--�I�����ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND IV.InventoryMonth = @InventoryMonth'+ @CRLF
		END

		IF ((@LocationCode IS NOT NULL) AND (@LocationCode <>''))												--���P�[�V�����R�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND IV.LocationCode like ''%' + @LocationCode + '%'''+ @CRLF
		END

		IF ((@LocationName IS NOT NULL) AND (@LocationName <>''))												--���P�[�V�������ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND L.LocationName like N''%' + @LocationName + '%'''+ @CRLF
		END

		IF ((@PartsNumber IS NOT NULL) AND (@PartsNumber <>''))													--���i�ԍ��ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND IV.PartsNumber like ''%' + @PartsNumber + '%'''+ @CRLF
		END

		IF ((@PartsNameJp IS NOT NULL) AND (@PartsNameJp <>''))													--���i���ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND P.PartsNameJp like N''%' + @PartsNameJp + '%'''+ @CRLF
		END

		IF ((@WarehouseCode IS NOT NULL) AND (@WarehouseCode <>''))												--�q�ɃR�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND IV.WarehouseCode like ''%' + @WarehouseCode + '%'''+ @CRLF
		END

		IF ((@StockZeroVisibility IS NOT NULL) AND (@StockZeroVisibility <> '1'))								--�݌ɂO�\���t���O�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND ISNULL(IV.Quantity, 0) <> 0'+ @CRLF
		END
		
		EXECUTE sp_executeSQL @SQL, @PARAM, @DepartmentCode, @InventoryMonth, @LocationCode, @LocationName,@PartsNumber, @PartsNameJp, @WarehouseCode, @StockZeroVisibility
		CREATE INDEX ix_temp_InventoryStock ON #temp_InventoryStock(InventoryMonth, PartsNumber, WarehouseCode)
	END

	/*-------------------------------------------*/
	/* ����E�q�ɑg�����}�X�^�擾				 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_DepartmentWarehouse (
	     DepartmentCode nvarchar(3)			--����R�[�h
	   , DepartmentName nvarchar(20)		--���喼
	   , WarehouseCode nvarchar(6)			--�q�ɃR�[�h
	   , WarehouseName nvarchar(20)			--�q�ɖ�
		)

	BEGIN
		SET @PARAM = '@DepartmentCode nvarchar(3), @WarehouseCode nvarchar(6)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_DepartmentWarehouse' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '	DW.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName' + @CRLF
		SET @SQL = @SQL + ',DW.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ',W.WarehouseName' + @CRLF
		SET @SQL = @SQL + 'FROM dbo.DepartmentWarehouse AS DW' + @CRLF
		SET @SQL = @SQL + 'INNER JOIN dbo.Department AS D ON DW.DepartmentCode = D.DepartmentCode' + @CRLF
		SET @SQL = @SQL + 'INNER JOIN dbo.Warehouse AS W ON DW.WarehouseCode = W.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '		DW.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '	AND D.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL + '	AND W.DelFlag = ''0''' + @CRLF
		
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))				--����R�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND DW.DepartmentCode = @DepartmentCode'+ @CRLF
		END
		IF ((@WarehouseCode IS NOT NULL) AND (@WarehouseCode <>''))					--�q�ɃR�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND DW.WarehouseCode like ''%' + @WarehouseCode + '%''' + @CRLF
		END

		EXECUTE sp_executeSQL @SQL, @PARAM, @DepartmentCode, @WarehouseCode 
		CREATE INDEX ix_temp_DepartmentWarehouse ON #temp_DepartmentWarehouse(WarehouseCode)
	END


	/*-------------------------------------------*/
	/* ���i�݌ɏ��̎擾						 */
	/*-------------------------------------------*/
	BEGIN
		SET @PARAM = '@DepartmentCode nvarchar(3)'
		SET @SQL = ''
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '  IV.InventoryMonth' + @CRLF
		SET @SQL = @SQL + ', IV.PartsNumber' + @CRLF
		SET @SQL = @SQL + ', IV.LocationCode' + @CRLF
		SET @SQL = @SQL + ', IV.Quantity' + @CRLF
		SET @SQL = @SQL + ', IV.PhysicalQuantity' + @CRLF
		SET @SQL = @SQL + ', IV.Comment' + @CRLF
		SET @SQL = @SQL + ', IV.ProvisionQuantity' + @CRLF
		SET @SQL = @SQL + ', IV.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ', IV.LocationName' + @CRLF
		SET @SQL = @SQL + ', IV.LocationType' + @CRLF
		SET @SQL = @SQL + ', IV.PartsNameJp' + @CRLF
		SET @SQL = @SQL + ', IV.AverageCost' + @CRLF
		SET @SQL = @SQL + ', IV.StandardPrice' + @CRLF
		SET @SQL = @SQL + ', DW.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ', DW.DepartmentName' + @CRLF
		SET @SQL = @SQL + 'FROM #temp_InventoryStock AS IV ' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_DepartmentWarehouse AS DW ON IV.WarehouseCode = DW.WarehouseCode ' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '		1 = 1' + @CRLF
		
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))				--����R�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND DW.DepartmentCode IS NOT NULL AND DW.DepartmentCode = @DepartmentCode'+ @CRLF
		END

		SET @SQL = @SQL + 'ORDER BY'+ @CRLF
		SET @SQL = @SQL + 'IV.LocationCode, IV.PartsNumber'+ @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM, @DepartmentCode
	END

	BEGIN TRY
		--temp�e�[�u���폜
		DROP TABLE #temp_DepartmentWarehouse
		DROP TABLE #temp_InventoryStock
	END TRY
	BEGIN CATCH
		--����
	END CATCH
	--*/
END


GO


