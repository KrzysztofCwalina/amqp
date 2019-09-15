using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Alan.Amqp
{
    public static class AmqpWriter
    {
        public static bool TryWrite(Span<byte> destination, string text, out int bytesWritten)
        {
            const int headerShort = 2; // size + count (i1) 
            const int headerLong = 5; // size + count (i4)

            if (destination.Length < headerShort)
            {
                bytesWritten = 0;
                return false;
            }

            var byteCount = Encoding.UTF8.GetByteCount(text);
            if (byteCount < 256)
            {
                destination[0] = (byte)AmqpConstructor.String8;
                bytesWritten = byteCount + headerShort;
                if (bytesWritten > destination.Length) return false;
                destination[1] = (byte)byteCount;
                destination = destination.Slice(headerShort);
            }
            else
            {
                destination[0] = (byte)AmqpConstructor.String32;
                bytesWritten = byteCount + headerLong;
                if (bytesWritten > destination.Length) return false;
                BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(1), byteCount);
                destination = destination.Slice(headerLong);
            }

            Encoding.UTF8.GetBytes(text, destination);
            return true;
        }

        public static bool TryWrite(Span<byte> destination, ReadOnlySpan<byte> bytes, out int bytesWritten)
        {
            const int headerShort = 2; // size + count (i1) 
            const int headerLong = 5; // size + count (i4)

            if (destination.Length < headerShort)
            {
                bytesWritten = 0;
                return false;
            }

            var byteCount = bytes.Length;
            if (byteCount < 256)
            {
                destination[0] = (byte)AmqpConstructor.Binary8;
                bytesWritten = byteCount + headerShort;
                if (bytesWritten > destination.Length) return false;
                destination[1] = (byte)byteCount;
                destination = destination.Slice(headerShort);
            }
            else
            {
                destination[0] = (byte)AmqpConstructor.Binary32;
                bytesWritten = byteCount + headerLong;
                if (bytesWritten > destination.Length) return false;
                // TODO: this it really LE?
                BinaryPrimitives.WriteInt32BigEndian(destination.Slice(1), byteCount);
                destination = destination.Slice(headerLong);
            }

            bytes.CopyTo(destination);
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
