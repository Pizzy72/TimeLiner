// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;

namespace TimeLiner.Common
{
    internal class TimeLinerException : Exception
    {
        public TimeLinerException()
        {
        }

        public TimeLinerException(string message) : base(message)
        {
        }

        public TimeLinerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
