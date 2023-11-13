USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[CalcuratePartsAverageCost]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[CalcuratePartsAverageCost]
	@TargetMonth datetime,
	@EmployeeCode nvarchar(50)
as
begin
insert into PartsAverageCost
select 
	CompanyCode,
	@TargetMonth,
	PartsNumber,
	AveragePrice,
	GETDATE(),
	GETDATE(),
	@EmployeeCode,
	GETDATE(),
	@EmployeeCode,
	'0'
from V_PartsAverageCostTarget
where TargetMonth=@TargetMonth

insert into PartsAverageCostControl values(@TargetMonth,'0')
end
GO
