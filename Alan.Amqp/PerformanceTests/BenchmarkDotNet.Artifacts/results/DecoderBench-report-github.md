``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.900 (1903/May2019Update/19H1)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|             Method |           Mean |         Error |        StdDev |   Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|------------------- |---------------:|--------------:|--------------:|--------:|--------:|--------:|----------:|
| Bytes_Decode1MB_MA | 334,466.985 ns | 4,330.0777 ns | 3,615.8110 ns | 17.5781 | 17.5781 | 17.5781 | 1048604 B |
| Bytes_Decode1MB_SP |       4.331 ns |     0.0479 ns |     0.0448 ns |       - |       - |       - |         - |
