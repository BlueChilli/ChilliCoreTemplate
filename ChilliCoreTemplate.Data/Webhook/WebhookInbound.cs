
using ChilliCoreTemplate.Models.Api;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChilliCoreTemplate.Data;

[Index(nameof(WebhookIdHash))]
[Index(nameof(Processed))]
public class WebhookInbound
{
    public int Id { get; set; }

    [StringLength(100)]
    public string WebhookId { get; set; }

    public int WebhookIdHash { get; set; }

    public WebhookType Type { get; set; }

    [StringLength(100)]
    public string Subtype { get; set; }

    [StringLength(100)]
    public string SubtypeId { get; set; }

    public string Raw { get; set; }

    public string Error { get; set; }

    public bool Success { get; set; }

    public bool Processed { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ProcessedOn { get; set; }
}
