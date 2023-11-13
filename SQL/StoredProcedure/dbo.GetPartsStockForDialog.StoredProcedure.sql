USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetPartsStockForDialog]    Script Date: 2016/10/03 15:35:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ��������(����R�[�h)�͕s�v�̂��߁A�p�~
CREATE PROCEDURE [dbo].[GetPartsStockForDialog]
	@MakerCode nvarchar(5),			--���[�J�[�R�[�h
	@MakerName nvarchar(50),		--���[�J�[��
	@CarBrandCode nvarchar(30),		--�u�����h�R�[�h
	@CarBrandName nvarchar(50),		--�u�����h��
	@PartsNumber nvarchar(25),		--�p�[�c�i���o�[
	@PartsNameJp nvarchar(50),		--�p�[�c��
	@DepartmentCode nvarchar(3),	--����R�[�h
	@SupplierCode nvarchar(10)		--�d����R�[�h

AS
	SET NOCOUNT ON
	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)
	
	/*-------------------------------------------*/
	/* �_�[�e�B�[���[�h�̐ݒ�					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;


	/*-------------------------------------------*/
	/* ���P�[�V�����e�[�u��						 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_Location (
	     LocationCode nvarchar(12) NOT NULL			--���P�[�V�����R�[�h
	   , WarehouseCode nvarchar(6) NOT NULL			--�q�ɃR�[�h
	   , LocationType nvarchar(3) NOT NULL			--���P�[�V�������
		)

	INSERT INTO #temp_Location
	SELECT
		 l.LocationCode								--���P�[�V�����R�[�h
		,l.WarehouseCode							--�q�ɃR�[�h
		,l.LocationType								--���P�[�V�������
	FROM
		dbo.Location l
	WHERE
		EXISTS
		(
			SELECT 'X' FROM dbo.DepartmentWarehouse dw WHERE dw.DepartmentCode = @DepartmentCode AND dw.DelFlag = '0' AND l.WarehouseCode = dw.WarehouseCode
		)
	CREATE INDEX ix_temp_Location ON #temp_Location(LocationCode, WarehouseCode)


	/*-------------------------------------------*/
	/* ���[�J�[���								 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_Maker_M (
		  MakerCode nvarchar(5)
		, MakerName nvarchar(50)

	)

	SET @PARAM = '@CarBrandCode nvarchar(30), @CarBrandName nvarchar(50), @MakerCode nvarchar(5), @MakerName nvarchar(50)'

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_Maker_M' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  M.MakerCode' + @CRLF
	SET @SQL = @SQL +'	, M.MakerName' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.Maker AS M' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    M.DelFlag = ''0''' + @CRLF
	
	
	--�u�����h�R�[�h
	IF ((@CarBrandCode IS NOT NULL) AND (@CarBrandCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND EXISTS(SELECT 1 FROM dbo.Brand AS B WHERE B.DelFlag = ''0'' AND B.CarBrandCode LIKE ''%' + @CarBrandCode + '%'' AND B.MakerCode = M.MakerCode)' + @CRLF
	END

	--�u�����h��
	IF ((@CarBrandName IS NOT NULL) AND (@CarBrandName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND EXISTS(SELECT 1 FROM dbo.Brand AS B WHERE B.DelFlag = ''0'' AND B.CarBrandName LIKE ''%' + @CarBrandName + '%'' AND B.MakerCode = M.MakerCode)' + @CRLF
	END
		
	--���[�J�[�R�[�h
	IF ((@MakerCode IS NOT NULL) AND (@MakerCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND M.MakerCode LIKE ''%' + @MakerCode + '%''' + @CRLF
	END

	--���[�J�[��
	IF ((@MakerName IS NOT NULL) AND (@MakerName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND M.MakerName LIKE ''%' + @MakerName + '%''' + @CRLF
	END

	SET @SQL = @SQL +'GROUP BY M.MakerCode, M.MakerName' + @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM, @CarBrandCode, @CarBrandName, @MakerCode, @MakerName
	CREATE INDEX ix_temp_Brand_B ON #temp_Maker_M(MakerCode)




	/*-------------------------------------------*/
	/* ���i���									 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_Parts_P (
		  PartsNumber nvarchar(25)
		, PartsNameJp nvarchar(50)
		, MakerCode nvarchar(5)
	)

	SET @PARAM = '@PartsNumber nvarchar(25), @PartsNameJp nvarchar(50), @MakerName nvarchar(5)'

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_Parts_P' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  P.PartsNumber' + @CRLF
	SET @SQL = @SQL +'	, P.PartsNameJp' + @CRLF
	SET @SQL = @SQL +'	, P.MakerCode' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.Parts AS P' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    P.DelFlag = ''0''' + @CRLF 

	--���i�ԍ�
	IF ((@PartsNumber IS NOT NULL) AND (@PartsNumber <> ''))
	BEGIN
		SET @SQL = @SQL +'AND P.PartsNumber LIKE ''%' + @PartsNumber + '%''' + @CRLF
	END
	--���i��
	IF ((@PartsNameJp IS NOT NULL) AND (@PartsNameJp <> ''))
	BEGIN
		SET @SQL = @SQL +'AND P.PartsNameJp LIKE ''%' + @PartsNameJp + '%''' + @CRLF
	END
	
	--���[�J�[�R�[�h
	IF ((@MakerCode IS NOT NULL) AND (@MakerCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND P.MakerCode LIKE ''%' + @MakerCode+ '%''' + @CRLF
	END

	EXECUTE sp_executeSQL @SQL, @PARAM, @PartsNumber, @PartsNameJp, @MakerCode

	/*-------------------------------------------*/
	/* �d�����т̂���d������					 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_PartsPurchase_PP (
		  SupplierCode nvarchar(10)
		, PartsNumber nvarchar(50)
	)

	SET @PARAM = '@SupplierCode nvarchar(10), @DepartmentCode nvarchar(3)'
	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_PartsPurchase_PP' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  PP.SupplierCode' + @CRLF
	SET @SQL = @SQL +'	, PP.PartsNumber' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.PartsPurchase AS PP' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    PP.DelFlag = ''0''' + @CRLF
	SET @SQL = @SQL +'AND PP.PurchaseStatus = ''002''' + @CRLF
	
	--����R�[�h
	SET @SQL = @SQL +'AND PP.DepartmentCode = @DepartmentCode' + @CRLF

	SET @SQL = @SQL + 'GROUP BY PP.SupplierCode, PP.PartsNumber' + @CRLF
	
	EXECUTE sp_executeSQL @SQL, @PARAM, @SupplierCode, @DepartmentCode
	CREATE INDEX ix_temp_PartsPurchase_PP ON #temp_PartsPurchase_PP(PartsNumber)


	/*-------------------------------------------*/
	/* �݌ɐ���									 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_PartsStock_PS (
		  PartsNumber nvarchar(25)
		, Quantity decimal(10,2)
	)

	SET @PARAM = ''
	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_PartsStock_PS' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  PS.PartsNumber' + @CRLF
	SET @SQL = @SQL +'	, SUM(ISNULL(PS.Quantity, 0)) - SUM(ISNULL(PS.ProvisionQuantity, 0)) AS Quantity' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.PartsStock AS PS' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    PS.DelFlag = ''0''' + @CRLF 
	--����R�[�h
	--SET @SQL = @SQL +'AND EXISTS(SELECT 1 FROM dbo.Location AS L WHERE L.LocationType = ''001'' AND L.DepartmentCode = @DepartmentCode AND L.LocationCode = PS.LocationCode)' + @CRLF
	SET @SQL = @SQL +'AND EXISTS(SELECT 1 FROM #temp_Location AS L WHERE L.LocationType = ''001'' AND L.LocationCode = PS.LocationCode)' + @CRLF		--Mod 2016/08/13 arc yano #3596

	
	SET @SQL = @SQL +'GROUP BY PS.PartsNumber' + @CRLF 

	EXECUTE sp_executeSQL @SQL, @PARAM
	CREATE INDEX ix_temp_PartsStock_PS ON #temp_PartsStock_PS(PartsNumber)

	/*-------------------------------------------*/
	/* ���i�݌Ɍ���								 */
	/*-------------------------------------------*/

	SET @PARAM = '@MakerName nvarchar(50), @SupplierCode nvarchar(10)'

	SET @SQL = ''
	SET @SQL = @SQL + 'SELECT'+ @CRLF
	SET @SQL = @SQL + '   M.MakerName AS MakerName'+ @CRLF
	SET @SQL = @SQL + ' , P.PartsNumber AS PartsNumber'+ @CRLF
	SET @SQL = @SQL + ' , P.PartsNameJp AS PartsNameJp'+ @CRLF
	SET @SQL = @SQL + ' , ISNULL(PS.Quantity, 0) AS Quantity'+ @CRLF
	SET @SQL = @SQL + ' , SP.SupplierName AS SupplierName'+ @CRLF

    SET @SQL = @SQL + 'FROM #temp_Parts_P AS P'+ @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsPurchase_PP AS PP ON PP.PartsNumber = P.PartsNumber'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_PartsStock_PS AS PS ON PS.PartsNumber = P.PartsNumber'+ @CRLF
	SET @SQL = @SQL + 'INNER JOIN #temp_Maker_M AS M ON M.MakerCode = P.MakerCode'+ @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Supplier AS SP ON SP.SupplierCode = PP.SupplierCode'+ @CRLF

	SET @SQL = @SQL + 'WHERE 1 = 1'+ @CRLF		

	--�d����R�[�h
	IF ((@SupplierCode IS NOT NULL) AND (@SupplierCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND PP.SupplierCode = @SupplierCode' + @CRLF
	END

	SET @SQL = @SQL + 'ORDER BY P.MakerCode, P.PartsNumber'+ @CRLF


	EXECUTE sp_executeSQL @SQL, @PARAM, @MakerName, @SupplierCode


BEGIN

	BEGIN TRY
		DROP TABLE #temp_Maker_M
		DROP TABLE #temp_Parts_P
		DROP TABLE #temp_PartsPurchase_PP
		DROP TABLE #temp_PartsStock_PS
	END TRY
	BEGIN CATCH
		--����
	END CATCH

END




GO


