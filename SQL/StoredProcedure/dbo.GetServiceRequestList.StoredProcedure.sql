USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetServiceRequestList]    Script Date: 2017/03/27 17:14:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--2017/02/21 arc nakayama #3626_【車】車両伝票の「作業依頼書」へ受注後に追加されない　新規作成

CREATE PROCEDURE [dbo].[GetServiceRequestList]

	@DepartmentCode nvarchar(3),			--依頼部門コード＜CarSalesHeaderを検索＞
	@EmployeeCode nvarchar(50),				--依頼担当者コード＜CarSalesHeaderを検索＞
	@CustomerName nvarchar(80),				--顧客名＜CarSalesHeaderを検索＞
	@Vin nvarchar(20),						--車台番号＜CarSalesHeaderを検索＞
	@ArrivalPlanDateFrom nvarchar(10),		--入庫予定日(YYYY/MM/dd)From＜CarPurchaseOrderを検索＞
	@ArrivalPlanDateTo nvarchar(10),		--入庫予定日(YYYY/MM)To＜CarPurchaseOrderを検索＞
	@DeliveryRequirementFrom nvarchar(10),	--希望納期From＜ServiceRequestを検索＞
	@DeliveryRequirementTo nvarchar(10)		--希望納期日To＜ServiceRequestを検索＞

AS
	SET NOCOUNT ON

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)
	
	/*-------------------------------------------*/
	/* ダーティーリードの設定					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/* 作業依頼テーブル絞込み		             */
	/*-------------------------------------------*/

	CREATE TABLE #temp_ServiceRequest_sr (
		  OriginalSlipNumber nvarchar(50)
		 ,DeliveryRequirement datetime
		 ,Memo nvarchar(100)

	)

	SET @PARAM = '@DeliveryRequirementFrom nvarchar(10), @DeliveryRequirementTo nvarchar(10)'
	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_ServiceRequest_sr' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'	  OriginalSlipNumber' + @CRLF
	SET @SQL = @SQL +'	, DeliveryRequirement' + @CRLF
	SET @SQL = @SQL +'	, Memo' + @CRLF
	SET @SQL = @SQL +'FROM' + @CRLF
	SET @SQL = @SQL +'	dbo.ServiceRequest' + @CRLF
	SET @SQL = @SQL +'WHERE' + @CRLF
	SET @SQL = @SQL +'    DelFlag = ''0''' + @CRLF 


	--登録年月日
	IF ((@DeliveryRequirementFrom is not null) AND (@DeliveryRequirementFrom <> '') AND ISDATE(@DeliveryRequirementFrom) = 1)
		IF ((@DeliveryRequirementTo is not null) AND (@DeliveryRequirementTo <> '') AND ISDATE(@DeliveryRequirementTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND DeliveryRequirement >= @DeliveryRequirementFrom AND DeliveryRequirement < DateAdd(d, 1, @DeliveryRequirementTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND DeliveryRequirement = @DeliveryRequirementFrom' + @CRLF 
		END
	ELSE
		IF ((@DeliveryRequirementTo is not null) AND (@DeliveryRequirementTo <> '') AND ISDATE(@DeliveryRequirementTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND DeliveryRequirement < DateAdd(d, 1, @DeliveryRequirementTo)' + @CRLF 
		END

	EXECUTE sp_executeSQL @SQL, @PARAM, @DeliveryRequirementFrom, @DeliveryRequirementTo
	CREATE INDEX ix_temp_ServiceRequest_sr ON #temp_ServiceRequest_sr(OriginalSlipNumber)

	/*-------------------------------------------*/
	/* 車両発注絞込み			　　			 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_CarPurchaseOrder_po (
		SlipNumber nvarchar(50)
	   ,ArrivalPlanDate datetime
    )

	SET @PARAM = '@ArrivalPlanDateFrom nvarchar(10), @ArrivalPlanDateTo nvarchar(10)'
	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_CarPurchaseOrder_po' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'  po.SlipNumber' + @CRLF
	SET @SQL = @SQL +', po.ArrivalPlanDate' + @CRLF
	SET @SQL = @SQL +'FROM'  + @CRLF
	SET @SQL = @SQL +'dbo.CarPurchaseOrder as po' + @CRLF
	SET @SQL = @SQL +'WHERE po.DelFlag = ''0''' + @CRLF
	SET @SQL = @SQL +'  AND EXISTS(SELECT 1 FROM #temp_ServiceRequest_sr AS sr WHERE sr.OriginalSlipNumber = po.SlipNumber)' + @CRLF

	--入庫予定日
	IF ((@ArrivalPlanDateFrom is not null) AND (@ArrivalPlanDateFrom <> '') AND ISDATE(@ArrivalPlanDateFrom) = 1)
		IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND ArrivalPlanDate >= @ArrivalPlanDateFrom AND ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF
		END
		ELSE
		BEGIN
			SET @SQL = @SQL +'AND ArrivalPlanDate = @ArrivalPlanDateFrom' + @CRLF 
		END
	ELSE
		IF ((@ArrivalPlanDateTo is not null) AND (@ArrivalPlanDateTo <> '') AND ISDATE(@ArrivalPlanDateTo) = 1)
		BEGIN
			SET @SQL = @SQL +'AND ArrivalPlanDate < DateAdd(d, 1, @ArrivalPlanDateTo)' + @CRLF 
		END

	EXECUTE sp_executeSQL @SQL, @PARAM, @ArrivalPlanDateFrom, @ArrivalPlanDateTo
	CREATE INDEX ix_temp_CarPurchaseOrder_po ON #temp_CarPurchaseOrder_po(SlipNumber)

	/*-------------------------------------------*/
	/* 依頼もとの車両伝伝票検索	　　			 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_CarSalesHeader_h (
		SlipNumber nvarchar(50)
	   ,DepartmentCode nvarchar(3)
	   ,EmployeeCode nvarchar(50)
	   ,CustomerName nvarchar(80)
	   ,CarName nvarchar(50)
	   ,Vin nvarchar(20)
    )

	SET @PARAM = '@DepartmentCode nvarchar(3), @EmployeeCode nvarchar(50), @CustomerName nvarchar(80), @Vin nvarchar(20)'
	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_CarSalesHeader_h' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'  h.SlipNumber' + @CRLF
	SET @SQL = @SQL +', h.DepartmentCode' + @CRLF
	SET @SQL = @SQL +', h.EmployeeCode' + @CRLF
	SET @SQL = @SQL +', c.CustomerName' + @CRLF
	SET @SQL = @SQL +', h.CarName' + @CRLF
	SET @SQL = @SQL +', h.Vin' + @CRLF
	SET @SQL = @SQL +'FROM'  + @CRLF
	SET @SQL = @SQL +'dbo.CarSalesHeader AS h' + @CRLF
	SET @SQL = @SQL + 'INNER JOIN dbo.Customer c ON c.CustomerCode = h.CustomerCode' + @CRLF
	SET @SQL = @SQL +'WHERE h.DelFlag = ''0''' + @CRLF
	SET @SQL = @SQL +'  AND EXISTS(SELECT 1 FROM #temp_CarPurchaseOrder_po AS po WHERE po.SlipNumber = h.SlipNumber)' + @CRLF

	--部門コード
	IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND h.DepartmentCode = @DepartmentCode' + @CRLF
	END

	--担当者コード
	IF ((@EmployeeCode IS NOT NULL) AND (@EmployeeCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND h.EmployeeCode = @EmployeeCode' + @CRLF
	END

	--顧客名
	IF ((@CustomerName IS NOT NULL) AND (@CustomerName <> ''))
	BEGIN
		SET @SQL = @SQL +'AND c.CustomerName LIKE ''%' + @CustomerName + '%''' + @CRLF
	END

	--車台番号
	IF ((@Vin IS NOT NULL) AND (@Vin <> ''))
	BEGIN
		SET @SQL = @SQL +'AND h.Vin LIKE ''%' + @Vin + '%''' + @CRLF
	END

	EXECUTE sp_executeSQL @SQL, @PARAM, @DepartmentCode, @EmployeeCode, @CustomerName, @Vin
	CREATE INDEX ix_temp_CarSalesHeader_h ON #temp_CarSalesHeader_h(SlipNumber)


	/*-------------------------------------------*/
	/* 車検案内発送先リスト取得					 */
	/*-------------------------------------------*/

	SET @PARAM = ''
	SET @SQL = ''
	SET @SQL = @SQL + 'SELECT'+ @CRLF
	SET @SQL = @SQL + '   h.DepartmentCode'+ @CRLF
	SET @SQL = @SQL + ' , d.DepartmentName'+ @CRLF
	SET @SQL = @SQL + '	, e.EmployeeName' + @CRLF
	SET @SQL = @SQL + '	, sr.OriginalSlipNumber' + @CRLF
	SET @SQL = @SQL + '	, h.CustomerName' + @CRLF
	SET @SQL = @SQL + '	, h.CarName' + @CRLF
	SET @SQL = @SQL + '	, h.Vin' + @CRLF
	SET @SQL = @SQL + '	, po.ArrivalPlanDate' + @CRLF
	SET @SQL = @SQL + '	, sr.DeliveryRequirement' + @CRLF
	SET @SQL = @SQL + '	, sr.Memo' + @CRLF
    SET @SQL = @SQL + 'FROM #temp_ServiceRequest_sr AS sr'+ @CRLF
	SET @SQL = @SQL + 'LEFT OUTER JOIN #temp_CarPurchaseOrder_po AS po ON po.SlipNumber = sr.OriginalSlipNumber' + @CRLF
	SET @SQL = @SQL + 'INNER JOIN #temp_CarSalesHeader_h AS h ON h.SlipNumber = po.SlipNumber' + @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Department AS d ON d.DepartmentCode = h.DepartmentCode'+ @CRLF
    SET @SQL = @SQL + 'LEFT OUTER JOIN dbo.Employee AS e ON e.EmployeeCode = h.EmployeeCode'+ @CRLF
	SET @SQL = @SQL + 'WHERE NOT EXISTS(SELECT 1 FROM dbo.ServiceSalesHeader AS sh WHERE sh.DelFlag = ''0'' AND sh.CarSlipNumber != NULL AND sh.CarSlipNumber = sr.OriginalSlipNumber)'+ @CRLF
	SET @SQL = @SQL + 'ORDER BY h.DepartmentCode, sr.OriginalSlipNumber desc'+ @CRLF

	EXECUTE sp_executeSQL @SQL, @PARAM


BEGIN

	BEGIN TRY
		DROP TABLE #temp_ServiceRequest_sr
		DROP TABLE #temp_CarPurchaseOrder_po
		DROP TABLE #temp_CarSalesHeader_h
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

END





GO


