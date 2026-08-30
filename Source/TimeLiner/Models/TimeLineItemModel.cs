// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;

namespace TimeLiner.Models
{
    /// <summary>
    /// The model of a timeline item.
    /// </summary>
    internal class TimeLineItemModel
    {
        /// <summary>
        /// The name of the timeline item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The start time of the timeline item.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The end time of the timeline item.
        /// </summary>
        /// <remarks>
        /// If start and end time are different, the timeline item is shown as a time span (bar).
        /// If start and end time are identical, it is shown as a time event (diamond).
        /// </remarks>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// The color of the timeline item.
        /// </summary>
        public string Color { get; set; }
    }
}
