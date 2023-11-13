USE [WPH_DB]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[W_Make_Receivable_sv_Detail]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[W_Make_Receivable_sv_Detail]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[W_Make_Receivable_sv_Detail]
	@TargetMonth date
AS
BEGIN

-- データの削除
delete [WPH_DB].dbo.ReceivableDetail where DATEDIFF(m,InventoryMonth,@TargetMonth) = 0

-- 入金予定
insert into [WPH_DB].dbo.ReceivableDetail
select
	NEWID(),R1.InventoryMonth,R2.SlipNumber,R2.CustomerClaimCode,R1.DepartmentCode,R1.SlipType,
	R1.SalesDate,sum(R2.Amount),null,null,null,'sys',GETDATE(),'sys',GETDATE(),'0',null
from [WPH_DB].dbo.Receivable R1
	inner join ReceiptPlan R2 on R1.SlipNumber=R2.SlipNumber
where R1.DelFlag='0' and R2.DelFlag='0'
	and DATEDIFF(m,InventoryMonth,@TargetMonth) = 0
Group By R1.InventoryMonth,R2.SlipNumber,R2.CustomerClaimCode,R1.DepartmentCode,R1.SlipType,R1.SalesDate

--予定と実績の合わせ（伝票番号と請求先コードがあっているもの）
update [WPH_DB].dbo.ReceivableDetail
	set MaeAmount=J.MaeAmount,AtoAmount=J.AtoAmount
from [WPH_DB].dbo.ReceivableDetail R
	inner join (
		select 
			SlipNumber,CustomerClaimCode,SUM(MaeAmount) as MaeAmount,SUM(AtoAmount) as AtoAmount
		From (
			select 
				SlipNumber,CustomerClaimCode,JournalDate,
				Case when datediff(m,@TargetMonth,JournalDate) > 0 then isnull(Amount,0) else 0 end AS MaeAmount,
				Case when datediff(m,@TargetMonth,JournalDate) <= 0 then isnull(Amount,0) else 0 end As AtoAmount
			from Journal
			where (DelFlag='0' or AccountType='011') and JournalType='001' and AccountType<>'099'
		) A 
		Group By SlipNumber,CustomerClaimCode
	) J on R.SlipNumber=J.SlipNumber and R.CustomerClaimCode=J.CustomerClaimCode
where DATEDIFF(m,R.InventoryMonth,@TargetMonth) = 0


--予定と実績の合わせ（伝票番号と請求先コードがあっていない）
insert into [WPH_DB].dbo.ReceivableDetail

Select NEWID(),B.* From (
	Select 
		distinct @TargetMonth as InventoryMonth,J.SlipNumber,isnull(J.CustomerClaimCode,'000001') as CustomerClaimCode,R.DepartmentCode,R.SlipType,
		R.SalesDate,0 as H,J.MaeAmount,J.AtoAmount,null as A,'sys' as B,GETDATE() as C,'sys' as D,GETDATE() AS E,'0' AS F,null as I
	from [WPH_DB].dbo.ReceivableDetail R
		right join (
			select 
				SlipNumber,CustomerClaimCode,SUM(MaeAmount) as MaeAmount,SUM(AtoAmount) as AtoAmount
			From (
				select 
					SlipNumber,CustomerClaimCode,JournalDate,
					Case when datediff(m,@TargetMonth,JournalDate) > 0 then isnull(Amount,0) else 0 end AS MaeAmount,
					Case when datediff(m,@TargetMonth,JournalDate) <= 0 then isnull(Amount,0) else 0 end As AtoAmount
				from Journal 
				where (DelFlag='0' or AccountType='011') and JournalType='001' and AccountType<>'099'
			) A 
			Group By SlipNumber,CustomerClaimCode
		) J on R.SlipNumber=J.SlipNumber and R.CustomerClaimCode=J.CustomerClaimCode
	where R.CustomerClaimCode is null and J.SlipNumber is not null
	) B
where DATEDIFF(m,InventoryMonth,@TargetMonth) = 0

-- NULL 補正
update [WPH_DB].dbo.ReceivableDetail Set DepartmentCode=R.DepartmentCode
from [WPH_DB].dbo.Receivable R
	inner join [WPH_DB].dbo.ReceivableDetail RD on R.SlipNumber=RD.SlipNumber
where RD.DepartmentCode is null

update [WPH_DB].dbo.ReceivableDetail Set SalesDate=R.SalesDate
from [WPH_DB].dbo.Receivable R
	inner join [WPH_DB].dbo.ReceivableDetail RD on R.SlipNumber=RD.SlipNumber
where RD.SalesDate is null

update [WPH_DB].dbo.ReceivableDetail Set TotalBalanceAmount=R.BalanceAmount
from [WPH_DB].dbo.Receivable R
	inner join [WPH_DB].dbo.ReceivableDetail RD on R.SlipNumber=RD.SlipNumber
where DATEDIFF(m,R.InventoryMonth,@TargetMonth)=0
and  DATEDIFF(m,RD.InventoryMonth,@TargetMonth)=0


update [WPH_DB].dbo.ReceivableDetail Set SlipType=R.SlipType
from [WPH_DB].dbo.Receivable R
	inner join [WPH_DB].dbo.ReceivableDetail RD on R.SlipNumber=RD.SlipNumber
where RD.SlipType is null

update [WPH_DB].dbo.ReceivableDetail Set MaeAmount = 0 where MaeAmount is null
update [WPH_DB].dbo.ReceivableDetail Set AtoAmount = 0 where AtoAmount is null
update [WPH_DB].dbo.ReceivableDetail Set Amount = 0 where Amount is null
update [WPH_DB].dbo.ReceivableDetail Set BalanceAmount = 0 where BalanceAmount is null

-- 前々期のデータは削除
delete [WPH_DB].dbo.ReceivableDetail
where SalesDate <= '2010/7/1'
delete [WPH_DB].dbo.ReceivableDetail
where SlipType is null

-- 不要なデータは削除する
delete [WPH_DB].dbo.ReceivableDetail
	where datediff(m,InventoryMonth,@TargetMonth)=0
	and DATEDIFF(m,inventorymonth,salesdate) > 0
	and MaeAmount=0 and AtoAmount=0
	and SlipType='1'
	
--納車日が翌期の場合は残高は０
update [WPH_DB].dbo.ReceivableDetail
set BalanceAmount=0,Amount=0,AtoAmount=0
where datediff(m,SalesDate,@TargetMonth) < 0
	and SlipType='1'

-- 調整登録
insert into 
[WPH_DB].dbo.ReceivableDetail
select 
	NEWID(),inventorymonth,R.slipnumber,'000001',
	isnull(DepartmentCode,'999'),
	R.SlipType,
	isnull(SalesDate,DATEADD(m,0,@TargetMonth)),
	Amount ,MaeAmount,AtoAmount,BalanceAmount,
	'sys',GETDATE(),'sys',GETDATE(),'0',null
from [WPH_DB].dbo.Receivableajust R
	left join (
		Select SlipNumber,DepartmentCode,SalesDate From CarSalesHeader where DelFlag='0'
		union all
		Select SlipNumber,DepartmentCode,SalesDate From ServiceSalesHeader where DelFlag='0'
		) H
	on R.SlipNumber=H.SlipNumber
where DATEDIFF(m,inventorymonth,@TargetMonth)=0


-- バランス計算
update [WPH_DB].dbo.ReceivableDetail
	set BalanceAmount = ISNULL(Amount,0)-ISNULL(AtoAmount,0)-ISNULL(MaeAmount,0)
where DATEDIFF(m,InventoryMonth,@TargetMonth) = 0



END
GO


