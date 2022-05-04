using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Models
{

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


}
