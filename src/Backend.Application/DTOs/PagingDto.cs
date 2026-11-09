using Backend.Domain.Enums;

namespace Backend.Application.DTOs;

// Query-string paging: ?index=1&limit=20
// index starts at 1. limit defaults to 20 and is capped at 100.
public class PagedRequestDto
{
    private int _index = 1;
    private int _limit = 20;

    public int Index
    {
        get => _index;
        set => _index = value < 1 ? 1 : value;
    }

    public int Limit
    {
        get => _limit;
        set => _limit = value < 1 ? 20 : Math.Min(value, 100);
    }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
    public int Index { get; set; }
    public int Limit { get; set; }

    // Pages a list that is already in memory.
    public static PagedResult<T> FromList(
        IEnumerable<T> all, int index, int limit)
    {
        var items = all
            .Skip((index - 1) * limit)
            .Take(limit + 1)
            .ToList();

        return FromPlusOne(items, index, limit);
    }

    // items holds up to limit + 1 rows. The extra row only tells us
    // that a next page exists, so no separate count query is needed.
    public static PagedResult<T> FromPlusOne(
        List<T> items, int index, int limit)
    {
        var hasNext = items.Count > limit;

        return new PagedResult<T>
        {
            Items = hasNext ? items.Take(limit).ToList() : items,
            HasNext = hasNext,
            HasPrevious = index > 1,
            Index = index,
            Limit = limit
        };
    }

    public PagedResult<TOut> Map<TOut>(Func<T, TOut> map)
    {
        return new PagedResult<TOut>
        {
            Items = Items.Select(map).ToList(),
            HasNext = HasNext,
            HasPrevious = HasPrevious,
            Index = Index,
            Limit = Limit
        };
    }
}

// Filters for claim lists (maker and approver).
public class ClaimFilterDto : PagedRequestDto
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public int? EmployeeId { get; set; }
    public int? CategoryId { get; set; }
    public ClaimStatus? Status { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? MaxPerItem { get; set; }
}

public class LeaveTypeDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MaxDaysPerYear { get; set; }
    public Boolean RequiresAttachment { get; set; } = false;

}