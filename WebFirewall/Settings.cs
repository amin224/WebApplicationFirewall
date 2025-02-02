﻿using System;
using System.Runtime.CompilerServices;

namespace WebFirewall
{
    internal static class Settings
    {
        internal static readonly int MaxRequestsPerMin = 10;
        internal static readonly TimeSpan ResetTime = TimeSpan.FromMinutes(10);
        internal static readonly int DefaultBlockDurationSec = 60;
        internal static readonly int DefaultExtendedBlockDurationSec = 300;
        internal static readonly bool isDDoSSecurityActive = true;
        internal static readonly bool isFloodSecurityActive = true;
        internal static readonly bool isSqlInjectionSecurityActive = true;
        internal static readonly bool isXssSecurityActive = true;
        internal static readonly bool isCsrfSecurityActive = true;
        internal static readonly bool isFileInclusionSecurityActive = true;
        internal const string CustomHeaderName = "Private-Custom-Header";
        internal const string CustomHeaderValue = "FS86FGB70398NBSG5262C4MNMV01GS5X";
        internal const string CsrfToken = "A6YD*!fV?NN89B0";
    }
}
