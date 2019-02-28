using System.Threading.Tasks;

namespace Wikiled.Console.Auth
{
    public interface IAuthClient<T>
        where T : class
    {
        string BuildAuthorizeUrl(string callback = null);

        Task<T> GetToken(string code, string redirectUri = null);

        Task<T> RefreshToken(T token);
    }
}
