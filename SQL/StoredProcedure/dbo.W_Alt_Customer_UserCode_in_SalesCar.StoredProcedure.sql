USE [WPH_DB]
GO
/****** Object:  StoredProcedure [dbo].[W_Alt_Customer_UserCode_in_SalesCar]    Script Date: 08/04/2014 09:03:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[W_Alt_Customer_UserCode_in_SalesCar]
AS
BEGIN

-- 引当処理が完了していたらSalesCarのUsercodeに反映させる

update SalesCar
	set UserCode=H.CustomerCode
from CarSalesHeader H 
	inner join SalesCar S on H.SalesCarNumber = S.SalesCarNumber
where H.DelFlag='0' and SalesOrderStatus in ('002','003','004')
	and H.CustomerCode <> isnull(S.UserCode,'')
--	and (RTrim(S.UserCode)='' or LEFT(LTrim(S.UserCode),2)='00')
	and H.CustomerCode <>'000001'
	and (H.SalesCarNumber is not null or RTRIM(H.SalesCarNumber) <> '')
	
--販売済みの顧客は帰納客にする
--区分が空白は001:未納にセット
update Customer 
	set CustomerKind='001',LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='kamachi.akira'
where RTRIM(CustomerKind)=''

--販売実績のある顧客は帰納
update Customer
	set CustomerKind='002',LastUpdateDate=GETDATE(),LastUpdateEmployeeCode='kamachi.akira'
from CarSalesHeader H
	inner join Customer C On H.CustomerCode=C.CustomerCode
where H.DelFlag='0' and H.SalesOrderStatus='005'
	and C.CustomerKind ='001'
	and DATEDIFF(m,H.salesdate,GETDATE()) < 2
	
 
END
GO
