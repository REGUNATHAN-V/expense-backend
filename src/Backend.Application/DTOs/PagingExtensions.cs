using Microsoft.EntityFrameworkCore;
using Backend.Application.DTOs;

namespace Backend.Infrastructure.Common;

public static class PagingExtensions
{
    // The query must already be ordered, otherwise pages are not stable.
    public static async Task<PagedResult<T>> ToPagedAsync<T>(
        this IQueryable<T> query,
        int index,
        int limit,
        CancellationToken ct = default)
    {
        var items = await query
            .Skip((index - 1) * limit)
            .Take(limit + 1)
            .ToListAsync(ct);

        return PagedResult<T>.FromPlusOne(items, index, limit);
    }
}