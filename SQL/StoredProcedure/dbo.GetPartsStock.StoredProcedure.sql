USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsStock]    Script Date: 2018/12/11 13:03:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �V�K�쐬 
-- Update date
-- 2018/10/25 yano #3951���i�݌Ɍ����@���ˑ������Ō����ł��Ȃ�
-- 2018/05/14 arc yano #3880 ���㌴���v�Z�y�ђI���]���@�̕ύX
-- Description:	<Description,,>
-- ���i�݌ɏ��̎擾
-- ======================================================================================================================================
--

CREATE PROCEDURE [dbo].[GetPartsStock]

	@DepartmentCode nvarchar(3),		            --����R�[�h
	@WarehouseCode nvarchar(6),						--�q�ɃR�[�h
	@LocationCode nvarchar(12),						--���P�[�V�����R�[�h
	@LocationName nvarchar(50),						--���P�[�V������
	@PartsNumber nvarchar(25),						--���i�ԍ�
	@PartsNameJp nvarchar(50),						--���i��
	@StockZeroVisibility nvarchar(1)				--�݌ɂO�\���t���O(1:�݌ɐ��O���\���Ώ� 0:�݌ɐ��O�͕\���ΏۊO)
AS

BEGIN

/*	--�߂�l�̌^
	SELECT
		  '' AS PartsNumber
		, '' AS PartsNameJp
		, '' AS LocationCode
		, '' AS LocationName
		, '' AS LocationType
		, '' AS DepartmentCode
		, '' AS DepartmentName
		, '' AS WarehouseCode
		, '' AS WarehouseName
		, convert(decimal(10, 2), null) AS Quantity
		, convert(decimal(10, 2), null) AS ProvisionQuantity
		, convert(decimal(10, 0), null) AS Price
		, convert(decimal(10, 0), null) AS Cost
		, convert(decimal(10, 0), null) AS MovingAverageCost
*/

--/*
	SET NOCOUNT ON

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @ROWCNT INT = 0			--�s��

	/*-------------------------------------------*/
	/* ���i�݌ɏ��擾 �iPartsStock)			 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_PartsStock (
		 PartsNumber		   nvarchar(25)			--���i�ԍ�
	   , PartsNameJp		   nvarchar(50)			--���i��
	   , LocationCode		   nvarchar(12)			--���P�[�V�����R�[�h
	   , LocationName		   nvarchar(50)			--���P�[�V�����R�[�h
	   , LocationType		   nvarchar(3)			--���P�[�V�������
	   , WarehouseCode		   nvarchar(6)			--�q�ɃR�[�h
	   , Quantity			   decimal(10, 2)		--����
	   , ProvisionQuantity	   decimal(10, 2)		--�����ϐ���
	   , Price				   decimal(10, 0)		--���i
	   , Cost				   decimal(10, 0)		--����
	   , MovingAverageCost	   decimal(10, 0)		--�ړ����ϒP�� --Add -2018/05/14 arc yano #3880
		)

	BEGIN
		SET @PARAM = '@PartsNumber nvarchar(25),  @PartsNameJp nvarchar(50), @LocationCode nvarchar(12), @LocationName nvarchar(50), @WarehouseCode nvarchar(6), @StockZeroVisibility nvarchar(1)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsStock' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF		
		SET @SQL = @SQL + '  PS.PartsNumber' + @CRLF
		SET @SQL = @SQL + ', P.PartsNameJp' + @CRLF
		SET @SQL = @SQL + ', PS.LocationCode' + @CRLF
		SET @SQL = @SQL + ', L.LocationName' + @CRLF
		SET @SQL = @SQL + ', L.LocationType' + @CRLF
		SET @SQL = @SQL + ', L.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ', PS.Quantity' + @CRLF
		SET @SQL = @SQL + ', PS.ProvisionQuantity' + @CRLF
		SET @SQL = @SQL + ', P.Price' + @CRLF
		SET @SQL = @SQL + ', P.Cost' + @CRLF
		SET @SQL = @SQL + ', MAC.Price' + @CRLF
		SET @SQL = @SQL + ' FROM dbo.PartsStock AS PS' + @CRLF
		SET @SQL = @SQL + ' LEFT OUTER JOIN dbo.Location AS L ON PS.LocationCode = L.LocationCode' + @CRLF
		SET @SQL = @SQL + ' LEFT OUTER JOIN dbo.Parts AS P ON PS.PartsNumber = P.PartsNumber' + @CRLF
		SET @SQL = @SQL + ' LEFT OUTER JOIN dbo.PartsMovingAverageCost AS MAC ON PS.PartsNumber = MAC.PartsNumber' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + ' PS.DelFlag = ''0'''+ @CRLF
		SET @SQL = @SQL + ' AND L.DelFlag = ''0'''+ @CRLF
		SET @SQL = @SQL + ' AND P.DelFlag = ''0'''+ @CRLF
		SET @SQL = @SQL + ' AND MAC.DelFlag = ''0'''+ @CRLF
		
		IF ((@PartsNumber IS NOT NULL) AND (@PartsNumber <>''))													--���i�ԍ��ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND PS.PartsNumber like ''%' + @PartsNumber + '%'''+ @CRLF
		END

		IF ((@PartsNameJp IS NOT NULL) AND (@PartsNameJp <>''))													--���i���ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND P.PartsNameJp like N''%' + @PartsNameJp + '%'''+ @CRLF						--Mod 2018/10/25 yano #3951
		END

		IF ((@LocationCode IS NOT NULL) AND (@LocationCode <>''))												--���P�[�V�����R�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND PS.LocationCode like ''%' + @LocationCode + '%'''+ @CRLF
		END

		IF ((@LocationName IS NOT NULL) AND (@LocationName <>''))												--���P�[�V�������ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND L.LocationName like N''%' + @LocationName + '%'''+ @CRLF						--Mod 2018/10/25 yano #3951
		END

		IF ((@WarehouseCode IS NOT NULL) AND (@WarehouseCode <>''))												--�q�ɃR�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND L.WarehouseCode = @WarehouseCode'+ @CRLF
		END

		IF ((@StockZeroVisibility IS NOT NULL) AND (@StockZeroVisibility <> '1'))								--�݌ɂO�\���t���O�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND ISNULL(PS.Quantity, 0) <> 0'+ @CRLF
		END
		
		EXECUTE sp_executeSQL @SQL, @PARAM, @PartsNumber, @PartsNameJp, @LocationCode, @LocationName, @WarehouseCode, @StockZeroVisibility
		CREATE INDEX ix_temp_PartsStock ON #temp_PartsStock(PartsNumber, LocationCode, WarehouseCode)
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
			SET @SQL = @SQL + 'AND DW.WarehouseCode = @WarehouseCode'+ @CRLF
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
		SET @SQL = @SQL + '	PS.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PS.PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',PS.LocationCode' + @CRLF
		SET @SQL = @SQL + ',PS.LocationName' + @CRLF
		SET @SQL = @SQL + ',PS.LocationType' + @CRLF
		SET @SQL = @SQL + ',DW.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',DW.DepartmentName' + @CRLF
		SET @SQL = @SQL + ',PS.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ',DW.WarehouseName' + @CRLF
		SET @SQL = @SQL + ',PS.Quantity' + @CRLF
		SET @SQL = @SQL + ',PS.ProvisionQuantity' + @CRLF
		SET @SQL = @SQL + ',PS.Price' + @CRLF
		SET @SQL = @SQL + ',PS.Cost' + @CRLF
		SET @SQL = @SQL + ',PS.MovingAverageCost' + @CRLF
		SET @SQL = @SQL + 'FROM #temp_PartsStock AS PS ' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_DepartmentWarehouse AS DW ON PS.WarehouseCode = DW.WarehouseCode ' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '		1 = 1' + @CRLF
		
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))				--����R�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND DW.DepartmentCode IS NOT NULL AND DW.DepartmentCode = @DepartmentCode'+ @CRLF
		END

		SET @SQL = @SQL + 'ORDER BY'+ @CRLF
		SET @SQL = @SQL + 'PS.LocationCode, PS.PartsNumber'+ @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM, @DepartmentCode
	END

	BEGIN TRY
		--temp�e�[�u���폜
		DROP TABLE #temp_DepartmentWarehouse
		DROP TABLE #temp_PartsStock
	END TRY
	BEGIN CATCH
		--����
	END CATCH
--*/
END



GO


