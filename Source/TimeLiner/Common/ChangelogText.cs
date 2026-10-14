// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TimeLiner.Common
{
    /// <summary>Loads the bundled changelog and converts its Markdown subset to readable text.</summary>
    internal static class ChangelogText
    {
        public static string Load()
        {
            using Stream stream = typeof(ChangelogText).Assembly.GetManifestResourceStream("TimeLiner.CHANGELOG.md")
                ?? throw new InvalidOperationException("The bundled changelog could not be found.");
            using StreamReader reader = new(stream, Encoding.UTF8);
            string markdown = reader.ReadToEnd();
            // The dialog title supplies the context; start with the first release.
            Match firstRelease = Regex.Match(markdown, @"^##\s+", RegexOptions.Multiline);
            return Convert(firstRelease.Success ? markdown[firstRelease.Index..] : markdown);
        }

        public static string Convert(string markdown)
        {
            ArgumentNullException.ThrowIfNull(markdown);
            StringBuilder result = new();
            using StringReader reader = new(markdown);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Match heading = Regex.Match(line, @"^(#{1,6})\s+(.+?)\s*$");
                string text = heading.Success ? heading.Groups[2].Value : line;
                text = Regex.Replace(text, @"\[([^\]]+)\]\(([^)]+)\)", "$1 ($2)");
                // Versions use brackets without a link target in Keep a Changelog.
                if (heading.Success)
                    text = Regex.Replace(text, @"^\[([^\]]+)\]", "$1");
                text = Regex.Replace(text, @"`([^`]+)`", "$1");
                text = Regex.Replace(text, @"\*\*(.+?)\*\*", "$1");
                text = Regex.Replace(text, @"(?<!\*)\*([^*]+)\*(?!\*)", "$1");
                result.AppendLine(text);
                if (heading.Success && heading.Groups[1].Length <= 2)
                    result.AppendLine(new string('=', text.Length));
                else if (heading.Success && reader.Peek() != -1 && reader.Peek() != '\r' && reader.Peek() != '\n')
                    result.AppendLine();
            }
            return result.ToString();
        }
    }
}