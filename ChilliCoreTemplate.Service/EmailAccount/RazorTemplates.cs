using ChilliCoreTemplate.Models;
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
        public static RazorTemplate RegistrationComplete = new RazorTemplate("Emails/EmailAccount/RegistrationComplete");
        public static RazorTemplate AccountAlreadyRegistered = new RazorTemplate("Emails/EmailAccount/AccountAlreadyRegistered");
        public static RazorTemplate AccountNotRegistered = new RazorTemplate("Emails/EmailAccount/AccountNotRegistered");

        public static RazorTemplate ResetPassword = new RazorTemplate("Emails/EmailAccount/ResetPassword");
        public static RazorTemplate PasswordChanged = new RazorTemplate("Emails/EmailAccount/PasswordChanged");
        public static RazorTemplate OneTimePassword = new RazorTemplate("Emails/EmailAccount/OneTimePassword");
        public static RazorTemplate OneTimePassword_Sms = new RazorTemplate("Sms/User/OneTimePassword");

        public static RazorTemplate WelcomeEmail = new RazorTemplate("Emails/EmailAccount/WelcomeEmail");

        public static RazorTemplate InviteUser = new RazorTemplate("Emails/EmailAccount/InviteUser");

        public static RazorTemplate SendSmsViaEmail = new RazorTemplate("Emails/EmailAccount/SendSmsViaEmail");
        public static RazorTemplate ForgotPin = new RazorTemplate("Emails/EmailAccount/ForgotPin");

        public static RazorTemplate ErrorAlert = new RazorTemplate("Emails/Admin/ErrorAlert");
        public static RazorTemplate ErrorDaily = new RazorTemplate("Emails/Admin/ErrorDaily");

    }
}
