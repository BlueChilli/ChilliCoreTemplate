using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using Moq;
using Xunit;

namespace ChilliCoreTemplate.Tests
{

    class FakeSmtpClient : IEmailClient
    {

        private MailMessage _message;
        
        public void Dispose()
        {
            
        }

        public bool EnableSsl { get; set; }
        public ICredentialsByHost Credentials { get; set; }

        public async Task SendAsync(MailMessage message)
        {
            _message = message;
            await Task.Delay(0);
        }

        public void Send(MailMessage message)
        {
            _message = message;
        }

        public MailMessage Message => _message;
    }
    public class EmailSenderTests
    {

        private readonly ProjectSettings _settings;

        public EmailSenderTests()
        {
            _settings = TestHelper.GetProjectConfiguration(TestHelper.GetTestFolder());
        }
        
        private IEmailSender GetSender(Func<MailConfigurationSection, IEmailClient> clientFactory)
        {
            var storageMock = new Mock<IFileStorage>();
            storageMock.Setup(c => c.GetContentAsync(It.IsAny<string>(), It.IsAny<StorageEncryptionKeys>(), CancellationToken.None))
                .ReturnsAsync(() => FileStorageResponse.Create("test.txt", 0, "text/plain", new MemoryStream()));

            long contentLength;
            string contentType;

            storageMock.Setup(c => c.GetContent(It.IsAny<string>(), It.IsAny<StorageEncryptionKeys>(), out contentLength, out contentType))
                .Returns(() => new MemoryStream());
        
            return new EmailSender(
                _settings,
                storageMock.Object,
                clientFactory,
                null
                );
        }
        
        [Fact]
        public async Task SendAsync_SendsEmail()
        {
            var clientMock = new Mock<IEmailClient>();
            clientMock.Setup(m => m.SendAsync(It.IsAny<MailMessage>()))
                .Returns(() => Task.Delay(0));
            var sender = GetSender(((_) => clientMock.Object));
            
            var emailData = new EmailData.Builder()
                .From("admin@bluechilli.com", "Admin")
                .To("max@bluechilli.com")
                .Subject("Some subject")
                .Html("this is an html mail")
                .Build();
            
            var r = await sender.SendAsync(emailData);
            Assert.True(r.Success);

            clientMock.Verify();
        }
        
        [Fact]
        public void EmailDataBuilder_Build_Should_ThrowWhenToEmailAddressIsNullOrEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new EmailData.Builder()
                .From("admin@bluechilli.com", "Admin")
                .Subject("Some subject")
                .Html("this is an html mail")
                .Build());
        }
        
        [Fact]
        public async Task SendAsync_FromAddress_ShouldReadFromSettingsWhenToSpecified()
        {
            var client = new FakeSmtpClient();
            var sender = GetSender(((_) => client));
            
            var emailData = new EmailData.Builder()
                .To("max@bluechilli.com")
                .Subject("Some subject")
                .Html("this is an html mail")
                .Build();
            
            var r = await sender.SendAsync(emailData);
            
            Assert.True(r.Success);
            Assert.NotNull(client.Message);
            Assert.Equal(_settings.MailSettings.From.Address, client.Message.From.Address);
        }
        
        [Fact]
        public async Task SendAsync_ShouldAttachFiles()
        {
            var client = new FakeSmtpClient();
            var sender = GetSender(((_) => client));
            
            var emailData = new EmailData.Builder()
                .To("max@bluechilli.com")
                .Subject("Some subject")
                .Html("this is an html mail")
                .Attachment("test1.txt", "text1.txt")
                .Attachment("test2.txt", "text2.txt")
                .Attachment("test3.txt", "text3.txt")
                .Attachment("test4.txt", "text4.txt")
                .Build();
            
            var r = await sender.SendAsync(emailData);

            Assert.True(r.Success);
           

        }
        
        [Fact(Skip = "you need to use papercut or read smtp server to test this")]
        public async Task SendAsync_CanSendRealEmail()
        {
            var storageMock = new Mock<IFileStorage>();
            storageMock.Setup(c => c.GetContentAsync(It.IsAny<string>(), It.IsAny<StorageEncryptionKeys>(),CancellationToken.None))
                .ReturnsAsync(() => FileStorageResponse.Create("test.txt", 0, "text/plain", new MemoryStream()));

            long contentLength;
            string contentType;

            storageMock.Setup(c => c.GetContent(It.IsAny<string>(), It.IsAny<StorageEncryptionKeys>(), out contentLength, out contentType))
                .Returns(() => new MemoryStream());
        
            var sender = new EmailSender(
                _settings,
                storageMock.Object,
                (config) => new EmailClient(config.Host, config.Port, config.UserName, config.Password, config.EnableSsl),
                null
            );
             
            var emailData = new EmailData.Builder()
                .From(new EmailData_Address("support@bluechilli.com", "support"))
                .To("max@bluechilli.com")
                .Subject("Some subject")
                .Html("this is an html mail")
                .Build();
            
            var r = await sender.SendAsync(emailData);

            Assert.Empty(r.Error);
            Assert.True(r.Success);           
        }
        
           
        [Fact(Skip = "you need to use papercut or read smtp server to test this")]
        public async Task SendAsync_CanSendRealEmailWithAttachments()
        {
            var file = File.OpenRead(Path.Combine(TestHelper.GetTestFolder(), "test.txt"));
            var storageMock = new Mock<IFileStorage>();
            storageMock.Setup(c => c.GetContentAsync(It.IsAny<string>(), It.IsAny<StorageEncryptionKeys>(), CancellationToken.None))
                .ReturnsAsync(() => FileStorageResponse.Create("test.txt",  file.Length
                    , "text/plain", file));

            long contentLength;
            string contentType;

            storageMock.Setup(c => c.GetContent(It.IsAny<string>(), It.IsAny<StorageEncryptionKeys>(), out contentLength, out contentType))
                .Returns(() => new MemoryStream());
        
            var sender = new EmailSender(
                _settings,
                storageMock.Object,
                (config) => new EmailClient(config.Host, config.Port, config.UserName, config.Password, config.EnableSsl),
                null
            );
             
            var emailData = new EmailData.Builder()
                .To("max@bluechilli.com")
                .Subject("Some subject")
                .Html("this is an html mail")
                .Attachment("test.txt", "text.txt")
                .Build();
            
            var r = await sender.SendAsync(emailData);

            Assert.True(r.Success);          
        }
    }
}