using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public partial class AccountService
    {

        public void QueueCompanyMail(RazorTemplate template, string to, IEmailTemplateDataModel model, List<IEmailAttachment> attachments = null, EmailData_Address from = null, EmailData_Address bcc = null)
        {
            QueueCompanyMail(User.UserData().CompanyId, template, to, model, attachments, from, bcc);
        }

        public void QueueCompanyMail(int? companyId, RazorTemplate template, string to, IEmailTemplateDataModel model, List<IEmailAttachment> attachments = null, EmailData_Address from = null, EmailData_Address bcc = null)
        {
            if (companyId.HasValue)
            {
                var company = Context.Companies.First(c => c.Id == companyId.Value);
                model.CompanyId = company.Id;
                model.CompanyName = company.Name;
                model.Logo = String.IsNullOrEmpty(company.LogoPath) ? null : _fileStoragePath.GetImagePath(company.LogoPath, fullPath: true) + "?h=75";
                model.PublicUrl = company.Website;
                model.Email = _config.EmailTemplate.Email;
                if (from == null) from = new EmailData_Address(model.Email, $"{company.Name} via {_config.ProjectDisplayName}");

                var companyAdmin = GetCompanyAdmin(companyId.Value);
                model.CompanyEmail = companyAdmin.Email;
            }
            if (String.IsNullOrEmpty(model.CompanyName)) model.CompanyName = _config.ProjectDisplayName;
            if (String.IsNullOrEmpty(model.PublicUrl)) model.PublicUrl = _config.PublicUrl;
            QueueMail(template, to, model, attachments, null, from, new List<EmailData_Address> { bcc });

        }

        internal User GetCompanyAdmin(int companyId)
        {
            return Context.Users
                .Include(x => x.UserRoles)
                .Where(x => x.UserRoles.Any(r => r.CompanyId == companyId && r.Role.HasFlag(Role.CompanyAdmin)) && x.Status != UserStatus.Deleted)
                .FirstOrDefault();

        }

    }
}
