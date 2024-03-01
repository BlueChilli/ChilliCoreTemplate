using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web.MVC;
using ChilliSource.Core.Extensions;
using FoolProof.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Models.EmailAccount
{
    public class MfaSetupModel : MfaConfirmModel
    {
        public string SetupCode { get; set; }
        public string QrCodeImage { get; set; }
    }

    public class MfaConfirmModel
    {
        [MaxLength(100), Required]
        public string ConfirmationCode { get; set; }


        [CheckBox(Label = "Trust this device")]
        public bool TrustDevice { get; set; }


        public static string SkipCodeKey = "mfaskipcode";

        public static string GetSkipCode(UserData userData, ProjectSettings config) => $"{DateTime.UtcNow.AddDays(config.MfaSettings.TrustDeviceInDays.Value).ToIsoDateTime()}|{userData.UserId}".AesEncrypt(config.ProjectId.ToString(), userData.Email.ToLower());

        public static bool IsValidSkipCode(string code, UserData userData, ProjectSettings config)
        {
            try
            {
                if (config.MfaSettings.TrustDeviceInDays == null) return false;
                if (string.IsNullOrEmpty(code)) return false;
                var data = code.AesDecrypt(config.ProjectId.ToString(), userData.Email.ToLower());
                var parts = data.Split('|');
                if (DateTime.TryParse(parts[0], out DateTime validUntil) &&  int.TryParse(parts[1], out int userId))
                {
                    return userId == userData.UserId && validUntil > DateTime.UtcNow;
                }
            }
            catch
            {
            }
            return false;
        }
    }
}
