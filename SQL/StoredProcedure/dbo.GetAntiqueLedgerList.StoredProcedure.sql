USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetAntiqueLedgerList]    Script Date: 2018/12/11 13:02:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/07 arc yano #3731 �T�u�V�X�e���@�\�ڍs(�Õ��䒠)
-- Update date
-- 2018/10/25 yano #3947 �ԗ��d�����́@���͍��ځi�Õ����������̊m�F���@�j�̒ǉ�
-- Description:	<Description,,>
-- �w�茎�̌Õ��䒠�̕\��
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetAntiqueLedgerList]
	  @TargetDateFrom datetime				--�Ώ۔N����From
	, @Searched nvarchar(1)					--�����Ώ�
AS
	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)
	
	
	/*-------------------------------------------*/
	/* �_�[�e�B�[���[�h�̐ݒ�					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/*�d���f�[�^								 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_CarPurchase (
		 [SalesCarNumber] nvarchar(50)
		,[PurchaseDate] datetime
		,[PurchaseStatus] nvarchar(3)
		,[SupplierCode] nvarchar(10)
		,[Amount] decimal
		,[EmployeeCode] nvarchar(50)
		,[CarPurchaseType] nvarchar(3)
	)
--/*	
	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_CarPurchase' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  SalesCarNumber' + @CRLF
	SET @SQL = @SQL +'	, PurchaseDate' + @CRLF
	SET @SQL = @SQL +'	, PurchaseStatus' + @CRLF
    SET @SQL = @SQL +'	, SupplierCode' + @CRLF
    SET @SQL = @SQL +'	, Amount' + @CRLF
    SET @SQL = @SQL +'	, EmployeeCode' + @CRLF
	SET @SQL = @SQL +'	, CarPurchaseType' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.CarPurchase' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    PurchaseStatus = ''002''' + @CRLF	--�d���X�e�[�^�X=�u�d���ρv
	SET @SQL = @SQL +'  AND DelFlag = ''0''' + @CRLF			

	
	--���������Ŏd�������I������Ă���ꍇ
	IF (@Searched = '0')
	BEGIN
		SET @SQL = @SQL + ' AND PurchaseDate >= CONVERT(datetime, ''' + CONVERT(nvarchar(10), @TargetDateFrom, 111) + ''')' + @CRLF
		SET @SQL = @SQL + ' AND PurchaseDate < CONVERT(datetime, ''' + CONVERT(nvarchar(10), DATEADD(m, 1, @TargetDateFrom), 111) + ''')' + @CRLF
	END

	EXECUTE sp_executeSQL @SQL
	CREATE INDEX ix_temp_CarPurchase ON #temp_CarPurchase(SalesCarNumber)

	/*-------------------------------------------*/
	/*�̔��f�[�^								 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_CarSalesHeader (
		 [SalesCarNumber] nvarchar(50)
		,[SalesDate] datetime
		,[CustomerCode] nvarchar(10)
		,[SalesType] nvarchar(3)
	)
	
	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_CarSalesHeader' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  SalesCarNumber' + @CRLF
	SET @SQL = @SQL +'	, SalesDate' + @CRLF
	SET @SQL = @SQL +'	, CustomerCode' + @CRLF
    SET @SQL = @SQL +'	, SalesType' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.CarSalesHeader' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'  SalesOrderStatus = ''005''' + @CRLF
	SET @SQL = @SQL +'  AND DelFlag = ''0''' + @CRLF			--�`�[�X�e�[�^�X=�u�[�ԍρv

	EXECUTE sp_executeSQL @SQL
	CREATE INDEX ix_temp_CarSalesHeader ON #temp_CarSalesHeader(SalesCarNumber)

	/*-------------------------------------------*/
	/*�ԗ��f�[�^								 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_SalesCar (
		 [SalesCarNumber] nvarchar(50)
		,[MakerName] nvarchar(50)
		,[ModelName] nvarchar(20)
		,[ManufacturingYear] nvarchar(10)
		,[ExteriorColorName] nvarchar(50)
		,[Mileage] decimal(12,2)
		,[Vin] nvarchar(20)
		,[MorterViecleOfficialCode] nvarchar(5)
		,[RegistrationNumberKana] nvarchar(1)
		,[RegistrationNumberPlate] nvarchar(4)
		,[RegistrationNumberType] nvarchar(3)
		,[CarGradeCode] nvarchar(30)
		,ConfirmDriverLicense bit			--Add 2018/10/25 yano #3947
		,ConfirmCertificationSeal bit		--Add 2018/10/25 yano #3947
		,ConfirmOther nvarchar(100)			--Add 2018/10/25 yano #3947
	)
	
	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_SalesCar' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  SalesCarNumber' + @CRLF
	SET @SQL = @SQL +'	, MakerName' + @CRLF
	SET @SQL = @SQL +'	, ModelName' + @CRLF
    SET @SQL = @SQL +'	, ManufacturingYear' + @CRLF
	SET @SQL = @SQL +'	, ExteriorColorName' + @CRLF
	SET @SQL = @SQL +'	, Mileage' + @CRLF
	SET @SQL = @SQL +'	, Vin' + @CRLF
	SET @SQL = @SQL +'	, MorterViecleOfficialCode' + @CRLF
	SET @SQL = @SQL +'	, RegistrationNumberKana' + @CRLF
	SET @SQL = @SQL +'	, RegistrationNumberPlate' + @CRLF
	SET @SQL = @SQL +'	, RegistrationNumberType' + @CRLF
	SET @SQL = @SQL +'	, CarGradeCode' + @CRLF
	SET @SQL = @SQL +'	, ConfirmDriverLicense' + @CRLF				--Add 2018/10/25 yano #3947
	SET @SQL = @SQL +'	, ConfirmCertificationSeal' + @CRLF			--Add 2018/10/25 yano #3947
	SET @SQL = @SQL +'	, ConfirmOther' + @CRLF						--Add 2018/10/25 yano #3947
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.SalesCar' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'  NewUsedType = ''U''' + @CRLF
	SET @SQL = @SQL +'  AND DelFlag = ''0''' + @CRLF

	EXECUTE sp_executeSQL @SQL
	CREATE INDEX ix_temp_SalesCar ON #temp_SalesCar(SalesCarNumber)

	/*-------------------------------------------*/
	/* �Õ��䒠									 */
	/*-------------------------------------------*/
	SET @SQL = ''
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  P.PurchaseDate AS PurchaseDate' + @CRLF
	SET @SQL = @SQL +'	, B.Name AS PurchaseStatus' + @CRLF
	SET @SQL = @SQL +'	, S.MakerName AS MakerName' + @CRLF
    SET @SQL = @SQL +'	, W.CarName AS CarName' + @CRLF
	SET @SQL = @SQL +'	, S.ModelName AS ModelName' + @CRLF
	SET @SQL = @SQL +'	, S.ManufacturingYear AS ManufacturingYear' + @CRLF
	SET @SQL = @SQL +'	, S.ExteriorColorName AS ExteriorColorName' + @CRLF
	SET @SQL = @SQL +'	, S.Mileage AS Mileage' + @CRLF
	SET @SQL = @SQL +'	, P.Amount AS Amount' + @CRLF
	SET @SQL = @SQL +'	, S.SalesCarNumber AS SalesCarNumber' + @CRLF
	SET @SQL = @SQL +'	, S.Vin AS Vin' + @CRLF
	SET @SQL = @SQL +'	, E.EmployeeName AS EmployeeName' + @CRLF
	SET @SQL = @SQL +'	, ISNULL(S.MorterViecleOfficialCode,'''') + '' '' + ISNULL(S.RegistrationNumberKana,'''') + '' '' + ISNULL(S.RegistrationNumberPlate,'''') + '' '' + isnull(S.RegistrationNumberType,'''') as Registration' + @CRLF
	SET @SQL = @SQL +'	, A.SupplierName AS SupplierName' + @CRLF
	SET @SQL = @SQL +'	, A.PostCode AS S_PostCode' + @CRLF
	SET @SQL = @SQL +'	, A.Prefecture AS S_Prefecture' + @CRLF
	SET @SQL = @SQL +'	, A.City AS S_City' + @CRLF
	SET @SQL = @SQL +'	, A.Address1 AS S_Address1' + @CRLF
	SET @SQL = @SQL +'	, A.Address2 AS S_Address2' + @CRLF
	SET @SQL = @SQL +'	, C3.Name AS SalesTypeName' + @CRLF
	SET @SQL = @SQL +'	, H.SalesDate AS SalesDate' + @CRLF
	SET @SQL = @SQL +'	, C.CustomerName AS CustomerName' + @CRLF
	SET @SQL = @SQL +'	, C.Prefecture AS C_Prefecture' + @CRLF
	SET @SQL = @SQL +'	, C.City AS C_City' + @CRLF
	SET @SQL = @SQL +'	, C.Address1 AS C_Address1' + @CRLF
	SET @SQL = @SQL +'	, C.Address2 AS C_Address2' + @CRLF
	SET @SQL = @SQL +'	, D.Name AS OccupationName' + @CRLF
	SET @SQL = @SQL +'	, C2.Birthday AS Birthday' + @CRLF
	SET @SQL = @SQL +'	, S.ConfirmDriverLicense AS ConfirmDriverLicense' + @CRLF				--Add 2018/10/25 yano #3947
	SET @SQL = @SQL +'	, S.ConfirmCertificationSeal AS ConfirmCertificationSeal' + @CRLF		--Add 2018/10/25 yano #3947
	SET @SQL = @SQL +'	, S.ConfirmOther AS ConfirmOther' + @CRLF								--Add 2018/10/25 yano #3947
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	#temp_CarPurchase P INNER JOIN' + @CRLF
	SET @SQL = @SQL +'	#temp_SalesCar S ON P.SalesCarNumber = S.SalesCarNumber LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Supplier A on P.SupplierCode = A.SupplierCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	#temp_CarSalesHeader H ON P.SalesCarNumber = H.SalesCarNumber LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Customer C on H.CustomerCode = C.CustomerCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	V_CarMaster W on S.CarGradeCode=W.CarGradeCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Employee E on P.EmployeeCode=E.EmployeeCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	Customer C2 on C2.CustomerCode=A.SupplierCode LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_CarPurchaseType B on P.CarPurchaseType=B.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_SalesType C3 on H.SalesType=C3.Code LEFT JOIN' + @CRLF
	SET @SQL = @SQL +'	c_Occupation D on C2.Occupation=D.Code' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'  1 = 1' + @CRLF
	--���������Ŕ[�ԓ����I������Ă���ꍇ
	IF (@Searched = '1')
	BEGIN
		SET @SQL = @SQL + ' AND SalesDate >= CONVERT(datetime, ''' + CONVERT(nvarchar(10), @TargetDateFrom, 111) + ''')' + @CRLF
		SET @SQL = @SQL + ' AND SalesDate < CONVERT(datetime, ''' + CONVERT(nvarchar(10), DATEADD(m, 1, @TargetDateFrom), 111) + ''')' + @CRLF
	END
	SET @SQL = @SQL +'ORDER BY' + @CRLF
	SET @SQL = @SQL +'  P.PurchaseDate' + @CRLF
	
	EXECUTE sp_executeSQL @SQL

	SELECT
		 P.PurchaseDate
		,B.Name AS PurchaseStatus
		,S.MakerName
		,W.CarName
		,S.ModelName
		,S.ManufacturingYear
		,S.ExteriorColorName
		,S.Mileage
		,P.Amount
		,S.SalesCarNumber
		,S.Vin
		,E.EmployeeName
		,isnull(S.MorterViecleOfficialCode,'') + ' ' + isnull(S.RegistrationNumberKana,'') + ' ' + isnull(S.RegistrationNumberPlate,'') + ' ' + isnull(S.RegistrationNumberType,'') as Registration
		,A.SupplierName
		,A.PostCode as S_PostCode
		,A.Prefecture as S_Prefecture
		,A.City as S_City
		,A.Address1 as S_Address1
		,A.Address2 as S_Address2
		,C3.Name as SalesTypeName
		,H.SalesDate
		,C.CustomerName
		,C.Prefecture as C_Prefecture
		,C.City as C_City
		,C.Address1 as C_Address1
		,C.Address2 as C_Address2
		,D.Name as OccupationName
		,C2.Birthday
		,S.ConfirmDriverLicense					--Add 2018/10/25 yano #3947
		,S.ConfirmCertificationSeal				--Add 2018/10/25 yano #3947
		,S.ConfirmOther							--Add 2018/10/25 yano #3947
	FROM
		#temp_CarPurchase P INNER JOIN 
		#temp_SalesCar S ON P.SalesCarNumber = S.SalesCarNumber LEFT JOIN
		Supplier A on P.SupplierCode = A.SupplierCode LEFT JOIN
		#temp_CarSalesHeader H ON P.SalesCarNumber = H.SalesCarNumber LEFT JOIN
		Customer C on H.CustomerCode = C.CustomerCode LEFT JOIN
		V_CarMaster W on S.CarGradeCode=W.CarGradeCode LEFT JOIN
		Employee E on P.EmployeeCode=E.EmployeeCode LEFT JOIN
		Customer C2 on C2.CustomerCode=A.SupplierCode LEFT JOIN
		c_CarPurchaseType B on P.CarPurchaseType=B.Code LEFT JOIN
		c_SalesType C3 on H.SalesType=C3.Code LEFT JOIN
		c_Occupation D on C2.Occupation=D.Code
	ORDER BY
		P.PurchaseDate

	BEGIN	
	BEGIN TRY
		DROP TABLE #temp_CarPurchase
		DROP TABLE #temp_CarSalesHeader
		DROP TABLE #temp_SalesCar
	END TRY
	BEGIN CATCH
		--����
	END CATCH
	END
	--*/

	/*
	SELECT
		 GETDATE() AS PurchaseDate
		,CONVERT(nvarchar(50), NULL) AS PurchaseStatus
		,CONVERT(nvarchar(50), NULL) AS MakerName
		,CONVERT(nvarchar(20), NULL) AS CarName
		,CONVERT(nvarchar(20), NULL) AS ModelName
		,CONVERT(nvarchar(10), NULL) AS ManufacturingYear
		,CONVERT(nvarchar(50), NULL) AS ExteriorColorName
		,CONVERT(decimal(12, 2), NULL) AS Mileage
		,CONVERT(decimal(10, 0), '0') AS Amount
		,CONVERT(nvarchar(50), '') AS SalesCarNumber
		,CONVERT(nvarchar(20), NULL) AS Vin
		,CONVERT(nvarchar(40), NULL) AS EmployeeName
		,CONVERT(nvarchar(20), NULL) AS Registration
		,CONVERT(nvarchar(80), NULL) AS SupplierName
		,CONVERT(nvarchar(8), NULL) AS S_PostCode
		,CONVERT(nvarchar(50), NULL) AS S_Prefecture
		,CONVERT(nvarchar(50), NULL) AS S_City
		,CONVERT(nvarchar(100), NULL) AS S_Address1
		,CONVERT(nvarchar(100), NULL) AS S_Address2
		,CONVERT(nvarchar(50), NULL) AS SalesTypeName
		,CONVERT(datetime, NULL) AS SalesDate
		,CONVERT(nvarchar(80), NULL) AS CustomerName
		,CONVERT(nvarchar(50), NULL) AS C_Prefecture
		,CONVERT(nvarchar(50), NULL) AS C_City
		,CONVERT(nvarchar(100), NULL) AS C_Address1
		,CONVERT(nvarchar(100), NULL) AS C_Address2
		,CONVERT(nvarchar(50), NULL) AS OccupationName
		,CONVERT(datetime, NULL) AS Birthday
		,CONVERT(bit, NULL) AS ConfirmDriverLicense
		,CONVERT(bit, NULL) AS ConfirmCertificationSeal
		,CONVERT(nvarchar(100), NULL) AS ConfirmOther
		*/




GO