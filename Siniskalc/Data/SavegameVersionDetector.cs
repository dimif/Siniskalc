namespace Siniskalc.Data
{
    using System;
    using System.Text;
    using Siniskalc.Entities;

    /// <summary>
    /// Tells which game version wrote a savefile, from its header.
    /// </summary>
    public sealed class SavegameVersionDetector : ISavegameVersionDetector
    {
        private static readonly Encoding HeaderEncoding = Encoding.Latin1;

        public GameVersion Detect(byte[] bytes)
        {
            if (bytes.Length >= SaveHeaderLayout.ChroniclesSignature.Length
                && HeaderEncoding.GetString(bytes, 0, SaveHeaderLayout.ChroniclesSignature.Length) == SaveHeaderLayout.ChroniclesSignature)
            {
                return GameVersion.HeroesChronicles;
            }

            var signature = bytes.Length >= SaveHeaderLayout.Signature.Length
                ? HeaderEncoding.GetString(bytes, 0, SaveHeaderLayout.Signature.Length)
                : string.Empty;
            if (bytes.Length < SaveHeaderLayout.MinimumLength
                || (signature != SaveHeaderLayout.Signature && signature != SaveHeaderLayout.CampaignSignature))
            {
                return GameVersion.Unknown;
            }

            var major = bytes[SaveHeaderLayout.MajorVersion];
            var minor = bytes[SaveHeaderLayout.MinorVersion];

            if (major >= 16 && major <= 41 && minor == 0)
            {
                return GameVersion.RestorationOfErathia;
            }

            if (major == 42 && minor == 1)
            {
                return GameVersion.ArmageddonsBlade;
            }

            if (major >= 42 && major <= 43 && minor <= 4)
            {
                return GameVersion.ShadowOfDeath;
            }

            if (major >= 44 && minor >= 5)
            {
                return GameVersion.HornOfTheAbyss;
            }

            return GameVersion.Unknown;
        }
    }
}
