using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 部品マスタアクセスクラス
    ///   部品マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class PartsDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public PartsDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 部品マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="partsCode">部品コード</param>
        /// <returns>部品マスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
        public Parts GetByKey(string partsCode, bool includeDeleted = false)
        {
            // 部品データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            Parts parts =
                (from a in db.Parts
                 where a.PartsNumber.Equals(partsCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (parts != null)
            {
                parts = EditModel(parts);
            }

            // 部品データの返却
            return parts;
        }

        /// <summary>
        /// 部品マスタデータ検索
        /// </summary>
        /// <param name="condition">部品検索条件</param>
        /// <returns>検索結果</returns>
        /// <history>
        ///  2018/05/22 arc yano #3887 Excel取込(部品価格改定)
        /// </history>
        public List<Parts> GetListByCondition(Parts condition)
        {
            var query =
                   from a in db.Parts
                   where (string.IsNullOrEmpty(condition.PartsNumber) || a.PartsNumber.Contains(condition.PartsNumber))
                   && (string.IsNullOrEmpty(condition.GenuineType) || a.GenuineType.Equals(condition.GenuineType))
                   && (string.IsNullOrEmpty(condition.PartsNameJp) || a.PartsNameJp.Contains(condition.PartsNameJp))
                   && (string.IsNullOrEmpty(condition.MakerCode) || a.MakerCode.Contains(condition.MakerCode))
                   && (string.IsNullOrEmpty(condition.DelFlag) || a.MakerCode.Contains(condition.DelFlag))
                   && (string.IsNullOrEmpty(condition.NonInventoryFlag) || a.MakerCode.Contains(condition.NonInventoryFlag))
                   select a;

            return query.ToList();
        }


        /// <summary>
        /// 部品マスタデータ検索
        /// </summary>
        /// <param name="partsCondition">部品検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>部品マスタデータ検索結果</returns>
        /// <history>
        ///  2021/02/22 yano #4083 【部品マスタ検索】検索処理のパフォーマンス改善対応
        ///  2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 純正区分の検索条件追加
        /// </history>
        public PaginatedList<PartsCriteria> GetListByCondition(Parts partsCondition, int? pageIndex, int? pageSize)
        {
            string partsCode = partsCondition.PartsNumber;
            string partsName = partsCondition.PartsNameJp;
            string makerCode = null;
            try { makerCode = partsCondition.Maker.MakerCode; } catch (NullReferenceException) { }
            string makerName = null;
            try { makerName = partsCondition.Maker.MakerName; } catch (NullReferenceException) { }
            string delFlag = partsCondition.DelFlag;

            //bool installable = partsCondition.InstallableFlag ?? false; //Mod 2021/02/22 yano #4083
            string brandName = partsCondition.CarBrandName;
            string brandCode = partsCondition.CarBrandCode;

            //純正区分
            string geneineType = partsCondition.GenuineType;

            // 部品データの取得
            IQueryable<PartsCriteria> partsList;

            //Mod 2021/02/22 yano #4083
            partsList =
                from a in db.Parts
                where (string.IsNullOrEmpty(partsCode) || a.PartsNumber.Contains(partsCode))
                && (string.IsNullOrEmpty(geneineType) || a.GenuineType.Equals(geneineType)) //2015/11/09 arc yano #3291
                && (string.IsNullOrEmpty(partsName) || a.PartsNameJp.Contains(partsName) || a.PartsNameEn.Contains(partsName))
                && (string.IsNullOrEmpty(makerCode) || a.MakerCode.Contains(makerCode))
                && (string.IsNullOrEmpty(makerName) || a.Maker.MakerName.Contains(makerName))
                && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                && (string.IsNullOrEmpty(brandCode) ||
                    (from b in db.Brand
                     where b.CarBrandCode.Contains(brandCode) && b.DelFlag.Equals("0")
                     select b.MakerCode).Contains(a.MakerCode))
                && (string.IsNullOrEmpty(brandName) ||
                    (from c in db.Brand
                     where c.CarBrandName.Contains(brandName) && c.DelFlag.Equals("0")
                     select c.MakerCode).Contains(a.MakerCode))
                orderby a.MakerCode, a.PartsNumber
                select new PartsCriteria()
                {
                    PartsNumber = a.PartsNumber
                    ,
                    PartsNameJp = a.PartsNameJp
                    ,
                    MakerCode = a.MakerCode   
                    ,
                    MakerName = (a.Maker != null ? a.Maker.MakerName : "")
                    ,
                    DelFlag = a.DelFlag
                    ,
                    DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, a.DelFlag)
                };

            PaginatedList<PartsCriteria> ret = new PaginatedList<PartsCriteria>(partsList, pageIndex ?? 0, pageSize ?? 0);


            //if (installable && !string.IsNullOrEmpty(brandName)) {
            //    //取付可能部品のみ表示する場合
            //    partsList =
            //        from a in db.Parts
            //        where (string.IsNullOrEmpty(partsCode) || a.PartsNumber.Contains(partsCode))
            //        && (string.IsNullOrEmpty(geneineType) || a.GenuineType.Equals(geneineType)) //2015/11/09 arc yano #3291
            //        && (string.IsNullOrEmpty(partsName) || a.PartsNameJp.Contains(partsName) || a.PartsNameEn.Contains(partsName))
            //        && (string.IsNullOrEmpty(makerCode) || a.MakerCode.Contains(makerCode))
            //        && (string.IsNullOrEmpty(makerName) || a.Maker.MakerName.Contains(makerName))
            //        && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
            //        && (from b in db.InstallableParts 
            //            where b.Brand.CarBrandName.Contains(brandName) && !b.DelFlag.Equals("1")
            //            select b.PartsNumber).Contains(a.PartsNumber)
            //        && (string.IsNullOrEmpty(brandName) ||
            //            (from c in db.Brand
            //            where c.CarBrandName.Contains(brandName) && !c.DelFlag.Equals("1")
            //            select c.MakerCode).Contains(a.MakerCode))
            //        orderby a.MakerCode, a.PartsNumber
            //        select a;
            //} else {
            //    partsList =
            //        from a in db.Parts
            //        where (string.IsNullOrEmpty(partsCode) || a.PartsNumber.Contains(partsCode))
            //        && (string.IsNullOrEmpty(geneineType) || a.GenuineType.Equals(geneineType)) //2015/11/09 arc yano #3291
            //        && (string.IsNullOrEmpty(partsName) || a.PartsNameJp.Contains(partsName) || a.PartsNameEn.Contains(partsName))
            //        && (string.IsNullOrEmpty(makerCode) || a.MakerCode.Contains(makerCode))
            //        && (string.IsNullOrEmpty(makerName) || a.Maker.MakerName.Contains(makerName))
            //        && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
            //        && (string.IsNullOrEmpty(brandCode) ||
            //            (from b in db.Brand
            //             where b.CarBrandCode.Contains(brandCode) && b.DelFlag.Equals("0")
            //             select b.MakerCode).Contains(a.MakerCode))
            //        && (string.IsNullOrEmpty(brandName) ||
            //            (from c in db.Brand
            //             where c.CarBrandName.Contains(brandName) && c.DelFlag.Equals("0")
            //             select c.MakerCode).Contains(a.MakerCode))
            //        orderby a.MakerCode, a.PartsNumber
            //        select a;
            //}
            // ページング制御情報を付与した部品データの返却

            //PaginatedList<Parts> ret = new PaginatedList<Parts>(partsList, pageIndex ?? 0, pageSize ?? 0);

            //// 内部コード項目の名称情報取得
            //for (int i = 0; i < ret.Count; i++)
            //{
            //    ret[i] = EditModel(ret[i]);
            //}

            // 出口
            return ret;
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="parts">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Parts EditModel(Parts parts)
        {
            // 内部コード項目の名称情報取得
            parts.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, parts.DelFlag);

            // 出口
            return parts;
        }


        /// <summary>
        /// 部品在庫検索ダイアログ用検索メソッド
        /// </summary>
        /// <param name="partsCondition">部品検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>部品マスタデータ検索結果</returns>
        public PaginatedList<GetPartsStockForDialogResult> GetListByConditionForDialog(PartsStockSearchCondition Condition, int? pageIndex, int? pageSize)
        {
            var partsList = db.GetPartsStockForDialog(Condition.MakerCode,
                                                      Condition.MakerName,
                                                      Condition.CarBrandCode,
                                                      Condition.CarBrandName,
                                                      Condition.PartsNumber,
                                                      Condition.PartsNameJp,
                                                      Condition.DepartmentCode,
                                                      Condition.SupplierCode
                                                      );


            List<GetPartsStockForDialogResult> list = new List<GetPartsStockForDialogResult>();

            foreach (var Ret in partsList)
            {
                GetPartsStockForDialogResult List = new GetPartsStockForDialogResult();
                List.MakerName = Ret.MakerName;
                List.PartsNumber = Ret.PartsNumber;
                List.PartsNameJp = Ret.PartsNameJp;
                List.Quantity = Ret.Quantity;
                List.SupplierName = Ret.SupplierName;
                //リスト追加
                list.Add(List);
            }

            return new PaginatedList<GetPartsStockForDialogResult>(list.AsQueryable<GetPartsStockForDialogResult>(), pageIndex ?? 0, pageSize ?? 0);
        }



        /// <summary>
        ///  当該部品が在庫管理対象かどうかを判定する
        /// </summary>
        /// <param name="PartsNumber">部品番号</param>
        /// <history>
        /// Mod 2016/01/26 arc yano #3453 不具合修正
        /// Add 2015/10/28 arc yano #3289 部品仕入機能改善(サービス伝票入力)
        /// </history>
        /// <returns>判定結果(true:在庫管理対象 false:在庫管理対象外)</returns>
        public bool IsInventoryParts(string partsNumber)
        {
            bool ret = false;
            
            if(!string.IsNullOrWhiteSpace(partsNumber))
            {
                var query = from a in db.Parts
                            where a.PartsNumber.Equals(partsNumber)
                            && a.DelFlag.Equals("0")
                            && (a.NonInventoryFlag == null || !a.NonInventoryFlag.Equals("1"))
                            select a;

                ret = (query.Count() > 0) ? true : false;
            }

            return ret;
        }

        //Add 2015/12/15 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
        /// <summary>
        /// 部品マスタデータ取得(メーカー部品番号指定)
        /// </summary>
        /// <param name="partsCode">メーカー部品番号</param>
        /// <returns>部品マスタデータ(1件)</returns>
        /// <history>
        /// Mod 2016/03/03 arc yano #3413 部品マスタ メーカー部品番号の重複 取得項目追加(顧客コード) メーカー部品番号の検索により
        ///                               部品が複数取得できた場合は、その店舗の主要メーカーによる絞り込みを行う
        /// Mod 2016/02/26 arc yano #3432 部品仕入　LinkEntry、返品行の取込スキップ対応　　　　　　　　 
        /// Mod 2016/01/21 arc yano #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善) 課題管理表対応(No13) 純正区分追加
        /// Add 2015/12/15 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
        /// </history>
        public Parts GetByMakerPartsNumber(string MakerPartsNumber, string genuineType, string leUserCode, bool includeDeleted = false)
        {

            Parts ret = null;

            // 部品データの取得
            var parts =
                (from a in db.Parts
                 where a.MakerPartsNumber.Equals(MakerPartsNumber)
                 &&  a.GenuineType.Equals(genuineType) //Mod 2016/01/21 arc yano #3294
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                );

            // 複数の部品データが取得できた場合はその店舗の主要メーカーコードによる絞り込みを行う
            if (parts.Count() > 1)
            {
                //主要メーカーコード取得
                Department dep = new DepartmentDao(db).GetByLEUserCode(leUserCode);

                if (dep != null)
                {
                    string mainMakerCode = dep.MainMakerCode;
                    ret = parts.Where(x => x.MakerCode.Equals(mainMakerCode)).FirstOrDefault();
                }

            }
            else
            {
                ret = parts.FirstOrDefault();
            }

            // 部品データの返却
            return ret;
        }
         

    }
}
