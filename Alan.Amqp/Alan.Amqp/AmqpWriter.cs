using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace System.Buffers.Amqp
{
    public static class AmqpWriter
    {
        public static bool TryWriteString(Span<byte> destination, string text, out int bytesWritten)
        {
            const int headerShort = 2; // size + count (i1) 
            const int headerLong = 5; // size + count (i4)

            var byteCount = Encoding.UTF8.GetByteCount(text);
            if (byteCount < 256)
            {
                bytesWritten = byteCount + headerShort;
                if (bytesWritten > destination.Length) return false;
                destination[0] = (byte)AmqpType.String8;
                destination[1] = (byte)byteCount;
                destination = destination.Slice(headerShort);
            }
            else
            {
                bytesWritten = byteCount + headerLong;
                if (bytesWritten > destination.Length) return false;
                destination[0] = (byte)AmqpType.String32;
                BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(1), byteCount);
                destination = destination.Slice(headerLong);
            }

            Encoding.UTF8.GetBytes(text, destination);
            return true;
        }

        public static bool TryWriteString(Span<byte> destination, ReadOnlySpan<byte> utf8Text, out int bytesWritten)
        {
            const int headerShort = 2; // size + count (i1) 
            const int headerLong = 5; // size + count (i4)

            var byteCount = utf8Text.Length;
            if (byteCount < 256)
            {
                bytesWritten = byteCount + headerShort;
                if (bytesWritten > destination.Length) return false;
                destination[0] = (byte)AmqpType.String8;
                destination[1] = (byte)byteCount;
                destination = destination.Slice(headerShort);
            }
            else
            {
                bytesWritten = byteCount + headerLong;
                if (bytesWritten > destination.Length) return false;
                destination[0] = (byte)AmqpType.String32;
                BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(1), byteCount);
                destination = destination.Slice(headerLong);
            }

            utf8Text.CopyTo(destination);
            return true;
        }

        public static bool TryWriteBinary(Span<byte> destination, ReadOnlySpan<byte> bytes, out int bytesWritten)
        {
            const int headerShort = 2; // code (i1) + count (i1) 
            const int headerLong = 5; // code (i1) + count (i4)

            var byteCount = bytes.Length;
            if (byteCount < 256)
            {
                bytesWritten = byteCount + headerShort;
                if (bytesWritten > destination.Length) return false;
                destination[0] = (byte)AmqpType.Binary8;
                destination[1] = (byte)byteCount;
                destination = destination.Slice(headerShort);
            }
            else
            {
                bytesWritten = byteCount + headerLong;
                if (bytesWritten > destination.Length) return false;
                destination[0] = (byte)AmqpType.Binary32;
                BinaryPrimitives.WriteInt32BigEndian(destination.Slice(1), byteCount);
                destination = destination.Slice(headerLong);
            }

            bytes.CopyTo(destination);
            return true;
        }

        public static OperationStatus WriteBinary(Span<byte> destination, ReadOnlySpan<byte> bytes, out int bytesWritten)
        {
            bytesWritten = 0;
            if (destination.Length < 1) return OperationStatus.DestinationTooSmall;

            var byteCount = bytes.Length;
            if (byteCount < 256) destination[0] = (byte)AmqpType.Binary8;
            else destination[0] = (byte)AmqpType.Binary32;
            bytesWritten = 1;

            if (destination.Length < 2) return OperationStatus.DestinationTooSmall;
            if (byteCount < 256)
            {
                destination[1] = (byte)byteCount;
                bytesWritten = 2;
            }
            else
            {
                if (destination.Length < 5) return OperationStatus.DestinationTooSmall;
                else
                {
                    BinaryPrimitives.WriteInt32BigEndian(destination.Slice(1), byteCount);
                    bytesWritten = 5;
                }
            }

            destination = destination.Slice(bytesWritten);
            var toWrite = Math.Min(destination.Length, bytes.Length);
            bytes.Slice(0, toWrite).CopyTo(destination);
            bytesWritten += toWrite;

            if(toWrite <= destination.Length) return OperationStatus.Done;
            return OperationStatus.DestinationTooSmall;
        }

        public static bool TryWriteConstructor(Span<byte> destination, uint domainId, uint descriptorId, out int bytesWritten)
        {
            ulong numericValue = domainId << 32 | descriptorId;
            return TryWriteULong(destination, numericValue, out bytesWritten);
        }

        public static bool TryWriteArrayInt(Span<byte> destination, ReadOnlySpan<int> values, out int bytesWritten)
        {
            const int headerShort = 4; // code (i1) + size (i1) + count (i1) + constructor (i1)
            const int headerLong = 10; // code (i1) + size (i4) + count (i4) + constructor (i1)

            var valueCount = values.Length;
            if (valueCount < 256)
            {
                bytesWritten = headerShort + valueCount * sizeof(int);
                if (bytesWritten > destination.Length) return false;

                destination[0] = (byte)AmqpType.Array8;
                destination[1] = (byte)sizeof(int); // TODO: I think there is a bug here. I think this value is total size, not item size
                destination[2] = (byte)valueCount;
                destination[3] = (byte)AmqpType.Int;
                destination = destination.Slice(headerShort);
            }
            else
            {
                bytesWritten = headerLong + valueCount * sizeof(int);
                if (bytesWritten > destination.Length) return false;

                destination[0] = (byte)AmqpType.Array32;
                // TODO: size = count (i4) + ctor (i1) + data_size (according to Microsoft.AMQP, but it does make much sense for Array8)
                BinaryPrimitives.WriteInt32BigEndian(destination.Slice(sizeof(AmqpType)), sizeof(int) + sizeof(byte) + valueCount * sizeof(int));
                BinaryPrimitives.WriteInt32BigEndian(destination.Slice(sizeof(AmqpType) + sizeof(int)), valueCount);
                destination[sizeof(AmqpType) + sizeof(int) + sizeof(int)] = (byte)AmqpType.Int;
                destination = destination.Slice(headerLong);
            }

            for (int i = 0; i < values.Length; i++)
            {
                BinaryPrimitives.WriteInt32BigEndian(destination.Slice(i * sizeof(int)), values[i]);
            }
            return true;
        }

        public static bool TryWriteSymbol(Span<byte> destination, ReadOnlySpan<byte> ascii, out int bytesWritten)
        {
            const int headerShort = 2; // size + count (i1) 
            const int headerLong = 5; // size + count (i4)

            var byteCount = ascii.Length;
            if (byteCount < 256)
            {
                bytesWritten = byteCount + headerShort;
                if (bytesWritten > destination.Length) return false;
                destination[0] = (byte)AmqpType.Symbol8;
                destination[1] = (byte)byteCount;
                destination = destination.Slice(headerShort);
            }
            else
            {
                bytesWritten = byteCount + headerLong;
                if (bytesWritten > destination.Length) return false;
                destination[0] = (byte)AmqpType.Symbol32;
                BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(1), byteCount);
                destination = destination.Slice(headerLong);
            }

            ascii.CopyTo(destination);
            return true;
        }

        public static bool TryWriteULong(Span<byte> destination, ulong value, out int bytesWritten)
        {
            bytesWritten = 0;
            if (value == 0)
            {
                if (destination.Length == 0) return false;
                destination[0] = 0x44;
                bytesWritten = 1;
            }
            if (value < 256)
            {
                if (destination.Length < 2) return false;
                destination[0] = 0x53;
                destination[1] = (byte)value;
                bytesWritten = 2;
            }
            else
            {
                if (destination.Length < 5) return false;
                destination[0] = 0x80;
                BinaryPrimitives.WriteUInt64BigEndian(destination.Slice(1), value);
                bytesWritten = 9;
            }

            return true;
        }

        public static bool TryWriteUInt(Span<byte> destination, uint value, out int bytesWritten)
        {
            bytesWritten = 0;
            if (value == 0)
            {
                if (destination.Length == 0) return false;
                destination[0] = 0x43;
                bytesWritten = 1;
            }
            if (value < 256)
            {
                if (destination.Length < 2) return false;
                destination[0] = 0x52;
                destination[1] = (byte)value;
                bytesWritten = 2;
            }
            else
            {
                if (destination.Length < 5) return false;
                destination[0] = 0x70;
                BinaryPrimitives.WriteUInt32BigEndian(destination.Slice(1), value);
                bytesWritten = 5;
            }

            return true;
        }

        public static bool TryWriteUShort(Span<byte> destination, ushort value, out int bytesWritten)
        {
            if (destination.Length < 3)
            {
                bytesWritten = 0;
                return false;
            }
            
            destination[0] = 0x60;
            BinaryPrimitives.WriteUInt16BigEndian(destination.Slice(1), value);
            bytesWritten = 3;
            return true;
        }

        public static bool TryWriteUByte(Span<byte> destination, byte value, out int bytesWritten)
        {
            if (destination.Length < 2)
            {
                bytesWritten = 0;
                return false;
            }

            destination[0] = 0x50;
            destination[1] = value;
            bytesWritten = 2;
            return true;
        }

        public static bool TryWriteBoolean(Span<byte> destination, bool value, out int bytesWritten)
        {
            if (destination.Length == 0)
            {
                bytesWritten = 0;
                return false;
            }

            destination[0] = value? (byte)0x41 : (byte)0x42;
            bytesWritten = 1;
            return true;
        }

        public static bool TryWriteNull(Span<byte> destination, ushort value, out int bytesWritten)
        {
            if (destination.Length == 0)
            {
                bytesWritten = 0;
                return false;
            }

            destination[0] = 0x40;
            bytesWritten = 1;
            return true;
        }

        internal static bool TryWriteEmptyArraySymbol(Span<byte> destination, out int bytesWritten)
        {
            if (destination.Length < 4)
            {
                bytesWritten = 0;
                return false;
            }

            destination[0] = (byte)AmqpType.Array8;
            destination[1] = (byte)0;
            destination[2] = (byte)0;
            destination[3] = (byte)AmqpType.Symbol8;

            bytesWritten = 4;
            return true;
        }

        internal static bool TryWriteEmptyMap(Span<byte> destination, out int bytesWritten)
        {
            if (destination.Length < 2)
            {
                bytesWritten = 0;
                return false;
            }

            destination[0] = 0xc1;
            destination[1] = 0;

            bytesWritten = 2;
            return true;
        }
    }
}
