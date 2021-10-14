using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service
{   
    public partial class RazorTemplates
    {
        public static RazorTemplate PdfExample = new RazorTemplate("Pdfs/Example");
    }

    public static class RazorTemplateExtensions
    {
        public static void QueueMail(this RazorTemplate template, AccountService service, string email, IEmailTemplateDataModel model, IEnumerable<IEmailAttachment> attachments = null, EmailData_Address replyTo = null, EmailData_Address from = null, List<EmailData_Address> bcc = null)
        {
            service.QueueMail(template, email, model, attachments, replyTo, from, bcc);
        }
    }

}
