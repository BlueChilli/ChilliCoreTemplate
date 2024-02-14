﻿using AutoMapper;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using DataTables.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public static  class EmailServiceHelpers
    {
        public static void LinqMapperConfigure()
        {
            LinqMapper.CreateMap<Email, EmailSummaryModel>();
        }

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
        public void QueueUserMail(RazorTemplate template, string email, IEmailTemplateDataModel model, IEnumerable<IEmailAttachment> attachments = null, EmailData_Address replyTo = null, EmailData_Address from = null, List<EmailData_Address> bcc = null)
        {
            if (from == null) from = new EmailData_Address(_config.EmailTemplate.Email, $"{User.UserData().Name} via {_config.ProjectDisplayName}");
            if (replyTo == null) replyTo = new EmailData_Address(User.UserData().Email);

            QueueMail(template, email, model, attachments, replyTo, from, bcc);
        }

        public void QueueUserMail(RazorTemplate template, string userName, string userEmail, string email, IEmailTemplateDataModel model, IEnumerable<IEmailAttachment> attachments = null, List<EmailData_Address> bcc = null)
        {
            var from = new EmailData_Address(_config.EmailTemplate.Email, $"{userName} via {_config.ProjectDisplayName}");
            var replyTo = new EmailData_Address(userEmail);

            QueueMail(template, email, model, attachments, replyTo, from, bcc);
        }

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

        public ServiceResult<EmailListModel> Email_List()
        {
            var model = new EmailListModel();
            model.TemplateList = Context.Emails.Select(x => x.TemplateId).Distinct().ToList().ToSelectList();
            return ServiceResult<EmailListModel>.AsSuccess(model);
        }

        public ServiceResult<EmailViewModel> Email_Get(int id)
        {
            var email = Context.Emails.FirstOrDefault(e => e.Id == id);
            var model = _mapper.Map<EmailViewModel>(email);
            model.Data = this._fileStorage.GetContent(email.Model).DeserializeTo<EmailData>();
            model.Data.Attachments.ForEach(x => model.Attachments.Add(x.FileName, _fileStoragePath.GetPreSignedUrl(x.Path)));

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

            return queryOrdered
                .Materialize<Email, EmailSummaryModel>()
                .ToPagedList(model.Start / model.Length + 1, model.Length);
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
                    email.Error = null;
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

            return ServiceResult<EmailViewModel>.AsSuccess(_mapper.Map<EmailViewModel>(email));
        }

        public ServiceResult<EmailUnsubscribeModel> Email_GetForUnsubscribe(Guid trackingId)
        {
            var record = Context.Emails
                .Where(e => e.TrackingId == trackingId)
                .Materialize<Email, EmailUnsubscribeModel>()
                .FirstOrDefault();

            if (record == null) return ServiceResult<EmailUnsubscribeModel>.AsError("Email not found");

            return ServiceResult<EmailUnsubscribeModel>.AsSuccess(record);
        }

        public ServiceResult<EmailUnsubscribeModel> Email_Unsubscribe(EmailUnsubscribeModel model)
        {
            var record = Context.Emails
                .FirstOrDefault(e => e.TrackingId == model.Id.Guid);

            if (record != null && !record.IsUnsubscribed)
            {
                record.IsUnsubscribed = true;
                record.UnsubscribeDate = DateTime.UtcNow;
                Context.SaveChanges();

                if (record.UserId.HasValue)
                {
                    if (!Context.EmailUsers.Any(x => x.UserId == record.UserId && x.TemplateIdHash == record.TemplateIdHash && x.TemplateId == record.TemplateId && x.IsUnsubscribed))
                    {
                        Context.EmailUsers.Add(new EmailUser
                        {
                            UserId = record.UserId.Value,
                            //CompanyId = record.CampaignStage?.Campaign?.PartnerId,
                            TemplateId = record.TemplateId,
                            TemplateIdHash = record.TemplateIdHash,
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

            if (email.Template == RazorTemplates.AccountAlreadyRegistered || email.Template == RazorTemplates.AccountNotRegistered)
            {
                email.Data = model.Data.FromJson<string>();
                return Email_Preview<string>(email);
            }
            else if (email.Template == RazorTemplates.InviteUser)
            {
                email.Data = model.Data.FromJson<InviteEditModel>();
                return Email_Preview<InviteEditModel>(email);
            }
            else if (email.Template == RazorTemplates.PasswordChanged)
            {
                email.Data = model.Data.FromJson<AccountViewModel>();
                return Email_Preview<AccountViewModel>(email);
            }
            else if (email.Template == RazorTemplates.ResetPassword)
            {
                email.Data = model.Data.FromJson<ResetPasswordRequestModel>();
                return Email_Preview<ResetPasswordRequestModel>(email);
            }
            else if (email.Template == RazorTemplates.RegistrationComplete || email.Template == RazorTemplates.WelcomeEmail)
            {
                email.Data = model.Data.FromJson<RegistrationCompleteViewModel>();
                return Email_Preview<RegistrationCompleteViewModel>(email);
            }

            return ServiceResult<EmailViewModel>.AsError("Template not found");
        }

        internal ServiceResult<EmailViewModel> Email_Preview<T>(EmailPreviewItemModel email) where T : class
        {
            var templateModel = new RazorTemplateDataModel<T>(email.Data as T);
            templateModel.TemplateId = email.Id;
            templateModel.TrackingId = FakeTrackingId.ToShortGuid();
            templateModel.CompanyId = email.CompanyId;
            templateModel.CompanyName = email.CompanyName;
            templateModel.Logo = email.CompanyLogo;
            EmailServiceHelpers.SetConfigProperties(templateModel, _config, _config.AdminEmail);

            var html = TaskHelper.GetResultSafeSync(() => _templateViewRenderer.RenderAsync(email.Template.TemplateName, templateModel));

            MjmlToHtmlHelper.Render(ref html);

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
