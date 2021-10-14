using ChilliSource.Cloud.Core;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public partial class AccountService : Service<DataContext>
    {

        /// <summary>
        /// Add token to account collection. Token is NOT saved. Calling code will need to call Context.SaveChanges()
        /// </summary>
        internal Guid Token_Add(User user, UserTokenType type, TimeSpan? expiry = null)
        {
            if (user.Tokens == null) user.Tokens = new List<UserToken>();

            var token = user.Tokens.FirstOrDefault(t => t.Type == type);
            if (token == null)
            {
                token = new UserToken
                {
                    Type = type
                };
            }

            if (token.Expiry == null || token.Expiry < DateTime.Now) token.Token = Guid.NewGuid();
            token.Expiry = DateTime.UtcNow.AddTicks(expiry.GetValueOrDefault(new TimeSpan()).Ticks == 0 ? new TimeSpan(1, 0, 0).Ticks : expiry.Value.Ticks);

            if (token.Id == 0) user.Tokens.Add(token);

            return token.Token;
        }


        public ServiceResult<AccountViewModel> User_GetByEmailToken(EmailTokenModel model)
        {
            var accountResult = User_GetAccountByEmailToken(model);

            if (!accountResult.Success) return ServiceResult<AccountViewModel>.AsError(accountResult.Error);

            return ServiceResult<AccountViewModel>.AsSuccess(GetSingle<AccountViewModel, User>(accountResult.Result));
        }

        internal ServiceResult<User> User_GetAccountByEmailToken(EmailTokenModel model)
        {
            var account = GetAccountByEmail(model.Email);
            var tokenKey = ShortGuid.Decode(model.Token);

            if (account != null)
            {
                var token = account.Tokens.FirstOrDefault(t => t.Token == tokenKey && t.Expiry > DateTime.UtcNow);
                if (token != null)
                {
                    token.Expiry = DateTime.UtcNow.AddMinutes(5);   //Expire the token (if saved) in a few minutes to not cause errors when users double click activation links
                    Context.SaveChanges();
                    return ServiceResult<User>.AsSuccess(account);
                }
            }

            return ServiceResult<User>.AsError(account == null ? "Account not found or access denied" : "Token is invalid or has expired");
        }

        internal ServiceResult<User> User_GetAccountByOneTimePassword(EmailTokenModel model)
        {
            var account = GetAccountByEmail(model.Email);

            if (account != null)
            {
                var token = account.Tokens.FirstOrDefault(t => new OneTimePasswordModel(t.Token).Code == model.Token && t.Expiry > DateTime.UtcNow);
                if (token != null)
                {
                    token.Expiry = DateTime.UtcNow;
                    Context.SaveChanges();
                    return ServiceResult<User>.AsSuccess(account);
                }
            }

            return ServiceResult<User>.AsError(account == null ? "Account not found or access denied" : "Code is invalid or has expired");
        }

    }

}
