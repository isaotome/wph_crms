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
        /// Validationチェック共通関数
        /// String：必須
        /// DateTime：属性、必須
        /// Decimal：属性、フォーマット、必須
        /// Int：属性、必須
        /// </summary>
        /// <param name="fieldName">フィールド名(En)</param>
        /// <param name="fieldNameJp">フィールド名(Jp)</param>
        /// <param name="model">モデル</param>
        /// <param name="IsNullable">必須チェックの有無</param>
        public void CommonValidate(string fieldName, string fieldNameJp,object model,bool isNotNull) {
            
            //Reflectionでフィールドの属性と値を取得
            PropertyInfo property = model.GetType().GetProperty(fieldName);
            object value = property.GetValue(model, null);

            //エラーの初期化
            if (ModelState[fieldName]!=null && ModelState[fieldName].Errors.Count > 1) {
                ModelState[fieldName].Errors.RemoveAt(0);
            }
            
            //フィールドの型毎に処理
            if (property.ToString().IndexOf("System.Decimal") >= 0) {

                //(走行距離だけ特別対応）
                if (fieldName.Contains("Mileage")) {

                    //必須チェック
                    if (isNotNull && (!ModelState.IsValidField(fieldName) || value == null)) {
                        ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0002", new string[] { fieldNameJp, "正の整数10桁以内かつ小数2桁以内" }));

                        //属性・フォーマットチェック
                    } else if (!ModelState.IsValidField(fieldName) || (value != null &&
                                (!Regex.IsMatch(value.ToString(), @"^\d{1,10}\.\d{1,2}$") && !Regex.IsMatch(value.ToString(), @"^\d{1,10}$")))) {
                        ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0004", new string[] { fieldNameJp, "正の整数10桁以内かつ小数2桁以内" }));
                    }

                //金額
                } else {
                    //必須チェック
                    if (isNotNull && (!ModelState.IsValidField(fieldName) || value == null)) {
                        ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0002", new string[] { fieldNameJp, "10桁以内の整数のみ" }));
                        //属性・フォーマットチェック
                    } else if (!ModelState.IsValidField(fieldName) || (value != null && !Regex.IsMatch(value.ToString(), @"^[-]?\d{1,10}$"))) {
                        ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0004", new string[] { fieldNameJp, "10桁以内の整数のみ" }));
                    }
                }
            //文字列
            } else if (property.ToString().IndexOf("String") >= 0) {

                //必須チェック
                if (isNotNull && (value== null || (value!=null && string.IsNullOrEmpty(value.ToString())))) {
                        ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0001", fieldNameJp));
                }

            //日付
            } else if (property.ToString().IndexOf("DateTime") >= 0) {
                
                //必須チェック
                if (isNotNull && (!ModelState.IsValidField(fieldName) || value==null)) {
                        ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0003", fieldNameJp));

                //属性・フォーマットチェック
                } else if (!ModelState.IsValidField(fieldName)) {
                    ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0005", fieldNameJp));
                }
            //GUID
            } else if (property.ToString().IndexOf("Guid") >= 0) {
                if (isNotNull && (!ModelState.IsValidField(fieldName) || value == null)) {
                    ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0001", fieldNameJp));
                }
            //数値
            } else if (property.ToString().IndexOf("Int") >= 0) {
                if (isNotNull && (!ModelState.IsValidField(fieldName) || value == null))
                {
                    ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0001", fieldNameJp));
                    // DEL 2014/06/09 arc uchida ModelStateの初期化処理の削除
                    //ModelState[fieldName].Errors.RemoveAt(0);
                }
            } 
        }
    }
}
