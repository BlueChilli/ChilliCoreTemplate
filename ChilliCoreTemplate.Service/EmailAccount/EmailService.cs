using AutoMapper;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using DataTables.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public static  class EmailServiceHelpers
    {
        public static void SetConfigProperties(IEmailTemplateDataModel model, ProjectSettings config, string userEmail)
        {
            model.Site = config.ProjectDisplayName;
            model.UserEmail = userEmail;

            if (String.IsNullOrEmpty(model.Email)) model.Email = config.EmailTemplate.Email;
            if (String.IsNullOrEmpty(model.CompanyName)) model.CompanyName = config.ProjectDisplayName;
            if (String.IsNullOrEmpty(model.PublicUrl)) model.PublicUrl = config.PublicUrl;
        }
    }

    public partial class AccountService
    {
        public void QueueMail(RazorTemplate template, string email, IEmailTemplateDataModel model, IEnumerable<IEmailAttachment> attachments = null, EmailData_Address replyTo = null, EmailData_Address from = null, List<EmailData_Address> bcc = null)
        {
            EmailServiceHelpers.SetConfigProperties(model, _config, email);
            model.IsApi = this.IsApi;

            List<IEmailAttachment> memoryAttachments = new List<IEmailAttachment>();

            if (attachments != null)
            {
                foreach (var a in attachments)
                {
                    memoryAttachments.Add(a.CopyStreamToMemory());
                }
            }

            var item = new EmailQueueItem<IEmailTemplateDataModel>.Builder()
                    .To(email)
                    .Subject(model.Subject)
                    .UseData(model)
                    .UseTemplate(template)
                    .ForUser(model.UserId)
                    .ReplyTo(replyTo)
                    .From(from)
                    .Bcc(bcc)
                    .Attachments(memoryAttachments)
                    .Build();

            //Email generation and persistence happens in the background, so we can return to the caller immediately
            _backgroundTaskQueue.QueueBackgroundWorkItem(async (ct) => await QueueMail(item));
        }

        private static async Task QueueMail(EmailQueueItem<IEmailTemplateDataModel> item)
        {
            using (var scope = ScopeContextFactory.Instance.CreateScope())
            {
                var emailQueue = scope.ServiceProvider.GetRequiredService<IEmailQueue>();
                await emailQueue.Enqueue(item);
            }
        }

        /// <summary>
        /// Donn't send email if it has already been set recently (defaults to 1 week)
        /// </summary>
        public void QueueMail_Distinct(RazorTemplate template, string email, IEmailTemplateDataModel model, TimeSpan? span = null)
        {
            if (span == null) span = new TimeSpan(7, 0, 0, 0);

            var oneWeekAgo = DateTime.UtcNow.AddTicks(span.Value.Ticks * -1);
            var emailId = new Email(template);
            if (!Context.Emails.Any(e => e.Recipient == email && e.DateQueued > oneWeekAgo && e.TemplateIdHash == emailId.TemplateIdHash && e.TemplateId == emailId.TemplateId))
            {
                QueueMail(template, email, model);
            };
        }

        public ServiceResult<EmailViewModel> Email_Get(int id)
        {
            var email = Context.Emails.FirstOrDefault(e => e.Id == id);
            var model = GetSingle<EmailViewModel, Email>(email);
            model.Data = this._fileStorage.GetContent(email.Model).ReadToByteArray().To<EmailData>();

            return ServiceResult<EmailViewModel>.AsSuccess(model);
        }

        public PagedList<EmailSummaryModel> Email_Search(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo)
        {
            dateFrom = dateFrom.FromUserTimezone();
            dateTo = dateTo.FromUserTimezone().Add(new TimeSpan(23, 59, 59));
            var templateQuery = model.Columns.First(c => c.Field == "templateId").Search.Value;
            var templateQueryHash = String.IsNullOrEmpty(templateQuery) ? 0 : templateQuery.GetIndependentHashCode();
            var isOpenedValue = model.Columns.First(c => c.Field == "isOpened").Search.Value;
            bool? isOpened = String.IsNullOrEmpty(isOpenedValue) ? null : bool.Parse(isOpenedValue).ToNullable<bool>();
            var isClickedValue = model.Columns.First(c => c.Field == "isClicked").Search.Value;
            bool? isClicked = String.IsNullOrEmpty(isClickedValue) ? null : bool.Parse(isClickedValue).ToNullable<bool>();

            var query = Context.Emails
                    .Where(e => e.DateQueued > dateFrom && e.DateQueued < dateTo);

            if (!String.IsNullOrEmpty(model.Search.Value)) query = query.Where(e => e.Recipient.Contains(model.Search.Value));
            if (templateQueryHash != 0) query = query.Where(e => e.TemplateIdHash == templateQueryHash);
            if (isOpened != null) query = query.Where(e => e.IsOpened == isOpened);
            if (isClicked != null) query = query.Where(e => e.IsClicked == isClicked);

            IOrderedQueryable<Email> queryOrdered = null;
            var sortColumn = model.Columns.FirstOrDefault(c => c.Sort != null);
            if (sortColumn == null || sortColumn.Field == "dateQueuedDisplay")
            {
                queryOrdered = sortColumn.Sort?.Direction == SortDirection.Ascending
                    ? query.OrderBy(e => e.DateQueued)
                    : query.OrderByDescending(e => e.DateQueued);
            }
            else
            {
                if (sortColumn.Field == "recipient")
                {
                    queryOrdered = sortColumn.Sort.Direction == SortDirection.Ascending
                        ? query.OrderBy(e => e.Recipient)
                        : query.OrderByDescending(e => e.Recipient);
                }
                else if (sortColumn.Field == "isSent")
                {
                    queryOrdered = sortColumn.Sort.Direction == SortDirection.Ascending
                        ? query.OrderBy(e => e.IsSent)
                        : query.OrderByDescending(e => e.IsSent);
                }
            }

            return GetPagedList<EmailSummaryModel, Email>(queryOrdered, model.Start / model.Length + 1, model.Length);
        }

        public int Email_Count()
        {
            return Context.Emails.Count();
        }

        public void Email_Open(ShortGuid id)
        {
            var email = Context.Emails.FirstOrDefault(e => e.TrackingId == id.Guid);

            if (email != null)
            {
                email.OpenCount++;
                if (!email.IsOpened)
                {
                    email.IsOpened = true;
                    email.OpenDate = DateTime.UtcNow;
                }
                Context.SaveChanges();
            }
        }

        public ServiceResult Email_Clicked(ShortGuid id)
        {
            if (id.Guid == FakeTrackingId) return ServiceResult.AsSuccess();

            var email = Context.Emails.FirstOrDefault(e => e.TrackingId == id.Guid);

            if (email != null)
            {
                email.ClickCount++;
                if (!email.IsClicked)
                {
                    email.IsClicked = true;
                    email.ClickDate = DateTime.UtcNow;
                }
                Context.SaveChanges();
                return ServiceResult.AsSuccess();
            }
            return ServiceResult.AsError();
        }

        public ServiceResult<EmailViewModel> Email_Resend(int id)
        {
            var email = Context.Emails.FirstOrDefault(e => e.Id == id);
            email.IsSent = false;
            email.IsOpened = false;
            email.IsClicked = false;
            Context.SaveChanges();

            return ServiceResult<EmailViewModel>.AsSuccess(GetSingle<EmailViewModel, Email>(email));
        }

        public ServiceResult<EmailUnsubscribeModel> Email_GetForUnsubscribe(Guid trackingId)
        {
            var email = Context.Emails.FirstOrDefault(e => e.TrackingId == trackingId);

            if (email == null || String.IsNullOrEmpty(email.Model)) return ServiceResult<EmailUnsubscribeModel>.AsError("Email not found");

            return ServiceResult<EmailUnsubscribeModel>.AsSuccess(Mapper.Map<EmailUnsubscribeModel>(email));
        }

        public ServiceResult<EmailUnsubscribeModel> Email_Unsubscribe(EmailUnsubscribeModel model)
        {
            var email = Context.Emails.FirstOrDefault(e => e.Id == model.Id && e.Recipient == model.Recipient);
            if (!email.IsUnsubscribed)
            {
                email.IsUnsubscribed = true;
                email.UnsubscribeDate = DateTime.UtcNow;
                Context.SaveChanges();

                var user = GetAccountByEmail(email.Recipient);
                if (user != null)
                {
                    if (!Context.EmailUsers.Any(x => x.UserId == user.Id && x.TemplateIdHash == email.TemplateIdHash && x.TemplateId == email.TemplateId && x.IsUnsubscribed))
                    {
                        Context.EmailUsers.Add(new EmailUser
                        {
                            UserId = user.Id,
                            TemplateId = email.TemplateId,
                            TemplateIdHash = email.TemplateIdHash,
                            IsUnsubscribed = true,
                            UnsubscribeDate = DateTime.UtcNow,
                            Reason = model.Reason,
                            ReasonOther = model.ReasonOther
                        });
                        Context.SaveChanges();
                    }
                }
            }
            return ServiceResult<EmailUnsubscribeModel>.AsSuccess(model);
        }

        public ServiceResult<EmailPreviewModel> Email_Preview()
        {
            var model = new EmailPreviewModel
            {
                Emails = new List<EmailPreviewItemModel>
                {
                    new EmailPreviewItemModel
                    {
                        Template = RazorTemplates.AccountAlreadyRegistered,
                        Data = _config.AdminEmail
                    },
                    new EmailPreviewItemModel
                    {
                        Template = RazorTemplates.AccountNotRegistered,
                        Data = _config.AdminEmail
                    },
                    new EmailPreviewItemModel
                    {
                        Template = RazorTemplates.InviteUser,
                        Data = new InviteEditModel
                        {
                            Email = _config.AdminEmail,
                            FirstName = "Jim",
                            LastName = "Smith",
                            InviteRole = new InviteRoleViewModel { Role = Role.CompanyAdmin },
                            Token = "ABC123",
                            Inviter = "Lisa Dodgy"
                        }
                    },
                    new EmailPreviewItemModel
                    {
                        Template = RazorTemplates.PasswordChanged,
                        Data = new AccountViewModel { FirstName = "Jim" }
                    },
                    new EmailPreviewItemModel
                    {
                        Template = RazorTemplates.RegistrationComplete,
                        Data = new RegistrationCompleteViewModel { Email = _config.AdminEmail, FirstName = "Jim", Token = "ABC123" }
                    },
                    new EmailPreviewItemModel
                    {
                        Template = RazorTemplates.ResetPassword,
                        Data = new ResetPasswordRequestModel { Email = _config.AdminEmail, Token = Guid.NewGuid() }
                    },
                    new EmailPreviewItemModel
                    {
                        Template = RazorTemplates.WelcomeEmail,
                        Data =  new RegistrationCompleteViewModel
                        {
                            FirstName = "Developer",
                            Email = _config.AdminEmail,
                            Token = "ABC123"
                        }
                    },
                }
            };
            model.EmailList = model.Emails.ToSelectList(v => (int?)model.Emails.IndexOf(v), t => String.IsNullOrEmpty(t.EmailName) ? t.Id.SplitByUppercase() : t.EmailName);

            return ServiceResult<EmailPreviewModel>.AsSuccess(model);
        }

        private Guid FakeTrackingId => new Guid("D547C67A-EB4D-4EA9-AF51-077E1389FECB");

        public ServiceResult<EmailViewModel> Email_Preview(EmailPreviewModel model)
        {
            var email = Email_Preview().Result.Emails[model.Id.Value];
            IEmailTemplateDataModel templateModel = null;

            if (email.Template == RazorTemplates.AccountAlreadyRegistered)
            {
                templateModel = new RazorTemplateDataModel<string> { Data = model.Data.FromJson<string>() };
            }
            else if (email.Template == RazorTemplates.AccountNotRegistered)
            {
                templateModel = new RazorTemplateDataModel<string> { Data = model.Data.FromJson<string>() };
            }
            else if (email.Template == RazorTemplates.InviteUser)
            {
                templateModel = new RazorTemplateDataModel<InviteEditModel> { Data = model.Data.FromJson<InviteEditModel>() };
            }
            else if (email.Template == RazorTemplates.PasswordChanged)
            {
                templateModel = new RazorTemplateDataModel<AccountViewModel> { Data = model.Data.FromJson<AccountViewModel>() };
            }
            else if (email.Template == RazorTemplates.RegistrationComplete)
            {
                templateModel = new RazorTemplateDataModel<RegistrationCompleteViewModel> { Data = model.Data.FromJson<RegistrationCompleteViewModel>() };
            }
            else if (email.Template == RazorTemplates.ResetPassword)
            {
                templateModel = new RazorTemplateDataModel<ResetPasswordRequestModel> { Data = model.Data.FromJson<ResetPasswordRequestModel>() };
            }
            else if (email.Template == RazorTemplates.WelcomeEmail)
            {
                templateModel = new RazorTemplateDataModel<RegistrationCompleteViewModel>
                {
                    Data = model.Data.FromJson<RegistrationCompleteViewModel>()
                };
            }
            templateModel.TemplateId = email.Id;
            templateModel.TrackingId = FakeTrackingId.ToShortGuid();
            EmailServiceHelpers.SetConfigProperties(templateModel, _config, _config.AdminEmail);

            var html = TaskHelper.GetResultSafeSync(() => _templateViewRenderer.RenderAsync(email.Template.TemplateName, templateModel));

            var data = new EmailData.Builder()
                        .To("fake@example.com")
                        .Subject(templateModel.Subject)
                        .Html(html)
                        .Build();

            return ServiceResult<EmailViewModel>.AsSuccess(new EmailViewModel
            {
                TemplateId = email.Id,
                Data = data
            });
        }

    }
}
