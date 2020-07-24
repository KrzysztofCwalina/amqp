using System.Buffers.Amqp;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Azure.Amqp;
using System;
using System.Runtime.InteropServices;

public class AmqpBench
{
    public static readonly byte[] RandomBytes1MB = new byte[1024 * 1024];
    public static readonly int[] RandomInt32Array1M = new int[1024 * 1024];
    public static readonly int[] RandomInt32Array1K = new int[1024];

    // buffers for 1M values
    public static readonly ByteBuffer ScratchByteBuffer = new ByteBuffer(new byte[1024 * 1024 * 10], autoGrow: false);
    public static readonly byte[] ScratchArray = new byte[1024 * 1024 * 10];

    public static readonly int[] ScratchInt32Array = new int[1024 * 1024 * 2];

    public static readonly byte[] EncodedBytes1MB;
    public static readonly ByteBuffer EncodedBytes1MBBuffer;

    public static readonly byte[] EncodedInt32Array1M;
    public static readonly byte[] EncodedInt32Array1K;
    public static readonly ByteBuffer EncodedInt32Array1MBuffer;
    public static readonly ByteBuffer EncodedInt32Array1KBuffer;

    static AmqpBench()
    {
        Random rng = new Random(0);
        rng.NextBytes(RandomBytes1MB);
        rng.NextBytes(MemoryMarshal.AsBytes(RandomInt32Array1M.AsSpan()));
        rng.NextBytes(MemoryMarshal.AsBytes(RandomInt32Array1K.AsSpan()));

        ScratchByteBuffer.Reset(); 
        AmqpCodec.EncodeBinary(RandomBytes1MB, ScratchByteBuffer);
        EncodedBytes1MB = ScratchByteBuffer.Buffer.AsMemory(0, ScratchByteBuffer.WritePos).ToArray();
        EncodedBytes1MBBuffer = new ByteBuffer(EncodedBytes1MB, 0, EncodedBytes1MB.Length);

        ScratchByteBuffer.Reset();
        AmqpCodec.EncodeArray(RandomInt32Array1M, ScratchByteBuffer);
        EncodedInt32Array1M = ScratchByteBuffer.Buffer.AsMemory(0, ScratchByteBuffer.WritePos).ToArray();
        EncodedInt32Array1MBuffer = new ByteBuffer(EncodedInt32Array1M, 0, EncodedInt32Array1M.Length);

        EncodedInt32Array1K = ScratchByteBuffer.Buffer.AsMemory(0, ScratchByteBuffer.WritePos).ToArray();
        EncodedInt32Array1KBuffer = new ByteBuffer(EncodedInt32Array1M, 0, EncodedInt32Array1M.Length);
    }
}

[MemoryDiagnoser]
public class BinaryEncodeBench : AmqpBench
{
    [Benchmark]
    public ReadOnlyMemory<byte> Bytes_Encode_MAA()
    {
        ScratchByteBuffer.Reset();
        AmqpCodec.EncodeBinary(RandomBytes1MB, ScratchByteBuffer);
        return ScratchByteBuffer.Buffer.AsMemory(0, ScratchByteBuffer.WritePos);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> Bytes_Encode_SBA()
    {
        AmqpWriter.TryWriteBinary(ScratchArray, RandomBytes1MB, out int written);
        return ScratchArray.AsMemory(0, written);
    }
}

[MemoryDiagnoser]
public class BinaryDecodeBench : AmqpBench
{
    [Benchmark]
    public ArraySegment<byte> Bytes_Decode_MAA()
    {
        EncodedBytes1MBBuffer.Seek(0);
        var result = AmqpCodec.DecodeBinary(EncodedBytes1MBBuffer);
        return result;
    }

    [Benchmark]
    public ReadOnlySpan<byte> Bytes_Decode_SBA()
    {
        var reader = new AmqpReader(EncodedBytes1MB);
        reader.MoveNext();
        var bytes = reader.Bytes;
        return bytes;
    }

    [Benchmark]
    public int Bytes_Decode_SBA_ToArray()
    {
        var reader = new AmqpReader(EncodedBytes1MB);
        reader.MoveNext();
        var bytes = reader.Bytes.ToArray();
        return bytes.Length;
    }
}

[MemoryDiagnoser]
public class ArrayEncodeBench : AmqpBench
{
    [Benchmark]
    public ReadOnlyMemory<byte> ArrayInt32Encode_MAA_1M()
    {
        ScratchByteBuffer.Reset();
        AmqpCodec.EncodeArray(RandomInt32Array1M, ScratchByteBuffer);
        return ScratchByteBuffer.Buffer.AsMemory(0, ScratchByteBuffer.WritePos);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> ArrayInt32Encode_SBA_1M()
    {
        AmqpWriter.TryWriteArray(ScratchArray, RandomInt32Array1M, out int written);
        return ScratchArray.AsMemory(0, written);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> ArrayInt32Encode_MAA_1K()
    {
        ScratchByteBuffer.Reset();
        AmqpCodec.EncodeArray(RandomInt32Array1K, ScratchByteBuffer);
        return ScratchByteBuffer.Buffer.AsMemory(0, ScratchByteBuffer.WritePos);
    }

    [Benchmark]
    public ReadOnlyMemory<byte> ArrayInt32Encode_SBA_1K()
    {
        AmqpWriter.TryWriteArray(ScratchArray, RandomInt32Array1K, out int written);
        return ScratchArray.AsMemory(0, written);
    }
}

[MemoryDiagnoser]
public class ArrayDecodeBench : AmqpBench
{
    [Benchmark]
    public int ArrayInt32Decode_1M_MAA()
    {
        EncodedInt32Array1MBuffer.Seek(0);
        var result = AmqpCodec.DecodeArray<int>(EncodedInt32Array1MBuffer);
        return result.Length;
    }

    [Benchmark]
    public int ArrayInt32Decode_1M_SBA()
    {
        var reader = new AmqpReader(EncodedInt32Array1M);
        reader.MoveNext();
        if(!reader.TryGetInt32Array(ScratchInt32Array, out int written))
        {
            throw new InvalidOperationException("buffer too small");
        }
        return written;
    }

    [Benchmark]
    public int ArrayInt32Decode_1M_SBA_ToArray()
    {
        var reader = new AmqpReader(EncodedInt32Array1M);
        reader.MoveNext();
        int[] ints = reader.GetInt32Array();
        return ints.Length;
    }

    [Benchmark]
    public int ArrayInt32Decode_1K_MAA()
    {
        EncodedInt32Array1KBuffer.Seek(0);
        var result = AmqpCodec.DecodeArray<int>(EncodedInt32Array1KBuffer);
        return result.Length;
    }

    [Benchmark]
    public int ArrayInt32Decode_1K_SBA()
    {
        var reader = new AmqpReader(EncodedInt32Array1K);
        reader.MoveNext();
        if (!reader.TryGetInt32Array(ScratchInt32Array, out int written))
        {
            throw new InvalidOperationException("buffer too small");
        }
        return written;
    }

    [Benchmark]
    public int ArrayInt32Decode_1K_SBA_ToArray()
    {
        var reader = new AmqpReader(EncodedInt32Array1K);
        reader.MoveNext();
        int[] ints = reader.GetInt32Array();
        return ints.Length;
    }

    //[Benchmark]
    //public int ArrayInt32_Decode_1M_AC_Iterator()
    //{
    //    var reader = new AmqpReader(EncodedInt32Array_1M);
    //    reader.MoveNext();
    //    int len = 0;
    //    while (reader.CurrentType != AmqpToken.ArrayEnd)
    //    {
    //        int value = reader.ReadInt32();
    //        len++;
    //    }
    //    return len;
    //}
}

public class Program
{
    public static void Main(string[] args)
    {
        var binaryEncoder = new BinaryEncodeBench();
        var binaryDecoder = new BinaryDecodeBench();
        var arrayEncoder = new ArrayEncodeBench();

        var encodedIntArrayMAA = arrayEncoder.ArrayInt32Encode_MAA_1M();
        var encodedIntArraySBA = arrayEncoder.ArrayInt32Encode_SBA_1M();
        var areEqual = encodedIntArrayMAA.Span.SequenceEqual(encodedIntArraySBA.Span);
        if (!areEqual)
        {
            Console.WriteLine("Int32 array encoding test did not pass!");
            return;
        }

        var encodedBinaryMAA = binaryEncoder.Bytes_Encode_MAA();
        var encodedBinarySBA = binaryEncoder.Bytes_Encode_SBA();
        if (!encodedBinaryMAA.Span.SequenceEqual(encodedBinarySBA.Span))
        {
            Console.WriteLine("Binary encoding test did not pass!");
            return;
        }

        var decodedBinaryMAA = binaryDecoder.Bytes_Decode_SBA();
        if (!decodedBinaryMAA.SequenceEqual(AmqpBench.RandomBytes1MB))
        {
            Console.WriteLine("Binary encoding test did not pass!");
            return;
        }

        BenchmarkSwitcher
            .FromTypes(new Type[] { 
                typeof(BinaryEncodeBench), 
                typeof(BinaryDecodeBench), 
                typeof(ArrayDecodeBench), 
                typeof(ArrayEncodeBench) 
            })
            .Run(args);
    }
}


