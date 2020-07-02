using Alan.Amqp;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Azure.Amqp;
using System;
using System.Runtime.InteropServices;

public class Amqp
{
    public static readonly byte[] _randomBytes1MB = new byte[1024 * 1024];
    public static readonly int[] _randomInt32_1M = new int[1024 * 1024];

    // buffers for 1M values
    public static readonly ByteBuffer _scratchByteBuffer = new ByteBuffer(new byte[1024 * 1024 * 10], autoGrow: false);
    public static readonly byte[] _scratchArray = new byte[1024 * 1024 * 10];

    public readonly byte[] _encodedBytesArray;
    public readonly ByteBuffer _encodedBytesBuffer;

    public Amqp()
    {
        Random rng = new Random(0);     
        rng.NextBytes(_randomBytes1MB);
        rng.NextBytes(MemoryMarshal.AsBytes(_randomInt32_1M.AsSpan()));

        _encodedBytesArray = Bytes_Encode1MB_MA().ToArray();
        _encodedBytesBuffer = new ByteBuffer(_encodedBytesArray);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> Bytes_Encode1MB_MA()
    {
        _scratchByteBuffer.Reset();
        AmqpCodec.EncodeBinary(_randomBytes1MB, _scratchByteBuffer);
        return _scratchByteBuffer.Buffer.AsMemory(0, _scratchByteBuffer.WritePos);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> Bytes_Encode1MB_SP()
    {
        AmqpWriter.TryWriteBinary(_scratchArray, _randomBytes1MB, out int written);
        return _scratchArray.AsMemory(0, written);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> Int32_Encode1M_MA()
    {
        _scratchByteBuffer.Reset();
        AmqpCodec.EncodeArray(_randomInt32_1M, _scratchByteBuffer);
        return _scratchByteBuffer.Buffer.AsMemory(0, _scratchByteBuffer.WritePos);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> Int32_Encode1M_SP()
    {
        AmqpWriter.TryWriteArray(_scratchArray, _randomInt32_1M, out int written);
        return _scratchArray.AsMemory(0, written);
    }

    //[Benchmark]
    //public ReadOnlyMemory<byte> Bytes_Decode1MB_MA()
    //{
    //    _encodedBytesBuffer.Reset();
    //    var result = AmqpCodec.DecodeBinary(_encodedBytesBuffer);
    //    return result.AsMemory();
    //}

    //[Benchmark]
    //public ReadOnlyMemory<byte> Bytes_Decode1MB_SP()
    //{
    //    var reader = new AmqpReader(_encodedBytesArray);
    //    while (reader.Read())
    //    {
    //        switch (reader.Type)
    //        {
    //            default: throw new NotImplementedException();
    //        }
    //    }
    //    throw new NotImplementedException();
    //}
}

public class Program
{
    public static void Main()
    {
        var test = new Amqp();

        var encodedIntArrayMA = test.Int32_Encode1M_MA();
        var encodedIntArraySP = test.Int32_Encode1M_SP();
        var areEqual = encodedIntArrayMA.Span.SequenceEqual(encodedIntArraySP.Span);
        if (!areEqual)
        {
            Console.WriteLine("Int32 array encoding test did not pass!");
            return;
        }

        var encodedBinaryMA = test.Bytes_Encode1MB_MA();
        var encodedBinarySP = test.Bytes_Encode1MB_SP();
        if (!encodedBinaryMA.Span.SequenceEqual(encodedBinarySP.Span))
        {
            Console.WriteLine("Binary encoding test did not pass!");
            return;
        }

        var summary = BenchmarkRunner.Run<Amqp>();
    }
}
