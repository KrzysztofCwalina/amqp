using Alan.Amqp;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Azure.Amqp;
using System;

public class Amqp
{
    public static readonly byte[] _randomBytes1MB = new byte[1024 * 1024];
    public static readonly ByteBuffer _scratchBuffer2MB = new ByteBuffer(new byte[1024 * 1024 * 2], autoGrow: false);
    public static readonly byte[] _scratchArray2MB = new byte[1024 * 1024 * 2];

    public Amqp()
    {
        Random rng = new Random(0);     
        rng.NextBytes(_randomBytes1MB);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> Encode1MB_MA()
    {
        _scratchBuffer2MB.Reset();
        AmqpCodec.EncodeBinary(_randomBytes1MB, _scratchBuffer2MB);
        return _scratchBuffer2MB.Buffer.AsMemory(0, _scratchBuffer2MB.WritePos);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> Encode1MB_SP()
    {
        AmqpWriter.TryWrite(_scratchArray2MB, _randomBytes1MB, out int written);
        return _scratchArray2MB.AsMemory(0, written);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var test = new Amqp();
        var a1 = test.Encode1MB_MA();
        var a2 = test.Encode1MB_SP();
        var areEqual = a1.Span.SequenceEqual(a2.Span);
        if(!areEqual)
        {
            Console.WriteLine("Test did not pass!");
            return;
        }

        var summary = BenchmarkRunner.Run<Amqp>();
    }
}
