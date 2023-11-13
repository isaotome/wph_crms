USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCarSalesHeaderList]    Script Date: 2023/10/17 16:15:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--2023/09/18 yano #4181【車両伝票】オプション区分追加（サーチャージ）
--2023/01/16 yano #4138【納車リスト】集計項目の追加（メンテナンスパッケージ、延長保証）
--2020/01/17 yano #4027 【納車リスト】サマリー画面から明細画面を表示する時にサマリー画面の絞り込み条件を引き継がない
--2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
--2017/03/09 arc nakayama #3723_納車リスト　新規作成

CREATE PROCEDURE [dbo].[GetCarSalesHeaderList]

	@SelectYear nvarchar(4),				--納車年（YYYY）
	@SelectMonth nvarchar(2),				--納車月（MM）
	@DepartmentCode nvarchar(3) = NULL,		--部門コード
	@NewUsedType nvarchar(3) = NULL,	    --新中区分
	@AAType nvarchar(3) = NULL				--AAタイプ(1:一般[個人、法人] 2:AA・業販[AA、業販、店間移動] 3:デモ・自登[自社登録、デモカ登録] 4:依廃・他(その他、依廃)

AS
--/*
	SET NOCOUNT ON

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @SalesDateFrom datetime
	DECLARE @SalesDateTo datetime

	--月がnullまたは空文字の場合その年度全て(xxxx/07/01～xxx△/06/30)が対象となる
	IF ((@SelectYear IS NOT NULL) AND (@SelectYear <> '') AND ((@SelectMonth IS NULL) OR (@SelectMonth = '')))
	BEGIN
		SET @SalesDateFrom = CONVERT(datetime, @SelectYear + '0701')
		SET @SalesDateTo = DATEADD(year, 1, @SalesDateFrom)
	END

	/*-------------------------------------------*/
	/* ダーティーリードの設定					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/* 納車年月で車両伝票を絞る					 */
	/*-------------------------------------------*/

	SET @PARAM = '@SelectYear nvarchar(4), @SelectMonth nvarchar(2), @DepartmentCode nvarchar(3), @NewUsedType nvarchar(3), @AAType nvarchar(3)'
	SET @SQL = ''
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL +'  H.SalesDate AS ''SalesDate''' + @CRLF
	SET @SQL = @SQL +'  ,Cn.Name AS ''NewUsedTypeName''' + @CRLF
	SET @SQL = @SQL +'  ,H.SlipNumber AS ''SlipNumber''' + @CRLF
	SET @SQL = @SQL +'  ,H.SalesCarNumber AS ''SalesCarNumber''' + @CRLF
	SET @SQL = @SQL +'  ,H.Vin AS ''Vin''' + @CRLF
	SET @SQL = @SQL +'  ,C.CustomerName AS ''CustomerName''' + @CRLF
	SET @SQL = @SQL +'  ,D.DepartmentCode AS ''DepartmentCode''' + @CRLF
	SET @SQL = @SQL +'  ,D.DepartmentName AS ''DepartmentName''' + @CRLF
	SET @SQL = @SQL +'  ,E.Employeename AS ''Employeename''' + @CRLF
	SET @SQL = @SQL +'  ,ISNULL(SalesPrice, 0) + ISNULL(SalesTax, 0) AS ''SalesPrice''' + @CRLF												--Mod 2023/09/18 yano #4181
	--SET @SQL = @SQL +'  ,SalesPrice AS ''SalesPrice''' + @CRLF			
	SET @SQL = @SQL +'  ,ISNULL(ShopOptionAmount, 0)+ISNULL(ShopOptionTaxAmount,0) - (ISNULL(MaintenancePackageAmount, 0)+ISNULL(MaintenancePackageTaxAmount,0) + ISNULL(ExtendedWarrantyAmount, 0)+ISNULL(ExtendedWarrantyTaxAmount,0) + ISNULL(SurchargeAmount, 0) + ISNULL(SurchargeTaxAmount,0)) AS ''ShopOptionAmountWithTax''' + @CRLF	--Mod 2023/09/18 yano #4181 --Mod 2023/01/16 yano #4138
	SET @SQL = @SQL +'  ,ISNULL(MaintenancePackageAmount, 0)+ISNULL(MaintenancePackageTaxAmount,0) AS ''MaintenancePackageAmount''' + @CRLF	--Add 2023/01/16 yano #4138
	SET @SQL = @SQL +'  ,ISNULL(ExtendedWarrantyAmount, 0)+ISNULL(ExtendedWarrantyTaxAmount,0) AS ''ExtendedWarrantyAmount''' + @CRLF		--Add 2023/01/16 yano #4138
	SET @SQL = @SQL +'  ,ISNULL(SurchargeAmount, 0)+ISNULL(SurchargeTaxAmount,0) AS ''SurchargeAmount''' + @CRLF							--Add 2023/09/18 yano #4181
	--SET @SQL = @SQL +'  ,ISNULL(MakerOptionAmount,0) AS ''MakerOptionAmount''' + @CRLF
	SET @SQL = @SQL +'  ,ISNULL(MakerOptionAmount,0) + ISNULL(MakerOptionTaxAmount, 0) AS ''MakerOptionAmount''' + @CRLF					--Mod 2023/09/18 yano #4181
	SET @SQL = @SQL +'  ,Case When L.Amount is null THEN 0 ELSE L.Amount END AS ''AAAmount''' + @CRLF
	--SET @SQL = @SQL +'  ,ISNULL(SalesCostTotalAmount,0) AS ''SalesCostTotalAmount''' + @CRLF
	SET @SQL = @SQL +'  ,ISNULL(SalesCostTotalAmount,0) + ISNULL(SalesCostTotalTaxAmount,0) AS ''SalesCostTotalAmount''' + @CRLF			--Mod 2023/09/18 yano #4181
	--SET @SQL = @SQL +'  ,ISNULL(DiscountAmount, 0) *-1 AS ''DiscountAmount''' + @CRLF
	SET @SQL = @SQL +'  ,(ISNULL(DiscountAmount, 0) *-1) + (ISNULL(DiscountTax, 0) *-1) AS ''DiscountAmount''' + @CRLF						--Mod 2023/09/18 yano #4181
	SET @SQL = @SQL +'  ,ISNULL(OtherCostTotalAmount,0) AS ''OtherCostTotalAmount''' + @CRLF
	SET @SQL = @SQL +'  ,ISNULL(TaxFreeTotalAmount,0) AS ''TaxFreeTotalAmount''' + @CRLF
	SET @SQL = @SQL +'  ,ISNULL(CarLiabilityInsurance,0) AS ''CarLiabilityInsurance''' + @CRLF
	SET @SQL = @SQL +'  ,ISNULL(RecycleDeposit,0) AS ''RecycleDeposit''' + @CRLF
	SET @SQL = @SQL +'  ,ISNULL(GrandTotalAmount,0) AS ''GrandTotalAmount''' + @CRLF
	SET @SQL = @SQL +'  ,ISNULL(TradeInTotalAmount,0)-ISNULL(TradeInTaxTotalAmount,0) AS ''TradeInTotalAmountNotTax''' + @CRLF
	SET @SQL = @SQL +'  ,H.TradeInVin1 AS ''TradeInVin1''' + @CRLF
	SET @SQL = @SQL +'  ,H.TradeInVin2 AS ''TradeInVin2''' + @CRLF
	SET @SQL = @SQL +'  ,H.TradeInVin3 AS ''TradeInVin3''' + @CRLF
	SET @SQL = @SQL +'  ,ISNULL(TradeInUnexpiredCarTaxTotalAmount,0) AS ''TradeInUnexpiredCarTaxTotalAmount''' + @CRLF
	SET @SQL = @SQL +'  ,ISNULL(TradeInRemainDebtTotalAmount,0) AS ''TradeInRemainDebtTotalAmount''' + @CRLF
	SET @SQL = @SQL +'  ,ISNULL(TradeInAppropriationTotalAmount,0) AS ''TradeInAppropriationTotalAmount''' + @CRLF
	SET @SQL = @SQL +'  ,H.CarName AS ''CarName''' + @CRLF
	SET @SQL = @SQL +'  ,H.CarBrandName AS ''CarBrandName''' + @CRLF
	SET @SQL = @SQL +'FROM [dbo].[CarSalesHeader] H' + @CRLF
	SET @SQL = @SQL +'INNER JOIN [dbo].[Department] D on H.DepartmentCode = D.DepartmentCode' + @CRLF
	SET @SQL = @SQL +'INNER JOIN [dbo].[Employee] E on H.EmployeeCode = E.EmployeeCode' + @CRLF
	SET @SQL = @SQL +'INNER JOIN [dbo].[Customer] C on H.CustomerCode = C.CustomerCode' + @CRLF
	SET @SQL = @SQL +'INNER JOIN [dbo].[c_NewUsedType] Cn on H.NewUsedType = Cn.Code' + @CRLF
	SET @SQL = @SQL +'INNER JOIN [dbo].[c_SalesOrderStatus] Cs on H.SalesOrderStatus = Cs.Code' + @CRLF
	SET @SQL = @SQL +'LEFT OUTER JOIN ' + @CRLF
	SET @SQL = @SQL +'	(' + @CRLF
	SET @SQL = @SQL +'		SELECT ' + @CRLF
	SET @SQL = @SQL +'			 SlipNumber ' + @CRLF
	SET @SQL = @SQL +'			,RevisionNumber ' + @CRLF
	SET @SQL = @SQL +'			,SUM(ISNULL(Amount, 0)) AS Amount ' + @CRLF
	SET @SQL = @SQL +'			,SUM(ISNULL(Amount, 0) + ISNULL(TaxAmount, 0)) AS TotalAmount ' + @CRLF
	SET @SQL = @SQL +'		FROM ' + @CRLF
	SET @SQL = @SQL +'			CarSalesLine ' + @CRLF
	SET @SQL = @SQL +'		WHERE ' + @CRLF
	SET @SQL = @SQL +'			    DelFlag = ''0''' + @CRLF
	SET @SQL = @SQL +'			AND OptionType = ''001''' + @CRLF
	SET @SQL = @SQL +'			AND CarOptionCode IN (''AA001'',''AA002'') ' + @CRLF
	SET @SQL = @SQL +'		GROUP BY ' + @CRLF
	SET @SQL = @SQL +'			 SlipNumber ' + @CRLF
	SET @SQL = @SQL +'			,RevisionNumber ' + @CRLF
	SET @SQL = @SQL +'	) L ON H.SlipNumber = L.SlipNumber AND H.RevisionNumber = L.RevisionNumber ' + @CRLF


	SET @SQL = @SQL +'WHERE H.DelFlag=''0''' + @CRLF
	SET @SQL = @SQL +'  AND H.SalesOrderStatus in (''005'')' + @CRLF

	--納車年月
	IF ((@SelectYear IS NOT NULL) AND (@SelectYear <> '') AND (@SelectMonth IS NOT NULL) AND (@SelectMonth <> ''))
	BEGIN
		SET @SQL = @SQL +'AND YEAR(H.SalesDate) = @SelectYear' + @CRLF
		SET @SQL = @SQL +'AND MONTH(H.SalesDate) = @SelectMonth' + @CRLF
	END
	ELSE IF ((@SelectYear IS NOT NULL) AND (@SelectYear <> '') AND ((@SelectMonth IS NULL) OR (@SelectMonth = '')))
	BEGIN
		SET @SQL = @SQL +'AND H.SalesDate >= CONVERT(datetime, ''' + CONVERT(nvarchar, @SalesDateFrom, 112) + ''')' + @CRLF
		SET @SQL = @SQL +'AND H.SalesDate < CONVERT(datetime, ''' + CONVERT(nvarchar, @SalesDateTo, 112) + ''')' + @CRLF
	END

	--部門コード
	IF ((@DepartmentCode IS NOT NULL) AND (@DepartmentCode <> ''))
	BEGIN
		SET @SQL = @SQL +'AND H.DepartmentCode = @DepartmentCode' + @CRLF
	END	

	--新中区分
	IF ((@NewUsedType IS NOT NULL) AND (@NewUsedType <> ''))
	BEGIN
		SET @SQL = @SQL +'AND H.NewUsedType = @NewUsedType' + @CRLF
	END	

	--Add 2020/01/17 yano #4027
	--販売区分
	if(@AAType = '1')--一般
	BEGIN
		SET @SQL = @SQL +'and H.SalesType in (''001'',''002'')' + @CRLF
	END

	if(@AAType  = '2')--AA・業販
	BEGIN
		SET @SQL = @SQL +'and H.SalesType in (''003'',''004'',''009'')' + @CRLF
	END

	if(@AAType  = '3')--デモ・自登
	BEGIN
		SET @SQL = @SQL +'and H.SalesType in (''005'',''006'')' + @CRLF
	END

	if(@AAType  = '4')--依廃・他
	BEGIN
		SET @SQL = @SQL +'and H.SalesType in (''007'',''008'')' + @CRLF
	END


	--IF ((@AAType IS NOT NULL) AND (@AAType <> '') AND (@AAType = '001'))	--AA含まない
	--BEGIN
	--	SET @SQL = @SQL +'AND C.CustomerType <> ''201''' + @CRLF
	--END
	--ELSE IF ((@AAType IS NOT NULL) AND (@AAType <> '') AND (@AAType = '003'))	--AAのみ
	--BEGIN
	--	SET @SQL = @SQL +'AND C.CustomerType = ''201''' + @CRLF
	--END


	SET @SQL = @SQL +'ORDER BY H.DepartmentCode,Cn.Name,H.SalesDate' + @CRLF


	--DEBUG
	--PRINT @SQL

	EXECUTE sp_executeSQL @SQL, @PARAM,@SelectYear, @SelectMonth, @DepartmentCode, @NewUsedType, @AAType
	--*/

	/*
	SELECT
		 convert(datetime, null) AS SalesDate
		,convert(varchar(50), '') AS NewUsedTypeName
		,convert(nvarchar(50), '') AS SlipNumber
		,convert(nvarchar(50), '') AS SalesCarNumber
		,convert(nvarchar(20), '') AS Vin
		,convert(nvarchar(80), '') AS CustomerName
		,convert(nvarchar(3), '') AS DepartmentCode
		,convert(nvarchar(20), '') AS DepartmentName
		,convert(nvarchar(40), '') AS Employeename
		,convert(decimal(10, 0), null) AS SalesPrice
		,convert(decimal(10, 0), null) AS ShopOptionAmountWithTax
		,convert(decimal(10, 0), null) AS MaintenancePackageAmount			--Add 2023/01/16 yano #4138
		,convert(decimal(10, 0), null) AS ExtendedWarrantyAmount			--Add 2023/01/16 yano #4138
		,convert(decimal(10, 0), null) AS SurchargeAmount					--Add 2023/01/16 yano #4138
		,convert(decimal(10, 0), null) AS MakerOptionAmount
		,convert(decimal(10, 0), null) AS AAAmount
		,convert(decimal(10, 0), null) AS SalesCostTotalAmount
		,convert(decimal(10, 0), null) AS DiscountAmount
		,convert(decimal(10, 0), null) AS OtherCostTotalAmount
		,convert(decimal(10, 0), null) AS TaxFreeTotalAmount
		,convert(decimal(10, 0), null) AS CarLiabilityInsurance
		,convert(decimal(10, 0), null) AS RecycleDeposit
		,convert(decimal(10, 0), null) AS GrandTotalAmount
		,convert(decimal(10, 0), null) AS TradeInTotalAmountNotTax
		,convert(nvarchar(20), '') AS TradeInVin1
		,convert(nvarchar(20), '') AS TradeInVin2
		,convert(nvarchar(20), '') AS TradeInVin3
		,convert(decimal(10, 0), null) AS TradeInUnexpiredCarTaxTotalAmount
		,convert(decimal(10, 0), null) AS TradeInRemainDebtTotalAmount
		,convert(decimal(10, 0), null) AS TradeInAppropriationTotalAmount
		,convert(nvarchar(20), '') AS CarName
		,convert(nvarchar(50), '') AS CarBrandName
	*/



--更新履歴テーブルに登録
	INSERT INTO [dbo].[DB_ReleaseHistory]
			([HistoryID]
			,[TicketNumber]
			,[QueryName]
			,[ReleaseDate]
			,[Summary]
			,[ExecEmployeeCode]
			,[ExecDate])
	VALUES
		(NEWID()
		,'4181'--チケット番号
		,'20230918_#4181【車両伝票】オプション区分追加（サーチャージ）/07_Alter_Procedure_GetCarSalesHeaderLIst.sql'
		,CONVERT(datetime, '2023/10/27', 120)		--※※※※※※※※※※リリース日(WPH様入力)※※※※※※※※※※※
		,''--コメント
		,'arima.yuji'--実行者
		,GETDATE()--実行日
	)


GO


