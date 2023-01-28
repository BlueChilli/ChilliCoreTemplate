
using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Admin;
using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliCoreTemplate.Service.EmailAccount;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.LinqMapper;
using ChilliSource.Core.Extensions;
using DataTables.AspNet.Core;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;

namespace ChilliCoreTemplate.Service.Admin
{
    public partial class AdminService : Service<DataContext>
    {
        IFileStorage _fileStorage;

        public AdminService(IPrincipal user, DataContext context, IFileStorage fileStorage)
            : base(user, context)
        {
            _fileStorage = fileStorage;
        }

        public void ChangeUserStatus(ChangeUserStatusModel model)
        {            
            var account = Context.Users.Where(a => a.Id == model.Id).First();
            if (account.Status != model.Status)
            {
                account.Status = model.Status;
                account.UpdatedDate = DateTime.UtcNow;

                Context.SaveChanges();
            }
        }      

        public List<AccountViewModel> GetUsers()
        {
            return GetList<AccountViewModel, User>(AccountService.VisibleUsers(this)
                                                    .Where(a => a.Status != UserStatus.Deleted)
                                                    .Include(a => a.UserRoles));
        }

        public PagedList<UserSummaryViewModel> Users_Query(IDataTablesRequest model)
        {
            Expression<Func<User, bool>> filter = (User x) => true;
            if (!String.IsNullOrEmpty(model.Search.Value))
            {
                filter = filter.And(x => x.FirstName.Contains(model.Search.Value) || x.LastName.Contains(model.Search.Value) || x.Email.Contains(model.Search.Value) || x.Phone.Contains(model.Search.Value));
            }

            foreach (var column in model.Columns.Where(c => c.IsSearchable && !String.IsNullOrEmpty(c.Search.Value)))
            {
                switch (column.Field)
                {
                    case "role":
                        var role = EnumHelper.Parse<Role>(column.Search.Value.ToLower());
                        filter = filter.And(x => x.UserRoles.Any(r => r.Role == role));
                        break;
                    case "status":
                        var status = EnumHelper.Parse<UserStatus>(column.Search.Value);
                        filter = filter.And(x => x.Status == status);
                        break;
                }
            }

            var query = AccountService.VisibleUsers(this)
                .Where(filter);

            IOrderedQueryable<User> queryOrdered = null;
            var sortColumn = model.Columns.FirstOrDefault(c => c.Sort != null);
            if (sortColumn == null || sortColumn.Field == "firstName")
            {
                queryOrdered = sortColumn.Sort?.Direction == SortDirection.Descending
                    ? query.OrderByDescending(x => x.FirstName)
                    : query.OrderBy(x => x.FirstName == null).ThenBy(x => x.FirstName);
            }
            else
            {
                if (sortColumn.Field == "lastName")
                {
                    queryOrdered = sortColumn.Sort.Direction == SortDirection.Ascending
                        ? query.OrderBy(x => x.LastName == null).ThenBy(x => x.LastName)
                        : query.OrderByDescending(x => x.LastName);
                }
                else if (sortColumn.Field == "email")
                {
                    queryOrdered = sortColumn.Sort.Direction == SortDirection.Ascending
                        ? query.OrderBy(x => x.Email == null).ThenBy(x => x.Email)
                        : query.OrderByDescending(x => x.Email);
                }
                else if (sortColumn.Field == "phone")
                {
                    queryOrdered = sortColumn.Sort.Direction == SortDirection.Ascending
                        ? query.OrderBy(x => x.Phone == null).ThenBy(x => x.Phone)
                        : query.OrderByDescending(x => x.Phone);
                }
                else if (sortColumn.Field == "company")
                {
                    queryOrdered = sortColumn.Sort.Direction == SortDirection.Ascending
                        ? query.OrderBy(x => x.UserRoles.FirstOrDefault().CompanyId == null).ThenBy(x => x.UserRoles.FirstOrDefault().Company.Name)
                        : query.OrderByDescending(x => x.UserRoles.FirstOrDefault().Company.Name);
                }
                else if (sortColumn.Field == "lastLoginOn")
                {
                    queryOrdered = sortColumn.Sort.Direction == SortDirection.Ascending
                        ? query.OrderBy(x => x.LastLoginDate)
                        : query.OrderByDescending(x => x.LastLoginDate);
                }
            }

            return queryOrdered
                .Materialize<User, UserSummaryViewModel>()
                .ToPagedList(model.Start / model.Length + 1, model.Length);
        }


        public int Users_Total()
        {
            return AccountService.VisibleUsers(this)
                .Where(a => a.Status != UserStatus.Deleted)
                .Count();
        }

        public ApiPagedList<DataLinkModel> User_List(string searchTerm, ApiPaging paging, int? id)
        {
            var query = AccountService.VisibleUsers(this);

            if (!String.IsNullOrEmpty(searchTerm))
            {
                if (int.TryParse(searchTerm, out var userId)) query = query.Where(x => x.Id == userId);
                else query = query.Where(x => x.FullName.Contains(searchTerm));
            }
            else if (id.HasValue) query = query.Where(x => x.Id == id.Value);

            return query.Materialize<User, DataLinkModel>()
                .Query(q => q.OrderBy(x => x.Name))
                .ToPagedList(paging);
        }

        public StatisticsModel GetUsersStatistics()
        {
            var model = new StatisticsModel { StatisticsRow1 = new List<StatisticModel>(), StatisticsRow2 = new List<StatisticModel>() };

            var now = DateTime.UtcNow; //this should be utc, but the rest of the account package code is in local time :(

            var startDate = now.Date.AddMonths(-6).StartOfMonth();
            var logins = Context.UserActivities
                .Where(a => a.ActivityOn > startDate && a.ActivityType == ActivityType.Create && a.EntityType == EntityType.Session)
                .Select(act => new { act.Id, act.UserId, act.ActivityOn }).ToList();

            var logins6Months = Enumerable.Range(-6, 6).Select(diff =>
            {
                var month = now.AddMonths(diff).Month;
                var count = logins.Where(r => r.ActivityOn.Month == month).Count();

                return new { Month = month, Count = count };
            }).ToList();

            model.StatisticsRow1.Add(new StatisticModel
            {
                Title = "Logins",
                PeriodLabel = "6 months",
                LabelType = "primary",
                Total = logins6Months.Sum(d => d.Count),
                PercentageChange = PercentageChanged(logins6Months[4].Count, logins6Months[5].Count),
                Data = logins6Months.Select(d => d.Count).ToList(),
                
                Labels = logins6Months.Select(d => $"'{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(d.Month)}'").ToList()
            });

            var logins6Weeks = Enumerable.Range(-6, 6).Select(diff =>
            {
                var week = DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(DateTime.UtcNow.AddDays(diff * 7), CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                var count = logins.Where(r => DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(r.ActivityOn, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) == week).Count();

                return new { Week = week, Count = count };
            }).ToList();

            model.StatisticsRow2.Add(new StatisticModel
            {
                Title = "Logins",
                PeriodLabel = "6 weeks",
                LabelType = "primary",
                Total = logins6Weeks.Sum(d => d.Count),
                PercentageChange = PercentageChanged(logins6Weeks[4].Count, logins6Weeks[5].Count),
                Data = logins6Weeks.Select(d => d.Count).ToList(),
                Labels = logins6Weeks.Select(d => $"'{d.Week.ToString()}'").ToList()
            });

            var uniqueLogins6Months = Enumerable.Range(-6, 6).Select(diff =>
            {
                var month = now.AddMonths(diff).Month;
                var count = logins.Where(r => r.ActivityOn.Month == month).DistinctBy(r => r.UserId).Count();

                return new { Month = month, Count = count };
            }).ToList();

            model.StatisticsRow1.Add(new StatisticModel
            {
                Title = "Unique Logins",
                PeriodLabel = "6 months",
                LabelType = "info",
                Total = logins.DistinctBy(l => l.UserId).Count(),
                PercentageChange = PercentageChanged(uniqueLogins6Months[4].Count, uniqueLogins6Months[5].Count),
                Data = uniqueLogins6Months.Select(d => d.Count).ToList(),
                Labels = uniqueLogins6Months.Select(d => $"'{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(d.Month)}'").ToList()
            });

            var uniqueLogins6Weeks = Enumerable.Range(-6, 6).Select(diff =>
            {
                var week = DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(DateTime.UtcNow.AddDays(diff * 7), CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                var count = logins.Where(r => DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(r.ActivityOn, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) == week)
                                .DistinctBy(r => r.UserId).Count();

                return new { Week = week, Count = count };
            }).ToList();

            model.StatisticsRow2.Add(new StatisticModel
            {
                Title = "Unique Logins",
                PeriodLabel = "6 weeks",
                LabelType = "info",
                Total = logins.Where(l => l.ActivityOn > now.Date.AddDays(-7 * 6)).DistinctBy(l => l.UserId).Count(),
                PercentageChange = PercentageChanged(uniqueLogins6Weeks[4].Count, uniqueLogins6Weeks[5].Count),
                Data = uniqueLogins6Weeks.Select(d => d.Count).ToList(),
                Labels = uniqueLogins6Weeks.Select(d => $"'{d.Week.ToString()}'").ToList()
            });

            var registrationsStats = this.GetRegistrationStatistics();

            model.StatisticsRow1.AddRange(registrationsStats.Where(e => e.PeriodLabel == "6 months"));
            model.StatisticsRow2.AddRange(registrationsStats.Where(e => e.PeriodLabel == "6 weeks"));

            return model;
        }

        public List<StatisticModel> GetRegistrationStatistics()
        {
            var result = new List<StatisticModel>();
            var now = DateTime.UtcNow; //this should be utc, but the rest of the account package code is in local time :(

            var startDate = now.Date.AddMonths(-6).StartOfMonth();
            var registrations = Context.Users.Where(a => a.CreatedDate > startDate).Select(a => new { a.Id, a.CreatedDate }).ToList();

            var registrations6Months = Enumerable.Range(-6, 6).Select(diff =>
            {
                var month = now.AddMonths(diff).Month;
                var count = registrations.Where(r => r.CreatedDate.Month == month).Count();

                return new { Month = month, Count = count };
            }).ToList();

            result.Add(new StatisticModel
            {
                Title = "Registrations",
                PeriodLabel = "6 months",
                LabelType = "success",
                Total = registrations6Months.Sum(d => d.Count),
                PercentageChange = PercentageChanged(registrations6Months[4].Count, registrations6Months[5].Count),
                Data = registrations6Months.Select(d => d.Count).ToList(),
                Labels = registrations6Months.Select(d => $"'{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(d.Month)}'").ToList()
            });

            var registrations6Weeks = Enumerable.Range(-6, 6).Select(diff =>
            {
                var week = DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(now.AddDays(diff * 7), CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                var count = registrations.Where(r => DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(r.CreatedDate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) == week).Count();

                return new { Week = week, Count = count };
            }).ToList();

            result.Add(new StatisticModel
            {
                Title = "Registrations",
                PeriodLabel = "6 weeks",
                LabelType = "success",
                Total = registrations6Weeks.Sum(d => d.Count),
                PercentageChange = PercentageChanged(registrations6Weeks[4].Count, registrations6Weeks[5].Count),
                Data = registrations6Weeks.Select(d => d.Count).ToList(),
                Labels = registrations6Weeks.Select(d => $"'{d.Week.ToString()}'").ToList()
            });

            var registrations6Days = Enumerable.Range(-6, 6).Select(diff =>
            {
                var date = now.AddDays(diff).Date;
                var count = registrations.Where(r => r.CreatedDate.Date == date).Count();

                return new { Date = date, Count = count };
            }).ToList();

            result.Add(new StatisticModel
            {
                Title = "Registrations",
                PeriodLabel = "6 Days",
                LabelType = "success",
                Total = registrations6Days.Sum(d => d.Count),
                PercentageChange = PercentageChanged(registrations6Days[4].Count, registrations6Days[5].Count),
                Data = registrations6Days.Select(d => d.Count).ToList(),
                Labels = registrations6Days.Select(d => $"'{d.Date.ToString("dd/MM")}'").ToList()
            });

            return result;
        }

        private int PercentageChanged(int from, int to)
        {
            if (from == 0 && to == 0) return 0;
            if (to == 0) return -100;
            if (from == 0) return to * 100;

            return (100 * (to - from) / Math.Abs(from));
        }

        public PagedList<UserActivityViewModel> GetActivities(IDataTablesRequest model, DateTime dateFrom, DateTime dateTo, EntityType? entityType, ActivityType? activityType)
        {
            dateFrom = dateFrom.FromUserTimezone();
            dateTo = dateTo.FromUserTimezone().Add(new TimeSpan(23, 59, 59));

            Expression<Func<UserActivity, bool>> filter = (UserActivity aa) => (aa.ActivityOn > dateFrom && aa.ActivityOn < dateTo);
            if (!String.IsNullOrEmpty(model.Search.Value))
            {
                filter = filter.And(aa => (aa.User.FirstName.Contains(model.Search.Value) || aa.User.LastName.Contains(model.Search.Value) || aa.User.Email.Contains(model.Search.Value)));
            }
            
            if (activityType != null)
            {
                filter = filter.And(aa => aa.ActivityType == activityType.Value);
            }

            if (entityType != null)
            {
                filter = filter.And(aa => aa.EntityType == entityType.Value);
            }

            var query = Context.UserActivities.Where(filter)
                            .OrderByDescending(aa => aa.ActivityOn)
                            .Include(aa => aa.User);

            return GetPagedList<UserActivityViewModel, UserActivity>(query, model.Start / model.Length + 1, model.Length);
        }

        public int GetActivityTotal()
        {
            return Context.UserActivities.Count();
        }

        public UserActivityViewModel GetActivity(int id)
        {
            var activity = Context.UserActivities.First(a => a.Id == id);

            var model = GetSingle<UserActivityViewModel, UserActivity>(activity);

            switch (model.EntityType)
            {
                case EntityType.User:
                case EntityType.Session:
                case EntityType.Password:
                    model.EntityDescription = Context.Users.First(x => x.Id == model.EntityId).FullName;
                    break;
                case EntityType.Company:
                    model.EntityDescription = Context.Companies.First(x => x.Id == model.EntityId).Name;
                    break;
            }

            if (model.TargetId.HasValue)
            {
                //if (model.ActivityType == ActivityType.Repost)
                //{
                //    model.TargetDescription = Context.Discoveries.First(x => x.Id == model.TargetId.Value).Name;
                //}
                //else if(model.ActivityType == ActivityType.Rating)
                //{
                //    model.TargetDescription = model.Entity == EntityType.Discovery
                //        ? Context.DiscoveryRatings.First(x => x.Id == model.TargetId.Value).Rating.ToString()
                //        : Context.DiscoveryCommentRatings.First(x => x.Id == model.TargetId.Value).Rating.ToString();
                //}
                //else
                {
                    model.TargetDescription = Context.Users.First(x => x.Id == model.TargetId.Value).FullName;
                }
            }

            return model;
        }

    }
}
