USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_CarAppraisalReport]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_CarAppraisalReport]
as
select
	a1.CarAppraisalId
	,a1.FirstRegistrationYear
	,a1.InspectionExpireDate
	,a1.UserName
	,a1.MakerName
	,a1.CarName
	,a1.CarGradeName
	,a1.ModelName
	,a1.Displacement
	,a1.Figure
	,a1.Door
	,a1.TransMission
	,a1.ModelSpecificateNumber
	,a1.ExteriorColorName
	,a1.InteriorColorName
	,a1.ChangeColor
	,a1.OriginalColorName
	,a1.Capacity
	,a1.Guarantee
	,a1.Instructions
	,a1.ClassificationTypeNumber
	,a1.Steering
	,a1.Import
	,a1.Light
	,a1.Aw
	,a1.Aero
	,a1.Sr
	,a1.Cd
	,a1.Md
	,a1.NaviType
	,a1.NaviEquipment
	,a1.NaviDashboard
	,a1.SeatColor
	,a1.SeatType
	,a1.MorterViecleOfficialCode
	,a1.RegistrationNumberType
	,a1.Recycle
	,a1.RecycleTicket
	,a1.RecycleDeposit
	,case 
		when LEN(a1.RecycleDeposit)>0 then RIGHT(CONVERT(varchar,a1.RecycleDeposit),1)
		else null
	end as Recycle1
	,case
		when LEN(a1.RecycleDeposit)>1 then SUBSTRING(CONVERT(varchar,a1.RecycleDeposit),LEN(a1.RecycleDeposit)-1,1)
		else null
	end as Recycle2
	,case 
		when LEN(a1.RecycleDeposit)>2 then SUBSTRING(CONVERT(varchar,a1.RecycleDeposit),LEN(a1.RecycleDeposit)-2,1)
		else null
	end as Recycle3
	,case
		when LEN(a1.RecycleDeposit)>3 then SUBSTRING(CONVERT(varchar,a1.RecycleDeposit),LEN(a1.RecycleDeposit)-3,1)
		else null
	end as Recycle4
	,case
		when LEN(a1.RecycleDeposit)>4 then SUBSTRING(CONVERT(varchar,a1.RecycleDeposit),LEN(a1.RecycleDeposit)-4,1)
		else null
	end as Recycle5
	,a1.RegistrationNumberKana
	,a1.RegistrationNumberPlate
	,case
		when LEN(a1.RegistrationNumberPlate)>0 then SUBSTRING(a1.RegistrationNumberPlate,1,1)
		else null
	end as NumberPlate1
	,case 
		when LEN(a1.RegistrationNumberPlate)>1 then SUBSTRING(a1.RegistrationNumberPlate,2,1)
		else null
	end as NumberPlate2
	,case
		when LEN(a1.RegistrationNumberPlate)>2 then SUBSTRING(a1.RegistrationNumberPlate,3,1)
		else null
	end as NumberPlate3
	,case
		when LEN(a1.RegistrationNumberPlate)>3 then SUBSTRING(a1.RegistrationNumberPlate,4,1)
		else null
	end as NumberPlate4
	,a1.UsVin
	,case
		when LEN(a1.UsVin)>0 then SUBSTRING(a1.UsVin,1,1)
		else null
	end as Serial1
	,case
		when LEN(a1.UsVin)>1 then SUBSTRING(a1.UsVin,2,1)
		else null
	end as Serial2
	,case
		when LEN(a1.UsVin)>2 then SUBSTRING(a1.UsVin,3,1)
		else null
	end as Serial3
	,case
		when LEN(a1.UsVin)>3 then SUBSTRING(a1.UsVin,4,1)
		else null
	end as Serial4
	,case
		when LEN(a1.UsVin)>4 then SUBSTRING(a1.UsVin,5,1)
		else null
	end as Serial5
	,case
		when LEN(a1.UsVin)>5 then SUBSTRING(a1.UsVin,6,1)
		else null
	end as Serial6
	,case
		when LEN(a1.UsVin)>6 then SUBSTRING(a1.UsVin,7,1)
		else null
	end as Serial7
	,case
		when LEN(a1.UsVin)>7 then SUBSTRING(a1.UsVin,8,1)
		else null
	end as Serial8
	,case
		when LEN(a1.UsVin)>8 then SUBSTRING(a1.UsVin,9,1)
		else null
	end as Serial9
	,case
		when LEN(a1.UsVin)>9 then SUBSTRING(a1.UsVin,10,1)
		else null
	end as Serial10
	,case
		when LEN(a1.UsVin)>10 then SUBSTRING(a1.UsVin,11,1)
		else null
	end as Serial11
	,case
		when LEN(a1.UsVin)>11 then SUBSTRING(a1.UsVin,12,1)
		else null
	end as Serial12
	,case
		when LEN(a1.UsVin)>12 then SUBSTRING(a1.UsVin,13,1)
		else null
	end as Serial13
	,case
		when LEN(a1.UsVin)>13 then SUBSTRING(a1.UsVin,14,1)
		else null
	end as Serial14
	,case
		when LEN(a1.UsVin)>14 then SUBSTRING(a1.UsVin,15,1)
		else null
	end as Serial15
	,case
		when LEN(a1.UsVin)>15 then SUBSTRING(a1.UsVin,16,1)
		else null
	end as Serial16
	,case
		when LEN(a1.UsVin)>16 then SUBSTRING(a1.UsVin,17,1)
		else null
	end as Serial17
	,case
		when LEN(a1.UsVin)>17 then SUBSTRING(a1.UsVin,18,1)
		else null
	end as Serial18
	
	,a1.Mileage
	,case
		when LEN(a1.Mileage)>0 then SUBSTRING(CONVERT(varchar,CONVERT(int,a1.Mileage)),LEN(CONVERT(int,a1.Mileage)),1)
		else null
	end as AppraisalMileage1
	,case
		when LEN(a1.Mileage)>1 then SUBSTRING(CONVERT(varchar,CONVERT(int,a1.Mileage)),LEN(CONVERT(int,a1.Mileage))-1,1)
		else null
	end as AppraisalMileage2
	,case
		when LEN(a1.Mileage)>2 then SUBSTRING(CONVERT(varchar,CONVERT(int,a1.Mileage)),LEN(CONVERT(int,a1.Mileage))-2,1)
		else null
	end as AppraisalMileage3
	,case
		when LEN(a1.Mileage)>3 then SUBSTRING(CONVERT(varchar,CONVERT(int,a1.Mileage)),LEN(CONVERT(int,a1.Mileage))-3,1)
		else null
	end as AppraisalMileage4
	,case
		when LEN(a1.Mileage)>4 then SUBSTRING(CONVERT(varchar,CONVERT(int,a1.Mileage)),LEN(CONVERT(int,a1.Mileage))-4,1)
		else null
	end as AppraisalMileage5
	,case
		when LEN(a1.Mileage)>5 then SUBSTRING(CONVERT(varchar,CONVERT(int,a1.Mileage)),LEN(CONVERT(int,a1.Mileage))-5,1)
		else null
	end as AppraisalMileage6
	,a1.MileageUnit as AppraisalMileageUnit
	,a1.ExteriorEvaluation
	,a1.InteriorEvaluation
	,a1.ReparationRecord
	,a1.AppraisalPrice
	,a1.Remarks
	,a1.ModelYear
	,a3.OfficeName
	,a4.EmployeeName
	,a5.SalesCarNumber
	,case 
		when a5.PurchaseStatus = '002' then a5.TotalAmount
		else null
	end as PurchaseAmount
	,a1.Vin
	,case 
		when len(a1.Vin)>0 then SUBSTRING(a1.Vin,1,1)
		else null
	end as Vin1
	,case
		when LEN(a1.Vin)>1 then SUBSTRING(a1.Vin,2,1)
		else null
	end as Vin2
	,case
		when LEN(a1.Vin)>2 then SUBSTRING(a1.Vin,3,1)
		else null
	end as Vin3
	,case
		when LEN(a1.Vin)>3 then SUBSTRING(a1.Vin,4,1)
		else null
	end as Vin4
	,case
		when LEN(a1.Vin)>4 then SUBSTRING(a1.Vin,5,1)
		else null
	end as Vin5
	,case 
		when LEN(a1.Vin)>5 then SUBSTRING(a1.Vin,6,1)
		else null
	end as Vin6
	,case
		when LEN(a1.Vin)>6 then SUBSTRING(a1.Vin,7,1)
		else null
	end as Vin7
	,case
		when LEN(a1.Vin)>7 then SUBSTRING(a1.Vin,8,1)
		else null
	end as Vin8
	,case
		when LEN(a1.Vin)>8 then SUBSTRING(a1.Vin,9,1)
		else null
	end as Vin9
	,case
		when LEN(a1.Vin)>9 then SUBSTRING(a1.Vin,10,1)
		else null
	end as Vin10
	,case
		when LEN(a1.Vin)>10 then SUBSTRING(a1.Vin,11,1)
		else null
	end as Vin11
	,case
		when LEN(a1.Vin)>11 then SUBSTRING(a1.Vin,12,1)
		else null
	end as Vin12
	,case
		when LEN(a1.Vin)>12 then SUBSTRING(a1.Vin,13,1)
		else null
	end as Vin13
	,case
		when LEN(a1.Vin)>13 then SUBSTRING(a1.Vin,14,1)
		else null
	end as Vin14
	,case
		when LEN(a1.Vin)>14 then SUBSTRING(a1.Vin,15,1)
		else null
	end as Vin15
	,case
		when LEN(a1.Vin)>15 then SUBSTRING(a1.Vin,16,1)
		else null
	end as Vin16
	,case
		when LEN(a1.Vin)>16 then SUBSTRING(a1.Vin,17,1)
		else null
	end as Vin17
	,case
		when LEN(a1.Vin)>17 then SUBSTRING(a1.Vin,18,1)
		else null
	end as Vin18
	,case
		when LEN(a1.Vin)>18 then SUBSTRING(a1.Vin,19,1)
		else null
	end as Vin19
	,case
		when LEN(a1.Vin)>19 then SUBSTRING(a1.Vin,20,1)
		else null
	end as Vin20
	,a5.TotalAmount
	,a3.FullName
	,a8.CompanyName
	,a1.CreateDate
	,a9.ShortName as Fuel
	,a1.AppraisalDate
from CarAppraisal a1
left join Department a2
on a1.DepartmentCode=a2.DepartmentCode
left join Office a3
on a2.OfficeCode=a3.OfficeCode
left join Employee a4
on a1.EmployeeCode=a4.EmployeeCode
left join CarPurchase a5
on a1.CarAppraisalId = a5.CarAppraisalId
left join Company a8
on a3.CompanyCode = a8.CompanyCode
left join c_Fuel a9
on a1.Fuel=a9.Code
GO
