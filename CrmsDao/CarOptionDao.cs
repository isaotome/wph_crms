using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// オプションマスタアクセスクラス
    ///   オプションマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CarOptionDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CarOptionDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// オプションマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="carOptionCode">オプションコード</param>
        /// <returns>オプションマスタデータ(1件)</returns>
        public CarOption GetByKey(string carOptionCode)
        {
            // オプションデータの取得
            CarOption carOption =
                (from a in db.CarOption
                 where a.CarOptionCode.Equals(carOptionCode)
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (carOption != null)
            {
                carOption = EditModel(carOption);
            }

            // オプションデータの返却
            return carOption;
        }

        /// <summary>
        /// オプションマスタデータ検索
        /// </summary>
        /// <param name="carOptionCondition">オプション検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>オプションマスタデータ検索結果</returns>
        public PaginatedList<GetCarOptionMaster_Result> GetListByCondition(CarOption carOptionCondition, int? pageIndex, int? pageSize)
        {
            string carOptionCode = carOptionCondition.CarOptionCode;
            string carOptionName = carOptionCondition.CarOptionName;
            string makerCode = null;
            try { makerCode = carOptionCondition.Maker.MakerCode; } catch (NullReferenceException) { }
            string makerName = null;
            try { makerName = carOptionCondition.Maker.MakerName; } catch (NullReferenceException) { }
            string delFlag = carOptionCondition.DelFlag;
            string requiredFlag = carOptionCondition.RequiredFlag;
            string carcode = carOptionCondition.CarGradeCode;
            string actionFlag = carOptionCondition.ActionFlag;

            // オプションデータの取得
            var carOptionList = db.GetCarOptionMaster(makerCode, makerName, carOptionCode, carOptionName, carcode, requiredFlag, delFlag, actionFlag);

            List<GetCarOptionMaster_Result> RetList = new List<GetCarOptionMaster_Result>();

            foreach (var Ret in carOptionList)
            {
                GetCarOptionMaster_Result CarOp = new GetCarOptionMaster_Result();
                CarOp.CarOptionCode = Ret.CarOptionCode;
                CarOp.MakerCode = Ret.MakerCode;
                CarOp.MakerName = Ret.MakerName;
                CarOp.CarOptionName = Ret.CarOptionName;
                CarOp.DelFlag = Ret.DelFlag;
                CarOp.OptionType = Ret.OptionType;
                CarOp.CarGradeName = Ret.CarGradeName;
                CarOp.RequiredFlag = Ret.RequiredFlag;
                CarOp.DelFlag = Ret.DelFlag;

                RetList.Add(CarOp);
            }

            // ページング制御情報を付与したオプションデータの返却
            PaginatedList<GetCarOptionMaster_Result> ret = new PaginatedList<GetCarOptionMaster_Result>(RetList.AsQueryable<GetCarOptionMaster_Result>(), pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel2(ret[i]);
            }

            // 出口
            return ret;
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="carOption">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private CarOption EditModel(CarOption carOption)
        {
            // 内部コード項目の名称情報取得
            carOption.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, carOption.DelFlag);

            // 出口
            return carOption;
        }

        /// <summary>
        /// モデルデータの編集(ストプロ用)
        /// </summary>
        /// <param name="carOption">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private GetCarOptionMaster_Result EditModel2(GetCarOptionMaster_Result carOption)
        {
            // 内部コード項目の名称情報取得
            carOption.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, carOption.DelFlag);

            // 出口
            return carOption;
        }

        /// <summary>
        /// 車種に対して必須になるオプションを取得する
        /// </summary>
        /// <param name="carOptionCondition">オプション検索条件</param>
        /// <param name="CarCode">車種コード</param>
        /// <param name="MakerCode">メーカーコード</param>
        /// <returns>オプションマスタデータ検索結果</returns>
        public List<GetCarOptionSetListResult> GetRequiredOptionByCarCode(string CarGradeCode, string MakerCode)
        {
            var QueryRet = db.GetCarOptionSetList(CarGradeCode, MakerCode);

            List<GetCarOptionSetListResult> OptionSetList = new List<GetCarOptionSetListResult>();

            foreach (var ret in QueryRet)
            {
                GetCarOptionSetListResult Option = new GetCarOptionSetListResult();
                Option.CarOptionCode = ret.CarOptionCode;
                Option.CarOptionName = ret.CarOptionName;
                Option.OptionType = ret.OptionType;
                Option.MakerCode = ret.MakerCode;
                Option.CarGradeCode = ret.CarGradeCode;
                Option.Cost = ret.Cost;
                Option.SalesPrice = ret.SalesPrice;
                OptionSetList.Add(Option);
            }

            return OptionSetList;

        }

    }
}
