using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.EmailAccount
{
    public class UserActivityViewModel
    {
        public int Id { get; set; }

        public AccountViewModel Account { get; set; }

        public string User { get { return Account.FullName + "<br/>" + Account.Email; } }

        public DateTime ActivityOn { get; set; }

        public string ActivityOnDisplay { get { return ActivityOn.ToTimezone().ToIsoDateTime(); } }

        public ActivityType ActivityType { get; set; }

        public string ActivityTypeDescription { get { return ActivityType.GetDescription(); } }

        public EntityType EntityType { get; set; }

        public string EntityTypeDescription { get { return EntityType.GetDescription(); } }

        public int EntityId { get; set; }

        public string EntityDescription { get; set; }

        public string TargetDescription { get; set; }

        public int? TargetId { get; set; }

        public string JsonData { get; set; }
    }

    public class UserActivityModel
    {
        public EntityType? Entity { get; set; }

        public ActivityType? Activity { get; set; }

    }

    public class UserDetailsModel
    {
        public AccountViewModel Account { get; set; }

        public List<UserActivityViewModel> LastActivities { get; set; }
    }

    public enum ActivityType
    {
        Create = 1,
        Delete,
        Update,
        View,
        Activate
    }

    public enum EntityType
    {
        User = 1,
        Session,
        Password,
        Email,
        Sms,
        Company
    }


}
