using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace ChilliCoreTemplate.Models.EmailAccount
{
    [Serializable]
    public class EmailQueueItem<T>
    {
        private EmailQueueItem(
            RazorTemplate template,
            string email,
            T model,
            IEnumerable<IEmailAttachment> attachments = null)
        {
            Email = email;
            Model = model;
            Attachments = attachments;
            Template = template;

        }

        public T Model { get; }

        public string Email { get; }

        public RazorTemplate Template { get; }

        public EmailData_Address ReplyTo { get; private set; }
        public EmailData_Address From { get; private set; }
        public List<EmailData_Address> Bcc { get; private set; }

        public string Subject { get; private set; }

        public int? UserId { get; private set; }

        public IEnumerable<IEmailAttachment> Attachments { get; }

        public class Builder
        {
            private string _email;
            private T _model;
            private RazorTemplate _template;
            private EmailData_Address _replyTo;
            private List<EmailData_Address> _bcc;
            private EmailData_Address _from;
            private string _subject;
            private List<IEmailAttachment> _attachments;
            private int? _userId;

            public Builder()
            {
                _attachments = new List<IEmailAttachment>();
            }

            public Builder To(string address)
            {
                _email = address;

                return this;
            }


            public Builder From(EmailData_Address address)
            {
                _from = address;
                return this;
            }

            public Builder From(string address, string displayName)
            {
                _from = new EmailData_Address(address, displayName);
                return this;
            }

            public Builder Subject(string subject)
            {
                _subject = subject;
                return this;
            }

            public Builder ReplyTo(EmailData_Address address)
            {
                _replyTo = address;
                return this;
            }

            public Builder ReplyTo(string address, string displayName)
            {
                _replyTo = new EmailData_Address(address, displayName);
                return this;
            }

            public Builder Bcc(List<EmailData_Address> addresses)
            {
                _bcc = addresses;
                return this;
            }


            public Builder Bcc(EmailData_Address address)
            {
                _bcc = new List<EmailData_Address> { address };
                return this;
            }

            public Builder Bcc(string address, string displayName)
            {
                _bcc = new List<EmailData_Address> { new EmailData_Address(address, displayName) };
                return this;
            }

            public Builder Attachments(IEnumerable<IEmailAttachment> attachments)
            {
                _attachments.Clear();
                _attachments.AddRange(attachments);
                return this;
            }

            public Builder Attachment(EmailAttachment attachment)
            {
                _attachments.Add(attachment);
                return this;
            }

            public Builder Attachment(string filename, string path)
            {
                _attachments.Add(new EmailAttachment(filename, path));
                return this;
            }

            public Builder Attachment(Stream stream, string filename, string mimeType)
            {
                _attachments.Add(new EmailAttachment(stream, filename, mimeType));
                return this;
            }

            public Builder UseTemplate(RazorTemplate template)
            {
                _template = template;
                return this;
            }

            public Builder UseData(T data)
            {
                _model = data;
                return this;
            }

            public Builder ForUser(int? userId)
            {
                _userId = userId;
                return this;
            }

            public EmailQueueItem<T> Build()
            {
                if (String.IsNullOrWhiteSpace(_email))
                {
                    throw new ArgumentException("Email address is required. set it by To()");
                }

                if (_template == null)
                {
                    throw new ArgumentException("Template is required. set it by UseTemplate()");
                }

                if (_model == null)
                {
                    throw new ArgumentException("Data is required. set it by UseData()");
                }


                return new EmailQueueItem<T>(_template, _email, _model, _attachments)
                {
                    ReplyTo = _replyTo,
                    Bcc = _bcc,
                    From = _from,
                    Subject = _subject,
                    UserId = _userId
                };
            }

        }
    }

    public interface IEmailTemplateDataModel
    {
        string TemplateId { get; set; }
        string EmailPreview { get; set; }
        string TagLine { get; set; }
        string FileStoragePath { get; set; }
        bool HasSocialMedia { get; }
        string TwitterUrl { get; set; }
        string FacebookUrl { get; set; }
        string GooglePlusUrl { get; set; }
        string YoutubeUrl { get; set; }
        string LinkedInUrl { get; set; }
        string InstagramUrl { get; set; }
        string FooterTextColor { get; set; }
        string Subject { get; set; }
        ShortGuid TrackingId { get; set; }
        int? UserId { get; set; }
        string UserEmail { get; set; }

        string Site { get; set; }
        string CompanyName { get; set; }
        string PublicUrl { get; set; }
        string Logo { get; set; }
        string Email { get; set; }
        bool IsApi { get; set; }
    }


    public class RazorTemplateDataModel<T> : IEmailTemplateDataModel
    {
        public RazorTemplateDataModel()
        {
            FooterTextColor = "#000";
        }

        public RazorTemplateDataModel(T data) : base()
        {
            Data = data;
        }

        public T Data { get; set; }

        public ShortGuid TrackingId { get; set; }
        public string TemplateId { get; set; }
        public string Subject { get; set; }

        //public string BaseUrl { get; set; }
        //public string HostName { get; set; }

        public string Site { get; set; }
        public bool CanUnsubscribe { get; set; }
        public int? UserId { get; set; }
        public string UserEmail { get; set; }
        public bool IsApi { get; set; }

        #region Template config data
        /// <summary>
        /// Hidden text in the email used to display a brief summary of the email alongside the subject (if used by email client).
        /// Overrides email client default of extracing the first x characters of the email as the summary
        /// </summary>
        public string EmailPreview { get; set; }
        public string TagLine { get; set; }
        public string FileStoragePath { get; set; }
        public bool HasSocialMedia { get { return !String.IsNullOrEmpty(TwitterUrl) || !String.IsNullOrEmpty(FacebookUrl) || !String.IsNullOrEmpty(GooglePlusUrl) || !String.IsNullOrEmpty(YoutubeUrl) || !String.IsNullOrEmpty(LinkedInUrl) || !String.IsNullOrEmpty(InstagramUrl); } }
        public string TwitterUrl { get; set; }
        public string FacebookUrl { get; set; }
        public string GooglePlusUrl { get; set; }
        public string YoutubeUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string Email { get; set; }
        public string FooterTextColor { get; set; }
        public string CompanyName { get; set; }
        public string PublicUrl { get; set; }

        public string Logo { get; set; }
        #endregion
    }

    public interface IEmailAttachment
    {
        string Path { get; }
        string FileName { get; }

        [JsonIgnore]
        Stream Stream { get; }

        [JsonIgnore]
        byte[] File { get; }

        string MimeType { get; }

        IEmailAttachment CopyStreamToMemory();

        Task<IEmailAttachment> SaveAsync(string folder, Guid fileId, string fileName, IFileStorage fileStorage,
            StorageEncryptionKeys encryptionKeys = null,
            CancellationToken cancellationToken = default(CancellationToken));
        Task<IEmailAttachment> LoadAsync(IFileStorage fileStorage,
            StorageEncryptionKeys encryptionKeys = null,
            CancellationToken cancellationToken = default(CancellationToken));

        IEmailAttachment Load(IFileStorage fileStorage,
            StorageEncryptionKeys encryptionKeys = null);

        IEmailAttachment Save(string folder, Guid fileId, string fileName, IFileStorage fileStorage,
            StorageEncryptionKeys encryptionKeys = null);
    }

    [Serializable]
    public class EmailAttachment : IEmailAttachment
    {

        public EmailAttachment(string filename, string path)
        {
            Path = path;
            FileName = filename;
        }

        public EmailAttachment(Stream stream, string filename, string mimeType)
        {
            _stream = stream;
            FileName = filename;
            MimeType = mimeType;
        }

        public EmailAttachment(byte[] file, string filename, string mimeType)
        {
            _file = file;
            FileName = filename;
            MimeType = mimeType;
        }

        public string Path { get; }
        public string FileName { get; }


        public Stream Stream { get { return _stream; } }
        [NonSerialized]
        private Stream _stream;

        public byte[] File { get { return _file; } }
        [NonSerialized]
        private byte[] _file;

        public string MimeType { get; }

        public IEmailAttachment CopyStreamToMemory()
        {
            if (Stream != null)
            {
                var fileContent = Stream.ReadToByteArray();
                return new EmailAttachment(fileContent, FileName, MimeType);
            }

            return this;
        }

        public async Task<IEmailAttachment> SaveAsync(string folder, Guid fileId, string fileName, IFileStorage fileStorage,
            StorageEncryptionKeys encryptionKeys = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var storageFileName = $"{fileId}_{Guid.NewGuid()}.bin";

            if (Stream != null)
            {
                var path = await fileStorage.SaveAsync(new StorageCommand()
                {
                    FileName = storageFileName,
                    Folder = folder,
                    ContentType = MimeType,
                    EncryptionOptions = encryptionKeys != null ? new StorageEncryptionOptions(encryptionKeys.Secret, encryptionKeys.Salt) : null
                }.SetStreamSource(Stream), cancellationToken);

                return new EmailAttachment(fileName, path);
            }

            if (File != null)
            {
                var path = await fileStorage.SaveAsync(new StorageCommand()
                {
                    FileName = storageFileName,
                    Folder = folder,
                    ContentType = MimeType,
                    EncryptionOptions = encryptionKeys != null ? new StorageEncryptionOptions(encryptionKeys.Secret, encryptionKeys.Salt) : null
                }.SetByteArraySource(File), cancellationToken);

                return new EmailAttachment(fileName, path);
            }

            return this;
        }

        public async Task<IEmailAttachment> LoadAsync(IFileStorage fileStorage,
            StorageEncryptionKeys encryptionKeys = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Stream != null)
            {
                return this;
            }

            if (File != null)
            {
                return new EmailAttachment(new MemoryStream(File), FileName, MimeType);
            }

            if (!String.IsNullOrWhiteSpace(Path))
            {
                var response = await fileStorage.GetContentAsync(Path, encryptionKeys, cancellationToken);
                return new EmailAttachment(response.Stream, response.FileName, response.ContentType);
            }

            return null;
        }

        public IEmailAttachment Load(IFileStorage fileStorage, StorageEncryptionKeys encryptionKeys = null)
        {
            if (Stream != null)
            {
                return this;
            }

            if (File != null)
            {
                return new EmailAttachment(new MemoryStream(File), FileName, MimeType);
            }

            if (!String.IsNullOrWhiteSpace(Path))
            {
                var stream = fileStorage.GetContent(Path, encryptionKeys, out var contentLength, out var contentType);
                return new EmailAttachment(stream, FileName, contentType);
            }

            return null;
        }

        public IEmailAttachment Save(string folder, Guid fileId, string fileName, IFileStorage fileStorage, StorageEncryptionKeys encryptionKeys = null)
        {
            var storageFileName = $"{fileId}_{Guid.NewGuid()}.bin";

            if (Stream != null)
            {
                var path = fileStorage.Save(new StorageCommand()
                {
                    FileName = storageFileName,
                    Folder = folder,
                    ContentType = MimeType
                }.SetStreamSource(Stream));

                return new EmailAttachment(fileName, path);
            }

            if (File != null)
            {
                var path = fileStorage.Save(new StorageCommand()
                {
                    FileName = storageFileName,
                    Folder = folder,
                    ContentType = MimeType
                }.SetByteArraySource(File));

                return new EmailAttachment(fileName, path);
            }

            return this;
        }
    }

    [Serializable]
    public class EmailData
    {

        private EmailData()
        {
            _attachments = new List<IEmailAttachment>();
        }

        public string To { get; set; }
        public EmailData_Address From { get; private set; }
        public string Subject { get; private set; }
        public string MessageHtml { get; private set; }
        public string MessageText => HtmlToTextConverter.StripHtml(MessageHtml ?? "");
        public EmailData_Address ReplyTo { get; private set; }
        public List<EmailData_Address> Bcc { get; private set; }

        private List<IEmailAttachment> _attachments;
        public IReadOnlyList<IEmailAttachment> Attachments => _attachments.AsReadOnly();

        public override string ToString() => $"to: {To}, from:{From?.Address}, subject:{Subject}, message: {MessageHtml}, replyTo: {ReplyTo?.Address}, bcc: {Bcc?.Count}";

        public class Builder
        {
            private readonly EmailData _data;

            public Builder()
            {
                _data = new EmailData();
            }

            public Builder To(string address)
            {
                if (String.IsNullOrWhiteSpace(address))
                {
                    throw new ArgumentNullException("address");
                }

                _data.To = address;
                return this;
            }


            public Builder From(EmailData_Address address)
            {
                _data.From = address;
                return this;
            }

            public Builder From(string address, string displayName)
            {
                _data.From = new EmailData_Address(address, displayName);
                return this;
            }

            public Builder Subject(string subject)
            {
                _data.Subject = subject;
                return this;
            }

            public Builder Html(string message)
            {
                _data.MessageHtml = message;
                return this;
            }

            public Builder ReplyTo(EmailData_Address address)
            {
                _data.ReplyTo = address;
                return this;
            }

            public Builder ReplyTo(string address, string displayName)
            {
                _data.ReplyTo = new EmailData_Address(address, displayName);
                return this;
            }

            public Builder Bcc(List<EmailData_Address> addresses)
            {
                _data.Bcc = addresses;
                return this;
            }

            public Builder Attachments(IEnumerable<IEmailAttachment> attachments)
            {
                _data._attachments.AddRange(attachments);
                return this;
            }

            public Builder Attachment(EmailAttachment attachment)
            {
                _data._attachments.Add(attachment);
                return this;
            }

            public Builder Attachment(string filename, string path)
            {
                _data._attachments.Add(new EmailAttachment(filename, path));
                return this;
            }

            public Builder Attachment(Stream stream, string filename, string mimeType)
            {
                _data._attachments.Add(new EmailAttachment(stream, filename, mimeType));
                return this;
            }

            public EmailData Build()
            {
                if (String.IsNullOrWhiteSpace(_data.To))
                {
                    throw new ArgumentNullException("To");
                }

                return _data;
            }

        }
    }

    [Serializable]
    public class EmailData_Address
    {
        public EmailData_Address()
        {

        }

        public EmailData_Address(string address, string displayName = null)
        {
            Address = address;
            DisplayName = displayName;
        }

        public EmailData_Address(MailAddress address)
        {
            Address = address.Address;
            DisplayName = address.DisplayName;
        }

        public string Address { get; set; }

        public string DisplayName { get; set; }

        public MailAddress ToMailAddress() => new MailAddress(Address, DisplayName);

    }

    public class EmailSummaryModel
    {
        public int Id { get; set; }

        public string TemplateId { get; set; }

        public string Recipient { get; set; }

        public DateTime DateQueued { get; set; }

        public string DateQueuedDisplay { get { return DateQueued.ToTimezone().ToIsoDateTime(); } }

        public bool IsSent { get; set; }

        public bool IsOpened { get; set; }

        public bool IsClicked { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class EmailViewModel
    {
        public int Id { get; set; }

        public string TemplateId { get; set; }

        public string Recipient { get; set; }

        public DateTime DateQueued { get; set; }

        public bool IsSent { get; set; }

        public bool IsOpened { get; set; }

        public EmailData Data { get; set; }

        public string Error { get; set; }

        public string Preview()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(Data.MessageHtml);
            var node = doc.GetElementbyId("email-preview");
            if (node != null)
            {
                return node.InnerText.Replace("&amp;zwnj;&amp;nbsp;", "");
            }
            return String.Empty;
        }

    }

    public class EmailUnsubscribeModel : EmailViewModel
    {
        [Required, EmptyItem("Unsubscribe reason")]
        public EmailUnsubscribeReason? Reason { get; set; }

        [MaxLength(200), Placeholder("Reason")] //RequiredIf("Reason", EmailUnsubscribeReason.Other, ErrorMessage = "If reason is Other then a description of the reason is required")
        public string ReasonOther { get; set; }

    }

    public enum EmailUnsubscribeReason
    {
        [Description("You send too many emails")]
        Frequency = 1,
        [Description("The email content is not relevant to me")]
        Content,
        Other
    }

    public class EmailPreviewModel
    {
        [DisplayName("Email"), EmptyItem]
        public int? Id { get; set; }

        [DataType(DataType.MultilineText), MaxLength(5000)]
        public string Data { get; set; }

        public SelectList EmailList { get; set; }

        public List<EmailPreviewItemModel> Emails { get; set; }
    }

    public class EmailPreviewItemModel
    {
        public string Id { get { return Template.TemplateName.Substring(Template.TemplateName.LastIndexOf("/") + 1); } }

        public RazorTemplate Template { get; set; }

        public object Data { get; set; }

        /// <summary>
        /// Email name will default to template name, use this to override.
        /// </summary>
        public string EmailName { get; set; }
    }
}
