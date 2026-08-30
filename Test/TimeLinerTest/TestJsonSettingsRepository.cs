// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using TimeLiner.Models;
using TimeLiner.Themes;

namespace TimeLinerTest
{
    /// <summary>
    /// Tests for the JsonSettingsRepository class.
    /// </summary>
    [TestClass]
    public class TestJsonSettingsRepository
    {
        private string _tempFilePath;
        private JsonSettingsRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
            _repository = new JsonSettingsRepository(_tempFilePath);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (File.Exists(_tempFilePath))
            {
                try
                {
                    File.Delete(_tempFilePath);
                }
                catch
                {
                    // Ignore because cleanup must not break a test
                }
            }
        }

        [TestMethod]
        public void SaveAndLoad_Roundtrip_UsesTempFile()
        {
            // Arrange
            SettingsModel settingsOriginal = new()
            {
                IsUniversalTime = false,
                IsCompactTimeGrid = true,
                IsTimeLocatorLocked = true,
                IsNameVisible = false,
                TimeZone = "UTC+2",
                Theme = AppTheme.Dark
            };

            // Act
            _repository.Save(settingsOriginal);
            SettingsModel settingsLoaded = _repository.Load();

            // Assert
            Assert.AreEqual(settingsOriginal.IsUniversalTime, settingsLoaded.IsUniversalTime);
            Assert.AreEqual(settingsOriginal.IsCompactTimeGrid, settingsLoaded.IsCompactTimeGrid);
            Assert.AreEqual(settingsOriginal.IsTimeLocatorLocked, settingsLoaded.IsTimeLocatorLocked);
            Assert.AreEqual(settingsOriginal.IsNameVisible, settingsLoaded.IsNameVisible);
            Assert.AreEqual(settingsOriginal.TimeZone, settingsLoaded.TimeZone);
            Assert.AreEqual(settingsOriginal.Theme, settingsLoaded.Theme);
        }

        [TestMethod]
        public void Load_WhenFileDoesNotExist_ReturnsDefaults()
        {
            // Arrange

            // Act
            SettingsModel settings = _repository.Load();

            // Assert
            Assert.IsNotNull(settings);
        }

        [TestMethod]
        public void Load_LegacyVersion2File_PreservesSettings()
        {
            // Arrange – this is the format previously written by Newtonsoft.Json.
            string json = """
                {
                  "Version": 2,
                  "Settings": {
                    "IsUniversalTime": false,
                    "IsCompactTimeGrid": true,
                    "IsCompactTimeLines": true,
                    "IsTimeLocatorLocked": true,
                    "IsNameVisible": false,
                    "TimeZone": "W. Europe Standard Time",
                    "Theme": "Dark",
                    "MainWindowSettings": {
                      "Top": 12.5,
                      "Left": 25.5,
                      "Width": 1400.0,
                      "Height": 900.0,
                      "WindowState": "Maximized"
                    }
                  }
                }
                """;

            File.WriteAllText(_tempFilePath, json);

            // Act
            SettingsModel settings = _repository.Load();

            // Assert
            Assert.IsFalse(settings.IsUniversalTime);
            Assert.IsTrue(settings.IsCompactTimeGrid);
            Assert.IsTrue(settings.IsCompactTimeLines);
            Assert.IsTrue(settings.IsTimeLocatorLocked);
            Assert.IsFalse(settings.IsNameVisible);
            Assert.AreEqual("W. Europe Standard Time", settings.TimeZone);
            Assert.AreEqual(AppTheme.Dark, settings.Theme);
            Assert.AreEqual(12.5, settings.MainWindowSettings.Top);
            Assert.AreEqual(25.5, settings.MainWindowSettings.Left);
            Assert.AreEqual(1400.0, settings.MainWindowSettings.Width);
            Assert.AreEqual(900.0, settings.MainWindowSettings.Height);
            Assert.AreEqual(WindowState.Maximized, settings.MainWindowSettings.WindowState);
        }

        
        [TestMethod]
        public void Load_Version1File_MigratesIsCompactGridToIsCompactTimeGrid()
        {
            // Arrange – build a v1 JsonObject in memory, no file system required
            JsonObject v1Root = new()
            {
                ["Version"] = 1,
                ["Settings"] = new JsonObject
                {
                    ["IsUniversalTime"] = false,
                    ["IsCompactGrid"] = true,   // old name, non-default value
                    ["IsTimeLocatorLocked"] = true,
                    ["IsNameVisible"] = false,
                    ["TimeZone"] = "W. Europe Standard Time",
                    ["Theme"] = "Dark"
                }
            };

            SettingsMigrationV1ToV2 migration = new();

            // Act
            JsonObject migratedRoot = migration.Migrate(v1Root);
            JsonObject migratedSettings = (JsonObject)migratedRoot["Settings"];

            // Assert – old key must be gone
            Assert.IsNull(migratedSettings!["IsCompactGrid"],
                "IsCompactGrid must be removed after migration.");

            // Assert – new key carries the stored value
            Assert.AreEqual(true, migratedSettings["IsCompactTimeGrid"]?.GetValue<bool>(),
                "IsCompactGrid=true in v1 must be migrated to IsCompactTimeGrid=true.");

            // Assert – IsCompactTimeLines is absent; SettingsModel default (false) will apply on deserialisation
            Assert.IsNull(migratedSettings["IsCompactTimeLines"],
                "IsCompactTimeLines must not be injected by the migration; SettingsModel provides the default.");

            // Assert – all other properties are preserved unchanged
            Assert.AreEqual(false, migratedSettings["IsUniversalTime"]?.GetValue<bool>());
            Assert.AreEqual(true,  migratedSettings["IsTimeLocatorLocked"]?.GetValue<bool>());
            Assert.AreEqual(false, migratedSettings["IsNameVisible"]?.GetValue<bool>());
            Assert.AreEqual("W. Europe Standard Time", migratedSettings["TimeZone"]?.GetValue<string>());
            Assert.AreEqual("Dark", migratedSettings["Theme"]?.GetValue<string>());
        }
    }
}
