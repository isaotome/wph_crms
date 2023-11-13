USE [WPH_DB]
GO

/****** Object:  StoredProcedure [dbo].[InsertInventoryStock]    Script Date: 2017/02/17 16:11:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- ==============================================================================================================================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Update date:
-- 2017/02/08 arc yano #3620 �T�[�r�X�`�[���́@�`�[�ۑ��A�폜�A�ԓ`���̕��i�̍݌ɂ̖߂��Ή�
-- 2016/08/13   arc yano  #3596 �y�區�ځz����I�����Ή� 
--                               �@�I���̊Ǘ��𕔖喈����q�ɖ��ɕύX
--                               �A�I���ΏۊO�t���O��NULL�̏ꍇ�́A�I���Ώۂƌ���
-- 2016/02/03   arc yano  #3402 ���i�݌Ɋm�F�@�����݌ɂ̎Z�o���@�̕ύX �����݌ɂ̎Z�o�����������P�[�V�����̍݌ɐ��������ϐ��ɕύX
-- 2015/04/28   arc yano  ���i�݌ɊǗ��@�\ �s��C�� �i���ݏ����̒ǉ�
--						  (�L���ȕ��i[�폜�t���O='0' && �I���ΏۊO�t���O <> '1']�A�L���ȃ��P�[�V����[�폜�t���O='0']�̂�InventoryStock�ɓ����)
-- Description:	
--				���i�݌Ƀf�[�^�ޔ�
-- ==============================================================================================================================================
CREATE PROCEDURE [dbo].[InsertInventoryStock]
	 @InventoryMonth datetime, 
	 @WarehouseCode nvarchar(6),
	 @EmployeeCode nvarchar(50)
	 	 
AS	
	BEGIN
		INSERT INTO dbo.InventoryStock
			SELECT 
				NEWID(),																			--�I��ID
				'',																					--����R�[�h
				@InventoryMonth,																	--�I����
				LocationCode,																		--���P�[�V�����R�[�h
				@EmployeeCode,																		--�Ј��R�[�h
				'002',																				--�I���^�C�v
				null,																				--�Ǘ��ԍ�
				PartsNumber,																		--���i�ԍ�
				ISNULL(Quantity, 0),																--����
				@EmployeeCode,																		--�쐬��
			    GETDATE(),																			--�쐬��
				@EmployeeCode,																		--�ŏI�X�V��
				GETDATE(),																			--�ŏI�X�V��
				DelFlag,																			--�폜�t���O
				null,																				--�T�}��
			    CASE WHEN ISNULL(Quantity, 0) <= 0 THEN 0 ELSE Quantity END AS PhysicalQuantity,	--���I
				null,																				--�R�����g
				ProvisionQuantity,																	--�����ϐ�		--Add 2016/02/03   arc yano
				@WarehouseCode																		--�q�ɃR�[�h
			FROM 	
				PartsStock a
			WHERE exists 
				(
					SELECT 'X' FROM Location WHERE WarehouseCode = @WarehouseCode and DelFlag = '0' and LocationCode = a.LocationCode
				)
			and exists 
				(
					SELECT 'Y' FROM Parts WHERE DelFlag = '0' and ISNULL(NonInventoryFlag, 0) <> '1' and PartsNumber = a.PartsNumber
				)
			and
				ISNULL(a.DelFlag, '0') <> '1'						--Add 2017/02/08 arc yano #3620
	END




GO


