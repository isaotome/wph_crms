USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetMechanicRanking]    Script Date: 2018/10/16 12:32:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/18 arc yano #3727 サブシステム移行(メカニックランキング) 新規作成
-- Update date
-- 2018/09/12 yano #3935 メカニックランキング　集計結果に含まれない伝票がある
-- Description:	<Description,,>
-- 整備履歴の取得
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetMechanicRanking] 
	  @TargetDateFrom datetime						--対象年月From
AS 
BEGIN

	SET NOCOUNT ON;

	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @TargetDateTo datetime = DATEADD(m, 1, @TargetDateFrom)


	/*-------------------------------------------*/
	/* データ取得								 */
	/*-------------------------------------------*/
	SELECT		
		ROW_NUMBER() OVER 
		( 
			ORDER BY 
				SUM
				(
					(
						CASE WHEN RTRIM(ServiceMenuCode)='DISCNT01' THEN ISNULL(L.TechnicalFeeAmount,0)*-1 ELSE ISNULL(L.TechnicalFeeAmount,0) END
					) -isnull(L.Cost,0)
				) 
			DESC 
		) AS Ranking

		, CASE WHEN D.DepartmentCode ='301' THEN D.DepartmentName ELSE O.OfficeName END AS DepartmentName
	
		, E.EmployeeName AS EmployeeName
	
		, 
		  SUM
		  (
			(
				CASE WHEN RTRIM(ServiceMenuCode)='DISCNT01' THEN ISNULL(L.TechnicalFeeAmount,0) * -1 ELSE ISNULL(L.TechnicalFeeAmount,0) END
			) - ISNULL(L.Cost,0)
		  ) AS TechnicalFeeAmount	
		FROM 
			ServiceSalesHeader H		
			INNER JOIN ServiceSalesLine L ON H.SlipNumber=L.SlipNumber AND H.RevisionNumber=L.RevisionNumber	
			INNER JOIN Employee E ON L.EmployeeCode=E.EmployeeCode
			INNER JOIN Customer C ON H.CustomerCode=C.CustomerCode			--2018/09/12 yano #3935
			--INNER JOIN Customer C ON H.CustomerCode=C.CustomerClaimCode	
			INNER JOIN Department D ON E.DepartmentCode=D.DepartmentCode
				INNER JOIN Office O ON D.OfficeCode=O.OfficeCode
		WHERE 
			H.DelFlag='0' AND 
			H.ServiceOrderStatus in ('005','006') AND 
			L.ServiceType='002' AND
			H.SalesDate >= @TargetDateFrom AND
			H.SalesDate < @TargetDateTo AND
			L.ServiceWorkCode<>'99001'
		GROUP BY 
			  D.DepartmentName
			, D.DepartmentCode
			, E.EmployeeName
			, O.OfficeName		
		ORDER BY 
			SUM(
					(
						Case when Rtrim(ServiceMenuCode)='DISCNT01' THEN ISNULL(L.TechnicalFeeAmount,0) * -1 ELSE ISNULL(L.TechnicalFeeAmount,0) END
					) - ISNULL(L.Cost,0)
				) DESC		

	/*
	SELECT
	
		  CONVERT(bigint, NULL) AS Ranking
		, CONVERT(nvarchar(20), NULL) AS DepartmentName
		, CONVERT(nvarchar(40), NULL) AS EmployeeName
		, CONVERT(decimal(10, 2), NULL) AS TechnicalFeeAmount	
		*/
END



GO