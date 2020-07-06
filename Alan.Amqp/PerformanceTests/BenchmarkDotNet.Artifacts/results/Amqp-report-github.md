``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.900 (1903/May2019Update/19H1)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|             Method |          Mean |        Error |       StdDev |      Gen 0 |   Gen 1 |   Gen 2 |  Allocated |
|------------------- |--------------:|-------------:|-------------:|-----------:|--------:|--------:|-----------:|
| Bytes_Encode1MB_MA |      30.72 μs |     0.553 μs |     0.517 μs |          - |       - |       - |          - |
| Bytes_Encode1MB_SP |      30.03 μs |     0.358 μs |     0.335 μs |          - |       - |       - |          - |
|  Int32_Encode1M_MA | 110,935.94 μs | 1,320.161 μs | 1,030.695 μs | 12000.0000 |       - |       - | 50331874 B |
|  Int32_Encode1M_SP |     917.08 μs |    14.194 μs |    13.277 μs |          - |       - |       - |          - |
| Bytes_Decode1MB_MA |     239.52 μs |     4.732 μs |    11.608 μs |    20.0195 | 20.0195 | 20.0195 |  1048600 B |
