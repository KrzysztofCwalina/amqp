``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.900 (1903/May2019Update/19H1)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|             Method |          Mean |      Error |     StdDev |
|------------------- |--------------:|-----------:|-----------:|
| Bytes_Encode1MB_MA |      29.88 μs |   0.363 μs |   0.339 μs |
| Bytes_Encode1MB_SP |      29.83 μs |   0.418 μs |   0.391 μs |
|  Int32_Encode1M_MA | 107,245.85 μs | 677.583 μs | 633.811 μs |
|  Int32_Encode1M_SP |     900.39 μs |  17.168 μs |  19.082 μs |
