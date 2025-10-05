namespace PastebinAPINet
{
    /// <summary>
    /// Pastebin API Options.
    /// </summary>
    public enum PasteOption
    {
        /// <summary>
        /// Creates a new paste.
        /// </summary>
        Paste,

        /// <summary>
        /// Gets a list of the users pastes.
        /// </summary>
        List,

        /// <summary>
        /// Deletes a paste.
        /// </summary>
        Delete,

        /// <summary>
        /// Gets details about the user.
        /// </summary>
        UserDetails,

        /// <summary>
        /// Gets the raw content of a paste.
        /// </summary>
        ShowPaste,
    }
}
