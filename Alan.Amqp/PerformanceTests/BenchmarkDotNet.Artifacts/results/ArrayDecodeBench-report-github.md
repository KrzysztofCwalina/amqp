``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.959 (1903/May2019Update/19H1)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|                          Method |        Mean |     Error |    StdDev |     Gen 0 |   Gen 1 |   Gen 2 |  Allocated |
|-------------------------------- |------------:|----------:|----------:|----------:|--------:|--------:|-----------:|
|         ArrayInt32Decode_1M_MAA | 90,001.8 μs | 393.87 μs | 328.90 μs | 6000.0000 |       - |       - | 29360380 B |
|         ArrayInt32Decode_1M_SBA |    890.5 μs |   2.85 μs |   2.38 μs |         - |       - |       - |        2 B |
| ArrayInt32Decode_1M_SBA_ToArray |  2,194.8 μs |   5.32 μs |   4.71 μs |   39.0625 | 39.0625 | 39.0625 |  4194328 B |
|         ArrayInt32Decode_1K_MAA | 86,279.3 μs | 549.68 μs | 487.28 μs | 6000.0000 |       - |       - | 29360399 B |
|         ArrayInt32Decode_1K_SBA |    890.3 μs |   2.15 μs |   1.80 μs |         - |       - |       - |        1 B |
| ArrayInt32Decode_1K_SBA_ToArray |  2,192.5 μs |   9.25 μs |   8.65 μs |   39.0625 | 39.0625 | 39.0625 |  4194333 B |
