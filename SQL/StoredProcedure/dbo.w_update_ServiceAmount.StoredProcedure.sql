USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[w_update_ServiceAmount]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[w_update_ServiceAmount]
AS
BEGIN
	update ServiceSalesLine set Amount = 0 where Amount is null and DelFlag='0' and DATEDIFF(d,lastupdatedate,getdate()) < 30
	update ServiceSalesLine set TaxAmount = 0 where TaxAmount is null and DelFlag='0' and DATEDIFF(d,lastupdatedate,getdate()) < 30
	update ServiceSalesLine set cost = 0 where Cost is null and DelFlag='0' and DATEDIFF(d,lastupdatedate,getdate()) < 30
	update ServiceSalesLine set TechnicalFeeAmount = 0 where TechnicalFeeAmount is null and DelFlag='0' and DATEDIFF(d,lastupdatedate,getdate()) < 30
END
GO
