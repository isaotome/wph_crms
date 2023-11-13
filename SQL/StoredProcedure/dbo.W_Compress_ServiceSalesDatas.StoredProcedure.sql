USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[W_Compress_ServiceSalesDatas]    Script Date: 2019/02/25 13:30:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =================================================================================
-- Author:		Kamachi Akira
-- Create date: 2012/1/27
-- Description:	見積期日が2ヶ月以上前の履歴データは削除する
-- Edit date : 
--				2019/02/19 yano #3965 WE版新システム対応
--				2013/10/30 見積日からではなく最終更新日からに変更
-- =================================================================================
CREATE PROCEDURE [dbo].[W_Compress_ServiceSalesDatas]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    
    -- 確認
	--Add 2019/02/19 yano #3965
	declare @dbName nvarchar(50)
	declare @backupDBName nvarchar(50)
	declare @SQL nvarchar(MAX) = ''
	declare @PARAM nvarchar(1024) = ''
	declare @CRLF nvarchar(2) = char(13)+char(10)

	select @dbName = DB_NAME()

	IF @dbName = 'WPH_DB'
		SET @backupDBName = 'WPH_DB_Backup'
	ELSE
		SET @backupDBName = 'WE_DB_Backup'
	

	-- Key 重複エラー解消する (ServiceSalesLine)
	-- Mod 2019/02/19 yano #3965

	SET @SQL = ''
	SET @SQL = @SQL +'delete ' +  @backupDBName + '.dbo.ServiceSalesLine' + @CRLF
	SET @SQL = @SQL +'from' + @CRLF
	SET @SQL = @SQL +'	  ' + @backupDBName + '.dbo.ServiceSalesLine A' + @CRLF
	SET @SQL = @SQL +'	inner join (' + @CRLF
	SET @SQL = @SQL +'	select L.*' + @CRLF
	SET @SQL = @SQL +'	from dbo.ServiceSalesHeader H inner join dbo.ServiceSalesLine L on H.SlipNumber=L.SlipNumber and H.RevisionNumber=L.RevisionNumber' + @CRLF
	SET @SQL = @SQL +'	where H.DelFlag=''1''' + @CRLF
	SET @SQL = @SQL +'	and datediff(m,H.LastUpdateDate,GETDATE()) > 1' + @CRLF
	SET @SQL = @SQL +'	and datediff(m,H.QuoteExpireDate,GETDATE()) > 1' + @CRLF
	SET @SQL = @SQL +'	and H.RevisionNumber <> 1' + @CRLF
	SET @SQL = @SQL +'	) B on A.SlipNumber=B.SlipNumber and A.RevisionNumber=B.RevisionNumber and A.LineNumber=B.LineNumber' + @CRLF

	EXECUTE sp_executeSQL @SQL

	--delete WPH_DB_Backup.dbo.ServiceSalesLine
	--From WPH_DB_Backup.dbo.ServiceSalesLine A
	--inner join (
	--select L.*
	--from WPH_DB.dbo.ServiceSalesHeader H inner join WPH_DB.dbo.ServiceSalesLine L on H.SlipNumber=L.SlipNumber and H.RevisionNumber=L.RevisionNumber 
	--where H.DelFlag='1'
	--  and datediff(m,H.LastUpdateDate,GETDATE()) > 1
	--  and datediff(m,H.QuoteExpireDate,GETDATE()) > 1
	--  and H.RevisionNumber <> 1
	--) B on A.SlipNumber=B.SlipNumber and A.RevisionNumber=B.RevisionNumber and A.LineNumber=B.LineNumber
	
	-- INSERT (ServiceSalesLine)
	SET @SQL = ''
	SET @SQL = @SQL +'insert into ' + @backupDBName + '.dbo.ServiceSalesLine ' + @CRLF
	SET @SQL = @SQL +'select L.*' + @CRLF
	SET @SQL = @SQL +'	 from dbo.ServiceSalesHeader H'  + @CRLF
	SET @SQL = @SQL +'	inner join dbo.ServiceSalesLine L on H.SlipNumber=L.SlipNumber and H.RevisionNumber=L.RevisionNumber ' + @CRLF
	SET @SQL = @SQL +'	where H.DelFlag=''1''' + @CRLF
	SET @SQL = @SQL +'		and datediff(m,H.LastUpdateDate,GETDATE()) > 1'
	SET @SQL = @SQL +'		and datediff(m,H.QuoteExpireDate,GETDATE()) > 1'
	SET @SQL = @SQL +'		and H.RevisionNumber <> 1'
	
	EXECUTE sp_executeSQL @SQL

	--insert into WPH_DB_Backup.dbo.ServiceSalesLine
	--select L.* 
	--from WPH_DB.dbo.ServiceSalesHeader H 
	--inner join WPH_DB.dbo.ServiceSalesLine L on H.SlipNumber=L.SlipNumber and H.RevisionNumber=L.RevisionNumber 
	--where H.DelFlag='1'
	--  and datediff(m,H.LastUpdateDate,GETDATE()) > 1
	--  and datediff(m,H.QuoteExpireDate,GETDATE()) > 1
	--  and H.RevisionNumber <> 1

	-- Key 重複エラー解消する (ServiceSalesPayment)

	-- Mod 2019/02/19 yano #3965

	SET @SQL = ''
	SET @SQL = @SQL +'delete ' +  @backupDBName + '.dbo.ServiceSalesPayment' + @CRLF
	SET @SQL = @SQL +'from' + @CRLF
	SET @SQL = @SQL +'	  ' + @backupDBName + '.dbo.ServiceSalesPayment A' + @CRLF
	SET @SQL = @SQL +'	inner join (' + @CRLF
	SET @SQL = @SQL +'	select L.*' + @CRLF
	SET @SQL = @SQL +'	from dbo.ServiceSalesHeader H inner join dbo.ServiceSalesPayment L on H.SlipNumber=L.SlipNumber and H.RevisionNumber=L.RevisionNumber ' + @CRLF
	SET @SQL = @SQL +'	where H.DelFlag=''1''' + @CRLF
	SET @SQL = @SQL +'	and datediff(m,H.LastUpdateDate,GETDATE()) > 1' + @CRLF
	SET @SQL = @SQL +'	and datediff(m,H.QuoteExpireDate,GETDATE()) > 1' + @CRLF
	SET @SQL = @SQL +'	and H.RevisionNumber <> 1' + @CRLF
	SET @SQL = @SQL +'	) B on A.SlipNumber=B.SlipNumber and A.RevisionNumber=B.RevisionNumber and A.LineNumber=B.LineNumber' + @CRLF

	EXECUTE sp_executeSQL @SQL

	--delete WPH_DB_Backup.dbo.ServiceSalesPayment
	--From WPH_DB_Backup.dbo.ServiceSalesPayment A
	--inner join (
	--select L.* 
	--from WPH_DB.dbo.ServiceSalesHeader H
	--inner join WPH_DB.dbo.ServiceSalesPayment L on H.SlipNumber=L.SlipNumber and H.RevisionNumber=L.RevisionNumber 
	--where H.DelFlag='1'
	--  and datediff(m,H.LastUpdateDate,GETDATE()) > 1
	--  and datediff(m,H.QuoteExpireDate,GETDATE()) > 1
	--  and H.RevisionNumber <> 1
	--) B on A.SlipNumber=B.SlipNumber and A.RevisionNumber=B.RevisionNumber and A.LineNumber=B.LineNumber

	-- INSERT (ServiceSalesPayment)
	SET @SQL = ''
	SET @SQL = @SQL +'insert into ' + @backupDBName + '.dbo.ServiceSalesPayment ' + @CRLF
	SET @SQL = @SQL +'select L.*' + @CRLF
	SET @SQL = @SQL +'	 from dbo.ServiceSalesHeader H'  + @CRLF
	SET @SQL = @SQL +'	inner join dbo.ServiceSalesPayment L on H.SlipNumber=L.SlipNumber and H.RevisionNumber=L.RevisionNumber ' + @CRLF
	SET @SQL = @SQL +'	where H.DelFlag=''1''' + @CRLF
	SET @SQL = @SQL +'		and datediff(m,H.LastUpdateDate,GETDATE()) > 1'
	SET @SQL = @SQL +'		and datediff(m,H.QuoteExpireDate,GETDATE()) > 1'
	SET @SQL = @SQL +'		and H.RevisionNumber <> 1'
	
	EXECUTE sp_executeSQL @SQL

	--insert into WPH_DB_Backup.dbo.ServiceSalesPayment
	--select L.*
	--from WPH_DB.dbo.ServiceSalesHeader H
	--inner join WPH_DB.dbo.ServiceSalesPayment L on H.SlipNumber=L.SlipNumber and H.RevisionNumber=L.RevisionNumber 
	--where H.DelFlag='1'
	--  and datediff(m,H.LastUpdateDate,GETDATE()) > 1
	--  and datediff(m,H.QuoteExpireDate,GETDATE()) > 1
	--  and H.RevisionNumber <> 1

	-- Key 重複エラー解消する (ServiceSalesHeader)
	SET @SQL = ''
	SET @SQL = @SQL +'delete ' +  @backupDBName + '.dbo.ServiceSalesHeader' + @CRLF
	SET @SQL = @SQL +'from' + @CRLF
	SET @SQL = @SQL +'	  dbo.ServiceSalesHeader A' + @CRLF
	SET @SQL = @SQL +'	inner join ' +  @backupDBName + '.dbo.ServiceSalesHeader B on A.SlipNumber=B.SlipNumber and A.RevisionNumber=B.RevisionNumber' + @CRLF
	SET @SQL = @SQL +'where A.DelFlag=''1''' + @CRLF
	SET @SQL = @SQL +'and datediff(m,A.LastUpdateDate,GETDATE()) > 1' + @CRLF
	SET @SQL = @SQL +'and datediff(m,A.QuoteExpireDate,GETDATE()) > 1' + @CRLF
	SET @SQL = @SQL +'and A.RevisionNumber <> 1' + @CRLF

	EXECUTE sp_executeSQL @SQL

	--delete WPH_DB_Backup.dbo.ServiceSalesHeader
	--From WPH_DB.dbo.ServiceSalesHeader A
	--inner join WPH_DB_Backup.dbo.ServiceSalesHeader B on A.SlipNumber=B.SlipNumber and A.RevisionNumber=B.RevisionNumber
	--where A.DelFlag='1'
	--  and datediff(m,A.LastUpdateDate,GETDATE()) > 1
	--  and datediff(m,A.QuoteExpireDate,GETDATE()) > 1
	--  and A.RevisionNumber <> 1

	-- INSERT (ServiceSalesHeader)
	SET @SQL = ''
	SET @SQL = @SQL +'insert into ' + @backupDBName + '.dbo.ServiceSalesHeader ' + @CRLF
	SET @SQL = @SQL +'select *' + @CRLF
	SET @SQL = @SQL +'	 from dbo.ServiceSalesHeader'  + @CRLF
	SET @SQL = @SQL +'where DelFlag=''1''' + @CRLF
	SET @SQL = @SQL +'and datediff(m,LastUpdateDate,GETDATE()) > 1' + @CRLF
	SET @SQL = @SQL +'and datediff(m,QuoteExpireDate,GETDATE()) > 1' + @CRLF
	SET @SQL = @SQL +'and RevisionNumber <> 1' + @CRLF
	
	EXECUTE sp_executeSQL @SQL

	--insert into WPH_DB_Backup.dbo.ServiceSalesHeader
	--Select *
	--from WPH_DB.dbo.ServiceSalesHeader
	--where DelFlag='1'
	--  and datediff(m,LastUpdateDate,GETDATE()) > 1
	--  and datediff(m,QuoteExpireDate,GETDATE()) > 1
	--  and RevisionNumber <> 1

	--削除
	Delete dbo.ServiceSalesLine
	From dbo.ServiceSalesHeader H inner join dbo.ServiceSalesLine L on H.SlipNumber=L.SlipNumber and H.RevisionNumber=L.RevisionNumber 
	where H.DelFlag='1'
	  and datediff(m,H.LastUpdateDate,GETDATE()) > 1
	  and datediff(m,H.QuoteExpireDate,GETDATE()) > 1
	  and H.RevisionNumber <> 1
	
	Delete dbo.ServiceSalesPayment
	from dbo.ServiceSalesHeader H inner join dbo.ServiceSalesPayment L on H.SlipNumber=L.SlipNumber and H.RevisionNumber=L.RevisionNumber 
	where H.DelFlag='1'
	  and datediff(m,H.LastUpdateDate,GETDATE()) > 1
	  and datediff(m,H.QuoteExpireDate,GETDATE()) > 1
	  and H.RevisionNumber <> 1

	Delete dbo.ServiceSalesHeader 
	where DelFlag='1'
	  and datediff(m,LastUpdateDate,GETDATE()) > 1
	  and datediff(m,QuoteExpireDate,GETDATE()) > 1
	  and RevisionNumber <> 1
		    
	----------------
	--Errorlogの圧縮
	----------------

	--Backup
	SET @SQL = ''
	SET @SQL = @SQL +'insert into ' + @backupDBName + '.dbo.ErrorLog ' + @CRLF
	SET @SQL = @SQL +'select ' + @CRLF
	SET @SQL = @SQL +'CreateEmployeeCode, CreateDate, Uri, Controller, ' + @CRLF
	SET @SQL = @SQL +'Action, Message, KeyData, Stack ' + @CRLF
	SET @SQL = @SQL +'FROM ErrorLog' + @CRLF
	SET @SQL = @SQL +'where datediff(m,CreateDate,GETDATE()) > 3' + @CRLF
	
	EXECUTE sp_executeSQL @SQL

	--insert into WPH_DB_Backup.dbo.ErrorLog
	--SELECT
	--	CreateEmployeeCode, CreateDate, Uri, Controller, 
	--	Action, Message, KeyData, Stack
	--FROM ErrorLog
	--where datediff(m,CreateDate,GETDATE()) > 3

	--Delete
	delete ErrorLog
	where datediff(m,CreateDate,GETDATE()) > 3
    
END


GO


