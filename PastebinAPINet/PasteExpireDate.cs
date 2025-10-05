namespace PastebinAPINet
{    /// <summary>
     /// Expiration time for a paste.
     /// </summary>
    public enum PasteExpireDate
    {
        /// <summary>
        /// Paste never expires.
        /// </summary>
        Never,

        /// <summary>
        /// Paste expires after 10 minutes.
        /// </summary>
        TenMinutes,

        /// <summary>
        /// Expires after 1 hour.
        /// </summary>
        OneHour,

        /// <summary>
        /// Expires after 1 day.
        /// </summary>
        OneDay,

        /// <summary>
        /// Expires after 1 week.
        /// </summary>
        OneWeek,

        /// <summary>
        /// Expires after 2 weeks.
        /// </summary>
        TwoWeeks,

        /// <summary>
        /// Expires after 1 month.
        /// </summary>
        OneMonth,

        /// <summary>
        /// Expires after 6 months.
        /// </summary>
        SixMonths,

        /// <summary>
        /// Expires after 1 year.
        /// </summary>
        OneYear,
    }
}