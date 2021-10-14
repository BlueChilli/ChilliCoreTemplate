using ChilliSource.Cloud.Core; using ChilliSource.Cloud.Web.MVC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Http; using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using ChilliCoreTemplate.Models.EmailAccount;
using System.Net;

namespace ChilliCoreTemplate.Models
{
    public class LocationEditModel
    {
        public LocationEditModel()
        {
        }

        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(1000), DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required, EmptyItem, MaxLength(50)]
        public string Timezone { get; set; }

        [CheckBox]
        public bool IsActive { get; set; }

    }

    public class LocationViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string IpAddress { get; set; }

        public bool IsActive { get; set; }
    }

    public class LocationDetailModel : LocationViewModel
    {
        public LocationDetailModel()
        {
            User = new LocationUserInviteModel();
        }

        public string Description { get; set; }

        public LocationUserInviteModel User { get; set; }

    }

    public class LocationListModel
    {

        #region Filter
        [EmptyItem("Any status")]
        public bool? Status { get; set; }
        public SelectList StatusList { get; set; }

        [Placeholder("Search"), MaxLength(100)]
        public string Search { get; set; }

        #endregion

        public List<LocationViewModel> Locations { get; set; }

    }

    public class LocationUserViewModel
    {

        public int Id { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Status { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsNew { get { return CreatedOn.AddMinutes(5) > DateTime.UtcNow; } }

    }

    public class LocationUserInviteModel : InviteEditModel, IValidatableObject
    {
        public LocationUserInviteModel()
        {
            LocationIds = new List<int>();
        }

        [Required, DisplayName("Locations")]
        public List<int> LocationIds { get; set; }
        public SelectList LocationList { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (InviteRole != null && (InviteRole.Role & (Role.CompanyUser)) > 0)
            {
                if (LocationIds.Count == 0)
                    yield return new ValidationResult("A location must be chosen.", new string[] { "LocationIds" });
            }
        }
    }

    public class LocationUserDetails
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

    }
}
