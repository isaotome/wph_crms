USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetJournalCash]    Script Date: 2017/09/22 11:49:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






--Mod 2017/09/22 arc yano #3796 現金出納帳出力　Excel出力時のソート順について
--Add 2015/03/16 arc yano 現金出納帳エクセル化
CREATE PROCEDURE [dbo].[GetJournalCash]
	 @TargetYear int,					--対象年月(年)
	 @TargetMonth int,					--対象年月(月)
	 @OfficeCode nvarchar(3),			--会社コード
	 @CashAccountCode nvarchar(3)		--現金口座コード
	 	 
AS	
	BEGIN
		SELECT
		
		-- 現金出納データ取得
			  @TargetYear AS TargetDateY
			, @TargetMonth AS TargetDateM
			, Day(J.JournalDate) AS TargetDateD
			, J.AccountCode AS AccountCode
			, A.AccountName AS AccountName
			, J.DepartmentCode AS DepartmentCode
			, J.SlipNumber AS SlipNumber
			, C.CustomerClaimName AS CustomerClaimName
			, 'A' AS Blank1
			, 'B' AS Blank2
			, 'C' AS Blank3
			, Summary
			, 'D' AS Blank4
			, 'E' AS Blank5
			--, C.CustomerClaimCode
			, CASE WHEN (J.JournalType='001' and Amount >= 0) or (J.JournalType='002' and Amount < 0) THEN ABS(Amount) ELSE 0 END AS InAmount
			, CASE WHEN (J.JournalType='001' and Amount < 0) or (J.JournalType='002' and Amount >= 0) THEN ABS(Amount) ELSE 0 END AS OutAmount
			--, 'F' AS Blank6
			--, 'G' AS Blank7
			--, 'H' AS Blank8
			--, J.JournalID AS JournalID
			--, J.OfficeCode
			--, CA.CashAccountName
			--, O.OfficeName
		FROM 
			Journal J left join Account A ON J.AccountCode=A.AccountCode
			left outer join CustomerClaim C ON J.CustomerClaimCode = C.CustomerClaimCode
			inner join Office O ON J.OfficeCode=O.OfficeCode
			inner join CashAccount CA ON J.CashAccountCode = CA.CashAccountCode and J.OfficeCode = CA.OfficeCode
		WHERE 
				J.DelFlag='0' 
			and AccountType='001'
			and J.OfficeCode=@OfficeCode
			and J.CashAccountCode = @CashAccountCode
			and YEAR(JournalDate)= @TargetYear 
			and MONTH(JournalDate)= @TargetMonth
		ORDER BY 
			  J.JournalDate
			, J.JournalType		--2017/09/22 arc yano #3796
			, J.AccountCode
			, J.DepartmentCode
			, J.CreateDate
			, J.JournalId
	END




GO


