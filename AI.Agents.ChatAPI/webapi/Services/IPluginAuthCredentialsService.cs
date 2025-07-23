using System.Text.RegularExpressions;

namespace CopilotChat.WebApi.Services
{
    public interface IPluginAuthCredentialsService
    {
        Dictionary<string, string> GetPluginAuthHeaders();
    }

    public class PluginAuthCredentialsService : IPluginAuthCredentialsService
    {

        private readonly IHeaderDictionary _headers;

        public PluginAuthCredentialsService(IHeaderDictionary headers)
        { _headers = headers; }

        public Dictionary<string, string> GetPluginAuthHeaders()
        {
            // Create a regex to match the headers
            var regex = new Regex("x-sk-copilot-(.*)-auth", RegexOptions.IgnoreCase);

            // Create a dictionary to store the matched headers and values
            var authHeaders = new Dictionary<string, string>();

            // Loop through the request headers and add the matched ones to the dictionary
            foreach (var header in _headers)
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
