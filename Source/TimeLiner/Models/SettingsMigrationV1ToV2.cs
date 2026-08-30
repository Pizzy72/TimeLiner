// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System.Text.Json.Nodes;

namespace TimeLiner.Models
{
    /// <summary>
    /// Migrates the TimeLiner settings from version 1 to version 2.
    /// </summary>
    internal sealed class SettingsMigrationV1ToV2 : ISettingsMigration
    {
        /// <inheritdoc/>
        public int FromVersion => 1;

        /// <inheritdoc/>
        public JsonObject Migrate(JsonObject root)
        {
            if (root["Settings"] is not JsonObject settings)
            {
                return root;
            }

            // Rename IsCompactGrid into IsCompactTimeGrid, preserving the stored value.
            if (settings["IsCompactGrid"] is JsonNode isCompactGrid)
            {
                settings.Remove("IsCompactGrid");
                settings["IsCompactTimeGrid"] = isCompactGrid;
            }

            return root;
        }
    }
}
