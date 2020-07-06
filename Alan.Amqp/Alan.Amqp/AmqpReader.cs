using System;
using System.Buffers.Binary;

namespace Alan.Amqp
{
    public enum AmqpType : byte
    {
        Utf8        = 1,
        Binary,

        ArrayStart,
        ArrayEnd,

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
            Type = default;
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
                case AmqpConstructor.Binary8:
                    if (_buffer.Length == 1) return false;
                    _currentLength = _buffer[1];
                    _current = 2;
                    _next += _currentLength + _current;
                    Type = AmqpType.Binary;
                    return true;
                case AmqpConstructor.Binary32:
                    _currentLength = BinaryPrimitives.ReadInt32BigEndian(_buffer.Slice(1));
                    _current = 5;
                    _next += _currentLength + _current;
                    Type = AmqpType.Binary;
                    return true;

                case AmqpConstructor.Array8:
                    throw new NotImplementedException();
                case AmqpConstructor.Array32:
                    throw new NotImplementedException();

                case AmqpConstructor.String8:
                    _current = 2;
                    _currentLength = _buffer[1];
                    _next += _currentLength + _current;
                    Type = AmqpType.Utf8;
                    return true;
                case AmqpConstructor.String32:
                    _current = 5;
                    _currentLength = BinaryPrimitives.ReadInt32BigEndian(_buffer.Slice(1));
                    _next += _currentLength + _current;
                    Type = AmqpType.Utf8;
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
