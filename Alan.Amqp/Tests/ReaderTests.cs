using Alan.Amqp;
using Microsoft.Azure.Amqp;
using NUnit.Framework;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace Tests
{
    public class Tests
    {
        readonly static string s_text = "Hello Glorious Messaging World";
        readonly ReadOnlyMemory<byte> s_text_bytes = Encoding.UTF8.GetBytes(s_text);

        public readonly byte[] _randomBytes1_MB = new byte[1024 * 1024];
        byte[] _encodedBytes_1MB;

        public readonly int[] _randomInt32_1M = new int[1024 * 1024];
        byte[] _encodedInt32_1M;

        // buffers for 1M values
        public readonly ByteBuffer _scratchByteBuffer = new ByteBuffer(new byte[1024 * 1024 * 10], autoGrow: false);
        public readonly byte[] _scratchArray = new byte[1024 * 1024 * 10];


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
        public void WriteString8()
        {
            byte[] buffer = new byte[256];
            Assert.True(AmqpWriter.TryWrite(buffer, s_text, out int written));
            Assert.AreEqual(((byte)AmqpConstructor.String8), buffer[0]);
            Assert.AreEqual(s_text_bytes.Length, buffer[1]);
            Assert.True(s_text_bytes.Span.SequenceEqual(buffer.AsSpan(2, written - 2)));
        }

        [Test]
        public void ReadBinary()
        {
            var reader = new AmqpReader(_encodedBytes_1MB);
            Assert.True(reader.Read());
            Assert.AreEqual(AmqpType.Binary, reader.Type);
            Assert.True(reader.Bytes.SequenceEqual(_randomBytes1_MB));
        }

        [Test]
        public void ReadArrayInt32()
        {
            var reader = new AmqpReader(_encodedInt32_1M);
            Assert.True(reader.Read());
            Assert.AreEqual(AmqpType.ArrayStart, reader.Type);
        }

        [Test]
        public void ComplexReader()
        {
            byte[] buffer = new byte[1024];
            AmqpWriter.TryWrite(buffer, s_text, out int written);

            var reader = new AmqpReader(buffer.AsSpan().Slice(0, written));
            while (reader.Read())
            {
                switch(reader.Type)
                {
                    case AmqpType.Utf8:
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
    }
}