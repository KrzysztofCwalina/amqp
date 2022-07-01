using System;
using System.Buffers.Amqp;
using System.Text;
using NUnit.Framework;

namespace Tests
{
    public class TransportTests
    {
        readonly static string endpoint = null;

        [Test]
        public void Handshake()
        {
            var transport = new AmqpTransport(endpoint);
            var clientProtocol = new AmqpProtocol(AmqpSecurity.Tls);

            Assert.True(transport.TryHandshake(clientProtocol, out var serverProtocol));
            Assert.True(clientProtocol.Equals(serverProtocol));

            transport.Close();
        }

        [Test]
        public void Open()
        {
            var transport = new AmqpTransport(endpoint);
            var clientProtocol = new AmqpProtocol(AmqpSecurity.Tls);

            Assert.True(transport.TryHandshake(clientProtocol, out var serverProtocol));

            var buffer = new byte[256];

            var frame = new OpenFrame(buffer);

            var containerId = Encoding.ASCII.GetBytes(Guid.NewGuid().ToString());
            frame.TryWrite(containerId, hostname: ""u8);
            frame.Send(transport);

            buffer.AsSpan().Clear();

            transport.Receive(buffer, out int read);

            transport.Close();
        }
    }
}