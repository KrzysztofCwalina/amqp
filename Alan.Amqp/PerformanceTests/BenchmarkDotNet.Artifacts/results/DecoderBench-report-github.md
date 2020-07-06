``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18362.900 (1903/May2019Update/19H1)
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.3.20216.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|                         Method |              Mean |           Error |          StdDev |     Gen 0 |   Gen 1 |   Gen 2 |  Allocated |
|------------------------------- |------------------:|----------------:|----------------:|----------:|--------:|--------:|-----------:|
|             Bytes_Decode1MB_MA |    339,882.596 ns |   1,199.1462 ns |   1,063.0118 ns |   12.6953 | 12.6953 | 12.6953 |  1048600 B |
|             Bytes_Decode1MB_SP |          4.057 ns |       0.0378 ns |       0.0354 ns |         - |       - |       - |          - |
|     Bytes_Decode1MB_SP_ToArray |    331,643.311 ns |   1,262.6331 ns |   1,181.0678 ns |   13.6719 | 13.6719 | 13.6719 |  1048600 B |
|         ArrayInt32_Decode1M_MA | 87,962,851.111 ns | 524,770.1456 ns | 490,870.3198 ns | 6000.0000 |       - |       - | 29360399 B |
|         ArrayInt32_Decode1M_SP |    901,041.782 ns |   3,771.8368 ns |   3,149.6546 ns |         - |       - |       - |        1 B |
| ArrayInt32_Decode1M_SP_ToArray |  1,727,568.307 ns |  11,738.0149 ns |  10,979.7464 ns |   44.9219 | 44.9219 | 44.9219 |  4194330 B |
