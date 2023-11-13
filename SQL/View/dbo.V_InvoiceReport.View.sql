USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_InvoiceReport]    Script Date: 2019/06/04 15:22:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- -------------------------------------------------------------------------------------------------------------------
-- 機能：請求書
-- 作成日：???
-- 更新日：
--		  2019/05/22 yano #3988 【サービス受付】承り書（車両修正・受付票）出力時に顧客住所が表示されない　類似対応
-- ------------------------------------------------------------------------------------------------------------------
CREATE view [dbo].[V_InvoiceReport]
as
select
	a1.ReceiptPlanId,
	a5.PostCode as DepartmentPostCode,
	ISNULL(a5.Prefecture, '') + ISNULL(a5.City, '') as DepartmentPrefecture,			--Mod 2019/05/22 yano #3988
	ISNULL(a5.Address1, '') + ISNULL(a5.Address2, '') as DepartmentAddress1,			--Mod 2019/05/22 yano #3988
	a7.CompanyName + a5.DepartmentName as DepartmentName,
	a5.TelNumber1 as DepartmentTelNumber,
	a5.FaxNumber as DepartmentFaxNumber,
	a4.CustomerClaimName as CustomerName,
	a4.PostCode as CustomerPostCode,
	ISNULL(a4.Prefecture, '') + ISNULL(a4.City, '') as CustomerPrefecture,				--Mod 2019/05/22 yano #3988
	ISNULL(a4.Address1, '') + ISNULL(a4.Address2, '') as CustomerAddress1,				--Mod 2019/05/22 yano #3988
	a4.CustomerClaimCode,
	a1.SlipNumber,
	isnull(a3.SalesDate,isnull(a2.SalesDate,null)) as SalesDate,
	--a1.Amount,
	a1.ReceivableBalance as Amount,
	case when a2.SlipNumber is null then 
		case when a3.SlipNumber is null then ''
		else '車両代'
		end
	else '整備代'
	end as LineContents,
	a5.BankName + ' ' + a5.BranchName + ' ' + a8.Name + ' ' + a5.AccountNumber as BankName,
	a5.AccountHolder
from
	ReceiptPlan a1
left join ServiceSalesHeader a2 on a1.SlipNumber=a2.SlipNumber and a2.DelFlag<>'1'
left join CarSalesHeader a3 on a1.SlipNumber=a3.SlipNumber and a3.DelFlag<>'1'
left join CustomerClaim a4 on a1.CustomerClaimCode=a4.CustomerClaimCode
left join Department a5 on a1.OccurredDepartmentCode = a5.DepartmentCode
left join Office a6 on a5.OfficeCode = a6.OfficeCode
left join Company a7 on a6.CompanyCode = a7.CompanyCode
left join c_DepositKind a8 on a5.DepositKind = a8.Code



GO


