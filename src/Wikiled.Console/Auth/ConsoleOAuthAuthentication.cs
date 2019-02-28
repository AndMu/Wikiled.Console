using System;
using System.Threading.Tasks;
using Wikiled.Common.Utilities.Auth;

namespace Wikiled.Console.Auth
{
    public class ConsoleOAuthAuthentication<T> : IAuthentication<T>
        where T : class
    {
        private readonly IAuthClient<T> client;

        private readonly OAuthHelper helper;

        public ConsoleOAuthAuthentication(IAuthClient<T> client, OAuthHelper helper)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public async Task<T> Authenticate()
        {
            var auth = client.BuildAuthorizeUrl(helper.RedirectUri);
            await helper.Start(auth, null).ConfigureAwait(false);
            var code = helper.Code;
            T token = await client.GetToken(code, helper.RedirectUri);
            return token;
        }

        public Task<T> Refresh(T old)
        {
            return client.RefreshToken(old);
        }
    }
}
