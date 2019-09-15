using System;
using System.Buffers.Binary;

namespace Alan.Amqp
{
    public enum AmqpType : byte
    {
        Unknown,
        Utf8,
    }

    public ref struct AmqpReader
    {
        Span<byte> _buffer;
        int _current;
        int _currentLength;
        int _next;

        public AmqpReader(Span<byte> buffer)
        {
            _buffer = buffer;
            Type = AmqpType.Unknown;
            _current = 0;
            _currentLength = 0;
            _next = 0;
        }

        public AmqpType Type { get; private set; }

        public ReadOnlySpan<byte> Bytes => _buffer.Slice(_current, _currentLength);

        public bool Read()
        {
            _buffer = _buffer.Slice(_next);
            if (_buffer.Length == 0) return false;

            var constructor = (AmqpConstructor)_buffer[0];
            switch (constructor)
            {
                case AmqpConstructor.String8:
                    _current = 2;
                    _currentLength = _buffer[1];
                    _next += _currentLength + 2;
                    return true;
                case AmqpConstructor.String32:
                    _current = 5;
                    _currentLength = BinaryPrimitives.ReadInt32BigEndian(_buffer.Slice(1));
                    _next += _currentLength + 5;
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
