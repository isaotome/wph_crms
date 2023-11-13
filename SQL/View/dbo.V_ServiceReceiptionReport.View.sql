USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_ServiceReceiptionReport]    Script Date: 2020/06/09 16:39:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- ----------------------------------------------------------------------------------
-- 機能：車両整備受付
-- 作成日：???
-- 更新日：
--		  2020/06/05 yano #4053【サービス受付】問診表にメールアドレス追加
--		  2019/08/30 yano #3962 サービス受付　車両整備・修理受付問診表に営業担当追加
--		  2019/05/22 yano #3988 【サービス受付】承り書（車両修正・受付票）出力時に顧客住所が表示されない
--		  2019/02/07 yano #3962 サービス受付　車両整備・修理受付問診表に営業担当追加
--		  2017/11/02 arc yano #3797 車両整備受付対応
-- ----------------------------------------------------------------------------------
CREATE VIEW [dbo].[V_ServiceReceiptionReport]
AS
SELECT
	 a.CarReceiptionId AS CarReceiptionId
	,YEAR(a.ReceiptionDate) AS ReceiptionDateYear
	,MONTH(a.ReceiptionDate) AS ReceiptionDateMonth
	,DAY(a.ReceiptionDate) AS ReceiptionDateDay
	,YEAR(a.ArrivalPlanDate) AS ArrivalPlanDateYear
	,MONTH(a.ArrivalPlanDate) AS ArrivalPlanDateMonth
	,DAY(a.ArrivalPlanDate) AS ArrivalPlanDateDay
	,f.CustomerName AS CustomerName
	,f.TelNumber AS TelNumber
	,f.MobileNumber AS MobileNumber --Add 2017/11/02 arc yano 3797
	,f.FaxNumber AS FaxNumber
	,ISNULL(f.Prefecture, '') + ISNULL(f.City, '') + ISNULL(f.Address1, '') + ISNULL(f.Address2, '') AS CustomerAddress		--Mod  2019/05/22 yano #3988
	,f.PostCode AS PostCode
	,f.Prefecture AS Prefecture
	,f.City AS City
	,f.Address1 AS Address1
	,f.Address2 AS Address2
	,f.CarEmployeeCode AS CarEmployeeCode		--Add 2019/02/07 yano #3962
	,g.FullName AS DepartmentName
	,h.FullName AS OfficeName
	,ISNULL(g.Prefecture, '') + ISNULL(g.City, '') + ISNULL(g.Address1, '') + ISNULL(g.Address2, '') AS DepartmentAddress	--Mod  2019/05/22 yano #3988
	,g.Prefecture AS DepartmentPrefecture
	,g.City AS DepartmentCity
	,g.Address1 AS DepartmentAddress1
	,g.Address2 AS DepartmentAddress2
	,g.TelNumber1 AS DepartmentTelNumber
	,g.TelNumber2 AS DepartmentTelNumber2
	,g.FaxNumber AS DepartmentFaxNumber
	,e.MakerName AS MakerName
	,d.CarBrandName AS CarBrandName
	,c.CarName AS CarName
	,b.CarGradeName AS CarGradeName
	,a.MorterViecleOfficialCode AS MorterViecleOfficialCode
	,a.RegistrationNumberType AS RegistrationNumberType
	,a.RegistrationNumberKana AS RegistrationNumberKana
	,a.RegistrationNumberPlate AS RegistrationNumberPlate
	,a.Mileage AS Mileage
	,a.Vin AS Vin
	,i.CompanyName AS CompanyName
	,j.EmployeeName AS EmployeeName
	,k.ManufacturingYear AS ManufacturingYear
	,k.PossesorName as OwnerName
	,g.StoreName AS StoreName									--FCA登録屋号		--Add 2017/11/02 arc yano 3797
	,l.EmployeeName AS CarEmployeeName							--Add 2019/02/07 yano #3962
	,f.MailAddress AS CustomerMailAddress						--Add 2020/06/05 yano #4053
	,f.MobileMailAddress AS CustomerMobileMailAddress			--Add 2020/06/05 yano #4053
	,CASE WHEN ISDATE(a.FirstRegistrationYear + '/01') = 1 THEN YEAR(CONVERT(datetime, a.FirstRegistrationYear + '/01')) ELSE NULL END AS FirstRegistrationYear		--初年度登録(年)	--Add 2017/11/02 arc yano 3797
	,CASE WHEN ISDATE(a.FirstRegistrationYear + '/01') = 1 THEN MONTH(CONVERT(datetime, a.FirstRegistrationYear + '/01')) ELSE NULL END AS FirstRegistrationMonth	--初年度登録(月)	--Add 2017/11/02 arc yano 3797
	
FROM
	CustomerReceiption a
	left join CarGrade b on a.CarGradeCode = b.CarGradeCode
	left join Car c on b.CarCode = c.CarCode
	left join Brand d on c.CarBrandCode = d.CarBrandCode
	left join Maker e on d.MakerCode = e.MakerCode
	left join Customer f on a.CustomerCode = f.CustomerCode
	left join Department g on a.DepartmentCode = g.DepartmentCode
	left join Office h on g.OfficeCode = h.OfficeCode
	left join Company i on h.CompanyCode = i.CompanyCode
	left join Employee j on a.EmployeeCode = j.EmployeeCode
	left join SalesCar k on a.SalesCarNumber = k.SalesCarNumber
	left join Employee l on f.CarEmployeeCode = l.EmployeeCode		--Add 2019/02/07 yano #3962

GO


