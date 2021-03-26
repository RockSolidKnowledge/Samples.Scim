using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Rsk.AspNetCore.Scim.Authenticators;
using Rsk.AspNetCore.Scim.Enums;
using Rsk.AspNetCore.Scim.Results;

namespace Authentication.Authenticator
{
    public class Authenticator : IAuthenticate
    {
        public Task<IScimResult> Authenticate(HttpClient httpClient, string serviceProviderName)
        {
            Encoding encoding = Encoding.UTF8;
            string credential = "userName:password";

            string encodedString = Convert.ToBase64String(encoding.GetBytes(credential));

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedString);

            var success = new ScimResult(ScimResultStatus.Success);

            return Task.FromResult((IScimResult) success);
        }
    }
}
