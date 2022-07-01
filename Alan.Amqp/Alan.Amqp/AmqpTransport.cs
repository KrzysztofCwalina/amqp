using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace System.Buffers.Amqp
{
    public class AmqpTransport
    {
        const int AmqpPort = 5671;
        readonly static byte[] s_Amqp = new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P' };

        string _endpoint;
        TcpClient _client;
        Stream _stream;
        Stream _inner;

        public AmqpTransport(string endpoint)
        {
            _endpoint = endpoint;
        }

        public bool TryHandshake(AmqpProtocol client, out AmqpProtocol server)
        {
            if (_client == null)
            {
                _client = new TcpClient();
                _client.Connect(_endpoint, AmqpPort);
                _stream = _client.GetStream();
                _inner = _stream;
                UpgradeToTls();
            }

            Span<byte> amqp = stackalloc byte[16]; // slack is needed for read (below)
            amqp[0] = (byte)'A';
            amqp[1] = (byte)'M';
            amqp[2] = (byte)'Q';
            amqp[3] = (byte)'P'; 
            amqp[4] = (byte)client.Security;
            amqp[5] = client.Major;
            amqp[6] = client.Minor;
            amqp[7] = client.Revision;
            _stream.Write(amqp.Slice(0, 8));
            _stream.Flush();

            amqp.Clear();
            int read = _stream.Read(amqp);
            var response = amqp.Slice(0, read);
            return TryParseHandshake(response, out server);
        }

        public void UpgradeToTls()
        {
            var ssl = new SslStream(_stream, false, new RemoteCertificateValidationCallback(
                (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => {
                    throw new NotImplementedException(); 
                }
                ));
            ssl.AuthenticateAsClient(_endpoint);
            _stream = ssl;
        }
        bool TryParseHandshake(ReadOnlySpan<byte> response, out AmqpProtocol protocol)
        {
            if (response.Length == 8 && response.StartsWith(s_Amqp)) {
                protocol = new AmqpProtocol();
                protocol.Security = (AmqpSecurity)response[4];
                protocol.Major = response[5];
                protocol.Minor = response[6];
                protocol.Revision = response[7];
                return true;
            }
            else
            {
                protocol = default;
                return false;
            }
        }
        public void SendFrame(ReadOnlySpan<byte> frame, bool flush = true)
        {
            _stream.Write(frame);
            if (flush) _stream.Flush();
        }
        public void Receive(Span<byte> buffer, out int read)
        {
            read = _stream.Read(buffer);
        }

        public void Close()
        {
            _client.Close();
        }
    }

    public enum AmqpSecurity : byte
    {
        Tls = 2,
    }

    public struct AmqpProtocol : IEquatable<AmqpProtocol>
    {
        public AmqpProtocol(AmqpSecurity security, byte major = 1, byte minor = 0, byte revision = 0)
        {
            Security = security;
            Major = major;
            Minor = minor;
            Revision = revision;
        }
        public AmqpSecurity Security { get; set; }
        public byte Major { get; set; }
        public byte Minor { get; set; }
        public byte Revision { get; set; }

        public bool Equals(AmqpProtocol other)
        {
            if (Security != other.Security) return false;
            if (Major != other.Major) return false;
            if (Minor != other.Minor) return false;
            if (Revision != other.Revision) return false;
            return true;
        }
    }
}
