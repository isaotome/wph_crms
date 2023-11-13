USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetLocationList]    Script Date: 2016/10/03 15:19:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





--2016/08/17 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �V�K�쐬 

CREATE PROCEDURE [dbo].[GetLocationList]

	@DepartmentCode nvarchar(3),		            --����R�[�h
	@DepartmentName nvarchar(20),		            --���喼
	@WarehouseCode nvarchar(6),			            --�q�ɃR�[�h
	@WarehouseName nvarchar(20),		            --�q�ɖ�
	@LocationCode nvarchar(12),						--���P�[�V�����R�[�h
	@LocationName nvarchar(50),						--���P�[�V������
	@LocationType nvarchar(3),						--���P�[�V�����^�C�v
	@DelFlag nvarchar(2)							--�폜�t���O
AS

BEGIN
	
	SET NOCOUNT ON

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @ROWCNT INT = 0			--�s��

	/*-------------------------------------------*/
	/* ���P�[�V�������擾 �iLocation)			 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_Location (
		 LocationCode      nvarchar(12)			--���P�[�V�����R�[�h
	   , LocationName      nvarchar(50)			--���P�[�V������
	   , LocationType      nvarchar(3)			--���P�[�V�������
	   , LocationTypeName  nvarchar(50)			--���P�[�V������ʖ�
	   , WarehouseCode     nvarchar(6)			--�q�ɃR�[�h
	   , WarehouseName     nvarchar(20)			--�q�ɖ�
	   , DelFlag           nvarchar(2)			--�폜�t���O         
		)

	BEGIN
		SET @PARAM = '@LocationCode nvarchar(12), @LocationName nvarchar(50), @LocationType nvarchar(3), @DelFlag nvarchar(2), @WarehouseCode nvarchar(6), @WarehouseName nvarchar(20)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_Location' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '  L.LocationCode' + @CRLF
		SET @SQL = @SQL + ', L.LocationName' + @CRLF
		SET @SQL = @SQL + ', L.LocationType' + @CRLF
		SET @SQL = @SQL + ', cLT.Name' + @CRLF
		SET @SQL = @SQL + ', L.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ', W.WarehouseName' + @CRLF
		SET @SQL = @SQL + ', L.DelFlag' + @CRLF
		SET @SQL = @SQL + ' FROM dbo.Location AS L' + @CRLF
		SET @SQL = @SQL + ' INNER JOIN dbo.c_LocationType AS cLT ON L.LocationType = cLT.Code' + @CRLF
		SET @SQL = @SQL + ' LEFT OUTER JOIN dbo.Warehouse AS W ON L.WarehouseCode = W.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ' WHERE cLT.DelFlag = ''0''' + @CRLF
		--SET @SQL = @SQL + ' AND W.DelFlag = ''0'''+ @CRLF
		
		IF ((@LocationCode IS NOT NULL) AND (@LocationCode <>''))				--���P�[�V�����R�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND L.LocationCode like ''%' + @LocationCode + '%''' + @CRLF
		END
		
		IF ((@LocationName IS NOT NULL) AND (@LocationName <>''))				--���P�[�V�������ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND L.LocationName like ''%' + @LocationName + '%''' + @CRLF
		END

		IF ((@LocationType IS NOT NULL) AND (@LocationType <>''))				--���P�[�V������ʂɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND L.LocationType = @LocationType'+ @CRLF
		END

		IF ((@DelFlag IS NOT NULL) AND (@DelFlag <>''))							--�폜�t���O�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND L.DelFlag = @DelFlag'+ @CRLF
		END
		
		IF ((@WarehouseCode IS NOT NULL) AND (@WarehouseCode <>''))					--�q�ɃR�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND W.WarehouseCode = @WarehouseCode'+ @CRLF
		END
		IF ((@WarehouseName IS NOT NULL) AND (@WarehouseName <>''))					--�q�ɖ��ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND W.WarehouseName like ''%' + @WarehouseName + '%'''+ @CRLF
		END
		
		EXECUTE sp_executeSQL @SQL, @PARAM, @LocationCode, @LocationName, @LocationType, @DelFlag, @WarehouseCode, @WarehouseName
		CREATE INDEX ix_temp_Location ON #temp_Location(LocationCode)
	END

	/*-------------------------------------------*/
	/* ����E�q�ɑg�����}�X�^�擾				 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_DepartmentWarehouse (
	     DepartmentCode nvarchar(3)			--����R�[�h
	   , DepartmentName nvarchar(20)		--���喼
	   , WarehouseCode nvarchar(6)			--�q�ɃR�[�h
	   , BusinessType nvarchar(3)			--�Ɩ��敪
		)

	BEGIN
		SET @PARAM = '@DepartmentCode nvarchar(3), @DepartmentName nvarchar(20)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_DepartmentWarehouse' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '	DW.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName' + @CRLF
		SET @SQL = @SQL + ',DW.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ',D.BusinessType' + @CRLF
		SET @SQL = @SQL + 'FROM dbo.DepartmentWarehouse AS DW' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON DW.DepartmentCode = D.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '		DW.DelFlag = ''0''' + @CRLF
		
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))				--����R�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND DW.DepartmentCode = @DepartmentCode'+ @CRLF
		END
		IF ((@DepartmentName IS NOT NULL) AND (@DepartmentName <>''))				--���喼�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND D.DepartmentName like ''%' + @DepartmentName + '%'''+ @CRLF
		END
		
		EXECUTE sp_executeSQL @SQL, @PARAM, @DepartmentCode, @DepartmentName
		CREATE INDEX ix_temp_DepartmentWarehouse ON #temp_DepartmentWarehouse(WarehouseCode)
	END

	/*-------------------------------------------*/
	/* ���P�[�V�������̎擾					 */
	/*-------------------------------------------*/
	BEGIN
		SET @PARAM = '@DepartmentCode nvarchar(3), @DepartmentName nvarchar(20)'
		SET @SQL = ''
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '	DW.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',DW.DepartmentName' + @CRLF
		SET @SQL = @SQL + ',DW.BusinessType' + @CRLF
		SET @SQL = @SQL + ',L.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ',L.WarehouseName' + @CRLF
		SET @SQL = @SQL + ',L.LocationCode' + @CRLF
		SET @SQL = @SQL + ',L.LocationName' + @CRLF
		SET @SQL = @SQL + ',L.LocationTypeName' + @CRLF
		SET @SQL = @SQL + ',L.DelFlag' + @CRLF
		SET @SQL = @SQL + 'FROM #temp_Location AS L' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_DepartmentWarehouse AS DW ON L.WarehouseCode = DW.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '		1 = 1' + @CRLF
		
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))				--����R�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND DW.DepartmentCode IS NOT NULL AND DW.DepartmentCode = @DepartmentCode'+ @CRLF
		END
		IF ((@DepartmentName IS NOT NULL) AND (@DepartmentName <>''))				--���喼�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND DW.DepartmentName IS NOT NULL AND DW.DepartmentName like ''%' + @DepartmentName + '%''' + @CRLF
		END

		EXECUTE sp_executeSQL @SQL, @PARAM, @DepartmentCode, @DepartmentName
	END
	
	BEGIN TRY
		--temp�e�[�u���폜
		DROP TABLE #temp_DepartmentWarehouse
		DROP TABLE #temp_Location
		--DROP TABLE #temp_PartsPurchase_ChangeParts
	END TRY
	BEGIN CATCH
		--����
	END CATCH

	/*
	declare
	  @retDepC nvarchar(3) = '',
	  @retDepN nvarchar(20) = '',
	  @retBusinessType nvarchar(3) = '',
	  @retWareC nvarchar(6) = '',
	  @retWareN nvarchar(20) = '',
	  @retLocC nvarchar(12) = '',
	  @retLocN nvarchar(50) = '',
	  @retLocTypeName nvarchar(50) = '',
	  @retLocDelFlag nvarchar(2) = ''



	SELECT
	  @retDepC AS DepartmentCode
	 ,@retDepN AS DepartmentName
	 ,@retBusinessType AS BusinessType
	 ,@retWareC AS WarehouseCode
	 ,@retWareN AS WarehouseName
	 ,@retLocC AS LocationCode
	 ,@retLocN AS LocationName
	 ,@retLocTypeName AS LocationTypeName
	 ,@retLocDelFlag AS DelFlag
	 */
END



GO


