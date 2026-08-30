// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System.Collections.Generic;

namespace TimeLiner.Models
{
    /// <summary>
    /// The timeline model.
    /// </summary>
    internal class TimeLineModel
    {
        /// <see cref="TimeLineItems"/>
        private List<TimeLineItemModel> _timeLineItems = [];

        /// <summary>
        /// The timeline name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The timeline items of this timeline.
        /// </summary>
        public IReadOnlyList<TimeLineItemModel> TimeLineItems => _timeLineItems.AsReadOnly();

        /// <summary>
        /// Add the given timeline item.
        /// </summary>
        public void AddTimeLineItem(TimeLineItemModel timeLineItem)
        {
            _timeLineItems.Add(timeLineItem);
        }

        /// <summary>
        /// Remove the given timeline item.
        /// </summary>
        public bool RemoveTimeLineItem(TimeLineItemModel timeLineItem)
        {
            return _timeLineItems.Remove(timeLineItem);
        }
    }
}
