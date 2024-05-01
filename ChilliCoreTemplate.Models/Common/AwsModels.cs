using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models.Aws
{

    public class SnsMessage
    {
        public string Type { get; set; }
        public string MessageId { get; set; }
        public string Token { get; set; }
        public string TopicArn { get; set; }
        public string Message { get; set; }
        public string SubscribeURL { get; set; }
        public DateTime Timestamp { get; set; }
        public string SignatureVersion { get; set; }
        public string Signature { get; set; }
        public string SigningCertURL { get; set; }
    }


    /// <summary>
    /// http://docs.aws.amazon.com/ses/latest/DeveloperGuide/notification-contents.html#top-level-json-object
    /// </summary>
    public class SnsNotification
    {
        public string NotificationType { get; set; }

        public MailObject Mail { get; set; }

        public BounceObject Bounce { get; set; }

        public ComplaintObject Complaint { get; set; }
        //public object Delivery { get; set; }

    }

    public class MailObject
    {
        public string Timestamp { get; set; }
        public string MessageId { get; set; }
        public string Source { get; set; }
        public string SourceArn { get; set; }
        public string SourceIp { get; set; }
        public string SendingAccountId { get; set; }
        public IList<string> Destination { get; set; }
        public bool HeadersTruncated { get; set; }
        public IList<Header> Headers { get; set; }
        public CommonHeaders CommonHeaders { get; set; }
    }

    public class Header
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class CommonHeaders
    {
        public IList<string> From { get; set; }
        public string Date { get; set; }
        public IList<string> To { get; set; }
        public string MessageId { get; set; }
        public string Subject { get; set; }
    }

    public class BouncedRecipient
    {
        public string Status { get; set; }
        public string Action { get; set; }
        public string DiagnosticCode { get; set; }
        public string EmailAddress { get; set; }
    }

    public class BounceObject
    {
        public BounceType BounceType { get; set; }
        public BounceSubType BounceSubType { get; set; }
        public IList<BouncedRecipient> BouncedRecipients { get; set; }
        public string ReportingMTA { get; set; }
        public DateTime Timestamp { get; set; }
        public string FeedbackId { get; set; }
        public string RemoteMtaIp { get; set; }
    }

    public enum BounceType
    {
        Undetermined,
        Permanent,
        Transient
    }

    public enum BounceSubType
    {
        Undetermined,
        General,
        NoEmail,
        Suppressed,
        OnAccountSuppressionList,
        MailboxFull,
        MessageTooLarge,
        ContentRejected,
        AttachmentRejected
    }

    public class ComplainedRecipient
    {

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }
    }

    //https://docs.aws.amazon.com/ses/latest/dg/notification-contents.html#complaint-object
    public class ComplaintObject
    {

        [JsonProperty("feedbackId")]
        public string FeedbackId { get; set; }

        [JsonProperty("complaintSubType")]
        public string ComplaintSubType { get; set; }

        [JsonProperty("complaintFeedbackType")]
        public string ComplaintFeedbackType { get; set; }

        [JsonProperty("complainedRecipients")]
        public IList<ComplainedRecipient> ComplainedRecipients { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("arrivalDate")]
        public DateTime ArrivalDate { get; set; }
    }
}
