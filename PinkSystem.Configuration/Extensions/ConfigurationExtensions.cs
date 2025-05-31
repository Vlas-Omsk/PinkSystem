using Microsoft.Extensions.Configuration;

namespace PinkSystem.Configuration
{
    public static class ConfigurationExtensions
    {
        public static T GetRequired<T>(this IConfiguration configuration, Action<BinderOptions>? configureOptions = null)
        {
            return configuration.Get<T>(configureOptions) ?? throw new NullReferenceException();
        }

        public static object GetRequired(this IConfiguration configuration, Type type, Action<BinderOptions>? configureOptions = null)
        {
            return configuration.Get(type, configureOptions) ?? throw new NullReferenceException();
        }

        public static T GetValueRequired<T>(this IConfiguration configuration, string key)
        {
            return configuration.GetRequiredSection(key).GetRequired<T>();
        }

        public static object GetValueRequired(this IConfiguration configuration, Type type, string key)
        {
            return configuration.GetRequiredSection(key).GetRequired(type);
        }
    }
}
