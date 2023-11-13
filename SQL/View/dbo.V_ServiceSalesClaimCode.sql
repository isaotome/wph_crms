USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_ServiceSalesClaimCode]    Script Date: 2023/08/12 9:55:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


--2023/07/26 openwave #4141ÅyÉTÅ[ÉrÉXì`ï[ì¸óÕÅzêøãÅñæç◊èëä÷òAÇÃâ¸èC
CREATE VIEW [dbo].[V_ServiceSalesClaimCode]
AS
	SELECT	SlipNumber,
			RevisionNumber,

			-- Mod 2023/08/01 yano #4141
			CASE WHEN 
				(CustomerClaimCode is null or CustomerClaimCode = '')
			THEN
				CustomerCode
			ELSE
				CustomerClaimCode
			END AS	CustomerClaimCode
			--IsNull(CustomerClaimCode,CustomerCode)	AS	CustomerClaimCode
		FROM	ServiceSalesHeader
		WHERE	DelFlag		=	'0'
		AND	   (ISNULL(CarTax,0)	<>	0	or	ISNULL(CarLiabilityInsurance,0)	<>	0	or	ISNULL(CarWeightTax,0)	<>	0
		or		ISNULL(NumberPlateCost,0)	<>	0	or	ISNULL(FiscalStampCost,0)	<>	0	or	ISNULL(OptionalInsurance,0)	<>	0
		or		ISNULL(TaxFreeFieldValue,0)	<>	0	or	ISNULL(SubscriptionFee,0)	<>	0	or	ISNULL(TaxableFreeFieldValue,0)	<>	0)
	UNION
	SELECT	hed.SlipNumber,
			hed.RevisionNumber,
			-- Mod 2023/08/01 yano #4141
			CASE WHEN 
				(lin.CustomerClaimCode is null or lin.CustomerClaimCode = '')
			THEN
				hed.CustomerCode
			ELSE
				lin.CustomerClaimCode
			END
			--IsNull(lin.CustomerClaimCode,hed.CustomerCode)
		FROM	ServiceSalesHeader	hed
		INNER	JOIN	ServiceSalesLine	lin
		ON		lin.SlipNumber		=	hed.SlipNumber
		AND		lin.RevisionNumber	=	hed.RevisionNumber
		WHERE	hed.DelFlag		=	'0'
;

GO


