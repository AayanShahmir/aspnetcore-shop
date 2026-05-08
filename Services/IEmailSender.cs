namespace BIsm2.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string conlink);
        //string subject, string msg
    }
}
