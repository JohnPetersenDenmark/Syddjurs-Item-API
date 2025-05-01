namespace Syddjurs_Item_API.Services
{
    public interface IEmailService
    {

        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
