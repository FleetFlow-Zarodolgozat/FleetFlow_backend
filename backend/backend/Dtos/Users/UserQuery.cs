namespace backend.Dtos.Users
{
    public class UserQuery
    {
        public string? StringQ { get; set; }
        public bool IsActiveQ { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}
