USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsLocationList]    Script Date: 2016/10/03 15:20:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




--2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �V�K�쐬 
CREATE PROCEDURE [dbo].[GetPartsLocationList]

	@PartsNumber nvarchar(25),						--���i�ԍ�
	@DepartmentCode nvarchar(3),		            --����R�[�h
	@WarehouseCode nvarchar(6),			            --�q�ɃR�[�h
	@LocationCode nvarchar(12),						--���P�[�V�����R�[�h
	@DelFlag nvarchar(2)							--�폜�t���O
AS

BEGIN

	SET NOCOUNT ON

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @ROWCNT INT = 0			--�s��

	/*-------------------------------------------*/
	/* ���i���P�[�V�����擾 �iPartsLocation)	 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_PartsLocation (
		 PartsNumber       nvarchar(25)			--���i�ԍ�
	   , PartsNameJp	   nvarchar(50)			--���i��
	   , WarehouseCode     nvarchar(6)			--�q�ɃR�[�h
	   , WarehouseName     nvarchar(20)			--�q�ɖ�
	   , LocationCode      nvarchar(12)			--���P�[�V�����R�[�h
	   , LocationName      nvarchar(50)			--���P�[�V�����R�[�h
	   , DelFlag           nvarchar(2)			--�폜�t���O         
		)

	BEGIN
		SET @PARAM = '@PartsNumber nvarchar(25),  @WarehouseCode nvarchar(6), @LocationCode nvarchar(12), @DelFlag nvarchar(2)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_PartsLocation' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '  PL.PartsNumber' + @CRLF
		SET @SQL = @SQL + ', P.PartsNameJp' + @CRLF
		SET @SQL = @SQL + ', PL.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ', W.WarehouseName' + @CRLF
		SET @SQL = @SQL + ', PL.LocationCode' + @CRLF
		SET @SQL = @SQL + ', L.LocationName' + @CRLF
		SET @SQL = @SQL + ', PL.DelFlag' + @CRLF
		SET @SQL = @SQL + ' FROM dbo.PartsLocation AS PL' + @CRLF
		SET @SQL = @SQL + ' LEFT OUTER JOIN dbo.Warehouse AS W ON PL.WarehouseCode = W.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ' LEFT OUTER JOIN dbo.Location AS L ON PL.LocationCode = L.LocationCode' + @CRLF
		SET @SQL = @SQL + ' LEFT OUTER JOIN dbo.Parts AS P ON PL.PartsNumber = P.PartsNumber' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + ' 1 = 1 '+ @CRLF
		
		IF ((@PartsNumber IS NOT NULL) AND (@PartsNumber <>''))													--���i�ԍ��ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND PL.PartsNumber = @PartsNumber'+ @CRLF
		END

		IF ((@WarehouseCode IS NOT NULL) AND (@WarehouseCode <>''))												--�q�ɃR�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND PL.WarehouseCode = @WarehouseCode'+ @CRLF
		END

		IF ((@LocationCode IS NOT NULL) AND (@LocationCode <>''))												--���P�[�V�����R�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND PL.LocationCode = @LocationCode'+ @CRLF
		END
		
		IF ((@DelFlag IS NOT NULL) AND (@DelFlag <>''))															--�폜�t���O�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND PL.DelFlag = @DelFlag'+ @CRLF
		END
		
		EXECUTE sp_executeSQL @SQL, @PARAM, @PartsNumber, @WarehouseCode, @LocationCode, @DelFlag
		CREATE INDEX ix_temp_PartsLocation ON #temp_PartsLocation(PartsNumber, WarehouseCode, LocationCode)
	END


	/*-------------------------------------------*/
	/* ����E�q�ɑg�����}�X�^�擾				 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_DepartmentWarehouse (
	     DepartmentCode nvarchar(3)			--����R�[�h
	   , DepartmentName nvarchar(20)		--���喼
	   , WarehouseCode nvarchar(6)			--�q�ɃR�[�h
		)

	BEGIN
		SET @PARAM = '@DepartmentCode nvarchar(3), @WarehouseCode nvarchar(6)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_DepartmentWarehouse' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '	DW.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName' + @CRLF
		SET @SQL = @SQL + ',DW.WarehouseCode' + @CRLF
		SET @SQL = @SQL + 'FROM dbo.DepartmentWarehouse AS DW' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON DW.DepartmentCode = D.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '		DW.DelFlag = ''0''' + @CRLF
		
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
	/* ���i���P�[�V�������̎擾					 */
	/*-------------------------------------------*/
	BEGIN
		SET @PARAM = '@DepartmentCode nvarchar(3)'
		SET @SQL = ''
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '	PL.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',PL.PartsNameJp' + @CRLF
		SET @SQL = @SQL + ',DW.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',DW.DepartmentName' + @CRLF
		SET @SQL = @SQL + ',PL.WarehouseCode' + @CRLF
		SET @SQL = @SQL + ',PL.WarehouseName' + @CRLF
		SET @SQL = @SQL + ',PL.LocationCode' + @CRLF
		SET @SQL = @SQL + ',PL.LocationName' + @CRLF
		SET @SQL = @SQL + ',PL.DelFlag' + @CRLF
		SET @SQL = @SQL + 'FROM #temp_PartsLocation AS PL ' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_DepartmentWarehouse AS DW ON PL.WarehouseCode = DW.WarehouseCode ' + @CRLF
		SET @SQL = @SQL + ' WHERE' + @CRLF
		SET @SQL = @SQL + '		1 = 1' + @CRLF
		
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))				--����R�[�h�ɂ��i��
		BEGIN
			SET @SQL = @SQL + 'AND DW.DepartmentCode IS NOT NULL AND DW.DepartmentCode = @DepartmentCode'+ @CRLF
		END

		SET @SQL = @SQL + 'ORDER BY'+ @CRLF
		SET @SQL = @SQL + 'PL.PartsNumber, DW.DepartmentCode'+ @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM, @DepartmentCode
	END


	/*
	SELECT
		 PL.PartsNumber					--���i
		,PL.PartsNameJp					--���i��
		,DW.DepartmentCode				--����R�[�h
		,DW.DepartmentName				--���喼
		,PL.WarehouseCode				--�q�ɃR�[�h
		,PL.WarehouseName				--�q�ɖ�
		,PL.LocationCode				--���P�[�V�����R�[�h
		,PL.LocationName				--���P�[�V������
		,PL.DelFlag						--�폜�t���O
	FROM
		#temp_PartsLocation PL
	LEFT OUTER JOIN
		#temp_DepartmentWarehouse DW ON PL.WarehouseCode = DW.WarehouseCode 
	ORDER BY
		PL.PartsNumber,
		DW.DepartmentCode
	*/

	BEGIN TRY
		--temp�e�[�u���폜
		DROP TABLE #temp_DepartmentWarehouse
		DROP TABLE #temp_PartsLocation
	END TRY
	BEGIN CATCH
		--����
	END CATCH
	
END




GO


