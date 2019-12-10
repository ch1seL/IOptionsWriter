using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using JStreamAsyncNet;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IOptionsWriter.IntegrationTests
{
    public class OptionsWritableTests : TestsBase
    {
        private const string AppSettingsFile = "appsettings.json";
        
        private static async Task<JsonDocument> JsonDocumentFromFile(string file)
        {
            await using var stream = File.OpenRead(file);
            return await JsonDocument.ParseAsync(stream);
        }

        [Fact]
        public async Task OptionsWriterShouldNotUpdateOtherSections()
        {
            var expectedSection = new {TestSection1 = new {Key1 = "Value1"}};
            
            //save test section to appsettings.json file
            await File.OpenWrite(AppSettingsFile).WriteFromObjectAsync(expectedSection);
            
            //update options
            var optionsWritable = TestHost.Services.GetRequiredService<IOptionsWritable<TestOptions>>();
            await optionsWritable.Update(options => options.TestOption = "some update event value");
            
            //reread test section from file 
            var jsonDocument = await JsonDocumentFromFile(AppSettingsFile);
            var actualTestSection1KeyValue = jsonDocument.RootElement.EnumerateObject()
                .SingleOrDefault(p => p.NameEquals(nameof(expectedSection.TestSection1)))
                .Value.EnumerateObject()
                .SingleOrDefault(p => p.NameEquals(nameof(expectedSection.TestSection1.Key1)))
                .Value.GetString();

            actualTestSection1KeyValue
                .IsSameOrEqualTo(expectedSection.TestSection1.Key1);
        }

        [Fact]
        public async Task OptionsWriterShouldRefreshValue()
        {
            var expectedTestOptionValue = "changedValue";

            var optionsWritable = TestHost.Services.GetRequiredService<IOptionsWritable<TestOptions>>();
            await optionsWritable.Update(options => options.TestOption = expectedTestOptionValue);
            var anotherOptionsWritable = TestHost.Services.GetRequiredService<IOptionsWritable<TestOptions>>();

            anotherOptionsWritable.Value.TestOption.Should().Be(expectedTestOptionValue);
        }

        [Fact]
        public void OptionsWriterShouldReturnDefaultValue()
        {
            const string expectedTestOptionValue = "defaultOptions";

            var optionsWritable = TestHost.Services.GetRequiredService<IOptionsWritable<TestOptions>>();

            optionsWritable.Value.TestOption.Should().Be(expectedTestOptionValue);
        }

        [Fact]
        public async Task OptionsWriterShouldUpdateFileContent()
        {
            const string expectedTestOptionValue = "changedValue";

            var optionsWritable = TestHost.Services.GetRequiredService<IOptionsWritable<TestOptions>>();
            await optionsWritable.Update(options => options.TestOption = expectedTestOptionValue);
            var jsonDocument = await JsonDocumentFromFile(AppSettingsFile);
            var actualTestOptions = jsonDocument.RootElement.EnumerateObject()
                .SingleOrDefault(p => p.NameEquals(nameof(TestOptions))).Value
                .ToObject<TestOptions>();

            actualTestOptions.TestOption.Should().Be(expectedTestOptionValue);
        }
    }
}