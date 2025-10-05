using System.Xml.Serialization;

namespace PastebinAPINet
{
    [XmlRoot("paste")]
    public class PastebinPaste
    {
        [XmlElement("paste_key")]
        public string Key { get; set; } = string.Empty;

        [XmlElement("paste_date")]
        public long Date { get; set; }

        [XmlIgnore]
        public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(Date).DateTime;

        [XmlElement("paste_title")]
        public string Name { get; set; } = string.Empty;

        [XmlElement("paste_size")]
        public int Size { get; set; }

        [XmlElement("paste_expire_date")]
        public long ExpireDate { get; set; }

        [XmlIgnore]
        public DateTime ExpireDateTime => DateTimeOffset.FromUnixTimeSeconds(ExpireDate).DateTime;

        [XmlElement("paste_private")]
        public int Private { get; set; }

        [XmlIgnore]
        public PasteExposure Exposure => (PasteExposure)Private;

        [XmlElement("paste_format_long")]
        public string FormatLong { get; set; } = string.Empty;

        [XmlElement("paste_format_short")]
        public string FormatShort { get; set; } = string.Empty;

        [XmlElement("paste_url")]
        public string Url { get; set; } = string.Empty;

        [XmlElement("paste_hits")]
        public int Views { get; set; }
    }

    [XmlRoot("pastes")]
    public class PasteList
    {
        [XmlElement("paste")]
        public List<PastebinPaste> Pastes { get; set; } = new List<PastebinPaste>();
    }
}
