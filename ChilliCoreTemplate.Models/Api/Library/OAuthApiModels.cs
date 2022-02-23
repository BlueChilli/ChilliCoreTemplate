using ChilliSource.Cloud.Core;
using FoolProof.Core;
using HybridModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ChilliCoreTemplate.Models.Api.OAuth
{

    public enum OAuthProvider
    {
        Unknown,
        Google,
        Facebook,
        //Apple Not completed
    }

    public class OAuthRegisterApiModel
    {
        [Required]
        public OAuthProvider? Provider { get; set; }

        [RequiredIfEmpty("Code")]
        public string Token { get; set; }

        [RequiredIfEmpty("Token")]
        public string Code { get; set; }

    }

    public class OAuthLoginApiModel : OAuthRegisterApiModel
    {
        public bool Cookieless { get; set; }

        public string DeviceId { get; set; }

    }

    public class OAuthUrlApiModel
    {
        [Required]
        public OAuthProvider? Provider { get; set; }

        [Required]
        public string RedirectUrl { get; set; }

        [JsonIgnore]
        public string Email { get; set; }

        [JsonIgnore]
        [HybridBindProperty(Source.Header, "UserKey")]
        public string UserKey { get; set; }

    }

    public class OAuthCodeApiModel
    {
        public string Code { get; set; }

        public string Scope { get; set; }

        public string AuthUser { get; set; }

        public string Error { get; set; }
        public string Error_Code { get; set; }
        public string GetError => Error ?? Error_Code;

        public string Error_Description { get; set; }
        public string Error_Message { get; set; }
        public string GetErrorMessage => Error_Description ?? Error_Message;

        public string Prompt { get; set; }

        [Required]
        public string State { get; set; }

        public string Id_Token { get; set; }    //Apple

    }

    public class OAuthCodeResultApiModel
    {
        public string Email { get; set; }

        public Guid Token { get; set; }
    }

    public class OAuthDeauthApiModel
    {
        public OAuthProvider Provider { get; set; }

        [HybridBindProperty(Source.Form, "signed_request")]
        public string SignedRequest { get; set; }

    }

    public abstract class OAuthTokenModel
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

    }

    public class OAuthTokenFacebookModel : OAuthTokenModel
    {
        public OAuthErrorModel Error { get; set; }

        public bool HasError => Error != null;
        public ServiceResult GetError => new ServiceResult { Success = false, Key = Error.Type, Error = Error.Message };
    }

    public class OAuthTokenGoogleModel : OAuthTokenModel
    {
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

        public bool HasError => Error != null;
        public ServiceResult GetError => new ServiceResult { Success = false, Key = Error, Error = ErrorDescription };
    }

    public class OAuthErrorModel
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }

    public class OAuthJWT
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string KeyId { get; set; }

        public string Key { get; set; }

    }

    public class OAuthUserModel
    {
        public string Id { get; set; }
        public int IdHash => CommonLibrary.CalculateHash(Id).Value;

        public string Email { get; set; }
        public bool EmailIsVerified { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProfilePhotoUrl { get; set; }

        public string Token { get; set; }
    }

    public class OAuthGoogleUserModel
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("verified_email")]
        public bool EmailIsVerified { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("given_name")]
        public string FirstName { get; set; }

        [JsonProperty("family_name")]
        public string LastName { get; set; }

        [JsonProperty("picture")]
        public string ProfilePhotoUrl { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

    }

    public class OAuthFacebookUserModel
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("picture")]
        public OAuthFacebookDataModel<OAuthFacebookPictureModel> Picture {get; set; }

    }

    public class OAuthFacebookDataModel<T>
    {
        public T Data { get; set; }
    }

    public class OAuthFacebookPictureModel
    {
        public string Url { get; set; }
    }

    public class OAuthFacebookDeauthModel
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

    }

    public enum OAuthMode
    {
        Any,
        Login,
        Register
    }
}
