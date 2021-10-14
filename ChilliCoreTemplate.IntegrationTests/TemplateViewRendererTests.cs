using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ChilliCoreTemplate.IntegrationTests
{
    public class TemplateEngineTests
    {

        private readonly ITemplateViewRenderer _viewRenderer;
        public TemplateEngineTests()
        {
            var server = new TemplateEngineTestServerFixture();
            var serviceProvider = server.GetRequiredService<IServiceProvider>();
            var viewEngine = server.GetRequiredService<IRazorViewEngine>();
            var tempDataProvider = server.GetRequiredService<ITempDataProvider>();

            var mockOptions = new Mock<IOptions<TemplateViewRendererOptions>>();
            mockOptions.Setup(c => c.Value).Returns(() => new TemplateViewRendererOptions() { BaseUri = new Uri("https://localhost") });

            _viewRenderer = new TemplateViewRenderer(viewEngine, serviceProvider, tempDataProvider, mockOptions.Object);
        }
   
          
        [Fact]
        public async Task RenderAsync_Should_ReturnTemplateAsString()
        {
            var model = "test";
            var actual = await _viewRenderer.RenderAsync("TestView", model);

            Assert.Contains(model, actual);
        }
        
        [Fact]
        public async Task  RenderAsync_ShouldThrow_WhenViewCanNotBeFound()
        {
            var model = "test";
         
            await Assert.ThrowsAsync<TemplateViewRenderException>(() => _viewRenderer.RenderAsync("TestViewX", model));
            
        }
        
            
        [Fact]
        public async Task RenderAsync_ShouldRender_WhenModelObjectIsPassed()
        {
            var model = new TestViewModel()
            {
                Name = "Test1",
                Subject = "Some Subject",
                Description = "This is the template test"
            };
            
            var actual = await _viewRenderer.RenderAsync("TestView2", model);

            Assert.Contains(model.Description, actual);
            Assert.Contains(model.Name, actual);
            Assert.Contains(model.Subject, actual);
        }
        
              
        [Fact]
        public async Task RenderAsync_ShouldRender_WithNestedTemplate()
        {
            var model = new TestViewModel()
            {
                Name = "Test1",
                Subject = "Some Subject",
                Description = "This is the template test"
            };
            
            var actual = await _viewRenderer.RenderAsync("Emails/EmailAccounts/NestedTemplate", model);

            Assert.Contains(model.Description, actual);
            Assert.Contains(model.Name, actual);
            Assert.Contains(model.Subject, actual);
        }
        
        [Fact]
        public async Task RenderAsync_ShouldRender_WithGenericClass()
        {
            var model = new RazorTemplateDataModel<string>()
            {
                Data =  "hello",
                Site = "site"
            };

            var m = (IEmailTemplateDataModel) model;
            
            var actual = await _viewRenderer.RenderAsync("TestGenericInterfaceClassView", m);

            Assert.Contains(model.Site, actual);
         
        }
        
    }

    public class TemplateEngineTestServerFixture : WebApplicationFactory<TemplateEngineTestStartUp>
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
            return hostBuilder.UseStartup<TemplateEngineTestStartUp>();
        }

        // uncomment if your test project isn't in a child folder of the .sln file
        // protected override void ConfigureWebHost(IWebHostBuilder builder)
        // {
        //    builder.UseSolutionRelativeContentRoot("relative/path/to/test/project");
        // }
    }


    public class TemplateEngineTestStartUp : IStartup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddCookieTempDataProvider();
                
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            
        }
    }
}