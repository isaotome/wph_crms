USE [WPH_DB]
GO

/****** Object:  View [dbo].[V_CarSalesJournalReport]    Script Date: 2023/10/17 16:09:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- --------------------------------------------------------------------------------
-- 機能：車両売上
-- 作成日：???
-- 更新日：
--		2023/09/28 yano #4183 インボイス対応(経理対応)
--		2023/09/18 yano #4181【車両伝票】オプション区分追加（サーチャージ）
--		2023/01/11 yano #4158 【車両伝票入力】任意保険料入力項目の追加
--		2021/06/08 yano #4091【車両伝票】オプション行の区分追加  
--		2019/02/25 yano #3965 WE版新システム対応（Web.configによる処理の分岐)
-- --------------------------------------------------------------------------------
CREATE View [dbo].[V_CarSalesJournalReport] As
Select 
    SalesDate,
    NewUsedType,
    DepartmentCode,
    BrandStoreCode,
    JournalCode,
    Case when JounalOrder='1' then '*' else ' ' end as Mark,
    right(YEAR(salesdate)*10000+Month(Salesdate)*100+DAY(SalesDate),6) as SalesDate2,
    Case when (Dr_DivType='0' OR RTRIM(DR_AccountCode) = '5810' OR RTRIM(DR_AccountCode) = '3410') then '0' else
	--Mod 2019/01/12 yano #3965
	Case when Dr_DivType='1' then DepartmentCodeForJournalExport else
    Case when Dr_DivType='2' then LEFT(DepartmentCodeForJournalExport,1)+'01' Else '' end end end as DrDepartmentCode, 
    RTRIM(DR_AccountCode) as DrAccountCode,
    RTRIM(Dr_AccountSub) as DrAccountSub,
    Case when JounalOrder IN (1,3,8,9,14) then DrAmount + DrTaxAmount else null end as DrAmount,										--Mod 2023/09/28 yano #4183
    Case when JounalOrder IN (1,3,8,9,14) then Case when Dr_TaxType='0' then 0 else DrTaxAmount end else null end as DrTaxAmount,		--Mod 2023/09/28 yano #4183
    Case when JounalOrder IN (1,3,8,9,14) then '1' else '' end Flag1,																	--Mod 2023/09/28 yano #4183
    RTRIM(Dr_TaxType) as DrTaxType,
    Case when Cr_DivType='0' then '0' else
	--Mod 2019/01/12 yano #3965
	Case when Cr_DivType='1' then DepartmentCodeForJournalExport else
	Case when Cr_DivType='2' then LEFT(DepartmentCodeForJournalExport,1)+'01' Else '' end end end as CrDepartmentCode, 
    RTRIM(CR_AccountCode) as CrAccountCode,
	RTRIM(Cr_AccountSub) as CrAccountSub, 
    Case when JounalOrder not IN (3,9) then CrAmount + CrTaxAmount else null end as CrAmount,
    Case when JounalOrder not IN (3,9) then Case when Cr_TaxType='0' then 0 else CrTaxAmount end else null end as CrTaxAmount,
    Case when JounalOrder not IN (3,9) then '1' else '' end as Flag2,
    RTRIM(Cr_TaxType) as CrTaxType,
    Case when JounalOrder = '8' then
        SlipNumber+' '+left(replace(replace(replace(replace(replace(replace(CustomerName,'<MFY>',''),' ',''),'株式会社','㈱'),'（株）','㈱'),'有限会社','㈲'),'（有）','㈲'),26)+' 下取' 
    else
    Case when JounalOrder = '9' then
        SlipNumber+' '+left(replace(replace(replace(replace(replace(replace(CustomerName,'<MFY>',''),' ',''),'株式会社','㈱'),'（株）','㈱'),'有限会社','㈲'),'（有）','㈲'),26)+' 下取ﾘｻ'
    else
        SlipNumber+' '+left(replace(replace(replace(replace(replace(replace(CustomerName,'<MFY>',''),' ',''),'株式会社','㈱'),'（株）','㈱'),'有限会社','㈲'),'（有）','㈲'),30)
    end end as Description
from 
(
select
    SlipNumber,
    C.CustomerName,
    H.salescarnumber,
    H.SalesDate,
    H.DepartmentCode,
	CD.DepartmentCodeForJournalExport,	--Add 2019/01/12 yano #3965	
    D.BrandStoreCode,
    H.NewUsedType,
    A.JournalCode,
    A.JounalOrder,
    A.Dr_AccountCode,
    A.Dr_Accountsub,
    A.Dr_taxType,
    A.Dr_DivType,
    A.Cr_AccountCode,
    A.Cr_Accountsub,
    A.Cr_taxType,
    A.Cr_DivType,
    Case when A.JounalOrder=1 then 
        isnull(H.SalesPrice,0) + isnull(H.ShopOptionAmount,0) + isnull(H.MakerOptionAmount,0) + isnull(H.SalesCostTotalAmount,0) +
        isnull(H.CarLiabilityInsurance,0) + isnull(H.RecycleDeposit,0) + isnull(H.TaxFreeTotalAmount,0) + isnull(H.OtherCostTotalAmount,0)
        - isnull(H.CarLiabilityInsurance,0) - isnull(H.RecycleDeposit,0) - isnull(H.DiscountAmount,0)
    else
        Case when A.JounalOrder=3 then isnull(H.DiscountAmount,0) else
        Case when A.JounalOrder=8 then isnull(H.TradeInTotalAmount,0) - isnull(H.TradeInTaxTotalAmount,0) - isnull(H.TradeInRecycleTotalAmount,0) else
        Case when A.JounalOrder=9 then isnull(H.TradeInRecycleTotalAmount,0) else
		Case when A.JounalOrder=14 then isnull(H.SuspendTaxRecv,0) else null	--Add 2023/09/28 yano #4183

    end end end end end AS DrAmount,
    
    Case when A.JounalOrder=1 then
        isnull(H.SalesTax,0)+isnull(H.ShopOptionTaxAmount,0)+isnull(H.MakerOptionTaxAmount,0)+isnull(H.SalesCostTotalTaxAmount,0)-isnull(H.DiscountTax,0) else
    Case when A.JounalOrder=3 then isnull(H.DiscountTax,0) else
    Case when A.JounalOrder=8 then isnull(H.TradeInTaxTotalAmount,0) else 
    Case when A.JounalOrder=9 then 0 else 
	Case when A.JounalOrder=14 then 0 else null--Add 2023/09/28 yano #4183 else null
    end end end end end AS DrTaxAmount,
        
	--Mod 2021/06/08 yano #4091
	Case 
		when A.JounalOrder=1 then isnull(H.SalesPrice,0)
		when A.JounalOrder=2 then isnull(H.ShopOptionAmount,0)+isnull(H.MakerOptionAmount,0) - isnull(H.MaintenancePackageAmount, 0) - isnull(H.ExtendedWarrantyAmount, 0) - isnull(H.SurchargeAmount, 0)	--Add 2023/09/18 yano #4181
		when A.JounalOrder=4 then isnull(H.SalesCostTotalAmount,0)
		when A.JounalOrder=5 then isnull(H.CarLiabilityInsurance,0)
		when A.JounalOrder=6 then isnull(H.RecycleDeposit,0)
		when A.JounalOrder=7 then isnull(H.TaxFreeTotalAmount,0)+isnull(H.OtherCostTotalAmount,0)-isnull(H.CarLiabilityInsurance,0)-isnull(H.RecycleDeposit,0)-isnull(H.VoluntaryInsuranceAmount,0)		--Mod 2023/01/11 yano #4158
		--when A.JounalOrder=7 then isnull(H.TaxFreeTotalAmount,0)+isnull(H.OtherCostTotalAmount,0)-isnull(H.CarLiabilityInsurance,0)-isnull(H.RecycleDeposit,0)
		when A.JounalOrder=8 then isnull(H.TradeInTotalAmount,0) 
		when A.JounalOrder=10 then isnull(H.MaintenancePackageAmount,0)
		when A.JounalOrder=11 then isnull(H.ExtendedWarrantyAmount,0)
		when A.JounalOrder=12 then isnull(H.VoluntaryInsuranceAmount,0)		--Add 2023/01/11 yano #4158
		when A.JounalOrder=13 then isnull(H.SurchargeAmount,0)				--Add 2023/09/18 yano #4181
		when A.JounalOrder=14 then isnull(H.SuspendTaxRecv,0)				--Add 2023/09/28 yano #4183
		else null
	end AS CrAmount,

	--Del 2023/01/11 yano
	--Mod 2021/06/08 yano #4091
	Case 
		when A.JounalOrder=1 then isnull(H.SalesTax,0)
		when A.JounalOrder=2 then isnull(H.ShopOptionTaxAmount,0)+isnull(H.MakerOptionTaxAmount,0)  - isnull(H.MaintenancePackageTaxAmount, 0) - isnull(H.ExtendedWarrantyTaxAmount, 0) - isnull(H.SurchargeTaxAmount, 0)	--Add 2023/09/18 yano #4181
		when A.JounalOrder=4 then isnull(H.SalesCostTotalTaxAmount,0)
		when A.JounalOrder=5 then 0
		when A.JounalOrder=6 then 0
		when A.JounalOrder=7 then 0
		when A.JounalOrder=8 then 0 
		when A.JounalOrder=10 then  isnull(H.MaintenancePackageTaxAmount,0) 					
		when A.JounalOrder=11 then  isnull(H.ExtendedWarrantyTaxAmount,0)
		when A.JounalOrder=12 then 0		--Add 2023/01/11 yano #4158
		when A.JounalOrder=13 then isnull(H.SurchargeTaxAmount,0)			--Add 2023/09/18 yano #4181
		when A.JounalOrder=14 then 0										---Add 2023/09/28 yano #4183
		else null
	end AS CrTaxAmount,

	--Del 2023/01/11 yano
	--Mod 2021/06/08 yano #4091
	Case
		when A.JounalOrder=1 and isnull(H.SalesPrice,0)=0 then 0
		when A.JounalOrder=2 and (isnull(H.ShopOptionAmount,0)+isnull(H.MakerOptionAmount,0) - isnull(H.MaintenancePackageAmount, 0) - isnull(H.ExtendedWarrantyAmount, 0) - isnull(H.SurchargeAmount, 0))=0 then 0 --Add 2023/09/18 yano #4181
		when A.JounalOrder=3 and isnull(H.DiscountAmount,0)=0 then 0
		when A.JounalOrder=4 and isnull(H.SalesCostTotalAmount,0)=0 then 0 
		when A.JounalOrder=5 and isnull(H.CarLiabilityInsurance,0)=0 then 0
		when A.JounalOrder=6 and isnull(H.RecycleDeposit,0)=0 then 0
		when A.JounalOrder=7 and isnull(H.TaxFreeTotalAmount,0)+isnull(H.OtherCostTotalAmount,0)-isnull(H.CarLiabilityInsurance,0)-isnull(H.RecycleDeposit,0)-isnull(H.VoluntaryInsuranceAmount,0)=0 then 0			--Mod 2023/01/11 yano #4158
		--when A.JounalOrder=7 and isnull(H.TaxFreeTotalAmount,0)+isnull(H.OtherCostTotalAmount,0)-isnull(H.CarLiabilityInsurance,0)-isnull(H.RecycleDeposit,0)=0 then 0
		when A.JounalOrder=8 and isnull(H.TradeInTotalAmount,0)=0 then 0
		when A.JounalOrder=9 and isnull(H.TradeInRecycleTotalAmount,0)=0 and isnull(H.TradeInTotalAmount,0)=0 then 0
		when A.JounalOrder=10  and isnull(H.MaintenancePackageAmount,0)=0 then 0
		when A.JounalOrder=11  and isnull(H.ExtendedWarrantyAmount,0)=0 then 0
		when A.JounalOrder=12 and isnull(H.VoluntaryInsuranceAmount,0)= 0 then 0		--Add 2023/01/11 yano #4158
		when A.JounalOrder=13 and isnull(H.SurchargeAmount,0)=0 then 0					--Add 2023/09/18 yano #4181
		when A.JounalOrder=14 and isnull(H.SuspendTaxRecv,0)=0 then 0					--Add 2023/09/28 yano #4183
		else 1
	end AS Outputtype

	--Del 2023/01/11 yano

from AccountCode A 
    CROSS JOIN CarSalesHeader H
    inner join Customer C on H.CustomerCode=C.CustomerCode
    left join Department D on H.DepartmentCode=D.DepartmentCode
	left join ConvDepartmentCodeForJournalExport CD on H.DepartmentCode = CD.DepartmentCode		--Add 2019/01/12 yano #3965
where H.DelFlag='0'
    and SalesOrderStatus='005'
    and C.CustomerType <>'201'
) tbl
where Outputtype = 1

GO


