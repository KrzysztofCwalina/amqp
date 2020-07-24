``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.959 (1903/May2019Update/19H1)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|                         Method |        Mean |     Error |    StdDev |     Gen 0 |   Gen 1 |   Gen 2 |  Allocated |
|------------------------------- |------------:|----------:|----------:|----------:|--------:|--------:|-----------:|
|         ArrayInt32_Decode1M_MA | 92,688.1 μs | 659.51 μs | 616.91 μs | 6000.0000 |       - |       - | 29360271 B |
|         ArrayInt32_Decode1M_AC |    896.1 μs |   6.47 μs |   6.06 μs |         - |       - |       - |        1 B |
| ArrayInt32_Decode1M_AC_ToArray |  2,191.1 μs |  29.06 μs |  27.19 μs |   39.0625 | 39.0625 | 39.0625 |  4194333 B |
