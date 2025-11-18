using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            var company = companyId.HasValue ? Context.Companies.First(c => c.Id == companyId.Value) : null;
            QueueCompanyMail(company, template, to, model, attachments, from, bcc);
        }

        public void QueueCompanyMail(Company company, RazorTemplate template, string to, IEmailTemplateDataModel model, List<IEmailAttachment> attachments = null, EmailData_Address from = null, EmailData_Address bcc = null)
        {
            EmailData_Address replyTo = null;
            if (company != null)
            {
                model.CompanyId = company.Id;
                model.CompanyName = company.Name;
                model.Logo = String.IsNullOrEmpty(company.LogoPath) ? null : _fileStoragePath.GetImagePath(company.LogoPath, fullPath: true);
                model.PublicUrl = company.Website;
                model.Email = _config.EmailTemplate.Email;
                if (from == null) from = new EmailData_Address(model.Email, $"{company.Name} via {_config.ProjectDisplayName}");

                var companyAdmin = GetCompanyAdmin(company.Id);
                model.CompanyEmail = companyAdmin?.Email;
                //replyTo = new EmailData_Address(_config.EmailTemplate.Email, _config.ProjectDisplayName);
            }
            if (String.IsNullOrEmpty(model.CompanyName)) model.CompanyName = _config.ProjectDisplayName;
            if (String.IsNullOrEmpty(model.PublicUrl)) model.PublicUrl = _config.PublicUrl;
            _email.QueueMail(template, to, model, attachments, replyTo, from, bcc == null ? null : new List<EmailData_Address> { bcc });
        }

        public void QueueCompanyAdminsMail(int companyId, RazorTemplate template, IEmailTemplateDataModel model)
        {
            var company = Context.Companies
                .AsNoTracking()
                .Include(c => c.UserRoles).ThenInclude(c => c.User)
                .Where(c => c.Id == companyId)
                .FirstOrDefault();

            model.CompanyName = company.Name;
            //model.Logo = String.IsNullOrEmpty(company.LogoPath) ? null : _fileStoragePath.GetImagePath(company.LogoPath, fullPath: true) + "?h=75";
            //model.PublicUrl = company.Website;
            model.Email = _config.EmailTemplate.Email;

            var to = company.UserRoles
                .Where(x => x.Role == Role.CompanyAdmin && x.Status == null && x.User.Status != UserStatus.Deleted)
                .Select(x => x.User.Email)
                .ToList();
            if (!to.Any()) to.Add(_config.AdminEmail);
            foreach (var email in to)   //Warning Layout or email can modify data, becareful this works in a loop.
                _email.QueueMail(template, email, model);
        }

        public List<UserBasicModel> GetCompanyAdmins(int companyId)
        {
            return GetCompanyAdmins(Context, companyId);
        }

        public static List<UserBasicModel> GetCompanyAdmins(DataContext context, int companyId)
        {
            return GetCompanyAdminsQuery(context, companyId)
                .Materialize<User, UserBasicModel>()
                .ToList();
        }

        internal User GetCompanyAdmin(int companyId)
        {
            return GetCompanyAdmin(Context, companyId);
        }

        internal static User GetCompanyAdmin(DataContext context, int companyId)
        {
            return GetCompanyAdminsQuery(context, companyId)
                .Include(x => x.UserRoles).ThenInclude((UserRole r) => r.Company)
                .FirstOrDefault();
        }

        private static IQueryable<User> GetCompanyAdminsQuery(DataContext context, int companyId)
        {
            return context.Users
                .Include(x => x.UserRoles)
                .Where(x => x.UserRoles.Any(r => r.CompanyId == companyId && r.Role.HasFlag(Role.CompanyAdmin)) && x.Status != UserStatus.Deleted);
        }

        internal static SelectList AdminEmailList(DataContext context, int companyId)
        {
            return GetCompanyAdminsQuery(context, companyId).OrderBy(x => x.Email).Select(x => new { x.Id, x.Email }).ToSelectList(v => v.Id, t => t.Email);
        }

        public UserDataPrincipal CreateCompanyPrincipal(int companyId)
        {
            return CreatePrincipal(GetCompanyAdmin(companyId), null);
        }
    }
}
