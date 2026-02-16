namespace ContactManager.Models
{
    public record RegisterRequest(string UserName, string Email, string Password);
    public record LoginRequest(string UserOrEmail, string Password);
}
