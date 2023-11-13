USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[W_Hourly_Data_Maintenance]    Script Date: 2019/02/25 13:32:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =================================================================================
-- Author:		Kamachi Akira
-- Create date: ????/??/??
-- Description:	サービス売掛金明細データ作成
-- Edit date : 
--				2019/02/19 yano #3965 WE版新システム対応
-- =================================================================================
CREATE PROCEDURE [dbo].[W_Hourly_Data_Maintenance]
AS
BEGIN

DECLARE @test int = 0

--Del 2019/02/19 yano #3965
---------------------------------
--兼務者への受注承認タスクの移動
---------------------------------
--exec W_Make_ApprovalTasks

---------------------------------
--入金消込フラグのチェック
---------------------------------
--exec W_Make_DepositFLag

--Del 2019/02/19 yano #3965
---------------------------------
--メカニックランキングの生成
---------------------------------
--exec EUCDB.dbo.Make_mekaranking

END


GO


