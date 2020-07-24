using System;

namespace System.Buffers.Amqp
{
    public enum AmqpType : byte
    {
        Descriptor = 0x00,

        // primitive format codes
        Null = 0x40,

        True = 0x41,
        False = 0x42,
        Boolean = 0x56,

        UByte = 0x50,

        UShort = 0x60,

        UInt = 0x70,
        UInt0 = 0x43,
        UIntS = 0x52,

        ULong = 0x80,
        ULong0 = 0x43,
        ULongS = 0x53,

        Byte = 0x51,
        
        Short = 0x61,
        
        Int = 0x71,
        Int8 = 0x54,

        Long = 0x81,
        Long8 = 0x55,
        
        Float = 0x72,
        Double = 0x82,

        Decimal32 = 0x74,
        Decimal64 = 0x84,
        Decimal128 = 0x94, 

        Char = 0x73, // UTF-32BE
        Timestamp = 0x83,
        Uuid = 0x98,

        Binary8 = 0xA0,
        Binary32 = 0xB0,

        String8 = 0xA1, // UTF8
        String32 = 0xB1,

        Symbol8 = 0xA3,
        Symbol32 = 0xB3,

        List0 = 0x45,
        List8 = 0xC0,
        List32 = 0xD0,

        Map8 = 0xC1,
        Map32 = 0xD1,

        Array8 = 0xE0,
        Array32 = 0xF0,
    }

    public static class AmqpParser
    {
        public static bool TryReadConstructor(Span<byte> span, out AmqpType constructor)
        {
            if (span.Length == 0)
            {
                constructor = default;
                return false;
            }
            constructor = (AmqpType)span[0];
            if (constructor == AmqpType.String8) return true;
            if (constructor == AmqpType.Descriptor) return true;
            return false;
        }
    }
}
