USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[InsertInventoryStock]    Script Date: 2016/02/16 15:58:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- ==============================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Update date:
-- 2016/02/03   arc yano  #3402 部品在庫確認　引当在庫の算出方法の変更 引当在庫の算出元を引当ロケーションの在庫数→引当済数に変更
-- 2015/04/28   arc yano  部品在庫管理機能 不具合修正 絞込み条件の追加
--						  (有効な部品[削除フラグ='0' && 棚卸対象外フラグ <> '1']、有効なロケーション[削除フラグ='0']のみInventoryStockに入れる)
-- Description:	
--				部品在庫データ退避
-- ==============================================================================================================================================
CREATE PROCEDURE [dbo].[InsertInventoryStock]
	 @InventoryMonth datetime, 
	 @DepartmentCode nvarchar(3),
	 @EmployeeCode nvarchar(50)
	 	 
AS	
	BEGIN
		INSERT INTO dbo.InventoryStock
			SELECT 
				NEWID(),					--棚卸ID
				@DepartmentCode,			--部門コード
				@InventoryMonth,			--棚卸月
				LocationCode,				--ロケーションコード
				@EmployeeCode,				--社員コード
				'002',						--棚卸タイプ
				null,						--管理番号
				PartsNumber,				--部品番号
				ISNULL(Quantity, 0),		--数量
				@EmployeeCode,				--作成者
			    GETDATE(),					--作成日
				@EmployeeCode,				--最終更新者
				GETDATE(),					--最終更新日
				DelFlag,					--削除フラグ
				null,						--サマリ
			    CASE WHEN ISNULL(Quantity, 0) <= 0 THEN 0 ELSE Quantity END AS PhysicalQuantity,	--実棚
				null,						--コメント
				ProvisionQuantity			--引当済数		--Add 2016/02/03   arc yano
			FROM 	
				PartsStock a
			WHERE exists 
				(
					SELECT 'X' FROM Location WHERE DepartmentCode = @DepartmentCode and DelFlag = '0' and LocationCode = a.LocationCode
				)

			and exists 
				(
					SELECT 'Y' FROM Parts WHERE DelFlag = '0' and NonInventoryFlag <> '1' and PartsNumber = a.PartsNumber
				)
	END



GO


