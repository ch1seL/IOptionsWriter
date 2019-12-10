using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IOptionsWriter.IntegrationTests
{
    public abstract class TestsBase
    {
        protected IHost TestHost;

        protected TestsBase()
        {
            //Ensure that settings file is not exists
            File.Delete("appsettings.json");
            
            //create test host
            TestHost = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.ConfigureWritable<TestOptions>();
                })
                .Build();
        }
    }
}