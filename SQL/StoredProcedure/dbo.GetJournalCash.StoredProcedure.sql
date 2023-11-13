USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetJournalCash]    Script Date: 2017/09/22 11:49:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






--Mod 2017/09/22 arc yano #3796 �����o�[���o�́@Excel�o�͎��̃\�[�g���ɂ���
--Add 2015/03/16 arc yano �����o�[���G�N�Z����
CREATE PROCEDURE [dbo].[GetJournalCash]
	 @TargetYear int,					--�Ώ۔N��(�N)
	 @TargetMonth int,					--�Ώ۔N��(��)
	 @OfficeCode nvarchar(3),			--��ЃR�[�h
	 @CashAccountCode nvarchar(3)		--���������R�[�h
	 	 
AS	
	BEGIN
		SELECT
		
		-- �����o�[�f�[�^�擾
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


