using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IOptionsWriter.IntegrationTests
{
    public class OptionsWritableTests : TestsBase
    {
        [Fact]
        public void OptionsWriterShouldReturnDefaultValue()
        {
            var expectedTestOptionValue = "defaultOptions";
            
            var optionsWritable = TestHost.Services.GetRequiredService<IOptionsWritable<TestOptions>>();
            
            optionsWritable.Value.TestOption.Should().Be(expectedTestOptionValue);
        }

        [Fact]
        public async Task OptionsWriterShouldUpdateFileContent()
        {
            var expectedTestOptionValue = "changedValue";
            
            var optionsWritable = TestHost.Services.GetRequiredService<IOptionsWritable<TestOptions>>();
            await optionsWritable.Update(options => options.TestOption = expectedTestOptionValue);
            var jsonDocument = await JsonDocument.ParseAsync(File.OpenRead("appsettings.json"));
            var actualTestOptions = jsonDocument.RootElement.EnumerateObject().SingleOrDefault(p => p.NameEquals(nameof(TestOptions))).Value
                .ToObject<TestOptions>();

            actualTestOptions.TestOption.Should().Be(expectedTestOptionValue);
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
    }
}