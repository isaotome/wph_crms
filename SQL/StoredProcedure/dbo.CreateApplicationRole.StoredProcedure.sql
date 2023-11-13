USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[CreateApplicationRole]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateApplicationRole] AS
BEGIN

begin transaction
delete from ApplicationRole

declare @securityrolecode nvarchar(50)
declare @applicationcode nvarchar(50)

declare sec cursor for
	select securityrolecode from securityrole

declare app cursor for
	select applicationcode from dbo.[application]
	
set nocount on

open sec
fetch next from sec
into @securityrolecode

while @@FETCH_STATUS = 0
begin
	open app
	fetch next from app
	into @applicationcode
	
	while @@FETCH_STATUS = 0
	begin
		insert into ApplicationRole
		values(@securityrolecode,@applicationcode,'false','tryumura',SYSDATETIME(),'tryumura',SYSDATETIME(),'0')

		fetch next from app
		into @applicationcode

	end	
	close app

	fetch next from sec
	into @securityrolecode
end

deallocate app
close sec
deallocate sec

commit
end
GO
