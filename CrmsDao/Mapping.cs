/// <summary>
/// マッピングクラス
/// </summary>
/// <remarks>
/// Add 2015/03/20 arc yano 現金出納帳クラス
/// <T>型のオブジェクトを<Tres>型のオブジェクトに変換
/// ためのマッピングクラスです。
/// </remarks>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace CrmsDao
{
    public class Mapping<T>
    {

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name = ""></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public Mapping()
        {
        }

        // <summary>
        /// 実行
        /// </summary>
        /// <param name = "item"></param>
        /// <returns>マッピング後のオブジェクト</returns>
        /// <remarks>
        /// <T>型のオブジェクト→<Tres>型に変換
        /// </remarks>
        public static Tres Execute<Tres>(T item)
        {
            return ReflectionMapping<Tres>(item);
        }

        // <summary>
        /// 配列型の変換
        /// </summary>
        /// <param name = "target"></param>
        /// <returns></returns>
        /// <remarks>
        /// <T>型の配列→<Tres>型の配列に変換
        /// </remarks>
        public static Tres[] ToArray<Tres>(T[] target)
        {
            IList<Tres> list = new List<Tres>();
            ReflectionMappings<Tres>(target, list);
            return list.ToArray();
        }

        // <summary>
        /// リスト型の変換
        /// </summary>
        /// <param name = "target"></param>
        /// <returns></returns>
        /// <remarks>
        /// <T>型のリスト→<Tres>型のリストに変換
        /// </remarks>
        public static IList<Tres> ToList<Tres>(IList<T> target)
        {
            IList<Tres> results = new List<Tres>();
            ReflectionMappings<Tres>(target, results);
            return results;
        }

        // <summary>
        /// リフレクションマッピング
        /// </summary>
        /// <param name = "target"></param>
        /// <param name = "results"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        private static void ReflectionMappings<Tres>(IEnumerable<T> target, IList<Tres> results)
        {
            foreach (var item in target)
            {
                Tres result = ReflectionMapping<Tres>(item);

                results.Add(result);
            }
        }

        private static Tres ReflectionMapping<Tres>(T item)
        {
            Type type = item.GetType();
            PropertyInfo[] props = type.GetProperties();

            Tres result = Activator.CreateInstance<Tres>();
            Type resType = result.GetType();

            foreach (var prop in props)
            {
                PropertyInfo resProp = resType.GetProperty(prop.Name);
                if (resProp != null)
                {
                    resProp.SetValue(result, prop.GetValue(item, null), null);
                }
            }
            return result;
        }
    }
}
