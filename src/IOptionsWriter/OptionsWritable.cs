using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
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
        private readonly IOptionsMonitor<T> _options;
        private readonly bool _forceReloadAfterWrite;
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

            ExpandoObject config;
            if (!File.Exists(fullPath))
            {
                var value = Value;
                applyChanges(value);
                config = new ExpandoObject();
                config.TryAdd(_section, value);
            }
            else
            {
                config = await File.OpenRead(fullPath).ToObjectAsync<ExpandoObject>();
                var sectionObject = (T) config.SingleOrDefault(p => string.Equals(p.Key, _section)).Value;
                applyChanges(sectionObject);
                config.Remove(_section, out _);
                config.TryAdd(_section, sectionObject);
            }

            await File.Create(fullPath).WriteFromObjectAsync(config, new JsonSerializerOptions {WriteIndented = true});

            if (_forceReloadAfterWrite) _configurationRoot.Reload();
        }
    }
}