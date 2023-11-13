USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_Make_Receivable_old]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[W_Make_Receivable_old]
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
delete EUCDB.dbo.Receivable
where DateDiff(m,InventoryMonth,@TargetMonth)=0

--　車両の納車済みデータ
Declare SlipNumbers CURSOR FOR
	select 
		'0',H.SlipNumber,H.DepartmentCode,H.SalesDate,
		sum(R.Amount) as BirthAmount,sum(R.ReceivableBalance) as BalanceAmount
	from CarSalesHeader H
		inner join ReceiptPlan R on H.SlipNumber=R.SlipNumber
	where H.DelFlag='0' and R.DelFlag='0' and SalesOrderStatus='005'
		and datediff(m,H.SalesDate,@TargetMonth) >= 0
	Group By H.SlipNumber,H.DepartmentCode,H.SalesDate
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
into @SlipType,@SlipNumber,@DepartmentCode ,@SalesDate, @BirthAmount, @BalanceAmount

While @@FETCH_STATUS=0
Begin

-- 前受・売掛の判定
	Set @MaeAmount =
		(
		select Sum(Amount) from Journal
		where DelFlag='0' and JournalType='001' and AccountType<>'099'
			and SlipNumber=@SlipNumber and JournalDate <= @TargetMonth
			and datediff(m,JournalDate,@TargetMonth) >= 0
		)
	Set @AtoAmount =
		(
		select Sum(Amount) from Journal
		where DelFlag='0' and JournalType='001' and AccountType<>'099'
			and SlipNumber=@SlipNumber and JournalDate > @TargetMonth
			and datediff(m,JournalDate,@TargetMonth) >= 0
		)
	insert  EUCDB.dbo.Receivable 
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
			'kamachi.akira', GETDATE(), 'kamachi.akira', GETDATE(), '0'
		)
	
	Fetch Next From SlipNumbers
into @SlipType,@SlipNumber,@DepartmentCode ,@SalesDate, @BirthAmount, @BalanceAmount
	
End

Close SlipNumbers
DeAllocate SlipNumbers

-- 前々期のデータは削除
delete EUCDB.dbo.Receivable
where SalesDate <= '2010/7/1'


END
GO
