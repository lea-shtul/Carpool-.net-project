namespace Carpool.Core.Dtos.Common;

/// <summary>
/// Generic page of results. Populated from database-side <c>Skip</c>/<c>Take</c>
/// paging plus a <c>Count</c> — the full table is never loaded into memory (spec §20, §65).
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
