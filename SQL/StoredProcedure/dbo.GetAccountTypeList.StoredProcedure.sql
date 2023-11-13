USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetAccountTypeList]    Script Date: 2016/06/10 15:15:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[GetAccountTypeList]

	@ListType varchar(3)--リストタイプ
AS
	SET NOCOUNT ON
	
	/*-------------------------------------------*/
	/* ダーティーリードの設定					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/* 組み合わせ抽出							 */
	/*-------------------------------------------*/

	SELECT *
	FROM [WPH_DB].[dbo].[c_AccountType] ca
	where DelFlag = '0'
	  and exists(select 1 from AccountTypeListCombination al where DelFlag = '0' and al.ListType = @ListType and al.AccountTypeCode = ca.Code)
	ORDER BY 
		DisplayOrder



GO


