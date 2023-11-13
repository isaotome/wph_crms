USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_T_Receivable]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[W_T_Receivable]
AS
BEGIN
-- 一時的に作成したストアド
--　本来、締め処理した日付では、再計算しないほうがよい

Declare @CheckMonth as date

--締め処理完了して2日間は、締め処理月の売掛を計算する
set @CheckMonth = (
select 
	Case when DATEDIFF(d,isnull(lastupdatedate,getdate()),getdate()) <=2 then convert(date, CloseMonth, 111) 
	else DATEADD(m,1,convert(date, CloseMonth, 111)) end
from CloseMonthControl
where CloseStatus='003'
	and CloseMonth = (select MAX(CloseMonth) from CloseMonthControl where CloseStatus='003')
)

--売掛計算処理開始
exec W_Make_Receivable @TargetMonth = @CheckMonth
exec W_Make_Receivable_sv @TargetMonth = @CheckMonth
exec W_Make_Receivable_sv_Detail @TargetMonth = @CheckMonth

END
GO
