﻿using System;
using System.Runtime.InteropServices;
using LibHac.Common;

namespace LibHac.Fs
{
    [StructLayout(LayoutKind.Explicit, Size = MaxLength + 1)]
    public struct FsPath
    {
        internal const int MaxLength = 0x300;

        [FieldOffset(0)] private byte _str;

        public Span<byte> Str => SpanHelpers.CreateSpan(ref _str, MaxLength + 1);
    }
}