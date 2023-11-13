USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_MakeTransfar]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[W_MakeTransfar]
AS
BEGIN
	update 
		SalesCar 
	Set 
		 LocationCode=T.ArrivalLocationCode
		,LastUpdateEmployeeCode = 'SYS(W_MakeTransfar)'				--Add 2017/02/20 arc yano #3707 
		,LastUpdateDate = GETDATE()									--Add 2017/02/20 arc yano #3707
	from Transfer T
	inner join SalesCar S on T.SalesCarNumber=S.SalesCarNumber
	and T.ArrivalDate is null and datediff(d,T.LastUpdateDate,GETDATE()) <= 1
	and left(T.DepartureLocationCode,3) <> '052'

	update 
		Transfer 
	set 
		 ArrivalDate=ArrivalPlanDate
		,ArrivalEmployeeCode='SYS(W_MakeTransfar)'					--Add 2017/02/20 arc yano #3707
		,LastUpdateEmployeeCode = 'SYS(W_MakeTransfar)'				--Add 2017/02/20 arc yano #3707 
		,LastUpdateDate = GETDATE()									--Add 2017/02/20 arc yano #3707
	where 
		SalesCarNumber is not null
	and ArrivalDate is null 
	and datediff(d,LastUpdateDate,GETDATE()) <= 1
	and left(DepartureLocationCode,3) <> '052'

END
GO
