// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TimeLiner.Common;
using TimeLinerTest.TestDoubles;

namespace TimeLinerTest
{
    [TestClass]
    public class TestTimeFormat
    {
        public TestContext TestContext { get; set; }

        [DataRow(8, 0, 0, 0, 0, "1 wk 1 d")]
        [DataRow(-8, 0, 0, 0, 0, "1 wk 1 d")]
        [DataRow(1, 2, 3, 4, 5, "1 d 2 h")]
        [DataRow(-1, -2, -3, -4, -5, "1 d 2 h")]
        [DataRow(1, 0, 0, 0, 0, "1 d")]
        [DataRow(0, 1, 2, 3, 4, "1 h 2 min")]
        [DataRow(0, -1, -2, -3, -4, "1 h 2 min")]
        [DataRow(0, 1, 0, 0, 0, "1 h")]
        [DataRow(0, 0, 1, 2, 3, "1 min 2 s")]
        [DataRow(0, 0, -1, -2, -3, "1 min 2 s")]
        [DataRow(0, 0, 1, 0, 0, "1 min")]
        [DataRow(0, 0, 0, 10, 0, "10 s")]
        [DataRow(0, 0, 0, 1, 2, "1.2 s")]
        [DataRow(0, 0, 0, -1, -2, "1.2 s")]
        [DataRow(0, 0, 0, 0, 1001, "1.1 s")]
        [DataRow(0, 0, 0, 0, -1001, "1.1 s")]
        [DataRow(0, 0, 0, 1, 0, "1 s")]
        [DataRow(0, 0, 0, 0, 999, "999 ms")]
        [DataRow(0, 0, 0, 0, -999, "999 ms")]
        [DataRow(0, 0, 0, 0, 1, "1 ms")]
        [DataRow(0, 0, 0, 0, -1, "1 ms")]
        [DataRow(0, 0, 0, 0, 0, "0 s")]
        [TestMethod]
        public void GetDurationString_Duration_ReturnsDurationString(
            int days,
            int hours,
            int minutes,
            int seconds,
            int milliseconds,
            string exptectedString
            )
        {
            // Arrange
            TimeSpan duration = new(days, hours, minutes, seconds, milliseconds);

            // Act
            string actualString = TimeFormat.GetDurationString(duration);

            // Assert
            Assert.AreEqual(exptectedString, actualString);
        }

        [TestMethod]
        public void GetTimeString_FormatAsUtcTime_ReturnsUtcTimeString()
        {
            // Arrange
            DateTime utcTime = DateTime.Parse("2021-02-14T20:30:00Z").ToUniversalTime();

            ITimeZoneInfoProvider provider = new TimeZoneInfoProviderStub("W. Europe Standard Time", true);

            // Act
            string timeString = TimeFormat.GetTimeString(utcTime, provider);

            // Assert
            Assert.AreEqual("2021-02-14 20:30:00.000 Z", timeString);
        }

        [TestMethod]
        [DataRow("2025-10-26T00:59:59Z", "W. Europe Standard Time", "2025-10-26 02:59:59.000 +02:00")]
        [DataRow("2025-10-26T00:59:59Z", "Middle East Standard Time", "2025-10-26 02:59:59.000 +02:00")]
        [DataRow("2025-10-26T01:00:00Z", "W. Europe Standard Time", "2025-10-26 02:00:00.000 +01:00")]
        [DataRow("2025-10-26T01:00:00Z", "Middle East Standard Time", "2025-10-26 03:00:00.000 +02:00")]
        public void GetTimeString_FormatAsLocalTime_ReturnsLocalTimeString(
            string utcTimeString,
            string timeZoneId,
            string expectedTimeString
            )
        {
            // Arrange
            DateTime utcTime = DateTime.Parse(utcTimeString).ToUniversalTime();

            ITimeZoneInfoProvider provider = new TimeZoneInfoProviderStub(timeZoneId, false);

            // Act
            string timeString = TimeFormat.GetTimeString(utcTime, provider);

            // Assert
            Assert.AreEqual(expectedTimeString, timeString);
        }
    }
}
