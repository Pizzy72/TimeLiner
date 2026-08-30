// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using TimeLiner.Common;
using TimeLiner.Models;

namespace TimeLinerTest
{
    /// <summary>
    /// Summary description for TestModel
    /// </summary>
    [TestClass]
    public class TestTimeLinesModel
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task LoadAsync_ValidCsvHeader_Succeeds()
        {
            // Arrange
            string filePath = @"TestData\ValidCsvHeader.csv";

            // Act
            TimeLinesModel model = await TimeLinesModel.LoadAsync(filePath);

            // Assert
            Assert.AreEqual(1, model.TimeLines.Count);
        }

        [TestMethod]
        public async Task LoadAsync_InvalidCsvHeader_ThrowsException()
        {
            // Arrange
            string filePath = @"TestData\InvalidCsvHeader.csv";

            // Act => Assert
            await Assert.ThrowsExactlyAsync<TimeLinerException>(async () =>
            {
                await TimeLinesModel.LoadAsync(filePath);
            });
        }

        [TestMethod]
        public async Task LoadAsync_RedundantCsvHeader_ThrowsException()
        {
            // Arrange
            string filePath = @"TestData\RedundantCsvHeader.csv";

            // Act => Assert
            await Assert.ThrowsExactlyAsync<TimeLinerException>(async () =>
            {
                await TimeLinesModel.LoadAsync(filePath);
            });
        }

        [TestMethod]
        public async Task LoadAsync_InvalidColumnCount_ThrowsException()
        {
            // Arrange
            string filePath = @"TestData\InvalidColumnCount.csv";

            // Act => Assert
            await Assert.ThrowsExactlyAsync<TimeLinerException>(async () =>
            {
                await TimeLinesModel.LoadAsync(filePath);
            });
        }

        [TestMethod]
        public async Task LoadAsync_InvalidColorName_ThrowsException()
        {
            // Arrange
            string filePath = @"TestData\InvalidColorName.csv";

            // Act => Assert
            await Assert.ThrowsExactlyAsync<TimeLinerException>(async () =>
            {
                await TimeLinesModel.LoadAsync(filePath);
            });
        }

        [TestMethod]
        public async Task LoadAsync_InvalidStartTime_ThrowsException()
        {
            // Arrange
            string filePath = @"TestData\InvalidStartTime.csv";

            // Act => Assert
            await Assert.ThrowsExactlyAsync<TimeLinerException>(async () =>
            {
                await TimeLinesModel.LoadAsync(filePath);
            });
        }

        [TestMethod]
        public async Task LoadAsync_InvalidEndTime_ThrowsException()
        {
            // Arrange
            string filePath = @"TestData\InvalidEndTime.csv";

            // Act => Assert
            await Assert.ThrowsExactlyAsync<TimeLinerException>(async () =>
            {
                await TimeLinesModel.LoadAsync(filePath);
            });
        }

        [TestMethod]
        public async Task LoadAsync_InvalidTimeOrder_ThrowsException()
        {
            // Arrange
            string filePath = @"TestData\InvalidTimeOrder.csv";

            // Act => Assert
            await Assert.ThrowsExactlyAsync<TimeLinerException>(async () =>
            {
                await TimeLinesModel.LoadAsync(filePath);
            });
        }

        [TestMethod]
        public async Task LoadAsync_InvalidLine_ThrowsException()
        {
            // Arrange
            string filePath = @"TestData\InvalidLine.csv";

            // Act => Assert
            await Assert.ThrowsExactlyAsync<TimeLinerException>(async () =>
            {
                await TimeLinesModel.LoadAsync(filePath);
            });
        }

        [TestMethod]
        public async Task LoadAsync_CommentedLine_Succeeds()
        {
            // Arrange
            string filePath = @"TestData\CommentedLine.csv";

            // Act
            TimeLinesModel model = await TimeLinesModel.LoadAsync(filePath);

            // Assert
            Assert.AreEqual(1, model.TimeLineItemCount);
        }

        [TestMethod]
        public async Task LoadAsync_QuotedColumns_Succeeds()
        {
            // Arrange
            string filePath = @"TestData\QuotedColumns.csv";

            // Act
            TimeLinesModel model = await TimeLinesModel.LoadAsync(filePath);
            TimeLineModel timeLine = model.TimeLines.First();
            TimeLineItemModel timeLineItem = timeLine.TimeLineItems.First();

            // Assert
            Assert.AreEqual(1, model.TimeLineItemCount);
            Assert.AreEqual("TimeLineName", timeLine.Name);
            Assert.AreEqual("TimeLineItemName", timeLineItem.Name);
            Assert.AreEqual("Tomato", timeLineItem.Color);
            Assert.AreEqual("2020-01-01T08:00:00.0000000Z", timeLineItem.StartTime.ToString("O"));
            Assert.AreEqual("2020-01-01T08:01:00.0000000Z", timeLineItem.EndTime.ToString("O"));
        }

        [TestMethod]
        public async Task LoadAsync_UnquotedColumns_Succeeds()
        {
            // Arrange
            string filePath = @"TestData\UnquotedColumns.csv";

            // Act
            TimeLinesModel model = await TimeLinesModel.LoadAsync(filePath);
            TimeLineModel timeLine = model.TimeLines.First();
            TimeLineItemModel timeLineItem = timeLine.TimeLineItems.First();

            // Assert
            Assert.AreEqual(1, model.TimeLineItemCount);
            Assert.AreEqual("TimeLineName", timeLine.Name);
            Assert.AreEqual("TimeLineItemName", timeLineItem.Name);
            Assert.AreEqual("Tomato", timeLineItem.Color);
            Assert.AreEqual("2020-01-01T08:00:00.0000000Z", timeLineItem.StartTime.ToString("O"));
            Assert.AreEqual("2020-01-01T08:01:00.0000000Z", timeLineItem.EndTime.ToString("O"));
        }

        [TestMethod]
        public async Task LoadAsync_EmptyEndTime_Succeeds()
        {
            // Arrange
            string filePath = @"TestData\EmptyEndTime.csv";

            // Act
            TimeLinesModel model = await TimeLinesModel.LoadAsync(filePath);
            TimeLineModel timeLine = model.TimeLines.First();
            TimeLineItemModel timeLineItem = timeLine.TimeLineItems.First();

            // Assert
            Assert.AreEqual("2020-01-01T08:00:00.0000000Z", timeLineItem.StartTime.ToString("O"));
            Assert.AreEqual("2020-01-01T08:00:00.0000000Z", timeLineItem.EndTime.ToString("O"));
        }

        [TestMethod]
        public async Task LoadAsync_EmptyTimeLine_Succeeds()
        {
            // Arrange
            string filePath = @"TestData\EmptyTimeLine.csv";

            // Act
            TimeLinesModel model = await TimeLinesModel.LoadAsync(filePath);

            // Assert
            Assert.AreEqual(1, model.TimeLines.Count);
            Assert.AreEqual(0, model.TimeLineItemCount);
        }
    }
}
