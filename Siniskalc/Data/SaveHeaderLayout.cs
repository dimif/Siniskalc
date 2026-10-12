namespace Siniskalc.Data
{
    /// <summary>
    /// Where the signature and the format version are in the header of a savefile, and what the signatures say.
    /// </summary>
    public static class SaveHeaderLayout
    {
        /// <summary>The signature of a Heroes Chronicles savefile.</summary>
        public const string ChroniclesSignature = "HCHRONSVG";

        /// <summary>The signature of a savefile of the other versions.</summary>
        public const string Signature = "H3SVG";

        /// <summary>The signature of a savefile of a campaign.</summary>
        public const string CampaignSignature = "H3SVC";

        /// <summary>The offset of the major format version, one byte.</summary>
        public const int MajorVersion = 8;

        /// <summary>The offset of the minor format version, one byte.</summary>
        public const int MinorVersion = 12;

        /// <summary>The smallest length a savefile has to have to hold the signature and both versions.</summary>
        public const int MinimumLength = MinorVersion + 1;
    }
}
