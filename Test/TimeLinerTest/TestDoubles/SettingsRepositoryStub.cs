// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using TimeLiner.Models;

namespace TimeLinerTest.TestDoubles
{
    internal sealed class SettingsRepositoryStub : ISettingsRepository
    {
        private SettingsModel _settings;

        public SettingsRepositoryStub(SettingsModel settings)
        {
            _settings = settings;
        }

        public SettingsModel Load()
        {
            return _settings;
        }

        public void Save(SettingsModel settings)
        {
            _settings = settings;
        }
    }
}
