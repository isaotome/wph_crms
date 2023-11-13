USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCustomerIntegrateData]    Script Date: 2017/03/29 13:13:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





--2017/03/18 arc nakayama #3722_名寄せツール　新規作成

CREATE PROCEDURE [dbo].[GetCustomerIntegrateData]

	@CustomerCode nvarchar(10)		--顧客コード
AS
	SET NOCOUNT ON

	SELECT 
		C.CustomerCode,
		CustomerName,
		TelNumber,
		MobileNumber,
		PostCode,
		Prefecture,
		City,
		Address1,
		Address2,
		A.CarCnt,
		B.ServiceCnt
	FROM Customer C
	LEFT OUTER JOIN (
					select CustomerCode,
							COUNT(*) AS CarCnt
					from CarSalesHeader 
					where DelFlag='0'
						and CustomerCode = @CustomerCode 
					Group By CustomerCode
					) A ON C.CustomerCode=A.CustomerCode
	LEFT OUTER JOIN (
					select CustomerCode,
						   COUNT(*) AS ServiceCnt 
					from ServiceSalesHeader
					where DelFlag='0' and CustomerCode = @CustomerCode
					Group By CustomerCode
					) B ON C.CustomerCode=B.CustomerCode
	WHERE C.DelFlag='0'
	  AND C.CustomerCode = @CustomerCode
GO


