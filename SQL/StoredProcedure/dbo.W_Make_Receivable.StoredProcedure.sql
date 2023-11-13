USE [WPH_DB]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[W_Make_Receivable]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[W_Make_Receivable]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[W_Make_Receivable]
	@TargetMonth date
AS
BEGIN

Declare @SlipType char(1)
Declare @SlipNumber nvarchar(50)
Declare @DepartmentCode nvarchar(3)
Declare @SalesDate date
Declare @BirthAmount decimal(18,0)
Declare @BalanceAmount decimal(18,0)
Declare @MaeAmount Decimal(18,0)
Declare @AtoAmount Decimal(18,0)

-- Tableの初期化
delete [WPH_DB].dbo.Receivable
where DateDiff(m,InventoryMonth,@TargetMonth)=0
	and SlipType='0'

--　車両の納車済みデータ
Declare SlipNumbers CURSOR FOR

	Select 
		A.SlipType,A.SlipNumber,A.DepartmentCode,A.SalesDate,
		A.BirthAmount,A.BalanceAmount,isnull(B.MaeAmount,0) as MaeAmount,isnull(B.AtoAmount,0) as AtoAmount
	From 
	
	(
	select 
		'0' as SlipType,H.SlipNumber,H.DepartmentCode,H.SalesDate,
		sum(R.Amount) as BirthAmount,sum(R.ReceivableBalance) as BalanceAmount
	from CarSalesHeader H
		inner join ReceiptPlan R on H.SlipNumber=R.SlipNumber
	where H.DelFlag='0' and R.DelFlag='0' and SalesOrderStatus='005'
	Group By H.SlipNumber,H.DepartmentCode,H.SalesDate
	) A
	left join 
	(
	Select 
		A.SlipNumber,SUM(A.MaeAmount) as MaeAmount,SUM(A.atoamount) as AtoAmount
	From
		(
		select 
			H.SlipNumber,SalesDate,JournalDate,
			Case when datediff(m,@TargetMonth,salesdate) > 0 then isnull(j.Amount,0) else 0 end AS MaeAmount,
			Case when datediff(m,@TargetMonth,salesdate) <= 0 then isnull(j.Amount,0) else 0 end As AtoAmount
		from CarSalesHeader H
			inner join Journal J on H.SlipNumber=J.SlipNumber
		where H.DelFlag='0' and H.SalesOrderStatus='005'
			and J.DelFlag='0' and J.JournalType='001' and J.AccountType<>'099'
			and DATEDIFF(m,J.JournalDate,@targetmonth) >= 0
			and not(DATEDIFF(m,J.JournalDate,@targetmonth) < 0 and DATEDIFF(m,H.SalesDate,@targetmonth) < 0)
		) A
	Group By SlipNumber
	) B on A.SlipNumber=B.SlipNumber
	
	
	
	/* まずは車両だけ
	union all
	select 
		'1',H.SlipNumber,H.DepartmentCode,H.SalesDate,
		sum(R.Amount) as BirthAmount,sum(R.ReceivableBalance) as BalanceAmount
	from ServiceSalesHeader H
		inner join ReceiptPlan R on H.SlipNumber=R.SlipNumber
	where H.DelFlag='0' and R.DelFlag='0' and ServiceOrderStatus='006'
		and datediff(m,H.SalesDate,@TargetMonth) >= 0
	Group By H.SlipNumber,H.DepartmentCode,H.SalesDate
	*/
Open SlipNumbers
Fetch Next From SlipNumbers
into @SlipType,@SlipNumber,@DepartmentCode ,@SalesDate, @BirthAmount, @BalanceAmount,@MaeAmount,@AtoAmount

While @@FETCH_STATUS=0
Begin

-- 前受・売掛の判定

	insert  [WPH_DB].dbo.Receivable 
		(
			ReceivableCode, InventoryMonth, 
			SlipNumber, DepartmentCode, SlipType, SalesDate, 
			Amount, MaeAmount, AtoAmount, BalanceAmount, 
			CreateEmployeeCode, CreateDate, LastUpdateEmployeeCode, LastUpdateDate, DelFlag
		)
	values 
		(
			NEWID(), @TargetMonth, 
			@SlipNumber, @DepartmentCode, @SlipType, @SalesDate, 
			isnull(@BirthAmount,0), isnull(@MaeAmount,0), isnull(@AtoAmount,0), isnull(@BirthAmount,0)-isnull(@MaeAmount,0)-isnull(@AtoAmount,0), 
			'sys', GETDATE(), 'sys', GETDATE(), '0'
		)
	
	Fetch Next From SlipNumbers
into @SlipType,@SlipNumber,@DepartmentCode ,@SalesDate, @BirthAmount, @BalanceAmount,@MaeAmount,@AtoAmount
	
End

Close SlipNumbers
DeAllocate SlipNumbers

-- 前々期のデータは削除
delete [WPH_DB].dbo.Receivable
where SalesDate <= '2010/7/1'


-- 不要なデータは削除する
delete [WPH_DB].dbo.Receivable
	where datediff(m,InventoryMonth,@TargetMonth)=0
	and DATEDIFF(m,inventorymonth,salesdate) > 0
	and MaeAmount=0 and AtoAmount=0
	and SlipType='0'

--残高調整
update [WPH_DB].dbo.Receivable
	set BalanceAmount = ISNULL(Amount,0)-ISNULL(AtoAmount,0)-ISNULL(MaeAmount,0)

where datediff(m,InventoryMonth,@TargetMonth)=0
	and SlipType='0'
--納車日が翌期の場合は残高は０
update [WPH_DB].dbo.Receivable
set BalanceAmount=0,Amount=0
where datediff(m,SalesDate,@TargetMonth) < 0
	and SlipType='0'


--調整入力(下取入庫など）
insert into 
[WPH_DB].dbo.Receivable
select 
	NEWID(),inventorymonth,R.slipnumber,
	isnull(DepartmentCode,'999'),
	'0',
	isnull(SalesDate,DATEADD(m,1,@TargetMonth)),
	Amount ,MaeAmount,AtoAmount,BalanceAmount,
	'sys',GETDATE(),'sys',GETDATE(),'0'
from [WPH_DB].dbo.Receivableajust R
	left join (Select SlipNumber,DepartmentCode,SalesDate From CarSalesHeader where DelFlag='0') H
	on R.SlipNumber=H.SlipNumber
where DATEDIFF(m,inventorymonth,@TargetMonth)=0
	and R.Sliptype='0'
END



GO


