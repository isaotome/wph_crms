USE [WPH_DB]
GO
/****** Object:  View [dbo].[V_CarQuoteLineReport]    Script Date: 08/04/2014 09:03:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [dbo].[V_CarQuoteLineReport]
as
select 
	a1.SlipNumber,
	a1.RevisionNumber,
	a1.LineNumber,
	a1.CarOptionCode,
	case when a1.CarOptionName='' then a2.CarOptionName else a1.CarOptionName  end as CarOptionName,
	a3.Name as OptionType,
	a1.Amount,
	a1.DelFlag
from CarSalesLine a1 
left join CarOption a2 on a1.CarOptionCode=a2.CarOptionName
left join c_OptionType a3 on a1.OptionType=a3.Code
GO
