using Alan.Amqp;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Azure.Amqp;
using System;
using System.Runtime.InteropServices;

public class AmqpBench
{
    public static readonly byte[] RandomBytes_1MB = new byte[1024 * 1024];
    public static readonly int[] RandomInt32_1MB = new int[1024 * 1024];

    // buffers for 1M values
    public static readonly ByteBuffer ScratchByteBuffer = new ByteBuffer(new byte[1024 * 1024 * 10], autoGrow: false);
    public static readonly byte[] ScratchArray = new byte[1024 * 1024 * 10];

    public static readonly byte[] EncodedBytes_1MB;
    public static readonly ByteBuffer EncodedBytes_1MB_buffer;

    static AmqpBench()
    {
        Random rng = new Random(0);
        rng.NextBytes(RandomBytes_1MB);
        rng.NextBytes(MemoryMarshal.AsBytes(RandomInt32_1MB.AsSpan()));

        ScratchByteBuffer.Reset();
        AmqpCodec.EncodeBinary(RandomBytes_1MB, ScratchByteBuffer);
        EncodedBytes_1MB = ScratchByteBuffer.Buffer.AsMemory(0, ScratchByteBuffer.WritePos).ToArray();
        EncodedBytes_1MB_buffer = new ByteBuffer(EncodedBytes_1MB, 0, EncodedBytes_1MB.Length);
    }
}

[MemoryDiagnoser]
public class EncoderBench : AmqpBench
{
    [Benchmark]
    public ReadOnlyMemory<byte> Bytes_Encode1MB_MA()
    {
        ScratchByteBuffer.Reset();
        AmqpCodec.EncodeBinary(RandomBytes_1MB, ScratchByteBuffer);
        return ScratchByteBuffer.Buffer.AsMemory(0, ScratchByteBuffer.WritePos);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> Bytes_Encode1MB_SP()
    {
        AmqpWriter.TryWriteBinary(ScratchArray, RandomBytes_1MB, out int written);
        return ScratchArray.AsMemory(0, written);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> Int32_Encode1M_MA()
    {
        ScratchByteBuffer.Reset();
        AmqpCodec.EncodeArray(RandomInt32_1MB, ScratchByteBuffer);
        return ScratchByteBuffer.Buffer.AsMemory(0, ScratchByteBuffer.WritePos);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> Int32_Encode1M_SP()
    {
        AmqpWriter.TryWriteArray(ScratchArray, RandomInt32_1MB, out int written);
        return ScratchArray.AsMemory(0, written);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> Bytes_Decode1MB_MA()
    {
        EncodedBytes_1MB_buffer.Seek(0);
        var result = AmqpCodec.DecodeBinary(EncodedBytes_1MB_buffer);
        return result.AsMemory();
    }
}

[MemoryDiagnoser]
public class DecoderBench : AmqpBench
{
    [Benchmark]
    public int Bytes_Decode1MB_MA()
    {
        EncodedBytes_1MB_buffer.Seek(0);
        var result = AmqpCodec.DecodeBinary(EncodedBytes_1MB_buffer);
        return result.Count;
    }

    [Benchmark]
    public int Bytes_Decode1MB_SP()
    {
        var reader = new AmqpReader(EncodedBytes_1MB);
        reader.Read();
        var bytes = reader.Bytes;
        return bytes.Length;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var test = new EncoderBench();

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

        var decodedBinaryMA = test.Bytes_Decode1MB_MA();
        if (!decodedBinaryMA.Span.SequenceEqual(AmqpBench.RandomBytes_1MB.AsSpan()))
        {
            Console.WriteLine("Binary encoding test did not pass!");
            return;
        }

        BenchmarkSwitcher
            .FromTypes(new Type[] { typeof(DecoderBench), typeof(EncoderBench) })
            .Run(args);
    }
}
