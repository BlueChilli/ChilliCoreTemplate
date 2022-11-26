using ChilliSource.Cloud.Web.MVC;
using ChilliCoreTemplate.Models.Admin;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Http; using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ChilliCoreTemplate.Models.EmailAccount;
using Newtonsoft.Json;
using ChilliSource.Core.Extensions;

namespace ChilliCoreTemplate.Models
{
    //Marker interface
    public interface ICompanyViewModel
    {
    }

    public class CompanyViewModel: ICompanyViewModel
    {
        public int Id { get; set; }

        public string MasterCompany { get; set; }

        public string Name { get; set; }

        public Guid Guid { get; set; }

        public string Description { get; set; }

        public string StripeId { get; set; }

        public string Timezone { get; set; }

        public string LogoPath { get; set; }

        public string Website { get; set; }

        public string Email { get; set; }

        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        [DisplayName("Archived")]
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public bool HasAdmins { get; set; }

        public bool CanImpersonate() => HasAdmins && !IsDeleted;

        public static string CompanyAbbreviation(string name)
        {
            if (String.IsNullOrEmpty(name)) return name;
            var abbreviation = name.Any(x => x == ' ') ? name.ToUpper().Split(' ').Where(x => !String.IsNullOrEmpty(x)).Select(x => x[0]) : name.ToUpper().Take(2);
            return String.Join("", abbreviation);
        }
    }

    public class CompanyDetailViewModel : CompanyViewModel
    {
        public CompanyDetailViewModel()
        {
            Admin = new InviteEditModel();
        }

        public InviteEditModel Admin { get; set; }
    }

    public class CompanyListModel
    {

        [EmptyItem("Any status")]
        public bool? Status { get; set; } = true;
        public SelectList StatusList => new KeyValuePair<bool, string>[] { new KeyValuePair<bool, string>(true, "Active"), new KeyValuePair<bool, string>(false, "Inactive") }.ToSelectList(v => v.Key, t => t.Value, true);

        public string Search { get; set; }
    }

    public class CompanySummaryModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public DateTime CreatedAt { get; set; }

        public string Created => CreatedAt.ToTimezone().ToIsoDate();

        public bool IsDeleted { get; set; }

        public bool HasAdmins { get; set; }
    }

    public class CompanyUserViewModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsNew { get { return CreatedAt.AddMinutes(5) > DateTime.UtcNow; } }

    }

    public static class TimezoneHelper
    {
        public static IEnumerable<SelectListItem> GetSelectTimezones(string selected)
        {
            var utcTime = LocalDateTime.FromDateTime(DateTime.UtcNow).InZoneLeniently(DateTimeZoneProviders.Tzdb["UTC"]);

            var zones = new List<SelectListItem>(DateTimeZoneProviders.Tzdb.Ids.Count + 1);

            zones.Add(new SelectListItem() { Text = "", Value = "" });

            zones.AddRange(DateTimeZoneProviders.Tzdb.Ids
                .Where(id => id.IndexOf("GMT+", StringComparison.OrdinalIgnoreCase) < 0 && id.IndexOf("GMT-", StringComparison.OrdinalIgnoreCase) < 0)
                .Select(id => new
                {
                    Id = id,
                    Offset = new TimeSpan(utcTime.WithZone(DateTimeZoneProviders.Tzdb[id]).Offset.Ticks)
                })
                .OrderBy(z => z.Offset).Select(z => new SelectListItem()
                {
                    Text = $"(GMT {(z.Offset < TimeSpan.Zero ? "-" : "+")}{z.Offset.ToString("hh\\:mm")}) {z.Id}",
                    Value = z.Id,
                    Selected = z.Id == selected
                }));

            return zones;
        }
    }
}
