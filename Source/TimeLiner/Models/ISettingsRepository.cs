// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

namespace TimeLiner.Models
{
    /// <summary>
    /// Defines a repository for loading and saving application settings.
    /// </summary>
    public interface ISettingsRepository
    {
        /// <summary>
        /// Loads the persisted application settings.
        /// </summary>
        /// <returns>
        /// The loaded <see cref="SettingsModel"/> instance.
        /// If no settings are available, a new instance with default values is returned.
        /// </returns>
        SettingsModel Load();

        /// <summary>
        /// Persists the specified application settings.
        /// </summary>
        /// <param name="settings">
        /// The <see cref="SettingsModel"/> instance to persist.
        /// </param>
        void Save(SettingsModel settings);
    }
}
