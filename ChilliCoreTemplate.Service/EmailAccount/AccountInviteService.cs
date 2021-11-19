using AutoMapper;
using ChilliSource.Cloud.Core;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Core.Extensions;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Text;
using ChilliSource.Cloud.Web;
using System.Globalization;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public partial class AccountService
    {
        public List<AccountViewModel> GetPendingInvites()
        {
            var users = VisibleUsers().Where(a => a.Status == UserStatus.Invited)
                            .OrderBy(a => a.InvitedDate)
                            .Include(a => a.UserRoles);

            return GetList<AccountViewModel, User>(users);
        }

        public ServiceResult<AccountViewModel> Invite(InviteEditModel model, bool sendEmail)
        {
            var createModel = Mapper.Map<UserCreateModel>(model);

            var createAccountRequest = Create(createModel);
            if (createAccountRequest.Success)
            {
                var account = createAccountRequest.Result;
                model.Token = account.GetToken(UserTokenType.Invite);
                model.Inviter = User.Identity.Name;

                if (sendEmail) QueueMail(RazorTemplates.InviteUser, model.Email, new RazorTemplateDataModel<InviteEditModel> { Data = model });

                var company = Context.Companies.FirstOrDefault(c => c.Id == model.InviteRole.CompanyId);
                Mixpanel.SendAccountToMixpanel(account, "Invite", data: new Dictionary<string, object> { { "Company", company?.Name } });

                return ServiceResult<AccountViewModel>.AsSuccess(GetSingle<AccountViewModel, User>(account));
            }
            else
            {
                return ServiceResult<AccountViewModel>.AsError("Account already exists");
            }
        }

        public ServiceResult<AccountViewModel> Reinvite(int id)
        {
            var account = GetAccount(id);
            var model = Mapper.Map<InviteEditModel>(account);
            return Invite(model, sendEmail: true);
        }

        public ServiceResult<UserData> ConfirmInvite(ResetPasswordViewModel model)
        {
            var result = this.Password_Reset(model); //Reset of password will confirm the account
            if (!result.Success) return ServiceResult<UserData>.AsError("Your invitation has expired. Please request another invitation.");

            var account = Context.Users.First(a => a.Email == model.Email);
            return ServiceResult<UserData>.AsSuccess(MapUserData(account, null));
        }

        private void Invite_Confirm(User user)
        {
            user.Status = UserStatus.Activated;
            user.ActivatedDate = DateTime.UtcNow;

            Mixpanel.SendAccountToMixpanel(user, "Invite confirmed");
            Activity_Add(new UserActivity { UserId = user.Id, ActivityType = ActivityType.Activate, EntityId = user.Id, EntityType = EntityType.User });
        }

        public ServiceResult<int> Invite_Upload(InviteUploadModel model)
        {
            var result = 0;
            try
            {
                using (var inputStream = model.InviteFile.OpenReadStream())
                using (var reader = new CsvReader(new StreamReader(inputStream), CultureInfo.InvariantCulture))
                {
                    var users = reader.GetRecords<InviteUploadItemModel>().ToList();

                    foreach (var user in users)
                    {
                        if (user.Role == Role.Administrator) continue;

                        var inviteModel = Mapper.Map<InviteEditModel>(user);
                        //inviteModel.CompanyId = Context.Companies.FirstOrDefault(c => c.Name == user.CompanyName)?.Id;
                        //if (inviteModel.CompanyId == null) continue;

                        inviteModel.InviteRole = new InviteRoleViewModel()
                        {
                            Role = user.Role
                        };

                        var invite = Invite(inviteModel, sendEmail: false);
                        if (invite.Success) result++;
                    }

                    return ServiceResult<int>.AsSuccess(result);
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.AsError(ex.Message);
            }
        }

        public ServiceResult<UserImportResultModel> ImportUsers(UserImportModel model)
        {
            var errors = new List<string>();
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, TrimOptions = TrimOptions.Trim };
            var users = new List<UserCreateModel>();
            int rowCount = 0;
            var imported = new List<UserImportedModel>();

            try
            {
                using (var inputStream = model.CsvFile.OpenReadStream())
                using (var reader = new CsvReader(new StreamReader(inputStream), csvConfig))
                {
                    reader.Read();
                    reader.ReadHeader();
                    var headerRow = reader.Context.Reader.HeaderRecord;
                    while (reader.Read())
                    {
                        var user = new UserCreateModel();
                        rowCount++;
                        var columns = reader.Context.Reader.ColumnCount;
                        if (columns != headerRow.Count())
                        {
                            return ServiceResult<UserImportResultModel>.AsError(error: $"Error in row {rowCount}. Each row must contain the same number of columns as the header row. Found {columns}, expected {headerRow.Count()}.");
                        }

                        for (int i = 0; i < columns; i++)
                        {
                            var data = reader.GetField(i);
                            var header = (ColumnNameEnum)Enum.Parse(typeof(ColumnNameEnum), headerRow[i].RemoveSpaces(), ignoreCase: true);
                            switch (header)
                            {
                                case ColumnNameEnum.LastName:
                                    user.LastName = ValidateField(errors, rowCount, data, header, 25);
                                    break;
                                case ColumnNameEnum.FirstName:
                                    user.FirstName = ValidateField(errors, rowCount, data, header, 25);
                                    break;
                                case ColumnNameEnum.Email:
                                    user.Email = ValidateEmail(errors, rowCount, data, header, 100);
                                    break;
                            }
                        }
                        if (errors.Count > 0) return ServiceResult<UserImportResultModel>.AsError(error: String.Join("<br>", errors));
                        users.Add(user);
                    }
                }
                foreach (var user in users)
                {
                    user.Status = model.Status.Value;
                    if (model.Roles != null)
                    {
                        user.UserRoles.Add(new RoleSelectionViewModel()
                        {
                            Role = model.Roles.Value,
                            CompanyId = model.CompanyId
                        });
                    }

                    var createAccountRequest = Create(user);
                    if (createAccountRequest.Success)
                    {
                        var account = createAccountRequest.Result;
                        imported.Add(new UserImportedModel
                        {
                            Email = account.Email,
                            FirstName = account.FirstName,
                            LastName = account.LastName,
                            InviteUrl = model.Status.Value == UserStatus.Invited ? _config.ResolveUrl("~/EmailAccount/ConfirmInvite", new UserTokenModel
                            {
                                Token = account.GetToken(UserTokenType.Invite),
                                Email = account.Email
                            }) : ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogException();
                return ServiceResult<UserImportResultModel>.AsError(error: ex.Message);
            }

            var path = _fileStorage.Save(new StorageCommand { Extension = ".csv", Folder = "Temp" }.SetByteArraySource(Encoding.UTF8.GetBytes(imported.ToCsvFile())));
            var result = new UserImportResultModel
            {
                Processed = rowCount,
                Invited = imported.Count(),
                Path = path
            };
            return ServiceResult<UserImportResultModel>.AsSuccess(result);
        }

        public byte[] ImportUsersResult(string path)
        {
            if (_fileStorage.Exists(path))
            {
                var data = _fileStorage.GetContent(path);
                _fileStorage.Delete(path);
                return data.ReadToByteArray();
            }
            return null;
        }

        private string ValidateField(List<string> errors, int rowCount, string data, ColumnNameEnum header, int maxLength, bool isRequired = true)
        {
            if (isRequired && String.IsNullOrWhiteSpace(data)) errors.Add($"{header.GetDescription()} in line {rowCount} is empty.");
            if (data.Length > maxLength) errors.Add($"{header.GetDescription()} in line {rowCount} is too long.");
            return data;
        }

        private string ValidateEmail(List<string> errors, int rowCount, string data, ColumnNameEnum header, int maxLength)
        {
            ValidateField(errors, rowCount, data, header, maxLength);
            if (!new EmailAddressWebAttribute().IsValid(data))
            {
                errors.Add($"{header.GetDescription()} in line {rowCount} is not valid.");
                return null;
            }
            return data;
        }

        private enum ColumnNameEnum
        {
            LastName,
            FirstName,
            Email,
        }

        private class UserImportedModel
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string Email { get; set; }

            public string InviteUrl { get; set; }
        }

    }
}
