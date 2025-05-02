namespace Syddjurs_Item_API.Interfaces
{
    public interface IUserContext
    {
        string? UserId { get; set; }
        string? UserName { get; set; }
        string? Email { get; set; }
    }
}
