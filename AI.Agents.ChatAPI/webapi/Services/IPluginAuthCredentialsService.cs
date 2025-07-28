using CopilotChat.WebApi.Auth;
using System.Text.RegularExpressions;

namespace CopilotChat.WebApi.Services
{
    public interface IPluginAuthCredentialsService
    {
        Dictionary<string, string> GetPluginAuthHeaders();
        IAuthInfo GetAuthInfo();
    }

    public class PluginAuthCredentialsService : IPluginAuthCredentialsService
    {

        //private readonly IHeaderDictionary _headers;
        private readonly IAuthInfo _authInfo;
        private readonly IHttpContextAccessor _contextAccessor;

        public PluginAuthCredentialsService(
            IAuthInfo authInfo,
            IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            _authInfo = authInfo;
        }

        public IAuthInfo GetAuthInfo()
        {
            return _authInfo;
        }

        public Dictionary<string, string> GetPluginAuthHeaders()
        {
            var headers = _contextAccessor.HttpContext.Request.Headers;
            // Create a regex to match the headers
            var regex = new Regex("x-sk-copilot-(.*)-auth", RegexOptions.IgnoreCase);

            // Create a dictionary to store the matched headers and values
            var authHeaders = new Dictionary<string, string>();

            // Loop through the request headers and add the matched ones to the dictionary
            foreach (var header in headers)
            {
                var match = regex.Match(header.Key);
                if (match.Success)
                {
                    // Use the first capture group as the key and the header value as the value
                    authHeaders.Add(match.Groups[1].Value.ToUpperInvariant(), header.Value!);
                }
            }

            return authHeaders;
        }
    }
}
