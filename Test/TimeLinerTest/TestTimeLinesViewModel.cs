// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using TimeLiner.Common;
using TimeLiner.Models;
using TimeLiner.UI;
using TimeLiner.ViewModels;
using TimeLinerTest.TestDoubles;

namespace TimeLinerTest
{
    [TestClass]
    public class TestTimeLinesViewModel
    {
        private DialogServiceStub _dialogService;
        private TimeLinesViewModel _timeLinesViewModel;
        private SettingsViewModel _settingsViewModel;
        private TimeLineScalingViewModel _timeLineScalingViewModel;
        private SettingsRepositoryStub _settingsRepository;

        [TestInitialize()]
        public void TestInitialize()
        {
            _dialogService = new DialogServiceStub();
            _settingsRepository = CreateSettingsRepository();
            _settingsViewModel = new SettingsViewModel(_settingsRepository);
            _timeLineScalingViewModel = new TimeLineScalingViewModel(_settingsViewModel);
            _timeLinesViewModel = new TimeLinesViewModel(_dialogService, _settingsViewModel, _timeLineScalingViewModel);
        }

        [TestMethod]
        public async Task LoadAsync_ValidFile_LoadsTimeLines()
        {
            // Arrange
            string filePath = @"TestData\Minutes.csv";

            // Act
            await _timeLinesViewModel.LoadAsync(filePath, 1000d);

            // Assert
            Assert.AreEqual(5, _timeLinesViewModel.TimeLines.Count);
            Assert.AreEqual(5, _timeLinesViewModel.TimeLineItems.Count);
            Assert.IsFalse(_timeLinesViewModel.IsModified);
        }

        [TestMethod]
        public async Task LoadAsync_MissingFile_ThrowsException()
        {
            // Arrange
            string filePath = @"TestData\MissingFile.csv";

            try
            {
                // Act
                await _timeLinesViewModel.LoadAsync(filePath, 1000d);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsInstanceOfType(ex, typeof(TimeLinerException));
            }
        }

        [TestMethod]
        public async Task LoadAsync_BadFile_ThrowsException()
        {
            // Arrange

            try
            {
                // Act
                await _timeLinesViewModel.LoadAsync(@"TestData\BadFile.csv", 1000d);
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsInstanceOfType(ex, typeof(TimeLinerException));
            }
        }


        [TestMethod]
        public async Task Create_ModelIsLoaded_ModelIsEmpty()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _timeLinesViewModel.Create(1000d);

            // Assert
            Assert.IsFalse(_timeLinesViewModel.IsModified);
            Assert.AreEqual(0, _timeLinesViewModel.TimeLines.Count);
            Assert.AreEqual(0, _timeLinesViewModel.TimeLineItems.Count);
        }

        [TestMethod]
        public async Task LoadAsync_LoadBadFileOverLoadedModel_ModelIsEmpty()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            try
            {
                await _timeLinesViewModel.LoadAsync(@"TestData\BadFile.csv", 1000d);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(TimeLinerException));
            }

            // Assert
            Assert.IsFalse(_timeLinesViewModel.IsModified);
            Assert.AreEqual(0, _timeLinesViewModel.TimeLines.Count);
            Assert.AreEqual(0, _timeLinesViewModel.TimeLineItems.Count);
        }

        [TestMethod]
        public async Task LoadAsync_LoadValidFileOverLoadedModel_ModelIsLoaded()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            await _timeLinesViewModel.LoadAsync(@"TestData\OneTimeLine.csv", 1000d);

            // Assert
            Assert.IsFalse(_timeLinesViewModel.IsModified);
            Assert.AreEqual(1, _timeLinesViewModel.TimeLines.Count);
            Assert.AreEqual(1, _timeLinesViewModel.TimeLineItems.Count);
        }

        [TestMethod]
        public async Task TimeLineItem_SetColor_IsModified()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _timeLinesViewModel.TimeLineItems.First().Color = Colors.Green;

            // Assert
            Assert.IsTrue(_timeLinesViewModel.IsModified);
        }

        [TestMethod]
        public async Task TimeLineItem_SetName_IsModified()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _timeLinesViewModel.TimeLineItems.First().Name = "Test";

            // Assert
            Assert.IsTrue(_timeLinesViewModel.IsModified);
        }

        [TestMethod]
        public async Task TimeLineItem_SetStartTime_IsModified()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _timeLinesViewModel.TimeLineItems.First().StartTime = DateTime.MinValue;

            // Assert
            Assert.IsTrue(_timeLinesViewModel.IsModified);
        }

        [TestMethod]
        public async Task TimeLineItem_SetEndTime_IsModified()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _timeLinesViewModel.TimeLineItems.First().EndTime = DateTime.MaxValue;

            // Assert
            Assert.IsTrue(_timeLinesViewModel.IsModified);
        }

        [TestMethod]
        public async Task TimeLine_SetName_IsModified()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _timeLinesViewModel.TimeLines.First().Name = "Test";

            // Assert
            Assert.IsTrue(_timeLinesViewModel.IsModified);
        }

        [DataRow(0d)]
        [DataRow(100d)]
        [DataRow(200d)]
        [TestMethod]
        public async Task DeltaFirstLastItem_AfterScrolling_YieldsExpectedDuration(double horizontalScrollOffset)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _timeLinesViewModel.Scale = ScaleIndex.TenSeconds;

            // Act
            _timeLinesViewModel.HorizontalScrollOffset = horizontalScrollOffset;

            // Assert
            Assert.AreEqual("5 min", _timeLinesViewModel.DeltaFirstLastItem);
        }

        [DataRow(ScaleIndex.HalfMinute)]
        [DataRow(ScaleIndex.TenSeconds)]
        [DataRow(ScaleIndex.FiveSeconds)]
        [TestMethod]
        public async Task DeltaFirstLastItem_AfterZooming_YieldsExpectedDuration(ScaleIndex scale)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _timeLinesViewModel.Scale = scale;

            // Assert
            Assert.AreEqual("5 min", _timeLinesViewModel.DeltaFirstLastItem);
        }

        [DataRow(0d)]
        [DataRow(100d)]
        [DataRow(200d)]
        [TestMethod]
        public async Task DurationAllItems_AfterScrolling_YieldsExpectedDuration(double horizontalScrollOffset)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _timeLinesViewModel.Scale = ScaleIndex.TenSeconds;

            // Act
            _timeLinesViewModel.HorizontalScrollOffset = horizontalScrollOffset;

            // Assert
            Assert.AreEqual("5 min", _timeLinesViewModel.DurationAllItems);
        }

        [DataRow(ScaleIndex.HalfMinute)]
        [DataRow(ScaleIndex.TenSeconds)]
        [DataRow(ScaleIndex.FiveSeconds)]
        [TestMethod]
        public async Task DurationAllItems_AfterZooming_YieldsExpectedDuration(ScaleIndex scale)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _timeLinesViewModel.Scale = scale;

            // Assert
            Assert.AreEqual("5 min", _timeLinesViewModel.DurationAllItems);
        }

        [TestMethod]
        public async Task EndLocator_ForCompactGrid_YieldsExpectedX()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1100d);

            // Act
            _settingsViewModel.IsCompactTimeGrid = true;

            // Assert
            Assert.AreEqual(550d, _timeLinesViewModel.EndTimeLocatorViewModel.X);
        }

        [TestMethod]
        public async Task EndLocator_ForLocalTime_YieldsExpectedTime()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _settingsViewModel.TimeZone = _settingsViewModel.TimeZones.First(tz => tz.DisplayName.Contains("Saratov")).Id;
            _settingsViewModel.IsUniversalTime = false;

            // Assert
            Assert.AreEqual("2020-01-01 12:05:00.000 +04:00", _timeLinesViewModel.EndTimeLocatorViewModel.TimeText);
        }

        [DataRow(0d, "2020-01-01 08:05:00.000 Z")]
        [DataRow(100d, "2020-01-01 08:05:10.000 Z")]
        [DataRow(200d, "2020-01-01 08:05:20.000 Z")]
        [TestMethod]
        public async Task EndLocator_WhenLockedAfterScrolling_YieldsExpectedTime(double horizontalScrollOffset, string expectedTime)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _timeLinesViewModel.Scale = ScaleIndex.TenSeconds;
            _settingsViewModel.IsTimeLocatorLocked = true;

            // Act
            _timeLinesViewModel.HorizontalScrollOffset = horizontalScrollOffset;

            // Assert
            Assert.AreEqual(expectedTime, _timeLinesViewModel.EndTimeLocatorViewModel.TimeText);
        }

        [DataRow(ScaleIndex.HalfMinute, "2020-01-01 08:05:00.000 Z")]
        [DataRow(ScaleIndex.TenSeconds, "2020-01-01 08:01:40.000 Z")]
        [DataRow(ScaleIndex.FiveSeconds, "2020-01-01 08:00:50.000 Z")]
        [TestMethod]
        public async Task EndLocator_WhenLockedAfterZooming_YieldsExpectedTime(ScaleIndex scale, string expectedTime)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _settingsViewModel.IsTimeLocatorLocked = true;

            // Act
            _timeLinesViewModel.Scale = scale;

            // Assert
            Assert.AreEqual(expectedTime, _timeLinesViewModel.EndTimeLocatorViewModel.TimeText);
        }

        [DataRow(0d)]
        [DataRow(100d)]
        [DataRow(200d)]
        [TestMethod]
        public async Task EndLocator_AfterScrolling_YieldsExpectedTime(double horizontalScrollOffset)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _timeLinesViewModel.Scale = ScaleIndex.TenSeconds;

            // Act
            _timeLinesViewModel.HorizontalScrollOffset = horizontalScrollOffset;

            // Assert
            Assert.AreEqual("2020-01-01 08:05:00.000 Z", _timeLinesViewModel.EndTimeLocatorViewModel.TimeText);
        }

        [DataRow(150d, "2020-01-01 08:00:30.000 Z")]
        [DataRow(200d, "2020-01-01 08:01:00.000 Z")]
        [DataRow(250d, "2020-01-01 08:01:30.000 Z")]
        [TestMethod]
        public async Task EndLocator_AfterSettingX_YieldsExpectedTime(double endLocatorX, string expectedTimeText)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _timeLinesViewModel.Scale = ScaleIndex.OneMinute;

            // Act
            _timeLinesViewModel.EndTimeLocatorViewModel.X = endLocatorX;

            // Assert
            Assert.AreEqual(expectedTimeText, _timeLinesViewModel.EndTimeLocatorViewModel.TimeText);
        }

        [DataRow(ScaleIndex.HalfMinute)]
        [DataRow(ScaleIndex.TenSeconds)]
        [DataRow(ScaleIndex.FiveSeconds)]
        [TestMethod]
        public async Task EndLocator_AfterZooming_YieldsExpectedTime(ScaleIndex scale)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _timeLinesViewModel.Scale = scale;

            // Assert
            Assert.AreEqual("2020-01-01 08:05:00.000 Z", _timeLinesViewModel.EndTimeLocatorViewModel.TimeText);
        }

        [TestMethod]
        public async Task EndLocator_AfterZooming_YieldsExpectedX()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1200d);

            // Act
            _timeLinesViewModel.Scale = ScaleIndex.TenSeconds;

            // Assert
            Assert.AreEqual(3100d, _timeLinesViewModel.EndTimeLocatorViewModel.X);
        }

        [TestMethod]
        public async Task EndLocator_AfterResizing_YieldsExpectedX()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 800d);
            _timeLinesViewModel.Scale = ScaleIndex.HalfMinute;

            // Act
            _timeLinesViewModel.TimeLinesVisibleWidth = 1200d;

            // Assert
            Assert.AreEqual(1100d, _timeLinesViewModel.EndTimeLocatorViewModel.X);
        }


        [TestMethod]
        public async Task EndLocator_AfterResizingSmaller_IsVisible()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 800d);
            _timeLinesViewModel.Scale = ScaleIndex.HalfMinute;

            // Act
            _timeLinesViewModel.TimeLinesVisibleWidth = 1200d;

            // Assert
            Assert.AreEqual(true, _timeLinesViewModel.EndTimeLocatorViewModel.IsVisible);
        }


        [TestMethod]
        public async Task EndLocator_AfterResizingBigger_IsHidden()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1200d);
            _timeLinesViewModel.Scale = ScaleIndex.HalfMinute;

            // Act
            _timeLinesViewModel.TimeLinesVisibleWidth = 800d;

            // Assert
            Assert.AreEqual(false, _timeLinesViewModel.EndTimeLocatorViewModel.IsVisible);
        }


        [DataRow(0d)]
        [DataRow(100d)]
        [DataRow(200d)]
        [TestMethod]
        public async Task StartAndEndLocator_AfterScrolling_YieldExpectedDelta(double horizontalScrollOffset)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _timeLinesViewModel.Scale = ScaleIndex.TenSeconds;

            // Act
            _timeLinesViewModel.HorizontalScrollOffset = horizontalScrollOffset;
            _timeLinesViewModel.StartTimeLocatorViewModel.X = 100d;
            _timeLinesViewModel.EndTimeLocatorViewModel.X = 200d;

            // Assert
            Assert.AreEqual("10 s", _timeLinesViewModel.LocatorDelta);
        }

        [DataRow(100d, 100d, "0 s")]
        [DataRow(100d, 200d, "1 min")]
        [DataRow(200d, 100d, "1 min")]
        [TestMethod]
        public async Task StartAndEndLocator_AfterSettingX_YieldExpectedDelta(double startLocatorX, double endLocatorX, string expectedDelta)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _timeLinesViewModel.Scale = ScaleIndex.OneMinute;

            // Act
            _timeLinesViewModel.StartTimeLocatorViewModel.X = startLocatorX;
            _timeLinesViewModel.EndTimeLocatorViewModel.X = endLocatorX;

            // Assert
            Assert.AreEqual(expectedDelta, _timeLinesViewModel.LocatorDelta);
        }

        [TestMethod]
        public async Task StartLocator_ForCompactGrid_YieldsExpectedX()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1100d);

            // Act
            _settingsViewModel.IsCompactTimeGrid = true;

            // Assert
            Assert.AreEqual(50d, _timeLinesViewModel.StartTimeLocatorViewModel.X);
        }

        [TestMethod]
        public async Task StartLocator_ForLocalTime_YieldsExpectedTime()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _settingsViewModel.TimeZone = _settingsViewModel.TimeZones.First(tz => tz.DisplayName.Contains("Saratov")).Id;
            _settingsViewModel.IsUniversalTime = false;

            // Assert
            Assert.AreEqual("2020-01-01 12:00:00.000 +04:00", _timeLinesViewModel.StartTimeLocatorViewModel.TimeText);
        }

        [DataRow(0d, "2020-01-01 08:00:00.000 Z")]
        [DataRow(100d, "2020-01-01 08:00:10.000 Z")]
        [DataRow(200d, "2020-01-01 08:00:20.000 Z")]
        [TestMethod]
        public async Task StartLocator_WhenLockedAfterScrolling_YieldsExpectedTime(double horizontalScrollOffset, string expectedTime)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _timeLinesViewModel.Scale = ScaleIndex.TenSeconds;
            _settingsViewModel.IsTimeLocatorLocked = true;

            // Act
            _timeLinesViewModel.HorizontalScrollOffset = horizontalScrollOffset;

            // Assert
            Assert.AreEqual(expectedTime, _timeLinesViewModel.StartTimeLocatorViewModel.TimeText);
        }

        [DataRow(ScaleIndex.HalfMinute)]
        [DataRow(ScaleIndex.TenSeconds)]
        [DataRow(ScaleIndex.FiveSeconds)]
        [TestMethod]
        public async Task StartLocator_WhenLockedAfterZooming_YieldsExpectedTime(ScaleIndex scale)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _settingsViewModel.IsTimeLocatorLocked = true;

            // Act
            _timeLinesViewModel.Scale = scale;

            // Assert
            Assert.AreEqual("2020-01-01 08:00:00.000 Z", _timeLinesViewModel.StartTimeLocatorViewModel.TimeText);
        }

        [DataRow(0d)]
        [DataRow(100d)]
        [DataRow(200d)]
        [TestMethod]
        public async Task StartLocator_AfterScrolling_YieldsExpectedTime(double horizontalScrollOffset)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _timeLinesViewModel.Scale = ScaleIndex.TenSeconds;

            // Act
            _timeLinesViewModel.HorizontalScrollOffset = horizontalScrollOffset;

            // Assert
            Assert.AreEqual("2020-01-01 08:00:00.000 Z", _timeLinesViewModel.StartTimeLocatorViewModel.TimeText);
        }

        [DataRow(150d, "2020-01-01 08:00:30.000 Z")]
        [DataRow(200d, "2020-01-01 08:01:00.000 Z")]
        [DataRow(250d, "2020-01-01 08:01:30.000 Z")]
        [TestMethod]
        public async Task StartLocator_AfterSettingX_YieldsExpectedTime(double startLocatorX, string expectedTimeText)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _timeLinesViewModel.Scale = ScaleIndex.OneMinute;

            // Act
            _timeLinesViewModel.StartTimeLocatorViewModel.X = startLocatorX;

            // Assert
            Assert.AreEqual(expectedTimeText, _timeLinesViewModel.StartTimeLocatorViewModel.TimeText);
        }

        [DataRow(ScaleIndex.HalfMinute)]
        [DataRow(ScaleIndex.TenSeconds)]
        [DataRow(ScaleIndex.FiveSeconds)]
        [TestMethod]
        public async Task StartLocator_AfterZooming_YieldsExpectedTime(ScaleIndex scale)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _timeLinesViewModel.Scale = scale;

            // Assert
            Assert.AreEqual("2020-01-01 08:00:00.000 Z", _timeLinesViewModel.StartTimeLocatorViewModel.TimeText);
        }

        [TestMethod]
        public async Task StartLocator_AfterZooming_YieldsExpectedX()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1100d);

            // Act
            _timeLinesViewModel.Scale = ScaleIndex.TenSeconds;

            // Assert
            Assert.AreEqual(100d, _timeLinesViewModel.StartTimeLocatorViewModel.X);
        }

        [TestMethod]
        public async Task TimeLines_AfterReloadingLessTimeLines_YieldsOnlyVisibleTimeLines()
        {
            // Arrange
            _timeLinesViewModel.TimeLinesVisibleHeight = 150;

            // Act
            double timeLinesVisibleWidth = 1000d;
            await _timeLinesViewModel.LoadAsync(@"TestData\ManyTimeLines.csv", timeLinesVisibleWidth);
            _timeLinesViewModel.VerticalScrollOffset = 30;
            await _timeLinesViewModel.LoadAsync(@"TestData\OneTimeLine.csv", timeLinesVisibleWidth);
            List<TimeLineViewModel> timeLines = _timeLinesViewModel.TimeLineCollectionView.Cast<TimeLineViewModel>().ToList();

            // Assert
            Assert.AreEqual(1, timeLines.Count);
        }

        [DataRow(0d, 0)]
        [DataRow(30d, 1)]
        [DataRow(35d, 2)]
        [DataRow(60d, 2)]
        [DataRow(70d, 3)]
        [DataRow(90d, 3)]
        [DataRow(200d, 5)]
        [TestMethod]
        public async Task TimeLines_ForVisibleHeight_YieldsOnlyVisibleTimeLines(double timeLinesVisibleHeight, int expectedTimeLineCount)
        {
            // Arrange
            _timeLinesViewModel.TimeLinesVisibleHeight = timeLinesVisibleHeight;
            await _timeLinesViewModel.LoadAsync(@"TestData\VisibleTimeLines.csv", 1000d);

            // Act
            List<TimeLineViewModel> timeLines = _timeLinesViewModel.TimeLineCollectionView.Cast<TimeLineViewModel>().ToList();

            // Assert
            Assert.AreEqual(expectedTimeLineCount, timeLines.Count);
        }

        [DataRow(0d, 0)]
        [DataRow(100d, 0)]
        [DataRow(200d, 1)]
        [DataRow(300d, 2)]
        [DataRow(400d, 3)]
        [DataRow(500d, 3)]
        [TestMethod]
        public async Task TimeLineItems_ForVisibleWidth_YieldsOnlyVisibleTimeLineItems(double timeLinesVisibleWidth, int expectedTimeLineItemCount)
        {
            // Arrange
            _timeLinesViewModel.TimeLinesVisibleHeight = 30d;
            await _timeLinesViewModel.LoadAsync(@"TestData\VisibleTimeLineItems.csv", timeLinesVisibleWidth);
            _timeLinesViewModel.Scale = ScaleIndex.OneMinute;

            // Act
            List<TimeLineViewModel> timeLines = _timeLinesViewModel.TimeLineCollectionView.Cast<TimeLineViewModel>().ToList();
            List<TimeLineItemViewModel> timeLineItems = timeLines.First().TimeLineItemCollectionView.Cast<TimeLineItemViewModel>().ToList();

            // Assert
            Assert.AreEqual(expectedTimeLineItemCount, timeLineItems.Count);
        }

        [DataRow("Utilization_0%.csv", "0 %")]
        [DataRow("Utilization_50%.csv", "50 %")]
        [DataRow("Utilization_100%.csv", "100 %")]
        [DataRow("Utilization_200%.csv", "200 %")]
        [TestMethod]
        public async Task TimeUtilization_AfterLoading_YieldsExpectedValue(string fileName, string expectedUtilization)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync($@"TestData\{fileName}", 1000d);

            // Act
            string actualUtilization = $"{_timeLinesViewModel.TimeUtilization * 100} %";

            // Assert
            Assert.AreEqual(expectedUtilization, actualUtilization);
        }

        [TestMethod]
        public async Task TotalEndTime_ForLocalTime_YieldsExpectedTime()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _settingsViewModel.TimeZone = _settingsViewModel.TimeZones.First(tz => tz.DisplayName.Contains("Saratov")).Id;
            _settingsViewModel.IsUniversalTime = false;

            // Assert
            Assert.AreEqual("2020-01-01 12:05:00.000 +04:00", _timeLinesViewModel.TotalEndTimeText);
        }

        [DataRow(0d)]
        [DataRow(100d)]
        [DataRow(200d)]
        [TestMethod]
        public async Task TotalEndTime_AfterScrolling_YieldsExpectedTime(double horizontalScrollOffset)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _timeLinesViewModel.Scale = ScaleIndex.TenSeconds;

            // Act
            _timeLinesViewModel.HorizontalScrollOffset = horizontalScrollOffset;

            // Assert
            Assert.AreEqual("2020-01-01 08:05:00.000 Z", _timeLinesViewModel.TotalEndTimeText);
        }

        [DataRow(ScaleIndex.HalfMinute)]
        [DataRow(ScaleIndex.TenSeconds)]
        [DataRow(ScaleIndex.FiveSeconds)]
        [TestMethod]
        public async Task TotalEndTime_AfterZooming_YieldsExpectedTime(ScaleIndex scale)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _timeLinesViewModel.Scale = scale;

            // Assert
            Assert.AreEqual("2020-01-01 08:05:00.000 Z", _timeLinesViewModel.TotalEndTimeText);
        }

        [TestMethod]
        public async Task TotalStartTime_ForLocalTime_YieldsExpectedTime()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _settingsViewModel.TimeZone = _settingsViewModel.TimeZones.First(tz => tz.DisplayName.Contains("Saratov")).Id;
            _settingsViewModel.IsUniversalTime = false;

            // Assert
            Assert.AreEqual("2020-01-01 12:00:00.000 +04:00", _timeLinesViewModel.TotalStartTimeText);
        }

        [DataRow(0d)]
        [DataRow(100d)]
        [DataRow(200d)]
        [TestMethod]
        public async Task TotalStartTime_AfterScrolling_YieldsExpectedTime(double horizontalScrollOffset)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            _timeLinesViewModel.Scale = ScaleIndex.TenSeconds;

            // Act
            _timeLinesViewModel.HorizontalScrollOffset = horizontalScrollOffset;

            // Assert
            Assert.AreEqual("2020-01-01 08:00:00.000 Z", _timeLinesViewModel.TotalStartTimeText);
        }

        [DataRow(ScaleIndex.HalfMinute)]
        [DataRow(ScaleIndex.TenSeconds)]
        [DataRow(ScaleIndex.FiveSeconds)]
        [TestMethod]
        public async Task TotalStartTime_AfterZooming_YieldsExpectedTime(ScaleIndex scale)
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            _timeLinesViewModel.Scale = scale;

            // Assert
            Assert.AreEqual("2020-01-01 08:00:00.000 Z", _timeLinesViewModel.TotalStartTimeText);
        }

        [TestMethod]
        public async Task DeleteTimeLineItem_DeleteOneTimeLineItem_OneIsDeleted()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            TimeLineItemViewModel timeLineItem = _timeLinesViewModel.TimeLineItems.First();

            // Act
            await _timeLinesViewModel.DeleteTimeLineItem(timeLineItem);

            // Assert
            Assert.AreEqual(4, _timeLinesViewModel.TimeLineItems.Count);
            Assert.AreEqual(5, _timeLinesViewModel.TimeLines.Count);
        }

        [TestMethod]
        public async Task DeleteTimeLine_DeleteOneTimeLine_OneIsDeleted()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            TimeLineViewModel timeLine = _timeLinesViewModel.TimeLines.First();

            // Act
            await _timeLinesViewModel.DeleteTimeLine(timeLine);

            // Assert
            Assert.AreEqual(4, _timeLinesViewModel.TimeLines.Count);
            Assert.AreEqual(4, _timeLinesViewModel.TimeLineItems.Count);
        }

        [TestMethod]
        public async Task MoveTimeLine_MoveToLastTimeLine_Moves()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            await _timeLinesViewModel.MoveTimeLine(
                _timeLinesViewModel.TimeLines.First(),
                _timeLinesViewModel.TimeLines.Last()
                );

            // Assert
            Assert.AreEqual("2", _timeLinesViewModel.TimeLines.First().Name);
            Assert.AreEqual("1", _timeLinesViewModel.TimeLines.Last().Name);
        }

        [TestMethod]
        public async Task MoveTimeLine_MoveToSelf_DoesNotMove()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            // Act
            await _timeLinesViewModel.MoveTimeLine(
                _timeLinesViewModel.TimeLines.First(),
                _timeLinesViewModel.TimeLines.First()
                );

            // Assert
            Assert.AreEqual("1", _timeLinesViewModel.TimeLines.First().Name);
            Assert.AreEqual("5", _timeLinesViewModel.TimeLines.Last().Name);
            Assert.IsFalse(_timeLinesViewModel.IsModified);
        }

        [TestMethod]
        public async Task MoveTimeLineItem_MoveToLastTimeLine_Moves()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);
            TimeLineItemViewModel timelineItem = _timeLinesViewModel.TimeLines.First().TimeLineItems.First();

            // Act
            await _timeLinesViewModel.MoveTimeLineItem(
                timelineItem,
                _timeLinesViewModel.TimeLines.Last()
                );

            // Assert
            Assert.AreEqual(0, _timeLinesViewModel.TimeLines.First().TimeLineItems.Count);
            Assert.AreEqual(2, _timeLinesViewModel.TimeLines.Last().TimeLineItems.Count);
            Assert.AreEqual(5, _timeLinesViewModel.TimeLines.Count);
            Assert.AreEqual("5", timelineItem.TimeLineViewModel.Name);
            Assert.IsTrue(_timeLinesViewModel.IsModified);
        }

        [TestMethod]
        public async Task MoveTimeLineItem_MoveToOwnTimeLine_DoesNotMove()
        {
            // Arrange
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000d);

            TimeLineViewModel timeLine = _timeLinesViewModel.TimeLines.First();
            TimeLineItemViewModel timelineItem = timeLine.TimeLineItems.First();

            // Act
            await _timeLinesViewModel.MoveTimeLineItem(timelineItem, timeLine);

            // Assert
            Assert.AreEqual(1, timeLine.TimeLineItems.Count);
            Assert.AreEqual("1", timelineItem.TimeLineViewModel.Name);
            Assert.IsFalse(_timeLinesViewModel.IsModified);
        }
        [TestMethod]
        public async Task HorizontalScroll_UpdatesOnlyVisibleRows_AndRefreshesRowsOnEntry()
        {
            _timeLinesViewModel.TimeLinesVisibleHeight = 30;
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000);
            _timeLinesViewModel.Scale = ScaleIndex.Second;
            var first = _timeLinesViewModel.TimeLines[0].TimeLineItems[0];
            var hidden = _timeLinesViewModel.TimeLines[1].TimeLineItems[0];
            List<string> firstChanges = [];
            List<string> hiddenChanges = [];
            first.PropertyChanged += (_, e) => firstChanges.Add(e.PropertyName);
            hidden.PropertyChanged += (_, e) => hiddenChanges.Add(e.PropertyName);
            double originalLeft = hidden.Left;

            _timeLinesViewModel.HorizontalScrollOffset = 50;

            CollectionAssert.AreEquivalent(new[] { "Left", "Width", "IsTimeSpanVisible", "IsTimeEventVisible" }, firstChanges);
            Assert.AreEqual(0, hiddenChanges.Count);
            Assert.AreEqual(originalLeft - 50, hidden.Left);

            _timeLinesViewModel.VerticalScrollOffset = 30;
            CollectionAssert.AreEquivalent(new[] { "Left", "Width", "IsTimeSpanVisible", "IsTimeEventVisible" }, hiddenChanges);
            hiddenChanges.Clear();
            firstChanges.Clear();
            _timeLinesViewModel.HorizontalScrollOffset = 75;
            Assert.AreEqual(0, firstChanges.Count);
            Assert.AreEqual(4, hiddenChanges.Count);
            _timeLinesViewModel.VerticalScrollOffset = 0;
            Assert.AreEqual(4, firstChanges.Count);
        }

        [TestMethod]
        public async Task DeferredScroll_RefreshesAfterResizeZoomAndReorder()
        {
            _timeLinesViewModel.TimeLinesVisibleHeight = 30;
            await _timeLinesViewModel.LoadAsync(@"TestData\Minutes.csv", 1000);
            _timeLinesViewModel.Scale = ScaleIndex.Second;
            var row = _timeLinesViewModel.TimeLines[4];
            var item = row.TimeLineItems[0];
            List<string> changes = [];
            item.PropertyChanged += (_, e) => changes.Add(e.PropertyName);
            _timeLinesViewModel.HorizontalScrollOffset = 50;
            Assert.AreEqual(0, changes.Count);

            _timeLinesViewModel.Scale = ScaleIndex.HalfMinute;
            changes.Clear();
            item.Name = "Edited while off screen";
            changes.Clear();
            _timeLinesViewModel.TimeLinesVisibleWidth = 800;
            _timeLinesViewModel.TimeLinesVisibleHeight = 150;
            Assert.IsTrue(changes.Contains("Width"));
            Assert.AreEqual("Edited while off screen", item.Name);

            _timeLinesViewModel.TimeLinesVisibleHeight = 30;
            _timeLinesViewModel.HorizontalScrollOffset = 100;
            changes.Clear();
            await _timeLinesViewModel.MoveTimeLine(row, _timeLinesViewModel.TimeLines[0]);
            Assert.AreEqual(0, row.RowIndex);
            Assert.IsTrue(row.IsInVerticalViewport);
            Assert.IsTrue(changes.Contains("Left"));
            CollectionAssert.AreEqual(Enumerable.Range(0, 5).ToArray(),
                _timeLinesViewModel.TimeLines.Select(x => x.RowIndex).ToArray());

            await _timeLinesViewModel.DeleteTimeLine(row);
            CollectionAssert.AreEqual(Enumerable.Range(0, 4).ToArray(),
                _timeLinesViewModel.TimeLines.Select(x => x.RowIndex).ToArray());
            await _timeLinesViewModel.UndoAsync();
            CollectionAssert.AreEqual(Enumerable.Range(0, 5).ToArray(),
                _timeLinesViewModel.TimeLines.Select(x => x.RowIndex).ToArray());
        }

        private static SettingsRepositoryStub CreateSettingsRepository()
        {
            return new SettingsRepositoryStub(new SettingsModel());
        }

    }
}
