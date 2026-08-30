// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace TimeLiner.Models
{
    /// <summary>
    /// Provides a JSON-based implementation of <see cref="ISettingsRepository"/>
    /// for persisting application settings in the user's application data directory.
    /// </summary>
    public sealed class JsonSettingsRepository : ISettingsRepository
    {
        /// <summary>
        /// Full path to the settings file.
        /// </summary>
        private readonly string _filePath;

        /// <summary>
        /// Current supported version of the settings file format.
        /// </summary>
        private const int CurrentVersion = 2;

        /// <summary>
        /// Ordered chain of migrations. Add new steps here to extend the chain.
        /// </summary>
        private static readonly IReadOnlyList<ISettingsMigration> _migrations =
        [
            new SettingsMigrationV1ToV2()
            // new SettingsMigrationV2ToV3(), // add future steps here
        ];

        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
        
        /// <summary>
        /// Default constructor that uses the standard application data directory.
        /// </summary>
        public JsonSettingsRepository() : this(GetDefaultFilePath())
        {
        }

        /// <summary>
        /// Constructor which accepts the path of the settings file.
        /// </summary>
        public JsonSettingsRepository(string filePath)
        {
            _filePath = filePath;
        }

        /// <summary>
        /// Loads the persisted application settings from the JSON settings file.
        /// </summary>
        public SettingsModel Load()
        {
            if (!File.Exists(_filePath))
            {
                return new SettingsModel();
            }

            JsonObject root;
            using (FileStream stream = File.OpenRead(_filePath))
            {
                try
                {
                    root = JsonNode.Parse(stream) as JsonObject;
                }
                catch (JsonException)
                {
                    return new SettingsModel();
                }
            }

            if (root == null)
            {
                return new SettingsModel();
            }

            int version = root["Version"]?.GetValue<int>() ?? 1;

            if (version != CurrentVersion)
            {
                root = Migrate(root, version);
            }

            SettingsFile settingsFile = root.Deserialize<SettingsFile>(_serializerOptions);

            return settingsFile?.Settings ?? new SettingsModel();
        }

        
        /// <summary>
        /// Chains all applicable migrations in order from <paramref name="fromVersion"/>
        /// up to <see cref="CurrentVersion"/>.
        /// </summary>
        private static JsonObject Migrate(JsonObject root, int fromVersion)
        {
            IEnumerable<ISettingsMigration> steps = _migrations
                .Where(m => m.FromVersion >= fromVersion)
                .OrderBy(m => m.FromVersion);

            foreach (ISettingsMigration step in steps)
            {
                root = step.Migrate(root);
            }

            root["Version"] = CurrentVersion;
            return root;
        }

        /// <summary>
        /// Persists the specified application settings to the JSON settings file.
        /// </summary>
        public void Save(SettingsModel settings)
        {
            SettingsFile settingsFile = new()
            {
                Version = CurrentVersion,
                Settings = settings
            };

            using FileStream stream = File.Create(_filePath);
            JsonSerializer.Serialize(stream, settingsFile, _serializerOptions);
        }

        /// <summary>
        /// Gets the default file path for the settings file 
        /// (e.g., "%APPDATA%\TimeLiner\settings.json" on Windows).
        /// </summary>
        private static string GetDefaultFilePath()
        {
            string dirPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TimeLiner"
                );

            Directory.CreateDirectory(dirPath);

            return Path.Combine(dirPath, "settings.json");
        }
    }
}
