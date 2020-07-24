``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.959 (1903/May2019Update/19H1)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|             Method |           Mean |         Error |        StdDev |   Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|------------------- |---------------:|--------------:|--------------:|--------:|--------:|--------:|----------:|
| Bytes_Encode1MB_MA |  29,876.878 ns |   329.3771 ns |   308.0996 ns |       - |       - |       - |         - |
| Bytes_Encode1MB_AC |  29,467.193 ns |    77.2353 ns |    60.3003 ns |       - |       - |       - |         - |
| Bytes_Decode1MB_MA | 447,075.068 ns | 1,175.0590 ns | 1,099.1509 ns | 11.7188 | 11.7188 | 11.7188 | 1048600 B |
| Bytes_Decode1MB_AC |       4.884 ns |     0.0489 ns |     0.0458 ns |       - |       - |       - |         - |
