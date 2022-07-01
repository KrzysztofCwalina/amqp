using System.Buffers.Amqp;
using Microsoft.Azure.Amqp;
using Microsoft.Azure.Amqp.Encoding;
using Microsoft.Azure.Amqp.Framing;
using NUnit.Framework;
using System;
using System.Buffers.Binary;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Tests
{
    public class Tests
    {
        readonly static string s_text = "Hello Glorious Messaging World";
        readonly ReadOnlyMemory<byte> s_text_bytes = Encoding.UTF8.GetBytes(s_text);

        public readonly byte[] _randomBytes1_MB = new byte[1000 * 1000];
        byte[] _encodedBytes_1MB;

        public readonly int[] _randomInt32_1M = new int[1000 * 1000];
        byte[] _encodedInt32_1M;

        // buffers for 1M values
        public readonly ByteBuffer _scratchByteBuffer = new ByteBuffer(new byte[1024 * 1024 * 10], autoGrow: false);
        public readonly byte[] _scratchByteArray = new byte[1024 * 1024 * 10];

        public static readonly int[] ScratchInt32Array = new int[1024 * 1024 * 2];

        [SetUp]
        public void Setup()
        {
            Random rng = new Random(0);
            rng.NextBytes(_randomBytes1_MB);
            rng.NextBytes(MemoryMarshal.AsBytes(_randomInt32_1M.AsSpan()));

            _scratchByteBuffer.Reset();
            AmqpCodec.EncodeBinary(_randomBytes1_MB, _scratchByteBuffer);
            _encodedBytes_1MB = _scratchByteBuffer.Buffer.AsMemory(0, _scratchByteBuffer.WritePos).ToArray();

            _scratchByteBuffer.Reset();
            AmqpCodec.EncodeArray(_randomInt32_1M, _scratchByteBuffer);
            _encodedInt32_1M = _scratchByteBuffer.Buffer.AsMemory(0, _scratchByteBuffer.WritePos).ToArray();
        }

        [Test]
        public void ReadDescribedString8()
        {
            string value = "ABC";
            var descriptor = new DescribedType("GHI", "DEF");
            var describedValue = new DescribedType(descriptor, value);

            _scratchByteBuffer.Reset();
            AmqpCodec.EncodeObject(describedValue, _scratchByteBuffer);
            var encoded = _scratchByteBuffer.Buffer.AsMemory(0, _scratchByteBuffer.WritePos).ToArray();
            var encodedBuffer = new ByteBuffer(encoded, 0, encoded.Length);

            var decoded = (DescribedType)AmqpCodec.DecodeObject(encodedBuffer);
            Assert.AreEqual(value, decoded.Value);

            var decodedDescriptor = (DescribedType)decoded.Descriptor;
            Assert.AreEqual(descriptor.Value, decodedDescriptor.Value);
            Assert.AreEqual(descriptor.Descriptor, decodedDescriptor.Descriptor);

            var reader = new AmqpReader(encoded);
            Assert.AreEqual(AmqpToken.Descriptor, reader.MoveNext());

            // let's see what's the type of the descriptor. Oh, it's another descriptor
            Assert.AreEqual(AmqpToken.Descriptor, reader.MoveNext());

            Assert.AreEqual(AmqpToken.String, reader.MoveNext());
            var d1 = Encoding.UTF8.GetString(reader.Bytes);

            Assert.AreEqual(AmqpToken.String, reader.MoveNext());
            var d2 = Encoding.UTF8.GetString(reader.Bytes);

            Assert.AreEqual(AmqpToken.String, reader.MoveNext());
            var v1 = Encoding.UTF8.GetString(reader.Bytes);
        }

        [Test]
        public void WriteString8()
        {
            byte[] buffer = new byte[256];
            Assert.True(AmqpWriter.TryWriteString(buffer, s_text, out int written));
            Assert.AreEqual(((byte)AmqpType.String8), buffer[0]);
            Assert.AreEqual(s_text_bytes.Length, buffer[1]);
            Assert.True(s_text_bytes.Span.SequenceEqual(buffer.AsSpan(2, written - 2)));
        }

        [Test]
        public void ReadBinary()
        {
            var reader = new AmqpReader(_encodedBytes_1MB);
            Assert.True(reader.MoveNext() == AmqpToken.Binary);
            Assert.AreEqual(AmqpToken.Binary, reader.CurrentType);
            Assert.True(reader.Bytes.SequenceEqual(_randomBytes1_MB));
        }

        [Test]
        public void ReadArrayInt32()
        {
            var reader = new AmqpReader(_encodedInt32_1M);
            Assert.True(reader.MoveNext() == AmqpToken.Array);
            Assert.AreEqual(AmqpToken.Array, reader.CurrentType);
            int[] decoded = reader.GetInt32Array();
            Assert.True(_randomInt32_1M.AsSpan().SequenceEqual(decoded));
        }

        [Test]
        public void ReadSpanInt32()
        {
            var reader = new AmqpReader(_encodedInt32_1M);
            Assert.True(reader.MoveNext() == AmqpToken.Array);
            Assert.AreEqual(AmqpToken.Array, reader.CurrentType);

            if (!reader.TryGetInt32Array(ScratchInt32Array, out int written))
            {
                throw new InvalidOperationException("buffer too small");
            }

            Assert.True(ScratchInt32Array.AsSpan(0, written).SequenceEqual(_randomInt32_1M));
        }

        [Test]
        public void ComplexReader()
        {
            byte[] buffer = new byte[1024];
            AmqpWriter.TryWriteString(buffer, s_text, out int written);

            var reader = new AmqpReader(buffer.AsSpan().Slice(0, written));
            while (reader.MoveNext() != AmqpToken.EndOfData)
            {
                switch(reader.CurrentType)
                {
                    case AmqpToken.String:
                        Assert.AreEqual(s_text, Encoding.UTF8.GetString(reader.Bytes));
                        break;
                }
            }
        }

        [Test]
        public void ReadReference()
        {
            {
                var buffer = new ByteBuffer(_encodedBytes_1MB, 0, _encodedBytes_1MB.Length);
                buffer.Seek(0);
                var result = AmqpCodec.DecodeBinary(buffer);
                var decoded = result.AsMemory();
                decoded.Span.SequenceEqual(_randomBytes1_MB);
            }

            {
                var buffer = new ByteBuffer(_encodedInt32_1M, 0, _encodedInt32_1M.Length);
                buffer.Seek(0);
                var result = AmqpCodec.DecodeArray<Int32>(buffer);
                var decoded = result.AsMemory();
                decoded.Span.SequenceEqual(_randomInt32_1M);
            }
        }

        [Test]
        public void MessageReader()
        {
            var value = new AmqpValue()
            {
                Value = "ABC",
                Descriptor = "DEF"
            };

            var message = AmqpMessage.Create(value);
            //message.Header.Durable = true;

            var payload = message.GetPayload();
        }
    }
}