namespace Projet_Final.Services
{
    public interface ISMSSenderService
    {
        Task SendSmsAsync(string number, string message);
    }
}
