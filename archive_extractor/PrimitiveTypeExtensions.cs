﻿using System.Buffers.Binary;

namespace archive_extractor
{
    internal static class PrimitiveTypeExtensions
    {
        public static ushort ReadU16(this byte[] bs, int position) => BinaryPrimitives.ReadUInt16LittleEndian(bs.AsSpan()[position..]);
        public static uint ReadU32(this byte[] bs, long position) => BinaryPrimitives.ReadUInt32LittleEndian(bs.AsSpan()[(int)position..]);
    }
}
