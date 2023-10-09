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

        public bool CanDelete { get; set; }

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

        public List<CompanyViewModel> SubCompanies { get; set; }
    }

    public class CompanyListModel
    {
        [EmptyItem("Any status")]
        public bool? Status { get; set; }
        public SelectList StatusList => new KeyValuePair<bool, string>[] { new KeyValuePair<bool, string>(true, "Active"), new KeyValuePair<bool, string>(false, "Inactive") }.ToSelectList(v => v.Key, t => t.Value, true);

        [Placeholder]
        public string Search { get; set; }
    }

    public class CompanyEditModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [DisplayName("Master company"), EmptyItem]
        public int? MasterCompanyId { get; set; }
        public SelectList CompanyList { get; set; }

        [Required]
        public Guid ApiKey { get; set; }

        [MaxLength(50), DisplayName("Stripe Id")]
        public string StripeId { get; set; }

        [CheckBox(Label = "Create Stripe account")]
        public bool CreateStripeAccount { get; set; }

        [MaxLength(1000), DataType(DataType.MultilineText), CharactersLeft, HelpText("Notes are only visible to superadmins")]
        public string Notes { get; set; }

        [MaxLength(100), Url(ErrorMessage = Constants.UrlErrorMessage)]
        public string Website { get; set; }

        #region Address
        public bool IsManualAddress { get; set; }

        [MaxLength(100), Placeholder("Company primary address")]
        public string Address { get; set; }

        [StringLength(100)]
        public string StreetAddress { get; set; }

        [StringLength(50)]
        public string Suburb { get; set; }

        [StringLength(50)]
        public string State { get; set; }

        [StringLength(10)]
        public string Postcode { get; set; }

        public Country? Country { get; set; }

        [Required, EmptyItem]
        public string Timezone { get; set; }
        public SelectList TimezoneList { get; set; }

        #endregion

        [DisplayName("Logo")]
        [FileMaxSize(8 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: Constants.AllowedGraphicExtensions)]
        public IFormFile LogoFile { get; set; }
        public string LogoPath { get; set; }

        [CheckBox, Label("Archived")]
        public bool IsDeleted { get; set; }
    }

    public class CompanySettingsModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100), Url(ErrorMessage = Constants.UrlErrorMessage)]
        public string Website { get; set; }

        [DisplayName("Logo")]
        [FileMaxSize(8 * 1024 * 1024), FileMinSize]
        [HttpPostedFileExtensions(allowedExtensions: Constants.AllowedGraphicExtensions)]
        public IFormFile LogoFile { get; set; }
        public string LogoPath { get; set; }

        [Required, EmptyItem]
        public string Timezone { get; set; }
        public SelectList TimezoneList { get; set; }
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

    public class CompanyExportModel
    {
        public string Name { get; set; }

        public string LogoUrl { get; set; }

        public string Address { get; set; }

        public string HasCommission { get; set; }

        public string UserFullName { get; set; }

        public string UserEmail { get; set; }
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
