using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.IntegrationTests.Helpers;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ChilliCoreTemplate.IntegrationTests
{
    public class AsyncDispatchEmailQueueTests
    {

        private readonly ITemplateViewRenderer _templateViewRenderer;
        private readonly IServiceProvider _serviceProvider;

        public AsyncDispatchEmailQueueTests()
        {
            var server = new TestServerFixture<AsyncEmailQueueTestStartUp>();
            _serviceProvider = server.GetRequiredService<IServiceProvider>();
            var viewEngine = server.GetRequiredService<IRazorViewEngine>();
            var tempDataProvider = server.GetRequiredService<ITempDataProvider>();

            var mockOptions = new Mock<IOptions<TemplateViewRendererOptions>>();
            mockOptions.Setup(c => c.Value).Returns(() => new TemplateViewRendererOptions() { BaseUri = new Uri("https://localhost") });

            _templateViewRenderer = new TemplateViewRenderer(viewEngine, _serviceProvider, tempDataProvider, mockOptions.Object);
        }


        private IEmailQueue CreateEmailQueue(IEmailSender sender)
        {
            return new ImmediateDispatchEmailQueue(_serviceProvider, _templateViewRenderer, sender);
        }

        private IEmailSender CreateSender(ProjectSettings settings, Func<MailConfigurationSection, IEmailClient> clientFactory)
        {
            var storageMock = new Mock<IFileStorage>();
            storageMock.Setup(c => c.GetContentAsync(It.IsAny<string>(), It.IsAny<StorageEncryptionKeys>(), CancellationToken.None))
                .ReturnsAsync(() => FileStorageResponse.Create("test.txt", 0, "text/plain", new MemoryStream()));

            long contentLength;
            string contentType;

            storageMock.Setup(c => c.GetContent(It.IsAny<string>(), It.IsAny<StorageEncryptionKeys>(), out contentLength, out contentType))
                .Returns(() => new MemoryStream());

            return new EmailSender(
                settings,
                storageMock.Object,
                clientFactory,
                null
            );
        }


        [Fact]
        public async Task Enqueue_Should_DispatchEmailImmediately()
        {
            var mock = new Mock<IEmailSender>();
            mock.Setup(m => m.SendAsync(It.IsAny<EmailData>())).Returns(() => Task.FromResult(ServiceResult.AsSuccess()));
            var queue = CreateEmailQueue(mock.Object);

            var item = new EmailQueueItem<TestViewModel>.Builder()
                .Subject("test")
                .To("max@bluechilli.com")
                .UseData(new TestViewModel()
                {
                    Name = "Hello",
                    Description = "test",
                    Subject = "test"
                })
                .UseTemplate(new RazorTemplate("TestView2"))
                .Build();

            await queue.Enqueue(item);
            mock.Verify();
        }

        [Fact]
        public async Task Enqueue_Should_ThrowWhenEmailFailedToSend()
        {
            var mock = new Mock<IEmailSender>();
            mock.Setup(m => m.SendAsync(It.IsAny<EmailData>())).Returns(() => Task.FromResult(ServiceResult.AsError("failed")));
            var queue = CreateEmailQueue(mock.Object);

            var item = new EmailQueueItem<TestViewModel>.Builder()
                .Subject("test")
                .To("max@bluechilli.com")
                .UseData(new TestViewModel()
                {
                    Name = "Hello",
                    Description = "test",
                    Subject = "test"
                })
                .UseTemplate(new RazorTemplate("TestView2"))
                .Build();

            await Assert.ThrowsAsync<ApplicationException>(() => queue.Enqueue(item));
        }

        [Fact(Skip = "use paper cut or change the config to ses to test this")]
        public async Task Enqueue_ShouldSendRealEmail()
        {
            var settings = TestHelper.GetProjectConfiguration(TestHelper.GetTestFolder());
            var sender = CreateSender(settings, (config) => new EmailClient(config.Host, config.Port));
            var queue = CreateEmailQueue(sender);

            var item = new EmailQueueItem<TestViewModel>.Builder()
                .Subject("test")
                .To("max@bluechilli.com")
                .UseData(new TestViewModel()
                {
                    Name = "Hello",
                    Description = "test",
                    Subject = "test"
                })
                .UseTemplate(new RazorTemplate("TestView2"))
                .Build();

            await queue.Enqueue(item);

            Assert.True(true);
        }
    }



    public class AsyncEmailQueueTestStartUp : IStartup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var mockDataContext = new Mock<DataContext>();
            services.AddScoped<DataContext>((_) => mockDataContext.Object);
            services.AddMvc()
                .AddCookieTempDataProvider();

            //var mockProjectSettings = new Mock<ProjectSettings>();
            //services.AddSingleton<ProjectSettings>((_) => mockProjectSettings.Object);

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