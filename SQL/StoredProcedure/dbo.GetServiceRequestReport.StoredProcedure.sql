USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetServiceRequestReport]    Script Date: 2017/10/31 14:11:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--2021/06/09 yano #4091 �y�ԗ��`�[�z�I�v�V�����s�̋敪�̒ǉ�
--2017/10/30 arc yano #3785 �ԗ���ƈ˗��� ���F���ǉ�
--2017/02/21 arc nakayama #3626_�y�ԁz�ԗ��`�[�́u��ƈ˗����v�֎󒍌�ɒǉ�����Ȃ��@�V�K�쐬

CREATE PROCEDURE [dbo].[GetServiceRequestReport]

	@OriginalSlipNumber nvarchar(50)		--�`�[�ԍ�����ƈ˗����o�͗p�̏���-1���ɍi��ׂ̏�����

AS
	SET NOCOUNT ON	
	/*-------------------------------------------*/
	/* �_�[�e�B�[���[�h�̐ݒ�					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/* ��ƈ˗��e�[�u���i����		             */
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
	/* ��ƈ˗����� ��1�s�ɂ܂Ƃ߂�				 */
	/*-------------------------------------------*/
	
	CREATE TABLE #temp_ServiceRequestLineList_sl(
		 ServiceRequestId nvarchar(36)
		,OptionTypeName1 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode1 nvarchar(25)
		,CarOptionName1 nvarchar(100)
		,Amount1 decimal(10,0)
		,ClaimType1 bit
		,RequestComment1 nvarchar(100)
		,OptionTypeName2 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode2 nvarchar(25)
		,CarOptionName2 nvarchar(100)
		,Amount2 decimal(10,0)
		,ClaimType2 bit
		,RequestComment2 nvarchar(100)
		,OptionTypeName3 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode3 nvarchar(25)
		,CarOptionName3 nvarchar(100)
		,Amount3 decimal(10,0)
		,ClaimType3 bit
		,RequestComment3 nvarchar(100)
		,OptionTypeName4 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode4 nvarchar(25)
		,CarOptionName4 nvarchar(100)
		,Amount4 decimal(10,0)
		,ClaimType4 bit
		,RequestComment4 nvarchar(100)
		,OptionTypeName5 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode5 nvarchar(25)
		,CarOptionName5 nvarchar(100)
		,Amount5 decimal(10,0)
		,ClaimType5 bit
		,RequestComment5 nvarchar(100)
		,OptionTypeName6 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode6 nvarchar(25)
		,CarOptionName6 nvarchar(100)
		,Amount6 decimal(10,0)
		,ClaimType6 bit
		,RequestComment6 nvarchar(100)
		,OptionTypeName7 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode7 nvarchar(25)
		,CarOptionName7 nvarchar(100)
		,Amount7 decimal(10,0)
		,ClaimType7 bit
		,RequestComment7 nvarchar(100)
		,OptionTypeName8 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode8 nvarchar(25)
		,CarOptionName8 nvarchar(100)
		,Amount8 decimal(10,0)
		,ClaimType8 bit
		,RequestComment8 nvarchar(100)
		,OptionTypeName9 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode9 nvarchar(25)
		,CarOptionName9 nvarchar(100)
		,Amount9 decimal(10,0)
		,ClaimType9 bit
		,RequestComment9 nvarchar(100)
		,OptionTypeName10 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode10 nvarchar(25)
		,CarOptionName10 nvarchar(100)
		,Amount10 decimal(10,0)
		,ClaimType10 bit
		,RequestComment10 nvarchar(100)
		,OptionTypeName11 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode11 nvarchar(25)
		,CarOptionName11 nvarchar(100)
		,Amount11 decimal(10,0)
		,ClaimType11 bit
		,RequestComment11 nvarchar(100)
		,OptionTypeName12 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode12 nvarchar(25)
		,CarOptionName12 nvarchar(100)
		,Amount12 decimal(10,0)
		,ClaimType12 bit
		,RequestComment12 nvarchar(100)
		,OptionTypeName13 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode13 nvarchar(25)
		,CarOptionName13 nvarchar(100)
		,Amount13 decimal(10,0)
		,ClaimType13 bit
		,RequestComment13 nvarchar(100)
		,OptionTypeName14 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode14 nvarchar(25)
		,CarOptionName14 nvarchar(100)
		,Amount14 decimal(10,0)
		,ClaimType14 bit
		,RequestComment14 nvarchar(100)
		,OptionTypeName15 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode15 nvarchar(25)
		,CarOptionName15 nvarchar(100)
		,Amount15 decimal(10,0)
		,ClaimType15 bit
		,RequestComment15 nvarchar(100)
		,OptionTypeName16 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode16 nvarchar(25)
		,CarOptionName16 nvarchar(100)
		,Amount16 decimal(10,0)
		,ClaimType16 bit
		,RequestComment16 nvarchar(100)
		,OptionTypeName17 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode17 nvarchar(25)
		,CarOptionName17 nvarchar(100)
		,Amount17 decimal(10,0)
		,ClaimType17 bit
		,RequestComment17 nvarchar(100)
		,OptionTypeName18 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode18 nvarchar(25)
		,CarOptionName18 nvarchar(100)
		,Amount18 decimal(10,0)
		,ClaimType18 bit
		,RequestComment18 nvarchar(100)
		,OptionTypeName19 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode19 nvarchar(25)
		,CarOptionName19 nvarchar(100)
		,Amount19 decimal(10,0)
		,ClaimType19 bit
		,RequestComment19 nvarchar(100)
		,OptionTypeName20 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode20 nvarchar(25)
		,CarOptionName20 nvarchar(100)
		,Amount20 decimal(10,0)
		,ClaimType20 bit
		,RequestComment20 nvarchar(100)
		,OptionTypeName21 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode21 nvarchar(25)
		,CarOptionName21 nvarchar(100)
		,Amount21 decimal(10,0)
		,ClaimType21 bit
		,RequestComment21 nvarchar(100)
		,OptionTypeName22 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode22 nvarchar(25)
		,CarOptionName22 nvarchar(100)
		,Amount22 decimal(10,0)
		,ClaimType22 bit
		,RequestComment22 nvarchar(100)
		,OptionTypeName23 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode23 nvarchar(25)
		,CarOptionName23 nvarchar(100)
		,Amount23 decimal(10,0)
		,ClaimType23 bit
		,RequestComment23 nvarchar(100)
		,OptionTypeName24 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode24 nvarchar(25)
		,CarOptionName24 nvarchar(100)
		,Amount24 decimal(10,0)
		,ClaimType24 bit
		,RequestComment24 nvarchar(100)
		,OptionTypeName25 nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
		,CarOptionCode25 nvarchar(25)
		,CarOptionName25 nvarchar(100)
		,Amount25 decimal(10,0)
		,ClaimType25 bit
		,RequestComment25 nvarchar(100)

	)

	declare @Linecount int --����
	declare @Loopcount int = 0 --���[�v�J�E���g

	DECLARE
	     @ServiceRequestId nvarchar(36)
		,@OptionTypeName nvarchar(50)		--Mod 2021/06/09 yano #4091 nvarchar(3)��nvarchar(50)
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
	/* �ԗ������i����			�@�@			 */
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
	/* �˗����Ƃ̎ԗ��`�`�[����	�@�@			 */
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
	/* ��ƈ˗������							 */
	/*-------------------------------------------*/

	SELECT
		 sr.OriginalSlipNumber				--�ԗ��`�[�ԍ�
		,c1.Name as 'NewUsedType'			--�V���敪
		,c2.Name as 'SalesType'				--�̔��敪
		,h.CarGradeCode						--�O���[�h�R�[�h
		,h.MakerName						--���[�J�[��
		,h.CarBrandName						--�u�����h��
		,h.CarName							--�Ԏ햼
		,h.CarGradeName						--�O���[�h��
		,h.ExteriorColorName				--�O���F
		,h.InteriorColorName				--�����F
		,h.Vin								--�ԑ�ԍ�
		,h.modelName						--�^��
		,h.Mileage							--���s����
		,c3.Name as 'MileageUnit'			--���s�����P��
		,h.CustomerCode						--�ڋq�R�[�h
		,c.CustomerName						--�ڋq��
		,c.CustomerNameKana					--�ڋq���i�J�i�j
		,c.Prefecture						--�Z��
		,c.City
		,c.Address1
		,c.Address2
		,d.DepartmentName					--�˗��敔��
		,sr.Memo							--���l
		,l.LocationName						--���ݒn
		,CONVERT(NVARCHAR, po.MakerShipmentDate, 111) AS 'MakerShipmentDate'	--�o�ח\���
		,CONVERT(NVARCHAR, po.ArrivalPlanDate, 111) AS 'ArrivalPlanDate'		--�����\���
		,CONVERT(NVARCHAR, po.RegistrationPlanDate, 111) AS 'RegistrationPlanDate'--�o�^�\���
		,CONVERT(NVARCHAR, h.SalesPlanDate, 111) AS 'SalesPlanDate'		--�[�ԗ\���
		,c4.Name as 'OwnershipChange'		--�敪
		,c5.Name as 'AnnualInspection'		--12�����_��
		,c6.Name as 'InsuranceInheritance'	--�ۏ،p��
		,CONVERT(NVARCHAR, sr.DeliveryRequirement, 111) AS 'DeliveryRequirement'	--��]�[��
		,sl.OptionTypeName1	--�敪1
		,sl.CarOptionCode1	--�i��1
		,sl.OptionTypeName1	--�敪1
		,sl.CarOptionCode1	--�i��1
		,sl.CarOptionName1	--�i��1
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount1, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount1, 0)), 1), '.00', '') END AS 'Amount1'		--���z1
		,CASE WHEN sl.ClaimType1 = 1 THEN '�L��' ELSE '' END AS 'ClaimType1'--�L��1
		,sl.RequestComment1	--�R�����g1
		,sl.OptionTypeName2	--�敪2
		,sl.CarOptionCode2	--�i��2
		,sl.CarOptionName2	--�i��2
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount2, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount2, 0)), 1), '.00', '') END AS 'Amount2'		--���z2
		,CASE WHEN sl.ClaimType2 = 1 THEN '�L��' ELSE '' END AS 'ClaimType2'--�L��2
		,sl.RequestComment2	--�R�����g2
		,sl.OptionTypeName3	--�敪3
		,sl.CarOptionCode3	--�i��3
		,sl.CarOptionName3	--�i��3
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount3, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount3, 0)), 1), '.00', '') END AS 'Amount3'		--���z3
		,CASE WHEN sl.ClaimType3 = 1 THEN '�L��' ELSE '' END AS 'ClaimType3'--�L��3
		,sl.RequestComment3	--�R�����g3
		,sl.OptionTypeName4	--�敪4
		,sl.CarOptionCode4	--�i��4
		,sl.CarOptionName4	--�i��4
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount4, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount4, 0)), 1), '.00', '') END AS 'Amount4'		--���z4
		,CASE WHEN sl.ClaimType4 = 1 THEN '�L��' ELSE '' END AS 'ClaimType4'--�L��4
		,sl.RequestComment4	--�R�����g4
		,sl.OptionTypeName5	--�敪5
		,sl.CarOptionCode5	--�i��5
		,sl.CarOptionName5	--�i��5
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount5, 0)), 1), '.00', '') = '0'	THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount5, 0)), 1), '.00', '') END AS 'Amount5'		--���z5
		,CASE WHEN sl.ClaimType5 = 1 THEN '�L��' ELSE '' END AS 'ClaimType5'--�L��5
		,sl.RequestComment5	--�R�����g5
		,sl.OptionTypeName6	--�敪6
		,sl.CarOptionCode6	--�i��6
		,sl.CarOptionName6	--�i��6
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount6, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount6, 0)), 1), '.00', '') END AS 'Amount6'		--���z6
		,CASE WHEN sl.ClaimType6 = 1 THEN '�L��' ELSE '' END AS 'ClaimType6'--�L��6
		,sl.RequestComment6	--�R�����g6
		,sl.OptionTypeName7	--�敪7
		,sl.CarOptionCode7	--�i��7
		,sl.CarOptionName7	--�i��7
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount7, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount7, 0)), 1), '.00', '') END AS 'Amount7'		--���z7
		,CASE WHEN sl.ClaimType7 = 1 THEN '�L��' ELSE '' END AS 'ClaimType7'--�L��7
		,sl.RequestComment7	--�R�����g7
		,sl.OptionTypeName8	--�敪8
		,sl.CarOptionCode8	--�i��8
		,sl.CarOptionName8	--�i��8
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount8, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount8, 0)), 1), '.00', '') END AS 'Amount8'		--���z8
		,CASE WHEN sl.ClaimType8 = 1 THEN '�L��' ELSE '' END AS 'ClaimType8'--�L��8
		,sl.RequestComment8	--�R�����g8
		,sl.OptionTypeName9	--�敪9
		,sl.CarOptionCode9	--�i��9
		,sl.CarOptionName9	--�i��9
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount9, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount9, 0)), 1), '.00', '') END AS 'Amount9'		--���z9
		,CASE WHEN sl.ClaimType9 = 1 THEN '�L��' ELSE '' END AS 'ClaimType9'--�L��9
		,sl.RequestComment9	--�R�����g9
		,sl.OptionTypeName10	--�敪10
		,sl.CarOptionCode10		--�i��10
		,sl.CarOptionName10		--�i��10
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount10, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount10, 0)), 1), '.00', '') END AS 'Amount10'		--���z10
		,CASE WHEN sl.ClaimType10 = 1 THEN '�L��' ELSE '' END AS 'ClaimType10'--�L��10
		,sl.RequestComment10	--�R�����g10
		,sl.OptionTypeName11	--�敪11
		,sl.CarOptionCode11		--�i��11
		,sl.CarOptionName11		--�i��11
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount11, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount11, 0)), 1), '.00', '') END AS 'Amount11'		--���z11
		,CASE WHEN sl.ClaimType11 = 1 THEN '�L��' ELSE '' END AS 'ClaimType11'--�L��11
		,sl.RequestComment11	--�R�����g11
		,sl.OptionTypeName12	--�敪12
		,sl.CarOptionCode12		--�i��12
		,sl.CarOptionName12		--�i��12
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount12, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount12, 0)), 1), '.00', '') END AS 'Amount12'		--���z12
		,CASE WHEN sl.ClaimType12 = 1 THEN '�L��' ELSE '' END AS 'ClaimType12'--�L��12
		,sl.RequestComment12	--�R�����g12
		,sl.OptionTypeName13	--�敪13
		,sl.CarOptionCode13		--�i��13
		,sl.CarOptionName13		--�i��13
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount13, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount13, 0)), 1), '.00', '') END AS 'Amount13'		--���z13
		,CASE WHEN sl.ClaimType13 = 1 THEN '�L��' ELSE '' END AS 'ClaimType13'--�L��13
		,sl.RequestComment13	--�R�����g13
		,sl.OptionTypeName14	--�敪14
		,sl.CarOptionCode14		--�i��14
		,sl.CarOptionName14		--�i��14
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount14, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount14, 0)), 1), '.00', '') END AS 'Amount14'		--���z14
		,CASE WHEN sl.ClaimType14 = 1 THEN '�L��' ELSE '' END AS 'ClaimType14'--�L��14
		,sl.RequestComment14	--�R�����g14
		,sl.OptionTypeName15	--�敪15
		,sl.CarOptionCode15		--�i��15
		,sl.CarOptionName15		--�i��15
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount15, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount15, 0)), 1), '.00', '') END AS 'Amount15'		--���z15
		,CASE WHEN sl.ClaimType15 = 1 THEN '�L��' ELSE '' END AS 'ClaimType15'--�L��15
		,sl.RequestComment15	--�R�����g15
		,sl.OptionTypeName16--�敪16
		,sl.CarOptionCode16--�i��16
		,sl.CarOptionName16--�i��16
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount16, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount16, 0)), 1), '.00', '') END AS 'Amount16'--���z16
		,CASE WHEN sl.ClaimType16 = 1 THEN '�L��' ELSE '' END AS 'ClaimType16'--�L��16
		,sl.RequestComment16--�R�����g16
		,sl.OptionTypeName17--�敪17
		,sl.CarOptionCode17--�i��17
		,sl.CarOptionName17--�i��17
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount17, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount17, 0)), 1), '.00', '') END AS 'Amount17'--���z17
		,CASE WHEN sl.ClaimType17 = 1 THEN '�L��' ELSE '' END AS 'ClaimType17'--�L��17
		,sl.RequestComment17--�R�����g17
		,sl.OptionTypeName18--�敪18
		,sl.CarOptionCode18--�i��18
		,sl.CarOptionName18--�i��18
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount18, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount18, 0)), 1), '.00', '') END AS 'Amount18'--���z18
		,CASE WHEN sl.ClaimType18 = 1 THEN '�L��' ELSE '' END AS 'ClaimType18'--�L��18
		,sl.RequestComment18--�R�����g18
		,sl.OptionTypeName19--�敪19
		,sl.CarOptionCode19--�i��19
		,sl.CarOptionName19--�i��19
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount19, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount19, 0)), 1), '.00', '') END AS 'Amount19'--���z19
		,CASE WHEN sl.ClaimType19 = 1 THEN '�L��' ELSE '' END AS 'ClaimType19'--�L��19
		,sl.RequestComment19--�R�����g19
		,sl.OptionTypeName20--�敪20
		,sl.CarOptionCode20--�i��20
		,sl.CarOptionName20--�i��20
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount20, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount20, 0)), 1), '.00', '') END AS 'Amount20'--���z20
		,CASE WHEN sl.ClaimType20 = 1 THEN '�L��' ELSE '' END AS 'ClaimType20'--�L��20
		,sl.RequestComment20--�R�����g20
		,sl.OptionTypeName21--�敪21
		,sl.CarOptionCode21--�i��21
		,sl.CarOptionName21--�i��21
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount21, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount21, 0)), 1), '.00', '') END AS 'Amount21'--���z21
		,CASE WHEN sl.ClaimType21 = 1 THEN '�L��' ELSE '' END AS 'ClaimType21'--�L��21
		,sl.RequestComment21--�R�����g21
		,sl.OptionTypeName22--�敪22
		,sl.CarOptionCode22--�i��22
		,sl.CarOptionName22--�i��22
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount22, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount22, 0)), 1), '.00', '') END AS 'Amount22'--���z22
		,CASE WHEN sl.ClaimType22 = 1 THEN '�L��' ELSE '' END AS 'ClaimType22'--�L��22
		,sl.RequestComment22--�R�����g22
		,sl.OptionTypeName23--�敪23
		,sl.CarOptionCode23--�i��23
		,sl.CarOptionName23--�i��23
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount23, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount23, 0)), 1), '.00', '') END AS 'Amount23'--���z23
		,CASE WHEN sl.ClaimType23 = 1 THEN '�L��' ELSE '' END AS 'ClaimType23'--�L��23
		,sl.RequestComment23--�R�����g23
		,sl.OptionTypeName24--�敪24
		,sl.CarOptionCode24--�i��24
		,sl.CarOptionName24--�i��24
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount24, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount24, 0)), 1), '.00', '') END AS 'Amount24'--���z24
		,CASE WHEN sl.ClaimType24 = 1 THEN '�L��' ELSE '' END AS 'ClaimType24'--�L��24
		,sl.RequestComment24--�R�����g24
		,sl.OptionTypeName25--�敪25
		,sl.CarOptionCode25--�i��25
		,sl.CarOptionName25--�i��25
		,CASE WHEN REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount25, 0)), 1), '.00', '') = '0' THEN '' ELSE REPLACE(CONVERT(nvarchar,CONVERT(money, ISNULL(sl.Amount25, 0)), 1), '.00', '') END AS 'Amount25'--���z25
		,CASE WHEN sl.ClaimType25 = 1 THEN '�L��' ELSE '' END AS 'ClaimType25'--�L��25
		,sl.RequestComment25--�R�����g25
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
		--����
	END CATCH

END




GO



