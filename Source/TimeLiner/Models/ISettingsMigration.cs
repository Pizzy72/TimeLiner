// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System.Text.Json.Nodes;

namespace TimeLiner.Models
{
    /// <summary>
    /// Represents a single version-step migration for the settings file.
    /// </summary>
    internal interface ISettingsMigration
    {
        /// <summary>
        /// Gets the version this migration upgrades from.
        /// </summary>
        int FromVersion { get; }

        /// <summary>
        /// Applies the migration to the raw settings JSON.
        /// </summary>
        JsonObject Migrate(JsonObject root);
    }
}
