using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;
using System.Linq.Expressions;
namespace CrmsDao
{
    public class V_CarMasterDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext"></param>
        public V_CarMasterDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }


        /// <summary>
        /// 車両データ取得
        /// </summary>
        /// <param name="condition">メーカーコード</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>車両データ</returns>
        public V_CarMaster GetByMakerkey(string MakerCode)
        {
            V_CarMaster query =
                (from a in db.V_CarMaster
                 where (MakerCode == null || a.MakerCode.Equals(MakerCode))
                 && (a.CarDelFlag == "0")
                 select a).FirstOrDefault();

            return query;
        }

        /// <summary>
        /// 車両データ取得
        /// </summary>
        /// <param name="condition">メーカーコード</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>車両データ</returns>
        public V_CarMaster GetByCarkey(string CarCode)
        {
            V_CarMaster query =
                (from a in db.V_CarMaster
                 where (CarCode == null || a.CarCode.Equals(CarCode))
                 && (a.CarDelFlag == "0")
                 select a).FirstOrDefault();

            return query;
        }



        /// <summary>
        /// 重複しないメーカーデータ取得
        /// </summary>
        /// <param name="condition">メーカーコード</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>メーカーデータ</returns>
        public List<V_CarMaster> GetPrivateListBykey(string MakerCode, string PrivateFlag = null)
        {
            var query =
                (from a in db.V_CarMaster
                 where (string.IsNullOrEmpty(MakerCode) || a.MakerCode.Equals(MakerCode))
                && (a.MakerDelFlag == "0")
                && (string.IsNullOrEmpty(PrivateFlag) || a.PrivateFlag.Equals(PrivateFlag))
                 select new { a.MakerCode, a.MakerName, a.PrivateFlag }).Distinct().OrderBy(x => x.MakerCode);

            List<V_CarMaster> CarMasterList = new List<V_CarMaster>();

            foreach (var ret in query)
            {
                V_CarMaster CarMaster = new V_CarMaster();
                CarMaster.MakerCode = ret.MakerCode;
                CarMaster.MakerName = ret.MakerName;
                CarMaster.PrivateFlag = ret.PrivateFlag;

                CarMasterList.Add(CarMaster);

            }

            return CarMasterList;
        }


        /// <summary>
        /// 重複しない車両データ取得
        /// </summary>
        /// <param name="condition">メーカーコード</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>車両データ</returns>
        public List<V_CarMaster> GetCarListBykey(string MakerCode, string PrivateFlag = null)
        {
            var query =
                (from a in db.V_CarMaster
                 where (string.IsNullOrEmpty(MakerCode) || a.MakerCode.Equals(MakerCode))
                && (a.CarDelFlag == "0")
                && (string.IsNullOrEmpty(PrivateFlag) || a.PrivateFlag.Equals(PrivateFlag))
                select new { a.MakerCode, a.CarCode, a.CarName, a.PrivateFlag }).Distinct().OrderBy(x => x.MakerCode).ThenBy(x => x.CarCode);


            List<V_CarMaster> CarMasterList = new List<V_CarMaster>();

            foreach (var ret in query)
            {
                V_CarMaster CarMaster = new V_CarMaster();
                CarMaster.CarCode = ret.CarCode;
                CarMaster.CarName = ret.CarName;
                CarMaster.PrivateFlag = ret.PrivateFlag;

                CarMasterList.Add(CarMaster);

            }

            return CarMasterList;
        }



        /// <summary>
        /// 車両データ取得
        /// </summary>
        /// <param name="condition">メーカーコード</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>車両データ</returns>
        public List<V_CarMaster> GetListBykey(string MakerCode)
        {
            var query =
                (from a in db.V_CarMaster
                 where (MakerCode == null || a.MakerCode.Equals(MakerCode))
                 && (a.CarDelFlag == "0")
                 select new { a.MakerCode, a.MakerName, a.CarCode, a.CarName }).Distinct().OrderBy(x => x.MakerCode).ThenBy(x => x.CarCode);

            List<V_CarMaster> CarMasterList = new List<V_CarMaster>();

            foreach (var ret in query)
            {
                V_CarMaster CarMaster = new V_CarMaster();

                CarMaster.MakerCode = ret.MakerCode;
                CarMaster.MakerName = ret.MakerName;
                CarMaster.CarCode = ret.CarCode;
                CarMaster.CarName = ret.CarName;
                

                CarMasterList.Add(CarMaster);

            }

            return CarMasterList;
        }

        /// <summary>
        /// 車両データ取得(全て)
        /// </summary>
        /// <param name="condition">メーカーコード</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>車両データ</returns>
        public PaginatedList<V_CarMaster> GetListByCondition(V_CarMaster V_CarMasterCondition, int? pageIndex, int? pageSize)
        {
            IQueryable<V_CarMaster> query =
                from a in db.V_CarMaster
                where (V_CarMasterCondition.MakerCode == null || a.MakerCode.Equals(V_CarMasterCondition.MakerCode))
                && (V_CarMasterCondition.CarCode == null || a.CarCode.Equals(V_CarMasterCondition.CarCode))
                && (a.BrandDelFlag == "0")
                && (a.CarDelFlag == "0")
                && (a.GradeDelFlag == "0")
                && (a.MakerDelFlag == "0")
                orderby a.MakerCode
                select a;

            return new PaginatedList<V_CarMaster>(query, pageIndex ?? 0, pageSize ?? 0);
        }


        /// <summary>
        /// 車両データ取得(自社取扱いメーカのみ)
        /// </summary>
        /// <param name="condition">メーカーコード</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>車両データ</returns>
        public PaginatedList<V_CarMaster> GetPrivateListByCondition(V_CarMaster V_CarMasterCondition, int? pageIndex, int? pageSize)
        {
            IQueryable<V_CarMaster> query =
                from a in db.V_CarMaster
                where (V_CarMasterCondition.MakerCode == null || a.MakerCode.Equals(V_CarMasterCondition.MakerCode))
                && (V_CarMasterCondition.CarCode == null || a.CarCode.Equals(V_CarMasterCondition.CarCode))
                && (a.BrandDelFlag == "0")
                && (a.CarDelFlag == "0")
                && (a.GradeDelFlag == "0")
                && (a.MakerDelFlag == "0")
                && (a.PrivateFlag == "1")
                orderby a.MakerCode
                select a;

            return new PaginatedList<V_CarMaster>(query, pageIndex ?? 0, pageSize ?? 0);
        }

        /// <summary>
        /// 車両データ取得
        /// </summary>
        /// <param name="condition">メーカーコード</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>車両データ</returns>
        public V_CarMaster GetBykey(string MakerCode)
        {
            V_CarMaster query =
                (from a in db.V_CarMaster
                where (MakerCode == null || a.MakerCode.Equals(MakerCode))
                && (a.BrandDelFlag == "0")
                && (a.CarDelFlag == "0")
                && (a.GradeDelFlag == "0")
                && (a.MakerDelFlag == "0")
                 orderby a.MakerCode
                select a).FirstOrDefault();

            return query;
        }


    }
}
