using System;
using System.Buffers.Binary;
using System.Diagnostics;

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
        AmqpConstructor _itemType;

        public AmqpReader(Span<byte> buffer)
        {
            _buffer = buffer;
            Type = default;
            _current = 0;
            _currentLength = 0;
            _next = 0;
            _itemType = default;
        }

        public AmqpType Type { get; private set; }

        public ReadOnlySpan<byte> Bytes => _buffer.Slice(_current, _currentLength);

        public bool TryGetInt32Array(Span<int> buffer, out int written)
        {
            if (Type != AmqpType.ArrayStart || _itemType != AmqpConstructor.Int) throw new InvalidOperationException();
            written = _currentLength / sizeof(int);
            if (buffer.Length < written) return false;

            var data = _buffer.Slice(_current, _currentLength);
            for (int i = 0; i < written; i++)
            {
                buffer[i] = BinaryPrimitives.ReadInt32BigEndian(data);
                data = data.Slice(sizeof(int));
            }
            return true;
        }

        public int[] GetInt32Array()
        {
            if (Type != AmqpType.ArrayStart || _itemType != AmqpConstructor.Int) throw new InvalidOperationException();
            var data = _buffer.Slice(_current, _currentLength);
            var result = new int[_currentLength / sizeof(int)];
            for(int i=0; i<result.Length; i++)
            {
                result[i] = BinaryPrimitives.ReadInt32BigEndian(data);
                data = data.Slice(sizeof(int));
            }
            return result;
        }

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
                    {
                        if (_buffer.Length < 4) return false;
                        var itemSize = _buffer[1];
                        _currentLength = _buffer[2];
                        _itemType = (AmqpConstructor)_buffer[3];
                        _current = 4;
                        _next = _current + (_currentLength * itemSize);
                        Type = AmqpType.ArrayStart;
                        return true;
                    }
                case AmqpConstructor.Array32:
                    {
                        if (_buffer.Length < 10) return false;
                        var dataSize = BinaryPrimitives.ReadInt32BigEndian(_buffer.Slice(1)) - 5;
                        _currentLength = BinaryPrimitives.ReadInt32BigEndian(_buffer.Slice(5)) * sizeof(int);
                        _itemType = (AmqpConstructor)_buffer[9];
                        _current = 10;
                        _next = _current + dataSize;
                        Type = AmqpType.ArrayStart;
                        return true;
                    }
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
