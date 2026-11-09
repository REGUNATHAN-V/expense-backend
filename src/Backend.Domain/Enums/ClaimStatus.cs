namespace Backend.Domain.Enums;

public enum ClaimStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    Returned = 4,
    Cancelled = 5
}

 
public enum UserRole
{
    Maker = 1,
    Approver = 2,
    Admin = 3
}
 