USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetServiceSalesHistoryLine]    Script Date: 2017/03/16 18:50:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- ======================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- 2017/03/13 arc yano #3725 サブシステム移行(整備履歴) 新規作成
-- Description:	<Description,,>
-- 整備履歴の取得
-- ======================================================================================================================================
CREATE PROCEDURE [dbo].[GetServiceSalesHistoryLine] 
	@DivType nvarchar(3) = '018',					--CJ/FA
	@SlipNumber varchar(50) = ''				    --伝票番号
AS 
BEGIN

	SET NOCOUNT ON;

	
	DECLARE @SQL NVARCHAR(MAX) = ''
	DECLARE @PARAM NVARCHAR(1024) = ''
	DECLARE @CRLF NVARCHAR(2) = CHAR(13)+CHAR(10)

	DECLARE @TableNameLine nvarchar(50)			--検索するテーブル名（明細）

	IF @DivType = '019'
		SET @TableNameLine = 'T_CM_ServiceSalesLine'
	ELSE		
		SET @TableNameLine = 'T_FC_ServiceSalesLine'

		/*-------------------------------------------*/
		/* データ挿入								 */
		/*-------------------------------------------*/
		SET @SQL = ''
		SET @SQL = @SQL +'SELECT' + @CRLF
		SET @SQL = @SQL +'	  DepartmentName AS L_DepartmentName' + @CRLF
		SET @SQL = @SQL +'	, SystemName AS L_SystemName' + @CRLF
		SET @SQL = @SQL +'	, SlipNumber AS L_SlipNumber' + @CRLF
		SET @SQL = @SQL +'	, LineNumber AS L_LineNumber' + @CRLF
		SET @SQL = @SQL +'	, SalesInputDate AS L_SalesInputDate' + @CRLF
		SET @SQL = @SQL +'	, RedSlipType AS L_RedSlipType' + @CRLF
		SET @SQL = @SQL +'	, CustomerCode AS L_CustomerCode' + @CRLF
		SET @SQL = @SQL +'	, CustomerNameKana AS L_CustomerNameKana' + @CRLF
		SET @SQL = @SQL +'	, CustomerName1 AS L_CustomerName1' + @CRLF
		SET @SQL = @SQL +'	, CustomerName2 AS L_CustomerName2' + @CRLF
		SET @SQL = @SQL +'	, RegistName AS L_RegistName' + @CRLF
		SET @SQL = @SQL +'	, RegistType AS L_RegistType' + @CRLF
		SET @SQL = @SQL +'	, RegistNumberKana AS L_RegistNumberKana' + @CRLF
		SET @SQL = @SQL +'	, RegistNumber AS L_RegistNumber' + @CRLF
		SET @SQL = @SQL +'	, ServiceWorkCode AS L_ServiceWorkCode' + @CRLF
		SET @SQL = @SQL +'	, ServiceWorkName AS L_ServiceWorkName' + @CRLF
		SET @SQL = @SQL +'	, ContentsName AS L_ContentsName' + @CRLF
		SET @SQL = @SQL +'	, ContentsType AS L_ContentsType' + @CRLF
		SET @SQL = @SQL +'	, FrontEmployeeCode AS L_FrontEmployeeCode' + @CRLF
		SET @SQL = @SQL +'	, FrontEmployeeName AS L_FrontEmployeeName' + @CRLF
		SET @SQL = @SQL +'	, MechanicEmployeeCode AS L_MechanicEmployeeCode' + @CRLF
		SET @SQL = @SQL +'	, MechanicEmployeeName AS L_MechanicEmployeeName' + @CRLF
		SET @SQL = @SQL +'	, MechanicEmployeeCodeDetail AS L_MechanicEmployeeCodeDetail' + @CRLF
		SET @SQL = @SQL +'	, MechanicEmployeeNameDetail AS L_MechanicEmployeeNameDetail' + @CRLF
		SET @SQL = @SQL +'	, Quantity AS L_Quantity' + @CRLF
		SET @SQL = @SQL +'	, TechnicalFeeAmount AS L_TechnicalFeeAmount' + @CRLF
		SET @SQL = @SQL +'	, PartsAmount AS L_PartsAmount' + @CRLF
		SET @SQL = @SQL +'	, VariousAmount AS L_VariousAmount' + @CRLF
		SET @SQL = @SQL +'	, ServiceName AS L_ServiceName' + @CRLF
		SET @SQL = @SQL +'	, TotalTechnicalFeeAmount AS L_TotalTechnicalFeeAmount' + @CRLF
		SET @SQL = @SQL +'	, TotalPartsAmount AS L_TotalPartsAmount' + @CRLF
		SET @SQL = @SQL +'	, TotalVariousAmount AS L_TotalVariousAmount' + @CRLF
		SET @SQL = @SQL +'	, TechnicalFeeTaxAmount AS L_TechnicalFeeTaxAmount' + @CRLF
		SET @SQL = @SQL +'	, VariousTaxAmount AS L_VariousTaxAmount' + @CRLF
		SET @SQL = @SQL +'	, TotalTaxAmount AS L_TotalTaxAmount' + @CRLF
		SET @SQL = @SQL +'	, RunningData AS L_RunningData' + @CRLF
		SET @SQL = @SQL +'FROM' + @CRLF
		SET @SQL = @SQL + @TableNameLine + @CRLF
		SET @SQL = @SQL +'WHERE' + @CRLF
		SET @SQL = @SQL +'    SlipNumber = ''' + @SlipNumber + '''' + @CRLF

		EXECUTE sp_executeSQL @SQL

	/*
	SELECT
	
		  CONVERT(varchar(50), NULL) AS H_DepartmentName
		, CONVERT(varchar(50), NULL) AS H_SystemName
		, CONVERT(varchar(50), NULL) AS H_SlipNumber
		, CONVERT(varchar(50), NULL) AS H_SalesInputDate
		, CONVERT(varchar(50), NULL) AS H_RedSlipType
		, CONVERT(varchar(50), NULL) AS H_CustomerCode
		, CONVERT(varchar(50), NULL) AS H_CustomerNameKana
		, CONVERT(varchar(50), NULL) AS H_CustomerName1
		, CONVERT(varchar(50), NULL) AS H_CustomerName2
		, CONVERT(varchar(50), NULL) AS H_RegistName
		, CONVERT(varchar(50), NULL) AS H_RegistType
		, CONVERT(varchar(50), NULL) AS H_RegistNumberKana
		, CONVERT(varchar(50), NULL) AS H_RegistNumber
		, CONVERT(varchar(50), NULL) AS H_ServiceWorkCode
		, CONVERT(varchar(50), NULL) AS H_ServiceWorkName
		, CONVERT(varchar(50), NULL) AS H_FrontEmployeeCode
		, CONVERT(varchar(50), NULL) AS H_FrontEmployeeName
		, CONVERT(varchar(50), NULL) AS H_MechanicEmployeeCode
		, CONVERT(varchar(50), NULL) AS H_MechanicEmployeeName
		, CONVERT(varchar(50), NULL) AS H_TechnicalFeeAmount
		, CONVERT(varchar(50), NULL) AS H_PartsAmount
		, CONVERT(varchar(50), NULL) AS H_VariousAmount
		, CONVERT(varchar(50), NULL) AS H_TechnicalFeeTaxAmount
		, CONVERT(varchar(50), NULL) AS H_VariousTaxAmount
		, CONVERT(varchar(50), NULL) AS H_TotalTaxAmount
		, CONVERT(varchar(50), NULL) AS H_RunningData
		, CONVERT(varchar(50), NULL) AS H_Vin
		,
		  CONVERT(varchar(50), NULL) AS L_DepartmentName
		, CONVERT(varchar(50), NULL) AS L_SystemName
		, CONVERT(varchar(50), NULL) AS L_SlipNumber
		, CONVERT(varchar(50), NULL) AS L_LineNumber
		, CONVERT(varchar(50), NULL) AS L_SalesInputDate
		, CONVERT(varchar(50), NULL) AS L_RedSlipType
		, CONVERT(varchar(50), NULL) AS L_CustomerCode
		, CONVERT(varchar(50), NULL) AS L_CustomerNameKana
		, CONVERT(varchar(50), NULL) AS L_CustomerName1
		, CONVERT(varchar(50), NULL) AS L_CustomerName2
		, CONVERT(varchar(50), NULL) AS L_RegistName
		, CONVERT(varchar(50), NULL) AS L_RegistType
		, CONVERT(varchar(50), NULL) AS L_RegistNumberKana
		, CONVERT(varchar(50), NULL) AS L_RegistNumber
		, CONVERT(varchar(50), NULL) AS L_ServiceWorkCode
		, CONVERT(varchar(500), NULL) AS L_ServiceWorkName
		, CONVERT(varchar(500), NULL) AS L_ContentsName
		, CONVERT(varchar(500), NULL) AS L_ContentsType
		, CONVERT(varchar(500), NULL) AS L_FrontEmployeeCode
		, CONVERT(varchar(50), NULL) AS L_FrontEmployeeName
		, CONVERT(varchar(50), NULL) AS L_MechanicEmployeeCode
		, CONVERT(varchar(50), NULL) AS L_MechanicEmployeeName
		, CONVERT(varchar(50), NULL) AS L_MechanicEmployeeCodeDetail
		, CONVERT(varchar(50), NULL) AS L_MechanicEmployeeNameDetaL
		, CONVERT(varchar(50), NULL) AS L_Quantity
		, CONVERT(varchar(50), NULL) AS L_TechnicalFeeAmount
		, CONVERT(varchar(50), NULL) AS L_PartsAmount
		, CONVERT(varchar(50), NULL) AS L_VariousAmount
		, CONVERT(varchar(50), NULL) AS L_ServiceName
		, CONVERT(varchar(50), NULL) AS L_TotalTechnicalFeeAmount
		, CONVERT(varchar(50), NULL) AS L_TotalPartsAmount
		, CONVERT(varchar(50), NULL) AS L_TotalVariousAmount
		, CONVERT(varchar(50), NULL) AS L_TechnicalFeeTaxAmount
		, CONVERT(varchar(50), NULL) AS L_VariousTaxAmount
		, CONVERT(varchar(50), NULL) AS L_TotalTaxAmount
		, CONVERT(varchar(50), NULL) AS L_RunnnigData
		*/
END





GO
		--更新履歴テーブルに登録
		INSERT INTO [dbo].[DB_ReleaseHistory]
				([HistoryID]
				,[TicketNumber]
				,[QueryName]
				,[ReleaseDate]
				,[Summary]
				,[ExecEmployeeCode]
				,[ExecDate])
		VALUES
			(NEWID()
			,'3725'--チケット番号
			,'20170313_#3725_サブシステム移行（整備履歴）/09_Create_Procedure_GetServiceSalesHistoryLine.sql'
			,CONVERT(datetime, '2017/03/??', 120)		--※※※※※※※※※※リリース日(WPH様入力)※※※※※※※※※※※
			,''--コメント
			,'arima.yuji'--実行者
			,GETDATE()--実行日
		)




