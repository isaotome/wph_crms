using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrmsDao {
    public class PaginatedList<T> : List<T> {
        public PaginatedProperty PageProperty { get; set; }
        public PaginatedList() {
            PaginatedProperty paginatedProperty = new PaginatedProperty();
            paginatedProperty.PageIndex = 0;
            paginatedProperty.PageSize = 0;
            paginatedProperty.TotalCount = 0;
            paginatedProperty.TotalPages = 0;
            paginatedProperty.StartPageIndex = 0;
            this.PageProperty = paginatedProperty;
        }
        public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize) {
            PaginatedProperty paginatedProperty = new PaginatedProperty();
            paginatedProperty.PageIndex = pageIndex;
            paginatedProperty.PageSize = pageSize;
            paginatedProperty.TotalCount = source.Count();
            paginatedProperty.TotalPages = (int)Math.Ceiling(source.Count() / (double)pageSize);
            if (pageIndex > 10)
                paginatedProperty.StartPageIndex = pageIndex - 10;
            else
                paginatedProperty.StartPageIndex = 0;

            this.PageProperty = paginatedProperty;

            this.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize));
            this.PageProperty.Count = this.Count();
        }
        /// <summary>
        /// ページクラス初期化
        /// </summary>
        /// <history>
        /// 2021/02/22 yano #4083 【部品マスタ検索】検索処理のパフォーマンス改善対応
        /// </history>
        public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize, int recCount)
        {
            PaginatedProperty paginatedProperty = new PaginatedProperty();
            paginatedProperty.PageIndex = pageIndex;
            paginatedProperty.PageSize = pageSize;
            paginatedProperty.TotalCount = recCount;
            paginatedProperty.TotalPages = (int)Math.Ceiling(recCount / (double)pageSize);
            if (pageIndex > 10)
                paginatedProperty.StartPageIndex = pageIndex - 10;
            else
                paginatedProperty.StartPageIndex = 0;

            this.PageProperty = paginatedProperty;

            this.AddRange(source.Skip(pageIndex * pageSize).Take(pageSize));
            this.PageProperty.Count = this.Count();
        }
    }

 
    public class PaginatedProperty {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int StartPageIndex { get; set; }
        public int Count { get; set; }
        public bool HasPreviousPage {
            get {
                return (PageIndex > 0);
            }
        }

        public bool HasNextPage {
            get {
                return (PageIndex + 1 < TotalPages);
            }
        }
    }
}
