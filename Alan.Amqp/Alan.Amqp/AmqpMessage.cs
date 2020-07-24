using System;

namespace System.Buffers.Amqp
{
    public enum AmqpMessageSection
    {
        Header,
        DeliveryAnnotations,
        MessageAnnotations,
        MessageProperties,
        ApplicationProperties,
        ApplicationData,
        Footer,
        EOM,
    }

    public ref struct AmqpMessageReader
    {
        AmqpReader _reader;
        ReadOnlySpan<byte> _bytes;

        public AmqpMessageReader(ReadOnlySpan<byte> messageBytes)
        {
            _bytes = messageBytes;
            _reader = new AmqpReader(messageBytes);
        }

        public AmqpMessageSection MoveNext()
        {
            return AmqpMessageSection.EOM;
        }

        public AmqpHeader ReadHeader()
        {
            return new AmqpHeader(_bytes);
        }
    }

    public ref struct AmqpHeader
    {
        ReadOnlySpan<byte> _bytes;

        public AmqpHeader(ReadOnlySpan<byte> bytes)
        {
            _bytes = bytes;
        }
    }
}
