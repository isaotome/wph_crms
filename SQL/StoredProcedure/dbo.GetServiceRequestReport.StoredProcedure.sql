USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetServiceRequestReport]    Script Date: 2017/10/31 14:11:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--2021/06/09 yano #4091 【車両伝票】オプション行の区分の追加
--2017/10/30 arc yano #3785 車両作業依頼書 承認欄追加
--2017/02/21 arc nakayama #3626_【車】車両伝票の「作業依頼書」へ受注後に追加されない　新規作成

CREATE PROCEDURE [dbo].[GetServiceRequestReport]

	@OriginalSlipNumber nvarchar(50)		--伝票番号＜作業依頼書出力用の条件-1件に絞る為の条件＞

AS
	SET NOCOUNT ON	
	/*-------------------------------------------*/
	/* ダーティーリードの設定					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/* 作業依頼テーブル絞込み		             */
	/*-------------------------------------------*/

	CREATE TABLE #temp_ServiceRequest_sr (
		  ServiceRequestId nvarchar(36)
		 ,OriginalSlipNumber nvarchar(50)
		 ,DepartmentCode nvarchar(3)
		 ,DeliveryRequirement datetime
		 ,OwnershipChange nvarchar(3)
		 ,AnnualInspection nvarchar(3)
		 ,InsuranceInheritance nvarchar(3)
		 ,Memo nvarchar(100)

	)

	INSERT INTO #temp_ServiceRequest_sr
	SELECT
		  ServiceRequestId
		, OriginalSlipNumber
		, DepartmentCode
		, DeliveryRequirement
		, OwnershipChange
		, AnnualInspection
		, InsuranceInheritance
		, Memo
	FROM
		dbo.ServiceRequest
	WHERE DelFlag = '0' 
	  AND OriginalSlipNumber = @OriginalSlipNumber
	
	CREATE INDEX ix_temp_ServiceRequest_sr ON #temp_ServiceRequest_sr(OriginalSlipNumber, ServiceRequestId)

		
	/*-------------------------------------------*/
	/* 作業依頼明細 を1行にまとめる				 */
	/*-------------------------------------------*/
	
	CREATE TABLE #temp_ServiceRequestLineList_sl(
		 ServiceRequestId nvarchar(36)
		,OptionTypeName1 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode1 nvarchar(25)
		,CarOptionName1 nvarchar(100)
		,Amount1 decimal(10,0)
		,ClaimType1 bit
		,RequestComment1 nvarchar(100)
		,OptionTypeName2 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode2 nvarchar(25)
		,CarOptionName2 nvarchar(100)
		,Amount2 decimal(10,0)
		,ClaimType2 bit
		,RequestComment2 nvarchar(100)
		,OptionTypeName3 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode3 nvarchar(25)
		,CarOptionName3 nvarchar(100)
		,Amount3 decimal(10,0)
		,ClaimType3 bit
		,RequestComment3 nvarchar(100)
		,OptionTypeName4 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode4 nvarchar(25)
		,CarOptionName4 nvarchar(100)
		,Amount4 decimal(10,0)
		,ClaimType4 bit
		,RequestComment4 nvarchar(100)
		,OptionTypeName5 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode5 nvarchar(25)
		,CarOptionName5 nvarchar(100)
		,Amount5 decimal(10,0)
		,ClaimType5 bit
		,RequestComment5 nvarchar(100)
		,OptionTypeName6 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode6 nvarchar(25)
		,CarOptionName6 nvarchar(100)
		,Amount6 decimal(10,0)
		,ClaimType6 bit
		,RequestComment6 nvarchar(100)
		,OptionTypeName7 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode7 nvarchar(25)
		,CarOptionName7 nvarchar(100)
		,Amount7 decimal(10,0)
		,ClaimType7 bit
		,RequestComment7 nvarchar(100)
		,OptionTypeName8 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode8 nvarchar(25)
		,CarOptionName8 nvarchar(100)
		,Amount8 decimal(10,0)
		,ClaimType8 bit
		,RequestComment8 nvarchar(100)
		,OptionTypeName9 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode9 nvarchar(25)
		,CarOptionName9 nvarchar(100)
		,Amount9 decimal(10,0)
		,ClaimType9 bit
		,RequestComment9 nvarchar(100)
		,OptionTypeName10 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode10 nvarchar(25)
		,CarOptionName10 nvarchar(100)
		,Amount10 decimal(10,0)
		,ClaimType10 bit
		,RequestComment10 nvarchar(100)
		,OptionTypeName11 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode11 nvarchar(25)
		,CarOptionName11 nvarchar(100)
		,Amount11 decimal(10,0)
		,ClaimType11 bit
		,RequestComment11 nvarchar(100)
		,OptionTypeName12 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode12 nvarchar(25)
		,CarOptionName12 nvarchar(100)
		,Amount12 decimal(10,0)
		,ClaimType12 bit
		,RequestComment12 nvarchar(100)
		,OptionTypeName13 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode13 nvarchar(25)
		,CarOptionName13 nvarchar(100)
		,Amount13 decimal(10,0)
		,ClaimType13 bit
		,RequestComment13 nvarchar(100)
		,OptionTypeName14 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode14 nvarchar(25)
		,CarOptionName14 nvarchar(100)
		,Amount14 decimal(10,0)
		,ClaimType14 bit
		,RequestComment14 nvarchar(100)
		,OptionTypeName15 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode15 nvarchar(25)
		,CarOptionName15 nvarchar(100)
		,Amount15 decimal(10,0)
		,ClaimType15 bit
		,RequestComment15 nvarchar(100)
		,OptionTypeName16 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode16 nvarchar(25)
		,CarOptionName16 nvarchar(100)
		,Amount16 decimal(10,0)
		,ClaimType16 bit
		,RequestComment16 nvarchar(100)
		,OptionTypeName17 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode17 nvarchar(25)
		,CarOptionName17 nvarchar(100)
		,Amount17 decimal(10,0)
		,ClaimType17 bit
		,RequestComment17 nvarchar(100)
		,OptionTypeName18 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode18 nvarchar(25)
		,CarOptionName18 nvarchar(100)
		,Amount18 decimal(10,0)
		,ClaimType18 bit
		,RequestComment18 nvarchar(100)
		,OptionTypeName19 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode19 nvarchar(25)
		,CarOptionName19 nvarchar(100)
		,Amount19 decimal(10,0)
		,ClaimType19 bit
		,RequestComment19 nvarchar(100)
		,OptionTypeName20 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode20 nvarchar(25)
		,CarOptionName20 nvarchar(100)
		,Amount20 decimal(10,0)
		,ClaimType20 bit
		,RequestComment20 nvarchar(100)
		,OptionTypeName21 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode21 nvarchar(25)
		,CarOptionName21 nvarchar(100)
		,Amount21 decimal(10,0)
		,ClaimType21 bit
		,RequestComment21 nvarchar(100)
		,OptionTypeName22 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode22 nvarchar(25)
		,CarOptionName22 nvarchar(100)
		,Amount22 decimal(10,0)
		,ClaimType22 bit
		,RequestComment22 nvarchar(100)
		,OptionTypeName23 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode23 nvarchar(25)
		,CarOptionName23 nvarchar(100)
		,Amount23 decimal(10,0)
		,ClaimType23 bit
		,RequestComment23 nvarchar(100)
		,OptionTypeName24 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode24 nvarchar(25)
		,CarOptionName24 nvarchar(100)
		,Amount24 decimal(10,0)
		,ClaimType24 bit
		,RequestComment24 nvarchar(100)
		,OptionTypeName25 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,CarOptionCode25 nvarchar(25)
		,CarOptionName25 nvarchar(100)
		,Amount25 decimal(10,0)
		,ClaimType25 bit
		,RequestComment25 nvarchar(100)

	)

	declare @Linecount int --件数
	declare @Loopcount int = 0 --ループカウント

	DECLARE
	     @ServiceRequestId nvarchar(36)
		,@OptionTypeName nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)→nvarchar(50)
		,@CarOptionCode nvarchar(25)
		,@CarOptionName nvarchar(100)
		,@Amount decimal(10,0)
		,@ClaimType nvarchar(2)
		,@RequestComment nvarchar(100);

	DECLARE ServiceRequestLineList CURSOR FOR
	SELECT
		  sl.ServiceRequestId
		 ,co.Name
		 ,sl.CarOptionCode
		 ,sl.CarOptionName
		 ,sl.Amount
		 ,case when sl.ClaimType = 1 then '1' else '0' end
		 ,sl.RequestComment
	FROM dbo.ServiceRequestLine sl
	INNER JOIN c_OptionType AS co ON co.Code = sl.OptionType
	INNER JOIN #temp_ServiceRequest_sr AS sr ON sr.ServiceRequestId = sl.ServiceRequestId
	ORDER BY sl.LineNumber

	OPEN ServiceRequestLineList;

	FETCH NEXT FROM ServiceRequestLineList
	INTO @ServiceRequestId
		,@OptionTypeName
		,@CarOptionCode
		,@CarOptionName
		,@Amount
		,@ClaimType
		,@RequestComment;

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	SET @SQL = ''
	SET @SQL = @SQL +'INSERT INTO #temp_ServiceRequestLineList_sl' + @CRLF
	SET @SQL = @SQL +'SELECT' + @CRLF
	SET @SQL = @SQL + '''' + @ServiceRequestId + '''' + @CRLF
	SET @SQL = @SQL + ',' + '''' + ISNULL(@OptionTypeName, '') + '''' + @CRLF
	SET @SQL = @SQL + ',' + '''' + ISNULL(@CarOptionCode, '') + '''' + @CRLF
	SET @SQL = @SQL + ',' + '''' + ISNULL(@CarOptionName, '') + '''' + @CRLF
	SET @SQL = @SQL + ',' + convert(nvarchar(10), ISNULL(@Amount, 0)) + @CRLF
	SET @SQL = @SQL + ',' + '''' + ISNULL(@ClaimType, '') + '''' + @CRLF
	SET @SQL = @SQL + ',' + '''' + ISNULL(@RequestComment, '') + '''' + @CRLF
	
	WHILE @Loopcount < 24
	BEGIN
		FETCH NEXT FROM ServiceRequestLineList
		INTO @ServiceRequestId
			,@OptionTypeName
			,@CarOptionCode
			,@CarOptionName
			,@Amount
			,@ClaimType
			,@RequestComment;

		IF @@FETCH_STATUS = 0
			BEGIN
				SET @SQL = @SQL + ',' + '''' + ISNULL(@OptionTypeName, '') + '''' + @CRLF
				SET @SQL = @SQL + ',' + '''' + ISNULL(@CarOptionCode, '') + '''' + @CRLF
				SET @SQL = @SQL + ',' + '''' + ISNULL(@CarOptionName, '') + '''' + @CRLF
				SET @SQL = @SQL + ',' + convert(nvarchar(10), ISNULL(@Amount, 0)) + @CRLF
				SET @SQL = @SQL + ',' + '''' + ISNULL(@ClaimType, '') + '''' + @CRLF
				SET @SQL = @SQL + ',' + '''' +  ISNULL(@RequestComment, '') + '''' + @CRLF
			END
		ELSE
			BEGIN
			 	SET @SQL = @SQL + ',''''' + @CRLF
				SET @SQL = @SQL + ',''''' + @CRLF
				SET @SQL = @SQL + ',''''' + @CRLF
				SET @SQL = @SQL + ',NULL' + @CRLF
				SET @SQL = @SQL + ',''0''' + @CRLF
				SET @SQL = @SQL + ',''''' + @CRLF
			END
		SET @Loopcount = @Loopcount + 1;
	END

	CLOSE ServiceRequestLineList;
	DEALLOCATE ServiceRequestLineList;
	
	EXECUTE sp_executeSQL @SQL

	/*-------------------------------------------*/
	/* 車両発注絞込み			　　			 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_CarPurchaseOrder_po (
		SlipNumber nvarchar(50)
	   ,MakerShipmentDate datetime
	   ,ArrivalPlanDate datetime
	   ,RegistrationPlanDate datetime
    )

	INSERT INTO #temp_CarPurchaseOrder_po
	SELECT
	  po.SlipNumber
	, po.MakerShipmentDate
	, po.ArrivalPlanDate
	, po.RegistrationPlanDate
	FROM dbo.CarPurchaseOrder as po
	WHERE po.DelFlag = '0'
	  AND EXISTS(SELECT 1 FROM #temp_ServiceRequest_sr AS sr WHERE sr.OriginalSlipNumber = po.SlipNumber)

	CREATE INDEX ix_temp_CarPurchaseOrder_po ON #temp_CarPurchaseOrder_po(SlipNumber)

	/*-------------------------------------------*/
	/* 依頼もとの車両伝伝票検索	　　			 */
	/*-------------------------------------------*/

	CREATE TABLE #temp_CarSalesHeader_h (
		SlipNumber nvarchar(50)
	   ,SalesPlanDate datetime
	   ,NewUsedType nvarchar(3)
	   ,SalesType nvarchar(3)
	   ,CarGradeCode nvarchar(30)
	   ,CarGradeName nvarchar(100)
	   ,MakerName nvarchar(50)
	   ,CarBrandName nvarchar(50)
	   ,CarName nvarchar(50)
	   ,ExteriorColorName nvarchar(50)
	   ,InteriorColorName nvarchar(50)
	   ,Vin nvarchar(20)
	   ,modelName nvarchar(20)
	   ,Mileage decimal(12,2)
	   ,MileageUnit nvarchar(3)
	   ,SalesCarNumber nvarchar(50)
	   ,CustomerCode nvarchar(10)
    )

	INSERT INTO #temp_CarSalesHeader_h
	SELECT
	  h.SlipNumber
	, h.SalesPlanDate
	, h.NewUsedType
	, h.SalesType
	, h.CarGradeCode
	, h.CarGradeName
	, h.MakerName
	, h.CarBrandName
	, h.CarName
	, h.ExteriorColorName
	, h.InteriorColorName
	, h.Vin
	, h.modelName
	, h.Mileage
	, h.MileageUnit
	, h.SalesCarNumber
	, h.CustomerCode
	FROM dbo.CarSalesHeader AS h
	WHERE h.DelFlag = '0'
	  AND h.SlipNumber = @OriginalSlipNumber

	CREATE INDEX ix_temp_CarSalesHeader_h ON #temp_CarSalesHeader_h(SlipNumber)

	/*-------------------------------------------*/
	/* 作業依頼書情報							 */
	/*-------------------------------------------*/

	SELECT
		 sr.OriginalSlipNumber				--車両伝票番号
		,c1.Name as 'NewUsedType'			--新中区分
		,c2.Name as 'SalesType'				--販売区分
		,h.CarGradeCode						--グレードコード
		,h.MakerName						--メーカー名
		,h.CarBrandName						--ブランド名
		,h.CarName							--車種名
		,h.CarGradeName						--グレード名
		,h.ExteriorColorName				--外装色
		,h.InteriorColorName				--内装色
		,h.Vin								--車台番号
		,h.modelName						--型式
		,h.Mileage							--走行距離
		,c3.Name as 'MileageUnit'			--走行距離単位
		,h.CustomerCode						--顧客コード
		,c.CustomerName						--顧客名
		,c.CustomerNameKana					--顧客名（カナ）
		,c.Prefecture						--住所
		,c.City
		,c.Address1
		,c.Address2
		,d.DepartmentName					--依頼先部門
		,sr.Memo							--備考
		,l.LocationName						--現在地
		,CONVERT(NVARCHAR, po.MakerShipmentDate, 111) AS 'MakerShipmentDate'	--出荷予定日
		,CONVERT(NVARCHAR, po.ArrivalPlanDate, 111) AS 'ArrivalPlanDate'		--到着予定日
		,CONVERT(NVARCHAR, po.RegistrationPlanDate, 111) AS 'RegistrationPlanDate'--登録予定日
		,CONVERT(NVARCHAR, h.SalesPlanDate, 111) AS 'SalesPlanDate'		--納車予定日
		,c4.Name as 'OwnershipChange'		--区分
		,c5.Name as 'AnnualInspection'		--12ヶ月点検
		,c6.Name as 'InsuranceInheritance'	--保証継承
		,CONVERT(NVARCHAR, sr.DeliveryRequirement, 111) AS 'DeliveryRequirement'	--希望納期
		,sl.OptionTypeName1	--区分1
		,sl.CarOptionCode1	--品番1
		,sl.OptionTypeName1	--区分1
		,sl.CarOptionCode1	--品番1
		,sl.CarOptionName1	--品名1
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount1, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount1, 0)), 1), '.00', '') END AS 'Amount1'		--金額1
		,CASE WHEN sl.ClaimType1 = 1 THEN '有償' ELSE '' END AS 'ClaimType1'--有償1
		,sl.RequestComment1	--コメント1
		,sl.OptionTypeName2	--区分2
		,sl.CarOptionCode2	--品番2
		,sl.CarOptionName2	--品名2
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount2, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount2, 0)), 1), '.00', '') END AS 'Amount2'		--金額2
		,CASE WHEN sl.ClaimType2 = 1 THEN '有償' ELSE '' END AS 'ClaimType2'--有償2
		,sl.RequestComment2	--コメント2
		,sl.OptionTypeName3	--区分3
		,sl.CarOptionCode3	--品番3
		,sl.CarOptionName3	--品名3
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount3, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount3, 0)), 1), '.00', '') END AS 'Amount3'		--金額3
		,CASE WHEN sl.ClaimType3 = 1 THEN '有償' ELSE '' END AS 'ClaimType3'--有償3
		,sl.RequestComment3	--コメント3
		,sl.OptionTypeName4	--区分4
		,sl.CarOptionCode4	--品番4
		,sl.CarOptionName4	--品名4
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount4, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount4, 0)), 1), '.00', '') END AS 'Amount4'		--金額4
		,CASE WHEN sl.ClaimType4 = 1 THEN '有償' ELSE '' END AS 'ClaimType4'--有償4
		,sl.RequestComment4	--コメント4
		,sl.OptionTypeName5	--区分5
		,sl.CarOptionCode5	--品番5
		,sl.CarOptionName5	--品名5
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount5, 0)), 1), '.00', '') = '0'	THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount5, 0)), 1), '.00', '') END AS 'Amount5'		--金額5
		,CASE WHEN sl.ClaimType5 = 1 THEN '有償' ELSE '' END AS 'ClaimType5'--有償5
		,sl.RequestComment5	--コメント5
		,sl.OptionTypeName6	--区分6
		,sl.CarOptionCode6	--品番6
		,sl.CarOptionName6	--品名6
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount6, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount6, 0)), 1), '.00', '') END AS 'Amount6'		--金額6
		,CASE WHEN sl.ClaimType6 = 1 THEN '有償' ELSE '' END AS 'ClaimType6'--有償6
		,sl.RequestComment6	--コメント6
		,sl.OptionTypeName7	--区分7
		,sl.CarOptionCode7	--品番7
		,sl.CarOptionName7	--品名7
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount7, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount7, 0)), 1), '.00', '') END AS 'Amount7'		--金額7
		,CASE WHEN sl.ClaimType7 = 1 THEN '有償' ELSE '' END AS 'ClaimType7'--有償7
		,sl.RequestComment7	--コメント7
		,sl.OptionTypeName8	--区分8
		,sl.CarOptionCode8	--品番8
		,sl.CarOptionName8	--品名8
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount8, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount8, 0)), 1), '.00', '') END AS 'Amount8'		--金額8
		,CASE WHEN sl.ClaimType8 = 1 THEN '有償' ELSE '' END AS 'ClaimType8'--有償8
		,sl.RequestComment8	--コメント8
		,sl.OptionTypeName9	--区分9
		,sl.CarOptionCode9	--品番9
		,sl.CarOptionName9	--品名9
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount9, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount9, 0)), 1), '.00', '') END AS 'Amount9'		--金額9
		,CASE WHEN sl.ClaimType9 = 1 THEN '有償' ELSE '' END AS 'ClaimType9'--有償9
		,sl.RequestComment9	--コメント9
		,sl.OptionTypeName10	--区分10
		,sl.CarOptionCode10		--品番10
		,sl.CarOptionName10		--品名10
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount10, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount10, 0)), 1), '.00', '') END AS 'Amount10'		--金額10
		,CASE WHEN sl.ClaimType10 = 1 THEN '有償' ELSE '' END AS 'ClaimType10'--有償10
		,sl.RequestComment10	--コメント10
		,sl.OptionTypeName11	--区分11
		,sl.CarOptionCode11		--品番11
		,sl.CarOptionName11		--品名11
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount11, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount11, 0)), 1), '.00', '') END AS 'Amount11'		--金額11
		,CASE WHEN sl.ClaimType11 = 1 THEN '有償' ELSE '' END AS 'ClaimType11'--有償11
		,sl.RequestComment11	--コメント11
		,sl.OptionTypeName12	--区分12
		,sl.CarOptionCode12		--品番12
		,sl.CarOptionName12		--品名12
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount12, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount12, 0)), 1), '.00', '') END AS 'Amount12'		--金額12
		,CASE WHEN sl.ClaimType12 = 1 THEN '有償' ELSE '' END AS 'ClaimType12'--有償12
		,sl.RequestComment12	--コメント12
		,sl.OptionTypeName13	--区分13
		,sl.CarOptionCode13		--品番13
		,sl.CarOptionName13		--品名13
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount13, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount13, 0)), 1), '.00', '') END AS 'Amount13'		--金額13
		,CASE WHEN sl.ClaimType13 = 1 THEN '有償' ELSE '' END AS 'ClaimType13'--有償13
		,sl.RequestComment13	--コメント13
		,sl.OptionTypeName14	--区分14
		,sl.CarOptionCode14		--品番14
		,sl.CarOptionName14		--品名14
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount14, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount14, 0)), 1), '.00', '') END AS 'Amount14'		--金額14
		,CASE WHEN sl.ClaimType14 = 1 THEN '有償' ELSE '' END AS 'ClaimType14'--有償14
		,sl.RequestComment14	--コメント14
		,sl.OptionTypeName15	--区分15
		,sl.CarOptionCode15		--品番15
		,sl.CarOptionName15		--品名15
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount15, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount15, 0)), 1), '.00', '') END AS 'Amount15'		--金額15
		,CASE WHEN sl.ClaimType15 = 1 THEN '有償' ELSE '' END AS 'ClaimType15'--有償15
		,sl.RequestComment15	--コメント15
		,sl.OptionTypeName16--区分16
		,sl.CarOptionCode16--品番16
		,sl.CarOptionName16--品名16
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount16, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount16, 0)), 1), '.00', '') END AS 'Amount16'--金額16
		,CASE WHEN sl.ClaimType16 = 1 THEN '有償' ELSE '' END AS 'ClaimType16'--有償16
		,sl.RequestComment16--コメント16
		,sl.OptionTypeName17--区分17
		,sl.CarOptionCode17--品番17
		,sl.CarOptionName17--品名17
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount17, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount17, 0)), 1), '.00', '') END AS 'Amount17'--金額17
		,CASE WHEN sl.ClaimType17 = 1 THEN '有償' ELSE '' END AS 'ClaimType17'--有償17
		,sl.RequestComment17--コメント17
		,sl.OptionTypeName18--区分18
		,sl.CarOptionCode18--品番18
		,sl.CarOptionName18--品名18
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount18, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount18, 0)), 1), '.00', '') END AS 'Amount18'--金額18
		,CASE WHEN sl.ClaimType18 = 1 THEN '有償' ELSE '' END AS 'ClaimType18'--有償18
		,sl.RequestComment18--コメント18
		,sl.OptionTypeName19--区分19
		,sl.CarOptionCode19--品番19
		,sl.CarOptionName19--品名19
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount19, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount19, 0)), 1), '.00', '') END AS 'Amount19'--金額19
		,CASE WHEN sl.ClaimType19 = 1 THEN '有償' ELSE '' END AS 'ClaimType19'--有償19
		,sl.RequestComment19--コメント19
		,sl.OptionTypeName20--区分20
		,sl.CarOptionCode20--品番20
		,sl.CarOptionName20--品名20
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount20, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount20, 0)), 1), '.00', '') END AS 'Amount20'--金額20
		,CASE WHEN sl.ClaimType20 = 1 THEN '有償' ELSE '' END AS 'ClaimType20'--有償20
		,sl.RequestComment20--コメント20
		,sl.OptionTypeName21--区分21
		,sl.CarOptionCode21--品番21
		,sl.CarOptionName21--品名21
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount21, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount21, 0)), 1), '.00', '') END AS 'Amount21'--金額21
		,CASE WHEN sl.ClaimType21 = 1 THEN '有償' ELSE '' END AS 'ClaimType21'--有償21
		,sl.RequestComment21--コメント21
		,sl.OptionTypeName22--区分22
		,sl.CarOptionCode22--品番22
		,sl.CarOptionName22--品名22
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount22, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount22, 0)), 1), '.00', '') END AS 'Amount22'--金額22
		,CASE WHEN sl.ClaimType22 = 1 THEN '有償' ELSE '' END AS 'ClaimType22'--有償22
		,sl.RequestComment22--コメント22
		,sl.OptionTypeName23--区分23
		,sl.CarOptionCode23--品番23
		,sl.CarOptionName23--品名23
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount23, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount23, 0)), 1), '.00', '') END AS 'Amount23'--金額23
		,CASE WHEN sl.ClaimType23 = 1 THEN '有償' ELSE '' END AS 'ClaimType23'--有償23
		,sl.RequestComment23--コメント23
		,sl.OptionTypeName24--区分24
		,sl.CarOptionCode24--品番24
		,sl.CarOptionName24--品名24
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount24, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount24, 0)), 1), '.00', '') END AS 'Amount24'--金額24
		,CASE WHEN sl.ClaimType24 = 1 THEN '有償' ELSE '' END AS 'ClaimType24'--有償24
		,sl.RequestComment24--コメント24
		,sl.OptionTypeName25--区分25
		,sl.CarOptionCode25--品番25
		,sl.CarOptionName25--品名25
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount25, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount25, 0)), 1), '.00', '') END AS 'Amount25'--金額25
		,CASE WHEN sl.ClaimType25 = 1 THEN '有償' ELSE '' END AS 'ClaimType25'--有償25
		,sl.RequestComment25--コメント25
		,REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount1, 0) + ISNULL(sl.Amount2, 0) + ISNULL(sl.Amount3, 0) + ISNULL(sl.Amount4, 0) + ISNULL(sl.Amount5, 0) + ISNULL(sl.Amount6, 0) + ISNULL(sl.Amount7, 0) + ISNULL(sl.Amount8, 0) + ISNULL(sl.Amount9, 0) + ISNULL(sl.Amount10, 0) + ISNULL(sl.Amount11, 0) + ISNULL(sl.Amount12, 0) + ISNULL(sl.Amount13, 0) + ISNULL(sl.Amount14, 0) + ISNULL(sl.Amount15, 0) + ISNULL(sl.Amount16, 0) + ISNULL(sl.Amount17, 0) + ISNULL(sl.Amount18, 0) + ISNULL(sl.Amount19, 0) + ISNULL(sl.Amount20, 0) + ISNULL(sl.Amount21, 0) + ISNULL(sl.Amount22, 0) + ISNULL(sl.Amount23, 0) + ISNULL(sl.Amount24, 0) + ISNULL(sl.Amount25, 0)), 1), '.00', '') AS 'LineTotalAmount'


    FROM #temp_ServiceRequest_sr AS sr
	LEFT OUTER JOIN #temp_CarPurchaseOrder_po AS po ON po.SlipNumber = sr.OriginalSlipNumber
	LEFT OUTER JOIN #temp_ServiceRequestLineList_sl AS sl ON sl.ServiceRequestId = sr.ServiceRequestId
	LEFT OUTER JOIN #temp_CarSalesHeader_h AS h ON h.SlipNumber = po.SlipNumber
    LEFT OUTER JOIN dbo.Department AS d ON d.DepartmentCode = sr.DepartmentCode
	LEFT OUTER JOIN dbo.Customer c ON c.CustomerCode = h.CustomerCode
	LEFT OUTER JOIN dbo.SalesCar s ON s.SalesCarNumber = h.SalesCarNumber
	LEFT OUTER JOIN dbo.Location l ON l.LocationCode = s.LocationCode
	INNER JOIN dbo.c_NewUsedType AS c1 ON c1.Code = h.NewUsedType
	INNER JOIN dbo.c_SalesType AS c2 ON c2.Code = h.SalesType
	INNER JOIN dbo.c_MileageUnit AS c3 ON c3.Code = h.MileageUnit
	INNER JOIN dbo.c_OwnershipChange c4 ON c4.Code = sr.OwnershipChange
	INNER JOIN dbo.c_OnOff AS c5 ON c5.Code = sr.AnnualInspection
	INNER JOIN dbo.c_OnOff AS c6 ON c6.Code = sr.InsuranceInheritance
BEGIN

	BEGIN TRY
		DROP TABLE #temp_ServiceRequest_sr
		DROP TABLE #temp_ServiceRequestLineList_sl
		DROP TABLE #temp_CarPurchaseOrder_po
		DROP TABLE #temp_CarSalesHeader_h
	END TRY
	BEGIN CATCH
		--無視
	END CATCH

END




GO



