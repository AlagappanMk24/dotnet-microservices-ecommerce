using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.SharedLibrary.Responses
{
    public sealed record PagedResult<T>(
      IReadOnlyList<T> Items,
      int PageNumber,
      int PageSize,
      int TotalCount)
    {
        public int TotalPages =>
            (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;
    }
}