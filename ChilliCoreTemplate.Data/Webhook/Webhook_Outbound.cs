using ChilliCoreTemplate.Models.Api;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliCoreTemplate.Data
{
    //[Table("Webhooks_Outbound")]
    //public class Webhook_Outbound
    //{
    //    public int Id { get; set; }

    //    public int OrganisationId { get; set; } //TODO replace with (for example accountid) whatever you are storing the private apikey against
    //    //public Organisation Organisation { get; set; }

    //    [Index]
    //    public WebhookEvent Event { get; set; }

    //    [StringLength(100)]
    //    public string Target_Url { get; set; }

    //    public string ResourceUrl(int id, object parameters = null)
    //    {
    //        var url = ProjectConfigurationSection.GetConfig().BaseUrl;

    //        switch (Event)
    //        {
    //            case WebhookEvent.Dog_Created:
    //                url += $"/api/webhooks/dogs/{id}";
    //                break;
    //            case WebhookEvent.Cat_Created:
    //            case WebhookEvent.Cat_FirstBirthday:
    //                url += $"/api/webhooks/cats/{id}";
    //                //if (parameters != null) url = new Uri(url).AddQuery(parameters).ToString();
    //                break;
    //            case WebhookEvent.Cat_Cleaned:
    //                url += $"/api/webhooks/cats/{id}";
    //                //if (parameters != null) url = new Uri(url).AddQuery(parameters).ToString();
    //                break;
    //        }

    //        return url;
    //    }
    //}
}
