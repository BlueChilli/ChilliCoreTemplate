using System.ComponentModel.DataAnnotations;

namespace ChilliCoreTemplate.Data
{
    public interface IExternalId
    {
        string ExternalId { get; set; }

        int? ExternalIdHash { get; set; }

    }
}