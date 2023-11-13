using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using Crms.Models;              //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using System.Data.Linq;         //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using System.Data.SqlClient;    //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    //Add 2015/01/14 arc yano ���̃R���g���[���Ɠ������A�t�B���^����(��O�A�Z�L�����e�B�A�o�̓L���b�V��)��ǉ�
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarReservationCancelController : Controller
    {

        //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "��������";   // ��ʖ�
        private static readonly string PROC_NAME = "�݌ɂɖ߂�"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarReservationCancelController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// ������ʕ\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria() {
            return Criteria(new FormCollection(), null);
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form,List<CarSalesHeader> item) {

            if (form["ActionType"]!=null && form["ActionType"].Equals("update")) {
                // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
                db = new CrmsLinqDataContext();
                db.Log = new OutputWriter();

                string prefix = string.Format("item[{0}].",form["RowId"]);
                string locationCode = form[prefix+"LocationCode"];
                if (string.IsNullOrEmpty(locationCode)) {
                    //ModelState.AddModelError(prefix + "LocationCode", MessageUtils.GetMessage("E0001", "���P�[�V����"));
                    PaginatedList<CarSalesHeader> model = GetSearchResult(form);
                    return View("CarReservationCancelCriteria", model);
                }
                string salesCarNumber = form[prefix+"SalesCarNumber"];
                SalesCar target = new SalesCarDao(db).GetByKey(salesCarNumber);
                target.LocationCode = locationCode;
                target.CarStatus = "001";
                target.LastUpdateDate = DateTime.Now;
                target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        // �f�[�^����̎��s�E�R�~�b�g
                        db.SubmitChanges();
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, "");
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
            }

            ModelState.Clear();
            PaginatedList<CarSalesHeader> list = GetSearchResult(form);
            return View("CarReservationCancelCriteria", list);
        }
        private PaginatedList<CarSalesHeader> GetSearchResult(FormCollection form) {
            PaginatedList<CarSalesHeader> list = new CarSalesOrderDao(db).GetCancelList(int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            foreach (var header in list) {
                header.OriginalCarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber.Substring(0, header.SlipNumber.Length - 2));
                header.LocationCode = form[string.Format("item[{0}].LocationCode", header.SalesCarNumber)];
                header.LocationName = form[string.Format("item[{0}].LocationName", header.SalesCarNumber)];
            }
            return list;
        }
    }
}
