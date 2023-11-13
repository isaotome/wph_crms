USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[GetCarOptionSetList]    Script Date: 2016/03/07 16:33:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- 2016/02/09 arc nakayama 車両伝票作成時のオプションのデフォルト設定 新規作成
-- 2016/03/07 arc nakayama 車両伝票作成時のオプションのデフォルト設定 車種コードからグレードコードに変更

CREATE PROCEDURE [dbo].[GetCarOptionSetList] 
	@CarGradeCode nvarchar(30)	--車種コード
	,@MakerCode nvarchar(5)	--メーカーコード
AS
BEGIN
	SET NOCOUNT ON;
	
	/*-------------------------------------------*/
	/* ダーティーリードの設定					 */
	/*-------------------------------------------*/
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	/*-------------------------------------------*/
	/* 該当車種に関する必須オプション取得		 */
	/*-------------------------------------------*/

	SELECT [CarOptionCode]
		  ,[CarOptionName]
		  ,[MakerCode]
		  ,[Cost]
		  ,[SalesPrice]
		  ,[OptionType]
		  ,[CarGradeCode]
	FROM [WPH_DB].[dbo].[CarOption]
	WHERE DelFlag = '0'
	  AND CarGradeCode = @CarGradeCode
	  AND MakerCode = @MakerCode
	  AND RequiredFlag = '1'
END


GO


