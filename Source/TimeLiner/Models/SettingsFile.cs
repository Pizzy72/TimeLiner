// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

namespace TimeLiner.Models
{
    /// <summary>
    /// Represents the root structure of the persisted settings file.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the file format version and the actual
    /// application settings. It serves as a container to support
    /// versioning and future migrations.
    /// The version is always set explicitly by <see cref="JsonSettingsRepository"/>.
    /// </remarks>
    public sealed class SettingsFile
    {
        /// <summary>
        /// Gets or sets the version of the settings file format.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the application settings.
        /// </summary>
        public SettingsModel Settings { get; set; } = new();
    }
}
