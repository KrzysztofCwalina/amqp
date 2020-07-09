using System;
using System.Buffers.Binary;

namespace Alan.Amqp
{
    public ref struct AmqpValue
    {

    }
    public enum AmqpToken : byte
    {
        // strings
        Descriptor = 1,
        Binary = 2,
        String,

        // collections
        Array, // monomorphic
        ArrayEnd,
        List, // polymorphic
        ListEnd,
        Map,

        // primitives
        Null,
        Boolean,
        Int32,

        // control
        EndOfData = byte.MaxValue
    }

    public ref struct AmqpReader
    {
        Span<byte> _buffer;
        int _current;
        int _currentLength;
        int _next;

        // Array Specific
        AmqpType _itemType;

        public AmqpReader(Span<byte> buffer)
        {
            _buffer = buffer;
            CurrentType = default;
            _current = 0;
            _currentLength = 0;
            _next = 0;
            _itemType = default;
        }

        public AmqpToken CurrentType { get; private set; }
        public int CurrentLength => _currentLength;
        public int CurrentIndex => _current;

        public AmqpToken MoveNext()
        {
            _buffer = _buffer.Slice(_next);
            _next = 0;
            if (_buffer.Length == 0) return AmqpToken.EndOfData;

            var constructor = (AmqpType)_buffer[0];
            switch (constructor)
            {
                case AmqpType.Descriptor:
                    _next++;
                    CurrentType = AmqpToken.Descriptor;
                    break;

                case AmqpType.Binary8:
                    if (_buffer.Length == 1) return AmqpToken.EndOfData;
                    _currentLength = _buffer[1];
                    _current = 2;
                    _next += _currentLength + _current;
                    CurrentType = AmqpToken.Binary;
                    break;

                case AmqpType.Binary32:
                    _currentLength = BinaryPrimitives.ReadInt32BigEndian(_buffer.Slice(1));
                    _current = 5;
                    _next += _currentLength + _current;
                    CurrentType = AmqpToken.Binary;
                    break;

                case AmqpType.String8:
                    _current = 2;
                    _currentLength = _buffer[1];
                    _next += _currentLength + _current;
                    CurrentType = AmqpToken.String;
                    break;
                case AmqpType.String32:
                    _current = 5;
                    _currentLength = BinaryPrimitives.ReadInt32BigEndian(_buffer.Slice(1));
                    _next += _currentLength + _current;
                    CurrentType = AmqpToken.String;
                    break;

                case AmqpType.Array8:
                    {
                        if (_buffer.Length < 4) return AmqpToken.EndOfData;
                        var itemSize = _buffer[1];
                        _currentLength = _buffer[2];
                        _itemType = (AmqpType)_buffer[3];
                        _current = 4;
                        _next = _current + (_currentLength * itemSize);
                        CurrentType = AmqpToken.Array;
                        break;
                    }
                case AmqpType.Array32:
                    {
                        if (_buffer.Length < 10) return AmqpToken.EndOfData;
                        var dataSize = BinaryPrimitives.ReadInt32BigEndian(_buffer.Slice(1)) - 5;
                        _currentLength = BinaryPrimitives.ReadInt32BigEndian(_buffer.Slice(5)) * sizeof(int);
                        _itemType = (AmqpType)_buffer[9];
                        _current = 10;
                        _next = _current + dataSize;
                        CurrentType = AmqpToken.Array;
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }
            return CurrentType;
        }

        public ReadOnlySpan<byte> Bytes => _buffer.Slice(_current, _currentLength);

        public bool TryGetInt32Array(Span<int> buffer, out int written)
        {
            if (CurrentType != AmqpToken.Array || _itemType != AmqpType.Int) throw new InvalidOperationException();
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
            if (CurrentType != AmqpToken.Array || _itemType != AmqpType.Int) throw new InvalidOperationException();
            var data = _buffer.Slice(_current, _currentLength);
            var result = new int[_currentLength / sizeof(int)];
            for(int i=0; i<result.Length; i++)
            {
                result[i] = BinaryPrimitives.ReadInt32BigEndian(data);
                data = data.Slice(sizeof(int));
            }
            return result;
        }

        public int GetInt32()
        {
            if (CurrentType != AmqpToken.Array || _itemType != AmqpType.Int) throw new InvalidOperationException();
            if (_currentLength < sizeof(int)) throw new InvalidOperationException();
            int value = BinaryPrimitives.ReadInt32BigEndian(_buffer.Slice(_current));
            return value;
        }

        public int ReadInt32()
        {
            if (CurrentType != AmqpToken.Array || _itemType != AmqpType.Int) throw new InvalidOperationException();
            if (_currentLength < sizeof(int)) throw new InvalidOperationException();
            int value = BinaryPrimitives.ReadInt32BigEndian(_buffer.Slice(_current));
            _current += sizeof(int);
            _currentLength -= sizeof(int);
            if (_currentLength == 0) CurrentType = AmqpToken.ArrayEnd;
            return value;
        }
    }
}
