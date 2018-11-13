using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace IOptionsWriter
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection ConfigureWritable<T>(this IServiceCollection services, IConfigurationSection section, string settingsFile = "appsettings.json", bool reloadAfterWrite =false) where T : class, new()
		{
			services.Configure<T>(section);
			services.AddTransient<IOptionsWritable<T>>(provider =>
			{
				var environment = provider.GetRequiredService<IHostingEnvironment>();
				var options = provider.GetRequiredService<IOptionsMonitor<T>>();
				var configurationRoot = (ConfigurationRoot)provider.GetRequiredService<IConfiguration>();
				return new OptionsWritable<T>(environment, options, configurationRoot, section.Key, settingsFile, reloadAfterWrite);
			});

			return services;
		}
	}

}