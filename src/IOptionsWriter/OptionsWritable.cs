using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using JStreamAsyncNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace IOptionsWriter
{
    public class OptionsWritable<T> : IOptionsWritable<T> where T : class, new()
    {
        private readonly ConfigurationRoot _configurationRoot;
        private readonly IHostingEnvironment _environment;
        private readonly bool _forceReloadAfterWrite;
        private readonly IOptionsMonitor<T> _options;
        private readonly string _section;
        private readonly string _settingsFile;

        public OptionsWritable(IHostingEnvironment environment,
            IOptionsMonitor<T> options,
            ConfigurationRoot configurationRoot,
            string section,
            string settingsFile,
            bool forceReloadAfterWrite = false)
        {
            _environment = environment;
            _options = options;
            _configurationRoot = configurationRoot;
            _section = section;
            _settingsFile = settingsFile;
            _forceReloadAfterWrite = forceReloadAfterWrite;
        }

        public T Get(string name)
        {
            return _options.Get(name);
        }

        public async Task Update(Action<T> applyChanges)
        {
            var fullPath = Path.IsPathRooted(_settingsFile)
                ? _settingsFile
                : _environment.ContentRootFileProvider.GetFileInfo(_settingsFile).PhysicalPath;

            ExpandoObject config;
            applyChanges(Value);

            if (!File.Exists(fullPath))
            {
                config = new ExpandoObject();
                ((IDictionary<string, object>) config).Add(_section, Value);
            }
            else
            {
                config = await File.OpenRead(fullPath).ToObjectAsync<ExpandoObject>();
                ((IDictionary<string, object>) config)[_section] = Value;
            }

            await File.Create(fullPath).WriteFromObjectAsync(config, new JsonSerializerOptions {WriteIndented = true});

            if (_forceReloadAfterWrite) _configurationRoot.Reload();
        }

        public T Value => _options.CurrentValue;
    }
}