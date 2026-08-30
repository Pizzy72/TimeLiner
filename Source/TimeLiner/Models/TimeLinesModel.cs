// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using TimeLiner.Common;
using TimeLiner.Properties;

namespace TimeLiner.Models
{
    /// <summary>
    /// The timelines model.
    /// </summary>
    internal class TimeLinesModel
    {
        /// <summary>
        /// CSV file constants.
        /// </summary>
        private static class CsvFile
        {
            /// <summary>
            /// The CSV file columns.
            /// </summary>
            public enum Column
            {
                TimeLine,
                Item,
                Start,
                End,
                Color
            }

            /// <summary>
            /// The CSV column names.
            /// </summary>
            public static string[] ColumnNames = Enum.GetNames(typeof(Column)).ToArray();

            /// <summary>
            /// The number of CSV columns.
            /// </summary>
            public static readonly int MaxColumns = ColumnNames.Length;

            /// <summary>
            /// The CSV separator character.
            /// </summary>
            public const char Separator = ',';

            /// <summary>
            /// Comment character.
            /// </summary>
            public const char Comment = '#';
        }

        /// <see cref="TimeLines"/>
        private readonly List<TimeLineModel> _timeLines = [];

        /// <summary>
        /// The file path of the loaded model.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// The timeline models.
        /// </summary>
        public IReadOnlyList<TimeLineModel> TimeLines => _timeLines.AsReadOnly();

        /// <summary>
        /// The total number of timeline items.
        /// </summary>
        public int TimeLineItemCount => TimeLines.Sum(tl => tl.TimeLineItems.Count);

        /// <summary>
        /// Creates a deep copy of the model.
        /// </summary>
        public async Task<TimeLinesModel> CloneAsync()
        {
            using MemoryStream stream = new();

            await SaveAsync(stream);

            stream.Position = 0;

            TimeLinesModel clone = await LoadAsync(stream);

            clone.FilePath = FilePath;

            return clone;
        }

        /// <summary>
        /// Save model as file.
        /// </summary>
        public async Task SaveAsync(string filePath)
        {
            using FileStream stream = File.OpenWrite(filePath);

            await SaveAsync(stream);

            FilePath = filePath;
        }

        /// <summary>
        /// Save model as stream.
        /// </summary>
        public async Task SaveAsync(Stream stream)
        {
            string sep = CsvFile.Separator.ToString();

            using StreamWriter writer = new(
                stream,
                System.Text.Encoding.UTF8,
                bufferSize: 1024,
                leaveOpen: true);

            string csvHeader = string.Join(sep, Enum.GetNames(typeof(CsvFile.Column)));

            await writer.WriteLineAsync(csvHeader);

            foreach (TimeLineModel timeLine in TimeLines)
            {
                string timeLineName = timeLine.Name.Replace(sep, string.Empty).Trim();

                if (timeLine.TimeLineItems.Count > 0)
                {
                    foreach (TimeLineItemModel item in timeLine.TimeLineItems)
                    {
                        string itemName = item.Name.Replace(sep, string.Empty).Trim();

                        await writer.WriteAsync($"{timeLineName}{sep}{itemName}{sep}{item.StartTime.ToUniversalTime():o}{sep}");

                        if (item.StartTime != item.EndTime)
                        {
                            await writer.WriteAsync($"{item.EndTime.ToUniversalTime():o}");
                        }

                        await writer.WriteLineAsync($"{sep}{item.Color}");
                    }
                }
                else
                {
                    await writer.WriteLineAsync($"{timeLineName}{sep}{sep}{sep}{sep}");
                }
            }

            await writer.FlushAsync();
        }

        /// <summary>
        /// Load model from file.
        /// </summary>
        public static async Task<TimeLinesModel> LoadAsync(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);

            TimeLinesModel model = await LoadAsync(stream);
            model.FilePath = filePath;

            return model;
        }

        /// <summary>
        /// The CSV parser state.
        /// </summary>
        private enum ParserState
        {
            CsvHeader,
            CsvData
        }

        /// <summary>
        /// Load model from stream.
        /// </summary>
        public static async Task<TimeLinesModel> LoadAsync(Stream stream)
        {
            TimeLinesModel model = new();
            Dictionary<string, TimeLineModel> timeLines = new();

            ParserState state = ParserState.CsvHeader;

            using StreamReader reader = new(
                stream,
                System.Text.Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024,
                leaveOpen: true
                );

            string line;
            long lineNumber = 0;

            try
            {
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;

                    string row = line.Trim().Replace("\"", string.Empty);

                    if (string.IsNullOrEmpty(row))
                        continue;

                    if (row[0] == CsvFile.Comment)
                        continue;

                    string[] columns = row
                        .Split(CsvFile.Separator)
                        .Select(c => c.Trim())
                        .ToArray();

                    if (columns.Length != CsvFile.MaxColumns)
                        throw new TimeLinerException(Resources.ErrorInvalidNumberOfColumns);

                    if (state == ParserState.CsvHeader)
                    {
                        state = ParserState.CsvData;

                        if (columns.SequenceEqual(CsvFile.ColumnNames, StringComparer.OrdinalIgnoreCase))
                            continue;
                    }

                    string timeLineName = columns[(int)CsvFile.Column.TimeLine];
                    string itemName = columns[(int)CsvFile.Column.Item];
                    string startTimeStr = columns[(int)CsvFile.Column.Start];
                    string endTimeStr = columns[(int)CsvFile.Column.End];
                    string colorName = columns[(int)CsvFile.Column.Color];

                    if (!timeLines.TryGetValue(timeLineName, out TimeLineModel timeLine))
                    {
                        timeLine = new TimeLineModel
                        {
                            Name = timeLineName
                        };

                        timeLines[timeLineName] = timeLine;
                    }

                    if (string.IsNullOrEmpty(itemName) &&
                        string.IsNullOrEmpty(startTimeStr) &&
                        string.IsNullOrEmpty(endTimeStr) &&
                        string.IsNullOrEmpty(colorName))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(itemName))
                        throw new TimeLinerException(Resources.ErrorMissingItemName);

                    if (string.IsNullOrEmpty(startTimeStr))
                        throw new TimeLinerException(Resources.ErrorMissingStartTime);

                    DateTime startTime = DateTime.Parse(startTimeStr).ToUniversalTime();

                    DateTime endTime;

                    if (string.IsNullOrEmpty(endTimeStr))
                    {
                        endTime = startTime;
                    }
                    else
                    {
                        endTime = DateTime.Parse(endTimeStr).ToUniversalTime();
                    }

                    if (startTime > endTime)
                        throw new TimeLinerException(Resources.ErrorStartTimeBehindEndTime);

                    if (!string.IsNullOrEmpty(colorName) && !IsValidColor(colorName))
                        throw new TimeLinerException(string.Format(Resources.ErrorUnknownColor, colorName));

                    TimeLineItemModel timeLineItem = new()
                    {
                        Name = itemName,
                        StartTime = startTime,
                        EndTime = endTime,
                        Color = colorName
                    };

                    timeLine.AddTimeLineItem(timeLineItem);
                }
            }
            catch (Exception ex)
            {
                throw new TimeLinerException(string.Format(Resources.ErrorInLine, lineNumber, ex.Message));
            }

            foreach (TimeLineModel timeLine in timeLines.Values)
            {
                model.AddTimeLine(timeLine);
            }

            return model;
        }

        /// <summary>
        /// Insert timeline at given index.
        /// </summary>
        public void InsertTimeLine(int index, TimeLineModel timeLine)
        {
            _timeLines.Insert(index, timeLine);
        }

        /// <summary>
        /// Add the given timeline from the list.
        /// </summary>
        public void AddTimeLine(TimeLineModel timeLine)
        {
            _timeLines.Add(timeLine);
        }

        /// <summary>
        /// Remove the given timeline from the list.
        /// </summary>
        public bool RemoveTimeLine(TimeLineModel timeLine)
        {
            return _timeLines.Remove(timeLine);
        }

        /// <summary>
        /// Move the timeline at the specified index to a new location.
        /// </summary>
        public void MoveTimeLine(int oldIndex, int newIndex)
        {
            _timeLines.Move(oldIndex, newIndex);
        }

        /// <summary>
        /// Delete the given timeline item.
        /// </summary>
        public void DeleteTimeLineItem(TimeLineItemModel timeLineItem)
        {
            foreach (TimeLineModel timeLine in TimeLines)
            {
                if (timeLine.RemoveTimeLineItem(timeLineItem))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Check if the given string is a valid color.
        /// </summary>
        private static bool IsValidColor(string value)
        {
            try
            {
                ColorConverter.ConvertFromString(value);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
