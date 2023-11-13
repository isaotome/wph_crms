using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// グレードマスタアクセスクラス
    ///   グレードマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CarGradeDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CarGradeDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// グレードマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="carGradeCode">グレードコード</param>
        /// <returns>グレードマスタデータ(1件)</returns>
        public CarGrade GetByKey(string carGradeCode)
        {
            // グレードデータの取得
            CarGrade carGrade =
                (from a in db.CarGrade
                 where a.CarGradeCode.Equals(carGradeCode)
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (carGrade != null)
            {
                carGrade = EditModel(carGrade);
            }

            // グレードデータの返却
            return carGrade;
        }

        /// <summary>
        /// グレードマスタデータ検索
        /// </summary>
        /// <param name="carGradeCondition">グレード検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>グレードマスタデータ検索結果</returns>
        public PaginatedList<CarGrade> GetListByCondition(CarGrade carGradeCondition, int? pageIndex, int? pageSize)
        {
            string carGradeCode = carGradeCondition.CarGradeCode;
            string carGradeName = carGradeCondition.CarGradeName;
            string carCode = null;
            try { carCode = carGradeCondition.Car.CarCode; } catch (NullReferenceException) { }
            string carName = null;
            try { carName = carGradeCondition.Car.CarName; } catch (NullReferenceException) { }
            string carBrandCode = null;
            try { carBrandCode = carGradeCondition.Car.Brand.CarBrandCode; } catch (NullReferenceException) { }
            string carBrandName = null;
            try { carBrandName = carGradeCondition.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
            string carClassCode = null;
            try { carClassCode = carGradeCondition.CarClass.CarClassCode; } catch (NullReferenceException) { }
            string carClassName = null;
            try { carClassName = carGradeCondition.CarClass.CarClassName; } catch (NullReferenceException) { }
            string delFlag = carGradeCondition.DelFlag;
            string companyCode = null;
            try { companyCode = carGradeCondition.Car.Brand.CompanyCode; } catch (NullReferenceException) { }
            string classificationTypeNumber = carGradeCondition.ClassificationTypeNumber;
            string modelSpecificateNumber = carGradeCondition.ModelSpecificateNumber;
            string modelName = carGradeCondition.ModelName;

            // グレードデータの取得
            IOrderedQueryable<CarGrade> carGradeList =
                    from a in db.CarGrade
                    where (string.IsNullOrEmpty(carGradeCode) || a.CarGradeCode.Contains(carGradeCode))
                    && (string.IsNullOrEmpty(carGradeName) || a.CarGradeName.Contains(carGradeName))
                    && (string.IsNullOrEmpty(carCode) || a.CarCode.Contains(carCode))
                    && (string.IsNullOrEmpty(carName) || a.Car.CarName.Contains(carName))
                    && (string.IsNullOrEmpty(carBrandCode) || a.Car.CarBrandCode.Contains(carBrandCode))
                    && (string.IsNullOrEmpty(carBrandName) || a.Car.Brand.CarBrandName.Contains(carBrandName))
                    && (string.IsNullOrEmpty(carClassCode) || a.CarClassCode.Contains(carClassCode))
                    && (string.IsNullOrEmpty(carClassName) || a.CarClass.CarClassName.Contains(carClassName))
                    && (string.IsNullOrEmpty(companyCode) || a.Car.Brand.CompanyCode.Equals(companyCode))
                    && (string.IsNullOrEmpty(classificationTypeNumber) || a.ClassificationTypeNumber.Contains(classificationTypeNumber))
                    && (string.IsNullOrEmpty(modelSpecificateNumber) || a.ModelSpecificateNumber.Contains(modelSpecificateNumber))
                    && (string.IsNullOrEmpty(modelName) || a.ModelName.Equals(modelName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.Car.CarBrandCode, a.CarCode, a.ModelYear descending, a.CarClassCode, a.CarGradeCode
                    select a;

            // ページング制御情報を付与したグレードデータの返却
            PaginatedList<CarGrade> ret = new PaginatedList<CarGrade>(carGradeList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }

        public PaginatedList<CarGrade> GetListByConditionForDialog(CarGrade carGradeCondition, int? pageIndex, int? pageSize) {
            string carCode = null;
            try { carCode = carGradeCondition.Car.CarCode; } catch (NullReferenceException) { }
            string carBrandCode = null;
            try { carBrandCode = carGradeCondition.Car.Brand.CarBrandCode; } catch (NullReferenceException) { }
            string delFlag = carGradeCondition.DelFlag;
            string classificationTypeNumber = carGradeCondition.ClassificationTypeNumber;
            string modelSpecificateNumber = carGradeCondition.ModelSpecificateNumber;
            string modelName = carGradeCondition.ModelName;

            // グレードデータの取得
            IOrderedQueryable<CarGrade> carGradeList =
                    from a in db.CarGrade
                    where (string.IsNullOrEmpty(carCode) || a.CarCode.Equals(carCode))
                    && (string.IsNullOrEmpty(carBrandCode) || a.Car.CarBrandCode.Equals(carBrandCode))
                    && (string.IsNullOrEmpty(classificationTypeNumber) || a.ClassificationTypeNumber.Contains(classificationTypeNumber))
                    && (string.IsNullOrEmpty(modelSpecificateNumber) || a.ModelSpecificateNumber.Contains(modelSpecificateNumber))
                    && (string.IsNullOrEmpty(modelName) || a.ModelName.Equals(modelName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.Car.CarBrandCode, a.CarCode, a.ModelYear descending, a.CarClassCode, a.CarGradeCode
                    select a;

            // ページング制御情報を付与したグレードデータの返却
            PaginatedList<CarGrade> ret = new PaginatedList<CarGrade>(carGradeList, pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }

        /// <summary>
        /// 車種コードから型式リストを取得する
        /// </summary>
        /// <param name="carCode">車種コード</param>
        /// <returns></returns>
        public List<string> GetModelNameList(string carCode) {
            var query = from a in db.CarGrade
                        where a.CarCode.Equals(carCode)
                        && a.ModelName!=null && !a.ModelName.Equals(string.Empty)
                        select a.ModelName;
            return query.Distinct().ToList();
        
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="carGrade">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private CarGrade EditModel(CarGrade carGrade)
        {
            // 内部コード項目の名称情報取得
            carGrade.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, carGrade.DelFlag);

            // 出口
            return carGrade;
        }
    }
}
