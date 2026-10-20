// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TimeLiner.Common;
using TimeLiner.Models;
using TimeLiner.ViewModels;
using TimeLinerTest.TestDoubles;

namespace TimeLinerTest
{
    [TestClass]
    public class TestTimeDisplay
    {
        [TestMethod]
        [DataRow("+00:00", 0)]
        [DataRow("-00:00", 0)]
        [DataRow("+02:00", 120)]
        [DataRow("-05:30", -330)]
        [DataRow("+05:45", 345)]
        [DataRow("+14:00", 840)]
        [DataRow("-14:00", -840)]
        public void ExplicitOffset_AcceptsWholeMinuteOffsets(string text, int minutes)
        {
            Assert.IsTrue(UtcOffset.TryParse(text, out TimeSpan offset));
            Assert.AreEqual(TimeSpan.FromMinutes(minutes), offset);
        }

        [TestMethod]
        [DataRow("+14:01")]
        [DataRow("-14:30")]
        [DataRow("+15:00")]
        [DataRow("+02:60")]
        [DataRow("+2:00")]
        [DataRow("02:00")]
        [DataRow("+02:00:01")]
        [DataRow("+02:00\n")]
        [DataRow("")]
        [DataRow(null)]
        public void ExplicitOffset_RejectsInvalidValues(string text)
        {
            Assert.IsFalse(UtcOffset.TryParse(text, out _));
        }

        [TestMethod]
        [DataRow("2026-10-25T00:00:00Z", 120, "2026-10-25 02:00:00.000 +02:00")]
        [DataRow("2026-10-25T01:00:00Z", 120, "2026-10-25 03:00:00.000 +02:00")]
        [DataRow("2026-10-25T00:00:00Z", 60, "2026-10-25 01:00:00.000 +01:00")]
        [DataRow("2026-10-25T01:00:00Z", 60, "2026-10-25 02:00:00.000 +01:00")]
        [DataRow("2026-10-25T00:00:00Z", -330, "2026-10-24 18:30:00.000 -05:30")]
        [DataRow("2026-10-25T00:00:00Z", 345, "2026-10-25 05:45:00.000 +05:45")]
        public void FixedOffset_DoesNotChangeAtRegionalDstBoundary(string utc, int minutes, string expected)
        {
            var provider = new TimeZoneInfoProviderStub("W. Europe Standard Time", false, TimeSpan.FromMinutes(minutes));
            Assert.AreEqual(expected, TimeFormat.GetTimeString(DateTimeOffset.Parse(utc).UtcDateTime, provider));
        }

        [TestMethod]
        [DataRow("2026-10-25T00:00:00Z", "2026-10-25 02:00:00.000 +02:00")]
        [DataRow("2026-10-25T01:00:00Z", "2026-10-25 02:00:00.000 +01:00")]
        public void RegionalTime_RepeatsHourWithDifferentOffsets(string utc, string expected)
        {
            var provider = new TimeZoneInfoProviderStub("W. Europe Standard Time", false);
            Assert.AreEqual(expected, TimeFormat.GetTimeString(DateTimeOffset.Parse(utc).UtcDateTime, provider));
        }

        [TestMethod]
        [DataRow("2026-10-25 02:00:00", "2026-10-25T00:00:00Z")]
        [DataRow("2026-10-25 02:00:00 +01:00", "2026-10-25T01:00:00Z")]
        [DataRow("2026-10-25T02:00:00Z", "2026-10-25T02:00:00Z")]
        public void EditorInput_UsesFixedOffsetUnlessInputHasItsOwnOffset(string text, string expected)
        {
            var provider = new TimeZoneInfoProviderStub("UTC", false, TimeSpan.FromHours(2));
            Assert.IsTrue(TimeFormat.TryParseUtcTime(text, provider, out DateTime actual));
            Assert.AreEqual(DateTimeOffset.Parse(expected).UtcDateTime, actual);
            Assert.AreEqual(DateTimeKind.Utc, actual.Kind);
        }

        [TestMethod]
        [DataRow("2026-01-15 12:00:00", "2026-01-15T11:00:00Z")]
        [DataRow("2026-07-15 12:00:00", "2026-07-15T10:00:00Z")]
        [DataRow("2026-10-25 02:30:00 +02:00", "2026-10-25T00:30:00Z")]
        [DataRow("2026-10-25 02:30:00 +01:00", "2026-10-25T01:30:00Z")]
        public void RegionalInput_UsesRulesForTheEventDate(string text, string expected)
        {
            var provider = new TimeZoneInfoProviderStub("W. Europe Standard Time", false);
            Assert.IsTrue(TimeFormat.TryParseUtcTime(text, provider, out DateTime actual));
            Assert.AreEqual(DateTimeOffset.Parse(expected).UtcDateTime, actual);
        }

        [TestMethod]
        [DataRow("2026-10-25 02:30:00")]
        [DataRow("2026-03-29 02:30:00")]
        public void RegionalInput_RejectsAmbiguousOrNonexistentClockTimesWithoutOffset(string text)
        {
            var provider = new TimeZoneInfoProviderStub("W. Europe Standard Time", false);
            Assert.IsFalse(TimeFormat.TryParseUtcTime(text, provider, out _));
        }

        [TestMethod]
        public void FixedOffset_OutsideSupportedDateRangeDoesNotCrashDisplay()
        {
            var provider = new TimeZoneInfoProviderStub("UTC", false, TimeSpan.FromHours(-5));
            Assert.AreEqual("Outside supported date range", TimeFormat.GetTimeString(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc), provider));
        }

        [TestMethod]
        [DataRow(true, TimeDisplayMode.FixedOffset)]
        [DataRow(false, TimeDisplayMode.Regional)]
        public void LegacySettings_PreserveDisplayChoice(bool utc, TimeDisplayMode expected)
        {
            var settings = CreateSettings(new SettingsModel { IsUniversalTime = utc });
            Assert.AreEqual(expected, settings.DisplayMode);
            Assert.AreEqual(utc, settings.IsUniversalTime);
        }

        [TestMethod]
        public void OffsetEditing_CommitsOrCancelsWithoutChangingInstantsOnInvalidInput()
        {
            var settings = CreateSettings();
            settings.SelectFixedOffset(TimeSpan.FromHours(2));
            settings.OffsetInput = "+05:45";
            Assert.AreEqual(TimeSpan.FromHours(2), settings.UtcOffset, "Typing must not apply partial edits.");
            Assert.IsTrue(settings.TryCommitOffsetInput());
            Assert.AreEqual(TimeSpan.FromMinutes(345), settings.UtcOffset);
            settings.OffsetInput = "+15:00";
            Assert.IsFalse(settings.TryCommitOffsetInput());
            Assert.IsTrue(settings.HasOffsetError);
            Assert.AreEqual(TimeSpan.FromMinutes(345), settings.UtcOffset);
            settings.CancelOffsetInput();
            Assert.AreEqual("+05:45", settings.OffsetInput);
            Assert.IsFalse(settings.HasOffsetError);
        }

        [TestMethod]
        public void FirstEventChoice_ReevaluatesOnLoad_AndCanBeOverriddenManually()
        {
            var settings = CreateSettings();
            settings.SetFirstEventOffset(TimeSpan.FromHours(2));
            settings.SelectFixedOffset(TimeSpan.Zero);
            settings.OffsetInput = settings.FirstEventOffsetLabel;
            Assert.IsTrue(settings.TryCommitOffsetInput());
            Assert.IsTrue(settings.UsesFirstEventOffset);
            Assert.AreEqual(TimeSpan.FromHours(2), settings.UtcOffset);
            settings.SetFirstEventOffset(TimeSpan.FromHours(1));
            Assert.AreEqual(TimeSpan.FromHours(1), settings.UtcOffset);
            Assert.AreEqual("First event (+01:00)", settings.OffsetInput);
            settings.SetFirstEventOffset(null);
            Assert.IsTrue(settings.HasOffsetError);
            Assert.AreEqual(TimeSpan.FromHours(1), settings.UtcOffset, "Missing metadata must retain the previous valid display.");
            settings.OffsetInput = "+05:30";
            Assert.IsTrue(settings.TryCommitOffsetInput());
            Assert.IsFalse(settings.UsesFirstEventOffset);
            settings.SetFirstEventOffset(TimeSpan.FromHours(-5));
            Assert.AreEqual(TimeSpan.FromMinutes(330), settings.UtcOffset);
        }

        [TestMethod]
        public void UnavailableFirstEventChoice_LeavesManualOffsetAndSourceUnchanged()
        {
            var settings = CreateSettings();
            settings.SelectFixedOffset(TimeSpan.FromHours(-5));
            settings.OffsetInput = settings.FirstEventOffsetLabel;
            Assert.IsFalse(settings.TryCommitOffsetInput());
            Assert.IsFalse(settings.UsesFirstEventOffset);
            Assert.AreEqual(TimeSpan.FromHours(-5), settings.UtcOffset);
        }

        [TestMethod]
        public void Toggle_PreservesSelectedOffset_AndNotifiesExistingTimeViews()
        {
            var settings = CreateSettings();
            settings.SelectFixedOffset(TimeSpan.FromHours(2));
            int notifications = 0;
            settings.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(settings.IsUniversalTime)) notifications++; };
            settings.ToggleTimeFormat();
            Assert.IsTrue(settings.IsRegionalTime);
            settings.ToggleTimeFormat();
            Assert.IsTrue(settings.IsFixedOffset);
            Assert.AreEqual(TimeSpan.FromHours(2), settings.UtcOffset);
            Assert.AreEqual(2, notifications);
        }

        [TestMethod]
        [DataRow("2026-10-25T02:00:00+02:00", 120)]
        [DataRow("2026-10-25T02:00:00-05:30", -330)]
        [DataRow("2026-10-25T02:00:00Z", 0)]
        [DataRow("2026-10-25T02:00:00", int.MaxValue)]
        public async Task Import_FirstEventMeansFileOrder_NotChronologicalOrder(string firstStart, int expectedMinutes)
        {
            string csv = $"# ignored comment\nTimeLine,Item,Start,End,Color\nEmpty,,,,\nLater,First,{firstStart},,\nEarlier,Second,2025-01-01T12:00:00+01:00,,\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            TimeLinesModel model = await TimeLinesModel.LoadAsync(stream);
            TimeSpan? expected = expectedMinutes == int.MaxValue ? null : TimeSpan.FromMinutes(expectedMinutes);
            Assert.AreEqual(expected, model.FirstEventOffset);
            Assert.AreEqual(DateTimeKind.Utc, model.TimeLines[1].TimeLineItems[0].StartTime.Kind);
            TimeLinesModel clone = await model.CloneAsync();
            Assert.AreEqual(expected, clone.FirstEventOffset, "Undo snapshots must preserve import context.");
        }

        [TestMethod]
        public async Task Import_UsesFirstStart_NotItsEnd_AndExportStillWritesUtc()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Line,Span,2026-10-25T02:00:00+02:00,2026-10-25T02:00:00+01:00,\n"));
            TimeLinesModel model = await TimeLinesModel.LoadAsync(stream);
            Assert.AreEqual(TimeSpan.FromHours(2), model.FirstEventOffset);
            var item = model.TimeLines[0].TimeLineItems[0];
            Assert.AreEqual(TimeSpan.FromHours(1), item.EndTime - item.StartTime);
            using var saved = new MemoryStream();
            await model.SaveAsync(saved);
            saved.Position = 0;
            TimeLinesModel reloaded = await TimeLinesModel.LoadAsync(saved);
            Assert.AreEqual(TimeSpan.Zero, reloaded.FirstEventOffset);
            Assert.AreEqual(item.StartTime, reloaded.TimeLines[0].TimeLineItems[0].StartTime);
        }

        [TestMethod]
        public async Task Import_EmptyFileHasNoOffset()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("TimeLine,Item,Start,End,Color\nEmpty,,,,\n"));
            Assert.IsNull((await TimeLinesModel.LoadAsync(stream)).FirstEventOffset);
        }

        [TestMethod]
        public async Task LoadingAndUndo_KeepAutomaticOffsetBasedOnImportedFile()
        {
            string firstPath = Path.Combine(Path.GetTempPath(), $"timeliner-offset-{Guid.NewGuid():N}.csv");
            string secondPath = Path.Combine(Path.GetTempPath(), $"timeliner-offset-{Guid.NewGuid():N}.csv");
            var settings = CreateSettings();
            settings.SelectFixedOffset(TimeSpan.Zero, fromFirstEvent: true);
            var timelines = new TimeLinesViewModel(new DialogServiceStub(), settings, new TimeLineScalingViewModel(settings));
            try
            {
                await File.WriteAllTextAsync(firstPath, "Line,Summer,2026-10-25T02:00:00+02:00,,\nLine,Winter,2026-10-25T02:00:00+01:00,,\n");
                await File.WriteAllTextAsync(secondPath, "Line,First,2026-01-15T12:00:00+01:00,,\nLine,Earlier,2025-01-15T12:00:00+03:00,,\n");
                await timelines.LoadAsync(firstPath, 1000);
                Assert.AreEqual(TimeSpan.FromHours(2), settings.UtcOffset);
                Assert.AreEqual(TimeSpan.FromHours(1), timelines.TimeLineItems[1].StartTime - timelines.TimeLineItems[0].StartTime);

                await timelines.CaptureUndoAsync();
                var edited = timelines.TimeLineItems[0];
                edited.StartTime = DateTimeOffset.Parse("2026-10-25T04:00:00Z").UtcDateTime;
                edited.EndTime = edited.StartTime;
                Assert.AreEqual(TimeSpan.FromHours(2), settings.UtcOffset);
                await timelines.UndoAsync();
                Assert.AreEqual(TimeSpan.FromHours(2), settings.UtcOffset);
                await timelines.RedoAsync();
                Assert.AreEqual(TimeSpan.FromHours(2), settings.UtcOffset);

                await timelines.LoadAsync(secondPath, 1000);
                Assert.AreEqual(TimeSpan.FromHours(1), settings.UtcOffset);
                Assert.AreEqual("First event (+01:00)", settings.OffsetInput);
                timelines.Create(1000);
                Assert.IsNull(settings.FirstEventOffset);
                Assert.IsTrue(settings.HasOffsetError);
                Assert.AreEqual(TimeSpan.FromHours(1), settings.UtcOffset);
            }
            finally
            {
                File.Delete(firstPath);
                File.Delete(secondPath);
            }
        }

        [TestMethod]
        [DataRow("+02:00", 120)]
        [DataRow("-05:30", -330)]
        [DataRow("+00:00", 0)]
        public void CommandLine_AcceptsExplicitOffset(string argument, int minutes)
        {
            var options = TimeDisplayOptions.Parse(["app.exe", "data.csv", "-offset", argument]);
            Assert.AreEqual(TimeSpan.FromMinutes(minutes), options.Offset);
            Assert.IsFalse(options.AutoOffset);
            Assert.IsNull(options.TimeZoneId);
        }

        [TestMethod]
        public void CommandLine_AcceptsAutoAndExistingZoneOptions()
        {
            Assert.IsTrue(TimeDisplayOptions.Parse(["-OFFSET", "AUTO"]).AutoOffset);
            Assert.AreEqual(TimeZoneInfo.Local.Id, TimeDisplayOptions.Parse(["-timezone", "local"]).TimeZoneId);
            Assert.AreEqual("UTC", TimeDisplayOptions.Parse(["-timezone", "UTC"]).TimeZoneId);
            Assert.IsNull(TimeDisplayOptions.Parse(["-minimal", "data.csv"]).Offset);
        }

        [TestMethod]
        [DataRow("-offset", "+02:00", "-timezone", "UTC")]
        [DataRow("-timezone", "UTC", "-offset", "auto")]
        [DataRow("-offset", "+02:00", "-offset", "+01:00")]
        [DataRow("-timezone", "UTC", "-timezone", "local")]
        public void CommandLine_RejectsConflictingOrRepeatedOptions(string a, string b, string c, string d)
        {
            Assert.ThrowsExactly<ArgumentException>(() => TimeDisplayOptions.Parse([a,b,c,d]));
        }

        [TestMethod]
        [DataRow("-offset", "+15:00")]
        [DataRow("-offset", "+02:60")]
        [DataRow("-offset", "Berlin")]
        [DataRow("-timezone", "missing-zone")]
        public void CommandLine_RejectsInvalidValues(string option, string value)
        {
            Assert.ThrowsExactly<ArgumentException>(() => TimeDisplayOptions.Parse([option,value]));
        }

        [TestMethod]
        [DataRow("-offset")]
        [DataRow("-timezone")]
        public void CommandLine_RejectsMissingValue(string option)
        {
            Assert.ThrowsExactly<ArgumentException>(() => TimeDisplayOptions.Parse([option]));
        }

        private static SettingsViewModel CreateSettings(SettingsModel model = null) =>
            new(new SettingsRepositoryStub(model ?? new SettingsModel()));
    }
}
