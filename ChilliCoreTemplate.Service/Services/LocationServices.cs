using AutoMapper;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Cloud.Web.MVC;
using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using Microsoft.EntityFrameworkCore;
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Service.EmailAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace ChilliCoreTemplate.Service
{
    public partial class Services
    {
        private IQueryable<Location> Location_Authorised(bool mustBeActive = true)
        {
            var query = Context.Locations.AsQueryable();

            if (IsAdmin) return query;

            query = query.Where(x => x.CompanyId == CompanyId);

            if (IsCompanyAdmin) return query;

            return query.Where(x => x.IsActive && x.Users.Any(u => u.UserId == UserId));
        }

        private IQueryable<LocationUser> LocationUser_Authorised()
        {
            var query = Context.LocationUsers.AsQueryable();

            if (IsAdmin) return query;

            query = query.Where(x => x.Location.CompanyId == CompanyId && x.User.Status != UserStatus.Deleted);

            if (IsCompanyAdmin) return query;

            return query.Where(x => x.Location.Users.Any(u => u.UserId == UserId));
        }

        public ServiceResult<LocationViewModel> Location_Get(int id)
        {
            var farm = Location_Authorised(mustBeActive: false)
                .FirstOrDefault(x => x.Id == id);

            if (farm == null) return ServiceResult<LocationViewModel>.AsError($"Location not found for {id}");

            var model = GetSingle<LocationViewModel, Location>(farm);
            return ServiceResult<LocationViewModel>.AsSuccess(model);
        }

        public ServiceResult<LocationDetailModel> Location_GetDetail(int id)
        {
            var farm = Location_Authorised(mustBeActive: false)
                .Include(x => x.Company)
                .FirstOrDefault(x => x.Id == id);

            if (farm == null) return ServiceResult<LocationDetailModel>.AsError($"Location not found for {id}");

            var model = GetSingle<LocationDetailModel, Location>(farm);

            return ServiceResult<LocationDetailModel>.AsSuccess(model);
        }

        public ServiceResult<LocationEditModel> Location_Edit(LocationEditModel model)
        {
            var farm = Location_Authorised(mustBeActive: false)
                .FirstOrNew(l => l.Id == model.Id);

            if (farm.Id != model.Id)
            {
                return ServiceResult<LocationEditModel>.AsError("Location not found");
            }

            if (farm.Id == 0)
            {
                farm.CompanyId = CompanyId.Value;
                Context.Locations.Add(farm);
            }

            Mapper.Map(model, farm);

            Context.SaveChanges();

            return ServiceResult<LocationEditModel>.AsSuccess(Mapper.Map<LocationEditModel>(farm));
        }

        public ServiceResult<LocationEditModel> Location_GetForEdit(int? id)
        {
            var farm = Location_Authorised(mustBeActive: false)
                .FirstOrDefault(x => x.Id == id);

            if (farm == null && id.GetValueOrDefault(0) != 0) return ServiceResult<LocationEditModel>.AsError("Location not found");

            var model = Mapper.Map<LocationEditModel>(farm) ?? new LocationEditModel { };

            return ServiceResult<LocationEditModel>.AsSuccess(model);
        }

        public ServiceResult<LocationListModel> Location_List()
        {
            var model = new LocationListModel
            {
                Locations = Mapper.Map<List<LocationViewModel>>(Location_Authorised()
                    .AsNoTracking()
                    .ToList()
                ),
                StatusList = new KeyValuePair<int?, string>[] { new KeyValuePair<int?, string>(1, "Active"), new KeyValuePair<int?, string>(0, "Inactive") }.ToSelectList(v => v.Key, t => t.Value)
            };

            model.Locations = model.Locations
                .OrderBy(x => x.Name)
                .ToList();

            return ServiceResult<LocationListModel>.AsSuccess(model);
        }

        public ServiceResult<LocationViewModel> Location_Delete(int id)
        {
            var farm = Location_Authorised(mustBeActive: false)
                .FirstOrDefault(x => x.Id == id);

            //TODO revisted

            //if (Shipment_Active(true).Any(x => x.OriginId == id || x.DestinationId == id))
            //{
            //    return ServiceResult<LocationViewModel>.AsError("Location has active shipments");
            //}

            if (farm != null)
            {
                //if (farm.Departments.Count > 0)
                //{
                //    farm.Departments.Clear();
                //}

                //if (Context.Shipments.Any(x => x.OriginId == id || x.DestinationId == id) ||
                //    Context.Alerts.OfType<LocationAlert>().Any(x => x.LocationId == id))
                //{
                //    farm.IsActive = false;
                //}
                //else
                //{
                //    if (farm.Products.Count > 0)
                //    {
                //        farm.Products.Clear();
                //    }
                //    Context.Locations.Remove(farm);
                //}

                Context.Locations.Remove(farm);
                Context.SaveChanges();
                return ServiceResult<LocationViewModel>.AsSuccess(Mapper.Map<LocationViewModel>(farm));
            }

            return ServiceResult<LocationViewModel>.AsError("Location not found");
        }

        public ServiceResult<LocationDetailModel> Location_User_Add(int id, LocationUserInviteModel model)
        {
            var farm = Location_Authorised(mustBeActive: false)
                .FirstOrDefault(x => x.Id == id);
            Context.Entry(farm)
                .Collection(x => x.Users)
                .Query()
                .Where(u => u.User.Email == model.Email)
                .ToList();

            if (farm == null) return ServiceResult<LocationDetailModel>.AsError("Location not found");
            if (farm.Users.Any(x => x.User.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase))) return ServiceResult<LocationDetailModel>.AsError($"Location already has the user {model.Email}");

            var user = _accountService.GetAccountByEmail(model.Email);
            var userId = user?.Id;

            if (user == null)
            {   //Invite code will add user to farm
                var newUserRequest = _accountService.Invite(model, true);
                if (!newUserRequest.Success) return ServiceResult<LocationDetailModel>.CopyFrom(newUserRequest);
                userId = newUserRequest.Result.Id;
            }
            Context.LocationUsers.Add(new LocationUser { LocationId = farm.Id, UserId = userId.Value, CreatedOn = DateTime.UtcNow });
            Context.SaveChanges();

            return ServiceResult<LocationDetailModel>.AsSuccess(Location_GetDetail(id).Result);
        }

        public ServiceResult<List<LocationUserViewModel>> Location_User_List(int id, Role role)
        {
            var users = LocationUser_Authorised()
                .Where(x => x.LocationId == id && x.User.UserRoles.Any(r => (r.Role & role) > 0))
                .OrderBy(x => x.User.FullName)
                .Materialize<LocationUser, LocationUserViewModel>()
                .ToList();

            return ServiceResult<List<LocationUserViewModel>>.AsSuccess(users);
        }

        public PagedList<LocationUserViewModel> Location_User_List(IDataTablesRequest model, int id, Role role)
        {
            var query = LocationUser_Authorised()
                .Where(x => x.LocationId == id && x.User.UserRoles.Any(r => (r.Role & role) > 0));

            if (!String.IsNullOrEmpty(model.Search.Value))
            {
                query = query.Where(x => x.User.Email.StartsWith(model.Search.Value));
            }

            var isNew = DateTime.UtcNow.AddMinutes(-5);
            var queryOrdered = query.OrderBy(x => x.CreatedOn > isNew ? 0 : 1);

            foreach (var column in model.Columns.Where(c => c.IsSortable))
            {
                switch (column.Field)
                {
                    case "name":
                        if (column.IsSortable)
                        {
                            if (column.Sort.Direction == SortDirection.Ascending) queryOrdered = queryOrdered.ThenBy(x => x.User.FullName);
                            else queryOrdered = queryOrdered.ThenByDescending(x => x.User.FullName);
                        }
                        break;
                }
            }

            var users = queryOrdered
                .Materialize<LocationUser, LocationUserViewModel>()
                .ToPagedList(model.Start / model.Length + 1, model.Length);

            return users;
        }

        public int Location_User_Count(int id, Role role)
        {
            var query = LocationUser_Authorised()
                .Where(x => x.LocationId == id && x.User.UserRoles.Any(r => (r.Role & role) > 0));

            return query.Count();
        }

        public ServiceResult<LocationUserViewModel> Location_User_Get(int id, int userId)
        {
            var user = LocationUser_Authorised()
                .Where(x => x.LocationId == id && x.UserId == userId)
                .Materialize<LocationUser, LocationUserViewModel>()
                .FirstOrDefault();

            if (user == null) return ServiceResult<LocationUserViewModel>.AsError("User not found");

            return ServiceResult<LocationUserViewModel>.AsSuccess(user);
        }

        public ServiceResult Location_User_Delete(int id, int userId)
        {
            var user = LocationUser_Authorised()
                .FirstOrDefault(x => x.LocationId == id && x.UserId == userId);

            if (user == null) return ServiceResult.AsError("User not found");

            Context.LocationUsers.Remove(user);
            Context.SaveChanges();

            return ServiceResult.AsSuccess();
        }

        public LocationUserDetails Location_User_Details(string email, Role role)
        {
            var emailHash = CommonLibrary.CalculateHash(email);
            var user = _accountService.VisibleUsers()
                .Where(x => x.EmailHash == emailHash && x.Email == email && x.UserRoles.Any(r => r.CompanyId == CompanyId.Value && (r.Role & role) > 0))
                .Materialize<User, LocationUserDetails>()
                .FirstOrDefault();

            return user ?? new LocationUserDetails();
        }

    }
}
