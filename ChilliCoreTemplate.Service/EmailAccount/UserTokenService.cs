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
            return Token_Add(user, type, expiry, out _);
        }

        internal Guid Token_Add(User user, UserTokenType type, TimeSpan? expiry, out bool wasExpired)
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

            wasExpired = false;
            if (token.Expiry == null || token.Expiry < DateTime.UtcNow.AddMinutes(2))
            {
                token.Token = Guid.NewGuid();
                wasExpired = true;
            }
            token.Expiry = DateTime.UtcNow.AddTicks(expiry.GetValueOrDefault(new TimeSpan()).Ticks == 0 ? new TimeSpan(1, 0, 0).Ticks : expiry.Value.Ticks);

            if (token.Id == 0) user.Tokens.Add(token);

            return token.Token;
        }

        internal ServiceResult<User> Token_Check(User user, string tokenString)
        {
            var tokenKey = ShortGuid.Decode(tokenString);

            if (user != null)
            {
                var token = user.Tokens.FirstOrDefault(t => t.Token == tokenKey && t.Expiry > DateTime.UtcNow);
                if (token != null)
                {
                    token.Expiry = DateTime.UtcNow.AddMinutes(2);   //Expire the token (if saved) in a few minutes to not cause errors when users double click activation links
                    Context.SaveChanges();
                    return ServiceResult<User>.AsSuccess(user);
                }
            }

            return ServiceResult<User>.AsError(user == null ? "Account not found or access denied" : "Token is invalid or has expired");
        }

        public ServiceResult<AccountViewModel> User_GetByEmailToken(UserTokenModel model, bool includeDeleted = false)
        {
            var accountResult = User_GetAccountByEmailToken(model);

            if (!accountResult.Success) return ServiceResult<AccountViewModel>.AsError(accountResult.Error);

            return ServiceResult<AccountViewModel>.AsSuccess(_mapper.Map<AccountViewModel>(accountResult.Result));
        }

        internal ServiceResult<User> User_GetAccountByEmailToken(UserTokenModel model, bool includeDeleted = false)
        {
            var account = GetAccountByEmail(model.Email, includeDeleted);
            return Token_Check(account, model.Token);
        }

        internal ServiceResult<User> User_GetAccountByOneTimePassword(UserTokenModel model)
        {
            var data = User_GetToken(model);
            if (data != null && data.Item2 != null)
            {
                data.Item2.Expiry = DateTime.UtcNow;
                Context.SaveChanges();
                return ServiceResult<User>.AsSuccess(data.Item1);
            }

            return ServiceResult<User>.AsError(data == null ? "Account not found or access denied" : "Code is invalid or has expired");
        }

        internal Tuple<User, UserToken> User_GetToken(UserTokenModel model)
        {
            var account = GetAccountByEmail(model.Email);

            if (account != null)
            {
                var token = account.Tokens.FirstOrDefault(t => new OneTimePasswordModel(t.Token).Code == model.Token && t.Expiry > DateTime.UtcNow);
                return Tuple.Create(account, token);
            }

            return null;
        }

    }

}
