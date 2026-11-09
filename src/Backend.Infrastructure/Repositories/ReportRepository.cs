using Microsoft.EntityFrameworkCore;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Repositories;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Infrastructure.Common;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

internal sealed class ItemRow
{
    public int ItemId { get; set; }
    public int ClaimId { get; set; }
    public int EmployeeId { get; set; }
    public int? ApproverId { get; set; }
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateOnly ExpenseDate { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? ReceiptUrl { get; set; }
    public ClaimStatus Status { get; set; }
}

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    // 1. Total per employee per month, by category.
    public async Task<PagedResult<MonthlyTotalDto>> GetMonthlyTotalsAsync(
        ReportFilterDto filter,
        CancellationToken ct = default)
    {
        // Sum per day in the database, then group by month in memory.
        var rows = await ItemRows(filter)
            .GroupBy(r => new
            {
                r.EmployeeId,
                r.CategoryId,
                r.Category,
                r.ExpenseDate
            })
            .Select(g => new
            {
                g.Key.EmployeeId,
                g.Key.CategoryId,
                g.Key.Category,
                g.Key.ExpenseDate,
                Total = g.Sum(x => x.Amount)
            })
            .ToListAsync(ct);

        var monthly = rows
            .GroupBy(r => new
            {
                r.EmployeeId,
                Month = $"{r.ExpenseDate.Year:D4}-{r.ExpenseDate.Month:D2}",
                r.CategoryId,
                r.Category
            })
            .Select(g => new
            {
                g.Key.EmployeeId,
                g.Key.Month,
                g.Key.CategoryId,
                g.Key.Category,
                Total = g.Sum(x => x.Total)
            })
            .ToList();

        // Every maker is listed, even without claims.
        var claimantIds = monthly.Select(m => m.EmployeeId).Distinct().ToList();
        var makerRole = (int)UserRole.Maker;

        var usersQuery = _context.Users.AsNoTracking()
            .Where(u => !u.IsDeleted &&
                        (u.Role == makerRole || claimantIds.Contains(u.Id)));

        if (filter.EmployeeId is not null)
        {
            var employeeId = filter.EmployeeId.Value;
            usersQuery = usersQuery.Where(u => u.Id == employeeId);
        }

        var users = await usersQuery
            .OrderBy(u => u.Name)
            .ThenBy(u => u.Id)
            .Select(u => new { u.Id, u.Name })
            .ToListAsync(ct);

        var result = new List<MonthlyTotalDto>();

        foreach (var user in users)
        {
            var mine = monthly
                .Where(m => m.EmployeeId == user.Id)
                .OrderBy(m => m.Month)
                .ThenBy(m => m.Category)
                .ToList();

            if (mine.Count == 0)
            {
                // With a category filter, an employee with nothing in that
                // category is simply left out instead of shown as empty.
                if (filter.CategoryId is not null)
                {
                    continue;
                }

                result.Add(new MonthlyTotalDto
                {
                    EmployeeId = user.Id,
                    EmployeeName = user.Name,
                    Total = 0
                });
                continue;
            }

            result.AddRange(mine.Select(m => new MonthlyTotalDto
            {
                EmployeeId = user.Id,
                EmployeeName = user.Name,
                Month = m.Month,
                CategoryId = m.CategoryId,
                Category = m.Category,
                Total = m.Total
            }));
        }

        return PagedResult<MonthlyTotalDto>.FromList(
            result, filter.Index, filter.Limit);
    }

    // 2. Top categories by amount since a date.
    public async Task<List<TopCategoryDto>> GetTopCategoriesAsync(
        DateOnly since,
        int limit,
        int? employeeId,
        CancellationToken ct = default)
    {
        var filter = new ReportFilterDto
        {
            From = since,
            EmployeeId = employeeId
        };

        return await ItemRows(filter)
            .GroupBy(r => new { r.CategoryId, r.Category })
            .OrderByDescending(g => g.Sum(x => x.Amount))
            .Take(limit)
            .Select(g => new TopCategoryDto
            {
                CategoryId = g.Key.CategoryId,
                Category = g.Key.Category,
                Total = g.Sum(x => x.Amount)
            })
            .ToListAsync(ct);
    }

    // 3. Claims whose total does not match the sum of their items.
    public async Task<PagedResult<MismatchedClaimDto>> GetMismatchedClaimsAsync(
        ReportFilterDto filter,
        CancellationToken ct = default)
    {
        var claims = ClaimsQuery(filter);

        var page = await claims
            .Select(c => new
            {
                ClaimId = c.Id,
                c.EmployeeId,
                c.ClaimDate,
                c.Status,
                c.TotalAmount,
                ItemsSum = _context.ClaimItems
                    .Where(i => i.ClaimId == c.Id && !i.IsDeleted)
                    .Sum(i => (decimal?)i.Amount) ?? 0m
            })
            .Where(x => x.TotalAmount != x.ItemsSum)
            .OrderBy(x => x.ClaimId)
            .ToPagedAsync(filter.Index, filter.Limit, ct);

        var names = await UserNamesAsync(
            page.Items.Select(x => x.EmployeeId), ct);

        return page.Map(x => new MismatchedClaimDto
        {
            ClaimId = x.ClaimId,
            EmployeeId = x.EmployeeId,
            EmployeeName = names.TryGetValue(x.EmployeeId, out var n)
                ? n
                : string.Empty,
            ClaimDate = x.ClaimDate,
            Status = x.Status.ToString(),
            TotalAmount = x.TotalAmount,
            ItemsSum = x.ItemsSum,
            Difference = x.TotalAmount - x.ItemsSum
        });
    }

    // 4. Each employee's latest claim (same idea as ROW_NUMBER() = 1):
    //    keep a claim only if it is the newest one for its employee.
    public async Task<PagedResult<LatestClaimDto>> GetLatestClaimsAsync(
        ReportFilterDto filter,
        CancellationToken ct = default)
    {
        var claims = ClaimsQuery(filter);

        var page = await claims
            .Where(c => c.Id == claims
                .Where(x => x.EmployeeId == c.EmployeeId)
                .OrderByDescending(x => x.ClaimDate)
                .ThenByDescending(x => x.Id)
                .Select(x => x.Id)
                .First())
            .Join(
                _context.Users.AsNoTracking(),
                c => c.EmployeeId,
                u => u.Id,
                (c, u) => new
                {
                    c.EmployeeId,
                    EmployeeName = u.Name,
                    ClaimId = c.Id,
                    c.ClaimDate,
                    c.TotalAmount,
                    c.Status,
                    c.ApproverId,
                    c.DecidedAt
                })
            .OrderBy(x => x.EmployeeName)
            .ThenBy(x => x.EmployeeId)
            .ToPagedAsync(filter.Index, filter.Limit, ct);

        var approverNames = await UserNamesAsync(
            page.Items
                .Where(x => x.ApproverId != null)
                .Select(x => x.ApproverId!.Value),
            ct);

        return page.Map(x => new LatestClaimDto
        {
            EmployeeId = x.EmployeeId,
            EmployeeName = x.EmployeeName,
            ClaimId = x.ClaimId,
            ClaimDate = x.ClaimDate,
            TotalAmount = x.TotalAmount,
            Status = x.Status.ToString(),
            ApproverId = x.ApproverId,
            ApproverName = x.ApproverId is not null &&
                           approverNames.TryGetValue(x.ApproverId.Value, out var a)
                ? a
                : null,
            DecidedAt = x.DecidedAt
        });
    }

    // 5. Every expense item behind a monthly total (employee + month + category).
    //    Uses the same rows as the monthly totals, so the numbers always agree.
    public async Task<PagedResult<ExpenseItemDto>> GetCategoryItemsAsync(
        ReportFilterDto filter,
        CancellationToken ct = default)
    {
        var page = await ItemRows(filter)
            .OrderByDescending(r => r.ExpenseDate)
            .ThenByDescending(r => r.ItemId)
            .ToPagedAsync(filter.Index, filter.Limit, ct);

        var names = await UserNamesAsync(
            page.Items.Select(r => r.EmployeeId)
                .Concat(page.Items
                    .Where(r => r.ApproverId != null)
                    .Select(r => r.ApproverId!.Value)),
            ct);

        return page.Map(r => new ExpenseItemDto
        {
            ItemId = r.ItemId,
            ClaimId = r.ClaimId,
            EmployeeId = r.EmployeeId,
            EmployeeName = names.TryGetValue(r.EmployeeId, out var maker)
                ? maker
                : string.Empty,
            CategoryId = r.CategoryId,
            Category = r.Category,
            Amount = r.Amount,
            ExpenseDate = r.ExpenseDate,
            Description = r.Description,
            ReceiptUrl = r.ReceiptUrl,
            Status = r.Status.ToString(),
            ApproverId = r.ApproverId,
            ApproverName = r.ApproverId is not null &&
                           names.TryGetValue(r.ApproverId.Value, out var approver)
                ? approver
                : null
        });
    }

    // Employees for the filter pickers: every maker, plus anyone who has claims.
    public async Task<List<EmployeeLookupDto>> GetEmployeesAsync(
        CancellationToken ct = default)
    {
        var makerRole = (int)UserRole.Maker;

        return await _context.Users.AsNoTracking()
            .Where(u => !u.IsDeleted &&
                        (u.Role == makerRole ||
                         _context.Claims.Any(c =>
                             c.EmployeeId == u.Id && !c.IsDeleted)))
            .OrderBy(u => u.Name)
            .ThenBy(u => u.Id)
            .Select(u => new EmployeeLookupDto { Id = u.Id, Name = u.Name })
            .ToListAsync(ct);
    }

    private async Task<Dictionary<int, string>> UserNamesAsync(
        IEnumerable<int> userIds,
        CancellationToken ct)
    {
        var ids = userIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<int, string>();
        }

        return await _context.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name, ct);
    }

    // Claim items joined to their claim and category.
    // Without a Status filter, Draft and Cancelled claims are left out.
    private IQueryable<ItemRow> ItemRows(ReportFilterDto f)
    {
        var query =
            from i in _context.ClaimItems.AsNoTracking()
            join c in _context.Claims.AsNoTracking() on i.ClaimId equals c.Id
            join cat in _context.Categories.AsNoTracking() on i.CategoryId equals cat.Id
            where !i.IsDeleted && !c.IsDeleted
            select new ItemRow
            {
                ItemId = i.Id,
                ClaimId = c.Id,
                EmployeeId = c.EmployeeId,
                ApproverId = c.ApproverId,
                CategoryId = cat.Id,
                Category = cat.Name,
                ExpenseDate = i.ExpenseDate,
                Amount = i.Amount,
                Description = i.Description,
                ReceiptUrl = i.ReceiptUrl,
                Status = c.Status
            };

        if (f.Status is not null)
        {
            var status = f.Status.Value;
            query = query.Where(r => r.Status == status);
        }
        else
        {
            query = query.Where(r =>
                r.Status != ClaimStatus.Draft &&
                r.Status != ClaimStatus.Cancelled);
        }

        if (f.EmployeeId is not null)
        {
            var employeeId = f.EmployeeId.Value;
            query = query.Where(r => r.EmployeeId == employeeId);
        }

        if (f.CategoryId is not null)
        {
            var categoryId = f.CategoryId.Value;
            query = query.Where(r => r.CategoryId == categoryId);
        }

        if (f.From is not null)
        {
            var from = f.From.Value;
            query = query.Where(r => r.ExpenseDate >= from);
        }

        if (f.To is not null)
        {
            var to = f.To.Value;
            query = query.Where(r => r.ExpenseDate <= to);
        }

        return query;
    }

    // Claims with the optional filters. All statuses unless one is given.
    private IQueryable<ClaimEntity> ClaimsQuery(ReportFilterDto f)
    {
        var query = _context.Claims.AsNoTracking().Where(c => !c.IsDeleted);

        if (f.Status is not null)
        {
            var status = f.Status.Value;
            query = query.Where(c => c.Status == status);
        }

        if (f.EmployeeId is not null)
        {
            var employeeId = f.EmployeeId.Value;
            query = query.Where(c => c.EmployeeId == employeeId);
        }

        // Only claims that contain at least one item of this category.
        if (f.CategoryId is not null)
        {
            var categoryId = f.CategoryId.Value;
            query = query.Where(c => _context.ClaimItems.Any(i =>
                i.ClaimId == c.Id &&
                i.CategoryId == categoryId &&
                !i.IsDeleted));
        }

        if (f.From is not null)
        {
            var from = f.From.Value;
            query = query.Where(c => c.ClaimDate >= from);
        }

        if (f.To is not null)
        {
            var to = f.To.Value;
            query = query.Where(c => c.ClaimDate <= to);
        }

        return query;
    }
}