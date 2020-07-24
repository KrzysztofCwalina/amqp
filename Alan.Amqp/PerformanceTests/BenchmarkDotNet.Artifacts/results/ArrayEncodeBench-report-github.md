``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.959 (1903/May2019Update/19H1)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|                  Method |             Mean |           Error |          StdDev |      Gen 0 | Gen 1 | Gen 2 |  Allocated |
|------------------------ |-----------------:|----------------:|----------------:|-----------:|------:|------:|-----------:|
| ArrayInt32Encode_MAA_1M | 113,282,667.7 ns | 1,322,618.99 ns | 1,104,446.76 ns | 12000.0000 |     - |     - | 50332008 B |
| ArrayInt32Encode_SBA_1M |     895,843.6 ns |     4,737.69 ns |     4,199.84 ns |          - |     - |     - |        1 B |
| ArrayInt32Encode_MAA_1K |     107,027.2 ns |       553.63 ns |       517.87 ns |    11.7188 |     - |     - |    49265 B |
| ArrayInt32Encode_SBA_1K |         886.1 ns |         3.66 ns |         3.42 ns |          - |     - |     - |          - |
