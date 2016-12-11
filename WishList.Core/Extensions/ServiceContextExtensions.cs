using System.Fabric;

namespace WishList.Core.Extensions
{
    public static class ServiceContextExtensions
    {
        public static string GetConfigurationValue(this ServiceContext context, string parameterName, string defaultValue = null, string configurationSection = "Configuration", string configurationPackage = "Config")
        {
            var config = context.CodePackageActivationContext.GetConfigurationPackageObject(configurationPackage);
            if (config == null) { return defaultValue; }

            var section = config.Settings.Sections[configurationSection];
            if (section == null) { return defaultValue; }

            if (!section.Parameters.Contains(parameterName)) { return defaultValue; }
            return section.Parameters[parameterName]?.Value ?? defaultValue;
        }
    }
}
