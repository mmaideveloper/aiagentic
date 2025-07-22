using System.Runtime.CompilerServices;

namespace CopilotChat.WebApi.Plugins.Utils
{
    public static class ConfigurationExtensions
    {
        private static IEnumerable<IConfigurationSection> GetPlugins(this IConfiguration configuration)
        {
            return configuration.GetSection("Plugins").GetChildren();
        }

        private static IConfigurationSection? GetPluginByName(this IConfiguration configuration, string pluginName)
        {
            var plugin = configuration.GetPlugins()
                .FirstOrDefault(p => p["Name"]?.Equals(pluginName, StringComparison.OrdinalIgnoreCase) == true);
            return plugin;
        }

        public static T GetPluginValue<T>(this IConfiguration configuration, string pluginName, string key)  where T : class,new ()
        {
            var plugin = configuration.GetPluginByName(pluginName);
            var response = plugin.GetValue<T>(key);
            if( response == null )
            {
                var section  = plugin.GetSection(key);
                response = new T();
                section.Bind(response);
            }

            return response;
        }
    }
}
