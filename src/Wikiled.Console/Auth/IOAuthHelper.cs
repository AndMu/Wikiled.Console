using System.Threading.Tasks;

namespace Wikiled.Console.Auth
{
    public interface IOAuthHelper
    {
        string RedirectUri { get; set; }

        string Code { get; }

        bool IsSuccessful { get; }

        Task Start(string serviceUrl, string state = null);
    }
}