namespace Siniskalc.Tests.Homm.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Finds the lines of a game's view models that touch byte chunks or the byte layout of a struct, which CLAUDE.md forbids: view models
    /// ask the collection classes for models.
    /// </summary>
    public static class ViewModelLayering
    {
        /// <summary>
        /// Lists, as <c>File.cs:line: code</c>, each line of the view models in <paramref name="folder"/> that matches <paramref name="forbidden"/>.
        /// A line that is a comment is left out, and so is a file in <paramref name="allowed"/>.
        /// </summary>
        public static IReadOnlyList<string> Offenders(string folder, Regex forbidden, IReadOnlyCollection<string> allowed)
        {
            var offenders = new List<string>();
            foreach (var file in Directory.GetFiles(folder, "*.cs").Where(f => !allowed.Contains(Path.GetFileName(f))))
            {
                var lines = File.ReadAllLines(file);
                for (var i = 0; i < lines.Length; i++)
                {
                    var code = lines[i].TrimStart();
                    if (!code.StartsWith("//", StringComparison.Ordinal) && forbidden.IsMatch(code))
                    {
                        offenders.Add($"{Path.GetFileName(file)}:{i + 1}: {code}");
                    }
                }
            }

            return offenders;
        }
    }
}
