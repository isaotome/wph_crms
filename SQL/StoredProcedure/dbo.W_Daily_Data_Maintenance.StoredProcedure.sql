USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[W_Daily_Data_Maintenance]    Script Date: 2019/02/25 13:28:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- ---------------------------------------------------------------------------------------------------------
-- �@�\�F��������
-- �쐬���F???
-- �X�V���F
--		   2019/02/19 yano #3965 WE�ŐV�V�X�e���Ή��ECM/WE�ɂ�鏈���̕���
--		   2017/11/03 arc yano #3805 �ԗ��`�[�X�e�[�^�X�C���@�o�b�`�ōX�V�����C����������ʂɕ\������Ȃ�
--�@�@�@�@ 2017/02/21 arc yano #3711�@�ŏI�X�V���A�ŏI�X�V�҂̍X�V
--                                    ���ŏI�X�V�҂́uSYS(������)�v�œo�^
-- -----------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[W_Daily_Data_Maintenance]
AS
BEGIN

-- �s���ȃ��P�[�V�������C��

--Add 2019/01/18 yano #3965	--DB�����擾
declare @dbName nvarchar(50)
declare @eucdbName nvarchar(50)
declare @SQL nvarchar(MAX) = ''
declare @PARAM nvarchar(1024) = ''
declare @CRLF nvarchar(2) = char(13)+char(10)

select @dbName = DB_NAME()

IF @dbName = 'WPH_DB'
	SET @eucdbName = 'EUCDB'
ELSE IF @dbName = 'WE_DB'
	SET @eucdbName = 'EUCDB_WE'
	

update
	SalesCar
set
 	  LocationCode='999'
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()									--Add 2017/02/21 arc yano #3708
where
	DelFlag='0' and 
	CarStatus='001' and 
	LocationCode is null


---------------------------------
--�ԗ��ړ��F�������ɏ���
---------------------------------
exec W_MakeTransfar

--Del 2019/01/18 yano #3965
---------------------------------
--�����҂ւ̎󒍏��F�^�X�N�̈ړ�
---------------------------------
--exec W_Make_ApprovalTasks

--2016/05/16 arc yano #3533 �o�b�`�����@���������`�F�b�N�����̒�~
---------------------------------
--���������t���O�̃`�F�b�N
---------------------------------
--exec W_Make_DepositFLag

---------------------------------
--�[�Ԏ��̃��P�[�V�����C���A�ԑ�ԍ�������
---------------------------------
exec W_Check_CarLocation_CarSalesNumber

---------------------------------------
--�ڋq�f�[�^�R�s�[�@To�@������A�d����A�x����
---------------------------------------
exec W_Make_CustomerClain_Supplier

---------------------------------------
--�o�[���̌ڋq�R�[�h��Null��������
---------------------------------------
exec W_Alt_Customer_Journal

----------------------------------------------
--�������ς񂾎ԗ���SalesCar��UserCode���X�V��
----------------------------------------------
--2011/8/4�@�V�X�e���������[�X����Ă���̂Ńo�b�`�̕K�v�͂Ȃ��Ȃ���
--exec W_Alt_Customer_UserCode_in_SalesCar

--------------------------------------------------
-- AA / �˗��p�� / FAG�쐬�̎󒍓`�[��[�ԏ�������
--------------------------------------------------
exec W_AutoCarDelivery

--------------------------------------------------
-- ���܂��Ă����`�[�C���X�e�[�^�X����������
--------------------------------------------------
--Del 2017/11/03 arc yano #3805
/*
update
	W_CarSlipStatusChange 
Set
	  ChangeStatus='2'
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'					--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
where
	ChangeStatus='1' and 
	(
		SalesOrderStatus='002' or 
		SalesOrderStatus='007'
	)
*/
--------------------------------------------------
--�^�X�N�̃N���A
--------------------------------------------------
exec W_TaskClear

--------------------------------------------------
--AA�E�Г��̔��|�f�[�^������������
--------------------------------------------------
exec W_AA_ReceiptDataClear

--------------------------------------------------
-- �T�[�r�X�`�[�ɂČ��ϊ�����2�����ȏ�O�́u�����v
-- �ȃf�[�^�͍폜����B�C�������̃f�[�^��
-- WPH_DB_Backup �ɕۑ�����
--------------------------------------------------
EXEC W_Compress_ServiceSalesDatas

---------------------------------
-- �ԗ��S���A�T�[�r�X�S���̓o�^--
---------------------------------
-- �ڋq�}�X�^�̒S���ҁA�S�����_��`�[������������ēo�^����
exec W_AlterCustomerDepartmentEmployeeCode

--�����ԈႢ�̏C��
update
	  CarSalesHeader
Set
	  Vin = S.Vin
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
from
	CarSalesHeader H inner join 
	SalesCar S on H.SalesCarNumber=S.SalesCarNumber
where
	H.DelFlag='0' and 
	S.DelFlag='0' and 
	H.Vin <> S.Vin

update
	CarPurchaseOrder
set
	  Vin = S.Vin
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
from
	CarPurchaseOrder H inner join SalesCar S on H.SalesCarNumber=S.SalesCarNumber
where 
	H.DelFlag='0' and 
	S.DelFlag='0' and 
	H.Vin <> S.Vin

--�T�[�r�X�`�[�ƃ}�X�^�̍��ق��C��

update
	ServiceSalesHeader
Set
	  SalesCarNumber = S.SalesCarNumber
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
from
	ServiceSalesHeader H inner join SalesCar S on H.Vin = S.Vin
where
	H.SalesCarNumber ='' and 
	H.vin <> '' and 
	H.DelFlag='0' and 
	S.DelFlag='0'

update
	ServiceSalesHeader
set 
	  Vin = S.Vin
	, MorterViecleOfficialCode=S.MorterViecleOfficialCode
	, RegistrationNumberKana=S.RegistrationNumberKana
	, RegistrationNumberPlate=S.RegistrationNumberPlate
	, RegistrationNumberType=S.RegistrationNumberType
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
from
	ServiceSalesHeader H inner join SalesCar S on H.SalesCarNumber=S.SalesCarNumber
where
	( H.Vin <> S.Vin) or 
	(H.MorterViecleOfficialCode<>S.MorterViecleOfficialCode) or 
	(H.RegistrationNumberKana<>S.RegistrationNumberKana) or 
	(H.RegistrationNumberPlate<>S.RegistrationNumberPlate) or 
	(H.RegistrationNumberType<>S.RegistrationNumberType)

-- �ڋq�̌g�єԍ��Ɛ�����̘A����Q��A��
update
	CustomerClaim
set
	  TelNumber2 = C.MobileNumber
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
from
	Customer C inner join CustomerClaim CC on C.CustomerClaimCode=CC.CustomerClaimCode
where
	( 
		RTRIM(C.MobileNumber)<>'' and 
		C.MobileNumber is not  null
	) and 
	( 
		Rtrim(CC.TelNumber2)='' or 
		CC.TelNumber2 is null
	) 

--�t�H�[�h�T�[�r�X�����̌v�㕔��������I��222�ɏC��
/*
select JournalDate,SlipNumber,Amount,AccountType,DepartmentCode,Summary from Journal 
where DelFlag='0' and DepartmentCode='282' and AccountType='002'
and datediff(m,JournalDate,getdate())=0
order by JournalDate
*/
--�X�V


--Mod 2019/01/18 yano #3965
IF @dbName = 'WPH_DB'
BEGIN

	update
		Journal 
	set
		  DepartmentCode='222' 
		, Summary='(282)'+ISNULL(summary,'')
		, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
	where
		DelFlag='0' and 
		DepartmentCode='282' and 
	--and AccountType in ('001','002')
		datediff(m,JournalDate,getdate())<=1

	--Add 2015/01/14 arc yano #3149  CJ����Ó�T�[�r�X�����̌v�㕔��������I��FA����Ó�T�[�r�X�ɏC��
	update
		Journal 
	set
		  DepartmentCode='292'
		, Summary='(162)'+ISNULL(summary,'')
		, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
		, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
	where
		DelFlag='0' and 
		DepartmentCode='162' and
	--and AccountType in ('001','002')
		datediff(m,JournalDate,getdate())<=1
END

-- �T�[�r�X�`�[�̃��b�N�����@60���ȏネ�b�N���Ă�����̂���
update
	ServiceSalesHeader
set
	  ProcessSessionId = null
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()												--Add 2017/02/21 arc yano #3708
where
	ProcessSessionId in
	(
		select
			ProcessSessionId
		From
			ProcessSessionControl
		where
			DATEDIFF(n,CreateDate,GETDATE()) > 60
	)

delete From 
	ProcessSessionControl
where
	DATEDIFF(n,CreateDate,GETDATE()) > 60

--------------------
--�ԗ��d�����i�̏C��
--------------------
exec W_Edit_Carpurchase

--------------------
--�T�u�V�X�e���p�@���ƒǉ����ꂽ�炱�������Ă��Ȃ��
--------------------

--Mod 2019/01/18 yano #3965
	SET @SQL = ''
	SET @SQL = @SQL +'insert into' + @CRLF
	SET @SQL = @SQL + @eucdbName + '.dbo.AccountClassification' + @CRLF
	SET @SQL = @SQL +'select' + @CRLF
	SET @SQL = @SQL +' W.Classification1' + @CRLF
	SET @SQL = @SQL +' , W.ServiceWorkCode' + @CRLF
	SET @SQL = @SQL +' , ''SYS(W_Daily_Data_Maintenance)''' + @CRLF
	SET @SQL = @SQL +', GETDATE()' + @CRLF
	SET @SQL = @SQL +' , ''SYS(W_Daily_Data_Maintenance)''' + @CRLF
	SET @SQL = @SQL +', GETDATE()' + @CRLF
	SET @SQL = @SQL +', ''0''' + @CRLF
	SET @SQL = @SQL +'from' + @CRLF
	SET @SQL = @SQL + @eucdbName + '.dbo.AccountClassification A right join' + @CRLF
	SET @SQL = @SQL +'ServiceWork W on A.ServiceWorkCode=W.ServiceWorkCode' + @CRLF
	SET @SQL = @SQL +'where' + @CRLF
	SET @SQL = @SQL +'	A.ServiceWorkCode is null and ' + @CRLF
	SET @SQL = @SQL +'  W.DelFlag=''0''' + @CRLF
	
	EXECUTE sp_executeSQL @SQL

--insert into
--	eucdb.dbo.AccountClassification
--select 
--	  W.Classification1
--	, W.ServiceWorkCode
--	, 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
--	, GETDATE()
--	, 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
--	, GETDATE()
--	, '0'
--from 
--	eucdb.dbo.AccountClassification A right join
--	ServiceWork W on A.ServiceWorkCode=W.ServiceWorkCode
--where
--	A.ServiceWorkCode is null and 
--	W.DelFlag='0'
---

--- �������т����݂��Ă��Ȃ��̂ɁA���|�c����0�ɂȂ��Ă���f�[�^�̏C��
update
	ReceiptPlan
set
	  CompleteFlag='0' 
	, ReceivableBalance=R.Amount
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()									--Add 2017/02/21 arc yano #3708
from
	ReceiptPlan R left join 
	(
		Select 
			*
		from
			Journal
		where
			DelFlag='0'
	) J on left(R.SlipNumber,8)=left(J.SlipNumber,8)
where
	R.DelFlag='0' and 
	R.CompleteFlag='1' and 
	R.ReceivableBalance=0 and 
	J.SlipNumber is null and 
	R.Amount <> 0


-- ���ʂ�NULL�ɂȂ镔�i�݌ɏC��
update
	PartsStock
set
	  Quantity=0
	, LastUpdateEmployeeCode = 'SYS(W_Daily_Data_Maintenance)'		--Add 2017/02/21 arc yano #3708
	, LastUpdateDate = GETDATE()									--Add 2017/02/21 arc yano #3708
where
	Quantity is null
--Del 2015/03/17 wph uesugi #3161 ���i���׏������ɕ��i�݌Ƀe�[�u����DelFlag��null�ɂȂ�o�O�C��
--update PartsStock Set DelFlag='0' where DelFlag is null

--Excel Status Update
--Mod 2019/01/18 yano #3965
SET @SQL = ''
SET @SQL = @SQL +'update' + @CRLF
SET @SQL = @SQL + @eucdbName + '.dbo.ExcelDetail' + @CRLF
SET @SQL = @SQL +'Set' + @CRLF
SET @SQL = @SQL +' MakeStatus=''003''' + @CRLF
SET @SQL = @SQL +'where' + @CRLF
SET @SQL = @SQL +' MakeStatus=''002'' and ' + @CRLF
SET @SQL = @SQL +'datediff(d,CompleteDate,GETDATE()) >=7' + @CRLF
	
EXECUTE sp_executeSQL @SQL

--update
--	eucdb.dbo.ExcelDetail
--Set
--	MakeStatus='003'
--where
--	MakeStatus='002' and 
--	datediff(d,CompleteDate,GETDATE()) >=7

END



GO


