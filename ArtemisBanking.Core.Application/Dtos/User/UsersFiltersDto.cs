namespace ArtemisBanking.Core.Application.Dtos.User;

public class UsersApiFiltersDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Role { get; set; }
}