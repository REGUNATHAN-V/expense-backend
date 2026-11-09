namespace Backend.API.Common.Responses;

public class PaginationDto
{
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
    public int Index { get; set; }
    public int Limit { get; set; }
}