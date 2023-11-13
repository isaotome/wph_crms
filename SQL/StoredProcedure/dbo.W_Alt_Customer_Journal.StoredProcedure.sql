USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_Alt_Customer_Journal]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[W_Alt_Customer_Journal]
AS
BEGIN

update Journal Set CustomerClaimCode = H.CustomerCode
 from Journal J
inner join ( Select SlipNumber,CustomerCode from CarSalesHeader where DelFlag='0' union all 
 Select SlipNumber,CustomerCode from ServiceSalesHeader where DelFlag='0' ) H
 on J.SlipNumber=H.SlipNumber 
where (J.CustomerClaimCode is null or J.CustomerClaimCode ='' or J.CustomerClaimCode='000001')
 and J.SlipNumber is not null and RTRIM(J.SlipNumber) <> ''
 
 update ReceiptPlan Set CustomerClaimCode = H.CustomerCode
 from ReceiptPlan J
inner join ( Select SlipNumber,CustomerCode from CarSalesHeader where DelFlag='0' union all 
 Select SlipNumber,CustomerCode from ServiceSalesHeader where DelFlag='0' ) H
 on J.SlipNumber=H.SlipNumber 
where (J.CustomerClaimCode is null or J.CustomerClaimCode ='' or J.CustomerClaimCode='000001')
 and J.SlipNumber is not null and RTRIM(J.SlipNumber) <> ''
 
END
GO
