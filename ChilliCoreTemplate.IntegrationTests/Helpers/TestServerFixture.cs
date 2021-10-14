using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ChilliCoreTemplate.IntegrationTests.Helpers
{
    public class TestServerFixture<T> : WebApplicationFactory<T> where T : class
    {
        public TService GetRequiredService<TService>()
        {
            if (Server == null)
            {
                CreateDefaultClient();
            }
            
            return Server.Host.Services.GetRequiredService<TService>();
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var hostBuilder = new WebHostBuilder();
            return hostBuilder.UseStartup<T>();
        }

        // uncomment if your test project isn't in a child folder of the .sln file
        // protected override void ConfigureWebHost(IWebHostBuilder builder)
        // {
        //    builder.UseSolutionRelativeContentRoot("relative/path/to/test/project");
        // }
    }
}