using System.Xml.Serialization;

namespace PastebinAPINet
{
    #pragma warning disable CS1591
    [XmlRoot("user")]
    public class PastebinUserDetails
    {
        [XmlElement("user_name")]
        public string Name { get; set; } = string.Empty;

        [XmlElement("user_format_short")]
        public string FormatShort { get; set; } = string.Empty;

        [XmlElement("user_expiration")]
        public string Expiration { get; set; } = string.Empty;

        [XmlElement("user_avatar_url")]
        public string AvatarUrl { get; set; } = string.Empty;

        [XmlElement("user_private")]
        public int Private { get; set; }

        [XmlIgnore]
        public PasteExposure Exposure => (PasteExposure)Private;

        [XmlElement("user_website")]
        public string Website { get; set; } = string.Empty;

        [XmlElement("user_email")]
        public string Email { get; set; } = string.Empty;

        [XmlElement("user_location")]
        public string Location { get; set; } = string.Empty;

        [XmlElement("user_account_type")]
        public int accountType { get; set; }

        [XmlIgnore]
        public PastebinAccountType AccountType => (PastebinAccountType)accountType;
    }
}