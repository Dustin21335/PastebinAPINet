namespace PastebinAPINet
{
    /// <summary>
    /// Visibility of a paste.
    /// </summary>
    public enum PasteExposure
    {
        /// <summary>
        /// Accessible by anyone.
        /// </summary>
        Public = 0,

        /// <summary>
        /// Only accessible with direct link.
        /// </summary>
        Unlisted = 1,

        /// <summary>
        /// Only accessible by the account owner.
        /// </summary>
        Private = 2,
    }
}
