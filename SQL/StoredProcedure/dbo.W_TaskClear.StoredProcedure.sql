USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_TaskClear]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[W_TaskClear]
AS
BEGIN

--7日以上前のタスクは消す。ただし「101:車両発注承認」は消さない
update task 
Set		TaskCompleteDate=GETDATE(),
		ActionResult='(AUTO)SYSTEM',
		LastUpdateDate=GETDATE(),
		LastUpdateEmployeeCode='kamachi.akira'
where 
		TaskCompleteDate is null 
		and DelFlag='0' 
		and TaskConfigId not in ('101')
		and DateDiff(d,CreateDate,GETDATE()) > 7
--60日以上のタスクは無条件に消す
update task 
Set		TaskCompleteDate=GETDATE(),
		ActionResult='(AUTO)SYSTEM',
		LastUpdateDate=GETDATE(),
		LastUpdateEmployeeCode='kamachi.akira'
where 
		TaskCompleteDate is null 
		and DelFlag='0' 
		and DateDiff(d,CreateDate,GETDATE()) > 60

END
GO
