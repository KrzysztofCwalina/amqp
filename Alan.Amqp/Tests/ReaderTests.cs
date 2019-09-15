using Alan.Amqp;
using NUnit.Framework;
using System;
using System.Buffers.Binary;
using System.Text;

namespace Tests
{
    public class Tests
    {
        readonly static string s_text = "Hello Glorious Messaging World";
        readonly static ReadOnlyMemory<byte> s_text_bytes = Encoding.UTF8.GetBytes(s_text);

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
        public void Reader()
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
    }
}