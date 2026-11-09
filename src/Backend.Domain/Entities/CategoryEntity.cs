namespace Backend.Domain.Entities;

public class CategoryEntity: BaseEntity
{

    /// <summary>Taxi, Meals, Parking ...</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional limit for a single item in this category.</summary>
    public decimal? MaxPerItem { get; set; }
}


public class LeaveTypeEntity{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MaxDaysPerYear { get; set; }
    public Boolean RequiresAttachment { get; set; } = false;
}