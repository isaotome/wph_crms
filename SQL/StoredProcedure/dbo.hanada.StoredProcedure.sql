USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[hanada]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[hanada]
	@POSCD char(13),
	@PRTDATE char(10)
AS 
BEGIN
	CREATE TABLE #TargetHeader (
		SlipNumber		nvarchar(50),
		Rev				int
	);

	-- テンポラリテーブルにこの後のクエリ―に必要なデータを INSERT しておく。

	INSERT INTO #TargetHeader
		select
			SlipNumber, RevisionNumber as Rev 
		from 
			CarSalesHeader 
		where 
			DepartmentCode=121
			AND cast(SalesOrderStatus as int) = 5	-- 納車済み
			AND DelFlag = 0
			AND LoanTotalAmount >0
--			AND PaymentTermFromA != NULL
--		group by
--			SlipNumber
		order by SlipNumber asc
	select
		a.SlipNumber, a.Rev, 
		b.CustomerCode, b.DepartmentCode, b.LoanTotalAmount, 
		b.PaymentPlanType,
		b.PaymentTermFromA, b.PaymentTermToA, 
		b.PaymentTermFromB, b.PaymentTermToB, 
		b.PaymentTermFromC, b.PaymentTermToC,
		c.CustomerName, c.PostCode, c.Prefecture, c.City, c.Address1, c.Address2, c.TelNumber
	from 
		#TargetHeader as a
	inner join 
		dbo.CarSalesHeader as b ON a.SlipNumber = b.SlipNumber AND a.Rev = b.RevisionNumber
	inner join 
		dbo.Customer as c ON b.CustomerCode = c.CustomerCode
--	where 
--		LoanTotalAmount >0
--		AND b.DepartmentCode=121
	select * from #targetHeader;
END
--		ch.DepartmentCode, LoanTotalAmount, PaymentTermFromA, PaymentTermToA, PaymentTermFromB, PaymentTermToB, PaymentTermFromC, PaymentTermToC,
GO
