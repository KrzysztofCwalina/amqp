``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.959 (1903/May2019Update/19H1)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|                   Method |           Mean |         Error |        StdDev |   Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|------------------------- |---------------:|--------------:|--------------:|--------:|--------:|--------:|----------:|
|         Bytes_Decode_MAA | 440,118.792 ns |   995.3619 ns |   882.3624 ns | 10.7422 | 10.7422 | 10.7422 | 1048601 B |
|         Bytes_Decode_SBA |       4.841 ns |     0.0165 ns |     0.0155 ns |       - |       - |       - |         - |
| Bytes_Decode_SBA_ToArray | 462,939.020 ns | 4,602.3062 ns | 4,305.0001 ns | 11.7188 | 11.7188 | 11.7188 | 1048601 B |
