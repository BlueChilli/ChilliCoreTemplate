using ChilliCoreTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ChilliCoreTemplate.Web
{
    public static class Mvc
    {
        public static readonly RootActions Root = new RootActions();
        public static readonly AdminActions Admin = new AdminActions();
        public static readonly CompanyActions Company = new CompanyActions();
    }

    public class RootActions : MvcActionContainer
    {
        public RootActions() : base(area: null) { }

        public readonly IMvcActionDefinition Public_Index = MvcAction();
        public readonly IMvcActionDefinition Public_ConfirmationModal = MvcAction();

        public readonly IMvcActionDefinition EmailAccount_Login = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_LoginOAuth = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_LoginWithToken = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_ChooseRole = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_SelectRole = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_Registration = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_RegistrationOAuth = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_RegistrationActivationSent = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_RegistrationComplete = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_ForgotPassword = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_ForgotPasswordSent = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_ResetPassword = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_ResetPasswordSuccess = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_ConfirmInvite = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_ConfirmInviteSuccess = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_Details = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_ChangePassword = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_ChangeDetails = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_Logout = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_EmailRedirect = MvcAction();
        public readonly IMvcActionDefinition EmailAccount_EmailUnsubscribe = MvcAction();

        public readonly IMvcActionDefinition Entry_Index = MvcAction();
        public readonly IMvcActionDefinition Entry_ImpersonateRedirect = MvcAction();
    }

    public class AdminActions : MvcActionContainer
    {
        public AdminActions() : base(area: "Admin") { }

        public readonly IMvcActionDefinition Default = MvcAction();

        public readonly IMvcActionDefinition Company = MvcAction();
        public readonly IMvcActionDefinition Company_List = MvcAction();
        public readonly IMvcActionDefinition Company_ListData = MvcAction();
        public readonly IMvcActionDefinition Company_ListJson = MvcAction();
        public readonly IMvcActionDefinition Company_Edit = MvcAction();
        public readonly IMvcActionDefinition Company_Detail = MvcAction();
        public readonly IMvcActionDefinition Company_Delete = MvcAction();
        public readonly IMvcActionDefinition Company_Purge = MvcAction();
        public readonly IMvcActionDefinition Company_Admin_List = MvcAction();
        public readonly IMvcActionDefinition Company_Admin_Detail = MvcAction();
        public readonly IMvcActionDefinition Company_Admin_Add = MvcAction();
        public readonly IMvcActionDefinition Company_Admin_Remove = MvcAction();
        public readonly IMvcActionDefinition Company_Admin_Resend = MvcAction();
        public readonly IMvcActionDefinition Company_Impersonate = MvcAction();

        public readonly IMvcActionDefinition User_Users = MvcAction();
        public readonly IMvcActionDefinition User_Users_Query = MvcAction();
        public readonly IMvcActionDefinition User_Users_Json = MvcAction();
        public readonly IMvcActionDefinition User_Users_Details = MvcAction();
        public readonly IMvcActionDefinition User_Statistics = MvcAction();
        public readonly IMvcActionDefinition User_Impersonate = MvcAction();
        public readonly IMvcActionDefinition User_UndoImpersonate = MvcAction();
        public readonly IMvcActionDefinition User_ResetPassword = MvcAction();
        public readonly IMvcActionDefinition User_ChangeDetails = MvcAction();
        public readonly IMvcActionDefinition User_ChangeStatus = MvcAction();
        public readonly IMvcActionDefinition User_ChangeRole = MvcAction();
        public readonly IMvcActionDefinition User_Purge = MvcAction();
        public readonly IMvcActionDefinition User_Activity = MvcAction();
        public readonly IMvcActionDefinition User_ActivityQuery = MvcAction();
        public readonly IMvcActionDefinition User_ActivityDetail = MvcAction();
        public readonly IMvcActionDefinition User_Invite = MvcAction();
        public readonly IMvcActionDefinition User_InviteResend = MvcAction();
        public readonly IMvcActionDefinition User_InviteUpload = MvcAction();
        public readonly IMvcActionDefinition User_Import = MvcAction();
        public readonly IMvcActionDefinition User_ImportResult = MvcAction();
        public readonly IMvcActionDefinition User_Emails = MvcAction();
        public readonly IMvcActionDefinition User_EmailsQuery = MvcAction();
        public readonly IMvcActionDefinition User_EmailsDetail = MvcAction();
        public readonly IMvcActionDefinition User_EmailsResend = MvcAction();
        public readonly IMvcActionDefinition User_EmailsPreview = MvcAction();
        public readonly IMvcActionDefinition User_EmailsPreviewShow = MvcAction();
        public readonly IMvcActionDefinition User_Sms_List = MvcAction();
        public readonly IMvcActionDefinition User_Sms_Query = MvcAction();
        public readonly IMvcActionDefinition User_Sms_Detail = MvcAction();
        public readonly IMvcActionDefinition User_Notification_List = MvcAction();
        public readonly IMvcActionDefinition User_Notification_Query = MvcAction();
        public readonly IMvcActionDefinition User_Notification_Detail = MvcAction();
        public readonly IMvcActionDefinition User_Error_List = MvcAction();
        public readonly IMvcActionDefinition User_Error_Query = MvcAction();
        public readonly IMvcActionDefinition User_Error_Detail = MvcAction();
    }

    public class CompanyActions : MvcActionContainer
    {
        public CompanyActions() : base(area: "Company") { }

        public readonly IMvcActionDefinition Default = MvcAction();

        public readonly IMvcActionDefinition User_List = MvcAction();
        public readonly IMvcActionDefinition User_ListData = MvcAction();
        public readonly IMvcActionDefinition User_Detail = MvcAction();
        public readonly IMvcActionDefinition User_Impersonate = MvcAction();
        public readonly IMvcActionDefinition User_UndoImpersonate = MvcAction();
        public readonly IMvcActionDefinition User_ResetPassword = MvcAction();
        public readonly IMvcActionDefinition User_ChangeDetails = MvcAction();
        public readonly IMvcActionDefinition User_ChangeStatus = MvcAction();
        public readonly IMvcActionDefinition User_ChangeRole = MvcAction();
        public readonly IMvcActionDefinition User_Invite = MvcAction();
        public readonly IMvcActionDefinition User_InviteResend = MvcAction();
        public readonly IMvcActionDefinition User_InviteUpload = MvcAction();

        public readonly IMvcActionDefinition Company_Detail = MvcAction();
        public readonly IMvcActionDefinition Company_Edit = MvcAction();
    }
}