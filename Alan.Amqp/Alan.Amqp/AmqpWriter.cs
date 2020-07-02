using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace Alan.Amqp
{
    public static class AmqpWriter
    {
        public static bool TryWrite(Span<byte> destination, string text, out int bytesWritten)
        {
            const int headerShort = 2; // size + count (i1) 
            const int headerLong = 5; // size + count (i4)

            var byteCount = Encoding.UTF8.GetByteCount(text);
            if (byteCount < 256)
            {
                bytesWritten = byteCount + headerShort;
                if (bytesWritten > destination.Length) return false;
                destination[0] = (byte)AmqpConstructor.String8;
                destination[1] = (byte)byteCount;
                destination = destination.Slice(headerShort);
            }
            else
            {
                bytesWritten = byteCount + headerLong;
                if (bytesWritten > destination.Length) return false;
                destination[0] = (byte)AmqpConstructor.String32;
                BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(1), byteCount);
                destination = destination.Slice(headerLong);
            }

            Encoding.UTF8.GetBytes(text, destination);
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
                destination[0] = (byte)AmqpConstructor.Binary8;
                destination[1] = (byte)byteCount;
                destination = destination.Slice(headerShort);
            }
            else
            {
                bytesWritten = byteCount + headerLong;
                if (bytesWritten > destination.Length) return false;
                destination[0] = (byte)AmqpConstructor.Binary32;
                BinaryPrimitives.WriteInt32BigEndian(destination.Slice(1), byteCount);
                destination = destination.Slice(headerLong);
            }

            bytes.CopyTo(destination);
            return true;
        }

        public static bool TryWriteArray(Span<byte> destination, ReadOnlySpan<int> values, out int bytesWritten)
        {
            const int headerShort = 4; // code (i1) + size (i1) + count (i1) + constructor (i1)
            const int headerLong = 10; // code (i1) + size (i4) + count (i4) + constructor (i1)

            var valueCount = values.Length;
            if (valueCount < 256)
            {
                bytesWritten = headerShort + valueCount * sizeof(int);
                if (bytesWritten > destination.Length) return false;

                destination[0] = (byte)AmqpConstructor.Array8;
                destination[1] = (byte)sizeof(int);
                destination[2] = (byte)valueCount;
                destination[3] = (byte)AmqpConstructor.Int;
                destination = destination.Slice(headerShort);
            }
            else
            {
                bytesWritten = headerLong + valueCount * sizeof(int);
                if (bytesWritten > destination.Length) return false;

                destination[0] = (byte)AmqpConstructor.Array32;
                // TODO: size = count (i4) + ctor (i1) + data_size (according to Microsoft.AMQP, but it does make much sense for Array8)
                BinaryPrimitives.WriteInt32BigEndian(destination.Slice(sizeof(AmqpConstructor)), sizeof(int) + sizeof(byte) + valueCount * sizeof(int));
                BinaryPrimitives.WriteInt32BigEndian(destination.Slice(sizeof(AmqpConstructor)+sizeof(int)), valueCount);
                destination[sizeof(AmqpConstructor) + sizeof(int) + sizeof(int)] = (byte)AmqpConstructor.Int;
                destination = destination.Slice(headerLong);
            }

            for(int i=0; i<values.Length; i++)
            {
                BinaryPrimitives.WriteInt32BigEndian(destination.Slice(i*sizeof(int)), values[i]);
            }
            return true;
        }

        public static OperationStatus Write(Span<byte> destination, ReadOnlySpan<byte> bytes, out int bytesWritten)
        {
            bytesWritten = 0;
            if (destination.Length < 1) return OperationStatus.DestinationTooSmall;

            var byteCount = bytes.Length;
            if (byteCount < 256) destination[0] = (byte)AmqpConstructor.Binary8;
            else destination[0] = (byte)AmqpConstructor.Binary32;
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
    }
}
