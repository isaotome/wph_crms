USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetInspectGuidList]    Script Date: 2015/12/21 9:39:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







--2015/07/24 arc nakayama �ڋqDM�Ή� �Ԍ��ē������惊�X�g�ǉ��@�V�K�쐬
--2015/12/16 arc ookubo   #3318_���[���A�h���X���o�͍��ڂɒǉ� 

CREATE PROCEDURE [dbo].[GetInspectGuidList]
	@InspectGuidFlag nvarchar(3),		 --�Ԍ��ē�
	@DepartmentCode2 nvarchar(3),		 --�c�ƒS������R�[�h���ڋq���
	@CarEmployeeCode nvarchar(50),		 --�c�ƕ���S���҃R�[�h���ڋq���
	@ServiceDepartmentCode2 nvarchar(3), --�T�[�r�X�S������R�[�h���ڋq���
	@MakerName nvarchar(50),			 --���[�J�[��
	@CarName nvarchar(20),				 --�Ԏ햼
	@FirstRegistrationDateFrom nvarchar(10), --���N�x�o�^(YYYY/MM)From
	@FirstRegistrationDateTo nvarchar(10),	 --���N�x�o�^(YYYY/MM)To 	
	@RegistrationDateFrom nvarchar(10),	 --�o�^�N����From
	@RegistrationDateTo nvarchar(10),	 --�o�^�N����To
	@NextInspectionDateFrom nvarchar(10), --����_����From
	@NextInspectionDateTo nvarchar(10),	 --����_����To
	@ExpireDateFrom nvarchar(10),		 --�Ԍ�������From
	@ExpireDateTo nvarchar(10),			 --�Ԍ�������To
	@CustomerAddressReconfirm bit,		 --�Z���Ċm�F
	@CustomerKind nvarchar(3)			 --�ڋq���

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
	/* �Z���Ċm�F�v�ە����}�X�^					 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_CodeName_013 (
		[Code] Bit
	,	[ShortName] NVARCHAR(50)
	)
	INSERT INTO #temp_CodeName_013
	SELECT CASE [Code] WHEN '001' THEN 1 ELSE 0 END
    ,	[ShortName]
	FROM [c_CodeName]
	WHERE [CategoryCode] = '013'

	/*-------------------------------------------*/
	/* �ڋq�Ɋ֘A����ԗ����擾                */
	/*-------------------------------------------*/

	CREATE TABLE #temp_SalesCar_S (
		  SalesCarNumber nvarchar(50)
		, CarGradeCode nvarchar(30)
		, MorterViecleOfficialCode nvarchar(5)
		, RegistrationNumberType  nvarchar(3)
		, RegistrationNumberKana nvarchar(1)
		, RegistrationNumberPlate nvarchar(4)
		, RegistrationDate datetime
		, FirstRegistrationYear nvarchar(9)
		, Vin nvarchar(20)
		, [ExpireDate] datetime
		, NextInspectionDate datetime
		, UserCode nvarchar(10)
		, InspectGuidFlag nvarchar(3)
		, InspectGuidMemo nvarchar(100)
		, FirstRegistrationDate datetime
		, DelFlag nvarchar(2)
		, CarWeight int
	)

	SET @PARAM = '@InspectGuidFlag nvarchar(3), @RegistrationDateFrom nvarchar(10), @RegistrationDateTo nvarchar(10), @NextInspectionDateFrom nvarchar(10), @NextInspectionDateTo nvarchar(10), @ExpireDateFrom nvarchar(10), @ExpireDateTo nvarchar(10), @FirstRegistrationDateFrom nvarchar(10), @FirstRegistrationDateTo nvarchar(10)'

	

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_SalesCar_S' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  SalesCarNumber' + @CRLF
	SET @SQL = @SQL +'	, CarGradeCode' + @CRLF
	SET @SQL = @SQL +'	, MorterViecleOfficialCode' + @CRLF
    SET @SQL = @SQL +'	, RegistrationNumberType' + @CRLF
    SET @SQL = @SQL +'	, RegistrationNumberKana' + @CRLF
    SET @SQL = @SQL +'	, RegistrationNumberPlate' + @CRLF
	SET @SQL = @SQL +'	, RegistrationDate' + @CRLF
	SET @SQL = @SQL +'	, FirstRegistrationYear' + @CRLF
	SET @SQL = @SQL +'	, Vin' + @CRLF
	SET @SQL = @SQL +'	, ExpireDate' + @CRLF
	SET @SQL = @SQL +'	, NextInspectionDate' + @CRLF
	SET @SQL = @SQL +'	, UserCode' + @CRLF
	SET @SQL = @SQL +'	, InspectGuidFlag' + @CRLF
	SET @SQL = @SQL +'	, InspectGuidMemo' + @CRLF
	SET @SQL = @SQL +'	, FirstRegistrationDate' + @CRLF
	SET @SQL = @SQL +'	, DelFlag' + @CRLF
	SET @SQL = @SQL +'	, CarWeight' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.SalesCar' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    DelFlag = ''0''' + @CRLF 

	--�Ԍ��ē�	
	IF ((@InspectGuidFlag != '000') AND (@InspectGuidFlag <> ''))
	BEGIN
		SET @SQL = @SQL +'AND InspectGuidFlag = @InspectGuidFlag' + @CRLF
	END

	--�o�^�N����
	IF ((@RegistrationDateFrom is not null) AND (@RegistrationDateFrom <> '') AND ISDATE(@RegistrationDateFrom) = 1)
		IF ((@RegistrationDateTo is not null) AND (@RegistrationDateTo <> '') AND ISDATE(@RegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND RegistrationDate >= @RegistrationDateFrom AND RegistrationDate < DateAdd(d, 1, @RegistrationDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND RegistrationDate = @RegistrationDateFrom' + @CRLF 
		END
	ELSE
		IF ((@RegistrationDateTo is not null) AND (@RegistrationDateTo <> '') AND ISDATE(@RegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND RegistrationDate < DateAdd(d, 1, @RegistrationDateTo)' + @CRLF 
		END

	--����_���� NextInspectionDate
	IF ((@NextInspectionDateFrom is not null) AND (@NextInspectionDateFrom <> '') AND ISDATE(@NextInspectionDateFrom) = 1)
		IF ((@NextInspectionDateTo is not null) AND (@NextInspectionDateTo <> '') AND ISDATE(@NextInspectionDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND NextInspectionDate >= @NextInspectionDateFrom AND NextInspectionDate < DateAdd(d, 1, @NextInspectionDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND NextInspectionDate = @NextInspectionDateFrom' + @CRLF 
		END
	ELSE
		IF ((@NextInspectionDateTo is not null) AND (@NextInspectionDateTo <> '') AND ISDATE(@NextInspectionDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND NextInspectionDate < DateAdd(d, 1, @NextInspectionDateTo)' + @CRLF 
		END

	--�Ԍ������� ExpireDate
	IF ((@ExpireDateFrom is not null) AND (@ExpireDateFrom <> '') AND ISDATE(@ExpireDateFrom) = 1)
		IF ((@ExpireDateTo is not null) AND (@ExpireDateTo <> '') AND ISDATE(@ExpireDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND ExpireDate >= @ExpireDateFrom AND ExpireDate < DateAdd(d, 1, @ExpireDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND ExpireDate = @ExpireDateFrom' + @CRLF 
		END
	ELSE
		IF ((@ExpireDateTo is not null) AND (@ExpireDateTo <> '') AND ISDATE(@ExpireDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND ExpireDate < DateAdd(d, 1, @ExpireDateTo)' + @CRLF 
		END

	--���N�x�o�^ FirstRegistrationDate
	IF ((@FirstRegistrationDateFrom is not null) AND (@FirstRegistrationDateFrom <> '') AND ISDATE(@FirstRegistrationDateFrom) = 1)
		IF ((@FirstRegistrationDateTo is not null) AND (@FirstRegistrationDateTo <> '') AND ISDATE(@FirstRegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND FirstRegistrationDate >= @FirstRegistrationDateFrom AND FirstRegistrationDate < DateAdd(d, 1, @FirstRegistrationDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND FirstRegistrationDate = @FirstRegistrationDateFrom' + @CRLF 
		END
	ELSE
		IF ((@FirstRegistrationDateTo is not null) AND (@FirstRegistrationDateTo <> '') AND ISDATE(@FirstRegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND FirstRegistrationDate < DateAdd(d, 1, @FirstRegistrationDateTo)' + @CRLF 
		END

	EXECUTE sp_executeSQL @SQL, @PARAM, @InspectGuidFlag, @RegistrationDateFrom, @RegistrationDateTo, @NextInspectionDateFrom, @NextInspectionDateTo, @ExpireDateFrom, @ExpireDateTo, @FirstRegistrationDateFrom, @FirstRegistrationDateTo
	CREATE INDEX ix_temp_SalesCar_S ON #temp_SalesCar_S(UserCode)	--(DelFlag)


	/*-------------------------------------------*/
	/* �ԗ��}�X�^�擾			�@�@			 */
	/*-------------------------------------------*/
	
	SET @PARAM = '@MakerName nvarchar(50), @CarName nvarchar(20)'
	CREATE TABLE #temp_CarMaster_VC (
	  MakerName nvarchar(50)
	, CarName nvarchar(50)
	, CarGradeCode nvarchar(30)
    )

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_CarMaster_VC' + @CRLF

	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'  M.MakerName' + @CRLF
	SET @SQL = @SQL +', CA.CarName' + @CRLF
	SET @SQL = @SQL +', CG.CarGradeCode' + @CRLF
	SET @SQL = @SQL +'FROM'  + @CRLF
	SET @SQL = @SQL +'dbo.Brand AS B' + @CRLF
	SET @SQL = @SQL +'INNER JOIN dbo.Maker AS M ON B.MakerCode = M.MakerCode' + @CRLF
	SET @SQL = @SQL +'INNER JOIN dbo.Car AS CA ON B.CarBrandCode = CA.CarBrandCode' + @CRLF
	SET @SQL = @SQL +'INNER JOIN dbo.CarGrade CG ON CA.CarCode = CG.CarCode' + @CRLF
	SET @SQL = @SQL +'WHERE 1=1' + @CRLF

	IF ((@MakerName IS NOT NULL) AND (@MakerName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND M.MakerName = @MakerName' + @CRLF
	END

	IF ((@CarName IS NOT NULL) AND (@CarName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND CA.CarName = @CarName' + @CRLF
	END

	EXECUTE sp_executeSQL @SQL, @PARAM, @MakerName, @CarName
	CREATE INDEX ix_temp_CarMaster_VC ON #temp_CarMaster_VC(CarGradeCode)

	/*-------------------------------------------*/
	/* �Ԍ��ē������惊�X�g�擾					 */
	/*-------------------------------------------*/

	SET @PARAM = '@InspectGuidFlag nvarchar(3), @DepartmentCode2 nvarchar(3),@CarEmployeeCode nvarchar(50), @ServiceDepartmentCode2 nvarchar(3),@MakerName nvarchar(50), @CarName nvarchar(20), @FirstRegistrationDateFrom nvarchar(10), @FirstRegistrationDateTo nvarchar(10), @RegistrationDateFrom nvarchar(10), @RegistrationDateTo nvarchar(10), @NextInspectionDateFrom nvarchar(10), @NextInspectionDateTo nvarchar(10), @ExpireDateFrom nvarchar(10), @ExpireDateTo nvarchar(10), @CustomerAddressReconfirm bit, @CustomerKind nvarchar(3)'

	SET @SQL = ''
	SET @SQL = @SQL + 'SELECT'+ @CRLF
	SET @SQL = @SQL + '   S.InspectGuidFlag'+ @CRLF
	SET @SQL = @SQL + ' , AO2.Name AS InspectGuidFlagName'+ @CRLF
	SET @SQL = @SQL + ' , S.InspectGuidMemo'+ @CRLF
	SET @SQL = @SQL + ' , C.CustomerCode'+ @CRLF
	SET @SQL = @SQL + ' , C.CustomerName'+ @CRLF
	SET @SQL = @SQL + ' , C.CustomerKind'+ @CRLF
	SET @SQL = @SQL + ' , CK.Name AS CustomerKindName'+ @CRLF
	SET @SQL = @SQL + ' , C.DepartmentCode AS DepartmentCode2'+ @CRLF
	SET @SQL = @SQL + ' , D.DepartmentName AS DepartmentName2'+ @CRLF
	SET @SQL = @SQL + ' , C.CarEmployeeCode'+ @CRLF
	SET @SQL = @SQL + ' , E1.EmployeeName AS CarEmployeeName'+ @CRLF
	SET @SQL = @SQL + ' , C.ServiceDepartmentCode AS ServiceDepartmentCode2'+ @CRLF
	SET @SQL = @SQL + ' , D4.DepartmentName AS ServiceDepartmentName2'+ @CRLF
	SET @SQL = @SQL + ' , C.ServiceEmployeeCode AS ServiceEmployeeCode2'+ @CRLF
	SET @SQL = @SQL + ' , E2.EmployeeName AS ServiceEmployeeName2'+ @CRLF
	SET @SQL = @SQL + ' , C.PostCode'+ @CRLF
	SET @SQL = @SQL + ' , C.Prefecture'+ @CRLF
	SET @SQL = @SQL + ' , C.City'+ @CRLF
	SET @SQL = @SQL + ' , C.Address1'+ @CRLF
	SET @SQL = @SQL + ' , C.Address2'+ @CRLF
	SET @SQL = @SQL + ' , C.TelNumber'+ @CRLF
	SET @SQL = @SQL + ' , C.MobileNumber'+ @CRLF
	SET @SQL = @SQL + ' , CN013.ShortName AS CustomerAddressReconfirmName'+ @CRLF
	SET @SQL = @SQL + ' , CD.FirstName'+ @CRLF
	SET @SQL = @SQL + ' , CD.LastName'+ @CRLF
	SET @SQL = @SQL + ' , CD.PostCode AS CDPostCode'+ @CRLF
	SET @SQL = @SQL + ' , CD.Prefecture AS CDPrefecture'+ @CRLF
	SET @SQL = @SQL + ' , CD.City AS CDCity'+ @CRLF
	SET @SQL = @SQL + ' , CD.Address1 AS CDAddress1'+ @CRLF
	SET @SQL = @SQL + ' , CD.Address2 AS CDAddress2'+ @CRLF
	SET @SQL = @SQL + ' , CD.TelNumber AS CDTelNumber'+ @CRLF
	SET @SQL = @SQL + ' , S.SalesCarNumber'+ @CRLF
	SET @SQL = @SQL + ' , S.MorterViecleOfficialCode'+ @CRLF
    SET @SQL = @SQL +'	, S.RegistrationNumberType' + @CRLF
    SET @SQL = @SQL +'	, S.RegistrationNumberKana' + @CRLF
    SET @SQL = @SQL +'	, S.RegistrationNumberPlate' + @CRLF
	SET @SQL = @SQL + ' , S.Vin'+ @CRLF
	SET @SQL = @SQL + ' , VC.MakerName'+ @CRLF
	SET @SQL = @SQL + ' , VC.CarName'+ @CRLF
	SET @SQL = @SQL + ' , S.FirstRegistrationYear'+ @CRLF
	SET @SQL = @SQL + ' , S.RegistrationDate'+ @CRLF
	SET @SQL = @SQL + ' , S.NextInspectionDate'+ @CRLF
	SET @SQL = @SQL + ' , S.ExpireDate'+ @CRLF
	SET @SQL = @SQL + ' , S.FirstRegistrationDate'+ @CRLF
	SET @SQL = @SQL + ' , S.CarWeight'+ @CRLF
	SET @SQL = @SQL + ' , CD.DelFlag AS DmDelFlag'+ @CRLF
	SET @SQL = @SQL + '	, C.MailAddress' + @CRLF
	SET @SQL = @SQL + '	, C.MobileMailAddress' + @CRLF
    SET @SQL = @SQL + 'FROM dbo.Customer AS C'+ @CRLF
	SET @SQL = @SQL + 'INNER JOIN #temp_SalesCar_S AS S ON C.CustomerCode = S.UserCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CarMaster_VC AS VC ON S.CarGradeCode = VC.CarGradeCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON C.DepartmentCode = D.DepartmentCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D4 ON C.ServiceDepartmentCode = D4.DepartmentCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E1 ON C.CarEmployeeCode = E1.EmployeeCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E2 ON C.ServiceEmployeeCode = E2.EmployeeCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.CustomerDM AS CD ON C.CustomerCode = CD.CustomerCode'+ @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_Allowance AS AO2 ON S.InspectGuidFlag = AO2.Code'+ @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.c_CustomerKind AS CK ON C.CustomerKind = CK.Code'+ @CRLF

	SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CodeName_013 AS CN013 ON C.AddressReconfirm = CN013.Code'+ @CRLF

	SET @SQL = @SQL + 'WHERE'+ @CRLF
	SET @SQL = @SQL + '   (C.DelFlag = ''0'')'+ @CRLF

	--�Ԍ��ē�	
	IF ((@InspectGuidFlag != '000') AND (@InspectGuidFlag <> ''))
	BEGIN
		SET @SQL = @SQL +'AND S.InspectGuidFlag = @InspectGuidFlag' + @CRLF
	END

	--�Z���Ċm�F
	IF((@CustomerAddressReconfirm IS NOT NULL))
	BEGIN
		SET @SQL = @SQL +'AND C.AddressReconfirm = @CustomerAddressReconfirm' + @CRLF
	END

	--�ڋq���
	IF ((@CustomerKind IS NOT NULL) AND (@CustomerKind <>''))
		BEGIN
			SET @SQL = @SQL + 'AND C.CustomerKind = @CustomerKind'+ @CRLF
		END

	IF ((@DepartmentCode2 IS NOT NULL) AND (@DepartmentCode2 <>''))
		BEGIN
			SET @SQL = @SQL + 'AND C.DepartmentCode = @DepartmentCode2'+ @CRLF
		END

	IF ((@CarEmployeeCode IS NOT NULL) AND (@CarEmployeeCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND C.CarEmployeeCode = @CarEmployeeCode'+ @CRLF
		END

	IF ((@ServiceDepartmentCode2 IS NOT NULL) AND (@ServiceDepartmentCode2 <>''))
		BEGIN
			SET @SQL = @SQL + 'AND C.ServiceDepartmentCode = @ServiceDepartmentCode2'+ @CRLF
		END
		
				
	IF ((@MakerName IS NOT NULL) AND (@MakerName <>''))
		BEGIN
			SET @SQL = @SQL + 'AND VC.MakerName = @MakerName'+ @CRLF
		END	

	IF ((@CarName IS NOT NULL) AND (@CarName <>''))
	BEGIN
		SET @SQL = @SQL + 'AND VC.CarName = @CarName'+ @CRLF
	END
	
	--�o�^�N����
	IF ((@RegistrationDateFrom is not null) AND (@RegistrationDateFrom <> '') AND ISDATE(@RegistrationDateFrom) = 1)
		IF ((@RegistrationDateTo is not null) AND (@RegistrationDateTo <> '') AND ISDATE(@RegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.RegistrationDate >= @RegistrationDateFrom AND S.RegistrationDate < DateAdd(d, 1, @RegistrationDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND S.RegistrationDate = @RegistrationDateFrom' + @CRLF 
		END
	ELSE
		IF ((@RegistrationDateTo is not null) AND (@RegistrationDateTo <> '') AND ISDATE(@RegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.RegistrationDate < DateAdd(d, 1, @RegistrationDateTo)' + @CRLF 
		END

	--����_���� NextInspectionDate
	IF ((@NextInspectionDateFrom is not null) AND (@NextInspectionDateFrom <> '') AND ISDATE(@NextInspectionDateFrom) = 1)
		IF ((@NextInspectionDateTo is not null) AND (@NextInspectionDateTo <> '') AND ISDATE(@NextInspectionDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.NextInspectionDate >= @NextInspectionDateFrom AND S.NextInspectionDate < DateAdd(d, 1, @NextInspectionDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND S.NextInspectionDate = @NextInspectionDateFrom' + @CRLF 
		END
	ELSE
		IF ((@NextInspectionDateTo is not null) AND (@NextInspectionDateTo <> '') AND ISDATE(@NextInspectionDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.NextInspectionDate < DateAdd(d, 1, @NextInspectionDateTo)' + @CRLF 
		END

	--�Ԍ������� ExpireDate
	IF ((@ExpireDateFrom is not null) AND (@ExpireDateFrom <> '') AND ISDATE(@ExpireDateFrom) = 1)
		IF ((@ExpireDateTo is not null) AND (@ExpireDateTo <> '') AND ISDATE(@ExpireDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.ExpireDate >= @ExpireDateFrom AND S.ExpireDate < DateAdd(d, 1, @ExpireDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND S.ExpireDate = @ExpireDateFrom' + @CRLF 
		END
	ELSE
		IF ((@ExpireDateTo is not null) AND (@ExpireDateTo <> '') AND ISDATE(@ExpireDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.ExpireDate < DateAdd(d, 1, @ExpireDateTo)' + @CRLF 
		END

	--���N�x�o�^ FirstRegistrationDate
	IF ((@FirstRegistrationDateFrom is not null) AND (@FirstRegistrationDateFrom <> '') AND ISDATE(@FirstRegistrationDateFrom) = 1)
		IF ((@FirstRegistrationDateTo is not null) AND (@FirstRegistrationDateTo <> '') AND ISDATE(@FirstRegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.FirstRegistrationDate >= @FirstRegistrationDateFrom AND S.FirstRegistrationDate < DateAdd(d, 1, @FirstRegistrationDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND S.FirstRegistrationDate = @FirstRegistrationDateFrom' + @CRLF 
		END
	ELSE
		IF ((@FirstRegistrationDateTo is not null) AND (@FirstRegistrationDateTo <> '') AND ISDATE(@FirstRegistrationDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND S.FirstRegistrationDate < DateAdd(d, 1, @FirstRegistrationDateTo)' + @CRLF 
		END


	SET @SQL = @SQL +'ORDER BY C.CustomerCode, S.SalesCarNumber' + @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM,@InspectGuidFlag, @DepartmentCode2,@CarEmployeeCode, @ServiceDepartmentCode2, @MakerName, @CarName, @FirstRegistrationDateFrom, @FirstRegistrationDateTo, @RegistrationDateFrom, @RegistrationDateTo, @NextInspectionDateFrom, @NextInspectionDateTo, @ExpireDateFrom, @ExpireDateTo, @CustomerAddressReconfirm, @CustomerKind


BEGIN

	BEGIN TRY
		DROP TABLE #temp_SalesCar_S
		DROP TABLE #temp_CarMaster_VC
		DROP TABLE #temp_CodeName_013 
	END TRY
	BEGIN CATCH
		--����
	END CATCH

END




GO


