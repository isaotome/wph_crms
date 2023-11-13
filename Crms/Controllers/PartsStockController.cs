using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;
//Add 2014/08/04 arc yano IPO�Ή�(�݌ɊǗ��@�\) 
using System.Data.SqlClient;            
using Crms.Models;                 
using System.Data.Linq;
using System.Transactions;
using System.Xml.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

//Add 2016/07/05 arc yano #3598 ���i�݌Ɍ����@Excel�o�͋@�\�ǉ�
using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;

// Del 2015/04/23 arc nakayama ���i�݌Ɍ�����ʌ������@�I���Ɋւ���@�\�폜(���\�[�X�͌Â����r�W�������Q�Ƃ��ĉ����� ���I���@�\�͕ʉ�ʂɈڍs)
namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsStockController : Controller
    {
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// ������ʏ����\�������t���O
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PartsStockController() {
            db = new CrmsLinqDataContext();
            //�^�C���A�E�g�l�̐ݒ�
            db.CommandTimeout = 300;
        }

        //Add 2014/12/04 arc yano IPO�Ή�(�݌Ɍ���)
        private static readonly string TYP_PARTS = "002";                           // ���i(�I�����)
        private static readonly string FORM_NAME = "���i�݌�";                      // ��ʖ�
        private static readonly string PROC_NAME_SEARCH = "���i�݌Ɍ���"; 			// �������i���O�o�͗p�j
        private static readonly string PROC_NAME_EXCELDOWNLOAD = "Excel�o��";       // ������(Excel�o��)   //Add 2016/07/05 arc yano #3598 ���i�݌Ɍ����@Excel�o�͋@�\�ǉ�

        //Add 2017/07/18 arc yano #3779
        private static readonly string PROC_NAME_DATAEDIT = "�f�[�^�ҏW"; 			// �������i���O�o�͗p�j
        //private static readonly string PROC_NAME_ADDLINE  = "�f�[�^�s�ǉ�";         // �������i���O�o�͗p�j
        private static readonly string PROC_NAME_DELLINE  = "�f�[�^�s�폜";         // �������i���O�o�͗p�j

     
        /// <summary>
        /// ������ʏ����\��
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2016/07/05 arc yano #3598 ���i�݌Ɍ����@Excel�o�͋@�\�ǉ� ������S�̓I�Ɍ�����
        /// </history>
        public ActionResult Criteria() {
            
            criteriaInit = true;
            FormCollection form = new FormCollection();
            EntitySet<CommonPartsStock> line = new EntitySet<CommonPartsStock>();

            //�����l�̐ݒ�
            form["StockZeroVisibility"] = "0";                                      //0(�݌ɐ���=0��\�����Ȃ�)
            form["DefaultStockZeroVisibility"] = form["StockZeroVisibility"];       //0(�݌ɐ���=0��\�����Ȃ�)
            form["RequestFlag"] = "5";                                              //5(���N�G�X�g�̎�ނ́u�����v)
            form["TargetRange"] = "0";                                              //0(�Ώ۔N���w��Ȃ�)
            form["DefaultTargetRange"] = form["TargetRange"];                       //0(�Ώ۔N���w��Ȃ�)

            ViewData["PartsStockSearch"] = true;                                    //�݌Ƀe�[�u������

            return Criteria(form, line);
        }

        /// <summary>
        /// ���i�݌ɉ�ʕ\��
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2016/07/05 arc yano #3598 ���i�݌Ɍ����@Excel�o�͋@�\�ǉ� ������S�̓I�Ɍ�����
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form, EntitySet<CommonPartsStock> line)
        {

            ModelState.Clear();

            ActionResult ret = null;

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            PaginatedList<CommonPartsStock> list = new PaginatedList<CommonPartsStock>();

            //����\���̏ꍇ
            if (criteriaInit)
            {
                ret = View("PartsStockCriteria", list);
            }
            //���� or Excel�o��
            else 
            {
                //���͒l�̃`�F�b�N
                ValidateForSearch(form);

                if (!ModelState.IsValid)
                {
                    //��ʐݒ�
                    SetComponent(form);
                    // ������ʂ̕\��
                    return View("PartsStockCriteria", list);
                }

                switch (form["RequestFlag"])
                {
                    case "1": //Excel�o��

                        ret = Download(form);
                        
                        break;

                    default: //��������

                        list = new PaginatedList<CommonPartsStock>(SearchList(form).AsQueryable<CommonPartsStock>(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                        ret = View("PartsStockCriteria", list);
                        break;
                }
            }

            //��ʐݒ�
            SetComponent(form);

            //�R���g���[���̗L������
            GetViewResult(form);

            return ret ;
        }

        //Del 2016/08/13 arc yano #3596 �s�v�̂��߁A�폜

        /// <summary>
        /// ���i�݌Ɍ�����ʕ\��
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/24 arc yano #3799 ���i�݌Ɍ����@���݁^�ߋ������ʂ���t���O�̐ݒ��ǉ�
        /// 2016/07/05 arc yano #3598 ���i�݌Ɍ����@Excel�o�͋@�\�ǉ� ������S�̓I�Ɍ�����
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        private List<CommonPartsStock> SearchList(FormCollection form)
        {
            // Info���O�o��
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);
            List<CommonPartsStock> list = new List<CommonPartsStock>();

            //�S�̂̒I���X�e�[�^�X�擾
            CodeDao dao = new CodeDao(db);

            //�I�����̎擾
            DateTime targetDate = DateTime.Parse((dao.GetYear(true, form["TargetDateY"]) == null ? string.Format("{0:yyyy}", DateTime.Now) : dao.GetYear(true, form["TargetDateY"]).Name) + "/" + (dao.GetMonth(true, form["TargetDateM"]) == null ? string.Format("{0:MM}", DateTime.Now) : dao.GetMonth(true, form["TargetDateM"]).Name) + "/01");  //�N�ƌ����Ȃ���    

            //-----------------------------------------------------------------------
            // Mod 2015/03/10 arc yano #3160(����R�[�h��C�Ӎ��ڂɕύX) 
            //     �Q�Ƃ���e�[�u�����ȉ��̏����ŕύX����悤�ɏC��
            //     �Ώ۔N���̓��͂���̏ꍇ��InventoryStock
            //     �Ώ۔N���̓��͂Ȃ��̏ꍇ��PartsStock
            //-----------------------------------------------------------------------
            if ((string.IsNullOrWhiteSpace(form["TargetDateY"])) || (string.IsNullOrWhiteSpace(form["TargetDateM"]))) //�Ώ۔N���̂����ꂩ�����I����Ԃ̏ꍇ
            {
                list = GetSearchResultListNow(form, targetDate);
                ViewData["PartsStockSearch"] = true;                //Add 2017/07/24 arc yano #3799
            }
            else
            {
                //��������
                list = GetSearchResultListPast(form, targetDate);
                ViewData["PartsStockSearch"] = false;                //Add 2017/07/24 arc yano #3799
            }

            //��ʐݒ�
            SetComponent(form);

            return list;
        }
       
        #region ���i�݌Ɍ���
        /// <summary>
        /// ���i�݌Ɍ���(���݌�)
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �݌ɂO�ΏہA�݌ɂO�ΏۊO�ŌĂԃ��\�b�h����{��
        /// 2016/07/05 arc yano #3598 ���i�݌Ɍ����@Excel�o�͋@�\�ǉ� ������S�̓I�Ɍ�����
        /// </history>
        //���i�݌Ɍ���(���݌�)
        private List<CommonPartsStock> GetSearchResultListNow(FormCollection form, DateTime targetDate)
        {
            //�������ڃZ�b�g
            CodeDao dao = new CodeDao(db);
            PartsStock condition = new PartsStock();
            condition.Location = new Location();
            condition.Parts = new Parts();
            //condition.Location.DepartmentCode = form["DepartmentCode"];
            condition.DepartmentCode = form["DepartmentCode"];
            condition.Location.LocationCode = form["LocationCode"];
            condition.Location.LocationName = form["LocationName"];
            condition.Parts.PartsNumber = form["PartsNumber"];
            condition.Parts.PartsNameJp = form["PartsNameJp"];

            //Mod 2016/08/13 arc yano #3596
            //�݌ɂO�\�����ǂ����̏�Ԃ�ݒ�
            condition.StockZeroVisibility = form["StockZeroVisibility"];


            //���݌ɂ̌���
            return new PartsStockDao(db).GetListAllByCondition(condition, targetDate);
            
            /*
            if (form["StockZeroVisibility"] == "1")
            {
                //�݌Ƀ[���Ώ�
                return new PartsStockDao(db).GetListByDepartmentAll(condition, targetDate);
            }
            else
            {
                //�݌Ƀ[���ΏۊO
                return new PartsStockDao(db).GetListByDepartmentAllNotQuantityZero(condition, targetDate);
            }
            */
        }
        #endregion

        #region  ���i�݌Ɍ���(�w�茎�݌�)
        /// <summary>
        /// ���i�݌Ɍ���(�w�茎�݌�)
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �݌ɂO�ΏہA�݌ɂO�ΏۊO�ŌĂԃ��\�b�h����{��
        /// 2016/07/05 arc yano #3598 ���i�݌Ɍ����@Excel�o�͋@�\�ǉ� ������S�̓I�Ɍ�����
        /// </history>

        private List<CommonPartsStock> GetSearchResultListPast(FormCollection form, DateTime targetDate)
        {
            CodeDao dao = new CodeDao(db);

            //�������ڃZ�b�g
            CommonPartsStockSearch CommonPartsStockSearchCondition = new CommonPartsStockSearch();
            CommonPartsStockSearchCondition.TargetDate = targetDate;
            CommonPartsStockSearchCondition.DepartmentCode = form["DepartmentCode"];
            CommonPartsStockSearchCondition.LocationCode = form["LocationCode"];
            CommonPartsStockSearchCondition.LocationName = form["LocationName"];
            CommonPartsStockSearchCondition.PartsNumber = form["PartsNumber"];
            CommonPartsStockSearchCondition.PartsNameJp = form["PartsNameJp"];

            
            //Mod 2016/08/13 arc yano #3596
            //�݌ɂO�\�����ǂ����̏�Ԃ�ݒ�
            CommonPartsStockSearchCondition.StockZeroVisibility = form["StockZeroVisibility"];


            //�w�茎�݌ɂ̌���
            return new InventoryStockDao(db).GetListAllByCondition(CommonPartsStockSearchCondition);

            /*
            if (form["StockZeroVisibility"] == "1")
            {
                return new InventoryStockDao(db).GetListByDepartmentAll(CommonPartsStockSearchCondition);
            }
            else
            {
                return new InventoryStockDao(db).GetListByDepartmentAllNotQuantityZero(CommonPartsStockSearchCondition);
            }
            */
        }
        #endregion

        #region �ʏ����pValidationcheck

        /// <summary>
        /// ��������Validation�`�F�b�N
        /// </summary>
        /// <param name="form"></param>
        /// <history>
        /// 2015/03/11 arc yano #3160
        /// </history>
        private void ValidateForSearch(FormCollection form)
        {
            //�Ώ۔N��(�N=���I���A��=�I��)
            if (string.IsNullOrEmpty(form["TargetDateY"]) && !string.IsNullOrEmpty(form["TargetDateM"]))
            {
                ModelState.AddModelError("TargetDateY", "�Ώ۔N��(�N)��I�����Ă�������");
            }
            //�Ώ۔N��(�N=�I���A��=���I��)
            if (!string.IsNullOrEmpty(form["TargetDateY"]) && string.IsNullOrEmpty(form["TargetDateM"]))
            {
                ModelState.AddModelError("TargetDateM", "�Ώ۔N��(��)��I�����Ă�������");
            }
            //�Ώ۔N���w�肠�肩�A�Ώ۔N��(�N=�I���A��=���I��)
            if ((form["TargetRange"] == "1") && (string.IsNullOrEmpty(form["TargetDateY"]) && string.IsNullOrEmpty(form["TargetDateM"])))
            {
                ModelState.AddModelError("TargetDateY", "�Ώ۔N���w�肠��̏ꍇ�́A�Ώ۔N����I�����Ă�������");
                ModelState.AddModelError("TargetDateM", "");
            }

            return;
        }

        /// <summary>
        /// ���i�݌ɕҏW����validation�`�F�b�N
        /// </summary>
        /// <param name="form"></param>
        /// <history>
        /// 2022/08/30 yano #4101�y���i�݌ɕҏW�z�݌ɕҏW��ʂ̒������̃��b�Z�[�W
        /// 2017/07/18 arc yano #3779 ���i�݌Ɂi���i�݌ɂ̏C���j �V�K�쐬
        /// </history>
        private void ValidateForDataEdit(FormCollection form, EntitySet<CommonPartsStock> line)
        {
            int cnt = 0;
            bool msg = false;

            //�ҏW�s���O�s�̏ꍇ�̓G���[���A����ȍ~��validation�G���[
            if (line == null || line.Count == 0)
            {
                ModelState.AddModelError("", "���ׂ�0�s�̂��߁A�ۑ��ł��܂���");

                //�����^�[��
                return;
            }

            foreach (var a in line)
            {
                ///------------------------
                // �K�{�`�F�b�N
                //------------------------           
                //���P�[�V�����R�[�h
                if (string.IsNullOrWhiteSpace(a.LocationCode))
                {
                    ModelState.AddModelError("line[" + cnt + "].LocationCode", MessageUtils.GetMessage("E0001", cnt + 1 + "�s�ڂ̃��P�[�V�����R�[�h"));
                }
                //���i�ԍ�
                if (string.IsNullOrWhiteSpace(a.PartsNumber))
                {
                    ModelState.AddModelError("line[" + cnt + "].PartsNumber", MessageUtils.GetMessage("E0001", cnt + 1 + "�s�ڂ̕��i�ԍ�"));
                }

                //Add 2022/08/30 yano #4101
                //---------------------------------------------------
                // �ҏW���ɍ݌ɐ��A�����ϐ����X�V���ꂽ�ꍇ�̓G���[
                //---------------------------------------------------
              
                  PartsStock stock = new PartsStockDao(db).GetByKey(a.PartsNumber, a.LocationCode);
          
                  decimal quantity = stock != null ? (stock.Quantity ?? 0m) : 0m;
                  decimal provisionquantity = stock != null ? (stock.ProvisionQuantity ?? 0m) : 0m;

                  string quantityindex  = "line[" + cnt + "].SavedQuantity";
                  string provisionindex = "line[" + cnt + "].SavedProvisionQuantity";
                    
                  //�C���O�ɐ���
                  decimal prequantity;
                  decimal.TryParse(form[quantityindex], out prequantity);

                  //�C���O�Ɉ����ϐ�
                  decimal preprovision;
                  decimal.TryParse(form[provisionindex], out preprovision);
          
                  //���ʂɕύX�����������m�F
                  if (quantity != prequantity)
                  {
                      if (ModelState.IsValid) 
                      { 
                          ModelState.Clear();
                      }
                      a.Quantity = quantity;
                      ModelState.AddModelError("line[" + cnt + "].Quantity",  cnt + 1 + "�s�ڂ̍݌ɐ����ʂ̃��[�U�ɂ��'" + prequantity + "'��'" + quantity + "'�ɍX�V����܂����B�ēx�C�����Ă�������");
                  }
                    //�����ϐ��ɕύX�����������m�F
                  if (provisionquantity != preprovision)
                  {
                      if (ModelState.IsValid) 
                      { 
                          ModelState.Clear();
                      }

                      a.ProvisionQuantity = provisionquantity;
                      ModelState.AddModelError("line[" + cnt + "].ProvisionQuantity", cnt + 1 + "�s�ڂ̈����ϐ����ʂ̃��[�U�ɂ��'" + preprovision + "'��'" + provisionquantity + "'�ɕύX����܂����B�ēx�C�����Ă�������");
                  }
                 
                //----------------------------------------------
                // �����ϐ������ʂ������Ă��Ȃ����̃`�F�b�N
                //----------------------------------------------
                if (a.Quantity - a.ProvisionQuantity < 0)
                {
                    ModelState.AddModelError("line[" + cnt + "].ProvisionQuantity", "���ʂ�葽�������ϐ���ݒ�ł��܂���");
                }
                
                //--------------------------------------------------------
                //�@�����ϐ����T�[�r�X�`�[�̈����ϐ��������ꍇ�̓G���[
                //--------------------------------------------------------
                if (ModelState.IsValid)
                {
                    //����q�Ƀ}�X�^����q�ɃR�[�h�̊���o��
                    DepartmentWarehouse condition = new DepartmentWarehouse();

                    condition.WarehouseCode = new LocationDao(db).GetByKey(a.LocationCode).WarehouseCode;

                    List<DepartmentWarehouse> dwList = new DepartmentWarehouseDao(db).GetByCondition(condition);

                    //�T�[�r�X�`�[�̑������ϐ����Z�o
                    decimal svProvisionQuantity = 0m;
                    foreach (var b in dwList)
                    {
                        svProvisionQuantity += new ServiceSalesOrderDao(db).GetPartsProvisionQuantityByPartsNumber(b.DepartmentCode, a.PartsNumber);
                    }

                    //�q�ɓ��ɑ��݂���݌ɂ̈����ϐ����Z�o
                    decimal stcProvisionQuantity = 0m;

                    stcProvisionQuantity = new PartsStockDao(db).GetListByWarehouse(condition.WarehouseCode, a.PartsNumber, false, true).Where(x => !x.LocationCode.Equals(a.LocationCode)).AsQueryable().Sum(x => x.ProvisionQuantity ?? 0);

                    stcProvisionQuantity += (a.ProvisionQuantity ?? 0);

                    if (stcProvisionQuantity < svProvisionQuantity)
                    {
                        ModelState.AddModelError("line[" + cnt + "].ProvisionQuantity", "�T�[�r�X�`�[�ň�������Ă��鐔�ʂ̍��v��菭�Ȃ������ϐ���ݒ�ł��܂���");
                    }
                }

                //--------------------------------------------------------
                //�@���ɑ��݂���݌ɏ���V�K�쐬�����ꍇ�̓G���[
                //--------------------------------------------------------
                if (a.NewRecFlag.Equals(true))
                {
                    PartsStock rec = new PartsStockDao(db).GetByKey(a.PartsNumber, a.LocationCode, false);

                    if (rec != null)
                    {
                        ModelState.AddModelError("line[" + cnt + "].LocationCode", "");
                        ModelState.AddModelError("line[" + cnt + "].PartsNumber", "���ɓ���̕��i�ԍ��A���P�[�V�����̍݌ɏ�񂪑��݂��Ă��邽�߁A�V�K�쐬�ł��܂���B");
                    }
                }

                //���P�[�V�����R�[�h�E���i�ԍ���null�ł͂Ȃ��ꍇ
                if (!string.IsNullOrWhiteSpace(a.LocationCode) && !string.IsNullOrWhiteSpace(a.PartsNumber))
                {
                    //----------------------------------------------------------------
                    // ���ꃍ�P�[�V�����E���i�ԍ��̃��R�[�h���ҏW��ʓ��ɕ����s����
                    //----------------------------------------------------------------
                    int count = line.Where(x => x.LocationCode.Equals(a.LocationCode) && x.PartsNumber.Equals(a.PartsNumber)).Count();

                    if (count > 1)
                    {
                        if (msg == false)
                        {
                            ModelState.AddModelError("line[" + cnt + "].PartsNumber", "");
                            ModelState.AddModelError("line[" + cnt + "].LocationCode", "���P�[�V�����R�[�h=" + line[cnt].LocationCode + ": ���i�ԍ�=" + line[cnt].PartsNumber + "���ҏW��ʏ�ɕ����s���݂��܂�");
                            msg = true;
                        }
                        else
                        {
                            ModelState.AddModelError("line[" + cnt + "].PartsNumber", "");
                            ModelState.AddModelError("line[" + cnt + "].LocationCode", "");
                        }

                    }                
                }
                cnt++;
            }

            return;
        }

        #endregion

        #region ��ʃR���|�[�l���g�ݒ�
        /// <summary>
        /// �e�R���g���[���̒l�̐ݒ�
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        private void SetComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);
            
            //Mod 2015/02/22 arc iijima �S���啔�i�݌Ɍ����Ή�
            //���������̍ăZ�b�g

            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(false), form["TargetDateY"], true);
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(false), form["TargetDateM"], true);
            form["NowTargetDateY"] = DateTime.Today.Year.ToString().Substring(1, 3);            //����ʌ����p�����N
            form["NowTargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');           //����ʌ����p������
            ViewData["NowTargetDateY"] = form["NowTargetDateY"];
            ViewData["NowTargetDateM"] = form["NowTargetDateM"];
            ViewData["TargetDateY"] = form["TargetDateY"];
            ViewData["TargetDateM"] = form["TargetDateM"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];

            //�����͈�
            ViewData["TargetRange"] = form["TargetRange"];
            ViewData["DefaultTargetRange"] = form["DefaultTargetRange"];

            if (string.IsNullOrEmpty(form["DepartmentCode"])){              //���喼
                ViewData["DepartmentName"] = "";                            //����R�[�h�������͂Ȃ�󕶎�
            }
            else{
                ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"].ToString()).DepartmentName;  //���͍ς݂Ȃ畔��e�[�u�����猟��
            }
            ViewData["LocationCode"] = form["LocationCode"];
            ViewData["LocationName"] = form["LocationName"];
            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["PartsNameJp"] = form["PartsNameJp"];
            ViewData["DefaultTargetDateY"] = form["DefaultTargetDateY"];    //�Ώۓ��t(yyyy)
            ViewData["DefaultTargetDateM"] = form["DefaultTargetDateM"];    //�Ώۓ��t(MM)
            
            //Mod 2015/02/12 arc yano ���i�݌Ɍ��� �t�H�[���̍݌�0�\���̒l��NULL�̏ꍇ��0���f�t�H���g�Őݒ肷��B
            ViewData["StockZeroVisibility"] = (form["StockZeroVisibility"] != null ? form["StockZeroVisibility"] : "0");
            
            //Mod 2015/03/11 arc yano #3160
            ViewData["DefaultStockZeroVisibility"] = form["DefaultStockZeroVisibility"];
            ViewData["RequestFlag"] = form["RequestFlag"];

            //���i�I����Ɠ��擾
            PartsInventoryWorkingDate WorkingDate = new PartsInventoryWorkingDateDao(db).GetAllVal();
            if (WorkingDate != null)
            {
                ViewData["PartsInventoryWorkingDate"] = WorkingDate.InventoryWorkingDate;
            }

            //Mod 2015/02/03 arc nakayama ���i�I�������ԗ��ƕ�����Ή�(InventorySchedule �� InventoryScheduleParts)
            //�I���X�e�[�^�X�̐ݒ�
            //Mod 2015/02/22 arc iijima ���tnull�Ή�
            if (string.IsNullOrEmpty(form["TargetDateY"]) || string.IsNullOrEmpty(form["TargetDateM"]))
            {
                ViewData["DepartmentInventoryStatus"] = ""; //���t�����͂���Ă��Ȃ����߁A�X�e�[�^�X�͋�
                return;
            }
            else
            {
                DateTime targetDate = DateTime.Parse(dao.GetYear(false, form["TargetDateY"]).Name + "/" + dao.GetMonth(false, form["TargetDateM"]).Name + "/01");
                InventoryScheduleParts ivs = new InventorySchedulePartsDao(db).GetByKey(form["DepartmentCode"], targetDate, TYP_PARTS);

                if (ivs != null)
                {
                    ViewData["DepartmentInventoryStatus"] = ivs.InventoryStatus;
                }
            }
            return;
        }
        #endregion

        //Add 2016/07/05 arc yano #3598 ���i�݌Ɍ����@Excel�o�͋@�\�ǉ�
        #region Excel�o�͏���
        /// <summary>
        /// Excel�t�@�C���̃_�E�����[�h
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        private ActionResult Download(FormCollection form)
        {
            //-------------------------------
            //��������
            //-------------------------------
            // Info���O�o��
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCELDOWNLOAD);

            ModelState.Clear();

            PaginatedList<CommonPartsStock> list = new PaginatedList<CommonPartsStock>();
            

            //-------------------------------
            //Excel�o�͏���
            //-------------------------------
            //�t�@�C����
            string fileName = "PartsStock" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //���[�N�t�H���_�擾
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //�e���v���[�g�t�@�C���p�X�擾
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForPartsStock"]) ? "" : ConfigurationManager.AppSettings["TemplateForPartsStock"];

            //�e���v���[�g�t�@�C���̃p�X���ݒ肳��Ă��Ȃ��ꍇ
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C���̃p�X���ݒ肳��Ă��܂���");
                SetComponent(form);
                return View("PartsStockCriteria", list);
            }

            //�G�N�Z���f�[�^�쐬
            byte[] excelData = MakeExcelData(form, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetComponent(form);
                return View("PartsStockCriteria", list);
            }

            //�R���e���c�^�C�v�̐ݒ�
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);
        }

        /// <summary>
        /// �G�N�Z���f�[�^�쐬(�e���v���[�g�t�@�C������)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="fileName">���[��</param>
        /// <param name="tfileName">���[�e���v���[�g</param>
        /// <returns>�G�N�Z���f�[�^</returns>
        private byte[] MakeExcelData(FormCollection form, string fileName, string tfileName)
        {

            //----------------------------
            //��������
            //----------------------------
            ConfigLine configLine;                  //�ݒ�l
            byte[] excelData = null;                //�G�N�Z���f�[�^
            bool ret = false;
            bool tFileExists = true;                //�e���v���[�g�t�@�C������^�Ȃ�(���ۂɂ��邩�ǂ���)


            //�f�[�^�o�̓N���X�̃C���X�^���X��
            DataExport dExport = new DataExport();

            //�G�N�Z���t�@�C���I�[�v��(�e���v���[�g�t�@�C������)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //�e���v���[�g�t�@�C�������������ꍇ
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C����������܂���ł����B");
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //----------------------------
            // �ݒ�V�[�g�擾
            //----------------------------
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //�ݒ�f�[�^���擾(config)
            if (config != null)
            {
                configLine = dExport.GetConfigLine(config, 2);
            }
            else //config�V�[�g�������ꍇ�̓G���[
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C����config�V�[�g���݂���܂���");

                excelData = excelFile.GetAsByteArray();

                //�t�@�C���폜
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //���[�N�V�[�g�I�[�v��
            var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            //----------------------------
            // ���������o��
            //----------------------------
            configLine.SetPos[0] = "A1";

            //����������������쐬
            DataTable dtCondtion = MakeConditionRow(form);

            //�f�[�^�ݒ�
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // �f�[�^�s�o��
            //----------------------------
            //�o�͈ʒu�̐ݒ�
            configLine = dExport.GetConfigLine(config, 2);

            //�������ʂ̎擾
            List<CommonPartsStock> list = SearchList(form);

            List<ExcelCommonPartsStock> elist = null;

            //�擾�����������ʂ�Excel�o�͗p�ɐ��`
            elist = MakeExcelList(list, form);

            //�f�[�^�ݒ�
            ret = dExport.SetData<ExcelCommonPartsStock, ExcelCommonPartsStock>(ref excelFile, elist, configLine);

            excelData = excelFile.GetAsByteArray();

            //���[�N�t�@�C���폜
            try
            {
                excelFile.Stream.Close();
                excelFile.Dispose();
                dExport.DeleteFileStream(fileName);
            }
            catch
            {
                //
            }

            return excelData;
        }

          /// <summary>
        /// �������ʂ�Excel�p�ɐ��`
        /// </summary>
        /// <param name="list">��������</param>
        /// <returns name="elist">��������(Exel�o�͗p)</returns>
        private List<ExcelCommonPartsStock> MakeExcelList(List<CommonPartsStock> list, FormCollection form)
        {
            List<ExcelCommonPartsStock> elist = new List<ExcelCommonPartsStock>();

            //�Ώ۔N���̎w�肪����ꍇ�͎��I�𐔗ʂ�
            if (!string.IsNullOrWhiteSpace(form["TargetDateY"]) && !string.IsNullOrWhiteSpace(form["TargetDateM"]))
            {
                elist = list.Select(x =>
                        new ExcelCommonPartsStock()
                        {
                            DepartmentName = x.DepartmentName
                            ,
                            LocationCode = x.LocationCode
                            ,
                            LocationName = x.LocationName
                            ,
                            PartsNumber = x.PartsNumber
                            ,
                            PartsNameJp = x.PartsNameJp
                            ,
                            Quantity = string.Format("{0:F1}", (x.PhysicalQuantity ?? 0))
                            ,
                            ProvisionQuantity = !x.LocationType.Equals("001") ? "-" : string.Format("{0:F1}", (x.ProvisionQuantity ?? 0))
                            ,
                            FreeQuantity = !x.LocationType.Equals("001") ? "-" : string.Format("{0:F1}", (x.PhysicalQuantity ?? 0) - (x.ProvisionQuantity ?? 0))
                            ,
                            StandardPrice = x.StandardPrice != null ? string.Format("{0:N0}", x.StandardPrice) : ""
                            ,
                            MoveAverageUnitPrice = x.MoveAverageUnitPrice != null ? string.Format("{0:N0}", x.MoveAverageUnitPrice) : ""
                            ,
                            Price = x.Price != null ? string.Format("{0:N0}", x.Price) : ""
                        }
                ).ToList();
            }
            else
            {
                elist = list.Select(x =>
                        new ExcelCommonPartsStock()
                        {
                            DepartmentName = x.DepartmentName
                            ,
                            LocationCode = x.LocationCode
                            ,
                            LocationName = x.LocationName
                            ,
                            PartsNumber = x.PartsNumber
                            ,
                            PartsNameJp = x.PartsNameJp
                            ,
                            Quantity = string.Format("{0:F1}", (x.Quantity ?? 0))
                            ,
                            ProvisionQuantity = !x.LocationType.Equals("001") ? "-" : string.Format("{0:F1}", (x.ProvisionQuantity ?? 0))
                            ,
                            FreeQuantity = !x.LocationType.Equals("001") ? "-" : string.Format("{0:F1}", (x.Quantity ?? 0) - (x.ProvisionQuantity ?? 0))
                            ,
                            StandardPrice = x.StandardPrice != null ? string.Format("{0:N0}", x.StandardPrice) : ""
                            ,
                            MoveAverageUnitPrice = x.MoveAverageUnitPrice != null ? string.Format("{0:N0}", x.MoveAverageUnitPrice) : ""
                            ,
                            Price = x.Price != null ? string.Format("{0:N0}", x.Price) : ""
                        }
                ).ToList();
            }

            return elist;

        }

        /// <summary>
        /// �����������쐬(Excel�o�͗p)
        /// </summary>
        /// <param name="condition">��������</param>
        /// <returns></returns>
        private DataTable MakeConditionRow(FormCollection form)
        {
            //�o�̓o�b�t�@�p�R���N�V����
            DataTable dt = new DataTable();
            String conditionText = "";

            //---------------------
            //�@���`
            //---------------------
            dt.Columns.Add("CondisionText", Type.GetType("System.String"));
            

            //---------------
            //�f�[�^�ݒ�
            //---------------
            DataRow row = dt.NewRow();
            //�I���Ώ۔N��
            if (!string.IsNullOrWhiteSpace(form["TargetDateY"]) && !string.IsNullOrWhiteSpace(form["TargetDateM"]))
            {
                CodeDao dao = new CodeDao(db);

                conditionText += "�Ώ۔N��=" + dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name;
            }
            //����
            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

                conditionText += "�@����=" + dep.DepartmentName + "(" + dep.DepartmentCode + ")";
            }
            //���P�[�V�����R�[�h
            if (!string.IsNullOrEmpty(form["LocationCode"]))
            {
                conditionText += "�@���P�[�V�����R�[�h=" + form["LocationCode"];
            }
            //���P�[�V������
            if (!string.IsNullOrEmpty(form["LocationName"]))
            {
                conditionText += "�@���P�[�V������=" + form["LocationName"];
            }
            //���i�ԍ�
            if (!string.IsNullOrEmpty(form["PartsNumber"]))
            {
                conditionText += "�@���i�ԍ�=" + form["PartsNumber"];
            }
            //���i��
            if (!string.IsNullOrEmpty(form["PartsNameJp"]))
            {
                conditionText += "�@���i��=" + form["PartsNameJp"];
            }
            //�݌Ƀ[���\��
            if (form["StockZeroVisibility"].Equals("0"))
            {
                conditionText += "�@�݌Ƀ[���\��=���Ȃ�";
            }
            else
            {
                conditionText += "�@�݌Ƀ[���\��=����";
            }

            //�쐬�����e�L�X�g���J�����ɐݒ�
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);

            return dt;
        }

        /// <summary>
        /// �w�b�_�s�̍쐬(Excel�o�͗p)
        /// </summary>
        /// <param name="list">�񖼃��X�g</param>
        /// <returns></returns>
        private DataTable MakeHeaderRow(IEnumerable<XElement> list)
        {
            //�o�̓o�b�t�@�p�R���N�V����
            DataTable dt = new DataTable();

            //�f�[�^�e�[�u����xml�̒l��ݒ肷��
            int i = 1;
            DataRow row = dt.NewRow();
            foreach (var header in list)
            {
                dt.Columns.Add("Column" + i, Type.GetType("System.String"));
                row["Column" + i] = header.Value;
                i++;
            }

            dt.Rows.Add(row);

            return dt;
        }
        #endregion

       
        /// <summary>
        /// �f�[�^�ҏW��ʏ����\��
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/06/01 arc yano #3900 ���i�݌Ɍ����@�ҏW��ʂ�\������Ɠ���̍݌ɏ�񂪂Q�s�\�������
        /// 2017/07/18 arc yano #3779 ���i�݌Ɂi���i�݌ɂ̏C���j�V�K�쐬
        /// </history>
        public ActionResult DataEdit()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();
            EntitySet<CommonPartsStock> line = new EntitySet<CommonPartsStock>();

            //�����l�̐ݒ�
            form["RequestFlag"] = "99";                                              // 99(���N�G�X�g�̎�ނ́u�����v)

            form["CreateFlag"] = Request["CreateFlag"];                              //�V�K�쐬�t���O

            form["LocationCode"] = Request["LocationCode"];                          //���P�[�V�����R�[�h
            form["PartsNumber"]  = Request["PartsNumber"];                           //���i�ԍ�
            form["DepartmentCode"] = Request["DepartmentCode"];                      //����R�[�h    //Add 2018/06/01 arc yano #3900

            return DataEdit(form, line);
        }

        /// <summary>
        /// �f�[�^�ҏW��ʕ\��
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/01 arc yano #3900 ���i�݌Ɍ����@�ҏW��ʂ�\������Ɠ���̍݌ɏ�񂪂Q�s�\�������
        /// 2017/07/18 arc yano #3779  ���i�݌Ɂi���i�݌ɂ̏C���j�V�K�쐬
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DataEdit(FormCollection form, EntitySet<CommonPartsStock> line)
        {
            ModelState.Clear();

            ActionResult ret = null;

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            List<CommonPartsStock> list = new List<CommonPartsStock>();

            //�V�K�쐬�̏ꍇ
            if (!string.IsNullOrWhiteSpace(form["CreateFlag"]) && form["CreateFlag"].Equals("1"))
            {
                ret = AddLine(form, line);
            }
            //�X�V�̏ꍇ
            else
            {
                switch (form["RequestFlag"])
                {
                    case "10" : //�ۑ�

                        //���͒l�̃`�F�b�N
                        ValidateForDataEdit(form, line);

                        if (!ModelState.IsValid)
                        {
                            //��ʐݒ�
                            SetComponent(form);
                            //�R���g���[���̗L������
                            GetViewResult(form);

                            if (line != null && line.Count > 0)
                            {
                                list = line.ToList();
                            }
                            
                            return View("PartsStockDataEdit", list);
                        }

                        //�ۑ�����
                        ret = DataSave(form, line);

                        break;
                    
                    case "11": //�s�ǉ�

                        ret = AddLine(form, line);

                        break;

                    case "12": //�s�폜

                        ret = DelLine(form, line);

                        break;

                    default: //��������

                        list = SearchList(form).Where(x => x.LocationCode.Equals(form["LocationCode"]) && x.PartsNumber.Equals(form["PartsNumber"]) && x.DepartmentCode.Equals(form["DepartmentCode"])).ToList<CommonPartsStock>();     //Add 2018/06/01 arc yano #3900
                        ret = View("PartsStockDataEdit", list);
                        break;
                }
            }

            //��ʐݒ�
            SetComponent(form);
                
            //�R���g���[���̗L������
            GetViewResult(form);

            return ret;
        }
        
        /// <summary>
        /// ���i�݌Ɍ����_�C�A���O�����\��
        /// </summary>
        /// <returns></returns>
        public ActionResult CriteriaDialog(){
            criteriaInit = true;
            Employee emp = (Employee)Session["Employee"];
            FormCollection form = new FormCollection();
            //if(emp.SecurityRole.SecurityLevelCode.Equals("001")){
                form["DepartmentCode"] = emp.DepartmentCode;
            //}
            return CriteriaDialog(form);
        }
        /// <summary>
        /// ���i�݌Ɍ����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���i������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {
            
            // �N�G���X�g�����O���猟���������擾
            form["CarBrandName"] = Request["CarBrandName"];

            PaginatedList<GetPartsStockForDialogResult> list = new PaginatedList<GetPartsStockForDialogResult>();
            Employee emp = (Employee)Session["Employee"];
            //form["DepartmentCode"] = emp.DepartmentCode;

            // ���������̐ݒ�
            // Add 2015/10/07 arc nakayama #3266_���i�݌Ɍ����_�C�A���O�Ɏd����̌������ڂ�ǉ�
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["PartsNameJp"] = form["PartsNameJp"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["SupplierCode"] = form["SupplierCode"];
            if (!string.IsNullOrEmpty(form["SupplierCode"]))
            {
                ViewData["SupplierName"] = new SupplierDao(db).GetByKey(form["SupplierCode"], false).SupplierName;
            }
            form["DelFlag"] = "0";

            Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

            // �}�X�^�ɑ��݂��Ȃ�����̓G���[
            if (!criteriaInit && dep == null) {
                ModelState.AddModelError("", MessageUtils.GetMessage("E0016", "����R�[�h"));
                return View("PartsStockCriteriaDialog", list);
            } else {
                ViewData["DepartmentName"] = dep.DepartmentName;
            }

            // �w�肳�ꂽ����̎Q�ƌ������Ȃ��ꍇ�̓G���[
            switch (emp.SecurityRole.SecurityLevelCode) {
                case "001": //�����
                    if (!emp.DepartmentCode.Equals(dep.DepartmentCode) &&
                        !emp.DepartmentCode1.Equals(dep.DepartmentCode) &&
                        !emp.DepartmentCode2.Equals(dep.DepartmentCode) &&
                        !emp.DepartmentCode3.Equals(dep.DepartmentCode)) {
                        ModelState.AddModelError("", "�w�肳�ꂽ����̍݌ɂ��Q�Ƃ��錠��������܂���");
                        return View("PartsStockCriteriaDialog", list);
                    }
                    break;
                case "002": //���ƕ���
                    if (!emp.Department1.OfficeCode.Equals(dep.OfficeCode) &&
                        !(emp.AdditionalDepartment1!=null && emp.AdditionalDepartment1.OfficeCode.Equals(dep.OfficeCode)) &&
                        !(emp.AdditionalDepartment2!=null && emp.AdditionalDepartment2.OfficeCode.Equals(dep.OfficeCode)) &&
                        !(emp.AdditionalDepartment3!=null && emp.AdditionalDepartment3.OfficeCode.Equals(dep.OfficeCode)))
                    {
                        ModelState.AddModelError("", "�w�肳�ꂽ����̍݌ɂ��Q�Ƃ��錠��������܂���");
                        return View("PartsStockCriteriaDialog", list);
                    }
                    break;
                case "003": //��Г�
                    if (!emp.Department1.Office.CompanyCode.Equals(dep.Office.CompanyCode) &&
                        !(emp.AdditionalDepartment1!=null && emp.AdditionalDepartment1.Office!=null && !emp.AdditionalDepartment1.Office.CompanyCode.Equals(dep.Office.CompanyCode)) &&
                        !(emp.AdditionalDepartment2!=null && emp.AdditionalDepartment2.Office!=null && !emp.AdditionalDepartment2.Office.CompanyCode.Equals(dep.Office.CompanyCode)) &&
                        !(emp.AdditionalDepartment3!=null && emp.AdditionalDepartment3.Office!=null && !emp.AdditionalDepartment3.Office.CompanyCode.Equals(dep.Office.CompanyCode))
                        ) {
                        ModelState.AddModelError("", "�w�肳�ꂽ����̍݌ɂ��Q�Ƃ��錠��������܂���");
                        return View("PartsStockCriteriaDialog", list);
                    }
                    break;
                case "004": //ALL
                    break;

            }

            // �������ʃ��X�g�̎擾
            if (criteriaInit) {
                ViewData["InstallableFlag"] = false;
                return View("PartsStockCriteriaDialog", list);
            } else {
                if (string.IsNullOrEmpty(form["DepartmentCode"])) {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", "����R�[�h"));
                    return View("PartsStockCriteriaDialog", list);
                }
                list = GetPartsSearchResultList(form);
            }

            // ���i�݌Ɍ�����ʂ̕\��
            return View("PartsStockCriteriaDialog", list);
        }

        /// <summary>
        /// �݌Ƀf�[�^�ۑ�����
        /// </summary>
        /// <param name="form"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano ���i�݌Ɂi���i�݌ɂ̏C���j#3779 �V�K�쐬
        /// </history>
        private ActionResult DataSave(FormCollection form, EntitySet<CommonPartsStock> line)
        {
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                //------------------------------------------------------
                //inventoryStockCar�e�[�u���̃��R�[�h�X�V
                //------------------------------------------------------
                List<PartsStock> psList = new List<PartsStock>();

                for (int k = 0; k < line.Count; k++)
                {
                    PartsStock rec = new PartsStockDao(db).GetByKey(line[k].PartsNumber, line[k].LocationCode, true);

                    if (rec != null)
                    {
                        rec.Quantity = line[k].Quantity;
                        rec.ProvisionQuantity = line[k].ProvisionQuantity;
                        rec.DelFlag = "0";                                      //�폜�t���O�͏�ɗL���ɂ��Ă���

                        rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        rec.LastUpdateDate = DateTime.Now;
                    }
                    else
                    {
                        PartsStock partsstock = new PartsStock();
                        partsstock.PartsNumber = line[k].PartsNumber;                                    //���i�ԍ�
                        partsstock.LocationCode = line[k].LocationCode;                                  //���P�[�V�����R�[�h
                        partsstock.Quantity = line[k].Quantity;                                          //����
                        partsstock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    //�쐬��
                        partsstock.CreateDate = DateTime.Now;                                                   //�쐬��
                        partsstock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;       //�ŏI�X�V��
                        partsstock.LastUpdateDate = DateTime.Now;                                               //�ŏI�X�V��
                        partsstock.DelFlag = "0";                                                               //�폜�t���O
                        partsstock.ProvisionQuantity = line[k].ProvisionQuantity;                               //�����ϐ�

                        psList.Add(partsstock);
                    }

                    //�ۑ���͐V�K�쐬�t���O�𗎂Ƃ�
                    if (line[k].NewRecFlag.Equals(true))
                    {
                        line[k].NewRecFlag = false;
                    }
                }

                db.PartsStock.InsertAllOnSubmit(psList);

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException cfe)
                    {
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(cfe, PROC_NAME_DATAEDIT, FORM_NAME, "");
                            ts.Dispose();
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_DATAEDIT, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�ۑ�"));
                            ts.Dispose();
                            return View("PartsStockDataEdit", line.ToList());
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME_DATAEDIT, FORM_NAME, "");
                            ts.Dispose();
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME_DATAEDIT, FORM_NAME, "");
                        ts.Dispose();
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }

            ModelState.AddModelError("", "�ۑ����܂���");

            return View("PartsStockDataEdit", line.ToList());
        }

        /// <summary>
        /// ���i���X�g����������
        /// </summary>
        /// <param name="form">�t�H�[�����͒l</param>
        /// <returns>���i�f�[�^���X�g</returns>
        private PaginatedList<GetPartsStockForDialogResult> GetPartsSearchResultList(FormCollection form) {
            
            PartsStockSearchCondition condition = new PartsStockSearchCondition();
            condition.MakerCode = form["MakerCode"];
            condition.MakerName = form["MakerName"];
            condition.PartsNumber = form["PartsNumber"];
            condition.PartsNameJp = form["PartsNameJp"];
            condition.CarBrandName = form["CarBrandName"];
            condition.CarBrandCode = form["CarBrandCode"];
            condition.DepartmentCode = form["DepartmentCode"];
            condition.SupplierCode = form["SupplierCode"];

            return new PartsDao(db).GetListByConditionForDialog(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���i�ԍ����畔�i�݌ɏڍ׏����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���i�ԍ�</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2018/05/14 arc yano #3880 ���㌴���v�Z�y�ђI���]���@�̕ύX �P���̎擾���dbo.PartsAverageCsot��dbo.PartsMovingAverageCost�ɕύX
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// 2015/10/28 arc yano #3289 �T�[�r�X�`�[ �����敪���擾���鏈���̒ǉ�
        /// </history>
        public ActionResult GetMasterDetail(string code, string departmentCode) {
            if (Request.IsAjaxRequest()) {

                //Add 2016/08/13 arc yano #3596
                //����R�[�h����g�p�q�ɂ����o��
                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, departmentCode);

                decimal quantity = new PartsStockDao(db).GetStockQuantity(code, (dWarehouse != null ? dWarehouse.WarehouseCode : ""));

                Dictionary<string, string> retParts = new Dictionary<string, string>();

                //Mod 2018/05/14 arc yano #3880
                PartsMovingAverageCost condition = new PartsMovingAverageCost();
                condition.CompanyCode = ((Employee)Session["Employee"]).Department1.Office.CompanyCode;
                condition.PartsNumber = code;
                PartsMovingAverageCost averageCost = new PartsMovingAverageCostDao(db).GetByKey(condition);
                
                /*
                    PartsAverageCost condition = new PartsAverageCost();
                    condition.CompanyCode = ((Employee)Session["Employee"]).Department1.Office.CompanyCode;
                    condition.PartsNumber = code;
                    PartsAverageCost averageCost = new PartsAverageCostDao(db).GetByKey(condition);
                */
                retParts.Add("Quantity", quantity.ToString());
                Parts parts = new PartsDao(db).GetByKey(code);
                if (parts != null) {
                    retParts.Add("PartsNumber", code);
                    retParts.Add("Price", parts.Price.ToString());
                    retParts.Add("PartsName", string.IsNullOrEmpty(parts.PartsNameJp) ? parts.PartsNameEn : parts.PartsNameJp);
                    retParts.Add("GenuineType", parts.GenuineType ?? "");   //Add 2015/10/28 arc yano #3289
                    if (averageCost != null) {
                        retParts.Add("Cost", averageCost.Price.Value.ToString());
                    } else {
                        //Mod 2016/07/14 arc kashiwada #3619
                        //retParts.Add("Cost", "0");
                        // �V�K�o�^���i�������͈ړ����ϒP�����������i�̏ꍇ�A���i�}�X�^�̌�����\������
                        if (parts.Cost != null)
                        {
                            retParts.Add("Cost", parts.Cost.Value.ToString());
                        }
                        else
                        {
                            retParts.Add("Cost", "0");
                        }

                    }
                }
                return Json(retParts);
            }
            return new EmptyResult();
        }


        /// <summary>
        /// ���i�ԍ��ƃ��P�[�V�����R�[�h���畔�i�݌ɏڍ׏����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���i�ԍ�</param>
        /// <param name="location">���P�[�V�����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>2016/06/20 arc yano #3582</history>
        public ActionResult GetMaster(string partsNumber,string location) {
            if (Request.IsAjaxRequest()) {
                Dictionary<string, string> retParts = new Dictionary<string, string>();
                PartsStock stock = new PartsStockDao(db).GetByKey(partsNumber, location);
                
                retParts.Add("Quantity", stock!=null ? ((stock.Quantity ?? 0) - (stock.ProvisionQuantity ?? 0)).ToString() : "0");  //Mod 2016/06/20 arc yano #3582
                Parts parts = new PartsDao(db).GetByKey(partsNumber);
                if (parts != null) {
                    retParts.Add("PartsNumber", parts.PartsNumber);
                    retParts.Add("PartsNameJp", parts.PartsNameJp);
                    retParts.Add("PartsNameEn", parts.PartsNameEn);
                    //retParts.Add("Cost", parts.Cost.ToString());
                }
                return Json(retParts);
            }
            return new EmptyResult();
        }

         #region �R���g���[���̗L������
        /// <summary>
        /// �R���g���[���̗L��������Ԃ�Ԃ��B
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano #3779 ���i�݌Ɂi���i�݌ɂ̏C���j�f�[�^�ҏW
        /// </history>
        private void GetViewResult(FormCollection form)
        {
            Employee loginUser = (Employee)Session["Employee"];
            string securityLevel = loginUser.SecurityRole != null ? loginUser.SecurityRole.SecurityLevelCode : "";
            //�Z�L�����e�B���x����4�łȂ����[�U�́A�{�^���������s�ɂ���B
            if (!securityLevel.Equals("004"))
            {
                ViewData["ButtonEnabled"] = false;
            }
            else
            {
                ViewData["ButtonEnabled"] = true;
            }

            //Add 2017/07/18 arc yano #3779
            //�f�[�^�ҏW�{�^���\���E��\�������@�����̂��郆�[�U�̂݁i����̓V�X�e���ۃ��[�U�j�\��
            ApplicationRole rec = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "StockDataEdit");

            if (rec != null && rec.EnableFlag.Equals(true))
            {
                ViewData["EditButtonVisible"] = true;
            }
            else
            {
                ViewData["EditButtonVisible"] = false;
            }

            return;
        }
        #endregion

        #region ���i�݌ɍs�ǉ��E�폜
        /// <summary>
        /// ���i�݌ɍs�ǉ�
        /// </summary>
        /// <param name="form">�t�H�[��</param>
        /// <param name="line">���i�݌ɖ���</param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano #3779 ���i�݌Ɂi���i�݌ɂ̏C���j �V�K�쐬
        /// </history>
        private ActionResult AddLine(FormCollection form, EntitySet<CommonPartsStock> line)
        {
            if (line == null)
            {
                line = new EntitySet<CommonPartsStock>();
            }

            CommonPartsStock rec = new CommonPartsStock();

            //���ʁA�����ϐ��̏�����
            rec.Quantity = 0;
            rec.ProvisionQuantity = 0;

            rec.NewRecFlag = true;

            line.Add(rec);

            form["EditFlag"] = "true";

            SetComponent(form);

            return View("PartsStockDataEdit", line.ToList());
        }

        /// <summary>
        /// ���i�݌Ƀf�[�^�s�폜
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="line">���׃f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano #3779 ���i�݌Ɂi���i�݌ɂ̏C���j �V�K�쐬
        /// </history>
        private ActionResult DelLine(FormCollection form, EntitySet<CommonPartsStock> line)
        {
            ModelState.Clear();

            int targetId = int.Parse(form["DelLine"]);

            //---------------------------------------------------
            //�T�[�r�X�`�[�ň����ς̏ꍇ�͍폜�ł��Ȃ��悤�ɂ���
            //---------------------------------------------------

            //����q�Ƀ}�X�^����q�ɃR�[�h�̊���o��
            DepartmentWarehouse condition = new DepartmentWarehouse();

            condition.WarehouseCode = new LocationDao(db).GetByKey(line[targetId].LocationCode).WarehouseCode;

            List<DepartmentWarehouse> dwList = new DepartmentWarehouseDao(db).GetByCondition(condition);

            //�T�[�r�X�`�[�̑������ϐ����Z�o
            decimal svProvisionQuantity = 0m;
            foreach (var b in dwList)
            {
                svProvisionQuantity += new ServiceSalesOrderDao(db).GetPartsProvisionQuantityByPartsNumber(b.DepartmentCode, line[targetId].PartsNumber);
            }

            //�q�ɓ��ɑ��݂���݌ɂ̈����ϐ����Z�o
            decimal stcProvisionQuantity = 0m;

            stcProvisionQuantity = new PartsStockDao(db).GetListByWarehouse(condition.WarehouseCode, line[targetId].PartsNumber, false, true).Where(x => !x.LocationCode.Equals(line[targetId].LocationCode)).AsQueryable().Sum(x => x.ProvisionQuantity ?? 0);

            if (stcProvisionQuantity < svProvisionQuantity)
            {
                ModelState.AddModelError("", "�T�[�r�X�`�[�ň�������Ă��鐔�ʂ̍��v��菭�Ȃ��Ȃ邽�߁A�폜�ł��܂���");

                SetComponent(form);

                return View("PartsStockDataEdit", line.ToList());
            }

            using (TransactionScope ts = new TransactionScope())
            {
                if (line[targetId].NewRecFlag.Equals(false))
                {
                    PartsStock rec = new PartsStockDao(db).GetByKey(line[targetId].PartsNumber, line[targetId].LocationCode);

                    if (rec != null)
                    {
                        rec.DelFlag = "1";          //�폜�t���O�𗧂Ă�
                        rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        rec.LastUpdateDate = DateTime.Now;
                    }
                }
                try
                {
                    //�G���e�B�e�B�̍폜����
                    line.RemoveAt(targetId);

                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(se, PROC_NAME_DELLINE, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME_DELLINE, FORM_NAME, "");
                    ts.Dispose();
                }
            }
         
            SetComponent(form);

            return View("PartsStockDataEdit", line.ToList());
        }

        #endregion


        //Add 2014/12/15 arc yano IPO�Ή�(���i����) �������Ή�
       /// <summary>
       /// ���������ǂ������擾����B(Ajax��p�j
       /// </summary>
       /// <param name="processType">�������</param>
       /// <returns>�A�C�h�����O���</returns>
       public ActionResult GetProcessed(string processType)
       {
           if (Request.IsAjaxRequest())
           {
               Dictionary<string, string> retParts = new Dictionary<string, string>();
                
               retParts.Add("ProcessedTime", "�A�C�h�����O���c");
                
               return Json(retParts);
           }
           return new EmptyResult();
       }
    }
}
