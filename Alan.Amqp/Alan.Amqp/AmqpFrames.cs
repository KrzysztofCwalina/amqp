using System;
using System.Buffers.Binary;

namespace System.Buffers.Amqp
{
    /*
    <type name="open" class="composite" source="list" provides="frame">
        <descriptor name="amqp:open:list" code="0x00000000:0x00000010"/>
        <field name="container-id" type="string" mandatory="true"/>
        <field name="hostname" type="string"/>
        <field name="max-frame-size" type="uint" default="4294967295"/>
        <field name="channel-max" type="ushort" default="65535"/>
        <field name="idle-time-out" type="milliseconds"/>
        <field name="outgoing-locales" type="ietf-language-tag" multiple="true"/>, e.g. array["en-US"]
        <field name="incoming-locales" type="ietf-language-tag" multiple="true"/>
        <field name="offered-capabilities" type="symbol" multiple="true"/>
        <field name="desired-capabilities" type="symbol" multiple="true"/>
        <field name="properties" type="fields"/>
    </type>
    */
    public ref struct OpenFrame
    {
        Span<byte> _buffer;
        int _written;

        public OpenFrame(Span<byte> buffer)
        {
            _buffer = buffer;
            _written = 0;
            _written += 8; // reserve header space
        }

        public bool TryWrite(
            ReadOnlySpan<byte> containerId,
            ReadOnlySpan<byte> hostname,
            uint maxFrameSize = 4294967295,
            ushort channelMax = 65535,
            uint idleTimeOut = 60000
        )
        {
            if (containerId.Length == 0) throw new ArgumentOutOfRangeException(nameof(containerId));

            if (_buffer.Length <= _written) return false;
            _buffer[_written] = (byte)AmqpType.Descriptor;
            _written++;

            int justWritten;

            if (!AmqpWriter.TryWriteConstructor(_buffer.Slice(_written), 0x00000000, 0x00000010, out justWritten)) return false;
            _written += justWritten;

            if (_buffer.Length <= _written) return false;
            _buffer[_written] = (byte)AmqpType.List8;
            _written++;

            // size
            if (_buffer.Length <= _written) return false;
            int size = _written; // reserve
            _written++;

            // count
            if (_buffer.Length <= _written) return false;
            _buffer[_written] = (byte)2;
            _written++;

            if (!AmqpWriter.TryWriteString(_buffer.Slice(_written), containerId, out justWritten)) return false;
            _written += justWritten;

            if (!AmqpWriter.TryWriteString(_buffer.Slice(_written), hostname, out justWritten)) return false;
            _written += justWritten;

            //if (!AmqpWriter.TryWriteUInt(_buffer.Slice(_written), maxFrameSize, out justWritten)) return false;
            //_written += justWritten;

            //if (!AmqpWriter.TryWriteUShort(_buffer.Slice(_written), channelMax, out justWritten)) return false;
            //_written += justWritten;

            //if (!AmqpWriter.TryWriteUInt(_buffer.Slice(_written), idleTimeOut, out justWritten)) return false;
            //_written += justWritten;

            //// outgoing-locales
            //if (!AmqpWriter.TryWriteEmptyArraySymbol(_buffer.Slice(_written), out justWritten)) return false;
            //_written += justWritten;

            //// incoming-locales
            //if (!AmqpWriter.TryWriteEmptyArraySymbol(_buffer.Slice(_written), out justWritten)) return false;
            //_written += justWritten;

            //// offered-capabilities
            //if (!AmqpWriter.TryWriteEmptyArraySymbol(_buffer.Slice(_written), out justWritten)) return false;
            //_written += justWritten;

            //// desired-capabilities
            //if (!AmqpWriter.TryWriteEmptyArraySymbol(_buffer.Slice(_written), out justWritten)) return false;
            //_written += justWritten;

            //// properties
            //if (!AmqpWriter.TryWriteEmptyMap(_buffer.Slice(_written), out justWritten)) return false;
            //_written += justWritten;

            _buffer[size] = (byte)(_written - size); // write total size

            return true;
        }

        public void Send(AmqpTransport transport)
        {
            BinaryPrimitives.WriteUInt32BigEndian(_buffer, (uint)_written);
            _buffer[4] = 2; // DOFF
            _buffer[5] = 0; // AMQP frame type
            BinaryPrimitives.WriteUInt16BigEndian(_buffer.Slice(6, 2), 0);

            transport.SendFrame(_buffer.Slice(0, _written));
        }
    }
}
