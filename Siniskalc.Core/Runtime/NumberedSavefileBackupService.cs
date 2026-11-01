namespace Siniskalc.Core.Runtime
{
    using System;
    using System.Globalization;
    using System.IO;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Copies a file to <c>name.bak</c>, and when that exists to <c>name.bak1</c>, <c>name.bak2</c> and so on, one number higher than
    /// the highest copy there is. A copy is never overwritten. Has no settings; throws when the file does not exist.
    /// </summary>
    public sealed class NumberedSavefileBackupService : IFileBackupService
    {
        private const string Extension = ".bak";

        private static readonly ILogger<NumberedSavefileBackupService> Logger = LogFactory.For<NumberedSavefileBackupService>();

        /// <summary>
        /// Copies <paramref name="filePath"/> to the first <c>.bak</c> name that is free, one number above the highest copy there is.
        /// </summary>
        /// <param name="filePath">The path of the file to back up.</param>
        /// <returns>The path of the copy.</returns>
        /// <exception cref="FileNotFoundException">The file does not exist.</exception>
        /// <exception cref="IOException">The copy could not be made.</exception>
        public string Backup(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The file to back up does not exist.", filePath);
            }

            var next = HighestCopy(filePath) + 1;
            Logger.LogTrace("Backing up {File} as copy number {Index}", Path.GetFileName(filePath), next);
            while (true)
            {
                var target = PathOfCopy(filePath, next);
                try
                {
                    File.Copy(filePath, target, overwrite: false);
                    Logger.LogDebug("Backed up {File} as {Backup}", Path.GetFileName(filePath), Path.GetFileName(target));
                    return target;
                }
                catch (IOException) when (File.Exists(target))
                {
                    // Someone made that copy since we looked.
                    Logger.LogDebug("Backup {File} appeared while it was being made, trying the next number", Path.GetFileName(target));
                    next++;
                }
            }
        }

        private static string PathOfCopy(string filePath, int number) => number == 0 ? filePath + Extension : filePath + Extension + number;

        /// <summary>
        /// Returns the number of the highest copy: 0 for <c>name.bak</c>, -1 when there is no copy.
        /// </summary>
        /// <param name="filePath">The path of the file that is backed up.</param>
        /// <returns>The highest number found among the copies in the folder of the file.</returns>
        private static int HighestCopy(string filePath)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? string.Empty;
            var prefix = Path.GetFileName(filePath) + Extension;
            var highest = -1;
            foreach (var candidate in Directory.EnumerateFiles(directory, prefix + "*"))
            {
                var suffix = Path.GetFileName(candidate)[prefix.Length..];
                if (suffix.Length == 0)
                {
                    highest = Math.Max(highest, 0);
                }
                else if (int.TryParse(suffix, NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number > 0)
                {
                    highest = Math.Max(highest, number);
                }
            }

            return highest;
        }
    }
}
