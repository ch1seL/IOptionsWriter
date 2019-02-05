using System;
using System.IO;
using System.Threading.Tasks;
using JStreamAsyncNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IOptionsWriter
{
	public class OptionsWritable<T> : IOptionsWritable<T> where T : class, new()
	{
		private readonly IHostingEnvironment _environment;
		private readonly IOptionsMonitor<T> _options;
		private readonly ConfigurationRoot _configurationRoot;
		private readonly string _section;
		private readonly string _settingsFile;
		private readonly bool _reloadAfterWrite;

		public OptionsWritable(IHostingEnvironment environment,
			IOptionsMonitor<T> options,
			ConfigurationRoot configurationRoot,
			string section,
			string settingsFile,
			bool reloadAfterWrite = false)
		{
			_environment = environment;
			_options = options;
			_configurationRoot = configurationRoot;
			_section = section;
			_settingsFile = settingsFile;
			_reloadAfterWrite = reloadAfterWrite;
		}

		public T Value => _options.CurrentValue;

		public T Get(string name)
		{
			return _options.Get(name);
		}

		public async Task Update(Action<T> applyChanges)
		{
			var fullPath = Path.IsPathRooted(_settingsFile)
				? _settingsFile
				: _environment.ContentRootFileProvider.GetFileInfo(_settingsFile).PhysicalPath;

			JObject jObject;
			if (!File.Exists(fullPath))
			{
				T n = Value;
				applyChanges(n);
				jObject = new JObject {{_section, JObject.FromObject(n)}};
			}
			else
			{
				jObject = await File.OpenRead(fullPath).ToObjectAsync<JObject>();
				var sectionObject = jObject.TryGetValue(_section, out JToken section)
					? JsonConvert.DeserializeObject<T>(section.ToString())
					: Value ?? new T();
				applyChanges(sectionObject);
				jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
			}

			await File.Create(fullPath).WriteFromObjectAsync(jObject,
				JsonSerializer.Create(new JsonSerializerSettings {Formatting = Formatting.Indented}));

			if (_reloadAfterWrite) _configurationRoot.Reload();
		}
	}
}