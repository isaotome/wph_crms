USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[CustomerIntegrate]    Script Date: 2017/03/29 13:14:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





--2017/03/18 arc nakayama #3722_名寄せツール　新規作成

CREATE PROCEDURE [dbo].[CustomerIntegrate]
	@CustomerCode1 nvarchar(10),	--残したい顧客コード
	@CustomerCode2 nvarchar(10),	--消したい顧客コード
	@EmployeeCode nvarchar(50)		--更新者
AS
	SET NOCOUNT ON

	Update Customer Set DelFlag='1' where CustomerCode=@CustomerCode2

	Update CarSalesHeader Set CustomerCode=@CustomerCode1 where CustomerCode=@CustomerCode2

	Update ServiceSalesHeader Set CustomerCode=@CustomerCode1 where CustomerCode=@CustomerCode2

	Update CustomerReceiption Set CustomerCode=@CustomerCode1 where CustomerCode=@CustomerCode2

	Update CustomerUpdateLog Set CustomerCode=@CustomerCode1 where CustomerCode=@CustomerCode2

	Update CustomerClaim Set DelFlag='1' where CustomerClaimCode=@CustomerCode2

	Update CustomerClaimable Set CustomerClaimCode=@CustomerCode1 where CustomerClaimCode=@CustomerCode2

	Update Journal Set CustomerClaimCode=@CustomerCode1 where CustomerClaimCode=@CustomerCode2

	Update Loan Set CustomerClaimCode=@CustomerCode1 where CustomerClaimCode=@CustomerCode2

	Update ReceiptPlan Set CustomerClaimCode=@CustomerCode1 where CustomerClaimCode=@CustomerCode2

	Update ServiceSalesLine Set CustomerClaimCode=@CustomerCode1 where CustomerClaimCode=@CustomerCode2

	Update ServiceSalesPayment Set CustomerClaimCode=@CustomerCode1 where CustomerClaimCode=@CustomerCode2

	Update CarSalesPayment Set CustomerClaimCode=@CustomerCode1 where CustomerClaimCode=@CustomerCode2

	Update Supplier Set DelFlag='1' where SupplierCode=@CustomerCode2

	Update CarPurchase Set SupplierCode=@CustomerCode1 where SupplierCode=@CustomerCode2

	Update CarPurchaseOrder Set SupplierCode=@CustomerCode1 where SupplierCode=@CustomerCode2

	Update PartsPurchase Set SupplierCode=@CustomerCode1 where SupplierCode=@CustomerCode2

	Update PartsPurchaseOrder Set SupplierCode=@CustomerCode1 where SupplierCode=@CustomerCode2

	Update ServiceSalesLine Set SupplierCode=@CustomerCode1 where SupplierCode=@CustomerCode2

	Update SupplierPayment Set DelFlag='1' where SupplierPaymentCode=@CustomerCode2

	Update CarPurchaseOrder Set SupplierPaymentCode=@CustomerCode1 where SupplierPaymentCode=@CustomerCode2

	Update PartsPurchaseOrder Set SupplierPaymentCode=@CustomerCode1 where SupplierPaymentCode=@CustomerCode2

	Update PaymentPlan Set SupplierPaymentCode=@CustomerCode1 where SupplierPaymentCode=@CustomerCode2

	Update CarAppraisal Set UserCode=@CustomerCode1 where UserCode=@CustomerCode2

	Update CarSalesHeader Set UserCode=@CustomerCode1 where UserCode=@CustomerCode2

	Update SalesCar Set UserCode=@CustomerCode1 where UserCode=@CustomerCode2

	Update CarAppraisal Set OwnerCode=@CustomerCode1 where OwnerCode=@CustomerCode2

	Update SalesCar Set OwnerCode=@CustomerCode1 where OwnerCode=@CustomerCode2

	Insert Into W_CustomerIntegradeHistory values (NewID(),@CustomerCode1,@CustomerCode2, @EmployeeCode,getdate())

	
	--userとOwnerの住所・名前も修正
	update SalesCar
		Set UserName = left(C.CustomerName,80),
		UserAddress = left(isnull(RTRIM(C.Prefecture),'')+isnull(RTRIM(C.City),'')+isnull(RTRIM(C.Address1),'')+isnull(RTRIM(C.Address2),''),300)
	from Customer C
	inner join SalesCar S on C.CustomerCode=S.UserCode
	where C.CustomerCode=@CustomerCode1


	update SalesCar
		Set PossesorName = left(C.CustomerName,80),
		PossesorAddress = left(isnull(RTRIM(C.Prefecture),'')+isnull(RTRIM(C.City),'')+isnull(RTRIM(C.Address1),'')+isnull(RTRIM(C.Address2),''),300)
	from Customer C
	inner join SalesCar S on C.CustomerCode=S.OwnerCode
	where C.CustomerCode=@CustomerCode1


GO


