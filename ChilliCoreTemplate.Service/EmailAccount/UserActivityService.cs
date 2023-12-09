using ChilliCoreTemplate.Data;
using ChilliCoreTemplate.Data.EmailAccount;
using ChilliCoreTemplate.Models.EmailAccount;
using ChilliSource.Cloud.Core.LinqMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Service.EmailAccount
{
    public partial class AccountService
    {
        public List<UserActivityViewModel> Activity_Last(int accountId, int count)
        {
            return Context.UserActivities
                .Where(ua => ua.UserId == accountId)
                .OrderByDescending(ua => ua.ActivityOn)
                .Take(count)
                .Materialize<UserActivity, UserActivityViewModel>()
                .ToList();
        }

        internal void Activity_Add(UserActivity activity)
        {
            if (activity.UserId == 0) return;
            activity.ActivityOn = DateTime.UtcNow;
            Context.UserActivities.Add(activity);
            Context.SaveChanges();
        }

        internal static void Activity_Add(DataContext context, UserActivity activity)
        {
            if (activity.UserId == 0) return;
            activity.ActivityOn = DateTime.UtcNow;
            context.UserActivities.Add(activity);
            context.SaveChanges();
        }

        internal static async Task Activity_AddAsync(DataContext context, UserActivity activity)
        {
            if (activity.UserId == 0) return;
            activity.ActivityOn = DateTime.UtcNow;
            context.UserActivities.Add(activity);
            await context.SaveChangesAsync();
        }
    }
}
