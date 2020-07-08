using Alan.Amqp;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Azure.Amqp;
using System;
using System.Runtime.InteropServices;

public class AmqpBench
{
    public static readonly byte[] RandomBytes_1MB = new byte[1024 * 1024];
    public static readonly int[] RandomInt32Array_1M = new int[1024 * 1024];

    // buffers for 1M values
    public static readonly ByteBuffer ScratchByteBuffer = new ByteBuffer(new byte[1024 * 1024 * 10], autoGrow: false);
    public static readonly byte[] ScratchArray = new byte[1024 * 1024 * 10];

    public static readonly int[] ScratchInt32Array = new int[1024 * 1024 * 2];

    public static readonly byte[] EncodedBytes_1MB;
    public static readonly ByteBuffer EncodedBytes_1MB_buffer;

    public static readonly byte[] EncodedInt32Array_1M;
    public static readonly ByteBuffer EncodedInt32Array_1M_buffer;

    static AmqpBench()
    {
        Random rng = new Random(0);
        rng.NextBytes(RandomBytes_1MB);
        rng.NextBytes(MemoryMarshal.AsBytes(RandomInt32Array_1M.AsSpan()));

        ScratchByteBuffer.Reset();
        AmqpCodec.EncodeBinary(RandomBytes_1MB, ScratchByteBuffer);
        EncodedBytes_1MB = ScratchByteBuffer.Buffer.AsMemory(0, ScratchByteBuffer.WritePos).ToArray();
        EncodedBytes_1MB_buffer = new ByteBuffer(EncodedBytes_1MB, 0, EncodedBytes_1MB.Length);

        ScratchByteBuffer.Reset();
        AmqpCodec.EncodeArray(RandomInt32Array_1M, ScratchByteBuffer);
        EncodedInt32Array_1M = ScratchByteBuffer.Buffer.AsMemory(0, ScratchByteBuffer.WritePos).ToArray();
        EncodedInt32Array_1M_buffer = new ByteBuffer(EncodedInt32Array_1M, 0, EncodedInt32Array_1M.Length);
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
        AmqpCodec.EncodeArray(RandomInt32Array_1M, ScratchByteBuffer);
        return ScratchByteBuffer.Buffer.AsMemory(0, ScratchByteBuffer.WritePos);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> Int32_Encode1M_SP()
    {
        AmqpWriter.TryWriteArray(ScratchArray, RandomInt32Array_1M, out int written);
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

    [Benchmark]
    public int Bytes_Decode1MB_SP_ToArray()
    {
        var reader = new AmqpReader(EncodedBytes_1MB);
        reader.Read();
        var bytes = reader.Bytes.ToArray();
        return bytes.Length;
    }

    [Benchmark]
    public int ArrayInt32_Decode1M_MA()
    {
        EncodedInt32Array_1M_buffer.Seek(0);
        var result = AmqpCodec.DecodeArray<int>(EncodedInt32Array_1M_buffer);
        return result.Length;
    }

    [Benchmark]
    public int ArrayInt32_Decode1M_SP()
    {
        var reader = new AmqpReader(EncodedInt32Array_1M);
        reader.Read();
        if(!reader.TryGetInt32Array(ScratchInt32Array, out int written))
        {
            throw new InvalidOperationException("buffer too small");
        }
        return written;
    }

    [Benchmark]
    public int ArrayInt32_Decode1M_SP_ToArray()
    {
        var reader = new AmqpReader(EncodedInt32Array_1M);
        reader.Read();
        int[] ints = reader.GetInt32Array();
        return ints.Length;
    }

    [Benchmark]
    public int ArrayInt32_Decode1M_SP_Iterator()
    {
        var reader = new AmqpReader(EncodedInt32Array_1M);
        reader.Read();
        int len = 0;
        while (reader.Type != AmqpType.ArrayEnd)
        {
            int value = reader.GetInt32();
            len++;
        }
        return len;
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
