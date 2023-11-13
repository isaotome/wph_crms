USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetServiceSalesReport]    Script Date: 2023/08/12 9:47:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--2015/09/17 arc nakayama #3165_�T�[�r�X�W�v�\ �V�K�쐬


CREATE PROCEDURE [dbo].[GetServiceSalesReport]

	@SalesDateFrom nvarchar(10),	--�[�ԓ��N��From
	@SalesDateTo nvarchar(10),		--�[�ԓ��N��To
	@WoerkType nvarchar(2),			--�敪(�P�F�Г��@�Q�F�Г�)
	@DepartmentCode nvarchar(3)		--����R�[�h
AS

BEGIN
	BEGIN TRY
		--temp�e�[�u���폜
		DROP TABLE #temp_CodeName_016
		DROP TABLE #temp_ServiceSalesHeader
		DROP TABLE #temp_ServiceSalesLine
		DROP TABLE #temp_ServiceSalesLine2
		DROP TABLE #temp_ServiceSalesLine3
		DROP TABLE #temp_ServiceSalesReport
	END TRY
	BEGIN CATCH
		--����
	END CATCH

	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; --�_�[�e�B�[���[�h�ݒ�

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)


	/*-------------------------------------------*/
	/* �敪�i�Г�/�ЊO�j�����}�X�^�擾			 */
	/*-------------------------------------------*/
	CREATE TABLE #temp_CodeName_016 (
		[Code] NVARCHAR(3)
	,	[Name] NVARCHAR(50)
	)
	INSERT INTO #temp_CodeName_016
	SELECT [Code] 
		  ,[Name]
	FROM [c_CodeName]
	WHERE [CategoryCode] = '016'

	/*-------------------------------------------*/
	/* �T�[�r�X�`�[�̏��擾					 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_ServiceSalesHeader (
		 SlipNumber nvarchar(50)
	   , RevisionNumber int 
	   , ServiceOrderStatus nvarchar(3)
	   , SalesDate datetime
	   , DepartmentCode nvarchar(3)
	   , FrontEmployeeCode nvarchar(50)
	   , WorkingEndDate datetime
	   , QuoteDate datetime
	   , ArrivalPlanDate datetime

		)

		SET @PARAM = '@SalesDateFrom nvarchar(10), @SalesDateTo nvarchar(10), @DepartmentCode nvarchar(3), @WoerkType nvarchar(2)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_ServiceSalesHeader' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + '	 h.SlipNumber' + @CRLF
		SET @SQL = @SQL + ', h.RevisionNumber' + @CRLF
		SET @SQL = @SQL + ', h.ServiceOrderStatus' + @CRLF
		SET @SQL = @SQL + ', h.SalesDate' + @CRLF
		SET @SQL = @SQL + ', h.DepartmentCode' + @CRLF
		SET @SQL = @SQL + ', h.FrontEmployeeCode' + @CRLF
		SET @SQL = @SQL + ', h.WorkingEndDate' + @CRLF
		SET @SQL = @SQL + ', h.QuoteDate' + @CRLF
		SET @SQL = @SQL + ', h.ArrivalPlanDate' + @CRLF
    
		SET @SQL = @SQL + 'FROM dbo.ServiceSalesHeader h' + @CRLF
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'      h.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL +'  AND h.ServiceOrderStatus = ''006''' + @CRLF --�[�ԍς݂̂�
				
		IF ((@SalesDateFrom is not null) AND (@SalesDateFrom <> '') AND ISDATE(@SalesDateFrom) = 1)
			IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
			BEGIN
				SET @SalesDateTo = DateAdd(d, 1, @SalesDateTo)
				SET @SQL = @SQL +'AND h.SalesDate >= @SalesDateFrom AND h.SalesDate < @SalesDateTo' + @CRLF
			END
			ELSE
			BEGIN
				SET @SQL = @SQL +'AND SalesDate = @SalesDateFrom' + @CRLF 
			END
		ELSE
			IF ((@SalesDateTo is not null) AND (@SalesDateTo <> '') AND ISDATE(@SalesDateTo) = 1)
			BEGIN
				SET @SalesDateTo = DateAdd(d, 1, @SalesDateTo)
				SET @SQL = @SQL +'AND SalesDate < @SalesDateTo' + @CRLF 
			END
		
		IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <>''))
		BEGIN
			SET @SQL = @SQL + 'AND DepartmentCode = @DepartmentCode'+ @CRLF
		END

		IF ((@WoerkType IS NOT NULL) AND (@WoerkType <>''))
		BEGIN
			SET @SQL = @SQL + 'AND EXISTS(SELECT 1 FROM dbo.ServiceSalesLine L WHERE L.DelFlag = ''0'' AND LEFT(L.ServiceWorkCode, 1) = @WoerkType AND h.SlipNumber = L.SlipNumber AND h.RevisionNumber = L.RevisionNumber)'+ @CRLF
		END

		EXECUTE sp_executeSQL @SQL, @PARAM, @SalesDateFrom, @SalesDateTo, @DepartmentCode, @WoerkType
		

	/*-------------------------------------------*/
	/* ���׏��擾								 */
	/*-------------------------------------------*/


	CREATE TABLE #temp_ServiceSalesLine (
		 SlipNumber nvarchar(50)
	   , RevisionNumber int
	   , ServiceWorkCode nvarchar(5)
	   , ServiceType nvarchar(3)
	   , ServiceMenuCode nvarchar(11)
	   , CustomerClaimCode nvarchar(10)
	   , PartsNumber nvarchar(25)
	   , Amount decimal(10,0)
	   , TechnicalFeeAmount decimal(10,0)
	   , TaxAmount decimal(10,0)
	   , cost decimal(10,0)
		)

		SET @PARAM = '@WoerkType nvarchar(2)'
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_ServiceSalesLine' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' L.SlipNumber' + @CRLF
		SET @SQL = @SQL + ',L.RevisionNumber' + @CRLF
		SET @SQL = @SQL + ',L.ServiceWorkCode' + @CRLF
		SET @SQL = @SQL + ',L.ServiceType' + @CRLF
		SET @SQL = @SQL + ',L.ServiceMenuCode' + @CRLF
		SET @SQL = @SQL + ',L.CustomerClaimCode' + @CRLF
		SET @SQL = @SQL + ',L.PartsNumber' + @CRLF
		SET @SQL = @SQL + ',CASE WHEN L.amount IS NULL THEN 0 ELSE L.amount END' + @CRLF
		SET @SQL = @SQL + ',CASE WHEN L.TechnicalFeeAmount IS NULL THEN 0 ELSE L.TechnicalFeeAmount END' + @CRLF
		SET @SQL = @SQL + ',CASE WHEN L.TaxAmount IS NULL THEN 0 ELSE L.TaxAmount END' + @CRLF
		SET @SQL = @SQL + ',CASE WHEN L.cost IS NULL THEN 0 ELSE L.Cost END' + @CRLF

		SET @SQL = @SQL + 'FROM #temp_ServiceSalesHeader H' + @CRLF
		SET @SQL = @SQL + 'INNER JOIN dbo.ServiceSalesLine L ON L.SlipNumber = H.SlipNumber AND L.RevisionNumber = H.RevisionNumber' + @CRLF
		SET @SQL = @SQL +' AND L.DelFlag = ''0''' + @CRLF
		SET @SQL = @SQL +' AND L.ServiceType in (''001'', ''002'', ''003'')'  + @CRLF --��ʂ����ƁA�T�[�r�X���j���[�A���i�ōi��
		SET @SQL = @SQL +' AND L.ServiceWorkCode <> ''99001'' '  + @CRLF --���Ƃ���u�W�v�O�v�����O
		IF ((@WoerkType IS NOT NULL) AND (@WoerkType <>''))
		BEGIN
			SET @SQL = @SQL + 'AND LEFT(L.ServiceWorkCode, 1) = @WoerkType'+ @CRLF
		END

		EXECUTE sp_executeSQL @SQL, @PARAM, @WoerkType


	/*-------------------------------------------*/
	/* ���׏��擾2(��������߂�)				 */
	/*-------------------------------------------*/


	CREATE TABLE #temp_ServiceSalesLine2 (
		 SlipNumber nvarchar(50)
	   , RevisionNumber int
	   , ServiceWorkCode nvarchar(5)
	   , ServiceType nvarchar(3)
	   , ServiceMenuCode nvarchar(11)
	   , CustomerClaimCode nvarchar(10)
	   , PartsNumber nvarchar(25)
	   , Amount decimal(10,0)
	   , TaxAmount decimal(10,0)
	   , cost decimal(10,0)

		)

		SET @PARAM = ''
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_ServiceSalesLine2' + @CRLF
		SET @SQL = @SQL + ' SELECT' + @CRLF
		SET @SQL = @SQL + ' SlipNumber' + @CRLF
		SET @SQL = @SQL + ',RevisionNumber' + @CRLF
		SET @SQL = @SQL + ',ServiceWorkCode' + @CRLF
		SET @SQL = @SQL + ',ServiceType' + @CRLF
		SET @SQL = @SQL + ',ServiceMenuCode' + @CRLF
		SET @SQL = @SQL + ',CustomerClaimCode' + @CRLF
		SET @SQL = @SQL + ',PartsNumber' + @CRLF
		SET @SQL = @SQL + ',CASE WHEN LEFT(ServiceMenuCode, 6) = ''DISCNT'' OR LEFT(PartsNumber, 6) = ''DISCNT'' THEN (Amount + TechnicalFeeAmount) * - 1 ELSE Amount + TechnicalFeeAmount END AS Amount' + @CRLF
		SET @SQL = @SQL + ',CASE WHEN LEFT(ServiceMenuCode, 6) = ''DISCNT'' OR LEFT(PartsNumber, 6) = ''DISCNT'' THEN (TaxAmount) * - 1 ELSE TaxAmount END AS TaxAmount' + @CRLF
		SET @SQL = @SQL + ',cost' + @CRLF


		SET @SQL = @SQL + 'FROM #temp_ServiceSalesLine' + @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM

		CREATE INDEX ix_temp_ServiceSalesLine2 ON #temp_ServiceSalesLine2(SlipNumber, RevisionNumber, ServiceWorkCode, CustomerClaimCode)



	/*-------------------------------------------------------*/
	/* ���׏��擾3(���Ɩ��ɍH���ƕ��i�̍��v�����߂�)	 */
	/*-------------------------------------------------------*/


	CREATE TABLE #temp_ServiceSalesLine3 (
		 SlipNumber nvarchar(50)
	   , RevisionNumber int
	   , ServiceWorkCode nvarchar(5)
	   , CustomerClaimCode nvarchar(10)
	   , ServiceAmount decimal(10,0) --�H������
	   , PartsAmount decimal(10,0)	 --���i����
	   , ServiceCost decimal(10,0)   --�H������
	   , PartsCost decimal(10,0)     --���i����
	   , TaxAmount decimal(10,0)
	   , ServiceTypeCode nchar(1)	--2:�Г�/1�ЊO
		)

		SET @PARAM = ''
		SET @SQL = ''
		SET @SQL = @SQL + 'INSERT INTO #temp_ServiceSalesLine3' + @CRLF
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' SlipNumber' + @CRLF
		SET @SQL = @SQL + ',RevisionNumber' + @CRLF
		SET @SQL = @SQL + ',ServiceWorkCode' + @CRLF
		SET @SQL = @SQL + ',CustomerClaimCode' + @CRLF
		SET @SQL = @SQL + ',SUM(Case when ServiceType=''002'' THEN Amount else 0 end) as ServiceAmount' + @CRLF
		SET @SQL = @SQL + ',SUM(Case when ServiceType=''003'' THEN Amount else 0 end) as PartsAmount' + @CRLF
		SET @SQL = @SQL + ',SUM(Case when ServiceType=''002'' THEN Cost else 0 end) as ServiceCost' + @CRLF
		SET @SQL = @SQL + ',SUM(Case when ServiceType=''003'' THEN Cost else 0 end) as PartsCost' + @CRLF
		SET @SQL = @SQL + ',SUM(TaxAmount)' + @CRLF
		SET @SQL = @SQL + ',LEFT(ServiceWorkCode, 1) AS ServiceTypeCode' + @CRLF
		SET @SQL = @SQL + 'FROM #temp_ServiceSalesLine2' + @CRLF
		SET @SQL = @SQL + 'GROUP BY SlipNumber, RevisionNumber, ServiceWorkCode, CustomerClaimCode' + @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM

		CREATE INDEX ix_temp_ServiceSalesLine3 ON #temp_ServiceSalesLine3(SlipNumber, RevisionNumber)


		/*-------------------------------------------*/
		/* �T�[�r�X�W�v�擾							 */
		/*-------------------------------------------*/

		SET @PARAM = ''
		SET @SQL = ''
		SET @SQL = @SQL + 'SELECT' + @CRLF
		SET @SQL = @SQL + ' H.SalesDate AS SalesDate' + @CRLF
		SET @SQL = @SQL + ',H.QuoteDate AS QuoteDate' + @CRLF
		SET @SQL = @SQL + ',H.WorkingEndDate AS WorkingEndDate' + @CRLF
		SET @SQL = @SQL + ',H.ArrivalPlanDate AS ArrivalPlanDate' + @CRLF
		SET @SQL = @SQL + ',H.SlipNumber AS SlipNumber' + @CRLF
		SET @SQL = @SQL + ',L.ServiceWorkCode AS ServiceWorkCode' + @CRLF
		SET @SQL = @SQL + ',CN.Name AS WorkTypeName' + @CRLF
		SET @SQL = @SQL + ',SW.Name AS ServiceWorkName' + @CRLF
		SET @SQL = @SQL + ',H.DepartmentCode AS DepartmentCode' + @CRLF
		SET @SQL = @SQL + ',D.DepartmentName AS DepartmentName' + @CRLF
		SET @SQL = @SQL + ',ISNULL(L.ServiceAmount, 0) AS ServiceAmount' + @CRLF
		SET @SQL = @SQL + ',ISNULL(L.PartsAmount, 0) AS PartsAmount' + @CRLF
		SET @SQL = @SQL + ',ISNULL(L.ServiceAmount + L.PartsAmount, 0) AS TotalAmount' + @CRLF
		SET @SQL = @SQL + ',ISNULL(L.TaxAmount, 0) AS TaxAmount' + @CRLF
		SET @SQL = @SQL + ',ISNULL(L.ServiceCost, 0) AS ServiceCost' + @CRLF
		SET @SQL = @SQL + ',ISNULL(L.PartsCost, 0) AS PartsCost' + @CRLF
		SET @SQL = @SQL + ',ISNULL(L.ServiceCost + L.PartsCost, 0) AS TotalCost' + @CRLF
		SET @SQL = @SQL + ',ISNULL(L.ServiceAmount - L.ServiceCost, 0) AS ServiceProfits' + @CRLF
		SET @SQL = @SQL + ',ISNULL(L.PartsAmount - L.PartsCost, 0) AS PartsProfits' + @CRLF
		SET @SQL = @SQL + ',ISNULL((L.ServiceAmount - L.ServiceCost) + (L.PartsAmount - L.PartsCost), 0) AS TotalProfits' + @CRLF
		/* Add 2023/05/15 openwave #4131 */
		SET @SQL = @SQL + ',ISNULL(H.TaxFreeFiledValue, 0) AS TotalTaxFreeCost' + @CRLF
		SET @SQL = @SQL + ',ISNULL(H.SubscriptionFee/1.1, 0) AS TotalSubscriptionBase' + @CRLF
		SET @SQL = @SQL + ',ISNULL(H.SubscriptionFee - (H.SubscriptionFee/1.1), 0) AS TotalSubscriptionTax' + @CRLF
		SET @SQL = @SQL + ',ISNULL(H.SubscriptionFee, 0) AS TotalSubscriptionFee' + @CRLF
		SET @SQL = @SQL + ',H.SubscriptionFeeMemo AS SubscriptionFeeMemo' + @CRLF
		SET @SQL = @SQL + ',ISNULL(H.TaxableFreeFieldValue/1.1, 0) AS TotalTaxableBase' + @CRLF
		SET @SQL = @SQL + ',ISNULL(H.TaxableFreeFieldValue - (H.TaxableFreeFieldValue/1.1), 0) AS TotalTaxableTax' + @CRLF
		SET @SQL = @SQL + ',ISNULL(H.TaxableFreeFieldValue, 0) AS TotalTaxableCost' + @CRLF
		SET @SQL = @SQL + ',H.TaxableFreeFieldName AS TaxableFreeFieldName' + @CRLF
		/* Add 2023/05/15 openwave #4131 */
		SET @SQL = @SQL + ',E.EmployeeName AS FrontEmployeeName' + @CRLF
		SET @SQL = @SQL + ',dbo.f_getMechanicEmployeeName(L.SlipNumber, L.RevisionNumber, L.ServiceWorkCode) AS MechanicEmployeeName' + @CRLF
		SET @SQL = @SQL + ',CC.CustomerClaimName AS CustomerClaimName' + @CRLF

		SET @SQL = @SQL + 'FROM #temp_ServiceSalesHeader AS H' + @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_ServiceSalesLine3 AS L ON L.SlipNumber = H.SlipNumber AND L.RevisionNumber = H.RevisionNumber'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS E ON E.EmployeeCode = H.FrontEmployeeCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.ServiceWork AS SW ON SW.ServiceWorkCode = L.ServiceWorkCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS D ON D.DepartmentCode = H.DepartmentCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.CustomerClaim AS CC ON CC.CustomerClaimCode = L.CustomerClaimCode'+ @CRLF
		SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CodeName_016 AS CN ON CN.Code = L.ServiceTypeCode'+ @CRLF

		SET @SQL = @SQL +'ORDER BY H.DepartmentCode, H.SalesDate' + @CRLF

		EXECUTE sp_executeSQL @SQL, @PARAM

END

GO


