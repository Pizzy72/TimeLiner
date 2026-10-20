// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeLiner.Common;

namespace TimeLinerTest
{
    [TestClass]
    public class TestChangelogText
    {
        [TestMethod]
        public void ConvertsVersionAndSectionHeadings()
        {
            string result = ChangelogText.Convert("## [2.15.5] - 2026-10-13\n\n### Fixed\n- Fehler behoben.");
            string expected = string.Join(Environment.NewLine,
                "2.15.5 - 2026-10-13", "===================", "", "Fixed", "", "- Fehler behoben.", "");
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void PreservesLinksUnicodeAndLiteralCharacters()
        {
            Assert.AreEqual("Über [Text] — Ctrl+Plus, fett, kursiv; Guide (https://example.org); a_b * 2" + Environment.NewLine,
                ChangelogText.Convert("Über [Text] — `Ctrl+Plus`, **fett**, *kursiv*; [Guide](https://example.org); a_b * 2"));
        }

        [TestMethod]
        public void AcceptsAllCommonLineEndings()
        {
            foreach (string newline in new[] { "\n", "\r\n", "\r" })
                Assert.AreEqual("One" + Environment.NewLine + Environment.NewLine + "- Two" + Environment.NewLine,
                    ChangelogText.Convert("One" + newline + newline + "- Two"));
            Assert.AreEqual("", ChangelogText.Convert(""));
        }

        [TestMethod]
        public void LoadsBundledChangelog()
        {
            string text = ChangelogText.Load();
            Assert.IsTrue(text.StartsWith("Unreleased" + Environment.NewLine));
            StringAssert.Contains(text, "2.15.6 - 2026-10-14");
            Assert.IsFalse(text.Contains("All notable changes"));
            StringAssert.Contains(text, "Changed" + Environment.NewLine + Environment.NewLine + "- ");
            StringAssert.Contains(text, "2.15.5 - 2026-10-13");
            Assert.IsFalse(text.Contains("## ["));
        }
    }
}
