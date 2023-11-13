using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Text.RegularExpressions;
using System.Reflection;
namespace Crms.Controllers
{
    public class InheritedController : Controller
    {
        /// <summary>
        /// Validation�`�F�b�N���ʊ֐�
        /// String�F�K�{
        /// DateTime�F�����A�K�{
        /// Decimal�F�����A�t�H�[�}�b�g�A�K�{
        /// Int�F�����A�K�{
        /// </summary>
        /// <param name="fieldName">�t�B�[���h��(En)</param>
        /// <param name="fieldNameJp">�t�B�[���h��(Jp)</param>
        /// <param name="model">���f��</param>
        /// <param name="IsNullable">�K�{�`�F�b�N�̗L��</param>
        public void CommonValidate(string fieldName, string fieldNameJp,object model,bool isNotNull) {
            
            //Reflection�Ńt�B�[���h�̑����ƒl���擾
            PropertyInfo property = model.GetType().GetProperty(fieldName);
            object value = property.GetValue(model, null);

            //�G���[�̏�����
            if (ModelState[fieldName]!=null && ModelState[fieldName].Errors.Count > 1) {
                ModelState[fieldName].Errors.RemoveAt(0);
            }
            
            //�t�B�[���h�̌^���ɏ���
            if (property.ToString().IndexOf("System.Decimal") >= 0) {

                //(���s�����������ʑΉ��j
                if (fieldName.Contains("Mileage")) {

                    //�K�{�`�F�b�N
                    if (isNotNull && (!ModelState.IsValidField(fieldName) || value == null)) {
                        ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0002", new string[] { fieldNameJp, "���̐���10���ȓ�������2���ȓ�" }));

                        //�����E�t�H�[�}�b�g�`�F�b�N
                    } else if (!ModelState.IsValidField(fieldName) || (value != null &&
                                (!Regex.IsMatch(value.ToString(), @"^\d{1,10}\.\d{1,2}$") && !Regex.IsMatch(value.ToString(), @"^\d{1,10}$")))) {
                        ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0004", new string[] { fieldNameJp, "���̐���10���ȓ�������2���ȓ�" }));
                    }

                //���z
                } else {
                    //�K�{�`�F�b�N
                    if (isNotNull && (!ModelState.IsValidField(fieldName) || value == null)) {
                        ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0002", new string[] { fieldNameJp, "10���ȓ��̐����̂�" }));
                        //�����E�t�H�[�}�b�g�`�F�b�N
                    } else if (!ModelState.IsValidField(fieldName) || (value != null && !Regex.IsMatch(value.ToString(), @"^[-]?\d{1,10}$"))) {
                        ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0004", new string[] { fieldNameJp, "10���ȓ��̐����̂�" }));
                    }
                }
            //������
            } else if (property.ToString().IndexOf("String") >= 0) {

                //�K�{�`�F�b�N
                if (isNotNull && (value== null || (value!=null && string.IsNullOrEmpty(value.ToString())))) {
                        ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0001", fieldNameJp));
                }

            //���t
            } else if (property.ToString().IndexOf("DateTime") >= 0) {
                
                //�K�{�`�F�b�N
                if (isNotNull && (!ModelState.IsValidField(fieldName) || value==null)) {
                        ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0003", fieldNameJp));

                //�����E�t�H�[�}�b�g�`�F�b�N
                } else if (!ModelState.IsValidField(fieldName)) {
                    ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0005", fieldNameJp));
                }
            //GUID
            } else if (property.ToString().IndexOf("Guid") >= 0) {
                if (isNotNull && (!ModelState.IsValidField(fieldName) || value == null)) {
                    ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0001", fieldNameJp));
                }
            //���l
            } else if (property.ToString().IndexOf("Int") >= 0) {
                if (isNotNull && (!ModelState.IsValidField(fieldName) || value == null))
                {
                    ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0001", fieldNameJp));
                    // DEL 2014/06/09 arc uchida ModelState�̏����������̍폜
                    //ModelState[fieldName].Errors.RemoveAt(0);
                }
            } 
        }
    }
}
